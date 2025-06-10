/* Fixes/Adds:
 * - [TK:2341] - FK was missing "On Delete, Cascade"
 * - [RQ:2873] - Reportable Entity for baselines.
 * - Update Incident 'Resolved Release' to 'Planned Release'
 * - Update Incident Report - same reason as above.
 * - Add Project Collection entries for Baseline Sorted Grid setting storage.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 651 to 652</summary>
	public class UpgradeTasks652 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 652;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 650;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 651;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks652(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in this call.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 6; } }

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

			//1 - Update FK Cascade.
			_logger.WriteLine(loggerStr + "UpdateFKCascade - Starting");
			UpdateFKCascade();
			_logger.WriteLine(loggerStr + "UpdateFKCascade - Finished");

			//2 - Add new View.
			_logger.WriteLine(loggerStr + "AddReportView - Starting");
			AddReportView();
			_logger.WriteLine(loggerStr + "AddReportView - Finished");

			//3 - Add new View.
			_logger.WriteLine(loggerStr + "InsertPrCollection - Starting");
			InsertPrCollection();
			_logger.WriteLine(loggerStr + "InsertPrCollection - Finished");

			//4 - Update Artifact Field.
			_logger.WriteLine(loggerStr + "UpdateArtField - Starting");
			UpdateArtField();
			_logger.WriteLine(loggerStr + "UpdateArtField - Finished");

			//5 - Update Artifact Field.
			_logger.WriteLine(loggerStr + "UpdateRptSection - Starting");
			UpdateRptSection();
			_logger.WriteLine(loggerStr + "UpdateRptSection - Finished");

			//6 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

		#region Private SQL Functions
		/// <summary>Updates the IncidentDetails report section to fix the name from 'Resolved Release' to 'Planned Release'.</summary>
		private void UpdateRptSection()
		{
			//Update the progress bar..
			UpdateProgress();

			//Get the report contents.
			string newReport = ZipReader.GetContents("DB_v652.zip", "reportSection_IncidentDetails.sql");

			//Execute it.
			SQLUtilities.ExecuteCommand(_connection, newReport);
		}

		/// <summary>Updates the Artifact Field table for row #8 - 'Resolved Release' -> 'Planned Release'.</summary>
		private void UpdateArtField()
		{
			//Update the progress bar..
			UpdateProgress();

			string sqlUpd = "UPDATE [TST_ARTIFACT_FIELD] SET [CAPTION] = 'Planned Release' WHERE [ARTIFACT_FIELD_ID] = 8;";
			SQLUtilities.ExecuteCommand(_connection, sqlUpd);
		}

		/// <summary>Adds the needed Project Colelctions for storing Basline Grid settings.</summary>
		private void InsertPrCollection()
		{
			//Update the progress bar..
			UpdateProgress();

			//The keys we need to add.
			List<string> keys = new List<string>
			{
				"Baseline.Filters",
				"Baseline.SortExpression",
				"Baseline.Pagination"
			};

			//Loop through each one - if it already exists, fantastic. If not, add it.
			foreach (var key in keys)
			{
				string sqlCheck = "SELECT COUNT(*) FROM [TST_PROJECT_COLLECTION] WHERE [NAME] = '" + key + "';";
				var checkValue = SQLUtilities.GetDataQuery(_connection, sqlCheck);

				//Check the result here.
				var ans = Convert.ToInt16(checkValue.Tables[0].Rows[0][0]);
				if (ans < 1)
				{
					//Add the setting.
					string sqlCreate = "INSERT INTO [TST_PROJECT_COLLECTION] VALUES ('" + key + "','Y');";
					SQLUtilities.ExecuteCommand(_connection, sqlCreate);
				}
			}
		}

		/// <summary>Corrects the FK between History Changeset table and Baseline table. See [TK:2341]</summary>
		private void UpdateFKCascade()
		{
			//Update the progress bar..
			UpdateProgress();

			//Run our commands.
			string sql = ZipReader.GetContents("DB_v652.zip", "fixfk.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}

		/// <summary>Adds the report view for Baselines. See [RQ:2873]</summary>
		private void AddReportView()
		{
			//Update the progress bar..
			UpdateProgress();

			//Run our commands.
			string sql = ZipReader.GetContents("DB_v652.zip", "rpt_baselines.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sql);
		}
		#endregion Private SQL Functions

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
