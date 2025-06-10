using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;

using EFTracingProvider;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

//Ignore warnings about missing XML comments
#pragma warning disable 1572, 1573, 1570

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Graphs in the system
	/// </summary
	public class GraphManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.GraphManager::";

		const int MAX_NUMBER_ROWS_GRAPH = 1000;

		#region Custom Graphs

		/// <summary>
		/// Executes a Dynamic Entity SQL specified in a custom graph, substituting the project id, release id and project group id if necessary
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="projectGroupId">The id of the current project group</param>
		/// <param name="graphId">The id of the custom graph to execute the query for</param>
		/// <param name="releaseId">The id of the release (optional, null = all releases)</param>
		/// <returns>A datatable containing the data</returns>
		public DataTable GraphCustom_ExecuteSQL(int projectId, int projectGroupId, int graphId, int? releaseId = null)
		{
			const string METHOD_NAME = "GraphCustom_ExecuteSQL";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//first retrieve the graph, make sure it's active
				GraphCustom graph = this.GraphCustom_RetrieveById(graphId);
				if (graph == null || !graph.IsActive)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.GraphManager_GraphNotExist, graphId));
				}
				var spquery = graph.Query.Split(new string[] { "SpiraTestEntities.R_" }, StringSplitOptions.None);
				var spquery1 = spquery[1].Split(' ');

				return this.GraphCustom_ExecuteSQL(projectId, projectGroupId, graph.Query, releaseId, spquery1[0]);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Executes a Dynamic Entity SQL command, substituting the project id and project group id if necessary
		/// </summary>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="projectGroupId">The id of the current project group</param>
		/// <param name="sql">The Entity SQL to execute</param>
		/// <returns>A datatable containing the data</returns>
		public DataTable GraphCustom_ExecuteSQL(int projectId, int projectGroupId, string sql, int? releaseId = null,string Type=null)
		{
			const string METHOD_NAME = "GraphCustom_ExecuteSQL";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			string esqlQuery = sql;
			try
			{
				//Replace the project id, release id and project group id
				if (projectId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectId}", projectId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (projectGroupId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectGroupId}", projectGroupId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (releaseId.HasValue && releaseId.Value > 0)
				{
					//We populate two tokens ${ReleaseId} for the release and ${ReleaseAndChildIds} to get a list of releases and sprints
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);

					esqlQuery = esqlQuery.Replace("${ReleaseId}", releaseId.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
					if (!String.IsNullOrEmpty(releaseList))
					{
						esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
					}
				}
				else
				{
					//Populate as -1 to avoid a syntax error, that way it just doesn't match anything
					esqlQuery = esqlQuery.Replace("${ReleaseId}", "-1", StringComparison.InvariantCultureIgnoreCase);

					//For this one, we can return all the releases in the project
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = String.Join(",", releaseManager.RetrieveByProjectId(projectId, true, true).Select(r => r.ReleaseId));
					esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
				}

				//Create the new data table
				DataTable dataTable = new DataTable();

				//Setup the connection
				using (EntityConnection conn = new EntityConnection("name=SpiraTestEntities"))
				{
					//Open the connection
					conn.Open();

					//Execute the command
					using (EntityCommand cmd = new EntityCommand(esqlQuery, conn))
					{
						//Set a larger timeout value (in seconds)
						cmd.CommandTimeout = Common.Properties.Settings.Default.ReportingESQLTimeout;

						using (EntityDataReader dataReader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							// Iterate through the collection of records (limited to the max size)
							int count = 0;
							while (dataReader.Read() && count < MAX_NUMBER_ROWS_GRAPH)
							{
								//Read the name/values
								Dictionary<string, object> rowData = new Dictionary<string, object>();
								Dictionary<string, object> rowDataNew = new Dictionary<string, object>();


								// Iterate over the fields of the data reader

								for (int i = 0; i < dataReader.FieldCount; i++)
								{
									string f1=dataReader.GetName(i);
									string fieldName = dataReader.GetName(i);
									object fieldValue = dataReader.IsDBNull(i) ? DBNull.Value : dataReader[i]; // Handle DBNull values
									rowData.Add(fieldName, fieldValue);

								}
								var c1 = "";
								object c2 = "";
								foreach (KeyValuePair<string, object> kvp in rowData)
								{

									if (Type == "Risk Statuses" || Type == "RiskStatuses")
									{
										if (kvp.Key == "RISK_STATUS_ID" || kvp.Key == "PROJECT_TEMPLATE_ID")
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
										}
									}
									else if (Type == "Risk Types" || Type == "RiskTypes")
									{
										if (kvp.Key == "RISK_TYPE_ID" || kvp.Key == "PROJECT_TEMPLATE_ID")
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
										}
									}
									else if (Type == "Events")
									{
										if (kvp.Key == "EVENT_TYPE_ID")
										{
											//rowDataNew.Add(kvp.Key, kvp.Value);
											{
												c1 = kvp.Key;
												c2 = kvp.Value;
												continue;
											}
										}

										else if (kvp.Key == "EVENT_TIME")
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
											if (c1 != "" && c2 != "")
											{
												rowDataNew.Add(c1, c2);
												c1 = "";
												c2 = "";
											}
										}
									}
									else if (Type == "History Change Sets" || Type == "HistoryChangeSets")
									{
										if (kvp.Key == "ARTIFACT_TYPE_ID" || kvp.Key == "CHANGESET_ID")
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
										}
									}

									
									else if(Type == "HistoryDetails")
									{
										if((kvp.Key == "ARTIFACT_HISTORY_ID" || kvp.Key == "CHANGESET_ID"))
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
										}
									}
									else if (Type == "Test Set Incidents"|| Type == "TestSetIncidents")
									{
										

										if (kvp.Key == "TEST_SET_ID")
										{
											//rowDataNew.Add(kvp.Key, kvp.Value);
											{
												c1 = kvp.Key;
												c2 = kvp.Value;
												continue;
											}
										}

										else if (kvp.Key == "INCIDENT_ID")
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
											if (c1 != "" && c2 != "")
											{
												rowDataNew.Add(c1, c2);
												c1 = "";
												c2 = "";
											}
										}
									}
									else
									{
										if ((Type == "Artifact Types" || Type == "ArtifactTypes") && kvp.Key == "ARTIFACT_TYPE_ID"
											|| (kvp.Key == "ARTIFACT_TYPE_ID" && Type == "Comments") 
											
											|| (kvp.Key == "PROJECT_GROUP_ID" && (Type == "Project Groups" || Type == "ProjectGroups"))
											|| (kvp.Key == "PROJECT_ID" && (Type == "Project Membership" || Type == "ProjectMembership"))
											|| (kvp.Key == "PROJECT_ID" && (Type == "Project Release Resources" || Type == "ProjectReleaseResources"))
											|| kvp.Key == "CHANGESET_ID" || kvp.Key == "PORTFOLIO_ID"
											|| ((Type == "Project Roles" || Type == "ProjectRoles") && kvp.Key == "PROJECT_ROLE_ID")
											|| (kvp.Key == "PROJECT_TEMPLATE_ID" && (Type == "Project Templates" || Type == "ProjectTemplates"))
											|| (kvp.Key == "RISK_STATUS_ID" && (Type == "Risk Statuses" || Type == "RiskStatuses"))
											|| (kvp.Key == "RISK_TYPE_ID" && (Type == "Risk Types" || Type == "RiskTypes"))
											|| (kvp.Key == "PARENT_TEST_CASE_FOLDER_ID" && (Type == "Test Case Folders" || Type == "TestCaseFolders"))
											|| (kvp.Key == "USER_ID" && Type == "Users"))

										{
											c1 = kvp.Key;
											c2 = kvp.Value;
											continue;
										}

										else
										{
											rowDataNew.Add(kvp.Key, kvp.Value);
											if (c1 != "" && c2 != "")
											{
												rowDataNew.Add(c1, c2);
												c1 = "";
												c2 = "";
											}

										}
									}
								}

								// If this is the first record, create the columns
								if (count == 0)
								{
									bool firstColumn = true;

									foreach (KeyValuePair<string, object> kvp in rowDataNew)
									{
										
											DataColumn dataColumn = new DataColumn
											{
												Caption = kvp.Key,
												ColumnName = kvp.Key,
												DataType = kvp.Value == DBNull.Value ? typeof(string) : kvp.Value.GetType(), // Set the data type to string if DBNull is present
												AllowDBNull = true // Allow null values in columns (you may want to change this based on requirements)
											};

											// Add the column to the DataTable
											dataTable.Columns.Add(dataColumn);
										

										// (Optional) If you want to enforce numeric types, uncomment the following block:
										//if (!firstColumn && !(kvp.Value is Int32 || kvp.Value is Int64 || kvp.Value is Decimal || kvp.Value is Double || kvp.Value is float || kvp.Value is Int16))
										//{
										//    throw new GraphDataInvalidException(String.Format(GlobalResources.Messages.GraphManager_InvalidColumn, kvp.Key));
										//}
										firstColumn = false;
									}
								}

								// Create a new row and populate it with data from the rowData dictionary
								DataRow dataRow = dataTable.NewRow();
								foreach (KeyValuePair<string, object> kvp in rowDataNew)
								{
									// Check if the column allows DBNull
									if (dataTable.Columns[kvp.Key].AllowDBNull)
									{
										dataRow[kvp.Key] = kvp.Value == DBNull.Value ? DBNull.Value : kvp.Value; // Assign DBNull if the value is DBNull
									}
									else
									{
										dataRow[kvp.Key] = kvp.Value ?? DBNull.Value; // Use DBNull if value is null, but column disallows nulls
									}
								}

								// Add the populated row to the DataTable
								dataTable.Rows.Add(dataRow);



								//Add all the columns to the row
								foreach (KeyValuePair<string, object> kvp in rowDataNew)
								{
									//Populate the column for this row, handling nulls correctly
									//since DataTables use DBNull not .NET null
									if (kvp.Value == null)
									{
										dataRow[kvp.Key] = DBNull.Value;
									}
									else
									{
										dataRow[kvp.Key] = kvp.Value;
									}
								}

								count++;
							}
						}
					}

					//Close the connection
					conn.Close();
				}

				return dataTable;
			}
			catch (EntitySqlException exception)
			{
				//Also log the SQL if we have this exception.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, esqlQuery);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		public DataTable GraphCustom_ExecuteSQLCustom(int projectId, int projectGroupId, string sql, int? releaseId = null, string Type = null)
		{
			const string METHOD_NAME = "GraphCustom_ExecuteSQL";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			string esqlQuery = sql;
			try
			{
				//Replace the project id, release id and project group id
				if (projectId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectId}", projectId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (projectGroupId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectGroupId}", projectGroupId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (releaseId.HasValue && releaseId.Value > 0)
				{
					//We populate two tokens ${ReleaseId} for the release and ${ReleaseAndChildIds} to get a list of releases and sprints
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);

					esqlQuery = esqlQuery.Replace("${ReleaseId}", releaseId.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
					if (!String.IsNullOrEmpty(releaseList))
					{
						esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
					}
				}
				else
				{
					//Populate as -1 to avoid a syntax error, that way it just doesn't match anything
					esqlQuery = esqlQuery.Replace("${ReleaseId}", "-1", StringComparison.InvariantCultureIgnoreCase);

					//For this one, we can return all the releases in the project
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = String.Join(",", releaseManager.RetrieveByProjectId(projectId, true, true).Select(r => r.ReleaseId));
					esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
				}

				//Create the new data table
				DataTable dataTable = new DataTable();

				//Setup the connection
				using (EntityConnection conn = new EntityConnection("name=SpiraTestEntities"))
				{
					//Open the connection
					conn.Open();

					//Execute the command
					using (EntityCommand cmd = new EntityCommand(esqlQuery, conn))
					{
						//Set a larger timeout value (in seconds)
						cmd.CommandTimeout = Common.Properties.Settings.Default.ReportingESQLTimeout;

						using (EntityDataReader dataReader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							// Iterate through the collection of records (limited to the max size)
							int count = 0;
							while (dataReader.Read() && count < MAX_NUMBER_ROWS_GRAPH)
							{
								//Read the name/values
								Dictionary<string, object> rowData = new Dictionary<string, object>();


								// Iterate over the fields of the data reader

								for (int i = 0; i < dataReader.FieldCount; i++)
								{
									string f1 = dataReader.GetName(i);
									string fieldName = dataReader.GetName(i);
									object fieldValue = dataReader.IsDBNull(i) ? DBNull.Value : dataReader[i]; // Handle DBNull values
									rowData.Add(fieldName, fieldValue);

								}
								

								// If this is the first record, create the columns
								if (count == 0)
								{
									bool firstColumn = true;

									foreach (KeyValuePair<string, object> kvp in rowData)
									{

										DataColumn dataColumn = new DataColumn
										{
											Caption = kvp.Key,
											ColumnName = kvp.Key,
											DataType = kvp.Value == DBNull.Value ? typeof(string) : kvp.Value.GetType(), // Set the data type to string if DBNull is present
											AllowDBNull = true // Allow null values in columns (you may want to change this based on requirements)
										};

										// Add the column to the DataTable
										dataTable.Columns.Add(dataColumn);


										// (Optional) If you want to enforce numeric types, uncomment the following block:
										//if (!firstColumn && !(kvp.Value is Int32 || kvp.Value is Int64 || kvp.Value is Decimal || kvp.Value is Double || kvp.Value is float || kvp.Value is Int16))
										//{
										//    throw new GraphDataInvalidException(String.Format(GlobalResources.Messages.GraphManager_InvalidColumn, kvp.Key));
										//}
										firstColumn = false;
									}
								}

								// Create a new row and populate it with data from the rowData dictionary
								DataRow dataRow = dataTable.NewRow();
								foreach (KeyValuePair<string, object> kvp in rowData)
								{
									// Check if the column allows DBNull
									if (dataTable.Columns[kvp.Key].AllowDBNull)
									{
										dataRow[kvp.Key] = kvp.Value == DBNull.Value ? DBNull.Value : kvp.Value; // Assign DBNull if the value is DBNull
									}
									else
									{
										dataRow[kvp.Key] = kvp.Value ?? DBNull.Value; // Use DBNull if value is null, but column disallows nulls
									}
								}

								// Add the populated row to the DataTable
								dataTable.Rows.Add(dataRow);



								//Add all the columns to the row
								foreach (KeyValuePair<string, object> kvp in rowData)
								{
									//Populate the column for this row, handling nulls correctly
									//since DataTables use DBNull not .NET null
									if (kvp.Value == null)
									{
										dataRow[kvp.Key] = DBNull.Value;
									}
									else
									{
										dataRow[kvp.Key] = kvp.Value;
									}
								}

								count++;
							}
						}
					}

					//Close the connection
					conn.Close();
				}

				return dataTable;
			}
			catch (EntitySqlException exception)
			{
				//Also log the SQL if we have this exception.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, esqlQuery);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		/// <summary>
		/// Saves the changes to the custom graph
		/// </summary>
		/// <param name="graph">The updated graph object</param>
		public void GraphCustom_Update(GraphCustom graph, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "GraphCustom_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (graph != null)
				{
					//Attach to the context and apply/save the changes
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.GraphCustoms.ApplyChanges(graph);
						context.AdminSaveChanges(userId, graph.GraphId, null, adminSectionId, action, true, true, null);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new custom graph
		/// </summary>
		/// <param name="name">The name of the graph</param>
		/// <param name="esqlQuery">The ESQL query being used</param>
		/// <param name="isActive">Is the graph active</param>
		/// <param name="description">The description of the graph</param>
		/// <param name="position">The position in the graph list</param>
		/// <returns>The id of the new graph</returns>
		public int GraphCustom_Create(string name, string esqlQuery, bool isActive = true, string description = "", int? position = null, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "GraphCustom_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			AdminAuditManager adminAuditManager = new AdminAuditManager();
			try
			{
				int graphId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we have been given a position
					if (!position.HasValue || position < 0)
					{
						//Get the next available position
						var query = from g in context.GraphCustoms
									orderby g.Position descending
									select g.Position;

						int? lastPosition = query.FirstOrDefault();
						if (lastPosition.HasValue)
						{
							//Increment the last used position
							position = lastPosition + 1;
						}
						else
						{
							//No existing graphs, so choose the first position
							position = 1;
						}
					}

					//Add the new custom graph
					GraphCustom graph = new GraphCustom();
					graph.GraphTypeId = (int)Graph.GraphTypeEnum.CustomGraphs;
					graph.Name = name;
					graph.Description = description;
					graph.IsActive = isActive;
					graph.Position = position.Value;
					graph.Query = esqlQuery;
					context.GraphCustoms.AddObject(graph);
					context.SaveChanges();

					graphId = graph.GraphId;

					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = graph.Name;

					if (userId != null)
					{
						//Log history.
						adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), graph.GraphTypeId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Graph, "GraphId");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return graphId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Clones the current graph
		/// </summary>
		/// <param name="graphId">The id of the graph to clone</param>
		/// <returns>The id of the new cloned graph</returns>
		public int GraphCustom_Clone(int graphId, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "GraphCustom_Clone";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the graph
				GraphCustom graph = this.GraphCustom_RetrieveById(graphId);
				if (graph == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.GraphManager_GraphNotExist, graphId));
				}

				//Create the clone using the name with a suffix
				int newGraphId = this.GraphCustom_Create(graph.Name + " - " + GlobalResources.General.Global_Copy, graph.Query, graph.IsActive, graph.Description, null, userId, adminSectionId, action);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newGraphId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a custom graph
		/// </summary>
		/// <param name="graphId">The id to delete</param>
		public void GraphCustom_Delete(int graphId, int? userId = null)
		{
			const string METHOD_NAME = "GraphCustom_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from g in context.GraphCustoms
								where g.GraphId == graphId
								select g;

					GraphCustom graph = query.FirstOrDefault();
					if (graph != null)
					{
						context.GraphCustoms.DeleteObject(graph);
						context.SaveChanges();

						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "Edit Graphs";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)userId, graph.GraphId, graph.Name, adminSectionId, "Graph Deleted", DateTime.UtcNow, ArtifactTypeEnum.Graph, "GraphId");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the custom graphs in the system
		/// </summary>
		/// <param name="activeOnly">Do we want active only?</param>
		/// <returns>The list of graphs</returns>
		public List<GraphCustom> GraphCustom_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "GraphCustom_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<GraphCustom> graphs;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from g in context.GraphCustoms
								where (g.IsActive || !activeOnly)
								orderby g.Position, g.GraphId
								select g;

					graphs = query.ToList();
				}

				//return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return graphs;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single custom graph in the system
		/// </summary>
		/// <param name="graphId">The id of the graph</param>
		/// <returns>The graph entity</returns>
		public GraphCustom GraphCustom_RetrieveById(int graphId)
		{
			const string METHOD_NAME = "GraphCustom_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				GraphCustom graph;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from g in context.GraphCustoms
								where g.GraphId == graphId
								select g;

					graph = query.FirstOrDefault();
				}

				//return the item
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return graph;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Graph Entity Retrieves

		/// <summary>
		/// Retrieves the list of graphs that are a certain type
		/// </summary>
		/// <param name="graphType">The type of graph we're interested in</param>
		/// <param name="artifactType">Optional filter on artifact type</param>
		public List<Graph> RetrieveByType(Graph.GraphTypeEnum graphType, Nullable<DataModel.Artifact.ArtifactTypeEnum> artifactType = null)
		{
			const string METHOD_NAME = "RetrieveByType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Graph> graphs;

				int graphTypeId = (int)graphType;
				int? artifactTypeId = null;
				if (artifactType.HasValue)
				{
					artifactTypeId = (int)artifactType.Value;
				}
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from g in context.Graphs
								.Include(g => g.Type)
								where g.IsActive && g.GraphTypeId == graphTypeId
								select g;

					//Add on the artifact filter
					if (artifactTypeId.HasValue)
					{
						query = query.Where(g => g.ArtifactTypeId == artifactTypeId.Value);
					}

					//Sort
					query = query.OrderBy(g => g.Position).ThenBy(g => g.GraphId);

					//Now execute the query
					graphs = query.ToList();

					//Finally we need to localize the various names and descriptions
					foreach (Graph graph in graphs)
					{
						if (!String.IsNullOrEmpty(GlobalResources.General.ResourceManager.GetString(graph.Name)))
						{
							graph.Name = GlobalResources.General.ResourceManager.GetString(graph.Name);
						}
						if (!String.IsNullOrEmpty(graph.Description) && !String.IsNullOrEmpty(GlobalResources.General.ResourceManager.GetString(graph.Description)))
						{
							graph.Description = GlobalResources.General.ResourceManager.GetString(graph.Description);
						}
					}
				}

				//return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return graphs;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single graph by its ID
		/// </summary>
		/// <returns>The graph entity</returns>
		/// <param name="graphId">The id of graph we're interested in</param>
		public Graph RetrieveById(int graphId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Graph graph;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from g in context.Graphs
								.Include(g => g.Type)
								where g.IsActive && g.GraphId == graphId
								select g;

					//Now execute the query
					graph = query.FirstOrDefault();
				}

				//Make sure at least one row is returned
				if (graph == null)
				{
					throw new ArtifactNotExistsException(String.Format("Unable to retrieve graph with ID={0}", graphId));
				}

				//Finally we need to localize the various names and descriptions
				if (!String.IsNullOrEmpty(GlobalResources.General.ResourceManager.GetString(graph.Name)))
				{
					graph.Name = GlobalResources.General.ResourceManager.GetString(graph.Name);
				}
				if (!String.IsNullOrEmpty(graph.Description) && !String.IsNullOrEmpty(GlobalResources.General.ResourceManager.GetString(graph.Description)))
				{
					graph.Description = GlobalResources.General.ResourceManager.GetString(graph.Description);
				}

				//return the entity
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return graph;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Graph Data Retrieval

		/// <summary>
		/// Retrieves the list of artifact fields and custom properties that can be reported on (x-axis or grouping)
		/// </summary>
		/// <param name="artifactType">The artifact type</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The dataset of fields</returns>
		public System.Data.DataSet RetrieveSummaryChartFields(int projectId, int projectTemplateId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "RetrieveSummaryChartFields";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DataSet artifactFields;

				//Create select command for retrieving the list of fields to display for an artifact type
				//in the context of a specific user and project
				//Note: We need to specifically include ExecutionStatusId on Test Cases despite it being an 'equalizer' field.
				//In the future we could consider adding a CHARTABLE_YN field to the ARTIFACT_FIELD table instead if more cases arise
				string specialCases = "";
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase)
				{
					specialCases = "OR ARF.NAME = 'ExecutionStatusId'";
				}

				string sql =
					"SELECT	ARF.NAME AS Name, ARF.CAPTION AS Caption, ARTIFACT_FIELD_TYPE_ID AS ArtifactFieldTypeId, ARF.LIST_DEFAULT_POSITION AS ListPosition " +
					"FROM	TST_ARTIFACT_FIELD ARF " +
					"WHERE 	ARF.IS_ACTIVE = 1 " +
					"AND    (ARF.ARTIFACT_FIELD_TYPE_ID IN (" + (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup + "," + (int)DataModel.Artifact.ArtifactFieldTypeEnum.MultiList + "," + (int)DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup + ") " + specialCases + ")" +
					"AND	ARF.ARTIFACT_TYPE_ID = " + (int)artifactType + " " +
					"UNION " +
					"SELECT	'" + CustomProperty.FIELD_PREPEND + "' + RIGHT('0'+ RTRIM(CAST (CPR.PROPERTY_NUMBER AS NVARCHAR)), 2) AS Name, CPR.NAME AS Caption, " + ((int)DataModel.Artifact.ArtifactFieldTypeEnum.CustomPropertyLookup).ToString() + " AS ArtifactFieldTypeId, " +
					"       (CPR.PROPERTY_NUMBER + 50) AS ListPosition " +
					"FROM	TST_CUSTOM_PROPERTY CPR " +
					"WHERE 	CPR.IS_DELETED = 0 " +
					"AND    CPR.CUSTOM_PROPERTY_TYPE_ID IN (" + (int)CustomProperty.CustomPropertyTypeEnum.List + "," + (int)CustomProperty.CustomPropertyTypeEnum.MultiList + "," + (int)CustomProperty.CustomPropertyTypeEnum.User + ") " +
					"AND	CPR.PROJECT_TEMPLATE_ID = " + projectTemplateId + " " +
					"AND	CPR.ARTIFACT_TYPE_ID = " + (int)artifactType + " " +
					"ORDER BY Caption";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactFields = ExecuteSql(context, sql, "ArtifactFields");
				}

				//For test cases, we need to remove the TestCaseId since it's not part of the main table
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase)
				{
					for (int i = 0; i < artifactFields.Tables["ArtifactFields"].Rows.Count; i++)
					{
						DataRow dataRow = artifactFields.Tables["ArtifactFields"].Rows[i];
						if ((string)dataRow["Name"] == "ReleaseId")
						{
							dataRow.Delete();
						}
					}
					artifactFields.Tables["ArtifactFields"].AcceptChanges();
				}

				//For test sets, we need to remove the TestCaseReleaseId since it's not part of the main table
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestSet)
				{
					for (int i = 0; i < artifactFields.Tables["ArtifactFields"].Rows.Count; i++)
					{
						DataRow dataRow = artifactFields.Tables["ArtifactFields"].Rows[i];
						if ((string)dataRow["Name"] == "TestCaseReleaseId")
						{
							dataRow.Delete();
						}
					}
					artifactFields.Tables["ArtifactFields"].AcceptChanges();
				}

				//If we have certain product licenses installed, then we need to disable certain fields
				if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.TestCase))
				{
					//Remove requirements and release test coverage columns
					foreach (DataRow dataRow in artifactFields.Tables["ArtifactFields"].Rows)
					{
						if ((string)dataRow["Name"] == "CoverageId")
						{
							dataRow.Delete();
						}
					}
					artifactFields.Tables["ArtifactFields"].AcceptChanges();
				}
				if (!ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.Task))
				{
					//Remove requirements and release task progress columns
					foreach (DataRow dataRow in artifactFields.Tables["ArtifactFields"].Rows)
					{
						if ((string)dataRow["Name"] == "ProgressId")
						{
							dataRow.Delete();
						}
					}
					artifactFields.Tables["ArtifactFields"].AcceptChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactFields;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Sets the known colors for certain data-series in the summary charts
		/// </summary>
		/// <param name="groupByColumn">The datacolumn used to store the color code</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="groupByField">The group by field name</param>
		/// <param name="groupById">The id of the field value</param>
		/// <param name="colorLookupList">The incident priority/severity list</param>
		protected void SetKnownColors(DataColumn groupByColumn, DataModel.Artifact.ArtifactTypeEnum artifactType, string groupByField, int? groupById, object colorLookupList = null)
		{
			//Have to hard-code these currently
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Requirement && groupByField == "ImportanceId" && colorLookupList != null && colorLookupList is List<Importance>)
			{
				if (groupById.HasValue)
				{
					List<Importance> importances = (List<Importance>)colorLookupList;
					Importance importance = importances.FirstOrDefault(i => i.ImportanceId == groupById.Value);
					if (importance != null)
					{
						groupByColumn.ExtendedProperties.Add("Color", importance.Color);
					}
				}
				else
				{
					//(None) is grey
					groupByColumn.ExtendedProperties.Add("Color", "eeeeee");
				}
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase && groupByField == "TestCasePriorityId" && colorLookupList != null && colorLookupList is List<TestCasePriority>)
			{
				if (groupById.HasValue)
				{
					List<TestCasePriority> priorities = (List<TestCasePriority>)colorLookupList;
					TestCasePriority priority = priorities.FirstOrDefault(i => i.TestCasePriorityId == groupById.Value);
					if (priority != null)
					{
						groupByColumn.ExtendedProperties.Add("Color", priority.Color);
					}
				}
				else
				{
					//(None) is grey
					groupByColumn.ExtendedProperties.Add("Color", "eeeeee");
				}
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase && groupByField == "ExecutionStatusId")
			{
				//Need to pass the color of the status since the graph cannot handle CSS classes
				groupByColumn.ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(groupById));
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestRun && groupByField == "ExecutionStatusId")
			{
				//Need to pass the color of the status since the graph cannot handle CSS classes
				groupByColumn.ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(groupById));
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Task && groupByField == "TaskPriorityId" && colorLookupList != null && colorLookupList is List<TaskPriority>)
			{
				if (groupById.HasValue)
				{
					List<TaskPriority> priorties = (List<TaskPriority>)colorLookupList;
					TaskPriority priority = priorties.FirstOrDefault(i => i.TaskPriorityId == groupById.Value);
					if (priority != null)
					{
						groupByColumn.ExtendedProperties.Add("Color", priority.Color);
					}
				}
				else
				{
					//(None) is grey
					groupByColumn.ExtendedProperties.Add("Color", "eeeeee");
				}
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Incident && groupByField == "PriorityId" && colorLookupList != null && colorLookupList is List<IncidentPriority>)
			{
				if (groupById.HasValue)
				{
					List<IncidentPriority> priorities = (List<IncidentPriority>)colorLookupList;
					IncidentPriority priority = priorities.FirstOrDefault(i => i.PriorityId == groupById.Value);
					if (priority != null)
					{
						groupByColumn.ExtendedProperties.Add("Color", priority.Color);
					}
				}
				else
				{
					//(None) is grey
					groupByColumn.ExtendedProperties.Add("Color", "eeeeee");
				}
			}
			if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Incident && groupByField == "SeverityId" && colorLookupList != null && colorLookupList is List<IncidentSeverity>)
			{
				if (groupById.HasValue)
				{
					List<IncidentSeverity> severities = (List<IncidentSeverity>)colorLookupList;
					IncidentSeverity severity = severities.FirstOrDefault(i => i.SeverityId == groupById.Value);
					if (severity != null)
					{
						groupByColumn.ExtendedProperties.Add("Color", severity.Color);
					}
				}
				else
				{
					//(None) is grey
					groupByColumn.ExtendedProperties.Add("Color", "eeeeee");
				}
			}
		}

		#region Requirement Graphs

		/// <summary>Retrieves the data used to populate the requirement burndown graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in</param>
		/// <returns>Dataset of story points against the x-axis</returns>
		/// <remarks>
		/// 1) If no release specified, we display points against releases
		/// 2) If release id is set to a release then we display points against iterations
		/// 3) If release id is set to an iteration then we display points against days
		/// </remarks>
		public DataSet Requirement_RetrieveBurnDown(int projectId, int? releaseId, double utcOffset)
		{
			const string METHOD_NAME = "Requirement_RetrieveBurnDown";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet burndownDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				burndownDataSet.Tables.Add("Burndown");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = burndownDataSet.Tables["Burndown"].Columns.Add("XAxis", typeof(string));
				burndownDataSet.Tables["Burndown"].PrimaryKey = primaryKeys;
				DataColumn dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("CompletedPoints", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_CompletedPoints;
				dataColumn.ExtendedProperties.Add("Color", "7eff7a");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);
				dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("IdealBurndown", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_IdealBurndown;
				dataColumn.ExtendedProperties.Add("Color", "bbebfe");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);
				dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("RemainingPoints", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_RemainingPoints;
				dataColumn.ExtendedProperties.Add("Color", "f47457");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);

				//Now see if we have a release or not
				if (releaseId.HasValue)
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//Get the whole number of hours and minutes that the UTC offset is
						int utcOffsetHours = (int)Math.Truncate(utcOffset);
						double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
						int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

						//Next get the aggregate requirement story points for the dates in the iteration
						string query = @"
SELECT ISNULL(SUM(POINTS),0) AS POINTS, GRAPH_DATE
FROM
	(
	SELECT REQ.ESTIMATE_POINTS AS POINTS, CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",LAST_UPDATE_DATE)) AS FLOAT))AS DATETIME) AS GRAPH_DATE 
	FROM TST_REQUIREMENT REQ
	WHERE REQ.PROJECT_ID = " + projectId + @"
	AND REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
	AND REQ.RELEASE_ID = " + releaseOrIteration.ReleaseId + @") TMP
GROUP BY GRAPH_DATE
ORDER BY GRAPH_DATE
";
						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the starting number of points
							decimal remainingPoints = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								remainingPoints += (decimal)pointRow["POINTS"];
							}
							decimal idealBurndown = remainingPoints;
							decimal idealBurndownInterval = idealBurndown / (decimal)pointsDataSet.Tables[0].Rows.Count;

							//Copy across the burndown information
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
								burndownRow["XAxis"] = ((DateTime)pointRow["GRAPH_DATE"]).ToShortDateString();
								decimal points = (decimal)pointRow["POINTS"];

								burndownRow["CompletedPoints"] = points;
								burndownRow["IdealBurndown"] = idealBurndown;
								burndownRow["RemainingPoints"] = remainingPoints;
								burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);

								//Subtract the points from the remaining and update ideal
								remainingPoints -= points;
								idealBurndown -= idealBurndownInterval;
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Date;
						}
					}
					else
					{
						//Get the list of iterations in this release
						string releaseIndentLevel = releaseOrIteration.IndentLevel;
						List<ReleaseView> releases = releaseManager.RetrieveIterations(projectId, releaseId.Value, false);

						//Make sure we have at least one iteration
						if (releases.Count < 1)
						{
							return burndownDataSet;
						}

						//Next get the aggregate requirement story points for the iterations in this release
						string query = @"
SELECT REQ.RELEASE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
AND REL.RELEASE_TYPE_ID = " + (int)Release.ReleaseTypeEnum.Iteration + " AND LEN(REL.INDENT_LEVEL) = " + (releaseIndentLevel.Length + 3) + @"
AND SUBSTRING(REL.INDENT_LEVEL, 1, " + releaseIndentLevel.Length + @") = '" + releaseIndentLevel + @"' 
GROUP BY REQ.RELEASE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the starting number of points
							decimal remainingPoints = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								remainingPoints += (decimal)pointRow["POINTS"];
							}
							decimal idealBurndown = remainingPoints;
							decimal idealBurndownInterval = idealBurndown / (decimal)releases.Count;

							//Copy across the burndown information
							foreach (ReleaseView releaseRow in releases)
							{
								DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
								burndownRow["XAxis"] = releaseRow.VersionNumber;

								//See if we have that value in our requirements estimates
								decimal points = 0.0M;
								foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
								{
									//Add the release points
									if ((int)pointRow["RELEASE_ID"] == releaseRow.ReleaseId)
									{
										points += (decimal)pointRow["POINTS"];
									}
								}

								burndownRow["CompletedPoints"] = points;
								burndownRow["IdealBurndown"] = idealBurndown;
								burndownRow["RemainingPoints"] = remainingPoints;
								burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);

								//Subtract the points from the remaining and update ideal
								remainingPoints -= points;
								idealBurndown -= idealBurndownInterval;
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
						}
					}
				}
				else
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					if (releases.Count > 0)
					{
						//Next get the aggregate requirement story points for the releases and iterations in this project
						string query = @"
SELECT REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
GROUP BY REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						//Calculate the starting number of points
						decimal remainingPoints = 0.0M;
						foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
						{
							remainingPoints += (decimal)pointRow["POINTS"];
						}
						decimal idealBurndown = remainingPoints;
						decimal idealBurndownInterval = idealBurndown / (decimal)releases.Count;

						//Copy across the burndown information
						foreach (ReleaseView releaseRow in releases)
						{
							DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
							burndownRow["XAxis"] = releaseRow.VersionNumber;

							//See if we have that value in our requirements estimates (in the release or an iteration of the release)
							decimal points = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								string indentLevel = (string)pointRow["INDENT_LEVEL"];
								//Add the release points
								if ((int)pointRow["RELEASE_TYPE_ID"] != (int)Release.ReleaseTypeEnum.Iteration && indentLevel == releaseRow.IndentLevel)
								{
									points += (decimal)pointRow["POINTS"];
								}
								//Add the points for the child iterations
								if ((int)pointRow["RELEASE_TYPE_ID"] == (int)Release.ReleaseTypeEnum.Iteration && indentLevel.SafeSubstring(0, releaseRow.IndentLevel.Length) == releaseRow.IndentLevel && indentLevel.Length == releaseRow.IndentLevel.Length + 3)
								{
									points += (decimal)pointRow["POINTS"];
								}
							}

							burndownRow["CompletedPoints"] = points;
							burndownRow["IdealBurndown"] = idealBurndown;
							burndownRow["RemainingPoints"] = remainingPoints;
							burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);

							//Subtract the points from the remaining and update ideal
							remainingPoints -= points;
							idealBurndown -= idealBurndownInterval;
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Release;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return burndownDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the data used to populate the requirement burnup graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in</param>
		/// <returns>Dataset of points against the x-axis</returns>
		/// <remarks>1) If release id is null then we display points against major releases
		/// 2) If release id is set to a release then we display points against iterations
		/// 3) If release id is set to an iteration then we display points against days</remarks>
		public DataSet Requirement_RetrieveBurnUp(int projectId, int? releaseId, double utcOffset)
		{
			const string METHOD_NAME = "Requirement_RetrieveBurnUp";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet burnupDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				burnupDataSet.Tables.Add("Burnup");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = burnupDataSet.Tables["Burnup"].Columns.Add("XAxis", typeof(string));
				burnupDataSet.Tables["Burnup"].PrimaryKey = primaryKeys;
				DataColumn dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("CompletedPoints", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_CompletedPoints;
				dataColumn.ExtendedProperties.Add("Color", "7eff7a");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);
				dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("IdealBurnup", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_IdealBurnup;
				dataColumn.ExtendedProperties.Add("Color", "bbebfe");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);
				dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("ActualBurnup", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_ActualBurnup;
				dataColumn.ExtendedProperties.Add("Color", "f47457");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);

				//Add an initial starting row
				DataRow burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
				burnupRow["XAxis"] = GlobalResources.General.Graph_Start;
				burnupRow["CompletedPoints"] = 0.0M;
				burnupRow["IdealBurnup"] = 0.0M;
				burnupRow["ActualBurnup"] = 0.0M;
				burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);

				//Now see if we have a release or not
				if (releaseId.HasValue)
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//Get the whole number of hours and minutes that the UTC offset is
						int utcOffsetHours = (int)Math.Truncate(utcOffset);
						double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
						int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

						//Next get the aggregate requirement story points for the dates in the iteration
						string query = @"
