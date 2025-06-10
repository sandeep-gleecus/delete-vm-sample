using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.TestSuite;
using Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation;
using Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation.SpiraRestService60;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using System.Collections;

namespace Inflectra.SpiraTest.ApiTestSuite.API_Tests
{
    /// <summary>
    /// This fixture tests that the v6.0 import/export web service interface works correctly
    /// </summary>
    [
    TestFixture,
    SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
    ]
    public class V6_0_RestTest
    {
        protected static SpiraRestClient60 spiraSoapClient;
        protected static RemoteCredentials credentials;

        protected static long lastArtifactHistoryId;
        protected static long lastArtifactChangeSetId;

        protected static int projectId1;
        protected static int projectId2;
        protected static int projectId3;
        protected static int projectId4;

        protected static int projectTemplateId1;
        protected static int projectTemplateId3;
        protected static int projectTemplateId4;
        protected static int projectTemplateId5;

        protected static int customListId1;
        protected static int customListId2;
        protected static int customValueId1;
        protected static int customValueId2;
        protected static int customValueId3;

        protected static int componentId1;
        protected static int componentId2;
        protected static int componentId3;

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
		protected static int testCaseParameterId1;

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
        protected static int testRunId6;

        protected static int taskFolderId1;
        protected static int taskFolderId2;

        protected static int taskId1;
        protected static int taskId2;

        protected static int graphCustomId1 = 0;
        protected static int reportSavedId = 0;

        private const int PROJECT_ID = 1;
        private const int PROJECT_TEMPLATE_ID = 1;

        private const int USER_ID_SYSTEM_ADMIN = 1;
        private const int USER_ID_FRED_BLOGGS = 2;
        private const int USER_ID_JOE_SMITH = 3;

        private const string API_KEY_SYSTEM_ADMIN = "{B9050F75-C5E6-4244-8712-FBF20061A976}";
        private const string API_KEY_FRED_BLOGGS = "{7A05FD06-83C3-4436-B37F-51BCF0060483}";
        private const string API_KEY_ADAM_ANT = "{C51C4A02-B42D-4228-82D1-2F584E725C4C}";

        private const int AUTOMATION_ENGINE_ID_QTP = 2;

        private const string PLUGIN_NAME = "ApiUnitTest";

        /// <summary>
        /// Sets up the web service interface
        /// </summary>
        [TestFixtureSetUp]
        public void Init()
        {
            //Instantiate the web-service proxy classes and set the URL from the .config file
            spiraSoapClient = new SpiraRestClient60(Properties.Settings.Default.WebServiceUrl + "v6_0/RestService.svc");

            //Get the last artifact id
            Business.HistoryManager history = new Business.HistoryManager();
            history.RetrieveLatestDetails(out lastArtifactHistoryId, out lastArtifactChangeSetId);

            //We have to make the data-syncs active
            InternalRoutines.ExecuteNonQuery("UPDATE TST_DATA_SYNC_SYSTEM SET IS_ACTIVE = 1");
        }

        /// <summary>
        /// Cleans up any created projects and associated data
        /// </summary>
        [TestFixtureTearDown]
        public void CleanUp()
        {
            //Delete the newly created project and all its artifacts
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            spiraSoapClient.Project_Delete(credentials, projectId1);

            //Delete any other projects
            if (projectId3 > 0)
            {
                spiraSoapClient.Project_Delete(credentials, projectId3);
            }
            if (projectId4 > 0)
            {
                spiraSoapClient.Project_Delete(credentials, projectId4);
            }

            //Delete the template (no v5.0 api call available for this)
            TemplateManager templateManager = new TemplateManager();
            templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId1);

