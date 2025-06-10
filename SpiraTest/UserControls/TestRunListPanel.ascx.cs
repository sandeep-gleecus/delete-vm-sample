using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls
{
    public partial class TestRunListPanel : ArtifactUserControlBase, IArtifactUserControl
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.TestRunListPanel::";

        //Local constants
        protected const int NUMBER_OF_ROWS_PER_PAGE = 15;

        #region Properties

        /// <summary>
        /// Contains the first row to be displayed in the pagination
        /// </summary>
        public int PaginationStartRow
        {
            get
            {
                if (ViewState["PaginationStartRow"] == null)
                {
                    //Default to first page
                    return 1;
                }
                else
                {
                    return (int)ViewState["PaginationStartRow"];
                }
            }
            set
            {
                ViewState["PaginationStartRow"] = value;
            }
        }

        /// <summary>
        /// The base url
        /// </summary>
        protected string GridBaseUrl
        {
            get
            {
                return UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, ProjectId, -2));
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the page first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Populate the user and project id in the grid control
            this.grdTestRunList.ProjectId = this.ProjectId;
            this.grdTestRunList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_RUN;

            //Depending on the artifact that these test runs are for (test case, release or test set)
            //we need to set the grid properties accordingly and also indicate if we have any data
            if (this.ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestCase)
            {
                //Set the filter that it knows to filter by test case
                Dictionary<string, object> testRunFilters = new Dictionary<string, object>();
                testRunFilters.Add("TestCaseId", this.ArtifactId);
                this.grdTestRunList.SetFilters(testRunFilters);

                //Set the display mode
                this.grdTestRunList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Runs;
            }

            if (this.ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestSet)
            {
                //Set the filter that it knows to filter by test set
                Dictionary<string, object> testRunFilters = new Dictionary<string, object>();
                testRunFilters.Add("TestSetId", this.ArtifactId);
                this.grdTestRunList.SetFilters(testRunFilters);

                //Set the display mode
                this.grdTestRunList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Runs;
            }

            if (this.ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Release)
            {
                //Set the filter that it knows to filter by release
                Dictionary<string, object> testRunFilters = new Dictionary<string, object>();
                testRunFilters.Add("ReleaseId", this.ArtifactId);
                this.grdTestRunList.SetFilters(testRunFilters);
            }

            if (this.ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Build)
            {
                //Set the filter that it knows to filter by release
                Dictionary<string, object> testRunFilters = new Dictionary<string, object>();
                testRunFilters.Add("BuildId", this.ArtifactId);
                this.grdTestRunList.SetFilters(testRunFilters);
            }

            if (this.ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.AutomationHost)
            {
                //Set the filter that it knows to filter by release
                Dictionary<string, object> testRunFilters = new Dictionary<string, object>();
                testRunFilters.Add("AutomationHostId", this.ArtifactId);
                this.grdTestRunList.SetFilters(testRunFilters);
            }

            //Populate the list of test run columns to show/hide and databind
            this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.TestRun);
            this.ddlShowHideColumns.DataBind();

            //Specify if we need to auto-load the data (used if tab is initially visible)
            this.grdTestRunList.AutoLoad = this.AutoLoad;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        #endregion

        #region Methods

        public void LoadAndBindData(bool dataBind)
        {
            //Does nothing since we have an AJAX control
        }

        #endregion
    }
}