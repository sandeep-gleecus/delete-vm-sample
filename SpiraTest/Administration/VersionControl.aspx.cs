using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the administration version control integration home page
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_VersionControl_Title", "System-Integration/#version-control-integration-on-premise-customers-only", "Admin_VersionControl_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class VersionControl : AdministrationBase
    {
        private const string CLASS_NAME = "Web.Administration.VersionControl::";

        protected string productName = "";
        protected List<ProjectView> projects = null;
        protected bool anyActiveProviders = false;

        /// <summary>
        /// Called when the control is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Set the licensed product name (used in several places)
            this.productName = ConfigurationSettings.Default.License_ProductType;

            //See if this project has any active providers (if not we want to make all dropdowns selected)
            this.anyActiveProviders = new SourceCodeManager().IsActiveProvidersForProject(ProjectId);

            //Register the event handlers
            this.grdVersionControlProviders.RowCommand += new GridViewCommandEventHandler(grdVersionControlProviders_RowCommand);
            this.grdVersionControlProviders.RowDataBound += GrdVersionControlProviders_RowDataBound;
            this.grdVersionControlProviders.RowCreated += GrdVersionControlProviders_RowCreated;
            this.btnAdd.Click += new EventHandler(btnAdd_Click);

            //Load the page if not postback
            if (!Page.IsPostBack)
            {
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Creates child elements - in this case the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdVersionControlProviders_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get a handle to the dropdown
                DropDownHierarchy ddlProjectSettings = (DropDownHierarchy)e.Row.FindControl("ddlProjectSettings");
                if (ddlProjectSettings != null)
                {
                    ddlProjectSettings.DataSource = this.projects;
                    ddlProjectSettings.NoValueItemText = Resources.Dialogs.Global_PleaseSelectDropDown;
                }
            }
        }

        /// <summary>
        /// Adds conditional formatting and selects the projects
        /// </summary>
        private void GrdVersionControlProviders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                VersionControlSystem versionControlSystem = (VersionControlSystem)e.Row.DataItem;

                //Get a handle to the dropdown
                DropDownHierarchy ddlProjectSettings = (DropDownHierarchy)e.Row.FindControl("ddlProjectSettings");
                if (ddlProjectSettings != null)
                {
                    //Get the list of active projects for this plugin,
                    //Display active projects as 'summary' items that are bold and a different icon
                    List<VersionControlProject> versionControlProjects = new SourceCodeManager().RetrieveProjectsForSystem(versionControlSystem.VersionControlSystemId);
                    foreach (ListItem listItem in ddlProjectSettings.Items)
                    {
                        if (versionControlProjects.Any(d => d.ProjectId.ToString() == listItem.Value && d.IsActive))
                        {
                            listItem.Attributes[DropDownHierarchy.AttributeKey_Summary] = "Y";
                        }
                        if (!String.IsNullOrEmpty(listItem.Value))
                        {
                            listItem.Attributes[DropDownHierarchy.AttributeKey_IndentLevel] = "AAA";    //Indent one position
                        }
                    }

                    //Set the base URL
                    ddlProjectSettings.BaseUrl = UrlRoots.RetrieveProjectAdminUrl(-2, "VersionControlProjectSettings") + "?" + GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID + "=" + versionControlSystem.VersionControlSystemId;

                    //Set the current project if it's active for this plugin, or if there are no active providers for this project
                    if (ProjectId > 0 && (!this.anyActiveProviders || (versionControlProjects.Any(d => d.ProjectId == ProjectId && d.IsActive))))
                    {
                        ddlProjectSettings.SelectedValue = ProjectId.ToString();
                    }
                }

                //Change the color of the cell depending on the status
                if (versionControlSystem.IsActive)
                {
                    e.Row.Cells[3].CssClass = "bg-success";
                }
                else
                {
                    //Inactive
                    e.Row.Cells[3].CssClass = "bg-warning";
                }
            }
        }

        /// <summary>
        /// Called when you click on the button to add a version control provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAdd_Click(object sender, EventArgs e)
        {
            //Redirect to the provider add/edit screen in add mode
            Response.Redirect("VersionControlProviderDetails.aspx");
        }

        /// <summary>
        /// Called when you click on a link in the provider grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdVersionControlProviders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //See which command was executed
            if (e.CommandName == "EditProvider")
            {
                //Redirect to the provider add/edit screen in add mode
                int versionControlSystemId = Int32.Parse((string)e.CommandArgument);
                Response.Redirect("VersionControlProviderDetails.aspx?" + GlobalFunctions.PARAMETER_VERSION_CONTROL_SYSTEM_ID + "=" + versionControlSystemId);
            }
            else if (e.CommandName == "DeleteProvider")
            {
                //Delete and reload the list
                SourceCodeManager sourceCode = new SourceCodeManager();
                int versionControlSystemId = Int32.Parse((string)e.CommandArgument);
                sourceCode.DeleteSystem(versionControlSystemId);
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Loads and displays the page's contents when called
        /// </summary>
        protected void LoadAndBindData()
        {
            //Get a list of all active projects
            ProjectManager projectManager = new ProjectManager();
            this.projects = projectManager.Retrieve();

            //Load the list of version control providers and databind
            SourceCodeManager sourceCodeManager = new SourceCodeManager();
            this.grdVersionControlProviders.DataSource = sourceCodeManager.RetrieveSystems();
            this.grdVersionControlProviders.DataBind();

            //Display if the cache is running or not
            if (SourceCodeManager.IsCacheUpdateRunning)
            {
                this.msgStatus.Text = Resources.Messages.VersionControl_CacheUpdateRunning;
                this.msgStatus.Type = ServerControls.MessageBox.MessageType.Error;
            }
            else
            {
                this.msgStatus.Text = Resources.Messages.VersionControl_CacheUpToDate;
                this.msgStatus.Type = ServerControls.MessageBox.MessageType.Information;
            }
        }
    }
}
