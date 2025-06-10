/**
 * DB Revision 672 is for Spira v6.8.0.0
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 671 to 672</summary>
	public class UpgradeTasks672 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		private string SQL_COL_1 = "ALTER TABLE [{0}] ADD [{1}] BIT CONSTRAINT [DEF_{0}_{1}] DEFAULT (0) NOT NULL;";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 672;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 671;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 671;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks672(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in this call.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 14; } }

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

			//1 - Add new tables.
			_logger.WriteLine(loggerStr + "AddNewTables - Starting");
			AddNewTables();
			_logger.WriteLine(loggerStr + "AddNewTables - Finished");

			//2 - Add new Static Data.
			_logger.WriteLine(loggerStr + "AddStaticData - Starting");
			AddStaticData();
			_logger.WriteLine(loggerStr + "AddStaticData - Finished");

			//3 - Add columns to Workflow Tables.
			_logger.WriteLine(loggerStr + "WkFlColumns - Starting");
			WkFlColumns();
			_logger.WriteLine(loggerStr + "WkFlColumns - Finished");

			//4 - Add columns to Workflow Tables.
			_logger.WriteLine(loggerStr + "UsrPrfColumn - Starting");
			UsrPrfColumn();
			_logger.WriteLine(loggerStr + "UsrPrfColumn - Finished");

			//5 - Update the Time Card tables. 
			_logger.WriteLine(loggerStr + "TimeCrdCols - Starting");
			TimeCrdCols();
			_logger.WriteLine(loggerStr + "TimeCrdCols - Finished");

			//6 - Update IncidentList report.
			_logger.WriteLine(loggerStr + "UpdateRpt - Starting");
			UpdateRpt();
			_logger.WriteLine(loggerStr + "UpdateRpt - Finished");

			//7 - Add new Custom Property fields.
			_logger.WriteLine(loggerStr + "UpdateCustProp - Starting");
			UpdateCustProp();
			_logger.WriteLine(loggerStr + "UpdateCustProp - Finished");

			//8 - Update Global Filetypes
			_logger.WriteLine(loggerStr + "UpdateFiletypes - Starting");
			UpdateFiletypes();
			_logger.WriteLine(loggerStr + "UpdateFiletypes - Finished");

			//9 - Non-FK general columns added.
			_logger.WriteLine(loggerStr + "AddNewCols1 - Starting");
			AddNewCols1();
			_logger.WriteLine(loggerStr + "AddNewCols1 - Finished");

			//10 - FK-Related columns added.
			_logger.WriteLine(loggerStr + "AddNewCols2 - Starting");
			AddNewCols2();
			_logger.WriteLine(loggerStr + "AddNewCols2 - Finished");

			//11 - Updated the Report Available Section table fields.
			_logger.WriteLine(loggerStr + "UpdRptSection - Starting");
			UpdRptSection();
			_logger.WriteLine(loggerStr + "UpdRptSection - Finished");

			//12 - Update Build table indexes.
			_logger.WriteLine(loggerStr + "UpdBuildIdx - Starting");
			UpdBuildIdx();
			_logger.WriteLine(loggerStr + "UpdBuildIdx - Finished");

			//13 - Update the indexes on the TST_TEST_RUN table.
			_logger.WriteLine(loggerStr + "UpdTRIdx - Starting");
			UpdTRIdx();
			_logger.WriteLine(loggerStr + "UpdTRIdx - Finished");

			//13 - Update the indexes on the TST_TEST_RUN table.
			_logger.WriteLine(loggerStr + "UpdTRPIdx - Starting");
			UpdTRPIdx();
			_logger.WriteLine(loggerStr + "UpdTRPIdx - Finished");

			//14 - Fix issue with TST_HISTORY_POSITION missing identity
			_logger.WriteLine(loggerStr + "FixHistoryPositionIdentity - Starting");
			FixHistoryPositionIdentity();
			_logger.WriteLine(loggerStr + "FixHistoryPositionIdentity - Finished");

			//15 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

		#region Internal SQL Functions
		/// <summary>Creates a whole bunch of new tables.</summary>
		private void AddNewTables()
		{
			//Update the progress bar..
			UpdateProgress();

			//We have all table generation in a SQL file.
			string sql = ZipReader.GetContents("DB_v672.zip", "NewTables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Inserts new Static Data on two of our tables.</summary>
		private void AddStaticData()
		{
			//Update the progress bar..
			UpdateProgress();

			//We have all table generation in a SQL file.
			string sql = ZipReader.GetContents("DB_v672.zip", "InsertNewStatic.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Adds columns to our workflow tables.</summary>
		private void WkFlColumns()
		{
			//Update the progress bar..
			UpdateProgress();

			List<string> tables = new List<string>
			{
				"TST_RISK_WORKFLOW_TRANSITION",
				"TST_DOCUMENT_WORKFLOW_TRANSITION",
				"TST_REQUIREMENT_WORKFLOW_TRANSITION",
				"TST_TEST_CASE_WORKFLOW_TRANSITION",
				"TST_RELEASE_WORKFLOW_TRANSITION",
				"TST_TASK_WORKFLOW_TRANSITION"
			};
			List<string> bitFields = new List<string>
			{
				"IS_BLANK_OWNER",
				"IS_NOTIFY_CREATOR",
				"IS_NOTIFY_OWNER"
			};

			//Loop through each table and add columns.
			foreach (var table in tables)
			{
				//Add the three new BIT fields.
				foreach (var col in bitFields)
				{
					SQLUtilities.AddBitColumn(_connection, table, col, false);
				}

				//Now add the last field.
				SQLUtilities.AddStringColumn(_connection, table, "NOTIFY_SUBJECT", 40);
			}

			//Handle the Incident Workflow table [TST_WORKFLOW_TRANSITION] that only needs the one column added. 
			string sqlToRun2 = string.Format(SQL_COL_1,
				"TST_WORKFLOW_TRANSITION",
				bitFields[0]);
			SQLUtilities.ExecuteCommand(_connection, sqlToRun2);
		}

		/// <summary>Adds new column to the TST_USER_PROFILE table.</summary>
		private void UsrPrfColumn()
		{
			//Update the progress bar..
			UpdateProgress();

			string sqlToRun = string.Format(SQL_COL_1,
				"TST_USER_PROFILE",
				"IS_REPORT_ADMIN");
			SQLUtilities.ExecuteCommand(_connection, sqlToRun);
		}

		/// <summary>Adds / Changes fields to the TimeCard tables.</summary>
		private void TimeCrdCols()
		{
			//Update the progress bar..
			UpdateProgress();

			//First, add the 'DESCRIPTION' column to the TST_TIMECARD_ENTRY and TST_TIMECARD_ENTRY_TYPE tables.
			foreach (var table in new string[] { "TST_TIMECARD_ENTRY_TYPE", "TST_TIMECARD_ENTRY" })
				SQLUtilities.AddStringColumn(_connection, table, "DESCRIPTION", null);

			//Now, changes to the TST_TIMECARD table.
			// - First, remove the IS_APPROVED column.
			SQLUtilities.DropColumn(_connection, "TST_TIMECARD", "IS_APPROVED");

			// - Add new colums - APPROVER_USER_ID, TIMECARD_STATUS_ID, PPROVER_COMMENTS
			SQLUtilities.AddInt32Column(_connection, "TST_TIMECARD", "APPROVER_USER_ID");
			SQLUtilities.AddInt32Column(_connection, "TST_TIMECARD", "TIMECARD_STATUS_ID");
			SQLUtilities.AddStringColumn(_connection, "TST_TIMECARD", "APPROVER_COMMENTS", null);

			// - Rename the USER_ID column to SUBMITTER_USER_ID
			string sqlRen = "sp_rename 'TST_TIMECARD.USER_ID', 'SUBMITTER_USER_ID', 'COLUMN';"; //This dosen't update any views, but DOES update indexes.
			SQLUtilities.ExecuteCommand(_connection, sqlRen);

			// - Upgrade the legnth of the COMMENTS field.
			string sqlUpd = "ALTER TABLE [TST_TIMECARD] ALTER COLUMN [COMMENTS] NVARCHAR(max);";
			SQLUtilities.ExecuteCommand(_connection, sqlUpd);

			// - Now add the new indexes for the Foreign keys.
			SQLUtilities.AddIndex(_connection, "IDX_TST_TIMECARD_2_FK", "TST_TIMECARD", "TIMECARD_STATUS_ID");
			SQLUtilities.AddIndex(_connection, "IDX_TST_TIMECARD_3_FK", "TST_TIMECARD", "APPROVER_USER_ID");

			// - Now add the Foreign Keys.
			SQLUtilities.AddForeignKey(_connection, "FK_TST_TIMECARD_STATUS_TST_TIMECARD", "TST_TIMECARD_STATUS", "TIMECARD_STATUS_ID", "TST_TIMECARD", "TIMECARD_STATUS_ID");
			SQLUtilities.AddForeignKey(_connection, "FK_TST_USER_TST_TIMECARD_APPROVER", "TST_USER", "USER_ID", "TST_TIMECARD", "APPROVER_USER_ID");
		}

		/// <summary>
		/// Update the IncidentList report's content.
		/// </summary>
		private void UpdateRpt()
		{
			//Update the progress bar..
			UpdateProgress();

			//Load the XML into memory.
			string newXML = ZipReader.GetContents("DB_v672.zip", "reportSection_IncidentList.xml");
			string sql1 = "UPDATE [TST_REPORT_SECTION] SET [DEFAULT_TEMPLATE] = '"
				+ SQLUtilities.SqlEncode(newXML)
				+ "' WHERE [TOKEN] = 'IncidentList';";
			SQLUtilities.ExecuteCommand(_connection, sql1);
		}

		/// <summary>
		/// Update the File Icons for Markdown types.
		/// </summary>
		private void UpdateFiletypes()
		{
			//Update the progress bar..
			UpdateProgress();

			//Update the Global Filetype table -
			//  All "text/markdown" entries get their icon changed to "Markdown.svg".
			string sql = "UPDATE [TST_GLOBAL_FILETYPES] SET [FILETYPE_ICON] = 'Markdown.svg' WHERE [FILETYPE_MIME] = 'text/markdown';";
			SQLUtilities.ExecuteCommand(_connection, sql);
			//  All "text/x-feature" entries get their icon changed to "Feature.svg".
			sql = "UPDATE [TST_GLOBAL_FILETYPES] SET [FILETYPE_ICON] = 'Feature.svg' WHERE [FILETYPE_MIME] = 'text/x-feature';";
			SQLUtilities.ExecuteCommand(_connection, sql);
		}

		/// <summary>
		/// Add Custom Property fields.
		/// </summary>
		private void UpdateCustProp()
		{
			//Update the progress bar..
			UpdateProgress();

			//The template SQL.
			string sqlTmpl = "ALTER TABLE [{0}] ADD {1};";
			string sqlColTmpl = "[CUST_{0}] NVARCHAR(max), ";

			string sqlColCompl = "";

			//Loop through.
			for (int i = 31; i <= 99; i++)
			{
				sqlColCompl += string.Format(sqlColTmpl,
					i.ToString());
			}
			//Trim off the extra ','.
			sqlColCompl = sqlColCompl.Trim(new char[] { ',', ' ' });

			//Make the full SQL.
			string sqlToRun = string.Format(sqlTmpl,
				"TST_ARTIFACT_CUSTOM_PROPERTY",
				sqlColCompl);
			SQLUtilities.ExecuteCommand(_connection, sqlToRun);

			//Now add the 'Position' field to the TST_CUSTOM_PROPERTY table.
			SQLUtilities.AddInt32Column(_connection, "TST_CUSTOM_PROPERTY", "POSITION");
		}

		/// <summary>
		/// Adds other general new columns that do NOT have any foreign keys lonked to them.
		/// This includes the Custom Property and Requirement tables.
		/// </summary>
		private void AddNewCols1()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add 'POSITION' to the TST_GLOBAL_CUSTOM_PROPERTIES table.
			SQLUtilities.AddInt32Column(_connection, "TST_GLOBAL_CUSTOM_PROPERTY", "POSITION");

			//ADD 'IS_SUSPECT' to TST_REQUIREMENT table.
			SQLUtilities.AddBitColumn(_connection, "TST_REQUIREMENT", "IS_SUSPECT", false);
		}

		/// <summary>
		/// Adds other general new columns that have a foreign key with them.
		/// This is for the TST_REPORT_CATEGORY, RICK tables.
		/// </summary>
		private void AddNewCols2()
		{
			//Update the progress bar..
			UpdateProgress();

			// TST_RISK - Risk Detectability.
			SQLUtilities.AddInt32Column(_connection, "TST_RISK", "RISK_DETECTABILITY_ID");
			SQLUtilities.AddForeignKey(
				_connection,
				"FK_TST_RISK_DETECTABILITY_TST_RISK",
				"TST_RISK_DETECTABILITY",
				"RISK_DETECTABILITY_ID",
				"TST_RISK",
				"RISK_DETECTABILITY_ID");

			// TST_REPORT_CATEGORY
			SQLUtilities.AddInt32Column(_connection, "TST_REPORT_CATEGORY", "WORKSPACE_TYPE_ID", 1); // 1: Product/Project.
			SQLUtilities.AddForeignKey(
				_connection,
				"FK_TST_WORKSPACE_TYPE_TST_REPORT_CATEGORY",
				"TST_WORKSPACE_TYPE",
				"WORKSPACE_TYPE_ID",
				"TST_REPORT_CATEGORY",
				"WORKSPACE_TYPE_ID");
		}

		/// <summary>Adds new Primary Key Column to TST_REPORT_AVAILABLE_SAECTION</summary>
		private void UpdRptSection()
		{
			//Update the progress bar..
			UpdateProgress();

			//First, add the new column. 
			// TODO: On SQL 2019, this row was autopopulated with values from the existing primary key. See if this will work as expected on earlier versions of SQL Server.
			string sqlAdd = "ALTER TABLE [TST_REPORT_AVAILABLE_SECTION] ADD [REPORT_AVAILABLE_SECTION_ID] INTEGER IDENTITY(1,1) NOT NULL;";
			SQLUtilities.ExecuteCommand(_connection, sqlAdd);

			//Now, remove the existing old primary key.
			string sqlRem = "ALTER TABLE [TST_REPORT_AVAILABLE_SECTION] DROP CONSTRAINT [PK_TST_REPORT_AVAILABLE_SECTION];";
			SQLUtilities.ExecuteCommand(_connection, sqlRem);

			//And finally, create the new clustered index.
			string sqlIdx = "ALTER TABLE [TST_REPORT_AVAILABLE_SECTION] ADD CONSTRAINT[PK_TST_REPORT_AVAILABLE_SECTION] PRIMARY KEY CLUSTERED([REPORT_AVAILABLE_SECTION_ID]);";
			SQLUtilities.ExecuteCommand(_connection, sqlIdx);
		}

		/// <summary>Updates the indexes on the TST_BUILD table.</summary>
		private void UpdBuildIdx()
		{
			//Update the progress bar..
			UpdateProgress();

			//Remove the indexes, first.
			foreach (string idx in new string[] { "AK_TST_BUILD_3", "AK_TST_BUILD_2", "AK_TST_BUILD_1" })
			{
				string sqlIdx = "DROP INDEX [TST_BUILD].[" + idx + "]";
				SQLUtilities.ExecuteCommand(_connection, sqlIdx);
			}

			//Now create the new index.
			List<SQLUtilities.ColumnDirection> colsList = new List<SQLUtilities.ColumnDirection>
			{
				new SQLUtilities.ColumnDirection() { ColumnName = "RELEASE_ID", Descending = false },
				new SQLUtilities.ColumnDirection() { ColumnName = "BUILD_STATUS_ID", Descending = false },
				new SQLUtilities.ColumnDirection() { ColumnName = "PROJECT_ID", Descending = false },
				new SQLUtilities.ColumnDirection() { ColumnName = "CREATION_DATE", Descending = true }
			};

			SQLUtilities.AddIndex(_connection, "AK_TST_BUILD_1", "TST_BUILD", colsList);
		}

		/// <summary>
		/// Recreates indexes #3 and #5 on the TST_TEST_RUN table.
		/// </summary>
		private void UpdTRIdx()
		{
			//Update the progress bar..
			UpdateProgress();

			//Drop index #3.
			SQLUtilities.DropIndex(_connection, "AK_TST_TEST_RUN_3", "TST_TEST_RUN");

			//Recreate #3.
			List<SQLUtilities.ColumnDirection> colsList = new List<SQLUtilities.ColumnDirection>
			{
				new SQLUtilities.ColumnDirection() { ColumnName = "TEST_CASE_ID", Descending = false },
				new SQLUtilities.ColumnDirection() { ColumnName = "END_DATE", Descending = true }
			};
			SQLUtilities.AddIndex(_connection, "AK_TST_TEST_RUN_3", "TST_TEST_RUN", colsList);

			//Drop index #5.
			SQLUtilities.DropIndex(_connection, "AK_TST_TEST_RUN_5", "TST_TEST_RUN");

			//Recreate #5.
			// - Use the previous column list, since #5 shares the first two fields.
			colsList.Add(new SQLUtilities.ColumnDirection() { ColumnName = "RELEASE_ID", Descending = false });
			SQLUtilities.AddIndex(_connection, "AK_TST_TEST_RUN_5", "TST_TEST_RUN", colsList);
		}

		/// <summary>
		/// FixHistoryPositionIdentity
		/// </summary>
		private void FixHistoryPositionIdentity()
		{
			string commands =
@"
IF (OBJECTPROPERTY(OBJECT_ID('TST_HISTORY_POSITION'), 'TableHasIdentity') = 0)
BEGIN
	ALTER TABLE [TST_HISTORY_POSITION] DROP CONSTRAINT [PK_TST_HISTORY_POSITION];
	ALTER TABLE [TST_HISTORY_POSITION] ADD [HISTORY_POSITION_ID_TEMP] BIGINT IDENTITY(1,1) NOT NULL;
	ALTER TABLE [TST_HISTORY_POSITION] DROP COLUMN [HISTORY_POSITION_ID];
	EXEC sp_rename 'TST_HISTORY_POSITION.HISTORY_POSITION_ID_TEMP', 'HISTORY_POSITION_ID', 'COLUMN';
	ALTER TABLE [TST_HISTORY_POSITION] ADD CONSTRAINT [PK_TST_HISTORY_POSITION] PRIMARY KEY ([HISTORY_POSITION_ID]);
END
";
			SQLUtilities.ExecuteSqlCommands(_connection, commands);
		}

		/// <summary>Updates an index on the TST_TEST_RUNS_PENDING table.</summary>
		private void UpdTRPIdx()
		{
			//Update the progress bar..
			UpdateProgress();

			//Drop index #3.
			SQLUtilities.DropIndex(_connection, "AK_TST_TEST_RUNS_PENDING_2", "TST_TEST_RUNS_PENDING");

			//Recreate #3.
			List<SQLUtilities.ColumnDirection> colsList = new List<SQLUtilities.ColumnDirection>
			{
				new SQLUtilities.ColumnDirection() { ColumnName = "TESTER_ID", Descending = false },
				new SQLUtilities.ColumnDirection() { ColumnName = "CREATION_DATE", Descending = true }
			};
			SQLUtilities.AddIndex(_connection, "AK_TST_TEST_RUNS_PENDING_2", "TST_TEST_RUNS_PENDING", colsList);
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
