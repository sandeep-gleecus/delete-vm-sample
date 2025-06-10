using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using Microsoft.Security.Application;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>Displays the project administration data-synchronization home page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataSynchronization_Title", "System-Administration", "Admin_DataSynchronization_Title")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class DataSynchronization : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.DataSynchronization::";
		protected string productName = "";

		DataMappingManager dataMappingManager = new DataMappingManager();

		/// <summary>Called when the control is first loaded</summary>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Register any event handlers
			btnRefresh.Click += new EventHandler(btnRefresh_Click);
			grdDataSynchronization.RowDataBound += grdDataSynchronization_RowDataBound;

			//Load the page if not postback
			if (!Page.IsPostBack)
			{
				LoadAndBindData();
			}

			//Set the return to project admin home url
			lnkAdminHome.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			//Set the licensed product name (used in several places)
			productName = ConfigurationSettings.Default.License_ProductType;
		}

		/// <summary>Adds selective formatting to the status field</summary>
		void grdDataSynchronization_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				DataSyncSystem dataSyncSystem = (DataSyncSystem)e.Row.DataItem;

				//We need to see if this project is active for this system
				try
				{
					DataSyncProject dataSyncProject = dataMappingManager.RetrieveDataSyncProject(dataSyncSystem.DataSyncSystemId, ProjectId);
					if (dataSyncProject != null)
					{
						e.Row.Cells[2].Text = GlobalFunctions.DisplayYnFlag(dataSyncProject.ActiveYn);
						//Change the color of the cell depending on the status
						if (dataSyncProject.ActiveYn == "Y")
						{
							switch ((DataSyncSystem.DataSyncStatusEnum)dataSyncSystem.DataSyncStatusId)
							{
								case DataSyncSystem.DataSyncStatusEnum.Failure:
									e.Row.Cells[3].CssClass = "bg-danger";
									break;
								case DataSyncSystem.DataSyncStatusEnum.NotRun:
									e.Row.Cells[3].CssClass = "bg-info";
									break;
								case DataSyncSystem.DataSyncStatusEnum.Success:
									e.Row.Cells[3].CssClass = "bg-success";
									break;
								case DataSyncSystem.DataSyncStatusEnum.Warning:
									e.Row.Cells[3].CssClass = "bg-warning";
									break;
							}
							e.Row.Cells[4].Text = Encoder.HtmlEncode(dataSyncProject.ExternalKey);
						}
						else
						{
							//Inactive for this project
							e.Row.Cells[3].CssClass = "bg-light-gray";
							e.Row.Cells[3].Text = Resources.Fields.NA;
							e.Row.Cells[4].Text = "-";
						}
					}
				}
				catch (DataSyncNotConfiguredException)
				{
					//Inactive for this project
					e.Row.Cells[2].Text = GlobalFunctions.DisplayYnFlag(false);
					e.Row.Cells[3].CssClass = "bg-light-gray";
					e.Row.Cells[3].Text = Resources.Fields.NA;
					e.Row.Cells[4].Text = "-";
				}
			}
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
			//Display the project name
			lblProjectName.Text = Encoder.HtmlEncode(ProjectName);

			//Load the list of active (since project admin only) data-syncs and databind
			grdDataSynchronization.DataSource = dataMappingManager.RetrieveDataSyncSystems(true);
			grdDataSynchronization.DataBind();
		}
	}
}
