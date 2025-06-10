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
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// planning board for the current project group and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.GroupPlanningBoard, "SiteMap_PlanningBoard", "Program-Planning-Board")]
    public partial class ProjectGroupPlanningBoard : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.PlanningBoard::";

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

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

            //Populate the user and project group id in the planning board
            //We have to call it the 'project id', but it's really the group
            this.ajxPlanningBoard.ProjectId = this.ProjectGroupId.Value;

            //Only load the data once
            if (!IsPostBack)
            {
                //Get the stored settings, default to backlog by status
                int selectedProjectId = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD, GlobalFunctions.USER_SETTINGS_KEY_SELECTED_PROJECT, -1);
                bool includeTasks = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TASKS, false);
                bool includeTestCases = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_TEST_CASES, false);
                bool includeDetails = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD, GlobalFunctions.PROJECT_SETTINGS_KEY_INCLUDE_DETAILS, true);
                int groupByOption = GetUserSetting(GlobalFunctions.USER_SETTINGS_PROJECT_GROUP_PLANNING_BOARD, GlobalFunctions.PROJECT_SETTINGS_KEY_GROUP_BY_OPTION, 6);

                //Populate the list of projects and databind
                //Also need to add the 'All Projects' entry
                ProjectManager projectManager = new ProjectManager();
                Hashtable filters = new Hashtable();
                filters.Add("ProjectGroupId", ProjectGroupId.Value);
                filters.Add("ActiveYn", "Y");
                List<ProjectView> projects = projectManager.Retrieve(filters, null);
                ProjectView allProjectsRow = new ProjectView();
                allProjectsRow.Name = Resources.Dialogs.Global_AllProjects;
                allProjectsRow.ProjectId = -2;
                projects.Insert(0, allProjectsRow);
                this.ddlSelectProject.DataSource = projects;

                //Load the list of group by options (depends on release selection)
                List<ServerControls.PlanningBoard.GroupByOption> groupByOptions = this.ajxPlanningBoard.GetProjectGroupGroupByOptions((selectedProjectId == -1), (selectedProjectId == -2));
                this.ddlGroupBy.DataSource = groupByOptions;

                //Databind the controls
                this.DataBind();

                //Set the selected project
                if (selectedProjectId == -1)
                {
                    this.ddlSelectProject.SelectedValue = "";
                }
                else
                {
                    try
                    {
                        this.ddlSelectProject.SelectedValue = selectedProjectId.ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //This occurs if the project is not a member of  this group, just ignore
                    }
                }

                //Set the group-by dropdown and option
                if (groupByOption > 0)
                {
                    //Make sure we have this in the list of active options
                    if (groupByOptions.Any(g => g.Id == groupByOption && g.IsActive))
                    {
                        this.ajxPlanningBoard.GroupBy = (ServerControls.PlanningBoard.PlanningGroupByOptions)groupByOption;
                        this.ddlGroupBy.SelectedValue = groupByOption.ToString();
                    }
                }

                //Set the 'include artifact' checkboxes
                this.chkIncludeTasks.Checked = includeTasks;
                this.chkIncludeTestCases.Checked = includeTestCases;
                this.chkIncludeDetails.Checked = includeDetails;

                //Set the options on the planning board
                this.ajxPlanningBoard.ReleaseId = selectedProjectId;
                this.ajxPlanningBoard.IncludeDetails = includeDetails;
                this.ajxPlanningBoard.IncludeTasks = includeTasks;
                this.ajxPlanningBoard.IncludeTestCases = includeTestCases;

                //Add any client-side event handlers
                Dictionary<string, string> planningBoardHandlers = new Dictionary<string, string>();
                planningBoardHandlers.Add("changeGroupBy", "ajxPlanningBoard_changeGroupBy");
                this.ajxPlanningBoard.SetClientEventHandlers(planningBoardHandlers);

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