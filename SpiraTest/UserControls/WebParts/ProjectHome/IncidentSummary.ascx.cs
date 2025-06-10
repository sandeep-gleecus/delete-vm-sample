using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the table that summarizes the count of incidents
	/// </summary>
	public partial class IncidentSummary : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.IncidentSummary::";

		#region Enumerations

		public enum IncidentSummaryOrganizeBy
		{
			Priority = 1,
			Severity = 2
		}

        public enum IncidentCountReleaseType
        {
            DetectedRelease = 1,
            ResolvedRelease = 2
        }


		#endregion

		#region User Configurable Properties

        /// <summary>
        /// Determines whether to filter by detected release or resolved release
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_ChooseReleaseTypeSetting"),
        LocalizedWebDescription("Global_ChooseReleaseTypeSettingTooltip")
        ]
        public IncidentCountReleaseType ReleaseType
        {
            get
            {
                return this.releaseType;
            }
            set
            {
                this.releaseType = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected IncidentCountReleaseType releaseType = IncidentCountReleaseType.DetectedRelease;

		/// <summary>
		/// Determines whether to display priority or severity on the x-axis
		/// </summary>
		[
		WebBrowsable,
		Personalizable,
        LocalizedWebDisplayName("Global_UsePrioritySeveritySetting"),
        LocalizedWebDescription("Global_UsePrioritySeveritySettingTooltip")
		]
		public IncidentSummaryOrganizeBy OrganizeBy
		{
			get
			{
				return this.organizeBy;
			}
			set
			{
				this.organizeBy = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected IncidentSummaryOrganizeBy organizeBy = IncidentSummaryOrganizeBy.Priority;

		#endregion

		/// <summary>
		/// Overrides the onInit method to add any dynamic columns
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnInit(EventArgs e)
		{
			//Add the dynamic columns to the incident summary datagrid
			//Needs to be added in the OnInit method since they need
			//to be added before viewstate is loaded and load() methods called
			AddDynamicColumns();
			base.OnInit(e);
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Add any event handlers
				this.grdIncidentSummary.RowCommand += new GridViewCommandEventHandler(grdIncidentSummary_RowCommand);
				this.ddlIncidentTypeFilter.SelectedIndexChanged += new EventHandler(ddlIncidentTypeFilter_SelectedIndexChanged);

				//Now load the content
				if (!IsPostBack && WebPartVisible)
				{
					LoadAndBindData();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				if (this.Message != null)
				{
					this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
					this.Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Adds the configurable fields to the list of columns
		/// </summary>
		private void AddDynamicColumns()
		{
			if (ProjectTemplateId > 0)
			{
				//We need to dynamically add the incident priorities and severities as columns to the Incident Summary table
				//We need to add both because the OrganizeBy property hasn't been set yet by WebParts
				//so we don't know which we need to add
				IncidentManager incidentManager = new IncidentManager();
				//First the severities

				List<IncidentSeverity> incidentSeverities = incidentManager.RetrieveIncidentSeverities(ProjectTemplateId, true);
                for (int i = 0; i < incidentSeverities.Count; i++)
				{
					Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx buttonColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
					buttonColumn.DataTextField = incidentSeverities[i].SeverityId.ToString();
					buttonColumn.CommandName = "ColumnId:" + incidentSeverities[i].SeverityId.ToString();
					buttonColumn.CommandArgumentField = "IncidentStatusId";
					buttonColumn.ButtonType = ButtonType.Link;
					buttonColumn.HeaderText = incidentSeverities[i].Name;
					buttonColumn.FooterField = incidentSeverities[i].SeverityId.ToString();
					buttonColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
					buttonColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
					buttonColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.CssClass = "priority4";
                    buttonColumn.HeaderStyle.CssClass = "priority4";
                    buttonColumn.FooterStyle.CssClass = "priority4";
					buttonColumn.MetaData = "Severity";
					this.grdIncidentSummary.Columns.Add(buttonColumn);
				}

				//Next the priorities
                List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(ProjectTemplateId, true);
                for (int i = 0; i < incidentPriorities.Count; i++)
				{
					Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx buttonColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
					buttonColumn.DataTextField = incidentPriorities[i].PriorityId.ToString();
					buttonColumn.CommandName = "ColumnId:" + incidentPriorities[i].PriorityId.ToString();
					buttonColumn.CommandArgumentField = "IncidentStatusId";
					buttonColumn.ButtonType = ButtonType.Link;
					buttonColumn.HeaderText = incidentPriorities[i].Name;
					buttonColumn.FooterField = incidentPriorities[i].PriorityId.ToString();
					buttonColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
					buttonColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
					buttonColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.CssClass = "priority4";
                    buttonColumn.HeaderStyle.CssClass = "priority4";
                    buttonColumn.FooterStyle.CssClass = "priority4";
					buttonColumn.MetaData = "Priority";
					this.grdIncidentSummary.Columns.Add(buttonColumn);
				}

				//Now add the column for the (None) and TOTAL columns
				Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx noneColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
				noneColumn.DataTextField = "None";
				noneColumn.CommandName = "ColumnNone";
				noneColumn.CommandArgumentField = "IncidentStatusId";
				noneColumn.ButtonType = ButtonType.Link;
                noneColumn.HeaderText = "(" + Resources.Fields.None + ")";
				noneColumn.FooterField = "None";
				noneColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
				noneColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
				noneColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                noneColumn.ItemStyle.CssClass = "priority4";
                noneColumn.HeaderStyle.CssClass = "priority4";
                noneColumn.FooterStyle.CssClass = "priority4";
				this.grdIncidentSummary.Columns.Add(noneColumn);

				Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx totalColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
				totalColumn.DataTextField = "Total";
				totalColumn.ButtonType = ButtonType.Link;
				totalColumn.CommandName = "ColumnTotal";
				totalColumn.CommandArgumentField = "IncidentStatusId";
                totalColumn.HeaderText = Resources.Fields.TotalCaps;
				totalColumn.FooterField = "Total";
				totalColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
				totalColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
				totalColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                totalColumn.ItemStyle.CssClass = "priority1";
                totalColumn.HeaderStyle.CssClass = "priority1";
                totalColumn.FooterStyle.CssClass = "priority1";
				totalColumn.ItemStyle.Font.Bold = true;
				this.grdIncidentSummary.Columns.Add(totalColumn);
			}
		}

		/// <summary>
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		/// <summary>
		/// Loads the control data
		/// </summary>
		public void LoadAndBindData()
		{
			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);
            if (releaseId.HasValue && releaseId.Value < 1)
            {
                releaseId = null;
            }
			IncidentManager incidentManager = new IncidentManager();

			//We need to make the appropriate columns visible or not (severity vs. priority)
			foreach (DataControlField dataControlField in this.grdIncidentSummary.Columns)
			{
				if (dataControlField is ButtonFieldEx)
				{
					ButtonFieldEx buttonField = (ButtonFieldEx)dataControlField;
					if (buttonField.MetaData == "Priority" && this.organizeBy != IncidentSummaryOrganizeBy.Priority)
					{
						buttonField.Visible = false;
					}
					if (buttonField.MetaData == "Severity" && this.organizeBy != IncidentSummaryOrganizeBy.Severity)
					{
						buttonField.Visible = false;
					}
				}
			}

			//If we have a postback, the drop-down has viewstate enabled so we can just get its current value
			int? incidentTypeId = null;
			if (this.ddlIncidentTypeFilter == null || !Page.IsPostBack)
			{
				incidentTypeId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENT_TYPE, (int?)null);
                if (incidentTypeId.HasValue && incidentTypeId.Value < 1)
                {
                    incidentTypeId = null;
                }
			}
			else
			{
				//We have postback case
				if (this.ddlIncidentTypeFilter.SelectedValue == "")
				{
					incidentTypeId = null;
				}
				else
				{
					incidentTypeId = Int32.Parse(this.ddlIncidentTypeFilter.SelectedValue);
				}
			}

			//Now get the incident summary untyped dataset (organized by priority or severity)
			bool useSeverity = (this.OrganizeBy == IncidentSummaryOrganizeBy.Severity);
            bool useResolvedRelease = (this.ReleaseType == IncidentCountReleaseType.ResolvedRelease);
            DataSet incidentSummaryDataSet = incidentManager.RetrieveProjectSummary(ProjectId, ProjectTemplateId, incidentTypeId, releaseId, useSeverity, useResolvedRelease);
			this.grdIncidentSummary.DataSource = incidentSummaryDataSet;
			this.grdIncidentSummary.DataBind();

			//Get the incident type lookup data
			List<IncidentType> incidentTypes = incidentManager.RetrieveIncidentTypes(ProjectTemplateId, true);
            this.ddlIncidentTypeFilter.DataSource = incidentTypes;
			this.ddlIncidentTypeFilter.DataBind();

			//Now we need to set the dropdown to the selected incident type id
			if (incidentTypeId.HasValue && this.ddlIncidentTypeFilter != null)
			{
				try
				{
					ddlIncidentTypeFilter.SelectedValue = incidentTypeId.Value.ToString();
				}
				catch (Exception)
				{
					//Fail quietly in the case where the incident type has been deactivated
				}
			}
		}

		/// <summary>
		/// This event handler applies the filter to the incident summary grid
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdIncidentSummary_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdIncidentSummary_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

			//Check to see which command was executed
			//Check to see if the None or Total column cells were clicked
			if ((e.CommandName == "ColumnNone" || e.CommandName == "ColumnTotal") && e.CommandArgument.ToString() != "")
			{
				//We need to redirect to the incident list, filtered by whatever the pulldown is set to
				//and based on where in the table the link was clicked (status only)
				//We leave the sort setting alone
				ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
				filters.Clear();

				//Get the currently saved value of the incident type dropdown
				int? incidentTypeId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENT_TYPE, (int?)null);
				if (incidentTypeId.HasValue && incidentTypeId.Value > 0)
				{
					filters.Add("IncidentTypeId", incidentTypeId.Value);
				}

				//Add a detected-by release filter if appropriate
				if (releaseId != -1)
				{
					filters.Add("DetectedReleaseId", releaseId);
				}

				//Get the incident status id
				int incidentStatusId = Int32.Parse(e.CommandArgument.ToString());
				filters.Add("IncidentStatusId", incidentStatusId);
				filters.Save();
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId), true);
			}

			//Check to see if the individual cell links were clicked
			if (e.CommandName.Length > "ColumnId:".Length && e.CommandName.Substring(0, "ColumnId:".Length) == "ColumnId:" && e.CommandArgument.ToString() != "")
			{
				//We need to redirect to the incident list, filtered by whatever the pulldown is set to
				//and based on where in the table the link was clicked (priority vs status)
				//We leave the sort setting alone
				ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
				filters.Clear();
				if (ddlIncidentTypeFilter.SelectedValue != null && ddlIncidentTypeFilter.SelectedValue != "")
				{
					filters.Add("IncidentTypeId", Int32.Parse(ddlIncidentTypeFilter.SelectedValue));
				}
				//See if we're using priority or severity for the x-axis
				if (OrganizeBy == IncidentSummaryOrganizeBy.Severity)
				{
					//Get the severity id and the incident status id
					int severityId = Int32.Parse(e.CommandName.Substring("ColumnId:".Length, e.CommandName.Length - "ColumnId:".Length));
					filters.Add("SeverityId", severityId);
				}
				else
				{
					//Get the priority id and the incident status id
					int priorityId = Int32.Parse(e.CommandName.Substring("ColumnId:".Length, e.CommandName.Length - "ColumnId:".Length));
					filters.Add("PriorityId", priorityId);
				}

				//Add a detected-by release filter if appropriate
				if (releaseId != -1)
				{
					filters.Add("DetectedReleaseId", releaseId);
				}

				int incidentStatusId = Int32.Parse(e.CommandArgument.ToString());
				filters.Add("IncidentStatusId", incidentStatusId);
				filters.Save();
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId), true);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles changes to the incident type filter dropdown
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		void ddlIncidentTypeFilter_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Save the value from the dropdown
			if (this.ddlIncidentTypeFilter.SelectedValue == "")
			{
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENT_TYPE, (int?)null);
			}
			else
			{
				SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_INCIDENT_TYPE, Int32.Parse(this.ddlIncidentTypeFilter.SelectedValue));
			}

			//Now reload the widget
			LoadAndBindData();
		}
	}
}