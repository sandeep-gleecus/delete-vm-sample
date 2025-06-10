using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Web.Services;
using System.Web.Services.Protocols;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.TestSuite;
using Inflectra.SpiraTest.ApiTestSuite.SpiraImportExport22;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using NUnit.Framework;

namespace Inflectra.SpiraTest.ApiTestSuite.API_Tests
{
	/// <summary>
	/// This fixture tests that the v2.2 import/export web service interface works correctly
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class V2_2_ApiTest
	{
		protected static SpiraImportExport22.ImportExport spiraImportExport;
		protected static CookieContainer cookieContainer;

		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;

		protected static int projectId1;
		protected static int projectId2;
        protected static int projectId3;
        protected static int projectTemplateId1;
        protected static int projectTemplateId3;

        protected static int customListId1;
		protected static int customValueId1;
		protected static int customValueId2;

		protected static int userId1;
		protected static int userId2;

		protected static int releaseId1;
		protected static int iterationId1;
		protected static int iterationId2;

		protected static int requirementId1;
		protected static int requirementId2;
		protected static int requirementId3;
		protected static int requirementId4;
		protected static int requirementId5;

		protected static int testFolderId1;
		protected static int testFolderId2;
		protected static int testCaseId1;
		protected static int testCaseId2;
		protected static int testCaseId3;
		protected static int testCaseId4;

		protected static int testStepId1;
		protected static int testStepId2;
		protected static int testStepId3;
		protected static int testStepId4;
		protected static int testStepId5;
		protected static int testStepId6;

		protected static int testSetFolderId1;
		protected static int testSetId1;
		protected static int testSetId2;

		protected static int incidentTypeId;
		protected static int incidentStatusId;
		protected static int incidentId1;
		protected static int incidentId2;
		protected static int incidentId3;

		protected static int testRunId;
		protected static int testRunStepId1;
		protected static int testRunStepId2;
		protected static int testRunId3;
		protected static int testRunId4;
		protected static int testRunId5;

		protected static int taskId1;
		protected static int taskId2;

		private const int PROJECT_ID = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
        private const int USER_ID_SYSTEM_ADMIN = 1;

        /// <summary>
        /// Sets up the web service interface
        /// </summary>
        [TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the web-service proxy class and set the URL from the .config file
			spiraImportExport = new SpiraImportExport22.ImportExport();
			spiraImportExport.Url = Properties.Settings.Default.WebServiceUrl + "v2_2/ImportExport.asmx";

			//Create a new cookie container to hold the session handle
			cookieContainer = new CookieContainer();
			spiraImportExport.CookieContainer = cookieContainer;

			//Get the last artifact id
			Business.HistoryManager history = new Business.HistoryManager();
			history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);
		}

        /// <summary>
        /// Cleans up any created projects and associated data
        /// </summary>
        [TestFixtureTearDown]
        public void CleanUp()
        {
            //Delete the newly created project and all its artifacts
            spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
            spiraImportExport.Project_Delete(projectId1);

            //Delete any other projects
            if (projectId3 > 0)
            {
                spiraImportExport.Project_Delete(projectId3);
            }

            //Delete the template (no v2.2 api call available for this)
            TemplateManager templateManager = new TemplateManager();
            templateManager.Delete(User.UserSystemAdministrator, projectTemplateId1);

            //Delete any other project templates
            if (projectTemplateId3 > 0)
            {
                templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId3);
            }

