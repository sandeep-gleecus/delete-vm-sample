/* Performs the following:
 * - Adds 6 rows to the Project Collection table for needed Baseline Filter saving/sorting/pagination
 * - Adds row to the ArtifactField table for Release 'Planned Points' field.
 * - Adds Folder Hierarchy tables for performance issues.
 * - Populates above tables with initial data.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 652 to 660</summary>
	public class UpgradeTasks660 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 660;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 652;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 652;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks660(
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

			//1 - Insert new Project Collection
			_logger.WriteLine(loggerStr + "InsertPrCollection - Starting");
			InsertPrCollection();
			_logger.WriteLine(loggerStr + "InsertPrCollection - Finished");

			//2 - Add ArtifactField "PlannedPoints"
			_logger.WriteLine(loggerStr + "InsertArtFields - Starting");
			InsertArtFields();
			_logger.WriteLine(loggerStr + "InsertArtFields - Finished");

			//3 - Add Folder Hierarchy Cache tables.
			_logger.WriteLine(loggerStr + "AddFolderCacheTables - Starting");
			AddFolderCacheTables();
			_logger.WriteLine(loggerStr + "AddFolderCacheTables - Finished");

			//4 - Update/Populate the Folder Hierarchy Cache tables.
			_logger.WriteLine(loggerStr + "UpdateFolderCacheTables - Starting");
			UpdateFolderCacheTables();
			_logger.WriteLine(loggerStr + "UpdateFolderCacheTables - Finished");

			//4 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

		/// <summary>Adds the needed Project Colelctions for storing Basline Grid settings.</summary>
		private void InsertPrCollection()
		{
			//Update the progress bar..
			UpdateProgress();

			//The keys we need to add.
			List<string> keys = new List<string>
			{
				"Admin.Baseline.Filters",
				"Admin.Baseline.SortExpression",
				"Admin.Baseline.Pagination",
				"Artifact.Baseline.Filters",
				"Artifact.Baseline.SortExpression",
				"Artifact.Baseline.Pagination"
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

		/// <summary>Inserts the row for the field 'Planned Points' for Requirements.</summary>
		private void InsertArtFields()
		{
			//Update the progress bar..
			UpdateProgress();


			string sqlToRun = ZipReader.GetContents("DB_v660.zip", "artfields.sql");
			SQLUtilities.ExecuteCommand(_connection, sqlToRun);
		}

		/// <summary>Creates the Cache Tables in the database.</summary>
		private void AddFolderCacheTables()
		{
			//Update the progress bar..
			UpdateProgress();


			string sqlToRun = ZipReader.GetContents("DB_v660.zip", "createtables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlToRun);
		}

		/// <summary>Create the stored Proc and then run it for each project.</summary>
		private void UpdateFolderCacheTables()
		{
			//Update the progress bar..
			UpdateProgress();

			_logger.WriteLine("Installing procs..");
			//Create the stored procs. (Yes, we do this later, however, it also then means all this code
			//  would be in the main thread class, which my OCD just .. can't allow! :)
			// - attachment_refresh_folder_hierarchy.sql
			List<string> funsCreate = new List<string>
			{
				"fn_global_create_indent_level.sql",
				"attachment_refresh_folder_hierarchy.sql",
				"task_refresh_folder_hierarchy.sql",
				"testcase_refresh_folder_hierarchy.sql",
				"testcase_refresh_parameter_hierarchy.sql",
				"testset_refresh_folder_hierarchy.sql"
			};
			foreach (string name in funsCreate)
			{
				string sql1 = ZipReader.GetContents("DB_v660.zip", name);
				SQLUtilities.ExecuteSqlCommands(_connection, sql1);
			}

			//Get a list of all our project IDs.
			string sqlPrj = "SELECT DISTINCT [project_id] FROM [TST_PROJECT];";
			var dataSet = SQLUtilities.GetDataQuery(_connection, sqlPrj);

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

				_logger.WriteLine("Running procs for Project #" + projId.ToString() + "... (" + DateTime.Now.ToString("HH:mm:ss.ffff") + ")");

				foreach (var sqltmp in procNames)
				{
					string sqlExec = "EXEC [" + sqltmp + "] @ProjectId = " + projId.ToString();
					SQLUtilities.ExecuteCommand(_connection, sqlExec);
				}
			}

			//Log a done! and time, just to keep track.
			_logger.WriteLine("Done! (" + DateTime.Now.ToString("HH:mm:ss.ffff") + ")");
		}
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
