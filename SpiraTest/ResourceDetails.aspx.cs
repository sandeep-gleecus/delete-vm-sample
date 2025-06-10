using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Microsoft.Security.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// details of a particular requirement and its test coverage information
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Resources, null, "Resource-Tracking/#resource-details", "ResourceDetails_Title")]
	public partial class ResourceDetails : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ResourceDetails::";
		
		protected string ArtifactTabName = null;

		#region Properties

		protected int userDetailsId
		{
			get;
			set;
		}

		#endregion

		#region Event Handlers

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
			{
				Response.Redirect(UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -1, null), true);
			}
			this.userDetailsId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);

			//Check if the Actions tab should be visible.
			this.tabActions.Visible = (UserIsAdmin || UserIsGroupAdmin || UserIsProjectAdmin ||	this.userDetailsId == this.UserId);

			this.grdUserActivity.SetFilters(new Dictionary<string, object>() { { "UserId", this.userDetailsId } });

			//Attach events.
            try
            {
                this.grdOwnedIncidents.RowDataBound += new GridViewRowEventHandler(grdOwnedIncidents_RowDataBound);
                this.grdOwnedTestCases.RowDataBound += new GridViewRowEventHandler(grdOwnedTestCases_RowDataBound);
                this.grdOwnedTestSets.RowDataBound += new GridViewRowEventHandler(grdOwnedTestSets_RowDataBound);
                this.btnAddContact.Click += new DropMenuEventHandler(btnAddContact_Click);
                this.btnRemoveContact.Click += new DropMenuEventHandler(btnRemoveContact_Click);
                this.ddlSelectRelease.SelectedIndexChanged += new EventHandler(ddlSelectRelease_SelectedIndexChanged);

                Business.UserManager userManager = new Business.UserManager();
                Business.ProjectManager projectManager = new Business.ProjectManager();
                User user = userManager.GetUserById(this.userDetailsId);
                ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(this.ProjectId, userDetailsId);
                if (user == null || projectUser == null)
                {
                    Response.Redirect(UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -1, null), true);
                }

                //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
                int projectRoleIdToCheck = ProjectRoleId;
                if (UserIsAdmin && ProjectRoleId > 0)
                {
                    projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
                }

                //If the user doesn't have permission to view Tasks, make sure we hide the Reqs & Tasks tab
                //by default the control itself only checks for Requirements view permissions
                Project.AuthorizationState canViewTasks = projectManager.IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View);
                if (canViewTasks != Project.AuthorizationState.Authorized)
                {
                    this.tabRQ.CheckPermissions = false;    //Otherwise it will be auto re-enabled
                    this.tabRQ.Visible = false;
                }

                //The user's avatar logo
                this.imgAvatar.UserId = user.UserId;

                //Update the add/remove contact buttons
                UpdateContactButtons();

                this.lblResourceName.Text = Encoder.HtmlEncode(user.FullName);
                this.lblName.Text = Encoder.HtmlEncode(user.FullName);
                this.lblRole.Text = Encoder.HtmlEncode(projectUser.ProjectRoleName);
                this.lblDepartment.Text = ((String.IsNullOrEmpty(user.Profile.Department)) ? "" : Encoder.HtmlEncode(user.Profile.Department));
                this.lblEmail.Text = Encoder.HtmlEncode(user.EmailAddress);
                this.lblEmail.NavigateUrl = "mailto:" + user.EmailAddress;
                Project project = projectManager.RetrieveById(this.ProjectId);
                this.lblHrsDay.Text = project.WorkingHours.ToString();
                this.lblDaysWeek.Text = project.WorkingDays.ToString();
                this.lblNonWorkMonth.Text = project.NonWorkingHours.ToString();

                //Update the page title/description with the resource id and name
                ((MasterPages.Main)this.Master).PageTitle = GlobalFunctions.ARTIFACT_PREFIX_USER + user.UserId + " - " + user.FullName;

                //Set the URL formats on the three GridViews
                ((NameDescriptionFieldEx)this.grdOwnedIncidents.Columns[1]).NavigateUrlFormat = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Incidents, this.ProjectId, -3, null);
                ((NameDescriptionFieldEx)this.grdOwnedTestCases.Columns[1]).NavigateUrlFormat = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.TestCases, this.ProjectId, -3, null);
                ((NameDescriptionFieldEx)this.grdOwnedTestSets.Columns[1]).NavigateUrlFormat = UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.TestSets, this.ProjectId, -3, null);

                if (!IsPostBack)
                {
                    this.LoadAndBindData();
                }

                //Specify the context for the navigation bar
                this.navResourceList.ProjectId = ProjectId;
                this.navResourceList.ListScreenUrl = ReturnToListPageUrl;
                if (this.userDetailsId != -1)
                {
                    this.navResourceList.SelectedItemId = this.userDetailsId;
                }

                this.navResourceList.DisplayMode = (NavigationBar.DisplayModes)GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_DISPLAY_MODE, (int)NavigationBar.DisplayModes.FilteredList);
                this.navResourceList.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
                this.navResourceList.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 250));

                //See if a tab's been specified.
                if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME]))
                {
					string tabRequest = Request.QueryString[GlobalFunctions.PARAMETER_TAB_NAME];

					switch (tabRequest)
					{
                        case GlobalFunctions.PARAMETER_TAB_TESTCASE:
                            tclResourceDetails.SelectedTab = this.pnlTC.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_TASK:
                        case GlobalFunctions.PARAMETER_TAB_REQUIREMENT:
                            tclResourceDetails.SelectedTab = this.pnlRQTK.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_INCIDENT:
                            tclResourceDetails.SelectedTab = this.pnlIN.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

                        case GlobalFunctions.PARAMETER_TAB_TESTSET:
                            tclResourceDetails.SelectedTab = this.pnlTX.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						case GlobalFunctions.PARAMETER_TAB_ACTION:
							tclResourceDetails.SelectedTab = this.pnlActions.ClientID;
							this.ArtifactTabName = tabRequest;
							break;

						default:
							tclResourceDetails.SelectedTab = this.pnlRQTK.ClientID;
							this.ArtifactTabName = GlobalFunctions.PARAMETER_TAB_REQUIREMENT;
							break;
					}
                }

				//Specify the base URL in the navigation control
				this.navResourceList.ItemBaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, ProjectId, -2, ArtifactTabName);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (ArtifactNotExistsException)
            {
                //Redirect back to the resource list page
                Response.Redirect(UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -1, null), true);
            }
		}

        /// <summary>
        /// Updates the current release selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlSelectRelease_SelectedIndexChanged(object sender, EventArgs e)
        {
            //See if we have a release selected
            if (String.IsNullOrEmpty(this.ddlSelectRelease.SelectedValue))
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
            }
            else
            {
                int releaseId;
                if (Int32.TryParse(this.ddlSelectRelease.SelectedValue, out releaseId))
                {
                    SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, releaseId);
                }
            }

            //Now load and bind, need to capture the viewed user id first
            this.userDetailsId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
            this.LoadAndBindData();
        }

        /// <summary>
        /// Updates the state of the add/remove contact buttons
        /// </summary>
        private void UpdateContactButtons()
        {
            //Are they a contact of the current user
            bool isContact = new UserManager().UserContact_IsContact(UserId, this.userDetailsId);
            this.btnAddContact.Visible = !isContact;
            this.btnRemoveContact.Visible = isContact;
        }

        /// <summary>
        /// Removes the user from the current user's contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRemoveContact_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                Response.Redirect(UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -1, null), true);
            }
            this.userDetailsId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
            UserManager userManager = new UserManager();
            userManager.UserContact_Remove(UserId, this.userDetailsId);

            //Reload
            this.LoadAndBindData();

            //Update the add/remove contact buttons
            UpdateContactButtons();
        }

        /// <summary>
        /// Adds the user to the current user's contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAddContact_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]))
            {
                Response.Redirect(UrlRoots.RetrieveURL(UrlRoots.NavigationLinkEnum.Resources, this.ProjectId, -1, null), true);
            }
            this.userDetailsId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_USER_ID]);
            try
            {
                UserManager userManager = new UserManager();
                userManager.UserContact_Add(UserId, this.userDetailsId);
            }
            catch (InvalidOperationException)
            {
                //Happens when a user tries to add themselves as a contact, which the UI now prevents
            }

            //Reload
            this.LoadAndBindData();

            //Update the add/remove contact buttons
            UpdateContactButtons();
        }

		/// <summary>
		/// Redirects back to the list page
		/// </summary>
		protected string ReturnToListPageUrl
		{
			get
			{
				//Redirect to the appropriate page
				return UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, ProjectId);
			}
		}

		/// <summary>
		/// Adds conditional formatting to the test set grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void grdOwnedTestSets_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                if (e.Row.DataItem is TestSetView)
                {
                    TestSetView testSet = (TestSetView)e.Row.DataItem;
                    //Color the date red if it is before today.
                    if (testSet.PlannedDate.HasValue && testSet.PlannedDate.Value < DateTime.UtcNow)
                    {
                        e.Row.Cells[4].ForeColor = Color.FromName("Red");
                    }
                    LabelEx lblPlannedDate = (LabelEx)e.Row.Cells[4].FindControl("lblPlannedDate");
                    if (testSet.PlannedDate.HasValue)
                    {
                        lblPlannedDate.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(testSet.PlannedDate.Value));
                        lblPlannedDate.ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testSet.PlannedDate.Value));
                    }
                    else
                    {
                        lblPlannedDate.Text = "-";
                    }

                    //Configure the equalizer bars for execution status
                    int passedCount = testSet.CountPassed;
                    int failureCount = testSet.CountFailed;
                    int cautionCount = testSet.CountCaution;
                    int blockedCount = testSet.CountBlocked;
                    int notRunCount = testSet.CountNotRun;
                    int notApplicableCount = testSet.CountNotApplicable;

                    //Calculate the percentages, handling rounding correctly
                    //We don't include N/A ones in the total as they are either inactive or folders
                    int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount;
                    int percentPassed = 0;
                    int percentFailed = 0;
                    int percentCaution = 0;
                    int percentBlocked = 0;
                    int percentNotRun = 0;
                    int percentNotApplicable = 0;
                    if (totalCount != 0)
                    {
                        //Need check to handle divide by zero case
                        percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                        percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                        percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                        percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                        percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                        percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
                    }

                    //Add a persistent tooltip to the equalizer
                    string tooltipText = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();

                    //Now populate the equalizer graph
                    Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[5].FindControl("eqlExecutionStatus");
                    if (eqlExecutionStatus != null)
                    {
                        eqlExecutionStatus.PercentGreen = percentPassed;
                        eqlExecutionStatus.PercentRed = percentFailed;
                        eqlExecutionStatus.PercentOrange = percentCaution;
                        eqlExecutionStatus.PercentYellow = percentBlocked;
                        eqlExecutionStatus.PercentGray = percentNotRun;
                        eqlExecutionStatus.ToolTip = tooltipText;
                    }
                }
                if (e.Row.DataItem is TestSetReleaseView)
                {
                    TestSetReleaseView testSet = (TestSetReleaseView)e.Row.DataItem;
                    //Color the date red if it is before today.
                    if (testSet.PlannedDate.HasValue && testSet.PlannedDate.Value < DateTime.UtcNow)
                    {
                        e.Row.Cells[4].ForeColor = Color.FromName("Red");
                    }
                    LabelEx lblPlannedDate = (LabelEx)e.Row.Cells[4].FindControl("lblPlannedDate");
                    if (testSet.PlannedDate.HasValue)
                    {
                        lblPlannedDate.Text = String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(testSet.PlannedDate.Value));
                        lblPlannedDate.ToolTip = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(testSet.PlannedDate.Value));
                    }
                    else
                    {
                        lblPlannedDate.Text = "-";
                    }

                    //Configure the equalizer bars for execution status
                    int passedCount = testSet.CountPassed;
                    int failureCount = testSet.CountFailed;
                    int cautionCount = testSet.CountCaution;
                    int blockedCount = testSet.CountBlocked;
                    int notRunCount = testSet.CountNotRun;
                    int notApplicableCount = testSet.CountNotApplicable;

                    //Calculate the percentages, handling rounding correctly
                    //We don't include N/A ones in the total as they are either inactive or folders
                    int totalCount = passedCount + failureCount + cautionCount + blockedCount + notRunCount;
                    int percentPassed = 0;
                    int percentFailed = 0;
                    int percentCaution = 0;
                    int percentBlocked = 0;
                    int percentNotRun = 0;
                    int percentNotApplicable = 0;
                    if (totalCount != 0)
                    {
                        //Need check to handle divide by zero case
                        percentPassed = (int)Decimal.Round(((decimal)passedCount * (decimal)100) / (decimal)totalCount, 0);
                        percentFailed = (int)Decimal.Round(((decimal)failureCount * (decimal)100) / (decimal)totalCount, 0);
                        percentCaution = (int)Decimal.Round(((decimal)cautionCount * (decimal)100) / (decimal)totalCount, 0);
                        percentBlocked = (int)Decimal.Round(((decimal)blockedCount * (decimal)100) / (decimal)totalCount, 0);
                        percentNotRun = (int)Decimal.Round(((decimal)notRunCount * (decimal)100) / (decimal)totalCount, 0);
                        percentNotApplicable = (int)Decimal.Round(((decimal)notApplicableCount * (decimal)100) / (decimal)totalCount, 0);
                    }

                    //Add a persistent tooltip to the equalizer
                    string tooltipText = "# " + Resources.Fields.Passed + "=" + passedCount.ToString() + ", # " + Resources.Fields.Failed + "=" + failureCount.ToString() + ", # " + Resources.Fields.Caution + "=" + cautionCount.ToString() + ", # " + Resources.Fields.Blocked + "=" + blockedCount.ToString() + ", # " + Resources.Fields.NotRun + "=" + notRunCount.ToString();

                    //Now populate the equalizer graph
                    Equalizer eqlExecutionStatus = (Equalizer)e.Row.Cells[5].FindControl("eqlExecutionStatus");
                    if (eqlExecutionStatus != null)
                    {
                        eqlExecutionStatus.PercentGreen = percentPassed;
                        eqlExecutionStatus.PercentRed = percentFailed;
                        eqlExecutionStatus.PercentOrange = percentCaution;
                        eqlExecutionStatus.PercentYellow = percentBlocked;
                        eqlExecutionStatus.PercentGray = percentNotRun;
                        eqlExecutionStatus.ToolTip = tooltipText;
                    }
                }
			}
		}

		/// <summary>
		/// Adds conditional formatting to the test cases grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void grdOwnedTestCases_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                if (e.Row.DataItem is TestCaseView)
                {
                    TestCaseView testCase = (TestCaseView)e.Row.DataItem;
                    //Set the execution color.
                    e.Row.Cells[4].CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCase.ExecutionStatusId);
                    //Set the priority color
                    if (testCase.TestCasePriorityId.HasValue)
                    {
                        e.Row.Cells[5].BackColor = Color.FromName("#" + testCase.TestCasePriorityColor);
                    }
                }
                if (e.Row.DataItem is TestCaseReleaseView)
                {
                    TestCaseReleaseView testCase = (TestCaseReleaseView)e.Row.DataItem;
                    //Set the execution color.
                    e.Row.Cells[4].CssClass = GlobalFunctions.GetExecutionStatusCssClass(testCase.ExecutionStatusId);
                    //Set the priority color
                    if (testCase.TestCasePriorityId.HasValue)
                    {
                        e.Row.Cells[5].BackColor = Color.FromName("#" + testCase.TestCasePriorityColor);
                    }
                }

			}
		}

		/// <summary>
		/// Adds formatting to the incidents grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void grdOwnedIncidents_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			//Set the url for the name column.
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
                IncidentView incidentView = ((IncidentView)e.Row.DataItem);
				//Set the priority and Status colors.
                e.Row.Cells[5].BackColor = ((!incidentView.PriorityId.HasValue) ? Color.Empty : Color.FromName("#" + incidentView.PriorityColor));
                e.Row.Cells[6].BackColor = ((!incidentView.SeverityId.HasValue) ? Color.Empty : Color.FromName("#" + incidentView.SeverityColor));

				//Set the equalizer progress
				Equalizer eqlProgress = (Equalizer)e.Row.Cells[7].FindControl("eqlProgress");
				if (eqlProgress != null)
				{
                    Incident incident = incidentView.ConvertTo<IncidentView, Incident>();
					int percentGreen;
					int percentRed;
					int percentYellow;
					int percentGray;
                    string tooltip = IncidentManager.CalculateProgress(incident, out percentGreen, out percentRed, out percentYellow, out percentGray);
					eqlProgress.PercentGreen = percentGreen;
					eqlProgress.PercentRed = percentRed;
					eqlProgress.PercentYellow = percentYellow;
					eqlProgress.PercentGray = percentGray;
					eqlProgress.ToolTip = tooltip;
				}
			}
		}

		private void LoadAndBindData()
		{
			//Activity 
			this.grdUserActivity.ProjectId = this.ProjectId;

			//Requirements/Tasks panel (AJAX)
			this.grdReqTaskList.ProjectId = this.ProjectId;

            //The user online status and message options. Hide the message options if the user is viewing themself!
            this.ajxUserStatus.UserId = this.userDetailsId;
            this.ajxUserStatus.Visible = ConfigurationSettings.Default.Message_Enabled;
            this.plcMessageOptions.Visible = (ConfigurationSettings.Default.Message_Enabled && this.UserId != this.userDetailsId);

            //Set the client-side url for the message sending button
            this.btnSendMessage.ClientScriptMethod = "tstucMessageManager.send_new_message(" + this.userDetailsId + ",event)";

            //The release selector
            this.ddlSelectRelease.BaseUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, -2);

            //Get the current test case release filter
            int passedInReleaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_RESOURCES_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);

            //Populate the list of releases and databind
            ReleaseManager releaseManager = new ReleaseManager();
            List<ReleaseView> releases = releaseManager.RetrieveByProjectId(this.ProjectId, true);
            this.ddlSelectRelease.DataSource = releases;
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

            //Set the filter that it only displays reqs and tasks owned by this user
			Dictionary<string, object> reqFilters = new Dictionary<string, object>();
			reqFilters.Add("OwnerId", this.userDetailsId);
            //Add the release filter if appropriate
            if (passedInReleaseId > 0)
            {
                reqFilters.Add("ReleaseId", passedInReleaseId);
            }
			this.grdReqTaskList.SetFilters(reqFilters);

			//Get the count of requirements for the 'has-data' flag
			Hashtable reqFilters2 = new Hashtable();
			reqFilters2.Add("OwnerId", this.userDetailsId);
			RequirementManager requirement = new RequirementManager();
            int reqCount = requirement.CountNonSummary(this.ProjectId, reqFilters2, GlobalFunctions.GetCurrentTimezoneUtcOffset());
			this.tabRQ.HasData = (reqCount > 0);

			//Incident panel.
            List<IncidentView> incidents;
            if (passedInReleaseId > 0)
            {
                //All tasks (open or closed) for this user in the specified release/iteration
                Hashtable incidentFilters = new Hashtable();
                incidentFilters.Add("OwnerId", this.userDetailsId);
                incidentFilters.Add("ResolvedReleaseId", passedInReleaseId);
                incidents = new IncidentManager().Retrieve(ProjectId, "PriorityName", true, 1, Int32.MaxValue, incidentFilters, 0);
            }
            else
            {
                //All open incidents for this user in the current project
                incidents = new IncidentManager().RetrieveOpenByOwnerId(this.userDetailsId, this.ProjectId, null);
            }
            this.grdOwnedIncidents.DataSource = incidents;
            this.tabIN.HasData = (incidents.Count > 0);

			//Testcase panel.
            if (passedInReleaseId > 0)
            {
                Hashtable testCaseFilters = new Hashtable();
                testCaseFilters.Add("OwnerId", this.userDetailsId);
                List<TestCaseReleaseView> testCases = new TestCaseManager().RetrieveByReleaseId(ProjectId, passedInReleaseId, "Name", true, 1, Int32.MaxValue, testCaseFilters, 0);
                this.grdOwnedTestCases.DataSource = testCases;
                this.tabTC.HasData = (testCases.Count > 0);
            }
            else
            {
                List<TestCaseView> testCases = new TestCaseManager().RetrieveByOwnerId(this.userDetailsId, this.ProjectId);
                this.grdOwnedTestCases.DataSource = testCases;
                this.tabTC.HasData = (testCases.Count > 0);
            }

			//Testset panel.
			//Can't use RetrieveByOwner because we need the option to filter by release
			Hashtable filters = new Hashtable();
			filters.Add("OwnerId", this.userDetailsId);
            if (passedInReleaseId > 0)
            {
                filters.Add("ReleaseId", passedInReleaseId);
            }
            List<TestSetView> testSets = new TestSetManager().Retrieve(this.ProjectId, "Name", true, 1, Int32.MaxValue, filters, GlobalFunctions.GetCurrentTimezoneUtcOffset(), TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
            this.grdOwnedTestSets.DataSource = testSets;
            this.tabTX.HasData = (testSets.Count > 0);

            //Actions panel
            Hashtable activityFilters = new Hashtable();
            activityFilters.Add("UserId", this.userDetailsId);
            int activityCount = new HistoryManager().CountSet(ProjectId, activityFilters, GlobalFunctions.GetCurrentTimezoneUtcOffset());
            this.tabActions.HasData = (activityCount > 0);

            //Databind
            this.DataBind();
		}

        /// <summary>
        /// Gets the time from a raw number of minutes
        /// </summary>
        /// <param name="numMinutes"></param>
        /// <returns></returns>
		protected string GetTimeFromMinutes(object numMinutes)
		{
            if (numMinutes == null)
            {
                return "";
            }
            if (numMinutes.GetType() == typeof(int?))
            {
                int? mins = (int?)numMinutes;
                if (mins.HasValue)
                {
                    return GlobalFunctions.GetEffortInFractionalHours(mins.Value);
                }
                else
                {
                    return "";
                }
            }
            else if (numMinutes.GetType() == typeof(int))
            {
                int mins = (int)numMinutes;
                return GlobalFunctions.GetEffortInFractionalHours(mins);
            }
            else
            {
                return "";
            }
		}
		#endregion
	}
}
