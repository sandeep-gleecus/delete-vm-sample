/* Fixes:
 *  - Report Format Name Changes [IN:5445]
 *  - Updated Stored Procs [IN:5373]
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade v630 to v640</summary>
	public class UpgradeTasks650 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 650;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 640;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 640;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks650(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in the constructor.
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

			//1 - Update Stored Procs
			_logger.WriteLine(loggerStr + "UpdateUsrCollection - Starting");
			UpdateUsrCollection();
			_logger.WriteLine(loggerStr + "UpdateUsrCollection - Finished");

			//2 - Update Report Names.
			_logger.WriteLine(loggerStr + "AddStaticColumns - Starting");
			AddStaticColumns();
			_logger.WriteLine(loggerStr + "AddStaticColumns - Finished");

			//3 - Update Artifact Fields.
			_logger.WriteLine(loggerStr + "UpdateArtFields - Starting");
			UpdateArtFields();
			_logger.WriteLine(loggerStr + "UpdateArtFields - Finished");

			//4 - Update Administrator User Profile.
			_logger.WriteLine(loggerStr + "UpdateAdminUser - Starting");
			UpdateAdminUser();
			_logger.WriteLine(loggerStr + "UpdateAdminUser - Finished");

			//5 - Update Administrator User Profile.
			_logger.WriteLine(loggerStr + "UpdateFKs - Starting");
			UpdateFKs();
			_logger.WriteLine(loggerStr + "UpdateFKs - Finished");

			//6 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

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

		#region Private SQL Functions
		/// <summary>Updates the names of some existing standard report formats. See [TK:1628]</summary>
		private void UpdateUsrCollection()
		{
			//Update the progress bar..
			UpdateProgress();

			//Get the SQL
			string sqlRun = ZipReader.GetContents("DB_v650.zip", "AddUserCollection.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlRun);
		}

		/// <summary>Updates the names of some existing standard report formats. See [TK:1628]</summary>
		private void AddStaticColumns()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add columns to the TST_RELEASE table.
			SQLUtilities.AddDecimalColumn(_connection, "TST_RELEASE", "PLANNED_POINTS", 9, 1);
			SQLUtilities.AddDecimalColumn(_connection, "TST_RELEASE", "REQUIREMENT_POINTS", 9, 1);
			SQLUtilities.AddInt32Column(_connection, "TST_RELEASE", "REQUIREMENT_COUNT", 0);

			//Add columsn to the TST_PROJECT table.
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT", "REQUIREMENT_COUNT", 0);

			//Add columsn to the TST_PROJECT_GROUP table.
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT_GROUP", "REQUIREMENT_COUNT", 0);

			//Add columsn to the TST_PORTFOLIO table.
			SQLUtilities.AddInt32Column(_connection, "TST_PORTFOLIO", "REQUIREMENT_COUNT", 0);
		}

		/// <summary>Updates the names of some existing standard report formats. See [TK:1628]</summary>
		private void UpdateArtFields()
		{
			//Update the progress bar..
			UpdateProgress();

			string sqlQuery5 = ZipReader.GetContents("DB_v650.zip", "UpdateArtFields.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery5);
		}

		/// <summary>Updates the names of some existing standard report formats. See [TK:1628]</summary>
		private void UpdateAdminUser()
		{
			//Update the progress bar..
			UpdateProgress();

			string sqlQuery5 = "UPDATE TST_USER_PROFILE SET IS_PORTFOLIO_ADMIN = 1 WHERE [USER_ID] = 1";
			SQLUtilities.ExecuteCommand(_connection, sqlQuery5);
		}

		/// <summary>Updates the names of some existing standard report formats. See [TK:1628]</summary>
		private void UpdateFKs()
		{
			//Update the progress bar..
			UpdateProgress();

			// For logging.
			int numUpdated = 0;

			//Need to get a list of our ImportanceIds and Names based on Template IDs. first.
			Dictionary<int, List<int>> importanceIds = new Dictionary<int, List<int>>();
			Dictionary<int, string> importanceNames = new Dictionary<int, string>();
			using (var dsImpId = SQLUtilities.GetDataQuery(_connection, "SELECT [PROJECT_TEMPLATE_ID], [IMPORTANCE_ID], [NAME] FROM [TST_IMPORTANCE] ORDER BY [PROJECT_TEMPLATE_ID];"))
			{
				foreach (DataRow dr in dsImpId.Tables[0].Rows)
				{
					int? tmplId = dr[0] as int?;
					int? impId = dr[1] as int?;
					string implName = dr[2] as string;

					//If the Template ID does not already exist, create it.
					if (tmplId.HasValue && impId.HasValue)
					{
						if (!importanceIds.ContainsKey(tmplId.Value))
							importanceIds.Add(tmplId.Value, new List<int>());

						//Add this Importance ID to the list.
						importanceIds[tmplId.Value].Add(impId.Value);
					}

					//Get the name.
					if (!importanceNames.ContainsKey(impId.Value))
						importanceNames.Add(impId.Value, implName);
				}
			}

			//Now get our Project ID and Template ID mapping.
			Dictionary<int, List<int>> projectMapping = new Dictionary<int, List<int>>();
			using (var dsProjId = SQLUtilities.GetDataQuery(_connection, "SELECT [PROJECT_TEMPLATE_ID], [PROJECT_ID] FROM [TST_PROJECT] ORDER BY [PROJECT_TEMPLATE_ID];"))
			{
				foreach (DataRow dr in dsProjId.Tables[0].Rows)
				{
					//Pull the values from the table.
					int? projId = dr[1] as int?;
					int? tmplId = dr[0] as int?;

					//If the Template ID does not already exist, create it.
					if (tmplId.HasValue && projId.HasValue)
					{
						if (!projectMapping.ContainsKey(tmplId.Value))
							projectMapping.Add(tmplId.Value, new List<int>());

						//Add this Importance ID to the list.
						projectMapping[tmplId.Value].Add(projId.Value);
					}
				}
			}

			//Loop through each template and pull the requirements that may have a wrong ID.
			foreach (KeyValuePair<int, List<int>> projList in projectMapping)
			{
				string inProject = string.Join(",", projList.Value); //String used for Projed List.
				string inTemplate = string.Join(",", importanceIds[projList.Key]); //String used for Importaqnces.

				//The SQL to return Requirement IDs
				string query = "SELECT [REQUIREMENT_ID], [IMPORTANCE_ID] " +
					"FROM [TST_REQUIREMENT] " +
					"WHERE [PROJECT_ID] IN (" + inProject + ") AND [IMPORTANCE_ID] NOT IN (" + inTemplate + ");";

				using (var dsReqs = SQLUtilities.GetDataQuery(_connection, query))
				{
					//Loop through each of the Requiremetns.
					foreach (DataRow dr in dsReqs.Tables[0].Rows)
					{
						//Get the IDs..
						int? reqId = dr[0] as int?;
						int? intId = dr[1] as int?;

						if (reqId.HasValue && intId.HasValue)
						{
							//Get the string of the current importance.
							string reqImpName = null;
							if (importanceNames.ContainsKey(intId.Value))
								reqImpName = importanceNames[intId.Value];

							//If that's not null, find a match.
							int newImpId = 0;
							if (!string.IsNullOrWhiteSpace(reqImpName))
							{
								//Find a matching Importance. (We take the list, filter it by all the names that match, select only the ID,
								//  and convert that back into a list.
								List<int> matchingNames = importanceNames.Where(g => g.Value.Equals(reqImpName)).Select(h => h.Key).ToList();

								//Now find an importance with the mathcing name in the proper template.
								newImpId = importanceIds[projList.Key].SingleOrDefault(f => matchingNames.Any(g => g == f));
							}

							//Set the update string.
							string updString = ((newImpId > 0) ? newImpId.ToString() : "NULL");

							//Now run the command.
							string updateSql = "UPDATE [TST_REQUIREMENT] " +
								"SET [IMPORTANCE_ID] = " + updString + " " +
								"WHERE [REQUIREMENT_ID] = " + reqId.Value + ";";

							SQLUtilities.ExecuteCommand(_connection, updateSql);
							numUpdated++;
						}
					}
				}
			}

			//Log it.
			string logMsg = "";
			if (numUpdated > 0)
				logMsg = "Corrected " + numUpdated.ToString() + " records.";
			else
				logMsg = "No records updated.";
			App.logFile.WriteLine(logMsg);
		}
		#endregion Private SQL Functions
	}
}

