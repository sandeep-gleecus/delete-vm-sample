using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using System.Collections;

namespace Inflectra.SpiraTest.Web
{
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.GroupReleases, "SiteMap_Releases", "Program-Releases")]
    public partial class ProjectGroupReleases : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectGroupReleases::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Populate the user and project group id in the grid control
            this.grdReleaseList.ProjectId = this.ProjectGroupId.Value;
            this.grdReleaseList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_RELEASE;

            //We need to make sure that the system is licensed for SpiraPlan (SpiraTeam/Test cannot access this page)
            if (License.LicenseProductName != LicenseProductNameEnum.SpiraPlan)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.ProjectGroup_InvalidLicense, true);
            }

            //Make sure we're authorized for this group
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            if (!this.ProjectGroupId.HasValue || !projectGroupManager.IsAuthorized(UserId, this.ProjectGroupId.Value))
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + "You are not authorized to view this Project Group!", true);
            }

            //Only load the data once
            if (!IsPostBack)
            {
                //Instantiate the business classes
                Business.ReleaseManager release = new Business.ReleaseManager();

                //Populate the list of indent levels for the show indent level dropdown list
                this.ddlShowLevel.DataSource = CreateShowLevelList();
                this.ddlShowLevel.DataBind();

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = GetReleaseColumnsList();
                this.ddlShowHideColumns.DataBind();

                //Custom CSS for the grid
                Dictionary<string, string> releasesCssClasses = new Dictionary<string, string>();
                releasesCssClasses.Add("VersionNumber", "priority2");
                this.grdReleaseList.SetCustomCssClasses(releasesCssClasses);

                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //Reset the error message
            this.divMessage.Text = "";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Gets the list of columns that can be shown/hidden for releases in a project group
        /// </summary>
        /// <returns>The show/hide list</returns>
        /// <remarks>
        /// 1) Unlike the project grids this comes from the user settings collection not from the usual field/custom properties tables
        /// </remarks>
        protected Dictionary<string, string> GetReleaseColumnsList()
        {
            //Get the list of configurable fields and their visibility status
            ReleaseManager releaseManager = new ReleaseManager();
            List<ArtifactListFieldDisplay> artifactFields = releaseManager.RetrieveFieldsForProjectGroupLists(ProjectGroupId.Value, UserId, GlobalFunctions.USER_SETTINGS_GROUP_RELEASE_COLUMNS);

            //Resort based on visible status
            IEnumerable artifactFields2 = artifactFields.OrderBy(af => af.IsVisible).ThenBy(af => af.Name);

            //Convert to dropdown list dictionary
            Dictionary<string, string> showHideList = new Dictionary<string, string>();
            foreach (ArtifactListFieldDisplay artifactField in artifactFields2)
            {
                //See if we can localize the field name or not
                string localizedName = Resources.Fields.ResourceManager.GetString(artifactField.Name);
                if (!String.IsNullOrEmpty(localizedName))
                {
                    artifactField.Caption = localizedName;
                }
                string legend = (artifactField.IsVisible) ? Resources.Dialogs.Global_Hide + " " + artifactField.Caption : Resources.Dialogs.Global_Show + " " + artifactField.Caption;
                showHideList.Add(artifactField.Name, legend);
            }

            return showHideList;
        }
    }
}