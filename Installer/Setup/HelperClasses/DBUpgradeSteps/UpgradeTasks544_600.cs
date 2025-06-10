using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade v540 to v600</summary>
	public class UpgradeTasks600 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private List<int> projectIds = new List<int>();    //List of projects (also templates in v5.x)
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		public const int DB_REV = 600;
		public const int DB_UPG_MIN = 540;
		public const int DB_UPG_MAX = 540;

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks600(StreamWriter LogWriter, string RootDataFolder) : base(LogWriter, RootDataFolder)
		{
			//We don't do anything in the constructor.
		}

		/// <summary>The revision this upgrade leaves the Database in.</summary>
		public int DBRevision { get { return DB_REV; } }

		/// <summary>The number of steps this process has. (For progress bar calculation.)</summary>
		public int NumberSteps { get { return 12; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMin { get { return DB_UPG_MIN; } }

		/// <summary>The allowable lower-range for the DB to be for this upgrader to work. Must be inclusive.</summary>
		public int DBRevisionUpgradeMax { get { return DB_UPG_MAX; } }

		/// <summary>The location for this upgrade class's database files.</summary>
		public string DatabaseFilePath { get { return null; } }

		/// <summary>Do the database upgrade!</summary>
		/// <param name="ConnectionInfo"></param>
		/// <returns>tatus of update.</returns>
		public bool UpgradeDB(DBConnection ConnectionInfo, Action<object> ProgressOverride, int curNum, float totNum)
		{
			//Save the event.
			ProgressHandler = ProgressOverride;
			_connection = ConnectionInfo;
			_totJob = totNum;

			//Now run the steps!
			// 1 - Delete all foreign key restraints.
			_logger.WriteLine(loggerStr + "DeleteExistingForeignKeyConstraints - Starting");
			DeleteExistingForeignKeyConstraints();
			_logger.WriteLine(loggerStr + "DeleteExistingForeignKeyConstraints - Finished");

			// 2 - Delete Full Text Indexes
			_logger.WriteLine(loggerStr + "DeleteFullTextIndexes - Starting");
			DeleteFullTextIndexes();
			_logger.WriteLine(loggerStr + "DeleteFullTextIndexes - Finished");

			// 3 - Add Empty Tables
			_logger.WriteLine(loggerStr + "AddEmptyTables - Starting");
			AddEmptyTables();
			_logger.WriteLine(loggerStr + "AddEmptyTables - Finished");

			// 4 - Add Dynaic Tables
			_logger.WriteLine(loggerStr + "AddDynamicTables - Starting");
			AddDynamicTables();
			_logger.WriteLine(loggerStr + "AddDynamicTables - Finished");

			// 5 - Update Static Tables
			_logger.WriteLine(loggerStr + "UpdateStaticTables - Starting");
			UpdateStaticTables();
			_logger.WriteLine(loggerStr + "UpdateStaticTables - Finished");

			// 6 - Update Dynamic Tables
			_logger.WriteLine(loggerStr + "UpdateDynamicTables - Starting");
			UpdateDynamicTables();
			_logger.WriteLine(loggerStr + "UpdateDynamicTables - Finished");

			// 7 - Migrate Types & Priorities to Templates
			_logger.WriteLine(loggerStr + "MigrateTypesPrioritiesToTemplateBased - Starting");
			MigrateTypesPrioritiesToTemplateBased();
			_logger.WriteLine(loggerStr + "MigrateTypesPrioritiesToTemplateBased - Finshed");

			// 8 - Migrate Notification Templates
			_logger.WriteLine(loggerStr + "MigrateNotificationTemplates - Starting");
			MigrateNotificationTemplates();
			_logger.WriteLine(loggerStr + "MigrateNotificationTemplates - Finished");

			// 9 - Drop Old Tables
			_logger.WriteLine(loggerStr + "DeleteOldTables - Starting");
			DeleteOldTables();
			_logger.WriteLine(loggerStr + "DeleteOldTables - Finished");

			// 10 - Create Foreign Keys
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Starting");
			CreateForeignKeys();
			_logger.WriteLine(loggerStr + "CreateForeignKeys - Finished");

			// 11 - Create Full Text Indexes
			_logger.WriteLine(loggerStr + "CreateFullTextIndexes - Starting");
			CreateFullTextIndexes();
			_logger.WriteLine(loggerStr + "CreateFullTextIndexes - Finished");

			// 12 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			//If we made it this far, return.
			return true;
		}
		#endregion Interface Functions

		/// <summary>Checks that our v5.x database is truly v5.4 before we attempt to upgrade it/// </summary>
		/// <returns>True if the database is upgradeable</returns>
		public bool VerifyDatabaseIsCorrectVersionToUpgrade(DBConnection conninfo, StreamWriter streamWriter)
		{
			int dbVer = 0;

			try
			{
				//Get the reported DB version, first.
				dbVer = SQLUtilities.GetExistingDBRevision(conninfo);
			}
			catch (Exception exception)
			{
				streamWriter.WriteLine(
					"Unable to determine if database can be upgraded:" +
					Environment.NewLine +
					Logger.DecodeException(exception));
			}

			//Return our value.
			return (dbVer >= DB_UPG_MIN && dbVer <= DB_UPG_MAX);
		}

		#region Private SQL Functions
		/// <summary>Deletes any obsolete tables (and primary keys/indexes)</summary>
		private void DeleteOldTables()
		{
			//Update progress bar.
			UpdateProgress();

			//Get the SQL File. 
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "DeleteOldTables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);
		}

		/// <summary>Adds the new database tables that are initially empty (no migration needs)</summary>
		private void AddEmptyTables()
		{
			//Update status bar.
			UpdateProgress();

			//Get the SQL File. 
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "AddEmptyTables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);
		}

		/// <summary>Adds the tables that contain dynamic data</summary>
		private void AddDynamicTables()
		{
			//Update the progress bar..
			UpdateProgress();

			//The big script, first.
			string dynamicTableChanges = ZipReader.GetContents("DB_v600.zip", "AddDynamicTables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//Get the list of projects
			try
			{
				using (SqlConnection conn = new SqlConnection(SQLUtilities.GenerateConnectionString(_connection)))
				{
					using (SqlCommand cmd = new SqlCommand())
					{
						conn.Open();
						cmd.Connection = conn;
						cmd.CommandTimeout = 1200;
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = @"SELECT PROJECT_ID FROM TST_PROJECT";
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							// Call Read before accessing data.
							while (reader.Read())
							{
								int projectId = (int)reader[0];
								projectIds.Add(projectId);
							}

							reader.Close();
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.WriteLine("Trying to get ProjectIDs:" + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}

			//Populate the initial workflows for Risks and Documents (previously did not have workflow)
			foreach (int projectId in projectIds)
				PopulateDocumentAndRiskInitialWorkflows(projectId);

			//TST_ATTACHMENT
			//This needs to be done after the document workflows are published
			//Also need to handle the 'edge' case where you have a row in TST_ATTACHMENT but not in TST_PROJECT_ATTACHMENT
			//should not happen in theory, but did with our live database!
			SQLUtilities.AddInt32Column(_connection, "TST_ATTACHMENT", "DOCUMENT_STATUS_ID");
			dynamicTableChanges = @"
UPDATE TST_ATTACHMENT
SET DOCUMENT_STATUS_ID = (SELECT TOP 1 DOCUMENT_STATUS_ID FROM TST_DOCUMENT_STATUS)
WHERE ATTACHMENT_ID NOT IN (SELECT ATTACHMENT_ID FROM TST_PROJECT_ATTACHMENT)
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			dynamicTableChanges = @"
UPDATE TST_ATTACHMENT
    SET DOCUMENT_STATUS_ID = DOC.DOCUMENT_STATUS_ID
FROM TST_ATTACHMENT ATT
    INNER JOIN TST_PROJECT_ATTACHMENT PAT ON ATT.ATTACHMENT_ID = PAT.ATTACHMENT_ID
    INNER JOIN TST_DOCUMENT_STATUS DOC ON PAT.PROJECT_ID = DOC.PROJECT_TEMPLATE_ID
WHERE DOC.IS_DEFAULT = 1
GO
ALTER TABLE [TST_ATTACHMENT]
    ALTER COLUMN [DOCUMENT_STATUS_ID] INTEGER NOT NULL
GO
CREATE  INDEX [IDX_TST_ATTACHMENT_4_FK] ON [TST_ATTACHMENT] ([DOCUMENT_STATUS_ID])
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);
		}

		/// <summary>Updates any static tables</summary>
		private void UpdateStaticTables()
		{
			//Update the UI.
			UpdateProgress();

			//First the simple static tables. Delte the needed entries..
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_ARTIFACT_FIELD]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_ATTACHMENT_TYPE]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_GLOBAL_FILETYPES]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_HISTORY_CHANGESET_TYPE]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_PROJECT_COLLECTION]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_PROJECT_GROUP_ROLE]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_RELEASE_TYPE]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_REQUIREMENT_STATUS]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_TASK_STATUS]");
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_USER_COLLECTION]");
			SQLUtilities.AddBitColumn(_connection, "TST_ARTIFACT_TYPE", "IS_GLOBAL_ITEM", false);
			SQLUtilities.ExecuteCommand(_connection, "DELETE FROM [TST_ARTIFACT_TYPE]");
			// Now run the file to repopulate..
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "UpdateStaticTables.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);


			/* Report Static Data Changes */

			//TST_REPORT_CATEGORY
			string reportQuery = @"
