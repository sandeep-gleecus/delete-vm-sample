using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web.Security;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the Project business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class ProjectManagerTest
	{
		protected static Business.ProjectManager projectManager;
		protected static Business.TemplateManager templateManager;

		protected static int projectId;
		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;

		protected static int projectId_forSettings1 = -1;
		protected static int projectId_forSettings2 = -1;
		protected static int projectId_forSettings3 = -1;

		private const int PROJECT_ID = 1;
		private const int PROJECT_TEMPLATE_ID = 1;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_ROGER_RAMJET = 4;

		Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

		[TestFixtureSetUp]
		public void Init()
		{
			projectManager = new ProjectManager();
			templateManager = new TemplateManager();

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

		[TestFixtureTearDown]
		public void CleanUp()
		{
			//We need to delete any artifact history items created during the test run
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());

			//Delete any temporary projects
			if (projectId_forSettings1 > 0)
			{
				projectManager.Delete(USER_ID_SYS_ADMIN, projectId_forSettings1);
			}
			if (projectId_forSettings2 > 0)
			{
				projectManager.Delete(USER_ID_SYS_ADMIN, projectId_forSettings2);
			}
			if (projectId_forSettings3 > 0)
			{
				projectManager.Delete(USER_ID_SYS_ADMIN, projectId_forSettings3);
			}
		}

		[
		Test,
		SpiraTestCase(26)
		]
		public void _01_Retrieves()
		{
			//First lets test that we can retrieve a single project record
			ProjectView project = projectManager.RetrieveById2(1);
			Assert.AreEqual(1, project.ProjectId);
			Assert.AreEqual("Library Information System (Sample)", project.Name);
			Assert.AreEqual("Sample application that allows users to manage books, authors and lending records for a typical branch library", project.Description);
			Assert.AreEqual("www.libraryinformationsystem.org", project.Website);
			//Assert.IsTrue(project.CreationDate >= DateTime.UtcNow.AddDays(-152));
			Assert.AreEqual(true, project.IsActive);
			Assert.AreEqual(2, project.ProjectGroupId);
			Assert.AreEqual("Sample Program", project.ProjectGroupName);

			//Used for filtering later
			DateTime filterDateTime = project.CreationDate.Date;

			//Now lets test that we can retrieve a list of projects for a specific user
			List<ProjectForUserView> projectsForUser = projectManager.RetrieveForUser(2);
			Assert.AreEqual(7, projectsForUser.Count);
			Assert.AreEqual(4, projectsForUser[2].ProjectId);
			Assert.AreEqual("ERP: Financials", projectsForUser[2].Name);
			Assert.AreEqual("An example ERP product for example, Dynamics AX or SAP. This is the core financials system.", projectsForUser[2].Description);
			Assert.IsTrue(String.IsNullOrEmpty(projectsForUser[2].Website));
			//Assert.IsTrue(projectsForUser[2].CreationDate >= DateTime.UtcNow.AddDays(-152));
			Assert.AreEqual(true, projectsForUser[2].IsActive);
			Assert.AreEqual(3, projectsForUser[2].ProjectGroupId);
			Assert.AreEqual("Corporate Systems", projectsForUser[2].ProjectGroupName);

			//Now lets test that we can retrieve the list of projects in the system un-filtered
			List<ProjectView> projects = projectManager.Retrieve(null, null);
			Assert.AreEqual(7, projects.Count);
			Assert.AreEqual("Company Website", projects[0].Name);
			Assert.AreEqual("Sales and Marketing", projects[0].ProjectGroupName);
			Assert.AreEqual("ERP: Human Resources", projects[3].Name);
			Assert.AreEqual("Corporate Systems", projects[3].ProjectGroupName);

			//This should match the projects returned by the count method
			int projectsCountAll = projectManager.Count(true);
			int projectsCountActive = projectManager.Count();
			Assert.AreEqual(7, projectsCountAll);
			Assert.AreEqual(7, projectsCountActive);

			//Now lets test that we can retrieve the list of projects in the system filtered
			Hashtable filters = new Hashtable();
			filters.Add("Name", "Lib");
			filters.Add("WebSite", "w");
			filters.Add("CreationDate", filterDateTime);
			filters.Add("ActiveYn", "Y");
			projects = projectManager.Retrieve(filters, null);
			Assert.AreEqual(1, projects.Count);
			Assert.AreEqual("Library Information System (Sample)", projects[0].Name);
			Assert.AreEqual("Sample Program", projects[0].ProjectGroupName);

			//Now lets test that we can retrieve the list of projects that we're the project owner of (zero in fact!)
			projects = projectManager.Retrieve(null, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(0, projects.Count);

			//Now lets test that we can retrieve the list of projects in a specific group

			//Default Group
			projects = projectManager.Project_RetrieveByGroup(1);
			Assert.AreEqual(0, projects.Count);

			//Sample Program
			projects = projectManager.Project_RetrieveByGroup(2);
			Assert.AreEqual(3, projects.Count);
			Assert.AreEqual("Library Information System (Sample)", projects[0].Name);
			Assert.AreEqual("Sample Empty Product 1", projects[1].Name);
			Assert.AreEqual("Sample Empty Product 2", projects[2].Name);

			//Corporate Systems
			projects = projectManager.Project_RetrieveByGroup(3);
			Assert.AreEqual(2, projects.Count);
			Assert.AreEqual("ERP: Financials", projects[0].Name);
			Assert.AreEqual("ERP: Human Resources", projects[1].Name);
		}

		[
		Test,
		SpiraTestCase(49)
		]
		public void _02_EditProject()
		{
			//Now lets test that we can modify the details of a project

			//First verify its current state
			ProjectView projectView = projectManager.RetrieveById2(1);
			Assert.AreEqual(1, projectView.ProjectId);
			Assert.AreEqual("Library Information System (Sample)", projectView.Name);
			Assert.AreEqual("Sample application that allows users to manage books, authors and lending records for a typical branch library", projectView.Description);
			Assert.AreEqual("www.libraryinformationsystem.org", projectView.Website);
			//Assert.IsTrue(projectView.CreationDate >= DateTime.UtcNow.AddDays(-152));
			Assert.AreEqual(true, projectView.IsActive);
			Assert.AreEqual(2, projectView.ProjectGroupId);
			Assert.AreEqual("Sample Program", projectView.ProjectGroupName);

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Now make some changes and update
			Project project = projectManager.RetrieveById(1);
			project.StartTracking();
			project.Name = "Library Management System";
			project.Description = "Changed Project Description";
			project.Website = "www.librarymanagementsystem.org";
			project.IsActive = false;
			project.ProjectGroupId = 3;   //Corporate Systems
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now verify the changes were made
			projectView = projectManager.RetrieveById2(1);
			Assert.AreEqual(1, projectView.ProjectId);
			Assert.AreEqual("Library Management System", projectView.Name);
			Assert.AreEqual("Changed Project Description", projectView.Description);
			Assert.AreEqual("www.librarymanagementsystem.org", projectView.Website);
			//Assert.IsTrue(projectView.CreationDate >= DateTime.UtcNow.AddDays(-152));
			Assert.AreEqual(false, projectView.IsActive);
			Assert.AreEqual(3, projectView.ProjectGroupId);
			Assert.AreEqual("Corporate Systems", projectView.ProjectGroupName);

			//Now return the project to its initial state
			project = projectManager.RetrieveById(1);
			project.StartTracking();
			project.Name = "Library Information System (Sample)";
			project.Description = "Sample application that allows users to manage books, authors and lending records for a typical branch library";
			project.Website = "www.libraryinformationsystem.org";
			project.IsActive = true;
			project.ProjectGroupId = 2;   //Sample Program One
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now verify the changes were made
			projectView = projectManager.RetrieveById2(1);
			Assert.AreEqual(1, projectView.ProjectId);
			Assert.AreEqual("Library Information System (Sample)", projectView.Name);
			Assert.AreEqual("Sample application that allows users to manage books, authors and lending records for a typical branch library", projectView.Description);
			Assert.AreEqual("www.libraryinformationsystem.org", projectView.Website);
			//Assert.IsTrue(projectView.CreationDate >= DateTime.UtcNow.AddDays(-152));
			Assert.AreEqual(true, projectView.IsActive);
			Assert.AreEqual(2, projectView.ProjectGroupId);
			Assert.AreEqual("Sample Program", projectView.ProjectGroupName);
		}

		[
		Test,
		SpiraTestCase(158)
		]
		public void _03_Edit_Membership()
		{
			//Lets test that we can retrieve the user membership list for a project
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(1);
			Assert.AreEqual(12, projectUsers.Count);
			Assert.AreEqual("System Administrator", projectUsers[11].FullName);
			Assert.AreEqual("administrator", projectUsers[11].UserName);
			Assert.AreEqual("Product Owner", projectUsers[11].ProjectRoleName);
			Assert.AreEqual("Fred Bloggs", projectUsers[3].FullName);
			Assert.AreEqual("fredbloggs", projectUsers[3].UserName);
			Assert.AreEqual("Manager", projectUsers[3].ProjectRoleName);
			Assert.AreEqual("Joe P Smith", projectUsers[6].FullName);
			Assert.AreEqual("joesmith", projectUsers[6].UserName);
			Assert.AreEqual("Observer", projectUsers[6].ProjectRoleName);

			//Now lets make some changes to the roles we should be able to change (i.e. not the built-in sys admin)
			projectUsers[3].StartTracking();
			projectUsers[3].ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			projectUsers[6].StartTracking();
			projectUsers[6].ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			projectManager.UpdateMembership(projectUsers);

			//Verify the changes occured
			projectUsers = projectManager.RetrieveUserMembershipById(1);
			Assert.AreEqual(12, projectUsers.Count);
			Assert.AreEqual("System Administrator", projectUsers[11].FullName);
			Assert.AreEqual("administrator", projectUsers[11].UserName);
			Assert.AreEqual("Product Owner", projectUsers[11].ProjectRoleName);
			Assert.AreEqual("Fred Bloggs", projectUsers[3].FullName);
			Assert.AreEqual("fredbloggs", projectUsers[3].UserName);
			Assert.AreEqual("Developer", projectUsers[3].ProjectRoleName);
			Assert.AreEqual("Joe P Smith", projectUsers[6].FullName);
			Assert.AreEqual("joesmith", projectUsers[6].UserName);
			Assert.AreEqual("Developer", projectUsers[6].ProjectRoleName);

			//Now need to put them back
			//Now lets make some changes to the roles we should be able to change (i.e. not the built-in sys admin)
			projectUsers[3].StartTracking();
			projectUsers[3].ProjectRoleId = InternalRoutines.PROJECT_ROLE_MANAGER;
			projectUsers[6].StartTracking();
			projectUsers[6].ProjectRoleId = InternalRoutines.PROJECT_ROLE_OBSERVER;
			projectManager.UpdateMembership(projectUsers);

			//Verify the changes occured
			projectUsers = projectManager.RetrieveUserMembershipById(1);
			Assert.AreEqual(12, projectUsers.Count);
			Assert.AreEqual("administrator", projectUsers[11].UserName);
			Assert.AreEqual("Product Owner", projectUsers[11].ProjectRoleName);
			Assert.AreEqual("Manager", projectUsers[3].ProjectRoleName);
			Assert.AreEqual("Observer", projectUsers[6].ProjectRoleName);

			//Try to change the system administration role - it will simply not update
			projectUsers[11].StartTracking();
			projectUsers[11].ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
			projectManager.UpdateMembership(projectUsers);

			//Make sure it didn't change
			projectUsers = projectManager.RetrieveUserMembershipById(1);
			Assert.AreEqual(12, projectUsers.Count);
			Assert.AreEqual("administrator", projectUsers[11].UserName);
			Assert.AreEqual("Product Owner", projectUsers[11].ProjectRoleName);

			//Finally verify that we display the list of projects that a user is a member of
			//This function includes any inactive projects
			projectUsers = projectManager.RetrieveProjectMembershipForUser(Business.UserManager.UserSystemAdministrator);
			Assert.AreEqual(7, projectUsers.Count);
			Assert.AreEqual("Company Website", projectUsers[0].ProjectName);
			Assert.AreEqual(true, projectUsers[0].Project.IsActive);
			Assert.AreEqual("Customer Relationship Management (CRM)", projectUsers[1].ProjectName);
			Assert.AreEqual(true, projectUsers[1].Project.IsActive);
			Assert.AreEqual("ERP: Financials", projectUsers[2].ProjectName);
			Assert.AreEqual(true, projectUsers[2].Project.IsActive);
			Assert.AreEqual("ERP: Human Resources", projectUsers[3].ProjectName);
			Assert.AreEqual(true, projectUsers[3].Project.IsActive);

			//Test that we can also do this including their group membership
			//This one does not include inactive projects
			List<ProjectUserView> projectUsersView = projectManager.RetrieveProjectMembershipForUserIncludingGroupMembership(USER_ID_FRED_BLOGGS);
			Assert.AreEqual(7, projectUsersView.Count);
			Assert.AreEqual("Company Website", projectUsersView[0].ProjectName);
			Assert.AreEqual("Customer Relationship Management (CRM)", projectUsersView[1].ProjectName);
			Assert.AreEqual("ERP: Financials", projectUsersView[2].ProjectName);

			//Test that we can see the permissions that this user has
			List<ProjectViewPermission> projectViewPermissions = projectManager.RetrieveProjectViewPermissionsForUser(USER_ID_FRED_BLOGGS);
			Assert.AreEqual(75, projectViewPermissions.Count);
			Assert.AreEqual(1, projectViewPermissions[0].ProjectId);
			Assert.AreEqual(1, projectViewPermissions[0].ArtifactTypeId);
		}

		[
		Test,
		SpiraTestCase(50)
		]
		public void _04_AddDelete_Membership()
		{
			//First get a count of the existing number of members
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(3);
			Assert.AreEqual(3, projectUsers.Count);

			//First lets try and add a new user membership record and verify it added
			projectManager.InsertUserMembership(3, 3, InternalRoutines.PROJECT_ROLE_MANAGER);
			projectUsers = projectManager.RetrieveUserMembershipById(3);
			Assert.AreEqual(4, projectUsers.Count);
			Assert.AreEqual(3, projectUsers[1].ProjectId);
			Assert.AreEqual(3, projectUsers[1].UserId);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_MANAGER, projectUsers[1].ProjectRoleId);
			Assert.AreEqual("Sample Empty Product 2", projectUsers[1].ProjectName);
			Assert.AreEqual("Joe P Smith", projectUsers[1].FullName);
			Assert.AreEqual("Manager", projectUsers[1].ProjectRoleName);

			//Now make sure we can delete this and verify count afterwards
			projectManager.DeleteUserMembership(3, 3, 1);
			projectUsers = projectManager.RetrieveUserMembershipById(3);
			Assert.AreEqual(3, projectUsers.Count);

			//Make sure we can't delete the sys admin role
			projectManager.DeleteUserMembership(User.UserSystemAdministrator, 3, 1);
			projectUsers = projectManager.RetrieveUserMembershipById(3);
			Assert.AreEqual(3, projectUsers.Count);
		}

		[
		Test,
		SpiraTestCase(51)
		]
		public void _05_CreateProject()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Lets test that we can create a new project using the default project group and a new template of its own
			projectId = projectManager.Insert("New Project", null, "Very Cool New Project", null, true, null, 1, adminSectionId, "Inserted Project");

			//Now verify the insert
			Project project = projectManager.RetrieveById(projectId);
			Assert.AreEqual(projectId, project.ProjectId);
			Assert.AreEqual("New Project", project.Name);
			Assert.AreEqual("Very Cool New Project", project.Description);
			Assert.IsTrue(String.IsNullOrEmpty(project.Website));
			Assert.AreEqual(true, project.IsActive);
			int templateId = project.ProjectTemplateId;

			//All new projects should have at least one new member - the administrator
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(projectId);
			Assert.AreEqual(1, projectUsers.Count);
			Assert.AreEqual(UserManager.UserSystemAdministrator, projectUsers[0].UserId);
			Assert.AreEqual(Business.ProjectManager.ProjectRoleProjectOwner, projectUsers[0].ProjectRoleId);

			//We need to delete the created project and template
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);
		}

		/// <summary>
		/// Lets test that we can create a new project and have the various artifact lists populate
		/// </summary>
		[
		Test,
		SpiraTestCase(184)
		]
		public void _06_ArtifactListsCreated()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Lets test that we can create a new project with a new template and have the various artifact lists populate
			projectId = projectManager.Insert("New Project", null, "Very Cool New Project", null, true, null, 1, adminSectionId, "Inserted Project");
			Project project = projectManager.RetrieveById(projectId);
			int projectTemplateId = project.ProjectTemplateId;

			#region Incident Fields

			//Now verify that the various incident lists were populated
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(projectTemplateId, true);
			Assert.AreEqual(8, incidentTypes.Count);
			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);
			Assert.AreEqual(8, incidentStati.Count);
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true);
			Assert.AreEqual(4, incidentPriorities.Count);
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, true);
			Assert.AreEqual(4, incidentSeverities.Count);

			//Now verify that the various workflow records were added
			Business.WorkflowManager workflowManager = new Business.WorkflowManager();
			List<Workflow> workflows = workflowManager.Workflow_Retrieve(projectTemplateId, true);
			Assert.AreEqual(1, workflows.Count);
			Workflow workflow = workflowManager.Workflow_RetrieveById(workflows[0].WorkflowId, true);
			Assert.AreEqual(16, workflow.Transitions.Count);

			#endregion

			#region Requirement Fields

			RequirementManager requirementManager = new RequirementManager();
			RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();

			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(projectTemplateId, true);
			Assert.AreEqual(7, requirementTypes.Count);
			requirementTypes = requirementManager.RequirementType_Retrieve(projectTemplateId, false);
			Assert.AreEqual(6, requirementTypes.Count);
			List<Importance> requirementPriorities = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
			Assert.AreEqual(4, requirementPriorities.Count);
			List<RequirementWorkflow> requirementWorkflows = requirementWorkflowManager.Workflow_Retrieve(projectTemplateId);
			Assert.AreEqual(1, requirementWorkflows.Count);

			#endregion

			#region Task Fields

			TaskManager taskManager = new TaskManager();
			TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();

			List<TaskType> taskTypes = taskManager.TaskType_Retrieve(projectTemplateId);
			Assert.AreEqual(7, taskTypes.Count);
			List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, taskPriorities.Count);
			List<TaskWorkflow> taskWorkflows = taskWorkflowManager.Workflow_Retrieve(projectTemplateId);
			Assert.AreEqual(1, taskWorkflows.Count);

			#endregion

			#region Test Case Fields

			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();

			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(projectTemplateId);
			Assert.AreEqual(12, testCaseTypes.Count);
			List<TestCasePriority> testCasePriorities = testCaseManager.TestCasePriority_Retrieve(projectTemplateId);
			Assert.AreEqual(4, testCasePriorities.Count);
			List<TestCaseWorkflow> testCaseWorkflows = testCaseWorkflowManager.Workflow_Retrieve(projectTemplateId);
			Assert.AreEqual(1, testCaseWorkflows.Count);

			#endregion

			#region Release Workflows

			ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();

			List<ReleaseWorkflow> releaseWorkflows = releaseWorkflowManager.Workflow_Retrieve(projectTemplateId);
			Assert.AreEqual(1, releaseWorkflows.Count);

			#endregion

			#region Document Fields

			AttachmentManager attachmentManager = new AttachmentManager();
			DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();

			List<DocumentType> documentTypes = attachmentManager.RetrieveDocumentTypes(projectTemplateId, true);
			Assert.AreEqual(1, documentTypes.Count);
			List<DocumentStatus> documentStatuses = attachmentManager.DocumentStatus_Retrieve(projectTemplateId);
			Assert.AreEqual(7, documentStatuses.Count);
			List<DocumentWorkflow> documentWorkflows = documentWorkflowManager.Workflow_Retrieve(projectTemplateId);
			Assert.AreEqual(1, documentWorkflows.Count);

			#endregion

			//We need to delete the created project and template
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		[
		Test,
		SpiraTestCase(194)
		]
		public void _07_StoreRetrieveUserProjectSettings()
		{
			//Test that we can use the ProjectCollection class to store per-user project data in the database
			//whilst using the Hashtable interface to make its access and storage seamless
			ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_FRED_BLOGGS, "IncidentFiltersList");

			//Verify the existing contents
			projectSettingsCollection.Restore();
			Assert.AreEqual(2, projectSettingsCollection.Count);
			Assert.AreEqual(2, projectSettingsCollection["IncidentTypeId"]);
			Assert.AreEqual(2, projectSettingsCollection["OpenerId"]);

			//Lets try adding a new entry, updating an entry and removing an entry
			projectSettingsCollection.Add("ClosedDate", DateTime.Parse("10/10/2007"));
			projectSettingsCollection["OpenerId"] = 1;
			projectSettingsCollection.Remove("IncidentTypeId");

			//Now save the properties
			projectSettingsCollection.Save();

			//Now verify that the data was saved correctly
			projectSettingsCollection.Restore();
			Assert.AreEqual(2, projectSettingsCollection.Count);
			Assert.AreEqual(DateTime.Parse("10/10/2007"), projectSettingsCollection["ClosedDate"]);
			Assert.AreEqual(1, projectSettingsCollection["OpenerId"]);

			//Finally we need to return the data to its previous state
			projectSettingsCollection["OpenerId"] = 2;
			projectSettingsCollection.Remove("ClosedDate");
			projectSettingsCollection.Add("IncidentTypeId", 2);
			projectSettingsCollection.Save();

			//Now test that we can retrieve the sort string simple setting
			projectSettingsCollection = new ProjectSettingsCollection(PROJECT_ID, USER_ID_FRED_BLOGGS, "IncidentSortExpression");
			projectSettingsCollection.Restore();
			Assert.AreEqual("PriorityName DESC", (string)projectSettingsCollection["SortExpression"]);
		}

		[
		Test,
		SpiraTestCase(264)
		]
		public void _08_CreateModifyDeleteProjectRoles()
		{
			//First test that we can retrieve the list of all project roles (not just active)
			List<ProjectRole> projectRoles = projectManager.RetrieveProjectRoles(false);
			Assert.AreEqual(6, projectRoles.Count);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, projectRoles[0].ProjectRoleId);
			Assert.AreEqual("Product Owner", projectRoles[0].Name);
			Assert.AreEqual("Can see all product artifacts. Can create/modify all artifacts. Can access the product/template administration tools", projectRoles[0].Description);
			Assert.AreEqual(true, projectRoles[0].IsActive);
			Assert.AreEqual(true, projectRoles[0].IsAdmin);
			Assert.AreEqual(true, projectRoles[0].IsSourceCodeEdit);
			Assert.AreEqual(true, projectRoles[0].IsSourceCodeView);
			Assert.AreEqual(true, projectRoles[0].IsDiscussionsAdd);
			Assert.AreEqual(false, projectRoles[0].IsLimitedView);

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Project Roles";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;


			//Now test that we can create a new project role
			int projectRoleId1 = projectManager.InsertProjectRole("Bungler", null, false, true, true, true, false, true, false, USER_ID_FRED_BLOGGS,
					adminSectionId,
					"Inserted Project Role");
			ProjectRole projectRole = projectManager.RetrieveRolePermissions(projectRoleId1);
			Assert.IsNotNull(projectRole);
			Assert.AreEqual(projectRoleId1, projectRole.ProjectRoleId);
			Assert.AreEqual("Bungler", projectRole.Name);
			Assert.IsTrue(projectRole.Description.IsNull());
			Assert.AreEqual(true, projectRole.IsActive);
			Assert.AreEqual(false, projectRole.IsAdmin);
			Assert.AreEqual(false, projectRole.IsSourceCodeEdit);
			Assert.AreEqual(true, projectRole.IsSourceCodeView);
			Assert.AreEqual(true, projectRole.IsDiscussionsAdd);
			Assert.AreEqual(false, projectRole.IsLimitedView);

			//Now update the role
			projectRole.StartTracking();
			projectRole.Description = "Bungles everything";
			projectRole.IsActive = false;
			projectRole.IsDiscussionsAdd = false;
			projectRole.IsSourceCodeEdit = true;
			projectRole.IsLimitedView = true;
			projectManager.UpdateProjectRole(new List<ProjectRole> { projectRole }, USER_ID_FRED_BLOGGS, adminSectionId, "Updated Project Role");

			//Verify the updates
			projectRole = projectManager.RetrieveRolePermissions(projectRoleId1);
			Assert.IsNotNull(projectRole);
			Assert.AreEqual(projectRoleId1, projectRole.ProjectRoleId);
			Assert.AreEqual("Bungler", projectRole.Name);
			Assert.AreEqual("Bungles everything", projectRole.Description);
			Assert.AreEqual(false, projectRole.IsActive);
			Assert.AreEqual(false, projectRole.IsAdmin);
			Assert.AreEqual(true, projectRole.IsSourceCodeEdit);
			Assert.AreEqual(true, projectRole.IsSourceCodeView);
			Assert.AreEqual(false, projectRole.IsDiscussionsAdd);
			Assert.AreEqual(true, projectRole.IsLimitedView);

			//Verify we can retrieve the role details without the permissions correctly
			projectRole = projectManager.RetrieveRoleById(projectRoleId1);
			Assert.IsNotNull(projectRole);
			Assert.AreEqual(projectRoleId1, projectRole.ProjectRoleId);
			Assert.AreEqual("Bungler", projectRole.Name);
			Assert.AreEqual("Bungles everything", projectRole.Description);

			//Now delete the role, testing that it handles the case where the role
			//is being used by a project and a workflow transition
			projectManager.InsertUserMembership(USER_ID_ROGER_RAMJET, PROJECT_ID, projectRoleId1);
			Business.WorkflowManager workflowManager = new Business.WorkflowManager();
			WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 1);
			workflowTransition.StartTracking();
			workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = projectRoleId1, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
			workflowManager.WorkflowTransition_Update(workflowTransition);
			//Verify it added successfully
			workflowTransition = workflowManager.WorkflowTransition_RetrieveById(1, 1);
			Assert.IsTrue(workflowTransition.TransitionRoles.Any(tr => tr.ProjectRoleId == projectRoleId1));
			//Now try the delete
			projectManager.DeleteProjectRole(projectRoleId1, USER_ID_FRED_BLOGGS);

			//We need to verify that we can't delete one of the built-in roles
			bool exceptionMatch = false;
			try
			{
				projectManager.DeleteProjectRole(InternalRoutines.PROJECT_ROLE_MANAGER, USER_ID_FRED_BLOGGS);
			}
			catch (ProjectRoleNotDeletableException exception)
			{
				exceptionMatch = (exception.Message == "You cannot delete one of the default project roles");
			}
			finally
			{
				Assert.AreEqual(true, exceptionMatch, "Should not be able to delete a built-in project role");
			}

			//We need to verify that we can't deactivate the project owner role
			exceptionMatch = false;
			try
			{
				projectRole = projectManager.RetrieveRolePermissions(Business.ProjectManager.ProjectRoleProjectOwner);
				projectRole.StartTracking();
				projectRole.IsActive = false;
				projectManager.UpdateProjectRole(new List<ProjectRole>() { projectRole }, USER_ID_FRED_BLOGGS, adminSectionId, "Updated Project Role");
			}
			catch (ProjectRoleNotDeactivatableException)
			{
				exceptionMatch = true;
			}
			finally
			{
				Assert.IsTrue(exceptionMatch, "Should not be able to deactivate the ProjectOwner role");
			}
		}

		[
		Test,
		SpiraTestCase(265)
		]
		public void _09_ModifyProjectRolePermissions()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Project Roles";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Lets test that we can retrieve a specific project role and its associated permissions
			ProjectRole projectRole = projectManager.RetrieveRolePermissions(InternalRoutines.PROJECT_ROLE_TESTER);
			//ProjectRole
			Assert.IsNotNull(projectRole);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, projectRole.ProjectRoleId);
			Assert.AreEqual("Tester", projectRole.Name);
			Assert.AreEqual("Can see all product artifacts (except tasks). Can create new and modify your own risks and tests. Can create and modify all / bulk edit documents, incidents, and automation hosts", projectRole.Description);
			Assert.AreEqual(true, projectRole.IsActive);
			Assert.AreEqual(false, projectRole.IsAdmin);
			//ProjectRolePermission
			Assert.AreEqual(24, projectRole.RolePermissions.Count);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, projectRole.RolePermissions[0].ProjectRoleId);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, projectRole.RolePermissions[0].ArtifactTypeId);
			Assert.AreEqual((int)Project.PermissionEnum.View, projectRole.RolePermissions[0].PermissionId);

			//Lets test that we can retrieve all the project roles and their associated permissions
			List<ProjectRole> projectRoles = projectManager.RetrieveRolePermissions();
			//ProjectRole
			Assert.AreEqual(6, projectRoles.Count);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, projectRoles[3].ProjectRoleId);
			Assert.AreEqual("Tester", projectRoles[3].Name);
			Assert.AreEqual("Can see all product artifacts (except tasks). Can create new and modify your own risks and tests. Can create and modify all / bulk edit documents, incidents, and automation hosts", projectRoles[3].Description);
			Assert.AreEqual(true, projectRoles[3].IsActive);
			Assert.AreEqual(false, projectRoles[3].IsAdmin);
			Assert.AreEqual(false, projectRoles[3].IsLimitedView);
			//ProjectRolePermission for Project Owner
			Assert.AreEqual(61, projectRoles[0].RolePermissions.Count);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, projectRoles[0].RolePermissions[0].ProjectRoleId);
			Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, projectRoles[0].RolePermissions[0].ArtifactTypeId);
			Assert.AreEqual((int)Project.PermissionEnum.Create, projectRoles[0].RolePermissions[0].PermissionId);

			//Now lets test that we can change a role's permissions and have the changes persist

			//First lets create a new role and verify that it has no permissions
			int projectRoleId1 = projectManager.InsertProjectRole("Mangler", "Mangles things", false, true, false, false, false, false, false, USER_ID_FRED_BLOGGS,
					adminSectionId,
					"Inserted Project Role");
			projectRole = projectManager.RetrieveRolePermissions(projectRoleId1);
			Assert.AreEqual(0, projectRole.RolePermissions.Count);

			//Now add two permissions
			projectRole.StartTracking();
			projectRole.RolePermissions.Add(new ProjectRolePermission() { ProjectRoleId = projectRoleId1, ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, PermissionId = (int)Project.PermissionEnum.Create });
			projectRole.RolePermissions.Add(new ProjectRolePermission() { ProjectRoleId = projectRoleId1, ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, PermissionId = (int)Project.PermissionEnum.Modify });
			projectManager.UpdateProjectRole(new List<ProjectRole>() { projectRole }, USER_ID_FRED_BLOGGS, adminSectionId, "Updated Project Role");
			//Verify the update
			projectRole = projectManager.RetrieveRolePermissions(projectRoleId1);
			Assert.AreEqual(2, projectRole.RolePermissions.Count);

			//Now remove one of the permissions
			projectRole.StartTracking();
			ProjectRolePermission projectRolePermission = projectRole.RolePermissions.FirstOrDefault(p => p.ProjectRoleId == projectRoleId1 && p.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.TestCase && p.PermissionId == (int)Project.PermissionEnum.Create);
			projectRolePermission.MarkAsDeleted();
			projectManager.UpdateProjectRole(new List<ProjectRole>() { projectRole }, USER_ID_FRED_BLOGGS, adminSectionId, "Updated Project Role");
			//Verify the update
			projectRole = projectManager.RetrieveRolePermissions(projectRoleId1);
			Assert.AreEqual(1, projectRole.RolePermissions.Count);

			//Finally lets delete the role, it should delete the associated permission
			projectManager.DeleteProjectRole(projectRoleId1, USER_ID_FRED_BLOGGS);
		}

		[
		Test,
		SpiraTestCase(267)
		]
		public void _10_CreateProjectFromExisting()
		{
			//First lets create a new project based on the sample project, using its own new template
			projectId = projectManager.CreateFromExisting("New Library System", null, null, PROJECT_ID, true, true);

			//Now lets verify the data created

			//First the project itself
			Project project = projectManager.RetrieveById(projectId);
			Assert.AreEqual("New Library System", project.Name);
			Assert.AreEqual(true, project.IsActive);
			int templateId = project.ProjectTemplateId;

			//Next, the project membership
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(projectId);
			Assert.AreEqual(12, projectUsers.Count);

			#region Custom Properties

			//Next, the custom properties
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> requirementCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(templateId, Artifact.ArtifactTypeEnum.Requirement, false, false);
			Assert.AreEqual(7, requirementCustomProperties.Count);
			List<CustomProperty> testCaseCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(templateId, Artifact.ArtifactTypeEnum.TestCase, false, false);
			Assert.AreEqual(3, testCaseCustomProperties.Count);
			List<CustomProperty> releaseCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(templateId, Artifact.ArtifactTypeEnum.Release, false, false);
			Assert.AreEqual(2, releaseCustomProperties.Count);
			List<CustomProperty> incidentCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(templateId, Artifact.ArtifactTypeEnum.Incident, false, false);
			Assert.AreEqual(9, incidentCustomProperties.Count);
			List<CustomProperty> testRunCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(templateId, Artifact.ArtifactTypeEnum.TestRun, false, false);
			Assert.AreEqual(3, testRunCustomProperties.Count);

			#endregion

			#region Incident Fields

			//Next the incident types, statuses, priorities and severities
			IncidentManager incidentManager = new IncidentManager();
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(templateId, false);
			Assert.AreEqual(8, incidentTypes.Count);
			List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(templateId, false);
			Assert.AreEqual(8, incidentStati.Count);
			List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(templateId, false);
			Assert.AreEqual(4, incidentPriorities.Count);
			List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(templateId, false);
			Assert.AreEqual(4, incidentSeverities.Count);

			//Next, the workflows
			Business.WorkflowManager workflowManager = new Business.WorkflowManager();
			List<Workflow> oldProjectWorkflows = workflowManager.Workflow_Retrieve(PROJECT_TEMPLATE_ID, false);
			List<Workflow> newProjectWorkflows = workflowManager.Workflow_Retrieve(templateId, false);
			Assert.AreEqual(oldProjectWorkflows.Count, newProjectWorkflows.Count);
			Assert.AreEqual(oldProjectWorkflows[0].Name, newProjectWorkflows[0].Name);
			Assert.AreEqual(oldProjectWorkflows[0].IsActive, newProjectWorkflows[0].IsActive);
			Assert.AreEqual(oldProjectWorkflows[0].IsDefault, newProjectWorkflows[0].IsDefault);
			Assert.AreEqual(oldProjectWorkflows[0].IsNotify, newProjectWorkflows[0].IsNotify);

			//Next the workflow transitions
			Workflow oldProjectWorkflow = workflowManager.Workflow_RetrieveById(oldProjectWorkflows[0].WorkflowId, true, true);
			Workflow newProjectWorkflow = workflowManager.Workflow_RetrieveById(newProjectWorkflows[0].WorkflowId, true, true);
			Assert.AreEqual(oldProjectWorkflow.Transitions.Count, newProjectWorkflow.Transitions.Count);

			//Next the workflow field states
			Assert.AreEqual(oldProjectWorkflow.Fields.Count, newProjectWorkflow.Fields.Count);
			foreach (WorkflowField workflowField in newProjectWorkflow.Fields)
			{
				//Make sure that all of the incident status are from the new project
				Assert.IsNotNull(incidentStati.FirstOrDefault(s => s.IncidentStatusId == workflowField.IncidentStatusId));
			}

			//Next the workflow custom property states
			Assert.AreEqual(oldProjectWorkflow.CustomProperties.Count, newProjectWorkflow.CustomProperties.Count);
			foreach (WorkflowCustomProperty workflowCustomProperty in newProjectWorkflow.CustomProperties)
			{
				//Make sure that all of the incident status are from the new project
				Assert.IsNotNull(incidentStati.FirstOrDefault(s => s.IncidentStatusId == workflowCustomProperty.IncidentStatusId));

				//Make sure that all of the incident custom properties are from the new project
				Assert.IsTrue(incidentCustomProperties.Any(c => c.CustomPropertyId == workflowCustomProperty.CustomPropertyId));
			}

			#endregion

			#region Requirement Fields

			RequirementManager requirementManager = new RequirementManager();
			RequirementWorkflowManager requirementWorkflowManager = new RequirementWorkflowManager();

			List<RequirementType> requirementTypes = requirementManager.RequirementType_Retrieve(templateId, true);
			Assert.AreEqual(8, requirementTypes.Count);
			requirementTypes = requirementManager.RequirementType_Retrieve(templateId, false);
			Assert.AreEqual(7, requirementTypes.Count);
			List<Importance> requirementPriorities = requirementManager.RequirementImportance_Retrieve(templateId);
			Assert.AreEqual(4, requirementPriorities.Count);
			List<RequirementWorkflow> requirementWorkflows = requirementWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, requirementWorkflows.Count);

			#endregion

			#region Task Fields

			TaskManager taskManager = new TaskManager();
			TaskWorkflowManager taskWorkflowManager = new TaskWorkflowManager();

			List<TaskType> taskTypes = taskManager.TaskType_Retrieve(templateId);
			Assert.AreEqual(7, taskTypes.Count);
			List<TaskPriority> taskPriorities = taskManager.TaskPriority_Retrieve(templateId);
			Assert.AreEqual(4, taskPriorities.Count);
			List<TaskWorkflow> taskWorkflows = taskWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, taskWorkflows.Count);

			#endregion

			#region Test Case Fields

			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();

			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(templateId);
			Assert.AreEqual(12, testCaseTypes.Count);
			List<TestCasePriority> testCasePriorities = testCaseManager.TestCasePriority_Retrieve(templateId);
			Assert.AreEqual(4, testCasePriorities.Count);
			List<TestCaseWorkflow> testCaseWorkflows = testCaseWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, testCaseWorkflows.Count);

			#endregion

			#region Release Workflows

			ReleaseWorkflowManager releaseWorkflowManager = new ReleaseWorkflowManager();

			List<ReleaseWorkflow> releaseWorkflows = releaseWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, releaseWorkflows.Count);

			#endregion

			#region Risk Fields

			RiskManager riskManager = new RiskManager();
			RiskWorkflowManager riskWorkflowManager = new RiskWorkflowManager();

			List<RiskType> riskTypes = riskManager.RiskType_Retrieve(templateId);
			Assert.AreEqual(5, riskTypes.Count);
			List<RiskStatus> riskStatuses = riskManager.RiskStatus_Retrieve(templateId);
			Assert.AreEqual(6, riskStatuses.Count);
			List<RiskProbability> riskProbabilities = riskManager.RiskProbability_Retrieve(templateId);
			Assert.AreEqual(5, riskProbabilities.Count);
			List<RiskImpact> riskImpacts = riskManager.RiskImpact_Retrieve(templateId);
			Assert.AreEqual(5, riskImpacts.Count);
			List<RiskWorkflow> riskWorkflows = riskWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, riskWorkflows.Count);

			#endregion

			#region Document Fields

			AttachmentManager attachmentManager = new AttachmentManager();
			DocumentWorkflowManager documentWorkflowManager = new DocumentWorkflowManager();

			List<DocumentType> documentTypes = attachmentManager.RetrieveDocumentTypes(templateId, true);
			Assert.AreEqual(6, documentTypes.Count);
			List<DocumentStatus> documentStatuses = attachmentManager.DocumentStatus_Retrieve(templateId);
			Assert.AreEqual(7, documentStatuses.Count);
			List<DocumentWorkflow> documentWorkflows = documentWorkflowManager.Workflow_Retrieve(templateId);
			Assert.AreEqual(1, documentWorkflows.Count);

			#endregion

			//Finally clean up by deleting the new project and its template
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);
		}

		[
		Test,
		SpiraTestCase(266)
		]
		public void _11_CopyProject()
		{
			#region Copy using Same Template

			{

				#region setup
				//First make changes to the base project so that all artifacts have all parts setup for full testing
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Release, USER_ID_FRED_BLOGGS);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.TestSet, USER_ID_FRED_BLOGGS);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 12, ArtifactTypeEnum.TestRun, USER_ID_FRED_BLOGGS);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Task, USER_ID_FRED_BLOGGS);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Risk, USER_ID_FRED_BLOGGS);

				DiscussionManager discussionManager = new DiscussionManager();
				int sourceDocumentDiscussionId = discussionManager.Insert(USER_ID_FRED_BLOGGS, 2, ArtifactTypeEnum.Document, "Test message from Fred", PROJECT_ID, false, false);

				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				int sourceTestCaseArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.TestCase, 2, ArtifactTypeEnum.Task, 43, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);
				int sourceTestStepArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.TestStep, 2, ArtifactTypeEnum.Requirement, 4, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);
				int sourceRiskArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.Risk, 1, ArtifactTypeEnum.Risk, 2, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);

				IncidentManager incidentManager = new IncidentManager();
				List<int> incidentIds = new List<int> { 1 };
				incidentManager.Incident_AssociateToTestRunStep(PROJECT_ID, 17, incidentIds, USER_ID_FRED_BLOGGS);
				int sourceIncidentDiscussionId = incidentManager.InsertResolution(6, "Test resolution from fred", DateTime.UtcNow, USER_ID_FRED_BLOGGS, false);

				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				//Now make a copy of the sample project and use the same template
				projectId = projectManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, false, null, adminSectionId, "Project Cloned");
				//Refresh cache so that all data should match up with original project after the clone
				projectManager.RefreshTestStatusAndTaskProgressCache(projectId, false, null);

				#endregion

				//Now lets verify the data created

				//First the project itself
				Project project = projectManager.RetrieveById(projectId);
				Assert.AreEqual("Library Information System (Sample) - Copy", project.Name);
				Assert.AreEqual(true, project.IsActive);

				//Next the component count
				List<Component> components = new ComponentManager().Component_Retrieve(projectId);
				Assert.AreEqual(3, components.Count);

				#region Requirements
				//Verify the count
				Business.RequirementManager requirementManager = new Business.RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
				List<RequirementView> sourceRequirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, Int32.MaxValue, null, 0);
				Assert.AreEqual(sourceRequirements.Count, requirements.Count);

				//Verify that the requirements are attached to the new components
				RequirementView requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.IsTrue(components.Any(c => c.ComponentId == requirement.ComponentId.Value));

				//Verify that a use case has its steps copied over as well
				requirement = requirements.FirstOrDefault(r => r.Name == "Creating a new book in the system");
				RequirementView sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Creating a new book in the system");
				Assert.AreEqual(requirementManager.CountSteps(sourceRequirement.RequirementId), requirementManager.CountSteps(requirement.RequirementId));

				//Verify standard fields
				requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.AreEqual(sourceRequirement.ReleaseVersionNumber, requirement.ReleaseVersionNumber);
				Assert.AreEqual(sourceRequirement.ImportanceName, requirement.ImportanceName);
				Assert.AreEqual(sourceRequirement.ImportanceId, requirement.ImportanceId);

				//Verify the status - use a main parent RQ to get good data
				requirement = requirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				Assert.AreEqual(sourceRequirement.RequirementStatusName, requirement.RequirementStatusName);
				Assert.AreEqual(sourceRequirement.RequirementStatusId, requirement.RequirementStatusId);
				Assert.AreEqual(sourceRequirement.CoverageCountBlocked, requirement.CoverageCountBlocked);
				Assert.AreEqual(sourceRequirement.CoverageCountCaution, requirement.CoverageCountCaution);
				Assert.AreEqual(sourceRequirement.CoverageCountFailed, requirement.CoverageCountFailed);
				Assert.AreEqual(sourceRequirement.CoverageCountPassed, requirement.CoverageCountPassed);
				Assert.AreEqual(sourceRequirement.CoverageCountTotal, requirement.CoverageCountTotal);
				Assert.AreEqual(sourceRequirement.TaskCount, requirement.TaskCount);
				Assert.AreEqual(sourceRequirement.TaskPercentLateFinish, requirement.TaskPercentLateFinish);
				Assert.AreEqual(sourceRequirement.TaskPercentOnTime, requirement.TaskPercentOnTime);

				//Verify custom properties
				requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.AreEqual(Int32.Parse(sourceRequirement.Custom_06), Int32.Parse(requirement.Custom_06));

				//Verify comments
				IEnumerable<IDiscussion> sourceDiscussion = discussionManager.Retrieve(sourceRequirement.RequirementId, ArtifactTypeEnum.Requirement);
				IEnumerable<IDiscussion> discussion = discussionManager.Retrieve(requirement.RequirementId, ArtifactTypeEnum.Requirement);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				List<ProjectAttachmentView> sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRequirement.RequirementId, ArtifactTypeEnum.Requirement, "AttachmentId", true, 1, 10, null, 0);
				List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirement.RequirementId, ArtifactTypeEnum.Requirement, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify associations
				List<ArtifactLinkView> sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Requirement, sourceRequirement.RequirementId, "CreationDate");
				List<ArtifactLinkView> associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Requirement, requirement.RequirementId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify test coverage
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCase> sourceRequirementTestCases = testCaseManager.RetrieveCoveredByRequirementId(PROJECT_ID, sourceRequirement.RequirementId);
				List<TestCase> requirementTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirement.RequirementId);
				Assert.AreEqual(sourceRequirementTestCases.Count, requirementTestCases.Count);
				Assert.AreEqual(sourceRequirementTestCases[0].Name, requirementTestCases[0].Name);
				Assert.AreEqual(sourceRequirementTestCases[0].ExecutionStatusId, requirementTestCases[0].ExecutionStatusId);

				#endregion

				#region releases
				//Verify the count
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> sourceReleases = releaseManager.RetrieveByProjectId(PROJECT_ID, false);
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
				Assert.AreEqual(sourceReleases.Count, releases.Count);

				//Verify standard fields
				ReleaseView sourceRelease = sourceReleases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				ReleaseView release = releases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				Assert.AreEqual(sourceRelease.Name, release.Name);
				Assert.AreEqual(sourceRelease.StartDate, release.StartDate);
				Assert.AreEqual(sourceRelease.ReleaseTypeId, release.ReleaseTypeId);

				//Verify the status
				Assert.AreEqual(sourceRelease.ReleaseStatusName, release.ReleaseStatusName);
				Assert.AreEqual(sourceRelease.ReleaseStatusId, release.ReleaseStatusId);
				Assert.AreEqual(sourceRelease.CountBlocked, release.CountBlocked);
				Assert.AreEqual(sourceRelease.CountCaution, release.CountCaution);
				Assert.AreEqual(sourceRelease.CountFailed, release.CountFailed);
				Assert.AreEqual(sourceRelease.RequirementCount, release.RequirementCount);
				Assert.AreEqual(sourceRelease.PercentComplete, release.PercentComplete);
				Assert.AreEqual(sourceRelease.TaskCount, release.TaskCount);
				Assert.AreEqual(sourceRelease.TaskPercentOnTime, release.TaskPercentOnTime);

				//Verify custom properties
				Assert.AreEqual(sourceRelease.Custom_01, release.Custom_01);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceRelease.ReleaseId, ArtifactTypeEnum.Release);
				discussion = discussionManager.Retrieve(release.ReleaseId, ArtifactTypeEnum.Release);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRelease.ReleaseId, ArtifactTypeEnum.Release, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, release.ReleaseId, ArtifactTypeEnum.Release, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count, "should be 1");
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify owner fields correctly copy
				sourceRelease = sourceReleases.FirstOrDefault(r => r.VersionNumber == "1.1.0.0.0002");
				release = releases.FirstOrDefault(r => r.VersionNumber == "1.1.0.0.0002");
				Assert.AreEqual(sourceRelease.OwnerId, release.OwnerId);

				//Verify test coverage
				List<TestCaseReleaseView> sourceReleaseTestCases = testCaseManager.RetrieveMappedByReleaseId2(PROJECT_ID, sourceRelease.ReleaseId);
				List<TestCaseReleaseView> releaseTestCases = testCaseManager.RetrieveMappedByReleaseId2(projectId, release.ReleaseId);
				TestCaseReleaseView sourceReleaseTestCase = sourceReleaseTestCases.FirstOrDefault(r => r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				TestCaseReleaseView releaseTestCase = releaseTestCases.FirstOrDefault(r => r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				// we don't do count as they won't match with sample data 
				Assert.AreEqual(sourceReleaseTestCase.Name, releaseTestCase.Name);
				Assert.AreEqual(sourceReleaseTestCase.ExecutionStatusId, releaseTestCase.ExecutionStatusId);

				#endregion

				#region Documents
				//Verify folders
				List<ProjectAttachmentFolderHierarchy> sourceDocumentFolders = attachmentManager.RetrieveFoldersByProjectId(PROJECT_ID);
				List<ProjectAttachmentFolderHierarchy> documentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
				Assert.AreEqual(sourceDocumentFolders.Count, documentFolders.Count);
				Assert.AreEqual(sourceDocumentFolders[0].Name, documentFolders[0].Name);
				Assert.AreEqual(sourceDocumentFolders[0].HierarchyLevel, documentFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceDocumentFolders[1].Name, documentFolders[1].Name);
				Assert.AreEqual(sourceDocumentFolders[1].HierarchyLevel, documentFolders[1].HierarchyLevel);

				//Verify count
				List<ProjectAttachmentView> sourceDocuments = attachmentManager.RetrieveForProject(PROJECT_ID, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<ProjectAttachmentView> documents = attachmentManager.RetrieveForProject(projectId, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceDocuments.Count, documents.Count);

				//Verify standard fields
				ProjectAttachmentView sourceDocument = sourceDocuments.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				ProjectAttachmentView document = documents.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				Assert.AreEqual(sourceDocument.AuthorId, document.AuthorId);
				Assert.AreEqual(sourceDocument.Tags.First(), document.Tags.First());
				Assert.AreEqual(sourceDocument.UploadDate, document.UploadDate);
				Assert.AreEqual(sourceDocument.AttachmentTypeName, document.AttachmentTypeName);
				Assert.AreEqual(sourceDocument.AttachmentTypeId, document.AttachmentTypeId);

				//Verify the status
				Assert.AreEqual(sourceDocument.DocumentStatusName, document.DocumentStatusName);
				Assert.AreEqual(sourceDocument.DocumentStatusId, document.DocumentStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceDocument.Custom_02, document.Custom_02);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceDocument.AttachmentId, ArtifactTypeEnum.Document);
				discussion = discussionManager.Retrieve(document.AttachmentId, ArtifactTypeEnum.Document);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);

				//Verify associations
				sourceDocument = sourceDocuments.FirstOrDefault(r => r.Filename == "Expected Result Screenshot.png");
				document = documents.FirstOrDefault(r => r.Filename == "Expected Result Screenshot.png");
				List<ArtifactAttachmentView> sourceAttachmentAssociations = artifactLinkManager.RetrieveByAttachmentId(PROJECT_ID, sourceDocument.AttachmentId);
				List<ArtifactAttachmentView> attachmentAssociations = artifactLinkManager.RetrieveByAttachmentId(projectId, document.AttachmentId);
				Assert.AreEqual(sourceAttachmentAssociations.Count, attachmentAssociations.Count);
				Assert.AreEqual(sourceAttachmentAssociations[0].CreationDate, attachmentAssociations[0].CreationDate);
				Assert.AreEqual(sourceAttachmentAssociations[0].Comment, attachmentAssociations[0].Comment);


				#endregion

				#region Test Cases
				//Verify folders
				List<TestCaseFolderHierarchyView> sourceTestCaseFolders = testCaseManager.TestCaseFolder_GetList(PROJECT_ID);
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				Assert.AreEqual(sourceTestCaseFolders.Count, testCaseFolders.Count);
				Assert.AreEqual(sourceTestCaseFolders[0].Name, testCaseFolders[0].Name);
				Assert.AreEqual(sourceTestCaseFolders[0].HierarchyLevel, testCaseFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTestCaseFolders[1].Name, testCaseFolders[1].Name);
				Assert.AreEqual(sourceTestCaseFolders[1].HierarchyLevel, testCaseFolders[1].HierarchyLevel);

				//Verify count
				List<TestCaseView> sourceTestCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				Assert.AreEqual(sourceTestCases.Count, testCases.Count);

				//Verify component
				TestCaseView sourceTestCase = sourceTestCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				TestCaseView testCase = testCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				List<int> artifactComponents = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
				Assert.IsTrue(components.Any(c => c.ComponentId == artifactComponents.First()));

				//Verify test steps
				List<TestStepView> sourceTestSteps = testCaseManager.RetrieveStepsForTestCase(sourceTestCase.TestCaseId);
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCase.TestCaseId);
				Assert.AreEqual(sourceTestSteps.Count, testSteps.Count);
				Assert.AreEqual(sourceTestSteps[1].ExecutionStatusName, testSteps[1].ExecutionStatusName);
				Assert.AreEqual(sourceTestSteps[1].Description, testSteps[1].Description);
				Assert.AreEqual(sourceTestSteps[1].Position, testSteps[1].Position);
				Assert.AreEqual(sourceTestSteps[2].ExecutionStatusName, testSteps[2].ExecutionStatusName);
				Assert.AreEqual(sourceTestSteps[2].Description, testSteps[2].Description);
				Assert.AreEqual(sourceTestSteps[2].Position, testSteps[2].Position);

				//Verify test step custom properties
				Assert.AreEqual(sourceTestSteps[2].Custom_01, testSteps[2].Custom_01);

				//Verify test step associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestStep, sourceTestSteps[1].TestStepId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestStep, testSteps[1].TestStepId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify test step attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestSteps[2].TestStepId, ArtifactTypeEnum.TestStep, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testSteps[2].TestStepId, ArtifactTypeEnum.TestStep, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify standard fields
				Assert.AreEqual(sourceTestCase.TestCasePriorityName, testCase.TestCasePriorityName);
				Assert.AreEqual(sourceTestCase.TestCasePriorityId, testCase.TestCasePriorityId);
				Assert.AreEqual(sourceTestCase.TestCaseTypeName, testCase.TestCaseTypeName);
				Assert.AreEqual(sourceTestCase.TestCaseTypeId, testCase.TestCaseTypeId);
				Assert.AreEqual(sourceTestCase.Description, testCase.Description);
				Assert.AreEqual(sourceTestCase.AuthorId, testCase.AuthorId);

				//Verify the status
				Assert.AreEqual(sourceTestCase.ExecutionStatusName, testCase.ExecutionStatusName);
				Assert.AreEqual(sourceTestCase.ExecutionStatusId, testCase.ExecutionStatusId);
				Assert.AreEqual(sourceTestCase.TestCaseStatusName, testCase.TestCaseStatusName);
				Assert.AreEqual(sourceTestCase.TestCaseStatusId, testCase.TestCaseStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTestCase.Custom_01, testCase.Custom_01);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTestCase.TestCaseId, ArtifactTypeEnum.TestCase);
				discussion = discussionManager.Retrieve(testCase.TestCaseId, ArtifactTypeEnum.TestCase);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestCase, sourceTestCase.TestCaseId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestCase, testCase.TestCaseId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestCase.TestCaseId, ArtifactTypeEnum.TestCase, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testCase.TestCaseId, ArtifactTypeEnum.TestCase, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				#endregion

				#region Test Sets
				TestSetManager testSetManager = new TestSetManager();

				//Verify folders
				List<TestSetFolderHierarchyView> sourceTestSetFolders = testSetManager.TestSetFolder_GetList(PROJECT_ID);
				List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
				Assert.AreEqual(sourceTestSetFolders.Count, testSetFolders.Count);
				Assert.AreEqual(sourceTestSetFolders[0].Name, testSetFolders[0].Name);
				Assert.AreEqual(sourceTestSetFolders[0].HierarchyLevel, testSetFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTestSetFolders[1].Name, testSetFolders[1].Name);
				Assert.AreEqual(sourceTestSetFolders[1].HierarchyLevel, testSetFolders[1].HierarchyLevel);

				//Verify count
				List<TestSetView> sourceTestSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				Assert.AreEqual(sourceTestSets.Count, testSets.Count);

				//Verify standard fields
				TestSetView sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				TestSetView testSet = testSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				Assert.AreEqual(sourceTestSet.ReleaseVersionNumber, testSet.ReleaseVersionNumber);
				Assert.AreEqual(sourceTestSet.Description, testSet.Description);
				Assert.AreEqual(sourceTestSet.OwnerId, testSet.OwnerId);
				Assert.AreEqual(sourceTestSet.TestRunTypeId, testSet.TestRunTypeId);

				//Verify the status
				Assert.AreEqual(sourceTestSet.TestSetStatusName, testSet.TestSetStatusName);
				Assert.AreEqual(sourceTestSet.TestSetStatusId, testSet.TestSetStatusId);
				Assert.AreEqual(sourceTestSet.CountPassed, testSet.CountPassed);
				Assert.AreEqual(sourceTestSet.CountBlocked, testSet.CountBlocked);
				Assert.AreEqual(sourceTestSet.CountCaution, testSet.CountCaution);
				Assert.AreEqual(sourceTestSet.CountFailed, testSet.CountFailed);
				Assert.AreEqual(sourceTestSet.CountNotApplicable, testSet.CountNotApplicable);
				Assert.AreEqual(sourceTestSet.CountNotRun, testSet.CountNotRun);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTestSet.TestSetId, ArtifactTypeEnum.TestSet);
				discussion = discussionManager.Retrieve(testSet.TestSetId, ArtifactTypeEnum.TestSet);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestSet.TestSetId, ArtifactTypeEnum.TestSet, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testSet.TestSetId, ArtifactTypeEnum.TestSet, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count, "should be 1");
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify custom properties
				sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				testSet = testSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				Assert.AreEqual(sourceTestSet.Custom_01, testSet.Custom_01);

				#endregion

				#region Test Runs
				//Verify count
				TestRunManager testRunManager = new TestRunManager();
				List<TestRunView> sourceTestRuns = testRunManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceTestRuns.Count, testRuns.Count);

				//Verify test run steps
				TestRunView sourceTestRun = sourceTestRuns.FirstOrDefault(r => r.Name == "Ability to edit existing book" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Caution);
				TestRunView testRun = testRuns.FirstOrDefault(r => r.Name == "Ability to edit existing book" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Caution);

				List<TestRunStep> sourceTestRunSteps = testRunManager.RetrieveByIdWithSteps(sourceTestRun.TestRunId).TestRunSteps.ToList();
				List<TestRunStep> testRunSteps = testRunManager.RetrieveByIdWithSteps(testRun.TestRunId).TestRunSteps.ToList();
				Assert.AreEqual(sourceTestRunSteps.Count, testRunSteps.Count);
				Assert.AreEqual(sourceTestRunSteps[1].ExecutionStatusId, testRunSteps[1].ExecutionStatusId);
				Assert.AreEqual(sourceTestRunSteps[1].Description, testRunSteps[1].Description);
				Assert.AreEqual(sourceTestRunSteps[1].Position, testRunSteps[1].Position);
				Assert.AreEqual(sourceTestRunSteps[1].ActualResult, testRunSteps[1].ActualResult);
				Assert.AreEqual(sourceTestRunSteps[2].ExecutionStatusId, testRunSteps[2].ExecutionStatusId);
				Assert.AreEqual(sourceTestRunSteps[2].Description, testRunSteps[2].Description);
				Assert.AreEqual(sourceTestRunSteps[2].Position, testRunSteps[2].Position);
				Assert.AreEqual(sourceTestRunSteps[2].ActualResult, testRunSteps[2].ActualResult);

				//Verify test run step associations
				List<IncidentView> sourceTestRunincidents = incidentManager.RetrieveByTestRunStepId(sourceTestRunSteps[1].TestRunStepId);
				List<IncidentView> testRunincidents = incidentManager.RetrieveByTestRunStepId(testRunSteps[1].TestRunStepId);
				Assert.AreEqual(sourceTestRunincidents.Count, testRunincidents.Count);
				Assert.AreEqual(sourceTestRunincidents[0].Name, testRunincidents[0].Name);
				Assert.AreEqual(sourceTestRunincidents[0].IncidentTypeName, testRunincidents[0].IncidentTypeName);

				//Verify standard fields
				Assert.AreEqual(sourceTestRun.TesterId, testRun.TesterId);
				Assert.AreEqual(sourceTestRun.ReleaseVersionNumber, testRun.ReleaseVersionNumber);
				Assert.AreEqual(sourceTestRun.TestRunTypeName, testRun.TestRunTypeName);
				Assert.AreEqual(sourceTestRun.EndDate, testRun.EndDate);
				Assert.AreEqual(sourceTestRun.StartDate, testRun.StartDate);

				//Verify the status
				Assert.AreEqual(sourceTestRun.ExecutionStatusName, testRun.ExecutionStatusName);
				Assert.AreEqual(sourceTestRun.ExecutionStatusId, testRun.ExecutionStatusId);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestRun.TestRunId, ArtifactTypeEnum.TestRun, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testRun.TestRunId, ArtifactTypeEnum.TestRun, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify custom properties
				sourceTestRun = sourceTestRuns.FirstOrDefault(r => r.Name == "Ability to edit existing author" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				testRun = testRuns.FirstOrDefault(r => r.Name == "Ability to edit existing author" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				Assert.AreEqual(sourceTestRun.Custom_03, testRun.Custom_03);

				#endregion

				#region Automation Hosts
				//Verify the count
				AutomationManager automationManager = new AutomationManager();
				List<AutomationHostView> sourceAutomationHosts = automationManager.RetrieveHosts(PROJECT_ID);
				List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
				Assert.AreEqual(sourceAutomationHosts.Count, automationHosts.Count);

				//Verify standard fields
				AutomationHostView sourceAutomationHost = sourceAutomationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				AutomationHostView automationHost = automationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				Assert.AreEqual(sourceAutomationHost.IsActive, automationHost.IsActive);
				Assert.AreEqual(sourceAutomationHost.Description, automationHost.Description);
				Assert.AreEqual(sourceAutomationHost.Token, automationHost.Token);

				//Verify custom properties
				Assert.IsTrue(!String.IsNullOrEmpty(automationHost.Custom_01), "check that there is any value set at all");

				#endregion

				#region Test Configurations
				//Verify the count
				TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
				List<TestConfigurationSet> sourceTestConfigurations = testConfigurationManager.RetrieveSets(PROJECT_ID);
				List<TestConfigurationSet> testConfigurations = testConfigurationManager.RetrieveSets(projectId);
				Assert.AreEqual(sourceTestConfigurations.Count, testConfigurations.Count);

				//Verify standard fields
				TestConfigurationSet sourceTestConfiguration = sourceTestConfigurations.FirstOrDefault(r => r.Name == "Target web browsers and operating systems");
				TestConfigurationSet testConfiguration = testConfigurations.FirstOrDefault(r => r.Name == "Target web browsers and operating systems");
				Assert.AreEqual(sourceTestConfiguration.IsActive, testConfiguration.IsActive);
				Assert.AreEqual(sourceTestConfiguration.Description, testConfiguration.Description);

				#endregion

				#region Incidents
				//Verify count
				List<IncidentView> sourceIncidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceIncidents.Count, incidents.Count);

				//Verify standard fields
				IncidentView sourceIncident = sourceIncidents.FirstOrDefault(r => r.Name == "The book listing screen doesn't sort");
				IncidentView incident = incidents.FirstOrDefault(r => r.Name == "The book listing screen doesn't sort");
				Assert.AreEqual(sourceIncident.OwnerId, incident.OwnerId);
				Assert.AreEqual(sourceIncident.Description, incident.Description);
				Assert.AreEqual(sourceIncident.DetectedReleaseVersionNumber, incident.DetectedReleaseVersionNumber);

				//Verify the status
				Assert.AreEqual(sourceIncident.IncidentStatusName, incident.IncidentStatusName);
				Assert.AreEqual(sourceIncident.IncidentStatusId, incident.IncidentStatusId);
				Assert.AreEqual(sourceIncident.IncidentTypeName, incident.IncidentTypeName);
				Assert.AreEqual(sourceIncident.IncidentTypeId, incident.IncidentTypeId);
				Assert.AreEqual(sourceIncident.PriorityName, incident.PriorityName);

				//Verify custom properties
				Assert.AreEqual(sourceIncident.Custom_01, incident.Custom_01);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Incident, sourceIncident.IncidentId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Incident, incident.IncidentId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify comments
				List<IncidentResolution> sourceResolutions = incidentManager.RetrieveById(sourceIncident.IncidentId, true).Resolutions.ToList();
				List<IncidentResolution> resolutions = incidentManager.RetrieveById(incident.IncidentId, true).Resolutions.ToList();
				Assert.AreEqual(sourceResolutions.Count, resolutions.Count);
				Assert.AreEqual(sourceResolutions[0].Resolution, resolutions[0].Resolution);
				Assert.AreEqual(sourceResolutions[0].CreatorId, resolutions[0].CreatorId);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceIncident.IncidentId, ArtifactTypeEnum.Incident, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, incident.IncidentId, ArtifactTypeEnum.Incident, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify components
				artifactComponents = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
				Assert.IsTrue(components.Any(c => c.ComponentId == artifactComponents.First()));

				#endregion

				#region Tasks
				TaskManager taskManager = new TaskManager();

				//Verify Folders
				List<TaskFolderHierarchyView> sourceTaskFolders = taskManager.TaskFolder_GetList(PROJECT_ID);
				List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(projectId);
				Assert.AreEqual(sourceTaskFolders.Count, taskFolders.Count);
				Assert.AreEqual(sourceTaskFolders[0].Name, taskFolders[0].Name);
				Assert.AreEqual(sourceTaskFolders[0].HierarchyLevel, taskFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTaskFolders[1].Name, taskFolders[1].Name);
				Assert.AreEqual(sourceTaskFolders[1].HierarchyLevel, taskFolders[1].HierarchyLevel);

				//Verify count
				List<TaskView> sourceTasks = taskManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<TaskView> tasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceTasks.Count, tasks.Count);

				//Verify standard fields
				TaskView sourceTask = sourceTasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				TaskView task = tasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				Assert.AreEqual(sourceTask.OwnerId, task.OwnerId);
				Assert.AreEqual(sourceTask.TaskPriorityName, task.TaskPriorityName);
				Assert.AreEqual(sourceTask.EstimatedEffort, task.EstimatedEffort);
				Assert.AreEqual(sourceTask.TaskTypeName, task.TaskTypeName);

				//Verify the status
				Assert.AreEqual(sourceTask.TaskStatusName, task.TaskStatusName);
				Assert.AreEqual(sourceTask.TaskStatusId, task.TaskStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTask.Custom_02, task.Custom_02);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Task, sourceTask.TaskId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Task, task.TaskId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTask.TaskId, ArtifactTypeEnum.Task);
				discussion = discussionManager.Retrieve(task.TaskId, ArtifactTypeEnum.Task);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTask.TaskId, ArtifactTypeEnum.Task, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, task.TaskId, ArtifactTypeEnum.Task, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify components
				Assert.IsTrue(components.Any(c => c.ComponentId == task.ComponentId.Value));

				#endregion

				#region Risks
				//Verify count
				RiskManager riskManager = new RiskManager();
				List<RiskView> sourceRisks = riskManager.Risk_Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<RiskView> risks = riskManager.Risk_Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceRisks.Count, risks.Count);

				//Verify standard fields
				RiskView sourceRisk = sourceRisks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				RiskView risk = risks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				Assert.AreEqual(sourceRisk.OwnerId, risk.OwnerId);
				Assert.AreEqual(sourceRisk.ReleaseName, risk.ReleaseName);
				Assert.AreEqual(sourceRisk.Description, risk.Description);

				//Verify the template fields like status
				Assert.AreEqual(sourceRisk.RiskProbabilityName, risk.RiskProbabilityName);
				Assert.AreEqual(sourceRisk.RiskProbabilityId, risk.RiskProbabilityId);
				Assert.AreEqual(sourceRisk.RiskTypeName, risk.RiskTypeName);
				Assert.AreEqual(sourceRisk.RiskTypeId, risk.RiskTypeId);
				Assert.AreEqual(sourceRisk.RiskStatusName, risk.RiskStatusName);
				Assert.AreEqual(sourceRisk.RiskStatusId, risk.RiskStatusId);

				//Verify custom properties - none defined in originating template

				//Verify mitigations
				List<RiskMitigation> sourceRiskMitagtions = riskManager.RiskMitigation_Retrieve(sourceRisk.RiskId);
				List<RiskMitigation> riskMitagtions = riskManager.RiskMitigation_Retrieve(risk.RiskId);
				Assert.AreEqual(sourceRiskMitagtions.Count, riskMitagtions.Count);
				Assert.AreEqual(sourceRiskMitagtions[0].ReviewDate, riskMitagtions[0].ReviewDate);
				Assert.AreEqual(sourceRiskMitagtions[0].Description, riskMitagtions[0].Description);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Risk, sourceRisk.RiskId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Risk, risk.RiskId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].ArtifactStatusName, associations[0].ArtifactStatusName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceRisk.RiskId, ArtifactTypeEnum.Risk);
				discussion = discussionManager.Retrieve(risk.RiskId, ArtifactTypeEnum.Risk);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRisk.RiskId, ArtifactTypeEnum.Risk, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, risk.RiskId, ArtifactTypeEnum.Risk, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify components
				risk = risks.FirstOrDefault(r => r.Name == "We may not get enough authors to sign up");
				Assert.IsTrue(components.Any(c => c.ComponentId == risk.ComponentId.Value));

				#endregion

				#region Cleanup
				//Finally clean up by deleting the new project
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);

				//Then reset the original project back to what it was before
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Release, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.TestSet, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 12, ArtifactTypeEnum.TestRun, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Task, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Risk, 1);

				discussionManager.DeleteDiscussionId(sourceDocumentDiscussionId, ArtifactTypeEnum.Document, true, 1, 1);
				incidentManager.Resolution_Delete(PROJECT_ID, 6, sourceIncidentDiscussionId, USER_ID_SYS_ADMIN);
				incidentManager.Incident_AssociateToTestRunStepRemove(PROJECT_ID, 17, incidentIds, USER_ID_FRED_BLOGGS);

				artifactLinkManager.Delete(sourceTestCaseArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.TestCase, 1, 1);
				artifactLinkManager.Delete(sourceTestStepArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.TestStep, 1, 1);
				artifactLinkManager.Delete(sourceRiskArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.Risk, 1, 1);

				//NOTE can't remove the incident from the test run step as per 6.9.0
				#endregion
			}

			#endregion

			#region Copy using a new Template

			{
				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;
				//First lets make a copy of the sample project and use a unique template
				projectId = projectManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, true, null, adminSectionId, "Project Cloned");

				//Now lets verify the data created

				//First the project itself
				Project project = projectManager.RetrieveById(projectId);
				Assert.AreEqual("Library Information System (Sample) - Copy", project.Name);
				Assert.AreEqual(true, project.IsActive);
				int templateId = project.ProjectTemplateId;

				//Next the component count
				List<Component> components = new ComponentManager().Component_Retrieve(projectId);
				Assert.AreEqual(3, components.Count);

				#region Requirements
				//Verify the count
				Business.RequirementManager requirementManager = new Business.RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
				List<RequirementView> sourceRequirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, Int32.MaxValue, null, 0);
				Assert.AreEqual(sourceRequirements.Count, requirements.Count);

				//Verify the status - use a main parent RQ to get good data
				RequirementView sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				RequirementView requirement = requirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				Assert.AreEqual(sourceRequirement.RequirementStatusName, requirement.RequirementStatusName);
				Assert.AreEqual(sourceRequirement.RequirementStatusId, requirement.RequirementStatusId);

				//Verify custom properties
				requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.AreEqual(Int32.Parse(sourceRequirement.Custom_06), Int32.Parse(requirement.Custom_06));

				#endregion

				#region releases
				//Verify the count
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> sourceReleases = releaseManager.RetrieveByProjectId(PROJECT_ID, false);
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
				Assert.AreEqual(sourceReleases.Count, releases.Count);

				//Verify the status
				ReleaseView sourceRelease = sourceReleases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				ReleaseView release = releases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				Assert.AreEqual(sourceRelease.ReleaseStatusName, release.ReleaseStatusName);
				Assert.AreEqual(sourceRelease.ReleaseStatusId, release.ReleaseStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceRelease.Custom_01, release.Custom_01);

				#endregion

				#region Documents
				//Verify count
				AttachmentManager attachmentManager = new AttachmentManager();
				List<ProjectAttachmentView> sourceDocuments = attachmentManager.RetrieveForProject(PROJECT_ID, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<ProjectAttachmentView> documents = attachmentManager.RetrieveForProject(projectId, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceDocuments.Count, documents.Count);

				//Verify the status
				ProjectAttachmentView sourceDocument = sourceDocuments.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				ProjectAttachmentView document = documents.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				Assert.AreEqual(sourceDocument.DocumentStatusName, document.DocumentStatusName);
				Assert.AreNotEqual(sourceDocument.DocumentStatusId, document.DocumentStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceDocument.Custom_02, document.Custom_02);

				#endregion

				#region Test Cases
				//Verify count
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCaseView> sourceTestCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				Assert.AreEqual(sourceTestCases.Count, testCases.Count);

				//Verify component
				TestCaseView sourceTestCase = sourceTestCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				TestCaseView testCase = testCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				List<int> artifactComponents = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
				Assert.IsTrue(components.Any(c => c.ComponentId == artifactComponents.First()));

				//Verify test step custom properties
				List<TestStepView> sourceTestSteps = testCaseManager.RetrieveStepsForTestCase(sourceTestCase.TestCaseId);
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCase.TestCaseId);
				Assert.AreEqual(sourceTestSteps[2].Custom_01, testSteps[2].Custom_01);

				//Verify the status
				Assert.AreEqual(sourceTestCase.TestCaseStatusName, testCase.TestCaseStatusName);
				Assert.AreEqual(sourceTestCase.TestCaseStatusId, testCase.TestCaseStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTestCase.Custom_01, testCase.Custom_01);

				#endregion

				#region Test Sets
				TestSetManager testSetManager = new TestSetManager();

				//Verify count
				List<TestSetView> sourceTestSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				Assert.AreEqual(sourceTestSets.Count, testSets.Count);

				//Verify the status
				TestSetView sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				TestSetView testSet = testSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				Assert.AreEqual(sourceTestSet.TestSetStatusName, testSet.TestSetStatusName);
				Assert.AreEqual(sourceTestSet.TestSetStatusId, testSet.TestSetStatusId);

				//Verify custom properties
				sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				testSet = testSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				Assert.AreEqual(sourceTestCase.Custom_01, testCase.Custom_01);

				#endregion

				#region Test Runs
				//Verify count
				TestRunManager testRunManager = new TestRunManager();
				List<TestRunView> sourceTestRuns = testRunManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceTestRuns.Count, testRuns.Count);

				//Verify custom properties
				TestRunView sourceTestRun = sourceTestRuns.FirstOrDefault(r => r.Name == "Ability to edit existing author" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				TestRunView testRun = testRuns.FirstOrDefault(r => r.Name == "Ability to edit existing author" && r.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked);
				Assert.AreEqual(sourceTestRun.Custom_03, testRun.Custom_03);

				#endregion

				#region Automation Hosts
				//Verify the count
				AutomationManager automationManager = new AutomationManager();
				List<AutomationHostView> sourceAutomationHosts = automationManager.RetrieveHosts(PROJECT_ID);
				List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
				Assert.AreEqual(sourceAutomationHosts.Count, automationHosts.Count);

				//Verify custom properties
				AutomationHostView sourceAutomationHost = sourceAutomationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				AutomationHostView automationHost = automationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				Assert.IsTrue(!String.IsNullOrEmpty(automationHost.Custom_01), "check that there is any value set at all");

				#endregion

				#region Test Configurations
				//Verify the count
				TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
				List<TestConfigurationSet> sourceTestConfigurations = testConfigurationManager.RetrieveSets(PROJECT_ID);
				List<TestConfigurationSet> testConfigurations = testConfigurationManager.RetrieveSets(projectId);
				Assert.AreEqual(sourceTestConfigurations.Count, testConfigurations.Count);

				#endregion

				#region Incidents
				//Verify count
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentView> sourceIncidents = incidentManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceIncidents.Count, incidents.Count);

				//Verify the status
				IncidentView sourceIncident = sourceIncidents.FirstOrDefault(r => r.Name == "The book listing screen doesn't sort");
				IncidentView incident = incidents.FirstOrDefault(r => r.Name == "The book listing screen doesn't sort");
				Assert.AreEqual(sourceIncident.IncidentStatusName, incident.IncidentStatusName);
				Assert.AreNotEqual(sourceIncident.IncidentStatusId, incident.IncidentStatusId);
				Assert.AreEqual(sourceIncident.IncidentTypeName, incident.IncidentTypeName);
				Assert.AreNotEqual(sourceIncident.IncidentTypeId, incident.IncidentTypeId);
				Assert.AreEqual(sourceIncident.PriorityName, incident.PriorityName);

				//Verify custom properties
				Assert.AreEqual(sourceIncident.Custom_01, incident.Custom_01);

				//Verify components
				artifactComponents = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
				Assert.IsTrue(components.Any(c => c.ComponentId == artifactComponents.First()));

				#endregion

				#region Tasks
				TaskManager taskManager = new TaskManager();

				//Verify count
				List<TaskView> sourceTasks = taskManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<TaskView> tasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceTasks.Count, tasks.Count);

				//Verify the status
				TaskView sourceTask = sourceTasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				TaskView task = tasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				Assert.AreEqual(sourceTask.TaskTypeName, task.TaskTypeName);
				Assert.AreEqual(sourceTask.TaskStatusName, task.TaskStatusName);
				Assert.AreEqual(sourceTask.TaskStatusId, task.TaskStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTask.Custom_02, task.Custom_02);

				//Verify components
				Assert.IsTrue(components.Any(c => c.ComponentId == task.ComponentId.Value));

				#endregion

				#region Risks
				//Verify count
				RiskManager riskManager = new RiskManager();
				List<RiskView> sourceRisks = riskManager.Risk_Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<RiskView> risks = riskManager.Risk_Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceRisks.Count, risks.Count);

				//Verify the template fields like status
				RiskView sourceRisk = sourceRisks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				RiskView risk = risks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				Assert.AreEqual(sourceRisk.RiskProbabilityName, risk.RiskProbabilityName);
				Assert.AreNotEqual(sourceRisk.RiskProbabilityId, risk.RiskProbabilityId);
				Assert.AreEqual(sourceRisk.RiskTypeName, risk.RiskTypeName);
				Assert.AreNotEqual(sourceRisk.RiskTypeId, risk.RiskTypeId);
				Assert.AreEqual(sourceRisk.RiskStatusName, risk.RiskStatusName);
				Assert.AreNotEqual(sourceRisk.RiskStatusId, risk.RiskStatusId);

				//Verify custom properties - none defined in originating template

				//Verify components
				risk = risks.FirstOrDefault(r => r.Name == "We may not get enough authors to sign up");
				Assert.IsTrue(components.Any(c => c.ComponentId == risk.ComponentId.Value));

				#endregion

				#region Cleanup
				//Finally clean up by deleting the new project and template
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
				templateManager.Delete(USER_ID_FRED_BLOGGS, templateId);

				#endregion


			}

			#endregion

			#region Copy and Reset using Same Template

			{

				#region setup
				//First make changes to the base project so that all artifacts have all parts setup for full testing
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Release);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.TestSet);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Task);
				attachmentManager.InsertArtifactAssociation(PROJECT_ID, 19, 1, ArtifactTypeEnum.Risk);

				DiscussionManager discussionManager = new DiscussionManager();
				int sourceDocumentDiscussionId = discussionManager.Insert(USER_ID_FRED_BLOGGS, 2, ArtifactTypeEnum.Document, "Test message from Fred", PROJECT_ID, false, false);

				ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();
				int sourceTestCaseArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.TestCase, 2, ArtifactTypeEnum.Task, 43, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);
				int sourceTestStepArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.TestStep, 2, ArtifactTypeEnum.Requirement, 4, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);
				int sourceRiskArtifactLinkId = artifactLinkManager.Insert(PROJECT_ID, ArtifactTypeEnum.Risk, 1, ArtifactTypeEnum.Risk, 2, USER_ID_FRED_BLOGGS, "inserted by unit test", DateTime.Now);

				//Now make a copy of the sample project and use the same template
				projectId = projectManager.CopyReset(USER_ID_FRED_BLOGGS, PROJECT_ID, false);
				//Refresh cache so that all data should match up with original project after the clone
				projectManager.RefreshTestStatusAndTaskProgressCache(projectId, false, null);

				#endregion

				//Now lets verify the data created

				//First the project itself
				Project project = projectManager.RetrieveById(projectId);
				ProjectTemplate projectTemplate = templateManager.RetrieveForProject(projectId);
				Assert.AreEqual("Library Information System (Sample) - Copy", project.Name);
				Assert.AreEqual(true, project.IsActive);

				//Next the component count
				List<Component> components = new ComponentManager().Component_Retrieve(projectId);
				Assert.AreEqual(3, components.Count);

				#region Requirements
				//Verify the count
				Business.RequirementManager requirementManager = new Business.RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(USER_ID_FRED_BLOGGS, projectId, 1, Int32.MaxValue, null, 0);
				List<RequirementView> sourceRequirements = requirementManager.Retrieve(USER_ID_SYS_ADMIN, PROJECT_ID, 1, Int32.MaxValue, null, 0);
				Assert.AreEqual(sourceRequirements.Count, requirements.Count);

				//Verify that the requirements are attached to the new components
				RequirementView requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.IsTrue(components.Any(c => c.ComponentId == requirement.ComponentId.Value));

				//Verify that a use case has its steps copied over as well
				requirement = requirements.FirstOrDefault(r => r.Name == "Creating a new book in the system");
				RequirementView sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Creating a new book in the system");
				Assert.AreEqual(requirementManager.CountSteps(sourceRequirement.RequirementId), requirementManager.CountSteps(requirement.RequirementId));

				//Verify standard fields
				requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.AreEqual(sourceRequirement.ReleaseVersionNumber, requirement.ReleaseVersionNumber);
				Assert.AreEqual(sourceRequirement.ImportanceName, requirement.ImportanceName);
				Assert.AreEqual(sourceRequirement.ImportanceId, requirement.ImportanceId);

				//Verify the status - use a main parent RQ to get good data
				requirement = requirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Functional System Requirements");
				Assert.AreEqual((int)Requirement.RequirementStatusEnum.Planned, requirement.RequirementStatusId);
				Assert.AreEqual(0, requirement.CoverageCountBlocked);
				Assert.AreEqual(0, requirement.CoverageCountCaution);
				Assert.AreEqual(0, requirement.CoverageCountFailed);
				Assert.AreEqual(0, requirement.CoverageCountPassed);
				Assert.AreEqual(sourceRequirement.CoverageCountTotal, requirement.CoverageCountTotal);
				Assert.AreEqual(sourceRequirement.TaskCount, requirement.TaskCount);
				Assert.AreEqual(0, requirement.TaskPercentLateFinish);
				Assert.AreEqual(0, requirement.TaskPercentOnTime);

				//Verify custom properties
				requirement = requirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				sourceRequirement = sourceRequirements.FirstOrDefault(r => r.Name == "Ability to add new books to the system");
				Assert.AreEqual(Int32.Parse(sourceRequirement.Custom_06), Int32.Parse(requirement.Custom_06));

				//Verify comments
				IEnumerable<IDiscussion> sourceDiscussion = discussionManager.Retrieve(sourceRequirement.RequirementId, ArtifactTypeEnum.Requirement);
				IEnumerable<IDiscussion> discussion = discussionManager.Retrieve(requirement.RequirementId, ArtifactTypeEnum.Requirement);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				List<ProjectAttachmentView> sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRequirement.RequirementId, ArtifactTypeEnum.Requirement, "AttachmentId", true, 1, 10, null, 0);
				List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirement.RequirementId, ArtifactTypeEnum.Requirement, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify associations
				List<ArtifactLinkView> sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Requirement, sourceRequirement.RequirementId, "CreationDate");
				List<ArtifactLinkView> associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Requirement, requirement.RequirementId, "CreationDate");
				Assert.AreEqual(2, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify test coverage
				TestCaseManager testCaseManager = new TestCaseManager();
				List<TestCase> sourceRequirementTestCases = testCaseManager.RetrieveCoveredByRequirementId(PROJECT_ID, sourceRequirement.RequirementId);
				List<TestCase> requirementTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirement.RequirementId);
				Assert.AreEqual(sourceRequirementTestCases.Count, requirementTestCases.Count);
				Assert.AreEqual(sourceRequirementTestCases[0].Name, requirementTestCases[0].Name);
				Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, requirementTestCases[0].ExecutionStatusId);

				#endregion

				#region releases
				//Verify the count
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> sourceReleases = releaseManager.RetrieveByProjectId(PROJECT_ID, false);
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, false);
				Assert.AreEqual(sourceReleases.Count, releases.Count);

				//Verify standard fields
				ReleaseView sourceRelease = sourceReleases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				ReleaseView release = releases.FirstOrDefault(r => r.VersionNumber == "1.0.0.0");
				Assert.AreEqual(sourceRelease.Name, release.Name);
				Assert.AreEqual(sourceRelease.StartDate, release.StartDate);
				Assert.AreEqual(sourceRelease.ReleaseTypeId, release.ReleaseTypeId);

				//Verify the status
				int releaseTestCaseTotal = sourceRelease.CountBlocked + sourceRelease.CountCaution + sourceRelease.CountFailed + sourceRelease.CountNotApplicable + sourceRelease.CountNotRun + sourceRelease.CountPassed;
				Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, release.ReleaseStatusId);
				Assert.AreEqual(0, release.CountBlocked);
				Assert.AreEqual(0, release.CountCaution);
				Assert.AreEqual(0, release.CountFailed);
				Assert.AreEqual(0, release.CountPassed);
				Assert.AreEqual(releaseTestCaseTotal, release.CountNotRun);
				Assert.AreEqual(sourceRelease.RequirementCount, release.RequirementCount);
				Assert.AreEqual(0, release.PercentComplete);
				Assert.AreEqual(sourceRelease.TaskCount, release.TaskCount);
				Assert.AreEqual(0, release.TaskPercentOnTime);

				//Verify custom properties
				Assert.AreEqual(sourceRelease.Custom_01, release.Custom_01);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceRelease.ReleaseId, ArtifactTypeEnum.Release);
				discussion = discussionManager.Retrieve(release.ReleaseId, ArtifactTypeEnum.Release);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRelease.ReleaseId, ArtifactTypeEnum.Release, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, release.ReleaseId, ArtifactTypeEnum.Release, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count, "should be 1");
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify test coverage
				release = releases.FirstOrDefault(r => r.VersionNumber == "1.1.0.0.0002");
				List<TestCaseReleaseView> releaseTestCases = testCaseManager.RetrieveMappedByReleaseId2(projectId, release.ReleaseId);
				TestCaseReleaseView releaseTestCase = releaseTestCases[0];
				// we don't do count as they won't match with sample data 
				Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, releaseTestCase.ExecutionStatusId);

				#endregion

				#region Documents
				//Verify folders
				List<ProjectAttachmentFolderHierarchy> sourceDocumentFolders = attachmentManager.RetrieveFoldersByProjectId(PROJECT_ID);
				List<ProjectAttachmentFolderHierarchy> documentFolders = attachmentManager.RetrieveFoldersByProjectId(projectId);
				Assert.AreEqual(sourceDocumentFolders.Count, documentFolders.Count);
				Assert.AreEqual(sourceDocumentFolders[0].Name, documentFolders[0].Name);
				Assert.AreEqual(sourceDocumentFolders[0].HierarchyLevel, documentFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceDocumentFolders[1].Name, documentFolders[1].Name);
				Assert.AreEqual(sourceDocumentFolders[1].HierarchyLevel, documentFolders[1].HierarchyLevel);

				//Verify count
				List<ProjectAttachmentView> sourceDocuments = attachmentManager.RetrieveForProject(PROJECT_ID, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<ProjectAttachmentView> documents = attachmentManager.RetrieveForProject(projectId, null, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceDocuments.Count, documents.Count);

				//Verify standard fields
				ProjectAttachmentView sourceDocument = sourceDocuments.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				ProjectAttachmentView document = documents.FirstOrDefault(r => r.Filename == "Library System Performance Metrics.xls");
				Assert.AreEqual(sourceDocument.AuthorId, document.AuthorId);
				Assert.AreEqual(sourceDocument.Tags.First(), document.Tags.First());
				Assert.AreEqual(sourceDocument.UploadDate, document.UploadDate);
				Assert.AreEqual(sourceDocument.AttachmentTypeName, document.AttachmentTypeName);
				Assert.AreEqual(sourceDocument.AttachmentTypeId, document.AttachmentTypeId);

				//Verify the status
				DocumentStatus defaultDocumentStatus = attachmentManager.DocumentStatusRetrieveDefault(projectTemplate.ProjectTemplateId);
				Assert.AreEqual(defaultDocumentStatus.DocumentStatusId, document.DocumentStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceDocument.Custom_02, document.Custom_02);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceDocument.AttachmentId, ArtifactTypeEnum.Document);
				discussion = discussionManager.Retrieve(document.AttachmentId, ArtifactTypeEnum.Document);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);

				//Verify associations
				sourceDocument = sourceDocuments.FirstOrDefault(r => r.Filename == "Expected Result Screenshot.png");
				document = documents.FirstOrDefault(r => r.Filename == "Expected Result Screenshot.png");
				List<ArtifactAttachmentView> sourceAttachmentAssociations = artifactLinkManager.RetrieveByAttachmentId(PROJECT_ID, sourceDocument.AttachmentId);
				List<ArtifactAttachmentView> attachmentAssociations = artifactLinkManager.RetrieveByAttachmentId(projectId, document.AttachmentId);
				Assert.AreEqual(sourceAttachmentAssociations.Count, attachmentAssociations.Count);
				Assert.AreEqual(sourceAttachmentAssociations[0].CreationDate, attachmentAssociations[0].CreationDate);
				Assert.AreEqual(sourceAttachmentAssociations[0].Comment, attachmentAssociations[0].Comment);


				#endregion

				#region Test Cases
				//Verify folders
				List<TestCaseFolderHierarchyView> sourceTestCaseFolders = testCaseManager.TestCaseFolder_GetList(PROJECT_ID);
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				Assert.AreEqual(sourceTestCaseFolders.Count, testCaseFolders.Count);
				Assert.AreEqual(sourceTestCaseFolders[0].Name, testCaseFolders[0].Name);
				Assert.AreEqual(sourceTestCaseFolders[0].HierarchyLevel, testCaseFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTestCaseFolders[1].Name, testCaseFolders[1].Name);
				Assert.AreEqual(sourceTestCaseFolders[1].HierarchyLevel, testCaseFolders[1].HierarchyLevel);

				//Verify count
				List<TestCaseView> sourceTestCases = testCaseManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				Assert.AreEqual(sourceTestCases.Count, testCases.Count);

				//Verify component
				TestCaseView sourceTestCase = sourceTestCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				TestCaseView testCase = testCases.FirstOrDefault(r => r.Name == "Ability to create new book");
				List<int> artifactComponents = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
				Assert.IsTrue(components.Any(c => c.ComponentId == artifactComponents.First()));

				//Verify test steps
				List<TestStepView> sourceTestSteps = testCaseManager.RetrieveStepsForTestCase(sourceTestCase.TestCaseId);
				List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCase.TestCaseId);
				Assert.AreEqual(sourceTestSteps.Count, testSteps.Count);
				Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testSteps[1].ExecutionStatusId);
				Assert.AreEqual(sourceTestSteps[1].Description, testSteps[1].Description);
				Assert.AreEqual(sourceTestSteps[1].Position, testSteps[1].Position);
				Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testSteps[2].ExecutionStatusId);
				Assert.AreEqual(sourceTestSteps[2].Description, testSteps[2].Description);
				Assert.AreEqual(sourceTestSteps[2].Position, testSteps[2].Position);

				//Verify test step custom properties
				Assert.AreEqual(sourceTestSteps[2].Custom_01, testSteps[2].Custom_01);

				//Verify test step associations
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestStep, testSteps[1].TestStepId, "CreationDate");
				Assert.AreEqual(1, associations.Count);

				//Verify test step attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestSteps[2].TestStepId, ArtifactTypeEnum.TestStep, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testSteps[2].TestStepId, ArtifactTypeEnum.TestStep, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify standard fields
				Assert.AreEqual(sourceTestCase.TestCasePriorityName, testCase.TestCasePriorityName);
				Assert.AreEqual(sourceTestCase.TestCasePriorityId, testCase.TestCasePriorityId);
				Assert.AreEqual(sourceTestCase.TestCaseTypeName, testCase.TestCaseTypeName);
				Assert.AreEqual(sourceTestCase.TestCaseTypeId, testCase.TestCaseTypeId);
				Assert.AreEqual(sourceTestCase.Description, testCase.Description);
				Assert.AreEqual(sourceTestCase.AuthorId, testCase.AuthorId);

				//Verify the status
				Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, testCase.ExecutionStatusId);
				Assert.AreEqual((int)TestCase.TestCaseStatusEnum.Draft, testCase.TestCaseStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTestCase.Custom_01, testCase.Custom_01);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTestCase.TestCaseId, ArtifactTypeEnum.TestCase);
				discussion = discussionManager.Retrieve(testCase.TestCaseId, ArtifactTypeEnum.TestCase);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestCase, sourceTestCase.TestCaseId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.TestCase, testCase.TestCaseId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestCase.TestCaseId, ArtifactTypeEnum.TestCase, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testCase.TestCaseId, ArtifactTypeEnum.TestCase, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				#endregion

				#region Test Sets
				TestSetManager testSetManager = new TestSetManager();

				//Verify folders
				List<TestSetFolderHierarchyView> sourceTestSetFolders = testSetManager.TestSetFolder_GetList(PROJECT_ID);
				List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
				Assert.AreEqual(sourceTestSetFolders.Count, testSetFolders.Count);
				Assert.AreEqual(sourceTestSetFolders[0].Name, testSetFolders[0].Name);
				Assert.AreEqual(sourceTestSetFolders[0].HierarchyLevel, testSetFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTestSetFolders[1].Name, testSetFolders[1].Name);
				Assert.AreEqual(sourceTestSetFolders[1].HierarchyLevel, testSetFolders[1].HierarchyLevel);

				//Verify count
				List<TestSetView> sourceTestSets = testSetManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				List<TestSetView> testSets = testSetManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				Assert.AreEqual(sourceTestSets.Count, testSets.Count);

				//Verify standard fields
				TestSetView sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				TestSetView testSet = testSets.FirstOrDefault(r => r.Name == "Testing Cycle for Release 1.0");
				Assert.AreEqual(sourceTestSet.ReleaseVersionNumber, testSet.ReleaseVersionNumber);
				Assert.AreEqual(sourceTestSet.Description, testSet.Description);
				Assert.AreEqual(sourceTestSet.OwnerId, testSet.OwnerId);
				Assert.AreEqual(sourceTestSet.TestRunTypeId, testSet.TestRunTypeId);

				//Verify the status
				Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, testSet.TestSetStatusId);
				int testSetTestCaseCount = sourceTestSet.CountBlocked + sourceTestSet.CountCaution + sourceTestSet.CountFailed + sourceTestSet.CountNotApplicable + sourceTestSet.CountNotRun + sourceTestSet.CountPassed;
				Assert.AreEqual(0, testSet.CountPassed);
				Assert.AreEqual(0, testSet.CountBlocked);
				Assert.AreEqual(0, testSet.CountCaution);
				Assert.AreEqual(0, testSet.CountFailed);
				Assert.AreEqual(0, testSet.CountNotApplicable);
				Assert.AreEqual(testSetTestCaseCount, testSet.CountNotRun);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTestSet.TestSetId, ArtifactTypeEnum.TestSet);
				discussion = discussionManager.Retrieve(testSet.TestSetId, ArtifactTypeEnum.TestSet);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTestSet.TestSetId, ArtifactTypeEnum.TestSet, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, testSet.TestSetId, ArtifactTypeEnum.TestSet, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count, "should be 1");
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify custom properties
				sourceTestSet = sourceTestSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				testSet = testSets.FirstOrDefault(r => r.Name == "Regression Testing for Windows 8");
				Assert.AreEqual(sourceTestCase.Custom_01, testCase.Custom_01);

				#endregion

				#region Test Runs
				//Verify count
				TestRunManager testRunManager = new TestRunManager();
				List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(0, testRuns.Count);

				#endregion

				#region Automation Hosts
				//Verify the count
				AutomationManager automationManager = new AutomationManager();
				List<AutomationHostView> sourceAutomationHosts = automationManager.RetrieveHosts(PROJECT_ID);
				List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId);
				Assert.AreEqual(sourceAutomationHosts.Count, automationHosts.Count);

				//Verify standard fields
				AutomationHostView sourceAutomationHost = sourceAutomationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				AutomationHostView automationHost = automationHosts.FirstOrDefault(r => r.Name == "Windows 8 Host");
				Assert.AreEqual(sourceAutomationHost.IsActive, automationHost.IsActive);
				Assert.AreEqual(sourceAutomationHost.Description, automationHost.Description);
				Assert.AreEqual(sourceAutomationHost.Token, automationHost.Token);

				//Verify custom properties
				Assert.IsTrue(!String.IsNullOrEmpty(automationHost.Custom_01), "check that there is any value set at all");

				#endregion

				#region Test Configurations
				//Verify the count
				TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
				List<TestConfigurationSet> sourceTestConfigurations = testConfigurationManager.RetrieveSets(PROJECT_ID);
				List<TestConfigurationSet> testConfigurations = testConfigurationManager.RetrieveSets(projectId);
				Assert.AreEqual(sourceTestConfigurations.Count, testConfigurations.Count);

				//Verify standard fields
				TestConfigurationSet sourceTestConfiguration = sourceTestConfigurations.FirstOrDefault(r => r.Name == "Target web browsers and operating systems");
				TestConfigurationSet testConfiguration = testConfigurations.FirstOrDefault(r => r.Name == "Target web browsers and operating systems");
				Assert.AreEqual(sourceTestConfiguration.IsActive, testConfiguration.IsActive);
				Assert.AreEqual(sourceTestConfiguration.Description, testConfiguration.Description);

				#endregion

				#region Incidents
				//Verify count
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(0, incidents.Count);

				#endregion

				#region Tasks
				TaskManager taskManager = new TaskManager();

				//Verify Folders
				List<TaskFolderHierarchyView> sourceTaskFolders = taskManager.TaskFolder_GetList(PROJECT_ID);
				List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(projectId);
				Assert.AreEqual(sourceTaskFolders.Count, taskFolders.Count);
				Assert.AreEqual(sourceTaskFolders[0].Name, taskFolders[0].Name);
				Assert.AreEqual(sourceTaskFolders[0].HierarchyLevel, taskFolders[0].HierarchyLevel);
				Assert.AreEqual(sourceTaskFolders[1].Name, taskFolders[1].Name);
				Assert.AreEqual(sourceTaskFolders[1].HierarchyLevel, taskFolders[1].HierarchyLevel);

				//Verify count
				List<TaskView> sourceTasks = taskManager.Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<TaskView> tasks = taskManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceTasks.Count, tasks.Count);

				//Verify standard fields
				TaskView sourceTask = sourceTasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				TaskView task = tasks.FirstOrDefault(r => r.Name == "Develop new book entry screen");
				Assert.AreEqual(sourceTask.OwnerId, task.OwnerId);
				Assert.AreEqual(sourceTask.TaskPriorityName, task.TaskPriorityName);
				Assert.AreEqual(sourceTask.EstimatedEffort, task.EstimatedEffort);
				Assert.AreEqual(sourceTask.TaskTypeName, task.TaskTypeName);
				Assert.AreEqual(null, task.StartDate);
				Assert.AreEqual(null, task.EndDate);
				Assert.AreEqual(null, task.ActualEffort);

				//Verify the status
				Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, task.TaskStatusId);

				//Verify custom properties
				Assert.AreEqual(sourceTask.Custom_02, task.Custom_02);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Task, sourceTask.TaskId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Task, task.TaskId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceTask.TaskId, ArtifactTypeEnum.Task);
				discussion = discussionManager.Retrieve(task.TaskId, ArtifactTypeEnum.Task);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceTask.TaskId, ArtifactTypeEnum.Task, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, task.TaskId, ArtifactTypeEnum.Task, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify components
				Assert.IsTrue(components.Any(c => c.ComponentId == task.ComponentId.Value));

				#endregion

				#region Risks
				//Verify count
				RiskManager riskManager = new RiskManager();
				List<RiskView> sourceRisks = riskManager.Risk_Retrieve(PROJECT_ID, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				List<RiskView> risks = riskManager.Risk_Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET);
				Assert.AreEqual(sourceRisks.Count, risks.Count);

				//Verify standard fields
				RiskView sourceRisk = sourceRisks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				RiskView risk = risks.FirstOrDefault(r => r.Name == "The v1.1 release may not be ready in time");
				Assert.AreEqual(sourceRisk.OwnerId, risk.OwnerId);
				Assert.AreEqual(sourceRisk.ReleaseName, risk.ReleaseName);
				Assert.AreEqual(sourceRisk.Description, risk.Description);

				//Verify the template fields like status
				RiskStatus defaultRiskStatus = riskManager.RiskStatus_RetrieveDefault(projectTemplate.ProjectTemplateId);
				Assert.AreEqual(sourceRisk.RiskProbabilityName, risk.RiskProbabilityName);
				Assert.AreEqual(sourceRisk.RiskProbabilityId, risk.RiskProbabilityId);
				Assert.AreEqual(sourceRisk.RiskTypeName, risk.RiskTypeName);
				Assert.AreEqual(sourceRisk.RiskTypeId, risk.RiskTypeId);
				Assert.AreEqual(defaultRiskStatus.RiskStatusId, risk.RiskStatusId);

				//Verify custom properties - none defined in originating template

				//Verify mitigations
				List<RiskMitigation> riskMitagtions = riskManager.RiskMitigation_Retrieve(risk.RiskId);
				Assert.AreEqual(0, riskMitagtions.Count);

				//Verify associations
				sourceAssociations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Risk, sourceRisk.RiskId, "CreationDate");
				associations = artifactLinkManager.RetrieveByArtifactId(ArtifactTypeEnum.Risk, risk.RiskId, "CreationDate");
				Assert.AreEqual(sourceAssociations.Count, associations.Count);
				Assert.AreEqual(sourceAssociations[0].ArtifactName, associations[0].ArtifactName);
				Assert.AreEqual(sourceAssociations[0].Comment, associations[0].Comment);

				//Verify comments
				sourceDiscussion = discussionManager.Retrieve(sourceRisk.RiskId, ArtifactTypeEnum.Risk);
				discussion = discussionManager.Retrieve(risk.RiskId, ArtifactTypeEnum.Risk);
				Assert.AreEqual(sourceDiscussion.Count(), discussion.Count());
				Assert.AreEqual(sourceDiscussion.First().CreatorName, discussion.First().CreatorName);
				Assert.AreEqual(sourceDiscussion.First().Text, discussion.First().Text);
				Assert.AreEqual(sourceDiscussion.First().CreationDate, discussion.First().CreationDate);

				//Verify attachments
				sourceAttachments = attachmentManager.RetrieveByArtifactId(PROJECT_ID, sourceRisk.RiskId, ArtifactTypeEnum.Risk, "AttachmentId", true, 1, 10, null, 0);
				attachments = attachmentManager.RetrieveByArtifactId(projectId, risk.RiskId, ArtifactTypeEnum.Risk, "AttachmentId", true, 1, 10, null, 0);
				Assert.AreEqual(sourceAttachments.Count, attachments.Count);
				Assert.AreEqual(sourceAttachments[0].AuthorId, attachments[0].AuthorId);
				Assert.AreEqual(sourceAttachments[0].Filename, attachments[0].Filename);
				Assert.AreEqual(sourceAttachments[0].AttachmentTypeName, attachments[0].AttachmentTypeName);

				//Verify components
				risk = risks.FirstOrDefault(r => r.Name == "We may not get enough authors to sign up");
				Assert.IsTrue(components.Any(c => c.ComponentId == risk.ComponentId.Value));

				#endregion

				#region Cleanup
				//Finally clean up by deleting the new project
				projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);

				//Then reset the original project back to what it was before
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Release, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.TestSet, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Task, 1);
				attachmentManager.Delete(PROJECT_ID, 19, 1, ArtifactTypeEnum.Risk, 1);

				discussionManager.DeleteDiscussionId(sourceDocumentDiscussionId, ArtifactTypeEnum.Document, true, 1, 1);

				artifactLinkManager.Delete(sourceTestCaseArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.TestCase, 1, 1);
				artifactLinkManager.Delete(sourceTestStepArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.TestSet, 1, 1);
				artifactLinkManager.Delete(sourceRiskArtifactLinkId, DataModel.Artifact.ArtifactTypeEnum.Risk, 1, 1);

				//NOTE can't remove the incident from the test run step as per 6.9.0
				#endregion
			}

			#endregion

		}

		[
		Test,
		SpiraTestCase(268)
		]
		public void _12_DeleteProject()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First lets make a copy of the sample project that we can test the project delete against
			//In this case we will use the existing template, so no need to delete the template at the end
			projectId = projectManager.Copy(USER_ID_FRED_BLOGGS, PROJECT_ID, true, null, adminSectionId, "Project Cloned");
			int templateId = templateManager.RetrieveForProject(projectId).ProjectTemplateId;

			//Now lets create an incident
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(
				projectId,
				null,
				null,
				USER_ID_FRED_BLOGGS,
				USER_ID_FRED_BLOGGS,
				null,
				"Test Incident",
				"Test Incident Description",
				null,
				null,
				null,
				incidentManager.GetDefaultIncidentType(templateId),
				incidentManager.IncidentStatus_RetrieveDefault(templateId).IncidentStatusId,
				DateTime.Now,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				USER_ID_FRED_BLOGGS
				);

			//Now lets add a resolution to that incident
			Incident incident = incidentManager.RetrieveById(incidentId, true);
			incident.StartTracking();
			IncidentResolution resolution = new IncidentResolution();
			resolution.CreatorId = USER_ID_FRED_BLOGGS;
			resolution.Resolution = "Test Resolution";
			resolution.CreationDate = DateTime.UtcNow;
			incident.Resolutions.Add(resolution);
			incidentManager.Update(incident, USER_ID_JOE_SMITH);

			//Verify the resolution was added
			incident = incidentManager.RetrieveById(incidentId, true);
			Assert.AreEqual(1, incident.Resolutions.Count);

			//Now lets execute at least one test run
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, InternalRoutines.UTC_OFFSET, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
			TestRunManager testRunManager = new TestRunManager();
			List<int> testCaseList = new List<int>();
			testCaseList.Add(testCases[1].TestCaseId);
			TestRunsPending testRunsPending = testRunManager.CreateFromTestCase(USER_ID_FRED_BLOGGS, projectId, null, testCaseList, true);
			int pendingId = testRunsPending.TestRunsPendingId;
			testRunsPending = testRunManager.RetrievePendingById(pendingId, true);
			testRunsPending.TestRuns[0].TestRunSteps[0].StartTracking();
			testRunsPending.TestRuns[0].TestRunSteps[0].ActualResult = "broke";
			testRunsPending.TestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			testRunManager.UpdateExecutionStatus(projectId, USER_ID_FRED_BLOGGS, testRunsPending, 0, DateTime.UtcNow, true);

			//Test that any saved reports get deleted
			ReportManager reportManager = new ReportManager();
			reportManager.InsertSaved(1, 1, 1, projectId, "Test Report", "", false);

			//Test that placeholders get deleted
			PlaceholderManager placeholderManager = new PlaceholderManager();
			placeholderManager.Placeholder_Create(projectId);

			//Test that user artifact / custom property column settings get deleted
			ArtifactManager artifactManager = new ArtifactManager();
			artifactManager.ArtifactField_ToggleListVisibility(projectId, 1, Artifact.ArtifactTypeEnum.Requirement, "ImportanceId");
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			customPropertyManager.CustomProperty_ToggleListVisibility(projectId, templateId, 1, Artifact.ArtifactTypeEnum.Requirement, "Custom_01");

			//Add a document with tags and verify it can be deleted with the project (had an issue previously)
			AttachmentManager attachmentManager = new AttachmentManager();
			int attachmentId1 = attachmentManager.Insert(projectId, "www.x.com", null, USER_ID_FRED_BLOGGS, incidentId, Artifact.ArtifactTypeEnum.Incident, "1.0", "tag1, tag2", null, null, null);

			//Now have one with null tags record, that apparently broke v6.0
			int attachmentId2 = attachmentManager.Insert(projectId, "www.y.com", null, USER_ID_FRED_BLOGGS, incidentId, Artifact.ArtifactTypeEnum.Incident, "1.0", "tag1, tag2", null, null, null);
			ProjectAttachment attachment = attachmentManager.RetrieveForProjectById(projectId, attachmentId2, true);
			attachment.Attachment.StartTracking();
			attachment.Attachment.Tags = null;
			attachmentManager.Update(attachment, USER_ID_FRED_BLOGGS);

			//Now test that we can successfully delete the project (not the template)
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
		}

		/// <summary>
		/// This tests that we can change the schedule information associated with a project
		/// </summary>
		/// <remarks>It does NOT test the parts of the system that depend on those settings</remarks>
		[
		Test,
		SpiraTestCase(436)
		]
		public void _15_Ability_Modify_Project_Schedule_Settings()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First lets test that we can create a new project with the default settings
			int projectId1 = projectManager.Insert("New Project1", null, "", "", true, null, 1, adminSectionId, "Inserted Project");

			//Verify the data
			Project project = projectManager.RetrieveById(projectId1);
			Assert.AreEqual("New Project1", project.Name);
			Assert.AreEqual(8, project.WorkingHours);
			Assert.AreEqual(5, project.WorkingDays);
			Assert.AreEqual(0, project.NonWorkingHours);
			Assert.AreEqual(true, project.IsTimeTrackIncidents);
			Assert.AreEqual(true, project.IsTimeTrackTasks);
			Assert.AreEqual(true, project.IsEffortIncidents);
			Assert.AreEqual(true, project.IsEffortTasks);
			Assert.AreEqual(false, project.IsTasksAutoCreate);
			Assert.AreEqual(1.0M, project.ReqDefaultEstimate.Value);
			Assert.IsFalse(project.TaskDefaultEffort.HasValue);
			Assert.AreEqual(480, project.ReqPointEffort);
			int templateId1 = project.ProjectTemplateId;

			//Now lets test that we can create a new projct with custom settings
			int projectId2 = projectManager.Insert("New Project2", null, "", "", true, null, 1, adminSectionId, "Inserted Project");

			//Verify the data
			project = projectManager.RetrieveById(projectId2);
			Assert.AreEqual("New Project2", project.Name);
			Assert.AreEqual(8, project.WorkingHours);
			Assert.AreEqual(5, project.WorkingDays);
			Assert.AreEqual(0, project.NonWorkingHours);
			Assert.AreEqual(true, project.IsTimeTrackIncidents);
			Assert.AreEqual(true, project.IsTimeTrackTasks);
			Assert.AreEqual(true, project.IsEffortIncidents);
			Assert.AreEqual(true, project.IsEffortTasks);
			Assert.AreEqual(false, project.IsEffortTestCases);
			Assert.AreEqual(false, project.IsTasksAutoCreate);
			//Assert.AreEqual(420, project.TaskDefaultEffort);
			Assert.AreEqual(1.0M, project.ReqDefaultEstimate);
			Assert.AreEqual(480, project.ReqPointEffort);
			int templateId2 = project.ProjectTemplateId;

			//Now lets test that we can update the settings
			project.StartTracking();
			project.WorkingHours = 7;
			project.WorkingDays = 4;
			project.NonWorkingHours = 2;
			project.IsTimeTrackIncidents = false;
			project.IsTimeTrackTasks = false;
			project.IsEffortIncidents = true;
			project.IsEffortTasks = true;
			project.IsEffortTestCases = true;
			project.IsTasksAutoCreate = true;
			project.TaskDefaultEffort = null;
			project.ReqDefaultEstimate = 1.0M;
			project.ReqPointEffort = 360;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify the data
			project = projectManager.RetrieveById(projectId2);
			Assert.AreEqual("New Project2", project.Name);
			Assert.AreEqual(7, project.WorkingHours);
			Assert.AreEqual(4, project.WorkingDays);
			Assert.AreEqual(2, project.NonWorkingHours);
			Assert.AreEqual(false, project.IsTimeTrackIncidents);
			Assert.AreEqual(false, project.IsTimeTrackTasks);
			Assert.AreEqual(true, project.IsEffortIncidents);
			Assert.AreEqual(true, project.IsEffortTasks);
			Assert.AreEqual(true, project.IsEffortTestCases);
			Assert.AreEqual(true, project.IsTasksAutoCreate);
			Assert.IsFalse(project.TaskDefaultEffort.HasValue);
			Assert.AreEqual(1.0M, project.ReqDefaultEstimate);
			Assert.AreEqual(360, project.ReqPointEffort);

			//Now lets create a new project based on this project and verify that it copies the settings
			int projectId3 = projectManager.CreateFromExisting("New Project3", "", "", projectId2);

			//Verify the data
			project = projectManager.RetrieveById(projectId3);
			Assert.AreEqual("New Project3", project.Name);
			Assert.AreEqual(7, project.WorkingHours);
			Assert.AreEqual(4, project.WorkingDays);
			Assert.AreEqual(2, project.NonWorkingHours);
			Assert.AreEqual(false, project.IsTimeTrackIncidents);
			Assert.AreEqual(false, project.IsTimeTrackTasks);
			Assert.AreEqual(true, project.IsEffortIncidents);
			Assert.AreEqual(true, project.IsEffortTasks);
			Assert.IsFalse(project.TaskDefaultEffort.HasValue);
			Assert.AreEqual(1.0M, project.ReqDefaultEstimate);
			Assert.AreEqual(360, project.ReqPointEffort);

			//Now lets copy the project and verify that it copies the settings
			int projectId4 = projectManager.Copy(USER_ID_FRED_BLOGGS, projectId2, true, null, adminSectionId, "Project Cloned");

			//Verify the data
			project = projectManager.RetrieveById(projectId4);
			Assert.AreEqual("New Project2 - Copy", project.Name);
			Assert.AreEqual(7, project.WorkingHours);
			Assert.AreEqual(4, project.WorkingDays);
			Assert.AreEqual(2, project.NonWorkingHours);
			Assert.AreEqual(false, project.IsTimeTrackIncidents);
			Assert.AreEqual(false, project.IsTimeTrackTasks);
			Assert.AreEqual(true, project.IsEffortIncidents);
			Assert.AreEqual(true, project.IsEffortTasks);
			Assert.AreEqual(true, project.IsTasksAutoCreate);
			Assert.IsFalse(project.TaskDefaultEffort.HasValue);
			Assert.AreEqual(1.0M, project.ReqDefaultEstimate);
			Assert.AreEqual(360, project.ReqPointEffort);

			//Clean up
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId1);
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId2);
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId3);
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId4);

			//We only have two templates to delete
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId1);
			templateManager.Delete(USER_ID_FRED_BLOGGS, templateId2);
		}

		/// <summary>
		/// This tests that changes to a project's planning options affect the various planning calculations
		/// </summary>
		[
		Test,
		SpiraTestCase(437)
		]
		public void _16_ProjectScheduleSettingsUpdateEffortValues()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First we need to create a new project with some initial planning settings
			//8-hour day, 5-day week, no non-working days and effort calculations enabled for incidents, tasks and test cases
			//It also does not create tasks for in-progress requirements and has no default effort for new tasks/requirements
			projectId = projectManager.Insert("New Project", null, "", "", true, null, 8, 5, 0, true, true, true, true, true, false, null, null, null, true, true, true, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//First we test that the release planned effort respects the calendar specifications

			//This release has 2 resource, with one of the resources having a vacation day
			ReleaseManager releaseManager = new ReleaseManager();
			int releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(20640, release.PlannedEffort);
			//Now Delete the release
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);

			//Now modify the project to have a 9-hour working day and a 6-day working week
			Project project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.WorkingDays = 6;
			project.WorkingHours = 9;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now recreate the same release and try again
			releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(27540, release.PlannedEffort);
			//Now Delete the release
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);

			//Now modify the project to have a 10-hour working day, a 6-day working week and a national holiday on one of the days
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.WorkingDays = 6;
			project.WorkingHours = 10;
			project.NonWorkingHours = 10;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Now recreate the same release and try again
			releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			release = releaseManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, releaseId);
			Assert.AreEqual(29640, release.PlannedEffort);
			//Now Delete the release
			releaseManager.MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId, releaseId);

			//Now reset the project back to a 8-hour working day, a 5-day working week
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.WorkingDays = 5;
			project.WorkingHours = 8;
			project.NonWorkingHours = 0;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Recreate the release again and verify
			releaseId = releaseManager.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(20640, release.PlannedEffort);
			Assert.AreEqual(0, release.TaskEstimatedEffort);
			Assert.AreEqual(0, release.TaskActualEffort);
			Assert.AreEqual(20640, release.AvailableEffort);

			//Lets create a task and an incident and assign both to this release
			TaskManager taskManager = new TaskManager();
			int tk_priorityHighId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 2).TaskPriorityId;
			int taskId = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, releaseId, null, tk_priorityHighId, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			IncidentManager incidentManager = new IncidentManager();
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, null, null, "Incident 1", "Really bad incident", releaseId, releaseId, null, null, null, DateTime.Now, DateTime.Parse("3/5/2009"), null, 1200, 1220, 40, null, null, USER_ID_FRED_BLOGGS);

			//Verify the effort values
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(20640, release.PlannedEffort);
			Assert.AreEqual(3120, release.TaskEstimatedEffort);
			Assert.AreEqual(1280, release.TaskActualEffort);
			Assert.AreEqual(1960, release.TaskRemainingEffort);
			Assert.AreEqual(3180, release.TaskProjectedEffort);
			Assert.AreEqual(17460, release.AvailableEffort);

			//Now turn off consideration of incidents in the effort calculations
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.IsEffortTasks = true;
			project.IsEffortIncidents = false;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Refresh the effort data and verify the new calculations
			projectManager.RefreshTaskProgressCache(projectId);
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(20640, release.PlannedEffort);
			Assert.AreEqual(1920, release.TaskEstimatedEffort);
			Assert.AreEqual(60, release.TaskActualEffort);
			Assert.AreEqual(1920, release.TaskRemainingEffort);
			Assert.AreEqual(1920, release.TaskProjectedEffort);
			Assert.AreEqual(18720, release.AvailableEffort);

			//Now turn off consideration of tasks in the effort calculations
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.IsEffortTasks = false;
			project.IsEffortIncidents = true;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Refresh the effort data and verify the new calculations
			projectManager.RefreshTaskProgressCache(projectId);
			release = releaseManager.RetrieveById2(projectId, releaseId);
			Assert.AreEqual(20640, release.PlannedEffort);
			Assert.AreEqual(1200, release.TaskEstimatedEffort);
			Assert.AreEqual(1220, release.TaskActualEffort);
			Assert.AreEqual(40, release.TaskRemainingEffort);
			Assert.AreEqual(1260, release.TaskProjectedEffort);
			Assert.AreEqual(19380, release.AvailableEffort);

			//Next test that we can enable the project to create requirements and tasks with default effort values
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.ReqDefaultEstimate = 1.0M;
			project.ReqPointEffort = 360;
			project.TaskDefaultEffort = 240;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify
			RequirementManager requirementManager = new RequirementManager();
			int rq_importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;
			int requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, rq_importanceHighId, "Test Req 1", null, null, USER_ID_FRED_BLOGGS);
			RequirementView requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(360, requirementView.EstimatedEffort);
			taskId = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, tk_priorityHighId, "Task 1", "", null, null, null, null, null, USER_ID_FRED_BLOGGS);
			Task task = taskManager.RetrieveById(taskId);
			Assert.AreEqual(240, task.EstimatedEffort);
			Assert.AreEqual(240, task.RemainingEffort);

			//Now put the values back
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.ReqDefaultEstimate = null;
			project.TaskDefaultEffort = null;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify
			requirementId = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, null, null, (int?)null, Requirement.RequirementStatusEnum.Requested, null, USER_ID_FRED_BLOGGS, null, rq_importanceHighId, "Test Req 1", null, null, USER_ID_FRED_BLOGGS);
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.IsFalse(requirementView.EstimatedEffort.HasValue);
			taskId = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, null, null, null, tk_priorityHighId, "Task 1", "", null, null, null, null, null, USER_ID_FRED_BLOGGS);
			task = taskManager.RetrieveById(taskId);
			Assert.IsFalse(task.EstimatedEffort.HasValue);
			Assert.IsFalse(task.RemainingEffort.HasValue);

			//Test that we can configure whether to create a default task for an in-progress requirement or not

			//First check that it doesn't happen with the current settings
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.InProgress;
			requirement.ReleaseId = releaseId;
			requirement.EstimatePoints = 6.0M;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(0, requirementView.TaskCount);

			//Now change the settings to auto-create a task
			project = projectManager.RetrieveById(projectId);
			project.StartTracking();
			project.IsTasksAutoCreate = true;
			projectManager.Update(project, 1, adminSectionId, "Updated Project");

			//Verify
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Requested;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			requirement = requirementView.ConvertTo<RequirementView, Requirement>();
			requirement.StartTracking();
			requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.InProgress;
			requirementManager.Update(USER_ID_FRED_BLOGGS, projectId, new List<Requirement>() { requirement });
			requirementView = requirementManager.RetrieveById(USER_ID_FRED_BLOGGS, projectId, requirementId);
			Assert.AreEqual(1, requirementView.TaskCount);

			//Finally Clean up the project and template
			projectManager.Delete(USER_ID_FRED_BLOGGS, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can retrieve a list of project personnel resources and their committed effort
		/// </summary>
		[
		Test,
		SpiraTestCase(445)
		]
		public void _17_RetrieveProjectResources()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//So that we can get stable data we need to create a new project
			int projectId = projectManager.Insert("RetrieveProjectResources Project", null, "", "", true, null, 1, adminSectionId, "Inserted Project");

			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			//Now add some users to the project
			projectManager.InsertUserMembership(USER_ID_FRED_BLOGGS, projectId, 2);    //Manager
			projectManager.InsertUserMembership(USER_ID_JOE_SMITH, projectId, 5);    //Observer

			//Now we need to create some new releases, iterations, requirements, incidents and tasks

			//First the release and child iteration
			ReleaseManager release = new ReleaseManager();
			int releaseId = release.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1", "", "1.0", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.MajorRelease, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			int iterationId = release.Insert(USER_ID_FRED_BLOGGS, projectId, USER_ID_FRED_BLOGGS, "Release 1 - Sprint 1", "", "1.0 001", (int?)null, Release.ReleaseStatusEnum.Planned, Release.ReleaseTypeEnum.Iteration, DateTime.Parse("3/2/2009"), DateTime.Parse("3/31/2009"), 2, 1, null, false);
			release.Indent(USER_ID_FRED_BLOGGS, projectId, iterationId);

			//Next the requirements, tasks and incidents
			RequirementManager requirementManager = new RequirementManager();
			TaskManager taskManager = new TaskManager();
			IncidentManager incidentManager = new IncidentManager();
			int tk_priorityHighId = taskManager.TaskPriority_Retrieve(projectTemplateId).FirstOrDefault(p => p.Score == 2).TaskPriorityId;
			int rq_importanceHighId = requirementManager.RequirementImportance_Retrieve(projectTemplateId).FirstOrDefault(i => i.Score == 2).ImportanceId;
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, iterationId, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, rq_importanceHighId, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId, releaseId, null, (int?)null, Requirement.RequirementStatusEnum.Completed, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, rq_importanceHighId, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int taskId1 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, null, requirementId1, iterationId, USER_ID_FRED_BLOGGS, tk_priorityHighId, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(projectId, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, iterationId, USER_ID_JOE_SMITH, tk_priorityHighId, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int incidentId = incidentManager.Insert(projectId, null, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Incident 1", "Really bad incident", releaseId, releaseId, null, null, null, DateTime.Now, DateTime.Parse("3/5/2009"), null, 1200, 1220, 40, null, null, USER_ID_FRED_BLOGGS);

			//First retrieve the resource usage for the project as a whole
			List<ProjectResourceView> projectResources = projectManager.RetrieveResources(projectId);
			Assert.AreEqual(3, projectResources.Count);
			ProjectResourceView resourcesRow = projectResources[0];
			Assert.AreEqual("Fred Bloggs", resourcesRow.FullName);
			Assert.AreEqual("Manager", resourcesRow.ProjectRoleName);
			Assert.AreEqual(null, resourcesRow.ResourceEffort);
			Assert.AreEqual(60, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(1260, resourcesRow.IncidentEffort);
			Assert.AreEqual(1320, resourcesRow.TotalEffort);
			Assert.AreEqual(1260, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(null, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[1];
			Assert.AreEqual("Joe Smith", resourcesRow.FullName);
			Assert.AreEqual("Observer", resourcesRow.ProjectRoleName);
			Assert.AreEqual(null, resourcesRow.ResourceEffort);
			Assert.AreEqual(2880, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(2880, resourcesRow.TotalEffort);
			Assert.AreEqual(1920, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(null, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[2];
			Assert.AreEqual("System Administrator", resourcesRow.FullName);
			Assert.AreEqual("Product Owner", resourcesRow.ProjectRoleName);
			Assert.AreEqual(null, resourcesRow.ResourceEffort);
			Assert.AreEqual(0, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(0, resourcesRow.TotalEffort);
			Assert.AreEqual(0, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(null, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);

			//Now lets retrieve the resource usage for a release
			projectResources = projectManager.RetrieveResources(projectId, releaseId);
			Assert.AreEqual(3, projectResources.Count);
			resourcesRow = projectResources[0];
			Assert.AreEqual("System Administrator", resourcesRow.FullName);
			Assert.AreEqual("Product Owner", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(0, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(0, resourcesRow.TotalEffort);
			Assert.AreEqual(0, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(10560, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[1];
			Assert.AreEqual("Fred Bloggs", resourcesRow.FullName);
			Assert.AreEqual("Manager", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(60, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(1260, resourcesRow.IncidentEffort);
			Assert.AreEqual(1320, resourcesRow.TotalEffort);
			Assert.AreEqual(1260, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(9240, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[2];
			Assert.AreEqual("Joe Smith", resourcesRow.FullName);
			Assert.AreEqual("Observer", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(2880, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(2880, resourcesRow.TotalEffort);
			Assert.AreEqual(1920, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(7680, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);

			//Now lets retrieve the resource uage for one of its child iterations
			projectResources = projectManager.RetrieveResources(projectId, iterationId);
			Assert.AreEqual(3, projectResources.Count);
			resourcesRow = projectResources[0];
			Assert.AreEqual("System Administrator", resourcesRow.FullName);
			Assert.AreEqual("Product Owner", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(0, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(0, resourcesRow.TotalEffort);
			Assert.AreEqual(0, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(10560, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[1];
			Assert.AreEqual("Fred Bloggs", resourcesRow.FullName);
			Assert.AreEqual("Manager", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(60, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(60, resourcesRow.TotalEffort);
			Assert.AreEqual(0, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(10500, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);
			resourcesRow = projectResources[2];
			Assert.AreEqual("Joe Smith", resourcesRow.FullName);
			Assert.AreEqual("Observer", resourcesRow.ProjectRoleName);
			Assert.AreEqual(10560, resourcesRow.ResourceEffort);
			Assert.AreEqual(1920, resourcesRow.ReqTaskEffort);
			Assert.AreEqual(0, resourcesRow.IncidentEffort);
			Assert.AreEqual(1920, resourcesRow.TotalEffort);
			Assert.AreEqual(1920, resourcesRow.TotalOpenEffort);
			Assert.AreEqual(8640, resourcesRow.RemainingEffort);
			Assert.AreEqual(false, resourcesRow.IsOverAllocated);

			//Now we need to test that we can filter and sort the results
			Hashtable filters = new Hashtable();

			//Sort by user id ascending, no filter
			projectResources = projectManager.RetrieveResources(projectId, null, "UserId", true, filters);
			Assert.AreEqual(3, projectResources.Count);
			Assert.AreEqual(1, projectResources[0].UserId);
			Assert.AreEqual(2, projectResources[1].UserId);
			Assert.AreEqual(3, projectResources[2].UserId);

			//Sort by user id descending, no filter
			projectResources = projectManager.RetrieveResources(projectId, null, "UserId", false, filters);
			Assert.AreEqual(3, projectResources.Count);
			Assert.AreEqual(3, projectResources[0].UserId);
			Assert.AreEqual(2, projectResources[1].UserId);
			Assert.AreEqual(1, projectResources[2].UserId);

			//Sort by total effort ascending, no filter
			projectResources = projectManager.RetrieveResources(projectId, null, "TotalEffort", true, filters);
			Assert.AreEqual(3, projectResources.Count);
			Assert.AreEqual(1, projectResources[0].UserId);
			Assert.AreEqual(2, projectResources[1].UserId);
			Assert.AreEqual(3, projectResources[2].UserId);

			//Sort by total effort descending, no filter
			projectResources = projectManager.RetrieveResources(projectId, null, "TotalEffort", false, filters);
			Assert.AreEqual(3, projectResources.Count);
			Assert.AreEqual(3, projectResources[0].UserId);
			Assert.AreEqual(2, projectResources[1].UserId);
			Assert.AreEqual(1, projectResources[2].UserId);

			//Filtering by name, sorting by full name ascending
			filters.Clear();
			filters.Add("FullName", "fred");
			projectResources = projectManager.RetrieveResources(projectId, null, "FullName", true, filters);
			Assert.AreEqual(1, projectResources.Count);
			Assert.AreEqual(2, projectResources[0].UserId);

			//Filtering by role, sorting by full name ascending
			filters.Clear();
			filters.Add("ProjectRoleId", 1);
			projectResources = projectManager.RetrieveResources(projectId, null, "FullName", true, filters);
			Assert.AreEqual(1, projectResources.Count);
			Assert.AreEqual(1, projectResources[0].UserId);

			//Filtering by req/task effort, sorting by full name ascending
			EffortRange effortRange = new EffortRange();
			effortRange.MaxValue = 50;
			effortRange.MinValue = 1;
			filters.Clear();
			filters.Add("ReqTaskEffort", effortRange);
			projectResources = projectManager.RetrieveResources(projectId, null, "FullName", true, filters);
			Assert.AreEqual(2, projectResources.Count);
			Assert.AreEqual(2, projectResources[0].UserId);
			Assert.AreEqual(3, projectResources[1].UserId);

			//Filtering by total effort, sorting by full name ascending
			effortRange = new EffortRange();
			effortRange.MaxValue = 30;
			effortRange.MinValue = 1;
			filters.Clear();
			filters.Add("TotalEffort", effortRange);
			projectResources = projectManager.RetrieveResources(projectId, null, "FullName", true, filters);
			Assert.AreEqual(1, projectResources.Count);
			Assert.AreEqual(2, projectResources[0].UserId);

			//Filtering by allocation, sorting by full name ascending - for Release 1.0.0.0
			filters.Clear();
			filters.Add("AllocationIndicator", 2);  // >= 25%
			projectResources = projectManager.RetrieveResources(projectId, releaseId, "FullName", true, filters);
			Assert.AreEqual(1, projectResources.Count);
			Assert.AreEqual(3, projectResources[0].UserId);

			//Finally delete the project and template
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId);
			templateManager.Delete(USER_ID_FRED_BLOGGS, projectTemplateId);
		}

		/// <summary>
		/// Tests that we can configure which projects can share artifacts with each other
		/// </summary>
		[
		Test,
		SpiraTestCase(1505)
		]
		public void _18_CrossProjectAssociationMangement()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First create a new project that we can associate artifacts with
			int destProjectId = projectManager.Insert("Dest Project", null, null, null, true, null, 1, adminSectionId, "Inserted Project");
			int destTemplateId = templateManager.RetrieveForProject(destProjectId).ProjectTemplateId;

			//Verify that nothing is shared with this project
			List<ProjectArtifactSharing> projectAssociations = projectManager.ProjectAssociation_RetrieveForProject(PROJECT_ID);
			Assert.IsFalse(projectAssociations.Any(p => p.DestProjectId == destProjectId));

			//Verify using boolean method
			bool canShare = projectManager.ProjectAssociation_CanProjectShare(destProjectId, PROJECT_ID, Artifact.ArtifactTypeEnum.Requirement);
			Assert.IsFalse(canShare);

			//Add a new association
			projectManager.ProjectAssociation_Add(PROJECT_ID, destProjectId, new List<int>() { 1, 2, 3 });

			//Verify the associations
			projectAssociations = projectManager.ProjectAssociation_RetrieveForProject(PROJECT_ID);
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 1));
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 2));
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 3));

			//Verify using boolean method
			canShare = projectManager.ProjectAssociation_CanProjectShare(destProjectId, PROJECT_ID, Artifact.ArtifactTypeEnum.Requirement);
			Assert.IsTrue(canShare);

			//Verify that the artifact and project names are included
			ProjectArtifactSharing projectAssociation = projectAssociations.Single(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 1);
			Assert.AreEqual("Requirement", projectAssociation.ArtifactType.Name);
			Assert.AreEqual("Dest Project", projectAssociation.DestProject.Name);

			//Verify the other retrieval methods
			//Retrieve by destination project and artifact type
			projectAssociations = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifact(destProjectId, Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(1, projectAssociations.Count);
			Assert.IsTrue(projectAssociations.Any(p => p.SourceProjectId == PROJECT_ID && p.ArtifactTypeId == 1));

			//Retrieve by destination project and artifact type
			projectAssociations = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifacts(destProjectId, new List<int>() { 1, 2 });
			Assert.AreEqual(2, projectAssociations.Count);
			Assert.IsTrue(projectAssociations.Any(p => p.SourceProjectId == PROJECT_ID && p.ArtifactTypeId == 1));
			Assert.IsTrue(projectAssociations.Any(p => p.SourceProjectId == PROJECT_ID && p.ArtifactTypeId == 2));

			//Retrieve by destination project including destination project itself
			List<Project> projects = projectManager.ProjectAssociation_RetrieveForDestProjectIncludingSelf(destProjectId);
			Assert.AreEqual(2, projects.Count);
			Assert.IsTrue(projects.Any(p => p.ProjectId == PROJECT_ID));
			Assert.IsTrue(projects.Any(p => p.ProjectId == destProjectId));

			//Retrieve by source project
			projects = projectManager.ProjectAssociation_RetrieveForSourceProject(PROJECT_ID);
			Assert.AreEqual(3, projects.Count);
			Assert.IsTrue(projects.Any(p => p.ProjectId == destProjectId));

			//Modify the association
			projectManager.ProjectAssociation_Update(PROJECT_ID, destProjectId, new List<int>() { 1, 2, 4 });

			//Verify the associations
			projectAssociations = projectManager.ProjectAssociation_RetrieveForProject(PROJECT_ID);
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 1));
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 2));
			Assert.IsTrue(projectAssociations.Any(p => p.DestProjectId == destProjectId && p.ArtifactTypeId == 4));

			//Remove all the project associations for these two projects
			projectManager.ProjectAssociation_Delete(PROJECT_ID, destProjectId);

			//Verify that nothing is shared with this project
			projectAssociations = projectManager.ProjectAssociation_RetrieveForProject(PROJECT_ID);
			Assert.IsFalse(projectAssociations.Any(p => p.DestProjectId == destProjectId));

			//Verify using boolean method
			canShare = projectManager.ProjectAssociation_CanProjectShare(destProjectId, PROJECT_ID, Artifact.ArtifactTypeEnum.Requirement);
			Assert.IsFalse(canShare);

			//Finally delete the new project and template
			projectManager.Delete(USER_ID_SYS_ADMIN, destProjectId);
			templateManager.Delete(USER_ID_SYS_ADMIN, destTemplateId);
		}

		/// <summary>
		/// Tests that we can manage project settings using the special ProjectSettings configuration provider
		/// </summary>
		[
		Test,
		SpiraTestCase(2391)
		]
		public void _19_StoreRetrieveProjectSettings()
		{
			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Setup - create two new projects, they can both use template 1 in this case
			projectId_forSettings1 = projectManager.Insert("Settings Project 1", null, null, null, true, 1, 1, adminSectionId, "Inserted Project");
			projectId_forSettings2 = projectManager.Insert("Settings Project 2", null, null, null, true, 1, 1, adminSectionId, "Inserted Project");

			//First, get the default settings values for a project
			ProjectSettings projectSettings = new ProjectSettings(projectId_forSettings1);
			Assert.IsNotNull(projectSettings);
			Assert.IsFalse(projectSettings.BaseliningEnabled);
			Assert.IsTrue(projectSettings.DisplayHoursOnPlanningBoard);

			//Next test that we can store some settings
			projectSettings.BaseliningEnabled = true;
			projectSettings.Save(USER_ID_JOE_SMITH);

			//Verify
			projectSettings = new ProjectSettings(projectId_forSettings1);
			Assert.IsNotNull(projectSettings);
			Assert.IsTrue(projectSettings.BaseliningEnabled);
			Assert.IsTrue(projectSettings.DisplayHoursOnPlanningBoard);

			//Next test that the settings are truly project-specific
			projectSettings = new ProjectSettings(projectId_forSettings2);
			Assert.IsNotNull(projectSettings);
			Assert.IsFalse(projectSettings.BaseliningEnabled);
			Assert.IsTrue(projectSettings.DisplayHoursOnPlanningBoard);

			//Next test that we can store some settings in this project
			projectSettings.DisplayHoursOnPlanningBoard = false;
			projectSettings.Save(USER_ID_SYS_ADMIN);

			//Verify
			projectSettings = new ProjectSettings(projectId_forSettings2);
			Assert.IsNotNull(projectSettings);
			Assert.IsFalse(projectSettings.BaseliningEnabled);
			Assert.IsFalse(projectSettings.DisplayHoursOnPlanningBoard);

			//Verify original project
			projectSettings = new ProjectSettings(projectId_forSettings1);
			Assert.IsNotNull(projectSettings);
			Assert.IsTrue(projectSettings.BaseliningEnabled);
			Assert.IsTrue(projectSettings.DisplayHoursOnPlanningBoard);

			//Test that if we clone/created a project based on existing, the settings get cloned
			projectId_forSettings3 = projectManager.CreateFromExisting("Settings Project 3", null, null, projectId_forSettings1);

			//Verify settings
			projectSettings = new ProjectSettings(projectId_forSettings3);
			Assert.IsNotNull(projectSettings);
			Assert.IsTrue(projectSettings.BaseliningEnabled);
			Assert.IsTrue(projectSettings.DisplayHoursOnPlanningBoard);

			//Finally check that you cannot use the 'default' static instance of the project settings class
			bool exceptionCaught = false;
			try
			{
				projectSettings = ProjectSettings.Default;
				projectSettings.DisplayHoursOnPlanningBoard = false;
				projectSettings.Save(USER_ID_SYS_ADMIN);
			}
			catch (InvalidOperationException)
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught);
		}

		/// <summary>
		/// Tests that the system can track the last projects you viewed
		/// </summary>
		[
		Test,
		SpiraTestCase(2743)
		]
		public void _20_RecentProjects()
		{
			//First delete all the existing items (if there are any)
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER_RECENT_PROJECT");

			//Now verify that there are no entries
			List<UserRecentProject> recentProjects = projectManager.RetrieveRecentProjectsForUser(USER_ID_FRED_BLOGGS, 100);
			Assert.AreEqual(0, recentProjects.Count);

			//Register a couple of projects
			projectManager.AddUpdateRecentProject(USER_ID_FRED_BLOGGS, 1);
			projectManager.AddUpdateRecentProject(USER_ID_FRED_BLOGGS, 2);
			projectManager.AddUpdateRecentProject(USER_ID_FRED_BLOGGS, 3);

			//Verify the list and latest entry
			recentProjects = projectManager.RetrieveRecentProjectsForUser(USER_ID_FRED_BLOGGS, 100);
			Assert.AreEqual(3, recentProjects.Count);
			UserRecentProject recentProject = recentProjects.First();
			Assert.AreEqual(3, recentProject.ProjectId);

			//Now update an existing artifact in the list (makes it more recent)
			projectManager.AddUpdateRecentProject(USER_ID_FRED_BLOGGS, 1);

			//Verify the list
			recentProjects = projectManager.RetrieveRecentProjectsForUser(USER_ID_FRED_BLOGGS, 100);
			Assert.AreEqual(3, recentProjects.Count);
			recentProject = recentProjects.First();
			Assert.AreEqual(1, recentProject.ProjectId);

			//Clean up
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER_RECENT_PROJECT");
		}
	}
}
