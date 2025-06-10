using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// Summary description for _Default.
	/// </summary>
	public partial class _Default : PageBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//See which page the user has specified to be redirected to from user settings
			MembershipUser user = Membership.GetUser();
			if (user == null)
			{
				//Just redirect to My Page
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, 0, 0), true);
			}
			else
			{
				int userId = (int)user.ProviderUserKey;
				UserSettingsCollection userSettingsCollection = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
				userSettingsCollection.Restore();
				if (userSettingsCollection[GlobalFunctions.USER_SETTINGS_KEY_START_PAGE] != null && userSettingsCollection[GlobalFunctions.USER_SETTINGS_KEY_START_PAGE] is int)
				{
					GlobalNavigation.NavigationHighlightedLink startPage = (GlobalNavigation.NavigationHighlightedLink)((int)userSettingsCollection[GlobalFunctions.USER_SETTINGS_KEY_START_PAGE]);
					int currentProjectId = Profile.LastOpenedProjectId.HasValue ? (int)Profile.LastOpenedProjectId.Value : 0;

					switch (startPage)
					{
						case GlobalNavigation.NavigationHighlightedLink.MyPage:
							{
								//Redirect to My Page - use the last opened project if there is one
								Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, currentProjectId, 0), true);
							}
							break;

						case GlobalNavigation.NavigationHighlightedLink.ProjectHome:
							{
								//Redirect to the last opened project (if there is one)
								if (Profile.LastOpenedProjectId.HasValue)
								{
									//See which home page they last used (General/Dev/Test)
									ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(currentProjectId, userId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS);
									projectSettingsCollection.Restore();
									if (projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] != null && projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE] is string)
									{
										//Add on the URL suffix (dev/test)
										string tabName = (string)projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE];
										Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, currentProjectId, 0, tabName), true);
									}
									else
									{
										Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, currentProjectId, 0, "General"), true);
									}
								}
								else
								{
									//Just redirect to My Page
									Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, 0, 0), true);
								}
							}
							break;

						case GlobalNavigation.NavigationHighlightedLink.ProjectGroupHome:
							{
								//Redirect to the last opened project group (if there is one)
								if (Profile.LastOpenedProjectGroupId.HasValue)
								{
									Response.Redirect(UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, (int)Profile.LastOpenedProjectGroupId), true);
								}
								else
								{
									//Just redirect to My Page
									Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, 0, 0), true);
								}
							}
							break;

						default:
							{
								//Just redirect to My Page
								Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, currentProjectId, 0), true);
							}
							break;
					}
				}
				else
				{
					//Just redirect to My Page - use the last opened project if there is one
					int currentProjectId = Profile.LastOpenedProjectId.HasValue ? (int)Profile.LastOpenedProjectId : 0;
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, currentProjectId, 0), true);
				}
			}
		}
	}
}
