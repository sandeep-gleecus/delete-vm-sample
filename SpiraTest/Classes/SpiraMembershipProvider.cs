using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Classes
{
	/// <summary>
	/// Provides the user authentication and authorization provider for SpiraTest
	/// </summary>
	public class SpiraMembershipProvider : MembershipProvider
	{
		//Constants
		public const string PROVIDER_NAME = "SpiraMembershipProvider";
		public const string APPLICATION_NAME = "SpiraTest";
		/// <summary>The minimum legnth for an auto-generated password.</summary>
		public const int GENERATE_PASS_LEN = 14;

		// Fields
		private bool _EnablePasswordReset;
		private bool _EnablePasswordRetrieval;
		private MembershipPasswordCompatibilityMode _LegacyPasswordCompatibilityMode;
		private MembershipPasswordFormat _PasswordFormat;
		private string _PasswordStrengthRegularExpression;
		private bool _RequiresQuestionAndAnswer;
		private bool _RequiresUniqueEmail;
		private const int PASSWORD_SIZE = 14;
		private string s_HashAlgorithm;
		private const int SALT_SIZE = 0x10;

		//Http Request constants
		private const string CURRENT_USER = PROVIDER_NAME + "_CurrentUser";

		// Static Methods

		/// <summary>
		/// Gets the current instance of the SpiraMembership provider (or null if it's not the default provider for the application)
		/// </summary>
		/// <returns>The current instance of the provider</returns>
		/// <remarks>This is used when you need to access the non-standard ...AsAdmin() functions</remarks>
		public static SpiraMembershipProvider GetCurrentInstance()
		{
			if (Membership.Provider is SpiraMembershipProvider)
			{
				return (SpiraMembershipProvider)Membership.Provider;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Sets the SpiraTeam authentication cookie
		/// </summary>
		/// <param name="userName">The username to set</param>
		/// <param name="expiry">The expiry of the cookie</param>
		/// <param name="createPersistentCookie">Should we create a persistent cookie</param>
		public static void SetAuthCookie(string userName, bool createPersistentCookie, int expiry)
		{
			//Get the authentication information from the web.config file
			//We don't use the timeout value (which is set instead in settings, but we do use some of the other settings)
			System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
			AuthenticationSection authentication = (AuthenticationSection)configuration.GetSection("system.web/authentication");
			if (authentication.Forms != null)
			{
				string cookieName = ".ASPXAUTH";
				if (!string.IsNullOrEmpty(authentication.Forms.Name))
				{
					cookieName = authentication.Forms.Name;
				}
				string cookiePath = "/";
				if (!string.IsNullOrEmpty(authentication.Forms.Path))
				{
					cookiePath = authentication.Forms.Path;
				}
				string cookieDomain = authentication.Forms.Domain;
				FormsProtectionEnum protection = authentication.Forms.Protection;
				HttpContext context = HttpContext.Current;
				HttpCookie cookie = GetAuthCookie(cookieName, cookieDomain, userName, createPersistentCookie, cookiePath, true, protection, expiry);
				context.Response.Cookies.Add(cookie);
			}
		}

		/// <summary>
		/// Gets the SpiraTeam authentication cookie
		/// </summary>
		/// <param name="userName">The username</param>
		/// <param name="createPersistentCookie">Are we using a persistent cookie</param>
		/// <param name="cookiePath">The cookie path</param>
		/// <param name="hexEncodedTicket">Should the ticket be hex encoded</param>
		/// <param name="expiry">The expiration of the cookie (in minutes)</param>
		/// <param name="cookieName">The name of the cookie</param>
		/// <param name="cookieDomain">The domain of the cookie</param>
		/// <param name="protection">The kind of protection being used</param>
		/// <returns>The HTTP cookie</returns>
		private static HttpCookie GetAuthCookie(string cookieName, string cookieDomain, string userName, bool createPersistentCookie, string cookiePath, bool hexEncodedTicket, FormsProtectionEnum protection, int expiry)
		{
			if (userName == null)
			{
				userName = string.Empty;
			}
			if (string.IsNullOrEmpty(cookiePath))
			{
				cookiePath = "/";
			}
			DateTime utcNow = DateTime.UtcNow;
			DateTime expirationUtc = utcNow.AddMinutes((double)expiry);
			FormsAuthenticationTicket ticket = CreateFormsTicketFromUtc(2, userName, utcNow, expirationUtc, createPersistentCookie, string.Empty, cookiePath);
			string str = FormsAuthentication.Encrypt(ticket);
			if (string.IsNullOrEmpty(str))
			{
				throw new HttpException(SR.GetString("Unable_to_encrypt_cookie_ticket"));
			}
			HttpCookie cookie = new HttpCookie(cookieName, str);
			cookie.HttpOnly = true;
			cookie.Path = cookiePath;
			cookie.Secure = false;
			if (!string.IsNullOrEmpty(cookieDomain))
			{
				cookie.Domain = cookieDomain;
			}
			//We only set the expiration date for the persistent 'Remember Me' cookies
			//The expiration for session (non-remember me) cookies is handled solely from the Expiration property
			//of the ticket (not the cookie containing the ticket)
			if (ticket.IsPersistent)
			{
				cookie.Expires = ticket.Expiration;
			}

			return cookie;
		}

		/// <summary>
		/// Creates the Forms ticket from UTC
		/// </summary>
		private static FormsAuthenticationTicket CreateFormsTicketFromUtc(int version, string name, DateTime issueDateUtc, DateTime expirationUtc, bool isPersistent, string userData, string cookiePath)
		{
			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(version, name, issueDateUtc.ToLocalTime(), expirationUtc.ToLocalTime(), isPersistent, userData, cookiePath);
			return ticket;
		}

		// Methods

		/// <summary>
		/// Does the provided password have sufficient special characters
		/// </summary>
		/// <param name="newPassword">The new password</param>
		/// <returns>True if password is ok</returns>
		public bool DoesPasswordHaveEnoughSpecialChars(string newPassword)
		{
			int numberNonAlphanumericCharacters = 0;
			for (int i = 0; i < newPassword.Length; i++)
			{
				if (!char.IsLetterOrDigit(newPassword, i))
				{
					numberNonAlphanumericCharacters++;
				}
			}
			return (numberNonAlphanumericCharacters >= MinRequiredNonAlphanumericCharacters);
		}

		/// <summary>
		/// Changes a user's password
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="oldPassword">The user's current password</param>
		/// <param name="newPassword">The user's new password</param>
		/// <returns>True if successful</returns>
		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			int passwordFormat;
			bool success;
			bool usesLegacyFormat;
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			SecUtility.CheckParameter(ref oldPassword, true, true, false, 0x80, "oldPassword");
			SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");
			string salt = null;
			if (!CheckPassword(username, oldPassword, false, false, out salt, out passwordFormat, out usesLegacyFormat))
			{
				return false;
			}
			if (newPassword.Length < MinRequiredPasswordLength)
			{
				throw new ArgumentException(SR.GetString("Password_too_short", new object[] { "newPassword", MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture) }));
			}
			int numberNonAlphanumericCharacters = 0;
			for (int i = 0; i < newPassword.Length; i++)
			{
				if (!char.IsLetterOrDigit(newPassword, i))
				{
					numberNonAlphanumericCharacters++;
				}
			}
			if (numberNonAlphanumericCharacters < MinRequiredNonAlphanumericCharacters)
			{
				throw new ArgumentException(SR.GetString("Password_need_more_non_alpha_numeric_chars", new object[] { "newPassword", MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture) }));
			}
			if ((PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
			{
				throw new ArgumentException(SR.GetString("Password_does_not_match_regular_expression", new object[] { "newPassword" }));
			}

			//If we have expiring passwords, don't allow the password to be changed to the same one
			//since this defeat's the whole point
			if (ConfigurationSettings.Default.Security_PasswordChangeInterval > 0 && oldPassword == newPassword)
			{
				throw new MembershipPasswordException(Resources.Messages.SpiraMembershipProvider_PasswordCannotBeSame);
			}

			//See if they are allowed to have their username in the password
			if (ConfigurationSettings.Default.Security_PasswordNoNames && newPassword.ToLowerInvariant().Contains(username.ToLowerInvariant()))
			{
				throw new MembershipPasswordException(Resources.Messages.SpiraMembershipProvider_PasswordCannotContainUsername);
			}

			//If this used to use legacy format, need to generate a new SALT
			if (usesLegacyFormat)
			{
				salt = GenerateSalt();
			}
			string encryptedNewPassword = EncodePassword(newPassword, passwordFormat, salt);
			if (encryptedNewPassword.Length > 0x80)
			{
				throw new ArgumentException(SR.GetString("Membership_password_too_int"), "newPassword");
			}
			ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
			OnValidatingPassword(e);
			if (e.Cancel)
			{
				if (e.FailureInformation != null)
				{
					throw e.FailureInformation;
				}
				throw new ArgumentException(SR.GetString("Membership_Custom_Password_Validation_Failure"), "newPassword");
			}
			try
			{
				//Update the user's password
				UserManager userManager = new UserManager();
				int status = userManager.SetPassword(username, encryptedNewPassword, salt, passwordFormat);

				if (status != 0)
				{
					string exceptionText = GetExceptionText(status);
					if (IsStatusDueToBadPassword(status))
					{
						throw new MembershipPasswordException(exceptionText);
					}
					throw new ProviderException(exceptionText);
				}
				success = true;
			}
			catch
			{
				throw;
			}
			return success;
		}

		/// <summary>
		/// Changes a user's password when we're an administrator and don't know their current password
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="newPassword">The user's new password</param>
		/// <returns>True if successful</returns>
		/// <remarks>This is not called through the main ASP.NET MembershipUser, but instead is called directly</remarks>
		public bool ChangePasswordAsAdmin(string username, string newPassword)
		{
			const string METHOD_NAME = ".ChangePasswordAsAdmin";

			int passwordFormat;
			bool success;
			string passwdFromDB;
			int status;
			int failedPasswordAttemptCount;
			int failedPasswordAnswerAttemptCount;
			bool isApproved;
			bool isActive;
			bool legacyFormat;
			DateTime lastLoginDate;
			DateTime lastActivityDate;
			string ldapDn;
			bool isOauth;

			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			SecUtility.CheckParameter(ref newPassword, true, true, false, 0x80, "newPassword");
			string salt = null;

			//Get their current password format and salt
			UserManager userManager = new UserManager();
			userManager.GetPasswordWithFormat(username, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate, out legacyFormat, out ldapDn, out isActive, out isOauth);

			if (newPassword.Length < MinRequiredPasswordLength)
			{
				throw new ArgumentException(SR.GetString("Password_too_short", new object[] { "newPassword", MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture) }));
			}
			int num3 = 0;
			for (int i = 0; i < newPassword.Length; i++)
			{
				if (!char.IsLetterOrDigit(newPassword, i))
				{
					num3++;
				}
			}
			if (num3 < MinRequiredNonAlphanumericCharacters)
			{
				throw new ArgumentException(SR.GetString("Password_need_more_non_alpha_numeric_chars", new object[] { "newPassword", MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture) }));
			}
			if (!string.IsNullOrWhiteSpace(PasswordStrengthRegularExpression) && !Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
			{
				throw new ArgumentException(SR.GetString("Password_does_not_match_regular_expression", new object[] { "newPassword" }));
			}
			//If this used to use legacy format, need to generate a new SALT
			if (legacyFormat || string.IsNullOrWhiteSpace(salt))
			{
				salt = GenerateSalt();
			}
			string encryptedNewPassword = EncodePassword(newPassword, passwordFormat, salt);
			if (encryptedNewPassword.Length > 0x80)
			{
				throw new ArgumentException(SR.GetString("Membership_password_too_int"), "newPassword");
			}
			ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, newPassword, false);
			OnValidatingPassword(e);
			if (e.Cancel)
			{
				if (e.FailureInformation != null)
				{
					throw e.FailureInformation;
				}
				throw new ArgumentException(SR.GetString("Membership_Custom_Password_Validation_Failure"), "newPassword");
			}
			try
			{
				//Update the user's password
				int setPasswordStatus = userManager.SetPassword(username, encryptedNewPassword, salt, passwordFormat);

				if (setPasswordStatus != 0)
				{
					string exceptionText = GetExceptionText(setPasswordStatus);
					if (IsStatusDueToBadPassword(setPasswordStatus))
					{
						throw new MembershipPasswordException(exceptionText);
					}
					throw new ProviderException(exceptionText);
				}
				success = true;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(PROVIDER_NAME + METHOD_NAME, exception);
				throw;
			}
			return success;
		}

		/// <summary>Unlinks the user from the Oauth provider.</summary>
		/// <param name="userId">The ID of the user to unlink.</param>
		/// <param name="newUsername">The new username of the user. (If null, does not change.)</param>
		/// <param name="newPass">The user's new password.</param>
		/// <param name="newQuestion">THe user's new Security Question</param>
		/// <param name="newAnswer">The user's new Security Answer.</param>
		/// <param name="enforcePass">Not used at this time.</param>
		/// <returns></returns>
		public bool UnlinkAccountFromOAuth(int userId, string newUsername, string newPass, string newQuestion, string newAnswer, bool enforcePass = true)
		{
			bool retValue = true;

			//HACK: Most of this code should be in a single location. However, because the functions
			//  are spread out so much (between UserManager and MembershipProvider) it is difficult
			//  to have it contained.
			//It has been attempted to move most of the handling code into the OAuth Manager. This helps with UnitTests,
			//  as well as having the code with other like-code (i.e. LINKING a provider..) The OAuthManager will call 
			//  the functions that are needed in the membership provider by delegates - so that any changes needed to be
			//  made will only have to be made in one location. (If any used Function signature changes occur, it will then
			//  throw a compile-time error to make sure that everything is iupdated.

			//First get the full user's account.
			UserManager uMgr = new UserManager();
			OAuthManager oMgr = new OAuthManager();

			//Make sure the username isn't already in use!
			try
			{
				User chkUser = uMgr.GetUserByLogin(newUsername);

				//We pass if the user is NULL, or the user is not null, but the user is the same ID.
				retValue = (chkUser == null ||
					(chkUser != null &&
					chkUser.UserId == userId &&
					chkUser.UserId != 1)
				);
			}
			catch { }

			//Only continue if we have a user and the username isn't already used.
			if (retValue)
			{
				string errorMsg;

				if (userId != 1)
				{
					var ret = oMgr.User_UnlinkProvider(
						userId,
						newUsername,
						newPass,
						newQuestion,
						newAnswer,
						out errorMsg,
						ChangePasswordAsAdmin,
						ChangePasswordQuestionAndAnswerAsAdmin);
				}
				else
					errorMsg = "Admin account should not be linked to OAuth or LDAP!";

				//See if we threw an error.
				if (!string.IsNullOrWhiteSpace(errorMsg))
					throw new Exception("Could not unlink user: " + errorMsg);

				//We got this far, pass!
				retValue = true;
			}

			return retValue;
		}

		/// <summary>
		/// Changes the password question and answer for a user when you are an administrator and don't know the current user's password
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="newPasswordQuestion">The new password question</param>
		/// <param name="newPasswordAnswer">The new password answer</param>
		/// <returns>True if successful</returns>
		/// <remarks>This is not called through the main ASP.NET MembershipUser, but instead is called directly</remarks>
		public bool ChangePasswordQuestionAndAnswerAsAdmin(string username, string newPasswordQuestion, string newPasswordAnswer)
		{
			string salt;
			int passwordFormat;
			string encodedPasswordAnswer;
			string passwdFromDB;
			int status;
			bool isApproved;
			bool isActive;
			bool legacyFormat;
			DateTime lastLoginDate;
			DateTime lastActivityDate;
			bool success;
			int failedPasswordAttemptCount;
			int failedPasswordAnswerAttemptCount;
			string ldapDn;
			bool isOauth;

			//Verify username format
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");

			//Get the current password format and salt
			UserManager userManager = new UserManager();
			userManager.GetPasswordWithFormat(username, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate, out legacyFormat, out ldapDn, out isActive, out isOauth);

			//If this used to use legacy format, need to generate a new SALT
			if (legacyFormat || string.IsNullOrWhiteSpace(salt))
			{
				salt = GenerateSalt();
			}

			SecUtility.CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x100, "newPasswordQuestion");
			if (newPasswordAnswer != null)
			{
				newPasswordAnswer = newPasswordAnswer.Trim();
			}
			SecUtility.CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
			if (!string.IsNullOrEmpty(newPasswordAnswer))
			{
				encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
			}
			else
			{
				encodedPasswordAnswer = newPasswordAnswer;
			}
			SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
			try
			{
				int changePasswordQuestionAnswerStatus = userManager.ChangePasswordQuestionAndAnswer(username, newPasswordQuestion, encodedPasswordAnswer);

				if (changePasswordQuestionAnswerStatus != 0)
				{
					throw new ProviderException(GetExceptionText(changePasswordQuestionAnswerStatus));
				}
				success = (changePasswordQuestionAnswerStatus == 0);
			}
			catch
			{
				throw;
			}
			return success;
		}

		/// <summary>
		/// Changes the password question and answer for a user
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="password">The user's current password</param>
		/// <param name="newPasswordQuestion">The new password question</param>
		/// <param name="newPasswordAnswer">The new password answer</param>
		/// <returns>True if successful</returns>
		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			string salt;
			int passwordFormat;
			string encodedPasswordAnswer;
			bool flag;
			bool usesLegacyFormat;
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			SecUtility.CheckParameter(ref password, true, true, false, 0x80, "password");
			if (!CheckPassword(username, password, false, false, out salt, out passwordFormat, out usesLegacyFormat))
			{
				return false;
			}

			//If this used to use legacy format, need to generate a new SALT
			if (usesLegacyFormat)
			{
				salt = GenerateSalt();
			}

			SecUtility.CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x100, "newPasswordQuestion");
			if (newPasswordAnswer != null)
			{
				newPasswordAnswer = newPasswordAnswer.Trim();
			}
			SecUtility.CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
			if (!string.IsNullOrEmpty(newPasswordAnswer))
			{
				encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
			}
			else
			{
				encodedPasswordAnswer = newPasswordAnswer;
			}
			SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "newPasswordAnswer");
			try
			{
				UserManager userManager = new UserManager();
				int status = userManager.ChangePasswordQuestionAndAnswer(username, newPasswordQuestion, encodedPasswordAnswer);

				if (status != 0)
				{
					throw new ProviderException(GetExceptionText(status));
				}
				flag = status == 0;
			}
			catch
			{
				throw;
			}
			return flag;
		}

		/// <summary>
		/// Checks the specified username and password to see if correct and that user is approved. Also updates the last login activity date
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="password">The user's current password</param>
		/// <param name="updateLastLoginActivityDate">Should we update the last login and activity dates</param>
		/// <param name="failIfNotApproved">Should it fail if not approved</param>
		/// <returns>True if it passes</returns>
		private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
		{
			string salt;
			int passwordFormat;
			bool usesLegacyFormat;
			return CheckPassword(username, password, updateLastLoginActivityDate, failIfNotApproved, out salt, out passwordFormat, out usesLegacyFormat);
		}

		/// <summary>
		/// Checks the specified username and password to see if correct and that user is approved. Also updates the last login activity date
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="password">The user's current password</param>
		/// <param name="updateLastLoginActivityDate">Should we update the last login and activity dates</param>
		/// <param name="failIfNotApproved">Should it fail if not approved</param>
		/// <param name="passwordFormat">Returns the current password format</param>
		/// <param name="salt">Returns the current password SALT</param>
		/// <param name="useLegacyFormat">Does this user use the legacy Spira v3.2 password format</param>
		/// <returns>True if it passes</returns>
		private bool CheckPassword(string username, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat, out bool useLegacyFormat)
		{
			const string METHOD_NAME = "CheckPassword";

			string storedEncodedPassword;
			int status;
			int failedPasswordAttemptCount;
			int failedPasswordAnswerAttemptCount;
			bool isApproved;
			bool isActive;
			string ldapDn;
			bool isOauth;
			DateTime lastLoginDate;
			DateTime lastActivityDate;
			UserManager userManager = new UserManager();

			//Get the password format, LDAP and other password information
			userManager.GetPasswordWithFormat(username, updateLastLoginActivityDate, out status, out storedEncodedPassword, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate, out useLegacyFormat, out ldapDn, out isActive, out isOauth);
			if (status != 0)
			{
				if (status == 1)
				{
					try { Logger.LogFailureAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_UserNotExist, username)); }
					catch { /* Catch in case running in unit tests */ }
				}
				else if (status == 99)
				{
					try { Logger.LogFailureAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_UserLocked, username)); }
					catch { /* Catch in case running in unit tests */ }
				}
				return false;
			}
			if (!isActive)
			{
				try { Logger.LogFailureAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_UserNotActive, username)); }
				catch { /* Catch in case running in unit tests */ }
				return false;
			}
			if (!isApproved && failIfNotApproved)
			{
				try { Logger.LogFailureAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_UserNotApproved, username)); }
				catch { /* Catch in case running in unit tests */ }
				return false;
			}

			//Remove all OAuth Cookies/Session. Why here? Because Logging In is currently handled by hidden Microsoft code,
			//  in the Login control. All this code happens AFTER code on the Login.aspx pagfe is executed, so no code on
			//  Login.aspx page knows that the login actually *failed*. So, there's no way that code on the Login.aspx page
			//  hand handle these Cookies. Once we get rid of the MS Login control (which we SHOULD....), we can move this to
			//  its proper place. This should be done first.
			var ctx = HttpContext.Current;
			bool inOauth = false;
			if (ctx != null)
			{
				//Remove the session variables
				//All session access needs to be inside the block because AJAX calls are sessionless, so it's null
				if (ctx.Session != null)
				{
					ctx.Session.Remove(OAuthManager.AUTHTOCTRL_SESSNAME);
					if (ctx.Session[OAuthManager.AUTH_COOKNAME] != null)
					{
						ctx.Session.Remove(OAuthManager.AUTH_COOKNAME);
						inOauth = true;
					}
				}
				//Remove any cookies.
				if (ctx.Request.Cookies.AllKeys.Any(n => n.Equals(OAuthManager.AUTHTOLOGIN_COOKNAME)))
				{
					ctx.Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Value = ".";
					ctx.Response.Cookies[OAuthManager.AUTHTOLOGIN_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
					inOauth = true;
				}
				if (ctx.Request.Cookies.AllKeys.Any(n => n.Equals(OAuthManager.AUTHTOCTRL_COOKNAME)))
				{
					ctx.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Value = ".";
					ctx.Response.Cookies[OAuthManager.AUTHTOCTRL_COOKNAME].Expires = DateTime.UtcNow.AddYears(-1);
					inOauth = true;
				}
			}
			//if we are IN an Oauth workflow AND the user is currently configured for LDAP, *OR* the user is already 
			//  linked to OAuth, just return,
			if ((inOauth & !string.IsNullOrWhiteSpace(ldapDn)) || isOauth)
			{
				//Return a false - can NOT directly link LDAP to Oauth. Thi
				return false;
			}


			if (!string.IsNullOrWhiteSpace(ldapDn))
			{
				//Use the LDAP client to authenticate
				LdapClient ldapClient = new LdapClient();
				ldapClient.LoadSettings();
				Logger.LogTraceEvent(PROVIDER_NAME + "::" + METHOD_NAME, "Authenticating user DN: " + ldapDn + " against ldap server " + ldapClient.LdapHost);
				bool validUser = ldapClient.Authenticate(ldapDn, password);

				//Update the last username date if successful
				if (validUser)
				{
					try
					{
						DateTime utcNow = DateTime.UtcNow;
						status = userManager.UpdatePasswordInfo(username, true, updateLastLoginActivityDate, MaxInvalidPasswordAttempts, PasswordAttemptWindow, utcNow, utcNow);
					}
					catch
					{
						throw;
					}
				}
				return validUser;
			}
			else
			{
				string encodedPassword = EncodePassword(password, passwordFormat, salt, useLegacyFormat);
				bool passwordMatches = encodedPassword.Equals(storedEncodedPassword);

				if (!passwordMatches)
				{
					try { Logger.LogFailureAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_PasswordNotMatch, username)); }
					catch { /* Catch in case running in unit tests */ }

				}
				else if (passwordMatches)
				{
					try { Logger.LogSuccessAuditEvent(PROVIDER_NAME + "::" + METHOD_NAME, string.Format(Resources.Messages.SpiraMembershipProvider_PasswordMatch, username)); }
					catch { /* Catch in case running in unit tests */ }
				}

				if ((passwordMatches && (failedPasswordAttemptCount == 0)) && (failedPasswordAnswerAttemptCount == 0))
				{
					//Login was successful, so set the flag that lets the system know that a user has logged in successfully once
					//(this is used to enable the 'register new account' option
					if (!ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce)
					{
						ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce = true;
						ConfigurationSettings.Default.Save();
					}
					return true;
				}
				try
				{
					DateTime utcNow = DateTime.UtcNow;
					status = userManager.UpdatePasswordInfo(username, passwordMatches, updateLastLoginActivityDate, MaxInvalidPasswordAttempts, PasswordAttemptWindow, passwordMatches ? utcNow : lastLoginDate, passwordMatches ? utcNow : lastActivityDate);
				}
				catch
				{
					throw;
				}
				return passwordMatches;
			}
		}

		/// <summary>
		/// Creates a new membership user
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="password">The new password for the user</param>
		/// <param name="email">The email address for the user</param>
		/// <param name="passwordQuestion">The password question</param>
		/// <param name="passwordAnswer">The password answer</param>
		/// <param name="isApproved">Is the user approved</param>
		/// <param name="ldapDn">The user's LDAP DN</param>
		/// <param name="rssToken">The user's RSS Token</param>
		/// <param name="status">The status of the user</param>
		/// <returns>The newly created membership user object</returns>
		/// <remarks>
		/// This one is not part of the Microsoft base implementation because it provides support for additional fields such as
		/// LdapDn and RssToken which are not part of the MembershipUser object. So you need to get a handle to the actual
		/// SpiraMembershipProvider instance not just the generic MembershipProvider class
		/// This one is used by an admin and doesn't enforce some of the password rules (e.g. no names)
		/// </remarks>
		public MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, string ldapDn, string rssToken, out MembershipCreateStatus status)
		{
			//If we have an LDAP DN we don't need any of the password stuff
			string salt = "";
			string pass = "";
			string encodedPasswordAnswer = "";
			MembershipUser membershipUser;
			if (string.IsNullOrEmpty(ldapDn))
			{
				if (!SecUtility.ValidateParameter(ref password, true, true, false, 0x80))
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				salt = GenerateSalt();
				pass = EncodePassword(password, (int)_PasswordFormat, salt);
				if (pass.Length > 0x80)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				if (passwordAnswer != null)
				{
					passwordAnswer = passwordAnswer.Trim();
				}
				if (!string.IsNullOrEmpty(passwordAnswer))
				{
					if (passwordAnswer.Length > 0x80)
					{
						status = MembershipCreateStatus.InvalidAnswer;
						return null;
					}
					encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_PasswordFormat, salt);
				}
				else
				{
					encodedPasswordAnswer = passwordAnswer;
				}
				if (!SecUtility.ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 0x80))
				{
					status = MembershipCreateStatus.InvalidAnswer;
					return null;
				}
				if (!SecUtility.ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 0x100))
				{
					status = MembershipCreateStatus.InvalidQuestion;
					return null;
				}
				if (password.Length < MinRequiredPasswordLength)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				int num = 0;
				for (int i = 0; i < password.Length; i++)
				{
					if (!char.IsLetterOrDigit(password, i))
					{
						num++;
					}
				}
				if (num < MinRequiredNonAlphanumericCharacters)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				if ((PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(password, PasswordStrengthRegularExpression))
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
				OnValidatingPassword(e);
				if (e.Cancel)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
			}
			if (!SecUtility.ValidateParameter(ref username, true, true, true, 0x100))
			{
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}
			if (!SecUtility.ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100))
			{
				status = MembershipCreateStatus.InvalidEmail;
				return null;
			}
			try
			{
				int errorCode;
				UserManager userManager = new UserManager();
				User user = userManager.CreateUser(username, pass, salt, email, passwordQuestion, encodedPasswordAnswer, isApproved, RequiresUniqueEmail, (int)PasswordFormat, ldapDn, rssToken, password, out errorCode);

				if ((errorCode < 0) || (errorCode > 11))
				{
					errorCode = 11;
				}
				status = (MembershipCreateStatus)errorCode;
				if (errorCode != 0)
				{
					return null;
				}
				//Populate membership object
				membershipUser = new MembershipUser(
					Name,
					user.UserName,
					user.UserId,
					user.EmailAddress,
					user.PasswordQuestion,
					user.Comment,
					user.IsApproved,
					user.IsLocked,
					user.CreationDate.ToLocalTime(),
					(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()
					);
			}
			catch
			{
				throw;
			}
			return membershipUser;
		}

		/// <summary>
		/// Creates a new membership user
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="password">The new password for the user</param>
		/// <param name="email">The email address for the user</param>
		/// <param name="passwordQuestion">The password question</param>
		/// <param name="passwordAnswer">The password answer</param>
		/// <param name="isApproved">Is the user approved</param>
		/// <param name="providerUserKey">The ID of the user</param>
		/// <param name="status">The status of the user</param>
		/// <returns>The newly created membership user object</returns>
		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			//Vars used in other blocks..
			string pass = null, salt = null, encodedPasswordAnswer = null;
			MembershipUser membershipUser;

			//See if we need to check for Password security. If coming from OAuth, no, we don't.
			OAuthManager.UserLoginInfo oAuthTok = null;
			if (HttpContext.Current != null && HttpContext.Current.Session[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
			{
				string cook = HttpContext.Current.Session[OAuthManager.AUTHTOCTRL_COOKNAME] as string;
				oAuthTok = OAuthManager.UserLoginInfo.Parse(cook);
			}

			if (oAuthTok == null || string.IsNullOrWhiteSpace(oAuthTok.UserProviderId))
			{
				if (!SecUtility.ValidateParameter(ref password, true, true, false, 0x80))
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}

				if (!DoesPasswordComplyWithNameRules(username, password))
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}

				salt = GenerateSalt();
				pass = EncodePassword(password, (int)_PasswordFormat, salt);
				if (pass.Length > 0x80)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}

				if (passwordAnswer != null)
					passwordAnswer = passwordAnswer.Trim();

				if (!string.IsNullOrEmpty(passwordAnswer))
				{
					if (passwordAnswer.Length > 0x80)
					{
						status = MembershipCreateStatus.InvalidAnswer;
						return null;
					}
					encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_PasswordFormat, salt);
				}
				else encodedPasswordAnswer = passwordAnswer;

				if (!SecUtility.ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 0x80))
				{
					status = MembershipCreateStatus.InvalidAnswer;
					return null;
				}

				if (!SecUtility.ValidateParameter(ref username, true, true, true, 0x100))
				{
					status = MembershipCreateStatus.InvalidUserName;
					return null;
				}

				if (!SecUtility.ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100))
				{
					status = MembershipCreateStatus.InvalidEmail;
					return null;
				}

				if (!SecUtility.ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 0x100))
				{
					status = MembershipCreateStatus.InvalidQuestion;
					return null;
				}

				if ((providerUserKey != null) && !(providerUserKey is Guid))
				{
					status = MembershipCreateStatus.InvalidProviderUserKey;
					return null;
				}

				if (password.Length < MinRequiredPasswordLength)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}

				int num = 0;
				for (int i = 0; i < password.Length; i++)
				{
					if (!char.IsLetterOrDigit(password, i))
					{
						num++;
					}
				}

				if (num < MinRequiredNonAlphanumericCharacters)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}

				if ((PasswordStrengthRegularExpression.Length > 0) && !Regex.IsMatch(password, PasswordStrengthRegularExpression))
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
				ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, true);
				OnValidatingPassword(e);
				if (e.Cancel)
				{
					status = MembershipCreateStatus.InvalidPassword;
					return null;
				}
			}
			else
			{
				//In this case, we need to set some needed fields.
				// username will be the email address.
				username = email;
			}

			try
			{
				//Create new RSS token
				string rssToken = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

				//Get the OAuth Token, ONLY if the user has an identity ket from the OAuth provider.
				Guid? oAuthId = null;
				if (oAuthTok != null && !string.IsNullOrWhiteSpace(oAuthTok.UserProviderId)) oAuthId = oAuthTok.ProviderId;

				int errorCode;
				UserManager userManager = new UserManager();
				User user = userManager.CreateUser(
					username,
					pass,
					salt,
					email,
					passwordQuestion,
					encodedPasswordAnswer,
					isApproved,
					RequiresUniqueEmail,
					(int)PasswordFormat,
					null,
					rssToken,
					password,
					out errorCode,
					oAuthId);

				//Set up OAuth fields, if necessary.
				if (oAuthTok != null && !string.IsNullOrWhiteSpace(oAuthTok.UserProviderId) && user != null)
				{
					OAuthManager oMgr = new OAuthManager();
					user = oMgr.User_LinkToProvider(user.UserId, oAuthTok.ProviderId, oAuthTok.UserProviderId);

					//Upload the Avatar?
					if (oAuthTok.ProviderData.Keys.Any(n => n.Equals((int)Provider.ClaimTypeEnum.Picture)) && oAuthTok.ProviderData[(int)Provider.ClaimTypeEnum.Picture] != null)
						oMgr.User_SetAvatarAsync(user.UserId, oAuthTok.ProviderData[(int)Provider.ClaimTypeEnum.Picture]);
				}

				//Error handling.
				if ((errorCode < 0) || (errorCode > 11)) errorCode = 11;
				status = (MembershipCreateStatus)errorCode;
				if (errorCode != 0) return null;

				//Populate membership object
				membershipUser = new MembershipUser(
					Name,
					user.UserName,
					user.UserId,
					user.EmailAddress,
					user.PasswordQuestion,
					user.Comment,
					user.IsApproved,
					user.IsLocked,
					user.CreationDate.ToLocalTime(),
					(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
					(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime());
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent("Web.Classes.SpiraMembershipProvider:CreateUser()", ex, "Creating user account.");
				throw ex;
			}
			return membershipUser;
		}

		/// <summary>
		/// Deletes a user by its username
		/// </summary>
		/// <param name="username">The username of the user to be deleted</param>
		/// <param name="deleteAllRelatedData">Should we delete all related data</param>
		/// <returns>True if successful</returns>
		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			bool flag;
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			try
			{
				UserManager userManager = new UserManager();
				flag = userManager.DeleteUser(username, deleteAllRelatedData);
			}
			catch
			{
				throw;
			}
			return flag;
		}

		/// <summary>
		/// Deletes a user by its id
		/// </summary>
		/// <param name="userId">The id of the user to be deleted</param>
		/// <param name="deleteAllRelatedData">Should we delete all related data</param>
		/// <returns>True if successful</returns>
		/// <remarks>Not part of the standard membership provider interface</remarks>
		public bool DeleteUser(int userId, bool deleteAllRelatedData)
		{
			bool flag;
			try
			{
				UserManager userManager = new UserManager();
				flag = userManager.DeleteUser(userId, deleteAllRelatedData);
			}
			catch
			{
				throw;
			}
			return flag;
		}

		/// <summary>
		/// Approves a user by its id
		/// </summary>
		/// <param name="userId">The id of the user to be deleted</param>
		/// <param name="deleteAllRelatedData">Should we delete all related data</param>
		/// <returns>True if successful</returns>
		/// <remarks>Not part of the standard membership provider interface</remarks>
		public bool ApproveUser(int userId)
		{
			try
			{
				UserManager userManager = new UserManager();
				User user = userManager.GetUserById(userId, false);
				if (!user.IsApproved)
				{
					user.StartTracking();
					user.IsApproved = true;
					userManager.Update(user);

					//Send approval email
					userManager.SendUserApproveNotification(user.UserName);
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Encodes the password so that we can compare with the saved password
		/// </summary>
		/// <param name="pass">The unencrypted password</param>
		/// <param name="passwordFormat">The password format</param>
		/// <param name="salt">The password SALT (if provided)</param>
		/// <param name="useLegacyFormat">True if this was a user from Spira v3.x or earlier (will be MD5 encrypted instead of SHA1)</param>
		/// <returns>The encrypted password</returns>
		private string EncodePassword(string pass, int passwordFormat, string salt, bool useLegacyFormat = false)
		{
			//If we have the old legacy format, we need to use the older ASP.NET 1.x FORMS authentication HASH (MD5 unsalted)
			if (useLegacyFormat)
			{
				return FormsAuthentication.HashPasswordForStoringInConfigFile(pass, "md5");
			}

			if (passwordFormat == 0)
			{
				return pass;
			}
			byte[] bytes = Encoding.Unicode.GetBytes(pass);
			byte[] src = Convert.FromBase64String(salt);
			byte[] inArray = null;
			if (passwordFormat == 1)
			{
				HashAlgorithm hashAlgorithm = GetHashAlgorithm();
				if (hashAlgorithm is KeyedHashAlgorithm)
				{
					KeyedHashAlgorithm algorithm2 = (KeyedHashAlgorithm)hashAlgorithm;
					if (algorithm2.Key.Length == src.Length)
					{
						algorithm2.Key = src;
					}
					else if (algorithm2.Key.Length < src.Length)
					{
						byte[] dst = new byte[algorithm2.Key.Length];
						Buffer.BlockCopy(src, 0, dst, 0, dst.Length);
						algorithm2.Key = dst;
					}
					else
					{
						int num2;
						byte[] buffer5 = new byte[algorithm2.Key.Length];
						for (int i = 0; i < buffer5.Length; i += num2)
						{
							num2 = Math.Min(src.Length, buffer5.Length - i);
							Buffer.BlockCopy(src, 0, buffer5, i, num2);
						}
						algorithm2.Key = buffer5;
					}
					inArray = algorithm2.ComputeHash(bytes);
				}
				else
				{
					byte[] buffer6 = new byte[src.Length + bytes.Length];
					Buffer.BlockCopy(src, 0, buffer6, 0, src.Length);
					Buffer.BlockCopy(bytes, 0, buffer6, src.Length, bytes.Length);
					inArray = hashAlgorithm.ComputeHash(buffer6);
				}
			}
			else
			{
				byte[] buffer7 = new byte[src.Length + bytes.Length];
				Buffer.BlockCopy(src, 0, buffer7, 0, src.Length);
				Buffer.BlockCopy(bytes, 0, buffer7, src.Length, bytes.Length);
				inArray = EncryptPassword(buffer7, _LegacyPasswordCompatibilityMode);
			}
			return Convert.ToBase64String(inArray);
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			SecUtility.CheckParameter(ref emailToMatch, false, false, false, 0x100, "emailToMatch");
			if (pageIndex < 0)
			{
				throw new ArgumentException(SR.GetString("PageIndex_bad"), "pageIndex");
			}
			if (pageSize < 1)
			{
				throw new ArgumentException(SR.GetString("PageSize_bad"), "pageSize");
			}
			int num = ((pageIndex * pageSize) + pageSize) - 1;
			if (num > 0x7fffffff)
			{
				throw new ArgumentException(SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
			}

			MembershipUserCollection membershipUsers = new MembershipUserCollection();
			totalRecords = 0;
			try
			{
				//Query for users with matching email address
				Expression<Func<User, bool>> userFilter = u => u.EmailAddress.ToLower().Contains(emailToMatch.ToLower());
				List<SortEntry<User, string>> userSorts = new List<SortEntry<User, string>>();
				SortEntry<User, string> sort1 = new SortEntry<User, string>();
				sort1.Direction = SortDirection.Ascending;
				sort1.Expression = u => u.EmailAddress;
				userSorts.Add(sort1);
				UserManager userManager = new UserManager();
				List<User> users = userManager.GetUsers(userFilter, userSorts, pageIndex, pageSize, out totalRecords);

				//Create the membership users list
				foreach (User user in users)
				{
					MembershipUser membershipUser = new MembershipUser(
						Name,
						user.UserName,
						user.UserId,
						user.EmailAddress,
						user.PasswordQuestion,
						user.Comment,
						user.IsApproved,
						user.IsLocked,
						user.CreationDate.ToLocalTime(),
						(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()

						);
					membershipUsers.Add(membershipUser);
				}
				return membershipUsers;
			}
			catch
			{
				throw;
			}
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			SecUtility.CheckParameter(ref usernameToMatch, true, true, false, 0x100, "usernameToMatch");
			if (pageIndex < 0)
			{
				throw new ArgumentException(SR.GetString("PageIndex_bad"), "pageIndex");
			}
			if (pageSize < 1)
			{
				throw new ArgumentException(SR.GetString("PageSize_bad"), "pageSize");
			}
			int num = ((pageIndex * pageSize) + pageSize) - 1;
			if (num > 0x7fffffff)
			{
				throw new ArgumentException(SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
			}

			MembershipUserCollection membershipUsers = new MembershipUserCollection();
			totalRecords = 0;
			try
			{
				//Query for users with matching email address
				Expression<Func<User, bool>> userFilter = u => (u.UserName.ToLower().Contains(usernameToMatch.ToLower()));
				List<SortEntry<User, string>> userSorts = new List<SortEntry<User, string>>();
				SortEntry<User, string> sort1 = new SortEntry<User, string>();
				sort1.Direction = SortDirection.Ascending;
				sort1.Expression = u => u.UserName;
				userSorts.Add(sort1);
				UserManager userManager = new UserManager();
				List<User> users = userManager.GetUsers(userFilter, userSorts, pageIndex, pageSize, out totalRecords);

				//Create the membership users list
				foreach (User user in users)
				{
					MembershipUser membershipUser = new MembershipUser(
						Name,
						user.UserName,
						user.UserId,
						user.EmailAddress,
						user.PasswordQuestion,
						user.Comment,
						user.IsApproved,
						user.IsLocked,
						user.CreationDate.ToLocalTime(),
						(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()
						);
					membershipUsers.Add(membershipUser);
				}
				return membershipUsers;
			}
			catch
			{
				throw;
			}
		}

		public virtual string GeneratePassword()
		{
			return Membership.GeneratePassword(Math.Min(MinRequiredPasswordLength, GENERATE_PASS_LEN), MinRequiredNonAlphanumericCharacters);
		}

		/// <summary>
		/// Generates a new hash salt to make data secure
		/// </summary>
		/// <returns>The salt</returns>
		private string GenerateSalt()
		{
			byte[] data = new byte[0x10];
			new RNGCryptoServiceProvider().GetBytes(data);
			return Convert.ToBase64String(data);
		}

		/// <summary>
		/// Gets all the users in the system
		/// </summary>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <param name="totalRecords"></param>
		/// <returns></returns>
		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
			{
				throw new ArgumentException(SR.GetString("PageIndex_bad"), "pageIndex");
			}
			if (pageSize < 1)
			{
				throw new ArgumentException(SR.GetString("PageSize_bad"), "pageSize");
			}
			int num = ((pageIndex * pageSize) + pageSize) - 1;
			if (num > 0x7fffffff)
			{
				throw new ArgumentException(SR.GetString("PageIndex_PageSize_bad"), "pageIndex and pageSize");
			}
			MembershipUserCollection membershipUsers = new MembershipUserCollection();
			totalRecords = 0;
			try
			{
				UserManager userManager = new UserManager();
				List<User> users = userManager.GetUsers(pageIndex, pageSize, out totalRecords);

				//Create the membership users list
				foreach (User user in users)
				{
					MembershipUser membershipUser = new MembershipUser(
						Name,
						user.UserName,
						user.UserId,
						user.EmailAddress,
						user.PasswordQuestion,
						user.Comment,
						user.IsApproved,
						user.IsLocked,
						user.CreationDate.ToLocalTime(),
						(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()
						);
					membershipUsers.Add(membershipUser);
				}
				return membershipUsers;
			}
			catch
			{
				throw;
			}
		}

		private string GetEncodedPasswordAnswer(string username, string passwordAnswer)
		{
			int num;
			int num2;
			int num3;
			int num4;
			string str;
			string str2;
			bool flag;
			bool legacyFormat;
			bool isActive;
			DateTime time;
			DateTime time2;
			string ldapDn;
			bool isOauth;

			if (passwordAnswer != null)
			{
				passwordAnswer = passwordAnswer.Trim();
			}
			if (string.IsNullOrEmpty(passwordAnswer))
			{
				return passwordAnswer;
			}
			UserManager userManager = new UserManager();
			userManager.GetPasswordWithFormat(username, false, out num, out str, out num2, out str2, out num3, out num4, out flag, out time, out time2, out legacyFormat, out ldapDn, out isActive, out isOauth);
			if (num != 0)
			{
				throw new ProviderException(GetExceptionText(num));
			}
			return EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), num2, str2);
		}

		private string GetExceptionText(int status)
		{
			string str;
			switch (status)
			{
				case 0:
					return string.Empty;

				case 1:
					str = "Membership_UserNotFound";
					break;

				case 2:
					str = "Membership_WrongPassword";
					break;

				case 3:
					str = "Membership_WrongAnswer";
					break;

				case 4:
					str = "Membership_InvalidPassword";
					break;

				case 5:
					str = "Membership_InvalidQuestion";
					break;

				case 6:
					str = "Membership_InvalidAnswer";
					break;

				case 7:
					str = "Membership_InvalidEmail";
					break;

				case 0x63:
					str = "Membership_AccountLockOut";
					break;

				default:
					str = "Provider_Error";
					break;
			}
			return SR.GetString(str);
		}

		/// <summary>
		/// Gets the hash algorithm we should use
		/// </summary>
		/// <returns>The appropriate algorithm</returns>
		private HashAlgorithm GetHashAlgorithm()
		{
			const string METHOD_NAME = "GetHashAlgorithm";

			if (s_HashAlgorithm != null)
			{
				return HashAlgorithm.Create(s_HashAlgorithm);
			}
			string hashAlgorithmType = Membership.HashAlgorithmType;
			if (((_LegacyPasswordCompatibilityMode == MembershipPasswordCompatibilityMode.Framework20)) && (hashAlgorithmType != "MD5"))
			{
				hashAlgorithmType = "SHA1";
			}
			HashAlgorithm algorithm = HashAlgorithm.Create(hashAlgorithmType);
			if (algorithm == null)
			{
				Logger.LogErrorEvent(PROVIDER_NAME + METHOD_NAME, "Unable to create Hash Algorithm");

				throw new ProviderException("Unable to create Hash algorithm");
			}
			s_HashAlgorithm = hashAlgorithmType;
			return algorithm;
		}

		private string GetNullableString(SqlDataReader reader, int col)
		{
			if (!reader.IsDBNull(col))
			{
				return reader.GetString(col);
			}
			return null;
		}

		public override int GetNumberOfUsersOnline()
		{
			int num;
			try
			{
				UserManager userManager = new UserManager();
				num = userManager.GetNumberOfUsersOnline(Membership.UserIsOnlineTimeWindow);
			}
			catch
			{
				throw;
			}
			return num;
		}

		/// <summary>
		/// Gets the password if that functionality is enabled
		/// </summary>
		/// <param name="username"></param>
		/// <param name="passwordAnswer"></param>
		/// <returns></returns>
		public override string GetPassword(string username, string passwordAnswer)
		{
			if (!EnablePasswordRetrieval)
			{
				throw new NotSupportedException(SR.GetString("Membership_PasswordRetrieval_not_supported"));
			}
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			string encodedPasswordAnswer = GetEncodedPasswordAnswer(username, passwordAnswer);
			SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "passwordAnswer");
			int passwordFormat = 0;
			int status = 0;
			UserManager userManager = new UserManager();
			string pass = userManager.GetPassword(username, encodedPasswordAnswer, RequiresQuestionAndAnswer, PasswordAttemptWindow, MaxInvalidPasswordAttempts, out passwordFormat, out status);
			if (pass != null)
			{
				return UnEncodePassword(pass, passwordFormat);
			}
			string exceptionText = GetExceptionText(status);
			if (IsStatusDueToBadPassword(status))
			{
				throw new MembershipPasswordException(exceptionText);
			}
			throw new ProviderException(exceptionText);
		}

		/// <summary>
		/// Gets a membership user by its id
		/// </summary>
		/// <param name="providerUserKey"></param>
		/// <param name="userIsOnline"></param>
		/// <returns></returns>
		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
			//See if we already have a cached copy (make sure same user)
			if (HttpContext.Current != null && HttpContext.Current.Items[CURRENT_USER] != null && providerUserKey is Int32)
			{
				MembershipUser membershipUserCached = (MembershipUser)HttpContext.Current.Items[CURRENT_USER];
				if (membershipUserCached.ProviderUserKey is Int32 && (int)membershipUserCached.ProviderUserKey == (int)providerUserKey)
				{
					return membershipUserCached;
				}
			}

			MembershipUser membershipUser = null;
			if (providerUserKey == null)
			{
				throw new ArgumentNullException("providerUserKey");
			}
			if (!(providerUserKey is int))
			{
				throw new ArgumentException(SR.GetString("Membership_InvalidProviderUserKey"), "providerUserKey");
			}
			try
			{
				try
				{
					//Get the user object (and update activity)
					UserManager userManager = new UserManager();
					int userId = (int)providerUserKey;
					User user = userManager.GetUserById(userId, userIsOnline);

					//Populate membership object
					membershipUser = new MembershipUser(
						Name,
						user.UserName,
						user.UserId,
						user.EmailAddress,
						user.PasswordQuestion,
						user.Comment,
						user.IsApproved,
						user.IsLocked,
						user.CreationDate.ToLocalTime(),
						(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()
						);
				}
				catch (ArtifactNotExistsException)
				{
					membershipUser = null;
				}
			}
			catch
			{
				throw;
			}
			if (HttpContext.Current != null)
			{
				HttpContext.Current.Items[CURRENT_USER] = membershipUser;
			}
			return membershipUser;
		}

		/// <summary>
		/// Is the specified user an LDAP user
		/// </summary>
		/// <param name="username">The user name</param>
		/// <returns>True if the user is an LDAP user</returns>
		/// <remarks>If the user cannot be found, returns false rather than throwing an exception</remarks>
		public bool IsLdapUser(string username)
		{
			SecUtility.CheckParameter(ref username, true, false, true, 0x100, "username");

			try
			{
				//Get the user object
				UserManager userManager = new UserManager();
				User user = userManager.GetUserByLogin(username, false, true);
				return !string.IsNullOrEmpty(user.LdapDn);
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
		}

		/// <summary>Gets a specific user entity by username</summary>
		/// <param name="username">The username</param>
		/// <returns>A Spira user entity</returns>
		/// <remarks>Used when we need to access extended properties like RssToken</remarks>
		public User GetProviderUser(string username)
		{
			SecUtility.CheckParameter(ref username, true, false, true, 0x100, "username");
			try
			{
				try
				{
					//Get the user object
					UserManager userManager = new UserManager();
					User user = userManager.GetUserByLogin(username, false, true);
					return user;
				}
				catch (ArtifactNotExistsException)
				{
					return null;
				}
			}
			catch
			{
				throw;
			}
		}

		/// <summary>Gets a specific user entity by id</summary>
		/// <param name="username">The username</param>
		/// <returns>A Spira user entity</returns>
		/// <remarks>Used when we need to access extended properties like RssToken</remarks>
		public User GetProviderUser(int userId)
		{
			try
			{
				try
				{
					//Get the user object
					UserManager userManager = new UserManager();
					User user = userManager.GetUserById(userId, false);
					return user;
				}
				catch (ArtifactNotExistsException)
				{
					return null;
				}
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// If the password not containing user's name or login is enabled, make sure the new password doesn't violate this rule
		/// </summary>
		/// <param name="user">The user object</param>
		/// <param name="newPassword">The new password</param>
		/// <returns></returns>
		public bool DoesPasswordComplyWithNameRules(DataModel.User user, string newPassword)
		{
			//First see if we even need to check for this 
			if (!ConfigurationSettings.Default.Security_PasswordNoNames)
			{
				return true;
			}

			//Check for null cases (should not happen in theory) 
			if (newPassword == null || user == null || user.UserName == null)
			{
				Logger.LogErrorEvent("SpiraMembershipProvider.DoesPasswordComplyWithNameRules", "Unable to check password name rule, field is null.");
				return false;
			}
			if (newPassword.ToLowerInvariant().Contains(user.UserName.ToLowerInvariant()))
			{
				return false;
			}
			if (user.Profile != null && user.Profile.FirstName != null && user.Profile.LastName != null)
			{
				if (newPassword.ToLowerInvariant().Contains(user.Profile.FirstName.ToLowerInvariant()) || newPassword.ToLowerInvariant().Contains(user.Profile.LastName.ToLowerInvariant()))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// If the password not containing user's login is enabled, make sure the new password doesn't violate this rule
		/// </summary>
		/// <param name="user">The user object</param>
		/// <param name="newPassword">The new password</param>
		/// <returns></returns>
		public bool DoesPasswordComplyWithNameRules(string username, string newPassword)
		{
			//First see if we even need to check for this
			if (!ConfigurationSettings.Default.Security_PasswordNoNames)
			{
				return true;
			}

			//Check for null cases (should not happen in theory)
			if (newPassword == null || username == null)
			{
				Logger.LogErrorEvent("SpiraMembershipProvider.DoesPasswordComplyWithNameRules", "Unable to check password name rule, field is null.");
				return false;
			}
			if (newPassword.ToLowerInvariant().Contains(username.ToLowerInvariant()))
			{
				return false;
			}

			return true;
		}

		/// <summary>Gets a specific user by username</summary>
		/// <param name="username"></param>
		/// <param name="userIsOnline">Is the user online, and we should update the last activity date</param>
		/// <returns></returns>
		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
			//See if we already have a cached copy
			if (HttpContext.Current != null && HttpContext.Current.Items[CURRENT_USER] != null)
			{
				MembershipUser membershipUserCached = (MembershipUser)HttpContext.Current.Items[CURRENT_USER];
				if (membershipUserCached.UserName == username)
				{
					return membershipUserCached;
				}
			}

			MembershipUser membershipUser = null;
			SecUtility.CheckParameter(ref username, true, false, true, 0x100, "username");
			try
			{
				try
				{
					//Get the user object (and update activity)
					UserManager userManager = new UserManager();
					User user = userManager.GetUserByLogin(username, userIsOnline, true);

					//Populate membership object
					membershipUser = new MembershipUser(
						Name,
						user.UserName,
						user.UserId,
						user.EmailAddress,
						user.PasswordQuestion,
						user.Comment,
						user.IsApproved,
						user.IsLocked,
						user.CreationDate.ToLocalTime(),
						(user.LastLoginDate.HasValue) ? user.LastLoginDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastActivityDate.HasValue) ? user.LastActivityDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastPasswordChangedDate.HasValue) ? user.LastPasswordChangedDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime(),
						(user.LastLockoutDate.HasValue) ? user.LastLockoutDate.Value.ToLocalTime() : user.CreationDate.ToLocalTime()
						);
				}
				catch (ArtifactNotExistsException)
				{
					membershipUser = null;
				}
			}
			catch
			{
				throw;
			}
			if (HttpContext.Current != null)
			{
				HttpContext.Current.Items[CURRENT_USER] = membershipUser;
			}
			return membershipUser;
		}

		/// <summary>
		/// Gets the user name from its email address
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public override string GetUserNameByEmail(string email)
		{
			string str2;
			SecUtility.CheckParameter(ref email, false, false, false, 0x100, "email");
			try
			{
				try
				{
					//Get the user by email address (they are guaranteed to be unique through database constraint)
					UserManager userManager = new UserManager();
					User user = userManager.GetUserByEmailAddress(email);
					str2 = user.UserName;
				}
				catch (ArtifactNotExistsException)
				{
					//No matching user exists
					return "";
				}
			}
			catch
			{
				throw;
			}
			return str2;
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			if (string.IsNullOrEmpty(name))
			{
				name = PROVIDER_NAME;
			}
			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "This provider authenticates users against the application database using the business logic layer");
			}
			base.Initialize(name, config);
			_EnablePasswordRetrieval = SecUtility.GetBooleanValue(config, "enablePasswordRetrieval", false);
			_EnablePasswordReset = SecUtility.GetBooleanValue(config, "enablePasswordReset", true);
			_RequiresQuestionAndAnswer = SecUtility.GetBooleanValue(config, "requiresQuestionAndAnswer", true);
			_RequiresUniqueEmail = SecUtility.GetBooleanValue(config, "requiresUniqueEmail", true);
			_PasswordStrengthRegularExpression = ConfigurationSettings.Default.Membership_PasswordStrengthRegularExpression;
			if (_PasswordStrengthRegularExpression != null)
			{
				_PasswordStrengthRegularExpression = _PasswordStrengthRegularExpression.Trim();
				if (_PasswordStrengthRegularExpression.Length == 0)
				{
					goto Label_016C;
				}
				try
				{
					new Regex(_PasswordStrengthRegularExpression);
					goto Label_016C;
				}
				catch (ArgumentException exception)
				{
					throw new ProviderException(exception.Message, exception);
				}
			}
			_PasswordStrengthRegularExpression = string.Empty;
			Label_016C:
			string str = config["passwordFormat"];
			if (str == null)
			{
				str = "Hashed";
			}
			string str4 = str;
			if (str4 != null)
			{
				if (!(str4 == "Clear"))
				{
					if (str4 == "Encrypted")
					{
						_PasswordFormat = MembershipPasswordFormat.Encrypted;
						goto Label_025C;
					}
					if (str4 == "Hashed")
					{
						_PasswordFormat = MembershipPasswordFormat.Hashed;
						goto Label_025C;
					}
				}
				else
				{
					_PasswordFormat = MembershipPasswordFormat.Clear;
					goto Label_025C;
				}
			}
			throw new ProviderException(SR.GetString("Provider_bad_password_format"));
			Label_025C:
			if ((PasswordFormat == MembershipPasswordFormat.Hashed) && EnablePasswordRetrieval)
			{
				throw new ProviderException(SR.GetString("Provider_can_not_retrieve_hashed_password"));
			}
			string str2 = config["passwordCompatMode"];
			if (!string.IsNullOrEmpty(str2))
			{
				_LegacyPasswordCompatibilityMode = (MembershipPasswordCompatibilityMode)Enum.Parse(typeof(MembershipPasswordCompatibilityMode), str2);
			}
			config.Remove("enablePasswordRetrieval");
			config.Remove("enablePasswordReset");
			config.Remove("requiresQuestionAndAnswer");
			config.Remove("requiresUniqueEmail");
			config.Remove("name");
			config.Remove("passwordStrengthRegularExpression");
			config.Remove("passwordCompatMode");
			config.Remove("passwordFormat");
			//The following have been replaced by Spira Settings
			//config.Remove("maxInvalidPasswordAttempts");
			//config.Remove("passwordAttemptWindow");
			//config.Remove("minRequiredPasswordLength");
			//config.Remove("minRequiredNonalphanumericCharacters");
			if (config.Count > 0)
			{
				string key = config.GetKey(0);
				if (!string.IsNullOrEmpty(key))
				{
					throw new ProviderException(string.Format("SpiraMembershipProvider: unrecognized attribute '{0}'", key));
				}
			}
		}

		private bool IsStatusDueToBadPassword(int status)
		{
			return (((status >= 2) && (status <= 6)) || (status == 0x63));
		}

		/// <summary>
		/// Resets the current user's password
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="passwordAnswer">The correct answer to the password challenge question</param>
		/// <returns>The new password (unencrypted/hashed)</returns>
		public override string ResetPassword(string username, string passwordAnswer)
		{
			string salt;
			int passwordFormat;
			string passwdFromDB;
			int status;
			int failedPasswordAttemptCount;
			int failedPasswordAnswerAttemptCount;
			bool isApproved;
			bool isActive;
			bool legacyFormat;
			DateTime lastLoginDate;
			DateTime lastActivityDate;
			string ldapDn;
			bool isOauth;

			if (!EnablePasswordReset)
			{
				throw new NotSupportedException(SR.GetString("Not_configured_to_support_password_resets"));
			}
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			UserManager userManager = new UserManager();
			userManager.GetPasswordWithFormat(username, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate, out legacyFormat, out ldapDn, out isActive, out isOauth);
			if (status == 0)
			{
				if (!isActive)
				{
					//Cannot reset the password of inactive users
					throw new ProviderException(SR.GetString("Membership_AccountNotActive"));
				}

				string encodedPasswordAnswer;
				string newPassword;
				if (passwordAnswer != null)
				{
					passwordAnswer = passwordAnswer.Trim();
				}
				if (!string.IsNullOrEmpty(passwordAnswer))
				{
					encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt, legacyFormat);
				}
				else
				{
					encodedPasswordAnswer = passwordAnswer;
				}

				//If this is a legacy Spira v3.x user we need to generate a new salt (for the new password)
				if (legacyFormat || string.IsNullOrWhiteSpace(salt))
				{
					salt = GenerateSalt();
				}

				SecUtility.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 0x80, "passwordAnswer");
				string password = GeneratePassword();
				ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(username, password, false);
				OnValidatingPassword(e);
				if (e.Cancel)
				{
					if (e.FailureInformation != null)
					{
						throw e.FailureInformation;
					}
					throw new ProviderException(SR.GetString("Membership_Custom_Password_Validation_Failure"));
				}
				try
				{
					string passwordAnswer2 = null;
					if (RequiresQuestionAndAnswer)
					{
						passwordAnswer2 = encodedPasswordAnswer;
					}
					status = userManager.ResetPassword(username, EncodePassword(password, passwordFormat, salt), MaxInvalidPasswordAttempts, PasswordAttemptWindow, salt, passwordFormat, passwordAnswer2);

					if (status != 0)
					{
						string exceptionText = GetExceptionText(status);
						if (IsStatusDueToBadPassword(status))
						{
							throw new MembershipPasswordException(exceptionText);
						}
						throw new ProviderException(exceptionText);
					}
					newPassword = password;
				}
				catch
				{
					throw;
				}
				return newPassword;
			}
			if (IsStatusDueToBadPassword(status))
			{
				throw new MembershipPasswordException(GetExceptionText(status));
			}
			throw new ProviderException(GetExceptionText(status));
		}

		private DateTime RoundToSeconds(DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
		}

		private string UnEncodePassword(string pass, int passwordFormat)
		{
			switch (passwordFormat)
			{
				case 0:
					return pass;

				case 1:
					throw new ProviderException(SR.GetString("Provider_can_not_decode_hashed_password"));
			}
			byte[] encodedPassword = Convert.FromBase64String(pass);
			byte[] bytes = DecryptPassword(encodedPassword);
			if (bytes == null)
			{
				return null;
			}
			return Encoding.Unicode.GetString(bytes, 0x10, bytes.Length - 0x10);
		}

		public override bool UnlockUser(string username)
		{
			bool flag;
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "username");
			try
			{
				UserManager userManager = new UserManager();
				flag = userManager.UnlockUser(username);
				return flag;
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Updates a provider user object
		/// </summary>
		/// <param name="user"></param>
		public void UpdateProviderUser(User user)
		{
			string email = user.EmailAddress;
			SecUtility.CheckParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100, "Email");
			new UserManager().Update(user);
		}

		public override void UpdateUser(MembershipUser user)
		{
			if (user == null)
			{
				throw new ArgumentNullException("user");
			}
			string username = user.UserName;
			SecUtility.CheckParameter(ref username, true, true, true, 0x100, "UserName");
			string email = user.Email;
			SecUtility.CheckParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 0x100, "Email");
			user.Email = email;
			try
			{
				//Set null dates?

				UserManager userManager = new UserManager();
				int status = userManager.UpdateUser(user.UserName, user.Email, user.Comment, user.IsApproved, user.LastLoginDate.ToUniversalTime(), user.LastActivityDate.ToUniversalTime(), RequiresUniqueEmail);

				if (status != 0)
				{
					throw new ProviderException(GetExceptionText(status));
				}
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Verifies to see if the user can login with the provided username and password
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="password">The user's password</param>
		/// <returns>True if the username/password match</returns>
		public override bool ValidateUser(string username, string password)
		{
			if ((SecUtility.ValidateParameter(ref username, true, true, true, 0x100) && SecUtility.ValidateParameter(ref password, true, true, false, 0x80)) && CheckPassword(username, password, true, true))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Validates the user id and RSS token combination to make sure user can authenticate correctly
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="rssToken">The user's RSS token</param>
		/// <param name="updateLastLoginActivityDate">Should we update the last login and activity dates</param>
		/// <param name="failIfNotApproved">Should it fail if not approved</param>
		/// <returns>True if authenticates</returns>
		public bool ValidateUserByRssToken(int userId, string rssToken, bool updateLastLoginActivityDate = true, bool failIfNotApproved = true)
		{
			const string METHOD_NAME = "ValidateUserByRssToken";

			UserManager userManager = new UserManager();

			//Get the user record
			//Make sure active and approved (if required)
			User user = null;
			try
			{
				user = userManager.GetUserById(userId);
			}
			catch (ArtifactNotExistsException)
			{
				return false;

			}
			if (user == null)
			{
				return false;
			}

			if (user.IsLocked || !user.IsActive)
			{
				return false;
			}
			if (!user.IsApproved && failIfNotApproved)
			{
				return false;
			}

			//If we don't have an RSS token then automatically fail
			if (string.IsNullOrWhiteSpace(user.RssToken))
			{
				return false;
			}
			else
			{
				//See if the token matches, add to the failed attempt count if not
				int failedPasswordAttemptCount = user.FailedPasswordAttemptCount;
				int failedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount;
				bool tokenMatches = user.RssToken.Equals(rssToken);
				DateTime lastLoginDate = (user.LastLoginDate.HasValue) ? user.LastLoginDate.Value : DateTime.MinValue;
				DateTime lastActivityDate = (user.LastActivityDate.HasValue) ? user.LastActivityDate.Value : DateTime.MinValue;
				if ((tokenMatches && (failedPasswordAttemptCount == 0)) && (failedPasswordAnswerAttemptCount == 0))
				{
					//Login was successful, so set the flag that lets the system know that a user has logged in successfully once
					//(this is used to enable the 'register new account' option
					if (!ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce)
					{
						ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce = true;
						ConfigurationSettings.Default.Save();
					}
					return true;
				}
				try
				{
					DateTime utcNow = DateTime.UtcNow;
					int status = userManager.UpdatePasswordInfo(user.UserName, tokenMatches, updateLastLoginActivityDate, MaxInvalidPasswordAttempts, PasswordAttemptWindow, tokenMatches ? utcNow : lastLoginDate, tokenMatches ? utcNow : lastActivityDate);
				}
				catch
				{
					throw;
				}
				return tokenMatches;
			}
		}

		/// <summary>
		/// Validates the username and RSS token combination to make sure user can authenticate correctly
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <param name="rssToken">The user's RSS token</param>
		/// <param name="updateLastLoginActivityDate">Should we update the last login and activity dates</param>
		/// <param name="failIfNotApproved">Should it fail if not approved</param>
		/// <returns>True if authenticates</returns>
		public bool ValidateUserByRssToken(string username, string rssToken, bool updateLastLoginActivityDate = true, bool failIfNotApproved = true)
		{
			const string METHOD_NAME = "ValidateUserByRssToken";

			UserManager userManager = new UserManager();

			//Get the user record
			//Make sure active and approved (if required)
			User user = null;
			try
			{
				user = userManager.GetUserByLogin(username);
			}
			catch (ArtifactNotExistsException)
			{
				return false;

			}
			if (user == null)
			{
				return false;
			}

			if (user.IsLocked || !user.IsActive)
			{
				return false;
			}
			if (!user.IsApproved && failIfNotApproved)
			{
				return false;
			}

			//If we don't have an RSS token then automatically fail
			if (string.IsNullOrWhiteSpace(user.RssToken))
			{
				return false;
			}
			else
			{
				//See if the token matches, add to the failed attempt count if not
				int failedPasswordAttemptCount = user.FailedPasswordAttemptCount;
				int failedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount;
				bool tokenMatches = user.RssToken.Equals(rssToken);
				DateTime lastLoginDate = (user.LastLoginDate.HasValue) ? user.LastLoginDate.Value : DateTime.MinValue;
				DateTime lastActivityDate = (user.LastActivityDate.HasValue) ? user.LastActivityDate.Value : DateTime.MinValue;
				if ((tokenMatches && (failedPasswordAttemptCount == 0)) && (failedPasswordAnswerAttemptCount == 0))
				{
					//Login was successful, so set the flag that lets the system know that a user has logged in successfully once
					//(this is used to enable the 'register new account' option
					if (!ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce)
					{
						ConfigurationSettings.Default.Membership_SomeoneHasLoggedInOnce = true;
						ConfigurationSettings.Default.Save();
					}
					return true;
				}
				try
				{
					DateTime utcNow = DateTime.UtcNow;
					int status = userManager.UpdatePasswordInfo(user.UserName, tokenMatches, updateLastLoginActivityDate, MaxInvalidPasswordAttempts, PasswordAttemptWindow, tokenMatches ? utcNow : lastLoginDate, tokenMatches ? utcNow : lastActivityDate);
				}
				catch
				{
					throw;
				}
				return tokenMatches;
			}
		}

		// Properties

		/// <summary>
		/// The application name (not used since users are global to application)
		/// </summary>
		public override string ApplicationName
		{
			get
			{
				return APPLICATION_NAME;
			}
			set
			{
				throw new InvalidOperationException("This provider does not support multiple application instances");
			}
		}

		public override bool EnablePasswordReset
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _EnablePasswordReset;
			}
		}

		public override bool EnablePasswordRetrieval
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _EnablePasswordRetrieval;
			}
		}

		public override int MaxInvalidPasswordAttempts
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return ConfigurationSettings.Default.Membership_MaxInvalidPasswordAttempts;
			}
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return ConfigurationSettings.Default.Membership_MinRequiredNonalphanumericCharacters;
			}
		}

		public override int MinRequiredPasswordLength
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return ConfigurationSettings.Default.Membership_MinRequiredPasswordLength;
			}
		}

		public override int PasswordAttemptWindow
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return ConfigurationSettings.Default.Membership_PasswordAttemptWindow;
			}
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _PasswordFormat;
			}
		}

		public override string PasswordStrengthRegularExpression
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _PasswordStrengthRegularExpression;
			}
		}

		public override bool RequiresQuestionAndAnswer
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _RequiresQuestionAndAnswer;
			}
		}

		public override bool RequiresUniqueEmail
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return _RequiresUniqueEmail;
			}
		}
	}
}