using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>This fixture tests the Project Group business object</summary>
	[TestFixture]
	[SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class ProjectGroupTest
	{
		protected static ProjectGroupManager projectGroupManager;
		protected static int projectGroupId;
		protected static int projectId;
		protected static int projectTemplateId;
		protected static long lastArtifactHistoryId;

		private const int USER_ID_SYS_ADMIN = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int USER_ID_ROGER_RAMJET = 4;

		[TestFixtureSetUp]
		public void Init()
		{
			projectGroupManager = new Business.ProjectGroupManager();
		}

		/// <summary>
		/// Tests that you can retrieve a list of project groups
		/// </summary>
		[
		Test,
		SpiraTestCase(509)
		]
		public void _01_RetrieveProjectGroups()
		{
			//First lets test that we can retrieve a single project group record
			ProjectGroup projectGroup = projectGroupManager.RetrieveById(2);
			Assert.AreEqual(2, projectGroup.ProjectGroupId);
			Assert.AreEqual("Sample Program", projectGroup.Name);
			Assert.AreEqual("Contains products related to customers, relationships and contacts", projectGroup.Description);
			Assert.AreEqual("www.libraryinformationsystem.org", projectGroup.Website);
			Assert.AreEqual(true, projectGroup.IsActive);
			Assert.AreEqual(false, projectGroup.IsDefault);

			//Next make sure that attempting to retrieve a non-existing project group throws an exception
			bool exceptionThrown = false;
			try
			{
				projectGroupManager.RetrieveById(100);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Did not throw ArtifactNotExistsException as expected");

			//Now lets test that we can retrieve the list of project groups in the system
			List<ProjectGroup> projectGroups = projectGroupManager.Retrieve(null, null, null);
			Assert.AreEqual(4, projectGroups.Count);
			Assert.AreEqual("(Default Program)", projectGroups[0].Name);
			Assert.AreEqual("Corporate Systems", projectGroups[1].Name);
			Assert.AreEqual("Sales and Marketing", projectGroups[2].Name);

			//Now lets test that we can retrieve the list of project groups in the system filtered
			Hashtable filters = new Hashtable();
			filters.Add("Name", "S");
			filters.Add("WebSite", "w");
			filters.Add("ActiveYn", "Y");
			filters.Add("DefaultYn", "N");
			projectGroups = projectGroupManager.Retrieve(filters, null, null);
			Assert.AreEqual(1, projectGroups.Count);
			Assert.AreEqual("Sample Program", projectGroups[0].Name);
			Assert.AreEqual(true, projectGroups[0].IsActive);
			Assert.AreEqual(false, projectGroups[0].IsDefault);

			//We need to test the sorting
			projectGroups = projectGroupManager.Retrieve(null, "Name ASC");
			Assert.AreEqual("(Default Program)", projectGroups[0].Name);
			Assert.AreEqual("Corporate Systems", projectGroups[1].Name);
			Assert.AreEqual("Sales and Marketing", projectGroups[2].Name);

			projectGroups = projectGroupManager.Retrieve(null, "Name DESC");
			Assert.AreEqual("Sample Program", projectGroups[0].Name);
			Assert.AreEqual("Sales and Marketing", projectGroups[1].Name);
			Assert.AreEqual("Corporate Systems", projectGroups[2].Name);

			projectGroups = projectGroupManager.Retrieve(null, "Website ASC");
			Assert.AreEqual("(Default Program)", projectGroups[0].Name);
			Assert.AreEqual("Corporate Systems", projectGroups[1].Name);
			Assert.AreEqual("Sales and Marketing", projectGroups[2].Name);

			projectGroups = projectGroupManager.Retrieve(null, "ProjectGroupId DESC");
			Assert.AreEqual("Sales and Marketing", projectGroups[0].Name);
			Assert.AreEqual("Corporate Systems", projectGroups[1].Name);
			Assert.AreEqual("Sample Program", projectGroups[2].Name);

			//Next lets test that we can retrieve just the project groups that the user is an owner of
			projectGroups = projectGroupManager.Retrieve(null, null, USER_ID_FRED_BLOGGS);
			Assert.AreEqual(1, projectGroups.Count);
			Assert.AreEqual("Sample Program", projectGroups[0].Name);

			//Next lets test that we can retrieve all the project groups that the user is a member (not necessarily owner) of
			List<ProjectGroupUserView> projectGroupsForUser = projectGroupManager.RetrieveForUser(USER_ID_FRED_BLOGGS);
			Assert.AreEqual(2, projectGroupsForUser.Count);
			Assert.AreEqual("Corporate Systems", projectGroupsForUser[0].ProjectGroupName);
			Assert.AreEqual("Executive", projectGroupsForUser[0].ProjectGroupRoleName);
			Assert.AreEqual("Sample Program", projectGroupsForUser[1].ProjectGroupName);
			Assert.AreEqual("Program Owner", projectGroupsForUser[1].ProjectGroupRoleName);
		}

		/// <summary>
		/// Tests that you can create and modify the project groups
		/// </summary>
		[Test]
		[SpiraTestCase(510)]
		public void _02_CreateAndModifyProjectGroups()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Programs";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//First lets test that we can create a new project group that becomes the default
			projectGroupId = projectGroupManager.Insert("New Group 1", "Description 1", null, true, true, 1, 1, 1, adminSectionId, "Inserted Program");

			//Verify that the new project group was inserted correctly
			ProjectGroup projectGroup = projectGroupManager.RetrieveById(projectGroupId);
			Assert.AreEqual(projectGroupId, projectGroup.ProjectGroupId);
			Assert.AreEqual("New Group 1", projectGroup.Name);
			Assert.AreEqual("Description 1", projectGroup.Description);
			Assert.IsTrue(projectGroup.Website.IsNull());
			Assert.AreEqual(true, projectGroup.IsActive);
			Assert.AreEqual(true, projectGroup.IsDefault);

			//Now verify that the old default project group is no longer the default
			projectGroup = projectGroupManager.RetrieveById(1);
			Assert.AreEqual(1, projectGroup.ProjectGroupId);
			Assert.AreEqual("(Default Program)", projectGroup.Name);
			Assert.AreEqual(false, projectGroup.IsDefault);

			//Now edit the group to change some of the information
			projectGroup = projectGroupManager.RetrieveById(projectGroupId);
			projectGroup.StartTracking();
			projectGroup.Name = "New Group 1a";
			projectGroup.Description = null;
			projectGroup.Website = "http://tempuri.org";
			projectGroupManager.Update(projectGroup, 1, adminSectionId, "Updated Program");

			//Verify the changes
			projectGroup = projectGroupManager.RetrieveById(projectGroupId);
			Assert.AreEqual(projectGroupId, projectGroup.ProjectGroupId);
			Assert.AreEqual("New Group 1a", projectGroup.Name);
			Assert.IsTrue(projectGroup.Description.IsNull());
			Assert.AreEqual("http://tempuri.org", projectGroup.Website);
			Assert.AreEqual(true, projectGroup.IsActive);
			Assert.AreEqual(true, projectGroup.IsDefault);

			//Now change it back
			projectGroup.StartTracking();
			projectGroup.Name = "New Group 1";
			projectGroup.Description = "Description 1";
			projectGroup.Website = null;
			projectGroupManager.Update(projectGroup, 1, adminSectionId, "Updated Program");

			//Verify the changes
			projectGroup = projectGroupManager.RetrieveById(projectGroupId);
			Assert.AreEqual(projectGroupId, projectGroup.ProjectGroupId);
			Assert.AreEqual("New Group 1", projectGroup.Name);
			Assert.AreEqual("Description 1", projectGroup.Description);
			Assert.IsTrue(projectGroup.Website.IsNull());
			Assert.AreEqual(true, projectGroup.IsActive);
			Assert.AreEqual(true, projectGroup.IsDefault);

			//Verify that you can't make the default group inactive
			projectGroup.StartTracking();
			projectGroup.IsActive = false;
			bool exceptionThrown = false;
			try
			{
				projectGroupManager.Update(projectGroup, 1, adminSectionId, "Updated Program");
			}
			catch (ProjectGroupDefaultException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make the default group inactive");

			//Verify that making a different group the default, makes all others default = no
			//This also cleans the data by making project group id the default again
			projectGroup = projectGroupManager.RetrieveById(1);
			projectGroup.StartTracking();
			projectGroup.IsDefault = true;
			projectGroupManager.Update(projectGroup, 1, adminSectionId, "Updated Program");

			//Now verify that our new group is now not the default
			projectGroup = projectGroupManager.RetrieveById(projectGroupId);
			Assert.AreEqual(projectGroupId, projectGroup.ProjectGroupId);
			Assert.AreEqual("New Group 1", projectGroup.Name);
			Assert.AreEqual("Description 1", projectGroup.Description);
			Assert.IsTrue(projectGroup.Website.IsNull());
			Assert.AreEqual(true, projectGroup.IsActive);
			Assert.AreEqual(false, projectGroup.IsDefault);

			//Now lets add a project to the group and verify that we can get the list of projects in a group
			ProjectManager projectManager = new ProjectManager();
			Business.AdminAuditManager adminAuditManager1 = new Business.AdminAuditManager();

			string adminSectionName1 = "View / Edit Projects";
			var adminSection1 = adminAuditManager1.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection.ADMIN_SECTION_ID;

			projectId = projectManager.Insert("New Project", projectGroupId, null, null, true, null, 1, adminSectionId1, "Inserted Project");
			projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
			Hashtable filters = new Hashtable();
			filters.Add("ProjectGroupId", projectGroupId);
			List<ProjectView> projects = projectManager.Retrieve(filters, null);
			Assert.AreEqual(1, projects.Count);
			Assert.AreEqual("New Project", projects[0].Name);
			Assert.AreEqual(projectGroupId, projects[0].ProjectGroupId);

			//Finally verify that you can't insert a new project that is the default but not active
			exceptionThrown = false;
			try
			{
				projectGroupManager.Insert("New Group 2", "Description 2", null, false, true, null, 1, 1, adminSectionId, "Inserted Program");
			}
			catch (ProjectGroupDefaultException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Should not be able to make the default group inactive");
		}

		/// <summary>
		/// Tests that you can delete project groups, and that the default group cannot be deleted
		/// </summary>
		[
		Test,
		SpiraTestCase(511)
		]
		public void _03_DeleteProjectGroups()
		{
			//Delete the newly created project group
			projectGroupManager.Delete(projectGroupId, 1);

			//Verify that it was deleted
			bool exceptionThrown = false;
			try
			{
				projectGroupManager.RetrieveById(projectGroupId);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Project group was not successfully deleted");

			//Verify that any associated projects were ressigned to the default project group
			int defaultProjectGroupId = projectGroupManager.GetDefault();
			ProjectManager projectManager = new ProjectManager();
			Hashtable filters = new Hashtable();
			filters.Add("ProjectGroupId", defaultProjectGroupId);
			List<ProjectView> projects = projectManager.Retrieve(filters, null);
			Assert.AreEqual(1, projects.Count);
			Assert.AreEqual("New Project", projects[0].Name);
			Assert.AreEqual(defaultProjectGroupId, projects[0].ProjectGroupId);

			//Now delete this project as no longer needed
			projectManager.Delete(UserManager.UserSystemAdministrator, projects[0].ProjectId);
			new TemplateManager().Delete(UserManager.UserSystemAdministrator, projectTemplateId);

			//Test that you can't delete the default project group
			exceptionThrown = false;
			try
			{
				projectGroupManager.Delete(defaultProjectGroupId, 1);
			}
			catch (ProjectGroupDefaultException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Cannot delete the default project");

			//Test that trying to delete a non-existant project group throws a business exception
			exceptionThrown = false;
			try
			{
				projectGroupManager.Delete(100, 1);
			}
			catch (ArtifactNotExistsException)
			{
				exceptionThrown = true;
			}
			Assert.IsTrue(exceptionThrown, "Cannot delete an non-existant project");
		}

		/// <summary>
		/// Tests that you can edit the users associated with a project group
		/// </summary>
		[
		Test,
		SpiraTestCase(512)
		]
		public void _04_ViewAndEditGroupMembership()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//First lets get the list of users that are members of one of the sample project groups
			ProjectGroup projectGroup = projectGroupManager.RetrieveById(2, true);
			Assert.AreEqual("Sample Program", projectGroup.Name);

			//Verify the user membership
			Assert.AreEqual(2, projectGroup.Users.Count);
			ProjectGroupUser projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual("Fred Bloggs", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_SYS_ADMIN);
			Assert.AreEqual("System Administrator", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);

			//Now lets add a user to the membership
			List<ProjectGroupUser> projectGroupUsers = projectGroupManager.RetrieveUserMembership(2);
			ProjectGroupUser newProjectGroupUser = new ProjectGroupUser();
			newProjectGroupUser.MarkAsAdded();
			newProjectGroupUser.ProjectGroupId = 2;
			newProjectGroupUser.UserId = USER_ID_JOE_SMITH;
			newProjectGroupUser.ProjectGroupRoleId = (int)ProjectGroup.ProjectGroupRoleEnum.Executive;
			projectGroupUsers.Add(newProjectGroupUser);
			projectGroupManager.SaveUserMembership(projectGroupUsers, USER_ID_FRED_BLOGGS, adminSectionId, "Updated UserMembership in Program");

			//Verify the changes
			projectGroup = projectGroupManager.RetrieveById(2, true);
			Assert.AreEqual(3, projectGroup.Users.Count);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual("Fred Bloggs", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_JOE_SMITH);
			Assert.AreEqual("Joe P Smith", projectGroupUser.FullName);
			Assert.AreEqual("Executive", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_SYS_ADMIN);
			Assert.AreEqual("System Administrator", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);

			//Now lets change that user's role
			projectGroupUsers = projectGroupManager.RetrieveUserMembership(2);
			projectGroupUsers.FirstOrDefault(p => p.UserId == USER_ID_JOE_SMITH).MarkAsDeleted();
			newProjectGroupUser = new ProjectGroupUser();
			newProjectGroupUser.MarkAsAdded();
			newProjectGroupUser.ProjectGroupId = 2;
			newProjectGroupUser.UserId = USER_ID_JOE_SMITH;
			newProjectGroupUser.ProjectGroupRoleId = (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner;
			projectGroupUsers.Add(newProjectGroupUser);
			projectGroupManager.SaveUserMembership(projectGroupUsers, USER_ID_JOE_SMITH, adminSectionId, "Updated UserMembership in Program");

			//Verify the changes
			projectGroup = projectGroupManager.RetrieveById(2, true);
			Assert.AreEqual(3, projectGroup.Users.Count);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual("Fred Bloggs", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_JOE_SMITH);
			Assert.AreEqual("Joe P Smith", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_SYS_ADMIN);
			Assert.AreEqual("System Administrator", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);

			//Now lets remove that user
			projectGroupUsers = projectGroupManager.RetrieveUserMembership(2);
			projectGroupUsers.FirstOrDefault(p => p.UserId == USER_ID_JOE_SMITH).MarkAsDeleted();
			projectGroupManager.SaveUserMembership(projectGroupUsers, USER_ID_JOE_SMITH, adminSectionId, "Updated UserMembership in Program");

			//Verify the changes
			projectGroup = projectGroupManager.RetrieveById(2, true);
			Assert.AreEqual(2, projectGroup.Users.Count);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual("Fred Bloggs", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);
			projectGroupUser = projectGroup.Users.FirstOrDefault(u => u.UserId == USER_ID_SYS_ADMIN);
			Assert.AreEqual("System Administrator", projectGroupUser.FullName);
			Assert.AreEqual("Program Owner", projectGroupUser.ProjectGroupRoleName);

			//Now we need to verify that we can see if a user is an owner of any project group
			//this is needed to know if they should see the Administration link or not
			bool isAdmin = projectGroupManager.IsAdmin(USER_ID_FRED_BLOGGS);
			Assert.IsTrue(isAdmin, "Fred Bloggs");
			isAdmin = projectGroupManager.IsAdmin(USER_ID_JOE_SMITH);
			Assert.IsFalse(isAdmin, "Joe Smith");

			//Now we need to verify that we can see if a user is a member of any project group
			//this is needed to know if they should see the Project Group Dashboard link or not
			bool isAuthorized = projectGroupManager.IsAuthorized(USER_ID_FRED_BLOGGS, 2);
			Assert.IsTrue(isAuthorized, "Fred Bloggs");
			isAuthorized = projectGroupManager.IsAuthorized(USER_ID_JOE_SMITH, 2);
			Assert.IsFalse(isAuthorized, "Joe Smith");

			//Verify that we can get the list of project group roles
			//First active roles
			List<ProjectGroupRole> projectGroupRoles = projectGroupManager.RetrieveRoles(true);
			Assert.AreEqual(2, projectGroupRoles.Count);
			Assert.AreEqual("Executive", projectGroupRoles[0].Name);
			Assert.AreEqual("Program Owner", projectGroupRoles[1].Name);

			//Next all roles
			projectGroupRoles = projectGroupManager.RetrieveRoles(false);
			Assert.AreEqual(2, projectGroupRoles.Count);
			Assert.AreEqual("Executive", projectGroupRoles[0].Name);
			Assert.AreEqual("Program Owner", projectGroupRoles[1].Name);

			//Verify if they are an admin of a specific group.
			isAdmin = projectGroupManager.IsAdmin(USER_ID_FRED_BLOGGS, 2);
			Assert.IsTrue(isAdmin, "Fred Bloggs");
			isAdmin = projectGroupManager.IsAdmin(USER_ID_JOE_SMITH, 2);
			Assert.IsFalse(isAdmin, "Joe Smith");
		}

		/// <summary>
		/// Tests that you can retrieve all the resources in the project group
		/// </summary>
		[
		Test,
		SpiraTestCase(1178)
		]
		public void _05_RetrieveGroupResources()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName1 = "View / Edit Programs";
			var adminSection1 = adminAuditManager.AdminSection_RetrieveByName(adminSectionName1);

			int adminSectionId1 = adminSection1.ADMIN_SECTION_ID;
			//So that we can get stable data we need to create two new projects in a new group
			int projectGroupId = projectGroupManager.Insert("RetrieveGroupResources Group", "", "", true, false, 1, 1, 1, adminSectionId1, "Inserted Program");
			ProjectManager projectManager = new ProjectManager();

			string adminSectionName = "View / Edit Projects";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			int projectId1 = projectManager.Insert("RetrieveGroupResources Project 1", projectGroupId, "", "", true, null, 1, adminSectionId, "Inserted Project");
			int projectId2 = projectManager.Insert("RetrieveGroupResources Project 2", projectGroupId, "", "", true, null, 1, adminSectionId, "Inserted Project");

			//Get the templates associated with the projects
			TemplateManager templateManager = new TemplateManager();
			int projectTemplateId1 = templateManager.RetrieveForProject(projectId1).ProjectTemplateId;
			int projectTemplateId2 = templateManager.RetrieveForProject(projectId2).ProjectTemplateId;

			//Now add some users to the projects
			projectManager.InsertUserMembership(USER_ID_FRED_BLOGGS, projectId1, 2);    //Manager
			projectManager.InsertUserMembership(USER_ID_FRED_BLOGGS, projectId2, 2);    //Manager
			projectManager.InsertUserMembership(USER_ID_JOE_SMITH, projectId1, 5);    //Observer
			projectManager.InsertUserMembership(USER_ID_JOE_SMITH, projectId2, 5);    //Observer

			//Next the requirements, tasks and incidents
			RequirementManager requirementManager = new RequirementManager();
			TaskManager taskManager = new TaskManager();
			IncidentManager incidentManager = new IncidentManager();
			int requirementId1 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId1, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId2 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId1, null, null, (int?)null, Requirement.RequirementStatusEnum.Completed, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int taskId1 = taskManager.Insert(projectId1, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, null, requirementId1, null, USER_ID_FRED_BLOGGS, null, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int taskId2 = taskManager.Insert(projectId1, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId1, null, USER_ID_JOE_SMITH, null, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int incidentId1 = incidentManager.Insert(projectId1, null, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Incident 1", "Really bad incident", null, null, null, null, null, DateTime.Now, DateTime.Parse("3/5/2009"), null, 1200, 1220, 40, null, null, USER_ID_FRED_BLOGGS);
			int requirementId3 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, null, null, (int?)null, Requirement.RequirementStatusEnum.Planned, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int requirementId4 = requirementManager.Insert(USER_ID_FRED_BLOGGS, projectId2, null, null, (int?)null, Requirement.RequirementStatusEnum.Completed, null, USER_ID_FRED_BLOGGS, USER_ID_JOE_SMITH, null, "Test Req 1", null, 2.0M, USER_ID_FRED_BLOGGS);
			int taskId3 = taskManager.Insert(projectId2, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.Completed, null, null, requirementId3, null, USER_ID_FRED_BLOGGS, null, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int taskId4 = taskManager.Insert(projectId2, USER_ID_FRED_BLOGGS, Task.TaskStatusEnum.NotStarted, null, null, requirementId3, null, USER_ID_JOE_SMITH, null, "Task 1", "", DateTime.Parse("3/2/2009"), DateTime.Parse("3/10/2009"), 1920, 60, 1920, USER_ID_FRED_BLOGGS);
			int incidentId2 = incidentManager.Insert(projectId2, null, null, USER_ID_FRED_BLOGGS, USER_ID_FRED_BLOGGS, null, "Incident 1", "Really bad incident", null, null, null, null, null, DateTime.Now, DateTime.Parse("3/5/2009"), null, 1200, 1220, 40, null, null, USER_ID_FRED_BLOGGS);

			List<ProjectResourceView> projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId);
			Assert.AreEqual(3, projectGroupResources.Count);

			//Row 0
			ProjectResourceView projectGroupResource = projectGroupResources[0];
			Assert.AreEqual("Fred Bloggs", projectGroupResource.FullName);
			Assert.AreEqual("", projectGroupResource.ProjectRoleName);
			Assert.AreEqual(null, projectGroupResource.ResourceEffort);
			Assert.AreEqual(60 * 2, projectGroupResource.ReqTaskEffort);
			Assert.AreEqual(1260 * 2, projectGroupResource.IncidentEffort);
			Assert.AreEqual(1320 * 2, projectGroupResource.TotalEffort);
			Assert.AreEqual(1260 * 2, projectGroupResource.TotalOpenEffort);
			Assert.AreEqual(null, projectGroupResource.RemainingEffort);
			Assert.AreEqual(false, projectGroupResource.IsOverAllocated);

			//Row 1
			projectGroupResource = projectGroupResources[1];
			Assert.AreEqual("Joe Smith", projectGroupResource.FullName);
			Assert.AreEqual("", projectGroupResource.ProjectRoleName);
			Assert.AreEqual(null, projectGroupResource.ResourceEffort);
			Assert.AreEqual(2880 * 2, projectGroupResource.ReqTaskEffort);
			Assert.AreEqual(0 * 2, projectGroupResource.IncidentEffort);
			Assert.AreEqual(2880 * 2, projectGroupResource.TotalEffort);
			Assert.AreEqual(1920 * 2, projectGroupResource.TotalOpenEffort);
			Assert.AreEqual(null, projectGroupResource.RemainingEffort);
			Assert.AreEqual(false, projectGroupResource.IsOverAllocated);

			//Row 2
			projectGroupResource = projectGroupResources[2];
			Assert.AreEqual("System Administrator", projectGroupResource.FullName);
			Assert.AreEqual("", projectGroupResource.ProjectRoleName);
			Assert.AreEqual(null, projectGroupResource.ResourceEffort);
			Assert.AreEqual(0, projectGroupResource.ReqTaskEffort);
			Assert.AreEqual(0, projectGroupResource.IncidentEffort);
			Assert.AreEqual(0, projectGroupResource.TotalEffort);
			Assert.AreEqual(0, projectGroupResource.TotalOpenEffort);
			Assert.AreEqual(null, projectGroupResource.RemainingEffort);
			Assert.AreEqual(false, projectGroupResource.IsOverAllocated);

			//Now we need to test that we can filter and sort the results
			Hashtable filters = new Hashtable();

			//Sort by user id ascending, no filter
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "UserId", true, filters);
			Assert.AreEqual(3, projectGroupResources.Count);
			Assert.AreEqual(1, projectGroupResources[0].UserId);
			Assert.AreEqual(2, projectGroupResources[1].UserId);
			Assert.AreEqual(3, projectGroupResources[2].UserId);

			//Sort by user id descending, no filter
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "UserId", false, filters);
			Assert.AreEqual(3, projectGroupResources.Count);
			Assert.AreEqual(3, projectGroupResources[0].UserId);
			Assert.AreEqual(2, projectGroupResources[1].UserId);
			Assert.AreEqual(1, projectGroupResources[2].UserId);

			//Sort by total effort ascending, no filter
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "TotalEffort", true, filters);
			Assert.AreEqual(3, projectGroupResources.Count);
			Assert.AreEqual(1, projectGroupResources[0].UserId);
			Assert.AreEqual(2, projectGroupResources[1].UserId);
			Assert.AreEqual(3, projectGroupResources[2].UserId);

			//Sort by total effort descending, no filter
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "TotalEffort", false, filters);
			Assert.AreEqual(3, projectGroupResources.Count);
			Assert.AreEqual(3, projectGroupResources[0].UserId);
			Assert.AreEqual(2, projectGroupResources[1].UserId);
			Assert.AreEqual(1, projectGroupResources[2].UserId);

			//Filtering by name, sorting by full name ascending
			filters.Clear();
			filters.Add("FullName", "fred");
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "FullName", true, filters);
			Assert.AreEqual(1, projectGroupResources.Count);
			Assert.AreEqual(2, projectGroupResources[0].UserId);

			//Filtering by req/task effort, sorting by full name ascending
			EffortRange effortRange = new EffortRange();
			effortRange.MaxValue = 100;
			effortRange.MinValue = 1;
			filters.Clear();
			filters.Add("ReqTaskEffort", effortRange);
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "FullName", true, filters);
			Assert.AreEqual(2, projectGroupResources.Count);
			Assert.AreEqual(2, projectGroupResources[0].UserId);
			Assert.AreEqual(3, projectGroupResources[1].UserId);

			//Filtering by total effort, sorting by full name ascending
			effortRange = new EffortRange();
			effortRange.MaxValue = 60;
			effortRange.MinValue = 1;
			filters.Clear();
			filters.Add("TotalEffort", effortRange);
			projectGroupResources = projectGroupManager.RetrieveResourcesForGroup(projectGroupId, "FullName", true, filters);
			Assert.AreEqual(1, projectGroupResources.Count);
			Assert.AreEqual(2, projectGroupResources[0].UserId);

			//Finally delete the project, templates and the group
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId1);
			projectManager.Delete(USER_ID_SYS_ADMIN, projectId2);
			projectGroupManager.Delete(projectGroupId, 1);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId1);
			templateManager.Delete(USER_ID_SYS_ADMIN, projectTemplateId2);
		}

		/// <summary>
		/// Tests that we can manage program (project group) settings using the special ProjectGroupSettings configuration provider
		/// </summary>
		[
		Test,
		SpiraTestCase(2738)
		]
		public void _06_StoreRetrieveProjectGroupSettings()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

			string adminSectionName = "View / Edit Programs";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Setup - create two new projectGroups
			int projectGroupForSettings1 = projectGroupManager.Insert("Settings ProjectGroup 1", null, null, true, false, 1, 1, 1, adminSectionId, "Inserted Program");
			int projectGroupForSettings2 = projectGroupManager.Insert("Settings ProjectGroup 2", null, null, true, false, 1, 1, 1, adminSectionId, "Inserted Program");

			//First, get the default settings values for a project
			ProjectGroupSettings projectGroupSettings = new ProjectGroupSettings(projectGroupForSettings1);
			Assert.IsNotNull(projectGroupSettings);
			Assert.IsNullOrEmpty(projectGroupSettings.TestSetting);

			//Next test that we can store some settings
			projectGroupSettings.TestSetting = "Test123";
			projectGroupSettings.Save();

			//Verify
			projectGroupSettings = new ProjectGroupSettings(projectGroupForSettings1);
			Assert.IsNotNull(projectGroupSettings);
			Assert.AreEqual("Test123", projectGroupSettings.TestSetting);

			//Next test that the settings are truly project-specific
			projectGroupSettings = new ProjectGroupSettings(projectGroupForSettings2);
			Assert.IsNotNull(projectGroupSettings);
			Assert.IsNullOrEmpty(projectGroupSettings.TestSetting);

			//Finally check that you cannot use the 'default' static instance of the settings class
			bool exceptionCaught = false;
			try
			{
				projectGroupSettings = ProjectGroupSettings.Default;
				projectGroupSettings.TestSetting = "XYZ";
				projectGroupSettings.Save();
			}
			catch (InvalidOperationException)
			{
				exceptionCaught = true;
			}
			Assert.IsTrue(exceptionCaught);

			//Delete the projectGroups
			projectGroupManager.Delete(projectGroupForSettings1, 1);
			projectGroupManager.Delete(projectGroupForSettings2, 1);
		}
	}
}