INSERT INTO TST_REPORT_CATEGORY
(
REPORT_CATEGORY_ID, NAME, POSITION, ARTIFACT_TYPE_ID, IS_ACTIVE
)
VALUES
(
6, 'Risk Reports', 600, 14, 1
)
";
			SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT_CATEGORY", reportQuery);

			//TST_REPORT_ELEMENT
			reportQuery = @"
INSERT INTO TST_REPORT_ELEMENT
(
REPORT_ELEMENT_ID, TOKEN, NAME, DESCRIPTION, IS_ACTIVE, ARTIFACT_TYPE_ID
)
VALUES
(
13, 'Mitigations', 'Mitigations', NULL, 1, NULL
)
";
			SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT_ELEMENT", reportQuery);
		}

		/// <summary>Updates any dynamic tables</summary>
		private void UpdateDynamicTables()
		{
			//Update the UI progress bar.
			UpdateProgress();

			//TST_ATTACHMENT_VERSION
			SQLUtilities.AddInt64Column(_connection, "TST_ATTACHMENT_VERSION", "CHANGESET_ID", null, "IDX_TST_ATTACHMENT_VERSION_3_FK");

			//TST_CUSTOM_PROPERTY
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_CUSTOM_PROPERTY");

			//TST_CUSTOM_PROPERTY_LIST
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_CUSTOM_PROPERTY_LIST");

			//TST_CUSTOM_PROPERTY_VALUE
			SQLUtilities.AddInt32Column(_connection, "TST_CUSTOM_PROPERTY_VALUE", "PARENT_CUSTOM_PROPERTY_VALUE_ID", null, "IDX_TST_CUSTOM_PROPERTY_VALUE_3_FK");

			//TST_DATA_SYNC_SYSTEM
			SQLUtilities.AddBitColumn(_connection, "TST_DATA_SYNC_SYSTEM", "IS_ACTIVE", true);

			// Static SQLs, TST_HISTORY_CHANGESET & TST_HISTORY_DETAIL
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_1.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);

			//TST_INCIDENT
			SQLUtilities.AddDateColumn(_connection, "TST_INCIDENT", "END_DATE", false);
			SQLUtilities.AddInt32Column(_connection, "TST_INCIDENT", "RESOLVED_BUILD_ID", null, "IDX_TST_INCIDENT_11_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_INCIDENT", "DETECTED_BUILD_ID", null, "IDX_TST_INCIDENT_12_FK");
			// Static SQLs, TST_HISTORY_CHANGESET & TST_HISTORY_DETAIL
			string sqlQuery2 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_2.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery2);

			//TST_INCIDENT_PRIORITY
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_INCIDENT_PRIORITY");

			//TST_INCIDENT_SEVERITY
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_INCIDENT_SEVERITY");
			string dynamicTableChanges = @"
ALTER TABLE [TST_INCIDENT_SEVERITY]
    ALTER COLUMN [NAME] NVARCHAR(50) NOT NULL
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_INCIDENT_STATUS
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_INCIDENT_STATUS");

			//TST_INCIDENT_TYPE
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_INCIDENT_TYPE");

			//TST_NOTIFICATION_EVENT
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_NOTIFICATION_EVENT");
			SQLUtilities.ConvertFlagToBitField(_connection, "TST_NOTIFICATION_EVENT", "ACTIVE_YN", "IS_ACTIVE", true);
			SQLUtilities.ConvertFlagToBitField(_connection, "TST_NOTIFICATION_EVENT", "ARTIFACT_CREATION_YN", "IS_ARTIFACT_CREATION", false);

			//TST_PROJECT
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT", "PROJECT_TEMPLATE_ID");
			SQLUtilities.AddDateColumn(_connection, "TST_PROJECT", "START_DATE", false);
			SQLUtilities.AddDateColumn(_connection, "TST_PROJECT", "END_DATE", false);
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT", "PERCENT_COMPLETE", 0);
			dynamicTableChanges = @"
UPDATE TST_PROJECT
    SET PROJECT_TEMPLATE_ID = PROJECT_ID
GO
ALTER TABLE [TST_PROJECT]
    ALTER COLUMN [PROJECT_TEMPLATE_ID] INTEGER NOT NULL
GO
CREATE  INDEX [IDX_TST_PROJECT_2_FK] ON [TST_PROJECT] ([PROJECT_TEMPLATE_ID])
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_PROJECT_ATTACHMENT
			dynamicTableChanges = @"
