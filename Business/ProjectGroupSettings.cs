using System.Configuration;
namespace Inflectra.SpiraTest.Business {


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	[SettingsProvider(typeof(ProjectGroupSettingsProvider))]
	public sealed partial class ProjectGroupSettings {

		/// <summary>
		/// Used by .NET to instantiate the default instance, which should not be used
		/// </summary>
		public ProjectGroupSettings() {
        }

		/// <summary>
		/// The constructor that should be used when using the project group settings
		/// </summary>
		/// <param name="projectGroupId">The id of the project group these settings are for</param>
		public ProjectGroupSettings(int projectGroupId)
		{
			//Store the project group in context
			Context.Add("ProjectGroupId", projectGroupId);
		}

		private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
