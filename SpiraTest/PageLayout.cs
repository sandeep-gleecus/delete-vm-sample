using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This is the base class for all the application web pages that use the Main.Master master page
	/// </summary>
	public class PageLayout : PageBase
	{
		private const string CLASS_NAME = "Web.PageLayout::";

		//Member variables
		protected Exception lastPageError;

		#region Properties

		/// <summary>
		/// Contains the last error raised by the page
		/// </summary>
		public Exception LastPageError
		{
			get
			{
				return this.lastPageError;
			}
			set
			{
				this.lastPageError = value;
			}
		}

		/// <summary>
		/// Contains the id of the current project
		/// </summary>
		public int ProjectId
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && context.ProjectId.HasValue)
				{
					return context.ProjectId.Value;
				}
				return -1;
			}
		}

		/// <summary>
		/// Contains the id of the current project template
		/// </summary>
		public int ProjectTemplateId
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && context.ProjectTemplateId.HasValue)
				{
					return context.ProjectTemplateId.Value;
				}
				return -1;
			}
		}

		/// <summary>
		/// Contains the name of the current project template
		/// </summary>
		public string ProjectTemplateName
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && !String.IsNullOrEmpty(context.ProjectTemplateName))
				{
					return context.ProjectTemplateName;
				}
				return "";
			}
		}

		/// <summary>
		/// Contains the id of the current project group (if any)
		/// </summary>
		public int? ProjectGroupId
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.ProjectGroupId;
				}
				return null;
			}
		}

        /// <summary>
        /// Contains the id of the current portfolio (or -1 of not set)
        /// </summary>
        public int PortfolioId
        {
            get
            {
                SpiraContext context = SpiraContext.Current;
                if (context != null && context.PortfolioId.HasValue)
                {
                    return context.PortfolioId.Value;
                }
                return -1;
            }
        }

        /// <summary>
        /// Contains the id of the current project role
        /// </summary>
        public int ProjectRoleId
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && context.ProjectRoleId.HasValue)
				{
					return context.ProjectRoleId.Value;
				}
				return -1;
			}
		}

		/// <summary>
		/// Contains the name of the current project
		/// </summary>
		public string ProjectName
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && !String.IsNullOrEmpty(context.ProjectName))
				{
					return context.ProjectName;
				}
				return "";
			}
		}

		/// <summary>
		/// Contains the name of the current project role
		/// </summary>
		public string ProjectRoleName
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null && !String.IsNullOrEmpty(context.ProjectRoleName))
				{
					return context.ProjectRoleName;
				}
				return "";
			}
		}

		/// <summary>
		/// Contains the rss token of the current user (or null if no token)
		/// </summary>
		public string UserRssToken
		{
			get
			{
				//The RSS token is not part of the default membership system so we need to access the Spira provider directly
				SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
				MembershipUser membershipUser = Membership.GetUser();
				DataModel.User user = provider.GetProviderUser(membershipUser.UserName);
				if (user != null)
				{
					return user.RssToken;
				}
				return "";
			}
		}

        /// <summary>
        /// Contains the id of the current user (-1 if not authenticated, which shouldn't happen in reality)
        /// COPIED TO MASTERPAGEBASE so make sure to change there too as needed
        /// </summary>
        public int UserId
		{
			get
			{
				MembershipUser user = Membership.GetUser();
				if (user != null)
				{
					return (int)user.ProviderUserKey;
				}
				return -1;
			}
		}

		/// <summary>
		/// Contains the full name of the current user
		/// </summary>
		/// <remarks>
		/// The data is held in the user's profile
		/// </remarks>
		public string UserFullName
		{
			get
			{
				return new ProfileEx().FullName;
			}
		}

		/// <summary>
		/// Contains the login name of the current user
		/// </summary>
		public string UserName
		{
			get
			{
				MembershipUser user = Membership.GetUser();
				if (user != null)
				{
					return user.UserName;
				}
				return "";
			}
		}

		/// <summary>
		/// Stores whether the current user is a system admin or not
		/// </summary>
		public bool UserIsAdmin
		{
			get
			{
				return new ProfileEx().IsAdmin;
			}
		}

        /// <summary>
        /// Stores whether the current user is a portfolio viewer
        /// </summary>
        /// <remarks>
        /// Also returns false if the system does not support portfolios
        /// </remarks>
        public bool UserIsPortfolioViewer
        {
            get
            {
                ProfileEx profile = new ProfileEx();
                return profile.IsPortfolioAdmin && Common.Global.Feature_Portfolios;
            }
        }

		/// <summary>
		/// Stores whether the current user is a report admin
		/// </summary>
		public bool UserIsReportAdmin
		{
			get
			{
				ProfileEx profile = new ProfileEx();
				return profile.IsReportAdmin;
			}
		}

		/// <summary>
		/// Stores whether the current user is a an admin of the current project or not
		/// </summary>
		public bool UserIsProjectAdmin
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.IsProjectAdmin;
				}
				return false;
			}
		}

		/// <summary>Stores whether the current user is a project group admin or not</summary>
		public bool UserIsGroupAdmin
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.IsGroupAdmin;
				}
				return false;
			}
		}

		/// <summary>
		/// Stores whether the user is a member of the current project's group
		/// </summary>
		public bool UserIsGroupMember
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.IsGroupMember;
				}
				return false;
			}
		}

		/// <summary>Stores whether the user is a template admin</summary>
		public bool UserIsTemplateAdmin
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.IsTemplateAdmin;
				}
				return false;
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Web Forms should not have any code inside their constructors
		/// since the OnInit event handler sets up the page
		/// </summary>
		public PageLayout()
					: base()
		{
			//Do Nothing
		}

		#endregion

		#region Methods Called From WebForm

		/// <summary>
		/// Returns the permission level that a user has for a specific artifact/project/permission
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="permission">The permission level</param>
		/// <returns>Authorized, Limited or Prohibited</returns>
		/// <remarks>
		/// 1) It relies on SpiraContext being available and populated
		/// 2) To determine if someone has limited modify, you check for Modify and verify the state back, don't pass in LimitedModify as the permission
		/// </remarks>
		protected Project.AuthorizationState IsAuthorized(Artifact.ArtifactTypeEnum artifactType, Project.PermissionEnum permission)
		{
			bool isSystemAdmin = UserIsAdmin;
			bool isGroupAdmin = UserIsGroupAdmin;
			int projectRoleId = ProjectRoleId;

			//First handle the special case that we require a system administrator
			//This takes affect regardless of the artifact type or role id
			if (permission == Project.PermissionEnum.SystemAdmin)
			{
				if (isSystemAdmin)
					return Project.AuthorizationState.Authorized;
				else
					return Project.AuthorizationState.Prohibited;
			}

			//Now handle the case of a Project Group Admin
			if (permission == Project.PermissionEnum.ProjectGroupAdmin)
			{
				//See if the user is either a system admin or a project group admin
				if (isSystemAdmin || isGroupAdmin)
				{
					return Project.AuthorizationState.Authorized;
				}
				else
				{
					return Project.AuthorizationState.Prohibited;
				}
			}

			//For the other cases we need to access the project role permissions
			Project.AuthorizationState authorized = Project.AuthorizationState.Prohibited;

			//If we have a placeholder artifact, the permission is taken from the Incident artifact type
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Placeholder)
			{
				artifactType = DataModel.Artifact.ArtifactTypeEnum.Incident;
			}

			//First see if the user has the appropriate standard permissions from his/her role
			Business.ProjectManager projectManager = new Business.ProjectManager();
			authorized = projectManager.IsAuthorized(projectRoleId, artifactType, permission);

			//Now if the user is a system administrator, this automatically authorizes him/her for all other permissions
			if (isSystemAdmin)
			{
				authorized = Project.AuthorizationState.Authorized;
			}
			return authorized;
		}

		/// <summary>
		/// Verifies that the current project in session matches the current artifact
		/// </summary>
		/// <remarks>
		/// Prevents spoofed URLs being used to bypass security and also to handle the situation
		/// when a user clicks on a notification URL and accesses an artifact from a different
		/// project to the current one
		/// </remarks>
		protected void VerifyArtifactProject(UrlRoots.NavigationLinkEnum navigationLink, int artifactId)
		{
			//First we need to get the project from the artifact
			ArtifactInfo artifactInfo = new ArtifactManager().RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)navigationLink, artifactId, null);
			if (artifactInfo != null && artifactInfo.ProjectId.HasValue)
			{
				VerifyArtifactProject(artifactInfo.ProjectId.Value, navigationLink, artifactId);
			}
		}

		/// <summary>
		/// Verifies that the current project in session matches the current artifact
		/// </summary>
		/// <param name="artifactProjectId">The project ID that the artifact belongs to</param>
		/// <remarks>
		/// Prevents spoofed URLs being used to bypass security and also to handle the situation
		/// when a user clicks on a notification URL and accesses an artifact from a different
		/// project to the current one
		/// </remarks>
		protected void VerifyArtifactProject(int artifactProjectId, UrlRoots.NavigationLinkEnum navigationLink, int artifactId)
		{
			int sessionProjectId = ProjectId;
			int userId = UserId;
			if (artifactProjectId != sessionProjectId)
			{
				//Since the current project in session doesn't match, we need to verify that the user is
				//authorized to access the project that the artifact belongs to
				Business.ProjectManager projectManager = new Business.ProjectManager();
				ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(artifactProjectId, userId);
				if (projectUser != null)
				{
					//Get the new rewritten URL to redirect them to
					if (navigationLink == UrlRoots.NavigationLinkEnum.None)
					{
						//There was no found artifact type. Redirect them to the home page.
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, artifactProjectId), true);
					}
					else
					{
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(navigationLink, artifactProjectId, artifactId), true);
					}
				}
				else
				{
					//Redirect to the user's home page instead since they're not authorized
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, artifactProjectId, 0), true);
				}
			}
		}

		#endregion

		#region Methods used internally by the derived classes

		/// <summary>Gets a collection of user settings for the current user - COPIED TO MASTERPAGEBASE so make sure to change there too as needed</summary>
		/// <param name="collectionName">The name of the collection to retrieve.</param>
		/// <returns>A UserSettingsCollection</returns>
		/// <remarks>1) The user id is obtained from session, will throw exception if not
		/// 2) The collection is returned from session (if available) otherwise restored from the database</remarks>
		protected UserSettingsCollection GetUserSettings(string collectionName)
		{
			//First get the user id from session
			if (UserId < 1)
			{
				throw new Exception("User Id Not Available");
			}
			UserSettingsCollection userSettingsCollection;

			//Now see if we have this setting already in context
			if (Context.Items[collectionName] == null)
			{
				//Restore settings from database and put in session
				userSettingsCollection = new UserSettingsCollection(UserId, collectionName);
				userSettingsCollection.Restore();
				Context.Items[collectionName] = userSettingsCollection;
				return userSettingsCollection;
			}
			//Restore from context
			userSettingsCollection = (UserSettingsCollection)Context.Items[collectionName];

			//return the copy from session
			return userSettingsCollection;
		}

        /// <summary>
        /// Gets a simple (i.e. single entry) string user setting
        /// COPIED TO MASTERPAGEBASE so make sure to change there too as needed
        /// </summary>
        /// <param name="collectionName">The name of the user setting</param>
        /// <param name="entryKey">The name of the setting key</param>
        /// <returns>The value of the key, or "" if not set</returns>
        protected string GetUserSetting(string collectionName, string entryKey)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			if (userSettingsCollection[entryKey] == null)
			{
				return "";
			}
			return (userSettingsCollection[entryKey].ToString());
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected int GetUserSetting(string collectionName, string entryKey, int defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			if (userSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((int)userSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="defaultValue">The default value</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected string GetUserSetting(string collectionName, string entryKey, string defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			if (userSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((string)userSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) boolean user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected bool GetUserSetting(string collectionName, string entryKey, bool defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			if (userSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((bool)userSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a collection of project settings for the current user/project
		/// </summary>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		/// <remarks>
		/// 1) The user id and project id are obtained from session, will throw exception if not
		/// 2) The collection is returned from session (if available) otherwise restored from the database
		/// </remarks>
		protected ProjectSettingsCollection GetProjectSettings(string collectionName)
		{
			//First get the project id and user id from session
			if (UserId < 0)
			{
				throw new Exception("User Id Not Available");
			}
			if (ProjectId < 1)
			{
				throw new Exception("Current Project Id Not Stored in Context");
			}
			ProjectSettingsCollection projectSettingsCollection;

			//Now see if we have this setting already in context
			if (Context.Items[collectionName] == null)
			{
				//Restore settings from database and put in session
				projectSettingsCollection = new ProjectSettingsCollection(ProjectId, UserId, collectionName);
				projectSettingsCollection.Restore();
				return projectSettingsCollection;
			}
			//Restore from session
			projectSettingsCollection = (ProjectSettingsCollection)Context.Items[collectionName];

			//Make sure that the project id's match, if not, need to do a fresh restore from the database
			if (projectSettingsCollection.ProjectId == ProjectId)
			{
				//return the copy from session
				return projectSettingsCollection;
			}
			else
			{
				//Restore settings from database and put in session
				projectSettingsCollection = new ProjectSettingsCollection(ProjectId, UserId, collectionName);
				projectSettingsCollection.Restore();
				Context.Items[collectionName] = projectSettingsCollection;
				return projectSettingsCollection;
			}
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) string project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(string collectionName, string entryKey, string entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			projectSettingsCollection[entryKey] = entryValue;
			projectSettingsCollection.Save();
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(string collectionName, string entryKey, int entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			projectSettingsCollection[entryKey] = entryValue;
			projectSettingsCollection.Save();
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) integer user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveUserSetting(string collectionName, string entryKey, int entryValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			userSettingsCollection[entryKey] = entryValue;
			userSettingsCollection.Save();
		}

        /// <summary>
        /// Saves a simple (i.e. single entry) datetime user setting
        /// </summary>
        /// <param name="collectionName">The name of the user setting</param>
        /// <param name="entryKey">The name of the setting key</param>
        /// <param name="entryValue">The value to set it to</param>
        protected void SaveUserSetting(string collectionName, string entryKey, DateTime entryValue)
        {
            UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
            userSettingsCollection[entryKey] = entryValue;
            userSettingsCollection.Save();
        }

        /// <summary>
        /// Saves a simple (i.e. single entry) boolean user setting
        /// </summary>
        /// <param name="collectionName">The name of the user setting</param>
        /// <param name="entryKey">The name of the setting key</param>
        /// <param name="entryValue">The value to set it to</param>
        protected void SaveUserSetting(string collectionName, string entryKey, bool entryValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			userSettingsCollection[entryKey] = entryValue;
			userSettingsCollection.Save();
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) string user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveUserSetting(string collectionName, string entryKey, string entryValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			userSettingsCollection[entryKey] = entryValue;
			userSettingsCollection.Save();
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) string project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or "" if not set</returns>
		protected string GetProjectSetting(string collectionName, string entryKey)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return "";
			}
			return (projectSettingsCollection[entryKey].ToString());
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected string GetProjectSetting(string collectionName, string entryKey, string defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((string)projectSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected int GetProjectSetting(string collectionName, string entryKey, int defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((int)projectSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) boolean project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected bool GetProjectSetting(string collectionName, string entryKey, bool defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((bool)projectSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Called when an error is raised during the execution of a page
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnError(EventArgs e)
		{
			//Extract the last error
			Exception exception = Server.GetLastError();

			//Store the error in a public property so that the next page can access it
			this.LastPageError = exception;

			//Log the error first
			Logger.LogErrorEvent("PageLayout:: OnError", exception);
			Logger.Flush();

			//If we have partial postback, need to handle differently as we can't use Server.Transfer
			if (ScriptManager.GetCurrent(this) != null && ScriptManager.GetCurrent(this).IsInAsyncPostBack)
			{
				//Redirect to the error page - handle request validation errors differently
				if (exception.GetType() == typeof(HttpRequestValidationException))
				{
					Server.ClearError();
					Response.Redirect(this.Request.RawUrl, true);
				}
				else
				{
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ErrorPage), true);
				}
			}
			else
			{
				//Redirect to the error page - handle request validation errors differently
				if (exception.GetType() == typeof(HttpRequestValidationException))
				{
					Server.ClearError();
					Response.Redirect(this.Request.RawUrl, true);
				}
				else
				{
					Server.Transfer(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ErrorPage), true);
				}
			}
		}

		/// <summary>
		/// Checks to see if there was request validation exception raised that was trapped
		/// </summary>
		/// <returns>The exception</returns>
		protected Exception CheckLastError()
		{
			//Try and get the error information from the calling page
			Exception lastException = null;
			try
			{
				Inflectra.SpiraTest.Web.PageLayout lastPage = (Inflectra.SpiraTest.Web.PageLayout)Context.Handler;
				lastException = lastPage.LastPageError;
			}
			catch (InvalidCastException)
			{
				//Fail quietly
			}
			return lastException;
		}

		/// <summary>
		/// Redirects to the login page, adding the original URL to the querystring
		/// </summary>
		private void RedirectToLoginPage()
		{
			string originalUrl = Request.QueryString[GlobalFunctions.PARAMETER_LOGIN_RETURN_URL];
			string url = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login, 0, 0);
			//Make sure we only have a single return URL
			if (String.IsNullOrEmpty(originalUrl))
			{
				//Make sure the original URL is not the Login page
				if (!Request.RawUrl.Contains("Login.aspx"))
				{
					url += "?" + GlobalFunctions.PARAMETER_LOGIN_RETURN_URL + "=" + Server.UrlEncode(Request.RawUrl);
				}
			}
			else
			{
				url += "?" + GlobalFunctions.PARAMETER_LOGIN_RETURN_URL + "=" + originalUrl;
			}
			Response.Redirect(url, true);
		}

		/// <summary>
		/// Initializes the project-level data for the current request
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreInit(EventArgs e)
		{
			const string METHOD_NAME = "OnPreInit";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			base.OnPreInit(e);

			//Next make sure that we have the project/program info in SpiraContext
			PutProjectInContext();
		}

		/// <summary>
		/// Logs when the pre-render is done (used in debugging)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRenderComplete(EventArgs e)
		{
			const string METHOD_NAME = "OnPreRenderComplete";

			if (this.Request != null && this.Request.Url != null)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, this.Request.Url.AbsoluteUri);
			}
			base.OnPreRenderComplete(e);
		}

		/// <summary>
		/// Logs when the load is done (used in debugging)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoadComplete(EventArgs e)
		{
			const string METHOD_NAME = "OnLoadComplete";

			if (this.Request != null && this.Request.Url != null)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, this.Request.Url.AbsoluteUri);
			}
			base.OnLoadComplete(e);
		}

		/// <summary>
		/// Logs when the save state is done (used in debugging)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSaveStateComplete(EventArgs e)
		{
			const string METHOD_NAME = "OnSaveStateComplete";

			if (this.Request != null && this.Request.Url != null)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, this.Request.Url.AbsoluteUri);
			}
			base.OnSaveStateComplete(e);
		}

		/// <summary>
		/// This is the default functionality for all pages when they load
		/// </summary>
		/// <param name="e">The page load event arguments</param>
		/// <remarks>Currently used to make sure that user is authenticated and authorized</remarks>
		protected override void OnLoad(EventArgs e)
		{
			const string METHOD_NAME = "OnLoad";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we're authenticated
            if (!Page.User.Identity.IsAuthenticated)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "User Not Authenticated. Redirecting to Login Page.");
				Session.Abandon();
				RedirectToLoginPage();
			}

            //Next make sure the user is actually a valid user (causes endless unhandled loop if not)
            if (UserId < 1)
            {
                Session.Abandon();
                RedirectToLoginPage();
            }

			//Enable scroll position rememebering on pages that inherit from this class
			this.MaintainScrollPositionOnPostBack = true;

			//Make sure we don't have multiple sessions open for the current user
			//We don't do if we have a license of 50+ concurrent users
			DetectMultipleUserSessions();

			//Make Sure we have a valid server license - except on Administration License Details Page since that allows you to change the license
			if (Page.GetType().ToString().ToLowerInvariant() != "asp.administration_licensedetails_aspx")
			{
				//Check to see that some kind of license is loaded
				if (Common.License.LicenseType == LicenseTypeEnum.None || Common.License.LicenseProductName == LicenseProductNameEnum.None)
				{
					//Redirect to the error page
					Session.Abandon();
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.InvalidLicense), true);
				}
				//If we have a concurrent or demo license then check that we've not exceeded the # user sessions
				if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					DetectExceededConcurrentUsers(Common.License.Number);
				}
                //If we have a non-perpetual license then check that it has not expired
				if (Common.License.Expiration.HasValue && Common.License.Expiration < DateTime.UtcNow)
				{
					//Redirect to the error page
					Session.Abandon();
					//Send a different param to the URL query depending on the license type
					string licenseErrorType = Common.License.LicenseType == LicenseTypeEnum.Demonstration ? "demonstrationexpired" : "licensenexpired";
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.InvalidLicense) + "?" + GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE + "=" + licenseErrorType, true);
				}
			}

			//Finally call the base class
			base.OnLoad(e);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Overrides the Application's culture setting if the user has explicitly specified one
		/// </summary>
		/// <remarks>The user's profile is taken into account in the overriding class in PageLayout</remarks>
		protected override void InitializeCulture()
		{
			const string METHOD_NAME = "InitializeCulture";

			base.InitializeCulture();

			try
			{
				//See if we have a user profile culture set
				string userCulture = GetUserSetting(GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE, "");
				if (!String.IsNullOrEmpty(userCulture))
				{
					this.UICulture = userCulture;
					this.Culture = userCulture;
				}
				//See if we have a user profile timezone set
				if (!String.IsNullOrEmpty(Profile.Timezone))
				{
					SpiraContext.Current.TimezoneId = Profile.Timezone;
				}
			}
			catch (Exception exception)
			{
				//Log but don't throw (e.g. user id doesn't exist)
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Makes sure that we have the project, project template and program info in SpiraContext
		/// </summary>
		protected void PutProjectInContext()
		{
			//See if we have the case of a group admin
			Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
			SpiraContext.Current.IsGroupAdmin = projectGroupManager.IsAdmin(UserId);

			Business.ProjectManager projectManager = new Business.ProjectManager();

			#region Project

			//First see if we have the project id specified in the querystring
			if (String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID]))
			{
				//See if the user has a project specified in their profile
				if (Profile.LastOpenedProjectId.HasValue)
				{
					int projectId = Profile.LastOpenedProjectId.Value;
					//Make sure this project actually exists
					try
					{
						ProjectView project = projectManager.RetrieveById2(projectId);
						//Make sure project is active
						if (!project.IsActive || !new UserManager().Authorize(UserId, projectId))
						{
							//Remove the active project, and redirect again.
							Profile.LastOpenedProjectId = null;
							Profile.Save();

							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Put the project info into the current request context
						SpiraContext.Current.ProjectId = projectId;
						SpiraContext.Current.ProjectName = project.Name;

						//Set the workspace type here to the enterprise to act as default, that may be overridden below
						SpiraContext.Current.WorkspaceType = (int)Workspace.WorkspaceTypeEnum.Enterprise;

						//Put template in context if no template specified in the URL
						if (String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_TEMPLATE_ID]))
						{
							SpiraContext.Current.ProjectTemplateId = project.ProjectTemplateId;
						}

						//If no group ID in Querystring, also set the group info
						//If an ID is specified, the next section (Program) handles it
						if (String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_GROUP_ID]))
						{
							SpiraContext.Current.ProjectGroupId = project.ProjectGroupId;
							SpiraContext.Current.ProjectGroupName = project.ProjectGroupName;
							SpiraContext.Current.IsGroupMember = projectGroupManager.IsAuthorized(UserId, project.ProjectGroupId);
						}

						//Retrieve the project role that the user has
						ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, UserId);
						if (projectUser != null)
						{
							SpiraContext.Current.ProjectRoleId = projectUser.ProjectRoleId;
							SpiraContext.Current.ProjectRoleName = projectUser.ProjectRoleName;
							SpiraContext.Current.IsProjectAdmin = projectUser.IsAdmin;
							SpiraContext.Current.IsTemplateAdmin = projectUser.IsTemplateAdmin;
							SpiraContext.Current.CanUserAddCommentToArtifacts = projectManager.IsAuthorizedToAddComment(SpiraContext.Current.ProjectRoleId.Value);

							//Add/Update the recently accessed project list
							projectManager.AddUpdateRecentProject(UserId, projectId);
                        }
					}
					catch (ArtifactNotExistsException)
					{
						//The project no longer exists

						//Redirect to MyPage with no project in the URL
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
					}
				}
				else
				{
					if (RequiresProjectSpecified())
					{
						//Redirect to MyPage with no project in the URL
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
					}
				}
			}
			else
			{
				int projectId;
				if (Int32.TryParse(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_ID], out projectId))
				{
					//Make sure this project actually exists
					try
					{
						ProjectView project = projectManager.RetrieveById2(projectId);
						//Make sure project is active
						if (!project.IsActive && !this.UserIsAdmin)
						{
							//Redirect to MyPage with no project in the URL
							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Make sure the user is authorized
						if (!new UserManager().Authorize(UserId, projectId))
						{
							//Redirect to MyPage with no project in the URL
							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Put the project info into the current request context
						SpiraContext.Current.ProjectId = projectId;
                        SpiraContext.Current.ProjectTemplateId = project.ProjectTemplateId;
                        SpiraContext.Current.ProjectTemplateName = project.ProjectTemplateName;
                        SpiraContext.Current.ProjectName = project.Name;
						SpiraContext.Current.WorkspaceType = (int)Workspace.WorkspaceTypeEnum.Product;

						//If no group ID in Querystring, also set the group info
						//If an ID is specified, the next section (Program) handles it
						if (String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_GROUP_ID]))
						{
							SpiraContext.Current.ProjectGroupId = project.ProjectGroupId;
							SpiraContext.Current.ProjectGroupName = project.ProjectGroupName;
							SpiraContext.Current.IsGroupMember = projectGroupManager.IsAuthorized(UserId, project.ProjectGroupId);
						}

						//Retrieve the project role that the user has
						ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, UserId);
						if (projectUser != null)
						{
							SpiraContext.Current.ProjectRoleId = projectUser.ProjectRoleId;
							SpiraContext.Current.ProjectRoleName = projectUser.ProjectRoleName;
							SpiraContext.Current.IsProjectAdmin = projectUser.IsAdmin;
							SpiraContext.Current.IsTemplateAdmin = projectUser.IsTemplateAdmin;
							SpiraContext.Current.CanUserAddCommentToArtifacts = projectManager.IsAuthorizedToAddComment(SpiraContext.Current.ProjectRoleId.Value);

							//Add/Update the recently accessed project list
							projectManager.AddUpdateRecentProject(UserId, projectId);
						}

						//See if this matches the last opened id in the user's profile
						if (!Profile.LastOpenedProjectId.HasValue || Profile.LastOpenedProjectId.Value != projectId)
						{
							Profile.LastOpenedProjectId = SpiraContext.Current.ProjectId;
							Profile.Save();
						}
					}
					catch (ArtifactNotExistsException)
					{
						//The project no longer exists

						//Redirect to MyPage with no project in the URL
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
					}
				}
			}

			#endregion

			#region Program

			//First see if we have the project group id specified in the querystring
			if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_GROUP_ID]))
			{
				int projectGroupId;
				if (Int32.TryParse(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_GROUP_ID], out projectGroupId))
				{
					//Make sure this program actually exists
					try
					{
						ProjectGroup projectGroup = projectGroupManager.RetrieveById(projectGroupId);
						//Make sure project group is active or the user is an admin
						if (!projectGroup.IsActive && !this.UserIsAdmin && !projectGroupManager.IsAdmin(UserId, projectGroupId))
						{
							//Redirect to MyPage with no project in the URL
							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Make sure the user is authorized
						if (!this.UserIsAdmin && !projectGroupManager.IsAuthorized(UserId, projectGroupId))
						{
							//Redirect to MyPage with no project in the URL
							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Put the project info into the current request context
						SpiraContext.Current.ProjectGroupId = projectGroup.ProjectGroupId;
						SpiraContext.Current.ProjectGroupName = projectGroup.Name;
						SpiraContext.Current.IsGroupMember = true;
						SpiraContext.Current.WorkspaceType = (int)Workspace.WorkspaceTypeEnum.Program;

						//See if this matches the last opened id in the user's profile
						if (!Profile.LastOpenedProjectGroupId.HasValue || Profile.LastOpenedProjectGroupId.Value != projectGroupId)
						{
							Profile.LastOpenedProjectGroupId = SpiraContext.Current.ProjectGroupId;
							Profile.Save();
						}
					}
					catch (ArtifactNotExistsException)
					{
						//The project no longer exists

						//Redirect to MyPage with no project in the URL
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
					}
				}
			}

			#endregion

			#region Project Template

			//First see if we have the template id specified in the querystring
			if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_TEMPLATE_ID]))
			{
				int projectTemplateId;
				if (Int32.TryParse(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PROJECT_TEMPLATE_ID], out projectTemplateId))
				{
					//Make sure this template actually exists
					TemplateManager templateManager = new TemplateManager();
					DataModel.ProjectTemplate projectTemplate = templateManager.RetrieveById(projectTemplateId);
					if (projectTemplate == null)
					{
						//The project template no longer exists

						//Redirect to MyPage with no project in the URL
						Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
					}
					else
					{
						//Make sure project group is active or the user is an admin
						bool isTemplateAdmin = templateManager.IsAuthorizedToEditTemplate(UserId, projectTemplateId);
						if (!projectTemplate.IsActive && !this.UserIsAdmin && !isTemplateAdmin)
						{
							//Redirect to MyPage with no project in the URL
							Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
						}

						//Put the project template into the current request context
						SpiraContext.Current.ProjectTemplateId = projectTemplateId;
						SpiraContext.Current.ProjectTemplateName = projectTemplate.Name;
						SpiraContext.Current.IsTemplateAdmin = isTemplateAdmin;
						SpiraContext.Current.WorkspaceType = (int)Workspace.WorkspaceTypeEnum.ProjectTemplate;

						//See if this matches the last opened id in the user's profile
						if (!Profile.LastOpenedProjectTemplateId.HasValue || Profile.LastOpenedProjectTemplateId.Value != projectTemplateId)
						{
							Profile.LastOpenedProjectTemplateId = SpiraContext.Current.ProjectTemplateId;
							Profile.Save();
						}
					}
				}
			}

            #endregion

            #region Portfolio

            //First see if we have the template id specified in the querystring
            if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PORTFOLIO_ID]))
            {
                int portfolioId;
                if (Int32.TryParse(HttpContext.Current.Request.QueryString[GlobalFunctions.PARAMETER_PORTFOLIO_ID], out portfolioId))
                {
                    //Make sure this portfolio actually exists
                    PortfolioManager portfolioManager = new PortfolioManager();
                    DataModel.Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId);
                    if (portfolio == null)
                    {
                        //The portfolio no longer exists

                        //Redirect to MyPage with no project in the URL
                        Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, -1, 0), true);
                    }
                    else
                    {
                        //Put the portfolio into the current request context
                        SpiraContext.Current.PortfolioId = portfolioId;
                        SpiraContext.Current.PortfolioName = portfolio.Name;
                        SpiraContext.Current.WorkspaceType = (int)Workspace.WorkspaceTypeEnum.Portfolio;
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the current page needs the project to be specified
        /// </summary>
        /// <returns>Flag denoting that the project info is required</returns>
        /// <remarks>Current 'My Page', 'My Profile' and 'Administration' don't require a project to be specified</remarks>
        private bool RequiresProjectSpecified()
		{
			bool requiresProject = true;
			//See if the page we're bound to has the NavigationLink attribute set
			HeaderSettingsAttribute headerSettingsAttribute = (HeaderSettingsAttribute)System.Attribute.GetCustomAttribute(this.Page.GetType(), typeof(HeaderSettingsAttribute));

			if (headerSettingsAttribute != null)
			{
				if (headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.Administration ||
					headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.ErrorPage ||
					headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome ||
                    headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.PortfolioHome ||
                    headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.EnterpriseHome ||
                    headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.MyProfile ||
					headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.MyTimecard ||
					headerSettingsAttribute.HighlightedLink == GlobalNavigation.NavigationHighlightedLink.MyPage)
				{
					requiresProject = false;
				}
			}

			return requiresProject;
		}

		/// <summary>
		/// This method checks to see if there is an existing session for this user
		/// </summary>
		private void DetectMultipleUserSessions()
		{
			const string METHOD_NAME = "DetectMultipleUserSessions";

			//First we need to check to see if the user already has a session listed in the table
			//If so then we need to do a redirect to the page that allows him to force a new session
			//If they have 50+ concurrent user license then we don't do this anymore
			if (License.LicenseType != LicenseTypeEnum.Enterprise && License.Number < 50)
			{
				foreach (KeyValuePair<string, SessionDetails> item in Web.Global.UserSessionMapping)
				{
					//If we have a userId mapped against a DIFFERENT session id (ignore API sessions)
					if (item.Value.UserId == UserId && item.Key != Session.SessionID && String.IsNullOrEmpty(item.Value.PlugInName))
					{
						//Redirect to the session force page, appending the original URL as a parameter
						Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "Detected Multiple Active Sessions");
						Response.Redirect("~/ForceSession.aspx?" + GlobalFunctions.PARAMETER_ORIGINAL_URL + "=" + Server.UrlEncode(Request.RawUrl), true);
					}
				}
			}

			//Now we need to capture the session id and put into our user-session mapping table
			//Check to see if the session already has an entry, and either add or update accordingly
			Web.Global.RegisterSession(Session.SessionID, UserId);
		}

		/// <summary>
		/// Localizes the name of an artifact type
		/// </summary>
		/// <param name="name">The non-localized name</param>
		/// <returns>The localized name</returns>
		protected string LocalizeArtifactType(string name)
		{
			//The passed-in name may have spaces (e.g. "Test Case")
			//Try localizing the name without any spaces, see if we have a match (e.g. "TestCase")
			string localizedName = Resources.Fields.ResourceManager.GetString(name.Replace(" ", ""));
			if (!String.IsNullOrEmpty(localizedName))
			{
				name = localizedName;
			}
			return name;
		}

		/// <summary>
		/// Gets a localized list of artifact types
		/// </summary>
		/// <returns>The list of artifact types</returns>
		protected Dictionary<string, string> GetArtifactTypeList()
		{
			Dictionary<string, string> artifactTypeList = new Dictionary<string, string>();

			//Get the unlocalized list
			List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll();
			foreach (ArtifactType artifactType in artifactTypes)
			{
				string id = artifactType.ArtifactTypeId.ToString();
				string name = artifactType.Name;  //E.g. "Test Case"
				artifactTypeList.Add(id, LocalizeArtifactType(name));
			}

			return artifactTypeList;
		}

		/// <summary>
		/// Gets the list of values to display in the show/hide columns dropdown lists
		/// </summary>
		/// <returns></returns>
		protected Dictionary<string, string> CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			Dictionary<string, string> showHideList = new Dictionary<string, string>();

			//Need to get the non-localized list from the database
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(ProjectId, UserId, artifactType);
			foreach (ArtifactListFieldDisplay artifactField in artifactFields)
			{
				//See if we can localize the field name or not
				string localizedName = Resources.Fields.ResourceManager.GetString(artifactField.Name);
				if (!String.IsNullOrEmpty(localizedName))
				{
					artifactField.Caption = localizedName;
				}

                //For incidents, if we have the 'TaskProgress' column, need to call it just Progress
                if (artifactType == Artifact.ArtifactTypeEnum.Incident && artifactField.Name == "ProgressId")
                {
                    artifactField.Caption = Resources.Fields.Progress;
                }

				string legend = (artifactField.IsVisible) ? Resources.Dialogs.Global_Hide + " " + artifactField.Caption : Resources.Dialogs.Global_Show + " " + artifactField.Caption;
				showHideList.Add(artifactField.Name, legend);
			}

			return showHideList;
		}

		/// <summary>
		/// Creates the list of indent levels for the show indent level dropdown list
		/// </summary>
		/// <returns></returns>
		protected SortedDictionary<string, string> CreateShowLevelList()
		{
			SortedDictionary<string, string> indentLevelList = new SortedDictionary<string, string>();
			indentLevelList.Add("0", Resources.Dialogs.Global_AllLevels);
			indentLevelList.Add("1", Resources.Dialogs.Global_Level + " 1");
			indentLevelList.Add("2", Resources.Dialogs.Global_Level + " 2");
			indentLevelList.Add("3", Resources.Dialogs.Global_Level + " 3");
			indentLevelList.Add("4", Resources.Dialogs.Global_Level + " 4");
			indentLevelList.Add("5", Resources.Dialogs.Global_Level + " 5");
			indentLevelList.Add("6", Resources.Dialogs.Global_Level + " 6");
			indentLevelList.Add("7", Resources.Dialogs.Global_Level + " 7");
			indentLevelList.Add("8", Resources.Dialogs.Global_Level + " 8");
			indentLevelList.Add("9", Resources.Dialogs.Global_Level + " 9");

			return indentLevelList;
		}

		/// <summary>
		/// This method checks to see if we have more than the licensed number of users
		/// </summary>
		/// <param name="allowedNumber">The allowed number of concurrent users</param>
		private void DetectExceededConcurrentUsers(int allowedNumber)
		{
			//Iterate through the user session mapping table to count the number of distinct users
			//Also check to see if our user is already in the list of users
			List<int> foundUsers = new List<int>();
			foreach (KeyValuePair<string, SessionDetails> item in Web.Global.UserSessionMapping)
			{
				//See if we have this value in our list of users
				int userId = item.Value.UserId;
				if (!foundUsers.Contains(userId))
				{
					foundUsers.Add(userId);
				}
			}

			//If we have exceeded the number of users then redirect to the error page if the user is not present
			//Always allow the administrator to log-in!
			//Response.Write("DEBUG: " + foundUsers.Count + ">" + allowedNumber + ", currentUserInSession=" + currentUserInSession + ", UserId=" + UserId);
			if (foundUsers.Count > allowedNumber && UserId != Business.UserManager.UserSystemAdministrator)
			{
				Session.Abandon();
				Response.Redirect("~/InvalidLicense.aspx?" + GlobalFunctions.PARAMETER_LICENSE_ERROR_TYPE + "=exceededConcurrent", true);
			}
		}

		#endregion
	}
}
