using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.Business.HistoryManager;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	public class UserActivityLogManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.UserActivityLogManager::";

		#region Internal Methods
		
		/// <summary>Logs a creation into the ChangeSet for the specified adminSection id, type.</summary>
		/// <param name="userId">The userid who performed the creation.</param>
		/// <param name="adminSectionId">The adminSectionId.</param>
		/// <param name="changeDate">The date of the creation. If null, uses current date/time.</param>
		/// <returns>The ID of the changeset.</returns>
		internal long LogCreation(int userId, int adminSectionId, int updatedUserId, string userName, string action, DateTime? changeDate = null)
		{
			const string METHOD_NAME = "LogCreation()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new changeset.
			TST_USER_ACTIVITY_LOG_AUDIT hsChangeSet = new TST_USER_ACTIVITY_LOG_AUDIT();
			hsChangeSet.USER_ID = userId;
			hsChangeSet.ACTIVITY_DESCRIPTION = action;
			hsChangeSet.LOGIN_DATE = DateTime.UtcNow;

			long changeSetId = Insert(hsChangeSet);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return changeSetId;
		}

		#endregion Internal Methods

		/// <summary>
		/// Logs a single change set
		/// </summary>
		/// <param name="userHistoryChangeSet">A single changeset</param>
		/// <returns></returns>
		internal long Insert(TST_USER_ACTIVITY_LOG_AUDIT userHistoryChangeSet)
		{
			List<TST_USER_ACTIVITY_LOG_AUDIT> userHistoryChangeSets = new List<TST_USER_ACTIVITY_LOG_AUDIT>();
			userHistoryChangeSets.Add(userHistoryChangeSet);
			return Insert(userHistoryChangeSets);
		}

		/// <summary>Inserts a History Change set. Needs to have an unsaved ChangeSet row(s), with 0 or more HistoryField rows.</summary>
		/// <param name="userHistoryChangeSets">The changesets.</param>
		/// <returns>The id of the FIRST ChangeSet inserted.</returns>
		protected long Insert(List<TST_USER_ACTIVITY_LOG_AUDIT> userHistoryChangeSets)
		{
			const string METHOD_NAME = "Insert";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//All we're doing is saving this entity
				long userHistoryChangeSetId = 0;
				if (userHistoryChangeSets.Count > 0)
				{
					using (AuditTrailEntities context = new AuditTrailEntities())
					{
						foreach (TST_USER_ACTIVITY_LOG_AUDIT userHistoryChangeSet in userHistoryChangeSets)
						{
							TST_USER_ACTIVITY_LOG_AUDIT userActivityLogData = new TST_USER_ACTIVITY_LOG_AUDIT();

							userActivityLogData.USER_ID = userHistoryChangeSet.USER_ID;
							userActivityLogData.LOGIN_DATE = userHistoryChangeSet.LOGIN_DATE;
							userActivityLogData.LOGOUT_DATE = userHistoryChangeSet.LOGOUT_DATE;
							userActivityLogData.ACTIVITY_DESCRIPTION = userHistoryChangeSet.ACTIVITY_DESCRIPTION;

							context.TST_USER_ACTIVITY_LOG_AUDIT.Add(userActivityLogData);

						}
						context.SaveChanges();

						userHistoryChangeSetId = context.TST_USER_ACTIVITY_LOG_AUDIT.OrderByDescending(q => q.USER_ACTIVITY_LOG_ID)
.FirstOrDefault().USER_ACTIVITY_LOG_ID;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return userHistoryChangeSetId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<UserActivityLogResponse> RetrieveUserActivityLogs(double utcOffset, string sortProperty = null, bool sortAscending = true, Hashtable filterList = null, int startRow = 1, int paginationSize = -1)
		{
			const string METHOD_NAME = "RetrieveSetsByProjectId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<UserActivityLogResponse> changeSets;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					var query = from h in context.UserActivityLogs
								join u in context.UserProfiles
								on h.USER_ID equals u.UserId
								select new UserActivityLogResponse
								{
									ActivityDescription = h.ACTIVITY_DESCRIPTION,
									UserActivityLogId = h.USER_ACTIVITY_LOG_ID,
									LoginDate = h.LOGIN_DATE,
									LogoutDate = h.LOGOUT_DATE,
									//TimeZone = "UTC",
									UserName = u.FirstName + " " + u.LastName,
									Time = "",
									UserId = h.USER_ID,
								};

					//Add the dynamic filters
					if (filterList != null && !filterList.ContainsKey("Time"))
					{
						//Handle the signed filter separately (in memory using pure LINQ not LINQ-to-entities)
						List<string> ignoreList = new List<string>() { "Signed" };

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<UserActivityLogResponse, bool>> filterClause = CreateFilterExpression<UserActivityLogResponse>(null, null, ArtifactTypeEnum.None, filterList, utcOffset, ignoreList, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<UserActivityLogResponse>)query.Where(filterClause);
						}
					}

					string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
					query = query.OrderUsingSortExpression(sortExpression, "UserActivityLogId");

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

					if (filterList != null && filterList.ContainsKey("Time"))
					{
						if (filterList["Time"] is string)
						{
							string time = (string)filterList["Time"];
							changeSets = changeSets.Where(c => c.Time.Contains(time)).ToList();
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
					var query = from h in context.UserActivityLogs
								select h;

					//Add the dynamic filters
					if (filters != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TST_USER_ACTIVITY_LOG, bool>> filterClause = CreateFilterExpression<TST_USER_ACTIVITY_LOG>(projectId, null, ArtifactTypeEnum.None, filters, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TST_USER_ACTIVITY_LOG>)query.Where(filterClause);
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

		/// <summary>Retrieves the audit history log associated with a particular ChangeSet</summary>
		/// <param name="changeSetId">The changeset ID to retrieve</param>
		/// <param name="filterList">The list of filters to apply</param>
		/// <param name="sortProperty">What field to sort on</param>
		/// <param name="sortAscending">What direction to sort</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="paginationSize">The number of rows to return</param>
		/// <returns>A history detail list</returns>
		public List<UserActivityLogResponse> RetrieveByActivityLogId(int projectId, long changeSetId, string sortProperty, bool sortAscending, Hashtable filterList, int startRow, int paginationSize, double utcOffset)
		{
			const string METHOD_NAME = "RetrieveByActivityLogId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<UserActivityLogResponse> historyChanges;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from h in context.UserActivityLogs
								select new UserActivityLogResponse
								{
									ActivityDescription = h.ACTIVITY_DESCRIPTION,
									UserActivityLogId = h.USER_ACTIVITY_LOG_ID,
									LoginDate = h.LOGIN_DATE,
									TimeZone = "UTC",
									LogoutDate = h.LOGOUT_DATE,
									UserName = h.TST_USER.UserName,
									Time = "",
									UserId = h.USER_ID,
								};

					//Add the dynamic filters
					if (filterList != null)
					{
						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<UserActivityLogResponse, bool>> filterClause = CreateFilterExpression<UserActivityLogResponse>(projectId, null, ArtifactTypeEnum.None, filterList, utcOffset, null, null);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<UserActivityLogResponse>)query.Where(filterClause);
						}
					}

					//We always sort by the physical ID to guarantee stable sorting
					string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
					query = query.OrderUsingSortExpression(sortExpression, "UserActivityLogId");
					

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

		public TST_USER_ACTIVITY_LOG RetrieveActivityLogById(long activityLogId, bool includeDetails, bool includeAssociations = false, bool includePositions = false)
		{
			const string METHOD_NAME = "RetrieveActivityLogById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				TST_USER_ACTIVITY_LOG changeSet;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<TST_USER_ACTIVITY_LOG> historyChangeSets = context.UserActivityLogs
						.Include(h => h.TST_USER);

					changeSet = historyChangeSets.FirstOrDefault(h => h.USER_ACTIVITY_LOG_ID == activityLogId);
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

		/// <summary>Get Prcentage of active users</summary>
		/// <param name="year">Year</param>
		/// <param name="month">Month</param>
		public List<SYSTEM_USAGE_REPORT_Result> GetSystemUsageReport(string year)
		{
			const string METHOD_NAME = "GetPercentageofActiveUsers()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<SYSTEM_USAGE_REPORT_Result> result = new List<SYSTEM_USAGE_REPORT_Result>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the appropriate stored procedure depending on whether we're deleting for a project or artifact
					
						//percentage = context.PercentageCalculation(month, year).FirstOrDefault();
						result = context.GetSustemUsageReport(year).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return result;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<SystemUsageReportResponse> RetrieveSystemUsageReport(string year)
		{
			const string METHOD_NAME = "RetrieveSystemUsageReport()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<SystemUsageReportResponse> systemUsageReports = new List<SystemUsageReportResponse>();

				var data = GetSystemUsageReport(year);
				foreach(var report in data)
				{
					SystemUsageReportResponse reportData = new SystemUsageReportResponse();
					reportData.MonthName = report.MonthName;
					reportData.ActiveAccount = report.NumberOfActiveAccount != null ? report.NumberOfActiveAccount.ToString() : "";
					reportData.ActiveUserPercentage = report.ActiveUserPercentage != null ? Math.Round(decimal.Parse(report.ActiveUserPercentage.ToString())).ToString() : "";
					reportData.AvgNoOfConnPerDay = report.PerDay != null ? report.PerDay.ToString() : "";
					reportData.AvgNoOfConnPerWeek = report.PerWeek != null ? decimal.Parse(report.PerWeek.ToString()).ToString("0.00") : "";
					reportData.AvgNoOfConnPerMonth = report.PerMonth != null ? report.PerMonth.ToString(): "";
					reportData.AvgConnTimePerDay = report.TimePerDay != null ? report.TimePerDay.ToString() : "";
					reportData.AvgConnTimePerWeek = report.TimePerWeek != null ? report.TimePerWeek.ToString() : "";
					reportData.AvgConnTimePerMonth = report.TimePerMonth != null ? report.TimePerMonth.ToString() : "";
					systemUsageReports.Add(reportData);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return systemUsageReports;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}
}
