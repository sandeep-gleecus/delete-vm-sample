using System;
using System.Collections.Generic;
using System.Linq;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.UserControls;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
	/// <summary>Displays the administration history list page</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_HistoryChangeDetails", "Product-General-Settings/#history-details-screen", "Admin_HistoryChangeDetails")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator | AdministrationLevelAttribute.AdministrationLevels.ProjectOwner)]
	public partial class HistoryDetails : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Project.HistoryDetails::";

		protected int _changeSetId;
		private Artifact.ArtifactTypeEnum _artType;
		private int _artId;

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
						UrlRewriterModule.RetrieveRewriterURL(
							UrlRoots.NavigationLinkEnum.Administration, ProjectId) +
						"?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject,
						true);

				//Set events and data..
				btnBackList.Click += btnBackList_Click;
				btnViewItem.Click += btnViewItem_Click;
				lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
				lnkAdminHome.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

				_changeSetId = int.Parse(Request.QueryString[GlobalFunctions.PARAMETER_CHANGESET_ID]);
				if (_changeSetId < 0)
					_changeSetId = _changeSetId * (-1);

				//Add the client event handler to the background task process
				Dictionary<string, string> handlers = new Dictionary<string, string>();
				handlers.Add("succeeded", "operation_success");
				ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

				//Specify the changeset as the standard filter
				Dictionary<string, object> standardFilters = new Dictionary<string, object>
				{
					{ "ChangeSetId", _changeSetId }
				};
				sgHistory.SetFilters(standardFilters);
				sgHistory.ProjectId = ProjectId;
				sgHistory.DataBind();

				//Get the changeset..
				HistoryManager historyManager = new HistoryManager();
				HistoryChangeSet historyChangeSetView = historyManager.RetrieveChangeSetById(_changeSetId, true, true, true);
				txtChangeType.Text = historyChangeSetView.Type.Name;
				txtChangeType.Text += ((historyChangeSetView.RevertId.HasValue) ? " (" + string.Format(Resources.Main.Admin_History_ToChange, historyChangeSetView.RevertId.Value.ToString()) + ")" : "");
				txtName.Text = historyChangeSetView.ChangeSetId.ToString();
				txtUser.Text = historyChangeSetView.User.FullName;
				lnkArtifact.Text = (historyChangeSetView.ArtifactTypeId == (int)ArtifactTypeEnum.Project ? Resources.Fields.Project : historyChangeSetView.ArtifactType.Name)
					+ " [" + historyChangeSetView.ArtifactType.Prefix + ":" + historyChangeSetView.ArtifactId.ToString() + "]";
				txtDate.Text = string.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(historyChangeSetView.ChangeDate));
				lblSignatureHash.Text = historyChangeSetView.SignatureHash;
				if (historyChangeSetView.Signed == (int)HistoryChangeSetView.SignedStatusEnum.Valid)
				{
					statusSigned.Text = Resources.Fields.Signature_Valid;
					statusSigned.DataCssClass = "ExecutionStatusPassed";
				}
				else if (historyChangeSetView.Signed == (int)HistoryChangeSetView.SignedStatusEnum.Invalid)
				{
					statusSigned.Text = Resources.Fields.Signature_Invalid;
					statusSigned.DataCssClass = "ExecutionStatusFailed";
				}
				else
				{
					statusSigned.Text = Resources.Fields.Signature_NotSigned;
				}

				//Remove 'revert' button if changeset is not a field / detail change [TK:2670]
				if (historyChangeSetView.Details.Count() < 1)
				{
					//Hide the revert button.
					btnRestore.Visible = false;
				}
				//If this is a changeset without any changes at all, display a message at top of page and hide the grid. [TK:2670]
				if (historyChangeSetView.Details.Count() < 1 && historyChangeSetView.PositionChanges.Count() < 1 && historyChangeSetView.AssociationChanges.Count() < 1)
				{
					//Display a message saying that there are no changes.
					lblNoChanges.Visible = true;

					//Hide the grid.
					changesdiv.Visible = false;
					sgHistory.Visible = false; //Should not be needed, but just in case for Ajax calls.
				}


				//Set the URL to display the artifact, for risk mitigations and requirement steps they need to be the parent artifact instead
				string artifactUrl = "";
				if (historyChangeSetView.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.RequirementStep)
				{
					int requirementStepId = historyChangeSetView.ArtifactId;
					RequirementStep requirementStep = new RequirementManager().RetrieveStepById(requirementStepId);
					if (requirementStep != null)
					{
						artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, historyChangeSetView.ProjectId.Value, requirementStep.RequirementId);
					}
				}
				else if (historyChangeSetView.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.RiskMitigation)
				{
					int riskMitigationId = historyChangeSetView.ArtifactId;
					RiskMitigation riskMitigation = new RiskManager().RiskMitigation_RetrieveById(riskMitigationId);
					if (riskMitigation != null)
					{
						artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, historyChangeSetView.ProjectId.Value, riskMitigation.RiskId);
					}
				}
				else if (historyChangeSetView.ArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Project)
				{
					// Can not view the project. Just hide the button.
					btnViewItem.Enabled = false;
					btnViewItem.Visible = false;
				}
				else
				{
					artifactUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)historyChangeSetView.ArtifactTypeId, historyChangeSetView.ProjectId.Value, historyChangeSetView.ArtifactId);
				}
				lnkArtifact.NavigateUrl = artifactUrl;

				// Set the field set of the change - are we changing standard or custom fields, associations or position changes
				if (historyChangeSetView.Details.Count > 0 && historyChangeSetView.Details.First().CustomPropertyId.HasValue)
				{
					txtFieldType.Text = Resources.Main.Admin_History_CustomLabel;
				}
				else if (historyChangeSetView.Details.Count > 0 && historyChangeSetView.Details.First().FieldId.HasValue)
				{
					txtFieldType.Text = Resources.Main.Admin_Histoy_StandardLabel;
				}
				else if (historyChangeSetView.AssociationChanges.Count > 0)
				{
					txtFieldType.Text = Resources.Main.Admin_History_AssociationLabel;
				}
				else if (historyChangeSetView.PositionChanges.Count > 0)
				{
					txtFieldType.Text = Resources.Main.Admin_History_PositionLabel;
				}
				// if we do not what type of field change it is, hide the field completely
				else
				{
					liFieldType.Visible = false;
				}


				if (string.IsNullOrEmpty(historyChangeSetView.ArtifactDesc))
				{
					txtArtifactDesc2.Text = "";
				}
				else
				{
					txtArtifactDesc2.Text = Microsoft.Security.Application.Encoder.HtmlEncode(historyChangeSetView.ArtifactDesc);
				}

				_artId = historyChangeSetView.ArtifactId;
				_artType = (Artifact.ArtifactTypeEnum)historyChangeSetView.ArtifactTypeId;

				//For this change, get the type..
				HistoryManager.ChangeSetTypeEnum typeView = (HistoryManager.ChangeSetTypeEnum)historyChangeSetView.ChangeTypeId;

				//See if the last change is a delete or purge..
				HistoryChangeSet lastChangeSet = historyManager.RetrieveLastChangeSetForArtifactId(historyChangeSetView.ArtifactId, (Artifact.ArtifactTypeEnum)historyChangeSetView.ArtifactTypeId);
				bool isLastDelete = (lastChangeSet.ChangeTypeId == (int)HistoryManager.ChangeSetTypeEnum.Deleted);
				bool isLastPurged = (lastChangeSet.ChangeTypeId == (int)HistoryManager.ChangeSetTypeEnum.Purged);

				//Hide the Purge and Revert buttons if Baselining is enabled.
				projSettings = new ProjectSettings(ProjectId);
				bool baseLine_Enabled = projSettings.BaseliningEnabled && Common.Global.Feature_Baselines;

				//Get the latest Changeset number if Baselines are enabled.
				long? baselineChangeSetId = null;
				if (baseLine_Enabled)
				{
					//baselineChangeSetId = new BaselineManager().Baseline_RetrieveForProduct(projectId)?.Select(b => b.BaselineId)?.Max();
					var baselines = new BaselineManager().Baseline_RetrieveForProduct(ProjectId);
					if (baselines != null && baselines.Count > 0)
						baselineChangeSetId = baselines.Select(b => b.ChangeSetId).Max();
				}

				//The deleted message. (Only if the last is deleted or purged.)
				divIsDeleted.Visible = (isLastDelete || isLastPurged);

				//Prepare boolean for determining if 'Restore' can be visible.
				bool purgeHidden = isLastPurged;                                         // The item has already been purged, or - 
				purgeHidden |= (typeView == HistoryManager.ChangeSetTypeEnum.Imported || // This history record is an Import, or - 
					typeView == HistoryManager.ChangeSetTypeEnum.Exported ||             // This history record is an Export, or -
					typeView == HistoryManager.ChangeSetTypeEnum.Purged ||               // This history record is a Purge, or -
					typeView == HistoryManager.ChangeSetTypeEnum.Undelete ||             // This history record is already an Undelete, or -
					typeView == HistoryManager.ChangeSetTypeEnum.Rollback ||             // This history record is already a Rollback, or -
					typeView == HistoryManager.ChangeSetTypeEnum.Added);                 // This history record is a creation, or -

				// We only enable purge IF: The change type is correct, and
				// if it's not been purged already, and the last one is a delete
				// if Baselines is NOT enabled, OR, baselines IS enabled AND the changeset is higher than the last Baseline.
				btnPurge.Visible = (isLastDelete && !purgeHidden && (!baseLine_Enabled || baselineChangeSetId == null || _changeSetId > baselineChangeSetId));

				//We only want the restore button visible if Basleines are NOT enabled, OR, baselines ARE
				//  enabled, and the Changeset is higher than the last baseline.
				btnRestore.Visible &= (!purgeHidden && (!baseLine_Enabled || baselineChangeSetId == null || _changeSetId > baselineChangeSetId));
				// ... And we're not looking at a project change.
				btnRestore.Visible &= (historyChangeSetView.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Project);

				//View the active artifact button/div. (Only if the last isn't deleted or purged.)
				lnkArtifact.Enabled = btnViewItem.Visible = !(isLastPurged && isLastDelete) && (historyChangeSetView.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Project);

				if (!IsPostBack)
				{
					//DFatabind for the script on the menu?
					DataBind();
				}

			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryList"), true);
			}
		}

		/// <summary>
		/// Views the specific artifact
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void btnViewItem_Click(object sender, ServerControls.DropMenuEventArgs e)
		{
			string url = lnkArtifact.NavigateUrl;
			Response.Redirect(url, true);
		}

		/// <summary>Hit when the user wants to restore the item to this changeset.</summary>
		/// <param name="sender">btnRestore</param>
		/// <param name="e">DropMenuEventArgs</param>
		/// <remarks>2019-02 Simon: it looks like this method is never called - background service handles reverting instead - not deleted just in case</remarks>
		private void btnRestore_Click(object sender, ServerControls.DropMenuEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "btnRestore_Click()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//If Baselining is enabled, simply return.
			if (projSettings.BaseliningEnabled && Common.Global.Feature_Baselines)
			{
				Logger.LogTraceEvent(METHOD_NAME, "Baselines: Event can not execute.");
				return;
			}

			try
			{
				string resLog = "";

				//Try the restore.
				HistoryManager.RollbackResultEnum resResult = new HistoryManager().RollbackHistory(ProjectId, ProjectTemplateId, _artType, _artId, _changeSetId, UserId, ref resLog);

				switch (resResult)
				{
					case HistoryManager.RollbackResultEnum.Error:
						lblMessage2.Type = ServerControls.MessageBox.MessageType.Error;
						lblMessage2.Text = Resources.Messages.Admin_History_RevertError;
						break;
					case HistoryManager.RollbackResultEnum.Success:
						Session["MSGTYPE"] = ServerControls.MessageBox.MessageType.Information;
						Session["MSG"] = Resources.Messages.Admin_History_RevertSuccessful;
						Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryList"), true);
						break;
					case HistoryManager.RollbackResultEnum.Warning:
						Session["MSGTYPE"] = ServerControls.MessageBox.MessageType.Information;
						Session["MSG"] = Resources.Messages.Admin_History_RevertWarning;
						Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryList"), true);
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				lblMessage2.Type = ServerControls.MessageBox.MessageType.Error;
				lblMessage2.Text = Resources.Messages.Admin_History_RevertError;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Hit when the user wants to go back to the list.</summary>
		/// <param name="sender">DropMenu</param>
		/// <param name="e">DropMenuEventArgs</param>
		private void btnBackList_Click(object sender, ServerControls.DropMenuEventArgs e)
		{
			Response.Redirect(UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryList"), true);
		}
	}
}
