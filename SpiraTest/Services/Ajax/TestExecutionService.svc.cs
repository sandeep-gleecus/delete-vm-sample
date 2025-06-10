using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Activation;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// Used by TestExecution.aspx to handle the execution of manual test cases by the user
	/// </summary>
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class TestExecutionService : ListServiceBase, ITestExecutionService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionService::";

        /// <summary>
        /// The different display modes available (for information)
        /// </summary>
        public enum DisplayModeMain
        {
            Split = 1,
            Grid = 2,
            Mini = 3
        }
        public enum DisplayModeSub
        {
            Primary = 1,
            Secondary = 2
        }

		#region IFormService methods

		/// <summary>Returns a single test run data record (all columns) for use by the FormManager control</summary>
		/// <param name="artifactId">The id of the current test run pending entry (TestRunsPendingId)</param>
		/// <returns>A TestRun data item</returns>
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

			//Make sure we're authorized to create test runs
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Make sure we're provided a TestRunsPendingId
			if (!artifactId.HasValue)
			{
				throw new InvalidOperationException(Resources.Messages.TestExecutionService_TestRunsPendingIdNotProvided);
			}
			int testRunsPendingId = artifactId.Value;

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Instantiate the business classes
                TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//Retrieve the test run pending entry
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure the projects match, otherwise we're not really authorized
                if (testRunsPending.ProjectId != projectId)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Make sure that the current user is the owner of this pending test run or at least one of the test runs in the group
				bool isAnOwner = false;
                if (testRunsPending.TesterId == userId)
				{
					isAnOwner = true;
				}
                foreach (TestRun testRun in testRunsPending.TestRuns)
				{
					if (testRun.TesterId == userId)
					{
						isAnOwner = true;
					}
				}
				if (!isAnOwner)
				{
					throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
				}

				//Make sure that we have at least one test run in the pending list
                if (testRunsPending.TestRuns.Count == 0)
				{
					//Just return no data back
					Logger.LogWarningEvent(METHOD_NAME, Resources.Messages.TestExecutionService_NoTestRunsFound);
					return null;
				}
				//Get the first test run row and use that for the release, build and custom properties
				//since all the otehr ones will be the same.
                TestRun testRunItem = testRunsPending.TestRuns[0];
                int testRunId = testRunItem.TestRunId;

				//Populate the 'shape' of the data being returned
				SortedDataItem dataItem = new SortedDataItem();

				//We dynamically add the various columns from the field list for a test run (all fields, not just list ones)
                AddDynamicColumns(DataModel.Artifact.ArtifactTypeEnum.TestRun, null, projectId, projectTemplateId, userId, dataItem, null, null, false);

                //Add any custom extensions to the TestRunsPending entity query
                //Create the template item field

                string newRun = Resources.Main.TestCaseExecution_TestExecutionWizard;
                string resumedRun = Resources.Main.TestCaseExecution_TestExecutionResume;

                DataItemField dataItemField = new DataItemField();
                dataItemField.FieldName = "WizardTitle";
                dataItemField.AllowDragAndDrop = true;
                dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
                dataItemField.TextValue = testRunsPending.IsResumed ? resumedRun : newRun;
                dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

				//The main test run object does not have the custom properties, they need to be retrieved separately
				ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);

				//See if we have any existing artifact custom properties for this row
				if (artifactCustomProperty == null)
				{
					List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, true, false);
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, testRunItem.TestRunId, customProperties);
                    
                    //If we don't have a test set specified, populate the default test run custom properties
                    if (!testRunsPending.TestSetId.HasValue)
                    {
                        customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                    }                    
                    PopulateRow(dataItem, testRunsPendingId, testRunItem, customProperties, true, artifactCustomProperty);
				}
				else
				{
                    PopulateRow(dataItem, testRunsPendingId, testRunItem, artifactCustomProperty.CustomPropertyDefinitions, true, artifactCustomProperty);
				}

				return dataItem;
			}
			catch (System.ServiceModel.FaultException exception)
			{
				//Just rethrow
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
			catch (ArtifactNotExistsException)
			{
				//Just return no data back
				Logger.LogWarningEvent(METHOD_NAME, Resources.Messages.TestExecutionService_PendingTestRunDoesNotExist);
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Saves the user's Test Run 'first page' information (Release, Build and custom properties)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="dataItem">The data item being saved</param>
		/// <returns>Any validation messages</returns>
        /// <param name="signature">Any digital signature</param>
		/// <param name="operation">The type of save operation ('new', 'close', '', etc.)</param>
        public List<ValidationMessage> Form_Save(int projectId, DataItem dataItem, string operation, Signature signature)
		{
			const string METHOD_NAME = "Form_Save";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			//Get the test runs pending id
			int testRunsPendingId = dataItem.PrimaryKey;

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Retrieve the pending test run
                TestRunManager testRunManager = new TestRunManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure the project ids match
                if (projectId != testRunsPending.ProjectId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

				//We need to iterate through the test run list to update the release number, build id and custom properties
				int? releaseId = null;
                foreach (TestRun testRun in testRunsPending.TestRuns)
				{
                    DateTime originalConcurrencyDateTime = testRun.ConcurrencyDate;

					//Next get the custom properties for this test run
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRun.TestRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, true);
					if (artifactCustomProperty == null)
					{
						//First create a new row
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, false);
						artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.TestRun, testRun.TestRunId, customProperties);
						artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
					}
					else
					{
						artifactCustomProperty.StartTracking();
					}

                    //We don't explicitly change the concurrency date since we will be updating
                    //multiple test runs with the same information
                    testRun.ConcurrencyDate = originalConcurrencyDateTime;
                    testRun.AcceptChanges();

                    //Now we can start tracking any changes
                    testRun.StartTracking();

					//Update the field values
                    testRun.StartTracking();
					List<string> fieldsToIgnore = new List<string>();
					fieldsToIgnore.Add("ExecutionStatusId");
					fieldsToIgnore.Add("TesterId");
					fieldsToIgnore.Add("StartDate");
					fieldsToIgnore.Add("EndDate");
                    fieldsToIgnore.Add("ActualDuration");
                    fieldsToIgnore.Add("Name");
                    fieldsToIgnore.Add("Description");
					UpdateFields(validationMessages, dataItem, testRun, artifactCustomProperty.CustomPropertyDefinitions, artifactCustomProperty, projectId, testRun.TestRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, fieldsToIgnore);

					//Now verify the options for the custom properties to make sure all rules have been followed
					Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(artifactCustomProperty.CustomPropertyDefinitions, artifactCustomProperty);
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

					//Save the updated custom properties
					customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

					//See if we have a release
                    releaseId = testRun.ReleaseId;
				}

                //Save the entire pending test run
                testRunManager.Update(testRunsPending, userId, releaseId);

				//If we have a Release selected, send it back as the special $NewArtifactId message
				//Get the new release number
				if (releaseId.HasValue)
				{
					try
					{
						ReleaseManager releaseManager = new ReleaseManager();
						ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId.Value);
                        validationMessages.Add(new ValidationMessage() { FieldName = "$NewArtifactId", Message = release.VersionNumber });
					}
					catch (ArtifactNotExistsException)
					{
						//Do Nothing
					}
				}

				return validationMessages;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<DataItemField> Form_RetrieveWorkflowFieldStates(int projectId, int typeId, int stepId)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ITestExecutionService methods

        /// <summary>
        /// Adds an incident associations to the current test run step
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRunStepId">The id the current test run step</param>
        /// <param name="incidentIds">The list of incident ids to associate with the test run step</param>
        public void TestExecution_AddIncidentAssociation(int projectId, int testRunStepId, List<int> incidentIds)
        {
            const string METHOD_NAME = "TestExecution_AddIncidentAssociation";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, DataModel.Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business class and add the associations
                new IncidentManager().Incident_AssociateToTestRunStep(projectId, testRunStepId, incidentIds, userId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (DataValidationException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

		/// <summary>Retrieves a single test run step during execution together with the associated test run information</summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="userId">The id of the current user</param>
		/// <param name="testRunStepId">The id of the test run step</param>
		/// <returns>A data item containing the test run step</returns>
		public DataItem RetrieveTestRunStep(int projectId, int testRunStepId)
		{
			const string METHOD_NAME = "RetrieveTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the test run step and associated test run
				TestRunManager testRunManager = new TestRunManager();
                TestRunStep testRunStep = testRunManager.TestRunStep_RetrieveById(testRunStepId);
                if (testRunStep == null)
                {
					//If anything's been deleted manually, throw back a validation error.
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}
                TestRun testRun = testRunStep.TestRun;
                if (testRun == null)
                {
                    //If anything's been deleted manually, throw back a validation error.
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }

                int testRunStepPosition = testRunStep.Position;

				//Create the new data item
				DataItem dataItem = new DataItem();

				//Populate the standard information
				dataItem.PrimaryKey = testRunStepId;

				//The TestRunId field
				DataItemField dataItemField = new DataItemField();
				dataItemField.FieldName = "TestRunId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
				dataItemField.IntValue = testRun.TestRunId;
				dataItemField.TextValue = GlobalFunctions.ARTIFACT_PREFIX_TEST_RUN + String.Format(GlobalFunctions.FORMAT_ID, testRun.TestRunId);
				dataItem.Fields.Add("TestRunId", dataItemField);

				//The TestCaseId field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestCaseId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
				dataItemField.IntValue = testRun.TestCaseId;
				dataItemField.TextValue = GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE + String.Format(GlobalFunctions.FORMAT_ID, testRun.TestCaseId);
				dataItem.Fields.Add("TestCaseId", dataItemField);

				//The TestRunName field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestRunName";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Text;
				dataItemField.TextValue = testRun.Name;
				dataItem.Fields.Add("TestRunName", dataItemField);

				//The TestRunDescription field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestRunDescription";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                dataItemField.TextValue = String.IsNullOrEmpty(testRun.Description) ? "" : testRun.Description;
				dataItem.Fields.Add("TestRunDescription", dataItemField);

				//The Test Run Tester
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TesterId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
				dataItemField.IntValue = testRun.TesterId;
				dataItemField.TextValue = testRun.Tester.FullName;
				dataItem.Fields.Add("TesterId", dataItemField);

				//The TestRunStepId field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "TestRunStepId";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
				dataItemField.IntValue = testRunStep.TestRunStepId;
				dataItem.Fields.Add("TestRunStepId", dataItemField);

				//The test step id
				if (testRunStep.TestStepId.HasValue)
				{
					dataItemField = new DataItemField();
					dataItemField.FieldName = "TestStepId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = testRunStep.TestStepId.Value;
					dataItem.Fields.Add("TestStepId", dataItemField);
				}

				//The test case of the test step
				if (testRunStep.TestCaseId.HasValue)
				{
					dataItemField = new DataItemField();
					dataItemField.FieldName = "TestStepTestCaseId";
					dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
					dataItemField.IntValue = testRunStep.TestCaseId.Value;
					dataItem.Fields.Add("TestStepTestCaseId", dataItemField);
				}

				//The Position field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "Position";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
				dataItemField.IntValue = testRunStepPosition;
				dataItemField.TextValue = Resources.Main.Global_Step + " " + testRunStepPosition;
				dataItem.Fields.Add("Position", dataItemField);

				//The Description field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "Description";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
				dataItemField.TextValue = testRunStep.Description;
				dataItem.Fields.Add("Description", dataItemField);

				//The ExpectedResult field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ExpectedResult";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
				dataItemField.TextValue = String.IsNullOrEmpty(testRunStep.ExpectedResult) ? "" : testRunStep.ExpectedResult;
				dataItem.Fields.Add("ExpectedResult", dataItemField);

				//The SampleData field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "SampleData";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
				dataItemField.TextValue = String.IsNullOrEmpty(testRunStep.SampleData) ? "" : testRunStep.SampleData;
				dataItem.Fields.Add("SampleData", dataItemField);

				//The ActualResult field
				dataItemField = new DataItemField();
				dataItemField.FieldName = "ActualResult";
				dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
				dataItemField.TextValue = String.IsNullOrEmpty(testRunStep.ActualResult) ? "" : testRunStep.ActualResult;
				dataItem.Fields.Add("ActualResult", dataItemField);

				return dataItem;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (DataValidationException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Passes a test run step</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the current test run step</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <param name="incidentDataItem">The associated incident record</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
		public JsonDictionaryOfStrings PassTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem)
		{
			const string METHOD_NAME = "PassTestRunStep()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure we have at least one test step
				if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
				{
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
				}

				TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
				if (testRunRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}
				int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
				TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
				if (testRunStepRow == null)
				{
					throw new DataValidationException(String.Format(Resources.Messages.Global_TestRunStepDoesNotExist, testRunStepId));
				}

				//Make sure the user matches the owner of the test run (for security reasons)
				if (testRunRow.TesterId != userId)
				{
					throw new ArtifactAuthorizationException(Resources.Messages.TestExecutionService_NotAuthorizedToUpdateTestRun);
				}

				//Set the execution status, timing statuses, and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
				if (String.IsNullOrEmpty(actualResult))
				{
					testRunStepRow.ActualResult = null;
				}
				else
				{
					testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
				}

                //Get project settings
                ProjectSettings projectSettings = null;
                if (projectId > 0)
                {
                    projectSettings = new ProjectSettings(projectId);
                }
                //if the settings for this project always require an actual result, then send back message if actual result is empty
                if (projectSettings != null && projectSettings.Testing_ExecutionActualResultAlwaysRequired)
                {
                    //Check that scrubbed input isn't empty..
                    if (string.IsNullOrWhiteSpace(Strings.StripHTML(testRunStepRow.ActualResult)))
                    {
                        ValidationMessage valMsg = new ValidationMessage();
                        valMsg.FieldName = "ActualResult";
                        valMsg.Message = Resources.ClientScript.TestCaseExecution_ActualResultNeeded;
                        throw new DataValidationExceptionEx(new List<ValidationMessage>() { valMsg });
                    }

                }

                //Persist the changes
                testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

                //Check to see if we have to add a linked incident
                //We also need to check permissions, if not allowed, ignore
                int incidentId = -1;
				if (incidentDataItem != null && incidentDataItem.PrimaryKey < 1 && incidentDataItem.Fields.Count > 0 && IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident) == Project.AuthorizationState.Authorized)
				{
					incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow);
				}

				//Finally we need to send back the updated statuses so that the execution page can reflect the changes
				JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());

				//Return back the new incident id if created
				if (incidentId != -1)
				{
					executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
				}

				//Also return back two special values that the page will need to know which test case to next go to
				//and also if it should display the Finish button or not

				//If there are no test runs marked as 'not run' then we can display the finish button
				//Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

				//Find out the next test run step to display in the list
				int nextTestRunStepId = -1;
				int nextTestRunId = -1;
				//First see if we are the last step in the test run
				int testRunStepIndex = 0;
				for (int i = 0; i < testRunRow.TestRunSteps.Count; i++)
				{
					if (testRunStepRow == testRunRow.TestRunSteps[i])
					{
						testRunStepIndex = i;
						break;
					}
				}
				if (testRunStepIndex < testRunRow.TestRunSteps.Count - 1)
				{
					nextTestRunStepId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunStepId;
					nextTestRunId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunId;
				}
				else
				{
					//Find the next text run that has steps and get its first step
					if (testRunIndex < testRunsPending.TestRuns.Count - 1)
					{
						for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
						{
							if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
							{
								nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
								nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
								break;
							}
						}
					}
				}

				if (nextTestRunStepId != -1)
				{
					executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
					//See if the test run is different
					if (nextTestRunId != testRunId)
					{
						executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
					}
				}
				executionStatuses.Add("countNotRun", countNotRun.ToString());

				return executionStatuses;
			}
			catch (DataValidationExceptionEx)
			{
				//This is handled by the calling javascript function so we just throw
				throw;
			}
			catch (ArtifactAuthorizationException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (ArtifactNotExistsException exception)
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

		/// <summary>Passes all the test run steps in a test run</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
		public JsonDictionaryOfStrings PassAllTestRunSteps(int projectId, int testRunsPendingId, int testRunId, string actualResult, int testRunStepId, DateTime startDate, DateTime endDate, int actualDuration)
		{
			const string METHOD_NAME = "PassAllTestRunSteps()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
				//Get the pending test record and find the current test run and test run step
				TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure we have at least one test step
				if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
				{
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
				}

				TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
				if (testRunRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}
				int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);

				//Set the execution status and actual result for each step
				foreach (TestRunStep testRunStepRow in testRunRow.TestRunSteps)
				{
                    testRunStepRow.StartTracking();
                    testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;

                    //Get project settings
                    ProjectSettings projectSettings = null;
                    if (projectId > 0)
                    {
                        projectSettings = new ProjectSettings(projectId);
                    }
                    //if the settings for this project always require an actual result, then send back message as soon as an empty actual result is found (if any)
                    if (projectSettings != null && projectSettings.Testing_ExecutionActualResultAlwaysRequired)
                    {
                        //Check that scrubbed input isn't empty..
                        if (string.IsNullOrWhiteSpace(Strings.StripHTML(testRunStepRow.ActualResult)))
                        {
                            ValidationMessage valMsg = new ValidationMessage();
                            valMsg.FieldName = "ActualResult";
                            valMsg.Message = Resources.ClientScript.TestCaseExecution_ActualResultNeededPassAll;
                            throw new DataValidationExceptionEx(new List<ValidationMessage>() { valMsg });
                        }

                    }

                    //Set the duration and timing values for the first step in the run
                    if (testRunStepRow.TestRunStepId == testRunStepId)
                    {
                        testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                        testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                        testRunStepRow.ActualDuration = actualDuration;
                    }
                    //Set the remaining steps' start and end date to the end date of step 1 and duration to 0 - only if values not already entered
                    else
                    {
                        if (!testRunStepRow.StartDate.HasValue)
                        {
                            testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(endDate);
                        }
                        if (!testRunStepRow.EndDate.HasValue)
                        {
                            testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                        }
                        if (!testRunStepRow.ActualDuration.HasValue)
                        {
                            testRunStepRow.ActualDuration = 0;
                        }
                    }
					
					//make sure any existing values are not overwritten
                    if (String.IsNullOrWhiteSpace(testRunStepRow.ActualResult))
                    {
                        if (String.IsNullOrWhiteSpace(actualResult))
					    {
						    testRunStepRow.ActualResult = null;
					    }
					    else
					    {
                            testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
                        }
					}
				}
				//Persist the changes
                testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

				//Finally we need to send back the updated statuses so that the execution page can reflect the changes
				JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());
				//Also return back two special values that the page will need to know which test case to next go to
				//and also if it should display the Finish button or not

				//If there are no test runs marked as 'not run' then we can display the finish button
				//Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

				//Find out the next test run step to display in the list
				int nextTestRunStepId = -1;
				int nextTestRunId = -1;
				//Find the next text run that has steps and get its first step
				if (testRunIndex < testRunsPending.TestRuns.Count - 1)
				{
					for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
					{
						if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
						{
							nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
							nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
							break;
						}
					}
				}

				if (nextTestRunStepId != -1)
				{
					executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
					//See if the test run is different
					if (nextTestRunId != testRunId)
					{
						executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
					}
				}
				executionStatuses.Add("countNotRun", countNotRun.ToString());

				return executionStatuses;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
            catch (DataValidationExceptionEx)
			{
                //Do not log
                throw;
            }
            catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Fails a test run step</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the current test run step</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <param name="incidentDataItem">The associated incident record</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public JsonDictionaryOfStrings FailTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem)
		{
			const string METHOD_NAME = "FailTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
				{
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
				}

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
				if (testRunRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}

                //make sure that are allowed to test this case
                if (userId != testRunRow.TesterId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
				if (testRunStepRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}

				//Set the execution status and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
				if (String.IsNullOrEmpty(actualResult))
				{
					testRunStepRow.ActualResult = null;
				}
				else
				{
					testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
				}

				//Check that scrubbed input isn't empty..
				if (string.IsNullOrWhiteSpace(Strings.StripHTML(testRunStepRow.ActualResult)))
				{
					ValidationMessage valMsg = new ValidationMessage();
					valMsg.FieldName = "ActualResult";
					valMsg.Message = Resources.ClientScript.TestCaseExecution_ActualResultNeeded;
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { valMsg });
				}

				//Persist the changes
				testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

				//Check to see if we have to add a linked incident
                //We also need to check permissions, if not allowed, ignore
				int incidentId = -1;
                if (incidentDataItem != null && incidentDataItem.PrimaryKey < 1 && incidentDataItem.Fields.Count > 0 && IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident) == Project.AuthorizationState.Authorized)
				{
					incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow);
				}

				//Finally we need to send back the updated statuses so that the execution page can reflect the changes
				JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());

				//Return back the new incident id if created
				if (incidentId != -1)
				{
					executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
				}

				//Also return back two special values that the page will need to know which test case to next go to
				//and also if it should display the Finish button or not

				//If there are no test runs marked as 'not run' then we can display the finish button
				//Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

				//Find out the next test run step to display in the list
				int nextTestRunStepId = -1;
				int nextTestRunId = -1;
				//Find the next text run that has steps and get its first step
				if (testRunIndex < testRunsPending.TestRuns.Count - 1)
				{
					for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
					{
						if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
						{
							nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
							nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
							break;
						}
					}
				}

				if (nextTestRunStepId != -1)
				{
					executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
					//See if the test run is different
					if (nextTestRunId != testRunId)
					{
						executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
					}
				}
				executionStatuses.Add("countNotRun", countNotRun.ToString());

				return executionStatuses;
			}
			catch (DataValidationExceptionEx)
			{
				//This is handled by the calling javascript function so we just throw
				throw;
			}
			catch (ArtifactNotExistsException exception)
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

		/// <summary>Blocks a test run step</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the current test run step</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <param name="incidentDataItem">The associated incident record</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public JsonDictionaryOfStrings BlockTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem)
		{
			const string METHOD_NAME = "BlockTestRunStep()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure we have at least one test step
				if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
				{
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
				}

				TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
				if (testRunRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}
				int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
				TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
				if (testRunStepRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}

				//Set the execution status and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
				if (String.IsNullOrEmpty(actualResult))
				{
					testRunStepRow.ActualResult = null;
				}
				else
				{
					testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
				}

				//Check that scrubbed input isn't empty..
				if (string.IsNullOrWhiteSpace(Strings.StripHTML(testRunStepRow.ActualResult)))
				{
					ValidationMessage valMsg = new ValidationMessage();
					valMsg.FieldName = "ActualResult";
					valMsg.Message = Resources.ClientScript.TestCaseExecution_ActualResultNeeded;
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { valMsg });
				}

				//Persist the changes
                testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

                //Check to see if we have to add a linked incident
                //We also need to check permissions, if not allowed, ignore
                int incidentId = -1;
                if (incidentDataItem != null && incidentDataItem.PrimaryKey < 1 && incidentDataItem.Fields.Count > 0 && IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident) == Project.AuthorizationState.Authorized)
				{
					incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow);
				}

				//Finally we need to send back the updated statuses so that the execution page can reflect the changes
				JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());

				//Return back the new incident id if created
				if (incidentId != -1)
				{
					executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
				}

				//Also return back two special values that the page will need to know which test case to next go to
				//and also if it should display the Finish button or not

				//If there are no test runs marked as 'not run' then we can display the finish button
				//Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

				//Find out the next test run step to display in the list
				int nextTestRunStepId = -1;
				int nextTestRunId = -1;
				//Find the next text run that has steps and get its first step
				if (testRunIndex < testRunsPending.TestRuns.Count - 1)
				{
					for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
					{
						if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
						{
							nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
							nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
							break;
						}
					}
				}

				if (nextTestRunStepId != -1)
				{
					executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
					//See if the test run is different
					if (nextTestRunId != testRunId)
					{
						executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
					}
				}
				executionStatuses.Add("countNotRun", countNotRun.ToString());

				return executionStatuses;
			}
			catch (DataValidationExceptionEx)
			{
				//This is handled by the calling javascript function so we just throw
				throw;
			}
			catch (ArtifactNotExistsException exception)
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

        /// <summary>Marks a test run step as N/A</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the current test run step</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <param name="incidentDataItem">The associated incident record</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public JsonDictionaryOfStrings NotApplicableTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem)
        {
            const string METHOD_NAME = "PassTestRunStep()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
                }

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
                if (testRunRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }
                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
                if (testRunStepRow == null)
                {
                    throw new DataValidationException(String.Format(Resources.Messages.Global_TestRunStepDoesNotExist, testRunStepId));
                }

                //Make sure the user matches the owner of the test run (for security reasons)
                if (testRunRow.TesterId != userId)
                {
                    throw new ArtifactAuthorizationException(Resources.Messages.TestExecutionService_NotAuthorizedToUpdateTestRun);
                }

                //Set the execution status and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
                if (String.IsNullOrEmpty(actualResult))
                {
                    testRunStepRow.ActualResult = null;
                }
                else
                {
                    testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
                }
                //Persist the changes
                testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

                //Check to see if we have to add a linked incident
                //We also need to check permissions, if not allowed, ignore
                int incidentId = -1;
                if (incidentDataItem != null && incidentDataItem.PrimaryKey < 1 && incidentDataItem.Fields.Count > 0 && IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident) == Project.AuthorizationState.Authorized)
                {
                    incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow);
                }

                //Finally we need to send back the updated statuses so that the execution page can reflect the changes
                JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());

                //Return back the new incident id if created
                if (incidentId != -1)
                {
                    executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
                }

                //Also return back two special values that the page will need to know which test case to next go to
                //and also if it should display the Finish button or not

                //If there are no test runs marked as 'not run' then we can display the finish button
                //Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

                //Find out the next test run step to display in the list
                int nextTestRunStepId = -1;
                int nextTestRunId = -1;
                //First see if we are the last step in the test run
                int testRunStepIndex = 0;
                for (int i = 0; i < testRunRow.TestRunSteps.Count; i++)
                {
                    if (testRunStepRow == testRunRow.TestRunSteps[i])
                    {
                        testRunStepIndex = i;
                        break;
                    }
                }
                if (testRunStepIndex < testRunRow.TestRunSteps.Count - 1)
                {
                    nextTestRunStepId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunStepId;
                    nextTestRunId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunId;
                }
                else
                {
                    //Find the next text run that has steps and get its first step
                    if (testRunIndex < testRunsPending.TestRuns.Count - 1)
                    {
                        for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
                        {
                            if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
                            {
                                nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
                                nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
                                break;
                            }
                        }
                    }
                }

                if (nextTestRunStepId != -1)
                {
                    executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
                    //See if the test run is different
                    if (nextTestRunId != testRunId)
                    {
                        executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
                    }
                }
                executionStatuses.Add("countNotRun", countNotRun.ToString());

                return executionStatuses;
            }
            catch (DataValidationExceptionEx)
            {
                //This is handled by the calling javascript function so we just throw
                throw;
            }
            catch (ArtifactAuthorizationException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (ArtifactNotExistsException exception)
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

		/// <summary>Cautions a test run step</summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id of the current test run step</param>
		/// <param name="testRunId">The id of the current test run</param>
		/// <param name="testRunsPendingId">The id of the pending test runs entry</param>
		/// <param name="actualResult">The actual result</param>
		/// <param name="incidentDataItem">The associated incident record</param>
		/// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public JsonDictionaryOfStrings CautionTestRunStep(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DateTime startDate, DateTime endDate, int actualDuration, DataItem incidentDataItem)
		{
			const string METHOD_NAME = "CautionTestRunStep()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're authenticated
			if (!this.CurrentUserId.HasValue)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
			}
			int userId = this.CurrentUserId.Value;

			//Make sure we're authorized
			Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
			if (authorizationState == Project.AuthorizationState.Prohibited)
			{
				throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
			}

			try
			{
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
				TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

				//Make sure we have at least one test step
				if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
				{
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
				}

				TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
				if (testRunRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}
				int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
				TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
				if (testRunStepRow == null)
				{
					throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
				}

				//Set the execution status and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
				if (String.IsNullOrEmpty(actualResult))
				{
					testRunStepRow.ActualResult = null;
				}
				else
				{
					testRunStepRow.ActualResult = GlobalFunctions.HtmlScrubInput(actualResult.Trim());
				}

				//Check that scrubbed input isn't empty..
				if (string.IsNullOrWhiteSpace(Strings.StripHTML(testRunStepRow.ActualResult)))
				{
					ValidationMessage valMsg = new ValidationMessage();
					valMsg.FieldName = "ActualResult";
					valMsg.Message = Resources.ClientScript.TestCaseExecution_ActualResultNeeded;
					throw new DataValidationExceptionEx(new List<ValidationMessage>() { valMsg });
				}

				//Persist the changes
                testRunManager.UpdateExecutionStatus(projectId, userId, testRunsPending, testRunIndex, DateTime.UtcNow, true);

                //Check to see if we have to add a linked incident
                //We also need to check permissions, if not allowed, ignore
                int incidentId = -1;
				if (incidentDataItem != null && incidentDataItem.PrimaryKey < 1 && incidentDataItem.Fields.Count > 0 && IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Incident) == Project.AuthorizationState.Authorized)
				{
					incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow);
				}

				//Finally we need to send back the updated statuses so that the execution page can reflect the changes
				JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();
                executionStatuses.Add("TestRunId", testRunRow.TestRunId.ToString());
                executionStatuses.Add("TestRunExecutionStatusId", testRunRow.ExecutionStatusId.ToString());

				//Return back the new incident id if created
				if (incidentId != -1)
				{
					executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
				}

				//Also return back two special values that the page will need to know which test case to next go to
				//and also if it should display the Finish button or not

				//If there are no test runs marked as 'not run' then we can display the finish button
				//Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

				//Find out the next test run step to display in the list
				int nextTestRunStepId = -1;
				int nextTestRunId = -1;
				//First see if we are the last step in the test run
				int testRunStepIndex = 0;
				for (int i = 0; i < testRunRow.TestRunSteps.Count; i++)
				{
					if (testRunStepRow == testRunRow.TestRunSteps[i])
					{
						testRunStepIndex = i;
						break;
					}
				}
				if (testRunStepIndex < testRunRow.TestRunSteps.Count - 1)
				{
					nextTestRunStepId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunStepId;
					nextTestRunId = testRunRow.TestRunSteps[testRunStepIndex + 1].TestRunId;
				}
				else
				{
					//Find the next text run that has steps and get its first step
					if (testRunIndex < testRunsPending.TestRuns.Count - 1)
					{
						for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
						{
							if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
							{
								nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
								nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
								break;
							}
						}
					}
				}

				if (nextTestRunStepId != -1)
				{
					executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
					//See if the test run is different
					if (nextTestRunId != testRunId)
					{
						executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
					}
				}
				executionStatuses.Add("countNotRun", countNotRun.ToString());

				return executionStatuses;
			}
			catch (DataValidationExceptionEx)
			{
				//This is handled by the calling javascript function so we just throw
				throw;
			}
			catch (ArtifactNotExistsException exception)
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


        /// <summary>Updates the actual result text field of a test run step during executin</summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRunStepId">The id of the current test run step</param>
        /// <param name="testRunId">The id of the current test run</param>
        /// <param name="testRunsPendingId">The id of the pending test runs entry</param>
        /// <param name="textField">the actual text</param>
        /// <returns>Nothings needs to be returned</returns>
        public void UpdateTestRunActualResult(
            int projectId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string textField
            )
        {
            const string METHOD_NAME = "UpdateTestRunActualResult()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
                }

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
                if (testRunRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }
                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                testRunRow.StartTracking();

                //Make sure the user matches the owner of the test run (for security reasons)
                if (testRunRow.TesterId != userId)
                {
                    throw new ArtifactAuthorizationException(Resources.Messages.TestExecutionService_NotAuthorizedToUpdateTestRun);
                }

                //Update the actual result field
                //Check the test run id exists on the test run
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
                if (testRunStepRow == null)
                {
                    throw new DataValidationException(String.Format(Resources.Messages.Global_TestRunStepDoesNotExist, testRunStepId));
                }

                testRunStepRow.StartTracking();
                        
                //Set the actual result
                testRunStepRow.ActualResult = String.IsNullOrEmpty(textField) ? null : GlobalFunctions.HtmlScrubInput(textField.Trim());
        
                //Persist the changes
                testRunManager.UpdateExploratoryTestRun(projectId, userId, testRunsPending);
            }
            catch (DataValidationExceptionEx)
            {
                //This is handled by the calling javascript function so we just throw
                throw;
            }
            catch (ArtifactAuthorizationException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (ArtifactNotExistsException exception)
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
        /// Saves a new incident linked to the current test run step from the incident form
        /// </summary>
        /// <param name="incidentDataItem">The incident information</param>
        /// <param name="testRunRow">The Test Run row the incident is related to</param>
        /// <param name="testRunStep">The test run step the incident is related to</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="userId">The id of the current user</param>
        /// <param name="overrideActualResult">An actual result to store, if we don't want to use the one in the test step</param>
        /// <remarks>If there are documents attached to the test run then they are copied to the incident</remarks>
        /// <returns>The id of the new incident (if appropriate)</returns>
        protected int SaveLinkedIncident(int userId, int projectId, int projectTemplateId, DataItem incidentDataItem, TestRunStep testRunStep, string overrideActualResult = null)
		{
            const string METHOD_NAME = "SaveLinkedIncident";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the values from the data item object
			IncidentManager incidentManager = new IncidentManager();
			TestCaseManager testCaseManager = new TestCaseManager();
            int incidentStatusId = incidentManager.IncidentStatus_RetrieveDefault(projectTemplateId).IncidentStatusId;
			int incidentTypeId = (incidentDataItem.Fields["IncidentTypeId"].IntValue.HasValue) ? incidentDataItem.Fields["IncidentTypeId"].IntValue.Value : -1;

			//The description is populated from the expected and actual results
			//The format used depends on whether we have rich-text editing enabled or not
			string actualResult = "";
            if (String.IsNullOrEmpty(overrideActualResult))
            {
                if (!String.IsNullOrEmpty(testRunStep.ActualResult))
                {
                    actualResult = testRunStep.ActualResult;
                }
            }
            else
            {
                actualResult = overrideActualResult;
            }
			string expectedResult = "";
			if (!String.IsNullOrEmpty(testRunStep.ExpectedResult))
			{
				expectedResult = testRunStep.ExpectedResult;
			}

            //Get the test run entity for this test run step
            TestRun testRun = testRunStep.TestRun;

			string incidentDescription = "<b><u>" + Resources.Main.TestExecutionService_IncidentText_Description + "</u></b><br />\n" + testRunStep.Description + "<br /><br />\n<b><u>" + Resources.Main.TestExecutionService_IncidentText_ExpectedResult + "</u></b><br />\n" + expectedResult + "<br /><br />\n<b><u>" + Resources.Main.TestExecutionService_IncidentText_ActualResult + "</u></b><br />\n" + actualResult + "<br />";
			//If we have all the test steps, include them underneath as additional information
            TrackableCollection<TestRunStep> testRunSteps = testRun.TestRunSteps;
			if (testRunSteps != null && testRun.TestRunSteps.Count > 0)
			{
				incidentDescription += "<br/><b><u>" + Resources.ServerControls.TabControl_TestRunSteps + "</u></b>\n<ol>\n";
                foreach (TestRunStep testRunStepItem in testRunSteps)
				{
					//The ExecutionStatusName will not have the current status since the row didn't come from the database
					//and has just been updated in memory. Therefore we'll need to use the static lookup method instead
					//Highlight the step if this is the current row
                    if (testRunStepItem.TestRunStepId == testRunStep.TestRunStepId)
					{
                        incidentDescription += "<li><strong>" + testRunStepItem.Description + " = " + GlobalFunctions.LocalizeFields(testCaseManager.GetExecutionStatusName(testRunStepItem.ExecutionStatusId)) + "</strong></li>\n";
					}
					else
					{
                        incidentDescription += "<li>" + testRunStepItem.Description + " = " + GlobalFunctions.LocalizeFields(testCaseManager.GetExecutionStatusName(testRunStepItem.ExecutionStatusId)) + "</li>\n";
					}
				}
				incidentDescription += "</ol>\n";
			}

			//The detected release and incident description come from the test run, so we need to add to the data item
			if (testRun.ReleaseId.HasValue)
			{
				incidentDataItem.Fields["DetectedReleaseId"].IntValue = testRun.ReleaseId;
			}
			incidentDataItem.Fields["Description"].TextValue = incidentDescription;

			//Next we need to get any custom properties
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, false);
			ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Incident, -1, customProperties);
			artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);

			//Get the list of workflow fields and custom properties
			WorkflowManager workflowManager = new WorkflowManager();
			int workflowId;
			if (incidentTypeId < 1)
			{
				workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).WorkflowId;
			}
			else
			{
				workflowId = workflowManager.Workflow_GetForIncidentType(incidentTypeId);
			}
			List<WorkflowField> workflowFields = workflowManager.Workflow_RetrieveFieldStates(workflowId, incidentStatusId);
			List<WorkflowCustomProperty> workflowCustomProps = workflowManager.Workflow_RetrieveCustomPropertyStates(workflowId, incidentStatusId);

			//The return list..
			List<ValidationMessage> validationMessages = new List<ValidationMessage>();

			//Create a new incident data set to pass to the populate function
			int incidentId = -1; //Signifies new incident
            IncidentView incident = incidentManager.Incident_New(projectId, userId);

			//Update the field values
			List<string> fieldsToIgnore = new List<string>();
			fieldsToIgnore.Add("Resolution");
			fieldsToIgnore.Add("CreationDate");
			fieldsToIgnore.Add("LastUpdateDate");
			fieldsToIgnore.Add("PlaceholderId");
            UpdateFields(validationMessages, incidentDataItem, incident, customProperties, artifactCustomProperty, projectId, incidentId, 0, fieldsToIgnore, workflowFields, workflowCustomProps);

			//Now verify the options for the custom properties to make sure all rules have been followed
			Dictionary<string, string> customPropOptionMessages = customPropertyManager.CustomProperty_Check(customProperties, artifactCustomProperty);
			foreach (KeyValuePair<string, string> customPropOptionMessage in customPropOptionMessages)
			{
				ValidationMessage newMsg = new ValidationMessage();
				newMsg.FieldName = customPropOptionMessage.Key;
				newMsg.Message = customPropOptionMessage.Value;
				AddUniqueMessage(validationMessages, newMsg);
			}

			//If we have any validation messages, throw an exception and stop at this point
			if (validationMessages.Count > 0)
			{
				throw new DataValidationExceptionEx(validationMessages);
			}

			//Now, insert the new incident (with linked to the test run step)
			// - Get the build ID first..
			int? buildId = ((!testRun.BuildId.HasValue || testRun.BuildId < 1) ? new int?() : testRun.BuildId);
            string name = incident.Name;
            int? priorityId = incident.PriorityId;
            int? severityId = incident.SeverityId;
            int? ownerId = incident.OwnerId;
            int? detectedReleaseId = incident.DetectedReleaseId;
            int? resolvedReleaseId = incident.ResolvedReleaseId;
            int? verifiedReleaseId = incident.VerifiedReleaseId;
			DateTime creationDate = DateTime.UtcNow;
            List<int> componentIds = null;
            if (!String.IsNullOrEmpty(incident.ComponentIds))
            {
                componentIds = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
            }
			incidentId = incidentManager.Insert(projectId, priorityId, severityId, testRun.TesterId, ownerId, new List<int>() { testRunStep.TestRunStepId }, name, incidentDescription, detectedReleaseId, resolvedReleaseId, verifiedReleaseId, incidentTypeId, null, creationDate, null, null, null, null, null, buildId, componentIds, userId, true);

			//Now save the custom properties
			artifactCustomProperty.ArtifactId = incidentId;
			customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);

			//Finally copy across any attachments linked to the test run to the incident as well
			AttachmentManager attachment = new AttachmentManager();
			attachment.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRun.TestRunId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId);

            //Send the creation notification
            incidentManager.SendCreationNotification(incidentId, artifactCustomProperty, null);

			//Return the new incident id
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return incidentId;
		}

		#endregion

		#region INavigationService Methods

		/// <summary>
		/// Updates the display mode and width of the test execution navigation bar
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
				ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_EXECUTION_GENERAL_SETTINGS);
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

		public JsonDictionaryOfStrings NavigationBar_RetrievePaginationOptions(int projectId)
		{
			throw new NotImplementedException();
		}

		public void NavigationBar_UpdatePagination(int projectId, int pageSize, int currentPage)
		{
			throw new NotImplementedException();
		}

		public List<HierarchicalDataItem> NavigationBar_RetrieveList(int projectId, string indentLevel, int displayMode, int? selectedItemId, int? containerId)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Internal Functions

		/// <summary>
		/// Populates a data item from a test run entity
		/// </summary>
		/// <param name="dataItem">The dataitem to be populated</param>
		/// <param name="testRun">The datarow containing the data</param>
		/// <param name="customProperties">The list of custom property definitions and values</param>
		/// <param name="editable">Does the data need to be in editable form?</param>
		/// <param name="testRunsPendingId">The id of the pending test run</param>
		/// <param name="artifactCustomProperty">The artifact's custom property data (if not provided as part of dataitem) - pass null if not used</param>
		protected void PopulateRow(SortedDataItem dataItem, int testRunsPendingId, TestRun testRun, List<CustomProperty> customProperties, bool editable, ArtifactCustomProperty artifactCustomProperty)
		{
			//Set the primary key
			dataItem.PrimaryKey = testRunsPendingId;

			//Specify if it has an attachment or not
			dataItem.Attachment = testRun.IsAttachments;

			//Iterate through all the fields and get the corresponding values
			foreach (KeyValuePair<string, DataItemField> dataItemFieldKVP in dataItem.Fields)
			{
				string fieldName = dataItemFieldKVP.Key;
				DataItemField dataItemField = dataItemFieldKVP.Value;
                if (testRun.ContainsProperty(dataItemField.FieldName) || (artifactCustomProperty != null && artifactCustomProperty.ContainsProperty(dataItemField.FieldName)))
				{
					//First populate the data-item from the data-row
					PopulateFieldRow(dataItem, dataItemField, testRun, customProperties, artifactCustomProperty, editable, null);
				}
			}

			//If we have a specified test set, then make the first page control(s) read-only
			//if a release is specified since we don't want a user overriding the set values
			if (testRun.TestSetId.HasValue)
			{
				if (testRun.ReleaseId.HasValue)
				{
					dataItem.Fields["ReleaseId"].Editable = false;
				}
                else
				{
					dataItem.Fields["ReleaseId"].Editable = true;
				}

				//Also if any of the custom properties are already set then make them read-only too
				if (artifactCustomProperty != null)
				{
					//Loop through all the defined custom properties
					foreach (CustomProperty customProperty in customProperties)
					{
						//Only lock list/multi-list types
						if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
						{
							//Make sure the field exists
							if (dataItem.Fields.ContainsKey(customProperty.CustomPropertyFieldName))
							{
								//See if we have a value
								object value = artifactCustomProperty.CustomProperty(customProperty.PropertyNumber);
								if (value != null)
								{
									dataItem.Fields[customProperty.CustomPropertyFieldName].Editable = false;
								}
							}
						}
					}
				}
			}
		}

		#endregion

        #region Test Execution methods

        /// <summary>
        /// Create the model in a set way to enable knockout to correctly parse it
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="testRunsPendingId"></param>
        /// <returns></returns>
        public TestRunsPendingModel TestExecution_RetrieveTestRunsPending(int projectId, int testRunsPendingId)
        {
            const string METHOD_NAME = CLASS_NAME + "Form_Retrieve";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to create test runs
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate the business classes
                TestRunManager testRunManager = new TestRunManager();
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                //Retrieve the test run pending entry
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure the projects match, otherwise we're not really authorized
                if (testRunsPending.ProjectId != projectId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Make sure that the current user is the owner of this pending test run or at least one of the test runs in the group
                bool isAnOwner = false;
                if (testRunsPending.TesterId == userId)
                {
                    isAnOwner = true;
                }
                foreach (TestRun testRun in testRunsPending.TestRuns)
                {
                    if (testRun.TesterId == userId)
                    {
                        isAnOwner = true;
                    }
                }
                if (!isAnOwner)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Make sure that we have at least one test run in the pending list
                if (testRunsPending.TestRuns.Count == 0)
                {
                    //Just return no data back
                    Logger.LogWarningEvent(METHOD_NAME, Resources.Messages.TestExecutionService_NoTestRunsFound);
                    return null;
                }

                TestRunsPendingModel testRunsPendingModel = new TestRunsPendingModel();

                //Populate the pending runs
                testRunsPendingModel.TestRunsPendingId = testRunsPending.TestRunsPendingId;
                testRunsPendingModel.ProjectId = testRunsPending.ProjectId;
                testRunsPendingModel.TestSetId = testRunsPending.TestSetId;
                testRunsPendingModel.Name = testRunsPending.Name;
                testRunsPendingModel.CountPassed = testRunsPending.CountPassed;
                testRunsPendingModel.CountFailed = testRunsPending.CountFailed;
                testRunsPendingModel.CountBlocked = testRunsPending.CountBlocked;
                testRunsPendingModel.CountCaution = testRunsPending.CountCaution;
                testRunsPendingModel.CountNotRun = testRunsPending.CountNotRun;
                testRunsPendingModel.CountNotApplicable = testRunsPending.CountNotApplicable;
                

                //Populate the user settings
                //First get the right settings collection
                ProjectSettingsCollection projectSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_EXECUTION_GENERAL_SETTINGS);
                UserSettingsCollection userSettingsCollection = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);

                //Then define the new user settings list
                testRunsPendingModel.Settings = new List<UserSettings>();
                UserSettings userSettings = new UserSettings();
                
                //Add the user settings if present
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_MAIN] is Int32)
                {
                    userSettings.DisplayModeMain = (int)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_MAIN];
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_SUB] is Int32)
                {
                    userSettings.DisplayModeSub = (int)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_SUB];
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN] is bool)
                {
                    userSettings.AlwaysShowTestRun = (bool)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN];
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES] is bool)
                {
                    userSettings.ShowCustomProperties = (bool)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES];
                }
                if (userSettingsCollection[GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN] is bool)
                {
                    userSettings.GuidedTourSeen = (bool)userSettingsCollection[GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN];
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_ID] is Int32)
                {
                    userSettings.CurrentTestRunId = (int)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_ID];
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_STEP_ID] is Int32)
                {
                    userSettings.CurrentTestRunStepId = (int)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_STEP_ID];
                }
                testRunsPendingModel.Settings.Add(userSettings);

                //Populate the test runs (only those which have steps)
                if (testRunsPending.TestRuns != null && testRunsPending.TestRuns.Count > 0)
                {
                    testRunsPendingModel.ReleaseId = testRunsPending.TestRuns[0].ReleaseId;
                    if (testRunsPending.TestRuns[0].Release == null)
                    {
                        testRunsPendingModel.ReleaseVersion = null;
                    }
                    else
                    {
                        testRunsPendingModel.ReleaseVersion = testRunsPending.TestRuns[0].Release.VersionNumber;
                    }

                    testRunsPendingModel.TestRuns = new List<TestRunModel>();
                    foreach (TestRun testRun in testRunsPending.TestRuns)
                    {
                        
                        if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
                        {
                            TestRunModel testRunModel = new TestRunModel();
                            testRunModel.TestRunId = testRun.TestRunId;
                            testRunModel.TestCaseId = testRun.TestCaseId;
                            testRunModel.ReleaseId = testRun.ReleaseId;
                            testRunModel.TesterId = testRun.TesterId;
                            testRunModel.Name = testRun.Name;
                            testRunModel.Description = testRun.Description;
                            testRunModel.ExecutionStatusId = testRun.ExecutionStatusId;
                            testRunModel.StartDate = GlobalFunctions.LocalizeDate(testRun.StartDate);
                            testRunModel.EndDate = GlobalFunctions.LocalizeDate(testRun.EndDate);
                            testRunModel.ActualDuration = testRun.ActualDuration;

                            //Now add the test run steps to the test run
                            testRunModel.TestRunSteps = new List<TestRunStepModel>();
                            foreach (TestRunStep testRunStep in testRun.TestRunSteps)
                            {
                                TestRunStepModel testRunStepModel = new TestRunStepModel();
                                testRunStepModel.TestRunStepId = testRunStep.TestRunStepId;
                                testRunStepModel.TestStepId = testRunStep.TestStepId;
                                testRunStepModel.TestCaseId = testRunStep.TestCaseId;
                                testRunStepModel.Description = testRunStep.Description;
                                testRunStepModel.Position = testRunStep.Position;
                                testRunStepModel.ExpectedResult = testRunStep.ExpectedResult;
                                testRunStepModel.SampleData = testRunStep.SampleData;
                                testRunStepModel.ActualResult = testRunStep.ActualResult;
                                testRunStepModel.ExecutionStatusId = testRunStep.ExecutionStatusId;
                                testRunStepModel.StartDate = GlobalFunctions.LocalizeDate(testRunStep.StartDate);
                                testRunStepModel.EndDate = GlobalFunctions.LocalizeDate(testRunStep.EndDate);
                                testRunStepModel.ActualDuration = testRunStep.ActualDuration;

                                testRunModel.TestRunSteps.Add(testRunStepModel);
                            }
                            testRunsPendingModel.TestRuns.Add(testRunModel);
                        }
                        
                    }
                }

                return testRunsPendingModel;
            }
            catch (System.ServiceModel.FaultException exception)
            {
                //Just rethrow
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }
            catch (ArtifactNotExistsException)
            {
                //Just return no data back
                Logger.LogWarningEvent(METHOD_NAME, Resources.Messages.TestExecutionService_PendingTestRunDoesNotExist);
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                throw;
            }

        }

        /// <summary>Logs a new incident against a specific test run step</summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRunStepId">The id of the current test run step</param>
        /// <param name="testRunId">The id of the current test run</param>
        /// <param name="testRunsPendingId">The id of the pending test runs entry</param>
        /// <param name="actualResult">The actual result</param>
        /// <param name="incidentDataItem">The associated incident record</param>
        /// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public JsonDictionaryOfStrings TestExecution_LogIncident(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, string actualResult, DataItem incidentDataItem)
        {
            const string METHOD_NAME = "TestExecution_LogIncident";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
                }

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
                if (testRunRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }

                //make sure that are allowed to test this case
                if (userId != testRunRow.TesterId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
                if (testRunStepRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }

                //Make sure we have an incident
                if (incidentDataItem == null)
                {
                    throw new ApplicationException(Resources.Messages.TestExecutionService_NeedToProvideIncident);
                }

                //Check to see if we have to add a linked incident
                int incidentId = SaveLinkedIncident(userId, projectId, projectTemplateId, incidentDataItem, testRunStepRow, actualResult);

                //Finally we need to send back the updated statuses so that the execution page can reflect the changes
                JsonDictionaryOfStrings executionStatuses = new JsonDictionaryOfStrings();

                //Return back the new incident id if created
                if (incidentId != -1)
                {
                    executionStatuses.Add("incidentId", GlobalFunctions.ARTIFACT_PREFIX_INCIDENT + String.Format(GlobalFunctions.FORMAT_ID, incidentId));
                }

                //Also return back two special values that the page will need to know which test case to next go to
                //and also if it should display the Finish button or not

                //If there are no test runs marked as 'not run' then we can display the finish button
                //Any tests without steps don't count
                int testsWithoutSteps = testRunsPending.TestRuns.Count(t => t.TestRunSteps.Count == 0);
                int countNotRun = testRunsPending.CountNotRun - testsWithoutSteps;

                //Find out the next test run step to display in the list
                int nextTestRunStepId = -1;
                int nextTestRunId = -1;
                //Find the next text run that has steps and get its first step
                if (testRunIndex < testRunsPending.TestRuns.Count - 1)
                {
                    for (int j = 1; testRunIndex + j < testRunsPending.TestRuns.Count; j++)
                    {
                        if (testRunsPending.TestRuns[testRunIndex + j].TestRunSteps.Count > 0)
                        {
                            nextTestRunStepId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunStepId;
                            nextTestRunId = testRunsPending.TestRuns[testRunIndex + j].TestRunSteps[0].TestRunId;
                            break;
                        }
                    }
                }

                if (nextTestRunStepId != -1)
                {
                    executionStatuses.Add("nextNode", "ts" + nextTestRunStepId);
                    //See if the test run is different
                    if (nextTestRunId != testRunId)
                    {
                        executionStatuses.Add("nextParentNode", "tr" + nextTestRunId);
                    }
                }
                executionStatuses.Add("countNotRun", countNotRun.ToString());

                return executionStatuses;
            }
            catch (DataValidationExceptionEx)
            {
                //This is handled by the calling javascript function so we just throw
                throw;
            }
            catch (ArtifactNotExistsException exception)
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

        /// <summary>Logs a new task against a specific test run step</summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testCaseId">The id of the test case the test run originates from</param>
        /// <param name="testRunsPendingId">The id of the pending test runs entry</param>
        /// <param name="testRunId">The id of the current test run</param>
        /// <param name="testRunStepId">The id of the current test run step</param>
        /// <param name="name">The name of the task</param>
        /// <param name="incidentDataItem">The optional description of the task</param>
        /// <param name="ownerId">The optional id of the owner to whom the task is to be assigned</param>
        /// <returns>The ID of the newly created task</returns>
        public int TestExecution_LogTask(
            int projectId,
            int testCaseId,
            int testRunsPendingId,
            int testRunId,
            int testRunStepId,
            string name,
            string description,
            int? ownerId
            )
        {

            const string METHOD_NAME = "TestExecution_LogTask";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.Task);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
                }

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
                if (testRunRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }

                //make sure that are allowed to test this case
                if (userId != testRunRow.TesterId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
                if (testRunStepRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }

                //Make sure there is at least a task name
                if (String.IsNullOrEmpty(name))
                {
                    throw new ApplicationException(Resources.Messages.TestExecutionService_NeedToProvideTaskName);
                }

                //check that the userid sent through, if any, to make sure it's valid
                int? ownerIdVerified = null;
                if (ownerId.HasValue)
                {
                    ownerIdVerified = (int)ownerId;
                }

                //now add the task
                TaskManager taskManager = new TaskManager();
                int newTaskId = taskManager.Insert(
                    projectId,
                    userId,
                    Task.TaskStatusEnum.NotStarted,
                    null,
                    null,
                    null,
                    testRunRow.ReleaseId,
                    ownerIdVerified,
                    null,
                    name,
                    description,
                    null,
                    null,
                    null,
                    null,
                    null);

                //next we have to associate the task to the test case and the test run
                ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();

                DateTime currentTime = DateTime.Now;

                //first associate the task to the test case
                artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, newTaskId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, userId, "", currentTime, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);

                //then associate it with the test run
                artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Task, newTaskId, Artifact.ArtifactTypeEnum.TestRun, testRunId, userId, "", currentTime, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);

				//HANDLE NOTIFICATIONS
				//Get copies of everything..
				Task newTask = taskManager.RetrieveById(newTaskId);
				Artifact notificationArt = newTask;

				//Add the ID for notifications to work (in the planning board insert case only)
				((Task)notificationArt).MarkAsAdded();
				((Task)notificationArt).TaskId = newTaskId;

				//Call notifications..
				try
				{
					new NotificationManager().SendNotificationForArtifact(notificationArt);
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Task #" + newTaskId + ".");
				}

				//finally send the new task id back to the client
				return newTaskId;

            }
            catch (DataValidationExceptionEx)
            {
                //This is handled by the calling javascript function so we just throw
                throw;
            }
            catch (ArtifactNotExistsException exception)
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

        /// <summary>Saves the start and end date, and duration of a single test run step</summary>
        /// <param name="userId">The id of the current user</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRunStepId">The id of the current test run step</param>
        /// <returns>A list of the updated execution status so that the treeview can be updated</returns>
        public void TestExecution_LogStepTiming(int projectId, int testRunsPendingId, int testRunId, int testRunStepId, DateTime startDate, DateTime endDate, int actualDuration)
        {
            const string METHOD_NAME = "TestExecution_LogStepTiming()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Create, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Get the pending test record and find the current test run and test run step
                TestRunManager testRunManager = new TestRunManager();
                TestRunsPending testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);

                //Make sure we have at least one test step
                if (!testRunsPending.TestRuns.Any(t => t.TestRunSteps.Count > 0))
                {
                    throw new DataValidationExceptionEx(new List<ValidationMessage>() { new ValidationMessage() { Message = Resources.Messages.TestExecutionService_TestRunHasNoSteps } });
                }

                TestRun testRunRow = testRunsPending.TestRuns.FirstOrDefault(t => t.TestRunId == testRunId);
                if (testRunRow == null)
                {
                    throw new DataValidationException(Resources.Messages.TestExecutionService_NoTestCaseFound);
                }
                int testRunIndex = testRunsPending.TestRuns.IndexOf(testRunRow);
                TestRunStep testRunStepRow = testRunRow.TestRunSteps.FirstOrDefault(t => t.TestRunStepId == testRunStepId);
                if (testRunStepRow == null)
                {
                    throw new DataValidationException(String.Format(Resources.Messages.Global_TestRunStepDoesNotExist, testRunStepId));
                }

                //Make sure the user matches the owner of the test run (for security reasons)
                if (testRunRow.TesterId != userId)
                {
                    throw new ArtifactAuthorizationException(Resources.Messages.TestExecutionService_NotAuthorizedToUpdateTestRun);
                }

                //Set the execution status, timing statuses, and actual result
                testRunStepRow.StartTracking();
                testRunStepRow.StartDate = GlobalFunctions.UniversalizeDate(startDate);
                testRunStepRow.EndDate = GlobalFunctions.UniversalizeDate(endDate);
                testRunStepRow.ActualDuration = actualDuration;
                //Persist the changes
                testRunManager.Update(testRunsPending, userId);

            }
            catch (DataValidationExceptionEx)
            {
                //This is handled by the calling javascript function so we just throw
                throw;
            }
            catch (ArtifactAuthorizationException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (ArtifactNotExistsException exception)
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
        /// Updates the display modes for the user
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="displayModeMain">The main display mode to use</param>
        /// <param name="displayModeSub">The subordinate display mode to use</param>
        /// <param name="alwaysShowTestRun">Whether or not to show the test run details for every test run step when in the inspector view</param>
        public void TestExecution_LogDisplaySettings(int projectId, int? displayModeMain, int? displayModeSub, bool alwaysShowTestRun, bool showCustomProperties, bool guidedTourSeen)
        {
            const string METHOD_NAME = "TestExecution_LogDisplaySettings()";

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
                bool projectChanged = false;
                bool userChanged = false;
                ProjectSettingsCollection projectSettings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_EXECUTION_GENERAL_SETTINGS);
                UserSettingsCollection userSettings = GetUserSettings(userId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);

                if (displayModeMain.HasValue)
                {
                    projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_MAIN] = displayModeMain.Value;
                    projectChanged = true;
                }
                if (displayModeSub.HasValue)
                {
                    projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_DISPLAY_MODE_SUB] = displayModeSub.Value;
                    projectChanged = true;
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN] == null || (bool)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN] != alwaysShowTestRun)
                {
                    projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_ALWAYS_SHOW_TEST_RUN] = alwaysShowTestRun;
                    projectChanged = true;
                }
                if (projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES] == null || (bool)projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES] != showCustomProperties)
                {
                    projectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_SHOW_CUSTOM_PROPERTIES] = showCustomProperties;
                    projectChanged = true;
                }
                if (userSettings[GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN] == null || (bool)userSettings[GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN] != guidedTourSeen)
                {
                    userSettings[GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_TEST_EXECUTION_SEEN] = guidedTourSeen;
                    userChanged = true;
                }

                if (projectChanged)
                {
                    projectSettings.Save();
                }
                if (userChanged)
                {
                    userSettings.Save();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Updates settings to save the current position of the user
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="testRunStepId">The current test run step id</param>
        /// <param name="testRunId">The test run id</param>
        public void TestExecution_LogCurrentPosition(int projectId, int? testRunStepId, int? testRunId)
        {
            const string METHOD_NAME = "TestExecution_LogCurrentPosition()";

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
                ProjectSettingsCollection settings = GetProjectSettings(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_TEST_EXECUTION_GENERAL_SETTINGS);
                if (testRunStepId.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_STEP_ID] = testRunStepId.Value;
                    changed = true;
                }
                if (testRunId.HasValue)
                {
                    settings[GlobalFunctions.PROJECT_SETTINGS_KEY_TE_CURRENT_TEST_RUN_ID] = testRunId.Value;
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

    # endregion 
    }
}
