using System;
using System.Linq;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Business;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Web.MasterPages
{
    /// <summary>
    /// This is the master page for all the application pages in SpiraTest after you've logged-in
    /// </summary>
    public partial class Main : MasterPageBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.MasterPages.Main::";

        private string _antiXsrfTokenValue;

        #region Properties

        /// <summary>
        /// Returns the list of permissions for the current role
        /// </summary>
        public string Permissions
        {
            get
            {
                string output = "[]";   //Empty array
                if (SpiraContext.Current != null)
                {
                    int? projectRoleId = SpiraContext.Current.ProjectRoleId;
                    if (projectRoleId.HasValue)
                    {
                        //Get the list of roles and serialize the permissions
                        ProjectManager projectManager = new ProjectManager();
                        if (projectManager.AuthorizedProjectRoles != null)
                        {
                            DataModel.ProjectRole projectRole = projectManager.AuthorizedProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId.Value);
                            if (projectRole != null)
                            {
                                output = Newtonsoft.Json.JsonConvert.SerializeObject(projectRole.RolePermissions, Newtonsoft.Json.Formatting.None);
                            }
                        }
                    }
                }
                return output;
            }
        }

        /// <summary>
        /// Returns the project role definition
        /// </summary>
        public string ProjectRole
        {
            get
            {
                string output = "{}";   //Empty object
                if (SpiraContext.Current != null)
                {
                    int? projectRoleId = SpiraContext.Current.ProjectRoleId;
                    if (projectRoleId.HasValue)
                    {
                        //Get the list of roles and serialize the permissions
                        ProjectManager projectManager = new ProjectManager();
                        if (projectManager.AuthorizedProjectRoles != null)
                        {
                            DataModel.ProjectRole projectRole = projectManager.AuthorizedProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId.Value);
                            if (projectRole != null)
                            {
                                Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
                                output = Newtonsoft.Json.JsonConvert.SerializeObject(projectRole, Newtonsoft.Json.Formatting.None, settings);
                            }
                        }
                    }
                }
                return output;
            }
        }

        /// <summary>
        /// Returns a handle to the global navigation bar
        /// </summary>
        public GlobalNavigation TstGlobalNavigation
        {
            get
            {
                return this.tstGlobalNavigation;
            }
        }

        /// <summary>
        /// Returns the user setting for color mode - used for displaying dark mode
        /// </summary>
        public string ColorMode
        {
            get
            {
                if (UserId < 1)
                {
                    //Fail safely
                    return "auto";
                }
                string userColorMode = GetUserSetting(GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_CURRENT_COLOR_MODE);
                if (String.IsNullOrWhiteSpace(userColorMode))
                {
                    userColorMode = "auto";
                }
                return Microsoft.Security.Application.Encoder.HtmlAttributeEncode(userColorMode);
            }
        }
        #endregion

        /// <summary>
        /// Change the ID of the master page to something more meaningful
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "mpMain";
        }

        /// <summary>
        /// Handles the CSRF / XSRF prevention using a cookie and ViewState key
        /// </summary>
        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            HttpCookie requestCookie = Request.Cookies[GlobalFunctions.AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                HttpCookie responseCookie = new HttpCookie(GlobalFunctions.AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        /// <summary>
        /// Handles the CSRF / XSRF prevention using a cookie and ViewState key
        /// </summary>
        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            const string METHOD_NAME = "master_Page_PreLoad";

            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[GlobalFunctions.AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[GlobalFunctions.AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[GlobalFunctions.AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[GlobalFunctions.AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, Resources.Messages.MasterPage_ValidationOfXsrfTokenFailed);
                    throw new XsrfViolationException(Resources.Messages.MasterPage_ValidationOfXsrfTokenFailed);
                }
            }
        }

        /// <summary>
        /// Returns the CSRF anti-forgery token for use in WCF Ajax web service requests (as a header)
        /// </summary>
        /// <returns>The token JS Encoded</returns>
        protected string GetAntiXsrfTokenValue()
        {
            HttpCookie requestCookie = Request.Cookies[GlobalFunctions.AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                return Microsoft.Security.Application.Encoder.JavaScriptEncode(requestCookie.Value);
            }
            else
            {
                return "''";
            }
        }

        /// <summary>
        /// Called when the page first loads
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            // No client side cashing for non IE browsers as it can break ViewState (esp. Mozilla)
            if (Request.Browser.MSDomVersion.Major == 0) // Non IE Browser?)
            {
                Response.Cache.SetNoStore(); 
            }

            //Hide the instant messenger functionality if we're using SpiraTest or it has been disabled
            if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Message) || !ConfigurationSettings.Default.Message_Enabled)
            {
                this.ucMessageManager.Visible = false;
            }

            //See if the page we're bound to has the page title
            HeaderSettingsAttribute headerSettingsAttribute = (HeaderSettingsAttribute)System.Attribute.GetCustomAttribute(this.Page.GetType(), typeof(HeaderSettingsAttribute));

            if (headerSettingsAttribute == null)
            {
                //If we're not bound to a title, see if we've been passed a title directly
                if (String.IsNullOrEmpty(this.PageTitle))
                {
                    this.Page.Title = ConfigurationSettings.Default.License_ProductType;
                }
                else
                {
                    this.Page.Title = this.PageTitle + " | " + ConfigurationSettings.Default.License_ProductType;
                }
            }
            else
            {
                //See if we have a page-title or not
                if (String.IsNullOrEmpty(headerSettingsAttribute.PageTitle))
                {
                    //See if we've been passed a title directly
                    if (String.IsNullOrEmpty(this.PageTitle))
                    {
                        this.Page.Title = ConfigurationSettings.Default.License_ProductType;
                    }
                    else
                    {
                        this.Page.Title = this.PageTitle + " | " + ConfigurationSettings.Default.License_ProductType;
                    }
                }
                else
                {
                    //Get the page title attribute
                    string pageTitle = headerSettingsAttribute.PageTitle;

                    //See if it's actually a localized sitemap token
                    string localizedTitle = Resources.Main.ResourceManager.GetString(pageTitle);
                    if (String.IsNullOrEmpty(localizedTitle))
                    {
                        this.Page.Title = pageTitle + " | " + ConfigurationSettings.Default.License_ProductType;
                    }
                    else
                    {
                        this.Page.Title = localizedTitle + " | " + ConfigurationSettings.Default.License_ProductType;
                    }
                }
            }

            //Populate display constants
            this.lblVersionNumber.Text = GlobalFunctions.DISPLAY_SOFTWARE_VERSION;
            this.lblCopyrightYear.Text = GlobalFunctions.CopyrightYear;
            this.lblProductName.Text = ConfigurationSettings.Default.License_ProductType;
            this.lblBuildNumber.Text = GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD.ToString();
            this.lblCurrentCulture.Text = CultureInfo.CurrentUICulture.Name;
            try
            {
                TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(SpiraContext.Current.TimezoneId);
                string utcOffset = "(UTC" + timezone.GetUtcOffset(DateTime.Now).Hours.ToString("'+'0;'-'0") + ")";
                this.lblCurrentTimezone.Text = ((timezone.IsDaylightSavingTime(DateTime.Now.Date)) ? timezone.DaylightName : timezone.StandardName) + " " + utcOffset;
            }
            catch (TimeZoneNotFoundException)
            {
                //Do Nothing
            }

            #if DEBUG
            //Display if we're running in debug mode so that we can make we don't accidentally put into production
            this.ltrDebug.Visible = true;
            #endif

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Determine useragent of the device - returns a string to add as class on page
        /// </summary>
        protected string ResponsiveClasses()
		{
			string userAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            string classesToAdd = "";
			if (!String.IsNullOrEmpty(userAgent))
			{
				string userLeft = "";
				if (userAgent.Length > 4)
				{
					userLeft = userAgent.Substring(0, 4);
				}
				else
				{
					userLeft = userAgent;
				}
                if (GlobalFunctions.BrowserRegex.IsMatch(userAgent) || GlobalFunctions.VersionRegex.IsMatch(userLeft))
				{
                    classesToAdd = "mobile-device";
				}
                else
                {
                    classesToAdd = "desktop-device";
                }
			}
            return classesToAdd;
		}

        /// <summary>
        /// Determine useragent of the device - returns a boolean for use by JS
        /// </summary>
        protected string isMobileDevice()
        {
            string userAgent = Request.ServerVariables["HTTP_USER_AGENT"];
            bool isMobile = false;
            if (!String.IsNullOrEmpty(userAgent))
            {
                string userLeft = "";
                if (userAgent.Length > 4)
                {
                    userLeft = userAgent.Substring(0, 4);
                }
                else
                {
                    userLeft = userAgent;
                }
                if (GlobalFunctions.BrowserRegex.IsMatch(userAgent) || GlobalFunctions.VersionRegex.IsMatch(userLeft))
                {
                    isMobile = true;
                }
            }
            return isMobile.ToString().ToLowerInvariant();
        }
    }
}
