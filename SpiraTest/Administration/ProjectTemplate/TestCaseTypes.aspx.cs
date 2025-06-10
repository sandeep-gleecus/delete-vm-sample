using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.Administration.ProjectTemplate
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Administration Edit TestCase Types Page and handling all raised events
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "TestCaseTypes_Title", "Template-Test-Cases/#types", "TestCaseTypes_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class TestCaseTypes : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectTemplate.TestCaseTypes";

		//Bound data for the grid
		protected SortedList<string, string> flagList;
		protected List<TestCaseWorkflow> workflows;

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
				ddlFilterType.SelectedIndexChanged += new EventHandler(ddlFilterType_SelectedIndexChanged);
				btnTestCaseTypesAdd.Click += new EventHandler(btnTestCaseTypesAdd_Click);
				btnTestCaseTypesUpdate.Click += new EventHandler(btnTestCaseTypesUpdate_Click);

				//Only load the data once
				if (!IsPostBack)
				{
					LoadTestCaseTypes();
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
		/// Handles the event raised when the testCase types ADD button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnTestCaseTypesAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnTestCaseTypesAdd_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Update the types
				UpdateTypes();

				//Now we need to insert the new testCase type
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.TestCaseType_Insert(ProjectTemplateId, Resources.Dialogs.Global_NewValue, null, false, true, false);

				//Now we need to reload the bound dataset for the next databind
				LoadTestCaseTypes();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the testCase types UPDATE button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnTestCaseTypesUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnTestCaseTypesUpdate_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!IsValid)
			{
				return;
			}

			try
			{
				//Update the types
				bool success = UpdateTypes();

				//Now we need to reload the bound dataset for the next databind
				if (success)
				{
					LoadTestCaseTypes();

					//Let the user know that the settings were saved
					lblMessage.Text = Resources.Messages.TestCaseTypes_Success;
					lblMessage.Type = MessageBox.MessageType.Information;
				}
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
		/// Update the testCase types
		/// </summary>
		protected bool UpdateTypes()
		{
			//First we need to retrieve the existing list of testCase types
			TestCaseManager testCaseManager = new TestCaseManager();
			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(ProjectTemplateId, false);

			//Now iterate through the rows and get the id and values of the text-box containing the name and the active drop-down list
			for (int i = 0; i < grdEditTestCaseTypes.Rows.Count; i++)
			{
				//We only look at item rows (i.e. not headers and footers)
				if (grdEditTestCaseTypes.Rows[i].RowType == DataControlRowType.DataRow)
				{
					//Extract the various controls from the datagrid
					TextBoxEx txtTestCaseTypeName = (TextBoxEx)grdEditTestCaseTypes.Rows[i].FindControl("txtTestCaseTypeName");
					DropDownListEx ddlWorkflow = (DropDownListEx)grdEditTestCaseTypes.Rows[i].FindControl("ddlWorkflow");
					RadioButtonEx radDefault = (RadioButtonEx)grdEditTestCaseTypes.Rows[i].FindControl("radDefault");
					CheckBoxYnEx chkActiveYn = (CheckBoxYnEx)grdEditTestCaseTypes.Rows[i].FindControl("chkActiveYn");
					CheckBoxYnEx chkExploratory = (CheckBoxYnEx)grdEditTestCaseTypes.Rows[i].FindControl("chkExploratory");

					//Need to make sure that the default item is an active one
					if (radDefault.Checked && !(chkActiveYn.Checked))
					{
						lblMessage.Type = MessageBox.MessageType.Error;
						lblMessage.Text = Resources.Messages.TestCaseTypes_CannotSetDefaultTypeInactive;
						return false;
					}

					//Now get the testCase type id
					int testCaseTypeId = Int32.Parse(txtTestCaseTypeName.MetaData);

					//Find the matching row in the dataset
					TestCaseType testCaseType = testCaseTypes.FirstOrDefault(t => t.TestCaseTypeId == testCaseTypeId);

					//Make sure we found the matching row
					if (testCaseType != null)
					{
						//Update the various fields
						testCaseType.StartTracking();
						testCaseType.Name = txtTestCaseTypeName.Text.Trim();
						testCaseType.TestCaseWorkflowId = Int32.Parse(ddlWorkflow.SelectedValue);
						testCaseType.IsDefault = radDefault.Checked;
						testCaseType.IsActive = chkActiveYn.Checked;
						testCaseType.IsExploratory = chkExploratory.Checked;
					}
				}
			}

			foreach (TestCaseType testCaseType in testCaseTypes)
			{
				testCaseManager.TestCaseType_Update(testCaseType);
			}
			return true;
		}

		/// <summary>
		/// Loads the testCase types configured for the current project
		/// </summary>
		protected void LoadTestCaseTypes()
		{
			const string METHOD_NAME = "LoadTestCaseTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Instantiate the business objects
			TestCaseManager testCaseManager = new TestCaseManager();
			TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();

			//Get the yes/no flag list
			flagList = testCaseManager.RetrieveFlagLookup();

			//Get the filter type
			string filterType = ddlFilterType.SelectedValue;
			bool activeOnly = (filterType == "allactive");

			//Get the list of testCase types for this project
			List<TestCaseType> testCaseTypes = testCaseManager.TestCaseType_Retrieve(ProjectTemplateId, activeOnly);
			grdEditTestCaseTypes.DataSource = testCaseTypes;

			//Get the list of active workflows for this project (used as a lookup)
			workflows = workflowManager.Workflow_Retrieve(ProjectTemplateId, true);

			//Databind the grid
			grdEditTestCaseTypes.DataBind();

			//Populate any static fields
			lblTemplateName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectTemplateName);
			lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "Default");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Changes the display of data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ddlFilterType_SelectedIndexChanged(object sender, EventArgs e)
		{
			//Save the data and then reload
			UpdateTypes();
			LoadTestCaseTypes();
		}
	}
}
