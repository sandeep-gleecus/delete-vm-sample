using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade v600 or v610 to v620</summary>
	public class UpgradeTasks620 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 620;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 600;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 610;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks620(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in the constructor.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 8; } }

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


			//1 - Fix Null Fields
			_logger.WriteLine(loggerStr + "FixNullColumns - Starting");
			FixNullColumns(ConnectionInfo);
			_logger.WriteLine(loggerStr + "FixNullColumns - Finished");

			//2 - Fix Missing Index
			_logger.WriteLine(loggerStr + "FixMissingDefault - Starting");
			FixMissingDefault(ConnectionInfo);
			_logger.WriteLine(loggerStr + "FixMissingDefault - Finished");

			//3 - Delete existing FK's, if they're there.
			_logger.WriteLine(loggerStr + "DeleteExistingForeignKeyConstraints - Starting");
			UpdateProgress();
			SQLUtilities.DeleteExistingForeignKeyConstraints(ConnectionInfo);
			_logger.WriteLine(loggerStr + "DeleteExistingForeignKeyConstraints - Finished");

			//4 - Fix bad Missing FK data.
			_logger.WriteLine(loggerStr + "FixMissingFKDatas - Starting");
			FixMissingFKDatas(ConnectionInfo);
			_logger.WriteLine(loggerStr + "FixMissingFKDatas - Finished");

			//5 - Regenerate FKs.
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Starting");
			CreateForeignKeys(ConnectionInfo);
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Finished");

			//6 - Fix Column Size
			_logger.WriteLine(loggerStr + "FixColumnSize - Starting");
			FixColumnSize(ConnectionInfo);
			_logger.WriteLine(loggerStr + "FixColumnSize - Finished");

			//7 - Update static data.
			_logger.WriteLine(loggerStr + "UpdateStaticData - Starting");
			UpdateStaticData(ConnectionInfo);
			_logger.WriteLine(loggerStr + "UpdateStaticData - Finished");

			//8 - Update DB revision.
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
		/// <summary>Fixes the 'NULL' columns that were not set 'NOT NULL' on upgraded-only DBs.</summary>
		public void FixNullColumns(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			//We have to look at each one manually, due to indexes.

			//The template strins needed to perform work.
			//  - {0} Table
			//  - {1} Field
			//  - {2} Index
			string sqltmpSelectField = "SELECT COLUMNPROPERTY(OBJECT_ID('{0}', 'U'), '{1}', 'AllowsNull') AS [isnull]";
			string sqltmpFixField = "ALTER TABLE [{0}] ADD DEFAULT (0) FOR [{1}]; ALTER TABLE [{0}] ALTER COLUMN [{1}] INT NOT NULL";
			string sqltmpCreateIndex = "CREATE INDEX [{2}] ON [{0}] ([{1}])";
			string sqltmpDropIndex = "IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'{2}') DROP INDEX [{2}] ON [{0}]";

			//We need to, for each table, see if it is NULLABLE or not, and if so, perform some work.
			#region Table TST_IMPORTANCE
			string sqlTable = "TST_IMPORTANCE", sqlField = "PROJECT_TEMPLATE_ID";
			DataSet check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_IMPORTANCE_1_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_IMPORTANCE_1_FK"));
			}
			#endregion Table TST_IMPORTANCE

			#region Table TST_PROJECT_ATTACHMENT
			sqlTable = "TST_PROJECT_ATTACHMENT"; sqlField = "DOCUMENT_TYPE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "AK_TST_PROJECT_ATTACHMENT_3"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "AK_TST_PROJECT_ATTACHMENT_3"));
			}
			#endregion Table TST_PROJECT_ATTACHMENT

			#region Table TST_TASK_PRIORITY
			sqlTable = "TST_TASK_PRIORITY"; sqlField = "PROJECT_TEMPLATE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TASK_PRIORITY_1_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TASK_PRIORITY_1_FK"));
			}
			#endregion Table TST_TASK_PRIORITY

			#region Table TST_TASK_TYPE
			// - TST_TASK_TYPE
			sqlTable = "TST_TASK_TYPE"; sqlField = "PROJECT_TEMPLATE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TASK_TYPE_1_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TASK_TYPE_1_FK"));
			}
			sqlTable = "TST_TASK_TYPE"; sqlField = "TASK_WORKFLOW_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TASK_TYPE_2_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TASK_TYPE_2_FK"));
			}
			#endregion Table TST_TASK_TYPE

			#region Table TST_TASK_WORKFLOW
			sqlTable = "TST_TASK_WORKFLOW"; sqlField = "PROJECT_TEMPLATE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TASK_WORKFLOW_1_FK"));
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "AK_TST_TASK_WORKFLOW_1"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TASK_WORKFLOW_1_FK"));
			}
			#endregion Table TST_TASK_WORKFLOW

			#region Table TST_TEST_CASE_PRIORITY
			sqlTable = "TST_TEST_CASE_PRIORITY"; sqlField = "PROJECT_TEMPLATE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_PRIORITY_1_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_PRIORITY_1_FK"));
			}
			#endregion Table TST_TEST_CASE_PRIORITY

			#region Table TST_TEST_CASE_TYPE
			sqlTable = "TST_TEST_CASE_TYPE"; sqlField = "PROJECT_TEMPLATE_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_TYPE_1_FK"));
				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_TYPE_1_FK"));
			}
			sqlTable = "TST_TEST_CASE_TYPE"; sqlField = "TEST_CASE_WORKFLOW_ID";
			check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
				);
			if (check.Tables[0].Rows.Count > 0 &&                         //We have at least one row.
				check.Tables[0].Rows[0]["isnull"] != DBNull.Value &&      //The value is NOT a null. (i.e. Column was found.)
				(int)check.Tables[0].Rows[0]["isnull"] == 1)              //And the value is 1 - it ALLOWS nulls.
			{
				//Drop the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpDropIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_TYPE_2_FK"));
				//Fix bad/missing entries. (This is required for the bad 'Security' Test Case Type.)
				string updSql = @"UPDATE t1
	SET t1.[TEST_CASE_WORKFLOW_ID] = (
		SELECT TOP 1 t2.[TEST_CASE_WORKFLOW_ID]
		FROM [TST_TEST_CASE_WORKFLOW] t2
		WHERE t2.[PROJECT_TEMPLATE_ID] = t1.[PROJECT_TEMPLATE_ID]
		ORDER BY t2.[TEST_CASE_WORKFLOW_ID])
	FROM [TST_TEST_CASE_TYPE] t1
	WHERE t1.[TEST_CASE_WORKFLOW_ID] NOT IN (SELECT [TEST_CASE_WORKFLOW_ID] FROM [TST_TEST_CASE_WORKFLOW]) OR
		t1.[TEST_CASE_WORKFLOW_ID]  IS NULL;";
				SQLUtilities.ExecuteCommand(conninfo, updSql);

				//Fix the column.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpFixField, sqlTable, sqlField));
				//Re-add the index.
				SQLUtilities.ExecuteCommand(conninfo, string.Format(sqltmpCreateIndex, sqlTable, sqlField, "IDX_TST_TEST_CASE_TYPE_2_FK"));
			}
			#endregion Table TST_TEST_CASE_TYPE
		}

		/// <summary>Creates the missing default vaue. </summary>
		public void FixMissingDefault(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			string sqlTable = "TST_PROJECT_ATTACHMENT", sqlField = "IS_KEY_DOCUMENT";
			string sqltmpQuery = "SELECT object_definition(default_object_id) AS [def] FROM sys.columns WHERE name = '{1}' AND object_id = object_id('{0}')";
			string sqltmpCreate = "ALTER TABLE [{0}] ADD DEFAULT (0) FOR [{1}]";

			DataSet check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpQuery,
					sqlTable,
					sqlField)
			);

			//If there is no default, add it.
			if (check.Tables[0].Rows.Count == 0 ||                         // There are no rows, or 
				check.Tables[0].Rows[0]["def"] == DBNull.Value)            // We get a null value back.
			{
				SQLUtilities.ExecuteCommand(conninfo, string.Format(
					sqltmpCreate,
					sqlTable,
					sqlField)
				);
			}
		}

		/// <summary>Fixes the wrong column size in TST_TASK_PRIORITY, column NAME</summary>
		public void FixColumnSize(DBConnection conninfo)
		{
			//Update progress bar.
			UpdateProgress();

			string sqlTable = "TST_TASK_PRIORITY", sqlField = "NAME";
			string sqltmpSelectField = "SELECT COLUMNPROPERTY(OBJECT_ID('{0}', 'U'), '{1}', 'Precision') AS [len]";
			string sqltmpCreate = "ALTER TABLE [{0}] ALTER COLUMN [{1}] NVARCHAR({2}) NOT NULL;";

			DataSet check = SQLUtilities.GetDataQuery(
				conninfo,
				string.Format(sqltmpSelectField,
					sqlTable,
					sqlField)
			);

			//If there is no default, add it.
			if (check.Tables[0].Rows.Count != 0 &&                         // There are rows, and
				check.Tables[0].Rows[0]["len"] != DBNull.Value &&          // We get a non-null value back.
				check.Tables[0].Rows[0]["len"].GetType() == typeof(int))   // And it's an INT.
			{
				//Get the legnth.
				int len = (int)check.Tables[0].Rows[0]["len"];

				if (len != 50)
				{
					SQLUtilities.ExecuteCommand(
						conninfo,
						string.Format(sqltmpCreate,
							sqlTable,
							sqlField,
							50.ToString()
						)
					);
				}
			}
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
			//  {0} - The table removing from.
			//  {1} - The table we're checking against.
			//  {2} - The deleting table's key field.
			//  {3} - The check table's key field. (Usually is the same as {2}.
			//  {4} - The second table we're checking against.
			//  {5} - The second FK on the deleting table.
			//  {6} - The second table's FK field.
			//string SQLdel2 = "DELETE FROM [{0}] WHERE " +
			//	"([{2}] NOT IN (SELECT [{3}] FROM [{1}])) " +
			//	"OR " +
			//	"([{5}] NOT IN (SELECT [{6}] FROM [{4}]));";

			//  {0} - Table to Update
			//  {1} - Table to reference from
			//  {2} - Field to Update
			//  {3} - Field to reference from (usually same as {1})
			string SQLnull = "UPDATE [{0}] SET [{2}] = null WHERE [{2}] NOT IN (SELECT [{3}] FROM [{1}]);";

			#region Delete Calls
			//Now run it for everything needing deletion.
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_PRIORITY",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
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
				"TST_RELEASE_USER",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_COLLECTION_ENTRY",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_USER_ARTIFACT_FIELD",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_TRANSITION_ROLE",
				"TST_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_USER",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_TRANSITION_ROLE",
				"TST_WORKFLOW_TRANSITION_ROLE_TYPE",
				"WORKFLOW_TRANSITION_ROLE_TYPE_ID",
				"WORKFLOW_TRANSITION_ROLE_TYPE_ID")
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
				"TST_WORKFLOW_FIELD",
				"TST_INCIDENT_STATUS",
				"INCIDENT_STATUS_ID",
				"INCIDENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_WORKFLOW_FIELD",
				"TST_WORKFLOW",
				"WORKFLOW_ID",
				"WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_TYPE",
				"TST_WORKFLOW",
				"WORKFLOW_ID",
				"WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_TYPE",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_HISTORY_DETAIL",
				"TST_HISTORY_CHANGESET",
				"CHANGESET_ID",
				"CHANGESET_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_CUSTOM_PROPERTY_VALUE",
				"TST_CUSTOM_PROPERTY_LIST",
				"CUSTOM_PROPERTY_LIST_ID",
				"CUSTOM_PROPERTY_LIST_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_USER",
				"TST_PROJECT_ROLE",
				"PROJECT_ROLE_ID",
				"PROJECT_ROLE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_USER",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_USER",
				"TST_REQUIREMENT",
				"REQUIREMENT_ID",
				"REQUIREMENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_STATUS",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_STEP_PARAMETER",
				"TST_TEST_CASE_PARAMETER",
				"TEST_CASE_PARAMETER_ID",
				"TEST_CASE_PARAMETER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_STEP_PARAMETER",
				"TST_TEST_STEP",
				"TEST_STEP_ID",
				"TEST_STEP_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUN_STEP",
				"TST_TEST_RUN",
				"TEST_RUN_ID",
				"TEST_RUN_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUN_STEP",
				"TST_TEST_CASE",
				"TEST_CASE_ID",
				"TEST_CASE_ID")
			);

			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_TEST_CASE",
				"TST_TEST_CASE",
				"TEST_CASE_ID",
				"TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_TEST_CASE",
				"TST_REQUIREMENT",
				"REQUIREMENT_ID",
				"REQUIREMENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_CASE",
				"TST_TEST_CASE",
				"TEST_CASE_ID",
				"TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_CASE",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_IMPORTANCE",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_SEVERITY",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_PRIORITY",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_PRIORITY",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_ARTIFACT_USER",
				"TST_NOTIFICATION_EVENT",
				"NOTIFICATION_EVENT_ID",
				"NOTIFICATION_EVENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_PROJECT_ROLE",
				"TST_PROJECT_ROLE",
				"PROJECT_ROLE_ID",
				"PROJECT_ROLE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_PROJECT_ROLE",
				"TST_NOTIFICATION_EVENT",
				"NOTIFICATION_EVENT_ID",
				"NOTIFICATION_EVENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_INCIDENT_RESOLUTION",
				"TST_INCIDENT",
				"INCIDENT_ID",
				"INCIDENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_SET",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);

			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_TEST_CASE",
				"TST_TEST_CASE",
				"TEST_CASE_ID",
				"TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_SAVED_FILTER",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_SAVED_FILTER_ENTRY",
				"TST_SAVED_FILTER",
				"SAVED_FILTER_ID",
				"SAVED_FILTER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUNS_PENDING",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_CUSTOM_PROPERTY_LIST",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_PROJECT",
				"TST_DATA_SYNC_SYSTEM",
				"DATA_SYNC_SYSTEM_ID",
				"DATA_SYNC_SYSTEM_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_PROJECT",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING",
				"TST_DATA_SYNC_PROJECT",
				"DATA_SYNC_SYSTEM_ID",
				"DATA_SYNC_SYSTEM_ID")
			); SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_CUSTOM_PROPERTY_MAPPING",
				"TST_CUSTOM_PROPERTY",
				"CUSTOM_PROPERTY_ID",
				"CUSTOM_PROPERTY_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DATA_SYNC_CUSTOM_PROPERTY_VALUE_MAPPING",
				"TST_CUSTOM_PROPERTY_VALUE",
				"CUSTOM_PROPERTY_VALUE_ID",
				"CUSTOM_PROPERTY_VALUE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REPORT_AVAILABLE_FORMAT",
				"TST_REPORT",
				"REPORT_ID",
				"REPORT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REPORT_AVAILABLE_SECTION",
				"TST_REPORT",
				"REPORT_ID",
				"REPORT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_VERSION_CONTROL_PROJECT",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_VERSION_CONTROL_PROJECT",
				"TST_VERSION_CONTROL_SYSTEM",
				"VERSION_CONTROL_SYSTEM_ID",
				"VERSION_CONTROL_SYSTEM_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_TEST_CASE_PARAMETER",
				"TST_TEST_SET_TEST_CASE",
				"TEST_SET_TEST_CASE_ID",
				"TEST_SET_TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_TEST_CASE_PARAMETER",
				"TST_TEST_CASE_PARAMETER",
				"TEST_CASE_PARAMETER_ID",
				"TEST_CASE_PARAMETER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_NOTIFICATION_EVENT_FIELD",
				"TST_NOTIFICATION_EVENT",
				"NOTIFICATION_EVENT_ID",
				"NOTIFICATION_EVENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_DISCUSSION",
				"TST_REQUIREMENT",
				"ARTIFACT_ID",
				"REQUIREMENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_DISCUSSION",
				"TST_RELEASE",
				"ARTIFACT_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_DISCUSSION",
				"TST_TEST_CASE",
				"ARTIFACT_ID",
				"TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_DISCUSSION",
				"TST_TEST_SET",
				"ARTIFACT_ID",
				"TEST_SET_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_DISCUSSION",
				"TST_TASK",
				"ARTIFACT_ID",
				"TASK_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_HISTORY_CHANGESET",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_TAG_FREQUENCY",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_BUILD",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_BUILD_SOURCE_CODE",
				"TST_BUILD",
				"BUILD_ID",
				"BUILD_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_CUSTOM_PROPERTY_OPTION_VALUE",
				"TST_CUSTOM_PROPERTY",
				"CUSTOM_PROPERTY_ID",
				"CUSTOM_PROPERTY_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REPORT_CUSTOM_SECTION",
				"TST_REPORT",
				"REPORT_ID",
				"REPORT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_TYPE",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_STEP",
				"TST_REQUIREMENT",
				"REQUIREMENT_ID",
				"REQUIREMENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_COMPONENT",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_WORKFLOW",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_WORKFLOW_FIELD",
				"TST_REQUIREMENT_WORKFLOW",
				"REQUIREMENT_WORKFLOW_ID",
				"REQUIREMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_WORKFLOW_CUSTOM_PROPERTY",
				"TST_REQUIREMENT_WORKFLOW",
				"REQUIREMENT_WORKFLOW_ID",
				"REQUIREMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_WORKFLOW_TRANSITION",
				"TST_REQUIREMENT_WORKFLOW",
				"REQUIREMENT_WORKFLOW_ID",
				"REQUIREMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_REQUIREMENT_WORKFLOW_TRANSITION_ROLE",
				"TST_REQUIREMENT_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_TYPE",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_WORKFLOW_TRANSITION",
				"TST_TASK_WORKFLOW",
				"TASK_WORKFLOW_ID",
				"TASK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_WORKFLOW_TRANSITION_ROLE",
				"TST_TASK_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_WORKFLOW_FIELD",
				"TST_TASK_WORKFLOW",
				"TASK_WORKFLOW_ID",
				"TASK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TASK_WORKFLOW_CUSTOM_PROPERTY",
				"TST_TASK_WORKFLOW",
				"TASK_WORKFLOW_ID",
				"TASK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TVAULT_PROJECT_USER",
				"TST_TVAULT_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_WORKFLOW_TRANSITION",
				"TST_RELEASE_WORKFLOW",
				"RELEASE_WORKFLOW_ID",
				"RELEASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_WORKFLOW_CUSTOM_PROPERTY",
				"TST_RELEASE_WORKFLOW",
				"RELEASE_WORKFLOW_ID",
				"RELEASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_WORKFLOW_FIELD",
				"TST_RELEASE_WORKFLOW",
				"RELEASE_WORKFLOW_ID",
				"RELEASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_WORKFLOW_TRANSITION_ROLE",
				"TST_RELEASE_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_WORKFLOW_TRANSITION",
				"TST_TEST_CASE_WORKFLOW",
				"TEST_CASE_WORKFLOW_ID",
				"TEST_CASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_WORKFLOW_FIELD",
				"TST_TEST_CASE_WORKFLOW",
				"TEST_CASE_WORKFLOW_ID",
				"TEST_CASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_WORKFLOW_CUSTOM_PROPERTY",
				"TST_TEST_CASE_WORKFLOW",
				"TEST_CASE_WORKFLOW_ID",
				"TEST_CASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_WORKFLOW_TRANSITION_ROLE",
				"TST_TEST_CASE_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_CASE_TYPE",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_CASE_FOLDER",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_CASE_FOLDER",
				"TST_TEST_CASE_FOLDER",
				"TEST_CASE_FOLDER_ID",
				"TEST_CASE_FOLDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUN_STEP_INCIDENT",
				"TST_INCIDENT",
				"INCIDENT_ID",
				"INCIDENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_RUN_STEP_INCIDENT",
				"TST_TEST_RUN_STEP",
				"TEST_RUN_STEP_ID",
				"TEST_RUN_STEP_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_TEST_SET_PARAMETER",
				"TST_TEST_SET",
				"TEST_SET_ID",
				"TEST_SET_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_SET",
				"TST_TEST_SET",
				"TEST_SET_ID",
				"TEST_SET_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TEST_SET_FOLDER",
				"TST_TEST_SET_FOLDER",
				"TEST_SET_FOLDER_ID",
				"TEST_SET_FOLDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RELEASE_TYPE_WORKFLOW",
				"TST_RELEASE_WORKFLOW",
				"RELEASE_WORKFLOW_ID",
				"RELEASE_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_TRANSITION",
				"TST_DOCUMENT_WORKFLOW",
				"DOCUMENT_WORKFLOW_ID",
				"DOCUMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_TRANSITION",
				"TST_DOCUMENT_STATUS",
				"INPUT_DOCUMENT_STATUS_ID",
				"DOCUMENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE",
				"TST_DOCUMENT_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE",
				"TST_PROJECT_ROLE",
				"PROJECT_ROLE_ID",
				"PROJECT_ROLE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_FIELD",
				"TST_DOCUMENT_WORKFLOW",
				"DOCUMENT_WORKFLOW_ID",
				"DOCUMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_FIELD",
				"TST_DOCUMENT_STATUS",
				"DOCUMENT_STATUS_ID",
				"DOCUMENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD",
				"TST_DOCUMENT_WORKFLOW",
				"DOCUMENT_WORKFLOW_ID",
				"DOCUMENT_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_DOCUMENT_WORKFLOW_CUSTOM_FIELD",
				"TST_DOCUMENT_STATUS",
				"DOCUMENT_STATUS_ID",
				"DOCUMENT_STATUS_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_PROBABILITY",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_IMPACT",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK",
				"TST_PROJECT",
				"PROJECT_ID",
				"PROJECT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_STATUS",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_DISCUSSION",
				"TST_RISK",
				"ARTIFACT_ID",
				"RISK_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_MITIGATION",
				"TST_RISK",
				"RISK_ID",
				"RISK_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_WORKFLOW_TRANSITION",
				"TST_RISK_WORKFLOW",
				"RISK_WORKFLOW_ID",
				"RISK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_WORKFLOW_TRANSITION_ROLE",
				"TST_RISK_WORKFLOW_TRANSITION",
				"WORKFLOW_TRANSITION_ID",
				"WORKFLOW_TRANSITION_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_WORKFLOW_CUSTOM_PROPERTY",
				"TST_RISK_WORKFLOW",
				"RISK_WORKFLOW_ID",
				"RISK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_RISK_WORKFLOW_FIELD",
				"TST_RISK_WORKFLOW",
				"RISK_WORKFLOW_ID",
				"RISK_WORKFLOW_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLdel1,
				"TST_PROJECT_TEMPLATE_USER",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			#endregion

			#region Set Null calls
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TASK",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TASK",
				"TST_REQUIREMENT",
				"REQUIREMENT_ID",
				"REQUIREMENT_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TASK",
				"TST_TASK_FOLDER",
				"TASK_FOLDER_ID",
				"TASK_FOLDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_HISTORY_DETAIL",
				"TST_CUSTOM_PROPERTY",
				"CUSTOM_PROPERTY_ID",
				"CUSTOM_PROPERTY_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN_STEP",
				"TST_TEST_STEP",
				"TEST_STEP_ID",
				"TEST_STEP_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_REQUIREMENT",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN",
				"TST_TEST_SET ",
				"TEST_SET_ID ",
				"TEST_SET_ID ")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN",
				"TST_TEST_RUNS_PENDING",
				"TEST_RUNS_PENDING_ID",
				"TEST_RUNS_PENDING_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN",
				"TST_TEST_SET_TEST_CASE",
				"TEST_SET_TEST_CASE_ID",
				"TEST_SET_TEST_CASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUN",
				"TST_AUTOMATION_HOST",
				"AUTOMATION_HOST_ID",
				"AUTOMATION_HOST_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_CASE",
				"TST_TEST_CASE_FOLDER",
				"TEST_CASE_FOLDER_ID",
				"TEST_CASE_FOLDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_USER",
				"TST_GLOBAL_OAUTH_PROVIDERS",
				"OAUTH_PROVIDER_ID",
				"OAUTH_PROVIDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_SET",
				"TST_RELEASE",
				"RELEASE_ID",
				"RELEASE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_SET",
				"TST_AUTOMATION_HOST",
				"AUTOMATION_HOST_ID",
				"AUTOMATION_HOST_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_SET",
				"TST_TEST_SET_FOLDER",
				"TEST_SET_FOLDER_ID",
				"TEST_SET_FOLDER_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_TEST_RUNS_PENDING",
				"TST_TEST_SET",
				"TEST_SET_ID",
				"TEST_SET_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_PROJECT_GROUP",
				"TST_PROJECT_TEMPLATE",
				"PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_USER_PROFILE",
				"TST_PROJECT_GROUP",
				"LAST_OPENED_PROJECT_GROUP_ID",
				"PROJECT_GROUP_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_USER_PROFILE",
				"TST_PROJECT_TEMPLATE",
				"LAST_OPENED_PROJECT_TEMPLATE_ID",
				"PROJECT_TEMPLATE_ID")
			);
			SQLUtilities.ExecuteCommand(conninfo, string.Format(SQLnull,
				"TST_RISK",
				"TST_PROJECT_GROUP",
				"PROJECT_GROUP_ID",
				"PROJECT_GROUP_ID")
			);
			#endregion Set Null calls
		}

		/// <summary>Creates the new foreign keys</summary>
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

		/// <summary>Updates the '-1' entry in the database for TST_REQUIREMENT_TYPE from 'package' to 'Epic'.</summary>
		private void UpdateStaticData(DBConnection connectionInfo)
		{
			//Update progress bar.
			UpdateProgress();

			string updSql = "UPDATE [TST_REQUIREMENT_TYPE] SET [NAME] = 'Epic' WHERE [REQUIREMENT_TYPE_ID] = -1;";
			SQLUtilities.ExecuteCommand(connectionInfo, updSql);
		}
		#endregion
	}
}
