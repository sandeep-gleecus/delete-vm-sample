using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using NUnit.Framework;
using System.Data;
using Inflectra.SpiraTest.DataModel;
using System.Text;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Incident business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class IncidentTest
	{
		protected static IncidentManager incidentManager;
		protected static int incidentId1;
		protected static int incidentId2;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		protected static int projectId;
		private static int projectTemplateId;
		protected static List<IncidentType> incidentTypes;
		protected static List<IncidentStatus> incidentStati;
		protected static List<IncidentPriority> incidentPriorities;
		protected static List<IncidentSeverity> incidentSeverities;

		private const int PROJECT_ID = 1;
		private const int PROJECT_TEMPLATE_ID = 1;
		private const int PROJECT_GROUP_ID = 2;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int PROJECT_EMPTY_ID = 3;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;


		[TestFixtureSetUp]
		public void Init()
		{
			incidentManager = new IncidentManager();

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("IncidentTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project");

			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Get the list of statuses, priorities, etc. for this project
			incidentTypes = incidentManager.RetrieveIncidentTypes(projectTemplateId, false);
			incidentStati = incidentManager.IncidentStatus_Retrieve(projectTemplateId, false);
			incidentPriorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, false);
			incidentSeverities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, false);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Purge any deleted items in the projects
			Business.HistoryManager history = new Business.HistoryManager();
			history.PurgeAllDeleted(PROJECT_ID, USER_ID_FRED_BLOGGS);
			history.PurgeAllDeleted(PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS);

			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);

			//We need to delete any artifact history items created
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());
		}

		[
		Test,
		SpiraTestCase(20)
		]
		public void _01_RetrieveIncidents()
		{
			//First lets test that we can retrieve the count of incidents in the system
			int incidentCount = incidentManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(60, incidentCount);

			//Now lets test that the details of the incident are being retrieved correctly
			List<IncidentView> incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents[0].IncidentId);
			Assert.AreEqual("Cannot log into the application", incidents[0].Name);
			Assert.AreEqual("When trying to log into the application with a valid username and password, the system throws a fatal exception", incidents[0].Description);
			Assert.AreEqual("New", incidents[0].IncidentStatusName);
			Assert.IsTrue(incidents[0].PriorityId.IsNull());
			Assert.IsTrue(incidents[0].SeverityName.IsNull());
			Assert.AreEqual("Fred Bloggs", incidents[0].OpenerName);
			Assert.AreEqual("1.0.0.0", incidents[0].DetectedReleaseVersionNumber);
			Assert.AreEqual("1.0.1.0", incidents[0].ResolvedReleaseVersionNumber);
			Assert.AreEqual("1.0.1.0", incidents[0].VerifiedReleaseVersionNumber);
			Assert.IsTrue(incidents[0].PriorityColor.IsNull());

			//Now lets test that we can retrieve the list of all incidents sorted and paginated appropriately
			//IncidentId ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(1, incidents[0].IncidentId);
			Assert.AreEqual(15, incidents[14].IncidentId);
			//IncidentId ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(16, incidents[0].IncidentId);
			Assert.AreEqual(30, incidents[14].IncidentId);
			//IncidentId descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(30, incidents[0].IncidentId);
			Assert.AreEqual(16, incidents[14].IncidentId);
			//IncidentId descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(15, incidents[0].IncidentId);
			Assert.AreEqual(4, incidents[11].IncidentId);

			//IncidentType ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentTypeName", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Bug", incidents[0].IncidentTypeName);
			Assert.AreEqual("Change Request", incidents[14].IncidentTypeName);
			//IncidentType ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentTypeName", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Change Request", incidents[0].IncidentTypeName);
			Assert.AreEqual("Enhancement", incidents[14].IncidentTypeName);
			//IncidentType descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentTypeName", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Enhancement", incidents[0].IncidentTypeName);
			Assert.AreEqual("Change Request", incidents[14].IncidentTypeName);
			//IncidentType descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentTypeName", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Change Request", incidents[0].IncidentTypeName);
			Assert.AreEqual("Bug", incidents[11].IncidentTypeName);

			//IncidentStatus ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusName", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Assigned", incidents[0].IncidentStatusName);
			Assert.AreEqual("Assigned", incidents[14].IncidentStatusName);
			//IncidentStatus ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusName", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Assigned", incidents[0].IncidentStatusName);
			Assert.AreEqual("Duplicate", incidents[14].IncidentStatusName);
			//IncidentStatus descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusName", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Duplicate", incidents[0].IncidentStatusName);
			Assert.AreEqual("Assigned", incidents[14].IncidentStatusName);
			//IncidentStatus descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusName", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Assigned", incidents[0].IncidentStatusName);
			Assert.AreEqual("Assigned", incidents[11].IncidentStatusName);

			//Importance ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(true, incidents[0].PriorityName.IsNull());
			Assert.AreEqual("1 - Critical", incidents[14].PriorityName);
			//Importance ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("1 - Critical", incidents[0].PriorityName);
			Assert.AreEqual("2 - High", incidents[14].PriorityName);
			//Importance descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("2 - High", incidents[0].PriorityName);
			Assert.AreEqual("1 - Critical", incidents[14].PriorityName);
			//Priorty descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("1 - Critical", incidents[0].PriorityName);
			Assert.AreEqual(true, incidents[11].PriorityName.IsNull());

			//Name ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Ability to associate multiple authors", incidents[0].Name);
			Assert.AreEqual("Cannot log into the application", incidents[14].Name);
			//Name ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Clicking on link throws fatal error", incidents[0].Name);
			Assert.AreEqual("Sample Problem 2", incidents[14].Name);
			//Name descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Sample Problem 2", incidents[0].Name);
			Assert.AreEqual("Clicking on link throws fatal error", incidents[14].Name);
			//Name descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Cannot log into the application", incidents[0].Name);
			Assert.AreEqual("Ability to delete multiple authors", incidents[11].Name);

			//Owner ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OwnerName", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(true, incidents[0].OwnerName.IsNull());
			Assert.AreEqual(true, incidents[14].OwnerName.IsNull());
			//Owner ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OwnerName", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(true, incidents[0].OwnerName.IsNull());
			Assert.AreEqual("Fred Bloggs", incidents[14].OwnerName);
			//Owner descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OwnerName", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Fred Bloggs", incidents[0].OwnerName);
			Assert.AreEqual(true, incidents[14].OwnerName.IsNull());
			//Owner descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OwnerName", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(true, incidents[0].OwnerName.IsNull());
			Assert.AreEqual(true, incidents[11].OwnerName.IsNull());

			//CreationDate ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "CreationDate", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			//Assert.AreEqual(15, incidents.Count);
			//Assert.IsTrue(incidents[0].CreationDate >= DateTime.UtcNow.AddDays(-125) && incidents[0].CreationDate <= DateTime.UtcNow.AddDays(-121), "IN:" + incidents[0].IncidentId + " wrong creation date");
			//Assert.IsTrue(incidents[14].CreationDate >= DateTime.UtcNow.AddDays(-98) && incidents[14].CreationDate <= DateTime.UtcNow.AddDays(-94), "IN:" + incidents[14].IncidentId + " wrong creation date");
			//CreationDate ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "CreationDate", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			//Assert.IsTrue(incidents[0].CreationDate >= DateTime.UtcNow.AddDays(-92) && incidents[0].CreationDate <= DateTime.UtcNow.AddDays(-88), "IN:" + incidents[0].IncidentId + " wrong creation date");
			//Assert.IsTrue(incidents[14].CreationDate >= DateTime.UtcNow.AddDays(-26) && incidents[14].CreationDate <= DateTime.UtcNow.AddDays(-22), "IN:" + incidents[14].IncidentId + " wrong creation date");
			//CreationDate descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "CreationDate", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			//Assert.IsTrue(incidents[0].CreationDate >= DateTime.UtcNow.AddDays(-26) && incidents[0].CreationDate <= DateTime.UtcNow.AddDays(-22), "IN:" + incidents[0].IncidentId + " wrong creation date");
			//Assert.IsTrue(incidents[14].CreationDate >= DateTime.UtcNow.AddDays(-92) && incidents[14].CreationDate <= DateTime.UtcNow.AddDays(-88), "IN:" + incidents[14].IncidentId + " wrong creation date");
			//CreationDate descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "CreationDate", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			//Assert.IsTrue(incidents[0].CreationDate >= DateTime.UtcNow.AddDays(-98) && incidents[0].CreationDate <= DateTime.UtcNow.AddDays(-94), "IN:" + incidents[0].IncidentId + " wrong creation date");
			//Assert.IsTrue(incidents[14].CreationDate >= DateTime.UtcNow.AddDays(-125) && incidents[14].CreationDate <= DateTime.UtcNow.AddDays(-121), "IN:" + incidents[14].IncidentId + " wrong creation date");

			//Opener ascending, first page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OpenerName", true, 1, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Fred Bloggs", incidents[0].OpenerName);
			Assert.AreEqual("Fred Bloggs", incidents[14].OpenerName);
			//Opener ascending, second page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OpenerName", true, 16, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Fred Bloggs", incidents[0].OpenerName);
			Assert.AreEqual("Fred Bloggs", incidents[14].OpenerName);
			//Opener descending, third page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OpenerName", false, 31, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Fred Bloggs", incidents[0].OpenerName);
			Assert.AreEqual("Fred Bloggs", incidents[14].OpenerName);
			//Opener descending, fourth page
			incidents = incidentManager.Retrieve(PROJECT_ID, "OpenerName", false, 46, 15, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual("Fred Bloggs", incidents[0].OpenerName);
			Assert.AreEqual("Fred Bloggs", incidents[11].OpenerName);

			//There was a weird issue that we need to test for. If we have a pagination size much smaller
			//that the number of records and we sort by a lookup field, it was not merging properly due to stable sort issues
			int count = incidentManager.Count(PROJECT_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(60, count);
			const int PAGE_SIZE = 5;

			//First for the Incident Id field
			incidents = new List<IncidentView>();
			for (int i = 0; i <= count; i += PAGE_SIZE)
			{
				incidents.AddRange(incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, (1 + i), PAGE_SIZE, null, InternalRoutines.UTC_OFFSET));
			}
			Assert.AreEqual(count, incidents.Count);

			//Next for a lookup (IncidentStatusName) field
			incidents = new List<IncidentView>();
			for (int i = 0; i <= count; i += PAGE_SIZE)
			{
				incidents.AddRange(incidentManager.Retrieve(PROJECT_ID, "IncidentStatusName", true, (1 + i), PAGE_SIZE, null, InternalRoutines.UTC_OFFSET));
			}
			Assert.AreEqual(count, incidents.Count);
		}

		[
		Test,
		SpiraTestCase(41)
		]
		public void _02_RetrieveIncidentsWithFilters()
		{
			Hashtable filters = new Hashtable();

			//Lets test that we can retrieve the count of incidents in the system with a filter applied
			//Filter by Type=Bug
			filters.Add("IncidentTypeId", 2);
			int incidentCount = incidentManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(14, incidentCount);
			//Filter by Importance=P3
			filters.Clear();
			filters.Add("PriorityId", 3);
			incidentCount = incidentManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(11, incidentCount);
			//Add on filter Name LIKE 'the' - also finds description matches
			filters.Add("Name", "the");
			incidentCount = incidentManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(10, incidentCount, "Count1");
			//Filter by Creation Date
			filters.Clear();
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.UtcNow.AddDays(-64);
			dateRange.EndDate = DateTime.UtcNow.AddDays(-61);
			filters.Add("CreationDate", dateRange);
			incidentCount = incidentManager.Count(PROJECT_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, incidentCount);

			//Lets test that we can retrieve the list of all incident type=bug sorted by incident id
			//IncidentId ascending, filter by IncidentType=Issue
			filters.Clear();
			filters.Add("IncidentTypeId", 4);
			List<IncidentView> incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, incidents.Count);
			Assert.AreEqual(30, incidents[0].IncidentId);
			Assert.AreEqual(37, incidents[7].IncidentId);
			//IncidentId ascending, filter by IncidentStatus=New
			filters.Clear();
			filters.Add("IncidentStatusId", 1);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);
			Assert.AreEqual(1, incidents[0].IncidentId);
			Assert.AreEqual(3, incidents[2].IncidentId);
			//Name ascending, filter by Importance=P3
			filters.Clear();
			filters.Add("PriorityId", 3);
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(11, incidents.Count);
			Assert.AreEqual(27, incidents[0].IncidentId);
			Assert.AreEqual(13, incidents[10].IncidentId);
			//Name ascending, filter by Name contains 'does' - also find description matches
			filters.Clear();
			filters.Add("Name", "does");
			incidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(6, incidents.Count);
			Assert.AreEqual(32, incidents[0].IncidentId);
			Assert.AreEqual(6, incidents[5].IncidentId);
			//Importance descending, filter by Owner is 'Fred Bloggs'
			filters.Clear();
			filters.Add("OwnerId", 2);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(10, incidents[0].IncidentId, "IncidentId1");
			Assert.AreEqual(21, incidents[14].IncidentId);
			//Importance descending, filter by Creation Date is '1/1/2005'
			filters.Clear();
			dateRange.Clear();
			dateRange.StartDate = DateTime.Parse("1/1/2005");
			dateRange.EndDate = DateTime.Parse("1/1/2005");
			filters.Add("CreationDate", dateRange);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, incidents.Count);
			//Importance descending, filter by Opener is 'Joe P Smith'
			filters.Clear();
			filters.Add("OpenerId", 3);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(15, incidents.Count);
			Assert.AreEqual(14, incidents[0].IncidentId);
			Assert.AreEqual(8, incidents[14].IncidentId);
			//Importance descending, filter by IncidentId is 15
			filters.Clear();
			filters.Add("IncidentId", "15"); //Make sure it can handle numeric strings vs. numeric types
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(15, incidents[0].IncidentId);

			//Now lets try filtering and sorting on each of the custom property types

			//Text: Filter on Notes LIKE 'This'
			filters.Clear();
			filters.Add("Custom_01", "This");
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//Multilist: Filter on Operating System = Windows 2003
			filters.Clear();
			filters.Add("Custom_02", 9);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//Multilist: Filter on Operating System = Windows 2003 or Windows 8
			filters.Clear();
			MultiValueFilter mvf = new MultiValueFilter();
			mvf.Values.Add(9);
			mvf.Values.Add(13);
			filters.Add("Custom_02", mvf);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);
			Assert.AreEqual(7, incidents[1].IncidentId);

			//Boolean: Filter on Internal = True
			filters.Clear();
			filters.Add("Custom_04", true);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(7, incidents[0].IncidentId);

			//Boolean: Filter on Internal = 'Y'
			filters.Clear();
			filters.Add("Custom_04", "Y");
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(7, incidents[0].IncidentId);

			//Integer: Filter on Rank >= 1 and Rank <= 2
			IntRange intRange = new IntRange();
			intRange.MinValue = 1;
			intRange.MaxValue = 2;
			filters.Clear();
			filters.Add("Custom_05", intRange);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);
			Assert.AreEqual(7, incidents[1].IncidentId);

			//Integer: Filter on Rank == 2
			filters.Clear();
			filters.Add("Custom_05", 2);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(7, incidents[0].IncidentId);

			//Integer: Filter on Rank == "2"
			filters.Clear();
			filters.Add("Custom_05", "2");
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(7, incidents[0].IncidentId);

			//Date: Filter on Review Date >= '7/1/2012' && Review Date <= '7/5/2012'
			dateRange = new DateRange();
			dateRange.StartDate = DateTime.Parse("7/1/2012");
			dateRange.EndDate = DateTime.Parse("7/5/2012");
			filters.Clear();
			filters.Add("Custom_06", dateRange);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);
			Assert.AreEqual(7, incidents[1].IncidentId);

			//List: Filter on Difficulty = Difficult
			filters.Clear();
			filters.Add("Custom_07", 1);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//List: Filter on Difficulty = Difficult or Easy
			mvf = new MultiValueFilter();
			mvf.Values.Add(1);
			mvf.Values.Add(3);
			filters.Clear();
			filters.Add("Custom_07", mvf);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//User: Filter on Reviewer = System Administrator
			filters.Clear();
			filters.Add("Custom_08", 1);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//User: Filter on Reviewer = System Administrator or Fred Bloggs
			mvf = new MultiValueFilter();
			mvf.Values.Add(1);
			mvf.Values.Add(2);
			filters.Clear();
			filters.Clear();
			filters.Add("Custom_08", mvf);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);
			Assert.AreEqual(7, incidents[1].IncidentId);

			//Decimal: Filter on Decimal == 1.2
			filters.Clear();
			filters.Add("Custom_09", 1.2M);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, incidents.Count);

			//Decimal: Filter on Decimal == "1.2"
			filters.Clear();
			filters.Add("Custom_09", "1.2");
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, incidents.Count);

			//Decimal: Filter on Decimal >= 1 && Decimal <= 2
			DecimalRange decRange = new DecimalRange();
			decRange.MinValue = 1M;
			decRange.MaxValue = 2M;
			filters.Clear();
			filters.Add("Custom_09", decRange);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(6, incidents[0].IncidentId);

			//Now filter on Type=Bug and sort by operating system descending
			//This time make sure we can retrieve the custom column values
			filters.Clear();
			filters.Add("IncidentTypeId", 2);
			incidents = incidentManager.Retrieve(PROJECT_ID, "Custom_02", false, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(14, incidents.Count);
			Assert.AreEqual(7, incidents[1].IncidentId);
			Assert.AreEqual(13, incidents[1].Custom_02.FromDatabaseSerialization_Int32());
			Assert.AreEqual("May be an array bounds issue", incidents[1].Custom_01.FromDatabaseSerialization_String());
			Assert.AreEqual(6, incidents[0].IncidentId);
			Assert.AreEqual(9, incidents[0].Custom_02.FromDatabaseSerialization_Int32());
			Assert.AreEqual("This may be hard to reproduce", incidents[0].Custom_01.FromDatabaseSerialization_String());

			//Now test that we can retrieve incidents with multiple statuses
			//Lets get all the issues that are New, Open, Assigned
			//Sort by priority/severity
			filters.Clear();
			filters.Add("IncidentTypeId", 4);
			MultiValueFilter multiValueFilter = new MultiValueFilter();
			multiValueFilter.Values.Add(InternalRoutines.INCIDENT_STATUS_NEW);
			multiValueFilter.Values.Add(InternalRoutines.INCIDENT_STATUS_ASSIGNED);
			multiValueFilter.Values.Add(InternalRoutines.INCIDENT_STATUS_OPEN);
			//Add the status list to the filter
			filters.Add("IncidentStatusId", multiValueFilter);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 1, 5, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, incidents.Count);
			//Make sure we only have the expected status codes
			for (int i = 0; i < incidents.Count; i++)
			{
				Assert.IsTrue(incidents[i].IncidentStatusId == InternalRoutines.INCIDENT_STATUS_NEW || incidents[i].IncidentStatusId == InternalRoutines.INCIDENT_STATUS_OPEN || incidents[i].IncidentStatusId == InternalRoutines.INCIDENT_STATUS_ASSIGNED);
			}

			//Now lets test that we can filter on the aggregate incident statuses
			//First lets get all closed issues
			filters.Clear();
			filters.Add("IncidentTypeId", IncidentManager.IncidentTypeId_AllIssues);
			filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllClosed);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 1, 50, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(4, incidents.Count);

			//Next lets get all open risks
			filters.Clear();
			filters.Add("IncidentTypeId", IncidentManager.IncidentTypeId_AllRisks);
			filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllOpen);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 1, 50, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);

			//Next lets get all incidents that have no priority and no owner
			filters.Clear();
			multiValueFilter = new MultiValueFilter();
			multiValueFilter.IsNone = true;
			filters.Add("OwnerId", multiValueFilter);
			filters.Add("PriorityId", multiValueFilter);
			incidents = incidentManager.Retrieve(PROJECT_ID, "PriorityName", true, 1, 50, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(8, incidents.Count);

			//Test that we can retrieve incidents by test case
			filters.Clear();
			filters.Add("TestCaseId", 4);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId ASC", true, 1, 500, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Not able to add new author", incidents[1].Name);

			//Test that we can retrieve incidents by test run
			int count = incidentManager.CountByTestRunId(9);
			Assert.AreEqual(1, count);
			filters.Clear();
			filters.Add("TestRunId", 9);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId ASC", true, 1, 500, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Cannot add a new book to the system", incidents[0].Name);

			//Test that we can retrieve incidents by test step
			filters.Clear();
			filters.Add("TestStepId", 10);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId ASC", true, 1, 500, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Not able to add new author", incidents[1].Name);

			//Test that we can retrieve incidents by test set
			count = incidentManager.CountByTestSetId(1);
			Assert.AreEqual(2, count);
			filters.Clear();
			filters.Add("TestSetId", 1);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentId ASC", true, 1, 500, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Not able to add new author", incidents[0].Name);
			Assert.AreEqual("Cannot add a new book to the system", incidents[1].Name);
		}

		[
		Test,
		SpiraTestCase(42)
		]
		public void _03_LookupRetrieves()
		{
			//Lets test that we can load the Incident Type lookup list for the current project
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(PROJECT_ID, true);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(8, incidentTypes.Count);
			Assert.AreEqual(6, incidentTypes[2].IncidentTypeId);
			Assert.AreEqual("Limitation", incidentTypes[2].Name);
			Assert.AreEqual(false, incidentTypes[2].IsIssue);
			Assert.AreEqual(false, incidentTypes[2].IsRisk);
			Assert.AreEqual(false, incidentTypes[2].IsDefault);

			//Lets test that we can load the Incident Status lookup list for the current project
			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(PROJECT_ID, true);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(8, incidentStati.Count);
			Assert.AreEqual(3, incidentStati[5].IncidentStatusId);
			Assert.AreEqual("Assigned", incidentStati[5].Name);
			Assert.AreEqual(true, incidentStati[5].IsOpenStatus);
			Assert.AreEqual(false, incidentStati[5].IsDefault);

			//Lets test that we can load the Priority lookup list for the current project
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(PROJECT_ID, true);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(4, incidentPriorities.Count);
			Assert.AreEqual(4, incidentPriorities[0].PriorityId);
			Assert.AreEqual("4 - Low", incidentPriorities[0].Name);
			Assert.AreEqual("f4f356", incidentPriorities[0].Color);

			//Lets test that we can load the Severity lookup list for the current project
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(PROJECT_ID, true);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(4, incidentSeverities.Count);
			Assert.AreEqual(3, incidentSeverities[1].SeverityId);
			Assert.AreEqual("3 - Medium", incidentSeverities[1].Name);
			Assert.AreEqual("f5d857", incidentSeverities[1].Color);

			//Lets test that we can load the the pagination option lookup list
			SortedList<int, int> paginationOptionList = incidentManager.GetPaginationOptions();
			Assert.AreEqual(7, paginationOptionList.Count);
			Assert.AreEqual(5, (int)paginationOptionList.Values[0]);
			Assert.AreEqual(15, (int)paginationOptionList.Values[1]);
			Assert.AreEqual(30, (int)paginationOptionList.Values[2]);
			Assert.AreEqual(50, (int)paginationOptionList.Values[3]);
			Assert.AreEqual(100, (int)paginationOptionList.Values[4]);
			Assert.AreEqual(250, (int)paginationOptionList.Values[5]);
			Assert.AreEqual(500, (int)paginationOptionList.Values[6]);

			//Verify that we can get the default incident type for a project
			int defaultIncidentTypeId = incidentManager.GetDefaultIncidentType(PROJECT_ID);
			Assert.AreEqual(1, defaultIncidentTypeId);

			//Verify that we can get the default incident status for a project
			int defaultIncidentStatusId = incidentManager.IncidentStatus_RetrieveDefault(PROJECT_ID).IncidentStatusId;
			Assert.AreEqual(1, defaultIncidentStatusId);
		}

		[
		Test,
		SpiraTestCase(43)
		]
		public void _04_CreateIncident()
		{
			//First lets actually test inserting a new incident (with no associated test run step)
			//Need to make sure name can handle upto 255 characters and description can handle
			//upto 8000 characters - in this case we don't associate a particular release
			incidentId1 = incidentManager.Insert(
				PROJECT_ID,
				null,
				null,
				2,
				null,
				null,
				"Test Incident " + InternalRoutines.RepeatString("tes\u05d0", 60),
				"This is a test incident " + InternalRoutines.RepeatString("tes\u05d0", 994),
				null,
				null,
				null,
				1,
				null,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Now lets make sure we can retrieve the submitted item
			Incident incident = incidentManager.RetrieveById(incidentId1, false);
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			Assert.AreEqual(incidentId1, incident.IncidentId);
			Assert.IsTrue(incident.PriorityId.IsNull());
			Assert.IsTrue(incident.SeverityId.IsNull());
			Assert.AreEqual(1, incident.IncidentStatusId);
			Assert.AreEqual(1, incident.IncidentTypeId);
			Assert.AreEqual(2, incident.OpenerId);
			Assert.IsTrue(incident.DetectedReleaseId.IsNull());
			Assert.IsTrue(incident.ResolvedReleaseId.IsNull());
			Assert.IsTrue(incident.VerifiedReleaseId.IsNull());
			Assert.IsTrue(incident.OwnerId.IsNull());
			Assert.AreEqual("Test Incident " + InternalRoutines.RepeatString("tes\u05d0", 60), incident.Name);
			Assert.AreEqual("This is a test incident " + InternalRoutines.RepeatString("tes\u05d0", 994), incident.Description);
			Assert.IsTrue(incident.StartDate.IsNull());
			Assert.IsTrue(incident.ClosedDate.IsNull());
			Assert.AreEqual(0, incident.CompletionPercent);
			Assert.IsTrue(incident.EstimatedEffort.IsNull());
			Assert.IsTrue(incident.ActualEffort.IsNull());
			Assert.IsTrue(incidentView.PriorityName.IsNull());
			Assert.AreEqual("New", incidentView.IncidentStatusName);
			Assert.AreEqual("Incident", incidentView.IncidentTypeName);
			Assert.AreEqual("Fred Bloggs", incidentView.OpenerName);
			Assert.IsTrue(incidentView.OwnerName.IsNull());
			Assert.AreEqual(PROJECT_ID, incident.ProjectId);

			//Now lets test inserting an incident with all the fields filled-out
			incidentId2 = incidentManager.Insert(
				PROJECT_ID,
				2,
				3,
				2,
				3,
				null,
				"Test Incident",
				"This is a test incident",
				1,
				2,
				3,
				1,
				1,
				DateTime.UtcNow,
				DateTime.Parse("10/5/2005"),
				DateTime.Parse("10/10/2005"),
				45,
				48,
				0,
				USER_ID_FRED_BLOGGS,
				null,
				null
				);

			//Now lets make sure we can retrieve the submitted item
			incident = incidentManager.RetrieveById(incidentId2, false);
			incidentView = incidentManager.RetrieveById2(incidentId2);
			Assert.AreEqual(incidentId2, incident.IncidentId);
			Assert.AreEqual(2, incident.PriorityId);
			Assert.AreEqual(3, incident.SeverityId);
			Assert.AreEqual(1, incident.IncidentStatusId);
			Assert.AreEqual(1, incident.IncidentTypeId);
			Assert.AreEqual(2, incident.OpenerId);
			Assert.AreEqual(1, incident.DetectedReleaseId);
			Assert.AreEqual(2, incident.ResolvedReleaseId);
			Assert.AreEqual(3, incident.VerifiedReleaseId);
			Assert.AreEqual(3, incident.OwnerId);
			Assert.AreEqual("Test Incident", incident.Name);
			Assert.AreEqual("This is a test incident", incident.Description);
			Assert.AreEqual(DateTime.Parse("10/5/2005"), incident.StartDate);
			Assert.AreEqual(DateTime.Parse("10/10/2005"), incident.ClosedDate);
			Assert.AreEqual(100, incident.CompletionPercent);
			Assert.AreEqual(45, incident.EstimatedEffort);
			Assert.AreEqual(48, incident.ActualEffort);
			Assert.AreEqual("2 - High", incidentView.PriorityName);
			Assert.AreEqual("New", incidentView.IncidentStatusName);
			Assert.AreEqual("Incident", incidentView.IncidentTypeName);
			Assert.AreEqual("Fred Bloggs", incidentView.OpenerName);
			Assert.AreEqual("Joe P Smith", incidentView.OwnerName);
			Assert.AreEqual(PROJECT_ID, incident.ProjectId);

			//Now delete this second incident (since not used later)
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId2, USER_ID_FRED_BLOGGS);

			//Test that we can create a new unsaved incident 'shell' that is prepopulated with the creator's user id
			//and the default incident type and status
			incidentView = incidentManager.Incident_New(PROJECT_ID, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, incidentView.OpenerId);
			Assert.AreEqual(PROJECT_ID, incidentView.ProjectId);
			Assert.AreEqual(1, incidentView.IncidentStatusId);
			Assert.AreEqual(1, incidentView.IncidentTypeId);
			Assert.AreEqual("", incidentView.Name);
			Assert.AreEqual("", incidentView.Description);
			Assert.AreEqual(false, incidentView.IsAttachments);
			Assert.IsTrue(incident.CreationDate > DateTime.UtcNow.AddHours(-1));
		}

		[
		Test,
		SpiraTestCase(44)
		]
		public void _05_UpdateIncident()
		{
			//Lets make some updates
			Incident incident = incidentManager.RetrieveById(incidentId1, false);
			incident.StartTracking();
			incident.IncidentStatusId = 2; //Open
			incident.IncidentTypeId = 2; //Bug
			incident.PriorityId = 2; //2 - High
			incident.SeverityId = 3; //3 - Medium
			incident.Name = "Test Bug";
			incident.Description = "This is a test bug";
			incident.DetectedReleaseId = 1;
			incident.ResolvedReleaseId = 2;
			incident.VerifiedReleaseId = 3;

			//Verify that the data is good
			Dictionary<string, string> messages = incidentManager.Validate(incident);
			Assert.AreEqual(0, messages.Count, "Validate");

			//Make the update
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now lets make sure we can retrieve the updated item
			incident = incidentManager.RetrieveById(incidentId1, false);
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			Assert.AreEqual(incidentId1, incident.IncidentId);
			Assert.AreEqual(2, incident.PriorityId);
			Assert.AreEqual(3, incident.SeverityId);
			Assert.AreEqual(2, incident.IncidentStatusId);
			Assert.AreEqual(2, incident.IncidentTypeId);
			Assert.AreEqual(2, incident.OpenerId);
			Assert.AreEqual(1, incident.DetectedReleaseId);
			Assert.AreEqual(2, incident.ResolvedReleaseId);
			Assert.AreEqual(3, incident.VerifiedReleaseId);
			Assert.IsTrue(incident.OwnerId.IsNull());
			Assert.AreEqual("Test Bug", incident.Name);
			Assert.AreEqual("This is a test bug", incident.Description);
			Assert.IsTrue(incident.ClosedDate.IsNull());
			Assert.AreEqual("2 - High", incidentView.PriorityName);
			Assert.AreEqual("3 - Medium", incidentView.SeverityName);
			Assert.AreEqual("Open", incidentView.IncidentStatusName);
			Assert.AreEqual("Bug", incidentView.IncidentTypeName);
			Assert.AreEqual("Fred Bloggs", incidentView.OpenerName);
			Assert.IsTrue(incidentView.OwnerName.IsNull());

			//Now we need to verify that the validation routines catch certain issues
			incident = incidentManager.RetrieveById(1, false);

			//Completion percentage in range 0-100%
			incident.StartTracking();
			incident.CompletionPercent = 101;
			messages = incidentManager.Validate(incident);
			Assert.AreNotEqual(0, messages.Count);
			incident.CompletionPercent = -2;
			messages = incidentManager.Validate(incident);
			Assert.AreNotEqual(0, messages.Count);

			//Reset the completion percentage
			incident.CompletionPercent = 0;

			//We now allow the start date to be before/after the closed date
			//The previous restriction caused issue with data-syncs
			incident.StartDate = incident.CreationDate.AddDays(2);
			incident.ClosedDate = incident.CreationDate.AddDays(1);
			messages = incidentManager.Validate(incident);
			Assert.AreEqual(0, messages.Count); //It allows it, no messages

			//The start date cannot be before the detected date still
			incident.StartDate = incident.CreationDate.AddDays(-5);
			messages = incidentManager.Validate(incident);
			Assert.AreEqual(1, messages.Count);
		}

		[
		Test,
		SpiraTestCase(334)
		]
		public void _06_AddRetrieveResolutions()
		{
			//Lets add a resolution to an existing incident
			Incident incident = incidentManager.RetrieveById(incidentId1, true);
			Assert.AreEqual(0, incident.Resolutions.Count);
			incident.StartTracking();
			IncidentResolution incidentResolution = new IncidentResolution();
			incidentResolution.CreatorId = USER_ID_FRED_BLOGGS;
			incidentResolution.Resolution = "I think we've cracked the problem captain";
			incidentResolution.CreationDate = DateTime.UtcNow;
			incident.Resolutions.Add(incidentResolution);
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now lets retrieve the resolution
			incident = incidentManager.RetrieveById(incidentId1, true);
			Assert.AreEqual(1, incident.Resolutions.Count);
			Assert.AreEqual("Fred Bloggs", incident.Resolutions[0].Creator.FullName);
			Assert.AreEqual("I think we've cracked the problem captain", incident.Resolutions[0].Resolution);

			//Now lets add another resolution
			incident.StartTracking();
			incidentResolution = new IncidentResolution();
			incidentResolution.CreatorId = USER_ID_JOE_SMITH;
			incidentResolution.Resolution = "It's no use we're dead in space";
			incidentResolution.CreationDate = DateTime.UtcNow;
			incident.Resolutions.Add(incidentResolution);
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now lets retrieve both resolutions
			incident = incidentManager.RetrieveById(incidentId1, true);
			Assert.AreEqual(2, incident.Resolutions.Count);
			List<IncidentResolution> resolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();
			Assert.AreEqual("Fred Bloggs", resolutions[0].Creator.FullName);
			Assert.AreEqual("I think we've cracked the problem captain", resolutions[0].Resolution);
			Assert.AreEqual("Joe P Smith", resolutions[1].Creator.FullName);
			Assert.AreEqual("It's no use we're dead in space", resolutions[1].Resolution);

			//Now lets update a resolution and verify change
			incident = incidentManager.RetrieveById(incidentId1, true);
			incident.Resolutions[0].StartTracking();
			incident.Resolutions[0].Resolution = "There's klingons on the starboard bow";
			incident.Resolutions[1].StartTracking();
			incident.Resolutions[1].Resolution = "Scrape them off Jim";
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId1, true);
			resolutions = incident.Resolutions.OrderBy(r => r.CreationDate).ToList();
			Assert.AreEqual(2, resolutions.Count);
			Assert.AreEqual("Fred Bloggs", resolutions[0].Creator.FullName);
			Assert.AreEqual("There's klingons on the starboard bow", resolutions[0].Resolution);
			Assert.AreEqual("Joe P Smith", resolutions[1].Creator.FullName);
			Assert.AreEqual("Scrape them off Jim", resolutions[1].Resolution);

			//Now lets delete a resolution and verify
			incident = incidentManager.RetrieveById(incidentId1, true);
			incident.StartTracking();
			incident.Resolutions[0].MarkAsDeleted();
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId1, true);
			Assert.AreEqual(1, incident.Resolutions.Count);
			Assert.AreEqual("Joe P Smith", incident.Resolutions[0].Creator.FullName);
			Assert.AreEqual("Scrape them off Jim", incident.Resolutions[0].Resolution);

			//Now lets test that we can add a resolution using the simple add method
			int incidentResolutionId = incidentManager.InsertResolution(incidentId1, "Test Resolution", DateTime.UtcNow, USER_ID_JOE_SMITH, false);
			incident = incidentManager.RetrieveById(incidentId1, true);
			Assert.AreEqual(2, incident.Resolutions.Count);
			Assert.AreEqual(incidentResolutionId, incident.Resolutions[1].IncidentResolutionId);
			Assert.AreEqual("Joe P Smith", incident.Resolutions[1].Creator.FullName);
			Assert.AreEqual("Test Resolution", incident.Resolutions[1].Resolution);

			//Lets test that we can retrieve a resolution by its ID
			incidentResolution = incidentManager.Resolution_RetrieveById(PROJECT_ID, incidentId1, incidentResolutionId);
			Assert.IsNotNull(incidentResolution);
			Assert.AreEqual(incidentResolutionId, incidentResolution.IncidentResolutionId);
			Assert.AreEqual("Joe P Smith", incidentResolution.Creator.FullName);
			Assert.AreEqual("Test Resolution", incidentResolution.Resolution);

			//Lets delete this remaining resolution using the alternate Delete method
			incidentManager.Resolution_Delete(PROJECT_ID, incidentId1, incidentResolutionId, USER_ID_FRED_BLOGGS);

			//Verify the deletion
			bool assertThrown = false;
			try
			{
				incidentResolution = incidentManager.Resolution_RetrieveById(PROJECT_ID, incidentId1, incidentResolutionId);
			}
			catch (ArtifactNotExistsException)
			{
				assertThrown = true;
			}
			Assert.IsTrue(assertThrown);
		}

		[
		Test,
		SpiraTestCase(45)
		]
		public void _07_DeleteIncident()
		{
			//Now lets make sure we can delete the incident (including any associated resolutions)
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId1, USER_ID_FRED_BLOGGS);

			//Make sure it's deleted
			bool artifactExists = true;
			try
			{
				Incident incident = incidentManager.RetrieveById(incidentId1, false);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Incident not deleted correctly");

			//Now using the view method
			artifactExists = true;
			try
			{
				IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			}
			catch (ArtifactNotExistsException)
			{
				artifactExists = false;
			}
			Assert.IsFalse(artifactExists, "Incident not deleted correctly");
		}

		[
		Test,
		SpiraTestCase(47)
		]
		public void _09_RetrieveSummaryData()
		{
			//First get the incident summary untyped dataset -- no incident filter - all releases - using priority
			System.Data.DataSet summaryDataSet = incidentManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, null, null, false, false);

			//Make sure the data is as expected
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentSummary"].Rows.Count);
			Assert.AreEqual("Closed", summaryDataSet.Tables["IncidentSummary"].Rows[3]["IncidentStatusName"]);
			Assert.AreEqual(13, summaryDataSet.Tables["IncidentSummary"].Rows[3]["Total"]);
			Assert.AreEqual("Duplicate", summaryDataSet.Tables["IncidentSummary"].Rows[1]["IncidentStatusName"]);
			Assert.AreEqual(2, summaryDataSet.Tables["IncidentSummary"].Rows[1]["1"]);

			//Next get the incident summary untyped dataset -- no incident filter - specific detected release - using severity
			summaryDataSet = incidentManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, null, 1, true, false);

			//Make sure the data is as expected
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentSummary"].Rows.Count);
			Assert.AreEqual("Closed", summaryDataSet.Tables["IncidentSummary"].Rows[3]["IncidentStatusName"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentSummary"].Rows[3]["Total"]);
			Assert.AreEqual("Not Reproducible", summaryDataSet.Tables["IncidentSummary"].Rows[2]["IncidentStatusName"]);
			Assert.AreEqual(DBNull.Value, summaryDataSet.Tables["IncidentSummary"].Rows[2]["1"]);

			//Next get the incident summary untyped dataset -- no incident filter - specific resolved release - using priority
			summaryDataSet = incidentManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, null, 1, false, true);

			//Make sure the data is as expected
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentSummary"].Rows.Count);
			Assert.AreEqual("Closed", summaryDataSet.Tables["IncidentSummary"].Rows[3]["IncidentStatusName"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentSummary"].Rows[3]["Total"]);
			Assert.AreEqual("Not Reproducible", summaryDataSet.Tables["IncidentSummary"].Rows[2]["IncidentStatusName"]);
			Assert.AreEqual(DBNull.Value, summaryDataSet.Tables["IncidentSummary"].Rows[2]["1"]);

			//Next get the incident summary untyped dataset -- release that has iterations containing data - using priority
			summaryDataSet = incidentManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, null, 4, false, false);

			//Make sure the data is as expected
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentSummary"].Rows.Count);
			Assert.AreEqual("Reopen", summaryDataSet.Tables["IncidentSummary"].Rows[0]["IncidentStatusName"]);
			Assert.AreEqual(0, summaryDataSet.Tables["IncidentSummary"].Rows[0]["Total"]);
			Assert.AreEqual("Assigned", summaryDataSet.Tables["IncidentSummary"].Rows[5]["IncidentStatusName"]);
			//Assert.AreEqual(1, summaryDataSet.Tables["IncidentSummary"].Rows[5]["1"]);

			//Now get the incident summary untyped dataset -- for just the bugs - all releases - using severity
			summaryDataSet = incidentManager.RetrieveProjectSummary(PROJECT_ID, PROJECT_TEMPLATE_ID, 2, null, true, false);

			//Make sure the data is as expected
			Assert.AreEqual(8, summaryDataSet.Tables["IncidentSummary"].Rows.Count);
			Assert.AreEqual("Closed", summaryDataSet.Tables["IncidentSummary"].Rows[3]["IncidentStatusName"]);
			Assert.AreEqual(3, summaryDataSet.Tables["IncidentSummary"].Rows[3]["Total"]);
			Assert.AreEqual("New", summaryDataSet.Tables["IncidentSummary"].Rows[7]["IncidentStatusName"]);
			//Assert.AreEqual(1, summaryDataSet.Tables["IncidentSummary"].Rows[7]["2"]);

			//Now get the list of "open" incidents generated for a particular test case
			List<IncidentView> incidents = incidentManager.RetrieveByTestCaseId(2, true);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Cannot add a new book to the system", incidents[0].Name);
			Assert.AreEqual(1, incidents[0].PriorityId);
			Assert.AreEqual(InternalRoutines.INCIDENT_STATUS_ASSIGNED, incidents[0].IncidentStatusId);
			Assert.AreEqual(2, incidents[0].IncidentTypeId);

			//Now get the list of all incidents generated for a particular test case
			incidents = incidentManager.RetrieveByTestCaseId(2, false);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Cannot add a new book to the system", incidents[0].Name);
			Assert.AreEqual(1, incidents[0].PriorityId);
			Assert.AreEqual(InternalRoutines.INCIDENT_STATUS_ASSIGNED, incidents[0].IncidentStatusId);
			Assert.AreEqual(2, incidents[0].IncidentTypeId);


			//Now get the list of "open" incidents owned by a particular user - cross project
			incidents = incidentManager.RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, null, null);
			Assert.AreEqual(9, incidents.Count);
			Assert.AreEqual("Ability to associate multiple authors", incidents[0].Name);
			Assert.AreEqual(1, incidents[0].PriorityId);
			Assert.AreEqual("Sample Problem 3", incidents[8].Name);
			Assert.AreEqual(4, incidents[8].PriorityId);
			Assert.AreEqual("Library Information System (Sample)", incidents[8].ProjectName);

			//Now get the list of "open" incidents owned by a particular user - specific project
			incidents = incidentManager.RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, PROJECT_ID, null);
			Assert.AreEqual(9, incidents.Count);
			Assert.AreEqual("Ability to associate multiple authors", incidents[0].Name);
			Assert.AreEqual(1, incidents[0].PriorityId);
			Assert.AreEqual("Sample Problem 3", incidents[8].Name);
			Assert.AreEqual(4, incidents[8].PriorityId);
			Assert.AreEqual("Library Information System (Sample)", incidents[8].ProjectName);

			//Now get the list of "open" incidents owned by a specific user for a specific release (used in planning board)
			incidents = incidentManager.RetrieveOpenByOwnerId(USER_ID_FRED_BLOGGS, PROJECT_ID, 4);
			Assert.AreEqual(0, incidents.Count);

			//Now get the list of "open" incidents not owned by any user in a specific release (used in planning board)
			incidents = incidentManager.RetrieveOpenByOwnerId(null, PROJECT_ID, 4);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Cannot install system on Oracle 9i", incidents[0].Name);
			Assert.AreEqual("The book listing screen doesn't sort", incidents[1].Name);

			//Now get the list of "open" incidents detected by a particular user - cross project
			incidents = incidentManager.RetrieveOpenByOpenerId(USER_ID_FRED_BLOGGS, null);
			Assert.AreEqual(17, incidents.Count);
			Assert.AreEqual("User expectations from old client app", incidents[5].Name);
			Assert.AreEqual("4 - Low", incidents[5].PriorityName);
			Assert.AreEqual("Sample Problem 1", incidents[4].Name);
			Assert.AreEqual("1 - Critical", incidents[4].PriorityName);
			Assert.AreEqual("Library Information System (Sample)", incidents[4].ProjectName);

			//Now get the list of "open" incidents detected by a particular user - for a specific project
			incidents = incidentManager.RetrieveOpenByOpenerId(USER_ID_FRED_BLOGGS, PROJECT_ID);
			Assert.AreEqual(17, incidents.Count);
			Assert.AreEqual("User expectations from old client app", incidents[5].Name);
			Assert.AreEqual("4 - Low", incidents[5].PriorityName);
			Assert.AreEqual("Sample Problem 3", incidents[15].Name);
			Assert.AreEqual("4 - Low", incidents[15].PriorityName);
			Assert.AreEqual("Library Information System (Sample)", incidents[15].ProjectName);

			//Retrieve the top open issues for the project - using priority
			incidents = incidentManager.RetrieveOpenIssues(PROJECT_ID, null, 5, false);
			Assert.AreEqual(4, incidents.Count);
			Assert.AreEqual("Ability to be accessed by Mozilla", incidents[1].Name);
			Assert.AreEqual("2 - High", incidents[1].PriorityName);
			Assert.AreEqual("Issue", incidents[1].IncidentTypeName);
			Assert.AreEqual("Assigned", incidents[1].IncidentStatusName);
			Assert.AreEqual("f29e56", incidents[1].PriorityColor);

			//Retrieve the top open issues for a release that contains iterations - using severity
			incidents = incidentManager.RetrieveOpenIssues(PROJECT_ID, 4, 5, true);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("System may require process changes", incidents[0].Name);

			//Retrieve the top open risks for the project - using priority
			incidents = incidentManager.RetrieveOpenRisks(PROJECT_ID, null, 5, false);
			Assert.AreEqual(3, incidents.Count);
			Assert.AreEqual("Sample Problem 1", incidents[0].Name);
			Assert.AreEqual("1 - Critical", incidents[0].PriorityName);
			Assert.AreEqual("Problem", incidents[0].IncidentTypeName);
			Assert.AreEqual("Open", incidents[0].IncidentStatusName);
			Assert.AreEqual("f47457", incidents[0].PriorityColor);

			//Retrieve the top open risks for a release that contains iterations - using severity
			incidents = incidentManager.RetrieveOpenRisks(PROJECT_ID, 4, 5, true);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Sample Problem 1", incidents[0].Name);

			//Get the summary open count - all releases by priority
			List<IncidentOpenCountByPrioritySeverity> incidentOpenCount = incidentManager.RetrieveOpenCountByPrioritySeverity(PROJECT_ID, null, false, false);
			Assert.AreEqual(5, incidentOpenCount.Count);
			Assert.AreEqual("(None)", incidentOpenCount[0].PrioritySeverityName);
			Assert.AreEqual(7, incidentOpenCount[0].Count);
			Assert.AreEqual("2 - High", incidentOpenCount[2].PrioritySeverityName);
			Assert.AreEqual(9, incidentOpenCount[2].Count);

			//Get the summary open count - all releases by severity
			incidentOpenCount = incidentManager.RetrieveOpenCountByPrioritySeverity(PROJECT_ID, null, true, false);
			Assert.AreEqual(5, incidentOpenCount.Count);
			Assert.AreEqual("(None)", incidentOpenCount[0].PrioritySeverityName);
			Assert.AreEqual(15, incidentOpenCount[0].Count);
			Assert.AreEqual("2 - High", incidentOpenCount[2].PrioritySeverityName);
			Assert.AreEqual(6, incidentOpenCount[2].Count);

			//Get the summary open count - specific detected release by priority
			incidentOpenCount = incidentManager.RetrieveOpenCountByPrioritySeverity(PROJECT_ID, 1, false, false);
			Assert.AreEqual(2, incidentOpenCount.Count);
			Assert.AreEqual("(None)", incidentOpenCount[0].PrioritySeverityName);
			Assert.AreEqual(2, incidentOpenCount[0].Count);
			Assert.AreEqual("1 - Critical", incidentOpenCount[1].PrioritySeverityName);
			Assert.AreEqual(1, incidentOpenCount[1].Count);

			//Get the summary open count - specific detected release by severity
			incidentOpenCount = incidentManager.RetrieveOpenCountByPrioritySeverity(PROJECT_ID, 1, true, false);
			Assert.AreEqual(2, incidentOpenCount.Count);
			Assert.AreEqual("(None)", incidentOpenCount[0].PrioritySeverityName);
			Assert.AreEqual(2, incidentOpenCount[0].Count);
			Assert.AreEqual("3 - Medium", incidentOpenCount[1].PrioritySeverityName);
			Assert.AreEqual(1, incidentOpenCount[1].Count);


			//Get the summary open count - specific resolved release by severity
			incidentOpenCount = incidentManager.RetrieveOpenCountByPrioritySeverity(PROJECT_ID, 2, true, true);
			Assert.AreEqual(2, incidentOpenCount.Count);
			Assert.AreEqual("(None)", incidentOpenCount[0].PrioritySeverityName);
			Assert.AreEqual(2, incidentOpenCount[0].Count);
			Assert.AreEqual("3 - Medium", incidentOpenCount[1].PrioritySeverityName);
			Assert.AreEqual(1, incidentOpenCount[1].Count);

			//Get the aging summary - all releases
			summaryDataSet = incidentManager.RetrieveAging(PROJECT_ID, null, 90, 15);
			Assert.AreEqual(7, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("0-15", (string)summaryDataSet.Tables["IncidentAging"].Rows[0]["Age"]);
			Assert.AreEqual(3, (int)summaryDataSet.Tables["IncidentAging"].Rows[0]["Count"]);
			Assert.AreEqual("> 90", (string)summaryDataSet.Tables["IncidentAging"].Rows[6]["Age"]);
			Assert.AreEqual(9, (int)summaryDataSet.Tables["IncidentAging"].Rows[6]["Count"]);

			//Get the aging summary - specific release
			summaryDataSet = incidentManager.RetrieveAging(PROJECT_ID, 1, 90, 15);
			Assert.AreEqual(7, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("0-15", (string)summaryDataSet.Tables["IncidentAging"].Rows[0]["Age"]);
			Assert.AreEqual(0, (int)summaryDataSet.Tables["IncidentAging"].Rows[0]["Count"]);
			Assert.AreEqual("> 90", (string)summaryDataSet.Tables["IncidentAging"].Rows[6]["Age"]);
			Assert.AreEqual(0, (int)summaryDataSet.Tables["IncidentAging"].Rows[6]["Count"]);

			//Get the aging summary - for a whole project group
			summaryDataSet = incidentManager.RetrieveAging(2, 90, 15);
			Assert.AreEqual(7, summaryDataSet.Tables["IncidentAging"].Rows.Count);
			Assert.AreEqual("0-15", (string)summaryDataSet.Tables["IncidentAging"].Rows[0]["Age"]);
			Assert.AreEqual(3, (int)summaryDataSet.Tables["IncidentAging"].Rows[0]["Count"]);
			Assert.AreEqual("> 90", (string)summaryDataSet.Tables["IncidentAging"].Rows[6]["Age"]);
			Assert.AreEqual(9, (int)summaryDataSet.Tables["IncidentAging"].Rows[6]["Count"]);

			//Get the incident test coverage - all releases

			List<IncidentTestCoverage> incidentTestCoverage = incidentManager.RetrieveTestCoverage(PROJECT_ID, null);
			Assert.AreEqual(6, incidentTestCoverage.Count);

			Assert.AreEqual("Failed", incidentTestCoverage[0].ExecutionStatusName);
			Assert.AreEqual(0, incidentTestCoverage[0].TestCount);
			Assert.AreEqual("Caution", incidentTestCoverage[4].ExecutionStatusName);
			Assert.AreEqual(0, incidentTestCoverage[4].TestCount);

			//Get the incident test coverage - specific release
			incidentTestCoverage = incidentManager.RetrieveTestCoverage(PROJECT_ID, 1);
			Assert.AreEqual(6, incidentTestCoverage.Count);
			Assert.AreEqual("Failed", incidentTestCoverage[0].ExecutionStatusName);
			Assert.AreEqual(0, incidentTestCoverage[0].TestCount);
			Assert.AreEqual("Caution", incidentTestCoverage[4].ExecutionStatusName);
			Assert.AreEqual(0, incidentTestCoverage[4].TestCount);

			//Verify that we can get the summary open/closed count, used in the donut graphs on the incident list page
			List<IncidentOpenClosedCount> incidentOpenClosedCounts = incidentManager.RetrieveOpenClosedCount(PROJECT_ID, null, false);
			Assert.AreEqual(2, incidentOpenClosedCounts.Count);
			Assert.AreEqual(false, incidentOpenClosedCounts[0].IsOpenStatus);
			Assert.AreEqual(26, incidentOpenClosedCounts[0].IncidentCount);
			Assert.AreEqual(true, incidentOpenClosedCounts[1].IsOpenStatus);
			Assert.AreEqual(34, incidentOpenClosedCounts[1].IncidentCount);

			//Verify that we can get the summary open/closed count, for a specific detected release
			incidentOpenClosedCounts = incidentManager.RetrieveOpenClosedCount(PROJECT_ID, 4, false);
			Assert.AreEqual(1, incidentOpenClosedCounts.Count);
			Assert.AreEqual(true, incidentOpenClosedCounts[0].IsOpenStatus);
			Assert.AreEqual(6, incidentOpenClosedCounts[0].IncidentCount);

			//Verify that we can get the summary open/closed count, for a specific resolved release
			incidentOpenClosedCounts = incidentManager.RetrieveOpenClosedCount(PROJECT_ID, 4, true);
			Assert.AreEqual(1, incidentOpenClosedCounts.Count);
			Assert.AreEqual(true, incidentOpenClosedCounts[0].IsOpenStatus);
			Assert.AreEqual(2, incidentOpenClosedCounts[0].IncidentCount);
		}

		[
		Test,
		SpiraTestCase(179)
		]
		public void _11_RetrieveIncidentsByRelease()
		{
			Hashtable filters = new Hashtable();

			//Lets test that we can retrieve a list of incidents in the system for a specific release
			//Filter by Detected By Release = 1
			filters.Clear();
			filters.Add("DetectedReleaseId", 1);
			List<IncidentView> incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusId", true, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);

			//Filter by Resolved By Release = 2
			filters.Clear();
			filters.Add("ResolvedReleaseId", 2);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusId", true, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);

			//Filter by Verified By Release = 5
			filters.Clear();
			filters.Add("VerifiedReleaseId", 5);
			incidents = incidentManager.Retrieve(PROJECT_ID, "IncidentStatusId", true, 1, 99999, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);

			//Now test that we can use the special method that only retrieves incidents directly linked
			//to the specified release/iteration (i.e. ignores the fact that iterations are children of releases)
			//This method is used in the Incident scheduling/planning modules (uses ResolvedReleaseId)
			incidents = incidentManager.RetrieveByReleaseId(PROJECT_ID, 2);
			Assert.AreEqual(3, incidents.Count);

			//Now lets verify that we can retrieve the list of open incidents NOT associated with a release
			incidents = incidentManager.RetrieveByReleaseId(PROJECT_ID, null);
			Assert.AreEqual(25, incidents.Count);
			Assert.IsTrue(incidents[0].ResolvedReleaseId.IsNull());
		}

		[
		Test,
		SpiraTestCase(182)
		]
		public void _12_CustomizeIncidentFields()
		{
			//***** INCIDENT TYPES *****

			//First lets retrieve the list of incident types
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(PROJECT_ID, false);
			Assert.AreEqual(8, incidentTypes.Count);
			Assert.AreEqual("Issue", incidentTypes[4].Name);
			Assert.AreEqual(1, incidentTypes[4].WorkflowId);
			Assert.AreEqual(true, incidentTypes[4].IsIssue);
			Assert.AreEqual(false, incidentTypes[4].IsRisk);
			Assert.AreEqual(false, incidentTypes[4].IsDefault);
			Assert.AreEqual(true, incidentTypes[4].IsActive);

			//Now lets update one of the types
			incidentTypes[4].StartTracking();
			incidentTypes[4].Name = "Problem";
			incidentTypes[4].IsIssue = false;
			incidentTypes[4].IsRisk = true;
			incidentTypes[4].IsActive = false;
			incidentManager.IncidentType_Update(incidentTypes[4]);

			incidentTypes = incidentManager.RetrieveIncidentTypes(PROJECT_ID, false);
			Assert.AreEqual(8, incidentTypes.Count);
			Assert.AreEqual("Enhancement", incidentTypes[5].Name);
			Assert.AreEqual(1, incidentTypes[5].WorkflowId);
			Assert.AreEqual(false, incidentTypes[5].IsIssue);
			Assert.AreEqual(false, incidentTypes[5].IsRisk);
			Assert.AreEqual(false, incidentTypes[5].IsDefault);
			Assert.AreEqual(true, incidentTypes[5].IsActive);

			//Now lets put the data back the way it was
			incidentTypes[5].StartTracking();
			incidentTypes[5].Name = "Issue";
			incidentTypes[5].IsIssue = true;
			incidentTypes[5].IsRisk = false;
			incidentTypes[5].IsActive = true;
			incidentManager.IncidentType_Update(incidentTypes[5]);

			//Finally lets test that we can insert a new incident type
			int incidentTypeId = incidentManager.InsertIncidentType(PROJECT_ID, "Assistance Request", null, false, false, false, true);

			//Verify that it inserted successfully using the retrieve by id
			IncidentType incidentType = incidentManager.RetrieveIncidentTypeById(incidentTypeId);
			Assert.AreEqual("Assistance Request", incidentType.Name);
			Assert.AreEqual(1, incidentType.WorkflowId);    //The default workflow for the project
			Assert.AreEqual(incidentTypeId, incidentType.IncidentTypeId);

			//Finally clean up by deleting this item
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_INCIDENT_TYPE WHERE INCIDENT_TYPE_ID = " + incidentTypeId.ToString());

			//***** INCIDENT STATUSES *****

			//Next lets retrieve the list of incident statuses
			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(PROJECT_ID, false);
			Assert.AreEqual(8, incidentStati.Count);
			Assert.AreEqual("Reopen", incidentStati[0].Name);
			Assert.AreEqual(true, incidentStati[0].IsOpenStatus);
			Assert.AreEqual(false, incidentStati[0].IsDefault);
			Assert.AreEqual(true, incidentStati[0].IsActive);

			//Now lets update one of the statuses
			incidentStati[0].StartTracking();
			incidentStati[0].Name = "In Progress";
			incidentStati[0].IsOpenStatus = false;
			incidentStati[0].IsActive = false;
			incidentManager.IncidentStatus_Update(incidentStati[0]);

			incidentStati = incidentManager.IncidentStatus_Retrieve(PROJECT_ID, false);
			Assert.AreEqual(8, incidentStati.Count);
			Assert.AreEqual("Not Reproducible", incidentStati[2].Name);
			Assert.AreEqual(false, incidentStati[2].IsOpenStatus);
			Assert.AreEqual(false, incidentStati[2].IsDefault);
			Assert.AreEqual(true, incidentStati[2].IsActive);

			//Now lets put the data back the way it was
			incidentStati[2].StartTracking();
			incidentStati[2].Name = "Assigned";
			incidentStati[2].IsOpenStatus = true;
			incidentStati[2].IsActive = true;
			incidentManager.IncidentStatus_Update(incidentStati[2]);

			//Finally lets test that we can insert a new incident status
			int incidentStatusId = incidentManager.IncidentStatus_Insert(PROJECT_ID, "Under Consideration", true, false, true);

			//Verify that it inserted successfully using the retrieve by id
			IncidentStatus incidentStatus = incidentManager.IncidentStatus_RetrieveById(incidentStatusId);
			Assert.AreEqual("Under Consideration", incidentStatus.Name);
			Assert.AreEqual(incidentStatusId, incidentStatus.IncidentStatusId);

			//Finally clean up by deleting this item
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_INCIDENT_STATUS WHERE INCIDENT_STATUS_ID = " + incidentStatusId.ToString());

			//***** INCIDENT PRIORITIES *****

			//First lets retrieve the list of incident priorities
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(PROJECT_ID, false);
			Assert.AreEqual(4, incidentPriorities.Count);
			Assert.AreEqual("3 - Medium", incidentPriorities[1].Name);
			Assert.AreEqual("f5d857", incidentPriorities[1].Color);
			Assert.AreEqual(true, incidentPriorities[1].IsActive);

			//Now lets update one of the priorities
			incidentPriorities[1].StartTracking();
			incidentPriorities[1].Name = "2 - Important";
			incidentPriorities[1].Color = "ff5510";
			incidentPriorities[1].IsActive = false;
			incidentManager.IncidentPriority_Update(incidentPriorities[1]);

			incidentPriorities = incidentManager.RetrieveIncidentPriorities(PROJECT_ID, false);
			Assert.AreEqual(4, incidentPriorities.Count);
			Assert.AreEqual("2 - Important", incidentPriorities[1].Name);
			Assert.AreEqual("ff5510", incidentPriorities[1].Color);
			Assert.AreEqual(false, incidentPriorities[1].IsActive);

			//Now lets put the data back the way it was
			incidentPriorities[1].StartTracking();
			incidentPriorities[1].Name = "2 - High";
			incidentPriorities[1].Color = "f29e56";
			incidentPriorities[1].IsActive = true;
			incidentManager.IncidentPriority_Update(incidentPriorities[1]);

			//Finally lets test that we can insert a new incident priority
			int priorityId = incidentManager.InsertIncidentPriority(PROJECT_ID, "5 - Cosmetic", "ffff55", true);

			//Verify that it inserted successfully
			incidentPriorities = incidentManager.RetrieveIncidentPriorities(PROJECT_ID, false);
			Assert.AreEqual(5, incidentPriorities.Count);
			Assert.AreEqual("1 - Critical", incidentPriorities[4].Name);
			//Assert.AreEqual(priorityId, incidentPriorities[4].PriorityId);

			//Finally clean up by deleting this item
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_INCIDENT_PRIORITY WHERE PRIORITY_ID = " + priorityId.ToString());

			//***** INCIDENT SEVERITIES *****

			//First lets retrieve the list of incident severities
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(PROJECT_ID, false);
			Assert.AreEqual(4, incidentSeverities.Count);
			Assert.AreEqual("3 - Medium", incidentSeverities[1].Name);
			Assert.AreEqual("f5d857", incidentSeverities[1].Color);
			Assert.AreEqual(true, incidentSeverities[1].IsActive);

			//Now lets update one of the severities
			incidentSeverities[1].StartTracking();
			incidentSeverities[1].Name = "2 - Important";
			incidentSeverities[1].Color = "ff5510";
			incidentSeverities[1].IsActive = false;
			incidentManager.IncidentSeverity_Update(incidentSeverities[1]);

			incidentSeverities = incidentManager.RetrieveIncidentSeverities(PROJECT_ID, false);
			Assert.AreEqual(4, incidentSeverities.Count);
			Assert.AreEqual("2 - Important", incidentSeverities[1].Name);
			Assert.AreEqual("ff5510", incidentSeverities[1].Color);
			Assert.AreEqual(false, incidentSeverities[1].IsActive);

			//Now lets put the data back the way it was
			incidentSeverities[1].StartTracking();
			incidentSeverities[1].Name = "2 - High";
			incidentSeverities[1].Color = "f29e56";
			incidentSeverities[1].IsActive = true;
			incidentManager.IncidentSeverity_Update(incidentSeverities[1]);

			//Finally lets test that we can insert a new incident severity
			int severityId = incidentManager.InsertIncidentSeverity(PROJECT_ID, "5 - Cosmetic", "ffff55", true);

			//Verify that it inserted successfully
			incidentSeverities = incidentManager.RetrieveIncidentSeverities(PROJECT_ID, false);
			Assert.AreEqual(5, incidentSeverities.Count);
			Assert.AreEqual("1 - Critical", incidentSeverities[4].Name);
			//Assert.AreEqual(severityId, incidentSeverities[4].SeverityId);

			//Finally clean up by deleting this item
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_INCIDENT_SEVERITY WHERE SEVERITY_ID = " + severityId.ToString());
		}

		/// <summary>
		/// Tests that we can copy incidents within the same project
		/// </summary>
		[
		Test,
		SpiraTestCase(394)
		]
		public void _13_CopyIncidents()
		{
			//Lets make a copy of an existing incident, then verify that the details copied across correctly.
			incidentId1 = incidentManager.Copy(USER_ID_FRED_BLOGGS, 11);

			//Verify the data
			Incident incident = incidentManager.RetrieveById(incidentId1, true);

			//Incident Details
			Assert.AreEqual(PROJECT_ID, incident.ProjectId);
			Assert.AreEqual("Validation on the edit book page - Copy", incident.Name);
			Assert.AreEqual("The edit book page keeps throwing validation errors even where form filled out correctly", incident.Description);
			Assert.AreEqual(1, incident.PriorityId);
			Assert.AreEqual(3, incident.SeverityId);
			Assert.AreEqual(4, incident.IncidentStatusId);
			Assert.AreEqual(2, incident.IncidentTypeId);
			Assert.AreEqual(50, incident.CompletionPercent);

			//Resolutions
			Assert.AreEqual(2, incident.Resolutions.Count);
			Assert.AreEqual("There was an erroneous exception throw in the module - removed", incident.Resolutions[0].Resolution);
			Assert.AreEqual("Exception code retested and error no longer occurs", incident.Resolutions[1].Resolution);

			//Also verify that an association was created back to the original incident
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Incident, incidentId1);
			Assert.AreEqual(1, artifactLinks.Count);
			ArtifactLinkView artifactLinkRow = artifactLinks[0];
			Assert.AreEqual(11, artifactLinkRow.ArtifactId);
			Assert.AreEqual("Copied Incident", artifactLinkRow.Comment);

			//Now clean up by deleting the incident
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId1, USER_ID_FRED_BLOGGS);

			//Now we need to test that copying an incident with an attachment and custom properties copies those as well
			incidentId1 = incidentManager.Copy(USER_ID_FRED_BLOGGS, 7);

			//Verify that the custom properties and attachments list copied across successfully
			//Custom Properties
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			Assert.AreEqual("May be an array bounds issue", incidentView.Custom_01);
			Assert.AreEqual(13, incidentView.Custom_02.FromDatabaseSerialization_Int32());

			//Attachments
			AttachmentManager attachmentManager = new AttachmentManager();
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, incidentId1, Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);
			Assert.AreEqual("Error Stacktrace.doc", attachments[0].Filename);
			Assert.AreEqual("Web Page capture.htm", attachments[1].Filename);

			//Now clean up by deleting the incident
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId1, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that we can export incidents between two projects
		/// </summary>
		[
		Test,
		SpiraTestCase(395)
		]
		public void _14_ExportIncidents()
		{
			//First lets create a new incident in our current project
			int incidentId = incidentManager.Insert(projectId,
				incidentPriorities.FirstOrDefault(p => p.Name == "2 - High").PriorityId,
				incidentSeverities.FirstOrDefault(p => p.Name == "3 - Medium").SeverityId,
				USER_ID_FRED_BLOGGS, null, null, "Validation on the edit book page", "The edit book page keeps throwing validation errors even where form filled out correctly", null, null, null,
				incidentTypes.FirstOrDefault(p => p.Name == "Bug").IncidentTypeId,
				incidentStati.FirstOrDefault(p => p.Name == "Open").IncidentStatusId,
				DateTime.UtcNow, null, null, 10, 6, 5, null, null, USER_ID_FRED_BLOGGS);

			//Add some resolutions
			incidentManager.InsertResolution(incidentId, "There was an erroneous exception throw in the module - removed", DateTime.UtcNow, USER_ID_FRED_BLOGGS, false);
			incidentManager.InsertResolution(incidentId, "Exception code retested and error no longer occurs", DateTime.UtcNow, USER_ID_FRED_BLOGGS, false);

			//Add an attachment
			AttachmentManager attachmentManager = new AttachmentManager();
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			attachmentManager.Insert(projectId, "Error Stacktrace.doc", null, USER_ID_FRED_BLOGGS, attachmentData, incidentId, Artifact.ArtifactTypeEnum.Incident, "1.0", null, null, null, null);
			attachmentManager.Insert(projectId, "Web Page capture.htm", null, USER_ID_FRED_BLOGGS, attachmentData, incidentId, Artifact.ArtifactTypeEnum.Incident, "1.0", null, null, null, null);

			//Now lets create two new projects, one using the same template, one not
			ProjectManager projectManager = new ProjectManager();
			TemplateManager templateManager = new TemplateManager();
			int newProjectId1 = projectManager.CreateFromExisting("Export Project 1", null, null, projectId, true, false);
			int newProjectId2 = projectManager.CreateFromExisting("Export Project 2", null, null, projectId, true, true);
			int newTemplateId2 = templateManager.RetrieveForProject(newProjectId2).ProjectTemplateId;

			//Lets make a copy of an existing incident into the new projects, then verify that the details exported correctly.
			incidentId1 = incidentManager.Export(incidentId, newProjectId1, USER_ID_FRED_BLOGGS);
			incidentId2 = incidentManager.Export(incidentId, newProjectId2, USER_ID_FRED_BLOGGS);

			#region Export 1

			//Verify the data
			Incident incident = incidentManager.RetrieveById(incidentId1, true);

			//Incident Details
			Assert.AreEqual(newProjectId1, incident.ProjectId);
			Assert.AreEqual("Validation on the edit book page", incident.Name);
			Assert.AreEqual("The edit book page keeps throwing validation errors even where form filled out correctly", incident.Description);
			Assert.AreEqual(50, incident.CompletionPercent);

			//The values will be the same as the project uses the same template
			IncidentView incidentView = incidentManager.RetrieveById2(incidentId1);
			Assert.AreEqual("Open", incidentView.IncidentStatusName);
			Assert.AreEqual("Bug", incidentView.IncidentTypeName);
			Assert.AreEqual("2 - High", incidentView.PriorityName);
			Assert.AreEqual("3 - Medium", incidentView.SeverityName);

			//Resolutions
			Assert.AreEqual(2, incident.Resolutions.Count);
			Assert.AreEqual("There was an erroneous exception throw in the module - removed", incident.Resolutions[0].Resolution);
			Assert.AreEqual("Exception code retested and error no longer occurs", incident.Resolutions[1].Resolution);

			//Verify that the attachments list copied across successfully
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(newProjectId1, incidentId1, Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);
			Assert.AreEqual("Error Stacktrace.doc", attachments[0].Filename);
			Assert.AreEqual("Web Page capture.htm", attachments[1].Filename);

			#endregion

			#region Export 2

			//Verify the data
			incident = incidentManager.RetrieveById(incidentId2, true);

			//Incident Details
			Assert.AreEqual(newProjectId2, incident.ProjectId);
			Assert.AreEqual("Validation on the edit book page", incident.Name);
			Assert.AreEqual("The edit book page keeps throwing validation errors even where form filled out correctly", incident.Description);
			Assert.AreEqual(50, incident.CompletionPercent);

			//The values will be reset as the project uses different templates
			incidentView = incidentManager.RetrieveById2(incidentId2);
			Assert.AreEqual("New", incidentView.IncidentStatusName);
			Assert.AreEqual("Incident", incidentView.IncidentTypeName);
			Assert.IsFalse(incidentView.PriorityId.HasValue);
			Assert.IsFalse(incidentView.SeverityId.HasValue);

			//Resolutions
			Assert.AreEqual(2, incident.Resolutions.Count);
			Assert.AreEqual("There was an erroneous exception throw in the module - removed", incident.Resolutions[0].Resolution);
			Assert.AreEqual("Exception code retested and error no longer occurs", incident.Resolutions[1].Resolution);

			//Verify that the attachments list copied across successfully
			attachments = attachmentManager.RetrieveByArtifactId(newProjectId2, incidentId2, Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(2, attachments.Count);
			Assert.AreEqual("Error Stacktrace.doc", attachments[0].Filename);
			Assert.AreEqual("Web Page capture.htm", attachments[1].Filename);

			#endregion

			//Now clean up by deleting the incidents, projects and templates
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);
			projectManager.Delete(USER_ID_FRED_BLOGGS, newProjectId1);
			projectManager.Delete(USER_ID_FRED_BLOGGS, newProjectId2);
			templateManager.Delete(USER_ID_FRED_BLOGGS, newTemplateId2);
		}

		/// <summary>
		/// Tests the ability to retrieve incidents by a test step and/or a test run step
		/// </summary>
		[
		Test,
		SpiraTestCase(405)
		]
		public void _15_RetrieveIncidentsByTestStep()
		{
			//First lets retrieve all the incidents that are attached directly to a test run step only
			List<IncidentView> incidents = incidentManager.RetrieveByTestRunStepId(13);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(2, incidents[0].IncidentId);
			Assert.AreEqual("Not able to add new author", incidents[0].Name);
			Assert.AreEqual("Incident", incidents[0].IncidentTypeName);
			Assert.AreEqual("New", incidents[0].IncidentStatusName);
			Assert.IsTrue(incidents[0].PriorityId.IsNull());
			Assert.IsTrue(incidents[0].OwnerId.IsNull());
			Assert.AreEqual(USER_ID_JOE_SMITH, incidents[0].OpenerId);

			//Now lets retrieve all the incidents that are attached to a test step directly and indirectly through a test run
			//This is used during test execution where we need to display both cases
			incidents = incidentManager.RetrieveByTestStepId(2);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(7, incidents[0].IncidentId);
			Assert.AreEqual("Cannot add a new book to the system", incidents[0].Name);
		}

		/// <summary>
		/// Tests that we can change the release/iteration association of an incident
		/// </summary>
		[
		Test,
		SpiraTestCase(440)
		]
		public void _16_IterationScheduling()
		{
			//This tests that we can associate incidents with an iteration and have the start date populate

			//First create a new iteration inside the empty project
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0000", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("4/1/2004"), DateTime.Parse("4/30/2004"), 2, 0, null, false);

			//Now lets create a new unassociated incident
			int incidentId1 = incidentManager.Insert(PROJECT_EMPTY_ID, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Really bad incident", releaseId, null, null, null, null, DateTime.UtcNow, null, null, 2400, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Verify that the incident is unassociated
			Incident incident = incidentManager.RetrieveById(incidentId1, false);
			Assert.IsTrue(incident.ResolvedReleaseId.IsNull());
			Assert.IsTrue(incident.StartDate.IsNull());

			//Now associate the incident with the iteration
			incidentManager.AssociateToIteration(new List<int> { incidentId1 }, releaseId, USER_ID_FRED_BLOGGS);

			//Verify that it is associated and that the start date was set
			incident = incidentManager.RetrieveById(incidentId1, false);
			Assert.AreEqual(releaseId, incident.ResolvedReleaseId);
			Assert.AreEqual(DateTime.Parse("4/1/2004"), incident.StartDate);

			//Finally verify that we can deassociate the incident
			incidentManager.RemoveReleaseAssociation(new List<int> { incidentId1 }, USER_ID_FRED_BLOGGS);

			//Verify that the incident is unassociated
			incident = incidentManager.RetrieveById(incidentId1, false);
			Assert.IsTrue(incident.ResolvedReleaseId.IsNull());

			//Clean up
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId1, USER_ID_FRED_BLOGGS);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, releaseId);
		}

		/// <summary>
		/// Verify that multiple, concurrent updates to incidents are handled correctly
		/// in accordance with optimistic concurrency rules
		/// </summary>
		[
		Test,
		SpiraTestCase(580)
		]
		public void _17_Concurrency_Handling()
		{
			//First we need to create a new incident to verify the handling
			int incidentId = incidentManager.Insert(
				PROJECT_EMPTY_ID,
				null,
				null,
				2,
				null,
				null,
				"Test Incident 1",
				"This is a test incident",
				null,
				null,
				null,
				1,
				null,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Now retrieve the incident back and keep a copy of the entity
			Incident incident1 = incidentManager.RetrieveById(incidentId, false);
			Incident incident2 = incident1.Clone();

			//Now make a change to field and update
			incident1.StartTracking();
			incident1.Name = "Test Incident 1 Modified";
			incidentManager.Update(incident1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			Incident incident3 = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual("Test Incident 1 Modified", incident3.Name);

			//Now try making a change using the out of date entity (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				incident2.StartTracking();
				incident2.Name = "Test Incident 1 Refactored";
				incidentManager.Update(incident2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			incident3 = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual("Test Incident 1 Modified", incident3.Name);

			//Now refresh the old dataset and try again and verify it works
			incident2 = incidentManager.RetrieveById(incidentId, false);
			incident2.StartTracking();
			incident2.Name = "Test Incident 1 Refactored";
			incidentManager.Update(incident2, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate dataset
			incident3 = incidentManager.RetrieveById(incidentId, false);
			Assert.AreEqual("Test Incident 1 Refactored", incident3.Name);

			//Clean up
			incidentManager.MarkAsDeleted(PROJECT_ID, incidentId, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests that you can associate an incident with the build it was fixed in
		/// </summary>
		[
		Test,
		SpiraTestCase(813)
		]
		public void _18_IncidentsLinkedToBuilds()
		{
			//First we need to create a new iteration to associate any builds with
			ReleaseManager releaseManager = new ReleaseManager();
			int iterationId = releaseManager.Insert(USER_ID_FRED_BLOGGS, PROJECT_EMPTY_ID, USER_ID_FRED_BLOGGS, "Sprint 1", "", "1.0.0.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1, 0, null);

			//Now create two builds for this iteration
			BuildManager buildManager = new BuildManager();
			int buildId1 = buildManager.Insert(PROJECT_EMPTY_ID, iterationId, "Build 0001", "Nice Build", DateTime.UtcNow, Build.BuildStatusEnum.Succeeded, USER_ID_FRED_BLOGGS).BuildId;
			int buildId2 = buildManager.Insert(PROJECT_EMPTY_ID, iterationId, "Build 0002", "Naughty Build", DateTime.UtcNow, Build.BuildStatusEnum.Failed, USER_ID_FRED_BLOGGS).BuildId;

			//Now create a new incident linked to build 1
			int incidentId = incidentManager.Insert(
				PROJECT_EMPTY_ID,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				null,
				null,
				"Test Incident 1",
				"This is a test incident",
				null,
				iterationId,
				null,
				1,
				null,
				DateTime.UtcNow,
				null,
				null,
				null,
				null,
				null,
				buildId1,
				null
				);

			//Verify that you can retrieve the incidents associated with a build
			Hashtable filters = new Hashtable();
			filters.Add("BuildId", buildId1);
			List<IncidentView> incidents = incidentManager.Retrieve(PROJECT_EMPTY_ID, "IncidentId", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual(incidentId, incidents[0].IncidentId);
			Assert.AreEqual(buildId1, incidents[0].BuildId);
			Assert.AreEqual("Build 0001", incidents[0].BuildName);

			//Now update the incident to point to the second build
			Incident incident = incidentManager.RetrieveById(incidents[0].IncidentId, false);
			incident.StartTracking();
			incident.BuildId = buildId2;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Verify the change
			filters.Clear();
			filters.Add("BuildId", buildId2);
			incidents = incidentManager.Retrieve(PROJECT_EMPTY_ID, "IncidentId", true, 1, 15, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(incidentId, incidents[0].IncidentId);
			Assert.AreEqual(buildId2, incidents[0].BuildId);
			Assert.AreEqual("Build 0002", incidents[0].BuildName);

			//Test that phsically deleting the release, cleans up the build-incident relationships
			releaseManager.DeleteFromDatabase(iterationId, USER_ID_FRED_BLOGGS);

			//Clean up
			incidentManager.DeleteFromDatabase(incidentId, USER_ID_FRED_BLOGGS);
		}

		/// Tests that we can associate incidents with components
		/// and then retrieve them by component
		/// </summary>
		[Test]
		[SpiraTestCase(1304)]
		public void _19_Associate_Incidents_With_Components()
		{
			//First we need to add some components
			//Lets create three new components in the project, with two being active
			ComponentManager componentManager = new ComponentManager();
			int componentId1 = componentManager.Component_Insert(projectId, "Component 1");
			int componentId2 = componentManager.Component_Insert(projectId, "Component 2");
			int componentId3 = componentManager.Component_Insert(projectId, "Component 3", false);

			//Next, need to create three incidents, which are assigned to one or more components
			int incidentId1 = incidentManager.Insert(
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
				 new List<int>() { componentId1 },
				 USER_ID_FRED_BLOGGS
				 );

			int incidentId2 = incidentManager.Insert(
				 projectId,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS,
				 null,
				 null,
				 "Test Incident 2",
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
				 new List<int>() { componentId1, componentId2 },
				 USER_ID_FRED_BLOGGS
				 );

			int incidentId3 = incidentManager.Insert(
				 projectId,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS,
				 null,
				 null,
				 "Test Incident 3",
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
				 new List<int>() { componentId2, componentId3 },
				 USER_ID_FRED_BLOGGS
				 );

			//First get the list of incidents unfiltered, to verify
			List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);
			Assert.AreEqual("Test Incident 1", incidents[0].Name);
			Assert.AreEqual("Test Incident 2", incidents[1].Name);
			Assert.AreEqual("Test Incident 3", incidents[2].Name);

			//Now lets retrieve all the requirements associated with a specific component
			Hashtable filters = new Hashtable();
			MultiValueFilter mvf = new MultiValueFilter();
			mvf.Values.Add(componentId1);
			filters.Add("ComponentIds", mvf);
			incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, incidents.Count);
			Assert.AreEqual("Test Incident 1", incidents[0].Name);
			Assert.AreEqual("Test Incident 2", incidents[1].Name);

			//Now lets retrieve all the requirements associated with a selection of components
			filters.Clear();
			mvf = new MultiValueFilter();
			mvf.Values.Add(componentId1);
			mvf.Values.Add(componentId3);
			filters.Add("ComponentIds", mvf);
			incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);
			Assert.AreEqual("Test Incident 1", incidents[0].Name);
			Assert.AreEqual("Test Incident 2", incidents[1].Name);
			Assert.AreEqual("Test Incident 3", incidents[2].Name);

			//Next lets remove all the components from one incident
			Incident incident = incidentManager.RetrieveById(incidentId1, false);
			incident.StartTracking();
			incident.ComponentIds = null;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now lets verify that we can retrieve all the incidents with no component set
			filters.Clear();
			mvf = new MultiValueFilter();
			mvf.IsNone = true;
			filters.Add("ComponentIds", mvf);
			incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, incidents.Count);
			Assert.AreEqual("Test Incident 1", incidents[0].Name);

			//Now put a different value on that incident
			incident = incidentManager.RetrieveById(incidentId1, false);
			incident.StartTracking();
			incident.ComponentIds = (new List<int>() { componentId2 }).ToDatabaseSerialization();
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Verify the change
			filters.Clear();
			filters.Add("ComponentIds", componentId2);
			incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, incidents.Count);
			Assert.AreEqual("Test Incident 1", incidents[0].Name);
			Assert.AreEqual("Test Incident 2", incidents[1].Name);
			Assert.AreEqual("Test Incident 3", incidents[2].Name);

			//Finally verify that history entries were correctly recorded
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(incidentId1, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(3, historyChangeSets.Count);

			//The entries are newest first
			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Added, historyChangeSets[0].ChangeTypeId);
			//HistoryDetail historyDetail = historyChangeSets[0].Details[0];
			//Assert.IsTrue(historyDetail.OldValue.IsNull());
			//Assert.AreEqual(componentId2.ToDatabaseSerialization(), historyDetail.NewValue);

			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[1].ChangeTypeId);
			//historyDetail = historyChangeSets[1].Details[0];
			//Assert.IsTrue(historyDetail.NewValue.IsNull());
			//Assert.AreEqual(componentId1.ToDatabaseSerialization(), historyDetail.OldValue);

			Assert.AreEqual((int)HistoryManager.ChangeSetTypeEnum.Modified, historyChangeSets[2].ChangeTypeId);

			//Finally delete the incidents (components will be deleted during tear-down)
			incidentManager.MarkAsDeleted(projectId, incidentId1, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId2, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId3, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests the functions used by the planning board
		/// </summary>
		[
		Test,
		SpiraTestCase(1313)
		]
		public void _20_PlanningBoardTests()
		{
			//First lets create some components
			ComponentManager componentManager = new ComponentManager();
			int componentId1 = componentManager.Component_Insert(projectId, "Component 1");
			int componentId2 = componentManager.Component_Insert(projectId, "Component 2");

			//Next lets create a summary requirement
			RequirementManager requirementManager = new RequirementManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 1", null, null, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, null, "Requirement 2", null, 1.0M, USER_ID_FRED_BLOGGS);
			requirementManager.Indent(USER_ID_FRED_BLOGGS, projectId, requirementId2);

			//Next lets create some incidents
			int incidentId1 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 1", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId2 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 2", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId3 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 3", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId4 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 4", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId5 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 5", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId6 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 6", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId7 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 7", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId8 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 8", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId9 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 9", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId10 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 10", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			/* Backlog View */

			//Get product backlog by component
			List<IncidentView> incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, null);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId2);
			Assert.AreEqual(0, incidents.Count);

			//Get the list of packages
			List<RequirementView> requirements = requirementManager.Requirement_RetrieveSummaryBacklog(projectId);
			Assert.AreEqual(1, requirements.Count);
			Assert.AreEqual("Requirement 1", requirements[0].Name);

			//Get product backlog by package
			incidents = incidentManager.Incident_RetrieveBacklogByRequirementId(projectId, null);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByRequirementId(projectId, requirementId1);
			Assert.AreEqual(0, incidents.Count);

			//Now lets update the backlog
			//Incident 2
			Incident incident = incidentManager.RetrieveById(incidentId2, false);
			incident.StartTracking();
			incident.ComponentIds = componentId1.ToDatabaseSerialization();
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			//Incident 3
			incident = incidentManager.RetrieveById(incidentId3, false);
			incident.StartTracking();
			incident.ComponentIds = componentId2.ToDatabaseSerialization();
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			//Incident 4
			incident = incidentManager.RetrieveById(incidentId4, false);
			incident.StartTracking();
			incident.ComponentIds = new List<int>() { componentId1, componentId2 }.ToDatabaseSerialization();
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Associate this last incident with the requirement
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			artifactLinkManager.Insert(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId4, Artifact.ArtifactTypeEnum.Requirement, requirementId1, USER_ID_FRED_BLOGGS, "The bug belongs to this package", DateTime.UtcNow);

			//Get product backlog by component
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, null);
			Assert.AreEqual(7, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(2, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId2);
			Assert.AreEqual(2, incidents.Count);

			//Get product backlog by package
			incidents = incidentManager.Incident_RetrieveBacklogByRequirementId(projectId, null);
			Assert.AreEqual(9, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByRequirementId(projectId, requirementId1);
			Assert.AreEqual(1, incidents.Count);

			//Check the ordering of the incidents by component
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(incidentId2, incidents[0].IncidentId);
			Assert.AreEqual(incidentId4, incidents[1].IncidentId);

			//Re-rank them (incidents grooming)
			incidentManager.Incident_UpdateRanks(projectId, new List<int>() { incidentId4 }, 1);

			//Verify
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(incidentId4, incidents[0].IncidentId);
			Assert.AreEqual(incidentId2, incidents[1].IncidentId);

			//Re-rank them again
			int existingRank = incidentManager.RetrieveById(incidentId4, false).Rank.Value;
			incidentManager.Incident_UpdateRanks(projectId, new List<int>() { incidentId2 }, existingRank);

			//Verify
			incidents = incidentManager.Incident_RetrieveBacklogByComponentId(projectId, componentId1);
			Assert.AreEqual(incidentId2, incidents[0].IncidentId);
			Assert.AreEqual(incidentId4, incidents[1].IncidentId);

			/* All Release View */

			//Lets create two releases, with the first one having two iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(2), 2, 0, null, false);
			int iterationId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 001", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 002", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now.AddMonths(1), DateTime.Now.AddMonths(2), 2, 0, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now.AddMonths(2), DateTime.Now.AddMonths(4), 2, 0, null, false);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now lets verify that no releases have incidents
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(0, incidents.Count);

			//Assign some incidents to one of the releases
			incidentManager.AssociateToIteration(new List<int>() { incidentId2, incidentId4 }, releaseId1, USER_ID_FRED_BLOGGS);
			incidentManager.AssociateToIteration(new List<int>() { incidentId5 }, releaseId2, USER_ID_FRED_BLOGGS);

			//Verify the changes
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(0, incidents.Count);

			//Verify that we can retrieve for all releases by Person
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, null);
			Assert.AreEqual(3, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, USER_ID_JOE_SMITH);
			Assert.AreEqual(0, incidents.Count);

			//Get all releases view by component
			incidents = incidentManager.Incident_RetrieveAllReleasesByComponentId(projectId, null);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByComponentId(projectId, componentId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByComponentId(projectId, componentId2);
			Assert.AreEqual(0, incidents.Count);

			//Get all releases view by package
			incidents = incidentManager.Incident_RetrieveAllReleasesByRequirementId(projectId, null);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByRequirementId(projectId, requirementId1);
			Assert.AreEqual(0, incidents.Count);

			/* Specific Release View */

			//Get release backlog by iteration
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId1, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId2, false);
			Assert.AreEqual(0, incidents.Count);

			//Get release backlog by person
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, incidents.Count); //The product backlog
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, incidents.Count);

			//Now we need to assign the incidents to a particular iteration and user.
			incidentManager.AssociateToIteration(new List<int>() { incidentId2 }, iterationId1, USER_ID_FRED_BLOGGS);
			incidentManager.AssociateToIteration(new List<int>() { incidentId4 }, iterationId2, USER_ID_FRED_BLOGGS);
			incidentManager.AssignToUser(incidentId2, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);

			//Get release backlog by iteration
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, false);
			Assert.AreEqual(0, incidents.Count); //Release only
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, incidents.Count); //Release and child iterations
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId1, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId2, false);
			Assert.AreEqual(0, incidents.Count);

			//Get release backlog by person
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, incidents.Count); //The product backlog
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, incidents.Count);

			//Get specific release view by component
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, null, releaseId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, componentId1, releaseId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, componentId2, releaseId1);
			Assert.AreEqual(0, incidents.Count);

			//Get specific release view by package
			incidents = incidentManager.Incident_RetrieveForReleaseByRequirementId(projectId, null, releaseId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByRequirementId(projectId, requirementId1, releaseId1);
			Assert.AreEqual(0, incidents.Count);

			/* Specific Iteration View */

			//Get iteration backlog by person
			//Iteration #1
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, incidents.Count); //The product backlog
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, null, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, incidents.Count);
			//Iteration #2
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, incidents.Count); //The product backlog
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, incidents.Count);

			//Assign the other incident to Fred
			incidentManager.AssignToUser(incidentId4, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);

			//Get iteration backlog by person
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, false);
			Assert.AreEqual(0, incidents.Count); //The product backlog
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, null, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId2, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, incidents.Count);

			//Get specific iteration view by component
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, null, iterationId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, componentId1, iterationId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByComponentId(projectId, componentId2, iterationId1);
			Assert.AreEqual(0, incidents.Count);

			//Get specific iteration view by package
			incidents = incidentManager.Incident_RetrieveForReleaseByRequirementId(projectId, null, iterationId1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveForReleaseByRequirementId(projectId, requirementId1, iterationId1);
			Assert.AreEqual(0, incidents.Count);

			//Clean up by deleting the entire branch, test cases, tasks and components
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId1);
			incidentManager.MarkAsDeleted(projectId, incidentId1, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId2, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId3, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId4, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId5, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId6, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId7, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId8, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId9, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId10, USER_ID_FRED_BLOGGS);
			componentManager.Component_Delete(componentId1);
			componentManager.Component_Delete(componentId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, iterationId2);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId1);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId2);
		}

		/// <summary>
		/// Tests that we can link existing incidents to test run step
		/// </summary>
		[
		Test,
		SpiraTestCase(1327)
		]
		public void _21_Associate_Incidents_With_TestRunSteps()
		{
			//First we need to add a new test case with a test step
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseId1 = testCaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, "Sample Test Case 1", "Sample Test Case 1 Description", null, TestCase.TestCaseStatusEnum.ReadyForTest, null, null, null, null, null);
			int testStepId1 = testCaseManager.InsertStep(USER_ID_FRED_BLOGGS, testCaseId1, 1, "Sample Step 1.1", "It Works", "Sample Data");

			//Now execute this to create a test run with steps (and no linked incidents)
			TestRunManager testRunManager = new TestRunManager();
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, new List<int>() { testCaseId1 }, true);
			int testRunsPendingId = testRunsPending.TestRunsPendingId;

			//Retrieve it back to get the new IDs
			testRunsPending = testRunManager.RetrievePendingById(testRunsPendingId, true);
			int testRunId = testRunsPending.TestRuns[0].TestRunId;
			int testRunStepId = testRunsPending.TestRuns[0].TestRunSteps[0].TestRunStepId;

			//Next, need to create three incidents
			int incidentId1 = incidentManager.Insert(
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
				 USER_ID_FRED_BLOGGS
				 );

			int incidentId2 = incidentManager.Insert(
				 projectId,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS,
				 null,
				 null,
				 "Test Incident 2",
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
				 USER_ID_FRED_BLOGGS
				 );

			int incidentId3 = incidentManager.Insert(
				 projectId,
				 null,
				 null,
				 USER_ID_FRED_BLOGGS,
				 null,
				 null,
				 "Test Incident 3",
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
				 USER_ID_FRED_BLOGGS
				 );

			//Now first verify that no incidents are associated with this test run step
			List<IncidentView> incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
			Assert.AreEqual(0, incidents.Count);

			//Now lets associate two of the incidents
			incidentManager.Incident_AssociateToTestRunStep(projectId, testRunStepId, new List<int>() { incidentId1, incidentId2 }, USER_ID_FRED_BLOGGS);

			//Verify they were associated
			incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
			Assert.AreEqual(2, incidents.Count);
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId1));
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId2));

			//Now lets associate the remaining incident and see if we can add the same one twice without an error occurring
			incidentManager.Incident_AssociateToTestRunStep(projectId, testRunStepId, new List<int>() { incidentId2, incidentId3 }, USER_ID_FRED_BLOGGS);

			//Verify they were associated
			incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
			Assert.AreEqual(3, incidents.Count);
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId1));
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId2));
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId3));

			//Verify we can remove the association
			incidentManager.Incident_AssociateToTestRunStepRemove(projectId, testRunStepId, new List<int>() { incidentId2, incidentId3 }, USER_ID_FRED_BLOGGS);

			//Verify they are no longer associated
			incidents = incidentManager.RetrieveByTestRunStepId(testRunStepId);
			Assert.AreEqual(1, incidents.Count);
			Assert.IsTrue(incidents.Any(i => i.IncidentId == incidentId1));
			Assert.IsFalse(incidents.Any(i => i.IncidentId == incidentId2));
			Assert.IsFalse(incidents.Any(i => i.IncidentId == incidentId3));

			//Double check one of the incidents removed still exists
			Incident incident = incidentManager.RetrieveById(incidentId2, false);
			Assert.IsTrue(incident.IncidentId == incidentId2);

			//Finally clean up by deleting the test run, incidents, etc.
			testRunManager.CompletePending(testRunsPendingId, USER_ID_FRED_BLOGGS);
			testCaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, testCaseId1);
			incidentManager.MarkAsDeleted(projectId, incidentId1, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId2, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId3, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests the code that populates/updates the items in the Incident board view
		/// </summary>
		[Test, SpiraTestCase(1643)]
		public void _22_IncidentBoardTests()
		{
			int defaultIncidentStatusId = incidentManager.IncidentStatus_RetrieveDefault(projectTemplateId).IncidentStatusId;
			int defaultIncidentTypeId = incidentManager.GetDefaultIncidentType(projectTemplateId);

			//First test that we can create a new blank incident record (used in the popup task creation screen)
			IncidentView incidentView = incidentManager.Incident_New(projectId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual("", incidentView.Name);
			Assert.AreEqual("", incidentView.Description);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, incidentView.OpenerId);
			Assert.AreEqual(projectId, incidentView.ProjectId);
			Assert.AreEqual(defaultIncidentStatusId, incidentView.IncidentStatusId);
			Assert.AreEqual(defaultIncidentTypeId, incidentView.IncidentTypeId);

			//Next lets create some incidents
			int incidentId1 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 1", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId2 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 2", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId3 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 3", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId4 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 4", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId5 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 5", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId6 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 6", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId7 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 7", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId8 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 8", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId9 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 9", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);
			int incidentId10 = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Test Incident 10", "This is a test incident", null, null, null, null, null, DateTime.UtcNow, null, null, null, null, null, null, null, USER_ID_FRED_BLOGGS);

			//Lets create two releases, with the first one having two iterations
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.0", null, "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now, DateTime.Now.AddMonths(2), 2, 0, null, false);
			int iterationId1 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 001", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now, DateTime.Now.AddMonths(1), 2, 0, null, false);
			int iterationId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Sprint 1.0 002", null, "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Now.AddMonths(1), DateTime.Now.AddMonths(2), 2, 0, null, false);
			int releaseId2 = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 2.0", null, "2.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Now.AddMonths(2), DateTime.Now.AddMonths(4), 2, 0, null, false);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId1);
			releaseManager.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId2);

			//Now lets verify that no releases/iterations have incidents
			List<IncidentView> incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(10, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId2, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(0, incidents.Count);

			//Verify that all the incidents are in the initial status and have no release set
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, null, defaultIncidentStatusId);
			Assert.AreEqual(10, incidents.Count);

			//Verify that all the incidents have no priority set
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, null);
			Assert.AreEqual(10, incidents.Count);

			//Verify that all the incidents are unassigned
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, true);
			Assert.AreEqual(0, incidents.Count);

			//Now lets assign the priority to some of the incidents
			Incident incident = incidentManager.RetrieveById(incidentId3, false);
			incident.StartTracking();
			incident.PriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).First().PriorityId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId4, false);
			incident.StartTracking();
			incident.PriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).First().PriorityId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId5, false);
			incident.StartTracking();
			incident.PriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).Skip(1).First().PriorityId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId6, false);
			incident.StartTracking();
			incident.PriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).Skip(1).First().PriorityId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Now lets assign the incidents to various releases and iterations
			incidentManager.AssociateToIteration(new List<int> { incidentId1, incidentId2 }, releaseId1, USER_ID_FRED_BLOGGS);
			incidentManager.AssociateToIteration(new List<int> { incidentId3, incidentId4, incidentId5 }, iterationId1, USER_ID_FRED_BLOGGS);
			incidentManager.AssociateToIteration(new List<int> { incidentId6, incidentId7, incidentId8 }, iterationId2, USER_ID_FRED_BLOGGS);
			incidentManager.AssociateToIteration(new List<int> { incidentId9, incidentId10 }, releaseId2, USER_ID_FRED_BLOGGS);

			//Now lets assign the status to some of the incidents
			incident = incidentManager.RetrieveById(incidentId3, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Open").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId4, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Assigned").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId5, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Assigned").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId6, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Resolved").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId1, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Duplicate").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);
			incident = incidentManager.RetrieveById(incidentId2, false);
			incident.StartTracking();
			incident.IncidentStatusId = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Resolved").IncidentStatusId;
			incidentManager.Update(incident, USER_ID_FRED_BLOGGS);

			//Assign some of the incidents to users
			incidentManager.AssignToUser(incidentId3, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);
			incidentManager.AssignToUser(incidentId4, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS);
			incidentManager.AssignToUser(incidentId5, USER_ID_JOE_SMITH, USER_ID_FRED_BLOGGS);

			//Verify that the history item is recorded (wasn't previously due to a bug)
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveByArtifactId(incidentId3, Artifact.ArtifactTypeEnum.Incident);
			//Assert.IsTrue(historyChangeSets.Any(h => h.Details.Any(d => d.FieldName == "OwnerId")));

			//Now lets verify the distribution of *open* incidents by release
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, null, false);
			Assert.AreEqual(7, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId1, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId1, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, iterationId2, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId2, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByReleaseId(projectId, releaseId2, false);
			Assert.AreEqual(0, incidents.Count);

			//Now lets verify the distribution of incidents by status
			int statusId_new = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "New").IncidentStatusId;
			int statusId_open = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Open").IncidentStatusId;
			int statusId_assigned = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Assigned").IncidentStatusId;
			int statusId_resolved = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Resolved").IncidentStatusId;
			int statusId_duplicate = incidentManager.IncidentStatus_Retrieve(projectTemplateId).FirstOrDefault(s => s.Name == "Duplicate").IncidentStatusId;

			//No Release
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, null, defaultIncidentStatusId);
			Assert.AreEqual(4, incidents.Count);
			//All Releases
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, -2, statusId_new);
			Assert.AreEqual(4, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, -2, statusId_open);
			Assert.AreEqual(1, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, -2, statusId_assigned);
			Assert.AreEqual(2, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, -2, statusId_resolved);
			Assert.AreEqual(2, incidents.Count);
			//Release 1
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, releaseId1, statusId_new);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, releaseId1, statusId_open);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, releaseId1, statusId_assigned);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, releaseId1, statusId_resolved);
			Assert.AreEqual(0, incidents.Count);
			//Iteration 1
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, iterationId1, statusId_new);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, iterationId1, statusId_open);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, iterationId1, statusId_assigned);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByStatusId(projectId, iterationId1, statusId_resolved);
			Assert.AreEqual(0, incidents.Count);

			//Now lets verify the distribution of incidents by priority
			int priorityId_1 = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(s => s.Name == "1 - Critical").PriorityId;
			int priorityId_2 = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(s => s.Name == "2 - High").PriorityId;
			int priorityId_3 = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(s => s.Name == "3 - Medium").PriorityId;
			int priorityId_4 = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(s => s.Name == "4 - Low").PriorityId;

			//All Releases (open statuses)
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, null);
			Assert.AreEqual(6, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, priorityId_1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, priorityId_2);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, priorityId_3);
			Assert.AreEqual(2, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, null, priorityId_4);
			Assert.AreEqual(2, incidents.Count);

			//Specific Release
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, releaseId1, null);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, releaseId1, priorityId_1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, releaseId1, priorityId_2);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, releaseId1, priorityId_3);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, releaseId1, priorityId_4);
			Assert.AreEqual(0, incidents.Count);

			//Specific Iteration
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, iterationId1, null);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, iterationId1, priorityId_1);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, iterationId1, priorityId_2);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, iterationId1, priorityId_3);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveByPriorityId(projectId, -2, priorityId_4);
			Assert.AreEqual(0, incidents.Count);

			//Now lets verify the distribution of incidents by person
			//No Release
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, null, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, null, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, incidents.Count);
			//All Releases
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, null);
			Assert.AreEqual(4, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(2, incidents.Count);
			incidents = incidentManager.Incident_RetrieveAllReleasesByUserId(projectId, USER_ID_JOE_SMITH);
			Assert.AreEqual(1, incidents.Count);
			//Specific Release (including child iterations)
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, null, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, incidents.Count);
			//Specific Release (excluding child iterations)
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, null, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_FRED_BLOGGS, false);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, releaseId1, USER_ID_JOE_SMITH, false);
			Assert.AreEqual(0, incidents.Count);
			//Specific Iteration
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, null, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_FRED_BLOGGS, true);
			Assert.AreEqual(0, incidents.Count);
			incidents = incidentManager.Incident_RetrieveBacklogByUserId(projectId, iterationId1, USER_ID_JOE_SMITH, true);
			Assert.AreEqual(0, incidents.Count);

			//Clean up by deleting the created incidents
			incidentManager.MarkAsDeleted(projectId, incidentId1, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId2, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId3, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId4, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId5, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId6, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId7, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId8, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId9, USER_ID_FRED_BLOGGS);
			incidentManager.MarkAsDeleted(projectId, incidentId10, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Tests the code that is used to populate the Project Group > Incidents list page
		/// </summary>
		[Test, SpiraTestCase(1645)]
		public void _23_ProjectGroupIncidentTests()
		{
			//First we need to get the default list of fields to be displayed on the project group > incidents page
			List<ArtifactListFieldDisplay> incidentFields = incidentManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
			Assert.AreEqual(23, incidentFields.Count);
			Assert.IsTrue(incidentFields.FirstOrDefault(r => r.Name == "IncidentStatusId").IsVisible);

			//Toggle the field
			incidentManager.ToggleProjectGroupColumnVisibility(USER_ID_FRED_BLOGGS, PROJECT_GROUP_ID, "IncidentStatusId", Web.GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
			incidentFields = incidentManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
			Assert.AreEqual(23, incidentFields.Count);
			Assert.IsFalse(incidentFields.FirstOrDefault(r => r.Name == "IncidentStatusId").IsVisible);

			//Toggle it back
			incidentManager.ToggleProjectGroupColumnVisibility(USER_ID_FRED_BLOGGS, PROJECT_GROUP_ID, "IncidentStatusId", Web.GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
			incidentFields = incidentManager.RetrieveFieldsForProjectGroupLists(PROJECT_GROUP_ID, USER_ID_FRED_BLOGGS, Web.GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);
			Assert.AreEqual(23, incidentFields.Count);
			Assert.IsTrue(incidentFields.FirstOrDefault(r => r.Name == "IncidentStatusId").IsVisible);

			//Next we need to test the functions that retrieve and count all the incidents across a project group
			//TODO: Right now only one project has incidents, change these tests after we add more incidents to others

			//First lets count all of the incidents in the group unfiltered
			int count = incidentManager.Incident_CountForGroup(PROJECT_GROUP_ID, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(60, count);
			List<IncidentView> incidents = incidentManager.Incident_RetrieveForGroup(PROJECT_GROUP_ID, "IncidentId", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(count, incidents.Count);
			Assert.AreEqual(count - 0, incidents.Where(i => i.ProjectId == 1).Count());
			Assert.AreEqual(0, incidents.Where(i => i.ProjectId == 2).Count());
			Assert.AreEqual("Library Information System (Sample)", incidents.FirstOrDefault(i => i.ProjectId == 1).ProjectName);
			//Assert.AreEqual("Sample: Barebones Product", incidents.FirstOrDefault(i => i.ProjectId == 2).ProjectName);

			//Now lets try some filtering
			Hashtable filters = new Hashtable();

			//All Open Incidents
			filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllOpen);
			count = incidentManager.Incident_CountForGroup(PROJECT_GROUP_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(35, count);
			incidents = incidentManager.Incident_RetrieveForGroup(PROJECT_GROUP_ID, "IncidentId", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(count, incidents.Count);
			Assert.AreEqual(count - 0, incidents.Where(i => i.ProjectId == 1).Count());
			Assert.AreEqual(0, incidents.Where(i => i.ProjectId == 2).Count());

			//All Closed Incidents
			filters.Clear();
			filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllClosed);
			count = incidentManager.Incident_CountForGroup(PROJECT_GROUP_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(25, count);
			incidents = incidentManager.Incident_RetrieveForGroup(PROJECT_GROUP_ID, "IncidentId", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(count, incidents.Count);
			Assert.AreEqual(count - 0, incidents.Where(i => i.ProjectId == 1).Count());
			Assert.AreEqual(0, incidents.Where(i => i.ProjectId == 2).Count());

			//Project 1
			filters.Clear();
			filters.Add("ProjectId", 1);
			count = incidentManager.Incident_CountForGroup(PROJECT_GROUP_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(60, count);
			incidents = incidentManager.Incident_RetrieveForGroup(PROJECT_GROUP_ID, "IncidentId", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(count, incidents.Count);

			//Project 2
			filters.Clear();
			filters.Add("ProjectId", 2);
			count = incidentManager.Incident_CountForGroup(PROJECT_GROUP_ID, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(0, count);
			incidents = incidentManager.Incident_RetrieveForGroup(PROJECT_GROUP_ID, "IncidentId", true, 1, Int32.MaxValue, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(count, incidents.Count);
		}

		/// <summary>
		/// Tests that we can create an incident from an existing task
		/// </summary>
		[Test, SpiraTestCase(2490)]
		public void _24_CreateIncidentFromTask()
		{
			//Create a new task that we'll be making the incident from
			//We need to specify a priority to make sure that gets converted
			//Also need to create a requirement and component
			ComponentManager componentManager = new ComponentManager();
			ReleaseManager releaseManager = new ReleaseManager();
			DiscussionManager discussionManager = new DiscussionManager();
			TaskManager taskManager = new TaskManager();
			RequirementManager requirementManager = new RequirementManager();

			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1.2.1.0", "", "1.2.1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 2, 0, null, false);
			int taskPriorityId = taskManager.TaskPriority_Retrieve(projectTemplateId, true).FirstOrDefault(i => i.Name == "2 - High").TaskPriorityId;
			int componentId = componentManager.Component_Insert(projectId, "Component X");
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId, componentId, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, null, null, "Requirement for Task 1", null, null, USER_ID_FRED_BLOGGS);
			int taskId = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId, releaseId, USER_ID_JOE_SMITH, taskPriorityId, "Sample Task 1", null, DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 100, 50, 80);
			discussionManager.Insert(USER_ID_FRED_BLOGGS, taskId, Artifact.ArtifactTypeEnum.Task, "Comment 1", projectId, false, false);

			//We also need to add a list custom property to make sure it gets copied across
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			int customListId1 = customPropertyManager.CustomPropertyList_Add(projectTemplateId, "Test List", true, true).CustomPropertyListId;
			int customValueId1 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Value 1").CustomPropertyValueId;
			int customValueId2 = customPropertyManager.CustomPropertyList_AddValue(customListId1, "Value 2").CustomPropertyValueId;
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Task, (int)CustomProperty.CustomPropertyTypeEnum.List, 1, "Test Prop", null, null, customListId1);
			customPropertyManager.CustomPropertyDefinition_AddToArtifact(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, (int)CustomProperty.CustomPropertyTypeEnum.List, 2, "Test Prop", null, null, customListId1);
			ArtifactCustomProperty acp = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, taskId);
			acp.Custom_01 = customValueId1.ToDatabaseSerialization();
			customPropertyManager.ArtifactCustomProperty_Save(acp, USER_ID_FRED_BLOGGS);

			//We also need to attach a document to make sure it gets copied across
			AttachmentManager attachmentManager = new AttachmentManager();
			int attachmentId = attachmentManager.Insert(projectId, "www.x.com", null, USER_ID_FRED_BLOGGS, taskId, Artifact.ArtifactTypeEnum.Task, "1.0", null, null, null, null);

			//Now create the incident from this task
			int incidentId = incidentManager.Incident_CreateFromTask(taskId, USER_ID_JOE_SMITH);

			//Verify the incident itself
			int incidentPriorityId = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true).FirstOrDefault(i => i.Name == "2 - High").PriorityId;
			Incident incident = incidentManager.RetrieveById(incidentId, true);
			Assert.AreEqual(incident.Name, "Sample Task 1");
			Assert.AreEqual(incident.Description, "Incident created from existing task.");
			Assert.AreEqual(incident.EstimatedEffort, 100);
			Assert.AreEqual(incident.ActualEffort, 50);
			Assert.AreEqual(incident.RemainingEffort, 80);
			Assert.AreEqual(incident.OpenerId, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(incident.OwnerId, USER_ID_JOE_SMITH);
			Assert.AreEqual(incident.DetectedReleaseId, releaseId);
			Assert.AreEqual(incident.ResolvedReleaseId, releaseId);
			Assert.IsFalse(incident.VerifiedReleaseId.HasValue);
			Assert.AreEqual(incident.ComponentIds.FromDatabaseSerialization_Int32(), componentId);
			Assert.AreEqual(incident.PriorityId, incidentPriorityId);

			//Verify that a comment was added to the incident to match the task
			IEnumerable<IncidentResolution> comments = incident.Resolutions;
			Assert.AreEqual(1, comments.Count());
			Assert.AreEqual("Comment 1", comments.First().Resolution);

			//Verify the attachment was copied across
			List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual(attachmentId, attachments[0].AttachmentId);

			//Verify the custom property
			acp = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(customValueId1, acp.Custom_02.FromDatabaseSerialization_Int32());

			//Verify that two associations were created, one to the task, and one to the requirement
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
			List<ArtifactLinkView> associations = artifactLinkManager.RetrieveByArtifactId(Artifact.ArtifactTypeEnum.Incident, incidentId);
			Assert.AreEqual(2, associations.Count);
			Assert.IsTrue(associations.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && a.ArtifactId == requirementId));
			Assert.IsTrue(associations.Any(a => a.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && a.ArtifactId == taskId));

			//Verify the task auto-closed
			Task task = taskManager.RetrieveById(taskId);
			Assert.AreEqual(task.TaskStatusId, (int)Task.TaskStatusEnum.Completed);

			//Clean up
			customPropertyManager.ArtifactCustomProperty_DeleteByArtifactId(incidentId, Artifact.ArtifactTypeEnum.Incident);
			customPropertyManager.ArtifactCustomProperty_DeleteByArtifactId(taskId, Artifact.ArtifactTypeEnum.Task);
			incidentManager.MarkAsDeleted(projectId, incidentId, USER_ID_FRED_BLOGGS);
			requirementManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, requirementId);
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);
			attachmentManager.Delete(projectId, attachmentId, USER_ID_FRED_BLOGGS);
			taskManager.MarkAsDeleted(projectId, taskId, USER_ID_JOE_SMITH);
			componentManager.Component_Delete(componentId);
		}
	}
}
