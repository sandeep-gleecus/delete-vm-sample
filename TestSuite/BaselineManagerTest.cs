using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the BuildManager business object
	/// </summary>
	[TestFixture]
	[SpiraTestConfiguration(
		InternalRoutines.SPIRATEST_INTERNAL_URL,
		InternalRoutines.SPIRATEST_INTERNAL_LOGIN,
		InternalRoutines.SPIRATEST_INTERNAL_PASSWORD,
		InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID,
		InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class BaselineManagerTest
	{
		#region Setup Vars
		private static int? projectId = null;
		private static int? releaseId1 = null;
		private static int? releaseId2 = null;
		private static int? requirementId1 = null;
		private static int? requirementId2 = null;
		private static int? incidentId1 = null;
		private static int? incidentId2 = null;
		private static bool setupDone = false;
		#endregion Setup Vars

		#region Testing Vars
		private static int? baselineId1 = null;
		private static int? baselineId2 = null;
		private static int? baselineId3 = null;
		#endregion Testing Vars

		/// <summary>Sets up the fixture</summary>
		[TestFixtureSetUp]
		public void SetUp()
		{
			if (!setupDone)
			{
				//Inform.
				Console.WriteLine(Environment.NewLine + "Setting up test:");

				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				//Create a new project.
				Console.Write(" - Creating project, #");
				ProjectManager pMgr = new ProjectManager();
				projectId = pMgr.Insert("TestBaselineProject", null, "Used for Testing Baselines", null, true, null, 1, adminSectionId, "Inserted Project");
				Console.WriteLine(projectId.Value.ToString());

				//Create a release. We don't care aabout the actual values here, since we're only testing baselines.
				Console.Write(" - Creating first release: #");
				ReleaseManager rMgr = new ReleaseManager();
				releaseId1 = rMgr.Insert(
					1,
					projectId.Value,
					1,
					"Test Release 1",
					"Test Release 1",
					"1.0.0.0",
					"AAA",
					Release.ReleaseStatusEnum.InProgress,
					Release.ReleaseTypeEnum.MajorRelease,
					DateTime.UtcNow.AddDays(-2),
					DateTime.UtcNow.AddDays(2),
					3.2m,
					1.2m,
					null,
					true);
				Console.WriteLine(releaseId1.Value.ToString());
				Console.Write(" - Creating second release: #");
				releaseId2 = rMgr.Insert(
					1,
					projectId.Value,
					1,
					"Test Release 2",
					"Test Release 2",
					"1.5.0.0",
					"AAB",
					Release.ReleaseStatusEnum.Planned,
					Release.ReleaseTypeEnum.MinorRelease,
					DateTime.UtcNow.AddDays(0),
					DateTime.UtcNow.AddDays(4),
					3.2m,
					1.2m,
					null,
					true);
				Console.WriteLine(releaseId2.Value.ToString());

				//Let's create  arequirement, and makke some changes. This will create some history items.
				Console.WriteLine(" - For history ChangeSets, creating two requirements, and making changes to them.");
				RequirementManager rqMgr = new RequirementManager();
				requirementId1 = rqMgr.Insert(
					1,
					projectId.Value,
					releaseId1.Value,
					null,
					"AAA",
					Requirement.RequirementStatusEnum.InProgress,
					null,
					1,
					1,
					null,
					"Test Requirement 1",
					"Test Requirement for Baselines.",
					34.53m,
					1,
					true);
				requirementId2 = rqMgr.Insert(
					1,
					projectId.Value,
					releaseId2.Value,
					null,
					"AAB",
					Requirement.RequirementStatusEnum.InProgress,
					null,
					1,
					1,
					null,
					"Test Requirement 2",
					"Test Requirement for Baselines.",
					12.34m,
					1,
					true);
				Requirement req1 = rqMgr.RetrieveById3(projectId, requirementId1.Value, true);
				Requirement req2 = rqMgr.RetrieveById3(projectId, requirementId2.Value, true);
				req1.StartTracking();
				req2.StartTracking();
				req1.Description = "Updated Description for Requirement 1";
				req1.EndDate = DateTime.UtcNow.AddDays(2);
				req1.StartDate = DateTime.UtcNow.AddDays(-1);
				req2.Description = "Updated Description for Requiremente 2";
				req2.AuthorId = 2;
				rqMgr.Update(1, projectId.Value, new List<Requirement> { req1, req2 }, null, true);
				req1 = rqMgr.RetrieveById3(projectId, requirementId1.Value, true);
				req2 = rqMgr.RetrieveById3(projectId, requirementId2.Value, true);
				req1.StartTracking();
				req2.StartTracking();
				req1.Description = "Updated Description again for Requirement 1";
				req1.EndDate = DateTime.UtcNow.AddDays(1);
				req1.StartDate = DateTime.UtcNow;
				req2.Description = "Updated Description again for Requiremente 2";
				req1.EndDate = DateTime.UtcNow.AddDays(2);
				req1.StartDate = DateTime.UtcNow.AddDays(-1);
				rqMgr.Update(1, projectId.Value, new List<Requirement> { req1, req2 }, null, true);

				//Lets creats a couple incidents and update them too. That should give us enough history.
				Console.WriteLine(" - For history ChangeSets, creating two incidents, and making changes to them.");
				IncidentManager iMgr = new IncidentManager();
				incidentId1 = iMgr.Insert(
					projectId.Value,
					null,
					null,
					1,
					1,
					null,
					"Test Incident 1",
					"Test Baseline Incident",
					releaseId1.Value,
					null,
					null,
					null,
					null,
					DateTime.UtcNow.AddDays(-10),
					DateTime.UtcNow.AddDays(-2),
					null,
					5,
					7,
					null,
					null,
					null,
					1,
					true);
				incidentId2 = iMgr.Insert(
					projectId.Value,
					null,
					null,
					1,
					1,
					null,
					"Test Incident 2",
					"Another Test Baseline Incident",
					releaseId2.Value,
					null,
					null,
					null,
					null,
					DateTime.UtcNow.AddDays(-5),
					DateTime.UtcNow.AddDays(2),
					null,
					5,
					7,
					null,
					null,
					null,
					1,
					true);
				Incident in1 = iMgr.RetrieveById(incidentId1.Value, false, true, false);
				Incident in2 = iMgr.RetrieveById(incidentId2.Value, false, true, false);
				in1.StartTracking();
				in2.StartTracking();
				in1.ActualEffort = 14;
				in1.ClosedDate = DateTime.UtcNow;
				in1.OpenerId = 2;
				in2.StartDate = DateTime.UtcNow.AddDays(-6);
				in2.EndDate = DateTime.UtcNow.AddDays(1);
				iMgr.Update(in1, 1);
				iMgr.Update(in2, 2);
				in1 = iMgr.RetrieveById(incidentId1.Value, false, true, false);
				in2 = iMgr.RetrieveById(incidentId2.Value, false, true, false);
				in1.StartTracking();
				in2.StartTracking();
				in1.OwnerId = 2;
				in1.Rank = 5;
				in1.VerifiedReleaseId = releaseId2.Value;
				in2.ResolvedReleaseId = releaseId1.Value;
				iMgr.Update(in1, 1);
				iMgr.Update(in2, 2);

				//To mix things up, lets change a couple of the requirements again.
				req1 = rqMgr.RetrieveById3(projectId, requirementId1.Value, true);
				req2 = rqMgr.RetrieveById3(projectId, requirementId2.Value, true);
				req1.StartTracking();
				req2.StartTracking();
				req1.Description = "Second Update to Description for Requirement 1";
				req1.EndDate = DateTime.UtcNow.AddDays(5);
				req1.StartDate = DateTime.UtcNow;
				req2.Description = "Second Update to Description for Requiremente 2";
				req2.AuthorId = 1;
				rqMgr.Update(1, projectId.Value, new List<Requirement> { req1, req2 }, null, true);
				req1 = rqMgr.RetrieveById3(projectId, requirementId1.Value, true);
				req2 = rqMgr.RetrieveById3(projectId, requirementId2.Value, true);
				req1.StartTracking();
				req2.StartTracking();
				req1.Description = "Original Description for Requirement 1";
				req1.EndDate = DateTime.UtcNow.AddDays(1);
				req1.StartDate = DateTime.UtcNow.AddDays(-1);
				req2.Description = "Original Description for Requirement 2";
				req2.EndDate = DateTime.UtcNow.AddDays(1);
				req2.StartDate = DateTime.UtcNow.AddDays(0);
				rqMgr.Update(1, projectId.Value, new List<Requirement> { req1, req2 }, null, true);


				setupDone = true;
			}
			else
				Console.WriteLine("Setup already performed. Skipping.");

			Console.WriteLine("Setup Finished.");
		}

		/// <summary>Cleans up after testing is done</summary>
		[TestFixtureTearDown]
		public void TearDown()
		{
			Console.WriteLine(Environment.NewLine + "Baseline tests finished, cleaning up.");
			if (setupDone)
			{
				Console.WriteLine(" - Deleting the Project.");
				ProjectManager pMgr = new ProjectManager();

				//Get the project's Template.
				Project prj = pMgr.RetrieveById(projectId.Value);
				int templId = prj.ProjectTemplateId;

				//Delete the project.
				pMgr.Delete(1, projectId.Value);

				//Delete the template.
				Console.WriteLine(" - Deleting the Template.");
				TemplateManager tMgr = new TemplateManager();
				tMgr.Delete(1, templId);
			}

			Console.WriteLine("Finished.");
		}

		/// <summary>
		/// This tests the ability for the system to properly create baselines.
		/// </summary>
		[Test]
		[SpiraTestCase(2486)]
		[Description("This tests the ability for the system to properly create baselines.")]
		public void _01_CreateBaseline()
		{
			//Our manager.
			BaselineManager bMgr = new BaselineManager();

			//Set strings used for creation and checking.
			string name = "Create";
			string desc1 = "First one created, automatic ChangeSet Selection";
			string desc2 = "Second one created, manually specifying ChangeSet ID.";
			string desc3 = "Third created, using EF object.";
			string desc4 = "Fourth created. Should not actually <strong>BE</strong> created.";

			//We need to make sure that this project has some history changests, first. 
			//For testing, get the highest changeset in our project.
			var hist = new HistoryManager().RetrieveSetsByProjectId(projectId.Value, 0);
			long changeSetId = MaxChangeSetInProject();

			//Now let us create a Baseline.
			var baseline1 = bMgr.Baseline_Create(
				2,
				name + " 1",
				desc1,
				projectId.Value,
				releaseId1.Value);
			Assert.AreEqual(changeSetId, baseline1.ChangeSetId, "Newly created Baseline1 did not have correct ChangeSetId!");
			Assert.AreEqual(name + " 1", baseline1.Name, "Newly created Baseline1 did not have correct name!");
			Assert.AreEqual(desc1, baseline1.Description, "Newly created Baseline1 did not have correct description!");
			Assert.AreEqual(2, baseline1.CreatorUserId, "Newly created Baseline1 did not have correct creator!");
			Assert.AreEqual(true, baseline1.IsActive, "Newly created Baseline1 did not have correct Active flag!");
			Assert.AreEqual(true, baseline1.IsApproved, "Newly created Baseline1 did not have correct Approved flag!");
			Assert.AreEqual(false, baseline1.IsDeleted, "Newly created Baseline1 did not have correct Deleted flag!");
			Assert.AreEqual(projectId.Value, baseline1.ProjectId, "Newly created Baseline1 did not have correct ProjectId!");
			Assert.AreEqual(releaseId1.Value, baseline1.ReleaseId, "Newly created Baseline1 did not have correct ReleaseId!");
			baselineId1 = baseline1.BaselineId;

			//Let us try to manually specify a ChangeSet ID.
			var baseline2 = bMgr.Baseline_Insert(
				changeSetId - 1,
				1,
				name + " 2",
				desc2,
				projectId.Value,
				releaseId1.Value);
			Assert.AreEqual(changeSetId - 1, baseline2.ChangeSetId, "Newly created Baseline did not have correct ChangeSetId!");
			Assert.AreEqual(name + " 2", baseline2.Name, "Newly created Baseline1 did not have correct name!");
			Assert.AreEqual(desc2, baseline2.Description, "Newly created Baseline1 did not have correct description!");
			Assert.AreEqual(1, baseline2.CreatorUserId, "Newly created Baseline1 did not have correct creator!");
			Assert.AreEqual(true, baseline2.IsActive, "Newly created Baseline1 did not have correct Active flag!");
			Assert.AreEqual(true, baseline2.IsApproved, "Newly created Baseline1 did not have correct Approved flag!");
			Assert.AreEqual(false, baseline2.IsDeleted, "Newly created Baseline1 did not have correct Deleted flag!");
			Assert.AreEqual(projectId.Value, baseline2.ProjectId, "Newly created Baseline1 did not have correct ProjectId!");
			Assert.AreEqual(releaseId1.Value, baseline2.ReleaseId, "Newly created Baseline1 did not have correct ReleaseId!");
			baselineId2 = baseline2.BaselineId;

			//Create one with the object.
			ProjectBaseline baseline3 = new ProjectBaseline
			{
				Name = name + " 3",
				Description = desc3,
				ProjectId = projectId.Value,
				ReleaseId = releaseId2.Value,
				CreatorUserId = 2,
				IsActive = false,
				LastUpdateDate = DateTime.UtcNow.AddDays(10),
				ChangeSetId = changeSetId - 1,
				CreationDate = DateTime.UtcNow,
				ConcurrencyDate = DateTime.UtcNow,
				IsApproved = true
			};
			baseline3 = bMgr.Baseline_Insert(baseline3);
			Assert.AreEqual(changeSetId - 1, baseline3.ChangeSetId, "Newly created Baseline did not have correct ChangeSetId!");
			Assert.AreEqual(name + " 3", baseline3.Name, "Newly created Baseline1 did not have correct name!");
			Assert.AreEqual(desc3, baseline3.Description, "Newly created Baseline1 did not have correct description!");
			Assert.AreEqual(2, baseline3.CreatorUserId, "Newly created Baseline1 did not have correct creator!");
			Assert.AreEqual(false, baseline3.IsActive, "Newly created Baseline1 did not have correct Active flag!");
			Assert.AreEqual(true, baseline3.IsApproved, "Newly created Baseline1 did not have correct Approved flag!");
			Assert.AreEqual(false, baseline3.IsDeleted, "Newly created Baseline1 did not have correct Deleted flag!");
			Assert.AreEqual(projectId.Value, baseline3.ProjectId, "Newly created Baseline1 did not have correct ProjectId!");
			Assert.AreEqual(releaseId2.Value, baseline3.ReleaseId, "Newly created Baseline1 did not have correct ReleaseId!");
			baselineId3 = baseline3.BaselineId;


			//Try to create one with a bad ChangeSet ID.
			//Assert.Throws(
			//	typeof(EntityForeignKeyException),
			//	delegate
			//		{
			//			bMgr.Baseline_Insert(changeSetId + short.MaxValue, 1, name + " 4", desc4, projectId.Value, releaseId2.Value);
			//		},
			//	"Creating a Baseline with an invalid ChangeSet ID did not thrown an exception!");

			//Try to create one with an invalid ReleaseId. First, a release that does NOT exist at all..
			//Assert.Throws(
			//	typeof(ArgumentException),
			//	delegate
			//	{
			//		bMgr.Baseline_Insert(changeSetId, 1, name + " 4", null, projectId.Value, short.MaxValue);
			//	},
			//	"Creating a baseline with a non-existant ReleaseId does not throw exception!");

			//Try to create one with an invalid ReleaseId. Second, a release that is in the wrong Project.
			//Assert.Throws(
			//	typeof(ArgumentException),
			//	delegate
			//	{
			//		bMgr.Baseline_Insert(changeSetId, 1, name + " 5", null, projectId.Value, 1);
			//	},
			//	"Creating a baseline with a ReleaseId in the wrong project does not throw exception!");
		}

		/// <summary>
		/// This test verifies that retrieving baselines works as expected.
		/// </summary>
		[Test]
		[SpiraTestCase(2488)]
		[Description("This test verifies that retrieving baselines works as expected.")]
		public void _02_RetieveBaselines()
		{
			//Count the number of baselines that were created. Assuming everything in Test#1 passes, we should have three.
			int countOfRel1Baselines = 0, countOfRel2Baselines = 0;
			countOfRel1Baselines += (baselineId1.HasValue ? 1 : 0);
			countOfRel1Baselines += (baselineId2.HasValue ? 1 : 0);
			countOfRel2Baselines += (baselineId3.HasValue ? 1 : 0);

			//Our Baseline Manager.
			BaselineManager bMgr = new BaselineManager();

			//Lets pull all for the product.
			int count1 = bMgr.Baseline_Count(projectId.Value);
			//Assert.AreEqual(countOfRel1Baselines + countOfRel2Baselines, count1, "Total count in Project did not match!");

			//One release.
			int count2 = bMgr.Baseline_Count(projectId.Value, releaseId1.Value);
			Assert.AreEqual(countOfRel1Baselines, count2, "Count of Baselines in REL1 did not match!");

			//A different release.
			int count3 = bMgr.Baseline_Count(projectId.Value, releaseId2.Value);
			Assert.AreEqual(countOfRel2Baselines, count3, "Count of Baselines in REL2 did not match!");

			//An invalid release.
			int count4 = bMgr.Baseline_Count(projectId.Value, short.MaxValue);
			Assert.AreEqual(0, count4, "Count of invalid release baselines was not 0.");

			//Pull the actual baselines now.
			var bases1 = bMgr.Baseline_RetrieveForProduct(projectId.Value).OrderBy(b => b.BaselineId).ToList();
			Assert.AreEqual(countOfRel1Baselines + countOfRel2Baselines, bases1.Count, "Count was wrong!");
			Assert.AreEqual(baselineId1, bases1[0].BaselineId, "Baseline1 had the wrong ID.");
			Assert.AreEqual(baselineId2, bases1[1].BaselineId, "Baseline2 had the wrong ID.");
			Assert.AreEqual(baselineId3, bases1[2].BaselineId, "Baseline3 had the wrong ID.");
			Assert.AreEqual(countOfRel1Baselines, bases1.Count(b => b.ReleaseId == releaseId1.Value), "Count for REL1 was wrong!");
			Assert.AreEqual(countOfRel2Baselines, bases1.Count(b => b.ReleaseId == releaseId2.Value), "Count for REL2 was wrong!");

			//Same test, but DB-filtering on Release.
			var bases2 = bMgr.Baseline_RetrieveForProductRelease(projectId.Value, releaseId1.Value).OrderBy(b => b.BaselineId).ToList();
			Assert.AreEqual(countOfRel1Baselines, bases2.Count, "Count for Release-Filter was wrong!");
			Assert.AreEqual(baselineId1.Value, bases2[0].BaselineId, "Baseline1 had the wrong ID.");
			Assert.AreEqual(baselineId2.Value, bases2[1].BaselineId, "Baseline2 had the wrong ID.");

			//Verify that giving an incorrect release gives no resuts.
			var bases3 = bMgr.Baseline_RetrieveForProductRelease(projectId.Value, short.MaxValue);
			Assert.AreEqual(0, bases3.Count, "Invalid release resulted in a count!");

			//TODO: Setting a filter.
		}

		/// <summary>
		/// Tests that a Baseline can be modified.
		/// </summary>
		[Test]
		[SpiraTestCase(2487)]
		[Description("Tests that a Baseline can be modified.")]
		public void _03_UpdateBaseline()
		{
			//The manager.
			BaselineManager bMgr = new BaselineManager();

			//Get the first baseline.
			var baseline1 = bMgr.Baseline_RetrieveById(baselineId1.Value);
			Assert.IsNotNull(baseline1, "Could not pull baseline by ID!");

			//Make some changes!
			string newName = "Updated Baseline 1";
			string newDesc = "Update Description for Baseline2!";
			bool newActive = false;
			//Save it.
			var baseline1a = bMgr.Baseline_Update(baseline1.BaselineId, newName, newDesc, newActive);
			var baseline1b = bMgr.Baseline_RetrieveById(baselineId1.Value);
			Assert.AreEqual(newName, baseline1a.Name, "Name of returned Baseline did not match!");
			Assert.AreEqual(newName, baseline1b.Name, "Name of retrieved Baseline did not match!");
			Assert.AreEqual(newDesc, baseline1a.Description, "Description of returned Baseline did not match!");
			Assert.AreEqual(newDesc, baseline1b.Description, "Description of retrieved Baseline did not match!");
		}

		/// <summary>Tests deleting a baseline.</summary>
		[Test]
		[SpiraTestCase(2489)]
		[Description("Tests deleting a baseline.")]
		public void _04_DeleteBaseline()
		{
			//Our manager.
			BaselineManager bMgr = new BaselineManager();

			//First try to delete one that does not exist.
			var result1 = bMgr.Baseline_Delete(projectId.Value, short.MaxValue, 1);
			Assert.IsFalse(result1, "Somehow we deleted a baseline that did not exist!");

			//Try to delete a real baseline.
			var result2 = bMgr.Baseline_Delete(projectId.Value, baselineId1.Value, 1);
			Assert.IsTrue(result2, "We did not delete a baseline we should have!");

			//Try to delete the baseline *again*.
			var result3 = bMgr.Baseline_Delete(projectId.Value, baselineId1.Value, 1);
			Assert.IsFalse(result3, "Somehow we deleted a baseline that was already deleted.");
		}

		[Test]
        [SpiraTestCase(2508)]
		[Description("Tests retrieving the list of artifact changes in a baseline")]
		public void _05_Retrieve_Changes_in_Baseline()
		{
			//Our manager.
			BaselineManager baselineManager = new BaselineManager();

            //Get the list of changes in a baseline, no filters or sorts
            int count = 0;
            List<HistoryChangeSetNetChangeSquashed> historyEntries = baselineManager.Artifacts_ChangedBetweenChangesets(projectId.Value, baselineId3.Value, null, InternalRoutines.UTC_OFFSET,out count);
            Assert.AreEqual(6, historyEntries.Count);
            Assert.AreEqual(6, count);

            //Now lets sort and paginate
            SortFilter sortFilters = new SortFilter();
            sortFilters.PageSize = 5;
            sortFilters.SortProperty = "ChangeDate";
            sortFilters.SortAscending = true;
            historyEntries = baselineManager.Artifacts_ChangedBetweenChangesets(projectId.Value, baselineId3.Value, sortFilters, InternalRoutines.UTC_OFFSET, out count);
            Assert.AreEqual(5, historyEntries.Count);
            Assert.AreEqual(6, count);

            //Now filter by different fields

            //ArtifactTypeId = Requirement
            sortFilters.PageSize = 5;
            sortFilters.SortProperty = "ChangedArtifactId";
            sortFilters.SortAscending = false;
            sortFilters.FilterList = new System.Collections.Hashtable();
            sortFilters.FilterList.Add("ArtifactTypeId", (int)Artifact.ArtifactTypeEnum.Requirement);
            historyEntries = baselineManager.Artifacts_ChangedBetweenChangesets(projectId.Value, baselineId3.Value, sortFilters, InternalRoutines.UTC_OFFSET, out count);
            Assert.AreEqual(2, historyEntries.Count);
            Assert.AreEqual(2, count);

            //ArtifactTypeId = Requirement
            sortFilters.PageSize = 5;
            sortFilters.SortProperty = "ChangedArtifactId";
            sortFilters.SortAscending = false;
            sortFilters.FilterList = new System.Collections.Hashtable();
            sortFilters.FilterList.Add("ArtifactName", "Requirement");
            historyEntries = baselineManager.Artifacts_ChangedBetweenChangesets(projectId.Value, baselineId3.Value, sortFilters, InternalRoutines.UTC_OFFSET, out count);
            Assert.AreEqual(2, historyEntries.Count);
            Assert.AreEqual(2, count);
        }

        #region Private Functions
        private long MaxChangeSetInProject()
		{
			long count = 1;
			//We need to make sure that this project has some history changests, first. 
			//For testing, get the highest changeset in our project.
			var hist = new HistoryManager().RetrieveSetsByProjectId(projectId.Value, 0);
			if (hist.Count > 0)
			{
				count = hist.Max(g => g.ChangeSetId);
			}
			return count;
		}
		#endregion Private Functions
	}
}
