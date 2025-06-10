using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the SavedFilter business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class SavedFilterTest
	{
		private const int PROJECT_ID = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		private static int savedFilterId1;
		private static int savedFilterId2;

		private static int projectId;
		private static int projectTemplateId;

		[TestFixtureSetUp]
		public void Init()
		{
			//Create new projects for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("SavedFilterTest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary projects and templates
			new ProjectManager().Delete(USER_ID_FRED_BLOGGS, projectId);
			new TemplateManager().Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(398)
		]
		public void _01_ViewSavedFilters()
		{
			//First get the list of saved filters that are loaded as sample data - cross project
			SavedFilterManager savedFilterManager = new SavedFilterManager();
			List<SavedFilter> savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(6, savedFilters.Count);
			//1st Item
			Assert.AreEqual("Requirement", savedFilters[0].ArtifactTypeName);
			Assert.AreEqual("Critical Not-Covered Requirements", savedFilters[0].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[0].ProjectName);
			//2nd Item
			Assert.AreEqual("Test Case", savedFilters[1].ArtifactTypeName);
			Assert.AreEqual("Failed Active Test Cases", savedFilters[1].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[1].ProjectName);
			//3rd Item
			Assert.AreEqual("Incident", savedFilters[3].ArtifactTypeName);
			Assert.AreEqual("New Unassigned Incidents", savedFilters[3].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[3].ProjectName);

			//Next get the list of saved filters that are loaded as sample data - for a specific project
			savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID);
			Assert.AreEqual(6, savedFilters.Count);
			//1st Item
			Assert.AreEqual("Requirement", savedFilters[0].ArtifactTypeName);
			Assert.AreEqual("Critical Not-Covered Requirements", savedFilters[0].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[0].ProjectName);
			//2nd Item
			Assert.AreEqual("Test Case", savedFilters[1].ArtifactTypeName);
			Assert.AreEqual("Failed Active Test Cases", savedFilters[1].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[1].ProjectName);
			//3rd Item
			Assert.AreEqual("Incident", savedFilters[3].ArtifactTypeName);
			Assert.AreEqual("New Unassigned Incidents", savedFilters[3].Name);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[3].ProjectName);

			//Now we need to test that we can get the list of a users individual saved filters for a particular project and artifact type
			//Requirements
			savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(1, savedFilters.Count);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedFilters[0].UserId);
			Assert.AreEqual("Critical Not-Covered Requirements", savedFilters[0].Name);
			Assert.AreEqual(true, savedFilters[0].IsShared);
			//Incidents
			savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Incident, false);
			Assert.AreEqual(2, savedFilters.Count);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedFilters[0].UserId);
			Assert.AreEqual("All Reopened Incidents", savedFilters[0].Name);
			Assert.AreEqual(false, savedFilters[0].IsShared);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedFilters[1].UserId);
			Assert.AreEqual("New Unassigned Incidents", savedFilters[1].Name);
			Assert.AreEqual(false, savedFilters[1].IsShared);

			//Verify that Joe, has no individual filters
			savedFilters = savedFilterManager.Retrieve(USER_ID_JOE_SMITH, PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, false);
			Assert.AreEqual(0, savedFilters.Count);

			//Verify that Joe can access the shared filters
			savedFilters = savedFilterManager.Retrieve(USER_ID_JOE_SMITH, PROJECT_ID, DataModel.Artifact.ArtifactTypeEnum.Requirement, true);
			Assert.AreEqual(1, savedFilters.Count);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, savedFilters[0].UserId);
			Assert.AreEqual("Critical Not-Covered Requirements", savedFilters[0].Name);
			Assert.AreEqual(true, savedFilters[0].IsShared);
		}

		[
		Test,
		SpiraTestCase(400)
		]
		public void _02_RestoreAndSaveFilters()
		{
			//We'll use Joe Smith as he doesn't have any saved filters or existing filters set
			SavedFilterManager savedFilterManager = new SavedFilterManager();

			//First we need to actually generate a set of filters for requirements
			ProjectSettingsCollection filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "RequirementFiltersList");
			filtersCollection.Add("ImportanceId", 1);
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Completed);
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Accepted);
			filtersCollection.Add("RequirementStatusId", multiValueFilter);
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("1/1/2004");
			dateRange.EndDate = DateTime.Parse("1/10/2004");
			filtersCollection.Add("CreationDate", dateRange);
			filtersCollection.Save();

			//Now lets create a saved non-shared filter from this
			savedFilterManager.Save("Important Completed Requirements", DataModel.Artifact.ArtifactTypeEnum.Requirement, filtersCollection, false, false);

			//Now lets do the same for a set of filters and sort for incidents, but this time make it shared
			filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentFiltersList");
			filtersCollection.Add("PriorityId", 1);
			filtersCollection.Add("IncidentStatusId", 1);
			filtersCollection.Save();
			ProjectSettingsCollection sortCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentSortExpression");
			sortCollection.Add("SortExpression", "IncidentTypeName ASC");
			sortCollection.Save();
			savedFilterManager.Save("Saved Incidents Filter", DataModel.Artifact.ArtifactTypeEnum.Incident, filtersCollection, sortCollection, true, false);

			//Now verify that we can retrieve the list of filters
			List<SavedFilter> savedFilters = savedFilterManager.Retrieve(USER_ID_JOE_SMITH, null);
			Assert.AreEqual(2, savedFilters.Count);
			//1st Item
			savedFilterId1 = savedFilters[0].SavedFilterId;
			Assert.AreEqual("Requirement", savedFilters[0].ArtifactTypeName);
			Assert.AreEqual("Important Completed Requirements", savedFilters[0].Name);
			Assert.AreEqual(false, savedFilters[0].IsShared);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[0].ProjectName);
			//2nd Item
			savedFilterId2 = savedFilters[1].SavedFilterId;
			Assert.AreEqual("Incident", savedFilters[1].ArtifactTypeName);
			Assert.AreEqual("Saved Incidents Filter", savedFilters[1].Name);
			Assert.AreEqual(true, savedFilters[1].IsShared);
			Assert.AreEqual("Library Information System (Sample)", savedFilters[1].ProjectName);

			//Now verify that a third user can access the shared filters if the flag is set
			savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, Artifact.ArtifactTypeEnum.Incident, false);
			Assert.AreEqual(2, savedFilters.Count);
			savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, PROJECT_ID, Artifact.ArtifactTypeEnum.Incident, true);
			Assert.AreEqual(3, savedFilters.Count);

			//Now we need to reset the filters and sorts for the Joe Smith user - to make it a real test
			filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "RequirementFiltersList");
			filtersCollection.Restore();
			filtersCollection.Clear();
			filtersCollection.Save();
			filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentFiltersList");
			filtersCollection.Restore();
			filtersCollection.Clear();
			filtersCollection.Save();
			sortCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentSortExpression");
			sortCollection.Restore();
			sortCollection.Clear();
			sortCollection.Save();

			//Now restore back the saved filters and verify that the project collection values are set correctly

			//Requirements Filters
			savedFilterManager.Restore(savedFilterId1);
			Assert.AreEqual(PROJECT_ID, savedFilterManager.ProjectId);
			Assert.AreEqual(DataModel.Artifact.ArtifactTypeEnum.Requirement, savedFilterManager.Type);
			filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "RequirementFiltersList");
			savedFilterManager.Populate(filtersCollection, null, "");
			Assert.AreEqual(1, filtersCollection["ImportanceId"], "ImportanceId");
			MultiValueFilter newMultiValueFilter = (MultiValueFilter)filtersCollection["RequirementStatusId"];
			Assert.IsTrue(newMultiValueFilter.Values.Contains((int)Requirement.RequirementStatusEnum.Completed), "RequirementStatusId1");
			Assert.IsTrue(newMultiValueFilter.Values.Contains((int)Requirement.RequirementStatusEnum.Accepted), "RequirementStatusId2");
			DateRange newDateRange = (DateRange)filtersCollection["CreationDate"];
			Assert.AreEqual(DateTime.Parse("1/1/2004"), newDateRange.StartDate, "CreationDate.StartDate");
			Assert.AreEqual(DateTime.Parse("1/10/2004"), newDateRange.EndDate, "CreationDate.EndDate");

			//Incident Filters/Sort
			savedFilterManager.Restore(savedFilterId2);
			Assert.AreEqual(PROJECT_ID, savedFilterManager.ProjectId);
			Assert.AreEqual(DataModel.Artifact.ArtifactTypeEnum.Incident, savedFilterManager.Type);
			filtersCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentFiltersList");
			sortCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_JOE_SMITH, "IncidentSortExpression");
			savedFilterManager.Populate(filtersCollection, sortCollection, "SortExpression");
			Assert.AreEqual(1, filtersCollection["PriorityId"], "ImportanceId");
			Assert.AreEqual(1, filtersCollection["IncidentStatusId"], "RequirementStatusId");
			Assert.AreEqual("IncidentTypeName ASC", sortCollection["SortExpression"]);
		}

		[
		Test,
		SpiraTestCase(401)
		]
		public void _03_DeleteSavedFilters()
		{
			//Now delete the saved filters and verify that they deleted
			SavedFilterManager savedFilterManager = new SavedFilterManager();
			savedFilterManager.Delete(savedFilterId1);
			savedFilterManager.Delete(savedFilterId2);
			List<SavedFilter> savedFilters = savedFilterManager.Retrieve(USER_ID_JOE_SMITH, null);
			Assert.AreEqual(0, savedFilters.Count);
		}

		/// <summary>
		/// Tests that you can save the column selection, position and width with a filter (as an option)
		/// </summary>
		[
		Test,
		SpiraTestCase(2138)
		]
		public void _04_SaveColumnsWithFilter()
		{
			//We use a dedicated project for this test to avoid messing up the sample projects
			SavedFilterManager savedFilterManager = new SavedFilterManager();
			ArtifactManager artifactManager = new ArtifactManager();

			//Set an initial column width to test with
			artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId", 250);

			//First lets get the list of columns that are visible by default
			List<ArtifactListFieldDisplay> columns1 = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);

			//Lets make some changes
			artifactManager.ArtifactField_ToggleListVisibility(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId");
			artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId", 200);
			artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId", 2);

			//Verify (they should not be the same)
			List<ArtifactListFieldDisplay> columns2 = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			ArtifactListFieldDisplay severity1 = columns1.FirstOrDefault(f => f.Name == "SeverityId");
			ArtifactListFieldDisplay severity2 = columns2.FirstOrDefault(f => f.Name == "SeverityId");
			Assert.AreNotEqual(severity1.IsVisible, severity2.IsVisible);
			Assert.AreNotEqual(severity1.Width, severity2.Width);
			Assert.AreNotEqual(severity1.ListPosition, severity2.ListPosition);

			//Now lets save a filter
			ProjectSettingsCollection filtersCollection = new ProjectSettingsCollection(projectId, USER_ID_FRED_BLOGGS, "IncidentFiltersList");
			ProjectSettingsCollection sortCollection = new ProjectSettingsCollection(projectId, USER_ID_FRED_BLOGGS, "IncidentSortExpression");
			savedFilterManager.Save("Test with Columns", Artifact.ArtifactTypeEnum.Incident, filtersCollection, sortCollection, false, true);

			//Put the values back
			artifactManager.ArtifactField_ToggleListVisibility(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId");
			artifactManager.ArtifactField_ChangeColumnWidth(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId", 250);
			artifactManager.ArtifactField_ChangeListPosition(projectId, projectTemplateId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident, "SeverityId", 5);

			//Verify (they should be the same)
			List<ArtifactListFieldDisplay> columns3 = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			ArtifactListFieldDisplay severity3 = columns3.FirstOrDefault(f => f.Name == "SeverityId");
			Assert.AreEqual(severity1.IsVisible, severity3.IsVisible);
			Assert.AreEqual(severity1.Width, severity3.Width);
			Assert.AreEqual(severity1.ListPosition, severity3.ListPosition);

			//Restore the filter
			List<SavedFilter> savedFilters = savedFilterManager.Retrieve(USER_ID_FRED_BLOGGS, projectId);
			savedFilterManager.Restore(savedFilters[0].SavedFilterId);
			savedFilterManager.Populate(filtersCollection, sortCollection, "SortExpression");

			//Verify the column changes (should be the same as one set, but not the other)
			List<ArtifactListFieldDisplay> columns4 = artifactManager.ArtifactField_RetrieveForLists(projectId, USER_ID_FRED_BLOGGS, Artifact.ArtifactTypeEnum.Incident);
			ArtifactListFieldDisplay severity4 = columns4.FirstOrDefault(f => f.Name == "SeverityId");
			Assert.AreNotEqual(severity1.IsVisible, severity4.IsVisible);
			Assert.AreNotEqual(severity1.Width, severity4.Width);
			Assert.AreNotEqual(severity1.ListPosition, severity4.ListPosition);
			Assert.AreEqual(severity2.IsVisible, severity4.IsVisible);
			Assert.AreEqual(severity2.Width, severity4.Width);
			Assert.AreEqual(severity2.ListPosition, severity4.ListPosition);
		}

		/// <summary>
		/// Tests that you can update an existing filter
		/// </summary>
		[
		Test,
		SpiraTestCase(2143)
		]
		public void _05_UpdateExistingSavedFilter()
		{
			SavedFilterManager savedFilterManager = new SavedFilterManager();

			//First we need to actually generate a set of filters for requirements
			ProjectSettingsCollection filtersCollection = new ProjectSettingsCollection(projectId, USER_ID_JOE_SMITH, "RequirementFiltersList");
			filtersCollection.Add("ImportanceId", 1);
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Completed);
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Accepted);
			filtersCollection.Add("RequirementStatusId", multiValueFilter);
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("1/1/2004");
			dateRange.EndDate = DateTime.Parse("1/10/2004");
			filtersCollection.Add("CreationDate", dateRange);
			filtersCollection.Save();

			//Now lets create a saved non-shared filter from this
			savedFilterManager.Save("Important Completed Requirements", DataModel.Artifact.ArtifactTypeEnum.Requirement, filtersCollection, false, false);

			//Retrieve the filter by name
			List<SavedFilter> savedFilters = savedFilterManager.Retrieve(USER_ID_JOE_SMITH, projectId);
			SavedFilter savedFilter = savedFilters.FirstOrDefault(s => s.Name == "Important Completed Requirements");
			Assert.IsNotNull(savedFilter);
			int savedFilterId = savedFilter.SavedFilterId;

			//Retrieve and Verify
			savedFilter = savedFilterManager.RetrieveById(savedFilterId);
			Assert.IsNotNull(savedFilter);
			Assert.IsFalse(savedFilter.IsShared);
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "ImportanceId"));
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "RequirementStatusId"));
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "CreationDate"));

			//Make changes to the filter
			filtersCollection = new ProjectSettingsCollection(projectId, USER_ID_JOE_SMITH, "RequirementFiltersList");
			filtersCollection.Add("ImportanceId", 2);
			multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Completed);
			multiValueFilter.Values.Add((int)Requirement.RequirementStatusEnum.Accepted);
			filtersCollection.Add("RequirementStatusId", multiValueFilter);
			filtersCollection.Add("Name", "keyword");
			filtersCollection.Save();

			//Now update the filter and make it shared
			savedFilterManager.UpdateExisting(savedFilterId, Artifact.ArtifactTypeEnum.Requirement, filtersCollection, null, true, false);

			//Retrieve and Verify
			savedFilter = savedFilterManager.RetrieveById(savedFilterId);
			Assert.IsNotNull(savedFilter);
			Assert.IsTrue(savedFilter.IsShared);
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "ImportanceId"));
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "RequirementStatusId"));
			Assert.IsTrue(savedFilter.Entries.Any(s => s.EntryKey == "Name"));
			Assert.IsFalse(savedFilter.Entries.Any(s => s.EntryKey == "CreationDate"));

			//Clean up
			savedFilterManager.Delete(savedFilterId);
		}
	}
}
