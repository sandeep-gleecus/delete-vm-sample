using System;
using System.Data;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in UpdatePanel to include built-in support for an AJAX spinner
    /// </summary>
    [ToolboxData("<{0}:UpdatePanelEx runat=server></{0}:UpdatePanelEx>")]
    public class UpdatePanelEx : UpdatePanel
    {
        [
        Bindable(true),
        Category("Appearance"),
        Description("Should we display an AJAX spinner when the update panel reloads"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool DisplaySpinner
        {
            get
            {
                if (ViewState["displaySpinner"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["displaySpinner"];
                }
            }
            set
            {
                ViewState["dashboardName"] = value;
            }
        }

        /// <summary>
        /// If inside an Actionless Form, need to tell the form that it has an UpdatePanel control to be considered
        /// when rendering the Action attribute of the form
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.Page.Form.GetType() == typeof(ActionlessForm))
            {
                ActionlessForm form = (ActionlessForm)this.Page.Form;
                form.InUpdatePanel = true;
            }
        }

        /// <summary>
        /// Display the client-code to display the spinner
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //If the AJAX framework has not been explictly enabled site-wide
            //need to render out the necessary client-code here
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager != null && scriptManager.AjaxFrameworkMode == AjaxFrameworkMode.Disabled)
            {
                int asyncPostBackTimeout = 90;
                string formElement = this.Page.Form.ID;
                string scriptManagerID = scriptManager.UniqueID;// "mpMain$ajxScriptManager";
                string updatePanelIDs_01 = "tmpMain$" + this.UniqueID;
                string updatePanelIDs_02 = this.ClientID;
                string masterPageUniqueID = this.Page.Master.UniqueID;
                string clientScript = @"
Sys.WebForms.PageRequestManager._initialize('" + scriptManagerID + @"', '" + formElement + @"', ['" + updatePanelIDs_01 + @"','" + updatePanelIDs_02 + @"'], [], [], " + asyncPostBackTimeout + ", '" + masterPageUniqueID + @"');
";
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "UpdatePanelEx_Initialize", clientScript, true);
            }
            
            if (this.DisplaySpinner)
            {
                string clientScript = @"
var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();

pageRequestManager.add_initializeRequest(updatePanelEx_initializeRequest);
pageRequestManager.add_endRequest(updatePanelEx_endRequest);

function updatePanelEx_initializeRequest(sender, e)
{
    globalFunctions.display_spinner();
}

function updatePanelEx_endRequest(sender, e)
{
    globalFunctions.hide_spinner();
}
";
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "UpdatePanelEx_DisplaySpinner", clientScript, true);
            }
        }
    }
}
