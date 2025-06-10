using Inflectra.SpiraTest.Installer.ControlForm;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlInstallationType.xaml</summary>
	public partial class cntrlInstallationType : UserControl, IProcedureComponent
	{
		public cntrlInstallationType()
		{
			InitializeComponent();

			//Hook up events to our buttons.
			radDatabaseUpgrade.Click += butAction_Click;
			radAddApplication.Click += butAction_Click;
			radCleanInstall.Click += butAction_Click;
			radUninstall.Click += butAction_Click;
			radUpgrade.Click += butAction_Click;
			chkAdvanced.Click += chkAdvanced_Click;

			radCleanInstall.Background = new SolidColorBrush(Color.FromArgb(255, 253, 203, 38)); // spira yolk

			//Load up command-line options here.
			chkAdvanced.IsChecked = App._installationOptions.IsAdvancedInstall;
		}

		/// <summary>Hit when the 'Advanced' checkbox is clcked. Hides/shows the Advanced options.</summary>
		private void chkAdvanced_Click(object sender, RoutedEventArgs e)
		{
			bool isChecked = chkAdvanced.IsChecked.Value;
			GridLength hgt = ((isChecked) ? GridLength.Auto : new GridLength(0));

			grdMaintenanceOptions.RowDefinitions[1].Height = hgt;
			grdMaintenanceOptions.RowDefinitions[2].Height = hgt;
		}

		/// <summary>Hit when the user decides what action they want to perform.</summary>
		private void butAction_Click(object sender, RoutedEventArgs e)
		{
			//Based on the button clicked, we send them to their next page.
			if (sender is Button)
			{
				Button but = (Button)sender;

				//The page to advance to.
				string toAdv = "";
				switch ((string)but.Tag)
				{
					case "NewInstall":
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.CleanInstall;
						toAdv = "cntrlEula";
						break;
					case "FullUpgrade":
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.Upgrade;
						toAdv = "cntrlEula";
						break;
					case "AddApplication":
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.AddApplication;
						toAdv = "cntrlEula";
						break;
					case "DatabaseUpgrade":
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.DatabaseUpgrade;
                        toAdv = "cntrlDatabaseServer";
                        break;
					case "Uninstall":
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.Uninstall;
						toAdv = "cntrlChooseLocation";
						break;
					default:
						App._installationOptions.InstallationType = HelperClasses.InstallationTypeOption.Uninstall;
						break;
				}
				App._installationOptions.IsAdvancedInstall = chkAdvanced.IsChecked.Value;
				AdvanceFunction(toAdv);
			}
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.InstallationType_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlInstallationType";
		public bool AllowBack => true;
		public bool AllowNext => false;
		public bool IsAvailable
		{
			get
			{
				//It is NOT available if we were passed a command-line option.
				if (App._installationOptions.CommandLine != null &&
					App._installationOptions.CommandLine.InstallType != HelperClasses.InstallationTypeOption.Unknown)
				{
					App._installationOptions.InstallationType = App._installationOptions.CommandLine.InstallType;
					return false;
				}
				else
					return true;
			}
		}

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid() => true;
		#endregion

		/// <summary>The function from the main form that allows us to skip to a specific page (by name)</summary>
		public Action<string> AdvanceFunction { get; set; }

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			chkAdvanced.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{ }
	}
}
