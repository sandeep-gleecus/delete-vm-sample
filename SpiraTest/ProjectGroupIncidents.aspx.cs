using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web
{
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.GroupIncidents, "SiteMap_Incidents", "Program-Incidents")]
    public partial class ProjectGroupIncidents : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ProjectGroupIncidents::";

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
            this.grdIncidentList.ProjectId = this.ProjectGroupId.Value;
            this.grdIncidentList.ArtifactPrefix = GlobalFunctions.ARTIFACT_PREFIX_INCIDENT;

            //Custom CSS for the grid
            Dictionary<string, string> incidentCssClasses = new Dictionary<string, string>();
            incidentCssClasses.Add("IncidentStatusId", "priority2");
            incidentCssClasses.Add("IncidentTypeId", "priority2");
            this.grdIncidentList.SetCustomCssClasses(incidentCssClasses);

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
                IncidentManager incidentManager = new IncidentManager();

                //Populate the list of columns to show/hide and databind
                this.ddlShowHideColumns.DataSource = GetIncidentColumnsList();
                this.ddlShowHideColumns.DataBind();

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
        /// Gets the list of columns that can be shown/hidden for incidents in a project group
        /// </summary>
        /// <returns>The show/hide list</returns>
        /// <remarks>
        /// 1) Unlike the project grids this comes from the user settings collection not from the usual field/custom properties tables
        /// 2) The name, owner and ID fields are always shown and cannot be changed
        /// </remarks>
        protected Dictionary<string, string> GetIncidentColumnsList()
        {
            //Get the list of configurable fields and their visibility status
            IncidentManager incidentManager = new IncidentManager();
            List<ArtifactListFieldDisplay> artifactFields = incidentManager.RetrieveFieldsForProjectGroupLists(ProjectGroupId.Value, UserId, GlobalFunctions.USER_SETTINGS_GROUP_INCIDENT_COLUMNS);

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