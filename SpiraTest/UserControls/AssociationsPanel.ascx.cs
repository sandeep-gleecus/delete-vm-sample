namespace Inflectra.SpiraTest.Web.UserControls
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Inflectra.SpiraTest.Business;
    using Inflectra.SpiraTest.Common;
    using Inflectra.SpiraTest.Web.ServerControls;
    using Inflectra.SpiraTest.DataModel;
    using Inflectra.SpiraTest.Web.Classes;
    using System.ComponentModel;

    /// <summary>
    ///		This user control displays the generic associations panel used by some of the artifact
    ///		details pages. It is typically enclosed in a panel
    /// </summary>

    public partial class AssociationsPanel : ArtifactUserControlBase, IArtifactUserControl
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.AssociationsPanel::";

        #region Properties

        /// <summary>
        /// Returns if the grid is editable (based on the type of association, not permissions)
        /// </summary>
        [DefaultValue(true)]
        public bool IsGridEditable
        {
            get
            {
                if (ViewState["IsGridEditable"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["IsGridEditable"];
                }
            }
            set
            {
                ViewState["IsGridEditable"] = value;
            }
        }

        /// <summary>
        /// Should we display the 'Add' button
        /// </summary>
        [DefaultValue(true)]
        public bool ShowAddButton
        {
            get
            {
                if (ViewState["ShowAddButton"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["ShowAddButton"];
                }
            }
            set
            {
                ViewState["ShowAddButton"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Called when the control first loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Only load the data once
            if (!IsPostBack)
            {
                //Show right legend when appropriate
                this.IsGridEditable = true;
                if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Requirement && DisplayTypeEnum == Artifact.DisplayTypeEnum.Requirement_TestCases)
                {
                    this.IsGridEditable = false;
                }
                if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.Release && DisplayTypeEnum == Artifact.DisplayTypeEnum.Release_TestCases)
                {
                    this.IsGridEditable = false;
                }
                if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestStep && DisplayTypeEnum == Artifact.DisplayTypeEnum.TestStep_Requirements)
                {
                    this.IsGridEditable = false;
                }
                if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestCase && DisplayTypeEnum == Artifact.DisplayTypeEnum.TestCase_Releases)
                {
                    this.IsGridEditable = false;
                }
                if (ArtifactTypeEnum == DataModel.Artifact.ArtifactTypeEnum.TestCase && DisplayTypeEnum == Artifact.DisplayTypeEnum.TestCase_Requirements)
                {
                    this.IsGridEditable = false;
                }
                if (DisplayTypeEnum == Artifact.DisplayTypeEnum.Attachments || DisplayTypeEnum == Artifact.DisplayTypeEnum.Build_Associations)
                {
                    //Attachment and build associations have no editable fields
                    this.IsGridEditable = false;
                }
            }

            //Set the base url and prefix for the panel
            if (DisplayTypeEnum == Artifact.DisplayTypeEnum.Requirement_TestCases || DisplayTypeEnum == Artifact.DisplayTypeEnum.Release_TestCases)
            {
                this.grdAssociationLinks.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, ProjectId, -2);
                this.grdAssociationLinks.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE;
            }
            if (DisplayTypeEnum == Artifact.DisplayTypeEnum.TestCase_Requirements || DisplayTypeEnum == Artifact.DisplayTypeEnum.TestStep_Requirements)
            {
                this.grdAssociationLinks.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2);
                this.grdAssociationLinks.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT;
            }
            if (DisplayTypeEnum == Artifact.DisplayTypeEnum.TestCase_Releases)
            {
                this.grdAssociationLinks.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
                this.grdAssociationLinks.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_RELEASE;
            }
            if (DisplayTypeEnum == Artifact.DisplayTypeEnum.TestCase_TestSets)
            {
                this.grdAssociationLinks.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, ProjectId, -2);
                this.grdAssociationLinks.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_TEST_SET;
            }

            //Populate the user and project id in the grid control
            this.grdAssociationLinks.ProjectId = this.ProjectId;
            this.grdAssociationLinks.Authorized_ArtifactType = this.ArtifactTypeEnum;
            
            //send the displayType to the page so that JS can access it for rendering
            this.addPanelId.Attributes["data-display"] = ((int)this.DisplayTypeEnum).ToString();

            //Specify if we need to auto-load the data (used if tab is initially visible)
            this.grdAssociationLinks.AutoLoad = this.AutoLoad;

            //Specify the artifact as two standard filters
            Dictionary<string, object> standardFilters = new Dictionary<string, object>();
            standardFilters.Add("ArtifactId", this.ArtifactId);
            standardFilters.Add("ArtifactType", (int)this.ArtifactTypeEnum);
            this.grdAssociationLinks.SetFilters(standardFilters);

            //then specify the association panel type
            grdAssociationLinks.DisplayTypeId = (int)this.DisplayTypeEnum;

            //Set the authorized artifact on the various buttons and the grid context menu
            this.lnkAdd.Authorized_ArtifactType = this.ArtifactTypeEnum;
            this.lnkDelete.Authorized_ArtifactType = this.ArtifactTypeEnum;
            foreach (ContextMenuItem contextMenuItem in this.grdAssociationLinks.ContextMenuItems)
            {
                contextMenuItem.Authorized_ArtifactType = this.ArtifactTypeEnum;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Gets a list of artifact type ids that the user can see in comma separated string format
        /// </summary>
        /// <returns>The array of values</returns>
        protected string GetListOfViewableArtifactTypeIds()
        {
            List<int> artifactTypeIds = GlobalFunctions.GetArtifactTypesForPermission(ProjectRoleId, Project.PermissionEnum.View);
            string retVal = "";
            if (artifactTypeIds.Count > 0)
            {
                retVal += artifactTypeIds.ToDatabaseSerialization();
            }
            return retVal;
        }

        /// <summary>
        /// Loads and databinds anything
        /// </summary>
        /// <param name="dataBind"></param>
        public void LoadAndBindData(bool dataBind)
        {
            //Specify the artifact as two standard filters
            Dictionary<string, object> standardFilters = new Dictionary<string, object>();
            standardFilters.Add("ArtifactId", this.ArtifactId);
            standardFilters.Add("ArtifactType", (int)this.ArtifactTypeEnum);
            this.grdAssociationLinks.SetFilters(standardFilters);

            //Hide the add/remove buttons if the page doesn't want you to have access to them
            if (!ShowAddButton)
            {
                this.lnkAdd.Visible = false;
                this.lnkDelete.Visible = false;
            }
        }
    }
}