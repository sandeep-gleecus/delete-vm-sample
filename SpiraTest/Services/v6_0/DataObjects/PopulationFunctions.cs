using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections;


using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using System.Xml;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Helper class that populates the RemoteObjects from internal data classes
    /// </summary>
    public static class PopulationFunctions
    {
        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateDataTable(RemoteTableData remoteTableData, DataTable dataTable)
        {
            //Populate the columns
            remoteTableData.Columns = new List<RemoteTableColumn>();

            //Loop through all the columns first
            int columnPosition = 1;
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                RemoteTableColumn remoteTableColumn = new RemoteTableColumn();
                remoteTableColumn.Name = dataColumn.ColumnName;
                remoteTableColumn.Caption = dataColumn.Caption;
                remoteTableColumn.Position = columnPosition++;
                remoteTableColumn.Type = dataColumn.DataType.Name;
                remoteTableColumn.TypeNameSpace = dataColumn.DataType.Namespace;
                remoteTableData.Columns.Add(remoteTableColumn);
            }

            //Populate the rows
            remoteTableData.Rows = new List<RemoteTableRow>();

            //Now loop through all the rows
            int rowPosition = 1;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                RemoteTableRow remoteTableRow = new RemoteTableRow();
                remoteTableRow.Items = new List<RemoteTableRowItem>();
                remoteTableRow.RowNumber = rowPosition++;
                remoteTableData.Rows.Add(remoteTableRow);

                //Now add the various field values
                foreach (RemoteTableColumn remoteTableColumn in remoteTableData.Columns)
                {
                    RemoteTableRowItem item = new RemoteTableRowItem();
                    item.Column = remoteTableColumn;
                    item.Name = remoteTableColumn.Name;
                    remoteTableRow.Items.Add(item);

                    //Find the matching value or NULL
                    if (dataRow[remoteTableColumn.Name] == DBNull.Value)
                    {
                        item.Value = null;
                    }
                    else
                    {
                        item.Value = dataRow[remoteTableColumn.Name];
                    }
                }
            }
        }

        /// <summary>
        /// Populates the linked artifact API object from an XML document
        /// </summary>
        public static void PopulateDataTable(RemoteTableData remoteTableData, XmlDocument xmlDocument)
        {
            //Get the root node
            XmlNode xmlRootNode = xmlDocument.FirstChild;
            if (xmlRootNode != null)
            {
                //Populate the rows
                remoteTableData.Rows = new List<RemoteTableRow>();

                //Now loop through all the rows, which are basically the direct children
                int rowPosition = 1;
                foreach (XmlNode rowNode in xmlRootNode.ChildNodes)
                {
                    //If this is the first row, also create the column
                    if (rowPosition == 1)
                    {
                        //Populate the columns
                        remoteTableData.Columns = new List<RemoteTableColumn>();

                        //Loop through all the columns first
                        int columnPosition = 1;
                        foreach (XmlNode xmlFieldNode in rowNode.ChildNodes)
                        {
                            RemoteTableColumn remoteTableColumn = new RemoteTableColumn();
                            remoteTableColumn.Name = xmlFieldNode.Name;
                            remoteTableColumn.Position = columnPosition++;
                            remoteTableData.Columns.Add(remoteTableColumn);
                        }
                    }

                    RemoteTableRow remoteTableRow = new RemoteTableRow();
                    remoteTableRow.Items = new List<RemoteTableRowItem>();
                    remoteTableRow.RowNumber = rowPosition++;
                    remoteTableData.Rows.Add(remoteTableRow);

                    //Now add the various field values
                    foreach (XmlNode xmlFieldNode in rowNode.ChildNodes)
                    {
                        RemoteTableRowItem item = new RemoteTableRowItem();
                        item.Column = remoteTableData.Columns.FirstOrDefault(c => c.Name == xmlFieldNode.Name);
                        item.Name = xmlFieldNode.Name;
                        item.Value = xmlFieldNode.Value;
                        remoteTableRow.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateSavedReports(RemoteSavedReport remoteSavedReport, SavedReportView savedReport)
        {
            remoteSavedReport.SavedReportId = savedReport.ReportSavedId;
            remoteSavedReport.Name = savedReport.Name;
            remoteSavedReport.IsShared = savedReport.IsShared;
            remoteSavedReport.ProjectId = savedReport.ProjectId;
            remoteSavedReport.ReportFormatId = savedReport.ReportFormatId;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateMessage(RemoteMessage remoteMessage, Message message)
        {
            remoteMessage.MessageId = message.MessageId;
            remoteMessage.Body = message.Body;
            remoteMessage.CreationDate = message.CreationDate;
            remoteMessage.IsRead = message.IsRead;
            remoteMessage.LastUpdateDate = message.LastUpdateDate;
            remoteMessage.RecipientUser = new RemoteUser();
            remoteMessage.RecipientUser.UserId = message.RecipientUserId;
            remoteMessage.RecipientUser.UserName = message.RecipientName;
            remoteMessage.SenderUser = new RemoteUser();
            remoteMessage.SenderUser.UserId = message.SenderUserId;
            remoteMessage.SenderUser.UserName = message.SenderName;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateSourceCodeBranch(RemoteSourceCodeBranch remoteSourceCodeBranch, SourceCodeBranch sourceCodeBranch)
        {
            remoteSourceCodeBranch.Id = sourceCodeBranch.BranchKey;
            remoteSourceCodeBranch.Name = sourceCodeBranch.BranchKey;
            remoteSourceCodeBranch.IsDefault = sourceCodeBranch.IsDefault;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateSourceCodeRevision(RemoteSourceCodeRevision remoteSourceCodeRevision, SourceCodeCommit sourceCodeCommit)
        {
            remoteSourceCodeRevision.Id = sourceCodeCommit.Revisionkey;
            remoteSourceCodeRevision.Name = sourceCodeCommit.Name;
            remoteSourceCodeRevision.AuthorName = sourceCodeCommit.AuthorName;
            remoteSourceCodeRevision.Message = sourceCodeCommit.Name;
            remoteSourceCodeRevision.PropertiesChanged = sourceCodeCommit.PropertiesChanged;
            remoteSourceCodeRevision.UpdateDate = sourceCodeCommit.UpdateDate;
            remoteSourceCodeRevision.ContentChanged = sourceCodeCommit.ContentChanged;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateSourceCodeFile(RemoteSourceCodeFile remoteSourceCodeFile, SourceCodeFile sourceCodeFile)
        {
            remoteSourceCodeFile.Id = sourceCodeFile.FileKey;
            remoteSourceCodeFile.Name = sourceCodeFile.Name;
            remoteSourceCodeFile.Action = sourceCodeFile.Action;
            remoteSourceCodeFile.AuthorName = sourceCodeFile.AuthorName;
            remoteSourceCodeFile.LastUpdateDate = sourceCodeFile.LastUpdateDate;
            if (String.IsNullOrEmpty(sourceCodeFile.Path))
            {
                remoteSourceCodeFile.Path = sourceCodeFile.FileKey;
            }
            else
            {
                remoteSourceCodeFile.Path = sourceCodeFile.Path;
            }
            remoteSourceCodeFile.Size = sourceCodeFile.Size;

            if (!String.IsNullOrEmpty(sourceCodeFile.RevisionName))
            {
                remoteSourceCodeFile.LastRevision = new RemoteSourceCodeRevision();
                //remoteSourceCodeFile.LastRevision.Id = sourceCodeFile.RevisionKey;
                remoteSourceCodeFile.LastRevision.Name = sourceCodeFile.RevisionName;
            }
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateSourceCodeFolder(RemoteSourceCodeFolder remoteSourceCodeFolder, SourceCodeFolder sourceCodeFolder)
        {
            remoteSourceCodeFolder.Id = sourceCodeFolder.FolderKey;
            remoteSourceCodeFolder.Name = sourceCodeFolder.Name;
            remoteSourceCodeFolder.IsRoot = sourceCodeFolder.IsRoot;
            if (sourceCodeFolder.ParentFolderId.HasValue)
            {
                remoteSourceCodeFolder.ParentFolder = new RemoteSourceCodeFolder() { Id = sourceCodeFolder.ParentFolderId.ToString() };
            }
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateHistoryChangeSet(RemoteHistoryChangeSet remoteHistoryChangeSet, HistoryChangeSet historyChangeSet)
        {
            remoteHistoryChangeSet.HistoryChangeSetId = historyChangeSet.ChangeSetId;
            remoteHistoryChangeSet.ArtifactId = historyChangeSet.ArtifactId;
            remoteHistoryChangeSet.ArtifactName = historyChangeSet.ArtifactDesc;
            remoteHistoryChangeSet.ArtifactTypeId = historyChangeSet.ArtifactTypeId;
            remoteHistoryChangeSet.ChangeDate = historyChangeSet.ChangeDate;
            remoteHistoryChangeSet.ChangeTypeId = historyChangeSet.ChangeTypeId;
            remoteHistoryChangeSet.SignedId = historyChangeSet.Signed;
            remoteHistoryChangeSet.UserId = historyChangeSet.UserId;
            remoteHistoryChangeSet.Meaning = historyChangeSet.Meaning;
            if (historyChangeSet.User != null)
            {
                remoteHistoryChangeSet.UserFullName = historyChangeSet.User.FullName;
            }
            if (historyChangeSet.Type != null)
            {
                remoteHistoryChangeSet.ChangeTypeName = historyChangeSet.Type.Name;
            }
            if (historyChangeSet.ProjectId.HasValue)
            {
                remoteHistoryChangeSet.ProjectId = historyChangeSet.ProjectId.Value;
            }
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateHistoryChange(RemoteHistoryChange remoteHistoryChange, HistoryDetail historyDetail)
        {
            remoteHistoryChange.HistoryChangeId = historyDetail.ArtifactHistoryId;
            remoteHistoryChange.ChangeSetId = historyDetail.ChangeSetId;
            remoteHistoryChange.ArtifactFieldId = historyDetail.FieldId;
            remoteHistoryChange.CustomPropertyId = historyDetail.CustomPropertyId;
            remoteHistoryChange.FieldName = historyDetail.FieldName;
            remoteHistoryChange.FieldCaption = historyDetail.FieldCaption;
            remoteHistoryChange.NewValue = historyDetail.NewValue;
            remoteHistoryChange.OldValue = historyDetail.OldValue;
        }

        /// <summary>
        /// Populates the API object
        /// </summary>
        public static void PopulateHistoryChangeSet(RemoteHistoryChange remoteHistoryChange, HistoryChangeView historyChange)
        {
            remoteHistoryChange.HistoryChangeId = historyChange.ArtifactHistoryId;
            remoteHistoryChange.ChangeSetId = historyChange.ChangeSetId;
            remoteHistoryChange.ArtifactFieldId = historyChange.FieldId;
            remoteHistoryChange.CustomPropertyId = historyChange.CustomPropertyId;
            remoteHistoryChange.FieldName = historyChange.FieldName;
            remoteHistoryChange.FieldCaption = historyChange.FieldCaption;
            remoteHistoryChange.NewValue = historyChange.NewValue;
            remoteHistoryChange.OldValue = historyChange.OldValue;

            remoteHistoryChange.ChangeSet = new RemoteHistoryChangeSet();
            remoteHistoryChange.ChangeSet.HistoryChangeSetId = historyChange.ChangeSetId;
            remoteHistoryChange.ChangeSet.ArtifactId = historyChange.ArtifactId;
            remoteHistoryChange.ChangeSet.ArtifactTypeId = historyChange.ArtifactTypeId;
            remoteHistoryChange.ChangeSet.ChangeDate = historyChange.ChangeDate;
            remoteHistoryChange.ChangeSet.ChangeTypeId = historyChange.ChangeSetTypeId;
            remoteHistoryChange.ChangeSet.ChangeTypeName = historyChange.ChangeSetTypeName;
            remoteHistoryChange.ChangeSet.UserId = historyChange.UserId;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateArtifactAttachment(RemoteLinkedArtifact remoteLinkedArtifact, ArtifactAttachmentView artifactAttachment)
        {
            remoteLinkedArtifact.ArtifactId = artifactAttachment.ArtifactId;
            remoteLinkedArtifact.ArtifactTypeId = artifactAttachment.ArtifactTypeId;
            remoteLinkedArtifact.Name = artifactAttachment.ArtifactName;
            remoteLinkedArtifact.Status = artifactAttachment.ArtifactStatusName;
        }

        /// <summary>
        /// Populates the linked artifact API object
        /// </summary>
        public static void PopulateArtifactLink(RemoteLinkedArtifact remoteLinkedArtifact, ArtifactLinkView artifactLink)
        {
            remoteLinkedArtifact.ArtifactId = artifactLink.ArtifactId;
            remoteLinkedArtifact.ArtifactTypeId = artifactLink.ArtifactTypeId;
            remoteLinkedArtifact.Name = artifactLink.ArtifactName;
            remoteLinkedArtifact.Status = artifactLink.ArtifactStatusName;
        }

        /// <summary>
        /// Populates the association API object
        /// </summary>
        public static void PopulateAssociation(RemoteAssociation remoteAssociation, ArtifactLinkView artifactLink, int sourceArtifactId, int sourceArtifactTypeId)
        {
            remoteAssociation.ArtifactLinkId = artifactLink.ArtifactLinkId;
            remoteAssociation.SourceArtifactId = sourceArtifactId;
            remoteAssociation.SourceArtifactTypeId = sourceArtifactTypeId;
            remoteAssociation.DestArtifactId = artifactLink.ArtifactId;
            remoteAssociation.DestArtifactTypeId = artifactLink.ArtifactTypeId;
            remoteAssociation.CreatorId = artifactLink.CreatorId;
            remoteAssociation.Comment = artifactLink.Comment;
            remoteAssociation.CreationDate = artifactLink.CreationDate;
            remoteAssociation.DestArtifactName = artifactLink.ArtifactName;
            remoteAssociation.DestArtifactTypeName = artifactLink.ArtifactTypeName;
            remoteAssociation.CreatorName = artifactLink.CreatorName;
            remoteAssociation.ArtifactLinkTypeId = artifactLink.ArtifactLinkTypeId;
            remoteAssociation.ArtifactLinkTypeName = artifactLink.ArtifactLinkTypeName;
        }

        /// <summary>
        /// Populates the component API object
        /// </summary>
        /// <param name="component">The component entity</param>
        /// <param name="remoteComponent">The component API object</param>
        /// <remarks>We don't update the deleted flag, that is done using a delete API call instead</remarks>
        public static void PopulateComponent(RemoteComponent remoteComponent, Component component)
        {
            remoteComponent.ComponentId = component.ComponentId;
            remoteComponent.Name = component.Name;
            remoteComponent.IsActive = component.IsActive;
            remoteComponent.IsDeleted = component.IsDeleted;
            remoteComponent.ProjectId = component.ProjectId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteDataSyncSystem">The API data object</param>
        /// <param name="dataSyncSystem">The internal datarow</param>
        public static void PopulateDataSyncSystem(RemoteDataSyncSystem remoteDataSyncSystem, DataSyncSystem dataSyncSystem)
        {
            remoteDataSyncSystem.DataSyncSystemId = dataSyncSystem.DataSyncSystemId;
            remoteDataSyncSystem.DataSyncStatusId = dataSyncSystem.DataSyncStatusId;
            remoteDataSyncSystem.Name = dataSyncSystem.Name;
            remoteDataSyncSystem.DisplayName = dataSyncSystem.Caption;
            remoteDataSyncSystem.Description = dataSyncSystem.Description;
            remoteDataSyncSystem.ConnectionString = dataSyncSystem.ConnectionString;
            remoteDataSyncSystem.Login = dataSyncSystem.ExternalLogin;
            remoteDataSyncSystem.Password = dataSyncSystem.ExternalPassword;
            remoteDataSyncSystem.TimeOffsetHours = dataSyncSystem.TimeOffsetHours;
            remoteDataSyncSystem.LastSyncDate = dataSyncSystem.LastSyncDate;
            remoteDataSyncSystem.Custom01 = dataSyncSystem.Custom01;
            remoteDataSyncSystem.Custom02 = dataSyncSystem.Custom02;
            remoteDataSyncSystem.Custom03 = dataSyncSystem.Custom03;
            remoteDataSyncSystem.Custom04 = dataSyncSystem.Custom04;
            remoteDataSyncSystem.Custom05 = dataSyncSystem.Custom05;
            remoteDataSyncSystem.AutoMapUsers = (dataSyncSystem.AutoMapUsersYn == "Y");
            remoteDataSyncSystem.DataSyncStatusName = dataSyncSystem.DataSyncStatusName;
            remoteDataSyncSystem.IsActive = dataSyncSystem.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateManualTestRun(RemoteManualTestRun remoteTestRun, TestRun testRun, int projectId)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun, projectId);

            //Next any manual test run steps
            if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
            {
                remoteTestRun.TestRunSteps = new List<RemoteTestRunStep>();
                foreach (TestRunStep testRunStep in testRun.TestRunSteps)
                {
                    RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
                    remoteTestRun.TestRunSteps.Add(remoteTestRunStep);
                    //Populate the item
                    remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
                    remoteTestRunStep.TestRunId = testRunStep.TestRunId;
                    remoteTestRunStep.TestStepId = testRunStep.TestStepId;
                    remoteTestRunStep.TestCaseId = testRunStep.TestCaseId;
                    remoteTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
                    remoteTestRunStep.Position = testRunStep.Position;
                    remoteTestRunStep.Description = testRunStep.Description;
                    remoteTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
                    remoteTestRunStep.SampleData = testRunStep.SampleData;
                    remoteTestRunStep.ActualResult = testRunStep.ActualResult;
                    remoteTestRunStep.StartDate = testRunStep.StartDate;
                    remoteTestRunStep.EndDate = testRunStep.EndDate;
                    remoteTestRunStep.ActualDuration = testRunStep.ActualDuration;
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRun testRun, int projectId)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun, projectId);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.TestRunFormatId = (testRun.TestRunFormatId.HasValue) ? testRun.TestRunFormatId.Value : (int)TestRun.TestRunFormatEnum.PlainText;
            remoteTestRun.AutomationAttachmentId = null;

            //Next any automated test run steps
            if (testRun.TestRunSteps != null && testRun.TestRunSteps.Count > 0)
            {
                remoteTestRun.TestRunSteps = new List<RemoteTestRunStep>();
                foreach (TestRunStep testRunStep in testRun.TestRunSteps)
                {
                    RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
                    remoteTestRun.TestRunSteps.Add(remoteTestRunStep);
                    //Populate the item
                    remoteTestRunStep.TestRunStepId = testRunStep.TestRunStepId;
                    remoteTestRunStep.TestRunId = testRunStep.TestRunId;
                    remoteTestRunStep.TestStepId = testRunStep.TestStepId;
                    remoteTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
                    remoteTestRunStep.Position = testRunStep.Position;
                    remoteTestRunStep.Description = testRunStep.Description;
                    remoteTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
                    remoteTestRunStep.SampleData = testRunStep.SampleData;
                    remoteTestRunStep.ActualResult = testRunStep.ActualResult;
                    remoteTestRunStep.StartDate = testRunStep.StartDate;
                    remoteTestRunStep.EndDate = testRunStep.EndDate;
                    remoteTestRunStep.ActualDuration = testRunStep.ActualDuration;
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateAutomatedTestRun(RemoteAutomatedTestRun remoteTestRun, TestRunView testRun, int projectId)
        {
            //First populate the parts that are common to manual and automated test runs
            PopulateTestRun(remoteTestRun, testRun, projectId);

            //Now the automation-specific info
            remoteTestRun.RunnerName = testRun.RunnerName;
            remoteTestRun.RunnerTestName = testRun.RunnerTestName;
            remoteTestRun.RunnerAssertCount = testRun.RunnerAssertCount;
            remoteTestRun.RunnerMessage = testRun.RunnerMessage;
            remoteTestRun.RunnerStackTrace = testRun.RunnerStackTrace;
            remoteTestRun.AutomationHostId = testRun.AutomationHostId;
            remoteTestRun.AutomationEngineId = testRun.AutomationEngineId;
            remoteTestRun.TestRunFormatId = (testRun.TestRunFormatId.HasValue) ? testRun.TestRunFormatId.Value : (int)TestRun.TestRunFormatEnum.PlainText;
            remoteTestRun.AutomationAttachmentId = null;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the currernt project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRun testRun, int projectId)
        {
            //Artifact Fields
            remoteTestRun.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestRun;
            remoteTestRun.IsAttachments = testRun.IsAttachments;

            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.ProjectId = projectId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = testRun.StartDate;
            remoteTestRun.EndDate = testRun.EndDate;
            remoteTestRun.BuildId = testRun.BuildId;
            remoteTestRun.EstimatedDuration = testRun.EstimatedDuration;
            remoteTestRun.ActualDuration = testRun.ActualDuration;
            remoteTestRun.TestConfigurationId = testRun.TestConfigurationId;

            //Concurrency management
            remoteTestRun.ConcurrencyDate = testRun.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestRun">The API data object</param>
        /// <param name="projectId">The id of the currernt project</param>
        /// <param name="testRun">The internal datarow containing a test run and its test run steps</param>
        public static void PopulateTestRun(RemoteTestRun remoteTestRun, TestRunView testRun, int projectId)
        {
            //Artifact Fields
            remoteTestRun.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestRun;
            remoteTestRun.IsAttachments = testRun.IsAttachments;

            //Populate the parts that are common to manual and automated test runs
            remoteTestRun.TestRunId = testRun.TestRunId;
            remoteTestRun.ProjectId = projectId;
            remoteTestRun.Name = testRun.Name;
            remoteTestRun.TestCaseId = testRun.TestCaseId;
            remoteTestRun.TestRunTypeId = testRun.TestRunTypeId;
            remoteTestRun.TesterId = testRun.TesterId;
            remoteTestRun.ExecutionStatusId = testRun.ExecutionStatusId;
            remoteTestRun.ReleaseId = testRun.ReleaseId;
            remoteTestRun.TestSetId = testRun.TestSetId;
            remoteTestRun.TestSetTestCaseId = testRun.TestSetTestCaseId;
            remoteTestRun.StartDate = testRun.StartDate;
            remoteTestRun.EndDate = testRun.EndDate;
            remoteTestRun.BuildId = testRun.BuildId;
            remoteTestRun.EstimatedDuration = testRun.EstimatedDuration;
            remoteTestRun.ActualDuration = testRun.ActualDuration;
            remoteTestRun.TestConfigurationId = testRun.TEST_CONFIGURATION_ID;

            //Concurrency management
            remoteTestRun.ConcurrencyDate = testRun.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteAutomationEngine">The API data object</param>
        /// <param name="automationEngine">The internal datarow</param>
        public static void PopulateAutomationEngine(RemoteAutomationEngine remoteAutomationEngine, AutomationEngine automationEngine)
        {
            remoteAutomationEngine.AutomationEngineId = automationEngine.AutomationEngineId;
            remoteAutomationEngine.Name = automationEngine.Name;
            remoteAutomationEngine.Description = automationEngine.Description;
            remoteAutomationEngine.Token = automationEngine.Token;
            remoteAutomationEngine.Active = automationEngine.IsActive;
        }

        /// <summary>
        /// Populates an API object from the internal entity
        /// </summary>
        /// <param name="remoteBuild">The API data object</param>
        /// <param name="build">The internal entity</param>
        public static void PopulateBuild(RemoteBuild remoteBuild, Build build)
        {
            remoteBuild.BuildId = build.BuildId;
            remoteBuild.BuildStatusId = build.BuildStatusId;
            remoteBuild.ProjectId = build.ProjectId;
            remoteBuild.ReleaseId = build.ReleaseId;
            remoteBuild.Name = build.Name;
            remoteBuild.Description = build.Description;
            remoteBuild.LastUpdateDate = build.LastUpdateDate;
            remoteBuild.CreationDate = build.CreationDate;
            remoteBuild.BuildStatusName = build.BuildStatusName;
        }

        /// <summary>
        /// Converts the API sort object into an internal sort expression
        /// </summary>
        /// <param name="remoteSort">The API sort object</param>
        /// <returns>The sort expression</returns>
        public static string PopulateSort(RemoteSort remoteSort)
        {
            return remoteSort.PropertyName + " " + ((remoteSort.SortAscending) ? "ASC" : "DESC");
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTask">The API data object</param>
        /// <param name="task">The internal entity</param>
        public static void PopulateTask(RemoteTask remoteTask, TaskView task)
        {
            //Artifact Fields
            remoteTask.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Task;
            remoteTask.IsAttachments = task.IsAttachments;

            remoteTask.TaskId = task.TaskId;
            remoteTask.ProjectId = task.ProjectId;
            remoteTask.TaskStatusId = task.TaskStatusId;
            remoteTask.TaskTypeId = task.TaskTypeId;
            remoteTask.TaskFolderId = task.TaskFolderId;
            remoteTask.RequirementId = task.RequirementId;
            remoteTask.ReleaseId = task.ReleaseId;
            remoteTask.CreatorId = task.CreatorId;
            remoteTask.OwnerId = task.OwnerId;
            remoteTask.TaskPriorityId = task.TaskPriorityId;
            remoteTask.Name = task.Name;
            remoteTask.Description = task.Description;
            remoteTask.CreationDate = task.CreationDate;
            remoteTask.LastUpdateDate = task.LastUpdateDate;
            remoteTask.StartDate = task.StartDate;
            remoteTask.EndDate = task.EndDate;
            remoteTask.CompletionPercent = task.CompletionPercent;
            remoteTask.EstimatedEffort = task.EstimatedEffort;
            remoteTask.ActualEffort = task.ActualEffort;
            remoteTask.RemainingEffort = task.RemainingEffort;
            remoteTask.ProjectedEffort = task.ProjectedEffort;
            remoteTask.TaskStatusName = task.TaskStatusName;
            remoteTask.TaskTypeName = task.TaskTypeName;
            remoteTask.OwnerName = task.OwnerName;
            remoteTask.TaskPriorityName = task.TaskPriorityName;
            remoteTask.ProjectName = task.ProjectName;
            remoteTask.ReleaseVersionNumber = task.ReleaseVersionNumber;
            remoteTask.RequirementName = task.RequirementName;
            remoteTask.ComponentId = task.ComponentId;
            remoteTask.RiskId = task.RiskId;

            //Concurrency Management
            remoteTask.ConcurrencyDate = task.ConcurrencyDate;
        }

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		/// <param name="remoteRisk">The API data object</param>
		/// <param name="risk">The internal entity</param>
		public static void PopulateRisk(RemoteRisk remoteRisk, RiskView risk)
		{
			//Artifact Fields
			remoteRisk.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Risk;
			remoteRisk.IsAttachments = risk.IsAttachments;

			//Risk Fields
			remoteRisk.RiskId = risk.RiskId;
			remoteRisk.ClosedDate = risk.ClosedDate;
			remoteRisk.ComponentId = risk.ComponentId;
			remoteRisk.ComponentName = risk.ComponentName;
			remoteRisk.CreationDate = risk.CreationDate;
			remoteRisk.CreatorId = risk.CreatorId;
			remoteRisk.CreatorName = risk.CreatorName;
			remoteRisk.Description = risk.Description;
			remoteRisk.GoalId = risk.GoalId;
			remoteRisk.LastUpdateDate = risk.LastUpdateDate;
			remoteRisk.Name = risk.Name;
			remoteRisk.OwnerId = risk.OwnerId;
			remoteRisk.OwnerName = risk.OwnerName;
			remoteRisk.ProjectGroupId = risk.ProjectGroupId;
			remoteRisk.ProjectId = risk.ProjectId;
			remoteRisk.ReleaseId = risk.ReleaseId;
			remoteRisk.ReleaseName = risk.ReleaseName;
			remoteRisk.ReleaseVersionNumber = risk.ReleaseVersionNumber;
			remoteRisk.ReviewDate = risk.ReviewDate;
			remoteRisk.RiskDetectabilityId = risk.RiskDetectabilityId;
			remoteRisk.RiskExposure = risk.RiskExposure;
			remoteRisk.RiskImpactId = risk.RiskImpactId;
			remoteRisk.RiskImpactName = risk.RiskImpactName;
			remoteRisk.RiskProbabilityId = risk.RiskProbabilityId;
			remoteRisk.RiskProbabilityName = risk.RiskProbabilityName;
			remoteRisk.RiskStatusId = risk.RiskStatusId;
			remoteRisk.RiskStatusName = risk.RiskStatusName;
			remoteRisk.RiskTypeId = risk.RiskTypeId;
			remoteRisk.RiskTypeName = risk.RiskTypeName;
			remoteRisk.IsDeleted = risk.IsDeleted;

			//Concurrency Management
			remoteRisk.ConcurrencyDate = risk.ConcurrencyDate;
		}

		/// <summary>
		/// Populates an API object from the internal entity
		/// </summary>
		public static void PopulateFilters(RemoteFilter remoteFilter, DataModel.SavedFilterEntry savedFilterEntry, SavedFilterManager manager)
        {
            remoteFilter.PropertyName = savedFilterEntry.EntryKey;
            object value = manager.DeSerializeValue(savedFilterEntry.EntryValue, savedFilterEntry.EntryTypeCode);
            if (value != null)
            {
                if (value is Common.DateRange)
                {
                    remoteFilter.DateRangeValue = new DateRange((Common.DateRange)value);
                }
                else if (value is Common.MultiValueFilter)
                {
                    remoteFilter.MultiValue = new MultiValueFilter((Common.MultiValueFilter)value);
                }
                else if (value is Int32)
                {
                    remoteFilter.IntValue = (Int32)value;
                }
                else
                {
                    remoteFilter.StringValue = value.ToString();
                }
            }
        }

        /// <summary>
        /// Converts the API filter object into the kinds that can be consumed internally
        /// </summary>
        /// <param name="remoteFilters">The list of API filters</param>
        /// <returns>The populated Hashtable of internal filters</returns>
        public static Hashtable PopulateFilters(List<RemoteFilter> remoteFilters)
        {
            //Handle the null case
            if (remoteFilters == null)
            {
                return null;
            }
            Hashtable filters = new Hashtable();
            foreach (RemoteFilter remoteFilter in remoteFilters)
            {
                if (!String.IsNullOrEmpty(remoteFilter.PropertyName))
                {
                    //See what type we have and populate accordingly
                    if (remoteFilter.IntValue.HasValue)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.IntValue.Value);
                    }
                    else if (!String.IsNullOrEmpty(remoteFilter.StringValue))
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.StringValue);
                    }
                    else if (remoteFilter.MultiValue != null)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.MultiValue.Internal);
                    }
                    else if (remoteFilter.DateRangeValue != null)
                    {
                        filters.Add(remoteFilter.PropertyName, remoteFilter.DateRangeValue.Internal);
                    }

                }
            }
            return filters;
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal artifact custom property entity
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifactCustomProperty">The internal custom property entity</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, ArtifactCustomProperty artifactCustomProperty)
        {
            //Make sure we have a custom propery record and definitions
            if (artifactCustomProperty != null && artifactCustomProperty.CustomPropertyDefinitions != null && remoteArtifact.ProjectId > 0)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in artifactCustomProperty.CustomPropertyDefinitions)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId > 0 && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Now we need to populate the appropriate value
                        switch (customPropertyType)
                        {
                            case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                remoteArtifactCustomProperty.BooleanValue = (bool?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Date:
                                remoteArtifactCustomProperty.DateTimeValue = (DateTime?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                remoteArtifactCustomProperty.DecimalValue = (decimal?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Integer:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.List:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                remoteArtifactCustomProperty.IntegerListValue = (List<int>)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.Text:
                                remoteArtifactCustomProperty.StringValue = (string)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;

                            case CustomProperty.CustomPropertyTypeEnum.User:
                                remoteArtifactCustomProperty.IntegerValue = (int?)artifactCustomProperty.CustomProperty(propertyNumber);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal datarow
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="dataRow">The internal datarow</param>
        /// <param name="customProperties">The custom property definitions for this artifact type</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, DataRow dataRow, List<CustomProperty> customProperties)
        {
            //Make sure we have a custom propery definitions
            if (customProperties != null && remoteArtifact.ProjectId > 0)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in customProperties)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId > 0 && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Make sure the datarow has the custom property column defined
                        //Now we need to populate the appropriate value
                        if (dataRow.Table.Columns[customProperty.CustomPropertyFieldName] != null)
                        {
                            string serializedValue;
                            if (dataRow[customProperty.CustomPropertyFieldName] == DBNull.Value)
                            {
                                serializedValue = null;
                            }
                            else
                            {
                                serializedValue = (string)dataRow[customProperty.CustomPropertyFieldName];
                            }

                            switch (customPropertyType)
                            {
                                case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                    remoteArtifactCustomProperty.BooleanValue = serializedValue.FromDatabaseSerialization_Boolean();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Date:
                                    remoteArtifactCustomProperty.DateTimeValue = serializedValue.FromDatabaseSerialization_DateTime();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                    remoteArtifactCustomProperty.DecimalValue = serializedValue.FromDatabaseSerialization_Decimal();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Integer:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.List:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                    remoteArtifactCustomProperty.IntegerListValue = serializedValue.FromDatabaseSerialization_List_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Text:
                                    remoteArtifactCustomProperty.StringValue = serializedValue.FromDatabaseSerialization_String();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.User:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object's custom properties from the internal artifact entity
        /// </summary>
        /// <param name="remoteArtifact">The API data object</param>
        /// <param name="artifact">The internal artifact</param>
        /// <param name="customProperties">The custom property definitions for this artifact type</param>
        /// <remarks>The data object needs to inherit from RemoteArtifact</remarks>
        public static void PopulateCustomProperties(RemoteArtifact remoteArtifact, Artifact artifact, List<CustomProperty> customProperties)
        {
            //Make sure we have a custom propery definitions
            if (customProperties != null && remoteArtifact.ProjectId > 0)
            {
                //See if we have an existing API custom property collection, otherwise create one
                if (remoteArtifact.CustomProperties == null)
                {
                    remoteArtifact.CustomProperties = new List<RemoteArtifactCustomProperty>();
                }

                //Loop through all the defined custom properties
                foreach (CustomProperty customProperty in customProperties)
                {
                    //Get the template associated with the project
                    int projectTemplateId = new TemplateManager().RetrieveForProject(remoteArtifact.ProjectId).ProjectTemplateId;

                    //Make sure the projects match (we will have multiple project definitions in some of the retrieves)
                    if (remoteArtifact.ProjectId > 0 && customProperty.ProjectTemplateId == projectTemplateId)
                    {
                        int propertyNumber = customProperty.PropertyNumber;
                        CustomProperty.CustomPropertyTypeEnum customPropertyType = (CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId;

                        //See if we have an existing API custom property entry, otherwise create one
                        RemoteArtifactCustomProperty remoteArtifactCustomProperty = remoteArtifact.CustomProperties.FirstOrDefault(p => p.PropertyNumber == propertyNumber);
                        if (remoteArtifactCustomProperty == null)
                        {
                            remoteArtifactCustomProperty = new RemoteArtifactCustomProperty();
                            remoteArtifactCustomProperty.PropertyNumber = propertyNumber;
                            remoteArtifactCustomProperty.Definition = new RemoteCustomProperty(customProperty);
                            remoteArtifact.CustomProperties.Add(remoteArtifactCustomProperty);
                        }

                        //Make sure the datarow has the custom property column defined
                        //Now we need to populate the appropriate value
                        if (artifact.ContainsProperty(customProperty.CustomPropertyFieldName))
                        {
                            string serializedValue;
                            if (artifact[customProperty.CustomPropertyFieldName] == null)
                            {
                                serializedValue = null;
                            }
                            else
                            {
                                serializedValue = (string)artifact[customProperty.CustomPropertyFieldName];
                            }

                            switch (customPropertyType)
                            {
                                case CustomProperty.CustomPropertyTypeEnum.Boolean:
                                    remoteArtifactCustomProperty.BooleanValue = serializedValue.FromDatabaseSerialization_Boolean();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Date:
                                    remoteArtifactCustomProperty.DateTimeValue = serializedValue.FromDatabaseSerialization_DateTime();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Decimal:
                                    remoteArtifactCustomProperty.DecimalValue = serializedValue.FromDatabaseSerialization_Decimal();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Integer:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.List:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.MultiList:
                                    remoteArtifactCustomProperty.IntegerListValue = serializedValue.FromDatabaseSerialization_List_Int32();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.Text:
                                    remoteArtifactCustomProperty.StringValue = serializedValue.FromDatabaseSerialization_String();
                                    break;

                                case CustomProperty.CustomPropertyTypeEnum.User:
                                    remoteArtifactCustomProperty.IntegerValue = serializedValue.FromDatabaseSerialization_Int32();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, WorkflowTransition transition)
        {
            remoteTransition.ExecuteByCreator = transition.IsExecuteByDetector;
            remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
            remoteTransition.StatusId_Input = transition.InputIncidentStatusId;
            remoteTransition.StatusName_Input = transition.InputStatus.Name;
            remoteTransition.StatusId_Output = transition.OutputIncidentStatusId;
            remoteTransition.StatusName_Output = transition.OutputStatus.Name;
            remoteTransition.Name = transition.Name;
            remoteTransition.TransitionId = transition.WorkflowTransitionId;
            remoteTransition.WorkflowId = transition.WorkflowId;
            remoteTransition.RequireSignature = transition.IsSignatureRequired;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, ReleaseWorkflowTransition transition)
        {
            remoteTransition.ExecuteByCreator = transition.IsExecuteByCreator;
            remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
            remoteTransition.StatusId_Input = transition.InputReleaseStatusId;
            remoteTransition.StatusName_Input = transition.InputReleaseStatus.Name;
            remoteTransition.StatusId_Output = transition.OutputReleaseStatusId;
            remoteTransition.StatusName_Output = transition.OutputReleaseStatus.Name;
            remoteTransition.Name = transition.Name;
            remoteTransition.TransitionId = transition.WorkflowTransitionId;
            remoteTransition.WorkflowId = transition.ReleaseWorkflowId;
            remoteTransition.RequireSignature = transition.IsSignatureRequired;
        }

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, RiskWorkflowTransition transition)
		{
			remoteTransition.ExecuteByCreator = transition.IsExecuteByCreator;
			remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
			remoteTransition.StatusId_Input = transition.InputRiskStatusId;
			remoteTransition.StatusName_Input = transition.InputStatus.Name;
			remoteTransition.StatusId_Output = transition.OutputRiskStatusId;
			remoteTransition.StatusName_Output = transition.OutputStatus.Name;
			remoteTransition.Name = transition.Name;
			remoteTransition.TransitionId = transition.WorkflowTransitionId;
			remoteTransition.WorkflowId = transition.RiskWorkflowId;
			remoteTransition.RequireSignature = transition.IsSignature;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, TaskWorkflowTransition transition)
        {
            remoteTransition.ExecuteByCreator = transition.IsExecuteByCreator;
            remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
            remoteTransition.StatusId_Input = transition.InputTaskStatusId;
            remoteTransition.StatusName_Input = transition.InputTaskStatus.Name;
            remoteTransition.StatusId_Output = transition.OutputTaskStatusId;
            remoteTransition.StatusName_Output = transition.OutputTaskStatus.Name;
            remoteTransition.Name = transition.Name;
            remoteTransition.TransitionId = transition.WorkflowTransitionId;
            remoteTransition.WorkflowId = transition.TaskWorkflowId;
            remoteTransition.RequireSignature = transition.IsSignatureRequired;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, TestCaseWorkflowTransition transition)
        {
            remoteTransition.ExecuteByCreator = transition.IsExecuteByCreator;
            remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
            remoteTransition.StatusId_Input = transition.InputTestCaseStatusId;
            remoteTransition.StatusName_Input = transition.InputTestCaseStatus.Name;
            remoteTransition.StatusId_Output = transition.OutputTestCaseStatusId;
            remoteTransition.StatusName_Output = transition.OutputTestCaseStatus.Name;
            remoteTransition.Name = transition.Name;
            remoteTransition.TransitionId = transition.WorkflowTransitionId;
            remoteTransition.WorkflowId = transition.TestCaseWorkflowId;
            remoteTransition.RequireSignature = transition.IsSignatureRequired;
        }
        
        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowTransition(RemoteWorkflowTransition remoteTransition, RequirementWorkflowTransition transition)
        {
            remoteTransition.ExecuteByCreator = transition.IsExecuteByCreator;
            remoteTransition.ExecuteByOwner = transition.IsExecuteByOwner;
            remoteTransition.StatusId_Input = transition.InputRequirementStatusId;
            remoteTransition.StatusName_Input = transition.InputRequirementStatus.Name;
            remoteTransition.StatusId_Output = transition.OutputRequirementStatusId;
            remoteTransition.StatusName_Output = transition.OutputRequirementStatus.Name;
            remoteTransition.Name = transition.Name;
            remoteTransition.TransitionId = transition.WorkflowTransitionId;
            if (transition.RequirementWorkflowId.HasValue)
            {
                remoteTransition.WorkflowId = transition.RequirementWorkflowId.Value;
            }
            remoteTransition.RequireSignature = transition.IsSignatureRequired;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, WorkflowField workflowField)
        {
            remoteWorkflowField.FieldCaption = workflowField.Field.Caption;
            remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
            remoteWorkflowField.FieldName = workflowField.Field.Name;
            remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
        }

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, RiskWorkflowField workflowField)
		{
			remoteWorkflowField.FieldCaption = workflowField.ArtifactField.Caption;
			remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
			remoteWorkflowField.FieldName = workflowField.ArtifactField.Name;
			remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, ReleaseWorkflowField workflowField)
        {
            remoteWorkflowField.FieldCaption = workflowField.ArtifactField.Caption;
            remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
            remoteWorkflowField.FieldName = workflowField.ArtifactField.Name;
            remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, TaskWorkflowField workflowField)
        {
            remoteWorkflowField.FieldCaption = workflowField.ArtifactField.Caption;
            remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
            remoteWorkflowField.FieldName = workflowField.ArtifactField.Name;
            remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, TestCaseWorkflowField workflowField)
        {
            remoteWorkflowField.FieldCaption = workflowField.ArtifactField.Caption;
            remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
            remoteWorkflowField.FieldName = workflowField.ArtifactField.Name;
            remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowField(RemoteWorkflowField remoteWorkflowField, RequirementWorkflowField workflowField)
        {
            remoteWorkflowField.FieldCaption = workflowField.ArtifactField.Caption;
            remoteWorkflowField.FieldId = workflowField.ArtifactFieldId;
            remoteWorkflowField.FieldName = workflowField.ArtifactField.Name;
            remoteWorkflowField.FieldStateId = workflowField.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, WorkflowCustomProperty workflowCustomProperty)
        {
            remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, RiskWorkflowCustomProperty workflowCustomProperty)
		{
			remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
			remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
			remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
			remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, ReleaseWorkflowCustomProperty workflowCustomProperty)
        {
            remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, TaskWorkflowCustomProperty workflowCustomProperty)
        {
            remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, TestCaseWorkflowCustomProperty workflowCustomProperty)
        {
            remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateWorkflowCustomProperty(RemoteWorkflowCustomProperty remoteWorkflowCustomProperty, RequirementWorkflowCustomProperty workflowCustomProperty)
        {
            remoteWorkflowCustomProperty.CustomPropertyId = workflowCustomProperty.CustomPropertyId;
            remoteWorkflowCustomProperty.FieldName = workflowCustomProperty.CustomProperty.CustomPropertyFieldName;
            remoteWorkflowCustomProperty.FieldCaption = workflowCustomProperty.CustomProperty.Name;
            remoteWorkflowCustomProperty.FieldStateId = workflowCustomProperty.WorkflowFieldStateId;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteIncident">The API data object</param>
        /// <param name="incident">The internal entity</param>
        public static void PopulateIncident(RemoteIncident remoteIncident, IncidentView incident, List<int> testRunStepIds = null)
        {
            //Artifact Fields
            remoteIncident.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Incident;
            remoteIncident.IsAttachments = incident.IsAttachments;

            remoteIncident.IncidentId = incident.IncidentId;
            remoteIncident.ProjectId = incident.ProjectId;
            remoteIncident.PriorityId = incident.PriorityId;
            remoteIncident.SeverityId = incident.SeverityId;
            remoteIncident.IncidentStatusId = incident.IncidentStatusId;
            remoteIncident.IncidentTypeId = incident.IncidentTypeId;
            remoteIncident.OpenerId = incident.OpenerId;
            remoteIncident.OwnerId = incident.OwnerId;
            remoteIncident.TestRunStepIds = testRunStepIds;
            remoteIncident.DetectedReleaseId = incident.DetectedReleaseId;
            remoteIncident.ResolvedReleaseId = incident.ResolvedReleaseId;
            remoteIncident.VerifiedReleaseId = incident.VerifiedReleaseId;
            remoteIncident.Name = incident.Name;
            remoteIncident.Description = incident.Description;
            remoteIncident.CreationDate = incident.CreationDate;
            remoteIncident.StartDate = incident.StartDate;
            remoteIncident.ClosedDate = incident.ClosedDate;
            remoteIncident.CompletionPercent = incident.CompletionPercent;
            remoteIncident.EstimatedEffort = incident.EstimatedEffort;
            remoteIncident.ActualEffort = incident.ActualEffort;
            remoteIncident.ProjectedEffort = incident.ProjectedEffort;
            remoteIncident.RemainingEffort = incident.RemainingEffort;
            remoteIncident.LastUpdateDate = incident.LastUpdateDate;
            remoteIncident.PriorityName = incident.PriorityName;
            remoteIncident.SeverityName = incident.SeverityName;
            remoteIncident.IncidentStatusName = incident.IncidentStatusName;
            remoteIncident.IncidentTypeName = incident.IncidentTypeName;
            remoteIncident.OpenerName = incident.OpenerName;
            remoteIncident.OwnerName = incident.OwnerName;
            remoteIncident.ProjectName = incident.ProjectName;
            remoteIncident.DetectedReleaseVersionNumber = incident.DetectedReleaseVersionNumber;
            remoteIncident.ResolvedReleaseVersionNumber = incident.ResolvedReleaseVersionNumber;
            remoteIncident.VerifiedReleaseVersionNumber = incident.VerifiedReleaseVersionNumber;
            remoteIncident.IncidentStatusOpenStatus = incident.IncidentStatusIsOpenStatus;
            remoteIncident.FixedBuildId = incident.BuildId;
            remoteIncident.FixedBuildName = incident.BuildName;
            remoteIncident.ComponentIds = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
            remoteIncident.EndDate = incident.EndDate;
            remoteIncident.DetectedBuildId = incident.DetectedBuildId;

            //Concurrency Management
            remoteIncident.ConcurrencyDate = incident.ConcurrencyDate;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocument">The API data object</param>
        /// <param name="projectAttachmentView">The internal datarow</param>
        public static void PopulateDocument(RemoteDocument remoteDocument, ProjectAttachmentView projectAttachmentView)
        {
            //Artifact Fields
            remoteDocument.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document;

            remoteDocument.AttachmentId = projectAttachmentView.AttachmentId;
			remoteDocument.ProjectId = projectAttachmentView.ProjectId;
			remoteDocument.AttachmentTypeId = projectAttachmentView.AttachmentTypeId;
            remoteDocument.AuthorId = projectAttachmentView.AuthorId;
            remoteDocument.EditorId = projectAttachmentView.EditorId;
            remoteDocument.FilenameOrUrl = projectAttachmentView.Filename;
            remoteDocument.Description = projectAttachmentView.Description;
            remoteDocument.UploadDate = projectAttachmentView.UploadDate;
            remoteDocument.EditedDate = projectAttachmentView.EditedDate;
            remoteDocument.Size = projectAttachmentView.Size;
            remoteDocument.CurrentVersion = projectAttachmentView.CurrentVersion;
            remoteDocument.Tags = projectAttachmentView.Tags;
            remoteDocument.AttachmentTypeName = projectAttachmentView.AttachmentTypeName;
            remoteDocument.ProjectAttachmentFolderId = projectAttachmentView.ProjectAttachmentFolderId;
            remoteDocument.DocumentTypeId = projectAttachmentView.DocumentTypeId;
            remoteDocument.DocumentTypeName = projectAttachmentView.DocumentTypeName;
            remoteDocument.AuthorName = projectAttachmentView.AuthorName;
            remoteDocument.EditorName = projectAttachmentView.EditorName;
            remoteDocument.DocumentStatusId = projectAttachmentView.DocumentStatusId;
            remoteDocument.DocumentStatusName = projectAttachmentView.DocumentStatusName;

            //Concurrency Management
            remoteDocument.ConcurrencyDate = projectAttachmentView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentType">The API data object</param>
        /// <param name="documentType">The internal data row</param>
        public static void PopulateDocumentType(RemoteDocumentType remoteDocumentType, DocumentType documentType)
        {
            remoteDocumentType.DocumentTypeId = documentType.DocumentTypeId;
            remoteDocumentType.ProjectTemplateId = documentType.ProjectTemplateId;
            remoteDocumentType.Name = documentType.Name;
            remoteDocumentType.Description = documentType.Description;
            remoteDocumentType.Active = documentType.IsActive;
            remoteDocumentType.Default = documentType.IsDefault;
        }

        /// <summary>
        /// Populates a project user API object from the internal datarow
        /// </summary>
        /// <param name="remoteProjectUser">The API data object</param>
        /// <param name="projectUserRow">The internal datarow</param>
        public static void PopulateProjectUser(RemoteProjectUser remoteProjectUser, ProjectUser projectUserRow)
        {
            remoteProjectUser.UserId = projectUserRow.UserId;
            remoteProjectUser.ProjectId = projectUserRow.ProjectId;
            remoteProjectUser.ProjectRoleId = projectUserRow.ProjectRoleId;
            remoteProjectUser.ProjectRoleName = projectUserRow.ProjectRoleName;
            remoteProjectUser.UserName = projectUserRow.UserName;
            if (projectUserRow.User != null)
            {
                remoteProjectUser.EmailAddress = projectUserRow.User.EmailAddress;
                remoteProjectUser.Active = projectUserRow.User.IsActive;
                remoteProjectUser.LdapDn = projectUserRow.User.LdapDn;
                if (projectUserRow.User.Profile != null)
                {
                    remoteProjectUser.FirstName = projectUserRow.User.Profile.FirstName;
                    remoteProjectUser.MiddleInitial = projectUserRow.User.Profile.MiddleInitial;
                    remoteProjectUser.LastName = projectUserRow.User.Profile.LastName;
                    remoteProjectUser.Admin = projectUserRow.User.Profile.IsAdmin;
                    remoteProjectUser.FullName = projectUserRow.User.Profile.FullName;
                }
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateReleaseType(RemoteReleaseType remoteReleaseType, ReleaseType releaseType)
        {
            remoteReleaseType.ReleaseTypeId = releaseType.ReleaseTypeId;
            remoteReleaseType.Name = releaseType.Name;
            remoteReleaseType.Position = releaseType.Position;
            remoteReleaseType.Active = releaseType.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateReleaseStatus(RemoteReleaseStatus remoteReleaseStatus, ReleaseStatus releaseStatus)
        {
            remoteReleaseStatus.ReleaseStatusId = releaseStatus.ReleaseStatusId;
            remoteReleaseStatus.Name = releaseStatus.Name;
            remoteReleaseStatus.Position = releaseStatus.Position;
            remoteReleaseStatus.Active = releaseStatus.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTaskType(RemoteTaskType remoteTaskType, TaskType taskType)
        {
            remoteTaskType.TaskTypeId = taskType.TaskTypeId;
            remoteTaskType.Name = taskType.Name;
            remoteTaskType.Position = taskType.Position;
            remoteTaskType.IsActive = taskType.IsActive;
            remoteTaskType.IsDefault = taskType.IsDefault;
            remoteTaskType.IsCodeReview = taskType.IsCodeReview;
            remoteTaskType.IsPullRequest = taskType.IsPullRequest;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTaskPriority(RemoteTaskPriority remoteTaskPriority, TaskPriority taskPriority)
        {
            remoteTaskPriority.PriorityId = taskPriority.TaskPriorityId;
            remoteTaskPriority.Name = taskPriority.Name;
            remoteTaskPriority.Active = taskPriority.IsActive;
            remoteTaskPriority.Color = taskPriority.Color;
            remoteTaskPriority.Score = taskPriority.Score;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTestCasePriority(RemoteTestCasePriority remoteTestCasePriority, TestCasePriority testCasePriority)
        {
            remoteTestCasePriority.PriorityId = testCasePriority.TestCasePriorityId;
            remoteTestCasePriority.Name = testCasePriority.Name;
            remoteTestCasePriority.Active = testCasePriority.IsActive;
            remoteTestCasePriority.Color = testCasePriority.Color;
            remoteTestCasePriority.Score = testCasePriority.Score;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateRequirementImportance(RemoteRequirementImportance remoteRequirementImportance, Importance importance)
        {
            remoteRequirementImportance.ImportanceId = importance.ImportanceId;
            remoteRequirementImportance.Name = importance.Name;
            remoteRequirementImportance.Active = importance.IsActive;
            remoteRequirementImportance.Color = importance.Color;
            remoteRequirementImportance.Score = importance.Score;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTaskStatus(RemoteTaskStatus remoteTaskStatus, TaskStatus taskStatus)
        {
            remoteTaskStatus.TaskStatusId = taskStatus.TaskStatusId;
            remoteTaskStatus.Name = taskStatus.Name;
            remoteTaskStatus.Position = taskStatus.Position;
            remoteTaskStatus.Active = taskStatus.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTestCaseType(RemoteTestCaseType remoteTestCaseType, TestCaseType testCaseType)
        {
            remoteTestCaseType.TestCaseTypeId = testCaseType.TestCaseTypeId;
            remoteTestCaseType.Name = testCaseType.Name;
            remoteTestCaseType.Position = testCaseType.Position;
            remoteTestCaseType.IsActive = testCaseType.IsActive;
            remoteTestCaseType.IsDefault = testCaseType.IsDefault;
            remoteTestCaseType.IsExploratory = testCaseType.IsExploratory;
            remoteTestCaseType.IsBdd = testCaseType.IsBdd;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateTestCaseStatus(RemoteTestCaseStatus remoteTestCaseStatus, TestCaseStatus testCaseStatus)
        {
            remoteTestCaseStatus.TestCaseStatusId = testCaseStatus.TestCaseStatusId;
            remoteTestCaseStatus.Name = testCaseStatus.Name;
            remoteTestCaseStatus.Position = testCaseStatus.Position;
            remoteTestCaseStatus.Active = testCaseStatus.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateRequirementType(RemoteRequirementType remoteRequirementType, RequirementType requirementType)
        {
            remoteRequirementType.RequirementTypeId = requirementType.RequirementTypeId;
            remoteRequirementType.Name = requirementType.Name;
            remoteRequirementType.IsActive = requirementType.IsActive;
            remoteRequirementType.IsDefault = requirementType.IsDefault;
            remoteRequirementType.IsSteps = requirementType.IsSteps;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateRequirementStatus(RemoteRequirementStatus remoteRequirementStatus, RequirementStatus requirementStatus)
        {
            remoteRequirementStatus.RequirementStatusId = requirementStatus.RequirementStatusId;
            remoteRequirementStatus.Name = requirementStatus.Name;
            remoteRequirementStatus.Position = requirementStatus.Position;
            remoteRequirementStatus.Active = requirementStatus.IsActive;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteRelease">The API data object</param>
        /// <param name="release">The internal datarow</param>
        public static void PopulateRelease(RemoteRelease remoteRelease, ReleaseView release)
        {
            //Artifact Fields
            remoteRelease.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Release;
            remoteRelease.IsAttachments = release.IsAttachments;

            remoteRelease.ReleaseId = release.ReleaseId;
            remoteRelease.ProjectId = release.ProjectId;
            remoteRelease.IndentLevel = release.IndentLevel;
            remoteRelease.CreatorId = release.CreatorId;
            remoteRelease.OwnerId = release.OwnerId;
            remoteRelease.Name = release.Name;
            remoteRelease.Description = release.Description;
            remoteRelease.ReleaseStatusId = release.ReleaseStatusId;
            remoteRelease.ReleaseTypeId = release.ReleaseTypeId;
            remoteRelease.VersionNumber = release.VersionNumber;
            remoteRelease.CreationDate = release.CreationDate;
            remoteRelease.LastUpdateDate = release.LastUpdateDate;
            remoteRelease.Summary = release.IsSummary;
            remoteRelease.Active = release.IsActive;
            remoteRelease.StartDate = release.StartDate;
            remoteRelease.EndDate = release.EndDate;
            remoteRelease.ResourceCount = release.ResourceCount;
            remoteRelease.DaysNonWorking = release.DaysNonWorking;
            remoteRelease.CreatorName = release.CreatorName;
            remoteRelease.FullName = release.FullName;
            remoteRelease.ReleaseStatusName = release.ReleaseStatusName;
            remoteRelease.ReleaseTypeName = release.ReleaseTypeName;
            remoteRelease.OwnerName = release.OwnerName;
            remoteRelease.PercentComplete = release.PercentComplete;

            //Effort Information
            remoteRelease.PlannedEffort = release.PlannedEffort;
            remoteRelease.AvailableEffort = release.AvailableEffort;
            remoteRelease.TaskEstimatedEffort = release.TaskEstimatedEffort;
            remoteRelease.TaskActualEffort = release.TaskActualEffort;
            remoteRelease.TaskCount = release.TaskCount;

            //Test Information
            remoteRelease.CountBlocked = release.CountBlocked;
            remoteRelease.CountCaution = release.CountCaution;
            remoteRelease.CountFailed = release.CountFailed;
            remoteRelease.CountNotApplicable = release.CountNotApplicable;
            remoteRelease.CountNotRun = release.CountNotRun;
            remoteRelease.CountPassed = release.CountPassed;

            //Concurrency Management
            remoteRelease.ConcurrencyDate = release.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        public static void PopulateRequirementStep(RemoteRequirementStep remoteRequirementStep, RequirementStep requirementStep)
        {
            //Artifact Fields
            remoteRequirementStep.RequirementStepId = requirementStep.RequirementStepId;
            remoteRequirementStep.RequirementId = requirementStep.RequirementId;
            remoteRequirementStep.ConcurrencyDate = requirementStep.ConcurrencyDate;
            remoteRequirementStep.CreationDate = requirementStep.CreationDate;
            remoteRequirementStep.LastUpdateDate = requirementStep.LastUpdateDate;
            remoteRequirementStep.Position = requirementStep.Position;
            remoteRequirementStep.Description = requirementStep.Description;
        }
        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteRequirement">The API data object</param>
        /// <param name="requirement">The internal entity</param>
        public static void PopulateRequirement(RemoteRequirement remoteRequirement, RequirementView requirement)
        {
            //Artifact Fields
            remoteRequirement.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Requirement;
            remoteRequirement.IsAttachments = requirement.IsAttachments;

            remoteRequirement.RequirementId = requirement.RequirementId;
            remoteRequirement.StatusId = requirement.RequirementStatusId;
            remoteRequirement.RequirementTypeId = requirement.RequirementTypeId;
            remoteRequirement.ProjectId = requirement.ProjectId;
            remoteRequirement.IndentLevel = requirement.IndentLevel;
            remoteRequirement.AuthorId = requirement.AuthorId;
            remoteRequirement.OwnerId = requirement.OwnerId;
            remoteRequirement.ImportanceId = requirement.ImportanceId;
            remoteRequirement.ReleaseId = requirement.ReleaseId;
            remoteRequirement.ComponentId = requirement.ComponentId;
            remoteRequirement.Name = requirement.Name;
            remoteRequirement.Description =requirement.Description;
            remoteRequirement.CreationDate = requirement.CreationDate;
            remoteRequirement.LastUpdateDate = requirement.LastUpdateDate;
            remoteRequirement.Summary = requirement.IsSummary;
            remoteRequirement.CoverageCountTotal = requirement.CoverageCountTotal;
            remoteRequirement.CoverageCountPassed = requirement.CoverageCountPassed;
            remoteRequirement.CoverageCountFailed = requirement.CoverageCountFailed;
            remoteRequirement.CoverageCountCaution = requirement.CoverageCountCaution;
            remoteRequirement.CoverageCountBlocked = requirement.CoverageCountBlocked;
            remoteRequirement.EstimatePoints = requirement.EstimatePoints;
            remoteRequirement.EstimatedEffort = requirement.EstimatedEffort;
            remoteRequirement.TaskEstimatedEffort = requirement.TaskEstimatedEffort;
            remoteRequirement.TaskActualEffort = requirement.TaskActualEffort;
            remoteRequirement.TaskCount = requirement.TaskCount;
            remoteRequirement.ReleaseVersionNumber = requirement.ReleaseVersionNumber;
            remoteRequirement.AuthorName = requirement.AuthorName;
            remoteRequirement.OwnerName = requirement.OwnerName;
            remoteRequirement.StatusName = requirement.RequirementStatusName;
            remoteRequirement.ImportanceName = requirement.ImportanceName;
            remoteRequirement.ProjectName = requirement.ProjectName;
            remoteRequirement.RequirementTypeName = requirement.RequirementTypeName;

            //Concurrency Management
            remoteRequirement.ConcurrencyDate = requirement.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder
        /// </summary>
        /// <param name="remoteTestCaseFolder">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        public static void PopulateTestCaseFolder(RemoteTestCaseFolder remoteTestCaseFolder, TestCaseFolder testCaseFolder)
        {
            remoteTestCaseFolder.TestCaseFolderId = testCaseFolder.TestCaseFolderId;
            remoteTestCaseFolder.ParentTestCaseFolderId = testCaseFolder.ParentTestCaseFolderId;
            remoteTestCaseFolder.ProjectId = testCaseFolder.ProjectId;
            remoteTestCaseFolder.Name = testCaseFolder.Name;
            remoteTestCaseFolder.Description = testCaseFolder.Description;
            remoteTestCaseFolder.LastUpdateDate = testCaseFolder.LastUpdateDate;
            remoteTestCaseFolder.ExecutionDate = testCaseFolder.ExecutionDate;
            remoteTestCaseFolder.EstimatedDuration = testCaseFolder.EstimatedDuration;
            remoteTestCaseFolder.ActualDuration = testCaseFolder.ActualDuration;
            remoteTestCaseFolder.CountBlocked = testCaseFolder.CountBlocked;
            remoteTestCaseFolder.CountCaution = testCaseFolder.CountCaution;
            remoteTestCaseFolder.CountFailed = testCaseFolder.CountFailed;
            remoteTestCaseFolder.CountNotApplicable = testCaseFolder.CountNotApplicable;
            remoteTestCaseFolder.CountNotRun = testCaseFolder.CountNotRun;
            remoteTestCaseFolder.CountPassed = testCaseFolder.CountPassed;
            if (testCaseFolder.Project != null)
            {
                remoteTestCaseFolder.ProjectName = testCaseFolder.Project.Name;
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder
        /// </summary>
        /// <param name="remoteTestCaseFolder">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        public static void PopulateTestCaseFolder(RemoteTestCaseFolder remoteTestCaseFolder, TestCaseFolderHierarchyView testCaseFolder)
        {
            remoteTestCaseFolder.TestCaseFolderId = testCaseFolder.TestCaseFolderId;
            remoteTestCaseFolder.ParentTestCaseFolderId = testCaseFolder.ParentTestCaseFolderId;
            remoteTestCaseFolder.Name = testCaseFolder.Name;
            remoteTestCaseFolder.IndentLevel = testCaseFolder.IndentLevel;
            //Needed to fix WCF JSON bug for specific time zones - avoid returning a null
            remoteTestCaseFolder.LastUpdateDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder
        /// </summary>
        /// <param name="remoteTestCaseFolder">The API data object</param>
        /// <param name="testCaseFolder">The internal datarow</param>
        public static void PopulateTestCaseFolder(RemoteTestCaseFolder remoteTestCaseFolder, TestCaseFolderReleaseView testCaseFolder)
        {
            remoteTestCaseFolder.TestCaseFolderId = testCaseFolder.TestCaseFolderId;
            remoteTestCaseFolder.ParentTestCaseFolderId = testCaseFolder.ParentTestCaseFolderId;
            remoteTestCaseFolder.ProjectId = testCaseFolder.ProjectId;
            remoteTestCaseFolder.Name = testCaseFolder.Name;
            remoteTestCaseFolder.Description = testCaseFolder.Description;
            remoteTestCaseFolder.LastUpdateDate = testCaseFolder.LastUpdateDate;
            remoteTestCaseFolder.ExecutionDate = testCaseFolder.ExecutionDate;
            remoteTestCaseFolder.EstimatedDuration = testCaseFolder.EstimatedDuration;
            remoteTestCaseFolder.ActualDuration = testCaseFolder.ActualDuration;
            remoteTestCaseFolder.CountBlocked = testCaseFolder.CountBlocked;
            remoteTestCaseFolder.CountCaution = testCaseFolder.CountCaution;
            remoteTestCaseFolder.CountFailed = testCaseFolder.CountFailed;
            remoteTestCaseFolder.CountNotApplicable = testCaseFolder.CountNotApplicable;
            remoteTestCaseFolder.CountNotRun = testCaseFolder.CountNotRun;
            remoteTestCaseFolder.CountPassed = testCaseFolder.CountPassed;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder
        /// </summary>
        /// <param name="remoteTestSetFolder">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSetFolder(RemoteTestSetFolder remoteTestSetFolder, TestSetFolder testSetFolder)
        {
            remoteTestSetFolder.TestSetFolderId = testSetFolder.TestSetFolderId;
            remoteTestSetFolder.ParentTestSetFolderId = testSetFolder.ParentTestSetFolderId;
            remoteTestSetFolder.ProjectId = testSetFolder.ProjectId;
            remoteTestSetFolder.Name = testSetFolder.Name;
            remoteTestSetFolder.Description = testSetFolder.Description;
            remoteTestSetFolder.CreationDate = testSetFolder.CreationDate;
            remoteTestSetFolder.LastUpdateDate = testSetFolder.LastUpdateDate;
            remoteTestSetFolder.ExecutionDate = testSetFolder.ExecutionDate;
            remoteTestSetFolder.EstimatedDuration = testSetFolder.EstimatedDuration;
            remoteTestSetFolder.ActualDuration = testSetFolder.ActualDuration;
            remoteTestSetFolder.CountBlocked = testSetFolder.CountBlocked;
            remoteTestSetFolder.CountCaution = testSetFolder.CountCaution;
            remoteTestSetFolder.CountFailed = testSetFolder.CountFailed;
            remoteTestSetFolder.CountNotApplicable = testSetFolder.CountNotApplicable;
            remoteTestSetFolder.CountNotRun = testSetFolder.CountNotRun;
            remoteTestSetFolder.CountPassed = testSetFolder.CountPassed;
            if (testSetFolder.Project != null)
            {
                remoteTestSetFolder.ProjectName = testSetFolder.Project.Name;
            }
        }

        /// <summary>
        /// Populates a data-sync system API object from a test folder
        /// </summary>
        /// <param name="remoteTestSetFolder">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSetFolder(RemoteTestSetFolder remoteTestSetFolder, TestSetFolderReleaseView testSetFolder)
        {
            remoteTestSetFolder.TestSetFolderId = testSetFolder.TestSetFolderId;
            remoteTestSetFolder.ParentTestSetFolderId = testSetFolder.ParentTestSetFolderId;
            if (testSetFolder.ProjectId.HasValue)
            {
                remoteTestSetFolder.ProjectId = testSetFolder.ProjectId.Value;
            }
            remoteTestSetFolder.Name = testSetFolder.Name;
            remoteTestSetFolder.Description = testSetFolder.Description;
            if (testSetFolder.LastUpdateDate.HasValue)
            {
                remoteTestSetFolder.LastUpdateDate = testSetFolder.LastUpdateDate.Value;
            }
            //Needed to fix WCF JSON bug for specific time zones - avoid returning a null
            else
            {
                remoteTestSetFolder.LastUpdateDate = DateTime.UtcNow;
            }
            remoteTestSetFolder.ExecutionDate = testSetFolder.ExecutionDate;
            remoteTestSetFolder.EstimatedDuration = testSetFolder.EstimatedDuration;
            remoteTestSetFolder.ActualDuration = testSetFolder.ActualDuration;
            remoteTestSetFolder.CountBlocked = testSetFolder.CountBlocked;
            remoteTestSetFolder.CountCaution = testSetFolder.CountCaution;
            remoteTestSetFolder.CountFailed = testSetFolder.CountFailed;
            remoteTestSetFolder.CountNotApplicable = testSetFolder.CountNotApplicable;
            remoteTestSetFolder.CountNotRun = testSetFolder.CountNotRun;
            remoteTestSetFolder.CountPassed = testSetFolder.CountPassed;
        }

        /// <summary>
        /// Populates a data-sync system API object from a test set folder
        /// </summary>
        /// <param name="remoteTestSetFolder">The API data object</param>
        /// <param name="testSetFolder">The internal datarow</param>
        public static void PopulateTestSetFolder(RemoteTestSetFolder remoteTestSetFolder, TestSetFolderHierarchyView testSetFolder)
        {
            remoteTestSetFolder.TestSetFolderId = testSetFolder.TestSetFolderId;
            remoteTestSetFolder.ParentTestSetFolderId = testSetFolder.ParentTestSetFolderId;
            remoteTestSetFolder.Name = testSetFolder.Name;
            remoteTestSetFolder.IndentLevel = testSetFolder.IndentLevel;
            //Needed to fix WCF JSON bug for specific time zones - avoid returning a null
            remoteTestSetFolder.LastUpdateDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCase">The internal datarow</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseView testCase)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;
            remoteTestCase.IsAttachments = testCase.IsAttachments;

            remoteTestCase.TestCaseId = testCase.TestCaseId;
            remoteTestCase.ProjectId = testCase.ProjectId;
            remoteTestCase.ExecutionStatusId = testCase.ExecutionStatusId;
            remoteTestCase.AuthorId = testCase.AuthorId;
            remoteTestCase.OwnerId = testCase.OwnerId;
            remoteTestCase.TestCaseTypeId = testCase.TestCaseTypeId;
            remoteTestCase.TestCaseStatusId = testCase.TestCaseStatusId;
            remoteTestCase.TestCaseFolderId = testCase.TestCaseFolderId;
            remoteTestCase.ComponentIds = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
            remoteTestCase.TestCasePriorityId = testCase.TestCasePriorityId;
            remoteTestCase.AutomationEngineId = testCase.AutomationEngineId;
            remoteTestCase.AutomationAttachmentId = testCase.AutomationAttachmentId;
            remoteTestCase.Name = testCase.Name;
            remoteTestCase.Description = testCase.Description;
            remoteTestCase.CreationDate = testCase.CreationDate;
            remoteTestCase.LastUpdateDate = testCase.LastUpdateDate;
            remoteTestCase.ExecutionDate = testCase.ExecutionDate;
            remoteTestCase.EstimatedDuration = testCase.EstimatedDuration;
            remoteTestCase.ActualDuration = testCase.ActualDuration;
            remoteTestCase.IsSuspect = testCase.IsSuspect;
            remoteTestCase.IsTestSteps = testCase.IsTestSteps;

            //Lookups
            remoteTestCase.AuthorName = testCase.AuthorName;
            remoteTestCase.OwnerName = testCase.OwnerName;
            remoteTestCase.ProjectName = testCase.ProjectName;
            remoteTestCase.TestCasePriorityName = testCase.TestCasePriorityName;
            remoteTestCase.TestCaseStatusName = testCase.TestCaseStatusName;
            remoteTestCase.TestCaseTypeName = testCase.TestCaseTypeName;
            remoteTestCase.ExecutionStatusName = testCase.ExecutionStatusName;

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testCase.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testCase">The internal datarow</param>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestCaseReleaseView testCase)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;
            remoteTestCase.IsAttachments = testCase.IsAttachments;

            remoteTestCase.TestCaseId = testCase.TestCaseId;
            remoteTestCase.ProjectId = testCase.ProjectId;
            remoteTestCase.ExecutionStatusId = testCase.ExecutionStatusId;
            remoteTestCase.AuthorId = testCase.AuthorId;
            remoteTestCase.OwnerId = testCase.OwnerId;
            remoteTestCase.TestCaseTypeId = testCase.TestCaseTypeId;
            remoteTestCase.TestCaseStatusId = testCase.TestCaseStatusId;
            remoteTestCase.TestCaseFolderId = testCase.TestCaseFolderId;
            remoteTestCase.ComponentIds = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
            remoteTestCase.TestCasePriorityId = testCase.TestCasePriorityId;
            remoteTestCase.AutomationEngineId = testCase.AutomationEngineId;
            remoteTestCase.AutomationAttachmentId = testCase.AutomationAttachmentId;
            remoteTestCase.Name = testCase.Name;
            remoteTestCase.Description = testCase.Description;
            remoteTestCase.CreationDate = testCase.CreationDate;
            remoteTestCase.LastUpdateDate = testCase.LastUpdateDate;
            remoteTestCase.ExecutionDate = testCase.ExecutionDate;
            remoteTestCase.EstimatedDuration = testCase.EstimatedDuration;
            remoteTestCase.ActualDuration = testCase.ActualDuration;
            remoteTestCase.IsTestSteps = testCase.IsTestSteps;

            //Lookups
            remoteTestCase.AuthorName = testCase.AuthorName;
            remoteTestCase.OwnerName = testCase.OwnerName;
            remoteTestCase.TestCasePriorityName = testCase.TestCasePriorityName;
            remoteTestCase.TestCaseStatusName = testCase.TestCaseStatusName;
            remoteTestCase.TestCaseTypeName = testCase.TestCaseTypeName;
            remoteTestCase.ExecutionStatusName = testCase.ExecutionStatusName;

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testCase.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetView">The internal datarow</param>
        public static void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetView testSetView)
        {
            //Artifact Fields
            remoteTestSet.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestSet;
            remoteTestSet.IsAttachments = testSetView.IsAttachments;

            remoteTestSet.TestSetId = testSetView.TestSetId;
            remoteTestSet.ProjectId = testSetView.ProjectId;
            remoteTestSet.TestSetStatusId = testSetView.TestSetStatusId;
            remoteTestSet.CreatorId = testSetView.CreatorId;
            remoteTestSet.OwnerId = testSetView.OwnerId;
            remoteTestSet.ReleaseId = testSetView.ReleaseId;
            remoteTestSet.TestSetFolderId = testSetView.TestSetFolderId;
            remoteTestSet.AutomationHostId = testSetView.AutomationHostId;
            remoteTestSet.TestRunTypeId = testSetView.TestRunTypeId;
            remoteTestSet.RecurrenceId = testSetView.RecurrenceId;
            remoteTestSet.Name = testSetView.Name;
            remoteTestSet.Description = testSetView.Description;
            remoteTestSet.CreationDate = testSetView.CreationDate;
            remoteTestSet.LastUpdateDate = testSetView.LastUpdateDate;
            remoteTestSet.ExecutionDate = testSetView.ExecutionDate;
            remoteTestSet.PlannedDate = testSetView.PlannedDate;
            remoteTestSet.CountPassed = testSetView.CountPassed;
            remoteTestSet.CountFailed = testSetView.CountFailed;
            remoteTestSet.CountCaution = testSetView.CountCaution;
            remoteTestSet.CountBlocked = testSetView.CountBlocked;
            remoteTestSet.CountNotRun = testSetView.CountNotRun;
            remoteTestSet.CountNotApplicable = testSetView.CountNotApplicable;
            remoteTestSet.EstimatedDuration = testSetView.EstimatedDuration;
            remoteTestSet.ActualDuration = testSetView.ActualDuration;
            remoteTestSet.IsAutoScheduled = testSetView.IsAutoScheduled;
            remoteTestSet.BuildExecuteTimeInterval = testSetView.BuildExecuteTimeInterval;
            remoteTestSet.IsDynamic = testSetView.IsDynamic;
            remoteTestSet.DynamicQuery = testSetView.DynamicQuery;
            remoteTestSet.TestConfigurationSetId = testSetView.TestConfigurationSetId;

            //Lookups
            remoteTestSet.CreatorName = testSetView.CreatorName;
            remoteTestSet.OwnerName = testSetView.OwnerName;
            remoteTestSet.ProjectName = testSetView.ProjectName;
            remoteTestSet.TestSetStatusName = testSetView.TestSetStatusName;
            remoteTestSet.ReleaseVersionNumber = testSetView.ReleaseVersionNumber;
            remoteTestSet.RecurrenceName = testSetView.RecurrenceName;

            //Concurrency Management
            remoteTestSet.ConcurrencyDate = testSetView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestSet">The API data object</param>
        /// <param name="testSetView">The internal datarow</param>
        public static void PopulateTestSet(RemoteTestSet remoteTestSet, TestSetReleaseView testSetView)
        {
            //Artifact Fields
            remoteTestSet.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestSet;
            remoteTestSet.IsAttachments = testSetView.IsAttachments;

            remoteTestSet.TestSetId = testSetView.TestSetId;
            remoteTestSet.ProjectId = testSetView.ProjectId;
            remoteTestSet.TestSetStatusId = testSetView.TestSetStatusId;
            remoteTestSet.CreatorId = testSetView.CreatorId;
            remoteTestSet.OwnerId = testSetView.OwnerId;
            remoteTestSet.ReleaseId = testSetView.ReleaseId;
            remoteTestSet.TestSetFolderId = testSetView.TestSetFolderId;
            remoteTestSet.AutomationHostId = testSetView.AutomationHostId;
            remoteTestSet.TestRunTypeId = testSetView.TestRunTypeId;
            remoteTestSet.RecurrenceId = testSetView.RecurrenceId;
            remoteTestSet.Name = testSetView.Name;
            remoteTestSet.Description = testSetView.Description;
            remoteTestSet.CreationDate = testSetView.CreationDate;
            remoteTestSet.LastUpdateDate = testSetView.LastUpdateDate;
            remoteTestSet.ExecutionDate = testSetView.ExecutionDate;
            remoteTestSet.PlannedDate = testSetView.PlannedDate;
            remoteTestSet.CountPassed = testSetView.CountPassed;
            remoteTestSet.CountFailed = testSetView.CountFailed;
            remoteTestSet.CountCaution = testSetView.CountCaution;
            remoteTestSet.CountBlocked = testSetView.CountBlocked;
            remoteTestSet.CountNotRun = testSetView.CountNotRun;
            remoteTestSet.CountNotApplicable = testSetView.CountNotApplicable;
            remoteTestSet.EstimatedDuration = testSetView.EstimatedDuration;
            remoteTestSet.ActualDuration = testSetView.ActualDuration;

            //Lookups
            remoteTestSet.CreatorName = testSetView.CreatorName;
            remoteTestSet.OwnerName = testSetView.OwnerName;
            remoteTestSet.ProjectName = testSetView.ProjectName;
            remoteTestSet.TestSetStatusName = testSetView.TestSetStatusName;
            remoteTestSet.ReleaseVersionNumber = testSetView.ReleaseVersionNumber;
            remoteTestSet.RecurrenceName = testSetView.RecurrenceName;

            //Concurrency Management
            remoteTestSet.ConcurrencyDate = testSetView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestStep">The API data object</param>
        /// <param name="testStepView">The internal datarow</param>
        /// <param name="projectId">The id of the current project</param>
        public static void PopulateTestStep(RemoteTestStep remoteTestStep, TestStepView testStepView, int projectId)
        {
            //Artifact Fields
            remoteTestStep.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestStep;
            remoteTestStep.IsAttachments = testStepView.IsAttachments;

            remoteTestStep.TestStepId = testStepView.TestStepId;
            remoteTestStep.ProjectId = projectId;
            remoteTestStep.TestCaseId = testStepView.TestCaseId;
            remoteTestStep.ExecutionStatusId = testStepView.ExecutionStatusId;
            remoteTestStep.Position = testStepView.Position;
            remoteTestStep.Description = testStepView.Description;
            remoteTestStep.ExpectedResult = testStepView.ExpectedResult;
            remoteTestStep.SampleData = testStepView.SampleData;
            remoteTestStep.LinkedTestCaseId = testStepView.LinkedTestCaseId;
            remoteTestStep.LastUpdateDate = testStepView.LastUpdateDate;
            remoteTestStep.Precondition = testStepView.Precondition;

            //Concurrency Management
            remoteTestStep.ConcurrencyDate = testStepView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestStep">The API data object</param>
        /// <param name="testStepView">The internal datarow</param>
        /// <param name="projectId">The id of the current project</param>
        public static void PopulateTestStep(RemoteTestStep remoteTestStep, TestStep testStep, int projectId)
        {
            //Artifact Fields
            remoteTestStep.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestStep;
            remoteTestStep.IsAttachments = testStep.IsAttachments;

            remoteTestStep.TestStepId = testStep.TestStepId;
            remoteTestStep.ProjectId = projectId;
            remoteTestStep.TestCaseId = testStep.TestCaseId;
            remoteTestStep.ExecutionStatusId = testStep.ExecutionStatusId;
            remoteTestStep.Position = testStep.Position;
            remoteTestStep.Description = testStep.Description;
            remoteTestStep.ExpectedResult = testStep.ExpectedResult;
            remoteTestStep.SampleData = testStep.SampleData;
            remoteTestStep.LinkedTestCaseId = testStep.LinkedTestCaseId;
            remoteTestStep.LastUpdateDate = testStep.LastUpdateDate;
            remoteTestStep.Precondition = testStep.Precondition;

            //Concurrency Management
            remoteTestStep.ConcurrencyDate = testStep.ConcurrencyDate;
        }

		/// <summary>
		/// Adds new test step parameters
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testStep">The test step object identified by id in the API request</param>
		/// <param name="testStepParameters">Array of parameter objects (names and values) passed in with the API call</param>
		public static void PopulateTestStepParameter(int projectId, TestStep testStep, List<RemoteTestStepParameter> remoteTestStepParameters)
		{
			//Call the business object to actually retrieve the test case parameters
			TestCaseManager testCaseManager = new TestCaseManager();
			
			//Next retrieve the parameters set on the linked test step for the specific test case
			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);

			//Get the parameters of the test case itself that is a linked test step (so we can access the proper IDs)
			List<TestCaseParameter> testCaseParameters = new List<TestCaseParameter>();
			testCaseParameters = testCaseManager.RetrieveParameters(testStep.TestCaseId, true, true);

			//Loop through remote parameters
			List<TestStepParameter> testStepParametersToUpdate = new List<TestStepParameter>();
			//Add any new parameters
			foreach (TestCaseParameter testCaseParameter in testCaseParameters)
			{
				TestStepParameter testStepParameter = testStepParameterValues.FirstOrDefault(p => p.TestCaseParameterId == testCaseParameter.TestCaseParameterId);
				// if the parameter is already set just add it as is - it does not change
				if (testStepParameter != null)
				{
					testStepParametersToUpdate.Add(testStepParameter);
				}
				// if the parameter is NOT set see if we should add it
				else
				{
					RemoteTestStepParameter remoteTestStepParameter = remoteTestStepParameters.FirstOrDefault(p => p.Name == testCaseParameter.Name);
					if (remoteTestStepParameter != null)
					{
						TestStepParameter addTestStepParameter = new TestStepParameter()
						{
							Value = remoteTestStepParameter.Value,
							TestStepId = testStep.TestStepId,
							TestCaseParameterId = testCaseParameter.TestCaseParameterId
						};
						testStepParametersToUpdate.Add(addTestStepParameter);
					}
				}
			}

			//Add the new parameters to the linked test step
			testCaseManager.SaveParameterValues(projectId, testStep.TestStepId, testStepParametersToUpdate);
		}

		/// <summary>
		/// Populates a user API object from the internal datarow
		/// </summary>
		/// <param name="remoteUser">The API data object</param>
		/// <param name="isSysAdmin">is the logged-in user a sysadmin</param>
		/// <param name="user">The internal user entity object</param>
		/// <remarks>It only populates the RSS token when the user calling the API is a sysadmin for security reasons</remarks>
		public static void PopulateUser(RemoteUser remoteUser, User user, bool isSysAdmin)
        {
            remoteUser.UserId = user.UserId;
            remoteUser.FirstName = user.Profile.FirstName;
            remoteUser.LastName = user.Profile.LastName;
            remoteUser.MiddleInitial = user.Profile.MiddleInitial;
            remoteUser.UserName = user.UserName;
            remoteUser.LdapDn = user.LdapDn;
            remoteUser.EmailAddress = user.EmailAddress;
            remoteUser.Active = (user.IsActive);
            remoteUser.Admin = (user.Profile.IsAdmin);
            remoteUser.Department = user.Profile.Department;
            remoteUser.Approved = user.IsApproved;
            remoteUser.Locked = user.IsLocked;
            if (isSysAdmin)
            {
                remoteUser.RssToken = user.RssToken;
            }
        }


        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteTestCase">The API data object</param>
        /// <param name="testSetTestCaseView">The internal datarow</param>
        /// <remarks>Used to translate the test set test case datarow into a normal test case API object</remarks>
        public static void PopulateTestCase(RemoteTestCase remoteTestCase, TestSetTestCaseView testSetTestCaseView)
        {
            //Artifact Fields
            remoteTestCase.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.TestCase;

            remoteTestCase.TestCaseId = testSetTestCaseView.TestCaseId;
            remoteTestCase.ProjectId = testSetTestCaseView.ProjectId;
            remoteTestCase.ExecutionStatusId = testSetTestCaseView.ExecutionStatusId;
            remoteTestCase.AuthorId = testSetTestCaseView.AuthorId;
            remoteTestCase.OwnerId = testSetTestCaseView.OwnerId;
            remoteTestCase.TestCasePriorityId = testSetTestCaseView.TestCasePriorityId;
            remoteTestCase.Name = testSetTestCaseView.Name;
            remoteTestCase.Description = testSetTestCaseView.Description;
            remoteTestCase.CreationDate = testSetTestCaseView.CreationDate;
            remoteTestCase.LastUpdateDate = testSetTestCaseView.LastUpdateDate;
            remoteTestCase.ExecutionDate = testSetTestCaseView.ExecutionDate;
            remoteTestCase.EstimatedDuration = testSetTestCaseView.EstimatedDuration;
            remoteTestCase.ActualDuration = testSetTestCaseView.ActualDuration;
            remoteTestCase.IsTestSteps = testSetTestCaseView.IsTestSteps;
            remoteTestCase.TestCaseTypeId = testSetTestCaseView.TestCaseTypeId;
            remoteTestCase.TestCaseStatusId = testSetTestCaseView.TestCaseStatusId;

            //Lookups
            remoteTestCase.AuthorName = testSetTestCaseView.AuthorName;
            remoteTestCase.OwnerName = testSetTestCaseView.OwnerName;
            remoteTestCase.TestCasePriorityName = testSetTestCaseView.TestCasePriorityName;
            remoteTestCase.TestCaseStatusName = testSetTestCaseView.TestCaseStatusName;
            remoteTestCase.TestCaseTypeName = testSetTestCaseView.TestCaseTypeName;
            remoteTestCase.ExecutionStatusName = testSetTestCaseView.ExecutionStatusName;

            //Concurrency Management
            remoteTestCase.ConcurrencyDate = testSetTestCaseView.ConcurrencyDate;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="dataRow">The internal data row</param>
        public static void PopulateDocumentFolder(RemoteDocumentFolder remoteDocumentFolder, ProjectAttachmentFolderHierarchy projectAttachmentFolder)
        {
            remoteDocumentFolder.ProjectAttachmentFolderId = projectAttachmentFolder.ProjectAttachmentFolderId;
            remoteDocumentFolder.ProjectId = projectAttachmentFolder.ProjectId;
            remoteDocumentFolder.ParentProjectAttachmentFolderId = projectAttachmentFolder.ParentProjectAttachmentFolderId;
            remoteDocumentFolder.Name = projectAttachmentFolder.Name;
            remoteDocumentFolder.IndentLevel = projectAttachmentFolder.IndentLevel;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="dataRow">The internal data row</param>
        public static void PopulateDocumentFolder(RemoteDocumentFolder remoteDocumentFolder, ProjectAttachmentFolder projectAttachmentFolder)
        {
            remoteDocumentFolder.ProjectAttachmentFolderId = projectAttachmentFolder.ProjectAttachmentFolderId;
            remoteDocumentFolder.ProjectId = projectAttachmentFolder.ProjectId;
            remoteDocumentFolder.ParentProjectAttachmentFolderId = projectAttachmentFolder.ParentProjectAttachmentFolderId;
            remoteDocumentFolder.Name = projectAttachmentFolder.Name;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentFolder">The API data object</param>
        /// <param name="dataRow">The internal data row</param>
        public static void PopulateTaskFolder(RemoteTaskFolder remoteTaskFolder, TaskFolder taskFolder, string indentLevel = "")
        {
            remoteTaskFolder.TaskFolderId = taskFolder.TaskFolderId;
            remoteTaskFolder.ProjectId = taskFolder.ProjectId;
            remoteTaskFolder.ParentTaskFolderId = taskFolder.ParentTaskFolderId;
            remoteTaskFolder.Name = taskFolder.Name;
            remoteTaskFolder.IndentLevel = indentLevel;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        public static void PopulateTaskFolder(int projectId, RemoteTaskFolder remoteTaskFolder, TaskFolderHierarchyView taskFolder)
        {
            remoteTaskFolder.TaskFolderId = taskFolder.TaskFolderId;
            remoteTaskFolder.ProjectId = projectId;
            remoteTaskFolder.ParentTaskFolderId = taskFolder.ParentTaskFolderId;
            remoteTaskFolder.Name = taskFolder.Name;
            remoteTaskFolder.IndentLevel = taskFolder.IndentLevel;
        }

        /// <summary>
        /// Populates an API object from the internal datarow
        /// </summary>
        /// <param name="remoteDocumentVersion">The API data object</param>
        /// <param name="attachmentVersion">The internal datarow</param>
        public static void PopulateDocumentVersion(RemoteDocumentVersion remoteDocumentVersion, AttachmentVersionView attachmentVersion)
        {
            remoteDocumentVersion.AttachmentVersionId = attachmentVersion.AttachmentVersionId;
            remoteDocumentVersion.AttachmentId = attachmentVersion.AttachmentId;
            remoteDocumentVersion.AuthorId = attachmentVersion.AuthorId;
            remoteDocumentVersion.FilenameOrUrl = attachmentVersion.Filename;
            remoteDocumentVersion.Description = attachmentVersion.Description;
            remoteDocumentVersion.UploadDate = attachmentVersion.UploadDate;
            remoteDocumentVersion.Size = attachmentVersion.Size;
            remoteDocumentVersion.VersionNumber = attachmentVersion.VersionNumber;
            remoteDocumentVersion.AuthorName = attachmentVersion.AuthorName;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal entity
        /// </summary>
        /// <param name="remoteBuildSourceCode">The API data object</param>
        /// <param name="buildSourceCode">The internal entity</param>
        public static void PopulateBuildSourceCode(RemoteBuildSourceCode remoteBuildSourceCode, BuildSourceCode buildSourceCode)
        {
            remoteBuildSourceCode.BuildId = buildSourceCode.BuildId;
            remoteBuildSourceCode.RevisionKey = buildSourceCode.RevisionKey;
            remoteBuildSourceCode.CreationDate = buildSourceCode.CreationDate;
        }

        /// <summary>
        /// Populates a data-sync system API object from the internal datarow
        /// </summary>
        /// <param name="remoteAutomationHost">The API data object</param>
        /// <param name="automationHost">The internal datarow</param>
        public static void PopulateAutomationHost(RemoteAutomationHost remoteAutomationHost, AutomationHostView automationHost)
        {
            //Artifact Fields
            remoteAutomationHost.ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.AutomationHost;
            remoteAutomationHost.IsAttachments = automationHost.IsAttachments;

            remoteAutomationHost.AutomationHostId = automationHost.AutomationHostId;
            remoteAutomationHost.ProjectId = automationHost.ProjectId;
            remoteAutomationHost.Name = automationHost.Name;
            remoteAutomationHost.Description = automationHost.Description;
            remoteAutomationHost.Token = automationHost.Token;
            remoteAutomationHost.LastUpdateDate = automationHost.LastUpdateDate;
            remoteAutomationHost.Active = automationHost.IsActive;
            remoteAutomationHost.LastContactDate = automationHost.LastContactDate;

            //Concurrency Management
            remoteAutomationHost.ConcurrencyDate = automationHost.ConcurrencyDate;
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        /// <param name="remoteProject">The API data object</param>
        /// <param name="projectRow">The internal project entity</param>
        public static void PopulateProject(RemoteProject remoteProject, Project project)
        {
            remoteProject.ProjectId = project.ProjectId;
            remoteProject.ProjectTemplateId = project.ProjectTemplateId;
            remoteProject.ProjectGroupId = project.ProjectGroupId;
            remoteProject.Name = project.Name;
            remoteProject.Description = project.Description;
            remoteProject.Website = project.Website;
            remoteProject.CreationDate = project.CreationDate;
            remoteProject.Active = project.IsActive;
            remoteProject.WorkingHours = project.WorkingHours;
            remoteProject.WorkingDays = project.WorkingDays;
            remoteProject.NonWorkingHours = project.NonWorkingHours;
            remoteProject.StartDate = project.StartDate;
            remoteProject.EndDate = project.EndDate;
            remoteProject.PercentComplete = project.PercentComplete;
        }

        /// <summary>
        /// Populates a project template API object from the internal datarow
        /// </summary>
        /// <param name="remoteProjectTemplate">The API data object</param>
        /// <param name="projectTemplate">The internal project template entity</param>
        public static void PopulateProjectTemplate(RemoteProjectTemplate remoteProjectTemplate, ProjectTemplate projectTemplate)
        {
            remoteProjectTemplate.ProjectTemplateId = projectTemplate.ProjectTemplateId;
            remoteProjectTemplate.Name = projectTemplate.Name;
            remoteProjectTemplate.Description = projectTemplate.Description;
            remoteProjectTemplate.IsActive = projectTemplate.IsActive;
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        /// <param name="remoteProjectRole">The API data object</param>
        /// <param name="projectRoleRow">The internal project role datarow</param>
        public static void PopulateProjectRole(RemoteProjectRole remoteProjectRole, ProjectRole projectRoleRow)
        {
            //Project Role
            remoteProjectRole.ProjectRoleId = projectRoleRow.ProjectRoleId;
            remoteProjectRole.Name = projectRoleRow.Name;
            remoteProjectRole.Description = projectRoleRow.Description;
            remoteProjectRole.Active = (projectRoleRow.IsActive);
            remoteProjectRole.Admin = (projectRoleRow.IsAdmin);
            remoteProjectRole.DiscussionsAdd = (projectRoleRow.IsDiscussionsAdd);
            remoteProjectRole.SourceCodeView = (projectRoleRow.IsSourceCodeView);

            //Project Role Permissions
            if (projectRoleRow.RolePermissions != null)
            {
                remoteProjectRole.Permissions = new List<RemoteRolePermission>();
                foreach (ProjectRolePermission projectRolePermission in projectRoleRow.RolePermissions)
                {
                    RemoteRolePermission remoteRolePermission = new RemoteRolePermission();
                    remoteRolePermission.ArtifactTypeId = projectRolePermission.ArtifactTypeId;
                    remoteRolePermission.PermissionId = projectRolePermission.PermissionId;
                    remoteRolePermission.ProjectRoleId = projectRolePermission.ProjectRoleId;
                    remoteProjectRole.Permissions.Add(remoteRolePermission);
                }
            }
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        internal static void PopulateTestConfigurationSet(RemoteTestConfigurationSet remoteTestConfigurationSet, TestConfigurationSet testConfigurationSet)
        {
            remoteTestConfigurationSet.TestConfigurationSetId = testConfigurationSet.TestConfigurationSetId;
            remoteTestConfigurationSet.ProjectId = testConfigurationSet.ProjectId;
            remoteTestConfigurationSet.Name = testConfigurationSet.Name;
            remoteTestConfigurationSet.Description = testConfigurationSet.Description;
            remoteTestConfigurationSet.ConcurrencyDate = testConfigurationSet.ConcurrencyDate;
            remoteTestConfigurationSet.CreationDate = testConfigurationSet.CreationDate;
            remoteTestConfigurationSet.IsActive = testConfigurationSet.IsActive;
            remoteTestConfigurationSet.LastUpdatedDate = testConfigurationSet.LastUpdatedDate;
        }

        /// <summary>
        /// Populates a project API object from the internal datarow
        /// </summary>
        internal static void PopulateTestConfigurationEntries(RemoteTestConfigurationSet remoteTestConfigurationSet, List<TestConfigurationEntry> testConfigurationEntries)
        {
            if (testConfigurationEntries != null && testConfigurationEntries.Count > 0)
            {
                remoteTestConfigurationSet.Entries = new List<RemoteTestConfigurationEntry>();
                
                //Group by the entry
                List<IGrouping<int, TestConfigurationEntry>> testConfigurationsGroupedByConfiguration = testConfigurationEntries.GroupBy(t => t.TestCaseConfigurationId).ToList();
                foreach (IGrouping<int, TestConfigurationEntry> testConfigurationEntry in testConfigurationsGroupedByConfiguration)
                {
                    RemoteTestConfigurationEntry remoteTestConfigurationEntry = new RemoteTestConfigurationEntry();
                    remoteTestConfigurationEntry.TestConfigurationEntryId = testConfigurationEntry.Key;
                    remoteTestConfigurationSet.Entries.Add(remoteTestConfigurationEntry);

                    //Now add the parameter values
                    remoteTestConfigurationEntry.ParameterValues = new List<RemoteTestConfigurationParameterValue>();
                    foreach (TestConfigurationEntry testConfigurationEntryParam in testConfigurationEntry)
                    {
                        RemoteTestConfigurationParameterValue remoteTestConfigurationParameterValue = new RemoteTestConfigurationParameterValue();
                        remoteTestConfigurationParameterValue.TestCaseParameterId = testConfigurationEntryParam.TestCaseParameterId;
                        remoteTestConfigurationParameterValue.Name = testConfigurationEntryParam.ParameterName;
                        remoteTestConfigurationParameterValue.Value = testConfigurationEntryParam.ParameterValue;
                        remoteTestConfigurationEntry.ParameterValues.Add(remoteTestConfigurationParameterValue);
                    }
                }
            }
        }

		/// <summary>
		/// Populates the source code API object from the internal datarow
		/// </summary>
		internal static void PopulateSourceCodeConnection(int userId, int projectId, RemoteSourceCodeConnection remoteSourceCodeConnection)
		{
			//See if we have TaraVault or not
			SourceCodeManager sourceCodeManager = new SourceCodeManager(projectId);
			if (sourceCodeManager.RepositoryName == VaultManager.SOURCE_CODE_PROVIDER_TARA_VAULT)
			{
				VaultManager vaultManager = new VaultManager();
				Project tvProject = vaultManager.Project_RetrieveWithTaraVault(projectId);
				User tvUser = vaultManager.User_RetrieveWithTaraVault(userId);
				if (tvUser != null && tvUser.TaraVault != null && tvUser.TaraVault.VaultProject.Any(p => p.ProjectId == projectId))
				{
					remoteSourceCodeConnection.ProviderName = tvProject.TaraVault.VaultType.Name;
					remoteSourceCodeConnection.Connection = vaultManager.Project_GetConnectionString(projectId);
					remoteSourceCodeConnection.Login = tvUser.TaraVault.VaultUserLogin;
					remoteSourceCodeConnection.Password = new SimpleAES().DecryptString(tvUser.TaraVault.Password);
				}
			}
			else
			{
				remoteSourceCodeConnection.ProviderName = sourceCodeManager.RepositoryName;
				remoteSourceCodeConnection.Connection = sourceCodeManager.Connection;
			}
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		internal static void PopulateRiskStatus(RemoteRiskStatus remoteRiskStatus, RiskStatus riskStatus)
		{
			remoteRiskStatus.RiskStatusId = riskStatus.RiskStatusId;
			remoteRiskStatus.Name = riskStatus.Name;
			remoteRiskStatus.Position = riskStatus.Position;
			remoteRiskStatus.Active = riskStatus.IsActive;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		internal static void PopulateRiskType(RemoteRiskType remoteRiskType, RiskType riskType)
		{
			remoteRiskType.RiskTypeId = riskType.RiskTypeId;
			remoteRiskType.Name = riskType.Name;
			remoteRiskType.IsActive = riskType.IsActive;
			remoteRiskType.IsDefault = riskType.IsDefault;
			remoteRiskType.WorkflowId = riskType.RiskWorkflowId;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		internal static void PopulateRiskProbability(RemoteRiskProbability remoteRiskProbability, RiskProbability riskProbability)
		{
			remoteRiskProbability.RiskProbabilityId = riskProbability.RiskProbabilityId;
			remoteRiskProbability.Name = riskProbability.Name;
			remoteRiskProbability.Position = riskProbability.Position;
			remoteRiskProbability.Active = riskProbability.IsActive;
			remoteRiskProbability.Score = riskProbability.Score;
			remoteRiskProbability.Color = riskProbability.Color;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		internal static void PopulateRiskImpact(RemoteRiskImpact remoteRiskImpact, RiskImpact riskImpact)
		{
			remoteRiskImpact.RiskImpactId = riskImpact.RiskImpactId;
			remoteRiskImpact.Name = riskImpact.Name;
			remoteRiskImpact.Position = riskImpact.Position;
			remoteRiskImpact.Active = riskImpact.IsActive;
			remoteRiskImpact.Score = riskImpact.Score;
			remoteRiskImpact.Color = riskImpact.Color;
		}

		/// <summary>
		/// Populates a data-sync system API object from the internal datarow
		/// </summary>
		internal static void PopulateRiskMitigation(RemoteRiskMitigation remoteRiskMitigation, RiskMitigation riskMitigation)
		{
			//Artifact Fields
			remoteRiskMitigation.RiskMitigationId = riskMitigation.RiskMitigationId;
			remoteRiskMitigation.RiskId = riskMitigation.RiskId;
			remoteRiskMitigation.ConcurrencyDate = riskMitigation.ConcurrencyDate;
			remoteRiskMitigation.CreationDate = riskMitigation.CreationDate;
			remoteRiskMitigation.LastUpdateDate = riskMitigation.LastUpdateDate;
			remoteRiskMitigation.Position = riskMitigation.Position;
			remoteRiskMitigation.Description = riskMitigation.Description;
			remoteRiskMitigation.ReviewDate = riskMitigation.ReviewDate;
			remoteRiskMitigation.IsActive = riskMitigation.IsActive;
			remoteRiskMitigation.IsDeleted = riskMitigation.IsDeleted;
		}
	}
}
