using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Installing the product database onto the database server</summary>
		/// <param name="streamWriter">Used to write out to the logs</param>
		/// <param name="status">The status of the process</param>
		/// <returns>True if successful, False if failure</returns>
		private bool Install_Database(StreamWriter streamWriter, out FinalStatusEnum status)
		{
			int TaskDisplayLine = 2;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Installing the product database onto the database server");

			try
			{
				//First try and drop the database (fail quietly)
				streamWriter.WriteLine("First trying to drop the database if it already exists");
				DropDatabase(true);

				//Get the various properties
				string dbFilePath = Path.Combine(App._installationOptions.InstallationFolder, "Database");
				string dbServer = App._installationOptions.SQLServerAndInstance;
				AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbName = App._installationOptions.DatabaseName;
				string auditdbName = App._installationOptions.DatabaseName + "Audit";
				string auditdbUser = App._installationOptions.SQLInstallLogin;
				string auditdbPassword = App._installationOptions.SQLInstallPassword;
				string auditdbNewLogin = App._installationOptions.DatabaseUser;
				string auditdbNewUser = App._installationOptions.DatabaseUser;
				string auditdbWinLogin = Constants.WINDOWS_AUTH_LOGIN;
				string auditdbServer = App._installationOptions.SQLServerAndInstance;
				AuthenticationMode auditdbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbUser = App._installationOptions.SQLInstallLogin;
				string dbPassword = App._installationOptions.SQLInstallPassword;
				string dbNewLogin = App._installationOptions.DatabaseUser;
				string dbNewUser = App._installationOptions.DatabaseUser;
				string dbWinLogin = Constants.WINDOWS_AUTH_LOGIN;
				bool dbSampleData = App._installationOptions.DatabaseSampleData;

				//Connection token to use when running scripts.
				DBConnection _connection = new DBConnection()
				{
					AuditDatabaseName = auditdbName,
					DatabaseName = dbName,
					DatabaseServer = dbServer,
					LoginAuthType = dbAuthType,
					LoginPassword = dbPassword,
					LoginUser = dbUser,
					AuditDatabaseServer = auditdbServer,
					AuditLoginAuthType = auditdbAuthType,
					AuditLoginPassword = auditdbPassword,
					AuditLoginUser = auditdbUser
				};

				//First we need to find out the path to the MSSQL\Data folder on the database server
				streamWriter.WriteLine("Locating the path to the SQL Server data folder");
				string localDbFilename = "";
				using (SqlConnection connection2 = new SqlConnection(SQLUtilities.GenerateConnectionString("master", dbServer, dbAuthType, dbUser, dbPassword)))
				{
					try
					{
						connection2.Open();

						using (SqlCommand command = new SqlCommand())
						{
							command.Connection = connection2;
							command.CommandType = CommandType.Text;
							command.CommandText = "SELECT filename FROM sysfiles WHERE RTRIM(name) = 'master'";
							SqlDataReader reader = command.ExecuteReader();
							if (!reader.HasRows)
							{
								MessageBox.Show("Unable to locate the SQL Server data folder from the master database", Constants.MESSAGE_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
								return false;
							}
							reader.Read();
							localDbFilename = (string)reader["filename"];
							reader.Close();
						}
					}
					finally
					{
						connection2.Close();
					}
				}

				//Before creating the database we need to log out the user running the script as that can be very useful
				//debugging installations (when using Windows authentication) where the create fails
				streamWriter.WriteLine("Logging database diagnostics information");
				if (dbAuthType == AuthenticationMode.Windows)
				{
					using (SqlConnection connection2 = new SqlConnection(SQLUtilities.GenerateConnectionString("master", dbServer, dbAuthType, dbUser, dbPassword)))
					{
						try
						{
							string diagnostics1 = "";
							string diagnostics2 = "";
							string diagnostics3 = "";

							//SELECT SUSER_NAME() AS YourLogin, USER_NAME() AS YourDBUserIdentity;
							using (SqlCommand command2 = new SqlCommand())
							{
								connection2.Open();
								command2.Connection = connection2;
								command2.CommandType = CommandType.Text;
								command2.CommandText = "SELECT SUSER_NAME() + ' = ' + USER_NAME() AS YourDBUserIdentity";
								SqlDataReader reader2 = command2.ExecuteReader();
								if (reader2.HasRows)
								{
									reader2.Read();
									diagnostics1 = "-- DB User Identity: " + (string)reader2["YourDBUserIdentity"];
								}
								reader2.Close();

								//SELECT IS_SRVROLEMEMBER('sysadmin') AS True_is1_False_is0;
								command2.CommandText = "SELECT IS_SRVROLEMEMBER('sysadmin') AS True_is1_False_is0";
								reader2 = command2.ExecuteReader();
								if (reader2.HasRows)
								{
									reader2.Read();
									diagnostics2 = "-- Is User SysAdmin: " + (int)reader2["True_is1_False_is0"];
								}
								reader2.Close();

								//SELECT permission_name, state_desc FROM sys.server_permissions WHERE grantee_principal_id = SUSER_ID();
								command2.CommandText = "SELECT permission_name + ' = ' + state_desc AS PermissionState FROM sys.server_permissions WHERE grantee_principal_id = SUSER_ID()";
								reader2 = command2.ExecuteReader();
								if (reader2.HasRows)
								{
									reader2.Read();
									diagnostics3 += "-- PermissionState: " + (string)reader2["PermissionState"] + Environment.NewLine;
								}
								reader2.Close();
							}

							//Finally log out the diagnostics

							//Now create the folder that will hold the log files for this specific operation
							streamWriter.WriteLine(diagnostics1 +
								Environment.NewLine +
								diagnostics2 +
								Environment.NewLine +
								diagnostics3);
						}
						finally
						{
							connection2.Close();
						}
					}
				}
				streamWriter.WriteLine("Creating the database installation script.");

				//Strip out the filename from the path itself
				string localDbPath = localDbFilename.Substring(0, localDbFilename.IndexOf("\\master"));

				App.logFile.WriteLine(" Audit Database Create - Starting");
				
				//Read in the tokenized create database script
				string createFile = ((dbAuthType == AuthenticationMode.Windows) ? "create_tst_db_win.sql" : "create_tst_db_sql.sql");
				string sqlText = ZipReader.GetContents("base.DB_Create.zip", createFile);

				//If we are passed NT AUTHORITY\NETWORK SERVICE as the windows login, need to correctly
				//localize it so that it works on non-English versions of Windows
				
				if (dbWinLogin == "NT AUTHORITY\\NETWORK SERVICE")
				{
					SecurityIdentifier si = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
					NTAccount ntAccount = (NTAccount)si.Translate(typeof(NTAccount));
					dbWinLogin = ntAccount.Value;
					App.logFile.WriteLine(" NETWORK SERVICE ----- " + dbWinLogin);
				}
				App.logFile.WriteLine(" NETWORK SERVICE - " + dbWinLogin);

				//Replace the various tokens
				sqlText = sqlText.Replace("[DBDATAPATH]", localDbPath);
				sqlText = sqlText.Replace("[DBLOGIN]", dbNewLogin);
				sqlText = sqlText.Replace("[WINLOGIN]", dbWinLogin);
				sqlText = sqlText.Replace("[DBNAME]", dbName);
				sqlText = sqlText.Replace("[AUDITDBNAME]", auditdbName);
				sqlText = sqlText.Replace("[DBUSER]", dbNewUser);
				sqlText = sqlText.Replace("[DBPASSWORD]", Properties.Settings.Default.SQLUserPassword);
				App.logFile.WriteLine(" Audit Database Create Details");
				sqlText = sqlText.Replace("[AUDITDBLOGIN]", auditdbNewLogin);
				sqlText = sqlText.Replace("[AUDITWINLOGIN]", dbWinLogin);
				sqlText = sqlText.Replace("[AUDITDBNAME]", auditdbName);
				sqlText = sqlText.Replace("[AUDITDBUSER]", auditdbNewUser);
				sqlText = sqlText.Replace("[AUDITDBPASSWORD]", Properties.Settings.Default.SQLUserPassword);

				//DB Sizes (in MB)
				int dataSize = 50;
				int dataGrowth = 10;
				int logSize = 10;
				int logGrowth = 10;

				//See if we have a custom model size specified
				if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TST_DB_SIZE")))
				{
					int customSize;
					if (int.TryParse(Environment.GetEnvironmentVariable("TST_DB_SIZE"), out customSize))
					{
						dataSize = customSize;
						logSize = customSize / 5;
					}
				}
				sqlText = sqlText.Replace("[DATASIZE]", dataSize.ToString());
				sqlText = sqlText.Replace("[DATAGROWTH]", dataGrowth.ToString());
				sqlText = sqlText.Replace("[LOGSIZE]", logSize.ToString());
				sqlText = sqlText.Replace("[LOGGROWTH]", logGrowth.ToString());

				//Create the physical database - fail quietly in the extreme case that we have a database in use
				//that could not be previously dropped but still exists.
				streamWriter.WriteLine("Creating the physical database.");
				SQLUtilities.ExecuteSqlCommands(
					new DBConnection()
					{
						DatabaseServer = dbServer,
						LoginAuthType = dbAuthType,
						LoginPassword = dbPassword,
						LoginUser = dbUser,
					    AuditDatabaseServer = auditdbServer,
						AuditLoginAuthType = auditdbAuthType,
						AuditLoginPassword = auditdbPassword,
						AuditLoginUser = auditdbUser,
					},
					sqlText,
					true);

				streamWriter.WriteLine("Loading the database objects..");
				//Load the schame, tables, FKs, etc.
				string sqlCmd1 = ZipReader.GetContents("base.DB_Create.zip", "tst_schema.sql");
				SQLUtilities.ExecuteSqlCommands(_connection, sqlCmd1);
				//Create the Freetext indexes, if they have them enabled.
				sqlCmd1 = ZipReader.GetContents("base.DB_Create.zip", "create_freetext_catalogs.sql");
				SQLUtilities.ExecuteSqlCommands(_connection, sqlCmd1);
				//Create the Programmable Objects.
				SQLUtilities.CreateProgrammableObjects(_connection, streamWriter);
				//Load the Static Data - this must succeed, so we want to abort on failure.
				streamWriter.WriteLine("Loading the Static Data.");
				List<string> filesStatic = ZipReader.GetFilesIn("base.DB_StaticData.zip");
				foreach (string file in filesStatic.OrderBy(f => f))
				{
					try
					{
						string sqlToRun = ZipReader.GetContents("base.DB_StaticData.zip", file);
						SQLUtilities.ExecuteSqlCommands(_connection, sqlToRun);
					}
					catch (Exception ex)
					{
						string msg = "Error loading data in: " + file + Environment.NewLine + Logger.DecodeException(ex);
						streamWriter.WriteLineAsync(msg);
					}
				}

				//Load in the sample data if necessary
				if (dbSampleData)
				{
					streamWriter.WriteLine("Loading the Sample Data.");
					List<string> filesSample = ZipReader.GetFilesIn("base.DB_SampleData.zip");
					foreach (string file in filesSample.OrderBy(f => f))
					{
						try
						{
							string sqlToRun = ZipReader.GetContents("base.DB_SampleData.zip", file);
							SQLUtilities.ExecuteSqlCommands(_connection, sqlToRun);
						}
						catch (Exception ex)
						{
							string msg = "Error loading data in: " + file + Environment.NewLine + Logger.DecodeException(ex);
							streamWriter.WriteLineAsync(msg);
						}
					}
				}

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Unable to install the product database: " + Environment.NewLine + Logger.DecodeException(ex));

				string ErrorMsg = "Unable to install the product database:" + Environment.NewLine + ex.Message;
				ProgressUpdate(
					this,
							new ProgressArgs()
							{
								ErrorText = ErrorMsg,
								Progress = -1,
								Status = ItemProgress.ProcessStatusEnum.Error,
								TaskNum = TaskDisplayLine
							}
						);
				return false;
			}
		}

		/// <summary>
		/// Drops the database
		/// </summary>
		/// <param name="failQuietly">Do we fail quietly?</param>
		private bool DropDatabase(bool failQuietly)
		{
			//Get the various properties
			string dbFilePath = Path.Combine(App._installationOptions.InstallationFolder, "Database");
			string dbServer = App._installationOptions.SQLServerAndInstance;
			AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
			string dbName = App._installationOptions.DatabaseName;
			string auditdbName = App._installationOptions.AuditDatabaseName;
			string dbUser = App._installationOptions.SQLInstallLogin;
			string dbPassword = App._installationOptions.SQLInstallPassword;
			string dbNewLogin = App._installationOptions.SQLLoginToCreate;
			string dbNewUser = App._installationOptions.DatabaseUser;
			string dbWinLogin = Constants.WINDOWS_AUTH_LOGIN;
			bool dbSampleData = App._installationOptions.DatabaseSampleData;

			try
			{
				//Open the connection - using either windows or sql server authentication
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = SQLUtilities.GenerateConnectionString("master", dbServer, dbAuthType, dbUser, dbPassword);
					connection.Open();

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandType = CommandType.Text;

						//Now switch context to MASTER to avoid the dreaded 'In Use' error when we try the drop
						command.CommandText = "USE master";
						command.ExecuteNonQuery();

						App.logFile.WriteLine(" Audit Database Delete - Starting");
						command.CommandText = "DROP DATABASE [" + auditdbName + "]";
						command.ExecuteNonQuery();
						App.logFile.WriteLine(" Audit Database Delete - Ending");

						//Now actually execute the drop command
						App.logFile.WriteLine("VM Database Delete - Starting");
						command.CommandText = "DROP DATABASE [" + dbName + "]";
						command.ExecuteNonQuery();
						App.logFile.WriteLine(" VM Database Delete - Ending");

						//Next drop the login (the parameter depends on type of authentication we're using)
						if (dbAuthType == AuthenticationMode.Windows)
						{
							command.CommandText = "DROP LOGIN [" + dbWinLogin + "]";
						}
						else
						{
							command.CommandText = "DROP LOGIN [" + dbNewLogin + "]";
						}
						command.ExecuteNonQuery();
					}

					//Close the connection
					connection.Close();
				}
				return true;
			}
			catch (Exception exception)
			{
				if (failQuietly)
				{
					return true;
				}
				else
				{
					MessageBox.Show("Error occurred during database drop - " + exception.Message + " (connecting to '" + dbServer + "')", Constants.MESSAGE_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}
		}
	}
}
