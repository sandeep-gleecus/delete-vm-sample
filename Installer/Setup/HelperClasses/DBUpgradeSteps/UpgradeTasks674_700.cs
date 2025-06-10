using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 674 to 700</summary>
	public class UpgradeTasks700 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 700;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 674;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 700;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks700(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in this call.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 5; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMin { get { return DB_UPG_MIN; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMax { get { return DB_UPG_MAX; } }

		/// <summary>The location for this upgrade class's database files.</summary>
		/// <remarks>No files needed, so return a null.</remarks>
		public string DatabaseFilePath { get { return null; } }

		/// <summary>Do the database upgrade!</summary>
		/// <returns>tatus of update.</returns>
		public bool UpgradeDB(DBConnection ConnectionInfo, Action<object> ProgressOverride, int curNum, float totNum)
		{
			//Save the event.
			ProgressHandler = ProgressOverride;
			_connection = ConnectionInfo;
			_totJob = totNum;

			//1 - Add new column to the TST_RELEASE table.
			_logger.WriteLine(loggerStr + "UpdateReleaseTable - Starting");
			UpdateReleaseTable();
			_logger.WriteLine(loggerStr + "UpdateReleaseTable - Finished");

			//2 - Add a section to one of the reports.
			_logger.WriteLine(loggerStr + "UpdateReportTable - Starting");
			UpdateReportTable();
			_logger.WriteLine(loggerStr + "UpdateReportTable - Finished");

			//3 Update ReportSaved Table
			_logger.WriteLine(loggerStr + "UpdateReportSavedTable - Starting");
			UpdateReportSavedTable();
			_logger.WriteLine(loggerStr + "UpdateReportSavedTable - Finished");

			//4 - Add the new tables to the system.
			_logger.WriteLine(loggerStr + "AddNewTables - Starting");
			AddNewTables();
			_logger.WriteLine(loggerStr + "AddNewTables - Finished");

			//_logger.WriteLine(loggerStr + "AddNewScripts - Starting");
			//AddNewScripts();
			//_logger.WriteLine(loggerStr + "AddNewScripts - Finished");

			//5 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");			

			return true;
		}
		#endregion Interface Functions

		#region Internal SQL Functions

		/// <summary>Add new Source Code tables.</summary>
		public void AddNewTables()
		{
			//Update the progress bar..
			UpdateProgress();

			//Create the new tables.
			string sql = ZipReader.GetContents("DB_v700.zip", "newtables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		    sql = ZipReader.GetContents("DB_v700.zip", "StagingScriptUpdate.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
			sql = ZipReader.GetContents("DB_v700.zip", "AuditTrailDBScripts.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Add new Source Code tables.</summary>
		//public void AddNewScripts()
		//{
		//	//Update the progress bar..
		//	UpdateProgress();

		//	//Create the new tables.
		//	string sql = ZipReader.GetContents("StagingScriptUpdate.zip", "StagingScriptUpdate.sql");
		//	SQLUtilities.ExecuteSqlCommands(_connection, sql);
		//}

		/// <summary>Adds a field to the Release table for the Source Code update.</summary>
		public void UpdateReleaseTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add the '[PERIODIC_REVIEW_ALERT_ID]' field to the TST_RELEASE table.
			string sql = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TST_RELEASE' AND COLUMN_NAME = 'PERIODIC_REVIEW_ALERT_ID') BEGIN ALTER TABLE TST_RELEASE ADD PERIODIC_REVIEW_ALERT_ID int NULL END;";
			SQLUtilities.ExecuteCommand(_connection, sql);

			//Add the '[PERIODIC_REVIEW_DATE]' field to the TST_RELEASE table.
			string sql1 = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TST_RELEASE' AND COLUMN_NAME = 'PERIODIC_REVIEW_DATE') BEGIN ALTER TABLE TST_RELEASE ADD PERIODIC_REVIEW_DATE datetime NULL END;";
			SQLUtilities.ExecuteCommand(_connection, sql1);

			//Add the '[TEST_CASE_PREPARATION_STATUS_ID]' field to the TST_TEST_CASE table.
			//string sql2 = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TST_TEST_CASE' AND COLUMN_NAME = 'TEST_CASE_PREPARATION_STATUS_ID') BEGIN ALTER TABLE TST_TEST_CASE ADD TEST_CASE_PREPARATION_STATUS_ID int NULL END";
			//SQLUtilities.ExecuteCommand(_connection, sql2);

			//string sql3 = "IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TST_TEST_CASE_PREPARATION_STATUS') BEGIN ALTER TABLE[dbo].[TST_TEST_CASE] WITH CHECK ADD CONSTRAINT[FK_TST_TEST_CASE_PREPARATION_STATUS] FOREIGN KEY([TST_TEST_CASE_PREPARATION_STATUS_ID]) REFERENCES[dbo].[TEST_CASE_PREPARATION_STATUS]([TST_TEST_CASE_PREPARATION_STATUS_ID]) END";
			//SQLUtilities.ExecuteCommand(_connection, sql3);
			//SQLUtilities.AddForeignKey(_connection, "FK_TST_TEST_CASE_PREPARATION_STATUS_ID", "TST_TEST_CASE_PREPARATION_STATUS", "TEST_CASE_PREPARATION_STATUS_ID", "TST_TEST_CASE", "TEST_CASE_ID");


		}

		/// <summary>Inserts new section into report, and updates XSLT report.</summary>
		public void UpdateReportTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//See if we even need to, first. //sarvari - update the values
			string sqlQuery = "SELECT COUNT(*) FROM [TST_REPORT_SECTION_ELEMENT] WHERE [REPORT_SECTION_ID]=3 AND [REPORT_ELEMENT_ID]=6;";
			DataSet query = SQLUtilities.GetDataQuery(_connection, sqlQuery);
			if (query.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				query.Tables[0].Rows[0][0] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)query.Tables[0].Rows[0][0] < 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//sarvari - update the values
				string sql1 = "INSERT INTO [TST_REPORT_SECTION_ELEMENT] ([REPORT_SECTION_ID], [REPORT_ELEMENT_ID]) VALUES (3, 6);";
				SQLUtilities.ExecuteCommand(_connection, sql1);
			}
		}

		/// <summary>Adds a field to the ReportSaved table for the Source Code update.</summary>
		public void UpdateReportSavedTable()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add the '[CREATION_DATE]' field to the TST_REPORT_SAVED table.
			string sql = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TST_REPORT_SAVED' AND COLUMN_NAME = 'CREATION_DATE') BEGIN ALTER TABLE TST_REPORT_SAVED ADD CREATION_DATE DATETIME NULL END;";
			SQLUtilities.ExecuteCommand(_connection,  sql);
		}

		#endregion Internal SQL Functions

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
