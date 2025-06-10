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

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for managing report in the system
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Reports", "System-Reporting/#edit-reports", "Admin_Reports"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ReportAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Reports : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Reports::";

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Add the event handlers
            this.grdReports.RowCommand += new GridViewCommandEventHandler(grdReports_RowCommand);

            //Load and bind data
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Called when a button in the grid is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdReports_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdReports_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //See which command was clicked
                if (e.CommandName == "CopyReport")
                {
                    int reportId = Int32.Parse((string)e.CommandArgument);
                    new ReportManager().Report_Copy(reportId);
                    LoadAndBindData();
                    this.lblMessage.Text = Resources.Messages.Admin_Reports_ReportCopied;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                if (e.CommandName == "DeleteReport")
                {
                    int reportId = Int32.Parse((string)e.CommandArgument);
                    new ReportManager().Report_Delete(reportId);
                    LoadAndBindData();
                    this.lblMessage.Text = Resources.Messages.Admin_Reports_ReportDeleted;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Loads the report list
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Load in all the reports
                ReportManager reportManager = new ReportManager();
                List<Report> reports = reportManager.Report_Retrieve(false);

                this.grdReports.DataSource = reports;
                this.grdReports.DataBind();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
