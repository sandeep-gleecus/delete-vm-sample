using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls
{
    public partial class TestSetListPanel : ArtifactUserControlBase, IArtifactUserControl
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.TestSetListPanel::";

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Populate the user and project id in the grid control
            this.grdTestSetList.ProjectId = this.ProjectId;
            this.grdTestSetList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_SET;
            this.grdTestSetList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, -2);

            //Populate the list of test run columns to show/hide and databind
            this.ddlShowHideColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.TestSet);
            this.ddlShowHideColumns.DataBind();

            LoadAndBindData(true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        #region Methods

        public void LoadAndBindData(bool dataBind)
        {
            //Customize the legend depending on the artifact type
            if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestSet)
            {
                this.locTestSetsLegend.Text = Resources.Main.TestCaseDetails_TestSets;
            }
            if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet)
            {
                this.locTestSetsLegend.Text = Resources.Main.TestConfigurationDetails_TestSets;
            }
        }

        #endregion
    }
}