DROP INDEX [AK_TST_PROJECT_ATTACHMENT_3] ON [TST_PROJECT_ATTACHMENT]
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);
			SQLUtilities.AddBitColumn(_connection, "TST_PROJECT_ATTACHMENT", "IS_KEY_DOCUMENT", false);
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT_ATTACHMENT", "DOCUMENT_TYPE_ID", 0, "AK_TST_PROJECT_ATTACHMENT_3");
			dynamicTableChanges = @"
UPDATE [TST_PROJECT_ATTACHMENT]
    SET DOCUMENT_TYPE_ID = PROJECT_ATTACHMENT_TYPE_ID
	WHERE PROJECT_ATTACHMENT_TYPE_ID IS NOT NULL
GO
ALTER TABLE [TST_PROJECT_ATTACHMENT]
    DROP COLUMN [PROJECT_ATTACHMENT_TYPE_ID]
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_PROJECT_ATTACHMENT_TYPE
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_PROJECT_ATTACHMENT_TYPE");

			//TST_PROJECT_GROUP
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT_GROUP", "PROJECT_TEMPLATE_ID", null, "IDX_TST_PROJECT_GROUP_1_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT_GROUP", "PORTFOLIO_ID", null, "IDX_TST_PROJECT_GROUP_2_FK");
			SQLUtilities.ConvertFlagToBitField(_connection, "TST_PROJECT_GROUP", "ACTIVE_YN", "IS_ACTIVE");
			SQLUtilities.ConvertFlagToBitField(_connection, "TST_PROJECT_GROUP", "DEFAULT_YN", "IS_DEFAULT");
			SQLUtilities.AddDateColumn(_connection, "TST_PROJECT_GROUP", "START_DATE", false);
			SQLUtilities.AddDateColumn(_connection, "TST_PROJECT_GROUP", "END_DATE", false);
			SQLUtilities.AddInt32Column(_connection, "TST_PROJECT_GROUP", "PERCENT_COMPLETE", 0);

			//TST_PROJECT_ROLE
			SQLUtilities.AddBitColumn(_connection, "TST_PROJECT_ROLE", "IS_TEMPLATE_ADMIN");
			dynamicTableChanges = @"
UPDATE TST_PROJECT_ROLE
    SET IS_TEMPLATE_ADMIN = IS_ADMIN
GO
ALTER TABLE [TST_PROJECT_ROLE]
    ALTER COLUMN [IS_TEMPLATE_ADMIN] BIT NOT NULL
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_RELEASE
			SQLUtilities.AddInt32Column(_connection, "TST_RELEASE", "PERCENT_COMPLETE", 0);
			SQLUtilities.AddInt32Column(_connection, "TST_RELEASE", "MILESTONE_ID", null, "IDX_TST_RELEASE_8_FK");
			dynamicTableChanges = @"
ALTER TABLE [TST_RELEASE]
    ALTER COLUMN [RESOURCE_COUNT] DECIMAL NOT NULL
GO
ALTER TABLE [TST_RELEASE]
    ALTER COLUMN [DAYS_NON_WORKING] DECIMAL NOT NULL
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_RELEASE_WORKFLOW
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_RELEASE_WORKFLOW");

			//TST_REQUIREMENT_WORKFLOW
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_REQUIREMENT_WORKFLOW");

			//TST_TASK_WORKFLOW
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_TASK_WORKFLOW");

			//TST_TEST_CASE_WORKFLOW
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_TEST_CASE_WORKFLOW");

			//TST_TEST_RUN
			SQLUtilities.AddInt64Column(_connection, "TST_TEST_RUN", "CHANGESET_ID", null, "IDX_TST_TEST_RUN_18_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_TEST_RUN", "TEST_CONFIGURATION_ID", null, "IDX_TST_TEST_RUN_19_FK");

			//TST_TEST_SET_TEST_CASE
			SQLUtilities.AddDateColumn(_connection, "TST_TEST_SET_TEST_CASE", "PLANNED_DATE", false);
			SQLUtilities.AddBitColumn(_connection, "TST_TEST_SET_TEST_CASE", "IS_SETUP_TEARDOWN", false);

			//TST_TEST_STEP
			SQLUtilities.AddStringColumn(_connection, "TST_TEST_STEP", "PRECONDITION", null);

			//TST_USER_PROFILE
			SQLUtilities.AddBitColumn(_connection, "TST_USER_PROFILE", "IS_RESTRICTED", false);
			SQLUtilities.AddBitColumn(_connection, "TST_USER_PROFILE", "IS_RESOURCE_ADMIN", false);
			SQLUtilities.AddBitColumn(_connection, "TST_USER_PROFILE", "IS_PORTFOLIO_ADMIN", false);
			SQLUtilities.AddInt32Column(_connection, "TST_USER_PROFILE", "LAST_OPENED_PROJECT_GROUP_ID", null, "IDX_TST_USER_PROFILE_2_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_USER_PROFILE", "LAST_OPENED_PROJECT_TEMPLATE_ID", null, "IDX_TST_USER_PROFILE_3_FK");

			//TST_WORKFLOW
			SQLUtilities.ConvertProjectIdToProjectTemplateId(_connection, "TST_WORKFLOW");

			//TST_REPORT_FORMAT
			string sqlQuery3 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_3.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery3);

			//TST_PROJECT_ROLE
			dynamicTableChanges = @"
UPDATE TST_PROJECT_ROLE
    SET NAME = 'Product Owner'
WHERE PROJECT_ROLE_ID = 1
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			//TST_USER
			SQLUtilities.AddGuidColumn(_connection, "TST_USER", "OAUTH_PROVIDER_ID", null, "IDX_TST_USER_2_FK");
			SQLUtilities.AddStringColumn(_connection, "TST_USER", "OAUTH_ACCESS_TOKEN", 255);
			SQLUtilities.AddStringColumn(_connection, "TST_USER", "MFA_PHONE", 255);
			SQLUtilities.AddStringColumn(_connection, "TST_USER", "MFA_TOKEN", 255);

			//TST_PROJECT_ROLE_PERMISSION & TST_USER_CUSTOM_PROPERTY
			string sqlQuery4 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_4.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery4);

			//TST_REQUIREMENT
			SQLUtilities.AddInt32Column(_connection, "TST_REQUIREMENT", "THEME_ID", null, "IDX_TST_REQUIREMENT_10_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_REQUIREMENT", "GOAL_ID", null, "IDX_TST_REQUIREMENT_11_FK");
			SQLUtilities.AddDateColumn(_connection, "TST_REQUIREMENT", "START_DATE", false);
			SQLUtilities.AddDateColumn(_connection, "TST_REQUIREMENT", "END_DATE", false);
			SQLUtilities.AddInt32Column(_connection, "TST_REQUIREMENT", "PERCENT_COMPLETE");

			//TST_TASK
			SQLUtilities.AddInt32Column(_connection, "TST_TASK", "RISK_ID", null, "IDX_TST_TASK_10_FK");

			#region New Reports

			long reportId1;
			long reportId2;

			long reportSectionId1;
			long reportSectionId2;

			//TST_REPORT
			string report = @"
