using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text.RegularExpressions;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.MasterPages
{
	/// <summary>
	/// Base class for all SpiraTest master pages
	/// </summary>
	public class MasterPageBase : System.Web.UI.MasterPage
	{

		#region Properties

		public string PageTitle
		{
			get
			{
				if (ViewState["PageTitle"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["PageTitle"];
				}
			}
			set
			{
				ViewState["PageTitle"] = value;
			}
		}

		#endregion

		/// <summary>
		/// Gets the name of the current timezone
		/// </summary>
		/// <returns>The name of the current timezone</returns>
		protected string GetTimezone()
		{
			if (SpiraContext.Current == null)
			{
				return "";
			}
            return GlobalFunctions.JSEncode(SpiraContext.Current.TimezoneId);
		}

        /// <summary>
        /// Returns the project as a string that can be displayed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetProjectId()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.ProjectId.HasValue)
            {
                return SpiraContext.Current.ProjectId.Value.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Returns the project group id as a string that can be displayed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetProjectGroupId()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.ProjectGroupId.HasValue)
            {
                return SpiraContext.Current.ProjectGroupId.Value.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Returns the project template id as a string that can be displayed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetProjectTemplateId()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.ProjectTemplateId.HasValue)
            {
                return SpiraContext.Current.ProjectTemplateId.Value.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Returns the portfolio id as a string that can be displayed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetPortfolioId()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.PortfolioId.HasValue)
            {
                return SpiraContext.Current.PortfolioId.Value.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Returns the workplace type enum as a string that can be displayed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetWorkspaceType()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.WorkspaceType.HasValue)
            {
                return SpiraContext.Current.WorkspaceType.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Returns the base url as a string that can be display in JS
        /// </summary>
        /// <returns></returns>
        protected string GetBaseUrl()
        {
            return this.ResolveUrl("~");
        }

        /// <summary>
        /// Returns the registered product so that it can be accessed in JS
        /// </summary>
        /// <returns></returns>
        protected string GetProductType()
        {
            return Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType;
        }

        /// <summary>
        /// If theming is enabled, need to pass the theme folder so that images resolve correctly
        /// </summary>
        /// <returns></returns>
        protected string GetThemeUrl()
        {
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    return "/App_Themes/" + Page.Theme + "/";
                }
                else
                {
                    return HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the name of the current user (if any)
        /// </summary>
        /// <returns>The name of the current user</returns>
        protected string GetUserName()
        {
            MembershipUser membershipUser = Membership.GetUser();
            if (membershipUser == null)
            {
                return "";
            }
            return GlobalFunctions.JSEncode(membershipUser.UserName);
        }

        /// <summary>
        /// Gets the id of the current user (if any)
        /// </summary>
        /// <returns>The user id of the current user</returns>
        protected string GetUserId()
        {
            MembershipUser membershipUser = Membership.GetUser();
            if (membershipUser == null)
            {
                return "";
            }
            return membershipUser.ProviderUserKey.ToString();
        }

        /// <summary>
        /// Contains the id of the current user - COPIED FROM PAGELAYOUT
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
        /// Gets the current user's project role
        /// </summary>
        /// <returns>The users' project role</returns>
        protected string GetProjectRoleId()
        {
            if (SpiraContext.Current != null && SpiraContext.Current.ProjectRoleId.HasValue)
            {
                return SpiraContext.Current.ProjectRoleId.Value.ToString();
            }
            return "undefined";
        }

        /// <summary>
        /// Get if the current user is a project admin
        /// </summary>
        /// <returns>Whether the user is a project admin</returns>
        protected string GetIsProjectAdmin()
        {
            if (SpiraContext.Current != null)
            {
                return SpiraContext.Current.IsProjectAdmin.ToString().ToLowerInvariant();
            }
            return "false";
        }

        /// <summary>
        /// Get if the current user is a project template admin
        /// </summary>
        /// <returns>Whether the user is a project admin</returns>
        protected string GetIsTemplateAdmin()
        {
            if (SpiraContext.Current != null)
            {
                return SpiraContext.Current.IsTemplateAdmin.ToString().ToLowerInvariant();
            }
            return "false";
        }

        /// <summary>
        /// Can the current user add comments to artifacts
        /// </summary>
        public bool CanUserAddCommentToArtifacts
        {
            get
            {
                if (SpiraContext.Current != null)
                {
                    return SpiraContext.Current.CanUserAddCommentToArtifacts;
                }
                return false;
            }
        }

        /// <summary>
        /// Is the user system admin
        /// </summary>
        public bool IsSystemAdmin
        {
            get
            {
                return new ProfileEx().IsAdmin;
            }
        }

        /// <summary>
        /// Is the user a portfolio admin
        /// </summary>
        public bool IsPortfolioAdmin
        {
            get
            {
                return new ProfileEx().IsPortfolioAdmin;
            }
        }

		/// <summary>
		/// Is the user a custom report admin
		/// </summary>
		public bool IsReportAdmin
		{
			get
			{
				return new ProfileEx().IsReportAdmin;
			}
		}

		/// <summary>
		/// Get if the current user is a group admin
		/// </summary>
		/// <returns>Whether the user is a group admin</returns>
		protected string GetIsGroupAdmin()
        {
            if (SpiraContext.Current != null)
            {
                return SpiraContext.Current.IsGroupAdmin.ToString().ToLowerInvariant();
            }
            return "false";
        }

        /// <summary>
        /// Adds the mobile web browser 'app-store' icons to the various master pages
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            string productPrefix = "";
            if (!String.IsNullOrEmpty(Common.ConfigurationSettings.Default.License_ProductType))
            {
                productPrefix = Common.ConfigurationSettings.Default.License_ProductType;
            }

            //<link id="Link1" rel="apple-touch-icon" href="~/App_Themes/InflectraTheme/Images/app-icon-57x57.png" runat="server" />
            HtmlLink htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-57x57.png";
            htmlLink.Attributes.Add("rel", "apple-touch-icon");
            this.Page.Header.Controls.Add(htmlLink);
            
            //<link id="Link2" rel="apple-touch-icon" sizes="57x57" href="~/App_Themes/InflectraTheme/Images/app-icon-57x57.png" runat="server" />
            htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-57x57.png";
            htmlLink.Attributes.Add("rel", "apple-touch-icon");
            htmlLink.Attributes.Add("sizes", "57x57");
            this.Page.Header.Controls.Add(htmlLink);

            //<link id="Link3" rel="apple-touch-icon" sizes="72x72" href="~/App_Themes/InflectraTheme/Images/app-icon-72x72.png" runat="server" />
            htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-57x57.png";
            htmlLink.Attributes.Add("rel", "apple-touch-icon");
            htmlLink.Attributes.Add("sizes", "57x57");
            this.Page.Header.Controls.Add(htmlLink);
            
            //<link id="Link4" rel="apple-touch-icon" sizes="114x114" href="~/App_Themes/InflectraTheme/Images/app-icon-114x114.png" runat="server" />
            htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-57x57.png";
            htmlLink.Attributes.Add("rel", "apple-touch-icon");
            htmlLink.Attributes.Add("sizes", "57x57");
            this.Page.Header.Controls.Add(htmlLink);

            //<link id="Link4" rel="icon" sizes="192x192" href="~/App_Themes/InflectraTheme/Images/app-icon-192x192".png" runat="server" />
            htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-192x192.png";
            htmlLink.Attributes.Add("rel", "icon");
            htmlLink.Attributes.Add("sizes", "192x192");
            this.Page.Header.Controls.Add(htmlLink);

            //<link id="Link4" rel="icon" sizes="128x128" href="~/App_Themes/InflectraTheme/Images/app-icon-128x128.png" runat="server" />
            htmlLink = new HtmlLink();
            htmlLink.Href = "~/App_Themes/ValidationMasterTheme/Images/app-icon-" + productPrefix + "-128x128.png";
            htmlLink.Attributes.Add("rel", "icon");
            htmlLink.Attributes.Add("sizes", "128x128");
            this.Page.Header.Controls.Add(htmlLink);
        }



        /// <summary>Gets a collection of user settings for the current user - COPIED FROM PAGELAYOUT</summary>
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
        /// Gets a simple (i.e. single entry) string user setting - COPIED FROM PAGELAYOUT
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
    }
}
