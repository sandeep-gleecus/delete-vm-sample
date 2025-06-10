using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Data;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the project requirements as a mind-map
    /// </summary>
    /// <remarks>
    /// See https://dagrejs.github.io/project/dagre-d3/latest/demo/interactive-demo.html for the sample it was based on
    /// </remarks>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Requirements, "SiteMap_RequirementsMap", "Requirements-Management/#requirement-mindmap")]
    public partial class RequirementsMap : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RequirementsMap::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //We need to make sure that the system is licensed for planning boards (SpiraTeam or SpiraPlan only)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.RequirementsFeature_InvalidLicense, true);
            }

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized && !(UserIsAdmin && ProjectRoleId > 0))
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Update the setting that we're using the map view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Map")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Map");
            }

			//Specify the context for the ajax form control
			this.ajxFormManager.ProjectId = this.ProjectId;
			Dictionary<string, string> formManagerHandlers = new Dictionary<string, string>();
			formManagerHandlers.Add("dataSaved", "ajxFormManager_dataSaved");
			formManagerHandlers.Add("loaded", "ajxFormManager_loaded");
			this.ajxFormManager.SetClientEventHandlers(formManagerHandlers);

			//Only load the data once
			if (!IsPostBack)
            {
                //Populate the list of indent levels for the show indent level dropdown list
                this.ddlShowLevel.DataSource = CreateShowLevelList();

				//Populate the standard fields of the requirement creation dialog
				this.ddlRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);
				ComponentManager componentManager = new ComponentManager();
				RequirementManager requirementManager = new RequirementManager();
				ReleaseManager releaseManager = new ReleaseManager();
				List<Importance> importances = requirementManager.RequirementImportance_Retrieve(ProjectTemplateId);
				List<RequirementType> types = requirementManager.RequirementType_Retrieve(ProjectTemplateId, true);
				List<RequirementStatus> statuses = requirementManager.RetrieveStatusesInUse(ProjectTemplateId);
				List<DataModel.User> allUsers = new UserManager().RetrieveForProject(this.ProjectId);
				List<DataModel.User> activeUsers = new UserManager().RetrieveActiveByProjectId(this.ProjectId);
				List<ReleaseView> releases2 = releaseManager.RetrieveByProjectId(this.ProjectId, false, true);
				List<Component> components = componentManager.Component_Retrieve(this.ProjectId, true);

				//Force the package type to be inactive
				IEnumerable<RequirementType> virtualTypes = types.Where(r => r.RequirementTypeId < 0);
				foreach (RequirementType virtualType in virtualTypes)
				{
					virtualType.IsActive = false;
				}

				//Set the data sources
				this.ddlImportance.DataSource = importances;
				this.ddlAuthor.DataSource = allUsers;
				this.ddlOwner.DataSource = activeUsers;
				this.ddlRelease.DataSource = releases2;
				this.ddlType.DataSource = types;
				this.ddlStatus.DataSource = statuses;
				this.ddlComponent.DataSource = components;

				//Add the various custom properties to the table of fields
				UnityCustomPropertyInjector.CreateControls(
					ProjectId,
					ProjectTemplateId,
					DataModel.Artifact.ArtifactTypeEnum.Requirement,
					this.customFieldsDefault,
					this.ajxFormManager,
					this.customFieldsUsers,
					this.customFieldsDates,
					this.customFieldsRichText
				);

				//Databind the controls
				this.DataBind();

                //Set the initial value of the controls
                int openLevel = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_OPEN_LEVEL, 0);
                try
                {
                    this.ddlShowLevel.SelectedValue = openLevel.ToString();
                }
                catch (ArgumentOutOfRangeException)
                {
                    //leave unset
                }

                bool includeAssociations = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MIND_MAP_INCLUDE_ASSOCIATIONS, true);
                this.chkIncludeAssociations.Checked = includeAssociations;

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
