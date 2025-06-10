using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;


namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>
	/// Displays the admin page for managing project membership in the system
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_AssignedApprovers_Title", "Product-Users/#product-membership", "Admin_AssignedApprovers_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class AssignedApprovers : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.AssignedApprovers::";
		protected List<ProjectRole> projectRoles;
		protected UserManager userManager = new UserManager();
		private List<ProjectSignature> assignedApprovers = null;
		private const int TEST_CASE_ARTIFACT_TYPE_ID = 2;

		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the error messages
			lblMessage.Text = "";
			btnProjectMembershipUpdate.Click += new DropMenuEventHandler(btnProjectMembershipUpdate_Click);
			grdUserMembership.RowDataBound += new GridViewRowEventHandler(grdUserMembership_RowDataBound);

			//Don't reload on Postback
			if (!IsPostBack)
			{
				//Load and bind data
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Selects the project role dropdown for each row in the grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void grdUserMembership_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				var dataItem = e.Row.DataItem as ProjectUser;
				var found = assignedApprovers.Where(x => x.UserId == dataItem.UserId).FirstOrDefault();

				if (found != null)
				{
					(e.Row.FindControl("chkActive") as CheckBoxYnEx).Checked = true;
					(e.Row.FindControl("txtOrderId") as TextBoxEx).Text = found.OrderId.HasValue ? found.OrderId.Value.ToString() : String.Empty;
				}
			}
		}

		/// <summary>
		/// Loads the project membership information
		/// </summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (ProjectId < 1)
			{
				Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);
			}

			assignedApprovers = userManager.RetrieveAssignedApproversForProject(this.ProjectId);

			//Instantiate the business objects
			Business.ProjectManager projectManager = new Business.ProjectManager();

			//Retrieve the list of project roles
			projectRoles = projectManager.RetrieveProjectRoles(true);

			//Retrieve the project user membership dataset
			List<ProjectUser> projectUsers = projectManager.RetrieveUserMembershipById(ProjectId, true);
			grdUserMembership.DataSource = projectUsers;
			DataBind();

			//Populate any static fields
			lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
			lnkAdminHome.NavigateUrl = Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project membership Update button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnProjectMembershipUpdate_Click(object sender, EventArgs e)
		{
			
			const string METHOD_NAME = "btnProjectMembershipUpdate_Click()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//See if we have a project in session, if so retrieve the ID and name
			if (ProjectId > 0)
			{
				List<int> userIds = new List<int>();
				assignedApprovers = userManager.RetrieveAssignedApproversForProject(this.ProjectId);

				List<ProjectSignature> changedEntities = new List<ProjectSignature>();

				foreach (GridViewRow item in grdUserMembership.Rows)
				{
					CheckBoxYnEx chkActive = item.FindControl("chkActive") as CheckBoxYnEx;
					LabelEx lblUserId = item.FindControl("lblUserId") as LabelEx;
					string userId = lblUserId.Text;
					var found = assignedApprovers.Where(x => x.UserId == Int32.Parse(userId) && x.ArtifactTypeId == TEST_CASE_ARTIFACT_TYPE_ID).FirstOrDefault();

					if (chkActive.Checked)
					{

						ProjectSignature newItem = new ProjectSignature
						{
							ArtifactTypeId = TEST_CASE_ARTIFACT_TYPE_ID,
							IsActive = true,
							IsTestCaseSignatureRequired = true,
							LastUpdated = DateTime.Now,
							OrderId = null,
							ProjectId = this.ProjectId,
							UserId = Int32.Parse(userId)
						};

						changedEntities.Add(newItem.MarkAsAdded());

					}
					else
					{
						if (found != null)
						{
							found.OrderId = null;
							found.IsTestCaseSignatureRequired = false;
							found.IsActive = false;
							changedEntities.Add(found.MarkAsModified());
						}
					}
					//var found = assignedApprovers.Where(x => x.UserId == Int32.Parse(userId) && x.ArtifactTypeId == TEST_CASE_ARTIFACT_TYPE_ID).FirstOrDefault();
					//if(found != null)
					//{
					//	found.StartTracking();
					//	if (chkActive.Checked)
					//	{
					//		TextBoxEx txtOrderId = item.FindControl("txtOrderId") as TextBoxEx;
					//		if (!String.IsNullOrEmpty(txtOrderId.Text))
					//		{
					//			int orderId = Int32.Parse(txtOrderId.Text);
					//			found.OrderId = orderId;
					//		}
					//		else
					//		{
					//			found.IsTestCaseSignatureRequired = false;
					//			found.OrderId = null;
					//		}
					//	}
					//	else
					//	{
					//		found.IsTestCaseSignatureRequired = false;
					//		found.OrderId = null;
					//	}

						//	changedEntities.Add(found.MarkAsModified());
						//}
				}



				userManager.UpdateProjectAssignedApprovers(changedEntities);
				//Update the bound dataset and reload (in case some roles were not allowed to be changed)

				LoadAndBindData();

				//Display a confirmation message
				lblMessage.Text = Resources.Messages.Admin_ProjectMembership_Success;
				lblMessage.Type = MessageBox.MessageType.Information;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
