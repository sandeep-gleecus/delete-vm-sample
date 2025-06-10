using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.Security;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web.ServerControls.Authorization
{
	/// <summary>
	/// This is the default implementation of IAuthorizedControl
	/// </summary>
	public class AuthorizedControlBase : IAuthorizedControl
	{
		protected string visibleForModule = "";
		protected string activeForModule = "";
		private StateBag viewState;

		//Viewstate keys
		protected const string ViewStateKey_AuthorizedPermission = "AuthorizedPermission";
		protected const string ViewStateKey_AuthorizedArtifactType = "AuthorizedArtifactType";
		protected const string ViewStateKey_Authorized_Permission_SubControl = "AuthorizedPermissionSubControl";

		/// <summary>
		/// The constructor for the class
		/// </summary>
		public AuthorizedControlBase(StateBag viewState)
		{
			//Set the reference to Viewstate
			this.viewState = viewState;
		}

		/// <summary>
		/// Contains the id of the current user (-1 if not authenticated, which shouldn't happen in reality)
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
		/// Stores whether the current user is a system admin or not
		/// </summary>
		public bool UserIsAdmin
		{
			get
			{
				ProfileEx profile = new ProfileEx();
				if (profile.Default.IsAnonymous)
				{
					return false;
				}
				return profile.IsAdmin;
			}
		}

		/// <summary>
		/// Stores whether the current user owns the current artifact or created it
		/// </summary>
		/// <remarks>
		/// The data is held in request context
		/// </remarks>
		public bool UserIsArtifactCreatorOrOwner
		{
			get
			{
				SpiraContext context = SpiraContext.Current;
				if (context != null)
				{
					return context.IsArtifactCreatorOrOwner;
				}
				return false;
			}
		}

		/// <summary>
		/// Stores whether the current user is a project group admin or not
		/// </summary>
		/// <remarks>
		/// The data is held in session not viewstate since it's project-independent
		/// </remarks>
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

		protected StateBag ViewState
		{
			get
			{
				return viewState;
			}
		}

		/// <summary>
		/// This is the type of artifact that the user's role needs to have permissions for
		/// </summary>
		public Artifact.ArtifactTypeEnum Authorized_ArtifactType
		{
			get
			{
				if (ViewState[ViewStateKey_AuthorizedArtifactType] == null)
				{
					return Artifact.ArtifactTypeEnum.None;
				}
				else
				{
					return (Artifact.ArtifactTypeEnum)ViewState[ViewStateKey_AuthorizedArtifactType];
				}

			}
			set
			{
				ViewState[ViewStateKey_AuthorizedArtifactType] = value;
			}
		}

		/// <summary>
		/// This is the type of action that the user's role needs to have permissions for
		/// </summary>
		public Project.PermissionEnum Authorized_Permission
		{
			get
			{
				if (ViewState[ViewStateKey_AuthorizedPermission] == null)
				{
					return Project.PermissionEnum.None;
				}
				else
				{
					return (Project.PermissionEnum)ViewState[ViewStateKey_AuthorizedPermission];
				}
			}
			set
			{
				ViewState[ViewStateKey_AuthorizedPermission] = value;
			}
		}

		/// <summary>
		/// This is the type of action that the user's role needs to have permissions for to access the sub control
		/// </summary>
		public Project.PermissionEnum Authorized_Permission_SubControl
		{
			get
			{
				if (ViewState[ViewStateKey_Authorized_Permission_SubControl] == null)
				{
					return Project.PermissionEnum.None;
				}
				else
				{
					return (Project.PermissionEnum)ViewState[ViewStateKey_Authorized_Permission_SubControl];
				}
			}
			set
			{
				ViewState[ViewStateKey_Authorized_Permission_SubControl] = value;
			}
		}

		/// <summary>
		/// Called by the control to determine if the user is authorized
		/// </summary>
		/// <projectRoleId>The current role of the user</projectRoleId>
		/// <param name="isSystemAdmin">Is the current user a system administrator</param>
		/// <param name="isGroupAdmin">Is the current user a group administrator</param>
		/// <returns>Whether the user is authorized or not and whether limited or fully authorized</returns>
		public Project.AuthorizationState IsAuthorized(int projectRoleId, bool isSystemAdmin, bool isGroupAdmin)
		{
			//First handle the special case that we require a system administrator
			//This takes affect regardless of the artifact type or role id
			if (Authorized_Permission == Project.PermissionEnum.SystemAdmin)
			{
				if (isSystemAdmin)
					return Project.AuthorizationState.Authorized;
				else
					return Project.AuthorizationState.Prohibited;
			}

			//Now handle the case of a Project Group Admin
			if (Authorized_Permission == Project.PermissionEnum.ProjectGroupAdmin)
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
			Artifact.ArtifactTypeEnum artifactType = Authorized_ArtifactType;
			if (artifactType == Artifact.ArtifactTypeEnum.Placeholder)
			{
				artifactType = Artifact.ArtifactTypeEnum.Incident;
			}

			//First see if the user has the appropriate standard permissions from his/her role
			ProjectManager projectManager = new ProjectManager();
			authorized = projectManager.IsAuthorized(projectRoleId, artifactType, Authorized_Permission);

			//Now if the user is a system administrator, this automatically authorizes him/her for all other permissions
			if (isSystemAdmin)
			{
				authorized = Project.AuthorizationState.Authorized;
			}
			return authorized;
		}

		/// <summary>
		/// Called by the control to determine if the user is authorized re the sub control
		/// </summary>
		/// <projectRoleId>The current role of the user</projectRoleId>
		/// <param name="isSystemAdmin">Is the current user a system administrator</param>
		/// <param name="isGroupAdmin">Is the current user a group administrator</param>
		/// <returns>Whether the user is authorized or not and whether limited or fully authorized</returns>
		public Project.AuthorizationState IsAuthorizedSubControl(int projectRoleId, bool isSystemAdmin, bool isGroupAdmin)
		{
			//First handle the special case that we require a system administrator
			//This takes affect regardless of the artifact type or role id
			if (Authorized_Permission_SubControl == Project.PermissionEnum.SystemAdmin)
			{
				if (isSystemAdmin)
					return Project.AuthorizationState.Authorized;
				else
					return Project.AuthorizationState.Prohibited;
			}

			//Now handle the case of a Project Group Admin
			if (Authorized_Permission_SubControl == Project.PermissionEnum.ProjectGroupAdmin)
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
			Project.AuthorizationState authorizedSubControl = Project.AuthorizationState.Prohibited;

			//If we have a placeholder artifact, the permission is taken from the Incident artifact type
			Artifact.ArtifactTypeEnum artifactType = Authorized_ArtifactType;
			if (artifactType == Artifact.ArtifactTypeEnum.Placeholder)
			{
				artifactType = Artifact.ArtifactTypeEnum.Incident;
			}

			//First see if the user has the appropriate standard permissions from his/her role
			ProjectManager projectManager = new ProjectManager();
			authorizedSubControl = projectManager.IsAuthorized(projectRoleId, artifactType, Authorized_Permission_SubControl);

			//Now if the user is a system administrator, this automatically authorizes him/her for all other permissions
			if (isSystemAdmin)
			{
				authorizedSubControl = Project.AuthorizationState.Authorized;
			}
			return authorizedSubControl;
		}
	}
}
