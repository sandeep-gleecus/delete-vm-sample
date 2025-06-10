using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the HierarchicalGrid AJAX component for displaying/updating requirements data
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class RequirementsService : HierarchicalListServiceBase, IRequirementsService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService::";

		protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_REQUIREMENT_PAGINATION_SIZE;

		/// <summary>
		/// Constructor
		/// </summary>
		public RequirementsService()
		{
		}

		#region IFormService methods

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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				RequirementManager requirementManager = new RequirementManager();
				RequirementView tempRequirement = requirementManager.RetrieveById(userId, null, artifactId);
				string indentLevel = tempRequirement.IndentLevel;
				List<RequirementView> requirements = requirementManager.RetrievePeersChildrenAndParent(userId, projectId, indentLevel);

				int? newRequirementId = null;
				//Look through the dataset to see what is the next requirement on the list
				//If we are the last one on the list then we need to simply use the one before
				bool matchFound = false;
				int previousRequirementId = -1;
				foreach (RequirementView requirementItem in requirements)
				{
					int testRequirementId = requirementItem.RequirementId;
					if (testRequirementId == artifactId)
					{
						matchFound = true;
					}
					else
					{
						//If we found a match on the previous iteration, then we want to this (next) requirement
						if (matchFound)
						{
							newRequirementId = testRequirementId;
							break;
						}

						//If this matches the current requirement, set flag
						if (testRequirementId == artifactId)
						{
							matchFound = true;
						}
						if (!matchFound)
						{
							previousRequirementId = testRequirementId;
						}
					}
				}
				if (!newRequirementId.HasValue && previousRequirementId != -1)
				{
					newRequirementId = previousRequirementId;
				}

				//Next we need to delete the current requirement
				requirementManager.MarkAsDeleted(userId, projectId, artifactId);

				return newRequirementId;
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
		/// Creates a new requirement and returns it to the form
		/// </summary>
		/// <param name="artifactId">Thd if of the existing requirement we were on</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new requirement</returns>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the existing requirement and get its position to insert after
				RequirementManager requirementManager = new RequirementManager();

				//Next we need to simply get the existing indent level and increment
				string newIndentLevel = null;
				RequirementView requirement;
				try
				{
					requirement = requirementManager.RetrieveById2(projectId, artifactId);
					newIndentLevel = requirement.IndentLevel;
					newIndentLevel = HierarchicalList.IncrementIndentLevel(newIndentLevel);
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore, leave indent level as null;
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the requirement and then navigate to it
				int requirementId = requirementManager.Insert(userId, projectId, null, null, newIndentLevel, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
				requirement = requirementManager.RetrieveById2(projectId, requirementId);

				//We now need to populate the appropriate default custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
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

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Creates a new requirement, that's a child of requirement we are currently on and returns the new requirement to the form
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">Thd id of the existing requirement we were on - this is to become the new parent</param>
		/// <returns>The id of the new requirement</returns>
		public int? Form_InsertChild(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_InsertChild";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the existing requirement to verify it exists in this project
				RequirementManager requirementManager = new RequirementManager();
				RequirementView requirement;
				try
				{
					requirement = requirementManager.RetrieveById2(projectId, artifactId);
				}
				catch (ArtifactNotExistsException)
				{
					throw new System.ServiceModel.FaultException("The requirement to insert a child into does not exist");
				}

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the requirement and then navigate to it
				int requirementId = requirementManager.InsertChild(userId, projectId, null, null, artifactId, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
				requirement = requirementManager.RetrieveById2(projectId, requirementId);

				//We now need to populate the appropriate default custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
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

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//First retrieve the requirement
				RequirementManager requirementManager = new RequirementManager();
				RequirementView requirement = requirementManager.RetrieveById2(projectId, artifactId);

				//Next we need to simply get the existing indent level and increment
				string newIndentLevel = requirement.IndentLevel;
				newIndentLevel = Business.HierarchicalList.IncrementIndentLevel(newIndentLevel);

				//Now we need to copy the requirement (in front of the current one) and then navigate to it
				int newRequirementId = requirementManager.Copy(userId, projectId, requirement.RequirementId, requirement.RequirementId);

				return newRequirementId;
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

		/// <summary>Returns a single requirement data record (all columns) for use by the FormManager control</summary>
		/// <param name="projectId">The id of the current projet</param>
		/// <param name="artifactId">The id of the current requirement - null if creating a new requirement</param>
		/// <returns>A requirement data item</returns>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the business classes
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the data item record (no filter items)
				HierarchicalDataItem dataItem = new HierarchicalDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

				//Need to add the empty column to capture any new comments added
				if (!dataItem.Fields.ContainsKey("NewComment"))
				{
					dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
				}

				//Get the requirement for the specific requirement id or just create a temporary one
				RequirementView requirementView = null;
				ArtifactCustomProperty artifactCustomProperty = null;
				int ownerId = -1;
				if (artifactId.HasValue)
				{
					requirementView = requirementManager.RetrieveById2(projectId, artifactId.Value);

					//The main dataset does not have the custom properties, they need to be retrieved separately
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);

					//Make sure the user is authorized for this item
					if (requirementView.OwnerId.HasValue)
					{
						ownerId = requirementView.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && requirementView.AuthorId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
				}
				else
				{
					//Insert Case, need to create the new requirement
					requirementView = requirementManager.Requirement_New(projectId, userId, null);

					//Also need to add a _ParentRequirementId field that's used on saving the form on planning boards when adding a new requirement to an Epic
					if (!dataItem.Fields.ContainsKey("_ParentRequirementId"))
					{
						dataItem.Fields.Add("_ParentRequirementId", new DataItemField() { FieldName = "_ParentRequirementId" });
					}

					//Also we need to populate any default Artifact Custom Properties
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false);
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}

				//Get the list of workflow fields and custom properties, handle the special package type
				int workflowId;
				if (requirementView.RequirementTypeId < 0 || requirementView.IsSummary)
				{
					//For packages, always use the default workflow
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RequirementWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRequirementType(requirementView.RequirementTypeId);
				}
				int statusId = requirementView.RequirementStatusId;
				List<RequirementWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
				List<RequirementWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

				//See if we have any existing artifact custom properties for this row
				if (artifactCustomProperty == null)
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false);
					PopulateRow(dataItem, requirementView, customProperties, true, (ArtifactCustomProperty)null, workflowFields, workflowCustomProps);
				}
				else
				{
					PopulateRow(dataItem, requirementView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, workflowFields, workflowCustomProps);
				}

				//The New Comments field is not part of the entity so needs to be handled separately for workflow
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					DataItemField newCommentField = dataItem.Fields["NewComment"];
					if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
					{
						newCommentField.Required = true;
					}
					if (workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden))
					{
						newCommentField.Hidden = true;
					}
					if (!workflowFields.Any(f => f.ArtifactField.Name == "Comments" && f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive))
					{
						newCommentField.Editable = true;
					}
				}

				//Also need to return back a special field to denote if the user is the owner or creator of the artifact
				bool isArtifactCreatorOrOwner = (ownerId == userId || requirementView.AuthorId == userId);
				dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

				//Populate any data mapping values are not part of the standard 'shape'
				if (artifactId.HasValue)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Requirement, artifactId.Value);
					foreach (DataSyncArtifactMapping artifactMapping in artifactMappings)
					{
						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = DataMappingManager.FIELD_PREPEND + artifactMapping.DataSyncSystemId;
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Text;
						if (string.IsNullOrEmpty(artifactMapping.ExternalKey))
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

					//Populate the requirements hierarchy path as a special field
					if (requirementView.IndentLevel.Length > 3)
					{
						List<RequirementView> parents = requirementManager.RetrieveParents(User.UserInternal, projectId, requirementView.IndentLevel);
						string pathArray = "[";
						bool isFirst = true;
						foreach (RequirementView parent in parents)
						{
							if (isFirst)
							{
								isFirst = false;
							}
							else
							{
								pathArray += ",";
							}
							pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "\", \"id\": " + parent.RequirementId + " }";
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

				//If the requirement should have a new parent, send info about this requirement to the client (so the client can send back to server in Form_Save)
				//if (newParentArtifactId.HasValue)
				//{
				//	//Retrieve the requirement
				//	Requirement newParentRequirement = requirementManager.RetrieveById3(projectId, newParentArtifactId.Value);
				//	if (newParentRequirement != null && newParentRequirement.IsSummary && !newParentRequirement.IsDeleted)
				//	{
				//		//Add its id to as a field to the dataItem
				//		dataItem.Fields.Add("_ParentRequirementId", new DataItemField() { FieldName = "_ParentRequirementId", IntValue = newParentRequirement.RequirementId });
				//	}
				//}

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

		/// <summary>Saves a single requirement data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The requirement to save</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the requirement id
			int requirementId = dataItem.PrimaryKey;

			try
			{
				//Instantiate the business classes
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);

				//If we have a zero/negative primary key it means that it's actually a new item being inserted
				RequirementView requirementView;
				ArtifactCustomProperty artifactCustomProperty;
				if (requirementId < 1)
				{
					//Insert Case, need to use the Requirement_New() method since we need the mandatory fields populated
					requirementView = requirementManager.Requirement_New(projectId, userId, null);

					//Also we need to populate any default Artifact Custom Properties
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}
				else
				{
					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					requirementView = requirementManager.RetrieveById2(projectId, requirementId);

					//Make sure the user is authorized for this item if they only have limited permissions
					int ownerId = -1;
					if (requirementView.OwnerId.HasValue)
					{
						ownerId = requirementView.OwnerId.Value;
					}
					if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && requirementView.AuthorId != userId)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Create a new artifact custom property row if one doesn't already exist
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}
				}

				//Convert from the read-only view to the read/write entity
				Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["RequirementStatusId"].IntValue.HasValue) ? dataItem.Fields["RequirementStatusId"].IntValue.Value : -1;
				int originalStatusId = requirement.RequirementStatusId;
				int requirementTypeId = (dataItem.Fields["RequirementTypeId"].IntValue.HasValue) ? dataItem.Fields["RequirementTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties, handle the special package type
				int workflowId;
				if (requirementTypeId < 1 || requirement.IsSummary)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RequirementWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRequirementType(requirementTypeId);
				}
				List<RequirementWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<RequirementWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

				//Convert the workflow lists into the type expected by the ListServiceBase function
				List<WorkflowField> workflowFields2 = RequirementWorkflowManager.ConvertFields(workflowFields);
				List<WorkflowCustomProperty> workflowCustomProps2 = RequirementWorkflowManager.ConvertFields(workflowCustomProps);

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
							shouldVerifyDigitalSignature = requirement.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{
						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, requirement.AuthorId, requirement.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								requirement.SignatureMeaning = signature.Meaning;
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
				if (!string.IsNullOrEmpty(dataItem.ConcurrencyValue))
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

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				fieldsToIgnore.Add("NewComment");
				fieldsToIgnore.Add("Comments");
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise

				//Need to handle any data-mapping fields (project-admin only)
				if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId);
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
									if (string.IsNullOrWhiteSpace(dataItemField.TextValue))
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
				UpdateFields(validationMessages, dataItem, requirement, customProperties, artifactCustomProperty, projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, fieldsToIgnore, workflowFields2, workflowCustomProps2);

				//Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
				//because there is no Comments field on the Requirement entity
				if (workflowFields != null && workflowFields.Any(w => w.ArtifactField.Name == "Comments" && w.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Required))
				{
					//Comment is required, so check that it's present
					if (string.IsNullOrWhiteSpace(dataItem.Fields["NewComment"].TextValue))
					{
						AddUniqueMessage(validationMessages, new ValidationMessage() { FieldName = "NewComment", Message = string.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Comment) });
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

				//Clone the Requirement, CustomPropertyData..
				Artifact notificationArtifact = requirement.Clone();
				ArtifactCustomProperty notificationCustomProps = artifactCustomProperty.Clone();
				string notificationComment = null;

				//Either insert or update the requirement
				if (requirementId < 1)
				{
					//Handle enums
					Requirement.RequirementStatusEnum requirementStatus = (Requirement.RequirementStatusEnum)requirement.RequirementStatusId;

					//Check if the requirement should be added as a child of a specific parent
					if (dataItem.Fields.ContainsKey("_ParentRequirementId") && dataItem.Fields["_ParentRequirementId"].IntValue.HasValue)
					{
						//Submit the new requirement as a child of a specific requirement
						requirementId = requirementManager.InsertChild(
							userId,
							projectId,
							requirement.ReleaseId,
							requirement.ComponentId,
							dataItem.Fields["_ParentRequirementId"].IntValue.Value, //add as child to passed in parent
							requirementStatus,
							requirement.RequirementTypeId,
							requirement.AuthorId,
							requirement.OwnerId,
							requirement.ImportanceId,
							requirement.Name,
							requirement.Description,
							requirement.EstimatePoints,
							userId
							);
					}
					else
					{
						//Submit the new requirement at the end of the list
						requirementId = requirementManager.Insert(
							userId,
							projectId,
							requirement.ReleaseId,
							requirement.ComponentId,
							(int?)null, //add to end of list
							requirementStatus,
							requirement.RequirementTypeId,
							requirement.AuthorId,
							requirement.OwnerId,
							requirement.ImportanceId,
							requirement.Name,
							requirement.Description,
							requirement.EstimatePoints,
							userId
							);
					}

					//Now save the custom properties
					artifactCustomProperty.ArtifactId = requirementId;
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We don't need to worry about attachments for new requirements added through the planning board

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = requirementId.ToString();
					AddUniqueMessage(validationMessages, newMsg);

					//Add the ID for notifications to work (in the planning board insert case only)
					((Requirement)notificationArtifact).MarkAsAdded();
					((Requirement)notificationArtifact).RequirementId = requirementId;
				}
				else
				{
					//Update the requirement and any custom properties
					try
					{
						requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
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

				//See if we have a new comment encoded in the list of fields
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					string newComment = dataItem.Fields["NewComment"].TextValue;

					if (!string.IsNullOrWhiteSpace(newComment) && !string.IsNullOrWhiteSpace(newComment.ToLowerInvariant().Replace("<br>", "").Replace("<br/>", "").Replace("<br />", "")))
					{
						try
						{
							new DiscussionManager().Insert(userId, requirementId, Artifact.ArtifactTypeEnum.Requirement, newComment, DateTime.UtcNow, projectId, false, false);

							//Save text for notification.
							notificationComment = newComment;
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD_NAME, ex, "Trying to save comment for Requirement #" + requirement.RequirementId + ".");
							return CreateSimpleValidationMessage(Resources.Messages.Global_DataSavedWithoutNewComment);
						}
					}
				}

				//Send to Notification to see if we need to send anything out.
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArtifact, notificationCustomProps, notificationComment);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Requirement #" + requirement.RequirementId + ".");
				}

				//If we're asked to save and create a new requirement, need to do the insert and send back the new id
				if (operation == "new")
				{
					//Get the values from the existing requirement that we want to set on the new one (not status)

					//Next we need to simply get the existing indent level and increment
					string newIndentLevel = requirement.IndentLevel;
					newIndentLevel = HierarchicalList.IncrementIndentLevel(newIndentLevel);

					//Now we need to create a new requirement in the list and then navigate to it
					//If the existing type is a package, need to make the new requirement default since package is only
					//used for summary items
					int newRequirementId = requirementManager.Insert(
						userId,
						projectId,
						requirement.ReleaseId,
						requirement.ComponentId,
						newIndentLevel,
						Requirement.RequirementStatusEnum.Requested,
						(requirement.RequirementTypeId == Requirement.REQUIREMENT_TYPE_PACKAGE) ? null : (int?)requirement.RequirementTypeId,
						userId,
						requirementView.OwnerId,
						requirement.ImportanceId,
						"",
						null,
						null,
						userId
						);

					//We need to populate any custom property default values
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, newRequirementId, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = newRequirementId.ToString();
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(string.Format(Resources.Messages.RequirementsService_RequirementNotFound, requirementId));
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
		/// <param name="typeId">The id of the current requirement type</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
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
				List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.Requirement);
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);

				//Get the list of workflow fields and custom properties, handle the special package type
				RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();
				int workflowId;
				if (typeId < 0)
				{
					//For packages, always use the default workflow
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RequirementWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRequirementType(typeId);
				}

				//Get the list of workflow fields and custom properties for the specified type and step
				List<RequirementWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
				List<RequirementWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

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
		/// Retrieves the list of comments associated with a requirement
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the requirement</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the new list of comments
				List<CommentItem> commentItems = new List<CommentItem>();

				//Get the requirement (to verify permissions) and also the comments
				RequirementManager requirementManager = new RequirementManager();
				UserManager userManager = new UserManager();
				DiscussionManager discussion = new DiscussionManager();
				RequirementView requirementView = requirementManager.RetrieveById(userId, projectId, artifactId);
				List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.Requirement).ToList();

				//Make sure the user is either the owner or author if limited permissions
				int ownerId = -1;
				if (requirementView.OwnerId.HasValue)
				{
					ownerId = requirementView.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && requirementView.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we're sorting ascending or descending
				SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting
				SortDirection sortDirection = (SortDirection)sortDirectionId;
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
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
		/// <param name="artifactId">The id of the requirement</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Delete the comment, making sure we have permissions
				DiscussionManager discussion = new DiscussionManager();
				IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.Requirement);
				//If the comment no longer exists do nothing
				if (comment != null && !comment.IsPermanent)
				{
					if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
					{
						discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.Requirement);
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
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
				int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.Requirement, cleanedComment, projectId, false, true);

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

		#region IHierarchicalDocument Methods

		/// <summary>
		/// Returns a document of requirements in the system for the specific user/project
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="numberRows">The number of rows to retrieve</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="parentRequirementId">The requirement to focus on along with all its children, as per pagination (null = no parent requirement filter)</param>
		/// <returns>Collection of dataitems</returns>
		/// <remarks>
		/// It only retrieves the fields necessary for displaying the requirements in a simple document format
		/// </remarks>
		public HierarchicalData HierarchicalDocument_Retrieve(int projectId, int startRow, int numberRows, int? parentRequirementId = null)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the requirement and custom property business objects
				RequirementManager requirementManager = new RequirementManager();
				RequirementStepService requirementStepService = new RequirementStepService();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the array of data items (including the first filter item)
				HierarchicalData hierarchicalData = new HierarchicalData();
				List<HierarchicalDataItem> dataItems = hierarchicalData.Items;

				//Create the filter item first - we don't return it, just use it as a template for the real rows
				HierarchicalDataItem filterItem = new HierarchicalDataItem();
				HierarchicalDocument_PopulateShape(projectId, projectTemplateId, userId, filterItem);

				//**** Now we need to actually populate the rows of data to be returned ****

				//Verify the requirement is in this project and is a summary
				if (parentRequirementId.HasValue)
				{
					//If it doesn't exist we get an exception
					Requirement parentRequirement = requirementManager.RetrieveById3(projectId, parentRequirementId.Value, true);
					//If the requirement is no longer a summary or has been deleted pick the first epic in the product
					if (parentRequirement == null || !parentRequirement.IsSummary || parentRequirement.IsDeleted)
					{
						parentRequirementId = null;
					}
				}

				//Get the requirements list dataset for the user/project
				int totalCount;
				List<RequirementView> requirements = new List<RequirementView>();
				if (parentRequirementId.HasValue)
				{
					requirements = requirementManager.Requirement_RetrieveForMindMap(projectId, null, out totalCount, startRow, numberRows, parentRequirementId);
				}
				else
				{
					requirements = requirementManager.Requirement_RetrieveForMindMap(projectId, 1, out totalCount, startRow, numberRows, null);
				}

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Retrieved " + requirements.Count + " requirements.");
				int visibleCount = requirements.Count;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false, true);


				//Iterate through all the requirements and populate the dataitem
				foreach (RequirementView requirement in requirements)
				{
					//We clone the template item as the basis of all the new items
					HierarchicalDataItem dataItem = filterItem.Clone();

					//Now populate with the data
					PopulateRow(dataItem, requirement, customProperties, false, null);

					//Add in any description / tooltip for a custom property here
					//We do not do it in PopulateRow to keep this functionality limited to this method / view
					//Iterate through all the fields and get the corresponding values
					foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
					{
						string fieldName = dataItemFieldKVP.Key;
						DataItemField dataItemField = dataItemFieldKVP.Value;
						int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(dataItemField.FieldName);
						if (customPropertyNumber.HasValue)
						{
							//Get the custom property definition
							if (customProperties != null)
							{
								CustomProperty customProperty = customProperties.Find(cp => cp.PropertyNumber == customPropertyNumber.Value);
								if (customProperty != null)
								{
									//Add the description / tooltip
									dataItemField.Tooltip = customProperty.Description;
								}
							}
						}
					}

					//Add the data item
					dataItems.Add(dataItem);

					//If we have a user case, add the diagram code
					if (requirement.RequirementTypeIsSteps)
					{
						string dotNotation = requirementStepService.RequirementStep_RetrieveAsDotNotation(projectId, requirement.RequirementId);
						if (!string.IsNullOrEmpty(dotNotation))
						{
							DataItemField diagramField = new DataItemField();
							diagramField.FieldName = "_Diagram";
							diagramField.TextValue = dotNotation;
							dataItem.Fields.Add(diagramField.FieldName, diagramField);
						}
					}
				}

				//Display the visible and total count of artifacts
				hierarchicalData.VisibleCount = visibleCount;
				hierarchicalData.TotalCount = totalCount;

				//Get the project collection, then update the specific key and save
				ProjectSettingsCollection documentSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW);
				//If we have a parentRequirementId save it to the settings collection
				if (parentRequirementId.HasValue)
				{
					documentSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_PARENT_REQUIREMENT_ID] = parentRequirementId.Value;
				}
				else
				{
					documentSettings.Remove(GlobalFunctions.PROJECT_SETTINGS_KEY_PARENT_REQUIREMENT_ID);
				}
				documentSettings.Save();

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
		/// Saves a signle requirement from the requirement hierarchical document view for the specific user/project
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItem">The requirement dataitem</returns>
		/// <remarks>
		/// [based off Form_Save method] It only saves the fields that the HierarchicalDocument_Retrieve services returns
		/// </remarks>
		public List<ValidationMessage> HierarchicalDocument_Save(int projectId, HierarchicalDataItem dataItem)
		{
			const string METHOD_NAME = "HierarchicalDocument_Save";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (bulk edit required)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			//Get the requirement id
			int requirementId = dataItem.PrimaryKey;

			try
			{
				//Instantiate the requirement and custom property business objects
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);

				//If we have a zero/negative primary key it means that it's actually a new item being inserted
				RequirementView requirementView;
				ArtifactCustomProperty artifactCustomProperty;
				if (requirementId < 1)
				{
					//Insert Case, need to use the Requirement_New() method since we need the mandatory fields populated
					requirementView = requirementManager.Requirement_New(projectId, userId, null);

					//Also we need to populate any default Artifact Custom Properties
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, -1, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
				}
				else
				{
					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					requirementView = requirementManager.RetrieveById2(projectId, requirementId);

					//Create a new artifact custom property row if one doesn't already exist
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, false, customProperties);
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}
				}

				//Convert from the read-only view to the read/write entity
				Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();

				//Need to set the original date of this record to match the concurrency date
				//The value is already in UTC so no need to convert
				if (!string.IsNullOrEmpty(dataItem.ConcurrencyValue))
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

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise

				//Update the field values
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

				//If we have validation messages, stop now
				if (validationMessages.Count > 0)
				{
					return validationMessages;
				}

				//Clone the Requirement, CustomPropertyData..
				Artifact notificationArtifact = requirement.Clone();
				ArtifactCustomProperty notificationCustomProps = artifactCustomProperty.Clone();
				string notificationComment = null;

				//Either insert or update the requirement
				if (requirementId < 1)
				{
					//Handle enums
					Requirement.RequirementStatusEnum requirementStatus = (Requirement.RequirementStatusEnum)requirement.RequirementStatusId;

					//Submit the new requirement at the end of the list
					requirementId = requirementManager.Insert(
						userId,
						projectId,
						null,
						null,
						(int?)null, //Simply add to end of list for now
						requirementStatus,
						requirement.RequirementTypeId,
						userId, //this will be done on the document list page with no option to set the author, so use the current user
						requirement.OwnerId,
						requirement.ImportanceId,
						requirement.Name,
						requirement.Description,
						null,
						userId
						);

					//Now save the custom properties
					artifactCustomProperty.ArtifactId = requirementId;
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We don't need to worry about attachments for new requirements added through the planning board

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = requirementId.ToString();
					AddUniqueMessage(validationMessages, newMsg);

					//Add the ID for notifications to work (in the planning board insert case only)
					((Requirement)notificationArtifact).MarkAsAdded();
					((Requirement)notificationArtifact).RequirementId = requirementId;
				}
				else
				{
					//Update the requirement and any custom properties
					try
					{
						requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });

						//We need to encode the concurrency as a 'pseudo' validation message
						Requirement requirementUpdated = requirementManager.RetrieveById3(projectId, requirement.RequirementId);
						ValidationMessage newMsg = new ValidationMessage();
						newMsg.FieldName = "$newConcurrency";
						newMsg.Message = string.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, requirementUpdated.ConcurrencyDate);
						AddUniqueMessage(validationMessages, newMsg);
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

				//Send to Notification to see if we need to send anything out.
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArtifact, notificationCustomProps, notificationComment);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Requirement #" + requirement.RequirementId + ".");
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(string.Format(Resources.Messages.RequirementsService_RequirementNotFound, requirementId));
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds/removes a field from the list of fields displayed in the requirement document view
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="fieldName">The name of the field to display/hide</param>
		public void HierarchicalDocument_ToggleFieldVisibility(int projectId, string fieldName)
		{
			const string METHOD_NAME = "HierarchicalDocument_ToggleFieldVisibility";

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
				//Get the project collection
				ProjectSettingsCollection documentSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW);
				//Toggle the collection entry - if it exists remove it, if it does not exist add it with a 1 value 
				if (documentSettings.ContainsKey(fieldName))
				{
					documentSettings.Remove(fieldName);
				}
				else
				{
					documentSettings.Add(fieldName, 1);
				}
				documentSettings.Save();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		protected void HierarchicalDocument_PopulateShape(int projectId, int projectTemplateId, int userId, HierarchicalDataItem dataItem)
		{
			//Get the project collection
			ProjectSettingsCollection documentSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW);
			//If the collection is empty add the entries for the default fields
			if (documentSettings.Count == 0)
			{
				//Add the defaults fields to show
				documentSettings.Add("Description", 1);
				documentSettings.Add("ImportanceId", 1);
				documentSettings.Add("RequirementStatusId", 1);
				documentSettings.Add("RequirementTypeId", 1);
				documentSettings.Add("OwnerId", 1);
				documentSettings.Save();
			}

			//The columns in this method are always fixed, since we are displaying a simple requirements document only
			//Requirement Name
			DataItemField dataItemField = new DataItemField();
			dataItemField.FieldName = "Name";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.Requirement;
			dataItemField.Editable = true;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

			//Full Description
			string descriptionFieldName = "Description";
			if (documentSettings.ContainsKey(descriptionFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = descriptionFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
				dataItemField.Caption = Resources.Fields.Description;
				dataItemField.Editable = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Importance
			string importanceFieldName = "ImportanceId";
			if (documentSettings.ContainsKey(importanceFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = importanceFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ImportanceName";
				dataItemField.Caption = Resources.Fields.Importance;
				dataItemField.Editable = false;
				dataItemField.AllowDragAndDrop = true;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Status
			string statusFieldName = "RequirementStatusId";
			if (documentSettings.ContainsKey(statusFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = statusFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "RequirementStatusName";
				dataItemField.Caption = Resources.Fields.RequirementStatusId;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Type
			string typeFieldName = "RequirementTypeId";
			if (documentSettings.ContainsKey(typeFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = typeFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "RequirementTypeName";
				dataItemField.Caption = Resources.Fields.RequirementTypeId;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Owner
			string ownerFieldName = "OwnerId";
			if (documentSettings.ContainsKey(ownerFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = ownerFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "OwnerName";
				dataItemField.Caption = Resources.Fields.Owner;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Author
			string authorFieldName = "AuthorId";
			if (documentSettings.ContainsKey(authorFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = authorFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "AuthorName";
				dataItemField.Caption = Resources.Fields.Author;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Component
			string componentFieldName = "ComponentId";
			if (documentSettings.ContainsKey(componentFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = componentFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.LookupName = "ComponentName";
				dataItemField.Caption = Resources.Fields.ComponentId;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Release
			string releaseFieldName = "ReleaseId";
			if (documentSettings.ContainsKey(releaseFieldName))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = releaseFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup;
				dataItemField.LookupName = "ReleaseVersionNumber";
				dataItemField.Caption = Resources.Fields.Release;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Test Coverage - key differs from FieldName (key required for matching with artifact fields, fieldName required for population equalizer)
			if (documentSettings.ContainsKey("CoverageId"))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "CoverageCountTotal";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
				dataItemField.LookupName = "";
				dataItemField.Caption = Resources.Fields.CoverageId;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Task Progress - key differs from FieldName (key required for matching with artifact fields, fieldName required for population equalizer)
			if (documentSettings.ContainsKey("ProgressId"))
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TaskCount";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer;
				dataItemField.LookupName = "";
				dataItemField.Caption = Resources.Fields.TaskProgress;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Estimate Points
			string pointsFieldName = "EstimatePoints";
			bool showPoints = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW, pointsFieldName, 0) != 0;
			if (showPoints)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = pointsFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Decimal;
				dataItemField.LookupName = "";
				dataItemField.Caption = Resources.Fields.EstimatePoints;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Last Update Date
			string updateFieldName = "LastUpdateDate";
			bool showUpdate = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW, updateFieldName, 0) != 0;
			if (showUpdate)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = updateFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
				dataItemField.LookupName = "";
				dataItemField.Caption = Resources.Fields.LastUpdateDate;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Creation Date
			string creationFieldName = "CreationDate";
			bool showCreation = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW, creationFieldName, 0) != 0;
			if (showCreation)
			{
				dataItemField = new DataItemField();
				dataItemField.FieldName = creationFieldName;
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
				dataItemField.LookupName = "";
				dataItemField.Caption = Resources.Fields.CreationDate;
				dataItemField.Editable = false;
				dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
			}

			//Add rich text custom properties
			//Pull the custom property definitions
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			//Filter to only those in our current documentsettings list
			customProperties = customProperties.FindAll(customProperty => documentSettings.Contains(customProperty.CustomPropertyFieldName));
			//NOTE this should be the same sorting used on the UnityCustomPropertyInjector CreateControls method
			customProperties = customProperties.OrderBy(cp => cp.Position).ThenBy(cp => cp.PropertyNumber).ThenBy(cp => cp.CustomPropertyId).ToList();

			//Loop through each custom property 
			foreach (CustomProperty customProperty in customProperties)
			{
				//Check if there is a rich text option and if it set to true
				CustomPropertyOptionValue richTextOption = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText).FirstOrDefault();
				bool isRichText = richTextOption != null && richTextOption.Value == "Y";

				//Add the rich text custom property
				if (isRichText)
				{
					dataItemField = new DataItemField();
					dataItemField.FieldName = customProperty.CustomPropertyFieldName;
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
					dataItemField.LookupName = "";
					dataItemField.Caption = customProperty.CustomPropertyFieldName;
					dataItemField.Editable = true;
					dataItemField.Tooltip = customProperty.Description;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
				}
				//Otherwise remove the custom property from the documentsettings list
				//This is required for a custom property that WAS rich text, and was added to the list of fields to show, but later was switched to a different custom property type
				else
				{
					documentSettings.Remove(customProperty.CustomPropertyFieldName);
					documentSettings.Save();
				}
			}
		}


		#endregion

		/// <summary>
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="requirementId">The id of the requirement to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		public string RetrieveNameDesc(int? projectId, int requirementId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";
			const int MAX_DESCRIPTION_LENGTH = 2000;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Now retrieve the specific requirement - handle quietly if it doesn't exist
				try
				{
					//A negative requirement ID is used for the fake 'shared project' parent packages
					string tooltip = "";
					if (requirementId < 0)
					{
						int sharedProjectId = -requirementId;

						//Make sure this project shares with the current one
						ProjectManager projectManager = new ProjectManager();
						if (projectId.HasValue && projectManager.ProjectAssociation_CanProjectShare(projectId.Value, sharedProjectId, Artifact.ArtifactTypeEnum.Requirement))
						{
							ProjectView project = projectManager.RetrieveById2(sharedProjectId);
							if (string.IsNullOrEmpty(project.Description))
							{
								tooltip = Microsoft.Security.Application.Encoder.HtmlEncode(project.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_PROJECT, sharedProjectId, true);
							}
							else
							{
								tooltip = "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(project.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_PROJECT, sharedProjectId, true) + "</u><br />\n" + Strings.SafeSubstring(GlobalFunctions.HtmlRenderAsPlainText(project.Description), 0, MAX_DESCRIPTION_LENGTH);
							}
						}
					}
					else
					{
						RequirementView requirement = requirementManager.RetrieveById2(null, requirementId);
						if (string.IsNullOrEmpty(requirement.Description))
						{
							//See if it has any parents that we need to display in the tooltip
							if (requirement.IndentLevel.Length > 3 && projectId.HasValue)
							{
								List<RequirementView> parents = requirementManager.RetrieveParents(User.UserInternal, projectId.Value, requirement.IndentLevel);
								foreach (RequirementView parent in parents)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "</u> &gt; ";
								}
							}
							tooltip += Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, requirementId, true);
						}
						else
						{
							//See if it has any parents that we need to display in the tooltip
							if (requirement.IndentLevel.Length > 3 && projectId.HasValue)
							{
								List<RequirementView> parents = requirementManager.RetrieveParents(User.UserInternal, projectId.Value, requirement.IndentLevel);
								foreach (RequirementView parent in parents)
								{
									tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parent.Name) + "</u> &gt; ";
								}
							}
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, requirementId, true) + "</u><br />\n" + Strings.SafeSubstring(GlobalFunctions.HtmlRenderAsPlainText(requirement.Description), 0, MAX_DESCRIPTION_LENGTH);
						}

						//See if we have any comments to append
						IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(requirementId, Artifact.ArtifactTypeEnum.Requirement, false);
						if (comments.Count() > 0)
						{
							IDiscussion lastComment = comments.Last();
							tooltip += string.Format("<br /><i>{0} - {1} ({2})</i>",
								GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
								Strings.SafeSubstring(GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text), 0, MAX_DESCRIPTION_LENGTH),
								Microsoft.Security.Application.Encoder.HtmlEncode(lastComment.CreatorName)
								);
						}
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
		/// Expands a requirements node
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="requirementId">The requirement we're expanding</param>
		public void Expand(int projectId, int requirementId)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//If we have a negative requirement id, it is actually a shared project entry that needs to be expanded/collapsed
				if (requirementId < 0)
				{
					//Collapse the shared project
					int sharedProjectId = -requirementId;
					ProjectSettingsCollection projectSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
					string serializedValue = "";
					if (projectSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS))
					{
						serializedValue = (string)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS];
					}
					List<int> expandedProjectIds = serializedValue.FromDatabaseSerialization_List_Int32();
					if (!expandedProjectIds.Contains(sharedProjectId))
					{
						expandedProjectIds.Add(sharedProjectId);
						projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
						projectSettings.Save();
					}
				}
				else
				{
					//Expand the requirement
					Business.RequirementManager requirementManager = new Business.RequirementManager();
					requirementManager.Expand(userId, projectId, requirementId);
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Collapses a requirements node
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="requirementId">The requirement we're collapsing</param>
		public void Collapse(int projectId, int requirementId)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//If we have a negative requirement id, it is actually a shared project entry that needs to be expanded/collapsed
				if (requirementId < 0)
				{
					//Collapse the shared project
					int sharedProjectId = -requirementId;
					ProjectSettingsCollection projectSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
					string serializedValue = "";
					if (projectSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS))
					{
						serializedValue = (string)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS];
					}
					List<int> expandedProjectIds = serializedValue.FromDatabaseSerialization_List_Int32();
					if (expandedProjectIds.Contains(sharedProjectId))
					{
						expandedProjectIds.Remove(sharedProjectId);
						projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
						projectSettings.Save();
					}
				}
				else
				{
					//Collapse the requirement
					Business.RequirementManager requirementManager = new Business.RequirementManager();
					requirementManager.Collapse(userId, projectId, requirementId);
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
		/// Outdents a set of requirements
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				RequirementManager requirement = new Business.RequirementManager();
				//If a user has selected a summary item and some of its children, this will cause unexpected
				//behaviour - since some items will be doubly-outdented. Keep a record of any selected summary items
				//and if we have any children selected, simply ignore them
				List<string> summaryRequirements = new List<string>();
				for (int j = 0; j < items.Count; j++)
				{
					//Get the requirement ID
					int requirementId = int.Parse(items[j]);
					//Retrieve the item to check if a summary or not
					RequirementView selectedRequirement = requirement.RetrieveById(userId, projectId, requirementId);
					if (selectedRequirement.IsSummary)
					{
						//Add the requirement to the list of selected items together with its indent level
						summaryRequirements.Add(selectedRequirement.IndentLevel);
					}
				}

				//Iterate through all the items to be outdented
				//We need to iterate descending for Outdent since that will preserve the order
				for (int j = items.Count - 1; j >= 0; j--)
				{
					//Get the requirement ID
					int requirementId = int.Parse(items[j]);

					//Retrieve the item to check if a summary or not
					RequirementView selectedRequirement = requirement.RetrieveById(userId, projectId, requirementId);
					if (selectedRequirement.IsSummary)
					{
						//Actually perform the outdent
						requirement.Outdent(userId, projectId, requirementId);
					}
					else
					{
						//If we are the child of a selected item then don't outdent
						bool match = false;
						for (int i = 0; i < summaryRequirements.Count; i++)
						{
							string summaryIndentLevel = (string)summaryRequirements[i];
							//Are we the child of a selected item that's already been outdented
							if (SafeSubstring(selectedRequirement.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
							{
								match = true;
							}
						}
						if (!match)
						{
							requirement.Outdent(userId, projectId, requirementId);
						}
					}
				}
				return "";  //Success
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
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifactId">The id of the existing artifact we're inserting in front of (-1 for none)</param>
		/// <param name="artifact">The type of artifact we're inserting ('Requirement')</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <returns>The id of the new requirement</returns>
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

				//Check to see if we are inserting before an existing requirement or simply adding at the end
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				int requirementId = -1;
				if (artifactId == -1)
				{
					//Call the BO insert method will Null as the existing requirement Id
					//See if we're inserting a child or normal requirement
					if (artifact == "ChildRequirement")
					{
						requirementId = requirementManager.InsertChild(userId, projectId, null, null, null, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
					}
					else
					{
						requirementId = requirementManager.Insert(userId, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
					}
				}
				else
				{
					//See if we're inserting a child or normal requirement
					if (artifact == "ChildRequirement")
					{
						requirementId = requirementManager.InsertChild(userId, projectId, null, null, artifactId, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
					}
					else
					{
						requirementId = requirementManager.Insert(userId, projectId, null, null, artifactId, Requirement.RequirementStatusEnum.Requested, null, userId, null, null, "", null, null, userId);
					}
				}

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

                        //If the filter list has the type ID set to an epic then we should ignore it (because the sorted rq list shares filters with this view and it can filter by epic)
                        if (filterList.ContainsKey("RequirementTypeId"))
                        {
                            if (((MultiValueFilter)filterList["RequirementTypeId"]).Values.Contains(-1) /*epic*/)
                            {
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
					int requirementId = int.Parse(itemValue);
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
		/// Expands the list of requirements to a specific level
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="level">The number of levels to expand to (pass -1 for all levels)</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Change the indentation level of the entire requirements list
				Business.RequirementManager requirement = new Business.RequirementManager();

				if (level == -1)
				{
					//Handle the 'all-levels' case
					requirement.ExpandToLevel(userId, projectId, null);
				}
				else
				{
					requirement.ExpandToLevel(userId, projectId, level);
				}

				//See if we are also viewing shared projects
				bool filterbyProject = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, true);
				if (!filterbyProject)
				{
					ProjectManager projectManager = new ProjectManager();
					List<ProjectArtifactSharing> sharedProjects = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.Requirement);

					foreach (ProjectArtifactSharing sharedProject in sharedProjects)
					{
						if (level == -1)
						{
							//Expand to all levels
							requirement.ExpandToLevel(userId, sharedProject.SourceProjectId, null);
						}
						else
						{
							//Collapse to the specified level
							requirement.ExpandToLevel(userId, sharedProject.SourceProjectId, level - 1);
						}

						//Either expand or collapse all of the fake project 'packages'
						ProjectSettingsCollection projectSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
						string serializedValue = "";
						if (projectSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS))
						{
							serializedValue = (string)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS];
						}
						List<int> expandedProjectIds = new List<int>();
						if (level == -1 || level > 1)
						{
							expandedProjectIds = sharedProjects.Select(p => p.SourceProjectId).ToList();
						}
						projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS] = expandedProjectIds.ToDatabaseSerialization();
						projectSettings.Save();
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
		/// Moves a requirement in the system
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be moved and perform the operation
				//Check to make sure we don't have any duplicates
				RequirementManager requirement = new RequirementManager();
				List<int> existingIds = new List<int>();
				foreach (string itemValue in sourceItems)
				{
					//Get the source ID
					int sourceId = int.Parse(itemValue);
					if (!existingIds.Contains(sourceId))
					{
						requirement.Move(userId, projectId, sourceId, (destId == -1) ? null : (int?)destId);
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
				if (operation == "Update_Project_Filter")
				{
					//The value contains the state of the current project / all project selection
					if (value == "all")
					{
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false);
					}
					if (value == "current")
					{
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, true);
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
						int requirementId = int.Parse(item);
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
					List<int> requirementIds = items.Select(r => int.Parse(r)).ToList();
					TestSetManager testSetManager = new TestSetManager();
					testSetManager.CreateFromRequirements(userId, projectId, requirementIds);
				}
				else if (operation == "FocusOn")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
					if (authorizationState != Project.AuthorizationState.Authorized)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Make sure we have at least one selected
					if (items.Count > 0)
					{
						//Clear all filters
						bool expandAll;
						base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Requirement, out expandAll);

						//Focus on the specific requirement (just take the first one if they multiselect)
						int requirementId = int.Parse(items[0]);
						new RequirementManager().FocusOn(userId, requirementId);
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
		/// Exports a series of requirement (and their children) from one project to another
		/// </summary>
		/// <param name="userId">The user exporting the requirement</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="items">The list of requirements being exported</param>
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
					int requirementId = int.Parse(item);
					//Retrieve the item to check if a summary or not
					RequirementView selectedRequirement = requirement.RetrieveById(userId, sourceProjectId, requirementId);
					if (selectedRequirement.IsSummary)
					{
						//Actually perform the export
						requirement.Export(userId, sourceProjectId, requirementId, destProjectId);

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
							requirement.Export(userId, sourceProjectId, requirementId, destProjectId);
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
		/// Copies a set of requirements
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceItems">The items to copy</param>
		/// <param name="destId">The destination item's id (or -1 for no destination selected)</param>
		public void Copy(int projectId, List<string> sourceItems, int destId)
		{
			const string METHOD_NAME = "Copy";

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

				//Iterate through all the items to be copied and perform the operation
				RequirementManager requirement = new RequirementManager();
				foreach (string itemValue in sourceItems)
				{
					//Get the source ID
					int sourceId = int.Parse(itemValue);
					requirement.Copy(userId, projectId, sourceId, (destId == -1) ? null : (int?)destId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Indents a set of requirements
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				RequirementManager requirement = new Business.RequirementManager();
				//If a user has selected a summary item and some of its children, this will cause unexpected
				//behaviour - since some items will be doubly-indented. Keep a record of any selected summary items
				//and if we have any children selected, simply ignore them
				List<string> summaryRequirements = new List<string>();

				//Iterate through all the items to be deleted
				foreach (string item in items)
				{
					int requirementId = int.Parse(item);
					//Retrieve the item to check if a summary or not
					RequirementView selectedRequirement = requirement.RetrieveById(userId, projectId, requirementId);
					if (selectedRequirement.IsSummary)
					{
						//Actually perform the indent
						requirement.Indent(userId, projectId, requirementId);

						//Add the requirement to the list of selected items together with its indent level
						//Need to do a fresh retrieve after the indent so that it matches on the next iteration
						selectedRequirement = requirement.RetrieveById(userId, projectId, requirementId);
						summaryRequirements.Add(selectedRequirement.IndentLevel);
					}
					else
					{
						//If we are the child of a selected item then don't indent
						bool match = false;
						for (int i = 0; i < summaryRequirements.Count; i++)
						{
							string summaryIndentLevel = (string)summaryRequirements[i];
							//Are we the child of a selected item that's already been indented
							if (SafeSubstring(selectedRequirement.IndentLevel, summaryIndentLevel.Length) == summaryIndentLevel)
							{
								match = true;
							}
						}
						if (!match)
						{
							requirement.Indent(userId, projectId, requirementId);
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

				foreach (HierarchicalDataItem dataItem in dataItems)
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
						if (!string.IsNullOrEmpty(dataItem.ConcurrencyValue))
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

			return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.Requirement, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST, isShared, existingSavedFilterId, includeColumns);
		}

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
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
			bool expandAll = false;
			string result = base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.Requirement, out expandAll);
			if (expandAll)
			{
				ExpandToLevel(projectId, -1, null);
			}
			return result;
		}

		/// <summary>
		/// Returns the latest information on a single requirement in the system
		/// </summary>
		/// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
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
				HierarchicalDataItem dataItem = new HierarchicalDataItem();
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
		/// Populates a requirement package from a child project name. Used on the requirements list page to show shared child projects
		/// as pseudo-packages
		/// </summary>
		/// <param name="isExpanded">Should we display as expanded</param>
		/// <param name="dataItem">The data item</param>
		/// <param name="projectArtifactSharing">The project sharing record</param>
		protected void PopulateRow(HierarchicalDataItem dataItem, ProjectArtifactSharing projectArtifactSharing, string runningIndent, bool isExpanded)
		{
			//Set the primary key and concurrency value
			Project sharedProject = projectArtifactSharing.SourceProject;
			dataItem.PrimaryKey = -sharedProject.ProjectId;
			dataItem.Fields["Name"].TextValue = sharedProject.Name;

			//Specify if this is a summary row and whether expanded or not
			dataItem.Summary = true;
			dataItem.Expanded = isExpanded;
			dataItem.Alternate = false;
			dataItem.Attachment = false;
			dataItem.ReadOnly = true;
			dataItem.NotSelectable = true;

			//Add a custom URL to the requirement list page for that project
			dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, sharedProject.ProjectId));

			//Specify its indent position
			dataItem.Indent = runningIndent;
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
		protected void PopulateRow(HierarchicalDataItem dataItem, RequirementView requirementView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<RequirementWorkflowField> workflowFields = null, List<RequirementWorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = requirementView.RequirementId;
			dataItem.ConcurrencyValue = string.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, requirementView.ConcurrencyDate);

			//Specify if this is a summary row and whether expanded or not
			dataItem.Summary = requirementView.IsSummary;
			dataItem.Expanded = requirementView.IsExpanded;

			//If non-summary, check to see if it's a use-case type
			if (!requirementView.IsSummary && requirementView.RequirementTypeIsSteps)
			{
				dataItem.Alternate = true;
			}

			//Specify if it has an attachment or not
			dataItem.Attachment = requirementView.IsAttachments;

			//Specify its indent position
			dataItem.Indent = requirementView.IndentLevel;

			//Convert the workflow lists into the type expected by the ListServiceBase function
			List<WorkflowField> workflowFields2 = RequirementWorkflowManager.ConvertFields(workflowFields);
			List<WorkflowCustomProperty> workflowCustomProps2 = RequirementWorkflowManager.ConvertFields(workflowCustomProps);

			//The date and task effort fields are not editable for requirements
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
					if (dataItem.Summary && (fieldName == "RequirementStatusId" || fieldName == "EstimatePoints" || fieldName == "RequirementTypeId"))
					{
						dataItemField.Editable = false;
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
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, HierarchicalDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
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
		/// Returns the current hierarchy configuration for the current page
		/// </summary>
		/// <param name="userId">The user we're viewing the artifacts as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <returns>a dictionary where key=artifactid, value=indentlevel</returns>
		public JsonDictionaryOfStrings RetrieveHierarchy(int projectId, JsonDictionaryOfStrings standardFilters)
		{
			//The auth is done by the Retrieve method

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
			//The auth is done by the Retrieve method

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
		/// Returns a list of requirements in the system for the specific user/project
		/// </summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the requirement and custom property business objects
				RequirementManager requirementManager = new RequirementManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Create the array of data items (including the first filter item)
				HierarchicalData hierarchicalData = new HierarchicalData();
				List<HierarchicalDataItem> dataItems = hierarchicalData.Items;

				//Now get the list of populated filters
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);
				hierarchicalData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement);

				//Create the filter item first - we can clone it later
				HierarchicalDataItem filterItem = new HierarchicalDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created filter item with " + filterItem.Fields.Count.ToString() + " fields");

				//Now get the pagination information and add to the data
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
				//Get the number of requirements in the project
				int artifactCount = requirementManager.Count(userId, projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				int unfilteredCount = requirementManager.Count(User.UserInternal, projectId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				int primaryProjectsVisibleCount = artifactCount;

				//See if we are getting other projects
				List<ProjectArtifactSharing> sharedProjects = null;
				bool filterbyProject = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, true);
				Dictionary<int, int> visibleCountPerSharedProject = new Dictionary<int, int>();

				//If certain filters are being applied, we cannot include other projects
				if (filterList != null && !filterbyProject)
				{
					if (filterList.ContainsKey("ReleaseId") || filterList.ContainsKey("ComponentId"))
					{
						filterbyProject = true;
					}
					else
					{
						foreach (DictionaryEntry filter in filterList)
						{
							if (CustomPropertyManager.IsFieldCustomProperty((string)filter.Key).HasValue)
							{
								filterbyProject = true;
								break;
							}
						}
					}
				}

                //If certain filters are being applied, they need to be reset (eg if part of the filter is incompatible because it is used on the sorted rq list which shares filters with this view)
                if (filterList != null)
                {
                    if (filterList.ContainsKey("RequirementTypeId"))
                    {
                        if (((MultiValueFilter)filterList["RequirementTypeId"]).Values.Contains(-1) /*epic*/)
                        {
                            filterList.Remove("RequirementTypeId");
                        }
                    }
                }

                if (!updatedRecordsOnly && !filterbyProject)
				{
					//Get the list of projects that share requirements with us
					ProjectManager projectManager = new ProjectManager();
					sharedProjects = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.Requirement);
					artifactCount += sharedProjects.Count;

					foreach (ProjectArtifactSharing sharedProject in sharedProjects)
					{
						//Get the artifact count
						int sharedProjectId = sharedProject.SourceProjectId;
						artifactCount += requirementManager.Count(userId, sharedProjectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
						unfilteredCount += requirementManager.Count(User.UserInternal, sharedProjectId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset());
						if (!visibleCountPerSharedProject.ContainsKey(sharedProject.SourceProjectId))
						{
							visibleCountPerSharedProject.Add(sharedProject.SourceProjectId, artifactCount);
						}

						//Also add 1 to the total count for the project name which becomes a package
						unfilteredCount++;
					}
				}

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the requirements list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				List<RequirementView> requirements = requirementManager.Retrieve(userId, projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Retrieved " + requirements.Count + " requirements.");

				int pageCount = (int)decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
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
				int visibleCount = requirements.Count;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false, true);

				//Iterate through all the requirements and populate the dataitem
				foreach (RequirementView requirement in requirements)
				{
					//See if we're only asked to get updated items (we have a 5-min buffer
					if (!updatedRecordsOnly || requirement.LastUpdateDate > DateTime.UtcNow.AddMinutes(UPDATE_TIME_BUFFER_MINUTES))
					{
						//We clone the template item as the basis of all the new items
						HierarchicalDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, requirement, customProperties, false, null);
						dataItems.Add(dataItem);
					}
				}

				//If we're getting all items, also include the pagination info
				if (!updatedRecordsOnly)
				{
					hierarchicalData.PaginationOptions = this.RetrievePaginationOptions(projectId);

					//See if we have any space left on the page
					int remainingRows = paginationSize - requirements.Count;
					if (remainingRows < 1)
					{
						remainingRows = 0;
					}

					//See if we are getting other projects
					if (!filterbyProject && remainingRows > 0)
					{
						//Adjust the start row
						startRow -= primaryProjectsVisibleCount;
						if (startRow < 1)
						{
							startRow = 1;
						}

						//Add as packages
						string runningIndent = "";
						RequirementView lastTopLevelRequirement = requirements.Where(r => r.IndentLevel.Length == 3).LastOrDefault();
						if (lastTopLevelRequirement != null)
						{
							runningIndent = lastTopLevelRequirement.IndentLevel;
						}
						else
						{
							runningIndent = "AAA";
						}

						//Get the list of expanded projects
						List<int> expandedProjectIds = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_EXPANDED_PROJECTS, "").FromDatabaseSerialization_List_Int32();

						foreach (ProjectArtifactSharing sharedProject in sharedProjects)
						{
							int sharedProjectId = sharedProject.SourceProjectId;
							runningIndent = HierarchicalList.IncrementIndentLevel(runningIndent);

							//See if this is expanded or collapsed
							bool expanded = expandedProjectIds.Contains(sharedProjectId);

							//Add the project name as a top-level Package if this is the first row in the shared project
							HierarchicalDataItem dataItem;
							if (startRow == 1)
							{
								dataItem = filterItem.Clone();
								PopulateRow(dataItem, sharedProject, runningIndent, expanded);
								dataItems.Add(dataItem);
								remainingRows--;
								visibleCount++;
							}

							//Now get the requirements from this project if expanded
							if (expanded && remainingRows > 0)
							{
								requirements = requirementManager.Retrieve(userId, sharedProjectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
								foreach (RequirementView requirement in requirements)
								{
									//Add on the project indent level
									requirement.IndentLevel = runningIndent + requirement.IndentLevel;

									//We clone the template item as the basis of all the new items
									dataItem = filterItem.Clone();

									//Now populate with the data, no custom properties populated from child project
									PopulateRow(dataItem, requirement, null, false, null);
									dataItems.Add(dataItem);

									//These child items are read-only and non-selectable
									dataItem.ReadOnly = true;
									dataItem.NotSelectable = true;
									remainingRows--;
									visibleCount++;

									if (remainingRows < 1)
									{
										break;
									}
								}

								/*                                //Adjust the start row
																startRow -= requirements.Count;
																if (startRow < 1)
																{
																	startRow = 1;
																}*/
							}

							if (remainingRows < 1)
							{
								break;
							}

							//Reset the start row
							if (visibleCountPerSharedProject.ContainsKey(sharedProjectId))
							{
								startRow -= visibleCountPerSharedProject[sharedProjectId];
								if (startRow < 1)
								{
									startRow = 1;
								}
							}
						}
					}
				}

				//Display the visible and total count of artifacts
				hierarchicalData.VisibleCount = visibleCount;
				hierarchicalData.TotalCount = unfilteredCount;

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
					List<RequirementType> types = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
					lookupValues = ConvertLookupValues(types.OfType<DataModel.Entity>().ToList(), "RequirementTypeId", "Name");
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
					int percentPassed = (int)decimal.Round(((decimal)passedCoverage * (decimal)100) / (decimal)totalCoverage, 0);
					int percentFailed = (int)decimal.Round(((decimal)failedCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);
					int percentCaution = (int)decimal.Round(((decimal)cautionCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);
					int percentBlocked = (int)decimal.Round(((decimal)blockedCoverage * (decimal)100.00) / (decimal)totalCoverage, 0);

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

		#region IAssociationPanel methods

		/// <summary>
		/// Returns a plain-text version of the requirement name, description and all its successive parent requirements
		/// Used in the mapping list box where you want to see the hierarchy
		/// </summary>
		/// <param name="requirementId">The id of the requirement to get the data for</param>
		/// <returns>The name, description and parent list converted to plain-text</returns>
		public string RetrieveNameFolder(int requirementId)
		{
			const string METHOD_NAME = "RetrieveNameFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Now retrieve the specific requirement - handle quietly if it doesn't exist
				try
				{
					//First we need to get the requirement itself
					RequirementView requirement = requirementManager.RetrieveById2(null, requirementId);

					//Next we need to get the list of successive parent folders
					List<RequirementView> parentRequirements = requirementManager.RetrieveParents(Business.UserManager.UserInternal, requirement.ProjectId, requirement.IndentLevel);
					string tooltip = "";
					foreach (RequirementView parentRequirement in parentRequirements)
					{
						tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentRequirement.Name) + "</u> &gt; ";
					}

					//Now we need to get the requirement itself
					tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(requirement.Name) + "</u>";
					if (!string.IsNullOrEmpty(requirement.Description))
					{
						tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(requirement.Description);
					}

					//See if we have any comments to append
					IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(requirementId, Artifact.ArtifactTypeEnum.Requirement, false);
					if (comments.Count() > 0)
					{
						IDiscussion lastComment = comments.Last();
						tooltip += string.Format("<br /><i>{0} - {1} ({2})</i>",
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

		/// <summary>
		/// Creates a new requirement from the test case
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactId">The id of the test case</param>
		/// <param name="artifactTypeId">The type of artifact (ignored in this implementation)</param>
		/// <param name="selectedItems">Any selected items (used in some cases)</param>
		/// <param name="folderId">The of a folder (not used for requirements)</param>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the first selected item only
				int? existingRequirementId = null;
				if (selectedItems != null && selectedItems.Count > 0)
				{
					existingRequirementId = selectedItems[0];
				}

				//Create a new requirement from this test case
				RequirementManager requirementManager = new RequirementManager();
				int newRequirementId = 0;
				if (artifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident)
				{
					newRequirementId = requirementManager.CreateFromIncident(artifactId, userId);
				}
				else
				//by default create from test case - as original code did not have a check for artifact type and was used exclusively here for creating from a test case
				{
					newRequirementId = requirementManager.CreateFromTestCase(userId, projectId, artifactId, existingRequirementId);
				}

				//Handle notifications
				if (newRequirementId > 0)
				{
					//Retrieve the new requirement to pass to notification manager
					Requirement notificationArt = requirementManager.RetrieveById3(projectId, newRequirementId);
					((Requirement)notificationArt).MarkAsAdded();
					try
					{
						new NotificationManager().SendNotificationForArtifact(notificationArt);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Requirement.");
					}
				}
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				return "Unable to locate the test case, it may have been deleted.";
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				return "Unable to create new requirement, please check the server Event Log";
			}
			return "";
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
		/// Returns a list of requirements for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">The indent level of the parent folder, or empty string for all items</param>
		/// <returns>List of requirements</returns>
		/// <param name="displayMode">
		/// The display mode of the navigation list:
		/// 1 = Filtered List
		/// 2 = All Items (no filters)
		/// 3 = Assigned to the Current User
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);

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
				//Get the number of requirements in the project
				int artifactCount = requirementManager.Count(userId, projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the requirements list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				List<RequirementView> requirements;
				if (displayMode == 2)
				{
					//All Items
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
					requirements = requirementManager.Retrieve(userId, projectId, startRow, paginationSize, null, 0);
				}
				else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//Assigned to User, by priority then status
					requirements = requirementManager.RetrieveByOwnerId(userId, projectId, null, false);
				}
				else
				{
					//Filtered List
					if (authorizationState == Project.AuthorizationState.Limited)
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}
					requirements = requirementManager.Retrieve(userId, projectId, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());
				}

				int pageCount = (int)decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);
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

				//Iterate through all the requirements and populate the dataitem (only some columns are needed)
				foreach (RequirementView requirement in requirements)
				{
					//If we are passed a parent indent level, only return child items
					if (string.IsNullOrEmpty(indentLevel) || (requirement.IndentLevel.Length > indentLevel.Length && requirement.IndentLevel.Substring(0, indentLevel.Length) == indentLevel))
					{
						//Create the data-item
						HierarchicalDataItem dataItem = new HierarchicalDataItem();

						//Populate the necessary fields
						//If displaying for a user, don't indent because they are not displayed in indent order
						dataItem.PrimaryKey = requirement.RequirementId;
						if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
						{
							dataItem.Indent = "";
							dataItem.Expanded = true;
						}
						else
						{
							dataItem.Indent = requirement.IndentLevel;
							dataItem.Expanded = requirement.IsExpanded;
						}

						//Name/Desc
						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "Name";
						dataItemField.TextValue = requirement.Name;
						dataItem.Summary = requirement.IsSummary;
						dataItem.Fields.Add("Name", dataItemField);

						//If displaying by user, color by importance, since that's the sort order
						if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned && requirement.ImportanceId.HasValue)
						{
							//Importance style
							dataItemField.CssClass = requirement.ImportanceColor;
						}

						//If non-summary, check to see if it's a use-case type
						if (!requirement.IsSummary && requirement.RequirementTypeIsSteps)
						{
							dataItem.Alternate = true;
						}

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
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
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
		/// Retrieves the list of workflow operations for the current requirement
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="typeId">The requirement type</param>
		/// <param name="artifactId">The id of the requirement</param>
		/// <returns>The list of available workflow operations</returns>
		/// <remarks>Pass a specific type id if the user has changed the type of the requirement, but not saved it yet.</remarks>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the array of data items to store the workflow operations
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of available transitions for the current step in the workflow
				RequirementManager requirementManager = new RequirementManager();
				RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();
				RequirementView requirementView = requirementManager.RetrieveById2(projectId, artifactId);

				//If we have a summary/package requirement, we never return any operations
				if (requirementView.IsSummary)
				{
					return new List<DataItem>();
				}

				int workflowId;
				if (typeId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForRequirementType(typeId.Value);
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForRequirementType(requirementView.RequirementTypeId);
				}

				//Get the current user's role
				int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

				//Determine if the current user is the author or owner of the incident
				bool isAuthor = false;
				if (requirementView.AuthorId == CurrentUserId.Value)
				{
					isAuthor = true;
				}
				bool isOwner = false;
				if (requirementView.OwnerId.HasValue && requirementView.OwnerId.Value == CurrentUserId.Value)
				{
					isOwner = true;
				}
				int statusId = requirementView.RequirementStatusId;
				List<RequirementWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isAuthor, isOwner);

				//Populate the data items list
				foreach (RequirementWorkflowTransition workflowTransition in workflowTransitions)
				{
					//The data item itself
					DataItem dataItem = new DataItem();
					dataItem.PrimaryKey = (int)workflowTransition.WorkflowTransitionId;
					dataItems.Add(dataItem);

					//The WorkflowId field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "WorkflowId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.RequirementWorkflowId;
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
					dataItemField.IntValue = (int)workflowTransition.InputRequirementStatusId;
					dataItemField.TextValue = workflowTransition.InputRequirementStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusId field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = (int)workflowTransition.OutputRequirementStatusId;
					dataItemField.TextValue = workflowTransition.OutputRequirementStatus.Name;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//The OutputStatusOpenYn field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "OutputStatusOpenYn";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
					dataItemField.TextValue = (workflowTransition.OutputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Rejected || workflowTransition.OutputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Obsolete || workflowTransition.OutputRequirementStatusId == (int)Requirement.RequirementStatusEnum.Completed) ? "N" : "Y";
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

		#region IHierarchicalSelector Methods

		/// <summary>
		/// Returns a list of requirements that are available to mapped to a Task
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">The indent level of the parent folder, or empty string for root items</param>
		/// <returns>List of available requirements</returns>
		/// <remarks>Returns just the child items of the passed-in indent-level</remarks>
		public List<HierarchicalDataItem> HierarchicalSelector_RetrieveAvailable(int projectId, string indentLevel)
		{
			const string METHOD_NAME = "HierarchicalSelector_RetrieveAvailable";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the requirement business object
				RequirementManager requirementManager = new RequirementManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//Get the dataset of all the requirements that are child ones
				List<RequirementView> childRequirements = requirementManager.RetrieveChildren(Business.UserManager.UserInternal, projectId, indentLevel, false);

				//Iterate through all the requirements and populate the dataitem (only some columns are needed)
				foreach (RequirementView childRequirement in childRequirements)
				{
					//Create the data-item
					HierarchicalDataItem dataItem = new HierarchicalDataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = childRequirement.RequirementId;
					dataItem.Indent = childRequirement.IndentLevel;

					//Test Case Id
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Id";
					dataItemField.IntValue = childRequirement.RequirementId;
					dataItem.Fields.Add("Id", dataItemField);

					//Name/Desc
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = childRequirement.Name;
					dataItem.Summary = childRequirement.IsSummary;
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

		#endregion

		#region RequirementsService Methods

		/// <summary>
		/// Returns a list of requirements as DOT notation for use in mind maps and other graphical forms
		/// </summary>
		/// <param name="numberOfLevels">The number of levels to show (null = all)</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="includeAssociations">Should we include associations in the diagram</param>
		/// <param name="releaseId">The id of the release (null == all releases)</param>
		/// <returns>The DOT notation form of the requirements</returns>
		/// <remarks>
		/// Here's information on the DOT language:
		/// https://www.graphviz.org/pdf/dotguide.pdf
		/// </remarks>
		public string Requirement_RetrieveAsDotNotation(int projectId, int? numberOfLevels, bool includeAssociations, int? releaseId)
		{
			const string METHOD_NAME = "Requirement_RetrieveAsDotNotation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGE_SIZE = 50;

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the project (for its name)
				Project project = new ProjectManager().RetrieveById(projectId);

				//See if we need to update the stored settings
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS);
				bool dirty = false;
				int numberOfLevelsInt = (numberOfLevels.HasValue) ? numberOfLevels.Value : 0;
				if (!settings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL) || ((int)settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL]) != numberOfLevelsInt)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL] = numberOfLevelsInt;
					dirty = true;
				}
				if (!settings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS) || ((bool)settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS]) != includeAssociations)
				{
					settings[GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS] = includeAssociations;
					dirty = true;
				}
				if (dirty)
				{
					settings.Save();
				}

				//Get the hierarchy of requirements based on the specified level and release
				RequirementManager requirementManager = new RequirementManager();
				int count = int.MaxValue;
				List<RequirementView> requirements = new List<RequirementView>();

				//Loop through pages of Requirements. Stop once the next page takes us over 5000.
				for (int startRow = 1; startRow < count; startRow += PAGE_SIZE)
				{
					List<RequirementView> requirementsRange = requirementManager.Requirement_RetrieveForMindMap(projectId, numberOfLevels, out count, startRow, PAGE_SIZE);
					if (requirementsRange.Count + requirements.Count <= 5000)
						requirements.AddRange(requirementsRange);
					else
					{
						System.Diagnostics.Debug.WriteLine("Hit 5000.");
						break;
					}
				}


				/*

                digraph requirements_map{
                    rankdir=LR;
                    node [shape=box rx=5 ry=5 color=lightblue2 style=filled];
                    "Root" -> "RQ:4";
                    "Root" -> "RQ:5";
                    "Root" -> "RQ:6";
                    "RQ:1" -> "RQ:19" [label="related to", arrowhead="undirected", style="stroke: #aaa; stroke-dasharray:2,2" labelStyle="fill: #aaa;"];
                }

                 */

				//Convert the requirements into the dot notation
				string dotNotation =
	@"digraph requirements_map {
    rankdir=LR;
    node [rx=5, ry=5, style=filled];
";
				string rootToken = "Root";
				//Add the root label
				dotNotation += string.Format("    \"{0}\" [label=\"{1}\", labelType = \"html\"];\n", rootToken, HttpUtility.HtmlEncode(project.Name));
				//project

				foreach (RequirementView requirement in requirements)
				{
					//Add the label
					string link = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, projectId, requirement.RequirementId));
					string style = "";
					if (!string.IsNullOrEmpty(requirement.ImportanceColor))
					{
						if (requirement.IsSummary)
						{
							style = "fill: #" + requirement.ImportanceColor + "; font-weight: bold";
						}
						else
						{
							style = "fill: #" + requirement.ImportanceColor;
						}
					}
					else if (requirement.IsSummary)
					{
						style = "font-weight: bold";
					}
					dotNotation += string.Format("    \"{0}\" [label=\"<a href='{2}' data-requirement-id='{4}'>{1}</a>\", labelType = \"html\", style=\"{3}\"];\n", requirement.ArtifactToken, HttpUtility.HtmlEncode(requirement.Name) + " [" + requirement.ArtifactToken + "]", link, style, requirement.RequirementId);

					//Add the relationship
					if (requirement.IndentLevel.Length > 3)
					{
						//Find the parent
						RequirementView parentRequirement = requirements.FirstOrDefault(r => r.IndentLevel == requirement.IndentLevel.SafeSubstring(0, requirement.IndentLevel.Length - 3));
						if (parentRequirement == null)
						{
							//Fallback to root
							dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", rootToken, requirement.ArtifactToken);
						}
						else
						{
							dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", parentRequirement.ArtifactToken, requirement.ArtifactToken);
						}
					}
					else
					{
						dotNotation += string.Format("    \"{0}\" -> \"{1}\";\n", rootToken, requirement.ArtifactToken);
					}
				}

				//Next see if we have any associations to display
				if (includeAssociations)
				{
					int associationCount = int.MaxValue;
					List<ArtifactLink> associations = new List<ArtifactLink>();
					for (int startRow = 1; startRow < associationCount; startRow += PAGE_SIZE)
					{
						List<ArtifactLink> associationsRange = requirementManager.ArtifactLink_RetrieveAllForRequirements(projectId, out associationCount, startRow, PAGE_SIZE);
						associations.AddRange(associationsRange);
					}
					List<ArtifactLinkType> associationTypes = new ArtifactLinkManager().RetrieveLinkTypes(true);
					foreach (ArtifactLink association in associations)
					{
						//Make sure that both ends are already in the diagram
						if (requirements.Any(r => r.RequirementId == association.SourceArtifactId) && requirements.Any(r => r.RequirementId == association.DestArtifactId))
						{
							//Display as directional if a dependency link, otherwise just non-directional
							//For the label use the comment if provided, otherwise just the link type
							string comment = "";
							if (string.IsNullOrEmpty(association.Comment))
							{
								ArtifactLinkType associationType = associationTypes.FirstOrDefault(a => a.ArtifactLinkTypeId == association.ArtifactLinkTypeId);
								if (associationType != null)
								{
									comment = associationType.Name;
								}
							}
							else
							{
								comment = HttpUtility.HtmlAttributeEncode(association.Comment);
							}
							if (association.ArtifactLinkTypeId == (int)ArtifactLink.ArtifactLinkTypeEnum.DependentOn)
							{
								dotNotation += string.Format("    \"{0}\" -> \"{1}\" [label=\"{2}\", style=\"stroke: #aaa; stroke-dasharray:2,2\" labelStyle=\"fill: #aaa;\"];\n", Requirement.ARTIFACT_PREFIX + ":" + association.SourceArtifactId, Requirement.ARTIFACT_PREFIX + ":" + association.DestArtifactId, comment);
							}
							else
							{
								dotNotation += string.Format("    \"{0}\" -> \"{1}\" [label=\"{2}\", arrowhead=\"undirected\", style=\"stroke: #aaa; stroke-dasharray:2,2\" labelStyle=\"fill: #aaa;\"];\n", Requirement.ARTIFACT_PREFIX + ":" + association.SourceArtifactId, Requirement.ARTIFACT_PREFIX + ":" + association.DestArtifactId, comment);
							}
						}
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


		/// <summary>
		/// Splits a requirement into two smaller requirements
		/// </summary>
		/// <param name="estimatePoints">The estimate (in points) to assign to the NEW requirement (null uses the auto calculation)</param>
		/// <param name="ownerId">The owner of the NEW requirement, leaving null uses the existing requirement's owner</param>
		/// <param name="requirementId">The id of the requirement to split</param>
		/// <param name="name">The name of the new requirement</param>
		/// <param name="comment">The comment to add to the association between the two requirements (optional)</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns></returns>
		public int Requirement_Split(int projectId, int requirementId, string name, decimal? estimatePoints, int? ownerId, string comment)
		{
			const string METHOD_NAME = "Requirement_Split";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized (modify owned (limited) is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				RequirementManager requirementManager = new RequirementManager();

				//First retrieve the requirement and make sure it exists and is in the specified project
				RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
				if (requirement == null)
				{
					throw new ArtifactNotExistsException(Resources.Messages.RequirementDetails_ArtifactNotExists);
				}

				//Make sure we have a valid name
				if (string.IsNullOrEmpty(name))
				{
					throw new Exception(string.Format(Resources.Messages.ListServiceBase_FieldRequired, Resources.Fields.Name));
				}

				//Now do the split and capture the new ID
				int newRequirementId = requirementManager.Split(projectId, requirementId, name, userId, estimatePoints, ownerId, comment);
				return newRequirementId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the data for the project group requirements test coverage status graphs
		/// </summary>
        /// <param name="activeReleasesOnly">do we only want the active releases' information</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <returns></returns>
		public List<GraphEntry> Requirement_RetrieveGroupTestCoverage(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "Requirement_RetrieveGroupTestCoverage";

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
				//Now get the requirements test coverage list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				List<RequirementCoverageSummary> requirementsCoverages = new RequirementManager().RetrieveCoverageSummary(projectGroupId, activeReleasesOnly);
				if (requirementsCoverages != null)
				{
					foreach (RequirementCoverageSummary entry in requirementsCoverages)
					{
						if (entry.CoverageCount.HasValue)
						{
							GraphEntry graphEntry = new GraphEntry();
							graphEntry.Name = entry.CoverageStatusOrder.ToString();
							graphEntry.Caption = entry.CoverageStatus;
							graphEntry.Count = (int)Math.Round(entry.CoverageCount.Value);
							graphEntry.Color = RequirementManager.GetCoverageStatusColor(entry.CoverageStatusOrder);
							graphEntries.Add(graphEntry);
						}
					}
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
		/// Returns the data needed for the simple requirements burndown graph on the list page
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns></returns>
		public GraphData Requirement_RetrieveBurndown(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "Requirement_RetrieveBurndown";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to view requirements
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Hande the release = none case (just ignore)
				if (releaseId == ManagerBase.NoneFilterValue)
				{
					releaseId = null;
				}

				//Now get the requirements test coverage list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				DataSet graphDataSet = new GraphManager().Requirement_RetrieveBurnDown(projectId, releaseId, GlobalFunctions.GetCurrentTimezoneUtcOffset());

				if (graphDataSet == null)
				{
					return null;
				}

				//Create the graph data
				DataObjects.GraphData graphData = new DataObjects.GraphData();

				//Generate the number of different data-series
				DataTable dataTable = graphDataSet.Tables[0];
				foreach (DataColumn dataColumn in dataTable.Columns)
				{
					//We don't add the key columns as they are used for the axis
					if (dataTable.PrimaryKey.Contains(dataColumn))
					{
						graphData.XAxisCaption = dataColumn.Caption;
					}
					else
					{
						DataObjects.DataSeries series = new DataObjects.DataSeries();
						series.Name = dataColumn.ColumnName;
						series.Caption = dataColumn.Caption;
						//See if a color is included in the series
						if (dataColumn.ExtendedProperties.ContainsKey("Color"))
						{
							series.Color = (string)dataColumn.ExtendedProperties["Color"];
						}

						//See if a style of series is specified (defaults to bar if not specified)
						series.Type = (int)Graph.GraphSeriesTypeEnum.Bar;
						if (dataColumn.ExtendedProperties.ContainsKey("Type"))
						{
							series.Type = (int)dataColumn.ExtendedProperties["Type"];
						}

						graphData.Series.Add(series);
					}
				}

				//Populate the data items
				foreach (DataRow dataRow in dataTable.Rows)
				{
					//First we need to add the actual data x-axis values
					//Get the key column
					DataColumn dataColumn = dataTable.PrimaryKey[0];
					GraphAxisPosition axisPosition = new GraphAxisPosition();
					axisPosition.Id = dataTable.Rows.IndexOf(dataRow);
					axisPosition.StringValue = ((string)dataRow[dataColumn]);
					graphData.XAxis.Add(axisPosition);

					//Now add the data series
					foreach (DataObjects.DataSeries series in graphData.Series)
					{
						if (dataRow[series.Name] != null)
						{
							object value = dataRow[series.Name];
							if (value.GetType() == typeof(decimal))
							{
								series.Values.Add(axisPosition.Id.ToString(), decimal.Round((decimal)value, 1));
							}
							else if (value.GetType() == typeof(double))
							{
								double doubleValue = (double)value;
								series.Values.Add(axisPosition.Id.ToString(), decimal.Round(new decimal(doubleValue), 1));
							}
							else if (value.GetType() == typeof(int))
							{
								int intValue = (int)value;
								series.Values.Add(axisPosition.Id.ToString(), (decimal)intValue);
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return graphData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns the data for the requirements test coverage status graphs
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <param name="showRegressionCoverage">Whether this is the regression coverage graph or not</param>
		/// <returns></returns>
		public List<GraphEntry> Requirement_RetrieveTestCoverage(int projectId, int? releaseId, bool showRegressionCoverage)
		{
			const string METHOD_NAME = "Requirement_RetrieveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized to view requirements
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Requirement);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the requirements test coverage list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				List<RequirementCoverageSummary> requirementsCoverages = new RequirementManager().RetrieveCoverageSummary(projectId, releaseId, showRegressionCoverage);
				if (requirementsCoverages != null)
				{
					foreach (RequirementCoverageSummary entry in requirementsCoverages)
					{
						if (entry.CoverageCount.HasValue)
						{
							GraphEntry graphEntry = new GraphEntry();
							graphEntry.Name = entry.CoverageStatusOrder.ToString();
							graphEntry.Caption = entry.CoverageStatus;
							graphEntry.Count = (int)Math.Round(entry.CoverageCount.Value, 0, MidpointRounding.AwayFromZero);
							graphEntry.Color = RequirementManager.GetCoverageStatusColor(entry.CoverageStatusOrder);
							graphEntries.Add(graphEntry);
						}
					}
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

		#endregion
	}
}
