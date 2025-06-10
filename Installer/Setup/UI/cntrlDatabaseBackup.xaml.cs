using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlDatabaseBackup.xaml</summary>
	public partial class cntrlDatabaseBackup : UserControl, IProcedureComponent
	{
		public cntrlDatabaseBackup()
		{
			InitializeComponent();
		}

		/// <summary>Loads the settings into the UI</summary>
		public void LoadSettings()
		{
			//Write out the formatted text that is needed.
			txtIntro.Inlines.Clear();
			txtIntro.Inlines.Add(Themes.Inflectra.Resources.DatabaseBackup_Intro1);
			txtIntro.Inlines.Add(new LineBreak());
			txtIntro.Inlines.Add(new Run(Themes.Inflectra.Resources.DatabaseBackup_Intro2) { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic });
			txtIntro.Inlines.Add(": " + Themes.Inflectra.Resources.DatabaseBackup_Intro3);
			txtFileNameSummary.Inlines.Clear();
			txtFileNameSummary.Inlines.Add(Themes.Inflectra.Resources.DatabaseBackup_Filename1);
			txtFileNameSummary.Inlines.Add(new Run(
				string.Format(
					Themes.Inflectra.Resources.DatabaseBackup_Filename2,
					App._installationOptions.DatabaseName
					))
			{ FontStyle = FontStyles.Italic }
			);
			txtFileNameSummary.Inlines.Add(Themes.Inflectra.Resources.DatabaseBackup_Filename3);

			//See if we have a value already specified
			if (string.IsNullOrWhiteSpace(App._installationOptions.DBBackupDirectory))
			{
				if (string.IsNullOrWhiteSpace(App._installationOptions.CommandLine?.DBBackupDir))
				{
					App._installationOptions.DBBackupDirectory = SQLUtilities.GetSQLBackupDir(
						null,
						App._installationOptions.SQLServerAndInstance,
						App._installationOptions.DatabaseAuthentication,
						App._installationOptions.SQLInstallLogin,
						App._installationOptions.SQLInstallPassword);
				}
				else
					App._installationOptions.DBBackupDirectory = App._installationOptions.CommandLine?.DBBackupDir;
			}

			//Strip off the filename, if given.
			if (!string.IsNullOrEmpty(Path.GetExtension(App._installationOptions.DBBackupDirectory)))
				App._installationOptions.DBBackupDirectory =
					Path.GetDirectoryName(App._installationOptions.DBBackupDirectory);

			txbFolderPath.Text = App._installationOptions.DBBackupDirectory;

			//Attach events to the checkbox.
			chkBackup.Unchecked += chkBackup_Changed;
			chkBackup.Checked += chkBackup_Changed;
			chkBackup_Changed(null, null);
		}

		private void chkBackup_Changed(object sender, RoutedEventArgs e)
		{
			//Disable the other controls, based on the checkbox status.
			txbFolderPath.IsEnabled = chkBackup.IsChecked.Value;
			lblFileName.Opacity = (chkBackup.IsChecked.Value) ? 1 : 0.5;
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.DatabaseBackup_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlDatabaseBackup";
		public bool AllowBack => true;
		public bool AllowNext => true;

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//This screen is only available if:
				// - (Upgrade or DatabaseUpgrade or Uninstall) and Advanced
				return (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.DatabaseUpgrade ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Uninstall) &&
					App._installationOptions.IsAdvancedInstall;
			}
		}


		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid()
		{
			//We cannot do any checking here at the moment. Since the DB server may be on another server,
			// there is no (easy and worthwhile) way to check that the path exists, nor if a file by the same
			// name already exists (unlikely, but possible?). So for now - no matter what is entered, we 
			// approve it.
			return true;
		}
		#endregion

		/// <summary>Called when the user clicks the button to choose a folder</summary>
		private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
		{
			//Show the folder selection dialog and let the user select the folder.
			OpenFolderDialog dlg = new OpenFolderDialog
			{
				Description = "Select installation path:",
				ShowNewFolderButton = true,
				SelectedPath = txbFolderPath.Text
			};

			if (!string.IsNullOrEmpty(txbFolderPath.Text))
				dlg.SelectedPath = txbFolderPath.Text;

			if (dlg.ShowDialog(Window.GetWindow(this)).Value)
				txbFolderPath.Text = dlg.SelectedPath;
		}

		/// <summary>Called when the user control is loaded into memory. This loads our existing settings.</summary>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			//Display the settings
			LoadSettings();
		}

		/// <summary>Called when the user control is UNLOADED from memory.</summary>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			//Make sure we save the settings
			string path = txbFolderPath.Text.Trim(new char[] { ' ', '\t', '\\', '\n', '\r' });

			//TODO: Temporary.
			App._installationOptions.DBBackupDirectory = path;

			App._installationOptions.NoBackupDB = !chkBackup.IsChecked.Value;
		}
	}
}