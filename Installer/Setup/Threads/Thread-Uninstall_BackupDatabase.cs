using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Backs up the database.</summary>
		private bool Uninstall_BackupDatabase(StreamWriter streamWriter, string dbBackupPrepend, bool required = false)
		{
			streamWriter.WriteLine("Uninstall_BackupDatabase - Starting");

			int TaskDisplayLine = 14;
			ProgressUpdate(
				this,
				new ProgressArgs()
				{
					Progress = ++curStep / numSteps,
					Status = ItemProgress.ProcessStatusEnum.Processing,
					TaskNum = TaskDisplayLine
				});

			try
			{
				streamWriter.WriteLine("Backing up the database to:");

				//Now drop it.
				//Get the various properties
				string dbServer = App._installationOptions.SQLServerAndInstance;
				AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbName = App._installationOptions.DatabaseName;
				string dbUser = App._installationOptions.SQLInstallLogin;
				string dbPassword = App._installationOptions.SQLInstallPassword;
				string companyName = App._installationOptions.Organization;
				string licenseKey = App._installationOptions.LicenseKey;
				string productType = App._installationOptions.ProductName;
				string themeName = Themes.Inflectra.Resources.Global_ThemeName;

				//Gerenate the DB Connection
				DBConnection conninfo = new DBConnection
				{
					DatabaseName = dbName,
					DatabaseServer = dbServer,
					LoginAuthType = dbAuthType,
					LoginPassword = dbPassword,
					LoginUser = dbUser
				};

				//Generate the name of the database.
				string fileName = GenerateBackupName(dbBackupPrepend);

				//First we need to establish a connection with the database
				using (SqlConnection connection = new SqlConnection())
				{
					//Handle the cases of Windows/SQL authentication
					connection.ConnectionString = SQLUtilities.GenerateConnectionString(conninfo);
					connection.Open();

					//Now we need to delete any existing license information
					using (SqlCommand command = new SqlCommand())
					{
						//The string for our command to run.
						string dbCommand = "";

						//Get a defined DBBackup dir first.
						string dbBackup = App._installationOptions.DBBackupDirectory;
						if (!string.IsNullOrWhiteSpace(App._installationOptions?.CommandLine?.DBBackupDir))
							dbBackup = App._installationOptions.CommandLine.DBBackupDir;
						if (!string.IsNullOrWhiteSpace(dbBackup))
							fileName = Path.Combine(dbBackup, fileName);
						streamWriter.WriteLine("Database backup in: " + fileName);

						//We need to retrieve the location of the default backup location, first.

						if (string.IsNullOrWhiteSpace(dbBackup))
						{
							//This is amazing. You can query the SQL Server for Registry keys.
							dbCommand =
@"DECLARE @path NVARCHAR(4000);
EXEC master.dbo.xp_instance_regread
N'HKEY_LOCAL_MACHINE',
N'Software\Microsoft\MSSQLServer\MSSQLServer',N'BackupDirectory',
@path OUTPUT;
SET @path += '\" + fileName + ".bak';";
						}
						else
						{
							dbCommand = @"DECLARE @path NVARCHAR(4000); SET @path = '" + fileName + ".bak';";
						}

						//Generate the database backup SQL..
						dbCommand += Environment.NewLine;
						dbCommand += "BACKUP DATABASE [" + dbName + "] ";             //Backup the database
						dbCommand += "TO DISK=@path ";                               // to the hard drive
						dbCommand += "WITH NO_COMPRESSION, ";                        // without compression
						dbCommand += "DESCRIPTION='Application Uninstall Backup', "; // give it a descriptive, in case
						dbCommand += "INIT, FORMAT, ";                               // create a new Backup set,
						dbCommand += "CHECKSUM, STOP_ON_ERROR;";                     // make sure data is checked and verified.

						//Set up the command.
						command.Connection = connection;
						command.CommandType = CommandType.Text;
						command.CommandText = dbCommand;
						command.CommandTimeout = 7200; //Give it two hours, max.
						connection.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs evt)
						{
							if (!string.IsNullOrWhiteSpace(evt.Message))
							{
								streamWriter.Write(evt.Message);
								//Get the percentage to fire a UI update.
								//Get the first word.
								string prct = evt.Message.Split(' ')[0];
								int pct;
								if (int.TryParse(prct, out pct))
								{
									if (pct < 100 && pct > 0)
									{
										ProgressUpdate(
											this,
											new ProgressArgs()
											{
												Progress = pct / 100f,
												Status = ItemProgress.ProcessStatusEnum.Processing,
												TaskNum = TaskDisplayLine
											});
									}
								}
							}
						};

						//Execute the command.
						command.ExecuteNonQuery();

						//Add a newline, since our output does not.
						streamWriter.WriteLine("");
					}

					//Close the connection
					connection.Close();
				}

				streamWriter.WriteLine("Uninstall_BackupDatabase - Finished");
				ProgressUpdate(
					this,
					new ProgressArgs()
					{
						ErrorText = "",
						Progress = curStep / numSteps,
						Status = ItemProgress.ProcessStatusEnum.Success,
						TaskNum = TaskDisplayLine
					}
				);
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				string msg = "Error creating database backup. Database will not be modified/dropped." + Environment.NewLine;
				streamWriter.WriteLine(msg + Logger.DecodeException(ex));

				streamWriter.WriteLine("Uninstall_BackupDatabase - Finished");
				string errorMsg = msg + ex.Message;
				ProgressUpdate(
					this,
					new ProgressArgs()
					{
						ErrorText = errorMsg,
						Progress = -1,
						Status = ItemProgress.ProcessStatusEnum.Error,
						TaskNum = TaskDisplayLine
					});

				if (required)
					return false;
				else
					return true;
			}
		}

		/// <summary>Generates the name for the database backup.</summary>
		/// <param name="append">Any value to append to the end of the name, before the datetime.</param>
		/// <returns>The name of the DB file, without the extension.</returns>
		private string GenerateBackupName(string append)
		{
			//The name of the database.
			string retvalue = App._installationOptions.DatabaseName;

			//Now append anything needed.
			if (!string.IsNullOrWhiteSpace(append))
				retvalue += "_" + append;

			//Now add the current date/time (mmddyyhhmmss)
			retvalue += "_" + DateTime.UtcNow.ToString("MMddyyHHmmss");

			return retvalue;
		}
	}
}
