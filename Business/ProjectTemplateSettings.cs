using System.Configuration;

namespace Inflectra.SpiraTest.Business {


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	[SettingsProvider(typeof(ProjectTemplateSettingsProvider))]
	public sealed partial class ProjectTemplateSettings {

		/// <summary>
		/// Used by .NET to instantiate the default instance, which should not be used
		/// </summary>
		public ProjectTemplateSettings() {
        }

		/// <summary>
		/// The constructor that should be used when using the project template settings
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template these settings are for</param>
		public ProjectTemplateSettings(int projectTemplateId)
		{
			//Store the project template in context
			Context.Add("ProjectTemplateId", projectTemplateId);
		}

		private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
