using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Inflectra.SpiraTest.Installer.ControlForm;
using Inflectra.SpiraTest.Installer.HelperClasses;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for cntrlDatabaseServer.xaml</summary>
	public partial class cntrlDatabaseServer : UserControl, IProcedureComponent
	{
		const string INSTANCE_SUFFIX = "VMASTER";
		const string DEFAULT_SQL_ADMIN_LOGIN = "sa";

		public cntrlDatabaseServer()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members
		public string KeyText => Themes.Inflectra.Resources.DatabaseServer_KeyText;
		public bool IsLinkable => false;
		public string UniqueName => "cntrlDatabaseServer";
		public bool AllowBack => true;
		public bool AllowNext => true;

		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable
		{
			get
			{
				//Only available on CleanInstall, Database Upgrade & AddAplication
				return (App._installationOptions.InstallationType == InstallationTypeOption.AddApplication ||
					App._installationOptions.InstallationType == HelperClasses.InstallationTypeOption.DatabaseUpgrade ||
					App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall);
			}
		}

		/// <summary>Can we proceed to the next step in the wizard</summary>
		public bool IsValid()
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				//First test the basic connection settings
				bool success = TestConnection();
				if (!success) return false;

				//Next see if the database exists
				bool dbExists = false;
				string dbLogin = txbLogin.Text.Trim();
				string dbPassword = txbPassword.Password.Trim();
				string dbServer = txbServer.Text.Trim(new char[] { '\\', '/', ' ', '\t', '\n', '\r' });
				string dbName = txbDatabaseName.Text.Trim();
				string dbNewLogin = txbNewLogin.Text.Trim();

				//Handle SQL Server and Windows Authentication
				AuthenticationMode dbAuth = (radSQLAuth.IsChecked.Value) ? AuthenticationMode.SqlServer : AuthenticationMode.Windows;
				string connectionString = SQLUtilities.GenerateConnectionString("master", dbServer, dbAuth, dbLogin, dbPassword);

				try
				{
					using (SqlConnection connection = new SqlConnection())
					{
						//First test that we can connect in general
						connection.ConnectionString = connectionString;
						connection.Open();

						//See if the database already exists
						using (SqlCommand sqlCommand = new SqlCommand())
						{
							sqlCommand.Connection = connection;
							sqlCommand.CommandText = "SELECT COUNT(*) FROM sysdatabases AS DbCount WHERE name = '" + SQLUtilities.SqlEncode(dbName) + "'";
							sqlCommand.CommandType = CommandType.Text;
							object result = sqlCommand.ExecuteScalar();
							if (result != null)
								dbExists = ((int)result > 0);
						}

						connection.Close();
					}
				}
				catch (SqlException sqlException)
				{
					//Try and give more meaningful errors
					int errorCode = -1;
					int errorClass = -1;
					if (sqlException.Errors != null && sqlException.Errors.Count > 0)
					{
						errorCode = sqlException.Errors[0].Number;
						errorClass = sqlException.Errors[0].Class;
					}
					if (errorCode == -1 && errorClass == Constants.SQL_ERROR_INSTANCE_NOT_FOUND)
					{
						MessageBox.Show(
							string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailureServerInstanceNotFound, sqlException.Message).FixNewLineResource(),
							Themes.Inflectra.Resources.Global_Failure,
							MessageBoxButton.OK,
							MessageBoxImage.Exclamation);
						return false;
					}
					else if (errorCode == Constants.SQL_ERROR_LOGIN_FAILED)
					{
						MessageBox.Show(
							string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailureLoginPassword, sqlException.Message).FixNewLineResource(),
							Themes.Inflectra.Resources.Global_Failure,
							MessageBoxButton.OK,
							MessageBoxImage.Exclamation);
						return false;
					}
					else
					{
						//Give a more general error
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailure, sqlException.Message), Themes.Inflectra.Resources.Global_Failure, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}
				}
				catch (Exception exception)
				{
					//Give a more general error
					MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailure, exception.Message), Themes.Inflectra.Resources.Global_Failure, MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return false;
				}

				//The next checks will depend on the installation option
				if (App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall)
				{
					//For a clean install the database must not already exist
					if (dbExists)
					{
						string msgStr = App._installationOptions.IsAdvancedInstall ?
							Themes.Inflectra.Resources.DatabaseServer_DatabaseNameAlreadyExists :
							Themes.Inflectra.Resources.DatabaseServer_DatabaseNameAlreadyExists_Simple;

						MessageBox.Show(
							string.Format(msgStr, dbName),
							Themes.Inflectra.Resources.Global_ValidationError,
							MessageBoxButton.OK,
							MessageBoxImage.Exclamation);
						return false;
					}

					//Next we need to check that they are using a login that will conflict with the one we will be trying to create
					//Does not apply to Windows auth
					if (radSQLAuth.IsChecked.Value && dbNewLogin.ToLowerInvariant() == dbLogin.ToLowerInvariant())
					{
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_CannotUseNewLoginConflicts, dbNewLogin), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}
				}
				else if (App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade ||
					App._installationOptions.InstallationType == InstallationTypeOption.Upgrade ||
					App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
				{
					//For an uninstall or upgrade the database needs to already exists
					if (!dbExists)
					{
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_DatabaseNameDoesNotExist, dbName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}

					//Next we need to check that they are using a login that will conflict with the one we will be trying to create
					//Does not apply to Windows auth
					if (radSQLAuth.IsChecked.Value && dbNewLogin.ToLowerInvariant() == dbLogin.ToLowerInvariant())
					{
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_CannotUseNewLoginConflicts, dbNewLogin), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}
				}

				//For an upgrade, the database version must be correct. We can only upgrade v5.x systems
				if (App._installationOptions.InstallationType == InstallationTypeOption.DatabaseUpgrade || App._installationOptions.InstallationType == InstallationTypeOption.Upgrade)
				{
					connectionString = SQLUtilities.GenerateConnectionString(dbName, dbServer, dbAuth, dbLogin, dbPassword);

					using (SqlConnection connection = new SqlConnection())
					{
						//First test that we can connect in general
						connection.ConnectionString = connectionString;
						connection.Open();

						//See if the database version matches
						string databaseRevision = "";
						using (SqlCommand sqlCommand = new SqlCommand())
						{
							sqlCommand.Connection = connection;
							sqlCommand.CommandText = "SELECT VALUE FROM TST_GLOBAL_SETTING WHERE NAME = 'Database_Revision'";
							sqlCommand.CommandType = CommandType.Text;
							object result = sqlCommand.ExecuteScalar();
							if (result != null)
							{
								databaseRevision = (string)result;
							}
						}

						connection.Close();

						//if (databaseRevision.Trim() != Properties.Settings.Default.DatabaseVersionForUpgradingFrom)
						//{
						//	MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_InvalidVersionofDatabaseSchema, dbName, App._installationOptions.ProductName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						//	return false;
						//}
					}
				}

				if (App._installationOptions.InstallationType == InstallationTypeOption.AddApplication)
				{
					//For adding an application, the database name needs to exist
					if (!dbExists)
					{
						MessageBox.Show(string.Format(Themes.Inflectra.Resources.DatabaseServer_DatabaseNameDoesNotExist, dbName), Themes.Inflectra.Resources.Global_ValidationError, MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return false;
					}
				}

				//If using Windows Authentication and a non-local server, warn about application pool, still return TRUE
				if (radWindowsAuth.IsChecked.Value)
				{
					string localServer = Environment.MachineName;
					string[] nameAndInstance = txbServer.Text.Split('\\');
					if (nameAndInstance[0].ToLowerInvariant() != localServer.ToLowerInvariant() && nameAndInstance[0] != ".")
					{
						MessageBox.Show(Themes.Inflectra.Resources.DatabaseServer_ConnectSuccessWithAppPoolWarning, Themes.Inflectra.Resources.Global_Success, MessageBoxButton.OK, MessageBoxImage.Warning);
					}
				}

				return true;
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}
		#endregion

		/// <summary>Called when the user tries to connect to the database</summary>
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				bool success = TestConnection();
				if (success)
				{
					//If using Windows Authentication and a non-local server, warn about application pool
					if (radWindowsAuth.IsChecked.Value)
					{
						string localServer = Environment.MachineName;
						string[] nameAndInstance = txbServer.Text.Split('\\');
						if (nameAndInstance[0].ToLowerInvariant() == localServer.ToLowerInvariant() || nameAndInstance[0] == ".")
						{
							MessageBox.Show(Themes.Inflectra.Resources.DatabaseServer_ConnectSuccess, Themes.Inflectra.Resources.Global_Success, MessageBoxButton.OK, MessageBoxImage.Information);
						}
						else
						{
							MessageBox.Show(Themes.Inflectra.Resources.DatabaseServer_ConnectSuccessWithAppPoolWarning, Themes.Inflectra.Resources.Global_Success, MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
					else
					{
						MessageBox.Show(Themes.Inflectra.Resources.DatabaseServer_ConnectSuccess, Themes.Inflectra.Resources.Global_Success, MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
				//The fail messages are handled inside the TestConnection method itself
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		/// <summary>Tests that the database server can even be reached.</summary>
		private bool TestConnection()
		{
			//Test the connection
			string dbLogin = txbLogin.Text.Trim();
			string dbPassword = txbPassword.Password.Trim();
			string dbServer = txbServer.Text.Trim(new char[] { '\\', '/', ' ', '\t' });

			//Make sure we have the appropriate values
			if (string.IsNullOrEmpty(dbServer))
			{
				MessageBox.Show(
					Themes.Inflectra.Resources.DatabaseServer_ServerRequired,
					Themes.Inflectra.Resources.Global_Failure,
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				return false;
			}

			//Make sure they don't have a trailing backslash in the database server
			if (dbServer.EndsWith("\\"))
			{
				MessageBox.Show(
					Themes.Inflectra.Resources.DatabaseServer_ServerNoTrailingSlash,
					Themes.Inflectra.Resources.Global_Failure,
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				return false;
			}

			//Handle SQL Server and Windows Authentication
			string connectionString;
			if (radSQLAuth.IsChecked.Value)
			{
				if (string.IsNullOrEmpty(dbLogin))
				{
					MessageBox.Show(
						Themes.Inflectra.Resources.DatabaseServer_SQLLoginRequired,
						Themes.Inflectra.Resources.Global_Failure,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);
					return false;
				}

				//Make sure that there are no semicolons in the password (used to delimit connection strings)
				if (dbPassword.Contains(";"))
				{
					MessageBox.Show(
						Themes.Inflectra.Resources.DatabaseServer_PasswordNoSemiColons,
						Themes.Inflectra.Resources.Global_Failure,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);
					return false;
				}

				connectionString = "Password=" + dbPassword + ";User ID=" + dbLogin + ";Data Source=" + dbServer + ";";
			}
			else
			{
				connectionString = "Data Source=" + dbServer + ";Trusted_Connection=True;";
			}

			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					//First test that we can connect in general
					connection.ConnectionString = connectionString;
					connection.Open();
					connection.Close();

					//The next two checks require that we are connected to the master database
					connectionString += "database=master;";
					connection.ConnectionString = connectionString;
					connection.Open();

					//Tests that the current user has permissions to create database and logins (i.e. is db_owner of master database)
					//Not needed if just connecting the application server to an existing database
					if (App._installationOptions.InstallationType != InstallationTypeOption.AddApplication)
					{
						using (SqlCommand sqlCommand = new SqlCommand())
						{
							sqlCommand.Connection = connection;
							sqlCommand.CommandText = "SELECT IS_MEMBER('" + Constants.REQUIRED_DB_ROLE + "') AS DbOwner";
							sqlCommand.CommandType = CommandType.Text;
							object result = sqlCommand.ExecuteScalar();
							if (result == null)
							{
								MessageBox.Show(
									Themes.Inflectra.Resources.DatabaseServer_UnableToDetectDatabaseRole,
									Themes.Inflectra.Resources.Global_Failure,
									MessageBoxButton.OK,
									MessageBoxImage.Exclamation);
								return false;
							}
							int isOwner = (int)result;

							//Tests that the current user has permissions to create database and logins (i.e. is db_owner of master database)
							if (isOwner != 1)
							{
								MessageBox.Show(
									Themes.Inflectra.Resources.DatabaseServer_LoginNotSysAdmin,
									Themes.Inflectra.Resources.Global_Failure,
									MessageBoxButton.OK,
									MessageBoxImage.Exclamation);
								return false;
							}
						}
					}

					//Tests that the version of SQL Server is recent enough
					using (SqlCommand sqlCommand = new SqlCommand())
					{
						sqlCommand.Connection = connection;
						sqlCommand.CommandText = "SELECT SERVERPROPERTY('ProductVersion') AS ProductVersion";
						sqlCommand.CommandType = CommandType.Text;
						object result = sqlCommand.ExecuteScalar();
						if (result == null)
						{
							MessageBox.Show(
								Themes.Inflectra.Resources.DatabaseServer_UnableToDetectDatabaseVersion,
								Themes.Inflectra.Resources.Global_Failure,
								MessageBoxButton.OK,
								MessageBoxImage.Exclamation);
							return false;
						}
						string dbVersion = (string)result;
						string[] versionParts = dbVersion.Split('.');
						if (versionParts.Length == 0)
						{
							MessageBox.Show(
								Themes.Inflectra.Resources.DatabaseServer_UnableToDetectDatabaseVersion,
								Themes.Inflectra.Resources.Global_Failure,
								MessageBoxButton.OK,
								MessageBoxImage.Exclamation);
							return false;
						}

						//Tests that the database version is recent enough
						int dbMajorVersion = int.Parse(versionParts[0]);
						if (dbMajorVersion < Constants.REQUIRED_SQL_VERSION)
						{
							MessageBox.Show(
								string.Format(Themes.Inflectra.Resources.DatabaseServer_VersionTooOld, Constants.REQUIRED_SQL_VERSION_NAME),
								Themes.Inflectra.Resources.Global_Failure,
								MessageBoxButton.OK,
								MessageBoxImage.Exclamation);
							return false;
						}
					}

					connection.Close();
				}
				return true;
			}
			catch (SqlException sqlException)
			{
				//Try and give more meaningful errors
				int errorCode = -1;
				int errorClass = -1;
				if (sqlException.Errors != null && sqlException.Errors.Count > 0)
				{
					errorCode = sqlException.Errors[0].Number;
					errorClass = sqlException.Errors[0].Class;
				}
				if (errorCode == -1 && errorClass == Constants.SQL_ERROR_INSTANCE_NOT_FOUND)
				{
					MessageBox.Show(
						string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailureServerInstanceNotFound, sqlException.Message).FixNewLineResource(),
						Themes.Inflectra.Resources.Global_Failure,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);
					return false;
				}
				else if (errorCode == Constants.SQL_ERROR_LOGIN_FAILED)
				{
					MessageBox.Show(
						string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailureLoginPassword, sqlException.Message).FixNewLineResource(),
						Themes.Inflectra.Resources.Global_Failure,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);
					return false;
				}
				else
				{
					//Give a more general error
					MessageBox.Show(
						string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailure, sqlException.Message),
						Themes.Inflectra.Resources.Global_Failure,
						MessageBoxButton.OK,
						MessageBoxImage.Exclamation);
					return false;
				}
			}
			catch (Exception exception)
			{
				//Give a more general error
				MessageBox.Show(
					string.Format(Themes.Inflectra.Resources.DatabaseServer_ConnectFailure, exception.Message),
					Themes.Inflectra.Resources.Global_Failure,
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				return false;
			}
		}

		/// <summary>Called when the page is unloaded, save settings</summary>
		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			SaveDatabaseSettings();
		}

		/// <summary>Called when the page is first loaded</summary>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			//Set up screen options, first.
			// - Login Type
			radSQLAuth.Visibility = App._installationOptions.IsAdvancedInstall ? Visibility.Visible : Visibility.Collapsed;

			//Grids
			GridLength newVal = App._installationOptions.IsAdvancedInstall ?
				GridLength.Auto :
				new GridLength(0);
			grdDBSettings.RowDefinitions[0].Height = newVal; //SQL Server Login
			grdDBSettings.RowDefinitions[1].Height = newVal; //Database Name
			grdDBSettings.RowDefinitions[2].Height = newVal; //Database User
			grdConnection.RowDefinitions[2].Height = newVal; //Install Login
			grdConnection.RowDefinitions[3].Height = newVal; //Install Password

			//Now load data.
			LoadDatabaseSettings();
			radWindowsAuth_Checked(null, null);
		}

		/// <summary>Loads the database settings from the installer options, if we have any set</summary>
		/// <remarks>For upgrade cases, need to read them from the Web.config file if not set</remarks>
		private void LoadDatabaseSettings()
		{
			if (App._installationOptions.DatabaseAuthentication == AuthenticationMode.Unknown)
			{
				if (App._installationOptions.InstallationType == InstallationTypeOption.Upgrade || App._installationOptions.InstallationType == InstallationTypeOption.Uninstall)
				{
					//If upgrade/uninstall case, read from config file
					string webConfigFile = Path.Combine(App._installationOptions.InstallationFolder, Constants.WEB_CONFIG);
					ReadWebConfig.LoadSettingsFromWebConfig(webConfigFile);
					App._installationOptions.DataSyncFile = DataSyncConfig.LoadSettingsFromFile(Path.Combine(App._installationOptions.InstallationFolder, "DataSync", Constants.DATASYNC_CONFIG));
				}
				else
				{
					//Otherwise just use sensible defaults
					string localServer = "VMASTER";
					txbServer.Text = localServer;// + INSTANCE_SUFFIX;
					chkSampleData.IsChecked = true;
					radWindowsAuth.IsChecked = true;
					txbLogin.Text = "";
					txbPassword.Password = "";
					txbNewLogin.Text = Constants.WINDOWS_AUTH_LOGIN;
					txbDatabaseName.Text = App._installationOptions.ProductName;
					txbDatabaseUser.Text = @"VMASTER\master"; // App._installationOptions.ProductName;
					txbLogin.IsEnabled = false;
					txbPassword.IsEnabled = false;
					txbNewLogin.IsEnabled = false;
				}
			}
			else
			{
				txbServer.Text = App._installationOptions.SQLServerAndInstance;
				chkSampleData.IsChecked = App._installationOptions.DatabaseSampleData;

				//See the auth method
				if (App._installationOptions.DatabaseAuthentication == AuthenticationMode.Windows)
				{
					//Windows
					radWindowsAuth.IsChecked = true;
					txbLogin.IsEnabled = false;
					txbPassword.IsEnabled = false;
					txbNewLogin.IsEnabled = false;
					txbLogin.Text = "";
					txbPassword.Password = "";
					txbNewLogin.Text = Constants.WINDOWS_AUTH_LOGIN;
					txbDatabaseName.Text = App._installationOptions.DatabaseName;
					txbDatabaseUser.Text = @"VMASTER\master"; // App._installationOptions.DatabaseUser;
				}
				else
				{
					//SQL Server
					radSQLAuth.IsChecked = true;
					txbLogin.IsEnabled = true;
					txbPassword.IsEnabled = true;
					txbNewLogin.IsEnabled = true;
					txbLogin.Text = App._installationOptions.SQLInstallLogin;
					txbPassword.Password = App._installationOptions.SQLInstallPassword;
					txbNewLogin.Text = App._installationOptions.DatabaseUser;
					txbDatabaseName.Text = App._installationOptions.DatabaseName;
					txbDatabaseUser.Text = @"VMASTER\master"; // App._installationOptions.DatabaseUser;
				}
			}

			//If not a clean install, hide sample data option
			if (App._installationOptions.InstallationType == InstallationTypeOption.CleanInstall)
				chkSampleData.Visibility = Visibility.Visible;
			else
				chkSampleData.Visibility = Visibility.Collapsed;

			//add application, hide the new login name (SQL Auth only)
			if (App._installationOptions.InstallationType == InstallationTypeOption.AddApplication)
				txbNewLogin.Visibility = Visibility.Collapsed;
			else
				txbNewLogin.Visibility = Visibility.Visible;
		}

		/// <summary>Saves the database settings to the installer options, if we have any set</summary>
		private void SaveDatabaseSettings()
		{
			App._installationOptions.SQLServerAndInstance = txbServer.Text.Trim(new char[] { '\\', '/', ' ', '\t' });
			if (chkSampleData.IsChecked.HasValue)
			{
				App._installationOptions.DatabaseSampleData = chkSampleData.IsChecked.Value;
			}

			//See the auth method we are using
			if (radWindowsAuth.IsChecked.Value)
			{
				//Windows
				App._installationOptions.DatabaseAuthentication = AuthenticationMode.Windows;
				App._installationOptions.SQLInstallLogin = "";
				App._installationOptions.SQLInstallPassword = "";
				App._installationOptions.DatabaseUser = "";
			}
			else
			{
				//SQL Server
				App._installationOptions.DatabaseAuthentication = AuthenticationMode.SqlServer;
				App._installationOptions.SQLInstallLogin = txbLogin.Text.Trim();
				App._installationOptions.SQLInstallPassword = txbPassword.Password.Trim();
				App._installationOptions.DatabaseUser = txbNewLogin.Text.Trim();
			}

			App._installationOptions.DatabaseName = txbDatabaseName.Text.Trim();
			App._installationOptions.DatabaseUser = @"VMASTER\master"; // txbDatabaseUser.Text.Trim();
		}

		/// <summary>Switches to Windows Authentication</summary>
		private void radWindowsAuth_Checked(object sender, RoutedEventArgs e)
		{
			if (txbLogin != null)
			{
				txbLogin.IsEnabled = false;
				txbLogin.Text = "";
			}
			if (txbPassword != null)
			{
				txbPassword.IsEnabled = false;
				txbPassword.Password = "";
			}
			if (txbNewLogin != null)
			{
				txbNewLogin.IsEnabled = false;
				txbNewLogin.Text = Constants.WINDOWS_AUTH_LOGIN;
			}
		}

		/// <summary>Switches to SQL Authentication</summary>
		private void radSQLAuth_Checked(object sender, RoutedEventArgs e)
		{
			txbLogin.IsEnabled = true;
			txbPassword.IsEnabled = true;
			txbNewLogin.IsEnabled = true;
			txbLogin.Text = DEFAULT_SQL_ADMIN_LOGIN;
			txbPassword.Password = "";
			txbNewLogin.Text = App._installationOptions.ProductName;
		}
	}
}
