using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	/// <summary>
	/// Displays the table summarizing the count of requirements
	/// </summary>
	public partial class RequirementsSummary : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementsSummary::";

        /// <summary>
        /// Overrides the onInit method to add any dynamic columns
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            //Add the dynamic columns to the requirement summary datagrid
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
				//Add Event handlers
				this.grdRequirementSummary.RowCommand += new GridViewCommandEventHandler(grdRequirementSummary_RowCommand);

				//Now load the content
				if (!IsPostBack)
				{
                    if (WebPartVisible)
                    {
                        LoadAndBindData();
                    }
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
		/// Returns a handle to the interface
		/// </summary>
		/// <returns>IWebPartReloadable</returns>
		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

        /// <summary>
        /// Adds the configurable fields to the list of columns
        /// </summary>
        private void AddDynamicColumns()
        {
            if (ProjectTemplateId > 0)
            {
                //We need to dynamically add the requirement priorities and severities as columns to the Requirement Summary table
                //We need to add both because the OrganizeBy property hasn't been set yet by WebParts
                //so we don't know which we need to add
                RequirementManager requirementManager = new RequirementManager();
                //First the severities

                //Next the priorities
                List<Importance> importances = requirementManager.RequirementImportance_Retrieve(ProjectTemplateId, true);
                for (int i = 0; i < importances.Count; i++)
                {
                    Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx buttonColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
                    buttonColumn.DataTextField = importances[i].ImportanceId.ToString();
                    buttonColumn.CommandName = "ImportanceId:" + importances[i].ImportanceId.ToString();
                    buttonColumn.CommandArgumentField = "RequirementStatusId";
                    buttonColumn.ButtonType = ButtonType.Link;
                    buttonColumn.HeaderText = Microsoft.Security.Application.Encoder.HtmlEncode(importances[i].Name);
                    buttonColumn.FooterField = importances[i].ImportanceId.ToString();
                    buttonColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                    buttonColumn.ItemStyle.CssClass = "priority4";
                    buttonColumn.HeaderStyle.CssClass = "priority4";
                    buttonColumn.FooterStyle.CssClass = "priority4";
                    buttonColumn.MetaData = "Importance";
                    this.grdRequirementSummary.Columns.Add(buttonColumn);
                }

                //Now add the column for the (None) and TOTAL columns
                Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx noneColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
                noneColumn.DataTextField = "None";
                noneColumn.CommandName = "ImportanceNone";
                noneColumn.CommandArgumentField = "RequirementStatusId";
                noneColumn.ButtonType = ButtonType.Link;
                noneColumn.HeaderText = "(" + Resources.Fields.None + ")";
                noneColumn.FooterField = "None";
                noneColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                noneColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
                noneColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                noneColumn.ItemStyle.CssClass = "priority4";
                noneColumn.HeaderStyle.CssClass = "priority4";
                noneColumn.FooterStyle.CssClass = "priority4";
                this.grdRequirementSummary.Columns.Add(noneColumn);

                Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx totalColumn = new Inflectra.SpiraTest.Web.ServerControls.ButtonFieldEx();
                totalColumn.DataTextField = "Total";
                totalColumn.ButtonType = ButtonType.Link;
                totalColumn.CommandName = "ImportanceTotal";
                totalColumn.CommandArgumentField = "RequirementStatusId";
                totalColumn.HeaderText = Resources.Fields.TotalCaps;
                totalColumn.FooterField = "Total";
                totalColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                totalColumn.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
                totalColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                totalColumn.ItemStyle.CssClass = "priority1";
                totalColumn.HeaderStyle.CssClass = "priority1";
                totalColumn.FooterStyle.CssClass = "priority1";
                totalColumn.ItemStyle.Font.Bold = true;
                this.grdRequirementSummary.Columns.Add(totalColumn);
            }
        }

        /// <summary>
        /// Loads the control data
        /// </summary>
        public void LoadAndBindData()
		{
			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);

            //Convert old -1 saved releases to null
            if (releaseId.HasValue && releaseId < 1)
            {
                releaseId = null;
            }

            //Get the count of requirements, by priority and status. We hide all the rows with no values to avoid
            //taking up too much screen real-estate
			Business.RequirementManager requirementManager = new Business.RequirementManager();
            DataSet requirementSummaryDataSet = requirementManager.RetrieveProjectSummary(ProjectId, ProjectTemplateId, releaseId);
            requirementSummaryDataSet.Tables[0].DefaultView.RowFilter = "Total > 0";
			this.grdRequirementSummary.DataSource = requirementSummaryDataSet.Tables[0].DefaultView;
			this.grdRequirementSummary.DataBind();
		}

		/// <summary>
		/// This event handler handles click-events from the requirements summary datagrid
		/// </summary>
        /// <param name="source">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void grdRequirementSummary_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdRequirementSummary_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int? releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, (int?)null);

			//Check to see if the None or Total column cells were clicked
			if ((e.CommandName == "ImportanceNone" || e.CommandName == "ImportanceTotal") && e.CommandArgument.ToString() != "")
			{
				//We need to redirect to the requirements list, filtered by where in the table the
				//link was clicked (status only)
				ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);
				filters.Clear();
				//Get the requirement status id and set both it and the release id if applicable
				int requirementStatusId = Int32.Parse(e.CommandArgument.ToString());
                filters.Add("RequirementStatusId", requirementStatusId);
				if (releaseId.HasValue && releaseId > 0)
				{
					filters.Add("ReleaseId", releaseId.Value);
				}
                if (e.CommandName == "ImportanceNone")
                {
                    //Set the priority to None
                    filters.Add("ImportanceId", RequirementManager.NoneFilterValue);
                }
				filters.Save();
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, 0), true);
			}

			//Check to see if the individual cell links were clicked
			if (e.CommandName.Length > 13 && e.CommandName.Substring(0, 13) == "ImportanceId:" && e.CommandArgument.ToString() != "")
			{
				//We need to redirect to the requirements list, filtered by where in the table the
				//link was clicked (importance vs status)
				ProjectSettingsCollection filters = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);
				filters.Clear();
				//Get the priority id and the incident status id
				int importanceId = Int32.Parse(e.CommandName.Substring(13, e.CommandName.Length - 13));
				int requirementStatusId = Int32.Parse(e.CommandArgument.ToString());
				filters.Add("ImportanceId", importanceId);
				filters.Add("RequirementStatusId", requirementStatusId);
                if (releaseId.HasValue && releaseId > 0)
				{
					filters.Add("ReleaseId", releaseId.Value);
				}
				filters.Save();
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, 0), true);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
