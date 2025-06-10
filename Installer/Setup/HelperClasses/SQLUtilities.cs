using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Contains various utilities that are used by the installer</summary>
	public static class SQLUtilities
	{
		//For writing messags out.
		public static StreamWriter logWriter = null;

		//The SQL Server command timeout
		private static int COMMAND_TIMEOUT = 1800;  //30 minutes
		private static int SUBSTRING = 512; //Trim length of SQL commands.

		/// <summary>
		/// The string to add a column to the table..
		/// 0 - Table Name (Brackets Added)
		/// 1 - Column Name (Brackets Added)
		/// 2 - Data Type
		/// 3 - NULL/NOT NULL
		/// 4 - DEFAULT
		/// </summary>
		private const string SQL_ALTER_TABLE = "ALTER TABLE [{0}] ADD [{1}] {2} {4} {3}";

		/// <summary>
		/// The string to add a column to the table..
		/// 0 - Table Name
		/// 1 - Default Constraint
		///</summary>
		private const string SQL_ALTER_TABLE_DEFAULT = "CONSTRAINT [{1}] DEFAULT ({0})";

		/// <summary>
		/// The string to create an index.
		/// 0 - The Index name.
		/// 1 - The table name.
		/// 2 - The field to create the index on.
		/// </summary>
		private const string SQL_CREATE_INDEX = "CREATE INDEX [{0}] ON [{1}] ([{2}])";

		/// <summary>
		/// Creates a foreign key in the database.
		/// 0 - Constraint Name
		/// 1 - Master Table
		/// 2 - Master Table Fields
		/// 3 - Child Table
		/// 4 - Child Table Fields
		/// 5 - OnDelete/OnUpdate clause(s).
		/// </summary>
		internal const string SQL_CREATE_FK = "ALTER TABLE [{3}] ADD CONSTRAINT [{0}] FOREIGN KEY ({4}) REFERENCES [{1}] ({2}) {5};";

		/// <summary>
		/// Creates a new NON-CLUSTERED index.
		/// 0 - Index Name
		/// 1 - Table
		/// 2 - Columns
		/// </summary>
		internal const string SQL_CREATE_IDX = "CREATE NONCLUSTERED INDEX [{0}] ON [{1}] ({2});";

		/// <summary>
		/// Drops the specified index.
		/// 0 - Table Name
		/// 1 - Index Name
		/// </summary>
		private const string SQL_DROP_IDX = "DROP INDEX [{0}].[{1}];";

		/// <summary>The Regex for finding a 'GO'!</summary>
		private static Regex GO_SPLIT = new Regex("^GO\\s?(?:\\r?\\n|\\r|$)", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>This function takes input strings and renders them safe to use in a SQL query</summary>
		/// <param name="input">Input string that needs encoding</param>
		public static string SqlEncode(string input)
		{
			return (input.Replace("'", "''"));
		}

		/// <summary>Executes a SQL string containing multiple SQL statements</summary>
		/// <param name="commandList">The contents of the SQL command file</param>
		/// <param name="delimiter">The delimiter between SQL statements</param>
		/// <param name="connection">The details for connecting to the database.</param>
		public static void ExecuteSqlCommands(DBConnection connection, string commandList, bool failQuietly = false)
		{
			//Now split up the SQL statements
			string[] sqlCommands = GO_SPLIT.Split(commandList);

			using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
			{
				using (SqlCommand command = makeCommand(con))
				{
					try
					{
						command.Connection.Open();
						command.CommandType = CommandType.Text;

						//Execute each command in turn
						for (int i = 0; i < sqlCommands.Length; i++)
						{
							if (!string.IsNullOrWhiteSpace(sqlCommands[i]))
							{
								try
								{
									command.CommandText = sqlCommands[i];
									command.ExecuteNonQuery();
								}
								catch (Exception ex)
								{
									if (failQuietly)
									{
										WriteLog("While executing SQL command:" +
											Environment.NewLine +
											">>>" +
											command.CommandText.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') +
											Environment.NewLine +
											"<<<" +
											Environment.NewLine +
											Logger.DecodeException(ex)
										);
									}
									else
										throw;
								}
							}
						}
					}
					catch (Exception ex)
					{
						WriteLog("While executing SQL command:" +
							Environment.NewLine +
							">>>" +
							command.CommandText.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') +
							Environment.NewLine +
							"<<<" +
							Environment.NewLine +
							Logger.DecodeException(ex)
						);
						throw;
					}
					finally
					{
						con.Close();
					}
				}
			}
		}

		/// <summary>Executes a simple database command </summary>
		/// <param name="connection">SQL erver connection information.</param>
		/// <param name="query">The SQL Query</param>
		public static void ExecuteCommand(DBConnection connection, string query)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					using (SqlCommand command = makeCommand(con))
					{
						//Open connectoion.
						command.Connection.Open();
						command.CommandType = CommandType.Text;

						//Split the command up by 'GO's.
						List<string> commands = GO_SPLIT.Split(query).ToList();

						//Loop through each query found.
						foreach (var cmd in commands)
						{
							if (!string.IsNullOrWhiteSpace(cmd))
							{
								//Run command.
								command.CommandText = cmd;
								command.ExecuteNonQuery();
							}
						}

						//Close connection.
						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("Processing aborted. Error while executing SQL command:" + Environment.NewLine +
					"-- " + query.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}
		}

		/// <summary>
		/// Executes a simple query that returns back one value
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="query">The query</param>
		/// <returns>The object</returns>
		public static object ExecuteScalarCommand(DBConnection connection, string query)
		{
			object result = null;
			try
			{
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					using (SqlCommand command = makeCommand(con))
					{
						//Open connection and execute command
						command.Connection.Open();
						command.CommandType = CommandType.Text;
						command.CommandText = query;
						result = command.ExecuteScalar();

						//Close connection.
						command.Connection.Close();
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				WriteLog("Processing aborted. Error while executing SQL command:" + Environment.NewLine +
					"-- " + query.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}
		}

		/// <summary>Executes the specified command, insering parameters to run with it.</summary>
		/// <param name="connection">SQL erver connection information.</param>
		/// <param name="query">The SQL Query</param>
		/// <param name="parameters">Parameters to insert.</param>
		public static void ExecuteCommandParameters(DBConnection connection, string query, Dictionary<string, string> parameters)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					using (SqlCommand command = makeCommand(con))
					{
						//Open connectoion.
						command.Connection.Open();
						command.CommandType = CommandType.Text;

						//Add parameters.
						command.Parameters.Clear();
						foreach (var param in parameters)
						{
							SqlParameter sqlParam = new SqlParameter
							{
								ParameterName = "@" + param.Key,
								Value = param.Value
							};

							command.Parameters.Add(sqlParam);
						}

						if (!string.IsNullOrWhiteSpace(query))
						{
							//Run command.
							command.CommandText = query;
							command.ExecuteNonQuery();
						}

						//Close connection.
						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("Processing aborted. Error while executing SQL command:" + Environment.NewLine +
					"-- " + query.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}
		}

		/// <summary>Executes a query that needs to return the new identify generated</summary>
		/// <param name="tableName">The name of the databasetable</param>
		/// <param name="connection">The SQL connection information</param>
		/// <param name="query">The SQL Query</param>
		/// <returns>Highest ID of the new column.</returns>
		public static long ExecuteIdentityInsert(DBConnection connection, string tableName, string query, string identityColumn)
		{
			//Return value.
			long retValue = 0;

			try
			{
				//Create the connection & the command.
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					using (SqlCommand command = makeCommand(con))
					{
						//Generate the full SQL. We add the column, and then pull back the highest inserted ID.
						string sqlCmd = query +
							"; SELECT " + identityColumn + " FROM " + tableName + " WHERE " + identityColumn + " = @@IDENTITY;";

						//Open connectoion.
						command.Connection.Open();

						//Run command.
						command.CommandText = sqlCmd;
						command.CommandType = CommandType.Text;
						retValue = Convert.ToInt64((int)command.ExecuteScalar());

						//Close connection.
						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("While creating new Identity column:" + Environment.NewLine +
					"-- " + query.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}

			return retValue;
		}

		/// <summary>Executes a query where we need to specify the identity ID rather than let the database generate it for us</summary>
		/// <param name="tableName">The name of the databasetable</param>
		/// <param name="command">The SqlCommand object</param>
		/// <param name="query">The SQL Query</param>
		public static void ExecuteIdentityInsert(DBConnection connection, string tableName, string query)
		{
			try
			{
				//Create the connection & the command.
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					using (SqlCommand command = makeCommand(con))
					{
						//Generate the full command.
						string sqlCmd =
							"SET IDENTITY_INSERT [" + tableName + "] ON; " +
							query +
							"; SET IDENTITY_INSERT [" + tableName + "] OFF;";

						//Open connectoion.
						command.Connection.Open();

						//Run command.
						command.CommandText = sqlCmd;
						command.CommandType = CommandType.Text;
						command.ExecuteNonQuery();

						//Close connection.
						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("While insetrting record with identity column specified:" + Environment.NewLine +
					"-- " + query.SafeSubstring(1, SUBSTRING).Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine +
					Logger.DecodeException(ex)
				);
				throw;
			}
		}

		/// <summary>Converts a XXXX_YN field to a IS_XXXX bitfield</summary>
		/// <param name="hasDefaultValueSet">
		/// null = no default value
		/// false = Default Value = 0
		/// true = Default Value = 1
		/// </param>
		public static void ConvertFlagToBitField(DBConnection connection, string table, string oldField, string newField, bool? hasDefaultValueSet = null)
		{
			string alterSql;
			if (hasDefaultValueSet.HasValue)
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	DROP CONSTRAINT [DEF_" + table + "_" + oldField + @"]
GO
ALTER TABLE [" + table + @"]
	ADD [" + newField + @"] BIT CONSTRAINT [DEF_" + table + "_" + newField + @"] DEFAULT " + (hasDefaultValueSet.Value ? "1" : "0") + @"
GO
UPDATE [" + table + @"] SET " + newField + @" = 1 WHERE " + oldField + @" = 'Y'
GO
UPDATE [" + table + @"] SET " + newField + @" = 0 WHERE " + oldField + @" = 'N'
GO
ALTER TABLE [" + table + @"]
	DROP COLUMN [" + oldField + @"]
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + newField + @"] BIT NOT NULL
GO
";
			}
			else
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + newField + @"] BIT
GO
UPDATE [" + table + @"] SET " + newField + @" = 1 WHERE " + oldField + @" = 'Y'
GO
UPDATE [" + table + @"] SET " + newField + @" = 0 WHERE " + oldField + @" = 'N'
GO
ALTER TABLE [" + table + @"]
	DROP COLUMN [" + oldField + @"]
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + newField + @"] BIT NOT NULL
GO
";
			}
			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Converts a PROJECT_ID field to PROJECT_TEMPLATE_ID field</summary>
		/// <remarks>It doesn't change the data, just the name</remarks>
		public static void ConvertProjectIdToProjectTemplateId(DBConnection connection, string table)
		{
			string alterSql = "EXEC sp_rename '" + table + ".PROJECT_ID', 'PROJECT_TEMPLATE_ID', 'COLUMN';";
			ExecuteSqlCommands(connection, alterSql);
		}

		#region Add New Column Funcs
		/// <summary>Adds a date column and optionally populates with the current date</summary>
		public static void AddDateColumn(DBConnection connection, string table, string field, bool populateWithCurrentDate)
		{
			string alterSql;
			if (populateWithCurrentDate)
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] DATETIME
GO
UPDATE [" + table + @"] SET " + field + @" = GETUTCDATE()
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + field + @"] DATETIME NOT NULL
GO
";
			}
			else
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] DATETIME
";
			}

			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Adds a Guid column and optionally populates with the specified value</summary>
		public static void AddGuidColumn(DBConnection connection, string table, string field, Guid? defaultValue = null, string addIndex = "")
		{
			string alterSql;
			if (defaultValue.HasValue)
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] UNIQUEIDENTIFIER
GO
UPDATE [" + table + @"] SET " + field + @" = CAST('" + defaultValue.Value.ToString() + @"' AS UNIQUEIDENTIFIER)
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + field + @"] UNIQUEIDENTIFIER NOT NULL
GO
";
			}
			else
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] UNIQUEIDENTIFIER
";
			}

			//Also add an index if specified
			if (!string.IsNullOrEmpty(addIndex))
			{
				alterSql += @"
CREATE  INDEX [" + addIndex + @"] ON [" + table + @"] ([" + field + @"])
GO
";
			}

			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Adds an int column and optionally populates with the specified value</summary>
		public static void AddInt32Column(DBConnection connection, string table, string field, int? defaultValue = null, string addIndex = "")
		{
			//We assume that if a default is given, the field is NOT NULL.

			string defconName = "DEF_" + table.ToUpperInvariant() + "_" + field.ToUpperInvariant();
			string nullNot = ((defaultValue.HasValue) ? "NOT NULL" : "NULL");
			string defSql = ((defaultValue.HasValue)
				? string.Format(SQL_ALTER_TABLE_DEFAULT, defaultValue.Value.ToString(), defconName)
				: "");
			string alterSql1 = string.Format(SQL_ALTER_TABLE,
				table,
				field,
				"INTEGER",
				nullNot,
				defSql);

			ExecuteCommand(connection, alterSql1.Trim());

			//Also add an index if specified
			if (!string.IsNullOrEmpty(addIndex))
			{
				string alterSql2 = string.Format(SQL_CREATE_INDEX,
					addIndex,
					table,
					field);

				//Run the commands.
				ExecuteCommand(connection, alterSql2);
			}
		}

		/// <summary>Adds an bigint column and optionally populates with the specified value</summary>
		public static void AddInt64Column(DBConnection connection, string table, string field, long? defaultValue = null, string addIndex = "")
		{
			string alterSql;
			if (defaultValue.HasValue)
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] BIGINT
GO
UPDATE [" + table + @"] SET " + field + @" = " + defaultValue + @"
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + field + @"] BIGINT NOT NULL
GO
";
			}
			else
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] BIGINT
";
			}

			//Also add an index if specified
			if (!string.IsNullOrEmpty(addIndex))
			{
				alterSql += @"
CREATE  INDEX [" + addIndex + @"] ON [" + table + @"] ([" + field + @"])
GO
";
			}

			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Adds a bit column and optionally populates with the specified value</summary>
		public static void AddBitColumn(DBConnection connection, string table, string field, bool? defaultValue = null)
		{
			string defconName = "DEF_" + table.ToUpperInvariant() + "_" + field.ToUpperInvariant(); // The default constraint name.
			string defValue = ((defaultValue.HasValue && defaultValue.Value) ? "1" : "0"); // Set the default string.
			string nullNot = ((defaultValue.HasValue) ? "NOT NULL" : "NULL");
			string defSql = ((defaultValue.HasValue)
				? string.Format(SQL_ALTER_TABLE_DEFAULT, defValue, defconName)
				: "");
			string alterSql1 = string.Format(SQL_ALTER_TABLE,
				table,
				field,
				"BIT",
				nullNot,
				defSql);

			ExecuteCommand(connection, alterSql1.Trim());
		}

		/// <summary>Adds a bit column and optionally populates with the specified value</summary>
		public static void AddDecimalColumn(DBConnection connection, string table, string field, int precision, int scale, decimal? defaultValue = null, bool notNull = false)
		{
			//Check that we have a default value if not null.
			if (defaultValue == null && notNull)
				defaultValue = 0;

			string defconName = "DEF_" + table.ToUpperInvariant() + "_" + field.ToUpperInvariant();
			string sqlType = string.Format("DECIMAL({0},{1})", precision.ToString(), scale.ToString());
			string nullNot = ((notNull) ? "NOT NULL" : "NULL");
			string defSql = ((defaultValue.HasValue)
				? string.Format(SQL_ALTER_TABLE_DEFAULT, defaultValue.Value.ToString(), defconName)
				: "");
			string alterSql1 = string.Format(SQL_ALTER_TABLE,
				table,
				field,
				sqlType,
				nullNot,
				defSql);

			ExecuteCommand(connection, alterSql1.Trim());
		}

		/// <summary>Adds a CHAR(x) column and optionally populates with the specified value</summary>
		public static void AddCharColumn(DBConnection connection, string table, string field, int length, string defaultValue = null)
		{
			//TODO: Rewrite to use same code as AddInt32Column
			string alterSql;
			if (string.IsNullOrEmpty(defaultValue))
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] CHAR(" + length + @")
";
			}
			else
			{
				alterSql = @"
ALTER TABLE [" + table + @"]
	ADD [" + field + @"] CHAR(" + length + @")
GO
UPDATE [" + table + @"] SET " + field + @" = '" + SqlEncode(defaultValue) + @"'
GO
ALTER TABLE [" + table + @"]
	ALTER COLUMN [" + field + @"] CHAR(" + length + @") NOT NULL
GO
";
			}

			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Adds a string column and optionally populates with the specified value</summary>
		public static void AddStringColumn(DBConnection connection, string table, string field, int? length, string defaultValue = null)
		{
			string defconName = "DEF_" + table.ToUpperInvariant() + "_" + field.ToUpperInvariant();
			string sqlType = "NVARCHAR(" + ((length.HasValue && length.Value < int.MaxValue) ? length.Value.ToString() : "max") + ")";
			string nullNot = ((!string.IsNullOrWhiteSpace(defaultValue)) ? "NOT NULL" : "NULL");
			string defSql = ((!string.IsNullOrWhiteSpace(defaultValue))
				? string.Format(SQL_ALTER_TABLE_DEFAULT, "'" + defaultValue + "'", defconName)
				: "");
			string alterSql1 = string.Format(SQL_ALTER_TABLE,
				table,
				field,
				sqlType,
				nullNot,
				defSql);

			ExecuteCommand(connection, alterSql1.Trim());
		}
		#endregion Add New Column Funcs

		/// <summary>
		/// Creates a new Foreign key with the given values.
		/// </summary>
		/// <param name="keyName">The name of the foreign key.</param>
		/// <param name="parentTableName">The name of the parent (lookup) table.</param>
		/// <param name="parentTableField">Field in the parent (lookup) table.</param>
		/// <param name="childTableName">The name of the child table.</param>
		/// <param name="childTableField">The name of the child table field.</param>
		/// <param name="connection">Database Connecton</param>
		/// <param name="onDeleteAction">What to do on a Delete. (Default: Nothing)</param>
		/// <param name="onUpdateAction">What to do on an Update. (Default: Nothing)</param>
		public static void AddForeignKey(
			DBConnection connection,
			string keyName,
			string parentTableName,
			string parentTableField,
			string childTableName,
			string childTableField,
			ForeignKeyAction onDeleteAction = ForeignKeyAction.None,
			ForeignKeyAction onUpdateAction = ForeignKeyAction.None)
		{
			AddForeignKey(
				connection,
				keyName,
				parentTableName,
				new List<string> { parentTableField },
				childTableName,
				new List<string> { childTableField },
				onDeleteAction,
				onUpdateAction
			);
		}

		/// <summary>
		/// Creates a new Foreign key with the given values.
		/// </summary>
		/// <param name="keyName">The name of the foreign key.</param>
		/// <param name="parentTableName">The name of the parent (lookup) table.</param>
		/// <param name="parentTableFields">Fields in the parent (lookup) table.</param>
		/// <param name="childTableName">The name of the child table.</param>
		/// <param name="childTableFields">Fields in the child table.</param>
		/// <param name="connection">Database Connecton</param>
		/// <param name="onDeleteAction">What to do on a Delete. (Default: Nothing)</param>
		/// <param name="onUpdateAction">What to do on an Update. (Default: Nothing)</param>
		public static void AddForeignKey(
			DBConnection connection,
			string keyName,
			string parentTableName,
			List<string> parentTableFields,
			string childTableName,
			List<string> childTableFields,
			ForeignKeyAction onDeleteAction = ForeignKeyAction.None,
			ForeignKeyAction onUpdateAction = ForeignKeyAction.None)
		{
			//Generate the OnDelete/OnCasecase strings first.
			string onClause = "";
			if (onDeleteAction != ForeignKeyAction.None)
			{
				onClause += "ON DELETE ";
				switch (onDeleteAction)
				{
					case ForeignKeyAction.Cascade:
						onClause += "CASCADE";
						break;
					case ForeignKeyAction.Set_Default:
						onClause += "SET DEFAULT";
						break;
					case ForeignKeyAction.Set_Null:
						onClause += "SET NULL";
						break;
				}
			}
			if (onUpdateAction != ForeignKeyAction.None)
			{
				onClause += " ON UPDATE ";
				switch (onDeleteAction)
				{
					case ForeignKeyAction.Cascade:
						onClause += "CASCADE";
						break;
					case ForeignKeyAction.Set_Default:
						onClause += "SET DEFAULT";
						break;
					case ForeignKeyAction.Set_Null:
						onClause += "SET NULL";
						break;
				}
			}

			//Generate the Field Lists.
			// - Parent
			string masterFieldList = "";
			foreach (var field in parentTableFields)
				masterFieldList += ",[" + field.ToUpperInvariant() + "]";
			masterFieldList = masterFieldList.Trim(new char[] { ',', ' ' });
			// - Child
			string childFieldList = "";
			foreach (var field in childTableFields)
				childFieldList += ",[" + field.ToUpperInvariant() + "]";
			childFieldList = childFieldList.Trim(new char[] { ',', ' ' });

			//Now generate our string.
			string createFK = string.Format(SQL_CREATE_FK,
				keyName,
				parentTableName,
				masterFieldList,
				childTableName,
				childFieldList,
				onClause);

			ExecuteCommand(connection, createFK);
		}

		/// <summary>
		/// Creates a new index.
		/// </summary>
		/// <param name="connection">The DB connection.</param>
		/// <param name="indexName">The name of the gicen index.</param>
		/// <param name="tableName">The name of the table to add indexes to.</param>
		/// <param name="columns">The name of the column to add. Always ASCENDING.</param>
		public static void AddIndex(DBConnection connection, string indexName, string tableName, string column)
		{
			AddIndex(connection, indexName, tableName, new List<ColumnDirection> { new ColumnDirection { ColumnName = column } });
		}

		/// <summary>
		/// Creates the named index on the given table.
		/// </summary>
		/// <param name="indexName">The name of the gicen index.</param>
		/// <param name="tableName">The name of the table to add indexes to.</param>
		/// <param name="columns">Columns to add to the index.</param>
		/// <param name="connection">The DB connection.</param>
		public static void AddIndex(DBConnection connection, string indexName, string tableName, List<ColumnDirection> columns)
		{
			//Generate the column string.
			string cols = "";
			foreach (var col in columns)
				cols += ", [" + col.ColumnName + "] " + (col.Descending ? "DESC" : "ASC");
			cols = cols.Trim(new char[] { ' ', ',' });

			//Assemble the SQL.
			string sqlIdx = string.Format(SQL_CREATE_IDX,
				indexName,
				tableName,
				cols);

			//Execute the command.
			ExecuteCommand(connection, sqlIdx);
		}

		/// <summary>
		/// Drops the specified Index.
		/// </summary>
		/// <param name="connection">The DB connection.</param>
		/// <param name="indexName">The name of the gicen index.</param>
		/// <param name="tableName">The name of the table to add indexes to.</param>
		public static void DropIndex(DBConnection connection, string indexName, string tableName)
		{
			string sqlIdx = string.Format(SQL_DROP_IDX,
				tableName,
				indexName);
			SQLUtilities.ExecuteCommand(connection, sqlIdx);
		}

		/// <summary>Removes a column from a database table</summary>
		public static void DropColumn(DBConnection connection, string table, string field)
		{
			string alterSql = @"
ALTER TABLE [" + table + @"]
	DROP COLUMN [" + field + @"]
";
			ExecuteSqlCommands(connection, alterSql);
		}

		/// <summary>Generates the connection string to talk to the database server.</summary>
		static public string GenerateConnectionString(DBConnection conninfo)
		{
			return GenerateConnectionString(
				conninfo.DatabaseName,
				conninfo.DatabaseServer,
				conninfo.LoginAuthType,
				conninfo.LoginUser,
				conninfo.LoginPassword);
		}

		/// <summary>Generates the connection string to talk to the database server.</summary>
		static public string GenerateConnectionString(
			string databaseName,
			string databaseServer,
			AuthenticationMode sqlMode,
			string sqlLogin = null,
			string sqlPassword = null,
			int? timeout = null)
		{
			//The server. Always present!
			string retStr = "Data Source=" + databaseServer;

			//Specify a starting database if given.
			if (!string.IsNullOrWhiteSpace(databaseName))
				retStr += ";" + "Initial Catalog=" + databaseName;

			//Specify the log in type, and username / password.
			if (sqlMode == AuthenticationMode.Windows)
				retStr += ";" + "Trusted_Connection=true;";
			else
				retStr += ";" + "Password=" + sqlPassword +
						";" + "User ID=" + sqlLogin;

			if (timeout.HasValue)
				retStr += ";Connection Timeout=" + timeout.ToString();
			else
				retStr += ";Connection Timeout=" + COMMAND_TIMEOUT.ToString();


			//Testing adding Multiple Result Sets.
			//retStr += ";MultipleActiveResultSets=true";

			return retStr;
		}

		/// <summary>Pulls needed data fro mthe database.</summary>
		/// <param name="connection">The connection information.</param>
		/// <param name="select">The select query to return.</param>
		/// <returns></returns>
		static public DataSet GetDataQuery(DBConnection connection, string select)
		{
			DataSet retValue = null;

			try
			{
				//First we need to establish a connection with the database
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(connection)))
				{
					//Now we need to delete any existing license information
					using (SqlCommand command = makeCommand(con))
					{
						//The string for our command to run.
						command.Connection.Open();
						command.CommandType = CommandType.Text;
						command.CommandText = select;

						using (SqlDataAdapter reader = new SqlDataAdapter())
						{
							retValue = new DataSet();
							reader.SelectCommand = command;
							reader.Fill(retValue);
						}

						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("Could not run query:" + Environment.NewLine + select, ex);
			}

			return retValue;
		}

		/// <summary>Retrieves the backup directory from the SQL Server.</summary>
		/// <returns></returns>
		public static string GetSQLBackupDir(
			string dbName,
			string dbServer,
			AuthenticationMode dbAuthType,
			string dbUser,
			string dbPassword)
		{
			//The return value.
			string retVal = "";

			try
			{
				//First we need to establish a connection with the database
				using (SqlConnection connection = new SqlConnection(GenerateConnectionString(dbName, dbServer, dbAuthType, dbUser, dbPassword)))
				{
					//Now we need to delete any existing license information
					using (SqlCommand command = makeCommand(connection))
					{

						/* Lets try to retrieve the folder path.. */
						string dbCommand = "DECLARE @path NVARCHAR(4000); " +
							"EXEC master.dbo.xp_instance_regread " +
							"N'HKEY_LOCAL_MACHINE', " +
							"N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory', " +
							"@path OUTPUT; " +
							"SELECT(@path) AS Directory;";

						command.Connection.Open();
						command.CommandType = CommandType.Text;
						command.CommandText = dbCommand;
						var dataRow = command.ExecuteReader(CommandBehavior.SingleRow);
						if (dataRow.Read())
							retVal = dataRow.GetValue(0).ToString();

						command.Connection.Close();
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("Trying to retrieve backup database directory", ex);
			}

			//Return..
			return retVal;
		}

		/// <summary>Retrieves the current database revision from the Spira database.</summary>
		/// <param name="conn">SQl connection information</param>
		/// <returns>An integer of the DB revision. 0 if no DB found.</returns>
		public static int GetExistingDBRevision(DBConnection conn)
		{
			//The return value.
			int retVal = 0;

			try
			{
				//First we need to establish a connection with the database
				using (SqlConnection connection = new SqlConnection(GenerateConnectionString(conn)))
				{
					//Now we need to delete any existing license information
					using (SqlCommand command = makeCommand(connection))
					{
						//The string for our command to run.
						string dbCommand = "";

						/* Lets try to retrieve the folder path.. */
						dbCommand = @"SELECT TOP 1 [VALUE] FROM [TST_GLOBAL_SETTING] WHERE [NAME] = 'Database_Revision';";

						command.Connection.Open();
						command.CommandType = CommandType.Text;
						command.CommandText = dbCommand;
						var dataRow = command.ExecuteReader(CommandBehavior.SingleRow);
						if (dataRow.Read())
						{
							string dbRev = dataRow.GetValue(0).ToString();
							int.TryParse(dbRev, out retVal);
						}
						command.Connection.Close();
					}

					//If it reports as v5.0, make sure tht it is flavor 540.
					if (retVal == 500)
					{
						//Now we need to delete any existing license information
						int rowCount = 0;
						using (SqlConnection con = new SqlConnection(GenerateConnectionString(conn)))
						{
							con.Open();

							using (SqlCommand command = new SqlCommand())
							{
								//The string for our command to run.
								string dbCommand = "";

								//See if we have a user collection that was added in 5.4
								dbCommand = @"SELECT COUNT(*) FROM [TST_USER_COLLECTION] WHERE [NAME] = 'GroupReleases.Columns';";

								command.Connection = con;
								command.CommandType = CommandType.Text;
								command.CommandText = dbCommand;
								command.CommandTimeout = 1200;
								var dataRow = command.ExecuteReader(CommandBehavior.SingleRow);
								if (dataRow.Read())
									rowCount = (int)dataRow.GetValue(0);

								if (rowCount > 0)
									retVal = 540;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog("Trying to retrieve existing database version", ex);
			}

			//Return..
			return retVal;
		}

		/// <summary>Delete Existing Foreign Key Constraints</summary>
		public static void DeleteExistingForeignKeyConstraints(DBConnection _connection)
		{
			//Run a command to remove all the foreign keys.
			string commandtxt = @"
while(exists(select 1 from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_TYPE='FOREIGN KEY'))
begin
	declare @sql nvarchar(2000)
	SELECT TOP 1 @sql=('ALTER TABLE ' + TABLE_SCHEMA + '.[' + TABLE_NAME
	+ '] DROP CONSTRAINT [' + CONSTRAINT_NAME + ']')
	FROM information_schema.table_constraints
	WHERE CONSTRAINT_TYPE = 'FOREIGN KEY'
	exec (@sql)
end";
			ExecuteCommand(_connection, commandtxt);
		}

		/// <summary>Gets the version of the SQL Server running on the client's machine.</summary>
		public static string GetSQLServerVer(DBConnection conn)
		{
			string retVal = "-Unknown-";

			try
			{
				using (SqlConnection con = new SqlConnection(GenerateConnectionString(conn)))
				{
					con.Open();
					retVal = con.ServerVersion;
					con.Close();
				}
			}
			catch { }

			return retVal;
		}

		/// <summary>Drop Programmable Objects</summary>
		public static void DropProgrammableObjects(DBConnection connection, StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Database Upgrade: DropProgrammableObjects - Starting");

			//Drop all programmable objects.
			//Get the SQL File. 
			string sqlQuery1 = ZipReader.GetContents("base.ClrDropObjs.zip");
			ExecuteSqlCommands(connection, sqlQuery1);

			streamWriter.WriteLine("Database Upgrade: DropProgrammableObjects - Finished");
		}

		/// <summary>Creates all the Programmable objects in the database.</summary>
		public static void CreateProgrammableObjects(DBConnection connection, StreamWriter streamWriter)
		{
			streamWriter.WriteLine("Database: CreateProgrammableObjects - Starting");

			// 1. Procedures.
			List<string> files = ZipReader.GetFilesIn("base.DB_Procs.zip");
			foreach (string file in files.OrderBy(s => s))
			{
				try
				{
					string sqlQuery = ZipReader.GetContents("base.DB_Procs.zip", file);
					ExecuteSqlCommands(connection, sqlQuery);
				}
				catch (Exception ex)
				{
					string msg = "Error creating Stored Procedure: " + file + Environment.NewLine + Logger.DecodeException(ex);
					streamWriter.WriteLine(msg);
				}
			}

			// 2. Functions
			files = ZipReader.GetFilesIn("base.DB_Funcs.zip");
			foreach (string file in files.OrderBy(s => s))
			{
				try
				{

					string sqlQuery = ZipReader.GetContents("base.DB_Funcs.zip", file);
					ExecuteSqlCommands(connection, sqlQuery);
				}
				catch (Exception ex)
				{
					string msg = "Error creating Function: " + file + Environment.NewLine + Logger.DecodeException(ex);
					streamWriter.WriteLine(msg);
				}
			}

			// 3. Views
			files = ZipReader.GetFilesIn("base.DB_Views.zip");
			foreach (string file in files.OrderBy(s => s))
			{
				try
				{
					string sqlQuery = ZipReader.GetContents("base.DB_Views.zip", file);
					ExecuteSqlCommands(connection, sqlQuery);
				}
				catch (Exception ex)
				{
					string msg = "Error creating View: " + file + Environment.NewLine + Logger.DecodeException(ex);
					streamWriter.WriteLine(msg);
				}
			}


			streamWriter.WriteLine("Database: CreateProgrammableObjects - Finished");
		}

		#region Private Methods
		/// <summary>Write out the message to the log</summary>
		/// <param name="message">THe string to write out. Will be appended by a NewLine.</param>
		private static void WriteLog(string message)
		{
			//See if we have the object set.
			if (logWriter != null)
				logWriter.WriteLine(message);
		}

		/// <summary>Writes the given message and exception out to our output.</summary>
		/// <param name="message">The message to write.</param>
		/// <param name="ex">Exception to write.</param>
		private static void WriteLog(string message, Exception ex)
		{
			string msg = message +
				(message.EndsWith(":") ? "" : ":") +
				Environment.NewLine +
				Logger.DecodeException(ex);
			WriteLog(msg);
		}

		/// <summary>Returns a configured command.</summary>
		/// <param name="conn">The SQLConnection to use.</param>
		private static SqlCommand makeCommand(SqlConnection conn)
		{
			SqlCommand retCmd = new SqlCommand();
			retCmd.Connection = conn;
			retCmd.CommandTimeout = COMMAND_TIMEOUT;

			return retCmd;
		}
		#endregion

		#region Enumerations
		/// <summary>What to do on a Foreign Key 'On Delete' and 'On Update'.</summary>
		public enum ForeignKeyAction : int
		{
			/// <summary>Do nothing.</summary>
			None = 0,
			/// <summary>Cascade the Delete/Update</summary>
			Cascade = 1,
			/// <summary>Set the value to Null.</summary>
			Set_Null = 2,
			/// <summary>Set the value to default.</summary>
			Set_Default = 3
		}
		#endregion Enumerations

		#region Internal Classes
		public class ColumnDirection
		{
			/// <summary>The name of the column.</summary>
			public string ColumnName { get; set; }
			/// <summary>Specifies DESCENDIGN sorting direction.</summary>
			public bool Descending { get; set; }
		}
		#endregion Internal Classes
	}
}
