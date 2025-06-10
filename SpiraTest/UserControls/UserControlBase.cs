using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Business;
using System.Data;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls
{
    /// <summary>
    /// The base class for all user controls	
    /// </summary>
    public class UserControlBase : System.Web.UI.UserControl
    {
        private MessageBox messageLabelHandle;

        /// <summary>
        /// A handle to the message label control on the page containing the panel
        /// </summary>
        public MessageBox MessageLabelHandle
        {
            get
            {
                return this.messageLabelHandle;
            }
            set
            {
                this.messageLabelHandle = value;
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
        /// Contains the id of the current user
        /// </summary>
        /// <remarks>
        /// The data is held in session not viewstate since it's project-independent
        /// </remarks>
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
        /// Stores whether the current user is a project admin or not
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

        /// <summary>
        /// Stores whether the user is able to view portfolios
        /// </summary>
        public bool UserIsPortfolioViewer
        {
            get
            {
                return new ProfileEx().IsPortfolioAdmin;
            }
        }

		/// <summary>
		/// Stores whether the user is able to administer custom reports
		/// </summary>
		public bool UserIsReportAdmin
		{
			get
			{
				return new ProfileEx().IsReportAdmin;
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
        /// Contains the id of the current project's group
        /// </summary>
        public int ProjectGroupId
        {
            get
            {
                SpiraContext context = SpiraContext.Current;
                if (context != null && context.ProjectGroupId.HasValue)
                {
                    return context.ProjectGroupId.Value;
                }
                return -1;
            }
        }

        /// <summary>
        /// Contains the id of the current portfolio
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
        /// Contains the name of the current project group
        /// </summary>
        public string ProjectGroupName
        {
            get
            {
                SpiraContext context = SpiraContext.Current;
                if (context != null && !String.IsNullOrEmpty(context.ProjectGroupName))
                {
                    return context.ProjectGroupName;
                }
                return "";
            }
        }

        /// <summary>
        /// Contains the rss token of the current user (or null if no token)
        /// </summary>
        /// <remarks>
        /// The data is held in session not viewstate since it's project-independent
        /// It also strips off the leading/trailing curly brackets to make it an easier URL to handle
        /// </remarks>
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
                    if (String.IsNullOrEmpty(user.RssToken))
                    {
                        return "";
                    }
                    return user.RssToken.Replace("{", "").Replace("}", "");
                }
                return "";
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
                string legend = (artifactField.IsVisible) ? Resources.Dialogs.Global_Hide + " " + artifactField.Caption : Resources.Dialogs.Global_Show + " " + artifactField.Caption;
                if (!showHideList.ContainsKey(artifactField.Name))
                {
                    showHideList.Add(artifactField.Name, legend);
                }
            }

            return showHideList;
        }

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
    }
}