SELECT ISNULL(SUM(POINTS),0) AS POINTS, GRAPH_DATE
FROM
	(
	SELECT REQ.ESTIMATE_POINTS AS POINTS, CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",LAST_UPDATE_DATE)) AS FLOAT))AS DATETIME) AS GRAPH_DATE 
	FROM TST_REQUIREMENT REQ
	WHERE REQ.PROJECT_ID = " + projectId + @"
	AND REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
	AND REQ.RELEASE_ID = " + releaseOrIteration.ReleaseId + @") TMP
GROUP BY GRAPH_DATE
ORDER BY GRAPH_DATE
";
						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the starting number of points
							decimal cumulativePoints = 0.0M;
							decimal idealBurnup = cumulativePoints;
							decimal totalPoints = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								totalPoints += (decimal)pointRow["POINTS"];
							}
							decimal idealBurnupInterval = totalPoints / (decimal)pointsDataSet.Tables[0].Rows.Count;

							//Copy across the burnup information
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
								burnupRow["XAxis"] = ((DateTime)pointRow["GRAPH_DATE"]).ToShortDateString();
								decimal points = (decimal)pointRow["POINTS"];

								//Add the points to the cumulative and update ideal
								cumulativePoints += points;
								idealBurnup += idealBurnupInterval;

								burnupRow["CompletedPoints"] = points;
								burnupRow["IdealBurnup"] = idealBurnup;
								burnupRow["ActualBurnup"] = cumulativePoints;
								burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Date;
						}
					}
					else
					{
						//Get the list of iterations in this release
						string releaseIndentLevel = releaseOrIteration.IndentLevel;
						List<ReleaseView> releases = releaseManager.RetrieveIterations(projectId, releaseId.Value, false);

						//Make sure we have at least one iteration
						if (releases.Count < 1)
						{
							return burnupDataSet;
						}

						//Next get the aggregate requirement story points for the iterations in this release
						string query = @"
SELECT REQ.RELEASE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
AND REL.RELEASE_TYPE_ID = " + (int)Release.ReleaseTypeEnum.Iteration + " AND LEN(REL.INDENT_LEVEL) = " + (releaseIndentLevel.Length + 3) + @"
AND SUBSTRING(REL.INDENT_LEVEL, 1, " + releaseIndentLevel.Length + @") = '" + releaseIndentLevel + @"' 
GROUP BY REQ.RELEASE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the starting number of points
							decimal cumulativePoints = 0.0M;
							decimal idealBurnup = cumulativePoints;
							decimal totalPoints = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								totalPoints += (decimal)pointRow["POINTS"];
							}
							decimal idealBurnupInterval = totalPoints / (decimal)releases.Count;

							//Copy across the burnup information
							foreach (ReleaseView releaseRow in releases)
							{
								burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
								burnupRow["XAxis"] = releaseRow.VersionNumber;

								//See if we have that value in our requirements estimates
								decimal points = 0.0M;
								foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
								{
									//Add the release points
									if ((int)pointRow["RELEASE_ID"] == releaseRow.ReleaseId)
									{
										points += (decimal)pointRow["POINTS"];
									}
								}

								//Add the points to the cumulative and update ideal
								cumulativePoints += points;
								idealBurnup += idealBurnupInterval;

								burnupRow["CompletedPoints"] = points;
								burnupRow["IdealBurnup"] = idealBurnup;
								burnupRow["ActualBurnup"] = cumulativePoints;
								burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
						}
					}
				}
				else
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					if (releases.Count > 0)
					{
						//Next get the aggregate requirement story points for the releases and iterations in this project
						string query = @"
SELECT REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
GROUP BY REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						//Calculate the starting number of points
						decimal cumulativePoints = 0.0M;
						decimal idealBurnup = cumulativePoints;
						decimal totalPoints = 0.0M;
						foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
						{
							totalPoints += (decimal)pointRow["POINTS"];
						}
						decimal idealBurnupInterval = totalPoints / (decimal)releases.Count;

						//Copy across the burnup information
						foreach (ReleaseView releaseRow in releases)
						{
							burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
							burnupRow["XAxis"] = releaseRow.VersionNumber;

							//See if we have that value in our requirements estimates (in the release or an iteration of the release)
							decimal points = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								string indentLevel = (string)pointRow["INDENT_LEVEL"];
								//Add the release points
								if ((int)pointRow["RELEASE_TYPE_ID"] != (int)Release.ReleaseTypeEnum.Iteration && indentLevel == releaseRow.IndentLevel)
								{
									points += (decimal)pointRow["POINTS"];
								}
								//Add the points for the child iterations
								if ((int)pointRow["RELEASE_TYPE_ID"] == (int)Release.ReleaseTypeEnum.Iteration && indentLevel.SafeSubstring(0, releaseRow.IndentLevel.Length) == releaseRow.IndentLevel && indentLevel.Length == releaseRow.IndentLevel.Length + 3)
								{
									points += (decimal)pointRow["POINTS"];
								}
							}

							//Add the points to the cumulative and update ideal
							cumulativePoints += points;
							idealBurnup += idealBurnupInterval;

							burnupRow["CompletedPoints"] = points;
							burnupRow["IdealBurnup"] = idealBurnup;
							burnupRow["ActualBurnup"] = cumulativePoints;
							burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Release;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return burnupDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>Retrieves the data used to populate the requirement velocity graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in (optional)</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <returns>Dataset of story points against the x-axis</returns>
		/// <remarks>1) If release id is null then we display story points against major releases
		/// 2) If release id is set to a release then we display story points against iterations
		/// 3) If release id is set to an iteration then we display story points against days in the iteration interval</remarks>
		public DataSet Requirement_RetrieveVelocity(int projectId, int? releaseId, double utcOffset)
		{
			const string METHOD_NAME = "Requirement_RetrieveVelocity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet velocityDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				velocityDataSet.Tables.Add("Velocity");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = velocityDataSet.Tables["Velocity"].Columns.Add("XAxis", typeof(string));
				velocityDataSet.Tables["Velocity"].PrimaryKey = primaryKeys;
				//Actual Velocity
				DataColumn dataColumn = velocityDataSet.Tables["Velocity"].Columns.Add("ActualVelocity", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_ActualVelocity;
				dataColumn.ExtendedProperties.Add("Color", "f47457");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.CumulativeBar);
				//Average Velocity
				dataColumn = velocityDataSet.Tables["Velocity"].Columns.Add("AverageVelocity", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_AverageVelocity;
				dataColumn.ExtendedProperties.Add("Color", "bbebfe");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);
				//Rolling Average
				dataColumn = velocityDataSet.Tables["Velocity"].Columns.Add("RollingAverage", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_RollingAverage;
				dataColumn.ExtendedProperties.Add("Color", "669900");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);

				//Now see if we have a release or not
				if (releaseId.HasValue)
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//Get the whole number of hours and minutes that the UTC offset is
						int utcOffsetHours = (int)Math.Truncate(utcOffset);
						double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
						int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

						//Next get the aggregate requirement story points for the dates in the iteration
						string query = @"
SELECT ISNULL(SUM(POINTS),0) AS POINTS, GRAPH_DATE
FROM
	(
	SELECT REQ.ESTIMATE_POINTS AS POINTS, CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",LAST_UPDATE_DATE)) AS FLOAT))AS DATETIME) AS GRAPH_DATE 
	FROM TST_REQUIREMENT REQ
	WHERE REQ.PROJECT_ID = " + projectId + @"
	AND REQ.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
	AND REQ.RELEASE_ID = " + releaseOrIteration.ReleaseId + @") TMP
GROUP BY GRAPH_DATE
ORDER BY GRAPH_DATE
";
						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the absolute average
							decimal averageVelocity = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								averageVelocity += (decimal)pointRow["POINTS"];
							}
							averageVelocity = averageVelocity / (decimal)pointsDataSet.Tables[0].Rows.Count;

							//Copy across the velocity information
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
								velocityRow["XAxis"] = ((DateTime)pointRow["GRAPH_DATE"]).ToShortDateString();
								decimal points = (decimal)pointRow["POINTS"];

								//The rolling average
								decimal rollingAverage = points;
								int count = 1;
								for (int i = velocityDataSet.Tables["Velocity"].Rows.Count - 1; i >= 0; i--)
								{
									rollingAverage += (decimal)(velocityDataSet.Tables["Velocity"].Rows[i]["ActualVelocity"]);
									count++;
								}
								rollingAverage /= count;

								velocityRow["ActualVelocity"] = points;
								velocityRow["AverageVelocity"] = averageVelocity;
								velocityRow["RollingAverage"] = rollingAverage;
								velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Date;
						}
					}
					else
					{
						//Get the list of iterations in this release
						string releaseIndentLevel = releaseOrIteration.IndentLevel;
						List<ReleaseView> releases = releaseManager.RetrieveIterations(projectId, releaseId.Value, false);

						//Make sure we have at least one iteration
						if (releases.Count < 1)
						{
							return velocityDataSet;
						}

						//Next get the aggregate requirement story points for the iterations in this release
						string query = @"
SELECT REQ.RELEASE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
AND REL.RELEASE_TYPE_ID = " + (int)Release.ReleaseTypeEnum.Iteration + " AND LEN(REL.INDENT_LEVEL) = " + (releaseIndentLevel.Length + 3) + @"
AND SUBSTRING(REL.INDENT_LEVEL, 1, " + releaseIndentLevel.Length + @") = '" + releaseIndentLevel + @"' 
GROUP BY REQ.RELEASE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						if (pointsDataSet.Tables[0].Rows.Count > 0)
						{
							//Calculate the absolute average
							decimal averageVelocity = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								averageVelocity += (decimal)pointRow["POINTS"];
							}
							averageVelocity = averageVelocity / (decimal)releases.Count;

							//Copy across the velocity information
							foreach (ReleaseView releaseRow in releases)
							{
								DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
								velocityRow["XAxis"] = releaseRow.VersionNumber;

								//See if we have that value in our requirements estimates
								decimal points = 0.0M;
								foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
								{
									//Add the release points
									if ((int)pointRow["RELEASE_ID"] == releaseRow.ReleaseId)
									{
										points += (decimal)pointRow["POINTS"];
									}
								}

								//The rolling average
								decimal rollingAverage = points;
								int count = 1;
								for (int i = velocityDataSet.Tables["Velocity"].Rows.Count - 1; i >= 0; i--)
								{
									rollingAverage += (decimal)(velocityDataSet.Tables["Velocity"].Rows[i]["ActualVelocity"]);
									count++;
								}
								rollingAverage /= count;

								velocityRow["ActualVelocity"] = points;
								velocityRow["AverageVelocity"] = averageVelocity;
								velocityRow["RollingAverage"] = rollingAverage;
								velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
							}
							xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
						}
					}
				}
				else
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					if (releases.Count > 0)
					{
						//Next get the aggregate requirement story points for the releases and iterations in this project
						string query = @"
SELECT REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL, ISNULL(SUM(REQ.ESTIMATE_POINTS), 0) AS POINTS
FROM TST_REQUIREMENT REQ
INNER JOIN TST_RELEASE REL ON REQ.RELEASE_ID = REL.RELEASE_ID
WHERE REQ.PROJECT_ID = " + projectId + @"
AND REQ.IS_DELETED = 0 AND REL.IS_DELETED = 0 AND REQ.IS_SUMMARY = 0
GROUP BY REQ.RELEASE_ID, REL.RELEASE_TYPE_ID, REL.INDENT_LEVEL
ORDER BY REL.INDENT_LEVEL
";

						DataSet pointsDataSet;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							pointsDataSet = ExecuteSql(context, query, "RequirementPoints");
						}

						//Calculate the absolute average
						decimal averageVelocity = 0.0M;
						foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
						{
							averageVelocity += (decimal)pointRow["POINTS"];
						}
						averageVelocity = averageVelocity / (decimal)releases.Count;

						//Copy across the velocity information
						foreach (ReleaseView releaseRow in releases)
						{
							DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
							velocityRow["XAxis"] = releaseRow.VersionNumber;

							//See if we have that value in our requirements estimates (in the release or an iteration of the release)
							decimal points = 0.0M;
							foreach (DataRow pointRow in pointsDataSet.Tables[0].Rows)
							{
								string indentLevel = (string)pointRow["INDENT_LEVEL"];
								//Add the release points
								if ((int)pointRow["RELEASE_TYPE_ID"] != (int)Release.ReleaseTypeEnum.Iteration && indentLevel == releaseRow.IndentLevel)
								{
									points += (decimal)pointRow["POINTS"];
								}
								//Add the points for the child iterations
								if ((int)pointRow["RELEASE_TYPE_ID"] == (int)Release.ReleaseTypeEnum.Iteration && indentLevel.SafeSubstring(0, releaseRow.IndentLevel.Length) == releaseRow.IndentLevel && indentLevel.Length == releaseRow.IndentLevel.Length + 3)
								{
									points += (decimal)pointRow["POINTS"];
								}
							}

							//The rolling average
							decimal rollingAverage = points;
							int count = 1;
							for (int i = velocityDataSet.Tables["Velocity"].Rows.Count - 1; i >= 0; i--)
							{
								rollingAverage += (decimal)(velocityDataSet.Tables["Velocity"].Rows[i]["ActualVelocity"]);
								count++;
							}
							rollingAverage /= count;

							velocityRow["ActualVelocity"] = points;
							velocityRow["AverageVelocity"] = averageVelocity;
							velocityRow["RollingAverage"] = rollingAverage;
							velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Release;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return velocityDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset of total requirement coverage for a project by importance
		/// </summary>
		/// <param name="projectId">The project ID we're interested in</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="releaseId">The release we want to filter on (null for all)</param>
		/// <returns>Untyped dataset of requirements coverage by importance</returns>
		public System.Data.DataSet Requirement_RetrieveCoverageByImportance(int projectId, int projectTemplateId, Nullable<int> releaseId = null)
		{
			const string METHOD_NAME = "Requirement_RetrieveCoverageByImportance";

			System.Data.DataSet coverageDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the release filter if necessary
				string releaseFilterClause = "";
				if (releaseId.HasValue)
				{
					//The release may have child iterations that we need to account for
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilterClause = " AND RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//First create the dataset that will hold the results
				coverageDataSet.Tables.Add("RequirementsCoverage");

				//Now add the column that contains the coverage
				DataColumn[] primaryKeys = new DataColumn[1];
				primaryKeys[0] = coverageDataSet.Tables["RequirementsCoverage"].Columns.Add("CoverageStatus", typeof(string));
				primaryKeys[0].Caption = GlobalResources.General.Graph_CoverageStatus;
				coverageDataSet.Tables["RequirementsCoverage"].PrimaryKey = primaryKeys;

				//Now iterate through all the possible importance values to create the actual columns
				//Need to also add a column for Importance = None
				//Since the list of priorities is cached statically, we must not modify the source list itself
				List<Importance> importances = new List<Importance>();
				importances.AddRange(new RequirementManager().RequirementImportance_Retrieve(projectTemplateId));
				Importance noneImportance = new Importance();
				noneImportance.ImportanceId = -1;
				noneImportance.Name = GlobalResources.General.Global_None;
				importances.Add(noneImportance);
				foreach (Importance importance in importances)
				{
					int importanceId = importance.ImportanceId;

					//Create a new column for each importance
					DataColumn importanceColumn = new DataColumn(importanceId.ToString(), typeof(double));
					importanceColumn.DefaultValue = "0";
					importanceColumn.ExtendedProperties["Color"] = importance.Color;
					importanceColumn.Caption = importance.Name; //Put the importance name as the caption
					coverageDataSet.Tables["RequirementsCoverage"].Columns.Add(importanceColumn);

					//Now get the list of coverage count for this importance and release
					//Create select command for retrieving the total number of requirements per coverage status
					//(i.e. sum of coverage per requirement normalized by the count for that requirement)
					string importanceClause = "";
					if (importanceId == -1)
					{
						importanceClause = "IMPORTANCE_ID IS NULL";
					}
					else
					{
						importanceClause = "IMPORTANCE_ID = " + importanceId;
					}

					string sql =
						"SELECT	1 AS CoverageStatusOrder, 'Passed' AS CoverageStatus, SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_PASSED AS FLOAT(53)) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause +
						"UNION " +
						"SELECT	2 AS CoverageStatusOrder, 'Failed' AS CoverageStatus, SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_FAILED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause +
						"UNION " +
						"SELECT	3 AS CoverageStatusOrder, 'Blocked' AS CoverageStatus, SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_BLOCKED AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause +
						"UNION " +
						"SELECT	4 AS CoverageStatusOrder, 'Caution' AS CoverageStatus, SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE CAST(COVERAGE_COUNT_CAUTION AS FLOAT(53)) / CAST(COVERAGE_COUNT_TOTAL AS FLOAT(53)) END) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause +
						"UNION " +
						"SELECT	5 AS CoverageStatusOrder, 'Not Run' AS CoverageStatus, SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE (CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) - CAST (COVERAGE_COUNT_PASSED AS FLOAT(53)) - CAST (COVERAGE_COUNT_CAUTION AS FLOAT(53)) - CAST (COVERAGE_COUNT_BLOCKED AS FLOAT(53)) - CAST (COVERAGE_COUNT_FAILED AS FLOAT(53))) / CAST (COVERAGE_COUNT_TOTAL AS FLOAT(53)) END) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause +
						"UNION " +
						"SELECT	6 AS CoverageStatusOrder, 'Not Covered' AS CoverageStatus, CAST (COUNT(REQUIREMENT_ID) - SUM(CASE COVERAGE_COUNT_TOTAL WHEN 0 THEN 0 ELSE 1 END) AS FLOAT(53)) AS CoverageCount " +
						"FROM	TST_REQUIREMENT " +
						"WHERE	PROJECT_ID = " + projectId + " AND IS_DELETED = 0 AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected + " AND REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete + " AND " + importanceClause + " " + releaseFilterClause;

					//Actually execute the query and return the dataset
					DataSet tempDataSet;
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						tempDataSet = ExecuteSql(context, sql, "TempCount");
					}

					//Now iterate through this dataset and populate the master dataset
					for (int j = 0; j < tempDataSet.Tables["TempCount"].Rows.Count; j++)
					{
						string coverageStatus = (string)tempDataSet.Tables["TempCount"].Rows[j]["CoverageStatus"];
						double coverageCount = 0;
						if (tempDataSet.Tables["TempCount"].Rows[j]["CoverageCount"] != DBNull.Value)
						{
							coverageCount = (double)tempDataSet.Tables["TempCount"].Rows[j]["CoverageCount"];
						}

						//Locate the matching row in the master dataset - create if not already there
						DataRow matchingRow = coverageDataSet.Tables["RequirementsCoverage"].Rows.Find(coverageStatus);
						if (matchingRow == null)
						{
							matchingRow = coverageDataSet.Tables["RequirementsCoverage"].NewRow();
							matchingRow["CoverageStatus"] = coverageStatus;
							coverageDataSet.Tables["RequirementsCoverage"].Rows.Add(matchingRow);
						}
						matchingRow[importanceId.ToString()] = coverageCount;
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return coverageDataSet;
		}

		/// <summary>Retrieves a list of test-case execution status summary for a project / release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A Dictionary</returns>
		/// <remarks>Always returns a Dictionary of Author and TestCase count</remarks>
		public System.Data.DataSet RetrieveTestPreparationSummary(int projectId)
		{
			const string METHOD_NAME = "RetrieveTestPreparationSummary";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Execute the stored procedure
				Dictionary<string, int> executionStatiSummary = new Dictionary<string, int>();
				DataSet executionSummary = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					string sql = $@"EXEC [dbo].[VM_TEST_CASE_PREPARATION_STATUS_PIVOT] {projectId}";
					executionSummary = ExecuteSql(context, sql, "ExecutionSummary");
					DataColumn[] primaryKeys = new DataColumn[1];
					primaryKeys[0] = executionSummary.Tables[0].Columns[0];
					executionSummary.Tables[0].PrimaryKey = primaryKeys;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionSummary;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>Retrieves the summary count for a specified x-axis and group-by field. Helper method used by the various business objects.</summary>
		/// <param name="xAxisFieldName">The field we want to have as the x-axis</param>
		/// <param name="groupByFieldName">The field we want to group by</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <param name="artifactType">The type of artifact we're interested in</param>
		/// <param name="releaseId">The release id (null = all)</param>
		/// <returns>The count dataset</returns>
		/// <remarks>
		/// 1) This is used in the reports center.
		/// 2) This overload is used for entity based artifacts
		/// </remarks>
		public System.Data.DataSet RetrieveSummaryCount(int projectId, int projectTemplateId, string xAxisFieldName, string groupByFieldName, DataModel.Artifact.ArtifactTypeEnum artifactType, int? releaseId = null)
		{
			const string METHOD_NAME = "RetrieveSummaryCount";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			System.Data.DataSet summaryCountDataSet = new System.Data.DataSet();

			try
			{
				//If we have the incident priority or severity specified as the group by, need to get the list of priorities
				//and severities from the database so that we can specify the colors later
				List<IncidentPriority> incidentPriorities = null;
				List<IncidentSeverity> incidentSeverities = null;
				List<Importance> requirementImportances = null;
				List<TestCasePriority> testCasePriorities = null;
				List<TaskPriority> taskPriorities = null;
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Incident && groupByFieldName == "PriorityId")
				{
					incidentPriorities = new IncidentManager().RetrieveIncidentPriorities(projectTemplateId, false);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Incident && groupByFieldName == "SeverityId")
				{
					incidentSeverities = new IncidentManager().RetrieveIncidentSeverities(projectTemplateId, false);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Requirement && groupByFieldName == "ImportanceId")
				{
					requirementImportances = new RequirementManager().RequirementImportance_Retrieve(projectTemplateId, false);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestCase && groupByFieldName == "TestCasePriorityId")
				{
					testCasePriorities = new TestCaseManager().TestCasePriority_Retrieve(projectTemplateId, false);
				}
				if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Task && groupByFieldName == "TaskPriorityId")
				{
					taskPriorities = new TaskManager().TaskPriority_Retrieve(projectTemplateId, false);
				}

				//Instantiate helper classes
				ArtifactManager artifactManager = new ArtifactManager();
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();

				//The secondary table is used to link to the release-test-case table so that we can display the test results
				//properly for a specific release. [TK:1451]
				string secondaryTable = "";

				//Create the release filter clause if needed (different for incidents)
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						if (artifactType == Artifact.ArtifactTypeEnum.Incident)
						{
							releaseFilter = " AND PNT.DETECTED_RELEASE_ID IN (" + releaseList + ") ";
						}
						if (artifactType == Artifact.ArtifactTypeEnum.Requirement || artifactType == Artifact.ArtifactTypeEnum.Release || artifactType == Artifact.ArtifactTypeEnum.Risk || artifactType == Artifact.ArtifactTypeEnum.Task || artifactType == Artifact.ArtifactTypeEnum.TestRun || artifactType == Artifact.ArtifactTypeEnum.TestSet)
						{
							releaseFilter = " AND PNT.RELEASE_ID IN (" + releaseList + ") ";
						}

						//If we have a test case summary graph, need to join the appropriate TST_RELEASE_TEST_CASE table
						//Cannot do it through the WHERE clause like the other release filters because the execution status changes
						if (artifactType == Artifact.ArtifactTypeEnum.TestCase)
						{
							secondaryTable = " INNER JOIN TST_RELEASE_TEST_CASE SEC ON PNT.TEST_CASE_ID = SEC.TEST_CASE_ID AND SEC.RELEASE_ID = " + releaseId.Value + " INNER JOIN TST_EXECUTION_STATUS EXE ON SEC.EXECUTION_STATUS_ID = EXE.EXECUTION_STATUS_ID ";
						}
					}
				}

				//See if we are using one of the special known multi-select standard fields that need to get their
				//lookups from a data-source (similar to how custom properties work)
				List<Component> components = null;
				if (groupByFieldName == "ComponentIds" || xAxisFieldName == "ComponentIds")
				{
					components = new ComponentManager().Component_Retrieve(projectId);
				}

				//Get the table name and primary key from the artifact type
				//For dataset based artifacts, we need the dataset XSD path
				//For entity based artifacts, we get the table name from the .edmx metadata
				Type entityType = null;
				string primaryKey = "";
				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						entityType = typeof(RequirementView);
						primaryKey = "RequirementId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Task:
						entityType = typeof(TaskView);
						primaryKey = "TaskId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						entityType = typeof(IncidentView);
						primaryKey = "IncidentId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						entityType = typeof(TestCaseView);
						primaryKey = "TestCaseId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						entityType = typeof(TestSetView);
						primaryKey = "TestSetId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
						entityType = typeof(AutomationHostView);
						primaryKey = "AutomationHostId";
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						entityType = typeof(TestRunView);
						primaryKey = "TestRunId";
						break;
				}

				if (entityType == null)
				{
					throw new InvalidOperationException("You need to provide a valid artifact type for this function");
				}

				//Need to query the .edmx to get the entity physical information
				string sourceTable = artifactManager.GetEntityMapping(entityType);
				if (String.IsNullOrEmpty(sourceTable))
				{
					throw new Exception("Cannot locate the source table for database entity '" + entityType.FullName + "'");
				}

				//We need to get the underlying database information for the group-by field.
				//We get that from the artifact field meta-data
				//Also handle the special case of custom properties
				int? groupByCustomField = null;
				string groupBySourceColumn = null;
				string groupByLookupFieldName = null;
				string groupByLookupSourceColumn = null;
				bool groupByMultiSelect = false;

				//Handle the special case of a custom list property (single or multi-valued)
				int? customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(groupByFieldName);
				if (customPropertyNumber.HasValue)
				{
					//We're grouping by a custom property, so make sure we have a list, user or multilist type
					CustomProperty customProperty = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, artifactType, customPropertyNumber.Value, false);
					if (customProperty != null && (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User))
					{
						//We have a custom property
						groupByCustomField = customPropertyNumber.Value;

						//Get the physical table name
						groupBySourceColumn = artifactManager.GetPropertyMapping(entityType, groupByFieldName);
						if (String.IsNullOrEmpty(groupBySourceColumn))
						{
							throw new Exception("Unable to access source column alias for group-by field '" + groupByFieldName + "'");
						}
					}
				}
				else
				{
					//We're grouping by a standard field
					ArtifactField groupByField = artifactManager.ArtifactField_RetrieveByName(artifactType, groupByFieldName);
					if (groupByField == null)
					{
						throw new Exception("Unable to access artifact field for group-by field '" + groupByFieldName + "'");
					}

					//Get the physical table name
					groupBySourceColumn = artifactManager.GetPropertyMapping(entityType, groupByFieldName);
					if (String.IsNullOrEmpty(groupBySourceColumn))
					{
						throw new Exception("Unable to access source column alias for group-by field '" + groupByFieldName + "'");
					}

					//If we're filtering by a standard multi-select field, cannot use a lookup field, but have to get the values
					//directly from the appropriate manager
					if (groupByFieldName == "ComponentIds")
					{
						groupByMultiSelect = true;
					}
					else
					{
						//Now get the lookup name
						groupByLookupFieldName = groupByField.LookupProperty;
						if (String.IsNullOrEmpty(groupByLookupFieldName))
						{
							throw new Exception("Unable to access lookup property for group-by field '" + groupByFieldName + "'");
						}

						//Now get the lookup physical table name
						groupByLookupSourceColumn = artifactManager.GetPropertyMapping(entityType, groupByLookupFieldName);
						if (String.IsNullOrEmpty(groupByLookupSourceColumn))
						{
							throw new Exception("Unable to access source column alias for group-by lookup property '" + groupByLookupFieldName + "'");
						}
					}
				}

				//Now we need to get the group-by data set for either the standard or custom fields
				System.Data.DataSet groupByDataSet = new System.Data.DataSet();
				Dictionary<int, string> lookupValues = null;
				if (groupByCustomField.HasValue)
				{
					//See if we have a list or user field
					CustomProperty customProperty = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, artifactType, groupByCustomField.Value, true);
					if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
					{
						lookupValues = new Dictionary<int, string>();
						foreach (CustomPropertyValue cpv in customProperty.List.Values)
						{
							if (!lookupValues.ContainsKey(cpv.CustomPropertyValueId))
							{
								lookupValues.Add(cpv.CustomPropertyValueId, cpv.Name);
							}
						}
					}
					if (customProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User)
					{
						//Get the list of users in the project
						lookupValues = new Dictionary<int, string>();
						List<User> users = new UserManager().RetrieveActiveByProjectId(projectId);
						foreach (User user in users)
						{
							if (!lookupValues.ContainsKey(user.UserId))
							{
								lookupValues.Add(user.UserId, user.FullName);
							}
						}
					}
				}
				else
				{
					//See if we have a special case of a multi-select that we have known values
					if (groupByMultiSelect)
					{
						if (groupByFieldName == "ComponentIds" && components != null)
						{
							//Make a new dataset to hold the values
							DataTable dataTable = groupByDataSet.Tables.Add("GroupBy");
							dataTable.Columns.Add("GroupById", typeof(int));
							dataTable.Columns.Add("GroupByName", typeof(string));
							foreach (Component component in components)
							{
								DataRow dataRow = dataTable.NewRow();
								dataRow["GroupById"] = component.ComponentId;
								dataRow["GroupByName"] = component.Name;
								dataTable.Rows.Add(dataRow);
							}
							groupByDataSet.AcceptChanges();
						}
					}
					else
					{
						//We need to exclude 'not run' test runs from the count
						string addlWhereClause = "";
						if (artifactType == DataModel.Artifact.ArtifactTypeEnum.TestRun)
						{
							addlWhereClause = " AND PNT.EXECUTION_STATUS_ID <> " + (int)TestCase.ExecutionStatusEnum.NotRun + " ";
						}

						//Add on the release filter if necessary
						addlWhereClause += releaseFilter;

						//Create the appropriate group-by clause
						string groupByPrefix = "PNT";

						//See if we have a secondary table to use
						//Only for test case execution status by release currently
						if (!String.IsNullOrEmpty(secondaryTable) && groupBySourceColumn == "EXECUTION_STATUS_ID")
						{
							groupByPrefix = "SEC";
							groupByLookupSourceColumn = "EXE.NAME";
						}

						//Now we need to get the list of used values in the group-by field
						string sql =
							"SELECT		" + groupByPrefix + "." + groupBySourceColumn + " AS GroupById, MIN(" + groupByLookupSourceColumn + ") AS GroupByName " +
							"FROM		" + sourceTable + " PNT " + secondaryTable + " " +
							"WHERE		PNT.PROJECT_ID = " + projectId.ToString() + " " +
							"AND        PNT.IS_DELETED = 0 " + addlWhereClause + " " +
							"GROUP BY " + groupByPrefix + "." + groupBySourceColumn + " " +
							"ORDER BY	GroupByName";

						//Actually execute the query and return the dataset
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							groupByDataSet = ExecuteSql(context, sql, "GroupBy");
						}
					}
				}

				//Make sure we have the necessary x-axis metadata
				//Also handle the special case of custom properties
				int? xAxisCustomField = null;
				bool isXAxisUserCustomProperty = false;
				bool isXAxisMultiListCustomProperty = false;
				string xAxisSourceColumn = null;
				string xAxisLookupFieldName = null;
				string xAxisLookupSourceColumn = null;
				CustomProperty xAxisCustomProperty = null;

				//Handle the special case of a custom list property (single, user or multi-valued)
				customPropertyNumber = CustomPropertyManager.IsFieldCustomProperty(xAxisFieldName);
				if (customPropertyNumber.HasValue)
				{
					//We're using a custom property as the x-axis
					xAxisCustomProperty = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactTypeAtPropertyNumber(projectTemplateId, artifactType, customPropertyNumber.Value, false);
					if (xAxisCustomProperty != null && (xAxisCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List || xAxisCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList || xAxisCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User))
					{
						//We have a custom property
						xAxisCustomField = customPropertyNumber.Value;

						//Get the physical table name
						xAxisSourceColumn = artifactManager.GetPropertyMapping(entityType, xAxisFieldName);
						if (String.IsNullOrEmpty(xAxisSourceColumn))
						{
							throw new Exception("Unable to access source column alias for x-axis field '" + xAxisFieldName + "' in entity type '" + entityType.FullName + "'");
						}

						//See if we have a user custom property
						if (xAxisCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.User)
						{
							isXAxisUserCustomProperty = true;
						}

						//See if we have a multilist custom property
						if (xAxisCustomProperty.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList)
						{
							isXAxisMultiListCustomProperty = true;
						}
					}
				}
				else
				{
					//We're using a standard field for the x-axis

					//Get the definition of the x-axis field
					ArtifactField xAxisField = artifactManager.ArtifactField_RetrieveByName(artifactType, xAxisFieldName);
					if (xAxisField == null)
					{
						throw new Exception("Unable to access artifact field for x-axis field '" + xAxisFieldName + "' in entity type '" + entityType.FullName + "'");
					}

					//Get the physical table name
					xAxisSourceColumn = artifactManager.GetPropertyMapping(entityType, xAxisFieldName);
					if (String.IsNullOrEmpty(xAxisSourceColumn))
					{
						throw new Exception("Unable to access source column alias for x-axis field '" + xAxisFieldName + "' in entity type '" + entityType.FullName + "'");
					}

					//Now get the lookup name, unless known component multi-value field
					if (xAxisFieldName != "ComponentIds")
					{
						xAxisLookupFieldName = xAxisField.LookupProperty;
						if (String.IsNullOrEmpty(xAxisLookupFieldName))
						{
							throw new Exception("Unable to access lookup property for x-axis field '" + xAxisFieldName + "'");
						}

						//Now get the lookup physical table name
						xAxisLookupSourceColumn = artifactManager.GetPropertyMapping(entityType, xAxisLookupFieldName);
						if (String.IsNullOrEmpty(xAxisLookupSourceColumn))
						{
							throw new Exception("Unable to access source column alias for x-axis lookup property '" + xAxisLookupFieldName + "'");
						}
					}

					//Check to the case where we have a secondary table (used for test case execution status filtered by release)
					if (!String.IsNullOrEmpty(secondaryTable) && xAxisLookupSourceColumn == "EXECUTION_STATUS_NAME")
					{
						//We have to get the execution status from the separate release table not the normal lookup joined in the main view
						xAxisLookupSourceColumn = "EXE.NAME";
					}
				}

				//Now create the master dataset that will contain the xaxis field against group-by field
				const string outputTableName = "SummaryCount";
				summaryCountDataSet.Tables.Add(outputTableName);

				//Add the xaxis columns
				DataColumn xAxisColumn = summaryCountDataSet.Tables[outputTableName].Columns.Add("XAxis", typeof(string));
				xAxisColumn.Caption = xAxisFieldName;
				DataColumn[] primaryKeys = new DataColumn[1];
				primaryKeys[0] = xAxisColumn;
				summaryCountDataSet.Tables[outputTableName].PrimaryKey = primaryKeys;

				//Need to get the physical column name of the primary key (e.g. RequirementId -> REQUIREMENT_ID)
				string primaryKeyColumn = artifactManager.GetPropertyMapping(entityType, primaryKey);
				if (String.IsNullOrEmpty(primaryKeyColumn))
				{
					primaryKeyColumn = primaryKey;
				}

				//See if we have a custom field or not
				if (xAxisCustomField.HasValue && xAxisCustomProperty != null)
				{
					//Change the caption of the field
					xAxisColumn.Caption = xAxisCustomProperty.Name;

					//For custom property x-axis graphs we need to use a special LIKE comparison
					//to handle the case of multi-valued list values
					//Also we need to check for the case of user custom properties where we need to join against the TST_USER_PROFILE table instead
					string innerXAxisClause;
					if (isXAxisUserCustomProperty)
					{
						//User
						innerXAxisClause = "SELECT CP1.*, (UP.FIRST_NAME + ' ' + UP.LAST_NAME) AS XAxis FROM TST_ARTIFACT_CUSTOM_PROPERTY CP1 INNER JOIN TST_USER_PROFILE UP ON dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(CP1." + xAxisSourceColumn + ", UP.USER_ID) = 1 WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType;
					}
					else if (isXAxisMultiListCustomProperty)
					{
						//Multilist
						innerXAxisClause = "SELECT CP1.*, CP2.NAME AS XAxis FROM TST_ARTIFACT_CUSTOM_PROPERTY CP1 INNER JOIN TST_CUSTOM_PROPERTY_VALUE CP2 ON dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(CP1." + xAxisSourceColumn + ", CP2.CUSTOM_PROPERTY_VALUE_ID) = 1 WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType;
					}
					else
					{
						//List
						innerXAxisClause = "SELECT CP1.*, CP2.NAME AS XAxis FROM TST_ARTIFACT_CUSTOM_PROPERTY CP1 INNER JOIN TST_CUSTOM_PROPERTY_VALUE CP2 ON dbo.FN_CUSTOM_PROPERTY_EQUALS_INT(CP1." + xAxisSourceColumn + ", CP2.CUSTOM_PROPERTY_VALUE_ID) = 1 WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType;
					}

					//See if we have a standard field or custom property being grouped by
					if (lookupValues == null)
					{
						//Grouping by standard field
						//Create select command for retrieving the summary count for each item being grouped
						for (int i = 0; i < groupByDataSet.Tables["GroupBy"].Rows.Count; i++)
						{
							//Create a temporary dataset for holding the query for a particular group
							System.Data.DataSet tempDataSet = new System.Data.DataSet();

							//Standard fields store all their values as Int32
							int? groupById = (groupByDataSet.Tables["GroupBy"].Rows[i]["GroupById"] == DBNull.Value) ? null : (int?)((int)groupByDataSet.Tables["GroupBy"].Rows[i]["GroupById"]);
							string groupByName = (groupByDataSet.Tables["GroupBy"].Rows[i]["GroupByName"] == DBNull.Value) ? GlobalResources.General.Global_None : (string)groupByDataSet.Tables["GroupBy"].Rows[i]["GroupByName"];

							//Add the group-by name to the master dataset - default value 0
							DataColumn groupByColumn = new DataColumn((groupById.HasValue) ? groupById.Value.ToString() : "0", typeof(int));
							groupByColumn.DefaultValue = 0;
							groupByColumn.Caption = groupByName; //Put the name as the caption
							summaryCountDataSet.Tables[outputTableName].Columns.Add(groupByColumn);

							object colorLookupList = null;
							if (incidentPriorities != null)
							{
								colorLookupList = incidentPriorities;
							}
							if (incidentSeverities != null)
							{
								colorLookupList = incidentSeverities;
							}
							if (requirementImportances != null)
							{
								colorLookupList = requirementImportances;
							}
							if (testCasePriorities != null)
							{
								colorLookupList = testCasePriorities;
							}
							if (taskPriorities != null)
							{
								colorLookupList = taskPriorities;
							}

							//See if we have a special column that we need to assign a specific color to
							SetKnownColors(groupByColumn, artifactType, groupByFieldName, groupById, colorLookupList);

							//Create the appropriate group-by clause
							string groupByPrefix = "PNT";

							//See if we have a secondary table to use
							//Only for test case execution status by release currently
							if (!String.IsNullOrEmpty(secondaryTable) && groupBySourceColumn == "EXECUTION_STATUS_ID")
							{
								groupByPrefix = "SEC";
							}

							string whereClause;
							if (groupById.HasValue)
							{
								//Need to see if we have a multi-select or single-select
								if (groupByMultiSelect)
								{
									whereClause = "dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(" + groupByPrefix + "." + groupBySourceColumn + ", " + groupById + ") = 1 ";
								}
								else
								{
									whereClause = groupByPrefix + "." + groupBySourceColumn + " = " + groupById + " ";
								}
							}
							else
							{
								whereClause = groupByPrefix + "." + groupBySourceColumn + " IS NULL ";
							}
							whereClause += releaseFilter;

							string sql =
								"SELECT		ACP.XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
								"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
								"           (" + innerXAxisClause + ") ACP " +
								"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID " +
								"WHERE		PNT.PROJECT_ID = " + projectId + " " +
								"AND	    " + whereClause +
								"AND        PNT.IS_DELETED = 0 " +
								"GROUP BY	ACP.XAxis";

							//Actually execute the query and return the dataset
							using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
							{
								tempDataSet = ExecuteSql(context, sql, "TempCount");
							}

							//Copy across to the final table
							CopyAcrossSummaryRow(summaryCountDataSet, tempDataSet, groupById, outputTableName);
						}
					}
					else
					{
						//Grouping by custom property
						foreach (KeyValuePair<int, string> kvp in lookupValues)
						{
							int groupById = kvp.Key;
							string groupByName = kvp.Value;

							//Create a temporary dataset for holding the query for a particular group
							System.Data.DataSet tempDataSet = new System.Data.DataSet();

							//Add the group-by name to the master dataset - default value 0
							DataColumn groupByColumn = new DataColumn(groupById.ToString(), typeof(int));
							groupByColumn.DefaultValue = 0;
							groupByColumn.Caption = groupByName; //Put the name as the caption
							summaryCountDataSet.Tables[outputTableName].Columns.Add(groupByColumn);

							//Create the appropriate group-by clause
							string groupByPrefix = "ACP";

							//Since custom property values can be multi-valued we need to use a special comparison form
							string whereClause = "dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(" + groupByPrefix + "." + groupBySourceColumn + ", " + groupById + ") = 1 ";
							whereClause += releaseFilter;

							//Since test runs don't directly link to project-id, need to use an additional join
							string sql =
								"SELECT		ACP.XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
								"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
								"           (" + innerXAxisClause + ") ACP " +
								"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID " +
								"WHERE		PNT.PROJECT_ID = " + projectId + " " +
								"AND	    " + whereClause +
								"AND        PNT.IS_DELETED = 0 " +
								"GROUP BY	ACP.XAxis";

							//Actually execute the query and return the dataset
							using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
							{
								tempDataSet = ExecuteSql(context, sql, "TempCount");
							}

							//Copy across to the final table
							CopyAcrossSummaryRow(summaryCountDataSet, tempDataSet, groupById, outputTableName);
						}
					}
				}
				else
				{
					//See if we have a standard field or custom property being grouped by
					if (lookupValues == null)
					{
						//Grouping by standard field
						//Create select command for retrieving the summary count for each item being grouped
						for (int i = 0; i < groupByDataSet.Tables["GroupBy"].Rows.Count; i++)
						{
							//Create a temporary dataset for holding the query for a particular group
							System.Data.DataSet tempDataSet = new System.Data.DataSet();

							int? groupById = (groupByDataSet.Tables["GroupBy"].Rows[i]["GroupById"] == DBNull.Value) ? null : (int?)((int)groupByDataSet.Tables["GroupBy"].Rows[i]["GroupById"]);
							string groupByName = (groupByDataSet.Tables["GroupBy"].Rows[i]["GroupByName"] == DBNull.Value) ? GlobalResources.General.Global_None : (string)groupByDataSet.Tables["GroupBy"].Rows[i]["GroupByName"];

							//Add the group-by name to the master dataset - default value 0
							DataColumn groupByColumn = new DataColumn((groupById.HasValue) ? groupById.Value.ToString() : "0", typeof(int));
							groupByColumn.DefaultValue = 0;
							groupByColumn.Caption = groupByName; //Put the name as the caption
							summaryCountDataSet.Tables[outputTableName].Columns.Add(groupByColumn);

							object colorLookupList = null;
							if (incidentPriorities != null)
							{
								colorLookupList = incidentPriorities;
							}
							if (incidentSeverities != null)
							{
								colorLookupList = incidentSeverities;
							}
							if (requirementImportances != null)
							{
								colorLookupList = requirementImportances;
							}
							if (testCasePriorities != null)
							{
								colorLookupList = testCasePriorities;
							}
							if (taskPriorities != null)
							{
								colorLookupList = taskPriorities;
							}

							//See if we have a special column that we need to assign a specific color to
							SetKnownColors(groupByColumn, artifactType, groupByFieldName, groupById, colorLookupList);

							//Create the appropriate group-by clause
							string groupByPrefix = "PNT";

							//See if we have a secondary table to use
							//Only for test case execution status by release currently
							if (!String.IsNullOrEmpty(secondaryTable) && groupBySourceColumn == "EXECUTION_STATUS_ID")
							{
								groupByPrefix = "SEC";
							}

							string whereClause;
							if (groupById.HasValue)
							{
								//Need to see if we have a multi-select or single-select
								if (groupByMultiSelect)
								{
									whereClause = "dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(" + groupByPrefix + "." + groupBySourceColumn + ", " + groupById + ") = 1 ";
								}
								else
								{
									whereClause = groupByPrefix + "." + groupBySourceColumn + " = " + groupById + " ";
								}
							}
							else
							{
								whereClause = groupByPrefix + "." + groupBySourceColumn + " IS NULL ";
							}
							whereClause += releaseFilter;

							//See if we have a known Multi-valued standard field for the x-axis
							string sql;
							if (xAxisFieldName == "ComponentIds")
							{
								sql =
									"SELECT		CMP.NAME AS XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
									"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
									"           (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType + ") ACP " +
									"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID INNER JOIN TST_COMPONENT CMP " +
									"ON         dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(PNT." + xAxisSourceColumn + ", CMP.COMPONENT_ID) = 1 " +
									"WHERE		PNT.PROJECT_ID = " + projectId + " " +
									"AND	    " + whereClause +
									"AND        PNT.IS_DELETED = 0 " +
									"GROUP BY	CMP.NAME";
							}
							else
							{
								sql =
									"SELECT		" + xAxisLookupSourceColumn + " AS XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
									"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
									"           (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType + ") ACP " +
									"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID " +
									"WHERE		PNT.PROJECT_ID = " + projectId + " " +
									"AND	    " + whereClause +
									"AND        PNT.IS_DELETED = 0 " +
									"GROUP BY	" + xAxisLookupSourceColumn;
							}

							//Actually execute the query and return the dataset
							using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
							{
								tempDataSet = ExecuteSql(context, sql, "TempCount");
							}

							//Copy across to the final table
							CopyAcrossSummaryRow(summaryCountDataSet, tempDataSet, groupById, outputTableName);
						}
					}
					else
					{
						//Grouping by custom property
						foreach (KeyValuePair<int, string> kvp in lookupValues)
						{
							//Create a temporary dataset for holding the query for a particular group
							System.Data.DataSet tempDataSet = new System.Data.DataSet();

							int groupById = kvp.Key;
							string groupByName = kvp.Value;

							//Add the group-by name to the master dataset - default value 0
							DataColumn groupByColumn = new DataColumn(groupById.ToString(), typeof(int));
							groupByColumn.DefaultValue = 0;
							groupByColumn.Caption = groupByName; //Put the name as the caption
							summaryCountDataSet.Tables[outputTableName].Columns.Add(groupByColumn);

							//Create the appropriate group-by clause
							string groupByPrefix = "ACP";

							//Since custom property values can be multi-valued we need to use a special comparison form
							string whereClause = "dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(" + groupByPrefix + "." + groupBySourceColumn + ", " + groupById + ") = 1 ";

							string sql;
							if (xAxisFieldName == "ComponentIds")
							{
								sql =
									"SELECT		CMP.NAME AS XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
									"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
									"           (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType + ") ACP " +
									"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID INNER JOIN TST_COMPONENT CMP " +
									"ON         dbo.FN_CUSTOM_PROPERTY_EQUALS_INT_LIST(PNT." + xAxisSourceColumn + ",CMP.COMPONENT_ID) = 1 " +
									"WHERE		PNT.PROJECT_ID = " + projectId + " " +
									"AND	    " + whereClause +
									"AND        PNT.IS_DELETED = 0 " +
									"GROUP BY	CMP.NAME";
							}
							else
							{
								sql =
									"SELECT		" + xAxisLookupSourceColumn + " AS XAxis, COUNT(PNT." + primaryKeyColumn + ") AS GroupByCount " +
									"FROM		" + sourceTable + " PNT " + secondaryTable + " LEFT JOIN " +
									"           (SELECT * FROM TST_ARTIFACT_CUSTOM_PROPERTY WHERE PROJECT_ID = " + projectId + " AND ARTIFACT_TYPE_ID = " + (int)artifactType + ") ACP " +
									"ON         PNT." + primaryKeyColumn + " = ACP.ARTIFACT_ID " +
									"WHERE		PNT.PROJECT_ID = " + projectId + " " +
									"AND	    " + whereClause +
									"AND        PNT.IS_DELETED = 0 " +
									"GROUP BY	" + xAxisLookupSourceColumn;
							}

							//Actually execute the query and return the dataset
							using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
							{
								tempDataSet = ExecuteSql(context, sql, "TempCount");
							}

							//Copy across to the final table
							CopyAcrossSummaryRow(summaryCountDataSet, tempDataSet, groupById, outputTableName);
						}
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return summaryCountDataSet;
		}

		/// <summary>
		/// Copies across the values from the temp dataset to the main dataset
		/// </summary>
		/// <param name="groupById">The id we're grouping by</param>
		/// <param name="outputTableName">The output table name</param>
		/// <param name="summaryCountDataSet">The final summary count dataset</param>
		/// <param name="tempDataSet">The temporary dataset containing one group of values</param>
		private void CopyAcrossSummaryRow(DataSet summaryCountDataSet, DataSet tempDataSet, int? groupById, string outputTableName)
		{
			//Now iterate through this and copy across the results to the master dataset
			for (int j = 0; j < tempDataSet.Tables["TempCount"].Rows.Count; j++)
			{
				string xAxis;
				if (tempDataSet.Tables["TempCount"].Rows[j]["XAxis"] == DBNull.Value)
				{
					xAxis = GlobalResources.General.Global_None;
				}
				else
				{
					xAxis = (string)tempDataSet.Tables["TempCount"].Rows[j]["XAxis"];
				}
				int groupCount = (int)tempDataSet.Tables["TempCount"].Rows[j]["GroupByCount"];

				//Locate the matching row in the master dataset - create if not already there
				DataRow matchingRow = summaryCountDataSet.Tables[outputTableName].Rows.Find(xAxis);
				if (matchingRow == null)
				{
					matchingRow = summaryCountDataSet.Tables[outputTableName].NewRow();
					matchingRow["XAxis"] = xAxis;
					summaryCountDataSet.Tables[outputTableName].Rows.Add(matchingRow);
				}
				matchingRow[(groupById.HasValue) ? groupById.Value.ToString() : "0"] = groupCount;
			}
		}

		/// <summary>
		/// Retrieves a count of the number of incidents opened and closed for a specific date range
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dateRange">The date-range (in local time)</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass null for all)</param>
		/// <param name="incidentReportingInterval">Whether to use a weekly or daily time interval</param>
		/// <param name="includeDeleted">Whether to include deleted Incidents or not.</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <returns>The graph data</returns>
		/// <remarks>
		/// The purpose of the graph is to see what's open and closed in the same release,
		/// so we use the Detected by Release for both the opened and closed count
		/// </remarks>
		public DataSet RetrieveIncidentProgress(int projectId, Graph.ReportingIntervalEnum incidentReportingInterval, DateRange dateRange, double utcOffset, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentProgress";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet discoveryCountDataSet = new System.Data.DataSet();
			System.Data.DataSet closedCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//SELECT statement for the count of discovered incidents                
				string sql =
					"SELECT	CREATION_DATE As Date, COUNT(INCIDENT_ID) AS DiscoveredCount " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE, PROJECT_ID, INCIDENT_ID FROM TST_INCIDENT WHERE " +
					((includeDeleted) ? "1=1 " : "IS_DELETED = 0 ") + //Only undeleted incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CREATION_DATE >= " + CultureInvariantDateTime(startDate) + " " +
					"AND	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CREATION_DATE " +
					"ORDER BY CREATION_DATE DESC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					discoveryCountDataSet = ExecuteSql(context, sql, "DiscoveryCount");
				}

				//SELECT statement for the count of closed incidents
				sql =
					"SELECT	CLOSED_DATE As Date, COUNT(INCIDENT_ID) AS ClosedCount " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CLOSED_DATE)) AS FLOAT))AS DATETIME) AS CLOSED_DATE, PROJECT_ID, INCIDENT_ID FROM VW_INCIDENT_LIST WHERE INCIDENT_STATUS_IS_OPEN_STATUS = 0 AND CLOSED_DATE IS NOT NULL " +
					((includeDeleted) ? "1=1 " : "AND IS_DELETED = 0 ") + //Only undeleted incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CLOSED_DATE >= " + CultureInvariantDateTime(startDate) + " " +
					"AND	CLOSED_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CLOSED_DATE " +
					"ORDER BY CLOSED_DATE DESC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					closedCountDataSet = ExecuteSql(context, sql, "ClosedCount");
				}

				//Now we need to create an empty dataset containing just the empty date-range
				incidentCountDataSet.Tables.Add("IncidentCount");
				DataColumn column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("Date", typeof(DateTime));
				column.Caption = GlobalResources.General.Graph_Date;
				column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("DiscoveredCount", typeof(int));
				column.Caption = GlobalResources.General.Graph_DiscoveredCount;
				column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("ClosedCount", typeof(int));
				column.Caption = GlobalResources.General.Graph_ClosedCount;
				//Make Date the primary key of all three data-tables
				discoveryCountDataSet.Tables["DiscoveryCount"].PrimaryKey = new DataColumn[] { discoveryCountDataSet.Tables["DiscoveryCount"].Columns["Date"] };
				closedCountDataSet.Tables["ClosedCount"].PrimaryKey = new DataColumn[] { closedCountDataSet.Tables["ClosedCount"].Columns["Date"] };
				incidentCountDataSet.Tables["IncidentCount"].PrimaryKey = new DataColumn[] { incidentCountDataSet.Tables["IncidentCount"].Columns["Date"] };

				//Now loop through the dataset and populate with the appropriate day range and map in the discovered and closed counts

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				//Now set the running date to the start date
				DateTime runningDate = startDate;

				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = incidentCountDataSet.Tables["IncidentCount"].NewRow();
					dataRow["Date"] = runningDate;
					incidentCountDataSet.Tables["IncidentCount"].Rows.Add(dataRow);

					//Now see if we have discovery or closed counts for this interval
					int discoveredTotal = 0;
					int closedTotal = 0;
					for (int j = 0; j < dateInterval; j++)
					{
						//Discovered Incidents
						System.Data.DataRow foundDataRow = discoveryCountDataSet.Tables["DiscoveryCount"].Rows.Find(runningDate.AddDays(-j));
						if (foundDataRow != null)
						{
							discoveredTotal += (int)foundDataRow["DiscoveredCount"];
						}
						//Closed Incidents
						foundDataRow = closedCountDataSet.Tables["ClosedCount"].Rows.Find(runningDate.AddDays(-j));
						if (foundDataRow != null)
						{
							closedTotal += (int)foundDataRow["ClosedCount"];
						}
					}
					dataRow["DiscoveredCount"] = discoveredTotal;
					dataRow["ClosedCount"] = closedTotal;

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a cumulative count of the number of incidents by status for a specific date range</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="endDate">The ending date for the data-range</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass Null for all)</param>
		/// <param name="incidentReportingInterval">Whether to use a weekly or daily time interval</param>
		/// <param name="includeDeleted">Whether to include deleted incidents or not.</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <param name="dateRange">The date range</param>
		/// <param name="projectTemplateId">The project template we're using</param>
		/// <returns>Returns the last 60 days of data from the latest incident created</returns>
		/// <remarks>
		/// We use the detected release
		/// </remarks>
		public DataSet RetrieveIncidentCountByStatus(int projectId, int projectTemplateId, Graph.ReportingIntervalEnum incidentReportingInterval, DateRange dateRange, double utcOffset, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentCountByStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet discoveryCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//SELECT statement for the count of discovered incidents
				string sql =
					"SELECT	CREATION_DATE As Date, COUNT(INCIDENT_ID) AS DiscoveredCount, INCIDENT_STATUS_ID As IncidentStatusId " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE, PROJECT_ID, INCIDENT_ID, INCIDENT_STATUS_ID FROM TST_INCIDENT WHERE 1=1 " +
					((includeDeleted) ? "" : " AND IS_DELETED = 0 ") +  //Only undeleted Incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CREATION_DATE, INCIDENT_STATUS_ID " +
					"ORDER BY CREATION_DATE DESC, INCIDENT_STATUS_ID ASC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					discoveryCountDataSet = ExecuteSql(context, sql, "DiscoveryCount");
				}

				//Get the incident status lookup
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentStatus> incidentStati = incidentManager.IncidentStatus_Retrieve(projectTemplateId, true);

				//Now we need to create an empty dataset containing just the empty date-range
				incidentCountDataSet.Tables.Add("IncidentCount");
				DataColumn column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("Date", typeof(DateTime));
				column.Caption = GlobalResources.General.Graph_Date;
				//Add a column for each incident priority id (the name of the column is the ID)
				for (int i = 0; i < incidentStati.Count; i++)
				{
					column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add(incidentStati[i].IncidentStatusId.ToString(), typeof(int));
					column.Caption = incidentStati[i].Name;
				}

				//Make Date the primary key of all data-tables and also priority the primary key of two of them
				discoveryCountDataSet.Tables["DiscoveryCount"].PrimaryKey = new DataColumn[] {
					discoveryCountDataSet.Tables["DiscoveryCount"].Columns["Date"],
					discoveryCountDataSet.Tables["DiscoveryCount"].Columns["IncidentStatusId"]
				};
				incidentCountDataSet.Tables["IncidentCount"].PrimaryKey = new DataColumn[] {
					incidentCountDataSet.Tables["IncidentCount"].Columns["Date"]
				};

				//Now loop through the dataset and populate with the appropriate day range and map in the
				//current cumulative creation count for that date interval

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				//Now reset the running date to the earliest date
				DateTime runningDate = startDate;

				//We need to store the running number of open incidents at a given time (by priority/importance)
				int[] openCount = new int[incidentStati.Count + 1];

				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = incidentCountDataSet.Tables["IncidentCount"].NewRow();
					dataRow["Date"] = runningDate;
					incidentCountDataSet.Tables["IncidentCount"].Rows.Add(dataRow);

					//Now see if we have discovery counts for this interval
					//And add and subtract them from the running totals
					//If this is the first date entry on the x-axis, need to also include the values
					//for any dates PRIOR to this as well (this is a special case)
					int k;
					if (i == 0)
					{
						//We need to look at them for each status
						for (k = 0; k < incidentStati.Count; k++)
						{
							int incidentStatusId = incidentStati[k].IncidentStatusId;
							//Find the items before the first date on the graph and add to the initial counts
							foreach (DataRow testDataRow in discoveryCountDataSet.Tables["DiscoveryCount"].Rows)
							{
								if (((DateTime)testDataRow["Date"]).Date < runningDate.Date && (int)testDataRow["IncidentStatusId"] == incidentStatusId)
								{
									openCount[k] += (int)testDataRow["DiscoveredCount"];
								}
							}
						}
					}
					System.Data.DataRow foundDataRow;
					for (int j = 0; j < dateInterval; j++)
					{
						//We need to look at them for each status
						for (k = 0; k < incidentStati.Count; k++)
						{
							int incidentStatusId = incidentStati[k].IncidentStatusId;
							//Discovered Incidents
							foundDataRow = discoveryCountDataSet.Tables["DiscoveryCount"].Rows.Find(new object[] { runningDate.AddDays(-j), incidentStatusId.ToString() });
							if (foundDataRow != null)
							{
								openCount[k] += (int)foundDataRow["DiscoveredCount"];
							}
						}
					}

					//Now update the datarow for the interval
					for (k = 0; k < incidentStati.Count; k++)
					{
						int priorityId = incidentStati[k].IncidentStatusId;
						dataRow[priorityId.ToString()] = openCount[k];
					}

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a cumulative count of the number of incidents open and total for a specific date range</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="endDate">The ending date for the data-range</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass NullParamemeter for all)</param>
		/// <param name="incidentReportingInterval">Whether to use a weekly or daily time interval</param>
		/// <param name="includeDeleted">Whether to include deleted incidents or not.</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <returns>Returns the last 60 days of data from the latest incident created</returns>
		/// <remarks>
		/// We use the detected release
		/// </remarks>
		public System.Data.DataSet RetrieveIncidentCumulativeCount(int projectId, Graph.ReportingIntervalEnum incidentReportingInterval, DateRange dateRange, double utcOffset, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentCumulativeCount";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet openCountDataSet = new System.Data.DataSet();
			System.Data.DataSet totalCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//SELECT statement for the count of open incidents
				string sql =
					"SELECT	CREATION_DATE As Date, COUNT(INCIDENT_ID) AS OpenCount " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE, PROJECT_ID, INCIDENT_ID FROM VW_INCIDENT_LIST WHERE INCIDENT_STATUS_IS_OPEN_STATUS = 1 " +
					((includeDeleted) ? "1=1 " : "AND IS_DELETED = 0 ") + //Only undeleted incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CREATION_DATE " +
					"ORDER BY CREATION_DATE DESC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					openCountDataSet = ExecuteSql(context, sql, "OpenCount");
				}

				//SELECT statement for the total count of incidents
				sql =
					"SELECT	CREATION_DATE As Date, COUNT(INCIDENT_ID) AS TotalCount " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE, PROJECT_ID, INCIDENT_ID FROM TST_INCIDENT WHERE " +
					((includeDeleted) ? "1=1 " : "IS_DELETED = 0 ") + //Only undeleted incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CREATION_DATE " +
					"ORDER BY CREATION_DATE DESC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					totalCountDataSet = ExecuteSql(context, sql, "TotalCount");
				}

				//Now we need to create an empty dataset containing just the empty date-range
				incidentCountDataSet.Tables.Add("IncidentCount");
				DataColumn column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("Date", typeof(DateTime));
				column.Caption = GlobalResources.General.Graph_Date;
				column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("OpenCount", typeof(int));
				column.Caption = GlobalResources.General.Graph_OpenCount;
				column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("TotalCount", typeof(int));
				column.Caption = GlobalResources.General.Graph_TotalCount;
				//Make Date the primary key of all three data-tables
				openCountDataSet.Tables["OpenCount"].PrimaryKey = new DataColumn[] { openCountDataSet.Tables["OpenCount"].Columns["Date"] };
				totalCountDataSet.Tables["TotalCount"].PrimaryKey = new DataColumn[] { totalCountDataSet.Tables["TotalCount"].Columns["Date"] };
				incidentCountDataSet.Tables["IncidentCount"].PrimaryKey = new DataColumn[] { incidentCountDataSet.Tables["IncidentCount"].Columns["Date"] };

				//Now loop through the dataset and populate with the appropriate day range and map in the discovered and closed counts

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				//Now set the running date to the start date
				DateTime runningDate = startDate;

				int openCumulativeTotal = 0;
				int totalCumulativeTotal = 0;
				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = incidentCountDataSet.Tables["IncidentCount"].NewRow();
					dataRow["Date"] = runningDate;
					incidentCountDataSet.Tables["IncidentCount"].Rows.Add(dataRow);

					//Now see if we have open or total counts for this interval
					//If this is the first date entry on the x-axis, need to also include the values
					//for any dates PRIOR to this as well (this is a special case)
					int openTotal = 0;
					int totalTotal = 0;
					if (i == 0)
					{
						//Find the items before the first date on the graph and add to the initial counts
						foreach (DataRow testDataRow in openCountDataSet.Tables["OpenCount"].Rows)
						{
							if (((DateTime)testDataRow["Date"]).Date < runningDate.Date)
							{
								openTotal += (int)testDataRow["OpenCount"];
							}
						}
						foreach (DataRow testDataRow in totalCountDataSet.Tables["TotalCount"].Rows)
						{
							if (((DateTime)testDataRow["Date"]).Date < runningDate.Date)
							{
								totalTotal += (int)testDataRow["TotalCount"];
							}
						}
					}
					for (int j = 0; j < dateInterval; j++)
					{
						//Open Incidents
						System.Data.DataRow foundDataRow = openCountDataSet.Tables["OpenCount"].Rows.Find(runningDate.AddDays(-j));
						if (foundDataRow != null)
						{
							openTotal += (int)foundDataRow["OpenCount"];
						}
						//Closed Incidents
						foundDataRow = totalCountDataSet.Tables["TotalCount"].Rows.Find(runningDate.AddDays(-j));
						if (foundDataRow != null)
						{
							totalTotal += (int)foundDataRow["TotalCount"];
						}
					}

					//Add these interval totals to the cumulative running total
					openCumulativeTotal += openTotal;
					totalCumulativeTotal += totalTotal;
					dataRow["OpenCount"] = openCumulativeTotal;
					dataRow["TotalCount"] = totalCumulativeTotal;

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the incident aging count by priority</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The detected release (null = all)</param>
		/// <param name="projectTemplateId">The project template we're using</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass null for all)</param>
		/// <returns>The requested aging data</returns>
		public System.Data.DataSet RetrieveIncidentAgingByPriority(int projectId, int projectTemplateId, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentAgingByPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet openCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentAgingDataSet = new System.Data.DataSet();

			try
			{
				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//SELECT statement for the count of open incidents (filtered by type)
				string sql =
					"SELECT	Age, COUNT(INCIDENT_ID) AS OpenCount, PRIORITY_ID As PriorityId " +
					"FROM	(SELECT DATEDIFF(Day, CREATION_DATE, GETDATE()) As Age, CLOSED_DATE, PROJECT_ID, INCIDENT_ID, ISNULL(PRIORITY_ID, 0) AS PRIORITY_ID FROM TST_INCIDENT WHERE " +
					((includeDeleted) ? "" : " IS_DELETED = 0") + //Only undeleted Incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CLOSED_DATE IS NULL " +
					"GROUP BY Age, PRIORITY_ID " +
					"ORDER BY Age ASC, PriorityId ASC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					openCountDataSet = ExecuteSql(context, sql, "OpenCount");
				}

				//Store the maximum age value (needed later on) - corresponds to last row
				int maxAge = 91;
				if (openCountDataSet.Tables["OpenCount"].Rows.Count > 0)
				{
					if ((int)openCountDataSet.Tables["OpenCount"].Rows[openCountDataSet.Tables["OpenCount"].Rows.Count - 1]["Age"] > 91)
					{
						maxAge = (int)openCountDataSet.Tables["OpenCount"].Rows[openCountDataSet.Tables["OpenCount"].Rows.Count - 1]["Age"];
					}
				}

				//Get the incident priority lookup
				List<IncidentPriority> incidentPriorities = new IncidentManager().RetrieveIncidentPriorities(projectTemplateId, true);

				//Now we need to create an empty dataset containing the # days aged and a count column per priority
				incidentAgingDataSet.Tables.Add("IncidentAging");
				DataColumn column = incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("Age", typeof(string));
				column.Caption = GlobalResources.General.Graph_Age;
				//Add a column for each incident priority id (the name of the column is the ID)
				for (int i = 0; i < incidentPriorities.Count; i++)
				{
					column = incidentAgingDataSet.Tables["IncidentAging"].Columns.Add(incidentPriorities[i].PriorityId.ToString(), typeof(int));
					column.Caption = incidentPriorities[i].Name;
					column.ExtendedProperties.Add("Color", incidentPriorities[i].Color);
				}
				//And one column for no priority set
				column = incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("0", typeof(int));
				column.Caption = GlobalResources.General.Global_None;
				column.ExtendedProperties.Add("Color", "e0e0e0");

				//Make Age and Priority Id the primary keys
				openCountDataSet.Tables["OpenCount"].PrimaryKey = new DataColumn[] {
					openCountDataSet.Tables["OpenCount"].Columns["Age"],
					openCountDataSet.Tables["OpenCount"].Columns["PriorityId"]
					};
				incidentAgingDataSet.Tables["IncidentAging"].PrimaryKey = new DataColumn[] {
					incidentAgingDataSet.Tables["IncidentAging"].Columns["Age"]
					};

				//Now loop through the dataset and populate with the age-ranges labels
				//and count the number of incidents per priority in each range

				int lowerBound = 0;
				int upperBound = 7;

				//Loop until we have a lower bound of 90 days
				while (lowerBound <= 91)
				{
					//Populate the age value label (special case for greater than 90 days)
					System.Data.DataRow dataRow = incidentAgingDataSet.Tables["IncidentAging"].NewRow();
					if (lowerBound < 91)
					{
						dataRow["Age"] = lowerBound.ToString() + "-" + upperBound.ToString();
					}
					else
					{
						dataRow["Age"] = "> 90";
						upperBound = maxAge; //Catch anything bigger than 90
					}
					incidentAgingDataSet.Tables["IncidentAging"].Rows.Add(dataRow);

					//Now find all incidents that lie within this aging interval
					int[] ageTotal = new int[incidentPriorities.Count + 1];
					for (int j = lowerBound; j <= upperBound; j++)
					{
						//We need to look at them for each priority
						for (int k = 0; k < incidentPriorities.Count; k++)
						{
							int priorityId = incidentPriorities[k].PriorityId;
							//Get the row that matches this age and priority/importance
							System.Data.DataRow foundDataRow = openCountDataSet.Tables["OpenCount"].Rows.Find(new object[] { j, priorityId.ToString() });
							if (foundDataRow != null)
							{
								ageTotal[k] += (int)foundDataRow["OpenCount"];
							}
						}

						//Handle the null priority case
						System.Data.DataRow foundDataRow2 = openCountDataSet.Tables["OpenCount"].Rows.Find(new object[] { j, "0" });
						if (foundDataRow2 != null)
						{
							ageTotal[incidentPriorities.Count] += (int)foundDataRow2["OpenCount"];
						}
					}

					//Now update the datarow for the interval
					for (int k = 0; k < incidentPriorities.Count; k++)
					{
						int priorityId = incidentPriorities[k].PriorityId;
						dataRow[priorityId.ToString()] = ageTotal[k];
					}
					//Handle the null priority case
					dataRow["0"] = ageTotal[incidentPriorities.Count];

					//Move to the next aging interval
					lowerBound = upperBound + 1;
					upperBound += 7;

					//(special case for 84-90)
					if (lowerBound == 85)
					{
						lowerBound = 84;
						upperBound = 90;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentAgingDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the incident closure turnaround time (in days) by priority</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The detected release (null = all)</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass null for all)</param>
		/// <param name="projectTemplateId">The project template we're using</param>
		/// <returns>The requested aging data</returns>
		public System.Data.DataSet RetrieveIncidentTurnaroundByPriority(int projectId, int projectTemplateId, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentTurnaroundByPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet closedCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentTurnaroundDataSet = new System.Data.DataSet();

			try
			{
				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//SELECT statement for the count of open incidents (filtered by type)
				string sql =
					"SELECT	Turnaround, COUNT(INCIDENT_ID) AS ClosedCount, ISNULL(PRIORITY_ID, 0) As PriorityId " +
					"FROM	(SELECT DATEDIFF(Day, CREATION_DATE, CLOSED_DATE) As Turnaround, CLOSED_DATE, PROJECT_ID, INCIDENT_ID, ISNULL(PRIORITY_ID, 0) AS PRIORITY_ID FROM TST_INCIDENT WHERE " +
					((includeDeleted) ? "1=1 " : " IS_DELETED = 0 ") +
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CLOSED_DATE IS NOT NULL " +
					"GROUP BY Turnaround, PRIORITY_ID " +
					"ORDER BY Turnaround ASC, PriorityId ASC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					closedCountDataSet = ExecuteSql(context, sql, "ClosedCount");
				}

				//Store the maximum age value (needed later on) - corresponds to last row
				int maxAge = 91;
				if (closedCountDataSet.Tables["ClosedCount"].Rows.Count > 0)
				{
					if ((int)closedCountDataSet.Tables["ClosedCount"].Rows[closedCountDataSet.Tables["ClosedCount"].Rows.Count - 1]["Turnaround"] > 91)
					{
						maxAge = (int)closedCountDataSet.Tables["ClosedCount"].Rows[closedCountDataSet.Tables["ClosedCount"].Rows.Count - 1]["Turnaround"];
					}
				}

				//Get the incident priority lookup
				List<IncidentPriority> incidentPriorities = new IncidentManager().RetrieveIncidentPriorities(projectTemplateId, true);

				//Now we need to create an empty dataset containing the # days aged and a count column per priority
				incidentTurnaroundDataSet.Tables.Add("IncidentTurnaround");
				DataColumn column = incidentTurnaroundDataSet.Tables["IncidentTurnaround"].Columns.Add("Turnaround", typeof(string));
				column.Caption = GlobalResources.General.Graph_Turnaround;
				//Add a column for each priority id (the name of the column is the ID)
				for (int i = 0; i < incidentPriorities.Count; i++)
				{
					column = incidentTurnaroundDataSet.Tables["IncidentTurnaround"].Columns.Add(incidentPriorities[i].PriorityId.ToString(), typeof(int));
					column.Caption = incidentPriorities[i].Name;
					column.ExtendedProperties.Add("Color", incidentPriorities[i].Color);
				}
				//And one column for no priority set
				column = incidentTurnaroundDataSet.Tables["IncidentTurnaround"].Columns.Add("0", typeof(int));
				column.Caption = GlobalResources.General.Global_None;
				column.ExtendedProperties.Add("Color", "e0e0e0");

				//Make Age and Priority Id the primary keys
				closedCountDataSet.Tables["ClosedCount"].PrimaryKey = new DataColumn[] {
					closedCountDataSet.Tables["ClosedCount"].Columns["Turnaround"],
					closedCountDataSet.Tables["ClosedCount"].Columns["PriorityId"]
					};
				incidentTurnaroundDataSet.Tables["IncidentTurnaround"].PrimaryKey = new DataColumn[] {
					incidentTurnaroundDataSet.Tables["IncidentTurnaround"].Columns["Turnaround"]
					};

				//Now loop through the dataset and populate with the age-ranges labels
				//and count the number of incidents per priority in each range

				int lowerBound = 0;
				int upperBound = 7;

				//Loop until we have a lower bound of 90 days
				while (lowerBound <= 91)
				{
					//Populate the age value label (special case for greater than 90 days)
					System.Data.DataRow dataRow = incidentTurnaroundDataSet.Tables["IncidentTurnaround"].NewRow();
					if (lowerBound < 91)
					{
						dataRow["Turnaround"] = lowerBound.ToString() + "-" + upperBound.ToString();
					}
					else
					{
						dataRow["Turnaround"] = "> 90";
						upperBound = maxAge; //Catch anything bigger than 90
					}
					incidentTurnaroundDataSet.Tables["IncidentTurnaround"].Rows.Add(dataRow);

					//Now find all incidents that lie within this aging interval
					int[] ageTotal = new int[incidentPriorities.Count + 1];
					for (int j = lowerBound; j <= upperBound; j++)
					{
						//We need to look at them for each priority
						for (int k = 0; k < incidentPriorities.Count; k++)
						{
							int priorityId = incidentPriorities[k].PriorityId;
							//Get the row that matches this age and priority/importance
							System.Data.DataRow foundDataRow = closedCountDataSet.Tables["ClosedCount"].Rows.Find(new object[] { j, priorityId.ToString() });
							if (foundDataRow != null)
							{
								ageTotal[k] += (int)foundDataRow["ClosedCount"];
							}
						}
						//Handle the null priority case
						System.Data.DataRow foundDataRow2 = closedCountDataSet.Tables["ClosedCount"].Rows.Find(new object[] { j, "0" });
						if (foundDataRow2 != null)
						{
							ageTotal[incidentPriorities.Count] += (int)foundDataRow2["ClosedCount"];
						}
					}

					//Now update the datarow for the interval
					for (int k = 0; k < incidentPriorities.Count; k++)
					{
						int priorityId = incidentPriorities[k].PriorityId;
						dataRow[priorityId.ToString()] = ageTotal[k];
					}
					//Handle the null priority case
					dataRow["0"] = ageTotal[incidentPriorities.Count];

					//Move to the next aging interval
					lowerBound = upperBound + 1;
					upperBound += 7;

					//(special case for 84-90)
					if (lowerBound == 85)
					{
						lowerBound = 84;
						upperBound = 90;
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return (incidentTurnaroundDataSet);
		}

		#region Task Graphs

		/// <summary>Retrieves the data used to populate the velocity graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in (optional)</param>
		/// <returns>Dataset of effort against the x-axis</returns>
		/// <remarks>1) If release id is NullParameter then we display effort against major releases
		/// 2) If release id is set to a release then we display effort against iterations
		/// 3) If release id is set to an iteration then we display effort against days in the iteration interval</remarks>
		public DataSet RetrieveTaskVelocity(int projectId, Nullable<int> releaseId = null)
		{
			const string METHOD_NAME = "RetrieveTaskVelocity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet velocityDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				velocityDataSet.Tables.Add("Velocity");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = velocityDataSet.Tables["Velocity"].Columns.Add("XAxis", typeof(string));
				velocityDataSet.Tables["Velocity"].PrimaryKey = primaryKeys;
				DataColumn dataColumn = velocityDataSet.Tables["Velocity"].Columns.Add("ExpectedVelocity", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_ExpectedVelocity;
				dataColumn.ExtendedProperties.Add("Color", "bbebfe");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);
				dataColumn = velocityDataSet.Tables["Velocity"].Columns.Add("ActualVelocity", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_ActualVelocity;
				dataColumn.ExtendedProperties.Add("Color", "f47457");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);

				//Now see if we have a release or not
				if (!releaseId.HasValue)
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					//Copy across the velocity information
					foreach (ReleaseView releaseRow in releases)
					{
						DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
						velocityRow["XAxis"] = releaseRow.VersionNumber;
						//Convert into hours (from minutes)
						velocityRow["ExpectedVelocity"] = (decimal)releaseRow.PlannedEffort / (decimal)60;
						if (releaseRow.TaskEstimatedEffort.HasValue)
						{
							velocityRow["ActualVelocity"] = (decimal)releaseRow.TaskEstimatedEffort.Value / (decimal)60;
						}
						else
						{
							velocityRow["ActualVelocity"] = 0;
						}
						velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
					}
					xAxisColumn.Caption = GlobalResources.General.Graph_Release;
				}
				else
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//First we need populate the list of working days between the iteration start/end date
						DateTime startDate = releaseOrIteration.StartDate;
						DateTime endDate = releaseOrIteration.EndDate;
						for (DateTime runningDate = startDate; runningDate <= endDate; runningDate = runningDate.AddDays(1))
						{
							//Only consider working days
							if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
							{
								DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
								velocityRow["XAxis"] = runningDate.ToShortDateString();
								velocityRow["ExpectedVelocity"] = 0M;
								velocityRow["ActualVelocity"] = 0M;
								velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
							}
						}
						int dayCount = velocityDataSet.Tables["Velocity"].Rows.Count;

						//Now we can populate the expected velocity, which is just the release planned effort divided by the # days
						decimal plannedVelocity = (decimal)releaseOrIteration.PlannedEffort / (decimal)dayCount / 60M;  //Converted into hours
						foreach (DataRow velocityRow in velocityDataSet.Tables["Velocity"].Rows)
						{
							velocityRow["ExpectedVelocity"] = plannedVelocity;
						}

						//We now need to actually get the list of tasks in this iteration since we'll be calculating
						//effort by date-range. Iterate through and ...
						List<TaskView> tasks = new TaskManager().RetrieveByReleaseId(projectId, releaseId.Value);
						foreach (TaskView task in tasks)
						{
							//For each task that has an estimated effort we need to calculate the number of working
							//days it spans and then attribute the percentage of effort to each day in the range
							if (task.EstimatedEffort.HasValue && task.StartDate.HasValue && task.EndDate.HasValue)
							{
								int dayRange = (int)ReleaseManager.WorkingDays(task.StartDate.Value, task.EndDate.Value, project.WorkingDays);
								if (dayRange > 0)
								{
									decimal actualVelocityPerDay = (decimal)task.EstimatedEffort / (decimal)dayRange / 60M;   //Convert to hours

									//Now iterate over each day and add it to the running total held in the velocity dataset
									for (DateTime runningDate = task.StartDate.Value; runningDate <= task.EndDate.Value; runningDate = runningDate.AddDays(1))
									{
										//Only consider working days
										if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
										{
											DataRow matchedRow = velocityDataSet.Tables["Velocity"].Rows.Find(runningDate.ToShortDateString());
											if (matchedRow != null)
											{
												matchedRow["ActualVelocity"] = (decimal)matchedRow["ActualVelocity"] + actualVelocityPerDay;
											}
										}
									}
								}
							}
						}

						xAxisColumn.Caption = GlobalResources.General.Graph_Date;
					}
					else
					{
						//Get the list of iterations in this release
						List<ReleaseView> releases = releaseManager.RetrieveSelfAndIterations(projectId, releaseId.Value, false);

						//Copy across the velocity information
						foreach (ReleaseView releaseRow in releases)
						{
							//We need to ignore the initial release row
							if (releaseRow.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
							{
								DataRow velocityRow = velocityDataSet.Tables["Velocity"].NewRow();
								velocityRow["XAxis"] = releaseRow.VersionNumber;
								//Convert into hours (from minutes)
								velocityRow["ExpectedVelocity"] = (decimal)releaseRow.PlannedEffort / 60M;
								if (releaseRow.TaskEstimatedEffort.HasValue)
								{
									velocityRow["ActualVelocity"] = (decimal)releaseRow.TaskEstimatedEffort.Value / 60M;
								}
								else
								{
									velocityRow["ActualVelocity"] = 0M;
								}
								velocityDataSet.Tables["Velocity"].Rows.Add(velocityRow);
							}
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return velocityDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the data used to populate the burnup graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in</param>
		/// <returns>Dataset of effort against the x-axis</returns>
		/// <remarks>1) If release id is NullParameter then we display effort against major releases
		/// 2) If release id is set to a release then we display effort against iterations
		/// 3) If release id is set to an iteration then we display effort against days in the iteration interval</remarks>
		public DataSet RetrieveTaskBurnup(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveTaskBurnup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet burnupDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				burnupDataSet.Tables.Add("Burnup");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = burnupDataSet.Tables["Burnup"].Columns.Add("XAxis", typeof(string));
				burnupDataSet.Tables["Burnup"].PrimaryKey = primaryKeys;
				DataColumn dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("CompletedEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_CompletedEffort;
				dataColumn.ExtendedProperties.Add("Color", "7eff7a");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);
				dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("RemainingEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_RemainingEffort;
				dataColumn.ExtendedProperties.Add("Color", "f4f356");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);
				dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("IdealEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_IdealEffort;
				dataColumn.ExtendedProperties.Add("Color", "bbebfe");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);
				dataColumn = burnupDataSet.Tables["Burnup"].Columns.Add("EstimatedEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_EstimatedEffort;
				dataColumn.ExtendedProperties.Add("Color", "f47457");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);

				//Now see if we have a release or not
				if (releaseId.HasValue)
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//Calculate the burnup information. We start by considering the total estimated effort for the
						//iteration and then we subtract the effort apportioned to each day

						//We need a temporary column to hold the apportioned efforts per day
						burnupDataSet.Tables["Burnup"].Columns.Add("ApportionedEstimatedEffort", typeof(decimal));

						//Add an initial starting row
						DataRow burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
						burnupRow["XAxis"] = GlobalResources.General.Graph_Start;
						burnupRow["CompletedEffort"] = 0;
						burnupRow["RemainingEffort"] = 0;
						burnupRow["EstimatedEffort"] = 0;
						burnupRow["IdealEffort"] = 0;
						burnupRow["ApportionedEstimatedEffort"] = 0;
						burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);

						//First we need populate the list of working days between the iteration start/end date
						DateTime startDate = releaseOrIteration.StartDate;
						DateTime endDate = releaseOrIteration.EndDate;
						for (DateTime runningDate = startDate; runningDate <= endDate; runningDate = runningDate.AddDays(1))
						{
							//Only consider working days
							if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
							{
								burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
								burnupRow["XAxis"] = runningDate.ToShortDateString();
								burnupRow["CompletedEffort"] = 0;
								burnupRow["RemainingEffort"] = 0;
								burnupRow["EstimatedEffort"] = 0;
								burnupRow["IdealEffort"] = 0;
								burnupRow["ApportionedEstimatedEffort"] = 0;
								burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
							}
						}
						int dayCount = burnupDataSet.Tables["Burnup"].Rows.Count - 1;

						//We start the burnup with zero effort
						decimal runningEstimatedEffort = 0M;

						//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
						decimal idealEffort = 0M;
						decimal idealEffortSegment = 0M;
						if (releaseOrIteration.TaskEstimatedEffort.HasValue)
						{
							decimal releaseEffort = (decimal)releaseOrIteration.TaskEstimatedEffort.Value;
							idealEffortSegment = releaseEffort / (decimal)dayCount;
						}

						//Now iterate through the tasks and populate their allocation per day
						List<TaskView> tasks = new TaskManager().RetrieveByReleaseId(projectId, releaseId);
						foreach (TaskView task in tasks)
						{
							//For each task that has an estimated effort we need to calculate the number of working
							//days it spans and then attribute the percentage of effort to each day in the range
							if (task.EstimatedEffort.HasValue && task.StartDate.HasValue && task.EndDate.HasValue)
							{
								int dayRange = (int)ReleaseManager.WorkingDays(task.StartDate.Value, task.EndDate.Value, project.WorkingDays);
								if (dayRange > 0)
								{
									decimal actualBurnupPerDay = (decimal)task.EstimatedEffort.Value / (decimal)dayRange;

									//Now iterate over each day and add it to the running total held in the burnup dataset
									for (DateTime runningDate = task.StartDate.Value; runningDate <= task.EndDate.Value; runningDate = runningDate.AddDays(1))
									{
										//Only consider working days
										if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
										{
											DataRow matchedRow = burnupDataSet.Tables["Burnup"].Rows.Find(runningDate.ToShortDateString());
											if (matchedRow != null)
											{
												matchedRow["ApportionedEstimatedEffort"] = (decimal)matchedRow["ApportionedEstimatedEffort"] + actualBurnupPerDay;

												//Also add on the remaining effort (if known)
												if (task.RemainingEffort.HasValue)
												{
													matchedRow["RemainingEffort"] = (decimal)matchedRow["RemainingEffort"] + task.RemainingEffort.Value / dayRange;
												}
											}
										}
									}
								}
							}
						}

						//Now iterate through the days and subtract the effort from each day
						bool startingRow = true;
						foreach (DataRow burnupRow2 in burnupDataSet.Tables["Burnup"].Rows)
						{
							//Add to the ideal effort line and estimated effort cumulative bar
							if (!startingRow)
							{
								idealEffort += idealEffortSegment;
								runningEstimatedEffort += (decimal)burnupRow2["ApportionedEstimatedEffort"];
							}

							decimal remainingEffort = (decimal)burnupRow2["RemainingEffort"];
							burnupRow2["EstimatedEffort"] = runningEstimatedEffort / 60M;
							burnupRow2["IdealEffort"] = idealEffort / 60M;
							burnupRow2["RemainingEffort"] = remainingEffort / 60M;
							burnupRow2["CompletedEffort"] = ((decimal)burnupRow2["ApportionedEstimatedEffort"] - remainingEffort) / 60M;
							startingRow = false;
						}

						//Finally remove the temporary column
						burnupDataSet.Tables["Burnup"].Columns.Remove("ApportionedEstimatedEffort");

						xAxisColumn.Caption = GlobalResources.General.Graph_Date;

					}
					else
					{
						//Get the list of iterations in this release
						List<ReleaseView> releases = releaseManager.RetrieveSelfAndIterations(projectId, releaseId.Value, false);

						//Calculate the burnup information.
						//We use estimated effort not projected effort for burnup so that we can see progress not effort
						decimal runningEstimatedEffort = 0;

						//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
						decimal idealEffortSegment = 0;
						decimal idealEffort = 0;
						if (releaseOrIteration.TaskEstimatedEffort.HasValue)
						{
							decimal releaseEffort = (decimal)releaseOrIteration.TaskEstimatedEffort.Value;
							int iterationCount = releases.Count - 1;

							//Make sure we have at least one iteration (avoid division by zero error)
							if (iterationCount < 1)
							{
								return burnupDataSet;
							}

							idealEffortSegment = releaseEffort / iterationCount;
						}

						foreach (ReleaseView iterationRow in releases)
						{
							//If we are looking at the release row, then we need to handle that case differently
							//since that's the initial estimate with no work completed
							DataRow burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
							if (iterationRow.ReleaseTypeId != (int)Release.ReleaseTypeEnum.Iteration)
							{
								burnupRow["XAxis"] = GlobalResources.General.Graph_Start;
								burnupRow["RemainingEffort"] = 0M;
								burnupRow["CompletedEffort"] = 0M;
								burnupRow["EstimatedEffort"] = 0M;
								burnupRow["IdealEffort"] = 0M;
								burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
							}
							else
							{
								burnupRow["XAxis"] = iterationRow.VersionNumber;

								decimal iterationEstimatedEffort = 0M;
								decimal iterationCompletedEffort = 0M;
								decimal iterationRemainingEffort = 0M;
								if (iterationRow.TaskEstimatedEffort.HasValue)
								{
									iterationEstimatedEffort += iterationRow.TaskEstimatedEffort.Value;
									if (iterationRow.TaskRemainingEffort.HasValue)
									{
										iterationRemainingEffort = iterationRow.TaskRemainingEffort.Value;
										iterationCompletedEffort = iterationEstimatedEffort - iterationRemainingEffort;
										if (iterationCompletedEffort > iterationEstimatedEffort)
										{
											iterationCompletedEffort = iterationEstimatedEffort;
										}
									}
								}

								burnupRow["RemainingEffort"] = iterationRemainingEffort / 60M;
								burnupRow["CompletedEffort"] = iterationCompletedEffort / 60M;

								//Add the current's iteration's estimated effort to the running total
								runningEstimatedEffort += iterationEstimatedEffort;

								//Add to the ideal effort line
								idealEffort += idealEffortSegment;

								//Convert efforts into hours (from minutes) and add to row
								burnupRow["EstimatedEffort"] = runningEstimatedEffort / 60M;
								burnupRow["IdealEffort"] = idealEffort / 60M;
								burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
							}
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
					}
				}
				else
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					//Calculate the burnup information.
					//We use estimated effort not projected effort for burnup so that we can see progress not effort
					decimal runningEstimatedEffort = 0;

					//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
					decimal idealEffortSegment = 0;
					decimal idealEffort = 0;
					int releaseCount = releases.Count;
					//Make sure we have at least one release (avoid division by zero error)
					if (releaseCount < 1)
					{
						return burnupDataSet;
					}
					decimal releaseEffort = 0M;
					foreach (ReleaseView releaseRow in releases)
					{
						if (releaseRow.TaskEstimatedEffort.HasValue)
						{
							releaseEffort += (decimal)releaseRow.TaskEstimatedEffort.Value;
						}
					}
					idealEffortSegment = releaseEffort / releaseCount;

					//First we need to add the 'Start' zero effort position
					DataRow burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
					burnupRow["XAxis"] = GlobalResources.General.Graph_Start;
					burnupRow["RemainingEffort"] = 0M;
					burnupRow["CompletedEffort"] = 0M;
					burnupRow["EstimatedEffort"] = 0M;
					burnupRow["IdealEffort"] = 0M;
					burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);

					//Now we add the releases
					foreach (ReleaseView releaseRow in releases)
					{
						burnupRow = burnupDataSet.Tables["Burnup"].NewRow();
						burnupRow["XAxis"] = releaseRow.VersionNumber;

						decimal iterationEstimatedEffort = 0M;
						decimal iterationCompletedEffort = 0M;
						decimal iterationRemainingEffort = 0M;
						if (releaseRow.TaskEstimatedEffort.HasValue)
						{
							iterationEstimatedEffort += releaseRow.TaskEstimatedEffort.Value;
							if (releaseRow.TaskRemainingEffort.HasValue)
							{
								iterationRemainingEffort = releaseRow.TaskRemainingEffort.Value;
								iterationCompletedEffort = iterationEstimatedEffort - iterationRemainingEffort;
								if (iterationCompletedEffort > iterationEstimatedEffort)
								{
									iterationCompletedEffort = iterationEstimatedEffort;
								}
							}
						}

						burnupRow["RemainingEffort"] = iterationRemainingEffort / 60M;
						burnupRow["CompletedEffort"] = iterationCompletedEffort / 60M;

						//Add the current's iteration's estimated effort to the running total
						runningEstimatedEffort += iterationEstimatedEffort;

						//Add to the ideal effort line
						idealEffort += idealEffortSegment;

						//Convert efforts into hours (from minutes) and add to row
						burnupRow["EstimatedEffort"] = runningEstimatedEffort / 60M;
						burnupRow["IdealEffort"] = idealEffort / 60M;
						burnupDataSet.Tables["Burnup"].Rows.Add(burnupRow);
					}
					xAxisColumn.Caption = GlobalResources.General.Graph_Release;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return burnupDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the data used to populate the burndown graph</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we're interested in</param>
		/// <returns>Dataset of effort against the x-axis</returns>
		/// <remarks>
		/// 1) If no release specified, we display effort against releases
		/// 2) If release id is set to a release then we display effort against iterations
		/// 3) If release id is set to an iteration then we display effort against days in the iteration interval
		/// </remarks>
		public DataSet RetrieveTaskBurndown(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveTaskBurndown";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new untyped dataset
			DataSet burndownDataSet = new DataSet();

			try
			{
				//Make sure we have a valid project id
				if (projectId < 1)
				{
					return null;
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Construct the generic data-table that will hold the results
				burndownDataSet.Tables.Add("Burndown");

				//Add the columns
				DataColumn[] primaryKeys = new DataColumn[1];
				DataColumn xAxisColumn = primaryKeys[0] = burndownDataSet.Tables["Burndown"].Columns.Add("XAxis", typeof(string));
				burndownDataSet.Tables["Burndown"].PrimaryKey = primaryKeys;
				DataColumn dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("CompletedEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_CompletedEffort;
				dataColumn.ExtendedProperties.Add("Color", "39B228");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);


				dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("EstimatedEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_EstimatedEffort;
				dataColumn.ExtendedProperties.Add("Color", "F69419");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);


				dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("RemainingEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_RemainingEffort;
				dataColumn.ExtendedProperties.Add("Color", "F4F356");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Bar);


				dataColumn = burndownDataSet.Tables["Burndown"].Columns.Add("IdealEffort", typeof(decimal));
				dataColumn.Caption = GlobalResources.General.Graphs_IdealEffort;
				dataColumn.ExtendedProperties.Add("Color", "3CA2E2");
				dataColumn.ExtendedProperties.Add("Type", Graph.GraphSeriesTypeEnum.Line);


				//Now see if we have a release or not
				if (releaseId.HasValue)
				{
					//See if we have a release or iteration
					ReleaseManager releaseManager = new ReleaseManager();
					ReleaseView releaseOrIteration = releaseManager.RetrieveById2(projectId, releaseId.Value);
					if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
					{
						//Calculate the burndown information. We start by considering the total estimated effort for the
						//iteration and then we subtract the effort apportioned to each day

						//We need a temporary column to hold the apportioned efforts per day
						burndownDataSet.Tables["Burndown"].Columns.Add("ApportionedEstimatedEffort", typeof(decimal));

						//First we need populate the list of working days between the iteration start/end date
						DateTime startDate = releaseOrIteration.StartDate;
						DateTime endDate = releaseOrIteration.EndDate;
						for (DateTime runningDate = startDate; runningDate <= endDate; runningDate = runningDate.AddDays(1))
						{
							//Only consider working days
							if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
							{
								DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
								burndownRow["XAxis"] = runningDate.ToShortDateString();
								burndownRow["CompletedEffort"] = 0M;
								burndownRow["RemainingEffort"] = 0M;
								burndownRow["EstimatedEffort"] = 0M;
								burndownRow["IdealEffort"] = 0M;
								burndownRow["ApportionedEstimatedEffort"] = 0M;
								burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);
							}
						}
						int dayCount = burndownDataSet.Tables["Burndown"].Rows.Count;

						//Calculate the initial total effort for the iteration
						decimal runningEstimatedEffort = 0M;
						if (releaseOrIteration.TaskEstimatedEffort.HasValue)
						{
							runningEstimatedEffort = (decimal)releaseOrIteration.TaskEstimatedEffort.Value;
						}

						//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
						decimal idealEffort = runningEstimatedEffort;
						decimal idealEffortSegment = idealEffort / (decimal)dayCount;

						//Now iterate through the tasks and populate their allocation per day
						List<TaskView> tasks = new TaskManager().RetrieveByReleaseId(projectId, releaseId);
						foreach (TaskView task in tasks)
						{
							//For each task that has an estimated effort we need to calculate the number of working
							//days it spans and then attribute the percentage of effort to each day in the range
							if (task.EstimatedEffort.HasValue && task.StartDate.HasValue && task.EndDate.HasValue)
							{
								int dayRange = (int)ReleaseManager.WorkingDays(task.StartDate.Value, task.EndDate.Value, project.WorkingDays);
								if (dayRange > 0)
								{
									decimal actualBurndownPerDay = (decimal)task.EstimatedEffort.Value / (decimal)dayRange;

									//Now iterate over each day and add it to the running total held in the burndown dataset
									for (DateTime runningDate = task.StartDate.Value; runningDate <= task.EndDate.Value; runningDate = runningDate.AddDays(1))
									{
										//Only consider working days
										if (runningDate.DayOfWeek != DayOfWeek.Saturday && runningDate.DayOfWeek != DayOfWeek.Sunday)
										{
											DataRow matchedRow = burndownDataSet.Tables["Burndown"].Rows.Find(runningDate.ToShortDateString());
											if (matchedRow != null)
											{
												matchedRow["ApportionedEstimatedEffort"] = (decimal)matchedRow["ApportionedEstimatedEffort"] + actualBurndownPerDay;

												//Also add on the remaining effort (if known)
												if (task.RemainingEffort.HasValue)
												{
													matchedRow["RemainingEffort"] = (decimal)matchedRow["RemainingEffort"] + task.RemainingEffort.Value / dayRange;
												}
											}
										}
									}
								}
							}
						}

						//Now iterate through the days and subtract the effort from each day
						foreach (DataRow burndownRow in burndownDataSet.Tables["Burndown"].Rows)
						{
							decimal remainingEffort = (decimal)burndownRow["RemainingEffort"];
							burndownRow["EstimatedEffort"] = runningEstimatedEffort / 60M;
							burndownRow["IdealEffort"] = idealEffort / 60M;
							burndownRow["RemainingEffort"] = remainingEffort / 60M;
							burndownRow["CompletedEffort"] = ((decimal)burndownRow["ApportionedEstimatedEffort"] - remainingEffort) / 60M;
							runningEstimatedEffort -= (decimal)burndownRow["ApportionedEstimatedEffort"];

							//Subtract from the ideal effort line
							idealEffort -= idealEffortSegment;
						}

						//Finally remove the temporary column
						burndownDataSet.Tables["Burndown"].Columns.Remove("ApportionedEstimatedEffort");

						xAxisColumn.Caption = GlobalResources.General.Graph_Date;

					}
					else
					{
						//Get the list of iterations in this release
						List<ReleaseView> releases = releaseManager.RetrieveSelfAndIterations(projectId, releaseId.Value, false);

						//Calculate the burndown information. We start by considering the total estimated effort for the
						//release and then we subtract the amount completed for each iteration
						//We use estimated effort not projected effort for burndown so that we can see progress not effort
						decimal runningEstimatedEffort = 0M;
						if (releaseOrIteration.TaskEstimatedEffort.HasValue)
						{
							runningEstimatedEffort = (decimal)releaseOrIteration.TaskEstimatedEffort;
						}

						//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
						decimal idealEffort = runningEstimatedEffort;
						int iterationCount = releases.Count;

						//Make sure we have at least one iteration (avoid division by zero error)
						if (iterationCount < 1)
						{
							return burndownDataSet;
						}

						decimal idealEffortSegment = idealEffort / (decimal)iterationCount;
						foreach (ReleaseView iterationRow in releases)
						{
							//We only consider the iterations
							//if (iterationRow.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration)
							//{
							DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
							burndownRow["XAxis"] = iterationRow.VersionNumber;

							decimal iterationEstimatedEffort = 0M;
							decimal iterationCompletedEffort = 0M;
							decimal iterationRemainingEffort = 0M;
							if (iterationRow.TaskEstimatedEffort.HasValue)
							{
								iterationEstimatedEffort += iterationRow.TaskEstimatedEffort.Value;
								if (iterationRow.TaskRemainingEffort.HasValue)
								{
									iterationRemainingEffort = iterationRow.TaskRemainingEffort.Value;
									iterationCompletedEffort = iterationEstimatedEffort - iterationRemainingEffort;
									if (iterationCompletedEffort > iterationEstimatedEffort)
									{
										iterationCompletedEffort = iterationEstimatedEffort;
									}
								}
							}

							//Convert efforts into hours (from minutes) and add to row
							burndownRow["RemainingEffort"] = iterationRemainingEffort / 60M;
							burndownRow["CompletedEffort"] = iterationCompletedEffort / 60M;
							burndownRow["EstimatedEffort"] = runningEstimatedEffort / 60M;
							burndownRow["IdealEffort"] = idealEffort / 60M;
							burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);

							//Subtract the current's iteration's estimated effort from the running total
							runningEstimatedEffort -= iterationEstimatedEffort;

							//Subtract from the ideal effort line
							idealEffort -= idealEffortSegment;
							//}
						}
						xAxisColumn.Caption = GlobalResources.General.Graph_Iteration;
					}
				}
				else
				{
					//Get the list of releases in this project
					ReleaseManager releaseManager = new ReleaseManager();
					List<ReleaseView> releases = releaseManager.RetrieveByProjectId(projectId, true, false);

					//Calculate the burndown information. We start by considering the total estimated effort for all the
					//releases and then we subtract the amount completed for each release
					//We use estimated effort not projected effort for burndown so that we can see progress not effort
					decimal runningEstimatedEffort = 0M;
					foreach (ReleaseView releaseRow in releases)
					{
						if (releaseRow.TaskEstimatedEffort.HasValue)
						{
							runningEstimatedEffort += (decimal)releaseRow.TaskEstimatedEffort.Value;
						}
					}

					//We also calculate the 'ideal' effort that will be used for the superimposed line on the bar-chart
					decimal idealEffort = runningEstimatedEffort;
					int releaseCount = releases.Count;

					//Make sure we have at least one release (avoid division by zero error)
					if (releaseCount < 1)
					{
						return burndownDataSet;
					}

					decimal idealEffortSegment = idealEffort / (decimal)releaseCount;
					foreach (ReleaseView releaseRow in releases)
					{
						DataRow burndownRow = burndownDataSet.Tables["Burndown"].NewRow();
						burndownRow["XAxis"] = releaseRow.VersionNumber;

						decimal iterationEstimatedEffort = 0M;
						decimal iterationCompletedEffort = 0M;
						decimal iterationRemainingEffort = 0M;
						if (releaseRow.TaskEstimatedEffort.HasValue)
						{
							iterationEstimatedEffort += releaseRow.TaskEstimatedEffort.Value;
							if (releaseRow.TaskRemainingEffort.HasValue)
							{
								iterationRemainingEffort = releaseRow.TaskRemainingEffort.Value;
								iterationCompletedEffort = iterationEstimatedEffort - iterationRemainingEffort;
								if (iterationCompletedEffort > iterationEstimatedEffort)
								{
									iterationCompletedEffort = iterationEstimatedEffort;
								}
							}
						}

						//Convert efforts into hours (from minutes) and add to row
						burndownRow["RemainingEffort"] = iterationRemainingEffort / 60M;
						burndownRow["CompletedEffort"] = iterationCompletedEffort / 60M;
						burndownRow["EstimatedEffort"] = runningEstimatedEffort / 60M;
						burndownRow["IdealEffort"] = idealEffort / 60M;
						burndownDataSet.Tables["Burndown"].Rows.Add(burndownRow);

						//Subtract the current's iteration's estimated effort from the running total
						runningEstimatedEffort -= iterationEstimatedEffort;

						//Subtract from the ideal effort line
						idealEffort -= idealEffortSegment;
					}
					xAxisColumn.Caption = GlobalResources.General.Graph_Release;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return burndownDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>Retrieves the count of the number of incidents open for a specific date range by priority</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="endDate">The ending date for the data-range</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="incidentTypeId">The type of incident to filter (pass NullParamemeter for all)</param>
		/// <param name="incidentReportingInterval">Whether to use a weekly or daily time interval</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <param name="projectTemplateId">The project template we're using</param>
		/// <returns>The requested data for the time interval</returns>
		/// <remarks>
		/// We use the detected release
		/// </remarks>
		public System.Data.DataSet RetrieveIncidentOpenCountByPriority(int projectId, int projectTemplateId, Graph.ReportingIntervalEnum incidentReportingInterval, DateRange dateRange, double utcOffset, int? releaseId = null, Nullable<int> incidentTypeId = null, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIncidentOpenCountByPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new datasets
			System.Data.DataSet discoveryCountDataSet = new System.Data.DataSet();
			System.Data.DataSet closedCountDataSet = new System.Data.DataSet();
			System.Data.DataSet incidentCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND DETECTED_RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Create the incident type filter clause if needed
				string incidentTypeFilter = "";
				if (incidentTypeId.HasValue)
				{
					incidentTypeFilter = " AND INCIDENT_TYPE_ID = " + incidentTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//SELECT statement for the count of discovered incidents
				string sql =
					"SELECT	CREATION_DATE As Date, COUNT(INCIDENT_ID) AS DiscoveredCount, ISNULL(PRIORITY_ID, 0) As PriorityId " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE, PROJECT_ID, INCIDENT_ID, PRIORITY_ID FROM TST_INCIDENT WHERE 1=1 " +
					((includeDeleted) ? "" : " AND IS_DELETED = 0 ") +  //Only undeleted Incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CREATION_DATE, PRIORITY_ID " +
					"ORDER BY CREATION_DATE DESC, PRIORITY_ID ASC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					discoveryCountDataSet = ExecuteSql(context, sql, "DiscoveryCount");
				}

				//SELECT statement for the count of closed incidents
				sql =
					"SELECT	CLOSED_DATE As Date, COUNT(INCIDENT_ID) AS ClosedCount, ISNULL(PRIORITY_ID, 0) As PriorityId " +
					"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",CLOSED_DATE)) AS FLOAT))AS DATETIME) AS CLOSED_DATE, PROJECT_ID, INCIDENT_ID, PRIORITY_ID FROM VW_INCIDENT_LIST WHERE INCIDENT_STATUS_IS_OPEN_STATUS = 0 AND CLOSED_DATE IS NOT NULL " +
					((includeDeleted) ? "" : " AND IS_DELETED = 0 ") +  //Only undeleted Incidents.
					releaseFilter + incidentTypeFilter + ") AS INC " +
					"WHERE	PROJECT_ID = " + projectId.ToString() + " " +
					"AND	CLOSED_DATE <= " + CultureInvariantDateTime(endDate) + " " +
					"GROUP BY CLOSED_DATE, PRIORITY_ID " +
					"ORDER BY CLOSED_DATE DESC, PRIORITY_ID ASC";

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					closedCountDataSet = ExecuteSql(context, sql, "ClosedCount");
				}

				//Get the incident priority lookup
				IncidentManager incidentManager = new IncidentManager();
				List<IncidentPriority> incidentPriorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, true);

				//Now we need to create an empty dataset containing just the empty date-range
				incidentCountDataSet.Tables.Add("IncidentCount");
				DataColumn column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("Date", typeof(DateTime));
				column.Caption = GlobalResources.General.Graph_Date;
				//Add a column for each incident priority id (the name of the column is the ID)
				for (int i = 0; i < incidentPriorities.Count; i++)
				{
					column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add(incidentPriorities[i].PriorityId.ToString(), typeof(int));
					column.Caption = incidentPriorities[i].Name;
					column.ExtendedProperties.Add("Color", incidentPriorities[i].Color);
				}
				//And one column for no priority set
				column = incidentCountDataSet.Tables["IncidentCount"].Columns.Add("0", typeof(int));
				column.Caption = GlobalResources.General.Global_None;
				column.ExtendedProperties.Add("Color", "e0e0e0");

				//Make Date the primary key of all three data-tables and also priority the primary key of two of them
				discoveryCountDataSet.Tables["DiscoveryCount"].PrimaryKey = new DataColumn[] {
					discoveryCountDataSet.Tables["DiscoveryCount"].Columns["Date"],
					discoveryCountDataSet.Tables["DiscoveryCount"].Columns["PriorityId"]
					};
				closedCountDataSet.Tables["ClosedCount"].PrimaryKey = new DataColumn[] {
					closedCountDataSet.Tables["ClosedCount"].Columns["Date"],
					closedCountDataSet.Tables["ClosedCount"].Columns["PriorityId"],
				};
				incidentCountDataSet.Tables["IncidentCount"].PrimaryKey = new DataColumn[] {
					incidentCountDataSet.Tables["IncidentCount"].Columns["Date"]
				};

				//Now loop through the dataset and populate with the appropriate day range and map in the
				//current open count (created - closed) for that date interval

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (incidentReportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				//Now reset the running date to the earliest date
				DateTime runningDate = startDate;

				//We need to store the running number of open incidents at a given time (by priority/importance)
				int[] openCount = new int[incidentPriorities.Count + 1];

				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = incidentCountDataSet.Tables["IncidentCount"].NewRow();
					dataRow["Date"] = runningDate;
					incidentCountDataSet.Tables["IncidentCount"].Rows.Add(dataRow);

					//Now see if we have discovery or closed counts for this interval
					//And add and subtract them from the running totals
					//If this is the first date entry on the x-axis, need to also include the values
					//for any dates PRIOR to this as well (this is a special case)
					int k;
					if (i == 0)
					{
						//We need to look at them for each priority
						for (k = 0; k < incidentPriorities.Count; k++)
						{
							int priorityId = incidentPriorities[k].PriorityId;
							//Find the items before the first date on the graph and add to the initial counts
							foreach (DataRow testDataRow in discoveryCountDataSet.Tables["DiscoveryCount"].Rows)
							{
								if (((DateTime)testDataRow["Date"]).Date < runningDate.Date && (int)testDataRow["PriorityId"] == priorityId)
								{
									openCount[k] += (int)testDataRow["DiscoveredCount"];
								}
							}
							foreach (DataRow testDataRow in closedCountDataSet.Tables["ClosedCount"].Rows)
							{
								if (((DateTime)testDataRow["Date"]).Date < runningDate.Date && (int)testDataRow["PriorityId"] == priorityId)
								{
									openCount[k] -= (int)testDataRow["ClosedCount"];
								}
							}
						}

						//Now the incidents with no priority set
						//Find the items before the first date on the graph and add to the initial counts
						k = incidentPriorities.Count;
						foreach (DataRow testDataRow in discoveryCountDataSet.Tables["DiscoveryCount"].Rows)
						{
							if (((DateTime)testDataRow["Date"]).Date < runningDate.Date && (int)testDataRow["PriorityId"] == 0)
							{
								openCount[k] += (int)testDataRow["DiscoveredCount"];
							}
						}
						foreach (DataRow testDataRow in closedCountDataSet.Tables["ClosedCount"].Rows)
						{
							if (((DateTime)testDataRow["Date"]).Date < runningDate.Date && (int)testDataRow["PriorityId"] == 0)
							{
								openCount[k] -= (int)testDataRow["ClosedCount"];
							}
						}

					}
					System.Data.DataRow foundDataRow;
					for (int j = 0; j < dateInterval; j++)
					{
						//We need to look at them for each priority
						for (k = 0; k < incidentPriorities.Count; k++)
						{
							int priorityId = incidentPriorities[k].PriorityId;
							//Discovered Incidents
							foundDataRow = discoveryCountDataSet.Tables["DiscoveryCount"].Rows.Find(new object[] { runningDate.AddDays(-j), priorityId.ToString() });
							if (foundDataRow != null)
							{
								openCount[k] += (int)foundDataRow["DiscoveredCount"];
							}
							//Closed Incidents
							foundDataRow = closedCountDataSet.Tables["ClosedCount"].Rows.Find(new object[] { runningDate.AddDays(-j), priorityId.ToString() });
							if (foundDataRow != null)
							{
								openCount[k] -= (int)foundDataRow["ClosedCount"];
							}
						}

						//Now for those with no priority set
						k = incidentPriorities.Count;
						//Discovered Incidents
						foundDataRow = discoveryCountDataSet.Tables["DiscoveryCount"].Rows.Find(new object[] { runningDate.AddDays(-j), 0 });
						if (foundDataRow != null)
						{
							openCount[k] += (int)foundDataRow["DiscoveredCount"];
						}
						//Closed Incidents
						foundDataRow = closedCountDataSet.Tables["ClosedCount"].Rows.Find(new object[] { runningDate.AddDays(-j), 0 });
						if (foundDataRow != null)
						{
							openCount[k] -= (int)foundDataRow["ClosedCount"];
						}
					}

					//Now update the datarow for the interval
					for (k = 0; k < incidentPriorities.Count; k++)
					{
						int priorityId = incidentPriorities[k].PriorityId;
						dataRow[priorityId.ToString()] = openCount[k];
					}
					//Those with no priority (denote with 0)
					dataRow["0"] = openCount[incidentPriorities.Count];

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the count of the number of test cases executed on a specific date range by execution status
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dateRange">The date-range (in local time)</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="testCaseTypeId">Whether we want to filter by a specific test case type (null = all)</param>
		/// <param name="reportingInterval">Whether to use a weekly or daily time interval</param>
		/// <returns>The requested data for the time interval</returns>
		public System.Data.DataSet RetrieveTestCaseCountByExecutionStatus(int projectId, Graph.ReportingIntervalEnum reportingInterval, DateRange dateRange, double utcOffset, Nullable<int> releaseId = null, int? testCaseTypeId = null)
		{
			const string METHOD_NAME = "RetrieveTestCaseCountByExecutionStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new master dataset to hold the consolidated data
			System.Data.DataSet testCaseCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Add the test case type id filter clause if needed
				string typeFilter = "";
				if (testCaseTypeId.HasValue)
				{
					typeFilter = " AND TEST_CASE_TYPE_ID = " + testCaseTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//Now get the list of available execution statuses
				TestCaseManager testCaseManager = new TestCaseManager();
				List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();

				//Now we need to create the master dataset containing date and a column for execution status
				testCaseCountDataSet.Tables.Add("TestCaseCount");
				DataColumn dataColumn = testCaseCountDataSet.Tables["TestCaseCount"].Columns.Add("Date", typeof(DateTime));
				dataColumn.Caption = GlobalResources.General.Graph_Date;
				DataColumn[] primaryKeys = new DataColumn[1];
				primaryKeys[0] = dataColumn;
				testCaseCountDataSet.Tables["TestCaseCount"].PrimaryKey = primaryKeys;

				//Add a column for each execution status id (the name of the column is the ID)
				foreach (ExecutionStatus executionStatus in executionStati)
				{
					int executionStatusId = executionStatus.ExecutionStatusId;
					DataColumn column = testCaseCountDataSet.Tables["TestCaseCount"].Columns.Add(executionStatusId.ToString(), typeof(int));
					column.Caption = executionStatus.Name;
					column.DefaultValue = 0;
					column.AllowDBNull = false;

					//Need to pass the color of the status since the graph cannot handle CSS classes
					column.ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(executionStatusId));
				}

				//Now loop through the master dataset and populate with the appropriate day ranges

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (reportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (reportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				DateTime runningDate = startDate;
				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = testCaseCountDataSet.Tables["TestCaseCount"].NewRow();
					dataRow["Date"] = runningDate;
					testCaseCountDataSet.Tables["TestCaseCount"].Rows.Add(dataRow);

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				//Now iterate through all the execution statuses and retrieve the data
				foreach (ExecutionStatus executionStatus in executionStati)
				{
					int executionStatusId = executionStatus.ExecutionStatusId;

					//Create a temporary dataset to hold the results for one execution status
					DataSet tempDataSet = new DataSet();

					//For the not-run status we need to use a different query to find the number created during the period
					//this is cumulative
					string sql;
					int initialCount = 0;
					if (executionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun)
					{
						//We need to get the count of test cases created BEFORE the specified date range
						sql = @"
SELECT COUNT(TEST_CASE_ID) AS TestCaseCount
FROM	(SELECT DISTINCT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE,
		TST.TEST_CASE_ID
		FROM TST_TEST_CASE TST INNER JOIN TST_RELEASE_TEST_CASE RTC
		ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @" AND TST.IS_DELETED = 0) AS TST
WHERE	CREATION_DATE < " + CultureInvariantDateTime(startDate) + @"
";
						//Actually execute the query and return the dataset
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							tempDataSet = ExecuteSql(context, sql, "TempCount");
						}
						initialCount = (int)tempDataSet.Tables[0].Rows[0]["TestCaseCount"];

						//Now retrieve the count of test cases created per day
						sql = @"
SELECT	CREATION_DATE As Date, COUNT(TEST_CASE_ID) AS TestCaseCount
FROM	(SELECT DISTINCT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE,
		TST.TEST_CASE_ID
		FROM TST_TEST_CASE TST INNER JOIN TST_RELEASE_TEST_CASE RTC
		ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @" AND TST.IS_DELETED = 0) AS TST
WHERE	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + @" AND CREATION_DATE >= " + CultureInvariantDateTime(startDate) + @"
GROUP BY CREATION_DATE
ORDER BY CREATION_DATE DESC
";
					}
					else
					{
						//Now retrieve the count of test runs grouped by test case per day for this execution status (and release)
						sql = @"
SELECT	TRN.END_DATE As Date, COUNT(TRN.TEST_RUN_ID) AS TestCaseCount
FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",TRN1.END_DATE)) AS FLOAT))AS DATETIME) AS END_DATE,
		TRN1.TEST_RUN_ID, TRN1.EXECUTION_STATUS_ID , TRN1.TEST_CASE_ID
		FROM TST_TEST_RUN TRN1 INNER JOIN TST_TEST_CASE TST
		ON TST.TEST_CASE_ID = TRN1.TEST_CASE_ID
		INNER JOIN
			(SELECT TRN3.TEST_CASE_ID, MAX(TRN3.END_DATE) AS END_DATE, CAST(FLOOR(CAST(DATEADD(minute,0,DATEADD(hour,0,TRN3.END_DATE)) AS FLOAT))AS DATETIME) AS RUN_DAY
            FROM TST_TEST_RUN TRN3
			INNER JOIN TST_TEST_CASE TST
			ON TST.TEST_CASE_ID = TRN3.TEST_CASE_ID
			WHERE TRN3.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @"
			GROUP BY TRN3.TEST_CASE_ID, CAST(FLOOR(CAST(DATEADD(minute,0,DATEADD(hour,0,TRN3.END_DATE)) AS FLOAT))AS DATETIME)) TRN2
		ON TRN1.TEST_CASE_ID = TRN2.TEST_CASE_ID AND TRN1.END_DATE = TRN2.END_DATE
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @"
        AND   TST.IS_DELETED = 0
		) AS TRN
		
WHERE	TRN.END_DATE <= " + CultureInvariantDateTime(endDate) + @"
AND TRN.END_DATE >= " + CultureInvariantDateTime(startDate) + @"
AND	TRN.EXECUTION_STATUS_ID = " + executionStatusId + @"
GROUP BY TRN.END_DATE
ORDER BY TRN.END_DATE DESC
";
					}

					//Actually execute the query and return the dataset
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						tempDataSet = ExecuteSql(context, sql, "TempCount");
					}

					//Add the primary key
					primaryKeys = new DataColumn[1];
					primaryKeys[0] = tempDataSet.Tables["TempCount"].Columns["Date"];
					tempDataSet.Tables["TempCount"].PrimaryKey = primaryKeys;

					//Iterate through the number of dates summing up the interval for this status
					runningDate = startDate;
					int runningCount = initialCount;   //Only used for Not Run + Cumulative
					System.Data.DataRow foundDataRow;
					for (int j = 0; j < numberOfDates; j++)
					{
						//Sum up over the interval
						int testCaseCount = 0;
						for (int k = 0; k < dateInterval; k++)
						{
							foundDataRow = tempDataSet.Tables["TempCount"].Rows.Find(runningDate.AddDays(-k));
							if (foundDataRow != null)
							{
								testCaseCount += (int)foundDataRow["TestCaseCount"];
							}
						}

						//Populate the master dataset
						foundDataRow = testCaseCountDataSet.Tables["TestCaseCount"].Rows.Find(runningDate);
						if (foundDataRow != null)
						{
							if (executionStatusId == (int)TestCase.ExecutionStatusEnum.NotRun)
							{
								runningCount += testCaseCount;
								foundDataRow[executionStatusId.ToString()] = runningCount;
							}
							else
							{
								foundDataRow[executionStatusId.ToString()] = testCaseCount;
							}
						}

						//Increment the day count
						runningDate = runningDate.AddDays(dateInterval);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the count of the number of test cases executed on a specific date range by execution status cumulatively
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dateRange">The date-range (in local time)</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="testCaseTypeId">Whether we want to filter by a specific test case type (null = all)</param>
		/// <param name="reportingInterval">Whether to use a weekly or daily time interval</param>
		/// <returns>The requested data for the time interval</returns>
		public System.Data.DataSet RetrieveTestCaseCountByExecutionStatusCumulative(int projectId, Graph.ReportingIntervalEnum reportingInterval, DateRange dateRange, double utcOffset, Nullable<int> releaseId = null, int? testCaseTypeId = null)
		{
			const string METHOD_NAME = "RetrieveTestCaseCountByExecutionStatusCumulative";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new master dataset to hold the consolidated data
			System.Data.DataSet testCaseCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Add the test case type id filter clause if needed
				string typeFilter = "";
				if (testCaseTypeId.HasValue)
				{
					typeFilter = " AND TEST_CASE_TYPE_ID = " + testCaseTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//Now get the list of available execution statuses
				TestCaseManager testCaseManager = new TestCaseManager();
				List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();

				//Now we need to create the master dataset containing date and a column for execution status
				testCaseCountDataSet.Tables.Add("TestCaseCount");
				DataColumn dataColumn = testCaseCountDataSet.Tables["TestCaseCount"].Columns.Add("Date", typeof(DateTime));
				dataColumn.Caption = GlobalResources.General.Graph_Date;
				DataColumn[] primaryKeys = new DataColumn[1];
				primaryKeys[0] = dataColumn;
				testCaseCountDataSet.Tables["TestCaseCount"].PrimaryKey = primaryKeys;

				//Add a column for each execution status id (the name of the column is the ID)
				int executionStatusId;
				foreach (ExecutionStatus executionStatus in executionStati)
				{
					executionStatusId = executionStatus.ExecutionStatusId;
					DataColumn column = testCaseCountDataSet.Tables["TestCaseCount"].Columns.Add(executionStatusId.ToString(), typeof(int));
					column.Caption = executionStatus.Name;
					column.DefaultValue = 0;
					column.AllowDBNull = false;

					//Need to pass the color of the status since the graph cannot handle CSS classes
					column.ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(executionStatusId));
				}

				//Now loop through the master dataset and populate with the appropriate day ranges

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (reportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (reportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				DateTime runningDate = startDate;
				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = testCaseCountDataSet.Tables["TestCaseCount"].NewRow();
					dataRow["Date"] = runningDate;
					testCaseCountDataSet.Tables["TestCaseCount"].Rows.Add(dataRow);

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				//First we need to get the creation counts which are the base not-run values
				executionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;

				//Create a temporary dataset to hold the results for one execution status
				DataSet tempDataSet = new DataSet();

				//We need to get the count of test cases created BEFORE the specified sate range
				string sql = @"
SELECT COUNT(TEST_CASE_ID) AS TestCaseCount
FROM	(SELECT DISTINCT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE,
		TST.TEST_CASE_ID
		FROM TST_TEST_CASE TST INNER JOIN TST_RELEASE_TEST_CASE RTC
		ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @" AND TST.IS_DELETED = 0) AS TST
WHERE	CREATION_DATE < " + CultureInvariantDateTime(startDate) + @"
";
				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					tempDataSet = ExecuteSql(context, sql, "TempCount");
				}
				int initialCount = (int)tempDataSet.Tables[0].Rows[0]["TestCaseCount"];

				//Now retrieve the count of test cases created per day in the specified date range
				sql = @"
SELECT	CREATION_DATE As Date, COUNT(TEST_CASE_ID) AS TestCaseCount
FROM	(SELECT DISTINCT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",CREATION_DATE)) AS FLOAT))AS DATETIME) AS CREATION_DATE,
		TST.TEST_CASE_ID
		FROM TST_TEST_CASE TST INNER JOIN TST_RELEASE_TEST_CASE RTC
		ON TST.TEST_CASE_ID = RTC.TEST_CASE_ID
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @" AND TST.IS_DELETED = 0) AS TST
WHERE	CREATION_DATE <= " + CultureInvariantDateTime(endDate) + @" AND CREATION_DATE >= " + CultureInvariantDateTime(startDate) + @"
GROUP BY CREATION_DATE
ORDER BY CREATION_DATE DESC
";
				//Now we need to populate the values
				int runningCount = initialCount;
				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					tempDataSet = ExecuteSql(context, sql, "TempCount");
				}

				//Add the primary key
				primaryKeys = new DataColumn[1];
				primaryKeys[0] = tempDataSet.Tables["TempCount"].Columns["Date"];
				tempDataSet.Tables["TempCount"].PrimaryKey = primaryKeys;

				//Iterate through the number of dates summing up the interval for this status
				runningDate = startDate;
				System.Data.DataRow foundDataRow;
				for (int j = 0; j < numberOfDates; j++)
				{
					//Sum up over the interval
					int testCaseCount = 0;
					for (int k = 0; k < dateInterval; k++)
					{
						foundDataRow = tempDataSet.Tables["TempCount"].Rows.Find(runningDate.AddDays(-k));
						if (foundDataRow != null)
						{
							testCaseCount += (int)foundDataRow["TestCaseCount"];
						}
					}

					//Populate the master dataset
					foundDataRow = testCaseCountDataSet.Tables["TestCaseCount"].Rows.Find(runningDate);
					if (foundDataRow != null)
					{
						runningCount += testCaseCount;
						foundDataRow[executionStatusId.ToString()] = runningCount;
					}

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				//Create a second temp dataset organized by execution status id
				DataSet tempDataSet2 = new DataSet();

				//Iterate through the number of dates, we have to query the test runs for all runs up to this date
				//grouped by the test case and most recent status. This is the only accurate way to get the
				//cumulative test case status. We cannot just simply sum up the statuses for each interval
				//since we would otherwise get double counting for the same test case run on different days
				runningDate = startDate;
				for (int j = 0; j < numberOfDates; j++)
				{
					//Execute the query for all tests runs up to this date, grouped by status and test case
					sql = @"
SELECT	TRN.EXECUTION_STATUS_ID As ExecutionStatusId, COUNT(TRN.TEST_RUN_ID) AS TestCaseCount
FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + @",DATEADD(hour," + utcOffsetHours + @",TRN1.END_DATE)) AS FLOAT))AS DATETIME) AS END_DATE,
		TRN1.TEST_RUN_ID, TRN1.EXECUTION_STATUS_ID , TRN1.TEST_CASE_ID
		FROM TST_TEST_RUN TRN1 INNER JOIN TST_TEST_CASE TST
		ON TST.TEST_CASE_ID = TRN1.TEST_CASE_ID
		INNER JOIN
			(SELECT TRN3.TEST_CASE_ID, MAX(TRN3.END_DATE) AS END_DATE
            FROM TST_TEST_RUN TRN3
			INNER JOIN TST_TEST_CASE TST
			ON TST.TEST_CASE_ID = TRN3.TEST_CASE_ID
			WHERE TRN3.EXECUTION_STATUS_ID <> 3 /* Not Run */ AND TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @"
            AND TRN3.END_DATE <= " + CultureInvariantDateTime(runningDate) + @"
			GROUP BY TRN3.TEST_CASE_ID) TRN2
		ON TRN1.TEST_CASE_ID = TRN2.TEST_CASE_ID AND TRN1.END_DATE = TRN2.END_DATE
		WHERE TST.PROJECT_ID = " + projectId + @" " + releaseFilter + typeFilter + @"
        AND   TST.IS_DELETED = 0
		) AS TRN
		
WHERE	TRN.END_DATE <= " + CultureInvariantDateTime(runningDate) + @"
AND	TRN.EXECUTION_STATUS_ID <> 3 /* Not Run */
GROUP BY TRN.EXECUTION_STATUS_ID
ORDER BY TRN.EXECUTION_STATUS_ID ASC
";

					//Actually execute the query and return the dataset
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						tempDataSet2 = ExecuteSql(context, sql, "TempCount");
					}

					//Add the primary key for the second temp dataset
					primaryKeys = new DataColumn[1];
					primaryKeys[0] = tempDataSet2.Tables["TempCount"].Columns["ExecutionStatusId"];
					tempDataSet2.Tables["TempCount"].PrimaryKey = primaryKeys;

					//Loop through the statuses
					foreach (ExecutionStatus executionStatus in executionStati)
					{
						executionStatusId = executionStatus.ExecutionStatusId;
						if (executionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun)
						{
							int testCaseCount = 0;
							foundDataRow = tempDataSet2.Tables["TempCount"].Rows.Find(executionStatusId);
							if (foundDataRow != null)
							{
								testCaseCount = (int)foundDataRow["TestCaseCount"];
							}

							//Populate the master dataset
							foundDataRow = testCaseCountDataSet.Tables["TestCaseCount"].Rows.Find(runningDate);
							if (foundDataRow != null)
							{
								foundDataRow[executionStatusId.ToString()] = testCaseCount;

								//We also need to decrement from the 'not run count'
								foundDataRow[((int)TestCase.ExecutionStatusEnum.NotRun).ToString()] = (int)foundDataRow[((int)TestCase.ExecutionStatusEnum.NotRun).ToString()] - testCaseCount;
							}
						}
					}

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the count of the number of test runs executed on a specific date range by execution status
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="dateRange">The date-range (in local time)</param>
		/// <param name="utcOffset">The number of hours the requested timezone is offset from UTC</param>
		/// <param name="releaseId">Whether we want to filter by a specific release (null for all)</param>
		/// <param name="reportingInterval">Whether to use a weekly or daily time interval</param>
		/// <param name="testCaseTypeId">Whether we want to filter by a specific test case type (null = all)</param>
		/// <returns>The requested data for the time interval</returns>
		public System.Data.DataSet RetrieveTestRunCountByExecutionStatus(int projectId, Graph.ReportingIntervalEnum reportingInterval, DateRange dateRange, double utcOffset, Nullable<int> releaseId = null, int? testCaseTypeId = null)
		{
			const string METHOD_NAME = "RetrieveTestRunCountByExecutionStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new master dataset to hold the consolidated data
			System.Data.DataSet testRunCountDataSet = new System.Data.DataSet();

			try
			{
				//Get the whole number of hours and minutes that the UTC offset is
				int utcOffsetHours = (int)Math.Truncate(utcOffset);
				double utcOffsetFraction = utcOffset - Math.Truncate(utcOffset);
				int utcOffsetMinutes = (int)(utcOffsetFraction * (double)60);

				//Create the release filter clause if needed
				string releaseFilter = "";
				if (releaseId.HasValue)
				{
					//Need to consider the release and any child iterations
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);
					if (!String.IsNullOrEmpty(releaseList))
					{
						releaseFilter = " AND TRN.RELEASE_ID IN (" + releaseList + ") ";
					}
				}

				//Add the test case type id filter clause if needed
				string typeFilter = "";
				if (testCaseTypeId.HasValue)
				{
					typeFilter = " AND TEST_CASE_TYPE_ID = " + testCaseTypeId.Value + " ";
				}

				//Extract the start and end-date from the date-interval
				DateTime startDate = DateTime.Now.AddMonths(-1);
				if (dateRange.StartDate.HasValue)
				{
					startDate = dateRange.StartDate.Value.Date;
				}
				DateTime endDate = DateTime.Now;
				if (dateRange.EndDate.HasValue)
				{
					endDate = dateRange.EndDate.Value.Date;
				}

				//Make sure the start-date is not before the end-date
				//If so, just use the end-date and a 1-month range
				if (startDate > endDate)
				{
					startDate = endDate.AddMonths(-1);
				}

				//Now get the list of available execution statuses
				TestCaseManager testCaseManager = new TestCaseManager();
				List<ExecutionStatus> executionStati = testCaseManager.RetrieveExecutionStatuses();

				//Now we need to create the master dataset containing date and a column for execution status
				testRunCountDataSet.Tables.Add("TestRunCount");
				DataColumn dataColumn = testRunCountDataSet.Tables["TestRunCount"].Columns.Add("Date", typeof(DateTime));
				dataColumn.Caption = GlobalResources.General.Graph_Date;
				DataColumn[] primaryKeys = new DataColumn[1];
				primaryKeys[0] = dataColumn;
				testRunCountDataSet.Tables["TestRunCount"].PrimaryKey = primaryKeys;

				//Add a column for each execution status id (the name of the column is the ID)
				foreach (ExecutionStatus executionStatus in executionStati)
				{
					int executionStatusId = executionStatus.ExecutionStatusId;
					DataColumn column = testRunCountDataSet.Tables["TestRunCount"].Columns.Add(executionStatusId.ToString(), typeof(int));
					column.Caption = executionStatus.Name;
					column.DefaultValue = 0;
					column.AllowDBNull = false;

					//Need to pass the color of the status since the graph cannot handle CSS classes
					column.ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(executionStatusId));
				}

				//Now loop through the master dataset and populate with the appropriate day ranges

				//Calculate the number of dates in the date-range
				int numberOfDates = 0;
				int dateInterval = 0;
				TimeSpan timeSpan = endDate.Subtract(startDate);
				if (reportingInterval == Graph.ReportingIntervalEnum.Daily)
				{
					dateInterval = 1;
					numberOfDates = (int)timeSpan.TotalDays + 1;
				}
				if (reportingInterval == Graph.ReportingIntervalEnum.Weekly)
				{
					dateInterval = 7;
					numberOfDates = (int)((timeSpan.TotalDays + 1) / (double)dateInterval);
				}

				DateTime runningDate = startDate;
				for (int i = 0; i < numberOfDates; i++)
				{
					//Populate the day value
					System.Data.DataRow dataRow = testRunCountDataSet.Tables["TestRunCount"].NewRow();
					dataRow["Date"] = runningDate;
					testRunCountDataSet.Tables["TestRunCount"].Rows.Add(dataRow);

					//Increment the day count
					runningDate = runningDate.AddDays(dateInterval);
				}

				//Now iterate through all the execution statuses and retrieve the data
				foreach (ExecutionStatus executionStatus in executionStati)
				{
					int executionStatusId = executionStatus.ExecutionStatusId;

					//Don't consider incomplete test runs
					if (executionStatusId != (int)TestCase.ExecutionStatusEnum.NotRun)
					{
						//Create a temporary dataset to hold the results for one execution status
						DataSet tempDataSet = new DataSet();

						//Now retrieve the count of test runs grouped by day for this execution status (and release)
						string sql =
							"SELECT	END_DATE As Date, COUNT(TEST_RUN_ID) AS TestRunCount " +
							"FROM	(SELECT CAST(FLOOR(CAST(DATEADD(minute," + utcOffsetMinutes + ",DATEADD(hour," + utcOffsetHours + ",END_DATE)) AS FLOAT))AS DATETIME) AS END_DATE, TRN.TEST_RUN_ID, TRN.EXECUTION_STATUS_ID FROM TST_TEST_RUN TRN INNER JOIN TST_TEST_CASE TST ON TST.TEST_CASE_ID = TRN.TEST_CASE_ID WHERE TST.PROJECT_ID = " + projectId.ToString() + " " + releaseFilter + typeFilter + ") AS TRN " +
							"WHERE	END_DATE <= " + CultureInvariantDateTime(endDate) + " " +
							"AND	EXECUTION_STATUS_ID = " + executionStatusId.ToString() + " " +
							"GROUP BY END_DATE " +
							"ORDER BY END_DATE DESC";

						//Actually execute the query and return the dataset
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							tempDataSet = ExecuteSql(context, sql, "TempCount");
						}

						//Add the primary key
						primaryKeys = new DataColumn[1];
						primaryKeys[0] = tempDataSet.Tables["TempCount"].Columns["Date"];
						tempDataSet.Tables["TempCount"].PrimaryKey = primaryKeys;

						//Iterate through the number of dates summing up the interval for this status
						runningDate = startDate;
						System.Data.DataRow foundDataRow;
						for (int j = 0; j < numberOfDates; j++)
						{
							//Sum up over the interval
							int testRunCount = 0;
							for (int k = 0; k < dateInterval; k++)
							{
								foundDataRow = tempDataSet.Tables["TempCount"].Rows.Find(runningDate.AddDays(-k));
								if (foundDataRow != null)
								{
									testRunCount += (int)foundDataRow["TestRunCount"];
								}
							}

							//Populate the master dataset
							foundDataRow = testRunCountDataSet.Tables["TestRunCount"].Rows.Find(runningDate);
							if (foundDataRow != null)
							{
								foundDataRow[executionStatusId.ToString()] = testRunCount;
							}

							//Increment the day count
							runningDate = runningDate.AddDays(dateInterval);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunCountDataSet;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region EF-DataSet Code

		/// <summary>
		/// Executes dynamic SQL against the underlying provider connection and uses
		/// an ADO.NET DataAdapter to return a datatable/dataset.
		/// </summary>
		/// <param name="context">The EF object context</param>
		/// <param name="sql">The dynamic SQL</param>
		/// <remarks>
		/// This is needed because Entities are not well suited to dynamic columns,
		/// so we use untyped datasets for this situation
		/// </remarks>
		protected DataSet ExecuteSql(ObjectContext context, string sql, string dataTable)
		{
			const string METHOD_NAME = "ExecuteSql";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//The return object
				DataSet dataSet = new DataSet();

				EntityConnection entityConnection = (EntityConnection)context.Connection;
				SqlConnection sqlConnection = null;
				if (entityConnection.StoreConnection is SqlConnection)
				{
					sqlConnection = (SqlConnection)entityConnection.StoreConnection;
				}
				else if (entityConnection.StoreConnection is EFTracingProvider.EFTracingConnection)
				{
					EFTracingConnection tracingConnection = (EFTracingProvider.EFTracingConnection)entityConnection.StoreConnection;
					sqlConnection = (SqlConnection)tracingConnection.WrappedConnection;
				}
				if (sqlConnection == null)
				{
					throw new ApplicationException("Unable to cast StoreConnection: " + entityConnection.StoreConnection.GetType().Name);
				}

				ConnectionState initialState = sqlConnection.State;
				try
				{
					if (initialState != ConnectionState.Open)
					{
						sqlConnection.Open();  // open connection if not already open
					}
					SqlCommand sqlCommand = sqlConnection.CreateCommand();
					using (SqlDataAdapter daReport = new SqlDataAdapter(sqlCommand))
					{
						using (sqlCommand)
						{
							sqlCommand.CommandType = CommandType.Text;
							sqlCommand.CommandText = sql;
							sqlCommand.ExecuteNonQuery();
							daReport.Fill(dataSet, dataTable);
						}
					}
				}
				finally
				{
					if (initialState != ConnectionState.Open)
					{
						sqlConnection.Close(); // only close connection if not initially open
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion
	}
}

/// <summary>This exception is thrown when you try and run custom graph SQL that returns back ungraphable data</summary>
public class GraphDataInvalidException : ApplicationException
{
	public GraphDataInvalidException()
	{
	}
	public GraphDataInvalidException(string message)
		: base(message)
	{
	}
	public GraphDataInvalidException(string message, Exception inner)
		: base(message, inner)
	{
	}
}

