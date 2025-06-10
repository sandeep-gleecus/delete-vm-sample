/*
 * - Added an Cascade Delete to History Changeset -> History POsition table.
 * - Removed the UNIQUE constraint from index on Requirement Step table.
 * - Removed unused FK off of History POsition table.
 * - Added 30 new files to Global Filetype table.
 * - Updated one entry in same table.
 * - Increased legnth to LOGIN/PASSWORD fields in both Version Control for system & project.
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 670 to 671</summary>
	public class UpgradeTasks671 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 671;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 670;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 670;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks671(
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
		/// <returns>tatus of update.</returns>
		public bool UpgradeDB(DBConnection ConnectionInfo, Action<object> ProgressOverride, int curNum, float totNum)
		{
			//Save the event.
			ProgressHandler = ProgressOverride;
			_connection = ConnectionInfo;
			_totJob = totNum;

			//1 - Fix the TST_HISTORY_POSITION table.
			_logger.WriteLine(loggerStr + "CorrectHistoryTable - Starting");
			CorrectHistoryTable();
			_logger.WriteLine(loggerStr + "CorrectHistoryTable - Finished");

			//2 - Add new filetypes, and correct the Photoshop one.
			_logger.WriteLine(loggerStr + "UpdateFileTypes - Starting");
			UpdateFileTypes();
			_logger.WriteLine(loggerStr + "UpdateFileTypes - Finished");

			//3 - Add new settings for Source Control to the Project Collection table.
			_logger.WriteLine(loggerStr + "UpdateProjCollection - Starting");
			UpdateProjCollection();
			_logger.WriteLine(loggerStr + "UpdateProjCollection - Finished");

			//4 - Update the Version Control tables to increase max legnth on LOGIN/PASS fields.
			_logger.WriteLine(loggerStr + "AdjustVersionControl - Starting");
			AdjustVersionControl();
			_logger.WriteLine(loggerStr + "AdjustVersionControl - Finished");

			//5 - Remove the UNIQUE constraint on the Requirement Step Position table.
			_logger.WriteLine(loggerStr + "UpdateReqTable - Starting");
			UpdateReqTable();
			_logger.WriteLine(loggerStr + "UpdateReqTable - Finished");

			//6 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

		#region Private SQL Functions
		/// <summary>Handles updating a couple History tables. (2 events.)</summary>
		private void CorrectHistoryTable()
		{
			//Update the progress bar..
			UpdateProgress();
			// First, we are updating [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION] to add ON DROP CASCADE.
			string sql = "IF (OBJECT_ID('FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION', 'F') IS NOT NULL) " +
				"BEGIN ALTER TABLE [TST_HISTORY_POSITION] DROP CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION] END";
			SQLUtilities.ExecuteCommand(_connection, sql);
			sql = "ALTER TABLE [TST_HISTORY_POSITION] ADD CONSTRAINT [FK_TST_HISTORY_CHANGESET_TST_HISTORY_POSITION] " +
				"FOREIGN KEY([CHANGESET_ID]) REFERENCES[TST_HISTORY_CHANGESET]([CHANGESET_ID]) ON DELETE CASCADE";
			SQLUtilities.ExecuteCommand(_connection, sql);

			//Update the progress bar..
			UpdateProgress();
			// Completely remove [IDX_TST_HISTORY_POSITION_3_FK] FK, as it is not needed any more due to field changed made in 670.
			sql = "IF (EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_TST_HISTORY_POSITION_3_FK')) " +
				"BEGIN DROP INDEX [IDX_TST_HISTORY_POSITION_3_FK] ON [TST_HISTORY_POSITION] END";
			SQLUtilities.ExecuteCommand(_connection, sql);
		}

		/// <summary>Adds new files to the Filetypes table.</summary>
		private void UpdateFileTypes()
		{
			//Update the progress bar..
			UpdateProgress();

			//We have this stored in an SQL file.
			string sql = ZipReader.GetContents("DB_v671.zip", "GlobalFileTypes.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);

		}

		/// <summary>Adds new settings to Project Collections table.</summary>
		private void UpdateProjCollection()
		{
			//Update the progress bar..
			UpdateProgress();

			//We have this stored in an SQL file.
			string sql = ZipReader.GetContents("DB_v671.zip", "ProjCollection.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Increases field legnth in Version Control tables.</summary>
		private void AdjustVersionControl()
		{
			//Update the progress bar..
			UpdateProgress();

			//Loop through each table.
			foreach (var table in new List<string> { "TST_VERSION_CONTROL_SYSTEM", "TST_VERSION_CONTROL_PROJECT" })
			{
				//And for each table, looop through the two columns.
				foreach (var column in new List<string> { "LOGIN", "PASSWORD" })
				{
					// Generate the SQL.
					string sql = string.Format("ALTER TABLE [{0}] ALTER COLUMN [{1}] nvarchar(255) {2}NULL; ",
						table,
						column,
						(table.Contains("SYSTEM")) ? "NOT " : "");
					// And execute it.
					SQLUtilities.ExecuteCommand(_connection, sql);
				}
			}

		}

		/// <summary>Removes the UNIQUE constraint from AK_TST_REQUIREMENT_STEP_POSITION index.</summary>
		private void UpdateReqTable()
		{
			//Update the progress bar..
			UpdateProgress();

			string sql = ZipReader.GetContents("DB_v671.zip", "RemoveUnique.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}
		#endregion  Private SQL Functions

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
