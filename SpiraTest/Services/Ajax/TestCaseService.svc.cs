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
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the SortedGrid and TreeView AJAX components for displaying/updating test case data
	/// </summary>
	[
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
	]
	public class TestCaseService : SortedListServiceBase, ITestCaseService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService::";

		protected const string PROJECT_SETTINGS_PAGINATION = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_CASE_PAGINATION_SIZE;

		/// <summary>
		/// Constructor
		/// </summary>
		public TestCaseService()
		{
		}

		#region WorkflowOperations Methods

		/// <summary>
		/// Retrieves the list of workflow operations for the current testCase
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="typeId">The testCase type</param>
		/// <param name="artifactId">The id of the testCase</param>
		/// <returns>The list of available workflow operations</returns>
		/// <remarks>Pass a specific type id if the user has changed the type of the testCase, but not saved it yet.</remarks>
		public List<DataItem> WorkflowOperations_Retrieve(int projectId, int artifactId, int? typeId)
		{
			const string METHOD_NAME = "WorkflowOperations_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}
			//Create the array of data items to store the workflow operations
			List<DataItem> dataItems = new List<DataItem>();

			try
			{
				

				//Get the list of available transitions for the current step in the workflow
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, artifactId);
				int workflowId;
				if (typeId.HasValue)
				{
					workflowId = workflowManager.Workflow_GetForTestCaseType(typeId.Value);
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForTestCaseType(testCaseView.TestCaseTypeId);
				}

				//Get the current user's role
				int projectRoleId = (SpiraContext.Current.ProjectRoleId.HasValue) ? SpiraContext.Current.ProjectRoleId.Value : -1;

				//Determine if the current user is the author or owner of the incident
				bool isAuthor = false;
				if (testCaseView.AuthorId == CurrentUserId.Value)
				{
					isAuthor = true;
				}
				bool isOwner = false;
				if (testCaseView.OwnerId.HasValue && testCaseView.OwnerId.Value == CurrentUserId.Value)
				{
					isOwner = true;
				}
				int statusId = testCaseView.TestCaseStatusId;
				List<TestCaseWorkflowTransition> workflowTransitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, statusId, projectRoleId, isAuthor, isOwner);
				

				//Populate the data items list
				foreach (TestCaseWorkflowTransition workflowTransition in workflowTransitions)
				{
					var transitions = workflowManager.WorkflowTransition_RetrieveByInputStatus(workflowId, workflowTransition.InputTestCaseStatusId);
					if (IsDataItemFieldAvailableForTransition(workflowTransition, artifactId, projectId))
					{
						//The data item itself
						DataItem dataItem = new DataItem();


						//The WorkflowId field
						DataItemField dataItemField = new DataItemField();
						dataItemField.FieldName = "WorkflowId";
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
						dataItemField.IntValue = (int)workflowTransition.TestCaseWorkflowId;
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
						dataItemField.IntValue = (int)workflowTransition.InputTestCaseStatusId;
						dataItemField.TextValue = workflowTransition.InputTestCaseStatus.Name;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//The OutputStatusId field
						dataItemField = new DataItemField();
						dataItemField.FieldName = "OutputStatusId";
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Integer;
						dataItemField.IntValue = (int)workflowTransition.OutputTestCaseStatusId;
						dataItemField.TextValue = workflowTransition.OutputTestCaseStatus.Name;
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						//The OutputStatusOpenYn field
						dataItemField = new DataItemField();
						dataItemField.FieldName = "OutputStatusOpenYn";
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
						dataItemField.TextValue = (workflowTransition.OutputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Rejected || workflowTransition.OutputTestCaseStatusId == (int)TestCase.TestCaseStatusEnum.Obsolete) ? "N" : "Y";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

						dataItemField = new DataItemField();
						dataItemField.FieldName = "SignatureYn";
						dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Flag;
						dataItemField.TextValue = (workflowTransition.IsSignatureRequired) ? "Y" : "N";
						dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
						dataItems.Add(dataItem);


					}
				}
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

			return dataItems;
		}

		#endregion

		#region IFormService methods

		/// <summary>Returns a single test case data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current test case</param>
		/// <returns>A test case data item</returns>
		public DataItem Form_Retrieve(int projectId, int? artifactId)
		{
			const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited edit or full edit)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, null, false);

				//Need to add the empty column to capture any new comments added
				if (!dataItem.Fields.ContainsKey("NewComment"))
				{
					dataItem.Fields.Add("NewComment", new DataItemField() { FieldName = "NewComment", Required = false, Editable = true, Hidden = false });
				}
				if (!dataItem.Fields.ContainsKey("AutomationChanged"))
				{
					dataItem.Fields.Add("AutomationChanged", new DataItemField() { FieldName = "AutomationChanged", Required = false, Editable = true, Hidden = false });
				}

				//Get the test case for the specific test case id
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, artifactId.Value);

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (testCaseView.OwnerId.HasValue)
				{
					ownerId = testCaseView.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCaseView.AuthorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Get the list of workflow fields and custom properties
				int workflowId = workflowManager.Workflow_GetForTestCaseType(testCaseView.TestCaseTypeId);

				int statusId = testCaseView.TestCaseStatusId;
				List<TestCaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, statusId);
				List<TestCaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, statusId);

				//See if we have any existing artifact custom properties for this row
				if (artifactCustomProperty == null)
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false);
					PopulateRow(dataItem, testCaseView, customProperties, true, (ArtifactCustomProperty)null, null, workflowFields, workflowCustomProps);
				}
				else
				{
					PopulateRow(dataItem, testCaseView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null, workflowFields, workflowCustomProps);
				}

				//Set the hyperlink on the 'ExecutionStatus' that points to the latest test run
				if (dataItem.Fields.ContainsKey("ExecutionStatusId"))
				{
					dataItem.Fields["ExecutionStatusId"].Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCaseRuns, projectId, artifactId.Value));
				}

				//Also need to return back a special field to denote if the user is the owner or creator of the artifact
				bool isArtifactCreatorOrOwner = (ownerId == userId || testCaseView.AuthorId == userId);
				dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

				//Add a special field if we cannot edit or execute this test case in its current status
				if (!testCaseManager.IsTestCaseInExecutableStatus(testCaseView, workflowFields))
				{
					dataItem.Fields.Add("_IsTestCaseInExecutableStatus", new DataItemField() { FieldName = "_IsTestCaseInExecutableStatus", TextValue = "N" });
				}
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseView, workflowFields))
				{
					dataItem.Fields.Add("_AreTestStepsEditableInStatus", new DataItemField() { FieldName = "_AreTestStepsEditableInStatus", TextValue = "N" });
				}

				//Populate any data mapping values are not part of the standard 'shape'
				if (artifactId.HasValue)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestCase, artifactId.Value);
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

					//Populate the folder path as a special field
					if (testCaseView.TestCaseFolderId.HasValue)
					{
						List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseView.ProjectId, testCaseView.TestCaseFolderId.Value, true);
						string pathArray = "[";
						bool isFirst = true;
						foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
						{
							if (isFirst)
							{
								isFirst = false;
							}
							else
							{
								pathArray += ",";
							}
							pathArray += "{ \"name\": \"" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "\", \"id\": " + parentFolder.TestCaseFolderId + " }";
						}
						pathArray += "]";
						dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath", TextValue = pathArray });
					}
					else
					{
						//send a blank folder path object back so client knows this artifact has folders
						dataItem.Fields.Add("_FolderPath", new DataItemField() { FieldName = "_FolderPath" });
					}

					//See if we have an automation engine selected or not
					if (!testCaseView.AutomationEngineId.HasValue || !testCaseView.AutomationAttachmentId.HasValue)
					{
						AddAutomationField(dataItem.Fields, "AutomationFileIcon", Artifact.ArtifactFieldTypeEnum.Text, "artifact-Document.svg", -1);
						AddAutomationField(dataItem.Fields, "AutomationDocumentId", Artifact.ArtifactFieldTypeEnum.Lookup, Resources.Fields.View_File, 0);
						AddAutomationField(dataItem.Fields, "AutomationType", Artifact.ArtifactFieldTypeEnum.Text, "", -1);
						AddAutomationField(dataItem.Fields, "AutomationLink", Artifact.ArtifactFieldTypeEnum.Text, "", -1);
						AddAutomationField(dataItem.Fields, "AutomationDocumentTypeId", Artifact.ArtifactFieldTypeEnum.Lookup, "", -1);
						AddAutomationField(dataItem.Fields, "AutomationDocumentFolderId", Artifact.ArtifactFieldTypeEnum.Lookup, "", -1);
						AddAutomationField(dataItem.Fields, "AutomationVersion", Artifact.ArtifactFieldTypeEnum.Text, "", -1);
						AddAutomationField(dataItem.Fields, "AutomationScript", Artifact.ArtifactFieldTypeEnum.Text, "", -1);
					}
					else
					{
						//Retrieve the attachment and populate the fields accordingly
						AttachmentManager attachmentManager = new AttachmentManager();
						int attachmentId = testCaseView.AutomationAttachmentId.Value;
						ProjectAttachmentView projectAttachment = attachmentManager.RetrieveForProjectById2(projectId, attachmentId);
						AddAutomationField(dataItem.Fields, "AutomationDocumentTypeId", Artifact.ArtifactFieldTypeEnum.Lookup, "", projectAttachment.DocumentTypeId);

						//See if we have an attached or linked test script
						if (projectAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
						{
							//See if we have Rapise/SmarteStudio as they use 'repository' files
							if (!String.IsNullOrEmpty(projectAttachment.Filename) && projectAttachment.Filename.EndsWith(GlobalFunctions.REPOSITORY_TEST_EXTENSION))
							{
								//We have a repository test
								AddAutomationField(dataItem.Fields, "AutomationType", Artifact.ArtifactFieldTypeEnum.Text, "repository", -1);

								string fileTypeIcon = "Filetypes/" + GlobalFunctions.GetFileTypeImage(projectAttachment.Filename);
								AddAutomationField(dataItem.Fields, "AutomationFileIcon", Artifact.ArtifactFieldTypeEnum.Text, fileTypeIcon, -1);

								//See if we are using the 'older' format file that had a folder path included, if not, use the real folder path for the redirect
								if (projectAttachment.Filename.Contains('\\'))
								{
									//Old Format
									AddAutomationField(dataItem.Fields, "AutomationLink", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.Filename, -1, UrlRewriterModule.ResolveUrl("~/RepositoryRedirect.ashx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_REPOSITORY_PATH + "=" + HttpContext.Current.Server.UrlEncode(projectAttachment.Filename)));
								}
								else
								{
									//New Format
									ProjectAttachmentFolder repositoryFolder = attachmentManager.RetrieveFolderById(projectAttachment.ProjectAttachmentFolderId);
									string scriptProjectFilePath = repositoryFolder.Name + "\\" + projectAttachment.Filename;
									AddAutomationField(dataItem.Fields, "AutomationLink", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.Filename, -1, UrlRewriterModule.ResolveUrl("~/RepositoryRedirect.ashx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "=" + projectId + "&" + GlobalFunctions.PARAMETER_REPOSITORY_PATH + "=" + HttpContext.Current.Server.UrlEncode(scriptProjectFilePath)));
								}

								//The user cannot change these two fields for a repository test (since it will break the link)
								AddAutomationField(dataItem.Fields, "AutomationDocumentFolderId", Artifact.ArtifactFieldTypeEnum.Lookup, "", projectAttachment.ProjectAttachmentFolderId, "", false, false, false);
								AddAutomationField(dataItem.Fields, "AutomationVersion", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.CurrentVersion, -1, "", false, false, false);
							}
							else
							{
								//We have a regular attached test script
								string fileTypeIcon = String.IsNullOrEmpty(projectAttachment.Filename) ? "artifact-Document.svg" : "Filetypes/" + GlobalFunctions.GetFileTypeImage(projectAttachment.Filename);
								AddAutomationField(dataItem.Fields, "AutomationFileIcon", Artifact.ArtifactFieldTypeEnum.Text, fileTypeIcon, -1);
								AddAutomationField(dataItem.Fields, "AutomationDocumentId", Artifact.ArtifactFieldTypeEnum.Lookup, Resources.Fields.View_File, projectAttachment.AttachmentId);
								AddAutomationField(dataItem.Fields, "AutomationType", Artifact.ArtifactFieldTypeEnum.Text, "attached", -1);
								AddAutomationField(dataItem.Fields, "AutomationLink", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.Filename, -1);
								AddAutomationField(dataItem.Fields, "AutomationDocumentFolderId", Artifact.ArtifactFieldTypeEnum.Lookup, "", projectAttachment.ProjectAttachmentFolderId);
								AddAutomationField(dataItem.Fields, "AutomationVersion", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.CurrentVersion, -1);
							}

							string testScript = "";
							FileStream fileStream = attachmentManager.OpenById(attachmentId);

							//Extract the data from the stream in byte form
							byte[] attachmentBytes = new byte[fileStream.Length];
							fileStream.Read(attachmentBytes, 0, (int)fileStream.Length);

							//Convert into UTF8 text
							try
							{
								testScript = UnicodeEncoding.UTF8.GetString(attachmentBytes);

								//If this is a Rapise ssTest XML file, parse it and display useful information
								if (!String.IsNullOrEmpty(projectAttachment.Filename) && projectAttachment.Filename.EndsWith(GlobalFunctions.REPOSITORY_TEST_EXTENSION))
								{
									try
									{
										XmlDocument xmlRapiseTest = new XmlDocument();
										xmlRapiseTest.LoadXml(testScript);
										string scriptPath = "";
										if (xmlRapiseTest.SelectSingleNode("Test/ScriptPath") != null)
										{
											scriptPath = xmlRapiseTest.SelectSingleNode("Test/ScriptPath").InnerText;
										}
										string userFunctionsPath = "";
										if (xmlRapiseTest.SelectSingleNode("Test/UserFunctionsPath") != null)
										{
											userFunctionsPath = xmlRapiseTest.SelectSingleNode("Test/UserFunctionsPath").InnerText;
										}
										string objectsPath = "";
										if (xmlRapiseTest.SelectSingleNode("Test/ObjectsPath") != null)
										{
											objectsPath = xmlRapiseTest.SelectSingleNode("Test/ObjectsPath").InnerText;
										}
										testScript = Resources.Main.TestCaseDetails_ProjectFile + ": " + projectAttachment.Filename + "\r\n";
										testScript += Resources.Main.TestCaseDetails_ScriptPath + ": " + scriptPath + "\r\n";
										testScript += Resources.Main.TestCaseDetails_UserFunctionsPath + ": " + userFunctionsPath + "\r\n";
										testScript += Resources.Main.TestCaseDetails_ObjectsPath + ": " + objectsPath + "\r\n";
									}
									catch (XmlException exception)
									{
										Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
									}
								}
							}
							catch (Exception)
							{
								testScript = Resources.Messages.TestCaseDetails_UnableToConvertToText;
							}

							AddAutomationField(dataItem.Fields, "AutomationScript", Artifact.ArtifactFieldTypeEnum.Text, testScript, -1);
						}
						else
						{
							//We have a linked test script
							string fileTypeIcon = "Filetypes/Link.svg";
							AddAutomationField(dataItem.Fields, "AutomationFileIcon", Artifact.ArtifactFieldTypeEnum.Text, fileTypeIcon, -1);
							AddAutomationField(dataItem.Fields, "AutomationDocumentId", Artifact.ArtifactFieldTypeEnum.Lookup, Resources.Fields.View_File, projectAttachment.AttachmentId);
							AddAutomationField(dataItem.Fields, "AutomationType", Artifact.ArtifactFieldTypeEnum.Text, "linked", -1);
							AddAutomationField(dataItem.Fields, "AutomationLink", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.Filename, -1);
							AddAutomationField(dataItem.Fields, "AutomationScript", Artifact.ArtifactFieldTypeEnum.Text, "", -1);
							AddAutomationField(dataItem.Fields, "AutomationDocumentFolderId", Artifact.ArtifactFieldTypeEnum.Lookup, "", projectAttachment.ProjectAttachmentFolderId);
							AddAutomationField(dataItem.Fields, "AutomationVersion", Artifact.ArtifactFieldTypeEnum.Text, projectAttachment.CurrentVersion, -1);
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
		/// Adds an automation field to the standard 'shape' and populates the field
		/// </summary>
		/// <param name="fields">The existing collection of fields</param>
		/// <param name="fieldName">The new field name</param>
		/// <param name="textValue">The new field text value</param>
		/// <param name="intValue">The new field int value</param>
		/// <param name="editable">Is the field editable</param>
		/// <param name="fieldType">The type of field</param>
		/// <param name="hidden">Is the field hidden</param>
		/// <param name="required">Is teh field required</param>
		private void AddAutomationField(JsonDictionaryOfFields fields, string fieldName, Artifact.ArtifactFieldTypeEnum fieldType, string textValue, int intValue, string tooltip = "", bool hidden = false, bool required = false, bool editable = true)
		{
			DataItemField dataItemField = new DataItemField();
			fields.Add(fieldName, dataItemField);
			dataItemField.FieldName = fieldName;
			dataItemField.FieldType = fieldType;
			dataItemField.TextValue = textValue;
			dataItemField.IntValue = intValue;
			dataItemField.Hidden = hidden;
			dataItemField.Required = required;
			dataItemField.Editable = editable;
			dataItemField.Tooltip = tooltip;
		}

		/// <summary>
		/// Creates a new testCase and returns it to the form
		/// </summary>
		/// <param name="artifactId">The id of the existing test case we were on</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new testCase</returns>
		public override int? Form_New(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the existing artifact and get its folder to insert in
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase;
				int? folderId = null;
				try
				{
					testCase = testCaseManager.RetrieveById2(projectId, artifactId);
					folderId = testCase.TestCaseFolderId;
				}
				catch (ArtifactNotExistsException)
				{
					//Ignore, leave indent level as null;
				}

                //Get the project settings collection
                ProjectSettings projectSettings = null;
                if (projectId > 0)
                {
                    projectSettings = new ProjectSettings(projectId);
                }

                //Now we need to create the testCase and then navigate to it
                int testCaseId = testCaseManager.Insert(
					userId,
					projectId,
					userId,
					null,
					"",
					null,
					null,
					TestCase.TestCaseStatusEnum.Draft,
					null,
					folderId,
					null,
					null,
					null,
					true,
                    projectSettings != null ? projectSettings.Testing_CreateDefaultTestStep : false
                    );
				testCase = testCaseManager.RetrieveById2(projectId, testCaseId);

				//We now need to populate the appropriate default custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);
				if (testCase != null)
				{
					//If the artifact custom property row is null, create a new one and populate the defaults
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
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
				return testCaseId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Clones the current test case and returns the ID of the item to redirect to
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Clone(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Clone";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to create the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now we need to copy the test case into the current folder and then navigate to it
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase = testCaseManager.RetrieveById2(projectId, artifactId);
				int newTestCaseId = testCaseManager.TestCase_Copy(userId, projectId, testCase.TestCaseId, testCase.TestCaseFolderId);

				return newTestCaseId;
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
		/// Deletes the current test case and returns the ID of the item to redirect to (if any)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <returns>The id to redirect to</returns>
		public override int? Form_Delete(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Form_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to delete the item
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Look through the current dataset to see what is the next test case in the list
				//If we are the last one on the list then we need to simply use the one before
				int? newTestCaseId = null;

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Get the current folder
				int? folderId = null;
				int nodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (nodeId > 0)
				{
					folderId = nodeId;
				}

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_CASE_PAGINATION_SIZE);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 500;
				int currentPage = 1;
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
				{
					paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];
				}
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
				{
					currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];

				}
				//Get the number of testCases in the project
				TestCaseManager testCaseManager = new TestCaseManager();
				int artifactCount = testCaseManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				//Get the testCases list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}

				List<TestCaseView> testCaseNavigationList = testCaseManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				bool matchFound = false;
				int previousTestCaseId = -1;
				foreach (TestCaseView testCase in testCaseNavigationList)
				{
					int testTestCaseId = testCase.TestCaseId;
					if (testTestCaseId == artifactId)
					{
						matchFound = true;
					}
					else
					{
						//If we found a match on the previous iteration, then we want to this (next) testCase
						if (matchFound)
						{
							newTestCaseId = testTestCaseId;
							break;
						}

						//If this matches the current testCase, set flag
						if (testTestCaseId == artifactId)
						{
							matchFound = true;
						}
						if (!matchFound)
						{
							previousTestCaseId = testTestCaseId;
						}
					}
				}
				if (!newTestCaseId.HasValue && previousTestCaseId != -1)
				{
					newTestCaseId = previousTestCaseId;
				}

				//Next we need to delete the current test-set
				testCaseManager.MarkAsDeleted(userId, projectId, artifactId);
				return newTestCaseId;
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

		/// <summary>Saves a single test case data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The test case to save</param>
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
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited is OK, we check that later)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the test case id
			int testCaseId = dataItem.PrimaryKey;

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

				//This service only supports updates, so we should get a test case id that is valid

				//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
				TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);

				//Make sure the user is authorized for this item if they only have limited permissions
				int? ownerId = null;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Create a new artifact custom property row if one doesn't already exist
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, false, customProperties);
				if (artifactCustomProperty == null)
				{
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
				}
				else
				{
					artifactCustomProperty.StartTracking();
				}

				//For saving, need to use the current status and type of the dataItem which may be different to the one retrieved
				int currentStatusId = (dataItem.Fields["TestCaseStatusId"].IntValue.HasValue) ? dataItem.Fields["TestCaseStatusId"].IntValue.Value : -1;
				int originalStatusId = testCase.TestCaseStatusId;
				int testCaseTypeId = (dataItem.Fields["TestCaseTypeId"].IntValue.HasValue) ? dataItem.Fields["TestCaseTypeId"].IntValue.Value : -1;

				//Get the list of workflow fields and custom properties
				int workflowId;
				if (testCaseTypeId < 1)
				{
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TestCaseWorkflowId;
				}
				else
				{
					workflowId = workflowManager.Workflow_GetForTestCaseType(testCaseTypeId);
				}
				List<TestCaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, currentStatusId);
				List<TestCaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, currentStatusId);

				//Convert the workflow lists into the type expected by the ListServiceBase function
				List<WorkflowField> workflowFields2 = TestCaseWorkflowManager.ConvertFields(workflowFields);
				List<WorkflowCustomProperty> workflowCustomProps2 = TestCaseWorkflowManager.ConvertFields(workflowCustomProps);

				//If the workflow status changed, check to see if we need a digital signature and if it was provided and is valid
				if (currentStatusId != originalStatusId)
				{
					 
					RequestForApprovalsIfRequired(projectId, testCaseId, originalStatusId, currentStatusId, workflowId);

					if (originalStatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested && (currentStatusId == (int)TestCaseManager.TestCaseSignatureStatus.Draft) ||
						originalStatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested && (currentStatusId == (int)TestCaseManager.TestCaseSignatureStatus.Review))
					{
						CancelTestCaseWorkflow(testCaseId);
					}

					//Only attempt to verify signature requirements if we have no concurrency date or if the client side concurrency matches that from the DB
					bool shouldVerifyDigitalSignature = true;
					if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
					{
						DateTime concurrencyDateTimeValue;
						if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
						{
							shouldVerifyDigitalSignature = testCase.ConcurrencyDate == concurrencyDateTimeValue;
						}
					}

					if (shouldVerifyDigitalSignature)
					{
						bool? valid = VerifyDigitalSignature(workflowId, originalStatusId, currentStatusId, signature, testCase.AuthorId, testCase.OwnerId);
						if (valid.HasValue)
						{
							if (valid.Value)
							{
								//Add the meaning to the artifact so that it can be recorded
								testCase.SignatureMeaning = signature.Meaning;
								UpdateTestCaseWorkflowStatus(testCaseId, currentStatusId, signature.Meaning);
								dataItem.Fields["TestCaseStatusId"].IntValue = DetermineWorkflowStatus(testCaseId, currentStatusId, originalStatusId);
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
						testCase.ConcurrencyDate = concurrencyDateTimeValue;
						testCase.AcceptChanges();
					}
				}

				//Now we can start tracking any changes
				testCase.StartTracking();

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				fieldsToIgnore.Add("NewComment");
				fieldsToIgnore.Add("Comments");
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("LastUpdateDate");
				fieldsToIgnore.Add("ReleaseId");
				fieldsToIgnore.Add("ExecutionDate");
				fieldsToIgnore.Add("IsAttachments");
				fieldsToIgnore.Add("IsTestSteps");

				//The special automation fields that are not part of the main entity
				fieldsToIgnore.Add("AutomationChanged");
				fieldsToIgnore.Add("AutomationType");
				fieldsToIgnore.Add("AutomationLink");
				fieldsToIgnore.Add("AutomationDocumentTypeId");
				fieldsToIgnore.Add("AutomationDocumentFolderId");
				fieldsToIgnore.Add("AutomationVersion");
				fieldsToIgnore.Add("AutomationScript");

				//Need to handle any data-mapping fields (project-admin only)
				if (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin)
				{
					DataMappingManager dataMappingManager = new DataMappingManager();
					List<DataSyncArtifactMapping> artifactMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId);
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
				UpdateFields(validationMessages, dataItem, testCase, customProperties, artifactCustomProperty, projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, fieldsToIgnore, workflowFields2, workflowCustomProps2);

				//Check to see if a comment was required and if so, verify it was provided. It's not handled as part of 'UpdateFields'
				//because there is no Comments field on the Test Case entity
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

				//Prevent empty filenames being saved if automation id specified
				//Do it now because otherwise the test case itself is saved
				//We do a secondary check later on
				if (dataItem.Fields.ContainsKey("AutomationLink") && String.IsNullOrEmpty(dataItem.Fields["AutomationLink"].TextValue) &&
					dataItem.Fields.ContainsKey("AutomationEngineId") && dataItem.Fields["AutomationEngineId"].IntValue.HasValue && dataItem.Fields["AutomationEngineId"].IntValue > 0)
				{
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "AutomationLink";
					newMsg.Message = Resources.Messages.TestCaseDetails_NeedAutomationFilename;
					AddUniqueMessage(validationMessages, newMsg);
				}

				//If we have validation messages, stop now
				if (validationMessages.Count > 0)
				{
					return validationMessages;
				}

				//Clone the Test Case, CustomPropertyData..
				Artifact notificationArtifact = testCase.Clone();
				ArtifactCustomProperty notificationCustomProps = artifactCustomProperty.Clone();

				//Update the test case and any custom properties
				try
				{
					testCaseManager.Update(testCase, userId);
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

				//See if we have a new comment encoded in the list of fields
				string newComment = "";
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					newComment = dataItem.Fields["NewComment"].TextValue;

					if (!String.IsNullOrWhiteSpace(newComment))
					{
						new DiscussionManager().Insert(userId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, newComment, DateTime.UtcNow, projectId, false, false);
					}
				}

				//See if any of the automation information changed
				bool automationScriptChanged = false;
				if (dataItem.Fields.ContainsKey("AutomationChanged") && dataItem.Fields["AutomationChanged"].TextValue == "true")
				{
					automationScriptChanged = true;
				}
				//See if we have to update the test automation information
				bool automationInfoChanged = false;
				int? automationEngineId = null;
				if (dataItem.Fields["AutomationEngineId"].IntValue > 0)
				{
					automationEngineId = dataItem.Fields["AutomationEngineId"].IntValue;
				}
				if (automationEngineId == null && testCase.AutomationEngineId.HasValue)
				{
					automationInfoChanged = true;
				}
				if (automationEngineId != null)
				{
					if (!testCase.AutomationEngineId.HasValue)
					{
						automationInfoChanged = true;
					}
					else if (automationEngineId.Value != testCase.AutomationEngineId)
					{
						automationInfoChanged = true;
					}
				}

				string urlOrFilename = dataItem.Fields["AutomationLink"].TextValue.Trim();
				string automationType = dataItem.Fields["AutomationType"].TextValue.Trim();
				byte[] binaryData = null;
				if (automationType == "attached")
				{
					string script = dataItem.Fields["AutomationScript"].TextValue;
					binaryData = UnicodeEncoding.UTF8.GetBytes(script);
				}
				int? documentFolderId = null;
				int? documentTypeId = null;
				if (dataItem.Fields["AutomationDocumentFolderId"].IntValue > 0)
				{
					documentFolderId = dataItem.Fields["AutomationDocumentFolderId"].IntValue;
				}
				if (dataItem.Fields["AutomationDocumentTypeId"].IntValue > 0)
				{
					documentTypeId = dataItem.Fields["AutomationDocumentTypeId"].IntValue;
				}

				string version = dataItem.Fields["AutomationVersion"].TextValue.Trim();
				if (String.IsNullOrEmpty(version))
				{
					version = "1.0";
				}

				//See if any of the attachment fields changed
				bool isRepositoryTest = false;
				if (!testCase.AutomationAttachmentId.HasValue)
				{
					automationInfoChanged = true;
				}
				else
				{
					AttachmentManager attachmentManager = new AttachmentManager();
					ProjectAttachmentView existingAttachment = attachmentManager.RetrieveForProjectById2(projectId, testCase.AutomationAttachmentId.Value);
					if (urlOrFilename != existingAttachment.Filename)
					{
						automationInfoChanged = true;
					}
					if (version != existingAttachment.CurrentVersion)
					{
						automationInfoChanged = true;
					}
					if (automationScriptChanged)
					{
						automationInfoChanged = true;
					}
					if (documentFolderId.HasValue && documentFolderId != existingAttachment.ProjectAttachmentFolderId)
					{
						automationInfoChanged = true;
					}
					if (documentTypeId.HasValue && documentTypeId != existingAttachment.DocumentTypeId)
					{
						automationInfoChanged = true;
					}
					if (automationType == "attached" && existingAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
					{
						automationInfoChanged = true;
					}
					if (automationType == "linked" && existingAttachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
					{
						automationInfoChanged = true;
					}

					//See if we have Rapise/SmarteStudio as they use 'repository' files
					if (!String.IsNullOrEmpty(urlOrFilename) && urlOrFilename.EndsWith(GlobalFunctions.REPOSITORY_TEST_EXTENSION))
					{
						isRepositoryTest = true;
					}
				}

				//Prevent empty filenames being saved
				if (String.IsNullOrEmpty(urlOrFilename) && automationEngineId.HasValue)
				{
					automationInfoChanged = false;
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "AutomationLink";
					newMsg.Message = Resources.Messages.TestCaseDetails_NeedAutomationFilename;
					AddUniqueMessage(validationMessages, newMsg);
				}

				if (automationInfoChanged)
				{
					//If this is a repository test then only the document folder should be updated in Spira
					if (isRepositoryTest)
					{
						if (documentTypeId.HasValue)
						{
							AttachmentManager attachmentManager = new AttachmentManager();
							ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, testCase.AutomationAttachmentId.Value);
							projectAttachment.StartTracking();
							projectAttachment.DocumentTypeId = documentTypeId.Value;
							attachmentManager.Update(projectAttachment, userId);
						}
					}
					else
					{
						testCaseManager.AddUpdateAutomationScript(
							userId,
							projectId,
							testCaseId,
							automationEngineId,
							urlOrFilename,
							"",
							binaryData,
							version,
							documentTypeId,
							documentFolderId
							);
					}
				}

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArtifact, notificationCustomProps, newComment);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + testCase.ArtifactToken);
				}

				//If we're asked to save and create a new test case, need to do the insert and send back the new id
				if (operation == "new")
				{
                    //Get the project settings collection
                    ProjectSettings projectSettings = null;
                    if (projectId > 0)
                    {
                        projectSettings = new ProjectSettings(projectId);
                    }

                    //Get the values from the existing test case that we want to set on the new one (not status)
                    //Now we need to create a new test case in the same folder and then navigate to it
                    int newTestCaseId = testCaseManager.Insert(
						userId,
						projectId,
						userId,
						ownerId,
						"",
						null,
						testCase.TestCaseTypeId,
						TestCase.TestCaseStatusEnum.Draft,
						testCase.TestCasePriorityId,
						testCase.TestCaseFolderId,
						null,
						null,
						null,
						true,
                        projectSettings != null ? projectSettings.Testing_CreateDefaultTestStep : false
                        );

					//We need to populate any custom property default values
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, newTestCaseId, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = newTestCaseId.ToString();
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(String.Format(Resources.Messages.TestCaseService_TestCaseNotFound, testCaseId));
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
		/// <param name="creatorId">The creator of the testCase</param>
		/// <param name="ownerId">The owner of the testCase</param>
		/// <returns>True for a valid signature, Null if no signature required and False if invalid signature</returns>
		protected bool? VerifyDigitalSignature(int workflowId, int originalStatusId, int currentStatusId, Signature signature, int creatorId, int? ownerId)
		{
			const string METHOD_NAME = "VerifyDigitalSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();
				TestCaseWorkflowTransition workflowTransition = testCaseWorkflowManager.WorkflowTransition_RetrieveByStatuses(workflowId, originalStatusId, currentStatusId);
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
				workflowTransition = testCaseWorkflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransition.WorkflowTransitionId);
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
		/// <param name="typeId">The id of the current task type</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="stepId">The id of the current step/status</param>
		/// <returns>The list of workflow states only</returns>
		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			const string METHOD_NAME = "Form_RetrieveWorkflowFieldStates";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				List<DataItemField> dataItemFields = new List<DataItemField>();

				//Get the list of artifact fields and custom properties
				ArtifactManager artifactManager = new ArtifactManager();
				List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveWorkflowConfigurable(Artifact.ArtifactTypeEnum.TestCase);
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

				//Get the list of workflow fields and custom properties for the specified type and step
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
				int workflowId = workflowManager.Workflow_GetForTestCaseType(typeId);
				List<TestCaseWorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, stepId);
				List<TestCaseWorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, stepId);

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
		/// Retrieves the list of comments associated with a test case
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the test case</param>
		/// <returns>The list of comments</returns>
		public List<CommentItem> Comment_Retrieve(int projectId, int artifactId)
		{
			const string METHOD_NAME = "Comment_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Create the new list of comments
				List<CommentItem> commentItems = new List<CommentItem>();

				//Get the test case (to verify permissions) and also the comments
				TestCaseManager testCaseManager = new TestCaseManager();
				UserManager userManager = new UserManager();
				DiscussionManager discussion = new DiscussionManager();
				TestCaseView testCase = testCaseManager.RetrieveById(projectId, artifactId);
				List<IDiscussion> comments = discussion.Retrieve(artifactId, Artifact.ArtifactTypeEnum.TestCase).ToList();

				//Make sure the user is either the owner or author if limited permissions
				int ownerId = -1;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we're sorting ascending or descending
				SortDirection sortDirection = (SortDirection)GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)SortDirection.Descending);

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
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Update the setting
				SortDirection sortDirection = (SortDirection)sortDirectionId;
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_COMMENTS_SORT_DIRECTION, (int)sortDirectionId);
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
		/// <param name="artifactId">The id of the test case</param>
		public void Comment_Delete(int projectId, int artifactId, int commentId)
		{
			const string METHOD_NAME = "Comment_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Delete the comment, making sure we have permissions
				DiscussionManager discussion = new DiscussionManager();
				IDiscussion comment = discussion.RetrieveById(commentId, Artifact.ArtifactTypeEnum.TestCase);
				//If the comment no longer exists do nothing
				if (comment != null && !comment.IsPermanent)
				{
					if (comment.CreatorId == userId || (SpiraContext.Current != null && SpiraContext.Current.IsProjectAdmin))
					{
						discussion.DeleteDiscussionId(commentId, Artifact.ArtifactTypeEnum.TestCase);
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
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view the item (limited access is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Make sure we're allowed to add comments
			if (IsAuthorizedToAddComments(projectId) == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Add the comment
				string cleanedComment = GlobalFunctions.HtmlScrubInput(comment);
				DiscussionManager discussion = new DiscussionManager();
				int commentId = discussion.Insert(userId, artifactId, Artifact.ArtifactTypeEnum.TestCase, cleanedComment, projectId, false, true);

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

        #region ITestCaseService Methods

        /// <summary>
        /// Returns the data for the project group test execution status graph
        /// </summary>
        /// <param name="activeReleasesOnly">do we only want the active releases' information</param>
        /// <param name="projectGroupId">The id of the project group</param>
        /// <returns></returns>
        public List<GraphEntry> TestCase_RetrieveGroupExecutionSummary(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "TestCase_RetrieveGroupExecutionSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized for this group
			ProjectGroupManager projectGroupManager = new ProjectGroupManager();
			if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the execution status list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				List<TestCase_ExecutionStatusSummary> executionStatiSummary = new TestCaseManager().RetrieveExecutionStatusSummary(projectGroupId, activeReleasesOnly);
				if (executionStatiSummary != null)
				{
					foreach (TestCase_ExecutionStatusSummary entry in executionStatiSummary)
					{
						if (entry.StatusCount.HasValue)
						{
							GraphEntry graphEntry = new GraphEntry();
							graphEntry.Name = entry.ExecutionStatusId.ToString();
							graphEntry.Caption = entry.ExecutionStatusName;
							graphEntry.Count = entry.StatusCount.Value;
							graphEntry.Color = TestCaseManager.GetExecutionStatusColor(entry.ExecutionStatusId);
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

		///// <summary>
		///// Returns the data for the test execution status graph
		///// </summary>
		///// <param name="projectId">The id of the project</param>
		///// <param name="releaseId">The id of the release (optional)</param>
		///// <returns></returns>
		//public List<GraphEntry> TestCase_RetrieveExecutedSummary(int projectId)
		//{
		//	const string METHOD_NAME = "TestCase_RetrieveExecutedSummary";

		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	//Make sure we're authenticated
		//	if (!CurrentUserId.HasValue)
		//	{
		//		throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
		//	}
		//	int userId = CurrentUserId.Value;

		//	//Make sure we're authorized to view test cases
		//	Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestCase);
		//	if (authorizationState == Project.AuthorizationState.Prohibited)
		//	{
		//		throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
		//	}

		 
		//	List<GraphEntry> graphEntries = new List<GraphEntry>();
		//	try
		//	{
		//		//Now get the execution status list
	
		//		Dictionary<string, int> executionStatiSummary = new TestCaseManager().RetrieveTestPreparationSummary(projectId);
		//		if (executionStatiSummary != null)
		//		{
		//			foreach (var entry in executionStatiSummary)
		//			{

		//				GraphEntry graphEntry = new GraphEntry();
		//				graphEntry.Name = entry.Key;
		//				graphEntry.Caption = entry.Key;
		//				graphEntry.Count = entry.Value;
		//				graphEntry.Color = TestCaseManager.GetExecutionStatusColor(2);
		//				graphEntries.Add(graphEntry);

		//			}
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		Logger.Flush();
		//		return graphEntries;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		throw;
		//	}
		//}

		/// <summary>
		/// Returns the data for the test execution status graph
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns></returns>
		public List<GraphEntry> TestCase_RetrieveExecutionSummary(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "TestCase_RetrieveExecutionSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view test cases
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now get the execution status list
				List<GraphEntry> graphEntries = new List<GraphEntry>();
				List<TestCase_ExecutionStatusSummary> executionStatiSummary = new TestCaseManager().RetrieveExecutionStatusSummary(projectId, releaseId);
				if (executionStatiSummary != null)
				{
					foreach (TestCase_ExecutionStatusSummary entry in executionStatiSummary)
					{
						if (entry.StatusCount.HasValue)
						{
							GraphEntry graphEntry = new GraphEntry();
							graphEntry.Name = entry.ExecutionStatusId.ToString();
							graphEntry.Caption = entry.ExecutionStatusName;
							graphEntry.Count = entry.StatusCount.Value;
							graphEntry.Color = TestCaseManager.GetExecutionStatusColor(entry.ExecutionStatusId);
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

		public GraphData TestCase_RetrieveTestPreparationStatusSummary(int projectId)
		{
			const string METHOD_NAME = "TestCase_RetrieveTestPreparationStatusSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			GraphData data = new GraphData();

			try
			{
				GraphData graphData = new GraphData();

				DataSet dataSet = new TestCaseManager().RetrieveTestPreparationSummary(projectId);
				DataTable dataTable = dataSet.Tables[0];
				graphData.Categories = (from DataRow row in dataTable.Rows select row["NAME"].ToString()).ToList();

				for (int columnIndex = 1; columnIndex < dataTable.Columns.Count; columnIndex++)
				{
					DataSeries series = new DataSeries();
					series.Name = dataTable.Columns[columnIndex].ColumnName;
					series.IntegerValues = (from DataRow row in dataTable.Rows select Int32.Parse(row[columnIndex].ToString())).ToList();


					graphData.Series.Add(series);
				};

				return graphData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Save the list of parameters for the specified test case
		/// </summary>
		/// <param name="projectId">The ID of the current project</param>
		/// <param name="parameters">The list of new parameters to save</param>
		/// <param name="testCaseId">The id of the test case</param>
		/// <returns>Any error messages if the parameter cannot be added</returns>
		public void SaveParameters(int projectId, int testCaseId, List<DataItem> parameters)
		{
			const string METHOD_NAME = "SaveParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to modify test cases
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//First retrieve the current parameters
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseParameter> existingParameters = testCaseManager.RetrieveParameters(testCaseId);

				//Loop through the new parameters and see if we need to add or update
				foreach (DataItem parameter in parameters)
				{
					if (parameter.Fields.ContainsKey("Name"))
					{
						int testCaseParameterId = parameter.PrimaryKey;
						string parameterName = parameter.Fields["Name"].TextValue.Trim();
						string defaultValue = null;
						if (parameter.Fields.ContainsKey("DefaultValue"))
						{
							defaultValue = parameter.Fields["DefaultValue"].TextValue;
						}

						//See if we have a name or id match (since we don't want to delete/recreate if the name changed)
						//However new ones may not have an id
						TestCaseParameter existingParameter = existingParameters.FirstOrDefault(p => p.Name == parameterName || p.TestCaseParameterId == testCaseParameterId);
						if (existingParameter == null)
						{
							//Add the new parameter
							testCaseManager.InsertParameter(projectId, testCaseId, parameterName, defaultValue);
						}
						else
						{
							testCaseManager.UpdateParameter(projectId, existingParameter.TestCaseParameterId, parameterName, defaultValue);
						}
					}
				}

				//Finally do any deletes
                bool cannotDeleteAll = false;
				List<int> parametersToDelete = new List<int>();
				foreach (TestCaseParameter existingParameter in existingParameters)
				{
					//Check on both name and ID
					bool match = ((parameters.Any(p => p.Fields.ContainsKey("Name") && p.Fields["Name"].TextValue.Trim() == existingParameter.Name)) || parameters.Any(p => p.PrimaryKey == existingParameter.TestCaseParameterId));
                    if (!match)
					{
                        //Check the parameter is not in use on a test set
                        TestSetManager testSetManager = new TestSetManager();
                        List<TestSetParameter> testSetsWithParameter = testSetManager.RetrieveParameterValuesByParameter(existingParameter.TestCaseParameterId);

                        if (testSetsWithParameter.Count == 0 || testSetsWithParameter == null)
                        {
                            parametersToDelete.Add(existingParameter.TestCaseParameterId);
                        }
                        else
                        {
                            //If in use by a test set then give a helpful message
                            cannotDeleteAll = true;
                        }

					}
				}

				foreach (int testCaseParameterId in parametersToDelete)
				{
					testCaseManager.DeleteParameter(projectId, testCaseParameterId);
				}

                if (cannotDeleteAll)
                {
                    throw new DataValidationException(Resources.Messages.TestCaseDetails_ParameterInUse);
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
		/// Retrieves a list of test case parameters that are directly (not inherited) defined for a specific test case
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <returns>The list of parameter data objects</returns>
		/// <param name="includeAlreadySet">Do we want to include those from child test cases that already have a value set</param>
		/// <param name="includeInherited">Do we want to include those from child test cases</param>
		public List<DataItem> RetrieveParameters(int testCaseId, bool includeInherited, bool includeAlreadySet)
		{
			const string METHOD_NAME = "RetrieveParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the array of data items to store the parameter values
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of non-inherited parameters
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(testCaseId, includeInherited, includeAlreadySet);

				//Populate the data items list
				foreach (TestCaseParameter testCaseParameter in testCaseParameters)
				{
					//The data item itself
					DataItem dataItem = new DataItem();
					dataItem.PrimaryKey = testCaseParameter.TestCaseParameterId;
					dataItems.Add(dataItem);

					//The Name field
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = testCaseParameter.Name;
					dataItem.Fields.Add("Name", dataItemField);

					//The Token field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Token";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					dataItemField.TextValue = TestCaseManager.CreateParameterToken(testCaseParameter.Name);
					dataItem.Fields.Add("Token", dataItemField);

					//The DefaultValue field
					dataItemField = new DataItemField();
					dataItemField.FieldName = "DefaultValue";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					if (String.IsNullOrEmpty(testCaseParameter.DefaultValue))
					{
						dataItemField.TextValue = null;
					}
					else
					{
						dataItemField.TextValue = testCaseParameter.DefaultValue;
					}
					dataItem.Fields.Add("DefaultValue", dataItemField);
				}

				return dataItems;
			}
			catch (EntityInfiniteRecursionException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of test folders for the current project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>Just the fields needed for a lookup / dropdown list</returns>
		public JsonDictionaryOfStrings RetrieveTestFolders(int projectId)
		{
			const string METHOD_NAME = "RetrieveTestFolders";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (limited is OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				JsonDictionaryOfStrings lookupValues = ConvertLookupValues(testCaseFolders.OfType<Entity>().ToList(), "TestCaseFolderId", "Name", "IndentLevel");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return lookupValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IListService Methods

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

			try
			{
				//See which operation we have and handle accordingly
				if (operation == "SelectRelease")
				{
					//The value contains the id of the release we want to select
					//We need to capture the release and put it in the project setting
					if (value == "")
					{
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
					}
					else
					{
						int releaseId = Int32.Parse(value);
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
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
		/// Handles custom list operations used by the test case list screen, specifically adding test cases
		/// to releases or test sets
		/// </summary>
		/// <param name="operation">
		/// The operation being executed:
		///     TestSet - adds test cases to the selected test set
		///     Release - adds test cases to the selected release
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
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			try
			{
				//See which operation we have
				if (operation == "Release")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and add to the release, ignore any duplicate exceptions
					int releaseId = destId;
					TestCaseManager testCaseManager = new TestCaseManager();
					List<int> testCaseIds = new List<int>();
					foreach (string item in items)
					{
						int testCaseId = Int32.Parse(item);
						testCaseIds.Add(testCaseId);
					}
					//Now save the mappings
					testCaseManager.AddToRelease(projectId, releaseId, testCaseIds, userId);
				}
				else if (operation == "ReleaseRemove")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Release);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and remove from the release
					int releaseId = destId;
					TestCaseManager testCaseManager = new TestCaseManager();
					List<int> testCaseIds = new List<int>();
					foreach (string item in items)
					{
						int testCaseId = Int32.Parse(item);
						testCaseIds.Add(testCaseId);
					}
					//Now save the mappings
					testCaseManager.RemoveFromRelease(projectId, releaseId, testCaseIds, userId);
				}
				else if (operation == "Requirement")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Requirement);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and add to the requirement, handle any exceptions gracefully
					int requirementId = destId;
					TestCaseManager testCaseManager = new TestCaseManager();
					List<int> testCaseIds = new List<int>();
					foreach (string item in items)
					{
						int testCaseId = Int32.Parse(item);
						testCaseIds.Add(testCaseId);
					}
					//Now save the mappings
					testCaseManager.AddToRequirement(projectId, requirementId, testCaseIds, userId);
				}
				else if (operation == "TestSet")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestSet);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and add to the test set, ignore any duplicate position exceptions
					int testSetId = destId;

					//Make sure we were passed a test set not a test set folder
					if (testSetId < 1)
					{
						return Resources.Messages.TestCaseService_CannotAddToTestSetFolder;
					}

					//Convert to ints
					List<int> testCaseIds = items.Select(t => Int32.Parse(t)).Where(t => t > 0).ToList();
					List<int> testCaseFolderIds = items.Select(t => -Int32.Parse(t)).Where(t => t > 0).ToList();

					TestSetManager testSetManager = new TestSetManager();
					try
					{
						testSetManager.AddTestCases(projectId, testSetId, testCaseIds, null, null);
						foreach (int testCaseFolderId in testCaseFolderIds)
						{
							testSetManager.AddTestFolder(projectId, testSetId, testCaseFolderId, null, null);
						}
					}
					catch (TestSetDuplicateTestCasePositionException)
					{
						//Return a message
						return Resources.Messages.TestCaseService_DuplicatePositionException;
					}
				}
				else if (operation == "Block")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestCase);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and block them
					TestCaseManager testCaseManager = new TestCaseManager();
					foreach (string item in items)
					{
						int testCaseId = Int32.Parse(item);
						try
						{
							testCaseManager.Block(userId, projectId, testCaseId);
						}
						catch (ArtifactNotExistsException)
						{
							//Ignore
						}
					}
				}
				else if (operation == "UnBlock")
				{
					//Make sure we're authorized
					Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestCase);
					if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
					{
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					//Iterate through all the passed in test cases and unblock them
					TestCaseManager testCaseManager = new TestCaseManager();
					foreach (string item in items)
					{
						int testCaseId = Int32.Parse(item);
						try
						{
							testCaseManager.UnBlock(userId, projectId, testCaseId);
						}
						catch (ArtifactNotExistsException)
						{
							//Ignore
						}
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

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we have a custom property (they need to be handled differently)
				if (CustomPropertyManager.IsFieldCustomProperty(fieldName).HasValue)
				{
					//Toggle the status of the appropriate custom property
					CustomPropertyManager customPropertyManager = new CustomPropertyManager();
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCase, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCase, fieldName);
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
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="testCaseId">The id of the test case/folder to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		public string RetrieveNameDesc(int? projectId, int testCaseId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the test case business object
				TestCaseManager testCaseManager = new TestCaseManager();

				//Now retrieve the specific test case/folder - handle quietly if it doesn't exist
				try
				{
					//See if we have a test case or folder
					string tooltip = "";
					if (testCaseId < 0)
					{
						//Test folder IDs are negative
						int testFolderId = -testCaseId;

						TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);

						//See if we have any parent folders
						List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseFolder.ProjectId, testCaseFolder.TestCaseFolderId, false);
						foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
						{
							tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
						}

						tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testCaseFolder.Name) + "</u>";
						if (!String.IsNullOrEmpty(testCaseFolder.Description))
						{
							tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCaseFolder.Description);
						}
					}
					else
					{
						//First we need to get the test case itself
						TestCaseView testCaseView = testCaseManager.RetrieveById(null, testCaseId);

						//Next we need to get the list of successive parent folders
						if (testCaseView.TestCaseFolderId.HasValue)
						{
							List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseView.ProjectId, testCaseView.TestCaseFolderId.Value, true);
							foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
							{
								tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(parentFolder.Name) + "</u> &gt; ";
							}
						}

						//Now we need to get the test case itself
						tooltip += "<u>" + Microsoft.Security.Application.Encoder.HtmlEncode(testCaseView.Name) + " " + GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, testCaseId, true) + "</u>";
						if (!String.IsNullOrEmpty(testCaseView.Description))
						{
							tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCaseView.Description);
						}

						//See if we have any comments to append
						IEnumerable<IDiscussion> comments = new DiscussionManager().Retrieve(testCaseId, Artifact.ArtifactTypeEnum.TestCase, false);
						if (comments.Count() > 0)
						{
							IDiscussion lastComment = comments.Last();
							tooltip += String.Format("<br /><i>{0} - {1} ({2})</i>",
								GlobalFunctions.LocalizeDate(lastComment.CreationDate).ToShortDateString(),
								GlobalFunctions.HtmlRenderAsPlainText(lastComment.Text),
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
					//This is the case where the client still displays the test case, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test case");
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

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Change the width of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCase, fieldName, width);
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
		/// Changes the order of columns in the test case list
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="fieldName">The name of the column being moved</param>
		/// <param name="newIndex">The new index of the column's position</param>
		public override void List_ChangeColumnPosition(int projectId, string fieldName, int newIndex)
		{
			const string METHOD_NAME = "List_ChangeColumnPosition";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//The field position may be different to the index because index is zero-based
				int newPosition = newIndex + 1;

				//Toggle the status of the appropriate artifact field or custom property
				ArtifactManager artifactManager = new ArtifactManager();
				artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCase, fieldName, newPosition);
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
		/// Returns a list of pagination options that the user can choose from
		/// </summary>
		/// <returns>A dictionary of pagination options (numeric value = key, display value = value)</returns>
		public JsonDictionaryOfStrings RetrievePaginationOptions(int projectId)
		{
			const string METHOD_NAME = "RetrievePaginationOptions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			//Delegate to the generic method in the base class - passing the correct collection name
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, PROJECT_SETTINGS_PAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		#endregion

		#region IAssociationPanelService

		/// <summary>
		/// Creates a new test case from the requirement or test set from the release
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactId">The id of the requirement/release</param>
		/// <param name="artifactTypeId">Whether we have a requirement or release</param>
		/// <param name="selectedItems">Any selected items (used in some cases)</param>
		/// <param name="folderId">The id of any folder chosen (used in some cases)</param>
		/// <returns>Any error messages, or null string for success</returns>
		public string AssociationPanel_CreateNewLinkedItem(int projectId, int artifactId, int artifactTypeId, List<int> selectedItems, int? folderId)
		{
			const string METHOD_NAME = "CreateNewMappedItem";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Handle the release and requirement cases appropriately
			if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Requirement)
			{
				try
				{
					//Create a new test case from this requirement, and put in the selected folder (if any)
					int newTestCaseId = 0;
					TestCaseManager testCaseManager = new TestCaseManager();
					newTestCaseId = testCaseManager.CreateFromRequirement(userId, projectId, artifactId, folderId);

					//Handle notifications
					if (newTestCaseId > 0)
					{
						//Retrieve the new test case to pass to notification manager
						TestCase notificationArt = testCaseManager.RetrieveById2(projectId, newTestCaseId);
						((TestCase)notificationArt).MarkAsAdded();
						try
						{
							new NotificationManager().SendNotificationForArtifact(notificationArt);
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Test Case.");
						}
					}
				}
				catch (ArtifactNotExistsException exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					return "Unable to locate the requirement, it may have been deleted.";
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					return "Unable to create new test case, please check the server Event Log";
				}
			}

			if (artifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Release)
			{
				try
				{
					//Create a new test set from this release
					int newTestSetId = 0;
					TestSetManager testSetManager = new TestSetManager();
					newTestSetId = testSetManager.CreateFromRelease(projectId, artifactId, userId);

					//Handle notifications
					if (newTestSetId > 0)
					{
						//Retrieve the new test set to pass to notification manager
						TestSet notificationArt = testSetManager.RetrieveById2(projectId, newTestSetId);
						((TestSet)notificationArt).MarkAsAdded();
						try
						{
							new NotificationManager().SendNotificationForArtifact(notificationArt);
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Test Set.");
						}
					}
				}
				catch (ArtifactNotExistsException exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					return "Unable to locate the release, it may have been deleted.";
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					return "Unable to create new test set, please check the server Event Log";
				}
			}
			return "";
		}

		#endregion

		#region IItemSelector Methods

		/// <summary>
		/// Returns a list of test cases (just the basic name/id fields) for using in popup item selection dialog boxes
		/// </summary>
		/// <remarks>
		/// Does not return test case folders
		/// </remarks>
		/// <param name="projectId">The current project</param>
		/// <param name="standardFilters">Any standard filters (e.g. the folder)</param>
		public ItemSelectorData ItemSelector_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
		{
			const string METHOD_NAME = "ItemSelector_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Instantiate the test case business object
				TestCaseManager testCaseManager = new TestCaseManager();

				//Create the array of data items
				ItemSelectorData itemSelectorData = new ItemSelectorData();
				List<DataItem> dataItems = itemSelectorData.Items;

				//See if we have a folder selected (otherwise root folder)
				int? folderId = null;
				if (standardFilters != null && standardFilters.Count > 0)
				{
					Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
					if (deserializedFilters.ContainsKey("TestCaseFolderId") && deserializedFilters["TestCaseFolderId"] is Int32)
					{
						folderId = (int)deserializedFilters["TestCaseFolderId"];
					}
				}

				//Get the test case list for the folder
				List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

				//Iterate through all the test cases and populate the dataitem (only some columns are needed)
				foreach (TestCaseView testCase in testCases)
				{
					//Create the data-item
					DataItem dataItem = new DataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = testCase.TestCaseId;

					//Test Case Id
					DataItemField dataItemField = new DataItemField();
					dataItemField.FieldName = "Id";
					dataItemField.IntValue = testCase.TestCaseId;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//Name/Desc
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = testCase.Name;
					dataItem.Alternate = testCase.IsTestSteps;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//Status
					dataItemField = new DataItemField();
					dataItemField.FieldName = "StatusId";
					dataItemField.FieldType = Artifact.ArtifactFieldTypeEnum.Lookup;
					dataItemField.IntValue = testCase.TestCaseStatusId;
					dataItemField.TextValue = testCase.TestCaseStatusName;
					dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

					//Add to the items collection
					dataItems.Add(dataItem);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return itemSelectorData;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region IFilteredListService Methods

		/// <summary>
		/// Updates the filters stored in the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The array of filters (name,value)</param>
		public string UpdateFilters(int projectId, JsonDictionaryOfStrings filters, int? displayTypeId)
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

			bool isInitialFilter = false;
			string result = base.UpdateFilters(userId, projectId, filters, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestCase, out isInitialFilter);

			return result;
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

			return base.SaveFilter(userId, projectId, name, DataModel.Artifact.ArtifactTypeEnum.TestCase, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, isShared, existingSavedFilterId, includeColumns);
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

			//Delegate to the generic implementation
			return base.RetrieveFilters(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, includeShared);
		}

		#endregion

		#region ITreeViewService Methods

		/// <summary>
		/// Deletes a testCase folder
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="nodeId">The node id of the folder to be deleted</param>
		public void TreeView_DeleteNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_DeleteNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have test case delete)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			int testCaseFolderId = 0;
			if (Int32.TryParse(nodeId, out testCaseFolderId) && testCaseFolderId > 0)
			{
				try
				{
					//Delete the specified folder
					TestCaseManager testCaseManager = new TestCaseManager();
					testCaseManager.TestCaseFolder_Delete(projectId, testCaseFolderId, userId);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
		}

		/// <summary>
		/// Returns the parent node (if any) of the current node
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="nodeId">The node we're interested in</param>
		/// <returns>The parent node</returns>
		public string TreeView_GetParentNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_GetParentNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			int testCaseFolderId = 0;
			if (Int32.TryParse(nodeId, out testCaseFolderId) && testCaseFolderId > 0)
			{
				try
				{
					string parentNodeId = "";
					//Get the parent of the specified folder
					TestCaseManager testCaseManager = new TestCaseManager();
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
					if (testCaseFolder != null && testCaseFolder.ParentTestCaseFolderId.HasValue)
					{
						parentNodeId = testCaseFolder.ParentTestCaseFolderId.ToString();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return parentNodeId;
				}
				catch (ArtifactNotExistsException)
				{
					return "";
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>Called when test cases or folders are dropped onto a folder in the treeview</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="userId">The current user</param>
		/// <param name="artifactIds">The ids of the testCases</param>
		/// <param name="nodeId">The id of the folder</param>
		public void TreeView_DragDestination(int projectId, int[] artifactIds, int nodeId)
		{
			const string METHOD_NAME = "TreeView_DragDestination";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to modify testCases (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the folder id (or root if -1)
				int? folderId = null;
				if (nodeId > 0)
				{
					folderId = nodeId;
				}

				//Make sure the folder exists
				TestCaseManager testCaseManager = new TestCaseManager();
				if (folderId.HasValue)
				{
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(folderId.Value);
					if (testCaseFolder == null)
					{
						//Folder does not exist
						return;
					}
				}

				//Get the list of folders, not needed if moving to root
				List<TestCaseFolderHierarchyView> testCaseFolders = null;
				if (folderId.HasValue)
				{
					testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				}

				//Retrieve each artifact (test case or folder) in the list and move to the specified folder
				foreach (int artifactId in artifactIds)
				{
					//See if we have a folder or test case
					if (artifactId > 0)
					{
						//Test Case
						int testCaseId = artifactId;
						testCaseManager.TestCase_UpdateFolder(testCaseId, folderId);
					}
					if (artifactId < 0)
					{
						//Test Folder
						int testFolderId = -artifactId;

						TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);
						if (testCaseFolder != null)
						{
							//Check to make sure we're not making it's parent either this folder
							//or one of its children
							if (folderId.HasValue && testCaseFolders != null && testCaseFolders.Count > 0)
							{
								string folderIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == testFolderId).IndentLevel;
								string newParentIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == folderId.Value).IndentLevel;

								if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
								{
									//Throw a meaningful exception
									throw new InvalidOperationException(Resources.Messages.TestCasesService_CannotMoveFolderUnderItself);
								}
							}

							//Move the test folder, need to make sure we don't create an infinite loop
							testCaseFolder.StartTracking();
							testCaseFolder.ParentTestCaseFolderId = folderId;
							testCaseManager.TestCaseFolder_Update(testCaseFolder);
						}
					}
				}
			}
			catch (ArtifactNotExistsException)
			{
				//Fail quietly
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns the tooltip for a node (used if not provided when node created)</summary>
		/// <param name="nodeId">The id of the node (test case folder)</param>
		/// <returns>The tooltip</returns>
		public string TreeView_GetNodeTooltip(string nodeId)
		{
			if (String.IsNullOrEmpty(nodeId))
			{
				return "";
			}

			int testFolderId;
			if (Int32.TryParse(nodeId, out testFolderId))
			{
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);

                if (testCaseFolder != null)
                {
                    string tooltip = "";
                    //See if we have any parent folders
                    List<TestCaseFolderHierarchyView> parentFolders = testCaseManager.TestCaseFolder_GetParents(testCaseFolder.ProjectId, testCaseFolder.TestCaseFolderId, false);
                    foreach (TestCaseFolderHierarchyView parentFolder in parentFolders)
                    {
                        tooltip += "<u>" + parentFolder.Name + "</u> &gt; ";
                    }

                    tooltip += "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testCaseFolder.Name) + "</u>";
                    if (!String.IsNullOrEmpty(testCaseFolder.Description))
                    {
                        tooltip += "<br />\n" + GlobalFunctions.HtmlRenderAsPlainText(testCaseFolder.Description);
                    }
                    return tooltip;
                }
			}
			return null;
		}

		/// <summary>Returns the list of testCase folders contained in a parent node</summary>
		/// <param name="userId">The current user</param>
		/// <param name="parentId">The id of the parent folder</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of treeview nodes to display</returns>
		public List<TreeViewNode> TreeView_GetNodes(int projectId, string parentId)
		{
			const string METHOD_NAME = "TreeView_GetNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view testCases (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				List<TreeViewNode> nodes = new List<TreeViewNode>();

				//Get the list of project testCase folders from the business object
				TestCaseManager testCaseManager = new TestCaseManager();

				//See if we need the root folder (folderId = 0)
				if (String.IsNullOrEmpty(parentId))
				{
					nodes.Add(new TreeViewNode(0.ToString(), Resources.Main.Global_Root, null));
				}
				else
				{
					int? parentFolderId = null;
					if (!String.IsNullOrEmpty(parentId))
					{
						parentFolderId = Int32.Parse(parentId);
						if (parentFolderId == 0)
						{
							//We want the direct children of the root, so set to NULL
							parentFolderId = null;
						}
					}
					List<TestCaseFolder> testCaseFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, parentFolderId);

					foreach (TestCaseFolder testCaseFolder in testCaseFolders)
					{
						nodes.Add(new TreeViewNode(testCaseFolder.TestCaseFolderId.ToString(), testCaseFolder.Name, testCaseFolder.Description));
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return nodes;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Sets the currently selected node so that it can be persisted for future page loads</summary>
		/// <param name="nodeId">The id of the node to persist</param>
		/// <param name="projectId">The id of the project</param>
		public void TreeView_SetSelectedNode(int projectId, string nodeId)
		{
			const string METHOD_NAME = "TreeView_SetSelectedNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view testCases (limited view insufficient)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//We simply store this in a project setting
				int folderId = -1;  //Default to root folder
				if (!String.IsNullOrEmpty(nodeId))
				{
					int nodeIdInt;
					if (Int32.TryParse(nodeId, out nodeIdInt) && nodeIdInt > 0)
					{
						folderId = nodeIdInt;
					}
				}
				SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Gets a comma-separated list of parent nodes that are to be expanded based on the selected node stored in the project settings collection. Used when the page is first loaded or when refresh is clicked</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the project</param>
		public List<string> TreeView_GetExpandedNodes(int projectId)
		{
			const string METHOD_NAME = "TreeView_GetExpandedNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			try
			{
				List<string> nodeList = new List<string>();
				//Get the currently selected node (if there is one)
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId == -1)
				{
					//We have the root node selected
					nodeList.Insert(0, 0.ToString());
				}
				else
				{
					//Get the list of all folders in the project and locate the selected item
					TestCaseManager testCaseManager = new TestCaseManager();
					List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
					TestCaseFolderHierarchyView testCaseFolder = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == selectedNodeId);

					//Now iterate through successive parents to get the folder path
					while (testCaseFolder != null)
					{
						nodeList.Insert(0, testCaseFolder.TestCaseFolderId.ToString());
						Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Added node : " + testCaseFolder.TestCaseFolderId + " to list");
						if (testCaseFolder.ParentTestCaseFolderId.HasValue)
						{
							testCaseFolder = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == testCaseFolder.ParentTestCaseFolderId.Value);
						}
						else
						{
							testCaseFolder = null;
						}
					}

					//Finally add the root folder
					nodeList.Insert(0, 0.ToString());
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return nodeList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets all the test case folders in the treeview as a simple hierarchical lookup dictionary
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The datasource for the dropdown hierarchy control</returns>
		public JsonDictionaryOfStrings TreeView_GetAllNodes(int projectId)
		{
			const string METHOD_NAME = "TreeView_GetAllNodes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			try
			{
				//Get the list of all folders in the project
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);

				//Convert to the necessary lookup
				JsonDictionaryOfStrings testCaseFolderDic = ConvertLookupValues(testCaseFolders.OfType<Entity>().ToList(), "TestCaseFolderId", "Name", "IndentLevel");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return testCaseFolderDic;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Adds a new node to the tree
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="name">The name of the new node</param>
		/// <param name="description">The description of the node</param>
		/// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
		/// <returns>The id of the new node</returns>
		public string TreeView_AddNode(int projectId, string name, string parentNodeId, string description)
		{
			const string METHOD_NAME = "TreeView_AddNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have test case create)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				int? parentTestCaseFolderId = null;
				if (!String.IsNullOrEmpty(parentNodeId))
				{
					int intValue;
					if (Int32.TryParse(parentNodeId, out intValue))
					{
						parentTestCaseFolderId = intValue;
					}
					else
					{
						throw new FaultException(Resources.Messages.TestCasesService_TestCaseFolderIdNotInteger);
					}
				}

				if (String.IsNullOrWhiteSpace(name))
				{
					throw new FaultException(Resources.Messages.TestCasesService_TestCaseFolderNameRequired);
				}
				else
				{
					//Add the new folder and return the new node id
					TestCaseManager testCaseManager = new TestCaseManager();
					int newTestCaseFolderId = testCaseManager.TestCaseFolder_Create(name.Trim().SafeSubstring(0, 255), projectId, description, parentTestCaseFolderId).TestCaseFolderId;

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return newTestCaseFolderId.ToString();
				}

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates an existing node in the tree
		/// </summary>
		/// <param name="nodeId">The id of the node to update</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="name">The name of the new node</param>
		/// <param name="parentNodeId">The id of the parent node to add it under (optional)</param>
		/// <param name="description">the description of the node</param>
		public void TreeView_UpdateNode(int projectId, string nodeId, string name, string parentNodeId, string description)
		{
			const string METHOD_NAME = "TreeView_UpdateNode";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (need to have test case bulk edit)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			int testCaseFolderId = 0;
			if (Int32.TryParse(nodeId, out testCaseFolderId) && testCaseFolderId > 0)
			{
				try
				{
					int? parentTestCaseFolderId = null;
					if (!String.IsNullOrEmpty(parentNodeId))
					{
						int intValue;
						if (Int32.TryParse(parentNodeId, out intValue))
						{
							parentTestCaseFolderId = intValue;
						}
						else
						{
							throw new FaultException(Resources.Messages.TestCasesService_TestCaseFolderIdNotInteger);
						}
					}

					if (String.IsNullOrWhiteSpace(name))
					{
						throw new FaultException(Resources.Messages.TestCasesService_TestCaseFolderNameRequired);
					}
					else
					{
						//Update the existing folder (assuming that it exists)
						TestCaseManager testCaseManager = new TestCaseManager();
						TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderId);
						if (testCaseFolder != null)
						{
							testCaseFolder.StartTracking();
							testCaseFolder.Name = name.Trim().SafeSubstring(0, 255);
							testCaseFolder.Description = description;
							//Make sure you don't try and set a folder to be its own parent (!)
							if (!parentTestCaseFolderId.HasValue || parentTestCaseFolderId != testCaseFolderId)
							{
								testCaseFolder.ParentTestCaseFolderId = parentTestCaseFolderId;
							}
							testCaseManager.TestCaseFolder_Update(testCaseFolder);
						}

						Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
						Logger.Flush();
					}
				}
				catch (FolderCircularReferenceException)
				{
					throw new InvalidOperationException(Resources.Messages.TestCasesService_CannotMoveFolderUnderItself);
				}
				catch (ArtifactNotExistsException)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to update testCase folder '{0}' as it does not exist in the system ", nodeId));
					//Fail quietly
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Populates a data item from the entity
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="testFolder">The entity containing the data</param>
		protected void PopulateRow(SortedDataItem dataItem, TestCaseFolder testFolder)
		{
			//Set the primary key (negative for folders)
			dataItem.PrimaryKey = -testFolder.TestCaseFolderId;
			dataItem.Folder = true;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				//The execution status id is not a real field on folders, so special case
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (testFolder.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the entity
					PopulateFieldRow(dataItem, dataItemField, testFolder, null, null, false, PopulateEqualizer);
				}
			}
		}

		/// <summary>
		/// Populates a data item from the entity
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="testCaseView">The entity containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="workflowCustomProps">The workflow custom proeperty field states</param>
		/// <param name="workflowFields">The workflow standard field states</param>
		/// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		/// <param name="components">The list of project components</param>
		protected void PopulateRow(SortedDataItem dataItem, TestCaseView testCaseView, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty, List<Component> components, List<TestCaseWorkflowField> workflowFields = null, List<TestCaseWorkflowCustomProperty> workflowCustomProps = null)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = testCaseView.TestCaseId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, testCaseView.ConcurrencyDate);

			//Specify if it has an attachment or not
			dataItem.Attachment = testCaseView.IsAttachments;

			//Specify if it has test steps or not
			dataItem.Alternate = testCaseView.IsTestSteps;

			//Convert the workflow lists into the type expected by the ListServiceBase function
			List<WorkflowField> workflowFields2 = TestCaseWorkflowManager.ConvertFields(workflowFields);
			List<WorkflowCustomProperty> workflowCustomProps2 = TestCaseWorkflowManager.ConvertFields(workflowCustomProps);

			//Some fields are not editable for test cases
			List<string> readOnlyFields = new List<string>() { "CreationDate", "LastUpdateDate", "ActualDuration", "ExecutionDate", "IsTestSteps" };

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (testCaseView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the entity
					PopulateFieldRow(dataItem, dataItemField, testCaseView, customProperties, artifactCustomProperty, editable, PopulateEqualizer, workflowFields2, workflowCustomProps2, readOnlyFields);

					//Apply the conditional formatting to the priority column (if displayed) and execution status column (if displayed)
					if (dataItemField.FieldName == "TestCasePriorityId" && testCaseView.TestCasePriorityId.HasValue)
					{
						dataItemField.CssClass = "#" + testCaseView.TestCasePriorityColor;
					}

					//Add the component name(s) if specified
					if (components != null && dataItemField.FieldName == "ComponentIds" && !String.IsNullOrEmpty(testCaseView.ComponentIds))
					{
						List<int> componentIds = testCaseView.ComponentIds.FromDatabaseSerialization_List_Int32();
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
		/// Populates the 'shape' of the data item that will be used as a template for the retrieved data items
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="filterList">List of filters to be returned as first row (if appropriate)</param>
		/// <param name="returnJustListFields">Should we just return list fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, SortedDataItem dataItem, Hashtable filterList, bool returnJustListFields = true)
		{
			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(Artifact.ArtifactTypeEnum.TestCase, getLookupValues, projectId, projectTemplateId, userId, dataItem, filterList, PopulateEqualizerShape, returnJustListFields);
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
			if (fieldName == "ExecutionStatusId")
			{
				dataItemField.FieldName = "ExecutionStatusId";
				string filterLookupName = fieldName;
				dataItemField.Lookups = GetLookupValues(filterLookupName, projectId, projectTemplateId);
				//Set the filter value (if one is set)
				if (filterList != null && filterList.Contains(filterLookupName))
				{
					dataItemField.IntValue = (int)filterList[filterLookupName];
				}
			}
		}

		protected bool IsDataItemFieldAvailableForTransition(TestCaseWorkflowTransition workflowTransition, int testCaseId, int projectId)
		{
			if (!workflowTransition.IsSignatureRequired)
			{
				return true;
			}

			List<ProjectSignature> testCaseApprovers = new TestCaseWorkflowManager().GetApproversForTestCase(projectId);

			if (testCaseApprovers.Count() == 0)
			{
				return false;
			}

			var firstApprover = testCaseApprovers.First();
			bool isParallel = false;
			if (!testCaseApprovers.All(x => x.OrderId == firstApprover.OrderId))
			{
				isParallel = false;
			}

			var signatures = new TestCaseManager().GetTestCaseSignaturesForCurrentWorkflow(testCaseId);
			if (signatures.Count == 0)
				return true;

			if(signatures.Any(x => x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Approved && x.UserId == this.CurrentUserId))
			{
				return false;
			}

			if (isParallel)
			{
				if (signatures.Any(x => x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested && x.UserId == this.CurrentUserId))
				{
					return true;
				}
			}
			else
			{
				int currentUserIndex = testCaseApprovers.FindIndex(x => x.UserId == this.CurrentUserId);
				if (currentUserIndex == 0)
				{
					return true;
				}

				var prevApprover = testCaseApprovers[currentUserIndex - 1];
				if (signatures.Any(x => x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Approved && x.UserId == prevApprover.UserId))
				{
					return true;
				}
			}

			return false;
		}


		protected void RequestForApprovalsIfRequired(int projectId, int testCaseId, int originalStatusId, int currentStatusId, int workflowId)
		{
			TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();

			var approvalUsers = workflowManager.WorkflowTransition_RetrieveApproversByInputStatus(workflowId, currentStatusId);
			if(approvalUsers != null && approvalUsers.Count != 0)
			{
				new TestCaseManager().RequestApprovalForTestCase(projectId, testCaseId, approvalUsers.Where(x => x.OrderId.HasValue).Select(x => x.UserId).ToList());
			}
		}

		public void CancelTestCaseWorkflow(int testCaseId)
		{
			new TestCaseManager().CancelTestCaseApprovalWorkflow(testCaseId);
		}

		protected void UpdateTestCaseWorkflowStatus(int testCaseId, int currentStatus, string meaning)
		{
			if(currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Approved || 
				currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Rejected || currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Cancelled)
			{
				new TestCaseManager().UpdateTestCaseSignatureWorkflowState(testCaseId, this.CurrentUserId.Value, (TestCaseManager.TestCaseSignatureStatus)currentStatus, meaning);
			}
		}

		protected int DetermineWorkflowStatus(int testCaseId, int currentStatus, int prevStatus)
		{
			if (currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Approved)
			{
				var existingApprovals = new TestCaseManager().RetrieveExistingSignaturesForTestCase(testCaseId);
				if (existingApprovals.All(x => x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Approved))
				{
					return (int)TestCaseManager.TestCaseSignatureStatus.Approved;
				}
			}
			else if (currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Requested && prevStatus == (int)TestCaseManager.TestCaseSignatureStatus.Review)
			{
				return currentStatus;
			}
			else if (currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Draft && prevStatus == (int)TestCaseManager.TestCaseSignatureStatus.Requested)
			{
				return currentStatus;
			}

			//else if (currentStatus == (int)TestCaseManager.TestCaseSignatureStatus.Draft && prevStatus == (int)TestCaseManager.TestCaseSignatureStatus.Requested)
			//{
			//	return currentStatus;
			//}

			return prevStatus;

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
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				if (lookupName == "AuthorId" || lookupName == "OwnerId")
				{
					List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
					lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
				}
				if (lookupName == "TestCasePriorityId")
				{
					List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(priorities.OfType<Entity>().ToList(), "TestCasePriorityId", "Name");
				}
				if (lookupName == "TestCaseStatusId")
				{
					List<TestCaseStatus> stati = testCaseManager.RetrieveStatuses();
					lookupValues = ConvertLookupValues(stati.OfType<Entity>().ToList(), "TestCaseStatusId", "Name");
				}
				if (lookupName == "TestCaseTypeId")
				{
					List<TestCaseType> types = testCaseManager.TestCaseType_Retrieve(projectTemplateId);
					lookupValues = ConvertLookupValues(types.OfType<Entity>().ToList(), "TestCaseTypeId", "Name");
				}
				if (lookupName == "ComponentIds")
				{
					List<Component> components = new ComponentManager().Component_Retrieve(projectId);
					lookupValues = ConvertLookupValues(components.OfType<Entity>().ToList(), "ComponentId", "Name");
				}
				if (lookupName == "AutomationEngineId")
				{
					AutomationManager automationManager = new AutomationManager();
					List<AutomationEngine> automationEngines = automationManager.RetrieveEngines();
					lookupValues = ConvertLookupValues(automationEngines.OfType<Entity>().ToList(), "AutomationEngineId", "Name");
				}
				if (lookupName == "ExecutionStatusId")
				{
					List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
					lookupValues = ConvertLookupValues(executionStati.OfType<Entity>().ToList(), "ExecutionStatusId", "Name");
				}
				if (lookupName == "IsTestSteps" || lookupName == "IsSuspect")
				{
					lookupValues = new JsonDictionaryOfStrings(GlobalFunctions.YesNoList());
				}

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, customPropertyNumber.Value, true);
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
							List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
							lookupValues = ConvertLookupValues(users.OfType<Entity>().ToList(), "UserId", "FullName");
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
		/// Populates the equalizer type graph for the test case execution status field
		/// </summary>
		/// <param name="dataItem">The data item</param>
		/// <param name="dataItemField">The field being populated</param>
		/// <param name="artifact">The data row</param>
		protected void PopulateEqualizer(DataItem dataItem, DataItemField dataItemField, Artifact artifact)
		{
			//See if we have a test case or folder
			if (artifact is TestCaseView)
			{
				TestCaseView testCaseView = (TestCaseView)artifact;

				//Populate color/name/tooltip
				dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCaseView.ExecutionStatusId);
				dataItemField.TextValue = testCaseView.ExecutionStatusName;
				dataItemField.Tooltip = testCaseView.ExecutionStatusName;
			}
			else if (artifact is TestCaseFolder)
			{
				TestCaseFolder testCaseFolder = (TestCaseFolder)artifact;

				int passedCount = testCaseFolder.CountPassed;
				int failureCount = testCaseFolder.CountFailed;
				int cautionCount = testCaseFolder.CountCaution;
				int blockedCount = testCaseFolder.CountBlocked;
				int notRunCount = testCaseFolder.CountNotRun;
				int notApplicableCount = testCaseFolder.CountNotApplicable;

				//Calculate the percentages, handling rounding correctly
				int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount + notApplicableCount;
				int percentPassed = 0;
				int percentFailed = 0;
				int percentCaution = 0;
				int percentBlocked = 0;
				int percentNotRun = 0;
				int percentNotApplicable = 0;
				if (totalCount != 0)
				{
					//Need check to handle divide by zero case
					percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
					percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
					percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
					percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
					percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
					percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
				}

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

				//Add a tooltip to the cell for the raw data
				dataItemField.Tooltip = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();

				//Add the total count to the folder total count
				dataItem.ChildCount = totalCount;
			}
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
			UpdatePagination(projectId, pageSize, currentPage);
		}

		/// <summary>
		/// Returns a list of test cases for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">The indent level of the parent folder, or empty string for all items</param>
		/// <returns>List of test cases</returns>
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
			const string METHOD_NAME = CLASS_NAME + "NavigationBar_RetrieveList()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

			try
			{
				//Instantiate the business objects
				TestCaseManager testCaseManager = new TestCaseManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//See if we have a folder to filter by
				//-1 = root folder
				int? folderId = null;
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId != -1)
				{
					folderId = selectedNodeId;
				}

				//Now get the list of populated filters if appropriate
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST);

				//Get the sort information
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, PROJECT_SETTINGS_PAGINATION);
				paginationSettings.Restore();
				//Default values
				int paginationSize = 500;
				int currentPage = 1;
				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE] != null)
					paginationSize = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_PAGE_SIZE];

				if (paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] != null)
					currentPage = (int)paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE];

				//Get the number of testCases in the project
				int artifactCount = testCaseManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), countAllFolders: true, folderId: folderId);

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the requirements list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount) startRow = 1;

				List<TestCaseView> testCaseList = new List<TestCaseView>(); //Default to empty list

				if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.FilteredList)
				{
					//Filtered List
					if (authorizationState == Project.AuthorizationState.Limited)
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

					testCaseList = testCaseManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}
				else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.AllItems)
				{
					//All Items
					if (authorizationState == Project.AuthorizationState.Limited)
						throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);

					testCaseList = testCaseManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}
				else if (displayMode == (int)ServerControls.NavigationBar.DisplayModes.Assigned)
				{
					//Assigned to User
					testCaseList = testCaseManager.RetrieveByOwnerId(userId, projectId);
				}

				int pageCount = (int)Decimal.Ceiling((decimal)artifactCount / (decimal)paginationSize);

				//Make sure that the current page is not larger than the number of pages or less than 1
				if (currentPage > pageCount)
				{
					currentPage = pageCount;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}
				if (currentPage < 1)
				{
					currentPage = 1;
					paginationSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_NAVIGATION_CURRENT_PAGE] = currentPage;
					paginationSettings.Save();
				}

				//Populate the test cases
				PopulateTestCases(testCaseList, dataItems, "");

				Logger.LogExitingEvent(METHOD_NAME);
				return dataItems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Populates the list of testCases
		/// </summary>
		/// <param name="testCases">The testCase list</param>
		/// <param name="dataItems">The list of nav-bar items</param>
		/// <param name="folderIndentLevel">The requirement indent level (if there is one)</param>
		/// <param name="areTopLevel">Are these top-level (root) test cases</param>
		/// <returns>The last used indent level</returns>
		protected string PopulateTestCases(List<TestCaseView> testCases, List<HierarchicalDataItem> dataItems, string folderIndentLevel, bool areTopLevel = false)
		{
			//Iterate through all the testCases and populate the dataitem (only some columns are needed)
			string testCaseIndentLevel = (areTopLevel) ? folderIndentLevel : folderIndentLevel + "AAA"; //Add on to the req indent level
			foreach (TestCaseView testCase in testCases)
			{
				//Create the data-item
				HierarchicalDataItem dataItem = new HierarchicalDataItem();

				//Populate the necessary fields
				dataItem.PrimaryKey = testCase.TestCaseId;
				dataItem.Indent = testCaseIndentLevel;
				dataItem.Alternate = testCase.IsTestSteps;

				//Name/Desc
				DataItemField dataItemField = new DataItemField();
				dataItemField.FieldName = "Name";
				dataItemField.TextValue = testCase.Name;
				dataItem.Summary = false;
				dataItem.Fields.Add("Name", dataItemField);

				//Add to the items collection
				dataItems.Add(dataItem);

				//Increment the indent level
				testCaseIndentLevel = HierarchicalList.IncrementIndentLevel(testCaseIndentLevel);
			}

			return testCaseIndentLevel;
		}

		/// <summary>
		/// Updates the display settings used by the Navigation Bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="displayMode">The current display mode</param>
		/// <param name="displayWidth">The display width</param>
		/// <param name="minimized">Is the navigation bar minimized or visible</param>
		public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
		{
			const string METHOD_NAME = "NavigationBar_UpdateSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

			try
			{
				//Update the user's project settings
				bool changed = false;
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS);
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

		#region ISortedList Methods

		/// <summary>
		/// Allows sorted lists with folders to focus on a specific item and open its containing folder
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="artifactId">Id of a test case (or negative for a folder)</param>
		/// <returns>The id of the folder (if any)</returns>
		public override int? SortedList_FocusOn(int projectId, int artifactId, bool clearFilters)
		{
			const string METHOD_NAME = "SortedList_FocusOn";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized to view test cases
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//See if we have a folder or test case
				TestCaseManager testCaseManager = new TestCaseManager();
				if (artifactId > 0)
				{
					int testCaseId = artifactId;

					//Retrieve this test case
					TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
					if (testCase != null)
					{
						//Get the folder
						int folderId = (testCase.TestCaseFolderId.HasValue) ? testCase.TestCaseFolderId.Value : -1;

						//Unset the current filters and then set the current folder to this one
						bool isInitialFilter = false;
						string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestCase, out isInitialFilter);
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, folderId);
						return folderId;
					}
				}
				if (artifactId < 0)
				{
					int testFolderId = -artifactId;

					//Retrieve this test folder
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);
					if (testCaseFolder != null)
					{
						//Unset the current filters and then set the current folder to this one
						if (clearFilters)
						{
							bool isInitialFilter = false;
							string result = base.UpdateFilters(userId, projectId, null, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST, DataModel.Artifact.ArtifactTypeEnum.TestCase, out isInitialFilter);
						}
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, testFolderId);
						return testFolderId;
					}
				}
				return null;
			}
			catch (ArtifactNotExistsException)
			{
				//Ignore, do not log
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a set of test cases
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="items">The items to delete</param>
		public void SortedList_Delete(int projectId, List<string> items, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Iterate through all the items to be deleted
				TestCaseManager testCaseManager = new TestCaseManager();
				foreach (string item in items)
				{
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or test case
						if (artifactId > 0)
						{
							//Test Case
							int testCaseId = artifactId;

							//Delete the test case
							try
							{
								testCaseManager.MarkAsDeleted(userId, projectId, testCaseId);
							}
							catch (ArtifactNotExistsException)
							{
								//Ignore any errors due to deleting a folder and some of its children at the same time
							}
						}
						if (artifactId < 0)
						{
							//Test Folder
							int testFolderId = -artifactId;

							//Delete the folder
							try
							{
								testCaseManager.TestCaseFolder_Delete(projectId, testFolderId, userId);
							}
							catch (ArtifactNotExistsException)
							{
								//Ignore any errors due to deleting a folder and some of its children at the same time
							}
						}
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
		/// Inserts a new test case into the system
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifact">The type of artifact we're inserting (TestCase, Folder)</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <returns>The id of the new test case</returns>
		/// <remarks>Not used for folders</remarks>
		public int SortedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate business objects
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Get the current test case folder that we need to insert into
                //First, check if a folder was passed in via the filters
                int? passedInFolderId = null;
                if (standardFilters != null && standardFilters.Count > 0)
                {
                    Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
                    foreach (KeyValuePair<string, object> filter in deserializedFilters)
                    {
                        //See if we have the folder id passed through as a filter
                        if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                        {
                            passedInFolderId = (int)(filter.Value);
                        }
                    }
                }

                //See if we have a folder to filter by
                //-1 = root folder
                int? folderId = null;
                if (passedInFolderId.HasValue && passedInFolderId.Value > 0)
                {
                    // set the folder id and update the selected node (set to root folder if the folder does not exist)
                    int intValue = (int)(passedInFolderId.Value);
                    folderId = testCaseManager.TestCaseFolder_Exists(projectId, intValue) ? intValue : ManagerBase.NoneFilterValue;
                    this.TreeView_SetSelectedNode(projectId, folderId.ToString());
                }
                else
                {
                    int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNodeId > 0)
                    {
                        if (testCaseManager.TestCaseFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
                            folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder tasks only) and update the projectsetting
                            folderId = ManagerBase.NoneFilterValue;
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

                //Get the project settings collection
                ProjectSettings projectSettings = null;
                if (projectId > 0)
                {
                    projectSettings = new ProjectSettings(projectId);
                }

                //Simply insert the new item into the test case list
                int testCaseId = testCaseManager.Insert(
					userId,
					projectId,
					userId,
					null,
					"",
					null,
					null,
					TestCase.TestCaseStatusEnum.Draft,
					null,
                    folderId,
					null,
					null,
					null,
					true,
                    projectSettings != null ? projectSettings.Testing_CreateDefaultTestStep : false
					);

				//If we have a release selected, then automatically add the new test case to the release so that it's visible
				int releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
				if (releaseId != -1)
				{
					ReleaseManager releaseManager = new ReleaseManager();
					List<int> mappedReleases = new List<int>();
					mappedReleases.Add(releaseId);
					releaseManager.AddToTestCase(projectId, testCaseId, mappedReleases, userId);
				}

				//We now need to populate the appropriate default custom properties
				TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);
				if (testCase != null)
				{
					//If the artifact custom property row is null, create a new one and populate the defaults
					if (artifactCustomProperty == null)
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
						artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					//If we have filters currently applied to the view, then we need to set this new test case/folder to the same value
					//(if possible) so that it will show up in the list
					ProjectSettingsCollection filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST);
					if (filterList.Count > 0)
					{
						testCase.StartTracking();
						//We need to tell it to ignore any filtering by the ID, creation date since we cannot set that on a new item
						List<string> fieldsToIgnore = new List<string>() { "TestCaseId", "CreationDate", "IsSuspect", "IsTestSteps", "ExecutionStatusId" };
						UpdateToMatchFilters(projectId, filterList, testCaseId, testCase, artifactCustomProperty, fieldsToIgnore);
						testCaseManager.Update(testCase, userId);
					}

					//Save the custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of test cases in the system for the specific user/project
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">Any standard filters that need to be set</param>
		/// <returns>Collection of dataitems</returns>
		public SortedData SortedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the test case and custom property business objects
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Get the list of components (cannot use a lookup field since multilist). Include inactive, but not deleted
				List<Component> components = new ComponentManager().Component_Retrieve(projectId, false, false);

				//Create the array of data items (including the first filter item)
				SortedData sortedData = new SortedData();
				List<SortedDataItem> dataItems = sortedData.Items;

				//Now get the list of populated filters and the current sort
				Hashtable filterList = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST);
				string sortCommand = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");
				string sortProperty = sortCommand.Substring(0, sortCommand.IndexOf(" "));
				string sortDirectionString = sortCommand.Substring(sortCommand.IndexOf(" "), sortCommand.Length - sortCommand.IndexOf(" ")).Trim();
				bool sortAscending = (sortDirectionString == "ASC");

                //Add any standard filters
                int? folderId = null;
				if (standardFilters != null && standardFilters.Count > 0)
				{
					Dictionary<string, object> deserializedFilters = GlobalFunctions.DeSerializeCollection(standardFilters);
					foreach (KeyValuePair<string, object> filter in deserializedFilters)
					{
                        //See if we have the folder id passed through as a filter
                        if (filter.Key == GlobalFunctions.SPECIAL_FILTER_FOLDER_ID && filter.Value is Int32)
                        {
                            int intValue = (int)(filter.Value);
                            if (intValue > 0 && testCaseManager.TestCaseFolder_Exists(projectId, intValue))
                            {
                                folderId = intValue;
                            }
                        }
                        else
                        {
                            filterList[filter.Key] = filter.Value;
                        }
					}
                }
                else
                {
                    //See if we have a folder to filter on, not applied if we have a standard filter
                    //because those screens don't display the folders on the left-hand side

                    //-1 = no filter
                    //0 = root folder
                    int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                    if (selectedNodeId >= 1)
                    {
                        if (testCaseManager.TestCaseFolder_Exists(projectId, selectedNodeId))
                        {
                            //Filter by specific Folder
                            folderId = selectedNodeId;
                        }
                        else
                        {
                            //Set to the Root Folder (i.e. no folder items only) and update the projectsetting
                            SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
                        }
                    }
                }

				//Create the filter item first - we can clone it later
				SortedDataItem filterItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, filterItem, filterList);
				dataItems.Add(filterItem);
                sortedData.FilterNames = GetFilterNames(filterList, projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase);

                //Get the ProjectSettings collection
                ProjectSettings projectSettings = null;
                if (projectId > 0)
                {
                    projectSettings = new ProjectSettings(projectId);
                }
                //** WORX - Add the special WorX column if enabled
                if (projectSettings != null && projectSettings.Testing_WorXEnabled)
				{
					DataItemField worxField = new DataItemField();
					worxField.FieldName = "worx";
					worxField.Caption = "Worx";
					worxField.FieldType = Artifact.ArtifactFieldTypeEnum.Html;
					worxField.AllowDragAndDrop = true;
					filterItem.Fields.Add("worx", worxField);
				}

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

				//See if we are viewing for one release or all releases
				int releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

				//Make sure the release actually exists
				if (releaseId > 0)
				{
					try
					{
						Release release = new ReleaseManager().RetrieveById3(projectId, releaseId);
						if (release == null)
						{
							SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
						}
					}
					catch (ArtifactNotExistsException)
					{
						SaveProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
					}
				}

				//Get the number of test cases in the project or release
				int artifactCount = 0;
				if (releaseId > 0)
				{
					artifactCount = testCaseManager.CountByRelease(projectId, releaseId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}
				else
				{
					artifactCount = testCaseManager.Count(projectId, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);
				}

				//Calculate the number of pages
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
				sortedData.CurrPage = currentPage;
				sortedData.PageCount = pageCount;

				//**** Now we need to actually populate the rows of data to be returned ****

				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}
				sortedData.StartRow = startRow;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false, true);

				//The queries depend on whether we are filtering by project or not
				if (releaseId > 0)
				{
					//Get the test case list for the project or release
					List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(projectId, releaseId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

					//We also need to get and sub-folders in this folder
					List<TestCaseFolderReleaseView> testFolders = testCaseManager.TestCaseFolder_GetByParentIdForRelease(projectId, folderId, releaseId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

					//Get the visible count from the data rows returned
					sortedData.VisibleCount = testCases.Count;

					//Iterate through all the test folders and populate the data items
					foreach (TestCaseFolderReleaseView testFolder in testFolders)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, testFolder.ConvertTo<TestCaseFolderReleaseView, TestCaseFolder>());
						dataItems.Add(dataItem);
					}

					//Iterate through all the test cases and populate the dataitems
					foreach (TestCaseReleaseView testCaseView in testCases)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, testCaseView.ConvertTo<TestCaseReleaseView, TestCaseView>(), customProperties, false, null, components);
						dataItems.Add(dataItem);

						//WorX - populate Worx links
						if (projectSettings != null && projectSettings.Testing_WorXEnabled)
						{
							if (dataItem.Fields.ContainsKey("worx"))
							{
								DataItemField worxField = dataItem.Fields["worx"];
								worxField.TextValue =
									"<a href=\"worx:spira/open/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Open</a> | " +
									"<a href=\"worx:spira/review/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Review</a> | " +
									"<a href=\"worx:spira/execute/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Execute</a>";
							}
						}
					}

					//For test cases we need to also provide the visible count and total count of
					//actual test cases (not folders) with no filters for all folders
					sortedData.TotalCount = testCaseManager.CountByRelease(projectId, releaseId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), null, false, true);
				}
				else
				{
					//Get the test case list for the project or release
					List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, sortProperty, sortAscending, startRow, paginationSize, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset(), folderId);

					//We also need to get any sub-folders in this folder
					List<TestCaseFolder> testFolders = testCaseManager.TestCaseFolder_GetByParentId(projectId, folderId, sortProperty, sortAscending, filterList, GlobalFunctions.GetCurrentTimezoneUtcOffset());

					//Get the visible count from the data rows returned
					sortedData.VisibleCount = testCases.Count;

					//Iterate through all the test folders and populate the data items
					foreach (TestCaseFolder testFolder in testFolders)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, testFolder);
						dataItems.Add(dataItem);
					}

					//Iterate through all the test cases and populate the dataitems
					foreach (TestCaseView testCaseView in testCases)
					{
						//We clone the template item as the basis of all the new items
						SortedDataItem dataItem = filterItem.Clone();

						//Now populate with the data
						PopulateRow(dataItem, testCaseView, customProperties, false, null, components);
						dataItems.Add(dataItem);

						//WorX - populate Worx links
						if (projectSettings != null && projectSettings.Testing_WorXEnabled)
						{
							if (dataItem.Fields.ContainsKey("worx"))
							{
								DataItemField worxField = dataItem.Fields["worx"];
								worxField.TextValue =
									"<a href=\"worx:spira/open/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Open</a> | " +
									"<a href=\"worx:spira/review/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Review</a> | " +
									"<a href=\"worx:spira/execute/pr" + projectId + "/tc" + dataItem.PrimaryKey + "\">Execute</a>";
							}
						}
					}

					//For test cases we need to also provide the visible count and total count of
					//actual test cases (not folders) with no filters for all folders
					sortedData.TotalCount = testCaseManager.Count(projectId, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), null, false, true);
				}

				//Include the pagination info
				sortedData.PaginationOptions = RetrievePaginationOptions(projectId);

				//Display the sort information
				sortedData.SortProperty = sortProperty;
				sortedData.SortAscending = sortAscending;

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
		/// Returns the latest information on a single test case in the system
		/// </summary>
		/// <param name="artifactId">The id of the particular artifact we want to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A single dataitem object</returns>
		public SortedDataItem SortedList_Refresh(int projectId, int artifactId, int? displayTypeId)
		{
			const string METHOD_NAME = "Refresh";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the test case and custom property business objects
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the data item record (no filter items)
				SortedDataItem dataItem = new SortedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem, null);

				//Get the test case dataset record for the specific test case id
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, artifactId);

				//Make sure the user is authorized for this item
				if (authorizationState == Project.AuthorizationState.Limited && testCaseView.OwnerId != userId && testCaseView.AuthorId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, DataModel.Artifact.ArtifactTypeEnum.TestCase, true);

				//Finally populate the dataitem from the dataset
				if (testCaseView != null)
				{
					//See if we already have an artifact custom property row
					if (artifactCustomProperty != null)
					{
						PopulateRow(dataItem, testCaseView, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty, null);
					}
					else
					{
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false);
						PopulateRow(dataItem, testCaseView, customProperties, true, null, null);
					}

					//See if we are allowed to bulk edit status (template setting)
					ProjectTemplateSettings templateSettings = new ProjectTemplateSettings(projectTemplateId);
					if (!templateSettings.Workflow_BulkEditCanChangeStatus && dataItem.Fields.ContainsKey("TestCaseStatusId"))
					{
						dataItem.Fields["TestCaseStatusId"].Editable = false;
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

			//Call the base method with the appropriate settings collection
			return base.UpdateSort(userId, projectId, sortProperty, sortAscending, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS);
		}

		/// <summary>
		/// Updates a list of test cases in the system
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dataItems">The updated data records</param>
		/// <returns>Validation messages</returns>
		public List<ValidationMessage> SortedList_Update(int projectId, List<SortedDataItem> dataItems, int? displayTypeId)
		{
			const string METHOD_NAME = "SortedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.BulkEdit, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Iterate through each data item and make the updates
				TestCaseManager testCaseManager = new TestCaseManager();
				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);

				foreach (SortedDataItem dataItem in dataItems)
				{
					//Get the test case id
					int testCaseId = dataItem.PrimaryKey;

					//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
					TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					if (testCase != null)
					{
						//Need to set the original date of this record to match the concurrency date
						if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
						{
							DateTime concurrencyDateTimeValue;
							if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
							{
								testCase.ConcurrencyDate = concurrencyDateTimeValue;
								testCase.AcceptChanges();
							}
						}

						//Start Tracking Changes
						testCase.StartTracking();

						//Update the field values
						List<string> fieldsToIgnore = new List<string>();
						fieldsToIgnore.Add("CreationDate");
						fieldsToIgnore.Add("ConcurrencyDate");   //Breaks concurrency otherwise
						UpdateFields(validationMessages, dataItem, testCase, customProperties, artifactCustomProperty, projectId, testCaseId, 0, fieldsToIgnore);

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
							Artifact notificationArt = testCase.Clone();
							ArtifactCustomProperty notificationCust = artifactCustomProperty.Clone();

							//Persist to database
							try
							{
								testCaseManager.Update(testCase, userId);
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
								new NotificationManager().SendNotificationForArtifact(notificationArt, notificationCust, null);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Test Case #" + testCase.TestCaseId + ".");
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
		/// Copies a set of test cases/folders
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="items">The items to copy</param>
		public void SortedList_Copy(int projectId, List<string> items)
		{
			const string METHOD_NAME = "SortedList_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();

				//Get the current folder
				//-1 = root folder
				int? currentFolderId = null;
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId != -1)
				{
					currentFolderId = selectedNodeId;
				}

				//Get the list of folders, not needed if moving to root
				List<TestCaseFolderHierarchyView> testCaseFolders = null;
				if (currentFolderId.HasValue)
				{
					testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				}

				//Iterate through all the items to be copied
				foreach (string item in items)
				{
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or test case
						if (artifactId > 0)
						{
							//Test Case
							int testCaseId = artifactId;

							//Copy the single test case
							testCaseManager.TestCase_Copy(userId, projectId, testCaseId, currentFolderId);
						}
						if (artifactId < 0)
						{
							//Test Folder
							int testFolderId = -artifactId;

							//Check to make sure we're not making it's parent either this folder
							//or one of its children
							if (currentFolderId.HasValue && testCaseFolders != null && testCaseFolders.Count > 0)
							{
								string folderIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == testFolderId).IndentLevel;
								string newParentIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == currentFolderId.Value).IndentLevel;

								if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
								{
									//Throw a meaningful exception
									throw new DataValidationExceptionEx(CreateSimpleValidationMessage(Resources.Messages.TestCasesService_CannotMoveFolderUnderItself));
								}
							}

							//Copy the test case folder
							testCaseManager.TestCaseFolder_Copy(userId, projectId, testFolderId, currentFolderId);
						}
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
		/// Moves a test case/folder to be under the current folder
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="items">The items to move</param>
		public override void SortedList_Move(int projectId, List<string> items)
		{
			const string METHOD_NAME = "SortedList_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized (full modify needed)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();

				//Get the current folder
				//-1 = root folder
				int? currentFolderId = null;
				int selectedNodeId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID, -1);
				if (selectedNodeId != -1)
				{
					currentFolderId = selectedNodeId;
				}

				//Get the list of folders, not needed if moving to root
				List<TestCaseFolderHierarchyView> testCaseFolders = null;
				if (currentFolderId.HasValue)
				{
					testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				}

				//Iterate through all the items to be moved
				foreach (string item in items)
				{
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or test case
						if (artifactId > 0)
						{
							//Test Case
							int testCaseId = artifactId;

							//Move the single test case
							testCaseManager.TestCase_UpdateFolder(testCaseId, currentFolderId);
						}
						if (artifactId < 0)
						{
							//Test Folder
							int testFolderId = -artifactId;
							TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);
							if (testCaseFolder != null)
							{
								//Check to make sure we're not making it's parent either this folder
								//or one of its children
								if (currentFolderId.HasValue && testCaseFolders != null && testCaseFolders.Count > 0)
								{
									string folderIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == testFolderId).IndentLevel;
									string newParentIndent = testCaseFolders.FirstOrDefault(f => f.TestCaseFolderId == currentFolderId.Value).IndentLevel;

									if (newParentIndent.Length >= folderIndent.Length && newParentIndent.Substring(0, folderIndent.Length) == folderIndent)
									{
										//Throw a meaningful exception
										throw new DataValidationExceptionEx(CreateSimpleValidationMessage(Resources.Messages.TestCasesService_CannotMoveFolderUnderItself));
									}
								}

								//Move the test folder, need to make sure we don't create an infinite loop
								testCaseFolder.StartTracking();
								testCaseFolder.ParentTestCaseFolderId = currentFolderId;
								testCaseManager.TestCaseFolder_Update(testCaseFolder);
							}
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
		/// Exports a series of test cases/folders from one project to another
		/// </summary>
		/// <param name="items">The list of test cases/folders being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		public void SortedList_Export(int destProjectId, List<string> items)
		{
			const string METHOD_NAME = "SortedList_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!CurrentUserId.HasValue)
			{
				throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(destProjectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestCase);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				TestCaseManager testCaseManager = new TestCaseManager();

				//We need to keep track of the mapping between the new and old test cases in case
				//we have linked test cases that need to have the links maintained
				Dictionary<int, int> testCaseMapping = new Dictionary<int, int>();
				Dictionary<int, int> testFolderMapping = new Dictionary<int, int>();

				//Iterate through all the items to be exported
				foreach (string item in items)
				{
					int artifactId;
					if (Int32.TryParse(item, out artifactId))
					{
						//See if we have a folder or test case
						if (artifactId > 0)
						{
							//Test Case
							int testCaseId = artifactId;
							TestCaseView testCase = testCaseManager.RetrieveById(null, testCaseId);
							if (testCase != null)
							{
								//Export the single test case
								testCaseManager.TestCase_Export(userId, testCase.ProjectId, testCaseId, destProjectId, testCaseMapping);
							}
						}
						if (artifactId < 0)
						{
							//Test Folder
							int testFolderId = -artifactId;
							TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testFolderId);
							if (testCaseFolder != null)
							{
								//Export the whole folder
								testCaseManager.TestCaseFolder_Export(userId, testCaseFolder.ProjectId, testFolderId, destProjectId, testCaseMapping, testFolderMapping);
							}
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

		#endregion

		public List<HierarchicalDataItem> RetrieveAvailable(int projectId, int artifactId, int artifactTypeId, string indentLevel)
		{
			throw new NotImplementedException();
		}
	}
}
