using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>This class encapsulates all the data access functionality for reading and writing TaraVault properties in the system.</summary>
    public partial class VaultManager : ManagerBase
    {
        //Used for generating new passwords
        private static char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static char[] startingChars = new char[] { '<', '&' };

		//Regex for matching if we have a demo site or not
		private const string REGEX_DEMO_SITE_HOST = @"demo\-[A-Za-z]{2}\.spiraservice\.net";

		/// <summary>
		/// The two different types of vault
		/// </summary>
		public enum VaultTypeEnum
        {
            Subversion = 1,
            Git = 2
        }

        /// <summary>The class name, for logging.</summary>
        private const string CLASS_NAME = "Business.VaultManager::";

        /// <summary>Strings used to generate the URL.</summary>
        private const string TARA_URL = "https://{2}.taravault.net/{0}/{1}/{3}";
        private const string TARA_URL_GIT6 = "git6";
        private const string TARA_URL_SVN6 = "svn6";
        public const string TARA_USER_ADDR_SUFFIX = "taravault.net";
        public const string TARA_USER_ADDR_FORMAT = "{0}@{1}.taravault.net";

        public const string SOURCE_CODE_PROVIDER_TARA_VAULT = "TaraVaultProvider";

        #region Project Methods
        /// <summary>Retrieves the project with included TaraVaul information.</summary>
        /// <param name="projectId">The project ID to return.</param>
        /// <returns>A project. If the project is not set up for TaraVault, the TaraVaul object will be null.</returns>
        public Project Project_RetrieveWithTaraVault(long projectId)
        {
            const string METHOD_NAME = CLASS_NAME + "Project_RetrieveWithTaraVault()";
            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                //Create select command for retrieving the project record
                Project project;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from p in context.Projects.Include("TaraVault").Include("TaraVault.VaultType").Include("TaraVault.VaultUsers")
                                where p.ProjectId == projectId
                                select p;

                    project = query.FirstOrDefault();
                }

                //Throw an exception if the project record is not found
                if (project == null)
                {
                    throw new ArtifactNotExistsException("Project #" + projectId + " doesn't exist in the system");
                }

                //Return the project
                Logger.LogExitingEvent(METHOD_NAME);
                return project;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(METHOD_NAME, ex);
                throw;
            }
        }

        /// <summary>Deletes the TaraVault setup from the project.</summary>
        /// <param name="projectId">The ProjectId to remove the TaraVault setup info from.</param>
        public void Project_DeleteTaraVault(long projectId)
        {
            const string METHOD_NAME = CLASS_NAME + "Project_DeleteTaraVault()";
            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx ctx = new SpiraTestEntitiesEx())
                {
                    TaraVaultProject tvProj = ctx.TaraVaultProjects.Include(t => t.VaultUsers).SingleOrDefault(tvp => tvp.ProjectId == projectId);
                    if (tvProj != null)
                    {
                        //Check whether we're running hosted with live APIs
                        if (!Common.Global.Feature_TaraVault)
                        {
                            //Unit tests, so don't call API, just delete
                            ctx.TaraVaultProjects.DeleteObject(tvProj);
                            ctx.SaveChanges();
                            return;
                        }

                        //Remove users from the project.
                        foreach (TaraVaultUser user in tvProj.VaultUsers)
                        {
                            Tara_Project_RemoveUser(
                                ConfigurationSettings.Default.TaraVault_AccountId,
                                tvProj.VaultId,
                                Convert.ToInt64(user.VaultUserId));
                        }

                        //Call API to remove the project.
                        try
                        {
                            int i = 1;
                            Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.Project proj = Tara_Project_Delete(
                              ConfigurationSettings.Default.TaraVault_AccountId,
                            tvProj.VaultId);

                            while (proj != null && i < 10)
                            {
                                //Second call.
                                proj = Tara_Project_Delete(
                                  ConfigurationSettings.Default.TaraVault_AccountId,
                                tvProj.VaultId);
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting project. PL API");
                        }

                        //Verify project is gone before removing it from our database..
                        PlugIns.VersionControlPlugIn2.Project tvCheck =
                            Tara_Project_Retrieve(ConfigurationSettings.Default.TaraVault_AccountId, tvProj.ProjectId);
                        if (tvCheck == null)
                        {
                            //It really is, remove the TaraVault projects
                            ctx.TaraVaultProjects.DeleteObject(tvProj);
							ctx.SaveChanges();
                        }

						//Now remove the version control project
						var query = from vpc in ctx.VersionControlProjects
									where vpc.ProjectId == projectId && vpc.VersionControlSystem.Name == "TaraVaultProvider"
									select vpc;

						VersionControlProject versionControlProject = query.FirstOrDefault();
						if (versionControlProject != null)
						{
							ctx.VersionControlProjects.DeleteObject(versionControlProject);
							ctx.SaveChanges();
						}
					}
					else
                    {
                        Logger.LogTraceEvent(METHOD_NAME, "Cannot delete project #" + projectId + ", not a TV project.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(METHOD_NAME, ex, "Deleting TV Project");
                throw ex;
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Creates/Enables TaraVault for the given projectId.</summary>
        /// <param name="projectId">The project ID to create the TaraVault Project for.</param>
        /// <param name="projectType">The type of the project. (Subversion/Git)</param>
        /// <param name="taraName">The name of the project. (Max = 40 chars)</param>
        /// <returns>A complete Project object with TaraVault details.</returns>
        public TaraVaultProject Project_CreateUpdateTaraVault(int projectId, VaultTypeEnum projectType, string taraName)
        {
            const string METHOD_NAME = CLASS_NAME + "Project_CreateTaraVault()";
            Logger.LogEnteringEvent(METHOD_NAME);

            TaraVaultProject retValue = null;

            using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
            {
                //See if there is a database entry..
                retValue = ct.TaraVaultProjects.SingleOrDefault(tp => tp.ProjectId == projectId);

                //Create new one, if needed.
                bool isNew = false;
                if (retValue == null)
                {
                    retValue = new TaraVaultProject();
                    isNew = true;
                }

                //Save values..
                retValue.StartTracking();
                retValue.ProjectId = projectId;
                retValue.VaultId = 0;
                retValue.VaultTypeId = (int)projectType;
                retValue.Name = taraName;

                //Attach it, if needed.
                bool newCreate = false;
                if (isNew)
                {
                    try
                    {
                        //Call the TV API.. or make a mock-call if running on-premise (unit tests)
                        Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.Project proj;
                        if (!Common.Global.Feature_TaraVault)
                        {
                            proj = new PlugIns.VersionControlPlugIn2.Project();
                            proj.id = projectId;
                            proj.account_id = 1;
                            proj.account_name = "";
                            proj.name = taraName;
                            proj.repository_type = (projectType == VaultTypeEnum.Subversion) ? "svn" : "git";
                        }
                        else
                        {
                            //Running hosted
                            int projectTypeId = (int)projectType;
                            PlugIns.VersionControlPlugIn2.Project.RepositoryTypeEnum repositoryType = (PlugIns.VersionControlPlugIn2.Project.RepositoryTypeEnum)projectTypeId;
                            proj = Tara_Project_Create(
                                ConfigurationSettings.Default.TaraVault_AccountId,
                                retValue.Name,
                                (PlugIns.VersionControlPlugIn2.Project.RepositoryTypeEnum)projectType);
                        }

                        if (proj != null && proj.id > 0)
                        {
                            retValue.VaultId = proj.id;
                            ct.TaraVaultProjects.AddObject(retValue);
                            newCreate = true;

                            //Add reference in the Version Contrtol table for this project, if necessary.
                            VersionControlSystem sys = ct.VersionControlSystems.SingleOrDefault(vc => vc.Name.Equals(SOURCE_CODE_PROVIDER_TARA_VAULT));
                            if (sys != null)
                            {
                                //See if the project already exists, and insert if not
                                VersionControlProject vProj = ct.VersionControlProjects.FirstOrDefault(vp => vp.ProjectId == projectId && vp.VersionControlSystemId == sys.VersionControlSystemId);
                                if (vProj == null)
                                {
                                    vProj = new VersionControlProject();
                                    vProj.VersionControlSystemId = sys.VersionControlSystemId;
                                    vProj.IsActive = true;
                                    vProj.ProjectId = projectId;

                                    ct.VersionControlProjects.AddObject(vProj);
                                }
                                else
                                {
                                    //Make active
                                    vProj.StartTracking();
                                    vProj.IsActive = true;
                                }
                            }
                        }
                        else
                        {
                            Logger.LogErrorEvent(METHOD_NAME, "No project ID returned. PL API");
                            throw new InvalidOperationException("No project ID returned. PL API");
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorEvent(METHOD_NAME, ex, "Creating TaraProject. PL API");
                        throw;
                    }
                }

                //Save!
                if (!isNew || (isNew && newCreate))
                {
                    ct.SaveChanges();

                    //Add the Admin user if it's new.
                    if (isNew && newCreate)
                        User_AddToTaraVaultProject(1, projectId);
                }
            }

            return retValue;
        }

        /// <summary>Retrieves all projects that are defined with a TaraVault interface.</summary>
        /// <returns>A list of projects.</returns>
        public List<Project> Project_RetrieveTaraDefined()
        {
            const string METHOD_NAME = CLASS_NAME + "Project_RetrieveTaraDefined()";
            Logger.LogEnteringEvent(METHOD_NAME);

            List<Project> retList = new List<Project>();
            using (SpiraTestEntitiesEx ex = new SpiraTestEntitiesEx())
            {
                retList = ex.Projects
                            .Include(p => p.TaraVault)
                            .Include(p => p.TaraVault.VaultUsers)
                            .Include(p => p.TaraVault.VaultType)
                            .Where(p => p.TaraVault != null)
                            .ToList();
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return retList;
        }

        /// <summary>Returns the connection string for the given project. Null, if the project isn't set up.</summary>
        /// <returns></returns>
        public string Project_GetConnectionString(int projNum)
        {
            const string METHOD_NAME = CLASS_NAME + "Progect_GetConnectionString()";
            Logger.LogEnteringEvent(METHOD_NAME);

            string retValue = "";

            Project proj = Project_RetrieveWithTaraVault(projNum);

            if (proj != null)
                retValue = string.Format(TARA_URL,
                    ConfigurationSettings.Default.TaraVault_AccountName,
                    proj.TaraVault.Name,
                    ((proj.TaraVault.VaultTypeId == (int)VaultTypeEnum.Subversion) ? TARA_URL_SVN6 : TARA_URL_GIT6),
                    ((proj.TaraVault.VaultTypeId == (int)VaultTypeEnum.Subversion) ? "svn" : "git") + "/"
                    );

            Logger.LogExitingEvent(METHOD_NAME);
            return retValue;
        }
        #endregion Project Methods

        #region Account Methods
        /// <summary>Activates the account for this installation of SpiraTest.</summary>
        /// <returns>True if Activation is successful; False otherwise.</returns>
		/// <param name="deleteTestProvider">Should we delete the test provider (needed for unit tests)</param>
        public bool Account_Activate(bool deleteTestProvider = true)
        {
            const string METHOD_NAME = CLASS_NAME + "Account_Activate()";
            Logger.LogEnteringEvent(METHOD_NAME);

            bool retValue = false;

            string accountUrl = GetAccountNameFromURL(ConfigurationSettings.Default.General_WebServerUrl);

            //Call the TaraVault function..
            try
            {
                //Only call the TaraVault API if we are set to be running hosted mode
                //this lets us use it for unit testing
                PlugIns.VersionControlPlugIn2.Account acct;
                if (!Common.Global.Feature_TaraVault)
                {
                    //Unit Test - mock the API
                    acct = new PlugIns.VersionControlPlugIn2.Account();
                    acct.active = true;
                    acct.id = 1;
                    acct.name = "localhost";
                    acct.server = "testing.local";
                }
                else
                {
                    //Hosted - use real API
                    acct = Tara_Account_Create(accountUrl);
                }

                if (acct == null)
                {
                    throw new ApplicationException("The TaraVault account was not activated correctly (acct=null)");
                }

                //Save variables..
                ConfigurationSettings.Default.TaraVault_AccountName = acct.name;
                ConfigurationSettings.Default.TaraVault_HasAccount = true;
                ConfigurationSettings.Default.TaraVault_AccountId = acct.id;
                ConfigurationSettings.Default.TaraVault_RemoteServer = acct.server;
                ConfigurationSettings.Default.Save();
                retValue = (acct.id > 0);

                //Now create our Administrator user. Get the password. (Last 11 bytes.)
                string adminPass = GeneratePassword();
                string adminLogin = String.Format(TARA_USER_ADDR_FORMAT, "administrator", ConfigurationSettings.Default.TaraVault_AccountName);
                Logger.LogSuccessAuditEvent(METHOD_NAME, String.Format("Creating TV admin login with username '{0}' and password '{1}'", adminLogin, adminPass));
                User_CreateUpdateTaraAccount(User.UserSystemAdministrator, adminLogin, adminPass);

                //Clean up the version control tables..
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
					//Delete sample provider project info
                    List<VersionControlProject> versionControlProjects = context.VersionControlProjects.ToList();
					foreach (VersionControlProject versionControlProject in versionControlProjects)
					{
						//Clear the existing caches associated with the projects
						context.SourceCode_DeleteProjectCache(versionControlProject.VersionControlSystemId, versionControlProject.ProjectId);
						context.VersionControlProjects.DeleteObject(versionControlProject);
					}

					//Delete sample provider system info
                    List<VersionControlSystem> versionControlSystems = context.VersionControlSystems.ToList();
                    foreach (VersionControlSystem versionControlSystem in versionControlSystems)
                    {
						if (deleteTestProvider || (versionControlSystem.Name != SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME2 && versionControlSystem.Name != SourceCodeManager.TEST_VERSION_CONTROL_PROVIDER_NAME))
						{
							context.VersionControlSystems.DeleteObject(versionControlSystem);
						}
                    }

                    context.SaveChanges();
                }
                //Add the VersionControl system to the table.
                SourceCodeManager mgr = new SourceCodeManager();

				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "Source Code";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				//
				mgr.InsertSystem(
                    SOURCE_CODE_PROVIDER_TARA_VAULT,
                    "Built-in Provider for Taravault connections.",
                    true,
                    "",
                    adminLogin,
                    new SimpleAES().EncryptToString(adminPass),
                    "TV",
                    "", "", "", "", "", 1, adminSectionId, "Inserted Source Code");

                //Finally we update the user a second time, for some reason needed for the password to be usable
                User_CreateUpdateTaraAccount(User.UserSystemAdministrator, adminLogin, adminPass);
            }
            catch (Exception ex)
            {
                //Reset variables, in case.
                ConfigurationSettings.Default.TaraVault_AccountName = "";
                ConfigurationSettings.Default.TaraVault_HasAccount = false;
                ConfigurationSettings.Default.TaraVault_AccountId = 0;
                ConfigurationSettings.Default.TaraVault_RemoteServer = "";
                ConfigurationSettings.Default.Save();

                Logger.LogErrorEvent(METHOD_NAME, ex, "Creating account. PL API");
                throw ex;
            }

            return retValue;
        }

        /// <summary>
        /// Generates a random password for new users
        /// </summary>
        /// <remarks>Defaults to 20 characters, including 2 special chars</remarks>
        /// <returns>The password</returns>
        public static string GeneratePassword()
        {
            return GeneratePassword(12, 2);
        }

        private static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException("Membership_password_length_incorrect");
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException("numberOfNonAlphanumericCharacters");
            }

            string password;
            int index;
            byte[] buf;
            char[] cBuf;
            int count;

            do
            {
                buf = new byte[length];
                cBuf = new char[length];
                count = 0;

                (new RNGCryptoServiceProvider()).GetBytes(buf);

                for (int iter = 0; iter < length; iter++)
                {
                    int i = (int)(buf[iter] % 87);
                    if (i < 10)
                        cBuf[iter] = (char)('0' + i);
                    else if (i < 36)
                        cBuf[iter] = (char)('A' + i - 10);
                    else if (i < 62)
                        cBuf[iter] = (char)('a' + i - 36);
                    else
                    {
                        cBuf[iter] = punctuations[i - 62];
                        count++;
                    }
                }

                if (count < numberOfNonAlphanumericCharacters)
                {
                    int j, k;
                    Random rand = new Random();

                    for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                    {
                        do
                        {
                            k = rand.Next(0, length);
                        }
                        while (!Char.IsLetterOrDigit(cBuf[k]));

                        cBuf[k] = punctuations[rand.Next(0, punctuations.Length)];
                    }
                }

                password = new string(cBuf);
            }
            while (IsDangerousString(password, out index));

            return password;
        }

        internal static bool IsDangerousString(string s, out int matchIndex)
        {
            //bool inComment = false;
            matchIndex = 0;

            for (int i = 0; ;)
            {

                // Look for the start of one of our patterns
                int n = s.IndexOfAny(startingChars, i);

                // If not found, the string is safe
                if (n < 0) return false;

                // If it's the last char, it's safe
                if (n == s.Length - 1) return false;

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?') return true;
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. &#83;)
                        if (s[n + 1] == '#') return true;
                        break;
                }

                // Continue searching
                i = n + 1;
            }
        }

        private static bool IsAtoZ(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        /// <summary>Deactivate the account's TaraVault account.</summary>
        /// <returns>True if deactivation was successful. False, if not.</returns>
        public bool Account_Deactivate()
        {
            const string METHOD_NAME = CLASS_NAME + "Account_Deactivate()";
            Logger.LogEnteringEvent(METHOD_NAME);
            //The user wants to deactivate their TaraAccount account.

            //Need to deactivate/delete all projects, then remove the account.
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                //Go through all Projects that have a TaraVault setup.
                List<TaraVaultProject> projs = context.TaraVaultProjects.ToList<TaraVaultProject>();
                for (int i = 0; i < projs.Count; i++)
                {
                    this.Project_DeleteTaraVault(projs[i].ProjectId);
                }

                //Go through the users and remove their config.
                List<TaraVaultUser> users = context.TaraVaultUsers.ToList<TaraVaultUser>();
                for (int i = 0; i < users.Count; i++)
                {
                    context.TaraVaultUsers.DeleteObject(users[i]);
                }

                //Remove the source code entries.
                VersionControlSystem versionControlSystem = context.VersionControlSystems.Include(v => v.VersionControlProjects).FirstOrDefault(vc => vc.Name == SOURCE_CODE_PROVIDER_TARA_VAULT);
                if (versionControlSystem != null)
                {
                    versionControlSystem.StartTracking();
                    versionControlSystem.VersionControlProjects.Clear();
                    context.VersionControlSystems.DeleteObject(versionControlSystem);
                }

                context.SaveChanges();
            }

            //They're all deleted, now deactivate the account.

            //Save settings.
            ConfigurationSettings.Default.TaraVault_AccountName = "";
            ConfigurationSettings.Default.TaraVault_HasAccount = false;
            ConfigurationSettings.Default.TaraVault_UserLicense = 0;
            ConfigurationSettings.Default.TaraVault_AccountId = -1;
            ConfigurationSettings.Default.Save();
            return true;
        }
        #endregion Account Methods

        #region User Methods
        /// <summary>Returns the user's profile with declared TaraVault information.</summary>
        /// <param name="userId">The userId to pull.</param>
        /// <returns>A user object with TaraVault definition (if any). Null if the user is not found.</returns>
        public User User_RetrieveWithTaraVault(long userId)
        {
            const string METHOD_NAME = CLASS_NAME + "User_RetrieveWithTaraVault()";
            Logger.LogEnteringEvent(METHOD_NAME);

            User retUser = null;

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                retUser = context.Users
                            .Include("TaraVault")
                            .Include("TaraVault.VaultProject")
                            .Include("TaraVault.VaultProject.Project")
                            .Include("Profile")
                            .SingleOrDefault(u => u.UserId == userId);
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return retUser;
        }

        /// <summary>Retruieves a list of projects that this user is assigned to/active for.</summary>
        /// <param name="UserId">The user id to pull for.</param>
        /// <returns>A list of projects, or an empty list if none.</returns>
        public List<Project> User_RetrieveTaraProjectsForId(long userId)
        {
            const string METHOD_NAME = CLASS_NAME + "User_RetrieveTaraProjectsForId()";
            Logger.LogEnteringEvent(METHOD_NAME);

            List<Project> retList = new List<Project>();

            //Get the user info..
            User user = this.User_RetrieveWithTaraVault(userId);

            if (user != null)
            {
                if (user.TaraVault != null && user.TaraVault.VaultProject != null)
                {
                    //Loop through and get each project..
                    foreach (TaraVaultProject proj in user.TaraVault.VaultProject)
                    {
                        if (proj.Project != null)
                            retList.Add(proj.Project);
                    }
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return retList;
        }

        /// <summary>Adds the user to the project, if not already a user.</summary>
        /// <param name="userId">The user ID to add.</param>
        /// <param name="projectId">The project to add the user to.</param>
        public void User_AddToTaraVaultProject(int userId, int projectId)
        {
            const string METHOD_NAME = CLASS_NAME + "User_AddToTaraVaultProject()";
            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
            {
                //Get the project..
                TaraVaultProject proj = ct.TaraVaultProjects
                    .Include("VaultUsers")
                    .SingleOrDefault(p => p.ProjectId == projectId);

                if (proj != null)
                {
                    //See if the user is in there, first..
                    if (!proj.VaultUsers.Any(u => u.UserId == userId))
                    {
                        //User is not a member of the project.. get the TV user.
                        TaraVaultUser user = ct.TaraVaultUsers
                            .SingleOrDefault(u => u.UserId == userId);

                        //Now add it to the project.
                        if (user != null)
                        {
                            //Add it to TV.
                            if (!Common.Global.Feature_TaraVault)
                            {
                                //Running as a unit test so don't call API, just add
                                proj.StartTracking();
                                proj.VaultUsers.Add(user);
                                ct.SaveChanges();
                            }
                            else
                            {
                                try
                                {
                                    Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.User projAdded = Tara_Project_AddUser(
                                            ConfigurationSettings.Default.TaraVault_AccountId,
                                            proj.VaultId,
                                            Convert.ToInt32(user.VaultUserId));

                                    if (projAdded != null)
                                    {
                                        proj.StartTracking();
                                        proj.VaultUsers.Add(user);
                                        ct.SaveChanges();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogErrorEvent(METHOD_NAME, ex, "Cannot add user to project. PL API");
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogTraceEvent(METHOD_NAME, "User #" + userId + " is already a member of Project #" + projectId + ".");
                    }
                }
                else
                {
                    Logger.LogTraceEvent(METHOD_NAME, "Cannot add User #" + userId + " to Project #" + projectId + ", project is not enabled for TV.");
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Creates the user in TaraVault, or modifies it if the user already exists.</summary>
        /// <param name="password">The user's TaraVault password.</param>
        /// <param name="userId">The user ID to save the info for.</param>
        /// <param name="userLogin">The user's TaraLogin.</param>
        public void User_CreateUpdateTaraAccount(int userId, string userLogin, string password)
        {
            const string METHOD_NAME = CLASS_NAME + "User_CreateTaraAccount()";
            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx ex = new SpiraTestEntitiesEx())
            {
                //Flag on whether we need to delete newly created account. (For bad DB save..)
                long? newCreated = null;

                //Pull the user record.
                User userAcct = ex.Users
                    .Include("TaraVault")
                    .Include("Profile")
                    .SingleOrDefault(u => u.UserId == userId);

                //If we have an account already..
                if (userAcct.TaraVault == null || string.IsNullOrWhiteSpace(userAcct.TaraVault.VaultUserId))
                {
                    //Call API unless self hosted (running through unit tests)
                    Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.User userAPI = null;
                    if (!Common.Global.Feature_TaraVault)
                    {
                        //On-premise, running as part of the unit tests
                        //so just mock the object and use a simple email address format
                        userAPI = new PlugIns.VersionControlPlugIn2.User();
                        userAPI.active = true;
                        userAPI.email = userAPI.login = ((userLogin.EndsWith(TARA_USER_ADDR_SUFFIX)) ? userLogin : userLogin.Trim() + "@" + TARA_USER_ADDR_SUFFIX);
                        userAPI.firstname = userAcct.Profile.FirstName;
                        userAPI.lastname = userAcct.Profile.LastName;
                        userAPI.password = password;
                        userAPI.is_admin = false;
                        userAPI.id = userId;
                        newCreated = userAPI.id;
                    }
                    else
                    {
                        //Cloud-hosted so call real API
                        try
                        {
                            string taraVaultLogin;
                            if (userLogin.EndsWith(TARA_USER_ADDR_SUFFIX))
                            {
                                taraVaultLogin = userLogin.Trim();
                            }
                            else
                            {
                                //We create a fake email based on their login name and their instance name. Need to handle @ symbols
                                taraVaultLogin = String.Format(TARA_USER_ADDR_FORMAT, userLogin.Trim().Replace("@", "_"), ConfigurationSettings.Default.TaraVault_AccountName);
                            }
                            userAPI = new PlugIns.VersionControlPlugIn2.User();
                            userAPI.email = taraVaultLogin;
                            userAPI.login = taraVaultLogin;
                            userAPI.firstname = userAcct.Profile.FirstName;
                            userAPI.lastname = userAcct.Profile.LastName;
                            userAPI.password = password;
                            userAPI = Tara_User_Create(
                                ConfigurationSettings.Default.TaraVault_AccountId,
                                userAPI);
                            newCreated = userAPI.id;
                        }
                        catch (Exception ex2)
                        {
                            //Log error.
                            Logger.LogErrorEvent(METHOD_NAME, ex2, "Calling the PL API");
                            userAPI = null;
                        }
                    }

                    if (userAPI != null)
                    {
                        //Account was created successfully. Add it to our database.
                        TaraVaultUser userTar = new TaraVaultUser();
                        userTar.StartTracking();
                        userTar.UserId = userId;
                        userTar.VaultUserLogin = userAPI.login;
                        userTar.VaultUserId = userAPI.id.ToString();
                        userTar.Password = new SimpleAES().EncryptToString(password);
                        userTar.IsActive = true;
                        ex.TaraVaultUsers.AddObject(userTar);
                    }

                    try
                    {
                        //Save changes to the Spira DB
                        ex.SaveChanges();
                    }
                    catch (Exception ex3)
                    {
                        Logger.LogErrorEvent(METHOD_NAME, ex3, "Updating database.");
                        if (newCreated.HasValue)
                        {
                            //Remove the newly created user.
                            try
                            {
                                Tara_User_Delete(
                                    ConfigurationSettings.Default.TaraVault_AccountId,
                                    newCreated.Value);
                            }
                            catch (Exception ex4)
                            {
                                Logger.LogErrorEvent(METHOD_NAME, ex4, "Deleting newly created user. PL API");
                            }
                        }
                        throw;
                    }

                }
                else
                {
                    //Call API unless self hosted (running through unit tests)
                    if (Common.Global.Feature_TaraVault)
                    {
                        //Update the API
                        long taraVaultUserId = Int64.Parse(userAcct.TaraVault.VaultUserId);
                        Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.User userAPI = Tara_User_Retrieve(ConfigurationSettings.Default.TaraVault_AccountId, taraVaultUserId);
                        Tara_User_Update(
                                   ConfigurationSettings.Default.TaraVault_AccountId,
                                   userAPI,
                                   password);
                    }

                    //Update password.
                    userAcct.StartTracking();
                    userAcct.TaraVault.StartTracking();
                    userAcct.TaraVault.Password = new SimpleAES().EncryptToString(password);

                    //Save changes to the Spira DB
                    ex.SaveChanges();
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Removes the user's active TaraVault account.</summary>
        /// <param name="userid">The user ID to remove.</param>
        public void User_RemoveTaraAccount(int userId)
        {
            const string METHOD_NAME = CLASS_NAME + "User_RemoveTaraAccount()";
            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx ex = new SpiraTestEntitiesEx())
            {
                //Find the user..
                TaraVaultUser user = ex.TaraVaultUsers.SingleOrDefault(tvu => tvu.UserId == userId);
                if (user != null)
                {
                    //Call the TV API or mock call if running as part of the unit tests
                    if (Common.Global.Feature_TaraVault)
                    {
                        try
                        {
                            //We need to call it twice, first to deactiuvate, and again to actually delete.
                            Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.User delUser = Tara_User_Delete(ConfigurationSettings.Default.TaraVault_AccountId, Convert.ToInt64(user.VaultUserId));
                            int i = 0;
                            while (delUser != null && i < 10)
                            {
                                //Delete again..
                                delUser = Tara_User_Delete(
                                    ConfigurationSettings.Default.TaraVault_AccountId,
                                    Convert.ToInt64(user.VaultUserId));
                                i++;
                            }
                        }
                        catch (Exception ex2)
                        {
                            Logger.LogErrorEvent(METHOD_NAME, ex2, "Removing user. PL API");
                        }
                    }

                    try
                    {
                        //Now remove it from database.
                        ex.TaraVaultUsers.DeleteObject(user);
                        ex.SaveChanges();
                    }
                    catch (Exception ex1)
                    {
                        Logger.LogErrorEvent(METHOD_NAME, ex1, "Removing TV user from DB");
                    }
                }
                else
                {
                    //They're already removed.
                    Logger.LogTraceEvent(METHOD_NAME, "User " + userId + " not a member, cannot remove him.");
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Removes the user from the specified TaraVault project.</summary>
        /// <param name="userId">The userId to remove.</param>
        /// <param name="projectId">The project to remove the user from.</param>
        public void User_RemoveFromTaraVaultProject(int userId, int projectId)
        {
            const string METHOD_NAME = CLASS_NAME + "User_RemoveFromTaraVaultProject()";
            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
            {
                //Get the project..
                TaraVaultProject proj = ct.TaraVaultProjects
                    .Include("VaultUsers")
                    .SingleOrDefault(p => p.ProjectId == projectId);

                if (proj != null) //Make sure the project is activatd.
                {
                    TaraVaultUser user = proj.VaultUsers.SingleOrDefault(u => u.UserId == userId);
                    if (user != null) //And the user is an actual user.
                    {
                        //Call the TVAPI.. or make a mock-call if running on-premise (unit tests)
                        try
                        {
                            if (Common.Global.Feature_TaraVault)
                            {
                                Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2.User delUser = Tara_Project_RemoveUser(
                                    ConfigurationSettings.Default.TaraVault_AccountId,
                                    proj.VaultId,
                                    Convert.ToInt64(user.VaultUserId));
                            }

                            //User is a member of the project, remove him..
                            proj.StartTracking();
                            proj.VaultUsers.Remove(user);
                            ct.SaveChanges();
                        }
                        catch (Exception ex2)
                        {
                            Logger.LogErrorEvent(METHOD_NAME, ex2, "Removing user project. PL API");
                        }
                    }
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Returns all the users (regardless of project) that are active for Taravault.</summary>
        /// <returns>A list of users with their TaraVaul properties set.</returns>
        public List<User> User_RetrieveAllTVActive()
        {
            const string METHOD_NAME = CLASS_NAME + "User_RetrieveAllTVActive()";
            Logger.LogEnteringEvent(METHOD_NAME);

            List<User> retList = new List<User>();

            using (SpiraTestEntitiesEx ex = new SpiraTestEntitiesEx())
            {
                retList = ex.Users
                            .Include("TaraVault")
                            .Include("TaraVault.VaultProject")
                            .Include("TaraVault.VaultProject.Project")
                            .Include("Profile")
                            .Where(u => u.TaraVault != null)
                            .ToList();
            }

            Logger.LogExitingEvent(METHOD_NAME);
            return retList;

        }
        #endregion User Methods

        #region Private Methods
        private string GetAccountNameFromURL(string inUrl)
        {
            string retValue;

            //Get the service URL..
            Uri sysUrl = new Uri(inUrl);

            //See if it's a demo account, first.
            if (Regex.IsMatch(sysUrl.Host, REGEX_DEMO_SITE_HOST, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
            {
                //In this case, we need to get the first folder from the URL path.
                retValue = sysUrl.AbsolutePath.Trim('/').Trim('\\').Trim();
            }
            else
            {
                if (sysUrl.Host.Contains(".spiraservice.net"))
                    retValue = sysUrl.Host.Replace(".spiraservice.net", "").Trim();
                else
                    retValue = sysUrl.Host.Replace(".", "").Trim();
            }

            return retValue;
        }
        #endregion
    }
}
