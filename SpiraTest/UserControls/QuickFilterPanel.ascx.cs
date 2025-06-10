using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls
{
    /// <summary>
    /// Displays the list of individual filters, shared filters, releases and components for the project
    /// </summary>
    public partial class QuickFilterPanel : UserControlBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.QuickFilterPanel::";

        #region Properties

        /// <summary>
        /// Should we display the list of releases (or not)
        /// </summary>        
        /// <remarks>The default value = TRUE</remarks>
        public bool DisplayReleases
        {
            get
            {
                if (ViewState["DisplayReleases"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["DisplayReleases"];
                }
            }
            set
            {
                ViewState["DisplayReleases"] = value;
            }
        }

        /// <summary>
        /// The type of artifact we're dealing with
        /// </summary>
        public Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                if (ViewState["ArtifactType"] == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                else
                {
                    return (DataModel.Artifact.ArtifactTypeEnum)ViewState["ArtifactType"];
                }
            }
            set
            {
                ViewState["ArtifactType"] = value;
            }
        }

        /// <summary>
        /// The id of the control that the filters will interact with
        /// </summary>
        public string AjaxServerControlId
        {
            get
            {
                if (ViewState["AjaxServerControlId"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["AjaxServerControlId"];
                }
            }
            set
            {
                ViewState["AjaxServerControlId"] = value;
            }
        }

        /// <summary>
        /// The web service class the Folders list uses
        /// </summary>
        public string WebServiceClass
        {
            get
            {
                if (ViewState["WebServiceClass"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["WebServiceClass"];
                }
            }
            set
            {
                ViewState["WebServiceClass"] = value;
            }
        }

        /// <summary>
        /// The release field to filter by
        /// </summary>
        [System.ComponentModel.DefaultValue("ReleaseId")]
        public string ReleaseFilterField
        {
            get
            {
                if (ViewState["ReleaseFilterField"] == null)
                {
                    return "ReleaseId";
                }
                else
                {
                    return (string)ViewState["ReleaseFilterField"];
                }
            }
            set
            {
                ViewState["ReleaseFilterField"] = value;
            }
        }

        /// <summary>
        /// The client id of the server control
        /// </summary>
        public string AjaxServerControlClientId
        {
            get
            {
                return this.ajaxServerControlClientId;
            }
        }
        private string ajaxServerControlClientId = "";

        #endregion

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Register event handlers
            this.rptMyFilters.ItemDataBound += new RepeaterItemEventHandler(rptMyFilters_ItemDataBound);

            //Populate the client id of the passed-in server control
            Control ajaxControl = Page.FindControlRecursive(this.AjaxServerControlId);
            if (ajaxControl != null)
            {
                this.ajaxServerControlClientId = ajaxControl.ClientID;
            }

            //Add the URL to the release hierarchical drop-down
            this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
            
            //Only load the data once
            if (!IsPostBack)
            {
                //Get the list of saved filters for the current user
                if (this.UserId > 0)
                {
                    SavedFilterManager savedFilterManager = new SavedFilterManager();
                    List<DataModel.SavedFilter> savedFilters = savedFilterManager.Retrieve(UserId, ProjectId, ArtifactType, true);

                    //Get the shared and non-shared filters separately (any that the current user has shared will appear in both lists)
                    this.rptMyFilters.DataSource = savedFilters;

                    //Get the list of components for this project if the artifact type uses components
                    if (ArtifactType == Artifact.ArtifactTypeEnum.Requirement || ArtifactType == Artifact.ArtifactTypeEnum.Task || ArtifactType == Artifact.ArtifactTypeEnum.Incident || ArtifactType == Artifact.ArtifactTypeEnum.TestCase || ArtifactType == Artifact.ArtifactTypeEnum.Risk)
                    {
                        List<Component> components = new ComponentManager().Component_Retrieve(ProjectId);
                        if (components.Count > 0)
                        {
                            this.rptComponents.DataSource = components;
                        }
                        else
                        {
                            this.plcComponents.Visible = false;
                        }
                    }
                    else
                    {
                        this.plcComponents.Visible = false;
                    }

                    //Populate the list of releases and databind. We include inactive ones and let the dropdown list filter by active
                    //that ensures that a legacy filter is displayed even if it is no longer selectable now
                    List<ReleaseView> releases = new ReleaseManager().RetrieveByProjectId(this.ProjectId, false);
                    this.ddlRelease.DataSource = releases;

                    //Databind
                    this.DataBind();

                    //See if we have a release already specified in the appropriate settings for the artifact in question
                    string collectionName = "";
                    if (ArtifactType == Artifact.ArtifactTypeEnum.Requirement)
                    {
                        collectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST;
                    }
                    if (ArtifactType == Artifact.ArtifactTypeEnum.Risk)
                    {
                        collectionName = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS;
                    }
                    if (ArtifactType == Artifact.ArtifactTypeEnum.Task)
                    {
                        collectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
                    }
                    if (ArtifactType == Artifact.ArtifactTypeEnum.TestRun)
                    {
                        collectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
                    }
                    if (!String.IsNullOrEmpty(collectionName))
                    {
                        ProjectSettingsCollection projectSetting = new ProjectSettingsCollection(ProjectId, UserId, collectionName);
                        projectSetting.Restore();
                        if (projectSetting[ReleaseFilterField] != null)
                        {
                            int releaseId = (int)projectSetting[ReleaseFilterField];
                            try
                            {
                                this.ddlRelease.SelectedValue = releaseId.ToString();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                //This occurs if the release has been subsequently deleted. In which case we need to update
                                //the stored settings
                                projectSetting.Remove(ReleaseFilterField);
                                projectSetting.Save();
                            }
                        }
                    }
                }

                //See if we need to display releases
                this.plcReleases.Visible = DisplayReleases;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
        }

       

        void rptMyFilters_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (rptMyFilters.Items.Count < 1)
            {
                if (e.Item.ItemType == ListItemType.Footer)
                {
                    Localize locNoMyFilters = (Localize)e.Item.FindControl("locNoMyFilters");
                    locNoMyFilters.Visible = true;
                }
            }
        }
    }
}