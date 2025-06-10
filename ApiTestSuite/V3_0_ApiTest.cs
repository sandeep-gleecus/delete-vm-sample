using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.TestSuite;
using Inflectra.SpiraTest.ApiTestSuite.SpiraImportExport30;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

namespace Inflectra.SpiraTest.ApiTestSuite.API_Tests
{
	/// <summary>
	/// This fixture tests that the v3.0 import/export web service interface works correctly
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class V3_0_ApiTest
	{
		protected static SpiraImportExport30.ImportExportClient spiraImportExport;

		protected static long lastArtifactHistoryId;
		protected static long lastArtifactChangeSetId;

		protected static int projectId1;
		protected static int projectId2;
        protected static int projectId3;
        protected static int projectTemplateId1;
        protected static int projectTemplateId3;

        protected static int customListId1;
		protected static int customListId2;
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

		protected static int attachmentId1;
		protected static int attachmentId2;
		protected static int attachmentId3;
		protected static int projectAttachmentFolderId;

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

		protected static int testRunId1;
		protected static int testRunId2;
		protected static int testRunStepId1;
		protected static int testRunStepId2;
		protected static int testRunStepId3;
		protected static int testRunId3;
		protected static int testRunId4;
		protected static int testRunId5;

		protected static int taskId1;
		protected static int taskId2;

		private const int PROJECT_ID = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;
		private const int AUTOMATION_ENGINE_ID_QTP = 2;
        private const int USER_ID_SYSTEM_ADMIN = 1;

        /// <summary>
        /// Sets up the web service interface
        /// </summary>
        [TestFixtureSetUp]
		public void Init()
		{
			//Instantiate the web-service proxy class and set the URL from the .config file
			spiraImportExport = new SpiraImportExport30.ImportExportClient();
			spiraImportExport.Endpoint.Address = new EndpointAddress(Properties.Settings.Default.WebServiceUrl + "v3_0/ImportExport.svc");

			//Configure the HTTP Binding to handle session cookies
			BasicHttpBinding httpBinding = (BasicHttpBinding)spiraImportExport.Endpoint.Binding;
			httpBinding.AllowCookies = true;
			httpBinding.Security.Mode = BasicHttpSecurityMode.None;

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

            //Delete the template (no v3.0 api call available for this)
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
		SpiraTestCase(639)
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
		SpiraTestCase(640)
		]
		public void _02_RetrieveProjectList()
		{
			//Download the project list for a normal user with single-case password and username
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			RemoteProject[] remoteProjects = spiraImportExport.Project_Retrieve();

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
			catch (FaultException<ServiceFaultMessage> exception)
			{
				//Need to get the underlying message fault
				ServiceFaultMessage detail = exception.Detail;
				if (detail.Type == "SessionNotAuthenticated")
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
			catch (FaultException<ServiceFaultMessage> exception)
			{
				//Need to get the underlying message fault
				ServiceFaultMessage detail = exception.Detail;
				if (detail.Type == "SessionNotAuthenticated")
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
		SpiraTestCase(650)
		]
		public void _03_CreateProject()
		{
			//Create a new project into which we will import various other artifacts
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			RemoteProject remoteProject = new RemoteProject();
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
			remoteProject = new RemoteProject();
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
		SpiraTestCase(653)
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
			Assert.AreEqual("Adam", remoteProjectUsers[0].FirstName);
			Assert.AreEqual("Martha", remoteProjectUsers[1].FirstName);
			Assert.AreEqual("Ant", remoteProjectUsers[0].LastName);
			Assert.AreEqual("Muffin", remoteProjectUsers[1].LastName);
			Assert.AreEqual("aant@antmusic.com", remoteProjectUsers[0].EmailAddress);
			Assert.AreEqual("mmuffin@echobeach.biz", remoteProjectUsers[1].EmailAddress);
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
		SpiraTestCase(652)
		]
		public void _05_ImportCustomProperties()
		{
			//First lets authenticate and connect to the new project as an administrator
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets see if we have any requirement custom properties
			RemoteCustomProperty[] remoteCustomProperties = spiraImportExport.CustomProperty_RetrieveForArtifactType((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(0, remoteCustomProperties.Length);

			//Now add a new custom list to the project with one value
			RemoteCustomListValue remoteCustomListValue = new RemoteCustomListValue();
			remoteCustomListValue.Name = "Feature";
			remoteCustomListValue.Active = true;
			RemoteCustomList remoteCustomList = new RemoteCustomList();
			remoteCustomList.Name = "Req Types";
			remoteCustomList.Active = true;
			remoteCustomList.Values = new RemoteCustomListValue[] { remoteCustomListValue };
			customListId1 = spiraImportExport.CustomProperty_AddCustomList(remoteCustomList).CustomPropertyListId.Value;

			//Now add another value to the existing custom list
			remoteCustomListValue = new RemoteCustomListValue();
			remoteCustomListValue.CustomPropertyListId = customListId1;
			remoteCustomListValue.Name = "Technical Quality";
			remoteCustomListValue.Active = true;
			customValueId2 = spiraImportExport.CustomProperty_AddCustomListValue(remoteCustomListValue).CustomPropertyValueId.Value;

			//Lets verify that the list was correctly populated
			remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId1);
			Assert.AreEqual("Req Types", remoteCustomList.Name);
			Assert.AreEqual(true, remoteCustomList.Active);
			Assert.AreEqual(2, remoteCustomList.Values.Length);
			//Value 1
			Assert.AreEqual("Feature", remoteCustomList.Values[0].Name);
			Assert.AreEqual(true, remoteCustomList.Values[0].Active);
			//Value 2
			Assert.AreEqual("Technical Quality", remoteCustomList.Values[1].Name);
			Assert.AreEqual(true, remoteCustomList.Values[1].Active);

			//Now lets add a second list with values
			remoteCustomListValue = new RemoteCustomListValue();
            remoteCustomListValue.Name = "Component One";
			remoteCustomListValue.Active = true;
			remoteCustomList = new RemoteCustomList();
			remoteCustomList.Name = "Components";
			remoteCustomList.Active = true;
			remoteCustomList.Values = new RemoteCustomListValue[] { remoteCustomListValue };
			customListId2 = spiraImportExport.CustomProperty_AddCustomList(remoteCustomList).CustomPropertyListId.Value;

			//Now lets verify that we can retrieve all the lists in the project
			RemoteCustomList[] remoteCustomLists = spiraImportExport.CustomProperty_RetrieveCustomLists();
			Assert.AreEqual(2, remoteCustomLists.Length);
			Assert.AreEqual("Req Types", remoteCustomLists[0].Name);
			Assert.AreEqual(true, remoteCustomLists[0].Active);
			Assert.AreEqual("Components", remoteCustomLists[1].Name);
			Assert.AreEqual(true, remoteCustomLists[1].Active);

			//Verify that we can modify a custom list / custom list value
			remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId2);
			remoteCustomList.Name = "Component Names";
			remoteCustomList.Values[0].Name = "Component One";
			spiraImportExport.CustomProperty_UpdateCustomList(remoteCustomList);

			//Verify the changes
			remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId2);
			Assert.AreEqual("Component Names", remoteCustomList.Name);
			Assert.AreEqual(true, remoteCustomList.Active);
			Assert.AreEqual(1, remoteCustomList.Values.Length);
			Assert.AreEqual("Component One", remoteCustomList.Values[0].Name);
			Assert.AreEqual(true, remoteCustomList.Values[0].Active);

			//Get a reference to one of the lists
			remoteCustomList = spiraImportExport.CustomProperty_RetrieveCustomListById(customListId1);

			//Now lets add a text custom property and one list value to both requirements and incidents
			List<RemoteCustomProperty> remoteCustomPropertiesNew = new List<RemoteCustomProperty>();
			//Text Property
			RemoteCustomProperty remoteCustomProperty = new RemoteCustomProperty();
			remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			remoteCustomProperty.CustomPropertyId = 1;
			remoteCustomProperty.ProjectId = projectId1;
			remoteCustomProperty.Alias = "Source";
			remoteCustomPropertiesNew.Add(remoteCustomProperty);
			//List Property
			remoteCustomProperty = new RemoteCustomProperty();
			remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			remoteCustomProperty.CustomPropertyId = 11;
			remoteCustomProperty.ProjectId = projectId1;
			remoteCustomProperty.Alias = "Req Type";
			remoteCustomProperty.CustomList = remoteCustomList;
			remoteCustomPropertiesNew.Add(remoteCustomProperty);
			spiraImportExport.CustomProperty_UpdateCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Requirement, remoteCustomPropertiesNew.ToArray());

			//Verify the additions
			remoteCustomProperties = spiraImportExport.CustomProperty_RetrieveForArtifactType((int)DataModel.Artifact.ArtifactTypeEnum.Requirement);
			Assert.AreEqual(2, remoteCustomProperties.Length);
            Assert.AreEqual("TEXT_01", remoteCustomProperties.FirstOrDefault(c => c.Alias == "Source").CustomPropertyName);
            Assert.AreEqual("LIST_01", remoteCustomProperties.FirstOrDefault(c => c.Alias == "Req Type").CustomPropertyName);
            Assert.AreEqual(1, remoteCustomProperties.FirstOrDefault(c => c.Alias == "Source").CustomPropertyTypeId);
            Assert.AreEqual(2, remoteCustomProperties.FirstOrDefault(c => c.Alias == "Req Type").CustomPropertyTypeId);

			//Now add the same two custom properties to incidents
			remoteCustomPropertiesNew = new List<RemoteCustomProperty>();
			//Text Property
			remoteCustomProperty = new RemoteCustomProperty();
			remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteCustomProperty.CustomPropertyId = 1;
			remoteCustomProperty.ProjectId = projectId1;
			remoteCustomProperty.Alias = "Source";
			remoteCustomPropertiesNew.Add(remoteCustomProperty);
			//List Property
			remoteCustomProperty = new RemoteCustomProperty();
			remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteCustomProperty.CustomPropertyId = 11;
			remoteCustomProperty.ProjectId = projectId1;
			remoteCustomProperty.Alias = "Req Type";
			remoteCustomProperty.CustomList = remoteCustomList;
			remoteCustomPropertiesNew.Add(remoteCustomProperty);
			spiraImportExport.CustomProperty_UpdateCustomProperties((int)DataModel.Artifact.ArtifactTypeEnum.Incident, remoteCustomPropertiesNew.ToArray());

			//Verify the additions
			remoteCustomProperties = spiraImportExport.CustomProperty_RetrieveForArtifactType((int)DataModel.Artifact.ArtifactTypeEnum.Incident);
			Assert.AreEqual(2, remoteCustomProperties.Length);
            Assert.AreEqual("TEXT_01", remoteCustomProperties.FirstOrDefault(c => c.Alias == "Source").CustomPropertyName);
            Assert.AreEqual("LIST_01", remoteCustomProperties.FirstOrDefault(c => c.Alias == "Req Type").CustomPropertyName);

			//Also verify that the values of the custom list are returned as well
			remoteCustomList = remoteCustomProperties[1].CustomList;
			Assert.AreEqual("Req Types", remoteCustomList.Name);
			Assert.AreEqual(true, remoteCustomList.Active);
			Assert.AreEqual(2, remoteCustomList.Values.Length);
			//Value 1
			Assert.AreEqual("Feature", remoteCustomList.Values[0].Name);
			Assert.AreEqual(true, remoteCustomList.Values[0].Active);
			//Value 2
			Assert.AreEqual("Technical Quality", remoteCustomList.Values[1].Name);
			Assert.AreEqual(true, remoteCustomList.Values[1].Active);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}


		/// <summary>
		/// Verifies that you can import new releases into the system
		/// </summary>
		[
		Test,
		SpiraTestCase(654)
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
			Assert.AreEqual("AAA", remoteRelease.IndentLevel);
			Assert.AreEqual("1.0", remoteRelease.VersionNumber);
			Assert.AreEqual("First version of the system", remoteRelease.Description);
			Assert.AreEqual(false, remoteRelease.Iteration);
			Assert.AreEqual(userId1, remoteRelease.CreatorId);
			Assert.AreEqual(true, remoteRelease.Active);
			Assert.IsTrue(remoteRelease.PlannedEffort > 0);

			//Now retrieve one of the iterations
			remoteRelease = spiraImportExport.Release_RetrieveById(iterationId1);
			Assert.AreEqual("1.0.001", remoteRelease.VersionNumber);
			Assert.AreEqual("AAAAAA", remoteRelease.IndentLevel);
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
		SpiraTestCase(655)
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
            remoteRequirement.List01 = customValueId1; remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
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
			Assert.AreEqual("AAA", remoteRequirement.IndentLevel);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, remoteRequirement.StatusId);
			Assert.AreEqual(userId1, remoteRequirement.AuthorId);
			Assert.IsNull(remoteRequirement.ImportanceId);

			//Now retrieve a first-level indent requirement
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId3);
			Assert.AreEqual("Requirement 2", remoteRequirement.Name);
			Assert.AreEqual("Requirement Description 2", remoteRequirement.Description);
			Assert.AreEqual("AAAAAB", remoteRequirement.IndentLevel);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId);
			Assert.AreEqual(userId1, remoteRequirement.AuthorId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);
            Assert.AreEqual("test value2", remoteRequirement.Text01);
            Assert.AreEqual(customValueId1, remoteRequirement.List01);

			//Now retrieve a second-level indent requirement
			remoteRequirement = spiraImportExport.Requirement_RetrieveById(requirementId4);
			Assert.AreEqual("Requirement 3", remoteRequirement.Name);
			Assert.AreEqual("AAAAABAAA", remoteRequirement.IndentLevel);
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
			Assert.AreEqual("AAAAACAAB", remoteRequirement.IndentLevel);
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
		SpiraTestCase(656)
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
			Assert.AreEqual(true, remoteTestCase.Folder);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, remoteTestCase.ExecutionStatusId);
			Assert.IsNull(remoteTestCase.OwnerId);

			//Now retrieve the second-level folder
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testFolderId2);
			Assert.AreEqual("Test Folder B", remoteTestCase.Name);
            Assert.AreEqual(true, remoteTestCase.Folder);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotApplicable, remoteTestCase.ExecutionStatusId);
            Assert.IsNull(remoteTestCase.OwnerId);

			//Now retrieve a test case
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4", remoteTestCase.Name);
            Assert.AreEqual(false, remoteTestCase.Folder);
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
			spiraImportExport.TestCase_Update(remoteTestCase);

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
		SpiraTestCase(657)
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
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId3);
			testStepId1 = remoteTestStep.TestStepId.Value;
			//Step 2
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 2;
			remoteTestStep.Description = "Test Step 2";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId3);
			testStepId2 = remoteTestStep.TestStepId.Value;
			//Step 3
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 4";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep.SampleData = "test 123";
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId4);
			testStepId3 = remoteTestStep.TestStepId.Value;
			//Step 4
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 3";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId4);
			testStepId4 = remoteTestStep.TestStepId.Value;

			//Now verify that the import was successful

			//Retrieve the first test case with its steps
			RemoteTestCase remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId3);
			Assert.AreEqual("Test Case 3", remoteTestCase.Name);
			RemoteTestStep[] remoteTestSteps = remoteTestCase.TestSteps;
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 1");
			Assert.AreEqual("Test Step 1", remoteTestSteps[0].Description);
			Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 1");
			Assert.AreEqual("Test Step 2", remoteTestSteps[1].Description);
			Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 2");

			//Retrieve the second test case with its steps
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4a", remoteTestCase.Name);
			remoteTestSteps = remoteTestCase.TestSteps;
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 2");
			Assert.AreEqual("Test Step 3", remoteTestSteps[0].Description);
			Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 3");
			Assert.AreEqual("Test Step 4", remoteTestSteps[1].Description);
			Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 4");

			//Now lets add a linked test case as a test step, first with no parameters
			testStepId5 = spiraImportExport.TestCase_AddLink(testCaseId1, 1, testCaseId3, null);

			//Now lets add a linked test case as a test step with two parameters
			RemoteTestStepParameter[] parameterArray = new RemoteTestStepParameter[2];
			parameterArray[0] = new RemoteTestStepParameter();
			parameterArray[0].Name = "param1";
			parameterArray[0].Value = "value1";
			parameterArray[1] = new RemoteTestStepParameter();
			parameterArray[1].Name = "param2";
			parameterArray[1].Value = "value2";
			testStepId6 = spiraImportExport.TestCase_AddLink(testCaseId1, 2, testCaseId2, parameterArray);

			//Retrieve the linked test case steps
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual("Test Case 1", remoteTestCase.Name);
			remoteTestSteps = remoteTestCase.TestSteps;
			Assert.AreEqual(2, remoteTestSteps.Length, "Test Step Count 3");
			Assert.AreEqual("Call", remoteTestSteps[0].Description);
			Assert.AreEqual(testCaseId3, remoteTestSteps[0].LinkedTestCaseId);
			Assert.AreEqual("Call", remoteTestSteps[1].Description);
			Assert.AreEqual(testCaseId2, remoteTestSteps[1].LinkedTestCaseId);

			//Finally make an update to one of the test cases and its test steps
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			remoteTestCase.Name = "Test Case 4b";
			remoteTestCase.Description = "Test Case Description 4b";
			remoteTestSteps = remoteTestCase.TestSteps;
			remoteTestSteps[0].Description = "Test Step 3b";
			remoteTestSteps[1].Description = "Test Step 4b";
			spiraImportExport.TestCase_Update(remoteTestCase);

			//Verify the change
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual("Test Case 4b", remoteTestCase.Name);
			Assert.AreEqual("Test Case Description 4b", remoteTestCase.Description);
			remoteTestSteps = remoteTestCase.TestSteps;
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
		SpiraTestCase(658)
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
			RemoteRequirementTestCaseMapping[] requirementTestCaseMappings = spiraImportExport.Requirement_RetrieveTestCoverage(requirementId2);
			Assert.AreEqual(3, requirementTestCaseMappings.Length);
            Assert.AreEqual(testCaseId1, requirementTestCaseMappings[0].TestCaseId);
            Assert.AreEqual(testCaseId2, requirementTestCaseMappings[1].TestCaseId);
            Assert.AreEqual(testCaseId3, requirementTestCaseMappings[2].TestCaseId);

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
			requirementTestCaseMappings = spiraImportExport.Requirement_RetrieveTestCoverage(requirementId2);
			Assert.AreEqual(0, requirementTestCaseMappings.Length);

			//Test that we can map test cases against releases & iterations

			//First test that nothing is mapped against the release or iterations, except those that were auto-mapped from the requirement
			RemoteReleaseTestCaseMapping[] releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(releaseId1);
			Assert.AreEqual(3, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId1);
			Assert.AreEqual(0, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId2);
			Assert.AreEqual(0, releaseTestCaseMappings.Length);

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
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(releaseId1);
			Assert.AreEqual(3, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId1);
			Assert.AreEqual(2, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId2);
			Assert.AreEqual(2, releaseTestCaseMappings.Length);

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

			//Test the mapping changes
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(releaseId1);
			Assert.AreEqual(2, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId1);
			Assert.AreEqual(1, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId2);
			Assert.AreEqual(1, releaseTestCaseMappings.Length);

			//Now test the add multiple mapping API function
			//Now remove some of the mappings
			//Release 1
			List<RemoteReleaseTestCaseMapping> remoteReleaseTestCaseMappings = new List<RemoteReleaseTestCaseMapping>();
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = releaseId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			remoteReleaseTestCaseMappings.Add(remoteReleaseTestCaseMapping);
			//Iteration 1
			remoteReleaseTestCaseMapping = new RemoteReleaseTestCaseMapping();
			remoteReleaseTestCaseMapping.ReleaseId = iterationId1;
			remoteReleaseTestCaseMapping.TestCaseId = testCaseId1;
			remoteReleaseTestCaseMappings.Add(remoteReleaseTestCaseMapping);
			spiraImportExport.Release_AddTestMapping2(remoteReleaseTestCaseMappings.ToArray());

			//Now test that the mappings have been added
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(releaseId1);
			Assert.AreEqual(3, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId1);
			Assert.AreEqual(2, releaseTestCaseMappings.Length);

			//Now remove these mappings
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

			//Finally test the mapping changes
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(releaseId1);
			Assert.AreEqual(2, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId1);
			Assert.AreEqual(1, releaseTestCaseMappings.Length);
			releaseTestCaseMappings = spiraImportExport.Release_RetrieveTestMapping(iterationId2);
			Assert.AreEqual(1, releaseTestCaseMappings.Length);
		}

		/// <summary>
		/// Verifies that you can import test sets into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(660)
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
			Assert.AreEqual(false, remoteTestSet.Folder);
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
            Assert.AreEqual(false, remoteTestSet.Folder);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.InProgress, remoteTestSet.TestSetStatusId, "TestSetStatusId");
			Assert.AreEqual("Test Set 2 Description", remoteTestSet.Description, "Description");
			Assert.IsNull(remoteTestSet.ReleaseId);
			Assert.AreEqual(userId2, remoteTestSet.OwnerId, "OwnerId");
			Assert.IsNotNull(remoteTestSet.PlannedDate, "PlannedDate");
			Assert.IsNull(remoteTestSet.ExecutionDate, "ExecutionDate");

			//Now test that we can add some test cases to one of the test sets
			//Need to verify that we can add the same test case twice
			//and if necessary specify a different OwnerId
			RemoteTestSetTestCaseMapping remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId1;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping, null, null);
			remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId3;
			remoteTestSetTestCaseMapping.OwnerId = USER_ID_JOE_SMITH;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping, null, null);
			remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId1;
			remoteTestSetTestCaseMapping.OwnerId = USER_ID_FRED_BLOGGS;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping, null, null);

			//Now verify the data
			RemoteTestCase[] remoteTestCases = spiraImportExport.TestCase_RetrieveByTestSetId(testSetId1);
			Assert.AreEqual(3, remoteTestCases.Length);
			Assert.AreEqual("Test Case 1", remoteTestCases[0].Name);
			Assert.AreEqual(null, remoteTestCases[0].OwnerId);
			Assert.AreEqual("Test Case 3", remoteTestCases[1].Name);
			Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestCases[1].OwnerId);
			Assert.AreEqual("Test Case 1", remoteTestCases[2].Name);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestCases[2].OwnerId);

			//Now verify that we can get the mapping object version of the data
			RemoteTestSetTestCaseMapping[] remoteTestSetTestCaseMappings = spiraImportExport.TestSet_RetrieveTestCaseMapping(testSetId1);
			Assert.AreEqual(3, remoteTestSetTestCaseMappings.Length);
			Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[0].TestCaseId);
			Assert.AreEqual(null, remoteTestSetTestCaseMappings[0].OwnerId);
			Assert.AreEqual(testCaseId3, remoteTestSetTestCaseMappings[1].TestCaseId);
			Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestSetTestCaseMappings[1].OwnerId);
			Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[2].TestCaseId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestSetTestCaseMappings[2].OwnerId);

			int existingTestSetTestCaseId = remoteTestSetTestCaseMappings[2].TestSetTestCaseId;

			//Now add a new test case in the middle of the list and specify some parameter values
			List<RemoteTestSetTestCaseParameter> parameters = new List<RemoteTestSetTestCaseParameter>();
			RemoteTestSetTestCaseParameter parameter1 = new RemoteTestSetTestCaseParameter();
			parameter1.Name = "param1";
			parameter1.Value = "test value 1";
			parameters.Add(parameter1);
			RemoteTestSetTestCaseParameter parameter2 = new RemoteTestSetTestCaseParameter();
			parameter2.Name = "param2";
			parameter2.Value = "test value 2";
			parameters.Add(parameter2);

			remoteTestSetTestCaseMapping = new RemoteTestSetTestCaseMapping();
			remoteTestSetTestCaseMapping.TestSetId = testSetId1;
			remoteTestSetTestCaseMapping.TestCaseId = testCaseId2;
			remoteTestSetTestCaseMapping.OwnerId = USER_ID_FRED_BLOGGS;
			spiraImportExport.TestSet_AddTestMapping(remoteTestSetTestCaseMapping, existingTestSetTestCaseId, parameters.ToArray());

			//Now verify the data
			remoteTestCases = spiraImportExport.TestCase_RetrieveByTestSetId(testSetId1);
			Assert.AreEqual(4, remoteTestCases.Length);
			Assert.AreEqual("Test Case 1", remoteTestCases[0].Name);
			Assert.AreEqual(null, remoteTestCases[0].OwnerId);
			Assert.AreEqual("Test Case 3", remoteTestCases[1].Name);
			Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestCases[1].OwnerId);
			Assert.AreEqual("Test Case 2", remoteTestCases[2].Name);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestCases[2].OwnerId);
			Assert.AreEqual("Test Case 1", remoteTestCases[3].Name);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestCases[3].OwnerId);

			//Now verify that we can get the mapping object version of the data
			remoteTestSetTestCaseMappings = spiraImportExport.TestSet_RetrieveTestCaseMapping(testSetId1);
			Assert.AreEqual(4, remoteTestSetTestCaseMappings.Length);
			Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[0].TestCaseId);
			Assert.AreEqual(null, remoteTestSetTestCaseMappings[0].OwnerId);
			Assert.AreEqual(testCaseId3, remoteTestSetTestCaseMappings[1].TestCaseId);
			Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestSetTestCaseMappings[1].OwnerId);
			Assert.AreEqual(testCaseId2, remoteTestSetTestCaseMappings[2].TestCaseId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestSetTestCaseMappings[2].OwnerId);
			Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[3].TestCaseId);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestSetTestCaseMappings[3].OwnerId);
		}

		/// <summary>
		/// Verifies that you can import a test run complete with test run steps
		/// </summary>
		[
		Test,
		SpiraTestCase(659)
		]
		public void _12_ImportTestRun()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now we need to create a test run from existing test cases we've already imported that have test steps
			RemoteManualTestRun[] remoteManualTestRuns = spiraImportExport.TestRun_CreateFromTestCases(new int[] { testCaseId3, testCaseId4 }, iterationId1);

			//Now mark the first step as pass, the second as fail
			remoteManualTestRuns[0].TestRunSteps[0].ActualResult = "passed";
			remoteManualTestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteManualTestRuns[0].TestRunSteps[1].ActualResult = "broke";
			remoteManualTestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;

			//For the second test case, mark the first step as blocked
			remoteManualTestRuns[1].TestRunSteps[0].ActualResult = "blocked";
			remoteManualTestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;

			//Finally actually save the run
			remoteManualTestRuns = spiraImportExport.TestRun_Save(remoteManualTestRuns, DateTime.Now);
			testRunId1 = remoteManualTestRuns[0].TestRunId.Value;
			testRunStepId1 = remoteManualTestRuns[0].TestRunSteps[0].TestRunStepId.Value;
			testRunStepId2 = remoteManualTestRuns[0].TestRunSteps[1].TestRunStepId.Value;
			testRunId2 = remoteManualTestRuns[1].TestRunId.Value;
			testRunStepId3 = remoteManualTestRuns[1].TestRunSteps[0].TestRunStepId.Value;

			//Now verify that it saved correctly

			//First check the test run
			//Generic RemoteTestRun object
			RemoteTestRun remoteTestRun = spiraImportExport.TestRun_RetrieveById(testRunId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.ExecutionStatusId, "TestRun");
			Assert.AreEqual(iterationId1, remoteTestRun.ReleaseId, "ReleaseId");

			//Specic RemoteManualTestRun object
			RemoteManualTestRun remoteManualTestRun = spiraImportExport.TestRun_RetrieveManualById(testRunId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.ExecutionStatusId, "TestRun");
			Assert.AreEqual(iterationId1, remoteManualTestRun.ReleaseId, "ReleaseId");

			//Now check the test run steps
			Assert.AreEqual("passed", remoteManualTestRun.TestRunSteps[0].ActualResult);
			Assert.AreEqual("broke", remoteManualTestRun.TestRunSteps[1].ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteManualTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

			//Now check the test case itself to see if it was correctly updated
			RemoteTestCase remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId3);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNotNull(remoteTestCase.ExecutionDate);

			//The second test case in the list should be listed as blocked
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId4);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNotNull(remoteTestCase.ExecutionDate);

			//Now test that we can create a test run from a test set
			remoteManualTestRuns = spiraImportExport.TestRun_CreateFromTestSet(testSetId1);

			//Now mark the first step as pass, the second as fail
			remoteManualTestRuns[0].TestRunSteps[0].ActualResult = "passed";
			remoteManualTestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteManualTestRuns[0].TestRunSteps[1].ActualResult = "broke";
			remoteManualTestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			remoteManualTestRuns[1].TestRunSteps[0].ActualResult = "caution";
			remoteManualTestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;

			//Finally actually save the run (add on 1 min to make sure execution date is later so that status changes)
			remoteManualTestRuns = spiraImportExport.TestRun_Save(remoteManualTestRuns, DateTime.Now.AddMinutes(1));
			testRunId1 = remoteManualTestRuns[0].TestRunId.Value;
			testRunId2 = remoteManualTestRuns[1].TestRunId.Value;
			testRunStepId1 = remoteManualTestRuns[0].TestRunSteps[0].TestRunStepId.Value;
			testRunStepId2 = remoteManualTestRuns[0].TestRunSteps[1].TestRunStepId.Value;
			testRunStepId3 = remoteManualTestRuns[1].TestRunSteps[0].TestRunStepId.Value;

			//Now verify that it saved correctly
			//Since the test set retrievebyid method doesn't include execution data, no point verifying the test set counts

			//First check the first test case itself to see if it was correctly updated
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNotNull(remoteTestCase.ExecutionDate);
			//The second test case in the test set should be listed as caution
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId3);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, remoteTestCase.ExecutionStatusId, "TestCase");
			Assert.IsNotNull(remoteTestCase.ExecutionDate);

			//Now check the test run
			remoteManualTestRun = spiraImportExport.TestRun_RetrieveManualById(testRunId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.ExecutionStatusId, "TestRun");
			Assert.AreEqual(testSetId1, remoteManualTestRun.TestSetId, "TestSetId");

			//Finally check the test run steps
			Assert.AreEqual("passed", remoteManualTestRun.TestRunSteps[0].ActualResult);
			Assert.AreEqual("broke", remoteManualTestRun.TestRunSteps[1].ActualResult);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteManualTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

			//*** Now test that we can use the automated test runner API ***

			//This tests that we can create a successful automated test run with a release but no test set
			RemoteAutomatedTestRun remoteAutomatedTestRun = new RemoteAutomatedTestRun();
			remoteAutomatedTestRun.TestCaseId = testCaseId1;
			remoteAutomatedTestRun.ReleaseId = iterationId2;
			remoteAutomatedTestRun.StartDate = DateTime.Now;
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteAutomatedTestRun.RunnerName = "TestSuite";
			remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
			testRunId3 = spiraImportExport.TestRun_RecordAutomated1(remoteAutomatedTestRun).TestRunId.Value;

			//Now retrieve the test run to check that it saved correctly - using the web service
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(testRunId3);

			//Verify Counts (no steps for automated runs)
			Assert.AreEqual(testRunId3, remoteAutomatedTestRun.TestRunId);

			//Now verify the data
			Assert.AreEqual(userId1, remoteAutomatedTestRun.TesterId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteAutomatedTestRun.ExecutionStatusId);
			Assert.AreEqual(iterationId2, remoteAutomatedTestRun.ReleaseId, "ReleaseId");
			Assert.IsNull(remoteAutomatedTestRun.TestSetId, "TestSetId");
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), remoteAutomatedTestRun.TestRunTypeId);
			Assert.AreEqual("TestSuite", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual("02_Test_Method", remoteAutomatedTestRun.RunnerTestName);
			Assert.AreEqual(0, remoteAutomatedTestRun.RunnerAssertCount, "RunnerAssertCount");
			Assert.AreEqual("Nothing Reported", remoteAutomatedTestRun.RunnerMessage);
			Assert.AreEqual("Nothing Reported", remoteAutomatedTestRun.RunnerStackTrace);

			//Now verify that the test case itself updated
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestCase.ExecutionStatusId);

			//This tests that we can create a failed automated test run with a test set instead of a release
			remoteAutomatedTestRun = new RemoteAutomatedTestRun();
			remoteAutomatedTestRun.TesterId = userId2;
			remoteAutomatedTestRun.TestCaseId = testCaseId1;
			remoteAutomatedTestRun.TestSetId = testSetId1;
			remoteAutomatedTestRun.StartDate = DateTime.Now;
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			remoteAutomatedTestRun.RunnerName = "TestSuite";
			remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
			remoteAutomatedTestRun.RunnerAssertCount = 5;
			remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 0";
			remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
			testRunId4 = spiraImportExport.TestRun_RecordAutomated1(remoteAutomatedTestRun).TestRunId.Value;

			//Now retrieve the test run to check that it saved correctly - using the web service
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(testRunId4);

			//Verify Counts (no steps for automated runs)
			Assert.AreEqual(testRunId4, remoteAutomatedTestRun.TestRunId);

			//Now verify the data
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteAutomatedTestRun.ExecutionStatusId);
			Assert.AreEqual(userId2, remoteAutomatedTestRun.TesterId);
			Assert.IsNull(remoteAutomatedTestRun.ReleaseId, "ReleaseId");
			Assert.AreEqual(testSetId1, remoteAutomatedTestRun.TestSetId, "TestSetId");
			Assert.AreEqual(TestRun.TestRunTypeEnum.Automated.GetHashCode(), remoteAutomatedTestRun.TestRunTypeId);
			Assert.AreEqual("TestSuite", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual("02_Test_Method", remoteAutomatedTestRun.RunnerTestName);
			Assert.AreEqual(5, remoteAutomatedTestRun.RunnerAssertCount);
			Assert.AreEqual("Expected 1, Found 0", remoteAutomatedTestRun.RunnerMessage);
			Assert.AreEqual("Error Stack Trace........", remoteAutomatedTestRun.RunnerStackTrace);

			//Now verify that the test case itself updated
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId);

			//This tests that we can create a failed automated test run using the sessionless method overload
			BasicHttpBinding httpBinding = (BasicHttpBinding)spiraImportExport.Endpoint.Binding;
			httpBinding.AllowCookies = false;
			testRunId5 = spiraImportExport.TestRun_RecordAutomated2("aant", "aant123456789", projectId1, userId2, testCaseId1, iterationId2, null, null, DateTime.Now, DateTime.Now.AddSeconds(20), (int)TestCase.ExecutionStatusEnum.Failed, "TestSuite", "02_Test_Method", 5, "Expected 1, Found 0", "Error Stack Trace........");

			//Now retrieve the test run to check that it saved correctly - using the web service
			//Need to restore the use of cookies first
			httpBinding = (BasicHttpBinding)spiraImportExport.Endpoint.Binding;
			httpBinding.AllowCookies = true;
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(testRunId5);

			//Verify Info (no steps for automated runs)
			Assert.AreEqual(testRunId5, remoteAutomatedTestRun.TestRunId);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteAutomatedTestRun.ExecutionStatusId);
			Assert.AreEqual("TestSuite", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual("02_Test_Method", remoteAutomatedTestRun.RunnerTestName);

			//Finally we need to test that we can upload multiple runs using the batch API
			List<RemoteAutomatedTestRun> remoteAutomatedTestRuns = new List<RemoteAutomatedTestRun>();
			remoteAutomatedTestRun = new RemoteAutomatedTestRun();
			remoteAutomatedTestRun.TesterId = userId2;
			remoteAutomatedTestRun.TestCaseId = testCaseId1;
			remoteAutomatedTestRun.TestSetId = testSetId1;
			remoteAutomatedTestRun.StartDate = DateTime.Now;
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
			remoteAutomatedTestRun.RunnerName = "TestSuite";
			remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
			remoteAutomatedTestRun.RunnerAssertCount = 5;
			remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 0";
			remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
			remoteAutomatedTestRuns.Add(remoteAutomatedTestRun);
			remoteAutomatedTestRun = new RemoteAutomatedTestRun();
			remoteAutomatedTestRun.TesterId = userId2;
			remoteAutomatedTestRun.TestCaseId = testCaseId2;
			remoteAutomatedTestRun.TestSetId = testSetId1;
			remoteAutomatedTestRun.StartDate = DateTime.Now;
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
			remoteAutomatedTestRun.RunnerName = "TestSuite";
			remoteAutomatedTestRun.RunnerTestName = "03_Test_Method";
			remoteAutomatedTestRun.RunnerAssertCount = 3;
			remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 2";
			remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
			remoteAutomatedTestRuns.Add(remoteAutomatedTestRun);
			RemoteAutomatedTestRun[] remoteAutomatedTestRuns2 = spiraImportExport.TestRun_RecordAutomated3(remoteAutomatedTestRuns.ToArray());
			int batchTestRunId1 = remoteAutomatedTestRuns2[0].TestRunId.Value;
			int batchTestRunId2 = remoteAutomatedTestRuns2[1].TestRunId.Value;

            //Need to now refresh the cached data (after using this API function) - requires project owner permission
            spiraImportExport.Connection_Authenticate("administrator", "Welcome123$");
            spiraImportExport.Connection_ConnectToProject(projectId1);
            spiraImportExport.Project_RefreshProgressExecutionStatusCaches(null, false);
            spiraImportExport.Connection_Authenticate("aant", "aant123456789");
            spiraImportExport.Connection_ConnectToProject(projectId1);

			//Verify the data
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(batchTestRunId1);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, remoteAutomatedTestRun.ExecutionStatusId);
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(batchTestRunId2);
			Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, remoteAutomatedTestRun.ExecutionStatusId);
		}

		/// <summary>
		/// Verifies that you can import incidents into the project
		/// </summary>
		[
		Test,
		SpiraTestCase(662)
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

			//Test that we can retrieve incidents by Test Run Step, Test Step, and Test Case
			//Test Run Step
			RemoteIncident[] remoteIncidents = spiraImportExport.Incident_RetrieveByTestRunStep(testRunStepId2);
			Assert.AreEqual(1, remoteIncidents.Length);
			Assert.AreEqual(incidentId3, remoteIncidents[0].IncidentId);
			//Test Step
			remoteIncidents = spiraImportExport.Incident_RetrieveByTestStep(testStepId2);
			Assert.AreEqual(1, remoteIncidents.Length);
			Assert.AreEqual(incidentId3, remoteIncidents[0].IncidentId);
			//Test Case
			remoteIncidents = spiraImportExport.Incident_RetrieveByTestCase(testCaseId3, false);
			Assert.AreEqual(0, remoteIncidents.Length);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can import attachments to project artifacts
		/// </summary>
		[
		Test,
		SpiraTestCase(661)
		]
		public void _14_ImportAttachments()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets try adding two attachments to a test case
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

			//Attachment 1
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			RemoteDocument remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "test_data.xls";
			remoteDocument.Description = "Sample Test Case Attachment";
			remoteDocument.AuthorId = userId2;
			remoteDocument.ArtifactId = testCaseId1;
			remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			attachmentId1 = spiraImportExport.Document_AddFile(remoteDocument, attachmentData).AttachmentId.Value;

			//Attachment 2
			attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored 2");
			remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "test_data2.xls";
			remoteDocument.Description = "Sample Test Case Attachment 2";
			remoteDocument.AuthorId = userId2;
			remoteDocument.ArtifactId = testCaseId1;
			remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			attachmentId2 = spiraImportExport.Document_AddFile(remoteDocument, attachmentData).AttachmentId.Value;

			//Now lets get the attachment meta-data and verify
			remoteDocument = spiraImportExport.Document_RetrieveById(attachmentId1);
			Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(1, remoteDocument.Size);

			//Store the folder id for later
			projectAttachmentFolderId = remoteDocument.ProjectAttachmentFolderId.Value;

			//Need to test the URL method
			remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
			remoteDocument.Description = "Sample Test Case URL";
			remoteDocument.AuthorId = userId2;
			remoteDocument.ArtifactId = testCaseId2;
			remoteDocument.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			attachmentId3 = spiraImportExport.Document_AddUrl(remoteDocument).AttachmentId.Value;

			//Now lets get the attachment meta-data and verify
			remoteDocument = spiraImportExport.Document_RetrieveById(attachmentId3);
			Assert.AreEqual("http://www.tempuri.org/test123.htm", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case URL", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("URL", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(0, remoteDocument.Size);
		}

		/// <summary>
		/// Tests that we can import tasks into SpiraTeam/Plan
		/// </summary>
		[
		Test,
		SpiraTestCase(663)
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
			remoteTask.RemainingEffort = 75;
			remoteTask.EstimatedEffort = 100;
			remoteTask.ActualEffort = 30;
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
			Assert.AreEqual(30, remoteTask.ActualEffort);
			Assert.AreEqual(105, remoteTask.ProjectedEffort);
			Assert.AreEqual(75, remoteTask.RemainingEffort);

			//Now lets add a task that has some values null
			remoteTask = new RemoteTask();
			remoteTask.Name = "New Task 2";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
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
			Assert.IsNull(remoteTask.RemainingEffort);
			Assert.IsNull(remoteTask.ProjectedEffort);

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
			Assert.IsNull(remoteTask.RemainingEffort);
			Assert.IsNull(remoteTask.ProjectedEffort);
		}

		/// <summary>
		/// Verifies that you can retrieve the incidents from a specific date
		/// </summary>
		[
		Test,
		SpiraTestCase(643)
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
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { 1, 2 };
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "IncidentStatusId";
			remoteFilter.MultiValue = new MultiValueFilter();
            remoteFilter.MultiValue.Values = new int[] { IncidentManager.IncidentStatusId_AllOpen };
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
		SpiraTestCase(646)
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
			Assert.AreEqual("Functionality Area", remoteTask.RequirementName);
			Assert.IsNotNull(remoteTask.StartDate);
			Assert.IsNotNull(remoteTask.EndDate);
			Assert.AreEqual(25, remoteTask.CompletionPercent);
			Assert.AreEqual(100, remoteTask.EstimatedEffort);
			Assert.AreEqual(30, remoteTask.ActualEffort);
			Assert.AreEqual(75, remoteTask.RemainingEffort);
			Assert.AreEqual(105, remoteTask.ProjectedEffort);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();

			//Now lets test that we can retrieve a generic filtered list of P1,P2 completed Release 1 tasks from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "TaskPriorityId";
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { 1, 2 };
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
		SpiraTestCase(641)
		]
		public void _18_RetrieveRequirements()
		{
			//Now lets test that we can retrieve a generic list of requirements from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ImportanceId";
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { 1, 2 };
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
		SpiraTestCase(642)
		]
		public void _19_RetrieveTestCases()
		{
			//Now lets test that we can retrieve a generic list of test cases and test runs from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "OwnerId";
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { 2, 3 };
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

			//Verify the data returned
            Assert.AreEqual(10, remoteTestCases.Length);
            Assert.AreEqual("Common Tests", remoteTestCases[0].Name);
            Assert.AreEqual("AAA", remoteTestCases[0].IndentLevel);
            Assert.AreEqual("Functional Tests", remoteTestCases[1].Name);
            Assert.AreEqual("AAB", remoteTestCases[1].IndentLevel);
            Assert.AreEqual("Ability to reassign book to different author", remoteTestCases[6].Name);
            Assert.AreEqual("AABAAE", remoteTestCases[6].IndentLevel);

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

			//Now lets test that we can get the list of test case in a test folder
			remoteTestCases = spiraImportExport.TestCase_RetrieveByFolder(1);

			//Verify the data returned
            Assert.AreEqual(5, remoteTestCases.Length);
            Assert.AreEqual("Ability to create new author", remoteTestCases[0].Name);
            Assert.AreEqual("AABAAA", remoteTestCases[0].IndentLevel);
            Assert.AreEqual("Ability to reassign book to different author", remoteTestCases[4].Name);
            Assert.AreEqual("AABAAE", remoteTestCases[4].IndentLevel);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of releases
		/// </summary>
		[
		Test,
		SpiraTestCase(644)
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
            Assert.AreEqual("Library System Release 1.2", remoteReleases[2].Name);
            Assert.AreEqual("Library System Release 1.1 SP1", remoteReleases[0].Name);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Verifies that you can retrieve filtered lists of test sets
		/// </summary>
		[
		Test,
		SpiraTestCase(645)
		]
		public void _21_RetrieveTestSets()
		{
			//Now lets test that we can retrieve a generic list of test sets from the sample project
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			spiraImportExport.Connection_ConnectToProject(1);

			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "OwnerId";
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { 2, 3 };
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "TestSetStatusId";
			remoteFilter.MultiValue = new MultiValueFilter();
			remoteFilter.MultiValue.Values = new int[] { (int)Task.TaskStatusEnum.NotStarted, (int)Task.TaskStatusEnum.InProgress };
			remoteFilters.Add(remoteFilter);
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "PlannedDate";
			remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-126);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow.AddDays(-8);
            remoteFilters.Add(remoteFilter);
			RemoteTestSet[] remoteTestSets = spiraImportExport.TestSet_Retrieve(remoteFilters.ToArray(), 1, 999999);

			//Verify the data returned
            Assert.AreEqual(4, remoteTestSets.Length);
            Assert.AreEqual("Functional Test Sets", remoteTestSets[0].Name);
            Assert.AreEqual("AAA", remoteTestSets[0].IndentLevel);
            Assert.AreEqual("Testing Cycle for Release 1.0", remoteTestSets[1].Name);
            Assert.AreEqual("AAAAAA", remoteTestSets[1].IndentLevel);
            Assert.AreEqual("Testing Cycle for Release 1.1", remoteTestSets[2].Name);
            Assert.AreEqual("AAAAAB", remoteTestSets[2].IndentLevel);
            //Assert.AreEqual("Testing New Functionality", remoteTestSets[3].Name);
            //Assert.AreEqual("AAAAAC", remoteTestSets[3].IndentLevel);
            Assert.AreEqual("Regression Test Sets", remoteTestSets[3].Name);
            Assert.AreEqual("AAB", remoteTestSets[3].IndentLevel);

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
		SpiraTestCase(647)
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
		SpiraTestCase(648)
		]
		public void _23_RetrieveSettings()
		{
			// Log in, though this may not even be necessary.
            spiraImportExport.Connection_Authenticate("fredbloggs", "PleaseChange");
			// Pull the version object.
			RemoteSetting[] remoteSettings = spiraImportExport.System_GetSettings();
			// Verify they meet requirements.
			Assert.IsTrue(remoteSettings.Length > 0, "There were no settings returned!");

			//Verify that we can retrieve artifact urls for generating inbound links to SpiraTest
			//The base url to Requirement 5 in project 2, set to the Comments tab
			string url = spiraImportExport.System_GetArtifactUrl(1, 2, 5, "Comments");
			Assert.AreEqual("~/2/Requirement/5/Comments.aspx", url);

			//The base url to an unspecified Test Case in project 3, set to the Test Steps tab
			url = spiraImportExport.System_GetArtifactUrl(2, 3, -2, "TestSteps");
			Assert.AreEqual("~/3/TestCase/{art}/TestSteps.aspx", url);

			//Finally disconnect from the project
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Tests that we can retrieve a list of project roles
		/// </summary>
		[
		Test,
		SpiraTestCase(649)
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
		SpiraTestCase(651)
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
            int rq_importanceHighId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId2).FirstOrDefault(i => i.Score == 2).ImportanceId;
            int rq_importanceLowId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId2).FirstOrDefault(i => i.Score == 4).ImportanceId;
            int tc_priorityLowId = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 4).TestCasePriorityId;
            int tc_priorityHighId = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 2).TestCasePriorityId;

            /************************* TASKS **********************/

            //Now lets add a task that has all values set
            RemoteTask remoteTask = new RemoteTask();
			remoteTask.Name = "New Task 1";
			remoteTask.Description = "Task 1 Description";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
			remoteTask.TaskPriorityId = tk_priorityMediumId;
			remoteTask.StartDate = DateTime.Now;
			remoteTask.EndDate = DateTime.Now.AddDays(5);
			remoteTask.EstimatedEffort = 100;
			remoteTask.ActualEffort = 20;
			remoteTask.RemainingEffort = 100;
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
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
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
			task.MarkAsDeleted(projectId1, taskId, USER_ID_FRED_BLOGGS);

			/************************* INCIDENTS **********************/

			//Now lets add a incident that has all values set
			RemoteIncident remoteIncident = new RemoteIncident();
			remoteIncident.Name = "New Incident 1";
			remoteIncident.IncidentStatusId = null;   //Signifies the default
			remoteIncident.IncidentTypeId = null;     //Signifies the default
			remoteIncident.Description = "Incident 1 Description";
			remoteIncident.StartDate = DateTime.Now;
			remoteIncident.EstimatedEffort = 100;
			remoteIncident.ActualEffort = 20;
			remoteIncident.ProjectedEffort = 100;
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
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
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

			/************************* REQUIREMENTS **********************/
			//First we need to create a new requirement to verify the handling
			RemoteRequirement remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = (int)Requirement.RequirementStatusEnum.Requested;
			remoteRequirement.Name = "Functionality Area";
			remoteRequirement.Description = String.Empty;
			remoteRequirement = spiraImportExport.Requirement_Create1(remoteRequirement, 0);
			int requirementId = remoteRequirement.RequirementId.Value;

			//Now retrieve the requirement back into two copies
			RemoteRequirement remoteRequirement1 = spiraImportExport.Requirement_RetrieveById(requirementId);
			RemoteRequirement remoteRequirement2 = spiraImportExport.Requirement_RetrieveById(requirementId);

			//Now make a change to field and update
			remoteRequirement1.ImportanceId = rq_importanceHighId;
			spiraImportExport.Requirement_Update(remoteRequirement1);

			//Verify it updated correctly using separate dataset
			RemoteRequirement remoteRequirement3 = spiraImportExport.Requirement_RetrieveById(requirementId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement3.ImportanceId);

			//Now try making a change using the out of date data object (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteRequirement2.ImportanceId = rq_importanceLowId;
				spiraImportExport.Requirement_Update(remoteRequirement2);
			}
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
                if (detail.Type == "OptimisticConcurrencyException")
                {
                    exceptionThrown = true;
                }
                else
                {
                    Assert.Fail(detail.Type);
                }
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate data object
			remoteRequirement3 = spiraImportExport.Requirement_RetrieveById(requirementId);
			Assert.AreEqual(rq_importanceHighId, remoteRequirement3.ImportanceId);

			//Now refresh the old data object and try again and verify it works
			remoteRequirement2 = spiraImportExport.Requirement_RetrieveById(requirementId);
			remoteRequirement2.ImportanceId = rq_importanceLowId;
			spiraImportExport.Requirement_Update(remoteRequirement2);

			//Verify it updated correctly using separate data object
			remoteRequirement3 = spiraImportExport.Requirement_RetrieveById(requirementId);
			Assert.AreEqual(rq_importanceLowId, remoteRequirement3.ImportanceId);

			//Clean up (can't use the API as it doesn't have deletes)
			new RequirementManager().MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId1, requirementId);

			/************************* RELEASES **********************/
			//First we need to create a new release to verify the handling
			RemoteRelease remoteRelease = new RemoteRelease();
			remoteRelease.Name = "Release 1";
			remoteRelease.VersionNumber = "1.0";
			remoteRelease.Description = "First version of the system";
			remoteRelease.Active = true;
			remoteRelease.Iteration = false;
			remoteRelease.StartDate = DateTime.Now;
			remoteRelease.EndDate = DateTime.Now.AddMonths(3);
			remoteRelease.ResourceCount = 5;
			int releaseId = spiraImportExport.Release_Create(remoteRelease, null).ReleaseId.Value;

			//Now retrieve the release back and keep a copy of the dataset
			RemoteRelease remoteRelease1 = spiraImportExport.Release_RetrieveById(releaseId);
			RemoteRelease remoteRelease2 = spiraImportExport.Release_RetrieveById(releaseId);

			//Now make a change to field and update
			remoteRelease1.DaysNonWorking = 3;
			spiraImportExport.Release_Update(remoteRelease1);

			//Verify it updated correctly using separate dataset
			RemoteRelease remoteRelease3 = spiraImportExport.Release_RetrieveById(releaseId);
			Assert.AreEqual(3, remoteRelease3.DaysNonWorking);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteRelease2.DaysNonWorking = 4;
				spiraImportExport.Release_Update(remoteRelease2);
			}
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteRelease3 = spiraImportExport.Release_RetrieveById(releaseId);
			Assert.AreEqual(3, remoteRelease3.DaysNonWorking);

			//Now refresh the old dataset and try again and verify it works
			remoteRelease2 = spiraImportExport.Release_RetrieveById(releaseId);
			remoteRelease2.DaysNonWorking = 4;
			spiraImportExport.Release_Update(remoteRelease2);

			//Verify it updated correctly using separate dataset
			remoteRelease3 = spiraImportExport.Release_RetrieveById(releaseId);
			Assert.AreEqual(4, remoteRelease3.DaysNonWorking);

			//Clean up (can't use the API as it doesn't have deletes)
			new ReleaseManager().MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId1, releaseId);

			/************************* TEST SETS **********************/

			//First we need to create a new test set to verify the handling
			RemoteTestSet remoteTestSet = new RemoteTestSet();
			remoteTestSet.Name = "Test Set 1";
			remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
			int testSetId = spiraImportExport.TestSet_Create(remoteTestSet, null).TestSetId.Value;

			//Now retrieve the testSet back and keep a copy of the dataset
			RemoteTestSet remoteTestSet1 = spiraImportExport.TestSet_RetrieveById(testSetId);
			RemoteTestSet remoteTestSet2 = spiraImportExport.TestSet_RetrieveById(testSetId);

			//Now make a change to field and update
			remoteTestSet1.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Blocked;
			spiraImportExport.TestSet_Update(remoteTestSet1);

			//Verify it updated correctly using separate dataset
			RemoteTestSet remoteTestSet3 = spiraImportExport.TestSet_RetrieveById(testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, remoteTestSet3.TestSetStatusId);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteTestSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
				spiraImportExport.TestSet_Update(remoteTestSet2);
			}
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteTestSet3 = spiraImportExport.TestSet_RetrieveById(testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, remoteTestSet3.TestSetStatusId);

			//Now refresh the old dataset and try again and verify it works
			remoteTestSet2 = spiraImportExport.TestSet_RetrieveById(testSetId);
			remoteTestSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
			spiraImportExport.TestSet_Update(remoteTestSet2);

			//Verify it updated correctly using separate dataset
			remoteTestSet3 = spiraImportExport.TestSet_RetrieveById(testSetId);
			Assert.AreEqual((int)TestSet.TestSetStatusEnum.Deferred, remoteTestSet3.TestSetStatusId);

			//Clean up (can't use the API as it doesn't have deletes)
			new TestSetManager().MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId1, testSetId);

			/************************* TEST CASES/STEPS **********************/
			//First we need to create a new test case to verify the handling
			RemoteTestCase remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 1";
			remoteTestCase.Description = "Test Case Description 1";
			remoteTestCase.Active = true;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, null);
			int testCaseId = remoteTestCase.TestCaseId.Value;

			//Now retrieve the testCase back and keep a copy of the dataset
			RemoteTestCase remoteTestCase1 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			RemoteTestCase remoteTestCase2 = spiraImportExport.TestCase_RetrieveById(testCaseId);

			//Now make a change to field and update
			remoteTestCase1.TestCasePriorityId = tc_priorityHighId;
			spiraImportExport.TestCase_Update(remoteTestCase1);

			//Verify it updated correctly using separate dataset
			RemoteTestCase remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(tc_priorityHighId, remoteTestCase3.TestCasePriorityId);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteTestCase2.TestCasePriorityId = tc_priorityLowId;
				spiraImportExport.TestCase_Update(remoteTestCase2);
			}
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(tc_priorityHighId, remoteTestCase3.TestCasePriorityId);

			//Now refresh the old dataset and try again and verify it works
			remoteTestCase2 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			remoteTestCase2.TestCasePriorityId = tc_priorityLowId;
			spiraImportExport.TestCase_Update(remoteTestCase2);

			//Verify it updated correctly using separate dataset
			remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(tc_priorityLowId, remoteTestCase3.TestCasePriorityId);

			//Next we need to create a new test step to verify the handling
			RemoteTestStep remoteTestStep = new RemoteTestStep();
			remoteTestStep.Description = "Desc 1";
			remoteTestStep.ExpectedResult = "Expected 1";
			remoteTestStep.Position = 1;
			spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId);

			//Now retrieve the test step back and keep a copy of the dataset
			remoteTestCase1 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			remoteTestCase2 = spiraImportExport.TestCase_RetrieveById(testCaseId);

			//Now make a change to field and update
			remoteTestCase1.TestSteps[0].Description = "Desc 2";
			spiraImportExport.TestCase_Update(remoteTestCase1);

			//Verify it updated correctly using separate dataset
			remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual("Desc 2", remoteTestCase3.TestSteps[0].Description);

			//Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
			exceptionThrown = false;
			try
			{
				remoteTestCase2.TestSteps[0].Description = "Desc 3";
				spiraImportExport.TestCase_Update(remoteTestCase2);
			}
			catch (FaultException<ServiceFaultMessage> soapException)
			{
				ServiceFaultMessage detail = soapException.Detail;
				if (detail.Type == "OptimisticConcurrencyException")
				{
					exceptionThrown = true;
				}
			}
			Assert.IsTrue(exceptionThrown, "exceptionThrown");

			//Verify it didn't update using separate dataset
			remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual("Desc 2", remoteTestCase3.TestSteps[0].Description);

			//Now refresh the old dataset and try again and verify it works
			remoteTestCase2 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			remoteTestCase2.TestSteps[0].Description = "Desc 3";
			spiraImportExport.TestCase_Update(remoteTestCase2);

			//Verify it updated correctly using separate dataset
			remoteTestCase3 = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual("Desc 3", remoteTestCase3.TestSteps[0].Description);

			//Clean up (can't use the API as it doesn't have deletes)
			new TestCaseManager().MarkAsDeleted(USER_ID_FRED_BLOGGS, projectId1, testCaseId);

			//Finally disconnect
			spiraImportExport.Connection_Disconnect();
		}

		/// <summary>
		/// Pulls a smaple workflow transition.
		/// </summary>
		[
			Test,
			SpiraTestCase(664)
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
			{
				isOwner = (incident.OpenerName.ToLowerInvariant() == "administrator");
			}
			if (incident.OwnerName != null)
			{
				isDetector = (incident.OwnerName.ToLowerInvariant() == "administrator");
			}

			//Get transitions.
			RemoteWorkflowIncidentTransition[] transitions = spiraImportExport.Incident_RetrieveWorkflowTransitions(incident.IncidentTypeId.Value, incident.IncidentStatusId.Value, isDetector, isOwner);

			//Errorchecking.
			Assert.AreEqual(transitions.Length, 2);
			bool isOneOpen = false;
			bool isOneAssigned = false;
			for (int i = 0; i < transitions.Length; i++)
			{
				if (transitions[i].IncidentStatusName_Output == "Open")
				{
					isOneOpen = true;
				}
				if (transitions[i].IncidentStatusName_Output == "Assigned")
				{
					isOneAssigned = true;
				}
			}
			Assert.AreEqual(isOneAssigned, true);
			Assert.AreEqual(isOneOpen, true);
		}

		/// <summary>
		/// Pulls allowed fields.
		/// </summary>
		[
			Test,
			SpiraTestCase(665)
		]
		public void _27_WorkflowTransitionFields_Pulling()
		{
			const int INCIDENT_ID = 3;

			//Connect to the pre-existing project 2
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(PROJECT_ID);

			//Get the incident.
			RemoteIncident incident = spiraImportExport.Incident_RetrieveById(INCIDENT_ID);

			//Verify that we can retrieve the fields
			RemoteWorkflowIncidentFields[] fields = spiraImportExport.Incident_RetrieveWorkflowFields(incident.IncidentTypeId.Value, incident.IncidentStatusId.Value);

			Assert.AreEqual(fields.Length, 16);
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Type" && f.FieldStateId == 2));
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Started On" && f.FieldStateId == 1));

			//Now verify that we can retrieve the custom fields
			RemoteWorkflowIncidentCustomProperties[] customProperties = spiraImportExport.Incident_RetrieveWorkflowCustomProperties(incident.IncidentTypeId.Value, incident.IncidentStatusId.Value);
			Assert.AreEqual(11, customProperties.Length);
            Assert.IsTrue(customProperties.Any(cp => cp.FieldName == "LIST_02" && cp.FieldStateId == 1));
            Assert.IsTrue(customProperties.Any(cp => cp.FieldName == "LIST_02" && cp.FieldStateId == 2));
        }

		/// <summary>
		/// Verifies that we can retrieve and delete documents through the API
		/// </summary>
		[
		Test,
		SpiraTestCase(666)
		]
		public void _28_RetrieveDeleteAttachments()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Next lets try retrieving the list of documents for a specific project folder, no filter, sorting by upload date ascending
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "UploadDate";
			remoteSort.SortAscending = true;
			RemoteDocument[] remoteDocuments = spiraImportExport.Document_RetrieveForFolder(projectAttachmentFolderId, null, remoteSort, 1, 999);

			//Verify the data retrieved
			Assert.AreEqual(3, remoteDocuments.Length);
			RemoteDocument remoteDocument = remoteDocuments[0];
			Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(1, remoteDocument.Size);

			//Now lets try filtering by a field and sorting by filename
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "Filename";
			remoteFilter.StringValue = "test_data";
			remoteSort.PropertyName = "Filename";
			remoteSort.SortAscending = true;
			remoteDocuments = spiraImportExport.Document_RetrieveForFolder(projectAttachmentFolderId, new RemoteFilter[] { remoteFilter }, remoteSort, 1, 999);

			//Verify the data retrieved
			Assert.AreEqual(2, remoteDocuments.Length);
			remoteDocument = remoteDocuments[0];
			Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(1, remoteDocument.Size);

			//Next lets try retrieving the list of documents for a specific artifact, no filter, sorting by upload date descending
			remoteSort = new RemoteSort();
			remoteSort.PropertyName = "UploadDate";
			remoteSort.SortAscending = false;
			remoteDocuments = spiraImportExport.Document_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, null, remoteSort);

			//Verify the data retrieved
			Assert.AreEqual(2, remoteDocuments.Length);
			remoteDocument = remoteDocuments[0];
			Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment 2", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(1, remoteDocument.Size);

			//Now lets try filtering by a field and sorting by filename
			remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "Filename";
			remoteFilter.StringValue = "test_data2";
			remoteSort.PropertyName = "AttachmentId";
			remoteSort.SortAscending = true;
			remoteDocuments = spiraImportExport.Document_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, new RemoteFilter[] { remoteFilter }, remoteSort);

			//Verify the data retrieved
			Assert.AreEqual(1, remoteDocuments.Length);
			remoteDocument = remoteDocuments[0];
			Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment 2", remoteDocument.Description);
			Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("Default", remoteDocument.ProjectAttachmentTypeName);
			Assert.AreEqual(1, remoteDocument.Size);

			//Need to test that we can get the contents of a file attachment
			byte[] attachmentData = spiraImportExport.Document_OpenFile(attachmentId1);
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			string attachmentStringData = unicodeEncoding.GetString(attachmentData);
			Assert.AreEqual("Test Attachment Data To Be Stored", attachmentStringData);

			//Test that we can remove an attachment from an artifact
			spiraImportExport.Document_DeleteFromArtifact(attachmentId2, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1);

			//Verify the data changed
			remoteDocuments = spiraImportExport.Document_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, null, remoteSort);
			Assert.AreEqual(1, remoteDocuments.Length);
		}


		/// <summary>
		/// Verifies that we can create, retrieve and update artifact associations through the API
		/// </summary>
		[
		Test,
		SpiraTestCase(667)
		]
		public void _29_CreateRetrieveUpdateAssociations()
		{
			//First lets add an association between a requirement and an incident
			RemoteAssociation remoteAssociation = new RemoteAssociation();
			remoteAssociation.SourceArtifactId = requirementId1;
			remoteAssociation.SourceArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			remoteAssociation.DestArtifactId = incidentId1;
			remoteAssociation.DestArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteAssociation.Comment = "They are related";
			spiraImportExport.Association_Create(remoteAssociation);

			//Next add an association between two incidents
			remoteAssociation = new RemoteAssociation();
			remoteAssociation.SourceArtifactId = incidentId1;
			remoteAssociation.SourceArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteAssociation.DestArtifactId = incidentId2;
			remoteAssociation.DestArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteAssociation.Comment = "They are the same bugs man";
			spiraImportExport.Association_Create(remoteAssociation);

			//Verify that the associations were created
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "Comment";
			remoteSort.SortAscending = true;
			RemoteAssociation[] remoteAssociations = spiraImportExport.Association_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, null, remoteSort);
			Assert.AreEqual(2, remoteAssociations.Length);
			//Record 1
			Assert.AreEqual(requirementId1, remoteAssociations[0].DestArtifactId);
			Assert.AreEqual("Requirement", remoteAssociations[0].DestArtifactTypeName);
			Assert.AreEqual("They are related", remoteAssociations[0].Comment);
			//Record 2
			Assert.AreEqual(incidentId2, remoteAssociations[1].DestArtifactId);
			Assert.AreEqual("Incident", remoteAssociations[1].DestArtifactTypeName);
			Assert.AreEqual("They are the same bugs man", remoteAssociations[1].Comment);

			//Now test that we can get a filtered list
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "ArtifactTypeId";
			remoteFilter.IntValue = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			remoteAssociations = spiraImportExport.Association_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, new RemoteFilter[] { remoteFilter }, remoteSort);
			Assert.AreEqual(1, remoteAssociations.Length);
			Assert.AreEqual(incidentId2, remoteAssociations[0].DestArtifactId);
			Assert.AreEqual("Incident", remoteAssociations[0].DestArtifactTypeName);
			Assert.AreEqual("They are the same bugs man", remoteAssociations[0].Comment);

			//Finally test that we can update the comment
			remoteAssociation = remoteAssociations[0];
			remoteAssociation.Comment = "They are the same bugs lady";
			spiraImportExport.Association_Update(remoteAssociation);
			remoteAssociations = spiraImportExport.Association_RetrieveForArtifact((int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, new RemoteFilter[] { remoteFilter }, remoteSort);
			Assert.AreEqual(1, remoteAssociations.Length);
			Assert.AreEqual(incidentId2, remoteAssociations[0].DestArtifactId);
			Assert.AreEqual("Incident", remoteAssociations[0].DestArtifactTypeName);
			Assert.AreEqual("They are the same bugs lady", remoteAssociations[0].Comment);
		}

		/// <summary>
		/// Verifies that you can use the API to get a list of automated test runs to be used by a remote host
		/// and then report back the results of the testing to Spira
		/// </summary>
		/// <remarks>
		/// Separated from the ImportTestRun test method to make testing easier.
		/// </remarks>
		[
		Test,
		SpiraTestCase(715)
		]
		public void _30_AutomatedTestLauncher()
		{
			//First we need to attached a test script to some test cases in the project
			spiraImportExport.Connection_Authenticate("administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);
			byte[] binaryData = UnicodeEncoding.UTF8.GetBytes("This is a QTP test script");

			spiraImportExport.TestCase_AddUpdateAutomationScript(testCaseId1, AUTOMATION_ENGINE_ID_QTP, "file:///c:/mytests/test1.qtp", "", null, "1.0", null, null);
			spiraImportExport.TestCase_AddUpdateAutomationScript(testCaseId2, AUTOMATION_ENGINE_ID_QTP, "test2.qtp", "", binaryData, "1.0", null, null);
			spiraImportExport.TestCase_AddUpdateAutomationScript(testCaseId3, AUTOMATION_ENGINE_ID_QTP, "file:///c:/mytests/test3.qtp", "", null, "1.0", null, null);

			//Next we need to create an automation host in the project
			RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
			remoteAutomationHost.Name = "Window XP Host";
			remoteAutomationHost.Token = "Win8";
			remoteAutomationHost.Description = "Test Win8 host";
			remoteAutomationHost.Active = true;
			int automationHostId = spiraImportExport.AutomationHost_Create(remoteAutomationHost).AutomationHostId.Value;

			//Assign one of our test sets to this host and set the planned date
			RemoteTestSet remoteTestSet = spiraImportExport.TestSet_RetrieveById(testSetId1);
			remoteTestSet.AutomationHostId = automationHostId;
			remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
			remoteTestSet.PlannedDate = DateTime.Now;
			spiraImportExport.TestSet_Update(remoteTestSet);

			//Now use the API method to actually get the list of test runs to be executed along with associated parameters
			DateRange dateRange = new DateRange();
			dateRange.StartDate = DateTime.Now.AddHours(-1);
			dateRange.EndDate = DateTime.Now.AddHours(1);
			RemoteAutomatedTestRun[] remoteAutomatedTestRuns = spiraImportExport.TestRun_CreateForAutomationHost("Win8", dateRange);

			//Verify the data
            Assert.IsNotNull(remoteAutomatedTestRuns);
			Assert.AreEqual(4, remoteAutomatedTestRuns.Length);
			Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[0].Name);
			Assert.AreEqual("Test Case 3", remoteAutomatedTestRuns[1].Name);
			Assert.AreEqual("Test Case 2", remoteAutomatedTestRuns[2].Name);
			Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[3].Name);

			//Verify one test case in detail
			RemoteAutomatedTestRun remoteAutomatedTestRun = remoteAutomatedTestRuns[2];
			Assert.AreEqual(testCaseId2, remoteAutomatedTestRun.TestCaseId);
			Assert.AreEqual(testSetId1, remoteAutomatedTestRun.TestSetId);
			Assert.AreEqual(AUTOMATION_ENGINE_ID_QTP, remoteAutomatedTestRun.AutomationEngineId);
			Assert.AreEqual(automationHostId, remoteAutomatedTestRun.AutomationHostId);
			Assert.AreEqual("Quick Test Pro", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, remoteAutomatedTestRun.TestRunTypeId);
			int automationAttachmentId = remoteAutomatedTestRun.AutomationAttachmentId.Value;

			//Test that we can retrieve the test script
			RemoteDocument remoteTestScript = spiraImportExport.Document_RetrieveById(automationAttachmentId);
			Assert.AreEqual("test2.qtp", remoteTestScript.FilenameOrUrl);
			binaryData = spiraImportExport.Document_OpenFile(automationAttachmentId);
			string testScript = System.Text.UnicodeEncoding.UTF8.GetString(binaryData);
			Assert.AreEqual("This is a QTP test script", testScript);

			//Verify that we can view the test case parameters
			Assert.AreEqual(2, remoteAutomatedTestRun.Parameters.Length);
			Assert.AreEqual("param1", remoteAutomatedTestRun.Parameters[0].Name);
			Assert.AreEqual("test value 1", remoteAutomatedTestRun.Parameters[0].Value);
			Assert.AreEqual("param2", remoteAutomatedTestRun.Parameters[1].Name);
			Assert.AreEqual("test value 2", remoteAutomatedTestRun.Parameters[1].Value);

			//Now record some results using these test runs
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
			remoteAutomatedTestRun.RunnerAssertCount = 1;
			remoteAutomatedTestRun.RunnerMessage = "Failed with error";
			remoteAutomatedTestRun.RunnerTestName = "MyTest1";
			remoteAutomatedTestRun.RunnerStackTrace = "Failed with error during database operation X";
			remoteAutomatedTestRun.StartDate = DateTime.Now.AddMinutes(-1);
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(1);
			remoteAutomatedTestRun = spiraImportExport.TestRun_RecordAutomated1(remoteAutomatedTestRun);

			//Verify that the results can be retrieved
			remoteAutomatedTestRun = spiraImportExport.TestRun_RetrieveAutomatedById(remoteAutomatedTestRun.TestRunId.Value);
			Assert.AreEqual(testCaseId2, remoteAutomatedTestRun.TestCaseId);
			Assert.AreEqual(testSetId1, remoteAutomatedTestRun.TestSetId);
			Assert.AreEqual(AUTOMATION_ENGINE_ID_QTP, remoteAutomatedTestRun.AutomationEngineId);
			Assert.AreEqual(automationHostId, remoteAutomatedTestRun.AutomationHostId);
			Assert.AreEqual("Quick Test Pro", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, remoteAutomatedTestRun.TestRunTypeId);
			Assert.AreEqual((int)(int)TestCase.ExecutionStatusEnum.Failed, remoteAutomatedTestRun.ExecutionStatusId);
			Assert.AreEqual(1, remoteAutomatedTestRun.RunnerAssertCount);
			Assert.AreEqual("MyTest1", remoteAutomatedTestRun.RunnerTestName);
			Assert.AreEqual("Failed with error", remoteAutomatedTestRun.RunnerMessage);
			Assert.AreEqual("Failed with error during database operation X", remoteAutomatedTestRun.RunnerStackTrace);

			//Now use the API method to get a test runs when we specify the host and test set id explicitly
			remoteAutomatedTestRuns = spiraImportExport.TestRun_CreateForAutomatedTestSet(testSetId1, "Win8");

			//Verify the data
			Assert.AreEqual(4, remoteAutomatedTestRuns.Length);
			Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[0].Name);
			Assert.AreEqual("Test Case 3", remoteAutomatedTestRuns[1].Name);
			Assert.AreEqual("Test Case 2", remoteAutomatedTestRuns[2].Name);
			Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[3].Name);

			//Verify one test case in detail
			remoteAutomatedTestRun = remoteAutomatedTestRuns[2];
			Assert.AreEqual(testCaseId2, remoteAutomatedTestRun.TestCaseId);
			Assert.AreEqual(testSetId1, remoteAutomatedTestRun.TestSetId);
			Assert.AreEqual(AUTOMATION_ENGINE_ID_QTP, remoteAutomatedTestRun.AutomationEngineId);
			Assert.AreEqual(automationHostId, remoteAutomatedTestRun.AutomationHostId);
			Assert.AreEqual("Quick Test Pro", remoteAutomatedTestRun.RunnerName);
			Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, remoteAutomatedTestRun.TestRunTypeId);

			//Delete the host
			spiraImportExport.AutomationHost_Delete(automationHostId);

			//Verify it has been deleted
			Assert.IsNull(spiraImportExport.AutomationHost_RetrieveById(automationHostId));
		}

		/// <summary>
		/// Tests that you can add and retrieve comments through the API
		/// </summary>
		[
		Test,
		SpiraTestCase(720)
		]
		public void _31_ImportRetrieveArtifactComments()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("aant", "aant123456789");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now lets add some comments to various artifacts
			RemoteComment remoteComment = new RemoteComment();

			//Requirement
			remoteComment.ArtifactId = requirementId1;
			remoteComment.Text = "This is a comment";
			spiraImportExport.Requirement_CreateComment(remoteComment);
			//Verify the data
			RemoteComment[] remoteComments = spiraImportExport.Requirement_RetrieveComments(requirementId1);
			Assert.AreEqual(1, remoteComments.Length);
			Assert.AreEqual("This is a comment", remoteComments[0].Text);
			Assert.IsNotNull(remoteComments[0].CreationDate);

			//Release
			remoteComment.ArtifactId = releaseId1;
			remoteComment.Text = "This is a comment";
			spiraImportExport.Release_CreateComment(remoteComment);
			//Verify the data
			remoteComments = spiraImportExport.Release_RetrieveComments(releaseId1);
			Assert.AreEqual(1, remoteComments.Length);
			Assert.AreEqual("This is a comment", remoteComments[0].Text);
			Assert.IsNotNull(remoteComments[0].CreationDate);

			//Task
			remoteComment.ArtifactId = taskId1;
			remoteComment.Text = "This is a comment";
			spiraImportExport.Task_CreateComment(remoteComment);
			//Verify the data
			remoteComments = spiraImportExport.Task_RetrieveComments(taskId1);
			Assert.AreEqual(1, remoteComments.Length);
			Assert.AreEqual("This is a comment", remoteComments[0].Text);
			Assert.IsNotNull(remoteComments[0].CreationDate);

			//Test Set
			remoteComment.ArtifactId = testSetId1;
			remoteComment.Text = "This is a comment";
			spiraImportExport.TestSet_CreateComment(remoteComment);
			//Verify the data
			remoteComments = spiraImportExport.TestSet_RetrieveComments(testSetId1);
			Assert.AreEqual(1, remoteComments.Length);
			Assert.AreEqual("This is a comment", remoteComments[0].Text);
			Assert.IsNotNull(remoteComments[0].CreationDate);

			//Test Case
			remoteComment.ArtifactId = testCaseId1;
			remoteComment.Text = "This is a comment";
			spiraImportExport.TestCase_CreateComment(remoteComment);
			//Verify the data
			remoteComments = spiraImportExport.TestCase_RetrieveComments(testCaseId1);
			Assert.AreEqual(1, remoteComments.Length);
			Assert.AreEqual("This is a comment", remoteComments[0].Text);
			Assert.IsNotNull(remoteComments[0].CreationDate);
		}

		/// <summary>
		/// Verifies that we can make data-mapping changes to the project
		/// </summary>
		[
		Test,
		SpiraTestCase(724)
		]
		public void _32_DataMappingChanges()
		{
			//First lets authenticate and connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Next lets add a new user to the system
			RemoteUser remoteUser = new RemoteUser();
			remoteUser.FirstName = "Bob";
			remoteUser.LastName = "Holness";
			remoteUser.UserName = "bholness";
			remoteUser.Active = true;
			remoteUser.EmailAddress = "bobholness@blockbusters.com";
			remoteUser.Password = "test123";
			int userId = spiraImportExport.User_Create(remoteUser, 1).UserId.Value;

			//Verify that we have no mappings for this user
			RemoteDataMapping[] remoteUserMappings = spiraImportExport.DataMapping_RetrieveUserMappings(1);
			Assert.AreEqual(3, remoteUserMappings.Length);
			Assert.AreEqual(1, remoteUserMappings[0].InternalId);
			Assert.AreEqual("administrator", remoteUserMappings[0].ExternalKey);
			Assert.AreEqual(2, remoteUserMappings[1].InternalId);
			Assert.AreEqual("fredbloggs", remoteUserMappings[1].ExternalKey);
			Assert.AreEqual(3, remoteUserMappings[2].InternalId);
			Assert.AreEqual("joesmith", remoteUserMappings[2].ExternalKey);

			//Add a mapping
			remoteUserMappings[0].InternalId = userId;
			remoteUserMappings[0].ExternalKey = "bob123";
			spiraImportExport.DataMapping_AddUserMappings(1, remoteUserMappings);

			//Verify the data was added
			remoteUserMappings = spiraImportExport.DataMapping_RetrieveUserMappings(1);
			Assert.AreEqual(4, remoteUserMappings.Length);
			Assert.AreEqual(userId, remoteUserMappings[3].InternalId);
			Assert.AreEqual("bob123", remoteUserMappings[3].ExternalKey);
			Assert.AreEqual(true, remoteUserMappings[3].Primary);

			//Clean up (can't use API since it has no delete methods)
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_PROJECT_USER WHERE USER_ID = " + userId);
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_DATA_SYNC_USER_MAPPING WHERE USER_ID = " + userId);
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId);
		}

		/// <summary>
		/// Verifies that we can retrieve and update the list of automation hosts and engines
		/// </summary>
		[
		Test,
		SpiraTestCase(777)
		]
		public void _33_RetrieveAutomationHostsAndEngines()
		{
			//First verify that we can get the list of active automation engines in the system
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			RemoteAutomationEngine[] remoteAutomationEngines = spiraImportExport.AutomationEngine_Retrieve(true);

			//Verify the count and some of the data
            Assert.AreEqual(4, remoteAutomationEngines.Length);
            Assert.AreEqual("NeoLoad", remoteAutomationEngines[0].Name);
            Assert.AreEqual("NeoLoad", remoteAutomationEngines[0].Token);
            Assert.AreEqual(true, remoteAutomationEngines[0].Active);
            Assert.AreEqual("Quick Test Pro", remoteAutomationEngines[1].Name);
            Assert.AreEqual("QuickTestPro9", remoteAutomationEngines[1].Token);
            Assert.AreEqual(true, remoteAutomationEngines[1].Active);
            Assert.AreEqual("Rapise", remoteAutomationEngines[2].Name);
            Assert.AreEqual("Rapise", remoteAutomationEngines[2].Token);
            Assert.AreEqual(true, remoteAutomationEngines[2].Active);

			//Now try retrieving a single engine by id
			RemoteAutomationEngine remoteAutomationEngine = spiraImportExport.AutomationEngine_RetrieveById(remoteAutomationEngines[1].AutomationEngineId.Value);

			//Verify data
            Assert.AreEqual("Quick Test Pro", remoteAutomationEngine.Name);
            Assert.AreEqual("QuickTestPro9", remoteAutomationEngine.Token);
            Assert.AreEqual(true, remoteAutomationEngine.Active);

			//Now try retrieving a single engine by token
			remoteAutomationEngine = spiraImportExport.AutomationEngine_RetrieveByToken("QuickTestPro9");

			//Verify data
            Assert.AreEqual("Quick Test Pro", remoteAutomationEngine.Name);
            Assert.AreEqual("QuickTestPro9", remoteAutomationEngine.Token);
            Assert.AreEqual(true, remoteAutomationEngine.Active);

			//Next verify that we can get the list of all (active and inactive) automation engines in the system
			remoteAutomationEngines = spiraImportExport.AutomationEngine_Retrieve(false);
			Assert.AreEqual(15, remoteAutomationEngines.Length);

			//Now connect to the newly created project and create two automation hosts
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//First verify that we don't have any existing hosts
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "Name";
			remoteSort.SortAscending = true;
			RemoteAutomationHost[] remoteAutomationHosts = spiraImportExport.AutomationHost_Retrieve(null, remoteSort, 1, 9999);
			Assert.AreEqual(0, remoteAutomationHosts.Length);

			//Now add two hosts
			RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
			remoteAutomationHost.Name = "Test Machine 1";
			remoteAutomationHost.Token = "Test01";
			remoteAutomationHost.Active = true;
			int automationHostId1 = spiraImportExport.AutomationHost_Create(remoteAutomationHost).AutomationHostId.Value;
			remoteAutomationHost = new RemoteAutomationHost();
			remoteAutomationHost.Name = "Test Machine 2";
			remoteAutomationHost.Token = "Test02";
			remoteAutomationHost.Description = "This machine has Windows XP running";
			remoteAutomationHost.Active = true;
			int automationHostId2 = spiraImportExport.AutomationHost_Create(remoteAutomationHost).AutomationHostId.Value;

			//Verify that we can retrieve them
			remoteAutomationHosts = spiraImportExport.AutomationHost_Retrieve(null, remoteSort, 1, 9999);
			Assert.AreEqual(2, remoteAutomationHosts.Length);
			Assert.AreEqual("Test Machine 1", remoteAutomationHosts[0].Name);
			Assert.AreEqual("Test01", remoteAutomationHosts[0].Token);
			Assert.AreEqual(true, remoteAutomationHosts[0].Active);
			Assert.AreEqual("Test Machine 2", remoteAutomationHosts[1].Name);
			Assert.AreEqual("Test02", remoteAutomationHosts[1].Token);
			Assert.AreEqual(true, remoteAutomationHosts[1].Active);

			//Now try filtering and sorting
			remoteSort.PropertyName = "Token";
			remoteSort.SortAscending = false;
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "Token";
			remoteFilter.StringValue = "Test02";
			remoteAutomationHosts = spiraImportExport.AutomationHost_Retrieve(new RemoteFilter[] { remoteFilter }, remoteSort, 1, 9999);
			Assert.AreEqual(1, remoteAutomationHosts.Length);
			Assert.AreEqual("Test Machine 2", remoteAutomationHosts[0].Name);
			Assert.AreEqual("Test02", remoteAutomationHosts[0].Token);
			Assert.AreEqual(true, remoteAutomationHosts[0].Active);

			//Now try and retrieve by id
			remoteAutomationHost = spiraImportExport.AutomationHost_RetrieveById(automationHostId1);
			Assert.AreEqual("Test Machine 1", remoteAutomationHost.Name);
			Assert.AreEqual("Test01", remoteAutomationHost.Token);
			Assert.AreEqual(true, remoteAutomationHost.Active);

			//Now try and retrieve by token
			remoteAutomationHost = spiraImportExport.AutomationHost_RetrieveByToken("Test02");
			Assert.AreEqual("Test Machine 2", remoteAutomationHost.Name);
			Assert.AreEqual("Test02", remoteAutomationHost.Token);
			Assert.AreEqual(true, remoteAutomationHost.Active);

			//Now test that we can update one of the hosts
			remoteAutomationHost.Name = "Test Machine 2a";
			remoteAutomationHost.Description = "Updated test machine";
			spiraImportExport.AutomationHost_Update(remoteAutomationHost);

			//Verify
			remoteAutomationHost = spiraImportExport.AutomationHost_RetrieveByToken("Test02");
			Assert.AreEqual("Test Machine 2a", remoteAutomationHost.Name);
			Assert.AreEqual("Updated test machine", remoteAutomationHost.Description);
			Assert.AreEqual("Test02", remoteAutomationHost.Token);
			Assert.AreEqual(true, remoteAutomationHost.Active);

			//Now test that we can delete one of the hosts
			spiraImportExport.AutomationHost_Delete(automationHostId2);

			//Verify
			remoteAutomationHosts = spiraImportExport.AutomationHost_Retrieve(null, remoteSort, 1, 9999);
			Assert.AreEqual(1, remoteAutomationHosts.Length);
			Assert.AreEqual("Test Machine 1", remoteAutomationHosts[0].Name);
			Assert.AreEqual("Test01", remoteAutomationHosts[0].Token);
			Assert.AreEqual(true, remoteAutomationHosts[0].Active);
		}

		/// <summary>
		/// Verifies that we can make retrieve and update document types, folders and versions
		/// </summary>
		[
		Test,
		SpiraTestCase(778)
		]
		public void _34_RetrieveDocumentFoldersTypesAndVersions()
		{
			//First lets get a list of document types in the sample project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(PROJECT_ID);
			RemoteDocumentType[] remoteDocumentTypes = spiraImportExport.Document_RetrieveTypes(true);

            //Verify
            Assert.AreEqual(6, remoteDocumentTypes.Length);
            Assert.AreEqual("Default", remoteDocumentTypes[5].Name);
            Assert.AreEqual(true, remoteDocumentTypes[5].Active);
            Assert.AreEqual(true, remoteDocumentTypes[5].Default);
            Assert.AreEqual("Functional Specification", remoteDocumentTypes[4].Name);
            Assert.AreEqual("Functional specification for the system. Can be performance or feature related", remoteDocumentTypes[4].Description);
            Assert.AreEqual(true, remoteDocumentTypes[4].Active);
            Assert.AreEqual(false, remoteDocumentTypes[4].Default);

            //Connect to the new project
            spiraImportExport.Connection_ConnectToProject(projectId1);

			//Now verify that we have a single root folder (created by default)
			RemoteDocumentFolder[] remoteDocumentFolders = spiraImportExport.Document_RetrieveFolders();
			Assert.AreEqual(1, remoteDocumentFolders.Length);
			Assert.AreEqual("Root Folder", remoteDocumentFolders[0].Name);
			int oldRootFolder = remoteDocumentFolders[0].ProjectAttachmentFolderId.Value;

			//Now add a two-level hierarchy under this root
			RemoteDocumentFolder remoteDocumentFolder = new RemoteDocumentFolder();
			remoteDocumentFolder.Name = "My Files";
			remoteDocumentFolder.ParentProjectAttachmentFolderId = oldRootFolder;
			int folderId1 = spiraImportExport.Document_AddFolder(remoteDocumentFolder).ProjectAttachmentFolderId.Value;
			remoteDocumentFolder = new RemoteDocumentFolder();
			remoteDocumentFolder.Name = "Photos";
			remoteDocumentFolder.ParentProjectAttachmentFolderId = folderId1;
			int folderId2 = spiraImportExport.Document_AddFolder(remoteDocumentFolder).ProjectAttachmentFolderId.Value;
			remoteDocumentFolder = new RemoteDocumentFolder();
			remoteDocumentFolder.Name = "Documents";
			remoteDocumentFolder.ParentProjectAttachmentFolderId = folderId1;
			int folderId3 = spiraImportExport.Document_AddFolder(remoteDocumentFolder).ProjectAttachmentFolderId.Value;

			//Verify the new folders were added
			remoteDocumentFolders = spiraImportExport.Document_RetrieveFolders();
			Assert.AreEqual(4, remoteDocumentFolders.Length);
			Assert.AreEqual("Root Folder", remoteDocumentFolders[0].Name);
			Assert.AreEqual("AAA", remoteDocumentFolders[0].IndentLevel);
			Assert.AreEqual("My Files", remoteDocumentFolders[1].Name);
			Assert.AreEqual("AAAAAA", remoteDocumentFolders[1].IndentLevel);
			Assert.AreEqual("Documents", remoteDocumentFolders[2].Name);
			Assert.AreEqual("AAAAAAAAA", remoteDocumentFolders[2].IndentLevel);
			Assert.AreEqual("Photos", remoteDocumentFolders[3].Name);
			Assert.AreEqual("AAAAAAAAB", remoteDocumentFolders[3].IndentLevel);

			//Verify that we can retrieve a folder by its id (no indent level available)
			remoteDocumentFolder = spiraImportExport.Document_RetrieveFolderById(folderId3);
			Assert.AreEqual("Documents", remoteDocumentFolder.Name);
			Assert.IsNull(remoteDocumentFolder.IndentLevel);

			//Verify that we can update a folder (name and position)
			remoteDocumentFolder = remoteDocumentFolders[3];
			remoteDocumentFolder.Name = "Photos2";
			remoteDocumentFolder.ParentProjectAttachmentFolderId = oldRootFolder;
			spiraImportExport.Document_UpdateFolder(remoteDocumentFolder);

			//Verify the update
			remoteDocumentFolders = spiraImportExport.Document_RetrieveFolders();
			Assert.AreEqual(4, remoteDocumentFolders.Length);
			Assert.AreEqual("Root Folder", remoteDocumentFolders[0].Name);
			Assert.AreEqual("AAA", remoteDocumentFolders[0].IndentLevel);
			Assert.AreEqual("My Files", remoteDocumentFolders[1].Name);
			Assert.AreEqual("AAAAAA", remoteDocumentFolders[1].IndentLevel);
			Assert.AreEqual("Documents", remoteDocumentFolders[2].Name);
			Assert.AreEqual("AAAAAAAAA", remoteDocumentFolders[2].IndentLevel);
			Assert.AreEqual("Photos2", remoteDocumentFolders[3].Name);
			Assert.AreEqual("AAAAAB", remoteDocumentFolders[3].IndentLevel);

			//Verify that we can delete a folder
			spiraImportExport.Document_DeleteFolder(folderId3);

			//Verify the folders were deleted
			remoteDocumentFolders = spiraImportExport.Document_RetrieveFolders();
			Assert.AreEqual(3, remoteDocumentFolders.Length);
			Assert.AreEqual("Root Folder", remoteDocumentFolders[0].Name);
			Assert.AreEqual("AAA", remoteDocumentFolders[0].IndentLevel);
			Assert.AreEqual("My Files", remoteDocumentFolders[1].Name);
			Assert.AreEqual("AAAAAA", remoteDocumentFolders[1].IndentLevel);
			Assert.AreEqual("Photos2", remoteDocumentFolders[2].Name);
			Assert.AreEqual("AAAAAB", remoteDocumentFolders[2].IndentLevel);

			//Now try adding a document to a folder
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
			RemoteDocument remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "test_data.xls";
			remoteDocument.Description = "Sample Test Case Attachment";
			remoteDocument.AuthorId = USER_ID_FRED_BLOGGS;
			remoteDocument.CurrentVersion = "1.0";
			remoteDocument.ProjectAttachmentFolderId = folderId2;
			int attachmentId1 = spiraImportExport.Document_AddFile(remoteDocument, attachmentData).AttachmentId.Value;

			//Now try adding a newer version
			byte[] attachmentData2 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored (Modified)");
			RemoteDocumentVersion remoteDocumentVersion = new RemoteDocumentVersion();
			remoteDocumentVersion.AttachmentId = attachmentId1;
			remoteDocumentVersion.VersionNumber = "2.0";
			remoteDocumentVersion.FilenameOrUrl = "test_data2.xls";
			remoteDocumentVersion.AuthorId = USER_ID_JOE_SMITH;
			spiraImportExport.Document_AddFileVersion(remoteDocumentVersion, attachmentData2, true);

			//Verify that the data added successfully
			remoteDocument = spiraImportExport.Document_RetrieveById(attachmentId1);
			Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
			Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("2.0", remoteDocument.CurrentVersion);
			Assert.AreEqual(1, remoteDocument.Size);
			Assert.AreEqual(2, remoteDocument.Versions.Length);
			Assert.AreEqual("2.0", remoteDocument.Versions[0].VersionNumber);
			Assert.AreEqual("1.0", remoteDocument.Versions[1].VersionNumber);

			//Now try adding a URL to a folder
			remoteDocument = new RemoteDocument();
			remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
			remoteDocument.Description = "Sample Test Case URL";
			remoteDocument.AuthorId = USER_ID_FRED_BLOGGS;
			remoteDocument.ProjectAttachmentFolderId = folderId2;
			remoteDocument.CurrentVersion = "1.0";
			int attachmentId2 = spiraImportExport.Document_AddUrl(remoteDocument).AttachmentId.Value;

			//Now try adding a newer version
			remoteDocumentVersion = new RemoteDocumentVersion();
			remoteDocumentVersion.AttachmentId = attachmentId2;
			remoteDocumentVersion.FilenameOrUrl = "http://www.tempuri.org/test456.htm";
			remoteDocumentVersion.VersionNumber = "2.0";
			remoteDocumentVersion.AuthorId = USER_ID_JOE_SMITH;
			spiraImportExport.Document_AddUrlVersion(remoteDocumentVersion, true);

			//Verify
			remoteDocument = spiraImportExport.Document_RetrieveById(attachmentId2);
			Assert.AreEqual("http://www.tempuri.org/test456.htm", remoteDocument.FilenameOrUrl);
			Assert.AreEqual("Sample Test Case URL", remoteDocument.Description);
			Assert.AreEqual("URL", remoteDocument.AttachmentTypeName);
			Assert.AreEqual("2.0", remoteDocument.CurrentVersion);
			Assert.AreEqual(0, remoteDocument.Size);
			Assert.AreEqual(2, remoteDocument.Versions.Length);
			Assert.AreEqual("http://www.tempuri.org/test456.htm", remoteDocument.Versions[0].FilenameOrUrl);
			Assert.AreEqual("http://www.tempuri.org/test123.htm", remoteDocument.Versions[1].FilenameOrUrl);

			//Finally test that we can delete the document and url
			spiraImportExport.Document_Delete(attachmentId1);
			spiraImportExport.Document_Delete(attachmentId2);

			//Verify that they were deleted
			Assert.IsNull(spiraImportExport.Document_RetrieveById(attachmentId1));
			Assert.IsNull(spiraImportExport.Document_RetrieveById(attachmentId2));
		}

		/// <summary>
		/// Verifies that we can delete requirements, releases, test cases, test sets, incidents and tasks
		/// </summary>
		[
		Test,
		SpiraTestCase(779)
		]
		public void _35_DeleteItems()
		{
			//Connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Delete a requirement and verify
			spiraImportExport.Requirement_Delete(requirementId1);
			Assert.IsNull(spiraImportExport.Requirement_RetrieveById(requirementId1));

			//Delete a release and verify
			spiraImportExport.Release_Delete(iterationId1);
			Assert.IsNull(spiraImportExport.Release_RetrieveById(iterationId1));

			//Delete a test case and verify
			spiraImportExport.TestCase_Delete(testCaseId1);
			Assert.IsNull(spiraImportExport.TestCase_RetrieveById(testCaseId1));

			//Delete a test set and verify
			spiraImportExport.TestSet_Delete(testSetId1);
			Assert.IsNull(spiraImportExport.TestSet_RetrieveById(testSetId1));

			//Delete an incident and verify
			spiraImportExport.Incident_Delete(incidentId1);
			Assert.IsNull(spiraImportExport.Incident_RetrieveById(incidentId1));

			//Delete a task and verify
			spiraImportExport.Task_Delete(taskId1);
			Assert.IsNull(spiraImportExport.Task_RetrieveById(taskId1));

			//The delete a test step is tested separately in the _37_MoveArtifacts test
		}

		/// <summary>
		/// Tests that you can create new builds through the API and associate incidents, test runs and source code revisions
		/// </summary>
		[
		Test,
		SpiraTestCase(814)
		]
		public void _36_BuildManagement()
		{
			//Connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Record a couple of new builds against an existing iteration, adding some revisions to the first one
			//Build 1
			RemoteBuild remoteBuild1 = new RemoteBuild();
			remoteBuild1.BuildStatusId = 2;  //Succeeded
			remoteBuild1.ReleaseId = iterationId2;
			remoteBuild1.Name = "Build 0001";
			remoteBuild1.Description = "The first build";
			remoteBuild1.CreationDate = DateTime.Now.AddDays(-1);
			List<RemoteBuildSourceCode> revisions = new List<RemoteBuildSourceCode>();
			revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev001" });
			revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev002" });
			revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev003" });
			revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev004" });
			revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev005" });
			remoteBuild1.Revisions = revisions.ToArray();
			remoteBuild1 = spiraImportExport.Build_Create(remoteBuild1);
			//Build 2
			RemoteBuild remoteBuild2 = new RemoteBuild();
			remoteBuild2.BuildStatusId = 1;  //Failed
			remoteBuild2.ReleaseId = iterationId2;
			remoteBuild2.Name = "Build 0002";
			remoteBuild2.Description = null;
			remoteBuild2 = spiraImportExport.Build_Create(remoteBuild2);

			//Now test that we can retrieve a single build with its revisions
			RemoteBuild remoteBuild = spiraImportExport.Build_RetrieveById(iterationId2, remoteBuild1.BuildId.Value);
			Assert.IsNotNull(remoteBuild);
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("The first build", remoteBuild.Description);
			Assert.AreEqual(5, remoteBuild.Revisions.Length);
			Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
			Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
			Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
			Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
			Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Now test that we can retrieve all the builds with their revisions
			RemoteBuild[] remoteBuilds = spiraImportExport.Build_RetrieveByReleaseId(iterationId2, null, null, 1, 15);
			Assert.AreEqual(2, remoteBuilds.Length);
			remoteBuild = remoteBuilds[1];
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("The first build", remoteBuild.Description);
			Assert.AreEqual(5, remoteBuild.Revisions.Length);
			Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
			Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
			Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
			Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
			Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Now try filtering by build status and sending a sort
			RemoteSort remoteSort = new RemoteSort();
			remoteSort.PropertyName = "Name";
			remoteSort.SortAscending = true;
			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildStatusId", IntValue = 2 });
			remoteBuilds = spiraImportExport.Build_RetrieveByReleaseId(iterationId2, remoteFilters.ToArray(), remoteSort, 1, 15);
			Assert.AreEqual(1, remoteBuilds.Length);
			remoteBuild = remoteBuilds[0];
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("The first build", remoteBuild.Description);
			Assert.AreEqual(5, remoteBuild.Revisions.Length);
			Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
			Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
			Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
			Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
			Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Now try filtering by multiple build statuses and sending a sort
			remoteSort = new RemoteSort();
			remoteSort.PropertyName = "Name";
			remoteSort.SortAscending = true;
			MultiValueFilter multiValues = new MultiValueFilter();
			multiValues.Values = new int[2] { 1, 2 };
			remoteFilters.Clear();
			remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildStatusId", MultiValue = multiValues });
			remoteBuilds = spiraImportExport.Build_RetrieveByReleaseId(iterationId2, remoteFilters.ToArray(), remoteSort, 1, 15);
			Assert.AreEqual(2, remoteBuilds.Length);
			remoteBuild = remoteBuilds[0];
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("The first build", remoteBuild.Description);
			Assert.AreEqual(5, remoteBuild.Revisions.Length);
			Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
			Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
			Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
			Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
			Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Now lets record an automated test against this build
			RemoteAutomatedTestRun remoteAutomatedTestRun = new RemoteAutomatedTestRun();
			remoteAutomatedTestRun.TestCaseId = testCaseId2;
			remoteAutomatedTestRun.ReleaseId = iterationId2;
			remoteAutomatedTestRun.BuildId = remoteBuild1.BuildId.Value;
			remoteAutomatedTestRun.StartDate = DateTime.Now;
			remoteAutomatedTestRun.EndDate = DateTime.Now.AddMinutes(2);
			remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
			remoteAutomatedTestRun.RunnerName = "TestSuite";
			remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
			testRunId3 = spiraImportExport.TestRun_RecordAutomated1(remoteAutomatedTestRun).TestRunId.Value;

			//Retrieve the list of test runs by build
			remoteFilters.Clear();
			int[] filterValues = { remoteBuild1.BuildId.Value };
			remoteSort.PropertyName = "TestRunId";
			remoteSort.SortAscending = true;
			remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildId", MultiValue = new MultiValueFilter() { Values = filterValues } });
			RemoteTestRun[] remoteTestRuns = spiraImportExport.TestRun_Retrieve(remoteFilters.ToArray(), remoteSort, 1, 15);
			Assert.AreEqual(1, remoteTestRuns.Length);
			Assert.AreEqual(remoteBuild1.BuildId, remoteTestRuns[0].BuildId.Value);

			//Now lets mark a defect as fixed in this build
			RemoteIncident remoteIncident = new RemoteIncident();
			remoteIncident.IncidentTypeId = incidentTypeId;
			remoteIncident.IncidentStatusId = incidentStatusId;
			remoteIncident.Name = "Fixed Incident";
			remoteIncident.ResolvedReleaseId = iterationId2;
			remoteIncident.FixedBuildId = remoteBuild1.BuildId.Value;
			remoteIncident.Description = "This is a test incident fixed in the build";
			remoteIncident.CreationDate = DateTime.Parse("1/5/2005");
			incidentId1 = spiraImportExport.Incident_Create(remoteIncident).IncidentId.Value;

			//Retrieve the list of defects by build
			remoteSort.PropertyName = "IncidentId";
			remoteSort.SortAscending = true;
			RemoteIncident[] remoteIncidents = spiraImportExport.Incident_Retrieve(remoteFilters.ToArray(), remoteSort, 1, 15);
			Assert.AreEqual(1, remoteTestRuns.Length);
			Assert.AreEqual(remoteBuild1.BuildId, remoteTestRuns[0].BuildId.Value);
		}

		/// <summary>
		/// Tests that you can move requirements, releases, test sets, test cases and test steps
		/// </summary>
		[
		Test,
		SpiraTestCase(815)
		]
		public void _37_MoveArtifacts()
		{
			//Connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Lets add some requirements, a test case, test set and release to the end of their respective lists
			//Requirement
			RemoteRequirement remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 2;
			remoteRequirement.ImportanceId = 1;
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.Name = "Requirement To Move";
			remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, null);
			int requirementId1 = remoteRequirement.RequirementId.Value;
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.StatusId = 2;
			remoteRequirement.ImportanceId = 1;
			remoteRequirement.ReleaseId = releaseId1;
			remoteRequirement.Name = "Requirement To Move";
			remoteRequirement = spiraImportExport.Requirement_Create2(remoteRequirement, null);
			int requirementId2 = remoteRequirement.RequirementId.Value;
			//Release
			RemoteRelease remoteRelease = new RemoteRelease();
			remoteRelease.Name = "Release 5";
			remoteRelease.VersionNumber = "5.0";
			remoteRelease.Active = true;
			remoteRelease.Iteration = false;
			remoteRelease.StartDate = DateTime.Now;
			remoteRelease.EndDate = DateTime.Now.AddMonths(1);
			remoteRelease.ResourceCount = 1;
			int releaseId = spiraImportExport.Release_Create(remoteRelease, null).ReleaseId.Value;
			//Test Case
			RemoteTestCase remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case To Move";
			remoteTestCase.Active = true;
			remoteTestCase.TestCasePriorityId = 1;
			remoteTestCase.EstimatedDuration = 30;
			remoteTestCase = spiraImportExport.TestCase_Create(remoteTestCase, null);
			int testCaseId = remoteTestCase.TestCaseId.Value;
			//Test Steps
			//Step 1
			RemoteTestStep remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 1;
			remoteTestStep.Description = "Test Step 1";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep.SampleData = "test 123";
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId);
			int testStepId1 = remoteTestStep.TestStepId.Value;
			//Step 2
			remoteTestStep = new RemoteTestStep();
			remoteTestStep.Position = 2;
			remoteTestStep.Description = "Test Step 2";
			remoteTestStep.ExpectedResult = "It should work";
			remoteTestStep = spiraImportExport.TestCase_AddStep(remoteTestStep, testCaseId);
			int testStepId2 = remoteTestStep.TestStepId.Value;
			//Test Set
			RemoteTestSet remoteTestSet = new RemoteTestSet();
			remoteTestSet.Name = "Test Set 1";
			remoteTestSet.CreatorId = userId1;
			remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
			int testSetId = spiraImportExport.TestSet_Create(remoteTestSet, null).TestSetId.Value;

            //Verify requirement starting positions
            Assert.AreEqual("AAA", spiraImportExport.Requirement_RetrieveById(requirementId1).IndentLevel);
            Assert.AreEqual("AAB", spiraImportExport.Requirement_RetrieveById(requirementId2).IndentLevel);

			//Now move the requirement to the start of the list
			spiraImportExport.Requirement_Move(requirementId2, requirementId1);
			//Verify
			Assert.AreEqual("AAA", spiraImportExport.Requirement_RetrieveById(requirementId2).IndentLevel);
			//Now move back to the end of the list
			spiraImportExport.Requirement_Move(requirementId2, null);
			//Verify
			Assert.AreEqual("AAB", spiraImportExport.Requirement_RetrieveById(requirementId2).IndentLevel);

            //Now move the test case under a folder
            spiraImportExport.TestCase_Move(testCaseId, testFolderId1);
            //Verify
            Assert.IsTrue(spiraImportExport.TestCase_RetrieveByFolder(testFolderId1).Any(t => t.TestCaseId == testCaseId));
            //Now move back to the end of the list
            spiraImportExport.TestCase_Move(testCaseId, null);
            //Verify
            Assert.IsFalse(spiraImportExport.TestCase_RetrieveByFolder(testFolderId1).Any(t => t.TestCaseId == testCaseId));

            //Now move the test set under a folder
            spiraImportExport.TestSet_Move(testSetId, testSetFolderId1);
            //Verify
            RemoteTestSet[] testSets = spiraImportExport.TestSet_Retrieve(null, 1, Int32.MaxValue);
            Assert.AreEqual("AAAAAA", testSets.FirstOrDefault(t => t.TestSetId == testSetId).IndentLevel);
            //Now move back to the end of the list
            spiraImportExport.TestSet_Move(testSetId, null);
            //Verify
            testSets = spiraImportExport.TestSet_Retrieve(null, 1, Int32.MaxValue);
            Assert.AreNotEqual("AAAAAA", testSets.FirstOrDefault(t => t.TestSetId == testSetId).IndentLevel);

			//Now move the release to the start of the list
			spiraImportExport.Release_Move(releaseId, releaseId1);
			//Verify
			Assert.AreEqual("AAA", spiraImportExport.Release_RetrieveById(releaseId).IndentLevel);
			//Now move back to the end of the list
			spiraImportExport.Release_Move(releaseId, null);
			//Verify
			Assert.AreNotEqual("AAA", spiraImportExport.Release_RetrieveById(releaseId).IndentLevel);

			//Now Try moving one of the test steps in the test case
			spiraImportExport.TestCase_MoveStep(testCaseId, testStepId2, testStepId1);
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[0].TestStepId.Value);
			Assert.AreEqual(testStepId1, remoteTestCase.TestSteps[1].TestStepId.Value);

			//Now Try moving one of the test steps in the test case back to the end
			spiraImportExport.TestCase_MoveStep(testCaseId, testStepId2, null);
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(testStepId1, remoteTestCase.TestSteps[0].TestStepId.Value);
			Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[1].TestStepId.Value);

			//Finally test that we can delete a test step
			spiraImportExport.TestCase_DeleteStep(testCaseId, testStepId1);
			remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
			Assert.AreEqual(1, remoteTestCase.TestSteps.Length);
			Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[0].TestStepId.Value);
		}

		/// <summary>Tests that the counts we get back are as expected.</summary>
		[Test, SpiraTestCase(1124)]
		public void _38_CheckCounts()
		{
			//Connect to the project
			spiraImportExport.Connection_Authenticate("Administrator", "Welcome123$");
			spiraImportExport.Connection_ConnectToProject(projectId1);

			//Get the count functions..
			long numInc = spiraImportExport.Incident_Count(null);
			long numReq = spiraImportExport.Requirement_Count(null);
			long numRel = spiraImportExport.Release_Count(null);
			long numTsk = spiraImportExport.Task_Count(null);
			long numTC = spiraImportExport.TestCase_Count(null);
			long numTS = spiraImportExport.TestSet_Count(null);
			long numTR = spiraImportExport.TestRun_Count(null);

			Assert.AreEqual(3, numInc);
			Assert.AreEqual(2, numReq);
			Assert.AreEqual(3, numRel);
			Assert.AreEqual(1, numTsk);
            Assert.AreEqual(4, numTC);
            Assert.AreEqual(2, numTS);
			Assert.AreEqual(7, numTR);
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
        [SpiraTestCase(1859)]
        public void _39_CheckCompatibilityWithOldStaticIDs()
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
            spiraImportExport.TestCase_Update(remoteTestCase);

            //Verify the data
            remoteTestCase = spiraImportExport.TestCase_RetrieveById(testCaseId);
            Assert.AreNotEqual(3, remoteTestCase.TestCasePriorityId);
        }
    }
}
