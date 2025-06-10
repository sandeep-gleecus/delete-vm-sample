using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Inflectra.InternalTools.DatabasePatcher
{
	/// <summary>
	/// Responsible for actually patching the database
	/// </summary>
	public static class Patcher
	{
		private const string DEFAULT_CACHE_FOLDER = "D:\\SpiraDataCache\\{0}\\VersionControlCache";

		private static Regex GO_SPLIT = new Regex("^GO\\s?(?:\\r?\\n|\\r|$)", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		private static Regex GOIF_FIX = new Regex("^GO\\s?IF", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		private const int LONG_TIMEOUT_SECONDS = 30 * 60; //30 minutes

		/// <summary>
		/// Should we run the v6.5 post upgrade migration script
		/// </summary>
		static bool run650_migration = false;

		static int old_databaseVersion = 0;
		static int new_databaseVersion = 0;

		/// <summary>
		/// Applies the patch
		/// </summary>
		/// <returns>Any error message, null means success</returns>
		public static string ApplyPatch()
		{
			//Get the current directory
			//It assumes that the EXE is inside the Database folder
			string databaseFolder = Path.GetDirectoryName(Application.ExecutablePath);

			//The Web.Config file should be one level up
			string parentFolder = Directory.GetParent(databaseFolder).FullName;
			string webConfigFile = Path.Combine(parentFolder, "Web.Config");

			Console.Write("-- Web config -- ");
			Console.Write("exist" + File.Exists(webConfigFile));
			if (File.Exists(webConfigFile))
			{
				try
				{
					Console.Write(" Web Config starts ");
					//Update Web.Config
					PatchWebConfig(webConfigFile);
				}
				catch (Exception exception)
				{
					return string.Format("Error updating web.config file for site ('{0}')", exception.Message);
				}

				string entityConnectionString = getConnectionString(webConfigFile);

				if (string.IsNullOrEmpty(entityConnectionString))
				{
					return "Connection String in Web.Config is empty, or could not find the file!";
				}

				//Now get the provider connection string
				EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder(entityConnectionString);
				string providerConnection = ecsb.ProviderConnectionString;

				//Get the list of database files to be applied (X.X_patch_0*.sql)
				DirectoryInfo databaseDirectory = new DirectoryInfo(databaseFolder);
				FileInfo[] files = databaseDirectory.GetFiles(Properties.Settings.Default.Version + "_patch_0*.sql");

				if (files == null || files.Length < 1)
				{
					return "Unable to locate any Files in the database folder!";
				}

				//Now try and connect
				try
				{
					string result;
					SqlConnection sqlConnection = new SqlConnection(providerConnection);
					sqlConnection.Open();

					//Get the old database version, fail quietly
					try
					{
						using (SqlCommand command = new SqlCommand())
						{
							//Open connection.
							command.Connection = sqlConnection;
							command.CommandType = CommandType.Text;
							command.CommandText = "SELECT [VALUE] FROM [TST_GLOBAL_SETTING] WHERE [NAME] = 'Database_Revision'";
							string value = (string)command.ExecuteScalar();
							old_databaseVersion = Int32.Parse(value);
						}
					}
					catch (Exception)
					{
						//Do nothing
					}

					//Execute each of the patch files
					foreach (FileInfo file in files)
					{
						try
						{
							//Exclude the files that are not from the patch, but are actually part of the normal install
							if (file.Name.ToLowerInvariant() != "create_tst_db.sql" &&
								file.Name.ToLowerInvariant() != "create_tst_db_sql.sql" &&
								file.Name.ToLowerInvariant() != "create_tst_db_win.sql" &&
								file.Name.ToLowerInvariant() != "indent_level_collations.sql" &&
								file.Name.ToLowerInvariant() != "create_freetext_catalogs.sql" &&
								file.Name.ToLowerInvariant() != "tst_sample_data.sql" &&
								file.Name.ToLowerInvariant() != "tst_schema.sql" &&
								file.Name.ToLowerInvariant() != "tst_schema_drop.sql" &&
								file.Name.ToLowerInvariant() != "tst_static_data.sql" &&
								file.Name.ToLowerInvariant() != "tst_addl_objects.sql" &&
								file.Name.ToLowerInvariant() != "tst_erwin.sql" &&
								file.Name.ToLowerInvariant() != "tst_erwin_drop.sql")
							{
								result = ExecuteSqlFile(sqlConnection, file.FullName, "GO", false);
								if (!string.IsNullOrEmpty(result))
								{
									return file.FullName + ": " + result;
								}
							}
						}
						catch (Exception exception)
						{
							throw new Exception(string.Format("Error running database update '{0}' - error: {1}", file.Name, exception.Message));
						}
					}

					//Next rebuild all the database objects
					files = databaseDirectory.GetFiles("tst_addl_objects.sql");
					if (files == null || files.Length < 1)
					{
						return "Unable to locate 'tst_addl_objects.sql' in the database folder!";
					}
					FileInfo objectsFile = files[0];
					result = ExecuteSqlFile2(sqlConnection, objectsFile.FullName, false);
					if (!string.IsNullOrEmpty(result))
					{
						return objectsFile.FullName + ": " + result;
					}

					if (run650_migration)
					{
						//Finally run the migration command with a long timeout, fail quietly since data tools will also sort it out
						result = ExecuteProcedure(sqlConnection, "dbo.MIGRATION_POPULATE_REQUIREMENT_COMPLETION", LONG_TIMEOUT_SECONDS, true);
						if (!string.IsNullOrEmpty(result))
						{
							return objectsFile.FullName + ": " + result;
						}
					}

					//If needed, update hierarchy tables.
					var query1 = GetDataQuery(sqlConnection, "SELECT COUNT(*) FROM [TST_TEST_CASE_FOLDER_HIERARCHY]");
					if ((int)query1.Tables[0].Rows[0][0] == 0)
					{
						//We now loop past each project # and run the stored procs.
						var dataSet = GetDataQuery(sqlConnection, "SELECT DISTINCT [PROJECT_ID] FROM [TST_PROJECT] ORDER BY [PROJECT_ID] ASC;");

						//List of procedures to execute per-project.
						List<string> procNames = new List<string>
						{
							"TESTCASE_REFRESH_PARAMETER_HIERARCHY",
							"TESTCASE_REFRESH_FOLDER_HIERARCHY",
							"TASK_REFRESH_FOLDER_HIERARCHY",
							"ATTACHMENT_REFRESH_FOLDER_HIERARCHY",
							"TESTSET_REFRESH_FOLDER_HIERARCHY"
						};

						//Loop through each project, and run the 5 stored procs.
						foreach (DataRow projRow in dataSet.Tables[0].Rows)
						{
							//Get the project ID.
							int projId = (int)projRow[0];

							foreach (var sqltmp in procNames)
							{
								string sqlExec = "EXEC [" + sqltmp + "] @ProjectId = " + projId.ToString();
								using (SqlCommand command = new SqlCommand())
								{
									//Set command properties.
									command.Connection = sqlConnection;
									command.CommandTimeout = 1800;
									command.CommandType = CommandType.Text;
									command.CommandText = sqlExec;

									//Open connectoion.
									command.ExecuteNonQuery();
								}
							}
						}
					}

					//Get the new database version, fail quietly
					try
					{
						using (SqlCommand command = new SqlCommand())
						{
							//Open connection.
							command.Connection = sqlConnection;
							command.CommandType = CommandType.Text;
							command.CommandText = "SELECT [VALUE] FROM [TST_GLOBAL_SETTING] WHERE [NAME] = 'Database_Revision'";
							string value = (string)command.ExecuteScalar();
							new_databaseVersion = Int32.Parse(value);
						}
					}
					catch (Exception)
					{
						//Do nothing
					}

					//If the old version was < 6.7 and the new version >= 6.7 need to clear the version caches
					if (old_databaseVersion < 670 && new_databaseVersion >= 670)
					{
						string siteName = Path.GetFileName(parentFolder);
						DeleteOldVersionCaches(sqlConnection, siteName);
					}

					sqlConnection.Close();
					sqlConnection.Dispose();

					return null;
				}
				catch (SqlException exception)
				{
					return string.Format("Error connecting to database to apply update ('{0} - {1}: {2}')", exception.ErrorCode, exception.Message, exception.ToString());
				}
				catch (Exception exception)
				{
					return string.Format("Error connecting to database to apply update ('{0}')", exception.Message);
				}
			}
			else
			{
				return string.Format("Unable to find Web.Config file in location '{0}', make sure that the DatabasePatcher is inside the Database folder.", webConfigFile);
			}

		}

		//Splits a string by a string
		private static string[] SplitByString(string testString, string split)
		{
			return SplitByString2(testString);
		}

		//Splits a string by a string
		private static string[] SplitByString2(string testString)
		{
			//Convert to Regex. Because.
			return GO_SPLIT.Split(testString);
		}

		/// <summary>
		/// Returns the folder to be used for temporary data during installation/migration
		/// </summary>
		/// <remarks>
		/// By default it will use the Users\AppData\Local folder unless a special environment variable %TST_WORKING_FOLDER% is set
		/// </remarks>
		private static string TempFolder
		{
			get
			{
				if (string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("TST_WORKING_FOLDER")))
				{
					return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				}
				else
				{
					return System.Environment.GetEnvironmentVariable("TST_WORKING_FOLDER");
				}
			}
		}

		public static object SQLUtilities { get; private set; }

		/// <summary>Executes a database stored procedure</summary>
		/// <param name="connection">SQL erver connection information.</param>
		/// <param name="procedure">The named of the stored proc</param>
		/// <returns>The return code (null for error)</returns>
		/// <remarks>Logs the result of the installation to .log files in the user's Temp folder</remarks>
		private static string ExecuteProcedure(SqlConnection connection, string procedure, int? timeout = null, bool failQuietly = false)
		{
			StreamWriter logWriter = null;
			try
			{
				//First, get the temp folder for the current user
				string folderBase = TempFolder;

				//Create an Inflectra subfolder (if one doesn't already exist)
				folderBase += @"\Inflectra";
				if (!Directory.Exists(folderBase))
				{
					Directory.CreateDirectory(folderBase);
				}
				//Now create the folder that will hold the log files for this specific operation
				string folderPath = folderBase + @"\SpiraInstallLog_" + DateTime.Now.ToString("yyyyMMdd");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				//Create the log file for writing, fail quietly
				try
				{
					logWriter = File.CreateText(folderPath + "\\" + procedure + ".log");
				}
				catch (Exception)
				{
					//Fail quietly
				}

				using (SqlCommand command = new SqlCommand())
				{
					//Open connection.
					command.Connection = connection;
					if (timeout.HasValue)
					{
						command.CommandTimeout = timeout.Value;
					}
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = procedure;
					command.ExecuteNonQuery();
				}

				if (logWriter != null)
				{
					logWriter.Close();
				}
				return null;
			}
			catch (Exception exception)
			{
				if (logWriter != null)
				{
					logWriter.WriteLine("Error Occured: " + exception.Message);
				}
				if (failQuietly)
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return null;
				}
				else
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
				}
			}
		}

		/// <summary>
		/// Executes a .SQL file containing multiple SQL statements
		/// </summary>
		/// <param name="filepath">The full path to the file</param>
		/// <param name="delimiter">The delimiter between SQL statements</param>
		/// <param name="connection">The database connection</param>
		/// <returns>The return code (null for error)</returns>
		/// <remarks>Logs the result of the installation to .log files in the user's Temp folder</remarks>
		private static string ExecuteSqlFile(SqlConnection connection, string filepath, string delimiter, bool failQuietly)
		{
			StreamWriter logWriter = null;
			try
			{
				//First, get the temp folder for the current user
				string folderBase = TempFolder;

				//Create an Inflectra subfolder (if one doesn't already exist)
				folderBase += @"\Inflectra";
				if (!Directory.Exists(folderBase))
				{
					Directory.CreateDirectory(folderBase);
				}
				//Now create the folder that will hold the log files for this specific operation
				string folderPath = folderBase + @"\SpiraInstallLog_" + DateTime.Now.ToString("yyyyMMdd");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				//Create the log file for writing, fail quietly
				try
				{
					string filename = Path.GetFileName(filepath);
					logWriter = File.CreateText(folderPath + "\\" + filename + ".log");
				}
				catch (Exception)
				{
					//Fail quietly
				}

				//Next open the file containing the SQL statements
				StreamReader streamReader = File.OpenText(filepath);
				string sqlCommandStream = streamReader.ReadToEnd();
				streamReader.Close();

				//Now split up the SQL statements if necessary
				if (string.IsNullOrEmpty(delimiter))
				{
					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					try
					{
						command.CommandType = CommandType.Text;
						command.CommandText = sqlCommandStream;
						if (logWriter != null)
						{
							logWriter.WriteLine("Executing: " + command.CommandText);
						}
						int rowCount = command.ExecuteNonQuery();
					}
					catch (Exception exception)
					{
						if (logWriter != null)
						{
							logWriter.WriteLine("Error Occured: " + exception.Message);
						}
						if (!failQuietly)
						{
							if (logWriter != null)
							{
								logWriter.Close();
							}
							return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
						}
					}
				}
				else
				{
					string[] sqlCommands = SplitByString(sqlCommandStream, delimiter);

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					//Execute each command in turn
					for (int i = 0; i < sqlCommands.Length; i++)
					{
						try
						{
							command.CommandType = CommandType.Text;
							command.CommandText = sqlCommands[i];
							if (!string.IsNullOrWhiteSpace(command.CommandText))
							{
								if (logWriter != null)
								{
									logWriter.WriteLine("Executing: " + command.CommandText);
								}
								int rowCount = command.ExecuteNonQuery();
							}
						}
						catch (Exception exception)
						{
							if (logWriter != null)
							{
								logWriter.WriteLine("Error Occured: " + exception.Message);
							}
							if (!failQuietly)
							{
								if (logWriter != null)
								{
									logWriter.Close();
								}
								return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
							}
						}
					}
				}
				if (logWriter != null)
				{
					logWriter.Close();
				}
				return null;
			}
			catch (Exception exception)
			{
				if (failQuietly)
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return null;
				}
				else
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
				}
			}
		}

		/// <summary>
		/// Executes a .SQL file containing multiple SQL statements using a different SPLIT function
		/// that is needed for the concatenated addl_objects.sql
		/// </summary>
		/// <param name="filepath">The full path to the file</param>
		/// <param name="delimiter">The delimiter between SQL statements</param>
		/// <param name="connection">The database connection</param>
		/// <returns>The return code (null for error)</returns>
		/// <remarks>Logs the result of the installation to .log files in the user's Temp folder</remarks>
		private static string ExecuteSqlFile2(SqlConnection connection, string filepath, bool failQuietly)
		{
			StreamWriter logWriter = null;
			try
			{
				//First, get the temp folder for the current user
				string folderBase = TempFolder;

				//Create an Inflectra subfolder (if one doesn't already exist)
				folderBase += @"\Inflectra";
				if (!Directory.Exists(folderBase))
				{
					Directory.CreateDirectory(folderBase);
				}
				//Now create the folder that will hold the log files for this specific operation
				string folderPath = folderBase + @"\SpiraInstallLog_" + DateTime.Now.ToString("yyyyMMdd");
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				//Create the log file for writing, fail quietly
				try
				{
					string filename = Path.GetFileName(filepath);
					logWriter = File.CreateText(folderPath + "\\" + filename + ".log");
				}
				catch (Exception)
				{
					//Fail quietly
				}

				//Next open the file containing the SQL statements
				StreamReader streamReader = File.OpenText(filepath);
				string sqlCommandStream = streamReader.ReadToEnd();
				streamReader.Close();

				//Clean up potential 
				string[] sqlCommands = SplitByString2(sqlCommandStream);

				SqlCommand command = new SqlCommand();
				command.Connection = connection;

				//Execute each command in turn
				for (int i = 0; i < sqlCommands.Length; i++)
				{
					try
					{
						command.CommandType = CommandType.Text;
						command.CommandText = sqlCommands[i];
						if (!string.IsNullOrWhiteSpace(command.CommandText))
						{
							if (logWriter != null)
							{
								logWriter.WriteLine("Executing: " + command.CommandText);
							}
							int rowCount = command.ExecuteNonQuery();
						}
					}
					catch (Exception exception)
					{
						if (logWriter != null)
						{
							logWriter.WriteLine("Error Occured: " + exception.Message);
						}
						if (!failQuietly)
						{
							if (logWriter != null)
							{
								logWriter.Close();
							}
							return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
						}
					}
				}
				if (logWriter != null)
				{
					logWriter.Close();
				}
				return null;
			}
			catch (Exception exception)
			{
				if (failQuietly)
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return null;
				}
				else
				{
					if (logWriter != null)
					{
						logWriter.Close();
					}
					return "Error occurred during installation - " + exception.Message + " (SQL Execution)";
				}
			}
		}

		/// <summary>Runs a query and returns the result.</summary>
		/// <param name="connection"></param>
		/// <param name="select"></param>
		/// <returns></returns>
		static public DataSet GetDataQuery(SqlConnection connection, string select)
		{
			DataSet retValue = null;

			//Now we need to delete any existing license information
			using (SqlCommand command = new SqlCommand())
			{
				command.Connection = connection;
				command.CommandTimeout = 1800;
				command.CommandType = CommandType.Text;
				command.CommandText = select;

				using (SqlDataAdapter reader = new SqlDataAdapter())
				{
					retValue = new DataSet();
					reader.SelectCommand = command;
					reader.Fill(retValue);
				}
			}

			return retValue;
		}

		/// <summary>Makes sure the Wed.Config document is up to date!</summary>
		/// <param name="doc">The document!</param>
		/// <returns>Status of the updated patch.</returns>
		private static bool PatchWebConfig(string filePath)
		{
			Console.Write(" Patch WebConfig ");
			bool retValue = false;
			bool madeChanges = false;
			string fileToWrite = null;


			using (var xmlReader = openWebConfig(filePath))
			{
				//Create & load the document.
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(xmlReader);

				//Generate the f*ckin' namespace without prefix for M$.
				XmlNamespaceManager bindMgr = new XmlNamespaceManager(xmlDocument.NameTable);
				bindMgr.AddNamespace("aa", "urn:schemas-microsoft-com:asm.v1");

				//Get our root element.
				XmlNode confNode = xmlDocument.SelectSingleNode("/configuration") as XmlElement;
				if (confNode != null)
				{
					#region v6.4
					/* v6.4 Additions:
					 * - Oauth Authorization pages set to NOT require Authentication.
					 * - Element added to AppSettings, "ValidationSettings:UnobtrusiveValidationMode" set to "None"
					 * - Attribute "targetFramework" set to "4.6" set on to "system.web/httpRuntime" Element.
					 * 
					 * See: https://spira.inflectra.com/6/Incident/5512.aspx
					 */

					//Get our reference element. (To insert items before.
					XmlNode insertBefore = confNode.SelectSingleNode("location[@path=\"EmailPassword.aspx\"]");

					//The OAuth Handler
					var location = confNode.SelectSingleNode("location[@path=\"OauthHandler.ashx\"]");
					if (location == null)
					{
						//Didn't already exist. Add it!
						/*
						 *	<system.web>
						 *		<authorization>
						 *			<allow users="?"/>
						 *		</authorization>
						 *	</system.web>
						 */
						XmlElement xmlAllow = xmlDocument.CreateElement("allow");
						xmlAllow.SetAttribute("users", "?");
						XmlElement xmlAuth = xmlDocument.CreateElement("authorization");
						xmlAuth.AppendChild(xmlAllow);
						XmlElement xmlSysW = xmlDocument.CreateElement("system.web");
						xmlSysW.AppendChild(xmlAuth);
						XmlElement xmlLoca = xmlDocument.CreateElement("location");
						xmlLoca.AppendChild(xmlSysW);
						xmlLoca.SetAttribute("path", "OauthHandler.ashx");

						if (insertBefore != null)
							confNode.InsertBefore(xmlLoca, insertBefore);
						else
							confNode.AppendChild(xmlLoca);

						madeChanges = true;
					}

					//The OAuth subdirectory.
					location = confNode.SelectSingleNode("location[@path=\"Oauth\"]");
					if (location == null)
					{
						/*
						 *	<system.web>
						 *		<authorization>
						 *			<allow users="?"/>
						 *		</authorization>
						 *	</system.web>
						 */
						XmlElement xmlAllow = xmlDocument.CreateElement("allow");
						xmlAllow.SetAttribute("users", "?");
						XmlElement xmlAuth = xmlDocument.CreateElement("authorization");
						xmlAuth.AppendChild(xmlAllow);
						XmlElement xmlSysW = xmlDocument.CreateElement("system.web");
						xmlSysW.AppendChild(xmlAuth);
						XmlElement xmlLoca = xmlDocument.CreateElement("location");
						xmlLoca.AppendChild(xmlSysW);
						xmlLoca.SetAttribute("path", "Oauth");

						if (insertBefore != null)
							confNode.InsertBefore(xmlLoca, insertBefore);
						else
							confNode.AppendChild(xmlLoca);

						madeChanges = true;
					}

					//Add the TargetFramework to the httpRuntime node.
					XmlElement httpRun = xmlDocument.SelectSingleNode("/configuration/system.web/httpRuntime") as XmlElement;
					if (httpRun != null)
					{
						//If there already is an attribute, skip it.
						if (httpRun.Attributes["targetFramework"] == null)
						{
							httpRun.SetAttribute("targetFramework", "4.6");
							madeChanges = true;
						}
					}

					//Add the AppSetting 'UnobtrusiveValidation' flag.
					XmlElement appSett = xmlDocument.SelectSingleNode("/configuration/appSettings") as XmlElement;
					if (appSett != null)
					{
						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"ValidationSettings:UnobtrusiveValidationMode\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "ValidationSettings:UnobtrusiveValidationMode");
							newSet.SetAttribute("value", "None");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
					}
					#endregion v6.4

					#region v6.5
					XmlElement profProps = xmlDocument.SelectSingleNode("/configuration/system.web/profile[@defaultProvider=\"SpiraProfileProvider\"]/properties") as XmlElement;
					if (profProps != null)
					{
						//Check that the three properties exist.
						if (profProps.SelectSingleNode("add[@name=\"IsPortfolioAdmin\"]") == null)
						{
							XmlElement newProf1 = xmlDocument.CreateElement("add");
							newProf1.SetAttribute("name", "IsPortfolioAdmin");
							newProf1.SetAttribute("type", "Boolean");
							newProf1.SetAttribute("readOnly", "false");
							newProf1.SetAttribute("allowAnonymous", "false");

							profProps.AppendChild(newProf1);
							madeChanges = true;
							run650_migration = true;
						}
						if (profProps.SelectSingleNode("add[@name=\"IsRestricted\"]") == null)
						{
							XmlElement newProf1 = xmlDocument.CreateElement("add");
							newProf1.SetAttribute("name", "IsRestricted");
							newProf1.SetAttribute("type", "Boolean");
							newProf1.SetAttribute("readOnly", "false");
							newProf1.SetAttribute("allowAnonymous", "false");

							profProps.AppendChild(newProf1);
							madeChanges = true;
							run650_migration = true;
						}
						if (profProps.SelectSingleNode("add[@name=\"IsResourceAdmin\"]") == null)
						{
							XmlElement newProf1 = xmlDocument.CreateElement("add");
							newProf1.SetAttribute("name", "IsResourceAdmin");
							newProf1.SetAttribute("type", "Boolean");
							newProf1.SetAttribute("readOnly", "false");
							newProf1.SetAttribute("allowAnonymous", "false");

							profProps.AppendChild(newProf1);
							madeChanges = true;
							run650_migration = true;
						}
					}
					#endregion v6.5

					#region v6.6.1

					//Add the TLS 1.2+ default  to the runtime node.
					//<AppContextSwitchOverrides value="Switch.System.Net.DontEnableSystemDefaultTlsVersions=false"/>
					XmlElement xmlRuntimeElement = xmlDocument.SelectSingleNode("/configuration/runtime") as XmlElement;
					if (xmlRuntimeElement != null)
					{
						//If there already is an element, skip it.
						XmlNode appContextSwitchOverrides = xmlRuntimeElement.SelectSingleNode("AppContextSwitchOverrides");
						if (appContextSwitchOverrides == null)
						{
							XmlElement appContextSwitchOverridesElement = xmlDocument.CreateElement("AppContextSwitchOverrides");
							appContextSwitchOverridesElement.SetAttribute("value", "Switch.System.Net.DontEnableSystemDefaultTlsVersions=false");
							xmlRuntimeElement.AppendChild(appContextSwitchOverridesElement);
							madeChanges = true;
						}
					}

					//Also need to add the following:
					//<add key="AppContext.SetSwitch:Switch.System.Net.DontEnableSystemDefaultTlsVersions" value="false" />
					appSett = xmlDocument.SelectSingleNode("/configuration/appSettings") as XmlElement;
					if (appSett != null)
					{
						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"AppContext.SetSwitch:Switch.System.Net.DontEnableSystemDefaultTlsVersions\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "AppContext.SetSwitch:Switch.System.Net.DontEnableSystemDefaultTlsVersions");
							newSet.SetAttribute("value", "false");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
					}

					#endregion v6.6.1

					#region 6.7
					// In v6.6.1 we noticed we had two different <runtime> nodes within the <configuration>. This was fixed in the ON-Premise installer, but was not
					//   noticed until too late that this issue also existed in the Cloud installes. One or both nodes has the <enforceFIPSPolicy> tag, while the other
					//   has everything else. So, first we are going to check that two <runtime> exists.
					var nodes67 = xmlDocument.SelectNodes("/configuration/runtime");
					if (nodes67.Count > 1)
					{
						// We want to add the enableFIPS to the runtime node that has the other entries, if it does not already exist. We can't search on the enableFips, because
						//  if both <runtime> has it, we still won't know if we have the right one. 
						int nodeToDelIdx = 1;
						var node = nodes67[0].SelectSingleNode("AppContextSwitchOverrides");
						if (node == null)
						{
							nodeToDelIdx = 0;
							node = nodes67[1].SelectSingleNode("AppContextSwitchOverrides");
						}
						//Get the parent node, now that we have the child node we're looking for.
						if (node != null)
						{
							node = node.ParentNode;

							//Double-check, to be safe.
							if (node != null)
							{
								//See if the 'enforceFIPSPolicy' is present. (If not, add it.)
								if (node.SelectSingleNode("enforceFIPSPolicy") == null)
								{
									//It did not exist in the runtime we wanted it to, so add it.
									XmlElement appContextSwitchOverridesElement = xmlDocument.CreateElement("enforceFIPSPolicy");
									appContextSwitchOverridesElement.SetAttribute("enabled", "false");
									nodes67[0].AppendChild(appContextSwitchOverridesElement);
									madeChanges = true;
								}
							}
						}
						else
						{
							//BIG ERROR. Neither <runtime> had 'AppContextSwitchOverrides' in it, which means that
							//  this Webconfig is not trustable. Abort with error.
							Console.WriteLine("Error in Web.Config, v7.0 RUNTIME update.");
							throw new Exception("Error in Web.Config, v7.0 RUNTIME update.");
						}

						//Now that that is taken care of, we need to remove the other, unneeded <runtime>.
						confNode.RemoveChild(nodes67[nodeToDelIdx]);
						madeChanges = true;

					}
					#endregion 6.7

					#region 6.7.1
					//Fixes issue with Password Expiration. 
					location = confNode.SelectSingleNode("location[@path=\"PasswordExpired.aspx\"]");
					if (location == null)
					{
						//Didn't already exist. Add it!
						/*
						 *	<system.web>
						 *		<authorization>
						 *			<allow users="?"/>
						 *		</authorization>
						 *	</system.web>
						 */
						XmlElement xmlAllow = xmlDocument.CreateElement("allow");
						xmlAllow.SetAttribute("users", "?");
						XmlElement xmlAuth = xmlDocument.CreateElement("authorization");
						xmlAuth.AppendChild(xmlAllow);
						XmlElement xmlSysW = xmlDocument.CreateElement("system.web");
						xmlSysW.AppendChild(xmlAuth);
						XmlElement xmlLoca = xmlDocument.CreateElement("location");
						xmlLoca.AppendChild(xmlSysW);
						xmlLoca.SetAttribute("path", "PasswordExpired.aspx");

						insertBefore = confNode.SelectSingleNode("location[@path=\"Services\"]");
						if (insertBefore != null)
							confNode.InsertBefore(xmlLoca, insertBefore);
						else
							confNode.AppendChild(xmlLoca);

						madeChanges = true;
					}

					//Removes no-longer-needed Mobile directory from needing Forms auth.
					location = confNode.SelectSingleNode("location[@path=\"Mobile/Login.aspx\"]");
					if (location != null)
					{
						confNode.RemoveChild(location);
						madeChanges = true;
					}
					#endregion 6.7.1

					#region 6.8
					//Add the two custom header controls, "X-Frame-Options" and "X-Content-Type-Options".
					var customHeaders = confNode.SelectSingleNode("system.webServer/httpProtocol/customHeaders");
					if (customHeaders != null)
					{
						//Make sure that it dosen't exist, first.
						var addFrameOpts = customHeaders.SelectSingleNode("add[@name='X-Frame-Options']");
						if (addFrameOpts == null)
						{
							XmlElement newFramOpts = xmlDocument.CreateElement("add");
							newFramOpts.SetAttribute("name", "X-Frame-Options");
							newFramOpts.SetAttribute("value", "DENY");

							customHeaders.AppendChild(newFramOpts);
							madeChanges = true;
						}

						//Now check the other one. 
						var addContentType = customHeaders.SelectSingleNode("add[@name='X-Content-Type-Options']");
						if (addContentType == null)
						{
							XmlElement newContentType = xmlDocument.CreateElement("add");
							newContentType.SetAttribute("name", "X-Content-Type-Options");
							newContentType.SetAttribute("value", "nosniff");

							customHeaders.AppendChild(newContentType);
							madeChanges = true;
						}
					}

					//Update the ViewStateEncryptionMode to 'Always'.
					var pageSettings = confNode.SelectSingleNode("system.web/pages");
					if (pageSettings != null)
					{
						//Change the 'viewStateEncryptionMode' from Never to Always.
						pageSettings.Attributes["viewStateEncryptionMode"].Value = "Always";
						madeChanges = true;
					}
					#endregion 6.8

					#region 6.9
					//Add the new Profile property "IsReportAdmin".
					var profileProp = xmlDocument.SelectSingleNode("configuration/system.web/profile/properties") as XmlElement;
					if (profileProp != null)
					{
						var profile = profileProp.SelectSingleNode("add[@name='IsReportAdmin']");
						if (profile == null)
						{
							//We need to add new property.
							XmlElement newContentType = xmlDocument.CreateElement("add");
							newContentType.SetAttribute("name", "IsReportAdmin");
							newContentType.SetAttribute("type", "Boolean");
							newContentType.SetAttribute("readOnly", "false");
							newContentType.SetAttribute("allowAnonymous", "false");

							profileProp.AppendChild(newContentType);
							madeChanges = true;
						}
					}

					//Add the new header 'Content-Security-Policy'.
					if (customHeaders != null && customHeaders.SelectSingleNode("add[@name='Content-Security-Policy']") == null)
					{
						XmlElement newContentType = xmlDocument.CreateElement("add");
						newContentType.SetAttribute("name", "Content-Security-Policy");
						newContentType.SetAttribute("value", "default-src 'self' data: ;script-src 'self' 'unsafe-inline' 'unsafe-eval' ;style-src 'self' 'unsafe-inline' data: ;frame-ancestors 'none' ; frame-src *;");
						customHeaders.AppendChild(newContentType);
						madeChanges = true;
					}

					//See if we need to add our assembly bindings.
					var configRuntime = xmlDocument.SelectSingleNode("configuration/runtime");
					bool foundBinding = false;
					bool updateBinding = false;
					if (configRuntime != null)
					{
						if (configRuntime.ChildNodes != null)
						{
							var looper = configRuntime.ChildNodes.GetEnumerator();
							while (looper.MoveNext() && !foundBinding)
							{
								//See if we have the 'assemblyBinding' node.
								XmlElement item = looper.Current as XmlElement;
								if (item != null)
								{
									if (item.Name.Equals("assemblyBinding"))
									{
										//We have an assembly binding. Let's make sure we have at LEAST the three we need.
										foundBinding = true;

										//Check the NewtonSoft binding override.
										updateBinding |= item.SelectSingleNode("aa:dependentAssembly/aa:assemblyIdentity[@name='Newtonsoft.Json']", bindMgr)?
											.ParentNode?
											.SelectSingleNode("aa:bindingRedirect[@newVersion='12.0.0.0']", bindMgr)
											== null;

										//Check the System.Net.Http.Formatting binding override.
										updateBinding |= item.SelectSingleNode("aa:dependentAssembly/aa:assemblyIdentity[@name='System.Net.Http.Formatting']", bindMgr)?
											.ParentNode?
											.SelectSingleNode("aa:bindingRedirect[@newVersion='5.2.7.0']", bindMgr)
											== null;

										//Check the System.Web.Http binding override.
										updateBinding |= item.SelectSingleNode("aa:dependentAssembly/aa:assemblyIdentity[@name='System.Web.Http']", bindMgr)?
											.ParentNode?
											.SelectSingleNode("aa:bindingRedirect[@newVersion='5.2.7.0']", bindMgr)
											== null;

										//If we have to update anything, we will just remove it all and create it all from scratch.
										if (updateBinding)
										{
											//Remove the whole thing.
											configRuntime.RemoveChild(item);
											//Set the flag.
											foundBinding = false;
											//Break our loop.
											break;
										}
									}
								}
							}
						}
					}

					//Now if we need to readd them.
					if (updateBinding || !foundBinding)
					{
						//Create it and create all its children.
						XmlElement assemblyProp1 = xmlDocument.CreateElement("assemblyBinding");
						assemblyProp1.SetAttribute("xmlns", "urn:schemas-microsoft-com:asm.v1");

						//Get the parent and add it as a chile.
						xmlDocument.SelectSingleNode("configuration/runtime").AppendChild(assemblyProp1);
						madeChanges = true;

						//Okay, recreate the three needed assemblies.
						assemblyProp1.AppendChild(createAssemblyBinding("Newtonsoft.Json", "30ad4fe6b2a6aeed", "12.0.0.0", xmlDocument));
						assemblyProp1.AppendChild(createAssemblyBinding("System.Net.Http.Formatting", "31bf3856ad364e35", "5.2.7.0", xmlDocument));
						assemblyProp1.AppendChild(createAssemblyBinding("System.Web.Http", "31bf3856ad364e35", "5.2.7.0", xmlDocument));

						madeChanges = true;
					}

					//Add the <location> tag for the ODATA API.
					//The OAuth Handler
					location = confNode.SelectSingleNode("location[@path=\"api/odata\"]");
					if (location == null)
					{
						/*Didn't already exist. Add it!
							<location path="api/odata">
								<system.web>
									<authorization>
										<allow users="?" />
									</authorization>
								</system.web>
							</location>
						 */
						XmlElement xmlAllow = xmlDocument.CreateElement("allow");
						xmlAllow.SetAttribute("users", "?");
						XmlElement xmlAuth = xmlDocument.CreateElement("authorization");
						xmlAuth.AppendChild(xmlAllow);
						XmlElement xmlSysW = xmlDocument.CreateElement("system.web");
						xmlSysW.AppendChild(xmlAuth);
						XmlElement xmlLoca = xmlDocument.CreateElement("location");
						xmlLoca.AppendChild(xmlSysW);
						xmlLoca.SetAttribute("path", "api/odata");

						//Find the one to insert before.
						insertBefore = confNode.SelectSingleNode("location[@path=\"Services\"]");
						if (insertBefore != null)
							confNode.InsertBefore(xmlLoca, insertBefore);
						else
							confNode.AppendChild(xmlLoca);

						madeChanges = true;
					}


					#endregion 6.9


					#region ValidationMaster 6.7
					/*
						* 		<!--Added for VM Scheduler-->
					<add key="TestRunReportId" value="1" />
					<add key="BatchDestination" value="SharePointOnLine" />
					<add key="DeliveryLocation" value="BatchExports" />
					<add key="SchedulerAPI" value="/VMReporting/api/ReportApi/CreateSchedule/" />

					<!--Added for VM Reporting-->
					<add key="outputFolder" value="C:\Program Files (x86)\ValidationMaster\Reporting\output\" />
					<add key="podFolder" value="C:\Program Files (x86)\ValidationMaster\reporting\podfiles\" />
					<add key="templateFolder" value="C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\" />

					<!--Added for TestMaster Activation-->
					<add key="RecorderLogin" value="&amp;RPdobAi*T29NbOk7tKz" />
					*/

					appSett = xmlDocument.SelectSingleNode("/configuration/appSettings") as XmlElement;
					if (appSett != null)
					{
						Console.Write("-- appSett --");
						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"TestRunReportId\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "TestRunReportId");
							newSet.SetAttribute("value", "1");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"BatchDestination\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "BatchDestination");
							newSet.SetAttribute("value", "SharePointOnLine");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"DeliveryLocation\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "DeliveryLocation");
							newSet.SetAttribute("value", "BatchExports");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"SchedulerAPI\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "SchedulerAPI");
							newSet.SetAttribute("value", "/VMReporting/api/ReportApi/CreateSchedule/");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"outputFolder\"]") == null)
						{
							Console.Write("-- outputFolder --");
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "outputFolder");
							newSet.SetAttribute("value", @"C:\Program Files (x86)\ValidationMaster\Reporting\output\");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"podFolder\"]") == null)
						{
							Console.Write("-- podFolder --");
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "podFolder");
							newSet.SetAttribute("value", @"C:\Program Files (x86)\ValidationMaster\reporting\podfiles\");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"templateFolder\"]") == null)
						{
							Console.Write("-- templateFolder --");
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "templateFolder");
							newSet.SetAttribute("value", @"C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"RecorderLogin\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "RecorderLogin");
							newSet.SetAttribute("value", "&amp;RPdobAi*T29NbOk7tKz");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"Customer\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "Customer");
							newSet.SetAttribute("value", "Default");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
					}

					#endregion

					#region ValidationMaster 7.0
					/*

					<!--Added for VM Reporting-->
					<add key="downloadablefilesFolder" value="C:\Program Files (x86)\ValidationMaster\reporting\downloadablefiles\" />

					*/

					appSett = xmlDocument.SelectSingleNode("/configuration/appSettings") as XmlElement;
					Console.Write(appSett + " Patcher App sett");
					if (appSett != null)
					{
						Console.Write(" Patcher App sett");
						//See if it already exists.
						if (appSett.SelectSingleNode("add[@key=\"downloadablefilesFolder\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "downloadablefilesFolder");
							newSet.SetAttribute("value", @"C:\Program Files (x86)\ValidationMaster\reporting\downloadablefiles\");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
						Console.Write("Check Risk Summary ReportName --");
						Console.Write(appSett.SelectSingleNode("add[@key=\"RiskSummaryReportName\"]"));
						if (appSett.SelectSingleNode("add[@key=\"RiskSummaryReportName\"]") == null)
						{
							Console.Write(" Risk Summary ReportName");
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "RiskSummaryReportName");
							newSet.SetAttribute("value", "New_Risk_Summary_Report");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}

						if (appSett.SelectSingleNode("add[@key=\"RiskSummaryImagePath\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "RiskSummaryImagePath");
							newSet.SetAttribute("value", @"C:\Program Files (x86)\ValidationMaster\Attachments\risksummary\");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
						if (appSett.SelectSingleNode("add[@key=\"Customer\"]") == null)
						{
							XmlElement newSet = xmlDocument.CreateElement("add");
							newSet.SetAttribute("key", "Customer");
							newSet.SetAttribute("value", "Default");

							//Add it..
							appSett.AppendChild(newSet);
							madeChanges = true;
						}
					}

					#endregion

					#region Generate File String
					//Here, we need to generate the string to ouput. Because the new format
					//  locks the file open, we need to geneate the string from the document,
					//  and then close the file/document, and write out the modified text.
					//  So here, we are going to get our string, if needed.
					if (madeChanges)
					{
						StringBuilder sb = new StringBuilder();
						using (StringWriter sw = new StringWriter(sb))
						{
							XmlTextWriter xtw = null;
							try
							{
								xtw = new XmlTextWriter(sw)
								{
									IndentChar = '\t',
									Indentation = 1,
									Formatting = Formatting.Indented,
								};

								//Send the file to the stringbuilder.
								xmlDocument.WriteTo(xtw);

								//Pull the formatted string from the string builder.
								fileToWrite = sb.ToString();
							}
							catch (Exception ex)
							{

							}
						}
					}
					#endregion Generate File String
				}
			}

			#region Save File
			//If we made changes, AND the generation of the XML didn't fail for some reason,
			//  write it out to our file.
			if (madeChanges && !string.IsNullOrWhiteSpace(fileToWrite))
			{
				try
				{
					//Write out the file.
					File.WriteAllText(filePath, fileToWrite);
					retValue = true;
				}
				catch (Exception ex)
				{
					retValue = false;
				}
			}
			#endregion Save File

			return retValue;
		}

		/// <summary>
		/// Deletes the existing version control caches, so that they are rebuilt post-upgrade
		/// </summary>
		private static void DeleteOldVersionCaches(SqlConnection sqlConnection, string siteName)
		{
			try
			{
				//Get the cache folder
				string cacheFolder = String.Format(DEFAULT_CACHE_FOLDER, siteName);

				//Now see if we have a folder specified in settings instead
				try
				{
					using (SqlCommand command = new SqlCommand())
					{
						//Open connection.
						command.Connection = sqlConnection;
						command.CommandType = CommandType.Text;
						command.CommandText = "SELECT [VALUE] FROM [TST_GLOBAL_SETTING] WHERE [NAME] = 'Cache_Folder'";
						object settingsLocation = command.ExecuteScalar();
						if (settingsLocation != null && settingsLocation is String)
						{
							string settingsLocation2 = (string)settingsLocation;
							if (!String.IsNullOrWhiteSpace(settingsLocation2))
							{
								cacheFolder = Path.Combine(settingsLocation2, "VersionControlCache");
							}
						}
					}
				}
				catch (Exception)
				{
					//Do nothing
				}

				//Get the files in there
				string[] files = System.IO.Directory.GetFiles(cacheFolder);
				foreach (string file in files)
				{
					if (file.EndsWith(".cache"))
					{
						System.IO.File.Delete(file);
					}
				}

				//Finally we need to delete all of the branches in the database as well
				using (SqlCommand command = new SqlCommand())
				{
					//Open connection.
					command.Connection = sqlConnection;
					command.CommandType = CommandType.Text;
					command.CommandText = "DELETE FROM [TST_VERSION_CONTROL_BRANCH]";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception)
			{
				//Fail quietly since the upgrade can proceed
			}
		}

		/// <summary>
		/// Used to create a new "dependentAssembly" XmlNode with children for the given assembly name.
		/// Note that the minVersion will always be "0.0.0.0", and culture will always be "neutral".
		/// </summary>
		/// <param name="name">The assembly name</param>
		/// <param name="publicKey">The assembly public key.</param>
		/// <param name="xmlDoc">The XML Documemnt, used to create our XmlElement</param>
		/// <param name="newVersion">The high/new version to redirect.</param>
		/// <returns>An XML node to be inserted into the document.</returns>
		private static XmlElement createAssemblyBinding(string name, string publicKey, string newVersion, XmlDocument xmlDoc)
		{
			//Create the two children.
			// - <assemblyIdentity>
			XmlElement assemblyIdentity = xmlDoc.CreateElement("assemblyIdentity");
			assemblyIdentity.SetAttribute("name", name);
			assemblyIdentity.SetAttribute("publicKeyToken", publicKey);
			assemblyIdentity.SetAttribute("culture", "neutral");

			// - <bindingRedirect>
			XmlElement bindingRedirect = xmlDoc.CreateElement("bindingRedirect");
			bindingRedirect.SetAttribute("newVersion", newVersion);
			bindingRedirect.SetAttribute("oldVersion", "0.0.0.0-" + newVersion);

			//Create our container and add the two kids.
			// - <dependentAssembly>
			XmlElement dependentAssembly = xmlDoc.CreateElement("dependentAssembly");
			dependentAssembly.AppendChild(assemblyIdentity);
			dependentAssembly.AppendChild(bindingRedirect);

			//Return it.
			return dependentAssembly;
		}

		/// <summary>
		/// Returns the connection string from the web.config file.
		/// </summary>
		/// <param name="webconfig"></param>
		/// <returns></returns>
		private static string getConnectionString(string webconfig)
		{
			string retStr = null;
			using (var xmlReader = openWebConfig(webconfig))
			{
				//Create & load the document.
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(xmlReader);

				//Locate the connection string 
				XmlElement connectNode = (XmlElement)xmlDocument.SelectSingleNode("configuration/connectionStrings/add[@name='SpiraTestEntities']");

				if (connectNode != null)
					retStr = connectNode.Attributes["connectionString"].Value;
			}

			return retStr;
		}

		/// <summary>
		/// Opens the speficied XML file and returns a reader ready to scan
		/// </summary>
		/// <param name="webconfig">The filepath of the XML file to open.</param>
		/// <returns>An XmlTextReader. Should be used in a using()</returns>
		private static XmlTextReader openWebConfig(string webconfig)
		{
			var xmlReader = new XmlTextReader(File.OpenRead(webconfig))
			{
				//Namespaces = false
			};

			return xmlReader;
		}
	}
}
