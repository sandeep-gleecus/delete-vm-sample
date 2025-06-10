using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.UserControls
{
    public partial class IncidentListPanel : ArtifactUserControlBase, IArtifactUserControl
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.IncidentListPanel::";

        /// <summary>
        /// Displays the contents of the incident panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Populate the user and project id in the incident AJAX grid control
            //Need to tell the incident list that it's being filtered on a specific test set
            this.grdIncidentList.ProjectId = this.ProjectId;
            this.grdIncidentList.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2);
            this.grdIncidentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

            //Populate the incident grid show/hide columns list
            this.ddlShowHideIncidentColumns.DataSource = CreateShowHideColumnsList(DataModel.Artifact.ArtifactTypeEnum.Incident);
            this.ddlShowHideIncidentColumns.DataBind();

            //Set the display mode for incidents list
            switch (ArtifactTypeEnum)
            {
                case DataModel.Artifact.ArtifactTypeEnum.Build:
                    {
                        this.grdIncidentList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.Build_Incidents;
                        this.locArtifactTypeLegend.Text = Resources.Main.BuildDetails_Incidents;
                    }
                    break;

                case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                    {
                        this.grdIncidentList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.TestSet_Incidents;
                        this.locArtifactTypeLegend.Text = Resources.Main.TestSetDetails_Incidents;
                    }
                    break;

                case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                    {
                        this.grdIncidentList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.TestCase_Incidents;
                        this.locArtifactTypeLegend.Text = Resources.Main.TestCaseDetails_Incidents;
                    }
                    break;

                case DataModel.Artifact.ArtifactTypeEnum.TestRun:
                    {
                        this.grdIncidentList.DisplayTypeId = (int)DataModel.Artifact.DisplayTypeEnum.TestRun_Incidents;
                        this.locArtifactTypeLegend.Text = Resources.Main.TestRunDetails_Incidents;
                    }
                    break;
            }
        }

        #region Methods

        public void LoadAndBindData(bool dataBind)
        {
            //Does nothing since we have an AJAX control
        }

        #endregion
    }
}