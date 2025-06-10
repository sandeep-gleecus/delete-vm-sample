using System;
using System.Data;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 661 to 670</summary>
	public class UpgradeTasks670 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 670;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 661;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 661;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks670(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in this call.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 7; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMin { get { return DB_UPG_MIN; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMax { get { return DB_UPG_MAX; } }

		/// <summary>The location for this upgrade class's database files.</summary>
		/// <remarks>No files needed, so return a null.</remarks>
		public string DatabaseFilePath { get { return null; } }

		/// <summary>Do the database upgrade!</summary>
		/// <returns>status of update.</returns>
		public bool UpgradeDB(DBConnection ConnectionInfo, Action<object> ProgressOverride, int curNum, float totNum)
		{
			//Save the event.
			ProgressHandler = ProgressOverride;
			_connection = ConnectionInfo;
			_totJob = totNum;

			//1 - Add Filetypes!
			_logger.WriteLine(loggerStr + "AddFiletypes - Starting");
			AddFileTypes();
			_logger.WriteLine(loggerStr + "AddFiletypes - Finished");

			//2 - Add the new tables to the system.
			_logger.WriteLine(loggerStr + "AddNewTables - Starting");
			AddNewTables();
			_logger.WriteLine(loggerStr + "AddNewTables - Finished");

			//3 - Add new column to the TST_RELEASE table.
			_logger.WriteLine(loggerStr + "UpdateReleaseTable - Starting");
			UpdateReleaseTable();
			_logger.WriteLine(loggerStr + "UpdateReleaseTable - Finished");

			//4 - Make the changes needed for the TST_HISTORY_POSITION table.
			_logger.WriteLine(loggerStr + "UpdateHistoryTable - Starting");
			UpdateHistoryTable();
			_logger.WriteLine(loggerStr + "UpdateHistoryTable - Finished");

			//5 - Add a section to one of the reports.
			_logger.WriteLine(loggerStr + "UpdateReportTable - Starting");
			UpdateReportTable();
			_logger.WriteLine(loggerStr + "UpdateReportTable - Finished");

			//6 - Add a section to one of the reports.
			_logger.WriteLine(loggerStr + "UpdateReportXLST - Starting");
			UpdateReportXLST();
			_logger.WriteLine(loggerStr + "UpdateReportXLST - Finished");

			//7 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			//8 - Delete the old version control file caches
			_logger.WriteLine(loggerStr + "DeleteOldVersionCaches - Starting");
			DeleteOldVersionCaches(ConnectionInfo);
			_logger.WriteLine(loggerStr + "DeleteOldVersionCaches - Finished");

			return true;
		}
		#endregion Interface Functions

		#region Private SQL Functions
		/// <summary>Add new filetypes needed (YAML, Markdown, SVG, Batch, TypeSctrip)</summary>
		public void AddFileTypes()
		{
			//Update the progress bar..
			UpdateProgress();

			//Insert the new filetypes.
			string sql = ZipReader.GetContents("DB_v670.zip", "filetypes.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Add new Source Code tables.</summary>
		public void AddNewTables()
		{
			//Update the progress bar..
			UpdateProgress();

			//Create the new tables.
			string sql = ZipReader.GetContents("DB_v670.zip", "newtables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Adds a field to the Release table for the Source Code update.</summary>
		public void UpdateReleaseTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add the 'BRANCH_ID' field to the TST_RELEASE table.
			SQLUtilities.AddInt32Column(_connection, "TST_RELEASE", "BRANCH_ID");
		}

		/// <summary>Updates the TST_HISTORY_POSITION table.</summary>
		public void UpdateHistoryTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//Remove the FK that exists and depends on the column next.
			string sql1 = "IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_TST_ARTIFACT_TYPE_TST_HISTORY_POSITION_PARENT') > 0 ";
			sql1 += "ALTER TABLE [TST_HISTORY_POSITION] DROP CONSTRAINT [FK_TST_ARTIFACT_TYPE_TST_HISTORY_POSITION_PARENT]";
			SQLUtilities.ExecuteCommand(_connection, sql1);

			//Column 'PARENT_ARTIFACT_TYPE_ID' renamed to 'CHILD_ARTIFACT_ID'
			string sql2 = "EXEC sp_rename N'TST_HISTORY_POSITION.PARENT_ARTIFACT_TYPE_ID', N'CHILD_ARTIFACT_ID', 'COLUMN';";
			SQLUtilities.ExecuteCommand(_connection, sql2);

			//Remove index 'IDX_TST_HISTORY_POSITION_2_FK'.
			string sql3 = "DROP INDEX [TST_HISTORY_POSITION].[IDX_TST_HISTORY_POSITION_2_FK];";
			SQLUtilities.ExecuteCommand(_connection, sql3);

			//Column 'HISTORY_POSITION_ID', add IDENTITY(1,1). Since this cannot be added, we drop existing column, add new one.
			//  - This can be done now, as, as of 6.6.1, there are no entries in the table!
			string sql4 = "ALTER TABLE [TST_HISTORY_POSITION] DROP CONSTRAINT [PK_TST_HISTORY_POSITION];";
			SQLUtilities.ExecuteCommand(_connection, sql4);
			string sql5 = "ALTER TABLE [TST_HISTORY_POSITION] DROP COLUMN [HISTORY_POSITION_ID];";
			SQLUtilities.ExecuteCommand(_connection, sql5);
			string sql6 = "ALTER TABLE [TST_HISTORY_POSITION] ADD [HISTORY_POSITION_ID] [bigint] IDENTITY(1,1) NOT NULL;";
			SQLUtilities.ExecuteCommand(_connection, sql6);
			string sql7 = "ALTER TABLE [TST_HISTORY_POSITION] ADD CONSTRAINT [PK_TST_HISTORY_POSITION] PRIMARY KEY CLUSTERED ([HISTORY_POSITION_ID] ASC);";
			SQLUtilities.ExecuteCommand(_connection, sql7);
		}

		/// <summary>Inserts new section into report, and updates XSLT report.</summary>
		public void UpdateReportTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//See if we even need to, first.
			string sqlQuery = "SELECT COUNT(*) FROM [TST_REPORT_SECTION_ELEMENT] WHERE [REPORT_SECTION_ID]=3 AND [REPORT_ELEMENT_ID]=6;";
			DataSet query = SQLUtilities.GetDataQuery(_connection, sqlQuery);
			if (query.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				query.Tables[0].Rows[0][0] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)query.Tables[0].Rows[0][0] < 1)              //And the value is 1 - it ALLOWS nulls.
			{
				string sql1 = "INSERT INTO [TST_REPORT_SECTION_ELEMENT] ([REPORT_SECTION_ID], [REPORT_ELEMENT_ID]) VALUES (3, 6);";
				SQLUtilities.ExecuteCommand(_connection, sql1);
			}
		}

		/// <summary>Updates a static generic report's output.</summary>
		public void UpdateReportXLST()
		{
			//Update the progress bar..
			UpdateProgress();

			//Load the XML into memory.
			string newXML = ZipReader.GetContents("DB_v670.zip", "newReqDetails.xml");
			string sql1 = "UPDATE [TST_REPORT_SECTION] SET [DEFAULT_TEMPLATE] = '"
				+ SQLUtilities.SqlEncode(newXML)
				+ "' WHERE [TOKEN] = 'RequirementDetails';";
			SQLUtilities.ExecuteCommand(_connection, sql1);
		}
		#endregion Private SQL Functions

		#region Private Other Functions

		/// <summary>
		/// Deletes the existing version control caches, so that they are rebuilt post-upgrade
		/// </summary>
		private void DeleteOldVersionCaches(DBConnection conninfo)
		{
			try
			{
				//Get the default folder location
				string cacheFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Inflectra", "Spira", "VersionControlCache");

				//Now see if we have a folder specified in settings instead
				object settingsLocation = SQLUtilities.ExecuteScalarCommand(conninfo, "SELECT [VALUE] FROM [TST_GLOBAL_SETTING] WHERE [NAME] = 'Cache_Folder'");
				if (settingsLocation != null && settingsLocation is String)
				{
					string settingsLocation2 = (string)settingsLocation;
					if (!String.IsNullOrWhiteSpace(settingsLocation2))
					{
						cacheFolder = Path.Combine(settingsLocation2, "VersionControlCache");
					}
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
				SQLUtilities.ExecuteCommand(conninfo, "DELETE FROM [TST_VERSION_CONTROL_BRANCH]");
			}
			catch (Exception exception)
			{
				//Fail quietly since the upgrade can proceed
				_logger.WriteLine("*Warning*: Unable to delete the old v6.6 version control cache files: " + exception.Message);
			}
		}

		#endregion

		/// <summary>Checks that our database is truly v5.4 before we attempt to upgrade it.. </summary>
		/// <returns>True if the database is upgradeable</returns>
		public bool VerifyDatabaseIsCorrectVersionToUpgrade(DBConnection conninfo, StreamWriter streamWriter)
		{
			int dbRev = 0;
			try
			{
				//Get the reported DB version, first.
				dbRev = SQLUtilities.GetExistingDBRevision(conninfo);
			}
			catch (Exception exception)
			{
				streamWriter.WriteLine(
					"Unable to determine if database can be upgraded:" +
					Environment.NewLine +
					Logger.DecodeException(exception));
			}

			//Return our value.
			return (dbRev >= DB_UPG_MIN && dbRev <= DB_UPG_MAX);
		}
	}
}
