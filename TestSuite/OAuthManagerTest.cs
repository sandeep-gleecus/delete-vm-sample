using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using static Inflectra.SpiraTest.Business.OAuthManager;

namespace Inflectra.SpiraTest.TestSuite
{
	/// <summary>This tests OAuth Manager functions.</summary> 
	[TestFixture]
	[SpiraTestConfiguration(
		InternalRoutines.SPIRATEST_INTERNAL_URL,
		InternalRoutines.SPIRATEST_INTERNAL_LOGIN,
		InternalRoutines.SPIRATEST_INTERNAL_PASSWORD,
		InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID,
		InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class OAuthManagerTest : IRequiresSessionState
	{
		/// <summary>The GUID that represents the Dummy provider.</summary>
		static Guid guidDummy = new Guid("cba68583-4d3c-4aaa-8472-e225b06c392c");
		/// <summary>The GUID that represents the GitLab provider.</summary>
		static Guid guidGitlab = new Guid("c74f8326-d95e-4649-ab92-d450b57f6d77");

		#region Static Constants
		private const string PROV_DM_NAME = "Google";
		private const string PROV_GL_NAME = "GitHub";
		#endregion Static Constants

		static OAuthManagerTest()
		{
		}

		/// <summary>Sets up any data</summary> 
		[TestFixtureSetUp]
		public void Init()
		{
		}

		/// <summary>Cleans up any data that was changed</summary> 
		[TestFixtureTearDown]
		public void CleanUp()
		{
		}


		/// <summary>
		/// Retrieves all available providers, and checks some of their expected values. 
		/// </summary>
		/// <remarks>
		/// The solution build should at least copy GitLab and Dummy provider over.
		/// </remarks>
		[Test]
		[SpiraTestCase(2096)]
		[Description("Retrieves all available providers, and checks some of their expected values. ")]
		public void _001_Providers_RetrieveAll()
		{
			var oMgr = new OAuthManager();

			//Retrieve the providers.
			List<GlobalOAuthProvider> list1 = oMgr.Providers_RetrieveAll(false);
			Assert.IsNotNull(list1, "List1 returned back was null");
			//Check that we got at LEAST two.
			Assert.IsTrue(list1.Count >= 2, "Not enough providers returned! Returned count was: " + list1.Count.ToString());
			//Check that Google and GitLab exists.
			GlobalOAuthProvider provGitLab = list1.SingleOrDefault(p => p.Name.Equals("GitHub"));
			GlobalOAuthProvider provDummy = list1.SingleOrDefault(p => p.Name.Equals("Google"));
			Assert.IsNotNull(provDummy, "Could not load Dummy provider.");
			Assert.IsNotNull(provGitLab, "Could not load GitLab provider.");
			//Verify each provider's information.
			Assert.IsNotNull(provGitLab.OAuthProviderId, "Unable to retrieve GitLab provider GUID");
			Assert.IsNotNull(provDummy.OAuthProviderId, "Unable to retrieve Dummy provider GUID");
			Assert.IsTrue(provGitLab.OAuthProviderId.Equals(guidGitlab), "GitLab provider had wrong GUID.");
			Assert.IsTrue(provDummy.OAuthProviderId.Equals(guidDummy), "Dummy provider had wrong GUID.");
		}

		/// <summary>
		/// Retrieves a given provider and verifies what we asked for is returned.
		/// </summary>
		[Test]
		[SpiraTestCase(2097)]
		[Description("Retrieves a given provider and verifies what we asked for is returned.")]
		public void _002_Providers_RetrieveById()
		{
			var oMgr = new OAuthManager();

			//Pull the Gitlab provider.
			GlobalOAuthProvider prov1 = oMgr.Providers_RetrieveById(guidDummy);
			Assert.IsNotNull(prov1, "Unable to retrieve Dummy provider");
			Assert.IsTrue(prov1.Name.Equals(PROV_DM_NAME), "Wrong provider pulled! Name incorrect!");
			Assert.IsTrue(prov1.OAuthProviderId.Equals(guidDummy), "Wrong provider pulled! GUID incorrect!");

			//Pull the Gitlab provider.
			GlobalOAuthProvider prov2 = oMgr.Providers_RetrieveById(guidGitlab);
			Assert.IsNotNull(prov2, "Unable to retrieve GitLab provider");
			Assert.IsTrue(prov2.Name.Equals(PROV_GL_NAME), "Wrong provider pulled! Name incorrect!");
			Assert.IsTrue(prov2.OAuthProviderId.Equals(guidGitlab), "Wrong provider pulled! GUID incorrect!");
		}

		/// <summary>
		/// Should retrieve an asked-for provider, update it, and check that settings were saved.
		/// </summary>
		[Test]
		[SpiraTestCase(2098)]
		[Description("Should retrieve an asked-for provider, update it, and check that settings were saved.")]
		public void _003_Provider_Update()
		{
			var oMgr = new OAuthManager();

			//Variables used for changing the provider.
			string newCustom1 = "Test Custom 1";
			string newCustom2 = "Custom Property 2 - Saving nothing!";
			string newClientKey = Guid.NewGuid().ToString();
			string newClientSecret = Guid.NewGuid().ToString();

			//Pull the Gitlab provider.
			GlobalOAuthProvider prov1 = oMgr.Providers_RetrieveById(guidDummy);
			Assert.IsNotNull(prov1, "Unable to retrieve Dummy provider");
			Assert.IsTrue(prov1.Name.Equals(PROV_DM_NAME), "Wrong provider pulled! Name incorrect!");
			Assert.IsTrue(prov1.OAuthProviderId.Equals(guidDummy), "Wrong provider pulled! GUID incorrect!");

			//Update it!
			prov1.StartTracking();
			prov1.IsActive = true;
			prov1.Custom1 = newCustom1;
			prov1.Custom2 = newCustom2;
			prov1.ClientId = newClientKey;
			prov1.ClientSecret = newClientSecret;

			//Now save the provider.
			bool success = oMgr.Provider_Update(prov1, 1);

			if (success)
			{		
				//Now get the (modified) provider.
				prov1 = null;
				GlobalOAuthProvider prov2 = oMgr.Providers_RetrieveById(guidDummy);
				Assert.IsNotNull(prov2, "Unable to retrieve Dummy provider");
				Assert.IsTrue(prov2.Name.Equals(PROV_DM_NAME), "Wrong provider pulled! Name incorrect!");
				Assert.IsTrue(prov2.OAuthProviderId.Equals(guidDummy), "Wrong provider pulled! GUID incorrect!");
				Assert.IsNotNull(prov2.Custom1, "Custom Field 1 is null");
				Assert.IsNotNull(prov2.Custom2, "Custom Field 2 is null");
				Assert.IsTrue(prov2.Custom1.Equals(newCustom1), "Custom Field 1 Incorrect!");
				Assert.IsTrue(prov2.Custom2.Equals(newCustom2), "Custom Field 2 Incorrect!");
				Assert.IsTrue(prov2.IsActive, "IsActive was not set properly!");
				Assert.IsTrue(prov2.ClientSecret.Equals(newClientSecret), "Client Secret was not set properly!");
				Assert.IsTrue(prov2.ClientId.Equals(newClientKey), "Client Id was not set properly!");
			}
		}

		/// <summary>
		/// Deletes a given provider.
		/// </summary>
		[Test]
		[SpiraTestCase(2099)]
		[Description("Deletes a given provider.")]
		public void _004_Provider_Delete()
		{
			var oMgr = new OAuthManager();

			//Let us delete the GitLab Provider.
			bool ans = oMgr.Provider_Delete(guidGitlab);
			Assert.IsTrue(ans, "Got wrong responce back from Manager on Delete!");

			//Now let's pull ALL the providers.
			List<GlobalOAuthProvider> list1 = oMgr.Providers_RetrieveAll(false);
			Assert.IsFalse(list1.Any(p => p.OAuthProviderId.Equals(guidGitlab)), "Provider was returned in list of ALL providers!");

			//Let's try to pull JUST that provider.
			GlobalOAuthProvider prov1 = oMgr.Providers_RetrieveById(guidGitlab);
			Assert.IsNull(prov1, "Provider was returned specifically asking for it.");
		}

		/// <summary>
		/// Links a user up to a Provider, and verifies that the user is linked.
		/// </summary>
		[Test]
		[SpiraTestCase(2100)]
		[Description("Links a user up to a Provider, and verifies that the user is linked.")]
		public void _005_User_LinkToProvider()
		{
			var oMgr = new OAuthManager();

			//We are going to link Donna (#6) up to our Dummy Provider. 
			int userId = 6;
			string dUserKey = "DonnaKey";
			string aUserkey = "AdminKey";

			//Retrieve the provider first, check our numbers.
			GlobalOAuthProvider provDm = oMgr.Providers_RetrieveById(guidDummy, true);
			Assert.IsNotNull(provDm, "Unable to retrieve Dummy provider");
			Assert.IsTrue(provDm.Users.Count == 0, "Users assigned to this provider was not zero.");

			//Now link the user up, verify our info.
			User donna = oMgr.User_LinkToProvider(userId, guidDummy, dUserKey);
			Assert.IsNotNull(donna, "Did not get user back!");
			Assert.AreEqual(dUserKey, donna.OAuthAccessToken, "Provider Token was not saved properly.");
			Assert.AreEqual(guidDummy, donna.OAuthProviderId, "Provider GUID was not saved properly.");

			//Let us re-pull the provider, and make sure there is one user attached.
			provDm = oMgr.Providers_RetrieveById(guidDummy, true);
			Assert.IsNotNull(provDm, "Unable to retrieve Dummy provider");
			Assert.AreEqual(1, provDm.Users.Count, "User count was not 1!");
			Assert.IsTrue(provDm.Users.Any(u => u.UserId.Equals(userId)), "Donna was not in the list!");

			//Let's try to link the Administrator user.
			User admin = oMgr.User_LinkToProvider(1, guidDummy, aUserkey);
			Assert.IsNull(admin, "Could link root Admin account to OAuth!");
		}

		/// <summary>
		/// Unlinks a user from a provider, and verifies they are unlinked.
		/// </summary>
		[Test]
		[SpiraTestCase(2101)]
		[Description("Unlinks a user from a provider, and verifies they are unlinked.")]
		public void _006_User_UnlinkProvider()
		{
			var oMgr = new OAuthManager();

			//Set our constants, first.
			string newPass = "NewPassword";
			string newQuestion = "What is 10+10?";
			string newAnswer = "20";
			string newBadUsername = "Administrator";

			//Pull the user we want to remove from the provider.
			GlobalOAuthProvider provDm = oMgr.Providers_RetrieveById(guidDummy, true);
			Assert.IsNotNull(provDm, "Could not retrieve OAuth provider!");
			Assert.AreEqual(1, provDm.Users.Count, "User count was not 1!");
			User donnaUser = provDm.Users.SingleOrDefault();
			Assert.IsNotNull(donnaUser, "Could not retrieve Donna Harkness user object from provider!");

			//Create the provider. (Needed to call the provider's fnction, which in turn calls the Manager's function.
			var memProvider = new SpiraMembershipProvider();
			Assert.IsNotNull(memProvider, "Could not instantiate MemberShip Provider. Test cannot continue.");

			//Try unliking with an already-used UserName.
			bool try1 = memProvider.UnlinkAccountFromOAuth(6, newBadUsername, newPass, newQuestion, newAnswer);
			Assert.IsFalse(try1, "Got an incorrect return value from the Membership Provider. Bad Username.");

			//Now call the function to unlink.
			bool succ = memProvider.UnlinkAccountFromOAuth(6, donnaUser.UserName, newPass, newQuestion, newAnswer);
			Assert.IsTrue(succ, "Got an incorrect return value from the Membership Provider. Good Username.");

			//Let's make sure trying to unlink the admin accoutn fails.
			try
			{
				//Should throw an exception.
				bool ret = memProvider.UnlinkAccountFromOAuth(1, donnaUser.UserName, newPass, newQuestion, newAnswer);
				Assert.Fail("Unlinking Admin account did not throw exception.");
			}
			catch { }
		}

		[Test]
		[Description("")]
		public async void _007_Process_Callback()
		{
			var oMgr = new OAuthManager();

			string badStr = "Bad String. This will not decode properly.";
			byte[] machineKey = new byte[] { 122, 120, 56, 49, 83, 80, 69, 67, 84, 82, 85, 77 };

			//Here we need to generate our HTTPContext mnually, since we are NOT running this under IIS.
			HttpContext.Current = FakeHttpContext();
			var httpSes = HttpContext.Current.Session;

			//Now call without a cookie. We should get a null object back.
			UserLoginInfo testNull = await oMgr.Process_CallbackAsync();
			Assert.IsNull(testNull);

			//Now we need to create our 'fake' session. First, let's create one that has bad Base64 info in it.
			httpSes.Add(OAuthManager.AUTH_COOKNAME, badStr);
			UserLoginInfo testNull2 = await oMgr.Process_CallbackAsync();
			Assert.IsNull(testNull2);

			//Now make is a Base64 string, but not Encode it. 
			httpSes.Clear();
			httpSes.Add(OAuthManager.AUTH_COOKNAME, Convert.ToBase64String(Encoding.UTF8.GetBytes(badStr)));
			UserLoginInfo testNull3 = await oMgr.Process_CallbackAsync();
			Assert.IsNull(testNull3);

			//Now make is a Base64 string, Encode it, but not make it a valid AuthorizationContext object.
			httpSes.Clear();
			httpSes.Add(OAuthManager.AUTH_COOKNAME, Protect(Encoding.UTF8.GetBytes(badStr)));
			UserLoginInfo testNull4 = await oMgr.Process_CallbackAsync();
			Assert.IsNull(testNull4);
		}

		#region Private Utilities
		// On hold, as MachinKey varies 9strangely) between the Test DLL and the Business DLL.
		/*
		 * THIS SHOULD MATCH EXACTLY WHAT IS IN Oauthmanager.cs in Business!
		 */
		/// <summary>The locked key that provides encryption for our cookie.</summary>
		/// <remarks>Current value: zx81SPECTRUM [122 120 56 49 83 80 69 67 84 82 85 77]</remarks>
		private static readonly byte[] machineKey = new byte[] { 122, 120, 56, 49, 83, 80, 69, 67, 84, 82, 85, 77 };


		/*
		 * THIS SHOULD MATCH EXACTLY WHAT IS IN Oauthmanager.cs in Business!
		 */
		/// <summary>Encrpys the byte data.</summary>
		/// <param name="data">The data to encrypt.</param>
		string Protect(byte[] data)
		{
			if (data == null || data.Length == 0) return null;
			var value = MachineKey.Protect(data, Encoding.UTF8.GetString(machineKey));
			return Convert.ToBase64String(value);
		}

		/*
		 * THIS SHOULD MATCH EXACTLY WHAT IS IN Oauthmanager.cs in Business!
		 */
		/// <summary>Decode an encrypted string.</summary>
		/// <param name="value">The string of data to decode.</param>
		/// <returns>Unencrypoted bytes.</returns>
		byte[] Unprotect(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;
			var bytes = Convert.FromBase64String(value);
			return MachineKey.Unprotect(bytes, Encoding.UTF8.GetString(machineKey));
		}


		/// <summary>Generates a fake HttpContext that we can use for session testing.</summary>
		public static HttpContext FakeHttpContext()
		{
			var httpRequest = new HttpRequest("blah", "http://localhost/url", "");
			var stringWriter = new StringWriter();
			var httpResponse = new HttpResponse(stringWriter);
			var httpContext = new HttpContext(httpRequest, httpResponse);

			var sessionContainer = new HttpSessionStateContainer(
				"id",
				new SessionStateItemCollection(),
				new HttpStaticObjectsCollection(),
				10,
				true,
				HttpCookieMode.AutoDetect,
				SessionStateMode.InProc,
				false);

			httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
					BindingFlags.NonPublic | BindingFlags.Instance,
					null,
					CallingConventions.Standard,
					new[] { typeof(HttpSessionStateContainer) },
					null)
				.Invoke(new object[] { sessionContainer });

			return httpContext;
		}
		#endregion Private Utilities
	}
}
