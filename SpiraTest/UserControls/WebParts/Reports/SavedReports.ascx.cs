using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Reports
{
    /// <summary>
    /// Displays a list of reports the user has saved
    /// </summary>
    public partial class SavedReports : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SavedReports::";

        /// <summary>
        /// Called when the control is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Register event handlers
                this.lstSavedReports.ItemDataBound += new DataListItemEventHandler(lstSavedReports_ItemDataBound);
                this.lstSavedReports.ItemCommand += new DataListCommandEventHandler(lstSavedReports_ItemCommand);

                //Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
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
        /// Handles clicks on the display and delete buttons in the list of saved reports
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void lstSavedReports_ItemCommand(object source, DataListCommandEventArgs e)
        {
            //See which command was executed
            if (e.CommandName == "DeleteReport")
            {
                int reportSavedId = Int32.Parse((string)e.CommandArgument);
                //Delete the report and then databind
                ReportManager reportManager = new ReportManager();
                reportManager.DeleteSaved(reportSavedId);
                List<SavedReportView> savedReports = reportManager.RetrieveSaved(UserId, ProjectId, true);
                this.lstSavedReports.DataSource = savedReports;
                this.lstSavedReports.DataBind();
            }
        }

        /// <summary>
        /// Applies selective formatting to the items in the saved reports
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lstSavedReports_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            //We only want to touch data rows
            if ((e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) && e.Item.DataItem != null)
            {
                //If the user associated with the report doesn't match the current user then hide
                //the Delete button
                SavedReportView savedReport = (SavedReportView)e.Item.DataItem;
                if (savedReport.UserId != UserId)
                {
                    LinkButtonEx linkButton = (LinkButtonEx)e.Item.FindControl("btnDeleteReport");
                    if (linkButton != null)
                    {
                        linkButton.Visible = false;
                    }
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
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            //Load the list of saved reports
            ReportManager reportManager = new ReportManager();
            List<SavedReportView> savedReports = reportManager.RetrieveSaved(UserId, ProjectId, true);
            this.lstSavedReports.DataSource = savedReports;
            //Display the none label if no reports available
            if (savedReports.Count == 0)
            {
                this.lblNoSavedReports.Visible = true;
                this.lstSavedReports.Visible = false;
            }

            this.lstSavedReports.DataBind();
        }
    }
}
