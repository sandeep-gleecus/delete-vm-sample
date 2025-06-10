using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.IO;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Overrides the WCF AJAX return path when using AJAX WCF web services to not use an absolute (port, server name, etc.) URL
    /// to make it more load-balancer, firewall friendly
    /// </summary>
    public class ScriptManagerEx : ScriptManager
    {
        /// <summary>
        /// Adds the necessary commands to set the path
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //See if we have a virtual directory (or are running on the website root)
            string baseUrl = "";
            if (HttpContext.Current.Request.ApplicationPath != "/")
            {
                baseUrl = HttpContext.Current.Request.ApplicationPath;
            }
            //Loop through each web service, overriding the path
            foreach (ServiceReference serviceRef in this.Services)
            {
                if (!String.IsNullOrWhiteSpace(serviceRef.Path))
                {
                    RegisterLocationOverrideScript(serviceRef.Path, baseUrl);
                }
            }

            //Loop through all of the scripts that are NOT embedded in the assembly and make sure they have a version
            //number appended to prevent caching issues when people upgrade the product
            foreach (ScriptReference scriptRef in this.Scripts)
            {
                if (!String.IsNullOrWhiteSpace(scriptRef.Path) && !scriptRef.Path.Contains('?'))
                {
                    scriptRef.Path += "?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER;
                }
            }
        }

        /// <summary>
        /// Registers the client-side code to override the AJAX URL Location
        /// </summary>
        /// <param name="path">The app-root relative path (~)</param>
        /// <param name="baseUrl">The WCF override base URL</param>
        public void RegisterLocationOverrideScript(string path, string baseUrl)
        {
            string serviceNamespace = "Inflectra.SpiraTest.Web.Services.Ajax." + Path.GetFileNameWithoutExtension(path);
            string scriptClientUrl = path.Replace("~", baseUrl);
            //e.g. Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.set_path('http://machine/SpiraTest/Services/Ajax/RequirementsService.svc');
            string script = serviceNamespace + ".set_path('" + scriptClientUrl + "');";
            RegisterStartupScript(this.Page, typeof(ScriptManagerEx), path, script, true);
        }
    }
}