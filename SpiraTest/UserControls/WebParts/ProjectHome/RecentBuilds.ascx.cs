using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays the list of recent builds for a release or iteration
    /// </summary>
    public partial class RecentBuilds : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RecentBuilds::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is unlimited
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        ]
        public Nullable<int> RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                this.rowsToDisplay = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected Nullable<int> rowsToDisplay = null;

        #endregion

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
                //Register event handlers
                this.grdBuildList.RowDataBound += new GridViewRowEventHandler(grdBuildList_RowDataBound);

                //Now load the content
                if (WebPartVisible)
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
        /// Adds the selective formatting to the build grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdBuildList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                BuildView buildView = (BuildView)e.Row.DataItem;
                e.Row.Cells[2].CssClass = GlobalFunctions.GetBuildStatusCssClass((Build.BuildStatusEnum)buildView.BuildStatusId);
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
            //Get the release id from settings
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

            //If no release chosen, just get the latest builds for the project, we used to display a message that they needed to select a release
            /*if (releaseId == -1)
            {
                this.lblNoData.Text = Resources.Messages.RecentBuilds_NeedToSelectRelease;
                this.grdBuildList.Visible = false;
                return;
            }*/
            this.grdBuildList.Visible = true;
            this.lblNoData.Text = "";

            BuildManager buildManager = new BuildManager();
            int pageSize = 5;
            if (RowsToDisplay.HasValue)
            {
                pageSize = RowsToDisplay.Value;
            }

            int artifactCount;
            List<BuildView> builds;
            if (releaseId == -1)
            {
                builds = buildManager.RetrieveForProject(ProjectId, "CreationDate DESC", 0, pageSize, null, out artifactCount, GlobalFunctions.GetCurrentTimezoneUtcOffset());
            }
            else
            {
                builds = buildManager.RetrieveForRelease(ProjectId, releaseId, "CreationDate DESC", 0, pageSize, null, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out artifactCount);
            }

            //Set the navigate url for the name field
            ((NameDescriptionFieldEx)this.grdBuildList.Columns[1]).NavigateUrlFormat = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Builds, ProjectId, -3);

            this.grdBuildList.DataSource = builds;
            this.grdBuildList.DataBind();
        }
    }
}