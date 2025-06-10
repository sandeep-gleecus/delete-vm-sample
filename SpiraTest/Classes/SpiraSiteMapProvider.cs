using System;
using System.Diagnostics;
using System.Web;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.UserControls;
using System.Web.Security;
using System.Text.RegularExpressions;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Classes
{
	/// <summary>
	/// Overrides the standard site map provider to handle the role based security used in SpiraTest
	/// </summary>
	public class SpiraSiteMapProvider : XmlSiteMapProvider
	{
		//HttpRequest cache constants
		private const string GROUP_AUTHORIZATION_USER_PROJECT = "GroupAuthorizationForUserProject:";

		public SpiraSiteMapProvider()
		{
			//Do Nothing
		}

		/// <summary>
		/// Determines if the user has access to the specified sitemap node
		/// </summary>
		/// <param name="context">The HTTP context</param>
		/// <param name="node">The node in question</param>
		/// <returns>True if the node should be visible</returns>
		public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
		{
			bool isEnabled = true;
			if (!base.SecurityTrimmingEnabled)
			{
				return true;
			}

			//Make sure we have a project in context before we call!
			int projectRoleId = -1;
			int projectId = -1;
			int userId = -1;
			MembershipUser membershipUser = Membership.GetUser();
			if (membershipUser != null)
			{
				userId = (int)membershipUser.ProviderUserKey;
			}
			if (SpiraContext.Current != null && SpiraContext.Current.ProjectId.HasValue && SpiraContext.Current.ProjectRoleId.HasValue)
			{
				projectRoleId = SpiraContext.Current.ProjectRoleId.Value;
				projectId = SpiraContext.Current.ProjectId.Value;
			}

			//Make sure that we have a node value and then get the permissions
			if (node["value"] != null)
			{
				int linkId = Int32.Parse(node["value"]);

				if (projectId == -1)
				{
					if (
						linkId == (int)GlobalNavigation.NavigationHighlightedLink.MyPage || 
						linkId == (int)GlobalNavigation.NavigationHighlightedLink.Administration ||
						linkId == (int)GlobalNavigation.NavigationHighlightedLink.MyProfile ||
						linkId == (int)GlobalNavigation.NavigationHighlightedLink.MyTimecard
						)
					{
						isEnabled = true;
					}
					else
					{
						isEnabled = false;
					}
				}
				else
				{
					isEnabled = GetPermissions(linkId, userId, projectRoleId, projectId);
				}
			}
			else
			{
				isEnabled = !(projectId == -1);
			}

			return isEnabled;
		}

		/// <summary>Gets the enabled permissions for menu items</summary>
		/// <param name="linkId">The id of the selected link</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="projectRoleId">The id of the role in the project</param>
		/// <param name="userId">The id of the current user</param>
		protected bool GetPermissions(int linkId, int userId, int projectRoleId, int projectId)
		{
			bool enabled = true;
			ProjectManager projectManager = new ProjectManager();

			ProfileEx profile = new ProfileEx();
			//Check if user is a system admin and a member of the project
			//If so then they should have the same permissions as the read only role of Product Owner - which is 1.
			if (profile.IsAdmin && projectRoleId > 0)
			{
				projectRoleId = ProjectManager.ProjectRoleProjectOwner;
			}

			//They need to be fully authorized, not just limited to see the project menu items
			switch ((GlobalNavigation.NavigationHighlightedLink)linkId)
			{
				case GlobalNavigation.NavigationHighlightedLink.Requirements:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.Releases:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.TestCases:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.TestRuns:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.TestSets:
				case GlobalNavigation.NavigationHighlightedLink.TestConfigurations:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.Incidents:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

                case GlobalNavigation.NavigationHighlightedLink.Tasks:
				case GlobalNavigation.NavigationHighlightedLink.PullRequests:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
                    break;

				case GlobalNavigation.NavigationHighlightedLink.Documents:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.Iterations:
				case GlobalNavigation.NavigationHighlightedLink.PlanningBoard:
					enabled = (projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized && projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
					break;

				case GlobalNavigation.NavigationHighlightedLink.MyTimecard:
					enabled = License.LicenseProductName != LicenseProductNameEnum.SpiraTest
						&& (projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited
						&& projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited
						&& projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited);
					break;

				case GlobalNavigation.NavigationHighlightedLink.Resources:
					//Anyone with project access can see resources, since we limit the data on the resources pages
					//as necessary, except if they are limited view, so check for View+Task to check for that
					enabled = (License.LicenseProductName != LicenseProductNameEnum.SpiraTest && projectManager.IsAuthorized(projectRoleId, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
					break;

				case GlobalNavigation.NavigationHighlightedLink.Administration:
					enabled = (SpiraContext.Current.IsGroupAdmin || this.UserIsAdmin || SpiraContext.Current.IsProjectAdmin);
					break;

				case GlobalNavigation.NavigationHighlightedLink.AutomationHosts:
					enabled = projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized;
					break;

				case GlobalNavigation.NavigationHighlightedLink.Reports:
					enabled = (projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
					break;

				case GlobalNavigation.NavigationHighlightedLink.SourceCode:
				case GlobalNavigation.NavigationHighlightedLink.SourceCodeRevisions:
					enabled = (License.LicenseProductName != LicenseProductNameEnum.SpiraTest && projectManager.IsAuthorizedToViewSourceCode(projectRoleId));
					break;

				case GlobalNavigation.NavigationHighlightedLink.Risks:
					enabled = (License.LicenseProductName != LicenseProductNameEnum.SpiraTest
						&& License.LicenseProductName != LicenseProductNameEnum.SpiraTeam
						&& projectManager.IsAuthorized(projectRoleId, DataModel.Artifact.ArtifactTypeEnum.Risk, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
					break;
			}

			//The project group home and other options are only available if we're a group member of the current project's group
			if (linkId == (int)GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome ||
				linkId == (int)GlobalNavigation.NavigationHighlightedLink.GroupPlanningBoard ||
				linkId == (int)GlobalNavigation.NavigationHighlightedLink.GroupReleases ||
					linkId == (int)GlobalNavigation.NavigationHighlightedLink.GroupIncidents)
			{
				if (userId == -1 || projectId == -1)
				{
					enabled = false;
				}
				else
				{
					//The group planning board, releases, and incidents are only available in SpiraPlan
					if (License.LicenseProductName != LicenseProductNameEnum.SpiraPlan && linkId != (int)GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome)
					{
						enabled = false;
					}
					else
					{
						try
						{
							//See if we're a member of the current project's group

							//See if we already have a chached copy
							if (HttpContext.Current != null && HttpContext.Current.Items[GROUP_AUTHORIZATION_USER_PROJECT + userId + "_" + projectId] != null)
							{
								enabled = (bool)HttpContext.Current.Items[GROUP_AUTHORIZATION_USER_PROJECT + userId + "_" + projectId];
							}
							else
							{
								Project project = projectManager.RetrieveById(projectId);
								Business.ProjectGroupManager projectGroupManager = new ProjectGroupManager();
								enabled = projectGroupManager.IsAuthorized(userId, project.ProjectGroupId);
								HttpContext.Current.Items[GROUP_AUTHORIZATION_USER_PROJECT + userId + "_" + projectId] = enabled;
							}
						}
						catch (ArtifactNotExistsException)
						{
							enabled = false;
						}
					}
				}
			}
			return enabled;
		}

		public override SiteMapNode FindSiteMapNode(string rawUrl)
		{
			SiteMapNode retNode = base.FindSiteMapNode(rawUrl);

			//Only search if the normal method couldn't find it.
			if (retNode == null)
			{
				string newRawUrl = UrlRewriterModule.GetSiteMapURL(rawUrl);
				SiteMapNode tryNode = base.FindSiteMapNode(newRawUrl);

				//If it's still null, try one more time, stripping art/tab off.
				if (tryNode == null)
				{
					newRawUrl = newRawUrl.Replace("{artId}.aspx", "List.aspx");
					newRawUrl = newRawUrl.Replace("{artId}/{tabId}.aspx", "List.aspx");
					tryNode = base.FindSiteMapNode(newRawUrl);

					//If it's *still* null, send it the node for the root.
					if (tryNode == null)
					{
						if (HttpContext.Current.Request.ApplicationPath == "/")
						{
							tryNode = base.FindSiteMapNode("/Default.aspx");
						}
						else
						{
							tryNode = base.FindSiteMapNode(HttpContext.Current.Request.ApplicationPath + "/Default.aspx");
						}
					}
				}

				//If that failed, just return the root node.
				retNode = tryNode;
			}

			return retNode;
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
	}
}
