using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Linq;
using System.Threading;
using System.Web.Security;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the HierarchicalGrid AJAX component for displaying/updating release data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ]
    public class ReleasesService : HierarchicalListServiceBase, IReleasesService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService::";

        protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_RELEASE_RELEASE_PAGINATION_SIZE;

        protected const string EXPANDED_KEY_UNASSIGNED = "Unassigned";

        /// <summary>
        /// Constructor
        /// </summary>
        public ReleasesService()
        {
        }

        #region WorkflowOperations Methods

        /// <summary>
        /// Retrieves the list of workflow operations for the current release
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="typeId">The release type</param>
        /// <param name="artifactId">The id of the release</param>
        /// <returns>The list of available workflow operations</returns>
        /// <remarks>Pass a specific type id if the user has changed the type of the release, but not saved it yet.</remarks>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the array of data items to store the workflow operations
                List<DataItem> dataItems = new List<DataItem>();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the list of available transitions for the current step in the workflow
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseWorkflowManager workflowManager = new ReleaseWorkflowManager();
                ReleaseView releaseView = releaseManager.RetrieveById(User.UserInternal, projectId, artifactId);
                int workflowId;
                if (typeId.HasValue)
                {
                    workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, typeId.Value);
                }
                else
                {
                    workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, releaseView.ReleaseTypeId);
                }

                //Get the current user's role
                int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

                //Determine if the current user is the creator or owner of the release
                bool isCreator = false;
                if (releaseView.CreatorId == CurrentUserId.Value)
                {
                    isCreator = true;
                }
                bool isOwner = false;
                if (releaseView.OwnerId.HasValue && releaseView.OwnerId.Value == CurrentUserId.Value)
                {
                    isOwner = true;
                }
                int statusId = releaseView.ReleaseStatusId;
                List<ReleaseWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isCreator, isOwner);

                //Populate the data items list
                foreach (ReleaseWorkflowTransition workflowTransition in workflowTransitions)
                {
                    //The data item itself
                    DataItem dataItem = new DataItem();
                    dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
                    dataItems.Add(dataItem);

                    //The WorkflowId field
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "WorkflowId";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)workflowTransition.ReleaseWorkflowId;
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
                    dataItemField.IntValue = (int)workflowTransition.InputReleaseStatusId;
                    dataItemField.TextValue = workflowTransition.InputReleaseStatus.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The OutputStatusId field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "OutputStatusId";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = (int)workflowTransition.OutputReleaseStatusId;
                    dataItemField.TextValue = workflowTransition.OutputReleaseStatus.Name;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The OutputStatusOpenYn field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "OutputStatusOpenYn";
                    dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
                    dataItemField.TextValue = (workflowTransition.OutputReleaseStatusId == (int)Release.ReleaseStatusEnum.Cancelled || workflowTransition.OutputReleaseStatusId == (int)Release.ReleaseStatusEnum.Closed || workflowTransition.OutputReleaseStatusId == (int)Release.ReleaseStatusEnum.Deferred) ? "N" : "Y";
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

        /// <summary>
        /// Creates a new release and returns it to the form
        /// </summary>
        /// <param name="artifactId">The id of the existing release we were on</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The id of the new release</returns>
        public override int? Form_New(int projectId, int artifactId)
        {
            const string METHOD_NAME = "Form_New";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to create the item
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                ReleaseManager releaseManager = new ReleaseManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Next we need to simply get the existing indent level and increment
                string newIndentLevel = null;
                Release release;
                try
                {
                    release = releaseManager.RetrieveById3(projectId, artifactId);
                    newIndentLevel = release.IndentLevel;
                    newIndentLevel = HierarchicalList.IncrementIndentLevel(newIndentLevel);
                }
                catch (ArtifactNotExistsException)
                {
                    //Ignore, leave indent level as null;
                }

                //Now we need to create the release and then navigate to it
                int releaseId = releaseManager.Insert(
                        userId,
                        projectId,
                        userId,
                        "",
                        null,
                        null,
                        newIndentLevel,
                        Release.ReleaseStatusEnum.Planned,
                        Release.ReleaseTypeEnum.MajorRelease,
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddMonths(1),
                        1,
                        0,
                        null,
                        false
                        );
                release = releaseManager.RetrieveById3(projectId, releaseId);

                //We now need to populate the appropriate default custom properties
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, true);
                if (release != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, releaseId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return releaseId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Next we need to simply get the existing indent level and increment
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseView release = releaseManager.RetrieveById2(projectId, artifactId);
                string newIndentLevel = release.IndentLevel;
                newIndentLevel = Business.HierarchicalList.IncrementIndentLevel(newIndentLevel);

                //Now we need to copy the release (in front of the current one) and then navigate to it
                int newReleaseId = releaseManager.Copy(userId, projectId, projectTemplateId, release.ReleaseId, release.ReleaseId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return newReleaseId;
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState != Project.AuthorizationState.Authorized)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                int? newReleaseId = null;
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseView release = releaseManager.RetrieveById2(null, artifactId);

                //Retrieve the list of peer releases and databind overall form
                string indentLevel = release.IndentLevel;
                List<ReleaseView> tempReleases = releaseManager.RetrievePeersChildrenAndParent(userId, projectId, indentLevel);

                //Look through the left hand navigation to see what is the next release on the list
                //If we are the last one on the list then we need to simply use the one before
                bool matchFound = false;
                int previousReleaseId = -1;
                foreach (ReleaseView releaseRow in tempReleases)
                {
                    int testReleaseId = releaseRow.ReleaseId;
                    if (testReleaseId == artifactId)
                    {
                        matchFound = true;
                    }
                    else
                    {
                        //If we found a match on the previous iteration, then we want to this (next) release
                        if (matchFound)
                        {
                            newReleaseId = testReleaseId;
                            break;
                        }

                        //If this matches the current release, set flag
                        if (testReleaseId == artifactId)
                        {
                            matchFound = true;
                        }
                        if (!matchFound)
                        {
                            previousReleaseId = testReleaseId;
                        }
                    }
                }
                if (!newReleaseId.HasValue && previousReleaseId != -1)
                {
                    newReleaseId = previousReleaseId;
                }

                //Next we need to delete the current release
                releaseManager.MarkAsDeleted(userId, projectId, artifactId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return newReleaseId;
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

        /// <summary>Returns a single release data record (all columns) for use by the FormManager control</summary>
        /// <param name="artifactId">The id of the current release</param>
        /// <returns>A release data item</returns>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                ReleaseManager releaseManager = new ReleaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                ReleaseWorkflowManager workflowManager = new ReleaseWorkflowManager();

                //Create the data item record (no filter items)
                HierarchicalDataItem dataItem = new HierarchicalDataItem();
                PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

                //Need to add the empty column to capture any new comments added
                if (!dataItem.Fields.ContainsKey("NewComment"))
                {
                    dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
                }

				//Add a column for the AvailablePoints, which is not in the Static Data
				dataItem.Fields.Add("AvailablePoints", new DataItemField() { FieldName = "AvailablePoints", Required = false, Editable = false, Hidden = false });

                //Get the release for the specific release id
                ReleaseView releaseView = releaseManager.RetrieveById2(projectId, artifactId.Value);

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.Release, true);

                //Make sure the user is authorized for this item
                int ownerId = -1;
                if (releaseView.OwnerId.HasValue)
                {
                    ownerId = releaseView.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && releaseView.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Get the list of workflow fields and custom properties
                int workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, releaseView.ReleaseTypeId);
                int statusId = releaseView.ReleaseStatusId;
                List<ReleaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
                List<ReleaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

                //See if we have any existing artifact custom properties for this row
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true, false);
                    PopulateRow(dataItem, releaseView, customProperties, true, (ArtifactCustomProperty)null, workflowFields, workflowCustomProps);
                }
                else
                {
                    PopulateRow(dataItem, releaseView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, workflowFields, workflowCustomProps);
                }

                //Also need to return back a special field to denote if the user is the owner or creator of the artifact
                bool isArtifactCreatorOrOwner = (ownerId == userId || releaseView.CreatorId == userId);
                dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

                //Populate any data mapping values are not part of the standard 'shape'
                if (artifactId.HasValue)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Release, artifactId.Value);
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

                    //Populate the releases hierarchy path as a special field
                    if (releaseView.IndentLevel.Length > 3)
                    {
                        List<ReleaseView> parents = releaseManager.RetrieveParents(projectId, releaseView.IndentLevel, false);
                        string pathArray = "[";
                        bool isFirst = true;
                        foreach (ReleaseView parent in parents)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                pathArray += ",";
                            }
                            pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "\", \"id\": " + parent.ReleaseId + " }";
                        }
                        pathArray += "]";
                        dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });
                    }
                    else
                    {
                        //send a blank folder path object back so client knows this artifact has folders
                        dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath" });
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

        /// <summary>Saves a single release data item</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="dataItem">The release to save</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            //Get the release id
            int releaseId = dataItem.PrimaryKey;

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                ReleaseManager releaseManager = new ReleaseManager();
                ReleaseWorkflowManager workflowManager = new ReleaseWorkflowManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Load the custom property definitions (once, not per artifact)
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

                //This service only supports updates, so we should get a release id that is valid

                //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                Release release = releaseManager.RetrieveById3(projectId, releaseId);

                //Make sure the user is authorized for this item if they only have limited permissions
                int ownerId = -1;
                if (release.OwnerId.HasValue)
                {
                    ownerId = release.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && release.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Create a new artifact custom property row if one doesn't already exist
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, false, customProperties);
                if (artifactCustomProperty == null)
                {
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, releaseId, customProperties);
                }
                else
                {
                    artifactCustomProperty.StartTracking();
                }

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["ReleaseStatusId"].IntValue.HasValue) ? dataItem.Fields["ReleaseStatusId"].IntValue.Value : -1;
				int originalStatusId = release.ReleaseStatusId;
				int releaseTypeId = (dataItem.Fields["ReleaseTypeId"].IntValue.HasValue) ? dataItem.Fields["ReleaseTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties
				int workflowId;
				if (releaseTypeId < 1)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).ReleaseWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, releaseTypeId);
				}
				List<ReleaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<ReleaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

				//Convert the workflow lists into the type expected by the ListServiceBase function
				List<WorkflowField> workflowFields2 = ReleaseWorkflowManager.ConvertFields(workflowFields);
				List<WorkflowCustomProperty> workflowCustomProps2 = ReleaseWorkflowManager.ConvertFields(workflowCustomProps);

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
							shouldVerifyDigitalSignature = release.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{

						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, release.CreatorId, release.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								release.SignatureMeaning = signature.Meaning;
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
                        release.ConcurrencyDate = concurrencyDateTimeValue;
                        release.AcceptChanges();
                    }
                }

                //Now we can start tracking any changes
                release.StartTracking();

                //Update the field values, tracking changes
                List<string> fieldsToIgnore = new List<string>();
                fieldsToIgnore.Add("NewComment");
                fieldsToIgnore.Add("Comments");
                fieldsToIgnore.Add("CreationDate");
                fieldsToIgnore.Add("PlannedEffort");
                fieldsToIgnore.Add("AvailableEffort");
                fieldsToIgnore.Add("LastUpdateDate");   //Breaks concurrency otherwise

                //Need to handle any data-mapping fields (project-admin only)
                if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
                {
                    DataMappingManager dataMappingManager = new DataMappingManager();
                    List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Release, releaseId);
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
                UpdateFields(validationMessages, dataItem, release, customProperties, artifactCustomProperty, projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, fieldsToIgnore, workflowFields2, workflowCustomProps2);

                //Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
                //because there is no Comments field on the Release entity
                if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "Comments" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                {
                    //Comment is required, so check that it's present
                    if (String.IsNullOrWhiteSpace(dataItem.Fields["NewComment"].TextValue))
                    {
                        AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "NewComment", Message = String.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
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

                //If we have validation messages, stop now
                if (validationMessages.Count > 0)
                {
                    return validationMessages;
                }

				//Clone the Release, CustomPropertyData..
				Artifact notificationArtifact = release.Clone();
				ArtifactCustomProperty notificationCustomProps = artifactCustomProperty.Clone();
				string notificationComment = null;

				//Update the release and any custom properties
				try
                {
                    releaseManager.Update(new List<Release>() { release }, userId, projectId);
					if (release.PeriodicReviewDate.HasValue)
					{
						releaseManager.CreatePeriodicReviewSchedule(release);
					}
                }
                catch (IterationSummaryException) //Cannot make an iteration a summary.
                {
                    return CreateSimpleValidationMessage(Resources.Messages.ReleaseDetails_CannotMakeSummaryReleaseIteration);
                }
                catch (StartEndDateException) //Start date and end date were out of order.
                {
                    return CreateSimpleValidationMessage(Resources.Messages.ReleaseDetails_CannotMakeStartDateAfterEndDate);
                }
                catch (ReleaseVersionNumberException) //Version number was not unique.
                {
                    return CreateSimpleValidationMessage(Resources.Messages.ReleaseDetails_VersionNumberInUse);
                }
                catch (EntityForeignKeyException) //Someone deleted the item while you were working.
                {
                    return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
                }
                catch (OptimisticConcurrencyException) //Someone else updated the sucker before you did.
                {
                    return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                }
                customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                //See if we have a new comment encoded in the list of fields
                string newComment = "";
                if (dataItem.Fields.ContainsKey("NewComment"))
                {
                    newComment = dataItem.Fields["NewComment"].TextValue;

                    if (!String.IsNullOrWhiteSpace(newComment))
                    {
                        new DiscussionManager().Insert(userId, releaseId, Artifact.ArtifactTypeEnum.Release, newComment, DateTime.UtcNow, projectId, false, false);

						//Save text for notification.
						notificationComment = newComment;
					}
                }

				//Send to Notification to see if we need to send anything out.
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArtifact, notificationCustomProps, notificationComment);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Release #" + release.ReleaseId + ".");
				}

				//If we're asked to save and create a new release, need to do the insert and send back the new id
				if (operation == "new")
                {
                    //Get the values from the existing release that we want to set on the new one (not status)
                    //Get the values from the existing release/iteration that we want to set on the new one
                    Release.ReleaseTypeEnum releaseType = (Release.ReleaseTypeEnum)release.ReleaseTypeId;

                    //Make this new iteration/release start just after the old one by default
                    DateTime startDate = release.EndDate.AddDays(1);
                    DateTime endDate = startDate.AddMonths(1);
                    decimal resourceCount = release.ResourceCount;
                    decimal daysNonWorking = release.DaysNonWorking;

                    //Next we need to simply get the existing indent level and increment
                    string newIndentLevel = release.IndentLevel;
                    newIndentLevel = Business.HierarchicalList.IncrementIndentLevel(newIndentLevel);

                    //Now we need to create a new release/iteration in the list and then navigate to it
                    int newReleaseId = releaseManager.Insert(
                        userId,
                        projectId,
                        userId,
                        "",
                        null,
                        null,
                        newIndentLevel,
                        Release.ReleaseStatusEnum.Planned,
                        releaseType,
                        startDate,
                        endDate,
                        resourceCount,
                        daysNonWorking,
                        null
                        );

                    //We need to populate any custom property default values
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, newReleaseId, customProperties);
                    artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

                    //We need to encode the new artifact id as a 'pseudo' validation message
                    ValidationMessage newMsg = new ValidationMessage();
                    newMsg.FieldName = "$NewArtifactId";
                    newMsg.Message = newReleaseId.ToString();
                    AddUniqueMessage(validationMessages, newMsg);
                }

                //Return back any messages. For success it should only contain a new artifact ID if we're inserting
                return validationMessages;
            }
            catch (ArtifactNotExistsException)
            {
                //Let the user know that the ticket no inter exists
                return CreateSimpleValidationMessage(String.Format(Resources.Messages.ReleasesService_ReleaseNotFound, releaseId));
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Verifies the digital signature on a workflow status change if it is required
        /// </summary>
        /// <param name="workflowId">The id of the workflow</param>
        /// <param name="originalStatusId">The original status</param>
        /// <param name="currentStatusId">The new status</param>
        /// <param name="signature">The digital signature</param>
        /// <param name="creatorId">The creator of the release</param>
        /// <param name="ownerId">The owner of the release</param>
        /// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
        protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int creatorId, int? ownerId)
        {
            const string METHOD_NAME = "VerifyDigitalSignature";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();
                ReleaseWorkflowTransition workflowTransition = releaseWorkflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
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
                workflowTransition = releaseWorkflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
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
        /// Returns the list of workflow field states separate from the main retrieve (used when changing workflow only)
        /// </summary>
        /// <param name="typeId">The id of the current release type</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                List<DataItemField> dataItemFields = new List<DataItemField>();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the list of artifact fields and custom properties
                ArtifactManager artifactManager = new ArtifactManager();
                List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Release);
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

                //Get the list of workflow fields and custom properties for the specified type and step
                ReleaseWorkflowManager workflowManager = new ReleaseWorkflowManager();
                int workflowId = workflowManager.Workflow_GetForReleaseType(projectTemplateId, typeId);
                List<ReleaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
                List<ReleaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

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
                        if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
                        {
                            dataItemField.Editable = false;
                        }
                        if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
                        {
                            dataItemField.Required = true;
                        }
                        if (workflowFields.Any(w => w.ArtifactField.Name == dataItemField.FieldName && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
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
        /// Retrieves the list of comments associated with a release
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactId">The id of the release</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Create the new list of comments
                List<CommentItem> commentItems = new List<CommentItem>();

                //Get the release (to verify permissions) and also the comments
                ReleaseManager releaseManager = new ReleaseManager();
                UserManager userManager = new UserManager();
                DiscussionManager discussion = new DiscussionManager();
                ReleaseView release = releaseManager.RetrieveById2(projectId, artifactId);
                List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.Release).ToList();

                //Make sure the user is the creator if limited permissions
                int ownerId = -1;
                if (release.OwnerId.HasValue)
                {
                    ownerId = release.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && release.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //See if we're sorting ascending or descending
                SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

                int startIndex;
                int increment;
                if (sortDirection == SortDirection.Ascending)
                {
                    startIndex = 0;
                    increment = 1;
                }
                else
                {
                    startIndex = comments.Count - 1;
                    increment = -1;
                }
                for (var i = startIndex; (increment == 1 && i < comments.Count) || (increment == -1 && i >= 0); i += increment)
                {
                    IDiscussion discussionRow = comments[i];
                    //Add a new comment
                    CommentItem commentItem = new CommentItem();
                    commentItem.primaryKey = discussionRow.DiscussionId;
                    commentItem.text = discussionRow.Text;
                    commentItem.creatorId = discussionRow.CreatorId;
                    commentItem.creatorName = discussionRow.CreatorName;
                    commentItem.creationDate = GlobalFunctions.LocalizeDate(discussionRow.CreationDate);
                    commentItem.creationDateText = GlobalFunctions.LocalizeDate(discussionRow.CreationDate).ToNiceString(GlobalFunctions.LocalizeDate(DateTime.UtcNow));
                    commentItem.sortDirection = (int)sortDirection;

                    //Specify if the user can delete the item
                    if (!discussionRow.IsPermanent && (discussionRow.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)))
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Update the setting
                SortDirection sortDirection = (SortDirection)sortDirectionId;
                SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
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
        /// <param name="artifactId">The id of the release</param>
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Delete the comment, making sure we have permissions
                DiscussionManager discussion = new DiscussionManager();
                IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.Release);
                //If the comment no longer exists do nothing
                if (comment != null && !comment.IsPermanent)
                {
                    if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
                    {
                        discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.Release);
                    }
                }
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
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
                //Add the comment
                string cleanedComment = GlobalFunctions.HtmlScrubInput(comment);
                DiscussionManager discussion = new DiscussionManager();
                int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.Release, cleanedComment, projectId, false, true);

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

        #region ReleasesService Methods

        /// <summary>
        /// Returns a list of releases as DOT notation for use in mind maps and other graphical forms
        /// </summary>
        /// <param name="numberOfLevels">The number of levels to show (null = all)</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The DOT notation form of the releases</returns>
        /// <remarks>
        /// Here's information on the DOT language:
        /// https://www.graphviz.org/pdf/dotguide.pdf
        /// </remarks>
        public string Release_RetrieveAsDotNotation(int projectId, int? numberOfLevels)
        {
            const string METHOD_NAME = "Release_RetrieveAsDotNotation";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            const int PAGE_SIZE = 50;

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the project (for its name)
                Project project = new ProjectManager().RetrieveById(projectId);

                //See if we need to update the stored settings
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS);
                bool dirty = false;
                int numberOfLevelsInt = (numberOfLevels.HasValue) ? numberOfLevels.Value : 0;
                if (!settings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL) || ((int)settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL]) != numberOfLevelsInt)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL] = numberOfLevelsInt;
                    dirty = true;
                }
                if (dirty)
                {
                    settings.Save();
                }

                //Get the hierarchy of releases based on the specified level and release
                ReleaseManager releaseManager = new ReleaseManager();
                int count = int.MaxValue;
                List<ReleaseView> releases = new List<ReleaseView>();

                //Loop through pages of Releases. Stop once the next page takes us over 5000.
                for (int startRow = 1; startRow < count; startRow += PAGE_SIZE)
                {
                    List<ReleaseView> releasesRange = releaseManager.Release_RetrieveForMindMap(projectId, numberOfLevels, out count, startRow, PAGE_SIZE);
                    if (releasesRange.Count + releases.Count <= 5000)
                        releases.AddRange(releasesRange);
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Hit 5000.");
                        break;
                    }
                }


                /*

                digraph releases_map{
                    rankdir=TB;
                    node [shape=box rx=5 ry=5 color=lightblue2 style=filled];
                    "Root" -> "RL:4";
                    "Root" -> "RL:5";
                    "Root" -> "RL:6";
                    "RL:1" -> "RL:19" [label="related to", arrowhead="undirected", style="stroke: #aaa; stroke-dasharray:2,2" labelStyle="fill: #aaa;"];
                }

                 */

                //Convert the releases into the dot notation
                string dotNotation =
    @"digraph releases_map {
    rankdir=TB;
    node [rx=5, ry=5, style=filled];
