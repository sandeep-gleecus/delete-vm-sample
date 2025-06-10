using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web.UserControls
{
    /// <summary>
    /// Handles the instant messager polling and launching
    /// </summary>
    public partial class MessageManager : UserControlBase
    {
        /// <summary>
        /// Returns the base url for the avatar handler
        /// </summary>
        protected string AvatarBaseUrl
        {
            get
            {
                 return UrlRewriterModule.ResolveUrl("~/UserAvatar.ashx?" + GlobalFunctions.PARAMETER_USER_ID + "={0}&" + GlobalFunctions.PARAMETER_THEME_NAME + "=" + Page.Theme);
            }
        }

        /// <summary>
        /// Returns the folder containing the theme
        /// </summary>
        protected string ThemeFolder
        {
            get
            {
                //If theming is enabled, need to pass the theme folder so that images resolve correctly
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
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Register any scripts, use code-behind to avoid multiple loads
            //<Scripts>
            //  <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Messenger.js" />
            //  <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js" />
            //</Scripts>
            this.ajxScriptManager.Scripts.Add(new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Messenger.js")));
            this.ajxScriptManager.Scripts.Add(new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js")));
            this.ajxScriptManager.Scripts.Add(new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Notify.js")));
        }
    }
}