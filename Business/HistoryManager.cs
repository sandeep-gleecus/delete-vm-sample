using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using RestSharp.Extensions;
using static Inflectra.SpiraTest.DataModel.Artifact;
using static Inflectra.SpiraTest.DataModel.HistoryChangeSetView;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for storing and retrieving the audit history log of all artifact changes</summary>
	public class HistoryManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.HistoryManager::";

		#region Delegates

		/// <summary>
		/// Handles any special filters that need handling
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="expressionList">The expression list</param>
		/// <param name="filter">The filter</param>
		/// <param name="projectTemplateId">The id of the project template (not used)</param>
		/// <param name="utcOffset">The UTC offset (for date filters)</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		public bool historyChangeViewFilterHandler(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			if (filter.Key == "ChangeSetTypeId")
			{
				//Create the filter using the equivalent physical field -> ChangeTypeId
				MemberExpression memberExpression = Expression.PropertyOrField(p, "ChangeTypeId");
				CreateFilterEntryExpression(projectId, filter.Key, filter.Value, utcOffset, memberExpression, expressionList, p, false);
				return true;
			}
			if (filter.Key == "ChangerId")
			{
				//Create the filter using the equivalent physical field -> UserId
				MemberExpression memberExpression = Expression.PropertyOrField(p, "UserId");
				CreateFilterEntryExpression(projectId, filter.Key, filter.Value, utcOffset, memberExpression, expressionList, p, false);
				return true;
			}
			return false;
		}

		#endregion


		/// <summary>
		/// Retrieves the last changeset for a specific artifact
		/// </summary>
		/// <param name="artifactId">What is the ID of the artifact</param>
		/// <param name="artifactType">What is the type of artifact</param>
		/// <param name="includeDetails">Should we include the change records</param>
		/// <returns></returns>
		public HistoryChangeSetResponse RetrieveLastChangeSetForArtifactId(int artifactId, ArtifactTypeEnum artifactType, bool includeDetails = false)
		{
			const string METHOD_NAME = "RetrieveChangeSetById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				HistoryChangeSetResponse changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//ObjectQuery<HistoryChangeSet> historyChangeSets = context.HistoryChangeSets.Include("Type");
					//if (includeDetails)
					//{
					//	historyChangeSets = historyChangeSets.Include("Details");
					//}

					var query = from h in context.HistoryChangeSetsView
								where h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType
								orderby h.ChangeDate descending, h.ChangeSetId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									//ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};

					//Get the first record only (or null if none)
					changeSet = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public OldHistoryChangesetResponse RetrieveoldLastChangeSetForArtifactId(int artifactId, ArtifactTypeEnum artifactType, bool includeDetails = false)
		{
			const string METHOD_NAME = "RetrieveoldLastChangeSetForArtifactId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				OldHistoryChangesetResponse changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//ObjectQuery<HistoryChangeSet> historyChangeSets = context.HistoryChangeSets.Include("Type");
					//if (includeDetails)
					//{
					//	historyChangeSets = historyChangeSets.Include("Details");
					//}

					var query = from h in context.OldHistory_ChangeSetsView
								join p in context.Projects
								on h.PROJECT_ID equals p.ProjectId
								select new OldHistoryChangesetResponse
								{
									ProjectId = h.PROJECT_ID,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									ChangeTypeId = h.CHANGETYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									ArtifactId = h.ARTIFACT_ID,
									ArtifactTypeId = h.ARTIFACT_TYPE_ID,
									ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
									ArtifactDesc = h.ARTIFACT_DESC,
									SignatureHash = h.SIGNATURE_HASH,
									UserName = h.USER_NAME,
									Meaning = h.MEANING,
									UserId = h.USER_ID,
									//ProjectName = p.Name,
									//FieldName = h.ARTIFACT_TYPE_NAME,
									//Time = "",
									//NameOfSigner = h.SignatureHash != null ? h.UserName : null,
								};

					//Get the first record only (or null if none)
					changeSet = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the audit history log associated with a particular artifact</summary>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <param name="historyChanges">The detailed change log</param>
		/// <param name="historyChangeSets">The changeset log</param>
		/// <param name="changeSetTypes">The changeset types</param>
		/// <remarks>ChangeSet is sorted by ID ascending because we use this for rollbacks, ChangeDetails sorted by date descending</remarks>
		public void RetrieveByArtifactId(int artifactId, ArtifactTypeEnum artifactType, out List<HistoryChangeSetView> historyChangeSets, out List<HistoryChangeView> historyChanges, out List<HistoryChangeSetType> changeSetTypes)
		{
			const string METHOD_NAME = "RetrieveByArtifactId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We get the different components through separate queries
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the Changesets.
					var query1 = from h in context.HistoryChangeSetsView
								 where h.ArtifactTypeId == (int)artifactType && h.ArtifactId == artifactId
								 orderby h.ChangeSetId ascending
								 select h;

					historyChangeSets = query1.ToList();

					//Now get the history items.
					var query2 = from h in context.HistoryChangesView
								 where h.ArtifactTypeId == (int)artifactType && h.ArtifactId == artifactId
								 orderby h.ChangeDate descending, h.ArtifactHistoryId
								 select h;

					historyChanges = query2.ToList();
				}

				//Now get the types..
				changeSetTypes = RetrieveChangeSetTypes();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Retrieves the audit history log associated with a particular artifact</summary>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <remarks>This version does not use the views, but instead the real artifacts and navigation properties</remarks>
		public List<HistoryChangeSetResponse> RetrieveByArtifactId(int artifactId, ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "RetrieveByArtifactId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We get the different components through separate queries
				List<HistoryChangeSetResponse> historyChangeSets;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the Changesets.
					//var query = from h in context.HistoryChangeSets.Include("Type").Include("Details")
					//			where h.ArtifactTypeId == (int)artifactType && h.ArtifactId == artifactId
					//			orderby h.ChangeDate descending, h.ChangeSetId
					//			select h;

					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								where h.ArtifactTypeId == (int)artifactType
								&& h.ArtifactId == (int)artifactId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};

					historyChangeSets = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return historyChangeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Loads the ChangeSet types</summary>
		/// <returns>List of change set types</returns>
		public List<HistoryChangeSetType> RetrieveChangeSetTypes()
		{
			const string METHOD_NAME = "RetrieveChangeSetTypes";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetType> types;

				//Load up the types.
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryChangeSetTypes
								orderby h.ChangeTypeId
								select h;

					types = query.ToList();
				}

				Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
				return types;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Counts all the history items for an artifact</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="artifactType">The artifact type</param>
		/// <param name="artifactId">The artifact id</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="includeSteps">Get the count including associated steps for the given Test Case or Requirement.</param>
		/// <returns>The total number of history items</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int projectId, int artifactId, ArtifactTypeEnum artifactType, Hashtable filters, double utcOffset, bool includeSteps = false)
		{
			const string METHOD_NAME = "Count";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								where h.ArtifactTypeId == (int)artifactType 
								&& h.ArtifactId== (int)artifactId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};

					////Filter by project if specified
					if (projectId > 0)
					{
						query = query.Where(h => h.ProjectId == projectId);
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filters, utcOffset, null, historyChangeViewFilterHandler);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				//Include histroy changes for any child artifacts (steps).
				if (includeSteps)
				{
					if (artifactType == ArtifactTypeEnum.TestCase)
					{
						count += CountStepsForTestCase(projectId, artifactId, filters, utcOffset);
					}
					else if (artifactType == ArtifactTypeEnum.Requirement)
					{
						count += CountStepsForRequirement(projectId, artifactId, filters, utcOffset);
					}
					else if (artifactType == ArtifactTypeEnum.Risk)
					{
						count += CountStepsForRisk(projectId, artifactId, filters, utcOffset);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Gets the number of DataSets for the given project id.</summary>
		/// <param name="projectId">The project ID.</param>
		/// <param name="filters">Additional filters.</param>
		/// <returns>The number of changesets.</returns>
		public int CountSet(int? projectId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountSet()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					//var query = from h in context.HistoryChangeSets
					//			select h;

					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};


					//Filter by project if specified
					if (projectId.HasValue && projectId > 0)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<OldHistoryChangesetResponse> RetrieveOldHistorySetsByProjectId(int? projectId, double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<OldHistoryChangesetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.OldHistory_ChangeSetsView
								select new OldHistoryChangesetResponse
								{
									ProjectId = h.PROJECT_ID,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									ChangeTypeId = h.CHANGETYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									ArtifactId = h.ARTIFACT_ID,
									ArtifactTypeId = h.ARTIFACT_TYPE_ID,
									ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
									ArtifactDesc = h.ARTIFACT_DESC,
									SignatureHash = h.SIGNATURE_HASH,
									UserName = h.USER_NAME,
									Meaning = h.MEANING,
									UserId = h.USER_ID,
								};


					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<OldHistoryChangesetResponse, bool>> filterClause = CreateFilterExpression<OldHistoryChangesetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<OldHistoryChangesetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
					if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}

					//Get the count
					int count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();

					//Now filter by signature in-memory if needed
					if (filterList != null && filterList.ContainsKey("Signed"))
					{
						if (filterList["Signed"] is int)
						{
							int signedId = (int)filterList["Signed"];
							changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
						}
						if (filterList["Signed"] is MultiValueFilter)
						{
							MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
							changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
						}
					}

					//Now sort by signature in memory if needed
					if (sortProperty == "SignedName")
					{
						if (sortAscending)
						{
							changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		public List<OldHistoryChangesetResponse> RetrieveOldHistorySetsByProjectIdExport(int? projectId, double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<OldHistoryChangesetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.OldHistory_ChangeSetsView
								join p in context.Projects
								on h.PROJECT_ID equals p.ProjectId
								select new OldHistoryChangesetResponse
								{
									ProjectId = h.PROJECT_ID,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									ChangeTypeId = h.CHANGETYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									ArtifactId = h.ARTIFACT_ID,
									ArtifactTypeId = h.ARTIFACT_TYPE_ID,
									ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
									ArtifactDesc = h.ARTIFACT_DESC,
									SignatureHash = h.SIGNATURE_HASH,
									UserName = h.USER_NAME,
									Meaning = h.MEANING,
									UserId = h.USER_ID,
									ProjectName = p.Name,
									//FieldName = h.ARTIFACT_TYPE_NAME,
									//Time = "",
									//NameOfSigner = h.SignatureHash != null ? h.UserName : null,
								};


					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<OldHistoryChangesetResponse, bool>> filterClause = CreateFilterExpression<OldHistoryChangesetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<OldHistoryChangesetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
					if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}

					//Get the count
					int count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();

					//Now filter by signature in-memory if needed
					if (filterList != null && filterList.ContainsKey("Signed"))
					{
						if (filterList["Signed"] is int)
						{
							int signedId = (int)filterList["Signed"];
							changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
						}
						if (filterList["Signed"] is MultiValueFilter)
						{
							MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
							changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
						}
					}

					//Now sort by signature in memory if needed
					if (sortProperty == "SignedName")
					{
						if (sortAscending)
						{
							changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		public int OldCountSet(int? projectId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "OldCountSet()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.OldHistory_ChangeSetsView
								select new OldHistoryChangesetResponse
								{
									ProjectId = h.PROJECT_ID,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									ChangeTypeId = h.CHANGETYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									ArtifactId = h.ARTIFACT_ID,
									ArtifactTypeId = h.ARTIFACT_TYPE_ID,
									ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
									ArtifactDesc = h.ARTIFACT_DESC,
									SignatureHash = h.SIGNATURE_HASH,
									UserName = h.USER_NAME,
									Meaning = h.MEANING,
									UserId = h.USER_ID,
								};


					//Filter by project if specified
					if (projectId.HasValue && projectId > 0)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<OldHistoryChangesetResponse, bool>> filterClause = CreateFilterExpression<OldHistoryChangesetResponse>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<OldHistoryChangesetResponse>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public int AuditTrailCountSet(int? projectId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = "CountSet()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int count = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};
					//Filter by project if specified
					//if (projectId.HasValue && projectId > 0)
					//{
					//	query = query.Where(h => h.ProjectId == projectId.Value);
					//}

					//Add the dynamic filters
					if (filters != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Gets a count of changes for all test steps for the given test case Id.</summary>
		/// <param name="testCaseId">The test case ID to pull changes for.</param>
		/// <param name="filters">And filters needed.</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns>The count of test step changes.</returns>
		internal int CountStepsForTestCase(int projectId, int testCaseId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = CLASS_NAME + "CountStepsForTestCase(int,hash)";
			Logger.LogEnteringEvent(METHOD_NAME);

			int count = 0;
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of test step ids in a test case
					var tsQuery = from t in context.TestSteps
								  where t.TestCaseId == testCaseId
								  select t.TestStepId;

					//Build the base history query
					var query = from h in context.HistoryChangesView
								where h.ArtifactTypeId == (int)ArtifactTypeEnum.TestStep &&
									tsQuery.Contains(h.ArtifactId)
								select h;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeView, bool>> filterClause = CreateFilterExpression<HistoryChangeView>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeView>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}
			}
			catch (Exception ex)
			{
				//Log error and return 0.
				Logger.LogErrorEvent(METHOD_NAME, ex);
				count = 0;
			}

			Logger.LogEnteringEvent(METHOD_NAME);
			return count;
		}

		/// <summary>Gets a count of changes for all requirement steps for the given requirement Id.</summary>
		/// <param name="requirementId">The requirement ID to pull step changes for.</param>
		/// <param name="filters">And filters needed.</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns>The count of requirement step changes.</returns>
		internal int CountStepsForRequirement(int projectId, int requirementId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = CLASS_NAME + "CountStepsForRequirement(int,hash)";
			Logger.LogEnteringEvent(METHOD_NAME);

			int count = 0;
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of requirement step ids in a requirement
					var rsQuery = from r in context.RequirementSteps
								  where r.RequirementId == requirementId
								  select r.RequirementStepId;

					//Build the base history query
					var query = from h in context.HistoryChangesView
								where h.ArtifactTypeId == (int)ArtifactTypeEnum.RequirementStep &&
									rsQuery.Contains(h.ArtifactId)
								select h;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeView, bool>> filterClause = CreateFilterExpression<HistoryChangeView>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeView>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}
			}
			catch (Exception ex)
			{
				//Log error and return 0.
				Logger.LogErrorEvent(METHOD_NAME, ex);
				count = 0;
			}

			Logger.LogEnteringEvent(METHOD_NAME);
			return count;
		}

		/// <summary>Gets a count of changes for all risk mitigations for the given risk Id.</summary>
		/// <param name="riskId">The requirement ID to pull step changes for.</param>
		/// <param name="filters">And filters needed.</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns>The count of requirement step changes.</returns>
		internal int CountStepsForRisk(int projectId, int riskId, Hashtable filters, double utcOffset)
		{
			const string METHOD_NAME = CLASS_NAME + "CountStepsForRisk()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int count = 0;
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of risk mitigation IDs in a requirement
					List<int> rmItems = context.RiskMitigations
						.Where(r => r.RiskId == riskId)
						.Select(t => t.RiskMitigationId)
						.ToList();

					//Build the base history query
					var query = from h in context.HistoryChangesView
								where h.ArtifactTypeId == (int)ArtifactTypeEnum.RiskMitigation &&
									rmItems.Contains(h.ArtifactId)
								select h;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeView, bool>> filterClause = CreateFilterExpression<HistoryChangeView>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeView>)query.Where(filterClause);
						}
					}

					//Get the count
					count = query.Count();
				}
			}
			catch (Exception ex)
			{
				//Log error and return 0.
				Logger.LogErrorEvent(METHOD_NAME, ex);
				count = 0;
			}

			Logger.LogEnteringEvent(METHOD_NAME);
			return count;
		}

		/// <summary>Retrieves the audit history log associated with a particular artifact</summary>
		/// <param name="artifactId">The ID of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <param name="filterList">The list of filters to apply</param>
		/// <param name="sortProperty">What field to sort on</param>
		/// <param name="sortAscending">What direction to sort</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="includeStepChanges">Whether to include step changes, if artifact is a requirement or test case. Default: FALSE</param>
		/// <param name="paginationSize">The number of rows to return</param>
		/// <param name="projectId">The id of the current project</param>
		/// <returns>A history list</returns>
		public List<HistoryChangeSetResponse> RetrieveByArtifactId(int projectId, int artifactId, ArtifactTypeEnum artifactType, string sortProperty, bool sortAscending, Hashtable filterList, int startRow, int paginationSize, double utcOffset, bool includeStepChanges = false)
		{
			const string METHOD_NAME = "RetrieveByArtifactId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetResponse> historyChanges;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
									DetailId = h.ARTIFACT_HISTORY_ID,
								};

					if (artifactType == ArtifactTypeEnum.TestCase && includeStepChanges)
					{
						//See if we need to include test step changes
						var tsQuery = from p in context.TestSteps
									  where p.TestCaseId == artifactId
									  select p.TestStepId;
						List<int> testStepIds = tsQuery.ToList();

						//Now add them to the history query
						query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType || h.ArtifactId == artifactId && h.ArtifactTypeId == (int)ArtifactTypeEnum.TestCaseSignature) || (h.ArtifactTypeId == (int)ArtifactTypeEnum.TestStep && testStepIds.Contains(h.ArtifactId)));

						//** The following code does it in one query but the SQL generated had major performance issues **
						//query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType) || (h.ArtifactTypeId == (int)ArtifactTypeEnum.TestStep && tsQuery.Contains(h.ArtifactId)));
					}
					else if (artifactType == ArtifactTypeEnum.Requirement && includeStepChanges)
					{
						//See if we need to include requirement step changes
						var rsQuery = from p in context.RequirementSteps
									  where p.RequirementId == artifactId
									  select p.RequirementStepId;
						List<int> requirementStepIds = rsQuery.ToList();

						//Now add them to the history query
						query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType || h.ArtifactId == artifactId && h.ArtifactTypeId == (int)ArtifactTypeEnum.RequirementSignature) || (h.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.RequirementStep && requirementStepIds.Contains(h.ArtifactId)));

						//** The following code does it in one query but the SQL generated had major performance issues **
						//query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType) || (h.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.RequirementStep && rsQuery.Contains(h.ArtifactId)));
					}
					else if (artifactType == DataModel.Artifact.ArtifactTypeEnum.Risk && includeStepChanges)
					{
						//See if we need to include risk mitigation changes
						var rmQuery = from p in context.RiskMitigations
									  where p.RiskId == artifactId
									  select p.RiskMitigationId;
						List<int> riskMitigaionIds = rmQuery.ToList();

						//Now add them to the history query
						query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType || h.ArtifactId == artifactId && h.ArtifactTypeId == (int)ArtifactTypeEnum.RiskSignature) || (h.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.RiskMitigation && riskMitigaionIds.Contains(h.ArtifactId)));

						//** The following code does it in one query but the SQL generated had major performance issues **
						//query = query.Where(h => (h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType) || (h.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.RiskMitigation && rmQuery.Contains(h.ArtifactId)));
					}
					else
					{
						//Just the primary filter is needed
						query = query.Where(h => h.ArtifactId == artifactId && h.ArtifactTypeId == (int)artifactType);
					}

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, null, historyChangeViewFilterHandler);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.DetailId);
					}
					else
					{
						//Convert the EF extension property to the actual physical name
						if (sortProperty == "ChangeSetTypeName")
						{
							sortProperty = "ChangeTypeName";
						}

						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ArtifactId");
					}

					//Get the count
					int count = query.Count();


					var changeSets_1 = query.ToList();

					foreach (var c in changeSets_1)
					{
						DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
						c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					}

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
						}
					}
					//Now sort by Time in memory if needed
					if (sortProperty == "Time")
					{
						if (sortAscending)
						{
							changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
					}

					query = changeSets_1.AsQueryable();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					historyChanges = query.ToList();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the changes
				return historyChanges;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the audit history log associated with a particular ChangeSet</summary>
		/// <param name="changeSetId">The changeset ID to retrieve</param>
		/// <param name="filterList">The list of filters to apply</param>
		/// <param name="sortProperty">What field to sort on</param>
		/// <param name="sortAscending">What direction to sort</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="paginationSize">The number of rows to return</param>
		/// <returns>A history detail list</returns>
		public List<HistoryChangeSetResponse> RetrieveByChangeSetId(int projectId, long changeSetId, string sortProperty, bool sortAscending, Hashtable filterList, int startRow, int paginationSize, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveByChangeSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetResponse> historyChanges;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
									DetailId = h.ARTIFACT_HISTORY_ID
								};

					//Add the dynamic filters
					if (filterList != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}
					if(changeSetId > 1)
					{
						query = query.Where(h => h.ChangeSetId == changeSetId);
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ArtifactId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "DetailId");
					}

					//Get the count
					int count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					historyChanges = query
						.Skip(startRow - 1)
						.Take(paginationSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return historyChanges;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Gets a list of Changesets for the specified Project ID (or entire system if unset)</summary>
		/// <param name="projectId">The Project ID (optional)</param>
		/// <param name="filterList">List of filters to filter the list by. Default: NULL</param>
		/// <param name="paginationSize">Size of a page. -1 to list all. Default: -1</param>
		/// <param name="sortAscending">Whether to sort ascending or not. Default: TRUE</param>
		/// <param name="sortProperty">The property to sort on. Default: ID</param>
		/// <param name="utcOffset">
		/// <param name="startRow">The start row. Default: 1</param>
		/// <returns>A history dataset</returns>
		public List<HistoryChangeSetView> RetrieveSetsByProjectId(int? projectId, double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetView> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.HistoryChangeSetsView
								select h;

					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Add the dynamic filters
					if (filterList != null)
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetView, bool>> filterClause = CreateFilterExpression<HistoryChangeSetView>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetView>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
					if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}

					//Get the count
					int count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();

					//Now filter by signature in-memory if needed
					if (filterList != null && filterList.ContainsKey("Signed"))
					{
						if (filterList["Signed"] is int)
						{
							int signedId = (int)filterList["Signed"];
							changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
						}
						if (filterList["Signed"] is MultiValueFilter)
						{
							MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
							changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
						}
					}

					//Now sort by signature in memory if needed
					if (sortProperty == "SignedName")
					{
						if (sortAscending)
						{
							changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public string ConvertDateToTime(DateTime date)
		{
			DateTime utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
			return string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
		}

		public List<HistoryChangeSetResponse> RetrieveSetsByProjectId1(int? projectId, int? year, int? month, int? day, double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};

					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Filter by year, if needed..
					if (year.HasValue)
					{
						query = query.Where(h => h.ChangeDate.Year == year);
					}

					//Filter by month, if needed..
					if (month.HasValue)
					{
						query = query.Where(h => h.ChangeDate.Month == month);
					}

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
					if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}



					//Get the count
					int count = query.Count();

					var changeSets_1 = query.ToList();

					foreach (var c in changeSets_1)
					{
						DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
						c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					}

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
						}
					}
					//Now sort by Time in memory if needed
					if (sortProperty == "Time")
					{
						if (sortAscending)
						{
							changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
					}

					query = changeSets_1.AsQueryable();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();

					//Now filter by signature in-memory if needed
					if (filterList != null && filterList.ContainsKey("Signed"))
					{
						if (filterList["Signed"] is int)
						{
							int signedId = (int)filterList["Signed"];
							changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
						}
						if (filterList["Signed"] is MultiValueFilter)
						{
							MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
							changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
						}
					}

					//Now sort by signature in memory if needed
					if (sortProperty == "SignedName")
					{
						if (sortAscending)
						{
							changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		//public List<HistoryChangeSetResponse> RetrieveOldHistorySetsByProjectId(int? projectId, int? year, int? month, int? day, double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		//{
		//	const string METHOD_NAME = "RetrieveOldHistorySetsByProjectId()";
		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

		//	try
		//	{
		//		List<HistoryChangeSetResponse> changeSets;

		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			//Create the base query
		//			var query = from h in context.OldHistoryChangeSetsView
		//						join p in context.Projects
		//						on h.PROJECT_ID equals p.ProjectId
		//						select new HistoryChangeSetResponse
		//						{
		//							ProjectId = h.PROJECT_ID,
		//							ChangeSetId = h.CHANGESET_ID,
		//							ChangeDate = h.CHANGE_DATE,
		//							TimeZone = "UTC",
		//							ChangeTypeId = h.CHANGETYPE_ID,
		//							ChangeTypeName = h.CHANGETYPE_NAME,
		//							ArtifactId = h.ARTIFACT_ID,
		//							ArtifactTypeId = h.ARTIFACT_TYPE_ID,
		//							ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
		//							ArtifactDesc = h.ARTIFACT_DESC,
		//							SignatureHash = h.SIGNATURE_HASH,
		//							UserName = h.USER_NAME,
		//							Meaning = h.MEANING,
		//							NameOfSigner = h.SIGNATURE_HASH != null ? h.USER_NAME : null,
		//							OldValue = h.OLD_VALUE,
		//							NewValue = h.NEW_VALUE,
		//							ProjectName = p.Name,
		//							FieldName = h.FIELD_NAME,
		//							FieldId = h.FIELD_ID,
		//							Time = "",
		//							UserId = h.USER_ID,
		//						};

		//			//Filter our the project, if needed..
		//			if (projectId.HasValue)
		//			{
		//				query = query.Where(h => h.ProjectId == projectId.Value);
		//			}

		//			//Filter by year, if needed..
		//			if (year.HasValue)
		//			{
		//				query = query.Where(h => h.ChangeDate.Year == year);
		//			}

		//			//Filter by month, if needed..
		//			if (month.HasValue)
		//			{
		//				query = query.Where(h => h.ChangeDate.Month == month);
		//			}

		//			//Add the dynamic filters
		//			if (filterList != null && !filterList.ContainsKey("Time"))
		//			{
		//				//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
		//				List<string> ignoreList = new List<string>() { "Signed" };

		//				//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
		//				Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
		//				if (filterClause != null)
		//				{
		//					query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
		//				}
		//			}

		//			//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
		//			if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
		//			{
		//				//Default to sorting by change date descending
		//				query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
		//			}
		//			else
		//			{
		//				//We always sort by the physical ID to guarantee stable sorting
		//				string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
		//				query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
		//			}



		//			//Get the count
		//			int count = query.Count();

		//			var changeSets_1 = query.ToList();

		//			foreach (var c in changeSets_1)
		//			{
		//				DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
		//				c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
		//			}

		//			if (filterList != null && filterList.ContainsKey("Time"))
		//			{
		//				if (filterList["Time"] is string)
		//				{
		//					string time = (string)filterList["Time"];
		//					changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
		//				}
		//			}
		//			//Now sort by Time in memory if needed
		//			if (sortProperty == "Time")
		//			{
		//				if (sortAscending)
		//				{
		//					changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
		//				}
		//				else
		//				{
		//					changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
		//				}
		//			}

		//			query = changeSets_1.AsQueryable();

		//			//Make pagination is in range
		//			if (startRow < 1 || startRow > count)
		//			{
		//				startRow = 1;
		//			}

		//			//Execute the query
		//			query = query.Skip(startRow - 1);
		//			if (paginationSize > 0)
		//				query = query.Take(paginationSize);
		//			changeSets = query.ToList();

		//			//Now filter by signature in-memory if needed
		//			if (filterList != null && filterList.ContainsKey("Signed"))
		//			{
		//				if (filterList["Signed"] is int)
		//				{
		//					int signedId = (int)filterList["Signed"];
		//					changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
		//				}
		//				if (filterList["Signed"] is MultiValueFilter)
		//				{
		//					MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
		//					changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
		//				}
		//			}

		//			//Now sort by signature in memory if needed
		//			if (sortProperty == "SignedName")
		//			{
		//				if (sortAscending)
		//				{
		//					changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
		//				}
		//				else
		//				{
		//					changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
		//				}
		//			}
		//		}

		//		Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//		return changeSets;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		Logger.Flush();
		//		throw;
		//	}
		//}

		/// <summary>Gets a list of Changesets for the specified Project ID (or entire system if unset)</summary>
		/// <param name="filterList">List of filters to filter the list by. Default: NULL</param>
		/// <param name="paginationSize">Size of a page. -1 to list all. Default: -1</param>
		/// <param name="sortAscending">Whether to sort ascending or not. Default: TRUE</param>
		/// <param name="sortProperty">The property to sort on. Default: ID</param>
		/// <param name="utcOffset">
		/// <param name="startRow">The start row. Default: 1</param>
		/// <returns>A history dataset</returns>
		public List<HistoryChangeSetResponse> RetrieveSets(double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSetResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<HistoryChangeSetResponse, bool>> filterClause = CreateFilterExpression<HistoryChangeSetResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<HistoryChangeSetResponse>)query.Where(filterClause);
						}
					}

					//Add the dynamic sort, signatures need to be in-memory sorted since they are calculated fields
					if (String.IsNullOrEmpty(sortProperty) || sortProperty == "SignedName")
					{
						//Default to sorting by change date descending
						query = query.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "ChangeSetId");
					}

					//Get the count
					int count = query.Count();


					var changeSets_1 = query.ToList();

					foreach (var c in changeSets_1)
					{
						DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
						c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					}

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets_1 = changeSets_1.Where(c => c.Time.Contains(time)).ToList();
						}
					}
					//Now sort by Time in memory if needed
					if (sortProperty == "Time")
					{
						if (sortAscending)
						{
							changeSets_1 = changeSets_1.OrderBy(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets_1 = changeSets_1.OrderByDescending(h => h.Time).ThenBy(h => h.ChangeSetId).ToList();
						}
					}

					query = changeSets_1.AsQueryable();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					query = query.Skip(startRow - 1);
					if (paginationSize > 0)
						query = query.Take(paginationSize);
					changeSets = query.ToList();

					//foreach (var c in changeSets)
					//{
					//	DateTime utcDate = DateTime.SpecifyKind(c.ChangeDate, DateTimeKind.Utc);
					//	c.Time = string.Format("{0:hh:mm:ss tt}", utcDate.ToLocalTime());
					//}

					//if (filterList != null && filterList.ContainsKey("Time"))
					//{
					//	if (filterList["Time"] is string)
					//	{
					//		string time = (string)filterList["Time"];
					//		changeSets = changeSets.Where(c => c.Time.Contains(time)).ToList();
					//	}
					//}

					//Now filter by signature in-memory if needed
					if (filterList != null && filterList.ContainsKey("Signed"))
					{
						if (filterList["Signed"] is int)
						{
							int signedId = (int)filterList["Signed"];
							changeSets = changeSets.Where(c => c.Signed == signedId).ToList();
						}
						if (filterList["Signed"] is MultiValueFilter)
						{
							MultiValueFilter mvf = (MultiValueFilter)filterList["Signed"];
							changeSets = changeSets.Where(c => mvf.Values.Contains(c.Signed)).ToList();
						}
					}

					//Now sort by signature in memory if needed
					if (sortProperty == "SignedName")
					{
						if (sortAscending)
						{
							changeSets = changeSets.OrderBy(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
						else
						{
							changeSets = changeSets.OrderByDescending(h => h.SignedName).ThenBy(h => h.ChangeSetId).ToList();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single changeset by its id</summary>
		/// <param name="changeSetId">The ID of the changeset</param>
		/// <returns>A history changeset</returns>
		public HistoryChangeSetView RetrieveChangeSetById2(long changeSetId)
		{
			const string METHOD_NAME = "RetrieveChangeSetById2()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				HistoryChangeSetView changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryChangeSetsView
								where h.ChangeSetId == changeSetId
								select h;

					changeSet = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single changeset by its id along with associated change records</summary>
		/// <param name="changeSetId">The ID of the changeset</param>
		/// <param name="includeDetails">Should we include the details</param>
		/// <param name="includeAssociations">Should we include the association changes</param>
		/// <param name="includePositions">Should we include the position changes</param>
		/// <returns>A history changeset</returns>
		public HistoryChangeSetResponse RetrieveChangeSetById(long changeSetId, bool includeDetails, bool includeAssociations = false, bool includePositions = false)
		{
			const string METHOD_NAME = "RetrieveChangeSetById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				HistoryChangeSetResponse changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryChangeSetsView
								join p in context.Projects
								on h.ProjectId equals p.ProjectId
								select new HistoryChangeSetResponse
								{
									ProjectId = h.ProjectId,
									ChangeSetId = h.ChangeSetId,
									ChangeDate = h.ChangeDate,
									TimeZone = "UTC",
									ChangeTypeId = h.ChangeTypeId,
									ChangeTypeName = h.ChangeTypeName,
									ArtifactId = h.ArtifactId,
									ArtifactTypeId = h.ArtifactTypeId,
									ArtifactTypeName = h.ArtifactTypeName,
									ArtifactDesc = h.ArtifactDesc,
									SignatureHash = h.SignatureHash,
									UserName = h.UserName,
									Meaning = h.Meaning,
									NameOfSigner = h.SignatureHash != null ? h.UserName : null,
									OldValue = h.OLD_VALUE,
									NewValue = h.NEW_VALUE,
									ProjectName = p.Name,
									FieldName = h.FIELD_NAME,
									FieldId = h.FIELD_ID,
									Time = "",
									UserId = h.UserId,
								};
					//if (includeDetails)
					//{
					//	historyChangeSets = historyChangeSets.Include("Details");
					//}
					//if (includeAssociations)
					//{
					//	historyChangeSets = historyChangeSets.Include("AssociationChanges");
					//}
					//if (includePositions)
					//{
					//	historyChangeSets = historyChangeSets.Include("PositionChanges");
					//}

					List<HistoryChangeSetResponse> historyChangeSets = query.ToList();

					changeSet = historyChangeSets.FirstOrDefault(h => h.ChangeSetId == changeSetId);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return changeSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}




		public OldHistoryChangesetResponse RetrieveOldHistoryChangeSetById(long changeSetId, bool includeDetails, bool includeAssociations = false, bool includePositions = false)
		{
			const string METHOD_NAME = "RetrieveOldHistoryChangeSetById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				OldHistoryChangesetResponse changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.OldHistory_ChangeSetsView
								join p in context.Projects
								on h.PROJECT_ID equals p.ProjectId
								select new OldHistoryChangesetResponse
								{
									ProjectId = h.PROJECT_ID,
									ChangeSetId = h.CHANGESET_ID,
									ChangeDate = h.CHANGE_DATE,
									ChangeTypeId = h.CHANGETYPE_ID,
									ChangeTypeName = h.CHANGETYPE_NAME,
									ArtifactId = h.ARTIFACT_ID,
									ArtifactTypeId = h.ARTIFACT_TYPE_ID,
									ArtifactTypeName = h.ARTIFACT_TYPE_NAME,
									ArtifactDesc = h.ARTIFACT_DESC,
									SignatureHash = h.SIGNATURE_HASH,
									UserName = h.USER_NAME,
									Meaning = h.MEANING,
									UserId = h.USER_ID,
									//ProjectName = p.Name,
									//FieldName = h.ARTIFACT_TYPE_NAME,
									//Time = "",
									//NameOfSigner = h.SignatureHash != null ? h.UserName : null,
								};


					List<OldHistoryChangesetResponse> oldHistoryChangesetResponses = query.ToList();

					changeSets = oldHistoryChangesetResponses.FirstOrDefault(h => h.ChangeSetId == changeSetId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the ID of the most recent history FIELD CHANGE entry</summary>
		/// <returns>the id of the most recent entry</returns>
		/// <remarks>This is used by the unit tests to delete history entries created during execution of the various test fixtures</remarks>
		public void RetrieveLatestDetails(out long lastHistId, out long lastSetId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveLatest";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				lastHistId = 1;
				lastSetId = 1;
				//Get the last history item along with its changeset and changeset type
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryDetails
								orderby h.ArtifactHistoryId descending
								select h;

					HistoryDetail historyDetail = query.FirstOrDefault();
					if (historyDetail != null)
					{
						lastHistId = historyDetail.ArtifactHistoryId;
						lastSetId = historyDetail.ChangeSetId;
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Retrieves the ID of the most recent history SET entry</summary>
		/// <returns>the id of the most recent entry</returns>
		/// <remarks>This is used by the unit tests to delete history entries created during execution of the various test fixtures</remarks>
		public void RetrieveLatestSetId(out long lastSetId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveLatestSetId";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				lastSetId = 0;
				//Get the last history item along with its changeset and changeset type
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					lastSetId = context.HistoryChangeSets.Max(c => c.ChangeSetId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}


		/// <summary>Restores the specified artifact back to the specified changeset.</summary>
		/// <param name="artType">The artifact type.</param>
		/// <param name="projectId">the id of the current project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <param name="changeSetId">The changeset ID to roll back to.</param>
		/// <param name="userId">The user doing the rollback.</param>
		/// <param name="rollbackLog">Log of the rollback. (Which fields were rolled back, which weren't, etc.</param>
		/// <returns>The result of the rollback.</returns>
		public RollbackResultEnum RollbackHistory(int projectId, int projectTemplateId, ArtifactTypeEnum artType, int artifactId, long changeSetId, int userId, ref string rollbackLog)
		{
			const string METHOD_NAME = CLASS_NAME + "RollbackHistory()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Set the default.
			RollbackResultEnum retStatus = RollbackResultEnum.Success;

			//Get the artifact.
			rollbackLog += "- Getting current artifact data." + Environment.NewLine;

			DiscussionBase discussionBase = null;
			Artifact artifact = null;   //Used for entity-based artifacts
			Artifact alternateArtifact = null;   //Used for entity-based artifacts where we have a relationship
			ArtifactCustomProperty artifactCustomProperty = null;

			try
			{
				switch (artType)
				{
					case ArtifactTypeEnum.Incident:
						{
							Incident incident = new IncidentManager().RetrieveById(artifactId, false, true);
							incident.StartTracking();
							artifact = incident;
						}
						break;
					case ArtifactTypeEnum.Task:
						{
							Task task = new TaskManager().RetrieveById(artifactId, true);
							task.StartTracking();
							artifact = task;
						}
						break;
					case ArtifactTypeEnum.Risk:
						Risk risk = new RiskManager().Risk_RetrieveById(artifactId, true, true, true);
						risk.StartTracking();
						artifact = risk;
						break;
					case ArtifactTypeEnum.Document:
						{
							ProjectAttachment projectAttachment = new AttachmentManager().RetrieveForProjectById(projectId, artifactId);
							projectAttachment.StartTracking();
							projectAttachment.Attachment.StartTracking();
							artifact = projectAttachment.Attachment;
							alternateArtifact = projectAttachment;
						}
						break;

					case ArtifactTypeEnum.Release:
						{
							ReleaseView releaseView = new ReleaseManager().RetrieveById2(null, artifactId, true);
							Release release = releaseView.ConvertTo<ReleaseView, Release>();
							release.StartTracking();
							artifact = release;
						}
						break;

					case ArtifactTypeEnum.TestCase:
						{
							TestCase testCase = new TestCaseManager().RetrieveById2(null, artifactId, true);
							testCase.StartTracking();
							artifact = testCase;
							break;
						}

					case ArtifactTypeEnum.TestSet:
						{
							TestSet testSet = new TestSetManager().RetrieveById2(null, artifactId, true);
							testSet.StartTracking();
							artifact = testSet;
							break;
						}

					case ArtifactTypeEnum.TestStep:
						{
							TestStep testStep = new TestCaseManager().RetrieveStepById(null, artifactId, true);
							testStep.StartTracking();
							artifact = testStep;
							break;
						}

					case ArtifactTypeEnum.Requirement:
						{
							Requirement requirement = new RequirementManager().RetrieveById3(null, artifactId, true);
							requirement.StartTracking();
							artifact = requirement;
						}
						break;

					case ArtifactTypeEnum.RequirementStep:
						{
							RequirementStep requirementStep = new RequirementManager().RetrieveStepById(artifactId, true, true);
							requirementStep.StartTracking();
							artifact = requirementStep;
						}
						break;

					case ArtifactTypeEnum.TestRun:
						{
							TestRunView testRun = new TestRunManager().RetrieveById(artifactId);
							testRun.StartTracking();
							artifact = testRun;
						}
						break;

					case ArtifactTypeEnum.AutomationHost:
						{
							AutomationHost automationHost = new AutomationManager().RetrieveHostById2(artifactId, true);
							automationHost.StartTracking();
							artifact = automationHost;
						}
						break;
					case ArtifactTypeEnum.RiskMitigation:
						{
							RiskMitigation riskMit = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, true, true);
							riskMit.StartTracking();
							artifact = riskMit;
						}
						break;
					case ArtifactTypeEnum.TestConfigurationSet:
						{
							TestConfigurationSet testConfigurationSet = new TestConfigurationManager().RetrieveSetById(artifactId, true);
							testConfigurationSet.StartTracking();
							artifact = testConfigurationSet;
						}
						break;
					case ArtifactTypeEnum.ProjectBaseline:
						ProjectBaseline baselineManager = new BaselineManager().Baseline_RetrieveById(artifactId);
						baselineManager.StartTracking();
						artifact = baselineManager;
						break;
					case ArtifactTypeEnum.DocumentDiscussion:
						DocumentDiscussion documentDiscussion = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId);
						documentDiscussion.StartTracking();
						discussionBase = documentDiscussion;
						break;
					case ArtifactTypeEnum.RiskDiscussion:
						RiskDiscussion riskDiscussion = new DiscussionManager().RetrieveRiskDiscussionById(artifactId);
						riskDiscussion.StartTracking();
						discussionBase = riskDiscussion;
						break;
					case ArtifactTypeEnum.RequirementDiscussion:
						RequirementDiscussion requirementDiscussion = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId);
						requirementDiscussion.StartTracking();
						discussionBase = requirementDiscussion;
						break;
					case ArtifactTypeEnum.ReleaseDiscussion:
						ReleaseDiscussion releaseDiscussion = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId);
						releaseDiscussion.StartTracking();
						discussionBase = releaseDiscussion;
						break;
					case ArtifactTypeEnum.TestCaseDiscussion:
						TestCaseDiscussion testCaseDiscussion = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId);
						testCaseDiscussion.StartTracking();
						discussionBase = testCaseDiscussion;
						break;
					case ArtifactTypeEnum.TestSetDiscussion:
						TestSetDiscussion testSetDiscussion = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId);
						testSetDiscussion.StartTracking();
						discussionBase = testSetDiscussion;
						break;
					case ArtifactTypeEnum.TaskDiscussion:
						TaskDiscussion taskDiscussion = new DiscussionManager().RetrieveTaskDiscussionById(artifactId);
						taskDiscussion.StartTracking();
						discussionBase = taskDiscussion;
						break;
					case ArtifactTypeEnum.IncidentResolution:
						IncidentResolution incidentResolution = new IncidentManager().Resolution_RetrieveByResolutionId(artifactId);
						incidentResolution.StartTracking();
						//discussionBase = incidentResolution;
						break;
					case ArtifactTypeEnum.TestCaseParameter:
						TestCaseParameter testCaseParameter = new TestCaseManager().RetrieveParameterById(artifactId);
						testCaseParameter.StartTracking();
						//artifact = testCaseParameter;
						break;
					case ArtifactTypeEnum.TestSetParameter:
						TestSetParameter testSetParameter = new TestCaseManager().RetrieveTestSetParameterById(artifactId);
						testSetParameter.StartTracking();
						//artifact = testSetParameter;
						break;
					case ArtifactTypeEnum.ArtifactLink:
						ArtifactLink artifactLink = new ArtifactLinkManager().RetrieveById(artifactId);
						artifactLink.StartTracking();
						artifact = artifactLink;
						break;
					case ArtifactTypeEnum.ReleaseTestCase:
						//strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
						//artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
				}
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to rollback history changes because artifact has been purged!");
				Logger.Flush();
				return RollbackResultEnum.Error;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				return RollbackResultEnum.Error;
			}

			//We need to have at least one artifact.
			try
			{
				if (artifact != null || discussionBase != null)
				{
					try
					{
						//Load the current custom properties.
						artifactCustomProperty = new CustomPropertyManager().ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, artType);
						if (artifactCustomProperty != null)
						{
							artifactCustomProperty.StartTracking();
						}
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
						rollbackLog += "! Could not get custom properties for artifact." + Environment.NewLine;
					}

					//Get all changesets for the artifact, first.
					List<HistoryChangeSetView> historyChangeSets;
					List<HistoryChangeView> historyChanges;
					List<HistoryChangeSetType> changeSetTypes;
					RetrieveByArtifactId(artifactId, artType, out historyChangeSets, out historyChanges, out changeSetTypes);

					//Set flag to determine if we need to undelete or if we need to save the new item.
					bool needsUndelete = false;
					bool needsSaving = false;

					//Order the changesets and loop through each one.
					historyChangeSets = historyChangeSets.OrderByDescending(h => h.ChangeDate).ThenBy(h => h.ChangeSetId).ToList();
					foreach (HistoryChangeSetView historyChangeSet in historyChangeSets
						.Where(hcs => (hcs.ChangeTypeId == (int)ChangeSetTypeEnum.Modified || hcs.ChangeTypeId == (int)ChangeSetTypeEnum.Deleted))
						.OrderByDescending(hcs => hcs.ChangeDate))
					{
						//Basically, we roll back until we reach the ChangeSet ID.
						if (historyChangeSet.ChangeSetId >= changeSetId)
						{
							rollbackLog += "- Undoing Change #" + historyChangeSet.ChangeSetId.ToString() + ":" + Environment.NewLine;
							//And we only care about Modified or Deleted items.
							if ((ChangeSetTypeEnum)historyChangeSet.ChangeTypeId == ChangeSetTypeEnum.Modified)
							{
								try
								{
									//It was a change. Revert the fields.
									//First set the filter on the ArtifactHistory to only show those fields from this change.
									List<HistoryChangeView> changeSetHistoryChanges = historyChanges.Where(h => h.ChangeSetId == historyChangeSet.ChangeSetId).ToList();

									//Counters, for seeing how many fields changed.
									int numFields = changeSetHistoryChanges.Count;
									int numFailed = 0;

									//Loop through each available field in this changeset.
									foreach (HistoryChangeView rowFieldChange in changeSetHistoryChanges)
									{
										//Get the field name and set out booleans..
										string strField = rowFieldChange.FieldName;
										bool fieldChanged = false;

										//See if we have a custom field
										bool chkCustomField = (!rowFieldChange.FieldId.HasValue && rowFieldChange.CustomPropertyId.HasValue);

										//Okay, let's try to revert the custom field.

										#region Revert Custom Field

										if (chkCustomField && artifactCustomProperty != null)
										{
											try
											{
												//Get the id of the custom property
												int customPropertyId = rowFieldChange.CustomPropertyId.Value;

												//For custom properties the values are all serialized as strings so we don't
												//need to really use the other values (OldValueInt/Date)
												if (string.IsNullOrWhiteSpace(rowFieldChange.OldValue))
												{
													artifactCustomProperty[strField] = null;
												}
												else
												{
													artifactCustomProperty[strField] = rowFieldChange.OldValue;
												}

												//If we get this far, the field's been updated properly.
												fieldChanged = true;
												needsSaving = true;
												rollbackLog += "-- Successfully rolled back custom field '" + rowFieldChange.FieldName + "'" + Environment.NewLine;
											}
											catch (Exception ex)
											{
												Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
												rollbackLog += "-! Error setting custom field '" + rowFieldChange.FieldName + "'" + Environment.NewLine;
												numFailed++;
											}
										}
										#endregion

										//Now, if needed, revert the real field.
										#region Revert Standard Field
										if (!fieldChanged)
										{
											try
											{
												if (artifact != null)
												{
													if (artifact.ContainsProperty(strField))
													{
														//See what type of field it is..
														if (rowFieldChange.OldValueInt.HasValue || rowFieldChange.NewValueInt.HasValue) //It's an integer.
														{
															artifact[strField] = rowFieldChange.OldValueInt;
														}
														else if (rowFieldChange.OldValueDate.HasValue || rowFieldChange.NewValueDate.HasValue) //It's a date.
														{
															artifact[strField] = rowFieldChange.OldValueDate;
														}
														else //It's text.
														{
															if (strField == "DaysNonWorking" || strField == "ResourceCount"|| strField== "EstimatePoints")
															{
																artifact[strField] = Convert.ToDecimal(rowFieldChange.OldValue);
															}
															else if (strField == "IsActive")
															{
																if (rowFieldChange.OldValue == "True")
																{
																	artifact[strField] = true;
																}
																else
																{
																	artifact[strField] = false;
																}
															}
															else
															{
																artifact[strField] = rowFieldChange.OldValue;
															}
														}
													}
													else if (alternateArtifact != null && alternateArtifact.ContainsProperty(strField))
													{
														//Some artifacts have a related artifact that we should use
														//See what type of field it is..
														if (rowFieldChange.OldValueInt.HasValue || rowFieldChange.NewValueInt.HasValue) //It's an integer.
														{
															alternateArtifact[strField] = rowFieldChange.OldValueInt;
														}
														else if (rowFieldChange.OldValueDate.HasValue || rowFieldChange.NewValueDate.HasValue) //It's a date.
														{
															alternateArtifact[strField] = rowFieldChange.OldValueDate;
														}
														else //It's text.
														{
															alternateArtifact[strField] = rowFieldChange.OldValue;
														}
													}
												}
												else if (discussionBase != null)
												{
													if (discussionBase.ContainsProperty(strField))
													{
														//See what type of field it is..
														if (rowFieldChange.OldValueInt.HasValue || rowFieldChange.NewValueInt.HasValue) //It's an integer.
														{
															discussionBase[strField] = rowFieldChange.OldValueInt;
														}
														else if (rowFieldChange.OldValueDate.HasValue || rowFieldChange.NewValueDate.HasValue) //It's a date.
														{
															discussionBase[strField] = rowFieldChange.OldValueDate;
														}
														else //It's text.
														{
															discussionBase[strField] = rowFieldChange.OldValue;
														}
													}
													else if (alternateArtifact != null && alternateArtifact.ContainsProperty(strField))
													{
														//Some artifacts have a related artifact that we should use
														//See what type of field it is..
														if (rowFieldChange.OldValueInt.HasValue || rowFieldChange.NewValueInt.HasValue) //It's an integer.
														{
															alternateArtifact[strField] = rowFieldChange.OldValueInt;
														}
														else if (rowFieldChange.OldValueDate.HasValue || rowFieldChange.NewValueDate.HasValue) //It's a date.
														{
															alternateArtifact[strField] = rowFieldChange.OldValueDate;
														}
														else //It's text.
														{
															alternateArtifact[strField] = rowFieldChange.OldValue;
														}
													}
												}

												//Field was updated. Yay!
												needsSaving = true;
												fieldChanged = true;
												rollbackLog += "-- Successfully rolled back standard field '" + rowFieldChange.FieldCaption + "'" + Environment.NewLine;
											}
											catch (Exception ex)
											{
												Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
												rollbackLog += "-! Error setting standard field '" + rowFieldChange.FieldCaption + "'" + Environment.NewLine;
												numFailed++;
											}
										}
										#endregion
									}
									//Set the status accordingly.
									if (numFields > 0 && numFailed >= numFields)
										retStatus = RollbackResultEnum.Error;
									else if (numFailed > 0 && numFields > numFailed && retStatus == RollbackResultEnum.Success)
										retStatus = RollbackResultEnum.Warning;

								}
								catch (Exception ex)
								{
									Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
									rollbackLog += "-! Error getting fields, could not restore this change.";
									retStatus = RollbackResultEnum.Error;
								}
							}
							else if ((ChangeSetTypeEnum)historyChangeSet.ChangeTypeId == ChangeSetTypeEnum.Deleted)
							{
								//Is was a delete. Undelete it.
								needsUndelete = true;
								rollbackLog += "-- Marking artifact for undelete." + Environment.NewLine;
							}
						}
					}

					//Now we need to save the datasets/entities
					if (needsSaving)
					{
						if (artifactCustomProperty != null)
						{
							rollbackLog += "- Saving artifact custom properties." + Environment.NewLine;
							try
							{
								new CustomPropertyManager().ArtifactCustomProperty_Save(artifactCustomProperty, userId, changeSetId);
							}
							catch (Exception ex)
							{
								Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
								rollbackLog += "! Error saving custom properties." + Environment.NewLine;
							}
						}

						rollbackLog += "- Saving artifact." + Environment.NewLine;
						try
						{
							if (artifact != null)
							{
								switch (artType)
								{
									case ArtifactTypeEnum.Incident:
										{
											Incident incident = (Incident)artifact;
											new IncidentManager().Update(incident, userId, changeSetId);
										}
										break;

									case ArtifactTypeEnum.Task:
										{
											Task task = (Task)artifact;
											new TaskManager().Update(task, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.Risk:
										{
											Risk risk = (Risk)artifact;
											new RiskManager().Risk_Update(risk, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.Document:
										{
											Attachment attachment = (Attachment)artifact;
											ProjectAttachment projectAttachment = (ProjectAttachment)alternateArtifact;
											new AttachmentManager().Update(projectAttachment, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.TestCase:
										{
											TestCase testCase = (TestCase)artifact;
											new TestCaseManager().Update(testCase, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.TestStep:
										{
											TestStep testStep = (TestStep)artifact;
											new TestCaseManager().UpdateStep(testStep, userId, changeSetId);
										}
										break;

									case ArtifactTypeEnum.TestSet:
										{
											TestSet testSet = (TestSet)artifact;
											new TestSetManager().Update(testSet, userId, changeSetId);
										}
										break;

									case ArtifactTypeEnum.TestRun:
										{
											TestConfigurationSet testConfigurationSet = (TestConfigurationSet)artifact;
											new TestConfigurationManager().UpdateSet(testConfigurationSet, userId, changeSetId);
										}
										break;

									case ArtifactTypeEnum.Release:
										{
											Release release = (Release)artifact;
											new ReleaseManager().Update(new List<Release>() { release }, userId, release.ProjectId, changeSetId);
										}
										break;

									case ArtifactTypeEnum.Requirement:
										{
											Requirement requirement = (Requirement)artifact;
											new RequirementManager().Update(userId, requirement.ProjectId, new List<Requirement>() { requirement }, changeSetId);
										}
										break;
									case ArtifactTypeEnum.RequirementStep:
										{
											RequirementStep requirementStep = (RequirementStep)artifact;
											new RequirementManager().UpdateStep(requirementStep.ProjectId, requirementStep, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.AutomationHost:
										{
											AutomationHost automationHost = (AutomationHost)artifact;
											new AutomationManager().UpdateHost(automationHost, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.RiskMitigation:
										{
											new RiskManager().RiskMitigation_Update(((RiskMitigation)artifact).ProjectId, (RiskMitigation)artifact, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.TestConfigurationSet:
										{
											TestConfigurationSet testConfigurationSet = (TestConfigurationSet)artifact;
											new TestConfigurationManager().UpdateSet(testConfigurationSet, userId, changeSetId);
										}
										break;
									case ArtifactTypeEnum.ArtifactLink:
										{
											ArtifactLink artifactLink = (ArtifactLink)artifact;
											new ArtifactLinkManager().Update(artifactLink, userId, projectId, changeSetId);
										}
										break;
								}
							}
							else if (discussionBase != null)
							{
								switch (artType)
								{
									case ArtifactTypeEnum.RequirementDiscussion:
										{
											RequirementDiscussion requirementDiscussion = (RequirementDiscussion)discussionBase;
											new DiscussionManager().UpdateRequirementDiscussion(requirementDiscussion, userId, changeSetId);
										}
										break;
								}
							}
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
							rollbackLog += "! Error saving artifact." + Environment.NewLine;
							retStatus = RollbackResultEnum.Error;
						}
					}

					//Do our undelete, if needed. Needs to come after the main changes because otherwise the artifact save
					//would undo the un-delete (!!)
					if (needsUndelete)
					{
						try
						{
							bool shouldUpdateHist = (needsUndelete && !needsSaving);

							rollbackLog += "- Undeleting artifact." + Environment.NewLine;
							switch (artType)
							{
								case ArtifactTypeEnum.Incident:
									new IncidentManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.Task:
									new TaskManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.TestCase:
									new TestCaseManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.TestStep:
									new TestCaseManager().UndeleteStep(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.Release:
									new ReleaseManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.TestSet:
									new TestSetManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.Requirement:
									new RequirementManager().UnDelete(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.RequirementStep:
									new RequirementManager().UndeleteStep(projectId, artifactId, userId, changeSetId);
									break;
								case ArtifactTypeEnum.AutomationHost:
									new AutomationManager().UnDeleteHost(artifactId, userId, changeSetId, shouldUpdateHist);
									break;
								case ArtifactTypeEnum.RiskMitigation:
									new RiskManager().RiskMitigation_Undelete(projectId, artifactId, userId, changeSetId);
									break;
								case ArtifactTypeEnum.Risk:
									new RiskManager().Risk_UnDelete(artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.RequirementDiscussion:
									new DiscussionManager().RequirementDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;

								case ArtifactTypeEnum.DocumentDiscussion:
									new DiscussionManager().DocumentDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.RiskDiscussion:
									new DiscussionManager().RiskDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.ReleaseDiscussion:
									new DiscussionManager().ReleaseDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.TestCaseDiscussion:
									new DiscussionManager().TestCaseDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.TestSetDiscussion:
									new DiscussionManager().TestSetDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
								case ArtifactTypeEnum.TaskDiscussion:
									new DiscussionManager().TaskDiscussionUnDelete(projectId, artifactId, userId, changeSetId, true);
									break;
									//case ArtifactTypeEnum.IncidentResolution:
									//	IncidentResolution incidentResolution = new IncidentManager().Resolution_RetrieveByResolutionId(artifactId);
									//	incidentResolution.StartTracking();
									//	//discussionBase = incidentResolution;
									//	break;
									//case ArtifactTypeEnum.TestCaseParameter:
									//	TestCaseParameter testCaseParameter = new TestCaseManager().RetrieveParameterById(artifactId);
									//	testCaseParameter.StartTracking();
									//	//artifact = testCaseParameter;
									//	break;
									//case ArtifactTypeEnum.TestSetParameter:
									//	TestSetParameter testSetParameter = new TestCaseManager().RetrieveTestSetParameterById(artifactId);
									//	testSetParameter.StartTracking();
									//	//artifact = testSetParameter;
									//	break;
							}
						}
						catch
						{
							retStatus = RollbackResultEnum.Error;
						}
					}
				}
				else
				{
					rollbackLog += "! Artifact not found, cannot be recovered.";
					retStatus = RollbackResultEnum.Error;
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				rollbackLog += "! Artifact not found, cannot be recovered.";
				retStatus = RollbackResultEnum.Error;
			}

			return retStatus;
		}

		/// <summary>
		/// Logs a single change set
		/// </summary>
		/// <param name="historyChangeSet">A single changeset</param>
		/// <returns></returns>
		internal long Insert(HistoryChangeSet historyChangeSet)
		{
			List<HistoryChangeSet> historyChangeSets = new List<HistoryChangeSet>();
			historyChangeSets.Add(historyChangeSet);
			return Insert(historyChangeSets);
		}

		/// <summary>Inserts a History Change set. Needs to have an unsaved ChangeSet row(s), with 0 or more HistoryField rows.</summary>
		/// <param name="historyChangeSets">The changesets.</param>
		/// <returns>The id of the FIRST ChangeSet inserted.</returns>
		protected long Insert(List<HistoryChangeSet> historyChangeSets, string caption = null)
		{
			const string METHOD_NAME = "Insert";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//All we're doing is saving this entity
				long historyChangeSetId = 0;
				if (historyChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (HistoryChangeSet historyChangeSet in historyChangeSets)
						{
							//Ignore changesets for modified cases where no fields listed
							if (historyChangeSet.ChangeTypeId != (int)ChangeSetTypeEnum.Modified || historyChangeSet.Details.Count > 0)
							{
								TST_HISTORY_CHANGESET historyChangeset = new TST_HISTORY_CHANGESET();

								historyChangeset.USER_ID = historyChangeSet.UserId;
								historyChangeset.ARTIFACT_TYPE_ID = historyChangeSet.ArtifactTypeId;
								historyChangeset.ARTIFACT_ID = historyChangeSet.ArtifactId;
								historyChangeset.CHANGE_DATE = historyChangeSet.ChangeDate;
								historyChangeset.CHANGETYPE_ID = historyChangeSet.ChangeTypeId;
								historyChangeset.PROJECT_ID = historyChangeSet.ProjectId;
								historyChangeset.REVERT_ID = historyChangeSet.RevertId;
								historyChangeset.ARTIFACT_DESC = historyChangeSet.ArtifactDesc;
								//Get the permanent change information
								if (historyChangeSet.SignatureHash != null)
								{
									//We HASH this to avoid tampering
									string signature = historyChangeSet.UserId + ":" + historyChangeSet.ArtifactTypeId + ":" + historyChangeSet.ArtifactId + ":" + historyChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

									//Create the hash using SHA256
									historyChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
									historyChangeset.SIGNATURE_HASH = SimpleHash.GetHashSha256(signature);
								}
								historyChangeset.MEANING = historyChangeSet.Meaning;

								context.TST_HISTORY_CHANGESET.Add(historyChangeset);
							}
						}
						context.SaveChanges();
						historyChangeSetId = context.TST_HISTORY_CHANGESET.OrderByDescending(q => q.CHANGESET_ID)
.FirstOrDefault().CHANGESET_ID;

						foreach (HistoryChangeSet historyChangeSet in historyChangeSets)
						{
							//Ignore changesets for modified cases where no fields listed
							if (historyChangeSet.ChangeTypeId != (int)ChangeSetTypeEnum.Modified || historyChangeSet.Details.Count > 0)
							{
								TST_HISTORY_DETAIL historyDetail = new TST_HISTORY_DETAIL();

								for (int i = 0; i < historyChangeSet.Details.Count; i++)
								{
									//historyChangeSet.Details.Add(historyDetail);
									historyDetail.CHANGESET_ID = historyChangeSetId;
									historyDetail.FIELD_NAME = historyChangeSet.Details[i].FieldName;    //Field Name
									historyDetail.FIELD_CAPTION = historyChangeSet.Details[i].FieldCaption;       //Field's Caption
									historyDetail.OLD_VALUE = historyChangeSet.Details[i].OldValue;          //The string representation
									historyDetail.OLD_VALUE_INT = historyChangeSet.Details[i].OldValueInt;
									historyDetail.OLD_VALUE_DATE = historyChangeSet.Details[i].OldValueDate;
									historyDetail.NEW_VALUE = historyChangeSet.Details[i].NewValue;    //The string representation
									historyDetail.NEW_VALUE_INT = historyChangeSet.Details[i].NewValueInt;
									historyDetail.NEW_VALUE_DATE = historyChangeSet.Details[i].NewValueDate;
									historyDetail.FIELD_ID = historyChangeSet.Details[i].FieldId;

									context.TST_HISTORY_DETAIL.Add(historyDetail);
								}
								context.SaveChanges();
							}
						}
					}

					//using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					//{
					//	foreach (HistoryChangeSet historyChangeSet in historyChangeSets)
					//	{
					//		//Ignore changesets for modified cases where no fields listed
					//		if (historyChangeSet.ChangeTypeId != (int)ChangeSetTypeEnum.Modified || historyChangeSet.Details.Count > 0)
					//		{
					//			context.HistoryChangeSets.AddObject(historyChangeSet);

					//			//HistoryDetail historyDetails = new HistoryDetail()
					//			//{
					//			//	ChangeSet = historyChangeSet,
					//			//	FieldName = "",
					//			//	FieldCaption = caption,
					//			//	OldValue = "",
					//			//	NewValue = historyChangeSet.ArtifactDesc
					//			//};

					//			//context.HistoryDetails.AddObject(historyDetails);

					//		}
					//	}
					//	context.SaveChanges();
					//	historyChangeSetId = historyChangeSets.First().ChangeSetId;


					//}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return historyChangeSetId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Permanently deletes all items Marked for Deletion from the project.</summary>
		/// <param name="projectId">The project ID to purge all deleted items from.</param>
		/// <param name="userId">The user ID calling the purge.</param>
		public void PurgeAllDeleted(int projectId, int userId, dynamic bkgProcess = null)
		{
			const string METHOD_NAME = "PurgeAllDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//While it may take longer, we're going to call the PurgeFromDatabase for each item found - 
			//  this way, we are sure to not leave any stray database items in tables. On the downside,
			//  it means we need to get lists for each artifact type, then loop through and call purge
			//  on each one.

			Hashtable filterList = new Hashtable();
			int numDeleted = 0;

			// - Test Runs first.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Test Configurations..");
			List<TestConfigurationSet> configs = new TestConfigurationManager().RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_TestConfiguration;

			foreach (TestConfigurationSet config in configs)
			{
				//Update status, if we're in a background process..
				if (bkgProcess != null)
					bkgProcess.Progress = (int)((numDeleted / (float)(configs.Count - 1)) * 100);

				try
				{
					//It's still there, and it's deleted.
					new TestConfigurationManager().DeleteHostFromDatabase(config.TestConfigurationSetId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Test Runs first.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Test Runs..");
			List<TestRunView> testRuns = new TestRunManager().RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_TestRuns;

			foreach (TestRunView testRun in testRuns)
			{
				//Update status, if we're in a background process..
				if (bkgProcess != null)
					bkgProcess.Progress = (int)((numDeleted / (float)(testRuns.Count - 1)) * 100);

				try
				{
					//It's still there, and it's deleted.
					new TestRunManager().DeleteFromDatabase(testRun.TestRunId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Test Sets first.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Test Sets..");
			List<TestSetView> testSets = new TestSetManager().RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_TestSets;

			foreach (TestSetView rowTestSet in testSets)
			{
				//Update status, if we're in a background process..
				if (bkgProcess != null)
					bkgProcess.Progress = (int)((numDeleted / (float)(testSets.Count - 1)) * 100);

				try
				{
					//It's still there, and it's deleted.
					new TestSetManager().DeleteFromDatabase(rowTestSet.TestSetId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Test Cases, now.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Test Cases..");
			List<TestCaseView> testCases = new TestCaseManager().RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_TestCases;
			foreach (TestCaseView rowTestCase in testCases)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(testCases.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new TestCaseManager().DeleteFromDatabase(rowTestCase.TestCaseId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Test Steps, now.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Test Steps..");
			List<TestStep> delSteps = new TestCaseManager().RetrieveAllDeletedSteps(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_TestSteps;
			foreach (TestStep step in delSteps)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(delSteps.Count - 1)) * 100);

					new TestCaseManager().DeleteStepFromDatabase(userId, step.TestStepId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Now, Incidents
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Incidents..");
			List<IncidentView> incidents = new IncidentManager().RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Incidents;
			foreach (IncidentView incident in incidents)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(incidents.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new IncidentManager().DeleteFromDatabase(incident.IncidentId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Let's handle Tasks.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Tasks..");
			TaskManager taskManager = new TaskManager();
			List<TaskView> tasks = taskManager.RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Tasks;
			foreach (TaskView task in tasks)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(tasks.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new TaskManager().DeleteFromDatabase(task.TaskId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Let's handle Risk.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Risk..");
			RiskManager riskManager = new RiskManager();
			List<RiskView> risks = riskManager.RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Risks;
			foreach (RiskView risk in risks)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(tasks.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new RiskManager().Risk_DeleteFromDatabase(risk.RiskId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Let's handle Documents.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Documents..");
			AttachmentManager attachmentManager = new AttachmentManager();
			List<Attachment> attachments = attachmentManager.RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Attachments;
			foreach (Attachment attachment in attachments)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(tasks.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new AttachmentManager().Delete(projectId, attachment.AttachmentId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - And Requirements!
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Requirements..");
			RequirementManager requirementManager = new RequirementManager();
			List<RequirementView> requirements = requirementManager.RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Requirements;
			foreach (RequirementView requirement in requirements)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(requirements.Count - 1)) * 100);

					//It's still there, and it's deleted.
					requirementManager.DeleteFromDatabase(requirement.RequirementId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - And Requirement Steps
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Requirement Steps...");
			List<RequirementStep> requirementSteps = requirementManager.RetrieveDeletedSteps(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_RequirementSteps;
			foreach (RequirementStep requirementStep in requirementSteps)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(requirementSteps.Count - 1)) * 100);

					//It's still there, and it's deleted.
					requirementManager.PurgeStep(projectId, requirementStep.RequirementStepId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Releases.
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Releases..");
			ReleaseManager releaseManager = new ReleaseManager();
			List<ReleaseView> releases = releaseManager.RetrieveDeleted(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Releases;
			foreach (ReleaseView release in releases)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(releases.Count - 1)) * 100);

					//It's still there, and it's deleted.
					releaseManager.DeleteFromDatabase(release.ReleaseId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			// - Automation Hosts
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Purging Automation Hosts..");
			List<AutomationHostView> automationHosts = new AutomationManager().RetrieveDeletedHosts(projectId);
			numDeleted = 0;
			if (bkgProcess != null)
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_AutomationHosts;
			foreach (AutomationHostView rowHost in automationHosts)
			{
				try
				{
					//Update status, if we're in a background process..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)((numDeleted / (float)(automationHosts.Count - 1)) * 100);

					//It's still there, and it's deleted.
					new AutomationManager().DeleteHostFromDatabase(rowHost.AutomationHostId, userId);
					numDeleted++;
				}
				catch { }
			}
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "- " + numDeleted.ToString() + " purged.");

			//Send back succeeded message.
			if (bkgProcess != null)
			{
				bkgProcess.Progress = 100;
				bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Complete;
				bkgProcess.setStatus_Completed();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Called by the background Process Manager to delete a single item.</summary>
		/// <param name="projectId">The project ID.</param>
		/// <param name="changeSetId">The changeset ID of the deletion.</param>
		/// <param name="bkgProcess">The background process to report back to.</param>
		/// <param name="userId">The user ID performing the purge.</param>
		public void PurgeItem(int projectId, long changeSetId, int userId, dynamic bkgProcess = null)
		{
			const string METHOD_NAME = CLASS_NAME + "PurgeAllDeleted()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Get the changeset, first.
			HistoryChangeSetResponse changeSet = RetrieveChangeSetById(changeSetId, false);

			//Status..
			bool noError = true;

			//Update status, if we're in a background process..
			if (bkgProcess != null)
				bkgProcess.Progress = 50;

			if (changeSet != null)
			{
				if (changeSet.ChangeTypeId == (int)ChangeSetTypeEnum.Deleted)
				{
					//Get the type and ID..
					ArtifactTypeEnum artType = (ArtifactTypeEnum)changeSet.ArtifactTypeId;
					int artifactId = changeSet.ArtifactId;

					//Now, call the purge option..
					try
					{
						switch (artType)
						{
							case ArtifactTypeEnum.Incident:
								new IncidentManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.Task:
								new TaskManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.TestCase:
								new TestCaseManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.Document:
								new AttachmentManager().Delete(projectId, artifactId, userId);	
								break;
							case ArtifactTypeEnum.TestStep:
								new TestCaseManager().DeleteStepFromDatabase(userId, artifactId);
								break;
							case ArtifactTypeEnum.Release:
								new ReleaseManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.TestSet:
								new TestSetManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.TestRun:
								new TestRunManager().Delete(artifactId, userId);
								break;
							case ArtifactTypeEnum.Requirement:
								new RequirementManager().DeleteFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.AutomationHost:
								new AutomationManager().DeleteHostFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.TestConfigurationSet:
								new TestConfigurationManager().DeleteHostFromDatabase(artifactId, userId);
								break;
							case ArtifactTypeEnum.RequirementStep:
								new RequirementManager().PurgeStep(projectId, artifactId, userId);
								break;
							case ArtifactTypeEnum.RiskMitigation:
								new RiskManager().RiskMitigation_Purge(projectId, artifactId, userId);
								break;
							case ArtifactTypeEnum.Risk:
								new RiskManager().Risk_DeleteFromDatabase(artifactId, userId);
								break;
						}
					}
					catch (Exception ex)
					{
						Logger.LogWarningEvent(METHOD_NAME, ex, "Trying to undelete item.");
						noError = false;
					}

					//Send back succeeded message.
					if (bkgProcess != null)
					{
						bkgProcess.Progress = 100;
						bkgProcess.Message = GlobalResources.General.History_PurgeProgress_Complete;
						if (noError)
							bkgProcess.setStatus_Completed();
						else
							bkgProcess.setStatus_Error();
					}
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		#region Association Helper Functions
		/// <summary>Records an addition to Requirement Test Coverage.</summary>
		/// <returns></returns>
		/// <remarks>This is a override for RecordAssociatonChange()</remarks>
		public HistoryChangeSet AddRequirementTestCoverage(int projectId, int requirementId, string requirementName, int userId, List<int> testCaseIds)
		{
			const string METHOD_NAME = CLASS_NAME + "AddRequirementTestCoverage()";
			Logger.LogEnteringEvent(METHOD_NAME);
			HistoryChangeSet retValue = null;

			//Check if Baselining is enabled.
			ProjectSettings proj = new ProjectSettings(projectId);
			if (Global.Feature_Baselines && proj.BaseliningEnabled && testCaseIds != null && testCaseIds.Count > 0)
			{
				//See if we need to pull the RQ's name.
				if (string.IsNullOrWhiteSpace(requirementName))
				{
					using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
					{
						requirementName = ct.Requirements
							.Where(r => r.RequirementId == requirementId)
							.Select(r => r.Name)
							.FirstOrDefault();
					}
				}

				//Generate our records.
				List<AssociationChange> changes = new List<AssociationChange>();
				foreach (int testcaseId in testCaseIds)
				{
					AssociationChange newAdd = new AssociationChange
					{
						DestArtifactId = testcaseId,
						DestArtifactType = (int)ArtifactTypeEnum.TestCase,
						SourceArtifactId = requirementId,
						SourceArtifactType = (int)ArtifactTypeEnum.Requirement
					};
					changes.Add(newAdd);
				}

				if (changes.Count > 0)
				{
					retValue = RecordAssociationChange(
						projectId,
						requirementId,
						requirementName,
						ArtifactTypeEnum.TestCase,
						userId,
						false,
						changes);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Records a remove from the Requirement Test Coverage.</summary>
		/// <returns></returns>
		/// <remarks>This is a override for RecordAssociatonChange()</remarks>
		public HistoryChangeSet RemoveRequirementTestCoverage(int projectId, int requirementId, string requirementName, int userId, List<int> testCaseIds)
		{
			const string METHOD_NAME = CLASS_NAME + "RemoveRequirementTestCoverage()";
			Logger.LogEnteringEvent(METHOD_NAME);
			HistoryChangeSet retValue = null;

			//Check if Baselining is enabled.
			ProjectSettings proj = new ProjectSettings(projectId);
			if (Global.Feature_Baselines && proj.BaseliningEnabled && testCaseIds != null && testCaseIds.Count > 0)
			{
				//See if we need to pull the RQ's name.
				if (string.IsNullOrWhiteSpace(requirementName))
				{
					using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
					{
						requirementName = ct.Requirements
							.Where(r => r.RequirementId == requirementId)
							.Select(r => r.Name)
							.FirstOrDefault();
					}
				}

				//Generate our records.
				List<AssociationChange> changes = new List<AssociationChange>();
				foreach (int testcaseId in testCaseIds)
				{
					AssociationChange newAdd = new AssociationChange
					{
						DestArtifactId = testcaseId,
						DestArtifactType = (int)ArtifactTypeEnum.TestCase,
						SourceArtifactId = requirementId,
						SourceArtifactType = (int)ArtifactTypeEnum.Requirement
					};
					changes.Add(newAdd);

				}

				if (changes.Count > 0)
				{
					retValue = RecordAssociationChange(
						projectId,
						requirementId,
						requirementName,
						ArtifactTypeEnum.TestCase,
						userId,
						true,
						changes);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Records an addition to Release Test Coverage.</summary>
		/// <returns></returns>
		/// <remarks>This is a override for RecordAssociatonChange()</remarks>
		public HistoryChangeSet AddReleaseTestCoverage(int projectId, int releaseId, string releaseName, int userId, List<int> testCaseIds)
		{
			const string METHOD_NAME = CLASS_NAME + "AddReleaseTestCoverage()";
			Logger.LogEnteringEvent(METHOD_NAME);
			HistoryChangeSet retValue = null;

			//Check if Baselining is enabled.
			ProjectSettings proj = new ProjectSettings(projectId);
			if (Global.Feature_Baselines && proj.BaseliningEnabled && testCaseIds != null && testCaseIds.Count > 0)
			{
				//See if we need to pull the RQ's name.
				if (string.IsNullOrWhiteSpace(releaseName))
				{
					using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
					{
						releaseName = ct.Releases
							.Where(r => r.ReleaseId == releaseId)
							.Select(r => r.Name)
							.FirstOrDefault();
					}
				}

				//Generate our records.
				List<AssociationChange> changes = new List<AssociationChange>();
				foreach (int testcaseId in testCaseIds)
				{
					AssociationChange newAdd = new AssociationChange
					{
						DestArtifactId = testcaseId,
						DestArtifactType = (int)ArtifactTypeEnum.TestCase,
						SourceArtifactId = releaseId,
						SourceArtifactType = (int)ArtifactTypeEnum.Release
					};
					changes.Add(newAdd);
				}

				if (changes.Count > 0)
				{
					retValue = RecordAssociationChange(
						projectId,
						releaseId,
						releaseName,
						ArtifactTypeEnum.TestCase,
						userId,
						false,
						changes);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Records a remove from the Release Test Coverage.</summary>
		/// <returns></returns>
		/// <remarks>This is a override for RecordAssociatonChange()</remarks>
		public HistoryChangeSet RemoveReleaseTestCoverage(int projectId, int releaseId, string releaseName, int userId, List<int> testCaseIds)
		{
			const string METHOD_NAME = CLASS_NAME + "RemoveReleaseTestCoverage()";
			Logger.LogEnteringEvent(METHOD_NAME);
			HistoryChangeSet retValue = null;

			//Check if Baselining is enabled.
			ProjectSettings proj = new ProjectSettings(projectId);
			if (Global.Feature_Baselines && proj.BaseliningEnabled && testCaseIds != null && testCaseIds.Count > 0)
			{
				//See if we need to pull the RQ's name.
				if (string.IsNullOrWhiteSpace(releaseName))
				{
					using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
					{
						releaseName = ct.Releases
							.Where(r => r.ReleaseId == releaseId)
							.Select(r => r.Name)
							.FirstOrDefault();
					}
				}

				//Generate our records.
				List<AssociationChange> changes = new List<AssociationChange>();
				foreach (int testcaseId in testCaseIds)
				{
					AssociationChange newAdd = new AssociationChange
					{
						DestArtifactId = testcaseId,
						DestArtifactType = (int)ArtifactTypeEnum.TestCase,
						SourceArtifactId = releaseId,
						SourceArtifactType = (int)ArtifactTypeEnum.Release
					};
					changes.Add(newAdd);
				}

				if (changes.Count > 0)
				{
					retValue = RecordAssociationChange(
						projectId,
						releaseId,
						releaseName,
						ArtifactTypeEnum.TestCase,
						userId,
						true,
						changes);
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>
		/// Records the history of a document version being added, removed or the current version being changed
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="attachmentId">The id of the attachment</param>
		/// <param name="attachmentName">The name/filename of the attachment</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="operationType">The operation type (modify, create, delete) of the version</param>
		/// <param name="versionNumber">The version number</param>
		/// <param name="updateCurrentVersion">Did we update the current version</param>
		/// <returns></returns>
		internal HistoryChangeSet History_RecordDocumentVersionChange(
			int projectId,
			int attachmentId,
			string attachmentName,
			int userId,
			Project.PermissionEnum operationType,
			string versionNumber,
			bool updateCurrentVersion = false,
			string existingFilename = "",
			string existingVersionNumber = ""
			)
		{
			const string METHOD_NAME = CLASS_NAME + "History_RecordDocumentVersionChange()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return object.
			HistoryChangeSet newChangeset = null;

			using (var ct = new SpiraTestEntitiesEx())
			{
				//Create our Changeset  first.
				newChangeset = new HistoryChangeSet()
				{
					ArtifactDesc = attachmentName,
					ArtifactId = attachmentId,
					ArtifactTypeId = (int)Artifact.ArtifactTypeEnum.Document,
					ChangeDate = DateTime.UtcNow,
					ChangeTypeId = (int)ChangeSetTypeEnum.Modified,
					ProjectId = projectId,
					UserId = userId
				};
				ct.HistoryChangeSets.AddObject(newChangeset);

				//Add the Details for the add/remove
				if (operationType == Project.PermissionEnum.Create || operationType == Project.PermissionEnum.Delete)
				{
					HistoryDetail historyDetails = new HistoryDetail()
					{
						ChangeSet = newChangeset,
						FieldName = "_AttachmentVersion",
						FieldCaption = (operationType == Project.PermissionEnum.Create) ? GlobalResources.General.Document_AddVersion : GlobalResources.General.Document_DeleteVersion,
						OldValue = (operationType == Project.PermissionEnum.Create) ? "" : attachmentName + "(" + versionNumber + ")",
						NewValue = (operationType == Project.PermissionEnum.Delete) ? "" : attachmentName + "(" + versionNumber + ")"
					};
					ct.HistoryDetails.AddObject(historyDetails);
				}

				//Add the change of the current version (if appropriate)
				if (updateCurrentVersion)
				{
					HistoryDetail historyDetails = new HistoryDetail()
					{
						ChangeSet = newChangeset,
						FieldName = "_CurrentVersion",
						FieldCaption = GlobalResources.General.CurrentVersion,
						OldValue = existingFilename + "(" + existingVersionNumber + ")",
						NewValue = attachmentName + "(" + versionNumber + ")"
					};
					ct.HistoryDetails.AddObject(historyDetails);

				}

				//Save the changes.
				ct.SaveChanges();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newChangeset;
		}

		/// <summary>Records an association addition or removeal.</summary>
		/// <param name="projectId">The project ID this occurs in.</param>
		/// <param name="artifactId">The artifact Id that the associations are being added to or removed. (See notes.)</param>
		/// <param name="artifactName">The name of the artifact that the associations are being added to or removed.</param>
		/// <param name="artifactType">The artifact's type.</param>
		/// <param name="userId">The userId performing the action.</param>
		/// <param name="isRemove">True if the list of assocations are being REMOVED. if being added, this should be false.</param>
		/// <param name="associations">A list of assocations in this Changeset.</param>
		/// <returns></returns>
		internal HistoryChangeSet RecordAssociationChange(
			int projectId,
			int artifactId,
			string artifactName,
			ArtifactTypeEnum artifactType,
			int userId,
			bool isRemove,
			List<AssociationChange> associations)
		{
			const string METHOD_NAME = CLASS_NAME + "RecordAssociationChange()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return object.
			HistoryChangeSet newChangeset = null;

			using (var ct = new SpiraTestEntitiesEx())
			{
				//Create our Changeset  first.
				newChangeset = new HistoryChangeSet()
				{
					ArtifactDesc = artifactName,
					ArtifactId = artifactId,
					ArtifactTypeId = (int)artifactType,
					ChangeDate = DateTime.UtcNow,
					ChangeTypeId = (int)(isRemove ? ChangeSetTypeEnum.Association_Remove : ChangeSetTypeEnum.Association_Add),
					ProjectId = projectId,
					UserId = userId
				};
				ct.HistoryChangeSets.AddObject(newChangeset);

				//Create the association links.
				foreach (var ass in associations)
				{
					HistoryAssociation newAss = new HistoryAssociation()
					{
						DestArtifactId = ass.DestArtifactId,
						DestArtifactTypeId = ass.DestArtifactType,
						NewComment = ass.NewComment,
						OldComment = ass.OldComment,
						SourceArtifactId = ass.SourceArtifactId,
						SourceArtifactTypeId = ass.SourceArtifactType,
						NewArtifactLinkTypeId = (int?)ass.LinkType,
						ChangeSet = newChangeset
					};

					ct.HistoryAssociations.AddObject(newAss);
				}

				//Save the changes.
				ct.SaveChanges();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newChangeset;
		}

		/// <summary>This is only used for UnitTests. DO NOT USE.</summary>
		/// <param name="projectId">The ProjectId.</param>
		[Obsolete]
		public List<HistoryChangeSet> TestingOnly_RetrieveChangesetsWithAssociations(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "TestingOnly_RetrieveChangesetsWithAssociations()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<HistoryChangeSet> retList = new List<HistoryChangeSet>();

			using (var ct = new SpiraTestEntitiesEx())
			{
				retList = ct.HistoryChangeSets
					.Include(c => c.AssociationChanges)
					.Where(c => c.ProjectId == projectId &&
						(c.ChangeTypeId == (int)ChangeSetTypeEnum.Association_Add ||
						c.ChangeTypeId == (int)ChangeSetTypeEnum.Association_Modify ||
						c.ChangeTypeId == (int)ChangeSetTypeEnum.Association_Remove))
					.ToList();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>This is only used for UnitTests. DO NOT USE.</summary>
		/// <param name="projectId">The ProjectId.</param>
		[Obsolete]
		public List<HistoryChangeSet> TestingOnly_RetrieveChangesetsWithPositions(int projectId)
		{
			const string METHOD_NAME = CLASS_NAME + "TestingOnly_RetrieveChangesetsWithAssociations()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<HistoryChangeSet> retList = new List<HistoryChangeSet>();

			using (var ct = new SpiraTestEntitiesEx())
			{
				retList = ct.HistoryChangeSets
					.Include(c => c.PositionChanges)
					.Where(c => c.ProjectId == projectId &&
						c.ChangeTypeId == (int)ChangeSetTypeEnum.Modified &&
						c.PositionChanges.Count() > 0)
					.ToList();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}
		#endregion Association Helper Functions

		#region Positioning Log Functions
		/// <summary>Logs a history entry for an item changing position.</summary>
		/// <param name="projectId">The projectId to save the history for.</param>
		/// <param name="userId">The person making the change. </param>
		/// <param name="riskId">The Rick ID.</param>
		/// <param name="riskName">The Risk's name.</param>
		/// <param name="riskStepId">The Risk's ID.</param>
		/// <param name="oldPosition">The original position. (-1 - no previous position)</param>
		/// <param name="newPosition">The new position for this Mitigation Step artifact. (-1 - no previous position)</param>
		/// <param name="existChangeSet">If set, the ChangeSetId to add this action to.</param>
		/// <returns>The number of the created ChangeSet.</returns>
		public long RecordRiskStepPosition(
			int projectId,
			int userId,
			int riskId,
			string riskName,
			int riskStepId,
			int oldPosition,
			int newPosition,
			long? existChangeSet = null)
		{
			const string METHOD_NAME = CLASS_NAME + "RecordRiskStepPosition()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Call the paren't function.
			long retVal = RecordPositionChange(
				projectId,
				userId,
				riskId,
				ArtifactTypeEnum.Risk,
				riskName,
				riskStepId,
				ArtifactTypeEnum.RiskMitigation,
				oldPosition,
				newPosition,
				existChangeSet
				);

			Logger.LogExitingEvent(METHOD_NAME);
			return retVal;
		}

		/// <summary>Logs a history entry for an item changing position.</summary>
		/// <param name="projectId">The projectId to save the history for.</param>
		/// <param name="userId">The person making the change. </param>
		/// <param name="testCaseId">The Requirement ID.</param>
		/// <param name="testCaseName">The Requiremet's name.</param>
		/// <param name="testStepId">The Requirement's ID.</param>
		/// <param name="oldPosition">The original position. (-1 - no previous position)</param>
		/// <param name="newPosition">The new position for this Requirement Step artifact. (-1 - no previous position)</param>
		/// <param name="existChangeSet">If set, the ChangeSetId to add this action to.</param>
		/// <returns>The number of the created ChangeSet.</returns>
		public long RecordReqStepPosition(
			int projectId,
			int userId,
			int requirementId,
			string requirementName,
			int requirementStepId,
			int oldPosition,
			int newPosition,
			long? existChangeSet = null)
		{
			const string METHOD_NAME = CLASS_NAME + "RecordReqStepPosition()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Call the paren't function.
			long retVal = RecordPositionChange(
				projectId,
				userId,
				requirementId,
				ArtifactTypeEnum.Requirement,
				requirementName,
				requirementStepId,
				ArtifactTypeEnum.RequirementStep,
				oldPosition,
				newPosition,
				existChangeSet
				);

			Logger.LogExitingEvent(METHOD_NAME);
			return retVal;
		}

		/// <summary>Logs a history entry for an item changing position.</summary>
		/// <param name="projectId">The projectId to save the history for.</param>
		/// <param name="userId">The person making the change. </param>
		/// <param name="testCaseId">The Test Case ID.</param>
		/// <param name="testCaseName">The Test Case's name.</param>
		/// <param name="testStepId">The Test Step's ID.</param>
		/// <param name="oldPosition">The original position. (-1 - no previous position)</param>
		/// <param name="newPosition">The new position for this child artifact. (-1 - no previous position)</param>
		/// <param name="existChangeSet">If set, the ChangeSetId to add this action to.</param>
		/// <returns>The number of the created ChangeSet.</returns>
		public long RecordTestStepPosition(
			int projectId,
			int userId,
			int testCaseId,
			string testCaseName,
			int testStepId,
			int oldPosition,
			int newPosition,
			long? existChangeSet = null)
		{
			const string METHOD_NAME = CLASS_NAME + "RecordTestStepPosition()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Call the paren't function.
			long retVal = RecordPositionChange(
				projectId,
				userId,
				testCaseId,
				ArtifactTypeEnum.TestCase,
				testCaseName,
				testStepId,
				ArtifactTypeEnum.TestStep,
				oldPosition,
				newPosition,
				existChangeSet
				);

			Logger.LogExitingEvent(METHOD_NAME);
			return retVal;
		}

		/// <summary>Logs a history entry for an item changing position.</summary>
		/// <param name="projectId">The projectId to save the history for.</param>
		/// <param name="userId">The person making the change. </param>
		/// <param name="parentArtId">The artifact ID containing this item.</param>
		/// <param name="parentArtType">The ArtifactType of the parent artifact containing this item.</param>
		/// <param name="parentArtName">The name (for display purposes) of the parent artifact.</param>
		/// <param name="childArtId">The child's artifact ID.</param>
		/// <param name="childArtType">The child's artifact type.</param>
		/// <param name="oldPosition">The original position. (-1 - no previous position)</param>
		/// <param name="newPosition">The new position for this child artifact.</param>
		/// <param name="existChangeSet">If set, the ChangeSetId to add this action to.</param>
		/// <returns>The number of the created ChangeSet.</returns>
		public long RecordPositionChange(
			int projectId,
			int userId,
			int parentArtId,
			ArtifactTypeEnum parentArtType,
			string parentArtName,
			int childArtId,
			ArtifactTypeEnum childArtType,
			int oldPosition,
			int newPosition,
			long? existChangeSet = null
			)
		{
			const string METHOD_NAME = CLASS_NAME + "RecordPositionChange()";
			Logger.LogEnteringEvent(METHOD_NAME);

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				//Create our new Changeset..
				DateTime date = DateTime.UtcNow;

				//See if we need to make a new ChangeSet.
				if (!existChangeSet.HasValue)
				{
					//We do, so create it.
					HistoryChangeSet newSet = new HistoryChangeSet()
					{
						ArtifactId = parentArtId,
						ArtifactTypeId = (int)parentArtType,
						ChangeDate = date,
						ChangeTypeId = (int)ChangeSetTypeEnum.Modified,
						UserId = userId,
						ProjectId = projectId,
						ArtifactDesc = parentArtName
					};
					//Add it and save.
					ct.HistoryChangeSets.AddObject(newSet);
					ct.SaveChanges();

					//Pull the number.
					existChangeSet = newSet.ChangeSetId;
				}

				HistoryPosition newPos = new HistoryPosition
				{
					ChangeSetId = existChangeSet.Value,
					ChildArtifactId = childArtId,
					ChildArtifactTypeId = (int)childArtType,
					OldPosition = oldPosition,
					NewPosition = newPosition
				};
				//Add it and save.
				ct.HistoryPositions.AddObject(newPos);
				ct.SaveChanges();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return existChangeSet.Value;
		}
		#endregion Positioning Log Functions

		#region Sub Classes
		/// <summary>Class used to store a record of an association change.</summary>
		internal class AssociationChange
		{
			public ArtifactLinkTypeEnum? LinkType { get; set; }

			#region Source Artifact Info
			public int SourceArtifactId { get; set; }
			public int SourceArtifactType { get; set; }
			#endregion Source Artifact Info

			#region Destination Artifact Info
			public int DestArtifactId { get; set; }
			public int DestArtifactType { get; set; }
			#endregion Destination Artifact Info

			#region Comment Information
			public string NewComment { get; set; }
			public string OldComment { get; set; }
			#endregion Comment Information
		}
		#endregion Sub Classes

		#region Enumerations
		/// <summary>Enumeration of available ChangeSet types. Should match TST_HISTORY_CHANGESET_TYPE</summary>
		public enum ChangeSetTypeEnum : int
		{
			Modified = 1,
			Deleted = 2,
			Added = 3,
			Purged = 4,
			Rollback = 5,
			Undelete = 6,
			Imported = 7,
			Exported = 8,
			Deleted_Parent = 9,
			Added_Parent = 10,
			Purged_Parent = 11,
			Undelete_Parent = 12,
			Association_Add = 13,
			Association_Remove = 14,
			Association_Modify = 15
		}

		/// <summary>Enumeration of how a rollbak attemp completed.</summary>
		public enum RollbackResultEnum : int
		{
			/// <summary>All fields were able to be rolled back.</summary>
			Success = 1,
			/// <summary>Rollback was successful, but some fields weren't rolled back without an error.</summary>
			Warning = 2,
			/// <summary>There was an error rolling back. Rollback was unsuccessful.</summary>
			Error = 3
		}

		/// <summary>Enumeration of values for Artifact Associations.</summary>
		public enum ArtifactLinkTypeEnum : int
		{
			Related_To = 1,
			Depends_On = 2,
			Implicit = 3,
			Cource_Code = 4
		}
		#endregion Enumerations

		#region Internal Methods
		/// <summary>Prepares the history records for saving to the database.</summary>
		/// <param name="objMgr">The object manager that has our changed items.</param>
		/// <param name="changerId">The user id making the change.</param>
		/// <param name="rollbackId">The rollback ID used for recording rollback actions</param>
		/// <returns>A list of changesets and associated list of HistoryRecords.</returns>
		internal List<HistoryChangeSet> LogHistoryAction_Begin(ObjectStateManager objMgr, int changerId, long? rollbackId = null, int? projecttId = null, int? artId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "LogHistoryAction_Begin()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<HistoryChangeSet> historyChangeSets = new List<HistoryChangeSet>();

			//Log history..
			foreach (ObjectStateEntry entry in objMgr.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted))
			{
				#region ArtifactCustomProperty
				if (entry.Entity is ArtifactCustomProperty)
				{
					ArtifactCustomProperty artifactCustomProperty = (ArtifactCustomProperty)entry.Entity;

					//Get the various fields needed to record history
					int projectId = artifactCustomProperty.ProjectId;
					int artifactTypeId = artifactCustomProperty.ArtifactTypeId;
					int artifactId = artifactCustomProperty.ArtifactId;

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Let's get the artifact, first, if we can.
					string artifactName = "";
					try
					{
						switch (artifactCustomProperty.ArtifactTypeEnum)
						{
							case ArtifactTypeEnum.Incident:
								artifactName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
								break;
							case ArtifactTypeEnum.Project:
								artifactName = new ProjectManager().RetrieveById(artifactId).Name;
								break;
							case ArtifactTypeEnum.Release:
								artifactName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
								break;
							case ArtifactTypeEnum.Requirement:
								artifactName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
								break;
							case ArtifactTypeEnum.Task:
								artifactName = new TaskManager().RetrieveById(artifactId, true).Name;
								break;
							case ArtifactTypeEnum.TestCase:
								artifactName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
								break;
							case ArtifactTypeEnum.TestSet:
								artifactName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
								break;
							case ArtifactTypeEnum.TestStep:
								{
									TestStep testStep = new TestCaseManager().RetrieveStepById(projectId, artifactId, true);
									artifactName = testStep.TestCase.Name + " " + GlobalResources.General.TestStep_Step + " " + testStep.ArtifactToken;
								}
								break;
							case ArtifactTypeEnum.AutomationHost:
								artifactName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
								break;
							case ArtifactTypeEnum.Document:
								artifactName = new AttachmentManager().RetrieveById(artifactId).Filename;
								break;
							case ArtifactTypeEnum.TestRun:
								artifactName = new TestRunManager().RetrieveById(artifactId).Name;
								break;
							case ArtifactTypeEnum.Risk:
								artifactName = new RiskManager().Risk_RetrieveById(artifactId).Name;
								break;
							default: //It's a type we don't record for..
								continue;
						}
					}
					catch (Exception ex)
					{
						//Simply log it.
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
					}

					//Create the HistoryChangeSet first..
					//Custom properties only have Modified/Rollback changeset types since addition/deletes
					//are tracked by the main artifact type entry (rather than the associated custom property entity)

					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projectId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					if (artId != null)
					{
						historyChangeSet.ArtifactId = (int)artId;
					}
					else
					{
						historyChangeSet.ArtifactId = artifactId;
					}
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are not tracked for custom properties
							}
							break;

						case EntityState.Added:
							{
								//Get the custom property definition if it's not already part of the entity
								List<CustomProperty> customProperties = artifactCustomProperty.CustomPropertyDefinitions;
								if (customProperties == null || customProperties.Count < 1)
								{
									CustomPropertyManager customPropertyManager = new CustomPropertyManager();
									customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeId(projectTemplateId, artifactTypeId, false, true);
								}

								//Loop through all the current values (since the entity has just been added all are 'modified' unless null)
								foreach (KeyValuePair<string, PropertyInfo> property in artifactCustomProperty.Properties)
								{
									try
									{
										//Make sure we only consider properties that have a value
										string fieldName = property.Key;
										if (artifactCustomProperty[fieldName] != null)
										{
											//If it's a string, we don't care about extra spaces added/removed.
											if (artifactCustomProperty[fieldName].GetType() == typeof(string))
											{
												if (String.IsNullOrWhiteSpace((string)artifactCustomProperty[fieldName]))
												{
													continue;
												}
											}

											//Find the matching custom property
											CustomProperty customProperty = customProperties.Find(cp => cp.CustomPropertyFieldName == fieldName);

											if (customProperty != null)
											{
												//Get the new values (there are no old values in this case)
												//New Values
												object newValue = artifactCustomProperty[fieldName];
												string newValueSerialized = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												if (fieldName == "EstimatedDuration")
												{
													if (newValueSerialized != null)
													{
														newValueSerialized = newValueSerialized + " mins";
													}
													else
													{
														newValueSerialized = "0 mins";
													}
												}
												
												if (fieldName == "EstimatedEffort")
												{
													if (newValueSerialized != null)
													{
														newValueSerialized = newValueSerialized + " mins";
													}
													else
													{
														newValueSerialized = "0 mins";
													}
												}


												if (fieldName == "ComponentIds")
												{
													DataModel.Component component = new ComponentManager().Component_RetrieveById(int.Parse(newValueSerialized), true);
													if (component != null)
													{
														newValueSerialized = component.Name;
													}
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = customProperty.CustomPropertyFieldName;   //Field Name
												historyDetail.FieldCaption = customProperty.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueSerialized;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
												historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;

						case EntityState.Modified:
							{
								//Get the custom property definition if it's not already part of the entity
								List<CustomProperty> customProperties = artifactCustomProperty.CustomPropertyDefinitions;
								if (customProperties == null || customProperties.Count < 1)
								{
									CustomPropertyManager customPropertyManager = new CustomPropertyManager();
									customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeId(projectTemplateId, artifactTypeId, false, true);
								}

								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in artifactCustomProperty.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = artifactCustomProperty[fieldName];
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Find the matching custom property
											CustomProperty customProperty = customProperties.Find(cp => cp.CustomPropertyFieldName == fieldName);

											if (customProperty != null)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueSerialized = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueSerialized = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												if (fieldName == "EstimatedDuration")
												{
													if (newValueSerialized != null)
													{
														newValueSerialized = newValueSerialized + " mins";
													}
													else
													{
														newValueSerialized = "0 mins";
													}
												}

												if (fieldName == "ComponentIds")
												{
													DataModel.Component component = new ComponentManager().Component_RetrieveById(int.Parse(newValueSerialized), true);
													if (component != null)
													{
														newValueSerialized = component.Name;
													}
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = customProperty.CustomPropertyFieldName;   //Field Name
												historyDetail.FieldCaption = customProperty.Name;       //Field's Caption
												historyDetail.OldValue = oldValueSerialized;          //The string representation
												historyDetail.OldValueInt = oldValueInt;
												historyDetail.OldValueDate = oldValueDateTime;
												historyDetail.NewValue = newValueSerialized;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
												historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}
				}
				#endregion


				#region ArtifactLink

				else if (entry.Entity is ArtifactLink)
				{
					//Cast to an artifact
					ArtifactLink documentDiscussion = (ArtifactLink)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ArtifactLink;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the various fields needed to record history
					//int projectId = (int)artifact["ProjectId"];    //The artifact has a reference to the project id
					//int artifactId = (int)artifact[primaryKeyField];
					//string artifactName = String.Format(GlobalResources.General.History_ArtifactIdFormat, artifact.ArtifactPrefix, artifactId); //Used if no name available

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject((int)projecttId).ProjectTemplateId;

					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in documentDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = documentDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in documentDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(documentDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = oldValueString;          //The string representation
												historyDetail.OldValueInt = oldValueInt;
												historyDetail.OldValueDate = oldValueDateTime;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = artifactField.ArtifactFieldId;   //The FieldID, null if CustomField.
																										 //historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region Artifact

				else if (entry.Entity is Artifact)
				{
					//Cast to an artifact
					Artifact artifact = (Artifact)entry.Entity;
					int artifactTypeId = (int)artifact.ArtifactType;

					//if(artifact == "ArtifactLink")
					//{

					//}
					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					int artifactId = 0;
					if (artifactFields.Count > 0)
					{
						//Get the name field and the primary key
						string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
						artifactId = (int)artifact[primaryKeyField];
					}
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					//Get the various fields needed to record history
					int projectId = (int)artifact["ProjectId"];    //The artifact has a reference to the project id
					
					string artifactName = String.Format(GlobalResources.General.History_ArtifactIdFormat, artifact.ArtifactPrefix, artifactId); //Used if no name available

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					//List<HistoryChangeSet> historyChangeSet = new List<HistoryChangeSet>();
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					//historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projectId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = artifactId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//See if we have a digital signature to include
					if (!String.IsNullOrEmpty(artifact.SignatureMeaning))
					{
						//Add the meaning to the changeset itself
						historyChangeSet.Meaning = artifact.SignatureMeaning;

						//Add a comment for the digital signature
						new DiscussionManager().Insert(changerId, artifactId, (ArtifactTypeEnum)artifactTypeId, artifact.SignatureMeaning, projectId, true, false);

						//Get the permanent change information
						//We HASH this to avoid tampering
						string signature = historyChangeSet.UserId + ":" + historyChangeSet.ArtifactTypeId + ":" + historyChangeSet.ArtifactId + ":" + historyChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

						//Create the hash using SHA256
						historyChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);						
					}

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
							}
							break;

						case EntityState.Modified:
							{
								List<HistoryDetail> details = new List<HistoryDetail>();
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in artifact.EntityChangeTracker.OriginalValues)
								{
									HistoryChangeSet historyChangeSet1 = new HistoryChangeSet();
									historyChangeSets.Add(historyChangeSet1);
									historyChangeSet1.ProjectId = projectId;
									historyChangeSet1.UserId = changerId;
									historyChangeSet1.ArtifactTypeId = artifactTypeId;
									historyChangeSet1.ArtifactId = artifactId;
									historyChangeSet1.ArtifactDesc = artifactName;
									historyChangeSet1.ChangeDate = DateTime.UtcNow;
									historyChangeSet1.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
									historyChangeSet.RevertId = rollbackId;
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = artifact[fieldName];
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, projectId, projectTemplateId, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, projectId, projectTemplateId, fieldName, newValueInt.Value);
													}
												}


												if (fieldName == "EstimatedDuration")
												{
													if (newValueString != null)
													{
														newValueString = newValueString + " mins";
													}
													else
													{
														newValueString = "0 mins";
													}
													if (oldValueString != null)
													{
														oldValueString = oldValueString + " mins";
													}
													else
													{
														oldValueString = "0 mins";
													}
												}

												if (fieldName == "EstimatedEffort")
												{
													if (newValueString != null)
													{
														newValueString = newValueString + " mins";
													}
													else
													{
														newValueString = "0 mins";
													}
													if (oldValueString != null)
													{
														oldValueString = oldValueString + " mins";
													}
													else
													{
														oldValueString = "0 mins";
													}
												}
												if (newValueString != null) {
													if (fieldName == "ComponentIds")
													{
														if (newValueString.Contains(","))
														{
															// Split the string into an array of substrings
															string[] newValues = newValueString.Split(',');

															// Output each substring (fruit)
															foreach (string val in newValues)
															{
																DataModel.Component component = new ComponentManager().Component_RetrieveById(int.Parse(val), true);
																if (component != null)
																{
																	newValueString = component.Name;
																}
															}
														}
														else
														{
															DataModel.Component component = new ComponentManager().Component_RetrieveById(int.Parse(newValueString), true);
															if (component != null)
															{
																newValueString = component.Name;
															}
														}
													}
													if (!string.IsNullOrEmpty(oldValueString))
													{
														if (fieldName == "ComponentIds")
														{
															if (oldValueString.Contains(","))
															{
																// Split the string into an array of substrings
																string[] oldValues = oldValueString.Split(',');

																// Output each substring (fruit)
																foreach (string val in oldValues)
																{
																	DataModel.Component component1 = new ComponentManager().Component_RetrieveById(int.Parse(val), true);
																	if (component1 != null)
																	{
																		oldValueString = component1.Name;
																	}
																}
															}
															else
															{
																DataModel.Component component1 = new ComponentManager().Component_RetrieveById(int.Parse(oldValueString), true);
																if (component1 != null)
																{
																	oldValueString = component1.Name;
																}
															}
														}
													}

												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet1.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;   //Field Name
												historyDetail.FieldCaption = artifactField.Caption;     //Field's Caption
												historyDetail.OldValue = oldValueString;          //The string representation
												historyDetail.OldValueInt = oldValueInt;
												historyDetail.OldValueDate = oldValueDateTime;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = artifactField.ArtifactFieldId;   //The FieldID, 
												historyDetail.CustomPropertyId = null;//The custom property id, NULL since not a custom property
												
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}
				}

				#endregion


				#region ProjectTagFrequency

				else if (entry.Entity is ProjectTagFrequency)
				{
					//Cast to an artifact
					ProjectTagFrequency projectTagFrequency = (ProjectTagFrequency)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.Document;

					int projectId = projectTagFrequency.ProjectId;    //The artifact has a reference to the project id

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projectId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = Inflectra.SpiraTest.Common.Global.modifieddocumentid;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//string fieldName = "Tags";
								//object currentValue = projectTagFrequency.ChangeTracker.OriginalValues[fieldName];
								//foreach (PropertyInfo propertyInfo in projectTagFrequency.GetType().GetProperties())
								//{
								//	if (propertyInfo.Name == fieldName)
								//	{
								//		currentValue = (Object)propertyInfo.GetValue(projectTagFrequency);
								//		break;
								//	}
								//}
								//object originalValue = changedField.Value;

								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "Tags";
								historyDetail.FieldCaption = "Tags";     //Field's Caption
								historyDetail.OldValue = null;//Inflectra.SpiraTest.Common.Global.Tgmodifieddocumentname.ToString();          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = projectTagFrequency.Name;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in projectTagFrequency.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = projectTagFrequency.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in projectTagFrequency.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(projectTagFrequency);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = oldValueString;          //The string representation
												historyDetail.OldValueInt = oldValueInt;
												historyDetail.OldValueDate = oldValueDateTime;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region DocumentDiscussion

				else if (entry.Entity is DocumentDiscussion)
				{
					//Cast to an artifact
					DocumentDiscussion documentDiscussion = (DocumentDiscussion)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.DocumentDiscussion;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "Comments";
								historyDetail.FieldCaption = "Comments";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = documentDiscussion.Text;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in documentDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = documentDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in documentDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(documentDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion


				#region ReleaseDiscussion

				else if (entry.Entity is ReleaseDiscussion)
				{
					//Cast to an artifact
					ReleaseDiscussion releaseDiscussion = (ReleaseDiscussion)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.ReleaseDiscussion;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "ReleaseDiscussion";
								historyDetail.FieldCaption = "ReleaseDiscussion";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = releaseDiscussion.Text;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in releaseDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = releaseDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in releaseDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(releaseDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region TaskDiscussion

				else if (entry.Entity is TaskDiscussion)
				{
					//Cast to an artifact
					TaskDiscussion taskDiscussion = (TaskDiscussion)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.TaskDiscussion;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;


					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "TaskDiscussion";
								historyDetail.FieldCaption = "TaskDiscussion";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = taskDiscussion.Text;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in taskDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = taskDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in taskDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(taskDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region RiskDiscussion

				else if (entry.Entity is RiskDiscussion)
				{
					//Cast to an artifact
					RiskDiscussion riskDiscussion = (RiskDiscussion)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.RiskDiscussion;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "RiskDiscussion";
								historyDetail.FieldCaption = "RiskDiscussion";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = riskDiscussion.Text;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in riskDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = riskDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in riskDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(riskDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region RiskDiscussion

				else if (entry.Entity is TestSetDiscussion)
				{
					//Cast to an artifact
					TestSetDiscussion testSetDiscussion = (TestSetDiscussion)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.TestSetDiscussion;

					//int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
					//int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projecttId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "TestSet";
								historyDetail.FieldCaption = "TestSet";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = testSetDiscussion.Text;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in testSetDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = testSetDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in testSetDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(testSetDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion

				#region ProjectBaseline

				else if (entry.Entity is ProjectBaseline)
				{
					//Cast to an artifact
					ProjectBaseline documentDiscussion = (ProjectBaseline)entry.Entity;
					int artifactTypeId = (int)ArtifactTypeEnum.DocumentDiscussion;

					int projectId = documentDiscussion.ProjectId; //The artifact has a reference to the project id
																  //int projectId = 1;

					//Get all the fields defined for this artifact type
					ArtifactManager artifactManager = new ArtifactManager();
					List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveAll(artifactTypeId, false, true);

					//Get the name field and the primary key
					string primaryKeyField = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.Identifier).Name;
					string artifactNameField = null;
					ArtifactField af = artifactFields.FirstOrDefault(f => f.ArtifactFieldTypeId == (int)ArtifactFieldTypeEnum.NameDescription);
					if (af != null)
					{
						artifactNameField = af.Name;
					}

					var artifact = artifactFields.FirstOrDefault();
					int artifactId1 = artifact.ArtifactTypeId;
					string artifactName = "";

					//Get the template associated with the project
					if (String.IsNullOrEmpty(artifactNameField))
					{
						//See if it has the field 'Name' available
						if (artifact.ContainsProperty("Name"))
						{
							artifactName = (string)artifact["Name"];
						}
					}
					else
					{
						artifactName = (string)artifact[artifactNameField];
					}

					//Create the HistoryChangeSet first..
					HistoryChangeSet historyChangeSet = new HistoryChangeSet();
					historyChangeSets.Add(historyChangeSet);
					historyChangeSet.ProjectId = projectId;
					historyChangeSet.UserId = changerId;
					historyChangeSet.ArtifactTypeId = artifactTypeId;
					historyChangeSet.ArtifactId = (int)artId;
					historyChangeSet.ArtifactDesc = artifactName;
					historyChangeSet.ChangeDate = DateTime.UtcNow;
					historyChangeSet.ChangeTypeId = ((rollbackId.HasValue) ? (int)ChangeSetTypeEnum.Rollback : (int)ChangeSetTypeEnum.Modified);
					historyChangeSet.RevertId = rollbackId;

					//Now add the actions
					switch (entry.State)
					{
						case EntityState.Deleted:
							{
								//Deletes are tracked separately using the LogDeletion() function
							}
							break;

						case EntityState.Added:
							{
								//Inserts are tracked separately using the LogCreation() function
								HistoryDetail historyDetail = new HistoryDetail();
								historyChangeSet.Details.Add(historyDetail);
								historyDetail.FieldName = "ProjectBaseline";
								historyDetail.FieldCaption = "ProjectBaseline";     //Field's Caption
								historyDetail.OldValue = null;          //The string representation
								historyDetail.OldValueInt = null;
								historyDetail.OldValueDate = null;
								historyDetail.NewValue = documentDiscussion.Name;    //The string representation
								historyDetail.NewValueInt = null;
								historyDetail.NewValueDate = null;
								historyDetail.FieldId = null;

							}
							break;

						case EntityState.Modified:
							{
								//Loop through recorded fields that have changed or been added
								foreach (KeyValuePair<string, object> changedField in documentDiscussion.ChangeTracker.OriginalValues)
								{
									string fieldName = changedField.Key;
									try
									{
										//Get the current/original values from the actual Entity.ChangeTracker
										//because the entry.OriginalValues and entry.CurrentValues was not reliable
										//depending on which fields had changed
										object currentValue = documentDiscussion.ChangeTracker.OriginalValues[fieldName];
										foreach (PropertyInfo propertyInfo in documentDiscussion.GetType().GetProperties())
										{
											if (propertyInfo.Name == fieldName)
											{
												currentValue = (Object)propertyInfo.GetValue(documentDiscussion);
												break;
											}
										}
										object originalValue = changedField.Value;
										Logger.LogTraceEvent(METHOD_NAME, String.Format("Field={0}, OriginalValue={1}, CurrentValue={2}", fieldName, originalValue.ToSafeString(), currentValue.ToSafeString()));

										bool fieldHasChanged = false;
										if (currentValue == null)
										{
											if (originalValue != null)
											{
												fieldHasChanged = true;
											}
										}
										else
										{
											if (!currentValue.Equals(originalValue))
											{
												//If it's a string, we don't care about extra spaces added/removed.
												if (currentValue.GetType() == typeof(string))
												{
													if (((string)currentValue).Trim() != originalValue.ToSafeString().Trim())
													{
														fieldHasChanged = true;
													}
												}
												else
												{
													fieldHasChanged = true;
												}
											}
										}

										if (fieldHasChanged)
										{
											//Get the artifact field definition and make sure we should be recording history for this field
											ArtifactField artifactField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
											if (artifactField != null && artifactField.IsHistoryRecorded)
											{
												//Get the old and new values
												//New Values
												object newValue = currentValue;
												string newValueString = newValue.ToDatabaseSerialization();
												int? newValueInt = ((newValue is int?) ? (int?)newValue : null);
												DateTime? newValueDateTime = ((newValue is DateTime?) ? (DateTime?)newValue : null);

												//Old Values
												object oldValue = originalValue;
												string oldValueString = oldValue.ToDatabaseSerialization();
												int? oldValueInt = ((oldValue is int?) ? (int?)oldValue : null);
												DateTime? oldValueDateTime = ((oldValue is DateTime?) ? (DateTime?)oldValue : null);

												//If we have a lookup property for this field, need to get the old and new values for the lookup text
												if (!String.IsNullOrEmpty(artifactField.LookupProperty))
												{
													if (oldValueInt.HasValue)
													{
														oldValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, oldValueInt.Value);
													}
													if (newValueInt.HasValue)
													{
														newValueString = artifactManager.GetLookupValue((ArtifactTypeEnum)artifactTypeId, 0, 0, fieldName, newValueInt.Value);
													}
												}

												if (oldValueString == "Y")
												{
													oldValueString = "True";
												}
												else if (oldValueString == "N")
												{
													oldValueString = "False";
												}
												if (newValueString == "Y")
												{
													newValueString = "True";
												}
												else if (newValueString == "N")
												{
													newValueString = "False";
												}

												//Add a new history detail entry
												HistoryDetail historyDetail = new HistoryDetail();
												historyChangeSet.Details.Add(historyDetail);
												historyDetail.FieldName = artifactField.Name;    //Field Name
												historyDetail.FieldCaption = artifactField.Name;       //Field's Caption
												historyDetail.OldValue = null;          //The string representation
												historyDetail.OldValueInt = null;
												historyDetail.OldValueDate = null;
												historyDetail.NewValue = newValueString;    //The string representation
												historyDetail.NewValueInt = newValueInt;
												historyDetail.NewValueDate = newValueDateTime;
												historyDetail.FieldId = null;   //The FieldID, null if CustomField.
																				//historyDetail.CustomPropertyId = customProperty.CustomPropertyId;       //The custom property id
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD_NAME, ex, "Generating difference report.");
										throw ex;
									}
								}
							}
							break;
					}

				}
				#endregion



				else
				{
					Logger.LogTraceEvent(METHOD_NAME, "Not logging history for type: " + entry.Entity.GetType().ToString());
				}
			}
			Logger.LogExitingEvent(METHOD_NAME);
			
			return historyChangeSets;
		}

		/// <summary>Gets final data from the Object State Manager and saved the records to the history tables.</summary>
		/// <param name="historyChangeSets">The history changesets and associated change details</param>
		internal void LogHistoryAction_End(List<HistoryChangeSet> historyChangeSets)
		{
			const string METHOD_NAME = CLASS_NAME + "LogHistoryAction_End()";
			Logger.LogEnteringEvent(METHOD_NAME);
			try
			{
				//Save the dataset.
				Insert(historyChangeSets);

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Logs a deletion into the ChangeSet for the specified artifact id, type.</summary>
		/// <param name="userId">The userid who performed the deletion.</param>
		/// <param name="artType">The artifact type.</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <param name="changeDate">The date of the deletion. If null, uses current date/time.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogDeletion(int projectId, int userId, ArtifactTypeEnum artifactType, int artifactId, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, int? updatedArtifactId = null, string data = null)
		{
			const string METHOD_NAME = "LogDeletion()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Let's get the artifact name, first, if we can.
			string strName1 = "";
			string strName = "";

			long changeSetId = 0;

			ArtifactField artifactField = new ArtifactField();
			ArtifactField artifactField1 = new ArtifactField();

			try
			{
				if (artifactType1 != null)
				{
					switch (artifactType1)
					{
						case ArtifactTypeEnum.TestRun:
							strName1 = new TestRunManager().RetrieveById((int)updatedArtifactId).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName1 = new IncidentManager().RetrieveById((int)updatedArtifactId, false, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName1 = new ProjectManager().RetrieveById((int)updatedArtifactId).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName1 = new ReleaseManager().RetrieveById2(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName1 = new RequirementManager().RetrieveById2(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName1 = new RequirementManager().RetrieveStepById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName1 = new TaskManager().RetrieveById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Document:
							strName1 = new AttachmentManager().RetrieveById((int)updatedArtifactId).Filename;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Filename");
							break;
						case ArtifactTypeEnum.DocumentVersion:
							strName1 = new AttachmentManager().RetrieveVersionById((int)updatedArtifactId).Filename;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName1 = new TestCaseManager().RetrieveById(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName1 = new TestSetManager().RetrieveById(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.TestConfigurationSet:
							strName1 = new TestConfigurationManager().RetrieveSetById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName1 = new AutomationManager().RetrieveHostById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName1 = new TestCaseManager().RetrieveStepById(null, (int)updatedArtifactId, true).TestCase.Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "SampleData");
							break;
						case ArtifactTypeEnum.Risk:
							strName1 = new RiskManager().Risk_RetrieveById2((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName1 = new RiskManager().RiskMitigation_RetrieveById((int)updatedArtifactId, true, includeRisk: true).Description;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)updatedArtifactId, "Description");
							artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName1 = data;
							//artifactType1 = ArtifactTypeEnum.Release;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)updatedArtifactId, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName1 = new DiscussionManager().RetrieveDocumentDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Text");
							//artifactType1 = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName1 = data;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName1 = new DiscussionManager().RetrieveRequirementDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName1 = new DiscussionManager().RetrieveReleaseDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName1 = new DiscussionManager().RetrieveTestCaseDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName1 = new DiscussionManager().RetrieveTestSetDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName1 = new DiscussionManager().RetrieveTaskDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName1 = data;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Resolution");
							//artifactType1 = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName1 = data;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							//artifactType1 = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetParameter:
							strName1 = new TestCaseManager().RetrieveTestSetParameterById((int)updatedArtifactId).Value;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Value");
							//artifactType1 = ArtifactTypeEnum.TestSet;
							break;
					}

					switch (artifactType)
					{
						case ArtifactTypeEnum.TestRun:
							strName = new TestRunManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName = new ProjectManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName = new RequirementManager().RetrieveStepById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName = new TaskManager().RetrieveById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Document:
							strName = new AttachmentManager().RetrieveById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.DocumentVersion:
							strName = new AttachmentManager().RetrieveVersionById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestConfigurationSet:
							strName = new TestConfigurationManager().RetrieveSetById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName = new TestCaseManager().RetrieveStepById(null, artifactId, true).TestCase.Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
							break;
						case ArtifactTypeEnum.Risk:
							strName = new RiskManager().Risk_RetrieveById2(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, includeRisk: true).Description;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName = data;
							//artifactType = ArtifactTypeEnum.Release;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactId, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
							//artifactType = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName = new DiscussionManager().RetrieveRiskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName = new DiscussionManager().RetrieveTaskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName = data;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
							//artifactType = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName = new TestCaseManager().RetrieveParameterById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetParameter:
							strName = new TestCaseManager().RetrieveTestSetParameterById(artifactId).Value;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Value");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.ReleaseTestCase:
							if (updatedArtifactId != null)
							{
								strName = new TestCaseManager().RetrieveById(projectId, (int)updatedArtifactId, true).Name;
								artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							}
							break;
					}
					//Create a new changeset.
					HistoryChangeSet hsChangeSet = new HistoryChangeSet();
					hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
					hsChangeSet.UserId = userId;
					hsChangeSet.ArtifactTypeId = (int)artifactType;
					hsChangeSet.ArtifactId = artifactId;
					hsChangeSet.ArtifactDesc = strName;
					hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
					hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Deleted;
					hsChangeSet.RevertId = null;

					HistoryDetail historyDetail = new HistoryDetail();
					hsChangeSet.Details.Add(historyDetail);
					if (artifactField1 != null)
					{
						if (artifactField1.ArtifactFieldId > 0)
						{
							historyDetail.FieldId = artifactField1.ArtifactFieldId;
						}
						else
						{
							historyDetail.FieldId = null;
						}
						if (artifactField1.Name != null)
						{
							historyDetail.FieldName = artifactField1.Name;
						}
						else
						{
							historyDetail.FieldName = "";
						}
					}
					else
					{
						historyDetail.FieldId = null;
						historyDetail.FieldName = "";
					}
					historyDetail.FieldCaption = "Delete";
					historyDetail.NewValue = strName1 + " - " + artifactType1;
					historyDetail.OldValue = strName1 + " - " + artifactType1 + " deleted from " + strName + " - " + artifactType;
					changeSetId = Insert(hsChangeSet);
				}
				else
				{
					switch (artifactType)
					{
						case ArtifactTypeEnum.TestRun:
							if (data != null)
							{
								strName = data;
							}
							else
							{
								strName = new TestRunManager().RetrieveById1(artifactId).Name;
							}
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName = new ProjectManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName = new RequirementManager().RetrieveStepById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName = new TaskManager().RetrieveById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Document:
							if (data != null)
							{
								strName = data;
							}
							else
							{
								strName = new AttachmentManager().RetrieveById(artifactId).Filename;
							}
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName = new TestCaseManager().RetrieveStepById(null, artifactId, true).TestCase.Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
							break;
						case ArtifactTypeEnum.TestConfigurationSet:
							strName = new TestConfigurationManager().RetrieveSetById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Risk:
							strName = new RiskManager().Risk_RetrieveById2(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, includeRisk: true).Description;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName = data;
							//artifactType = ArtifactTypeEnum.Release;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactId, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
							//artifactType = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName = new DiscussionManager().RetrieveRiskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName = new DiscussionManager().RetrieveTaskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName = data;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
							//artifactType = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName = new TestCaseManager().RetrieveParameterById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetParameter:
							strName = new TestCaseManager().RetrieveTestSetParameterById(artifactId).Value;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Value");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
					}
					//Create a new changeset.
					HistoryChangeSet hsChangeSet = new HistoryChangeSet();
					hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
					hsChangeSet.UserId = userId;
					hsChangeSet.ArtifactTypeId = (int)artifactType;
					hsChangeSet.ArtifactId = artifactId;
					hsChangeSet.ArtifactDesc = strName;
					hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
					hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Deleted;
					hsChangeSet.RevertId = null;

					HistoryDetail historyDetail = new HistoryDetail();
					hsChangeSet.Details.Add(historyDetail);
					if (artifactField != null)
					{
						if (artifactField.ArtifactFieldId > 0)
						{
							historyDetail.FieldId = artifactField.ArtifactFieldId;
						}
						else
						{
							historyDetail.FieldId = null;
						}
						if (artifactField.Name != null)
						{
							historyDetail.FieldName = artifactField.Name;
						}
						else
						{
							historyDetail.FieldName = "";
						}
					}
					else
					{
						historyDetail.FieldId = null;
						historyDetail.FieldName = "";
					}
					historyDetail.FieldCaption = "Delete";
					historyDetail.NewValue = "";
					historyDetail.OldValue = strName;
					changeSetId = Insert(hsChangeSet);
				}
			}
			catch (Exception ex)
			{
				//Simply log it.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		/// <summary>Logs a deletion into the ChangeSet for the specified artifact id, type.</summary>
		/// <param name="userId">The userid who performed the deletion.</param>
		/// <param name="artType">The artifact type.</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <param name="changeDate">The date of the deletion. If null, uses current date/time.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogUnDeletion(int projectId, int userId, ArtifactTypeEnum artifactType, int artifactId, long rollbackId, DateTime? changeDate = null)
		{
			const string METHOD_NAME = "LogUnDeletion()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Let's get the artifact name, first, if we can.
			string strName = "";

			ArtifactField artifactField = new ArtifactField();
			try
			{
				switch (artifactType)
				{
					case ArtifactTypeEnum.TestRun:
						strName = new TestRunManager().RetrieveById(artifactId).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Incident:
						strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Project:
						strName = new ProjectManager().RetrieveById(artifactId).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Release:
						strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Requirement:
						strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.RequirementStep:
						strName = new RequirementManager().RetrieveStepById(artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Task:
						strName = new TaskManager().RetrieveById(artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.Document:
						strName = new AttachmentManager().RetrieveById(artifactId).Filename;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
						break;
					case ArtifactTypeEnum.TestCase:
						strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.TestSet:
						strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.TestConfigurationSet:
						strName = new TestConfigurationManager().RetrieveSetById(artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.AutomationHost:
						strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.TestStep:
						strName = new TestCaseManager().RetrieveStepById(null, artifactId, true).TestCase.Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
						break;
					case ArtifactTypeEnum.Risk:
						strName = new RiskManager().Risk_RetrieveById2(artifactId, true).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						break;
					case ArtifactTypeEnum.RiskMitigation:
						strName = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, includeRisk: true).Description;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
						artifactType = ArtifactTypeEnum.Risk;
						break;
					//case ArtifactTypeEnum.ProjectBaseline:
					//	strName = data;
					//	artifactType = ArtifactTypeEnum.Release;
					//	artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactId, "Name");
					//	break;
					case ArtifactTypeEnum.DocumentDiscussion:
						strName = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
						artifactType = ArtifactTypeEnum.Document;
						break;
					case ArtifactTypeEnum.RiskDiscussion:
						strName = new DiscussionManager().RetrieveRiskDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.Risk;
						break;
					case ArtifactTypeEnum.RequirementDiscussion:
						strName = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.Requirement;
						break;
					case ArtifactTypeEnum.ReleaseDiscussion:
						strName = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.Release;
						break;
					case ArtifactTypeEnum.TestCaseDiscussion:
						strName = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.TestCase;
						break;
					case ArtifactTypeEnum.TestSetDiscussion:
						strName = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.TestSet;
						break;
					case ArtifactTypeEnum.TaskDiscussion:
						strName = new DiscussionManager().RetrieveTaskDiscussionById(artifactId).Text;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
						artifactType = ArtifactTypeEnum.Task;
						break;
					//case ArtifactTypeEnum.IncidentResolution:
					//	strName = data;
					//	artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
					//	artifactType = ArtifactTypeEnum.Incident;
					//	break;
					case ArtifactTypeEnum.TestCaseParameter:
						strName = new TestCaseManager().RetrieveParameterById(artifactId).Name;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
						artifactType = ArtifactTypeEnum.TestCase;
						break;
					case ArtifactTypeEnum.TestSetParameter:
						strName = new TestCaseManager().RetrieveTestSetParameterById(artifactId).Value;
						artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Value");
						artifactType = ArtifactTypeEnum.TestSet;
						break;
				}
			}
			catch (Exception ex)
			{
				//Simply log it.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			// Create a new changeset.
			HistoryChangeSet hsChangeSet = new HistoryChangeSet();
			hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
			hsChangeSet.UserId = userId;
			hsChangeSet.ArtifactTypeId = (int)artifactType;
			hsChangeSet.ArtifactId = artifactId;
			hsChangeSet.ArtifactDesc = strName;
			hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Undelete;
			hsChangeSet.RevertId = null;

			HistoryDetail historyDetail = new HistoryDetail();
			hsChangeSet.Details.Add(historyDetail);
			if (artifactField != null)
			{
				if (artifactField.ArtifactFieldId > 0)
				{
					historyDetail.FieldId = artifactField.ArtifactFieldId;
				}
				else
				{
					historyDetail.FieldId = null;
				}
				if (artifactField.Name != null)
				{
					historyDetail.FieldName = artifactField.Name;
				}
				else
				{
					historyDetail.FieldName = "";
				}
			}
			else
			{
				historyDetail.FieldId = null;
				historyDetail.FieldName = "";
			}
			historyDetail.FieldCaption = "UnDelete";
			historyDetail.NewValue = strName;
			historyDetail.OldValue = "";
			long changeSetId = Insert(hsChangeSet);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;

		}

		/// <summary>Logs a creation into the ChangeSet for the specified artifact id, type.</summary>
		/// <param name="userId">The userid who performed the creation.</param>
		/// <param name="artType">The artifact type.</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <param name="changeDate">The date of the creation. If null, uses current date/time.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogCreation(int projectId, int userId, ArtifactTypeEnum artifactType, int artifactId, DateTime? changeDate = null, ArtifactTypeEnum? artifactType1 = null, int? updatedArtifactId = null, string status = null)
		{
			const string METHOD_NAME = "LogCreation()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Let's get the artifact name, first, if we can.
			string strName = "";
			string strName1 = "";
			string oldval = "";

			long changeSetId = 0;

			ArtifactField artifactField = new ArtifactField();
			ArtifactField artifactField1 = new ArtifactField();

			try
			{
				if (artifactType1 != null)
				{
					switch (artifactType1)
					{
						case ArtifactTypeEnum.TestRun:
							strName1 = new TestRunManager().RetrieveById((int)updatedArtifactId).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName1 = new IncidentManager().RetrieveById((int)updatedArtifactId, false, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName1 = new ProjectManager().RetrieveById((int)updatedArtifactId).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName1 = new ReleaseManager().RetrieveById2(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName1 = new RequirementManager().RetrieveById2(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName1 = new RequirementManager().RetrieveStepById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName1 = new TaskManager().RetrieveById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.Document:
							strName1 = new AttachmentManager().RetrieveById((int)updatedArtifactId).Filename;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Filename");
							break;
						case ArtifactTypeEnum.DocumentVersion:
							strName1 = new AttachmentManager().RetrieveVersionById((int)updatedArtifactId).Filename;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName1 = new TestCaseManager().RetrieveById(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName1 = new TestSetManager().RetrieveById(projectId, (int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName1 = new AutomationManager().RetrieveHostById((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName1 = new TestCaseManager().RetrieveStepById(null, (int)updatedArtifactId, true).TestCase.Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "SampleData");
							break;
						case ArtifactTypeEnum.Risk:
							strName1 = new RiskManager().Risk_RetrieveById2((int)updatedArtifactId, true).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName1 = new RiskManager().RiskMitigation_RetrieveById((int)updatedArtifactId, true, includeRisk: true).Description;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Description");
							//artifactType1 = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName1 = new BaselineManager().Baseline_RetrieveById((int)updatedArtifactId).Name;
							artifactType1 = ArtifactTypeEnum.ProjectBaseline;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName1 = new DiscussionManager().RetrieveDocumentDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Text");
							//artifactType1 = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName1 = new DiscussionManager().RetrieveRiskDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName1 = new DiscussionManager().RetrieveRequirementDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName1 = new DiscussionManager().RetrieveReleaseDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName1 = new DiscussionManager().RetrieveTestCaseDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName1 = new DiscussionManager().RetrieveTestSetDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName1 = new DiscussionManager().RetrieveTaskDiscussionById((int)updatedArtifactId).Text;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Comment");
							//artifactType1 = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName1 = new IncidentManager().Resolution_RetrieveByResolutionId((int)updatedArtifactId).Resolution;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Resolution");
							//artifactType1 = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName1 = new TestCaseManager().RetrieveParameterById((int)updatedArtifactId).Name;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Name");
							//artifactType1 = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetParameter:
							strName1 = new TestCaseManager().RetrieveTestSetParameterById((int)updatedArtifactId).Value;
							artifactField1 = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType1, "Value");
							//artifactType1 = ArtifactTypeEnum.TestSet;
							break;

					}

					switch (artifactType)
					{
						case ArtifactTypeEnum.TestRun:
							strName = new TestRunManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName = new ProjectManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName = new RequirementManager().RetrieveStepById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName = new TaskManager().RetrieveById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Document:
							strName = new AttachmentManager().RetrieveById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.DocumentVersion:
							strName = new AttachmentManager().RetrieveVersionById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName = new TestCaseManager().RetrieveStepById(null, artifactId, true).TestCase.Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
							break;
						case ArtifactTypeEnum.Risk:
							strName = new RiskManager().Risk_RetrieveById2(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, includeRisk: true).Description;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName = new BaselineManager().Baseline_RetrieveById(artifactId).Name;
							//artifactType = ArtifactTypeEnum.Release;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactId, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
							//artifactType = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName = new DiscussionManager().RetrieveRiskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName = new DiscussionManager().RetrieveTaskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							artifactType = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName = new IncidentManager().Resolution_RetrieveByResolutionId(artifactId).Resolution;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
							//artifactType = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName = new TestCaseManager().RetrieveParameterById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetParameter:
							strName = new TestCaseManager().RetrieveTestSetParameterById(artifactId).Value;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Value");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TestCaseSignature:
							strName = new TestCaseManager().RetrieveTestSetParameterById(artifactId).Value;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;

					}
					//Create a new changeset.
					HistoryChangeSet hsChangeSet = new HistoryChangeSet();
					hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
					hsChangeSet.UserId = userId;
					hsChangeSet.ArtifactTypeId = (int)artifactType;
					hsChangeSet.ArtifactId = artifactId;
					hsChangeSet.ArtifactDesc = strName;
					hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
					hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Modified;
					hsChangeSet.RevertId = null;

					if(artifactType == Artifact.ArtifactTypeEnum.TestCase || artifactType == Artifact.ArtifactTypeEnum.TestCaseSignature)
					{
						var testcaseSign = new TestCaseManager().RetrieveExistingSignaturesForTestCaseByUser(artifactId, userId);
						if (testcaseSign != null)
						{
							hsChangeSet.Meaning = testcaseSign.Meaning;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					HistoryDetail historyDetail = new HistoryDetail();
					hsChangeSet.Details.Add(historyDetail);
					if (artifactField1 != null)
					{
						if (artifactField1.ArtifactFieldId > 0)
						{
							historyDetail.FieldId = artifactField1.ArtifactFieldId;
						}
						else
						{
							historyDetail.FieldId = null;
						}
						if (artifactField1.Name != null)
						{
							historyDetail.FieldName = artifactField1.Name;
						}
						else
						{
							historyDetail.FieldName = "";
						}
					}
					else
					{
						historyDetail.FieldId = null;
						historyDetail.FieldName = "";
					}
					historyDetail.FieldCaption = "Insert";
					historyDetail.OldValue = "";
					historyDetail.NewValue = strName1 + " - " + artifactType1 + " added to " + strName + " - " + artifactType;
					changeSetId = Insert(hsChangeSet);
				}
				else
				{
					switch (artifactType)
					{
						case ArtifactTypeEnum.TestRun:
							strName = new TestRunManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Incident:
							strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Project:
							strName = new ProjectManager().RetrieveById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Release:
							strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Requirement:
							strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RequirementStep:
							strName = new RequirementManager().RetrieveStepById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Task:
							strName = new TaskManager().RetrieveById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.Document:
							strName = new AttachmentManager().RetrieveById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.DocumentVersion:
							strName = new AttachmentManager().RetrieveVersionById(artifactId).Filename;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
							break;
						case ArtifactTypeEnum.TestCase:
							strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestSet:
							strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.AutomationHost:
							strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.TestStep:
							strName = new TestCaseManager().RetrieveStepById(null, artifactId, true).TestCase.Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
							break;
						case ArtifactTypeEnum.Risk:
							strName = new RiskManager().Risk_RetrieveById2(artifactId, true).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.RiskMitigation:
							strName = new RiskManager().RiskMitigation_RetrieveById(artifactId, true, includeRisk: true).Description;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.ProjectBaseline:
							strName = new BaselineManager().Baseline_RetrieveById(artifactId).Name;
							//artifactType = ArtifactTypeEnum.Release;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							break;
						case ArtifactTypeEnum.DocumentDiscussion:
							strName = new DiscussionManager().RetrieveDocumentDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
							artifactType = ArtifactTypeEnum.Document;
							break;
						case ArtifactTypeEnum.RiskDiscussion:
							strName = new DiscussionManager().RetrieveRiskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Risk;
							break;
						case ArtifactTypeEnum.RequirementDiscussion:
							strName = new DiscussionManager().RetrieveRequirementDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Requirement;
							break;
						case ArtifactTypeEnum.ReleaseDiscussion:
							strName = new DiscussionManager().RetrieveReleaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Release;
							break;
						case ArtifactTypeEnum.TestCaseDiscussion:
							strName = new DiscussionManager().RetrieveTestCaseDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.TestSetDiscussion:
							strName = new DiscussionManager().RetrieveTestSetDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TaskDiscussion:
							strName = new DiscussionManager().RetrieveTaskDiscussionById(artifactId).Text;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
							//artifactType = ArtifactTypeEnum.Task;
							break;
						case ArtifactTypeEnum.IncidentResolution:
							strName = new IncidentManager().Resolution_RetrieveByResolutionId(artifactId).Resolution;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
							//artifactType = ArtifactTypeEnum.Incident;
							break;
						case ArtifactTypeEnum.TestCaseParameter:
							strName = new TestCaseManager().RetrieveParameterById(artifactId).Name;
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
							//artifactType = ArtifactTypeEnum.TestCase;
							break;
						case ArtifactTypeEnum.RequirementSignature:
							int reqstatusId = new RequirementManager().RetrieveReqSignature(artifactId, userId).STATUS_ID;
							var req = new RequirementManager().RetrieveReqSignature(artifactId, userId);
							string originalreqString = req.MEANING;
							//string reqtargetWord = "from ";

							//// Find the position of the word "from" and "to"
							//int reqfromIndex = originalreqString.IndexOf("from") + 5; // Skip past "from "
							//int reqtoIndex = originalreqString.IndexOf("to") - 1; // Skip before "to "

							//// Extract the substring between "from" and "to"
							//string reqstatusBefore = originalreqString.Substring(reqfromIndex, reqtoIndex - reqfromIndex);

							//// Trim any extra spaces or quotes
							//reqstatusBefore = reqstatusBefore.Trim().Trim('\'');

							oldval = status;

							// Find the originalString of the word "to"
							int reqtoIndex = originalreqString.IndexOf("to") + 3; // Skip past "to " (the space after "to")

							// Extract the substring starting after "to "
							string reqstatusAfter = originalreqString.Substring(reqtoIndex).Trim().Trim('\'');
							strName = reqstatusAfter;
							//strName = new RequirementManager().RetrieveStatusById(reqstatusId).Name;							

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.TestCaseSignature:
							int statusId = new TestCaseManager().RetrieveTestCaseSignature(artifactId).StatusId;
							var st = new TestCaseManager().RetrieveTestCaseSignature(artifactId);
							string originalString = st.Meaning;
							// Find the position of the word "from" and "to"
							int fromIndex = originalString.IndexOf("from") + 5; // Skip past "from "
							int toIndex = originalString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string statusBefore = originalString.Substring(fromIndex, toIndex - fromIndex);

							// Trim any extra spaces or quotes
							statusBefore = statusBefore.Trim().Trim('\'');

							oldval = statusBefore;

							if (statusId == 9)
							{
								if (string.IsNullOrWhiteSpace(originalString))
								{
									strName = string.Empty;
								}

								else
								{
									// Find the originalString of the word "to"
									int toIndex1 = originalString.IndexOf("to") + 3; // Skip past "to " (the space after "to")

									// Extract the substring starting after "to "
									string statusAfter = originalString.Substring(toIndex1).Trim().Trim('\'');
									strName = statusAfter;
								}
							}
							else
							{
								strName = new TestCaseManager().RetrieveStatusById(statusId).Name;
							}
							
							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.ReleaseSignature:
							int artifactTypeId = (int)ArtifactTypeEnum.Release;
							int releasestatusId = new ReleaseManager().RetrieveReleaseSignature(artifactId, artifactTypeId).STATUS_ID;
							var release = new ReleaseManager().RetrieveReleaseSignature(artifactId, artifactTypeId);
							string originalReleaseString = release.MEANING;
							// Find the position of the word "from" and "to"
							int releasefromIndex = originalReleaseString.IndexOf("from") + 5; // Skip past "from "
							int releasetoIndex = originalReleaseString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string releasestatusBefore = originalReleaseString.Substring(releasefromIndex, releasetoIndex - releasefromIndex);

							// Trim any extra spaces or quotes
							releasestatusBefore = releasestatusBefore.Trim().Trim('\'');

							oldval = releasestatusBefore;
							strName = new ReleaseManager().RetrieveStatusById(releasestatusId).Name;						

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							//artifactType = ArtifactTypeEnum.TestSet;
							break;
						case ArtifactTypeEnum.DocumentSignature:
							int documentartifactTypeId = (int)ArtifactTypeEnum.Document;
							int documentstatusId = new AttachmentManager().RetrieveDocumentSignature(artifactId, documentartifactTypeId).STATUS_ID;
							var document = new AttachmentManager().RetrieveDocumentSignature(artifactId, documentartifactTypeId);
							string originaldocumentString = document.MEANING;
							// Find the position of the word "from" and "to"
							int documentfromIndex = originaldocumentString.IndexOf("from") + 5; // Skip past "from "
							int documenttoIndex = originaldocumentString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string documentstatusBefore = originaldocumentString.Substring(documentfromIndex, documenttoIndex - documentfromIndex);

							// Trim any extra spaces or quotes
							documentstatusBefore = documentstatusBefore.Trim().Trim('\'');

							oldval = documentstatusBefore;

							strName = new AttachmentManager().RetrieveStatusById(documentstatusId).Name;

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							break;
						case ArtifactTypeEnum.IncidentSignature:
							int incidentartifactTypeId = (int)ArtifactTypeEnum.Incident;
							int incidentstatusId = new IncidentManager().RetrieveIncidentSignature(artifactId, incidentartifactTypeId).STATUS_ID;
							var incident = new IncidentManager().RetrieveIncidentSignature(artifactId, incidentartifactTypeId);
							string originalincidentString = incident.MEANING;
							// Find the position of the word "from" and "to"
							int incidentfromIndex = originalincidentString.IndexOf("from") + 5; // Skip past "from "
							int incidenttoIndex = originalincidentString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string incidentstatusBefore = originalincidentString.Substring(incidentfromIndex, incidenttoIndex - incidentfromIndex);

							// Trim any extra spaces or quotes
							incidentstatusBefore = incidentstatusBefore.Trim().Trim('\'');

							oldval = incidentstatusBefore;

							strName = new IncidentManager().RetrieveStatusById(incidentstatusId).Name;

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							break;
						case ArtifactTypeEnum.RiskSignature:
							int riskartifactTypeId = (int)ArtifactTypeEnum.Risk;
							int riskstatusId = new RiskManager().RetrieveRiskSignature(artifactId, riskartifactTypeId).STATUS_ID;
							var risk = new RiskManager().RetrieveRiskSignature(artifactId, riskartifactTypeId);
							string originalriskString = risk.MEANING;
							// Find the position of the word "from" and "to"
							int riskfromIndex = originalriskString.IndexOf("from") + 5; // Skip past "from "
							int risktoIndex = originalriskString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string riskstatusBefore = originalriskString.Substring(riskfromIndex, risktoIndex - riskfromIndex);

							// Trim any extra spaces or quotes
							riskstatusBefore = riskstatusBefore.Trim().Trim('\'');

							oldval = riskstatusBefore;

							strName = new RiskManager().RetrieveStatusById(riskstatusId).Name;

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							break;
						case ArtifactTypeEnum.TaskSignature:
							int taskartifactTypeId = (int)ArtifactTypeEnum.Task;
							int taskstatusId = new TaskManager().RetrieveTaskSignature(artifactId, taskartifactTypeId).STATUS_ID;
							var task = new TaskManager().RetrieveTaskSignature(artifactId, taskartifactTypeId);
							string originaltaskString = task.MEANING;
							// Find the position of the word "from" and "to"
							int taskfromIndex = originaltaskString.IndexOf("from") + 5; // Skip past "from "
							int tasktoIndex = originaltaskString.IndexOf("to") - 1; // Skip before "to "

							// Extract the substring between "from" and "to"
							string taskstatusBefore = originaltaskString.Substring(taskfromIndex, tasktoIndex - taskfromIndex);

							// Trim any extra spaces or quotes
							taskstatusBefore = taskstatusBefore.Trim().Trim('\'');

							oldval = taskstatusBefore;

							strName = new TaskManager().RetrieveStatusById(taskstatusId).Name;

							artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "StatusId");
							break;
					}
					//Create a new changeset.
					HistoryChangeSet hsChangeSet = new HistoryChangeSet();
					hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
					hsChangeSet.UserId = userId;
					hsChangeSet.ArtifactTypeId = (int)artifactType;
					hsChangeSet.ArtifactId = artifactId;
					hsChangeSet.ArtifactDesc = strName;
					hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
					hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Added;
					hsChangeSet.RevertId = null;

					if (artifactType == Artifact.ArtifactTypeEnum.TestCaseSignature)
					{
						var testcaseSign = new TestCaseManager().RetrieveExistingSignaturesForLastTestCaseByUser(artifactId, userId);
						if (testcaseSign != null)
						{
							hsChangeSet.Meaning = testcaseSign.Meaning;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.ReleaseSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Release;
						var releaseSign = new ReleaseManager().RetrieveReleaseSignature(artifactId, artifactTypeId);
						if (releaseSign != null)
						{
							hsChangeSet.Meaning = releaseSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.DocumentSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Document;
						var documentSign = new AttachmentManager().RetrieveDocumentSignature(artifactId, artifactTypeId);
						if (documentSign != null)
						{
							hsChangeSet.Meaning = documentSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.IncidentSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Incident;
						var incidentSign = new IncidentManager().RetrieveIncidentSignature(artifactId, artifactTypeId);
						if (incidentSign != null)
						{
							hsChangeSet.Meaning = incidentSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.TaskSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Task;
						var taskSign = new TaskManager().RetrieveTaskSignature(artifactId, artifactTypeId);
						if (taskSign != null)
						{
							hsChangeSet.Meaning = taskSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.RiskSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Risk;
						var riskSign = new RiskManager().RetrieveRiskSignature(artifactId, artifactTypeId);
						if (riskSign != null)
						{
							hsChangeSet.Meaning = riskSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					if (artifactType == Artifact.ArtifactTypeEnum.RequirementSignature)
					{
						int artifactTypeId = (int)ArtifactTypeEnum.Requirement;
						var reqSign = new RequirementManager().RetrieveReqSignature(artifactId, userId);
						if (reqSign != null)
						{
							hsChangeSet.Meaning = reqSign.MEANING;
							//Get the permanent change information
							//We HASH this to avoid tampering
							string signature = hsChangeSet.UserId + ":" + hsChangeSet.ArtifactTypeId + ":" + hsChangeSet.ArtifactId + ":" + hsChangeSet.ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

							//Create the hash using SHA256
							hsChangeSet.SignatureHash = SimpleHash.GetHashSha256(signature);
							//hsChangeSet.SignatureHash = testcaseSign.User.FullName;
						}
					}

					HistoryDetail historyDetail = new HistoryDetail();
					hsChangeSet.Details.Add(historyDetail);
					if (artifactField != null)
					{

						if (artifactField.ArtifactFieldId > 0)
						{
							historyDetail.FieldId = artifactField.ArtifactFieldId;
						}
						else
						{
							historyDetail.FieldId = null;
						}
						if (artifactField.Name != null)
						{
							historyDetail.FieldName = artifactField.Name;
						}
						else
						{
							historyDetail.FieldName = "";
						}
					}
					else
					{
						historyDetail.FieldId = null;
						historyDetail.FieldName = "";
					}
					historyDetail.FieldCaption = "Insert";
					historyDetail.OldValue = oldval;
					historyDetail.NewValue = strName;
					changeSetId = Insert(hsChangeSet);
				}
			}
			catch (Exception ex)
			{
				//Simply log it.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}




			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		/// <summary>Logs an import into the ChangeSet for the specified artifact id, type.</summary>
		/// <param name="userId">The userid who performed the creation.</param>
		/// <param name="artType">The artifact type.</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <param name="fromProjectId">The project ID the item was imported from.</param>
		/// <param name="artifactType">The typwe of the artifact.</param>
		/// <param name="projectId">The project ID the item was imported into.</param>
		/// <param name="changeDate">The date of the creation. If null, uses current date/time.</param>
		/// <param name="fromArtifactId">The original artifact ID.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogImport(int projectId, int fromProjectId, int fromArtifactId, int userId, ArtifactTypeEnum artifactType, int artifactId, DateTime? changeDate = null)
		{
			const string METHOD_NAME = CLASS_NAME + "LogCreation(projectId,fromProjectId, fromArtifactId, userId, artType, artifactId,[changeDate])";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Let's get the artifact name, first, if we can.
			string strName = "";
			try
			{
				switch (artifactType)
				{
					case ArtifactTypeEnum.Incident:
						strName = new IncidentManager().RetrieveById(artifactId, false, true).Name;
						break;
					case ArtifactTypeEnum.Project:
						strName = new ProjectManager().RetrieveById(artifactId).Name;
						break;
					case ArtifactTypeEnum.Release:
						strName = new ReleaseManager().RetrieveById2(projectId, artifactId, true).Name;
						break;
					case ArtifactTypeEnum.Requirement:
						strName = new RequirementManager().RetrieveById2(projectId, artifactId, true).Name;
						break;
					case ArtifactTypeEnum.Task:
						strName = new TaskManager().RetrieveById(artifactId, true).Name;
						break;
					case ArtifactTypeEnum.Document:
						strName = new AttachmentManager().RetrieveById(artifactId).Filename;
						break;
					case ArtifactTypeEnum.TestCase:
						strName = new TestCaseManager().RetrieveById(projectId, artifactId, true).Name;
						break;
					case ArtifactTypeEnum.TestSet:
						strName = new TestSetManager().RetrieveById(projectId, artifactId, true).Name;
						break;
					case ArtifactTypeEnum.AutomationHost:
						strName = new AutomationManager().RetrieveHostById(artifactId, true).Name;
						break;
				}
			}
			catch (Exception ex)
			{
				//Simply log it.
				Logger.LogErrorEvent(METHOD_NAME, ex);
			}

			//Create a new changeset for the Import
			HistoryChangeSet hsChangeSet = new HistoryChangeSet();
			hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)projectId;
			hsChangeSet.UserId = userId;
			hsChangeSet.ArtifactTypeId = (int)artifactType;
			hsChangeSet.ArtifactId = artifactId;
			hsChangeSet.ArtifactDesc = strName;
			hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Imported;
			hsChangeSet.RevertId = null;

			//Now add a history record to the changeset
			string strFrom = String.Format(GlobalResources.General.Global_ImportedFrom, fromProjectId.ToString(), artifactType.ToString(), fromArtifactId.ToString());
			HistoryDetail historyDetail = new HistoryDetail();
			hsChangeSet.Details.Add(historyDetail);
			historyDetail.FieldName = GlobalResources.General.History_Imported;
			historyDetail.FieldCaption = GlobalResources.General.History_Imported;
			historyDetail.OldValue = strFrom;

			//Insert this changeset
			long changeSetId = Insert(hsChangeSet);

			//Now create one for the Export..
			hsChangeSet = new HistoryChangeSet();
			hsChangeSet.ProjectId = (artifactType == ArtifactTypeEnum.Project) ? null : (int?)fromProjectId;
			hsChangeSet.UserId = userId;
			hsChangeSet.ArtifactTypeId = (int)artifactType;
			hsChangeSet.ArtifactId = fromArtifactId;
			hsChangeSet.ArtifactDesc = strName;
			hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Exported;
			hsChangeSet.RevertId = null;

			//Now add a history record to the changeset
			string strTo = String.Format(GlobalResources.General.Global_ExportedTo, projectId.ToString(), artifactType.ToString(), artifactId.ToString());
			historyDetail = new HistoryDetail();
			hsChangeSet.Details.Add(historyDetail);
			historyDetail.FieldName = GlobalResources.General.History_Exported;
			historyDetail.FieldCaption = GlobalResources.General.History_Exported;
			historyDetail.OldValue = strTo;

			//Save changes.
			changeSetId = Insert(hsChangeSet);

			Logger.LogExitingEvent(METHOD_NAME);
			return changeSetId;
		}

		/// <summary>Log a purge of an item. Will remove all previous history items and changesets, and insert a single purge changeset.</summary>
		/// <param name="projectId">The ProjectId</param>
		/// <param name="userId">The UserId performing the Purge.</param>
		/// <param name="artifactType">The artifact Type.</param>
		/// <param name="artifactId">The Artifact ID</param>
		/// <param name="changeDate">The date of the deletion. Default: Null = Now</param>
		/// <returns>The ChangeSet ID of the log.</returns>
		internal long LogPurge(int projectId, int userId, ArtifactTypeEnum artifactType, int artifactId, DateTime? changeDate = null, string strName = null)
		{
			const string METHOD_NAME = "LogPurge()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			ArtifactField artifactField = new ArtifactField();
			//Remove all previous entries for the item, first.
			DeleteChangeSets(artifactType, artifactId);

			switch (artifactType)
			{
				case ArtifactTypeEnum.TestRun:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Incident:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Project:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Release:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Requirement:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.RequirementStep:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Task:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Document:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Filename");
					break;
				case ArtifactTypeEnum.TestCase:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.TestSet:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.AutomationHost:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.TestStep:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "SampleData");
					break;
				case ArtifactTypeEnum.TestConfigurationSet:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.Risk:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					break;
				case ArtifactTypeEnum.RiskMitigation:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Description");
					//artifactType = ArtifactTypeEnum.Risk;
					break;
				case ArtifactTypeEnum.ProjectBaseline:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactId, "Name");
					break;
				case ArtifactTypeEnum.DocumentDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Text");
					//artifactType = ArtifactTypeEnum.Document;
					break;
				case ArtifactTypeEnum.RiskDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.Risk;
					break;
				case ArtifactTypeEnum.RequirementDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.Requirement;
					break;
				case ArtifactTypeEnum.ReleaseDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.Release;
					break;
				case ArtifactTypeEnum.TestCaseDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.TestCase;
					break;
				case ArtifactTypeEnum.TestSetDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.TestSet;
					break;
				case ArtifactTypeEnum.TaskDiscussion:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Comment");
					//artifactType = ArtifactTypeEnum.Task;
					break;
				case ArtifactTypeEnum.IncidentResolution:
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Resolution");
					//artifactType = ArtifactTypeEnum.Incident;
					break;
				case ArtifactTypeEnum.TestCaseParameter:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Name");
					//artifactType = ArtifactTypeEnum.TestCase;
					break;
				case ArtifactTypeEnum.TestSetParameter:
					
					artifactField = new ArtifactManager().ArtifactField_RetrieveByArtifactId((int)artifactType, "Value");
					//artifactType = ArtifactTypeEnum.TestSet;
					break;
			}

			//Create a new changeset.
			HistoryChangeSet hsChangeSet = new HistoryChangeSet();
			hsChangeSet.ProjectId = (int?)projectId;
			hsChangeSet.UserId = userId;
			hsChangeSet.ArtifactTypeId = (int)artifactType;
			hsChangeSet.ArtifactId = artifactId;
			hsChangeSet.ArtifactDesc = strName;
			hsChangeSet.ChangeDate = ((changeDate.HasValue) ? changeDate.Value : DateTime.UtcNow);
			hsChangeSet.ChangeTypeId = (int)ChangeSetTypeEnum.Purged;
			hsChangeSet.RevertId = null;

			HistoryDetail historyDetail = new HistoryDetail();
			hsChangeSet.Details.Add(historyDetail);

			if (artifactField != null)
			{
				if (artifactField.ArtifactFieldId > 0)
				{
					historyDetail.FieldId = artifactField.ArtifactFieldId;
				}
				else
				{
					historyDetail.FieldId = null;
				}
				if (artifactField.Name != null)
				{
					historyDetail.FieldName = artifactField.Name;
				}
				else
				{
					historyDetail.FieldName = "";
				}
			}
			else
			{
				historyDetail.FieldId = null;
				historyDetail.FieldName = "";
			}

			historyDetail.FieldCaption = "Purge";
			historyDetail.NewValue = "";
			historyDetail.OldValue = strName + " Deleted Permanently";
			long changeSetId = Insert(hsChangeSet);
		
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		/// <summary>For internal use, removes all changesets for the given Artifact Type and ID</summary>
		/// <param name="artifactType">The Artifact Type. If null, will delete the specified Project ID.</param>
		/// <param name="artifactId">The Artifact Id</param>
		internal void DeleteChangeSets(ArtifactTypeEnum? artifactType, int artifactId)
		{
			const string METHOD_NAME = "DeleteChangeSets()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the appropriate stored procedure depending on whether we're deleting for a project or artifact
					if (artifactType.HasValue)
					{
						context.History_DeleteChangeSets((int)(artifactType.Value), artifactId, null);
					}
					else
					{
						context.History_DeleteChangeSets(null, null, artifactId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}
		#endregion Internal Methods

		#region Protected Methods
		/// <summary>Curerently called only by TemplateManager.</summary>
		/// <param name="historyDetailId"></param>
		/// <returns></returns>
		protected internal HistoryDetail HistoryDetail_Retrieve_ById(long historyDetailId)
		{
			const string METHOD_NAME = "HistoryDetail_Retrieve_ById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				HistoryDetail historyDetail;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.HistoryDetails
								where h.ArtifactHistoryId == historyDetailId
								select h;

					historyDetail = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return historyDetail;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Changes a history detail item, mainly used when a project changes template and the IDs need to be updated to match
		/// </summary>
		/// <param name="historyDetail">The history detail record</param>
		/// <param name="userId">The user making the change (not used currently)</param>
		protected internal void HistoryDetail_Update(HistoryDetail historyDetail, int userId)
		{
			const string METHOD_NAME = "HistoryDetail_Update()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach and apply changes
					context.HistoryDetails.ApplyChanges(historyDetail);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of history changesets and detail entries for the project and artifact type specified
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="startRow">The start row</param>
		/// <param name="paginationSize">The page size</param>
		/// <param name="count">The count of items</param>
		/// <returns>The changeset list</returns>
		/// <remarks>
		/// Used by the TemplateManager to fix history entries when you change a project's template
		/// </remarks>
		protected internal List<HistoryChangeSet> HistoryChangeSet_RetrieveChangesForProjectAndArtifact(int projectId, ArtifactTypeEnum artifactType, out long count, int startRow = 1, int paginationSize = 50)
		{
			const string METHOD_NAME = "HistoryChangeSet_RetrieveChangesForProjectAndArtifact()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<HistoryChangeSet> changeSets;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.HistoryChangeSets
								.Include(h => h.Details)
								where h.ProjectId == projectId && h.ArtifactTypeId == (int)artifactType
								orderby h.ChangeSetId ascending
								select h;

					//Get the count
					count = query.LongCount();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					changeSets = query
						.Skip(startRow - 1)
						.Take(paginationSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return changeSets;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		#endregion Protected Methods
	}
}
