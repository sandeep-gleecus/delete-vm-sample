using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Inflectra.SpiraTest.Installer.ControlForm;

using Microsoft.Web.Administration;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>
	/// Interaction logic for cntrlWebServer.xaml
	/// </summary>
	public partial class cntrlWebServer : UserControl, IProcedureComponent
	{
		const string APP_POOL_SUFFIX = "AppPool";

		#region Helper Classes
		/// <summary>Stores the info about an IIS Application</summary>
		protected class ApplicationInfo
		{
			/// <summary>The display name of the IIS application</summary>
			public string Name { get; set; }
			/// <summary>The path of the IIS application</summary>
			public string Path { get; set; }

			/// <summary>The place on the file system</summary>
			public string Location { get; set; }

			/// <summary>The matching application pool</summary>
			public string AppPool { get; set; }
		}
		#endregion

		public cntrlWebServer()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.WebServer_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlWebServer";
		public bool AllowBack => true;
		public bool AllowNext => true;

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//Only available on installations Clean, AddApplication.
				return (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.AddApplication ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall);
			}
		}

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid()
		{
			//Reset the error flag.
			txbApplicationPool.Tag = null;

			//If a new install or adding an application, make sure they supplied an Application Pool name
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall || App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.AddApplication)
			{
				if (string.IsNullOrEmpty(txbApplicationPool.Text))
				{
					MessageBox.Show(string.Format(Themes.Inflectra.Resources.WebServer_NeedToProvideApplicationPoolName, App._installationOptions.ProductName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					txbApplicationPool.Tag = "1";
					return false;
				}

				//Check to see if the VDIR already exists and is in use (not allowed)
				if (!string.IsNullOrEmpty(txbVirtualDirectory.Text))
				{
					//Load the websites
					try
					{
						ServerManager iisManager = new ServerManager();
						SiteCollection websites = iisManager.Sites;

						//Find the current one
						if (cmbWebSite.SelectedValue is string && !string.IsNullOrEmpty((string)cmbWebSite.SelectedValue))
						{
							string websiteName = (string)cmbWebSite.SelectedValue;
							Site website = websites[websiteName];
							if (website != null)
							{
								//Get the list of applications
								if (website.Applications.Any(f => f.Path.Equals("/" + txbVirtualDirectory.Text.Trim())))
								{
									MessageBox.Show(string.Format(Themes.Inflectra.Resources.WebServer_ApplicationNameAlreadyInUse, App._installationOptions.ProductName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
									txbVirtualDirectory.Tag = "1";
									return false;
								}
							}
						}
					}
					catch (Exception ex)
					{
						string msg = "Could not load IIS Management Console:" + Environment.NewLine + Logger.DecodeException(ex);
						App.logFile.WriteLine(msg);

						MessageBox.Show("IIS Management Console is not installed per the prequequisites require. Install the IIS Management Console, and re-run the installer.",
							"Can Not Continue", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
						// Force an exit.
						System.Windows.Application.Current.Shutdown();
					}
				}
			}

			return true;
		}
		#endregion

		/// <summary>Hit when the user changes the selected Project.</summary>
		/// <param name="sender">cmbProjectList</param>
		/// <param name="e">EventArgs</param>
		private void cmbWebSite_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Load the list of applications
			LoadApplications();
		}

		/// <summary>Called when the control is UNLOADED saves the settings to memory object.</summary>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			//Store the website if set
			if (cmbWebSite.SelectedValue is string && !string.IsNullOrEmpty((string)cmbWebSite.SelectedValue))
			{
				App._installationOptions.IISWebsite = (string)cmbWebSite.SelectedValue;
			}

			//Store the application and application pool
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.AddApplication || App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall)
			{
				App._installationOptions.IISApplication = "/" + txbVirtualDirectory.Text.Trim();
				App._installationOptions.IISApplicationPool = txbApplicationPool.Text.Trim();
			}
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade || App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Uninstall)
			{
				if (cmbVirtualDirectory.SelectedValue is string && !string.IsNullOrEmpty((string)cmbVirtualDirectory.SelectedValue))
				{
					App._installationOptions.IISApplication = ((ApplicationInfo)cmbVirtualDirectory.SelectedItem).Path;
					App._installationOptions.IISApplicationPool = ((ApplicationInfo)cmbVirtualDirectory.SelectedItem).AppPool;
				}
			}
		}

		/// <summary>Show/hide the appropriate fields depending on installation type</summary>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				//If this is the upgrade case and nothing is already chosen for the web site or application
				//try and find the matching ones from the installation location that was chosen
				if ((App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade || App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Uninstall) && string.IsNullOrEmpty(App._installationOptions.IISWebsite))
				{
					//Loop through all of the web sites and all the virtual directories to see
					//if any match the Installation folder location and default to that
					string matchingSite = null;
					string matchingApp = null;
					string matchingAppPool = null;

					if (!string.IsNullOrEmpty(App._installationOptions.InstallationFolder))
					{
						//Load the websites
						try
						{
							ServerManager iisManager = new ServerManager();
							SiteCollection websites = iisManager.Sites;

							foreach (Site siteToCheck in websites)
							{
								foreach (Microsoft.Web.Administration.Application appToCheck in siteToCheck.Applications)
								{
									foreach (VirtualDirectory vdir in appToCheck.VirtualDirectories)
									{
										if (vdir.PhysicalPath.ToLowerInvariant() == App._installationOptions.InstallationFolder)
										{
											matchingSite = siteToCheck.Name;
											matchingApp = vdir.Path;
											matchingAppPool = appToCheck.ApplicationPoolName;
										}
									}
								}
							}

							if (matchingSite != null)
							{
								App._installationOptions.IISWebsite = matchingSite;
								if (matchingApp != null)
								{
									App._installationOptions.IISWebsite = matchingApp;
									App._installationOptions.IISApplicationPool = matchingAppPool;
								}
							}
						}
						catch (Exception ex)
						{
							string msg = "Could not load IIS Management Console:" + Environment.NewLine + Logger.DecodeException(ex);
							App.logFile.WriteLine(msg);

							MessageBox.Show("IIS Management Console is not installed per the prequequisites require. Install the IIS Management Console, and re-run the installer.",
								"Can Not Continue", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
							// Force an exit.
							System.Windows.Application.Current.Shutdown();
						}
					}
				}


				//Load the list of web sites
				LoadWebSites();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}

			//See if we are installing/uninstalling/upgrading and show the right boxes
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.AddApplication || App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.CleanInstall)
			{
				grpNewVirtualDirectory.Visibility = (App._installationOptions.IsAdvancedInstall) ? Visibility.Visible : Visibility.Collapsed;
				grpExistingVirtualDirectory.Visibility = Visibility.Collapsed;

				//If there is no Application specified, use the product name as a suggested
				if (string.IsNullOrEmpty(App._installationOptions.IISApplication) || !App._installationOptions.IsAdvancedInstall)
				{
					txbVirtualDirectory.Text = App._installationOptions.ProductName;
				}
				else
				{
					txbVirtualDirectory.Text = App._installationOptions.IISApplication.Replace("/", "");
				}
				if (string.IsNullOrEmpty(App._installationOptions.IISApplicationPool) || !App._installationOptions.IsAdvancedInstall)
				{
					txbApplicationPool.Text = App._installationOptions.ProductName + APP_POOL_SUFFIX;
				}
				else
				{
					txbApplicationPool.Text = App._installationOptions.IISApplicationPool;
				}
			}
			if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Upgrade)
			{
				grpNewVirtualDirectory.Visibility = Visibility.Collapsed;
				grpExistingVirtualDirectory.Visibility = Visibility.Visible;
				lblVirtualDirectory.Content = Themes.Inflectra.Resources.WebServer_ExistingVirtualDirectoryIntro2;

				//Load the applications as well
				LoadApplications();
			}
			else if (App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.Uninstall)
			{
				grpNewVirtualDirectory.Visibility = Visibility.Collapsed;
				grpExistingVirtualDirectory.Visibility = Visibility.Visible;
				lblVirtualDirectory.Content = Themes.Inflectra.Resources.WebServer_ExistingVirtualDirectoryIntro1;

				//Load the applications as well
				LoadApplications();
			}
		}

		/// <summary>Loads the list of IIS web sites</summary>
		private void LoadWebSites()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				//Load the websites
				ServerManager iisManager = new ServerManager();
				SiteCollection websites = iisManager.Sites;

				//Databind
				cmbWebSite.ItemsSource = websites;

				//See if we have a website selected already
				if (string.IsNullOrEmpty(App._installationOptions.IISWebsite))
				{
					//Choose the one with ID=1 (the default) if exists otherwise leave unselected
					Site defaultWebSite = websites.FirstOrDefault(w => w.Id == 1);
					if (defaultWebSite != null)
					{
						cmbWebSite.SelectedValue = defaultWebSite.Name;
					}
				}
				else
				{
					cmbWebSite.SelectedValue = App._installationOptions.IISWebsite;
				}
			}
			catch (Exception ex)
			{
				string msg = "Could not load IIS Management Console:" + Environment.NewLine + Logger.DecodeException(ex);
				App.logFile.WriteLine(msg);

				MessageBox.Show("IIS Management Console is not installed per the prequequisites require. Install the IIS Management Console, and re-run the installer.",
					"Can Not Continue", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
				// Force an exit.
				System.Windows.Application.Current.Shutdown();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		/// <summary>Loads the list of IIS web site applications (vdirs)</summary>
		private void LoadApplications()
		{
			//Make sure they know we're working.
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				//Load the websites
				ServerManager iisManager = new ServerManager();
				SiteCollection websites = iisManager.Sites;

				//Find the current one
				if (cmbWebSite.SelectedValue is string && !string.IsNullOrEmpty((string)cmbWebSite.SelectedValue))
				{
					string websiteName = (string)cmbWebSite.SelectedValue;
					Site website = websites[websiteName];
					if (website != null)
					{
						//Add the list of applications
						List<ApplicationInfo> applications = new List<ApplicationInfo>();
						foreach (Microsoft.Web.Administration.Application iisApp in website.Applications)
						{
							ApplicationInfo application = new ApplicationInfo();
							if (iisApp.Path == "/")
							{
								application.Name = Themes.Inflectra.Resources.WebServer_WebsiteRoot;
							}
							else
							{
								application.Name = iisApp.Path.Replace("/", "");
							}
							application.Path = iisApp.Path;
							application.Location = iisApp.VirtualDirectories.First().PhysicalPath;
							application.AppPool = iisApp.ApplicationPoolName;
							applications.Add(application);
						}

						//Databind
						cmbVirtualDirectory.ItemsSource = applications;

						//See if we have an application selected already
						if (!string.IsNullOrEmpty(App._installationOptions.IISApplication))
						{
							cmbVirtualDirectory.SelectedValue = App._installationOptions.IISApplication;
						}
					}
				}
			}
			catch (Exception ex)
			{
				string msg = "Could not load IIS Management Console:" + Environment.NewLine + Logger.DecodeException(ex);
				App.logFile.WriteLine(msg);

				MessageBox.Show("IIS Management Console is not installed per the prequequisites require. Install the IIS Management Console, and re-run the installer.",
					"Can Not Continue", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
				// Force an exit.
				System.Windows.Application.Current.Shutdown();
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		/// <summary>If they change the name of the application, change the app pool to match</summary>
		private void txbVirtualDirectory_TextChanged(object sender, TextChangedEventArgs e)
		{
			//Reset the error flag.
			txbApplicationPool.Tag = null;

			if (string.IsNullOrEmpty(txbVirtualDirectory.Text))
			{
				txbApplicationPool.Text = App._installationOptions.ProductName + APP_POOL_SUFFIX;
			}
			else
			{
				txbApplicationPool.Text = txbVirtualDirectory.Text + APP_POOL_SUFFIX;
			}
		}
	}
}
