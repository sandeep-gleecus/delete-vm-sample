// Fixes:
//  - https://spira.inflectra.com/6/Incident/5201.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade v620 to v621</summary>
	public class UpgradeTasks621 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 621;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 620;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 620;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks621(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in the constructor.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 4; } }

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

			//1 - Fix bad Missing FK data.
			_logger.WriteLine(loggerStr + "FixMissingFKDatas - Starting");
			FixMissingFKDatas(ConnectionInfo);
			_logger.WriteLine(loggerStr + "FixMissingFKDatas - Finished");

			//2 - Regenerate FKs.
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Starting");
			CreateForeignKeys(ConnectionInfo);
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Finished");

			//3 - Regenerate FKs.
			_logger.WriteLine(loggerStr + "UpdateStoredProcs - Starting");
			UpdateStoredProcs(ConnectionInfo);
			_logger.WriteLine(loggerStr + "UpdateStoredProcs - Finished");

			//4 - Update DB revision.
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

		#region Private DB Upgrade Functions
		/// <summary>Creates the new foreign keys</summary>
		/// <remarks>See: https://spira.inflectra.com/6/Incident/5201.aspx </remarks>
		private void CreateForeignKeys(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			//Get the SQL File. 
			string sqlQuery1 = ZipReader.GetContents("DB_v600-v621.zip");

			//Now continue only if we have a SQL to run. 
			if (!string.IsNullOrWhiteSpace(sqlQuery1))
			{
				try
				{
					SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);
				}
				catch (Exception ex)
				{
					// Could not execute file without throwing an error. 
					_logger.WriteLine(loggerStr + "Error running recreating foreign keys:" + Environment.NewLine + Logger.DecodeException(ex));
				}
			}
		}

		/// <summary>Creates the new foreign keys</summary>
		/// <remarks>See: https://spira.inflectra.com/6/Incident/5201.aspx </remarks>
		private void UpdateStoredProcs(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			//Get the SQL File.
			// - DO nothing here now. This is done after the install is finished.
		}

		/// <summary>
		/// Fixed bad data in the database for missing FKs. This includes manually deleting data 
		/// that a FK had 'ON DELETE CASCADE'  and setting proper columns to NULL for FKs that 
		/// had 'ON DELETE SET NULL'.
		/// </summary>
		/// <param name="conninfo"></param>
		public void FixMissingFKDatas(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			// Our strings needed to remove any data that shouldn't be there.

			// When there's a single possiblity...
			//  {0} - The table removing from.
			//  {1} - The table we're checking against.
			//  {2} - The deleting table's key field.
			//  {3} - The check table's key field. (Usually is the same as {2}.
			string SQLdel1 = "DELETE FROM [{0}] WHERE [{2}] NOT IN (SELECT [{3}] FROM [{1}]);";

			//  {0} - Table to Update
			//  {1} - Table to reference from
			//  {2} - Field to Update
			//  {3} - Field to reference from (usually same as {1})
			string SQLnull = "UPDATE [{0}] SET [{2}] = null WHERE [{2}] NOT IN (SELECT [{3}] FROM [{1}]);";

			//  {0} - Table to Update
			//  {1} - Table to reference from
			//  {2} - Field to Update
			//  {3} - Field to reference from (usually same as {1})
			//  {4} - The value to set the field to. (The 'defaul' value.
			string SQLdef = "UPDATE [{0}] SET [{2}] = {4} WHERE [{2}] NOT IN (SELECT [{3}] FROM [{1}]);";

			#region Delete Calls

			//Commenting our USER_ID FK calls since they are unique to one customer and can be handled in SQL

			/*
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
	"TST_USER_PAGE_VIEWED",
	"TST_USER",
	"USER_ID",
	"USER_ID")
);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TVAULT_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_MESSAGE_ARTIFACT",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_IDEA",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_ALLOCATION_ACTUAL",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TIMECARD",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TIMECARD_ENTRY",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_BASELINE",
				"TST_USER",
				"CREATOR_USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_MESSAGE_TRACK",
				"TST_USER",
				"SENDER_USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_MULTI_APPROVER_EXECUTED",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REPORT_GENERATED",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel2,
				"TST_MESSAGE",
				"TST_USER",
				"SENDER_USER_ID",
				"USER_ID",
				"TST_USER",
				"RECIPIENT_USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_CONTACT",
				"TST_USER",
				"CONTACT_USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_COLLECTION_ENTRY",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUNS_PENDING",
				"TST_USER",
				"TESTER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_USER_MAPPING",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_GROUP_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DASHBOARD_USER_PERSONALIZATION",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REPORT_SAVED",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_COLLECTION_ENTRY",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
*/

			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_SET",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_ARTIFACT_MAPPING",
				"TST_DATA_SYNC_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_ARTIFACT_MAPPING",
				"TST_DATA_SYNC_PROJECT",
				"DATA_SYNC_SYSTEM_ID",
				"DATA_SYNC_SYSTEM_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_TRANSITION_ROLE",
				"TST_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_TRANSITION",
				"TST_INCIDENT_STATUS",
				"INPUT_INCIDENT_STATUS_ID",
				"INCIDENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_TRANSITION",
				"TST_WORKFLOW",
				"WORKFLOW_ID",
				"WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_CUSTOM_PROPERTY",
				"TST_INCIDENT_STATUS",
				"INCIDENT_STATUS_ID",
				"INCIDENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_CUSTOM_PROPERTY",
				"TST_WORKFLOW",
				"WORKFLOW_ID",
				"WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_CUSTOM_PROPERTY_VALUE",
				"TST_CUSTOM_PROPERTY_LIST",
				"CUSTOM_PROPERTY_LIST_ID",
				"CUSTOM_PROPERTY_LIST_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_PROJECT_ROLE",
				"TST_PROJECT_ROLE",
				"PROJECT_ROLE_ID",
				"PROJECT_ROLE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING",
				"TST_DATA_SYNC_PROJECT",
				"DATA_SYNC_SYSTEM_ID",
				"DATA_SYNC_SYSTEM_ID")
			);

			/*
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_ARTIFACT_FIELD",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_CUSTOM_PROPERTY",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_ARTIFACT_LINK",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_RESOLUTION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_TEST_CASE",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_SAVED_FILTER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_USER_SUBSCRIPTION",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_PROFILE",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_PROFILE",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_TEMPLATE_USER",
				"TST_USER",
				"USER_ID",
				"USER_ID")
			);*/

			#endregion Delete Calls

			#region Set Null Calls

			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_REQUIREMENT",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_USER",
				"TST_GLOBAL_OAUTH_PROVIDERS",
				"OAUTH_PROVIDER_ID",
				"OAUTH_PROVIDER_ID")
			);

			/*
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_SET",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TASK",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_INCIDENT",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_REQUIREMENT",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_RELEASE",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_CASE",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_RISK",
				"TST_USER",
				"OWNER_ID",
				"USER_ID")
			);*/

			#endregion Set Null Calls

			#region Set Default Calls

			/*
            SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_HISTORY_CHANGESET",
				"TST_USER",
				"USER_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TASK",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_INCIDENT",
				"TST_USER",
				"OPENER_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_ATTACHMENT",
				"TST_USER",
				"AUTHOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_ATTACHMENT",
				"TST_USER",
				"EDITOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_REQUIREMENT",
				"TST_USER",
				"AUTHOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TEST_RUN",
				"TST_USER",
				"TESTER_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_RELEASE",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TEST_CASE",
				"TST_USER",
				"AUTHOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TEST_SET",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_ATTACHMENT_VERSION",
				"TST_USER",
				"AUTHOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_REQUIREMENT_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_RELEASE_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TEST_CASE_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TEST_SET_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_TASK_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_USER_CONTACT",
				"TST_USER",
				"CREATOR_USER_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_DOCUMENT_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_RISK_DISCUSSION",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdef,
				"TST_RISK",
				"TST_USER",
				"CREATOR_ID",
				"USER_ID",
				"1")
			);*/

			#endregion Set Default Calls
		}
		#endregion
	}
}

