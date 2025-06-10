using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web.Security;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Artifact and ArtifactManager business objects
	/// </summary>
	/// <remarks>
	/// We changed the name from ArtifactManagerTest to Z_ArtifactManagerTest because this test fixture relies on the SQL free text indexing
	/// being completed and that can take some time, so need to make sure this is one of the last unit tests to run.
	/// </remarks>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class Z_ArtifactManagerTest
	{
		private const int ART_TYPE_ID = 3;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		private const int PROJECT_ID = 1;

		private static int projectId1;
		private static int projectId3;
		private static int templateId1;
		private static int templateId2;

		private static bool freeTextIndexReady = true;

		/// <summary>
		/// Sets up any data
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			//Create new projects for testing with (only some tests currently use this) - based on existing ones
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId1 = projectManager.CreateFromExisting("ArtifactManagerTest Project 1", null, null, 1, true, true, 1,
					adminSectionId,
					"Inserted Project");
			projectId3 = projectManager.Insert("ArtifactManagerTest Project 3", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			TemplateManager templateManager = new TemplateManager();
			templateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
			templateId2 = templateManager.RetrieveForProject(projectId3).ProjectTemplateId;

			//Make sure the free text index has created itself, otherwise ignore that test.
			int count;
			List<ArtifactView> artifacts = new ArtifactManager().SearchByKeyword("book", 0, 15, out count);
			if (count == 0 || !artifacts.Any(a => a.ArtifactTypeId == 1 && a.ArtifactId == 5))
			{
				freeTextIndexReady = false;
			}
		}

		/// <summary>
		/// Cleans up any data that was changed
		/// </summary>
		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary projects
			ProjectManager projectManager = new ProjectManager();
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId1);
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId3);

			//Delete the templates
			TemplateManager templateManager = new TemplateManager();
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId1);
			templateManager.Delete(USER_ID_SYS_ADMIN, templateId2);
		}

		[
		Test,
		SpiraTestCase(722),
		Description("Should retrieve all available artifact types.")
		]
		public void _01_RetrieveArtifactTypes()
		{
			//Get all the active types
			List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();

			//Verify that we can retrieve the expected number of artifact types
			Assert.IsNotNull(artifactTypes, "Could not get list of artifacts!");
			Assert.AreEqual(37, artifactTypes.Count, "Not all artifacts were retrieved!");
			Assert.AreEqual("ArtifactLink", artifactTypes[0].Name);
			Assert.AreEqual("Automation Host", artifactTypes[1].Name);
			Assert.AreEqual("Configuration", artifactTypes[2].Name);
			Assert.AreEqual("Document", artifactTypes[3].Name);
			Assert.AreEqual("DocumentDiscussion", artifactTypes[4].Name);
			Assert.AreEqual("DocumentSignature", artifactTypes[5].Name);
			Assert.AreEqual("DocumentVersion", artifactTypes[6].Name);
			Assert.AreEqual("Incident", artifactTypes[7].Name);
			Assert.AreEqual("IncidentResolution", artifactTypes[8].Name);
			Assert.AreEqual("IncidentSignature", artifactTypes[9].Name);
			Assert.AreEqual("Placeholder", artifactTypes[10].Name);
			Assert.AreEqual("Project Tag Frequency", artifactTypes[11].Name);
			Assert.AreEqual("ProjectBaseline", artifactTypes[12].Name);
			Assert.AreEqual("Release", artifactTypes[13].Name);
			Assert.AreEqual("ReleaseDiscussion", artifactTypes[14].Name);
			Assert.AreEqual("ReleaseSignature", artifactTypes[15].Name);
			Assert.AreEqual("ReleaseTestCase", artifactTypes[16].Name);
			Assert.AreEqual("Requirement", artifactTypes[17].Name);
			Assert.AreEqual("Requirement Step", artifactTypes[18].Name);
			Assert.AreEqual("RequirementDiscussion", artifactTypes[19].Name);
			Assert.AreEqual("RequirementSignature", artifactTypes[20].Name);
			Assert.AreEqual("Risk", artifactTypes[21].Name);
			Assert.AreEqual("Risk Mitigation", artifactTypes[22].Name);
			Assert.AreEqual("RiskDiscussion", artifactTypes[23].Name);
			Assert.AreEqual("RiskSignature", artifactTypes[24].Name);
			Assert.AreEqual("Task", artifactTypes[25].Name);
			Assert.AreEqual("TaskDiscussion", artifactTypes[26].Name);
			Assert.AreEqual("TaskSignature", artifactTypes[27].Name);
			Assert.AreEqual("Test Case", artifactTypes[28].Name);
			Assert.AreEqual("Test Run", artifactTypes[29].Name);
			Assert.AreEqual("Test Set", artifactTypes[30].Name);
			Assert.AreEqual("Test Step", artifactTypes[31].Name);
			Assert.AreEqual("TestCaseDiscussion", artifactTypes[32].Name);
			Assert.AreEqual("TestCaseParameter", artifactTypes[33].Name);
			Assert.AreEqual("TestCaseSignature", artifactTypes[34].Name);
			Assert.AreEqual("TestSetDiscussion", artifactTypes[35].Name);
			Assert.AreEqual("TestSetParameter", artifactTypes[36].Name);

			//Only those that support notifications
			artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(true, false, false, false);
			Assert.AreEqual(8, artifactTypes.Count, "Not all artifacts were retrieved!");
			Assert.AreEqual("Document", artifactTypes[0].Name);
			Assert.AreEqual("Incident", artifactTypes[1].Name);
			Assert.AreEqual("Release", artifactTypes[2].Name);
			Assert.AreEqual("Requirement", artifactTypes[3].Name);
			Assert.AreEqual("Risk", artifactTypes[4].Name);
			Assert.AreEqual("Task", artifactTypes[5].Name);
			Assert.AreEqual("Test Case", artifactTypes[6].Name);
			Assert.AreEqual("Test Set", artifactTypes[7].Name);

			//Include the inactive types
			artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(false, true, false, false);
			Assert.AreEqual(65, artifactTypes.Count, "Not all artifacts were retrieved!");
			Assert.AreEqual("AdminAuditTrail", artifactTypes[0].Name);
			Assert.AreEqual("All Admin Audit Trail", artifactTypes[1].Name);
			Assert.AreEqual("All Audit Trail", artifactTypes[2].Name);
			Assert.AreEqual("All Project Audit Trail", artifactTypes[3].Name);
			Assert.AreEqual("All User Audit Trail", artifactTypes[4].Name);
			Assert.AreEqual("ArtifactLink", artifactTypes[5].Name);
			Assert.AreEqual("AuditTrail", artifactTypes[6].Name);
			Assert.AreEqual("Automation Engine", artifactTypes[7].Name);
			Assert.AreEqual("Automation Host", artifactTypes[8].Name);
			Assert.AreEqual("Configuration", artifactTypes[9].Name);
			Assert.AreEqual("DataSync", artifactTypes[10].Name);
			Assert.AreEqual("Document", artifactTypes[11].Name);
			Assert.AreEqual("DocumentDiscussion", artifactTypes[12].Name);
			Assert.AreEqual("DocumentSignature", artifactTypes[13].Name);
			Assert.AreEqual("DocumentVersion", artifactTypes[14].Name);
			Assert.AreEqual("Event", artifactTypes[15].Name);
			Assert.AreEqual("File Type Icon", artifactTypes[16].Name);
			Assert.AreEqual("Graph", artifactTypes[17].Name);
			Assert.AreEqual("HistoryDiscussion", artifactTypes[18].Name);
			Assert.AreEqual("Incident", artifactTypes[19].Name);
			Assert.AreEqual("IncidentResolution", artifactTypes[20].Name);
			Assert.AreEqual("IncidentSignature", artifactTypes[21].Name);
			Assert.AreEqual("Login Provider", artifactTypes[22].Name);
			Assert.AreEqual("Placeholder", artifactTypes[23].Name);
			Assert.AreEqual("Portfolios", artifactTypes[24].Name);
			Assert.AreEqual("Program", artifactTypes[25].Name);
			Assert.AreEqual("Project", artifactTypes[26].Name);
			Assert.AreEqual("Project Role", artifactTypes[27].Name);
			Assert.AreEqual("Project Role Permission", artifactTypes[28].Name);
			Assert.AreEqual("Project Tag Frequency", artifactTypes[29].Name);
			Assert.AreEqual("Project User Group", artifactTypes[30].Name);
			Assert.AreEqual("ProjectAuditTrail", artifactTypes[31].Name);
			Assert.AreEqual("ProjectBaseline", artifactTypes[32].Name);
			Assert.AreEqual("ProjectTemplate", artifactTypes[33].Name);
			Assert.AreEqual("Release", artifactTypes[34].Name);
			Assert.AreEqual("ReleaseDiscussion", artifactTypes[35].Name);
			Assert.AreEqual("ReleaseSignature", artifactTypes[36].Name);
			Assert.AreEqual("ReleaseTestCase", artifactTypes[37].Name);
			Assert.AreEqual("Report", artifactTypes[38].Name);
			Assert.AreEqual("ReportCustomSection", artifactTypes[39].Name);
			Assert.AreEqual("ReportSectionInstance", artifactTypes[40].Name);
			Assert.AreEqual("Requirement", artifactTypes[41].Name);
			Assert.AreEqual("Requirement Step", artifactTypes[42].Name);
			Assert.AreEqual("RequirementDiscussion", artifactTypes[43].Name);
			Assert.AreEqual("RequirementSignature", artifactTypes[44].Name);
			Assert.AreEqual("Risk", artifactTypes[45].Name);
			Assert.AreEqual("Risk Mitigation", artifactTypes[46].Name);
			Assert.AreEqual("RiskDiscussion", artifactTypes[47].Name);
			Assert.AreEqual("RiskSignature", artifactTypes[48].Name);
			Assert.AreEqual("SourceCode", artifactTypes[49].Name);
			Assert.AreEqual("SystemUsageReport", artifactTypes[50].Name);
			Assert.AreEqual("Task", artifactTypes[51].Name);
			Assert.AreEqual("TaskDiscussion", artifactTypes[52].Name);
			Assert.AreEqual("TaskSignature", artifactTypes[53].Name);
			Assert.AreEqual("Test Case", artifactTypes[54].Name);
			Assert.AreEqual("Test Run", artifactTypes[55].Name);
			Assert.AreEqual("Test Set", artifactTypes[56].Name);
			Assert.AreEqual("Test Step", artifactTypes[57].Name);
			Assert.AreEqual("TestCaseDiscussion", artifactTypes[58].Name);
			Assert.AreEqual("TestCaseParameter", artifactTypes[59].Name);
			Assert.AreEqual("TestCaseSignature", artifactTypes[60].Name);
			Assert.AreEqual("TestSetDiscussion", artifactTypes[61].Name);
			Assert.AreEqual("TestSetParameter", artifactTypes[62].Name);
			Assert.AreEqual("User", artifactTypes[63].Name);
			Assert.AreEqual("UserAudirTrail", artifactTypes[64].Name);

			//Only those that support data-sync
			artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(false, false, true, false);
			Assert.AreEqual(29, artifactTypes.Count, "Not all artifacts were retrieved!");
			Assert.AreEqual("ArtifactLink", artifactTypes[0].Name);
			Assert.AreEqual("Configuration", artifactTypes[1].Name);
			Assert.AreEqual("DocumentDiscussion", artifactTypes[2].Name);
			Assert.AreEqual("DocumentSignature", artifactTypes[3].Name);
			Assert.AreEqual("DocumentVersion", artifactTypes[4].Name);
			Assert.AreEqual("Incident", artifactTypes[5].Name);
			Assert.AreEqual("IncidentResolution", artifactTypes[6].Name);
			Assert.AreEqual("IncidentSignature", artifactTypes[7].Name);
			Assert.AreEqual("Project Tag Frequency", artifactTypes[8].Name);
			Assert.AreEqual("ProjectBaseline", artifactTypes[9].Name);
			Assert.AreEqual("Release", artifactTypes[10].Name);
			Assert.AreEqual("ReleaseDiscussion", artifactTypes[11].Name);
			Assert.AreEqual("ReleaseSignature", artifactTypes[12].Name);
			Assert.AreEqual("ReleaseTestCase", artifactTypes[13].Name);
			Assert.AreEqual("Requirement", artifactTypes[14].Name);
			Assert.AreEqual("RequirementDiscussion", artifactTypes[15].Name);
			Assert.AreEqual("RequirementSignature", artifactTypes[16].Name);
			Assert.AreEqual("Risk", artifactTypes[17].Name);
			Assert.AreEqual("RiskDiscussion", artifactTypes[18].Name);
			Assert.AreEqual("RiskSignature", artifactTypes[19].Name);
			Assert.AreEqual("Task", artifactTypes[20].Name);
			Assert.AreEqual("TaskDiscussion", artifactTypes[21].Name);
			Assert.AreEqual("TaskSignature", artifactTypes[22].Name);
			Assert.AreEqual("Test Case", artifactTypes[23].Name);
			Assert.AreEqual("TestCaseDiscussion", artifactTypes[24].Name);
			Assert.AreEqual("TestCaseParameter", artifactTypes[25].Name);
			Assert.AreEqual("TestCaseSignature", artifactTypes[26].Name);
			Assert.AreEqual("TestSetDiscussion", artifactTypes[27].Name);
			Assert.AreEqual("TestSetParameter", artifactTypes[28].Name);

			//Only those that support attachments
			artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(false, false, false, true);
			Assert.AreEqual(33, artifactTypes.Count, "Not all artifacts were retrieved!");
			Assert.AreEqual("ArtifactLink", artifactTypes[0].Name);
			Assert.AreEqual("Automation Host", artifactTypes[1].Name);
			Assert.AreEqual("Configuration", artifactTypes[2].Name);
			Assert.AreEqual("DocumentDiscussion", artifactTypes[3].Name);
			Assert.AreEqual("DocumentSignature", artifactTypes[4].Name);
			Assert.AreEqual("DocumentVersion", artifactTypes[5].Name);
			Assert.AreEqual("Incident", artifactTypes[6].Name);
			Assert.AreEqual("IncidentResolution", artifactTypes[7].Name);
			Assert.AreEqual("IncidentSignature", artifactTypes[8].Name);
			Assert.AreEqual("Project Tag Frequency", artifactTypes[9].Name);
			Assert.AreEqual("ProjectBaseline", artifactTypes[10].Name);
			Assert.AreEqual("Release", artifactTypes[11].Name);
			Assert.AreEqual("ReleaseDiscussion", artifactTypes[12].Name);
			Assert.AreEqual("ReleaseSignature", artifactTypes[13].Name);
			Assert.AreEqual("ReleaseTestCase", artifactTypes[14].Name);
			Assert.AreEqual("Requirement", artifactTypes[15].Name);
			Assert.AreEqual("RequirementDiscussion", artifactTypes[16].Name);
			Assert.AreEqual("RequirementSignature", artifactTypes[17].Name);
			Assert.AreEqual("Risk", artifactTypes[18].Name);
			Assert.AreEqual("RiskDiscussion", artifactTypes[19].Name);
			Assert.AreEqual("RiskSignature", artifactTypes[20].Name);
			Assert.AreEqual("Task", artifactTypes[21].Name);
			Assert.AreEqual("TaskDiscussion", artifactTypes[22].Name);
			Assert.AreEqual("TaskSignature", artifactTypes[23].Name);
			Assert.AreEqual("Test Case", artifactTypes[24].Name);
			Assert.AreEqual("Test Run", artifactTypes[25].Name);
			Assert.AreEqual("Test Set", artifactTypes[26].Name);
			Assert.AreEqual("Test Step", artifactTypes[27].Name);
			Assert.AreEqual("TestCaseDiscussion", artifactTypes[28].Name);
			Assert.AreEqual("TestCaseParameter", artifactTypes[29].Name);
			Assert.AreEqual("TestCaseSignature", artifactTypes[30].Name);
			Assert.AreEqual("TestSetDiscussion", artifactTypes[31].Name);
			Assert.AreEqual("TestSetParameter", artifactTypes[32].Name);
		}

		[
		Test,
		SpiraTestCase(723),
		Description("Verify all fields are pulled for an artifact.")
		]
		public void _02_RetrieveFields()
		{
			ArtifactManager artifactManager = new ArtifactManager();
			ArtifactType artifactType = artifactManager.ArtifactType_RetrieveById(ART_TYPE_ID);

			//Check that all fields were pulled.
			Assert.IsNotNull(artifactType, "Could not pull specified artifact!");
			Assert.AreEqual(25, artifactType.Fields.Count, "Number of fields did not match what was expected.");

			//Verify that we can retrieve the list of fields for an artifact to be displayed on a list page
			//regardless of the user's view settings

			//For incidents
			List<ArtifactField> fields = artifactManager.ArtifactField_RetrieveForLists(Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(23, fields.Count);
			Assert.AreEqual("Status", fields.FirstOrDefault(f => f.Name == "IncidentStatusId").Caption);
			Assert.AreEqual(true, fields.FirstOrDefault(f => f.Name == "IncidentStatusId").IsListDefault);

			//For releases
			fields = artifactManager.ArtifactField_RetrieveForLists(Artifact.ArtifactTypeEnum.Release);
			Assert.AreEqual(27, fields.Count);
			Assert.AreEqual("Version #", fields.FirstOrDefault(f => f.Name == "VersionNumber").Caption);
			Assert.AreEqual(true, fields.FirstOrDefault(f => f.Name == "VersionNumber").IsListDefault);
		}

		/// <summary>
		/// Tests that we can search across artifact types and projects by keyword
		/// </summary>
		[
		Test,
		SpiraTestCase(807)
		]
		public void _03_GlobalSearch()
		{
			if (!freeTextIndexReady)
			{
				//Just mark as not run
				Assert.Ignore();
				return;
			}

			//Test that we can search for a keyword across multiple projects regardless of permissions
			ArtifactManager artifactManager = new ArtifactManager();
			int count;
			List<ArtifactView> artifacts = artifactManager.SearchByKeyword("book", 0, 15, out count);

			//Verify the count and search results
			Assert.AreEqual(86, count);
			Assert.AreEqual(15, artifacts.Count);

			//Verify one of the results
			ArtifactView artifactView = artifacts.FirstOrDefault(a => a.ArtifactTypeId == 1 && a.ArtifactId == 5);
			Assert.IsNotNull(artifactView, "NullCheck1");
			Assert.AreEqual(5, artifactView.ArtifactId, "artifactView.ArtifactId");
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactView.ArtifactTypeId);
			Assert.AreEqual("Ability to edit existing books in the system", artifactView.Name);
			Assert.AreEqual("Library Information System (Sample)", artifactView.ProjectName);
			Assert.AreEqual("<p>The ability to edit existing books into the system, including ISBN, publisher and other related information.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", artifactView.Description);
			Assert.IsNotNull(artifactView.LastUpdateDate.Value.Date, "artifactView.LastUpdateDate");

			//Now test that we can filter by a couple of project and artifacts that the user has permissions to see
			List<ArtifactManager.ProjectArtifactTypeFilter> projectArtifactList = new List<ArtifactManager.ProjectArtifactTypeFilter>();
			projectArtifactList.Add(new ArtifactManager.ProjectArtifactTypeFilter() { ArtifactTypeId = 1, ProjectId = 1 });
			projectArtifactList.Add(new ArtifactManager.ProjectArtifactTypeFilter() { ArtifactTypeId = 4, ProjectId = 1 });
			artifacts = artifactManager.SearchByKeyword("book", 0, 15, out count, projectArtifactList);

			//Verify the count and search results
			Assert.AreEqual(12, count);
			Assert.AreEqual(12, artifacts.Count);
			Assert.IsNotNull(artifactView, "NullCheck2");
			artifactView = artifacts.FirstOrDefault(a => a.ArtifactTypeId == 1 && a.ArtifactId == 5);
			Assert.AreEqual(5, artifactView.ArtifactId, "artifactView.ArtifactId");
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactView.ArtifactTypeId);
			Assert.AreEqual("Ability to edit existing books in the system", artifactView.Name);
			Assert.AreEqual("Library Information System (Sample)", artifactView.ProjectName);
			Assert.AreEqual("<p>The ability to edit existing books into the system, including ISBN, publisher and other related information.</p><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus. Faucibus purus in massa tempor nec feugiat nisl. Eu scelerisque felis imperdiet proin. Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus. Urna et pharetra pharetra massa massa ultricies. Nibh tortor id aliquet lectus proin nibh nisl condimentum. Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Sollicitudin aliquam ultrices sagittis orci.</p>", artifactView.Description);
			Assert.IsNotNull(artifactView.LastUpdateDate.Value.Date, "artifactView.LastUpdateDate");

			//Now search by unique artifact token without permission filtering
			artifacts = artifactManager.SearchByKeyword("RQ5", 0, 15, out count);

			//Verify the count and search results
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, artifacts.Count);
			Assert.AreEqual(5, artifacts[0].ArtifactId);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifacts[0].ArtifactTypeId);

			//Now search by unique artifact token with permission filtering that matches
			artifacts = artifactManager.SearchByKeyword("RQ:5", 0, 15, out count, projectArtifactList);

			//Verify the count and search results
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, artifacts.Count);
			Assert.AreEqual(5, artifacts[0].ArtifactId);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, artifacts[0].ArtifactTypeId);

			//Now search by unique artifact token with permission filtering that doesn't match
			artifacts = artifactManager.SearchByKeyword("IN3", 0, 15, out count, projectArtifactList);

			//Verify the count and search results
			Assert.AreEqual(0, count);
			Assert.AreEqual(0, artifacts.Count);
		}

		/// <summary>
		/// Tests that we can show/hide columns on list screens.
		/// </summary>
		[
		Test,
		SpiraTestCase(185)
		]
		public void _04_ShowHideColumnsOnListScreens()
		{
			ArtifactManager artifactManager = new ArtifactManager();
			//Test that we can retrieve the list of fields that should be displayed in the Incidents list
			//First for Fred Bloggs project 1
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(32, artifactFields.Count);
			Assert.AreEqual("IncidentTypeId", artifactFields[24].Name);
			Assert.AreEqual("Type", artifactFields[24].Caption);
			Assert.AreEqual(true, artifactFields[24].IsVisible);
			Assert.AreEqual("SeverityId", artifactFields[0].Name);
			Assert.AreEqual("Severity", artifactFields[0].Caption);
			Assert.AreEqual(false, artifactFields[0].IsVisible);
			Assert.AreEqual("CreationDate", artifactFields[28].Name);
			Assert.AreEqual("Detected On", artifactFields[28].Caption);
			Assert.AreEqual(true, artifactFields[28].IsVisible);

			//Next for Fred Bloggs project 2
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(23, artifactFields.Count);
			Assert.AreEqual("IncidentTypeId", artifactFields[15].Name);
			Assert.AreEqual("Type", artifactFields[15].Caption);
			Assert.AreEqual(true, artifactFields[15].IsVisible);
			Assert.AreEqual("PriorityId", artifactFields[17].Name);
			Assert.AreEqual("Priority", artifactFields[17].Caption);
			Assert.AreEqual(true, artifactFields[17].IsVisible);
			Assert.AreEqual("ClosedDate", artifactFields[1].Name);
			Assert.AreEqual("Closed On", artifactFields[1].Caption);
			Assert.AreEqual(false, artifactFields[1].IsVisible);

			//Next for Joe Smith project 1
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_JOE_SMITH, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(32, artifactFields.Count);
			Assert.AreEqual("IncidentTypeId", artifactFields[24].Name);
			Assert.AreEqual("Type", artifactFields[24].Caption);
			Assert.AreEqual(true, artifactFields[24].IsVisible);
			Assert.AreEqual("SeverityId", artifactFields[0].Name);
			Assert.AreEqual("Severity", artifactFields[0].Caption);
			Assert.AreEqual(false, artifactFields[0].IsVisible);
			Assert.AreEqual("CreationDate", artifactFields[28].Name);
			Assert.AreEqual("Detected On", artifactFields[28].Caption);
			Assert.AreEqual(true, artifactFields[28].IsVisible);

			//Next we need to verify that we can update the artifact field visibility status
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual("IncidentTypeId", artifactFields[24].Name);
			Assert.AreEqual(true, artifactFields[24].IsVisible);

			//Hide IncidentType (insert new user setting since originally default)
			artifactManager.ArtifactField_ToggleListVisibility(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident, "IncidentTypeId");
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual("IncidentTypeId", artifactFields[0].Name);
			Assert.AreEqual(false, artifactFields[0].IsVisible);

			//Verify that it is unchanged for Joe Smith
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_JOE_SMITH, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(32, artifactFields.Count);
			Assert.AreEqual("IncidentTypeId", artifactFields[24].Name);
			Assert.AreEqual("Type", artifactFields[24].Caption);
			Assert.AreEqual(true, artifactFields[24].IsVisible);

			//Show IncidentType (update existing user setting)
			artifactManager.ArtifactField_ToggleListVisibility(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident, "IncidentTypeId");
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId1, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual("IncidentTypeId", artifactFields[24].Name);
			Assert.AreEqual(true, artifactFields[24].IsVisible);
		}

		/// <summary>
		/// Tests that we can reorder columns on list screens and have the change remembered.
		/// </summary>
		[
		Test,
		SpiraTestCase(1120)
		]
		public void _05_ReorderColumnsOnListScreens()
		{
			ArtifactManager artifactManager = new ArtifactManager();

			//First lets verify the order for Fred Bloggs in project 2
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);

			//We only want to consider the visible fields
			List<ArtifactListFieldDisplay> visibleFields = artifactFields.Where(f => f.IsVisible).ToList();
			Assert.AreEqual(9, visibleFields.Count);

			//Verify some of the positions
			Assert.AreEqual(2, visibleFields.First(f => f.Name == "IncidentTypeId").ListPosition);
			Assert.AreEqual(7, visibleFields.First(f => f.Name == "CreationDate").ListPosition);
			Assert.AreEqual(21, visibleFields.First(f => f.Name == "IncidentId").ListPosition);

			//Now lets move one of the columns
			artifactManager.ArtifactField_ChangeListPosition(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "IncidentId", 1);

			//Verify the positions
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			visibleFields = artifactFields.Where(f => f.IsVisible).ToList();
			Assert.AreEqual(3, visibleFields.First(f => f.Name == "IncidentTypeId").ListPosition);
			Assert.AreEqual(8, visibleFields.First(f => f.Name == "CreationDate").ListPosition);
			Assert.AreEqual(1, visibleFields.First(f => f.Name == "IncidentId").ListPosition);

			//Now lets move it to the middle
			artifactManager.ArtifactField_ChangeListPosition(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "IncidentId", 4);

			//Verify the positions
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			visibleFields = artifactFields.Where(f => f.IsVisible).ToList();
			Assert.AreEqual(2, visibleFields.First(f => f.Name == "IncidentTypeId").ListPosition);
			Assert.AreEqual(8, visibleFields.First(f => f.Name == "CreationDate").ListPosition);
			Assert.AreEqual(4, visibleFields.First(f => f.Name == "IncidentId").ListPosition);

			//Now lets move it to the end
			artifactManager.ArtifactField_ChangeListPosition(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "IncidentId", 21);

			//Verify the positions
			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Incident);
			visibleFields = artifactFields.Where(f => f.IsVisible).ToList();
			Assert.AreEqual(2, visibleFields.First(f => f.Name == "IncidentTypeId").ListPosition);
			Assert.AreEqual(7, visibleFields.First(f => f.Name == "CreationDate").ListPosition);
			Assert.AreEqual(21, visibleFields.First(f => f.Name == "IncidentId").ListPosition);
		}

		/// <summary>
		/// Tests some of the other artifact type retrieval functions (migrated from DataAccess.cs)
		/// </summary>
		[
		Test,
		SpiraTestCase(1118)
		]
		public void _06_RetrieveArtifactTypes2()
		{
			//Test that we can retrieve the list of artifact types
			ArtifactManager artifactManager = new Business.ArtifactManager();
			List<ArtifactType> artifactTypes = artifactManager.ArtifactType_RetrieveAll(false, false, false);
			Assert.AreEqual(37, artifactTypes.Count);
			Assert.AreEqual("ArtifactLink", artifactTypes[0].Name);
			Assert.AreEqual("AL", artifactTypes[0].Prefix);
			Assert.AreEqual("Automation Host", artifactTypes[1].Name);
			Assert.AreEqual("AH", artifactTypes[1].Prefix);
			Assert.AreEqual("Configuration", artifactTypes[2].Name);
			Assert.AreEqual("TC", artifactTypes[2].Prefix);
			Assert.AreEqual("Document", artifactTypes[3].Name);
			Assert.AreEqual("DC", artifactTypes[3].Prefix);
			Assert.AreEqual("DocumentDiscussion", artifactTypes[4].Name);
			Assert.AreEqual("DD", artifactTypes[4].Prefix);
			Assert.AreEqual("DocumentSignature", artifactTypes[5].Name);
			Assert.AreEqual("DS", artifactTypes[5].Prefix);
			Assert.AreEqual("DocumentVersion", artifactTypes[6].Name);
			Assert.AreEqual("DV", artifactTypes[6].Prefix);
			Assert.AreEqual("Incident", artifactTypes[7].Name);
			Assert.AreEqual("IN", artifactTypes[7].Prefix);
			Assert.AreEqual("IncidentResolution", artifactTypes[8].Name);
			Assert.AreEqual("IR", artifactTypes[8].Prefix);
			Assert.AreEqual("IncidentSignature", artifactTypes[9].Name);
			Assert.AreEqual("IS", artifactTypes[9].Prefix);
			Assert.AreEqual("Placeholder", artifactTypes[10].Name);
			Assert.AreEqual("PL", artifactTypes[10].Prefix);
			Assert.AreEqual("Project Tag Frequency", artifactTypes[11].Name);
			Assert.AreEqual("PT", artifactTypes[11].Prefix);
			Assert.AreEqual("ProjectBaseline", artifactTypes[12].Name);
			Assert.AreEqual("PB", artifactTypes[12].Prefix);
			Assert.AreEqual("Release", artifactTypes[13].Name);
			Assert.AreEqual("RL", artifactTypes[13].Prefix);
			Assert.AreEqual("ReleaseDiscussion", artifactTypes[14].Name);
			Assert.AreEqual("RD", artifactTypes[14].Prefix);
			Assert.AreEqual("ReleaseSignature", artifactTypes[15].Name);
			Assert.AreEqual("RI", artifactTypes[15].Prefix);
			Assert.AreEqual("ReleaseTestCase", artifactTypes[16].Name);
			Assert.AreEqual("RT", artifactTypes[16].Prefix);
			Assert.AreEqual("Requirement", artifactTypes[17].Name);
			Assert.AreEqual("RQ", artifactTypes[17].Prefix);
			Assert.AreEqual("Requirement Step", artifactTypes[18].Name);
			Assert.AreEqual("RS", artifactTypes[18].Prefix);
			Assert.AreEqual("RequirementDiscussion", artifactTypes[19].Name);
			Assert.AreEqual("RQ", artifactTypes[19].Prefix);
			Assert.AreEqual("RequirementSignature", artifactTypes[20].Name);
			Assert.AreEqual("RS", artifactTypes[20].Prefix);
			Assert.AreEqual("Risk", artifactTypes[21].Name);
			Assert.AreEqual("RK", artifactTypes[21].Prefix);
			Assert.AreEqual("Risk Mitigation", artifactTypes[22].Name);
			Assert.AreEqual("RM", artifactTypes[22].Prefix);
			Assert.AreEqual("RiskDiscussion", artifactTypes[23].Name);
			Assert.AreEqual("RR", artifactTypes[23].Prefix);
			Assert.AreEqual("RiskSignature", artifactTypes[24].Name);
			Assert.AreEqual("SR", artifactTypes[24].Prefix);
			Assert.AreEqual("Task", artifactTypes[25].Name);
			Assert.AreEqual("TK", artifactTypes[25].Prefix);
			Assert.AreEqual("TaskDiscussion", artifactTypes[26].Name);
			Assert.AreEqual("TS", artifactTypes[26].Prefix);
			Assert.AreEqual("TaskSignature", artifactTypes[27].Name);
			Assert.AreEqual("TS", artifactTypes[27].Prefix);
			Assert.AreEqual("Test Case", artifactTypes[28].Name);
			Assert.AreEqual("TC", artifactTypes[28].Prefix);
			Assert.AreEqual("Test Run", artifactTypes[29].Name);
			Assert.AreEqual("TR", artifactTypes[29].Prefix);
			Assert.AreEqual("Test Set", artifactTypes[30].Name);
			Assert.AreEqual("TX", artifactTypes[30].Prefix);
			Assert.AreEqual("Test Step", artifactTypes[31].Name);
			Assert.AreEqual("TS", artifactTypes[31].Prefix);
			Assert.AreEqual("TestCaseDiscussion", artifactTypes[32].Name);
			Assert.AreEqual("TD", artifactTypes[32].Prefix);
			Assert.AreEqual("TestCaseParameter", artifactTypes[33].Name);
			Assert.AreEqual("TP", artifactTypes[33].Prefix);
			Assert.AreEqual("TestCaseSignature", artifactTypes[34].Name);
			Assert.AreEqual("TS", artifactTypes[34].Prefix);
			Assert.AreEqual("TestSetDiscussion", artifactTypes[35].Name);
			Assert.AreEqual("TS", artifactTypes[35].Prefix);
			Assert.AreEqual("TestSetParameter", artifactTypes[36].Name);
			Assert.AreEqual("SP", artifactTypes[36].Prefix);

			//Verify that we can get the details of an artifact type by its ID/enum
			ArtifactType artifactType = artifactManager.ArtifactType_RetrieveById(DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual("Requirement", (string)artifactType.Name);
			Assert.AreEqual("RQ", (string)artifactType.Prefix);

			artifactType = artifactManager.ArtifactType_RetrieveById((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual("Requirement", (string)artifactType.Name);
			Assert.AreEqual("RQ", (string)artifactType.Prefix);

			//Verify that we can retrieve an artifact type by its prefix
			artifactType = artifactManager.ArtifactType_RetrieveByPrefix("RQ");
			Assert.AreEqual("Requirement", (string)artifactType.Name);
			Assert.AreEqual("RQ", (string)artifactType.Prefix);

		}

		/// <summary>
		/// Tests that we can resize columns on list screens and have the change remembered.
		/// </summary>
		[
		Test,
		SpiraTestCase(1512)
		]
		public void _07_ResizeColumnsOnListScreens()
		{
			ArtifactManager artifactManager = new ArtifactManager();
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//We need to add a custom property to the project to test with
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(templateId2, Artifact.ArtifactTypeEnum.Requirement, (int)CustomProperty.CustomPropertyTypeEnum.Text, 1, "Notes", null, null, null);

			//First lets verify the width of a couple of fields (will be null)
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Requirement);

			//Name
			ArtifactListFieldDisplay artifactField = artifactFields.FirstOrDefault(f => f.Name == "Name");
			Assert.AreEqual(null, artifactField.Width);
			//ImportanceId
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "EstimatedEffort");
			Assert.AreEqual(null, artifactField.Width);
			//Custom Field 01
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "Custom_01");
			Assert.AreEqual(null, artifactField.Width);

			//Make some changes
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "Name", 100);
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "EstimatedEffort", 150);
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "Custom_01", 200);

			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			//Name
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "Name");
			Assert.AreEqual(100, artifactField.Width);
			//ImportanceId
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "EstimatedEffort");
			Assert.AreEqual(150, artifactField.Width);
			//Custom Field 01
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "Custom_01");
			Assert.AreEqual(200, artifactField.Width);

			//Make some changes
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "Name", 220);
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "EstimatedEffort", 100);
			artifactManager.ArtifactField_ChangeColumnWidth(projectId3, templateId2, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Requirement, "Custom_01", 180);

			artifactFields = artifactManager.ArtifactField_RetrieveForLists(projectId3, USER_ID_FRED_BLOGGS, DataModel.Artifact.ArtifactTypeEnum.Requirement);
			//Name
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "Name");
			Assert.AreEqual(220, artifactField.Width);
			//ImportanceId
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "EstimatedEffort");
			Assert.AreEqual(100, artifactField.Width);
			//Custom Field 01
			artifactField = artifactFields.FirstOrDefault(f => f.Name == "Custom_01");
			Assert.AreEqual(180, artifactField.Width);

			//Remove the custom property
			customPropertyManager.CustomPropertyDefinition_RemoveFromArtifact(templateId2, Artifact.ArtifactTypeEnum.Requirement, 1);
		}

		/// <summary>
		/// Tests that we can generically basic info about all artifacts (name, description, id) using a standard method
		/// </summary>
		[Test]
		[SpiraTestCase(1537)]
		public void _08_RetrieveArtifactInfo()
		{
			ArtifactManager artifactManager = new ArtifactManager();

			//Requirement
			ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.Requirement, 4, PROJECT_ID);
			Assert.AreEqual(4, artifactInfo.ArtifactId);
			Assert.AreEqual("RQ:4", artifactInfo.ArtifactToken);
			Assert.AreEqual("Ability to add new books to the system", artifactInfo.Name);
			//Assert.AreEqual("<p>The ability to add new books into the system, complete with ISBN, publisher and other related information.</p>\n\n<p> Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tortor vitae purus faucibus ornare suspendisse sed nisi lacus.Faucibus purus in massa tempor nec feugiat nisl.Eu scelerisque felis imperdiet proin.Blandit libero volutpat sed cras ornare arcu dui vivamus arcu. Et magnis dis parturient montes nascetur ridiculus mus mauris vitae. Massa placerat duis ultricies lacus.Urna et pharetra pharetra massa massa ultricies.Nibh tortor id aliquet lectus proin nibh nisl condimentum.Faucibus purus in massa tempor. Scelerisque varius morbi enim nunc faucibus a pellentesque sit.Sollicitudin aliquam ultrices sagittis orci.</ p > ", artifactInfo.Description);

			//Test Case
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.TestCase, 2, PROJECT_ID);
			Assert.AreEqual(2, artifactInfo.ArtifactId);
			Assert.AreEqual("TC:2", artifactInfo.ArtifactToken);
			Assert.AreEqual("Ability to create new book", artifactInfo.Name);
			Assert.AreEqual("Tests that the user can create a new book in the system", artifactInfo.Description);

			//Incident
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.Incident, 9, PROJECT_ID);
			Assert.AreEqual(9, artifactInfo.ArtifactId);
			Assert.AreEqual("IN:9", artifactInfo.ArtifactToken);
			Assert.AreEqual("Editing the date on an author is clunky", artifactInfo.Name);
			Assert.AreEqual("The data-validation on the date fields is too strict, and there is no help text indicating the format to use. Consider adding a calendar control", artifactInfo.Description);

			//Release
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.Release, 1, PROJECT_ID);
			Assert.AreEqual(1, artifactInfo.ArtifactId);
			Assert.AreEqual("RL:1", artifactInfo.ArtifactToken);
			Assert.AreEqual("Library System Release 1", artifactInfo.Name);
			Assert.AreEqual("This is the initial release of the Library Management System", artifactInfo.Description);

			//Test Run
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.TestRun, 3, PROJECT_ID);
			Assert.AreEqual(3, artifactInfo.ArtifactId);
			Assert.AreEqual("TR:3", artifactInfo.ArtifactToken);
			Assert.AreEqual("Ability to create new book", artifactInfo.Name);
			Assert.AreEqual("Tests that the user can create a new book in the system", artifactInfo.Description);

			//Task
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.Task, 1, PROJECT_ID);
			Assert.AreEqual(1, artifactInfo.ArtifactId);
			Assert.AreEqual("TK:1", artifactInfo.ArtifactToken);
			Assert.AreEqual("Develop new book entry screen", artifactInfo.Name);
			Assert.AreEqual("Create a new dynamic page that allows the user to enter the details of a new book", artifactInfo.Description);

			//Test Set
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.TestSet, 1, PROJECT_ID);
			Assert.AreEqual(1, artifactInfo.ArtifactId);
			Assert.AreEqual("TX:1", artifactInfo.ArtifactToken);
			Assert.AreEqual("Testing Cycle for Release 1.0", artifactInfo.Name);
			Assert.AreEqual("This tests the functionality introduced in release 1.0 of the library system", artifactInfo.Description);

			//Test Step
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.TestStep, 2, PROJECT_ID);
			Assert.AreEqual(2, artifactInfo.ArtifactId);
			Assert.AreEqual("TS:2", artifactInfo.ArtifactToken);
			Assert.AreEqual("Ability to create new book", artifactInfo.Name);
			Assert.AreEqual("User clicks link to create book", artifactInfo.Description);

			//Automation Host
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.AutomationHost, 1, PROJECT_ID);
			Assert.AreEqual(1, artifactInfo.ArtifactId);
			Assert.AreEqual("AH:1", artifactInfo.ArtifactToken);
			Assert.AreEqual("Windows 8 Host", artifactInfo.Name);
			Assert.AreEqual("Windows 8 with IE10, Firefox 14, Chrome and Safari 5", artifactInfo.Description);

			//Document
			artifactInfo = artifactManager.RetrieveArtifactInfo(Artifact.ArtifactTypeEnum.Document, 1, PROJECT_ID);
			Assert.AreEqual(1, artifactInfo.ArtifactId);
			Assert.AreEqual("DC:1", artifactInfo.ArtifactToken);
			Assert.AreEqual("Book Management Functional Spec.doc", artifactInfo.Name);
			Assert.AreEqual("This document outlines the functional specification for the book management part of the library management system.", artifactInfo.Description);
		}
	}
}