            //Delete any other project templates
            if (projectTemplateId3 > 0)
            {
                templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId3);
            }
            if (projectTemplateId4 > 0)
            {
                templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId4);
            }
            if (projectTemplateId5 > 0)
            {
                templateManager.Delete(USER_ID_SYSTEM_ADMIN, projectTemplateId5);
            }


            //We need to delete any artifact history items before deleting the users
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_DETAIL WHERE ARTIFACT_HISTORY_ID > " + lastArtifactHistoryId.ToString());
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_HISTORY_CHANGESET WHERE CHANGESET_ID > " + lastArtifactChangeSetId.ToString());

            //Delete the newly created users
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId1.ToString());
            InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER WHERE USER_ID = " + userId2.ToString());

            //Make the data-syncs inactive again
            InternalRoutines.ExecuteNonQuery("UPDATE TST_DATA_SYNC_SYSTEM SET IS_ACTIVE = 0");

            //Delete any custom graphs/saved reports
            if (graphCustomId1 > 0)
            {
                new GraphManager().GraphCustom_Delete(graphCustomId1);
            }
            if (reportSavedId > 0)
            {
                new ReportManager().DeleteSaved(reportSavedId);
            }
        }

        ///// <summary>
        ///// This is just a simple test to make sure concurrency date-time handling is working
        ///// </summary>
        //[Test]
        //public void SimpleConcurrencyTest()
        //{
        //    credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

        //    //Just retrieve and make no changes
        //    DataModel.TestCaseView testCase = new Business.TestCaseManager().RetrieveById(1, 4);
        //    RemoteTestCase remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, 1, 4);
        //    Assert.AreEqual(testCase.ConcurrencyDate.ToDatabaseSerialization(), remoteTestCase.ConcurrencyDate.ToDatabaseSerialization());
        //    spiraSoapClient.TestCase_Update(credentials, 1, remoteTestCase);
        //}

        /// <summary>
        /// Tests to make sure all of the CORS preflight methods actually work (one per unique URL endpoint)
        /// </summary>
        /// <remarks>
        /// https://developer.mozilla.org/en-US/docs/Glossary/Preflight_request
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Request-Method
        /// </remarks>
        [Test]
        [SpiraTestCase(2080)]
        public void _00_TestCORSPreflight()
        {
            //We need to open up the JSON file and read the references
            JObject jDefinition = JObject.Parse(File.ReadAllText(@"C:\Git\SpiraTeam\ApiTestSuite\Rest References\Inflectra.SpiraTest.Web.Services.v6_0.RestServiceDescription.json"));

            //Get the base URL
            string baseUrl = Properties.Settings.Default.WebServiceUrl + "v6_0/RestService.svc";

            //Create a regex to match and replace parameters
            Regex regex = new Regex(@"\{[\w_]*\}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //Loop through all the resources/methods
            List<string> urlAndMethods = new List<string>();
            foreach (JProperty resource in jDefinition["resources"])
            {
                //Loop through all the methods
                foreach (JProperty method in resource.Value["methods"])
                {
                    //Get the path and method name
                    string methodName = method.Value["name"].Value<string>();
                    string relativeUrl = method.Value["path"].Value<string>();

                    //Replace any parameters with integers to make the path match
                    relativeUrl = regex.Replace(relativeUrl, "1");

                    //Remove the query string part
                    string fullUrl = baseUrl + "/" + relativeUrl;
                    Uri uri = new Uri(fullUrl);
                    fullUrl = uri.Scheme + "://" + uri.Host + uri.AbsolutePath;
                    string urlAndMethod = fullUrl + "|" + methodName;

                    //Add to list if not already added
                    if (!urlAndMethods.Contains(urlAndMethod))
                    {
                        urlAndMethods.Add(urlAndMethod);
                        Console.WriteLine(urlAndMethod);
                    }
                }
            }

            //Now loop through all of the URLs and try a pre-flight request using OPTIONS
            const string originUrl = "https://files.inflectra.com";
            foreach (string urlAndMethod in urlAndMethods)
            {
                string[] components = urlAndMethod.Split('|');
                string url = components[0];
                string method = components[1];

                RestRequest request = new RestRequest("Preflight");
                request.Credential = new System.Net.NetworkCredential();
                request.Credential.UserName = "fredbloggs";
                request.Credential.Password = API_KEY_FRED_BLOGGS;
                request.Method = "OPTIONS";
                request.Url = url;
                request.Headers.Add(new RestHeader() { Name = StandardHeaders.Accept, Value = "application/json" });
                request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });
                request.Headers.Add(new RestHeader() { Name = StandardHeaders.Access_Control_Request_Method, Value = method });
                request.Headers.Add(new RestHeader() { Name = StandardHeaders.Access_Control_Request_Headers, Value = "origin, x-requested-with" });
                request.Headers.Add(new RestHeader() { Name = StandardHeaders.Origin, Value = originUrl });

                //Authenticate against the web service by calling a simple method
                RestClient restClient = new RestClient(request);
                RestResponse response = restClient.SendRequest();
                string errorCode = response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value;
                bool success = (!response.IsErrorStatus && errorCode == "200 OK");
                string message = url + " - " + errorCode;
                Assert.IsTrue(success, message);

                //Now make sure we get back the expected CORS headers
                string allowOrigin = response.Headers.FirstOrDefault(h => h.Name == StandardHeaders.Access_Control_Allow_Origin).Value;
                string allowMethods = response.Headers.FirstOrDefault(h => h.Name == StandardHeaders.Access_Control_Allow_Methods).Value;
                Assert.AreEqual(originUrl, allowOrigin);
                Assert.IsTrue(allowMethods.Contains(method), url + " - " + allowMethods);
            }
        }

        /// <summary>
        /// Verifies that you can authenticate as a specific user
        /// </summary>
        [
        Test,
        SpiraTestCase(1975)
        ]
        public void _01_Authentication()
        {
            //We need to write this using native rest calls not the proxy classes
            //so that we can test the different auth methods (headers vs. url)
            RestClient restClient = new RestClient();

            //Create a simple request with no authentication, should fail
            RestRequest request = new RestRequest("Connection_Authenticate");
            request.Url = Properties.Settings.Default.WebServiceUrl + "v6_0/RestService.svc/users";
            request.Method = "GET";
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Accept, Value = "application/json" });
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });
            restClient.CurrentRequest = request;
            RestResponse response = restClient.SendRequest();
            Assert.IsTrue(response.IsErrorStatus);

            //Now use the basic auth header
            request.Credential = new System.Net.NetworkCredential();
            request.Credential.UserName = "fredbloggs";
            request.Credential.Password = API_KEY_FRED_BLOGGS;
            response = restClient.SendRequest();
            Assert.IsFalse(response.IsErrorStatus);

            //Now use the special Spira custom header
            request.Credential = null;
            request.Headers.Add(new RestHeader() { Name = "username", Value = "fredbloggs" });
            request.Headers.Add(new RestHeader() { Name = "api-key", Value = API_KEY_FRED_BLOGGS });
            response = restClient.SendRequest();
            Assert.IsFalse(response.IsErrorStatus);

            //Finally pass via. the URL
            request.Headers.Clear();
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Accept, Value = "application/json" });
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });
            request.Url = Properties.Settings.Default.WebServiceUrl + String.Format("v6_0/RestService.svc/users?username={0}&api-key={1}", "fredbloggs", API_KEY_FRED_BLOGGS);
            response = restClient.SendRequest();
            Assert.IsFalse(response.IsErrorStatus);
        }

        /// <summary>
        /// Verifies that you can retrieve the list of projects for different users
        /// </summary>
        [
        Test,
        SpiraTestCase(2008)
        ]
        public void _02_RetrieveProjectList()
        {
            //Download the project list for a normal user with single-case password and username
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);
            Assert.IsNotNull(credentials);
            RemoteProject[] remoteProjects = spiraSoapClient.Project_Retrieve(credentials);

            //Check the number of projects
            Assert.AreEqual(23, remoteProjects.Length, "Project List 1");

            //Check some of the data
            Assert.AreEqual("Billing System", remoteProjects[0].Name);
            Assert.IsTrue(remoteProjects[0].ProjectTemplateId.HasValue && remoteProjects[0].ProjectTemplateId > 0);
            Assert.AreEqual("Clinical Medical Devices", remoteProjects[1].Name);
            Assert.IsTrue(remoteProjects[1].ProjectTemplateId.HasValue && remoteProjects[1].ProjectTemplateId > 0);
            Assert.AreEqual("Company Website", remoteProjects[2].Name);
            Assert.IsTrue(remoteProjects[2].ProjectTemplateId.HasValue && remoteProjects[2].ProjectTemplateId > 0);

            //Download the project list for an Admin user with mixed-case password and username
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            Assert.IsNotNull(credentials);
            remoteProjects = spiraSoapClient.Project_Retrieve(credentials);

            //Check the number of projects
            Assert.AreEqual(23, remoteProjects.Length, "Project List 2");

            //Check some of the data
            Assert.AreEqual("Billing System", remoteProjects[0].Name);
            Assert.AreEqual("Clinical Medical Devices", remoteProjects[1].Name);
            Assert.AreEqual("Company Website", remoteProjects[2].Name);

            //Make sure that you can't get project list if no credentials passed
            bool errorCaught = false;
            try
            {
                remoteProjects = spiraSoapClient.Project_Retrieve(null);
            }
            catch (Exception exception)
            {
                //Need to get the underlying message fault
                if (exception.Message == "400 Bad Request")
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
            credentials = spiraSoapClient.Connection_Authenticate("wrong", "wrong", PLUGIN_NAME);
            errorCaught = false;
            try
            {
                remoteProjects = spiraSoapClient.Project_Retrieve(credentials);
            }
            catch (Exception exception)
            {
                //Need to get the underlying message fault
                if (exception.Message == "400 Bad Request")
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
        SpiraTestCase(1980)
        ]
        public void _03_CreateProject()
        {
            //Create a new project into which we will import various other artifacts
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            RemoteProject remoteProject = new RemoteProject();
            remoteProject.Name = "Imported Project";
            remoteProject.Description = "This project was imported by the Importer Unit Test";
            remoteProject.Website = "www.tempuri.org";
            remoteProject.Active = true;
            remoteProject = spiraSoapClient.Project_Create(credentials, null, remoteProject);
            projectId1 = remoteProject.ProjectId.Value;

            //Get the template associated with the project
            projectTemplateId1 = new TemplateManager().RetrieveForProject(projectId1).ProjectTemplateId;

            //Lets verify that we can retrieve the project in question
            remoteProject = spiraSoapClient.Project_RetrieveById(credentials, projectId1);
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
            remoteProject = spiraSoapClient.Project_Create(credentials, projectId1, remoteProject);
            projectId2 = remoteProject.ProjectId.Value;

            //Now lets test accessing the copied prokect
            remoteProject = spiraSoapClient.Project_RetrieveById(credentials, projectId2);
            Assert.IsNotNull(remoteProject, "Authorization 1 Failed");

            //Now lets test connecting to the original project
            remoteProject = spiraSoapClient.Project_RetrieveById(credentials, projectId1);
            Assert.IsNotNull(remoteProject, "Authorization 2 Failed");

            //Test that we can modify the copy of the project
            remoteProject = spiraSoapClient.Project_RetrieveById(credentials, projectId2);
            remoteProject.Name = "Imported Project 2a";
            remoteProject.Description = "Test 2";
            remoteProject.Active = true;
            spiraSoapClient.Project_Update(credentials, projectId2, remoteProject);

            //Verify
            remoteProject = spiraSoapClient.Project_RetrieveById(credentials, projectId2);
            Assert.AreEqual("Imported Project 2a", remoteProject.Name);
            Assert.AreEqual("Test 2", remoteProject.Description);
            Assert.AreEqual(true, remoteProject.Active);

            //Finally delete the copy of the project
            spiraSoapClient.Project_Delete(credentials, projectId2);
        }

        /// <summary>
        /// Verifies that you can import users to a particular project
        /// </summary>
        [
        Test,
        SpiraTestCase(1999)
        ]
        public void _04_ImportUsers()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Now lets add a couple of users to the project
            RemoteUser remoteUser = new RemoteUser();
            //User #1
            remoteUser.FirstName = "Adam";
            remoteUser.MiddleInitial = "";
            remoteUser.LastName = "Ant";
            remoteUser.UserName = "aant";
            remoteUser.EmailAddress = "aant@antmusic.com";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.Approved = true;
            remoteUser.LdapDn = "";
            remoteUser.RssToken = API_KEY_ADAM_ANT;
            remoteUser = spiraSoapClient.User_Create(credentials, "aant123456789", "What is 2+3?", "5", projectId1, InternalRoutines.PROJECT_ROLE_MANAGER, remoteUser);
            userId1 = remoteUser.UserId.Value;
            //User #2
            remoteUser = new RemoteUser();
            remoteUser.FirstName = "Martha";
            remoteUser.MiddleInitial = "T";
            remoteUser.LastName = "Muffin";
            remoteUser.UserName = "mmuffin";
            remoteUser.EmailAddress = "mmuffin@echobeach.biz";
            remoteUser.Admin = true;
            remoteUser.Active = true;
            remoteUser.Approved = true;
            remoteUser.LdapDn = "CN=Martha T Muffin,CN=Users,DC=EchoBeach,DC=Com";
            remoteUser = spiraSoapClient.User_Create(credentials, null, null, null, projectId1, InternalRoutines.PROJECT_ROLE_TESTER, remoteUser);
            userId2 = remoteUser.UserId.Value;

            //Verify that they inserted correctly
            //User #1
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, userId1);
            Assert.AreEqual("Adam", remoteUser.FirstName);
            Assert.AreEqual("Ant", remoteUser.LastName);
            Assert.AreEqual("aant", remoteUser.UserName);
            Assert.AreEqual("aant@antmusic.com", remoteUser.EmailAddress);
            Assert.IsTrue(String.IsNullOrEmpty(remoteUser.LdapDn));
            Assert.IsTrue(String.IsNullOrEmpty(remoteUser.Department));
            Assert.IsTrue(remoteUser.Approved);
            Assert.IsTrue(remoteUser.Active);
            Assert.IsFalse(remoteUser.Admin);
            Assert.IsFalse(remoteUser.Locked);
            Assert.IsFalse(String.IsNullOrEmpty(remoteUser.RssToken));
            //User #2
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, userId2);
            Assert.AreEqual("Martha", remoteUser.FirstName);
            Assert.AreEqual("Muffin", remoteUser.LastName);
            Assert.AreEqual("mmuffin", remoteUser.UserName);
            Assert.AreEqual("mmuffin@echobeach.biz", remoteUser.EmailAddress);
            Assert.AreEqual("CN=Martha T Muffin,CN=Users,DC=EchoBeach,DC=Com", remoteUser.LdapDn);
            Assert.IsTrue(String.IsNullOrEmpty(remoteUser.Department));
            Assert.IsTrue(remoteUser.Approved);
            Assert.IsTrue(remoteUser.Active);
            Assert.IsTrue(remoteUser.Admin);
            Assert.IsFalse(remoteUser.Locked);
            Assert.IsTrue(String.IsNullOrEmpty(remoteUser.RssToken));

            //Now verify their roles on the project
            RemoteProjectUser[] remoteProjectUsers = spiraSoapClient.Project_RetrieveUserMembership(credentials, projectId1);
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
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.LdapDn = "";
            remoteUser = spiraSoapClient.User_Create(credentials, "aant123456789", "What is 1+1?", "2", projectId1, InternalRoutines.PROJECT_ROLE_MANAGER, remoteUser);
            int userId3 = remoteUser.UserId.Value;
            Assert.AreEqual(userId1, userId3);

            //Test that we can add an existing user to a project
            RemoteProjectUser remoteProjectUser = new RemoteProjectUser();
            remoteProjectUser.UserId = USER_ID_FRED_BLOGGS;
            remoteProjectUser.ProjectId = projectId1;
            remoteProjectUser.ProjectRoleId = 2;    //Manager
            spiraSoapClient.Project_AddUserMembership(credentials, projectId1, remoteProjectUser);

            //Verify
            remoteProjectUsers = spiraSoapClient.Project_RetrieveUserMembership(credentials, projectId1);
            remoteProjectUser = remoteProjectUsers.FirstOrDefault(p => p.UserId == USER_ID_FRED_BLOGGS);
            Assert.IsNotNull(remoteProjectUser);
            Assert.AreEqual(2, remoteProjectUser.ProjectRoleId);

            //Test that we can update a user
            remoteUser = new RemoteUser();
            remoteUser.FirstName = "Zammo";
            remoteUser.MiddleInitial = "X";
            remoteUser.LastName = "Boris";
            remoteUser.UserName = "zboris";
            remoteUser.EmailAddress = "zboris@grangehill.net";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.Approved = true;
            remoteUser = spiraSoapClient.User_Create(credentials, "mypassword123456", "What is 1+1?", "2", projectId1, InternalRoutines.PROJECT_ROLE_TESTER, remoteUser);
            int dummyUserId = remoteUser.UserId.Value;

            //Now update the user
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, dummyUserId);
            remoteUser.FirstName = "Zingo";
            remoteUser.MiddleInitial = "X";
            remoteUser.LastName = "Bravo";
            remoteUser.UserName = "zbravo";
            remoteUser.EmailAddress = "zbravo@grangehill.edu";
            spiraSoapClient.User_Update(credentials, dummyUserId, remoteUser);

            //Verify
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, dummyUserId);
            Assert.AreEqual("zbravo", remoteUser.UserName);
            Assert.AreEqual("Zingo", remoteUser.FirstName);
            Assert.AreEqual("Bravo", remoteUser.LastName);
            Assert.AreEqual("zbravo@grangehill.edu", remoteUser.EmailAddress);

            //Verify that the user is a member of the project
            remoteProjectUsers = spiraSoapClient.Project_RetrieveUserMembership(credentials, projectId1);
            remoteProjectUser = remoteProjectUsers.FirstOrDefault(p => p.UserId == dummyUserId);
            Assert.IsNotNull(remoteProjectUser);
            Assert.AreEqual(InternalRoutines.PROJECT_ROLE_TESTER, remoteProjectUser.ProjectRoleId);

            //Change their role
            remoteProjectUser.ProjectRoleId = InternalRoutines.PROJECT_ROLE_DEVELOPER;
            spiraSoapClient.Project_UpdateUserMembership(credentials, projectId1, remoteProjectUser);

            //Verify
            remoteProjectUsers = spiraSoapClient.Project_RetrieveUserMembership(credentials, projectId1);
            remoteProjectUser = remoteProjectUsers.FirstOrDefault(p => p.UserId == dummyUserId);
            Assert.IsNotNull(remoteProjectUser);
            Assert.AreEqual(InternalRoutines.PROJECT_ROLE_DEVELOPER, remoteProjectUser.ProjectRoleId);

            //Remove their membership
            spiraSoapClient.Project_RemoveUserMembership(credentials, projectId1, dummyUserId);

            //Verify
            remoteProjectUsers = spiraSoapClient.Project_RetrieveUserMembership(credentials, projectId1);
            remoteProjectUser = remoteProjectUsers.FirstOrDefault(p => p.UserId == dummyUserId);
            Assert.IsNull(remoteProjectUser);

            //Test that we can disable a user (used to delete)
            spiraSoapClient.User_Delete(credentials, dummyUserId);

            //Verify
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, dummyUserId);
            Assert.IsFalse(remoteUser.Active);

            //Only makes it inactive now, so do the delete using business method
            new UserManager().DeleteUser(dummyUserId, true);

            //Verify
            bool errorThrown = false;
            try
            {
                remoteUser = spiraSoapClient.User_RetrieveById(credentials, dummyUserId);
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);
        }

        /// <summary>
        /// Verifies that you can import custom properties and custom lists into the system
        /// </summary>
        [
        Test,
        SpiraTestCase(1988)
        ]
        public void _05_ImportCustomProperties()
        {
            //First lets authenticate and connect to the new project as an administrator
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Lets see if we have any requirement custom properties
            RemoteCustomProperty[] remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "Requirement");
            Assert.AreEqual(0, remoteCustomProperties.Length);

            //Now add a new custom list to the project with one value
            RemoteCustomListValue remoteCustomListValue = new RemoteCustomListValue();
            remoteCustomListValue.Name = "Feature";
            RemoteCustomList remoteCustomList = new RemoteCustomList();
            remoteCustomList.ProjectTemplateId = projectTemplateId1;
            remoteCustomList.Name = "Req Types";
            remoteCustomList.Active = true;
            remoteCustomList.Values = new List<RemoteCustomListValue>() { remoteCustomListValue };
            customListId1 = spiraSoapClient.CustomProperty_AddCustomList(credentials, projectTemplateId1, remoteCustomList).CustomPropertyListId.Value;

            //Now add another value to the existing custom list
            remoteCustomListValue = new RemoteCustomListValue();
            remoteCustomListValue.CustomPropertyListId = customListId1;
            remoteCustomListValue.Name = "Technical Quality";
            customValueId2 = spiraSoapClient.CustomProperty_AddCustomListValue(credentials, projectTemplateId1, customListId1, remoteCustomListValue).CustomPropertyValueId.Value;

            //Lets verify that the list was correctly populated
            remoteCustomList = spiraSoapClient.CustomProperty_RetrieveCustomListById(credentials, projectTemplateId1, customListId1);
            Assert.AreEqual("Req Types", remoteCustomList.Name);
            Assert.AreEqual(true, remoteCustomList.Active);
            Assert.AreEqual(2, remoteCustomList.Values.Count);
            //Value 1
            Assert.AreEqual("Feature", remoteCustomList.Values[0].Name);
            //Value 2
            Assert.AreEqual("Technical Quality", remoteCustomList.Values[1].Name);

            //Now lets add a second list with values
            remoteCustomListValue = new RemoteCustomListValue();
            remoteCustomListValue.Name = "Component One";
            remoteCustomList = new RemoteCustomList();
            remoteCustomList.ProjectTemplateId = projectTemplateId1;
            remoteCustomList.Name = "Components";
            remoteCustomList.Active = true;
            remoteCustomList.Values = new List<RemoteCustomListValue>() { remoteCustomListValue };
            customListId2 = spiraSoapClient.CustomProperty_AddCustomList(credentials, projectTemplateId1, remoteCustomList).CustomPropertyListId.Value;

            //Now lets verify that we can retrieve all the lists in the project
            RemoteCustomList[] remoteCustomLists = spiraSoapClient.CustomProperty_RetrieveCustomLists(credentials, projectTemplateId1);
            Assert.AreEqual(2, remoteCustomLists.Length);
            Assert.AreEqual("Req Types", remoteCustomLists[0].Name);
            Assert.AreEqual(true, remoteCustomLists[0].Active);
            Assert.AreEqual("Components", remoteCustomLists[1].Name);
            Assert.AreEqual(true, remoteCustomLists[1].Active);

            //Verify that we can modify a custom list / custom list value
            remoteCustomList = spiraSoapClient.CustomProperty_RetrieveCustomListById(credentials, projectTemplateId1, customListId2);
            remoteCustomList.Name = "Component Names";
            remoteCustomList.Values[0].Name = "Component One";
            spiraSoapClient.CustomProperty_UpdateCustomList(credentials, projectTemplateId1, customListId2, remoteCustomList);

            //Verify the changes
            remoteCustomList = spiraSoapClient.CustomProperty_RetrieveCustomListById(credentials, projectTemplateId1, customListId2);
            Assert.AreEqual("Component Names", remoteCustomList.Name);
            Assert.AreEqual(true, remoteCustomList.Active);
            Assert.AreEqual(1, remoteCustomList.Values.Count);
            Assert.AreEqual("Component One", remoteCustomList.Values[0].Name);
            customValueId3 = remoteCustomList.Values[0].CustomPropertyValueId.Value;

            //Get a reference to one of the lists
            remoteCustomList = spiraSoapClient.CustomProperty_RetrieveCustomListById(credentials, projectTemplateId1, customListId1);

            //Now lets add a text custom property and one list value to both requirements and incidents

            //Text Property
            RemoteCustomProperty remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 1;
            remoteCustomProperty.CustomPropertyTypeId = 1;  //Text
            remoteCustomProperty.Name = "Source";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //List Property
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 2;
            remoteCustomProperty.CustomPropertyTypeId = 6;  //List
            remoteCustomProperty.Name = "Req Type";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, remoteCustomList.CustomPropertyListId, remoteCustomProperty);

            //Verify the additions
            remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "Requirement");
            Assert.AreEqual(2, remoteCustomProperties.Length);
            Assert.AreEqual("Custom_01", remoteCustomProperties.FirstOrDefault(c => c.Name == "Source").CustomPropertyFieldName);
            Assert.AreEqual("Custom_02", remoteCustomProperties.FirstOrDefault(c => c.Name == "Req Type").CustomPropertyFieldName);
            Assert.AreEqual(1, remoteCustomProperties.FirstOrDefault(c => c.Name == "Source").CustomPropertyTypeId);
            Assert.AreEqual(6, remoteCustomProperties.FirstOrDefault(c => c.Name == "Req Type").CustomPropertyTypeId);

            //Now add the same two custom properties to incidents

            //Text Property
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 1;
            remoteCustomProperty.CustomPropertyTypeId = 1;  //Text
            remoteCustomProperty.Name = "Source";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //List Property
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 2;
            remoteCustomProperty.CustomPropertyTypeId = 6;  //List
            remoteCustomProperty.Name = "Req Type";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, remoteCustomList.CustomPropertyListId, remoteCustomProperty);

            //Verify the additions
            remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "Incident");
            Assert.AreEqual(2, remoteCustomProperties.Length);
            Assert.AreEqual("Custom_01", remoteCustomProperties.FirstOrDefault(c => c.Name == "Source").CustomPropertyFieldName);
            Assert.AreEqual("Custom_02", remoteCustomProperties.FirstOrDefault(c => c.Name == "Req Type").CustomPropertyFieldName);
            Assert.AreEqual(1, remoteCustomProperties.FirstOrDefault(c => c.Name == "Source").CustomPropertyTypeId);
            Assert.AreEqual(6, remoteCustomProperties.FirstOrDefault(c => c.Name == "Req Type").CustomPropertyTypeId);

            //Also verify that the values of the custom list are returned as well
            remoteCustomList = remoteCustomProperties[1].CustomList;
            Assert.AreEqual("Req Types", remoteCustomList.Name);
            Assert.AreEqual(true, remoteCustomList.Active);
            Assert.AreEqual(2, remoteCustomList.Values.Count);
            //Value 1
            Assert.AreEqual("Feature", remoteCustomList.Values[0].Name);
            //Value 2
            Assert.AreEqual("Technical Quality", remoteCustomList.Values[1].Name);

            //Now we need to test some of the newer custom property types available in v4.0 as well as the
            //ability to set the different options on the custom properties. We'll add them to tasks for this test
            //Integer
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Integer;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 1;
            remoteCustomProperty.Name = "Integer";
            List<RemoteCustomPropertyOption> options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MinValue, Value = "5" });
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MaxValue, Value = "20" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //Decimal
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Decimal;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 2;
            remoteCustomProperty.Name = "Decimal";
            options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MinValue, Value = "5" });
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MaxValue, Value = "20" });
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Precision, Value = "1" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //Boolean
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Boolean;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 3;
            remoteCustomProperty.Name = "Boolean";
            options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Default, Value = "true" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //Date
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.Date;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 4;
            remoteCustomProperty.Name = "Date";
            options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.AllowEmpty, Value = "false" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //MultiList
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.MultiList;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 5;
            remoteCustomProperty.Name = "MultiList";
            options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.AllowEmpty, Value = "true" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //User
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.User;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 6;
            remoteCustomProperty.Name = "User";
            options = new List<RemoteCustomPropertyOption>();
            options.Add(new RemoteCustomPropertyOption() { CustomPropertyOptionId = (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Default, Value = "1" });
            remoteCustomProperty.Options = options;
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, null, remoteCustomProperty);

            //Verify the custom properties were added correctly
            remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "Task");
            Assert.AreEqual(6, remoteCustomProperties.Length);
            //Integer
            Assert.AreEqual("Integer", remoteCustomProperties[0].Name);
            Assert.AreEqual("Integer", remoteCustomProperties[0].CustomPropertyTypeName);
            Assert.AreEqual(2, remoteCustomProperties[0].Options.Count);
            Assert.IsTrue(remoteCustomProperties[0].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MinValue && o.Value == "5"));
            Assert.IsTrue(remoteCustomProperties[0].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MaxValue && o.Value == "20"));
            //Decimal
            Assert.AreEqual("Decimal", remoteCustomProperties[1].Name);
            Assert.AreEqual("Decimal", remoteCustomProperties[1].CustomPropertyTypeName);
            Assert.AreEqual(3, remoteCustomProperties[1].Options.Count);
            Assert.IsTrue(remoteCustomProperties[1].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MinValue && o.Value == "5"));
            Assert.IsTrue(remoteCustomProperties[1].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.MaxValue && o.Value == "20"));
            Assert.IsTrue(remoteCustomProperties[1].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Precision && o.Value == "1"));
            //Boolean
            Assert.AreEqual("Boolean", remoteCustomProperties[2].Name);
            Assert.AreEqual("Boolean", remoteCustomProperties[2].CustomPropertyTypeName);
            Assert.AreEqual(1, remoteCustomProperties[2].Options.Count);
            Assert.IsTrue(remoteCustomProperties[2].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Default && o.Value == "true"));
            //Date
            Assert.AreEqual("Date", remoteCustomProperties[3].Name);
            Assert.AreEqual("Date", remoteCustomProperties[3].CustomPropertyTypeName);
            Assert.AreEqual(1, remoteCustomProperties[3].Options.Count);
            Assert.IsTrue(remoteCustomProperties[3].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.AllowEmpty && o.Value == "false"));
            //MultiList
            Assert.AreEqual("MultiList", remoteCustomProperties[4].Name);
            Assert.AreEqual("MultiList", remoteCustomProperties[4].CustomPropertyTypeName);
            Assert.AreEqual(1, remoteCustomProperties[4].Options.Count);
            Assert.IsTrue(remoteCustomProperties[4].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.AllowEmpty && o.Value == "true"));
            //User
            Assert.AreEqual("User", remoteCustomProperties[5].Name);
            Assert.AreEqual("User", remoteCustomProperties[5].CustomPropertyTypeName);
            Assert.AreEqual(1, remoteCustomProperties[5].Options.Count);
            Assert.IsTrue(remoteCustomProperties[5].Options.Any(o => o.CustomPropertyOptionId == (int)DataModel.CustomProperty.CustomPropertyOptionEnum.Default && o.Value == "1"));

            //Now we need to remove the custom properties and options since they will cause task insert/updates to fail
            foreach (RemoteCustomProperty remoteCustomProperty2 in remoteCustomProperties)
            {
                spiraSoapClient.CustomProperty_DeleteDefinition(credentials, projectTemplateId1, remoteCustomProperty2.CustomPropertyId.Value);
            }

            //We need to add a list to both test sets and test runs so that we can verify
            //that setting a custom property on an automated test set will propagate down to the test runs
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.List;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestSet;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 1;
            remoteCustomProperty.Name = "Component";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, customListId2, remoteCustomProperty);
            remoteCustomProperty = new RemoteCustomProperty();
            remoteCustomProperty.CustomPropertyTypeId = (int)DataModel.CustomProperty.CustomPropertyTypeEnum.List;
            remoteCustomProperty.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestRun;
            remoteCustomProperty.ProjectTemplateId = projectTemplateId1;
            remoteCustomProperty.PropertyNumber = 2;
            remoteCustomProperty.Name = "Component";
            spiraSoapClient.CustomProperty_AddDefinition(credentials, projectTemplateId1, customListId2, remoteCustomProperty);

            //Verify that they both now have one custom property each
            remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "TestSet");
            Assert.AreEqual(1, remoteCustomProperties.Length);
            remoteCustomProperties = spiraSoapClient.CustomProperty_RetrieveForArtifactType(credentials, projectTemplateId1, "TestRun");
            Assert.AreEqual(1, remoteCustomProperties.Length);
        }

        /// <summary>
        /// Verifies that you can import custom properties and custom lists into the system
        /// </summary>
        [
        Test,
        SpiraTestCase(1986)
        ]
        public void _06_ImportComponents()
        {
            //First lets authenticate and connect to the new project as an administrator
            credentials = credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Next lets verify we don't have any components at the start
            RemoteComponent[] components = spiraSoapClient.Component_Retrieve(credentials, projectId1, true, false);
            Assert.AreEqual(0, components.Length);

            //Now lets add some components
            componentId1 = spiraSoapClient.Component_Create(credentials, projectId1, new RemoteComponent() { Name = "Component 1", IsActive = true, ProjectId = projectId1 }).ComponentId.Value;
            componentId2 = spiraSoapClient.Component_Create(credentials, projectId1, new RemoteComponent() { Name = "Component 2", IsActive = false, ProjectId = projectId1 }).ComponentId.Value;
            componentId3 = spiraSoapClient.Component_Create(credentials, projectId1, new RemoteComponent() { Name = "Component 3", IsActive = true, ProjectId = projectId1 }).ComponentId.Value;

            //Verify they added
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, true, false);
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId3, components[1].ComponentId);
            Assert.AreEqual("Component 3", components[1].Name);

            //Try the other retrieve options
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, false, false);
            Assert.AreEqual(3, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId2, components[1].ComponentId);
            Assert.AreEqual("Component 2", components[1].Name);
            Assert.AreEqual(projectId1, components[2].ProjectId);
            Assert.AreEqual(componentId3, components[2].ComponentId);
            Assert.AreEqual("Component 3", components[2].Name);

            //Try the other retrieve options
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, false, true);
            Assert.AreEqual(3, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId2, components[1].ComponentId);
            Assert.AreEqual("Component 2", components[1].Name);
            Assert.AreEqual(projectId1, components[2].ProjectId);
            Assert.AreEqual(componentId3, components[2].ComponentId);
            Assert.AreEqual("Component 3", components[2].Name);

            //Make a component active and change name
            RemoteComponent component = spiraSoapClient.Component_RetrieveById(credentials, projectId1, componentId2);
            component.IsActive = true;
            component.Name = "Component 2b";
            spiraSoapClient.Component_Update(credentials, projectId1, componentId2, component);

            //Verify the change
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, true, false);
            Assert.AreEqual(3, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId2, components[1].ComponentId);
            Assert.AreEqual("Component 2b", components[1].Name);
            Assert.AreEqual(projectId1, components[2].ProjectId);
            Assert.AreEqual(componentId3, components[2].ComponentId);
            Assert.AreEqual("Component 3", components[2].Name);

            //Now delete a component
            spiraSoapClient.Component_Delete(credentials, projectId1, componentId2);

            //Verify the delete
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, false, false);
            Assert.AreEqual(2, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId3, components[1].ComponentId);
            Assert.AreEqual("Component 3", components[1].Name);

            //Now undelete the components
            spiraSoapClient.Component_Undelete(credentials, projectId1, componentId2);

            //Verify
            components = spiraSoapClient.Component_Retrieve(credentials, projectId1, true, false);
            Assert.AreEqual(3, components.Length);
            Assert.AreEqual(projectId1, components[0].ProjectId);
            Assert.AreEqual(componentId1, components[0].ComponentId);
            Assert.AreEqual("Component 1", components[0].Name);
            Assert.AreEqual(projectId1, components[1].ProjectId);
            Assert.AreEqual(componentId2, components[1].ComponentId);
            Assert.AreEqual("Component 2b", components[1].Name);
            Assert.AreEqual(projectId1, components[2].ProjectId);
            Assert.AreEqual(componentId3, components[2].ComponentId);
            Assert.AreEqual("Component 3", components[2].Name);
        }

        [
        Test,
        SpiraTestCase(1991)
        ]
        public void _07_ImportReleases()
        {
            //First lets authenticate and connect to the project as one of our new users
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Next create a release and two child iterations
            //Release
            RemoteRelease remoteRelease = new RemoteRelease();
            remoteRelease.ProjectId = projectId1;
            remoteRelease.Name = "Release 1";
            remoteRelease.VersionNumber = "1.0";
            remoteRelease.Description = "First version of the system";
            remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
            remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MajorRelease;
            remoteRelease.StartDate = DateTime.UtcNow;
            remoteRelease.EndDate = DateTime.UtcNow.AddMonths(3);
            remoteRelease.ResourceCount = 5;
			remoteRelease.OwnerId = userId2;
			releaseId1 = spiraSoapClient.Release_Create1(credentials, projectId1, remoteRelease).ReleaseId.Value;
            //Iteration #1
            remoteRelease = new RemoteRelease();
            remoteRelease.ProjectId = projectId1;
            remoteRelease.Name = "Sprint 1";
            remoteRelease.VersionNumber = "1.0.001";
            remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
            remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.Iteration;
            remoteRelease.StartDate = DateTime.UtcNow;
            remoteRelease.EndDate = DateTime.UtcNow.AddMonths(1);
            remoteRelease.ResourceCount = 5;
            iterationId1 = spiraSoapClient.Release_Create2(credentials, projectId1, releaseId1, remoteRelease).ReleaseId.Value;
            //Iteration #2
            remoteRelease = new RemoteRelease();
            remoteRelease.ProjectId = projectId1;
            remoteRelease.Name = "Sprint 2";
            remoteRelease.VersionNumber = "1.0.002";
            remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
            remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.Iteration;
            remoteRelease.StartDate = DateTime.UtcNow.AddMonths(1);
            remoteRelease.EndDate = DateTime.UtcNow.AddMonths(2);
            remoteRelease.ResourceCount = 5;
            iterationId2 = spiraSoapClient.Release_Create2(credentials, projectId1, releaseId1, remoteRelease).ReleaseId.Value;

            //Now verify that the releases/iterations inserted correctly
            //First retrieve the release
            remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId1);
            Assert.AreEqual(4, remoteRelease.ArtifactTypeId);
            Assert.AreEqual("Release 1", remoteRelease.Name);
            Assert.AreEqual("AAA", remoteRelease.IndentLevel);
            Assert.AreEqual("1.0", remoteRelease.VersionNumber);
            Assert.AreEqual("First version of the system", remoteRelease.Description);
            Assert.AreEqual("Major Release", remoteRelease.ReleaseTypeName);
            Assert.AreEqual(userId1, remoteRelease.CreatorId);
            Assert.AreEqual(userId2, remoteRelease.OwnerId);
            Assert.AreEqual(true, remoteRelease.Active);
            Assert.IsTrue(remoteRelease.PlannedEffort > 0);

            //Now retrieve one of the iterations
            remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId1);
            Assert.AreEqual(4, remoteRelease.ArtifactTypeId);
            Assert.AreEqual("1.0.001", remoteRelease.VersionNumber);
            Assert.AreEqual("AAAAAA", remoteRelease.IndentLevel);
            Assert.IsNull(remoteRelease.Description);
            Assert.AreEqual("Sprint", remoteRelease.ReleaseTypeName);
            Assert.AreEqual(userId1, remoteRelease.CreatorId);
            Assert.AreEqual(true, remoteRelease.Active);
            Assert.IsTrue(remoteRelease.PlannedEffort > 0);

            //Now make an update to one of the iterations
            remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId2);
            remoteRelease.VersionNumber = "1.0.002b";
            remoteRelease.Description = "Second Sprint";
            spiraSoapClient.Release_Update(credentials, projectId1, remoteRelease);

            //Verify the change
            remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId2);
            Assert.AreEqual("1.0.002b", remoteRelease.VersionNumber);
            Assert.AreEqual("Second Sprint", remoteRelease.Description);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update a status
			remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId1);
			remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Completed;
			spiraSoapClient.Release_Update(credentials, projectId1, remoteRelease);

			//Verify that the status did not change
			remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId1);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, remoteRelease.ReleaseStatusId, "ReleaseStatusId");

			//Try adding an artifact with a non default status
			remoteRelease = new RemoteRelease();
			remoteRelease.ProjectId = projectId1;
			remoteRelease.Name = "Sprint 1";
			remoteRelease.VersionNumber = "1.0.001";
			remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
			remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.Iteration;
			remoteRelease.StartDate = DateTime.UtcNow;
			remoteRelease.EndDate = DateTime.UtcNow.AddMonths(1);
			remoteRelease.ResourceCount = 5;
			remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Completed;
			int iterationId3 = spiraSoapClient.Release_Create1(credentials, projectId1, remoteRelease).ReleaseId.Value;

			//Verify that the status did not change
			remoteRelease = spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId3);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, remoteRelease.ReleaseStatusId, "ReleaseStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			//NOTE: do not delete iterationId3 because this has a side effect in _39_ with Release_Move 
		}

		/// <summary>
		/// Verifies that you can import new requirements into the system
		/// </summary>
		[
        Test,
        SpiraTestCase(1992)
        ]
        public void _08_ImportRequirements()
        {
            int rq_importanceCriticalId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 1).ImportanceId;
            int rq_importanceHighId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 2).ImportanceId;
            int rq_importanceMediumId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId1).FirstOrDefault(i => i.Score == 3).ImportanceId;
            int rq_typeFeatureId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Feature").RequirementTypeId;
            int rq_typeUserStoryId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "User Story").RequirementTypeId;
            int rq_typeQualityId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Quality").RequirementTypeId;
            int rq_typeUseCaseId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Use Case").RequirementTypeId;
            int rq_typeNeedId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Need").RequirementTypeId;

            //First lets authenticate and connect to the project as one of our new users
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets add a nested tree of requirements
            //First the summary item
            RemoteRequirement remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = 1;
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement.Name = "Functionality Area";
            remoteRequirement.Description = String.Empty;
            remoteRequirement.AuthorId = userId1;
            remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
            requirementId1 = remoteRequirement.RequirementId.Value;
            //Detail Item 1
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = 2;
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement.ImportanceId = rq_importanceCriticalId;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.ComponentId = componentId1;
            remoteRequirement.Name = "Requirement 1";
            remoteRequirement.Description = "Requirement Description 1";
            remoteRequirement.AuthorId = userId1;
            //Add some custom property values
            remoteRequirement.CustomProperties = new List<RemoteArtifactCustomProperty>();
            remoteRequirement.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 1, StringValue = "test value1" });
            remoteRequirement.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 2, IntegerValue = customValueId2 });
            remoteRequirement = spiraSoapClient.Requirement_Create2(credentials, projectId1, 1, remoteRequirement);
            requirementId2 = remoteRequirement.RequirementId.Value;
            //Detail Item 2
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.RequirementTypeId = rq_typeUserStoryId;
            remoteRequirement.StatusId = 1;
            remoteRequirement.ImportanceId = rq_importanceHighId;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.ComponentId = componentId2;
            remoteRequirement.Name = "Requirement 2";
            remoteRequirement.Description = "Requirement Description 2";
            remoteRequirement.AuthorId = userId1;
            //Add some custom property values
            remoteRequirement.CustomProperties = new List<RemoteArtifactCustomProperty>();
            remoteRequirement.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 1, StringValue = "test value2" });
            remoteRequirement.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 2, IntegerValue = customValueId1 });
            remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
            requirementId3 = remoteRequirement.RequirementId.Value;
            //Detail Item 3
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = 4;
            remoteRequirement.ImportanceId = rq_importanceHighId;
            remoteRequirement.RequirementTypeId = rq_typeQualityId;
            remoteRequirement.Name = "Requirement 3";
            remoteRequirement.Description = "Requirement Description 3";
            remoteRequirement.AuthorId = userId2;
            remoteRequirement.ComponentId = componentId3;
            remoteRequirement = spiraSoapClient.Requirement_Create2(credentials, projectId1, 1, remoteRequirement);
            requirementId4 = remoteRequirement.RequirementId.Value;
            //Detail Item 4 - this one has scenario steps
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.StatusId = 3;
            remoteRequirement.RequirementTypeId = rq_typeUseCaseId;
            remoteRequirement.ImportanceId = rq_importanceMediumId;
            remoteRequirement.Name = "Requirement 4";
            remoteRequirement.Description = String.Empty;
            remoteRequirement.AuthorId = userId2;
            remoteRequirement = spiraSoapClient.Requirement_Create2(credentials, projectId1, -1, remoteRequirement);
            requirementId5 = remoteRequirement.RequirementId.Value;

            //Now add some steps to this one
            RemoteRequirementStep remoteRequirementStep = new RemoteRequirementStep();
            remoteRequirementStep.RequirementId = requirementId5;
            remoteRequirementStep.Description = "Requirement 4 Step 1";
            remoteRequirementStep.CreationDate = DateTime.UtcNow;
            spiraSoapClient.Requirement_AddStep(credentials, projectId1, requirementId5, null, null, remoteRequirementStep);
            remoteRequirementStep = new RemoteRequirementStep();
            remoteRequirementStep.RequirementId = requirementId5;
            remoteRequirementStep.Description = "Requirement 4 Step 3";
            remoteRequirementStep.CreationDate = DateTime.UtcNow;
            remoteRequirementStep = spiraSoapClient.Requirement_AddStep(credentials, projectId1, requirementId5, null, USER_ID_FRED_BLOGGS, remoteRequirementStep);
            int requirementStepId = remoteRequirementStep.RequirementStepId.Value;
            remoteRequirementStep = new RemoteRequirementStep();
            remoteRequirementStep.RequirementId = requirementId5;
            remoteRequirementStep.Description = "Requirement 4 Step 2";
            remoteRequirementStep.CreationDate = DateTime.UtcNow;
            spiraSoapClient.Requirement_AddStep(credentials, projectId1, requirementId5, requirementStepId, null, remoteRequirementStep);

            //First retrieve the parent requirement
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId1);
            Assert.AreEqual(1, remoteRequirement.ArtifactTypeId);
            Assert.AreEqual("Functionality Area", remoteRequirement.Name);
            Assert.AreEqual("AAA", remoteRequirement.IndentLevel);
            Assert.AreEqual((int)Requirement.RequirementStatusEnum.InProgress, remoteRequirement.StatusId);
            Assert.AreEqual(userId1, remoteRequirement.AuthorId);
            Assert.IsNull(remoteRequirement.ImportanceId);

            //Now retrieve a first-level indent requirement
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId3);
            Assert.AreEqual(1, remoteRequirement.ArtifactTypeId);
            Assert.AreEqual("Requirement 2", remoteRequirement.Name);
            Assert.AreEqual("Requirement Description 2", remoteRequirement.Description);
            Assert.AreEqual("AAAAAB", remoteRequirement.IndentLevel);
            Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId);
            Assert.AreEqual(userId1, remoteRequirement.AuthorId);
            Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);
            Assert.AreEqual("test value2", remoteRequirement.CustomProperties.FirstOrDefault(p => p.PropertyNumber == 1).StringValue);
            Assert.AreEqual(customValueId1, remoteRequirement.CustomProperties.FirstOrDefault(p => p.PropertyNumber == 2).IntegerValue);
            Assert.AreEqual(componentId2, remoteRequirement.ComponentId);

            //Now retrieve a second-level indent requirement
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId4);
            Assert.AreEqual(1, remoteRequirement.ArtifactTypeId);
            Assert.AreEqual("Requirement 3", remoteRequirement.Name);
            Assert.AreEqual("AAAAABAAA", remoteRequirement.IndentLevel);
            Assert.AreEqual("Requirement Description 3", remoteRequirement.Description);
            Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId);
            Assert.AreEqual(userId2, remoteRequirement.AuthorId);
            Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);

            //Now test that we can insert a requirement using the 'insert under parent' method
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.StatusId = 3;
            remoteRequirement.ImportanceId = rq_importanceCriticalId;
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement.Name = "Test Child 1";
            remoteRequirement.Description = String.Empty;
            remoteRequirement.AuthorId = userId1;
            remoteRequirement = spiraSoapClient.Requirement_Create3(credentials, projectId1, requirementId5, remoteRequirement);
            int requirementId6 = remoteRequirement.RequirementId.Value;
            //Verify insertion data
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId6);
            Assert.AreEqual("Test Child 1", remoteRequirement.Name);
            //Verify that parent became a summary
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId5);
            Assert.AreEqual("Requirement 4", remoteRequirement.Name);
            Assert.AreEqual(true, remoteRequirement.Summary);

            //Now test that we can insert a requirement under the same folder as an existing one
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.RequirementTypeId = rq_typeNeedId;
            remoteRequirement.StatusId = 3;
            remoteRequirement.ImportanceId = rq_importanceCriticalId;
            remoteRequirement.Name = "Test Child 2";
            remoteRequirement.Description = "Test Child Description 2";
            remoteRequirement.AuthorId = userId1;
            remoteRequirement = spiraSoapClient.Requirement_Create3(credentials, projectId1, requirementId5, remoteRequirement);
            int requirementId7 = remoteRequirement.RequirementId.Value;
            //Verify insertion data
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId7);
            Assert.AreEqual("AAAAACAAB", remoteRequirement.IndentLevel);
            Assert.AreEqual("Test Child 2", remoteRequirement.Name);

            //Now make an update to one of the requirements
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId7);
            remoteRequirement.StatusId = 4;
            remoteRequirement.ImportanceId = rq_importanceHighId;
            remoteRequirement.Name = "Test Child 2a";
            remoteRequirement.Description = "Test Child Description 2a";
            spiraSoapClient.Requirement_Update(credentials, projectId1, remoteRequirement);

            //Verify the change
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId7);
            Assert.AreEqual(4, remoteRequirement.StatusId);
            Assert.AreEqual(rq_importanceHighId, remoteRequirement.ImportanceId);
            Assert.AreEqual("Test Child 2a", remoteRequirement.Name);
            Assert.AreEqual("Test Child Description 2a", remoteRequirement.Description);

            //Verify that you can indent a requirement
            spiraSoapClient.Requirement_Indent(credentials, projectId1, requirementId3);
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId3);
            Assert.AreEqual("AAAAAAAAA", remoteRequirement.IndentLevel);
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2);
            Assert.AreEqual(true, remoteRequirement.Summary);

            //Now put it back
            spiraSoapClient.Requirement_Outdent(credentials, projectId1, requirementId3);
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId3);
            Assert.AreEqual("AAAAAB", remoteRequirement.IndentLevel);
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2);
            Assert.AreEqual(false, remoteRequirement.Summary);

            //Now verify the requirement steps
            remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId5);
            Assert.AreEqual(3, remoteRequirement.Steps.Count);
            Assert.AreEqual("Requirement 4 Step 1", remoteRequirement.Steps[0].Description);
            Assert.AreEqual("Requirement 4 Step 2", remoteRequirement.Steps[1].Description);
            Assert.AreEqual("Requirement 4 Step 3", remoteRequirement.Steps[2].Description);

            //Try using the step functions
            RemoteRequirementStep[] requirementSteps = spiraSoapClient.Requirement_RetrieveSteps(credentials, projectId1, requirementId5);
            Assert.AreEqual(3, requirementSteps.Length);
            Assert.AreEqual("Requirement 4 Step 1", requirementSteps[0].Description);
            Assert.AreEqual("Requirement 4 Step 2", requirementSteps[1].Description);
            Assert.AreEqual("Requirement 4 Step 3", requirementSteps[2].Description);
            requirementStepId = requirementSteps[0].RequirementStepId.Value;

            //Verify we can update one step
            remoteRequirementStep = spiraSoapClient.Requirement_RetrieveStepById(credentials, projectId1, requirementId5, requirementStepId);
            remoteRequirementStep.Description = "Requirement 4 Step 1A";
            spiraSoapClient.Requirement_UpdateStep(credentials, projectId1, requirementId5, remoteRequirementStep);

            //Verify the change
            remoteRequirementStep = spiraSoapClient.Requirement_RetrieveStepById(credentials, projectId1, requirementId5, requirementStepId);
            Assert.AreEqual("Requirement 4 Step 1A", remoteRequirementStep.Description);

            //Move a step to the end
            spiraSoapClient.Requirement_MoveStep(credentials, projectId1, requirementId5, requirementStepId, null);

            //Verify the move
            requirementSteps = spiraSoapClient.Requirement_RetrieveSteps(credentials, projectId1, requirementId5);
            Assert.AreEqual(3, requirementSteps.Length);
            Assert.AreEqual("Requirement 4 Step 2", requirementSteps[0].Description);
            Assert.AreEqual("Requirement 4 Step 3", requirementSteps[1].Description);
            Assert.AreEqual("Requirement 4 Step 1A", requirementSteps[2].Description);

            //Now move it back
            spiraSoapClient.Requirement_MoveStep(credentials, projectId1, requirementId5, requirementStepId, requirementSteps[0].RequirementStepId.Value);

            //Verify the move
            requirementSteps = spiraSoapClient.Requirement_RetrieveSteps(credentials, projectId1, requirementId5);
            Assert.AreEqual(3, requirementSteps.Length);
            Assert.AreEqual("Requirement 4 Step 1A", requirementSteps[0].Description);
            Assert.AreEqual("Requirement 4 Step 2", requirementSteps[1].Description);
            Assert.AreEqual("Requirement 4 Step 3", requirementSteps[2].Description);

            //Finally delete a step
            spiraSoapClient.Requirement_DeleteStep(credentials, projectId1, requirementId5, requirementSteps[1].RequirementStepId.Value);

            //Verify the delete
            requirementSteps = spiraSoapClient.Requirement_RetrieveSteps(credentials, projectId1, requirementId5);
            Assert.AreEqual(2, requirementSteps.Length);
            Assert.AreEqual("Requirement 4 Step 1A", requirementSteps[0].Description);
            Assert.AreEqual("Requirement 4 Step 3", requirementSteps[1].Description);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update a status
			remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId4);
			remoteRequirement.StatusId = (int)Requirement.RequirementStatusEnum.Completed;
			spiraSoapClient.Requirement_Update(credentials, projectId1, remoteRequirement);

			//Verify that the status did not change
			remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId4);
			Assert.AreEqual((int)Requirement.RequirementStatusEnum.Developed, remoteRequirement.StatusId, "RequirementStatusId");

			//Try adding an artifact with a non default status
			remoteRequirement = new RemoteRequirement();
			remoteRequirement.ProjectId = projectId1;
			remoteRequirement.StatusId = (int)Requirement.RequirementStatusEnum.Developed;
			remoteRequirement.ImportanceId = rq_importanceHighId;
			remoteRequirement.RequirementTypeId = rq_typeQualityId;
			remoteRequirement.Name = "Requirement 3";
			remoteRequirement.Description = "Requirement Description 3";
			remoteRequirement.AuthorId = userId2;
			remoteRequirement.ComponentId = componentId3;
			remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
			int requirementId8 = remoteRequirement.RequirementId.Value;

			//Verify that the status did not change
			remoteRequirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId8);
			Assert.AreEqual((int)Release.ReleaseStatusEnum.Planned, remoteRequirement.StatusId, "RequirementStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			spiraSoapClient.Requirement_Delete(credentials, projectId1, requirementId8);
		}

        /// <summary>
        /// Verifies that you can import new test folders and test cases into the system
        /// </summary>
        [
        Test,
        SpiraTestCase(1995)
        ]
        public void _09_ImportTestCases()
        {
            int tc_typeFunctionalId = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Functional").TestCaseTypeId;
            int tc_typeRegressionId = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Regression").TestCaseTypeId;
            int tc_typeScenarioId = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Scenario").TestCaseTypeId;

            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets add two nested test folders with two test cases in each
            //Folder A
            RemoteTestCaseFolder remoteTestCaseFolder = new RemoteTestCaseFolder();
            remoteTestCaseFolder.Name = "Test Folder A";
            remoteTestCaseFolder.ProjectId = projectId1;
            remoteTestCaseFolder = spiraSoapClient.TestCase_CreateFolder(credentials, projectId1, remoteTestCaseFolder);
            testFolderId1 = remoteTestCaseFolder.TestCaseFolderId.Value;
            //Folder B
            remoteTestCaseFolder = new RemoteTestCaseFolder();
            remoteTestCaseFolder.Name = "Test Folder B";
            remoteTestCaseFolder.ProjectId = projectId1;
            remoteTestCaseFolder.ParentTestCaseFolderId = testFolderId1;
            remoteTestCaseFolder = spiraSoapClient.TestCase_CreateFolder(credentials, projectId1, remoteTestCaseFolder);
            testFolderId2 = remoteTestCaseFolder.TestCaseFolderId.Value;
            //Test Case 1
            RemoteTestCase remoteTestCase = new RemoteTestCase();
            remoteTestCase.Name = "Test Case 1";
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.Description = "Test Case Description 1";
            remoteTestCase.TestCaseFolderId = testFolderId1;
            remoteTestCase.TestCaseTypeId = tc_typeFunctionalId;
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Draft;
            remoteTestCase.TestCasePriorityId = 1;
            remoteTestCase.EstimatedDuration = 30;
            remoteTestCase.ComponentIds = new List<int>() { componentId2, componentId3 };
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            testCaseId1 = remoteTestCase.TestCaseId.Value;
            //Test Case 2
            remoteTestCase = new RemoteTestCase();
            remoteTestCase.Name = "Test Case 2";
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.Description = "Test Case Description 2";
            remoteTestCase.TestCaseFolderId = testFolderId1;
            remoteTestCase.AuthorId = userId1;
            remoteTestCase.OwnerId = userId2;
            remoteTestCase.TestCaseTypeId = tc_typeRegressionId;
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.ReadyForTest;
            remoteTestCase.TestCasePriorityId = 2;
            remoteTestCase.EstimatedDuration = 25;
            remoteTestCase.ComponentIds = new List<int>() { componentId1, componentId2 };
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            testCaseId2 = remoteTestCase.TestCaseId.Value;
            //Test Case 3
            remoteTestCase = new RemoteTestCase();
            remoteTestCase.Name = "Test Case 3";
            remoteTestCase.Description = "Test Case Description 3";
            remoteTestCase.TestCaseFolderId = testFolderId2;
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.AuthorId = userId2;
            remoteTestCase.OwnerId = userId1;
            remoteTestCase.TestCaseTypeId = tc_typeScenarioId;
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Approved;
            remoteTestCase.EstimatedDuration = 45;
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            testCaseId3 = remoteTestCase.TestCaseId.Value;
            //Test Case 4
            remoteTestCase = new RemoteTestCase();
            remoteTestCase.Name = "Test Case 4";
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.TestCaseFolderId = testFolderId2;
            remoteTestCase.AuthorId = userId2;
            remoteTestCase.OwnerId = userId2;
            remoteTestCase.TestCaseTypeId = tc_typeFunctionalId;
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Draft;
            remoteTestCase.ComponentIds = new List<int>() { componentId1, componentId2 };
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            testCaseId4 = remoteTestCase.TestCaseId.Value;

            //First retrieve the top-level folder
            remoteTestCaseFolder = spiraSoapClient.TestCase_RetrieveFolderById(credentials, projectId1, testFolderId1);
            Assert.AreEqual("Test Folder A", remoteTestCaseFolder.Name);
            Assert.AreEqual(4, remoteTestCaseFolder.CountNotRun);
            Assert.AreEqual(100, remoteTestCaseFolder.EstimatedDuration);
            Assert.IsNull(remoteTestCaseFolder.ActualDuration);
            Assert.IsNull(remoteTestCaseFolder.ExecutionDate);

            //Now retrieve the second-level folder
            remoteTestCaseFolder = spiraSoapClient.TestCase_RetrieveFolderById(credentials, projectId1, testFolderId2);
            Assert.AreEqual("Test Folder B", remoteTestCaseFolder.Name);
            Assert.AreEqual(2, remoteTestCaseFolder.CountNotRun);
            Assert.AreEqual(45, remoteTestCaseFolder.EstimatedDuration);
            Assert.IsNull(remoteTestCaseFolder.ActualDuration);
            Assert.IsNull(remoteTestCaseFolder.ExecutionDate);

            //Now retrieve a test case
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            Assert.AreEqual(2, remoteTestCase.ArtifactTypeId);
            Assert.AreEqual("Test Case 4", remoteTestCase.Name);
            Assert.IsNull(remoteTestCase.Description);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.NotRun, remoteTestCase.ExecutionStatusId);
            Assert.AreEqual(userId2, remoteTestCase.AuthorId);
            Assert.AreEqual(userId2, remoteTestCase.OwnerId);
            Assert.IsNull(remoteTestCase.TestCasePriorityId);
            Assert.IsNull(remoteTestCase.EstimatedDuration);
            Assert.IsTrue(remoteTestCase.ComponentIds.Contains(componentId1));
            Assert.IsTrue(remoteTestCase.ComponentIds.Contains(componentId2));

            //Now we need to add some parameters to test case 1 and 2
            RemoteTestCaseParameter remoteTestCaseParameter = new RemoteTestCaseParameter();
            remoteTestCaseParameter.TestCaseId = testCaseId1;
            remoteTestCaseParameter.Name = "param0";
            remoteTestCaseParameter.DefaultValue = "value0";
            spiraSoapClient.TestCase_AddParameter(credentials, projectId1, remoteTestCaseParameter);
            remoteTestCaseParameter = new RemoteTestCaseParameter();
            remoteTestCaseParameter.TestCaseId = testCaseId2;
            remoteTestCaseParameter.Name = "param1";
            remoteTestCaseParameter.DefaultValue = "nothing";
            spiraSoapClient.TestCase_AddParameter(credentials, projectId1, remoteTestCaseParameter);
            remoteTestCaseParameter = new RemoteTestCaseParameter();
            remoteTestCaseParameter.TestCaseId = testCaseId2;
            remoteTestCaseParameter.Name = "param2";
            spiraSoapClient.TestCase_AddParameter(credentials, projectId1, remoteTestCaseParameter);

            //Verify that the parameters were added to the second test case
            RemoteTestCaseParameter[] remoteTestCaseParameters = spiraSoapClient.TestCase_RetrieveParameters(credentials, projectId1, testCaseId2);
			testCaseParameterId1 = (int)remoteTestCaseParameters[0].TestCaseParameterId; //used in test sets testing
			Assert.AreEqual(2, remoteTestCaseParameters.Length);
            Assert.AreEqual("param1", remoteTestCaseParameters[0].Name);
            Assert.AreEqual("nothing", remoteTestCaseParameters[0].DefaultValue);
            Assert.AreEqual("param2", remoteTestCaseParameters[1].Name);
            Assert.IsNull(remoteTestCaseParameters[1].DefaultValue);


            //Make an update to one of the test cases
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            remoteTestCase.Name = "Test Case 4a";
            remoteTestCase.Description = "Test Case Description 4a";
            remoteTestCase.AuthorId = userId2;
            remoteTestCase.OwnerId = userId1;
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase);

            //Verify the change
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            Assert.AreEqual("Test Case 4a", remoteTestCase.Name);
            Assert.AreEqual(userId2, remoteTestCase.AuthorId);
            Assert.AreEqual(userId1, remoteTestCase.OwnerId);
            Assert.AreEqual("Test Case Description 4a", remoteTestCase.Description);

			//Add a new parameter
			remoteTestCaseParameter = new RemoteTestCaseParameter();
			remoteTestCaseParameter.TestCaseId = testCaseId2;
			remoteTestCaseParameter.Name = "param3";
			remoteTestCaseParameter.DefaultValue = "none";
			spiraSoapClient.TestCase_AddParameter(credentials, projectId1, remoteTestCaseParameter);

			//Update the parameter
			remoteTestCaseParameters = spiraSoapClient.TestCase_RetrieveParameters(credentials, projectId1, testCaseId2);
			remoteTestCaseParameters[2].Name = "param3_updated";
			remoteTestCaseParameters[2].DefaultValue = "none_updated";
			int parameterId = (int)remoteTestCaseParameters[2].TestCaseParameterId;
			spiraSoapClient.TestCase_UpdateParameter(credentials, projectId1, remoteTestCaseParameters[2]);

			//Verify the change
			remoteTestCaseParameters = spiraSoapClient.TestCase_RetrieveParameters(credentials, projectId1, testCaseId2);
			Assert.AreEqual(3, remoteTestCaseParameters.Length);
			Assert.AreEqual("param3_updated", remoteTestCaseParameters[2].Name);
			Assert.AreEqual("none_updated", remoteTestCaseParameters[2].DefaultValue);
			Assert.AreEqual(parameterId, remoteTestCaseParameters[2].TestCaseParameterId);

			//Delete the parameter
			spiraSoapClient.TestCase_DeleteParameter(credentials, projectId1, remoteTestCaseParameters[2]);

			//Verify the delete
			remoteTestCaseParameters = spiraSoapClient.TestCase_RetrieveParameters(credentials, projectId1, testCaseId2);
			Assert.AreEqual(2, remoteTestCaseParameters.Length);

			//Verify that we can get the entire folder structure using the new API function added
			//to make integration with v5.0 easier (e.g. Rapise)
			RemoteTestCaseFolder[] remoteTestCaseFolders = spiraSoapClient.TestCase_RetrieveFolders(credentials, projectId1);
            Assert.AreEqual(2, remoteTestCaseFolders.Length);
            Assert.AreEqual("Test Folder A", remoteTestCaseFolders[0].Name);
            Assert.AreEqual("AAA", remoteTestCaseFolders[0].IndentLevel);
            Assert.AreEqual("Test Folder B", remoteTestCaseFolders[1].Name);
            Assert.AreEqual("AAAAAA", remoteTestCaseFolders[1].IndentLevel);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update a status
			remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
			remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.ReadyForTest;
			spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase);

			//Verify that the status did not change
			remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
			Assert.AreEqual((int)TestCase.TestCaseStatusEnum.Approved, remoteTestCase.TestCaseStatusId, "TestCaseStatusId");

			//Try adding an artifact with a non default status
			remoteTestCase = new RemoteTestCase();
			remoteTestCase.Name = "Test Case 3";
			remoteTestCase.Description = "Test Case Description 3";
			remoteTestCase.TestCaseFolderId = testFolderId2;
			remoteTestCase.ProjectId = projectId1;
			remoteTestCase.AuthorId = userId2;
			remoteTestCase.OwnerId = userId1;
			remoteTestCase.TestCaseTypeId = tc_typeScenarioId;
			remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.Approved;
			remoteTestCase.EstimatedDuration = 45;
			remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
			int testCase5 = remoteTestCase.TestCaseId.Value;

			//Verify that the status did not change
			remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCase5);
			Assert.AreEqual((int)TestCase.TestCaseStatusEnum.Draft, remoteTestCase.TestCaseStatusId, "TestCaseStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			spiraSoapClient.TestCase_Delete(credentials, projectId1, testCase5);
		}

        /// <summary>
        /// Verifies that you can import new test steps into the previously created test cases
        /// </summary>
        [
        Test,
        SpiraTestCase(1998)
        ]
        public void _10_ImportTestSteps()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets add two test steps to two of the test cases
            //Step 1
            RemoteTestStep remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 1;
            remoteTestStep.Description = "Test Step 1";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep.SampleData = "test 123";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId3, remoteTestStep);
            testStepId1 = remoteTestStep.TestStepId.Value;
            //Step 2
            remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 2;
            remoteTestStep.Description = "Test Step 2";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId3, remoteTestStep);
            testStepId2 = remoteTestStep.TestStepId.Value;
            //Step 3
            remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 1;
            remoteTestStep.Description = "Test Step 4";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep.SampleData = "test 123";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId4, remoteTestStep);
            testStepId3 = remoteTestStep.TestStepId.Value;
            //Step 4
            remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 1;
            remoteTestStep.Description = "Test Step 3";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId4, remoteTestStep);
            testStepId4 = remoteTestStep.TestStepId.Value;

            //Now verify that the import was successful

            //Retrieve the first test case with its steps
            RemoteTestCase remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
            Assert.AreEqual(2, remoteTestCase.ArtifactTypeId);
            Assert.AreEqual("Test Case 3", remoteTestCase.Name);
            List<RemoteTestStep> remoteTestSteps = remoteTestCase.TestSteps;
            Assert.AreEqual(7, remoteTestSteps[0].ArtifactTypeId);
            Assert.AreEqual(2, remoteTestSteps.Count, "Test Step Count 1");
            Assert.AreEqual("Test Step 1", remoteTestSteps[0].Description);
            Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 1");
            Assert.AreEqual("Test Step 2", remoteTestSteps[1].Description);
            Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 2");

            //Retrieve the second test case with its steps
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            Assert.AreEqual(2, remoteTestCase.ArtifactTypeId);
            Assert.AreEqual("Test Case 4a", remoteTestCase.Name);
            remoteTestSteps = remoteTestCase.TestSteps;
            Assert.AreEqual(2, remoteTestSteps.Count, "Test Step Count 2");
            Assert.AreEqual(7, remoteTestSteps[0].ArtifactTypeId);
            Assert.AreEqual("Test Step 3", remoteTestSteps[0].Description);
            Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 3");
            Assert.AreEqual("Test Step 4", remoteTestSteps[1].Description);
            Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 4");

			testStepId5 = spiraSoapClient.TestCase_AddLink(credentials, projectId1, testCaseId1, testCaseId3, 1, null);

            //Now lets add a linked test case as a test step with two parameters
            RemoteTestStepParameter[] parameterArray = new RemoteTestStepParameter[2];
            parameterArray[0] = new RemoteTestStepParameter();
            parameterArray[0].Name = "param1";
            parameterArray[0].Value = "value1";
            parameterArray[1] = new RemoteTestStepParameter();
            parameterArray[1].Name = "param2";
            parameterArray[1].Value = "value2";
            testStepId6 = spiraSoapClient.TestCase_AddLink(credentials, projectId1, testCaseId1, testCaseId2, 2, parameterArray);

            //Retrieve the linked test case steps
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId1);
            Assert.AreEqual("Test Case 1", remoteTestCase.Name);
            remoteTestSteps = remoteTestCase.TestSteps;
            Assert.AreEqual(2, remoteTestSteps.Count, "Test Step Count 3");
            Assert.AreEqual("Call", remoteTestSteps[0].Description);
            Assert.AreEqual(testCaseId3, remoteTestSteps[0].LinkedTestCaseId);
            Assert.AreEqual("Call", remoteTestSteps[1].Description);
            Assert.AreEqual(testCaseId2, remoteTestSteps[1].LinkedTestCaseId);

            //Verify that the parameters were added correctly
            RemoteTestStepParameter[] remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
            Assert.AreEqual(2, remoteTestStepParameters.Length);
            Assert.AreEqual("param1", remoteTestStepParameters[0].Name);
            Assert.AreEqual("value1", remoteTestStepParameters[0].Value);
            Assert.AreEqual("param2", remoteTestStepParameters[1].Name);
            Assert.AreEqual("value2", remoteTestStepParameters[1].Value);

			//Delete a parameter
			RemoteTestStepParameter remoteTestStepParameter = new RemoteTestStepParameter()
			{
				Name = "param2",
				Value = "value2"
			};
			RemoteTestStepParameter[] remoteParametersToUpdate = new RemoteTestStepParameter[1];
			remoteParametersToUpdate[0] = remoteTestStepParameter;
			spiraSoapClient.TestCase_DeleteStepParameters(credentials, projectId1, testCaseId1, testStepId6, remoteParametersToUpdate);

			//Verify the delete
			remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
			Assert.AreEqual(1, remoteTestStepParameters.Length);
			Assert.AreEqual("param1", remoteTestStepParameters[0].Name);
			Assert.AreEqual("value1", remoteTestStepParameters[0].Value);

			//Try and add a new paramater using the update method
			spiraSoapClient.TestCase_UpdateStepParameters(credentials, projectId1, testCaseId1, testStepId6, remoteParametersToUpdate);
			//Verify this did not work
			remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
			Assert.AreEqual(1, remoteTestStepParameters.Length);

			//Add a new parameter (add param 2 back)
			spiraSoapClient.TestCase_AddStepParameters(credentials, projectId1, testCaseId1, testStepId6, remoteParametersToUpdate);

			//Verify the addition
			remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
			Assert.AreEqual(2, remoteTestStepParameters.Length);
			Assert.AreEqual("param2", remoteTestStepParameters[1].Name);
			Assert.AreEqual("value2", remoteTestStepParameters[1].Value);

			//Try to update a parameter using the add method
			remoteParametersToUpdate[0].Value = "value2 - edited";
			//Verify this did not work
			remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
			Assert.AreEqual(2, remoteTestStepParameters.Length);
			Assert.AreEqual("param2", remoteTestStepParameters[1].Name);
			Assert.AreEqual("value2", remoteTestStepParameters[1].Value);

			//Update a parameter
			spiraSoapClient.TestCase_UpdateStepParameters(credentials, projectId1, testCaseId1, testStepId6, remoteParametersToUpdate);

			//Verify the update
			remoteTestStepParameters = spiraSoapClient.TestCase_RetrieveStepParameters(credentials, projectId1, testCaseId1, testStepId6);
			Assert.AreEqual(2, remoteTestStepParameters.Length);
			Assert.AreEqual("param2", remoteTestStepParameters[1].Name);
			Assert.AreEqual("value2 - edited", remoteTestStepParameters[1].Value);

			//Reset - just in case
			remoteParametersToUpdate[0].Value = "value2";
			spiraSoapClient.TestCase_UpdateStepParameters(credentials, projectId1, testCaseId1, testStepId6, remoteParametersToUpdate);

			//Finally make an update to one of the test cases and its test steps
			remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            remoteTestCase.Name = "Test Case 4b";
            remoteTestCase.Description = "Test Case Description 4b";
            remoteTestSteps = remoteTestCase.TestSteps;
            remoteTestSteps[0].Description = "Test Step 3b";
            remoteTestSteps[1].Description = "Test Step 4b";
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase);

            //Verify the change
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            Assert.AreEqual("Test Case 4b", remoteTestCase.Name);
            Assert.AreEqual("Test Case Description 4b", remoteTestCase.Description);
            remoteTestSteps = remoteTestCase.TestSteps;
            Assert.AreEqual(2, remoteTestSteps.Count, "Test Step Count 2");
            Assert.AreEqual("Test Step 3b", remoteTestSteps[0].Description);
            Assert.AreEqual(1, remoteTestSteps[0].Position, "Position 3");
            Assert.AreEqual("Test Step 4b", remoteTestSteps[1].Description);
            Assert.AreEqual(2, remoteTestSteps[1].Position, "Position 4");
        }

        /// <summary>
        /// Verifies that you can import requirements test coverage into the project
        /// </summary>
        [
        Test,
        SpiraTestCase(1987)
        ]
        public void _11_ImportCoverage()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Next lets add some coverage entries for a requirement
            //Entry 1
            RemoteRequirementTestCaseMapping remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
            spiraSoapClient.Requirement_AddTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);
            //Entry 2
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId2;
            spiraSoapClient.Requirement_AddTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);
            //Entry 3
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId3;
            spiraSoapClient.Requirement_AddTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);

            //Try adding a duplicate entry - should fail quietly
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
            spiraSoapClient.Requirement_AddTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);

            //Now verify the coverage data
            RemoteRequirementTestCaseMapping[] requirementTestCaseMappings = spiraSoapClient.Requirement_RetrieveTestCoverage(credentials, projectId1, requirementId2);
            Assert.AreEqual(3, requirementTestCaseMappings.Length);
            Assert.AreEqual(testCaseId1, requirementTestCaseMappings[0].TestCaseId);
            Assert.AreEqual(testCaseId2, requirementTestCaseMappings[1].TestCaseId);
            Assert.AreEqual(testCaseId3, requirementTestCaseMappings[2].TestCaseId);

            //Finally remove the coverage
            //Entry 1
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId1;
            spiraSoapClient.Requirement_RemoveTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);
            //Entry 2
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId2;
            spiraSoapClient.Requirement_RemoveTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);
            //Entry 3
            remoteRequirementTestCaseMapping = new RemoteRequirementTestCaseMapping();
            remoteRequirementTestCaseMapping.RequirementId = requirementId2;
            remoteRequirementTestCaseMapping.TestCaseId = testCaseId3;
            spiraSoapClient.Requirement_RemoveTestCoverage(credentials, projectId1, remoteRequirementTestCaseMapping);

            //Now verify the coverage data
            requirementTestCaseMappings = spiraSoapClient.Requirement_RetrieveTestCoverage(credentials, projectId1, requirementId2);
            Assert.AreEqual(0, requirementTestCaseMappings.Length);

            //Test that we can map test cases against releases & iterations

            //First test that nothing is mapped against the release or iterations, except those that were auto-mapped from the requirement
            RemoteReleaseTestCaseMapping[] releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, releaseId1);
            Assert.AreEqual(3, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId1);
            Assert.AreEqual(0, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId2);
            Assert.AreEqual(0, releaseTestCaseMappings.Length);

            //Now lets map the tests to the release and the iteration
            //Release 1
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, releaseId1, new int[] { testCaseId1 });
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, releaseId1, new int[] { testCaseId2 });
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, releaseId1, new int[] { testCaseId3 });
            //Iteration 1
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, iterationId1, new int[] { testCaseId1 });
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, iterationId1, new int[] { testCaseId2 });
            //Iteration 2
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, iterationId2, new int[] { testCaseId2 });
            spiraSoapClient.Release_AddTestMapping(credentials, projectId1, iterationId2, new int[] { testCaseId3 });

            //Now test that the mappings have been added
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, releaseId1);
            Assert.AreEqual(3, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId1);
            Assert.AreEqual(2, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId2);
            Assert.AreEqual(2, releaseTestCaseMappings.Length);

            //Now remove some of the mappings
            //Release 1
            spiraSoapClient.Release_RemoveTestMapping(credentials, projectId1, releaseId1, testCaseId1);
            //Iteration 1
            spiraSoapClient.Release_RemoveTestMapping(credentials, projectId1, iterationId1, testCaseId1);
            //Iteration 2
            spiraSoapClient.Release_RemoveTestMapping(credentials, projectId1, iterationId2, testCaseId3);

            //Test the mapping changes
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, releaseId1);
            Assert.AreEqual(2, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId1);
            Assert.AreEqual(1, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId2);
            Assert.AreEqual(1, releaseTestCaseMappings.Length);

            //Finally test the mapping changes
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, releaseId1);
            Assert.AreEqual(2, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId1);
            Assert.AreEqual(1, releaseTestCaseMappings.Length);
            releaseTestCaseMappings = spiraSoapClient.Release_RetrieveTestMapping(credentials, projectId1, iterationId2);
            Assert.AreEqual(1, releaseTestCaseMappings.Length);
        }

        /// <summary>
        /// Verifies that you can import test sets into the project
        /// </summary>
        [
        Test,
        SpiraTestCase(1997)
        ]
        public void _12_ImportTestSets()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets try importing a test set folder and put some test sets in it]
            RemoteTestSetFolder remoteTestSetFolder = new RemoteTestSetFolder();
            remoteTestSetFolder.ProjectId = projectId1;
            remoteTestSetFolder.Name = "Test Set Folder";
            testSetFolderId1 = spiraSoapClient.TestSet_CreateFolder(credentials, projectId1, remoteTestSetFolder).TestSetFolderId.Value;

            //First lets try inserting a new test set into this folder, that has a custom property value set
            RemoteTestSet remoteTestSet = new RemoteTestSet();
            remoteTestSet.ProjectId = projectId1;
            remoteTestSet.Name = "Test Set 1";
            remoteTestSet.TestSetFolderId = testSetFolderId1;
            remoteTestSet.CreatorId = userId1;
            remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
            remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
            remoteTestSet.CustomProperties = new List<RemoteArtifactCustomProperty>() { new RemoteArtifactCustomProperty() { PropertyNumber = 1, IntegerValue = customValueId3 } };
            testSetId1 = spiraSoapClient.TestSet_Create(credentials, projectId1, remoteTestSet).TestSetId.Value;

            //Verify that it inserted correctly
            remoteTestSet = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId1);
            Assert.AreEqual(8, remoteTestSet.ArtifactTypeId);
            Assert.AreEqual(testSetId1, remoteTestSet.TestSetId, "TestSetId");
            Assert.AreEqual(userId1, remoteTestSet.CreatorId, "CreatorId");
            Assert.AreEqual("Test Set 1", remoteTestSet.Name, "Name");
            Assert.AreEqual(testSetFolderId1, remoteTestSet.TestSetFolderId);
            Assert.AreEqual((int)TestSet.TestSetStatusEnum.NotStarted, remoteTestSet.TestSetStatusId, "TestSetStatusId");
            Assert.IsNull(remoteTestSet.Description, "Description");
            Assert.IsNull(remoteTestSet.ReleaseId);
            Assert.IsNull(remoteTestSet.OwnerId, "OwnerName");
            Assert.IsNull(remoteTestSet.PlannedDate, "PlannedDate");
            Assert.IsNull(remoteTestSet.ExecutionDate, "ExecutionDate");
            Assert.AreEqual(1, remoteTestSet.CustomProperties.Count);
            Assert.AreEqual(customValueId3, remoteTestSet.CustomProperties[0].IntegerValue.Value);
            Assert.AreEqual(1, remoteTestSet.CustomProperties[0].PropertyNumber);
            Assert.AreEqual("Component", remoteTestSet.CustomProperties[0].Definition.Name);

            //Next lets try inserting a new test set after it in the folder
            remoteTestSet = new RemoteTestSet();
            remoteTestSet.ProjectId = projectId1;
            remoteTestSet.Name = "Test Set 2";
            remoteTestSet.Description = "Test Set 2 Description";
            remoteTestSet.TestSetFolderId = testSetFolderId1;
            remoteTestSet.CreatorId = userId1;
            remoteTestSet.OwnerId = userId2;
            remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
            remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.InProgress;
            remoteTestSet.PlannedDate = DateTime.UtcNow.AddDays(1);
            testSetId2 = spiraSoapClient.TestSet_Create(credentials, projectId1, remoteTestSet).TestSetId.Value;
            //Verify that it inserted correctly
            remoteTestSet = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId2);
            Assert.AreEqual(8, remoteTestSet.ArtifactTypeId);
            Assert.AreEqual(testSetId2, remoteTestSet.TestSetId, "TestSetId");
            Assert.AreEqual(userId1, remoteTestSet.CreatorId, "CreatorId");
            Assert.AreEqual("Test Set 2", remoteTestSet.Name, "Name");
            Assert.AreEqual(testSetFolderId1, remoteTestSet.TestSetFolderId);
            Assert.AreEqual((int)TestSet.TestSetStatusEnum.InProgress, remoteTestSet.TestSetStatusId, "TestSetStatusId");
            Assert.AreEqual("Test Set 2 Description", remoteTestSet.Description, "Description");
            Assert.IsNull(remoteTestSet.ReleaseId);
            Assert.AreEqual(userId2, remoteTestSet.OwnerId, "OwnerId");
            Assert.IsNotNull(remoteTestSet.PlannedDate, "PlannedDate");
            Assert.IsNull(remoteTestSet.ExecutionDate, "ExecutionDate");

            //Now test that we can add some test cases to one of the test sets
            //Need to verify that we can add the same test case twice
            //and if necessary specify a different OwnerId

            //Test Case 1
            RemoteTestSetTestCaseMapping[] remoteTestSetTestCaseMappings = spiraSoapClient.TestSet_AddTestMapping(credentials, projectId1, testSetId1, testCaseId1, null, null, null);
            Assert.AreEqual(1, remoteTestSetTestCaseMappings.Length);
            Assert.AreEqual(testSetId1, remoteTestSetTestCaseMappings[0].TestSetId);
            Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[0].TestCaseId);
            Assert.IsTrue(remoteTestSetTestCaseMappings[0].TestSetTestCaseId > 0);
            int testSetTestCaseId = remoteTestSetTestCaseMappings[0].TestSetTestCaseId;

            //Test Case 3
            remoteTestSetTestCaseMappings = spiraSoapClient.TestSet_AddTestMapping(credentials, projectId1, testSetId1, testCaseId3, USER_ID_JOE_SMITH, null, null);
            Assert.AreEqual(1, remoteTestSetTestCaseMappings.Length);
            Assert.AreEqual(testSetId1, remoteTestSetTestCaseMappings[0].TestSetId);
            Assert.AreEqual(testCaseId3, remoteTestSetTestCaseMappings[0].TestCaseId);
            Assert.IsTrue(remoteTestSetTestCaseMappings[0].TestSetTestCaseId > 0);

            //Test Case 1 - with OwnerId specified
            remoteTestSetTestCaseMappings = spiraSoapClient.TestSet_AddTestMapping(credentials, projectId1, testSetId1, testCaseId1, USER_ID_FRED_BLOGGS, null, null);
            Assert.AreEqual(1, remoteTestSetTestCaseMappings.Length);
            Assert.AreEqual(testSetId1, remoteTestSetTestCaseMappings[0].TestSetId);
            Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[0].TestCaseId);
            Assert.IsTrue(remoteTestSetTestCaseMappings[0].TestSetTestCaseId > 0);
            //Make sure it gets a new instance id, separate from the first one
            Assert.AreNotEqual(testSetTestCaseId, remoteTestSetTestCaseMappings[0].TestSetTestCaseId);

            //Now verify the data
            RemoteTestCase[] remoteTestCases = spiraSoapClient.TestCase_RetrieveByTestSetId(credentials, projectId1, testSetId1);
            Assert.AreEqual(3, remoteTestCases.Length);
            Assert.AreEqual("Test Case 1", remoteTestCases[0].Name);
            Assert.AreEqual(null, remoteTestCases[0].OwnerId);
            Assert.AreEqual("Test Case 3", remoteTestCases[1].Name);
            Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestCases[1].OwnerId);
            Assert.AreEqual("Test Case 1", remoteTestCases[2].Name);
            Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestCases[2].OwnerId);

            //Now verify that we can get the mapping object version of the data
            remoteTestSetTestCaseMappings = spiraSoapClient.TestSet_RetrieveTestCaseMapping(credentials, projectId1, testSetId1);
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
            spiraSoapClient.TestSet_AddTestMapping(credentials, projectId1, testSetId1, testCaseId2, USER_ID_FRED_BLOGGS, existingTestSetTestCaseId, parameters.ToArray());

            //Now verify the data
            remoteTestCases = spiraSoapClient.TestCase_RetrieveByTestSetId(credentials, projectId1, testSetId1);
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
            remoteTestSetTestCaseMappings = spiraSoapClient.TestSet_RetrieveTestCaseMapping(credentials, projectId1, testSetId1);
            Assert.AreEqual(4, remoteTestSetTestCaseMappings.Length);
            Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[0].TestCaseId);
            Assert.AreEqual(null, remoteTestSetTestCaseMappings[0].OwnerId);
            Assert.AreEqual(testCaseId3, remoteTestSetTestCaseMappings[1].TestCaseId);
            Assert.AreEqual(USER_ID_JOE_SMITH, remoteTestSetTestCaseMappings[1].OwnerId);
            Assert.AreEqual(testCaseId2, remoteTestSetTestCaseMappings[2].TestCaseId);
            Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestSetTestCaseMappings[2].OwnerId);
            Assert.AreEqual(testCaseId1, remoteTestSetTestCaseMappings[3].TestCaseId);
            Assert.AreEqual(USER_ID_FRED_BLOGGS, remoteTestSetTestCaseMappings[3].OwnerId);

			//Verify the test set test case parameters were set
			int testSetTestCaseWithParametersId = remoteTestSetTestCaseMappings[2].TestSetTestCaseId;
			RemoteTestSetTestCaseParameter[] remoteTestSetTestCaseParameters = spiraSoapClient.TestSet_RetrieveTestCaseParameters(credentials, projectId1, testSetId1, testSetTestCaseWithParametersId);
			Assert.AreEqual(2, remoteTestSetTestCaseParameters.Length, "there should be 2 test set test case parameters");
			Assert.AreEqual("test value 1", remoteTestSetTestCaseParameters[0].Value, "the first test set test case parameter value is incorrect");
			Assert.AreEqual("param1", remoteTestSetTestCaseParameters[0].Name, "the first test set test case parameter name is incorrect");
			Assert.AreEqual(remoteTestSetTestCaseMappings[2].TestSetTestCaseId, remoteTestSetTestCaseParameters[0].TestSetTestCaseId, "the first test set test case parameter TestSetTestCaseId is incorrect");

			//Now check the overall test set status (available since v4.0)
			remoteTestSet = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId1);
            Assert.AreEqual(0, remoteTestSet.CountBlocked);
            Assert.AreEqual(0, remoteTestSet.CountCaution);
            Assert.AreEqual(0, remoteTestSet.CountFailed);
            Assert.AreEqual(0, remoteTestSet.CountNotApplicable);
            Assert.AreEqual(4, remoteTestSet.CountNotRun);
            Assert.AreEqual(0, remoteTestSet.CountPassed);

			//Add a new parameter
			RemoteTestSetParameter remoteTestSetParameter = new RemoteTestSetParameter();
			remoteTestSetParameter.TestSetId = testSetId1;
			remoteTestSetParameter.TestCaseParameterId = testCaseParameterId1;
			remoteTestSetParameter.Value = "value0-testSet";
			spiraSoapClient.TestSet_AddParameter(credentials, projectId1, remoteTestSetParameter);

			//Verify the addition
			RemoteTestSetParameter[] remoteTestSetParameters = spiraSoapClient.TestSet_RetrieveParameters(credentials, projectId1, testSetId1);
			Assert.AreEqual(1, remoteTestSetParameters.Length);
			Assert.AreEqual("param1", remoteTestSetParameters[0].Name);
			Assert.AreEqual("value0-testSet", remoteTestSetParameters[0].Value);
			Assert.AreEqual(testSetId1, remoteTestSetParameters[0].TestSetId);
			Assert.AreEqual(testCaseParameterId1, remoteTestSetParameters[0].TestCaseParameterId);

			//Update the parameter
			remoteTestSetParameters[0].Value = "value0-testSet-updated";
			spiraSoapClient.TestSet_UpdateParameter(credentials, projectId1, remoteTestSetParameters[0]);

			//Verify the change
			remoteTestSetParameters = spiraSoapClient.TestSet_RetrieveParameters(credentials, projectId1, testSetId1);
			Assert.AreEqual(1, remoteTestSetParameters.Length);
			Assert.AreEqual("param1", remoteTestSetParameters[0].Name);
			Assert.AreEqual("value0-testSet-updated", remoteTestSetParameters[0].Value);
			Assert.AreEqual(testSetId1, remoteTestSetParameters[0].TestSetId);
			Assert.AreEqual(testCaseParameterId1, remoteTestSetParameters[0].TestCaseParameterId);

			//Delete the parameter
			spiraSoapClient.TestSet_DeleteParameter(credentials, projectId1, remoteTestSetParameters[0]);

			//Verify the delete
			remoteTestSetParameters = spiraSoapClient.TestSet_RetrieveParameters(credentials, projectId1, testSetId1);
			Assert.AreEqual(0, remoteTestSetParameters.Length);

			//Add a new test case parameter
			TestCaseManager testCaseManager = new TestCaseManager();
			int testCaseParameter3id = testCaseManager.InsertParameter(projectId1, testCaseId2, "param3", "default for param3 at test case", USER_ID_FRED_BLOGGS);

			//Add a test set test case parameter using this test case parameter
			RemoteTestSetTestCaseParameter parameter3 = new RemoteTestSetTestCaseParameter();
			parameter3.TestSetTestCaseId = testSetTestCaseWithParametersId;
			parameter3.TestCaseParameterId = testCaseParameter3id;
			parameter3.Value = "test value 3";
			spiraSoapClient.TestSet_AddTestCaseParameter(credentials, projectId1, testSetId1, parameter3);

			//Verify the added test set test case parameter
			remoteTestSetTestCaseParameters = spiraSoapClient.TestSet_RetrieveTestCaseParameters(credentials, projectId1, testSetId1, testSetTestCaseWithParametersId);
			Assert.AreEqual(3, remoteTestSetTestCaseParameters.Length, "there should be 3 test set test case parameters");
			Assert.AreEqual("test value 3", remoteTestSetTestCaseParameters[2].Value, "the new test set test case parameter value is incorrect");
			Assert.AreEqual("param3", remoteTestSetTestCaseParameters[2].Name, "the new test set test case parameter name is incorrect");
			Assert.AreEqual(testSetTestCaseWithParametersId, remoteTestSetTestCaseParameters[2].TestSetTestCaseId, "the new test set test case parameter TestSetTestCaseId is incorrect");

			//Update a test set test case paramater
			parameter3.Value = "update test value 3";
			spiraSoapClient.TestSet_UpdateTestCaseParameter(credentials, projectId1, testSetId1, parameter3);

			//Verify the updated test set test case parameter
			remoteTestSetTestCaseParameters = spiraSoapClient.TestSet_RetrieveTestCaseParameters(credentials, projectId1, testSetId1, testSetTestCaseWithParametersId);
			Assert.AreEqual(3, remoteTestSetTestCaseParameters.Length, "there should be 3 test set test case parameters");
			Assert.AreEqual("update test value 3", remoteTestSetTestCaseParameters[2].Value, "the updated test set test case parameter value is incorrect");
			Assert.AreEqual("param3", remoteTestSetTestCaseParameters[2].Name, "the test set test case parameter name is incorrect");
			Assert.AreEqual(testSetTestCaseWithParametersId, remoteTestSetTestCaseParameters[2].TestSetTestCaseId, "the test set test case parameter TestSetTestCaseId is incorrect");

			//Delete the test set test case parameter
			spiraSoapClient.TestSet_DeleteTestCaseParameter(credentials, projectId1, testSetId1, parameter3);

			//Verify the deleted test set test case parameter
			remoteTestSetTestCaseParameters = spiraSoapClient.TestSet_RetrieveTestCaseParameters(credentials, projectId1, testSetId1, testSetTestCaseWithParametersId);
			Assert.AreEqual(2, remoteTestSetTestCaseParameters.Length, "there should be 2 test set test case parameters");
			Assert.AreEqual("test value 1", remoteTestSetTestCaseParameters[0].Value, "the test set test case parameter value is incorrect");
			Assert.AreEqual("test value 2", remoteTestSetTestCaseParameters[1].Value, "the test set test case parameter value is incorrect");

			//Cleanup
			//Delete the test case parameter completely
			testCaseManager.DeleteParameter(projectId1, testCaseParameter3id, USER_ID_FRED_BLOGGS);

		}

        /// <summary>
        /// Verifies that you can import a test run complete with test run steps
        /// </summary>
        [
        Test,
        SpiraTestCase(1996)
        ]
        public void _13_ImportTestRun()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Now we need to create a test run from existing test cases we've already imported that have test steps
            RemoteManualTestRun[] remoteManualTestRuns = spiraSoapClient.TestRun_CreateFromTestCases(credentials, projectId1, iterationId1, new int[] { testCaseId3, testCaseId4 });

            //Now mark the first step as pass, the second as fail
            remoteManualTestRuns[0].TestRunSteps[0].ActualResult = "passed";
            remoteManualTestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
            remoteManualTestRuns[0].TestRunSteps[1].ActualResult = "broke";
            remoteManualTestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;

            //For the second test case, mark the first step as blocked
            remoteManualTestRuns[1].TestRunSteps[0].ActualResult = "blocked";
            remoteManualTestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;

            //Finally actually save the run, specifying the actual durations
            remoteManualTestRuns[0].ActualDuration = 10;
            remoteManualTestRuns[1].ActualDuration = 15;
            remoteManualTestRuns = spiraSoapClient.TestRun_Save(credentials, projectId1, DateTime.UtcNow, remoteManualTestRuns);
            testRunId1 = remoteManualTestRuns[0].TestRunId.Value;
            testRunStepId1 = remoteManualTestRuns[0].TestRunSteps[0].TestRunStepId.Value;
            testRunStepId2 = remoteManualTestRuns[0].TestRunSteps[1].TestRunStepId.Value;
            testRunId2 = remoteManualTestRuns[1].TestRunId.Value;
            testRunStepId3 = remoteManualTestRuns[1].TestRunSteps[0].TestRunStepId.Value;

            //Now verify that it saved correctly

            //First check the test run
            //Generic RemoteTestRun object
            RemoteTestRun remoteTestRun = spiraSoapClient.TestRun_RetrieveById(credentials, projectId1, testRunId1);
            Assert.AreEqual(5, remoteTestRun.ArtifactTypeId);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestRun.ExecutionStatusId, "TestRun");
            Assert.AreEqual(iterationId1, remoteTestRun.ReleaseId, "ReleaseId");
            Assert.AreEqual(10, remoteTestRun.ActualDuration);

            //Specic RemoteManualTestRun object
            RemoteManualTestRun remoteManualTestRun = spiraSoapClient.TestRun_RetrieveManualById(credentials, projectId1, testRunId1);
            Assert.AreEqual(5, remoteManualTestRun.ArtifactTypeId);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.ExecutionStatusId, "TestRun");
            Assert.AreEqual(iterationId1, remoteManualTestRun.ReleaseId, "ReleaseId");
            Assert.AreEqual(10, remoteManualTestRun.ActualDuration);

            //Now check the test run steps
            Assert.AreEqual("passed", remoteManualTestRun.TestRunSteps[0].ActualResult);
            Assert.AreEqual("broke", remoteManualTestRun.TestRunSteps[1].ActualResult);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteManualTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

            //Now check the test case itself to see if it was correctly updated
            RemoteTestCase remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
            Assert.IsNotNull(remoteTestCase.ExecutionDate);

            //The second test case in the list should be listed as blocked
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId4);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, remoteTestCase.ExecutionStatusId, "TestCase");
            Assert.IsNotNull(remoteTestCase.ExecutionDate);

            //Now test that we can create a test run from a test set
            remoteManualTestRuns = spiraSoapClient.TestRun_CreateFromTestSet(credentials, projectId1, testSetId1);

            //Now mark the first step as pass, the second as fail
            remoteManualTestRuns[0].TestRunSteps[0].ActualResult = "passed";
            remoteManualTestRuns[0].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
            remoteManualTestRuns[0].TestRunSteps[1].ActualResult = "broke";
            remoteManualTestRuns[0].TestRunSteps[1].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
            remoteManualTestRuns[0].ActualDuration = 15;
            remoteManualTestRuns[1].TestRunSteps[0].ActualResult = "caution";
            remoteManualTestRuns[1].TestRunSteps[0].ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
            remoteManualTestRuns[1].ActualDuration = 20;

            //Finally actually save the run (add on 1 min to make sure execution date is later so that status changes)
            remoteManualTestRuns = spiraSoapClient.TestRun_Save(credentials, projectId1, DateTime.UtcNow.AddMinutes(1), remoteManualTestRuns);
            testRunId1 = remoteManualTestRuns[0].TestRunId.Value;
            testRunId2 = remoteManualTestRuns[1].TestRunId.Value;
            testRunStepId1 = remoteManualTestRuns[0].TestRunSteps[0].TestRunStepId.Value;
            testRunStepId2 = remoteManualTestRuns[0].TestRunSteps[1].TestRunStepId.Value;
            testRunStepId3 = remoteManualTestRuns[1].TestRunSteps[0].TestRunStepId.Value;

            //Now verify that it saved correctly

            //First check the first test case itself to see if it was correctly updated
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId1);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId, "TestCase");
            Assert.IsNotNull(remoteTestCase.ExecutionDate);
            //The second test case in the test set should be listed as caution
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, remoteTestCase.ExecutionStatusId, "TestCase");
            Assert.IsNotNull(remoteTestCase.ExecutionDate);

            //Now check the overall test set status (available since v4.0)
            RemoteTestSet remoteTestSet = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId1);
            Assert.AreEqual(0, remoteTestSet.CountBlocked);
            Assert.AreEqual(1, remoteTestSet.CountCaution);
            Assert.AreEqual(1, remoteTestSet.CountFailed);
            Assert.AreEqual(1, remoteTestSet.CountNotApplicable);   //Due to the linked step
            Assert.AreEqual(1, remoteTestSet.CountNotRun);
            Assert.AreEqual(0, remoteTestSet.CountPassed);

            //Now check the test run
            remoteManualTestRun = spiraSoapClient.TestRun_RetrieveManualById(credentials, projectId1, testRunId1);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.ExecutionStatusId, "TestRun");
            Assert.AreEqual(testSetId1, remoteManualTestRun.TestSetId, "TestSetId");
            Assert.AreEqual(30, remoteManualTestRun.EstimatedDuration, "EstimatedDuration");
            Assert.AreEqual(15, remoteManualTestRun.ActualDuration, "ActualDuration");

            //Finally check the test run steps
            Assert.AreEqual("passed", remoteManualTestRun.TestRunSteps[0].ActualResult);
            Assert.AreEqual("broke", remoteManualTestRun.TestRunSteps[1].ActualResult);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteManualTestRun.TestRunSteps[0].ExecutionStatusId, "TestRunStep 1");
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteManualTestRun.TestRunSteps[1].ExecutionStatusId, "TestRunStep 2");

            //*** Now test that we can use the automated test runner API ***

            //This tests that we can create a successful automated test run with a release but no test set
            RemoteAutomatedTestRun remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.ProjectId = projectId1;
            remoteAutomatedTestRun.TestCaseId = testCaseId1;
            remoteAutomatedTestRun.ReleaseId = iterationId2;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
            remoteAutomatedTestRun.TestRunFormatId = 1; //Plain Text
            testRunId3 = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun).TestRunId.Value;

            //Now retrieve the test run to check that it saved correctly - using the web service
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, testRunId3);
            Assert.AreEqual(5, remoteAutomatedTestRun.ArtifactTypeId);

            //Verify Counts (no steps for this automated run)
            Assert.AreEqual(testRunId3, remoteAutomatedTestRun.TestRunId);
            Assert.IsNull(remoteAutomatedTestRun.TestRunSteps);

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
            Assert.AreEqual(1, remoteAutomatedTestRun.TestRunFormatId);

            //Now verify that the test case itself updated
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId1);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Passed, remoteTestCase.ExecutionStatusId);

            //This tests that we can create a failed automated test run with a test set instead of a release
            remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.ProjectId = projectId1;
            remoteAutomatedTestRun.TesterId = userId2;
            remoteAutomatedTestRun.TestCaseId = testCaseId1;
            remoteAutomatedTestRun.TestSetId = testSetId1;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
            remoteAutomatedTestRun.RunnerAssertCount = 5;
            remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 0";
            remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
            remoteAutomatedTestRun.TestRunFormatId = 1; //Plain Text
            testRunId4 = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun).TestRunId.Value;

            //Now retrieve the test run to check that it saved correctly - using the web service
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, testRunId4);

            //Verify Counts (no steps for automated runs)
            Assert.AreEqual(testRunId4, remoteAutomatedTestRun.TestRunId);
            Assert.IsNull(remoteAutomatedTestRun.TestRunSteps);

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
            Assert.AreEqual(1, remoteAutomatedTestRun.TestRunFormatId);

            //Now verify that the test case itself updated
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId1);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteTestCase.ExecutionStatusId);

            //This tests that we can create a second failed automated test run using a release but no test set
            remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.ProjectId = projectId1;
            remoteAutomatedTestRun.TesterId = userId2;
            remoteAutomatedTestRun.TestCaseId = testCaseId1;
            remoteAutomatedTestRun.ReleaseId = iterationId2;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddSeconds(20);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
            remoteAutomatedTestRun.RunnerAssertCount = 5;
            remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 0";
            remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
            remoteAutomatedTestRun.TestRunFormatId = 1; //Plain Text
            testRunId5 = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun).TestRunId.Value;

            //Now retrieve the test run to check that it saved correctly - using the web service
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, testRunId5);

            //Verify Info (no steps for these automated runs)
            Assert.AreEqual(testRunId5, remoteAutomatedTestRun.TestRunId);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Failed, remoteAutomatedTestRun.ExecutionStatusId);
            Assert.AreEqual("TestSuite", remoteAutomatedTestRun.RunnerName);
            Assert.AreEqual("02_Test_Method", remoteAutomatedTestRun.RunnerTestName);
            Assert.IsNull(remoteAutomatedTestRun.TestRunSteps);

            //Finally we need to test that we can upload multiple runs using the batch API
            List<RemoteAutomatedTestRun> remoteAutomatedTestRuns = new List<RemoteAutomatedTestRun>();
            remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.TesterId = userId2;
            remoteAutomatedTestRun.TestCaseId = testCaseId1;
            remoteAutomatedTestRun.TestSetId = testSetId1;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Caution;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
            remoteAutomatedTestRun.RunnerAssertCount = 5;
            remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 0";
            remoteAutomatedTestRun.RunnerStackTrace = "Error Stack Trace........";
            remoteAutomatedTestRun.TestRunFormatId = 1; //Plain Text
            remoteAutomatedTestRuns.Add(remoteAutomatedTestRun);
            remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.TesterId = userId2;
            remoteAutomatedTestRun.TestCaseId = testCaseId2;
            remoteAutomatedTestRun.TestSetId = testSetId1;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "03_Test_Method";
            remoteAutomatedTestRun.RunnerAssertCount = 3;
            remoteAutomatedTestRun.RunnerMessage = "Expected 1, Found 2";
            remoteAutomatedTestRun.RunnerStackTrace = "<p>Error Stack Trace........</p>";
            remoteAutomatedTestRun.TestRunFormatId = 2; //HTML
            remoteAutomatedTestRuns.Add(remoteAutomatedTestRun);
            RemoteAutomatedTestRun[] remoteAutomatedTestRuns2 = spiraSoapClient.TestRun_RecordAutomated2(credentials, projectId1, remoteAutomatedTestRuns.ToArray());
            int batchTestRunId1 = remoteAutomatedTestRuns2[0].TestRunId.Value;
            int batchTestRunId2 = remoteAutomatedTestRuns2[1].TestRunId.Value;

            //Need to now refresh the cached data (after using this API function) - requires project owner permission
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            spiraSoapClient.Project_RefreshProgressExecutionStatusCaches1(credentials, projectId1, false);
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Verify the data
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, batchTestRunId1);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Caution, remoteAutomatedTestRun.ExecutionStatusId);
            Assert.AreEqual(1, remoteAutomatedTestRun.TestRunFormatId);
            Assert.IsNull(remoteAutomatedTestRun.TestRunSteps);
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, batchTestRunId2);
            Assert.AreEqual((int)TestCase.ExecutionStatusEnum.Blocked, remoteAutomatedTestRun.ExecutionStatusId);
            Assert.AreEqual(2, remoteAutomatedTestRun.TestRunFormatId);
            Assert.IsNull(remoteAutomatedTestRun.TestRunSteps);

            //This tests that we can create a failed automated test run where we provide some test steps to link the results to.
            //Some of the steps are linked to a test step id, some are just automation steps that don't
            //link to an actual test step
            //Also test that if we embed an <img> tag in the test steps with a special GUID
            //We can use that to correlate the images uploaded afterwards (used by Rapise 5.0+)
            remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.ProjectId = projectId1;
            //Test Run
            remoteAutomatedTestRun.TesterId = userId2;
            remoteAutomatedTestRun.TestCaseId = testCaseId3;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = 1;   //Failed
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "03_Test_Method";
            remoteAutomatedTestRun.RunnerAssertCount = 2;
            remoteAutomatedTestRun.RunnerMessage = "winMain didn't display correct title";
            remoteAutomatedTestRun.RunnerStackTrace = "<p>Error Stack Trace........</p>";
            remoteAutomatedTestRun.TestRunFormatId = 2;    //HTML
            //Test Run Steps
            remoteAutomatedTestRun.TestRunSteps = new List<RemoteTestRunStep>();
            //Test Step #1 Failed - linked to a real test step
            RemoteTestRunStep remoteTestRunStep = new RemoteTestRunStep();
            remoteTestRunStep.Description = "Verifying that winMain displays title";
            remoteTestRunStep.ExpectedResult = "Title displayed";
            remoteTestRunStep.SampleData = "test 123";
            remoteTestRunStep.ActualResult = "Object Not Available on Screen, see <img class=\"rapise_report_embedded_image img-responsive\" src=\"#{cb267200-470e-4b6c-8eed-64a5210eb699}\" scale=\"0\" alt=\"1.DoAction([]) returned true\">.";
            remoteTestRunStep.ExecutionStatusId = 1;   //Failed
            remoteTestRunStep.TestStepId = testStepId1;
            remoteAutomatedTestRun.TestRunSteps.Add(remoteTestRunStep);
            //Test Step #2 Caution - not linked to a test step
            remoteTestRunStep = new RemoteTestRunStep();
            remoteTestRunStep.Description = "txtName = 'The Scowrers'";
            remoteTestRunStep.ExpectedResult = "The Scowrers";
            remoteTestRunStep.ActualResult = "the scowrers, see <img class=\"rapise_report_embedded_image img-responsive\" src=\"#{6743731B-C6FD-4DF7-93F3-872960AD26B2}\" scale=\"0\" alt=\"1.DoAction([]) returned true\">.";
            remoteTestRunStep.ExecutionStatusId = 6;   //Caution
            remoteAutomatedTestRun.TestRunSteps.Add(remoteTestRunStep);
            //Test Step #3 Passed - not linked to a test step
            remoteTestRunStep = new RemoteTestRunStep();
            remoteTestRunStep.Description = "cboAuthor Matches Author";
            remoteTestRunStep.ExpectedResult = "AuthorId=5";
            remoteTestRunStep.ExecutionStatusId = 2;   //Passed
            remoteAutomatedTestRun.TestRunSteps.Add(remoteTestRunStep);
            testRunId6 = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun).TestRunId.Value;

            //Now upload two images and use the same GUIDs
            //Image 1
            byte[] dummyImage = UTF8Encoding.Unicode.GetBytes("dummy");
            RemoteDocumentFile remoteDocumentFile = new RemoteDocumentFile();
            remoteDocumentFile.ProjectId = projectId1;
            remoteDocumentFile.ProjectAttachmentFolderId = null;    //Use default
            remoteDocumentFile.FilenameOrUrl = "Step1.png";
            remoteDocumentFile.Description = "#{cb267200-470e-4b6c-8eed-64a5210eb699}";
            List<RemoteLinkedArtifact> linkedArtifacts = new List<RemoteLinkedArtifact>();
            linkedArtifacts.Add(new RemoteLinkedArtifact() { ArtifactId = testRunId6, ArtifactTypeId = /*Test Run*/5 });
            remoteDocumentFile.AttachedArtifacts = linkedArtifacts;
            remoteDocumentFile.BinaryData = dummyImage;
            RemoteDocument remoteDocument = spiraSoapClient.Document_AddFile(credentials, remoteDocumentFile.ProjectId, remoteDocumentFile);
            int attachmentId1 = remoteDocument.AttachmentId.Value;

            //Image2
            dummyImage = UTF8Encoding.Unicode.GetBytes("dummy");
            remoteDocumentFile = new RemoteDocumentFile();
            remoteDocumentFile.ProjectId = projectId1;
            remoteDocumentFile.ProjectAttachmentFolderId = null;    //Use default
            remoteDocumentFile.FilenameOrUrl = "Step2.png";
            remoteDocumentFile.Description = "#{6743731B-C6FD-4DF7-93F3-872960AD26B2}";
            linkedArtifacts = new List<RemoteLinkedArtifact>();
            linkedArtifacts.Add(new RemoteLinkedArtifact() { ArtifactId = testRunId6, ArtifactTypeId = /*Test Run*/5 });
            remoteDocumentFile.AttachedArtifacts = linkedArtifacts;
            remoteDocumentFile.BinaryData = dummyImage;
            remoteDocument = spiraSoapClient.Document_AddFile(credentials, remoteDocumentFile.ProjectId, remoteDocumentFile);
            int attachmentId2 = remoteDocument.AttachmentId.Value;


            //Verify the data
            //The 'real image url' should have been substituted
            string realUrl1 = spiraSoapClient.System_GetArtifactUrl(credentials, /*Attachment*/-14, projectId1, attachmentId1, "");
            realUrl1 = realUrl1.Replace("~", spiraSoapClient.System_GetWebServerUrl(credentials));
            string realUrl2 = spiraSoapClient.System_GetArtifactUrl(credentials, /*Attachment*/-14, projectId1, attachmentId2, "");
            realUrl2 = realUrl2.Replace("~", spiraSoapClient.System_GetWebServerUrl(credentials));

            //Test Run
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, testRunId6);
            Assert.AreEqual(1, remoteAutomatedTestRun.ExecutionStatusId);   //Failed
            Assert.AreEqual(2, remoteAutomatedTestRun.TestRunTypeId);   //Automated
            Assert.AreEqual("TestSuite", remoteAutomatedTestRun.RunnerName);
            Assert.AreEqual("03_Test_Method", remoteAutomatedTestRun.RunnerTestName);
            Assert.AreEqual(2, remoteAutomatedTestRun.RunnerAssertCount);
            Assert.AreEqual("winMain didn't display correct title", remoteAutomatedTestRun.RunnerMessage);
            Assert.AreEqual("<p>Error Stack Trace........</p>", remoteAutomatedTestRun.RunnerStackTrace);
            Assert.AreEqual(2, remoteAutomatedTestRun.TestRunFormatId);
            //Test Run Steps
            Assert.AreEqual(3, remoteAutomatedTestRun.TestRunSteps.Count);
            //Step 1
            Assert.AreEqual(1, remoteAutomatedTestRun.TestRunSteps[0].Position);
            Assert.AreEqual("Verifying that winMain displays title", remoteAutomatedTestRun.TestRunSteps[0].Description);
            Assert.AreEqual("Title displayed", remoteAutomatedTestRun.TestRunSteps[0].ExpectedResult);
            Assert.AreEqual("test 123", remoteAutomatedTestRun.TestRunSteps[0].SampleData);
            Assert.AreEqual(1, remoteAutomatedTestRun.TestRunSteps[0].ExecutionStatusId);
            Assert.AreEqual("Object Not Available on Screen, see <img class=\"rapise_report_embedded_image img-responsive\" src=\"" + realUrl1 + "\" scale=\"0\" alt=\"1.DoAction([]) returned true\">.", remoteAutomatedTestRun.TestRunSteps[0].ActualResult);
            Assert.AreEqual(testStepId1, remoteAutomatedTestRun.TestRunSteps[0].TestStepId);
            //Step 2
            Assert.AreEqual(2, remoteAutomatedTestRun.TestRunSteps[1].Position);
            Assert.AreEqual("txtName = 'The Scowrers'", remoteAutomatedTestRun.TestRunSteps[1].Description);
            Assert.AreEqual("The Scowrers", remoteAutomatedTestRun.TestRunSteps[1].ExpectedResult);
            Assert.IsTrue(String.IsNullOrEmpty(remoteAutomatedTestRun.TestRunSteps[1].SampleData));
            Assert.AreEqual(6, remoteAutomatedTestRun.TestRunSteps[1].ExecutionStatusId);
            Assert.AreEqual("the scowrers, see <img class=\"rapise_report_embedded_image img-responsive\" src=\"" + realUrl2 + "\" scale=\"0\" alt=\"1.DoAction([]) returned true\">.", remoteAutomatedTestRun.TestRunSteps[1].ActualResult);
            Assert.IsFalse(remoteAutomatedTestRun.TestRunSteps[1].TestStepId.HasValue);
            //Step 3
            Assert.AreEqual(3, remoteAutomatedTestRun.TestRunSteps[2].Position);
            Assert.AreEqual("cboAuthor Matches Author", remoteAutomatedTestRun.TestRunSteps[2].Description);
            Assert.AreEqual("AuthorId=5", remoteAutomatedTestRun.TestRunSteps[2].ExpectedResult);
            Assert.IsTrue(String.IsNullOrEmpty(remoteAutomatedTestRun.TestRunSteps[2].SampleData));
            Assert.AreEqual(2, remoteAutomatedTestRun.TestRunSteps[2].ExecutionStatusId);
            Assert.IsTrue(String.IsNullOrEmpty(remoteAutomatedTestRun.TestRunSteps[2].ActualResult));
            Assert.IsFalse(remoteAutomatedTestRun.TestRunSteps[2].TestStepId.HasValue);

            //Now verify that the test case and test step updated
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId3);
            Assert.AreEqual(1, remoteTestCase.ExecutionStatusId);
            Assert.AreEqual(1, remoteTestCase.TestSteps[0].ExecutionStatusId);  //Failed
            Assert.AreEqual(3, remoteTestCase.TestSteps[1].ExecutionStatusId);  //Not Run

            //Clean up by deleting the attachment
            spiraSoapClient.Document_Delete(credentials, projectId1, remoteDocument.AttachmentId.Value);
        }

        /// <summary>
        /// Verifies that you can import incidents into the project
        /// </summary>
        [
        Test,
        SpiraTestCase(1990)
        ]
        public void _14_ImportIncidents()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Now we need to make sure that we can retrieve the list of types, statuses, priorities and severities
            RemoteIncidentType[] remoteIncidentTypes = spiraSoapClient.Incident_RetrieveTypes(credentials, projectTemplateId1);
            Assert.AreEqual(remoteIncidentTypes.Length, 8);
            RemoteIncidentStatus[] remoteIncidentStatuses = spiraSoapClient.Incident_RetrieveStatuses(credentials, projectTemplateId1);
            Assert.AreEqual(remoteIncidentStatuses.Length, 8);
            RemoteIncidentPriority[] remoteIncidentPriorities = spiraSoapClient.Incident_RetrievePriorities(credentials, projectTemplateId1);
            Assert.AreEqual(remoteIncidentPriorities.Length, 4);
            RemoteIncidentSeverity[] remoteIncidentSeverities = spiraSoapClient.Incident_RetrieveSeverities(credentials, projectTemplateId1);
            Assert.AreEqual(remoteIncidentSeverities.Length, 4);

            //Also verify that we can get the default status and type for the project
            RemoteIncidentType defaultRemoteIncidentType = spiraSoapClient.Incident_RetrieveDefaultType(credentials, projectTemplateId1);
            Assert.AreEqual("Incident", defaultRemoteIncidentType.Name);
            Assert.AreEqual(true, defaultRemoteIncidentType.Active);
            RemoteIncidentStatus defaultRemoteIncidentStatus = spiraSoapClient.Incident_RetrieveDefaultStatus(credentials, projectTemplateId1);
            Assert.AreEqual("New", defaultRemoteIncidentStatus.Name);
            Assert.AreEqual(true, defaultRemoteIncidentStatus.Active);

            //Now lets add a custom incident type, status, priority and severity (which we'll then use to import with)
            //Type
            RemoteIncidentType remoteIncidentType = new RemoteIncidentType();
            remoteIncidentType.Name = "Problem";
            remoteIncidentType.Active = true;
            remoteIncidentType.Issue = false;
            remoteIncidentType.Risk = false;
            incidentTypeId = spiraSoapClient.Incident_AddType(credentials, projectTemplateId1, remoteIncidentType).IncidentTypeId.Value;
            //Status
            RemoteIncidentStatus remoteIncidentStatus = new RemoteIncidentStatus();
            remoteIncidentStatus.Name = "Indeterminate";
            remoteIncidentStatus.Active = true;
            remoteIncidentStatus.Open = true;
            incidentStatusId = spiraSoapClient.Incident_AddStatus(credentials, projectTemplateId1, remoteIncidentStatus).IncidentStatusId.Value;
            //Priority
            RemoteIncidentPriority remoteIncidentPriority = new RemoteIncidentPriority();
            remoteIncidentPriority.Name = "Not Good";
            remoteIncidentPriority.Active = true;
            remoteIncidentPriority.Color = "000000";
            int priorityId = spiraSoapClient.Incident_AddPriority(credentials, projectTemplateId1, remoteIncidentPriority).PriorityId.Value;
            //Severity
            RemoteIncidentSeverity remoteIncidentSeverity = new RemoteIncidentSeverity();
            remoteIncidentSeverity.Name = "Difficult";
            remoteIncidentSeverity.Active = true;
            remoteIncidentSeverity.Color = "000000";
            int severityId = spiraSoapClient.Incident_AddSeverity(credentials, projectTemplateId1, remoteIncidentSeverity).SeverityId.Value;

            //Now lets re-authenticate and connect to the project as a manager
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets add a new incident, an open issue and a closed bug (mapped to a test run step)
            //Lets send in a creation-date for one and leave one blank
            //Incident #1
            RemoteIncident remoteIncident = new RemoteIncident();
            remoteIncident.ProjectId = projectId1;
            remoteIncident.IncidentTypeId = incidentTypeId;
            remoteIncident.IncidentStatusId = incidentStatusId;
            remoteIncident.Name = "New Incident";
            remoteIncident.Description = "This is a test new incident";
            remoteIncident.CreationDate = DateTime.Parse("1/5/2005");
            remoteIncident.ComponentIds = new List<int>() { componentId1, componentId2 };
            //Add some custom property values
            remoteIncident.CustomProperties = new List<RemoteArtifactCustomProperty>();
            remoteIncident.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 1, StringValue = "test value" });
            remoteIncident.CustomProperties.Add(new RemoteArtifactCustomProperty() { PropertyNumber = 2, IntegerValue = customValueId2 });
            incidentId1 = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;
            //Incident #2
            remoteIncident = new RemoteIncident();
            remoteIncident.ProjectId = projectId1;
            remoteIncident.IncidentTypeId = incidentTypeId;
            remoteIncident.IncidentStatusId = incidentStatusId;
            remoteIncident.PriorityId = priorityId;
            remoteIncident.Name = "Open Issue";
            remoteIncident.Description = "This is a test open issue";
            remoteIncident.OpenerId = userId1;
            remoteIncident.OwnerId = userId2;
            remoteIncident.ComponentIds = new List<int>() { componentId2, componentId3 };
            incidentId2 = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;
            //Incident #3
            remoteIncident = new RemoteIncident();
            remoteIncident.ProjectId = projectId1;
            remoteIncident.IncidentTypeId = incidentTypeId;
            remoteIncident.IncidentStatusId = incidentStatusId;
            remoteIncident.PriorityId = priorityId;
            remoteIncident.SeverityId = severityId;
            remoteIncident.Name = "Closed Bug";
            remoteIncident.Description = "This is a test closed bug";
            remoteIncident.OpenerId = userId1;
            remoteIncident.OwnerId = userId2;
            remoteIncident.TestRunStepIds = new List<int>() { testRunStepId2 };
            remoteIncident.ClosedDate = DateTime.UtcNow;
            incidentId3 = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;

            //Finally add some comments to the incidents
            RemoteComment[] remoteComments = new RemoteComment[2];
            remoteComments[0] = new RemoteComment();
            remoteComments[0].ArtifactId = incidentId3;
            remoteComments[0].CreationDate = DateTime.UtcNow.AddSeconds(-2);
            remoteComments[0].Text = "Resolution 1";
            remoteComments[0].UserId = userId1;
            remoteComments[1] = new RemoteComment();
            remoteComments[1].ArtifactId = incidentId3;
            remoteComments[1].CreationDate = DateTime.UtcNow.AddSeconds(2);
            remoteComments[1].Text = "Resolution 2";
            remoteComments[1].UserId = userId1;
            spiraSoapClient.Incident_AddComments(credentials, projectId1, incidentId3, remoteComments);

            //Now verify that the import was successful

            //Retrieve the first incident
            remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId1);
            Assert.AreEqual(3, remoteIncident.ArtifactTypeId);
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "");
            Assert.AreEqual("New Incident", remoteIncident.Name, "Name");
            Assert.AreEqual(DateTime.Parse("1/5/2005"), remoteIncident.CreationDate, "CreationDate");
            Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.IsNull(remoteIncident.OwnerName, "OwnerName");
            remoteComments = spiraSoapClient.Incident_RetrieveComments(credentials, projectId1, incidentId1);
            Assert.AreEqual(0, remoteComments.Length);
            Assert.IsTrue(remoteIncident.ComponentIds.Contains(componentId1));
            Assert.IsTrue(remoteIncident.ComponentIds.Contains(componentId2));

            //Retrieve the second incident
            remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId2);
            Assert.AreEqual(3, remoteIncident.ArtifactTypeId);
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
            Assert.AreEqual("Open Issue", remoteIncident.Name, "Name");
            Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.AreEqual(remoteIncident.OwnerName, "Martha T Muffin", "OwnerName");
            remoteComments = spiraSoapClient.Incident_RetrieveComments(credentials, projectId1, incidentId2);
            Assert.AreEqual(0, remoteComments.Length);

            //Retrieve the third incident
            remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId3);
            Assert.AreEqual(3, remoteIncident.ArtifactTypeId);
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
            Assert.AreEqual(priorityId, remoteIncident.PriorityId, "PriorityId");
            Assert.AreEqual(severityId, remoteIncident.SeverityId, "SeverityId");
            Assert.AreEqual("Closed Bug", remoteIncident.Name, "Name");
            Assert.IsNotNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.AreEqual("Martha T Muffin", remoteIncident.OwnerName, "OwnerName");
            remoteComments = spiraSoapClient.Incident_RetrieveComments(credentials, projectId1, incidentId3);
            Assert.AreEqual(2, remoteComments.Length);
            Assert.AreEqual("Resolution 1", remoteComments[0].Text);
            Assert.AreEqual("Resolution 2", remoteComments[1].Text);

            //Finally make an update to one of the incidents
            remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId3);
            remoteIncident.Name = "Reopened Bug";
            remoteIncident.ClosedDate = null;
            remoteIncident.OwnerId = userId1;
            spiraSoapClient.Incident_Update(credentials, projectId1, incidentId3, remoteIncident);

			//Verify the change
			remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId3);
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");
            Assert.AreEqual(priorityId, remoteIncident.PriorityId, "PriorityId");
            Assert.AreEqual(severityId, remoteIncident.SeverityId, "SeverityId");
            Assert.AreEqual("Reopened Bug", remoteIncident.Name, "Name");
            Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.AreEqual("Adam Ant", remoteIncident.OwnerName, "OwnerName");
            remoteComments = spiraSoapClient.Incident_RetrieveComments(credentials, projectId1, incidentId3);
            Assert.AreEqual(2, remoteComments.Length);
            Assert.AreEqual("Resolution 1", remoteComments[0].Text);
            Assert.AreEqual("Resolution 2", remoteComments[1].Text);

			//Test that we can retrieve incidents by Test Run Step, Test Step, and Test Case
			//Test Run Step
			RemoteIncident[] remoteIncidents = spiraSoapClient.Incident_RetrieveByTestRunStep(credentials, projectId1, testRunStepId2);
            Assert.AreEqual(1, remoteIncidents.Length);
            Assert.AreEqual(incidentId3, remoteIncidents[0].IncidentId);
            //Test Step
            remoteIncidents = spiraSoapClient.Incident_RetrieveByTestStep(credentials, projectId1, testStepId2);
            Assert.AreEqual(1, remoteIncidents.Length);
            Assert.AreEqual(incidentId3, remoteIncidents[0].IncidentId);
            //Test Case
            remoteIncidents = spiraSoapClient.Incident_RetrieveByTestCase(credentials, projectId1, testCaseId3, false);
            Assert.AreEqual(0, remoteIncidents.Length);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update an incident status
			remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId3);
			remoteIncident.IncidentStatusId = remoteIncidentStatuses[0].IncidentStatusId;
			spiraSoapClient.Incident_Update(credentials, projectId1, incidentId3, remoteIncident);

			//Verify that the status did not change
			remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId3);
			Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");

			//Try adding an incident with a non default status
			remoteIncident = new RemoteIncident();
			remoteIncident.ProjectId = projectId1;
			remoteIncident.IncidentTypeId = incidentTypeId;
			remoteIncident.IncidentStatusId = incidentStatusId;
			remoteIncident.PriorityId = priorityId;
			remoteIncident.Name = "Trying to update status";
			remoteIncident.Description = "This is a test to set a non default status when that is not supported";
			remoteIncident.OpenerId = userId1;
			remoteIncident.OwnerId = userId2;
			int incidentId4 = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;

			//Verify that the status did not change
			remoteIncident = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId4);
			Assert.AreEqual(spiraSoapClient.Incident_RetrieveDefaultStatus(credentials, projectTemplateId1).IncidentStatusId, remoteIncident.IncidentStatusId, "IncidentStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			spiraSoapClient.Incident_Delete(credentials, projectId1, incidentId4);
		}

        /// <summary>
        /// Verifies that you can import attachments to project artifacts
        /// </summary>
        [
        Test,
        SpiraTestCase(1989)
        ]
        public void _15_ImportAttachments()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets try adding two attachments to a test case
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

            //Attachment 1
            byte[] attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored");
            RemoteDocumentFile remoteDocumentFile = new RemoteDocumentFile();
            remoteDocumentFile.ProjectId = projectId1;
            remoteDocumentFile.FilenameOrUrl = "test_data.xls";
            remoteDocumentFile.Description = "Sample Test Case Attachment";
            remoteDocumentFile.AuthorId = userId2;
            RemoteLinkedArtifact remoteLinkedArtifact = new RemoteLinkedArtifact();
            remoteLinkedArtifact.ArtifactId = testCaseId1;
            remoteLinkedArtifact.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
            remoteDocumentFile.AttachedArtifacts = new List<RemoteLinkedArtifact>() { remoteLinkedArtifact };
            remoteDocumentFile.BinaryData = attachmentData;
            attachmentId1 = spiraSoapClient.Document_AddFile(credentials, projectId1, remoteDocumentFile).AttachmentId.Value;

            //Attachment 2
            attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored 2");
            remoteDocumentFile = new RemoteDocumentFile();
            remoteDocumentFile.ProjectId = projectId1;
            remoteDocumentFile.FilenameOrUrl = "test_data2.xls";
            remoteDocumentFile.Description = "Sample Test Case Attachment 2";
            remoteDocumentFile.AuthorId = userId2;
            remoteDocumentFile.AttachedArtifacts = new List<RemoteLinkedArtifact>() { remoteLinkedArtifact };
            remoteDocumentFile.BinaryData = attachmentData;
            attachmentId2 = spiraSoapClient.Document_AddFile(credentials, projectId1, remoteDocumentFile).AttachmentId.Value;

            //Now lets get the attachment meta-data and verify
            RemoteDocument remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId1);
            Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Store the folder id for later
            projectAttachmentFolderId = remoteDocument.ProjectAttachmentFolderId.Value;

            //Need to test the URL method
            remoteDocument = new RemoteDocument();
            remoteDocument.ProjectId = projectId1;
            remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
            remoteDocument.Description = "Sample Test Case URL";
            remoteDocument.AuthorId = userId2;
            remoteLinkedArtifact = new RemoteLinkedArtifact();
            remoteLinkedArtifact.ArtifactId = testCaseId2;
            remoteLinkedArtifact.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
            remoteDocument.AttachedArtifacts = new List<RemoteLinkedArtifact>() { remoteLinkedArtifact };
            attachmentId3 = spiraSoapClient.Document_AddUrl(credentials, projectId1, remoteDocument).AttachmentId.Value;

            //Now lets get the attachment meta-data and verify
            remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId3);
            Assert.AreEqual("http://www.tempuri.org/test123.htm", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case URL", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("URL", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(0, remoteDocument.Size);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			AttachmentManager attachmentManager = new AttachmentManager();
			List<DocumentStatus> documentStatuses = attachmentManager.DocumentStatus_Retrieve(projectTemplateId1);
			documentStatuses = documentStatuses.Where(s => !s.IsDefault).ToList();

			//Try adding an artifact with a non default status
			attachmentData = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored 3");
			remoteDocumentFile = new RemoteDocumentFile();
			remoteDocumentFile.ProjectId = projectId1;
			remoteDocumentFile.FilenameOrUrl = "test_data3.xls";
			remoteDocumentFile.Description = "Sample Test Case Attachment 3";
			remoteDocumentFile.AuthorId = userId2;
			remoteLinkedArtifact = new RemoteLinkedArtifact();
			remoteLinkedArtifact.ArtifactId = testCaseId1;
			remoteLinkedArtifact.ArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			remoteDocumentFile.AttachedArtifacts = new List<RemoteLinkedArtifact>() { remoteLinkedArtifact };
			remoteDocumentFile.BinaryData = attachmentData;
			int attachmentId4 = spiraSoapClient.Document_AddFile(credentials, projectId1, remoteDocumentFile).AttachmentId.Value; ;

			//Verify that the status did not change
			DocumentStatus defaultDocumentStatus = attachmentManager.DocumentStatusRetrieveDefault(projectTemplateId1);
			remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId4);
			Assert.AreEqual(defaultDocumentStatus.DocumentStatusId, remoteDocument.DocumentStatusId, "DocumentStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			spiraSoapClient.Document_Delete(credentials, projectId1, attachmentId4);
		}

        /// <summary>
        /// Tests that we can import tasks and their folders into SpiraTeam
        /// </summary>
        [
        Test,
        SpiraTestCase(1994)
        ]
        public void _16_ImportTasks()
        {
            int tk_priorityMediumId = new TaskManager().TaskPriority_Retrieve(projectTemplateId1).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
            int tk_typeDevelopmentId = new TaskManager().TaskType_Retrieve(projectTemplateId1).FirstOrDefault(t => t.Name == "Development").TaskTypeId;
            int tk_typeInfrastructureId = new TaskManager().TaskType_Retrieve(projectTemplateId1).FirstOrDefault(t => t.Name == "Infrastructure").TaskTypeId;

            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Lets add two nested task folders in which we can add some tasks
            //Folder A
            RemoteTaskFolder remoteTaskFolder = new RemoteTaskFolder();
            remoteTaskFolder.ProjectId = projectId1;
            remoteTaskFolder.Name = "Task Folder A";
            remoteTaskFolder = spiraSoapClient.Task_CreateFolder(credentials, projectId1, remoteTaskFolder);
            taskFolderId1 = remoteTaskFolder.TaskFolderId.Value;
            //Folder B
            remoteTaskFolder = new RemoteTaskFolder();
            remoteTaskFolder.ProjectId = projectId1;
            remoteTaskFolder.Name = "Task Folder B";
            remoteTaskFolder.ParentTaskFolderId = taskFolderId1;
            remoteTaskFolder = spiraSoapClient.Task_CreateFolder(credentials, projectId1, remoteTaskFolder);
            taskFolderId2 = remoteTaskFolder.TaskFolderId.Value;

            //Now lets add a task that has all values set
            RemoteTask remoteTask = new RemoteTask();
            remoteTask.ProjectId = projectId1;
            remoteTask.Name = "New Task 1";
            remoteTask.Description = "Task 1 Description";
            remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
            remoteTask.TaskPriorityId = tk_priorityMediumId;
            remoteTask.TaskTypeId = tk_typeDevelopmentId;
            remoteTask.TaskFolderId = taskFolderId1;
            remoteTask.RequirementId = requirementId1;
            remoteTask.ReleaseId = iterationId2;
            remoteTask.OwnerId = userId2;
            remoteTask.StartDate = DateTime.UtcNow;
            remoteTask.EndDate = DateTime.UtcNow.AddDays(5);
            remoteTask.RemainingEffort = 75;
            remoteTask.EstimatedEffort = 100;
            remoteTask.ActualEffort = 30;
            taskId1 = spiraSoapClient.Task_Create(credentials, projectId1, remoteTask).TaskId.Value;

            //Verify the data
            remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId1);
            Assert.AreEqual("New Task 1", remoteTask.Name);
            Assert.AreEqual("Task 1 Description", remoteTask.Description);
            Assert.AreEqual("In Progress", remoteTask.TaskStatusName);
            Assert.AreEqual("3 - Medium", remoteTask.TaskPriorityName);
            Assert.AreEqual("1.0.002b", remoteTask.ReleaseVersionNumber);
            Assert.AreEqual(requirementId1, remoteTask.RequirementId);
            Assert.AreEqual(taskFolderId1, remoteTask.TaskFolderId);
            Assert.IsNotNull(remoteTask.StartDate);
            Assert.IsNotNull(remoteTask.EndDate);
            Assert.AreEqual(25, remoteTask.CompletionPercent);
            Assert.AreEqual(100, remoteTask.EstimatedEffort);
            Assert.AreEqual(30, remoteTask.ActualEffort);
            Assert.AreEqual(105, remoteTask.ProjectedEffort);
            Assert.AreEqual(75, remoteTask.RemainingEffort);

            //Now lets add a task that has some values null
            remoteTask = new RemoteTask();
            remoteTask.ProjectId = projectId1;
            remoteTask.Name = "New Task 2";
            remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
            remoteTask.TaskTypeId = tk_typeInfrastructureId;
            remoteTask.TaskFolderId = taskFolderId2;
            taskId2 = spiraSoapClient.Task_Create(credentials, projectId1, remoteTask).TaskId.Value;

            //Verify the data
            remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
            Assert.AreEqual(6, remoteTask.ArtifactTypeId);
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
            Assert.AreEqual(taskFolderId2, remoteTask.TaskFolderId);

            //Finally make an update to one of the tasks
            remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
            Assert.AreEqual(6, remoteTask.ArtifactTypeId);
            remoteTask.Name = "New Task 2b";
            remoteTask.Description = "New Task 2b Description";
            remoteTask.ReleaseId = iterationId1;
            remoteTask.StartDate = DateTime.UtcNow.AddDays(1).Date;
            remoteTask.EndDate = DateTime.UtcNow.AddDays(5).Date;
			spiraSoapClient.Task_Update(credentials, projectId1, remoteTask);

            //Verify the change
            remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
            Assert.AreEqual(6, remoteTask.ArtifactTypeId);
            Assert.AreEqual("New Task 2b", remoteTask.Name);
            Assert.AreEqual("New Task 2b Description", remoteTask.Description);
            Assert.AreEqual("Not Started", remoteTask.TaskStatusName);
            Assert.IsNull(remoteTask.TaskPriorityName);
            Assert.AreEqual("1.0.001", remoteTask.ReleaseVersionNumber);
            Assert.IsNull(remoteTask.RequirementId);
            Assert.AreEqual(DateTime.UtcNow.AddDays(1).Date, remoteTask.StartDate);
            Assert.AreEqual(DateTime.UtcNow.AddDays(5).Date, remoteTask.EndDate);
            Assert.AreEqual(0, remoteTask.CompletionPercent);
            Assert.IsNull(remoteTask.EstimatedEffort);
            Assert.IsNull(remoteTask.ActualEffort);
            Assert.IsNull(remoteTask.RemainingEffort);
            Assert.IsNull(remoteTask.ProjectedEffort);

            //Test that we can retrieve the folder hierarchy
            RemoteTaskFolder[] taskFolders = spiraSoapClient.Task_RetrieveFolders(credentials, projectId1);
            Assert.AreEqual(2, taskFolders.Length);
            Assert.AreEqual("Task Folder A", taskFolders[0].Name);
            Assert.AreEqual("Task Folder B", taskFolders[1].Name);

            //Test that we can retrieve a list of folders by parent
            taskFolders = spiraSoapClient.Task_RetrieveFoldersByParent(credentials, projectId1, null);
            Assert.AreEqual(1, taskFolders.Length);
            Assert.AreEqual("Task Folder A", taskFolders[0].Name);
            taskFolders = spiraSoapClient.Task_RetrieveFoldersByParent(credentials, projectId1, taskFolderId1);
            Assert.AreEqual(1, taskFolders.Length);
            Assert.AreEqual("Task Folder B", taskFolders[0].Name);

            //Verify that we can update one of the folders
            remoteTaskFolder = spiraSoapClient.Task_RetrieveFolderById(credentials, projectId1, taskFolderId1);
            remoteTaskFolder.Name = "Task Folder A1";
            spiraSoapClient.Task_UpdateFolder(credentials, projectId1, remoteTaskFolder);

            //Verify the update
            remoteTaskFolder = spiraSoapClient.Task_RetrieveFolderById(credentials, projectId1, taskFolderId1);
            Assert.AreEqual("Task Folder A1", remoteTaskFolder.Name);

            //Delete the folder and verify the tasks move to the root folder
            spiraSoapClient.Task_DeleteFolder(credentials, projectId1, taskFolderId2);
            remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
            Assert.IsNull(remoteTask.TaskFolderId);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update a status
			remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
			spiraSoapClient.Task_Update(credentials, projectId1, remoteTask);

			//Verify that the status did not change
			remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId2);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, remoteTask.TaskStatusId, "TaskStatusId");

			//Try adding an artifact with a non default status
			remoteTask = new RemoteTask();
			remoteTask.ProjectId = projectId1;
			remoteTask.Name = "New Task 3";
			remoteTask.Description = "Task 3 Description";
			remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.InProgress;
			remoteTask.TaskPriorityId = tk_priorityMediumId;
			remoteTask.TaskTypeId = tk_typeDevelopmentId;
			remoteTask.TaskFolderId = taskFolderId1;
			remoteTask.OwnerId = userId2;
			int taskId3 = spiraSoapClient.Task_Create(credentials, projectId1, remoteTask).TaskId.Value;

			//Verify that the status is set to the default
			remoteTask = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId3);
			Assert.AreEqual((int)Task.TaskStatusEnum.NotStarted, remoteTask.TaskStatusId, "TaskStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			spiraSoapClient.Task_Delete(credentials, projectId1, taskId3);
		}

        /// <summary>
        /// Verifies that you can retrieve the incidents from a specific date
        /// </summary>
        [
        Test,
        SpiraTestCase(2007)
        ]
        public void _17_RetrieveIncidents()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Now lets retrieve all incidents since a certain date
            RemoteIncident[] remoteIncidents = spiraSoapClient.Incident_RetrieveNew(credentials, projectId1, 1, 100, DateTime.Parse("1/1/2000"));

            //Verify the data returned
            Assert.AreEqual(3, remoteIncidents.Length);
            RemoteIncident remoteIncident = remoteIncidents[0];
            Assert.AreEqual(incidentTypeId, remoteIncident.IncidentTypeId, "IncidentTypeId");
            Assert.AreEqual(incidentStatusId, remoteIncident.IncidentStatusId, "");
            Assert.AreEqual("New Incident", remoteIncident.Name, "Name");
            Assert.IsNull(remoteIncident.ClosedDate, "IsClosedDateNull");
            Assert.AreEqual("Adam Ant", remoteIncident.OpenerName, "OpenerName");
            Assert.IsNull(remoteIncident.OwnerName, "OwnerName");
            RemoteComment[] remoteComments = spiraSoapClient.Incident_RetrieveComments(credentials, projectId1, incidentId1);
            Assert.AreEqual(0, remoteComments.Length);

            //Now lets test that we can retrieve a generic filtered list of P1,P2 open incidents from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

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
            remoteIncidents = spiraSoapClient.Incident_Retrieve3(credentials, PROJECT_ID, 1, 999999, "Name ASC", remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(13, remoteIncidents.Length);
            Assert.AreEqual("Ability to associate multiple authors", remoteIncidents[0].Name);
            Assert.AreEqual("Test Training Item", remoteIncidents[11].Name);

            //Now test that we can get a list of incidents assigned to the currently authenticated user
            remoteIncidents = spiraSoapClient.Incident_RetrieveForOwner(credentials);

            //Verify the data returned
            Assert.AreEqual(9, remoteIncidents.Length);
            Assert.AreEqual("Ability to associate multiple authors", remoteIncidents[0].Name);
            Assert.AreEqual("Sample Problem 3", remoteIncidents[8].Name);
        }

        /// <summary>
        /// Verifies that you can retrieve the tasks from a specific date
        /// </summary>
        [
        Test,
        SpiraTestCase(2013)
        ]
        public void _18_RetrieveTasks()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Now lets retrieve all tasks since a certain date
            RemoteTask[] remoteTasks = spiraSoapClient.Task_RetrieveNew(credentials, projectId1, DateTime.Parse("1/1/2000"), 1, 100);

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


            //Now lets test that we can retrieve a generic filtered list of P1,P2 completed Release 1 tasks from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

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
            remoteTasks = spiraSoapClient.Task_Retrieve(credentials, PROJECT_ID, 1, 999999, "TaskId", true, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(15, remoteTasks.Length);
            Assert.AreEqual("Develop new book entry screen", remoteTasks[0].Name);
            Assert.AreEqual("Write book object delete query", remoteTasks[8].Name);

            //Now test that we can get a list of tasks assigned to the currently authenticated user
            remoteTasks = spiraSoapClient.Task_RetrieveForOwner(credentials);

            //Verify the data returned
            Assert.AreEqual(5, remoteTasks.Length);
            Assert.AreEqual("Schedule meeting with customer to discuss scope", remoteTasks[0].Name);
            Assert.AreEqual("Write subject object update queries", remoteTasks[4].Name);
        }

        /// <summary>
        /// Verifies that you can retrieve filtered lists of requirements
        /// </summary>
        [
        Test,
        SpiraTestCase(2011)
        ]
        public void _19_RetrieveRequirements()
        {
            //Now lets test that we can retrieve a generic list of requirements from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

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
            RemoteRequirement[] remoteRequirements = spiraSoapClient.Requirement_Retrieve1(credentials, PROJECT_ID, 1, 999999, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(4, remoteRequirements.Length);
            Assert.AreEqual("Functional System Requirements", remoteRequirements[0].Name);
            Assert.AreEqual("Ability to delete existing books in the system", remoteRequirements[3].Name);

            //Now test that we can get a list of requirements assigned to the currently authenticated user
            remoteRequirements = spiraSoapClient.Requirement_RetrieveForOwner(credentials);

            //Verify the data returned
            Assert.AreEqual(4, remoteRequirements.Length);
            Assert.AreEqual("Ability to edit existing authors in the system", remoteRequirements[0].Name);
            Assert.AreEqual("Ability to import from legacy system x", remoteRequirements[3].Name);

            //Verify that we can get the requirement types and statuses
            RemoteRequirementType[] requirementTypes = spiraSoapClient.Requirement_RetrieveTypes(credentials, 1);
            Assert.AreEqual(7, requirementTypes.Length);
            Assert.IsTrue(requirementTypes.Any(r => r.Name == "Feature"));
            Assert.IsTrue(requirementTypes.Any(r => r.Name == "Quality"));

            RemoteRequirementStatus[] requirementStatuses = spiraSoapClient.Requirement_RetrieveStatuses(credentials, PROJECT_TEMPLATE_ID);
            Assert.AreEqual(16, requirementStatuses.Length);
            Assert.IsTrue(requirementStatuses.Any(r => r.Name == "Requested"));
            Assert.IsTrue(requirementStatuses.Any(r => r.Name == "Completed"));
        }

        /// <summary>
        /// Verifies that you can retrieve filtered lists of test cases
        /// </summary>
        [
        Test,
        SpiraTestCase(2014)
        ]
        public void _20_RetrieveTestCases()
        {
            //Now lets test that we can retrieve a generic list of test cases and test runs from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

            List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
            RemoteFilter remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "OwnerId";
            remoteFilter.MultiValue = new MultiValueFilter();
            remoteFilter.MultiValue.Values = new int[] { 2, 3 };
            remoteFilters.Add(remoteFilter);
            remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "TestCaseStatusId";
            remoteFilter.MultiValue = new MultiValueFilter();
            remoteFilter.MultiValue.Values = new int[] { /*Approved*/4, /*ReadyForTest*/5 };
            remoteFilters.Add(remoteFilter);
            remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "ExecutionDate";
            remoteFilter.DateRangeValue = new DateRange();
            remoteFilter.DateRangeValue.StartDate = DateTime.UtcNow.AddDays(-150);
            remoteFilter.DateRangeValue.EndDate = DateTime.UtcNow;
            remoteFilters.Add(remoteFilter);
            RemoteTestCase[] remoteTestCases = spiraSoapClient.TestCase_Retrieve2(credentials, PROJECT_ID, 1, 999999, "Name", true, null, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(5, remoteTestCases.Length);
            Assert.AreEqual("Ability to create new author", remoteTestCases[0].Name);
            Assert.AreEqual(1, remoteTestCases[0].TestCaseFolderId);
            Assert.AreEqual("Ability to create new book", remoteTestCases[1].Name);
            Assert.AreEqual(1, remoteTestCases[1].TestCaseFolderId);
            Assert.AreEqual("Ability to reassign book to different author", remoteTestCases[4].Name);
            Assert.AreEqual(1, remoteTestCases[4].TestCaseFolderId);

            //Now test that we can get the list of test cases in a specific release (filtered)
            remoteTestCases = spiraSoapClient.TestCase_Retrieve2(credentials, PROJECT_ID, 1, 999999, "Name", true, 1, remoteFilters.ToArray());
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
            RemoteTestRun[] remoteTestRuns = spiraSoapClient.TestRun_Retrieve2(credentials, PROJECT_ID, 1, 999999, "EndDate", false, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(4, remoteTestRuns.Length);
            Assert.IsTrue(remoteTestRuns[0].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[1].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[2].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));
            Assert.IsTrue(remoteTestRuns[3].EndDate.Value.Date > DateTime.UtcNow.AddDays(-150));

            //Now test that we can get a list of test cases assigned to the currently authenticated user
            remoteTestCases = spiraSoapClient.TestCase_RetrieveForOwner(credentials);

            //Verify the data returned
            Assert.AreEqual(2, remoteTestCases.Length);
            Assert.AreEqual("Ability to create new book", remoteTestCases[0].Name);
            Assert.AreEqual("Ability to edit existing book", remoteTestCases[1].Name);

            //Now lets test that we can get the list of test case in a test folder
            remoteTestCases = spiraSoapClient.TestCase_RetrieveByFolder1(credentials, PROJECT_ID, 1, 1, 999999, "Name", true, null);

            //Verify the data returned
            Assert.AreEqual(5, remoteTestCases.Length);
            Assert.AreEqual("Ability to create new author", remoteTestCases[0].Name);
            Assert.AreEqual(1, remoteTestCases[0].TestCaseFolderId);
            Assert.AreEqual("Ability to reassign book to different author", remoteTestCases[4].Name);
            Assert.AreEqual(1, remoteTestCases[4].TestCaseFolderId);

            //Verify that we can get the test case types and statuses
            RemoteTestCaseType[] testCaseTypes = spiraSoapClient.TestCase_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID);
            Assert.AreEqual(12, testCaseTypes.Length);
            Assert.IsTrue(testCaseTypes.Any(r => r.Name == "Usability"));
            Assert.IsTrue(testCaseTypes.Any(r => r.Name == "Functional"));
            Assert.IsTrue(testCaseTypes.Any(r => r.Name == "Exploratory"));

            RemoteTestCaseStatus[] testCaseStatuses = spiraSoapClient.TestCase_RetrieveStatuses(credentials, PROJECT_TEMPLATE_ID);
            Assert.AreEqual(8, testCaseStatuses.Length);
            Assert.IsTrue(testCaseStatuses.Any(r => r.Name == "Draft"));
            Assert.IsTrue(testCaseStatuses.Any(r => r.Name == "Approved"));
        }

        /// <summary>
        /// Verifies that you can retrieve filtered lists of releases
        /// </summary>
        [
        Test,
        SpiraTestCase(2010)
        ]
        public void _21_RetrieveReleases()
        {
            //Now lets test that we can retrieve a generic list of releases from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

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
            RemoteRelease[] remoteReleases = spiraSoapClient.Release_Retrieve2(credentials, PROJECT_ID, 1, 999999, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(2, remoteReleases.Length);
            Assert.AreEqual("Library System Release 1.1", remoteReleases[0].Name);
            Assert.AreEqual("Library System Release 1.1 SP1", remoteReleases[1].Name);

            //Verify that we can get the release types and statuses
            RemoteReleaseType[] releaseTypes = spiraSoapClient.Release_RetrieveTypes(credentials, 1);
            Assert.AreEqual(4, releaseTypes.Length);
            Assert.IsTrue(releaseTypes.Any(r => r.Name == "Major Release"));
            Assert.IsTrue(releaseTypes.Any(r => r.Name == "Sprint"));

            RemoteReleaseStatus[] releaseStatuses = spiraSoapClient.Release_RetrieveStatuses(credentials, PROJECT_TEMPLATE_ID);
            Assert.AreEqual(6, releaseStatuses.Length);
            Assert.IsTrue(releaseStatuses.Any(r => r.Name == "Planned"));
            Assert.IsTrue(releaseStatuses.Any(r => r.Name == "Cancelled"));
        }

        /// <summary>
        /// Verifies that you can retrieve filtered lists of test sets
        /// </summary>
        [
        Test,
        SpiraTestCase(2016)
        ]
        public void _22_RetrieveTestSets()
        {
            //Now lets test that we can retrieve a generic list of test sets from the sample project
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

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
            RemoteTestSet[] remoteTestSets = spiraSoapClient.TestSet_Retrieve2(credentials, PROJECT_ID, 1, 999999, null, "Name", true, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(3, remoteTestSets.Length);
            Assert.AreEqual("Testing Cycle for Release 1.0", remoteTestSets[0].Name);
            Assert.AreEqual(1, remoteTestSets[0].TestSetFolderId);
            Assert.AreEqual("Testing Cycle for Release 1.1", remoteTestSets[1].Name);
            Assert.AreEqual(1, remoteTestSets[1].TestSetFolderId);
            Assert.AreEqual("Testing New Functionality", remoteTestSets[2].Name);
            Assert.AreEqual(1, remoteTestSets[2].TestSetFolderId);

            //Now verify we can retrieve for a specific release
            remoteFilters = new List<RemoteFilter>();
            remoteFilter = new RemoteFilter();
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
            remoteTestSets = spiraSoapClient.TestSet_Retrieve2(credentials, PROJECT_ID, 1, 999999, 1, "Name", true, remoteFilters.ToArray());

            //Verify the data returned
            Assert.AreEqual(3, remoteTestSets.Length);
            Assert.AreEqual("Testing Cycle for Release 1.0", remoteTestSets[0].Name);
            Assert.AreEqual(1, remoteTestSets[0].TestSetFolderId);
            Assert.AreEqual("Testing Cycle for Release 1.1", remoteTestSets[1].Name);
            Assert.AreEqual(1, remoteTestSets[1].TestSetFolderId);
            Assert.AreEqual("Testing New Functionality", remoteTestSets[2].Name);
            Assert.AreEqual(1, remoteTestSets[2].TestSetFolderId);

            //Now test that we can get a list of test sets assigned to the currently authenticated user
            remoteTestSets = spiraSoapClient.TestSet_RetrieveForOwner(credentials);

            //Verify the data returned
            Assert.AreEqual(3, remoteTestSets.Length);
            Assert.AreEqual("Regression Testing for Windows 8", remoteTestSets[0].Name);
            Assert.AreEqual("Exploratory Testing", remoteTestSets[1].Name);
            Assert.AreEqual("Testing New Functionality", remoteTestSets[2].Name);
        }

        /// <summary>
        /// Verifies that the System_Version() function returns a non-null value.
        /// </summary>
        [
        Test,
        SpiraTestCase(2017)
        ]
        public void _23_RetrieveVersion()
        {
            // Log in, tho this may not even be necessary.
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);
            // Pull the version object.
            RemoteVersion remoteVersion = spiraSoapClient.System_GetProductVersion(credentials);
            // Verify they meet requirements.
            Assert.IsTrue(remoteVersion.Version != null, "Version string was null!");
            Assert.IsTrue(remoteVersion.Version.Length >= 3, "Version string was too short!");
            Assert.IsTrue(remoteVersion.Patch.HasValue, "Patch version was null!");

        }

        /// <summary>
        /// Verifies that settings are pulled from the system.
        /// </summary>
        [
        Test,
        SpiraTestCase(2012)
        ]
        public void _24_RetrieveSettings()
        {
            // Log in, though this may not even be necessary.
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);
            // Pull the version object.
            RemoteSetting[] remoteSettings = spiraSoapClient.System_GetSettings(credentials);
            // Verify they meet requirements.
            Assert.IsTrue(remoteSettings.Length > 0, "There were no settings returned!");

            //Verify that we can retrieve artifact urls for generating inbound links to SpiraTest
            //The base url to Requirement 5 in project 2, set to the Comments tab
            string url = spiraSoapClient.System_GetArtifactUrl(credentials, 1, 2, 5, "Comments");
            Assert.AreEqual("~/2/Requirement/5/Comments.aspx", url);

            //The base url to an unspecified Test Case in project 3, set to the Test Steps tab
            url = spiraSoapClient.System_GetArtifactUrl(credentials, 2, 3, -2, "TestSteps");
            Assert.AreEqual("~/3/TestCase/{art}/TestSteps.aspx", url);

			//Test that we can call the notification service (does nothing but makes sure it returns)
			spiraSoapClient.System_ProcessNotifications(credentials);
        }

        /// <summary>
        /// Tests that we can retrieve a list of project roles
        /// </summary>
        [
        Test,
        SpiraTestCase(2009)
        ]
        public void _25_RetrieveProjectRoles()
        {
            // Authenticate with the API
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

            //First test that we can retrieve the list of all project roles
            RemoteProjectRole[] remoteProjectRoles = spiraSoapClient.ProjectRole_Retrieve(credentials);
            Assert.AreEqual(6, remoteProjectRoles.Length);
            Assert.AreEqual(InternalRoutines.PROJECT_ROLE_PROJECT_OWNER, remoteProjectRoles[0].ProjectRoleId);
            Assert.AreEqual("Product Owner", remoteProjectRoles[0].Name);
            Assert.AreEqual("Can see all product artifacts. Can create/modify all artifacts. Can access the product/template administration tools", remoteProjectRoles[0].Description);
            Assert.IsTrue(remoteProjectRoles[0].Active, "Active");
            Assert.IsTrue(remoteProjectRoles[0].Admin, "Admin");
            Assert.IsTrue(remoteProjectRoles[0].DiscussionsAdd, "DiscussionsAdd");

            //Verify the permissions
            Assert.AreEqual(52, remoteProjectRoles[0].Permissions.Count);
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestSet && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Release && p.PermissionId == (int)Project.PermissionEnum.Create));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestCase && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.TestSet && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Task && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Incident && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Document && p.PermissionId == (int)Project.PermissionEnum.Modify));
            Assert.IsTrue(remoteProjectRoles[0].Permissions.Any(p => p.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Release && p.PermissionId == (int)Project.PermissionEnum.Modify));
        }

        /// <summary>
        /// Tests that the API handles data concurrency correctly
        /// </summary>
        [
        Test,
        SpiraTestCase(1979)
        ]
        public void _26_Concurrency_Handling()
        {
            //Connect to the pre-existing project 2
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the template associated with the project
            int projectTemplateId2 = new TemplateManager().RetrieveForProject(projectId1).ProjectTemplateId;
            int tk_priorityLowId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 4).TaskPriorityId;
            int tk_priorityMediumId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 3).TaskPriorityId;
            int tk_priorityHighId = new TaskManager().TaskPriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 2).TaskPriorityId;
            int tk_typeDevelopmentId = new TaskManager().TaskType_Retrieve(projectTemplateId2).FirstOrDefault(t => t.Name == "Development").TaskTypeId;
            int rq_importanceHighId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId2).FirstOrDefault(i => i.Score == 2).ImportanceId;
            int rq_importanceLowId = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId2).FirstOrDefault(i => i.Score == 4).ImportanceId;
            int rq_typeFeatureId = new RequirementManager().RequirementType_Retrieve(projectTemplateId2, false).FirstOrDefault(t => t.Name == "Feature").RequirementTypeId;
            int tc_priorityLowId = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 4).TestCasePriorityId;
            int tc_priorityHighId = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId2).FirstOrDefault(p => p.Score == 2).TestCasePriorityId;
            int tc_typeFunctionalId = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId2, false).FirstOrDefault(t => t.Name == "Functional").TestCaseTypeId;

            /************************* TASKS **********************/

            //Now lets add a task that has all values set
            RemoteTask remoteTask = new RemoteTask();
            remoteTask.ProjectId = projectId1;
            remoteTask.Name = "New Task 1";
            remoteTask.Description = "Task 1 Description";
            remoteTask.TaskStatusId = (int)Task.TaskStatusEnum.NotStarted;
            remoteTask.TaskTypeId = tk_typeDevelopmentId;
            remoteTask.TaskPriorityId = tk_priorityMediumId;
            remoteTask.StartDate = DateTime.UtcNow;
            remoteTask.EndDate = DateTime.UtcNow.AddDays(5);
            remoteTask.EstimatedEffort = 100;
            remoteTask.ActualEffort = 20;
            remoteTask.RemainingEffort = 100;
            int taskId = spiraSoapClient.Task_Create(credentials, projectId1, remoteTask).TaskId.Value;

            //Now retrieve the task back into two copies
            RemoteTask remoteTask1 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);
            RemoteTask remoteTask2 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);

            //Now make a change to field and update
            remoteTask1.TaskPriorityId = tk_priorityHighId;
            spiraSoapClient.Task_Update(credentials, projectId1, remoteTask1);

            //Verify it updated correctly using separate data object
            RemoteTask remoteTask3 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);
            Assert.AreEqual(tk_priorityHighId, remoteTask3.TaskPriorityId);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            bool exceptionThrown = false;
            try
            {
                remoteTask2.TaskPriorityId = tk_priorityLowId;
                spiraSoapClient.Task_Update(credentials, projectId1, remoteTask2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteTask3 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);
            Assert.AreEqual(tk_priorityHighId, remoteTask3.TaskPriorityId);

            //Now refresh the old dataset and try again and verify it works
            remoteTask2 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);
            remoteTask2.TaskPriorityId = tk_priorityLowId;
            spiraSoapClient.Task_Update(credentials, projectId1, remoteTask2);

            //Verify it updated correctly
            remoteTask3 = spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId);
            Assert.AreEqual(tk_priorityLowId, remoteTask3.TaskPriorityId);

            //Clean up (can't use the API as it doesn't have deletes)
            TaskManager task = new TaskManager();
            task.MarkAsDeleted(PROJECT_ID, taskId, USER_ID_FRED_BLOGGS);

            /************************* INCIDENTS **********************/

            //Now lets add a incident that has all values set
            RemoteIncident remoteIncident = new RemoteIncident();
            remoteIncident.ProjectId = projectId1;
            remoteIncident.Name = "New Incident 1";
            remoteIncident.IncidentStatusId = null;   //Signifies the default
            remoteIncident.IncidentTypeId = null;     //Signifies the default
            remoteIncident.Description = "Incident 1 Description";
            remoteIncident.StartDate = DateTime.UtcNow;
            remoteIncident.EstimatedEffort = 100;
            remoteIncident.ActualEffort = 20;
            remoteIncident.ProjectedEffort = 100;
            int incidentId = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;

            //Now retrieve the incident back into two copies
            RemoteIncident remoteIncident1 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);
            RemoteIncident remoteIncident2 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);

            //Now make a change to field and update
            remoteIncident1.Name = "New Incident 1 Mod001";
            spiraSoapClient.Incident_Update(credentials, projectId1, remoteIncident1.IncidentId.Value, remoteIncident1);

            //Verify it updated correctly using separate data object
            RemoteIncident remoteIncident3 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);
            Assert.AreEqual("New Incident 1 Mod001", remoteIncident3.Name);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteIncident2.Name = "New Incident 1 Mod002";
                spiraSoapClient.Incident_Update(credentials, projectId1, remoteIncident2.IncidentId.Value, remoteIncident2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteIncident3 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);
            Assert.AreEqual("New Incident 1 Mod001", remoteIncident3.Name);

            //Now refresh the old dataset and try again and verify it works
            remoteIncident2 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);
            remoteIncident2.Name = "New Incident 1 Mod002";
            spiraSoapClient.Incident_Update(credentials, projectId1, remoteIncident2.IncidentId.Value, remoteIncident2);

            //Verify it updated correctly
            remoteIncident3 = spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId);
            Assert.AreEqual("New Incident 1 Mod002", remoteIncident3.Name);

            //Clean up - delete the incident
            spiraSoapClient.Incident_Delete(credentials, projectId1, incidentId);

            /************************* REQUIREMENTS **********************/
            //First we need to create a new requirement to verify the handling
            RemoteRequirement remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = (int)Requirement.RequirementStatusEnum.Requested;
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement.Name = "Functionality Area";
            remoteRequirement.Description = String.Empty;
            remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
            int requirementId = remoteRequirement.RequirementId.Value;

            //Now retrieve the requirement back into two copies
            RemoteRequirement remoteRequirement1 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);
            RemoteRequirement remoteRequirement2 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);

            //Now make a change to field and update
            remoteRequirement1.ImportanceId = rq_importanceHighId;
            spiraSoapClient.Requirement_Update(credentials, projectId1, remoteRequirement1);

            //Verify it updated correctly using separate dataset
            RemoteRequirement remoteRequirement3 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);
            Assert.AreEqual(rq_importanceHighId, remoteRequirement3.ImportanceId);

            //Now try making a change using the out of date data object (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteRequirement2.ImportanceId = rq_importanceLowId;
                spiraSoapClient.Requirement_Update(credentials, projectId1, remoteRequirement2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate data object
            remoteRequirement3 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);
            Assert.AreEqual(rq_importanceHighId, remoteRequirement3.ImportanceId);

            //Now refresh the old data object and try again and verify it works
            remoteRequirement2 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);
            remoteRequirement2.ImportanceId = rq_importanceLowId;
            spiraSoapClient.Requirement_Update(credentials, projectId1, remoteRequirement2);

            //Verify it updated correctly using separate data object
            remoteRequirement3 = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId);
            Assert.AreEqual(rq_importanceLowId, remoteRequirement3.ImportanceId);

            //Clean up
            spiraSoapClient.Requirement_Delete(credentials, projectId1, requirementId);

            /************************* RELEASES **********************/
            //First we need to create a new release to verify the handling
            RemoteRelease remoteRelease = new RemoteRelease();
            remoteRelease.ProjectId = projectId1;
            remoteRelease.Name = "Release 1";
            remoteRelease.VersionNumber = "1.0";
            remoteRelease.Description = "First version of the system";
            remoteRelease.Active = true;
            remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MajorRelease;
            remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
            remoteRelease.StartDate = DateTime.UtcNow;
            remoteRelease.EndDate = DateTime.UtcNow.AddMonths(3);
            remoteRelease.ResourceCount = 5;
            int releaseId = spiraSoapClient.Release_Create1(credentials, projectId1, remoteRelease).ReleaseId.Value;

            //Now retrieve the release back and keep a copy of the dataset
            RemoteRelease remoteRelease1 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);
            RemoteRelease remoteRelease2 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);

            //Now make a change to field and update
            remoteRelease1.DaysNonWorking = 3;
            spiraSoapClient.Release_Update(credentials, projectId1, remoteRelease1);

            //Verify it updated correctly using separate dataset
            RemoteRelease remoteRelease3 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);
            Assert.AreEqual(3, remoteRelease3.DaysNonWorking);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteRelease2.DaysNonWorking = 4;
                spiraSoapClient.Release_Update(credentials, projectId1, remoteRelease2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteRelease3 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);
            Assert.AreEqual(3, remoteRelease3.DaysNonWorking);

            //Now refresh the old dataset and try again and verify it works
            remoteRelease2 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);
            remoteRelease2.DaysNonWorking = 4;
            spiraSoapClient.Release_Update(credentials, projectId1, remoteRelease2);

            //Verify it updated correctly using separate dataset
            remoteRelease3 = spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId);
            Assert.AreEqual(4, remoteRelease3.DaysNonWorking);

            //Clean up
            spiraSoapClient.Release_Delete(credentials, projectId1, releaseId);

            /************************* TEST SETS **********************/

            //First we need to create a new test set to verify the handling
            RemoteTestSet remoteTestSet = new RemoteTestSet();
            remoteTestSet.ProjectId = projectId1;

            remoteTestSet.Name = "Test Set 1";
            remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
            remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
            int testSetId = spiraSoapClient.TestSet_Create(credentials, projectId1, remoteTestSet).TestSetId.Value;

            //Now retrieve the testSet back and keep a copy of the dataset
            RemoteTestSet remoteTestSet1 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);
            RemoteTestSet remoteTestSet2 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);

            //Now make a change to field and update
            remoteTestSet1.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Blocked;
            spiraSoapClient.TestSet_Update(credentials, projectId1, remoteTestSet1);

            //Verify it updated correctly using separate dataset
            RemoteTestSet remoteTestSet3 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);
            Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, remoteTestSet3.TestSetStatusId);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteTestSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
                spiraSoapClient.TestSet_Update(credentials, projectId1, remoteTestSet2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteTestSet3 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);
            Assert.AreEqual((int)TestSet.TestSetStatusEnum.Blocked, remoteTestSet3.TestSetStatusId);

            //Now refresh the old dataset and try again and verify it works
            remoteTestSet2 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);
            remoteTestSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.Deferred;
            spiraSoapClient.TestSet_Update(credentials, projectId1, remoteTestSet2);

            //Verify it updated correctly using separate dataset
            remoteTestSet3 = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId);
            Assert.AreEqual((int)TestSet.TestSetStatusEnum.Deferred, remoteTestSet3.TestSetStatusId);

            //Clean up
            spiraSoapClient.TestSet_Delete(credentials, projectId1, testSetId);

            /************************* TEST CASES/STEPS **********************/
            //First we need to create a new test case to verify the handling
            RemoteTestCase remoteTestCase = new RemoteTestCase();
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.Name = "Test Case 1";
            remoteTestCase.Description = "Test Case Description 1";
            remoteTestCase.TestCaseTypeId = tc_typeFunctionalId;
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.ReadyForTest;
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            int testCaseId = remoteTestCase.TestCaseId.Value;

            //Now retrieve the testCase back and keep a copy of the dataset
            RemoteTestCase remoteTestCase1 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            RemoteTestCase remoteTestCase2 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);

            //Now make a change to field and update
            remoteTestCase1.TestCasePriorityId = tc_priorityHighId;
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase1);

            //Verify it updated correctly using separate dataset
            RemoteTestCase remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(tc_priorityHighId, remoteTestCase3.TestCasePriorityId);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteTestCase2.TestCasePriorityId = tc_priorityLowId;
                spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(tc_priorityHighId, remoteTestCase3.TestCasePriorityId);

            //Now refresh the old dataset and try again and verify it works
            remoteTestCase2 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            remoteTestCase2.TestCasePriorityId = tc_priorityLowId;
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase2);

            //Verify it updated correctly using separate dataset
            remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(tc_priorityLowId, remoteTestCase3.TestCasePriorityId);

            //Next we need to create a new test step to verify the handling
            RemoteTestStep remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Description = "Desc 1";
            remoteTestStep.ExpectedResult = "Expected 1";
            remoteTestStep.Position = 1;
            spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId, remoteTestStep);

            //Now retrieve the test step back and keep a copy of the dataset
            remoteTestCase1 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            remoteTestCase2 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);

            //Now make a change to field and update
            remoteTestCase1.TestSteps[0].Description = "Desc 2";
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase1);

            //Verify it updated correctly using separate dataset
            remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual("Desc 2", remoteTestCase3.TestSteps[0].Description);

            //Now try making a change using the out of date dataset (has the wrong LastUpdatedDate)
            exceptionThrown = false;
            try
            {
                remoteTestCase2.TestSteps[0].Description = "Desc 3";
                spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase2);
            }
            catch (Exception exception)
            {
                if (exception.Message == "409 Conflict")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown, "exceptionThrown");

            //Verify it didn't update using separate dataset
            remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual("Desc 2", remoteTestCase3.TestSteps[0].Description);

            //Now refresh the old dataset and try again and verify it works
            remoteTestCase2 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            remoteTestCase2.TestSteps[0].Description = "Desc 3";
            spiraSoapClient.TestCase_Update(credentials, projectId1, remoteTestCase2);

            //Verify it updated correctly using separate dataset
            remoteTestCase3 = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual("Desc 3", remoteTestCase3.TestSteps[0].Description);

            //Clean up
            spiraSoapClient.TestCase_Delete(credentials, projectId1, testCaseId);
        }

        /// <summary>
        /// Pulls a smaple workflow transition.
        /// </summary>
        [
        Test,
        SpiraTestCase(2021)
        ]
        public void _27_WorkflowTransition_Pulling()
        {
            const int INCIDENT_ID = 3;

            //Connect to the pre-existing project 2
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the incident.
            RemoteIncident incident = spiraSoapClient.Incident_RetrieveById(credentials, PROJECT_ID, INCIDENT_ID);

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
            RemoteWorkflowTransition[] transitions = spiraSoapClient.Incident_RetrieveWorkflowTransitions(credentials, PROJECT_ID, incident.IncidentTypeId.Value, incident.IncidentStatusId.Value, isDetector, isOwner);

            //Errorchecking.
            Assert.AreEqual(transitions.Length, 2);
            bool isOneOpen = false;
            bool isOneAssigned = false;
            for (int i = 0; i < transitions.Length; i++)
            {
                if (transitions[i].StatusName_Output == "Open")
                {
                    isOneOpen = true;
                }
                if (transitions[i].StatusName_Output == "Assigned")
                {
                    isOneAssigned = true;
                }
            }
            Assert.AreEqual(isOneAssigned, true);
            Assert.AreEqual(isOneOpen, true);

            //Task Transitions
            transitions = spiraSoapClient.Task_RetrieveWorkflowTransitions(credentials, PROJECT_ID, 1, 1, true, true);
            Assert.AreEqual(2, transitions.Length);
            Assert.AreEqual("Defer Task", transitions[0].Name);
            Assert.AreEqual("Start Task", transitions[1].Name);

            //Requirement Transitions
            transitions = spiraSoapClient.Requirement_RetrieveWorkflowTransitions(credentials, PROJECT_ID, 1, 1, true, true);
            Assert.AreEqual(2, transitions.Length);
            Assert.AreEqual("Accept Requirement", transitions[0].Name);
            Assert.AreEqual("Review Requirement", transitions[1].Name);

            //Test Case Transitions
            transitions = spiraSoapClient.TestCase_RetrieveWorkflowTransitions(credentials, PROJECT_ID, 1, 1, true, true);
            Assert.AreEqual(2, transitions.Length);
            Assert.AreEqual("Reject Test Case", transitions[0].Name);
            Assert.AreEqual("Review Test Case", transitions[1].Name);

            //Release Transitions
            transitions = spiraSoapClient.Release_RetrieveWorkflowTransitions(credentials, PROJECT_ID, 1, 1, true, true);
            Assert.AreEqual(3, transitions.Length);
            Assert.AreEqual("Cancel Release", transitions[0].Name);
            Assert.AreEqual("Defer Release", transitions[1].Name);
            Assert.AreEqual("Start Release", transitions[2].Name);

			//Risk Transitions
			transitions = spiraSoapClient.Risk_RetrieveWorkflowTransitions(credentials, PROJECT_ID, 1, 1, true, true);
			Assert.AreEqual(2, transitions.Length);
			Assert.AreEqual("Analyze Risk", transitions[0].Name);
			Assert.AreEqual("Reject Risk", transitions[1].Name);
		}

        /// <summary>
        /// Pulls allowed fields.
        /// </summary>
        [
            Test,
            SpiraTestCase(2020)
        ]
        public void _28_WorkflowTransitionFields_Pulling()
        {
            const int INCIDENT_ID = 3;

            //Connect to the pre-existing project 1
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the incident.
            RemoteIncident incident = spiraSoapClient.Incident_RetrieveById(credentials, PROJECT_ID, INCIDENT_ID);

            //Verify that we can retrieve the fields
            RemoteWorkflowField[] fields = spiraSoapClient.Incident_RetrieveWorkflowFields(credentials, PROJECT_ID, incident.IncidentTypeId.Value, incident.IncidentStatusId.Value);

            Assert.AreEqual(fields.Length, 12);
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Actual Effort" && f.FieldStateId == 1));   //Disabled
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Type" && f.FieldStateId == 2));            //Required
            Assert.IsTrue(fields.Any(f => f.FieldCaption == "Fixed Build" && f.FieldStateId == 3));   //Hidden

            //Now verify that we can retrieve the custom fields
            RemoteWorkflowCustomProperty[] customProperties = spiraSoapClient.Incident_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, incident.IncidentTypeId.Value, incident.IncidentStatusId.Value);
            Assert.AreEqual(4, customProperties.Length);
            Assert.IsTrue(customProperties.Any(cp => cp.FieldName == "Custom_09" && cp.FieldStateId == 1)); //Disabled
            Assert.IsTrue(customProperties.Any(cp => cp.FieldName == "Custom_02" && cp.FieldStateId == 2)); //Required
            Assert.IsTrue(customProperties.Any(cp => cp.FieldName == "Custom_05" && cp.FieldStateId == 3)); //Hidden

            //Task Fields
            fields = spiraSoapClient.Task_RetrieveWorkflowFields(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(2, fields.Length);
            customProperties = spiraSoapClient.Task_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, customProperties.Length);

            //Requirement Fields
            fields = spiraSoapClient.Requirement_RetrieveWorkflowFields(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, fields.Length);
            customProperties = spiraSoapClient.Task_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, customProperties.Length);

            //Test Case Fields
            fields = spiraSoapClient.TestCase_RetrieveWorkflowFields(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, fields.Length);
            customProperties = spiraSoapClient.Task_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, customProperties.Length);

            //Release Fields
            fields = spiraSoapClient.Release_RetrieveWorkflowFields(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, fields.Length);
            customProperties = spiraSoapClient.Task_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, 1, 1);
            Assert.AreEqual(0, customProperties.Length);

			//Risk Fields
			fields = spiraSoapClient.Risk_RetrieveWorkflowFields(credentials, PROJECT_ID, 1, 1);
			Assert.AreEqual(2, fields.Length);
			customProperties = spiraSoapClient.Risk_RetrieveWorkflowCustomProperties(credentials, PROJECT_ID, 1, 1);
			Assert.AreEqual(0, customProperties.Length);
		}

        /// <summary>
        /// Verifies that we can retrieve and delete documents through the API
        /// </summary>
        [
        Test,
        SpiraTestCase(2003)
        ]
        public void _29_RetrieveDeleteAttachments()
        {
            //First lets authenticate and connect to the project
            spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Next lets try retrieving the list of documents for a specific project folder, no filter, sorting by upload date ascending
            RemoteDocument[] remoteDocuments = spiraSoapClient.Document_RetrieveForFolder(credentials, projectId1, projectAttachmentFolderId, 1, 999, "UploadDate ASC", null);

            //Verify the data retrieved
            Assert.AreEqual(4, remoteDocuments.Length);
            RemoteDocument remoteDocument = remoteDocuments[1];
            Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Now lets try filtering by a field and sorting by filename
            RemoteFilter remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "Filename";
            remoteFilter.StringValue = "test_data";
            remoteDocuments = spiraSoapClient.Document_RetrieveForFolder(credentials, projectId1, projectAttachmentFolderId, 1, 999, "Filename ASC", new RemoteFilter[] { remoteFilter });

            //Verify the data retrieved
            Assert.AreEqual(2, remoteDocuments.Length);
            remoteDocument = remoteDocuments[0];
            Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Now retry the same thing, but without filtering by folder
            remoteDocuments = spiraSoapClient.Document_Retrieve2(credentials, projectId1, 1, 999, "Filename ASC", new RemoteFilter[] { remoteFilter });

            //Verify the data retrieved
            Assert.AreEqual(2, remoteDocuments.Length);
            remoteDocument = remoteDocuments[0];
            Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Next lets try retrieving the list of documents for a specific artifact, no filter, no sorting
            remoteDocuments = spiraSoapClient.Document_RetrieveForArtifact1(credentials, projectId1,  /*TestCase*/2, testCaseId1);

            //Verify the data retrieved
            Assert.AreEqual(2, remoteDocuments.Length);
            remoteDocument = remoteDocuments[0];
            Assert.AreEqual("test_data.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);
            remoteDocument = remoteDocuments[1];
            Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment 2", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Get the list of associated artifacts
            remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, remoteDocument.AttachmentId.Value);
            Assert.AreEqual(1, remoteDocument.AttachedArtifacts.Count);
            Assert.AreEqual(testCaseId1, remoteDocument.AttachedArtifacts[0].ArtifactId);
            Assert.AreEqual((int)DataModel.Artifact.ArtifactTypeEnum.TestCase, remoteDocument.AttachedArtifacts[0].ArtifactTypeId);

            //Now lets try filtering by a field and sorting by filename
            remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "Filename";
            remoteFilter.StringValue = "test_data2";
            remoteDocuments = spiraSoapClient.Document_RetrieveForArtifact2(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, "AttachmentId ASC", new RemoteFilter[] { remoteFilter });

            //Verify the data retrieved
            Assert.AreEqual(1, remoteDocuments.Length);
            remoteDocument = remoteDocuments[0];
            Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment 2", remoteDocument.Description);
            Assert.AreEqual("Martha T Muffin", remoteDocument.AuthorName);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("Default", remoteDocument.DocumentTypeName);
            Assert.AreEqual(1, remoteDocument.Size);

            //Need to test that we can get the contents of a file attachment
            byte[] attachmentData = spiraSoapClient.Document_OpenFile(credentials, projectId1, attachmentId1);
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            string attachmentStringData = unicodeEncoding.GetString(attachmentData);
            Assert.AreEqual("Test Attachment Data To Be Stored", attachmentStringData);

			//Test that we can remove an attachment from an artifact
			spiraSoapClient.Document_DeleteFromArtifact(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1, attachmentId2);

            //Verify the data changed
            remoteDocuments = spiraSoapClient.Document_RetrieveForArtifact1(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId1);
            Assert.AreEqual(1, remoteDocuments.Length);
        }


        /// <summary>
        /// Verifies that we can create, retrieve and update artifact associations through the API
        /// </summary>
        [
        Test,
        SpiraTestCase(1981)
        ]
        public void _30_CreateRetrieveUpdateAssociations()
        {
            //First lets add an association between a requirement and an incident
            RemoteAssociation remoteAssociation = new RemoteAssociation();
            remoteAssociation.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.RelatedTo;
            remoteAssociation.SourceArtifactId = requirementId1;
            remoteAssociation.SourceArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
            remoteAssociation.DestArtifactId = incidentId1;
            remoteAssociation.DestArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
            remoteAssociation.Comment = "They are related";
            spiraSoapClient.Association_Create(credentials, projectId1, remoteAssociation);

            //Next add an association between two incidents
            remoteAssociation = new RemoteAssociation();
            remoteAssociation.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.DependentOn;
            remoteAssociation.SourceArtifactId = incidentId1;
            remoteAssociation.SourceArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
            remoteAssociation.DestArtifactId = incidentId2;
            remoteAssociation.DestArtifactTypeId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
            remoteAssociation.Comment = "They are the same bugs man";
            spiraSoapClient.Association_Create(credentials, projectId1, remoteAssociation);

            //Verify that the associations were created
            RemoteAssociation[] remoteAssociations = spiraSoapClient.Association_RetrieveForArtifact1(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1).OrderBy(a => a.DestArtifactTypeId).ToArray();
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
            remoteAssociations = spiraSoapClient.Association_RetrieveForArtifact2(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, "Comment ASC", new RemoteFilter[] { remoteFilter });
            Assert.AreEqual(1, remoteAssociations.Length);
            Assert.AreEqual(incidentId2, remoteAssociations[0].DestArtifactId);
            Assert.AreEqual("Incident", remoteAssociations[0].DestArtifactTypeName);
            Assert.AreEqual("They are the same bugs man", remoteAssociations[0].Comment);
            
            //Test that we can update the comment
            remoteAssociation = remoteAssociations[0];
            remoteAssociation.Comment = "They are the same bugs lady";
            spiraSoapClient.Association_Update(credentials, projectId1, remoteAssociation);
            remoteAssociations = spiraSoapClient.Association_RetrieveForArtifact2(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, "Comment ASC", new RemoteFilter[] { remoteFilter });
            Assert.AreEqual(1, remoteAssociations.Length);
            Assert.AreEqual(incidentId2, remoteAssociations[0].DestArtifactId);
            Assert.AreEqual("Incident", remoteAssociations[0].DestArtifactTypeName);
            Assert.AreEqual("They are the same bugs lady", remoteAssociations[0].Comment);

			//Remove an association
			spiraSoapClient.Association_Delete(credentials, projectId1, remoteAssociation.ArtifactLinkId);
			//Verify it no longer exists
			remoteAssociations = spiraSoapClient.Association_RetrieveForArtifact2(credentials, projectId1, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId1, "Comment ASC", new RemoteFilter[] { remoteFilter });
			Assert.AreEqual(0, remoteAssociations.Length);


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
        SpiraTestCase(1976)
        ]
        public void _31_AutomatedTestLauncher()
        {
            //First we need to attach a test script to some test cases in the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            byte[] binaryData = UnicodeEncoding.UTF8.GetBytes("This is a QTP test script");

            spiraSoapClient.TestCase_AddUpdateAutomationScript(credentials, projectId1, testCaseId1, AUTOMATION_ENGINE_ID_QTP, "file:///c:/mytests/test1.qtp", "", "1.0", null, null, null);
            spiraSoapClient.TestCase_AddUpdateAutomationScript(credentials, projectId1, testCaseId2, AUTOMATION_ENGINE_ID_QTP, "test2.qtp", "", "1.0", null, null, binaryData);
            spiraSoapClient.TestCase_AddUpdateAutomationScript(credentials, projectId1, testCaseId3, AUTOMATION_ENGINE_ID_QTP, "file:///c:/mytests/test3.qtp", "", "1.0", null, null, null);

            //Next we need to create an automation host in the project
            RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
            remoteAutomationHost.ProjectId = projectId1;
            remoteAutomationHost.Name = "Window XP Host";
            remoteAutomationHost.Token = "Win8";
            remoteAutomationHost.Description = "Test Win8 host";
            remoteAutomationHost.Active = true;
            int automationHostId = spiraSoapClient.AutomationHost_Create(credentials, projectId1, remoteAutomationHost).AutomationHostId.Value;

            //Assign one of our test sets to this host and set the planned date
            RemoteTestSet remoteTestSet = spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId1);
            remoteTestSet.AutomationHostId = automationHostId;
            remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Automated;
            remoteTestSet.PlannedDate = DateTime.UtcNow;
            spiraSoapClient.TestSet_Update(credentials, projectId1, remoteTestSet);

            //Now use the API method to actually get the list of test runs to be executed along with associated parameters
            DateRange dateRange = new DateRange();
            dateRange.StartDate = DateTime.UtcNow.AddHours(-1);
            dateRange.EndDate = DateTime.UtcNow.AddHours(1);
            RemoteAutomatedTestRun[] remoteAutomatedTestRuns = spiraSoapClient.TestRun_CreateForAutomationHost(credentials, projectId1, "Win8", dateRange);

            //Verify the data
            Assert.AreEqual(4, remoteAutomatedTestRuns.Length);
            Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[0].Name);
            Assert.AreEqual("Test Case 3", remoteAutomatedTestRuns[1].Name);
            Assert.AreEqual("Test Case 2", remoteAutomatedTestRuns[2].Name);
            Assert.AreEqual("Test Case 1", remoteAutomatedTestRuns[3].Name);

            //Verify one test case in detail
            RemoteAutomatedTestRun remoteAutomatedTestRun = remoteAutomatedTestRuns[2];
            Assert.AreEqual(testCaseId2, remoteAutomatedTestRun.TestCaseId);
            Assert.AreEqual(testSetId1, remoteAutomatedTestRun.TestSetId);
            Assert.AreEqual(projectId1, remoteAutomatedTestRun.ProjectId);
            Assert.AreEqual(AUTOMATION_ENGINE_ID_QTP, remoteAutomatedTestRun.AutomationEngineId);
            Assert.AreEqual(automationHostId, remoteAutomatedTestRun.AutomationHostId);
            Assert.AreEqual("Quick Test Pro", remoteAutomatedTestRun.RunnerName);
            Assert.AreEqual((int)TestRun.TestRunTypeEnum.Automated, remoteAutomatedTestRun.TestRunTypeId);
            int automationAttachmentId = remoteAutomatedTestRun.AutomationAttachmentId.Value;

            //Verify the custom properties were copied across from the test set
            Assert.AreEqual(1, remoteAutomatedTestRun.CustomProperties.Count);
            Assert.AreEqual(customValueId3, remoteAutomatedTestRun.CustomProperties[0].IntegerValue.Value);
            Assert.AreEqual(2, remoteAutomatedTestRun.CustomProperties[0].PropertyNumber);
            Assert.AreEqual("Component", remoteTestSet.CustomProperties[0].Definition.Name);

            //Test that we can retrieve the test script
            RemoteDocument remoteTestScript = spiraSoapClient.Document_RetrieveById(credentials, projectId1, automationAttachmentId);
            Assert.AreEqual("test2.qtp", remoteTestScript.FilenameOrUrl);
            binaryData = spiraSoapClient.Document_OpenFile(credentials, projectId1, automationAttachmentId);
            string testScript = System.Text.UnicodeEncoding.UTF8.GetString(binaryData);
            Assert.AreEqual("This is a QTP test script", testScript);

            //Verify that we can view the test case parameters
            Assert.AreEqual(2, remoteAutomatedTestRun.Parameters.Count);
            Assert.AreEqual("param1", remoteAutomatedTestRun.Parameters[0].Name);
            Assert.AreEqual("test value 1", remoteAutomatedTestRun.Parameters[0].Value);
            Assert.AreEqual("param2", remoteAutomatedTestRun.Parameters[1].Name);
            Assert.AreEqual("test value 2", remoteAutomatedTestRun.Parameters[1].Value);

            //Verify that testCaseId1 has the default parameter value specified (new to the 4.0 or later APIs)
            remoteAutomatedTestRun = remoteAutomatedTestRuns[0];
            Assert.AreEqual(1, remoteAutomatedTestRun.Parameters.Count);
            Assert.AreEqual("param0", remoteAutomatedTestRun.Parameters[0].Name);
            Assert.AreEqual("value0", remoteAutomatedTestRun.Parameters[0].Value);

            //Next make sure that we can mark a specific test cases as 'in use' - used for the distributed scheduling feature of RemoteLaunch
            bool isInUse = spiraSoapClient.TestSet_CheckInUseStatus(credentials, projectId1, remoteAutomatedTestRun.TestSetId.Value, remoteAutomatedTestRun.TestSetTestCaseId.Value);
            Assert.IsFalse(isInUse);
            spiraSoapClient.TestSet_SetInUseStatus(credentials, projectId1, remoteAutomatedTestRun.TestSetId.Value, remoteAutomatedTestRun.TestSetTestCaseId.Value, true.ToString());
            isInUse = spiraSoapClient.TestSet_CheckInUseStatus(credentials, projectId1, remoteAutomatedTestRun.TestSetId.Value, remoteAutomatedTestRun.TestSetTestCaseId.Value);
            Assert.IsTrue(isInUse);
            spiraSoapClient.TestSet_SetInUseStatus(credentials, projectId1, remoteAutomatedTestRun.TestSetId.Value, remoteAutomatedTestRun.TestSetTestCaseId.Value, false.ToString());
            isInUse = spiraSoapClient.TestSet_CheckInUseStatus(credentials, projectId1, remoteAutomatedTestRun.TestSetId.Value, remoteAutomatedTestRun.TestSetTestCaseId.Value);
            Assert.IsFalse(isInUse);

            //Now record some results using these test runs
            remoteAutomatedTestRun = remoteAutomatedTestRuns[2];
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Failed;
            remoteAutomatedTestRun.RunnerAssertCount = 1;
            remoteAutomatedTestRun.RunnerMessage = "Failed with error";
            remoteAutomatedTestRun.RunnerTestName = "MyTest1";
            remoteAutomatedTestRun.RunnerStackTrace = "Failed with error during database operation X";
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow.AddMinutes(-1);
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(1);
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun);

            //Verify that the results can be retrieved
            remoteAutomatedTestRun = spiraSoapClient.TestRun_RetrieveAutomatedById(credentials, projectId1, remoteAutomatedTestRun.TestRunId.Value);
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
            remoteAutomatedTestRuns = spiraSoapClient.TestRun_CreateForAutomatedTestSet(credentials, projectId1, testSetId1, "Win8");

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

            //Verify that testCaseId1 has the default parameter value specified (new to the 4.0 and later API)
            remoteAutomatedTestRun = remoteAutomatedTestRuns[0];
            Assert.AreEqual(1, remoteAutomatedTestRun.Parameters.Count);
            Assert.AreEqual("param0", remoteAutomatedTestRun.Parameters[0].Name);
            Assert.AreEqual("value0", remoteAutomatedTestRun.Parameters[0].Value);

            //Verify the custom properties were copied across from the test set
            Assert.AreEqual(1, remoteAutomatedTestRun.CustomProperties.Count);
            Assert.AreEqual(customValueId3, remoteAutomatedTestRun.CustomProperties[0].IntegerValue.Value);
            Assert.AreEqual(2, remoteAutomatedTestRun.CustomProperties[0].PropertyNumber);
            Assert.AreEqual("Component", remoteTestSet.CustomProperties[0].Definition.Name);

            //Delete the host
            spiraSoapClient.AutomationHost_Delete(credentials, projectId1, automationHostId);

            //Verify it has been deleted
            bool errorThrown = false;
            try
            {
                spiraSoapClient.AutomationHost_RetrieveById(credentials, projectId1, automationHostId);
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);
        }

        /// <summary>
        /// Tests that you can add and retrieve comments through the API
        /// </summary>
        [
        Test,
        SpiraTestCase(1993)
        ]
        public void _32_ImportRetrieveArtifactComments()
        {
            //First lets authenticate and connect to the project
            spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Now lets add some comments to various artifacts
            RemoteComment remoteComment = new RemoteComment();

            //Requirement
            remoteComment.ArtifactId = requirementId1;
            remoteComment.Text = "This is a comment";
            spiraSoapClient.Requirement_CreateComment(credentials, projectId1, requirementId1, remoteComment);
            //Verify the data
            RemoteComment[] remoteComments = spiraSoapClient.Requirement_RetrieveComments(credentials, projectId1, requirementId1);
            Assert.AreEqual(1, remoteComments.Length);
            Assert.AreEqual("This is a comment", remoteComments[0].Text);
            Assert.IsNotNull(remoteComments[0].CreationDate);

            //Release
            remoteComment.ArtifactId = releaseId1;
            remoteComment.Text = "This is a comment";
            spiraSoapClient.Release_CreateComment(credentials, projectId1, releaseId1, remoteComment);
            //Verify the data
            remoteComments = spiraSoapClient.Release_RetrieveComments(credentials, projectId1, releaseId1);
            Assert.AreEqual(1, remoteComments.Length);
            Assert.AreEqual("This is a comment", remoteComments[0].Text);
            Assert.IsNotNull(remoteComments[0].CreationDate);

            //Task
            remoteComment.ArtifactId = taskId1;
            remoteComment.Text = "This is a comment";
            spiraSoapClient.Task_CreateComment(credentials, projectId1, taskId1, remoteComment);
            //Verify the data
            remoteComments = spiraSoapClient.Task_RetrieveComments(credentials, projectId1, taskId1);
            Assert.AreEqual(1, remoteComments.Length);
            Assert.AreEqual("This is a comment", remoteComments[0].Text);
            Assert.IsNotNull(remoteComments[0].CreationDate);

            //Test Set
            remoteComment.ArtifactId = testSetId1;
            remoteComment.Text = "This is a comment";
            spiraSoapClient.TestSet_CreateComment(credentials, projectId1, testSetId1, remoteComment);
            //Verify the data
            remoteComments = spiraSoapClient.TestSet_RetrieveComments(credentials, projectId1, testSetId1);
            Assert.AreEqual(1, remoteComments.Length);
            Assert.AreEqual("This is a comment", remoteComments[0].Text);
            Assert.IsNotNull(remoteComments[0].CreationDate);

            //Test Case
            remoteComment.ArtifactId = testCaseId1;
            remoteComment.Text = "This is a comment";
            spiraSoapClient.TestCase_CreateComment(credentials, projectId1, testCaseId1, remoteComment);
            //Verify the data
            remoteComments = spiraSoapClient.TestCase_RetrieveComments(credentials, projectId1, testCaseId1);
            Assert.AreEqual(1, remoteComments.Length);
            Assert.AreEqual("This is a comment", remoteComments[0].Text);
            Assert.IsNotNull(remoteComments[0].CreationDate);
        }

        /// <summary>
        /// Verifies that we can make data-mapping changes to the project
        /// </summary>
        [
        Test,
        SpiraTestCase(1982)
        ]
        public void _33_DataMappingChanges()
        {
            //First lets authenticate and connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Next lets add a new user to the system
            RemoteUser remoteUser = new RemoteUser();
            remoteUser.FirstName = "Bob";
            remoteUser.LastName = "Holness";
            remoteUser.UserName = "bholness";
            remoteUser.Active = true;
            remoteUser.EmailAddress = "bobholness@blockbusters.com";
            int userId = spiraSoapClient.User_Create(credentials, "test123", "What is 3+4?", "7", projectId1, 1, remoteUser).UserId.Value;

            //Verify that we have no mappings for this user
            RemoteDataMapping[] remoteUserMappings = spiraSoapClient.DataMapping_RetrieveUserMappings(credentials, 1);
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
            spiraSoapClient.DataMapping_AddUserMappings(credentials, 1, remoteUserMappings);

            //Verify the data was added
            remoteUserMappings = spiraSoapClient.DataMapping_RetrieveUserMappings(credentials, 1);
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
        SpiraTestCase(2005)
        ]
        public void _34_RetrieveAutomationHostsAndEngines()
        {
            //First verify that we can get the list of active automation engines in the system
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            RemoteAutomationEngine[] remoteAutomationEngines = spiraSoapClient.AutomationEngine_Retrieve(credentials, true);

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
            RemoteAutomationEngine remoteAutomationEngine = spiraSoapClient.AutomationEngine_RetrieveById(credentials, remoteAutomationEngines[1].AutomationEngineId.Value);

            //Verify data
            Assert.AreEqual("Quick Test Pro", remoteAutomationEngine.Name);
            Assert.AreEqual("QuickTestPro9", remoteAutomationEngine.Token);
            Assert.AreEqual(true, remoteAutomationEngine.Active);

            //Now try retrieving a single engine by token
            remoteAutomationEngine = spiraSoapClient.AutomationEngine_RetrieveByToken(credentials, "QuickTestPro9");

            //Verify data
            Assert.AreEqual("Quick Test Pro", remoteAutomationEngine.Name);
            Assert.AreEqual("QuickTestPro9", remoteAutomationEngine.Token);
            Assert.AreEqual(true, remoteAutomationEngine.Active);

            //Next verify that we can get the list of all (active and inactive) automation engines in the system
            remoteAutomationEngines = spiraSoapClient.AutomationEngine_Retrieve(credentials, false);
            Assert.AreEqual(15, remoteAutomationEngines.Length);

            //Test that we can create a new engine
            remoteAutomationEngine = new RemoteAutomationEngine();
            remoteAutomationEngine.Name = "Test Engine";
            remoteAutomationEngine.Token = "TestEngine1";
            remoteAutomationEngine.Active = true;
            int automationEngineId = spiraSoapClient.AutomationEngine_Create(credentials, remoteAutomationEngine).AutomationEngineId.Value;

            //Verify
            remoteAutomationEngine = spiraSoapClient.AutomationEngine_RetrieveById(credentials, automationEngineId);
            Assert.AreEqual(remoteAutomationEngine.Name, "Test Engine");
            Assert.AreEqual(remoteAutomationEngine.Token, "TestEngine1");
            Assert.AreEqual(remoteAutomationEngine.Active, true);

            //Test that we can update this engine
            remoteAutomationEngine.Name = "Test Engine 2";
            remoteAutomationEngine.Token = "TestEngine2";
            spiraSoapClient.AutomationEngine_Update(credentials, remoteAutomationEngine);

            //Verify
            remoteAutomationEngine = spiraSoapClient.AutomationEngine_RetrieveById(credentials, automationEngineId);
            Assert.AreEqual(remoteAutomationEngine.Name, "Test Engine 2");
            Assert.AreEqual(remoteAutomationEngine.Token, "TestEngine2");
            Assert.AreEqual(remoteAutomationEngine.Active, true);

            //Now we need to delete this engine, there is currently no API method to do this :-(
            new AutomationManager().DeleteEngine(automationEngineId, USER_ID_SYSTEM_ADMIN);

            //Now connect to the newly created project and create two automation hosts
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //First verify that we don't have any existing hosts
            RemoteAutomationHost[] remoteAutomationHosts = spiraSoapClient.AutomationHost_Retrieve1(credentials, projectId1);
            Assert.AreEqual(0, remoteAutomationHosts.Length);

            //Now add two hosts
            RemoteAutomationHost remoteAutomationHost = new RemoteAutomationHost();
            remoteAutomationHost.ProjectId = projectId1;
            remoteAutomationHost.Name = "Test Machine 1";
            remoteAutomationHost.Token = "Test01";
            remoteAutomationHost.Active = true;
            int automationHostId1 = spiraSoapClient.AutomationHost_Create(credentials, projectId1, remoteAutomationHost).AutomationHostId.Value;
            remoteAutomationHost = new RemoteAutomationHost();
            remoteAutomationHost.ProjectId = projectId1;
            remoteAutomationHost.Name = "Test Machine 2";
            remoteAutomationHost.Token = "Test02";
            remoteAutomationHost.Description = "This machine has Windows XP running";
            remoteAutomationHost.Active = true;
            int automationHostId2 = spiraSoapClient.AutomationHost_Create(credentials, projectId1, remoteAutomationHost).AutomationHostId.Value;

            //Verify that we can retrieve them
            remoteAutomationHosts = spiraSoapClient.AutomationHost_Retrieve1(credentials, projectId1);
            Assert.AreEqual(2, remoteAutomationHosts.Length);
            Assert.AreEqual(9, remoteAutomationHosts[0].ArtifactTypeId);
            Assert.AreEqual("Test Machine 1", remoteAutomationHosts[0].Name);
            Assert.AreEqual("Test01", remoteAutomationHosts[0].Token);
            Assert.AreEqual(true, remoteAutomationHosts[0].Active);
            Assert.AreEqual(9, remoteAutomationHosts[1].ArtifactTypeId);
            Assert.AreEqual("Test Machine 2", remoteAutomationHosts[1].Name);
            Assert.AreEqual("Test02", remoteAutomationHosts[1].Token);
            Assert.AreEqual(true, remoteAutomationHosts[1].Active);

            //Now try filtering and sorting
            RemoteFilter remoteFilter = new RemoteFilter();
            remoteFilter.PropertyName = "Token";
            remoteFilter.StringValue = "Test02";
            remoteAutomationHosts = spiraSoapClient.AutomationHost_Retrieve2(credentials, projectId1, 1, 9999, "Token", false, new RemoteFilter[] { remoteFilter });
            Assert.AreEqual(1, remoteAutomationHosts.Length);
            Assert.AreEqual("Test Machine 2", remoteAutomationHosts[0].Name);
            Assert.AreEqual("Test02", remoteAutomationHosts[0].Token);
            Assert.AreEqual(true, remoteAutomationHosts[0].Active);

            //Now try and retrieve by id
            remoteAutomationHost = spiraSoapClient.AutomationHost_RetrieveById(credentials, projectId1, automationHostId1);
            Assert.AreEqual(9, remoteAutomationHost.ArtifactTypeId);
            Assert.AreEqual("Test Machine 1", remoteAutomationHost.Name);
            Assert.AreEqual("Test01", remoteAutomationHost.Token);
            Assert.AreEqual(true, remoteAutomationHost.Active);

            //Now try and retrieve by token
            remoteAutomationHost = spiraSoapClient.AutomationHost_RetrieveByToken(credentials, projectId1, "Test02");
            Assert.AreEqual(9, remoteAutomationHost.ArtifactTypeId);
            Assert.AreEqual("Test Machine 2", remoteAutomationHost.Name);
            Assert.AreEqual("Test02", remoteAutomationHost.Token);
            Assert.AreEqual(true, remoteAutomationHost.Active);

            //Now test that we can update one of the hosts
            remoteAutomationHost.Name = "Test Machine 2a";
            remoteAutomationHost.Description = "Updated test machine";
            spiraSoapClient.AutomationHost_Update(credentials, projectId1, remoteAutomationHost);

            //Verify
            remoteAutomationHost = spiraSoapClient.AutomationHost_RetrieveByToken(credentials, projectId1, "Test02");
            Assert.AreEqual("Test Machine 2a", remoteAutomationHost.Name);
            Assert.AreEqual("Updated test machine", remoteAutomationHost.Description);
            Assert.AreEqual("Test02", remoteAutomationHost.Token);
            Assert.AreEqual(true, remoteAutomationHost.Active);

            //Now test that we can delete one of the hosts
            spiraSoapClient.AutomationHost_Delete(credentials, projectId1, automationHostId2);

            //Verify
            remoteAutomationHosts = spiraSoapClient.AutomationHost_Retrieve1(credentials, projectId1);
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
        SpiraTestCase(2006)
        ]
        public void _35_RetrieveDocumentFoldersTypesAndVersions()
        {
            //First lets get a list of document types in the sample project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            RemoteDocumentType[] remoteDocumentTypes = spiraSoapClient.Document_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID, true);

            //Verify
            Assert.AreEqual(6, remoteDocumentTypes.Length);
            Assert.AreEqual("Default", remoteDocumentTypes[0].Name);
            Assert.AreEqual(true, remoteDocumentTypes[0].Active);
            Assert.AreEqual(true, remoteDocumentTypes[0].Default);
            Assert.AreEqual("Functional Specification", remoteDocumentTypes[1].Name);
            Assert.AreEqual("Functional specification for the system. Can be performance or feature related", remoteDocumentTypes[1].Description);
            Assert.AreEqual(true, remoteDocumentTypes[1].Active);
            Assert.AreEqual(false, remoteDocumentTypes[1].Default);

			//Verify can retrieve the default type
			int defaultTypeId = (int)remoteDocumentTypes[0].DocumentTypeId;
			RemoteDocumentType defaultRemoteDocumentType = spiraSoapClient.Document_RetrieveDefaultType(credentials, PROJECT_TEMPLATE_ID);
			Assert.AreEqual(defaultTypeId, defaultRemoteDocumentType.DocumentTypeId, "Can retrieve the correct default document type");
			Assert.AreEqual(true, defaultRemoteDocumentType.Active);
			Assert.AreEqual(true, defaultRemoteDocumentType.Default);

			//Verify you can add a new type and set it to be the new default
			RemoteDocumentType remoteDocumentType = new RemoteDocumentType();
			remoteDocumentType.Name = "zz Type - New";
			remoteDocumentType.Description = "zz Type - Description";
			remoteDocumentType.Active = true;
			remoteDocumentType.Default = true;
			remoteDocumentType = spiraSoapClient.Document_AddType(credentials, PROJECT_TEMPLATE_ID, remoteDocumentType);
			defaultRemoteDocumentType = spiraSoapClient.Document_RetrieveDefaultType(credentials, PROJECT_TEMPLATE_ID);
			Assert.AreEqual(remoteDocumentType.DocumentTypeId, defaultRemoteDocumentType.DocumentTypeId, "The new type id matches the default type id");
			Assert.AreEqual(remoteDocumentType.Name, defaultRemoteDocumentType.Name, "The new type name matches the default type name");
			Assert.AreEqual(remoteDocumentType.Description, defaultRemoteDocumentType.Description, "The new type description matches the default type description");

			//Verify can update an existing type
			remoteDocumentTypes = spiraSoapClient.Document_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID, true);
			remoteDocumentType = remoteDocumentTypes[0];
			remoteDocumentType.Default = true;
			remoteDocumentType.Name = "Default - Updated";
			spiraSoapClient.Document_UpdateType(credentials, PROJECT_TEMPLATE_ID, remoteDocumentType);
			remoteDocumentTypes = spiraSoapClient.Document_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID, true);
			Assert.AreEqual("Default - Updated", remoteDocumentTypes[0].Name);
			Assert.AreEqual(true, remoteDocumentTypes[0].Default);
			Assert.AreEqual("zz Type - New", remoteDocumentTypes[6].Name);
			Assert.AreEqual(false, remoteDocumentTypes[6].Default);

			//Verify that we can retrieve all vs active types
			Assert.AreEqual(7, remoteDocumentTypes.Length);
			//Update the newly added type to make it inactive
			remoteDocumentTypes[6].Active = false;
			remoteDocumentTypes[0].Name = "Default";
			spiraSoapClient.Document_UpdateType(credentials, PROJECT_TEMPLATE_ID, remoteDocumentTypes[6]);
			spiraSoapClient.Document_UpdateType(credentials, PROJECT_TEMPLATE_ID, remoteDocumentTypes[0]);
			remoteDocumentTypes = spiraSoapClient.Document_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID, true);
			Assert.AreEqual(6, remoteDocumentTypes.Length);
			remoteDocumentTypes = spiraSoapClient.Document_RetrieveTypes(credentials, PROJECT_TEMPLATE_ID, false);
			Assert.AreEqual(8, remoteDocumentTypes.Length);

			//Now verify that we have a single root folder (created by default)
			RemoteDocumentFolder[] remoteDocumentFolders = spiraSoapClient.Document_RetrieveFolders(credentials, projectId1);
            Assert.AreEqual(1, remoteDocumentFolders.Length);
            Assert.AreEqual("Root Folder", remoteDocumentFolders[0].Name);
            int oldRootFolder = remoteDocumentFolders[0].ProjectAttachmentFolderId.Value;

            //Now add a two-level hierarchy under this root
            RemoteDocumentFolder remoteDocumentFolder = new RemoteDocumentFolder();
            remoteDocumentFolder.ProjectId = projectId1;
            remoteDocumentFolder.Name = "My Files";
            remoteDocumentFolder.ParentProjectAttachmentFolderId = oldRootFolder;
            int folderId1 = spiraSoapClient.Document_AddFolder(credentials, projectId1, remoteDocumentFolder).ProjectAttachmentFolderId.Value;
            remoteDocumentFolder = new RemoteDocumentFolder();
            remoteDocumentFolder.ProjectId = projectId1;
            remoteDocumentFolder.Name = "Photos";
            remoteDocumentFolder.ParentProjectAttachmentFolderId = folderId1;
            int folderId2 = spiraSoapClient.Document_AddFolder(credentials, projectId1, remoteDocumentFolder).ProjectAttachmentFolderId.Value;
            remoteDocumentFolder = new RemoteDocumentFolder();
            remoteDocumentFolder.ProjectId = projectId1;
            remoteDocumentFolder.Name = "Documents";
            remoteDocumentFolder.ParentProjectAttachmentFolderId = folderId1;
            int folderId3 = spiraSoapClient.Document_AddFolder(credentials, projectId1, remoteDocumentFolder).ProjectAttachmentFolderId.Value;

            //Verify the new folders were added
            remoteDocumentFolders = spiraSoapClient.Document_RetrieveFolders(credentials, projectId1);
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
            remoteDocumentFolder = spiraSoapClient.Document_RetrieveFolderById(credentials, projectId1, folderId3);
            Assert.AreEqual("Documents", remoteDocumentFolder.Name);
            Assert.IsNull(remoteDocumentFolder.IndentLevel);

            //Verify that we can update a folder (name and position)
            remoteDocumentFolder = remoteDocumentFolders[3];
            remoteDocumentFolder.Name = "Photos2";
            remoteDocumentFolder.ParentProjectAttachmentFolderId = oldRootFolder;
            spiraSoapClient.Document_UpdateFolder(credentials, projectId1, remoteDocumentFolder.ProjectAttachmentFolderId.Value, remoteDocumentFolder);

            //Verify the update
            remoteDocumentFolders = spiraSoapClient.Document_RetrieveFolders(credentials, projectId1);
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
            spiraSoapClient.Document_DeleteFolder(credentials, projectId1, folderId3);

            //Verify the folders were deleted
            remoteDocumentFolders = spiraSoapClient.Document_RetrieveFolders(credentials, projectId1);
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
            RemoteDocumentFile remoteDocumentFile = new RemoteDocumentFile();
            remoteDocumentFile.ProjectId = projectId1;
            remoteDocumentFile.FilenameOrUrl = "test_data.xls";
            remoteDocumentFile.Description = "Sample Test Case Attachment";
            remoteDocumentFile.AuthorId = USER_ID_FRED_BLOGGS;
            remoteDocumentFile.CurrentVersion = "1.0";
            remoteDocumentFile.ProjectAttachmentFolderId = folderId2;
            remoteDocumentFile.BinaryData = attachmentData;
            int attachmentId1 = spiraSoapClient.Document_AddFile(credentials, projectId1, remoteDocumentFile).AttachmentId.Value;

            //Now try adding a newer version
            byte[] attachmentData2 = unicodeEncoding.GetBytes("Test Attachment Data To Be Stored (Modified)");
            RemoteDocumentVersionFile remoteDocumentVersionFile = new RemoteDocumentVersionFile();
            remoteDocumentVersionFile.AttachmentId = attachmentId1;
            remoteDocumentVersionFile.VersionNumber = "2.0";
            remoteDocumentVersionFile.FilenameOrUrl = "test_data2.xls";
            remoteDocumentVersionFile.AuthorId = USER_ID_JOE_SMITH;
            remoteDocumentVersionFile.BinaryData = attachmentData2;
            spiraSoapClient.Document_AddFileVersion(credentials, projectId1, attachmentId1, true, remoteDocumentVersionFile);

            //Verify that the data added successfully
            RemoteDocument remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId1);
            Assert.AreEqual("test_data2.xls", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case Attachment", remoteDocument.Description);
            Assert.AreEqual("File", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("2.0", remoteDocument.CurrentVersion);
            Assert.AreEqual(1, remoteDocument.Size);
            Assert.AreEqual(2, remoteDocument.Versions.Count);
            Assert.AreEqual("2.0", remoteDocument.Versions[0].VersionNumber);
            Assert.AreEqual("1.0", remoteDocument.Versions[1].VersionNumber);

			//Verify we can retrieve a specific version
			byte[] attachmentVersionData = spiraSoapClient.Document_OpenVersion(credentials, projectId1, (int)remoteDocument.Versions[0].AttachmentVersionId);
			string attachmentVersionStringData = unicodeEncoding.GetString(attachmentVersionData);
			Assert.AreEqual("Test Attachment Data To Be Stored (Modified)", attachmentVersionStringData);

			//Now try adding a URL to a folder
			remoteDocument = new RemoteDocument();
            remoteDocument.ProjectId = projectId1;
            remoteDocument.FilenameOrUrl = "http://www.tempuri.org/test123.htm";
            remoteDocument.Description = "Sample Test Case URL";
            remoteDocument.AuthorId = USER_ID_FRED_BLOGGS;
            remoteDocument.ProjectAttachmentFolderId = folderId2;
            remoteDocument.CurrentVersion = "1.0";
            int attachmentId2 = spiraSoapClient.Document_AddUrl(credentials, projectId1, remoteDocument).AttachmentId.Value;

            //Now try adding a newer version
            RemoteDocumentVersion remoteDocumentVersion = new RemoteDocumentVersion();
            remoteDocumentVersion.AttachmentId = attachmentId2;
            remoteDocumentVersion.FilenameOrUrl = "http://www.tempuri.org/test456.htm";
            remoteDocumentVersion.VersionNumber = "2.0";
            remoteDocumentVersion.AuthorId = USER_ID_JOE_SMITH;
            spiraSoapClient.Document_AddUrlVersion(credentials, projectId1, attachmentId2, true, remoteDocumentVersion);

            //Verify
            remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId2);
            Assert.AreEqual("http://www.tempuri.org/test456.htm", remoteDocument.FilenameOrUrl);
            Assert.AreEqual("Sample Test Case URL", remoteDocument.Description);
            Assert.AreEqual("URL", remoteDocument.AttachmentTypeName);
            Assert.AreEqual("2.0", remoteDocument.CurrentVersion);
            Assert.AreEqual(0, remoteDocument.Size);
			//Verify that we have two versions
			Assert.AreEqual(2, remoteDocument.Versions.Count);
            Assert.AreEqual("http://www.tempuri.org/test456.htm", remoteDocument.Versions[0].FilenameOrUrl);
            Assert.AreEqual("http://www.tempuri.org/test123.htm", remoteDocument.Versions[1].FilenameOrUrl);



			//Delete a version
			spiraSoapClient.Document_DeleteVersion(credentials, projectId1, (int)remoteDocument.Versions[1].AttachmentVersionId);
			//Verify that we have one version only now
			remoteDocument = spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId2);
			Assert.AreEqual(1, remoteDocument.Versions.Count);

			//Finally test that we can delete the document and url
			spiraSoapClient.Document_Delete(credentials, projectId1, attachmentId1);
            spiraSoapClient.Document_Delete(credentials, projectId1, attachmentId2);

            //Verify that they were deleted
            bool errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Document_RetrieveById(credentials, projectId1, attachmentId2));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Now lets verify that we can retrieve a list of folders by parent folder
            RemoteDocumentFolder[] folders = spiraSoapClient.Document_RetrieveFoldersByParentFolderId(credentials, projectId1, oldRootFolder);
            Assert.AreEqual(2, folders.Length);
        }

        /// <summary>Tests that we can get project information back from known items..</summary>
        [Test, SpiraTestCase(1985)]
        public void _36_GetProjectNumbers()
        {
            //Connect to the server, but NOT the project.
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            int projId = 0;
            //Release..
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 4, releaseId1);
            Assert.AreEqual(projId, projectId1, "Release Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //Requirement..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 1, requirementId1);
            Assert.AreEqual(projId, projectId1, "Requirement Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //TestCase..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 2, testCaseId2);
            Assert.AreEqual(projId, projectId1, "TestCase Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //Incident..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 3, incidentId2);
            Assert.AreEqual(projId, projectId1, "Incident Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //TestRun..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 5, testRunId3);
            Assert.AreEqual(projId, projectId1, "Test Run Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //Task..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 6, taskId1);
            Assert.AreEqual(projId, projectId1, "Task Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //TestStep..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 7, testStepId1);
            Assert.AreEqual(projId, projectId1, "Task Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //TestSet..
            projId = 0;
            projId = spiraSoapClient.System_GetProjectIdForArtifact(credentials, 8, testSetId1);
            Assert.AreEqual(projId, projectId1, "Task Project ID did not match. Got " + projId.ToString() + ", expected " + projectId1.ToString());

            //Automation Host..
            //TBD
        }

        /// <summary>
        /// Tests that we can retrieve the test configurations in the sample project
        /// </summary>
        [Test, SpiraTestCase(2002)]
        public void _36b_RequirementTestStepAssociations()
        {
            //First lets authenticate and connect to the project as one of our new users
            spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

            //Verify that there are no test cases mapped to a requirement (and vice-versa)
            RemoteRequirementTestStepMapping[] mappings = spiraSoapClient.Requirement_RetrieveTestStepCoverage(credentials, projectId1, requirementId2);
            Assert.AreEqual(0, mappings.Length);
            mappings = spiraSoapClient.TestStep_RetrieveRequirementCoverage(credentials, projectId1, testStepId1);
            Assert.AreEqual(0, mappings.Length);

            //Also make sure the requirement and test step exist OK
            RemoteRequirement requirement = spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2);
            Assert.IsNotNull(requirement);
            RemoteTestStep testStep = spiraSoapClient.TestCase_RetrieveStepById(credentials, projectId1, testCaseId3, testStepId1);
            Assert.IsNotNull(testStep);

            //Add a mapping
            RemoteRequirementTestStepMapping mapping = new RemoteRequirementTestStepMapping();
            mapping.RequirementId = requirementId2;
            mapping.TestStepId = testStepId1;
            spiraSoapClient.Requirement_AddTestStepCoverage(credentials, projectId1, mapping);

            //Verify
            mappings = spiraSoapClient.Requirement_RetrieveTestStepCoverage(credentials, projectId1, requirementId2);
            Assert.AreEqual(1, mappings.Length);
            mappings = spiraSoapClient.TestStep_RetrieveRequirementCoverage(credentials, projectId1, testStepId1);
            Assert.AreEqual(1, mappings.Length);

            //Remove the mapping
            mapping = new RemoteRequirementTestStepMapping();
            mapping.RequirementId = requirementId2;
            mapping.TestStepId = testStepId1;
            spiraSoapClient.Requirement_RemoveTestStepCoverage(credentials, projectId1, mapping);

            //Verify
            mappings = spiraSoapClient.Requirement_RetrieveTestStepCoverage(credentials, projectId1, requirementId2);
            Assert.AreEqual(0, mappings.Length);
            mappings = spiraSoapClient.TestStep_RetrieveRequirementCoverage(credentials, projectId1, testStepId1);
            Assert.AreEqual(0, mappings.Length);
        }

        /// <summary>
        /// Verifies that we can delete requirements, releases, test cases, test sets, incidents and tasks
        /// </summary>
        [
        Test,
        SpiraTestCase(1984)
        ]
        public void _37_DeleteItems()
        {
            //Connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", "{B9050F75-C5E6-4244-8712-FBF20061A976}", PLUGIN_NAME);

            //Delete a requirement and verify
            spiraSoapClient.Requirement_Delete(credentials, projectId1, requirementId1);
            bool errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Delete a release and verify
            spiraSoapClient.Release_Delete(credentials, projectId1, iterationId1);
            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Release_RetrieveById(credentials, projectId1, iterationId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Delete a test case and verify
            spiraSoapClient.TestCase_Delete(credentials, projectId1, testCaseId1);
            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Delete a test set and verify
            spiraSoapClient.TestSet_Delete(credentials, projectId1, testSetId1);
            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.TestSet_RetrieveById(credentials, projectId1, testSetId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Delete an incident and verify
            spiraSoapClient.Incident_Delete(credentials, projectId1, incidentId1);
            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Incident_RetrieveById(credentials, projectId1, incidentId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Delete a task and verify
            spiraSoapClient.Task_Delete(credentials, projectId1, taskId1);
            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.Task_RetrieveById(credentials, projectId1, taskId1));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //Create a user for testing deletes
            RemoteUser remoteUser = new RemoteUser();
            remoteUser.FirstName = "Adam2";
            remoteUser.MiddleInitial = "";
            remoteUser.LastName = "Ant2";
            remoteUser.UserName = "aant2";
            remoteUser.EmailAddress = "aant2@antmusic.com";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.LdapDn = "";
            remoteUser = spiraSoapClient.User_Create(credentials, "aant123456789", "What is 1+1?", "2", projectId1, InternalRoutines.PROJECT_ROLE_MANAGER, remoteUser);

            //Disable the user and verify
            spiraSoapClient.User_Delete(credentials, remoteUser.UserId.Value);

            //Verify
            remoteUser = spiraSoapClient.User_RetrieveById(credentials, remoteUser.UserId.Value);
            Assert.IsFalse(remoteUser.Active);

            //Only makes it inactive now, so do the delete using business method
            new UserManager().DeleteUser(remoteUser.UserId.Value, true);

            errorThrown = false;
            try
            {
                Assert.IsNull(spiraSoapClient.User_RetrieveById(credentials, remoteUser.UserId.Value));
            }
            catch (Exception exception)
            {
                errorThrown = (exception.Message == "404 Not Found");
            }
            Assert.IsTrue(errorThrown);

            //The delete a test step is tested separately in the _37_MoveArtifacts test
        }

        /// <summary>
        /// Tests that you can create new builds through the API and associate incidents, test runs and source code revisions
        /// </summary>
        [
        Test,
        SpiraTestCase(1977)
        ]
        public void _38_BuildManagement()
        {
            //Connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Record a couple of new builds against an existing iteration, adding some revisions to the first one
            //Build 1
            RemoteBuild remoteBuild1 = new RemoteBuild();
            remoteBuild1.ProjectId = projectId1;
            remoteBuild1.BuildStatusId = 2;  //Succeeded
            remoteBuild1.ReleaseId = iterationId2;
            remoteBuild1.Name = "Build 0001";
            remoteBuild1.Description = "The first build";
            remoteBuild1.CreationDate = DateTime.UtcNow.AddDays(-1);
            List<RemoteBuildSourceCode> revisions = new List<RemoteBuildSourceCode>();
            revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev001" });
            revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev002" });
            revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev003" });
            revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev004" });
            revisions.Add(new RemoteBuildSourceCode() { RevisionKey = "rev005" });
            remoteBuild1.Revisions = revisions;
            remoteBuild1 = spiraSoapClient.Build_Create(credentials, projectId1, iterationId2, remoteBuild1);
            //Build 2
            RemoteBuild remoteBuild2 = new RemoteBuild();
            remoteBuild2.ProjectId = projectId1;
            remoteBuild2.BuildStatusId = 1;  //Failed
            remoteBuild2.ReleaseId = iterationId2;
            remoteBuild2.Name = "Build 0002";
            remoteBuild2.Description = null;
            remoteBuild2 = spiraSoapClient.Build_Create(credentials, projectId1, iterationId2, remoteBuild2);

            //Now test that we can retrieve a single build with its revisions
            RemoteBuild remoteBuild = spiraSoapClient.Build_RetrieveById(credentials, projectId1, iterationId2, remoteBuild1.BuildId.Value);
            Assert.IsNotNull(remoteBuild);
            Assert.AreEqual(2, remoteBuild.BuildStatusId);
            Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
            Assert.AreEqual("Build 0001", remoteBuild.Name);
            Assert.AreEqual("The first build", remoteBuild.Description);
            Assert.AreEqual(5, remoteBuild.Revisions.Count);
            Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
            Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
            Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
            Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
            Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Verify we can retrieve the build without a description if using that option
			remoteBuild = spiraSoapClient.Build_RetrieveById_NoDescription(credentials, projectId1, iterationId2, remoteBuild1.BuildId.Value);
			Assert.IsNotNull(remoteBuild);
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("", remoteBuild.Description);
			Assert.AreEqual(5, remoteBuild.Revisions.Count);
			Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Now test that we can retrieve all the builds with their revisions
			RemoteBuild[] remoteBuilds = spiraSoapClient.Build_RetrieveByReleaseId1(credentials, projectId1, iterationId2).OrderBy(b => b.Name).ToArray();
            Assert.AreEqual(2, remoteBuilds.Length);
            remoteBuild = remoteBuilds[0];
            Assert.AreEqual(2, remoteBuild.BuildStatusId);
            Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
            Assert.AreEqual("Build 0001", remoteBuild.Name);
            Assert.AreEqual("The first build", remoteBuild.Description);
            Assert.AreEqual(5, remoteBuild.Revisions.Count);
            Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
            Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
            Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
            Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
            Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

			//Verify we can retrieve all builds for a release without description if using that option
			remoteBuilds = spiraSoapClient.Build_RetrieveByReleaseId_NoDescription(credentials, projectId1, iterationId2).OrderBy(b => b.Name).ToArray();
			Assert.AreEqual(2, remoteBuilds.Length);
			remoteBuild = remoteBuilds[0];
			Assert.AreEqual(2, remoteBuild.BuildStatusId);
			Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
			Assert.AreEqual("Build 0001", remoteBuild.Name);
			Assert.AreEqual("", remoteBuild.Description);

			//Now try filtering by build status and sending a sort
			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
            remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildStatusId", IntValue = 2 });
            remoteBuilds = spiraSoapClient.Build_RetrieveByReleaseId2(credentials, projectId1, iterationId2, 1, 15, "Name ASC", remoteFilters.ToArray());
            Assert.AreEqual(1, remoteBuilds.Length);
            remoteBuild = remoteBuilds[0];
            Assert.AreEqual(2, remoteBuild.BuildStatusId);
            Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
            Assert.AreEqual("Build 0001", remoteBuild.Name);
            Assert.AreEqual("The first build", remoteBuild.Description);
            Assert.AreEqual(5, remoteBuild.Revisions.Count);
            Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
            Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
            Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
            Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
            Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

            //Now try filtering by multiple build statuses and sending a sort
            MultiValueFilter multiValues = new MultiValueFilter();
            multiValues.Values = new int[2] { 1, 2 };
            remoteFilters.Clear();
            remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildStatusId", MultiValue = multiValues });
            remoteBuilds = spiraSoapClient.Build_RetrieveByReleaseId2(credentials, projectId1, iterationId2, 1, 15, "Name ASC", remoteFilters.ToArray());
            Assert.AreEqual(2, remoteBuilds.Length);
            remoteBuild = remoteBuilds[0];
            Assert.AreEqual(2, remoteBuild.BuildStatusId);
            Assert.AreEqual("Succeeded", remoteBuild.BuildStatusName);
            Assert.AreEqual("Build 0001", remoteBuild.Name);
            Assert.AreEqual("The first build", remoteBuild.Description);
            Assert.AreEqual(5, remoteBuild.Revisions.Count);
            Assert.AreEqual("rev001", remoteBuild.Revisions[0].RevisionKey);
            Assert.AreEqual("rev002", remoteBuild.Revisions[1].RevisionKey);
            Assert.AreEqual("rev003", remoteBuild.Revisions[2].RevisionKey);
            Assert.AreEqual("rev004", remoteBuild.Revisions[3].RevisionKey);
            Assert.AreEqual("rev005", remoteBuild.Revisions[4].RevisionKey);

            //Now lets record an automated test against this build
            RemoteAutomatedTestRun remoteAutomatedTestRun = new RemoteAutomatedTestRun();
            remoteAutomatedTestRun.ProjectId = projectId1;
            remoteAutomatedTestRun.TestCaseId = testCaseId2;
            remoteAutomatedTestRun.ReleaseId = iterationId2;
            remoteAutomatedTestRun.BuildId = remoteBuild1.BuildId.Value;
            remoteAutomatedTestRun.StartDate = DateTime.UtcNow;
            remoteAutomatedTestRun.EndDate = DateTime.UtcNow.AddMinutes(2);
            remoteAutomatedTestRun.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Passed;
            remoteAutomatedTestRun.RunnerName = "TestSuite";
            remoteAutomatedTestRun.RunnerTestName = "02_Test_Method";
            testRunId3 = spiraSoapClient.TestRun_RecordAutomated1(credentials, projectId1, remoteAutomatedTestRun).TestRunId.Value;

            //Retrieve the list of test runs by build
            remoteFilters.Clear();
            int[] filterValues = { remoteBuild1.BuildId.Value };
            remoteFilters.Add(new RemoteFilter() { PropertyName = "BuildId", MultiValue = new MultiValueFilter() { Values = filterValues } });
            RemoteTestRun[] remoteTestRuns = spiraSoapClient.TestRun_Retrieve2(credentials, projectId1, 1, 15, "TestRunId", "ASC", remoteFilters.ToArray());
            Assert.AreEqual(1, remoteTestRuns.Length);
            Assert.AreEqual(remoteBuild1.BuildId, remoteTestRuns[0].BuildId.Value);

            //Now lets mark a defect as fixed in this build
            RemoteIncident remoteIncident = new RemoteIncident();
            remoteIncident.ProjectId = projectId1;
            remoteIncident.IncidentTypeId = incidentTypeId;
            remoteIncident.IncidentStatusId = incidentStatusId;
            remoteIncident.Name = "Fixed Incident";
            remoteIncident.ResolvedReleaseId = iterationId2;
            remoteIncident.FixedBuildId = remoteBuild1.BuildId.Value;
            remoteIncident.Description = "This is a test incident fixed in the build";
            remoteIncident.CreationDate = DateTime.Parse("1/5/2005");
            incidentId1 = spiraSoapClient.Incident_Create(credentials, projectId1, remoteIncident).IncidentId.Value;

            //Retrieve the list of defects by build
            RemoteIncident[] remoteIncidents = spiraSoapClient.Incident_Retrieve3(credentials, projectId1, 1, 15, "IncidentId ASC", remoteFilters.ToArray());
            Assert.AreEqual(1, remoteTestRuns.Length);
            Assert.AreEqual(remoteBuild1.BuildId, remoteTestRuns[0].BuildId.Value);
        }

        /// <summary>
        /// Tests that you can move requirements, releases, test sets, test cases and test steps
        /// </summary>
        [
        Test,
        SpiraTestCase(2001)
        ]
        public void _39_MoveArtifacts()
        {
            //Connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            int rq_typeFeatureId = new RequirementManager().RequirementType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Feature").RequirementTypeId;
            int tc_typeFunctionalId = new TestCaseManager().TestCaseType_Retrieve(projectTemplateId1, false).FirstOrDefault(t => t.Name == "Functional").TestCaseTypeId;

            //Lets add some requirements, a test case, test set and release to the end of their respective lists
            //Requirement
            RemoteRequirement remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = 2;
            remoteRequirement.ImportanceId = 1;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.Name = "Requirement To Move";
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
            int requirementId1 = remoteRequirement.RequirementId.Value;
            remoteRequirement = new RemoteRequirement();
            remoteRequirement.ProjectId = projectId1;
            remoteRequirement.StatusId = 2;
            remoteRequirement.ImportanceId = 1;
            remoteRequirement.ReleaseId = releaseId1;
            remoteRequirement.Name = "Requirement To Move";
            remoteRequirement.RequirementTypeId = rq_typeFeatureId;
            remoteRequirement = spiraSoapClient.Requirement_Create1(credentials, projectId1, remoteRequirement);
            int requirementId2 = remoteRequirement.RequirementId.Value;
            //Release
            RemoteRelease remoteRelease = new RemoteRelease();
            remoteRelease.ProjectId = projectId1;
            remoteRelease.Name = "Release 5";
            remoteRelease.VersionNumber = "5.0";
            remoteRelease.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
            remoteRelease.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MajorRelease;
            remoteRelease.StartDate = DateTime.UtcNow;
            remoteRelease.EndDate = DateTime.UtcNow.AddMonths(1);
            remoteRelease.ResourceCount = 1;
            int releaseId = spiraSoapClient.Release_Create1(credentials, projectId1, remoteRelease).ReleaseId.Value;
            //Test Case
            RemoteTestCase remoteTestCase = new RemoteTestCase();
            remoteTestCase.ProjectId = projectId1;
            remoteTestCase.Name = "Test Case To Move";
            remoteTestCase.TestCaseStatusId = (int)TestCase.TestCaseStatusEnum.ReadyForTest;
            remoteTestCase.TestCaseTypeId = tc_typeFunctionalId;
            remoteTestCase.TestCasePriorityId = 1;
            remoteTestCase.EstimatedDuration = 30;
            remoteTestCase = spiraSoapClient.TestCase_Create(credentials, projectId1, remoteTestCase);
            int testCaseId = remoteTestCase.TestCaseId.Value;
            //Test Steps
            //Step 1
            RemoteTestStep remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 1;
            remoteTestStep.Description = "Test Step 1";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep.SampleData = "test 123";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId, remoteTestStep);
            int testStepId1 = remoteTestStep.TestStepId.Value;
            //Step 2
            remoteTestStep = new RemoteTestStep();
            remoteTestStep.ProjectId = projectId1;
            remoteTestStep.Position = 2;
            remoteTestStep.Description = "Test Step 2";
            remoteTestStep.ExpectedResult = "It should work";
            remoteTestStep = spiraSoapClient.TestCase_AddStep(credentials, projectId1, testCaseId, remoteTestStep);
            int testStepId2 = remoteTestStep.TestStepId.Value;
            //Test Set
            RemoteTestSet remoteTestSet = new RemoteTestSet();
            remoteTestSet.ProjectId = projectId1;
            remoteTestSet.Name = "Test Set 1";
            remoteTestSet.CreatorId = userId1;
            remoteTestSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
            remoteTestSet.TestRunTypeId = (int)TestRun.TestRunTypeEnum.Manual;
            int testSetId = spiraSoapClient.TestSet_Create(credentials, projectId1, remoteTestSet).TestSetId.Value;

            //Verify requirement starting positions
            Assert.AreEqual("AAA", spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId1).IndentLevel);
            Assert.AreEqual("AAB", spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2).IndentLevel);

            //Now move the requirement to the start of the list
            spiraSoapClient.Requirement_Move(credentials, projectId1, requirementId2, requirementId1);
            //Verify
            Assert.AreEqual("AAA", spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2).IndentLevel);
            //Now move back to the end of the list
            spiraSoapClient.Requirement_Move(credentials, projectId1, requirementId2, null);
            //Verify
            Assert.AreEqual("AAB", spiraSoapClient.Requirement_RetrieveById(credentials, projectId1, requirementId2).IndentLevel);

            //Now move the test case under a folder
            spiraSoapClient.TestCase_Move(credentials, projectId1, testCaseId, testFolderId1);
            //Verify
            Assert.IsTrue(spiraSoapClient.TestCase_RetrieveByFolder1(credentials, projectId1, testFolderId1,1, 999999, "Name", true, null).Any(t => t.TestCaseId == testCaseId));
            //Now move back to the end of the root
            spiraSoapClient.TestCase_Move(credentials, projectId1, testCaseId, null);
            //Verify
            Assert.IsFalse(spiraSoapClient.TestCase_RetrieveByFolder1(credentials, projectId1, testFolderId1, 1, 999999, "Name", true, null).Any(t => t.TestCaseId == testCaseId));

            //Now move the test set under a folder
            spiraSoapClient.TestSet_Move(credentials, projectId1, testSetId, testSetFolderId1);
            //Verify
            RemoteTestSet[] testSets = spiraSoapClient.TestSet_RetrieveByFolder1(credentials, projectId1, testSetFolderId1, 1, 999999, "Name", true, null);
            Assert.AreEqual(2, testSets.Length);
            //Now move back to the end of the list
            spiraSoapClient.TestSet_Move(credentials, projectId1, testSetId, null);
            //Verify
            testSets = spiraSoapClient.TestSet_RetrieveByFolder1(credentials, projectId1, testSetFolderId1, 1, 999999, "Name", true, null);
            Assert.AreEqual(1, testSets.Length);

            //Now move the release to the start of the list
            spiraSoapClient.Release_Move(credentials, projectId1, releaseId, releaseId1);
            //Verify
            Assert.AreEqual("AAA", spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId).IndentLevel);
            //Now move back to the end of the list
            spiraSoapClient.Release_Move(credentials, projectId1, releaseId, null);
            //Verify
            Assert.AreNotEqual("AAA", spiraSoapClient.Release_RetrieveById(credentials, projectId1, releaseId).IndentLevel);

            //Now Try moving one of the test steps in the test case
            spiraSoapClient.TestCase_MoveStep(credentials, projectId1, testCaseId, testStepId2, testStepId1);
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[0].TestStepId.Value);
            Assert.AreEqual(testStepId1, remoteTestCase.TestSteps[1].TestStepId.Value);

            //Now Try moving one of the test steps in the test case back to the end
            spiraSoapClient.TestCase_MoveStep(credentials, projectId1, testCaseId, testStepId2, null);
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(testStepId1, remoteTestCase.TestSteps[0].TestStepId.Value);
            Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[1].TestStepId.Value);

            //Finally test that we can delete a test step
            spiraSoapClient.TestCase_DeleteStep(credentials, projectId1, testCaseId, testStepId1);
            remoteTestCase = spiraSoapClient.TestCase_RetrieveById(credentials, projectId1, testCaseId);
            Assert.AreEqual(1, remoteTestCase.TestSteps.Count);
            Assert.AreEqual(testStepId2, remoteTestCase.TestSteps[0].TestStepId.Value);
        }

        /// <summary>Tests that the counts we get back are as expected.</summary>
        [Test, SpiraTestCase(1978)]
        public void _40_CheckCounts()
        {
            //Connect to the project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the count functions..
            long numInc = spiraSoapClient.Incident_Count(credentials, projectId1);
            long numReq1 = spiraSoapClient.Requirement_Count1(credentials, projectId1);
            long numReq2 = spiraSoapClient.Requirement_Count2(credentials, projectId1, null);
            long numRel = spiraSoapClient.Release_Count(credentials, projectId1, null);
            long numTsk1 = spiraSoapClient.Task_Count1(credentials, projectId1);
            long numTsk2 = spiraSoapClient.Task_Count2(credentials, projectId1, null);
            long numTC1 = spiraSoapClient.TestCase_Count1(credentials, projectId1, null);
            long numTC2 = spiraSoapClient.TestCase_Count2(credentials, projectId1, null, null);
            long numTS1 = spiraSoapClient.TestSet_Count1(credentials, projectId1, null);
            long numTS2 = spiraSoapClient.TestSet_Count2(credentials, projectId1, null, null);
            long numTR1 = spiraSoapClient.TestRun_Count1(credentials, projectId1);
            long numTR2 = spiraSoapClient.TestRun_Count2(credentials, projectId1, null);

            Assert.AreEqual(3, numInc);
            Assert.AreEqual(2, numReq1);
            Assert.AreEqual(2, numReq2);
            Assert.AreEqual(4, numRel);
            Assert.AreEqual(1, numTsk1);
            Assert.AreEqual(1, numTsk2);
            Assert.AreEqual(4, numTC1);
            Assert.AreEqual(4, numTC2);
            Assert.AreEqual(2, numTS1);
            Assert.AreEqual(2, numTS2);
            Assert.AreEqual(8, numTR1);
            Assert.AreEqual(8, numTR2);
        }

        /// <summary>Tests the various functions in the DataSync API</summary>
        [Test, SpiraTestCase(1983)]
        public void _41_DataSyncApi()
        {
            //First authenticate the DataSync API
            RemoteCredentials credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            Assert.IsNotNull(credentials);

            //Get the list of data-sync systems configured
            RemoteDataSyncSystem[] dataSyncSystems = spiraSoapClient.DataSyncSystem_Retrieve(credentials);
            int numSyncs = dataSyncSystems.Length;
            Assert.AreEqual(3, dataSyncSystems.Length);
            Assert.AreEqual("GitHubDataSync", dataSyncSystems[0].Name);
            Assert.AreEqual("JiraDataSync", dataSyncSystems[1].Name);
            Assert.AreEqual("MsTfsDataSync", dataSyncSystems[2].Name);
            int dataSyncSystem1 = dataSyncSystems[2].DataSyncSystemId;

            //Now we need to retrieve some of the standard mappings for fields and custom properties
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, "UnitTest");
            Assert.IsNotNull(credentials);

            //Verify that we can retrieve a standard field mapping collection, we don't worry about the data, just make sure the calls succeed
            //Incident Severity
            RemoteDataMapping[] fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 1);
            //Incident Priority
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 2);
            //Incident Status
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 3);
            //Incident Type
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 4);
            //Requirement Importance
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 18);
            //Requirement Status
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 16);
            //Requirement Type
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 140);
            //Component
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 141);
            //Task Priority
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 59);
            //Task Status
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 57);
            //Task Type
            fieldMappings = spiraSoapClient.DataMapping_RetrieveFieldValueMappings(credentials, 1, dataSyncSystem1, 145);

            //Verify that we can retrieve custom property mapping
            RemoteDataMapping customPropertyMapping = spiraSoapClient.DataMapping_RetrieveCustomPropertyMapping(credentials, 1, dataSyncSystem1, 1, 1);

            //Verify that we can retrieve custom property mapping
            RemoteDataMapping[] customPropertyValueMappings = spiraSoapClient.DataMapping_RetrieveCustomPropertyValueMappings(credentials, 1, dataSyncSystem1, 1, 1);

            //Modify a datasync to make it inacttive.
            int syncNumber = dataSyncSystems[0].DataSyncSystemId;

            //We have to make the data-syncs active
            InternalRoutines.ExecuteNonQuery("UPDATE TST_DATA_SYNC_SYSTEM SET IS_ACTIVE = 0 WHERE DATA_SYNC_SYSTEM_ID = " + syncNumber.ToString()); ;

            //Do a full pull, we should get ONE LESS than the number we got last time.
            dataSyncSystems = spiraSoapClient.DataSyncSystem_Retrieve(credentials);
            Assert.AreEqual(numSyncs - 1, dataSyncSystems.Length, "Disabled DataSync, got wrong number back.");
        }

        /// <summary>Tests that we can query the linked source code repository through the API</summary>
        [Test, SpiraTestCase(2018)]
        public void _42_SourceCodeQueries()
        {
            try
            {
                //Connect to the pre-existing project 1
                credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

                //Make sure that the provider is active for project 1
                SourceCodeManager scm = new SourceCodeManager();
                VersionControlProject versionControlproject = scm.RetrieveProjectSettings(SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_ID, PROJECT_ID);
                if (!versionControlproject.IsActive)
                {
                    versionControlproject.StartTracking();
                    versionControlproject.IsActive = true;
                    scm.UpdateProjectSettings(versionControlproject);
                }

				//We need to make sure the source code cache has been built, so we first call a branch retrieve and wait 5 seconds
				scm = new SourceCodeManager(PROJECT_ID);
				if (!SourceCodeManager.IsCacheUpdateRunning)
				{
					Common.Logger.LogInformationalEvent("SourceCodeTester", "Refreshing Cache");
					scm.LaunchCacheRefresh();
				}
				System.Threading.Thread.Sleep(5000);

				//Get the connection information for the project
				RemoteSourceCodeConnection remoteSourceCodeConnection = spiraSoapClient.SourceCode_RetrieveConnectionInformation(credentials, PROJECT_ID);
				Assert.IsNotNull(remoteSourceCodeConnection);
				Assert.AreEqual(SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME2, remoteSourceCodeConnection.ProviderName);
				Assert.AreEqual("test://MyRepository", remoteSourceCodeConnection.Connection);
				Assert.IsNull(remoteSourceCodeConnection.Login);
				Assert.IsNull(remoteSourceCodeConnection.Password);

				//Verify that we get null for a project not enabled
				remoteSourceCodeConnection = spiraSoapClient.SourceCode_RetrieveConnectionInformation(credentials, 5);
				Assert.IsNull(remoteSourceCodeConnection);

				//Get the list of branches
				RemoteSourceCodeBranch[] branches = spiraSoapClient.SourceCode_RetrieveBranches(credentials, PROJECT_ID);
                Assert.IsTrue(branches.Length > 0);
                RemoteSourceCodeBranch defaultBranch = branches.FirstOrDefault(s => s.IsDefault);
                Assert.AreEqual("main", defaultBranch.Id);
                Assert.AreEqual("main", defaultBranch.Name);
                string branchId = defaultBranch.Id;

                //Get the root folder
                RemoteSourceCodeFolder[] folders = spiraSoapClient.SourceCode_RetrieveFoldersByParent(credentials, PROJECT_ID, branchId, null);
                Assert.AreEqual(1, folders.Length);
                Assert.AreEqual("test://MyRepository", folders[0].Id);
                Assert.AreEqual("Root", folders[0].Name);

                //Get the list of top-level folders
                folders = spiraSoapClient.SourceCode_RetrieveFoldersByParent(credentials, PROJECT_ID, branchId, "test://MyRepository");
                Assert.AreEqual(4, folders.Length);
                Assert.AreEqual("/Root/Code", folders[0].Id);
                Assert.AreEqual("Code", folders[0].Name);
                Assert.AreEqual("/Root/Samples", folders[1].Id);
                Assert.AreEqual("Samples", folders[1].Name);
                Assert.AreEqual("/Root/Documentation", folders[2].Id);
                Assert.AreEqual("Documentation", folders[2].Name);
                Assert.AreEqual("/Root/Tests", folders[3].Id);
                Assert.AreEqual("Tests", folders[3].Name);

                //Get the list of files in one of the folders
                string folderId = folders[1].Id;
                RemoteSourceCodeFile[] files = spiraSoapClient.SourceCode_RetrieveFilesByFolder(credentials, PROJECT_ID, branchId, folderId);
                Assert.IsTrue(files.Length > 0);
                Assert.IsNotNullOrEmpty(files[0].Id);
                Assert.IsNotNullOrEmpty(files[0].Name);
                Assert.IsNotNullOrEmpty(files[0].Path);
                Assert.IsNotNull(files[0].LastRevision);
                Assert.IsNotNullOrEmpty(files[0].LastRevision.Name);
                Assert.AreEqual(folderId, files[0].ParentFolder.Id);

                //Verify that we can retrieve a file by its ID
                string fileId = files[0].Id;
                RemoteSourceCodeFile file = spiraSoapClient.SourceCode_RetrieveFileById(credentials, PROJECT_ID, branchId, fileId);
                Assert.IsNotNull(file);
                Assert.AreEqual(files[0].Name, file.Name);
                Assert.AreEqual(files[0].Path, file.Path);
                Assert.AreEqual(files[0].LastRevision.Name, file.LastRevision.Name);
                Assert.AreEqual(files[0].ParentFolder.Id, file.ParentFolder.Id);

                //Now verify that we can get the revisions of this file
                RemoteSourceCodeRevision[] revisions = spiraSoapClient.SourceCode_RetrieveRevisionsForFile(credentials, PROJECT_ID, branchId, fileId);
                Assert.IsTrue(revisions.Length > 0);
                Assert.IsNotNullOrEmpty(revisions[0].Id);
                Assert.IsNotNullOrEmpty(revisions[0].Name);
                Assert.IsNotNullOrEmpty(revisions[0].Message);
                string revisionId = revisions[0].Id;

                //Verify that we can get a sorted, filtered list of revisions in general
                revisions = spiraSoapClient.SourceCode_RetrieveRevisions(credentials, PROJECT_ID, branchId, 1, 15, "Name", true, null);
                Assert.IsTrue(revisions.Length > 0);
                Assert.IsNotNullOrEmpty(revisions[0].Id);
                Assert.IsNotNullOrEmpty(revisions[0].Name);
                Assert.IsNotNullOrEmpty(revisions[0].Message);
                revisionId = revisions[0].Id;

                //Verify that we can get a single revision by its id
                RemoteSourceCodeRevision revision = spiraSoapClient.SourceCode_RetrieveRevisionById(credentials, PROJECT_ID, branchId, revisionId);
                Assert.IsNotNull(revision);
                Assert.AreEqual(revisions[0].Id, revision.Id);
                Assert.AreEqual(revisions[0].Name, revision.Name);
                Assert.AreEqual(revisions[0].Message, revision.Message);

                //Verify that we can get the files in a revision
                files = spiraSoapClient.SourceCode_RetrieveFilesByRevision(credentials, PROJECT_ID, branchId, revisionId);
                Assert.IsTrue(files.Length > 0);
                Assert.IsNotNullOrEmpty(files[0].Id);
                Assert.IsNotNullOrEmpty(files[0].Name);
                Assert.IsNotNullOrEmpty(files[0].Path);
                fileId = files[0].Id;

                //Verify that we can open the last revision of a file
                byte[] data = spiraSoapClient.SourceCode_OpenFileById(credentials, PROJECT_ID, branchId, fileId, null);
                Assert.IsTrue(data != null && data.Length > 0);

                //Verify that we can open a specific revision of a file
                data = spiraSoapClient.SourceCode_OpenFileById(credentials, PROJECT_ID, branchId, fileId, revisionId);
                Assert.IsTrue(data != null && data.Length > 0);

                //Verify that we can get the revisions associated with an artifact
                revisions = spiraSoapClient.SourceCode_RetrieveRevisionsForArtifact(credentials, PROJECT_ID, branchId, /*Incident*/3, 7);
                Assert.IsTrue(revisions.Length > 0);

                //Verify that we can get the artifacts for this revision
                RemoteLinkedArtifact[] artifacts = spiraSoapClient.SourceCode_RetrieveArtifactsForRevision(credentials, PROJECT_ID, branchId, revisions[0].Id);
                Assert.IsTrue(artifacts.Length > 0);

                //verify that we can get the files associated with an artifact
                files = spiraSoapClient.SourceCode_RetrieveFilesForArtifact(credentials, PROJECT_ID, branchId, /*Incident*/3, 7);
                Assert.IsTrue(revisions.Length > 0);
            }
            catch (Exception exception)
            {
				//This is a fault due to the cache rebuilding happening and blocking the test, just ignore
				Common.Logger.LogTraceEvent("SourceCodeQueries", exception.Message);
				return;
            }
        }

        /// <summary>Tests that we can use the messenger API from another application</summary>
        [Test, SpiraTestCase(2000)]
        public void _43_InstantMessenger()
        {
            //First login as admin
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Create two new users to use with instant messenger
            RemoteUser remoteUser = new RemoteUser();
            remoteUser.FirstName = "IM";
            remoteUser.MiddleInitial = "";
            remoteUser.LastName = "User 1";
            remoteUser.UserName = "imuser1";
            remoteUser.EmailAddress = "imuser1@spiratest.com";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.Approved = true;
            remoteUser.LdapDn = "";
            remoteUser.RssToken = "{C9C569C6-2BBF-47C2-B498-1238F8A7A0AF}";
            remoteUser = spiraSoapClient.User_Create(credentials, "PleaseChange", "What is 1+1?", "2", projectId1, 1, remoteUser);
            int imUser1 = remoteUser.UserId.Value;

            remoteUser = new RemoteUser();
            remoteUser.FirstName = "IM";
            remoteUser.MiddleInitial = "";
            remoteUser.LastName = "User 2";
            remoteUser.UserName = "imuser2";
            remoteUser.EmailAddress = "imuser2@spiratest.com";
            remoteUser.Admin = false;
            remoteUser.Active = true;
            remoteUser.Approved = true;
            remoteUser.LdapDn = "";
            remoteUser.RssToken = "{78197F7D-2166-49B0-BE7D-AD13FEBB4858}";
            remoteUser = spiraSoapClient.User_Create(credentials, "PleaseChange", "What is 1+1?", "2", projectId1, 1, remoteUser);
            int imUser2 = remoteUser.UserId.Value;

            //Connect as one of the new users using the RSS token
            credentials = spiraSoapClient.Connection_Authenticate("imuser1", "{C9C569C6-2BBF-47C2-B498-1238F8A7A0AF}", "NUnit");
            Assert.IsNotNull(credentials);

            //Verify that we have no messages waiting and that our user is online
            RemoteMessageInfo messageInfo = spiraSoapClient.Message_GetInfo(credentials);
            Assert.AreEqual(0, messageInfo.UnreadMessages);
            Assert.IsTrue(messageInfo.OnlineUsers.Count > 0);
            Assert.IsTrue(messageInfo.OnlineUsers.Any(u => u == imUser1));

            //Now connect as the other user and send a message to the first user
            credentials = spiraSoapClient.Connection_Authenticate("imuser2", "{78197F7D-2166-49B0-BE7D-AD13FEBB4858}", "NUnit");
            Assert.IsNotNull(credentials);
            messageInfo = spiraSoapClient.Message_GetInfo(credentials);
            Assert.AreEqual(0, messageInfo.UnreadMessages);
            Assert.IsTrue(messageInfo.OnlineUsers.Count > 0);
            Assert.IsTrue(messageInfo.OnlineUsers.Any(u => u == imUser2));
            RemoteMessageIndividual remoteMessageIndividual = new RemoteMessageIndividual();
            remoteMessageIndividual.RecipientUserId = imUser1;
            remoteMessageIndividual.MessageText = "This is a message 1 from user 2";
            spiraSoapClient.Message_PostNew(credentials, remoteMessageIndividual);

            //Verify that the first user sees the message
            credentials = spiraSoapClient.Connection_Authenticate("imuser1", "{C9C569C6-2BBF-47C2-B498-1238F8A7A0AF}", "NUnit");
            Assert.IsNotNull(credentials);
            messageInfo = spiraSoapClient.Message_GetInfo(credentials);
            Assert.AreEqual(1, messageInfo.UnreadMessages);
            Assert.IsTrue(messageInfo.OnlineUsers.Count > 0);
            Assert.IsTrue(messageInfo.OnlineUsers.Any(u => u == imUser1));

            //verify that there is one message from this user
            RemoteUserMessage[] messageUsers = spiraSoapClient.Message_GetUnreadMessageSenders(credentials);
            Assert.AreEqual(1, messageUsers.Length);
            Assert.AreEqual(imUser2, messageUsers[0].UserId);
            Assert.AreEqual(1, messageUsers[0].UnreadMessages);

            //Retrieve the message
            RemoteMessage[] messages = spiraSoapClient.Message_RetrieveUnread(credentials);
            Assert.AreEqual(messageUsers[0].UnreadMessages, messages.Length);
            Assert.AreEqual(imUser2, messages[0].SenderUser.UserId);
            Assert.AreEqual(imUser1, messages[0].RecipientUser.UserId);
            Assert.AreEqual("This is a message 1 from user 2", messages[0].Body);

            //Mark it as read
            spiraSoapClient.Message_MarkAllAsRead(credentials, imUser2);

            //Verify no more unread messages from this user and in general
            messageInfo = spiraSoapClient.Message_GetInfo(credentials);
            Assert.AreEqual(0, messageInfo.UnreadMessages);
            messageUsers = spiraSoapClient.Message_GetUnreadMessageSenders(credentials);
            Assert.AreEqual(0, messageUsers.Length);

            credentials = spiraSoapClient.Connection_Authenticate("imuser1", "{C9C569C6-2BBF-47C2-B498-1238F8A7A0AF}", "NUnit");
            Assert.IsNotNull(credentials);

            //Now we need to verify the contact management part
            RemoteUser[] contacts = spiraSoapClient.User_RetrieveContacts(credentials);
            Assert.AreEqual(0, contacts.Length);

            //Add a contact to the second user
            spiraSoapClient.User_AddContact(credentials, imUser2);

            //Verify the contact is added
            contacts = spiraSoapClient.User_RetrieveContacts(credentials);
            Assert.AreEqual(1, contacts.Length);
            Assert.AreEqual("imuser2", contacts[0].UserName);

            //Remove the contact
            spiraSoapClient.User_RemoveContact(credentials, imUser2);

            //Verify the contact is removed
            contacts = spiraSoapClient.User_RetrieveContacts(credentials);
            Assert.AreEqual(0, contacts.Length);

            //Delete the new users, need to delete messages first
            new MessageManager().Message_PurgeOld(true, DateTime.UtcNow.AddDays(1));

            //Need to use business classes for physical deletes
            new UserManager().DeleteUser(imUser1, true);
            new UserManager().DeleteUser(imUser2, true);
        }

        /// <summary>Tests that we can view the history of changes made to an artifact</summary>
        [Test, SpiraTestCase(2004)]
        public void _44_RetrieveArtifactHistory()
        {
            //Authenticate and connect with sample project
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the history of a requirement (unfiltered)
            RemoteHistoryChange[] changes = spiraSoapClient.History_RetrieveForArtifact1(credentials, 1, /*Requirement*/1, 4, 1, Int32.MaxValue, "ChangeDate", "DESC");
            Assert.AreEqual(2, changes.Length);
            Assert.AreEqual(12, changes[0].ChangeSetId);
            Assert.AreEqual("Status", changes[0].FieldCaption);
            Assert.AreEqual("In Progress", changes[0].OldValue);
            Assert.AreEqual("Developed", changes[0].NewValue);
            Assert.AreEqual(4, changes[1].ChangeSetId);
            Assert.AreEqual("Status", changes[1].FieldCaption);
            Assert.AreEqual("Requested", changes[1].OldValue);
            Assert.AreEqual("In Progress", changes[1].NewValue);

            //Try filtering
            RemoteFilter remoteFilter = new RemoteFilter();
            remoteFilter.StringValue = "Progress";
            remoteFilter.PropertyName = "OldValue";
            changes = spiraSoapClient.History_RetrieveForArtifact2(credentials, 1, /*Requirement*/1, 4, 1, Int32.MaxValue, "ChangeDate", "DESC", new RemoteFilter[] { remoteFilter });
            Assert.AreEqual(1, changes.Length);
            Assert.AreEqual(12, changes[0].ChangeSetId);
            Assert.AreEqual("Status", changes[0].FieldCaption);
            Assert.AreEqual("In Progress", changes[0].OldValue);
            Assert.AreEqual("Developed", changes[0].NewValue);

            //Retrieve a single change
            RemoteHistoryChangeSet changeSet = spiraSoapClient.History_RetrieveById(credentials, 1, 12);
            Assert.AreEqual(12, changeSet.HistoryChangeSetId);
            Assert.AreEqual("Modified", changeSet.ChangeTypeName);
            Assert.AreEqual("Status", changeSet.Changes[0].FieldCaption);
            Assert.AreEqual("In Progress", changeSet.Changes[0].OldValue);
            Assert.AreEqual("Developed", changeSet.Changes[0].NewValue);
        }

        /// <summary>Tests that we can get the subscriptions and saved searches for a user and that we can subscribe/unsubscribe from an item</summary>
        [Test, SpiraTestCase(2019)]
        public void _45_SubscriptionsAndSavedSearches()
        {
            //Authenticate and connect with sample project as Fred Bloggs
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

            //First verify that we can get the list of saved searches for a user
            RemoteSavedFilter[] savedFilters = spiraSoapClient.SavedFilter_RetrieveForUser(credentials);
            Assert.AreEqual(6, savedFilters.Length);
            Assert.AreEqual("Critical Not-Covered Requirements", savedFilters[0].Name);
            Assert.AreEqual(/*Requirement*/1, savedFilters[0].ArtifactTypeId);
            Assert.AreEqual("New Unassigned Incidents", savedFilters[3].Name);
            Assert.AreEqual(/*Incident*/3, savedFilters[3].ArtifactTypeId);

            //Get the details of one saved search
            RemoteSavedFilter savedFilter = savedFilters[0];
            Assert.AreEqual(1, savedFilter.ProjectId);
            Assert.AreEqual("Library Information System (Sample)", savedFilter.ProjectName);
            Assert.AreEqual(2, savedFilter.Filters.Count);
            Assert.AreEqual("CoverageId", savedFilter.Filters[0].PropertyName);
            Assert.AreEqual(1, savedFilter.Filters[0].IntValue);
            Assert.AreEqual("ImportanceId", savedFilter.Filters[1].PropertyName);
            Assert.AreEqual(1, savedFilter.Filters[1].IntValue);

            //Now verify that the admin is not subscribed to anything
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
            RemoteArtifactSubscription[] subscriptions = spiraSoapClient.Subscription_RetrieveForUser(credentials);
            Assert.AreEqual(0, subscriptions.Length);

            //Subscribe to a requirement and verify
            spiraSoapClient.Subscription_SubscribeToArtifact(credentials, 1, 1, 4);
            subscriptions = spiraSoapClient.Subscription_RetrieveForUser(credentials);
            Assert.AreEqual(1, subscriptions.Length);
            Assert.AreEqual("Ability to add new books to the system", subscriptions[0].ArtifactName);
            Assert.AreEqual(4, subscriptions[0].ArtifactId);
            Assert.AreEqual("Requirement", subscriptions[0].ArtifactTypeName);

            //Verify that the user is subscribed
            subscriptions = spiraSoapClient.Subscription_RetrieveForArtifact(credentials, 1, 1, 4);
            Assert.IsTrue(subscriptions.Length > 0);
            Assert.IsTrue(subscriptions.Any(s => s.UserId == 1));

            //Unsubscribe
            spiraSoapClient.Subscription_UnsubscribeFromArtifact(credentials, 1, 1, 4);

            //Verify it unsubscribed
            subscriptions = spiraSoapClient.Subscription_RetrieveForUser(credentials);
            Assert.AreEqual(0, subscriptions.Length);
        }

        /// <summary>
        /// Tests that we can retrieve the test configurations in the sample project
        /// </summary>
        [Test, SpiraTestCase(2015)]
        public void _46_RetrieveTestConfigurations()
        {
            //Authenticate and connect with sample project as Fred Bloggs
            credentials = spiraSoapClient.Connection_Authenticate("fredbloggs", API_KEY_FRED_BLOGGS, PLUGIN_NAME);

            //Retrieve the list of test configuration sets in the project
            RemoteTestConfigurationSet[] testConfigurationSets = spiraSoapClient.TestConfiguration_RetrieveSets(credentials, 1);
            Assert.AreEqual(3, testConfigurationSets.Length);
            Assert.AreEqual("Complete testing data, with browsers, operating systems and logins", testConfigurationSets[0].Name);
            Assert.AreEqual(true, testConfigurationSets[0].IsActive);
            Assert.AreEqual("List of library information system logins and passwords", testConfigurationSets[1].Name);
            Assert.AreEqual(true, testConfigurationSets[1].IsActive);
            Assert.AreEqual("Target web browsers and operating systems", testConfigurationSets[2].Name);
            Assert.AreEqual(true, testConfigurationSets[2].IsActive);

            //Retrieve the test configuration set associated with a specific test set (none currently)
            RemoteTestConfigurationSet testConfigurationSet = spiraSoapClient.TestConfiguration_RetrieveForTestSet(credentials, 1, 1);
            Assert.IsNull(testConfigurationSet);

            //Retrieve a test configuration set by its id and verify the entries as well
            testConfigurationSet = spiraSoapClient.TestConfiguration_RetrieveSetById(credentials, 1, 1);
            Assert.IsNotNull(testConfigurationSet);
            Assert.AreEqual("Target web browsers and operating systems", testConfigurationSet.Name);
            Assert.AreEqual("This set of data consists of all the web browsers and operating systems that the application needs to be tested with", testConfigurationSet.Description);
            Assert.AreEqual(true, testConfigurationSet.IsActive);

            //The entries
            Assert.AreEqual(6, testConfigurationSet.Entries.Count);
            Assert.AreEqual(2, testConfigurationSet.Entries[0].ParameterValues.Count);
            Assert.AreEqual("browserName", testConfigurationSet.Entries[0].ParameterValues[0].Name);
            Assert.AreEqual("Internet Explorer", testConfigurationSet.Entries[0].ParameterValues[0].Value);
            Assert.AreEqual("operatingSystem", testConfigurationSet.Entries[0].ParameterValues[1].Name);
            Assert.AreEqual("Windows 8", testConfigurationSet.Entries[0].ParameterValues[1].Value);
            Assert.AreEqual("browserName", testConfigurationSet.Entries[1].ParameterValues[0].Name);
            Assert.AreEqual("Firefox", testConfigurationSet.Entries[1].ParameterValues[0].Value);
            Assert.AreEqual("operatingSystem", testConfigurationSet.Entries[1].ParameterValues[1].Name);
            Assert.AreEqual("Windows 8", testConfigurationSet.Entries[1].ParameterValues[1].Value);
        }

        /// <summary>
        /// Tests that we can create, modify, delete project templates as well as retrieve
        /// some of the key fields for various artifact types (priorities, statuses, types)
        /// </summary>
        [Test]
        [SpiraTestCase(2027)]
        public void _47_ProjectTemplates()
        {
            //Authenticate as the system administrator
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //Get the list of project templates
            RemoteProjectTemplate[] templates = spiraSoapClient.ProjectTemplate_Retrieve(credentials);
            Assert.AreEqual(5, templates.Length);

            //Now create a new project template not based on an existing one
            RemoteProjectTemplate projectTemplate = new RemoteProjectTemplate();
            projectTemplate.Name = "API Testing Template 4";
            projectTemplate.Description = "Test 4";
            projectTemplate.IsActive = true;
            projectTemplate = spiraSoapClient.ProjectTemplate_Create(credentials, null, projectTemplate);
            projectTemplateId4 = projectTemplate.ProjectTemplateId.Value;

            //Now create a new project template based on an existing one
            projectTemplate = new RemoteProjectTemplate();
            projectTemplate.Name = "API Testing Template 5";
            projectTemplate.Description = "Test 5";
            projectTemplate.IsActive = true;
            projectTemplate = spiraSoapClient.ProjectTemplate_Create(credentials, projectTemplateId4, projectTemplate);
            projectTemplateId5 = projectTemplate.ProjectTemplateId.Value;

            //Verify the count and specific ones
            templates = spiraSoapClient.ProjectTemplate_Retrieve(credentials);
            Assert.AreEqual(7, templates.Length);
            projectTemplate = templates.FirstOrDefault(t => t.ProjectTemplateId == projectTemplateId4);
            Assert.IsNotNull(projectTemplate);
            Assert.AreEqual("API Testing Template 4", projectTemplate.Name);
            Assert.AreEqual("Test 4", projectTemplate.Description);
            Assert.AreEqual(true, projectTemplate.IsActive);

            //Now retrieve a specific template and make a change
            projectTemplate = spiraSoapClient.ProjectTemplate_RetrieveById(credentials, projectTemplateId5);
            projectTemplate.Name = "API Testing Template 5a";
            projectTemplate.Description = null;
            projectTemplate.IsActive = false;
            spiraSoapClient.ProjectTemplate_Update(credentials, projectTemplateId5, projectTemplate);

            //Verify the change
            projectTemplate = spiraSoapClient.ProjectTemplate_RetrieveById(credentials, projectTemplateId5);
            Assert.IsNotNull(projectTemplate);
            Assert.AreEqual("API Testing Template 5a", projectTemplate.Name);
            Assert.IsNull(projectTemplate.Description);
            Assert.AreEqual(false, projectTemplate.IsActive);

            //Now delete this template
            spiraSoapClient.ProjectTemplate_Delete(credentials, projectTemplateId5);

            //Verify
            templates = spiraSoapClient.ProjectTemplate_Retrieve(credentials);
            Assert.AreEqual(6, templates.Length);
            bool exceptionThrown = false;
            try
            {
                projectTemplate = spiraSoapClient.ProjectTemplate_RetrieveById(credentials, projectTemplateId5);
            }
            catch (Exception exception)
            {
                if (exception.Message == "404 Not Found")
                {
                    exceptionThrown = true;
                }
            }
            Assert.IsTrue(exceptionThrown);

            //Now we need to verify the default fields get populate for the remaining template

            //Before we do that, we have to associate the template with a project (since no template owner membership exists yet)
            RemoteProject remoteProject = new RemoteProject();
            remoteProject.Name = "Test Project 4";
            remoteProject.ProjectTemplateId = projectTemplateId4;
            remoteProject.Active = true;
            remoteProject = spiraSoapClient.Project_Create(credentials, null, remoteProject);
            projectId4 = remoteProject.ProjectId.Value;

            //Tasks
            RemoteTaskType[] taskTypes = spiraSoapClient.Task_RetrieveTypes(credentials, projectTemplateId4);
            Assert.AreEqual(7, taskTypes.Length);
            RemoteTaskPriority[] taskPriorities = spiraSoapClient.Task_RetrievePriorities(credentials, projectTemplateId4);
            Assert.AreEqual(4, taskPriorities.Length);
            RemoteTaskStatus[] taskStatuses = spiraSoapClient.Task_RetrieveStatuses(credentials, projectTemplateId4);
            Assert.AreEqual(9, taskStatuses.Length);

            //Test Cases
            RemoteTestCaseType[] testCaseTypes = spiraSoapClient.TestCase_RetrieveTypes(credentials, projectTemplateId4);
            Assert.AreEqual(12, testCaseTypes.Length);
            RemoteTestCasePriority[] testCasePriorities = spiraSoapClient.TestCase_RetrievePriorities(credentials, projectTemplateId4);
            Assert.AreEqual(4, testCasePriorities.Length);
            RemoteTestCaseStatus[] testCaseStatuses = spiraSoapClient.TestCase_RetrieveStatuses(credentials, projectTemplateId4);
            Assert.AreEqual(8, testCaseStatuses.Length);

            //Requirements
            RemoteRequirementType[] requirementTypes = spiraSoapClient.Requirement_RetrieveTypes(credentials, projectTemplateId4);
            Assert.AreEqual(6, requirementTypes.Length);
            RemoteRequirementImportance[] requirementImportances = spiraSoapClient.Requirement_RetrieveImportances(credentials, projectTemplateId4);
            Assert.AreEqual(4, requirementImportances.Length);
            RemoteRequirementStatus[] requirementStatuses = spiraSoapClient.Requirement_RetrieveStatuses(credentials, projectTemplateId4);
            Assert.AreEqual(16, requirementStatuses.Length);

            //Delete the remaining template (clean up) and project
            spiraSoapClient.Project_Delete(credentials, projectId4);
            spiraSoapClient.ProjectTemplate_Delete(credentials, projectTemplateId4);
        }

        /// <summary>
        /// Tests that you can run and schedule the reports through the API
        /// </summary>
        [Test]
        [SpiraTestCase(2071)]
        public void _48_Reports_Generation()
        {
            //Authenticate as the system administrator
            credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

            //First lets run an ESQL query and see that we get data back (using LIS sample project)
            RemoteTableData tableData = spiraSoapClient.Reports_RetrieveESQLQueryData(credentials, "select INC.INCIDENT_ID, INC.NAME as INCIDENT_NAME from SpiraTestEntities.R_Incidents as INC where INC.PROJECT_ID = " + PROJECT_ID);
            Assert.IsNotNull(tableData);
            Assert.AreEqual(2, tableData.Columns.Count);
            Assert.AreEqual("INCIDENT_ID", tableData.Columns[0].Name);
            Assert.AreEqual("INCIDENT_NAME", tableData.Columns[1].Name);
            Assert.AreEqual(60, tableData.Rows.Count);

            //We have to first create a new custom graph outside the API
            string query = @"select R.EXECUTION_STATUS_NAME, COUNT (R.TEST_RUN_ID) as COUNT
from SpiraTestEntities.R_TestRuns as R
where R.PROJECT_ID = ${ProjectId}
group by R.EXECUTION_STATUS_NAME";
            graphCustomId1 = new GraphManager().GraphCustom_Create("Test Graph", query, true, null, null);

            //Next lets get the data grid associated with the custom graph for the project
            tableData = spiraSoapClient.Reports_RetrieveCustomGraphData(credentials, graphCustomId1, PROJECT_ID, null);
            Assert.IsNotNull(tableData);
            Assert.AreEqual(2, tableData.Columns.Count);
            Assert.AreEqual("EXECUTION_STATUS_NAME", tableData.Columns[0].Name);
            Assert.AreEqual("COUNT", tableData.Columns[1].Name);
            Assert.AreEqual(4, tableData.Rows.Count);

            //Get the list of saved reports (should be 0)
            RemoteSavedReport[] savedReports = spiraSoapClient.Reports_RetrieveSaved(credentials, PROJECT_ID, true);
            Assert.AreEqual(0, savedReports.Length);

            //Now we need to create a saved report based on one of the standard reports
            //We wil run it in HTML format to save time during unit testing

            //Insert a non-shared report for the project (cannot be done through the API at present)
            const int REQUIREMENTS_REPORT_ID = 2;
            const int HTML_FORMAT_ID = 1;
            reportSavedId = new ReportManager().InsertSaved(
                REQUIREMENTS_REPORT_ID,
                HTML_FORMAT_ID,
                USER_ID_SYSTEM_ADMIN,
                PROJECT_ID,
                "Requirements Report 1",
                "reportFormatId=1&e_3_2=1&e_3_9=1&e_3_8=1&e_3_3=1&e_3_1=1&e_3_4=1&af_3_17=2&af_3_16=1,7&af_3_20=2009-09-17%7c2009-09-18&af_3_19=1",
                true
                ).ReportSavedId;

            //Get the list of saved reports (should be 1)
            savedReports = spiraSoapClient.Reports_RetrieveSaved(credentials, PROJECT_ID, true);
            Assert.AreEqual(1, savedReports.Length);
            Assert.AreEqual(reportSavedId, savedReports[0].SavedReportId);

            //Now we need to start generating the report asynchronously
            string reportRequestGuid = spiraSoapClient.Reports_GenerateSavedReport(credentials, PROJECT_ID, reportSavedId);

            //Check on the status (stop after 5 minutes)
            DateTime startTime = DateTime.UtcNow;
            int generatedReportId;
            do
            {
                generatedReportId = spiraSoapClient.Reports_CheckGeneratedReportStatus(credentials, PROJECT_ID, reportRequestGuid);
            }
            while (generatedReportId == 0 && DateTime.UtcNow < startTime.AddMinutes(5));
            Assert.IsTrue(generatedReportId > 0, "result code=" + generatedReportId);   //Success > 0, Failure = -1, Never finished = 0

            //Open the rpeort and verify that we get data back
            byte[] data = spiraSoapClient.Reports_RetrieveGeneratedReport(credentials, PROJECT_ID, generatedReportId);
            Assert.IsTrue(data.Length > 0);

            //Convert into text and verify it contains the title
            string reportText = System.Text.Encoding.UTF8.GetString(data);
            Assert.IsTrue(reportText.Contains("Requirements Detailed Report"));
        }

		/// <summary>
		/// Tests that you can import risks and mitigations and retrieve them afterwards.
		/// </summary>
		[Test]
		[SpiraTestCase(2758)]
		public void _49_Import_Retrieve_Risks()
		{
			//First lets authenticate and connect to the project
			credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

			//Next lets get the various template fields we will need later
			int rk_typeFinancialId = spiraSoapClient.Risk_RetrieveTypes(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Financial").RiskTypeId;
			int rk_typeScheduleId = spiraSoapClient.Risk_RetrieveTypes(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Schedule").RiskTypeId;
			int rk_statusIdentifiedId = spiraSoapClient.Risk_RetrieveStatuses(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Identified").RiskStatusId;
			int rk_statusClosedId = spiraSoapClient.Risk_RetrieveStatuses(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Closed").RiskStatusId;
			int rk_probabilityLikelyId = spiraSoapClient.Risk_RetrieveProbabilities(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Likely").RiskProbabilityId.Value;
			int rk_impactMarginalId = spiraSoapClient.Risk_RetrieveImpacts(credentials, projectTemplateId1).FirstOrDefault(p => p.Name == "Marginal").RiskImpactId.Value;

			//Now lets create two risks and add some mitigations
			//The first one will have maximal fields populated
			RemoteRisk remoteRisk = new RemoteRisk();
			remoteRisk.ProjectId = projectId1;
			remoteRisk.Name = "The database may not support the volume";
			remoteRisk.Description = "We have chosen a low end database platform for this application and it may not be sufficient to handle the number of queries we may receive.";
			remoteRisk.ReleaseId = releaseId1;
			remoteRisk.ComponentId = componentId1;
			remoteRisk.CreatorId = USER_ID_FRED_BLOGGS;
			remoteRisk.OwnerId = USER_ID_JOE_SMITH;
			remoteRisk.RiskTypeId = rk_typeFinancialId;
			remoteRisk.RiskStatusId = rk_statusIdentifiedId;
			remoteRisk.RiskProbabilityId = rk_probabilityLikelyId;
			remoteRisk.RiskImpactId = rk_impactMarginalId;
			remoteRisk.ReviewDate = DateTime.UtcNow.AddDays(10);
			remoteRisk.CreationDate = DateTime.UtcNow;
			int riskId1 = spiraSoapClient.Risk_Create(credentials, projectId1, remoteRisk).RiskId.Value;

			//The second one will have minimal fields populated
			remoteRisk = new RemoteRisk();
			remoteRisk.ProjectId = projectId1;
			remoteRisk.Name = "The software licenses may be too expensive";
			remoteRisk.Description = "Our choice of technology stack may require us to use expensive software licenses which could result in us exceeding the infrastructure budget.";
			remoteRisk.CreatorId = USER_ID_FRED_BLOGGS;
			remoteRisk.CreationDate = DateTime.UtcNow;
			int riskId2 = spiraSoapClient.Risk_Create(credentials, projectId1, remoteRisk).RiskId.Value;

			//Verify that we can retrieve the risks as a list and individually
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId1);
			Assert.AreEqual("The database may not support the volume", remoteRisk.Name);
			Assert.AreEqual("We have chosen a low end database platform for this application and it may not be sufficient to handle the number of queries we may receive.", remoteRisk.Description);
			Assert.AreEqual("Financial", remoteRisk.RiskTypeName);
			Assert.AreEqual("Identified", remoteRisk.RiskStatusName);
			Assert.AreEqual("Marginal", remoteRisk.RiskImpactName);
			Assert.AreEqual("Likely", remoteRisk.RiskProbabilityName);
			Assert.AreEqual(8, remoteRisk.RiskExposure);
			Assert.AreEqual(releaseId1, remoteRisk.ReleaseId);
			Assert.AreEqual(componentId1, remoteRisk.ComponentId);

			//Get all the risks
			RemoteRisk[] risks = spiraSoapClient.Risk_Retrieve(credentials, projectId1, 1, Int32.MaxValue, "RiskId", true, null);
			Assert.AreEqual(2, risks.Length);
			long count = spiraSoapClient.Risk_Count1(credentials, projectId1);
			Assert.AreEqual(2, count);
			count = spiraSoapClient.Risk_Count2(credentials, projectId1, null);
			Assert.AreEqual(2, count);

			//Filter and sort
			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();
			remoteFilters.Add(new RemoteFilter() { PropertyName = "Name", StringValue = "database" });
			risks = spiraSoapClient.Risk_Retrieve(credentials, projectId1, 1, Int32.MaxValue, "RiskStatusName", true, remoteFilters.ToArray());
			Assert.AreEqual(1, risks.Length);

			//Retrieve for owner
			risks = spiraSoapClient.Risk_RetrieveForOwner(credentials);
			Assert.AreEqual(0, risks.Length);

			//Now we need to update the second risk and make sure the updates are handled OK
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId2);
			Assert.AreEqual("The software licenses may be too expensive", remoteRisk.Name);
			Assert.AreEqual("Our choice of technology stack may require us to use expensive software licenses which could result in us exceeding the infrastructure budget.", remoteRisk.Description);
			Assert.AreEqual("Business", remoteRisk.RiskTypeName);
			Assert.AreEqual("Identified", remoteRisk.RiskStatusName);
			Assert.IsTrue(String.IsNullOrEmpty(remoteRisk.RiskImpactName));
			Assert.IsTrue(String.IsNullOrEmpty(remoteRisk.RiskProbabilityName));
			Assert.IsNull(remoteRisk.ReleaseId);
			Assert.IsNull(remoteRisk.ComponentId);
			Assert.IsFalse(remoteRisk.RiskExposure.HasValue);

			//Make changes
			remoteRisk.Name = "The software licenses may be too expensive 2";
			remoteRisk.Description = "Our choice of technology stack may require us to use expensive software licenses which could result in us exceeding the infrastructure budget 2.";
			remoteRisk.RiskImpactId = rk_impactMarginalId;
			remoteRisk.RiskProbabilityId = rk_probabilityLikelyId;
			remoteRisk.RiskTypeId = rk_typeScheduleId;
			remoteRisk.ReleaseId = releaseId1;
			remoteRisk.ComponentId = componentId1;
			spiraSoapClient.Risk_Update(credentials, projectId1, remoteRisk);

			//Verify the changes
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId2);
			Assert.AreEqual("The software licenses may be too expensive 2", remoteRisk.Name);
			Assert.AreEqual("Our choice of technology stack may require us to use expensive software licenses which could result in us exceeding the infrastructure budget 2.", remoteRisk.Description);
			Assert.AreEqual("Schedule", remoteRisk.RiskTypeName);
			Assert.AreEqual("Identified", remoteRisk.RiskStatusName);
			Assert.AreEqual("Marginal", remoteRisk.RiskImpactName);
			Assert.AreEqual("Likely", remoteRisk.RiskProbabilityName);
			Assert.AreEqual(releaseId1, remoteRisk.ReleaseId);
			Assert.AreEqual(componentId1, remoteRisk.ComponentId);
			Assert.AreEqual(8, remoteRisk.RiskExposure);

			//First lets authenticate and connect to the project
			credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);

			//Now delete this risk
			spiraSoapClient.Risk_Delete(credentials, projectId1, riskId2);

			//First lets authenticate and connect to the project
			credentials = spiraSoapClient.Connection_Authenticate("aant", API_KEY_ADAM_ANT, PLUGIN_NAME);

			//Verify
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId2);
			Assert.IsNull(remoteRisk);
			risks = spiraSoapClient.Risk_Retrieve(credentials, projectId1, 1, Int32.MaxValue, "RiskId", true, null);
			Assert.AreEqual(1, risks.Length);

			//Now lets add some mitigations and comments to the remaining risk
			RemoteRiskMitigation remoteRiskMitigation = new RemoteRiskMitigation();
			remoteRiskMitigation.RiskId = riskId1;
			remoteRiskMitigation.Description = "Mitigation 1";
			remoteRiskMitigation.CreationDate = DateTime.UtcNow;
			remoteRiskMitigation.IsActive = true;
			int mitigationId1 = spiraSoapClient.Risk_AddMitigation(credentials, projectId1, riskId1, null, USER_ID_FRED_BLOGGS, remoteRiskMitigation).RiskMitigationId.Value;

			remoteRiskMitigation = new RemoteRiskMitigation();
			remoteRiskMitigation.RiskId = riskId1;
			remoteRiskMitigation.Description = "Mitigation 2";
			remoteRiskMitigation.CreationDate = DateTime.UtcNow;
			remoteRiskMitigation.IsActive = true;
			int mitigationId2 = spiraSoapClient.Risk_AddMitigation(credentials, projectId1, riskId1, null, USER_ID_FRED_BLOGGS, remoteRiskMitigation).RiskMitigationId.Value;

			//Verify
			RemoteRiskMitigation[] mitigations = spiraSoapClient.Risk_RetrieveMitigations(credentials, projectId1, riskId1);
			Assert.AreEqual(2, mitigations.Length);
			remoteRiskMitigation = spiraSoapClient.Risk_RetrieveMitigationById(credentials, projectId1, riskId1, mitigationId1);
			Assert.AreEqual("Mitigation 1", remoteRiskMitigation.Description);
			Assert.AreEqual(1, remoteRiskMitigation.Position);

			//Make changes
			remoteRiskMitigation.Description = "Mitigation 1a";
			remoteRiskMitigation.ReviewDate = DateTime.UtcNow.AddDays(5);
			spiraSoapClient.Risk_UpdateMitigation(credentials, projectId1, riskId1, remoteRiskMitigation);

			//Verify
			remoteRiskMitigation = spiraSoapClient.Risk_RetrieveMitigationById(credentials, projectId1, riskId1, mitigationId1);
			Assert.AreEqual("Mitigation 1a", remoteRiskMitigation.Description);
			Assert.AreEqual(1, remoteRiskMitigation.Position);

			//Delete a mitigation
			spiraSoapClient.Risk_DeleteMitigation(credentials, projectId1, riskId1, mitigationId1);

			//Verify
			mitigations = spiraSoapClient.Risk_RetrieveMitigations(credentials, projectId1, riskId1);
			Assert.AreEqual(1, mitigations.Length);

			//Add comments
			RemoteComment remoteComment = new RemoteComment();
			remoteComment.ArtifactId = riskId1;
			remoteComment.Text = "Comment 1";
			spiraSoapClient.Risk_CreateComment(credentials, projectId1, riskId1, remoteComment);
			remoteComment = new RemoteComment();
			remoteComment.ArtifactId = riskId1;
			remoteComment.Text = "Comment 2";
			spiraSoapClient.Risk_CreateComment(credentials, projectId1, riskId1, remoteComment);

			//Verify
			RemoteComment[] comments = spiraSoapClient.Risk_RetrieveComments(credentials, projectId1, riskId1);
			Assert.AreEqual(2, comments.Length);


			//Update template setting to not allow bulk edit changes
			ProjectTemplateSettings projectTemplateSettings = new ProjectTemplateSettings(projectTemplateId1);
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = false;
			projectTemplateSettings.Save();

			//Try and update a status
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId1);
			remoteRisk.RiskStatusId = rk_statusClosedId;
			spiraSoapClient.Risk_Update(credentials, projectId1, remoteRisk);

			//Verify that the status did not change
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId1);
			Assert.AreEqual(rk_statusIdentifiedId, remoteRisk.RiskStatusId, "RiskStatusId");

			//Try adding an artifact with a non default status
			remoteRisk = new RemoteRisk();
			remoteRisk.ProjectId = projectId1;
			remoteRisk.Name = "Template does not let you set a non default status";
			remoteRisk.Description = "Template does not let you set a non default status.";
			remoteRisk.CreatorId = USER_ID_FRED_BLOGGS;
			remoteRisk.OwnerId = USER_ID_JOE_SMITH;
			remoteRisk.RiskTypeId = rk_typeFinancialId;
			remoteRisk.RiskStatusId = rk_statusClosedId;
			remoteRisk.CreationDate = DateTime.UtcNow;
			int riskId3 = spiraSoapClient.Risk_Create(credentials, projectId1, remoteRisk).RiskId.Value;

			//Verify that the status did not change
			remoteRisk = spiraSoapClient.Risk_RetrieveById(credentials, projectId1, riskId1);
			Assert.AreEqual(rk_statusIdentifiedId, remoteRisk.RiskStatusId, "RiskStatusId");

			//Clean up
			projectTemplateSettings.Workflow_BulkEditCanChangeStatus = true;
			projectTemplateSettings.Save();
			RiskManager riskManager = new RiskManager();
			riskManager.Risk_MarkAsDeleted(projectId1, riskId3, 1);
		}

		/// <summary>
		/// Tests that you can read data from the event log
		/// NOTE: we always execute the rest command first as that may add events to the event log 
		/// </summary>
		[Test]
		[SpiraTestCase(2757)]
		public void _50_Event_Logs()
		{
			//Setup
			credentials = spiraSoapClient.Connection_Authenticate("administrator", API_KEY_SYSTEM_ADMIN, PLUGIN_NAME);
			EventManager eventManager = new EventManager();
			string sortString = "EventTimeUtc DESC";
			string restSortField = "EventTimeUtc";
			string restSortDirection = "DESC";
			int pageIndex = 0;
			int pageSize = 10;
			double utcOffset = InternalRoutines.UTC_OFFSET;
			int count;
			List<RemoteFilter> remoteFilters = new List<RemoteFilter>();

			//Verify can retrieve log without filters
			RemoteEvent2[] remoteEvents = spiraSoapClient.System_RetrieveEvents(credentials, pageIndex + 1, pageSize, restSortField, restSortDirection, remoteFilters.ToArray());
			List<Event> events = eventManager.GetEvents(null, sortString, pageIndex, pageSize, utcOffset, out count);
			Assert.AreEqual(remoteEvents.Length, events.Count);
			Assert.AreEqual(remoteEvents[0].EventId, events[0].EventId);
			Assert.AreEqual(remoteEvents[0].EventTimeUtc, events[0].EventTimeUtc);
			Assert.AreEqual(remoteEvents[0].Message, events[0].Message);
			Assert.AreEqual(remoteEvents[0].Details, events[0].Details);

			//Verify sort direction
			restSortDirection = "ASC";
			sortString = "EventTimeUtc ASC";
			remoteEvents = spiraSoapClient.System_RetrieveEvents(credentials, pageIndex + 1, pageSize, restSortField, restSortDirection, remoteFilters.ToArray());
			events = eventManager.GetEvents(null, sortString, pageIndex, pageSize, utcOffset, out count);
			Assert.AreEqual(remoteEvents.Length, events.Count);
			Assert.AreEqual(remoteEvents[0].EventId, events[0].EventId);
			Assert.AreEqual(remoteEvents[0].EventTimeUtc, events[0].EventTimeUtc);
			Assert.AreEqual(remoteEvents[0].Message, events[0].Message);
			Assert.AreEqual(remoteEvents[0].Details, events[0].Details);

			//Verify pageIndex works
			pageIndex = 10;
			remoteEvents = spiraSoapClient.System_RetrieveEvents(credentials, pageIndex + 1, pageSize, restSortField, restSortDirection, remoteFilters.ToArray());
			events = eventManager.GetEvents(null, sortString, pageIndex, pageSize, utcOffset, out count);
			Assert.AreEqual(remoteEvents.Length, events.Count);
			Assert.AreEqual(remoteEvents[0].EventId, events[0].EventId);
			Assert.AreEqual(remoteEvents[0].EventTimeUtc, events[0].EventTimeUtc);
			Assert.AreEqual(remoteEvents[0].Message, events[0].Message);
			Assert.AreEqual(remoteEvents[0].Details, events[0].Details);

			//Verify can sort by a different field
			restSortField = "Message";
			sortString = "Message ASC";
			remoteEvents = spiraSoapClient.System_RetrieveEvents(credentials, pageIndex + 1, pageSize, restSortField, restSortDirection, remoteFilters.ToArray());
			events = eventManager.GetEvents(null, sortString, pageIndex, pageSize, utcOffset, out count);
			Assert.AreEqual(remoteEvents.Length, events.Count);
			Assert.AreEqual(remoteEvents[0].EventId, events[0].EventId);
			Assert.AreEqual(remoteEvents[0].EventTimeUtc, events[0].EventTimeUtc);
			Assert.AreEqual(remoteEvents[0].Message, events[0].Message);
			Assert.AreEqual(remoteEvents[0].Details, events[0].Details);

			//Verify filter
			RemoteFilter remoteFilter = new RemoteFilter();
			remoteFilter.PropertyName = "Message";
			remoteFilter.StringValue = "EventLog";
			remoteFilters.Add(remoteFilter);
			Hashtable filters = new Hashtable();
			filters.Add("Message", "EventLog");
			remoteEvents = spiraSoapClient.System_RetrieveEvents(credentials, pageIndex + 1, pageSize, restSortField, restSortDirection, remoteFilters.ToArray());
			events = eventManager.GetEvents(filters, sortString, pageIndex, pageSize, utcOffset, out count);
			Assert.AreEqual(remoteEvents.Length, events.Count);
			if (remoteEvents.Length > 0)
			{
				Assert.AreEqual(remoteEvents[0].EventId, events[0].EventId);
				Assert.AreEqual(remoteEvents[0].EventTimeUtc, events[0].EventTimeUtc);
				Assert.AreEqual(remoteEvents[0].Message, events[0].Message);
				Assert.AreEqual(remoteEvents[0].Details, events[0].Details);
			}
		}
	}
}