";
                string rootToken = "Root";
                //Add the root label
                dotNotation += string.Format("    \"{0}\" [label=\"{1}\", labelType = \"html\", style=\"{2}\"];\n", rootToken, HttpUtility.HtmlEncode(project.Name), "fill: #4949b7; color:white");
                //project

                foreach (ReleaseView release in releases)
                {
                    //Add the label
                    string link = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId, release.ReleaseId));
                    string style = "";
                    switch (release.ReleaseTypeId)
                    {
                        case (int)Release.ReleaseTypeEnum.MajorRelease:
                            {
                                style = "fill: #7686e5; font-weight: bold";
                                break;
                            }
                        case (int)Release.ReleaseTypeEnum.MinorRelease:
                            {
                                style = "fill: #7686e5; font-weight: normal";
                                break;
                            }
                        case (int)Release.ReleaseTypeEnum.Iteration:
                            {
                                style = "fill: #95b4d8; font-weight: normal";
                                break;
                            }
                        case (int)Release.ReleaseTypeEnum.Phase:
                            {
                                style = "fill: #95b4d8; font-weight: normal";
                                break;
                            }
                    }
                    dotNotation += string.Format("    \"{0}\" [label=\"<a href='{2}' data-release-id='{4}'>{1}</a>\", labelType = \"html\", style=\"{3}\"];\n", release.ArtifactToken, HttpUtility.HtmlEncode(release.Name) + " [" + release.ArtifactToken + "]", link, style, release.ReleaseId);

                    //Add the relationship
                    if (release.IndentLevel.Length > 3)
                    {
                        //Find the parent
                        ReleaseView parentRelease = releases.FirstOrDefault(r => r.IndentLevel == release.IndentLevel.SafeSubstring(0, release.IndentLevel.Length - 3));
                        if (parentRelease == null)
                        {
                            //Fallback to root
                            dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", rootToken, release.ArtifactToken);
                        }
                        else
                        {
                            dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", parentRelease.ArtifactToken, release.ArtifactToken);
                        }
                    }
                    else
                    {
                        dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", rootToken, release.ArtifactToken);
                    }
                }

                dotNotation += "}\n";

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return dotNotation;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

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
                    customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, fieldName);
                }
                else
                {
                    //Toggle the status of the appropriate field name
                    ArtifactManager artifactManager = new ArtifactManager();
                    artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, fieldName);
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

                //Get the correct field name (the equalizer bars use a different field name)
                string resolvedFieldName = ResolveFieldName(fieldName);

                //Change the width of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
                artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, resolvedFieldName, width);
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
        /// Changes the order of columns in the release list
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

                //Get the correct field name (the equalizer bars use a different field name)
                string resolvedFieldName = ResolveFieldName(fieldName);

                //Toggle the status of the appropriate artifact field or custom property
                ArtifactManager artifactManager = new ArtifactManager();
                artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, resolvedFieldName, newPosition);
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
        /// Returns the current hierarchy configuration for the current page
        /// </summary>
        /// <param name="userId">The user we're viewing the artifacts as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>a dictionary where key=artifactid, value=indentlevel</returns>
        public JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters)
        {
            //Authentication and authorization are done by the main Retrieve function

            //Get the full list of items for the current page
            List<HierarchicalDataItem> dataItems = this.HierarchicalList_Retrieve(projectId, standardFilters, false).Items;

            //Populate a dictionary with just the artifact ids and indent levels
            //as this will consume less bandwidth when retrieved by the client
            JsonDictionaryOfStrings hierarchyLevels = new JsonDictionaryOfStrings();
            for (int i = 1; i < dataItems.Count; i++)
            {
                hierarchyLevels.Add(dataItems[i].PrimaryKey.ToString(), dataItems[i].Indent);
            }
            return hierarchyLevels;
        }

        /// <summary>
        /// Returns a subset of the list of artifacts in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the artifacts as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="itemCount">The number of items to retrieve (-1 for all)</param>
        /// <param name="startIndex">The starting point relative to the current page</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        /// <remarks>Used when we only need to refresh part of the hierarchy</remarks>
        public HierarchicalData HierarchicalList_RetrieveSelection(int projectId, JsonDictionaryOfStrings standardFilters, int startIndex, int itemCount)
        {
            //Authentication/authorization is done in the main retrieve function

            //Get the full list of items for the current page
            HierarchicalData data = this.HierarchicalList_Retrieve(projectId, standardFilters, false);
            List<HierarchicalDataItem> dataItems = data.Items;

            //Return just the first row (header) and the subset requested
            HierarchicalData dataSubset = new HierarchicalData();
            List<HierarchicalDataItem> dataItemsSubset = dataSubset.Items;
            dataSubset.PageCount = data.PageCount;
            dataSubset.CurrPage = data.CurrPage;
            dataSubset.VisibleCount = data.VisibleCount;
            dataSubset.TotalCount = data.TotalCount;

            //First add the header row
            dataItemsSubset.Add(dataItems[0]);
            int endIndex = startIndex + itemCount;
            if (endIndex > dataItems.Count - 1 || itemCount == -1)
            {
                endIndex = dataItems.Count - 1;
            }
            //Now the data rows - add 1 to the start index since we don't need to return the header row
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                dataItemsSubset.Add(dataItems[i]);
            }
            return dataSubset;
        }

        /// <summary>
        /// Handles custom operations that are artifact/page-specific (buttons, drop-downs, etc.)
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="operation">The name of the operation</param>
        /// <param name="value">The parameter value being passed to the operation</param>
        /// <returns>Any error messages</returns>
        public override string CustomOperation(int projectId, string operation, string value)
        {
            const string METHOD_NAME = "CustomOperation";

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
                //See which operation we have and handle accordingly
                if (operation == "SelectPlanningRelease")
                {
                    //The value contains the id of the release we want to select
                    //We need to capture the release and put it in the project setting
                    if (value == "")
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                    else
                    {
                        int releaseId = Int32.Parse(value);
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                    }
                    //We also need to reset the iteration index back to zero
                    SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_STARTING_INDEX, 0);
                }
                if (operation == "IncludeTasks")
                {
                    //Do we want to include tasks
                    bool includeTasks;
                    if (Boolean.TryParse(value, out includeTasks))
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TASKS, includeTasks);
                    }
                }
                if (operation == "IncludeIncidents")
                {
                    //Do we want to include incidents
                    bool includeIncidents;
                    if (Boolean.TryParse(value, out includeIncidents))
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_INCIDENTS, includeIncidents);
                    }
                }
                if (operation == "IncludeTestCases")
                {
                    //Do we want to include test cases
                    bool includeTestCases;
                    if (Boolean.TryParse(value, out includeTestCases))
                    {
                        SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_ITERATION_PLAN_SELECTED_RELEASE, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TEST_CASES, includeTestCases);
                    }
                }
                return "";
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Handles custom list operations used by the releases list screen, specifically creating test sets from releases
        /// </summary>
        /// <param name="operation">
        /// The operation being executed:
        ///     CreateTestSet - create a new test set from the release
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
                if (operation == "CreateTestSet")
                {
                    //Make sure we're authorized
                    Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestSet);
                    if (authorizationState == Project.AuthorizationState.Prohibited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }

                    //Iterate through all the passed in releases and create new test set from them
                    //We don't actually use the destId
                    TestSetManager testSetManager = new TestSetManager();
                    foreach (string item in items)
                    {
                        int releaseId = Int32.Parse(item);
                        testSetManager.CreateFromRelease(projectId, releaseId, userId);
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

        /// <summary>
        /// Returns a list of release/iterations in the system for the specific user/project
        /// </summary>
        /// <param name="userId">The user we're viewing the release/iterations as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="updatedRecordsOnly"> Do we want to only return recently updates records</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>Collection of dataitems</returns>
        public HierarchicalData HierarchicalList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, bool updatedRecordsOnly)
        {
            const string METHOD_NAME = "HierarchicalList_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the release/iterations and custom property business objects
                ReleaseManager releaseManager = new ReleaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the array of data items (including the first filter item)
                HierarchicalData hierarchicalData = new HierarchicalData();
                List<HierarchicalDataItem> dataItems = hierarchicalData.Items;

                //Now get the list of populated filters
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST);
                hierarchicalData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Release);

                //Create the filter item first - we can clone it later
                HierarchicalDataItem filterItem = new HierarchicalDataItem();
                PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
                dataItems.Add(filterItem);

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

                //Now get the pagination information and add to the filter item
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

                //Get the number of releases in the project
                int artifactCount = releaseManager.Count(userId, projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                int unfilteredCount = releaseManager.Count(User.UserInternal, projectId, null, 0);

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the release list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(userId, projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

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
                hierarchicalData.CurrPage = currentPage;
                hierarchicalData.PageCount = pageCount;

                //Display the visible and total count of artifacts
                hierarchicalData.VisibleCount = releases.Count;
                hierarchicalData.TotalCount = unfilteredCount;

                //Now get the list of custom property options and lookup values for this artifact type / project
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true, false, true);

                //Iterate through all the releases and populate the dataitem
                foreach (ReleaseView release in releases)
                {
                    //See if we're only asked to get updated items (we have a 5-min buffer
                    if (!updatedRecordsOnly || release.LastUpdateDate > DateTime.UtcNow.AddMinutes(UPDATE_TIME_BUFFER_MINUTES))
                    {
                        //We clone the template item as the basis of all the new items
                        HierarchicalDataItem dataItem = filterItem.Clone();

                        //Now populate with the data
                        PopulateRow(dataItem, release, customProperties, false, null);
                        dataItems.Add(dataItem);
                    }
                }

                //If we're getting all items, also include the pagination info
                if (!updatedRecordsOnly)
                {
                    hierarchicalData.PaginationOptions = this.RetrievePaginationOptions(projectId);
                }

                Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return hierarchicalData;
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
        /// <param name="projectTemplateId">The project template</param>
        /// <param name="projectId">The project we're interested in</param>
        protected void PopulateEqualizerShape(string fieldName, DataItemField dataItemField, Hashtable filterList, int projectId, int projectTemplateId)
        {
            //Check to see if this is a field we can handle
            if (fieldName == "CoverageId")
            {
                dataItemField.FieldName = "CountPassed";
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
            if (fieldName == "CompletionId")
            {
                dataItemField.FieldName = "PercentComplete";
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
        /// Returns the resolved field name (for progress bars)
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The resolved field name</returns>
        protected static string ResolveFieldName(string fieldName)
        {
            string resolvedFieldName;
            switch (fieldName)
            {
                case "PercentComplete":
                    resolvedFieldName = "CompletionId";
                    break;

                case "TaskCount":
                    resolvedFieldName = "ProgressId";
                    break;

                case "CountPassed":
                    resolvedFieldName = "CoverageId";
                    break;

                default:
                    resolvedFieldName = fieldName;
                    break;
            }

            return resolvedFieldName;
        }

        /// <summary>
        /// Populates the equalizer type graph for the release test status field and other progress bars
        /// </summary>
        /// <param name="dataItem">The data item being populated</param>
        /// <param name="dataItemField">The field being populated</param>
        /// <param name="artifact">The data row</param>
        protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
        {
            //Recast to the specific artifact entity
            ReleaseView releaseRow = (ReleaseView)artifact;

            //See which equalizer we have
            if (dataItemField.FieldName == "CountPassed")
            {
                //Configure the equalizer bars for test status
                int passedCount = releaseRow.CountPassed;
                int failureCount = releaseRow.CountFailed;
                int cautionCount = releaseRow.CountCaution;
                int blockedCount = releaseRow.CountBlocked;
                int notRunCount = releaseRow.CountNotRun;
                int notApplicableCount = releaseRow.CountNotApplicable;

                //Calculate the percentages, handling rounding correctly
                //We don't include N/A ones in the total as they are either inactive or folders
                int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount;
                int percentPassed = 0;
                int percentFailed = 0;
                int percentCaution = 0;
                int percentBlocked = 0;
                int percentNotRun = 0;
                int percentNotApplicable = 0;
                if (totalCount == 0)
                {
                    dataItemField.TextValue = Resources.Fields.NoTests;
                    dataItemField.CssClass = "NotCovered";
                    dataItemField.Tooltip = Resources.Dialogs.ReleasesService_NoTestsMapped;
                }
                else
                {
                    //Need check to handle divide by zero case
                    percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                    percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                    percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                    percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);

                    //Specify the tooltip to be displayed
                    string tooltipText = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();
                    dataItemField.Tooltip = tooltipText;

                    //Now populate the equalizer graph
                    dataItemField.EqualizerGreen = percentPassed;
                    dataItemField.EqualizerRed = percentFailed;
                    dataItemField.EqualizerOrange = percentCaution;
                    dataItemField.EqualizerYellow = percentBlocked;
                    dataItemField.EqualizerGray = percentNotRun;
                }
            }
            if (dataItemField.FieldName == "TaskCount")
            {
                //First see how many tasks we have
                int taskCount = releaseRow.TaskCount;

                //Handle the no tasks case first
                if (taskCount == 0)
                {
                    dataItemField.Tooltip = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                    dataItemField.TextValue = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                    dataItemField.CssClass = "NotCovered";
                }
                else
                {
                    //Populate the percentages                    
                    dataItemField.EqualizerGreen = (releaseRow.TaskPercentOnTime < 0) ? 0 : releaseRow.TaskPercentOnTime;
                    dataItemField.EqualizerRed = (releaseRow.TaskPercentLateFinish < 0) ? 0 : releaseRow.TaskPercentLateFinish;
                    dataItemField.EqualizerYellow = (releaseRow.TaskPercentLateStart < 0) ? 0 : releaseRow.TaskPercentLateStart;
                    dataItemField.EqualizerGray = (releaseRow.TaskPercentNotStart < 0) ? 0 : releaseRow.TaskPercentNotStart;

                    //Populate Tooltip
                    dataItemField.TextValue = "";
                    dataItemField.Tooltip = ReleaseManager.GenerateTaskProgressTooltip(releaseRow);
                }
            }

            if (dataItemField.FieldName == "PercentComplete")
            {
                //First see how many requirements we have
                int requirementCount = releaseRow.RequirementCount;

                //Handle the no requirements case first
                if (requirementCount == 0)
                {
                    dataItemField.Tooltip = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                    dataItemField.TextValue = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                    dataItemField.CssClass = "NotCovered";
                }
                else
                {
                    //Populate the percentages
                    //It's always shown as green
                    dataItemField.EqualizerGreen = (releaseRow.PercentComplete < 0) ? 0 : releaseRow.PercentComplete;
                    dataItemField.EqualizerRed = 0;
                    dataItemField.EqualizerGray = (releaseRow.PercentComplete < 0) ? 100 : (100 - releaseRow.PercentComplete);

                    //Populate Tooltip
                    dataItemField.TextValue = "";
                    dataItemField.Tooltip = ReleaseManager.GenerateReqCompletionTooltip(releaseRow);
                }
            }
        }

        /// <summary>
        /// Populates a data item from a dataset datarow
        /// </summary>
        /// <param name="dataItem">The dataitem to be populated</param>
        /// <param name="releaseView">The datarow containing the data</param>
        /// <param name="customProperties">The list of custom property definitions and values</param>
        /// <param name="editable">Does the data need to be in editable form?</param>
        /// <param name="workflowCustomProps">The custom properties workflow states</param>
        /// <param name="workflowFields">The standard fields workflow states</param>
        /// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
        protected void PopulateRow(HierarchicalDataItem dataItem, ReleaseView releaseView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<ReleaseWorkflowField> workflowFields = null, List<ReleaseWorkflowCustomProperty> workflowCustomProps = null)
        {
            //Set the primary key and concurrency value
            dataItem.PrimaryKey = releaseView.ReleaseId;
            dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, releaseView.ConcurrencyDate);

            //Specify if this is a summary row and whether expanded or not
            dataItem.Summary = releaseView.IsSummary;
            dataItem.Expanded = releaseView.IsExpanded;

            //Specify if it has an attachment or not
            dataItem.Attachment = releaseView.IsAttachments;

            //Specify if it is an iteration or not
            if (!dataItem.Summary)
            {
                dataItem.Alternate = (releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase);
            }

            //Specify its indent position
            dataItem.Indent = releaseView.IndentLevel;

            //Convert the workflow lists into the type expected by the ListServiceBase function
            List<WorkflowField> workflowFields2 = ReleaseWorkflowManager.ConvertFields(workflowFields);
            List<WorkflowCustomProperty> workflowCustomProps2 = ReleaseWorkflowManager.ConvertFields(workflowCustomProps);

            //The date and task effort fields are not editable for releases
            List<string> readOnlyFields = new List<string>() { "CreationDate", "PlannedEffort", "AvailableEffort", "TaskEstimatedEffort", "TaskProjectedEffort", "TaskRemainingEffort", "TaskActualEffort", "RequirementCount", "RequirementPoints" };

            //Iterate through all the fields and get the corresponding values
            foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
            {
                string fieldName = dataItemFieldKVP.Key;
                DataItemField dataItemField = dataItemFieldKVP.Value;
                if (releaseView.ContainsProperty(dataItemField.FieldName))
                {
                    //First populate the data-item from the data-row
                    PopulateFieldRow(dataItem, dataItemField, releaseView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, workflowFields2, workflowCustomProps2, readOnlyFields);

                    //Apply the conditional formatting to the Start Date column (if displayed)
                    if (dataItemField.FieldName == "StartDate" && releaseView.StartDate < DateTime.UtcNow && releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned)
                    {
                        dataItemField.CssClass = "Warning";
                    }

                    //Apply the conditional formatting to the End Date column (if displayed)
                    if (dataItemField.FieldName == "EndDate" && releaseView.EndDate < DateTime.UtcNow && (releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || releaseView.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress))
                    {
                        dataItemField.CssClass = "Warning";
                    }
                }
            }
        }

        /// <summary>
        /// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="projectTemplateId">the project template we're using</param>
        /// <param name="userId">The user we're viewing the releases as</param>
        /// <param name="dataItem">The data item object that will be used as a template for the rows</param>
        /// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
        /// <param name="returnJustListFields">Should we just return list fields</param>
        protected void PopulateShape(int projectId, int projectTemplateId, int userId, HierarchicalDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
        {
            //We need to dynamically add the various columns from the field list
            LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
            AddDynamicColumns(DataModel.Artifact.ArtifactTypeEnum.Release, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);
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
                ReleaseManager release = new ReleaseManager();
                Business.UserManager user = new Business.UserManager();
                TaskManager task = new TaskManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                if (lookupName == "CreatorId" || lookupName == "OwnerId")
                {
                    List<DataModel.User> users = new UserManager().RetrieveActiveByProjectId(projectId);
                    lookupValues = ConvertLookupValues(users.OfType<DataModel.Entity>().ToList(), "UserId", "FullName");
                }
                if (lookupName == "ReleaseStatusId")
                {
                    List<ReleaseStatus> releaseStati = new ReleaseManager().RetrieveStatuses();
                    lookupValues = ConvertLookupValues(releaseStati.OfType<DataModel.Entity>().ToList(), "ReleaseStatusId", "Name");
                }
				if (lookupName == "PeriodicReviewAlertId")
				{
					List<PeriodicReviewAlertType> alertTypes = new ReleaseManager().RetrievePeriodicReviewAlertTypes();
					lookupValues = ConvertLookupValues(alertTypes.OfType<DataModel.Entity>().ToList(), "PeriodicReviewAlertId", "Name");
				}
				if (lookupName == "ReleaseTypeId")
                {
                    List<ReleaseType> releaseTypes = new ReleaseManager().RetrieveTypes();
                    lookupValues = ConvertLookupValues(releaseTypes.OfType<DataModel.Entity>().ToList(), "ReleaseTypeId", "Name");
                }
                if (lookupName == "CoverageId")
                {
                    lookupValues = new JsonDictionaryOfStrings(release.RetrieveCoverageFiltersLookup());
                }
                if (lookupName == "ProgressId")
                {
                    lookupValues = new JsonDictionaryOfStrings(task.RetrieveProgressFiltersLookup());
                }
                if (lookupName == "CompletionId")
                {
                    lookupValues = new JsonDictionaryOfStrings(new RequirementManager().RetrieveCompletionFiltersLookup());
                }

                //The custom property lookups
                int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
                if (customPropertyNumber.HasValue)
                {
                    CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.Release, customPropertyNumber.Value, true);
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
        /// Returns the latest information on a single release/iteration in the system
        /// </summary>
        /// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
        /// <param name="userId">The user we're viewing the release/iteration as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <returns>A single dataitem object</returns>
        public HierarchicalDataItem Refresh(int projectId, int artifactId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the release and custom property business objects
                ReleaseManager releaseManager = new ReleaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Create the data item record (no filter items)
                HierarchicalDataItem dataItem = new HierarchicalDataItem();
                PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

                //Get the release dataset record for the specific release id
                ReleaseView releaseView = releaseManager.RetrieveById2(projectId, artifactId);

                //Make sure the user is authorized for this item
                int ownerId = -1;
                if (releaseView.OwnerId.HasValue)
                {
                    ownerId = releaseView.OwnerId.Value;
                }
                if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && releaseView.CreatorId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //The main dataset does not have the custom properties, they need to be retrieved separately
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.Release, true);

                //Finally populate the dataitem from the dataset
                if (releaseView != null)
                {
                    //See if we already have an artifact custom property row
                    if (artifactCustomProperty != null)
                    {
                        PopulateRow(dataItem, releaseView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
                    }
                    else
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true, false);
                        PopulateRow(dataItem, releaseView, customProperties, true, null);
                    }

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("ReleaseStatusId"))
					{
						dataItem.Fields["ReleaseStatusId"].Editable = false;
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
        public List<ValidationMessage> HierarchicalList_Update(int projectId, List<HierarchicalDataItem> dataItems)
        {
            const string METHOD_NAME = "HierarchicalList_Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Release);
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
                ReleaseManager releaseManager = new ReleaseManager();
                //Load the custom property definitions (once, not per artifact)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);

                foreach (HierarchicalDataItem dataItem in dataItems)
                {
                    //Get the release id
                    int releaseId = dataItem.PrimaryKey;

                    //Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
                    Release release = releaseManager.RetrieveById3(projectId, releaseId);
                    ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, false, customProperties);

                    //Create a new artifact custom property row if one doesn't already exist
                    if (artifactCustomProperty == null)
                    {
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, releaseId, customProperties);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    if (release != null)
                    {
                        //Need to set the original date of this record to match the concurrency date
                        if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
                        {
                            DateTime concurrencyDateTimeValue;
                            if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
                            {
                                release.ConcurrencyDate = concurrencyDateTimeValue;
                                release.AcceptChanges();
                            }
                        }

                        //Now we can start tracking any changes
                        release.StartTracking();

                        //Update the field values
                        List<string> fieldsToIgnore = new List<string>();
                        fieldsToIgnore.Add("CreationDate");
                        fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise
                        UpdateFields(validationMessages, dataItem, release, customProperties, artifactCustomProperty, projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, fieldsToIgnore);

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
							Artifact notificationArt = release.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database
							try
                            {
                                releaseManager.Update(new List<Release>() { release }, userId, projectId);
                            }
                            catch (OptimisticConcurrencyException)
                            {
                                return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
                            }
                            catch (IterationSummaryException)
                            {
                                //Warn the user that you can't turn the release into an iteration as it's a summary item
                                return CreateSimpleValidationMessage(Resources.Messages.Releases_CannotTurnSummaryToIteration);
                            }
                            catch (StartEndDateException)
                            {
                                //Tell 'em their dates are backwards.
                                return CreateSimpleValidationMessage(Resources.Messages.ReleaseDetails_CannotMakeStartDateAfterEndDate);
                            }
                            catch (ReleaseVersionNumberException)
                            {
                                //Warn the user that you can't use a version number that's already in use in the project
                                return CreateSimpleValidationMessage(Resources.Messages.Releases_VersionNumberInUse);
                            }
                            customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

							//Call notifications..
							try
							{
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Release #" + release.ReleaseId + ".");
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
        /// Inserts a new release/iteration into the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="artifactId">The id of the existing artifact we're inserting in front of (-1 for none)</param>
        /// <param name="artifact">The type of artifact we're inserting (Release, Iteration)</param>
        /// <param name="standardFilters">Any standard filters that need to be set</param>
        /// <returns>The id of the new release/iteration</returns>
        public int HierarchicalList_Insert(int projectId, JsonDictionaryOfStrings standardFilters, int artifactId, string artifact)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business object(s)
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //See if we're inserting a release or iteration (default to a release)
                //Also see if we're inserting a child of an item or not
                Release.ReleaseTypeEnum releaseType = Release.ReleaseTypeEnum.MajorRelease;
                if (artifact == "Iteration" || artifact == "ChildIteration")
                {
                    releaseType = Release.ReleaseTypeEnum.Iteration;
                }

                //See if we have an existing release to insert before
                int releaseId = -1;
                if (artifactId == -1)
                {
                    //Simply insert the new item at the end
                    releaseId = releaseManager.Insert(
                        userId,
                        projectId,
                        userId,
                        "",
                        null,
                        null,
                        (int?)null,
                        Release.ReleaseStatusEnum.Planned,
                        releaseType,
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddMonths(1),
                        1,
                        0,
                        null,
                        false
                        );
                }
                else
                {
                    //See if we're inserting a child or normal release/iteration
                    if (artifact == "ChildRelease" || artifact == "ChildIteration")
                    {
                        //Insert under the specified child
                        try
                        {
                            releaseId = releaseManager.InsertChild(
                                userId,
                                projectId,
                                userId,
                                "",
                                null,
                                null,
                                artifactId,
                                Release.ReleaseStatusEnum.Planned,
                                releaseType,
                                DateTime.UtcNow,
                                DateTime.UtcNow.AddMonths(1),
                                1,
                                0,
                                null,
                                false
                                );
                        }
                        catch (IterationSummaryException)
                        {
                            //They tried to insert under an iteration
                            throw new DataValidationException(Resources.Messages.Releases_CannotInsertChildUnderIterationOrPhase);
                        }
                    }
                    else
                    {
                        //Insert at the specified position
                        releaseId = releaseManager.Insert(
                            userId,
                            projectId,
                            userId,
                            "",
                            null,
                            null,
                            artifactId,
                            Release.ReleaseStatusEnum.Planned,
                            releaseType,
                            DateTime.UtcNow,
                            DateTime.UtcNow.AddMonths(1),
                            1,
                            0,
                            null,
                            false
                            );
                    }
                }

                //We now need to populate the appropriate default custom properties
                Release release = releaseManager.RetrieveById3(projectId, releaseId);
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, true);
                if (release != null)
                {
                    //If the artifact custom property row is null, create a new one and populate the defaults
                    if (artifactCustomProperty == null)
                    {
                        List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, false);
                        artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Release, releaseId, customProperties);
                        artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }
                    else
                    {
                        artifactCustomProperty.StartTracking();
                    }

                    //If we have filters currently applied to the view, then we need to set this new release/iteration to the same value
                    //(if possible) so that it will show up in the list
                    ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST);
                    if (filterList.Count > 0)
                    {
                        List<string> fieldsToIgnore = new List<string>() { "VersionNumber" };
                        UpdateToMatchFilters(projectId, filterList, releaseId, release, artifactCustomProperty, fieldsToIgnore);
                        releaseManager.Update(new List<Release>() { release }, userId, projectId);
                    }

                    //Save the custom properties
                    customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
                }

                return releaseId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
        /// </summary>
        /// <param name="releaseId">The id of the release to get the data for</param>
        /// <returns>The name and description converted to plain-text</returns>
        public string RetrieveNameDesc(int? projectId, int releaseId, int? displayTypeId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the release business object
                ReleaseManager releaseManager = new ReleaseManager();

                //Now retrieve the specific release - handle quietly if it doesn't exist
                try
                {
                    ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);
                    string tooltip;
                    if (String.IsNullOrEmpty(release.Description))
                    {
                        tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName);
                    }
                    else
                    {
                        tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName) + "</u><br />\n" + GlobalFunctions.HtmlRenderAsPlainText(release.Description);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the release, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for release");
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
        /// Expands a release node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we're expanding</param>
        public void Expand(int projectId, int releaseId)
        {
            const string METHOD_NAME = "Expand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Expand the release
                Business.ReleaseManager release = new Business.ReleaseManager();
                release.Expand(userId, projectId, releaseId);
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
        /// Collapses a release node
        /// </summary>
        /// <param name="userId">The user making the change</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we're collapsing</param>
        public void Collapse(int projectId, int releaseId)
        {
            const string METHOD_NAME = "Collapse";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Collapse the release
                Business.ReleaseManager release = new Business.ReleaseManager();
                release.Collapse(userId, projectId, releaseId);
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
        /// Updates the filters stored in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="filters">The array of filters (name,value)</param>
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

            //We need to change CountPassed to CoverageId if that's one of the filters
            if (filters.ContainsKey("CountPassed"))
            {
                string filterValue = filters["CountPassed"];
                filters.Remove("CountPassed");
                filters.Add("CoverageId", filterValue);
            }
            //We need to change TaskCount to ProgressId if that's one of the filters
            if (filters.ContainsKey("TaskCount"))
            {
                string filterValue = filters["TaskCount"];
                filters.Remove("TaskCount");
                filters.Add("ProgressId", filterValue);
            }
            //We need to change PercentComplete to CompletionId if that's one of the filters
            if (filters.ContainsKey("PercentComplete"))
            {
                string filterValue = filters["PercentComplete"];
                filters.Remove("PercentComplete");
                filters.Add("CompletionId", filterValue);
            }
            bool expandAll = false;
            string result = base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Release, out expandAll);
            if (expandAll)
            {
                ExpandToLevel(projectId, -1, null);
            }
            return result;
        }

        /// <summary>
        /// Saves the current filters with the specified name
        /// </summary>
        /// <param name="includeColumns">Should we include the column selection</param>
        /// <param name="existingSavedFilterId">Populated if we're updating an existing saved filter</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="name">The name of the filter</param>
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

            return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Release, GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST, isShared, existingSavedFilterId, includeColumns);
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
            return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Release, includeShared);
        }

        /// <summary>
        /// Indents a set of releases/iterations
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to indent</param>
        public string Indent(int projectId, List<string> items)
        {
            const string METHOD_NAME = "Indent";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                //If a user has selected a summary item and some of its children, this will cause unexpected
                //behaviour - since some items will be doubly-indented. Keep a record of any selected summary items
                //and if we have any children selected, simply ignore them
                List<string> summaryReleases = new List<string>();

                //Iterate through all the items to be deleted
                foreach (string itemValue in items)
                {
                    //Get the release ID
                    int releaseId = Int32.Parse(itemValue);

                    //Retrieve the item to check if a summary or not
                    ReleaseView selectedRelease = releaseManager.RetrieveById(userId, projectId, releaseId);
                    if (selectedRelease.IsSummary && (selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease || selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MinorRelease))
                    {
                        //Actually perform the indent
                        releaseManager.Indent(userId, projectId, releaseId);

                        //Add the release to the list of selected items together with its indent level
                        //Need to do a fresh retrieve after the indent so that it matches on the next iteration
                        selectedRelease = releaseManager.RetrieveById(userId, projectId, releaseId);
                        summaryReleases.Add(selectedRelease.IndentLevel);
                    }
                    else
                    {
                        //If we are the child of a selected item then don't indent
                        bool match = false;
                        for (int i = 0; i < summaryReleases.Count; i++)
                        {
                            string summaryIndentLevel = (string)summaryReleases[i];
                            //Are we the child of a selected item that's already been indented
                            if (SafeSubstring(selectedRelease.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            try
                            {
                                releaseManager.Indent(userId, projectId, releaseId);
                            }
                            catch (IterationSummaryException)
                            {
                                //Warn the user that they can't indent an item below an iteration
                                return "Iterations cannot have child releases or iterations.";
                            }
                        }
                    }
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
        /// Outdents a set of releases/iterations
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to outdent</param>
        public string Outdent(int projectId, List<string> items)
        {
            const string METHOD_NAME = "Outdent";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                //If a user has selected a summary item and some of its children, this will cause unexpected
                //behaviour - since some items will be doubly-outdented. Keep a record of any selected summary items
                //and if we have any children selected, simply ignore them
                List<string> summaryReleases = new List<string>();
                for (int j = 0; j < items.Count; j++)
                {
                    //Get the release ID
                    int releaseId = Int32.Parse(items[j]);
                    //Retrieve the item to check if a summary or not
                    ReleaseView selectedRelease = releaseManager.RetrieveById(userId, projectId, releaseId);
                    if (selectedRelease.IsSummary && (selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease || selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MinorRelease))
                    {
                        //Add the release to the list of selected items together with its indent level
                        summaryReleases.Add(selectedRelease.IndentLevel);
                    }
                }

                //Iterate through all the items to be outdented
                //We need to iterate descending for Outdent since that will preserve the order
                for (int j = items.Count - 1; j >= 0; j--)
                {
                    //Get the release ID
                    int releaseId = Int32.Parse(items[j]);

                    //Retrieve the item to check if a summary or not
                    ReleaseView selectedRelease = releaseManager.RetrieveById(userId, projectId, releaseId);
                    if (selectedRelease.IsSummary && (selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease || selectedRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MinorRelease))
                    {
                        //Actually perform the outdent
                        releaseManager.Outdent(userId, projectId, releaseId);
                    }
                    else
                    {
                        //If we are the child of a selected item then don't outdent
                        bool match = false;
                        for (int i = 0; i < summaryReleases.Count; i++)
                        {
                            string summaryIndentLevel = (string)summaryReleases[i];
                            //Are we the child of a selected item that's already been outdented
                            if (SafeSubstring(selectedRelease.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            releaseManager.Outdent(userId, projectId, releaseId);
                        }
                    }
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
        /// Exports a series of releases/iterations (and their children) from one project to another
        /// </summary>
        /// <param name="userId">The user exporting the release</param>
        /// <param name="sourceProjectId">The project we're exporting from</param>
        /// <param name="items">The list of releases/iterations being exported</param>
        /// <param name="destProjectId">The project we're exporting to</param>
        public void Export(int sourceProjectId, int destProjectId, List<string> items)
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
            Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                ReleaseManager releaseManager = new Business.ReleaseManager();
                //If a user has selected a summary item and some of its children, this will cause unexpected
                //behaviour - since some items will be doubly-exported. Keep a record of any selected summary items
                //and if we have any children selected, simply ignore them
                List<string> summaryReleases = new List<string>();

                //Iterate through all the items to be deleted
                foreach (string item in items)
                {
                    int releaseId = Int32.Parse(item);
                    //Retrieve the item to check if a summary or not
                    ReleaseView release = releaseManager.RetrieveById(userId, sourceProjectId, releaseId);
                    if (release.IsSummary)
                    {
                        //Actually perform the export
                        releaseManager.Export(userId, sourceProjectId, releaseId, destProjectId);

                        //Add the release to the list of selected items together with its indent level
                        summaryReleases.Add(release.IndentLevel);
                    }
                    else
                    {
                        //If we are the child of a selected item then don't indent
                        bool match = false;
                        for (int i = 0; i < summaryReleases.Count; i++)
                        {
                            string summaryIndentLevel = (string)summaryReleases[i];
                            //Are we the child of a selected item that's already been exported
                            if (SafeSubstring(release.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            releaseManager.Export(userId, sourceProjectId, releaseId, destProjectId);
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
        /// Copies a set of releases/iterations
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to copy</param>
        /// <param name="destId">The destination item's id (or -1 for no destination selected)</param>
        public void Copy(int projectId, List<string> sourceItems, int destId)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Iterate through all the items to be copied and perform the operation
                ReleaseManager releaseManager = new ReleaseManager();
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceId = Int32.Parse(itemValue);
                    releaseManager.Copy(userId, projectId, projectTemplateId, sourceId, (destId > 0) ? (int?)destId : null);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Moves a release/iteration in the system
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceItems">The items to copy</param>
        /// <param name="destId">The destination item's id (or null for no destination selected)</param>
        public void Move(int projectId, List<string> sourceItems, int? destId)
        {
            const string METHOD_NAME = "Move";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be moved and perform the operation
                //Check to make sure we don't have any duplicates
                ReleaseManager release = new ReleaseManager();
                List<int> existingIds = new List<int>();
                foreach (string itemValue in sourceItems)
                {
                    //Get the source ID
                    int sourceId = Int32.Parse(itemValue);
                    if (!existingIds.Contains(sourceId))
                    {
                        release.Move(userId, projectId, sourceId, destId);
                        existingIds.Add(sourceId);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Deletes a set of releases/iterations
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="items">The items to delete</param>
        public void Delete(int projectId, List<string> items)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Iterate through all the items to be deleted
                ReleaseManager release = new ReleaseManager();
                foreach (string itemValue in items)
                {
                    //Get the release ID
                    int releaseId = Int32.Parse(itemValue);
                    release.MarkAsDeleted(userId, projectId, releaseId);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Expands the list of releases to a specific level
        /// </summary>
        /// <param name="userId">The user we're viewing as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="level">The number of levels to expand to (pass -1 for all levels)</param>
        /// <param name="standardFilters">Any standard filters</param>
        public void ExpandToLevel(int projectId, int level, JsonDictionaryOfStrings standardFilters)
        {
            const string METHOD_NAME = "ExpandToLevel";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Change the indentation level of the entire release list
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                if (level == -1)
                {
                    //Handle the 'all-levels' case
                    releaseManager.ExpandToLevel(userId, projectId, null);
                }
                else
                {
                    releaseManager.ExpandToLevel(userId, projectId, level);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #region IAssociationPanel methods

        /// <summary>
        /// Returns a plain-text version of the release name, description and all its successive parent releases
        /// Used in the mapping list box where you want to see the hierarchy
        /// </summary>
        /// <param name="releaseId">The id of the release to get the data for</param>
        /// <returns>The name, description and parent list converted to plain-text</returns>
        public string RetrieveNameFolder(int releaseId)
        {
            const string METHOD_NAME = "RetrieveNameFolder";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Instantiate the release business object
                ReleaseManager releaseManager = new ReleaseManager();

                //Now retrieve the specific release - handle quietly if it doesn't exist
                try
                {
                    //First we need to get the release itself
                    ReleaseView release = releaseManager.RetrieveById2(null, releaseId);

                    //Next we need to get the list of successive parent folders
                    List<ReleaseView> parentReleases = releaseManager.RetrieveParents(release.ProjectId, release.IndentLevel, false);
                    string tooltip = "";
                    foreach (ReleaseView releaseRow in parentReleases)
                    {
                        tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(releaseRow.FullName) + "</u> &gt; ";
                    }

                    //Now we need to get the release itself
                    tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(release.FullName) + "</u>";
                    if (!String.IsNullOrEmpty(release.Description))
                    {
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(release.Description);
                    }

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();
                    return tooltip;
                }
                catch (ArtifactNotExistsException)
                {
                    //This is the case where the client still displays the release, but it has already been deleted on the server
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for release");
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

        public string AssociationPanel_CreateNewLinkedItem(int projectId, int artifactId, int artifactTypeId, List<int> selectedItems, int? folderId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INavigationService Methods

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
        /// Returns a list of releases/iterations for display in the navigation bar
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="indentLevel">The indent level of the parent folder, or empty string for all items</param>
        /// <returns>List of releases/iterations</returns>
        /// <param name="displayMode">
        /// The display mode of the navigation list:
        /// 1 = Filtered List
        /// 2 = All Items (no filters)
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
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Release);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the release business object
                ReleaseManager releaseManager = new ReleaseManager();

                //Create the array of data items
                List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

                //Now get the list of populated filters if appropriate
                Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST);

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
                //Get the number of releases in the project
                int artifactCount = releaseManager.Count(userId, projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

                //**** Now we need to actually populate the rows of data to be returned ****

                //Get the releases list dataset for the user/project
                int startRow = ((currentPage - 1) * paginationSize) + 1;
                if (startRow > artifactCount)
                {
                    startRow = 1;
                }
                List<ReleaseView> releases;
                if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems)
                {
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    //All Items
                    releases = releaseManager.RetrieveByProjectId(userId, projectId, startRow, paginationSize, null, 0);
                }
                else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.ActiveOnly)
                {
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    //Active Items, show all rows, ignore pagination
                    Hashtable activeFilter = new Hashtable();
                    MultiValueFilter mvf = new MultiValueFilter();
                    mvf.Values.Add((int)Release.ReleaseStatusEnum.Planned);
                    mvf.Values.Add((int)Release.ReleaseStatusEnum.InProgress);
                    mvf.Values.Add((int)Release.ReleaseStatusEnum.Completed);
                    activeFilter.Add("ReleaseStatusId", mvf);
                    releases = releaseManager.RetrieveByProjectId(userId, projectId, 1, Int32.MaxValue, activeFilter, 0);
                }
                else
                {
                    if (authorizationState == Project.AuthorizationState.Limited)
                    {
                        throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                    }
                    //Filtered List
                    releases = releaseManager.RetrieveByProjectId(userId, projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
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

                //Iterate through all the releases and populate the dataitem (only some columns are needed)
                foreach (ReleaseView release in releases)
                {
                    //If we are passed a parent indent level, only return child items
                    if (String.IsNullOrEmpty(indentLevel) || (release.IndentLevel.Length > indentLevel.Length && release.IndentLevel.Substring(0, indentLevel.Length) == indentLevel))
                    {
                        //Create the data-item
                        HierarchicalDataItem dataItem = new HierarchicalDataItem();

                        //Populate the necessary fields
                        dataItem.PrimaryKey = release.ReleaseId;
                        dataItem.Indent = release.IndentLevel;
                        dataItem.Expanded = release.IsExpanded;

                        //Name/Desc
                        DataItemField dataItemField = new DataItemField();
                        dataItemField.FieldName = "Name";
                        dataItemField.TextValue = release.Name;
                        dataItem.Summary = release.IsSummary;
                        dataItem.Alternate = release.IsIterationOrPhase;
                        dataItem.Fields.Add("Name", dataItemField);

                        //Add to the items collection
                        dataItems.Add(dataItem);
                    }
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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_RELEASE_GENERAL_SETTINGS);
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
    }
}
