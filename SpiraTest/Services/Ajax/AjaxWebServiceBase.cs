using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.SessionState;
using System.ServiceModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using System.Collections.Generic;
using System.Collections;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
	/// <summary>
	/// The base class for all AJAX enabled web services
	/// </summary>
	[ServiceBehavior(MaxItemsInObjectGraph = 2147483647)]
	public class AjaxWebServiceBase
	{
		/// <summary>
		/// The login name of the currently logged-in user
		/// </summary>
		public string CurrentUserLogin
		{
			get
			{
				if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
				{
					return HttpContext.Current.User.Identity.Name;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the currently logged-in user
		/// </summary>
		public MembershipUser CurrentUser
		{
			get
			{
				return Membership.GetUser(false);
			}
		}


		/// <summary>
		/// Returns the ID of the currently logged-in user
		/// </summary>
		public int? CurrentUserId
		{
			get
			{
                //We need to make sure that we don't update the last activity date for web service requests because
                //they will cause locks on the TST_USER table. It's OK on the main web forms because they are less frequent.
				MembershipUser user = Membership.GetUser(false);
				if (user != null)
				{
					return (int)user.ProviderUserKey;
				}
				return null;
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
        /// Stores whether the current user is a system admin or not
        /// </summary>
        /// <remarks>
        /// Also returns false if the system is not running SpiraPlan
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
		/// Stores whether the current user is a report admin or not
		/// </summary>
		public bool UserIsReportAdmin
		{
			get
			{
				return new ProfileEx().IsReportAdmin;
			}
		}

		/// <summary>
		/// Called by service operations to see if the user is a member of the current project and has permissions to view source code
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>Whether the user is authorized or not (Limited state never returned)</returns>
		public Project.AuthorizationState IsAuthorizedToViewSourceCode(int projectId)
		{
			//Put the project id in context
			PutProjectInContext(projectId);
			SpiraContext context = SpiraContext.Current;

			//Make sure the user has a role on this project
			if (!context.ProjectRoleId.HasValue)
			{
				return Project.AuthorizationState.Prohibited;
			}

			//Next see if the user has the appropriate permissions from his/her role
            Business.ProjectManager projectManager = new Business.ProjectManager();
            bool authorized = projectManager.IsAuthorizedToViewSourceCode(context.ProjectRoleId.Value);
			return (authorized) ? Project.AuthorizationState.Authorized : Project.AuthorizationState.Prohibited;
		}


        /// <summary>
        /// Called by service operations to see if the user is a member of the current project and has permissions to add comments
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>Whether the user is authorized or not (Limited state never returned)</returns>
        public Project.AuthorizationState IsAuthorizedToAddComments(int projectId)
        {
            //Put the project id in context
            PutProjectInContext(projectId);
            SpiraContext context = SpiraContext.Current;

            //Make sure the user has a role on this project
            if (!context.ProjectRoleId.HasValue)
            {
                return Project.AuthorizationState.Prohibited;
            }

            //Next see if the user has the appropriate permissions from his/her role
            Business.ProjectManager projectManager = new Business.ProjectManager();
            bool authorized = projectManager.IsAuthorizedToAddComment(context.ProjectRoleId.Value);
            return (authorized) ? Project.AuthorizationState.Authorized : Project.AuthorizationState.Prohibited;
        }

        /// <summary>
        /// Called by the service operations to find out if a different (not the current) user has permissions for the specified action on the type of artifact
        /// and whether the authorization is limited to just items the user created/owns or all items of that type
        /// </summary>
        /// <param name="userId">The id of the user we're interested in</param>
        /// <param name="projectId">The id of the current project (if any)</param>
        /// <param name="permission">The permission required for the operation</param>
        /// <param name="artifactType">The type of artifact we're verifying permissions for</param>
        /// <returns>Whether the user is authorized or not and whether they are limited by artifact id</returns>
        public Project.AuthorizationState IsUserAuthorized(int userId, int? projectId, Project.PermissionEnum permission = Project.PermissionEnum.None, DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None)
        {
            Project.AuthorizationState ret = Project.AuthorizationState.Prohibited;

            //First retrieve the user info
            bool userIsAdmin = false;
            try
            {
                User user = new UserManager().GetUserById(userId, false);
                if (user == null || user.Profile == null)
                {
                    //The user does not exist any more
                    return Project.AuthorizationState.Prohibited;
                }
                userIsAdmin = user.Profile.IsAdmin;
            }
            catch (ArtifactNotExistsException)
            {
                //The user does not exist any more
                return Project.AuthorizationState.Prohibited;
            }

            //If the user is a system administrator, they can do anything..
            if (userIsAdmin)
            {
                ret = Project.AuthorizationState.Authorized;
            }
            else
            {
                if (projectId.HasValue)
                {
                    //See if we have the case of a group admin
                    Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
                    bool isGroupAdmin = projectGroupManager.IsAdmin(userId);
                    int? projectRoleId = null;
                    bool isProjectAdmin = false;

                    Business.ProjectManager projectManager = new Business.ProjectManager();
                    //Make sure this project actually exists
                    try
                    {
                        ProjectView project = projectManager.RetrieveById2(projectId.Value);
                        //Make sure project is active
                        if (project.IsActive)
                        {
                            //Make sure the user is authorized
                            if (new UserManager().Authorize(userId, projectId.Value))
                            {
                                //Retrieve the project role that the user has
                                ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId.Value, userId);
                                if (projectUser != null)
                                {
                                    projectRoleId = projectUser.ProjectRoleId;
                                    isProjectAdmin = projectUser.IsAdmin;
                                }
                            }
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //The project no longer exists, just fail quietly
                    }

                    //Now handle the case of a Project Group Admin
                    if (permission == Project.PermissionEnum.ProjectGroupAdmin)
                    {
                        //See if the user is either a system admin or a project group admin
                        ret = ((userIsAdmin || isGroupAdmin) ? Project.AuthorizationState.Authorized : Project.AuthorizationState.Prohibited);
                    }
                    else
                    {
                        //Make sure the user has a role on this project
                        if (projectRoleId.HasValue)
                        {
                            //First see if the user has the appropriate standard permissions from his/her role
                            ret = projectManager.IsAuthorized(projectRoleId.Value, artifactType, permission);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
		/// Called by the service operations to find out if the user has permissions for the specified action on the type of artifact
		/// and whether the authorization is limited to just items the user created/owns or all items of that type
		/// </summary>
		/// <param name="projectId">The id of the current project (if any)</param>
		/// <param name="permission">The permission required for the operation</param>
		/// <param name="artifactType">The type of artifact we're verifying permissions for</param>
		/// <returns>Whether the user is authorized or not and whether they are limited by artifact id</returns>
		public Project.AuthorizationState IsAuthorized(int? projectId, Project.PermissionEnum permission = Project.PermissionEnum.None, DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None)
		{
			Project.AuthorizationState ret = Project.AuthorizationState.Prohibited;

			//If the user is a system administrator, they can do anything..
            if (this.UserIsAdmin)
            {
                ret = Project.AuthorizationState.Authorized;
                if (projectId.HasValue)
                {
                    //We need force the user to be a 'member' of the project
                    Business.ProjectManager projectManager = new Business.ProjectManager();
                    //Make sure this project actually exists
                    try
                    {
                        ProjectView project = projectManager.RetrieveById2(projectId.Value);
                        //Make sure project is active
                        if (project.IsActive)
                        {
                            //Project information
                            SpiraContext.Current.ProjectId = projectId;
                            SpiraContext.Current.ProjectName = project.Name;
                            SpiraContext.Current.ProjectTemplateId = project.ProjectTemplateId;
                            SpiraContext.Current.ProjectGroupName = project.ProjectGroupName;
                            //Force them to be 'Project Owner'
                            SpiraContext.Current.ProjectRoleId = ProjectManager.ProjectRoleProjectOwner;
                            SpiraContext.Current.ProjectRoleName = "Project Owner";
                            SpiraContext.Current.IsProjectAdmin = true;
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //The project no longer exists, just fail quietly
                    }
                }
            }
            else
            {
                if (projectId.HasValue)
                {
                    PutProjectInContext(projectId.Value);
                    SpiraContext context = SpiraContext.Current;

                    //Now handle the case of a Project Group Admin
                    if (permission == Project.PermissionEnum.ProjectGroupAdmin)
                    {
                        //See if the user is either a system admin or a project group admin
                        ret = ((this.UserIsAdmin || context.IsGroupAdmin) ? Project.AuthorizationState.Authorized : Project.AuthorizationState.Prohibited);
                    }
                    else
                    {
                        //Make sure the user has a role on this project
                        if (context.ProjectRoleId.HasValue)
                        {
                            //First see if the user has the appropriate standard permissions from his/her role
                            Business.ProjectManager projectManager = new Business.ProjectManager();
                            ret = projectManager.IsAuthorized(context.ProjectRoleId.Value, artifactType, permission);
                        }
                    }
                }
            }

			return ret;
		}

		/// <summary>
		/// Makes sure that we have the project info in SpiraContext
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		protected void PutProjectInContext(int projectId)
		{
			//See if we have the case of a group admin
			Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
            SpiraContext.Current.IsGroupAdmin = projectGroupManager.IsAdmin(CurrentUserId.Value);

			Business.ProjectManager projectManager = new Business.ProjectManager();
			//Make sure this project actually exists
			try
			{
                ProjectView project = projectManager.RetrieveById2(projectId);
				//Make sure project is active
                if (!project.IsActive)
				{
					return;
				}

				//Make sure the user is authorized
				if (!new UserManager().Authorize(CurrentUserId.Value, projectId))
				{
					return;
				}

				//Put the project info into the current request context
				SpiraContext.Current.ProjectId = projectId;
                SpiraContext.Current.ProjectName = project.Name;
                SpiraContext.Current.ProjectTemplateId = project.ProjectTemplateId;
                SpiraContext.Current.ProjectGroupName = project.ProjectGroupName;

				//Retrieve the project role that the user has
                ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, CurrentUserId.Value);
                if (projectUser != null)
				{
                    SpiraContext.Current.ProjectRoleId = projectUser.ProjectRoleId;
                    SpiraContext.Current.ProjectRoleName = projectUser.ProjectRoleName;
                    SpiraContext.Current.IsProjectAdmin = projectUser.IsAdmin;
				}
			}
			catch (ArtifactNotExistsException)
			{
				//The project no longer exists, just fail quietly
			}
		}

		/// <summary>
		/// Returns a handle to the ASP.NET context properties, which are used to store data within the request lifespan
		/// </summary>
        /// <remarks>
        /// This used to store the data on the ASP.NET session object, but was changed to request context to avoid locking issues
        /// We left the name as session since it works like session but doesn't span multiple HTTP requests
        /// </remarks>
		public IDictionary Context
		{
			get
			{
				HttpContext httpContext = HttpContext.Current;
				return httpContext.Items;
			}
		}

		/// <summary>Gets a collection of user settings for the specified user</summary>
		/// <param name="collectionName">The name of the collection to retrieve.</param>
		/// <param name="userId">The id of the user</param>
		/// <returns>A UserSettingsCollection</returns>
		/// The collection is returned from session (if available) otherwise restored from the database</remarks>
		protected UserSettingsCollection GetUserSettings(int userId, string collectionName)
		{
			UserSettingsCollection userSettingsCollection;

			//Now see if we have this setting already in session
			if (Context[collectionName] == null)
			{
				//Restore settings from database and put in session
				userSettingsCollection = new UserSettingsCollection(userId, collectionName);
				userSettingsCollection.Restore();
				Context[collectionName] = userSettingsCollection;
				return userSettingsCollection;
			}
			//Restore from session
			userSettingsCollection = (UserSettingsCollection)Context[collectionName];

			//return the copy from session
			return userSettingsCollection;
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) string user setting
		/// </summary>
		/// <param name="userId">The id of the current user</param>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or "" if not set</returns>
		protected string GetUserSetting(int userId, string collectionName, string entryKey)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(userId, collectionName);
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
		/// <param name="userId">The id of the current user</param>
		protected int GetUserSetting(int userId, string collectionName, string entryKey, int defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(userId, collectionName);
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
		/// <param name="userId">The id of the current user</param>
		protected string GetUserSetting(int userId, string collectionName, string entryKey, string defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(userId, collectionName);
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
		/// <param name="userId">The id of the current user</param>
		protected bool GetUserSetting(int userId, string collectionName, string entryKey, bool defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(userId, collectionName);
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
		protected ProjectSettingsCollection GetProjectSettings(int userId, int projectId, string collectionName)
		{
			ProjectSettingsCollection projectSettingsCollection;

			//Now see if we have this setting already in session
			if (Context[collectionName] == null)
			{
				//Restore settings from database and put in session
				projectSettingsCollection = new ProjectSettingsCollection(projectId, userId, collectionName);
				projectSettingsCollection.Restore();
				return projectSettingsCollection;
			}
			//Restore from session
			projectSettingsCollection = (ProjectSettingsCollection)Context[collectionName];

			//Make sure that the project id's match, if not, need to do a fresh restore from the database
			if (projectSettingsCollection.ProjectId == projectId)
			{
				//return the copy from session
				return projectSettingsCollection;
			}
			else
			{
				//Restore settings from database and put in session
				projectSettingsCollection = new ProjectSettingsCollection(projectId, userId, collectionName);
				projectSettingsCollection.Restore();
				Context[collectionName] = projectSettingsCollection;
				return projectSettingsCollection;
			}
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(int userId, int projectId, string collectionName, string entryKey, int entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(userId, projectId, collectionName);
			projectSettingsCollection[entryKey] = entryValue;
			projectSettingsCollection.Save();
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) boolean project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(int userId, int projectId, string collectionName, string entryKey, bool entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(userId, projectId, collectionName);
			projectSettingsCollection[entryKey] = entryValue;
			projectSettingsCollection.Save();
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected int GetProjectSetting(int userId, int projectId, string collectionName, string entryKey, int defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(userId, projectId, collectionName);
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
		protected bool GetProjectSetting(int userId, int projectId, string collectionName, string entryKey, bool defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(userId, projectId, collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((bool)projectSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) string project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected string GetProjectSetting(int userId, int projectId, string collectionName, string entryKey, string defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(userId, projectId, collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((string)projectSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Creates a validation list from a simple message
		/// </summary>
		/// <param name="message">The message text</param>
		/// <returns>The validation message list</returns>
		public static List<ValidationMessage> CreateSimpleValidationMessage(string message)
		{
			List<ValidationMessage> messages = new List<ValidationMessage>();
			messages.Add(new ValidationMessage() { Message = message });
			return messages;
		}
	}
}
