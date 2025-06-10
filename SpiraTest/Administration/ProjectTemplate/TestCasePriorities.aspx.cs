using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Edit TestCase Types Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "TestCasePriorities_Title", "Template-Test-Cases/#priority", "TestCasePriorities_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class TestCasePriorities : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TestCasePriorities";

        //Bound data for the grid
        protected SortedList<string, string> flagList;

        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Redirect if there's no project template selected.
                if (ProjectTemplateId < 1)
                    Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProjectTemplate, true);

                //Add event handlers
                this.ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
                this.btnAdd.Click += new EventHandler(btnAdd_Click);
                this.btnUpdate.Click += new EventHandler(btnUpdate_Click);

                //Only load the data once
                if (!IsPostBack)
                {
                    LoadTestCasePriorities();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Loads the test case priorities configured for the current project template
        /// </summary>
        protected void LoadTestCasePriorities()
        {
            const string METHOD_NAME = "LoadTestCasePriorities";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Instantiate the business objects
            TestCaseManager testCaseManager = new TestCaseManager();

            //Get the filter type
            string filterType = this.ddlFilterType.SelectedValue;
            bool activeOnly = (filterType == "allactive");

            //Get the list of testCase priorities for this project
            List<TestCasePriority> priorities = testCaseManager.TestCasePriority_Retrieve(this.ProjectTemplateId, activeOnly);

            //Databind the grid
            this.grdEditTestCasePriorities.DataSource = priorities;
            this.grdEditTestCasePriorities.DataBind();

            //Populate any static fields
            this.lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
            this.lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		/// <summary>
		/// Handles the event raised when the testCase priority ADD button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try          
            {
                //First update the existing priorities
                UpdatePriorities();

				//Now we need to insert the new testCase priority (default to white)
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.TestCasePriority_Insert(this.ProjectTemplateId, Resources.Dialogs.Global_NewValue, "ffffff", true, 0);

				//Now we need to reload the bound dataset for the next databind
				LoadTestCasePriorities();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the testCase priority UPDATE button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			try
			{
                //Update the priorities
                UpdatePriorities();

                //Now we need to reload the bound dataset for the next databind
                LoadTestCasePriorities();

                //Let the user know that the settings were saved
                this.lblMessage.Text = Resources.Messages.Admin_Priorities_Success;
                this.lblMessage.Type = MessageBox.MessageType.Information;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Updates the priorities
        /// </summary>
        protected void UpdatePriorities()
        {
            //First we need to retrieve the existing list of testCase priorities
            TestCaseManager testCaseManager = new TestCaseManager();
            List<TestCasePriority> testCasePriorities = testCaseManager.TestCasePriority_Retrieve(this.ProjectTemplateId, false);

            //We need to make sure that at least one priority is active
            int activeCount = 0;

            //Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
            for (int i = 0; i < this.grdEditTestCasePriorities.Rows.Count; i++)
            {
                //We only look at item rows (i.e. not headers and footers)
                if (grdEditTestCasePriorities.Rows[i].RowType == DataControlRowType.DataRow)
                {
                    //Extract the various controls from the datagrid
                    TextBoxEx txtDisplayName = (TextBoxEx)grdEditTestCasePriorities.Rows[i].FindControl("txtTestCasePriorityName");
                    TextBoxEx txtScore = (TextBoxEx)grdEditTestCasePriorities.Rows[i].FindControl("txtScore");
                    ColorPicker colColor = (ColorPicker)grdEditTestCasePriorities.Rows[i].FindControl("colTestCasePriorityColor");
                    CheckBoxYnEx chkActiveFlag = (CheckBoxYnEx)grdEditTestCasePriorities.Rows[i].FindControl("ddlActive");

                    //Now get the testCase priority id
                    int priorityId = Int32.Parse(txtDisplayName.MetaData);
                    int score = 0;
                    Int32.TryParse(txtScore.Text, out score);

                    //Find the matching row in the dataset
                    TestCasePriority testCasePriority = testCasePriorities.FirstOrDefault(p => p.TestCasePriorityId == priorityId);

                    //Increment the active count if appropriate
                    if (chkActiveFlag.Checked)
                    {
                        activeCount++;
                    }

                    //Make sure we found the matching row
                    if (testCasePriority != null)
                    {
                        //Update the various fields
                        testCasePriority.StartTracking();
                        testCasePriority.Name = txtDisplayName.Text;
                        testCasePriority.Color = colColor.Text;
                        testCasePriority.IsActive = chkActiveFlag.Checked;
                        testCasePriority.Score = score;
                    }
                }
            }

            //Make sure that at least one priority is active
            if (activeCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.Admin_Priorities_AtLeastOneMustBeActive;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
            }

            //Make the updates
            for (int i = 0; i < testCasePriorities.Count; i++)
            {
                testCaseManager.TestCasePriority_Update(testCasePriorities[i]);
            }
        }

        /// <summary>
        /// Changes the display of data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Save the data and then reload
            UpdatePriorities();
            LoadTestCasePriorities();
        }
    }
}