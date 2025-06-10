using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web.Security;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using System.Configuration;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>
	/// This fixture tests the UserManager business object
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class UserManagerTest
	{
		protected static Business.UserManager userManager;
		protected static int userId;
		protected static int userId2;
		private static string salt;

		private const int PROJECT_ID = 1;
		private const int USER_ID_ADMINISTRATOR = 1;
		private const int USER_ID_FRED_BLOGGS = 2;
		private const int USER_ID_JOE_SMITH = 3;

		[TestFixtureSetUp]
		public void Init()
		{
			userManager = new Business.UserManager();
			salt = GenerateSalt();
		}

		[TestFixtureTearDown]
		public void Dispose()
		{
			//Delete the newly created users
			userManager.DeleteUser(userId, true);
			userManager.DeleteUser(userId2, true);
		}

		[
		Test,
		SpiraTestCase(34)
		]
		public void _01_LookupRetrieves()
		{
			//Lets test that we can get the list of all users in the entire system (including inactive and unapproved)
			List<User> users = userManager.GetUsers(true);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(13, users.Count);
			User user = users.Find(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, user.UserId);
			Assert.AreEqual("Fred Bloggs", user.FullName);

			//Lets test that we can get the list of all active users in the entire system
			users = userManager.RetrieveActive();
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(12, users.Count);
			user = users.Find(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, user.UserId);
			Assert.AreEqual("Fred Bloggs", user.FullName);
			Assert.IsTrue(user.IsActive);

			//Now lets test that we can load the active users for a given project
			users = userManager.RetrieveActiveByProjectId(PROJECT_ID);
			//Make sure we have the number of records and values we expect
			Assert.AreEqual(12, users.Count);
			user = users.Find(u => u.UserId == USER_ID_FRED_BLOGGS);
			Assert.AreEqual(USER_ID_FRED_BLOGGS, user.UserId);
			Assert.AreEqual("Fred Bloggs", user.FullName);
		}

		[
		Test,
		SpiraTestCase(86)
		]
		public void _02_Retrieves()
		{
			//Make sure that we can load existing users from sample-data by user-name
			User user = userManager.GetUserByLogin("joesmith");
			Assert.AreEqual(3, user.UserId);
			Assert.AreEqual("Joe", user.Profile.FirstName);
			Assert.AreEqual("Smith", user.Profile.LastName);
			Assert.AreEqual("P", user.Profile.MiddleInitial);
			Assert.AreEqual("joesmith", user.UserName);
			Assert.AreEqual("What is 1+1?", user.PasswordQuestion);
			Assert.AreEqual("Joe P Smith", user.FullName);
			Assert.AreEqual("joesmith@mycompany.com", user.EmailAddress);
			Assert.AreEqual(1, user.Profile.LastOpenedProjectId);
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(true, user.IsActive);
			Assert.True(user.LastLoginDate.HasValue, "IsLastLoginDateNull");
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn), "IsLdapDnNull");

			user = userManager.GetUserByLogin("fredbloggs");
			Assert.AreEqual(2, user.UserId);
			Assert.AreEqual("Fred", user.Profile.FirstName);
			Assert.AreEqual("Bloggs", user.Profile.LastName);
			Assert.IsTrue(String.IsNullOrEmpty(user.Profile.MiddleInitial));
			Assert.AreEqual("fredbloggs", user.UserName);
			Assert.AreEqual("What is 1+1?", user.PasswordQuestion);
			Assert.AreEqual("Fred Bloggs", user.FullName);
			Assert.AreEqual("fredbloggs@mycompany.com", user.EmailAddress);
			Assert.IsFalse(user.Profile.LastOpenedProjectId.HasValue);
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(false, user.Profile.IsEmailEnabled);
			Assert.AreEqual(true, user.IsActive);
			Assert.IsTrue(user.LastLoginDate.HasValue, "IsLastLoginDateNull");
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn), "IsLdapDnNull");

			//Now make sure we can load the same users by user-id
			user = userManager.GetUserById(3);
			Assert.AreEqual(3, user.UserId);
			Assert.AreEqual("Joe", user.Profile.FirstName);
			Assert.AreEqual("Smith", user.Profile.LastName);
			Assert.AreEqual("P", user.Profile.MiddleInitial);
			Assert.AreEqual("joesmith", user.UserName);
			Assert.AreEqual("What is 1+1?", user.PasswordQuestion);
			Assert.AreEqual("Joe P Smith", user.FullName);
			Assert.AreEqual("joesmith@mycompany.com", user.EmailAddress);
			Assert.AreEqual(1, user.Profile.LastOpenedProjectId);
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(false, user.Profile.IsEmailEnabled);
			Assert.AreEqual(true, user.IsActive);
			Assert.IsTrue(user.LastLoginDate.HasValue, "IsLastLoginDateNull");
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn), "IsLdapDnNull");

			//Now lets test that we can retrieve a user by his/her RSS token
			user = userManager.RetrieveByRssToken("{7911E6B3-2C9E-4837-8B4E-96F3E2B37EFC}");
			Assert.AreEqual(3, user.UserId);
			Assert.AreEqual("Joe", user.Profile.FirstName);
			Assert.AreEqual("Smith", user.Profile.LastName);
			Assert.AreEqual("P", user.Profile.MiddleInitial);
			Assert.AreEqual("joesmith", user.UserName);
			Assert.AreEqual("{7911E6B3-2C9E-4837-8B4E-96F3E2B37EFC}", user.RssToken);

			user = userManager.GetUserById(2);
			Assert.AreEqual(2, user.UserId);
			Assert.AreEqual("Fred", user.Profile.FirstName);
			Assert.AreEqual("Bloggs", user.Profile.LastName);
			Assert.IsTrue(String.IsNullOrEmpty(user.Profile.MiddleInitial), "IsMiddleInitialNull");
			Assert.AreEqual("fredbloggs", user.UserName);
			Assert.AreEqual("What is 1+1?", user.PasswordQuestion);
			Assert.AreEqual("Fred Bloggs", user.FullName);
			Assert.AreEqual("fredbloggs@mycompany.com", user.EmailAddress);
			Assert.IsFalse(user.Profile.LastOpenedProjectId.HasValue, "IsLastOpenedProjectIdNull");
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(true, user.IsActive);
			Assert.AreEqual(false, user.Profile.IsEmailEnabled);
			Assert.IsTrue(user.LastLoginDate.HasValue, "IsLastLoginDateNull");
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn), "IsLdapDnNull");

			//Now lets test that we can retrieve the list of users in the system un-filtered
			List<User> users = userManager.GetUsers(true);
			Assert.AreEqual(13, users.Count);
			Assert.AreEqual("Fred Bloggs", users[4].FullName);
			Assert.AreEqual("System Administrator", users[0].FullName);

			//Now lets test that we can retrieve the list of users in the system filtered
			Hashtable filters = new Hashtable();
			string sortExpression = "UserName ASC";
			int count;
			filters.Add("Profile.FirstName", "J");
			filters.Add("Profile.MiddleInitial", "P");
			filters.Add("Profile.LastName", "S");
			filters.Add("UserName", "j");
			filters.Add("Profile.IsAdmin", "N");
			filters.Add("IsActive", "Y");
			users = userManager.GetUsers(filters, sortExpression, 0, 99999, InternalRoutines.UTC_OFFSET, out count, false, true);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual("Joe P Smith", users[0].FullName);

			//Finally lets test that we can retrieve a list of users based on
			//a passed in list of arbitary user ids. This is used by the
			//Administration screen that displays active user sessions
			List<int> userIds = new List<int>();
			userIds.Add(2); //Fred Bloggs
			userIds.Add(3); //Joe Smith
			users = userManager.GetUsersByIds(userIds);

			//Verify the data retrieved
			Assert.AreEqual(2, users.Count);
			Assert.AreEqual("fredbloggs", users[0].UserName);
			Assert.AreEqual("joesmith", users[1].UserName);

			//Verify that we can retrieve a list of users that doesn't include existing members of a project or group
			//this is useful when we need to list users that can be added to a project or group

			//Project ID = 1
			filters.Clear();
			users = userManager.GetUsers(filters, sortExpression, 0, 99999, InternalRoutines.UTC_OFFSET, out count, false, false, 1, null);
			Assert.AreEqual(0, count);
			Assert.AreEqual(0, users.Count);

			//Project Group ID = 2
			users = userManager.GetUsers(filters, sortExpression, 0, 99999, InternalRoutines.UTC_OFFSET, out count, false, false, null, 2);
			Assert.AreEqual(10, count);
			Assert.AreEqual(10, users.Count);
			Assert.AreEqual("Amy E Cribbins", users[0].FullName);

			//First get a list of all users with profiles
			users = userManager.GetProfiles(0, 99999, out count);
			Assert.AreEqual(13, count);
			Assert.AreEqual(13, users.Count);
			Assert.AreEqual("Fred Bloggs", users[4].FullName);
			Assert.AreEqual("System Administrator", users[0].FullName);

			//Get a list of all users with profiles that have no activity since the current date
			users = userManager.GetInactiveProfiles(DateTime.UtcNow, 0, 99999, out count);
			Assert.AreEqual(13, count);
			Assert.AreEqual(13, users.Count);
			Assert.AreEqual("Fred Bloggs", users[4].FullName);
			Assert.AreEqual("System Administrator", users[0].FullName);

			//Get the list of profiles that match a login wildcard
			users = userManager.FindProfilesByLogin("joesmith", 0, 99999, out count);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual("Joe P Smith", users[0].FullName);

			//Get the list of user profiles that match a login wildcard that haven't had any user activity recently
			users = userManager.FindInactiveProfilesByLogin("joesmith", DateTime.UtcNow, 0, 99999, out count);
			Assert.AreEqual(1, count);
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual("Joe P Smith", users[0].FullName);

			//See how many users are currently online, could be any number, so just check that it's greater than -1
			int onlineUsers = userManager.GetNumberOfUsersOnline(10);
			Assert.IsTrue(onlineUsers > -1);

			//Verify that we can retrieve the list of users that belong to a certain project in a certain role.
			users = userManager.RetrieveByProjectRoleId(1, Business.ProjectManager.ProjectRoleProjectOwner); //Users who are project owner role on project PR1
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual(1, users[0].UserId);
			users = userManager.RetrieveByProjectRoleId(2, 3); //Users who are developer role on project PR2
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual(2, users[0].UserId);
		}

		[
		Test,
		SpiraTestCase(87)
		]
		public void _03_EditProfile()
		{
			string rssToken = "{" + System.Guid.NewGuid().ToString().ToUpper() + "}";

			//First lets make sure we can update a user's profile successfully
			User user = userManager.GetUserById(2);
			user.StartTracking();
			user.Profile.StartTracking();
			user.Profile.FirstName = "Samuel";
			user.Profile.MiddleInitial = "S";
			user.Profile.LastName = "Smithers";
			user.UserName = "samsmithers";
			user.Profile.LastOpenedProjectId = 2;
			user.EmailAddress = "samsmithers@spiratest.com";
			user.Profile.IsAdmin = true;
			user.IsActive = false;
			user.LdapDn = "CN=samsmithers,CN=Users,CN=SpiraTest,O=Inflectra,C=US";
			user.RssToken = rssToken;
			userManager.Update(user);

			//Verify the changes
			user = userManager.GetUserById(2);
			Assert.AreEqual(2, user.UserId);
			Assert.AreEqual("Samuel", user.Profile.FirstName);
			Assert.AreEqual("Smithers", user.Profile.LastName);
			Assert.IsFalse(String.IsNullOrEmpty(user.Profile.MiddleInitial));
			Assert.AreEqual("samsmithers", user.UserName);
			Assert.AreEqual("Samuel S Smithers", user.FullName);
			Assert.AreEqual("samsmithers@spiratest.com", user.EmailAddress);
			Assert.AreEqual(2, user.Profile.LastOpenedProjectId);
			Assert.AreEqual(true, user.Profile.IsAdmin);
			Assert.AreEqual(false, user.IsActive);
			Assert.AreEqual("CN=samsmithers,CN=Users,CN=SpiraTest,O=Inflectra,C=US", user.LdapDn);
			Assert.AreEqual(rssToken, user.RssToken);

			//Now we need to put it back to where it was
			user = userManager.GetUserById(2);
			user.StartTracking();
			user.Profile.StartTracking();
			user.Profile.FirstName = "Fred";
			user.Profile.MiddleInitial = null;
			user.Profile.LastName = "Bloggs";
			user.UserName = "fredbloggs";
			user.EmailAddress = "fredbloggs@spiratest.com";
			user.Profile.IsAdmin = false;
			user.IsActive = true;
			user.Profile.LastOpenedProjectId = null;
			user.LdapDn = null;
			user.RssToken = "{7A05FD06-83C3-4436-B37F-51BCF0060483}";
			userManager.Update(user);

			//Verify the changes
			user = userManager.GetUserById(2);
			Assert.AreEqual(2, user.UserId);
			Assert.AreEqual("Fred", user.Profile.FirstName);
			Assert.AreEqual("Bloggs", user.Profile.LastName);
			Assert.IsTrue(String.IsNullOrEmpty(user.Profile.MiddleInitial));
			Assert.AreEqual("fredbloggs", user.UserName);
			Assert.AreEqual("Fred Bloggs", user.FullName);
			Assert.AreEqual("fredbloggs@spiratest.com", user.EmailAddress);
			Assert.IsFalse(user.Profile.LastOpenedProjectId.HasValue);
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(true, user.IsActive);
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn));
			Assert.AreEqual("{7A05FD06-83C3-4436-B37F-51BCF0060483}", user.RssToken);

			//Make sure we can't change the user-name to one already in use
			user = userManager.GetUserById(2);
			user.StartTracking();
			user.UserName = "joesmith";
			bool exceptionThrown = false;

			try
			{
				userManager.Update(user);
			}
			catch (UserDuplicateUserNameException)
			{
				exceptionThrown = true;
			}

			//Make sure exception was thrown
			Assert.IsTrue(exceptionThrown, "System shouldn't allow duplicate user-names to be updated");

			//Now lets verify that we can update the same user profile through the ASP.NET Profile provider helper methods
			SettingsPropertyCollection properties = new SettingsPropertyCollection();
			properties.Add(new SettingsProperty("FirstName", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			properties.Add(new SettingsProperty("LastName", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			properties.Add(new SettingsProperty("Department", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
			userManager.GetProfileData(properties, values, "fredbloggs");

			//Verify the data
			Assert.AreEqual("Fred", (string)values["FirstName"].PropertyValue);
			Assert.AreEqual("Bloggs", (string)values["LastName"].PropertyValue);
			Assert.AreEqual("QA", (string)values["Department"].PropertyValue);

			//Update the data
			values["FirstName"].PropertyValue = "Boris";
			values["LastName"].PropertyValue = "Yeltsin";
			values["Department"].PropertyValue = "Government";
			userManager.SetProfileData(values, "fredloggs", true);

			//Verify the data
			Assert.AreEqual("Boris", (string)values["FirstName"].PropertyValue);
			Assert.AreEqual("Yeltsin", (string)values["LastName"].PropertyValue);
			Assert.AreEqual("Government", (string)values["Department"].PropertyValue);

			//Put it back
			values["FirstName"].PropertyValue = "Fred";
			values["LastName"].PropertyValue = "Bloggs";
			values["Department"].PropertyValue = "QA";
			userManager.SetProfileData(values, "fredloggs", true);

			//Verify the data
			Assert.AreEqual("Fred", (string)values["FirstName"].PropertyValue);
			Assert.AreEqual("Bloggs", (string)values["LastName"].PropertyValue);
			Assert.AreEqual("QA", (string)values["Department"].PropertyValue);
		}

		[
		Test,
		SpiraTestCase(88)
		]
		public void _04_UpdateLastOpenedProject()
		{
			//First get the current project id for a specific user
			User user = userManager.GetUserById(3);
			Assert.AreEqual(1, user.Profile.LastOpenedProjectId);

			//Now change the project id and verify the update
			userManager.UpdateLastOpenedProject(3, 2);
			user = userManager.GetUserById(3);
			Assert.AreEqual(2, user.Profile.LastOpenedProjectId);

			//Finally change back and verify the update
			userManager.UpdateLastOpenedProject(3, 1);
			user = userManager.GetUserById(3);
			Assert.AreEqual(1, user.Profile.LastOpenedProjectId);
		}

		[
		Test,
		SpiraTestCase(89)
		]
		public void _05_Authorization()
		{
			//Test to make sure the authorization rules work correctly
			bool authorizedUser = false;
			authorizedUser = userManager.Authorize(2, 3); //Should be TRUE
			Assert.IsTrue(authorizedUser, "Authorization failure");

			authorizedUser = userManager.Authorize(3, 3); //Should be FALSE
			Assert.IsFalse(authorizedUser, "Authorization failure");
		}

		[
		Test,
		SpiraTestCase(90)
		]
		public void _06_CreateNewUser()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			//Lets insert a new user and make sure it inserted correctly
			int errorCode;

			User user1 = new User();
			string name = "jjones";

			try
			{
				user1 = userManager.GetUserByLogin("jjones");

				if (user1 != null)
				{
					name = user1.UserName + "1";
				}
			}
			catch (ArtifactNotExistsException)
			{
				//This is expected, there should not be a user with this username
			}

			//This user has an RSS GUID token
			string rssToken = "{" + System.Guid.NewGuid().ToString().ToUpper() + "}";
			User user = userManager.CreateUser(name, "mypassword123", salt, "jjones@spiratest.com", "What is my first dog named?", "crusher", true, false, 1, null, rssToken, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			userId = user.UserId;

			//Verify that there were no errors
			Assert.AreEqual(0, errorCode);

			//Add a profile to this user
			user = userManager.GetUserById(userId);
			user.Profile = new UserProfile();
			user.Profile.FirstName = "Jim";
			user.Profile.LastName = "Jones";
			user.Profile.IsAdmin = false;
			user.Profile.IsEmailEnabled = true;
			user.Profile.Department = "Dept1";
			user.Profile.LastUpdateDate = DateTime.UtcNow;
			userManager.Update(user);

			//Verify the new user inserted correctly
			user = userManager.GetUserById(userId);
			Assert.AreEqual(userId, user.UserId);
			Assert.AreEqual("Jim", user.Profile.FirstName);
			Assert.AreEqual("Jones", user.Profile.LastName);
			Assert.IsTrue(String.IsNullOrEmpty(user.Profile.MiddleInitial));
			Assert.AreEqual(user.UserName, user.UserName);
			Assert.AreEqual("Jim Jones", user.FullName);
			Assert.AreEqual("jjones@spiratest.com", user.EmailAddress);
			Assert.IsFalse(user.Profile.LastOpenedProjectId.HasValue);
			Assert.AreEqual(false, user.Profile.IsAdmin);
			Assert.AreEqual(true, user.IsActive);
			Assert.AreEqual(true, user.Profile.IsEmailEnabled);
			Assert.AreEqual("Dept1", user.Profile.Department);
			Assert.IsTrue(String.IsNullOrEmpty(user.LdapDn));
			Assert.AreEqual(rssToken, user.RssToken);

			//Try and insert a user with the same user-name, make sure it returns an error code
			userManager.CreateUser(user.UserName, "mypassword123", salt, "bbob@spiratest.com", "What is my first dog named?", "crusher", true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);

			//Make sure exception was thrown
			Assert.AreEqual(6, errorCode, "System shouldn't allow duplicate user-names to be inserted");

			//Now try deleting this account
			userManager.DeleteUser(userId, true);

			//Verify the delete
		}

		[
		Test,
		SpiraTestCase(92)
		]
		public void _08_RetrieveProjectOwners()
		{
			//Make sure that we can retrieve a list of project owners
			List<User> users = userManager.RetrieveOwnersByProjectId(PROJECT_ID);

			//Verify the data returned
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual(UserManager.UserSystemAdministrator, users[0].UserId);
			Assert.AreEqual("System", users[0].Profile.FirstName);
			Assert.AreEqual("Administrator", users[0].Profile.LastName);
		}

		[
		Test,
		SpiraTestCase(191)
		]
		public void _09_RetrieveWorkflowNotifyList()
		{
			//Make sure that we can retrieve a list of people who should be notified about a change
			//in incident status based on their project role (i.e. not because they are owners, etc.)
			List<User> users = userManager.RetrieveNotifyListForWorkflowRole(1, 1, 3, 6);

			//Verify the data returned
			Assert.AreEqual(1, users.Count);
			Assert.AreEqual("Fred", users[0].Profile.FirstName);
			Assert.AreEqual("Bloggs", users[0].Profile.LastName);

			//Verify the negative case
			users = userManager.RetrieveNotifyListForWorkflowRole(1, 1, 1, 2);
			Assert.AreEqual(0, users.Count);
		}

		[
		Test,
		SpiraTestCase(327)
		]
		public void _10_StoreRetrieveUserSettings()
		{
			//Test that we can use the UserCollection class to store user settings data in the database
			//whilst using the Hashtable interface to make its access and storage seamless
			UserSettingsCollection userSettingsCollection = new UserSettingsCollection(USER_ID_FRED_BLOGGS, "MyPage.MyProjects");

			//Verify the existing contents
			userSettingsCollection.Restore();
			Assert.AreEqual(2, userSettingsCollection.Count);
			Assert.AreEqual(true, (bool)userSettingsCollection["Visible"]);
			Assert.AreEqual(false, (bool)userSettingsCollection["Minimized"]);

			//Lets try updating an entry and removing an entry
			userSettingsCollection["Visible"] = false;
			userSettingsCollection.Remove("Minimized");

			//Now save the properties
			userSettingsCollection.Save();

			//Now verify that the data was saved correctly
			userSettingsCollection.Restore();
			Assert.AreEqual(1, userSettingsCollection.Count);
			Assert.AreEqual(false, (bool)userSettingsCollection["Visible"]);
			Assert.IsNull(userSettingsCollection["Minimized"]);

			//Finally we need to return the data to its previous state.
			//This also tests that we can add an item
			userSettingsCollection["Visible"] = true;
			userSettingsCollection.Add("Minimized", false);
			userSettingsCollection.Save();

			//Verify the existing contents
			userSettingsCollection.Restore();
			Assert.AreEqual(2, userSettingsCollection.Count);
			Assert.AreEqual(true, (bool)userSettingsCollection["Visible"]);
			Assert.AreEqual(false, (bool)userSettingsCollection["Minimized"]);
		}

		/// <summary>
		/// Tests that we can register a new user in the system
		/// </summary>
		[
		Test,
		SpiraTestCase(944)
		]
		public void _11_RegisterNewUser()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;
			User user1;
			string name = "testUsersRegister";
			try
			{
				user1 = userManager.GetUserByLogin("testUsersRegister");

				if (user1 != null)
				{
					name = user1.UserName + "1";
				}
			}
			catch (ArtifactNotExistsException)
			{
				//This is expected, there should not be a user with this username
			}

			//First create a new user record
			int errorCode = 0;
			User user = userManager.CreateUser(name, "password1", salt, "testuser@company.com", "What is 1+1", "2", true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);

			//Verify that we can get the user record by id, email address and login
			int userId = user.UserId;
			user = userManager.GetUserByLogin(name, false);
			Assert.AreEqual(name, user.UserName);
			Assert.AreEqual("testuser@company.com", user.EmailAddress);
			Assert.AreEqual(null, user.Comment);
			user = userManager.GetUserById(userId, false);
			Assert.AreEqual(name, user.UserName);
			user = userManager.GetUserByEmailAddress("testuser@company.com");
			Assert.AreEqual(name, user.UserName);

			//Now add a profile to the user
			SettingsPropertyCollection properties = new SettingsPropertyCollection();
			properties.Add(new SettingsProperty("FirstName", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			properties.Add(new SettingsProperty("LastName", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			properties.Add(new SettingsProperty("Department", typeof(string), null, false, null, SettingsSerializeAs.ProviderSpecific, null, true, true));
			SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
			userManager.GetProfileData(properties, values, user.UserName);

			//Populate the values
			values["FirstName"].PropertyValue = "Roger";
			values["LastName"].PropertyValue = "Dodger";
			values["Department"].PropertyValue = "Testing";
			userManager.SetProfileData(values, name, true);

			//Verify the values
			values.Clear();
			userManager.GetProfileData(properties, values, user.UserName);
			Assert.AreEqual("Roger", values["FirstName"].PropertyValue);
			Assert.AreEqual("Dodger", values["LastName"].PropertyValue);
			Assert.AreEqual("Testing", values["Department"].PropertyValue);

			//Finally delete the user
			userManager.DeleteUser(name, true);
		}

		/// <summary>
		/// Tests that we can reset a user's password
		/// </summary>
		[
		Test,
		SpiraTestCase(945)
		]
		public void _12_ResetPassword()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			User user1;
			string name = "testResetuser";
			try
			{
				user1 = userManager.GetUserByLogin("testResetuser");

				if (user1 != null)
				{
					name = user1.UserName + "1";
				}
			}
			catch (ArtifactNotExistsException)
			{
				//This is expected, there should not be a user with this username
			}

			//First create a new user record
			int errorCode = 0;
			User user = userManager.CreateUser(name, "password1", salt, "testuser@company.com", "What is 1+1", "2", true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);

			//Now try and reset the password
			int result = userManager.ResetPassword(name, "password2", 3, 100, salt, 1, "2");
			Assert.AreEqual(0, result); //0 indicates success

			//Now get the password
			int passwordFormat;
			int status;
			string password = userManager.GetPassword(name, "2", true, 100, 3, out passwordFormat, out status);
			Assert.AreEqual(0, status);
			Assert.AreEqual("password2", password);
			Assert.AreEqual(1, passwordFormat);

			//Finally delete the user
			userManager.DeleteUser(name, true);
		}

		/// <summary>
		/// Tests that we can change a user's password 
		/// </summary>
		[
		Test,
		SpiraTestCase(946)
		]
		public void _13_ChangePassword()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			User user1;
			string name = "testChangePassworduser";

			try
			{
				user1 = userManager.GetUserByLogin("testChangePassworduser");

				if (user1 != null)
				{
					name = user1.UserName + "1";
				}
			}
			catch (ArtifactNotExistsException)
			{
				//This is expected, there should not be a user with this username
			}

			//First create a new user record
			int errorCode = 0;
			User user = userManager.CreateUser(name, "password1", salt, "testuser@company.com", "What is 1+1", "2", true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);

			//Now try and reset the password
			int result = userManager.SetPassword(name, "password3", salt, 1);
			Assert.AreEqual(0, result); //0 indicates success

			//Now get the password
			int passwordFormat;
			int status;
			string password = userManager.GetPassword(name, "2", true, 100, 3, out passwordFormat, out status);
			Assert.AreEqual(0, status);
			Assert.AreEqual("password3", password);
			Assert.AreEqual(1, passwordFormat);

			//Finally delete the user
			userManager.DeleteUser(name, true);
		}

		/// <summary>
		/// Tests that a user can manage their list of contacts (used by the IM system)
		/// </summary>
		[
		Test,
		SpiraTestCase(1269)
		]
		public void _14_ManageContacts()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Test that Fred Bloggs has two contacts
			List<User> contacts = userManager.UserContact_Retrieve(USER_ID_FRED_BLOGGS);
			Assert.AreEqual(2, contacts.Count);
			Assert.AreEqual("Joe P Smith", contacts[0].Profile.FullName);
			Assert.AreEqual("System Administrator", contacts[1].Profile.FullName);

			//Test that Joe Smith has no contacts
			contacts = userManager.UserContact_Retrieve(USER_ID_JOE_SMITH);
			Assert.AreEqual(0, contacts.Count);

			User user1 = new User();
			string name = "bingobob";

			try
			{
				user1 = userManager.GetUserByLogin("bingobob");

				if (user1 != null)
				{
					name = user1.UserName + "1";
				}
			}
			catch (ArtifactNotExistsException)
			{
				//This is expected, there should not be a user with this username
			}


			//Lets create a new user (and profile) to add contacts for.
			int errorCode;
			User user = userManager.CreateUser(name, "mypassword123", salt, "bingobob@spiratest.com", "What is my first dog named?", "crusher", true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			userId2 = user.UserId;

			//Verify that there were no errors
			Assert.AreEqual(0, errorCode);

			//Add a profile to this user
			user = userManager.GetUserById(userId2);
			user.Profile = new UserProfile();
			user.Profile.FirstName = "Bingo";
			user.Profile.LastName = "Bob";
			user.Profile.IsAdmin = false;
			user.Profile.IsEmailEnabled = true;
			user.Profile.Department = "Dept1";
			user.Profile.LastUpdateDate = DateTime.UtcNow;
			userManager.Update(user);

			//Verify that this user has no contacts
			contacts = userManager.UserContact_Retrieve(userId2);
			Assert.AreEqual(0, contacts.Count);

			//Now add some contacts
			userManager.UserContact_Add(userId2, USER_ID_FRED_BLOGGS);
			userManager.UserContact_Add(userId2, USER_ID_JOE_SMITH);

			//Verify they added successfully
			contacts = userManager.UserContact_Retrieve(userId2);
			Assert.AreEqual(2, contacts.Count);
			Assert.AreEqual("Fred Bloggs", contacts[0].Profile.FullName);
			Assert.AreEqual("Joe P Smith", contacts[1].Profile.FullName);

			//Verify that we can test to see if a user is a contact or not
			Assert.IsTrue(userManager.UserContact_IsContact(userId2, USER_ID_FRED_BLOGGS));
			Assert.IsTrue(userManager.UserContact_IsContact(userId2, USER_ID_JOE_SMITH));
			Assert.IsFalse(userManager.UserContact_IsContact(userId2, USER_ID_ADMINISTRATOR));

			//Now remove one of the contacts
			userManager.UserContact_Remove(userId2, USER_ID_JOE_SMITH);

			//Verify the removal
			contacts = userManager.UserContact_Retrieve(userId2);
			Assert.AreEqual(1, contacts.Count);
			Assert.AreEqual("Fred Bloggs", contacts[0].Profile.FullName);
		}

		/// <summary>
		/// Tests that the system can track the last artifacts you viewed
		/// </summary>
		[
		Test,
		SpiraTestCase(2742)
		]
		public void _15_RecentArtifacts()
		{
			//First delete all the existing items (if there are any)
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER_RECENT_ARTIFACT");

			//Now verify that there are no entries (all projects)
			List<UserRecentArtifact> recentArtifacts = userManager.RetrieveRecentArtifactsForUser(USER_ID_FRED_BLOGGS, null, 100);
			Assert.AreEqual(0, recentArtifacts.Count);

			//Register a couple of artifacts
			userManager.AddUpdateRecentArtifact(USER_ID_FRED_BLOGGS, 1, (int)Artifact.ArtifactTypeEnum.Requirement, 1);
			userManager.AddUpdateRecentArtifact(USER_ID_FRED_BLOGGS, 1, (int)Artifact.ArtifactTypeEnum.TestCase, 5);
			userManager.AddUpdateRecentArtifact(USER_ID_FRED_BLOGGS, 1, (int)Artifact.ArtifactTypeEnum.Task, 8);
			userManager.AddUpdateRecentArtifact(USER_ID_FRED_BLOGGS, 1, (int)Artifact.ArtifactTypeEnum.Incident, 10);

			//Verify the list
			recentArtifacts = userManager.RetrieveRecentArtifactsForUser(USER_ID_FRED_BLOGGS, null, 100);
			Assert.AreEqual(4, recentArtifacts.Count);
			UserRecentArtifact recentArtifact = recentArtifacts.First();
			Assert.AreEqual(10, recentArtifact.ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.Incident, recentArtifact.ArtifactTypeId);
			Assert.AreEqual(1, recentArtifact.ProjectId);

			//Now update an existing artifact in the list (makes it more recent)
			userManager.AddUpdateRecentArtifact(USER_ID_FRED_BLOGGS, 1, (int)Artifact.ArtifactTypeEnum.TestCase, 5);

			//Verify the list
			recentArtifacts = userManager.RetrieveRecentArtifactsForUser(USER_ID_FRED_BLOGGS, null, 100);
			Assert.AreEqual(4, recentArtifacts.Count);
			recentArtifact = recentArtifacts.First();
			Assert.AreEqual(5, recentArtifact.ArtifactId);
			Assert.AreEqual((int)Artifact.ArtifactTypeEnum.TestCase, recentArtifact.ArtifactTypeId);
			Assert.AreEqual(1, recentArtifact.ProjectId);

			//Retrieve by projects
			recentArtifacts = userManager.RetrieveRecentArtifactsForUser(USER_ID_FRED_BLOGGS, 1, 100);
			Assert.AreEqual(4, recentArtifacts.Count);
			recentArtifacts = userManager.RetrieveRecentArtifactsForUser(USER_ID_FRED_BLOGGS, 2, 100);
			Assert.AreEqual(0, recentArtifacts.Count);

			//Clean up
			InternalRoutines.ExecuteNonQuery("DELETE FROM TST_USER_RECENT_ARTIFACT");
		}

		/// <summary>
		/// Generates the SALT
		/// </summary>
		/// <returns></returns>
		private string GenerateSalt()
		{
			byte[] data = new byte[0x10];
			new RNGCryptoServiceProvider().GetBytes(data);
			return Convert.ToBase64String(data);
		}
	}
}
