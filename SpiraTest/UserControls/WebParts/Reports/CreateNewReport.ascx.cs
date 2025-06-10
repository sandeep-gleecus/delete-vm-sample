using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Reports
{
    /// <summary>
    /// Displays the list of new reports that the user can create/configure
    /// </summary>
    public partial class CreateNewReport : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.CreateNewReport::";

        protected List<Report> reports;

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
                this.rptReportCategories.ItemCreated += new RepeaterItemEventHandler(rptReportCategories_ItemCreated);
                this.rptReportCategories.ItemDataBound += new RepeaterItemEventHandler(rptReportCategories_ItemDataBound);

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
        /// Makes reports hidden that the user doesn't have permission to see
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rptReportCategories_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //Get the report category in question
                if (e.Item.DataItem != null)
                {
                    ReportCategory reportCategory = (ReportCategory)e.Item.DataItem;
                    //See if the user has permissions to see these reports or not
                    if (reportCategory.ArtifactTypeId.HasValue)
                    {
                        int artifactTypeId = reportCategory.ArtifactTypeId.Value;
                        Business.ProjectManager projectManager = new Business.ProjectManager();
                        e.Item.Visible = (projectManager.IsAuthorized(ProjectRoleId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the list of reports to each category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void rptReportCategories_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //Get the report category in question
                if (e.Item.DataItem != null)
                {
                    ReportCategory reportCategory = (ReportCategory)e.Item.DataItem;
                    int reportCategoryId = reportCategory.ReportCategoryId;
                    ReportManager reportManager = new ReportManager();
                    this.reports = reportManager.RetrieveByCategoryId(reportCategoryId);
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
            //Load the list of report categories
            ReportManager reportManager = new ReportManager();
            List<ReportCategory> categories = reportManager.RetrieveCategories();
            this.rptReportCategories.DataSource = categories;

            //Databind the form
            this.DataBind();
        }
    }
}