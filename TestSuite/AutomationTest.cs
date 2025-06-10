using System.Collections;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;

using NUnit.Framework;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;
using System.Data;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Automation business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class AutomationTest
	{
		protected static AutomationManager automationManager;
		protected static HistoryManager historyManager;

		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;
		private static int projectId;
		private static int projectTemplateId;

		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_SYS_ADMIN = 1;

		[TestFixtureSetUp]
		public void Init()
		{
			automationManager = new AutomationManager();
			historyManager = new HistoryManager();

			//Create a new project for testing with (only some tests currently use this)
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("AutomationManagerTest Project", null, null, null, true, null, 1,
					adminSectionId,
					"Inserted Project"); 
			//Get the template associated with the project
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//Delete the temporary project and  its template
			new ProjectManager().Delete(USER_ID_SYS_ADMIN, projectId);
			new TemplateManager().Delete(USER_ID_SYS_ADMIN, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can retrieve a sorted/filtered list of automation hosts and automation engines
		/// </summary>
		[
		Test,
		SpiraTestCase(687)
		]
		public void _01_Retrieves()
		{
			//Create some new hosts
			int automationHostId1 = automationManager.InsertHost(projectId, "Windows Server 1", "Win1", "", true, USER_ID_FRED_BLOGGS);
			int automationHostId2 = automationManager.InsertHost(projectId, "Windows Server 2", "Win2", "", true, USER_ID_FRED_BLOGGS);
			int automationHostId3 = automationManager.InsertHost(projectId, "Windows Server 3", "Win3", "", true, USER_ID_FRED_BLOGGS);
			int automationHostId4 = automationManager.InsertHost(projectId, "Linux Server 1", "Linux1", "", true, USER_ID_FRED_BLOGGS);
			int automationHostId5 = automationManager.InsertHost(projectId, "Linux Server 2", "Linux2", "", true, USER_ID_FRED_BLOGGS);
			int automationHostId6 = automationManager.InsertHost(projectId, "AT&T UNIX Server", "Unix", "Test123", false, USER_ID_FRED_BLOGGS);

			//Now retrieve a host by its id
			AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId1);
			Assert.AreEqual("Windows Server 1", automationHost.Name);
			Assert.AreEqual("Win1", automationHost.Token);
			Assert.AreEqual(true, automationHost.IsActive);
			Assert.IsTrue(automationHost.Description.IsNull());
			Assert.IsNull(automationHost.LastContactDate);

			//Now rerieve a host by its token
			automationHost = automationManager.RetrieveHostByToken(projectId, "Win2");
			Assert.AreEqual("Windows Server 2", automationHost.Name);
			Assert.AreEqual("Win2", automationHost.Token);
			Assert.AreEqual(true, automationHost.IsActive);
			Assert.IsTrue(automationHost.Description.IsNull());
			Assert.IsNull(automationHost.LastContactDate);

			//Now retrieve a lookup of all the active hosts
			List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
			Assert.AreEqual(5, automationHosts.Count);

			//Now retrieve a list of sorted/filtered hosts
			Hashtable filters = new Hashtable();
			//Window hosts sorted by name
			filters.Add("Name", "Windows");
			string sortProperty = "Name";
			bool sortAscending = true;
			automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, 1, 10, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(3, automationHosts.Count);
			Assert.AreEqual("Windows Server 1", automationHosts[0].Name);
			Assert.AreEqual("Windows Server 2", automationHosts[1].Name);
			Assert.AreEqual("Windows Server 3", automationHosts[2].Name);

			//Linux token servers sorted by token
			filters.Clear();
			filters.Add("Token", "Linux");
			sortProperty = "Token";
			sortAscending = false;
			automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, 1, 10, filters, InternalRoutines.UTC_OFFSET);
			Assert.AreEqual(2, automationHosts.Count);
			Assert.AreEqual("Linux Server 2", automationHosts[0].Name);
			Assert.AreEqual("Linux Server 1", automationHosts[1].Name);

			//Now delete the hosts
			automationManager.MarkHostAsDeleted(projectId, automationHostId1, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId2, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId3, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId4, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId5, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId6, USER_ID_FRED_BLOGGS);
			//Now purge the hosts.
			automationManager.DeleteHostFromDatabase(automationHostId1, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId2, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId3, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId4, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId5, USER_ID_FRED_BLOGGS);
			automationManager.DeleteHostFromDatabase(automationHostId6, USER_ID_FRED_BLOGGS);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Test Automation";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Create some new engines
			int automationEngineId1 = automationManager.InsertEngine("Automation Engine 1", "Engine1", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");
			int automationEngineId2 = automationManager.InsertEngine("Automation Engine 2", "Engine2", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");
			int automationEngineId3 = automationManager.InsertEngine("Automation Engine 3", "Engine3", "A Really Good Automation Engine", false, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");

			//Now retrieve an engine by its id
			AutomationEngine automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
			Assert.AreEqual("Automation Engine 1", automationEngine.Name);
			Assert.AreEqual("Engine1", automationEngine.Token);
			Assert.AreEqual(true, automationEngine.IsActive);
			Assert.IsTrue(automationEngine.Description.IsNull());

			//Now retrieve an engine by its token
			automationEngine = automationManager.RetrieveEngineByToken("Engine2");
			Assert.AreEqual("Automation Engine 2", automationEngine.Name);
			Assert.AreEqual("Engine2", automationEngine.Token);
			Assert.AreEqual(true, automationEngine.IsActive);
			Assert.IsTrue(automationEngine.Description.IsNull());

			//Now retrieve a list of all the active engines in the system
			List<AutomationEngine> automationEngines = automationManager.RetrieveEngines(true);
			Assert.AreEqual(6, automationEngines.Count);
			Assert.AreEqual("Automation Engine 1", automationEngines[0].Name);
			Assert.AreEqual("Automation Engine 2", automationEngines[1].Name);
			Assert.AreEqual("NeoLoad", automationEngines[2].Name);
			Assert.AreEqual("Quick Test Pro", automationEngines[3].Name);
			Assert.AreEqual("Rapise", automationEngines[4].Name);
			Assert.AreEqual("Selenium WebDriver", automationEngines[5].Name);

			//Now delete the engines
			automationManager.DeleteEngine(automationEngineId1, USER_ID_FRED_BLOGGS);
			automationManager.DeleteEngine(automationEngineId2, USER_ID_FRED_BLOGGS);
			automationManager.DeleteEngine(automationEngineId3, USER_ID_FRED_BLOGGS);
		}

		/// <summary>
		/// Creates, edits and deletes automation hosts
		/// </summary>
		[
		Test,
		SpiraTestCase(688)
		]
		public void _02_CreateUpdateDeleteHosts()
		{
			//Create some new hosts
			int automationHostId1 = automationManager.InsertHost(projectId, "Windows Server 1", "Win1", null, true, USER_ID_FRED_BLOGGS);
			int automationHostId2 = automationManager.InsertHost(projectId, "Windows Server 2", "Win2", null, true, USER_ID_FRED_BLOGGS);
			int automationHostId3 = automationManager.InsertHost(projectId, "Windows Server 3", "Win3", null, true, USER_ID_FRED_BLOGGS);
			int automationHostId4 = automationManager.InsertHost(projectId, "Linux Server 1", "Linux1", null, true, USER_ID_FRED_BLOGGS);
			int automationHostId5 = automationManager.InsertHost(projectId, "Linux Server 2", "Linux2", null, true, USER_ID_FRED_BLOGGS);
			int automationHostId6 = automationManager.InsertHost(projectId, "AT&T UNIX Server", "Unix", "Test123", false, USER_ID_FRED_BLOGGS);

			//Verify some of the inserts
			AutomationHostView automationHost = automationManager.RetrieveHostById(automationHostId1);
			Assert.AreEqual("Windows Server 1", automationHost.Name);
			Assert.AreEqual("Win1", automationHost.Token);
			Assert.AreEqual(true, automationHost.IsActive);
			Assert.IsTrue(automationHost.Description.IsNull());

			automationHost = automationManager.RetrieveHostById(automationHostId6);
			Assert.AreEqual("AT&T UNIX Server", automationHost.Name);
			Assert.AreEqual("Unix", automationHost.Token);
			Assert.AreEqual(false, automationHost.IsActive);
			Assert.AreEqual("Test123", automationHost.Description);

			//Now update some hosts
			AutomationHost automationHostEditable = automationManager.RetrieveHostById2(automationHostId1);
			automationHostEditable.StartTracking();
			automationHostEditable.Name = "Windows Server 1a";
			automationHostEditable.Token = "Win1a";
			automationHostEditable.Description = "Windows 2003 Server running SP2";
			automationHostEditable.IsActive = false;
			automationManager.UpdateHost(automationHostEditable, USER_ID_FRED_BLOGGS);

			//Verify the updates
			automationHost = automationManager.RetrieveHostById(automationHostId1);
			Assert.AreEqual("Windows Server 1a", automationHost.Name);
			Assert.AreEqual("Win1a", automationHost.Token);
			Assert.AreEqual(false, automationHost.IsActive);
			Assert.AreEqual("Windows 2003 Server running SP2", automationHost.Description);

			//Verify that history entries were added
			List<HistoryChangeSetView> historyChangeSets;
			List<HistoryChangeSetType> historyChangeSetTypes;
			List<HistoryChangeView> historyChanges;
			historyManager.RetrieveByArtifactId(automationHostId1, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, out historyChangeSets, out historyChanges, out historyChangeSetTypes);
			bool nameChange = false;
			bool tokenChange = false;
			bool descChange = false;
			bool activeChange = false;
			//Assert.AreEqual(5, historyChangeSets.Count);
			//Assert.AreEqual(5, historyChanges.Count);
			foreach (HistoryChangeView historyRow in historyChanges)
			{
				//Look for the various changes
				if (historyRow.FieldName == "Name" && historyRow.OldValue == "Windows Server 1" && historyRow.NewValue == "Windows Server 1a")
				{
					nameChange = true;
				}
				if (historyRow.FieldName == "Token" && historyRow.OldValue == "Win1" && historyRow.NewValue == "Win1a")
				{
					tokenChange = true;
				}
				if (historyRow.FieldName == "Description" && historyRow.OldValue.IsNull() && historyRow.NewValue == "Windows 2003 Server running SP2")
				{
					descChange = true;
				}
				if (historyRow.FieldName == "IsActive" && historyRow.OldValue == "Y" && historyRow.NewValue == "N")
				{
					activeChange = true;
				}
			}
			Assert.IsTrue(nameChange);
			Assert.IsTrue(tokenChange);
			Assert.IsTrue(descChange);
			//Assert.IsTrue(activeChange);

			//Verify that concurrency is handled correctly
			AutomationHost automationHost1 = automationManager.RetrieveHostById2(automationHostId1);
			AutomationHost automationHost2 = automationHost1.Clone();

			//Now make a change to field and update
			automationHost1.StartTracking();
			automationHost1.Name = "Windows Server 1b";
			automationManager.UpdateHost(automationHost1, USER_ID_FRED_BLOGGS);

			//Verify it updated correctly using separate entity
			AutomationHost automationHost3 = automationManager.RetrieveHostById2(automationHostId1);
			Assert.AreEqual("Windows Server 1b", automationHost3.Name);

			//Now try making a change using the out of date dataset (has the wrong ConcurrencyDate)
			bool exceptionThrown = false;
			try
			{
				automationHost2.StartTracking();
				automationHost2.Name = "Windows Server 1c";
				automationManager.UpdateHost(automationHost2, USER_ID_FRED_BLOGGS);
			}
			catch (OptimisticConcurrencyException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Make sure the token name is guaranteed to be unique
			exceptionThrown = false;
			try
			{
				automationHostEditable = automationManager.RetrieveHostById2(automationHostId1);
				automationHostEditable.StartTracking();
				automationHostEditable.Token = "Win2";
				automationManager.UpdateHost(automationHostEditable, USER_ID_FRED_BLOGGS);
			}
			catch (EntityConstraintViolationException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Now delete the hosts
			automationManager.MarkHostAsDeleted(projectId, automationHostId1, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId2, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId3, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId4, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId5, USER_ID_FRED_BLOGGS);
			automationManager.MarkHostAsDeleted(projectId, automationHostId6, USER_ID_FRED_BLOGGS);

			//Verify one of the deletes
			exceptionThrown = false;
			try
			{
				automationHost = automationManager.RetrieveHostById(automationHostId1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}

		/// <summary>
		/// Creates, edits and deletes automation engines
		/// </summary>
		[
		Test,
		SpiraTestCase(689)
		]
		public void _03_CreateUpdateDeleteEngines()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "Test Automation";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);
			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Create some new engines
			int automationEngineId1 = automationManager.InsertEngine("Automation Engine 1", "Engine1", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");
			int automationEngineId2 = automationManager.InsertEngine("Automation Engine 2", "Engine2", "", true, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");
			int automationEngineId3 = automationManager.InsertEngine("Automation Engine 3", "Engine3", "A Really Good Automation Engine", false, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, adminSectionId, "Inserted Test Automation");

			//Verify some of the inserts
			AutomationEngine automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
			Assert.AreEqual("Automation Engine 1", automationEngine.Name);
			Assert.AreEqual("Engine1", automationEngine.Token);
			Assert.AreEqual(true, automationEngine.IsActive);
			Assert.IsTrue(automationEngine.Description.IsNull());

			automationEngine = automationManager.RetrieveEngineById(automationEngineId3);
			Assert.AreEqual("Automation Engine 3", automationEngine.Name);
			Assert.AreEqual("Engine3", automationEngine.Token);
			Assert.AreEqual(false, automationEngine.IsActive);
			Assert.AreEqual("A Really Good Automation Engine", automationEngine.Description);

			//Now update one of the engines
			automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
			automationEngine.StartTracking();
			automationEngine.Name = "Automation Engine 1a";
			automationEngine.Token = "Engine1a";
			automationEngine.Description = "New Prototype Automation Engine";
			automationEngine.IsActive = false;
			automationManager.UpdateEngine(automationEngine, USER_ID_FRED_BLOGGS);

			//Verify the updates
			automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
			Assert.AreEqual("Automation Engine 1a", automationEngine.Name);
			Assert.AreEqual("Engine1a", automationEngine.Token);
			Assert.AreEqual(false, automationEngine.IsActive);
			Assert.AreEqual("New Prototype Automation Engine", automationEngine.Description);

			//Make sure the token name is guaranteed to be unique
			bool exceptionThrown = false;
			try
			{
				automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
				automationEngine.StartTracking();
				automationEngine.Token = "Engine2";
				automationManager.UpdateEngine(automationEngine, USER_ID_FRED_BLOGGS);
			}
			catch (EntityConstraintViolationException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Now delete the engines
			automationManager.DeleteEngine(automationEngineId1, USER_ID_FRED_BLOGGS);
			automationManager.DeleteEngine(automationEngineId2, USER_ID_FRED_BLOGGS);
			automationManager.DeleteEngine(automationEngineId3, USER_ID_FRED_BLOGGS);

			//Verify one of the deletes
			exceptionThrown = false;
			try
			{
				automationEngine = automationManager.RetrieveEngineById(automationEngineId1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown);
		}
	}
}
