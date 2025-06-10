using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Communicates with the various test run step AJAX components on the pages
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class TestRunStepService : AjaxWebServiceBase, ITestRunStepService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.TestRunStepService::";

        /// <summary>
        /// Adds a list of incidents to the specific test run step
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRunStepId">The id of the test run step</param>
        /// <param name="incidentIds">The id of the incidents</param>
        /// <param name="comment">The comment</param>
        public void TestRunStep_AddIncidentAssociation(int projectId, int testRunStepId, List<int> incidentIds, string comment)
        {
            const string METHOD_NAME = "TestRunStep_AddIncidentAssociation";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to modify incidents, since most users won't be able to modify test runs themselves
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.Modify, Artifact.ArtifactTypeEnum.Incident);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                if (incidentIds != null)
                {
                    IncidentManager incidentManager = new IncidentManager();
                    incidentManager.Incident_AssociateToTestRunStep(projectId, testRunStepId, incidentIds, userId);

                    //We add the comment to the incidents (since we have modify permissions)
                    //Don't send a notification in this case
                    if (!String.IsNullOrWhiteSpace(comment))
                    {
                        foreach (int incidentId in incidentIds)
                        {
                            incidentManager.InsertResolution(incidentId, comment, DateTime.UtcNow, userId, false);
                        }
                    }
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
        /// Returns the list of test run steps for a test run
        /// </summary>
        /// <param name="projectId">The project in question</param>
        /// <param name="testRunId">The test run we're interested in</param>
        /// <returns>The data object</returns>
        public OrderedData TestRunStep_Retrieve(int projectId, int testRunId)
        {
            const string METHOD_NAME = "TestRunStep_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized (limited edit or full view)
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, DataModel.Artifact.ArtifactTypeEnum.TestRun);
            if (authorizationState == Project.AuthorizationState.Prohibited)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Make sure the user is authorized for this item
                TestRunManager testRunManager = new TestRunManager();
                TestRunView testRun = testRunManager.RetrieveById(testRunId);
                if (authorizationState == Project.AuthorizationState.Limited && testRun.TesterId != userId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Make sure the project matches
                if (testRun.ProjectId != projectId)
                {
                    throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
                }

                //Create the array of data items
                OrderedData orderedData = new OrderedData();
                List<OrderedDataItem> dataItems = orderedData.Items;

                //Retrieve the list of test run steps
                List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(testRunId);
                
                //Populate the data items
                foreach (TestRunStepView testRunStep in testRunSteps)
                {
                    OrderedDataItem dataItem = new OrderedDataItem();
                    dataItems.Add(dataItem);
                    dataItem.PrimaryKey = testRunStep.TestRunStepId;

                    //The Position field
                    DataItemField dataItemField = new DataItemField();
                    dataItemField.FieldName = "Position";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItemField.IntValue = testRunStep.Position;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Description field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "Description";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                    dataItemField.TextValue = testRunStep.Description;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Expected Result field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "ExpectedResult";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                    dataItemField.TextValue = testRunStep.ExpectedResult;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Actual Result field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "ActualResult";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                    dataItemField.TextValue = testRunStep.ActualResult;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Sample Data field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "SampleData";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                    dataItemField.TextValue = testRunStep.SampleData;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);

                    //The Execution Status field
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "ExecutionStatusId";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    dataItemField.IntValue = testRunStep.ExecutionStatusId;
                    dataItemField.TextValue = testRunStep.ExecutionStatusName;
                    dataItemField.CssClass = GlobalFunctions.GetExecutionStatusCssClass(testRunStep.ExecutionStatusId);

                    //The test case / test step ids
                    if (testRunStep.TestCaseId.HasValue)
                    {
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.TestCase));

						dataItemField = new DataItemField();
                        dataItemField.FieldName = "TestCaseId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        dataItemField.IntValue = testRunStep.TestCaseId;
                        dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, testRunStep.TestCaseId.Value, true);
                        dataItemField.Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId, testRunStep.TestCaseId.Value));
                    }

                    if (testRunStep.TestStepId.HasValue)
                    {
						string artifactPrefix = GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum.TestStep));

						dataItemField = new DataItemField();
                        dataItemField.FieldName = "TestStepId";
                        dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Identifier;
                        dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                        dataItemField.IntValue = testRunStep.TestStepId;
                        dataItemField.TextValue = GlobalFunctions.GetTokenForArtifact(artifactPrefix, testRunStep.TestStepId.Value, true);
						dataItemField.Tooltip = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSteps, projectId, testRunStep.TestStepId.Value));
                    }

                    //Incident Count
                    dataItemField = new DataItemField();
                    dataItemField.FieldName = "IncidentCount";
                    dataItemField.FieldType = DataModel.Artifact.ArtifactFieldTypeEnum.Integer;
                    dataItem.Fields.Add(dataItemField.FieldName, dataItemField);
                    dataItemField.IntValue = testRunStep.IncidentCount;
                }

                return orderedData;
            }
            catch (ArtifactNotExistsException exception)
            {
                //Just return no data back
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                return null;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
