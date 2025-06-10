using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>
	/// Displays the testing settings admin page
	/// </summary>
	[
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "TestingSettings_Title", "Product-Planning/#testing-settings", "TestingSettings_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TestingSettings : AdministrationBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Project.TestingSettings::";
        private ProjectSettings projectSettings = null;

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
            //Get the ProjectSettings collection
            if (ProjectId > 0)
            {
                projectSettings = new ProjectSettings(ProjectId);
            }

            //Load the page if not postback
            if (!Page.IsPostBack)
			{
                //Instantiate the business classes
                ProjectManager projectManager = new ProjectManager();
                //Retrieve the project by its id
                DataModel.Project project;
                try
                {
                    project = projectManager.RetrieveById(ProjectId);
                }
                catch (ArtifactNotExistsException)
                {
                    //Project does not exist any more
                    Response.Redirect("~/Administration/ProjectList.aspx", true);
                    return;
                }
                //Set the project name
                lblProjectName.Text = GlobalFunctions.HtmlRenderAsPlainText(project.Name);
                this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

                LoadAndBindData();
            }

			//Add event handlers
			this.btnGeneralUpdate.Click += new EventHandler(btnGeneralUpdate_Click);
			this.btnCancel.Click += new EventHandler(btnCancel_Click);

        }

		/// <summary>
		/// Redirect back to the product administration home page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnCancel_Click(object sender, EventArgs e)
		{
			Response.Redirect("Default.aspx");
		}

		/// <summary>
		/// Loads and displays the panel's contents when called
		/// </summary>
		protected void LoadAndBindData()
		{
            if (ProjectId < 1)
            {
                Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);
            }
            
            //Databind
            this.DataBind();

            //Populate the fields from the global configuration settings).
            if (projectSettings != null)
            {
                this.chkExecutionDisplayBuild.Checked = projectSettings.Testing_ExecutionDisplayBuild;
                this.chkExecutionDisablePassAll.Checked = projectSettings.Testing_ExecutionDisablePassAll;
                this.chkExecutionDisableBlocked.Checked = projectSettings.Testing_ExecutionDisableBlocked;
                this.chkExecutionDisableCaution.Checked = projectSettings.Testing_ExecutionDisableCaution;
                this.chkExecutionDisableNA.Checked = projectSettings.Testing_ExecutionDisableNA;
                this.chkExecutionActualResultAlwaysRequire.Checked = projectSettings.Testing_ExecutionActualResultAlwaysRequired;
                this.chkExecutionRequireIncident.Checked = projectSettings.Testing_ExecutionRequireIncident;
                this.chkExecutionAllowTasks.Checked = projectSettings.Testing_ExecutionAllowTasks;
                this.chkTestCaseAutoUnassign.Checked = projectSettings.Testing_PassingTestCaseUnassigns;
                this.chkTestSetAutoUnassign.Checked = projectSettings.Testing_CompletingTestSetUnassigns;
                this.chkCreateDefaultTestStep.Checked = projectSettings.Testing_CreateDefaultTestStep;
                this.chkExecuteSetsOnly.Checked = projectSettings.Testing_ExecuteSetsOnly;
                this.chkEnableWorX.Checked = projectSettings.Testing_WorXEnabled;
				this.chkDisableRollupCalculations.Checked = projectSettings.General_DisableRollupCalculations;
			}

            if (!Common.Global.Feature_Tasks)
            {
                this.wrapperExecutionAllowTasks.Visible = false;
            }
        }

		/// <summary>
		/// This event handler updates the stored general system settings
		/// </summary>
		/// <param name="sender">The object sending the event</param>
		/// <param name="e">The event handler arguments</param>
		private void btnGeneralUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnGeneralUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure all validators have succeeded
			if (!Page.IsValid)
			{
				return;
			}

            //Update the stored configuration values from the provided information
            if (projectSettings != null)
            {
                projectSettings.Testing_ExecutionDisplayBuild = this.chkExecutionDisplayBuild.Checked;
                projectSettings.Testing_ExecutionDisablePassAll = this.chkExecutionDisablePassAll.Checked;
                projectSettings.Testing_ExecutionDisableBlocked = this.chkExecutionDisableBlocked.Checked;
                projectSettings.Testing_ExecutionDisableCaution = this.chkExecutionDisableCaution.Checked;
                projectSettings.Testing_ExecutionDisableNA = this.chkExecutionDisableNA.Checked;
                projectSettings.Testing_ExecutionActualResultAlwaysRequired = this.chkExecutionActualResultAlwaysRequire.Checked;
                projectSettings.Testing_ExecutionRequireIncident = this.chkExecutionRequireIncident.Checked;
                projectSettings.Testing_ExecutionAllowTasks = this.chkExecutionAllowTasks.Checked;
                projectSettings.Testing_PassingTestCaseUnassigns = this.chkTestCaseAutoUnassign.Checked;
                projectSettings.Testing_CompletingTestSetUnassigns = this.chkTestSetAutoUnassign.Checked;
                projectSettings.Testing_CreateDefaultTestStep = this.chkCreateDefaultTestStep.Checked;
                projectSettings.Testing_ExecuteSetsOnly = this.chkExecuteSetsOnly.Checked;
                projectSettings.Testing_WorXEnabled = this.chkEnableWorX.Checked;
				projectSettings.General_DisableRollupCalculations = this.chkDisableRollupCalculations.Checked;
				projectSettings.Save(UserId);

			    //Let the user know that the settings were saved
			    this.lblMessage.Text = Resources.Messages.TestingSettings_Success;
			    this.lblMessage.Type = MessageBox.MessageType.Information;
            }


			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
