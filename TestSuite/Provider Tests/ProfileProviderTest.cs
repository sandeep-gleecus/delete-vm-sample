using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Profile;

using Inflectra.SpiraTest.Web.Classes;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;
using System.Collections.Specialized;
using System.Reflection;
using System.Configuration;
using System.Web.Security;

namespace Inflectra.SpiraTest.TestSuite.Provider_Tests
{
    /// <summary>
    /// Tests the Spira ASP.NET Profile Provider
    /// </summary>
    [
    TestFixture,
    SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
    ]
    public class ProfileProviderTest
    {
        private static int userId1;
        private static string userName1;

        [TestFixtureSetUp]
        public void Init()
        {
            //We need to programatically add the Membership provider for the unit tests because we don't
            //use the Web.Config file when running the unit tests
            bool providerLoaded = false;
            if (Membership.Providers.Count > 0)
            {
                foreach (MembershipProvider provider in Membership.Providers)
                {
                    if (provider is SpiraMembershipProvider)
                    {
                        providerLoaded = true;
                    }
                }
            }

            if (!providerLoaded)
            {
                //Get a handle to our custom Spira membership provider, some of the functions require this
                SpiraMembershipProvider membershipProvider = new SpiraMembershipProvider();
                NameValueCollection configuration = new NameValueCollection();
                configuration.Add("enablePasswordRetrieval", "false");
                configuration.Add("enablePasswordReset", "true");
                configuration.Add("requiresQuestionAndAnswer", "true");
                configuration.Add("requiresUniqueEmail", "false");
                configuration.Add("passwordFormat", "Hashed");
                membershipProvider.Initialize("SpiraMembershipProvider", configuration);
                membershipProvider.AddTo(Membership.Providers, true);

            }
            //Verify the default provider
            Assert.AreEqual(typeof(SpiraMembershipProvider), Membership.Provider.GetType());

            //We need to programatically add the Profile provider for the unit tests because we don't
            //use the Web.Config file when running the unit tests

            //Get a handle to our custom Spira membership provider, some of the functions require this
            SpiraProfileProvider profileProvider = new SpiraProfileProvider();
            profileProvider.Initialize("SpiraProfileProvider", new NameValueCollection());
            profileProvider.AddTo(ProfileManager.Providers, true);

            //Use reflection to set some of the properties
            Type t = typeof(ProfileManager);
            //ProfileManager.AutomaticSaveEnabled = false;
            var field = t.GetField("s_AutomaticSaveEnabled", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, false);

            //Verify the default provider
            Assert.AreEqual(profileProvider.GetType(), ProfileManager.Provider.GetType());

            //Instantiate the properties collection
            SettingsPropertyCollection profileProperties = new SettingsPropertyCollection();
            profileProperties.Add(new SettingsProperty("FirstName", typeof(string), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("LastName", typeof(string), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("MiddleInitial", typeof(string), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("Department", typeof(string), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("IsAdmin", typeof(bool), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("IsEmailEnabled", typeof(bool), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("LastOpenedProjectId", typeof(int?), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("Timezone", typeof(string), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("IsBusy", typeof(bool), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));
            profileProperties.Add(new SettingsProperty("IsAway", typeof(bool), profileProvider, false, null, SettingsSerializeAs.ProviderSpecific, null, false, false));

            //Access the profile base once to let the standard initialization occur
            int dummy = ProfileBase.Properties.Count;

            //Now add our properties using reflection
            Type t2 = typeof(ProfileBase);
            field = t2.GetField("s_Properties", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, profileProperties);
            Assert.AreEqual(profileProperties.Count, ProfileBase.Properties.Count);
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            //Delete any new users created
            Membership.DeleteUser(userName1, true);
        }

        /// <summary>
        /// Tests that you can create a new profile for a user and make changes to it
        /// </summary>
        [Test]
        [SpiraTestCase(951)]
        public void _01_CreateUpdateUserProfile()
        {
            //First create a new user
            MembershipCreateStatus status;
            MembershipUser newUser = Membership.CreateUser("newuser", "newpassword123", "newuser@spiratest.com", "What is 1+1?", "2", false, out status);
            Assert.AreEqual(MembershipCreateStatus.Success, status);
            userId1 = (int)newUser.ProviderUserKey;
            userName1 = newUser.UserName;

            //Now first verify that the user has no profile
            ProfileEx profile = new ProfileEx("newuser");
            Assert.AreEqual(null, profile.FirstName);
            Assert.AreEqual(null, profile.LastName);
            Assert.AreEqual(null, profile.MiddleInitial);
            Assert.AreEqual(null, profile.Department);
            Assert.AreEqual(false, profile.IsAdmin);
            Assert.AreEqual(false, profile.IsEmailEnabled);
            Assert.AreEqual(null, profile.LastOpenedProjectId);
            Assert.AreEqual(null, profile.Timezone);

            //Now lets set some values on the profile
            profile.FirstName = "Harold";
            profile.LastName = "Godswin";
            profile.MiddleInitial = "T";
            profile.Department = "Saxony";
            profile.IsAdmin = true;
            profile.IsEmailEnabled = true;
            profile.LastOpenedProjectId = 1;
            profile.Timezone = "Eastern Time (US)";
            profile.Save();

            //Verify the values
            Assert.AreEqual("Harold", profile.FirstName);
            Assert.AreEqual("Godswin", profile.LastName);
            Assert.AreEqual("T", profile.MiddleInitial);
            Assert.AreEqual("Saxony", profile.Department);
            Assert.AreEqual(true, profile.IsAdmin);
            Assert.AreEqual(true, profile.IsEmailEnabled);
            Assert.AreEqual(1, profile.LastOpenedProjectId);
            Assert.AreEqual("Eastern Time (US)", profile.Timezone);

            //Now lets make some changes
            profile.FirstName = "Harald";
            profile.LastName = "Hardrada";
            profile.MiddleInitial = null;
            profile.Department = "Norway";
            profile.IsAdmin = false;
            profile.IsEmailEnabled = true;
            profile.LastOpenedProjectId = 2;
            profile.Timezone = "Pacific Time (US)";
            profile.Save();

            //Verify the values
            Assert.AreEqual("Harald", profile.FirstName);
            Assert.AreEqual("Hardrada", profile.LastName);
            Assert.AreEqual(null, profile.MiddleInitial);
            Assert.AreEqual("Norway", profile.Department);
            Assert.AreEqual(false, profile.IsAdmin);
            Assert.AreEqual(true, profile.IsEmailEnabled);
            Assert.AreEqual(2, profile.LastOpenedProjectId);
            Assert.AreEqual("Pacific Time (US)", profile.Timezone);
        }

        /// <summary>
        /// Tests that you can retrieve various profiles in the system
        /// </summary>
        [Test]
        [SpiraTestCase(952)]
        public void _02_RetrieveProfiles()
        {
            //We just verify the counts for the various functions because they are not actually used in SpiraTest
            //because we just use the more powerful UserManager directly for these types of query
            ProfileInfoCollection profiles = ProfileManager.GetAllProfiles(ProfileAuthenticationOption.All);
            Assert.AreEqual(14, profiles.Count);
            profiles = ProfileManager.GetAllInactiveProfiles(ProfileAuthenticationOption.All, DateTime.UtcNow.AddDays(-1));
            Assert.IsTrue(profiles.Count > 0);
            Assert.IsTrue(profiles.Count <= 14);
            int count = ProfileManager.GetNumberOfProfiles(ProfileAuthenticationOption.All);
            Assert.AreEqual(13, count);
            count = ProfileManager.GetNumberOfInactiveProfiles(ProfileAuthenticationOption.All, DateTime.UtcNow.AddDays(-1));
            Assert.IsTrue(count > 0);
            Assert.IsTrue(count <= 14);
            profiles = ProfileManager.FindProfilesByUserName(ProfileAuthenticationOption.All, "newuser");
            Assert.AreEqual(1, profiles.Count);
            profiles = ProfileManager.FindInactiveProfilesByUserName(ProfileAuthenticationOption.All, "newuser", DateTime.UtcNow.AddDays(-1));
            Assert.AreEqual(0, profiles.Count);
        }
    }
}