            //We need to delete any artifact history items before deleting the users
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());

            //Delete the newly created users
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId1.ToString());
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId2.ToString());
        }

        /// <summary>
        /// Verifies that you can authenticate as a specific user
        /// </summary>
        [
        Test,
		SpiraTestCase(460)
		]
		public void _01_Authentication()
		{
			//First Authenticate as Fred Bloggs (should succeed)
            bool success = spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			Assert.IsTrue(success, "Authentication 1 Failed");

			//Next Authenticate as System Administrator (should succeed), specifying the client name
			success = spiraImportExport.Connection_Authenticate2("Administrator", "Welcome123$", "MyPlugIn");
			Assert.IsTrue(success, "Authentication 2 Failed");

			//Finally Make sure that an invalid authentication fails
			success = spiraImportExport.Connection_Authenticate("testtest", "wrongwrong");
			Assert.IsFalse(success, "Authentication 3 Should Have Failed");

			//Lets test our API Key logins..
			success = spiraImportExport.Connection_Authenticate("fredbloggs", "{7A05FD06-83C3-4436-B37F-51BCF0060483}");
			Assert.IsTrue(success, "Authentication 4 Failed");
			success = spiraImportExport.Connection_Authenticate2("fredbloggs", "{7A05FD06-83C3-4436-B37F-51BCF0060483}", "MyPlugIn");
			Assert.IsTrue(success, "Authentication 5 Failed");
			success = spiraImportExport.Connection_Authenticate("fredbloggs", "{7A05FD06-83C3-4436-B37F-51BCF5560483}");
			Assert.IsFalse(success, "Authentication 6 Should Have Failed");
			success = spiraImportExport.Connection_Authenticate2("fredbloggs", "{7A05FD06-83C3-4436-B37F-51BCF0066483}", "MyPlugIn");
			Assert.IsFalse(success, "Authentication 7 Should Have Failed");
			success = spiraImportExport.Connection_Authenticate("donnaharkness", "{7A44FD06-83C3-4436-B37F-51BCF0060483}");
			Assert.IsFalse(success, "Authentication 8 Should Have Failed");

		}

		/// <summary>
		/// Verifies that you can retrieve the list of projects for different users
		/// </summary>
		[
		Test,
		SpiraTestCase(461)
		]
		public void _02_RetrieveProjectList()
		{
			//Download the project list for a normal user with single-case password and username
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			SpiraImportExport22.RemoteProject[] remoteProjects = spiraImportExport.Project_Retrieve();

			//Check the number of projects
			Assert.AreEqual(7, remoteProjects.Length, "Project List 1");

            //Check some of the data
            Assert.AreEqual("Company Website", remoteProjects[0].Name);
            Assert.AreEqual("Customer Relationship Management (CRM)", remoteProjects[1].Name);
            Assert.AreEqual("ERP: Financials", remoteProjects[2].Name);

            //Download the project list for an Admin user with mixed-case password and username
            spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			remoteProjects = spiraImportExport.Project_Retrieve();

			//Check the number of projects
			Assert.AreEqual(7, remoteProjects.Length, "Project List 2");

            //Check some of the data
            Assert.AreEqual("Company Website", remoteProjects[0].Name);
            Assert.AreEqual("Customer Relationship Management (CRM)", remoteProjects[1].Name);
            Assert.AreEqual("ERP: Financials", remoteProjects[2].Name);

            //Make sure that you can't get project list if not authenticated
            spiraImportExport.Connection_Disconnect();
			bool errorCaught = false;
			try
			{
				remoteProjects = spiraImportExport.Project_Retrieve();
			}
			catch (SoapException exception)
			{
				//ASP.NET 2.0 wraps our message in other stuff
                if (exception.Message.IndexOf("The session was not authenticated") > -1)
				{
					errorCaught = true;
				}
				else
				{
					Console.WriteLine(exception.Message);
				}
			}
            Assert.IsTrue(errorCaught, "Should not retrieve unless authenticated");

			//Make sure that you can't get project list if authentication failed
			spiraImportExport.Connection_Authenticate("wrong", "wrong");
			errorCaught = false;
			try
			{
				remoteProjects = spiraImportExport.Project_Retrieve();
			}
			catch (SoapException exception)
			{
				//ASP.NET 2.0 wraps our message in other stuff
                if (exception.Message.IndexOf("The session was not authenticated") > -1)
				{
					errorCaught = true;
				}
				else
				{
					Console.WriteLine(exception.Message);
				}
			}
			Assert.IsTrue(errorCaught, "Should not retrieve unless authenticated");
		}

		/// <summary>
		/// Verifies that you can create a new project for imported artifacts
		/// </summary>
		[
		Test,
		SpiraTestCase(463)
		]
		public void _03_CreateProject()
		{
			//Create a new project into which we will import various other artifacts
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			SpiraImportExport22.RemoteProject remoteProject = new SpiraImportExport22.RemoteProject();
			remoteProject.Name = "Imported Project";
			remoteProject.Description = "This project was imported by the Importer Unit Test";
			remoteProject.Website = "www.tempuri.org";
			remoteProject.Active = true;
			remoteProject = spiraImportExport.Project_Create(remoteProject, null);
			projectId1 = remoteProject.ProjectId.Value;
            
            //Get the template associated with the project
            projectTemplateId1 = new TemplateManager().RetrieveForProject(projectId1).ProjectTemplateId;

            //Lets verify that we can retrieve the project in question
            remoteProject = spiraImportExport.Project_RetrieveById(projectId1);
			Assert.AreEqual("Imported Project", remoteProject.Name);
			Assert.AreEqual("This project was imported by the Importer Unit Test", remoteProject.Description);
			Assert.AreEqual("www.tempuri.org", remoteProject.Website);
			Assert.AreEqual(true, remoteProject.Active);

			//Now lets test creating a new project based on the old project
			remoteProject = new SpiraImportExport22.RemoteProject();
			remoteProject.Name = "Imported Project (2)";
			remoteProject.Description = String.Empty;
			remoteProject.Website = "www.tempuri.org";
			remoteProject.Active = false;
			remoteProject = spiraImportExport.Project_Create(remoteProject, projectId1);
			projectId2 = remoteProject.ProjectId.Value;

			//Now lets test logging in to the copied prokect
			bool success = spiraImportExport.Connection_ConnectToProject(projectId2);
		//	Assert.IsTrue(success, "Authorization 1 Failed");

			//Now lets test connecting to the original project
			success = spiraImportExport.Connection_ConnectToProject(projectId1);
			Assert.IsTrue(success, "Authorization 2 Failed");

			//Finally delete the copy of the project
			spiraImportExport.Project_Delete(projectId2);
		}

		/// <summary>
		/// Verifies that you can import users to a particular project
		/// </summary>
		[
		Test,
		SpiraTestCase(464)
		]
		public void _04_ImportUsers()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now lets add a couple of users to the project
			RemoteUser remoteUser = new RemoteUser();
			//User #1
			remoteUser.FirstName = "Adam";
			remoteUser.MiddleInitial = "";
			remoteUser.LastName = "Ant";
			remoteUser.UserName = "aant";
			remoteUser.EmailAddress = "aant@antmusic.com";
			remoteUser.Password = "aant123456789";
			remoteUser.Admin = false;
			remoteUser.Active = true;
			remoteUser.LdapDn = "";
			remoteUser = spiraImportExport.User_Create(remoteUser, InternalRoutines.PROJECT_ROLE_MANAGER);
			userId1 = remoteUser.UserId.Value;
			//User #2
			remoteUser = new RemoteUser();
			remoteUser.FirstName = "Martha";
			remoteUser.MiddleInitial = "T";
			remoteUser.LastName = "Muffin";
			remoteUser.UserName = "mmuffin";
			remoteUser.EmailAddress = "mmuffin@echobeach.biz";
			remoteUser.Password = "";
			remoteUser.Admin = true;
			remoteUser.Active = true;
			remoteUser.LdapDn = "CN=Martha T Muffin,CN=Users,DC=EchoBeach,DC=Com";
			remoteUser = spiraImportExport.User_Create(remoteUser, InternalRoutines.PROJECT_ROLE_TESTER);
			userId2 = remoteUser.UserId.Value;

			//Verify that they inserted correctly
			remoteUser = spiraImportExport.User_RetrieveById(userId1);
			Assert.AreEqual("Adam", remoteUser.FirstName);
			Assert.AreEqual("Ant", remoteUser.LastName);
			Assert.AreEqual("aant", remoteUser.UserName);
			Assert.AreEqual("aant@antmusic.com", remoteUser.EmailAddress);

			remoteUser = spiraImportExport.User_RetrieveById(userId2);
			Assert.AreEqual("Martha", remoteUser.FirstName);
			Assert.AreEqual("Muffin", remoteUser.LastName);
			Assert.AreEqual("mmuffin", remoteUser.UserName);
			Assert.AreEqual("mmuffin@echobeach.biz", remoteUser.EmailAddress);

			//Now verify their roles on the project
            RemoteProjectUser[] remoteProjectUsers = spiraImportExport.Project_RetrieveUserMembership();
            Assert.AreEqual(userId1, remoteProjectUsers[0].UserId);
            Assert.AreEqual(userId2, remoteProjectUsers[1].UserId);
            Assert.AreEqual(InternalRoutines.PROJECT_ROLE_MANAGER, remoteProjectUsers[0].ProjectRoleId);
            Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, remoteProjectUsers[1].ProjectRoleId);
            Assert.AreEqual("Manager", remoteProjectUsers[0].ProjectRoleName);
            Assert.AreEqual("Tester", remoteProjectUsers[1].ProjectRoleName);

            //Verify that adding the same user again just returns the same user id
            remoteUser = new RemoteUser();
            remoteUser.FirstName = "Adam2";
            remoteUser.MiddleInitial = "";
            remoteUser.LastName = "Ant2";
            remoteUser.UserName = "aant";
            remoteUser.EmailAddress = "aant2@antmusic.com";
            remoteUser.Password = "aant123456789";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.LdapDn = "";
            remoteUser = spiraImportExport.User_Create(remoteUser, InternalRoutines.PROJECT_ROLE_MANAGER);
            int userId3 = remoteUser.UserId.Value;
            Assert.AreEqual(userId1, userId3);
        }

		/// <summary>
		/// Verifies that you can import custom properties and custom lists into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(477)
		]
		public void _05_ImportCustomProperties()
		{
			//First lets authenticate and connect to the new project as an administrator
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets see if we have any requirement custom properties
			RemoteProjectCustomProperty[] remoteProjectCustomProperties = spiraImportExport.Project_RetrieveCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(0, remoteProjectCustomProperties.Length);

			//Now add a new custom list to the project
			RemoteCustomList remoteCustomList = new RemoteCustomList();
			remoteCustomList.Name = "Req Types";
			remoteCustomList.Active = true;
			customListId1 = spiraImportExport.Project_AddCustomList(remoteCustomList).CustomPropertyListId.Value;

			//Now add some values to the custom list
			RemoteCustomListValue remoteCustomListValue = new RemoteCustomListValue();
			remoteCustomListValue.CustomPropertyListId = customListId1;
			remoteCustomListValue.Name = "Feature";
			remoteCustomListValue.Active = true;
			customValueId1 = spiraImportExport.Project_AddCustomListValue(remoteCustomListValue).CustomPropertyValueId.Value;
			remoteCustomListValue = new RemoteCustomListValue();
			remoteCustomListValue.CustomPropertyListId = customListId1;
			remoteCustomListValue.Name = "Technical Quality";
			remoteCustomListValue.Active = true;
			customValueId2 = spiraImportExport.Project_AddCustomListValue(remoteCustomListValue).CustomPropertyValueId.Value;

			//Now lets add a text custom property and one list value to both requirements and incidents
			List<RemoteProjectCustomProperty> remoteProjectCustomPropertiesNew = new List<RemoteProjectCustomProperty>();
			//Text Property
			RemoteProjectCustomProperty remoteProjectCustomProperty = new RemoteProjectCustomProperty();
			remoteProjectCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			remoteProjectCustomProperty.CustomPropertyId = 1;
			remoteProjectCustomProperty.ProjectId = projectId1;
			remoteProjectCustomProperty.Alias = "Source";
			remoteProjectCustomPropertiesNew.Add(remoteProjectCustomProperty);
			//List Property
			remoteProjectCustomProperty = new RemoteProjectCustomProperty();
			remoteProjectCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			remoteProjectCustomProperty.CustomPropertyId = 11;
			remoteProjectCustomProperty.ProjectId = projectId1;
			remoteProjectCustomProperty.Alias = "Req Type";
			remoteProjectCustomProperty.CustomPropertyListId = customListId1;
			remoteProjectCustomPropertiesNew.Add(remoteProjectCustomProperty);
			spiraImportExport.Project_SaveCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteProjectCustomPropertiesNew.ToArray());

			//Verify the additions
            remoteProjectCustomProperties = spiraImportExport.Project_RetrieveCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, remoteProjectCustomProperties.Length);
            Assert.IsTrue(remoteProjectCustomProperties.Any(c => c.Alias == "Source"));
            Assert.IsTrue(remoteProjectCustomProperties.Any(c => c.Alias == "Req Type"));

			//Now add the same two custom properties to incidents
			remoteProjectCustomPropertiesNew = new List<RemoteProjectCustomProperty>();
			//Text Property
			remoteProjectCustomProperty = new RemoteProjectCustomProperty();
			remoteProjectCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteProjectCustomProperty.CustomPropertyId = 1;
			remoteProjectCustomProperty.ProjectId = projectId1;
			remoteProjectCustomProperty.Alias = "Source";
			remoteProjectCustomPropertiesNew.Add(remoteProjectCustomProperty);
			//List Property
			remoteProjectCustomProperty = new RemoteProjectCustomProperty();
			remoteProjectCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteProjectCustomProperty.CustomPropertyId = 11;
			remoteProjectCustomProperty.ProjectId = projectId1;
			remoteProjectCustomProperty.Alias = "Req Type";
			remoteProjectCustomProperty.CustomPropertyListId = customListId1;
			remoteProjectCustomPropertiesNew.Add(remoteProjectCustomProperty);
			spiraImportExport.Project_SaveCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Incident, remoteProjectCustomPropertiesNew.ToArray());

			//Verify the additions
            remoteProjectCustomProperties = spiraImportExport.Project_RetrieveCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(2, remoteProjectCustomProperties.Length);
            Assert.IsTrue(remoteProjectCustomProperties.Any(c => c.Alias == "Source"));
            Assert.IsTrue(remoteProjectCustomProperties.Any(c => c.Alias == "Req Type"));

            //Verify using the other duplicative function (removed in v3.0+ APIs)
            RemoteCustomProperty[] remoteCustomProperties = spiraImportExport.CustomProperty_RetrieveProjectProperties((int)DataModel.Artifact.ArtifactTypeEnum.Incident);
            Assert.AreEqual(2, remoteCustomProperties.Length);
            Assert.IsTrue(remoteCustomProperties.Any(c => c.Alias == "Source"));
            Assert.IsTrue(remoteCustomProperties.Any(c => c.Alias == "Req Type"));            
            Assert.AreEqual(1, remoteCustomProperties.FirstOrDefault(c => c.Alias == "Source").CustomPropertyTypeId);
            Assert.AreEqual(2, remoteCustomProperties.FirstOrDefault(c => c.Alias == "Req Type").CustomPropertyTypeId);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}


		/// <summary>
		/// Verifies that you can import new releases into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(479)
		]
		public void _06_ImportReleases()
		{
			//First lets authenticate and connect to the project as one of our new users
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Next create a release and two child iterations
			//Release
			RemoteRelease remoteRelease = new RemoteRelease();
			remoteRelease.Name = "Release 1";
			remoteRelease.VersionNumber = "1.0";
			remoteRelease.Description = "First version of the system";
			remoteRelease.Active = true;
			remoteRelease.Iteration = false;
			remoteRelease.StartDate = DateTime.Now;
			remoteRelease.EndDate = DateTime.Now.AddMonths(3);
			remoteRelease.ResourceCount = 5;
			releaseId1 = spiraImportExport.Release_Create(remoteRelease, null).ReleaseId.Value;
			//Iteration #1
			remoteRelease = new RemoteRelease();
			remoteRelease.Name = "Sprint 1";
			remoteRelease.VersionNumber = "1.0.001";
			remoteRelease.Active = true;
			remoteRelease.Iteration = true;
			remoteRelease.StartDate = DateTime.Now;
			remoteRelease.EndDate = DateTime.Now.AddMonths(1);
			remoteRelease.ResourceCount = 5;
			iterationId1 = spiraImportExport.Release_Create(remoteRelease, releaseId1).ReleaseId.Value;
			//Iteration #2
			remoteRelease = new RemoteRelease();
			remoteRelease.Name = "Sprint 2";
			remoteRelease.VersionNumber = "1.0.002";
			remoteRelease.Active = true;
			remoteRelease.Iteration = false;
			remoteRelease.StartDate = DateTime.Now.AddMonths(1);
			remoteRelease.EndDate = DateTime.Now.AddMonths(2);
			remoteRelease.ResourceCount = 5;
			iterationId2 = spiraImportExport.Release_Create(remoteRelease, releaseId1).ReleaseId.Value;

			//Now verify that the releases/iterations inserted correctly
			//First retrieve the release
			remoteRelease = spiraImportExport.Release_RetrieveById(releaseId1);
			Assert.AreEqual("Release 1", remoteRelease.Name);
			Assert.AreEqual("1.0", remoteRelease.VersionNumber);
			Assert.AreEqual("First version of the system", remoteRelease.Description);
			Assert.AreEqual(false, remoteRelease.Iteration);
			Assert.AreEqual(userId1, remoteRelease.CreatorId);
			Assert.AreEqual(true, remoteRelease.Active);
			Assert.IsTrue(remoteRelease.PlannedEffort > 0);

			//Now retrieve one of the iterations
			remoteRelease = spiraImportExport.Release_RetrieveById(iterationId1);
			Assert.AreEqual("1.0.001", remoteRelease.VersionNumber);
			Assert.IsNull(remoteRelease.Description);
			Assert.AreEqual(true, remoteRelease.Iteration);
			Assert.AreEqual(userId1, remoteRelease.CreatorId);
			Assert.AreEqual(true, remoteRelease.Active);
			Assert.IsTrue(remoteRelease.PlannedEffort > 0);

			//Now make an update to one of the iterations
			remoteRelease = spiraImportExport.Release_RetrieveById(iterationId2);
			remoteRelease.VersionNumber = "1.0.002b";
			remoteRelease.Description = "Second Sprint";
			spiraImportExport.Release_Update(remoteRelease);

			//Verify the change
			remoteRelease = spiraImportExport.Release_RetrieveById(iterationId2);
			Assert.AreEqual("1.0.002b", remoteRelease.VersionNumber);
			Assert.AreEqual("Second Sprint", remoteRelease.Description);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new requirements into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(465)
		]
		public void _07_ImportRequirements()
		{
            int rq_importanceCriticalId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 1).ImportanceId;
            int rq_importanceHighId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 2).ImportanceId;
            int rq_importanceMediumId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 3).ImportanceId;

            //First lets authenticate and connect to the project as one of our new users
            spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets add a nested tree of requirements
			//First the summary item
			RemoteRequirement remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 1;
			remoteRequirement.Name = "Functionality Area";
			remoteRequirement.Description = String.Empty;
			remoteRequirement.AuthorId = userId1;
			remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
			requirementId1 = remoteRequirement.RequirementId.Value;
			//Detail Item 1
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 2;
			remoteRequirement.ImportanceId = rq_importanceCriticalId;
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.Name = "Requirement 1";
			remoteRequirement.Description = "Requirement Description 1";
			remoteRequirement.AuthorId = userId1;
            //Add some custom property values
            remoteRequirement.Text01 = "test value1";
            remoteRequirement.List01 = customValueId2;
            remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 1);
			requirementId2 = remoteRequirement.RequirementId.Value;
			//Detail Item 2
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 1;
			remoteRequirement.ImportanceId = rq_importanceHighId;
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.Name = "Requirement 2";
			remoteRequirement.Description = "Requirement Description 2";
			remoteRequirement.AuthorId = userId1;
            //Add some custom property values
            remoteRequirement.Text01 = "test value2";
            remoteRequirement.List01 = customValueId1;
            remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
			requirementId3 = remoteRequirement.RequirementId.Value;
			//Detail Item 3
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 4;
			remoteRequirement.ImportanceId = rq_importanceHighId;
			remoteRequirement.Name = "Requirement 3";
			remoteRequirement.Description = "Requirement Description 3";
			remoteRequirement.AuthorId = userId2;
			remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 1);
			requirementId4 = remoteRequirement.RequirementId.Value;
			//Detail Item 4
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.StatusId = 3;
			remoteRequirement.ImportanceId = rq_importanceMediumId;
			remoteRequirement.Name = "Requirement 4";
			remoteRequirement.Description = String.Empty;
			remoteRequirement.AuthorId = userId2;
			remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, -1);
			requirementId5 = remoteRequirement.RequirementId.Value;

			//First retrieve the parent requirement
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId1);
			Assert.AreEqual("Functionality Area", remoteRequirement.Name);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, remoteRequirement.StatusId);
			Assert.AreEqual(userId1, remoteRequirement.AuthorId);
			Assert.IsNull(remoteRequirement.ImportanceId);

			//Now retrieve a first-level indent requirement
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId3);
			Assert.AreEqual("Requirement 2", remoteRequirement.Name);
			Assert.AreEqual("Requirement Description 2", remoteRequirement.Description);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId);
			Assert.AreEqual(userId1, remoteRequirement.AuthorId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);
            Assert.AreEqual("test value2", remoteRequirement.Text01);
            Assert.AreEqual(customValueId1, remoteRequirement.List01);

			//Now retrieve a second-level indent requirement
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId4);
			Assert.AreEqual("Requirement 3", remoteRequirement.Name);
			Assert.AreEqual("Requirement Description 3", remoteRequirement.Description);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId);
			Assert.AreEqual(userId2, remoteRequirement.AuthorId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);

			//Now test that we can insert a requirement using the 'insert under parent' method
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.StatusId = 3;
			remoteRequirement.ImportanceId = rq_importanceCriticalId;
			remoteRequirement.Name = "Test Child 1";
			remoteRequirement.Description = String.Empty;
			remoteRequirement.AuthorId = userId1;
			remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, requirementId5);
			int requirementId6 = remoteRequirement.RequirementId.Value;
			//Verify insertion data
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId6);
			Assert.AreEqual("Test Child 1", remoteRequirement.Name);
			//Verify that parent became a summary
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId5);
			Assert.AreEqual("Requirement 4", remoteRequirement.Name);
			Assert.AreEqual(true, remoteRequirement.Summary);

			//Now test that we can insert a requirement under the same folder as an existing one
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.StatusId = 3;
			remoteRequirement.ImportanceId = rq_importanceCriticalId;
			remoteRequirement.Name = "Test Child 2";
			remoteRequirement.Description = "Test Child Description 2";
			remoteRequirement.AuthorId = userId1;
			remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, requirementId5);
			int requirementId7 = remoteRequirement.RequirementId.Value;
			//Verify insertion data
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId7);
			Assert.AreEqual("Test Child 2", remoteRequirement.Name);

			//Now make an update to one of the requirements
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId7);
			remoteRequirement.StatusId = 4;
			remoteRequirement.ImportanceId = rq_importanceHighId;
			remoteRequirement.Name = "Test Child 2a";
			remoteRequirement.Description = "Test Child Description 2a";
			spiraImportExport.Requirement_Update(remoteRequirement);

			//Verify the change
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId7);
			Assert.AreEqual(4, remoteRequirement.StatusId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);
			Assert.AreEqual("Test Child 2a", remoteRequirement.Name);
			Assert.AreEqual("Test Child Description 2a", remoteRequirement.Description);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new test folders and test cases into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(466)
		]
		public void _08_ImportTestCases()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets add two nested test folders with two test cases in each
			//Folder A
			RemoteTestCase remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Folder A";
			remoteTestCase = spiraImportExport.TestCase_CreateFolder(remoteTestCase, null);
			testFolderId1 = remoteTestCase.TestCaseId.Value;
			//Folder B
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Folder B";
			remoteTestCase.AuthorId = userId1;
			remoteTestCase.OwnerId = userId2;
			remoteTestCase = spiraImportExport.TestCase_CreateFolder(remoteTestCase, testFolderId1);
			testFolderId2 = remoteTestCase.TestCaseId.Value;
			//Test Case 1
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 1";
			remoteTestCase.Description = "Test Case Description 1";
			remoteTestCase.Active = true;
			remoteTestCase.TestCasePriorityId = 1;
			remoteTestCase.EstimatedDuration = 30;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, testFolderId1);
			testCaseId1 = remoteTestCase.TestCaseId.Value;
			//Test Case 2
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 2";
			remoteTestCase.Description = "Test Case Description 2";
			remoteTestCase.AuthorId = userId1;
			remoteTestCase.OwnerId = userId2;
			remoteTestCase.Active = true;
			remoteTestCase.TestCasePriorityId = 2;
			remoteTestCase.EstimatedDuration = 25;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, testFolderId1);
			testCaseId2 = remoteTestCase.TestCaseId.Value;
			//Test Case 3
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 3";
			remoteTestCase.Description = "Test Case Description 3";
			remoteTestCase.AuthorId = userId2;
			remoteTestCase.OwnerId = userId1;
			remoteTestCase.Active = true;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, testFolderId2);
			testCaseId3 = remoteTestCase.TestCaseId.Value;
			//Test Case 4
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 4";
			remoteTestCase.AuthorId = userId2;
			remoteTestCase.OwnerId = userId2;
			remoteTestCase.Active = true;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, testFolderId2);
			testCaseId4 = remoteTestCase.TestCaseId.Value;

			//First retrieve the top-level folder
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testFolderId1);
			Assert.AreEqual("Test Folder A", remoteTestCase.Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, remoteTestCase.ExecutionStatusId);
			Assert.AreEqual(true, remoteTestCase.Folder);
			Assert.IsNull(remoteTestCase.OwnerId);

			//Now retrieve the second-level folder
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testFolderId2);
			Assert.AreEqual("Test Folder B", remoteTestCase.Name);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, remoteTestCase.ExecutionStatusId);
            Assert.AreEqual(true, remoteTestCase.Folder);
            Assert.IsNull(remoteTestCase.OwnerId);

			//Now retrieve a test case
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4", remoteTestCase.Name);
			Assert.IsNull(remoteTestCase.Description);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, remoteTestCase.ExecutionStatusId);
			Assert.AreEqual(userId2, remoteTestCase.AuthorId);
			Assert.AreEqual(userId2, remoteTestCase.OwnerId);
			Assert.IsNull(remoteTestCase.TestCasePriorityId);
			Assert.IsNull(remoteTestCase.EstimatedDuration);

			//Now we need to add some parameters to test case 2
			RemoteTestCaseParameter remoteTestCaseParameter = new RemoteTestCaseParameter();
			remoteTestCaseParameter.TestCaseId = testCaseId2;
			remoteTestCaseParameter.Name = "param1";
			remoteTestCaseParameter.DefaultValue = "nothing";
			spiraImportExport.TestCase_AddParameter(remoteTestCaseParameter);
			remoteTestCaseParameter = new RemoteTestCaseParameter();
			remoteTestCaseParameter.TestCaseId = testCaseId2;
			remoteTestCaseParameter.Name = "param2";
			spiraImportExport.TestCase_AddParameter(remoteTestCaseParameter);

			//Finally make an update to one of the test cases
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			remoteTestCase.Name = "Test Case 4a";
			remoteTestCase.Description = "Test Case Description 4a";
			remoteTestCase.AuthorId = userId2;
			remoteTestCase.OwnerId = userId1;
			spiraImportExport.TestCase_Update(remoteTestCase, null);

			//Verify the change
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4a", remoteTestCase.Name);
			Assert.AreEqual(userId2, remoteTestCase.AuthorId);
			Assert.AreEqual(userId1, remoteTestCase.OwnerId);
			Assert.AreEqual("Test Case Description 4a", remoteTestCase.Description);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import new test steps into the previously created test cases
		/// </summary>
		[
		Test,
		SpiraTestCase(467)
		]
		public void _09_ImportTestSteps()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets add two test steps to two of the test cases
			//Step 1
			RemoteTestStep remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 1";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep.SampleData = "test 123";
			remoteTestStep = spiraImportExport.TestStep_Create(remoteTestStep, testCaseId3);
			testStepId1 = remoteTestStep.TestStepId.Value;
			//Step 2
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 2;
			remoteTestStep.Description = "Test Step 2";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep = spiraImportExport.TestStep_Create(remoteTestStep, testCaseId3);
			testStepId2 = remoteTestStep.TestStepId.Value;
			//Step 3
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 4";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep.SampleData = "test 123";
			remoteTestStep = spiraImportExport.TestStep_Create(remoteTestStep, testCaseId4);
			testStepId3 = remoteTestStep.TestStepId.Value;
			//Step 4
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 3";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep = spiraImportExport.TestStep_Create(remoteTestStep, testCaseId4);
			testStepId4 = remoteTestStep.TestStepId.Value;

			//Now verify that the import was successful

			//Retrieve the first test case with its steps
			RemoteTestStep[] remoteTestSteps = spiraImportExport.TestStep_RetrieveByTestCaseId(testCaseId3);
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 1");
			Assert.AreEqual("Test Step 1", remoteTestSteps[0].Description);
			Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 1");
			Assert.AreEqual("Test Step 2", remoteTestSteps[1].Description);
			Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 2");

			//Retrieve the second test case with its steps
			remoteTestSteps = spiraImportExport.TestStep_RetrieveByTestCaseId(testCaseId4);
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 2");
			Assert.AreEqual("Test Step 3", remoteTestSteps[0].Description);
			Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 3");
			Assert.AreEqual("Test Step 4", remoteTestSteps[1].Description);
			Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 4");

			//Now lets add a linked test case as a test step, first with no parameters
			testStepId5 = spiraImportExport.TestStep_CreateLink(testCaseId1, 1, testCaseId3, null);

			//Now lets add a linked test case as a test step with two parameters
			RemoteTestStepParameter[] parameterArray = new RemoteTestStepParameter[2];
			parameterArray[0] = new RemoteTestStepParameter();
			parameterArray[0].Name = "param1";
			parameterArray[0].Value = "value1";
			parameterArray[1] = new RemoteTestStepParameter();
			parameterArray[1].Name = "param2";
			parameterArray[1].Value = "value2";
			testStepId6 = spiraImportExport.TestStep_CreateLink(testCaseId1, 2, testCaseId2, parameterArray);

			//Retrieve the linked test case steps
			remoteTestSteps = spiraImportExport.TestStep_RetrieveByTestCaseId(testCaseId1);
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 3");
			Assert.AreEqual("Call", remoteTestSteps[0].Description);
			Assert.AreEqual(testCaseId3, remoteTestSteps[0].LinkedTestCaseId);
			Assert.AreEqual("Call", remoteTestSteps[1].Description);
			Assert.AreEqual(testCaseId2, remoteTestSteps[1].LinkedTestCaseId);

			//Finally make an update to one of the test cases and uts test steps
			RemoteTestCase remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			remoteTestCase.Name = "Test Case 4b";
			remoteTestCase.Description = "Test Case Description 4b";
			remoteTestSteps = spiraImportExport.TestStep_RetrieveByTestCaseId(testCaseId4);
			remoteTestSteps[0].Description = "Test Step 3b";
			remoteTestSteps[1].Description = "Test Step 4b";
			spiraImportExport.TestCase_Update(remoteTestCase, remoteTestSteps);

			//Verify the change
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4b", remoteTestCase.Name);
			Assert.AreEqual("Test Case Description 4b", remoteTestCase.Description);
			remoteTestSteps = spiraImportExport.TestStep_RetrieveByTestCaseId(testCaseId4);
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 2");
			Assert.AreEqual("Test Step 3b", remoteTestSteps[0].Description);
			Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 3");
			Assert.AreEqual("Test Step 4b", remoteTestSteps[1].Description);
			Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 4");

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import requirements test coverage into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(468)
		]
		public void _10_ImportCoverage()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Next lets add some coverage entries for a requirement
			//Entry 1
			RemoteRequirementTestCaseMapping remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Requirement_AddTestCoverage(remoteRequirementTestCaseMapping);
			//Entry 2
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.Requirement_AddTestCoverage(remoteRequirementTestCaseMapping);
			//Entry 3
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId3;
			spiraImportExport.Requirement_AddTestCoverage(remoteRequirementTestCaseMapping);

			//Try adding a duplicate entry - should fail quietly
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Requirement_AddTestCoverage(remoteRequirementTestCaseMapping);

			//Now verify the coverage data
			TestCaseManager testCaseManager = new TestCaseManager();
            List<TestCase> mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId1, requirementId2);
            Assert.AreEqual(3, mappedTestCases.Count);
            Assert.AreEqual(testCaseId1, mappedTestCases[0].TestCaseId);
            Assert.AreEqual(testCaseId2, mappedTestCases[1].TestCaseId);
            Assert.AreEqual(testCaseId3, mappedTestCases[2].TestCaseId);

			//Finally remove the coverage
			//Entry 1
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Requirement_RemoveTestCoverage(remoteRequirementTestCaseMapping);
			//Entry 2
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.Requirement_RemoveTestCoverage(remoteRequirementTestCaseMapping);
			//Entry 3
			remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
			remoteRequirementTestCaseMapping.RequirementId = requirementId2;
			remoteRequirementTestCaseMapping.TestCaseId = testCaseId3;
			spiraImportExport.Requirement_RemoveTestCoverage(remoteRequirementTestCaseMapping);

			//Now verify the coverage data
			mappedTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId1, requirementId2);
            Assert.AreEqual(0, mappedTestCases.Count);

			//Test that we can map test cases against releases & iterations

			//First test that nothing is mapped against the release or iterations, except those that were auto-mapped from the requirement
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, releaseId1);
            Assert.AreEqual(3, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId1);
            Assert.AreEqual(0, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId2);
            Assert.AreEqual(0, mappedTestCases.Count);

			//Now lets map the tests to the release and the iteration
			//Release 1
			RemoteReleaseTestCaseMapping remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = releaseId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = releaseId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = releaseId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId3;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			//Iteration 1
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			//Iteration 2
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId2;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId2;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId3;
			spiraImportExport.Release_AddTestMapping(remoteReleaseTestCaseMapping);

			//Now test that the mappings have been added
			mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, releaseId1);
            Assert.AreEqual(3, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId1);
            Assert.AreEqual(2, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId2);
            Assert.AreEqual(2, mappedTestCases.Count);

			//Now remove some of the mappings
			//Release 1
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = releaseId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Release_RemoveTestMapping(remoteReleaseTestCaseMapping);
			//Iteration 1
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.Release_RemoveTestMapping(remoteReleaseTestCaseMapping);
			//Iteration 2
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId2;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId3;
			spiraImportExport.Release_RemoveTestMapping(remoteReleaseTestCaseMapping);

			//Finally test the mapping changes
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, releaseId1);
            Assert.AreEqual(2, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId1);
            Assert.AreEqual(1, mappedTestCases.Count);
            mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId1, iterationId2);
            Assert.AreEqual(1, mappedTestCases.Count);
		}

		/// <summary>
		/// Verifies that you can import test sets into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(470)
		]
		public void _11_ImportTestSets()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets try importing a test set folder and put some test sets in it]
			RemoteTestSet remoteTestSet = new RemoteTestSet();
			remoteTestSet.Name = "Test Set Folder";
			remoteTestSet.CreatorId = userId1;
			testSetFolderId1 = spiraImportExport.TestSet_CreateFolder(remoteTestSet, null).TestSetId.Value;

			//First lets try inserting a new test set into this folder
			remoteTestSet = new RemoteTestSet();
			remoteTestSet.Name = "Test Set 1";
			remoteTestSet.CreatorId = userId1;
			remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
			testSetId1 = spiraImportExport.TestSet_Create(remoteTestSet, testSetFolderId1).TestSetId.Value;

			//Verify that it inserted correctly
			remoteTestSet = spiraImportExport.TestSet_RetrieveById(testSetId1);
			Assert.AreEqual(testSetId1, remoteTestSet.TestSetId, "TestSetId");
			Assert.AreEqual(userId1, remoteTestSet.CreatorId, "CreatorId");
			Assert.AreEqual("Test Set 1", remoteTestSet.Name, "Name");
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, remoteTestSet.TestSetStatusId, "TestSetStatusId");
			Assert.IsNull(remoteTestSet.Description, "Description");
			Assert.IsNull(remoteTestSet.ReleaseId);
			Assert.IsNull(remoteTestSet.OwnerId, "OwnerName");
			Assert.IsNull(remoteTestSet.PlannedDate, "PlannedDate");
			Assert.IsNull(remoteTestSet.ExecutionDate, "ExecutionDate");

			//Next lets try inserting a new test set after it in the folder
			remoteTestSet = new RemoteTestSet();
			remoteTestSet.Name = "Test Set 2";
			remoteTestSet.Description = "Test Set 2 Description";
			remoteTestSet.CreatorId = userId1;
			remoteTestSet.OwnerId = userId2;
			remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.InProgress;
			remoteTestSet.PlannedDate = DateTime.Now.AddDays(1);
			testSetId2 = spiraImportExport.TestSet_Create(remoteTestSet, testSetFolderId1).TestSetId.Value;
			//Verify that it inserted correctly
			remoteTestSet = spiraImportExport.TestSet_RetrieveById(testSetId2);
			Assert.AreEqual(testSetId2, remoteTestSet.TestSetId, "TestSetId");
			Assert.AreEqual(userId1, remoteTestSet.CreatorId, "CreatorId");
			Assert.AreEqual("Test Set 2", remoteTestSet.Name, "Name");
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.InProgress, remoteTestSet.TestSetStatusId, "TestSetStatusId");
			Assert.AreEqual("Test Set 2 Description", remoteTestSet.Description, "Description");
			Assert.IsNull(remoteTestSet.ReleaseId);
			Assert.AreEqual(userId2, remoteTestSet.OwnerId, "OwnerId");
			Assert.IsNotNull(remoteTestSet.PlannedDate, "PlannedDate");
			Assert.IsNull(remoteTestSet.ExecutionDate, "ExecutionDate");

			//Now test that we can add some test cases to one of the test set
			RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping);
			remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId2;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping);

			//Now verify the data
			RemoteTestCase[] remoteTestCases = spiraImportExport.TestCase_RetrieveByTestSetId(testSetId1);
			Assert.AreEqual(2, remoteTestCases.Length);
			Assert.AreEqual("Test Case 1", remoteTestCases[0].Name);
			Assert.AreEqual("Test Case 2", remoteTestCases[1].Name);
		}

		/// <summary>
		/// Verifies that you can import a test run complete with test run steps
		/// </summary>
		[
		Test,
		SpiraTestCase(469)
		]
		public void _12_ImportTestRun()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now we need to create a test run from an existing test case we've already imported that has steps
			RemoteTestRun remoteTestRun = spiraImportExport.TestRun_CreateFromTestCases(new int[] { testCaseId3 }, iterationId1);

			//Now mark the first step as pass, the second as fail
			remoteTestRun.TestRunSteps[0].ActualResult = "passed";
			remoteTestRun.TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteTestRun.TestRunSteps[1].ActualResult = "broke";
			remoteTestRun.TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;

			//Finally actually save the run
			remoteTestRun = spiraImportExport.TestRun_Save(remoteTestRun, DateTime.Now);
			testRunId = remoteTestRun.TestRunId.Value;
			testRunStepId1 = remoteTestRun.TestRunSteps[0].TestRunStepId.Value;
			testRunStepId2 = remoteTestRun.TestRunSteps[1].TestRunStepId.Value;

			//Now verify that it saved correctly

			//First check the test run
			remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.ExecutionStatusId, "TestRun");
			Assert.AreEqual(iterationId1, remoteTestRun.ReleaseId, "ReleaseId");

			//Now check the test run steps
			Assert.AreEqual("passed", remoteTestRun.TestRunSteps[0].ActualResult);
			Assert.AreEqual("broke", remoteTestRun.TestRunSteps[1].ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

            //Now check the test case itself to see if it was correctly updated
            RemoteTestCase remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId3);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
            Assert.IsNotNull(remoteTestCase.ExecutionDate);

			//Now test that we can create a test run from a test set
			remoteTestRun = spiraImportExport.TestRun_CreateFromTestSet(testSetId1);

			//Now mark the first step as pass, the second as fail
			remoteTestRun.TestRunSteps[0].ActualResult = "passed";
			remoteTestRun.TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteTestRun.TestRunSteps[1].ActualResult = "broke";
			remoteTestRun.TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;

			//Finally actually save the run
			remoteTestRun = spiraImportExport.TestRun_Save(remoteTestRun, DateTime.Now);
			testRunId = remoteTestRun.TestRunId.Value;
			testRunStepId1 = remoteTestRun.TestRunSteps[0].TestRunStepId.Value;
			testRunStepId2 = remoteTestRun.TestRunSteps[1].TestRunStepId.Value;

			//Now verify that it saved correctly
			//Since the test set retrievebyid method doesn't include execution data, no point verifying the test set counts

			//First check the first test case itself to see if it was correctly updated
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNotNull(remoteTestCase.ExecutionDate);
			//The second test case in the test set should still be listed as not run
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId2);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNull(remoteTestCase.ExecutionDate);

			//Now check the test run
			remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.ExecutionStatusId, "TestRun");
			Assert.AreEqual(testSetId1, remoteTestRun.TestSetId, "TestSetId");

			//Finally check the test run steps
			Assert.AreEqual("passed", remoteTestRun.TestRunSteps[0].ActualResult);
			Assert.AreEqual("broke", remoteTestRun.TestRunSteps[1].ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

			//*** Now test that we can use the automated test runner API ***

			//This tests that we can create a successful automated test run with a release but no test set
			remoteTestRun = new RemoteTestRun();
			remoteTestRun.TestCaseId = testCaseId1;
			remoteTestRun.ReleaseId = iterationId2;
			remoteTestRun.StartDate = DateTime.Now;
			remoteTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteTestRun.RunnerName = "TestSuite";
			remoteTestRun.RunnerTestName = "02_Test_Method";
			testRunId3 = spiraImportExport.TestRun_RecordAutomated1(remoteTestRun).TestRunId.Value;

			//Now retrieve the test run to check that it saved correctly - using the web service
			remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId3);

			//Verify Counts (no steps for automated runs)
			Assert.AreEqual(testRunId3, remoteTestRun.TestRunId);
			Assert.IsNull(remoteTestRun.TestRunSteps);

			//Now verify the data
			Assert.AreEqual(userId1, remoteTestRun.TesterId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestRun.ExecutionStatusId);
			Assert.AreEqual(iterationId2, remoteTestRun.ReleaseId, "ReleaseId");
			Assert.IsNull(remoteTestRun.TestSetId, "TestSetId");
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), remoteTestRun.TestRunTypeId);
			Assert.AreEqual("TestSuite", remoteTestRun.RunnerName);
			Assert.AreEqual("02_Test_Method", remoteTestRun.RunnerTestName);
			Assert.AreEqual(0, remoteTestRun.RunnerAssertCount, "RunnerAssertCount");
			Assert.AreEqual("Nothing Reported", remoteTestRun.RunnerMessage);
			Assert.AreEqual("Nothing Reported", remoteTestRun.RunnerStackTrace);

			//Now verify that the test case itself updated
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestCase.ExecutionStatusId);

			//This tests that we can create a failed automated test run with a test set instead of a release
			remoteTestRun = new RemoteTestRun();
			remoteTestRun.TesterId = userId2;
			remoteTestRun.TestCaseId = testCaseId1;
			remoteTestRun.TestSetId = testSetId1;
			remoteTestRun.StartDate = DateTime.Now;
			remoteTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			remoteTestRun.RunnerName = "TestSuite";
			remoteTestRun.RunnerTestName = "02_Test_Method";
			remoteTestRun.RunnerAssertCount = 5;
			remoteTestRun.RunnerMessage = "Expected 1, Found 0";
			remoteTestRun.RunnerStackTrace = "Error Stack Trace........";
			testRunId4 = spiraImportExport.TestRun_RecordAutomated1(remoteTestRun).TestRunId.Value;

			//Now retrieve the test run to check that it saved correctly - using the web service
			remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId4);

			//Verify Counts (no steps for automated runs)
			Assert.AreEqual(testRunId4, remoteTestRun.TestRunId);
			Assert.IsNull(remoteTestRun.TestRunSteps);

			//Now verify the data
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.ExecutionStatusId);
			Assert.AreEqual(userId2, remoteTestRun.TesterId);
			Assert.IsNull(remoteTestRun.ReleaseId, "ReleaseId");
			Assert.AreEqual(testSetId1, remoteTestRun.TestSetId, "TestSetId");
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), remoteTestRun.TestRunTypeId);
			Assert.AreEqual("TestSuite", remoteTestRun.RunnerName);
			Assert.AreEqual("02_Test_Method", remoteTestRun.RunnerTestName);
			Assert.AreEqual(5, remoteTestRun.RunnerAssertCount);
			Assert.AreEqual("Expected 1, Found 0", remoteTestRun.RunnerMessage);
			Assert.AreEqual("Error Stack Trace........", remoteTestRun.RunnerStackTrace);

			//Now verify that the test case itself updated
            remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId);

			//This tests that we can create a failed automated test run using the sessionless method overload
			spiraImportExport.CookieContainer = null;
			testRunId5 = spiraImportExport.TestRun_RecordAutomated2("aant", "aant123456789", projectId1, userId2, testCaseId1, iterationId2, null, DateTime.Now, DateTime.Now.AddSeconds(20), (int)TestCase.ExecutionStatusEnum.Failed, "TestSuite", "02_Test_Method", 5, "Expected 1, Found 0", "Error Stack Trace........");

			//Now retrieve the test run to check that it saved correctly - using the web service
			//Need to restore the use of cookies first
			spiraImportExport.CookieContainer = cookieContainer;
			remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId5);

			//Verify Counts (no steps for automated runs)
			Assert.AreEqual(testRunId5, remoteTestRun.TestRunId);
			Assert.IsNull(remoteTestRun.TestRunSteps);
		}

		/// <summary>
		/// Verifies that you can import incidents into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(472)
		]
		public void _13_ImportIncidents()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now we need to make sure that we can retrieve the list of types, statuses, priorities and severities
			RemoteIncidentType[] remoteIncidentTypes = spiraImportExport.Incident_RetrieveTypes();
			Assert.AreEqual(remoteIncidentTypes.Length, 8);
			RemoteIncidentStatus[] remoteIncidentStatuses = spiraImportExport.Incident_RetrieveStatuses();
			Assert.AreEqual(remoteIncidentStatuses.Length, 8);
			RemoteIncidentPriority[] remoteIncidentPriorities = spiraImportExport.Incident_RetrievePriorities();
			Assert.AreEqual(remoteIncidentPriorities.Length, 4);
			RemoteIncidentSeverity[] remoteIncidentSeverities = spiraImportExport.Incident_RetrieveSeverities();
			Assert.AreEqual(remoteIncidentSeverities.Length, 4);

			//Now lets add a custom incident type, status, priority and severity (which we'll then use to import with)
			//Type
			RemoteIncidentType remoteIncidentType = new RemoteIncidentType();
			remoteIncidentType.Name = "Problem";
			remoteIncidentType.Active = true;
			remoteIncidentType.Issue = false;
			remoteIncidentType.Risk = false;
			incidentTypeId = spiraImportExport.Incident_AddType(remoteIncidentType).IncidentTypeId.Value;
			//Status
			RemoteIncidentStatus remoteIncidentStatus = new RemoteIncidentStatus();
			remoteIncidentStatus.Name = "Indeterminate";
			remoteIncidentStatus.Active = true;
			remoteIncidentStatus.Open = true;
			incidentStatusId = spiraImportExport.Incident_AddStatus(remoteIncidentStatus).IncidentStatusId.Value;
			//Priority
			RemoteIncidentPriority remoteIncidentPriority = new RemoteIncidentPriority();
			remoteIncidentPriority.Name = "Not Good";
			remoteIncidentPriority.Active = true;
			remoteIncidentPriority.Color = "000000";
			int priorityId = spiraImportExport.Incident_AddPriority(remoteIncidentPriority).PriorityId.Value;
			//Severity
			RemoteIncidentSeverity remoteIncidentSeverity = new RemoteIncidentSeverity();
			remoteIncidentSeverity.Name = "Difficult";
			remoteIncidentSeverity.Active = true;
			remoteIncidentSeverity.Color = "000000";
			int severityId = spiraImportExport.Incident_AddSeverity(remoteIncidentSeverity).SeverityId.Value;

			//Now lets re-authenticate and connect to the project as a manager
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets add a new incident, an open issue and a closed bug (mapped to a test run step)
			//Lets send in a creation-date for one and leave one blank
			//Incident #1
			RemoteIncident remoteIncident = new RemoteIncident();
			remoteIncident.IncidentTypeId = incidentTypeId;
			remoteIncident.IncidentStatusId = incidentStatusId;
			remoteIncident.Name = "New Incident";
			remoteIncident.Description = "This is a test new incident";
			remoteIncident.CreationDate = DateTime.Parse("1/5/2005");
			//Add some custom property values
			remoteIncident.Text01 = "test value";
			remoteIncident.List01 = customValueId2;
			incidentId1 = spiraImportExport.Incident_Create(remoteIncident).IncidentId.Value;
			//Incident #2
			remoteIncident = new RemoteIncident();
			remoteIncident.IncidentTypeId = incidentTypeId;
			remoteIncident.IncidentStatusId = incidentStatusId;
			remoteIncident.PriorityId = priorityId;
			remoteIncident.Name = "Open Issue";
			remoteIncident.Description = "This is a test open issue";
			remoteIncident.OpenerId = userId1;
			remoteIncident.OwnerId = userId2;
			incidentId2 = spiraImportExport.Incident_Create(remoteIncident).IncidentId.Value;
			//Incident #3
			remoteIncident = new RemoteIncident();
			remoteIncident.IncidentTypeId = incidentTypeId;
			remoteIncident.IncidentStatusId = incidentStatusId;
			remoteIncident.PriorityId = priorityId;
			remoteIncident.SeverityId = severityId;
			remoteIncident.Name = "Closed Bug";
			remoteIncident.Description = "This is a test closed bug";
			remoteIncident.OpenerId = userId1;
			remoteIncident.OwnerId = userId2;
			remoteIncident.TestRunStepId = testRunStepId2;
			remoteIncident.ClosedDate = DateTime.Now;
			incidentId3 = spiraImportExport.Incident_Create(remoteIncident).IncidentId.Value;

			//Finally add some resolutions to the incidents
			RemoteIncidentResolution[] remoteIncidentResolutions = new RemoteIncidentResolution[2];
			remoteIncidentResolutions[0] = new RemoteIncidentResolution();
			remoteIncidentResolutions[0].IncidentId = incidentId3;
			remoteIncidentResolutions[0].CreationDate = DateTime.Now.AddSeconds(-2);
			remoteIncidentResolutions[0].Resolution = "Resolution 1";
			remoteIncidentResolutions[0].CreatorId = userId1;
			remoteIncidentResolutions[1] = new RemoteIncidentResolution();
			remoteIncidentResolutions[1].IncidentId = incidentId3;
			remoteIncidentResolutions[1].CreationDate = DateTime.Now.AddSeconds(2);
			remoteIncidentResolutions[1].Resolution = "Resolution 2";
			remoteIncidentResolutions[1].CreatorId = userId1;
			spiraImportExport.Incident_AddResolutions(remoteIncidentResolutions);

			//Now verify that the import was successful

			//Retrieve the first incident
			remoteIncident = spiraImportExport.Incident_RetrieveById(incidentId1);
			Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "");
			Assert.AreEqual("New Incident", remoteIncident.Name, "Name");
			Assert.AreEqual(DateTime.Parse("1/5/2005"), remoteIncident.CreationDate, "CreationDate");
			Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
			Assert.IsNull(remoteIncident.OwnerName, "OwnerName");
			remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(incidentId1);
			Assert.AreEqual(0, remoteIncidentResolutions.Length);

			//Retrieve the second incident
			remoteIncident = spiraImportExport.Incident_RetrieveById(incidentId2);
			Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
			Assert.AreEqual("Open Issue", remoteIncident.Name, "Name");
			Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
			Assert.AreEqual(remoteIncident.OwnerName, "Martha T Muffin", "OwnerName");
			remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(incidentId2);
			Assert.AreEqual(0, remoteIncidentResolutions.Length);

			//Retrieve the third incident
			remoteIncident = spiraImportExport.Incident_RetrieveById(incidentId3);
			Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
			Assert.AreEqual(priorityId, remoteIncident.PriorityId, "PriorityId");
			Assert.AreEqual(severityId, remoteIncident.SeverityId, "SeverityId");
			Assert.AreEqual("Closed Bug", remoteIncident.Name, "Name");
			Assert.IsNotNull(remoteIncident.ClosedDate, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
			Assert.AreEqual("Martha T Muffin", remoteIncident.OwnerName, "OwnerName");
			remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(incidentId3);
			Assert.AreEqual(2, remoteIncidentResolutions.Length);
			Assert.AreEqual("Resolution 1", remoteIncidentResolutions[0].Resolution);
            Assert.AreEqual("Resolution 2", remoteIncidentResolutions[1].Resolution);

			//Finally make an update to one of the incidents
			remoteIncident = spiraImportExport.Incident_RetrieveById(incidentId3);
			remoteIncident.Name = "Reopened Bug";
			remoteIncident.ClosedDate = null;
			remoteIncident.OwnerId = userId1;
			spiraImportExport.Incident_Update(remoteIncident);

			//Verify the change
			remoteIncident = spiraImportExport.Incident_RetrieveById(incidentId3);
			Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
			Assert.AreEqual(priorityId, remoteIncident.PriorityId, "PriorityId");
			Assert.AreEqual(severityId, remoteIncident.SeverityId, "SeverityId");
			Assert.AreEqual("Reopened Bug", remoteIncident.Name, "Name");
			Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
			Assert.AreEqual("Adam Ant", remoteIncident.OwnerName, "OwnerName");
			remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(incidentId3);
			Assert.AreEqual(2, remoteIncidentResolutions.Length);
			Assert.AreEqual("Resolution 1", remoteIncidentResolutions[0].Resolution);
            Assert.AreEqual("Resolution 2", remoteIncidentResolutions[1].Resolution);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import attachments to project artifacts
		/// </summary>
		[
		Test,
		SpiraTestCase(471)
		]
		public void _14_ImportAttachments()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets try adding and attachment to a test case
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			RemoteDocument remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "test_data.xls";
			remoteDocument.Description = "Sample Test Case Attachment";
			remoteDocument.AuthorId = userId2;
			remoteDocument.ArtifactId = testCaseId1;
			remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			int attachmentId1 = spiraImportExport.Document_AddFile(remoteDocument, attachmentData).AttachmentId.Value;

			//Now lets get the attachment meta-data and verify
			Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
            List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId1, testCaseId1, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("test_data.xls", attachments[0].Filename);
			Assert.AreEqual("Sample Test Case Attachment", attachments[0].Description);
			Assert.AreEqual("Martha T Muffin", attachments[0].AuthorName);
			Assert.AreEqual(1, attachments[0].Size);

			//Need to test the URL method
			remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
			remoteDocument.Description = "Sample Test Case URL";
			remoteDocument.AuthorId = userId2;
			remoteDocument.ArtifactId = testCaseId2;
			remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			int attachmentId2 = spiraImportExport.Document_AddUrl(remoteDocument).AttachmentId.Value;

			//Now lets get the attachment meta-data and verify
            attachments = attachmentManager.RetrieveByArtifactId(projectId1, testCaseId2, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);
			Assert.AreEqual(1, attachments.Count);
			Assert.AreEqual("http://www.tempuri.org/test123.htm", attachments[0].Filename);
			Assert.AreEqual("Sample Test Case URL", attachments[0].Description);
			Assert.AreEqual("Martha T Muffin", attachments[0].AuthorName);
			Assert.AreEqual(0, attachments[0].Size);
		}

		/// <summary>
		/// Tests that we can import tasks into SpiraTeam/Plan
		/// </summary>
		[
		Test,
		SpiraTestCase(473)
		]
		public void _15_ImportTasks()
		{
            int tk_priorityMediumId = new TaskManager().TaskPriority_Retrieve(projectTemplateId1).FirstOrDefault(p => p.Score == 3).TaskPriorityId;

            //First lets authenticate and connect to the project
            spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now lets add a task that has all values set
			RemoteTask remoteTask = new RemoteTask();
			remoteTask.Name = "New Task 1";
			remoteTask.Description = "Task 1 Description";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			remoteTask.TaskPriorityId = tk_priorityMediumId;
			remoteTask.RequirementId = requirementId1;
			remoteTask.ReleaseId = iterationId2;
			remoteTask.OwnerId = userId2;
			remoteTask.StartDate = DateTime.Now;
			remoteTask.EndDate = DateTime.Now.AddDays(5);
			remoteTask.CompletionPercent = 25;
			remoteTask.EstimatedEffort = 100;
			remoteTask.ActualEffort = 120;
			taskId1 = spiraImportExport.Task_Create(remoteTask).TaskId.Value;

			//Verify the data
			remoteTask = spiraImportExport.Task_RetrieveById(taskId1);
			Assert.AreEqual("New Task 1", remoteTask.Name);
			Assert.AreEqual("Task 1 Description", remoteTask.Description);
			Assert.AreEqual("In Progress", remoteTask.TaskStatusName);
			Assert.AreEqual("3 - Medium", remoteTask.TaskPriorityName);
			Assert.AreEqual("1.0.002b", remoteTask.ReleaseVersionNumber);
			Assert.AreEqual(requirementId1, remoteTask.RequirementId);
			Assert.IsNotNull(remoteTask.StartDate);
			Assert.IsNotNull(remoteTask.EndDate);
			Assert.AreEqual(25, remoteTask.CompletionPercent);
			Assert.AreEqual(100, remoteTask.EstimatedEffort);
			Assert.AreEqual(120, remoteTask.ActualEffort);

			//Now lets add a task that has some values null
			remoteTask = new RemoteTask();
			remoteTask.Name = "New Task 2";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
			remoteTask.CompletionPercent = 0;
			taskId2 = spiraImportExport.Task_Create(remoteTask).TaskId.Value;

			//Verify the data
			remoteTask = spiraImportExport.Task_RetrieveById(taskId2);
			Assert.AreEqual("New Task 2", remoteTask.Name);
			Assert.IsNull(remoteTask.Description);
			Assert.AreEqual("Not Started", remoteTask.TaskStatusName);
			Assert.IsNull(remoteTask.TaskPriorityName);
			Assert.IsNull(remoteTask.ReleaseVersionNumber);
			Assert.IsNull(remoteTask.RequirementId);
			Assert.IsNull(remoteTask.StartDate);
			Assert.IsNull(remoteTask.EndDate);
			Assert.AreEqual(0, remoteTask.CompletionPercent);
			Assert.IsNull(remoteTask.EstimatedEffort);
			Assert.IsNull(remoteTask.ActualEffort);

			//Finally make an update to one of the tasks
			remoteTask = spiraImportExport.Task_RetrieveById(taskId2);
			remoteTask.Name = "New Task 2b";
			remoteTask.Description = "New Task 2b Description";
			remoteTask.ReleaseId = iterationId1;
			remoteTask.StartDate = DateTime.Now.AddDays(1).Date;
			remoteTask.EndDate = DateTime.Now.AddDays(5).Date;
			spiraImportExport.Task_Update(remoteTask);

			//Verify the change
			remoteTask = spiraImportExport.Task_RetrieveById(taskId2);
			Assert.AreEqual("New Task 2b", remoteTask.Name);
			Assert.AreEqual("New Task 2b Description", remoteTask.Description);
			Assert.AreEqual("Not Started", remoteTask.TaskStatusName);
			Assert.IsNull(remoteTask.TaskPriorityName);
			Assert.AreEqual("1.0.001", remoteTask.ReleaseVersionNumber);
			Assert.IsNull(remoteTask.RequirementId);
			Assert.AreEqual(DateTime.Now.AddDays(1).Date, remoteTask.StartDate);
			Assert.AreEqual(DateTime.Now.AddDays(5).Date, remoteTask.EndDate);
			Assert.AreEqual(0, remoteTask.CompletionPercent);
			Assert.IsNull(remoteTask.EstimatedEffort);
			Assert.IsNull(remoteTask.ActualEffort);
		}

		/// <summary>
		/// Verifies that you can retrieve the incidents from a specific date
		/// </summary>
		[
		Test,
		SpiraTestCase(462)
		]
		public void _16_RetrieveIncidents()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now lets retrieve all incidents since a certain date
			RemoteIncident[] remoteIncidents = spiraImportExport.Incident_RetrieveNew(DateTime.Parse("1/1/2000"));

			//Verify the data returned
			Assert.AreEqual(3, remoteIncidents.Length);
			RemoteIncident remoteIncident = remoteIncidents[2];
			Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "");
			Assert.AreEqual("New Incident", remoteIncident.Name, "Name");
			Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
			Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
			Assert.IsNull(remoteIncident.OwnerName, "OwnerName");
            RemoteIncidentResolution[] remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(remoteIncident.IncidentId.Value);
			Assert.AreEqual(0, remoteIncidentResolutions.Length);

            //Now lets retrieve all incidents from '1/1/1900', used after a 'Force Resync' by most plugins
            remoteIncidents = spiraImportExport.Incident_RetrieveNew(DateTime.Parse("1/1/1900"));

            //Verify the data returned
            Assert.AreEqual(3, remoteIncidents.Length);
            remoteIncident = remoteIncidents[2];
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "");
            Assert.AreEqual("New Incident", remoteIncident.Name, "Name");
            Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.IsNull(remoteIncident.OwnerName, "OwnerName");
            remoteIncidentResolutions = spiraImportExport.Incident_RetrieveResolutions(remoteIncident.IncidentId.Value);
            Assert.AreEqual(0, remoteIncidentResolutions.Length);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();

			//Now lets test that we can retrieve a generic filtered list of P1,P2 open incidents from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "PriorityId";
			List<int> multiValues = new List<int>();
			multiValues.Add(1);
			multiValues.Add(2);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "IncidentStatusId";
			multiValues = new List<int>();
            multiValues.Add(IncidentManager.IncidentStatusId_AllOpen);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "CreationDate";
			remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-100);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow;
            remoteFilters.Add(remoteFilter);
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "Name";
			remoteSort.SortAscending = true;
			remoteIncidents = spiraImportExport.Incident_Retrieve(remoteFilters.ToArray(), remoteSort, 1, 999999);

            //Verify the data returned
            Assert.AreEqual(12, remoteIncidents.Length);
            Assert.AreEqual("Ability to associate multiple authors", remoteIncidents[0].Name);
            Assert.AreEqual("Test Training Item", remoteIncidents[11].Name);

            //Now test that we can get a list of incidents assigned to the currently authenticated user
            remoteIncidents = spiraImportExport.Incident_RetrieveForOwner();

			//Verify the data returned
			Assert.AreEqual(9, remoteIncidents.Length);
			Assert.AreEqual("Ability to associate multiple authors", remoteIncidents[0].Name);
			Assert.AreEqual("Sample Problem 3", remoteIncidents[8].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve the incidents from a specific date
		/// </summary>
		[
		Test,
		SpiraTestCase(478)
		]
		public void _17_RetrieveTasks()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now lets retrieve all tasks since a certain date
			RemoteTask[] remoteTasks = spiraImportExport.Task_RetrieveNew(DateTime.Parse("1/1/2000"));

			//Verify the data returned
			Assert.AreEqual(2, remoteTasks.Length);
			RemoteTask remoteTask = remoteTasks[0];
			Assert.AreEqual("New Task 1", remoteTask.Name);
			Assert.AreEqual("Task 1 Description", remoteTask.Description);
			Assert.AreEqual("In Progress", remoteTask.TaskStatusName);
			Assert.AreEqual("3 - Medium", remoteTask.TaskPriorityName);
			Assert.AreEqual("1.0.002b", remoteTask.ReleaseVersionNumber);
			Assert.AreEqual(requirementId1, remoteTask.RequirementId);
			Assert.IsNotNull(remoteTask.StartDate);
			Assert.IsNotNull(remoteTask.EndDate);
			Assert.AreEqual(25, remoteTask.CompletionPercent);
			Assert.AreEqual(100, remoteTask.EstimatedEffort);
			Assert.AreEqual(120, remoteTask.ActualEffort);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();

			//Now lets test that we can retrieve a generic filtered list of P1,P2 completed Release 1 tasks from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "TaskPriorityId";
			List<int> multiValues = new List<int>();
			multiValues.Add(1);
			multiValues.Add(2);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ProgressId";
			remoteFilter.IntValue = 5;  //Completed
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ReleaseId";
			remoteFilter.IntValue = 1;  //Release 1.0.0.0 and child iterations
			remoteFilters.Add(remoteFilter);
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "TaskId";
			remoteSort.SortAscending = true;
			remoteTasks = spiraImportExport.Task_Retrieve(remoteFilters.ToArray(), remoteSort, 1, 999999);

			//Verify the data returned
			Assert.AreEqual(15, remoteTasks.Length);
			Assert.AreEqual("Develop new book entry screen", remoteTasks[0].Name);
			Assert.AreEqual("Write book object delete query", remoteTasks[8].Name);

			//Now test that we can get a list of tasks assigned to the currently authenticated user
			remoteTasks = spiraImportExport.Task_RetrieveForOwner();

            //Verify the data returned
            Assert.AreEqual(5, remoteTasks.Length);
            Assert.AreEqual("Schedule meeting with customer to discuss scope", remoteTasks[0].Name);
            Assert.AreEqual("Write subject object update queries", remoteTasks[4].Name);

            //Finally disconnect from the project
            spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of requirements
		/// </summary>
		[
		Test,
		SpiraTestCase(557)
		]
		public void _18_RetrieveRequirements()
		{
			//Now lets test that we can retrieve a generic list of requirements from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ImportanceId";
			List<int> multiValues = new List<int>();
			multiValues.Add(1);
			multiValues.Add(2);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "CoverageId";
			remoteFilter.IntValue = 4;  //< 100% Run
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ReleaseId";
			remoteFilter.IntValue = 1;  //Release 1.0.0.0
			remoteFilters.Add(remoteFilter);
			RemoteRequirement[] remoteRequirements = spiraImportExport.Requirement_Retrieve(remoteFilters.ToArray(), 1, 999999);

			//Verify the data returned
			Assert.AreEqual(4, remoteRequirements.Length);
			Assert.AreEqual("Functional System Requirements", remoteRequirements[0].Name);
			Assert.AreEqual("Ability to delete existing books in the system", remoteRequirements[3].Name);

			//Now test that we can get a list of requirements assigned to the currently authenticated user
			remoteRequirements = spiraImportExport.Requirement_RetrieveForOwner();

			//Verify the data returned
			Assert.AreEqual(4, remoteRequirements.Length);
			Assert.AreEqual("Ability to edit existing authors in the system", remoteRequirements[0].Name);
			Assert.AreEqual("Ability to import from legacy system x", remoteRequirements[3].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of test cases
		/// </summary>
		[
		Test,
		SpiraTestCase(558)
		]
		public void _19_RetrieveTestCases()
		{
			//Now lets test that we can retrieve a generic list of test cases and test runs from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "OwnerId";
			List<int> multiValues = new List<int>();
			multiValues.Add(2);
			multiValues.Add(3);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ActiveYn";
			remoteFilter.StringValue = "Y";
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ExecutionDate";
			remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-150);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow;
            remoteFilters.Add(remoteFilter);
			RemoteTestCase[] remoteTestCases = spiraImportExport.TestCase_Retrieve(remoteFilters.ToArray(), 1, 999999);

            //Verify the data returned, all folders are currently returned since v5.0
			Assert.AreEqual(10, remoteTestCases.Length);
			Assert.AreEqual("Common Tests", remoteTestCases[0].Name);
            Assert.AreEqual("Functional Tests", remoteTestCases[1].Name);
            Assert.AreEqual("Ability to reassign book to different author", remoteTestCases[6].Name);

			//Now test that we can get the list of test cases in a specific release (filtered)
			remoteTestCases = spiraImportExport.TestCase_RetrieveByReleaseId(1, remoteFilters.ToArray(), 1, 999999);
            Assert.AreEqual(5, remoteTestCases.Length);
            Assert.AreEqual("Ability to create new author", remoteTestCases[0].Name);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCases[0].ExecutionStatusId);
            Assert.AreEqual("Ability to edit existing book", remoteTestCases[3].Name);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, remoteTestCases[3].ExecutionStatusId);

			//Now test that we can get a filtered list of test runs
			remoteFilters = new List<RemoteFilter>();
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "TestCaseId";
			remoteFilter.IntValue = 2;
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ExecutionStatusId";
			remoteFilter.IntValue = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteFilters.Add(remoteFilter);
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "EndDate";
			remoteSort.SortAscending = false;
			RemoteTestRun[] remoteTestRuns = spiraImportExport.TestRun_Retrieve(remoteFilters.ToArray(), remoteSort, 1, 999999);

            //Verify the data returned
            Assert.AreEqual(4, remoteTestRuns.Length);
            Assert.IsTrue(remoteTestRuns[0].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[1].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[2].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[3].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));

            //Now test that we can get a list of test sets assigned to the currently authenticated user
            remoteTestCases = spiraImportExport.TestCase_RetrieveForOwner();

			//Verify the data returned
			Assert.AreEqual(2, remoteTestCases.Length);
			Assert.AreEqual("Ability to create new book", remoteTestCases[0].Name);
			Assert.AreEqual("Ability to edit existing book", remoteTestCases[1].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of releases
		/// </summary>
		[
		Test,
		SpiraTestCase(559)
		]
		public void _20_RetrieveReleases()
		{
			//Now lets test that we can retrieve a generic list of releases from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "IterationYn";
			remoteFilter.StringValue = "N";
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "CoverageId";
			remoteFilter.IntValue = 4;  //< 100% Run
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "StartDate";
			remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-25);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow.AddDays(-5);
            remoteFilters.Add(remoteFilter);
			RemoteRelease[] remoteReleases = spiraImportExport.Release_Retrieve2(remoteFilters.ToArray(), 1, 999999);

			//Verify the data returned
            Assert.AreEqual(3, remoteReleases.Length);
            Assert.AreEqual("Library System Release 1.1 SP1", remoteReleases[0].Name);
            //Assert.AreEqual("Library System Release 1.1 SP1", remoteReleases[1].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of test sets
		/// </summary>
		[
		Test,
		SpiraTestCase(560)
		]
		public void _21_RetrieveTestSets()
		{
			//Now lets test that we can retrieve a generic list of test sets from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "OwnerId";
			List<int> multiValues = new List<int>();
			multiValues.Add(2);
			multiValues.Add(3);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "TestSetStatusId";
			multiValues = new List<int>();
			multiValues.Add((int)Task.TaskStatusEnum.NotStarted);
			multiValues.Add((int)Task.TaskStatusEnum.InProgress);
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = multiValues.ToArray();
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "PlannedDate";
			remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-126);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow.AddDays(-8);
            remoteFilters.Add(remoteFilter);
			RemoteTestSet[] remoteTestSets = spiraImportExport.TestSet_Retrieve(remoteFilters.ToArray(), 1, 999999);

			//Verify the data returned, all folders are currently returned since v5.0
			Assert.AreEqual(4, remoteTestSets.Length);
			Assert.AreEqual("Functional Test Sets", remoteTestSets[0].Name);
            Assert.AreEqual("Testing Cycle for Release 1.0", remoteTestSets[1].Name);
            Assert.AreEqual("Regression Test Sets", remoteTestSets[3].Name);

			//Now test that we can get a list of test sets assigned to the currently authenticated user
			remoteTestSets = spiraImportExport.TestSet_RetrieveForOwner();

			//Verify the data returned
			Assert.AreEqual(3, remoteTestSets.Length);
            Assert.AreEqual("Regression Testing for Windows 8", remoteTestSets[0].Name);
            Assert.AreEqual("Exploratory Testing", remoteTestSets[1].Name);
            Assert.AreEqual("Testing New Functionality", remoteTestSets[2].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that the System_Version() function returns a non-null value.
		/// </summary>
		[
		Test,
		SpiraTestCase(575)
		]
		public void _22_RetrieveVersion()
		{
			// Log in, tho this may not even be necessary.
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			// Pull the version object.
			RemoteVersion remoteVersion = spiraImportExport.System_GetProductVersion();
			// Verify they meet requirements.
			Assert.IsTrue(remoteVersion.Version != null, "Version string was null!");
			Assert.IsTrue(remoteVersion.Version.Length >= 3, "Version string was too short!");
			Assert.IsTrue(remoteVersion.Patch.HasValue, "Patch version was null!");

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that settings are pulled from the system.
		/// </summary>
		[
		Test,
		SpiraTestCase(576)
		]
		public void _23_RetrieveSettings()
		{
			// Log in, tho this may not even be necessary.
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			// Pull the version object.
			RemoteSetting[] remoteSettings = spiraImportExport.System_GetSettings();
			// Verify they meet requirements.
			Assert.IsTrue(remoteSettings.Length > 0, "There were no settings returned!");

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Tests that we can retrieve a list of project roles
		/// </summary>
		[
		Test,
		SpiraTestCase(579)
		]
		public void _24_RetrieveProjectRoles()
		{
			// Authenticate with the API
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");

			//First test that we can retrieve the list of all project roles
			RemoteProjectRole[] remoteProjectRoles = spiraImportExport.ProjectRole_Retrieve();
			Assert.AreEqual(6, remoteProjectRoles.Length);
			Assert.AreEqual(InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, remoteProjectRoles[0].ProjectRoleId);
			Assert.AreEqual("Product Owner", remoteProjectRoles[0].Name);
			Assert.AreEqual("Can see all product artifacts. Can create/modify all artifacts. Can access the product/template administration tools", remoteProjectRoles[0].Description);
			Assert.IsTrue(remoteProjectRoles[0].Active, "Active");
			Assert.IsTrue(remoteProjectRoles[0].Admin, "Admin");
			Assert.IsTrue(remoteProjectRoles[0].DocumentsAdd, "DocumentsAdd");
			Assert.IsTrue(remoteProjectRoles[0].DocumentsEdit, "DocumentsEdit");
			Assert.IsTrue(remoteProjectRoles[0].DocumentsDelete, "DocumentsDelete");
			Assert.IsTrue(remoteProjectRoles[0].DiscussionsAdd, "DiscussionsAdd");

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Tests that the API handles data concurrency correctly
		/// </summary>
		[
		Test,
		SpiraTestCase(582)
		]
		public void _25_Concurrency_Handling()
		{
			//Connect to the pre-existing project 2
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

            //Get the template associated with the project
            int projectTemplateId2 = new TemplateManager().RetrieveForProject(projectId1).ProjectTemplateId;
            int tk_priorityLowId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 4).TaskPriorityId;
            int tk_priorityMediumId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
            int tk_priorityHighId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 2).TaskPriorityId;

            //Now lets add a task that has all values set
            RemoteTask remoteTask = new RemoteTask();
			remoteTask.Name = "New Task 1";
			remoteTask.Description = "Task 1 Description";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
			remoteTask.TaskPriorityId = tk_priorityMediumId;
			remoteTask.StartDate = DateTime.Now;
			remoteTask.EndDate = DateTime.Now.AddDays(5);
			remoteTask.CompletionPercent = 0;
			remoteTask.EstimatedEffort = 100;
			remoteTask.ActualEffort = 120;
			int taskId = spiraImportExport.Task_Create(remoteTask).TaskId.Value;

			//Now retrieve the task back into two copies
			RemoteTask remoteTask1 = spiraImportExport.Task_RetrieveById(taskId);
			RemoteTask remoteTask2 = spiraImportExport.Task_RetrieveById(taskId);

			//Now make a change to field and update
			remoteTask1.TaskPriorityId = tk_priorityHighId;
			spiraImportExport.Task_Update(remoteTask1);

			//Verify it updated correctly using separate data object
			RemoteTask remoteTask3 = spiraImportExport.Task_RetrieveById(taskId);
			Assert.AreEqual(tk_priorityHighId, remoteTask3.TaskPriorityId);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			bool exceptionThrown = false;
			try
			{
				remoteTask2.TaskPriorityId = tk_priorityLowId;
				spiraImportExport.Task_Update(remoteTask2);
			}
			catch (SoapException soapException)
			{
				if (soapException.Detail.ChildNodes.Count > 0 && soapException.Detail.ChildNodes[0].Name == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteTask3 = spiraImportExport.Task_RetrieveById(taskId);
			Assert.AreEqual(tk_priorityHighId, remoteTask3.TaskPriorityId);

			//Now refresh the old dataset and try again and verify it works
			remoteTask2 = spiraImportExport.Task_RetrieveById(taskId);
			remoteTask2.TaskPriorityId = tk_priorityLowId;
			spiraImportExport.Task_Update(remoteTask2);

			//Verify it updated correctly
			remoteTask3 = spiraImportExport.Task_RetrieveById(taskId);
			Assert.AreEqual(tk_priorityLowId, remoteTask3.TaskPriorityId);

			//Clean up (can't use the API as it doesn't have deletes)
			TaskManager task = new TaskManager();
			task.MarkAsDeleted(projectId1, taskId, userId1);

			//Now lets add a incident that has all values set
			RemoteIncident remoteIncident = new RemoteIncident();
			remoteIncident.Name = "New Incident 1";
			remoteIncident.IncidentStatusId = -1;   //Signifies the default
			remoteIncident.IncidentTypeId = -1;     //Signifies the default
			remoteIncident.Description = "Incident 1 Description";
			remoteIncident.StartDate = DateTime.Now;
			remoteIncident.CompletionPercent = 0;
			remoteIncident.EstimatedEffort = 100;
			remoteIncident.ActualEffort = 120;
			int incidentId = spiraImportExport.Incident_Create(remoteIncident).IncidentId.Value;

			//Now retrieve the incident back into two copies
			RemoteIncident remoteIncident1 = spiraImportExport.Incident_RetrieveById(incidentId);
			RemoteIncident remoteIncident2 = spiraImportExport.Incident_RetrieveById(incidentId);

			//Now make a change to field and update
			remoteIncident1.Name = "New Incident 1 Mod001";
			spiraImportExport.Incident_Update(remoteIncident1);

			//Verify it updated correctly using separate data object
			RemoteIncident remoteIncident3 = spiraImportExport.Incident_RetrieveById(incidentId);
			Assert.AreEqual("New Incident 1 Mod001", remoteIncident3.Name);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteIncident2.Name = "New Incident 1 Mod002";
				spiraImportExport.Incident_Update(remoteIncident2);
			}
			catch (SoapException soapException)
			{
                if (soapException.Detail.ChildNodes.Count > 0 && soapException.Detail.ChildNodes[0].Name == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteIncident3 = spiraImportExport.Incident_RetrieveById(incidentId);
			Assert.AreEqual("New Incident 1 Mod001", remoteIncident3.Name);

			//Now refresh the old dataset and try again and verify it works
			remoteIncident2 = spiraImportExport.Incident_RetrieveById(incidentId);
			remoteIncident2.Name = "New Incident 1 Mod002";
			spiraImportExport.Incident_Update(remoteIncident2);

			//Verify it updated correctly
			remoteIncident3 = spiraImportExport.Incident_RetrieveById(incidentId);
			Assert.AreEqual("New Incident 1 Mod002", remoteIncident3.Name);

			//Clean up (can't use the API as it doesn't have deletes)
            IncidentManager incidentManager = new IncidentManager();
			incidentManager.MarkAsDeleted(projectId1, incidentId, USER_ID_FRED_BLOGGS);
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>Pulls a smaple workflow transition.</summary>
		[
			Test,
			SpiraTestCase(583)
		]
		public void _26_WorkflowTransition_Pulling()
		{
			const int INCIDENT_ID = 3;

			//Connect to the pre-existing project 2
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(PROJECT_ID);

			//Get the incident.
			RemoteIncident incident = spiraImportExport.Incident_RetrieveById(INCIDENT_ID);

			//Fields
			bool isOwner = false;
			bool isDetector = false;
			if (incident.OpenerName != null)
				isOwner = ( incident.OpenerName.ToLowerInvariant() == "administrator" );
			if (incident.OwnerName != null)
				isDetector = ( incident.OwnerName.ToLowerInvariant() == "administrator" );

			//Get transitions.
			RemoteWorkflowIncidentTransition[] transitions = spiraImportExport.Incident_RetrieveWorkflowTransitions(incident.IncidentTypeId, incident.IncidentStatusId, isDetector, isOwner);

			//Errorchecking.
			Assert.AreEqual(transitions.Length, 2);
			bool isOneOpen = false;
			bool isOneAssigned = false;
			for (int i = 0; i < transitions.Length; i++)
			{
				if (transitions[i].IncidentStatusName_Output == "Open")
					isOneOpen = true;
				if (transitions[i].IncidentStatusName_Output == "Assigned")
					isOneAssigned = true;
			}
			Assert.AreEqual(isOneAssigned, true);
			Assert.AreEqual(isOneOpen, true);
		}

		/// <summary>Pulls allowed fields.</summary>
		[
			Test,
			SpiraTestCase(585)
		]
		public void _27_WorkflowTransitionFields_Pulling()
		{
			const int INCIDENT_ID = 3;

			//Connect to the pre-existing project 2
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(PROJECT_ID);

			//Get the incident.
			RemoteIncident incident = spiraImportExport.Incident_RetrieveById(INCIDENT_ID);

			RemoteWorkflowIncidentFields[] fields = spiraImportExport.Incident_RetrieveWorkflowFields(incident.IncidentTypeId, incident.IncidentStatusId);
			Assert.AreEqual(fields.Length, 16);
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Type" && f.FieldStatus == 2));
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Started On" && f.FieldStatus == 1));
		}

        /// <summary>
        /// Tests that when we use the old static ID values for the following artifact fields, the API handles them correctly:
        /// - Requirement
        ///   - Importance
        /// - Task
        ///   - Priority
        /// - Test Case
        ///   - Priority
        /// </summary>
        /// <remarks>
        /// If there is no match, the default value should be used. Instead of an ID from the incorrect project
        /// </remarks>
        [Test]
        [SpiraTestCase(1860)]
        public void _28_CheckCompatibilityWithOldStaticIDs()
        {
            //First lets authenticate and create a new project
            spiraImportExport.Connection_Authenticate("administrator", "Welcome123$");
            RemoteProject remoteProject = new RemoteProject();
            remoteProject.Name = "API Compatibility Test Project";
            remoteProject.Active = true;
            remoteProject = spiraImportExport.Project_Create(remoteProject, null);
            projectId3 = remoteProject.ProjectId.Value;
            spiraImportExport.Connection_ConnectToProject(projectId3);

            //Get the template associated with the project
            projectTemplateId3 = new TemplateManager().RetrieveForProject(projectId3).ProjectTemplateId;

            //Lets add a new requirement with the old static IDs
            RemoteRequirement remoteRequirement = new RemoteRequirement();
            remoteRequirement.StatusId = 1;
            remoteRequirement.ImportanceId = 2; //High
            remoteRequirement.Name = "Test Requirement";
            remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
            int requirementId = remoteRequirement.RequirementId.Value;

            //Verify the data
            remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId);
            Assert.AreNotEqual(1, remoteRequirement.ImportanceId);

            //Lets add a new requirement using the other API method
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.StatusId = 1;
            remoteRequirement.ImportanceId = 2; //High
            remoteRequirement.Name = "Test Requirement 2";
            remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, null);
            requirementId = remoteRequirement.RequirementId.Value;

            //Verify the data
            remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId);
            Assert.AreNotEqual(1, remoteRequirement.ImportanceId);

            //Now try and update it back and verify it remains with the correct ID
            remoteRequirement.ImportanceId = 2;
            spiraImportExport.Requirement_Update(remoteRequirement);

            //Verify the data
            remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId);
            Assert.AreNotEqual(1, remoteRequirement.ImportanceId);

            //Now create a task with the old static IDs
            RemoteTask remoteTask = new RemoteTask();
            remoteTask.Name = "Sample Task";
            remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
            remoteTask.TaskPriorityId = 3;  //Medium
            int taskId = spiraImportExport.Task_Create(remoteTask).TaskId.Value;

            //Verify the data
            remoteTask = spiraImportExport.Task_RetrieveById(taskId);
            Assert.AreNotEqual(3, remoteTask.TaskPriorityId);

            //Now try and update it back and verify it remains with the correct ID
            remoteTask.TaskPriorityId = 3;
            spiraImportExport.Task_Update(remoteTask);

            //Verify the data
            remoteTask = spiraImportExport.Task_RetrieveById(taskId);
            Assert.AreNotEqual(3, remoteTask.TaskPriorityId);

            //Now create a test case with the old static IDs
            RemoteTestCase remoteTestCase = new RemoteTestCase();
            remoteTestCase.Name = "Sample Test Case";
            remoteTestCase.TestCasePriorityId = 3;  //Medium
            int testCaseId = spiraImportExport.TestCase_Create(remoteTestCase, null).TestCaseId.Value;

            //Verify the data
            remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
            Assert.AreNotEqual(3, remoteTestCase.TestCasePriorityId);

            //Now try and update it back and verify it remains with the correct ID
            remoteTestCase.TestCasePriorityId = 3;
            spiraImportExport.TestCase_Update(remoteTestCase, null);

            //Verify the data
            remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
            Assert.AreNotEqual(3, remoteTestCase.TestCasePriorityId);
        }
    }
}
