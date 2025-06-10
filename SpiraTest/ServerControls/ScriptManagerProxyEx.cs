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
    public class ScriptManagerProxyEx : ScriptManagerProxy
    {
        /// <summary>
        /// Adds the necessary commands to set the path
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Get a handle to the real script manager
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager is ScriptManagerEx)
            {
                //See if we have a virtual directory (or are running on the website root)
                string baseUrl = "";
                if (HttpContext.Current.Request.ApplicationPath != "/")
                {
                    baseUrl = HttpContext.Current.Request.ApplicationPath;
                }

                //Make sure we are using the extended script manager
                ScriptManagerEx scriptManagerEx = (ScriptManagerEx)scriptManager;
                //Loop through each web service, overriding the path
                foreach (ServiceReference serviceRef in this.Services)
                {
                    if (!String.IsNullOrWhiteSpace(serviceRef.Path))
                    {
                        scriptManagerEx.RegisterLocationOverrideScript(serviceRef.Path, baseUrl);
                    }
                }

                //Loop through all of the scripts that are NOT embedded in the assembly and make sure they have a version
                //number appended to prevent caching issues when people upgrade the product
                foreach (ScriptReference scriptRef in this.Scripts)
                {
                    if (String.IsNullOrEmpty(scriptRef.Assembly) && !String.IsNullOrWhiteSpace(scriptRef.Path) &&
                        !scriptRef.Path.Contains(".axd") && !scriptRef.Path.Contains('?'))
                    {
                        scriptRef.Path += "?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER;
                    }
                }
            }
        }
    }
}