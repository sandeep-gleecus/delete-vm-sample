using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Edit Components Page and handling all raised events
    /// </summary>fire1666
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_EditComponents", "Product-Planning/#edit-components", "Admin_EditComponents"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Components : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Components";

        //Bound data for the grid
        protected SortedList<string, string> flagList;

        /// <summary>
        /// Loads the page data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Redirect if there's no project selected.
                if (ProjectId < 1)
                {
                    Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);
                }

                //Register event handlers
                this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
                this.grdComponents.RowCommand += new GridViewCommandEventHandler(grdComponents_RowCommand);
                this.grdComponents.RowDataBound += new GridViewRowEventHandler(grdComponents_RowDataBound);
                this.btnCancel.Click += new EventHandler(btnCancel_Click);
                this.btnSave.Click += new EventHandler(btnSave_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadAndBindData();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds selective formatting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdComponents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                Component component = (Component)e.Row.DataItem;
                if (component.IsDeleted)
                {
                    e.Row.CssClass = "Deleted";
                }
            }
        }

        /// <summary>
        /// Saves the data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSave_Click(object sender, EventArgs e)
        {
            bool succSave = this.SaveData();
            if (!succSave)
            {
                this.lblMessage.Text = Resources.Messages.Admin_Components_SaveError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
            else
            {
                this.lblMessage.Text = Resources.Messages.Admin_Components_Saved;
                this.lblMessage.Type = MessageBox.MessageType.Information;
            }

            this.LoadAndBindData();
        }

        /// <summary>
        /// Returns the administration home page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            SaveData();
            LoadAndBindData();
        }

        /// <summary>
        /// Called when a button in the grid is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdComponents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //See what command was fired
            
            //Add a row
            if (e.CommandName.ToLowerInvariant() == "addnew")
            {
                //First save any changes
                SaveData();

                //Populate the active flag                
                this.flagList = new ComponentManager().RetrieveFlagLookup();

                //Create an empty row..
                List<Component> components = RetrieveComponents();
                components.Add(new Component() { ComponentId = -1, IsDeleted = false, IsActive = true, Name = "", ProjectId = ProjectId });
                this.grdComponents.DataSource = components;
                this.grdComponents.DataBind();
            }

            //Delete a row
            if (e.CommandName == "Remove")
            {
                //Save the data
                SaveData();

                //Find the row
                int componentId = Int32.Parse((string)e.CommandArgument);
                new ComponentManager().Component_Delete(componentId);

                //Reload
                LoadAndBindData();
            }

            //Undelete a row
            if (e.CommandName == "Undelete")
            {
                //Save the data
                SaveData();

                //Find the row
                int componentId = Int32.Parse((string)e.CommandArgument);
                new ComponentManager().Component_Undelete(componentId);

                //Reload
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Displays the status of the component
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        protected string GetStatus(Component component)
        {
            if (component.IsDeleted)
            {
                return Resources.Main.Global_Deleted;
            }
            else
            {
                if (component.IsActive)
                {
                    return Resources.Fields.ActiveYn;
                }
                else
                {
                    return Resources.Main.Global_Inactive;
                }
            }
        }

        /// <summary>
        /// Saves the data in the grid
        /// </summary>
        /// <returns></returns>
        protected bool SaveData()
        {
            bool retStatus = false;
            ComponentManager componentManager = new ComponentManager();

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            try
            {
                for (int i = 0; i < this.grdComponents.Rows.Count; i++)
                {
                    //We only look at item rows (i.e. not headers and footers)
                    GridViewRow row = this.grdComponents.Rows[i];
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        //Extract the various controls from the datagrid
                        int componentId = (int)this.grdComponents.DataKeys[i].Value;
                        TextBoxEx txtName = (TextBoxEx)row.FindControl("txtName");
                        CheckBoxEx chkActive = (CheckBoxEx)row.FindControl("chkActive");

                        if (!String.IsNullOrEmpty(txtName.Text.Trim()))
                        {
                            //See if we have an ID for this row (it's a new addition if not)
                            if (componentId < 0)
                            {
                                //New component, add it.
                                string name = txtName.Text.Trim();
                                bool isActive = (chkActive.Checked);
                                componentManager.Component_Insert(ProjectId, name, isActive);
                            }
                            else
                            {
                                //Existing component, update it.
                                Component component = componentManager.Component_RetrieveById(componentId, true);

                                //Make sure we found the matching row
                                if (component != null)
                                {
                                    //Update the various fields
                                    component.StartTracking();
                                    component.Name = txtName.Text.Trim();
                                    component.IsActive = (chkActive.Checked);

                                    //save it.
                                    componentManager.Component_Update(component);
                                }
                            }
                        }
                    }
                }
                retStatus = true;
            }
            catch (Exception exception)
            {
                retStatus = false;
                Logger.LogErrorEvent(CLASS_NAME + "SaveData()", exception, "Saving data rows.");
            }

            return retStatus;
        }

        /// <summary>
        /// Loads the components configured for the current project
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Populate any static fields
            this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

            //Get the yes/no flag list
            ComponentManager componentManager = new ComponentManager();
            this.flagList = componentManager.RetrieveFlagLookup();

            //Load the list of components into the grid
            List<Component> components = RetrieveComponents();
            this.grdComponents.DataSource = components;
            this.grdComponents.DataBind();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Gets the list of components for the current filter option
        /// </summary>
        /// <returns></returns>
        protected List<Component> RetrieveComponents()
        {
            ComponentManager componentManager = new ComponentManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");
            bool includeDeleted = (filterType == "all");

            //Return the component list
            return componentManager.Component_Retrieve(ProjectId, activeOnly, includeDeleted);
        }
    }
}