using Inflectra.SpiraTest.Installer.ControlForm;
using System;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.UI
{
	public partial class cntrlResults : UserControl, IProcedureComponent
	{
		public cntrlResults()
		{
			InitializeComponent();
			Loaded += cntrlResults_Loaded;
		}

		#region IProcedureComponent Members

		public string KeyText => Themes.Inflectra.Resources.Results_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlResults";
		public bool AllowBack => true;
		public bool AllowNext => true;
		public bool IsAvailable => true;

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid() => true;
		#endregion

		private void cntrlResults_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			string action = Themes.Inflectra.Resources.Global_Installation;
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.DatabaseUpgrade ||
				App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade)
				action = Themes.Inflectra.Resources.Install_Type_Upgrade;

			if (App.FinishArgs.Success)
			{
				txbFinishStatus.Text = string.Format(Themes.Inflectra.Resources.InstallResult_Message_SuccessTitle, null, action);
				txbFinishMessage.Text = string.Format(Themes.Inflectra.Resources.Results_Message, App._installationOptions.ProductName);
			}
			else
			{
				txbFinishStatus.Text = string.Format(Themes.Inflectra.Resources.InstallResult_Message_ErrorTitle, null, action);
				txbFinishMessage.Text =
					App.FinishArgs.ExtraMessage +
					Environment.NewLine +
					Environment.NewLine +
					Themes.Inflectra.Resources.Global_LogLocation;
			}
		}
	}
}
