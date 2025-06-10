using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the SortedGrid AJAX component for displaying/updating requirements in a sortable list format
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class RequirementsListService : SortedListServiceBase, IRequirementsListService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RequirementsListService::";

		protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_REQUIREMENT_PAGINATION_SIZE;

		/// <summary>
		/// Constructor
		/// </summary>
		public RequirementsListService()
		{
		}

		/// <summary>
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="requirementId">The id of the requirement to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int requirementId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Now retrieve the specific requirement - handle quietly if it doesn't exist
				try
				{
                    string tooltip = "";
                    RequirementView requirementView = requirementManager.RetrieveById2(projectId, requirementId);
                    if (String.IsNullOrEmpty(requirementView.Description))
                    {
                        //See if it has any parents that we need to display in the tooltip
                        if (requirementView.IndentLevel.Length > 3 && projectId.HasValue)
                        {
                            List<RequirementView> parents = requirementManager.RetrieveParents(User.UserInternal, projectId.Value, requirementView.IndentLevel);
                            foreach (RequirementView parent in parents)
                            {
                                tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "</u> &gt; ";
                            }
                        }
                        tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(requirementView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, requirementView.RequirementId, true);
                    }
                    else
                    {
                        //See if it has any parents that we need to display in the tooltip
                        if (requirementView.IndentLevel.Length > 3 && projectId.HasValue)
                        {
                            List<RequirementView> parents = requirementManager.RetrieveParents(User.UserInternal, projectId.Value, requirementView.IndentLevel);
                            foreach (RequirementView parent in parents)
                            {
                                tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "</u> &gt; ";
                            }
                        }

                        tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirementView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, requirementView.RequirementId, true) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirementView.Description);
                    }

                    //See if we have any comments to append
                    IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(requirementId, Artifact.ArtifactTypeEnum.Requirement, false);
                    if (comments.Count() > 0)
                    {
                        IDiscussion lastComment = comments.Last();
                        tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
                            GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
                            GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
							Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
                            );
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the requirement, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for requirement");
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
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldName, width);
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
		/// Changes the order of columns in the requirements list
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

				//Two of the columns provide the lookup name instead of the real column name
				if (fieldName == "CoverageCountTotal")
				{
					fieldName = "CoverageId";
				}
				if (fieldName == "TaskCount")
				{
					fieldName = "ProgressId";
				}

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Toggle the status of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldName, newPosition);
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
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldName);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Inserts a new requirement into the system
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifact">The type of artifact we're inserting</param>
        /// <param name="standardFilters">Any standard filters that are set by the page</param>
        /// <param name="displayTypeId">The location of the list we are on - to distinguish them from one another for display/filtering purposes</param>
        /// <returns>The id of the new requirement</returns>
        public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                if (artifact != "Requirement" && artifact != "ChildRequirement")
				{
					throw new NotImplementedException(Resources.Messages.RequirementsService_OnlySupportsRequirementInserts);
				}

				//For the sorted requirements list, they are just added at the end of the list, but always at the top level
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				int requirementId = requirementManager.Insert(userId, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId, true, true);

				//We now need to populate the appropriate default custom properties
				Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);
				if (requirement != null)
				{
					//If the artifact custom property row is null, create a new one and populate the defaults
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
						artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					//Start tracking changes
					requirement.StartTracking();

					//If we have filters currently applied to the view, then we need to set this new requirement to the same value
					//(if possible) so that it will show up in the list
					ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);
					if (filterList.Count > 0)
					{
                        //We need to tell it to ignore any filtering by the ID, creation date since we cannot set that on a new item
                        List<string> fieldsToIgnore = new List<string>() { "RequirementId", "CreationDate" };
                        if (filterList.ContainsKey("RequirementTypeId") && filterList["RequirementTypeId"] is MultiValueFilter)
                        {
                            MultiValueFilter mvf = (MultiValueFilter)filterList["RequirementTypeId"];
                            if (mvf.Values.Count == 1 && mvf.Values[0] == -1)
                            {
                                //Ignore a filter on  Epic
                                fieldsToIgnore.Add("RequirementTypeId");
                            }
                        }
                        UpdateToMatchFilters(projectId, filterList, requirementId, requirement, artifactCustomProperty, fieldsToIgnore);
						requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
					}

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				return requirementId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Deletes a set of requirements
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to delete</param>
        public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be deleted
				RequirementManager requirement = new RequirementManager();
				foreach (string itemValue in items)
				{
					//Get the requirement ID
					int requirementId = Int32.Parse(itemValue);
					requirement.MarkAsDeleted(userId, projectId, requirementId);
				}

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Handles custom list operations used by the requirements list screen, specifically creating test cases from requirements
		/// </summary>
		/// <param name="operation">
		/// The operation being executed:
		///     CreateTestCase - create a new test case from the requirement
		/// </param>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destId">The destination item id</param>
		/// <param name="items">The list of source items</param>
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
				if (operation == "CreateTestCase")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
					if (authorizationState == Project.AuthorizationState.Prohibited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in requirements and create new test cases from them
					//We don't actually use the destId
					TestCaseManager testCaseManager = new TestCaseManager();
					foreach (string item in items)
					{
						int requirementId = Int32.Parse(item);
						testCaseManager.CreateFromRequirement(userId, projectId, requirementId, null);
					}
				}
				else if (operation == "CreateTestSet")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestSet);
					if (authorizationState == Project.AuthorizationState.Prohibited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in requirements and create new test set from them
					//We don't actually use the destId
                    List<int> requirementIds = items.Select(r => Int32.Parse(r)).ToList();
					TestSetManager testSetManager = new TestSetManager();
                    testSetManager.CreateFromRequirements(userId, projectId, requirementIds);
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
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS;

            //Call the base method with the appropriate settings collection
            return UpdateSort(userId, projectId, sortProperty, sortAscending, sortSettingsCollection);
        }


        /// <summary>
        /// Exports a series of requirement (and their children) from one project to another
        /// </summary>
        /// <param name="items">The items to export</param>
        /// <param name="destProjectId">The project to export them to</param>
        public void SortedList_Export(int destProjectId, List<string> items)
        {
            const string METHOD_NAME = "SortedList_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				RequirementManager requirement = new Business.RequirementManager();
				//If a user has selected a summary item and some of its children, this will cause unexpected
				//behaviour - since some items will be doubly-exported. Keep a record of any selected summary items
				//and if we have any children selected, simply ignore them
				List<string> summaryRequirements = new List<string>();

				//Iterate through all the items to be deleted
				foreach (string item in items)
				{
					int requirementId = Int32.Parse(item);
					//Retrieve the item to check if a summary or not
					RequirementView selectedRequirement = requirement.RetrieveById(userId, null, requirementId);
					if (selectedRequirement.IsSummary)
					{
						//Actually perform the export
						requirement.Export(userId, selectedRequirement.ProjectId, requirementId, destProjectId);

						//Add the requirement to the list of selected items together with its indent level
						summaryRequirements.Add(selectedRequirement.IndentLevel);
					}
					else
					{
						//If we are the child of a selected item then don't indent
						bool match = false;
						for (int i = 0; i < summaryRequirements.Count; i++)
						{
							string summaryIndentLevel = (string)summaryRequirements[i];
							//Are we the child of a selected item that's already been exported
							if (SafeSubstring(selectedRequirement.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
							{
								match = true;
							}
						}
						if (!match)
						{
							requirement.Export(userId, selectedRequirement.ProjectId, requirementId, destProjectId);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

        /// <summary>
        /// Clones a set of requirements
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to clone</param>
        public void SortedList_Copy(int projectId, List<string> items)
        {
            const string METHOD_NAME = "SortedList_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we're authenticated
				if (!this.CurrentUserId.HasValue)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
				}
				int userId = this.CurrentUserId.Value;

				//Make sure we're authorized
				Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
				if (authorizationState == Project.AuthorizationState.Prohibited)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Iterate through all the items to be cloned and perform the operation
				RequirementManager requirement = new RequirementManager();
				foreach (string itemValue in items)
				{
					//Get the source ID
					int sourceId = Int32.Parse(itemValue);
					requirement.Copy(userId, projectId, sourceId, null);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
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
					paginationSettings["PaginationOption"] = pageSize;
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

		/// <summary>
		/// Updates records of data in the system
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItems">The updated data records</param>
		/// <returns>Validation messages</returns>
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            DateTime startDate = DateTime.UtcNow;

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Requirement);
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
                RequirementManager requirementManager = new RequirementManager();
				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the requirement id
					int requirementId = dataItem.PrimaryKey;

					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					if (requirement != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								requirement.ConcurrencyDate = concurrencyDateTimeValue;
								requirement.AcceptChanges();
							}
						}

						//Now we can start tracking any changes
						requirement.StartTracking();

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("CreationDate");
						fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise
						UpdateFields(validationMessages, dataItem, requirement, customProperties, artifactCustomProperty, projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldsToIgnore);

                        //Now verify the options for the custom properties to make sure all rules have been followed
                        Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
						foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
						{
							ValidationMessage newMsg = new ValidationMessage();
							newMsg.FieldName = customPropOptionMessage.Key;
							newMsg.Message = customPropOptionMessage.Value;
							AddUniqueMessage(validationMessages, newMsg);
						}

						//Make sure we have no validation messages before updating
						if (validationMessages.Count == 0)
						{
							//Get copies of everything..
							Artifact notificationArt = requirement.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database
							try
							{
								requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
							}
							catch (OptimisticConcurrencyException)
							{
								return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
							}
							customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                            //Call notifications..
                            try
                            {
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Requirement #" + requirement.RequirementId + ".");
							}
                        }
                    }
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return validationMessages;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
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
			return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, includeShared);
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

			return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Requirement, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, isShared, existingSavedFilterId, includeColumns);
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		/// <returns>Validation/error message (or empty string if none)</returns>
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

			//We need to change CoverageCountTotal to CoverageId if that's one of the filters
			if (filters.ContainsKey("CoverageCountTotal"))
			{
				string filterValue = filters["CoverageCountTotal"];
				filters.Remove("CoverageCountTotal");
				filters.Add("CoverageId", filterValue);
			}
			//We need to change TaskCount to ProgressId if that's one of the filters
			if (filters.ContainsKey("TaskCount"))
			{
				string filterValue = filters["TaskCount"];
				filters.Remove("TaskCount");
				filters.Add("ProgressId", filterValue);
			}
			return base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Requirement);
		}

        /// <summary>
        /// Returns the latest information on a single requirement in the system
        /// </summary>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
        {
            const string METHOD_NAME = "SortedList_Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the requirement and custom property business objects
                RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the requirement record for the specific requirement id
				RequirementView requirement = requirementManager.RetrieveById2(projectId, artifactId);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (requirement.OwnerId.HasValue)
				{
					ownerId = requirement.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && requirement.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);

				//Finally populate the dataitem from the dataset
				if (requirement != null)
				{
					//See if we already have an artifact custom property row
					if (artifactCustomProperty != null)
					{
						PopulateRow(dataItem, requirement, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
					}
					else
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false);
						PopulateRow(dataItem, requirement, customProperties, true, null);
					}

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("RequirementStatusId"))
					{
						dataItem.Fields["RequirementStatusId"].Editable = false;
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
		/// Populates a data item from a dataset datarow
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="requirementView">The requirement entity containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="workflowCustomProps">The custom properties workflow states</param>
		/// <param name="workflowFields">The standard fields workflow states</param>
		/// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		protected void PopulateRow(SortedDataItem dataItem, RequirementView requirementView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<RequirementWorkflowField> workflowFields = null, List<RequirementWorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = requirementView.RequirementId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, requirementView.ConcurrencyDate);

			//If non-summary, check to see if it's a use-case type
			if (!requirementView.IsSummary && requirementView.RequirementTypeIsSteps)
			{
				dataItem.Alternate = true;
			}

			//Specify if it has an attachment or not
			dataItem.Attachment = requirementView.IsAttachments;

			//Convert the workflow lists into the type expected by the ListServiceBase function
			List<WorkflowField> workflowFields2 = RequirementWorkflowManager.ConvertFields(workflowFields);
			List<WorkflowCustomProperty> workflowCustomProps2 = RequirementWorkflowManager.ConvertFields(workflowCustomProps);

            //The date and task effort fields are not editable for releases
            List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "EstimatedEffort", "TaskEstimatedEffort", "TaskProjectedEffort", "TaskRemainingEffort", "TaskActualEffort" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (requirementView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, requirementView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, workflowFields2, workflowCustomProps2, readOnlyFields);

					//Certain fields are not editable for summary items
					if (requirementView.IsSummary && (fieldName == "RequirementStatusId" || fieldName == "EstimatePoints" || fieldName == "RequirementTypeId"))
					{
						dataItemField.Editable = false;
					}
				}

                //If we have the name/desc field then we need to set the image to the appropriate artifact type
                //which is passed in the tooltip field
                if (dataItemField.FieldName == "Name")
                {
                    //The revision is a special type of 'pseudo artifact'
                    if (requirementView.IsSummary)
                    {
                        //Package/Epic
                        dataItemField.Tooltip = "artifact-RequirementSummary.svg";
                    }
                    else if (requirementView.RequirementTypeIsSteps)
                    {
                        //Use Case
                        dataItemField.Tooltip = "artifact-UseCase.svg";
                    }
                    else
                    {
                        //Requirement
                        dataItemField.Tooltip = "artifact-Requirement.svg";
                    }
                }

                //Apply the conditional formatting to the importance column (if displayed)
                if (dataItemField.FieldName == "ImportanceId" && requirementView.ImportanceId.HasValue)
				{
					dataItemField.CssClass = "#" + requirementView.ImportanceColor;
				}

                //If we have a package, display the type as such
                if (dataItemField.FieldName == "RequirementTypeId" && requirementView.IsSummary)
                {
                    dataItemField.IntValue = -1;
                    dataItemField.TextValue = Resources.Fields.Requirement_Type_Package;
                }
			}
		}

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
        /// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
		{
			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(DataModel.Artifact.ArtifactTypeEnum.Requirement, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);
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
        /// Returns a list of requirements in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the requirements as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="updatedRecordsOnly"> Do we want to only return recently updates records</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <param name="displayTypeId">The location of the list we are on - to distinguish them from one another for display/filtering purposes</param>
        /// <returns>Collection of dataitems</returns>
        public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the requirement and custom property business objects
                RequirementManager requirementManager = new RequirementManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items (including the first filter item)
                SortedData sortedData = new SortedData();
                List<SortedDataItem> dataItems = sortedData.Items;

                //Now get the list of populated filters and the current sort
                string filtersSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST;
                string sortSettingsCollection = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS;

                Hashtable filterList = GetProjectSettings(userId, projectId, filtersSettingsCollection);
                string sortCommand = GetProjectSetting(userId, projectId, sortSettingsCollection, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
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
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information
                ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
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

                //We need to replace a filter on Epic (RequirementTypeId = -1)
                //to the correct filter which is IsSummary = True
                if (filterList.ContainsKey("RequirementTypeId") && filterList["RequirementTypeId"] is MultiValueFilter)
                {
                    MultiValueFilter mvf = (MultiValueFilter)filterList["RequirementTypeId"];
                    if (mvf.Values.Count == 1 && mvf.Values[0] == -1)
                    {
                        filterList.Remove("RequirementTypeId");
                        filterList["IsSummary"] = true;
                    }
                }

                //Get the number of requirements in the project
                int artifactCount = requirementManager.Requirement_CountForSorted(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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
                List<RequirementView> requirements = requirementManager.Requirement_RetrieveSorted(projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), sortProperty, sortAscending);

                //Display the pagination information
                sortedData.CurrPage = currentPage;
                sortedData.PageCount = pageCount;
                sortedData.StartRow = startRow;

                //Display the visible and total count of artifacts
                sortedData.VisibleCount = requirements.Count;
                sortedData.TotalCount = artifactCount;

                //Display the sort information
                sortedData.SortProperty = sortProperty;
                sortedData.SortAscending = sortAscending;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false, true);

                //Iterate through all the requirements and populate the dataitem
                foreach (RequirementView requirement in requirements)
                {
                    //We clone the template item as the basis of all the new items
                    SortedDataItem dataItem = filterItem.Clone();

                    //Now populate with the data
                    PopulateRow(dataItem, requirement, customProperties, false, null);
                    dataItems.Add(dataItem);
                }

                //Also include the pagination info
                sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

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

        /// <summary>
        /// Used to populate the shape of the special compound fields used to display the information
        /// in the color-coded bar-chart 'equalizer' fields where different colors represent different values
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="dataItemField">The field whose shape we're populating</param>
        /// <param name="fieldName">The field name we're handling</param>
        /// <param name="filterList">The list of filters</param>
        /// <param name="projectId">The project we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
		{
			//Check to see if this is a field we can handle
			if (fieldName == "CoverageId")
			{
				dataItemField.FieldName = "CoverageCountTotal";
				string filterLookupName = fieldName;
				dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(filterLookupName))
				{
					dataItemField.IntValue = (int)filterList[filterLookupName];
				}
			}
			if (fieldName == "ProgressId")
			{
				dataItemField.FieldName = "TaskCount";
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
        /// Verifies the digital signature on a workflow status change if it is required
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="originalStatusId">The original status</param>
        /// <param name="currentStatusId">The new status</param>
        /// <param name="signature">The digital signature</param>
        /// <param name="creatorId">The creator of the requirement</param>
        /// <param name="ownerId">The owner of the requirement</param>
        /// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
        protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int creatorId, int? ownerId)
        {
            const string METHOD_NAME = "VerifyDigitalSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();
                RequirementWorkflowTransition workflowTransition = requirementWorkflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
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
                workflowTransition = requirementWorkflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
                if (workflowTransition.IsExecuteByCreator && creatorId == userId)
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

		/// <summary>
		/// Gets the list of lookup values and names for a specific lookup
		/// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
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
				RequirementManager requirementManager = new RequirementManager();
				ComponentManager componentManager = new ComponentManager();
				TaskManager taskManager = new TaskManager();
                ReleaseManager releaseManager = new ReleaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				if (lookupName == "RequirementStatusId")
				{
					List<RequirementStatus> statuses = requirementManager.RetrieveStatuses();
					lookupValues = ConvertLookupValues(statuses.OfType<DataModel.Entity>().ToList(), "RequirementStatusId", "Name");
				}
				if (lookupName == "RequirementTypeId")
				{
                    //For the sortable list version, we can filter on package, so include that
                    //Resort so that the Epic/Package is the first item (so do it by ID because package has an ID = -1)
					List<RequirementType> types = requirementManager.RequirementType_Retrieve(projectTemplateId, true, true);
					lookupValues = ConvertLookupValues(types.OrderBy(t => t.RequirementTypeId).OfType<DataModel.Entity>().ToList(), "RequirementTypeId", "Name");
				}
				if (lookupName == "ComponentId")
				{
					List<DataModel.Component> components = componentManager.Component_Retrieve(projectId);
					lookupValues = ConvertLookupValues(components.OfType<DataModel.Entity>().ToList(), "ComponentId", "Name");
				}
				if (lookupName == "AuthorId" || lookupName == "OwnerId")
				{
					List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "ImportanceId")
				{
					List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(importances.OfType<DataModel.Entity>().ToList(), "ImportanceId", "Name");
				}
				if (lookupName == "ReleaseId")
				{
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false, true);
                    lookupValues = ConvertLookupValues(releases.OfType<Entity>().ToList(), "ReleaseId", "FullName", "IndentLevel", "IsSummary", "IsIterationOrPhase", "IsActive");
                }
				if (lookupName == "CoverageId")
				{
					lookupValues = new JsonDictionaryOfStrings(requirementManager.RetrieveCoverageFiltersLookup());
				}
				if (lookupName == "ProgressId")
				{
					lookupValues = new JsonDictionaryOfStrings(taskManager.RetrieveProgressFiltersLookup());
				}

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, customPropertyNumber.Value, true);
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
		/// Populates the equalizer type graph for the requirements test coverage and task progress fields
		/// </summary>
		/// <param name="dataItemField">The field being populated</param>
		/// <param name="artifact">The artifact entity</param>
		protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
		{
			//Recast to the specific artifact entity
			RequirementView requirementView = (RequirementView)artifact;

			//See which equalizer we have
			if (dataItemField.FieldName == "CoverageCountTotal")
			{
				//Now lets correctly calculate the coverage information
				int totalCoverage = requirementView.CoverageCountTotal;
				int passedCoverage = requirementView.CoverageCountPassed;
				int failedCoverage = requirementView.CoverageCountFailed;
				int cautionCoverage = requirementView.CoverageCountCaution;
				int blockedCoverage = requirementView.CoverageCountBlocked;

				//Handle the not covered case
				if (totalCoverage == 0)
				{
					dataItemField.Tooltip = Resources.Fields.NotCovered;
					dataItemField.TextValue = Resources.Fields.NotCovered;
					dataItemField.CssClass = "NotCovered";
				}
				else
				{
					//Convert into percentages (use decimals and round function to avoid rounding errors)
					int percentPassed = (int)Decimal.Round(((decimal)passedCoverage * (decimal)100) / (decimal)totalCoverage, 0);
					int percentFailed = (int)Decimal.Round(((decimal)failedCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);
					int percentCaution = (int)Decimal.Round(((decimal)cautionCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);
					int percentBlocked = (int)Decimal.Round(((decimal)blockedCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);

					//Populate the equalizer percentages
					dataItemField.EqualizerGreen = percentPassed;
					dataItemField.EqualizerRed = percentFailed;
					dataItemField.EqualizerYellow = percentBlocked;
					dataItemField.EqualizerOrange = percentCaution;
					dataItemField.EqualizerGray = 100 - (percentPassed + percentFailed + percentCaution + percentBlocked);
					if (dataItemField.EqualizerGray < 0)
					{
						dataItemField.EqualizerGray = 0;
					}

					//Populate Tooltip
					dataItemField.TextValue = "";
					dataItemField.Tooltip = "# " + Resources.Fields.CoveringTests + "=" + totalCoverage + ", " + Resources.Fields.Passed + "=" + percentPassed + "%, " + Resources.Fields.Failed + "=" + percentFailed + "%, " + Resources.Fields.Caution + "=" + percentCaution + "%, " + Resources.Fields.Blocked + "=" + percentBlocked + "%";
				}
			}
			if (dataItemField.FieldName == "TaskCount")
			{
				//First see how many tasks we have
				int taskCount = requirementView.TaskCount;

				//Handle the no tasks case first
				if (taskCount == 0)
				{
					dataItemField.Tooltip = RequirementManager.GenerateTaskProgressTooltip(requirementView);
					dataItemField.TextValue = RequirementManager.GenerateTaskProgressTooltip(requirementView);
					dataItemField.CssClass = "NotCovered";
				}
				else
				{
					//Populate the percentages
					dataItemField.EqualizerGreen = (requirementView.TaskPercentOnTime < 0) ? 0 : requirementView.TaskPercentOnTime;
					dataItemField.EqualizerRed = (requirementView.TaskPercentLateFinish < 0) ? 0 : requirementView.TaskPercentLateFinish;
					dataItemField.EqualizerYellow = (requirementView.TaskPercentLateStart < 0) ? 0 : requirementView.TaskPercentLateStart;
					dataItemField.EqualizerGray = (requirementView.TaskPercentNotStart < 0) ? 0 : requirementView.TaskPercentNotStart;

					//Populate Tooltip
					dataItemField.TextValue = "";
					dataItemField.Tooltip = RequirementManager.GenerateTaskProgressTooltip(requirementView);
				}
			}
		}
    }
}
