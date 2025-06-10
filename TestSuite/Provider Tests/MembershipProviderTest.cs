using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Reflection;
using System.Web.Profile;
using System.Web.Security;

using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

using NUnit.Framework;

namespace Inflectra.SpiraTest.TestSuite.Provider_Tests
{
	/// <summary>Tests the Spira ASP.NET Membership Provider</summary>
	[TestFixture]
	[SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)]
	public class MembershipProviderTest
	{
		private static SpiraMembershipProvider provider;

		private static int legacyUserId;
		private static int legacyUserId2;
		private static int legacyUserId3;
		private static int newUser1;
		private static int newUser2;
		private static string newUserName1;
		private static string newUserName2;
		private static string rssToken;

		[TestFixtureSetUp]
		public void Init()
		{
			//We need to programatically add the Membership provider for the unit tests because we don't
			//use the Web.Config file when running the unit tests

			//Get a handle to our custom Spira membership provider, some of the functions require this
			provider = new SpiraMembershipProvider();
			NameValueCollection configuration = new NameValueCollection();
			configuration.Add("enablePasswordRetrieval", "false");
			configuration.Add("enablePasswordReset", "true");
			configuration.Add("requiresQuestionAndAnswer", "true");
			configuration.Add("requiresUniqueEmail", "false");
			configuration.Add("passwordFormat", "Hashed");
			provider.Initialize("SpiraMembershipProvider", configuration);
			provider.AddTo(Membership.Providers, true);

			//Verify the default provider
			Assert.AreEqual(provider.GetType(), Membership.Provider.GetType());

			//Create two test legacy users that uses the older v3.2 password system (MD5 with no SALT)
			//this needs to be done using the UserManager since the membership provider does not allow the creation of legacy users with MD5 unsalted passwords
			int errorCode;

			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//First Legacy User
			User user = new UserManager().CreateUser("legacyuser", FormsAuthentication.HashPasswordForStoringInConfigFile("oldpassword", "md5"), null, "legacyuser@spiratest.com", "What is 1+1?", FormsAuthentication.HashPasswordForStoringInConfigFile("2", "md5"), true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);
			legacyUserId = user.UserId;
			user = provider.GetProviderUser(legacyUserId);
			user.StartTracking();
			user.IsLegacyFormat = true;
			provider.UpdateProviderUser(user);

			//Second Legacy User
			user = new UserManager().CreateUser("legacyuser2", FormsAuthentication.HashPasswordForStoringInConfigFile("oldpassword", "md5"), null, "legacyuser2@spiratest.com", "What is 1+1?", FormsAuthentication.HashPasswordForStoringInConfigFile("2", "md5"), true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);
			legacyUserId2 = user.UserId;
			user = provider.GetProviderUser(legacyUserId2);
			user.StartTracking();
			user.IsLegacyFormat = true;
			provider.UpdateProviderUser(user);

			//Third Legacy User
			user = new UserManager().CreateUser("legacyuser3", FormsAuthentication.HashPasswordForStoringInConfigFile("oldpassword", "md5"), null, "legacyuser3@spiratest.com", "What is 1+1?", FormsAuthentication.HashPasswordForStoringInConfigFile("2", "md5"), true, false, 1, null, null, null, out errorCode, null, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(0, errorCode);
			legacyUserId3 = user.UserId;
			user = provider.GetProviderUser(legacyUserId3);
			user.StartTracking();
			user.IsLegacyFormat = true;
			provider.UpdateProviderUser(user);
		}

		[TestFixtureTearDown]
		public void Dispose()
		{
			//Delete the legacy users
			Membership.DeleteUser("legacyuser", true);
			Membership.DeleteUser("legacyuser2", true);
			Membership.DeleteUser("legacyuser3", true);

			//Delete the new users
			Membership.DeleteUser(newUserName1, true);
			Membership.DeleteUser(newUserName2, true);
		}

		[Test]
		[SpiraTestCase(947)]
		public void _01_RegisterUser()
		{
			Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
			string adminSectionName = "View / Edit Users";
			var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

			int adminSectionId = adminSection.ADMIN_SECTION_ID;

			//Register a new user (using the new format)
			MembershipCreateStatus status;
			MembershipUser newUser = Membership.CreateUser("newuser", "newpassword123", "newuser@spiratest.com", "What is 1+1?", "2", false, out status);
			Assert.AreEqual(MembershipCreateStatus.Success, status);
			newUser1 = (int)newUser.ProviderUserKey;
			newUserName1 = newUser.UserName;

			//Try creating a user with a password length that is too short (6 characters is the system default)
			Membership.CreateUser("newuser2", "abc", "newuser@spiratest.com", "What is 1+1?", "2", false, out status);
			Assert.AreEqual(MembershipCreateStatus.InvalidPassword, status);

			//Try creating a user with an invalid password question
			Membership.CreateUser("newuser2", "newpassword123", "newuser@spiratest.com", "", "2", false, out status);
			Assert.AreEqual(MembershipCreateStatus.InvalidQuestion, status);

			//Try creating a user with an invalid password answer
			Membership.CreateUser("newuser2", "newpassword123", "newuser@spiratest.com", "What is 1+1?", "", false, out status);
			Assert.AreEqual(MembershipCreateStatus.InvalidAnswer, status);

			//Try creating a user with a username that is already in use
			Membership.CreateUser("newuser", "newpassword123", "newuser@spiratest.com", "What is 1+1?", "2", false, out status);
			Assert.AreEqual(MembershipCreateStatus.DuplicateUserName, status);

			//Now verify that our new user is not yet approved
			MembershipUser membershipUser = Membership.GetUser(newUserName1);
			Assert.IsFalse(membershipUser.IsApproved);

			//Approve this user (provider-specific method)
			provider.ApproveUser(newUser1);

			//Now verify that our new user is approved
			membershipUser = Membership.GetUser(newUserName1);
			Assert.IsTrue(membershipUser.IsApproved);

			//Finally test that we can create a user using the provider native method, useful when
			//we need to specify an RSS token or LDAP DN
			rssToken = new Guid().ToString();
			provider.CreateUser("newuser2", "newpassword123", "newuser@spiratest.com", "What is 1+1?", "2", true, null, rssToken, out status, adminSectionId, "Inserted User", 1);
			Assert.AreEqual(MembershipCreateStatus.Success, status);
			User user = provider.GetProviderUser("newuser2");
			Assert.AreEqual(rssToken, user.RssToken);
			newUserName2 = user.UserName;
			newUser2 = user.UserId;
		}

		/// <summary>Tests that you can authenticate with the system</summary>
		[Test]
		[SpiraTestCase(85)]
		public void _02_Authentication()
		{
			//Make sure that only valid users can log-in
			//We are only able to test SpiraTest logins in this unit test (not LDAP)

			//First test that we can login using the new authentication scheme and the legacy (v3.2) scheme
			//New Scheme
			Assert.IsTrue(Membership.ValidateUser("newuser", "newpassword123"));
			Assert.IsTrue(Membership.ValidateUser("NewUser", "newpassword123"));
			Assert.IsTrue(Membership.ValidateUser("NEWUSER", "newpassword123"));
			//Legacy Scheme
			Assert.IsTrue(Membership.ValidateUser("legacyuser", "oldpassword"));
			Assert.IsTrue(Membership.ValidateUser("LegacyUser", "oldpassword"));
			Assert.IsTrue(Membership.ValidateUser("LEGACYUSER", "oldpassword"));

			//Test Unsuccessful logins
			Assert.IsFalse(Membership.ValidateUser("newuser", "wrongpassword"));
			Assert.IsFalse(Membership.ValidateUser("newuser", ""));
			Assert.IsFalse(Membership.ValidateUser("legacyuser", "wrongpassword"));
			Assert.IsFalse(Membership.ValidateUser("legacyuser", ""));
			Assert.IsFalse(Membership.ValidateUser("wronguser", "password"));
			Assert.IsFalse(Membership.ValidateUser("wronguser", ""));

			//Test that we cannot login in as an inactive user
			Assert.IsFalse(Membership.ValidateUser("rickypond", "PleaseChange"));

			//Test that a certain unsuccessful attempts causes the system to lockout
			int numberOfAttemptsNeeded = Membership.MaxInvalidPasswordAttempts;

			//First verify that the user is not locked out
			MembershipUser membershipUser = Membership.GetUser(newUserName1);
			Assert.IsFalse(membershipUser.IsLockedOut);

			//Deliberately not login correctly X times
			for (int i = 0; i < numberOfAttemptsNeeded; i++)
			{
				Membership.ValidateUser(newUserName1, "wrong");
			}
			//Verify that the user is now locked out
			User user = provider.GetProviderUser(newUserName1);
			Assert.IsTrue(user.FailedPasswordAttemptCount > 1);
			membershipUser = Membership.GetUser(newUserName1);
			Assert.IsTrue(membershipUser.IsLockedOut);

			//Verify that we can unlock the account (provider-specific method)
			provider.UnlockUser(newUserName1);
			membershipUser = Membership.GetUser(newUserName1);
			Assert.IsFalse(membershipUser.IsLockedOut);

			//Deliberately not login correctly X times
			//for (int i = 0; i < numberOfAttemptsNeeded; i++)
			//{
			//	Membership.ValidateUser(newUserName1, "wrong");
			//}
			////Verify that the user is now locked out
			//user = provider.GetProviderUser(newUserName1);
			//Assert.IsTrue(user.FailedPasswordAttemptCount > 1);
			//membershipUser = Membership.GetUser(newUserName1);
			//Assert.IsTrue(membershipUser.IsLockedOut);

			//Now verify that after the set number of minutes, we can log back in.


			//Verify that we can login with a valid user id and RSS token
			bool isValid = provider.ValidateUserByRssToken(newUser2, rssToken);
			Assert.IsTrue(isValid);

			//Verify that if we login more than X times incorrectly it will lock the account
			for (int i = 0; i < numberOfAttemptsNeeded; i++)
			{
				provider.ValidateUserByRssToken(newUser2, "XXXX");
			}

			//Verify that the user is now locked out
			user = provider.GetProviderUser(newUser2);
			Assert.IsTrue(user.FailedPasswordAttemptCount > 1);
			membershipUser = Membership.GetUser(newUser2);
			Assert.IsTrue(membershipUser.IsLockedOut);

			//Verify that we can unlock the account (provider-specific method)
			provider.UnlockUser(membershipUser.UserName);
			membershipUser = Membership.GetUser(newUser2);
			Assert.IsFalse(membershipUser.IsLockedOut);

			//Verify that we can login with a valid username and RSS token
			isValid = provider.ValidateUserByRssToken("newuser2", rssToken);
			Assert.IsTrue(isValid);

			//Verify that if we login more than X times incorrectly it will lock the account
			for (int i = 0; i < numberOfAttemptsNeeded; i++)
			{
				provider.ValidateUserByRssToken("newuser2", "XXXX");
			}

			//Verify that the user is now locked out
			user = provider.GetProviderUser("newuser2");
			Assert.IsTrue(user.FailedPasswordAttemptCount > 1);
			membershipUser = Membership.GetUser("newuser2");
			Assert.IsTrue(membershipUser.IsLockedOut);

			//Verify that we can unlock the account (provider-specific method)
			provider.UnlockUser("newuser2");
			membershipUser = Membership.GetUser("newuser2");
			Assert.IsFalse(membershipUser.IsLockedOut);
		}

		/// <summary>
		/// Tests that a user can change or reset their password or Q&A and an administrator can change the password and Q&A
		/// without knowing the current password
		/// </summary>
		[Test]
		[SpiraTestCase(948)]
		public void _03_ChangeAndResetPassword()
		{
			//First verify that our new user can change his/her password
			bool success = provider.ChangePassword(newUserName1, "newpassword123", "newpassword456");
			Assert.IsTrue(success);
			//Verify that we can login with this new password
			success = Membership.ValidateUser(newUserName1, "newpassword456");
			Assert.IsTrue(success);

			//Now verify that we can change a legacy user to a new password in the new format
			success = provider.ChangePassword("legacyuser", "oldpassword", "newpassword789");
			Assert.IsTrue(success);
			//Verify that we can login with this new password
			success = Membership.ValidateUser("legacyuser", "newpassword789");
			Assert.IsTrue(success);
			//Verify that we have the new format password (with a SALT)
			DataModel.User user = provider.GetProviderUser("legacyuser");
			Assert.IsFalse(user.IsLegacyFormat);
			Assert.IsFalse(String.IsNullOrEmpty(user.PasswordSalt));

			//Now we need to verify that an administrator can change a password
			//without knowing the current one
			success = provider.ChangePasswordAsAdmin(newUserName1, "newpasswordABC");
			Assert.IsTrue(success);
			//Verify that we can login with this new password
			success = Membership.ValidateUser(newUserName1, "newpasswordABC");
			Assert.IsTrue(success);

			//Now verify that we can change a legacy user to a new password in the new format
			success = provider.ChangePasswordAsAdmin("legacyuser2", "newpasswordABC");
			Assert.IsTrue(success);
			//Verify that we can login with this new password
			success = Membership.ValidateUser("legacyuser2", "newpasswordABC");
			Assert.IsTrue(success);
			//Verify that we have the new format password (with a SALT)
			user = provider.GetProviderUser("legacyuser");
			Assert.IsFalse(user.IsLegacyFormat);
			Assert.IsFalse(String.IsNullOrEmpty(user.PasswordSalt));

			//Verify that we cannot change a password (not as an admin if we don't know the correct one)
			success = provider.ChangePassword(newUserName1, "wrongpassword", "newpasswordXYZ");
			Assert.IsFalse(success);
			success = Membership.ValidateUser(newUserName1, "newpasswordABC");
			Assert.IsTrue(success);

			//Verify that we can change the password question and answer
			success = provider.ChangePasswordQuestionAndAnswer(newUserName1, "newpasswordABC", "What is 2+2?", "4");
			Assert.IsTrue(success);
			MembershipUser membershipUser = Membership.GetUser(newUserName1);
			Assert.AreEqual("What is 2+2?", membershipUser.PasswordQuestion);

			//Verify that we cannot change the password question and answer unless we know the correct current password
			success = provider.ChangePasswordQuestionAndAnswer(newUserName1, "wrongpassword", "What is 3+3?", "6");
			Assert.IsFalse(success);
			membershipUser = Membership.GetUser(newUserName1);
			Assert.AreEqual("What is 2+2?", membershipUser.PasswordQuestion);

			//Verify that an administrator can change the password question and answer without knowing the correct current password
			success = provider.ChangePasswordQuestionAndAnswerAsAdmin(newUserName1, "What is 4+4?", "8");
			Assert.IsTrue(success);
			membershipUser = Membership.GetUser(newUserName1);
			Assert.AreEqual("What is 4+4?", membershipUser.PasswordQuestion);

			//Verify that a user can reset the password as long as they know the question/answer
			string temporaryPassword = provider.ResetPassword(newUserName1, "8");

			//Verify that we can login with this new password
			success = Membership.ValidateUser(newUserName1, temporaryPassword);
			Assert.IsTrue(success);

			//Verify that a user can reset the password of a legacy Spira v3.2 account and the new password is upgraded to the new format
			temporaryPassword = provider.ResetPassword("legacyuser3", "2");
			success = Membership.ValidateUser("legacyuser3", temporaryPassword);
			Assert.IsTrue(success);
			//Verify that we have the new format password (with a SALT)
			user = provider.GetProviderUser("legacyuser3");
			Assert.IsFalse(user.IsLegacyFormat);
			Assert.IsFalse(String.IsNullOrEmpty(user.PasswordSalt));
		}

		/// <summary>
		/// Verifies that we can update a user
		/// </summary>
		[Test]
		[SpiraTestCase(949)]
		public void _04_UpdateUser()
		{
			//Verify the current data
			MembershipUser membershipUser = Membership.GetUser(newUserName1);
			Assert.AreEqual(newUserName1, membershipUser.UserName);
			Assert.AreEqual("newuser@spiratest.com", membershipUser.Email);
			Assert.IsTrue(String.IsNullOrEmpty(membershipUser.Comment));

			//Make some changes
			membershipUser.Email = "newuser2@spiratest.com";
			membershipUser.Comment = "Updated Matey";
			Membership.UpdateUser(membershipUser);

			//Verify the changes
			membershipUser = Membership.GetUser(newUserName1);
			Assert.AreEqual(newUserName1, membershipUser.UserName);
			Assert.AreEqual("newuser2@spiratest.com", membershipUser.Email);
			Assert.AreEqual("Updated Matey", membershipUser.Comment);
		}

		/// <summary>
		/// Verifies that we can retrieve a list of users
		/// </summary>
		[Test]
		[SpiraTestCase(950)]
		public void _05_RetrieveUsers()
		{
			//Get the list of current users
			int count;
			MembershipUserCollection membershipUsers = Membership.GetAllUsers(0, 99999, out count);
			//Verify the count
			Assert.AreEqual(18, count);
			Assert.AreEqual(18, membershipUsers.Count);

			//Get the list of users that have a specific email address partial match
			membershipUsers = Membership.FindUsersByEmail("spiratest.com", 0, 99999, out count);
			//Verify the count
			Assert.AreEqual(5, count);
			Assert.AreEqual(5, membershipUsers.Count);

			//Get the list of users that have a specific username partial match
			membershipUsers = Membership.FindUsersByName("newuser", 0, 99999, out count);
			//Verify the count
			Assert.AreEqual(2, count);
			Assert.AreEqual(2, membershipUsers.Count);
		}
	}

	/// <summary>
	/// Helper class that is used to dynamically add the membership provider to what is normally a read-only collection
	/// </summary>
	/// <remarks>
	/// http://stackoverflow.com/questions/1432508/programmatically-adding-a-membership-provider
	/// </remarks>
	public static class ProviderUtil
	{
		static private FieldInfo providerCollectionReadOnlyField;

		static ProviderUtil()
		{
			Type t = typeof(ProviderCollection);
			providerCollectionReadOnlyField = t.GetField("_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		static public void AddTo(this ProviderBase provider, ProviderCollection pc, bool makeDefault = false)
		{
			bool prevValue = (bool)providerCollectionReadOnlyField.GetValue(pc);
			if (prevValue)
				providerCollectionReadOnlyField.SetValue(pc, false);

			pc.Add(provider);

			if (prevValue)
				providerCollectionReadOnlyField.SetValue(pc, true);

			//Make the default
			if (makeDefault)
			{
				if (provider is MembershipProvider)
				{
					Type t = typeof(Membership);
					var field = t.GetField("s_Provider", BindingFlags.Static | BindingFlags.NonPublic);
					field.SetValue(null, provider);
				}
				if (provider is ProfileProvider)
				{
					Type t = typeof(ProfileManager);
					var field = t.GetField("s_Provider", BindingFlags.Static | BindingFlags.NonPublic);
					field.SetValue(null, provider);
				}
			}
		}
	}
}
