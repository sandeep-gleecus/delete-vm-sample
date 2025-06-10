using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System.Diagnostics;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the administration system information page
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_SystemInfo", "System-Administration", "Admin_SystemInfo"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class SystemInfo : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.SystemInfo::";
        private string UNKNOWN_STR = "<i><strong>" + Resources.Main.System_About_Unknown + "</strong></i>";
        private string LOCALHOST_STR = "<i>localhost</i>";

        /// <summary>
        /// Loads the system information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
        {
            string METHOD = CLASS_NAME + "Page_Load()";

            //If cloud-hosted, don't show
            if (!Common.Properties.Settings.Default.LicenseEditable)
            {
                Response.Redirect("~/Administration/Default.aspx");
            }

            //Load values into our labels:
            Version appVer = new Version(FileVersionInfo.GetVersionInfo(typeof(SystemInfo).Assembly.Location).FileVersion);

            //- Product Type
            lblLicenseProduct.Text = ConfigurationSettings.Default.License_ProductType + " " +
                "v" + GlobalFunctions.DISPLAY_SOFTWARE_VERSION + "." + GlobalFunctions.DISPLAY_SOFTWARE_VERSION_BUILD.ToString();

            //- Version.
            try
            {
                string build = "[Release]";
#if DEBUG
                build = "[Debug]";
#endif
                lblVersionBuild.Text = appVer.ToString() + " " + build;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(METHOD, ex, "Loading Application Version Info.");
            }

            //- Helper Assemblies.
            try
            {
                string assemblyVers = "Business: " + typeof(UserManager).Assembly.GetName().Version.ToString() + "<br />" +
                    "Common: " + typeof(Logger).Assembly.GetName().Version.ToString() + "<br />" +
                    "Data: " + typeof(Artifact).Assembly.GetName().Version.ToString() + "<br />";
                lblRefAssemblies.Text = assemblyVers;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(METHOD, ex, "Loading Application Reference Version Info.");
            }

            //- Detected Browser
            if (Request.Browser != null)
            {
                var browser = Request.Browser;
                lblBrowser.Text = browser.Browser + " [" + browser.Id + "]";
            }

            //Get our connection string and DB server props..
            Dictionary<string, string> props = ManagerBase.GetConnectionString();

            //- Database Server
            if (props.Keys.Any(k => k.Equals("data source")))
            {
                string server = props["data source"];
                if (server.Equals(".")) server = LOCALHOST_STR;
                lblDBServer.Text = server;
            }
            else if (props.Keys.Any(k => k.Equals("server")))
            {
                string server = props["server"].Trim();
                if (server.Equals(".")) server = LOCALHOST_STR;
                lblDBServer.Text = server;
            }
            else
                lblDBServer.Text = UNKNOWN_STR;

            //- Database Name
            if (props.Keys.Any(k => k.Equals("initial catalog")))
                lblDatabase.Text = props["initial catalog"];
            else if (props.Keys.Any(k => k.Equals("database")))
                lblDatabase.Text = props["database"];
            else
                lblDatabase.Text = UNKNOWN_STR;

            //- Database Login User
            if (props.Keys.Any(k => k.Equals("trusted connection") || k.Equals("trusted_connection") || k.Equals("integrated security")))
                lblDBUser.Text = "<i>IIS Application Pool User</i>";
            else if (props.Keys.Any(k => k.Equals("user id")))
                lblDBUser.Text = props["user id"];
            else
                lblDBUser.Text = UNKNOWN_STR;

            //- Database Version Sring
            if (props.Keys.Any(k => k.Equals("sysfullversion")))
            {
                string ver = props["sysfullversion"].Split('\n')[0].Trim();
                if (props.Keys.Any(k => k.Equals("edition")))
                    ver += " " + props["edition"];
                lblDBSvrVer.Text = ver;
            }
            else
                lblDBSvrVer.Text = UNKNOWN_STR;

            // - Collation in use.
            if (props.Keys.Any(k => k.Equals("collation")))
                lblServerColl.Text = props["collation"];
            else
                lblServerColl.Text = UNKNOWN_STR;

            //- Application Server IIS Version
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["SERVER_SOFTWARE"]))
                lblApp_IIS.Text = Request.ServerVariables["SERVER_SOFTWARE"];
            else
                lblApp_IIS.Text = UNKNOWN_STR;

            //- Application Server OS Version..
            try
            {
                string osVer = Environment.OSVersion.VersionString + " " +
                    ((Environment.Is64BitOperatingSystem) ? "x64" : "");
                lblApp_OS.Text = osVer.Trim();
            }
            catch { }

            //- Application ASP.NET version.
            try
            {
                string osVer = Environment.Version.ToString();
                lblApp_Asp.Text = osVer.Trim();
            }
            catch { }

            //- Application Path
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["APPL_PHYSICAL_PATH"]))
                lblApp_Path.Text = Request.ServerVariables["APPL_PHYSICAL_PATH"];
            else
                lblApp_Path.Text = UNKNOWN_STR;

            //- Application Server/Port
            string svrPrt = "";
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["SERVER_NAME"]))
                svrPrt = Request.ServerVariables["SERVER_NAME"];
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["SERVER_PORT"]))
                svrPrt += ":" + Request.ServerVariables["SERVER_PORT"];
            if (!string.IsNullOrWhiteSpace(svrPrt))
                lblApp_SvrPort.Text = svrPrt.Trim();
            else
                lblApp_SvrPort.Text = UNKNOWN_STR;

            //- Application protocol/secure.
            string svrSec = "";
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["SERVER_PROTOCOL"]))
                svrSec = Request.ServerVariables["SERVER_PROTOCOL"];
            if (!string.IsNullOrWhiteSpace(Request.ServerVariables["SERVER_PORT_SECURE"]))
                svrSec += ((Request.ServerVariables["SERVER_PORT_SECURE"].Equals("0")) ? "" : ":CERT");
            if (!string.IsNullOrWhiteSpace(svrSec))
                lblApp_Prot.Text = svrSec;
            else
                lblApp_Prot.Text = UNKNOWN_STR;

            //Databind the page
            this.DataBind();
        }
    }
}