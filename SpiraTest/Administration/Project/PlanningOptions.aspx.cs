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
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Planning Options Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_PlanningOptions_Title", "Product-Planning/#planning-options", "Admin_PlanningOptions_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class PlanningOptions : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.PlanningOptions::";

		#region Properties

		/// <summary>
		/// Returns the suggested # minutes for a story point
		/// </summary>
		protected string SuggestedPointEffortMetric
		{
			get
			{
				if (this.suggestedPointEffortMetric.HasValue)
				{
					//Convert into hours
					return GlobalFunctions.GetEffortInFractionalHours(this.suggestedPointEffortMetric.Value);
				}
				else
				{
					return "";
				}
			}
		}
		private int? suggestedPointEffortMetric;

		#endregion

		/// <summary>
		/// Loads the page data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Redirect if there's no project selected.
			if (ProjectId < 1)
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);


			//Reset the error message
			this.lblMessage.Text = "";

			//Register event handlers
			this.btnCancel.Click += new EventHandler(btnCancel_Click);
			this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

			if (!IsPostBack)
			{
				//Set the project name and link back to the data sync home
				this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
				this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

				LoadAndBindData();
			}

			//Set the URL for the data tools page
			this.lnkDataCaching1.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "DataTools");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Redirects back to the administration home page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnCancel_Click(object sender, EventArgs e)
		{
			Response.Redirect("Default.aspx");
		}

		/// <summary>
		/// Loads the planning options data
		/// </summary>
		/// <summary>
		/// Loads the planning options panel
		/// </summary>
		protected void LoadAndBindData()
		{
			//Set the datasource on the list of WIP limits - Only statuses that are Planned or after get included, and exclude Obsolete
			List<RequirementStatus> requirementStati = new RequirementManager().RetrieveStatusesInUse(this.ProjectTemplateId, false, true, false);
			this.rptStatusWip.DataSource = requirementStati;

			//Databind the page overall = ensures that the validators work correctly
			this.DataBind();

			//Now load the values from the current project record
			ProjectManager projectManager = new ProjectManager();
			DataModel.Project project = null;
			try
			{
				project = projectManager.RetrieveById(ProjectId);
			}
			catch (ArtifactNotExistsException)
			{
				//Project no longer exists so default to admin home page
				Response.Redirect("Default.aspx");
			}
			this.txtWorkingHoursPerDay.Text = project.WorkingHours.ToString();
			this.txtWorkingDaysPerWeek.Text = project.WorkingDays.ToString();
			this.txtNonWorkingHoursPerMonth.Text = project.NonWorkingHours.ToString();
			this.chkEffortIncidents.Checked = project.IsEffortIncidents;
			this.chkEffortTasks.Checked = project.IsEffortTasks;
			this.chkEffortTestCases.Checked = project.IsEffortTestCases;
			this.chkTimeTrackingIncidents.Checked = project.IsTimeTrackIncidents;
			this.chkTimeTrackingTasks.Checked = project.IsTimeTrackTasks;
			this.chkAutoCreateTasks.Checked = project.IsTasksAutoCreate;
			this.txtPointEffort.Text = GlobalFunctions.GetEffortInFractionalHours(project.ReqPointEffort);
			this.chkReqStatusChangedByTasks.Checked = project.IsReqStatusByTasks;
			this.chkReqStatusChangedByTestCase.Checked = project.IsReqStatusByTestCases;
			this.chkReqStatusAutoPlanned.Checked = project.IsReqStatusAutoPlanned;
			if (project.ReqDefaultEstimate.HasValue)
			{
				this.txtReqDefaultEstimate.Text = String.Format(GlobalFunctions.FORMAT_POINTS, project.ReqDefaultEstimate.Value);
			}
			else
			{
				this.txtReqDefaultEstimate.Text = "";
			}
			if (project.TaskDefaultEffort.HasValue)
			{
				this.txtTaskDefaultEffort.Text = GlobalFunctions.GetEffortInFractionalHours(project.TaskDefaultEffort.Value);
			}
			else
			{
				this.txtTaskDefaultEffort.Text = "";
			}

			//If we have SpiraTest, disable the task and planning board related stuff
			if (Common.License.LicenseProductName == Common.LicenseProductNameEnum.SpiraTest)
			{
				this.chkEffortTasks.Checked = false;
				this.chkEffortTasks.Enabled = false;
				this.chkReqStatusChangedByTasks.Checked = false;
				this.chkReqStatusChangedByTasks.Enabled = false;
				this.chkAutoCreateTasks.Checked = false;
				this.chkAutoCreateTasks.Enabled = false;
				this.plcTasksIncidents.Visible = false;
				this.plcKanbanWip.Visible = false;
			}

			//Project Settings
			ProjectSettings projectSettings = new ProjectSettings(ProjectId);
			this.chkDetectedReleaseActiveOnly.Checked = projectSettings.DisplayOnlyActiveReleasesForDetected;
			this.chkPlanUsingPoints.Checked = !projectSettings.DisplayHoursOnPlanningBoard;
			this.txtReleaseWipMultiplier.Text = String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, projectSettings.KanbanWip_ReleaseWipMultiplier);
			this.txtIterationWipMultiplier.Text = String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, projectSettings.KanbanWip_IterationWipMultiplier);

			//hide and disable the points effort field if planning by points
			if (!projectSettings.DisplayHoursOnPlanningBoard)
			{
				this.plcPointEffort.Visible = false;
			}
			else
			{
				//Get the suggested new story point effort value for this project so that the popup dialog box has it ready
				this.suggestedPointEffortMetric = new RequirementManager().SuggestNewPointEffortMetric(ProjectId);
				txtSuggestedPointEffort.Text = SuggestedPointEffortMetric;
			}

			//Need to populate the WIP percentages from the serialized value
			//Format = status,release%,iteration%|status,release%,iteration%
			string statusPercentages = projectSettings.KanbanWip_StatusPercentages;
			if (!String.IsNullOrWhiteSpace(statusPercentages))
			{
				string[] statuses = statusPercentages.Split('|');

				//Loop through each status
				foreach (string statusEntry in statuses)
				{
					int requirementStatusId = -1;
					decimal releasePercentage = 0M;
					decimal iterationPercentage = 0M;
					string[] parts = statusEntry.Split(',');
					if (parts.Length == 3)
					{
						if (Int32.TryParse(parts[0], out requirementStatusId))
						{
							foreach (RepeaterItem repeaterRow in this.rptStatusWip.Items)
							{
								TextBoxEx txtReleaseWipPercent = (TextBoxEx)repeaterRow.FindControl("txtReleaseWipPercent");
								TextBoxEx txtIterationWipPercent = (TextBoxEx)repeaterRow.FindControl("txtIterationWipPercent");
								if (txtReleaseWipPercent != null && txtIterationWipPercent != null && txtReleaseWipPercent.MetaData == requirementStatusId.ToString())
								{
									if (Decimal.TryParse(parts[1], out releasePercentage))
									{
										txtReleaseWipPercent.Text = String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, releasePercentage);
									}
									if (Decimal.TryParse(parts[2], out iterationPercentage))
									{
										txtIterationWipPercent.Text = String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, iterationPercentage);
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Updates the planning options with the values specified by the user
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure all validators have succeeded
			if (!Page.IsValid)
			{
				return;
			}

			//Make sure that certain other rules are observed
			int workingHours = Int32.Parse(this.txtWorkingHoursPerDay.Text);
			int workingDays = Int32.Parse(this.txtWorkingDaysPerWeek.Text);
			int nonWorkingHours = Int32.Parse(this.txtNonWorkingHoursPerMonth.Text);

			if (workingHours < 1 || workingHours > 24)
			{
				this.lblMessage.Text = Resources.Messages.Admin_PlanningOptions_WorkingHoursPerDayNotInRange;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}
			if (workingDays < 1 || workingDays > 7)
			{
				this.lblMessage.Text = Resources.Messages.Admin_PlanningOptions_WorkingDaysPerWeekNotInRange;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}
			if (nonWorkingHours > (workingHours * workingDays * 4))
			{
				this.lblMessage.Text = Resources.Messages.Admin_PlanningOptions_NonWorkingHoursExceedWorkingHours;
				this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}

			//Now load the values from the current project record
			ProjectManager projectManager = new ProjectManager();
			DataModel.Project project = null;
			try
			{
				project = projectManager.RetrieveById(ProjectId);
			}
			catch (ArtifactNotExistsException)
			{
				//Project no longer exists so default to admin home page
				Response.Redirect("Default.aspx");
			}

			//Update the project record with the new values
			project.StartTracking();
			project.WorkingHours = workingHours;
			project.WorkingDays = workingDays;
			project.NonWorkingHours = nonWorkingHours;
			project.IsEffortIncidents = this.chkEffortIncidents.Checked;
			project.IsEffortTasks = this.chkEffortTasks.Checked;
			project.IsEffortTestCases = this.chkEffortTestCases.Checked;
			project.IsTimeTrackIncidents = this.chkTimeTrackingIncidents.Checked;
			project.IsTimeTrackTasks = this.chkTimeTrackingTasks.Checked;
			project.IsTasksAutoCreate = this.chkAutoCreateTasks.Checked;
			project.IsReqStatusByTasks = this.chkReqStatusChangedByTasks.Checked;
			project.IsReqStatusByTestCases = this.chkReqStatusChangedByTestCase.Checked;
			project.IsReqStatusAutoPlanned = this.chkReqStatusAutoPlanned.Checked;
			if (String.IsNullOrWhiteSpace(this.txtReqDefaultEstimate.Text))
			{
				project.ReqDefaultEstimate = null;
			}
			else
			{
				decimal pointValue;
				if (Decimal.TryParse(this.txtReqDefaultEstimate.Text, out pointValue))
				{
					project.ReqDefaultEstimate = pointValue;
				}
			}
			if (!String.IsNullOrWhiteSpace(this.txtPointEffort.Text))
			{
				decimal effortValue;
				if (Decimal.TryParse(this.txtPointEffort.Text, out effortValue))
				{
					project.ReqPointEffort = (int)(effortValue * 60M);
				}
			}
			if (String.IsNullOrWhiteSpace(this.txtTaskDefaultEffort.Text))
			{
				project.TaskDefaultEffort = null;
			}
			else
			{
				decimal effortValue;
				if (Decimal.TryParse(this.txtTaskDefaultEffort.Text, out effortValue))
				{
					project.TaskDefaultEffort = (int)(effortValue * 60M);
				}
			}
			projectManager.Update(project);

			//Project Settings
			ProjectSettings projectSettings = new ProjectSettings(ProjectId);
			projectSettings.DisplayOnlyActiveReleasesForDetected = this.chkDetectedReleaseActiveOnly.Checked;
			projectSettings.DisplayHoursOnPlanningBoard = !this.chkPlanUsingPoints.Checked;
			decimal decValue;
			if (Decimal.TryParse(this.txtReleaseWipMultiplier.Text, out decValue))
			{
				projectSettings.KanbanWip_ReleaseWipMultiplier = decValue;
			}
			if (Decimal.TryParse(this.txtIterationWipMultiplier.Text, out decValue))
			{
				projectSettings.KanbanWip_IterationWipMultiplier = decValue;
			}

			//Need to loop through the WIP percentage repeater and serialize that value
			string wipPercentages = "";
			foreach (RepeaterItem repeaterRow in this.rptStatusWip.Items)
			{
				TextBoxEx txtReleaseWipPercent = (TextBoxEx)repeaterRow.FindControl("txtReleaseWipPercent");
				TextBoxEx txtIterationWipPercent = (TextBoxEx)repeaterRow.FindControl("txtIterationWipPercent");
				int requirementStatusId;
				string statusEntry = "";
				if (txtReleaseWipPercent != null && txtIterationWipPercent != null && Int32.TryParse(txtReleaseWipPercent.MetaData, out requirementStatusId))
				{
					statusEntry = requirementStatusId + ",";
					if (Decimal.TryParse(txtReleaseWipPercent.Text, out decValue))
					{
						statusEntry += String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, decValue);
					}
					statusEntry += ",";
					if (Decimal.TryParse(txtIterationWipPercent.Text, out decValue))
					{
						statusEntry += String.Format(GlobalFunctions.FORMAT_DECIMAL_1DP, decValue);
					}
				}
				if (!String.IsNullOrEmpty(statusEntry))
				{
					if (String.IsNullOrEmpty(wipPercentages))
					{
						wipPercentages += statusEntry;
					}
					else
					{
						wipPercentages += "|" + statusEntry;
					}
				}
			}

			//Store the serialized value
			projectSettings.KanbanWip_StatusPercentages = wipPercentages;

			//Save the settings
			projectSettings.Save(UserId);

			//Let the user know that the settings were saved
			this.lblMessage.Text = Resources.Messages.Admin_PlanningOptions_Success;
			this.lblMessage.Type = MessageBox.MessageType.Information;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

	}
}
