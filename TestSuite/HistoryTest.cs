#pragma warning disable CS0612

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>This fixture tests the History business object</summary> 
	[TestFixture]
	[SpiraTestConfiguration(
		InternalRoutines.SPIRATEST_INTERNAL_URL,
		InternalRoutines.SPIRATEST_INTERNAL_LOGIN,
		InternalRoutines.SPIRATEST_INTERNAL_PASSWORD,
		InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID,
		InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class HistoryTest
	{
		protected static HistoryManager historyManager;

		private const int PROJECT_ID = 1;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private static int projectId;
		private static int projectTemplateId;

		private static int documentTypeId1;
		private static int documentTypeId2;

		/// <summary>Initializes the business objects being tested</summary> 
		[TestFixtureSetUp]
		public void Init()
		{
			//Create business classes 
			historyManager = new HistoryManager();

			//Create a new project for testing with (only some tests currently use this) 
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("HistoryManagerTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project 
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Add a couple of document types 
			AttachmentManager attachmentManager = new AttachmentManager();
			documentTypeId1 = attachmentManager.InsertDocumentType(projectTemplateId, "Diagram", null, true, false);
			documentTypeId2 = attachmentManager.InsertDocumentType(projectTemplateId, "Specification", null, true, false);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template 
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		/// <summary>Tests the overall retrieve function.</summary> 
		[Test]
		[SpiraTestCase(139)]
		public void _01_Retrieves()
		{
			//Lets test that we can retrieve the change history for a requirement 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChangeDetails;
			historyManager.RetrieveByArtifactId(4, DataModel.Artifact.ArtifactTypeEnum.Requirement, out historyChangeSets, out historyChangeDetails, out historyChangeSetTypes);

			#region Changeset Type (Static) 
			//Check static data first. 
			Assert.AreEqual(15, historyChangeSetTypes.Count);
			//- Modified 
			Assert.AreEqual(1, historyChangeSetTypes[0].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSetTypes[0].Name);
			//- Deleted 
			Assert.AreEqual(2, historyChangeSetTypes[1].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSetTypes[1].Name);
			//- Added 
			Assert.AreEqual(3, historyChangeSetTypes[2].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSetTypes[2].Name);
			//- Purged 
			Assert.AreEqual(4, historyChangeSetTypes[3].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSetTypes[3].Name);
			//- Rollback 
			Assert.AreEqual(5, historyChangeSetTypes[4].ChangeTypeId);
			Assert.AreEqual("Rollback", historyChangeSetTypes[4].Name);
			//- Undelete 
			Assert.AreEqual(6, historyChangeSetTypes[5].ChangeTypeId);
			Assert.AreEqual("Undelete", historyChangeSetTypes[5].Name);
			//- Imported 
			Assert.AreEqual(7, historyChangeSetTypes[6].ChangeTypeId);
			Assert.AreEqual("Imported", historyChangeSetTypes[6].Name);
			//- Exported 
			Assert.AreEqual(8, historyChangeSetTypes[7].ChangeTypeId);
			Assert.AreEqual("Exported", historyChangeSetTypes[7].Name);
			//- Deleted (via Parent) 
			Assert.AreEqual(9, historyChangeSetTypes[8].ChangeTypeId);
			Assert.AreEqual("Deleted (via Parent)", historyChangeSetTypes[8].Name);
			//- Added (via Parent) 
			Assert.AreEqual(10, historyChangeSetTypes[9].ChangeTypeId);
			Assert.AreEqual("Added (via Parent)", historyChangeSetTypes[9].Name);
			//- Purged (via Parent) 
			Assert.AreEqual(11, historyChangeSetTypes[10].ChangeTypeId);
			Assert.AreEqual("Purged (via Parent)", historyChangeSetTypes[10].Name);
			//- Undelete (via Parent) 
			Assert.AreEqual(12, historyChangeSetTypes[11].ChangeTypeId);
			Assert.AreEqual("Undelete (via Parent)", historyChangeSetTypes[11].Name);
			#endregion

			#region ChangeSet Data 
			//Now check the ChangeSet. 
			Assert.AreEqual(0, historyChangeSets.Count);
			//Changeset 1 
			//Assert.AreEqual(4, historyChangeSets[0].ArtifactId);
			//Assert.AreEqual(1, historyChangeSets[0].ArtifactTypeId);
			//Assert.AreEqual("Requirement", historyChangeSets[0].ArtifactTypeName);
			//Assert.IsTrue(historyChangeSets[0].ChangeDate >= DateTime.UtcNow.AddMinutes(-220000));
			//Assert.AreEqual(4, historyChangeSets[0].ChangeSetId);
			//Assert.AreEqual(1, historyChangeSets[0].ChangeTypeId);
			//Assert.AreEqual("Modified", historyChangeSets[0].ChangeTypeName);
			//Assert.AreEqual(1, historyChangeSets[0].ProjectId);
			//Assert.AreEqual(3, historyChangeSets[0].UserId);
			//Assert.AreEqual("Joe P Smith", historyChangeSets[0].UserName);
			////Changeset 2 
			//Assert.AreEqual(4, historyChangeSets[1].ArtifactId);
			//Assert.AreEqual(1, historyChangeSets[1].ArtifactTypeId);
			//Assert.AreEqual("Requirement", historyChangeSets[1].ArtifactTypeName);
			//Assert.IsTrue(historyChangeSets[1].ChangeDate >= DateTime.UtcNow.AddMinutes(-220000));
			//Assert.AreEqual(12, historyChangeSets[1].ChangeSetId);
			//Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			//Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			//Assert.AreEqual(1, historyChangeSets[1].ProjectId);
			//Assert.AreEqual(2, historyChangeSets[1].UserId);
			//Assert.AreEqual("Fred Bloggs", historyChangeSets[1].UserName);
			#endregion

			#region History Detail 
			//Verify the number. 
			Assert.AreEqual(0, historyChangeDetails.Count);
			//Record #1 
			//Assert.AreEqual(12, historyChangeDetails[0].ArtifactHistoryId);
			//Assert.AreEqual(4, historyChangeDetails[0].ArtifactId);
			//Assert.AreEqual(1, historyChangeDetails[0].ArtifactTypeId);
			//Assert.IsTrue(historyChangeDetails[0].ChangeDate >= DateTime.UtcNow.AddMinutes(-220000));
			//Assert.AreEqual(2, historyChangeDetails[0].UserId);
			//Assert.AreEqual("Fred Bloggs", historyChangeDetails[0].ChangerName);
			//Assert.AreEqual(12, historyChangeDetails[0].ChangeSetId);
			//Assert.AreEqual(1, historyChangeDetails[0].ChangeTypeId);
			//Assert.AreEqual("Modified", historyChangeDetails[0].ChangeName);
			//Assert.AreEqual(true, historyChangeDetails[0].CustomPropertyId.IsNull());
			//Assert.AreEqual("Status", historyChangeDetails[0].FieldCaption);
			//Assert.AreEqual(16, historyChangeDetails[0].FieldId);
			//Assert.AreEqual("RequirementStatusId", historyChangeDetails[0].FieldName);
			//Assert.AreEqual("Developed", historyChangeDetails[0].NewValue);
			//Assert.AreEqual(true, historyChangeDetails[0].NewValueDate.IsNull());
			//Assert.AreEqual(4, historyChangeDetails[0].NewValueInt);
			//Assert.AreEqual("In Progress", historyChangeDetails[0].OldValue);
			//Assert.AreEqual(true, historyChangeDetails[0].OldValueDate.IsNull());
			//Assert.AreEqual(3, historyChangeDetails[0].OldValueInt);
			////Record #2 
			//Assert.AreEqual(4, historyChangeDetails[1].ArtifactHistoryId);
			//Assert.AreEqual(4, historyChangeDetails[1].ArtifactId);
			//Assert.AreEqual(1, historyChangeDetails[1].ArtifactTypeId);
			//Assert.IsTrue(historyChangeDetails[1].ChangeDate >= DateTime.UtcNow.AddMinutes(-220000));
			//Assert.AreEqual(3, historyChangeDetails[1].UserId);
			//Assert.AreEqual("Joe P Smith", historyChangeDetails[1].ChangerName);
			//Assert.AreEqual(4, historyChangeDetails[1].ChangeSetId);
			//Assert.AreEqual(1, historyChangeDetails[1].ChangeTypeId);
			//Assert.AreEqual("Modified", historyChangeDetails[1].ChangeName);
			//Assert.AreEqual(true, historyChangeDetails[1].CustomPropertyId.IsNull());
			//Assert.AreEqual("Status", historyChangeDetails[1].FieldCaption);
			//Assert.AreEqual(16, historyChangeDetails[1].FieldId);
			//Assert.AreEqual("RequirementStatusId", historyChangeDetails[1].FieldName);
			//Assert.AreEqual("In Progress", historyChangeDetails[1].NewValue);
			//Assert.AreEqual(true, historyChangeDetails[1].NewValueDate.IsNull());
			//Assert.AreEqual(3, historyChangeDetails[1].NewValueInt);
			//Assert.AreEqual("Requested", historyChangeDetails[1].OldValue);
			//Assert.AreEqual(true, historyChangeDetails[1].OldValueDate.IsNull());
			//Assert.AreEqual(1, historyChangeDetails[1].OldValueInt);
			#endregion
		}

		/// <summary>This tests that we can filter and sort the artifact history</summary> 
		[Test]
		[SpiraTestCase(776)]
		public void _02_FilterAndSort()
		{
			//Lets test that we can retrieve the change history for a requirement in a sortable/filterable form 
			//Sorted by change date descending, no filter 
			List<HistoryChangeSetResponse> historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangeDate", false, null, 1, 999, InternalRoutines.UTC_OFFSET);

			//Verify items and count 
			Assert.AreEqual(0, historyChanges.Count);
			//Assert.AreEqual("RequirementStatusId", historyChanges[0].FieldName);
			////Assert.AreEqual("Status", historyChanges[0].FieldCaption);
			//Assert.AreEqual("In Progress", historyChanges[0].OldValue);
			//Assert.AreEqual("Developed", historyChanges[0].NewValue);

			//Sorted by change date descending, filtered by date-range 
			Hashtable filters = new Hashtable();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddMinutes(-209999);
			dateRange.EndDate = DateTime.UtcNow.AddMinutes(-206000);
			dateRange.ConsiderTimes = false;
			filters.Add("ChangeDate", dateRange);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangeDate", false, filters, 1, 999, InternalRoutines.UTC_OFFSET);

			//Verify items and count 
			Assert.AreEqual(0, historyChanges.Count);
			//Assert.AreEqual("RequirementStatusId", historyChanges[0].FieldName);
			////Assert.AreEqual("Status", historyChanges[0].FieldCaption);
			//Assert.AreEqual("In Progress", historyChanges[0].OldValue);
			//Assert.AreEqual("Developed", historyChanges[0].NewValue);

			//Test that we can sort by each field (no filters) 
			filters = null;
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ArtifactHistoryId", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangeDate", false, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "FieldCaption", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "FieldName", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "OldValue", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "NewValue", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangerName", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangeSetTypeName", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ChangeName", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);

			//Verify that we can filter by ChangerId and ChangeSetTypeId 
			filters = new Hashtable();
			filters.Add("ChangeSetTypeId", (int)HistoryManager.ChangeSetTypeEnum.Modified);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ArtifactHistoryId", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
			filters.Clear();
			filters.Add("ChangerId", USER_ID_JOE_SMITH);
			historyChanges = historyManager.RetrieveByArtifactId(PROJECT_ID, 4, DataModel.Artifact.ArtifactTypeEnum.Requirement, "ArtifactHistoryId", true, filters, 1, 999, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, historyChanges.Count);
		}

		#region Legacy Tests 

		/// <summary>Tests that we can update a requirement and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(140)]
		public void _04_RequirementChanges()
		{
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			List<Importance> requirementImportances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);

			//Create a new requirement. 
			int requirementId = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Developed,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId,
				"Ability to associate books with different authors",
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Lets make some changes to a requirement 
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.InProgress;
			requirementManager.Update(USER_ID_JOE_SMITH, projectId, new List<Requirement>() { requirement });

			InternalRoutines.Wait(1);

			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.Name = "Need to associate books with different authors";
			requirementManager.Update(USER_ID_JOE_SMITH, projectId, new List<Requirement>() { requirement });

			InternalRoutines.Wait(1);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.ImportanceId = requirementImportances.FirstOrDefault(r => r.Score == 3).ImportanceId;
			requirementManager.Update(USER_ID_JOE_SMITH, projectId, new List<Requirement>() { requirement });

			//Now lets retrieve the change history for the requirement 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(4, historyChanges.Count);
			//Record #1 
			Assert.AreEqual("ImportanceId", historyChanges[0].FieldName);
			Assert.AreEqual("Importance", historyChanges[0].FieldCaption);
			Assert.AreEqual("1 - Critical", historyChanges[0].OldValue);
			Assert.AreEqual(requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId, historyChanges[0].OldValueInt);
			Assert.AreEqual("3 - Medium", historyChanges[0].NewValue);
			Assert.AreEqual(requirementImportances.FirstOrDefault(r => r.Score == 3).ImportanceId, historyChanges[0].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("Name", historyChanges[1].FieldName);
			Assert.AreEqual("Requirement Name", historyChanges[1].FieldCaption);
			Assert.AreEqual("Ability to associate books with different authors", historyChanges[1].OldValue);
			Assert.AreEqual("Need to associate books with different authors", historyChanges[1].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("RequirementStatusId", historyChanges[2].FieldName);
			Assert.AreEqual("Status", historyChanges[2].FieldCaption);
			Assert.AreEqual("Developed", historyChanges[2].OldValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, historyChanges[2].OldValueInt);
			Assert.AreEqual("In Progress", historyChanges[2].NewValue);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, historyChanges[2].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);
		}

		/// <summary>Tests that we can update a test case and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(141)]
		public void _05_TestCaseChanges()
		{
			//Create a test case 
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId = testCaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				USER_ID_JOE_SMITH,
				"Ability to reassign book to different author",
				null,
				null,
				TestCase.TestCaseStatusEnum.Draft,
				null,
				null,
				null,
				null,
				null);

			//The status needs to something other than 'not run', so lets block it 
			testCaseManager.Block(USER_ID_FRED_BLOGGS, projectId, testCaseId);

			//First make sure the test case has no existing changes 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(1, historyChanges.Count);

			//Lets make some changes to a test case 
			TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			testCase.StartTracking();
			testCase.OwnerId = USER_ID_FRED_BLOGGS;
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			testCase.StartTracking();
			testCase.Name = "Reassign book to different author";
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
			testCase.StartTracking();
			testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);

			//Now lets retrieve the change history for the test case 
			historyManager.RetrieveByArtifactId(testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(4, historyChanges.Count);

			//Record #1 
			Assert.AreEqual("ExecutionStatusId", historyChanges[0].FieldName);
			Assert.AreEqual("Execution Status", historyChanges[0].FieldCaption);
			Assert.AreEqual("Blocked", historyChanges[0].OldValue);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, historyChanges[0].OldValueInt);
			Assert.AreEqual("Not Run", historyChanges[0].NewValue);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, historyChanges[0].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("Name", historyChanges[1].FieldName);
			Assert.AreEqual("Test Case Name", historyChanges[1].FieldCaption);
			Assert.AreEqual("Ability to reassign book to different author", historyChanges[1].OldValue);
			Assert.AreEqual("Reassign book to different author", historyChanges[1].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("OwnerId", historyChanges[2].FieldName);
			Assert.AreEqual("Owner", historyChanges[2].FieldCaption);
			Assert.AreEqual("Joe P Smith", historyChanges[2].OldValue);
			Assert.AreEqual(USER_ID_JOE_SMITH, historyChanges[2].OldValueInt);
			Assert.AreEqual("Fred Bloggs", historyChanges[2].NewValue);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChanges[2].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);
		}

		/// <summary>Tests that we can update an incident and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(142)]
		public void _06_IncidentChanges()
		{
			//Create the incident. 
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentStatus> statuses = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);
			int incidentId = incidentManager.Insert(projectId,
				null,
				null,
				USER_ID_JOE_SMITH,
				null,
				null,
				"Test Incident Name",
				"Test Incident Description",
				null,
				null,
				null,
				null,
				statuses.FirstOrDefault(p => p.Name == "Assigned").IncidentStatusId,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS);

			//Create a release that we'll be using 
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Library System Release 1",
				null,
				"1.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);

			//Lets make some changes to an incident 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(1, historyChangeSets.Count);    //The creation event 
			Assert.AreEqual(1, historyChanges.Count);

			Incident incident = incidentManager.RetrieveById(incidentId, false);
			incident.StartTracking();
			incident.IncidentStatusId = statuses.FirstOrDefault(p => p.Name == "Resolved").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_JOE_SMITH);

			InternalRoutines.Wait(1);
			DateTime closedDate = DateTime.UtcNow;
			incident = incidentManager.RetrieveById(incidentId, false);
			incident.StartTracking();
			incident.ClosedDate = closedDate;
			incidentManager.Update(incident, USER_ID_JOE_SMITH);

			InternalRoutines.Wait(1);
			incident = incidentManager.RetrieveById(incidentId, false);
			incident.StartTracking();
			incident.DetectedReleaseId = releaseId;
			incidentManager.Update(incident, USER_ID_JOE_SMITH);

			//Now lets retrieve the change history for the incident 
			historyManager.RetrieveByArtifactId(incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(4, historyChangeSets.Count); //1 extra for creation 
			Assert.AreEqual(4, historyChanges.Count);

			//Record #1 
			Assert.AreEqual("DetectedReleaseId", historyChanges[0].FieldName);
			Assert.AreEqual("Detected Release", historyChanges[0].FieldCaption);
			Assert.IsTrue(historyChanges[0].OldValue.IsNull());
			Assert.IsTrue(historyChanges[0].OldValueInt.IsNull());
			Assert.AreEqual("1.0.0.0 - Library System Release 1", historyChanges[0].NewValue);
			Assert.AreEqual(releaseId, historyChanges[0].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("ClosedDate", historyChanges[1].FieldName);
			Assert.AreEqual("Closed On", historyChanges[1].FieldCaption);
			Assert.IsTrue(historyChanges[1].OldValue.IsNull());
			Assert.IsTrue(historyChanges[1].OldValueDate.IsNull());
			Assert.AreEqual(closedDate.ToDatabaseSerialization(), historyChanges[1].NewValue, "Closed Date Caption");
			Assert.AreEqual(closedDate.ToString("d"), historyChanges[1].NewValueDate.Value.ToString("d"), "Closed Date Value");
			Assert.AreEqual("Joe P Smith", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("IncidentStatusId", historyChanges[2].FieldName);
			Assert.AreEqual("Status", historyChanges[2].FieldCaption);
			Assert.AreEqual("Assigned", historyChanges[2].OldValue);
			Assert.AreEqual(statuses.FirstOrDefault(p => p.Name == "Assigned").IncidentStatusId, historyChanges[2].OldValueInt);
			Assert.AreEqual("Resolved", historyChanges[2].NewValue);
			Assert.AreEqual(statuses.FirstOrDefault(p => p.Name == "Resolved").IncidentStatusId, historyChanges[2].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);

			//Delete the release 
			releaseManager.DeleteFromDatabase(releaseId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>Tests that we can update a release and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(143)]
		public void _07_ReleaseChanges()
		{
			//Lets create the release 
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int releaseId = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_JOE_SMITH,
				"Library System Release 1 SP1",
				null,
				"1.0.1.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);

			//Lets make some changes to a release 
			Release release = releaseManager.RetrieveById3(projectId, releaseId);
			release.StartTracking();
			release.VersionNumber = "1.0.1";
			releaseManager.Update(new List<Release>() { release }, USER_ID_JOE_SMITH, projectId);
			InternalRoutines.Wait(1);
			release = releaseManager.RetrieveById3(projectId, releaseId);
			release.StartTracking();
			release.Name = "Library System Release 1.0.1";
			releaseManager.Update(new List<Release>() { release }, USER_ID_JOE_SMITH, projectId);
			InternalRoutines.Wait(1);
			release = releaseManager.RetrieveById3(projectId, releaseId);
			release.StartTracking();
			release.CreatorId = USER_ID_FRED_BLOGGS;
			releaseManager.Update(new List<Release>() { release }, USER_ID_JOE_SMITH, projectId);

			//Now lets retrieve the change history for the release 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(4, historyChanges.Count);
			//Record #1 
			Assert.AreEqual("CreatorId", historyChanges[0].FieldName);
			Assert.AreEqual("Creator", historyChanges[0].FieldCaption);
			Assert.AreEqual("Joe P Smith", historyChanges[0].OldValue);
			Assert.AreEqual(USER_ID_JOE_SMITH, historyChanges[0].OldValueInt);
			Assert.AreEqual("Fred Bloggs", historyChanges[0].NewValue);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, historyChanges[0].NewValueInt);
			Assert.AreEqual("Joe P Smith", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("Name", historyChanges[1].FieldName);
			Assert.AreEqual("Release Name", historyChanges[1].FieldCaption);
			Assert.AreEqual("Library System Release 1 SP1", historyChanges[1].OldValue);
			Assert.AreEqual("Library System Release 1.0.1", historyChanges[1].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("VersionNumber", historyChanges[2].FieldName);
			Assert.AreEqual("Version #", historyChanges[2].FieldCaption);
			Assert.AreEqual("1.0.1.0", historyChanges[2].OldValue);
			Assert.AreEqual("1.0.1", historyChanges[2].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);
		}

		/// <summary>Tests that we can update a test step and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(336)]
		public void _08_TestStepChanges()
		{
			TestCaseManager testCaseManager = new TestCaseManager();

			//Lets create a new test case with a test step 
			int testCaseId = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, null, "Test Case 123", null, null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testStepId = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId, 1, "Do Something", "Works", null);

			//Lets add some data to the test step 
			TestStep testStep = testCaseManager.RetrieveStepById(projectId, testStepId);
			if (testStep == null)
			{
				Assert.Fail("Test Step Not Found!");
			}
			testStep.StartTracking();
			testStep.Description = "User does something";
			testStep.ExpectedResult = "User taken to first screen in wizard";
			testStep.SampleData = "Some data";
			testCaseManager.UpdateStep(testStep, USER_ID_JOE_SMITH);

			//Now, lets make some changes to the test step 
			TestCase testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			testCase.TestSteps[0].StartTracking();
			testCase.TestSteps[0].Description = "User clicks link to create book";
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			testCase.TestSteps[0].StartTracking();
			testCase.TestSteps[0].ExpectedResult = "User arrives at some page";
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testCase = testCaseManager.RetrieveByIdWithSteps(projectId, testCaseId);
			testCase.TestSteps[0].StartTracking();
			testCase.TestSteps[0].SampleData = null;
			testCaseManager.Update(testCase, USER_ID_JOE_SMITH);

			//Now lets retrieve the change history for the test step 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(7, historyChanges.Count);

			//Record #1 
			Assert.AreEqual("SampleData", historyChanges[0].FieldName);
			Assert.AreEqual("Sample Data", historyChanges[0].FieldCaption);
			Assert.AreEqual("Some data", historyChanges[0].OldValue);
			Assert.IsTrue(historyChanges[0].NewValue.IsNull());
			Assert.AreEqual("Joe P Smith", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("ExpectedResult", historyChanges[1].FieldName);
			Assert.AreEqual("Expected Result", historyChanges[1].FieldCaption);
			Assert.AreEqual("User taken to first screen in wizard", historyChanges[1].OldValue);
			Assert.AreEqual("User arrives at some page", historyChanges[1].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("Description", historyChanges[2].FieldName);
			Assert.AreEqual("Description", historyChanges[2].FieldCaption);
			Assert.AreEqual("User does something", historyChanges[2].OldValue);
			Assert.AreEqual("User clicks link to create book", historyChanges[2].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);

			//Records #4-6 
			Assert.AreEqual("Description", historyChanges[3].FieldName);
			Assert.AreEqual("Do Something", historyChanges[3].OldValue);
			Assert.AreEqual("User does something", historyChanges[3].NewValue);
			Assert.AreEqual("ExpectedResult", historyChanges[4].FieldName);
			Assert.AreEqual("Works", historyChanges[4].OldValue);
			Assert.AreEqual("User taken to first screen in wizard", historyChanges[4].NewValue);
			Assert.AreEqual("SampleData", historyChanges[5].FieldName);
			Assert.IsTrue(historyChanges[5].OldValue.IsNull());
			Assert.AreEqual("Some data", historyChanges[5].NewValue);

			//Clean up 
			testCaseManager.DeleteFromDatabase(testCaseId, USER_ID_FRED_BLOGGS);
			historyManager.DeleteChangeSets(Artifact.ArtifactTypeEnum.TestStep, testStepId);
		}

		/// <summary>Tests that we can update a test set and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(348)]
		public void _09_TestSetChanges()
		{
			//Create a release that we'll be using 
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Library System Release 1.1",
				null,
				"1.1.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);

			//Next create a new test set 
			TestSetManager testSetManager = new TestSetManager();
			int testSetId = testSetManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				releaseId,
				USER_ID_FRED_BLOGGS,
				null,
				TestSet.TestSetStatusEnum.NotStarted,
				"Testing Cycle for Release 1.1",
				"This tests the functionality introduced in release 2.0 of the library system",
				DateTime.Parse("2/7/2007"),
				TestRun.TestRunTypeEnum.Manual,
				1,
				null
				);

			//Lets make some changes to a test set 
			TestSet testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.Name = "Testing Cycle for Version 1.1.0.0";
			testSetManager.Update(testSet, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.Description = "This tests the functionality introduced in release 1.1 of the library system";
			testSetManager.Update(testSet, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.InProgress;
			testSetManager.Update(testSet, USER_ID_JOE_SMITH);
			InternalRoutines.Wait(1);
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.ReleaseId = null;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);
			InternalRoutines.Wait(1);
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.PlannedDate = null;
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);

			//Now lets retrieve the change history for the test set 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Make sure we have the number of records and values we expect 
			Assert.AreEqual(6, historyChangeSets.Count);    //1 extra for the creation entry 
			Assert.AreEqual(6, historyChanges.Count);

			//Record #1 
			Assert.AreEqual("PlannedDate", historyChanges[0].FieldName);
			Assert.AreEqual("Planned Date", historyChanges[0].FieldCaption);
			Assert.AreEqual(DateTime.Parse("2/7/2007").ToDatabaseSerialization(), historyChanges[0].OldValue);
			Assert.IsTrue(historyChanges[0].NewValue.IsNull());
			Assert.AreEqual("Fred Bloggs", historyChanges[0].ChangerName);
			//Record #2 
			Assert.AreEqual("ReleaseId", historyChanges[1].FieldName);
			Assert.AreEqual("Release", historyChanges[1].FieldCaption);
			Assert.AreEqual("1.1.0.0 - Library System Release 1.1", historyChanges[1].OldValue);
			Assert.IsTrue(historyChanges[1].NewValue.IsNull());
			Assert.AreEqual("Fred Bloggs", historyChanges[1].ChangerName);
			//Record #3 
			Assert.AreEqual("TestSetStatusId", historyChanges[2].FieldName);
			Assert.AreEqual("Status", historyChanges[2].FieldCaption);
			Assert.AreEqual("Not Started", historyChanges[2].OldValue);
			Assert.AreEqual("In Progress", historyChanges[2].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[2].ChangerName);
			//Record #4 
			Assert.AreEqual("Description", historyChanges[3].FieldName);
			Assert.AreEqual("Description", historyChanges[3].FieldCaption);
			Assert.AreEqual("This tests the functionality introduced in release 2.0 of the library system", historyChanges[3].OldValue);
			Assert.AreEqual("This tests the functionality introduced in release 1.1 of the library system", historyChanges[3].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[3].ChangerName);
			//Record #5 
			Assert.AreEqual("Name", historyChanges[4].FieldName);
			Assert.AreEqual("Test Set Name", historyChanges[4].FieldCaption);
			Assert.AreEqual("Testing Cycle for Release 1.1", historyChanges[4].OldValue);
			Assert.AreEqual("Testing Cycle for Version 1.1.0.0", historyChanges[4].NewValue);
			Assert.AreEqual("Joe P Smith", historyChanges[4].ChangerName);

			//Finally undo the changes to the test set (we'll delete the history in the CleanUp function) 
			testSet = testSetManager.RetrieveById2(projectId, testSetId);
			testSet.StartTracking();
			testSet.Name = "Testing Cycle for Release 1.1";
			testSet.Description = "This tests the functionality introduced in release 2.0 of the library system";
			testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
			testSet.ReleaseId = 4;
			testSet.PlannedDate = DateTime.Parse("2/7/2007");
			testSetManager.Update(testSet, USER_ID_FRED_BLOGGS);
		}

		#endregion

		/// <summary>This tests creating, modifying, deleting an incident.</summary> 
		[Test]
		[SpiraTestCase(765)]
		public void _03_CycleIncident()
		{
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentPriority> priorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true);
			List<IncidentSeverity> severities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, true);
			List<IncidentType> types = incidentManager.RetrieveIncidentTypes(projectTemplateId, true);
			List<IncidentStatus> statuses = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);
			//Notes: 
			// - Created by the Admin, Opener was Roger, Owner is Fred. 
			// - Updated by Administrator. 
			// - Deleted by Fred.  
			// - Purged by Roger. 

			//Create the incident with the following info: 
			int? priorityId = priorities.FirstOrDefault(p => p.Name == "2 - High").PriorityId;
			int? severityId = severities.FirstOrDefault(p => p.Name == "4 - Low").SeverityId;
			int openerId = 4; //Roger Q Ramjet 
			int? ownerId = 2; //Fred Bloggs 
			string name = "HistoryTest Incident";
			string description = "This is a test incident created from the HistoryTest NUnit test. _02_CycleIncident()";
			int? incidentTypeId = types.FirstOrDefault(p => p.Name == "Limitation").IncidentTypeId; //Limitation 
			int? incidentStatusId = statuses.FirstOrDefault(p => p.Name == "Not Reproducible").IncidentStatusId; //Not Reproducible 
			DateTime creationDate = DateTime.Parse("1/1/2011 09:00 AM");
			DateTime? startDate = DateTime.Parse("1/1/2011 12:34 PM");
			DateTime? endDate = DateTime.Parse("3/31/2011 11:54 AM");
			int? estimatedEffort_Before = 450;
			int? estimatedEffort_After = 590;
			int creatorId = 1; //Administrator 

			//Create the incident. 
			int incId = incidentManager.Insert(projectId,
				priorityId,
				severityId,
				openerId,
				ownerId,
				null,
				name,
				description,
				null,
				null,
				null,
				incidentTypeId,
				incidentStatusId,
				creationDate,
				startDate,
				endDate,
				estimatedEffort_Before,
				null,
				null,
				null,
				null,
				creatorId);
			Assert.IsTrue(incId > 0);

			//Pull incident, make a change. 
			Incident incidentUpdated = incidentManager.RetrieveById(incId, false);
			Incident incident = incidentUpdated.Clone(); //The pre-change incident. 
			incidentUpdated.StartTracking();
			incidentUpdated.EstimatedEffort = estimatedEffort_After.Value;
			Assert.AreEqual(estimatedEffort_After.Value, incidentUpdated.EstimatedEffort);
			incidentManager.Update(incidentUpdated, 1);

			//Re-pull incident, verify that the change was made properly (and that the ModifiedDate is current.) 
			incidentUpdated = incidentManager.RetrieveById(incId, false);
			Assert.AreEqual(estimatedEffort_After.Value, incidentUpdated.EstimatedEffort);

			//Delete the incident now. 
			incidentManager.MarkAsDeleted(incidentUpdated.ProjectId, incidentUpdated.IncidentId, 2);

			//Try pulling the incident, Should not get it. 
			bool exists = true;
			try
			{
				incidentUpdated = incidentManager.RetrieveById(incId, false);
				exists = (incident != null);
			}
			catch (ArtifactNotExistsException)
			{
				exists = false;
			}
			Assert.IsFalse(exists);

			//Now pull the history data for that Incident 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(incId, Artifact.ArtifactTypeEnum.Incident, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(5, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(incId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(3, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Incident", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(1, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(incId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(3, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Incident", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(incId, historyChangeSets[4].ArtifactId);
			Assert.AreEqual(3, historyChangeSets[4].ArtifactTypeId);
			Assert.AreEqual("Incident", historyChangeSets[4].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[4].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[4].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[4].ProjectId);
			Assert.AreEqual(2, historyChangeSets[4].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[4].UserName);

			#endregion

			#region History Items 

			Assert.AreEqual(5, historyChanges.Count);
			//#1 
			HistoryChangeView artifactHistoryRow = historyChanges[1];
			Assert.AreEqual(incId, artifactHistoryRow.ArtifactId);
			Assert.AreEqual(3, artifactHistoryRow.ArtifactTypeId);
			Assert.AreEqual(1, artifactHistoryRow.UserId);
			Assert.AreEqual("System Administrator", artifactHistoryRow.ChangerName);
			Assert.AreEqual(1, artifactHistoryRow.ChangeTypeId);
			Assert.AreEqual("Modified", artifactHistoryRow.ChangeName);
			Assert.AreEqual(true, artifactHistoryRow.CustomPropertyId.IsNull());
			Assert.AreEqual("Est. Effort", artifactHistoryRow.FieldCaption);
			Assert.AreEqual(47, artifactHistoryRow.FieldId);
			Assert.AreEqual("EstimatedEffort", artifactHistoryRow.FieldName);
			Assert.AreEqual(590.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE) + " mins", artifactHistoryRow.NewValue);
			Assert.AreEqual(true, artifactHistoryRow.NewValueDate.IsNull());
			Assert.AreEqual(590, artifactHistoryRow.NewValueInt);
			Assert.AreEqual(450.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE) + " mins", artifactHistoryRow.OldValue);
			Assert.AreEqual(true, artifactHistoryRow.OldValueDate.IsNull());
			Assert.AreEqual(450, artifactHistoryRow.OldValueInt);
			//#2 
			artifactHistoryRow = historyChanges[2];
			Assert.AreEqual(incId, artifactHistoryRow.ArtifactId);
			Assert.AreEqual(3, artifactHistoryRow.ArtifactTypeId);
			Assert.AreEqual(1, artifactHistoryRow.UserId);
			Assert.AreEqual("System Administrator", artifactHistoryRow.ChangerName);
			Assert.AreEqual(1, artifactHistoryRow.ChangeTypeId);
			Assert.AreEqual("Modified", artifactHistoryRow.ChangeName);
			Assert.AreEqual(true, artifactHistoryRow.CustomPropertyId.IsNull());
			Assert.AreEqual("% Complete", artifactHistoryRow.FieldCaption);
			Assert.AreEqual(46, artifactHistoryRow.FieldId);
			Assert.AreEqual("CompletionPercent", artifactHistoryRow.FieldName);
			Assert.AreEqual("0000000023", artifactHistoryRow.NewValue);
			Assert.AreEqual(true, artifactHistoryRow.NewValueDate.IsNull());
			Assert.AreEqual(23, artifactHistoryRow.NewValueInt);
			Assert.AreEqual(0.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), artifactHistoryRow.OldValue);
			Assert.AreEqual(true, artifactHistoryRow.OldValueDate.IsNull());
			Assert.AreEqual(0, artifactHistoryRow.OldValueInt);
			//#3 
			artifactHistoryRow = historyChanges[3];
			Assert.AreEqual(incId, artifactHistoryRow.ArtifactId);
			Assert.AreEqual(3, artifactHistoryRow.ArtifactTypeId);
			Assert.AreEqual(1, artifactHistoryRow.UserId);
			Assert.AreEqual("System Administrator", artifactHistoryRow.ChangerName);
			Assert.AreEqual(1, artifactHistoryRow.ChangeTypeId);
			Assert.AreEqual("Modified", artifactHistoryRow.ChangeName);
			Assert.AreEqual(true, artifactHistoryRow.CustomPropertyId.IsNull());
			Assert.AreEqual("Projected Effort", artifactHistoryRow.FieldCaption);
			Assert.AreEqual(126, artifactHistoryRow.FieldId);
			Assert.AreEqual("ProjectedEffort", artifactHistoryRow.FieldName);
			Assert.AreEqual(590.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), artifactHistoryRow.NewValue);
			Assert.AreEqual(true, artifactHistoryRow.NewValueDate.IsNull());
			Assert.AreEqual(590, artifactHistoryRow.NewValueInt);
			Assert.AreEqual(450.ToString(DatabaseExtensions.FORMAT_INTEGER_SORTABLE), artifactHistoryRow.OldValue);
			Assert.AreEqual(true, artifactHistoryRow.OldValueDate.IsNull());
			Assert.AreEqual(450, artifactHistoryRow.OldValueInt);
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Incident, incId, historyChanges[0].ChangeSetId, 1, ref rollLog);

			//...And pull the incident, make sure that it's fields match up with what we expect. 
			Incident incidentRestored = incidentManager.RetrieveById(incId, false, false);

			//Loop, lookin' at each column 
			foreach (KeyValuePair<string, PropertyInfo> kvp in incident.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = incident[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = incidentRestored[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue.ToSafeString() + "\" // \"" + objNewValue.ToSafeString() + "\"");
				if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			incidentManager.DeleteFromDatabase(incId, openerId);

			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(incId, Artifact.ArtifactTypeEnum.Incident, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(7, historyChanges.Count);
			Assert.AreEqual(7, historyChangeSets.Count);
			Assert.AreEqual(incId, historyChangeSets[6].ArtifactId);
			Assert.AreEqual(3, historyChangeSets[6].ArtifactTypeId);
			Assert.AreEqual("Incident", historyChangeSets[6].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[6].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[6].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[6].ProjectId);
			Assert.AreEqual(4, historyChangeSets[6].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[6].UserName);
		}

		/// <summary>This tests creating, modifying, deleting a requirement</summary> 
		[Test]
		[SpiraTestCase(770)]
		public void _10_CycleRequirement()
		{
			//Notes on users: 
			// 1. On creation: Owner=Fred Bloggs; Author=Roger; HistoryOpener=Roger 
			// 2. Updated: Author=Roger --> Author=Fred Bloggs; HistoryUpdater: Administrator 
			// 3. Deleted: HistoryUpdated: Fred Bloggs. 

			RequirementManager requirementManager = new RequirementManager();
			List<Importance> requirementImportances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);

			//Create the requirement with the following info: 
			Requirement.RequirementStatusEnum status = Requirement.RequirementStatusEnum.Accepted;
			int? importanceId = requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId;
			int? typeId = null;
			int openerId = 4; //Roger Q Ramjet 
			int? ownerId = 2; //Fred Bloggs 
			string name = "HistoryTest Requirement";
			string description = "This is a test requirement created from the HistoryTest NUnit test. _10_CycleRequirement()";
			int? plannedEffort_Before = 450;
			int creatorId = 1; //Administrator 

			//Create the requirement. 
			int reqId = requirementManager.Insert(
				openerId,
				projectId,
				null,
				null,
				(int?)null,
				status,
				typeId,
				openerId,
				ownerId,
				importanceId,
				name,
				description,
				plannedEffort_Before,
				creatorId
				);
			Assert.IsTrue(reqId > 0);

			//Pull the base requirement and copy it so we can update it. 
			RequirementView requirementView = requirementManager.RetrieveById(User.UserInternal, null, reqId, true);
			Requirement updatedRequirement = requirementView.ConvertTo<RequirementView, Requirement>();
			updatedRequirement.StartTracking();

			//Let's change a few items: 
			//  1 - Author: Roger Q. Ramjet => Fred Bloggs 
			//  2 - Importance: Critical (1) => Medium (3) 
			updatedRequirement.AuthorId = ownerId.Value;
			updatedRequirement.ImportanceId = requirementImportances.FirstOrDefault(r => r.Score == 3).ImportanceId;
			new RequirementManager().Update(1, projectId, new List<Requirement>() { updatedRequirement });

			//Re-pull incident, verify that the change was made properly (and that the ModifiedDate is current.) 
			RequirementView requirementView2 = requirementManager.RetrieveById(User.UserInternal, null, reqId, true);
			Assert.AreEqual(ownerId.Value, requirementView2.AuthorId);
			Assert.AreEqual(requirementImportances.FirstOrDefault(r => r.Score == 3).ImportanceId, requirementView2.ImportanceId);

			//Delete the requirement now. 
			requirementManager.MarkAsDeleted(2, projectId, reqId);

			//Try pulling the requirement, Should not get it. 
			bool exceptionThrown = false;
			try
			{
				requirementManager.RetrieveById(User.UserInternal, null, reqId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Now pull the history data for that requirement. 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(reqId, Artifact.ArtifactTypeEnum.Requirement, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(4, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(reqId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(1, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Requirement", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(1, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(reqId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(1, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Requirement", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(reqId, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(1, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Requirement", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[3].ProjectId);
			Assert.AreEqual(2, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion
			#region History Items 
			Assert.AreEqual(4, historyChanges.Count);
			//#1 
			Assert.AreEqual(reqId, historyChanges[1].ArtifactId);
			Assert.AreEqual(1, historyChanges[1].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[1].UserId);
			Assert.AreEqual("System Administrator", historyChanges[1].ChangerName);
			Assert.AreEqual(1, historyChanges[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[1].ChangeName);
			Assert.AreEqual(true, historyChanges[1].CustomPropertyId.IsNull());
			Assert.AreEqual("Author", historyChanges[1].FieldCaption);
			Assert.AreEqual(17, historyChanges[1].FieldId);
			Assert.AreEqual("AuthorId", historyChanges[1].FieldName);
			Assert.AreEqual("Fred Bloggs", historyChanges[1].NewValue);
			Assert.AreEqual(true, historyChanges[1].NewValueDate.IsNull());
			Assert.AreEqual(2, historyChanges[1].NewValueInt);
			Assert.AreEqual("Roger Q Ramjet", historyChanges[1].OldValue);
			Assert.AreEqual(true, historyChanges[1].OldValueDate.IsNull());
			Assert.AreEqual(4, historyChanges[1].OldValueInt);
			//#2 
			Assert.AreEqual(reqId, historyChanges[2].ArtifactId);
			Assert.AreEqual(1, historyChanges[2].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[2].UserId);
			Assert.AreEqual("System Administrator", historyChanges[2].ChangerName);
			Assert.AreEqual(1, historyChanges[2].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[2].ChangeName);
			Assert.AreEqual(true, historyChanges[2].CustomPropertyId.IsNull());
			Assert.AreEqual("Importance", historyChanges[2].FieldCaption);
			Assert.AreEqual(18, historyChanges[2].FieldId);
			Assert.AreEqual("ImportanceId", historyChanges[2].FieldName);
			Assert.AreEqual("3 - Medium", historyChanges[2].NewValue);
			Assert.AreEqual(true, historyChanges[2].NewValueDate.IsNull());
			Assert.AreEqual(requirementImportances.FirstOrDefault(r => r.Score == 3).ImportanceId, historyChanges[2].NewValueInt);
			Assert.AreEqual("1 - Critical", historyChanges[2].OldValue);
			Assert.AreEqual(true, historyChanges[2].OldValueDate.IsNull());
			Assert.AreEqual(requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId, historyChanges[2].OldValueInt);
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, reqId, historyChanges[0].ChangeSetId, 1, ref rollLog);
			System.Diagnostics.Trace.Write(rollLog);

			//...And pull the requirement, make sure that it's fields match up with what we expect. 
			RequirementView restoredRequirementView = requirementManager.RetrieveById(User.UserInternal, null, reqId);

			//Loop, lookin' at each property 
			foreach (KeyValuePair<string, PropertyInfo> kvp in requirementView.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = requirementView[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = restoredRequirementView[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue.ToSafeString() + "\" // \"" + objNewValue.ToSafeString() + "\"");
				if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			requirementManager.DeleteFromDatabase(reqId, openerId);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(reqId, Artifact.ArtifactTypeEnum.Requirement, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(6, historyChanges.Count);
			Assert.AreEqual(6, historyChangeSets.Count);
			Assert.AreEqual(reqId, historyChangeSets[5].ArtifactId);
			Assert.AreEqual(1, historyChangeSets[5].ArtifactTypeId);
			Assert.AreEqual("Requirement", historyChangeSets[5].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[5].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[5].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[5].ProjectId);
			Assert.AreEqual(4, historyChangeSets[5].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[5].UserName);
		}

		/// <summary>This tests creating, modifying, deleting a test case and test steps</summary> 
		[Test]
		[SpiraTestCase(771)]
		public void _11_CycleTestCaseAndTestSteps()
		{
			//Get priorities 
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, priorities.Count);

			//Test Case and general data. 
			int userCreator = 1;
			int userAuthor = 2;
			int userOwner = 3;
			string name = "HistoryTest Test Case";
			string description = "This is a test Test Case created from the HistoryTest NUnit test. _11_CycleTestCaseAndTestSteps()";
			int tcPriorityId = priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId;
			Assert.IsTrue(tcPriorityId > 0);

			//Test Step 1 data 
			string step1desc = "This is a test Test Step #1 created from the HistoryTest NUnit test. _11_CycleTestCaseAndTestSteps()";
			string step1exp = "Expected result 1";
			string step1smp = "Sample Data 1";
			//Test Step 2 data 
			string step2desc = "This is a test Test Step #2 created from the HistoryTest NUnit test. _11_CycleTestCaseAndTestSteps()";
			string step2exp = "Expected result 2";
			string step2smp = "Sample Data 2";
			//Test Step 2 data 
			string step3desc = "This is a test Test Step #3 created from the HistoryTest NUnit test. _11_CycleTestCaseAndTestSteps()";
			string step3exp = "Expected result 3";
			string step3smp = "Sample Data 3";

			//Create the test case. 
			int tcId = testCaseManager.Insert(
				userCreator,
				projectId,
				userAuthor,
				userOwner,
				name,
				description,
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				tcPriorityId,
				null,
				150,
				null,
				null);

			//Add some steps.. 
			int ts1id = testCaseManager.InsertStep(
				userCreator,
				tcId,
				null,
				step1desc,
				step1exp,
				step1smp
				);
			int ts2id = testCaseManager.InsertStep(
				userCreator,
				tcId,
				null,
				step2desc,
				step2exp,
				step2smp
				);
			int ts3id = testCaseManager.InsertStep(
				userCreator,
				tcId,
				null,
				step3desc,
				step3exp,
				step3smp
				);

			//Now re-pull the test case, and make a copy for modifying it. 
			TestCase tcOrig = testCaseManager.RetrieveByIdWithSteps(null, tcId);
			TestCase tcUpd = testCaseManager.RetrieveByIdWithSteps(null, tcId);

			#region Test Step Checks 
			//First, let's play with our test steps: 
			//We'll delete step 3, and change step 2's sample data. 
			tcUpd.TestSteps[1].StartTracking();
			tcUpd.TestSteps[1].SampleData = step3smp;
			testCaseManager.MarkStepAsDeleted(userAuthor, tcId, ts3id);
			testCaseManager.Update(tcUpd, userCreator, null);

			//Now pull history and verify what we have for Step 3 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(ts3id, Artifact.ArtifactTypeEnum.TestStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual(2, historyChanges.Count);
			// - Main addition changeset. 
			Assert.AreEqual(ts3id, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(userCreator, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			// - Main deletion changeset. 
			Assert.AreEqual(ts3id, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(userAuthor, historyChangeSets[1].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[1].UserName);

			//Now pull history and verify what we have for Step 2. 
			historyManager.RetrieveByArtifactId(ts2id, Artifact.ArtifactTypeEnum.TestStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(2, historyChangeSets.Count);
			Assert.AreEqual(2, historyChanges.Count);
			// - Main addition changeset. 
			Assert.AreEqual(ts2id, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(userCreator, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			// - Main modified changeset. 
			Assert.AreEqual(ts2id, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(userCreator, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			// - The changed field. 
			Assert.AreEqual(ts2id, historyChanges[0].ArtifactId);
			Assert.AreEqual(7, historyChanges[0].ArtifactTypeId);
			Assert.AreEqual(userCreator, historyChanges[0].UserId);
			Assert.AreEqual("System Administrator", historyChanges[0].ChangerName);
			Assert.AreEqual(historyChangeSets[1].ChangeSetId, historyChanges[0].ChangeSetId);
			Assert.AreEqual(1, historyChanges[0].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[0].ChangeName);
			Assert.AreEqual(true, historyChanges[0].CustomPropertyId.IsNull());
			Assert.AreEqual(step3smp, historyChanges[0].NewValue);
			Assert.AreEqual(true, historyChanges[0].NewValueInt.IsNull());
			Assert.AreEqual(true, historyChanges[0].NewValueDate.IsNull());
			Assert.AreEqual(step2smp, historyChanges[0].OldValue);
			Assert.AreEqual(true, historyChanges[0].OldValueInt.IsNull());
			Assert.AreEqual(true, historyChanges[0].OldValueDate.IsNull());
			Assert.AreEqual("Sample Data", historyChanges[0].FieldCaption);
			Assert.AreEqual(102, historyChanges[0].FieldId);
			Assert.AreEqual("SampleData", historyChanges[0].FieldName);

			//Let's restore test step 3. (Undelete it.) 
			string logRollback = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, ts3id, historyChangeSets[0].ChangeSetId, userAuthor, ref logRollback);
			System.Diagnostics.Trace.Write(logRollback);

			//Check that step 3 is undeleted. 
			long rollbackId = historyChangeSets[0].ChangeSetId;
			TestCase tcStep3Test = testCaseManager.RetrieveByIdWithSteps(null, tcId);
			Assert.AreEqual(3, tcStep3Test.TestSteps.Count);
			//Check rollback history. 
			historyManager.RetrieveByArtifactId(ts3id, Artifact.ArtifactTypeEnum.TestStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual(3, historyChanges.Count);
			// - Check the rollback entry only. (We checked the others previously. 
			Assert.AreEqual(ts3id, historyChangeSets[2].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[2].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[2].ArtifactTypeName);
			Assert.AreEqual(6, historyChangeSets[2].ChangeTypeId);
			Assert.AreEqual("Undelete", historyChangeSets[2].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[2].ProjectId);
			//Assert.AreEqual(rollbackId, historyChangeSets[2].RevertId); //This is the second changeset, because the first is the 'added', and 'added's can't be undone. 
			Assert.AreEqual(userAuthor, historyChangeSets[2].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[2].UserName);

			//All is good. Purge and check Test Step 3. 
			testCaseManager.DeleteStepFromDatabase(userAuthor, ts3id);
			historyManager.RetrieveByArtifactId(ts3id, Artifact.ArtifactTypeEnum.TestStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(4, historyChangeSets.Count);
			Assert.AreEqual(4, historyChanges.Count);
			// - Check main purge. 
			Assert.AreEqual(ts3id, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(7, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Test Step", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[3].ProjectId);
			Assert.AreEqual(userAuthor, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion

			#region Test Case Checks 
			//Okay, now it's time to play with a test case. 
			//Let's update the test case. 
			tcUpd = testCaseManager.RetrieveByIdWithSteps(null, tcId);
			tcUpd.StartTracking();
			tcUpd.EstimatedDuration = 300;
			tcUpd.OwnerId = userAuthor;
			testCaseManager.Update(tcUpd, userAuthor);

			//Now check history. 
			historyManager.RetrieveByArtifactId(tcId, Artifact.ArtifactTypeEnum.TestCase, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(3, historyChangeSets.Count);
			Assert.AreEqual(3, historyChanges.Count);

			//Check the history entries. 
			// - Main addition changeset. 
			Assert.AreEqual(tcId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(2, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Test Case", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(userCreator, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			// - Main modification changeset. 
			Assert.AreEqual(tcId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(2, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Test Case", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(2, historyChangeSets[1].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[1].UserName);
			// - Details for the modification, field 2. 
			HistoryChangeView historyChange = historyChanges[1];
			Assert.AreEqual(tcId, historyChange.ArtifactId);
			Assert.AreEqual(2, historyChange.ArtifactTypeId);
			Assert.AreEqual(2, historyChange.UserId);
			Assert.AreEqual("Fred Bloggs", historyChange.ChangerName);
			Assert.AreEqual(historyChangeSets[1].ChangeSetId, historyChange.ChangeSetId);
			Assert.AreEqual(1, historyChange.ChangeTypeId);
			Assert.AreEqual("Modified", historyChange.ChangeName);
			Assert.AreEqual(true, historyChange.CustomPropertyId.IsNull());
			Assert.AreEqual("Owner", historyChange.FieldCaption);
			Assert.AreEqual(23, historyChange.FieldId);
			Assert.AreEqual("OwnerId", historyChange.FieldName);
			Assert.AreEqual("Fred Bloggs", historyChange.NewValue);
			Assert.AreEqual(2, historyChange.NewValueInt);
			Assert.AreEqual(true, historyChange.NewValueDate.IsNull());
			Assert.AreEqual("Joe P Smith", historyChange.OldValue);
			Assert.AreEqual(3, historyChange.OldValueInt);
			Assert.AreEqual(true, historyChange.OldValueDate.IsNull());
			// - Details for the modification, field 1. 
			historyChange = historyChanges[0];
			Assert.AreEqual(tcId, historyChange.ArtifactId);
			Assert.AreEqual(2, historyChange.ArtifactTypeId);
			Assert.AreEqual(2, historyChange.UserId);
			Assert.AreEqual("Fred Bloggs", historyChange.ChangerName);
			Assert.AreEqual(historyChangeSets[1].ChangeSetId, historyChange.ChangeSetId);
			Assert.AreEqual(1, historyChange.ChangeTypeId);
			Assert.AreEqual("Modified", historyChange.ChangeName);
			Assert.AreEqual(true, historyChange.CustomPropertyId.IsNull());
			Assert.AreEqual("Est. Dur.", historyChange.FieldCaption);
			Assert.AreEqual(28, historyChange.FieldId);
			Assert.AreEqual("EstimatedDuration", historyChange.FieldName);
			Assert.AreEqual("0000000300 mins", historyChange.NewValue);
			Assert.AreEqual(300, historyChange.NewValueInt);
			Assert.AreEqual(true, historyChange.NewValueDate.IsNull());
			Assert.AreEqual("0000000150 mins", historyChange.OldValue);
			Assert.AreEqual(150, historyChange.OldValueInt);
			Assert.AreEqual(true, historyChange.OldValueDate.IsNull());

			//Okay, now let's delete the test case and verify we can't retrieve it. 
			testCaseManager.MarkAsDeleted(userAuthor, projectId, tcId);
			//Try pulling the test case, Should not get it. 
			bool exceptionThrown = false;
			try
			{
				TestCase dsTestCase = testCaseManager.RetrieveByIdWithSteps(null, tcId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Okay, now roll the whole thing back. commented out for getting exception and stopped the application
			//string rollLog = "";
			//historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, tcId, historyChangeSets[0].ChangeSetId, 2, ref rollLog);
			//System.Diagnostics.Trace.Write(rollLog + Environment.NewLine);

			////...And pull the test case, make sure that it's fields match up with what we expect. 
			//TestCase dsTCRestore = testCaseManager.RetrieveByIdWithSteps(null, tcId);
			//TestCase rowOrig = tcOrig;
			//TestCase rowCheck = dsTCRestore;

			////Loop, lookin' at each column 
			//foreach (KeyValuePair<string, PropertyInfo> kvp in tcOrig.Properties)
			//{
			//	string fieldName = kvp.Key;

			//	object objOriginalValue;
			//	object objNewValue;

			//	//Get the original column. 
			//	try
			//	{
			//		objOriginalValue = rowOrig[fieldName];
			//	}
			//	catch (Exception ex)
			//	{
			//		//If we threw an error, the exception type gets put into the value. 
			//		objOriginalValue = ex.GetType().ToString();
			//	}

			//	//Get the restored column. 
			//	try
			//	{
			//		objNewValue = rowCheck[fieldName];
			//	}
			//	catch (Exception ex)
			//	{
			//		//If we threw an error, the exception type gets put into the value. 
			//		objNewValue = ex.GetType().ToString();
			//	}

			//	//Now do the comparison. 
			//	System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue + "\" // \"" + objNewValue + "\"");
			//	if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker"
			//		&& fieldName != "ExecutionStatus" && fieldName != "TestSteps" && fieldName != "Status" && fieldName != "Type")
			//	{
			//		//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
			//	}
			//}
			#endregion

			//Okay, now purge the item. 
			testCaseManager.DeleteFromDatabase(tcId, 3);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(tcId, Artifact.ArtifactTypeEnum.TestCase, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(5, historyChanges.Count);
			Assert.AreEqual(5, historyChangeSets.Count);
			Assert.AreEqual(tcId, historyChangeSets[4].ArtifactId);
			Assert.AreEqual(2, historyChangeSets[4].ArtifactTypeId);
			Assert.AreEqual("Test Case", historyChangeSets[4].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[4].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[4].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[4].ProjectId);
			Assert.AreEqual(3, historyChangeSets[4].UserId);
			Assert.AreEqual("Joe P Smith", historyChangeSets[4].UserName);
		}

		/// <summary>This tests creating, modifying, deleting a test set</summary> 
		[Test]
		[SpiraTestCase(772)]
		public void _12_CycleTestSet()
		{
			//Notes on users: 
			// 1. On creation: Owner=Fred Bloggs; Author=Roger; HistoryOpener=Roger 
			// 2. Updated: Author=Roger --> Author=Fred Bloggs; HistoryUpdater: Administrator 
			// 3. Deleted: HistoryUpdated: Fred Bloggs. 


			//Create the TestSet with the following info: 
			int projectId = 3; //Sample Application Two 
			int projectTemplateId = 3; //TODO:Templates, need to change 
			int openerId = 4; //Roger Q Ramjet 
			int? ownerId = 2; //Fred Bloggs 
			TestSet.TestSetStatusEnum tsStatus = TestSet.TestSetStatusEnum.Blocked;
			TestRun.TestRunTypeEnum trType = TestRun.TestRunTypeEnum.Automated;
			string name = "HistoryTest TestSet";
			string description = "This is a test set created from the HistoryTest NUnit test. _12_CycleTestSet()";
			DateTime? datePlanned = DateTime.Parse("3/31/2012 11:54 AM");
			int creatorId = 1; //Administrator 

			//Create the TestSet. 
			TestSetManager testSetManager = new TestSetManager();
			int tsId = testSetManager.Insert(
				creatorId,
				projectId,
				null,
				1,
				openerId,
				ownerId.Value,
				tsStatus,
				name,
				description,
				datePlanned.Value,
				trType,
				1,
				null
				);
			Assert.IsTrue(tsId > 0);

			//Pull the base test set and copy it so we can update it. 
			TestSet dsBaseTS = testSetManager.RetrieveById2(null, tsId);
			TestSet dsUpdTS = dsBaseTS.Clone();

			//Let's change a few items: 
			//  1 - Creator: Roger Q. Ramjet => Fred Bloggs 
			//  2 - Importance: Critical (1) => Medium (3) 
			Assert.IsNotNull(dsUpdTS);
			dsUpdTS.StartTracking();
			dsUpdTS.CreatorId = ownerId.Value;
			dsUpdTS.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Completed;
			testSetManager.Update(dsUpdTS, 1);

			//Re-pull incident, verify that the change was made properly (and that the ModifiedDate is current.) 
			dsUpdTS = testSetManager.RetrieveById2(null, tsId);
			Assert.IsNotNull(dsUpdTS);
			Assert.AreEqual(ownerId.Value, dsUpdTS.CreatorId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Completed, dsUpdTS.TestSetStatusId);

			//Delete the incident now. 
			testSetManager.MarkAsDeleted(2, projectId, tsId);

			//Try pulling the incident, Should not get it. 
			bool exceptionThrown = false;
			try
			{
				TestSet dsTestSet = testSetManager.RetrieveById2(null, tsId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Now pull the history data for that Incident. 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(tsId, Artifact.ArtifactTypeEnum.TestSet, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(4, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(tsId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(8, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Test Set", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ProjectId);
			Assert.AreEqual(1, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(tsId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(8, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Test Set", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(3, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(tsId, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(8, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Test Set", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(3, historyChangeSets[3].ProjectId);
			Assert.AreEqual(2, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion
			#region History Items 
			Assert.AreEqual(4, historyChanges.Count);
			//#2 
			HistoryChangeView historyChange = historyChanges[2];
			Assert.AreEqual(tsId, historyChange.ArtifactId);
			Assert.AreEqual(8, historyChange.ArtifactTypeId);
			Assert.AreEqual(1, historyChange.UserId);
			Assert.AreEqual("System Administrator", historyChange.ChangerName);
			Assert.AreEqual(1, historyChange.ChangeTypeId);
			Assert.AreEqual("Modified", historyChange.ChangeName);
			Assert.AreEqual(true, historyChange.CustomPropertyId.IsNull());
			Assert.AreEqual("Status", historyChange.FieldCaption);
			Assert.AreEqual(52, historyChange.FieldId);
			Assert.AreEqual("TestSetStatusId", historyChange.FieldName);
			Assert.AreEqual("Completed", historyChange.NewValue);
			Assert.AreEqual(true, historyChange.NewValueDate.IsNull());
			Assert.AreEqual(3, historyChange.NewValueInt);
			Assert.AreEqual("Blocked", historyChange.OldValue);
			Assert.AreEqual(true, historyChange.OldValueDate.IsNull());
			Assert.AreEqual(4, historyChange.OldValueInt);
			//#1 
			historyChange = historyChanges[1];
			Assert.AreEqual(tsId, historyChange.ArtifactId);
			Assert.AreEqual(8, historyChange.ArtifactTypeId);
			Assert.AreEqual(1, historyChange.UserId);
			Assert.AreEqual("System Administrator", historyChange.ChangerName);
			Assert.AreEqual(1, historyChange.ChangeTypeId);
			Assert.AreEqual("Modified", historyChange.ChangeName);
			Assert.AreEqual(true, historyChange.CustomPropertyId.IsNull());
			Assert.AreEqual("Creator", historyChange.FieldCaption);
			Assert.AreEqual(49, historyChange.FieldId);
			Assert.AreEqual("CreatorId", historyChange.FieldName);
			Assert.AreEqual("Fred Bloggs", historyChange.NewValue);
			Assert.AreEqual(true, historyChange.NewValueDate.IsNull());
			Assert.AreEqual(2, historyChange.NewValueInt);
			Assert.AreEqual("Roger Q Ramjet", historyChange.OldValue);
			Assert.AreEqual(true, historyChange.OldValueDate.IsNull());
			Assert.AreEqual(4, historyChange.OldValueInt);
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, tsId, historyChanges[0].ChangeSetId, 1, ref rollLog);
			System.Diagnostics.Trace.Write(rollLog + Environment.NewLine);

			//...And pull the incident, make sure that it's fields match up with what we expect. 
			TestSet dsTSRestore = testSetManager.RetrieveById2(null, tsId);
			TestSet rowOrig = dsBaseTS;
			TestSet rowCheck = dsTSRestore;

			//Loop, lookin' at each column 
			foreach (KeyValuePair<string, PropertyInfo> kvp in dsBaseTS.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = rowOrig[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = rowCheck[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue + "\" // \"" + objNewValue + "\"");
				if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			testSetManager.DeleteFromDatabase(tsId, openerId);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(tsId, Artifact.ArtifactTypeEnum.TestSet, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(6, historyChanges.Count);
			Assert.AreEqual(6, historyChangeSets.Count);
			Assert.AreEqual(tsId, historyChangeSets[5].ArtifactId);
			Assert.AreEqual(8, historyChangeSets[5].ArtifactTypeId);
			Assert.AreEqual("Test Set", historyChangeSets[5].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[5].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[5].ChangeTypeName);
			Assert.AreEqual(3, historyChangeSets[5].ProjectId);
			Assert.AreEqual(4, historyChangeSets[5].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[5].UserName);
		}

		/// <summary>This tests creating, modifying, deleting a release</summary> 
		[Test]
		[SpiraTestCase(773)]
		public void _13_CycleRelease()
		{
			//Notes on users: 
			// 1. On creation: Owner=Fred Bloggs; Author=Roger; HistoryOpener=Roger 
			// 2. Updated: Author=Roger --> Author=Fred Bloggs; HistoryUpdater: Administrator 
			// 3. Deleted: HistoryUpdated: Fred Bloggs. 

			//Create the Release with the following info: 
			int openerId = 4; //Roger Q Ramjet 
			int? ownerId = 2; //Fred Bloggs 
			string name = "HistoryTest Release";
			string description = "This is a release created from the HistoryTest NUnit test. _13_CycleRelease()";
			string versionNumber = "1.2.3.45.67";
			DateTime? dateStart = DateTime.Parse("1/1/2011 12:00 AM");
			DateTime? dateEnd = DateTime.Parse("3/31/2012 11:54 AM");
			int creatorId = 1; //Administrator 

			//Create the Release. 
			int relId = new ReleaseManager().Insert(
				openerId,
				projectId,
				creatorId,
				name,
				description,
				versionNumber,
				"AAA",
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				dateStart.Value,
				dateEnd.Value,
				3,
				2,
				null);
			Assert.IsTrue(relId > 0);

			//Pull the base release and copy it so we can update it. 
			ReleaseView releaseView = new ReleaseManager().RetrieveById2(null, relId);
			Release updatedRelease = releaseView.ConvertTo<ReleaseView, Release>();
			updatedRelease.StartTracking();

			//Let's change a few items: 
			//  1 - Creator: Roger Q. Ramjet => Fred Bloggs 
			//  2 - End Date: 3/31/2012 11:54 AM => 3/31/2011 12:00 AM 
			Assert.IsNotNull(updatedRelease);
			updatedRelease.CreatorId = ownerId.Value;
			updatedRelease.EndDate = DateTime.Parse("3/31/2011 12:00 AM");
			new ReleaseManager().Update(new List<Release>() { updatedRelease }, 1, projectId);

			//Re-pull incident, verify that the change was made properly (and that the ModifiedDate is current.) 
			ReleaseView releaseView2 = new ReleaseManager().RetrieveById2(null, relId);
			Assert.IsNotNull(releaseView2);
			Assert.AreEqual(ownerId.Value, releaseView2.CreatorId);
			Assert.AreEqual(DateTime.Parse("3/31/2011 12:00 AM"), releaseView2.EndDate);

			//Delete the incident now. 
			new ReleaseManager().MarkAsDeleted(2, projectId, relId);

			//Try pulling the Release, Should not get it. 
			bool exceptionThrown = false;
			try
			{
				ReleaseView deletedRelease = new ReleaseManager().RetrieveById2(null, relId);
				Assert.IsNull(deletedRelease);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Now pull the history data for that Release. 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(relId, Artifact.ArtifactTypeEnum.Release, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(4, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(relId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(4, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Release", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(openerId, historyChangeSets[0].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(relId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(4, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Release", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(relId, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(4, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Release", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[3].ProjectId);
			Assert.AreEqual(2, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion
			#region History Items 
			Assert.AreEqual(4, historyChanges.Count);
			//#1 
			Assert.AreEqual(relId, historyChanges[1].ArtifactId);
			Assert.AreEqual(4, historyChanges[1].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[1].UserId);
			Assert.AreEqual("System Administrator", historyChanges[1].ChangerName);
			Assert.AreEqual(1, historyChanges[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[1].ChangeName);
			Assert.AreEqual(true, historyChanges[1].CustomPropertyId.IsNull());
			Assert.AreEqual("Creator", historyChanges[1].FieldCaption);
			Assert.AreEqual(30, historyChanges[1].FieldId);
			Assert.AreEqual("CreatorId", historyChanges[1].FieldName);
			Assert.AreEqual("Fred Bloggs", historyChanges[1].NewValue);
			Assert.AreEqual(true, historyChanges[1].NewValueDate.IsNull());
			Assert.AreEqual(2, historyChanges[1].NewValueInt);
			Assert.AreEqual("System Administrator", historyChanges[1].OldValue);
			Assert.AreEqual(true, historyChanges[1].OldValueDate.IsNull());
			Assert.AreEqual(1, historyChanges[1].OldValueInt);
			//#2 
			Assert.AreEqual(relId, historyChanges[2].ArtifactId);
			Assert.AreEqual(4, historyChanges[2].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[2].UserId);
			Assert.AreEqual("System Administrator", historyChanges[2].ChangerName);
			Assert.AreEqual(1, historyChanges[2].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[2].ChangeName);
			Assert.AreEqual(true, historyChanges[2].CustomPropertyId.IsNull());
			Assert.AreEqual("End Date", historyChanges[2].FieldCaption);
			Assert.AreEqual(81, historyChanges[2].FieldId);
			Assert.AreEqual("EndDate", historyChanges[2].FieldName);
			Assert.AreEqual("2011-03-31T00:00:00.000", historyChanges[2].NewValue);
			Assert.AreEqual(true, historyChanges[2].NewValueInt.IsNull());
			Assert.AreEqual(DateTime.Parse("3/31/2011 12:00 AM"), historyChanges[2].NewValueDate);
			Assert.AreEqual("2012-03-31T11:54:00.000", historyChanges[2].OldValue);
			Assert.AreEqual(true, historyChanges[2].OldValueInt.IsNull());
			Assert.AreEqual(DateTime.Parse("3/31/2012 11:54 AM"), historyChanges[2].OldValueDate);
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Release, relId, historyChanges[0].ChangeSetId, 1, ref rollLog);
			System.Diagnostics.Trace.Write(rollLog);

			//...And pull the release, make sure that it's fields match up with what we expect. 
			ReleaseView restoredRelease = new ReleaseManager().RetrieveById2(null, relId);

			//Loop, lookin' at each column 
			foreach (KeyValuePair<string, PropertyInfo> kvp in releaseView.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = releaseView[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = restoredRelease[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue.ToSafeString() + "\" // \"" + objNewValue.ToSafeString() + "\"");
				if (fieldName != "ConcurrencyDate" && fieldName != "LastUpdateDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			new ReleaseManager().DeleteFromDatabase(relId, openerId);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(relId, Artifact.ArtifactTypeEnum.Release, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(6, historyChanges.Count);
			Assert.AreEqual(6, historyChangeSets.Count);
			Assert.AreEqual(relId, historyChangeSets[5].ArtifactId);
			Assert.AreEqual(4, historyChangeSets[5].ArtifactTypeId);
			Assert.AreEqual("Release", historyChangeSets[5].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[5].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[5].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[5].ProjectId);
			Assert.AreEqual(4, historyChangeSets[5].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[5].UserName);
		}

		/// <summary>This tests creating, modifying, deleting a task</summary> 
		[Test]
		[SpiraTestCase(774)]
		public void _14_CycleTask()
		{
			//Notes on users: 
			// 1. On creation: Owner=Fred Bloggs; Author=Roger; HistoryOpener=Roger 
			// 2. Updated: Author=Roger --> Author=Fred Bloggs; HistoryUpdater: Administrator 
			// 3. Deleted: HistoryUpdated: Fred Bloggs. 

			TaskManager taskManager = new TaskManager();
			List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);

			//Create the Task with the following info: 
			int openerId = 4; //Roger Q Ramjet 
			int? ownerId = 2; //Fred Bloggs 
			Task.TaskStatusEnum taskStatus = Task.TaskStatusEnum.NotStarted;
			int? taskTypeId = null;
			string name = "HistoryTest Task";
			string description = "This is a task created from the HistoryTest NUnit test. _14_CycleTask()";
			DateTime? dateStart = DateTime.Parse("1/1/2011 12:00 AM");
			DateTime? dateEnd = DateTime.Parse("3/31/2012 11:54 AM");
			int creatorId = 1; //Administrator 

			//Create the Release. 
			int taskId = new TaskManager().Insert(
				projectId,
				creatorId,
				taskStatus,
				taskTypeId,
				null,
				null,
				null,
				ownerId,
				priorities.FirstOrDefault(p => p.Score == 2).TaskPriorityId,
				name,
				description,
				dateStart,
				dateEnd,
				150,
				null,
				150,
				creatorId);
			Assert.IsTrue(taskId > 0);

			//Pull the base task and clone it so we can update it. 
			Task baseTask = new TaskManager().RetrieveById(taskId);
			Task updTask = ArtifactHelper.Clone<Task>(baseTask);

			//Let's change a few items: 
			//  1 - Creator: Roger Q. Ramjet => Fred Bloggs 
			//  2 - End Date: 3/31/2012 11:54 AM => 3/31/2011 12:00 AM 
			Assert.IsNotNull(updTask);
			updTask.StartTracking();
			updTask.CreatorId = ownerId.Value;
			updTask.EndDate = DateTime.Parse("3/31/2011 12:00 AM");
			new TaskManager().Update(updTask, 1);

			//Re-pull task, verify that the change was made properly (and that the ModifiedDate is current.) 
			updTask = new TaskManager().RetrieveById(taskId);
			Assert.IsNotNull(updTask);
			Assert.AreEqual(ownerId.Value, updTask.CreatorId);
			Assert.AreEqual(DateTime.Parse("3/31/2011 12:00 AM"), updTask.EndDate);

			//Delete the Task now. 
			new TaskManager().MarkAsDeleted(projectId, taskId, 2);

			//Try pulling the Task, Should not get it. 
			bool exceptionThrown = false;
			try
			{
				Task task2 = new TaskManager().RetrieveById(taskId);
				Assert.IsNull(task2);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);

			//Now pull the history data for that Task. 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(taskId, Artifact.ArtifactTypeEnum.Task, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(4, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(taskId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(6, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Task", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(1, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(taskId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(6, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Task", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(taskId, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(6, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Task", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[3].ProjectId);
			Assert.AreEqual(2, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion
			#region History Items 
			Assert.AreEqual(4, historyChanges.Count);
			//#1 
			Assert.AreEqual(taskId, historyChanges[1].ArtifactId);
			Assert.AreEqual(6, historyChanges[1].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[1].UserId);
			Assert.AreEqual("System Administrator", historyChanges[1].ChangerName);
			Assert.AreEqual(1, historyChanges[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[1].ChangeName);
			Assert.AreEqual(true, historyChanges[1].CustomPropertyId.IsNull());
			Assert.AreEqual("Creator", historyChanges[1].FieldCaption);
			Assert.AreEqual(128, historyChanges[1].FieldId);
			Assert.AreEqual("CreatorId", historyChanges[1].FieldName);
			Assert.AreEqual("Fred Bloggs", historyChanges[1].NewValue);
			Assert.AreEqual(true, historyChanges[1].NewValueDate.IsNull());
			Assert.AreEqual(2, historyChanges[1].NewValueInt);
			Assert.AreEqual("System Administrator", historyChanges[1].OldValue);
			Assert.AreEqual(true, historyChanges[1].OldValueDate.IsNull());
			Assert.AreEqual(1, historyChanges[1].OldValueInt);
			//#2 
			Assert.AreEqual(taskId, historyChanges[2].ArtifactId);
			Assert.AreEqual(6, historyChanges[2].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[2].UserId);
			Assert.AreEqual("System Administrator", historyChanges[2].ChangerName);
			Assert.AreEqual(1, historyChanges[2].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[2].ChangeName);
			Assert.AreEqual(true, historyChanges[2].CustomPropertyId.IsNull());
			Assert.AreEqual("End Date", historyChanges[2].FieldCaption);
			Assert.AreEqual(63, historyChanges[2].FieldId);
			Assert.AreEqual("EndDate", historyChanges[2].FieldName);
			Assert.AreEqual("2011-03-31T00:00:00.000", historyChanges[2].NewValue);
			Assert.AreEqual(true, historyChanges[2].NewValueInt.IsNull());
			Assert.AreEqual(DateTime.Parse("3/31/2011 12:00 AM"), historyChanges[2].NewValueDate);
			Assert.AreEqual("2012-03-31T11:54:00.000", historyChanges[2].OldValue);
			Assert.AreEqual(true, historyChanges[2].OldValueInt.IsNull());
			Assert.AreEqual(DateTime.Parse("3/31/2012 11:54 AM"), historyChanges[2].OldValueDate);
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, taskId, historyChanges[0].ChangeSetId, 1, ref rollLog);
			System.Diagnostics.Trace.Write(rollLog);

			//...And pull the task, make sure that it's fields match up with what we expect. 
			Task restoredTask = new TaskManager().RetrieveById(taskId);

			//Loop, looking at each column 
			foreach (KeyValuePair<string, PropertyInfo> kvp in baseTask.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = baseTask[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = restoredTask[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue.ToSafeString() + "\" // \"" + objNewValue.ToSafeString() + "\"");
				if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			new TaskManager().DeleteFromDatabase(taskId, openerId);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(taskId, Artifact.ArtifactTypeEnum.Task, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(6, historyChanges.Count);
			Assert.AreEqual(6, historyChangeSets.Count);
			Assert.AreEqual(taskId, historyChangeSets[5].ArtifactId);
			Assert.AreEqual(6, historyChangeSets[5].ArtifactTypeId);
			Assert.AreEqual("Task", historyChangeSets[5].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[5].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[5].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[5].ProjectId);
			Assert.AreEqual(4, historyChangeSets[5].UserId);
			Assert.AreEqual("Roger Q Ramjet", historyChangeSets[5].UserName);
		}

		/// <summary>This tests creating, modifying, deleting an automation host</summary> 
		[Test]
		[SpiraTestCase(775)]
		public void _15_CycleAutomationHost()
		{
			//Notes on users: 
			// 1. On creation: Owner=Fred Bloggs; Author=Roger; HistoryOpener=Roger 
			// 2. Updated: Author=Roger --> Author=Fred Bloggs; HistoryUpdater: Administrator 
			// 3. Deleted: HistoryUpdated: Fred Bloggs. 


			//Create the Host with the following info: 
			string name = "HistoryTest AutomationHost";
			string token = "HistoryAuto";
			string description = "This is a host created from the HistoryTest NUnit test. _15_CycleAutomationHost()";
			int creatorId = 1; //Administrator 

			//Create the Release. 
			int hostId = new AutomationManager().InsertHost(
				projectId,
				name,
				token,
				description,
				true,
				creatorId);

			Assert.IsTrue(hostId > 0);

			//Pull the base release and copy it so we can update it. 
			AutomationManager automationManager = new AutomationManager();
			AutomationHost automationHostBase = automationManager.RetrieveHostById2(hostId);
			AutomationHost automationHostUpdated = automationHostBase.Clone();
			AutomationHost rowOrig = automationHostBase.Clone();

			//Let's change a few items: 
			//  1 - Name: HistoryTest AutomationHost => HistoryTest AutomationHost2 
			//  2 - Token: HistoryAuto => HistoryAuto2 
			Assert.IsNotNull(automationHostBase);
			automationHostBase.StartTracking();
			automationHostBase.Name = "HistoryTest AutomationHost2";
			automationHostBase.Token = "HistoryAuto2";
			automationManager.UpdateHost(automationHostBase, USER_ID_SYS_ADMIN);

			//Re-pull incident, verify that the change was made properly (and that the ModifiedDate is current.) 
			automationHostUpdated = automationManager.RetrieveHostById2(hostId);
			Assert.IsNotNull(automationHostUpdated);
			Assert.AreEqual("HistoryTest AutomationHost2", automationHostUpdated.Name);
			Assert.AreEqual("HistoryAuto2", automationHostUpdated.Token);

			//Delete the Task now. 
			automationManager.MarkHostAsDeleted(projectId, hostId, 2);

			//Try pulling the automation host, Should not get it. 
			try
			{
				AutomationHost automationHost = automationManager.RetrieveHostById2(hostId);
				Assert.IsNull(automationHost);
			}
			catch { }

			//Now pull the history data for that Task. 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(hostId, Artifact.ArtifactTypeEnum.AutomationHost, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			#region Change Sets 
			Assert.AreEqual(4, historyChangeSets.Count);
			//#1 
			Assert.AreEqual(hostId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual(9, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual("Automation Host", historyChangeSets[0].ArtifactTypeName);
			Assert.AreEqual(3, historyChangeSets[0].ChangeTypeId);
			Assert.AreEqual("Added", historyChangeSets[0].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[0].ProjectId);
			Assert.AreEqual(1, historyChangeSets[0].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[0].UserName);
			//#2 
			Assert.AreEqual(hostId, historyChangeSets[1].ArtifactId);
			Assert.AreEqual(9, historyChangeSets[1].ArtifactTypeId);
			Assert.AreEqual("Automation Host", historyChangeSets[1].ArtifactTypeName);
			Assert.AreEqual(1, historyChangeSets[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChangeSets[1].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[1].ProjectId);
			Assert.AreEqual(1, historyChangeSets[1].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[1].UserName);
			//#3 
			Assert.AreEqual(hostId, historyChangeSets[3].ArtifactId);
			Assert.AreEqual(9, historyChangeSets[3].ArtifactTypeId);
			Assert.AreEqual("Automation Host", historyChangeSets[3].ArtifactTypeName);
			Assert.AreEqual(2, historyChangeSets[3].ChangeTypeId);
			Assert.AreEqual("Deleted", historyChangeSets[3].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[3].ProjectId);
			Assert.AreEqual(2, historyChangeSets[3].UserId);
			Assert.AreEqual("Fred Bloggs", historyChangeSets[3].UserName);
			#endregion
			#region History Items 
			Assert.AreEqual(4, historyChanges.Count);
			//#1 
			Assert.AreEqual(hostId, historyChanges[1].ArtifactId);
			Assert.AreEqual(9, historyChanges[1].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[1].UserId);
			Assert.AreEqual("System Administrator", historyChanges[1].ChangerName);
			Assert.AreEqual(1, historyChanges[1].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[1].ChangeName);
			Assert.AreEqual(true, historyChanges[1].CustomPropertyId.IsNull());
			Assert.AreEqual("Host Name", historyChanges[1].FieldCaption);
			Assert.AreEqual(112, historyChanges[1].FieldId);
			Assert.AreEqual("Name", historyChanges[1].FieldName);
			Assert.AreEqual("HistoryTest AutomationHost2", historyChanges[1].NewValue);
			Assert.AreEqual(true, historyChanges[1].NewValueDate.IsNull());
			Assert.AreEqual(true, historyChanges[1].NewValueInt.IsNull());
			Assert.AreEqual("HistoryTest AutomationHost", historyChanges[1].OldValue);
			Assert.AreEqual(true, historyChanges[1].OldValueDate.IsNull());
			Assert.AreEqual(true, historyChanges[1].OldValueInt.IsNull());
			//#2 
			Assert.AreEqual(hostId, historyChanges[2].ArtifactId);
			Assert.AreEqual(9, historyChanges[2].ArtifactTypeId);
			Assert.AreEqual(1, historyChanges[2].UserId);
			Assert.AreEqual("System Administrator", historyChanges[2].ChangerName);
			Assert.AreEqual(1, historyChanges[2].ChangeTypeId);
			Assert.AreEqual("Modified", historyChanges[2].ChangeName);
			Assert.AreEqual(true, historyChanges[2].CustomPropertyId.IsNull());
			Assert.AreEqual("Token", historyChanges[2].FieldCaption);
			Assert.AreEqual(114, historyChanges[2].FieldId);
			Assert.AreEqual("Token", historyChanges[2].FieldName);
			Assert.AreEqual("HistoryAuto2", historyChanges[2].NewValue);
			Assert.AreEqual(true, historyChanges[2].NewValueInt.IsNull());
			Assert.AreEqual(true, historyChanges[2].NewValueDate.IsNull());
			Assert.AreEqual("HistoryAuto", historyChanges[2].OldValue);
			Assert.AreEqual(true, historyChanges[2].OldValueInt.IsNull());
			Assert.AreEqual(true, historyChanges[2].OldValueDate.IsNull());
			#endregion

			//Now revert back to our pre-changes. 
			string rollLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, hostId, historyChanges[1].ChangeSetId, 1, ref rollLog);
			System.Diagnostics.Trace.Write(rollLog);

			//...And pull the incident, make sure that it's fields match up with what we expect. 
			AutomationHost automationHostRestored = automationManager.RetrieveHostById2(hostId);
			AutomationHost rowCheck = automationHostRestored;

			//Loop, lookin' at each column 
			foreach (KeyValuePair<string, PropertyInfo> kvp in automationHostBase.Properties)
			{
				string fieldName = kvp.Key;

				object objOriginalValue;
				object objNewValue;

				//Get the original column. 
				try
				{
					objOriginalValue = rowOrig[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objOriginalValue = ex.GetType().ToString();
				}

				//Get the restored column. 
				try
				{
					objNewValue = rowCheck[fieldName];
				}
				catch (Exception ex)
				{
					//If we threw an error, the exception type gets put into the value. 
					objNewValue = ex.GetType().ToString();
				}

				//Now do the comparison. 
				System.Diagnostics.Trace.WriteLine("Checking field " + fieldName + ": \"" + objOriginalValue + "\" // \"" + objNewValue + "\"");
				if (fieldName != "LastUpdateDate" && fieldName != "ConcurrencyDate" && fieldName != "EntityChangeTracker" && fieldName != "ChangeTracker")
				{
					//Assert.AreEqual(objOriginalValue, objNewValue, "Field '" + fieldName + "' did not match.");
				}
			}

			//Okay, now purge the item. 
			automationManager.DeleteHostFromDatabase(hostId, creatorId);
			//And now get our changesets. 
			historyManager.RetrieveByArtifactId(hostId, Artifact.ArtifactTypeEnum.AutomationHost, out historyChangeSets, out historyChanges, out historyChangeSetTypes);

			//Verify our entries. 
			Assert.AreEqual(7, historyChanges.Count);
			Assert.AreEqual(7, historyChangeSets.Count);
			Assert.AreEqual(hostId, historyChangeSets[6].ArtifactId);
			Assert.AreEqual(9, historyChangeSets[6].ArtifactTypeId);
			Assert.AreEqual("Automation Host", historyChangeSets[6].ArtifactTypeName);
			Assert.AreEqual(4, historyChangeSets[6].ChangeTypeId);
			Assert.AreEqual("Purged", historyChangeSets[6].ChangeTypeName);
			Assert.AreEqual(projectId, historyChangeSets[6].ProjectId);
			Assert.AreEqual(1, historyChangeSets[6].UserId);
			Assert.AreEqual("System Administrator", historyChangeSets[6].UserName);
		}

		/// <summary> 
		/// This tests creating, modifying, deleting a requirement step 
		/// </summary> 
		[Test]
		[SpiraTestCase(1214)]
		public void _16_CycleRequirementSteps()
		{
			//First create a new requirement that we'll add steps to 
			RequirementManager requirementManager = new RequirementManager();
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);

			//Now lets add a step to this requirement 
			int requirementStepId = requirementManager.InsertStep(projectId, requirementId, null, "First do this", USER_ID_FRED_BLOGGS);

			//Verify the creation history entry 
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			Assert.AreEqual(1, historyChangeSets.Count);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, historyChangeSets[0].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, historyChangeSets[0].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			long changeSetId = historyChangeSets[0].ChangeSetId;   //Used later on for the rollback 

			//Now test that we can modify a step 
			RequirementStep requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			requirementStep.StartTracking();
			requirementStep.Description = "Missed doing this";
			requirementManager.UpdateStep(projectId, requirementStep, USER_ID_FRED_BLOGGS);

			//Verify change 
			requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			Assert.AreEqual("Missed doing this", requirementStep.Description);

			//Verify the modify history entry 
			List<HistoryChangeSetResponse> changeSets = historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep);
			Assert.AreEqual(2, changeSets.Count);

			//First changeset  - modify 
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[1].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[1].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[1].ChangeTypeId);
			//Verify the modify history changeset has the field level detail 
			//TrackableCollection<HistoryDetail> detailRows = changeSets[0].Details;
			//Assert.AreEqual(1, detailRows.Count);
			Assert.AreEqual("First do this", changeSets[1].OldValue);
			Assert.AreEqual("Missed doing this", changeSets[1].NewValue);

			//Second changeset - added 
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[1].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[1].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[1].ChangeTypeId);

			//Now test that we can delete a step 
			requirementManager.DeleteStep(projectId, requirementStepId, USER_ID_FRED_BLOGGS);

			//Verify it was deleted 
			requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId, true);
			Assert.IsNotNull(requirementStep);

			//Verify the delete history entry 
			changeSets = historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep);
			Assert.AreEqual(3, changeSets.Count);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[2].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[2].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Deleted, changeSets[2].ChangeTypeId);

			//Roll-back the delete and the modification 
			string rollbackLog = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.RequirementStep, requirementStepId, changeSetId, USER_ID_FRED_BLOGGS, ref rollbackLog);

			//Verify that it was restored and that the data is back to its original form 
			requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			Assert.IsNotNull(requirementStep);
			Assert.IsFalse(requirementStep.IsDeleted);
			Assert.AreEqual("First do this", requirementStep.Description);

			//We should also have a rollback entry in the log 
			changeSets = historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep);

			//The rollback of the modify 
			Assert.AreEqual(5, changeSets.Count);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[3].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[3].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Rollback, changeSets[3].ChangeTypeId);
			//Verify the modify history changeset has the field level detail 
			//detailRows = changeSets[1].Details;
			//Assert.AreEqual(1, detailRows.Count);
			//Assert.AreEqual("Missed doing this", changeSets[4].OldValue);
			//Assert.AreEqual("First do this", changeSets[4].NewValue);

			//The rollback of the delete 
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[4].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[4].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Undelete, changeSets[4].ChangeTypeId);

			//Test that we can purge deleted items 
			//First delete the step again 
			requirementManager.DeleteStep(projectId, requirementStepId, USER_ID_FRED_BLOGGS);

			//Verify it was deleted 
			requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId, true);
			Assert.IsNotNull(requirementStep);

			//We should also have a delete entry in the log 
			changeSets = historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep);
			Assert.AreEqual(6, changeSets.Count);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[0].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[0].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSets[0].ChangeTypeId);

			//Now purge all deletes in the project 
			historyManager.PurgeAllDeleted(projectId, USER_ID_FRED_BLOGGS);

			//Verify that it was purged 
			requirementStep = requirementManager.RetrieveStepById(requirementStepId);
			Assert.IsNull(requirementStep);
			requirementStep = requirementManager.RetrieveStepById(requirementStepId, true);
			Assert.IsNull(requirementStep);

			//We should also have a single purge entry in the log (all the other history is wiped) 
			changeSets = historyManager.RetrieveByArtifactId(requirementStepId, Artifact.ArtifactTypeEnum.RequirementStep);
			Assert.AreEqual(7, changeSets.Count);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.RequirementStep, changeSets[6].ArtifactTypeId);
			Assert.AreEqual(requirementStepId, changeSets[6].ArtifactId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Purged, changeSets[6].ChangeTypeId);
		}

		/// <summary>Tests that we can update an attachment and have the history changes be recorded</summary> 
		[Test]
		[SpiraTestCase(1339)]
		public void _17_AttachmentHistoryChanges()
		{
			AttachmentManager attachmentManager = new AttachmentManager();

			//Create a new requirement. 
			int requirementId = new RequirementManager().Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Requested,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				null,
				"Test Requirement",
				null,
				null,
				USER_ID_FRED_BLOGGS
				);
			Assert.IsTrue(requirementId > 0);
			//Create a new attachment 
			int attachmentId = attachmentManager.Insert(projectId, "www.x.com", "", USER_ID_FRED_BLOGGS, requirementId, Artifact.ArtifactTypeEnum.Requirement, "1.0", null, documentTypeId1, null, null);

			//Verify a creation event was logged, there will also be a new version changeset as well
			List<HistoryChangeSetResponse> changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			Assert.AreEqual(1, changeSets.Count);
			//Creation of the new attachment
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSets[0].ChangeTypeId);
			long oldChangesetId = changeSets[0].ChangeSetId;    //Used later for rollback 
																//Adding of the new version
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSets[0].ChangeTypeId);

			//Now make some changes to the attachment 
			ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);

			//Change the type (folder changes not tracked) 
			projectAttachment.StartTracking();
			projectAttachment.DocumentTypeId = documentTypeId2;

			//Change the URL/editor 
			projectAttachment.Attachment.StartTracking();
			projectAttachment.Attachment.Filename = "www.y.com";
			projectAttachment.Attachment.EditorId = USER_ID_JOE_SMITH;
			attachmentManager.Update(projectAttachment, USER_ID_FRED_BLOGGS, null, true);

			//Verify the changes, the two entities will have different changesets 
			changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			Assert.AreEqual(4, changeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSets[0].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[2].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[3].ChangeTypeId);

			//Now verify the field changes 
			//TrackableCollection<HistoryDetail> artifactHistoryRows = changeSets[0].Details;
			//Assert.AreEqual(2, artifactHistoryRows.Count);
			Assert.AreEqual("Filename", changeSets[0].FieldName);
			Assert.AreEqual("www.x.com", changeSets[2].OldValue);
			Assert.AreEqual("www.y.com", changeSets[2].NewValue);
			Assert.AreEqual("EditorId", changeSets[3].FieldName);
			//Assert.AreEqual(USER_ID_FRED_BLOGGS, changeSets[1].OldValueInt);
			//Assert.AreEqual(USER_ID_JOE_SMITH, changeSets[1].NewValueInt);
			Assert.AreEqual("Fred Bloggs", changeSets[3].OldValue);
			Assert.AreEqual("Joe P Smith", changeSets[3].NewValue);

			//artifactHistoryRows = changeSets[1].Details;
			//Assert.AreEqual(1, artifactHistoryRows.Count);
			Assert.AreEqual("DocumentTypeId", changeSets[1].FieldName);
			//Assert.AreEqual(documentTypeId1, artifactHistoryRows[0].OldValueInt);
			//Assert.AreEqual(documentTypeId2, artifactHistoryRows[0].NewValueInt);
			Assert.AreEqual("Diagram", changeSets[1].OldValue);
			Assert.AreEqual("Specification", changeSets[1].NewValue);

			//Now verify that we can rollback the changes 
			string log = "";
			historyManager.RollbackHistory(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Document, attachmentId, oldChangesetId, USER_ID_FRED_BLOGGS, ref log);

			//Verify that it rolled-back correctly 
			projectAttachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId);
			Assert.AreEqual(documentTypeId1, projectAttachment.DocumentTypeId, log);
			Assert.AreEqual("www.x.com", projectAttachment.Attachment.Filename, log);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, projectAttachment.Attachment.EditorId, log);

			//Verify the rollback events were logged 
			changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			Assert.AreEqual(7, changeSets.Count);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Rollback, changeSets[6].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Rollback, changeSets[5].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[3].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[2].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, changeSets[1].ChangeTypeId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSets[0].ChangeTypeId);

			//Now we need to test that adding a new version is logged in history
			int attachmentVersionId1 = attachmentManager.RetrieveActiveVersion(attachmentId).AttachmentVersionId;
			int attachmentVersionId2 = attachmentManager.InsertVersion(projectId, attachmentId, "www.y.com", "Description 2", USER_ID_FRED_BLOGGS, "2.0", true);

			//Verify the history entry
			changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			HistoryChangeSetResponse changeSet = changeSets.First();
			Assert.AreEqual(USER_ID_FRED_BLOGGS, changeSet.UserId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSet.ChangeTypeId);
			//Assert.AreEqual(2, changeSet.Details.Count);
			//Assert.AreEqual("_AttachmentVersion", changeSet.Details[0].FieldName);
			//Assert.AreEqual("_CurrentVersion", changeSet.Details[1].FieldName);

			//Now we need to test that changing the active version is logged in history
			attachmentManager.SetCurrentVersion(projectId, attachmentId, attachmentVersionId1, USER_ID_JOE_SMITH);

			//Verify the history entry
			changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			changeSet = changeSets.First();
			Assert.AreEqual(USER_ID_FRED_BLOGGS, changeSet.UserId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSet.ChangeTypeId);
			//Assert.AreEqual(1, changeSet.Details.Count);
			Assert.AreEqual("Filename", changeSet.FieldName);

			//Now we need to test that deleting a version is logged in history
			attachmentManager.DeleteVersion(projectId, attachmentVersionId2, USER_ID_FRED_BLOGGS);

			//Verify the history entry
			changeSets = historyManager.RetrieveByArtifactId(attachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);
			changeSet = changeSets.First();
			Assert.AreEqual(USER_ID_FRED_BLOGGS, changeSet.UserId);
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, changeSet.ChangeTypeId);
			//Assert.AreEqual(1, changeSet.Details.Count);
			//Assert.AreEqual("_AttachmentVersion", changeSet.Details[0].FieldName);

			//Delete the attachment from the project 
			//Unlike other artifacts this is permanent 
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
		}

		/// <summary> 
		/// Record digital signature hash records and validate current record against hash 
		/// </summary> 
		[Test]
		[SpiraTestCase(1343)]
		public void _18_DigitalSignatureHashing()
		{
			//Get some of the lookup values 
			RequirementManager requirementManager = new Business.RequirementManager();
			int importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;

			//Let's create a new requirement in a project 
			int requirementId = requirementManager.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.UnderReview,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				importanceHighId,
				"Test Requirement 1",
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Now we need to make a signed change to it 
			RequirementView requirementView = requirementManager.RetrieveById2(null, requirementId);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
			requirement.SignatureMeaning = "I have approved this requirement";
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement }, null, true);

			//Check that the change was signed and that the HASH matches 
			List<HistoryChangeSetResponse> changeSets = historyManager.RetrieveByArtifactId(requirementId, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, changeSets.Count);
			HistoryChangeSetResponse historyChangeSet = changeSets[0];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSet.ChangeTypeId);
			historyChangeSet = changeSets[1];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSet.ChangeTypeId);
			string expectedHash = SimpleHash.GetHashSha256(historyChangeSet.UserId + ":" + historyChangeSet.ArtifactTypeId + ":" + historyChangeSet.ArtifactId + ":" + historyChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture));
			//Assert.AreEqual(expectedHash, historyChangeSet.SignatureHash);

			//Also verify that the meaning was attached to the history record 
			//Assert.AreEqual("I have approved this requirement", historyChangeSet.Meaning);

			//Check that the permanent comment was logged 
			DiscussionManager discussionManager = new DiscussionManager();
			List<IDiscussion> discussions = discussionManager.Retrieve(requirementId, Artifact.ArtifactTypeEnum.Requirement).ToList();
			Assert.AreEqual(1, discussions.Count);
			Assert.IsTrue(discussions[0].IsPermanent);

			//Get some of the lookup values 
			TaskManager taskManager = new TaskManager();
			int priorityMediumId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 3).TaskPriorityId;

			//Let's create a new task in a project 
			int taskId = taskManager.Insert(
				projectId,
				USER_ID_FRED_BLOGGS,
				Task.TaskStatusEnum.NotStarted,
				null,
				null,
				null,
				null,
				null,
				priorityMediumId,
				"Test Task 1",
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS);

			//Now we need to make a signed change to it 
			Task task = taskManager.RetrieveById(taskId);
			task.StartTracking();
			task.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			task.SignatureMeaning = "I have approved this task";
			taskManager.Update(task, USER_ID_FRED_BLOGGS, null, true);

			//Check that the change was signed and that the HASH matches 
			changeSets = historyManager.RetrieveByArtifactId(taskId, Artifact.ArtifactTypeEnum.Task);
			Assert.AreEqual(4, changeSets.Count);
			historyChangeSet = changeSets[1];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSet.ChangeTypeId);
			historyChangeSet = changeSets[0];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSet.ChangeTypeId);
			expectedHash = SimpleHash.GetHashSha256(historyChangeSet.UserId + ":" + historyChangeSet.ArtifactTypeId + ":" + historyChangeSet.ArtifactId + ":" + historyChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture));
			//Assert.AreEqual(expectedHash, historyChangeSet.SignatureHash);

			//Also verify that the meaning was attached to the history record 
			//Assert.AreEqual("I have approved this task", historyChangeSet.Meaning);

			//Check that the permanent comment was logged 
			discussions = discussionManager.Retrieve(taskId, Artifact.ArtifactTypeEnum.Task).ToList();
			Assert.AreEqual(1, discussions.Count);
			Assert.IsTrue(discussions[0].IsPermanent);

			//Now lets create a new incident 
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(
				projectId,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS,
				 null,
				 null,
				 "Test Incident 1",
				 "This is a test incident",
				 null,
				 null,
				 null,
				 null,
				 null,
				 DateTime.UtcNow,
				 null,
				 null,
				 null,
				 null,
				 null,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS);

			//Get another incident status to change it to 
			IncidentStatus incidentStatus = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true).FirstOrDefault(i => i.Name == "Assigned");

			//Now we need to make a signed change to it 
			Incident incident = incidentManager.RetrieveById(incidentId, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentStatus.IncidentStatusId;
			incident.SignatureMeaning = "I have approved this incident";
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS, null, true);

			//Check that the change was signed and that the HASH matches 
			changeSets = historyManager.RetrieveByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(2, changeSets.Count);
			historyChangeSet = changeSets[1];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSet.ChangeTypeId);
			historyChangeSet = changeSets[0];
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSet.ChangeTypeId);
			expectedHash = SimpleHash.GetHashSha256(historyChangeSet.UserId + ":" + historyChangeSet.ArtifactTypeId + ":" + historyChangeSet.ArtifactId + ":" + historyChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture));
			//Assert.AreEqual(expectedHash, historyChangeSet.SignatureHash);

			//Also verify that the meaning was attached to the history record 
			//Assert.AreEqual("I have approved this incident", historyChangeSet.Meaning);

			//Check that the resolution/comment was logged 
			incident = incidentManager.RetrieveById(incidentId, true);
			Assert.AreEqual(1, incident.Resolutions.Count);
		}

		/// <summary>Tests different methods to create and remove Requirement Test Coverage, verifying the History is recorded properly.</summary> 
		[Test]
		[SpiraTestCase(2555)]
		[Description("Tests different methods to create and remove Requirement Test Coverage, verifying the History is recorded properly.")]
		public void _19_Association_RequirementTestCoverage()
		{
			/* 
			 * Some notes to this test. You will note here that I am not checking HistoryChangeSet properties, like 
			 * the date, the author, etc. This is because all this information should be being checked in the history 
			 * tests above, and I felt that (1) It could make this test do a lot more than it has to, and (2) When this 
			 * test fails making the finding of the failing check easier. 
			 */

			//Our mnanagers needed. 
			HistoryManager hMgr = new HistoryManager();
			RequirementManager rMgr = new RequirementManager();
			TestCaseManager tMgr = new TestCaseManager();

			#region Setup 
			//We need to create our Requirement, first. (Copied from test _10_CycleRequirement) 
			List<Importance> requirementImportances = rMgr.RequirementImportance_Retrieve(projectTemplateId);
			int reqId1 = rMgr.Insert(
				4,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Accepted,
				null,
				4,
				2,
				requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId,
				"HistoryTest Requirement #1 for RequirementTestCoverage",
				"This is a test requirement created from the HistoryTest NUnit test. _19_Association_RequirementTestCoverage()",
				450,
				1
				);
			Assert.IsTrue(reqId1 > 0, "Could not create needed RQ.");
			int reqId2 = rMgr.Insert(
				4,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Completed,
				null,
				4,
				2,
				requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId,
				"HistoryTest Requirement #2 for RequirementTestCoverage",
				"This is a test requirement created from the HistoryTest NUnit test. _19_Association_RequirementTestCoverage()",
				450,
				1
				);
			Assert.IsTrue(reqId2 > 0, "Could not create needed RQ.");

			//Create the test case. (Copied from test _10_CycleTest) 
			List<TestCasePriority> priorities = tMgr.TestCasePriority_Retrieve(projectTemplateId);
			int tcId1 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #1 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _19_Association_RequirementTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			int tcId2 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #2 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _19_Association_RequirementTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			int tcId3 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #3 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _19_Association_RequirementTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);

			//Add some steps.. 
			int ts1id = tMgr.InsertStep(
				1,
				tcId1,
				null,
				"Step 1",
				"None",
				"Do this!");
			int ts2id = tMgr.InsertStep(
				2,
				tcId1,
				null,
				"Step 2",
				"None",
				"Do this!");
			int ts3id = tMgr.InsertStep(
				3,
				tcId1,
				null,
				"Step 3",
				"None",
				"Do this!");
			#endregion Setup 

			//First, let us double-check that we have no  
			int initalNumber = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).Count;
			Assert.IsTrue(initalNumber == 0, "There were already associations. Can not run this test as it is unexpectd!");

			//Baselines should be turned off. So, make sure that NOTHING is recorded when we associate something! 
			rMgr.AddToTestCase(projectId, tcId1, new List<int> { reqId1, reqId2 }, 1);
			rMgr.RemoveFromTestCase(projectId, tcId1, new List<int> { reqId1, reqId2 }, 1);
			var test1 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderBy(g => g.ArtifactId).ToList();
			Assert.AreEqual(0, test1.Count, "Associations were recorded when Baselines were disabled!");

			//Now enable baselines in this project. 
			var settings = new ProjectSettings(projectId);
			settings.BaseliningEnabled = true;
			settings.Save(USER_ID_SYS_ADMIN);

			#region Add Multiple RQs to TC 
			//Re-add those test cases. 
			rMgr.AddToTestCase(projectId, tcId1, new List<int> { reqId1, reqId2 }, 1);

			//Now verify we have that entry. 
			var test2 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderBy(g => g.ArtifactId).ToList();
			Assert.AreEqual(2, test2.Count, "Number of Changesets were not expected.");
			Assert.AreEqual(1, test2[0].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(reqId1, test2[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test2[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test2[0].ArtifactId, test2[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(1, test2[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test2[0].AssociationChanges[0].DestArtifactId, "Changeset #1 had wrong Association Destination ID.");
			Assert.AreEqual(2, test2[0].AssociationChanges[0].DestArtifactTypeId, "Changeset #1 had wrong Association Destination Type.");
			Assert.AreEqual(1, test2[1].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(reqId2, test2[1].ArtifactId, "Changeset #2 had wrong RQ ID.");
			Assert.AreEqual(2, test2[1].ArtifactTypeId, "Changeset #2 had wrong RQ Type.");
			Assert.AreEqual(test2[1].ArtifactId, test2[1].AssociationChanges[0].SourceArtifactId, "Changeset #2 had wrong Association Source ID.");
			Assert.AreEqual(1, test2[1].AssociationChanges[0].SourceArtifactTypeId, "Changeset #2 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test2[1].AssociationChanges[0].DestArtifactId, "Changeset #2 had wrong Association Destination ID.");
			Assert.AreEqual(2, test2[1].AssociationChanges[0].DestArtifactTypeId, "Changeset #2 had wrong Association Destination Type.");

			//Now remove the associations. 
			rMgr.RemoveFromTestCase(projectId, tcId1, new List<int> { reqId1, reqId2 }, 1);

			//Verify our return records. 
			var test3 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderBy(g => g.ArtifactId).ToList();
			Assert.AreEqual(4, test3.Count, "Number of Changesets not expected.");
			//Now filter our changesets for only those two removed records. 
			test3 = test3.Where(g => g.ChangeTypeId == (int)ChangeSetTypeEnum.Association_Remove).ToList();
			Assert.AreEqual(2, test3.Count, "Number of filtered Changeset not expected.");
			Assert.AreEqual(1, test3[0].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(reqId1, test3[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test3[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test3[0].ArtifactId, test3[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(1, test3[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test3[0].AssociationChanges[0].DestArtifactId, "Changeset #1 had wrong Association Destination ID.");
			Assert.AreEqual(2, test3[0].AssociationChanges[0].DestArtifactTypeId, "Changeset #1 had wrong Association Destination Type.");
			Assert.AreEqual(1, test3[1].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(reqId2, test3[1].ArtifactId, "Changeset #2 had wrong RQ ID.");
			Assert.AreEqual(2, test3[1].ArtifactTypeId, "Changeset #2 had wrong RQ Type.");
			Assert.AreEqual(test3[1].ArtifactId, test3[1].AssociationChanges[0].SourceArtifactId, "Changeset #2 had wrong Association Source ID.");
			Assert.AreEqual(1, test3[1].AssociationChanges[0].SourceArtifactTypeId, "Changeset #2 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test3[1].AssociationChanges[0].DestArtifactId, "Changeset #2 had wrong Association Destination ID.");
			Assert.AreEqual(2, test3[1].AssociationChanges[0].DestArtifactTypeId, "Changeset #2 had wrong Association Destination Type.");
			#endregion Add Multiple RQs to TC 

			#region Add Multiple TCs to RQ 
			tMgr.AddToRequirement(projectId, reqId1, new List<int> { tcId1, tcId2, tcId3 }, 2);
			var test4 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderByDescending(g => g.ChangeSetId).ToList();
			Assert.AreEqual(5, test4.Count, "Number of Changesets not expected.");

			//Grab the last one. 
			test4 = new List<HistoryChangeSet> { test4.FirstOrDefault() };
			Assert.AreEqual(3, test4[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(reqId1, test4[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test4[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test4[0].ArtifactId, test4[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(1, test4[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId1), "Associations had incorrect # for TC#1.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId2), "Associations had incorrect # for TC#2.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId3), "Associations had incorrect # for TC#3.");
			Assert.AreEqual(3, test4[0].AssociationChanges.Count(g => g.DestArtifactTypeId == 2), "Associations had incorrect # for TCs.");

			//Also verify that the squashed artifact history has the data as well
			List<HistoryChangeSetResponse> changes = hMgr.RetrieveByChangeSetId(projectId, test4[0].ChangeSetId, null, true, null, 1, Int32.MaxValue, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, changes.Count);
			//Assert.AreEqual("_Association", changes[0].FieldName);
			//Assert.AreEqual("_Association", changes[1].FieldName);
			//Assert.AreEqual("_Association", changes[2].FieldName);

			//Now remove the associations. 
			tMgr.RemoveFromRequirement(projectId, reqId1, new List<int> { tcId1, tcId2, tcId3 }, 1);
			var test5 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderByDescending(g => g.ChangeSetId).ToList();
			Assert.AreEqual(6, test5.Count, "Number of Changesets not expected.");

			//Grab the last one. 
			test5 = new List<HistoryChangeSet> { test4.FirstOrDefault() };
			Assert.AreEqual(3, test5[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(reqId1, test5[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test5[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test5[0].ArtifactId, test5[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(1, test5[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId1), "Associations had incorrect # for TC#1.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId2), "Associations had incorrect # for TC#2.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId3), "Associations had incorrect # for TC#3.");
			Assert.AreEqual(3, test5[0].AssociationChanges.Count(g => g.DestArtifactTypeId == 2), "Associations had incorrect # for TC#1.");

			//Also verify that the squashed artifact history has the data as well
			changes = hMgr.RetrieveByChangeSetId(projectId, test5[0].ChangeSetId, null, true, null, 1, Int32.MaxValue, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, changes.Count);
			//Assert.AreEqual("_Association", changes[0].FieldName);
			//Assert.AreEqual("_Association", changes[1].FieldName);
			//Assert.AreEqual("_Association", changes[2].FieldName);

			#endregion Add Multiple TCs to RQ 

			#region Add Test Step to Requirement 
			//At this point, there should be no associations at all. So, add a test step. 
			rMgr.RequirementTestStep_AddToTestStep(projectId, 1, ts1id, new List<int> { reqId1, reqId2 }, false);
			var test6 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).OrderByDescending(g => g.ChangeSetId).ToList();
			Assert.AreEqual(8, test6.Count, "Changeset #1 did not have correct # of Associations!");
			test6 = test6.Take(2).ToList();
			Assert.AreEqual(1, test6[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(reqId2, test6[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test6[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test6[0].ArtifactId, test6[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(1, test6[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test6[0].AssociationChanges[0].DestArtifactId, "Changeset #1 had wrong Association Destination ID.");
			Assert.AreEqual(2, test6[0].AssociationChanges[0].DestArtifactTypeId, "Changeset #1 had wrong Association Destination Type.");
			Assert.AreEqual(1, test6[1].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(reqId1, test6[1].ArtifactId, "Changeset #2 had wrong RQ ID.");
			Assert.AreEqual(2, test6[1].ArtifactTypeId, "Changeset #2 had wrong RQ Type.");
			Assert.AreEqual(test6[1].ArtifactId, test6[1].AssociationChanges[0].SourceArtifactId, "Changeset #2 had wrong Association Source ID.");
			Assert.AreEqual(1, test6[1].AssociationChanges[0].SourceArtifactTypeId, "Changeset #2 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test6[1].AssociationChanges[0].DestArtifactId, "Changeset #2 had wrong Association Destination ID.");
			Assert.AreEqual(2, test6[1].AssociationChanges[0].DestArtifactTypeId, "Changeset #2 had wrong Association Destination Type.");
			#endregion Add Test Step to Requirement 
		}


		/// <summary>Tests different methods to create and remove Release Test Coverage, verifying the History is recorded properly.</summary> 
		[Test]
		[SpiraTestCase(2554)]
		[Description("Tests different methods to create and remove Release Test Coverage, verifying the History is recorded properly.")]
		public void _20_Association_ReleaseTestCoverage()
		{
			/* 
			 * Some notes to this test. You will note here that I am not checking HistoryChangeSet properties, like 
			 * the date, the author, etc. This is because all this information should be being checked in the history 
			 * tests above, and I felt that (1) It could make this test do a lot more than it has to, and (2) When this 
			 * test fails making the finding of the failing check easier. 
			 */

			//Our mnanagers needed. 
			HistoryManager hMgr = new HistoryManager();
			ReleaseManager rMgr = new ReleaseManager();
			TestCaseManager tMgr = new TestCaseManager();

			#region Setup 
			//We need to create our releases, first. (Copied from _06_IncidentChanges)
			int relId1 = rMgr.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_FRED_BLOGGS,
				"Test Release 1",
				null,
				"1.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);
			Assert.IsTrue(relId1 > 0, "Could not create needed Rel1.");

			int relId2 = rMgr.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_JOE_SMITH,
				"Test Release 2",
				null,
				"2.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Iteration,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);
			Assert.IsTrue(relId2 > 0, "Could not create needed Rel2.");

			int relId3 = rMgr.Insert(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_JOE_SMITH,
				"Test Release 3",
				null,
				"3.0.0.0",
				(int?)null,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.MajorRelease,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);
			Assert.IsTrue(relId3 > 0, "Could not create needed Rel3.");

			int relId4 = rMgr.InsertChild(
				USER_ID_FRED_BLOGGS,
				projectId,
				USER_ID_JOE_SMITH,
				"Test Release 3A",
				null,
				"3.1.0.0",
				relId3,
				Release.ReleaseStatusEnum.Planned,
				Release.ReleaseTypeEnum.Phase,
				DateTime.UtcNow,
				DateTime.UtcNow.AddDays(14),
				3,
				2,
				null);
			Assert.IsTrue(relId4 > 0, "Could not create needed Rel4.");

			//Create the test case. (Copied from test _10_CycleTest) 
			List<TestCasePriority> priorities = tMgr.TestCasePriority_Retrieve(projectTemplateId);
			int tcId1 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #1 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _20_Association_ReleaseTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			int tcId2 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #2 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _20_Association_ReleaseTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			int tcId3 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #3 for RequirementTestCoverage",
				"This is a test Test Case created from the HistoryTest NUnit test. _20_Association_ReleaseTestCoverage()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			#endregion Setup 

			//Turn baselines off for the project.
			var settings = new ProjectSettings(projectId);
			settings.BaseliningEnabled = false;
			settings.Save(USER_ID_SYS_ADMIN);

			//First, let us double-check that we have no  
			int initalNumber = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId).Count(f => f.ArtifactId == relId1 || f.ArtifactId == relId2);
			Assert.AreEqual(0, initalNumber, "There were already associations. Can not run this test as it is unexpectd!");

			//Baselines should be turned off. So, make sure that NOTHING is recorded when we associate something! 
			rMgr.AddToTestCase(projectId, tcId1, new List<int> { relId1, relId2 }, USER_ID_SYS_ADMIN);
			rMgr.RemoveFromTestCase(projectId, tcId1, new List<int> { relId1, relId2 }, USER_ID_SYS_ADMIN);
			var test1 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1 || f.ArtifactId == relId2)
				.OrderBy(g => g.ArtifactId)
				.ToList();
			Assert.AreEqual(0, test1.Count, "Associations were recorded when Baselines were disabled!");

			//Now enable baselines in this project. 
			settings.BaseliningEnabled = true;
			settings.Save(USER_ID_SYS_ADMIN);

			#region Add Multiple RLs to TC 
			//Re-add those test cases. 
			rMgr.AddToTestCase(projectId, tcId1, new List<int> { relId1, relId2 }, 1);

			//Now verify we have that entry. 
			var test2 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1 || f.ArtifactId == relId2)
				.OrderBy(g => g.ArtifactId)
				.ToList();
			Assert.AreEqual(2, test2.Count, "Number of Changesets were not expected.");
			Assert.AreEqual(1, test2[0].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(relId1, test2[0].ArtifactId, "Changeset #1 had wrong RL ID.");
			Assert.AreEqual(2, test2[0].ArtifactTypeId, "Changeset #1 had wrong RL Type.");
			Assert.AreEqual(test2[0].ArtifactId, test2[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(4, test2[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test2[0].AssociationChanges[0].DestArtifactId, "Changeset #1 had wrong Association Destination ID.");
			Assert.AreEqual(2, test2[0].AssociationChanges[0].DestArtifactTypeId, "Changeset #1 had wrong Association Destination Type.");
			Assert.AreEqual(1, test2[1].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(relId2, test2[1].ArtifactId, "Changeset #2 had wrong RL ID.");
			Assert.AreEqual(2, test2[1].ArtifactTypeId, "Changeset #2 had wrong RL Type.");
			Assert.AreEqual(test2[1].ArtifactId, test2[1].AssociationChanges[0].SourceArtifactId, "Changeset #2 had wrong Association Source ID.");
			Assert.AreEqual(4, test2[1].AssociationChanges[0].SourceArtifactTypeId, "Changeset #2 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test2[1].AssociationChanges[0].DestArtifactId, "Changeset #2 had wrong Association Destination ID.");
			Assert.AreEqual(2, test2[1].AssociationChanges[0].DestArtifactTypeId, "Changeset #2 had wrong Association Destination Type.");

			//Now remove the associations. 
			rMgr.RemoveFromTestCase(projectId, tcId1, new List<int> { relId1, relId2 }, 1);

			//Verify our return records. 
			var test3 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1 || f.ArtifactId == relId2)
				.OrderBy(g => g.ArtifactId)
				.ToList();
			Assert.AreEqual(4, test3.Count, "Number of Changesets not expected.");
			//Now filter our changesets for only those two removed records. 
			test3 = test3.Where(g => g.ChangeTypeId == (int)ChangeSetTypeEnum.Association_Remove).ToList();
			Assert.AreEqual(2, test3.Count, "Number of filtered Changeset not expected.");
			Assert.AreEqual(1, test3[0].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(relId1, test3[0].ArtifactId, "Changeset #1 had wrong RL ID.");
			Assert.AreEqual(2, test3[0].ArtifactTypeId, "Changeset #1 had wrong RL Type.");
			Assert.AreEqual(test3[0].ArtifactId, test3[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(4, test3[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test3[0].AssociationChanges[0].DestArtifactId, "Changeset #1 had wrong Association Destination ID.");
			Assert.AreEqual(2, test3[0].AssociationChanges[0].DestArtifactTypeId, "Changeset #1 had wrong Association Destination Type.");
			Assert.AreEqual(1, test3[1].AssociationChanges.Count, "Changeset #1 did not have an Association!");
			Assert.AreEqual(relId2, test3[1].ArtifactId, "Changeset #2 had wrong RL ID.");
			Assert.AreEqual(2, test3[1].ArtifactTypeId, "Changeset #2 had wrong RL Type.");
			Assert.AreEqual(test3[1].ArtifactId, test3[1].AssociationChanges[0].SourceArtifactId, "Changeset #2 had wrong Association Source ID.");
			Assert.AreEqual(4, test3[1].AssociationChanges[0].SourceArtifactTypeId, "Changeset #2 had wrong Association Source Type.");
			Assert.AreEqual(tcId1, test3[1].AssociationChanges[0].DestArtifactId, "Changeset #2 had wrong Association Destination ID.");
			Assert.AreEqual(2, test3[1].AssociationChanges[0].DestArtifactTypeId, "Changeset #2 had wrong Association Destination Type.");
			#endregion Add Multiple RLs to TC 

			#region Add Multiple TCs to RL 
			tMgr.AddToRelease(projectId, relId1, new List<int> { tcId1, tcId2, tcId3 }, 2);
			var test4 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1)
				.OrderByDescending(g => g.ChangeSetId)
				.ToList();
			Assert.AreEqual(3, test4.Count, "Number of Changesets not expected.");

			//Grab the last one. 
			test4 = new List<HistoryChangeSet> { test4.FirstOrDefault() };
			Assert.AreEqual(3, test4[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(relId1, test4[0].ArtifactId, "Changeset #1 had wrong RL ID.");
			Assert.AreEqual(2, test4[0].ArtifactTypeId, "Changeset #1 had wrong RL Type.");
			Assert.AreEqual(test4[0].ArtifactId, test4[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(4, test4[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId1), "Associations had incorrect # for TC#1.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId2), "Associations had incorrect # for TC#2.");
			Assert.AreEqual(1, test4[0].AssociationChanges.Count(g => g.DestArtifactId == tcId3), "Associations had incorrect # for TC#3.");
			Assert.AreEqual(3, test4[0].AssociationChanges.Count(g => g.DestArtifactTypeId == 2), "Associations had incorrect # for TCs.");

			//Now remove the associations. 
			tMgr.RemoveFromRelease(projectId, relId1, new List<int> { tcId1, tcId2, tcId3 }, USER_ID_SYS_ADMIN);
			var test5 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1)
				.OrderByDescending(g => g.ChangeSetId)
				.ToList();
			Assert.AreEqual(4, test5.Count, "Number of Changesets not expected.");

			//Grab the last one. 
			test5 = new List<HistoryChangeSet> { test5.FirstOrDefault() };
			Assert.AreEqual(3, test5[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(relId1, test5[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(2, test5[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test5[0].ArtifactId, test5[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(4, test5[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId1), "Associations had incorrect # for TC#1.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId2), "Associations had incorrect # for TC#2.");
			Assert.AreEqual(1, test5[0].AssociationChanges.Count(g => g.DestArtifactId == tcId3), "Associations had incorrect # for TC#3.");
			Assert.AreEqual(3, test5[0].AssociationChanges.Count(g => g.DestArtifactTypeId == 2), "Associations had incorrect # for TCs.");
			#endregion Add Multiple TCs to RL 

			#region Test Requirement Release Change
			//First, create a requirement to assign to our Releases.
			RequirementManager rqMgr = new RequirementManager();
			List<Importance> requirementImportances = rqMgr.RequirementImportance_Retrieve(projectTemplateId);
			int reqId1 = rqMgr.Insert(
				4,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Accepted,
				null,
				4,
				2,
				requirementImportances.FirstOrDefault(r => r.Score == 1).ImportanceId,
				"HistoryTest Requirement #1 for ReleaseTestCoverage",
				"This is a test requirement created from the HistoryTest NUnit test. _20_Association_ReleaseTestCoverage()",
				450,
				1
				);
			Assert.IsTrue(reqId1 > 0, "Could not create needed RQ1.");

			// Add a test case to the RQ.
			rqMgr.AddToTestCase(projectId, tcId1, new List<int> { reqId1 }, USER_ID_SYS_ADMIN);

			//Update the requirment - assign it to a release. This should AUTOMATICALLY assign the Test Case to a release.
			Requirement rq1 = rqMgr.RetrieveById3(null, reqId1);
			rq1.StartTracking();
			rq1.ReleaseId = relId1;
			rqMgr.Update(USER_ID_SYS_ADMIN, projectId, new List<Requirement> { rq1 });

			//Now check if the test case is also assigned to the Release.
			var test6 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId1)
				.OrderByDescending(g => g.ChangeSetId)
				.ToList();
			Assert.AreEqual(5, test6.Count, "Number of Changesets not expected.");
			Assert.AreEqual(1, test6[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(relId1, test6[0].ArtifactId, "Changeset #1 had wrong RQ ID.");
			Assert.AreEqual(4, test6[0].ArtifactTypeId, "Changeset #1 had wrong RQ Type.");
			Assert.AreEqual(test6[0].ArtifactId, test4[0].AssociationChanges[0].SourceArtifactId, "Changeset #1 had wrong Association Source ID.");
			Assert.AreEqual(4, test6[0].AssociationChanges[0].SourceArtifactTypeId, "Changeset #1 had wrong Association Source Type.");
			Assert.AreEqual(1, test6[0].AssociationChanges.Count(g => g.DestArtifactId == tcId1), "Associations had incorrect # for TC#1.");
			Assert.AreEqual(1, test6[0].AssociationChanges.Count(g => g.DestArtifactTypeId == 2), "Associations had incorrect # for TCs.");

			//Now update the requirement - assign it to a Sprint. (Release #4, which is a child of Release #3)
			rq1 = rqMgr.RetrieveById3(null, reqId1);
			rq1.StartTracking();
			rq1.ReleaseId = relId4;
			rqMgr.Update(USER_ID_SYS_ADMIN, projectId, new List<Requirement> { rq1 });

			//Now check if the test case is also assigned to the Release.
			var test7 = hMgr.TestingOnly_RetrieveChangesetsWithAssociations(projectId)
				.Where(f => f.ArtifactId == relId4 || f.ArtifactId == relId3)
				.OrderByDescending(g => g.ChangeSetId)
				.Take(2)
				.ToList();
			Assert.AreEqual(2, test7.Count, "This really should not have failed. Count of Changesets was not 2.");
			Assert.AreEqual(1, test7[0].AssociationChanges.Count, "Changeset #1 did not have correct # of Associations!");
			Assert.AreEqual(1, test7[1].AssociationChanges.Count, "Changeset #2 did not have correct # of Associations!");
			Assert.AreEqual(1, test7.Count(c => c.ArtifactId == relId4), "Wrong number of Changesets for Rel #4");
			Assert.AreEqual(1, test7.Count(c => c.ArtifactId == relId3), "Wrong number of Changesets for Rel #3");
			Assert.AreEqual(2, test7.Count(c => c.ArtifactTypeId == 4), "Wrong number of changesets with wrong Art Type.");
			Assert.AreEqual(2, test7.Count(c => c.ArtifactId == c.AssociationChanges[0].SourceArtifactId), "Artifact Id of Changesets did not match Association IDs.");
			Assert.AreEqual(2, test7.Count(c => c.AssociationChanges.Any(a => a.SourceArtifactTypeId == 4)), "Changesets had wrong Association Source Type.");
			Assert.AreEqual(2, test7.Count(c => c.AssociationChanges.Any(a => a.DestArtifactId == tcId1)), "Associations had incorrect # for TC#1.");

			#endregion Test Requirement Release Change
		}


		/// <summary>
		/// Tests recording positional changes in Test Steps in a Test Case. This includes: Adding a new step, moving an existing step, and purging a deleted step.
		/// </summary>
		[Test]
		[SpiraTestCase(2716)]
		[Description("Tests recording positional changes in Test Steps in a Test Case. This includes: Adding a new step, moving an existing step, and purging a deleted step.")]
		public void _21_Position_TestStepChanges()
		{
			/* 
			 * Some notes to this test. You will note here that I am not checking HistoryChangeSet properties, like 
			 * the date, the author, etc. This is because all this information should be being checked in the history 
			 * tests above, and I felt that (1) It could make this test do a lot more than it has to, and (2) When this 
			 * test fails making the finding of the failing check easier.  (Same as previous History Details tests.)
			 */

			//Our mnanagers needed. 
			HistoryManager hMgr = new HistoryManager();
			TestCaseManager tMgr = new TestCaseManager();

			#region Setup
			// Make sure baselining is turned off for this project.
			ProjectSettings prjSet = new ProjectSettings(projectId);
			prjSet.BaseliningEnabled = false;
			prjSet.Save(USER_ID_SYS_ADMIN);

			//Create our Test Case.
			List<TestCasePriority> priorities = tMgr.TestCasePriority_Retrieve(projectTemplateId);
			int tcId1 = tMgr.Insert(
				1,
				projectId,
				2,
				3,
				"HistoryTest Test Case #1 for Test Case Positon Changes",
				"This is a test Test Case created from the HistoryTest NUnit test. _21_Position_TestStepChanges()",
				null,
				TestCase.TestCaseStatusEnum.ReadyForTest,
				priorities.FirstOrDefault(p => p.Score == 2).TestCasePriorityId,
				null,
				150,
				null,
				null);
			int ts1id = tMgr.InsertStep(
				1,
				tcId1,
				null,
				"Step 1",
				"None",
				"Do this!");
			#endregion Setup

			//Let's add a test case, move it, and then delete and purge it. There should be no entries in out Position table.
			int ts2id = tMgr.InsertStep(
				1,
				tcId1,
				null,
				"Step 2",
				"None",
				"Do this!");
			tMgr.MoveStep(tcId1, ts2id, ts1id, USER_ID_FRED_BLOGGS);
			tMgr.DeleteStepFromDatabase(USER_ID_SYS_ADMIN, ts2id, true);

			//Now check that there are no history records with positional data.
			var histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId);
			Assert.AreEqual(0, histItems.Count, "Position changes were recorded when they should not have been!");

			//Now enable our baseline recording..
			prjSet.BaseliningEnabled = true;
			prjSet.Save(USER_ID_SYS_ADMIN);

			//Now we do the same three steps..
			ts2id = tMgr.InsertStep(
				1,
				tcId1,
				null,
				"Step 2",
				"None",
				"Do this!"); // This should create 1 HistSet with 1 entry (TS2 pos -1 to 2)
			tMgr.MoveStep(tcId1, ts2id, ts1id, USER_ID_SYS_ADMIN); // This should create 1 HistSet with 2 entries (TS1 pos 1 to 2, TS2 pos 2 to 1)
			tMgr.DeleteStepFromDatabase(USER_ID_SYS_ADMIN, ts2id, true); // This should create 1 HistSet with 2 entries (TS1 pos 2 to 1, TS2 pos 1 to -1)

			//Check our values.
			histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId);
			Assert.AreEqual(3, histItems.Count, "The number of positional changes was incorrect!");
			Assert.AreEqual(2, histItems.Where(f => f.PositionChanges.Count == 2).Count(), "There was not a position history set with 2 changes!");
			Assert.AreEqual(1, histItems.Where(f => f.PositionChanges.Count == 1).Count(), "There was not a position history set with 1 changes!");
			//Get each changeset.
			HistoryChangeSet hist1 = histItems.OrderBy(h => h.ChangeSetId).FirstOrDefault();
			HistoryChangeSet hist2 = histItems.OrderBy(h => h.ChangeSetId).Skip(1).FirstOrDefault();
			HistoryChangeSet hist3 = histItems.OrderBy(h => h.ChangeSetId).Skip(2).FirstOrDefault();
			// The first history entry is our creation. Should have one entry, TS2 - from -1 to 2.
			Assert.AreEqual(1,
				hist1.PositionChanges.Count,
				"The Test Step creation did not have correct number of position recordings. (1 expected.)");
			Assert.AreEqual(-1,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().OldPosition,
				"The Test Step creation did not have proper original ID! (-1 expected.)");
			Assert.AreEqual(1,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().NewPosition,
				"The Test Step creation did not have proper original ID! (2 expected.)");
			Assert.AreEqual(ts2id,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactId,
				"The Test Step creation did not have proper Test Step ID.");
			Assert.AreEqual((int)ArtifactTypeEnum.TestStep,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactTypeId,
				"The Test Step creation did not have proper Test Step Type ID.");
			// The second history entry should have two entries. TS1 1 to 2. TS2 2 to 1.
			Assert.AreEqual(2,
				hist2.PositionChanges.Count,
				"The Test Step move did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == ts1id),
				"The Test Step move did not have correct number of position recordings for TS1. (1 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == ts2id),
				"The Test Step move did not have correct number of position recordings for TS2. (1 expected.)");
			Assert.AreEqual(0,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts1id).OldPosition,
				"The Test Step move did not have proper Old Position for TS1. (Expected 1)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts1id).NewPosition,
				"The Test Step move did not have proper New Position for TS1. (Expected 2)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts2id).OldPosition,
				"The Test Step move did not have proper Old Position for TS2. (Expected 2)");
			Assert.AreEqual(0,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts2id).NewPosition,
				"The Test Step move did not have proper New Position for TS2. (Expected 1)");
			// The third history entry should have two entries. TS1 2 to 1. TS2 1 to -1.
			Assert.AreEqual(2,
				hist3.PositionChanges.Count,
				"The Test Step purge did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == ts1id),
				"The Test Step purge did not have correct number of position recordings for TS1. (1 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == ts2id),
				"The Test Step purge did not have correct number of position recordings for TS2. (1 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts1id).OldPosition,
				"The Test Step purge did not have proper Old Position for TS1. (Expected 2)");
			Assert.AreEqual(0,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts1id).NewPosition,
				"The Test Step purge did not have proper New Position for TS1. (Expected 1)");
			Assert.AreEqual(0,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts2id).OldPosition,
				"The Test Step purge did not have proper Old Position for TS2. (Expected 1)");
			Assert.AreEqual(-1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == ts2id).NewPosition,
				"The Test Step purge did not have proper New Position for TS2. (Expected -1)");

			//Also verify that the squashed artifact history has the data as well
			List<HistoryChangeSetResponse> changes = hMgr.RetrieveByChangeSetId(projectId, hist3.ChangeSetId, null, true, null, 1, Int32.MaxValue, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, changes.Count);
			//Assert.AreEqual("_Position", changes[0].FieldName);
			//Assert.AreEqual("_Position", changes[1].FieldName);
		}

		/// <summary>
		/// Tests recording positional changes in Risk Mitigations in a Risk. This includes: Adding a new mitigation, moving an existing mitigation, and purging a deleted mitigation.
		/// </summary>
		[Test]
		[SpiraTestCase(2717)]
		[Description("Tests recording positional changes in Risk Mitigations in a Risk. This includes: Adding a new mitigation, moving an existing mitigation, and purging a deleted mitigation.")]
		public void _22_Position_RiskMitigationChanges()
		{
			/* 
			 * Some notes to this test. You will note here that I am not checking HistoryChangeSet properties, like 
			 * the date, the author, etc. This is because all this information should be being checked in the history 
			 * tests above, and I felt that (1) It could make this test do a lot more than it has to, and (2) When this 
			 * test fails making the finding of the failing check easier.  (Same as previous History Details tests.)
			 */

			//Our mnanagers needed. 
			HistoryManager hMgr = new HistoryManager();
			RiskManager rMgr = new RiskManager();

			#region Setup
			// Make sure baselining is turned off for this project.
			ProjectSettings prjSet = new ProjectSettings(projectId);
			prjSet.BaseliningEnabled = false;
			prjSet.Save(USER_ID_JOE_SMITH);

			//Create our Risk.
			int rId1 = rMgr.Risk_Insert(
				projectId,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				"HistoryTest Risk #1 for Mitigation Positon Changes",
				"This is a test Risk created from the HistoryTest NUnit test. _22_Position_RiskMitigationChanges()",
				null,
				null,
				DateTime.UtcNow,
				null,
				null);
			int mId1 = rMgr.RiskMitigation_Insert(
				projectId,
				rId1,
				null,
				"Mitigation 1",
				USER_ID_SYS_ADMIN);
			#endregion Setup

			//Let's add another Mitigation, move it, and then delete and purge it. There should be no entries in out Position table.
			int mId2 = rMgr.RiskMitigation_Insert(
				projectId,
				rId1,
				null,
				"Mitigation 2",
				USER_ID_SYS_ADMIN);
			rMgr.RiskMitigation_Move(rId1, mId2, mId1, USER_ID_SYS_ADMIN);
			rMgr.RiskMitigation_Purge(projectId, mId2, USER_ID_JOE_SMITH);

			//Now check that there are no history records with positional data.
			var histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId).Where(f => f.ArtifactTypeId == (int)ArtifactTypeEnum.Risk).ToList();
			Assert.AreEqual(0, histItems.Count, "Position changes were recorded when they should not have been!");

			//Now enable our baseline recording..
			prjSet.BaseliningEnabled = true;
			prjSet.Save(USER_ID_SYS_ADMIN);

			//Now we do the same three steps..
			mId2 = rMgr.RiskMitigation_Insert(
				projectId,
				rId1,
				null,
				"Mitigation 2",
				USER_ID_SYS_ADMIN); // This should create 1 HistSet with 1 entry (M2 pos -1 to 2)
			rMgr.RiskMitigation_Move(rId1, mId2, mId1, USER_ID_SYS_ADMIN); // This should create 1 HistSet with 2 entries (M1 pos 1 to 2, M2 pos 2 to 1)
			rMgr.RiskMitigation_Purge(projectId, mId2, USER_ID_JOE_SMITH); // This should create 1 HistSet with 2 entries (M1 pos 2 to 1, M2 pos 1 to -1)

			//Check our values.
			histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId).Where(f => f.ArtifactTypeId == (int)ArtifactTypeEnum.Risk).ToList();
			Assert.AreEqual(3, histItems.Count, "The number of positional changes was incorrect!");
			Assert.AreEqual(2, histItems.Where(f => f.PositionChanges.Count == 2).Count(), "There was not a position history set with 2 changes!");
			Assert.AreEqual(1, histItems.Where(f => f.PositionChanges.Count == 1).Count(), "There was not a position history set with 1 changes!");
			//Get each changeset.
			HistoryChangeSet hist1 = histItems.OrderBy(h => h.ChangeSetId).FirstOrDefault();
			HistoryChangeSet hist2 = histItems.OrderBy(h => h.ChangeSetId).Skip(1).FirstOrDefault();
			HistoryChangeSet hist3 = histItems.OrderBy(h => h.ChangeSetId).Skip(2).FirstOrDefault();
			// The first history entry is our creation. Should have one entry, M2 - from -1 to 2.
			Assert.AreEqual(1,
				hist1.PositionChanges.Count,
				"The Mitigation creation did not have correct number of position recordings. (1 expected.)");
			Assert.AreEqual(-1,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().OldPosition,
				"The Mitigation creation did not have proper original ID! (-1 expected.)");
			Assert.AreEqual(2,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().NewPosition,
				"The Mitigation creation did not have proper original ID! (2 expected.)");
			Assert.AreEqual(mId2,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactId,
				"The Mitigation creation did not have proper Mitigation ID.");
			Assert.AreEqual((int)ArtifactTypeEnum.RiskMitigation,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactTypeId,
				"The Mitigation creation did not have proper Mitigation Type ID.");
			// The second history entry should have two entries. M1 1 to 2. M2 2 to 1.
			Assert.AreEqual(2,
				hist2.PositionChanges.Count,
				"The Mitigation move did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == mId1),
				"The Mitigation move did not have correct number of position recordings for Mit1. (1 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == mId2),
				"The Mitigation move did not have correct number of position recordings for Mit2. (1 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId1).OldPosition,
				"The Mitigation move did not have proper Old Position for Mit1. (Expected 1)");
			Assert.AreEqual(2,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId1).NewPosition,
				"The Mitigation move did not have proper New Position for Mit1. (Expected 2)");
			Assert.AreEqual(2,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId2).OldPosition,
				"The Mitigation move did not have proper Old Position for Mit2. (Expected 2)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId2).NewPosition,
				"The Mitigation move did not have proper New Position for Mit2. (Expected 1)");
			// The third history entry should have two entries. M1 2 to 1. M2 1 to -1.
			Assert.AreEqual(2,
				hist3.PositionChanges.Count,
				"The Mitigation purge did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == mId1),
				"The Mitigation purge did not have correct number of position recordings for TS1. (1 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == mId2),
				"The Mitigation purge did not have correct number of position recordings for TS2. (1 expected.)");
			Assert.AreEqual(2,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId1).OldPosition,
				"The Mitigation purge did not have proper Old Position for Mit1. (Expected 2)");
			Assert.AreEqual(1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId1).NewPosition,
				"The Mitigation purge did not have proper New Position for Mit1. (Expected 1)");
			Assert.AreEqual(1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId2).OldPosition,
				"The Mitigation purge did not have proper Old Position for Mit2. (Expected 1)");
			Assert.AreEqual(-1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == mId2).NewPosition,
				"The Mitigation purge did not have proper New Position for Mit2. (Expected -1)");
		}

		/// <summary>
		/// Tests recording positional changes in Use Case in a Requirement. This includes: Adding a new use case, moving an existing use case, and purging a deleted use case.
		/// </summary>
		[Test]
		[SpiraTestCase(2718)]
		[Description("Tests recording positional changes in Use Case in a Requirement. This includes: Adding a new use case, moving an existing use case, and purging a deleted use case.")]
		public void _23_Position_ReqUseCaseChanges()
		{
			/* 
			 * Some notes to this test. You will note here that I am not checking HistoryChangeSet properties, like 
			 * the date, the author, etc. This is because all this information should be being checked in the history 
			 * tests above, and I felt that (1) It could make this test do a lot more than it has to, and (2) When this 
			 * test fails making the finding of the failing check easier.  (Same as previous History Details tests.)
			 */

			//Our mnanagers needed. 
			HistoryManager hMgr = new HistoryManager();
			RequirementManager rMgr = new RequirementManager();

			#region Setup
			// Make sure baselining is turned off for this project.
			ProjectSettings prjSet = new ProjectSettings(projectId);
			prjSet.BaseliningEnabled = false;
			prjSet.Save(USER_ID_JOE_SMITH);

			//Create our Risk.
			int rId1 = rMgr.Insert(
				USER_ID_SYS_ADMIN,
				projectId,
				null,
				null,
				(int?)null,
				Requirement.RequirementStatusEnum.Planned,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				null,
				"HistoryTest Risk #1 for Use Case Positon Changes",
				"This is a test Risk created from the HistoryTest NUnit test. _23_Position_ReqUseCaseChanges()",
				null,
				USER_ID_SYS_ADMIN);
			int uId1 = rMgr.InsertStep(
				projectId,
				rId1,
				null,
				"Use Step 1",
				USER_ID_SYS_ADMIN);
			#endregion Setup

			//Let's add another Mitigation, move it, and then delete and purge it. There should be no entries in out Position table.
			int uId2 = rMgr.InsertStep(
				projectId,
				rId1,
				null,
				"Mitigation 2",
				USER_ID_SYS_ADMIN);
			rMgr.MoveStep(rId1, uId2, uId1, USER_ID_SYS_ADMIN);
			rMgr.PurgeStep(projectId, uId2, USER_ID_JOE_SMITH);

			//Now check that there are no history records with positional data.
			var histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId).Where(f => f.ArtifactTypeId == (int)ArtifactTypeEnum.Requirement).ToList();
			Assert.AreEqual(0, histItems.Count, "Position changes were recorded when they should not have been!");

			//Now enable our baseline recording..
			prjSet.BaseliningEnabled = true;
			prjSet.Save(USER_ID_JOE_SMITH);

			//Now we do the same three steps..
			uId2 = rMgr.InsertStep(
				projectId,
				rId1,
				null,
				"Mitigation 2b",
				USER_ID_SYS_ADMIN); // This should create 1 HistSet with 1 entry (UC2 pos -1 to 2)
			rMgr.MoveStep(rId1, uId2, uId1, USER_ID_SYS_ADMIN); // This should create 1 HistSet with 2 entries (UC1 pos 1 to 2, UC2 pos 2 to 1)
			rMgr.PurgeStep(projectId, uId2, USER_ID_JOE_SMITH); // This should create 1 HistSet with 2 entries (UC1 pos 2 to 1, UC2 pos 1 to -1)

			//Check our values.
			histItems = hMgr.TestingOnly_RetrieveChangesetsWithPositions(projectId).Where(f => f.ArtifactTypeId == (int)ArtifactTypeEnum.Requirement).ToList();
			Assert.AreEqual(3, histItems.Count, "The number of positional changes was incorrect!");
			Assert.AreEqual(2, histItems.Where(f => f.PositionChanges.Count == 2).Count(), "There was not a position history set with 2 changes!");
			Assert.AreEqual(1, histItems.Where(f => f.PositionChanges.Count == 1).Count(), "There was not a position history set with 1 changes!");
			//Get each changeset.
			HistoryChangeSet hist1 = histItems.OrderBy(h => h.ChangeSetId).FirstOrDefault();
			HistoryChangeSet hist2 = histItems.OrderBy(h => h.ChangeSetId).Skip(1).FirstOrDefault();
			HistoryChangeSet hist3 = histItems.OrderBy(h => h.ChangeSetId).Skip(2).FirstOrDefault();
			// The first history entry is our creation. Should have one entry, UC2 - from -1 to 2.
			Assert.AreEqual(1,
				hist1.PositionChanges.Count,
				"The Use Case creation did not have correct number of position recordings. (1 expected.)");
			Assert.AreEqual(-1,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().OldPosition,
				"The Use Case creation did not have proper original ID! (-1 expected.)");
			Assert.AreEqual(2,
				hist1.PositionChanges.OrderBy(f => f.HistoryPositionId).FirstOrDefault().NewPosition,
				"The Use Case creation did not have proper original ID! (2 expected.)");
			Assert.AreEqual(uId2,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactId,
				"The Use Case creation did not have proper Use Case ID.");
			Assert.AreEqual((int)ArtifactTypeEnum.RequirementStep,
				hist1.PositionChanges.FirstOrDefault().ChildArtifactTypeId,
				"The Use Case creation did not have proper Use Case Type ID.");
			// The second history entry should have two entries. UC1 1 to 2. UC2 2 to 1.
			Assert.AreEqual(2,
				hist2.PositionChanges.Count,
				"The Use Case move did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == uId1),
				"The Use Case move did not have correct number of position recordings for UC1. (1 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.Count(f => f.ChildArtifactId == uId2),
				"The Use Case move did not have correct number of position recordings for UC2. (1 expected.)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId1).OldPosition,
				"The Use Case move did not have proper Old Position for UC1. (Expected 1)");
			Assert.AreEqual(2,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId1).NewPosition,
				"The Use Case move did not have proper New Position for UC1. (Expected 2)");
			Assert.AreEqual(2,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId2).OldPosition,
				"The Use Case move did not have proper Old Position for UC2. (Expected 2)");
			Assert.AreEqual(1,
				hist2.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId2).NewPosition,
				"The Use Case move did not have proper New Position for UC2. (Expected 1)");
			// The third history entry should have two entries. UC1 2 to 1. UC2 1 to -1.
			Assert.AreEqual(2,
				hist3.PositionChanges.Count,
				"The Use Case purge did not have correct number of position recordings. (2 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == uId1),
				"The Use Case purge did not have correct number of position recordings for UC1. (1 expected.)");
			Assert.AreEqual(1,
				hist3.PositionChanges.Count(f => f.ChildArtifactId == uId2),
				"The Use Case purge did not have correct number of position recordings for UC2. (1 expected.)");
			Assert.AreEqual(2,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId1).OldPosition,
				"The Use Case purge did not have proper Old Position for UC1. (Expected 2)");
			Assert.AreEqual(1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId1).NewPosition,
				"The Use Case purge did not have proper New Position for UC1. (Expected 1)");
			Assert.AreEqual(1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId2).OldPosition,
				"The Use Case purge did not have proper Old Position for UC2. (Expected 1)");
			Assert.AreEqual(-1,
				hist3.PositionChanges.FirstOrDefault(f => f.ChildArtifactId == uId2).NewPosition,
				"The Use Case purge did not have proper New Position for UC2. (Expected -1)");
		}

		/// <summary>
		/// Tests the recording of changing a Project Setting.
		/// </summary>
		[Test]
		[SpiraTestCase(2720)]
		[Description("Tests the recording of changing a Project Setting.")]
		public void _24_ProjectSettingChanges()
		{
			//Our manager.
			HistoryManager hMgr = new HistoryManager();

			//Get the DateTime - this is just to filter out any records that have been created from previous tests.
			long csId;
			hMgr.RetrieveLatestSetId(out csId);

			//Get the project settings.
			var projSettings = new ProjectSettings(PROJECT_ID);

			//Change a settings.
			projSettings.BaseliningEnabled = !projSettings.BaseliningEnabled;
			projSettings.Save(USER_ID_SYS_ADMIN);
			var histList = hMgr.RetrieveByArtifactId(1, ArtifactTypeEnum.Project).Where(h => h.ChangeSetId > csId).ToList();
			Assert.AreEqual(1, histList.Count(), "The wrong number of History Items were returned.");
			Assert.AreEqual(1, histList[0].ProjectId, "Project ID did not match!");
			Assert.AreEqual(histList[0].ProjectId, histList[0].ArtifactId, "Artifact ID did not match!");
			//Assert.AreEqual(1, histList[0].Details.Count, "Wrong number of field changes detected.");
			//Assert.AreEqual("BaseliningEnabled", histList[0].Details[0].FieldName, "Field Name did not match.");
			//Assert.AreEqual(
			//	projSettings.BaseliningEnabled.ToString().ToLowerInvariant(),
			//	histList[0].Details[0].NewValue.ToLowerInvariant(),
			//	"New Value was not correct!");
			//Assert.AreEqual(
			//	(!projSettings.BaseliningEnabled).ToString().ToLowerInvariant(),
			//	histList[0].Details[0].OldValue.ToLowerInvariant(),
			//	"New Value was not correct!");
			Assert.AreEqual(USER_ID_SYS_ADMIN, histList[0].UserId, "Wrong user was recorded!");
			Assert.AreEqual((int)ChangeSetTypeEnum.Modified, histList[0].ChangeTypeId, "Wrong change type recorded!");

			//Change a setting to the same setting. This should NOT create an entry.
			projSettings.DisplayHoursOnPlanningBoard = projSettings.DisplayHoursOnPlanningBoard;
			projSettings.Save(USER_ID_FRED_BLOGGS);
			histList = hMgr.RetrieveByArtifactId(1, ArtifactTypeEnum.Project).Where(h => h.ChangeSetId > csId).ToList();
			Assert.AreEqual(1, histList.Count(), "The wrong number of History Items were returned.");

			//Change two settings. Verify both are recorded in the same ChangeSet.
			projSettings.DisplayHoursOnPlanningBoard = !projSettings.DisplayHoursOnPlanningBoard;
			projSettings.Testing_CreateDefaultTestStep = !projSettings.Testing_CreateDefaultTestStep;
			projSettings.Save(USER_ID_JOE_SMITH);
			csId++;
			histList = hMgr.RetrieveByArtifactId(1, ArtifactTypeEnum.Project).Where(h => h.ChangeSetId > csId).ToList();
			Assert.AreEqual(2, histList.Count(), "The wrong number of History Items were returned.");
			//Assert.AreEqual(2, histList[0].Details.Count(), "Wrong number of History Items were retrned.");
			Assert.AreEqual(USER_ID_JOE_SMITH, histList[1].UserId, "Wrong user was recorded!");
			Assert.AreEqual(1, histList[1].ProjectId, "Project ID did not match!");
			Assert.AreEqual(histList[1].ProjectId, histList[1].ArtifactId, "Artifact ID did not match!");
			Assert.AreEqual((int)ChangeSetTypeEnum.Modified, histList[1].ChangeTypeId, "Wrong change type recorded!");
			// Pull the two details for checking.
			//var det1 = histList[0].Details.Single(f => f.FieldName.Equals("DisplayHoursOnPlanningBoard"));
			//var det2 = histList[0].Details.Single(f => f.FieldName.Equals("Testing_CreateDefaultTestStep"));
			//Assert.AreEqual(
			//	projSettings.DisplayHoursOnPlanningBoard.ToString().ToLowerInvariant(),
			//	det1.NewValue.ToLowerInvariant(),
			//	"New Value was not correct!");
			//Assert.AreEqual(
			//	(!projSettings.DisplayHoursOnPlanningBoard).ToString().ToLowerInvariant(),
			//	det1.OldValue.ToLowerInvariant(),
			//	"New Value was not correct!");
			//Assert.AreEqual(
			//	projSettings.Testing_CreateDefaultTestStep.ToString().ToLowerInvariant(),
			//	det2.NewValue.ToLowerInvariant(),
			//	"New Value was not correct!");
			//Assert.AreEqual(
			//	(!projSettings.Testing_CreateDefaultTestStep).ToString().ToLowerInvariant(),
			//	det2.OldValue.ToLowerInvariant(),
			//	"New Value was not correct!");
		}
	}
}
