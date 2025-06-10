using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>Displays the administration history list page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_BaselineDetails", "Product-General-Settings/#baseline-details", "Admin_BaselineDetails")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
	public partial class BaselineDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.BaselineDetails::";

		/// <summary>The baseline we're pulling details for.</summary>
		protected int baselineId;
		/// <summary>Used to store our project settings, only pulled once.</summary>
		private ProjectSettings projSettings = null;

		/// <summary>Called when the control is first loaded</summary>
		/// <param name="sender">Page</param>
		/// <param name="e">EventArgs</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Redirect if there's no project selected.
				if (ProjectId < 1)
					Response.Redirect(
						UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Administration, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject,
						true
					);

				//Check that Baselines is even enabled and available.
				if (!Common.Global.Feature_Baselines)
				{
					Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_BaselinesNotAvailable, true);
				}
				else
				{
					var projSettings = new ProjectSettings(ProjectId);
					if (projSettings == null || !projSettings.BaseliningEnabled)
					{
						Response.Redirect("~/Administration/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_BaselinesNotEnabled, true);
					}
				}

				//Set events and data..
				btnBackList.Click += btnBackList_Click;
				baselineId = int.Parse(Request.QueryString["bl"]);

				//Pull the baseline - we're not worried about Postback, since there are no actions (yet) available on this page.
				var baselines = new BaselineManager().Baseline_RetrieveForProduct(ProjectId);

				//Find the baseline we're viewing, and the one directly before it.
				baselines = baselines.OrderBy(b => b.BaselineId).ToList();
				ProjectBaseline currentBaseline = baselines.SingleOrDefault(b => b.BaselineId == baselineId);
				ProjectBaseline previousBaseline = null;
				if (currentBaseline != null)
				{
					//Get the prevoid Baseline.
					previousBaseline = baselines.LastOrDefault(b => b.BaselineId < baselineId);
				}
				else
				{
					//Need to be sent back to the Baseline list. Somehow the number they selected was bad.
				}

                //Populate fields here.
                txtDate.Text = String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(currentBaseline.CreationDate));
				txtDescription.Text = currentBaseline.Description;
				txtIsActive.Text = currentBaseline.IsActive.ToString();
				txtName.Text = currentBaseline.Name;
				txtUser.Text = currentBaseline.CreatorUserName;
				txtBaselineID.Text = currentBaseline.BaselineId.ToString();
				lnkChangeSet.Text = currentBaseline.ChangeSetId.ToString();
				lnkChangeSet.NavigateUrl = "~/" + ProjectId + "/Administration/HistoryDetails/" + currentBaseline.ChangeSetId.ToString() + ".aspx";
				lnkRelease.Text = currentBaseline.ReleaseFullName;
				lnkRelease.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, currentBaseline.ReleaseId.Value);
				lnkPrevBaseline.Text = previousBaseline == null ? Main.ProjectStart : previousBaseline.Name;
				lnkPrevBaseline.NavigateUrl = previousBaseline == null ? null : "~/" + ProjectId + "/Administration/BaselineDetails/" + previousBaseline.BaselineId.ToString() + ".aspx";
                imgBaseline.Visible = (previousBaseline != null);
                lnkPrevBaseline.Enabled = (previousBaseline != null);

                //Populate the grid's fixed filters.
                Dictionary<string, object> setFiltrs = new Dictionary<string, object>();
				setFiltrs.Add("baselineLatest", currentBaseline.BaselineId);
				if (previousBaseline != null)
					setFiltrs.Add("baselinePrevious", previousBaseline.BaselineId);
				sgChanges.ProjectId = ProjectId;
				sgChanges.SetFilters(setFiltrs);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "BaselineList"), true);
			}
		}

		/// <summary>Hit when the user wants to go back to the list.</summary>
		/// <param name="sender">DropMenu</param>
		/// <param name="e">DropMenuEventArgs</param>
		private void btnBackList_Click(object sender, ServerControls.DropMenuEventArgs e)
		{
			Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "BaselineList"), true);
		}
	}
}
