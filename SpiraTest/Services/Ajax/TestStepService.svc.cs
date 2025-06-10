using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.ServiceModel.Activation;
using System.Collections;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using System.ServiceModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Communicates with the OrderedGrid AJAX component for displaying/updating the list of test steps in a test case
	/// </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TestStepService : ListServiceBase, ITestStepService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestStepService::";

		#region IFormService methods

		/// <summary>Returns a single test step data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current test step</param>
		/// <returns>A test step data item</returns>
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

			//Make sure we're authorized (limited OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the data item record (no filter items)
				OrderedDataItem dataItem = new OrderedDataItem();
				PopulateShape(projectId, projectTemplateId, CurrentUserId.Value, dataItem, false);

				//Get the test step for the specific test step id
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCaseByStepId(artifactId.Value);
				TestStepView testStep = testSteps.FirstOrDefault(s => s.TestStepId == artifactId);
				int displayPosition = testSteps.IndexOf(testStep) + 1;

				//Get the related test case
				TestCaseView testCase = testCaseManager.RetrieveById(projectId, testStep.TestCaseId);

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, true);

				//Make sure the user is authorized for this item (the owner is at the test case level)
				int ownerId = -1;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//See if we have any existing artifact custom properties for this row
				if (artifactCustomProperty == null)
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, true, false);
					PopulateRow(projectId, dataItem, testStep, customProperties, true, displayPosition, (ArtifactCustomProperty)null);
				}
				else
				{
					PopulateRow(projectId, dataItem, testStep, artifactCustomProperty.CustomPropertyDefinitions, true, displayPosition, artifactCustomProperty);
				}

				//Populate any data mapping values are not part of the standard 'shape'

				//Also need to return back a special field to denote if the user is the owner or creator of the artifact
				bool isArtifactCreatorOrOwner = (ownerId == userId || testCase.AuthorId == userId);
				dataItem.Fields.Add("_IsArtifactCreatorOrOwner", new DataItemField() { FieldName = "_IsArtifactCreatorOrOwner", TextValue = isArtifactCreatorOrOwner.ToDatabaseSerialization() });

				//Adding the test case name
				DataItemField testCaseNameField = new DataItemField();
				testCaseNameField.FieldName = "TestCase";
				testCaseNameField.IntValue = testCase.TestCaseId;
				testCaseNameField.TextValue = testCase.Name;
				dataItem.Fields.Add(testCaseNameField.FieldName, testCaseNameField);

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
		/// Deletes the current test step and returns the ID of the item to redirect to (if any)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Retrieve the test step by its id
				TestCaseManager testCaseManager = new TestCaseManager();

				//Need to load in the test steps
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCaseByStepId(artifactId);

				//Find the current step
				TestStepView testStep = testSteps.FirstOrDefault(s => s.TestStepId == artifactId);

				if (testStep == null)
				{
					//If the artifact doesn't exist, return null
					return null;
				}

				//Store the test case id
				int testCaseId = testStep.TestCaseId;

				//Make sure the test step is not locked due to its workflow status
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//First we need to determine which test step id to redirect the user to after the delete
				int? newTestStepId = null;
				//Look through the current dataset to see what is the next test step on the list
				//If we are the last one on the list then we need to simply use the one before
				bool matchFound = false;
				int previousTestStepId = -1;
				foreach (TestStepView testStepView in testSteps)
				{
					int testTestStepId = testStepView.TestStepId;
					if (testTestStepId == artifactId)
					{
						matchFound = true;
					}
					else
					{
						//If we found a match on the previous iteration, then we want to this (next) test step
						if (matchFound)
						{
							newTestStepId = testTestStepId;
							break;
						}

						//If this matches the current test step, set flag
						if (testTestStepId == artifactId)
						{
							matchFound = true;
						}
						if (!matchFound)
						{
							previousTestStepId = testTestStepId;
						}
					}
				}
				if (!newTestStepId.HasValue && previousTestStepId != -1)
				{
					newTestStepId = previousTestStepId;
				}

				//Delete the current test step
				testCaseManager.MarkStepAsDeleted(userId, testCaseId, artifactId);

				return newTestStepId;
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
		/// Creates a new test step and returns it to the form
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">the id of the previous test step we were on</param>
		/// <returns>The id of the new testStep</returns>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the test step and then navigate to it
				TestCaseManager testCaseManager = new TestCaseManager();
				TestStep testStep = testCaseManager.RetrieveStepById(projectId, artifactId);
				int? testStepId = null;
				if (testStep != null)
				{
					int newPosition = testStep.Position + 1;
					testStepId = testCaseManager.InsertStep(
						userId,
						testStep.TestCaseId,
						newPosition,
						"",
						"",
						null
						);

					//We now need to populate the appropriate default custom properties
					CustomPropertyManager customPropertyManager = new CustomPropertyManager();
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testStepId.Value, DataModel.Artifact.ArtifactTypeEnum.TestStep, true);
					if (testStep != null)
					{
						//If the artifact custom property row is null, create a new one and populate the defaults
						if (artifactCustomProperty == null)
						{
							List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);
							artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestStep, testStepId.Value, customProperties);
							artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
						}
						else
						{
							artifactCustomProperty.StartTracking();
						}

						//Save the custom properties
						customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testStepId;
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
		/// Clones the current test step and returns the ID of the item to redirect to
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState != Project.AuthorizationState.Authorized)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Now we need to copy the test step and then navigate to it
				TestCaseManager testCaseManager = new TestCaseManager();
				TestStep testStep = testCaseManager.RetrieveStepById(projectId, artifactId);
				if (testStep != null)
				{
					//Make sure the test step is not locked due to its workflow status
					if (!testCaseManager.AreTestStepsEditableInStatus(testStep.TestCaseId))
					{
						throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
					}

					int newTestStepId = testCaseManager.CopyTestStep(userId, projectId, testStep.TestCaseId, artifactId);

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return newTestStepId;
				}

				//The item does not exist, so return null
				return null;
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

		/// <summary>Saves a single test step data item</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="dataItem">The test step to save</param>
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

			//Make sure we're authorized (limited OK)
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the test step id
			int testStepId = dataItem.PrimaryKey;

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business classes
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Load the custom property definitions (once, not per artifact)
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);

				//This service only supports updates, so we should get a test step id that is valid

				//Retrieve the existing record - and make sure it still exists. Also retrieve the associated custom property record
				TestStep testStep = testCaseManager.RetrieveStepById(projectId, testStepId);

				//Make sure the test step is not locked due to its workflow status
				if (!testCaseManager.AreTestStepsEditableInStatus(testStep.TestCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Create a new artifact custom property row if one doesn't already exist
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, false, customProperties);
				if (artifactCustomProperty == null)
				{
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestStep, testStepId, customProperties);
				}
				else
				{
					artifactCustomProperty.StartTracking();
				}

				//Need to set the original date of this record to match the concurrency date
				//The value is already in UTC so no need to convert
				if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
				{
					DateTime concurrencyDateTimeValue;
					if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
					{
						testStep.ConcurrencyDate = concurrencyDateTimeValue;
						testStep.AcceptChanges();
					}
				}

				//Now we can start tracking any changes
				testStep.StartTracking();

				//Update the field values, tracking changes
				List<string> fieldsToIgnore = new List<string>();
				fieldsToIgnore.Add("CreationDate");
				fieldsToIgnore.Add("NewComment");
				fieldsToIgnore.Add("Position");
				fieldsToIgnore.Add("LastUpdateDate");

				//Update the field values
				UpdateFields(validationMessages, dataItem, testStep, customProperties, artifactCustomProperty, projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, fieldsToIgnore);

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

				//Update the test step and any custom properties
				try
				{
					testCaseManager.UpdateStep(testStep, userId);
				}
				catch (OptimisticConcurrencyException)
				{
					return CreateSimpleValidationMessage(Resources.Messages.Global_DataChangedBySomeoneElse);
				}
				catch (EntityForeignKeyException)
				{
					return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
				}
				customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

				//See if we have a new comment encoded in the list of fields
				string newComment = "";
				if (dataItem.Fields.ContainsKey("NewComment"))
				{
					newComment = dataItem.Fields["NewComment"].TextValue;

					if (!String.IsNullOrWhiteSpace(newComment))
					{
						new DiscussionManager().Insert(userId, testStepId, Artifact.ArtifactTypeEnum.TestStep, newComment, DateTime.UtcNow, projectId, false, false);
					}
				}

				//If we're asked to save and create a new test step, need to do the insert and send back the new id
				if (operation == "new")
				{
					int newPosition = testStep.Position + 1;
					int testCaseId = testStep.TestCaseId;
					int newTestStepId = testCaseManager.InsertStep(
						userId,
						testCaseId,
						newPosition,
						"",
						"",
						null
						);

					//We need to populate any custom property default values
					artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestStep, newTestStepId, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//We need to encode the new artifact id as a 'pseudo' validation message
					ValidationMessage newMsg = new ValidationMessage();
					newMsg.FieldName = "$NewArtifactId";
					newMsg.Message = newTestStepId.ToString();
					AddUniqueMessage(validationMessages, newMsg);
				}

				//Return back any messages. For success it should only contain a new artifact ID if we're inserting
				return validationMessages;
			}
			catch (ArtifactNotExistsException)
			{
				//Let the user know that the ticket no inter exists
				return CreateSimpleValidationMessage(String.Format(Resources.Messages.TestStepService_TestStepNotFound, testStepId));
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Retrieves a list of test step parameters that are defined for a specific test step
		/// (either inherited or direct), including any values that are populated
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="linkedTestCaseId">The id of the test case being linked TO</param>
		/// <returns>The list of parameter data objects</returns>
		public List<DataItem> RetrieveParameters(int testStepId, int linkedTestCaseId)
		{
			const string METHOD_NAME = "RetrieveParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the array of data items to store the parameter values
				List<DataItem> dataItems = new List<DataItem>();

				//Get the list of parameters that are either direct or inherited for the test case
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(linkedTestCaseId, true, false);

				//Now get the list of values that have been already set on the test step link
				List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(testStepId);

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

					//The Value field from the test steps dataset
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Value";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
					TestStepParameter testStepParameter = testStepParameters.FirstOrDefault(p => p.TestStepId == testStepId && p.TestCaseParameterId == testCaseParameter.TestCaseParameterId);
					if (testStepParameter == null)
					{
						dataItemField.TextValue = null;
					}
					else
					{
						dataItemField.TextValue = testStepParameter.Value;
					}
					dataItem.Fields.Add("Value", dataItemField);
				}

				return dataItems;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return null
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Imports steps from a test case as the steps of the current test case
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testCaseId">The id of the current test case that we want to import INTO</param>
		/// <param name="testCaseToImportId">The id of another test case that we want to import steps FROM</param>
		/// <param name="testStepId">(optional) The id of a test step that we want to import the steps in front of, leaving null imports them at the end instead</param>
		public void TestStep_ImportTestCase(int projectId, int testCaseId, int testCaseToImportId, int? testStepId)
		{
			const string METHOD_NAME = "TestStep_ImportTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Retrieve the test case to make sure we have permissions to modify it (and that it exists)
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseView currentTestCase = testCaseManager.RetrieveById(projectId, testCaseId);
				if (authorizationState == Project.AuthorizationState.Limited && currentTestCase.AuthorId != userId && currentTestCase.OwnerId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Now do the import itself
				testCaseManager.TestCase_ImportSteps(projectId, testCaseId, testStepId, testCaseToImportId, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ArtifactNotExistsException)
			{
				//Just ignore since it no longer exists
			}
			catch (FaultException)
			{
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Cretaes a new test case that is then linked to the current test case as a step with the specified parameter values
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testCaseId">The id of the current test case that we want to import INTO</param>
		/// <param name="testStepId">(optional) The id of a test step that we want to import the steps in front of, leaving null imports them at the end instead</param>
		/// <param name="testCaseFolderId">The folder of the new test case (null = root)</param>
		/// <param name="testCaseName">The name of the new test case</param>
		/// <param name="parameters">Any parameter name/values</param>
		public void TestStep_CreateNewLinkedTestCase(int projectId, int testCaseId, int? testStepId, int? testCaseFolderId, string testCaseName, List<DataItem> parameters)
		{
			const string METHOD_NAME = "TestStep_CreateNewLinkedTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Retrieve the test case to make sure we have permissions to modify it (and that it exists)
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseView currentTestCase = testCaseManager.RetrieveById(projectId, testCaseId);
				if (authorizationState == Project.AuthorizationState.Limited && currentTestCase.AuthorId != userId && currentTestCase.OwnerId != userId)
				{
					throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Convert the parameters into a simple dictionary
				Dictionary<string, string> newParameters = new Dictionary<string, string>();
				if (parameters != null && parameters.Count > 0)
				{
					foreach (DataItem parameter in parameters)
					{
						if (parameter.Fields.ContainsKey("Name") && !String.IsNullOrWhiteSpace(parameter.Fields["Name"].TextValue))
						{
							string parameterName = parameter.Fields["Name"].TextValue.Trim();
							string parameterValue = null;
							if (parameter.Fields.ContainsKey("Value") && !String.IsNullOrWhiteSpace(parameter.Fields["Value"].TextValue))
							{
								parameterValue = parameter.Fields["Value"].TextValue;
							}

							if (!newParameters.ContainsKey(parameterName))
							{
								newParameters.Add(parameterName, parameterValue);
							}
						}
					}
				}

				//Now do the insert itself
				testCaseManager.TestCase_CreateNewLinkedTestCase(userId, projectId, testCaseId, testStepId, testCaseFolderId, testCaseName, newParameters);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ArtifactNotExistsException)
			{
				//Just ignore since it no longer exists
			}
			catch (FaultException)
			{
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Updates the list of parameter values with those provided
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="testStepParameterValues">The list of parameter value data objects</param>
		/// <param name="projectId">The ID of the project</param>
		/// <remarks>
		/// Will insert/delete parameter values if necessary
		/// </remarks>
		public void UpdateParameters(int projectId, int testStepId, List<DataItem> testStepParameterValues)
		{
			const string METHOD_NAME = "UpdateParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Convert the parameters into their EF equivalent objects, the BO function determines if INSERT/UPDATE/DELETE
				List<TestStepParameter> testStepParameters = new List<TestStepParameter>();
				foreach (DataItem parameterItem in testStepParameterValues)
				{
					if (parameterItem.Fields.ContainsKey("Value") && !String.IsNullOrWhiteSpace(parameterItem.Fields["Value"].TextValue))
					{
						TestStepParameter testStepParameter = new TestStepParameter();
						testStepParameter.TestStepId = testStepId;
						testStepParameter.TestCaseParameterId = parameterItem.PrimaryKey;
						testStepParameter.Value = parameterItem.Fields["Value"].TextValue;
						testStepParameters.Add(testStepParameter);
					}
				}

				//Commit the changes
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.SaveParameterValues(projectId, testStepId, testStepParameters);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Returns a list of test steps in the test case
		/// </summary>
		/// <param name="userId">The user we're viewing the test steps as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">A standard filters collection that contains a value for TestCaseId</param>
		/// <returns>Collection of JS serializable dataitems</returns>
		public OrderedData OrderedList_Retrieve(int projectId, JsonDictionaryOfStrings standardFilters)
		{
			const string METHOD_NAME = "OrderedList_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized, limited is OK as long as we're the owner of the test case the step belongs
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business objects
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Create the array of data items
				OrderedData orderedData = new OrderedData();
				List<OrderedDataItem> dataItems = orderedData.Items;

				//Create the first 'shape' item, we can clone others from it later
				OrderedDataItem shapeItem = new OrderedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, shapeItem);
				dataItems.Add(shapeItem);

				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Make sure the user is authorized for the test case containing these steps if they only have limited permissions
				TestCaseView testCase = testCaseManager.RetrieveById(projectId, testCaseId);
				int ownerId = -1;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Now get the pagination information and add to the shape item
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_STEP_PAGINATION);
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

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the list of test steps in the test case
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);
				int artifactCount = testSteps.Count;

				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
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
				orderedData.CurrPage = currentPage;
				orderedData.PageCount = pageCount;

				//Now get the list of custom property options and lookup values for this artifact type / project
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, true, false, true);

				//Iterate through all the test cases in the pagination range populate the dataitem
				int visibleCount = 0;
				for (int i = startRow - 1; i < startRow + paginationSize - 1 && i < artifactCount; i++)
				{
					TestStepView testStep = testSteps[i];

					//We clone the template/shape item as the basis of all the new items
					OrderedDataItem dataItem = shapeItem.Clone();

					//The dataset doesn't contain the position, but it's ordered by position, so can get it indirectly
					int displayPosition = i + 1;

					//Now populate with the data
					PopulateRow(projectId, dataItem, testStep, customProperties, false, displayPosition, null);
					dataItems.Add(dataItem);
					visibleCount++;
				}

				//Display the visible and total count of artifacts
				orderedData.VisibleCount = visibleCount;
				orderedData.TotalCount = artifactCount;

				//Add the pagination information
				orderedData.PaginationOptions = this.RetrievePaginationOptions(projectId);

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Created data items with " + dataItems.Count.ToString() + " rows");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				return orderedData;
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
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the test steps as</param>
		/// <param name="dataItem">The data item object that will be used as a template for the rows</param>
		/// <param name="returnJustListFields">Should we return just the list fields (default) or all the fields</param>
		protected void PopulateShape(int projectId, int projectTemplateId, int userId, OrderedDataItem dataItem, bool returnJustListFields = true)
		{
			//First add the static columns (always present) that occur before the dynamic ones
			DataItemField dataItemField = new DataItemField();
			dataItemField.FieldName = "Position";
			dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription;
			dataItemField.Caption = Resources.Fields.TestStepPosition;
			dataItemField.Editable = false;
			dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

			//We need to dynamically add the various columns from the field list
			LookupRetrieval getLookupValues = new LookupRetrieval(GetLookupValues);
			AddDynamicColumns(Artifact.ArtifactTypeEnum.TestStep, getLookupValues, projectId, projectTemplateId, userId, dataItem, null, null, returnJustListFields);
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

				if (lookupName == "ExecutionStatusId")
				{
					List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();
					lookupValues = ConvertLookupValues(executionStati.OfType<DataModel.Entity>().ToList(), "ExecutionStatusId", "Name");
				}

				//The custom property lookups
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(lookupName);
				if (customPropertyNumber.HasValue)
				{
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, customPropertyNumber.Value, true);
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

				return lookupValues;
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
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="artifactCustomProperty">The artifatc's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="testStepView">The datarow containing the data</param>
		/// <param name="displayPosition">The display position of the datarow</param>
		/// <param name="projectId">The id of the current project</param>
		protected void PopulateRow(int projectId, OrderedDataItem dataItem, TestStepView testStepView, List<CustomProperty> customProperties, bool editable, int displayPosition, ArtifactCustomProperty artifactCustomProperty)
		{
			//Set the primary key and concurrency value
			dataItem.PrimaryKey = testStepView.TestStepId;
			dataItem.ConcurrencyValue = String.Format(GlobalFunctions.FORMAT_DATE_TIME_INVARIANT, testStepView.ConcurrencyDate);

			//Specify if this is an alternate row - i.e. one that is really a linked test case not a true step
			if (testStepView.LinkedTestCaseId.HasValue)
			{
				dataItem.Alternate = true;
				dataItem.AlternateKey = testStepView.LinkedTestCaseId.Value;
				//Only allow scripting for alternate items (to avoid XSS attacks)
				dataItem.AllowScript = true;
			}
			else
			{
				dataItem.Alternate = false;
				dataItem.AlternateKey = -1;
				//Only allow scripting for alternate items (to avoid XSS attacks)
				dataItem.AllowScript = false;
			}

			//Specify if it has an attachment or not
			dataItem.Attachment = testStepView.IsAttachments;

			//Specify its position
			dataItem.Position = testStepView.Position;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
				if (testStepView.ContainsProperty(dataItemField.FieldName))
				{
					//First populate the data-item from the data-row
					PopulateFieldRow(dataItem, dataItemField, testStepView, customProperties, artifactCustomProperty, editable, null);

					//Specify which fields are editable or not
					//Unless specified, all fields are editable
					dataItemField.Editable = true;

					//The position field and execution status are not editable
					if (fieldName == "Position" || fieldName == "ExecutionStatusId")
					{
						dataItemField.Editable = false;
					}

					//The position needs to be specified as "Step X"
					if (dataItemField.FieldName == "Position")
					{
						dataItemField.TextValue = Resources.Fields.Step + "\u00a0" + displayPosition;
					}

					//If we have a linked test case, need to pass the test case name and id 
					//as a hyperlink and not just the description field
					//and parameters get inserted as an embedded table
					if (testStepView.LinkedTestCaseId.HasValue && !String.IsNullOrEmpty(testStepView.LinkedTestCaseName))
					{
						if (dataItemField.FieldName == "Description")
						{
							string url = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId, testStepView.LinkedTestCaseId.Value, GlobalFunctions.PARAMETER_TAB_TESTSTEP));
							string markup = @"
<a href=""" + url + @""" onmouseover=""TestStepService_onLinkMouseOver(" + testStepView.TestStepId + @",event)"" onmouseout=""TestStepService_onLinkMouseOut(event)"">" +
		Resources.Main.TestStepService_Call + " '" + Microsoft.Security.Application.Encoder.HtmlEncode(testStepView.LinkedTestCaseName) + "' (" + GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE + testStepView.LinkedTestCaseId + ")</a>\u00a0" +
	@"<img src=""[SPIRA_THEME]/Images/artifact-TestCase.svg"" class=""w4 h4"" alt=""Linked Test Case"" style=""vertical-align:middle"" />
";
							dataItemField.TextValue = markup;
						}
						if (dataItemField.FieldName == "SampleData")
						{
							dataItemField.TextValue = "";	//Prevent XSS
							//Now we need to check for parameters, and add on the parameter list to the name of the linked test case
							List<TestStepParameter> testStepParameterValues = new TestCaseManager().RetrieveParameterValues(testStepView.TestStepId);
							if (testStepParameterValues.Count > 0)
							{
								string markup = @"<table class=""parameter-table""><tbody>";
								foreach (TestStepParameter testStepParameter in testStepParameterValues)
								{
									markup += "<tr><td>" + Microsoft.Security.Application.Encoder.HtmlEncode(testStepParameter.Name) + "</td><td>" + Microsoft.Security.Application.Encoder.HtmlEncode(testStepParameter.Value) + "</td></tr>\n";
								}

								markup += @"</tbody></table>";
								dataItemField.TextValue = markup;
							}
						}
						if (dataItemField.FieldName == "ExpectedResult")
						{
							dataItemField.TextValue = "";   //Prevent XSS
						}
					}

					//Apply the conditional formatting to the execution status column (if displayed)
					if (dataItemField.FieldName == "ExecutionStatusId")
					{
						dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testStepView.ExecutionStatusId);
						//Also add code so that you can redirect to the appropriate test run (unless not run or n/a)
						if (testStepView.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun && testStepView.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotApplicable)
						{
							dataItemField.Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestStepRuns, projectId, testStepView.TestCaseId, testStepView.TestStepId));
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the latest information on a single test step in the system
		/// </summary>
		/// <param name="testStepId">The id of the particular test step we want to retrieve</param>
		/// <param name="userId">The user we're viewing the test cases/steps as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">A standard filters collection that contains a value for TestCaseId</param>
		/// <returns>A single dataitem object</returns>
		public OrderedDataItem OrderedList_Refresh(int projectId, JsonDictionaryOfStrings standardFilters, int testStepId)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Instantiate the business objects
				TestCaseManager testCaseManager = new TestCaseManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Create the data item record
				OrderedDataItem dataItem = new OrderedDataItem();
				PopulateShape(projectId, projectTemplateId, userId, dataItem);

				//Get the test step record for the specific test case and test step id
				TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
				TestStepView testStep = testCaseManager.RetrieveStepById2(testCaseId, testStepId);
				if (testCase == null || testStep == null)
				{
					throw new ArtifactNotExistsException("Unable to locate test step " + testStepId + " in the project. It no longer exists!");
				}

				//Make sure the user is authorized for this item
				int ownerId = -1;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//The main dataset does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, true);

				//Finally populate the dataitem from the dataset
				//See if we already have an artifact custom property row
				if (artifactCustomProperty != null)
				{
					PopulateRow(projectId, dataItem, testStep, artifactCustomProperty.CustomPropertyDefinitions, true, testStep.Position, artifactCustomProperty);
				}
				else
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, true, false);
					PopulateRow(projectId, dataItem, testStep, customProperties, true, testStep.Position, null);
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
		/// <param name="standardFilters">A standard filters collection that contains a value for TestCaseId</param>
		/// <returns>List of any validation messages</returns>
		public List<ValidationMessage> OrderedList_Update(int projectId, JsonDictionaryOfStrings standardFilters, List<OrderedDataItem> dataItems)
		{
			const string METHOD_NAME = "OrderedList_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized, limited is OK as long as we're the owner of the test case the step belongs
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Used to store any validation messages
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Iterate through each data item and make the updates
				TestCaseManager testCaseManager = new TestCaseManager();
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);

				//Make sure the user is authorized for the test case containing these steps if they only have limited permissions
				int ownerId = -1;
				if (testCase.OwnerId.HasValue)
				{
					ownerId = testCase.OwnerId.Value;
				}
				if (authorizationState == Project.AuthorizationState.Limited && ownerId != userId && testCase.AuthorId != userId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Make sure the test step is not locked due to its workflow status
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Load the custom property definitions (once, not per artifact)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);

				foreach (OrderedDataItem dataItem in dataItems)
				{
					//Get the test step id
					int testStepId = dataItem.PrimaryKey;

					//Locate the existing record - and make sure it still exists. Also retrieve the associated custom property record
					TestStep testStep = testCase.TestSteps.FirstOrDefault(s => s.TestStepId == testStepId);
					if (testStep == null)
					{
						throw new ArtifactNotExistsException("Unable to locate test step " + testStepId + " in the project. It no longer exists!");
					}
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, false, customProperties);

					//Create a new artifact custom property row if one doesn't already exist
					if (artifactCustomProperty == null)
					{
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestStep, testStepId, customProperties);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

					//Need to set the original date of this record to match the concurrency date
					if (!String.IsNullOrEmpty(dataItem.ConcurrencyValue))
					{
						DateTime concurrencyDateTimeValue;
						if (DateTime.TryParse(dataItem.ConcurrencyValue, out concurrencyDateTimeValue))
						{
							testStep.ConcurrencyDate = concurrencyDateTimeValue;
							testStep.AcceptChanges();
						}
					}

					//Start Tracking Changes
					testStep.StartTracking();

					//Update the field values
					List<string> fieldsToIgnore = new List<string>();
					fieldsToIgnore.Add("LastUpdateDate");
					UpdateFields(validationMessages, dataItem, testStep, customProperties, artifactCustomProperty, projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, fieldsToIgnore);

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
						//Persist to database
						try
						{
							testCaseManager.UpdateStep(testStep, userId);
						}
						catch (EntityForeignKeyException)
						{
							return CreateSimpleValidationMessage(Resources.Messages.Global_DependentArtifactDeleted);
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
		/// Inserts a new test step into the system
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="artifactId">The id of the existing test step we're inserting in front of (null for none)</param>
		/// <param name="artifact">The type of artifact we're inserting (Step, Link, etc.)</param>
		/// <param name="standardFilters">A standard filters collection that contains a value for TestCaseId</param>
		/// <returns>The id of the new test step</returns>
		public int OrderedList_Insert(int projectId, string artifact, JsonDictionaryOfStrings standardFilters, int? artifactId)
		{
			const string METHOD_NAME = CLASS_NAME+"Insert()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Make sure the test step is not locked due to its workflow status
				TestCaseManager testCaseManager = new TestCaseManager();
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Default to new steps
				if (String.IsNullOrEmpty(artifact))
				{
					artifact = "Step";
				}

				if (artifact == "Step")
				{
					int testStepId = -1;

					//Check to see if we are inserting before an existing test step or simply adding at the end
					if (!artifactId.HasValue)
					{
						//Call the BO insert method with Null as the position
						testStepId = testCaseManager.InsertStep(
							userId,
							testCaseId,
							null,
							"",
							"",
							null
							);
					}
					else
					{
						//We need to get the position of the existing step
						TestStep existingTestStep = testCaseManager.RetrieveStepById(projectId, artifactId.Value);

						//Locate the existing record - and make sure it still exists
						int? existingPosition = null;   //If we can't find a match, insert at end
						if (existingTestStep != null && existingTestStep.TestCaseId == testCaseId)
						{
							existingPosition = existingTestStep.Position;
						}

						//Call the BO insert passing the appropriate position indicator
						testStepId = testCaseManager.InsertStep(
							userId,
							testCaseId,
							existingPosition,
							"",
							"",
							null
							);
					}

					//We need to populate any custom property default values
					CustomPropertyManager customPropertyManager = new CustomPropertyManager();
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, false);
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestStep, testStepId, customProperties);
					artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					return testStepId;
				}
				else if (artifact == "Link")
				{
					//The test case we're linking to needs to be passed in as an additional 'standard filter'
					if (!standardFilters.ContainsKey("LinkedTestCaseId"))
					{
						throw new ArgumentException("You need to provide a LinkedTestCaseId as a standard filter");
					}
					int linkedTestCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["LinkedTestCaseId"]);


					// make sure we are not trying to link a test case to itself
					if (testCaseId == linkedTestCaseId)
					{
						throw new ArgumentException("You cannot link a test case to itself.");
					}

					//The parameters also get passed through as a standard filter based on their name
					Dictionary<string, string> testStepParameters = new Dictionary<string, string>();
					int prefixLength = "TestStepParameter_".Length;
					foreach (KeyValuePair<string, string> filter in standardFilters)
					{
						if (filter.Key.Length > prefixLength && filter.Key.Substring(0, prefixLength) == "TestStepParameter_")
						{
							string parameterName = filter.Key.Substring(prefixLength, filter.Key.Length - prefixLength);
							string parameterValue = (string)GlobalFunctions.DeSerializeValue(filter.Value);
							if (!String.IsNullOrEmpty(parameterValue))
							{
								testStepParameters.Add(parameterName, parameterValue);
							}
						}
					}

					int testStepId = -1;

					//Check to see if we are inserting before an existing test step or simply adding at the end
					if (!artifactId.HasValue)
					{
						//Call the BO insert method with Null as the position
						testStepId = testCaseManager.InsertLink(
							userId,
							testCaseId,
							null,
							linkedTestCaseId,
							testStepParameters
							);
					}
					else
					{
						//We need to get the position of the existing step
						TestStep existingTestStep = testCaseManager.RetrieveStepById(projectId, artifactId.Value);

						//Locate the existing record - and make sure it still exists
						int? existingPosition = null;   //If we can't find a match, insert at end
						if (existingTestStep != null && existingTestStep.TestCaseId == testCaseId)
						{
							existingPosition = existingTestStep.Position;
						}

						//Call the BO insert passing the appropriate position indicator
						testStepId = testCaseManager.InsertLink(
							userId,
							testCaseId,
							existingPosition,
							linkedTestCaseId,
							testStepParameters
							);
					}

					return testStepId;
				}
				else
				{
					throw new ArgumentException("The artifact '" + artifact + "' is not supported by this service.");
				}
			}
			catch (EntityInfiniteRecursionException)
			{
				//We need to let the user know that they have created a dangerous case of a self-referencing test step
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.TestStepService_SelfReferencingTestLink);
				throw new EntityInfiniteRecursionException(Resources.Messages.TestStepService_SelfReferencingTestLink);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Clones/copies a selection of test step/links
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="standardFilters">A standard filters collection that contains a value for TestCaseId</param>
		/// <param name="items">The items being copied/cloned</param>
		public void OrderedList_Copy(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Make sure the test step is not locked due to its workflow status
				TestCaseManager testCaseManager = new TestCaseManager();
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Iterate through all the items to be removed
				foreach (string itemValue in items)
				{
					//Get the test step ID
					int testStepId = Int32.Parse(itemValue);
					try
					{
						testCaseManager.CopyTestStep(userId, projectId, testCaseId, testStepId);
					}
					catch (ArtifactNotExistsException)
					{
						//Ignore any errors due to the test set of test case no longer existing
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
			JsonDictionaryOfStrings paginationDictionary = base.RetrievePaginationOptions(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_STEP_PAGINATION);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
			return paginationDictionary;
		}

		/// <summary>
		/// Returns a plain-text version of the artifact name/description typically used in dynamic tooltips
		/// </summary>
		/// <param name="testStepId">The id of the test step to get the data for</param>
		/// <returns>The name and description converted to plain-text</returns>
		public string RetrieveNameDesc(int? projectId, int testStepId, int? displayTypeId)
		{
			const string METHOD_NAME = "RetrieveNameDesc";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Instantiate the test case business object
				TestCaseManager testCaseManager = new TestCaseManager();

				try
				{
					//Now retrieve the specific test step - handle quietly if it doesn't exist                    
					TestStepView testStep = testCaseManager.RetrieveStepById2(null, testStepId);
					if (testStep == null)
					{
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test step " + testStepId);
						Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
						Logger.Flush();
						return Resources.Messages.Global_UnableRetrieveTooltip;
					}
					string tooltip;
					if (String.IsNullOrEmpty(testStep.ExpectedResult))
					{
						//See if we have a real test step or a linked test case
						if (!testStep.LinkedTestCaseId.HasValue)
						{
							tooltip = GlobalFunctions.HtmlRenderAsPlainText(testStep.Description);
						}
						else
						{
							//First concatenate the linked test case name and id
							tooltip = "<u>" + Resources.Main.TestStepService_Call + " '" + testStep.LinkedTestCaseName + "' (" + GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE + testStep.LinkedTestCaseId.Value + ")</u>";

							//Now we need to check for parameters, and add on the parameter list to the name of the linked test case
							List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);
							if (testStepParameterValues.Count > 0)
							{
								tooltip += "<i>";
								for (int i = 0; i < testStepParameterValues.Count; i++)
								{
									if (i == 0)
									{
										tooltip += "<br />" + Resources.Main.TestStepService_With + " ";
									}
									else
									{
										tooltip += ", ";
									}
									tooltip += testStepParameterValues[i].Name + "=" + testStepParameterValues[i].Value;
								}
								tooltip += "</i>";
							}
						}
					}
					else
					{
						tooltip = "<u>" + GlobalFunctions.HtmlRenderAsPlainText(testStep.Description) + "</u><br />\n<i>" + GlobalFunctions.HtmlRenderAsPlainText(testStep.ExpectedResult) + "</i>";
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return tooltip;
				}
				catch (ArtifactNotExistsException)
				{
					//This is the case where the client still displays the test case, but it has already been deleted on the server
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to retrieve tooltip for test step " + testStepId);
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
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_STEP_PAGINATION);
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
					customPropertyManager.CustomProperty_ToggleListVisibility(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestStep, fieldName);
				}
				else
				{
					//Toggle the status of the appropriate field name
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ToggleListVisibility(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestStep, fieldName);
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
		/// Changes the order of columns in the test step list
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
				//Also we have the fixed Step # field which always comes first and cannot be changed
				//The two factors cancel out so we don't need to +1 like the other artifacts
				int newPosition = newIndex;

				//Make sure in range
				if (newPosition > 0)
				{
					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Toggle the status of the appropriate artifact field or custom property
					ArtifactManager artifactManager = new ArtifactManager();
					artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, userId, DataModel.Artifact.ArtifactTypeEnum.TestStep, fieldName, newPosition);
				}
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
		/// Deletes the set of test steps from the specified test case
		/// </summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="items">The items being removed</param>
		/// <param name="standardFilters">The id of the test case the test steps belong to passed in as a filter (key = 'TestCaseId')</param>
		public void OrderedList_Delete(int projectId, JsonDictionaryOfStrings standardFilters, List<string> items)
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Delete, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Make sure the test step is not locked due to its workflow status
				TestCaseManager testCaseManager = new TestCaseManager();
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Iterate through all the items to be removed
				foreach (string itemValue in items)
				{
					//Get the test step ID
					int testStepId = Int32.Parse(itemValue);
					try
					{
						testCaseManager.MarkStepAsDeleted(userId, testCaseId, testStepId);
					}
					catch (ArtifactNotExistsException)
					{
						//Ignore any errors due to the test set of test case no longer existing
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
		/// Changes the position of a test step in the test case
		/// </summary>
		/// <param name="userId">The user we're viewing as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceItems">The items to move</param>
		/// <param name="destTestStepId">The destination item's id (or null for no destination selected)</param>
		/// <param name="standardFilters">The id of the test case the test steps belong to passed in as a filter (key = 'TestCaseId')</param>
		public void OrderedList_Move(int projectId, JsonDictionaryOfStrings standardFilters, List<string> sourceItems, int? destTestStepId)
		{
			const string METHOD_NAME = "OrderedList_Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//The test case needs to be passed in as a standard filter
				if (standardFilters == null)
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				if (!standardFilters.ContainsKey("TestCaseId"))
				{
					throw new ArgumentException("You need to provide a TestCaseId as a standard filter");
				}
				int testCaseId = (int)GlobalFunctions.DeSerializeValue(standardFilters["TestCaseId"]);

				//Make sure the test step is not locked due to its workflow status
				TestCaseManager testCaseManager = new TestCaseManager();
				if (!testCaseManager.AreTestStepsEditableInStatus(testCaseId))
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Iterate through all the items to be moved and perform the operation
				//Check to make sure we don't have any duplicates
				List<int> existingIds = new List<int>();
				foreach (string itemValue in sourceItems)
				{
					//Get the source ID
					int sourceTestStepId = Int32.Parse(itemValue);
					if (!existingIds.Contains(sourceTestStepId))
					{
						testCaseManager.MoveStep(testCaseId, sourceTestStepId, destTestStepId, CurrentUserId.Value);
						existingIds.Add(sourceTestStepId);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#region INavigationService interface

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
		/// Returns a list of test steps for display in the navigation bar
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="indentLevel">Not used for test steps since not hierarchical</param>
		/// <returns>List of test steps</returns>
		/// <param name="displayMode">
		/// The display mode of the navigation list:
		/// 1 = Filtered List
		/// 2 = All Items (no filters)
		/// </param>
		/// <param name="selectedItemId">The id of the currently selected item</param>
		/// <param name="containerId">The id of any containing artifact (test case for this service)</param>
		/// <remarks>Test steps don't allow filters, so option 1 and 2 will be identical</remarks>
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
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestStep);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Make sure we have a test case id specified
				if (!containerId.HasValue)
				{
					throw new InvalidOperationException("Unable to retrieve navigation list, missing container id)");
				}
				int testCaseId = containerId.Value;

				//Instantiate the business object
				TestCaseManager testCaseManager = new TestCaseManager();

				//Create the array of data items
				List<HierarchicalDataItem> dataItems = new List<HierarchicalDataItem>();

				//Test Steps don't allow filters

				//Now get the pagination information
				ProjectSettingsCollection paginationSettings = new ProjectSettingsCollection(projectId, userId, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_TEST_STEP_PAGINATION);
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

				//Retrieve the test case
				TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);

				//Get the number of test steps in the test case
				int artifactCount = testCase.TestSteps.Count;

				//**** Now we need to actually populate the rows of data to be returned ****

				//Get the incidents list dataset for the user/project
				int startRow = ((currentPage - 1) * paginationSize) + 1;
				if (startRow > artifactCount)
				{
					startRow = 1;
				}

				//All display modes are the same for test steps (since no filters)
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

				//First we need to add the parent test case as a pseudo-item
				HierarchicalDataItem dataItem = new HierarchicalDataItem();

				//Populate the necessary fields
				dataItem.PrimaryKey = 0;    //so that it doesn't try and live load it
				dataItem.Indent = "AAA";
				dataItem.Expanded = true;
				dataItem.Summary = true;
				dataItem.CustomUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId, testCaseId));

				//Name/Desc
				DataItemField dataItemField = new DataItemField();
				dataItemField.FieldName = "Name";
				dataItemField.TextValue = testCase.Name;
				dataItem.Fields.Add("Name", dataItemField);

				//Add to the items collection
				dataItems.Add(dataItem);

				//Iterate through all the test steps in the pagination range and populate the dataitem (only some columns are needed)
				string testStepIndentLevel = "AAAAAA";
				for (int i = startRow - 1; i < startRow + paginationSize - 1 && i < artifactCount; i++)
				{
					TestStep testStep = testCase.TestSteps[i];

					//The dataset doesn't contain the position, but it's ordered by position, so can get it indirectly
					int displayPosition = i + 1;

					//Create the data-item
					dataItem = new HierarchicalDataItem();

					//Populate the necessary fields
					dataItem.PrimaryKey = testStep.TestStepId;
					dataItem.Indent = testStepIndentLevel;
					dataItem.Expanded = false;

					//Test links should not be URLs, so send a custom URL that tells it to not display as a link
					if (testStep.LinkedTestCaseId.HasValue)
					{
						dataItem.CustomUrl = "#";
					}

					//Name/Desc
					dataItemField = new DataItemField();
					dataItemField.FieldName = "Name";
					dataItemField.TextValue = Resources.Fields.Step + " " + displayPosition + " (" + GlobalFunctions.ARTIFACT_PREFIX_TEST_STEP + String.Format(GlobalFunctions.FORMAT_ID, testStep.TestStepId) + ")";
					dataItem.Summary = false;
					dataItem.Alternate = testStep.LinkedTestCaseId.HasValue;
					dataItem.Fields.Add("Name", dataItemField);

					//Add to the items collection
					dataItems.Add(dataItem);

					//Increment the indent level
					testStepIndentLevel = HierarchicalList.IncrementIndentLevel(testStepIndentLevel);
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
		public void NavigationBar_UpdateSettings(int projectId, int? displayMode, int? displayWidth, bool? minimized)
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
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_STEP_GENERAL_SETTINGS);
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
