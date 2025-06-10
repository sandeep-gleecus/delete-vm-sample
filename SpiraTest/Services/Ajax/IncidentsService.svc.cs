using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Activation;
using System.Linq;
using System.Web.Security;
using Microsoft.Security.Application;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>Communicates with the SortableGrid AJAX component for displaying/updating incidents data</summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class IncidentsService : SortedListServiceBase, IIncidentsService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService::";

		protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_INCIDENT_PAGINATION_SIZE;

		#region IList interface methods

        /// <summary>
        /// Changes the width of a column in a grid. Needs to be overidden by the subclass
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fieldName">The name of the column being moved</param>
        /// <param name="width">The new width of the column (in pixels)</param>
        public override void List_ChangeColumnWidth(int projectId, string fieldName, int width)
        {
            const string METHOD_NAME = "List_ChangeColumnWidth";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Change the width of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, fieldName, width);
            }
            catch (InvalidOperationException)
            {
                //The field cannot be found, so fail quietly
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

		/// <summary>
		/// Changes the order of columns in the incident list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="newIndex">The new index of the column's position</param>
		public override void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex)
		{
			const string METHOD_NAME = "List_ChangeColumnPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//The field position may be different to the index because index is zero-based
				int newPosition = newIndex + 1;

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Toggle the status of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, fieldName, newPosition);
			}
			catch (InvalidOperationException)
			{
				//The field cannot be found, so fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

        #region Internal Functions

        /// <summary>Handles service-specific functionality that can be performed on a selected number of items in the sorted grid</summary>
		/// <param name="operation">The name of the operation</param>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destId">The destination id (if appropriate)</param>
		/// <param name="items">The items to peform the operation on</param>
		/// <returns></returns>
		public override string CustomListOperation(string operation, int projectId, int destId, List<string> items)
		{
			const string METHOD_NAME = "CustomListOperation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			try
			{
				//See which operation we have
				if (operation == "CreateRequirement")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
					if (authorizationState == Project.AuthorizationState.Prohibited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in requirements and create new test cases from them
					//We don't actually use the destId
					RequirementManager requirement = new RequirementManager();
					foreach (string item in items)
					{
						int incidentId = Int32.Parse(item);
						requirement.CreateFromIncident(incidentId, userId);
					}
				}
				else
				{
					throw new NotImplementedException("Operation '" + operation + "' is not currently supported");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return "";  //Success
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        #endregion

        #region Graphing Functions

        /// <summary>Retrieves the incident aging count for the project group</summary>
        /// <param name="projectGroupId">The project group we're interested in</param>
        /// <param name="ageInterval">The age interval</param>
        /// <param name="maximumAge">The maxium age that is displayed on the graph</param>
        /// <returns>The requested aging data</returns>
        public List<GraphEntry> Incident_RetrieveGroupAging(int projectGroupId, int maximumAge, int ageInterval)
        {
            const string METHOD_NAME = "Incident_RetrieveGroupAging";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the aging count and convert to JSON object
                IncidentManager incidentManager = new IncidentManager();
                DataSet incidentAgingDataSet = incidentManager.RetrieveAging(projectGroupId, maximumAge, ageInterval);
                List<GraphEntry> graphEntries = new List<GraphEntry>();
                foreach (DataRow dataRow in incidentAgingDataSet.Tables["IncidentAging"].Rows)
                {
                    GraphEntry graphEntry = new GraphEntry();
                    graphEntry.Name = (string)dataRow["Age"];
                    graphEntry.Caption = (string)dataRow["Age"];
                    graphEntry.Count = (int)dataRow["Count"];
                    graphEntries.Add(graphEntry);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return graphEntries;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Counts the number of incidents
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="artifact">The artifact we want the incidents for</param>
        /// <returns>The count</returns>
        public int Incident_Count(int projectId, ArtifactReference artifact)
        {
            const string METHOD_NAME = "Incident_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view test runs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Depending on the artifact that these incidents are for (test case, test run or test set)
                //we need to set the grid properties accordingly and also indicate if we have any data
                int incidentCount = 0;
                IncidentManager incidentManager = new IncidentManager();
                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestRun)
                {
                    incidentCount = incidentManager.CountByTestRunId(artifact.ArtifactId);
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Build)
                {
                    Hashtable incidentFilters = new Hashtable();
                    incidentFilters.Add("BuildId", artifact.ArtifactId);
                    incidentCount = incidentManager.Count(projectId, incidentFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestSet)
                {
                    incidentCount = incidentManager.CountByTestSetId(artifact.ArtifactId);
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase)
                {
                    incidentCount = incidentManager.RetrieveByTestCaseId(artifact.ArtifactId, false).Count;
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestStep)
                {
                    incidentCount = incidentManager.RetrieveByTestStepId(artifact.ArtifactId).Count;
                }

                if (artifact.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Release)
                {
                    //We need to see if any of the release incident fields have data (detected, resolved, verified)
                    incidentCount = 0;
                    Hashtable releaseFilter = new Hashtable();
                    releaseFilter.Add("ResolvedReleaseId", artifact.ArtifactId);
                    incidentCount += incidentManager.Count(projectId, releaseFilter, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                    releaseFilter.Clear();
                    releaseFilter.Add("VerifiedReleaseId", artifact.ArtifactId);
                    incidentCount += incidentManager.Count(projectId, releaseFilter, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                    releaseFilter.Clear();
                    releaseFilter.Add("DetectedReleaseId", artifact.ArtifactId);
                    incidentCount += incidentManager.Count(projectId, releaseFilter, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                }

                return incidentCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the incident aging count for the project/release</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we're interested in (optional)</param>
        /// <param name="ageInterval">The age interval</param>
        /// <param name="maximumAge">The maxium age that is displayed on the graph</param>
        /// <returns>The requested aging data</returns>
        public List<GraphEntry> Incident_RetrieveAging(int projectId, int? releaseId, int maximumAge, int ageInterval)
        {
            const string METHOD_NAME = "Incident_RetrieveAging";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view incidents
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the aging count and convert to JSON object
                IncidentManager incidentManager = new IncidentManager();
                DataSet incidentAgingDataSet = incidentManager.RetrieveAging(projectId, releaseId, maximumAge, ageInterval);
                List<GraphEntry> graphEntries = new List<GraphEntry>();
                foreach (DataRow dataRow in incidentAgingDataSet.Tables["IncidentAging"].Rows)
                {
                    GraphEntry graphEntry = new GraphEntry();
                    graphEntry.Name = (string)dataRow["Age"];
                    graphEntry.Caption = (string)dataRow["Age"];
                    graphEntry.Count = (int)dataRow["Count"];
                    graphEntries.Add(graphEntry);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return graphEntries;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }


        /// <summary>Retrieves the test case coverage for resolved incidents in the project/release</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we want to filter on (null for all)</param>
        /// <returns>List of incident test case coverage</returns>
        /// <remarks>Always returns all the execution status codes</remarks>
        public List<GraphEntry> Incident_RetrieveTestCoverage(int projectId, int? releaseId)
        {
            const string METHOD_NAME = "Incident_RetrieveTestCoverage";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view incidents
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the aging count and convert to JSON object
                IncidentManager incidentManager = new IncidentManager();
                List<IncidentTestCoverage> incidentTestCoverages = incidentManager.RetrieveTestCoverage(projectId, releaseId);
                List<GraphEntry> graphEntries = new List<GraphEntry>();
                foreach (IncidentTestCoverage incidentTestCoverage in incidentTestCoverages)
                {
                    GraphEntry graphEntry = new GraphEntry();
                    graphEntry.Name = incidentTestCoverage.ExecutionStatusId.ToString();
                    graphEntry.Caption = incidentTestCoverage.ExecutionStatusName;
                    graphEntry.Count = incidentTestCoverage.TestCount;
                    graphEntry.Color = TestCaseManager.GetExecutionStatusColor(incidentTestCoverage.ExecutionStatusId);
                    graphEntries.Add(graphEntry);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return graphEntries;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the data for a simple donut chart of the # open and closed incidents in a project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The counts for open and closed</returns>
        /// <param name="releaseId">Should we filter by release</param>
        /// <param name="useResolvedRelease">If filtering by release, use resolved release instead of detected release</param>
        public GraphData Incident_RetrieveCountByOpenClosedStatus(int projectId, int? releaseId, bool useResolvedRelease)
        {
            const string METHOD_NAME = "Incident_RetrieveCountByOpenClosedStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (full view needed since it's showing the count of all incidents)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                GraphData graphData = new GraphData();

                IncidentManager incidentManager = new IncidentManager();
                List<IncidentOpenClosedCount> counts = incidentManager.RetrieveOpenClosedCount(projectId, releaseId, useResolvedRelease);
                foreach (IncidentOpenClosedCount count in counts)
                {
                    DataSeries dataSeries = new DataSeries();
                    dataSeries.Value = count.IncidentCount;
                    dataSeries.Caption = (count.IsOpenStatus) ? Resources.Fields.IncidentStatus_AllOpen : Resources.Fields.IncidentStatus_AllClosed;
                    dataSeries.Color = (count.IsOpenStatus) ? "eeeeee" : "7eff7a";

                    graphData.Series.Add(dataSeries);
                }
                
                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the data for a simple donut chart of the # open incidents in a project by priority
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The counts for each priority, only for open incidents</returns>
        /// <param name="releaseId">Should we filter by release</param>
        /// <param name="useResolvedRelease">If filtering by release, use resolved release instead of detected release</param>
        public GraphData Incident_RetrieveCountByPriority(int projectId, int? releaseId, bool useResolvedRelease)
        {
            const string METHOD_NAME = "Incident_RetrieveCountByPriority";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (full view needed since it's showing the count of all incidents)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                GraphData graphData = new GraphData();

                IncidentManager incidentManager = new IncidentManager();
                List<IncidentOpenCountByPrioritySeverity> counts = incidentManager.RetrieveOpenCountByPrioritySeverity(projectId, releaseId, false, useResolvedRelease);
                foreach (IncidentOpenCountByPrioritySeverity count in counts)
                {
                    DataSeries dataSeries = new DataSeries();
                    dataSeries.Value = count.Count;
                    dataSeries.Caption = count.PrioritySeverityName;
                    dataSeries.Color = count.PrioritySeverityColor;


                    graphData.Series.Add(dataSeries);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the data for a simple donut chart of the # open incidents in a project by severity
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The counts for each priority, only for open incidents</returns>
        /// <param name="releaseId">Should we filter by release</param>
        /// <param name="useResolvedRelease">If filtering by release, use resolved release instead of detected release</param>
        public GraphData Incident_RetrieveCountBySeverity(int projectId, int? releaseId, bool useResolvedRelease)
        {
            const string METHOD_NAME = "Incident_RetrieveCountBySeverity";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (full view needed since it's showing the count of all incidents)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                GraphData graphData = new GraphData();

                IncidentManager incidentManager = new IncidentManager();
                List<IncidentOpenCountByPrioritySeverity> counts = incidentManager.RetrieveOpenCountByPrioritySeverity(projectId, releaseId, true, useResolvedRelease);
                foreach (IncidentOpenCountByPrioritySeverity count in counts)
                {
                    DataSeries dataSeries = new DataSeries();
                    dataSeries.Value = count.Count;
                    dataSeries.Caption = count.PrioritySeverityName;
                    dataSeries.Color = count.PrioritySeverityColor;

                    graphData.Series.Add(dataSeries);
                }

                Logger.LogExitingEvent(METHOD_NAME);
                Logger.Flush();

                return graphData;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Incident interface methods

        /// <summary>
		/// Updates the type of incident filter that should be applied on the release details page
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseFilterType">The type of filter (1 = detected, 2 = resolved, 3 = verified)</param>
		public void Incident_UpdateReleaseDetailsFilter(int projectId, int releaseFilterType)
		{
			const string METHOD_NAME = "Incident_UpdateReleaseDetailsFilter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited ok)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting for the release filter type (detected, resolved, verified)
				ProjectSettingsCollection collection = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS);
				collection[GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENTS_RELEASE_FILTER_TYPE] = releaseFilterType;
				collection.Save();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Retrieves a list of incidents that are related to a specific test run step or test step</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the test run step</param>
		/// <param name="testStepId">The id of the test step (optional)</param>
		/// <returns>List of data items</returns>
		/// <remarks>Also returns incidents related to previous test runs of the same test step</remarks>
		public SortedData RetrieveByTestRunStepId(int projectId, int testRunStepId, int? testStepId)
		{
			const string METHOD_NAME = "RetrieveByTestRunStepId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

			try
			{
				//Create the array of data items
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

				//If the test step was deleted then just get incidents linked to the test run step itself
				List<IncidentView> incidents;
				IncidentManager incidentManager = new IncidentManager();
				if (testStepId.HasValue)
				{
					incidents = incidentManager.RetrieveByTestStepId(testStepId.Value);
				}
				else
				{
					incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
				}

				//Populate the data items
				foreach (IncidentView incident in incidents)
				{
                    SortedDataItem dataItem = new SortedDataItem();
					dataItems.Add(dataItem);
					dataItem.PrimaryKey = incident.IncidentId;

					//The IncidentId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "IncidentId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = incident.IncidentId;
					dataItemField.TextValue = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incident.IncidentId);
					dataItem.Fields.Add("IncidentId", dataItemField);

					//The IncidentStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "IncidentStatusId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = incident.IncidentStatusId;
					dataItemField.TextValue = incident.IncidentStatusName;
					dataItem.Fields.Add("IncidentStatusId", dataItemField);

					//The IncidentTypeId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "IncidentTypeId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = incident.IncidentTypeId;
					dataItemField.TextValue = incident.IncidentTypeName;
					dataItem.Fields.Add("IncidentTypeId", dataItemField);

					//The PriorityId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "PriorityId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItem.Fields.Add("PriorityId", dataItemField);
					if (incident.PriorityId.HasValue)
					{
						dataItemField.IntValue = incident.PriorityId;
						dataItemField.TextValue = incident.PriorityName;
						dataItemField.CssClass = incident.PriorityColor;
					}

					//The SeverityId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "SeverityId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItem.Fields.Add("SeverityId", dataItemField);
					if (incident.SeverityId.HasValue)
					{
						dataItemField.IntValue = incident.SeverityId;
						dataItemField.TextValue = incident.SeverityName;
                        dataItemField.CssClass = incident.SeverityColor;
					}

					//The Name field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
					dataItemField.TextValue = incident.Name;
					dataItem.Fields.Add("Name", dataItemField);

					//The CreationDate field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "CreationDate";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
					dataItemField.DateValue = incident.CreationDate;
					dataItemField.TextValue = String.Format(GlobalFunctions.FORMAT_DATE, incident.CreationDate);
					dataItem.Fields.Add("CreationDate", dataItemField);

					//The OpenerId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OpenerId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItemField.IntValue = incident.OpenerId;
					dataItemField.TextValue = incident.OpenerName;
					dataItem.Fields.Add("OpenerId", dataItemField);

					//The OwnerId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OwnerId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
					dataItem.Fields.Add("OwnerId", dataItemField);
					if (incident.OwnerId.HasValue)
					{
						dataItemField.IntValue = incident.OwnerId.Value;
						dataItemField.TextValue = incident.OwnerName;
					}
				}

                return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Checks to see if an incident id exists and the cureent user has access to it (any project, or specific project)
        /// </summary>
        /// <param name="projectId">The current project (if specified)</param>
        /// <param name="incidentId">The id of the incident</param>
        /// <returns>True if it exists</returns>
        public bool Incident_CheckExists(int? projectId, int incidentId)
        {
            const string METHOD_NAME = "Incident_CheckExists";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            try
            {
                //Retrieve
                IncidentManager incidentManager = new IncidentManager();
                IncidentView incidentView = incidentManager.RetrieveById2(incidentId);
                if (incidentView == null)
                {
                    return false;
                }
                
                //See if we have a project specified
                if (projectId.HasValue && projectId != incidentView.ProjectId)
                {
                    return false;
                }

                //Make sure we're authorized
                Project.AuthorizationState authorizationState = IsAuthorized(incidentView.ProjectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
                if (authorizationState == Project.AuthorizationState.Prohibited)
                {
                    return false;
                }
                return true;
            }
            catch (ArtifactNotExistsException)
            {
                return false;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Sorted List interface methods

        /// <summary>Returns a list of incidents in the system for the specific user/project</summary>
        /// <param name="userId">The user we're viewing the incidents as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the incident and custom property business objects
				IncidentManager incidentManager = new IncidentManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the list of components (cannot use a lookup field since multilist). Include inactive, but not deleted
				List<Component> components = new ComponentManager().Component_Retrieve(projectId, false, false);

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Now get the list of populated filters and the current sort
                string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST;
                string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION;
                if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Incidents)
                {
                    filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_FILTERS;
                    sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_GENERAL;
                }
                if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Incidents)
                {
                    filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_FILTERS;
                    sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_GENERAL;
                }
                if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestRun_Incidents)
                {
                    filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_FILTERS;
                    sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_GENERAL;
                }

				//Now get the list of populated filters and the current sort
                Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
                string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "IncidentId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Add any standard filters
				if (standardFilters != null && standardFilters.Count > 0)
				{
					Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
					foreach (KeyValuePair<string, object> filter in deserializedFilters)
					{
						filterList[filter.Key] = filter.Value;
					}
				}

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);
				sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Incident);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (paginationSettings["NumberRowsPerPage"] != null)
				{
					paginationSize = (int)paginationSettings["NumberRowsPerPage"];
				}
				if (paginationSettings["CurrentPage"] != null)
				{
					currentPage = (int)paginationSettings["CurrentPage"];
				}
				//Get the number of incidents in the project
				int artifactCount = incidentManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}

				//**** Now we need to actually populate the rows of data to be returned ****
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//Display the pagination information
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;
				sortedData.StartRow = startRow;

				//Display the visible and total count of artifacts
				sortedData.VisibleCount = incidents.Count;
				sortedData.TotalCount = artifactCount;

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true, false, true);

				//Iterate through all the incidents and populate the dataitem
				foreach (IncidentView incident in incidents)
				{
					//We clone the template item as the basis of all the new items
					SortedDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(dataItem, incident, customProperties, false, null, components);
					dataItems.Add(dataItem);
				}

				//Also include the pagination info
				sortedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return sortedData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns a plain-text version of the artifact name/description typically used in dynamic tooltips</summary>
		/// <param name="incidentId">The id of the incident to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		/// <remarks>For incidents also includes the most recent resolution</remarks>
        public string RetrieveNameDesc(int? projectId, int incidentId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
			{
				//Instantiate the incident business object
				IncidentManager incidentManager = new IncidentManager();

				//Now retrieve the specific incident - handle quietly if it doesn't exist
				try
				{
					Incident incident = incidentManager.RetrieveById(incidentId, true);
					string tooltip;
					tooltip = "<u>" + Encoder.HtmlEncode(incident.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(incident.Description);

					//See if we have any comments to append
					if (incident.Resolutions.Count > 0)
					{
						IncidentResolution resolution = incident.Resolutions.OrderByDescending(r => r.CreationDate).First();

						tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
							GlobalFunctions.LocalizeDate(resolution.CreationDate).ToShortDateString(),
							GlobalFunctions.HtmlRenderAsPlainText(resolution.Resolution),
							resolution.Creator.FullName
							);
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the incident, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for incident");
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return Resources.Messages.Global_UnableRetrieveTooltip;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the current sort stored in the system (property and direction)
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The artifact property we want to sort on</param>
		/// <param name="sortAscending">Are we sorting ascending or not</param>
		/// <returns>Any error messages</returns>
		public string SortedList_UpdateSort(int projectId, string sortProperty, bool sortAscending, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

            string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION;
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Incidents)
            {
                sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_GENERAL;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Incidents)
            {
                sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_GENERAL;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestRun_Incidents)
            {
                sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_GENERAL;
            }

			//Call the base method with the appropriate settings collection
            return base.UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Any error messages</returns>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
		{
			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

            string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST;
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Incidents)
            {
                filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_DETAILS_INCIDENTS_FILTERS;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Incidents)
            {
                filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_DETAILS_INCIDENTS_FILTERS;
            }
            if (displayTypeId.HasValue && displayTypeId.Value == (int)DataModel.Artifact.DisplayTypeEnum.TestRun_Incidents)
            {
                filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_DETAILS_INCIDENTS_FILTERS;
            }

			//Call the base method with the appropriate settings collection
            return base.UpdateFilters(userId, projectId, filters, filtersSettingsCollection, DataModel.Artifact.ArtifactTypeEnum.Incident);
		}

        /// <summary>
        /// Saves the current filters with the specified name
        /// </summary>
        /// <param name="includeColumns">Should we include the column selection</param>
        /// <param name="existingSavedFilterId">Populated if we're updating an existing saved filter</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="name">The name of the filter</param>
        /// <param name="isShared">Is this a shared filter</param>
        /// <returns>Validation/error message (or empty string if none)</returns>
        public string SaveFilter(int projectId, string name, bool isShared, int? existingSavedFilterId, bool includeColumns)
		{
			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Incident, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, isShared, existingSavedFilterId, includeColumns);
		}

        /// <summary>
        /// Retrieves a list of saved filters for the current user/project
        /// </summary>
        /// <param name="includeShared">Should we include shared ones</param>
        /// <param name="projectId">The current project</param>
        /// <returns>Dictionary of saved filters</returns>
        public JsonDictionaryOfStrings RetrieveFilters(int projectId, bool includeShared)
		{
			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic implementation
			return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, includeShared);
		}

		/// <summary>
		/// Returns the latest information on a single incident in the system
		/// </summary>
		/// <param name="userId">The user we're viewing the incident as</param>
		/// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A single dataitem object</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			try
			{
				//Instantiate the incident and custom property business objects
				IncidentManager incidentManager = new IncidentManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the incident dataset record for the specific incident id
				IncidentView incident = incidentManager.RetrieveById2(artifactId);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (incident.OwnerId.HasValue)
				{
					ownerId = incident.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && incident.OpenerId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}
                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.Incident, true);

				//Finally populate the dataitem from the dataset
				if (incident != null)
				{
					//See if we already have an artifact custom property row
					if (artifactCustomProperty != null)
					{
						PopulateRow(dataItem, incident, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null);
					}
					else
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true, false);
						PopulateRow(dataItem, incident, customProperties, true, null, null);
					}

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("IncidentStatusId"))
					{
						dataItem.Fields["IncidentStatusId"].Editable = false;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates records of data in the system
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItems">The updated data records</param>
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Iterate through each data item and make the updates
                IncidentManager incidentManager = new IncidentManager();
				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the incident id
					int incidentId = dataItem.PrimaryKey;

					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					Incident incident = incidentManager.RetrieveById(incidentId, false);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					if (incident != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								incident.ConcurrencyDate = concurrencyDateTimeValue;
								incident.AcceptChanges();
							}
						}

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("Resolution");
						fieldsToIgnore.Add("CreationDate");
						UpdateFields(validationMessages, dataItem, incident, customProperties, artifactCustomProperty, projectId, incidentId, 0, fieldsToIgnore);

						//Now verify the options for the custom properties to make sure all rules have been followed
						Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
						foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = customPropOptionMessage.Key;
							newMsg.Message = customPropOptionMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Perform any business level validations on the datarow
						Dictionary<string, string> businessMessages = incidentManager.Validate(incident);
						foreach (KeyValuePair<string, string> businessMessage in businessMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = businessMessage.Key;
							newMsg.Message = businessMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Make sure we have no validation messages before updating
						if (validationMessages.Count == 0)
						{
							//Get copies of everything..
							Incident notificationArt = incident.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database
							try
							{
								incidentManager.Update(incident, userId);
							}
							catch (DataValidationException exception)
							{
								return CreateSimpleValidationMessage(exception.Message);
							}
							catch (OptimisticConcurrencyException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
							}
							customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

							//Call notifications..
							try
							{
								//Check Workflow Transitions, first.
								//Get the list of workflow fields and custom properties
								WorkflowManager workflowManager = new WorkflowManager();
								int workflowId = workflowManager.Workflow_GetForIncidentType(incident.IncidentTypeId);
								int numSent = new WorkflowManager().Workflow_NotifyStatusChange(notificationArt.IncidentId, workflowId, notificationArt);
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident " + incident.ArtifactToken + ".");
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Inserts a new incident into the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifact">The type of artifact we're inserting</param>
		/// <returns>Not implemented for incidents since they use the details screen</returns>
		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			throw new NotImplementedException("This operation is not currently implemented");
		}

        /// <summary>
        /// Adds/removes a column from the list of fields displayed in the current view
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="fieldName">The name of the column we displaying/hiding</param>
        public void ToggleColumnVisibility(int projectId, string fieldName)
		{
			const string METHOD_NAME = "ToggleColumnVisibility";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//See if we have a custom property (they need to be handled differently)
				if (CustomPropertyManager.IsFieldCustomProperty(fieldName).HasValue)
				{
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                    //Toggle the status of the appropriate custom property
                    Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, fieldName);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Copies a set of incidents
		/// </summary>
		/// <param name="userId">The ID of the user making the copy</param>
		/// <param name="items">The items to copy</param>
		public void SortedList_Copy(int projectId, List<string> items)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
			{
				//Iterate through all the items to be copied
				IncidentManager incidentManager = new IncidentManager();
				foreach (string itemValue in items)
				{
					//Get the incident ID
					int incidentId = Int32.Parse(itemValue);
					incidentManager.Copy(userId, incidentId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Exports a set of incidents to another project</summary>
		/// <param name="items">The items to export</param>
		/// <param name="destProjectId">The project to export them to</param>
		public void SortedList_Export(int destProjectId, List<string> items)
		{

			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be exported
				IncidentManager incidentManager = new IncidentManager();
				foreach (string itemValue in items)
				{
					//Get the incident ID
					int incidentId = Int32.Parse(itemValue);
					incidentManager.Export(incidentId, destProjectId, userId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a set of incidents
		/// </summary>
		/// <param name="items">The items to delete</param>
		/// <param name="projectId">The id of the project (not used)</param>
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be deleted
				IncidentManager incidentManager = new IncidentManager();
				foreach (string itemValue in items)
				{
					//Get the incident ID
					int incidentId = Int32.Parse(itemValue);
					incidentManager.MarkAsDeleted(projectId, incidentId, userId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, PROJECT_SETTINGS_PAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		/// <summary>
		/// Updates the size of pages returned and the currently selected page
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
		/// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
		public void UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			const string METHOD_NAME = "UpdatePagination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pagination settings collection and update
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				if (pageSize != -1)
				{
					paginationSettings["NumberRowsPerPage"] = pageSize;
				}
				if (currentPage != -1)
				{
					paginationSettings["CurrentPage"] = currentPage;
				}
				paginationSettings.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        #endregion

        #region Internal Functions

        /// <summary>
        /// Populates the equalizer type graph for the incident progress
        /// </summary>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
		{
			const string METHOD_NAME = "PopulateEqualizer";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Explicitly recast the data-row to the type we're expecting
			IncidentView incidentView = (IncidentView)artifact;

			//Calculate the information to display
			int percentGreen;
			int percentRed;
			int percentYellow;
			int percentGray;
			Incident incident = incidentView.ConvertTo<IncidentView, Incident>();
			string tooltipText = IncidentManager.CalculateProgress(incident, out percentGreen, out percentRed, out percentYellow, out percentGray);

			//Now populate the equalizer graph
			dataItemField.EqualizerGreen = percentGreen;
			dataItemField.EqualizerRed = percentRed;
			dataItemField.EqualizerYellow = percentYellow;
			dataItemField.EqualizerGray = percentGray;

			//Populate Tooltip
			dataItemField.TextValue = "";
			dataItemField.Tooltip = tooltipText;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="incidentView">The entity containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <param name="components">The list of components in the project (or null)</param>
		/// <param name="artifactCustomProperty">The artifatc's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		protected void PopulateRow(SortedDataItem dataItem, IncidentView incidentView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<Component> components, List<WorkflowField> workflowFields = null, List<WorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = incidentView.IncidentId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, incidentView.ConcurrencyDate);

			//Specify if it has an attachment or not
			dataItem.Attachment = incidentView.IsAttachments;

            //The date and some effort fields are not editable
            List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "ProjectedEffort", "CompletionPercent" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (incidentView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, incidentView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, workflowFields, workflowCustomProps, readOnlyFields);

					//Apply the conditional formatting to the priority and severity columns (if displayed)
					if (dataItemField.FieldName == "PriorityId" && incidentView.PriorityId.HasValue)
					{
						//Despite the name, cssClass can store either color or CSS class for SortedDataItem's
						dataItemField.CssClass = "#" + incidentView.PriorityColor;
					}
					if (dataItemField.FieldName == "SeverityId" && incidentView.SeverityId.HasValue)
					{
						//Despite the name, cssClass can store either color or CSS class for SortedDataItem's
						dataItemField.CssClass = "#" + incidentView.SeverityColor;
					}

					//Add the component name(s) if specified
					if (components != null && dataItemField.FieldName == "ComponentIds" && !String.IsNullOrEmpty(incidentView.ComponentIds))
					{
						List<int> componentIds = incidentView.ComponentIds.FromDatabaseSerialization_List_Int32();
						string textValue;
						string tooltip;
						ComponentManager.GetComponentNamesFromIds(componentIds, components, Resources.Main.Global_Multiple, out textValue, out tooltip);
						dataItemField.TextValue = textValue;
						dataItemField.Tooltip = tooltip;
					}
				}
			}
		}

		/// <summary>
		/// Gets the list of lookup values and names for a specific lookup
		/// </summary>
		/// <param name="lookupName">The name of the lookup</param>
		/// <param name="projectId">The id of the project - needed for some lookups</param>
		/// <returns>The name/value pairs</returns>
		protected JsonDictionaryOfStrings GetLookupValues(string lookupName, int projectId, int projectTemplateId)
		{
			const string METHOD_NAME = "GetLookupValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				JsonDictionaryOfStrings lookupValues = null;
				ReleaseManager release = new ReleaseManager();
				Business.UserManager user = new Business.UserManager();
				IncidentManager incidentManager = new IncidentManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				if (lookupName == "OpenerId" || lookupName == "OwnerId")
				{
					List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "ProgressId")
				{
					lookupValues = new JsonDictionaryOfStrings(incidentManager.RetrieveProgressFiltersLookup());
				}
				if (lookupName == "ComponentIds")
				{
					List<DataModel.Component> components = new ComponentManager().Component_Retrieve(projectId);
					lookupValues = ConvertLookupValues(components.OfType<DataModel.Entity>().ToList(), "ComponentId", "Name");
				}
				if (lookupName == "PriorityId")
				{
					List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true);
					lookupValues = ConvertLookupValues(incidentPriorities.OfType<Entity>().ToList(), "PriorityId", "Name");
				}
				if (lookupName == "BuildId")
				{
					List<BuildView> builds = new BuildManager().RetrieveForProject(projectId);
					lookupValues = ConvertLookupValues(builds.OfType<Entity>().ToList(), "BuildId", "Name");
				}
				if (lookupName == "SeverityId")
				{
					List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, true);
					lookupValues = ConvertLookupValues(incidentSeverities.OfType<Entity>().ToList(), "SeverityId", "Name");
				}
				if (lookupName == "IncidentStatusId")
				{
					List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);
					lookupValues = new JsonDictionaryOfStrings();
					//Add the composite (All Open) and (All Closed) items to the incident status filter
					lookupValues.Add(IncidentManager.IncidentStatusId_AllOpen.ToString(), Resources.Fields.IncidentStatus_AllOpen);
					lookupValues.Add(IncidentManager.IncidentStatusId_AllClosed.ToString(), Resources.Fields.IncidentStatus_AllClosed);

					//Now add the real lookup values
					AddLookupValues(lookupValues, incidentStati.OfType<Entity>().ToList(), "IncidentStatusId", "Name");
				}
				if (lookupName == "IncidentTypeId")
				{
					List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(projectTemplateId, true);
					lookupValues = new JsonDictionaryOfStrings();
					//Add the composite (All Issues) items to the incident type filter
					lookupValues.Add(IncidentManager.IncidentTypeId_AllIssues.ToString(), Resources.Fields.IncidentType_AllIssues);

					//Now add the real lookup values
					AddLookupValues(lookupValues, incidentTypes.OfType<Entity>().ToList(), "IncidentTypeId", "Name");
				}

                //See if we should return all releases or just active for the different release dropdowns
                ProjectSettings projectSettings = new ProjectSettings(projectId);
                if (projectSettings.DisplayOnlyActiveReleasesForDetected)
                {
                    if (lookupName == "ResolvedReleaseId" || lookupName == "VerifiedReleaseId" || lookupName == "DetectedReleaseId")
                    {
                        //Resolved-in and verified-in release includes just active releases
                        List<ReleaseView> releases = release.RetrieveByProjectId(projectId, true, true);
                        lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                    }
                }
                else
                {
                    if (lookupName == "DetectedReleaseId")
                    {
                        //Detected-in release includes all releases (active and inactive) - so don't pass in the active flag as a field
                        List<ReleaseView> releases = release.RetrieveByProjectId(projectId, false, true);
                        lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase");
                    }
                    if (lookupName == "ResolvedReleaseId" || lookupName == "VerifiedReleaseId")
                    {
                        //Resolved-in and verified-in release includes just active releases
                        List<ReleaseView> releases = release.RetrieveByProjectId(projectId, false, true);
                        lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                    }
                }

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, customPropertyNumber.Value, true);
					if (customProperty != null)
					{
						//Handle the case of normal lists
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
						{
							if (customProperty.List != null && customProperty.List.Values.Count > 0)
							{
								lookupValues = ConvertLookupValues(CustomPropertyManager.SortCustomListValuesForLookups(customProperty.List), "CustomPropertyValueId", "Name");
							}
						}

						//Handle the case of user lists
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User)
						{
							List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
							lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
						}

						//Handle the case of flags
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.Boolean)
						{
							lookupValues = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
						}
					}
				}

				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Used to populate the shape of the special compound fields used to display the information
		/// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
		/// </summary>
		/// <param name="dataItemField">The field whose shape we're populating</param>
		/// <param name="fieldName">The field name we're handling</param>
		/// <param name="filterList">The list of filters</param>
		/// <param name="projectId">The project we're interested in</param>
        /// <param name="projectTemplateId">the id of the project templayte</param>
		protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
		{
			//Check to see if this is a field we can handle
			if (fieldName == "ProgressId")
			{
				dataItemField.FieldName = "ProgressId";
				string filterLookupName = fieldName;
				dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(filterLookupName))
				{
					dataItemField.IntValue = (int)filterList[filterLookupName];
				}
			}
		}

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the incidents as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
		{
			//There are no static columns to add

			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(Artifact.ArtifactTypeEnum.Incident, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);

            //Change the heading of the Progress column to be 'Progress' not the default 'Task Progress'
            if (dataItem.Fields.ContainsKey("ProgressId"))
            {
                DataItemField progressField = dataItem.Fields["ProgressId"];
                if (progressField != null)
                {
                    progressField.Caption = Resources.Fields.Progress;
                }
            }
		}

        /// <summary>
        /// Verifies the digital signature on a workflow status change if it is required
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="originalStatusId">The original status</param>
        /// <param name="currentStatusId">The new status</param>
        /// <param name="signature">The digital signature</param>
        /// <param name="detectorId">The detector of the incident</param>
        /// <param name="ownerId">The owner of the incident</param>
        /// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
        protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int detectorId, int? ownerId)
        {
            const string METHOD_NAME = "VerifyDigitalSignature";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                WorkflowManager workflowManager = new WorkflowManager();
                WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
                if (workflowTransition == null)
                {
                    //No transition possible, so return failure
                    return false;
                }
                if (!workflowTransition.IsSignatureRequired)
                {
                    //No signature required, so return null
                    return null;
                }

                //Make sure we have a signature at this point
                if (signature == null)
                {
                    return false;
                }

                //Make sure the login/password was valid
                string lowerUser = signature.Login.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                bool isValidUser = Membership.ValidateUser(lowerUser, signature.Password);

                //If the password check does not return, lets see if the password is a GUID and test it against RSS/API Key
                Guid passGu;
                if (!isValidUser && Guid.TryParse(signature.Password, out passGu))
                {
                    SpiraMembershipProvider prov = (SpiraMembershipProvider)Membership.Provider;
                    if (prov != null)
                    {
                        isValidUser = prov.ValidateUserByRssToken(lowerUser, signature.Password, true, true);
                    }
                }

                if (!isValidUser)
                {
                    //User's login/password does not match
                    return false;
                }

                //Make sure the login is for the current user
                MembershipUser user = Membership.GetUser();
                if (user == null)
                {
                    //Not authenticated (should't ever hit this point)
                    return false;
                }
                if (user.UserName != signature.Login)
                {
                    //Signed login does not match current user
                    return false;
                }
                int userId = (int)user.ProviderUserKey;
                int? projectRoleId = SpiraContext.Current.ProjectRoleId;

                //Make sure the user can execute this transition
                bool isAllowed = false;
                workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
                if (workflowTransition.IsExecuteByDetector && detectorId == userId)
                {
                    isAllowed = true;
                }
                else if (workflowTransition.IsExecuteByOwner && ownerId.HasValue && ownerId.Value == userId)
                {
                    isAllowed = true;
                }
                else if (projectRoleId.HasValue && workflowTransition.TransitionRoles.Any(r => r.ProjectRoleId == projectRoleId.Value))
                {
                    isAllowed = true;
                }
                return isAllowed;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region INavigationService Methods

        /// <summary>
        /// Updates the size of pages returned and the currently selected page
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="pageSize">The number of rows per page (pass -1 to leave alone)</param>
        /// <param name="currentPage">The current page we're on (pass -1 to leave alone)</param>
        public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			//Same implementation as the list service
			this.UpdatePagination(projectId, pageSize, currentPage);
		}

		/// <summary>
		/// Returns a list of incidents for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">Not used for incidents since not hierarchical</param>
		/// <returns>List of incidents</returns>
		/// <param name="displayMode">
		/// The display mode of the navigation list:
		/// 1 = Filtered List
		/// 2 = All Items (no filters)
		/// 3 = Assigned to the Current User
		/// 4 = Detected by the Current User
		/// </param>
		/// <param name="selectedItemId">The id of the currently selected item</param>
		/// <remarks>Returns just the child items of the passed-in indent-level</remarks>
		public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
		{
			const string METHOD_NAME = "NavigationBar_RetrieveList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the incident business object
				IncidentManager incidentManager = new IncidentManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "IncidentId ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 15;
				int currentPage = 1;
				if (paginationSettings["NumberRowsPerPage"] != null)
				{
					paginationSize = (int)paginationSettings["NumberRowsPerPage"];
				}
				if (paginationSettings["CurrentPage"] != null)
				{
					currentPage = (int)paginationSettings["CurrentPage"];
				}
				//Get the number of incidents in the project
				int artifactCount = incidentManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the incidents list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				List<IncidentView> incidents = null;
				if (displayMode == 2)
				{
					//Make sure authorized for all items
					if (authorizationState != Project.AuthorizationState.Authorized)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//All Items
					incidents = incidentManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, 0);
				}
                else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//Assigned to User
					incidents = incidentManager.RetrieveOpenByOwnerId(userId, projectId, null);
				}
                else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Detected)
				{
					//Detected by User
					incidents = incidentManager.RetrieveOpenByOpenerId(userId, projectId);
				}
				else
				{
					//Make sure authorized for all items
					if (authorizationState != Project.AuthorizationState.Authorized)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Filtered List
					incidents = incidentManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				}

				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings["CurrentPage"] = currentPage;
					paginationSettings.Save();
				}

				//Iterate through all the incidents and populate the dataitem (only some columns are needed)
				foreach (IncidentView incident in incidents)
				{
					//Create the data-item
					HierarchicalDataItem dataItem = new HierarchicalDataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = incident.IncidentId;
					dataItem.Indent = "";
					dataItem.Expanded = false;

					//Name/Desc (include the IN #)
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = incident.Name + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId, true);
					dataItem.Summary = false;
					dataItem.Alternate = false;
					dataItem.Fields.Add("Name", dataItemField);

					//Add to the items collection
					dataItems.Add(dataItem);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
		{
			//Same implementation as the list service
			return RetrievePaginationOptions(projectId);
		}

		/// <summary>
		/// Updates the display settings used by the Navigation Bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="displayMode">The current display mode</param>
		/// <param name="displayWidth">The display width</param>
		/// <param name="minimized">Is the navigation bar minimized or visible</param>
		public void NavigationBar_UpdateSettings(int projectId, Nullable<int> displayMode, Nullable<int> displayWidth, Nullable<bool> minimized)
		{
			const string METHOD_NAME = "NavigationBar_UpdateSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the user's project settings
				bool changed = false;
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION);
				if (displayMode.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE] = displayMode.Value;
					changed = true;
				}
				if (minimized.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED] = minimized.Value;
					changed = true;
				}
				if (displayWidth.HasValue)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH] = displayWidth.Value;
					changed = true;
				}
				if (changed)
				{
					settings.Save();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region WorkflowOperations Methods

		/// <summary>
		/// Retrieves the list of workflow operations for the current incident
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="typeId">The incident type</param>
		/// <param name="artifactId">The id of the incident</param>
		/// <returns>The list of available workflow operations</returns>
		/// <remarks>Pass a specific type id if the user has changed the type of the incident, but not saved it yet.</remarks>
		public List<DataItem> WorkflowOperations_Retrieve(int projectId, int artifactId, int? typeId)
		{
			const string METHOD_NAME = "WorkflowOperations_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the array of data items to store the workflow operations
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of available transitions for the current step in the workflow
				IncidentManager incidentManager = new IncidentManager();
				WorkflowManager workflowManager = new WorkflowManager();
				Incident incident = incidentManager.RetrieveById(artifactId, false);
				int workflowId;
				if (typeId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForIncidentType(typeId.Value);
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForIncidentType(incident.IncidentTypeId);
				}

				//Get the current user's role
				int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

				//Determine if the current user is the detected or owner of the incident
				bool isDetector = false;
				if (incident.OpenerId == CurrentUserId.Value)
				{
					isDetector = true;
				}
				bool isOwner = false;
				if (incident.OwnerId.HasValue && incident.OwnerId.Value == CurrentUserId.Value)
				{
					isOwner = true;
				}
				int statusId = incident.IncidentStatusId;
				List<WorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isDetector, isOwner);

				//Populate the data items list
				foreach (WorkflowTransition workflowTransition in workflowTransitions)
				{
					//The data item itself
					DataItem dataItem = new DataItem();
					dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
					dataItems.Add(dataItem);

					//The WorkflowId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "WorkflowId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.WorkflowId;
					dataItem.Fields.Add("WorkflowId", dataItemField);

					//The Name field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = workflowTransition.Name;
					dataItem.Fields.Add("Name", dataItemField);

					//The InputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "InputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.InputIncidentStatusId;
					dataItemField.TextValue = workflowTransition.InputStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.OutputIncidentStatusId;
					dataItemField.TextValue = workflowTransition.OutputStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusOpenYn field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusOpenYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.OutputStatus.IsOpenStatus) ? "Y" : "N";
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The SignatureYn field (does it need a signature)
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "SignatureYn";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItemField.TextValue = (workflowTransition.IsSignatureRequired) ? "Y" : "N";
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				}

				return dataItems;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return nothing back
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IFormService methods

		/// <summary>Returns a single incident data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current incident - null means new incident</param>
		/// <returns>An incident data item</returns>
		public DataItem Form_Retrieve(int projectId, int? artifactId)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited edit or full edit)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the business classes
				IncidentManager incidentManager = new IncidentManager();
				WorkflowManager workflowManager = new WorkflowManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Create the data item record (no filter items)
                SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Get the incident for the specific incident id or just create a new temporary one
                IncidentView incidentView = null;
				ArtifactCustomProperty artifactCustomProperty = null;
				if (artifactId.HasValue)
				{
					incidentView = incidentManager.RetrieveById2(artifactId.Value);
					//The main dataset does not have the custom properties, they need to be retrieved separately
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, true);

					//Make sure the user is authorized for this item
					int ownerId = -1;
					if (incidentView.OwnerId.HasValue)
					{
						ownerId = incidentView.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && incidentView.OpenerId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

                    //Also need to add the read-only title field since we display 'New Incident' for new incidents
                    dataItem.Fields.Add("Title", new DataItemField() { FieldName = "Title", TextValue = incidentView.Name });

                    //Also need to return back a special field to denote if the user is the owner or creator of the incident
                    bool isArtifactCreatorOrOwner = (ownerId == userId || incidentView.OpenerId == userId);
                    dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });
				}
				else
				{
					//Insert Case, need to create the new incident
					incidentView = incidentManager.Incident_New(projectId, userId);

					//Also need to add a PlaceholderId field that's used to capture the placeholder id used for storing attachments and associations
					//prior to final ticket submission
					if (!dataItem.Fields.ContainsKey("PlaceholderId"))
					{
						dataItem.Fields.Add("PlaceholderId", new DataItemField() { FieldName = "PlaceholderId" });
					}

                    //Also need to add the read-only title field since we display 'New Incident' for new incidents
                    if (!dataItem.Fields.ContainsKey("Title"))
                    {
                        dataItem.Fields.Add("Title", new DataItemField() { FieldName = "Title", TextValue = Resources.Dialogs.Global_NewIncident });
                    }

					//Also we need to populate any default Artifact Custom Properties
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true, false);
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}

				//Get the list of workflow fields and custom properties
				int workflowId;
				int statusId;
				if (artifactId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForIncidentType(incidentView.IncidentTypeId);
					statusId = incidentView.IncidentStatusId;
				}
				else
				{
					//Get the default status and default workflow
					int incidentTypeId = incidentManager.GetDefaultIncidentType(projectTemplateId);
					workflowId = workflowManager.Workflow_GetForIncidentType(incidentTypeId);
					statusId = incidentManager.IncidentStatus_RetrieveDefault(projectTemplateId).IncidentStatusId;
					incidentView.IncidentStatusId = statusId; //Needed so that the form manager knows the default status
				}
				List<WorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
				List<WorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

				//Finally populate the dataitem from the dataset
				if (incidentView != null)
				{
					//See if we have any existing artifact custom properties for this row
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true, false);
						PopulateRow(dataItem, incidentView, customProperties, true, (ArtifactCustomProperty)null, null, workflowFields, workflowCustomProps);
					}
					else
					{
						PopulateRow(dataItem, incidentView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null, workflowFields, workflowCustomProps);
					}

                    //The Resolution (comments) field is not part of the entity so needs to be handled separately for workflow
					if (dataItem.Fields.ContainsKey("Resolution"))
					{
						DataItemField resolutionField = dataItem.Fields["Resolution"];
						if (workflowFields.Any(f => f.Field.Name == "Resolution" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							resolutionField.Required = true;
						}
						if (workflowFields.Any(f => f.Field.Name == "Resolution" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							resolutionField.Hidden = true;
						}
						if (!workflowFields.Any(f => f.Field.Name == "Resolution" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							resolutionField.Editable = true;
						}
					}

					//Populate any data mapping values are not part of the standard 'shape'
					if (artifactId.HasValue)
					{
						DataMappingManager dataMappingManager = new DataMappingManager();
						List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Incident, artifactId.Value);
						foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
						{
							DataItemField dataItemField = new DataItemField();
							dataItemField.FieldName = DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId;
							dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
							if (String.IsNullOrEmpty(artifactMapping.ExternalKey))
							{
								dataItemField.TextValue = "";
							}
							else
							{
								dataItemField.TextValue = artifactMapping.ExternalKey;
							}
							dataItemField.Editable = (SpiraContext.Current.IsProjectAdmin); //Read-only unless project admin
							dataItemField.Hidden = false;   //Always visible
							dataItem.Fields.Add(DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId, dataItemField);
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				Logger.Flush();

				return dataItem;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return no data back
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Creates a new incident id and returns it to the form
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The id of the new artifact or null if one is not created</returns>
        /// <remarks>
        /// Incidents don't create a new record when created, so we just need to return null for incidents
        /// </remarks>
        public override int? Form_New(int projectId, int artifactId)
        {
            return null;    //Tells the form manager to simply set the mode to 'New'
        }

        /// Clones the current requirement and returns the ID of the item to redirect to
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <returns>The id to redirect to</returns>
        public override int? Form_Clone(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Form_Clone";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to create the item
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Now we need to make a copy of it and then redirect to the copy
                IncidentManager incidentManager = new IncidentManager();
                int newIncidentId = incidentManager.Copy(userId, artifactId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return newIncidentId;
            }
            catch (ArtifactNotExistsException)
            {
                //The item does not exist, so return null
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes the current requirement and returns the ID of the item to redirect to (if any)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <returns>The id to redirect to</returns>
        public override int? Form_Delete(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Form_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to delete the item
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //First we need to determine which incident to redirect the user to after the delete
                int? newIncidentId = null;
                //Look through the current dataset to see what is the next incident in the list
                //If we are the last one on the list then we need to simply use the one before

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);

                //Get the sort information
                string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "IncidentId ASC");
                string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
                string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
                bool sortAscending = (sortDirectionString == "ASC");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_INCIDENT_PAGINATION_SIZE);
                paginationSettings.Restore();
                //Default values
                int paginationSize = 15;
                int currentPage = 1;
                if (paginationSettings["PaginationOption"] != null)
                {
                    paginationSize = (int)paginationSettings["PaginationOption"];
                }
                if (paginationSettings["CurrentPage"] != null)
                {
                    currentPage = (int)paginationSettings["CurrentPage"];
                }
                //Get the number of incidents in the project
                IncidentManager incidentManager = new IncidentManager();
                int artifactCount = incidentManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                //Get the incidents list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }

                List<IncidentView> incidentNavigationList = incidentManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                bool matchFound = false;
                int previousIncidentId = -1;
                foreach (IncidentView incident in incidentNavigationList)
                {
                    int testIncidentId = incident.IncidentId;
                    if (testIncidentId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) task
                        if (matchFound)
                        {
                            newIncidentId = testIncidentId;
                            break;
                        }

                        //If this matches the current incident, set flag
                        if (testIncidentId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousIncidentId = testIncidentId;
                        }
                    }
                }
                if (!newIncidentId.HasValue && previousIncidentId != -1)
                {
                    newIncidentId = previousIncidentId;
                }

                //Next we need to delete the current incident
                incidentManager.MarkAsDeleted(projectId, artifactId, userId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return newIncidentId;
            }
            catch (ArtifactNotExistsException)
            {
                //The item does not exist, so return null
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

		/// <summary>Saves a single incident data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The incident to save</param>
		/// <param name="operation">The type of save operation ('new', 'close', '', etc.)</param>
		/// <returns>Any error message or null if successful</returns>
        /// <param name="signature">Any digital signature</param>
        public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Save";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited is OK, we check that later)
            //For new incidents need create vs. modify permissions
            Project.PermissionEnum requiredPermission = (dataItem.PrimaryKey > 0) ? Project.PermissionEnum.Modify : Project.PermissionEnum.Create;

            Project.AuthorizationState authorizationState = IsAuthorized(projectId, requiredPermission, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the incident id
			int incidentId = dataItem.PrimaryKey;

			try
			{
				//Instantiate the business classes
				IncidentManager incidentManager = new IncidentManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				WorkflowManager workflowManager = new WorkflowManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

				//If we have a zero/negative primary key it means that it's actually a new item being inserted
				Incident incident;
				ArtifactCustomProperty artifactCustomProperty;
				if (incidentId < 1)
				{
					//Insert Case, need to use the Incident_New() method since we need the mandatory fields populated
					IncidentView incidentView = incidentManager.Incident_New(projectId, userId);
					incident = incidentView.ConvertTo<IncidentView, Incident>();

					//Also we need to populate any default Artifact Custom Properties
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}
				else
				{
					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					incident = incidentManager.RetrieveById(incidentId, false);

					//Make sure the user is authorized for this item if they only have limited permissions
					int ownerId = -1;
					if (incident.OwnerId.HasValue)
					{
						ownerId = incident.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && incident.OpenerId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

                    //Create a new artifact custom property row if one doesn't already exist
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, false, customProperties);
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}
				}

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["IncidentStatusId"].IntValue.HasValue) ? dataItem.Fields["IncidentStatusId"].IntValue.Value : -1;
				int originalStatusId = incident.IncidentStatusId;
				int incidentTypeId = (dataItem.Fields["IncidentTypeId"].IntValue.HasValue) ? dataItem.Fields["IncidentTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties
				int workflowId;
				if (incidentTypeId < 1)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).WorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForIncidentType(incidentTypeId);
				}
				List<WorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<WorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

                //If the workflow status changed, check to see if we need a digital signature and if it was provided and is valid
                if (currentStatusId != originalStatusId)
                {
					//Only attempt to verify signature requirements if we have no concurrency date or if the client side concurrency matches that from the DB
					bool shouldVerifyDigitalSignature = true;
					if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
					{
						DateTime concurrencyDateTimeValue;
						if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
						{
							shouldVerifyDigitalSignature = incident.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{
						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, incident.OpenerId, incident.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								incident.SignatureMeaning = signature.Meaning;
							}
							else
							{
								//Let the user know that the digital signature is not valid
								return CreateSimpleValidationMessage(Resources.Messages.Services_DigitalSignatureNotValid);
							}
						}
					}

                }

				//Need to set the original date of this record to match the concurrency date
				//The value is already in UTC so no need to convert
				if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
				{
					DateTime concurrencyDateTimeValue;
					if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
					{
						incident.ConcurrencyDate = concurrencyDateTimeValue;
						incident.AcceptChanges();
					}
				}

				//Now we can start tracking any changes
				incident.StartTracking();

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				if (incidentId < 1)
				{
					fieldsToIgnore.Add("PlaceholderId");
				}
				fieldsToIgnore.Add("Resolution");
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("LastUpdateDate");   //Breaks concurrency otherwise

				//Need to handle any data-mapping fields (project-admin only)
				if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId);
					foreach (KeyValuePair<string, DataItemField> kvp in dataItem.Fields)
					{
						DataItemField dataItemField = kvp.Value;
						if (dataItemField.FieldName.SafeSubstring(0, DataMappingManager.FIELD_PREPEND.Length) == DataMappingManager.FIELD_PREPEND)
						{
							//See if we have a matching row
							foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
							{
								if (DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId == dataItemField.FieldName)
								{
                                    artifactMapping.StartTracking();
                                    if (String.IsNullOrWhiteSpace(dataItemField.TextValue))
									{
										artifactMapping.ExternalKey = null;
									}
									else
									{
										artifactMapping.ExternalKey = dataItemField.TextValue;
									}
								}
							}
						}
					}

					//Now save the data
					dataMappingManager.SaveDataSyncArtifactMappings(artifactMappings);
				}

				//Update the field values
				UpdateFields(validationMessages, dataItem, incident, customProperties, artifactCustomProperty, projectId, incidentId, 0, fieldsToIgnore, workflowFields, workflowCustomProps);

				//Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
                //because there is no Comments field on the Incident entity
                //Only prompt if the status changed to avoid endless requests for a comment
                if (workflowFields != null && workflowFields.Any(w => w.Field.Name == "Resolution" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required) && currentStatusId != originalStatusId)
				{
					//Comment is required, so check that it's present
					if (String.IsNullOrWhiteSpace(dataItem.Fields["Resolution"].TextValue))
					{
						AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "Resolution", Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
					}
				}

				//Now verify the options for the custom properties to make sure all rules have been followed
				Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
				foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = customPropOptionMessage.Key;
					newMsg.Message = customPropOptionMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Perform any business level validations on the datarow
				Dictionary<string, string> businessMessages = incidentManager.Validate(incident);
				foreach (KeyValuePair<string, string> businessMessage in businessMessages)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = businessMessage.Key;
					newMsg.Message = businessMessage.Value;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//If we have validation messages, stop now
				if (validationMessages.Count > 0)
				{
					return validationMessages;
				}

				//Get copies of everything..
				Incident notificationArt = incident.Clone();
				ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

				//Either insert or update the incident
				if (incidentId < 1)
				{
					//Submit the new incident
					DateTime creationDate = DateTime.UtcNow;
                    List<int> componentIds = null;
                    if (!String.IsNullOrEmpty(incident.ComponentIds))
                    {
                        componentIds = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
                    }
					incidentId = incidentManager.Insert(
						projectId,
						incident.PriorityId,
						incident.SeverityId,
						incident.OpenerId,
						incident.OwnerId,
						null,
						incident.Name,
						incident.Description,
						incident.DetectedReleaseId,
						incident.ResolvedReleaseId,
						incident.VerifiedReleaseId,
						incident.IncidentTypeId,
						incident.IncidentStatusId,
						creationDate,
						incident.StartDate,
						incident.ClosedDate,
						incident.EstimatedEffort,
						incident.ActualEffort,
						incident.RemainingEffort,
						incident.BuildId,
                        componentIds,
						userId,
						true
						);

					//Now save the custom properties
					artifactCustomProperty.ArtifactId = incidentId;
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//Now move any attachments from the placeholder to the actual incident
					if (dataItem.Fields.ContainsKey("PlaceholderId") && dataItem.Fields["PlaceholderId"].IntValue.HasValue)
					{
						int placeholderId = dataItem.Fields["PlaceholderId"].IntValue.Value;
						AttachmentManager attachment = new AttachmentManager();
						attachment.Attachment_Move(userId, projectId, placeholderId, Artifact.ArtifactTypeEnum.Placeholder, incidentId, Artifact.ArtifactTypeEnum.Incident);
					}

                    //We need to encode the new artifact id as a 'pseudo' validation message
                    //We don't do this in the 'Save and New' case since it needs to load a new blank form
                    if (operation != "new")
                    {
                        ValidationMessage newMsg = new ValidationMessage();
                        newMsg.FieldName = "$NewArtifactId";
                        newMsg.Message = incidentId.ToString();
                        AddUniqueMessage(validationMessages, newMsg);
                    }

                    //Add the ID for notifications to work
                    ((Incident)notificationArt).MarkAsAdded();
                    ((Incident)notificationArt).IncidentId = incidentId;
				}
				else
				{
					try
					{
						incidentManager.Update(incident, userId, sendNotification: false);
					}
					catch (EntityForeignKeyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
					}
					catch (OptimisticConcurrencyException)
					{
						return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
					}
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				//See if we have a new comment (Resolution) encoded in the list of fields
				string notificationComment = null;
				if (dataItem.Fields.ContainsKey("Resolution") && incidentId > 0)
				{
					string newComment = dataItem.Fields["Resolution"].TextValue;

					if (!String.IsNullOrWhiteSpace(newComment))
					{
						incidentManager.InsertResolution(incidentId, newComment, DateTime.UtcNow, CurrentUserId.Value, false);
						notificationComment = newComment;
					}
				}

				//Send to Notification to see if we need to send anything out.
				if (notificationArt != null)
				{
					try
					{
						//Check Workflow Transitions, first.
						int numSent = new WorkflowManager().Workflow_NotifyStatusChange(notificationArt.IncidentId, workflowId, notificationArt);
						if (numSent < 1)
						{
							new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, notificationComment);
						}
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident.");
					}
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(String.Format(Resources.Messages.IncidentsService_IncidentNotFound, incidentId));
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the list of workflow field states separate from the main retrieve (used when changing workflow only)
		/// </summary>
		/// <param name="typeId">The id of the current incident type</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="stepId">The id of the current step/status</param>
		/// <returns>The list of workflow states only</returns>
		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			const string METHOD_NAME = "Form_RetrieveWorkflowFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                List<DataItemField> dataItemFields = new List<DataItemField>();

				//Get the list of artifact fields and custom properties
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Incident);
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);

				//Get the list of workflow fields and custom properties for the specified type and step
				WorkflowManager workflowManager = new WorkflowManager();
				int workflowId = workflowManager.Workflow_GetForIncidentType(typeId);
				List<WorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
				List<WorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

				//First the standard fields
				foreach (ArtifactField artifactField in artifactFields)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = artifactField.Name;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;
					if (workflowFields != null)
					{
						if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowFields.Any(w => w.Field.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				//Now the custom properties
				foreach (CustomProperty customProperty in customProperties)
				{
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = customProperty.CustomPropertyFieldName;
					dataItemFields.Add(dataItemField);

					//Set the workflow state
					//Specify which fields are editable or required
					dataItemField.Editable = true;
					dataItemField.Required = false;
					dataItemField.Hidden = false;

					//First see if the custom property is required due to its definition
					if (customProperty.Options != null)
					{
						CustomPropertyOptionValue customPropOptionValue = customProperty.Options.FirstOrDefault(co => co.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.AllowEmpty);
						if (customPropOptionValue != null)
						{
							bool? allowEmpty = customPropOptionValue.Value.FromDatabaseSerialization_Boolean();
							if (allowEmpty.HasValue)
							{
								dataItemField.Required = !allowEmpty.Value;
							}
						}
					}

					//Now check the workflow states
					if (workflowCustomProps != null)
					{
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
						{
							dataItemField.Editable = false;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
						{
							dataItemField.Required = true;
						}
						if (workflowCustomProps.Any(w => w.CustomProperty.CustomPropertyId == customProperty.CustomPropertyId && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
						{
							dataItemField.Hidden = true;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return dataItemFields;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region ICommentService Methods

		/// <summary>
		/// Retrieves the list of comments associated with an incident
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the incident</param>
		/// <returns>The list of comments</returns>
		public List<CommentItem> Comment_Retrieve(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Comment_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the new list of comments
				List<CommentItem> commentItems = new List<CommentItem>();

				IncidentManager incidentManager = new IncidentManager();
				UserManager userManager = new UserManager();
				Incident incident = incidentManager.RetrieveById(artifactId, true);

				//Make sure the user is either the owner or detector if limited permissions
				int ownerId = -1;
				if (incident.OwnerId.HasValue)
				{
					ownerId = incident.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && incident.OpenerId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we're sorting ascending or descending
				SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

				List<IncidentResolution> incidentResolutions;

				if (sortDirection == SortDirection.Ascending)
				{
					incidentResolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();
				}
				else
				{
					incidentResolutions = incident.Resolutions.OrderByDescending(r => r.CreationDate).ToList();
				}
				foreach (IncidentResolution incidentResolution in incidentResolutions)
				{
					//Add a new comment
					CommentItem commentItem = new CommentItem();
					commentItem.primaryKey = incidentResolution.IncidentResolutionId;
					commentItem.text = incidentResolution.Resolution;
					commentItem.creatorId = incidentResolution.CreatorId;
					commentItem.creatorName = incidentResolution.Creator.FullName;
					commentItem.creationDate = GlobalFunctions.LocalizeDate(incidentResolution.CreationDate);
					commentItem.creationDateText = GlobalFunctions.LocalizeDate(incidentResolution.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
					commentItem.sortDirection = (int)sortDirection;

					//Specify if the user can delete the item
					if (incidentResolution.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
					{
						commentItem.deleteable = true;
					}
					else
					{
						commentItem.deleteable = false;
					}

					commentItems.Add(commentItem);
				}

				//Return the comments
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return commentItems;
			}
			catch (ArtifactNotExistsException)
			{
				//The incident doesn't exist, so just return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the sort direction of the comments list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="sortDirectionId">The new direction for the sort</param>
		public void Comment_UpdateSortDirection(int projectId, int sortDirectionId)
		{
			const string METHOD_NAME = "Comment_UpdateSortDirection";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting
				SortDirection sortDirection = (SortDirection)sortDirectionId;
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a specific comment in the comment list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="commentId">The id of the comment</param>
		/// <param name="artifactId">The id of the incident</param>
		public void Comment_Delete(int projectId, int artifactId, int commentId)
		{
			const string METHOD_NAME = "Comment_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Delete the comment, making sure we have permissions
				IncidentManager incidentManager = new IncidentManager();
				IncidentResolution resolution = incidentManager.Resolution_RetrieveById(projectId, artifactId, commentId);
				if (resolution.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
				{
					incidentManager.Resolution_Delete(projectId, artifactId, commentId, userId);
				}
			}
			catch (ArtifactNotExistsException)
			{
				//The comment no longer exists so do nothing
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Adds a comment to an artifact
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="comment">The comment being added</param>
        /// <returns>The id of the newly added comment</returns>
        public int Comment_Add(int projectId, int artifactId, string comment)
        {
            const string METHOD_NAME = "Comment_Add";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view the item (limited access is OK)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Make sure we're allowed to add comments
            if (IsAuthorizedToAddComments(projectId) == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Add the incident resolution/comment
                string cleanedComment = GlobalFunctions.HtmlScrubInput(comment);
                IncidentManager incidentManager = new IncidentManager();
                int commentId = incidentManager.InsertResolution(artifactId, cleanedComment, DateTime.UtcNow, userId, true);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return commentId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region AssociationPanel Methods

        /// <summary>
        /// Creates a new incident from a task
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="artifactId">The id of the task</param>
        /// <param name="artifactTypeId">The type of artifact (e.g. task)</param>
        /// <param name="selectedItems">Any selected items (used in some cases)</param>
        /// <param name="folderId">The of a folder (not used for incidents)</param>
        /// <returns>Any error messages, or null string for success</returns>
        public string AssociationPanel_CreateNewLinkedItem(int projectId, int artifactId, int artifactTypeId, List<int> selectedItems, int? folderId)
        {
            const string METHOD_NAME = "AssociationPanel_CreateNewLinkedItem";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the first selected item only
                int? existingTaskId = null;
                if (selectedItems != null && selectedItems.Count > 0)
                {
                    existingTaskId = selectedItems[0];
                }

                //Create a new incident from this task
                IncidentManager incidentManager = new IncidentManager();
				int newIncidentId = 0;
                if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Task)
                {
					newIncidentId = incidentManager.Incident_CreateFromTask(artifactId, userId);
                }

				//Handle notifications
				if (newIncidentId > 0)
				{
					//Retrieve the new incident to pass to notification manager
					Incident notificationArt = incidentManager.RetrieveById(newIncidentId, false);
					((Incident)notificationArt).MarkAsAdded();
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					WorkflowManager workflowManager = new WorkflowManager();

					//Get the list of workflow fields and custom properties
					int workflowId;
					if (notificationArt.IncidentTypeId < 1)
					{
						workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).WorkflowId;
					}
					else
					{
						workflowId = workflowManager.Workflow_GetForIncidentType(notificationArt.IncidentTypeId);
					}

					try
					{
						//Check Workflow Transitions, first.
						int numSent = workflowManager.Workflow_NotifyStatusChange(notificationArt.IncidentId, workflowId, notificationArt);
						if (numSent < 1)
						{
							new NotificationManager().SendNotificationForArtifact(notificationArt);
						}
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident.");
					}
				}
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return "Unable to locate the task, it may have been deleted.";
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                return "Unable to create new incident, please check the server Event Log";
            }
            return "";
        }

        #endregion
    }
}
