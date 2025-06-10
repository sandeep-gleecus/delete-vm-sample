using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>Displays the administration data-synchronization home page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSynchronization_Title", "System-Integration/#data-synchronization", "Admin_DataSynchronization_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class DataSynchronization : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.DataSynchronization::";
		protected string productName = "";
        protected List<ProjectView> projects = null;

		/// <summary>Called when the control is first loaded</summary>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Register any event handlers
			btnRefresh.Click += new EventHandler(btnRefresh_Click);
			btnAdd.Click += new DropMenuEventHandler(btnAdd_Click);
			grdDataSynchronization.RowCommand += new GridViewCommandEventHandler(grdDataSynchronization_RowCommand);
			grdDataSynchronization.RowDataBound += grdDataSynchronization_RowDataBound;
            grdDataSynchronization.RowCreated += GrdDataSynchronization_RowCreated;

            //Load the page if not postback
            if (!Page.IsPostBack)
            {
                LoadAndBindData();
            }

			//Set the licensed product name (used in several places)
			productName = ConfigurationSettings.Default.License_ProductType;
		}

        /// <summary>
        /// Creates child elements - in this case the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GrdDataSynchronization_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get a handle to the dropdown
                DropDownHierarchy ddlProjectMappings = (DropDownHierarchy)e.Row.FindControl("ddlProjectMappings");
                if (ddlProjectMappings != null)
                {
                    ddlProjectMappings.DataSource = this.projects;
                    ddlProjectMappings.NoValueItemText = "--- " + Resources.Buttons.ViewProjectMappings + " ---";
                }
            }
        }

        /// <summary>Adds selective formatting to the status field</summary>
        void grdDataSynchronization_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				DataSyncSystem dataSyncSystem = (DataSyncSystem)e.Row.DataItem;

                //Get a handle to the dropdown
                DropDownHierarchy ddlProjectMappings = (DropDownHierarchy)e.Row.FindControl("ddlProjectMappings");
                if (ddlProjectMappings != null)
                {
                    //Get the list of active projects for this plugin,
                    //Display active projects as 'summary' items that are bold and a different icon
                    List<DataSyncProject> dataSyncProjects = new DataMappingManager().RetrieveDataSyncProjects(dataSyncSystem.DataSyncSystemId);
                    foreach (ListItem listItem in ddlProjectMappings.Items)
                    {
                        if (dataSyncProjects.Any(d => d.ProjectId.ToString() == listItem.Value && d.ActiveYn == "Y"))
                        {
                            listItem.Attributes[DropDownHierarchy.AttributeKey_Summary] = "Y";
                        }
                        if (!String.IsNullOrEmpty(listItem.Value))
                        {
                            listItem.Attributes[DropDownHierarchy.AttributeKey_IndentLevel] = "AAA";    //Indent one position
                        }
                    }

                    //Set the base URL
                    ddlProjectMappings.BaseUrl = UrlRoots.RetrieveProjectAdminUrl(-2, "DataSyncProjects") + "?" + GlobalFunctions.PARAMETER_DATA_SYNC_SYSTEM_ID + "=" + dataSyncSystem.DataSyncSystemId;

                    //Set the current project if it's active for this plugin
                    if (ProjectId > 0 && dataSyncProjects.Any(d => d.ProjectId == ProjectId && d.ActiveYn == "Y"))
                    {
                        ddlProjectMappings.SelectedValue = ProjectId.ToString();
                    }
                }

                //Change the color of the cell depending on the status
                if (dataSyncSystem.IsActive)
				{
					switch ((DataSyncSystem.DataSyncStatusEnum)dataSyncSystem.DataSyncStatusId)
					{
						case DataSyncSystem.DataSyncStatusEnum.Failure:
							e.Row.Cells[4].CssClass = "bg-danger";
							break;
						case DataSyncSystem.DataSyncStatusEnum.NotRun:
							e.Row.Cells[4].CssClass = "bg-info";
							break;
						case DataSyncSystem.DataSyncStatusEnum.Success:
							e.Row.Cells[4].CssClass = "bg-success";
							break;
						case DataSyncSystem.DataSyncStatusEnum.Warning:
							e.Row.Cells[4].CssClass = "bg-warning";
							break;
					}
				}
				else
				{
					//Basically used for N/A
					e.Row.Cells[4].CssClass = "bg-light-gray";
					e.Row.Cells[4].Text = Resources.Fields.NA;
				}
			}
		}

		/// <summary>Called when we want to enter a new data-sync plug-in</summary>
		void btnAdd_Click(object sender, EventArgs e)
		{
			//Redirect to the data-sync details page without passing an existing ID
			Response.Redirect("DataSyncDetails.aspx");
		}

		/// <summary>Refreshes the synchronization list when clicked</summary>
		void btnRefresh_Click(object sender, EventArgs e)
		{
			//Refresh the data-synchronization run info
			LoadAndBindData();
		}

		/// <summary>Loads and displays the panel's contents when called</summary>
		protected void LoadAndBindData()
		{
            //Get a list of all active projects
            ProjectManager projectManager = new ProjectManager();
            this.projects = projectManager.Retrieve();

            //Load the list of data-syncs and databind
            DataMappingManager dataMappingManager = new DataMappingManager();
			grdDataSynchronization.DataSource = dataMappingManager.RetrieveDataSyncSystems(false);
			grdDataSynchronization.DataBind();
		}

		/// <summary>This event handler forces the resync of the specified plug-in</summary>
		private void grdDataSynchronization_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "grdDataSynchronization_RowCommand()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//See which command was executed
			if (e.CommandName == "ForceReSync")
			{
				//Force a resync of the appropriate plug-in
				int dataSyncSystemId = int.Parse((string)e.CommandArgument);
				DataMappingManager dataMappingManager = new DataMappingManager();
				dataMappingManager.ResetLastRunInfo(dataSyncSystemId);
			}
			if (e.CommandName == "DeleteDataSync")
			{
				//Delete the plug-in completely
				int dataSyncSystemId = int.Parse((string)e.CommandArgument);
				DataMappingManager dataMappingManager = new DataMappingManager();
				dataMappingManager.DeleteDataSyncSystem(dataSyncSystemId);
			}

			//Refresh the data-synchronization run info
			LoadAndBindData();

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}
	}
}
