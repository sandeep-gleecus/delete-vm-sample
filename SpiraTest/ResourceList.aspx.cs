using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// List of project personnel resources and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Resources, "SiteMap_Resources", "Resource-Tracking")]
    public partial class ResourceList : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ResourceList::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Populate the user and project id in the grid control
            this.grdResourceList.ProjectId = this.ProjectId;
            this.grdResourceList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_USER;
			this.grdResourceList.BaseUrl = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -2, null, false);

            //Custom CSS for the grid
            Dictionary<string, string> resourcesCssClasses = new Dictionary<string, string>();
            resourcesCssClasses.Add("ProjectRoleId", "priority2");
            this.grdResourceList.SetCustomCssClasses(resourcesCssClasses);

            //Set the base url of the release dropdown
            this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2, GlobalFunctions.PARAMETER_TAB_TASK);

            //Only load the data once
            if (!IsPostBack)
            {
                //Get the current resources' release filter
                int passedInReleaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

                //Populate the list of releases and databind
                ReleaseManager releaseManager = new ReleaseManager();
                List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, true);
                this.ddlSelectRelease.DataSource = releases;

                //Need to add a 'fake' release entry to display the 'Project Group' option
                if (UserIsGroupMember && SpiraContext.Current != null)
                {
                    ReleaseView projectGroupReleaseRow = new ReleaseView();
                    projectGroupReleaseRow.FullName = "--- (" + Resources.Main.ResourceList_AllProjectsIn + " " + SpiraContext.Current.ProjectGroupName + ") ---";
                    projectGroupReleaseRow.Name = projectGroupReleaseRow.FullName;
                    projectGroupReleaseRow.ReleaseId = -2;
                    projectGroupReleaseRow.IndentLevel = "";
                    projectGroupReleaseRow.IsSummary = false;
                    projectGroupReleaseRow.ReleaseStatusId = (int)Release.ReleaseStatusEnum.Planned;
                    projectGroupReleaseRow.ReleaseTypeId = (int)Release.ReleaseTypeEnum.MajorRelease;
                    releases.Insert(0, projectGroupReleaseRow);
                }

                //Display the project name as the 'no-value' entry
                this.ddlSelectRelease.NoValueItemText = "--- " + ProjectName + " ---";

                this.ddlSelectRelease.DataBind();
                if (passedInReleaseId == -1)
                {
                    this.ddlSelectRelease.SelectedValue = "";
                }
                else
                {
                    try
                    {
                        this.ddlSelectRelease.SelectedValue = passedInReleaseId.ToString();
                    }
                    catch (Exception)
                    {
                        //This occurs if the release has been subsequently deleted. In which case we need to update
                        //the stored settings
                        SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                    }
                }

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //Reset the error message
            this.lblMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }
    }
}
