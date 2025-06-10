using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>This class will upgrade 672 to 673</summary>
	public class UpgradeTasks673 : IUpgradeDBInit, IUpgradeDB
	{
		#region Private Storage
		private string loggerStr = "Database Upgrade v" + DB_REV + ": ";
		#endregion Private Storage

		#region Public DB Revisions
		/// <summary>The ending revision of this upgrader.</summary>
		public const int DB_REV = 673;
		/// <summary>The minimum version of the Database allwed to upgrade.</summary>
		public const int DB_UPG_MIN = 672;
		/// <summary>The maximum version of the Database allowed to upgrade.</summary>
		public const int DB_UPG_MAX = 672;
		#endregion Public DB Revisions

		#region Interface Functions
		/// <summary>Creates new instance of the class!</summary>
		/// <param name="StaticDataFolder">The folder where the new static data SQLs are stored.</param>
		/// <param name="TemporaryFolder">The temporary folder.</param>
		public UpgradeTasks673(
			StreamWriter LogWriter,
			string RootDataFolder)
			: base(LogWriter, RootDataFolder)
		{
			//We don't do anything in this call.
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

			//1 - Fix as many potential upgrade differences as we can.
			_logger.WriteLine(loggerStr + "FixDifferences - Starting");
			FixDifferences();
			_logger.WriteLine(loggerStr + "FixDifferences - Finished");

			//2 - Static Data Changes
			_logger.WriteLine(loggerStr + "StaticUpdates - Starting");
			StaticUpdates();
			_logger.WriteLine(loggerStr + "StaticUpdates - Finished");

			//3 - Update Password fields.
			_logger.WriteLine(loggerStr + "UpdateVersionPasswordFields - Starting");
			UpdateVersionPasswordFields();
			_logger.WriteLine(loggerStr + "UpdateVersionPasswordFields - Finished");

			//4 - Update WebHooks table.
			_logger.WriteLine(loggerStr + "UpdateWebhook - Starting");
			UpdateWebhook();
			_logger.WriteLine(loggerStr + "UpdateWebhook - Finished");

			//5 - Add Release notification Template
			_logger.WriteLine(loggerStr + "AddNotifcationTemplates - Starting");
			AddNotifcationTemplates();
			_logger.WriteLine(loggerStr + "AddNotifcationTemplates - Finished");

			//6 - Add the 'Description' custom table field.
			_logger.WriteLine(loggerStr + "AddCustPropField - Starting");
			AddCustPropField();
			_logger.WriteLine(loggerStr + "AddCustPropField - Finished");

			//7 - Fix possible wrong 'NOT NULL' fields from v672 upgrade bug.  (KB593)
			_logger.WriteLine(loggerStr + "FixNotNullFields - Starting");
			FixNotNullFields();
			_logger.WriteLine(loggerStr + "FixNotNullFields - Finished");

			//8 - Adds index to the SOURCE CODE COMMIT table to help performance.
			_logger.WriteLine(loggerStr + "AddIndex - Starting");
			AddIndex();
			_logger.WriteLine(loggerStr + "AddIndex - Finished");

			//9 - Update DB revision.
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Starting");
			UpdateDatabaseRevisionNumber(DB_REV);
			_logger.WriteLine(loggerStr + "UpdateDatabaseRevisionNumber - Finished");

			return true;
		}
		#endregion Interface Functions

		/// <summary>
		/// Handles a lot of forgotten or variances found between v5.4 -> v6.8 upgraed, and fresh v6.8 installed databases. See [IN:6181]
		/// </summary>
		private void FixDifferences()
		{
			//Update the progress bar..
			UpdateProgress();

			/* Drop the incorrect indexes, first. */
			string indexDropList = ZipReader.GetContents("DB_v673.zip", "IndexesToDrop.txt");
			//Loop through each index.
			string dropIdxSql = "IF EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'{0}') DROP INDEX {1} ON {2}";
			foreach (string line in indexDropList.Split('\n'))
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					string indexName = line.Split('|')[0].Trim();
					string tableName = line.Split('|')[1].Trim();

					string sqlExec = string.Format(dropIdxSql,
						indexName.Trim(new char[] { '[', ']' }),  //When in quotes, we can not have the []'s/
						indexName,
						tableName);
					SQLUtilities.ExecuteCommand(_connection, sqlExec);
				}
			}

			//Update the progress bar..
			UpdateProgress();

			/* Add new indexes. */
			string indexAddList = ZipReader.GetContents("DB_v673.zip", "IndexesToAdd.txt");
			//Loop through each index.
			string addIdxSql = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'{0}') " + SQLUtilities.SQL_CREATE_IDX;
			foreach (string line in indexAddList.Split('\n'))
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					string indexName = line.Split('|')[0].Trim();
					string tableName = line.Split('|')[1].Trim();
					string columnList = line.Split('|')[2].Trim();

					string sqlExec = string.Format(addIdxSql,
						indexName.Trim(new char[] { '[', ']' }),  //The used SQL already has []'s.
						tableName.Trim(new char[] { '[', ']' }),
						columnList);
					SQLUtilities.ExecuteCommand(_connection, sqlExec);
				}
			}

			//Update the progress bar..
			UpdateProgress();

			/* Add default constraints. */
			List<string> newConstTables = new List<string>
			{
				"TST_PORTFOLIO",
				"TST_PROJECT_GROUP",
				"TST_RELEASE"
			};
			foreach (string table in newConstTables) //Should I check to see if the default already exists? Will the server throw an error if it does? I DUNNO!
			{
				string constName = "DEF_" + table.ToUpperInvariant() + "_REQUIREMENT_COUNT";
				string.Format(
					"ALTER TABLE [{0}] ADD CONSTRAINT [{1}] DEFAULT ((0)) FOR [REQUIREMENT_COUNT]",
					table,
					constName);
			}

			//Update the progress bar..
			UpdateProgress();

			/* Add missing FKs. */
			string fkAddList = ZipReader.GetContents("DB_v673.zip", "FKsToAdd.txt");
			foreach (string line in fkAddList.Split('\n'))
			{
				if (!string.IsNullOrWhiteSpace(line))
				{
					//Split up our options. The format of the file is:
					//  Table To Alter | FK Name | Fields to Link (same for Both Tables) | Dest Table | Delete Action
					string tableAlt = line.Split('|')[0].Trim();
					string fkName = line.Split('|')[1].Trim();
					string fldLink = line.Split('|')[2].Trim();
					string tableRef = line.Split('|')[3].Trim();
					string fkAct = line.Split('|')[4].Trim();

					//Check that the FK exists or not.
					string checkSql = string.Format("SELECT COUNT(*) FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{0}') AND parent_object_id = OBJECT_ID(N'{1}')",
						fkName,
						tableAlt);
					var fkCheck = SQLUtilities.GetDataQuery(_connection, checkSql);
					//If the count is less than 1, it don't exist! Create it..!
					if (fkCheck.Tables[0].Rows.Count < 1 || (int)fkCheck.Tables[0].Rows[0].ItemArray[0] < 1)
					{
						//Convert the action into a proper string.
						string onAction = "";
						if (fkAct.Equals("CASCADE"))
							onAction = "ON DELETE CASCADE";
						else if (fkAct.Equals("NULL"))
							onAction = "ON DELETE SET NULL";

						// Execute the SQL.
						string cmdToRun = string.Format(SQLUtilities.SQL_CREATE_FK,
							fkName,
							tableRef,
							fldLink,
							tableAlt,
							fldLink,
							onAction);
						SQLUtilities.ExecuteCommand(_connection, cmdToRun);
					}
				}
			}

			//Update the progress bar..
			UpdateProgress();

			//Fix some potentially wrong columns from v672 upgrade? I have NVARCHAR(40) in my code, however a new DB
			//  build specifies 128. Possible it was changed after I wrote the code for the installer, or when pulling
			//  the number, I just copied the wrong field's size. Either way, we'll fix it here.
			//  Also, the original table (TST_WORKFLOW_TRANSISTION) had this set to 255. We will bump ALL up to 255.
			//  
			List<string> tables = new List<string>
			{
				"TST_RISK_WORKFLOW_TRANSITION",
				"TST_DOCUMENT_WORKFLOW_TRANSITION",
				"TST_REQUIREMENT_WORKFLOW_TRANSITION",
				"TST_TEST_CASE_WORKFLOW_TRANSITION",
				"TST_RELEASE_WORKFLOW_TRANSITION",
				"TST_TASK_WORKFLOW_TRANSITION",
				"TST_WORKFLOW_TRANSITION"
			};
			foreach (string table in tables)
			{
				//The template for updating the table. 
				string sqlFix = "ALTER TABLE [{0}] ALTER COLUMN [NOTIFY_SUBJECT] NVARCHAR(128) NULL;";
				//For each table, first check to veriy that the field is under 128 characters.
				string sqlCheck = string.Format(
					"SELECT COLUMNPROPERTY(OBJECT_ID('{0}'), 'NOTIFY_SUBJECT', 'PRECISION');",
					table);
				var fkCheck = SQLUtilities.GetDataQuery(_connection, sqlCheck);
				//Do the check. If it's less than 128 chars, update it to 128. Check that we 
				if (fkCheck.Tables[0].Rows.Count == 1 && (int)fkCheck.Tables[0].Rows[0].ItemArray[0] < 128)
					SQLUtilities.ExecuteCommand(_connection, string.Format(sqlFix, table));
			}
		}

		/// <summary>
		/// Inserts and updates static rows in TST_PROJECT_COLLECTION, TST_ARTIFACT_TYPE, TST_NOTIFICATION_ARTIFACT_TEMPLATE
		/// </summary>
		private void StaticUpdates()
		{
			//Update the progress bar..
			UpdateProgress();

			//Add new item to the TST_PROJECT_COLLECTION table.
			string sqlInsert1 = "INSERT INTO [TST_PROJECT_COLLECTION] VALUES ('RequirementsList.DocumentView', 'Y')";
			SQLUtilities.ExecuteCommand(_connection, sqlInsert1);

			//Update TST_ARTIFACT_TYPE to set IS_NOTIFY to yes for Releases.
			string sqlUpdate1 = "UPDATE [TST_ARTIFACT_TYPE] SET [IS_NOTIFY] = 1 WHERE [ARTIFACT_TYPE_ID] = 4";
			SQLUtilities.ExecuteCommand(_connection, sqlUpdate1);
		}

		/// <summary>
		/// Adds an index to the Source Control Commits table to help speed.
		/// </summary>
		private void AddIndex()
		{
			//Update the progress bar..
			UpdateProgress();

			SQLUtilities.AddIndex(
				_connection,
				"AK_TST_SOURCE_CODE_COMMIT_2",
				"TST_SOURCE_CODE_COMMIT",
				new List<SQLUtilities.ColumnDirection> {
					new SQLUtilities.ColumnDirection() {
						ColumnName= "UPDATE_DATE",
						Descending=true}
				}
			);
		}

		/// <summary>
		/// Updates the two Version Control tables, adjusting password fields.
		/// </summary>
		private void UpdateVersionPasswordFields()
		{
			//Update the progress bar..
			UpdateProgress();

			//For each table, we are:
			// - Updating the PASSWORD field, and
			// - Adding the IS_ENCRYPTED field.
			List<string> tables = new List<string> { "TST_VERSION_CONTROL_SYSTEM", "TST_VERSION_CONTROL_PROJECT" };
			foreach (string table in tables)
			{
				//Update the field.
				string sqlUpdate = "ALTER TABLE [{0}] ALTER COLUMN [PASSWORD] NVARCHAR(MAX) {1}NULL";
				SQLUtilities.ExecuteCommand(
					_connection,
					string.Format(
						sqlUpdate,
						table,
						(table.Equals("TST_VERSION_CONTROL_SYSTEM") ? "NOT " : ""))
				);

				//Add the new field.
				string sqlAdd = "ALTER TABLE [{0}] ADD [IS_ENCRYPTED] BIT NOT NULL CONSTRAINT [DEF_{0}_IS_ENCRYPTED] DEFAULT (0)";
				SQLUtilities.ExecuteCommand(
					_connection,
					string.Format(sqlAdd, table));
			}
		}

		/// <summary>
		/// Increase password field, and add 'METHOD' field.
		/// </summary>
		private void UpdateWebhook()
		{
			//Update the progress bar..
			UpdateProgress();

			//Increase password field legnth.
			string table = "TST_NOTIFICATION_EVENT_WEBHOOK";
			SQLUtilities.ExecuteCommand(
				_connection,
				string.Format("ALTER TABLE [{0}] ALTER COLUMN [PASSWORD] NVARCHAR(MAX) NULL", table));

			//Add the METHOD field.
			SQLUtilities.ExecuteCommand(
				_connection,
				string.Format("ALTER TABLE [{0}] ADD [METHOD] NVARCHAR(10) NULL", table));
		}

		/// <summary>
		/// Inserts the default template into the TST_NOTIFICATION_ARTIFACT_TEMPLATE table for the new Release notifications.
		/// Repeated for every template.
		/// </summary>
		private void AddNotifcationTemplates()
		{
			//Update the progress bar..
			UpdateProgress();

			//First, get the string from the ZIP file.
			string XmlText = ZipReader.GetContents("DB_v673.zip", "emailTemplate_releases_inline_default.htm");
			XmlText = SQLUtilities.SqlEncode(XmlText);

			//Pull all our templates.
			string sqlSelect = "SELECT DISTINCT([PROJECT_TEMPLATE_ID]) FROM [TST_PROJECT_TEMPLATE] ORDER BY [PROJECT_TEMPLATE_ID]";
			var dataTable = SQLUtilities.GetDataQuery(_connection, sqlSelect);

			//The string used to populate.
			string sqlInsertFmt = "INSERT INTO [TST_NOTIFICATION_ARTIFACT_TEMPLATE] ([ARTIFACT_TYPE_ID], [PROJECT_TEMPLATE_ID], [TEMPLATE_TEXT]) VALUES (4, {0}, '{1}')";

			//Loop through each template ID we have.
			foreach (DataRow templateRow in dataTable.Tables[0].Rows)
			{
				//Pull the Template ID.
				int? templateId = templateRow.ItemArray[0] as int?;

				//Make sure we have a valid value.
				if (templateId.HasValue)
				{
					App.logFile.WriteLine("Inserting template for Template #" + templateId + ".");

					string sqlInsert = string.Format(sqlInsertFmt, templateId.Value, XmlText);
					SQLUtilities.ExecuteCommand(_connection, sqlInsert);
				}
			}

			//Now, insert a null entry, for new Templates.
			SQLUtilities.ExecuteCommand(_connection, string.Format(sqlInsertFmt, "NULL", XmlText));

		}

		/// <summary>
		/// Adds the new DESCRIPTION field to the TST_CUSTOM_PROPERTY table.
		/// </summary>
		private void AddCustPropField()
		{
			//Update the progress bar..
			UpdateProgress();

			SQLUtilities.AddStringColumn(_connection, "TST_CUSTOM_PROPERTY", "DESCRIPTION", 512);
		}


		/// <summary>
		/// Fixes and potential wrong 'NOT NULL' string fields that were added in the early v6.8 installer
		/// that had a bug and reversed the IF statement on NOT NULL/NULL column definition.
		/// </summary>
		private void FixNotNullFields()
		{
			//Update the progress bar..
			UpdateProgress();

			//Our collection of fields. First one is for (255), second is for (MAX).
			Dictionary<string, string> strFields255 = new Dictionary<string, string>
			{
				{"[OAUTH_ACCESS_TOKEN]","[TST_USER]"},
				{"[MFA_TOKEN]","[TST_USER]"},
				{"[MFA_PHONE]","[TST_USER]"}
			};
			Dictionary<string, string> strFieldsMAX = new Dictionary<string, string>
			{
				{"[TST_TEST_STEP]", "[PRECONDITION]" },
				{"[TST_TIMECARD]", "[APPROVER_COMMENTS]" },
				{"[TST_TIMECARD_ENTRY]", "[DESCRIPTION]" },
				{"[TST_TIMECARD_ENTRY_TYPE]", "[DESCRIPTION]" }
			};

			//Loop through the fields of each, changing them to NULL.
			foreach (var fieldTable in strFields255)
			{
				string sqlExecute = string.Format(
					"ALTER TABLE {1} ALTER COLUMN {0} NVARCHAR(255) NULL",
					fieldTable.Key,
					fieldTable.Value);

				SQLUtilities.ExecuteCommand(_connection, sqlExecute);
			}
			foreach (var fieldTable in strFieldsMAX)
			{
				string sqlExecute = string.Format(
					"ALTER TABLE {0} ALTER COLUMN {1} NVARCHAR(MAX) NULL",
					fieldTable.Key,
					fieldTable.Value);

				SQLUtilities.ExecuteCommand(_connection, sqlExecute);
			}
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