INSERT INTO TST_REPORT
(
REPORT_CATEGORY_ID, TOKEN, NAME, DESCRIPTION, HEADER, FOOTER, IS_ACTIVE
)
VALUES
(
6, 'RiskSummary', 'Risk Summary', 'This report displays all of the risks tracked for the current project. The risks are displayed in a summary table form.', '<p>This report displays all of the risks tracked for the current project. The risks are displayed in a summary table form.</p>', NULL, 1
)
";
			reportId1 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT", report, "REPORT_ID");

			report = @"
INSERT INTO TST_REPORT
(
REPORT_CATEGORY_ID, TOKEN, NAME, DESCRIPTION, HEADER, FOOTER, IS_ACTIVE
)
VALUES
(
6, 'RiskDetailed', 'Risk Detailed', 'This report displays all of the risks tracked for the current project. The risks are displayed, along with a tabular list of mitigations, tasks, comments, attached documents, and change history', '<p>This report displays all of the risks tracked for the current project. The risks are displayed, along with a tabular list of mitigations, tasks, comments, attached documents, and change history</p>', NULL, 1
)
";
			reportId2 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT", report, "REPORT_ID");

			//TST_REPORT_AVAILABLE_FORMAT
			string reportAvailableFormat = @"
INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 1
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 2
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 3
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 5
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 6
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 7
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId1 + @", 8
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId2 + @", 1
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId2 + @", 2
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId2 + @", 5
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId2 + @", 6
)
GO

INSERT INTO TST_REPORT_AVAILABLE_FORMAT
(
REPORT_ID, REPORT_FORMAT_ID
)
VALUES
(
" + reportId2 + @", 8
)
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, reportAvailableFormat);

			//TST_REPORT_SECTION
			string sqlQuery5 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_5.sql");
			reportSectionId1 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT_SECTION", sqlQuery5, "REPORT_SECTION_ID");
			string sqlQuery6 = ZipReader.GetContents("DB_v600.zip", "UpdateDynamicTables_6.sql");
			reportSectionId2 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REPORT_SECTION", sqlQuery6, "REPORT_SECTION_ID");

			//TST_REPORT_AVAILABLE_SECTION
			string reportAvailableSections = @"
INSERT INTO TST_REPORT_AVAILABLE_SECTION
(
REPORT_ID, REPORT_SECTION_ID, TEMPLATE, HEADER, FOOTER
)
VALUES
(
" + reportId1 + @", 1, NULL, NULL, NULL
)
GO

INSERT INTO TST_REPORT_AVAILABLE_SECTION
(
REPORT_ID, REPORT_SECTION_ID, TEMPLATE, HEADER, FOOTER
)
VALUES
(
" + reportId1 + @", " + reportSectionId1 + @", NULL, NULL, NULL
)
GO

INSERT INTO TST_REPORT_AVAILABLE_SECTION
(
REPORT_ID, REPORT_SECTION_ID, TEMPLATE, HEADER, FOOTER
)
VALUES
(
" + reportId2 + @", 1, NULL, NULL, NULL
)
GO

INSERT INTO TST_REPORT_AVAILABLE_SECTION
(
REPORT_ID, REPORT_SECTION_ID, TEMPLATE, HEADER, FOOTER
)
VALUES
(
" + reportId2 + @", " + reportSectionId2 + @", NULL, NULL, NULL
)
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, reportAvailableSections);

			//TST_REPORT_SECTION_ELEMENT
			dynamicTableChanges = @"
INSERT INTO TST_REPORT_SECTION_ELEMENT
(
REPORT_SECTION_ID, REPORT_ELEMENT_ID
)
VALUES
(
" + reportSectionId2 + @", 1
)
GO

INSERT INTO TST_REPORT_SECTION_ELEMENT
(
REPORT_SECTION_ID, REPORT_ELEMENT_ID
)
VALUES
(
" + reportSectionId2 + @", 2
)
GO

INSERT INTO TST_REPORT_SECTION_ELEMENT
(
REPORT_SECTION_ID, REPORT_ELEMENT_ID
)
VALUES
(
" + reportSectionId2 + @", 8
)
GO

