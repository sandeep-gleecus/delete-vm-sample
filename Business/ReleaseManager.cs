using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Transactions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using LinqToEdmx.Model.Conceptual;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for adding, modifying and deleting project releases/iterations</summary>
	public class ReleaseManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.Release::";

		//Default Schedule constants
		public const int DEFAULT_HOURS_PER_DAY = 8;
		public const int DEFAULT_DAYS_PER_WEEK = 5;
		public const int DEFAULT_NON_WORKING_HOURS_PER_MONTH = 0;

		//Cached lists
		public static List<ReleaseStatus> _staticReleaseStatuses = null;
		public static List<ReleaseType> _staticReleaseTypes = null;
		public static List<PeriodicReviewAlertType> _periodicReviewAlertTypes = null;

		#region Project Group Release List functions

		/// <summary>
		/// Retrieves the list of columns to display in the release list for a specific project group / user with a visible/hidden flag
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="userId">The id of the user</param>
		public List<ArtifactListFieldDisplay> RetrieveFieldsForProjectGroupLists(int projectGroupId, int userId, string collectionName)
		{
			const string METHOD_NAME = "RetrieveFieldsForProjectGroupLists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactListFieldDisplay> artifactFields = new List<ArtifactListFieldDisplay>();

				//Get the incident column settings
				UserSettingsCollection columnSettings = new UserSettingsCollection(userId, collectionName);
				columnSettings.Restore();

				//First get the list configurable fields in general
				List<ArtifactField> artifactListFields = new ArtifactManager().ArtifactField_RetrieveForLists(Artifact.ArtifactTypeEnum.Release);
				foreach (ArtifactField artifactListField in artifactListFields)
				{
					ArtifactListFieldDisplay artifactField = new ArtifactListFieldDisplay();
					artifactField.Name = artifactListField.Name;
					artifactField.ArtifactFieldTypeId = artifactListField.ArtifactFieldTypeId;
					artifactField.Caption = artifactListField.Caption;
					artifactField.LookupProperty = artifactListField.LookupProperty;
					artifactField.IsVisible = artifactListField.IsListDefault;
					artifactFields.Add(artifactField);

					//See if we have a column visibility setting specified
					if (columnSettings[artifactListField.Name] != null && columnSettings[artifactListField.Name] is bool)
					{
						artifactField.IsVisible = (bool)columnSettings[artifactListField.Name];
					}
				}

				//Return the fields
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
		/// Toggles which columns are hidden/visible for the current user/project group
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="fieldName">The field name to toggle show/hide</param>
		/// <param name="userId">The id of the user</param>
		/// <param name="collectionName">The settings collection used to store this setting</param>
		public void ToggleProjectGroupColumnVisibility(int userId, int projectGroupId, string fieldName, string collectionName)
		{
			const string METHOD_NAME = "ToggleProjectGroupColumnVisibility";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now we need to see which columns are visible from user settings
				UserSettingsCollection userSettings = new UserSettingsCollection(userId, collectionName);
				userSettings.Restore();

				if (userSettings[fieldName] == null)
				{
					//Get the default visibility setting and reverse
					ArtifactField artifactField = new ArtifactManager().ArtifactField_RetrieveByName(Artifact.ArtifactTypeEnum.Release, fieldName);
					bool isVisible = true;
					if (fieldName == "ProjectId")
					{
						//This is displayed by default so hide
						isVisible = false;
					}
					else if (artifactField != null)
					{
						isVisible = !artifactField.IsListDefault;
					}
					//Add a setting
					userSettings.Add(fieldName, isVisible);
				}
				else
				{
					//Toggle the setting
					bool isVisible = (bool)userSettings[fieldName];
					userSettings[fieldName] = !isVisible;
				}

				//Save any changes
				userSettings.Save();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>Retrieves a list of release types</summary>
		/// <returns>List of release types</returns>
		/// <remarks>Cached since they don't change</remarks>
		public List<ReleaseType> RetrieveTypes()
		{
			const string METHOD_NAME = "RetrieveTypes()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_staticReleaseTypes == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.ReleaseTypes
									where r.IsActive
									orderby r.Position, r.ReleaseTypeId
									select r;

						_staticReleaseTypes = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _staticReleaseTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_ARTIFACT_SIGNATURE RetrieveReleaseSignature(int releaseId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveReleaseSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of release in the project
				TST_ARTIFACT_SIGNATURE releaseSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ArtifactSignatures
								where t.ARTIFACT_ID == releaseId && t.ARTIFACT_TYPE_ID == artifactTypeId
								select t;

					query = query.OrderByDescending(r => r.UPDATE_DATE);

					releaseSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void ReleaseSignatureInsert(int projectId, int currentStatusId, Release release, string meaning, int? loggedinUserId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				var newReqSignature = new TST_ARTIFACT_SIGNATURE
				{
					STATUS_ID = currentStatusId,
					ARTIFACT_ID = release.ReleaseId,
					ARTIFACT_TYPE_ID = (int)ArtifactTypeEnum.Release,
					USER_ID = (int)loggedinUserId,
					UPDATE_DATE = DateTime.Now,
					MEANING = meaning,
				};

				context.ArtifactSignatures.AddObject(newReqSignature);

				context.SaveChanges();
				//log history
				new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.ReleaseSignature, release.ReleaseId, DateTime.UtcNow);

			}
		}

		public ReleaseStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				ReleaseStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ReleaseStati
								where t.ReleaseStatusId == statusId
								select t;

					status = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of release statuses</summary>
		/// <returns>List of release statuses</returns>
		/// <remarks>Cached since they don't change</remarks>
		public List<ReleaseStatus> RetrieveStatuses()
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_staticReleaseStatuses == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.ReleaseStati
									where r.IsActive
									orderby r.Position, r.ReleaseStatusId
									select r;

						_staticReleaseStatuses = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _staticReleaseStatuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of periodic review alert types</summary>
		/// <returns>List of PeriodicReviewAlertTypes</returns>
		/// <remarks>Cached since they don't change</remarks>
		public List<PeriodicReviewAlertType> RetrievePeriodicReviewAlertTypes()
		{
			const string METHOD_NAME = "RetrievePeriodicReviewAlertTypes()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_periodicReviewAlertTypes == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.PeriodicReviewAlertTypes
									where r.IsActive
									orderby r.Position
									select r;

						_periodicReviewAlertTypes = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _periodicReviewAlertTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves a list of periodic review notifications</summary>
		/// <returns>List of PeriodicReviewNotifications</returns>
		public List<PeriodicReviewNotification> RetrievePeriodicReviewNotifications(DateTime filter)
		{
			const string METHOD_NAME = "RetrievePeriodicReviewNotifications()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<PeriodicReviewNotification> notifications = new List<PeriodicReviewNotification>();


				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.PeriodicReviewNotifications
								where r.IsActive && r.ScheduledDate == filter
								select r;

					notifications = query.ToList();
				}


				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return notifications;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>This method takes the hash table of filters and returns the SQL where clause</summary>
		/// <param name="filters">The hash table of filters (property,value)</param>
		/// <returns>The SQL WHERE clause for the filters</returns>
		/// <param name="projectTemplateId">the id of the project template</param>
		/// <param name="projectId">The id of the current project</param>
		/// <remarks>Use an array list as the property value when you want a multiple set of values filtered against</remarks>
		protected string CreateFiltersClause(int projectId, int projectTemplateId, Hashtable filters, double utcOffset)
		{
			string filtersClause = "";
			if (filters != null)
			{
				IDictionaryEnumerator enumerator = filters.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string filterProperty = enumerator.Key.ToString();
					object filterValue = enumerator.Value;
					string filterColumn = "";
					EntityProperty filterPropertyInfo = null;

					//Handle the special case of coverage, since it doesn't map to a single column
					if (filterProperty.ToLower() == "fullname")
					{
						filtersClause += "AND (REL.NAME LIKE N'%" + SqlEncode((string)filterValue) + "%' OR REL.VERSION_NUMBER LIKE N'%" + SqlEncode((string)filterValue) + "%' OR REL.DESCRIPTION LIKE N'%" + SqlEncode((string)filterValue) + "%') ";
					}
					if (filterProperty.ToLower() == "coverageid")
					{
						switch ((int)filterValue)
						{
							//Not Covered
							case 1:
								filtersClause += "AND (REL.COUNT_PASSED + REL.COUNT_FAILED + REL.COUNT_CAUTION + REL.COUNT_BLOCKED + REL.COUNT_NOT_RUN) = 0 ";
								break;

							//Run Filters
							case 2:
								filtersClause += "AND REL.COUNT_NOT_RUN > 0 AND (REL.COUNT_PASSED + REL.COUNT_FAILED + REL.COUNT_CAUTION + REL.COUNT_BLOCKED) = 0 ";
								break;
							case 3:
								filtersClause += "AND REL.COUNT_NOT_RUN > 0 AND (REL.COUNT_PASSED + REL.COUNT_FAILED + REL.COUNT_CAUTION + REL.COUNT_BLOCKED) <= REL.COUNT_NOT_RUN ";
								break;
							case 4:
								filtersClause += "AND REL.COUNT_NOT_RUN > 0 ";
								break;

							//Failed Filters
							case 5:
								filtersClause += "AND REL.COUNT_FAILED > 0 ";
								break;
							case 6:
								filtersClause += "AND REL.COUNT_FAILED > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_CAUTION + REL.COUNT_BLOCKED) <= REL.COUNT_FAILED ";
								break;
							case 7:
								filtersClause += "AND REL.COUNT_FAILED > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_CAUTION + REL.COUNT_BLOCKED) = 0 ";
								break;

							//Caution Filters
							case 8:
								filtersClause += "AND REL.COUNT_CAUTION > 0 ";
								break;
							case 9:
								filtersClause += "AND REL.COUNT_CAUTION > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_FAILED + REL.COUNT_BLOCKED) <= REL.COUNT_CAUTION ";
								break;
							case 10:
								filtersClause += "AND REL.COUNT_CAUTION > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_FAILED + REL.COUNT_BLOCKED) = 0 ";
								break;

							//Blocked Filters
							case 11:
								filtersClause += "AND REL.COUNT_BLOCKED > 0 ";
								break;
							case 12:
								filtersClause += "AND REL.COUNT_BLOCKED > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_CAUTION + REL.COUNT_FAILED) <= REL.COUNT_BLOCKED ";
								break;
							case 13:
								filtersClause += "AND REL.COUNT_BLOCKED > 0 AND (REL.COUNT_PASSED + REL.COUNT_NOT_RUN + REL.COUNT_CAUTION + REL.COUNT_FAILED) = 0 ";
								break;
						}
					}
					else if (filterProperty.ToLower() == "progressid")
					{
						//Handle the special case of task progress, since it doesn't map to a single column
						switch ((int)filterValue)
						{
							//Not Started
							case 1:
								filtersClause += "AND REL.TASK_PERCENT_NOT_START > 0 ";
								break;
							//Starting Late
							case 2:
								filtersClause += "AND REL.TASK_PERCENT_LATE_START > 0 ";
								break;
							//On Schedule
							case 3:
								filtersClause += "AND REL.TASK_PERCENT_LATE_START = 0 AND REL.TASK_PERCENT_LATE_FINISH = 0 AND REL.TASK_PERCENT_ON_TIME > 0 AND REL.TASK_PERCENT_ON_TIME < 100 ";
								break;
							//Running Late
							case 4:
								filtersClause += "AND REL.TASK_PERCENT_LATE_FINISH > 0 ";
								break;
							//Completed
							case 5:
								filtersClause += "AND REL.TASK_PERCENT_ON_TIME = 100 ";
								break;
						}
					}
					else if (filterProperty.ToLower() == "completionid")
					{
						//Handle the special case of requirements completion, since it doesn't map to a single column
						switch ((int)filterValue)
						{
							//No Requirements
							case 1:
								filtersClause += "AND REL.REQUIREMENT_COUNT = 0 ";
								break;
							//= 0%
							case 2:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE = 0 ";
								break;
							//<= 25%
							case 3:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE <= 25 ";
								break;
							//<= 50%
							case 4:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE <= 50 ";
								break;
							//<= 75%
							case 5:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE <= 75 ";
								break;
							//< 100%
							case 6:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE < 100 ";
								break;
							//= 100%
							case 7:
								filtersClause += "AND REL.REQUIREMENT_COUNT > 0 AND REL.PERCENT_COMPLETE = 100 ";
								break;
						}
					}
					else
					{
						//Now we need to convert the entity property name to the database column name and get the data-type
						try
						{
							string mappedColumn = GetPropertyMapping(typeof(ReleaseView), filterProperty);
							if (!String.IsNullOrEmpty(mappedColumn))
							{
								filterColumn = mappedColumn;
							}
							EntityProperty entityPropertyInfo = GetPropertyInfo(typeof(ReleaseView), filterProperty);
							if (entityPropertyInfo != null)
							{
								filterPropertyInfo = entityPropertyInfo;
							}
						}
						catch (IndexOutOfRangeException) { }

						//If we are filtering by ReleaseId, check to see if we're actually passed a summary Release
						//If so, we need to get the list of child iterations and add those as a list of release/iteration ids
						if (filterProperty == "ReleaseId")
						{
							if (filterValue is Int32)
							{
								string releaseIdList = GetSelfAndIterationList(projectId, (int)filterValue);
								MultiValueFilter mvf;
								if (String.IsNullOrEmpty(releaseIdList))
								{
									mvf = new MultiValueFilter();
									mvf.IsNone = true;
									filterValue = mvf;
								}
								else
								{
									if (MultiValueFilter.TryParse(releaseIdList, out mvf))
									{
										filterValue = mvf;
									}
								}
							}
						}

						//Add the generic filters
						filtersClause += CreateFilterClauseGeneric(projectId, projectTemplateId, filterProperty, filterValue, filterColumn, "REL", filterPropertyInfo, Artifact.ArtifactTypeEnum.Release, utcOffset);
					}
				}
			}
			return filtersClause;
		}

		/// <summary>Counts all the releases in the project for the current user viewing</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include Deleted Releases</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The total number of releases</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int userId, int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Count()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int artifactCount = 0;

				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = this.CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				//Call the stored procedure to get the count
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactCount = context.Release_Count(userId, projectId, filtersClause, includeDeleted).FirstOrDefault().Value;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of releases with their associated test execution summary status</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release we're interested in (optional)</param>
		/// <returns>Release list</returns>
		/// <remarks>If we are passed a release id then we retrieve all the iterations for the release. Otherwise we return all the releases for the project (without iterations)</remarks>
		public List<ReleaseView> RetrieveTestSummary(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveTestSummary";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ReleaseView> releaseIterations;
			try
			{
				//Get the list of releases and iterations
				if (releaseId.HasValue)
				{
					releaseIterations = this.RetrieveSelfAndIterations(projectId, releaseId.Value, true, false);
				}
				else
				{
					releaseIterations = this.RetrieveByProjectId(projectId, true, false);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseIterations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Checks to see if the version number in question is already being used in the specified project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="versionNumber">The version number</param>
		/// <returns>True if in use</returns>
		public bool IsVersionNumberInUse(int projectId, string versionNumber)
		{
			bool exists;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var query = from r in context.Releases
							where r.VersionNumber == versionNumber && r.ProjectId == projectId
							select r;

				exists = query.Count() > 0;
			}
			return exists;
		}

		/// <summary>
		/// Retrieves the parent release of the current release/iteration (if there is one)
		/// </summary>
		/// <param name="releaseOrIterationId">The id of the release or iteration</param>
		/// <returns>The id of the parent release (or null if there isn't one)</returns>
		public int? GetParentReleaseId(int releaseOrIterationId)
		{
			const string METHOD_NAME = "GetParentReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? parentReleaseId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					parentReleaseId = context.Release_GetParentReleaseId(releaseOrIterationId).FirstOrDefault();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return parentReleaseId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Returns a comma-separated list of ids for the passed in release and its child rollup items</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose children we want to retrieve</param>
		/// <param name="includeMajorBranches">Whether or not to include child major branches. Default:FALSE</param>
		/// <returns>List of comma-separated releases</returns>
		/// <remarks>Recoded to use a simple stored procedure for speed</remarks>
		public string GetSelfAndChildRollupChildrenList(int projectId, int releaseId, bool includeMajorBranches = false)
		{
			const string METHOD_NAME = "GetSelfAndChildRollupChildrenList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the release and its children
				List<int> releaseIds = GetSelfAndChildRollupChildren(projectId, releaseId, includeMajorBranches);
				string releaseIdList = "";
				foreach (int releaseOrIterationId in releaseIds)
				{
					releaseIdList += releaseOrIterationId.ToString() + ",";
				}

				releaseIdList = releaseIdList.Trim(',');
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseIdList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a list of ids for the passed in release and its children</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose children we want to retrieve</param>
		/// <param name="includeMajorBranches">Whether or not to include child major branches. Default:FALSE</param>
		/// <returns>List of comma-separated releases</returns>
		/// <remarks>Recoded to use a simple stored procedure for speed</remarks>
		public List<int> GetSelfAndChildRollupChildren(int projectId, int releaseId, bool includeMajorBranches = false)
		{
			const string METHOD_NAME = "GetSelfAndChildRollupChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the release and its iterations
				List<int> releaseIds;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					releaseIds = context.Release_GetSelfAndChildren(projectId, releaseId, includeMajorBranches).OfType<int>().ToList();
				}

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Found " + releaseIds.Count + " releases/iterations");
				return releaseIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a comma-separated list of ids for the passed in release and its child iterations</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose child iterations we want to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include Deleted items in the dataset. Default:FALSE</param>
		/// <returns>List of comma-separated releases</returns>
		/// <remarks>Recoded to use a simple stored procedure for speed</remarks>
		public string GetSelfAndIterationList(int projectId, int releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetSelfAndIterationList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the release and its iterations
				List<int> releaseIds = GetSelfAndIterations(projectId, releaseId, includeDeleted);
				string releaseIdList = "";
				foreach (int releaseOrIterationId in releaseIds)
				{
					releaseIdList += releaseOrIterationId.ToString() + ",";
				}

				releaseIdList = releaseIdList.Trim(',');
				return releaseIdList;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the release ids in a project (inc. releases, sprints, phases, etc.)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="includeDeleted">Should we include deleted</param>
		/// <returns>The list of IDs</returns>
		public List<int> GetAllReleaseIdsInProject(int projectId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetAllReleaseIdsInProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the release and its iterations
				List<int> releaseIds;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Releases
								where r.ProjectId == projectId && (!r.IsDeleted || includeDeleted)
								orderby r.IndentLevel descending, r.ReleaseId
								select r.ReleaseId;

					releaseIds = query.ToList();
				}

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Found " + releaseIds.Count + " release ids");
				return releaseIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a list of ids for the passed in release and its child iterations</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose child iterations we want to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include Deleted items in the dataset. Default:FALSE</param>
		/// <returns>List of comma-separated releases</returns>
		/// <remarks>Recoded to use a simple stored procedure for speed</remarks>
		public List<int> GetSelfAndIterations(int projectId, int releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetSelfAndIterations";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the release and its iterations
				List<int> releaseIds;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					releaseIds = context.Release_GetSelfAndIterations(projectId, releaseId, includeDeleted).OfType<int>().ToList();
				}

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Found " + releaseIds.Count + " releases/iterations");
				return releaseIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the passed in release and any child iterations/phases (not child releases)</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose child iterations/phases we want to retrieve</param>
		/// <param name="activeOnly">Do we only want active iterations?</param>
		/// <param name="includeDeleted">Should we include deleted rows</param>
		/// <returns>Release/Iteration list</returns>
		public List<ReleaseView> RetrieveSelfAndIterations(int projectId, int releaseId, bool activeOnly, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveSelfAndIterations";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releaseIterations;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to get the indent level of the passed in release
					var query1 = from r in context.Releases
								 where
									r.ProjectId == projectId &&
									r.ReleaseId == releaseId &&
									(!r.IsDeleted || includeDeleted)
								 select r;

					Release release = query1.FirstOrDefault();
					if (release == null)
					{
						throw new ArtifactNotExistsException("The release passed in does not exist");
					}
					string indentLevel = release.IndentLevel;

					//Store the length of the passed in indent level
					int length = indentLevel.Length;

					//Create query for retrieving the release and its iterations/phases
					var query2 = from r in context.ReleasesView
								 where r.ProjectId == projectId &&
									 EntityFunctions.Left(r.IndentLevel, length) == indentLevel &&
									 (!r.IsDeleted || includeDeleted) &&
									 (r.ReleaseId == releaseId ||
										 ((r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase)
											 && (!activeOnly || (r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed))
											 && r.IndentLevel.Length == (length + 3)))
								 orderby r.IndentLevel, r.ReleaseId
								 select r;

					releaseIterations = query2.ToList();
				}

				//Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, String.Format("Found {0} releases/iterations", releaseIterations.Count));
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseIterations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the passed in release and any child releases/iterations/phases</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose children we want to retrieve</param>
		/// <param name="activeOnly">Do we only want active child items?</param>
		/// <param name="includeDeleted">Should we include deleted rows</param>
		/// <param name="includeMajorBranches">Should we include the child major branches</param>
		/// <returns>Release/Iteration/Phase list</returns>
		public List<ReleaseView> RetrieveSelfAndChildren(int projectId, int releaseId, bool activeOnly, bool includeMajorBranches, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveSelfAndChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releaseIterations;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to get the indent level of the passed in release
					var query1 = from r in context.Releases
								 where
									r.ProjectId == projectId &&
									r.ReleaseId == releaseId &&
									(!r.IsDeleted || includeDeleted)
								 select r;

					Release release = query1.FirstOrDefault();
					if (release == null)
					{
						throw new ArtifactNotExistsException("The release passed in does not exist");
					}
					string indentLevel = release.IndentLevel;

					//Store the length of the passed in indent level
					int length = indentLevel.Length;

					//Create query for retrieving the release and its iterations/phases
					var query2 = from r in context.ReleasesView
								 where r.ProjectId == projectId &&
									 EntityFunctions.Left(r.IndentLevel, length) == indentLevel &&
									 (!r.IsDeleted || includeDeleted) &&
									 (r.ReleaseId == releaseId ||
										 (!activeOnly || (r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed)))
								 orderby r.IndentLevel, r.ReleaseId
								 select r;

					releaseIterations = query2.ToList();

					//See if we need to 'prune' the child major branches
					if (!includeMajorBranches)
					{
						//Loop through the list
						List<ReleaseView> prunedList = new List<ReleaseView>();
						string lastMajorIndent = "";
						for (int i = 0; i < releaseIterations.Count; i++)
						{
							ReleaseView releaseOrIteration = releaseIterations[i];
							if (releaseOrIteration.ReleaseId == releaseId)
							{
								//Add self
								prunedList.Add(releaseOrIteration);
							}
							else if (String.IsNullOrEmpty(lastMajorIndent) || releaseOrIteration.IndentLevel.SafeSubstring(0, lastMajorIndent.Length) != lastMajorIndent)
							{
								if (releaseOrIteration.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease)
								{
									//We don't add any child of this either
									lastMajorIndent = releaseOrIteration.IndentLevel;
								}
								else
								{
									//Add item
									prunedList.Add(releaseOrIteration);
								}
							}
						}
						releaseIterations = prunedList;
					}
				}

				//Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, String.Format("Found {0} releases/iterations", releaseIterations.Count));
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseIterations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves any child iterations (not child releases/phases) of the specified release</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release whose child iterations we want to retrieve</param>
		/// <param name="activeOnly">Do we only want active iterations?</param>
		/// <param name="includeDeleted">Should we include deleted rows</param>
		/// <returns>Iteration list</returns>
		public List<ReleaseView> RetrieveIterations(int projectId, int releaseId, bool activeOnly, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveIterations";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<ReleaseView> iterations;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to get the indent level of the passed in release
					var query1 = from r in context.Releases
								 where
									r.ProjectId == projectId &&
									r.ReleaseId == releaseId &&
									(!r.IsDeleted || includeDeleted)
								 select r;

					Release release = query1.FirstOrDefault();
					if (release == null)
					{
						throw new ArtifactNotExistsException("The release passed in does not exist");
					}
					string indentLevel = release.IndentLevel;

					//Store the length of the passed in indent level
					int length = indentLevel.Length;

					//Create query for retrieving the iterations
					var query2 = from r in context.ReleasesView
								 where r.ProjectId == projectId &&
									 EntityFunctions.Left(r.IndentLevel, length) == indentLevel &&
									 (!r.IsDeleted || includeDeleted) &&
									 r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration &&
									 r.IndentLevel.Length == (length + 3) &&
									 (!activeOnly || (r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed))
								 orderby r.IndentLevel, r.ReleaseId
								 select r;

					iterations = query2.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return iterations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the child release of the release who's indent level is passed in</summary>
		/// <param name="indentLevel">The indent-level of the release who's children are to be returned</param>
		/// <param name="recursive">Flag to determine whether the search includes sub-children releases</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include Deleted Releases</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrieveChildren(int userId, int projectId, string indentLevel, bool recursive, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;

				//Store the length of the passed in indent level
				int length = indentLevel.Length;

				//Create custom SQL WHERE clause for retrieving the releases and execute
				if (recursive)
				{
					releases = this.Retrieve(userId, projectId,
						"SUBSTRING(INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "' " +
						"AND LEN(INDENT_LEVEL) >= " + (indentLevel.Length + 3) + " " +
						"ORDER BY INDENT_LEVEL",
						includeDeleted
						);
				}
				else
				{
					releases = this.Retrieve(userId, projectId, "SUBSTRING(INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "' AND LEN(INDENT_LEVEL) = " + (indentLevel.Length + 3) + " ORDER BY INDENT_LEVEL", includeDeleted);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the peer release, immediate children and the parent summary release to the passed in release</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="indentLevel">The indent-level of the release whose peers and parent we want to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include Deleted Releases</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrievePeersChildrenAndParent(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrievePeersChildrenAndParent";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;
				//Restore the parent release for this release and all peer releases
				int length = indentLevel.Length;

				string parentIndentLevel = indentLevel.Substring(0, length - 3);
				releases = this.Retrieve(userId, projectId, "SUBSTRING(REL.INDENT_LEVEL, 1, " + (length - 3) + ") = '" + parentIndentLevel + "' " +
					"AND ((LEN(REL.INDENT_LEVEL) = " + (length - 3) + ") OR (LEN(REL.INDENT_LEVEL) = " + length + ")) " +
					"OR (LEN(REL.INDENT_LEVEL) = " + (length + 3) + " AND SUBSTRING(REL.INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "') " +
					"AND REL.PROJECT_ID = " + projectId.ToString() + " " +
					"ORDER BY REL.INDENT_LEVEL",
					includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the parent release of the release who's indent level is passed in</summary>
		/// <param name="indentLevel">The indent-level of the release who's parent is to be returned</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="includeDeleted">Whether or not to pull the parent if it's deleted or not. Default:FALSE</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Release entity</returns>
		private ReleaseView RetrieveParent(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveParent";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the parent requirement by removing the last element of the indent level string
				string parentIndentLevel = indentLevel.Substring(0, indentLevel.Length - 3);

				//Create custom SQL WHERE clause for retrieving the content item and execute
				ReleaseView release = this.Retrieve(userId, projectId, "REL.INDENT_LEVEL = '" + parentIndentLevel + "'", includeDeleted).FirstOrDefault();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return release;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a set of requirement from the system via a custom SQL filter and/or sort and where the number of rows is restricted</summary>
		/// <param name="customFilterSort">A custom SQL WHERE and/or ORDER BY clause</param>
		/// <param name="numRows">The number of rows to return</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not the dataset should contain deleted items or not. Default:FALSE</param>
		/// <param name="onlyShowVisible">Should we only show visible items (based on user navigation data) Default:FALSE</param>
		/// <returns>Release list</returns>
		protected List<ReleaseView> Retrieve(int userId, int? projectId, string customFilterSort, int numRows, bool includeDeleted = false, bool onlyShowVisible = false)
		{
			const string METHOD_NAME = "Retrieve()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored proc to get the list of releases
					releases = context.Release_RetrieveCustom(userId, projectId, customFilterSort, numRows, includeDeleted, onlyShowVisible).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ReleaseView RetrieveRelease(int artifactId, int projectId)
		{
			const string METHOD_NAME = "RetrieveRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set up the data set that is returned
			ReleaseView requirements;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReleasesView
								where
									r.ProjectId == projectId
									&& !r.IsDeleted && r.ReleaseId == artifactId
								select r;

					requirements = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (requirements);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a set of releases from the system via a custom SQL filter and/or sort</summary>
		/// <param name="customFilterSort">A custom SQL WHERE and/or ORDER BY clause</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items.</param>
		/// <param name="onlyShowVisible">Should we only show visible items</param>
		/// <returns>A release list</returns>
		internal List<ReleaseView> Retrieve(int userId, int? projectId, string customFilterSort, bool includeDeleted = false, bool onlyShowVisible = false)
		{
			const string METHOD_NAME = "Retrieve()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored proc to get the list of releases
					releases = context.Release_RetrieveCustom(userId, projectId, customFilterSort, null, includeDeleted, onlyShowVisible).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific release record by its ID</summary>
		/// <param name="releaseId">The ID of the release to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="includeDeleted">Whether or not to include deleted items in the return set. Default:FALSE</param>
		/// <returns>A release entity</returns>
		public ReleaseView RetrieveById(int userId, int? projectId, int releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the release item and execute
				List<ReleaseView> releases = this.Retrieve(userId, projectId, "REL.RELEASE_ID = " + releaseId.ToString(), includeDeleted);

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (releases.Count == 0)
				{
					throw new ArtifactNotExistsException("Release " + releaseId + " doesn't exist in the system.");
				}

				//Return the data
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases.FirstOrDefault();
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

		/// <summary>Retrieves a specific release view record by its ID</summary>
		/// <param name="releaseId">The ID of the release to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items in the return set. Default:FALSE</param>
		/// <returns>A release view</returns>
		public ReleaseView RetrieveById2(int? projectId, int releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReleaseView release;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReleasesView
								where r.ReleaseId == releaseId
								select r;

					//See if we need to filter by project
					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId);
					}

					//Should we include deleted items
					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}

					release = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception
				if (release == null)
				{
					throw new ArtifactNotExistsException("Release " + releaseId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the view
				return release;
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

		/// <summary>Retrieves a specific release entity record by its ID</summary>
		/// <param name="releaseId">The ID of the release to retrieve</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items in the return set. Default:FALSE</param>
		/// <returns>A release entity</returns>
		public Release RetrieveById3(int? projectId, int releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById3";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Release release;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Releases
								where r.ReleaseId == releaseId
								select r;

					//See if we need to filter by project
					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId);
					}

					//Should we include deleted items
					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}

					release = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception
				if (release == null)
				{
					throw new ArtifactNotExistsException("Release " + releaseId + " doesn't exist in the system.");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the view
				return release;
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

		/// <summary>
		/// Retrieves a list of all the releases for a specific number of indent levels
		/// </summary>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="projectId">The of the project</param>
		/// <param name="numberOfLevels">The number of levels (null = all)</param>
		/// <returns>The list of releases</returns>
		public List<ReleaseView> Release_RetrieveForMindMap(int projectId, int? numberOfLevels, out int count, int startRow = 1, int numberOfRows = 15)
		{
			const string METHOD_NAME = "Release_RetrieveForMindMap";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from r in context.ReleasesView
								where !r.IsDeleted && r.ProjectId == projectId
								select r;

					if (numberOfLevels.HasValue)
					{
						query = query.Where(r => r.IndentLevel.Length <= (numberOfLevels.Value * 3));
					}

					//Add the sort
					query = query.OrderBy(r => r.IndentLevel).ThenBy(r => r.ReleaseId);

					//Get the count
					count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					releases = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of releases and iterations/phases/sprints associated with the program / project group
		/// </summary>
		/// <param name="projectGroupId">The ID of the project group</param>
		/// <param name="activeOnly">Do we only want active releases</param>
		/// <returns>The list of releases and iterations/sprints</returns>
		/// <remarks>
		/// 1) Does not use the hieararchy apart from the ordering
		/// 2) Active Releases are those that are in these statuses: (Planned, In Progress & Completed)
		/// </remarks>
		public List<Release> Release_RetrieveByProjectGroup(int projectGroupId, bool activeOnly = true)
		{
			const string METHOD_NAME = "Release_RetrieveByProjectGroup()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Release> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Releases
								where r.Project.ProjectGroupId == projectGroupId && !r.IsDeleted && r.Project.IsActive
								select r;

					//See if we want active only
					if (activeOnly)
					{
						query = query.Where(r => r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned
						|| r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress
						|| r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed);
					}

					//Sort by project, then indent position, then release id (as fallback)
					query = query
						.OrderBy(r => r.ProjectId)
						.ThenBy(r => r.IndentLevel)
						.ThenBy(r => r.ReleaseId);

					releases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
		}


		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases = this.Retrieve(User.UserInternal, projectId, "REL.IS_DELETED = 1", true); //Must include deleted items, to avoid collision.

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return null;
			}
		}

		/// <summary>Retrieves a single release in the system that has a certain indent level</summary>
		/// <param name="indentLevel">The indent-level of the release to be returned</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="includeDeleted">Whether or not to include deleted items or not. Default:FALSE</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrieveByIndentLevel(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByIndentLevel";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the requirement item and execute
				List<ReleaseView> releases = this.Retrieve(userId, projectId, "REL.INDENT_LEVEL = '" + indentLevel + "'", includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of releases already mapped against a test case</summary>
		/// <param name="testCaseId">The ID of the test case we want the list for</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing requirements as</param>
		/// <param name="includeDeleted">Whether or not to include Deleted Releases.</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrieveMappedByTestCaseId(int userId, int? projectId, int testCaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveMappedByTestCaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the releases and execute
				List<ReleaseView> releases = this.Retrieve(userId, projectId, "REL.RELEASE_ID IN (SELECT RELEASE_ID FROM TST_RELEASE_TEST_CASE WHERE TEST_CASE_ID = " + testCaseId.ToString() + ") " +
					"ORDER BY REL.INDENT_LEVEL ASC",
					includeDeleted
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Calculates the number of working days between two dates (inclusive of both dates)</summary>
		/// <param name="startDate">The starting date</param>
		/// <param name="endDate">The ending date</param>
		/// <param name="workingDaysPerWeek">How many working days are in a week</param>
		/// <returns>The number of days</returns>
		/// <remarks>The system assumes that Monday is always worked, with the other days varying depending on the number of working days parameter</remarks>
		public static long WorkingDays(DateTime startDate, DateTime endDate, int workingDaysPerWeek)
		{
			TimeSpan span = endDate.Subtract(startDate);
			long wholeWeeks = ((long)Math.Round(Math.Floor(span.TotalDays))) / 7;
			DateTime dateCount = startDate.AddDays(wholeWeeks * 7);
			int endDays = 0;
			while (dateCount.Date <= endDate.Date)
			{
				switch (dateCount.DayOfWeek)
				{
					case DayOfWeek.Monday:
						if (workingDaysPerWeek >= 1)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Tuesday:
						if (workingDaysPerWeek >= 2)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Wednesday:
						if (workingDaysPerWeek >= 3)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Thursday:
						if (workingDaysPerWeek >= 4)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Friday:
						if (workingDaysPerWeek >= 5)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Saturday:
						if (workingDaysPerWeek >= 6)
						{
							endDays++;
						}
						break;
					case DayOfWeek.Sunday:
						if (workingDaysPerWeek >= 7)
						{
							endDays++;
						}
						break;
					default:
						endDays++;
						break;
				}
				dateCount = dateCount.AddDays(1);
			}
			return wholeWeeks * workingDaysPerWeek + endDays;
		}

		/// <summary>Calculates the planned effort available in a release when provided with the dates, resource count and number of non-working days to be removed from the total.</summary>
		/// <param name="startDate">The starting date</param>
		/// <param name="endDate">The ending date</param>
		/// <param name="resourceCount">The number of notional resources available</param>
		/// <param name="standardHoursNonWorking">The number of non-working person hours per month to subtract (for each resource)</param>
		/// <param name="personHoursNonWorking">The number of additional non-person hours to substract (regardless of # resources)</param>
		/// <param name="workingHoursPerDay">The number of working hours per day</param>
		/// <param name="workingDaysPerWeek">The number of working days per week</param>
		/// <returns>The level of effort that is planned for this release (in minutes)</returns>
		protected internal static int CalculatePlannedEffort(DateTime startDate, DateTime endDate, decimal resourceCount, int workingDaysPerWeek, int workingHoursPerDay, int standardHoursNonWorking, int personHoursNonWorking)
		{
			//First we need to get the number of working days between the two dates
			int workingDays = (int)WorkingDays(startDate, endDate, workingDaysPerWeek);

			//Now get the total number of working hours
			int workingHours = workingHoursPerDay * workingDays;

			//Now we need to appoportion any 'non-working hours per month' for the date-range
			//For simplicity we use 31 days regardless of month. We always round down
			int releaseNonWorkingHours = standardHoursNonWorking * workingDays;
			releaseNonWorkingHours /= 31;

			//Now remove any non-working hours (holidays, other adjustments, etc.) for that month
			workingHours -= releaseNonWorkingHours;

			//Now get the work/effort in hours for the # resources
			decimal effortHours = (resourceCount * workingHours) - personHoursNonWorking;

			//Now calculate the effort in minutes
			int effort = (int)(effortHours * 60M);

			return effort;
		}

		/// <summary>Outdents a release in the system with the specified ID, including all subordinates</summary>
		/// <param name="releaseId">The ID of the release to be outdented</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <remarks>You cannot indent or outdent an iteration</remarks>
		public void Outdent(int userId, int projectId, int releaseId)
		{
			const string METHOD_NAME = "Outdent";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to see if it has any children
				//or if it's an iteration
				ReleaseView markedRelease = this.RetrieveById(userId, projectId, releaseId, true);
				string indentLevel = markedRelease.IndentLevel;

				//First, need to make sure it is not top-level already
				if (indentLevel.Length <= 3)
				{
					//We can't indent if it is top-level
					return;
				}

				//Get list of all child releases for use later on. We don't rely on summary flag
				//because there may be deleted children [IN:002251]
				//Get all child releases (not just immediate)
				List<ReleaseView> childReleases = this.RetrieveChildren(userId, projectId, indentLevel, true, true);

				//We need to modify the indent level of the selected item to be one level higher in the tree
				//We do this by getting the indent level of the parent and incrementing that
				ReleaseView parentRelease = this.RetrieveParent(userId, projectId, indentLevel, true);
				string parentIndentLevel = parentRelease.IndentLevel;
				int parentReleaseId = parentRelease.ReleaseId;
				string incrementedParentIndentLevel = HierarchicalList.IncrementIndentLevel(parentIndentLevel);
				markedRelease.IndentLevel = incrementedParentIndentLevel;

				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Need to see how many children the parent has, and if it only has one
						//then outdenting this item will result in it no longer being a summary item
						if (this.RetrieveChildren(userId, projectId, parentIndentLevel, false).Count <= 1)
						{
							parentRelease.IsSummary = false;
							parentRelease.IsExpanded = false;
							this.UpdatePositionalData(userId, new List<ReleaseView> { parentRelease });
						}

						//Before making the change to the selected item, need to move subsequent items first
						this.ReorderReleasesBeforeInsert(userId, projectId, incrementedParentIndentLevel);

						//Now commit the change
						this.UpdatePositionalData(userId, new List<ReleaseView> { markedRelease });

						//Now we need to update all the child elements
						if (childReleases.Count > 0)
						{
							foreach (ReleaseView childRow in childReleases)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRow.IndentLevel;
								string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, indentLevel, incrementedParentIndentLevel);
								childRow.IndentLevel = newchildIndentLevel;
							}
							//Commit the changes
							this.UpdatePositionalData(userId, childReleases);
						}

						//Now we need to reorder all subsequent items of the item itself
						ReorderReleasesAfterDelete(userId, projectId, indentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Update all the task/test/requirements information
				RefreshProgressEffortTestStatus(projectId, releaseId, false);
				RefreshProgressEffortTestStatus(projectId, parentReleaseId, true);
			}
			catch (IterationSummaryException)
			{
				//We don't log these exceptions
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Indents a release in the system with the specified ID, including all subordinates</summary>
		/// <param name="releaseId">The ID of the release to be indented</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <remarks>You cannot indent or outdent an iteration</remarks>
		public void Indent(int userId, int projectId, int releaseId)
		{
			const string METHOD_NAME = "Indent";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to see if it has any children
				//or if it's an iteration
				ReleaseView markedRelease = this.RetrieveById(userId, projectId, releaseId, true);
				string indentLevel = markedRelease.IndentLevel;

				//We need to modify the indent level of the selected item to be one level lower in the tree
				//We do this by getting the indent level of the last child of our predessor and incrementing that.
				//First we need to decrement our indent level to get our predecessors
				string predecessorIndentLevel = this.GetPreviousPeer(indentLevel, projectId, false);

				//First Need to check if we have a predecessor or not
				if (string.IsNullOrWhiteSpace(predecessorIndentLevel))
				{
					//We can't indent if we have no predecessor
					return;
				}

				//Get list of all child releases for use later on. We don't rely on summary flag
				//because there may be deleted children [IN:002251]
				//Get list of all child releases for use later on
				List<ReleaseView> childReleases = this.RetrieveChildren(userId, projectId, indentLevel, true, true);

				//Next we need to make the predecessor a summary item
				//However if the predecessor is an iteration (vs. a release) throw an exception
				//Since iterations are not allowed to include children of their own
				ReleaseView predecessorRelease = this.RetrieveByIndentLevel(userId, projectId, predecessorIndentLevel, true).FirstOrDefault();
				if (predecessorRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || predecessorRelease.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase)
				{
					throw new IterationSummaryException("Iterations/phases cannot have child releases or iterations.");
				}
				predecessorRelease.IsSummary = true;
				predecessorRelease.IsExpanded = true;
				int predecessorReleaseId = predecessorRelease.ReleaseId;

				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Make the initial position change update
						this.UpdatePositionalData(userId, new List<ReleaseView>() { predecessorRelease });

						//Next get his last immediate child and increment
						string newIndentLevel;
						List<ReleaseView> predecessorChildReleases = this.Retrieve(userId, projectId, "SUBSTRING(INDENT_LEVEL, 1, " + predecessorIndentLevel.Length.ToString() + ") = '" + predecessorIndentLevel + "' AND LEN(INDENT_LEVEL) = " + (predecessorIndentLevel.Length + 3) + " ORDER BY INDENT_LEVEL DESC", 1, true);
						if (predecessorChildReleases.Count == 0)
						{
							//Create a new child under the predecessor
							newIndentLevel = predecessorIndentLevel + "AAA";
						}
						else
						{
							string predecessorChildIndentLevel = predecessorChildReleases[0].IndentLevel;

							//Increment this to get our new level
							newIndentLevel = HierarchicalList.IncrementIndentLevel(predecessorChildIndentLevel);
						}
						markedRelease.IndentLevel = newIndentLevel;

						//Now commit the change
						this.UpdatePositionalData(userId, new List<ReleaseView>() { markedRelease });

						//Now we need to update all the child elements
						if (childReleases.Count > 0)
						{
							foreach (ReleaseView childRow in childReleases)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRow.IndentLevel;
								string newChildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, indentLevel, newIndentLevel);
								childRow.IndentLevel = newChildIndentLevel;
							}
							//Commit the changes
							this.UpdatePositionalData(userId, childReleases);
						}

						//Now we need to reorder all subsequent items from where it used to be
						//We don't need to reorder after its new position as it's always inserted at the end
						ReorderReleasesAfterDelete(userId, projectId, indentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Update all the task/test/requirements information
				RefreshProgressEffortTestStatus(projectId, releaseId, true);
				RefreshProgressEffortTestStatus(projectId, predecessorReleaseId, false);
			}
			catch (IterationSummaryException)
			{
				//We don't log these exceptions
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Returns the previous peer for the given indent level. If there is none, it will return an empty string. Used for indenting</summary>
		/// <param name="indentLevel">The indent level to find the peer for.</param>
		/// <param name="projectId">The project id to search on.</param>
		/// <param name="includeDeleted">Whether or not to include deleted items when finding the previous peer. Default:FALSE</param>
		/// <returns>The indent level ofthe previous peer. Null if there is none.</returns>
		protected string GetPreviousPeer(string indentLevel, int projectId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetPreviousPeer()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string retString = null;

				//First make sure it HAS a peer.
				if (indentLevel.SafeSubstring(indentLevel.Length - 3) != "AAA")
				{
					//It's not an 'AAA', so it MAY have one before it that is not yet deleted. Get the 
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						retString = context.Release_GetPreviousPeer(projectId, indentLevel, includeDeleted).FirstOrDefault();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retString;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns the next available indent level (for new inserts)</summary>
		/// <param name="ignoreLastInserted">Do we want the next item at the root level, or the next one after the last one inserted</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user that is viewing the hierarchy</param>
		/// <returns>The indent level of the next available item Null if there is none.</returns>
		protected string GetNextAvailableIndentLevel(int userId, int projectId, bool ignoreLastInserted)
		{
			const string METHOD_NAME = "GetNextAvailableIndentLevel()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string retString = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					retString = context.Release_GetNextAvailableIndentLevel(projectId, userId, ignoreLastInserted).FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retString;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Expands a summary release in the system with the specified ID. Note that expands do not cascade to subordinate summary items</summary>
		/// <param name="releaseId">The ID of the release to be expanded</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Expand(int userId, int projectId, int releaseId)
		{
			const string METHOD_NAME = "Expand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to make sure it is a collapsed summary one
				ReleaseView markedRelease = this.RetrieveById(userId, projectId, releaseId, true);
				if (markedRelease.IsSummary && !markedRelease.IsExpanded)
				{
					//First we need to update its expanded flag
					markedRelease.IsExpanded = true;
					this.UpdatePositionalData(userId, new List<ReleaseView>() { markedRelease });

					//Now get its immediate child items and make them visible
					//Call the stored procedure that makes the child items visible
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.Release_Expand(userId, projectId, markedRelease.IndentLevel);
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
		}

		/// <summary>Collapses a summary release in the system with the specified ID. Note that collapses cascade to affect all subordinate summary items</summary>
		/// <param name="releaseId">The ID of the release to be collapsed</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Collapse(int userId, int projectId, int releaseId)
		{
			const string METHOD_NAME = "Collapse";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME + " " + releaseId.ToString());

			try
			{
				//Call the stored procedure that collapses the summary release and its children
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Release_Collapse(userId, projectId, releaseId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Expands the list of releases to the specified number of places</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="level">The number of places to indent to. Pass null to expand all levels</param>
		public void ExpandToLevel(int userId, int projectId, int? level)
		{
			const string METHOD_NAME = "ExpandToLevel";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have been passed in null, change to 999 to represent all levels
			if (!level.HasValue)
			{
				level = 999;
			}

			try
			{
				//Call the stored procedure that handles the expansion
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Release_ExpandToLevel(userId, projectId, level);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes the user navigation data for all releases for a given user</summary>
		/// <param name="userId">The user we want to delete the data for</param>
		/// <remarks>This is primarily used by the unit tests to clean up the system</remarks>
		public void DeleteUserNavigationData(int userId)
		{
			const string METHOD_NAME = "DeleteUserNavigationData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Actually perform the delete
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Release_DeleteNavigationData(userId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Exports a release (and its children) from one project to another</summary>
		/// <param name="userId">The user exporting the release</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="sourceReleaseId">The id of the release being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <returns>The id of the release in the new project</returns>
		public int Export(int userId, int sourceProjectId, int sourceReleaseId, int destProjectId)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//First we need to retrieve the release to see if it has any children
				ReleaseView sourceRelease = this.RetrieveById2(sourceProjectId, sourceReleaseId);
				string sourceIndentLevel = sourceRelease.IndentLevel;

				//Get dataset of all child releases for use later on
				List<ReleaseView> childReleases = null;
				if (sourceRelease.IsSummary)
				{
					//Get all child releases (not just immediate)
					childReleases = this.RetrieveChildren(userId, sourceProjectId, sourceIndentLevel, true);
				}

				//Now we need to make sure that the version number is not already in use in the destination project
				string newVersionNumber = sourceRelease.VersionNumber;
				if (IsVersionNumberInUse(destProjectId, newVersionNumber))
				{
					//We need to generate a new version number
					newVersionNumber = null;
				}

				//Insert a new release with the data copied from the existing one
				int exportedReleaseId = this.Insert(
					userId,
					destProjectId,
					sourceRelease.CreatorId,
					sourceRelease.Name,
					sourceRelease.Description,
					newVersionNumber,
					null,
					(Release.ReleaseStatusEnum)sourceRelease.ReleaseStatusId,
					(Release.ReleaseTypeEnum)sourceRelease.ReleaseTypeId,
					sourceRelease.StartDate,
					sourceRelease.EndDate,
					sourceRelease.ResourceCount,
					sourceRelease.DaysNonWorking,
					sourceRelease.OwnerId,
					true,
					false
					);

				//Log history item..
				new HistoryManager().LogImport(sourceProjectId, destProjectId, sourceReleaseId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, exportedReleaseId, DateTime.UtcNow);

				//We copy custom properties if the templates are the same
				if (templatesSame)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, sourceProjectId, sourceRelease.ReleaseId, destProjectId, exportedReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release, userId);
				}

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(sourceProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, sourceReleaseId, destProjectId, exportedReleaseId);

				//We need to again re-retrieve the destination indent level to see if the reorder affected it
				ReleaseView destRelease = this.RetrieveById2(destProjectId, exportedReleaseId);
				string destIndentLevel = destRelease.IndentLevel;

				//Now we need to insert all the child elements
				if (sourceRelease.IsSummary)
				{
					//First make the copy an expanded summary item
					destRelease.IsSummary = true;
					destRelease.IsExpanded = true;
					this.UpdatePositionalData(userId, new List<ReleaseView>() { destRelease });

					foreach (ReleaseView childRow in childReleases)
					{
						//Need to replace the base of the indent level of all its children
						string childIndentLevel = childRow.IndentLevel;
						string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);

						//Now we need to make sure that the version number is not already in use in the destination project
						newVersionNumber = childRow.VersionNumber;
						if (IsVersionNumberInUse(destProjectId, newVersionNumber))
						{
							//We need to generate a new version number
							newVersionNumber = null;
						}

						//Insert a new release with the data copied from the existing one
						//We use the Insert overload that allows insertion at a specific indent level
						int newchildReleaseId = this.Insert(
							userId,
							destProjectId,
							childRow.CreatorId,
							childRow.Name,
							childRow.Description,
							newVersionNumber,
							newchildIndentLevel,
							(Release.ReleaseStatusEnum)childRow.ReleaseStatusId,
							(Release.ReleaseTypeEnum)childRow.ReleaseTypeId,
							childRow.StartDate,
							childRow.EndDate,
							childRow.ResourceCount,
							childRow.DaysNonWorking,
							childRow.OwnerId,
							false
							);

						//Log history item..
						new HistoryManager().LogImport(sourceProjectId, destProjectId, childRow.ReleaseId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, newchildReleaseId, DateTime.UtcNow);

						//We copy custom properties if the templates are the same
						if (templatesSame)
						{
							//Now we need to copy across any custom properties
							new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, sourceProjectId, childRow.ReleaseId, destProjectId, newchildReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release, userId);
						}

						//Now we need to override the summary-status to match the child (and also handle the expanded flag)
						ReleaseView newChildRelease = RetrieveById2(destProjectId, newchildReleaseId);
						newChildRelease.IsSummary = childRow.IsSummary;
						if (childRow.IsSummary)
						{
							//By default all new summary items are displayed as expanded
							newChildRelease.IsExpanded = true;
						}
						this.UpdatePositionalData(userId, new List<ReleaseView>() { newChildRelease });

						//Now we need to copy across any linked attachments
						attachmentManager.Export(sourceProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, childRow.ReleaseId, destProjectId, newchildReleaseId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the release id of the exported item
				return exportedReleaseId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a release (and its children) from one location to another</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceReleaseId">The release we want to copy</param>
		/// <param name="destReleaseId">The release we want to copy it in front of</param>
		/// <param name="projectTemplateId">The id of the projedct template</param>
		/// <remarks>Pass null for the destination release ID to copy the release to the end. This function also copies the test coverage information for the release(s) in question</remarks>
		/// <returns>The id of the copy of the release</returns>
		public int Copy(int userId, int projectId, int projectTemplateId, int sourceReleaseId, int? destReleaseId)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to see if it has any children
				ReleaseView sourceRelease = this.RetrieveById2(projectId, sourceReleaseId);
				string sourceIndentLevel = sourceRelease.IndentLevel;

				//Get dataset of all child releases for use later on
				List<ReleaseView> childReleases = null;
				if (sourceRelease.IsSummary)
				{
					//Get all child releases (not just immediate)
					childReleases = this.RetrieveChildren(userId, projectId, sourceIndentLevel, true);
				}

				//Insert a new release with the data copied from the existing one
				int copiedReleaseId = this.Insert(
					userId,
					projectId,
					sourceRelease.CreatorId,
					sourceRelease.Name + CopiedArtifactNameSuffix,
					sourceRelease.Description,
					null,
					destReleaseId,
					(Release.ReleaseStatusEnum)sourceRelease.ReleaseStatusId,
					(Release.ReleaseTypeEnum)sourceRelease.ReleaseTypeId,
					sourceRelease.StartDate,
					sourceRelease.EndDate,
					sourceRelease.ResourceCount,
					sourceRelease.DaysNonWorking,
					sourceRelease.OwnerId,
					true
					);

				//Now we need to copy across any coverage information
				CopyCoverage(userId, projectId, sourceReleaseId, copiedReleaseId);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, sourceReleaseId, copiedReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, sourceReleaseId, copiedReleaseId);

				//We need to again re-retrieve the destination indent level to see if the reorder affected it
				//We need to use the retrieve overload that specifies the user ID,
				//because otherwise when we go to save it, it will set the IsVisible flag to false (which is incorrect)
				ReleaseView destRelease = this.RetrieveById(userId, projectId, copiedReleaseId);
				string destIndentLevel = destRelease.IndentLevel;

				//Now we need to insert all the child elements
				if (sourceRelease.IsSummary)
				{
					//First make the copy an expanded summary item
					destRelease.IsSummary = true;
					destRelease.IsExpanded = true;
					this.UpdatePositionalData(userId, new List<ReleaseView>() { destRelease });

					foreach (ReleaseView childRow in childReleases)
					{
						//Need to replace the base of the indent level of all its children
						string childIndentLevel = childRow.IndentLevel;
						string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);

						//Insert a new release with the data copied from the existing one
						//We use the Insert overload that allows insertion at a specific indent level
						int newChildReleaseId = this.Insert(
							userId,
							projectId,
							childRow.CreatorId,
							childRow.Name,
							childRow.Description,
							null,
							newchildIndentLevel,
							(Release.ReleaseStatusEnum)childRow.ReleaseStatusId,
							(Release.ReleaseTypeEnum)childRow.ReleaseTypeId,
							childRow.StartDate,
							childRow.EndDate,
							childRow.ResourceCount,
							childRow.DaysNonWorking,
							childRow.OwnerId
							);

						//Now we need to override the summary-status to match the child (and also handle the expanded flag)
						ReleaseView newChildRelease = this.RetrieveById(userId, projectId, newChildReleaseId);
						newChildRelease.IsSummary = childRow.IsSummary;
						if (childRow.IsSummary)
						{
							//By default all new summary items are displayed as expanded
							newChildRelease.IsExpanded = true;
						}
						this.UpdatePositionalData(userId, new List<ReleaseView>() { newChildRelease });

						//Now we need to copy across any coverage information
						CopyCoverage(userId, projectId, childRow.ReleaseId, newChildReleaseId);

						//Now we need to copy across any custom properties
						new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, childRow.ReleaseId, newChildReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release, userId);

						//Now we need to copy across any linked attachments
						attachmentManager.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, childRow.ReleaseId, newChildReleaseId);
					}
				}

				//Rollup the test status to the parent releases
				List<int> releaseIds = new List<int>() { sourceReleaseId };
				if (destReleaseId.HasValue)
				{
					releaseIds.Add(destReleaseId.Value);
				}
				RefreshProgressEffortTestStatus(projectId, releaseIds);

				//Send a notification
				SendCreationNotification(copiedReleaseId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the release id of the copy
				return copiedReleaseId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a sorted list of values to populate the lookup for the requirements coverage filter</summary>
		/// <returns>Sorted List containing filter values</returns>
		/// <remarks>Delegates to the requirements object which has the same method</remarks>
		public Dictionary<string, string> RetrieveCoverageFiltersLookup()
		{
			Business.RequirementManager requirement = new Business.RequirementManager();
			return requirement.RetrieveCoverageFiltersLookup();
		}

		/// <summary>Moves a release (and its children) from one location to another</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceReleaseId">The release we want to move</param>
		/// <param name="destReleaseId">The release we want to move it in front of</param>
		/// <remarks>Pass null for the destination release ID to move the release to the end</remarks>
		public void Move(int userId, int projectId, int sourceReleaseId, int? destReleaseId)
		{
			const string METHOD_NAME = "Move";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string destIndentLevel = "";
				string sourceIndentLevel = "";
				int? parentReleaseToRefresh = null;

				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Check to see if we are being passed an existing release to insert before
						ReleaseView destRelease;
						ReleaseView sourceRelease;
						if (!destReleaseId.HasValue)
						{
							//We simply move the release to the end of the list in this case

							//Get the next available indent level and parent id
							destRelease = this.Retrieve(userId, projectId, "IS_VISIBLE = 1 ORDER BY REL.INDENT_LEVEL DESC", 1, true).FirstOrDefault();
							if (destRelease != null)
							{
								destIndentLevel = destRelease.IndentLevel;

								//Now increment the indent level
								destIndentLevel = HierarchicalList.IncrementIndentLevel(destIndentLevel);
							}
						}
						else
						{
							//Now we need to retrieve the destination release
							destRelease = RetrieveById(userId, projectId, destReleaseId.Value, true);
							if (destRelease == null)
							{
								throw (new System.Exception("Cannot retrieve destination release"));
							}
							destIndentLevel = destRelease.IndentLevel;

							//Make sure we're not trying to drag the release 'under itself' as this causes very strange issues
							sourceRelease = this.RetrieveById(userId, projectId, sourceReleaseId, true);
							sourceIndentLevel = sourceRelease.IndentLevel;
							if (sourceIndentLevel.Length < destIndentLevel.Length && destIndentLevel.SafeSubstring(0, sourceIndentLevel.Length) == sourceIndentLevel)
							{
								//Simply do nothing and log a warning message
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.HierarchicalItem_CannotMoveUnderItself);
								return;
							}

							//Next we need to create space in the hierarchy to prepare for the insert part of the move
							//Needed so that we don't violate the indent-level uniqueness constraint
							ReorderReleasesBeforeInsert(userId, projectId, destIndentLevel);
						}

						//Next we need to retrieve the release to see if it has any children
						sourceRelease = this.RetrieveById(userId, projectId, sourceReleaseId, true);
						sourceIndentLevel = sourceRelease.IndentLevel;

						//Need to get the parent and peers to see if moving it will make the parent non-summary
						if (sourceIndentLevel.Length > 3)
						{
							ReleaseView parentRelease = this.RetrieveParent(UserManager.UserInternal, projectId, sourceIndentLevel, true);
							if (parentRelease != null)
							{
								string parentIndentLevel = parentRelease.IndentLevel;
								int parentReleaseId = parentRelease.ReleaseId;

								//Need to see how many children the parent has, and if it only has one
								//then outdenting this item will result in it no longer being a summary item
								if (this.RetrieveChildren(UserManager.UserInternal, projectId, parentIndentLevel, false).Count <= 1)
								{
									parentRelease.IsSummary = false;
									parentRelease.IsExpanded = false;
									this.UpdatePositionalData(userId, new List<ReleaseView>() { parentRelease });
								}

								//If the current release is not a major release, then we'll need to update the test status
								//of its parent after release
								if (sourceRelease.ReleaseTypeId != (int)Release.ReleaseTypeEnum.MajorRelease)
								{
									parentReleaseToRefresh = parentRelease.ReleaseId;
								}
							}
						}

						//Get dataset of all child releases for use later on
						List<ReleaseView> childReleases = null;
						if (sourceRelease.IsSummary)
						{
							//Get all child releases (not just immediate)
							childReleases = this.RetrieveChildren(userId, projectId, sourceIndentLevel, true, true);
						}

						//Update the indent-level of the source release to the destination
						sourceRelease.IndentLevel = destIndentLevel;

						//Now actually reposition the item being moved since we've made space for it in the tree
						this.UpdatePositionalData(userId, new List<ReleaseView>() { sourceRelease });

						//Now we need to update all the child elements
						if (sourceRelease.IsSummary)
						{
							foreach (ReleaseView childRow in childReleases)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRow.IndentLevel;
								string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);
								childRow.IndentLevel = newchildIndentLevel;
							}

							//Now commit the changes
							this.UpdatePositionalData(userId, childReleases);
						}

						//Finally we need to update the indent levels of the subsequent releases to account for the delete part of the move
						this.ReorderReleasesAfterDelete(userId, projectId, sourceIndentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Rollup the test/task status to the parent releases
				if (destReleaseId.HasValue)
				{
					RefreshProgressEffortTestStatus(projectId, destReleaseId.Value, false);
				}
				if (parentReleaseToRefresh.HasValue)
				{
					RefreshProgressEffortTestStatus(projectId, parentReleaseToRefresh.Value, true);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Copies the test coverage information for a specific release</summary>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The current project</param>
		/// <param name="sourceReleaseId">The release we're copying coverage FROM</param>
		/// <param name="destReleaseId">The release we're copying coverage TO</param>
		protected void CopyCoverage(int userId, int projectId, int sourceReleaseId, int destReleaseId)
		{
			//Instantiate a Test Case business object
			TestCaseManager testCaseManager = new TestCaseManager();

			//Now we need to copy across any coverage information
			List<TestCase> sourceTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, sourceReleaseId);
			List<int> testCaseIds = new List<int>();
			foreach (TestCase testCaseRow in sourceTestCases)
			{
				//Add the row to the list
				testCaseIds.Add(testCaseRow.TestCaseId);
			}
			testCaseManager.AddToRelease(projectId, destReleaseId, testCaseIds, userId);
		}

		/// <summary>Cascades a change of release deletion status to its child releases/iterations</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project the release belongs to</param>
		/// <param name="indentLevel">The indent level of the release</param>
		/// <param name="isDeleted">The new value of the deletion flag</param>
		protected void CascadeDeleteFlagChange(int userId, int projectId, string indentLevel, bool isDeleted)
		{
			const string METHOD_NAME = "CascadeDeleteFlagChange()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of child items
				List<ReleaseView> childReleases = this.RetrieveChildren(userId, projectId, indentLevel, true, true);

				//Update the deleted flag for each of the child items
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (ReleaseView childRelease in childReleases)
					{
						//Attach to the context (converting from View to artifact)
						Release release = childRelease.ConvertTo<ReleaseView, Release>();
						context.Releases.ApplyChanges(release);
						release.StartTracking();
						release.IsDeleted = isDeleted;
						release.LastUpdateDate = DateTime.UtcNow;
						release.ConcurrencyDate = DateTime.UtcNow;

						//We don't Log history for child cascaded deletes, only for the primary delete
					}

					//Commit the changes
					context.SaveChanges(userId, false, false, null);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Marks the specified release (and any children) as deleted.</summary>
		/// <param name="userId">The userId making the deletion.</param>
		/// <param name="projectId">The projectId that the release belongs to.</param>
		/// <param name="releaseId">The release to delete.</param>
		public void MarkAsDeleted(int userId, int projectId, int releaseId)
		{
			const string METHOD_NAME = "MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to get its indent-level
				ReleaseView markedRelease = this.RetrieveById(userId, projectId, releaseId, true);

				if (markedRelease != null)
				{
					string indentLevel = markedRelease.IndentLevel;

					//Now we need to retrieve its parent and see if the operation will make it non-summary
					int? parentReleaseId = null;
					if (indentLevel.Length > 3)
					{
						ReleaseView parentRelease = this.RetrieveParent(userId, projectId, indentLevel, true);
						if (parentRelease != null)
						{
							string parentIndentLevel = parentRelease.IndentLevel;
							parentReleaseId = parentRelease.ReleaseId;

							//Need to see how many children the parent has, and if it only has one
							//then outdenting this item will result in it no longer being a summary item
							if (this.RetrieveChildren(userId, projectId, parentIndentLevel, false).Count <= 1)
							{
								parentRelease.IsSummary = false;
								parentRelease.IsExpanded = false;
								this.UpdatePositionalData(userId, new List<ReleaseView>() { parentRelease });
							}
						}
					}

					//Mark children as deleted.
					this.CascadeDeleteFlagChange(userId, projectId, markedRelease.IndentLevel, true);

					//Delete any associated builds
					new BuildManager().MarkAsDeletedForRelease(projectId, releaseId);

					//Now mark this one as deleted.
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Attach to the context (converting from View to artifact)
						Release release = markedRelease.ConvertTo<ReleaseView, Release>();
						context.Releases.ApplyChanges(release);
						release.StartTracking();
						release.IsDeleted = true;
						release.LastUpdateDate = DateTime.UtcNow;
						release.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes
						context.SaveChanges(userId, false, false, null);

					}

					//Update the requirements progress, test and task coverage.
					if (parentReleaseId.HasValue)
					{
						RefreshProgressEffortTestStatus(projectId, parentReleaseId.Value);
					}

					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes a release in the system that has the specified ID</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="releaseId">The ID of the release to be deleted</param>
		public void DeleteFromDatabase(int releaseId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to get its indent-level
				ReleaseView markedRelease = this.RetrieveById2(null, releaseId, true);

				if (markedRelease != null)
				{
					int projectId = markedRelease.ProjectId;
					string indentLevel = markedRelease.IndentLevel;
					Release.ReleaseTypeEnum releaseType = (Release.ReleaseTypeEnum)markedRelease.ReleaseTypeId;

					//Now we need to retrieve its parent and see if the operation will make it non-summary
					int? parentReleaseId = null;
					if (indentLevel.Length > 3)
					{
						ReleaseView parentRelease = this.RetrieveParent(userId, projectId, indentLevel, true);
						if (parentRelease != null)
							parentReleaseId = parentRelease.ReleaseId;
					}

					//Now we need to retrieve and handle all its children
					if (markedRelease.IsSummary)
					{
						//They are invisible, so we need to recursively delete them
						List<ReleaseView> childReleases = this.RetrieveChildren(userId, projectId, indentLevel, false, true);
						foreach (ReleaseView childRow in childReleases)
						{
							this.DeleteFromDatabase(childRow.ReleaseId, userId);
						}
					}

					//Tasks are automatically de-associated through cascades

					//Next we need to delete any attachments associated with the release
					Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
					attachmentManager.DeleteByArtifactId(releaseId, DataModel.Artifact.ArtifactTypeEnum.Release);

					//Next we need to delete any custom properties associated with the release
					new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(releaseId, DataModel.Artifact.ArtifactTypeEnum.Release);

					try
					{
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							//Begin transaction - needed to maintain integrity of hierarchy indent level
							using (TransactionScope transactionScope = new TransactionScope())
							{
								//Actually perform the delete
								context.Release_Delete(releaseId);

								//Now we need to reorder all subsequent items
								this.ReorderReleasesAfterDelete(userId, projectId, indentLevel);

								//Commit transaction - needed to maintain integrity of hierarchy indent level
								transactionScope.Complete();
							}
						}
					}
					catch (System.Exception exception)
					{
						//Rollback transaction - needed to maintain integrity of hierarchy indent level
						//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw;
					}

					//Log the purge.
					new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId, DateTime.UtcNow, markedRelease.Name);

					//If this is not a major release then we need to also rollup the task and test status to the parent release
					if (releaseType != Release.ReleaseTypeEnum.MajorRelease && parentReleaseId.HasValue)
					{
						RefreshProgressEffortTestStatus(projectId, parentReleaseId.Value);
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Updates a list of releases that are passed-in</summary>
		/// <param name="releases">The list to be persisted</param>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project the releases belong to</param>
		public void Update(List<Release> releases, int userId, int projectId, long? rollbackId = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//We need to access the project to find out the schedule configuration to be used
			ProjectManager projectManager = new ProjectManager();
			Project project = projectManager.RetrieveById(projectId);

			//Iterate through the releases being updated and make sure that we perform any calculations
			//or validations that need to happen before actually updating the data
			foreach (Release releaseRow in releases)
			{
				//First we need to make sure that we don't have any items with Iteration=Yes and Summary=Yes
				//since this is an invalid combination
				if (releaseRow.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration && releaseRow.IsSummary)
					throw new IterationSummaryException(GlobalResources.Messages.ReleaseManager_IterationSummaryException);

				if (releaseRow.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase && releaseRow.IsSummary)
					throw new IterationSummaryException(GlobalResources.Messages.ReleaseManager_PhaseSummaryException);

				//Now make sure the dates aren't reversed.
				if (releaseRow.StartDate > releaseRow.EndDate)
					throw new StartEndDateException();

				//Calculate the planned effort from the dates, # resources and non-working time
				//Calculate the planned effort
				int personHoursNonWorking = (int)(releaseRow.DaysNonWorking * project.WorkingHours);
				int plannedEffort = CalculatePlannedEffort(releaseRow.StartDate, releaseRow.EndDate, releaseRow.ResourceCount, project.WorkingDays, project.WorkingHours, project.NonWorkingHours, personHoursNonWorking);
				releaseRow.PlannedEffort = plannedEffort;
				if (releaseRow.TaskEstimatedEffort.HasValue)
				{
					releaseRow.AvailableEffort = plannedEffort - releaseRow.TaskEstimatedEffort.Value;
				}
				else
				{
					releaseRow.AvailableEffort = plannedEffort;
				}
			}

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Loop through each release
					foreach (Release release in releases)
					{
						//Make sure change tracking is enabled
						release.StartTracking();

						//Update the last-update and concurrency dates
						release.LastUpdateDate = DateTime.UtcNow;
						release.ConcurrencyDate = DateTime.UtcNow;

						//Now apply the changes
						context.Releases.ApplyChanges(release);

						//Save the changes, recording any history changes, and sending any notifications
						context.SaveChanges(userId, true, true, rollbackId);
					}
				}

				//Refresh the task progress and effort values in case anything changed
				List<int> releaseIds = releases.Select(r => r.ReleaseId).ToList();
				RefreshProgressEffortTestStatus(projectId, releaseIds, false, true);
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (EntityConstraintViolationException)
			{
				//This means that the version number is not unique so throw a specific exception
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.ReleaseManager_VersionNumberNotUnique);
				Logger.Flush();
				throw new ReleaseVersionNumberException(GlobalResources.Messages.ReleaseManager_VersionNumberNotUnique);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void CreatePeriodicReviewSchedule(Release release)
		{
			var scheduleGroupId = Guid.NewGuid();
			DateTime reviewDate = release.PeriodicReviewDate.Value;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var alertType = context.PeriodicReviewAlertTypes.Where(x => x.PeriodicReviewAlertId == release.PeriodicReviewAlertId.Value).FirstOrDefault();
				if (alertType != null)
				{
					DateTime alert1 = DateTime.Today;
					int reviewDays = alertType.AlertInDays * -1;

					DateTime alert2 = reviewDate.AddDays(reviewDays);
					if (alert2 < DateTime.Today)
					{
						alert2 = DateTime.Today;
					}

					var schedule1 = CreateScheduleInternal(release, alert1, Guid.NewGuid());
					schedule1.Status = "InitialNotification";

					var schedule2 = CreateScheduleInternal(release, alert2, Guid.NewGuid());
					schedule2.Status = "FinalNotification";

					var query = context.PeriodicReviewNotifications.Where(x => x.ReleaseId == release.ReleaseId && x.IsActive);
					foreach (var item in query)
					{
						item.IsActive = false;
					}

					context.PeriodicReviewNotifications.AddObject(schedule1);
					context.PeriodicReviewNotifications.AddObject(schedule2);
					context.SaveChanges();
				}
			}
		}

		private PeriodicReviewNotification CreateScheduleInternal(Release release, DateTime scheduleTime, Guid scheduleGroupId)
		{
			var schedule = new PeriodicReviewNotification
			{
				IsActive = true,
				ReleaseId = release.ReleaseId,
				ScheduledDate = scheduleTime,
				Status = "Ready"
			};

			return schedule;
		}

		private string GetParameterString(Release content)
		{
			string text = $"The {content.Name} is scheduled for periodic review on {content.PeriodicReviewDate.Value.ToShortDateString()}.  Please review the validation documentation in preparation for this upcoming date.  If you have any questions, please contact the validation program administrator/ manager";
			return text;

		}


		/// <summary>This function reorders a section of the release tree after a delete operation It syncs up the 'indent' level string with the actual normalized data</summary>
		/// <param name="indentLevel">The indent level that we want to reorder from</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		private void ReorderReleasesAfterDelete(int userId, int projectId, string indentLevel)
		{
			const string METHOD_NAME = "ReorderReleasesAfterDelete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to do the reorder
					context.Release_ReorderReleasesAfterDelete(userId, projectId, indentLevel);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>This function reorders a section of the release tree before an insert operation so that there is space in the releases indent-level scheme for the new item</summary>
		/// <param name="indentLevel">The indent level that we want to reorder from</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		/// <param name="projectId">The project we're interested in</param>
		private void ReorderReleasesBeforeInsert(int userId, int projectId, string indentLevel)
		{
			const string METHOD_NAME = "ReorderReleasesBeforeInsert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to do the reorder
					context.Release_ReorderReleasesBeforeInsert(userId, projectId, indentLevel);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Updates the positional data of the list of releases that is passed-in</summary>
		/// <param name="releases">The list to be persisted</param>
		/// <param name="userId">The user we're viewing the releases as</param>
		protected internal void UpdatePositionalData(int userId, List<ReleaseView> releases)
		{
			const string METHOD_NAME = "UpdatePositionalData()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Iterate through the various releases
					foreach (ReleaseView release in releases)
					{
						//Call the stored procedure that makes the updates
						context.Release_UpdatePositional(release.ReleaseId, (userId == User.UserInternal) ? null : (int?)userId, release.IsExpanded, release.IsVisible, release.IsSummary, release.IndentLevel);
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
		}

		/// <summary>Adds a list of releases to a specific test case</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testCaseId">The id of the test case we're associating them with</param>
		/// <param name="releaseIds">The list of releases being added</param>
		/// <remarks>Duplicates are ignored and releases have their child iterations added as well</remarks>
		/// <returns>The ids of the items that were actually mapped (e.g. the child items of a summary release)</returns>
		public List<int> AddToTestCase(int projectId, int testCaseId, List<int> releaseIds, int userId)
		{
			const string METHOD_NAME = "AddToTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped releases (to avoid duplicates)
				List<ReleaseView> mappedReleases = RetrieveMappedByTestCaseId(UserManager.UserInternal, projectId, testCaseId);

				//Now iterate through each of the releases and see if it's a summary with iterations
				List<int> validatedReleaseIds = new List<int>();
				foreach (int releaseId in releaseIds)
				{
					try
					{
						ReleaseView release = RetrieveById2(projectId, releaseId);
						if (release.IsSummary)
						{
							//Get the list of child iterations
							List<ReleaseView> childIterations = RetrieveIterations(projectId, release.ReleaseId, true);
							foreach (ReleaseView iteration in childIterations)
							{
								if (!mappedReleases.Any(r => r.ReleaseId == iteration.ReleaseId))
								{
									if (!validatedReleaseIds.Contains(iteration.ReleaseId))
									{
										validatedReleaseIds.Add(iteration.ReleaseId);
									}
								}
							}
						}
						//Check to see if it's already mapped, if not, add to validated list
						if (!mappedReleases.Any(r => r.ReleaseId == releaseId))
						{
							if (!validatedReleaseIds.Contains(releaseId))
							{
								validatedReleaseIds.Add(releaseId);
							}
						}
					}
					catch (ArtifactNotExistsException)
					{
						//We just ignore releases that have been deleted
					}
				}
				//Now add the validated items to the mapping table
				if (validatedReleaseIds != null && validatedReleaseIds.Count > 0)
				{
					HistoryManager hMgr = new HistoryManager();
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						foreach (int releaseId in validatedReleaseIds)
						{
							context.Release_SaveTestCoverageInsert(releaseId, testCaseId);

							//If we didn't throw an error, call the function to save an association.
							hMgr.AddReleaseTestCoverage(projectId, releaseId, null, userId, new List<int> { testCaseId });
						}
					}
				}

				//Refresh the test cases' execution status
				TestRunManager testRunManager = new TestRunManager();
				testRunManager.RefreshTestCaseExecutionStatus3(projectId, testCaseId);

				//Finally perform a bulk refresh of the release test status
				RefreshProgressEffortTestStatus(projectId, validatedReleaseIds);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validatedReleaseIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Removes a list of releases from a specific test case
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testCaseId">The id of the test case we're de-associating them from</param>
		/// <param name="releaseIds">The list of releases being removed</param>
		/// <remarks>Items that are not already mapped are ignored.</remarks>
		public void RemoveFromTestCase(int projectId, int testCaseId, List<int> releaseIds, int userId)
		{
			const string METHOD_NAME = "RemoveFromTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped test cases (to avoid concurrency errors)
				List<ReleaseView> mappedReleases = RetrieveMappedByTestCaseId(UserManager.UserInternal, projectId, testCaseId);

				//Now iterate through each of the releases and make sure it's mapped already
				List<int> validatedReleaseIds = new List<int>();
				foreach (int releaseId in releaseIds)
				{
					//Check to see if it's already mapped, if so, add to validated list
					if (mappedReleases.Any(r => r.ReleaseId == releaseId))
					{
						validatedReleaseIds.Add(releaseId);
					}
				}
				//Now remove the validated items from the mapping table
				if (validatedReleaseIds != null && validatedReleaseIds.Count > 0)
				{
					HistoryManager hMgr = new HistoryManager();
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						foreach (int releaseId in validatedReleaseIds)
						{
							context.Release_SaveTestCoverageDelete(releaseId, testCaseId);
							//If we didn't throw an error, call the function to save an association.
							hMgr.RemoveReleaseTestCoverage(projectId, releaseId, null, userId, new List<int> { testCaseId });

						}
					}
				}

				//Finally perform a bulk refresh of the release test summary data
				RefreshProgressEffortTestStatus(projectId, validatedReleaseIds);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves the parent releases of the release who's indent level is passed in</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether to include deleted items or not. Default:FALSE</param>
		/// <param name="indentLevel">The indent-level of the release who's parents are to be returned</param>
		/// <param name="immediateParentOnly">Do we want the immediate parent only</param>
		/// <returns>Release list</returns>
		public List<ReleaseView> RetrieveParents(int projectId, string indentLevel, bool immediateParentOnly, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveParents";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Store the length of the passed in indent level
				int length = indentLevel.Length;

				List<ReleaseView> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReleasesView
								where r.ProjectId == projectId
								select r;

					//See if we want deleted ones
					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}

					//See how many levels of parent we want
					if (immediateParentOnly)
					{
						query = query.Where(r => EntityFunctions.Left(indentLevel, r.IndentLevel.Length) == r.IndentLevel && r.IndentLevel.Length == (indentLevel.Length - 3));
					}
					else
					{
						query = query.Where(r => EntityFunctions.Left(indentLevel, r.IndentLevel.Length) == r.IndentLevel && r.IndentLevel.Length < indentLevel.Length);
					}

					//order by indent then id
					query = query.OrderBy(r => r.IndentLevel).ThenBy(r => r.ReleaseId);

					releases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list or releases for a specific project</summary>
		/// <param name="userId">The user we're viewing releases as</param>
		/// <param name="projectId">The ID of the project we're interested in</param>
		/// <param name="activeOnly">Do we want just active releases, or all releases</param>
		/// <param name="filters">The list of filters to apply to the results (null if none)</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row (1-based)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>A release list</returns>
		public List<ReleaseView> RetrieveByProjectId(int userId, int projectId, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Set up the data set that is returned
				List<ReleaseView> releases;

				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = this.CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					releases = context.Release_Retrieve(userId, projectId, filtersClause, startRow, numberOfRows, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Refreshes the summary test status for a particular release/iteration in the release tree</summary>
		/// <param name="projectId">The project we want to refresh the test status for</param>
		/// <param name="recalculateReleaseTestCaseCounts">Should we also update the actual count of test cases</param>
		/// <param name="releaseId">The release/iteration we want to refresh the test information for</param>
		/// <param name="recalculateReleaseTestCaseCounts">Do we want to recalulate the test case status from the test runs themselves</param>
		/// <remarks>If this is an iteration/phase/minor release then we need to roll up the results</remarks>
		protected void RefreshTestStatus(int projectId, int releaseId, bool recalculateReleaseTestCaseCounts = false)
		{
			const string METHOD_NAME = "RefreshTestStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//Next we need to retrieve the passed-in release/iteration record
				ReleaseView releaseView = null;
				try
				{
					releaseView = RetrieveById2(projectId, releaseId);
				}
				catch (ArtifactNotExistsException)
				{
					//The record no longer exists (perhaps deleted by a user so just end quietly)
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				string indentLevel = releaseView.IndentLevel;

				//If the item is a major release then we need to retrieve it plus its children, whereas if the item
				//is an iteration/phase/minor release already then we need to retrieve its parent release(s) and peers
				List<ReleaseView> releasesView;
				if (releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease)
				{
					//Get the release plus its child iterations/phases/releases
					//We need to remove any branches that are themselves a major release since they do not roll-up
					releasesView = RetrieveSelfAndChildren(projectId, releaseId, false, false);
				}
				else
				{
					//Get the parents of this item that need to be rolled-up to. Basically we stop at the first parent that
					//is a major release
					List<ReleaseView> parentReleases = this.RetrieveParents(projectId, indentLevel, false);
					//If we have no parent then we have the unusual case of a top-level iteration,
					//so we can just use the existing entity added to a single-item list
					if (parentReleases.Count > 0)
					{
						int parentReleaseId = -1;
						for (int i = parentReleases.Count - 1; i >= 0; i--)
						{
							//Stop once we hit a major release, or otherwise we just keep going
							parentReleaseId = parentReleases[i].ReleaseId;
							if (parentReleases[i].ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease)
							{
								break;
							}
						}

						//Now get all the child items (note that the first item is the release itself)
						releasesView = this.RetrieveSelfAndChildren(projectId, parentReleaseId, false, false);
					}
					else
					{
						//We have a top-level so just use the iteration itself
						releasesView = new List<ReleaseView>() { releaseView };
					}
				}

				if (releasesView.Count == 0)
				{
					//The record no longer exists (perhaps deleted by a user so just end quietly)
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}

				//Now we need to iterate through each of these and update the test summary data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;

					//[IN:4801] We have to loop through twice, once to calculate all the counts,
					//once then to refresh the test status. If we don't do it that way, we'll be incorrectly using incomplete counts

					if (recalculateReleaseTestCaseCounts)
					{
						foreach (ReleaseView release in releasesView)
						{
							//Refresh the test case counts themselves from the runs
							context.Release_RefreshTestCaseCounts(projectId, release.ReleaseId);
						}
					}

					foreach (ReleaseView release in releasesView)
					{
						//Does the update of that release and the appropriate parents
						context.Release_RefreshTestStatus(projectId, release.ReleaseId);
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

		/// <summary>Retrieves a list or releases for a specific project</summary>
		/// <param name="projectId">The ID of the project we're interested in</param>
		/// <param name="activeOnly">Do we want just active status releases, or all releases</param>
		/// <param name="includeIterationsAndPhases">Do we want to include iterations/phases in the release list</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>A release list</returns>
		/// <remarks>
		/// We use this version when we just want the releases for a dropdown list/lookup and we don't need to worry about
		/// whether the user has an item expanded/closed, etc.
		/// </remarks>
		public List<ReleaseView> RetrieveByProjectId(int projectId, bool activeOnly = true, bool includeIterationsAndPhases = true, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReleaseView> releases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from r in context.ReleasesView
								where r.ProjectId == projectId
								select r;

					//See if we want deleted items
					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}

					//See if we want only 'active' releases
					if (activeOnly)
					{
						//We only want planned/in-progress/completed releases
						query = query.Where(r => r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed);
					}

					//See if we want to include phases/iterations
					if (!includeIterationsAndPhases)
					{
						query = query.Where(r => r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease || r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MinorRelease);
					}

					//Sort by indent-level/id and return
					query = query.OrderBy(r => r.IndentLevel).ThenBy(r => r.ReleaseId);

					releases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releases;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Generates the tooltip to display on release requirements completion bars</summary>
		/// <param name="release">The release entity</param>
		public static string GenerateReqCompletionTooltip(ReleaseView release)
		{
			int requirementCount = release.RequirementCount;

			//Handle case of no requirements
			if (requirementCount == 0)
			{
				return GlobalResources.General.Global_NoRequirements;
			}

			string tooltipText = requirementCount + " " + GlobalResources.General.Global_Requirements + ", " + release.PercentComplete + "% " + GlobalResources.General.Global_Complete;
			return tooltipText;
		}

		/// <summary>Generates the tooltip to display on release task progress bars</summary>
		/// <param name="release">The release entity</param>
		public static string GenerateTaskProgressTooltip(ReleaseView release)
		{
			int taskCount = release.TaskCount;

			//Handle case of no tasks
			if (taskCount == 0)
			{
				return GlobalResources.General.Global_NoTasks;
			}

			string tooltipText =
				"# " + GlobalResources.General.Global_Tasks + "=" + taskCount +
				", " + GlobalResources.General.Task_OnSchedule + "=" + release.TaskPercentOnTime +
				"%, " + GlobalResources.General.Task_RunningLate + "=" + release.TaskPercentLateFinish +
				"%, " + GlobalResources.General.Task_StartingLate + "=" + release.TaskPercentLateStart +
				"%, " + GlobalResources.General.Task_NotStarted + "=" + release.TaskPercentNotStart + "%";
			return tooltipText;
		}

		/// <summary>
		/// Updates the requirements, task and test case progress, effort, status, etc. for a single releases
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release to update</param>
		/// <param name="recalculateReleaseTestCaseCounts">Do we want to recalulate the test case status from the test runs themselves</param>
		/// <param name="refreshParentReleases">Should we also refresh the parent releases of the one passed in</param>
		protected internal void RefreshProgressEffortTestStatus(int projectId, int releaseId, bool recalculateReleaseTestCaseCounts = false, bool refreshParentReleases = false)
		{
			//Call the more general overload
			RefreshProgressEffortTestStatus(projectId, new List<int>() { releaseId }, recalculateReleaseTestCaseCounts, refreshParentReleases);
		}

		/// <summary>
		/// Updates the requirements, task and test case progress, effort, status, etc. for a list of releases
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseIds">The list of the releases to update</param>
		/// <param name="refreshParentReleases">Should we also refresh the parent releases of the one passed in</param>
		/// <param name="recalculateReleaseTestCaseCounts">Do we want to recalulate the test case status from the test runs themselves</param>
		protected internal void RefreshProgressEffortTestStatus(int projectId, List<int> releaseIds, bool recalculateReleaseTestCaseCounts = false, bool refreshParentReleases = false)
		{
			const string METHOD_NAME = "RefreshProgressEffortTestStatus";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//First we need to get the project's planning options
				Project projectManager = new ProjectManager().RetrieveById(projectId);
				bool includeTaskEffort = projectManager.IsEffortTasks;
				bool includeIncidentEffort = projectManager.IsEffortIncidents;
				bool includeTestCaseEffort = projectManager.IsEffortTestCases;

				//See if we also need to refresh its parent releases
				if (refreshParentReleases)
				{
					List<int> specifiedReleaseIds = new List<int>(releaseIds);
					//We also need to get the list of any common parents
					try
					{
						foreach (int specifiedReleaseId in specifiedReleaseIds)
						{
							ReleaseView release = this.RetrieveById2(projectId, specifiedReleaseId);
							List<ReleaseView> parentReleases = RetrieveParents(projectId, release.IndentLevel, false);
							foreach (ReleaseView parentRelease in parentReleases)
							{
								//Add the parent release if not already present in list
								if (!releaseIds.Contains(parentRelease.ReleaseId))
								{
									releaseIds.Add(parentRelease.ReleaseId);
								}
							}
						}
					}
					catch (ArtifactNotExistsException exception)
					{
						//Log as a warning and continue
						Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
					}
				}


				//For right now, loop through all releases
				foreach (int releaseId in releaseIds)
				{
					//The next 3 have to be in this order becuase task and test case status changes will affect requirements status.

					//1. Refresh the task effort/progress
					RefreshProgressAndEffort(projectId, releaseId, includeTaskEffort, includeIncidentEffort, includeTestCaseEffort);

					//2. Refresh the test case status
					RefreshTestStatus(projectId, releaseId, recalculateReleaseTestCaseCounts);

					//3. Refresh the requirement counts/completion
					RefreshRequirementCompletion(projectId, releaseId);
				}

				//Finally we need to refresh the overall requirements completion for this project
				new ProjectManager().RefreshRequirementCompletion(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Refreshes the summary requirement completion (sum of points, # requirements, % complete) for a particular release/iteration in the release tree</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we want to refresh the requirement completion information for</param>
		/// <remarks>It rolls up the values from the child minor releases/iterations/phases</remarks>
		protected void RefreshRequirementCompletion(int projectId, int releaseId)
		{
			const string METHOD_NAME = "RefreshRequirementCompletion";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Refresh the release itself
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.Release_RefreshRequirementCompletion(projectId, releaseId);
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

		/// <summary>Refreshes the summary task progress and task estimated/actual effort for a particular release/iteration in the release tree</summary>
		/// <param name="includeIncidentEffort">Should we include incident effort in the release totals</param>
		/// <param name="includeTaskEffort">Should we include task effort in the release totals</param>
		/// <param name="includeTestCaseEffort">Should we include test case effort in the release totals</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release/iteration we want to refresh the effort/progress information for</param>
		/// <remarks>If this is an iteration/phase/minor release then we need to roll up the results</remarks>
		protected void RefreshProgressAndEffort(int projectId, int releaseId, bool includeTaskEffort, bool includeIncidentEffort, bool includeTestCaseEffort)
		{
			const string METHOD_NAME = "RefreshProgressAndEffort";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Next we need to retrieve the passed-in release/iteration record
				ReleaseView releaseView = null;
				try
				{
					releaseView = RetrieveById2(projectId, releaseId);
				}
				catch (ArtifactNotExistsException)
				{
					//The record no longer exists (perhaps deleted by a user so just end quietly)
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				string indentLevel = releaseView.IndentLevel;

				//If the item is a major release then we need to retrieve it plus its children, whereas if the item
				//is an iteration/phase/minor release already then we need to retrieve its parent release(s) and peers
				List<ReleaseView> releasesView;
				if (releaseView.ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease)
				{
					//Get the release plus its child iterations/phases/releases
					//We need to remove any branches that are themselves a major release since they do not roll-up
					releasesView = RetrieveSelfAndChildren(projectId, releaseId, false, false);
				}
				else
				{
					//Get the parents of this item that need to be rolled-up to. Basically we stop at the first parent that
					//is a major release
					List<ReleaseView> parentReleases = this.RetrieveParents(projectId, indentLevel, false);
					//If we have no parent then we have the unusual case of a top-level iteration,
					//so we can just use the existing entity added to a single-item list
					if (parentReleases.Count > 0)
					{
						int parentReleaseId = -1;
						for (int i = parentReleases.Count - 1; i >= 0; i--)
						{
							//Stop once we hit a major release, or otherwise we just keep going
							parentReleaseId = parentReleases[i].ReleaseId;
							if (parentReleases[i].ReleaseTypeId == (int)Release.ReleaseTypeEnum.MajorRelease)
							{
								break;
							}
						}

						//Now get all the child items (note that the first item is the release itself)
						releasesView = this.RetrieveSelfAndChildren(projectId, parentReleaseId, false, false);
					}
					else
					{
						//We have a top-level so just use the iteration itself
						releasesView = new List<ReleaseView>() { releaseView };
					}
				}

				if (releasesView.Count == 0)
				{
					//The record no longer exists (perhaps deleted by a user so just end quietly)
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Iterate through all the releases/iterations getting the appropriate effort data
					foreach (ReleaseView releaseViewItem in releasesView)
					{
						//Execute the stored procedure
						context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
						context.Release_RefreshProgressAndEffort(projectId, releaseViewItem.ReleaseId, includeTaskEffort, includeIncidentEffort, includeTestCaseEffort);
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

		/// <summary>Undeletes a release and all children releases, making it available to users.</summary>
		/// <param name="incidentID">The release to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void UnDelete(int releaseId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the release
					var query = from r in context.Releases
								where r.ReleaseId == releaseId && r.IsDeleted
								select r;

					Release release = query.FirstOrDefault();
					if (release != null)
					{
						release.StartTracking();
						release.IsDeleted = false;
						context.SaveChanges();

						//If successful, get the release and verify it's good.
						ReleaseView releaseView = this.RetrieveById2(null, releaseId, false);

						if (releaseView != null)
						{
							//Undelete any associated builds
							int projectId = releaseView.ProjectId;
							new BuildManager().UnDeleteForRelease(projectId, releaseId);

							//Get the projectId and recursively undelete items.
							this.CascadeDeleteFlagChange(userId, projectId, releaseView.IndentLevel, false);

							//Get the parent.
							ReleaseView parentRelease = this.RetrieveParent(User.UserInternal, projectId, releaseView.IndentLevel, true);
							if (parentRelease != null)
							{
								int parentReleaseId = parentRelease.ReleaseId;

								//Update summary and expanded flags..
								parentRelease.IsSummary = true;
								parentRelease.IsExpanded = true;
								this.UpdatePositionalData(userId, new List<ReleaseView>() { parentRelease });

								//Update the task and test coverage.
								RefreshProgressEffortTestStatus(projectId, parentReleaseId);
							}

							if (logHistory)
								new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId, rollbackId, DateTime.UtcNow);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}
		}

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="releaseId">The id of the release</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int releaseId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				ReleaseView notificationArt = RetrieveById2(null, releaseId);
				notificationArt.MarkAsAdded();
				new NotificationManager().SendNotificationForArtifact(notificationArt, artifactCustomProperty, newComment);
			}
			catch (Exception exception)
			{
				//Log, but don't throw;
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>Inserts a new release into the system at a point in front of the passed in release</summary>
		/// <param name="existingReleaseId">The release id of the existing release that we are inserting in front of. If null, then insert release at end</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project that the release is associated with</param>
		/// <param name="creatorId">The person who created the release</param>
		/// <param name="name">The name of the release</param>
		/// <param name="versionNumber">The version number string</param>
		/// <param name="position">The position that we want to insert in front of - inserts at end if not provided</param>
		/// <param name="releaseStatus">The status of the release</param>
		/// <param name="description">The detailed description of the release (optional)</param>
		/// <param name="releaseType">The type of release</param>
		/// <param name="startDate">The starting date</param>
		/// <param name="endDate">The ending date</param>
		/// <param name="resourceCount">The number of notional resources available</param>
		/// <param name="daysNonWorking">The number of non-working calendar days to subtract</param>
		/// <param name="ownerId">The owner of the release</param>
		/// <returns>The newly created release ID</returns>
		public int Insert(int userId, int projectId, int creatorId, string name, string description, string versionNumber, int? existingReleaseId, Release.ReleaseStatusEnum releaseStatus, Release.ReleaseTypeEnum releaseType, DateTime startDate, DateTime endDate, decimal resourceCount, decimal daysNonWorking, int? ownerId, bool ignoreLastInserted = false, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert(existingReleaseId)";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int releaseId = -1;

			try
			{
				string indentLevel;

				//Check to see if we are being passed an existing release to insert before
				if (existingReleaseId.HasValue)
				{
					//First lets restore the existing release
					ReleaseView existingRelease = RetrieveById(userId, projectId, existingReleaseId.Value);
					if (existingRelease == null)
					{
						throw new ArtifactNotExistsException(GlobalResources.Messages.Releases_CannotRetrieveExistingRelease);
					}
					indentLevel = existingRelease.IndentLevel;
				}
				else
				{
					//Get the next available indent level and parent id
					indentLevel = this.GetNextAvailableIndentLevel(userId, projectId, ignoreLastInserted);

					//If we have no rows then default to indent-level AAA
					if (String.IsNullOrEmpty(indentLevel))
					{
						indentLevel = "AAA";
					}
					else
					{
						//Now increment the indent level
						indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);
					}
				}

				//Now we need to actually perform the insert
				releaseId = this.Insert(userId, projectId, creatorId, name, description, versionNumber, indentLevel, releaseStatus, releaseType, startDate, endDate, resourceCount, daysNonWorking, ownerId, logHistory);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return releaseId;
		}

		/// <summary>Inserts a new release into the system under an existing parent release/iteration</summary>
		/// <param name="parentReleaseId">The release id of the existing release that we are inserting it as a child under (cannot be iteration/phase)</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project that the release is associated with</param>
		/// <param name="creatorId">The person who created the release</param>
		/// <param name="name">The name of the release</param>
		/// <param name="versionNumber">The version number string</param>
		/// <param name="position">The position that we want to insert in front of - inserts at end if not provided</param>
		/// <param name="releaseStatus">The status of the release</param>
		/// <param name="description">The detailed description of the release (optional)</param>
		/// <param name="releaseType">The type of release</param>
		/// <param name="startDate">The starting date</param>
		/// <param name="endDate">The ending date</param>
		/// <param name="resourceCount">The number of notional resources available</param>
		/// <param name="daysNonWorking">The number of non-working calendar days to subtract</param>
		/// <param name="ownerId">The owner of the release</param>
		/// <returns>The newly created release ID</returns>
		public int InsertChild(int userId, int projectId, int creatorId, string name, string description, string versionNumber, int? parentReleaseId, Release.ReleaseStatusEnum releaseStatus, Release.ReleaseTypeEnum releaseType, DateTime startDate, DateTime endDate, int resourceCount, int daysNonWorking, int? ownerId, bool ignoreLastInserted = false, bool logHistory = true)
		{
			const string METHOD_NAME = "InsertChild";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int releaseId = -1;

			try
			{
				//Check to make sure we are being passed an existing requirement to insert underneath
				if (parentReleaseId.HasValue)
				{
					//First lets restore the existing release
					ReleaseView parentRelease = RetrieveById2(projectId, parentReleaseId.Value);
					if (parentRelease == null)
					{
						throw new ArtifactNotExistsException(GlobalResources.Messages.Releases_CannotRetrieveExistingRelease);
					}

					//Make sure not an iteration or phase
					if (parentRelease.IsIterationOrPhase)
					{
						throw new IterationSummaryException(GlobalResources.Messages.ReleaseManager_IterationSummaryException);
					}

					List<ReleaseView> childReleases = this.RetrieveChildren(Business.UserManager.UserInternal, projectId, parentRelease.IndentLevel, false, true);

					//See if we have any existing child requirements
					if (childReleases.Count > 0)
					{
						//Get the indent level of the last existing child
						string indentLevel = childReleases.Last().IndentLevel;

						//Now get the next indent level and use for that for the new item
						indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);

						//Now insert the requirement at the specified position
						releaseId = this.Insert(userId, projectId, creatorId, name, description, versionNumber, indentLevel, releaseStatus, releaseType, startDate, endDate, resourceCount, daysNonWorking, ownerId, logHistory);

						//Because the child list can contain deleted items, need to explicitly set the parent to a summary item
						//if not already
						if (!parentRelease.IsSummary)
						{
							parentRelease.IsSummary = true;
							parentRelease.IsExpanded = true;
							UpdatePositionalData(userId, new List<ReleaseView>() { parentRelease });
						}
					}
					else
					{
						//We have no children so get the indent level of the parent and increment that
						//i.e. insert after the parent, then we can do an indent
						string indentLevel = HierarchicalList.IncrementIndentLevel(parentRelease.IndentLevel);

						//Now insert the release at the specified position
						releaseId = this.Insert(userId, projectId, creatorId, name, description, versionNumber, indentLevel, releaseStatus, releaseType, startDate, endDate, resourceCount, daysNonWorking, ownerId, logHistory);

						//Finally perform an indent
						this.Indent(userId, projectId, releaseId);
					}
				}
				else
				{
					//Just insert at the end of the list using the normal Insert method
					releaseId = this.Insert(userId, projectId, creatorId, name, description, versionNumber, (int?)null, releaseStatus, releaseType, startDate, endDate, resourceCount, daysNonWorking, ownerId, logHistory);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseId;
			}
			catch (IterationSummaryException)
			{
				//Don't log
				throw;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
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

		/// <summary>Inserts a new release into the system at a specified indent level</summary>
		/// <param name="indentLevel">The indent level to use for the new requirement</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project that the release is associated with</param>
		/// <param name="creatorId">The person who created the release</param>
		/// <param name="name">The name of the release</param>
		/// <param name="versionNumber">The version number string</param>
		/// <param name="position">The position that we want to insert in front of - inserts at end if not provided</param>
		/// <param name="description">The detailed description of the release (optional)</param>
		/// <param name="startDate">The starting date</param>
		/// <param name="endDate">The ending date</param>
		/// <param name="resourceCount">The number of notional resources available</param>
		/// <param name="daysNonWorking">The number of non-working person days to subtract</param>
		/// <param name="openerId">The ID of the user actually creating the item.</param>
		/// <param name="logHistory">Should we log a history creation record</param>
		/// <param name="releaseType">The type of the release</param>
		/// <param name="releaseStatus">The status of the release</param>
		/// <param name="ownerId">The owner of the release</param>
		/// <returns>The newly created release ID</returns>
		public int Insert(int userId, int projectId, int creatorId, string name, string description, string versionNumber, string indentLevel, Release.ReleaseStatusEnum releaseStatus, Release.ReleaseTypeEnum releaseType, DateTime startDate, DateTime endDate, decimal resourceCount, decimal daysNonWorking, int? ownerId, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert(indentLevel)";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int releaseId = -1;

			try
			{
				//If we are not passed in a version number, generate one automatically
				if (String.IsNullOrEmpty(versionNumber))
				{
					//Generate a version number or iteration build # that we know will be unique
					if (releaseType == Release.ReleaseTypeEnum.Iteration || releaseType == Release.ReleaseTypeEnum.Phase)
					{
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							var query = from r in context.Releases
										where (r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Iteration || r.ReleaseTypeId == (int)Release.ReleaseTypeEnum.Phase) && r.ProjectId == projectId
										orderby r.VersionNumber descending, r.ReleaseId
										select r.VersionNumber;

							versionNumber = query.FirstOrDefault();
							if (String.IsNullOrEmpty(versionNumber))
							{
								//We don't have any existing iterations
								versionNumber = "1.0.0.0.0000";
							}
							else
							{
								System.Text.StringBuilder newVersionNumber = new System.Text.StringBuilder(versionNumber);
								//increment that last character unless it's 9
								char element = newVersionNumber[newVersionNumber.Length - 1];
								if (element == '9')
								{
									if (newVersionNumber.Length >= 3)
									{
										if (newVersionNumber[newVersionNumber.Length - 2] == '.')
										{
											newVersionNumber[newVersionNumber.Length - 3]++;
										}
										else
										{
											newVersionNumber[newVersionNumber.Length - 2]++;
										}
									}
									else
									{
										//For one item versions we can't do much
										element++;
									}
									element = '0';
									newVersionNumber[newVersionNumber.Length - 1] = element;
								}
								else if (element < '0' || element > '9')
								{
									//If non-numeric just add a new numeric suffix
									newVersionNumber.Append(".0");
								}
								else
								{
									element++;
									newVersionNumber[newVersionNumber.Length - 1] = element;
								}
								versionNumber = newVersionNumber.ToString();
							}
						}
					}
					else
					{
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							var query = from r in context.Releases
										where (r.ReleaseTypeId != (int)Release.ReleaseTypeEnum.Iteration && r.ReleaseTypeId != (int)Release.ReleaseTypeEnum.Phase) && r.ProjectId == projectId
										orderby r.VersionNumber descending, r.ReleaseId
										select r.VersionNumber;

							versionNumber = query.FirstOrDefault();

							if (String.IsNullOrEmpty(versionNumber))
							{
								//We don't have any existing releases
								versionNumber = "1.0.0.0";
							}
							else
							{
								System.Text.StringBuilder newVersionNumber = new System.Text.StringBuilder(versionNumber);
								//increment that last character unless it's not numeric or is set to 9
								char element = newVersionNumber[newVersionNumber.Length - 1];
								if (element == '9')
								{
									//If we hit 9, just add a zero and make previous a 1
									newVersionNumber[newVersionNumber.Length - 1] = '1';
									newVersionNumber.Append("0");
								}
								else if (element < '0' || element > '9')
								{
									//Add on a numeric suffic
									newVersionNumber.Append(".0");
								}
								else
								{
									//Otherwise just increment the last character
									element++;
									newVersionNumber[newVersionNumber.Length - 1] = element;
								}
								versionNumber = newVersionNumber.ToString();
							}
						}
					}
				}

				//We need to access the project to find out the schedule configuration to be used
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Calculate the planned effort
				int personHoursNonWorking = (int)(daysNonWorking * project.WorkingHours);
				int plannedEffort = CalculatePlannedEffort(startDate, endDate, resourceCount, project.WorkingDays, project.WorkingHours, project.NonWorkingHours, personHoursNonWorking);
				int availableEffort = plannedEffort;    //Since we have no tasks at this point

				try
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Begin transaction - needed to maintain integrity of hierarchy indent level
						using (TransactionScope transactionScope = new TransactionScope())
						{
							//Before inserting, we need to first update the indent levels of the subsequent releases
							//so that there is a gap in the indent level structure for our new release
							ReorderReleasesBeforeInsert(userId, projectId, indentLevel);

							Release release = new Release();
							try
							{
								//Fill out the data for new release
								release.CreatorId = creatorId;
								release.OwnerId = ownerId;
								release.ProjectId = projectId;
								release.Name = name;
								release.Description = description;
								release.VersionNumber = versionNumber;
								release.CreationDate = DateTime.UtcNow;
								release.LastUpdateDate = DateTime.UtcNow;
								release.ConcurrencyDate = DateTime.UtcNow;
								release.IndentLevel = indentLevel;
								release.IsSummary = false;
								release.ReleaseStatusId = (int)releaseStatus;
								release.ReleaseTypeId = (int)releaseType;
								release.IsAttachments = false;
								release.StartDate = startDate;
								release.EndDate = endDate;
								release.DaysNonWorking = daysNonWorking;
								release.PlannedEffort = plannedEffort;
								release.AvailableEffort = availableEffort;
								release.ResourceCount = resourceCount;
								release.IsDeleted = false;

								//ReleaseUser info
								release.UserViewState.Add(new ReleaseUser() { IsExpanded = false, IsVisible = true, UserId = userId });

								//Perform the insert and get the new ID
								context.Releases.AddObject(release);
								context.SaveChanges(userId, false, true, null);
								releaseId = release.ReleaseId;
							}
							catch (EntityConstraintViolationException)
							{
								//This means that the version number is not unique so try use a ficticious one using the primary key
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.ReleaseManager_CreatingFakeVersionNumber);

								var query = from r in context.Releases
											where r.ProjectId == projectId
											orderby r.ReleaseId descending
											select r.ReleaseId;

								int oldReleaseId = query.FirstOrDefault();

								if (oldReleaseId < 1)
								{
									//This is almost impossible - unless someone deleted the last release at the same time
									throw new ReleaseVersionNumberException(GlobalResources.Messages.ReleaseManager_VersionNumberNotUnique);
								}
								oldReleaseId++; //Increment
								release.VersionNumber = "X." + oldReleaseId;

								context.SaveChanges(userId, false, true, null);
								releaseId = release.ReleaseId;
							}

							//Commit transaction - needed to maintain integrity of hierarchy indent level
							transactionScope.Complete();
						}
					}

					//Add a history record for the inserted incident.
					if (logHistory)
						new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId, DateTime.UtcNow);
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//We need to also rollup the requirements, task and test status to the parent release
				RefreshProgressEffortTestStatus(projectId, releaseId);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return releaseId;
		}

		/// <summary>Checks for and optionally corrects indentation errors.</summary>
		/// <param name="projectId">Project ID of releases to check</param>
		/// <param name="performFix">To actually perform the fix or not.</param>
		/// <param name="FillerName">Name of created releases.</param>
		/// <returns>True if all indentations are correct. False if error.</returns>
		public bool CheckIndention(int projectId, bool performFix, string fillerName = "Filler Release", dynamic bkgProcess = null)
		{
			const string METHOD_NAME = CLASS_NAME + "CheckIndention()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool retBool = true;

			//Get the info from our database..
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				List<DataTools_Release> rels = context.DataTools_Releases
					.Where(dt => dt.ProjectId == projectId)
					.OrderBy(dt => dt.IndentLevel)
					.ToList();

				//Get the total amount..
				int totalNum = rels.Count;

				//For each one we find, we need to:
				// - Look at its children. If it has children, and it's deleted, undelete it.
				// - Make sure it has a parent. If it has no parent, create one.
				for (int i = 0; i < rels.Count; i++)
				{
					DataTools_Release rel = rels[i];

					//Update status if we have one..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)(i / (float)(totalNum) * 100);

					// Check that its parents exist, regardless of Deleted Status.
					int indentIndex = 0;
					string parent = "";
					do
					{
						//Advance to the next level...
						indentIndex += 3;
						parent = rel.IndentLevel.Substring(0, indentIndex);

						//See if the parent exists.
						if (rels.Count(r => r.IndentLevel.Equals(parent)) < 1)
						{
							//Parent did not exist. Set flag and create it.
							retBool = false;

							if (performFix)
							{
								//We have to insert it manually into the database, as the functions to automagically insert them
								// will try to check and correct for parents.
								string verNum = new Random().Next(100, 500000).ToString();
								context.Release_InsertFiller(projectId, fillerName, parent, verNum);

								//Log it.
								string msg = "Added missing parent '" + parent + "' to satisfy row #" + i.ToString();
								Logger.LogInformationalEvent(METHOD_NAME, msg);

								//Add it to our list..
								DataTools_Release newRel = new DataTools_Release();
								newRel.IndentLevel = parent;
								newRel.IsDeleted = false;
								newRel.IsSummary = true;
								newRel.ProjectId = projectId;
								newRel.ReleaseId = -1;
								rels.Add(newRel);
							}
						}
					} while (!parent.Equals(rel.IndentLevel));

					// Now check that if it has children, it's marked as summary.
					if (rels.Any(r =>
						!r.IsDeleted &&
						r.IndentLevel.StartsWith(rel.IndentLevel) &&
						r.IndentLevel.Length > rel.IndentLevel.Length))
					{
						if (!rel.IsSummary)
						{
							//Flag as error.
							retBool = false;

							if (performFix)
							{
								//Pull the release..
								Release modRelease = context.Releases.SingleOrDefault(r => r.ReleaseId == rel.ReleaseId);
								modRelease.StartTracking();
								modRelease.IsSummary = true;
								context.SaveChanges();

								//Update this record..
								rel.IsSummary = true;
								try
								{
									context.Detach(rel);
								}
								catch { }
							}
						}

						//If this one is deleted, see if any of its childred are NOT deleted.
						if (rel.IsDeleted)
						{
							int numNoDeleted = rels.Count(r => r.IndentLevel.StartsWith(rel.IndentLevel) && !r.IsDeleted);

							if (numNoDeleted > 0)
							{
								retBool = false;

								if (performFix)
								{
									//Pull the release..
									Release modReq = context.Releases.SingleOrDefault(r => r.ReleaseId == rel.ReleaseId);
									modReq.StartTracking();
									modReq.IsDeleted = false;
									context.SaveChanges();

									//Update this record..
									rel.IsDeleted = false;
									try
									{
										context.Detach(rel);
									}
									catch { }
								}
							}
						}
					}
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retBool;
		}
	}

	/// <summary>This exception is thrown when you try and move a release either above the top of the list, or below the bottom
	/// </summary>
	public class ReleaseMovingException : ApplicationException
	{
		public ReleaseMovingException()
		{
		}
		public ReleaseMovingException(string message)
			: base(message)
		{
		}
		public ReleaseMovingException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>This exception is thrown when you give a release/iteration a version number that is not unique in the project</summary>
	public class ReleaseVersionNumberException : ApplicationException
	{
		public ReleaseVersionNumberException()
		{
		}
		public ReleaseVersionNumberException(string message)
			: base(message)
		{
		}
		public ReleaseVersionNumberException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try and perform
	/// an operation that would iterations summary items (not allowed)
	/// </summary>
	public class IterationSummaryException : ApplicationException
	{
		public IterationSummaryException()
		{
		}
		public IterationSummaryException(string message)
			: base(message)
		{
		}
		public IterationSummaryException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>This exception is thrown when a user tries to set the start date after the end date.</summary>
	public class StartEndDateException : ApplicationException
	{
		/// <summary>Creates a new instance of the exception.</summary>
		/// <param name="message">The message for the exception.</param>
		public StartEndDateException(string message = "The start date cannot be after the end date.")
			: base(message)
		{ }

		/// <summary>Creates a new instance of the exception.</summary>
		/// <param name="InnerException">The inner exception.</param>
		/// <param name="message">The message for the exception.</param>
		public StartEndDateException(Exception innerException, string message = "The start date cannot be after the end date.")
			: base(message, innerException)
		{ }
	}
}
