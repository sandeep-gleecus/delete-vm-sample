using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin
{
    public partial class ProjectList : WebPartBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin.ProjectList::";

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
                this.grdProjectList.RowDataBound += new GridViewRowEventHandler(grdProjectList_RowDataBound);

                //Set the URL format for the project column
                ((NameDescriptionFieldEx)this.grdProjectList.Columns[0]).NavigateUrlFormat = UrlRewriterModule.RetrieveProjectAdminUrl(-3, "Default");

                //Now load the content
                if (!IsPostBack && WebPartVisible)
                {
                    ProjectManager projectManager = new ProjectManager();
                    List<Project> projects = projectManager.RetrieveForTemplate(ProjectTemplateId);
                    this.grdProjectList.DataSource = projects;
                    this.grdProjectList.DataBind();
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
        /// Applies any selective formatting to the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdProjectList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Ignore headers, footers, etc.
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //See if the current user is allowed to view the project group dashboard
                Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
                Project project = ((Project)e.Row.DataItem);
                int projectGroupId = project.ProjectGroupId;
                int projectId = project.ProjectId;
                e.Row.Cells[1].Enabled = projectGroupManager.IsAuthorized(UserId, projectGroupId);

                //Set the project group URL
                HyperLinkEx lnkProjectGroup = (HyperLinkEx)e.Row.Cells[1].FindControl("lnkProjectGroup");
                if (lnkProjectGroup != null)
                {
                    lnkProjectGroup.NavigateUrl = UrlRewriterModule.RetrieveGroupAdminUrl(projectGroupId, "Edit");
                }
            }
        }

    }
}