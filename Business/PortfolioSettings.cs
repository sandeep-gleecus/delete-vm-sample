using System.Configuration;
namespace Inflectra.SpiraTest.Business {


	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	[SettingsProvider(typeof(PortfolioSettingsProvider))]
	public sealed partial class PortfolioSettings {

		/// <summary>
		/// Used by .NET to instantiate the default instance, which should not be used
		/// </summary>
		public PortfolioSettings() {
        }

		/// <summary>
		/// The constructor that should be used when using the portfolio settings
		/// </summary>
		/// <param name="portfolioId">The id of the portfolio these settings are for</param>
		public PortfolioSettings(int portfolioId)
		{
			//Store the portfolio in context
			Context.Add("PortfolioId", portfolioId);
		}

		private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
