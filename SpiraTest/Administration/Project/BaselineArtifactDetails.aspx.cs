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
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_BaselineArtifactDetails", "Product-General-Settings/#baseline-artifact-details", "Admin_BaselineDetails")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
	public partial class BaselineArtifactDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.BaselineArtifactDetails::";

		/// <summary>The baseline we're pulling details for.</summary>
		protected int baselineId;
		protected int artifactId;
		protected int artifactTypeId;

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
				try
				{
					baselineId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_BASELINE_ID]);
					artifactTypeId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID]);
					artifactId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_ARTIFACT_ID]);
				}
				catch (FormatException)
				{
					Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "BaselineList"), true);
				}

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

				//Retrieve the artifact specified
				ArtifactManager artifactManager = new ArtifactManager();
				ArtifactInfo artifactInfo = artifactManager.RetrieveArtifactInfo((Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, ProjectId);

                //Populate fields here
				lnkChangeSet.Text = currentBaseline.ChangeSetId.ToString();
				lnkChangeSet.NavigateUrl = "~/" + ProjectId + "/Administration/HistoryDetails/" + currentBaseline.ChangeSetId.ToString() + ".aspx";
				lnkRelease.Text = currentBaseline.ReleaseFullName;
				lnkRelease.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, ProjectId, currentBaseline.ReleaseId.Value);
				lblBaseline.Text = currentBaseline.Name;
				if (artifactInfo != null)
				{
					lnkArtifact.Text = artifactInfo.Name;
					lnkArtifact.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)artifactTypeId, ProjectId, artifactId);
					imgArtifact.ImageUrl = "Images/" + GlobalFunctions.GetIconForArtifactType(artifactTypeId);
				}

				//Populate the grid's fixed filters.
				Dictionary<string, object> setFilters = new Dictionary<string, object>();
				setFilters.Add("BaselineLatestId", currentBaseline.BaselineId);
				setFilters.Add("ArtifactId", artifactId);
				setFilters.Add("ArtifactType", artifactTypeId);
				if (previousBaseline != null)
					setFilters.Add("BaselinePreviousId", previousBaseline.BaselineId);
				sgChanges.ProjectId = ProjectId;
				sgChanges.SetFilters(setFilters);
				sgChanges.DisplayTypeId = (int)Artifact.DisplayTypeEnum.Baseline_ArtifactChanges;
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
			baselineId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_BASELINE_ID]);
			Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, String.Format("BaselineDetails/{0}", baselineId)), true);
		}
	}
}
