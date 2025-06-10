using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlChooseLocation.xaml</summary>
	public partial class cntrlChooseLocation : UserControl, IProcedureComponent
	{
		private bool generateFolder = true;
		public cntrlChooseLocation()
		{
			InitializeComponent();

			//Load up the settings from the command-line.
			if (!string.IsNullOrWhiteSpace(App._installationOptions.CommandLine?.InstallDir))
			{
				App._installationOptions.InstallationFolder = App._installationOptions.CommandLine.InstallDir;
				generateFolder = false;
			}

		}

		/// <summary>Loads the settings into the UI</summary>
		public void LoadSettings()
		{
			//Change the text to have the product name
			string intro = Themes.Inflectra.Resources.ChooseLocation_Description;
			if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
				intro = Themes.Inflectra.Resources.ChooseLocation_Description_Upgrade;
			else if (App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
				intro = Themes.Inflectra.Resources.ChooseLocation_Description_Uninstall;
			txtDescription.Text = string.Format(intro, App._installationOptions.ProductName);

			//TODO: Verify default paths for Uninstall and Upgrade.

			//See if we have an upgrade or clean install. For upgrades we use the (x86) folder as the default location
			//since that's where older versions of Spira installed
			if (string.IsNullOrEmpty(txbFolderPath.Text))
			{
				//Set the default location.
				if (generateFolder)
					txbFolderPath.Text = Path.Combine(App._installationOptions.InstallationFolder, App._installationOptions.ProductName);
				else
				{
					txbFolderPath.Text = App._installationOptions.InstallationFolder;
					generateFolder = false;
				}
			}
			else
			{
				txbFolderPath.Text = App._installationOptions.InstallationFolder;
			}
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.ChooseLocation_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlChooseLocation";
		public bool AllowBack => true;
		public bool AllowNext => true;

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//This screen should only be available if:
				// - NewInstall & AddApplication Advanced
				// - Uninstall Normal
				return (((App._installationOptions.InstallationType == InstallationTypeOption.AddApplication ||
					App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall) &&
					App._installationOptions.IsAdvancedInstall) ||
					App._installationOptions.InstallationType == InstallationTypeOption.Uninstall ||
					App._installationOptions.InstallationType == InstallationTypeOption.Upgrade);
			}
		}

		/// <summary>Can we proceed to the next step in the wizard</summary>
		/// <remarks>See the Confirmation panel on work done on Install Path when this panel is not shown.</remarks>
		public bool IsValid()
		{
			//Verify that the location is valid
			string path = txbFolderPath.Text;

			//For fresh installs and add applications, you need the folder to NOT exist, or exist and be empty.
			//Otherwise it needs to exist and have a Web.config file in the folder
			if (App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall ||
				App._installationOptions.InstallationType == InstallationTypeOption.AddApplication)
			{
				if (Directory.Exists(path) && Directory.GetFileSystemEntries(path).Length > 0)
					MessageBox.Show(
						string.Format(Themes.Inflectra.Resources.ChooseLocation_InstallationFolderAlreadyExists, path),
						Themes.Inflectra.Resources.Global_ValidationError,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);

				return true;
			}
			else
			{
				if (Directory.Exists(path))
				{
					string webConfigFile = Path.Combine(path, Constants.WEB_CONFIG);
					if (File.Exists(webConfigFile))
					{
						//Load up out settings.
						WebConfigSettings settings = ReadWebConfig.LoadSettingsFromWebConfig(webConfigFile);
						ReadWebConfig.LoadAppSettingsFromWeb(settings);

						//See if we have settings in either the old location (BIN) or new location (DataSync)
						string dataSyncConfigPath = Path.Combine(path, "DataSync", Constants.DATASYNC_CONFIG);
						if (File.Exists(dataSyncConfigPath))
						{
							App._installationOptions.DataSyncFile = DataSyncConfig.LoadSettingsFromFile(dataSyncConfigPath);
						}
						else
						{
							dataSyncConfigPath = Path.Combine(path, "bin", Constants.DATASYNC_CONFIG);
							if (File.Exists(dataSyncConfigPath))
							{
								App._installationOptions.DataSyncFile = DataSyncConfig.LoadSettingsFromFile(dataSyncConfigPath);
							}
						}

						//If we're reporting v5.4, check the DLL version.
						if (settings.VersionProgram == null || settings.VersionProgram == new Version("5.4.0.0"))
							settings.VersionProgram = Utilities.GetFileVersion(Path.Combine(path, "bin", "web.dll"));

						//Check to see if we are installing over a previous version. Only on an Upgrade.
						if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
						{
							Version instVer = Assembly.GetExecutingAssembly().GetName().Version;

							if (instVer == settings.VersionProgram)
							{
								var ret = MessageBox.Show(
								string.Format(Themes.Inflectra.Resources.Upgrade_SameVersion, Utilities.GetTruncatedVersion(instVer)),
									"Upgrade?",
									MessageBoxButton.YesNo,
									MessageBoxImage.Question,
									MessageBoxResult.Yes);

								return ret == MessageBoxResult.Yes;
							}
							else if (instVer < settings.VersionProgram)
							{
								MessageBox.Show(
									Themes.Inflectra.Resources.Upgrade_NewerVersion,
									"Cannot Upgrade",
									MessageBoxButton.OK,
									MessageBoxImage.Error);
								return false;
							}
							else if (settings.VersionProgram < new Version("5.4.0.0"))
							{
								MessageBox.Show(
									Themes.Inflectra.Resources.Upgrade_WrongVersion,
									"Cannot Upgrade",
									MessageBoxButton.OK,
									MessageBoxImage.Error);
								return false;
							}
							else
								return true;
						}
						else
							return true;
					}
					else
					{
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.ChooseLocation_InstallationFolderNoWebConfig, path, App._installationOptions.ProductName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}
				}
				else
				{
					MessageBox.Show(string.Format(Themes.Inflectra.Resources.ChooseLocation_InstallationFolderDoesNotExist, path), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return false;
				}
			}
		}
		#endregion

		/// <summary>Called when the user clicks the button to choose a folder</summary>
		private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
		{
			//Show the folder selection dialog and let the user select the folder.
			OpenFolderDialog dlg = new OpenFolderDialog
			{
				Description = "Select Installation Path:",
				ShowNewFolderButton = true,
				SelectedPath = txbFolderPath.Text
			};

			if (!string.IsNullOrEmpty(txbFolderPath.Text))
				dlg.SelectedPath = txbFolderPath.Text;

			if (dlg.ShowDialog(Window.GetWindow(this)).Value)
				txbFolderPath.Text = dlg.SelectedPath;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			//Display the settings
			LoadSettings();
		}

		/// <summary>Hit when the user moves to the next page!</summary>
		/// <remarks>See the CONFIRMATION panel for work done to the install path when this panel is not loaded.</remarks>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			//Make sure we save the settings
			App._installationOptions.InstallationFolder = txbFolderPath.Text;

			//Let us load the web.config
			if (App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
			{

			}
		}
	}
}
