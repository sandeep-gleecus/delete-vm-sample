using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the administration automation engine configuration page
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_AutomationEngines_Title", "System-Integration/#test-automation", "Admin_AutomationEngines_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class AutomationEngines : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.AutomationEngines::";

        protected string productName = "";

        /// <summary>
        /// Called when the control is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Set the licensed product name (used in several places)
            this.productName = ConfigurationSettings.Default.License_ProductType;

            //Register the event handlers
            this.grdAutomationEngines.RowDataBound += grdAutomationEngines_RowDataBound;
            this.grdAutomationEngines.RowCommand += new GridViewCommandEventHandler(grdAutomationEngines_RowCommand);
            this.btnAdd.Click += new EventHandler(btnAdd_Click);

            //Load the page if not postback
            if (!Page.IsPostBack)
            {
                LoadAndBindData();
            }
        }

        void grdAutomationEngines_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                AutomationEngine automationEngine = (AutomationEngine)e.Row.DataItem;
                if (automationEngine.IsActive)
                {
                    e.Row.Cells[3].CssClass = "priority2 bg-success";
                }
                else
                {
                    e.Row.Cells[3].CssClass = "priority2 bg-warning";
                }
            }
        }

        /// <summary>
        /// Called when you click on the button to add an automation engine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAdd_Click(object sender, EventArgs e)
        {
            //Redirect to the provider add/edit screen in add mode
            Response.Redirect("AutomationEngineDetails.aspx");
        }

        /// <summary>
        /// Called when you click on a link in the engine grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdAutomationEngines_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //See which command was executed
            if (e.CommandName == "DeleteEngine")
            {
                //Delete and reload the list
                AutomationManager automationManager = new AutomationManager();
                int automationEngineId = Int32.Parse((string)e.CommandArgument);
                automationManager.DeleteEngine(automationEngineId, this.UserId);
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Loads and displays the page's contents when called
        /// </summary>
        protected void LoadAndBindData()
        {
            //Load the list of automation engines and databind
            AutomationManager automationManager = new AutomationManager();
            this.grdAutomationEngines.DataSource = automationManager.RetrieveEngines(false);
            this.grdAutomationEngines.DataBind();
        }
    }
}
