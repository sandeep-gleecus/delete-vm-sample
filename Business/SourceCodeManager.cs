using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.PlugIns;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	public partial class SourceCodeManager : ManagerBase, IDisposable
	{
		#region Private Vars
		/// <summary>The default Build ID for V1 providers, that don't have Branch support built-in.</summary>
		private const string DEFAULT_BRANCH = "Trunk";

		/// <summary>
		/// The temporary branch name to display while we retrieve the actual branches
		/// </summary>
		public const string DEFAULT_BRANCH_V2 = "(none)";

		/// <summary>Flag for storing whether our Disposed has been called.</summary>
		private bool isDisposed = false;

		/// <summary>The name of the Repository we're interfacing with.</summary>
		private string repositoryName = "";

		/// <summary>The project ID that we are loaded for.</summary>
		private int projectId;

		/// <summary>
		/// The version control system we are loaded for
		/// </summary>
		private int versionControlSystemId;

		/// <summary>The class's name, for logging.</summary>
		private const string CLASS_NAME = "Business.SourceCodeManager::";

		/// <summary>The name of the 'test' control provider, which is used for testing purposes.</summary>
		public const string TEST_VERSION_CONTROL_PROVIDER_NAME = "TestVersionControlProvider";
		/// <summary>The name of the v2 'test' control provider.</summary>
		public const string TEST_VERSION_CONTROL_PROVIDER_NAME2 = "TestVersionControlProvider2";
		public const int TEST_VERSION_CONTROL_PROVIDER_ID = 1;

		/// <summary>The domain that the interface loads into. Saved for unloading during dispose.</summary>
		private AppDomain interfaceDomain = null;

		/// <summary>The library that we loaded.</summary>
		private object interfaceLibrary = null;

		/// <summary>The unique token given back from the source-code provider.</summary>
		private object interfaceToken = null;

		private SourceCodeSettings interfaceSettings = null;

		/// <summary>True if the library supports IVersionControlPlugin2 funtions.</summary>
		private bool isV2 = false;

		/// <summary>The cached data, if any, read from the disk.</summary>
		private Dictionary<string, SourceCodeCache> cachedData;

		/// <summary>
		/// To improve performance we keep a copy of the XML cache in memory for each project that has been accessed.
		/// It was taking 2 minutes to create the cache each time!
		/// </summary>
		private static Dictionary<int, Dictionary<string, SourceCodeCache>> projectCaches = new Dictionary<int, Dictionary<string, SourceCodeCache>>();

		/// <summary>The cache update process.</summary>
		private static Thread cacheUpdate = null;
		#endregion Private Vars

		#region Public Vars

		//Certain constants used when displaying revisions in conjunction with real Spira artifacts
		/// <summary>The name of a Revision/Commit</summary>
		public const string REVISION_ARTIFACT_TYPE_NAME = "Commit";

		//Field names for Filtering..
		public const string FIELD_NAME = "Name";
		public const string FIELD_SIZE = "Size";
		public const string FIELD_AUTHOR = "AuthorName";
		public const string FIELD_ACTION = "Action";
		public const string FIELD_COMMIT = "Revision";
		public const string FIELD_LASTUPDATED = "LastUpdated";
		public const string FIELD_MESSAGE = "Message";
		public const string FIELD_CONTENT_CHANGED = "ContentChanged";
		public const string FIELD_PROP_CHANGED = "PropertiesChanged";
		public const string FIELD_UPDATE_DATE = "UpdateDate";
		public const string FIELD_BRANCH_KEY = "BranchKey";

		//Field values for user setting data..
		//Settings Collections
		public const string SETTING_MAIN_COMMITS_FILTER = "SourceCodeList.CommitList.Filter";
		public const string SETTING_MAIN_COMMITS_OTHER = "SourceCodeList.CommitList";
		public const string SETTING_MAIN_FILES_FILTER = "SourceCodeList.FileList.Filter";
		public const string SETTING_MAIN_FILES_OTHER = "SourceCodeList.FileList";
		public const string SETTING_MAIN_BUILD_COMMITS_FILTER = "BuildDetails.Commits.Filters";
		public const string SETTING_MAIN_BUILD_COMMITS_OTHER = "BuildDetails.Commits.General";
		public const string SETTING_MAIN_FILE_DETAILS_COMMITS_FILTER = "SourceCodeFileDetails.Commits.Filters";
		public const string SETTING_MAIN_FILE_DETAILS_COMMITS_OTHER = "SourceCodeFileDetails.Commits.General";
		public const string SETTING_MAIN_PULLREQUEST_COMMITS_FILTER = "PullRequest.Commits.Filters";
		public const string SETTING_MAIN_PULLREQUEST_COMMITS_OTHER = "PullRequest.Commits.General";

		//Settings Keys
		public const string SETTING_KEY_SORTFIELD = "SortField";
		public const string SETTING_KEY_SORTASC = "SortAsc";
		public const string SETTING_KEY_ROWSPAGE = "NumberRowsPerPage";
		public const string SETTING_KEY_CURRPAGE = "CurrentPage";
		public const string SETTING_KEY_SELECTEDFOLDER = "SelectedFolder";
		public const string SETTING_KEY_SELECTEDBRANCH = "SelectedBranch";

		#endregion Public Vars

		#region Constructors & Setup
		/// <summary>Constructor method for class. Used in Administration for changing project and system settings.</summary>
		public SourceCodeManager()
			: base()
		{
			const string METHOD_NAME = CLASS_NAME + ".ctor()";
			Logger.LogEnteringEvent(METHOD_NAME);

			cachedData = new Dictionary<string, SourceCodeCache>();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Loads the manager, and the library and cache for the specified project id, if any.</summary>
		/// <param name="projectId">The Project's # to load up.</param>
		/// <param name="dontInitializeCache">Pass True if you will be clearing the cache</param>
		public SourceCodeManager(int projectId, bool dontInitializeCache = false)
			: base()
		{
			const string METHOD_NAME = CLASS_NAME + ".ctor(int)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Setup our cached data..
			this.projectId = projectId;
			cachedData = new Dictionary<string, SourceCodeCache>();

			//First we need to retrieve the appropriate plug-in for this project
			List<VersionControlProject> versionControlProjects = this.RetrieveProjectSettings(projectId);

			//Get the first active VersionControlProject.
			VersionControlProject vcProj = versionControlProjects.FirstOrDefault(vcp => vcp.IsActive == true);

			//Throw and exception if we don't have at least one provider
			if (vcProj == null)
				throw new SourceCodeProviderLoadingException(
					string.Format(GlobalResources.General.SourceCode_NoProvidersEnabled, projectId));
			else
			{
				try
				{
					//Get the system settings for the specified Project.
					VersionControlSystem vcSys = this.RetrieveSystemById(vcProj.VersionControlSystemId);

					//Store the system id
					this.versionControlSystemId = vcSys.VersionControlSystemId;

					//Get the credentials & parameters.
					NetworkCredential loginCreds = new NetworkCredential();
					Dictionary<string, string> parameters = new Dictionary<string, string>();
					if (vcSys.Domain != null && vcSys.Domain.Equals("TV"))
					{
						//TaraVault - Use the system user/password always. Decrypt password.
						loginCreds.UserName = vcSys.Login;
						loginCreds.Password = new SimpleAES().DecryptString(vcSys.Password);

						//Set extra parameters.
						using (SpiraTestEntitiesEx ex = new SpiraTestEntitiesEx())
						{
							TaraVaultProject tproj = ex.TaraVaultProjects.FirstOrDefault(tv => tv.ProjectId == projectId);
							if (tproj != null)
							{
								parameters.Add("Spira.AccountName", ConfigurationSettings.Default.TaraVault_AccountName);
								parameters.Add("Spira.AccountId", ConfigurationSettings.Default.TaraVault_AccountId.ToString());
								parameters.Add("Spira.AccountServer", ConfigurationSettings.Default.TaraVault_RemoteServer);
								parameters.Add("Spira.ProjectName", tproj.Name);
								parameters.Add("Spira.ProjectId", tproj.VaultId.ToString());
								parameters.Add("Spira.ProjectType", tproj.VaultTypeId.ToString());
								parameters.Add("Spira.EnableTrace", ((string.IsNullOrWhiteSpace(vcProj.Custom01) ? vcSys.Custom01 : vcProj.Custom01)));

								//For subversion we may need to use a real SVN login/password so we use User 1 (administrator)
								//This is so that we can call native SVN functions if the default TV API is unavailable
								if (tproj.VaultTypeId == (int)VaultManager.VaultTypeEnum.Subversion)
								{
									User tvUser = new VaultManager().User_RetrieveWithTaraVault(User.UserSystemAdministrator);
									parameters.Add("Subversion.Login", tvUser.TaraVault.VaultUserLogin);
									parameters.Add("Subversion.Password", new SimpleAES().DecryptString(tvUser.TaraVault.Password));
								}

								//For Git we may need to use a real Git login/password so we use User 1 (administrator)
								//This is so that we can call native Git functions if the default TV API is unavailable
								if (tproj.VaultTypeId == (int)VaultManager.VaultTypeEnum.Git)
								{
									User tvUser = new VaultManager().User_RetrieveWithTaraVault(User.UserSystemAdministrator);
									parameters.Add("Git.Login", tvUser.TaraVault.VaultUserLogin);
									parameters.Add("Git.Password", new SimpleAES().DecryptString(tvUser.TaraVault.Password));
								}
							}
						}
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(vcProj.Login))
						{
							loginCreds.UserName = vcProj.Login;
							loginCreds.Password = vcProj.Password;
						}
						else
						{
							loginCreds.UserName = vcSys.Login;
							loginCreds.Password = vcSys.Password;
						}
					}
					//Set the domain if specified
					if (!String.IsNullOrEmpty(vcProj.Domain))
					{
						loginCreds.Domain = vcProj.Domain;
					}
					else if (!String.IsNullOrEmpty(vcSys.Domain))
					{
						loginCreds.Domain = vcSys.Domain;
					}

					//Generate the settings object.
					SourceCodeSettings settings = new SourceCodeSettings();

					//Set the settings values.
					settings.Connection = ((string.IsNullOrWhiteSpace(vcProj.ConnectionString) ? vcSys.ConnectionString : vcProj.ConnectionString));
					settings.Custom01 = ((string.IsNullOrWhiteSpace(vcProj.Custom01) ? vcSys.Custom01 : vcProj.Custom01));
					settings.Custom02 = ((string.IsNullOrWhiteSpace(vcProj.Custom02) ? vcSys.Custom02 : vcProj.Custom02));
					settings.Custom03 = ((string.IsNullOrWhiteSpace(vcProj.Custom03) ? vcSys.Custom03 : vcProj.Custom03));
					settings.Custom04 = ((string.IsNullOrWhiteSpace(vcProj.Custom04) ? vcSys.Custom04 : vcProj.Custom04));
					settings.Custom05 = ((string.IsNullOrWhiteSpace(vcProj.Custom05) ? vcSys.Custom05 : vcProj.Custom05));
					settings.EventLog = Logger.ApplicationEventLog;
					settings.ProviderName = vcSys.Name;
					settings.Credentials = loginCreds;
					settings.Parameters = parameters;

					//Store the connection info as the repository name
					this.repositoryName = settings.ProviderName;
					this.interfaceSettings = settings;

					//Create & Initialize the provider..
					this.LoadProvider(settings.ProviderName);
					bool succ = this.InitializeProvider(settings);

					if (!succ)
					{
						//If we got this far, we should have been thrown an exception.
						//In case we haven't, make one on here and throw it.
						throw new VersionControlGeneralException(string.Format(GlobalResources.General.SourceCode_CouldNotIni, settings.ProviderName));
					}

					//If we will be subsequently deleting the cache, we don't want any updates running now
					if (dontInitializeCache)
					{
						return;
					}

					//If the cache is already refreshing, don't fire again
					this.InitializeXMLCache();
					if (!cacheUpdateRunning)
					{
						if (this.isV2)
						{
							//Get a date from one of the caches..
							if (this.cachedData.Keys.Count > 0)
							{
								string branch = this.cachedData.Keys.ElementAt(0);
								DateTime cacheDate = this.cachedData[branch].CacheDate;

								//Get the newest commit across all branches
								DateTime revDate = new DateTime(0);
								string revName = "";
								using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
								{
									var query = from c in context.SourceCodeCommits
												where c.VersionControlProject.ProjectId == projectId
												orderby c.UpdateDate descending, c.RevisionId
												select c;

									SourceCodeCommit mostRecentCommit = query.FirstOrDefault();
									if (mostRecentCommit != null)
									{
										revDate = mostRecentCommit.UpdateDate;
										revName = mostRecentCommit.Revisionkey;
									}
								}
							}
						}
						else
						{
							//Check that a new day has started from the last cache refresh..
							if (this.cachedData.ContainsKey(DEFAULT_BRANCH))
							{
								DateTime cacheDate = this.cachedData[DEFAULT_BRANCH].CacheDate;
								if (cacheDate < DateTime.UtcNow && cacheDate.Day != DateTime.UtcNow.Day)
								{
									Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to age time.");
									this.LaunchCacheRefresh();
								}
							}
							else
							{
								Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
								this.LaunchCacheRefresh();
							}
						}
					}
				}
				catch (VersionControlAuthenticationException exception)
				{
					//Rethrow as a provider error
					Logger.LogErrorEvent(
						METHOD_NAME,
						exception,
						GlobalResources.General.ResourceManager.GetString("SourceCode_CouldNotLogin", CultureInfo.InvariantCulture),
						Logger.EVENT_CATEGORY_VERSION_CONTROL);
					throw new SourceCodeProviderAuthenticationException(GlobalResources.General.SourceCode_CouldNotLogin, exception);
				}
				catch (VersionControlGeneralException exception)
				{
					//Rethrow as a provider error
					Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
					throw new SourceCodeProviderLoadingException(String.Format(GlobalResources.General.SourceCode_NoProvidersLoaded, projectId));
				}
				catch (Exception ex)
				{
					//Log and pass through
					Logger.LogErrorEvent(METHOD_NAME, ex, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Loads the version control provider</summary>
		/// <param name="name">The name of the assembly the provider uses</param>
		/// <returns>A reference to the instantialted provider or throws an exception otherwise</returns>
		private void LoadProvider(string name)
		{
			const string METHOD_NAME = CLASS_NAME + "LoadProvider()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Assemble the path to the DLL..
				string plugInFileName = name;
				plugInFileName = Path.GetDirectoryName(
					new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
				//Change the directory, if needed..
				if (name == TEST_VERSION_CONTROL_PROVIDER_NAME || name == TEST_VERSION_CONTROL_PROVIDER_NAME2)
					plugInFileName = Path.GetFullPath(plugInFileName + @"\PlugInInterfaces.dll");
				else
					plugInFileName = Path.GetFullPath(plugInFileName + @"\..\VersionControl\" + name + ".dll");

				//Create new AppDomain.. At this point, it should ALWAYS be null.
				if (this.interfaceDomain != null)
					throw new Exception("Assert in loading Version Control Library! (1)");
				else
					this.interfaceDomain = AppDomain.CreateDomain(name);
				if (this.interfaceDomain == null)
					throw new Exception("Assert in loading Version Control Library! (2)");

				//Now load the DLL, make sure our interface is empty as well..
				if (this.interfaceLibrary != null)
					throw new Exception("Assert in loading Version Control Library! (3)");

				//Load the types that this DLL materializes.
				Type[] types = Assembly.LoadFrom(plugInFileName).GetExportedTypes();

				//Loop through each one and find one that matches.
				//First look for V2
				this.interfaceLibrary = null;
				foreach (Type type in types)
				{
					if (typeof(IVersionControlPlugIn2).IsAssignableFrom(type) && type.IsClass)
					{
						//Check that it's a test provider..
						if (name == TEST_VERSION_CONTROL_PROVIDER_NAME2)
						{
							if (type.Name == name)
							{
								this.interfaceLibrary = (IVersionControlPlugIn2)Activator.CreateInstance(type);
								this.isV2 = true;
								break;
							}
						}
						else
						{
							this.interfaceLibrary = (IVersionControlPlugIn2)Activator.CreateInstance(type);
							this.isV2 = true;
							break;
						}
					}
				}

				//Fallback to V1
				if (this.interfaceLibrary == null)
				{
					foreach (Type type in types)
					{
						if (typeof(IVersionControlPlugIn).IsAssignableFrom(type) && type.IsClass)
						{
							//Check that it's a test provider..
							if (name == TEST_VERSION_CONTROL_PROVIDER_NAME)
							{
								if (type.Name == name)
								{
									this.interfaceLibrary = (IVersionControlPlugIn)Activator.CreateInstance(type);
									this.isV2 = false;
									break;
								}
							}
							else
							{
								this.interfaceLibrary = (IVersionControlPlugIn)Activator.CreateInstance(type);
								this.isV2 = false;
								break;
							}
						}
					}
				}

				if (this.interfaceLibrary == null)
					throw new SourceCodeProviderLoadingException(
						string.Format(GlobalResources.General.SouceCode_UnableToLoadDLL, name));

				//Check that the selected one can be V2 as well..
				this.isV2 = (this.interfaceLibrary is IVersionControlPlugIn2);
			}
			catch (Exception exception)
			{
				//Rethrow as a loading exception
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException(exception.Message);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Calls the provider's Initialize() function.</summary>
		/// <param name="settings">The settings to pass to it for initialization.</param>
		/// <returns>True if Initialize was successful, false otherwise.</returns>
		private bool InitializeProvider(SourceCodeSettings settings)
		{
			const string METHOD_NAME = CLASS_NAME + "InitializeProvider()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool retStatus = false;

			//See if we have a V1 or V2 plugin
			if (this.isV2)
			{
				this.interfaceToken = ((IVersionControlPlugIn2)(this.interfaceLibrary)).Initialize(
					settings.Connection,
					settings.Credentials,
					settings.Parameters,
					settings.EventLog,
					ConfigurationSettings.Default.Cache_Folder,
					settings.Custom01,
					settings.Custom02,
					settings.Custom03,
					settings.Custom04,
					settings.Custom05);
			}
			else
			{
				this.interfaceToken = ((IVersionControlPlugIn)this.interfaceLibrary).Initialize(
					settings.Connection,
					settings.Credentials,
					settings.Parameters,
					settings.EventLog,
					settings.Custom01,
					settings.Custom02,
					settings.Custom03,
					settings.Custom04,
					settings.Custom05);
			}

			if (this.interfaceToken != null)
				retStatus = true;

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;

		}

		/// <summary>Load up the XML data (cache) for the given provider.</summary>
		private void InitializeXMLCache()
		{
			const string METHOD_NAME = CLASS_NAME + "InitializeXMLCache()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Initialize the setting, first..
			if (string.IsNullOrWhiteSpace(ConfigurationSettings.Default.Cache_Folder))
			{
				ConfigurationSettings.Default.Cache_Folder = Common.Global.CACHE_FOLDERPATH;
				ConfigurationSettings.Default.Save();
			}
			if (ConfigurationSettings.Default.Cache_RefreshTime < 1)
			{
				//Minimum value is 1 minute for V2 providers
				ConfigurationSettings.Default.Cache_RefreshTime = 1;
				ConfigurationSettings.Default.Save();
			}
			if (ConfigurationSettings.Default.Cache_RefreshTime_v1 < 60)
			{
				//Minimum value is 1 hours (60 minutes) for V1 providers
				ConfigurationSettings.Default.Cache_RefreshTime_v1 = 60;
				ConfigurationSettings.Default.Save();
			}
			string cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);

			/* We need to look in the directory, and see if we have any files that match
			 * the project and name of the provider, as is: "[ProviderName]_[ProjectNum]_[BranchKey].xml"
			 * Where [ProviderName] is the name of the DLL ('SVNProvider', etc.), [ProjectNum]
			 * is the number of the project ('1', '34'), and [BranchKey] is the key string to the branch (made filename-safe).
			 */

			//Get a list of all the files that meet our specifications..
			string wildcardMatch = this.repositoryName + "_" + this.projectId + "_*.cache";
			Logger.LogTraceEvent(METHOD_NAME, "Searching cache directory: " + cacheFolder + "\\" + wildcardMatch);
			if (!Directory.Exists(cacheFolder))
			{
				Directory.CreateDirectory(cacheFolder);
			}

			string[] files = Directory.GetFiles(cacheFolder, wildcardMatch);

			if (files.Length == 0)
			{
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present cache file.");
				this.LaunchCacheRefresh();
			}
			else
			{
				//For performance reasons, see if we have a copy of the cache in memory already
				if (projectCaches.ContainsKey(this.projectId))
				{
					this.cachedData = projectCaches[this.projectId];
				}
				else
				{
					//Loop through each file and try to import it.
					foreach (string file in files)
					{
						try
						{
							//The class to hold our cache..
							SourceCodeCache cache = null;

							//Now create the deserializer..
							XmlSerializer serializer = new XmlSerializer(typeof(SourceCodeCache));

							StreamReader reader = new StreamReader(file);
							XmlTextReader xtr = new XmlTextReader(reader);
							cache = (SourceCodeCache)serializer.Deserialize(xtr);
							reader.Close();

							//If we got an object, add it to our cache..
							if (cache != null)
							{
								if (!this.cachedData.ContainsKey(cache.BranchKey))
								{
									this.cachedData.Add(cache.BranchKey, cache);
								}
							}
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD_NAME, ex, "Trying to deserialize '" + file + "'.");
							this.cachedData = new Dictionary<string, SourceCodeCache>();
						}
					}

					//Now store in memory
					projectCaches[this.projectId] = this.cachedData;
				}
			}

			//Now we need to see if we need an update.
			DateTime lastRunDate = new DateTime((long)0);
			foreach (KeyValuePair<string, SourceCodeCache> kvpCache in this.cachedData)
			{
				//Check static variable, first..
				string branchCacheName = this.projectId + "_" + Regex.Replace(kvpCache.Value.BranchKey, @"\W|_", "_", RegexOptions.CultureInvariant);
				if (SourceCodeManager.cache_dates.ContainsKey(branchCacheName) && SourceCodeManager.cache_dates[branchCacheName] > lastRunDate)
					lastRunDate = SourceCodeManager.cache_dates[branchCacheName];
				else if (kvpCache.Value.CacheDate > lastRunDate)
					lastRunDate = kvpCache.Value.CacheDate;
			}
			//Add our desired Cache Update delay date.
			//We cannot easily determine if we're using a V1 or V2 cache here, so we'll just look for specific known
			//V2 providers and assume V1 otherwise
			int cacheTime;
			if (this.repositoryName.ToLowerInvariant() == "gitprovider" ||
				this.repositoryName.ToLowerInvariant() == "subversionprovider" ||
				this.repositoryName.ToLowerInvariant() == "taravaultprovider" ||
				this.repositoryName.ToLowerInvariant() == "testversioncontrolprovider2")
			{
				//V2
				cacheTime = ConfigurationSettings.Default.Cache_RefreshTime;
			}
			else
			{
				//V1
				cacheTime = ConfigurationSettings.Default.Cache_RefreshTime_v1;
			}

			DateTime cacheValidUntilDate = lastRunDate.AddMinutes(cacheTime);
			//Now see if we need to do a cache reload.
			if (this.cachedData.Count < 1 || cacheValidUntilDate < DateTime.UtcNow)
			{
				Logger.LogWarningEvent(METHOD_NAME, String.Format("Launching refresh of cache. Cache Count: {0}; Last Update Date: {1:MM/dd/yy H:mm:ss zzz}", this.cachedData.Count, lastRunDate));
				this.LaunchCacheRefresh();
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Clears the cache files completely, and then does a refresh of the cache files
		/// </summary>
		public void ClearCacheAndRefresh(int? id = null, string name = null, int? userId = null, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS_NAME + "ClearCacheAndRefresh()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//We cannot clear the cache if an update is running.
			if (SourceCodeManager.cacheUpdate == null || !SourceCodeManager.cacheUpdate.IsAlive)
			{
				//Delete the existing cache in foreground thread
				ClearCache(true);

				//Create the thread.. and do the refresh
				SourceCodeManager.cacheUpdate = new Thread(new ThreadStart(this.RefreshCache));

				Logger.LogTraceEvent(METHOD_NAME, "Starting thread..");
				SourceCodeManager.cacheUpdate.Start();

				AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
				string adminSectionName = "Source Code";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				//Add a changeset to mark it as deleted.
				new AdminAuditManager().LogDeletion1((int)userId, (int)id, name, adminSectionId, "Cleared Cache", DateTime.UtcNow, ArtifactTypeEnum.SourceCode, "VersionControlSystemId");
			}
			else
			{
				throw new ApplicationException(GlobalResources.Messages.SourceCode_CannotDeleteCacheBecauseUpdateRunning);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Launches the background code to refresh the cache files.</summary>
		public void LaunchCacheRefresh()
		{
			const string METHOD_NAME = CLASS_NAME + "LaunchCacheRefresh()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (SourceCodeManager.cacheUpdate == null || !SourceCodeManager.cacheUpdate.IsAlive)
			{
				//Create the thread..
				SourceCodeManager.cacheUpdate = new Thread(new ThreadStart(this.RefreshCache));

				Logger.LogTraceEvent(METHOD_NAME, "Starting thread..");
				SourceCodeManager.cacheUpdate.Start();
			}
			else
			{
				Logger.LogInformationalEvent(METHOD_NAME, "Refresh thread already active. Skipping refresh call.");
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion Constructors & Setup

		#region Other Cache Functions

		/// <summary>Clears the cache that's held in memory.</summary>
		/// <param name="deleteFiles">Should we also physically delete the cache files [default=false]</param>
		private void ClearCache(bool deleteFiles = false)
		{
			//Clear the variables
			this.cachedData.Clear();
			this.cachedData = new Dictionary<string, SourceCodeCache>();
			if (projectCaches.ContainsKey(this.projectId))
			{
				projectCaches.Remove(this.projectId);
			}

			if (deleteFiles)
			{
				//Now delete the cache files
				String cacheFolder = Path.Combine(ConfigurationSettings.Default.Cache_Folder, folderName);
				string wildcardMatch = this.repositoryName + "_" + this.projectId + "_*.cache";
				string[] files = Directory.GetFiles(cacheFolder, wildcardMatch);
				foreach (string file in files)
				{
					File.Delete(file);
				}

				//Also we need to delete the branches and revisions from the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.SourceCode_DeleteProjectCache(this.versionControlSystemId, this.projectId);
				}
			}
		}

		#endregion Other Cache Functions

		#region Public Properties
		/// <summary>The name of the repository, if loaded. Null/Empty if not loaded.</summary>
		public string RepositoryName
		{
			get
			{
				return this.repositoryName;
			}
		}

		/// <summary>
		/// The name of the current connection (if loaded)
		/// </summary>
		public string Connection
		{
			get
			{
				if (this.interfaceSettings == null)
				{
					return null;
				}
				return this.interfaceSettings.Connection;
			}
		}

		#endregion Public Properties

		#region Plugin Administration Methods
		/// <summary>Retrieves a data containing the list of existing version control providers.</summary>
		/// <returns>List of defined VersionControl Systems. Used in local installs only, not used with TaraVault.</returns>
		public List<VersionControlSystem> RetrieveSystems()
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveSystems()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<VersionControlSystem> systems;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the list of version control providers
					var query = from v in context.VersionControlSystems
								orderby v.Name, v.VersionControlSystemId
								select v;

					systems = query.ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return systems;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					exception,
					"Loading list of available VersionControlProviders.",
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a single version control provider record by its Id</summary>
		/// <param name="versionControlSystemId">The id of the version control provider</param>
		/// <returns>The requested VersionControlSystem, or null if one is not found.</returns>
		public VersionControlSystem RetrieveSystemById(int versionControlSystemId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveSystemById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			VersionControlSystem retSys = null;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the list of version control providers
					var query = from v in context.VersionControlSystems
								where v.VersionControlSystemId == versionControlSystemId
								select v;

					retSys = query.FirstOrDefault();
				}

				if (retSys == null)
					Logger.LogTraceEvent(METHOD_NAME, "VersionControlSystem id#" + versionControlSystemId + " not found.");
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Retrieving System #" + versionControlSystemId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retSys;
		}

		/// <summary>Retrieves a dataset containing all the version control providers for a project</summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>version control dataset</returns>
		/// <param name="includeSystem">Should we include the system info</param>
		/// <param name="onlyActive">Do we only want active project settings</param>
		public List<VersionControlProject> RetrieveProjectSettings(int projectId, bool onlyActive = true, bool includeSystem = false)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveProjectSettings()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<VersionControlProject> retList = new List<VersionControlProject>();

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project settings
					ObjectQuery<VersionControlProject> set = context.VersionControlProjects;
					if (includeSystem)
					{
						set = set.Include(v => v.VersionControlSystem);
					}
					var query = from v in set
								where v.ProjectId == projectId && v.IsActive
								orderby v.VersionControlSystemId
								select v;

					retList = query.ToList();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Pulling VersionControlSystems for Project #" + projectId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>
		/// Determines if there are any active providers for the current project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>TRUE if there is at least one active project</returns>
		public bool IsActiveProvidersForProject(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "IsActiveProvidersForProject()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool isActiveProvider = false;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for checking the projects
					var query = from v in context.VersionControlProjects
								where v.ProjectId == projectId && v.IsActive && v.VersionControlSystem.IsActive
								select v;

					isActiveProvider = (query.Count() > 0);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return isActiveProvider;
		}

		/// <summary>Retrieves a list of all projects using a system</summary>
		/// <param name="versionControlSystemId">The id of the system</param>
		/// <returns>version control dataset</returns>
		public List<VersionControlProject> RetrieveProjectsForSystem(int versionControlSystemId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveProjectSettings()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<VersionControlProject> retList = new List<VersionControlProject>();

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the projects
					var query = from v in context.VersionControlProjects
									.Include(v => v.Project)
								where v.VersionControlSystemId == versionControlSystemId && v.IsActive
								orderby v.ProjectId
								select v;

					retList = query.ToList();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Pulling VersionControlSystems for Project #" + projectId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves the record containing the project settings for a specific version control provider and project</summary>
		/// <param name="versionControlSystemId">The id of the version control provider</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>version control list</returns>
		/// <remarks>Returns active and inactive entries</remarks>
		public VersionControlProject RetrieveProjectSettings(int versionControlSystemId, int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveProjectSettings()";
			Logger.LogEnteringEvent(METHOD_NAME);

			VersionControlProject retSys = null;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project settings
					var query = from v in context.VersionControlProjects
								where v.ProjectId == projectId && v.VersionControlSystemId == versionControlSystemId
								select v;

					retSys = query.FirstOrDefault();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Pulling VersionControlProject for System #" + versionControlSystemId + " & Project #" + projectId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retSys;
		}

		/// <summary>Inserts a new data sync plug-in entry into the system</summary>
		/// <param name="name">The name of the provider (must match the assembly name)</param>
		/// <param name="description">The description of the provider</param>
		/// <param name="connection">The connection information (usually a URI)</param>
		/// <param name="login">The login to the external system</param>
		/// <param name="password">The password for the external system</param>
		/// <param name="domain">Does the login belong to a Windows domain (optional)</param>
		/// <param name="custom01">Plug-in custom data (optional)</param>
		/// <param name="custom02">Plug-in custom data (optional)</param>
		/// <param name="custom03">Plug-in custom data (optional)</param>
		/// <param name="custom04">Plug-in custom data (optional)</param>
		/// <param name="custom05">Plug-in custom data (optional)</param>
		/// <param name="active">Is the provider active</param>
		/// <returns>The id of the new provider</returns>
		public int InsertSystem(string name, string description, bool active, string connection, string login, string password, string domain, string custom01, string custom02, string custom03, string custom04, string custom05, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS_NAME + "InsertSystem()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int retValue = 0;
			string newValue = "";
			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out record with data for new provider
					VersionControlSystem versionControlSystem = new VersionControlSystem();
					versionControlSystem.Name = name;
					versionControlSystem.Description = description;
					versionControlSystem.IsActive = active;
					versionControlSystem.ConnectionString = connection;
					versionControlSystem.Login = login;
					versionControlSystem.Password = password;
					versionControlSystem.Domain = domain;
					versionControlSystem.Custom01 = custom01;
					versionControlSystem.Custom02 = custom02;
					versionControlSystem.Custom03 = custom03;
					versionControlSystem.Custom04 = custom04;
					versionControlSystem.Custom05 = custom05;

					//Save the record and capture the new ID
					context.VersionControlSystems.AddObject(versionControlSystem);
					context.SaveChanges();
					retValue = versionControlSystem.VersionControlSystemId;
					newValue = versionControlSystem.Name;
				}
				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				details.NEW_VALUE = newValue;

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), retValue, action, details, DateTime.UtcNow, ArtifactTypeEnum.SourceCode, "VersionControlSystemId");
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Creating new VersionControlSystem.",
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Inserts a new project settings record for a version control provider</summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="versionControlSystemId">The id of the provider</param>
		/// <param name="connection">The connection information (optional)</param>
		/// <param name="login">The login to the external system (optional)</param>
		/// <param name="password">The password for the external system (optional)</param>
		/// <param name="domain">Does the login belong to a Windows domain (optional)</param>
		/// <param name="custom01">Plug-in custom data (optional)</param>
		/// <param name="custom02">Plug-in custom data (optional)</param>
		/// <param name="custom03">Plug-in custom data (optional)</param>
		/// <param name="custom04">Plug-in custom data (optional)</param>
		/// <param name="custom05">Plug-in custom data (optional)</param>
		/// <param name="active">Is the provider active for this project</param>
		public void InsertProjectSettings(int versionControlSystemId, int projectId, bool active, string connection, string login, string password, string domain, string custom01, string custom02, string custom03, string custom04, string custom05, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = CLASS_NAME + "InsertProjectSettings()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new provider
					VersionControlProject versionControlProject = new VersionControlProject();
					versionControlProject.VersionControlSystemId = versionControlSystemId;
					versionControlProject.ProjectId = projectId;
					versionControlProject.IsActive = active;
					versionControlProject.ConnectionString = connection;
					versionControlProject.Login = login;
					versionControlProject.Password = password;
					versionControlProject.Domain = domain;
					versionControlProject.Custom01 = custom01;
					versionControlProject.Custom02 = custom02;
					versionControlProject.Custom03 = custom03;
					versionControlProject.Custom04 = custom04;
					versionControlProject.Custom05 = custom05;

					//Actually perform the insert into the table
					context.VersionControlProjects.AddObject(versionControlProject);
					context.AdminSaveChanges(userId, versionControlSystemId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Inserting project settings for VersionControlSystem #" + versionControlSystemId + ", Project #" + projectId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			//Clear any cached data..
			this.ClearCache();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Updates a version control system record</summary>
		/// <param name="versionControlSystem">The data to be persisted</param>
		public void UpdateSystem(VersionControlSystem versionControlSystem, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateSystem()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes and persist
					context.VersionControlSystems.ApplyChanges(versionControlSystem);
					context.AdminSaveChanges(userId, versionControlSystem.VersionControlSystemId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Updating VersionControlSystem #" + versionControlSystem.VersionControlSystemId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				Logger.Flush();
				throw;
			}

			//Clear cached data..
			this.ClearCache();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Updates a version control project settings record</summary>
		/// <param name="versionControlProject">The data to be persisted</param>
		public void UpdateProjectSettings(VersionControlProject versionControlProject, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateProjectSettings()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes and persist
					context.VersionControlProjects.ApplyChanges(versionControlProject);
					context.AdminSaveChanges(userId, versionControlProject.VersionControlSystemId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"While updating project settings for Project #" + versionControlProject.ProjectId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Deletes a version control provider together with all its project-specific settings</summary>
		/// <param name="versionControlSystemId">The id of the version control provider</param>
		public void DeleteSystem(int versionControlSystemId, int? userId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "DeleteSystem()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve the system
					var query = from v in context.VersionControlSystems
									.Include(s => s.VersionControlProjects)
								where v.VersionControlSystemId == versionControlSystemId
								select v;

					//Make sure we have an existing entry
					VersionControlSystem versionControlSystem = query.FirstOrDefault();
					if (versionControlSystem != null)
					{
						//The delete will include any associated project records
						context.VersionControlSystems.DeleteObject(versionControlSystem);
						context.SaveChanges();

						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "Source Code";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)userId, versionControlSystem.VersionControlSystemId, versionControlSystem.Name, adminSectionId, "SourceCode Deleted", DateTime.UtcNow, ArtifactTypeEnum.SourceCode, "VersionControlSystemId");
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(
					METHOD_NAME,
					ex,
					"Deleting VersionControlSystem #" + versionControlSystemId,
					Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}
		#endregion Plugin Administration Methods

		#region Plugin TaraVault Admin Methods
		#endregion Plugin TaraVault Admin Methods

		#region Data Business Functions
		/// <summary>Tests the connection to a specific source code version control provider</summary>
		/// <param name="settings">Settings object.</param>
		/// <returns>True if connection succeeds</returns>
		public bool TestConnection(SourceCodeSettings settings)
		{
			const string METHOD_NAME = CLASS_NAME + "TestConnection()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool retValue = false;

			try
			{
				//Load up the DLL..
				this.LoadProvider(settings.ProviderName);

				//Initialize the DLL..
				object token;
				if (this.isV2)
				{
					retValue = ((IVersionControlPlugIn2)this.interfaceLibrary).TestConnection(
						settings.Connection,
						settings.Credentials,
						settings.Parameters,
						settings.EventLog,
						ConfigurationSettings.Default.Cache_Folder,
						settings.Custom01,
						settings.Custom02,
						settings.Custom03,
						settings.Custom04,
						settings.Custom05);
				}
				else
				{
					token = ((IVersionControlPlugIn)this.interfaceLibrary).Initialize(
						settings.Connection,
						settings.Credentials,
						settings.Parameters,
						settings.EventLog,
						settings.Custom01,
						settings.Custom02,
						settings.Custom03,
						settings.Custom04,
						settings.Custom05);

					retValue = (token != null);
				}
			}
			catch (Exception exception)
			{
				//Log the exception
				Logger.LogErrorEvent(METHOD_NAME, exception, "Unable to connect to '" + settings.ProviderName + "'", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				retValue = false;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Gets the external key of a revision for the passed in internal id</summary>
		/// <param name="revisionId">The numeric id assigned to this revision</param>
		/// <returns>The key for the revsion (e.g. name, number, etc.)</returns>
		public string GetRevisionKeyForId(int revisionId)
		{
			const string METHOD_NAME = CLASS_NAME + "GetRevisionKeyForId()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			string retValue = null;

			//Get it from the database
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var query = from s in context.SourceCodeCommits
							where s.RevisionId == revisionId && s.ProjectId == this.projectId
							select s;

				SourceCodeCommit commit = query.FirstOrDefault();
				if (commit != null)
					retValue = commit.Revisionkey;
			}

			if (string.IsNullOrWhiteSpace(retValue))
			{
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present RevisionId.");
				this.LaunchCacheRefresh();
				throw new SourceCodeCacheInvalidException();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Returns the file key for the assocation with the specified ID</summary>
		/// <param name="artifactSourceCodeId">The id of the source code artifact file association record</param>
		/// <returns>The file key</returns>
		public string GetFileKeyForAssociation(int artifactSourceCodeFileId)
		{
			const string METHOD_NAME = CLASS_NAME + "GetFileKeyForAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			string retValue = null;

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					ArtifactSourceCodeFile artifactSourceCodeFile =
						ct.ArtifactSourceCodeFiles.Where(
							a => a.ArtifactSourceCodeFileId == artifactSourceCodeFileId
						).FirstOrDefault();

					if (artifactSourceCodeFile == null)
						throw new ArtifactNotExistsException(string.Format("Unable to find source code file association with id = {0}", artifactSourceCodeFileId));

					retValue = artifactSourceCodeFile.FileKey;
				}
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Gets the external key of a file for the passed in internal id</summary>
		/// <param name="fileId">The numeric id assigned to this file</param>
		/// <returns>The key for the file (e.g. path or URI)</returns>
		public string GetFileKeyForId(int fileId, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "GetFileKeyForId()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			string retValue = null;

			//Get it from XML..
			if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
			{
				SourceCodeFile file = this.cachedData[branchKey].Files.Where(f => f.FileId == fileId).FirstOrDefault();
				if (file != null)
					retValue = file.FileKey;
			}

			//Throw error if it doesn't exists.
			if (string.IsNullOrEmpty(retValue))
			{
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FileId.");
				this.LaunchCacheRefresh();
				throw new SourceCodeCacheInvalidException();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Returns the id of the parent to the current folder</summary>
		/// <param name="folderId">The id of the current folder</param>
		/// <returns>The id of the parent folder (or null if root)</returns>
		public int? RetrieveParentFolder(int folderId, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveParentFolder()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			int? retValue = null;

			try
			{
				//See if it exists in the XML..
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the item's folder record, first.
					SourceCodeFolder folder = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderId == folderId);
					if (folder != null)
					{
						if (folder.ParentFolderId.HasValue)
						{
							//Now get the item's parent folder..
							SourceCodeFolder parent = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderId == folder.ParentFolderId);
							if (parent != null)
								retValue = parent.FolderId;
							else
							{
								Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present ParentFolderId.");
								this.LaunchCacheRefresh();
								throw new SourceCodeCacheInvalidException("Could not find specified parent folder.");
							}
						}
						else
						{
							//The item is already in the root - return nothing.
							retValue = null;
						}
					}
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FolderId.");
						this.LaunchCacheRefresh();
						throw new SourceCodeCacheInvalidException("Could not find specified folder.");
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					this.LaunchCacheRefresh();
					throw new SourceCodeCacheInvalidException("Branch asked for that did not exist.");
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Returns the list of parent folders that the file is contained in</summary>
		/// <param name="fileKey">The id of the file</param>
		/// <param name="branchKey">The branch to look in</param>
		/// <returns>The list of folders starting with the lowest and ending with the root</returns>
		public List<SourceCodeFolder> RetrieveParentFolders(string fileKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveParentFolders()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			List<SourceCodeFolder> folders = new List<SourceCodeFolder>();

			try
			{
				//See if it exists in the XML..
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the item's folder record, first.
					SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey == fileKey);
					if (file != null && file.ParentFolderId.HasValue)
					{
						PopulateParentFolder(folders, this.cachedData[branchKey], file.ParentFolderId.Value);
					}
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FileKey.");
						this.LaunchCacheRefresh();
						throw new SourceCodeCacheInvalidException("Could not find specified file.");
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					this.LaunchCacheRefresh();
					throw new SourceCodeCacheInvalidException("Branch asked for that did not exist.");
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return folders;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>
		/// Populates a folder and gets its parents recursively
		/// </summary>
		/// <param name="branch">The branch we're looking in</param>
		/// <param name="folders">The list of folders to return</param>
		/// <param name="parentFolderId">The id of the folder</param>
		protected void PopulateParentFolder(List<SourceCodeFolder> folders, SourceCodeCache branch, int parentFolderId)
		{
			SourceCodeFolder folder = branch.Folders.FirstOrDefault(f => f.FolderId == parentFolderId);
			if (folder != null)
			{
				folders.Add(folder);
				if (folder.ParentFolderId.HasValue)
				{
					PopulateParentFolder(folders, branch, folder.ParentFolderId.Value);
				}
			}
		}

		/// <summary>Retrieves a list of revisions made to a specific file in the repository</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="fileKey">The filekey for the file</param>
		/// <param name="branchKey">The key of the branch to pull data from.</param>
		/// <param name="filters">Hashtable of filters to apply to the data returned.</param>
		/// <param name="numRows">The number of rows to return.</param>
		/// <param name="sortAsc">Whether to sort ascending or not.</param>
		/// <param name="sortProperty">The property/field to sort our items on.</param>
		/// <param name="startRow">The row to start on.</param>
		public List<SourceCodeCommit> RetrieveRevisionsForFile(
			string fileKey,
			string branchKey,
			string sortProperty,
			bool sortAsc,
			int startRow,
			int numRows,
			Hashtable filters,
			double utcOffset,
			out int totalCount)
		{
			//Delegate to the more general method
			int fileId;
			return RetrieveRevisionsForFile(fileKey, branchKey, sortProperty, sortAsc, startRow, numRows, filters, utcOffset, out fileId, out totalCount);
		}

		/// <summary>Retrieves a list of revisions made to a specific file in the repository</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="fileKey">The filekey for the file</param>
		/// <param name="branchKey">The key of the branch to pull data from.</param>
		/// <param name="filters">Hashtable of filters to apply to the data returned.</param>
		/// <param name="numRows">The number of rows to return.</param>
		/// <param name="sortAsc">Whether to sort ascending or not.</param>
		/// <param name="sortProperty">The property/field to sort our items on.</param>
		/// <param name="startRow">The row to start on.</param>
		/// <param name="fileId">The id of the file (from its key) or zero if no file</param>
		public List<SourceCodeCommit> RetrieveRevisionsForFile(
			string fileKey,
			string branchKey,
			string sortProperty,
			bool sortAsc,
			int startRow,
			int numRows,
			Hashtable filters,
			double utcOffset,
			out int fileId,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionsForFile()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			List<SourceCodeCommit> scCommits = new List<SourceCodeCommit>();
			totalCount = 0;
			fileId = 0;

			//Try getting the data from the database and file cache
			if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
			{
				//Pull all revisions that exist in our file, get the File, first.
				SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey == fileKey);
				if (file != null)
				{
					fileId = file.FileId;
					//Search for all revisions that have the file key in their file list
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from s in context.SourceCodeCommits.Include(s => s.Files)
									where
										s.ProjectId == this.projectId &&
										s.Branches.Any(b => b.Name == branchKey) &&
										s.Files.Any(f => f.FileKey == file.FileKey)
									select s;

						//Add the dynamic sort
						if (String.IsNullOrEmpty(sortProperty))
						{
							//Default to sorting by last updated date descending
							query = query.OrderByDescending(s => s.UpdateDate).ThenBy(s => s.RevisionId);
						}
						else
						{
							//We always sort by the physical ID to guarantee stable sorting
							string sortExpression = sortProperty + " " + ((sortAsc) ? "ASC" : "DESC");
							query = query.OrderUsingSortExpression(sortExpression, "RevisionId");
						}

						//Add the dynamic filters
						if (filters != null)
						{
							//Get the template for this project
							int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

							//Handle the Action filter separately
							if (filters.ContainsKey("Action"))
							{
								string action = ((string)filters["Action"]).ToLower();
								query = query.Where(s => s.Files.Any(f => f.Action.ToLower().Contains(action) && f.FileKey == file.FileKey));
							}

							//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
							Expression<Func<SourceCodeCommit, bool>> filterClause = CreateFilterExpression<SourceCodeCommit>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.None, filters, utcOffset, null, null);
							if (filterClause != null)
							{
								query = (IOrderedQueryable<SourceCodeCommit>)query.Where(filterClause);
							}
						}

						//Get the total.
						totalCount = query.Count();

						//Make pagination is in range
						if (startRow < 1 || startRow > totalCount)
						{
							startRow = 1;
						}

						//Execute the query
						scCommits = query
							.Skip(startRow - 1)
							.Take(numRows)
							.ToList();
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FileKey.");
					scCommits.Add(this.generateInvalidCacheCommit());
					this.LaunchCacheRefresh();
				}
			}
			else
			{
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
				scCommits.Add(this.generateInvalidCacheCommit());
				this.LaunchCacheRefresh();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return scCommits;

		}

		/// <summary>Retrieves a list of associated SpiraTeam artifacts for a specific build</summary>
		/// <returns>An artifact link dataset</returns>
		/// <param name="buildId">the id of the build</param>
		/// <param name="projectId">The id of the project</param>
		/// <remarks>
		/// It returns associations for all branches, not just the current one
		/// </remarks>
		public List<ArtifactLinkView> RetrieveAssociationsForBuild(int projectId, int buildId)
		{
			const string METHOD_NAME = "RetrieveAssociationsForBuild()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<ArtifactLinkView> associations = new List<ArtifactLinkView>();

			try
			{
				//Get the list of associations for the specified build
				//This query joins across all projects, so need to filter out incorrect projects at the next stage
				List<ArtifactSourceCodeRevisionView> artifactSourceCodeRevisions;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeRevisionsView
								join s in context.SourceCodeCommits on a.RevisionKey equals s.Revisionkey
								join b in context.BuildSourceCodes on s.Revisionkey equals b.RevisionKey
								where b.BuildId == buildId && s.ProjectId == projectId
								select a;

					//Get the list
					artifactSourceCodeRevisions = query.ToList();
				}

				//Loop through each one, and create a record for it.
				ArtifactManager artifactManager = new ArtifactManager();
				foreach (ArtifactSourceCodeRevisionView artifactSourceCodeRevision in artifactSourceCodeRevisions)
				{
					//Make sure the artifact is in the current project
					int? artifactProjectId = artifactManager.GetProjectForArtifact((Artifact.ArtifactTypeEnum)artifactSourceCodeRevision.ArtifactTypeId, artifactSourceCodeRevision.ArtifactId);
					if (artifactProjectId.HasValue && artifactProjectId == this.projectId)
					{
						//The new link object.
						ArtifactLinkView newLink = new ArtifactLinkView();

						//Set standard values..
						newLink.CreationDate = artifactSourceCodeRevision.CreationDate;
						newLink.ArtifactLinkId = -1; //Not editable
						newLink.ArtifactId = artifactSourceCodeRevision.ArtifactId;
						newLink.Comment = artifactSourceCodeRevision.Comment;
						newLink.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
						newLink.ArtifactTypeId = artifactSourceCodeRevision.ArtifactTypeId;
						newLink.ArtifactTypeName = artifactSourceCodeRevision.ArtifactTypeName;

						//Get the artifact type..
						this.PopulateLinkedArtifact(ref newLink);
						associations.Add(newLink);
					}
				}

				//DeDupe - check the artifact is not already in the list of associations (happens if multiple commits in the build link to it)
				associations = associations.GroupBy(association => new { association.ArtifactId, association.ArtifactTypeId })
					.Select(group => group.First())
					.ToList();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return associations;
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a list of revisions that are in a pull request</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="pullRequestId">The id of the pull request</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="branchKey">The branch to pull revisions from.</param>
		/// <param name="filters">Filters to filter against.</param>
		/// <param name="startRow">The row to start returning from.</param>
		/// <param name="numRows">Number of rows to return/per page.</param>
		/// <param name="sortAsc">Sort ascending or not.</param>
		/// <param name="sortField">The field to sort on.</param>
		/// <remarks>
		/// If the pull request task has no branches defined, it will return nothing
		/// </remarks>
		public List<SourceCodeCommit> RetrieveRevisionsForPullRequest(
			int pullRequestId,
			string sortField,
			bool sortAsc,
			int startRow,
			int numRows,
			Hashtable filters,
			double utcOffset,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionsForPullRequest()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<SourceCodeCommit> retList = new List<SourceCodeCommit>();
			totalCount = 0;

			//First retrieve the pull request
			PullRequest pullRequest = new PullRequestManager().PullRequest_RetrieveById(pullRequestId);
			if (pullRequest == null)
			{
				return retList;
			}

			//Get the list of revisions that are in the source branch but not in the destination branch
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var query = from s in context.SourceCodeCommits
							where
								s.ProjectId == this.projectId &&
								s.VersionControlSystemId == this.versionControlSystemId &&
								s.Branches.Any(b => b.BranchId == pullRequest.SourceBranchId) &&
								!s.Branches.Any(b => b.BranchId == pullRequest.DestBranchId)
							select s;

				//Add the dynamic sort
				if (String.IsNullOrEmpty(sortField))
				{
					//Default to sorting by last updated date descending
					query = query.OrderByDescending(s => s.UpdateDate).ThenBy(s => s.RevisionId);
				}
				else
				{
					//We always sort by the physical ID to guarantee stable sorting
					string sortExpression = sortField + " " + ((sortAsc) ? "ASC" : "DESC");
					query = query.OrderUsingSortExpression(sortExpression, "RevisionId");
				}

				//Add the dynamic filters
				if (filters != null)
				{
					//Get the template for this project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
					Expression<Func<SourceCodeCommit, bool>> filterClause = CreateFilterExpression<SourceCodeCommit>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.None, filters, utcOffset, null, null);
					if (filterClause != null)
					{
						query = (IOrderedQueryable<SourceCodeCommit>)query.Where(filterClause);
					}
				}

				//Get the total.
				totalCount = query.Count();

				//Make pagination is in range
				if (startRow < 1 || startRow > totalCount)
				{
					startRow = 1;
				}

				//Execute the query
				retList = query
					.Skip(startRow - 1)
					.Take(numRows)
					.ToList();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves a list of revisions that were part of a specific build</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="buildId">The id of the build</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="branchKey">The branch to pull revisions from.</param>
		/// <param name="filters">Filters to filter against.</param>
		/// <param name="startRow">The row to start returning from.</param>
		/// <param name="numRows">Number of rows to return/per page.</param>
		/// <param name="sortAsc">Sort ascending or not.</param>
		/// <param name="sortField">The field to sort on.</param>
		/// <remarks>
		/// It retrieves the matching revisions from all branches
		/// </remarks>
		public List<SourceCodeCommit> RetrieveRevisionsForBuild(
			int buildId,
			string sortField,
			bool sortAsc,
			int startRow,
			int numRows,
			Hashtable filters,
			double utcOffset,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionsForBuild()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<SourceCodeCommit> retList = new List<SourceCodeCommit>();
			totalCount = 0;

			//Get the list of revisions for this build
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//We need to join on the revision key field
				var query = from s in context.SourceCodeCommits
							join b in context.BuildSourceCodes on s.Revisionkey equals b.RevisionKey
							where
								s.ProjectId == this.projectId &&
								b.BuildId == buildId
							select s;

				//Add the dynamic sort
				if (String.IsNullOrEmpty(sortField))
				{
					//Default to sorting by last updated date descending
					query = query.OrderByDescending(s => s.UpdateDate).ThenBy(s => s.RevisionId);
				}
				else
				{
					//We always sort by the physical ID to guarantee stable sorting
					string sortExpression = sortField + " " + ((sortAsc) ? "ASC" : "DESC");
					query = query.OrderUsingSortExpression(sortExpression, "RevisionId");
				}

				//Add the dynamic filters
				if (filters != null)
				{
					//Get the template for this project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
					Expression<Func<SourceCodeCommit, bool>> filterClause = CreateFilterExpression<SourceCodeCommit>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.None, filters, utcOffset, null, null);
					if (filterClause != null)
					{
						query = (IOrderedQueryable<SourceCodeCommit>)query.Where(filterClause);
					}
				}

				//Get the total.
				totalCount = query.Count();

				//Make pagination is in range
				if (startRow < 1 || startRow > totalCount)
				{
					startRow = 1;
				}

				//Execute the query
				retList = query
					.Skip(startRow - 1)
					.Take(numRows)
					.ToList();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>
		/// Returns the highest Revision ID in use
		/// </summary>
		/// <returns>The internal ID of the revision</returns>
		protected int GetLatestRevision()
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionByKey()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			int latestRevisionId = 0;

			try
			{
				//Pull it from the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.SourceCodeCommits
								where s.ProjectId == this.projectId && s.VersionControlSystemId == this.versionControlSystemId
								orderby s.RevisionId descending
								select s;

					SourceCodeCommit commit = query.FirstOrDefault();
					if (commit != null)
						latestRevisionId = commit.RevisionId;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return latestRevisionId;
		}

		/// <summary>
		/// Checks to see if the revision exists in the branch
		/// </summary>
		/// <param name="branchKey">The name of the branch</param>
		/// <param name="revisionKey">The key of the revision</param>
		/// <returns>True if it exists</returns>
		public bool DoesRevisionExistInBranch(string branchKey, string revisionKey)
		{
			const string METHOD_NAME = CLASS_NAME + "DoesRevisionExistInBranch()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				bool exists = false;

				//Pull it from the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.SourceCodeCommits
									.Include(s => s.Files)
								where
									s.Revisionkey == revisionKey &&
									s.ProjectId == this.projectId &&
									s.VersionControlSystemId == this.versionControlSystemId &&
									s.Branches.Any(b => b.Name == branchKey)
								select s;

					exists = (query.Count() > 0);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return exists;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a single revision by its key</summary>
		/// <returns>The revisions object</returns>
		/// <param name="revisionKey">The key for the revision</param>
		public SourceCodeCommit RetrieveRevisionByKey(string revisionKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionByKey()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			SourceCodeCommit retValue = null;

			try
			{
				//Pull it from the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.SourceCodeCommits
									.Include(s => s.Files)
								where s.Revisionkey == revisionKey && s.ProjectId == this.projectId && s.VersionControlSystemId == this.versionControlSystemId
								select s;

					SourceCodeCommit commit = query.FirstOrDefault();
					if (commit != null)
						retValue = commit;
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present RevisionKey.");
						retValue = this.generateInvalidCacheCommit();
						this.LaunchCacheRefresh();
					}
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve revision from version control provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>
		/// Retrieves the immediately preceding revision for a file (if there is one)
		/// </summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="revisionId">The id of the current revision</param>
		/// <returns>The previous revision, or null if there isn't one</returns>
		public SourceCodeCommit RetrievePreviousRevision(string fileKey, int revisionId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			SourceCodeCommit previousSourceCodeCommit = null;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the specified revision
					var query = from s in context.SourceCodeCommits
								where
									s.RevisionId == revisionId &&
									s.ProjectId == this.projectId &&
									s.VersionControlSystemId == this.versionControlSystemId
								select s;

					SourceCodeCommit commit = query.FirstOrDefault();
					if (commit != null)
					{
						//We want to find the previous revision for this file by date
						DateTime updateDate = commit.UpdateDate;
						var query2 = from s in context.SourceCodeCommits
									 where
										 s.ProjectId == this.projectId &&
										 s.VersionControlSystemId == this.versionControlSystemId &&
										 s.Files.Any(f => f.FileKey == fileKey) && s.UpdateDate < updateDate &&
										 s.RevisionId != revisionId
									 orderby s.UpdateDate descending
									 select s;

						previousSourceCodeCommit = query2.FirstOrDefault();
					}
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present RevisionKey.");
						this.LaunchCacheRefresh();
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return previousSourceCodeCommit;
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve revision from version control provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a single revision by its internal ID</summary>
		/// <returns>The revisions object</returns>
		/// <param name="revisionId">The internal ID for the revision</param>
		public SourceCodeCommit RetrieveRevisionById(int revisionId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			SourceCodeCommit retValue = null;

			try
			{
				//Pull it from the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.SourceCodeCommits
										.Include(s => s.Files)
								where s.RevisionId == revisionId && s.ProjectId == this.projectId && s.VersionControlSystemId == this.versionControlSystemId
								select s;

					SourceCodeCommit commit = query.FirstOrDefault();
					if (commit != null)
						retValue = commit;
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present RevisionKey.");
						retValue = this.generateInvalidCacheCommit();
						this.LaunchCacheRefresh();
					}
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve revision from version control provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Retrieves a list of revisions associated with a SpiraTeam artifact</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="artifactType">The type of artifact (task, incident, etc.)</param>
		/// <param name="artifactId">The id of the artifact</param>
		public List<SourceCodeCommit> RetrieveRevisionsForArtifact(
			DataModel.Artifact.ArtifactTypeEnum artifactType,
			int artifactId
			)
		{
			int totalCount;
			return this.RetrieveRevisionsForArtifact(artifactType, artifactId, FIELD_UPDATE_DATE, false, 1, Int32.MaxValue, null, 0, out totalCount);
		}

		/// <summary>Retrieves a list of revisions associated with a SpiraTeam artifact</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="userId">The current user</param>
		/// <param name="artifactType">The type of artifact (task, incident, etc.)</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="filters">Filters to filter against.</param>
		/// <param name="startRow">The row to start returning from.</param>
		/// <param name="numRows">Number of rows to return/per page.</param>
		/// <param name="sortAsc">Sort ascending or not.</param>
		/// <param name="sortField">The field to sort on.</param>
		/// <remarks>
		/// 1) This version shows all revisions regardless of which branch it can be found in.
		/// 2) This includes the revisions that are added in the tool and those in the commit message
		/// </remarks>
		public List<SourceCodeCommit> RetrieveRevisionsForArtifact(
			DataModel.Artifact.ArtifactTypeEnum artifactType,
			int artifactId,
			string sortField,
			bool sortAsc,
			int startRow,
			int numRows,
			Hashtable filters,
			double utcOffset,
			out int totalCount
			)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionsForArtifact()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<SourceCodeCommit> sourceCodeRevisions = new List<SourceCodeCommit>();

			try
			{
				//Get the list of revisions for this build
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to join on the association table
					var query = from s in context.SourceCodeCommits
								join a in context.ArtifactSourceCodeRevisionsView on s.Revisionkey equals a.RevisionKey
								where
									s.ProjectId == this.projectId &&
									a.ArtifactTypeId == (int)artifactType &&
									a.ArtifactId == artifactId
								select s;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortField))
					{
						//Default to sorting by last updated date descending
						query = query.OrderByDescending(s => s.UpdateDate).ThenBy(s => s.RevisionId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortField + " " + ((sortAsc) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "RevisionId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<SourceCodeCommit, bool>> filterClause = CreateFilterExpression<SourceCodeCommit>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<SourceCodeCommit>)query.Where(filterClause);
						}
					}

					//Get the total.
					totalCount = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > totalCount)
					{
						startRow = 1;
					}

					//Execute the query
					sourceCodeRevisions = query
						.Skip(startRow - 1)
						.Take(numRows)
						.ToList();
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return sourceCodeRevisions;
		}

		/// <summary>Adds a new association between a SpiraTeam artifact and a source code file</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="comment">A comment (optional)</param>
		/// <param name="creationDate">The creation date</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="branchKey">The branch to link to the file in.</param>
		/// <remarks>It checks to make sure that the file exists in the source code provider</remarks>
		/// <returns>The id of the new association record</returns>
		public int AddFileAssociation(int projectId, string fileKey, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, DateTime creationDate, string branchKey, string comment = "")
		{
			const string METHOD_NAME = CLASS_NAME + "AddFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			int retValue = -1;

			try
			{
				//First verify that the file exists, exceptions get simply rethrown
				this.RetrieveFileByKey(fileKey, branchKey);

				//Now verify that the artifact exists
				ArtifactManager artifactManager = new ArtifactManager();
				if (!artifactManager.VerifyArtifactExists(artifactType, artifactId, projectId))
				{
					//We can't associate with a non-existant artifact
					throw new ArtifactLinkDestNotFoundException("The artifact id specified doesn't exist");
				}

				//Now add the new association record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Populate the new artifact source code file association
					ArtifactSourceCodeFile artifactSourceCodeFile = new ArtifactSourceCodeFile();
					artifactSourceCodeFile.ArtifactTypeId = (int)artifactType;
					artifactSourceCodeFile.ArtifactId = artifactId;
					artifactSourceCodeFile.FileKey = fileKey;
					artifactSourceCodeFile.CreationDate = creationDate;
					artifactSourceCodeFile.Comment = comment;

					//Persist and capture new ID
					context.ArtifactSourceCodeFiles.AddObject(artifactSourceCodeFile);
					context.SaveChanges();
					retValue = artifactSourceCodeFile.ArtifactSourceCodeFileId;
				}
			}
			catch (EntityConstraintViolationException exception)
			{
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>
		/// Adds a new association between a SpiraTeam artifact and a source code revision.
		/// Unlike the ones that come from the provider, these ones can be deleted within SpiraTeam
		/// </summary>
		/// <param name="revisionKey">The key of the revision</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="comment">A comment (optional)</param>
		/// <param name="creationDate">The creation date</param>
		/// <param name="projectId">The id of the current project</param>
		/// <remarks>It checks to make sure that the file exists in the source code provider</remarks>
		/// <returns>The id of the new association record</returns>
		public int AddRevisionAssociation(int projectId, string revisionKey, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, DateTime creationDate, string comment = "")
		{
			const string METHOD_NAME = CLASS_NAME + "AddRevisionAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			int retValue = -1;

			try
			{
				//First verify that the revision exists in the current project, exceptions get simply rethrown
				this.RetrieveRevisionByKey(revisionKey);

				//Now verify that the artifact exists
				ArtifactManager artifactManager = new ArtifactManager();
				if (!artifactManager.VerifyArtifactExists(artifactType, artifactId, projectId))
				{
					//We can't associate with a non-existant artifact
					throw new ArtifactLinkDestNotFoundException("The artifact id specified doesn't exist");
				}

				//Now add the new association record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Populate the new artifact source code revision association
					ArtifactSourceCodeRevision artifactSourceCodeRevision = new ArtifactSourceCodeRevision();
					artifactSourceCodeRevision.ArtifactTypeId = (int)artifactType;
					artifactSourceCodeRevision.ArtifactId = artifactId;
					artifactSourceCodeRevision.RevisionKey = revisionKey;
					artifactSourceCodeRevision.CreationDate = creationDate;
					artifactSourceCodeRevision.Comment = comment;

					//Persist and capture new ID
					context.ArtifactSourceCodeRevisions.AddObject(artifactSourceCodeRevision);
					context.SaveChanges();
					retValue = artifactSourceCodeRevision.ArtifactSourceCodeRevisionId;
				}
			}
			catch (EntityConstraintViolationException exception)
			{
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Removes an existing association between a SpiraTeam artifact and a source code revision</summary>
		/// <param name="artifactSourceCodeId">The id of the association record</param>
		public void RemoveRevisionAssociation(int artifactSourceCodeId)
		{
			const string METHOD_NAME = CLASS_NAME + "RemoveFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the association
					var query = from a in context.ArtifactSourceCodeRevisions
								where a.ArtifactSourceCodeRevisionId == artifactSourceCodeId
								select a;

					ArtifactSourceCodeRevision artifactSourceCodeRevision = query.FirstOrDefault();
					if (artifactSourceCodeRevision != null)
					{
						//We have an association to delete
						context.ArtifactSourceCodeRevisions.DeleteObject(artifactSourceCodeRevision);
						context.SaveChanges();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes an existing association between a SpiraTeam artifact and a source code file</summary>
		/// <param name="artifactSourceCodeId">The id of the association record</param>
		public void RemoveFileAssociation(int artifactSourceCodeId)
		{
			const string METHOD_NAME = CLASS_NAME + "RemoveFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the association
					var query = from a in context.ArtifactSourceCodeFiles
								where a.ArtifactSourceCodeFileId == artifactSourceCodeId
								select a;

					ArtifactSourceCodeFile artifactSourceCodeFile = query.FirstOrDefault();
					if (artifactSourceCodeFile != null)
					{
						//We have an association to delete
						context.ArtifactSourceCodeFiles.DeleteObject(artifactSourceCodeFile);
						context.SaveChanges();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Removes an existing association between a SpiraTeam artifact and a source code file</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		public void RemoveFileAssociation(string fileKey, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
		{
			const string METHOD_NAME = CLASS_NAME + "RemoveFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				int artifactTypeId = (int)artifactType;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the association
					var query = from a in context.ArtifactSourceCodeFiles
								where a.FileKey == fileKey && a.ArtifactTypeId == artifactTypeId && a.ArtifactId == artifactId
								select a;

					ArtifactSourceCodeFile artifactSourceCodeFiles = query.FirstOrDefault();
					if (artifactSourceCodeFiles != null)
					{
						//We have an association to delete
						context.ArtifactSourceCodeFiles.DeleteObject(artifactSourceCodeFiles);
						context.SaveChanges();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves a list of associated SpiraTeam artifacts for a specific file</summary>
		/// <returns>An artifact link dataset</returns>
		/// <param name="branchKey">The key of the branch to pull the associations from.</param>
		/// <param name="fileKey">The key of the file</param>
		public List<ArtifactLinkView> RetrieveAssociationsForFile(string fileKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveAssociationsForRevision()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			List<ArtifactLinkView> retList = new List<ArtifactLinkView>();

			try
			{
				//Get a list of all artifact types
				List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();

				//First we need to query the table to get the list of artifact types and ids associated with the file key
				List<ArtifactSourceCodeFile> artifactSourceCodeFiles;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeFiles
								where a.FileKey == fileKey
								orderby a.ArtifactTypeId, a.ArtifactId, a.ArtifactSourceCodeFileId
								select a;

					artifactSourceCodeFiles = query.ToList();
				}

				//Now loop through the rows and get the list of files
				foreach (ArtifactSourceCodeFile artifactSourceCodeFile in artifactSourceCodeFiles)
				{
					int artifactTypeId = artifactSourceCodeFile.ArtifactTypeId;
					int artifactId = artifactSourceCodeFile.ArtifactId;
					string comment = artifactSourceCodeFile.Comment;
					DateTime creationDate = artifactSourceCodeFile.CreationDate;

					//Make sure we can get the matching artifact name
					string artifactTypeName = "";
					foreach (ArtifactType artifactType in artifactTypes)
					{
						if (artifactType.ArtifactTypeId == artifactTypeId)
						{
							artifactTypeName = artifactType.Name;
							break;
						}
					}
					if (!string.IsNullOrEmpty(artifactTypeName))
					{
						try
						{
							//Add a new row to the dataset
							ArtifactLinkView artifactLinkRow = new ArtifactLinkView();
							artifactLinkRow.ArtifactLinkId = artifactSourceCodeFile.ArtifactSourceCodeFileId;
							artifactLinkRow.ArtifactId = artifactId;
							artifactLinkRow.ArtifactTypeId = artifactTypeId;
							artifactLinkRow.CreationDate = creationDate;
							if (string.IsNullOrEmpty(comment))
							{
								artifactLinkRow.Comment = null;
							}
							else
							{
								artifactLinkRow.Comment = comment;
							}
							artifactLinkRow.ArtifactTypeName = artifactTypeName;
							artifactLinkRow.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
							artifactLinkRow.ArtifactLinkTypeName = GlobalResources.General.SourceCode_SourceCodeCommit;

							//The other fields need to come from the artifact itself
							this.PopulateLinkedArtifact(ref artifactLinkRow);
							retList.Add(artifactLinkRow);
						}
						catch (NotImplementedException)
						{
							//Just don't add the row - happens if test step linked to
						}
						catch (ArtifactNotExistsException)
						{
							//Just don't add the row - happens if artifact has been deleted
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a single source code file association
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactSourceCodeFileId">The id of the association</param>
		/// <param name="comment">The updated comment</param>
		public void UpdateFileAssociation(int projectId, int artifactSourceCodeFileId, string comment)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get the list of associations for the specified file
				ArtifactSourceCodeFile artifactSourceCodeFile;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeFiles
								where a.ArtifactSourceCodeFileId == artifactSourceCodeFileId
								select a;

					//Get the item
					artifactSourceCodeFile = query.FirstOrDefault();

					//Update the item
					if (artifactSourceCodeFile != null)
					{
						artifactSourceCodeFile.StartTracking();
						artifactSourceCodeFile.Comment = comment;
						context.SaveChanges();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Updates a single editable source code revision association
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactSourceCodeRevisionId">The id of the association</param>
		/// <param name="comment">The updated comment</param>
		public void UpdateRevisionAssociation(int projectId, int artifactSourceCodeRevisionId, string comment)
		{
			const string METHOD_NAME = CLASS_NAME + "UpdateRevisionAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get the list of associations for the specified revision
				//We only want the ones that are editable so we don't use the view that joins the ones from commit messages
				ArtifactSourceCodeRevision artifactSourceCodeRevision;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeRevisions
								where a.ArtifactSourceCodeRevisionId == artifactSourceCodeRevisionId
								select a;

					//Get the item
					artifactSourceCodeRevision = query.FirstOrDefault();

					//Update the item
					if (artifactSourceCodeRevision != null)
					{
						artifactSourceCodeRevision.StartTracking();
						artifactSourceCodeRevision.Comment = comment;
						context.SaveChanges();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}


		/// <summary>
		/// Retrieves a single source code file association by its id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactSourceCodeFileId">The id of the association</param>
		/// <returns>The artifact source code file record</returns>
		public ArtifactSourceCodeFile RetrieveFileAssociation2(int projectId, int artifactSourceCodeFileId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFileAssociation2()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get the list of associations for the specified file
				ArtifactSourceCodeFile artifactSourceCodeFile;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeFiles
								where a.ArtifactSourceCodeFileId == artifactSourceCodeFileId
								select a;

					//Get the file
					artifactSourceCodeFile = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return artifactSourceCodeFile;
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code file", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single source code file association by its id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactSourceCodeFileId">The id of the association</param>
		/// <returns>The artifact link record</returns>
		public ArtifactLink RetrieveFileAssociation(int projectId, int artifactSourceCodeFileId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFileAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ArtifactLink artifactLink = null;

			try
			{
				//Get the list of associations for the specified file
				ArtifactSourceCodeFile artifactSourceCodeFile;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeFiles
								where a.ArtifactSourceCodeFileId == artifactSourceCodeFileId
								select a;

					//Get the file
					artifactSourceCodeFile = query.FirstOrDefault();
				}
				if (artifactSourceCodeFile != null)
				{
					//Convert to ArtifactLinkView
					ArtifactManager artifactManager = new ArtifactManager();

					//Make sure the artifact is in the current project
					int? artifactProjectId = artifactManager.GetProjectForArtifact((Artifact.ArtifactTypeEnum)artifactSourceCodeFile.ArtifactTypeId, artifactSourceCodeFile.ArtifactId);
					if (artifactProjectId.HasValue && artifactProjectId == projectId)
					{
						//The new link object.
						artifactLink = new ArtifactLink();

						//Set standard values..
						artifactLink.ArtifactLinkId = artifactSourceCodeFile.ArtifactSourceCodeFileId;
						artifactLink.CreationDate = artifactSourceCodeFile.CreationDate;
						artifactLink.DestArtifactId = artifactSourceCodeFile.ArtifactId;
						artifactLink.Comment = artifactSourceCodeFile.Comment;
						artifactLink.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
						artifactLink.DestArtifactTypeId = artifactSourceCodeFile.ArtifactTypeId;
						artifactLink.SourceArtifactTypeId = (int)Artifact.ArtifactTypeEnum.SourceCodeRevision;
					}
				}
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code file", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return artifactLink;
		}

		/// <summary>
		/// Retrieves a single (editable) source code revision association by its id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactSourceCodeRevisionId">The id of the association</param>
		/// <returns>The artifact link record</returns>
		/// <remarks>
		/// We only want the ones that are editable so we don't use the view that joins the ones from commit messages
		/// </remarks>
		public ArtifactLink RetrieveRevisionAssociation(int projectId, int artifactSourceCodeRevisionId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionAssociation()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ArtifactLink artifactLink = null;

			try
			{
				//Get the list of associations for the specified revision
				//We only want the ones that are editable so we don't use the view that joins the ones from commit messages
				ArtifactSourceCodeRevision artifactSourceCodeRevision;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeRevisions
								where a.ArtifactSourceCodeRevisionId == artifactSourceCodeRevisionId
								select a;

					//Get the list
					artifactSourceCodeRevision = query.FirstOrDefault();
				}

				if (artifactSourceCodeRevision != null)
				{
					//Convert to ArtifactLinkView
					ArtifactManager artifactManager = new ArtifactManager();

					//Make sure the artifact is in the current project
					int? artifactProjectId = artifactManager.GetProjectForArtifact((Artifact.ArtifactTypeEnum)artifactSourceCodeRevision.ArtifactTypeId, artifactSourceCodeRevision.ArtifactId);
					if (artifactProjectId.HasValue && artifactProjectId == projectId)
					{
						//The new link object.
						artifactLink = new ArtifactLink();

						//Set standard values..
						artifactLink.ArtifactLinkId = artifactSourceCodeRevision.ArtifactSourceCodeRevisionId;   //-1 for commit message linked ones
						artifactLink.CreationDate = artifactSourceCodeRevision.CreationDate;
						artifactLink.DestArtifactId = artifactSourceCodeRevision.ArtifactId;
						artifactLink.Comment = artifactSourceCodeRevision.Comment;
						artifactLink.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
						artifactLink.DestArtifactTypeId = artifactSourceCodeRevision.ArtifactTypeId;
						artifactLink.SourceArtifactTypeId = (int)Artifact.ArtifactTypeEnum.SourceCodeRevision;
					}
				}
			}
			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return artifactLink;
		}

		/// <summary>Retrieves a list of associated SpiraTeam artifacts for a specific revision</summary>
		/// <returns>An artifact link dataset</returns>
		/// <param name="revisionKey">The key of the revision</param>
		public List<ArtifactLinkView> RetrieveAssociationsForRevision(string revisionKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveAssociationsForRevision()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<ArtifactLinkView> retList = new List<ArtifactLinkView>();

			try
			{
				//Get the list of associations for the specified revision
				//This query joins across all projects, so need to filter out incorrect projects at the next stage
				List<ArtifactSourceCodeRevisionView> artifactSourceCodeRevisions;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from a in context.ArtifactSourceCodeRevisionsView
								join s in context.SourceCodeCommits on a.RevisionKey equals s.Revisionkey
								where a.RevisionKey == revisionKey && s.ProjectId == projectId
								select a;

					//Get the list
					artifactSourceCodeRevisions = query.ToList();
				}

				//Loop through each one, and create a record for it.
				ArtifactManager artifactManager = new ArtifactManager();
				foreach (ArtifactSourceCodeRevisionView artifactSourceCodeRevision in artifactSourceCodeRevisions)
				{
					//Make sure the artifact is in the current project
					int? artifactProjectId = artifactManager.GetProjectForArtifact((Artifact.ArtifactTypeEnum)artifactSourceCodeRevision.ArtifactTypeId, artifactSourceCodeRevision.ArtifactId);
					if (artifactProjectId.HasValue && artifactProjectId == this.projectId)
					{
						//The new link object.
						ArtifactLinkView newLink = new ArtifactLinkView();

						//Set standard values..
						newLink.ArtifactLinkId = artifactSourceCodeRevision.ArtifactSourceCodeRevisionId;   //-1 for commit message linked ones
						newLink.CreationDate = artifactSourceCodeRevision.CreationDate;
						newLink.ArtifactId = artifactSourceCodeRevision.ArtifactId;
						newLink.Comment = artifactSourceCodeRevision.Comment;
						newLink.ArtifactLinkTypeId = (int)ArtifactLink.ArtifactLinkTypeEnum.SourceCodeCommit;
						newLink.ArtifactTypeId = artifactSourceCodeRevision.ArtifactTypeId;
						newLink.ArtifactTypeName = artifactSourceCodeRevision.ArtifactTypeName;

						//Get the artifact type..
						this.PopulateLinkedArtifact(ref newLink);
						retList.Add(newLink);
					}
				}

				//DeDupe - check the artifact is not already in the list of associations (happens if multiple commits in the build link to it)
				retList = retList.GroupBy(association => new { association.ArtifactId, association.ArtifactTypeId })
					.Select(group => group.First())
					.ToList();
			}

			catch (VersionControlArtifactPermissionDeniedException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderArtifactPermissionDeniedException("You do not have permission to access this source code revision", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>
		/// Retrieves the count of revisions between two dates
		/// </summary>
		/// <param name="branchKey">The id of the branch we want the data for</param>
		/// <param name="dateRange">The date range</param>
		/// <returns>The list of commit dates by count, most recent first</returns>
		public IOrderedEnumerable<IGrouping<DateTime, SourceCodeCommit>> RetrieveRevisionCountForDateRange(string branchKey, Common.DateRange dateRange)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisionCountForDateRange()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Normalize the key, if necessary.
				branchKey = this.normalizeBranchkey(branchKey);

				//Get all Revisions from the build from cache..
				IOrderedEnumerable<IGrouping<DateTime, SourceCodeCommit>> sortedWeeklyGroups = null;
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					DateTime startDate = (dateRange.StartDate.HasValue) ? dateRange.StartDate.Value : DateTime.MinValue;
					DateTime endDate = (dateRange.EndDate.HasValue) ? dateRange.EndDate.Value : DateTime.MaxValue;

					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from s in context.SourceCodeCommits
									where s.ProjectId == this.projectId && s.Branches.Any(b => b.Name == branchKey) && s.VersionControlSystemId == this.versionControlSystemId
									select s;

						//See if we want to consider times
						if (dateRange.ConsiderTimes)
						{
							List<SourceCodeCommit> commits = query.Where(c => c.UpdateDate >= startDate && c.UpdateDate <= endDate).ToList();
							sortedWeeklyGroups = commits
								.GroupBy(c => c.UpdateDate.AddDays(-(int)c.UpdateDate.DayOfWeek))
								.OrderBy(c => c.Key);
						}
						else
						{
							DateTime startDateDate = startDate.Date;
							DateTime endDateDate = endDate.Date;
							List<SourceCodeCommit> commits = query.Where(c => EntityFunctions.TruncateTime(c.UpdateDate) >= startDateDate && EntityFunctions.TruncateTime(c.UpdateDate) <= endDateDate).ToList();
							sortedWeeklyGroups = commits
								.GroupBy(c => c.UpdateDate.Date.AddDays(-(int)c.UpdateDate.DayOfWeek))
								.OrderBy(c => c.Key);
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return sortedWeeklyGroups;
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a list of revisions made to the repository</summary>
		/// <returns>The list of revisions</returns>
		/// <param name="totalCount">The total number of revisions when pagination is not taken into account</param>
		/// <param name="userId">The current user</param>
		/// <param name="utcOffset">The UTC offset</param>
		public List<SourceCodeCommit> RetrieveRevisions(
			string branchKey,
			string sortField,
			bool sortAsc,
			int rowStart,
			int rowToReturn,
			Hashtable filters,
			double utcOffset,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			List<SourceCodeCommit> retList = new List<SourceCodeCommit>();
			totalCount = 0;

			try
			{
				//Get all Revisions from database
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from s in context.SourceCodeCommits
									where s.ProjectId == this.projectId && s.Branches.Any(b => b.Name == branchKey) && s.VersionControlSystemId == this.versionControlSystemId
									select s;

						//Add the dynamic sort
						if (String.IsNullOrEmpty(sortField))
						{
							//Default to sorting by last updated date descending
							query = query.OrderByDescending(s => s.UpdateDate).ThenBy(s => s.RevisionId);
						}
						else
						{
							//We always sort by the physical ID to guarantee stable sorting
							string sortExpression = sortField + " " + ((sortAsc) ? "ASC" : "DESC");
							query = query.OrderUsingSortExpression(sortExpression, "RevisionId");
						}

						//Add the dynamic filters
						if (filters != null)
						{
							//Get the template for this project
							int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

							//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
							Expression<Func<SourceCodeCommit, bool>> filterClause = CreateFilterExpression<SourceCodeCommit>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.None, filters, utcOffset, null, null);
							if (filterClause != null)
							{
								query = (IOrderedQueryable<SourceCodeCommit>)query.Where(filterClause);
							}
						}

						//Get the total.
						totalCount = query.Count();

						//Make pagination is in range
						if (rowStart < 1 || rowStart > totalCount)
						{
							rowStart = 1;
						}

						//Execute the query
						retList = query
							.Skip(rowStart - 1)
							.Take(rowToReturn)
							.ToList();
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					retList.Add(this.generateInvalidCacheCommit());
					this.LaunchCacheRefresh();
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves the list of files for a specified artifact</summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="filters">Any filters to apply to the list (the fields match those used for file attachments)</param>
		/// <returns>The list of files</returns>
		/// <remarks>Searches all branches</remarks>
		public List<SourceCodeFile> RetrieveFilesForArtifact(
			DataModel.Artifact.ArtifactTypeEnum artifactType,
			int artifactId,
			Hashtable filters = null)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFilesForArtifact()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Initialize empty filters if needed.
			if (filters == null) filters = new Hashtable();

			List<SourceCodeFile> retList = new List<SourceCodeFile>();

			try
			{
				//First we need to query the table to get the list of source code file keys associated with the artifact
				List<ArtifactSourceCodeFile> artifactSourceCodeFiles;
				int artifactTypeId = (int)artifactType;
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					var query = from a in ct.ArtifactSourceCodeFiles
								where a.ArtifactId == artifactId && a.ArtifactTypeId == artifactTypeId
								orderby a.FileKey, a.ArtifactSourceCodeFileId
								select a;

					artifactSourceCodeFiles = query.ToList();
				}

				//Convert over the document filters to source code file filters (different fields)
				Hashtable sourceCodefilters = new Hashtable();
				if (filters.ContainsKey("Filename"))
				{
					sourceCodefilters.Add(FIELD_NAME, filters["Filename"]);
				}
				if (filters.ContainsKey("Size"))
				{
					sourceCodefilters.Add(FIELD_SIZE, filters["Size"]);
				}
				if (filters.ContainsKey("EditedDate"))
				{
					sourceCodefilters.Add(FIELD_LASTUPDATED, filters["EditedDate"]);
				}
				else if (filters.ContainsKey("UploadDate"))
				{
					sourceCodefilters.Add(FIELD_LASTUPDATED, filters["UploadDate"]);
				}
				if (filters.ContainsKey("ProjectAttachmentTypeId") || filters.ContainsKey("OwnerId") || filters.ContainsKey("AuthorId") || filters.ContainsKey("AttachmentId"))
				{
					//Won't every match so just return an empty list
					return new List<SourceCodeFile>();
				}

				//Now loop through the rows and get the list of files (in all branches)
				foreach (ArtifactSourceCodeFile artifactSourceCodeFile in artifactSourceCodeFiles)
				{
					foreach (KeyValuePair<string, SourceCodeCache> kvp in this.cachedData)
					{
						//Get the file object from the cached settings..
						string branchKey = kvp.Key;
						SourceCodeCache branch = kvp.Value;
						if (branch != null)
						{
							string fileKey = artifactSourceCodeFile.FileKey;
							SourceCodeFile file = branch.Files.FirstOrDefault(f => f.FileKey == fileKey);

							//Add it to the return list if it passes filters..
							//Only add it to the list once even if in multiple branches!
							if (file != null && this.DoesFilePassFilters(file, sourceCodefilters))
							{
								//Need to add the special unique association id for tracking purposes
								file.ArtifactSourceCodeId = artifactSourceCodeFile.ArtifactSourceCodeFileId;
								retList.Add(file);
								break;  //Stops it being added multiple times
							}
						}
					}
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			//Return list
			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Counts the number revisions that are in a pull request</summary>
		/// <returns>The count</returns>
		/// <param name="pullRequestId">The id of the pull request</param>
		/// <remarks>
		/// If the pull request task has no branches defined, it will return count=0
		/// </remarks>
		public int CountRevisionsForPullRequest(int pullRequestId)
		{
			const string METHOD_NAME = CLASS_NAME + "CountRevisionsForPullRequest()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//First retrieve the pull request
			PullRequest pullRequest = new PullRequestManager().PullRequest_RetrieveById(pullRequestId);
			if (pullRequest == null)
			{
				return 0;
			}

			try
			{
				int totalCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.SourceCodeCommits
								where
									s.ProjectId == this.projectId &&
									s.VersionControlSystemId == this.versionControlSystemId &&
									s.Branches.Any(b => b.BranchId == pullRequest.SourceBranchId) &&
									!s.Branches.Any(b => b.BranchId == pullRequest.DestBranchId)
								select s;

					totalCount = query.Count();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return totalCount;
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>
		/// Counts the number of revisions contained in a file
		/// </summary>
		/// <param name="branchKey">The branch name</param>
		/// <param name="fileKey">The key of the file</param>
		/// <returns>The count of revisions</returns>
		public int CountRevisionsForFile(string fileKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "CountRevisionsForFile()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			int totalCount = 0;

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			try
			{
				//Pull all revisions that exist in our file, get the File, first.
				SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey == fileKey);
				if (file != null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from s in context.SourceCodeCommits
									where
										s.ProjectId == this.projectId &&
										s.Branches.Any(b => b.Name == branchKey) &&
										s.Files.Any(f => f.FileKey == file.FileKey)
									select s;

						totalCount = query.Count();
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return totalCount;
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>
		/// Counts the number of files contained in a revision
		/// </summary>
		/// <param name="branchKey">The key of the branch</param>
		/// <param name="revisionKey">The key of the revision</param>
		/// <returns>The count of files</returns>
		/// <remarks>For all branches</remarks>
		public int CountFilesForRevision(string revisionKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "CountFilesForRevision()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			int totalCount = 0;

			try
			{
				//Normalize our branch..
				string normalizedBranchKey = normalizeBranchkey(branchKey);

				if (!string.IsNullOrEmpty(normalizedBranchKey) && this.cachedData.ContainsKey(normalizedBranchKey))
				{
					//Get the commit, first..
					SourceCodeCommit commit = RetrieveRevisionByKey(revisionKey);
					if (commit != null)
					{
						foreach (SourceCodeFileEntry scFileEntry in commit.Files)
						{
							string fileKey = scFileEntry.FileKey;

							//Make sure file is in the cache for this branch
							if (this.cachedData[normalizedBranchKey].Files.Any(f => f.FileKey == fileKey))
							{
								totalCount++;
							}
						}
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return totalCount;
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Retrieves a list of files that belong to a specific revision</summary>
		/// <returns>The list of files</returns>
		/// <param name="totalCount">The total number of artifacts when pagination not considered</param>
		/// <param name="revisionKey">The key of the revision we want the files for</param>
		///<param name="branchKey">The branch to pull the files for the revision from.</param>
		///<param name="filters">Filters that the user may have set.</param>
		///<param name="rowStart">The row number to start on, 1-based.</param>
		///<param name="rowsToReturn">The number of rows to return.</param>
		///<param name="sortAsc">Whether to sort ascending.</param>
		///<param name="sortField">The field to sort on.</param>
		public List<SourceCodeFile> RetrieveFilesForRevision(
			string revisionKey,
			string branchKey,
			string sortField,
			bool sortAsc,
			int rowStart,
			int rowsToReturn,
			Hashtable filters,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFilesForRevision()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			List<SourceCodeFile> retList = new List<SourceCodeFile>();
			totalCount = 0;

			try
			{
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the commit, first..
					SourceCodeCommit commit = RetrieveRevisionByKey(revisionKey);
					if (commit != null)
					{
						List<SourceCodeFile> files = new List<SourceCodeFile>();

						//Now we need to generate the list of files.
						foreach (SourceCodeFileEntry scFileEntry in commit.Files)
						{
							string fileKey = scFileEntry.FileKey;
							string action = scFileEntry.Action;

							//Pull the file itself from the cache
							SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey == fileKey);
							if (file != null)
							{
								//Override the action since we want the current revision action not the latest
								file.Action = action;

								//Add to our total count..
								totalCount++;

								//Make sure the Commit can pass our filters..
								if (this.DoesFilePassFilters(file, filters))
									files.Add(file);
							}
						}

						//Now lets' sort the list..
						this.sortFiles(ref files, sortField, sortAsc);

						//Now get the rows asked for..
						if (rowStart <= files.Count)
						{
							int startNo = rowStart - 1;
							int endNo = ((startNo + rowsToReturn > files.Count) ? files.Count - startNo : rowsToReturn);
							retList = files.GetRange(startNo, endNo);
						}
					}
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present RevisionKey.");
						retList.Add(this.generateInvalidCacheFile());
						this.LaunchCacheRefresh();
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					retList.Add(this.generateInvalidCacheFile());
					this.LaunchCacheRefresh();
				}

			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves a list of files that belong to a specific folder</summary>
		/// <returns>The list of files</returns>
		/// <param name="branchKey">The key of the branch to pull for.</param>
		/// <param name="filters">A list of filter the return list on.</param>
		/// <param name="folderId">The ID of the folder to pull for.</param>
		/// <param name="rowStart">The start row, 1-based.</param>
		/// <param name="rowsToReturn">The number of rows to return (per page)</param>
		/// <param name="sortAsc">Whether to sort ASCENDING or not.</param>
		/// <param name="sortField">The field to sor upon.</param>
		/// <param name="totalCount">The total number of files when pagination is not taken into account</param>
		/// <remarks>The current folder is set directly on the SourceCode object by calling SetCurrentFolder</remarks>
		public List<SourceCodeFile> RetrieveFilesByFolderId(
			int folderId,
			string sortField,
			bool sortAsc,
			int rowStart,
			int rowsToReturn,
			Hashtable filters,
			out string branchKey,
			out int totalCount)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFilesByFolderId()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			List<SourceCodeFile> retList = new List<SourceCodeFile>();
			branchKey = null;

			try
			{
				//Find the folder from the ID, first..
				SourceCodeFolder folder = this.RetrieveFolderById(folderId, out branchKey);

				if (folder != null)
				{
					List<int> fileIds = folder.ContainedFiles;

					foreach (int id in fileIds)
					{
						//Get the file ID..
						SourceCodeFile file = null;
						string ignBranch = null;
						try
						{
							file = this.RetrieveFileById(id, out ignBranch);
						}
						catch
						{ }

						if (file != null && this.DoesFilePassFilters(file, filters))
							retList.Add(file);
					}

					//Get the count before pagination
					totalCount = retList.Count;

					//Now sort the files..
					this.sortFiles(ref retList, sortField, sortAsc);

					//And get the range we need.
					if (rowStart <= retList.Count)
					{
						int startNo = rowStart - 1;
						int getNo = ((startNo + rowsToReturn > retList.Count) ? retList.Count - startNo : rowsToReturn);
						Logger.LogTraceEvent("DEBUG", "Bug Check: " + rowStart + "; " + startNo + "; " + getNo + "; " + rowsToReturn);
						retList = retList.GetRange(startNo, getNo);
					}

					//DEBUG: Print out list of #, FileID, FileName
					for (int i = 0; i < retList.Count; i++)
						Debug.WriteLine(i + " - " + retList[i].FileId + ": " + retList[i].Name);

				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FolderId or BranchKey.");
					retList.Add(this.generateInvalidCacheFile());
					totalCount = 0;
					this.LaunchCacheRefresh();
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves the parent folder of the specified file</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <returns>A single SourceCodeFolder object</returns>
		/// <param name="branchKey">The branch to pull this folder from.</param>
		public SourceCodeFolder RetrieveFolderByFileKey(string fileKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFolderByFileKey()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			SourceCodeFolder retValue = null;

			try
			{
				//Check that the cache exists, first.
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the file..
					SourceCodeFile file = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey.Equals(fileKey));

					if (file != null)
					{
						//Now get the ID from the file's field.
						retValue = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderId == file.ParentFolderId);
					}
					else
					{
						Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present FileKey.");
						retValue = this.generateInvalidCacheFolder();
						this.LaunchCacheRefresh();
					}

				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					retValue = this.generateInvalidCacheFolder();
					this.LaunchCacheRefresh();
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve folder from version control provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>
		/// Returns the DIFFerence between two versions of a file - as a side by side view
		/// </summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="currentRevisionKey">The current revision</param>
		/// <param name="previousRevisionKey">The previous revision</param>
		/// <param name="branchKey">The branch</param>
		/// <returns>The textual difference</returns>
		/// <remarks>
		/// Only works for text files
		/// </remarks>
		public SideBySideDiffModel GenerateSideBySideDiffBetweenFileRevisions(string fileKey, string currentRevisionKey, string previousRevisionKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "GenerateSideBySideDiffBetweenFileRevisions";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				//First we need to get the file and the two text versions
				string currentVersion = this.OpenFileAsText(fileKey, currentRevisionKey, branchKey);
				string previousVersion = this.OpenFileAsText(fileKey, previousRevisionKey, branchKey);

				//Make sure we have both textual versions
				SideBySideDiffModel sideBySideDiffModel = null;
				if (!String.IsNullOrEmpty(currentVersion) && !String.IsNullOrEmpty(previousVersion))
				{
					SideBySideDiffBuilder sideBySideDiffBuilder = new SideBySideDiffBuilder();
					sideBySideDiffModel = sideBySideDiffBuilder.BuildDiffModel(previousVersion, currentVersion);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return sideBySideDiffModel;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>
		/// Returns the DIFFerence between two versions of a file - as a unified text view
		/// </summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="currentRevisionKey">The current revision</param>
		/// <param name="previousRevisionKey">The previous revision</param>
		/// <param name="branchKey">The branch</param>
		/// <returns>The textual difference</returns>
		/// <remarks>
		/// Only works for text files
		/// </remarks>
		public DiffPaneModel GenerateUnifiedDiffBetweenFileRevisions(string fileKey, string currentRevisionKey, string previousRevisionKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "GenerateUnifiedDiffBetweenFileRevisions";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				//First we need to get the file and the two text versions
				string currentVersion = this.OpenFileAsText(fileKey, currentRevisionKey, branchKey);
				string previousVersion = this.OpenFileAsText(fileKey, previousRevisionKey, branchKey);

				//Make sure we have both textual versions
				DiffPaneModel diffPaneModel = null;
				if (!String.IsNullOrEmpty(currentVersion) && !String.IsNullOrEmpty(previousVersion))
				{
					InlineDiffBuilder inlineDiffBuilder = new InlineDiffBuilder();
					diffPaneModel = inlineDiffBuilder.BuildDiffModel(previousVersion, currentVersion);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return diffPaneModel;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Opens up a binary filestream and returns the text representation</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="revisionKey">The key of the revision, or set to null for the most recent version</param>
		/// <returns>The textual version of the file or null if non-text</returns>
		public string OpenFileAsText(string fileKey, string revisionKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "OpenFileAsText()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				SourceCodeFileStream sourceCodeFileStream = this.OpenFile(fileKey, revisionKey, branchKey);

				//Open the file
				byte[] binaryData;
				using (Stream stream = sourceCodeFileStream.DataStream)
				{
					//Read the file in.
					binaryData = new byte[stream.Length];
					stream.Read(binaryData, 0, (int)stream.Length);

					//Close the file stream and the response stream
					this.CloseFile(sourceCodeFileStream);
				}

				//Convert into UTF8 text
				string text;
				try
				{
					text = UnicodeEncoding.UTF8.GetString(binaryData);
				}
				catch (Exception)
				{
					text = null;
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return text;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Opens up a binary filestream to the file itself</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <param name="revisionKey">The key of the revision, or set to NullParameter for the most recent version</param>
		/// <returns>A filestream to the data held in the file</returns>
		public SourceCodeFileStream OpenFile(string fileKey, string revisionKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "OpenFile()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			SourceCodeFileStream retValue = null;

			try
			{
				//The interface expects null string for revisionKey not actual nulls
				if (string.IsNullOrWhiteSpace(revisionKey))
					revisionKey = "";

				//We don't need to use the cached settings because we have the real file and revision keys provided
				//We just need to pass this request on to the provider.
				VersionControlFileStream stream = null;
				if (this.isV2)
					stream = ((IVersionControlPlugIn2)this.interfaceLibrary).OpenFile(interfaceToken, fileKey, revisionKey, branchKey);
				else
					stream = ((IVersionControlPlugIn)this.interfaceLibrary).OpenFile(interfaceToken, fileKey, revisionKey);

				//Make sure we didn't get anything null back..
				if (stream != null)
				{
					//Convert into a SourceCodeFileStream object
					retValue = new SourceCodeFileStream();
					retValue.FileKey = stream.FileKey;
					retValue.RevisionKey = stream.RevisionKey;
					retValue.LocalPath = stream.LocalPath;
					retValue.DataStream = stream.DataStream;
					retValue.BranchKey = branchKey;
				}
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve file from version control provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Closes the binary filestream to the file itself</summary>
		/// <param name="sourceCodeFileStream">The stream to close</param>
		/// <remarks>Clients must NOT CLOSE THE STREAM DIRECTLY</remarks>
		public void CloseFile(SourceCodeFileStream sourceCodeFileStream)
		{
			const string METHOD_NAME = CLASS_NAME + "CloseFile()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				//We don't need to use the cached settings because we have the real file and revision keys provided
				//We just need to pass this request on to the provider
				VersionControlFileStream versionControlFileStream = new VersionControlFileStream();
				versionControlFileStream.FileKey = sourceCodeFileStream.FileKey;
				versionControlFileStream.RevisionKey = sourceCodeFileStream.RevisionKey;
				versionControlFileStream.LocalPath = sourceCodeFileStream.LocalPath;
				versionControlFileStream.DataStream = sourceCodeFileStream.DataStream;

				//See if we have V1 or V2 interface
				if (this.isV2)
				{
					((IVersionControlPlugIn2)this.interfaceLibrary).CloseFile(versionControlFileStream);
				}
				else
				{
					((IVersionControlPlugIn)this.interfaceLibrary).CloseFile(versionControlFileStream);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves a single file by its ID</summary>
		/// <param name="fileId">The ID of the file</param>
		/// <returns>A single SourceCodeFile object</returns>
		/// <param name="branchKey">The key of the branch that tyhe file exists in.</param>
		public SourceCodeFile RetrieveFileById(int fileId, out string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFileById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			SourceCodeFile retValue = null;
			branchKey = null;

			try
			{
				//We need the id, which is unique among all branches.
				foreach (KeyValuePair<string, SourceCodeCache> kvp in this.cachedData)
				{
					retValue = kvp.Value.Files.FirstOrDefault(f => f.FileId == fileId);

					if (retValue != null)
					{
						branchKey = kvp.Key;
						break;
					}
				}

				if (retValue == null)
					throw new VersionControlArtifactNotFoundException();

			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve file from version control provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Retrieves a single file by its key</summary>
		/// <param name="fileKey">The key of the file</param>
		/// <returns>A single SourceCodeFile object</returns>
		/// <param name="branchKey">The branch to pull the fuile from.</param>
		public SourceCodeFile RetrieveFileByKey(string fileKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFileByKey()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			SourceCodeFile retValue = null;

			try
			{
				//Make sure it exists in Cache..
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the file from cache..
					retValue = this.cachedData[branchKey].Files.FirstOrDefault(f => f.FileKey.Equals(fileKey));
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
					this.LaunchCacheRefresh();
					throw new SourceCodeCacheInvalidException("Requested branch not found in cache.");
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve file from version control provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Retrieves a single folder by its ID</summary>
		/// <param name="folderId">The ID of the folder</param>
		/// <returns>A single SourceCodeFolder object</returns>
		/// <param name="branchKey">The key of the branch that tyhe folder exists in.</param>
		public SourceCodeFolder RetrieveFolderById(int folderId, out string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFolderById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			SourceCodeFolder retValue = null;
			branchKey = null;

			try
			{
				//We need the id, which is unique among all branches.
				foreach (KeyValuePair<string, SourceCodeCache> kvp in this.cachedData)
				{
					retValue = kvp.Value.Folders.FirstOrDefault(f => f.FolderId == folderId);

					if (retValue != null)
					{
						branchKey = kvp.Key;
						break;
					}
				}

				if (retValue == null)
					throw new VersionControlArtifactNotFoundException();

			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve folder from version control provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Retrieves a single folder by its key (path)</summary>
		/// <param name="folderKey">The key of the folder</param>
		/// <returns>A single SourceCodeFolder object</returns>
		/// <param name="branchKey">The key of the branch to pull the folder from.</param>
		public SourceCodeFolder RetrieveFolderByKey(string folderKey, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFolderByKey()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Our return value..
			SourceCodeFolder retValue = null;

			//Normalize the key, if necessary.
			branchKey = this.normalizeBranchkey(branchKey);

			//If we still have a null branchkey, we need to throw an error.
			if (string.IsNullOrWhiteSpace(branchKey))
			{
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to non-present BranchKey.");
				this.LaunchCacheRefresh();
				throw new SourceCodeCacheInvalidException();
			}

			try
			{
				//Make sure it exists in Cache..
				if (!string.IsNullOrEmpty(branchKey) && this.cachedData.ContainsKey(branchKey))
				{
					//Get the file from cache..
					retValue = this.cachedData[branchKey].Folders.FirstOrDefault(f => f.FolderKey == folderKey);
				}
				else
				{
					this.LaunchCacheRefresh();
					throw new SourceCodeCacheInvalidException("Requested branch not found in cache.");
				}

			}
			catch (VersionControlArtifactNotFoundException exception)
			{
				//Throw the normal ArtifactNotExists exception used by the application
				Logger.LogWarningEvent(METHOD_NAME, exception.Message, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new ArtifactNotExistsException("Unable to retrieve folder from version control provider", exception);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Retrieves a set of source code file folders that have a specific parent id</summary>
		/// <param name="parentFolderId">The ID of the parent folder (Null for the root)</param>
		/// <returns>A collection of SourceCodeFolder objects</returns>
		/// <param name="branchkey">The key of the branch to pull the folders from, only if parentFolderId is null.</param>
		public List<SourceCodeFolder> RetrieveFoldersByParentId(int? parentFolderId, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveFoldersByParentId()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			//Our return list..
			List<SourceCodeFolder> retList = new List<SourceCodeFolder>();

			//Normalize our branch..
			string normalizedBranchKey = normalizeBranchkey(branchKey);

			//If we still have a null branchkey, we need to throw an error.
			if (string.IsNullOrWhiteSpace(normalizedBranchKey))
			{
				if (string.IsNullOrWhiteSpace(branchKey))
				{
					Logger.LogWarningEvent(METHOD_NAME, GlobalResources.Messages.SourceCode_BranchKeyNull);
				}
				else
				{
					Logger.LogWarningEvent(METHOD_NAME, string.Format(GlobalResources.Messages.SourceCode_BranchKeyNotFound, branchKey));
				}
				Logger.LogWarningEvent(METHOD_NAME, "Launching SourceCode Refresh due to empty BranchKey.");
				this.LaunchCacheRefresh();
				throw new SourceCodeCacheInvalidException();
			}

			//See if we have the branch in the cache
			if (!this.cachedData.ContainsKey(normalizedBranchKey))
			{
				Logger.LogInformationalEvent(METHOD_NAME, string.Format(GlobalResources.Messages.SourceCode_BranchKeyNotFound, normalizedBranchKey));
				throw new SourceCodeCacheInvalidException();
			}

			try
			{
				//The pulled source code folder..
				SourceCodeFolder folder = null;

				//Get the folder object from the cache..
				if (!parentFolderId.HasValue)
				{
					//Get the root folder..
					folder = this.cachedData[normalizedBranchKey].Folders.FirstOrDefault(f => f.IsRoot == true);
					retList = new List<SourceCodeFolder>();
					retList.Add(folder);
				}
				else
				{
					//Get the folder..
					folder = this.RetrieveFolderById(parentFolderId.Value, out normalizedBranchKey);

					//Now get all the containing folders..
					if (folder != null)
						retList = this.cachedData[normalizedBranchKey].Folders.Where(f => f.ParentFolderId.HasValue && f.ParentFolderId.Value == folder.FolderId).ToList();
				}
			}
			catch (VersionControlAuthenticationException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderAuthenticationException("Unable to authenticate with the source code provider", exception);
			}
			catch (VersionControlGeneralException exception)
			{
				//Rethrow as a provider error
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw new SourceCodeProviderLoadingException("Unable to load the source code provider", exception);
			}
			catch (SourceCodeCacheInvalidException exception)
			{
				Logger.LogWarningEvent(METHOD_NAME, exception, Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Returns a list of all the available branches from the version control cache</summary>
		/// <returns>A list of available branches.</returns>
		/// <remarks>
		/// Retrieves from the database, which is populated at the same time as the file caches
		/// </remarks>
		public List<SourceCodeBranch> RetrieveBranches()
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveBranches()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				List<DataModel.VersionControlBranch> versionControlBranches;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from b in context.VersionControlBranches
								where
									b.ProjectId == projectId &&
									b.IsActive &&
									b.VersionControlSystemId == this.versionControlSystemId
								orderby b.Name, b.BranchId
								select b;

					versionControlBranches = query.ToList();
				}

				//If we have no branches, fallback to returning a single one
				List<SourceCodeBranch> sourceCodeBranches = new List<SourceCodeBranch>();
				if (versionControlBranches.Count == 0)
				{
					//fallback to just returning one 'dummy' branch
					SourceCodeBranch newBranch = new SourceCodeBranch();
					newBranch.BranchKey = DEFAULT_BRANCH_V2;
					newBranch.IsDefault = true;
					sourceCodeBranches.Add(newBranch);
				}
				else
				{
					//Convert into the source code branch object
					foreach (DataModel.VersionControlBranch versionControlBranch in versionControlBranches)
					{
						SourceCodeBranch branch = new SourceCodeBranch();
						branch.BranchKey = versionControlBranch.Name;
						branch.IsDefault = versionControlBranch.IsHead;
						sourceCodeBranches.Add(branch);
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return sourceCodeBranches;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Returns a list of all the available branches from the database</summary>
		/// <returns>A list of available branches.</returns>
		/// <remarks>
		/// Retrieves from the database. This version does not require the cache to be loaded
		/// </remarks>
		public List<DataModel.VersionControlBranch> RetrieveBranches2(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveBranches2()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<DataModel.VersionControlBranch> versionControlBranches;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from b in context.VersionControlBranches
								where
									b.ProjectId == projectId &&
									b.IsActive
								orderby b.Name, b.BranchId
								select b;

					versionControlBranches = query.ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return versionControlBranches;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Returns a list of all the branches that a specific revision is in</summary>
		/// <returns>A list of matching branches</returns>
		/// <param name="revisionKey">The key of the revision</param>
		/// <remarks>
		/// Retrieves from the database, which is populated at the same time as the file caches
		/// </remarks>
		public List<SourceCodeBranch> RetrieveBranchesByRevision(string revisionKey)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveBranches()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				List<DataModel.VersionControlBranch> versionControlBranches;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from b in context.VersionControlBranches
								where b.ProjectId == projectId && b.IsActive && b.Commits.Any(c => c.Revisionkey == revisionKey)
								orderby b.Name, b.BranchId
								select b;

					versionControlBranches = query.ToList();
				}

				//If we have no branches, fallback to returning a single one
				List<SourceCodeBranch> sourceCodeBranches = new List<SourceCodeBranch>();
				if (versionControlBranches.Count == 0)
				{
					//fallback to just returning one 'dummy' branch
					SourceCodeBranch newBranch = new SourceCodeBranch();
					newBranch.BranchKey = DEFAULT_BRANCH_V2;
					newBranch.IsDefault = true;
					sourceCodeBranches.Add(newBranch);
				}
				else
				{
					//Convert into the source code branch object
					foreach (DataModel.VersionControlBranch versionControlBranch in versionControlBranches)
					{
						SourceCodeBranch branch = new SourceCodeBranch();
						branch.BranchKey = versionControlBranch.Name;
						branch.IsDefault = versionControlBranch.IsHead;
						sourceCodeBranches.Add(branch);
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return sourceCodeBranches;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		/// <summary>Returns a branch by its name</summary>
		/// <returns>A specific branch</returns>
		/// <remarks>
		/// Retrieves from the database, returns null if does not exist
		/// </remarks>
		public SourceCodeBranch RetrieveBranchByName(string branchName)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveBranchByName()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (this.projectId == 0)
				throw new InvalidOperationException("Manager was not loaded properly.");

			try
			{
				DataModel.VersionControlBranch versionControlBranch;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from b in context.VersionControlBranches
								where b.ProjectId == projectId &&
									b.Name == branchName &&
									b.VersionControlSystemId == this.versionControlSystemId
								select b;

					versionControlBranch = query.FirstOrDefault();
				}

				if (versionControlBranch == null)
				{
					return null;
				}

				//Convert into the source code branch object
				SourceCodeBranch sourceCodeBranch = new SourceCodeBranch();
				sourceCodeBranch.BranchKey = versionControlBranch.Name;
				sourceCodeBranch.IsDefault = versionControlBranch.IsHead;

				Logger.LogExitingEvent(METHOD_NAME);
				return sourceCodeBranch;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception, "", Logger.EVENT_CATEGORY_VERSION_CONTROL);
				throw;
			}
		}

		#endregion Data Business Functions

		#region IDispose Functions
		/// <summary>Disposes the manager.</summary>
		public void Dispose()
		{

			//Standard cleanup calls..
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Cleans up the object and saves any changes to the cache files.</summary>
		/// <param name="disposing">Called from owner code, or finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (isDisposed)
				return;

			if (disposing)
			{
				//Save cache files if needed.

				//Unload DLL.
				this.interfaceLibrary = null;
				this.interfaceDomain = null;
			}

			isDisposed = true;
		}

		/// <summary>Finalize the class.</summary>
		~SourceCodeManager()
		{
			this.Dispose(false);
		}
		#endregion IDispose Functions

		#region Internal Functions

		/// <summary>Returns true or false whether the given File passes the given filters.</summary>
		/// <param name="file">The file to check.</param>
		/// <param name="filters">Filters to check the commit against.</param>
		/// <returns>True if the file passes filters and can be displayed.</returns>
		private bool DoesFilePassFilters(SourceCodeFile file, Hashtable filters)
		{
			const string METHOD_NAME = CLASS_NAME + "doesFilePassFilters()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool doesPass = true;

			if (filters == null || filters.Count < 1)
			{
				//No filters, so passes
				return true;
			}

			//Go through each defined filter, and make sure that the revision/commit passes.
			if (filters.Contains(FIELD_NAME)) //Searching Name/Key field.
			{
				if ((string.IsNullOrEmpty(file.Name)) || (!file.Name.ToLowerInvariant().Contains(((string)filters[FIELD_NAME]).ToLowerInvariant())))
					doesPass = false;
			}
			if (filters.Contains(FIELD_LASTUPDATED))
			{
				if (!(((Inflectra.SpiraTest.Common.DateRange)filters[FIELD_LASTUPDATED]).Contains(file.LastUpdateDate)))
					doesPass = false;
			}
			if (filters.Contains(FIELD_AUTHOR)) //Searching by Author name.
			{
				if ((string.IsNullOrEmpty(file.AuthorName)) || (!file.AuthorName.ToLowerInvariant().Contains(((string)filters[FIELD_AUTHOR]).ToLowerInvariant())))
					doesPass = false;
			}
			if (filters.Contains(FIELD_COMMIT))
			{
				if ((string.IsNullOrEmpty(file.RevisionName)) || (!file.RevisionName.ToLowerInvariant().Contains(((string)filters[FIELD_COMMIT]).ToLowerInvariant())))
					doesPass = false;
			}
			if (filters.Contains(FIELD_CONTENT_CHANGED)) //Searching on Content Changed.
			{ }
			if (filters.Contains(FIELD_PROP_CHANGED)) //Searching on Properties Changed.
			{ }
			if (filters.Contains(FIELD_ACTION))
			{
				//Searching on File Action
				if ((string.IsNullOrEmpty(file.Action)) || (!file.Action.ToLowerInvariant().Contains(((string)filters[FIELD_ACTION]).ToLowerInvariant())))
					doesPass = false;
			}
			if (filters.Contains(FIELD_SIZE))
			{
				//Currently does not support filtering by size because we plan on varying the displayed units
				//So ignore
				//The calling function will check for this
				doesPass = true;
			}


			Logger.LogExitingEvent(METHOD_NAME + " - Passed: " + doesPass.ToSafeString());
			return doesPass;
		}

		/// <summary>Sorts the list of files with the given key/sort.</summary>
		/// <param name="files">The list of files to sort.</param>
		/// <param name="sortField">The field to sort against.</param>
		/// <param name="sortAsc">Whether we're soring ascending or descending.</param>
		private void sortFiles(ref List<SourceCodeFile> files, string sortField, bool sortAsc)
		{
			const string METHOD_NAME = CLASS_NAME + "sortFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			if (sortAsc)
			{
				switch (sortField)
				{
					case FIELD_AUTHOR:
						files.Sort(SourceCodeFile.AuthorAscComparison);
						break;
					case FIELD_COMMIT:
						files.Sort(SourceCodeFile.RevisionAscComparison);
						break;
					case FIELD_LASTUPDATED:
						files.Sort(SourceCodeFile.LastUpdatedAscComparison);
						break;
					case FIELD_NAME:
						files.Sort(SourceCodeFile.NameAscComparison);
						break;
					case FIELD_SIZE:
						files.Sort(SourceCodeFile.SizeAscComparison);
						break;
					case FIELD_ACTION:
						files.Sort(SourceCodeFile.ActionAscComparison);
						break;
				}
			}
			else
			{
				switch (sortField)
				{
					case FIELD_AUTHOR:
						files.Sort(SourceCodeFile.AuthorDescComparison);
						break;
					case FIELD_COMMIT:
						files.Sort(SourceCodeFile.RevisionDescComparison);
						break;
					case FIELD_LASTUPDATED:
						files.Sort(SourceCodeFile.LastUpdatedDescComparison);
						break;
					case FIELD_NAME:
						files.Sort(SourceCodeFile.NameDescComparison);
						break;
					case FIELD_SIZE:
						files.Sort(SourceCodeFile.SizeDescComparison);
						break;
					case FIELD_ACTION:
						files.Sort(SourceCodeFile.ActionDescComparison);
						break;
				}
			}
		}

		/// <summary>Populates a linked artifact record from its id and type</summary>
		/// <param name="artifactLink">The row to populate.</param>
		private void PopulateLinkedArtifact(ref ArtifactLinkView artifactLink)
		{
			const string METHOD_NAME = CLASS_NAME + "populateLinkedArtifact()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Need to handle each case in turn
			switch ((DataModel.Artifact.ArtifactTypeEnum)artifactLink.ArtifactTypeId)
			{
				case DataModel.Artifact.ArtifactTypeEnum.Requirement:
					RequirementManager requirementManager = new RequirementManager();
					RequirementView requirement = requirementManager.RetrieveById2(this.projectId, artifactLink.ArtifactId);
					artifactLink.ArtifactName = requirement.Name;
					artifactLink.CreatorId = requirement.AuthorId;
					artifactLink.CreatorName = requirement.AuthorName;
					break;

				case DataModel.Artifact.ArtifactTypeEnum.Release:
					{
						ReleaseManager releaseManager = new ReleaseManager();
						ReleaseView release = releaseManager.RetrieveById2(this.projectId, artifactLink.ArtifactId);
						artifactLink.ArtifactName = release.FullName;
						artifactLink.CreatorId = release.CreatorId;
						artifactLink.CreatorName = release.CreatorName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.TestCase:
					{
						TestCaseManager testCaseManager = new TestCaseManager();
						TestCaseView testCase = testCaseManager.RetrieveById(this.projectId, artifactLink.ArtifactId);
						artifactLink.ArtifactName = testCase.Name;
						artifactLink.CreatorId = testCase.AuthorId;
						artifactLink.CreatorName = testCase.AuthorName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.TestSet:
					{
						TestSetManager testSetManager = new TestSetManager();
						TestSetView testSet = testSetManager.RetrieveById(this.projectId, artifactLink.ArtifactId);
						artifactLink.ArtifactName = testSet.Name;
						artifactLink.CreatorId = testSet.CreatorId;
						artifactLink.CreatorName = testSet.CreatorName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.TestRun:
					{
						TestRunManager testRunManager = new TestRunManager();
						TestRunView testRun = testRunManager.RetrieveById(artifactLink.ArtifactId);
						artifactLink.ArtifactName = testRun.Name;
						artifactLink.CreatorId = testRun.TesterId;
						artifactLink.CreatorName = testRun.TesterName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.TestStep:
					{
						TestCaseManager testCaseManager = new TestCaseManager();
						TestStep testStep = testCaseManager.RetrieveStepById(this.projectId, artifactLink.ArtifactId);
						artifactLink.ArtifactName = testStep.Name + " (" + GlobalResources.General.TestStep_Step + ")";
						artifactLink.CreatorId = testStep.TestCase.AuthorId;
						artifactLink.CreatorName = "-";
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.Task:
					{
						TaskManager taskManager = new TaskManager();
						TaskView task = taskManager.TaskView_RetrieveById(artifactLink.ArtifactId);
						artifactLink.ArtifactName = task.Name;
						artifactLink.CreatorId = task.CreatorId;
						artifactLink.CreatorName = task.CreatorName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.Risk:
					{
						RiskManager riskManager = new RiskManager();
						RiskView risk = riskManager.Risk_RetrieveById2(artifactLink.ArtifactId);
						artifactLink.ArtifactName = risk.Name;
						artifactLink.CreatorId = risk.CreatorId;
						artifactLink.CreatorName = risk.CreatorName;
					}
					break;

				case DataModel.Artifact.ArtifactTypeEnum.Incident:
					{
						IncidentManager incidentManager = new IncidentManager();
						IncidentView incident = incidentManager.RetrieveById2(artifactLink.ArtifactId);
						artifactLink.ArtifactName = incident.Name;
						artifactLink.CreatorId = incident.OpenerId;
						artifactLink.CreatorName = incident.OpenerName;
					}
					break;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Generated a placeholder for displaying an invalid cache message.</summary>
		/// <returns>A 'dummy' Source Code Commit object.</returns>
		private SourceCodeCommit generateInvalidCacheCommit(string revisionKey = null)
		{
			SourceCodeCommit retValue = new SourceCodeCommit();
			retValue.AuthorName = "Administrator";
			retValue.ContentChanged = false;
			retValue.Message = GlobalResources.Messages.SourceCode_InvalidCacheLongMessage;
			retValue.Name = String.IsNullOrEmpty(revisionKey) ? GlobalResources.General.SourceCode_InvalidCache : revisionKey;
			retValue.PropertiesChanged = false;
			retValue.RevisionId = -1;
			retValue.UpdateDate = DateTime.UtcNow;

			return retValue;
		}

		/// <summary>Generated a placeholder for displaying an invalid cache message.</summary>
		/// <returns>A 'dummy' Source Code File object.</returns>
		private SourceCodeFile generateInvalidCacheFile()
		{
			SourceCodeFile retValue = new SourceCodeFile();
			retValue.AuthorName = "Administrator";
			retValue.Action = "Other";
			retValue.FileId = 01;
			retValue.LastUpdateDate = DateTime.UtcNow;
			retValue.Name = GlobalResources.General.SourceCode_InvalidCache;
			retValue.ParentFolderId = null;
			retValue.RevisionId = -1;
			retValue.RevisionKey = "";
			retValue.Size = 0;

			return retValue;
		}

		/// <summary>Generated a placeholder for displaying an invalid cache message.</summary>
		/// <returns>A 'dummy' Source Code Folder object.</returns>
		private SourceCodeFolder generateInvalidCacheFolder()
		{
			SourceCodeFolder retValue = new SourceCodeFolder();
			retValue.FolderId = -1;
			retValue.Name = GlobalResources.General.SourceCode_InvalidCache;
			retValue.ParentFolderId = null;

			return retValue;
		}

		/// <summary>Normalizes the branch key.</summary>
		/// <param name="branchKey"></param>
		/// <returns></returns>
		/// <remarks> If we are given the default branch name, AND the default name isn't in the 
		/// list of cached branches, then we'll find the existing branch marked as default and return that.</remarks>
		private string normalizeBranchkey(string branchKey)
		{
			string retValue = branchKey;

			if (string.IsNullOrWhiteSpace(branchKey) || branchKey.Equals(DEFAULT_BRANCH))
			{
				//Check if it's in the cache list..
				if (string.IsNullOrWhiteSpace(branchKey) || !this.cachedData.ContainsKey(branchKey))
				{
					//It's not in there, find the default.
					KeyValuePair<string, SourceCodeCache> kvp = this.cachedData.FirstOrDefault(c => c.Value.IsBranchDefault);
					if (kvp.Key != null)
						retValue = kvp.Key;
				}
			}

			return retValue;
		}
		#endregion Internal Functions

		#region User Settings (Static)
		#region Retrieve
		/// <summary>Gets the user's setting for sorting a file list.</summary>
		/// <param name="userId">The user ID to get the setting for.</param>
		/// <param name="projectId">The project ID to retrieve the setting for.</param>
		/// <param name="sortAsc">The user's selected sort direction.</param>
		/// <param name="sortKey">The user's selected sort field.</param>
		public static void Get_UserSortFiles(int userId, int projectId, out bool sortAsc, out string sortKey)
		{
			const string METHOD_NAME = CLASS_NAME + "GetUserSort_Files()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Get sorting dir...
			if (settings[SETTING_KEY_SORTASC] == null)
				sortAsc = true;
			else
				sortAsc = (bool)settings[SETTING_KEY_SORTASC];

			//Now get sorting key..
			if (settings[SETTING_KEY_SORTFIELD] == null)
				sortKey = FIELD_NAME;
			else
				sortKey = (string)settings[SETTING_KEY_SORTFIELD];

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Gets the user's setting for sorting a revision/Commit list.</summary>
		/// <param name="userId">The user ID to get the setting for.</param>
		/// <param name="projectId">The project ID to retrieve the setting for.</param>
		/// <param name="sortAsc">The user's selected sort direction.</param>
		/// <param name="sortKey">The user's selected sort field.</param>
		/// <remarks>Defaults to sorting by date descending</remarks>
		/// <param name="displayTypeId">The display type (which page)</param>
		public static void Get_UserSortRevisions(int userId, int projectId, int? displayTypeId, out bool sortAsc, out string sortKey)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserSortRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string otherCollection = GetRevisionOtherCollectionForDisplayType(displayTypeId);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, otherCollection);
			settings.Restore();

			//Get sorting dir...
			if (settings[SETTING_KEY_SORTASC] == null)
				sortAsc = false;
			else
				sortAsc = (bool)settings[SETTING_KEY_SORTASC];

			//Now get sorting key..
			if (settings[SETTING_KEY_SORTFIELD] == null)
				sortKey = FIELD_UPDATE_DATE;
			else
				sortKey = (string)settings[SETTING_KEY_SORTFIELD];


			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Returns the user's selected filter (if any) for a source code file list.</summary>
		/// <param name="userId">The user ID to pull the filter for.</param>
		/// <param name="projectId">The project ID to pull the filter for.</param>
		/// <returns>A hashtable of fields the user is sorting on.</returns>
		public static Hashtable Get_UserFilterFiles(int userId, int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserFilterFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_FILTER);
			settings.Restore();

			Logger.LogExitingEvent(METHOD_NAME);
			return settings;
		}

		/// <summary>Returns the user's selected filter (if any) for a source code file list.</summary>
		/// <param name="userId">The user ID to pull the filter for.</param>
		/// <param name="projectId">The project ID to pull the filter for.</param>
		/// <returns>A hashtable of fields the user is sorting on.</returns>
		/// <param name="displayTypeId">The display type (which page)</param>
		public static Hashtable Get_UserFilterRevisions(int userId, int projectId, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserFilterRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string filterCollection = GetRevisionFilterCollectionForDisplayType(displayTypeId);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, filterCollection);
			settings.Restore();

			Logger.LogExitingEvent(METHOD_NAME);
			return settings;
		}

		/// <summary>Retrieves the user's current pagnation settings.</summary>
		/// <param name="userid">The user ID to pull the settings for.</param>
		/// <param name="projectId">The project ID to pull the settings for.</param>
		/// <param name="numPerPage">The number of rows per page.</param>
		/// <param name="pageNum">The page that the user has selected.</param>
		public static void Get_UserPagnationFiles(int userId, int projectId, out int numPerPage, out int pageNum)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserPagnationFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Get our two settings..
			if (settings[SETTING_KEY_ROWSPAGE] == null)
				numPerPage = 15;
			else
				numPerPage = (int)settings[SETTING_KEY_ROWSPAGE];

			if (settings[SETTING_KEY_CURRPAGE] == null)
				pageNum = 1;
			else
				pageNum = (int)settings[SETTING_KEY_CURRPAGE];

			if (pageNum < 1) pageNum = 1;
			if (numPerPage < 15) numPerPage = 15;

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves the user's current pagnation settings.</summary>
		/// <param name="userid">The user ID to pull the settings for.</param>
		/// <param name="projectId">The project ID to pull the settings for.</param>
		/// <param name="numPerPage">The number of rows per page.</param>
		/// <param name="pageNum">The page that the user has selected.</param>
		public static void Get_UserPagnationRevisions(int userId, int projectId, out int numPerPage, out int pageNum)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserPagnationRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string otherCollection = GetRevisionOtherCollectionForDisplayType(null);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, otherCollection);
			settings.Restore();

			//Get our two settings..
			if (settings[SETTING_KEY_ROWSPAGE] == null)
				numPerPage = 15;
			else
				numPerPage = (int)settings[SETTING_KEY_ROWSPAGE];
			if (settings[SETTING_KEY_CURRPAGE] == null)
				pageNum = 1;
			else
				pageNum = (int)settings[SETTING_KEY_CURRPAGE];

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Returns the user's selected folder, if any.</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		/// <returns>A folder key (or null if not set) of the selected folder.</returns>
		public static string Get_UserSelectedSourceFolder(int userId, int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserSelectedSourceFolder()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string retValue = null;

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Get our two settings..
			if (settings[SETTING_KEY_SELECTEDFOLDER] != null && settings[SETTING_KEY_SELECTEDFOLDER] is string)
				retValue = (string)settings[SETTING_KEY_SELECTEDFOLDER];

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Sets the user's selected branch</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		/// <param name="branchKey">A string representing the branch.</param>
		public static void Set_UserSelectedBranch(int userId, int projectId, string branchKey)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserSelectedBranch()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Set the rbanch
			settings[SETTING_KEY_SELECTEDBRANCH] = branchKey;
			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Returns the user's selected branch, if any.</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		/// <returns>A string representing the branch.</returns>
		public static string Get_UserSelectedBranch(int userId, int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserSelectedBranch()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string retValue = null;

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Get our two settings..
			if (settings[SETTING_KEY_SELECTEDBRANCH] != null)
				retValue = (string)settings[SETTING_KEY_SELECTEDBRANCH];

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}
		#endregion Retrieve

		#region Store
		/// <summary>Saves the current settings for the user.</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		/// <param name="sortAsc">Whether we're sorting ascending or not.</param>
		/// <param name="sortKey">The field name we're sorting on.</param>
		public static void Set_UserSortFiles(int userId, int projectId, bool sortAsc, string sortKey)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserSortFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			settings[SETTING_KEY_SORTASC] = sortAsc;
			settings[SETTING_KEY_SORTFIELD] = sortKey;

			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Saves the current settings for the user.</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		/// <param name="sortAsc">Whether we're sorting ascending or not.</param>
		/// <param name="sortKey">The field name we're sorting on.</param>
		/// <param name="displayTypeId">The display type (which page)</param>
		public static void Set_UserSortRevisions(int userId, int projectId, bool sortAsc, string sortKey, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserSortRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string otherCollection = GetRevisionOtherCollectionForDisplayType(displayTypeId);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, otherCollection);
			settings.Restore();

			settings[SETTING_KEY_SORTASC] = sortAsc;
			settings[SETTING_KEY_SORTFIELD] = sortKey;

			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Records the new setting for the user's filters.</summary>
		/// <param name="userId">The userId to save the filter for.</param>
		/// <param name="projectId">The projectId to save the filter for.</param>
		/// <param name="filters">hashtable of filters.</param>
		public static void Set_UserFilterFiles(int userId, int projectId, Hashtable filters)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserFilterFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_FILTER);
			settings.Restore();
			settings.Clear();

			//Copy over our values..
			foreach (string key in filters.Keys)
			{
				settings.Add(key, filters[key]);
			}
			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Returns the filter collection to use for a specific display type
		/// </summary>
		/// <param name="displayTypeId">The ID of the display type</param>
		/// <returns>The name of the filter collection</returns>
		private static string GetRevisionFilterCollectionForDisplayType(int? displayTypeId)
		{
			if (displayTypeId.HasValue)
			{
				switch ((Artifact.DisplayTypeEnum)(displayTypeId.Value))
				{
					case Artifact.DisplayTypeEnum.SourceCodeFile_Revisions:
						return SETTING_MAIN_FILE_DETAILS_COMMITS_FILTER;

					case Artifact.DisplayTypeEnum.Build_Revisions:
						return SETTING_MAIN_BUILD_COMMITS_FILTER;

					case Artifact.DisplayTypeEnum.PullRequest_Revisions:
						return SETTING_MAIN_PULLREQUEST_COMMITS_FILTER;

					default:
						return SETTING_MAIN_FILE_DETAILS_COMMITS_FILTER;
				}
			}
			else
			{
				return SETTING_MAIN_COMMITS_FILTER;
			}
		}

		/// <summary>
		/// Returns the sort/pagination/other collection to use for a specific display type
		/// </summary>
		/// <param name="displayTypeId">The ID of the display type</param>
		/// <returns>The name of the project collection</returns>
		private static string GetRevisionOtherCollectionForDisplayType(int? displayTypeId)
		{
			if (displayTypeId.HasValue)
			{
				switch ((Artifact.DisplayTypeEnum)(displayTypeId.Value))
				{
					case Artifact.DisplayTypeEnum.SourceCodeFile_Revisions:
						return SETTING_MAIN_FILE_DETAILS_COMMITS_OTHER;

					case Artifact.DisplayTypeEnum.Build_Revisions:
						return SETTING_MAIN_BUILD_COMMITS_OTHER;

					case Artifact.DisplayTypeEnum.PullRequest_Revisions:
						return SETTING_MAIN_PULLREQUEST_COMMITS_OTHER;

					default:
						return SETTING_MAIN_FILE_DETAILS_COMMITS_OTHER;
				}
			}
			else
			{
				return SETTING_MAIN_COMMITS_OTHER;
			}
		}

		/// <summary>Records the new setting for the user's filters.</summary>
		/// <param name="userId">The userId to save the filter for.</param>
		/// <param name="projectId">The projectId to save the filter for.</param>
		/// <param name="filters">hashtable of filters.</param>
		/// <param name="displayTypeId">The display type (which page)</param>
		public static void Set_UserFilterRevisions(int userId, int projectId, Hashtable filters, int? displayTypeId)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserFilterRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string filterCollection = GetRevisionFilterCollectionForDisplayType(displayTypeId);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, filterCollection);
			settings.Restore();
			settings.Clear();

			//Copy over our values..
			foreach (string key in filters.Keys)
			{
				settings.Add(key, filters[key]);
			}
			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves the user's current pagnation settings.</summary>
		/// <param name="userid">The user ID to pull the settings for.</param>
		/// <param name="projectId">The project ID to pull the settings for.</param>
		/// <param name="numPerPage">The number of rows per page.</param>
		/// <param name="pageNum">The page that the user has selected.</param>
		public static void Set_UserPagnationFiles(int userId, int projectId, int numPerPage, int pageNum)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserPagnationFiles()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Set our values..
			if (pageNum > -1) settings[SETTING_KEY_CURRPAGE] = pageNum;
			if (numPerPage > -1) settings[SETTING_KEY_ROWSPAGE] = numPerPage;

			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves the user's current pagnation settings.</summary>
		/// <param name="userid">The user ID to pull the settings for.</param>
		/// <param name="projectId">The project ID to pull the settings for.</param>
		/// <param name="numPerPage">The number of rows per page.</param>
		/// <param name="pageNum">The page that the user has selected.</param>
		public static void Set_UserPagnationRevisions(int userId, int projectId, int numPerPage, int pageNum)
		{
			const string METHOD_NAME = CLASS_NAME + "Get_UserPagnationRevisions()";
			Logger.LogEnteringEvent(METHOD_NAME);

			string otherCollection = GetRevisionOtherCollectionForDisplayType(null);
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, otherCollection);
			settings.Restore();

			//Set our values..
			if (pageNum > -1) settings[SETTING_KEY_CURRPAGE] = pageNum;
			if (numPerPage > -1) settings[SETTING_KEY_ROWSPAGE] = numPerPage;

			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Stores the user's selected branch, if any.</summary>
		/// <param name="userId">The user id to save the setting for.</param>
		/// <param name="projectId">The project id to save the setting for.</param>
		public static void Set_UserSelectedSourceFolder(int userId, int projectId, string folderKey)
		{
			const string METHOD_NAME = CLASS_NAME + "Set_UserSelectedSourceFolder()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Pull the collection.
			ProjectSettingsCollection settings = new ProjectSettingsCollection(projectId, userId, SETTING_MAIN_FILES_OTHER);
			settings.Restore();

			//Set our saved setting.
			if (folderKey == null)
			{
				if (settings.ContainsKey(SETTING_KEY_SELECTEDFOLDER))
				{
					settings.Remove(SETTING_KEY_SELECTEDFOLDER);
				}
			}
			else
			{
				settings[SETTING_KEY_SELECTEDFOLDER] = folderKey;
			}
			settings.Save();

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#endregion Store
		#endregion User Settings (Static)
	}

	#region Exception Classes
	/// <summary>This exception is thrown a non-specific source provider error occurs. It's the base class for the other exceptions</summary>
	public class SourceCodeProviderGeneralException : ApplicationException
	{
		public SourceCodeProviderGeneralException()
		{ }

		public SourceCodeProviderGeneralException(string message)
			: base(message)
		{ }

		public SourceCodeProviderGeneralException(string message, Exception inner)
			: base(message, inner)
		{ }
	}

	/// <summary>This exception is thrown when you try and load a version control provider plug-in and it fails to load</summary>
	public class SourceCodeProviderLoadingException : SourceCodeProviderGeneralException
	{
		public SourceCodeProviderLoadingException()
		{ }
		public SourceCodeProviderLoadingException(string message)
			: base(message)
		{ }
		public SourceCodeProviderLoadingException(string message, Exception inner)
			: base(message, inner)
		{ }
	}

	/// <summary>This exception is thrown when you try and authenticate with a version control provider plug-in and it fails to load</summary>
	public class SourceCodeProviderAuthenticationException : SourceCodeProviderGeneralException
	{
		public SourceCodeProviderAuthenticationException()
		{ }
		public SourceCodeProviderAuthenticationException(string message)
			: base(message)
		{ }
		public SourceCodeProviderAuthenticationException(string message, Exception inner)
			: base(message, inner)
		{ }
	}

	/// <summary>This exception is thrown when permission to an artifact is denied</summary>
	public class SourceCodeProviderArtifactPermissionDeniedException : SourceCodeProviderGeneralException
	{
		public SourceCodeProviderArtifactPermissionDeniedException()
		{ }
		public SourceCodeProviderArtifactPermissionDeniedException(string message)
			: base(message)
		{ }
		public SourceCodeProviderArtifactPermissionDeniedException(string message, Exception inner)
			: base(message, inner)
		{ }
	}

	/// <summary>This exception is thrown when there is inconsistent/missing data in the cache and the page needs to refresh</summary>
	public class SourceCodeCacheInvalidException : ApplicationException
	{
		public SourceCodeCacheInvalidException()
		{ }
		public SourceCodeCacheInvalidException(string message)
			: base(message)
		{ }
		public SourceCodeCacheInvalidException(string message, Exception inner)
			: base(message, inner)
		{ }
	}

	#endregion Exception Classes

	#region Helper Classes
	/// <summary>Represents a single source code folder in the system</summary>
	[Serializable]
	public class SourceCodeFolder : IComparable<SourceCodeFolder>
	{
		#region Constructor
		/// <summary>Creates a new class of the SourceCodeFolder.</summary>
		public SourceCodeFolder()
		{
			this.Revisions = new List<int>();
			this.ContainedFiles = new List<int>();
			this.ContainedFolders = new List<int>();
		}
		#endregion Constructor

		#region Properties
		/// <summary>The Session/Cache ID of the folder.</summary>
		public int FolderId
		{ get; set; }

		/// <summary>The full nema of the folder.</summary>
		public string Name
		{ get; set; }

		public string FolderKey
		{ get; set; }

		/// <summary>The ID of the parent folder, if it has one.</summary>
		public int? ParentFolderId
		{ get; set; }

		/// <summary>A list of Revision IDs that this folder was changed/modified in.</summary>
		public List<int> Revisions
		{ get; set; }

		/// <summary>A list of File IDs that are contained within this folder.</summary>
		public List<int> ContainedFiles
		{ get; set; }

		/// <summary>A list of Folder IDs that are contained within this folder.</summary>
		public List<int> ContainedFolders
		{ get; set; }

		/// <summary>Flag indicating whether the current folder is the root folder or not.</summary>
		public bool IsRoot
		{ get; set; }
		#endregion Properties

		#region IComparable Members
		/// <summary>The default sorting order is by name ascending</summary>
		/// <param name="other">The object to compare this against.</param>
		/// <returns>Comparison result against the other object.</returns>
		public int CompareTo(SourceCodeFolder other)
		{
			return Name.CompareTo(other.Name);
		}
		#endregion IComparable Members

		#region Comparison Delegates
		#region Name
		/// <summary>Compares ascending between the revision names.</summary>
		public static Comparison<SourceCodeFolder> NameAscComparison = delegate (SourceCodeFolder obj1, SourceCodeFolder obj2)
		{
			return obj1.Name.CompareTo(obj2.Name);
		};

		/// <summary>Compares descending between the revision names.</summary>
		public static Comparison<SourceCodeFolder> NameDescComparison = delegate (SourceCodeFolder obj1, SourceCodeFolder obj2)
		{
			return obj2.Name.CompareTo(obj1.Name);
		};
		#endregion Name

		#region ID
		/// <summary>Compares ascending between the revision names.</summary>
		public static Comparison<SourceCodeFolder> IDAscComparison = delegate (SourceCodeFolder obj1, SourceCodeFolder obj2)
		{
			return obj1.FolderId.CompareTo(obj2.FolderId);
		};

		/// <summary>Compares descending between the revision names.</summary>
		public static Comparison<SourceCodeFolder> IDDescComparison = delegate (SourceCodeFolder obj1, SourceCodeFolder obj2)
		{
			return obj2.FolderId.CompareTo(obj1.FolderId);
		};
		#endregion Name

		#endregion Comparison Delegates
	}

	/// <summary>Represents a single source code file in the system</summary>
	[Serializable]
	public class SourceCodeFile : IComparable<SourceCodeFile>
	{
		#region Constructor
		/// <summary>Creates a new instance of the class.</summary>
		public SourceCodeFile()
		{
		}
		#endregion Constructor

		#region Properties
		/// <summary>The Session/Cache ID of the file</summary>
		public int FileId
		{ get; set; }

		/// <summary>The full filename</summary>
		public string Name
		{ get; set; }

		/// <summary>The size of the file, in bytes.</summary>
		public int Size
		{ get; set; }

		/// <summary>The name of the file's author</summary>
		public string AuthorName
		{ get; set; }

		/// <summary>The Session.Cache ID of the last revision of the file</summary>
		public int RevisionId
		{ get; set; }

		/// <summary>The name of the last revision of the file</summary>
		public string RevisionName
		{ get; set; }

		/// <summary>The unique key of the last revision of the file</summary>
		public string RevisionKey
		{ get; set; }

		/// <summary>The last update date/time</summary>
		public DateTime LastUpdateDate
		{ get; set; }

		/// <summary>The name of the last action performed on the file</summary>
		public string Action
		{ get; set; }

		/// <summary>The file's unique key.</summary>
		public string FileKey
		{ get; set; }

		/// <summary>The file's path (usually the connection string + any sub direcetories.</summary>
		/// <remarks>
		/// Not currently used since the FileKey contains the important part of the path
		/// </remarks>
		public string Path
		{ get; set; }

		/// <summary>The name of the linked artifact, only used by RetrieveFilesForArtifact()</summary>
		/// <see cref="RetrieveFilesForArtifact"/>
		public int? ArtifactSourceCodeId
		{ get; set; }

		/// <summary>The ID of the parent folder, if it has one.</summary>
		public int? ParentFolderId
		{ get; set; }

		#endregion Properties

		#region IComparable Members
		/// <summary>The default sorting order is by date ascending</summary>
		/// <param name="other">The object to compare this against.</param>
		/// <returns>Comparison result against the other object.</returns>
		public int CompareTo(SourceCodeFile other)
		{
			return this.Name.CompareTo(other.Name);
		}
		#endregion IComparable Members

		#region Comparison Delegates
		#region Name
		/// <summary>Compares ascending between the file names.</summary>
		public static Comparison<SourceCodeFile> NameAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			return obj1.Name.CompareTo(file2.Name);
		};

		/// <summary>Compares descending between the file names.</summary>
		public static Comparison<SourceCodeFile> NameDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			return file2.Name.CompareTo(obj1.Name);
		};
		#endregion Name

		#region Size
		/// <summary>Compares ascending between file sizes.</summary>
		public static Comparison<SourceCodeFile> SizeAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = obj1.Size.CompareTo(file2.Size);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};

		/// <summary>Compares descending between file sizes.</summary>
		public static Comparison<SourceCodeFile> SizeDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = file2.Size.CompareTo(obj1.Size);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};
		#endregion Size

		#region Revision
		/// <summary>Compares ascending between the Revision/Commit names.</summary>
		/// <remarks>Fallback to sorting by ID to provide stable results</remarks>
		public static Comparison<SourceCodeFile> RevisionAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = obj1.RevisionName.CompareTo(file2.RevisionName);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};

		/// <summary>Compares descending between the Revision/Commit keys.</summary>
		/// <remarks>Fallback to sorting by ID to provide stable results</remarks>
		public static Comparison<SourceCodeFile> RevisionDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = file2.RevisionName.CompareTo(obj1.RevisionName);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};
		#endregion Revision

		#region Author
		/// <summary>Compares ascending between the Author names.</summary>
		public static Comparison<SourceCodeFile> AuthorAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = obj1.AuthorName.CompareTo(file2.AuthorName);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};

		/// <summary>Compares descending between the Author names.</summary>
		public static Comparison<SourceCodeFile> AuthorDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = file2.AuthorName.CompareTo(obj1.AuthorName);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};
		#endregion Author

		#region LastUpdated
		/// <summary>Compares ascending between the Last Updated date.</summary>
		public static Comparison<SourceCodeFile> LastUpdatedAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = obj1.LastUpdateDate.CompareTo(file2.LastUpdateDate);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};

		/// <summary>Compares descending between the Last Updated date.</summary>
		public static Comparison<SourceCodeFile> LastUpdatedDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = file2.LastUpdateDate.CompareTo(obj1.LastUpdateDate);
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};
		#endregion LastUpdated

		#region Action
		/// <summary>Compares ascending between the item's 'Action'.</summary>
		/// <remarks>Fallback to sorting by ID to provide stable results</remarks>
		public static Comparison<SourceCodeFile> ActionAscComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = obj1.Action.ToString().CompareTo(file2.Action.ToString());
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};

		/// <summary>Compares descending between the item's 'Action'.</summary>
		/// <remarks>Fallback to sorting by ID to provide stable results</remarks>
		public static Comparison<SourceCodeFile> ActionDescComparison = delegate (SourceCodeFile obj1, SourceCodeFile file2)
		{
			int ret = file2.Action.ToString().CompareTo(obj1.Action.ToString());
			if (ret == 0)
			{
				ret = obj1.FileId.CompareTo(file2.FileId);
			}
			return ret;
		};
		#endregion Action
		#endregion Comparison Delegates
	}

	[Serializable]
	[XmlRoot(ElementName = "cache")]
	public class SourceCodeCache
	{
		#region Constructor
		/// <summary>Creates a new instance of the class.</summary>
		public SourceCodeCache()
		{
			this.Folders = new List<SourceCodeFolder>();
			this.Files = new List<SourceCodeFile>();
		}
		#endregion Constructor

		#region Properties
		/// <summary>The cached date/time of this data.</summary>
		[XmlAttribute(AttributeName = "date")]
		public DateTime CacheDate
		{ get; set; }

		/// <summary>The name of the VersionControlProvider.</summary>
		[XmlAttribute(AttributeName = "source")]
		public string VersionProvider
		{ get; set; }

		/// <summary>the branch name of this branch.</summary>
		[XmlAttribute(AttributeName = "branch")]
		public string BranchKey
		{ get; set; }

		/// <summary>the branch name of this branch.</summary>
		[XmlAttribute(AttributeName = "isdefault")]
		public bool IsBranchDefault
		{ get; set; }

		/// <summary>The highest ID (in this branch) of FolderIDs</summary>
		[XmlAttribute(AttributeName = "id1")]
		public int HighestFolderId
		{ get; set; }

		/// <summary>The highest ID (in this branch) of FileIDs</summary>
		[XmlAttribute(AttributeName = "id2")]
		public int HighestFileId
		{ get; set; }

		/// <summary>List of the folders contained within this system.</summary>
		[XmlElement(ElementName = "folders")]
		public List<SourceCodeFolder> Folders
		{ get; set; }

		/// <summary>List of the folders contained within this system.</summary>
		[XmlElement(ElementName = "files")]
		public List<SourceCodeFile> Files
		{ get; set; }

		/*
        /// <summary>List of the commits/revisions contained within this system.</summary>
        [XmlElement(ElementName = "commits")]
        public List<SourceCodeCommit> Commits
        { get; set; }*/
		#endregion Properties
	}

	/// <summary>Contains Branch information from the SourceCodeProvider.</summary>
	public class SourceCodeBranch
	{
		/// <summary>The unique key for the branch.</summary>
		public string BranchKey
		{ get; set; }

		/// <summary>Flag indicating whether the brnch is default or not.</summary>
		public bool IsDefault
		{ get; set; }
	}

	/// <summary>Represents the underlying data behind a single file</summary>
	public class SourceCodeFileStream
	{
		/// <summary>Constructor with no initial values. (Defaults IsRedirect to False)</summary>
		public SourceCodeFileStream()
		{
			this.IsRedirect = false;
		}

		/// <summary>Constructor with initial values specified</summary>
		public SourceCodeFileStream(string fileKey, string revisionKey, string localPath, Stream dataStream, bool isRedirect)
		{
			this.FileKey = fileKey;
			this.RevisionKey = revisionKey;
			this.LocalPath = localPath;
			this.DataStream = dataStream;
			this.IsRedirect = isRedirect;
		}

		#region Properties
		/// <summary>The id of the file</summary>
		public string FileKey
		{ get; set; }

		/// <summary>The id of the revision</summary>
		public string RevisionKey
		{ get; set; }

		/// <summary>Where the file is locally stored (internal to provider)</summary>
		public string LocalPath
		{ get; set; }

		/// <summary>The data behind the file</summary>
		public Stream DataStream
		{ get; set; }

		/// <summary>True of the 'LocalPath' is a redirect to another net-based URL.</summary>
		public Boolean IsRedirect
		{ get; set; }

		/// <summary>The branch that this key belongs to.</summary>
		public string BranchKey
		{ get; set; }
		#endregion
	}

	/// <summary>Contains setting information.</summary>
	public class SourceCodeSettings
	{
		/// <summary>The name (dll name) of the provider.</summary>
		public string ProviderName;
		/// <summary>Connection string of the repository.</summary>
		public string Connection;
		/// <summary>Login details for the specified repository.</summary>
		public NetworkCredential Credentials;
		/// <summary>Any additional connection parameters.</summary>
		public Dictionary<string, string> Parameters;
		/// <summary>The EventLog object to record messages to.</summary>
		public EventLog EventLog;
		/// <summary>Custom 1 Setting</summary>
		public string Custom01;
		/// <summary>Custom 2 Setting</summary>
		public string Custom02;
		/// <summary>Custom 3 Setting</summary>
		public string Custom03;
		/// <summary>Custom 4 Setting</summary>
		public string Custom04;
		/// <summary>Custom 5 Setting</summary>
		public string Custom05;

		/// <summary>Creates new instance of the class.</summary>
		public SourceCodeSettings()
		{
			this.Credentials = new NetworkCredential();
			this.Parameters = new Dictionary<string, string>();
			this.EventLog = null;
		}

		/// <summary>Creates a new instance of the class with the specified values.</summary>
		/// <param name="connection">Connection string for the repository.</param>
		/// <param name="credentials">Credentials to use for logging in to the repository.</param>
		/// <param name="parameters">Any additional connection parameters.</param>
		/// <param name="eventLog">The EventLog, for logging messages.</param>
		/// <param name="custom01">Custom 1 Setting</param>
		/// <param name="custom02">Custom 2 Setting</param>
		/// <param name="custom03">Custom 3 Setting</param>
		/// <param name="custom04">Custom 4 Setting</param>
		/// <param name="custom05">Custom 5 Setting</param>
		/// <param name="providerName">The name of the provider. Usually the DLL's name.</param>
		public SourceCodeSettings(
			string providerName,
			string connection,
			NetworkCredential credentials,
			Dictionary<string, string> parameters,
			EventLog eventLog,
			string custom01,
			string custom02,
			string custom03,
			string custom04,
			string custom05)
		{
			this.ProviderName = providerName;
			this.Connection = connection;
			this.Credentials = credentials;
			this.Parameters = parameters;
			this.EventLog = eventLog;
			this.Custom01 = custom01;
			this.Custom02 = custom02;
			this.Custom03 = custom03;
			this.Custom04 = custom04;
			this.Custom05 = custom05;
		}
	}
	#endregion Helper Classes
}
