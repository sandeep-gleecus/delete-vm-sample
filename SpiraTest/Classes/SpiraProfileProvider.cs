using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Profile;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// This profile provider integrates the SpiraTest data model with the ASP.NET profile provider service
    /// for authentication
    /// </summary>
    public class SpiraProfileProvider : ProfileProvider
    {
        //Constants
        public const string PROVIDER_NAME = "SpiraProfileProvider";
        public const string APPLICATION_NAME = "SpiraTest";

        #region ProfileProvider Members

        /// <summary>
        /// Initializes the provider
        /// </summary>
        /// <param name="name">The provider name</param>
        /// <param name="config">The config parameters</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrEmpty(name))
            {
                name = PROVIDER_NAME;
            }
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", PROVIDER_NAME);
            }
            base.Initialize(name, config);

            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrEmpty(attribUnrecognized))
                {
                    throw new ProviderException("Unrecognized config attribute:" + attribUnrecognized);
                }
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            if (collection == null || collection.Count < 1 || context == null)
                return svc;

            string username = (string)context["UserName"];
            if (String.IsNullOrEmpty(username))
                return svc;

            try
            {
                UserManager userManager = new UserManager();
                userManager.GetProfileData(collection, svc, username);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(this.GetType().Name, exception);
                throw;
            }

            return svc;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            string username = (string)context["UserName"];
            bool userIsAuthenticated = (bool)context["IsAuthenticated"];

            if (username == null || username.Length < 1 || collection.Count < 1)
            {
                return;
            }

            try
            {
                UserManager userManager = new UserManager();
                userManager.SetProfileData(collection, username, userIsAuthenticated);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(this.GetType().Name, exception);
                throw;
            }
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentNullException("profiles");
            }

            if (profiles.Count < 1)
            {
                throw new ArgumentException("Profiles collection is empty");
            }

            string[] usernames = new string[profiles.Count];

            int iter = 0;
            foreach (ProfileInfo profile in profiles)
            {
                usernames[iter++] = profile.UserName;
            }

            return DeleteProfiles(usernames);
        }

        public override int DeleteProfiles(string[] usernames)
        {
            if (usernames == null || usernames.Length < 1)
            {
                return 0;
            }

            try
            {
                UserManager userManager = new UserManager();
                int numProfilesDeleted = 0;
                foreach (string username in usernames)
                {
                    bool success = userManager.DeleteProfile(username);
                    if (success)
                    {
                        numProfilesDeleted++;
                    }
                }
                return numProfilesDeleted;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes inactive profiles
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <returns>The number deleted</returns>
        /// <remarks>This provider does not support anonymous user profiles</remarks>
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                UserManager userManager = new UserManager();
                int numProfilesDeleted = userManager.DeleteInactiveProfiles(userInactiveSinceDate);
                return numProfilesDeleted;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the count of inactive profiles
        /// </summary>
        /// <param name="authenticationOption"></param>
        /// <param name="userInactiveSinceDate"></param>
        /// <returns></returns>
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            try
            {
                UserManager userManager = new UserManager();
                List<User> inactiveUsers = userManager.GetInactiveProfiles(userInactiveSinceDate);
                if (inactiveUsers == null)
                {
                    return 0;
                }
                return inactiveUsers.Count;
            }
            catch
            {
                throw;
            }
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                UserManager userManager = new UserManager();
                List<User> users = userManager.GetProfiles(pageIndex, pageSize, out totalRecords);
                if (users == null)
                {
                    return null;
                }

                //Convert the users
                return GetProfilesForUsers(users);
            }
            catch
            {
                throw;
            }
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                UserManager userManager = new UserManager();
                List<User> inactiveUsers = userManager.GetInactiveProfiles(userInactiveSinceDate.ToUniversalTime(), pageIndex, pageSize, out totalRecords);
                if (inactiveUsers == null)
                {
                    return null;
                }

                //Convert the users
                return GetProfilesForUsers(inactiveUsers);
            }
            catch
            {
                throw;
            }
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                UserManager userManager = new UserManager();
                List<User> users = userManager.FindProfilesByLogin(usernameToMatch, pageIndex, pageSize, out totalRecords);
                if (users == null)
                {
                    return null;
                }

                //Convert the users
                return GetProfilesForUsers(users);
            }
            catch
            {
                throw;
            }
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            try
            {
                UserManager userManager = new UserManager();
                List<User> users = userManager.FindInactiveProfilesByLogin(usernameToMatch, userInactiveSinceDate.ToUniversalTime(), pageIndex, pageSize, out totalRecords);
                if (users == null)
                {
                    return null;
                }

                //Convert the users
                return GetProfilesForUsers(users);
            }
            catch
            {
                throw;
            }
        }
  
        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a list of users to a list of profile info objects
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        protected ProfileInfoCollection GetProfilesForUsers(List<User> users)
        {
            ProfileInfoCollection profiles = new ProfileInfoCollection();
            if (users != null)
            {
                foreach (User user in users)
                {
                    if (user.Profile != null)
                    {
                        //All profiles are not-anonymous
                        const bool isAnonymous = false;
                        DateTime lastActivityDate = DateTime.UtcNow;
                        if (user.LastActivityDate.HasValue)
                        {
                            lastActivityDate = user.LastActivityDate.Value;
                        }
                        DateTime lastUpdateDate = DateTime.UtcNow;
                        profiles.Add(new ProfileInfo(user.UserName, isAnonymous, lastActivityDate, lastUpdateDate, 0));
                    }
                }
            }
            return profiles;
        }

        #endregion

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
    }
}
