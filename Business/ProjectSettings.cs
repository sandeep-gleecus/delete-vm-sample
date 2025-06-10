using System;
using System.Collections.Generic;
using System.Configuration;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	[SettingsProvider(typeof(ProjectSettingsProvider))]
	public sealed partial class ProjectSettings
	{
		/// <summary>Stores changes until the next Save() is called.</summary>
		private Dictionary<string, SettingChangeRecord> latestChanges = new Dictionary<string, SettingChangeRecord>();

		/// <summary>
		/// Used by .NET to instantiate the default instance, which should not be used
		/// </summary>
		public ProjectSettings()
		{
		}

		/// <summary>
		/// The constructor that should be used when using the project settings
		/// </summary>
		/// <param name="projectId">The id of the project these settings are for</param>
		public ProjectSettings(int projectId)
		{
			//Store the project in context
			Context.Add("ProjectId", projectId);

			// Hook up event to record changes.
			SettingChanging += SettingChangingEventHandler;
		}

		/// <summary>Saves the new project settings, saving history. </summary>
		/// <param name="userId">The user instianting the change.</param>
		public void Save(int userId)
		{
			// Call the base Save() first. In case there's an issue and things can't be saved, we 
			//  don't have a rogue History entry.
			base.Save();

			//Since it IS possible to call Save() without changing ANYTHING (see TestingSettings.aspx.cs)
			//  we want to check to see if there's a reason we should create a history change set first.
			if (latestChanges.Count > 0)
			{
				//We need to get Project dtails.
				Project proj = new ProjectManager().RetrieveById((int)Context["ProjectId"]);

				//Generate our changeset.
				HistoryChangeSet changeSet = new HistoryChangeSet()
				{
					ArtifactDesc = proj.Name,
					ArtifactTypeId = (int)ArtifactTypeEnum.Project,
					ArtifactId = proj.ProjectId,
					ChangeDate = DateTime.UtcNow,
					ChangeTypeId = (int)ChangeSetTypeEnum.Modified,
					ProjectId = proj.ProjectId,
					UserId = userId
				};

				foreach (var change in latestChanges)
				{
					HistoryDetail newDet = new HistoryDetail()
					{
						FieldCaption = change.Key,
						FieldName = change.Key,
						NewValue = change.Value.NewValue.ToString(),
						OldValue = change.Value.OldValue.ToString(),
					};
					changeSet.Details.Add(newDet);
				}

				//Call the history manager to save it all.
				new HistoryManager().Insert(changeSet);

				//Audit Trail logs

				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

				string adminSectionName = "View / Edit Projects";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;

				//Create a new changeset.
				TST_ADMIN_HISTORY_CHANGESET_AUDIT hsChangeSet = new TST_ADMIN_HISTORY_CHANGESET_AUDIT();
				hsChangeSet.ADMIN_USER_ID = userId;
				hsChangeSet.ADMIN_SECTION_ID = adminSectionId;
				hsChangeSet.CHANGE_DATE = DateTime.UtcNow;
				hsChangeSet.HISTORY_CHANGESET_TYPE_ID = (int)ChangeSetTypeEnum.Modified;
				hsChangeSet.ACTION_DESCRIPTION = "Baselining Enabled in Project";

				long changeSetId = adminAuditManager.Insert1(hsChangeSet);
				TST_ADMIN_HISTORY_DETAILS_AUDIT detail = new TST_ADMIN_HISTORY_DETAILS_AUDIT();

				foreach (var change in latestChanges)
				{
					detail.ADMIN_CHANGESET_ID = changeSetId;
					detail.ADMIN_ARTIFACT_FIELD_NAME = change.Key;
					detail.ADMIN_ARTIFACT_FIELD_CAPTION = change.Key;
					detail.ADMIN_USER_ID = userId;
					detail.ADMIN_PROPERTY_NAME = change.Key;
					detail.NEW_VALUE = change.Value.NewValue.ToString();
					detail.OLD_VALUE = change.Value.OldValue.ToString();
				}

				adminAuditManager.DetailInsert1(detail);

				//Clear out changes.
				latestChanges.Clear();
			}
		}

		/// <summary>
		/// Saves the data.
		/// </summary>
		[Obsolete("Use Save(UserId) to record history.")]
		public override void Save()
		{
			base.Save();
		}

		private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
		{
			//Store the change.
			object oldVal = ((ProjectSettings)sender)[e.SettingName];
			object newVal = e.NewValue;
			//Only add it if the two values are not equal.
			if (!oldVal.Equals(newVal))
			{
				SettingChangeRecord newSet = new SettingChangeRecord()
				{
					OldValue = oldVal,
					NewValue = newVal
				};
				latestChanges.Add(e.SettingName, newSet);
			}
		}

		#region Project-level feature flags

		/// <summary>
		/// General flag that tells us whether we should roll-up data for the current project
		/// </summary>
		public bool RollupCalculationsDisabled
		{
			get
			{
				//First see if the overall setting is configured
				if (Common.ConfigurationSettings.Default.General_DisableRollupCalculations)
				{
					//If disabled instance-wide, then it's disabled
					return true;
				}
				else
				{
					//Otherwise the project-level setting takes control
					return this.General_DisableRollupCalculations;
				}
			}
		}

		#endregion

		#region Helper Class
		/// <summary>
		/// Allows storage of old, new values for a key.
		/// </summary>
		private class SettingChangeRecord
		{
			/// <summary>
			/// The origjnal value, before the change.
			/// </summary>
			public object OldValue { get; set; }

			/// <summary>
			/// The new value we are setting it to.
			/// </summary>
			public object NewValue { get; set; }
		}
		#endregion Helper Class
	}
}
