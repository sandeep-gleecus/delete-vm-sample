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
		/// <summary>Deletes the database.</summary>
		private bool Uninstall_DeleteDatabase(StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Uninstall_DeleteDatabase - Starting");

			int TaskDisplayLine = 15;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });

			try
			{
				streamWriter.WriteLine("Deleting the database:");

				//Get the various properties
				string dbServer = App._installationOptions.SQLServerAndInstance;
				AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbName = App._installationOptions.DatabaseName;
				string auditdbName = App._installationOptions.AuditDatabaseName;
				string dbUser = App._installationOptions.SQLInstallLogin;
				string dbPassword = App._installationOptions.SQLInstallPassword;
				string companyName = App._installationOptions.Organization;
				string licenseKey = App._installationOptions.LicenseKey;
				string productType = App._installationOptions.ProductName;
				string themeName = Themes.Inflectra.Resources.Global_ThemeName;

				//First we need to establish a connection with the database
				using (SqlConnection connection = new SqlConnection())
				{
					Console.Write("Connection" + connection);
					//Handle the cases of Windows/SQL authentication
					connection.ConnectionString = SQLUtilities.GenerateConnectionString("master", dbServer, dbAuthType, dbUser, dbPassword);
					connection.Open();
					Console.Write("Connection----");
					//Now we need to delete any existing license information
					using (SqlCommand command = new SqlCommand())
					{
						//The string for our command to run.
						//Turn off any automatic stats syncing.
						string dbCommand =
@"ALTER DATABASE [" + dbName + @"]
SET AUTO_UPDATE_STATISTICS_ASYNC OFF;
ALTER DATABASE [" + dbName + @"]
SET AUTO_UPDATE_STATISTICS OFF;";
						try
						{
							//First need to set the DB to single-user mode.
							dbCommand += Environment.NewLine +
	@"ALTER DATABASE [" + dbName + @"] 
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE;";
						}
						catch(Exception ex)
						{
							var msg = ex.Message;
							Console.Write("Exception" + ex);
						}
						

						//The command to drop the database.
						Console.Write(dbName + " Delete");
						dbCommand += Environment.NewLine + @"DROP DATABASE [" + dbName + "];";
						Console.Write(auditdbName + " Delete");
						dbCommand += Environment.NewLine + @"DROP DATABASE [" + auditdbName + "];";

						//Set up the command.
						command.Connection = connection;
						command.CommandType = CommandType.Text;
						command.CommandText = dbCommand;
						connection.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs evt)
						{
							streamWriter.Write(evt.Message);
							Console.Write(evt.Message);
						};

						//Execute the command.
						command.ExecuteNonQuery();
					}

					//Close the connection
					connection.Close();
				}

				streamWriter.WriteLine("Uninstall_DeleteDatabase - Finished");
				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				string msg = "Unable to remove the database: " + Environment.NewLine;
				//Log error.
				streamWriter.WriteLine(msg + Logger.DecodeException(ex));

				streamWriter.WriteLine("Uninstall_DeleteDatabase - Finished");

				string ErrorMsg = msg + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}

		private void cmd_Output(object sender, SqlInfoMessageEventArgs e)
		{

		}
	}
}