INSERT INTO TST_REPORT_SECTION_ELEMENT
(
REPORT_SECTION_ID, REPORT_ELEMENT_ID
)
VALUES
(
" + reportSectionId2 + @", 13
)
GO
";
			SQLUtilities.ExecuteSqlCommands(_connection, dynamicTableChanges);

			#endregion
		}

		/// <summary>Migrates the requirement, task, test case types/priorities to the new template based system</summary>
		private void MigrateTypesPrioritiesToTemplateBased()
		{
			//Update UI progress bar.
			UpdateProgress();

			//First we need to delete all of the existing data in the type and priority tables
			//Except for the static -1 package requirement type
			//Also delete any saved filters for those artifact types
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "MigrateTypesPrioritiesToTemplateBased_1.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);

			//Next alter the tables themselves

			//TST_TEST_CASE_TYPE
			SQLUtilities.AddInt32Column(_connection, "TST_TEST_CASE_TYPE", "PROJECT_TEMPLATE_ID", 0, "IDX_TST_TEST_CASE_TYPE_1_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_TEST_CASE_TYPE", "TEST_CASE_WORKFLOW_ID", 0, "IDX_TST_TEST_CASE_TYPE_2_FK");
			SQLUtilities.DropColumn(_connection, "TST_TEST_CASE_TYPE", "NAME");
			SQLUtilities.AddStringColumn(_connection, "TST_TEST_CASE_TYPE", "NAME", 50, "xxx"); //Table empty so 'xxx' is not really used
			SQLUtilities.AddBitColumn(_connection, "TST_TEST_CASE_TYPE", "IS_EXPLORATORY", false);
			SQLUtilities.AddBitColumn(_connection, "TST_TEST_CASE_TYPE", "IS_BDD", false);
			SQLUtilities.AddBitColumn(_connection, "TST_TEST_CASE_TYPE", "IS_DEFAULT", false);

			//TST_TASK_TYPE
			SQLUtilities.AddInt32Column(_connection, "TST_TASK_TYPE", "PROJECT_TEMPLATE_ID", 0, "IDX_TST_TASK_TYPE_1_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_TASK_TYPE", "TASK_WORKFLOW_ID", 0, "IDX_TST_TASK_TYPE_2_FK");
			SQLUtilities.AddBitColumn(_connection, "TST_TASK_TYPE", "IS_CODE_REVIEW", false);
			SQLUtilities.AddBitColumn(_connection, "TST_TASK_TYPE", "IS_PULL_REQUEST", false);
			SQLUtilities.AddBitColumn(_connection, "TST_TASK_TYPE", "IS_DEFAULT", false);

			//TST_REQUIREMENT_TYPE
			SQLUtilities.AddInt32Column(_connection, "TST_REQUIREMENT_TYPE", "PROJECT_TEMPLATE_ID", null, "IDX_TST_REQUIREMENT_TYPE_1_FK");
			SQLUtilities.AddInt32Column(_connection, "TST_REQUIREMENT_TYPE", "REQUIREMENT_WORKFLOW_ID", null, "IDX_TST_REQUIREMENT_TYPE_2_FK");
			SQLUtilities.AddBitColumn(_connection, "TST_REQUIREMENT_TYPE", "IS_STEPS", false);
			SQLUtilities.AddBitColumn(_connection, "TST_REQUIREMENT_TYPE", "IS_KEY_TYPE", false);
			SQLUtilities.AddBitColumn(_connection, "TST_REQUIREMENT_TYPE", "IS_DEFAULT", false);

			//TST_TEST_CASE_PRIORITY
			SQLUtilities.AddInt32Column(_connection, "TST_TEST_CASE_PRIORITY", "PROJECT_TEMPLATE_ID", 0, "IDX_TST_TEST_CASE_PRIORITY_1_FK");
			SQLUtilities.AddCharColumn(_connection, "TST_TEST_CASE_PRIORITY", "COLOR", 6, "xxx");   //Table empty so 'xxx' is not really used
			SQLUtilities.AddInt32Column(_connection, "TST_TEST_CASE_PRIORITY", "SCORE", 0);

			//TST_TASK_PRIORITY
			SQLUtilities.AddInt32Column(_connection, "TST_TASK_PRIORITY", "PROJECT_TEMPLATE_ID", 0, "IDX_TST_TASK_PRIORITY_1_FK");
			SQLUtilities.AddCharColumn(_connection, "TST_TASK_PRIORITY", "COLOR", 6, "xxx");   //Table empty so 'xxx' is not really used
			SQLUtilities.AddInt32Column(_connection, "TST_TASK_PRIORITY", "SCORE", 0);

			//TST_IMPORTANCE
			SQLUtilities.AddInt32Column(_connection, "TST_IMPORTANCE", "PROJECT_TEMPLATE_ID", 0, "IDX_TST_IMPORTANCE_1_FK");
			SQLUtilities.DropColumn(_connection, "TST_IMPORTANCE", "NAME");
			SQLUtilities.AddStringColumn(_connection, "TST_IMPORTANCE", "NAME", 50, "xxx"); //Table empty so 'xxx' is not really used
			SQLUtilities.AddCharColumn(_connection, "TST_IMPORTANCE", "COLOR", 6, "xxx");   //Table empty so 'xxx' is not really used
			SQLUtilities.AddInt32Column(_connection, "TST_IMPORTANCE", "SCORE", 0);

			//TST_INCIDENT_PRIORITY
			SQLUtilities.AddInt32Column(_connection, "TST_INCIDENT_PRIORITY", "SCORE");

			//TST_INCIDENT_SEVERITY
			SQLUtilities.AddInt32Column(_connection, "TST_INCIDENT_SEVERITY", "SCORE");

			//Now we need to create the per-project-template entries
			foreach (int projectId in projectIds)
			{
				//The template id is the same as the project id for migrated systems (one to one)
				int projectTemplateId = projectId;

				//Add the priorities/importances

				//TST_IMPORTANCE
				Dictionary<int, long> requirementImportanceMappings = new Dictionary<int, long>();
				requirementImportanceMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_IMPORTANCE", "INSERT INTO TST_IMPORTANCE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '1 - Critical', 1, 'f47457', 1)", "IMPORTANCE_ID"));
				requirementImportanceMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_IMPORTANCE", "INSERT INTO TST_IMPORTANCE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '2 - High', 1, 'f29e56', 2)", "IMPORTANCE_ID"));
				requirementImportanceMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_IMPORTANCE", "INSERT INTO TST_IMPORTANCE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '3 - Medium', 1, 'f5d857', 3)", "IMPORTANCE_ID"));
				requirementImportanceMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_IMPORTANCE", "INSERT INTO TST_IMPORTANCE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '4 - Low', 1, 'f4f356', 4)", "IMPORTANCE_ID"));

				//TST_TEST_CASE_PRIORITY
				Dictionary<int, long> testCasePriorityMappings = new Dictionary<int, long>();
				testCasePriorityMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_PRIORITY", "INSERT INTO TST_TEST_CASE_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '1 - Critical', 1, 'f47457', 1)", "TEST_CASE_PRIORITY_ID"));
				testCasePriorityMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_PRIORITY", "INSERT INTO TST_TEST_CASE_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '2 - High', 1, 'f29e56', 2)", "TEST_CASE_PRIORITY_ID"));
				testCasePriorityMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_PRIORITY", "INSERT INTO TST_TEST_CASE_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '3 - Medium', 1, 'f5d857', 3)", "TEST_CASE_PRIORITY_ID"));
				testCasePriorityMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_PRIORITY", "INSERT INTO TST_TEST_CASE_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '4 - Low', 1, 'f4f356', 4)", "TEST_CASE_PRIORITY_ID"));

				//TST_TASK_PRIORITY
				Dictionary<int, long> taskPriorityMappings = new Dictionary<int, long>();
				taskPriorityMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_PRIORITY", "INSERT INTO TST_TASK_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '1 - Critical', 1, 'f47457', 1)", "TASK_PRIORITY_ID"));
				taskPriorityMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_PRIORITY", "INSERT INTO TST_TASK_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '2 - High', 1, 'f29e56', 2)", "TASK_PRIORITY_ID"));
				taskPriorityMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_PRIORITY", "INSERT INTO TST_TASK_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '3 - Medium', 1, 'f5d857', 3)", "TASK_PRIORITY_ID"));
				taskPriorityMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_PRIORITY", "INSERT INTO TST_TASK_PRIORITY (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, COLOR, SCORE) VALUES (" + projectTemplateId + ", '4 - Low', 1, 'f4f356', 4)", "TASK_PRIORITY_ID"));

				//Next the types

				//TST_REQUIREMENT_TYPE
				Dictionary<int, long> requirementTypeMappings = new Dictionary<int, long>();
				requirementTypeMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Need', 'Requirement.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 1 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Feature', 'Requirement.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 2 AND PROJECT_ID = " + projectId + "), 0, 1, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Use Case', 'UseCase.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 3 AND PROJECT_ID = " + projectId + "), 1, 0, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'User Story', 'UserStory.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 4 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(5, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Quality', 'Quality.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 5 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(6, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Design Element', 'Requirement.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_TYPE_PROJECT_WORKFLOW WHERE REQUIREMENT_TYPE_ID = 6 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "REQUIREMENT_TYPE_ID"));
				requirementTypeMappings.Add(7, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_REQUIREMENT_TYPE", "INSERT INTO TST_REQUIREMENT_TYPE (PROJECT_TEMPLATE_ID, NAME, ICON, IS_ACTIVE, REQUIREMENT_WORKFLOW_ID, IS_STEPS, IS_DEFAULT, IS_KEY_TYPE) VALUES (" + projectTemplateId + ", 'Acceptance Criteria', 'Requirement.gif', 1, (SELECT TOP 1 REQUIREMENT_WORKFLOW_ID FROM TST_REQUIREMENT_WORKFLOW WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = " + projectTemplateId + "), 0, 0, 0)", "REQUIREMENT_TYPE_ID"));

				//TST_TASK_TYPE
				Dictionary<int, long> taskTypeMappings = new Dictionary<int, long>();
				taskTypeMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Development', 1, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_TYPE_PROJECT_WORKFLOW WHERE TASK_TYPE_ID = 1 AND PROJECT_ID = " + projectId + "), 1, 0, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Testing', 2, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_TYPE_PROJECT_WORKFLOW WHERE TASK_TYPE_ID = 2 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Management', 3, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_TYPE_PROJECT_WORKFLOW WHERE TASK_TYPE_ID = 3 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Infrastructure', 4, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_TYPE_PROJECT_WORKFLOW WHERE TASK_TYPE_ID = 4 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(5, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Other', 5, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_TYPE_PROJECT_WORKFLOW WHERE TASK_TYPE_ID = 5 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(6, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Code Review', 6, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_WORKFLOW WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = " + projectTemplateId + "), 0, 1, 0)", "TASK_TYPE_ID"));
				taskTypeMappings.Add(7, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TASK_TYPE", "INSERT INTO TST_TASK_TYPE (NAME, POSITION, IS_ACTIVE, PROJECT_TEMPLATE_ID, TASK_WORKFLOW_ID, IS_DEFAULT, IS_CODE_REVIEW, IS_PULL_REQUEST) VALUES ('Pull Request', 7, 1, " + projectTemplateId + ", (SELECT TOP 1 TASK_WORKFLOW_ID FROM TST_TASK_WORKFLOW WHERE IS_DEFAULT = 1 AND PROJECT_TEMPLATE_ID = " + projectTemplateId + "), 0, 0, 1)", "TASK_TYPE_ID"));

				//TST_TEST_CASE_TYPE
				Dictionary<int, long> testCaseTypeMappings = new Dictionary<int, long>();
				testCaseTypeMappings.Add(1, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Acceptance', 1, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 1 AND PROJECT_ID = " + projectId + "), 0, 0, 1)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(2, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Compatibility', 2, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 2 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(3, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Functional', 3, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 3 AND PROJECT_ID = " + projectId + "), 1, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(4, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Integration', 4, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 4 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(5, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Load/Performance', 5, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 5 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(6, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Network', 6, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 6 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(7, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Regression', 7, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 7 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(8, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Scenario', 8, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 8 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(9, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Security', 9, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 8 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(10, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Unit', 10, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 10 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(11, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Usability', 11, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 11 AND PROJECT_ID = " + projectId + "), 0, 0, 0)", "TEST_CASE_TYPE_ID"));
				testCaseTypeMappings.Add(12, SQLUtilities.ExecuteIdentityInsert(_connection, "TST_TEST_CASE_TYPE", "INSERT INTO TST_TEST_CASE_TYPE (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, TEST_CASE_WORKFLOW_ID, IS_DEFAULT, IS_EXPLORATORY, IS_BDD) VALUES (" + projectTemplateId + ", 'Exploratory', 12, 1, (SELECT TOP 1 TEST_CASE_WORKFLOW_ID FROM TST_TEST_CASE_TYPE_PROJECT_WORKFLOW WHERE TEST_CASE_TYPE_ID = 12 AND PROJECT_ID = " + projectId + "), 0, 1, 0)", "TEST_CASE_TYPE_ID"));

				//Now update the artifact records, data mapping and history entries that use these values
				//Requirement Importance Id
				foreach (KeyValuePair<int, long> kvp in requirementImportanceMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 18 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_REQUIREMENT SET IMPORTANCE_ID = " + newId + " WHERE IMPORTANCE_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 18 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 18 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}

				//Requirement Type Id
				foreach (KeyValuePair<int, long> kvp in requirementTypeMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 140 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_REQUIREMENT SET REQUIREMENT_TYPE_ID = " + newId + " WHERE REQUIREMENT_TYPE_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 140 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 140 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}

				//Task Priority Id
				foreach (KeyValuePair<int, long> kvp in taskPriorityMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 59 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_TASK SET TASK_PRIORITY_ID = " + newId + " WHERE TASK_PRIORITY_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 59 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 59 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}

				//Task Type Id
				foreach (KeyValuePair<int, long> kvp in taskTypeMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 145 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_TASK SET TASK_TYPE_ID = " + newId + " WHERE TASK_TYPE_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 145 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 145 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}

				//Test Case Priority Id
				foreach (KeyValuePair<int, long> kvp in testCasePriorityMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 24 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_TEST_CASE SET TEST_CASE_PRIORITY_ID = " + newId + " WHERE TEST_CASE_PRIORITY_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 24 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 24 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}

				//Test Case Type Id
				foreach (KeyValuePair<int, long> kvp in testCaseTypeMappings)
				{
					int oldId = kvp.Key;
					long newId = kvp.Value;
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_DATA_SYNC_ARTIFACT_FIELD_VALUE_MAPPING SET ARTIFACT_FIELD_VALUE = " + newId + " WHERE ARTIFACT_FIELD_VALUE = " + oldId + " AND ARTIFACT_FIELD_ID = 167 AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_TEST_CASE SET TEST_CASE_TYPE_ID = " + newId + " WHERE TEST_CASE_TYPE_ID = " + oldId + " AND PROJECT_ID = " + projectId);
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET OLD_VALUE_INT = " + newId + " WHERE OLD_VALUE_INT = " + oldId + " AND FIELD_ID = 167 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
					SQLUtilities.ExecuteCommand(_connection, "UPDATE TST_HISTORY_DETAIL SET NEW_VALUE_INT = " + newId + " WHERE NEW_VALUE_INT = " + oldId + " AND FIELD_ID = 167 AND CHANGESET_ID IN (SELECT CHANGESET_ID FROM TST_HISTORY_CHANGESET WHERE PROJECT_ID = " + projectId + ")");
				}
			}
		}

		/// <summary>Deletes any full text indexes</summary>
		private void DeleteFullTextIndexes()
		{
			//Update the status bar.
			UpdateProgress();

			string commands = @"
IF EXISTS (SELECT * FROM sys.sysfulltextcatalogs ftc WHERE ftc.name = N'FT_ARTIFACTS')
BEGIN
    DROP FULLTEXT INDEX ON TST_ATTACHMENT;
END
";

			SQLUtilities.ExecuteSqlCommands(_connection, commands, true);
		}

		/// <summary>Creates any full text indexes</summary>
		/// <remarks>
		/// We create the ones that changed in v6.0 as well creating any ones that might be missing
		/// in the case that SQL full text indexing was not originally enabled and then turned on post-install
		/// </remarks>
		private void CreateFullTextIndexes()
		{
			//Update progress bar.
			UpdateProgress();

			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "CreateFullTextIndexes.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);
		}

		/// <summary>Migrates the notification templates to be project template based</summary>
		private void MigrateNotificationTemplates()
		{
			//Update progress bar.
			UpdateProgress();

			//Get the SQL.
			string sqlQuery1 = ZipReader.GetContents("DB_v600.zip", "MigrateNotificationTemplates.sql");
			SQLUtilities.ExecuteSqlCommands(_connection, sqlQuery1);

			//Get the new template for risks
			string riskNotificationTemplate = ZipReader.GetContents("DB_v600.zip", "TST_ARTIFACT_NOTIFICATION_TEMPLATE.sql");

			//Now we need to create the per-project-template entries
			foreach (long projectId in projectIds)
			{
				//The template id is the same as the project id for migrated systems (one to one)
				long projectTemplateId = projectId;

				//For the existing ones (i.e. not risks) we simply have to copy what we have for reach project
				SQLUtilities.ExecuteCommand(_connection,
					"INSERT INTO TST_NOTIFICATION_ARTIFACT_TEMPLATE (ARTIFACT_TYPE_ID, PROJECT_TEMPLATE_ID, TEMPLATE_TEXT) SELECT ARTIFACT_TYPE_ID, " + projectTemplateId + " AS PROJECT_TEMPLATE_ID, TEMPLATE_TEXT FROM TST_NOTIFICATION_ARTIFACT_TEMPLATE WHERE PROJECT_TEMPLATE_ID IS NULL");

				//Now we need to add the new risk notification template
				string sqlToExecute = riskNotificationTemplate.Replace("[PROJECT_TEMPLATE_ID]", projectTemplateId.ToString());
				SQLUtilities.ExecuteCommand(_connection, sqlToExecute);
			}
		}

		/// <summary>Creates the new foreign keys</summary>
		private void CreateForeignKeys()
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

		/// <summary>Populate the initial workflows for Risks and Documents (previously did not have workflow)</summary>
		private void PopulateDocumentAndRiskInitialWorkflows(int projectId)
		{
			//Populate the tag cloud for each project
			#region Document Workflows

			/* Document Statuses */
			long documentStatusId1 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Draft', 1, 1, 1, 1)", "DOCUMENT_STATUS_ID");
			long documentStatusId2 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Under Review', 2, 1, 1, 0)", "DOCUMENT_STATUS_ID");
			long documentStatusId3 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Approved', 3, 1, 1, 0)", "DOCUMENT_STATUS_ID");
			long documentStatusId4 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Completed', 4, 1, 0, 0)", "DOCUMENT_STATUS_ID");
			long documentStatusId5 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Rejected', 5, 1, 0, 0)", "DOCUMENT_STATUS_ID");
			long documentStatusId6 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Retired', 6, 1, 0, 0)", "DOCUMENT_STATUS_ID");
			long documentStatusId7 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_STATUS", "INSERT INTO TST_DOCUMENT_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN_STATUS, IS_DEFAULT) VALUES (" + projectId + ", 'Checked Out', 7, 1, 1, 0)", "DOCUMENT_STATUS_ID");

			/* Document Workflow */
			int documentWorkflowId = projectId;
			SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW", "INSERT INTO TST_DOCUMENT_WORKFLOW (DOCUMENT_WORKFLOW_ID, PROJECT_TEMPLATE_ID, NAME, IS_DEFAULT, IS_ACTIVE) VALUES (" + documentWorkflowId + ", " + projectId + ", 'Default Workflow', 1, 1)");

			/* Document Workflow Transitions */
			List<long> documentWorkflowTransitions = new List<long>();
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId1 + ", " + documentStatusId2 + ", 'Review Document', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId2 + ", " + documentStatusId1 + ", 'Return to Draft', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId2 + ", " + documentStatusId3 + ", 'Approve Document', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId2 + ", " + documentStatusId5 + ", 'Reject Document', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId3 + ", " + documentStatusId4 + ", 'Complete Document', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId3 + ", " + documentStatusId2 + ", 'Return to Review', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId5 + ", " + documentStatusId2 + ", 'Return to Review', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId4 + ", " + documentStatusId6 + ", 'Retire Document', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId6 + ", " + documentStatusId2 + ", 'Return to Review', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId4 + ", " + documentStatusId2 + ", 'Return to Review', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId3 + ", " + documentStatusId7 + ", 'Checkout', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			documentWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_DOCUMENT_WORKFLOW_TRANSITION", "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION (DOCUMENT_WORKFLOW_ID, INPUT_DOCUMENT_STATUS_ID, OUTPUT_DOCUMENT_STATUS_ID, NAME, IS_EXECUTE_BY_AUTHOR, IS_EXECUTE_BY_EDITOR, IS_SIGNATURE_REQUIRED) VALUES (" + documentWorkflowId + ", " + documentStatusId7 + ", " + documentStatusId3 + ", 'Checkin', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));

			//Add project owner and manager to all transitions
			foreach (long documentWorkflowTransitionId in documentWorkflowTransitions)
			{
				SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE (WORKFLOW_TRANSITION_ID, WORKFLOW_TRANSITION_ROLE_TYPE_ID, PROJECT_ROLE_ID) VALUES (" + documentWorkflowTransitionId + ", 1, 1)");
				SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_TRANSITION_ROLE (WORKFLOW_TRANSITION_ID, WORKFLOW_TRANSITION_ROLE_TYPE_ID, PROJECT_ROLE_ID) VALUES (" + documentWorkflowTransitionId + ", 1, 2)");
			}

			//Document Field States (we leave custom properties as optional, visible, enabled)
			//Field 149
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 2, " + documentStatusId1 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 2, " + documentStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 2, " + documentStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 149, 1, " + documentStatusId7 + ")");
			//Field 150
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 2, " + documentStatusId1 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 2, " + documentStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 2, " + documentStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 150, 1, " + documentStatusId7 + ")");
			//Field 152
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 2, " + documentStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 3, " + documentStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 152, 1, " + documentStatusId7 + ")");
			//Field 154
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 2, " + documentStatusId1 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 2, " + documentStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 2, " + documentStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 154, 1, " + documentStatusId7 + ")");
			//Field 159
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 159, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 159, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 159, 1, " + documentStatusId7 + ")");
			//Field 161
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 161, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 161, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 161, 1, " + documentStatusId7 + ")");
			//Field 179
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 179, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 179, 1, " + documentStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 179, 1, " + documentStatusId6 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 179, 1, " + documentStatusId7 + ")");
			//Field 180
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 180, 2, " + documentStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_DOCUMENT_WORKFLOW_FIELD (DOCUMENT_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, DOCUMENT_STATUS_ID) VALUES (" + documentWorkflowId + ", 180, 1, " + documentStatusId6 + ")");

			#endregion

			#region Risk Workflows

			/* Risk Statuses */
			long riskStatusId1 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Identified', 1, 1, 1, 1)", "RISK_STATUS_ID");
			long riskStatusId2 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Analyzed', 2, 1, 1, 0)", "RISK_STATUS_ID");
			long riskStatusId3 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Evaluated', 3, 1, 1, 0)", "RISK_STATUS_ID");
			long riskStatusId4 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Open', 4, 1, 1, 0)", "RISK_STATUS_ID");
			long riskStatusId5 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Closed', 5, 1, 0, 0)", "RISK_STATUS_ID");
			long riskStatusId6 = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_STATUS", "INSERT INTO TST_RISK_STATUS (PROJECT_TEMPLATE_ID, NAME, POSITION, IS_ACTIVE, IS_OPEN, IS_DEFAULT) VALUES (" + projectId + ", 'Rejected', 6, 1, 0, 0)", "RISK_STATUS_ID");

			/* Risk Impacts */
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_IMPACT (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Catastrophic', 1, 'A23520', 1, 4)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_IMPACT (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Critical', 2, 'D8472B', 1, 3)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_IMPACT (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Marginal', 3, 'E27560', 1, 2)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_IMPACT (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Negligible', 4, 'ECA395', 1, 1)");

			/* Risk Probabilities */
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_PROBABILITY (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Certain', 1, 'A23520', 1, 5)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_PROBABILITY (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Likely', 2, 'D8472B', 1, 4)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_PROBABILITY (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Possible', 3, 'E27560', 1, 3)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_PROBABILITY (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Unlikely', 4, 'ECA395', 1, 2)");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_PROBABILITY (PROJECT_TEMPLATE_ID, NAME, POSITION, COLOR, IS_ACTIVE, SCORE) VALUES (" + projectId + ", 'Rare', 5, 'ECC3BB', 1, 1)");

			/* Risk Workflow */
			long riskWorkflowId = SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW", "INSERT INTO TST_RISK_WORKFLOW (PROJECT_TEMPLATE_ID, NAME, IS_DEFAULT, IS_ACTIVE) VALUES (" + projectId + ", 'Default Workflow', 1, 1)", "RISK_WORKFLOW_ID");

			/* Risk Types */
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_TYPE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT, RISK_WORKFLOW_ID) VALUES (" + projectId + ", 'Business', 1, 1, " + riskWorkflowId + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_TYPE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT, RISK_WORKFLOW_ID) VALUES (" + projectId + ", 'Technical', 1, 0, " + riskWorkflowId + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_TYPE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT, RISK_WORKFLOW_ID) VALUES (" + projectId + ", 'Financial', 1, 0, " + riskWorkflowId + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_TYPE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT, RISK_WORKFLOW_ID) VALUES (" + projectId + ", 'Schedule', 1, 0, " + riskWorkflowId + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_TYPE (PROJECT_TEMPLATE_ID, NAME, IS_ACTIVE, IS_DEFAULT, RISK_WORKFLOW_ID) VALUES (" + projectId + ", 'Other', 1, 0, " + riskWorkflowId + ")");

			/* Risk Workflow Transitions */
			List<long> riskWorkflowTransitions = new List<long>();
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId1 + ", " + riskStatusId2 + ", 'Analyze Risk', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId2 + ", " + riskStatusId3 + ", 'Evaluate Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId3 + ", " + riskStatusId4 + ", 'Treat Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId4 + ", " + riskStatusId5 + ", 'Close Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId1 + ", " + riskStatusId6 + ", 'Reject Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId2 + ", " + riskStatusId6 + ", 'Reject Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId3 + ", " + riskStatusId6 + ", 'Reject Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId4 + ", " + riskStatusId6 + ", 'Reject Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId5 + ", " + riskStatusId4 + ", 'Reopen Risk', 0, 1, 0)", "WORKFLOW_TRANSITION_ID"));
			riskWorkflowTransitions.Add(SQLUtilities.ExecuteIdentityInsert(_connection, "TST_RISK_WORKFLOW_TRANSITION", "INSERT INTO TST_RISK_WORKFLOW_TRANSITION (RISK_WORKFLOW_ID, INPUT_RISK_STATUS_ID, OUTPUT_RISK_STATUS_ID, NAME, IS_EXECUTE_BY_CREATOR, IS_EXECUTE_BY_OWNER, IS_SIGNATURE_REQUIRED) VALUES (" + riskWorkflowId + ", " + riskStatusId6 + ", " + riskStatusId1 + ", 'Reopen Risk', 1, 1, 0)", "WORKFLOW_TRANSITION_ID"));

			//Add project owner and manager to all transitions
			foreach (long riskWorkflowTransitionId in riskWorkflowTransitions)
			{
				SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_TRANSITION_ROLE (WORKFLOW_TRANSITION_ID, WORKFLOW_TRANSITION_ROLE_TYPE_ID, PROJECT_ROLE_ID) VALUES (" + riskWorkflowTransitionId + ", 1, 1)");
				SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_TRANSITION_ROLE (WORKFLOW_TRANSITION_ID, WORKFLOW_TRANSITION_ROLE_TYPE_ID, PROJECT_ROLE_ID) VALUES (" + riskWorkflowTransitionId + ", 1, 2)");
			}

			//Risk Field States (we leave custom properties as optional, visible, enabled)
			//Field 184
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 184, 2, " + riskStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 184, 2, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 184, 2, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 184, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 184, 1, " + riskStatusId6 + ")");
			//Field 185
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 185, 2, " + riskStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 185, 2, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 185, 2, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 185, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 185, 1, " + riskStatusId6 + ")");
			//Field 187
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 187, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 187, 1, " + riskStatusId6 + ")");
			//Field 188
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 188, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 188, 1, " + riskStatusId6 + ")");
			//Field 189
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 189, 2, " + riskStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 189, 2, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 189, 2, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 189, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 189, 1, " + riskStatusId6 + ")");
			//Field 190
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 190, 2, " + riskStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 190, 2, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 190, 2, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 190, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 190, 1, " + riskStatusId6 + ")");
			//Field 191
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 191, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 191, 1, " + riskStatusId6 + ")");
			//Field 192
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 192, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 192, 1, " + riskStatusId6 + ")");
			//Field 193
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 193, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 193, 1, " + riskStatusId6 + ")");
			//Field 194
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 194, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 194, 1, " + riskStatusId6 + ")");
			//Field 196
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 3, " + riskStatusId1 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 3, " + riskStatusId2 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 3, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 3, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 2, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 196, 1, " + riskStatusId6 + ")");
			//Field 198
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 198, 3, " + riskStatusId1 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 198, 2, " + riskStatusId3 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 198, 2, " + riskStatusId4 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 198, 1, " + riskStatusId5 + ")");
			SQLUtilities.ExecuteCommand(_connection, "INSERT INTO TST_RISK_WORKFLOW_FIELD (RISK_WORKFLOW_ID, ARTIFACT_FIELD_ID, WORKFLOW_FIELD_STATE_ID, RISK_STATUS_ID) VALUES (" + riskWorkflowId + ", 198, 2, " + riskStatusId6 + ")");

			#endregion
		}

		/// <summary>Deletes all the existing foreign key restraints.</summary>
		private void DeleteExistingForeignKeyConstraints()
		{
			//Update progress bar.
			UpdateProgress();

			SQLUtilities.DeleteExistingForeignKeyConstraints(_connection);
		}

		#endregion Private SQL Functions
	}
}
