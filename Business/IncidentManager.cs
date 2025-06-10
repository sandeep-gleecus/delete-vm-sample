using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Collections;
using System.Linq.Expressions;
using System.Data;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for reading and writing Incidents that are submitted and managed in the system.</summary>
	public class IncidentManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.IncidentManager";

		//List of the special incident types and statuses used in filters to pull back aggregate statuses

		//Incident Status
		public const int IncidentStatusId_AllOpen = -2;
		public const int IncidentStatusId_AllClosed = -3;

		//Incident Type
		public const int IncidentTypeId_AllIssues = -2;
		public const int IncidentTypeId_AllRisks = -3;

		//Cached lookups
		protected Dictionary<string, string> progressFiltersList;

		#region Incident Status Functions

		/// <summary>Gets the default (i.e. initial) incident status for all newly created incidents</summary>
		/// <param name="projectTemplateId">The current Project Template ID</param>
		/// <returns>The incident status</returns>
		/// <remarks>Returns null is there is no default incident status for the project template (shouldn't really happen)</remarks>
		public IncidentStatus IncidentStatus_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "IncidentStatus_RetrieveDefault";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentStatus incidentStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentStati
								where i.IsDefault && i.ProjectTemplateId == projectTemplateId
								select i;

					incidentStatus = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatus;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the IncidentStatus by the given ID.</summary>
		/// <param name="incidentStatusId">The status ID to retrieve.</param>
		/// <param name="workflowId">Set the value if you only want workflow fields/custom properties for a specific workflow</param>
		/// <returns>The IncidentStatus, or null if not found.</returns>
		/// <remarks>Will return deleted items.</remarks>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		public IncidentStatus IncidentStatus_RetrieveById(int incidentStatusId, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "IncidentStatus_RetrieveById(int,[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			IncidentStatus retStatus = null;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				ObjectQuery<IncidentStatus> incidentStati = context.IncidentStati;
				if (includeWorkflowFields)
				{
					incidentStati = incidentStati.Include("WorkflowFields").Include("WorkflowCustomProperties");
				}
				var query = from ts in incidentStati
							where ts.IncidentStatusId == incidentStatusId
							select ts;

				try
				{
					retStatus = query.First();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Retrieving Incident Status ID #" + incidentStatusId + ":");
					retStatus = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		/// <summary>
		/// Returns a list of all the open status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> IncidentStatus_RetrieveOpenIds(int projectTemplateId)
		{
			const string METHOD_NAME = "IncidentStatus_RetrieveOpenIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentStati
								where i.ProjectTemplateId == projectTemplateId && i.IsOpenStatus && i.IsActive
								orderby i.IncidentStatusId
								select i.IncidentStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the closed status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> IncidentStatus_RetrieveClosedIds(int projectTemplateId)
		{
			const string METHOD_NAME = "IncidentStatus_RetrieveClosedIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentStati
								where i.ProjectTemplateId == projectTemplateId && !i.IsOpenStatus && i.IsActive
								orderby i.IncidentStatusId
								select i.IncidentStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the all the incident statuses in the project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="activeOnly">Do we only want active statuses</param>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		/// <returns>The IncidentStatuses</returns>
		public List<IncidentStatus> IncidentStatus_Retrieve(int projectTemplateId, bool activeOnly = true, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "IncidentStatus_Retrieve(int,[bool],[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<IncidentStatus> retStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<IncidentStatus> incidentStati = context.IncidentStati;
					if (includeWorkflowFields)
					{
						incidentStati = incidentStati.Include("WorkflowFields").Include("WorkflowCustomProperties");
					}
					var query = from i in incidentStati
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.IncidentStatusId, i.Name
								select i;

					retStatus = query.OrderByDescending(i => i.IncidentStatusId).ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return retStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Updates an incident status in the system.</summary>
		/// <param name="incidentStatus">The status to update.</param>
		/// <returns>The updated status.</returns>
		public IncidentStatus IncidentStatus_Update(IncidentStatus incidentStatus)
		{
			const string METHOD_NAME = CLASS_NAME + "IncidentStatus_Update(IncidentStatus)";

			Logger.LogEnteringEvent(METHOD_NAME);

			IncidentStatus retStatus = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.IncidentStati.ApplyChanges(incidentStatus);
					context.SaveChanges();

					retStatus = incidentStatus;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Saving Status");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		/// <summary>Inserts a new incident status for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the incident status belongs to</param>
		/// <param name="name">The display name of the incident status</param>
		/// <param name="active">Whether the incident status is active or not</param>
		/// <param name="open">Is this incident status considered 'open'</param>
		/// <param name="defaultStatus">Is this the default (initial) status of newly created incidents</param>
		/// <returns>The newly created incident status id</returns>
		public int IncidentStatus_Insert(int projectTemplateId, string name, bool open, bool defaultStatus, bool active)
		{
			const string METHOD_NAME = "IncidentStatus_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentStatusId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					IncidentStatus incidentStatus = new IncidentStatus();
					incidentStatus.ProjectTemplateId = projectTemplateId;
					incidentStatus.Name = name.MaxLength(20);
					incidentStatus.IsDefault = defaultStatus;
					incidentStatus.IsOpenStatus = open;
					incidentStatus.IsActive = active;

					context.IncidentStati.AddObject(incidentStatus);
					context.SaveChanges();
					incidentStatusId = incidentStatus.IncidentStatusId;
				}

				//Now capture the newly created id and return
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatusId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		#endregion

		#region Incident Functions

		/// <summary>
		/// Returns the list of test run steps linked to an incident
		/// </summary>
		/// <param name="incidentId">The id of the incident</param>
		/// <returns>List of associations</returns>
		public List<TestRunStepIncidentView> Incident_RetrieveTestRunSteps(int incidentId)
		{
			const string METHOD_NAME = "Incident_RetrieveTestRunSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestRunStepIncidentView> testRunStepIncidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestRunStepIncidentsView
								where t.IncidentId == incidentId
								orderby t.TestRunStepId
								select t;

					testRunStepIncidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testRunStepIncidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Adds incident associations to the current test run step
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id the current test run step</param>
		/// <param name="incidentIds">The list of incident ids to associate with the test run step</param>
		/// <param name="userId">The id of the user making the association</param>
		/// <remarks>
		/// It does not let you link an incident in one project to a test run step in another project,
		/// currently cross-project associations does not allow this
		/// </remarks>
		public void Incident_AssociateToTestRunStep(int projectId, int testRunStepId, List<int> incidentIds, int userId)
		{
			const string METHOD_NAME = "Incident_AssociateToTestRunStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have at least one incident
				if (incidentIds != null && incidentIds.Count > 0)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Return the test run step and make sure it exists
						var query = from t in context.TestRunSteps.Include(t => t.Incidents)
									where t.TestRunStepId == testRunStepId
									select t;

						TestRunStep testRunStep = query.FirstOrDefault();
						if (testRunStep != null)
						{
							//Get the actual incidents (so that we can check they exist and are in the same project)
							var query2 = from i in context.Incidents
										 where incidentIds.Contains(i.IncidentId)
										 select i;
							List<Incident> incidents = query2.ToList();

							testRunStep.StartTracking();

							//Add the incidents to the test run step
							foreach (Incident incident in incidents)
							{
								//Make sure it does not already exist in the collection
								if (!testRunStep.Incidents.Any(i => i.IncidentId == incident.IncidentId) && incident.ProjectId == projectId)
								{
									testRunStep.Incidents.Add(incident);
								}
							}
						}

						//Commit the changes,(logging history)
						context.SaveChanges(userId, true, true, null);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Remove incident associations from the current test run step
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testRunStepId">The id the current test run step</param>
		/// <param name="incidentIds">The list of incident ids to remove association with the test run step</param>
		/// <param name="userId">The id of the user making the association</param>
		/// <remarks>
		/// As of 6.9 NOT used in the UI only for unit test cleanup
		/// </remarks>
		public void Incident_AssociateToTestRunStepRemove(int projectId, int testRunStepId, List<int> incidentIds, int userId)
		{
			const string METHOD_NAME = "Incident_AssociateToTestRunStepRemove";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have at least one incident
				if (incidentIds != null && incidentIds.Count > 0)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Return the test run step and make sure it exists
						var query = from t in context.TestRunSteps.Include(t => t.Incidents)
									where t.TestRunStepId == testRunStepId
									select t;

						TestRunStep testRunStep = query.FirstOrDefault();
						if (testRunStep != null)
						{
							//Get the actual incidents (so that we can check they exist and are in the same project)
							var query2 = from i in context.Incidents
										 where incidentIds.Contains(i.IncidentId)
										 select i;
							List<Incident> incidents = query2.ToList();

							testRunStep.StartTracking();

							//Add the incidents to the test run step
							foreach (Incident incident in incidents)
							{
								//Make sure it already exists in the collection
								if (testRunStep.Incidents.Any(i => i.IncidentId == incident.IncidentId) && incident.ProjectId == projectId)
								{
									testRunStep.Incidents.Remove(incident);
								}
							}
						}

						//Commit the changes,(logging history)
						context.SaveChanges(userId, true, true, null);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Handles any Incident specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">the current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleIncidentSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of progress, since it doesn't map to a single column
			if (filterProperty == "ProgressId")
			{
				if (filterValue is Int32)
				{
					switch ((int)filterValue)
					{
						//Not Started
						case 1:
							{
								//INC.COMPLETION_PERCENT = 0 AND (INC.START_DATE >= GETUTCDATE() OR INC.START_DATE IS NULL) 
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2a = Expression.GreaterThanOrEqual(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								Expression expression2b = Expression.Equal(startDateExp, LambdaExpression.Constant(null, typeof(DateTime?)));
								Expression expression2 = Expression.Or(expression2a, expression2b);
								expressionList.Add(expression2);
								break;
							}
						//Late Starting
						case 2:
							{
								//INC.COMPLETION_PERCENT = 0 AND INC.START_DATE < GETUTCDATE()
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								break;
							}

						//On Schedule
						case 3:
							{
								// -- Incidents do not currently have an END-DATE (unlike Tasks)
								//AND INC.COMPLETION_PERCENT > 0 AND INC.COMPLETION_PERCENT < 100 AND INC.END_DATE >= GETUTCDATE()
								//AND INC.COMPLETION_PERCENT > 0 AND INC.COMPLETION_PERCENT < 100 
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								Expression expression1 = Expression.GreaterThan(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression2);
								break;
							}

						//Late Finishing
						case 4:
							{
								// -- Incidents do not currently have an END-DATE
								//AND INC.COMPLETION_PERCENT < 100 AND INC.END_DATE < GETUTCDATE()
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression endDateExp = LambdaExpression.PropertyOrField(p, "EndDate");
								Expression expression1 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(endDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								break;
							}

						//Completed
						case 5:
							{
								//COMPLETION_PERCENT = 100
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								Expression expression = Expression.Equal(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression);
							}
							break;
					}
				}
				return true;
			}

			//Handle the special case of release filters where we want to also retrieve child iterations
			if (filterProperty == "DetectedReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "DetectedReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}
			if (filterProperty == "ResolvedReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "ResolvedReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}
			if (filterProperty == "VerifiedReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "VerifiedReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}

			//If we have the case of incident types and statuses then see if we have
			//one of the special aggregate values (denoted by negative ids)

			//Incident Status
			if (filterProperty == "IncidentStatusId" && projectTemplateId.HasValue)
			{
				//See if it's using a simple int or a multi-value filter
				int? incidentStatusId = null;
				if (filterValue is MultiValueFilter)
				{
					MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
					if (multiValueFilter.Values.Count == 1)
					{
						incidentStatusId = multiValueFilter.Values[0];
					}
				}
				if (filterValue is Int32)
				{
					incidentStatusId = (int)filterValue;
				}

				if (incidentStatusId.HasValue)
				{
					//All Open
					if (incidentStatusId == IncidentStatusId_AllOpen)
					{
						List<int> ids = IncidentStatus_RetrieveOpenIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentStatusId");
						//Equivalent to: p => ids.Contains(p.IncidentStatusId) i.e. (INCIDENT_STATUS_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
					//All Closed
					if (incidentStatusId == IncidentStatusId_AllClosed)
					{
						List<int> ids = IncidentStatus_RetrieveClosedIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentStatusId");
						//Equivalent to: p => ids.Contains(p.IncidentStatusId) i.e. (INCIDENT_STATUS_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
				}
			}

			//Incident Type
			if (filterProperty == "IncidentTypeId" && projectTemplateId.HasValue)
			{
				//See if it's using a simple int or a multi-value filter
				int? incidentTypeId = null;
				if (filterValue is MultiValueFilter)
				{
					MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
					if (multiValueFilter.Values.Count == 1)
					{
						incidentTypeId = multiValueFilter.Values[0];
					}
				}
				if (filterValue is Int32)
				{
					incidentTypeId = (int)filterValue;
				}

				if (incidentTypeId.HasValue)
				{
					//All Risks
					if (incidentTypeId == IncidentTypeId_AllRisks)
					{
						List<int> ids = IncidentType_RetrieveRiskIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentTypeId");
						//Equivalent to: p => ids.Contains(p.IncidentTypeId) i.e. (INCIDENT_TYPE_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
					//All Issues
					if (incidentTypeId == IncidentTypeId_AllIssues)
					{
						List<int> ids = IncidentType_RetrieveIssueIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentTypeId");
						//Equivalent to: p => ids.Contains(p.IncidentTypeId) i.e. (INCIDENT_TYPE_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
				}
			}

			//These ones were already handled beforehand in HandleIncidentSpecificFiltersEx()
			if (filterProperty == "TestCaseId" || filterProperty == "TestRunId" || filterProperty == "TestSetId" || filterProperty == "TestStepId")
			{
				return true;
			}

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>Handles any Incident specific filters that are not generic, in the incidents per group retrieval</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project (not used)</param>
		/// <param name="projectTemplateId">The current project template (not used)</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleIncidentSpecificFiltersForGroup(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of progress, since it doesn't map to a single column
			if (filterProperty == "ProgressId")
			{
				if (filterValue is Int32)
				{
					switch ((int)filterValue)
					{
						//Not Started
						case 1:
							{
								//INC.COMPLETION_PERCENT = 0 AND (INC.START_DATE >= GETUTCDATE() OR INC.START_DATE IS NULL) 
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2a = Expression.GreaterThanOrEqual(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								Expression expression2b = Expression.Equal(startDateExp, LambdaExpression.Constant(null, typeof(DateTime?)));
								Expression expression2 = Expression.Or(expression2a, expression2b);
								expressionList.Add(expression2);
								break;
							}
						//Late Starting
						case 2:
							{
								//INC.COMPLETION_PERCENT = 0 AND INC.START_DATE < GETUTCDATE()
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression startDateExp = LambdaExpression.PropertyOrField(p, "StartDate");
								Expression expression1 = Expression.Equal(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(startDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								break;
							}

						//On Schedule
						case 3:
							{
								// -- Incidents do not currently have an END-DATE (unlike Tasks)
								//AND INC.COMPLETION_PERCENT > 0 AND INC.COMPLETION_PERCENT < 100 AND INC.END_DATE >= GETUTCDATE()
								//AND INC.COMPLETION_PERCENT > 0 AND INC.COMPLETION_PERCENT < 100 
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								Expression expression1 = Expression.GreaterThan(completionPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression2);
								break;
							}

						//Late Finishing
						case 4:
							{
								// -- Incidents do not currently have an END-DATE
								//AND INC.COMPLETION_PERCENT < 100 AND INC.END_DATE < GETUTCDATE()
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								MemberExpression endDateExp = LambdaExpression.PropertyOrField(p, "EndDate");
								Expression expression1 = Expression.LessThan(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression1);
								Expression expression2 = Expression.LessThan(endDateExp, LambdaExpression.Constant(DateTime.UtcNow, typeof(DateTime?)));
								expressionList.Add(expression2);
								break;
							}

						//Completed
						case 5:
							{
								//COMPLETION_PERCENT = 100
								MemberExpression completionPercentExp = LambdaExpression.PropertyOrField(p, "CompletionPercent");
								Expression expression = Expression.Equal(completionPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression);
							}
							break;
					}
				}
				return true;
			}

			//If we have the case of incident types and statuses then see if we have
			//one of the special aggregate values (denoted by negative ids)

			//Project ID
			if (filterProperty == "ProjectId")
			{
				//If we have a (none) filter, simply ignore since all incidents have a project
				if (filterValue is MultiValueFilter)
				{
					MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
					if (multiValueFilter.IsNone)
					{
						//Tell parent function that we're handling it
						return true;
					}
				}
				if (filterValue is Int32)
				{
					int incidentStatusId = (int)filterValue;
					if (incidentStatusId == NoneFilterValue)
					{
						//Tell parent function that we're handling it
						return true;
					}
				}
			}

			//Incident Status
			if (filterProperty == "IncidentStatusId")
			{
				//See if it's using a simple int or a multi-value filter
				int? incidentStatusId = null;
				if (filterValue is MultiValueFilter)
				{
					MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
					if (multiValueFilter.Values.Count == 1)
					{
						incidentStatusId = multiValueFilter.Values[0];
					}
				}
				if (filterValue is Int32)
				{
					incidentStatusId = (int)filterValue;
				}

				if (incidentStatusId.HasValue)
				{
					//All Open
					if (incidentStatusId == IncidentStatusId_AllOpen)
					{
						ConstantExpression boolExpression = LambdaExpression.Constant(true);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentStatusIsOpenStatus");
						Expression expression = Expression.Equal(memberExpression, boolExpression);
						expressionList.Add(expression);
						return true;
					}
					//All Closed
					if (incidentStatusId == IncidentStatusId_AllClosed)
					{
						ConstantExpression boolExpression = LambdaExpression.Constant(false);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "IncidentStatusIsOpenStatus");
						Expression expression = Expression.Equal(memberExpression, boolExpression);
						expressionList.Add(expression);
						return true;
					}
				}
			}

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>
		/// Adds on any complex filters to the LINQ query that cannot be done easily using Expressions
		/// </summary>
		/// <param name="filters">The filters</param>
		/// <param name="query">The query</param>
		/// <see cref="HandleIncidentSpecificFilters"/>
		protected void HandleIncidentSpecificFiltersEx(ref IQueryable<IncidentView> query, Hashtable filters, SpiraTestEntitiesEx context)
		{
			//Filtered by test case, test run, test set or test step requires special handling since we have to do a sub-query
			if (filters.ContainsKey("TestCaseId") && filters["TestCaseId"].GetType() == typeof(int))
			{
				int testCaseId = (int)filters["TestCaseId"];
				//Use the TestCase -> Incidents VIEW to get just the incidents linked to this specific test case
				query = query.Where(p => context.TestCaseIncidentsView.Any(t => t.IncidentId == p.IncidentId && t.TestCaseId == testCaseId));
			}
			if (filters.ContainsKey("TestRunId") && filters["TestRunId"].GetType() == typeof(int))
			{
				int testRunId = (int)filters["TestRunId"];
				//Use the TestCase -> Incidents VIEW to get just the incidents linked to this specific test run
				query = query.Where(p => context.TestRunIncidentsView.Any(t => t.IncidentId == p.IncidentId && t.TestRunId == testRunId));
			}
			if (filters.ContainsKey("TestSetId") && filters["TestSetId"].GetType() == typeof(int))
			{
				int testSetId = (int)filters["TestSetId"];
				//Use the TestCase -> Incidents VIEW to get just the incidents linked to this specific test set
				query = query.Where(p => context.TestSetIncidentsView.Any(t => t.IncidentId == p.IncidentId && t.TestSetId == testSetId));
			}
			if (filters.ContainsKey("TestStepId") && filters["TestStepId"].GetType() == typeof(int))
			{
				int testStepId = (int)filters["TestStepId"]; ;
				//Use the TestStep -> Incidents VIEW to get just the incidents linked to this specific test step
				query = query.Where(p => context.TestStepIncidentsView.Any(t => t.IncidentId == p.IncidentId && t.TestStepId == testStepId));
			}
		}

		/// <summary>
		/// Creates a new incident from an existing task
		/// </summary>
		/// <param name="taskId">The id of the task</param>
		/// <param name="userId">The id of the user doing the creation</param>
		/// <returns>The id of the new incident</returns>
		public int Incident_CreateFromTask(int taskId, int userId)
		{
			const string METHOD_NAME = "Incident_CreateFromTask()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the task in question
				TaskManager taskManager = new TaskManager();
				Task task = taskManager.RetrieveById(taskId);
				int projectId = task.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we can match the task priority to an incident priority by Score or Name
				int? incidentProrityId = null;
				if (task.TaskPriorityId.HasValue)
				{
					TaskPriority taskPriority = taskManager.TaskPriority_RetrieveById(task.TaskPriorityId.Value);
					if (taskPriority != null)
					{
						//See if we have a match on score (or name)
						List<IncidentPriority> incidentPriorities = RetrieveIncidentPriorities(projectTemplateId, true);
						IncidentPriority incidentPriority = incidentPriorities.FirstOrDefault(i => i.Score == taskPriority.Score || i.Name == taskPriority.Name);
						if (incidentPriority != null)
						{
							incidentProrityId = incidentPriority.PriorityId;
						}
					}
				}

				//See if we have a component specified, it comes from the associated requirement
				List<int> componentIds = new List<int>();
				if (task.RequirementId.HasValue)
				{
					try
					{
						RequirementManager requirementManager = new RequirementManager();
						Requirement requirement = requirementManager.RetrieveById3(projectId, task.RequirementId.Value);
						if (requirement != null && requirement.ComponentId.HasValue)
						{
							componentIds.Add(requirement.ComponentId.Value);
						}
					}
					catch (ArtifactNotExistsException)
					{
						//Do nothing
					}
				}

				//Incidents require a description, tasks do not, os use a dummy one if none specified
				string description = GlobalResources.General.Incident_IncidentCreatedFromTask;
				if (!String.IsNullOrEmpty(task.Description))
				{
					description = task.Description;
				}

				//Now we need to create the new incident (using the default type and status)
				int incidentId = Insert(
					projectId,
					incidentProrityId,
					null,
					task.CreatorId,
					task.OwnerId,
					null,
					task.Name,
					description,
					task.ReleaseId,
					task.ReleaseId,
					null,
					null,
					null,
					DateTime.UtcNow,
					task.StartDate,
					null,
					task.EstimatedEffort,
					task.ActualEffort,
					task.RemainingEffort,
					null,
					componentIds,
					userId
					);

				//Next we need to create a link between the two
				ArtifactLinkManager artifactLink = new ArtifactLinkManager();
				artifactLink.Insert(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, userId, GlobalResources.General.Incident_IncidentCreatedFromTask, DateTime.UtcNow);

				//If the task had a requirement, lets also link that to the new incident
				if (task.RequirementId.HasValue)
				{
					artifactLink.Insert(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, task.RequirementId.Value, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, userId, GlobalResources.General.Incident_IncidentCreatedFromTask, DateTime.UtcNow);
				}

				//Next copy across the comments
				DiscussionManager discussionManager = new DiscussionManager();
				IEnumerable<IDiscussion> taskComments = discussionManager.Retrieve(taskId, Artifact.ArtifactTypeEnum.Task);
				if (taskComments != null)
				{
					foreach (IDiscussion taskComment in taskComments)
					{
						discussionManager.Insert(taskComment.CreatorId, incidentId, Artifact.ArtifactTypeEnum.Incident, taskComment.Text, projectId, taskComment.IsPermanent, false);
					}
				}

				//Now associate any attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Copy(projectId, Artifact.ArtifactTypeEnum.Task, taskId, Artifact.ArtifactTypeEnum.Incident, incidentId);

				//Finally we need to copy over any List/Multilist custom properties that are the same between the two artifact types
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				customPropertyManager.ArtifactCustomProperty_CopyListValues(projectId, projectTemplateId, userId, Artifact.ArtifactTypeEnum.Task, taskId, Artifact.ArtifactTypeEnum.Incident, incidentId);

				//Finally, mark the current task to closed
				task.StartTracking();
				task.TaskStatusId = (int)Task.TaskStatusEnum.Completed;
				taskManager.Update(task, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentId;
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

		/// <summary>Inserts a new incident into the system</summary>
		/// <param name="description">The long description of the incident</param>
		/// <param name="priorityId">The priority/importance of the incident</param>
		/// <param name="severityId">The severity of the incident</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="name">The name of the incident</param>
		/// <param name="incidentTypeId">The type of the incident (bug, issue, etc.)</param>
		/// <param name="incidentStatusId">The initial status of the incident (new, open, etc.)</param>
		/// <param name="openerId">Who opened/created the incident</param>
		/// <param name="ownerId">Who has been assigned to own its resolution</param>
		/// <param name="resolvedReleaseId">The release of the project the incident resolved in (Optional)</param>
		/// <param name="verifiedReleaseId">The release of the project the incident verified in (Optional)</param>
		/// <param name="testRunStepIds">An optional list of test run steps that failed, causing the incident to be reported.</param>
		/// <param name="detectedReleaseId">The release of the project the incident detected in (Optional)</param>
		/// <param name="closedDate">The date closed (Optional) - pass DateTimeNull if not set</param>
		/// <param name="startDate">The date work on the incident was started (Optional)</param>
		/// <param name="estimatedEffort">The estimated effort to resolve the incident (Optional)</param>
		/// <param name="actualEffort">The actual effort taken so far to resolve the incident (Optional)</param>
		/// <param name="remainingEffort">The effort remaining to resolve the incident (optional)</param>
		/// <param name="creationDate">The creation date</param>
		/// <param name="creatorId">Optional. The user who created the incident, for history logging. If not specified, uses the OpenerId.</param>
		/// <param name="buildId">The id of the build to associate the incident to</param>
		/// <param name="componentIds">List of components to add it to</param>
		/// <param name="logHistory">Should we log a new incident history event</param>
		/// <returns>The ID of the newly created incident</returns>
		public int Insert(int projectId, int? priorityId, int? severityId, int openerId, int? ownerId, List<int> testRunStepIds, string name, string description, int? detectedReleaseId, int? resolvedReleaseId, int? verifiedReleaseId, int? incidentTypeId, int? incidentStatusId, DateTime creationDate, DateTime? startDate, DateTime? closedDate, int? estimatedEffort, int? actualEffort, int? remainingEffort, int? buildId, List<int> componentIds, int? creatorId = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentId;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//If we're passed null for incident status, we need to use the default one for this project template
					if (!incidentStatusId.HasValue || !incidentTypeId.HasValue)
					{
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						if (!incidentStatusId.HasValue)
							incidentStatusId = IncidentStatus_RetrieveDefault(projectTemplateId).IncidentStatusId;

						//If we're passed null for incident type, we need to use the default one for this project
						if (!incidentTypeId.HasValue)
							incidentTypeId = GetDefaultIncidentType(projectTemplateId);
					}

					//If we have a estimated effort and a null value for remaining, set the remaining to the estimate
					if (estimatedEffort.HasValue && estimatedEffort.Value > 0 && !remainingEffort.HasValue)
						remainingEffort = estimatedEffort;

					//We need to calculate the %complete and projected effort based on the status and supplied effort
					int completionPercent = 0;
					int? projectedEffort = null;
					CalculateCompletion(estimatedEffort, actualEffort, remainingEffort, out completionPercent, out projectedEffort);

					//Serialize the component IDs if provided
					string componentIdList = null;
					if (componentIds != null && componentIds.Count > 0)
					{
						componentIdList = componentIds.ToDatabaseSerialization();
					}



					Incident incident = new Incident();
					incident.ProjectId = projectId;
					incident.PriorityId = priorityId;
					incident.SeverityId = severityId;
					incident.IncidentStatusId = incidentStatusId.Value;
					incident.IncidentTypeId = incidentTypeId.Value;
					incident.ComponentIds = componentIdList;
					incident.OpenerId = openerId;
					incident.OwnerId = ownerId;
					incident.DetectedReleaseId = detectedReleaseId;
					incident.ResolvedReleaseId = resolvedReleaseId;
					incident.VerifiedReleaseId = verifiedReleaseId;
					incident.BuildId = buildId;
					incident.Name = name.MaxLength(255);
					incident.Description = description;
					incident.CreationDate = creationDate;
					incident.StartDate = startDate;
					incident.ClosedDate = closedDate;
					incident.CompletionPercent = completionPercent;
					incident.EstimatedEffort = estimatedEffort;
					incident.ActualEffort = actualEffort;
					incident.ProjectedEffort = projectedEffort;
					incident.RemainingEffort = remainingEffort;
					incident.LastUpdateDate = System.DateTime.UtcNow;
					incident.ConcurrencyDate = DateTime.UtcNow;
					incident.IsAttachments = false;
					incident.IsDeleted = false;

					//See if we need to link to a test run step
					if (testRunStepIds != null && testRunStepIds.Count > 0)
					{
						foreach (int testRunStepId in testRunStepIds)
						{
							TestRunStep testRunStep = new TestRunStep();
							testRunStep.TestRunStepId = testRunStepId;
							context.TestRunSteps.Attach(testRunStep);
							incident.TestRunSteps.Add(testRunStep);
						}
					}

					//Persist the incident and get the incident id
					context.Incidents.AddObject(incident);
					context.SaveChanges();
					incidentId = incident.IncidentId;
				}

				//Add a history record for the inserted incident.
				if (logHistory)
				{
					new HistoryManager().LogCreation(projectId, ((creatorId.HasValue) ? creatorId.Value : openerId), DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, DateTime.UtcNow);
				}

				//Now refresh the release's effort information if appropriate
				if (resolvedReleaseId.HasValue)
				{
					ReleaseManager releaseManager = new ReleaseManager();
					releaseManager.RefreshProgressEffortTestStatus(projectId, resolvedReleaseId.Value);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Counts the number of incidents associated with a specific test run
		/// </summary>
		/// <param name="testRunId">The id of the test run</param>
		/// <returns>The incident count</returns>
		public int CountByTestRunId(int testRunId)
		{
			const string METHOD_NAME = "CountByTestRunId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int incidentCount = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from i in context.TestRunIncidentsView
								where i.TestRunId == testRunId
								select i;

					//Extract the count
					incidentCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Counts the number of incidents associated with a specific test set
		/// </summary>
		/// <param name="testSetId">The id of the test set</param>
		/// <returns>The incident count</returns>
		public int CountByTestSetId(int testSetId)
		{
			const string METHOD_NAME = "CountByTestSetId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from i in context.TestSetIncidentsView
								where i.TestSetId == testSetId
								select i;

					//Extract the count
					incidentCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the incidents in the system</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">True if you want the count to return deleted incidents as well.</param>
		/// <returns>The total number of Incidents</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.IncidentsView
								where (!i.IsDeleted || includeDeleted) && i.ProjectId == projectId
								select i;

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleIncidentSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
						Expression<Func<IncidentView, bool>> filterClause = CreateFilterExpression<IncidentView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Incident, filters, utcOffset, null, HandleIncidentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<IncidentView>)query.Where(filterClause);
						}
					}

					//Get the count
					incidentCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a summary count of open incidents by either priority or severity</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="useResolvedRelease">Should we use resolved release or detected release</param>
		/// <param name="releaseId">The release we're interested in (optional)</param>
		/// <param name="useSeverity">Should we use severity or priority</param>
		/// <returns>The list of incident counts by priority/severity</returns>
		public List<IncidentOpenCountByPrioritySeverity> RetrieveOpenCountByPrioritySeverity(int projectId, int? releaseId, bool useSeverity, bool useResolvedRelease)
		{
			const string METHOD_NAME = "RetrieveOpenCountByPrioritySeverity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentOpenCountByPrioritySeverity> incidentCounts;

				//Call the stored procedure to get the counts
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					incidentCounts = context.Incident_RetrieveOpenCountByPrioritySeverity(projectId, releaseId, useSeverity, useResolvedRelease).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCounts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents not assigned to a release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveBacklogByComponentId(int projectId, int? componentId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveBacklogByComponentId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									!i.DetectedReleaseId.HasValue &&
									!i.IsDeleted
								select i;

					//Add the component filter
					if (componentId.HasValue)
					{
						//We need to use both the padded and non-padded database serialization options
						string searchString1 = separator + componentId.Value.ToDatabaseSerialization() + separator;
						string searchString2 = separator + componentId.Value + separator;
						query = query.Where(i => (separator + i.ComponentIds + separator).Contains(searchString2) || (separator + i.ComponentIds + separator).Contains(searchString1));
					}
					else
					{
						query = query.Where(i => i.ComponentIds == null || i.ComponentIds == "");
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents assigned to any release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveAllReleasesByComponentId(int projectId, int? componentId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveAllReleasesByComponentId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.DetectedReleaseId.HasValue &&
									!i.IsDeleted
								select i;

					//Add the component filter
					if (componentId.HasValue)
					{
						//We need to use both the padded and non-padded database serialization options
						string searchString1 = separator + componentId.Value.ToDatabaseSerialization() + separator;
						string searchString2 = separator + componentId.Value + separator;
						query = query.Where(i => (separator + i.ComponentIds + separator).Contains(searchString2) || (separator + i.ComponentIds + separator).Contains(searchString1));
					}
					else
					{
						query = query.Where(i => i.ComponentIds == null || i.ComponentIds == "");
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents assigned to a specific release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <param name="releaseId">The id of the release/iteration</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveForReleaseByComponentId(int projectId, int? componentId, int releaseId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveForReleaseByComponentId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									!i.IsDeleted
								select i;

					//Add the component filter
					if (componentId.HasValue)
					{
						//We need to use both the padded and non-padded database serialization options
						string searchString1 = separator + componentId.Value.ToDatabaseSerialization() + separator;
						string searchString2 = separator + componentId.Value + separator;
						query = query.Where(i => (separator + i.ComponentIds + separator).Contains(searchString2) || (separator + i.ComponentIds + separator).Contains(searchString1));
					}
					else
					{
						query = query.Where(i => i.ComponentIds == null || i.ComponentIds == "");
					}
					if (releaseId != -2)
					{
						//Add the resolved release/iteration filter (include child iterations of a release)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);
						query = query.Where(i => releaseAndIterations.Contains(i.DetectedReleaseId.Value));
					}
					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents owner by a user and/or release/iteration
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <param name="releaseId">The id of the current release/iteration (null = no release/iteration)</param>
		/// <param name="considerChildIterations">Should we consider child iterations, only used when releaseId specified</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveBacklogByUserId(int projectId, int? releaseId, int? userId, bool considerChildIterations, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveBacklogByUserId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.DetectedReleaseId.HasValue &&
									!i.IsDeleted
								select i;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(r => r.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(r => !r.OwnerId.HasValue);
					}

					//Add the resolved release/iteration filter
					if (releaseId.HasValue && releaseId!=-2)
					{
						//Get the child iterations if required
						if (considerChildIterations)
						{
							List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
							query = query.Where(r => releaseAndIterations.Contains(r.DetectedReleaseId.Value));
						}
						else
						{
							query = query.Where(r => r.DetectedReleaseId.Value == releaseId.Value);
						}
					}
					//else
					//{
					//	query = query.Where(r => !r.DetectedReleaseId.HasValue);
					//}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of incidents by priority for use in the Incident Board
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the resolved release (null = any/no release, but only open status)</param>
		/// <param name="priorityId">The id of the incident priority (null == no priority)</param>
		/// <param name="startIndex">The start index</param>
		/// <param name="numRows">The number of rows</param>
		/// <returns>The incidents</returns>
		public List<IncidentView> Incident_RetrieveByPriorityId(int projectId, int? releaseId, int? priorityId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveByPriorityId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query for all undeleted incidents
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentsView
								where
									t.ProjectId == projectId &&
									!t.IsDeleted
								select t;

					//Filter by resolved release if necessary
					if (releaseId.HasValue)
					{
						//Need to be part of the release/child iteration (all statuses)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(t => releaseAndIterations.Contains(t.DetectedReleaseId.Value));
					}
					//else
					//{
					//	//Get only open incidents (otherwise there will be too many)
					//	query = query.Where(t => t.IncidentStatusIsOpenStatus);
					//}

					//Add the priority filter
					if (priorityId.HasValue)
					{
						query = query.Where(t => t.PriorityId.Value == priorityId.Value);
					}
					else
					{
						query = query.Where(t => !t.PriorityId.HasValue);
					}

					//Order by rank then priority, ID
					query = query.OrderByDescending(r => r.Rank).ThenBy(t => t.PriorityName).ThenBy(t => t.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<IncidentView> Incident_RetrieveByAllPriorityId(int projectId, int? releaseId, int? priorityId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveByAllPriorityId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query for all undeleted incidents
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentsView
								where
									t.ProjectId == projectId &&
									t.DetectedReleaseId.HasValue &&
									!t.IsDeleted
								select t;

					//Filter by resolved release if necessary
					if (releaseId.HasValue)
					{
						//Need to be part of the release/child iteration (all statuses)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(t => releaseAndIterations.Contains(t.DetectedReleaseId.Value));
					}
					//else
					//{
					//	//Get only open incidents (otherwise there will be too many)
					//	query = query.Where(t => t.IncidentStatusIsOpenStatus);
					//}

					//Add the priority filter
					if (priorityId.HasValue)
					{
						query = query.Where(t => t.PriorityId.Value == priorityId.Value);
					}
					else
					{
						query = query.Where(t => !t.PriorityId.HasValue);
					}

					//Order by rank then priority, ID
					query = query.OrderByDescending(r => r.Rank).ThenBy(t => t.PriorityName).ThenBy(t => t.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		/// <summary>
		/// Retrieves a list of incidents by priority for use in the Incident Board
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the resolved release (null = any/no release, but only open status)</param>
		/// <param name="priorityId">The id of the incident priority (null == no priority)</param>
		/// <param name="startIndex">The start index</param>
		/// <param name="numRows">The number of rows</param>
		/// <returns>The incidents</returns>
		public List<IncidentView> Incident_RetrieveByBacklogPriorityId(int projectId, int? releaseId, int? priorityId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveByBacklogPriorityId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query for all undeleted incidents
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentsView
								where
									t.ProjectId == projectId &&
									!t.DetectedReleaseId.HasValue &&
									!t.IsDeleted
								select t;

					//Filter by resolved release if necessary
					if (releaseId.HasValue)
					{
						//Need to be part of the release/child iteration (all statuses)
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(t => releaseAndIterations.Contains(t.ResolvedReleaseId.Value));
					}
					//else
					//{
					//	//Get only open incidents (otherwise there will be too many)
					//	query = query.Where(t => t.IncidentStatusIsOpenStatus);
					//}

					//Add the priority filter
					if (priorityId.HasValue)
					{
						query = query.Where(t => t.PriorityId.Value == priorityId.Value);
					}
					else
					{
						query = query.Where(t => !t.PriorityId.HasValue);
					}

					//Order by rank then priority, ID
					query = query.OrderByDescending(r => r.Rank).ThenBy(t => t.PriorityName).ThenBy(t => t.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves a list of incidents by status for use in the Incident Board
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the resolved release (null or -1 == no release, -2 == all releases)</param>
		/// <param name="statusId">The id of the incident status (null = not assigned to release)</param>
		/// <param name="startIndex">The start index</param>
		/// <param name="numRows">The number of rows</param>
		/// <returns>The incidents</returns>
		public List<IncidentView> Incident_RetrieveByBacklogStatusId(int projectId, int? releaseId, int? statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveByBacklogStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentsView
								where
									t.ProjectId == projectId &&
									!t.DetectedReleaseId.HasValue &&
									!t.IsDeleted
								select t;

					if (releaseId.HasValue)
					{
						//Add the status filter
						if (statusId.HasValue)
						{
							//Check for the 'all releases' (-2) case
							if (releaseId != -2)
							{
								//Need to be in the status and part of the release/child iteration
								List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
								query = query.Where(t => releaseAndIterations.Contains(t.ResolvedReleaseId.Value));
							}
							query = query.Where(t => t.IncidentStatusId == statusId.Value);
						}
						else
						{
							//If no status, then only show items that are not part of the release and are in an open status
							query = query.Where(t => !t.ResolvedReleaseId.HasValue && t.IncidentStatusIsOpenStatus);
						}
					}
					else
					{
						//Only get items that have no release set (product backlog grouped by status)
						query = query.Where(t => !t.ResolvedReleaseId.HasValue);
						if (statusId.HasValue)
						{
							query = query.Where(t => t.IncidentStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<IncidentView>();
						}
					}

					//Order by rank then priority, ID
					query = query.OrderByDescending(r => r.Rank).ThenBy(t => t.PriorityName).ThenBy(t => t.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of incidents by status for use in the Incident Board
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the resolved release (null or -1 == no release, -2 == all releases)</param>
		/// <param name="statusId">The id of the incident status (null = not assigned to release)</param>
		/// <param name="startIndex">The start index</param>
		/// <param name="numRows">The number of rows</param>
		/// <returns>The incidents</returns>
		public List<IncidentView> Incident_RetrieveByStatusId(int projectId, int? releaseId, int? statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentsView
								where
									t.ProjectId == projectId &&
									!t.IsDeleted
								select t;

					if (releaseId.HasValue)
					{
						//Add the status filter
						if (statusId.HasValue)
						{
							//Check for the 'all releases' (-2) case
							if (releaseId != -2)
							{
								//Need to be in the status and part of the release/child iteration
								List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
								query = query.Where(t => releaseAndIterations.Contains(t.DetectedReleaseId.Value));
							}
							
							query = query.Where(t => t.IncidentStatusId == statusId.Value);
						}
						else
						{
							//If no status, then only show items that are not part of the release and are in an open status
							query = query.Where(t => !t.DetectedReleaseId.HasValue && t.IncidentStatusIsOpenStatus);
						}
					}
					else
					{
						//Only get items that have no release set (product backlog grouped by status)
						//query = query.Where(t => !t.DetectedReleaseId.HasValue);
						if (statusId.HasValue)
						{
							query = query.Where(t => t.IncidentStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<IncidentView>();
						}
					}

					//Order by rank then priority, ID
					query = query.OrderByDescending(r => r.Rank).ThenBy(t => t.PriorityName).ThenBy(t => t.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents owner by a user for all releases in the project
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveAllReleasesByUserId(int projectId, int? userId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveAllReleasesByUserId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.ResolvedReleaseId.HasValue &&
									!i.IsDeleted
								select i;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(r => r.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(r => !r.OwnerId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents for a specific resolved release
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (or null for unassigned to a release)</param>
		/// <param name="considerChildIterations">Do we consider child iterations</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// 2) Includes the child iterations of the release
		/// </remarks>
		public List<IncidentView> Incident_RetrieveBacklogByReleaseId(int projectId, int? releaseId, bool considerChildIterations, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveBacklogByReleaseId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where
									i.IncidentStatusIsOpenStatus &&
									
									i.ProjectId == projectId &&
									!i.IsDeleted
								select i;

					//Add the release filter
					if (releaseId.HasValue)
					{
						//Get the child iterations if required
						if (considerChildIterations)
						{
							List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
							query = query.Where(i => releaseAndIterations.Contains(i.DetectedReleaseId.Value));
						}
						else
						{
							query = query.Where(i => i.DetectedReleaseId.Value == releaseId.Value);
						}
					}
					else
					{
						query = query.Where(i => !i.DetectedReleaseId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents not assigned to a release/iteration for a specific requirement
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveBacklogByRequirementId(int projectId, int? requirementId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveBacklogByRequirementId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we have a requirement id specified or not
					IQueryable<IncidentView> query;
					if (requirementId.HasValue)
					{
						query = from i in context.IncidentsView
								join ri in context.RequirementIncidentsView on i.IncidentId equals ri.IncidentId
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									!i.DetectedReleaseId.HasValue &&
									!i.IsDeleted &&
									ri.RequirementId == requirementId.Value
								select i;
					}
					else
					{
						//Find all the incidents that are not linked to any requirement
						var subQuery = from ri in context.RequirementIncidentsView
									   select ri.IncidentId;

						query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									!i.DetectedReleaseId.HasValue &&
									!i.IsDeleted &&
									!subQuery.Contains(i.IncidentId)
								select i;
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents assigned to any release/iteration for a specific requirement
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveAllReleasesByRequirementId(int projectId, int? requirementId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveAllReleasesByRequirementId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we have a requirement id specified or not
					IQueryable<IncidentView> query;
					if (requirementId.HasValue)
					{
						query = from i in context.IncidentsView
								join ri in context.RequirementIncidentsView on i.IncidentId equals ri.IncidentId
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.DetectedReleaseId.HasValue &&
									!i.IsDeleted &&
									ri.RequirementId == requirementId.Value
								select i;
					}
					else
					{
						//Find all the incidents that are not linked to any requirement
						var subQuery = from ri in context.RequirementIncidentsView
									   select ri.IncidentId;

						query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.DetectedReleaseId.HasValue &&
									!i.IsDeleted &&
									!subQuery.Contains(i.IncidentId)
								select i;
					}

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of incidents assigned to a specific release/iteration for a specific requirement
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <returns>The list of incidents</returns>
		/// <remarks>
		/// 1) Does not include includes in a closed statuses
		/// </remarks>
		public List<IncidentView> Incident_RetrieveForReleaseByRequirementId(int projectId, int? requirementId, int releaseId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Incident_RetrieveForReleaseByRequirementId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create the LINQ query
				string separator = DatabaseExtensions.FORMAT_LIST_SEPARATOR.ToString();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we have a requirement id specified or not
					IQueryable<IncidentView> query;
					if (requirementId.HasValue)
					{
						query = from i in context.IncidentsView
								join ri in context.RequirementIncidentsView on i.IncidentId equals ri.IncidentId
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									!i.IsDeleted &&
									i.DetectedReleaseId.HasValue &&
									ri.RequirementId == requirementId.Value
								select i;
					}
					else
					{
						//Find all the incidents that are not linked to any requirement
						var subQuery = from ri in context.RequirementIncidentsView
									   select ri.IncidentId;

						query = from i in context.IncidentsView
								where
									//i.IncidentStatusIsOpenStatus &&
									i.ProjectId == projectId &&
									i.DetectedReleaseId.HasValue &&
									!i.IsDeleted &&
									!subQuery.Contains(i.IncidentId)
								select i;
					}

					//Add the resolved release/iteration filter (include child iterations of a release)
					
					List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);
					query = query.Where(i => releaseAndIterations.Contains(i.DetectedReleaseId.Value));

					//Order by rank then priority
					query = query.OrderByDescending(i => i.Rank).ThenBy(i => i.PriorityName).ThenBy(i => i.IncidentId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the rank(s) of the specific incidents (shuffling others as necessary)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="incidentIds">The id of the incidents affected</param>
		/// <param name="existingRank">The rank we're inserting ahead of</param>
		public void Incident_UpdateRanks(int projectId, List<int> incidentIds, int? existingRank)
		{
			const string METHOD_NAME = "Incident_UpdateRanks";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (incidentIds != null && incidentIds.Count > 0)
				{
					//Convert the list into a CSV string
					string incidentIdList = incidentIds.ToDatabaseSerialization();

					//Call the stored procedure to update the ranks
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.Incident_UpdateRanks(projectId, incidentIdList, existingRank);
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
		/// Creates a new blank incident record
		/// </summary>
		/// <param name="openerId">The id of the current user who will be initially set as the opener</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The new blank incident entity with a single datarow</returns>
		public IncidentView Incident_New(int projectId, int openerId)
		{
			const string METHOD_NAME = "Incident_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the new entity
				IncidentView incident = new IncidentView();

				//Find the default status for the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
				IncidentStatus defaultStatus = IncidentStatus_RetrieveDefault(projectTemplateId);

				//Populate the new incident
				incident.ProjectId = projectId;
				incident.OpenerId = openerId;
				incident.IncidentStatusId = defaultStatus.IncidentStatusId;
				incident.IncidentStatusName = defaultStatus.Name;
				incident.IncidentTypeId = GetDefaultIncidentType(projectTemplateId);
				incident.CreationDate = incident.LastUpdateDate = incident.ConcurrencyDate = DateTime.UtcNow;
				incident.Name = "";
				incident.Description = "";
				incident.CompletionPercent = 0;
				incident.IsAttachments = false;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incident;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
		}

		/// <summary>Retrieves the list of incidents for a given test case</summary>
		/// <param name="testCaseId">The ID of the test case that the incident is associated with</param>
		/// <param name="openOnly">Do we only want those that are in an 'open' status</param>
		/// <returns>Incident dataset</returns>
		/// <remarks>1) The incidents are sorted by priority then type
		/// 2) We also merge the results due to static links between the test steps and incidents (note that test steps > incidents only support forward linking)
		/// 3) We can't use the generic method since we have to join on test run step and test run</remarks>
		public List<IncidentView> RetrieveByTestCaseId(int testCaseId, bool openOnly)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveByTestCaseId()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Call stored procedure for retrieving the incidents
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					incidents = context.Incident_RetrieveByTestCase(testCaseId, openOnly).ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open incidents (new, assigned or open) for a given owner irrespective of the project.</summary>
		/// <param name="ownerId">The ID of the person owning the incidents (pass null to retrieve all unassigned)</param>
		/// <param name="releaseId">The id of the release (null for all)</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Incident list</returns>
		/// <remarks>
		/// 1) The incidents are sorted by priority then type. Only displays for active projects
		/// 2) If you filter by release, it does NOT include items for the child iterations
		/// </remarks>
		/// <param name="projectId">The id of the project, or pass null for all</param>
		public List<IncidentView> RetrieveOpenByOwnerId(int? ownerId, int? projectId, int? releaseId, int numberRows = 500)
		{
			const string METHOD_NAME = "RetrieveOpenByOwnerId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted && i.ProjectIsActive && i.IncidentStatusIsOpenStatus
								select i;

					//Add the project filter if necessary
					if (projectId.HasValue)
					{
						query = query.Where(i => i.ProjectId == projectId.Value);

						//Add the release filter if necessary
						if (releaseId.HasValue)
						{
							query = query.Where(i => i.ResolvedReleaseId == releaseId.Value);
						}
					}

					//Add the Owner filter
					if (ownerId.HasValue)
					{
						query = query.Where(i => i.OwnerId == ownerId.Value);
					}
					else
					{
						query = query.Where(i => !i.OwnerId.HasValue);
					}

					//Order by priority then type
					query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentTypeName).ThenBy(i => i.IncidentId);

					//Execute the query
					incidents = query.Take(numberRows).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open incidents (new, assigned or open) for a given opener irrespective of the project.</summary>
		/// <param name="openerId">The ID of the person who detected/opened the incidents</param>
		/// <param name="projectId">The id of the project, or null for all</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Incident dataset</returns>
		/// <remarks>The incidents are sorted by last updated date descending. Only displays for active projects</remarks>
		public List<IncidentView> RetrieveOpenByOpenerId(int openerId, int? projectId, int numberRows = 500)
		{
			const string METHOD_NAME = "RetrieveOpenByOpenerId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted && i.ProjectIsActive && i.OpenerId == openerId && i.IncidentStatusIsOpenStatus
								select i;

					//Add the project filter if necessary
					if (projectId.HasValue)
					{
						query = query.Where(i => i.ProjectId == projectId.Value);
					}

					//Order by last updated date then id
					query = query.OrderByDescending(i => i.LastUpdateDate).ThenBy(i => i.IncidentId);

					//Execute the query
					incidents = query.Take(numberRows).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open issue-type incidents for a given project/release</summary>
		/// <param name="projectId">The ID of the project we're interested in</param>
		/// <param name="releaseId">The ID of the detected release we're interested in (null for all)</param>
		/// <param name="incidentCount">How many issues to return</param>
		/// <param name="useSeverity">Should we order by severity instead of priority</param>
		/// <returns>Incident list</returns>
		/// <remarks>The incidents are sorted by priority name ascending</remarks>
		public List<IncidentView> RetrieveOpenIssues(int projectId, int? releaseId, int incidentCount, bool useSeverity)
		{
			const string METHOD_NAME = "RetrieveOpenIssues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted &&
										i.ProjectId == projectId &&
										i.IncidentStatusIsOpenStatus &&
										i.IncidentTypeIsIssue
								select i;

					//Add the detected release filter if necessary
					if (releaseId.HasValue)
					{
						//If we have a release containing iterations, we actually need to get the risks of all the iterations
						//as well as the risks that belong to the release itself
						List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(i => releaseIds.Contains((int)i.DetectedReleaseId));
					}

					//Order by priority/severity then id
					if (useSeverity)
					{
						query = query.OrderBy(i => i.SeverityName).ThenBy(i => i.IncidentId);
					}
					else
					{
						query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentId);
					}

					//Execute the query
					if (incidentCount > 0)
					{
						incidents = query.Take(incidentCount).ToList();
					}
					else
					{
						return new List<IncidentView>();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open issue-type incidents for a given project group</summary>
		/// <param name="projectGroupId">The ID of the project group we're interested in</param>
		/// <param name="incidentCount">How many issues to return</param>
		/// <param name="useSeverity">Should we order by severity instead of priority</param>
		/// <returns>Incident list</returns>
		/// <remarks>The incidents are sorted by priority name ascending</remarks>
		public List<IncidentView> RetrieveOpenIssues(int projectGroupId, int incidentCount, bool useSeverity)
		{
			const string METHOD_NAME = "RetrieveOpenIssues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted &&
										i.ProjectIsActive &&
										i.IncidentStatusIsOpenStatus &&
										i.IncidentTypeIsIssue &&
										i.ProjectGroupId == projectGroupId
								select i;

					//Order by priority/severity then id
					if (useSeverity)
					{
						query = query.OrderBy(i => i.SeverityName).ThenBy(i => i.IncidentId);
					}
					else
					{
						query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentId);
					}

					//Execute the query
					if (incidentCount > 0)
					{
						incidents = query.Take(incidentCount).ToList();
					}
					else
					{
						return new List<IncidentView>();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open risk-type incidents for a given project/release</summary>
		/// <param name="projectId">The ID of the project we're interested in</param>
		/// <param name="releaseId">The ID of the release we're interested in (null for all)</param>
		/// <param name="incidentCount">How many risks to return</param>
		/// <param name="useSeverity">Should we order by severity instead of priority</param>
		/// <returns>Incident list</returns>
		/// <remarks>The incidents are sorted by priority name ascending</remarks>
		public List<IncidentView> RetrieveOpenRisks(int projectId, int? releaseId, int incidentCount, bool useSeverity)
		{
			const string METHOD_NAME = "RetrieveOpenRisks";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted &&
										i.ProjectId == projectId &&
										i.IncidentStatusIsOpenStatus &&
										i.IncidentTypeIsRisk
								select i;

					//Add the detected release filter if necessary
					if (releaseId.HasValue)
					{
						//If we have a release containing iterations, we actually need to get the risks of all the iterations
						//as well as the risks that belong to the release itself
						List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
						query = query.Where(i => releaseIds.Contains((int)i.DetectedReleaseId));
					}

					//Order by priority/severity then id
					if (useSeverity)
					{
						query = query.OrderBy(i => i.SeverityName).ThenBy(i => i.IncidentId);
					}
					else
					{
						query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentId);
					}

					//Execute the query
					if (incidentCount > 0)
					{
						incidents = query.Take(incidentCount).ToList();
					}
					else
					{
						return new List<IncidentView>();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open risk-type incidents for a given project group</summary>
		/// <param name="projectGroupId">The ID of the project group we're interested in</param>
		/// <param name="incidentCount">How many risks to return</param>
		/// <param name="useSeverity">Should we order by severity instead of priority</param>
		/// <returns>Incident list</returns>
		/// <remarks>The incidents are sorted by priority name ascending</remarks>
		public List<IncidentView> RetrieveOpenRisks(int projectGroupId, int incidentCount, bool useSeverity)
		{
			const string METHOD_NAME = "RetrieveOpenRisks";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the incident records
					var query = from i in context.IncidentsView
								where !i.IsDeleted &&
										i.ProjectIsActive &&
										i.IncidentStatusIsOpenStatus &&
										i.IncidentTypeIsRisk &&
										i.ProjectGroupId == projectGroupId
								select i;

					//Order by priority/severity then id
					if (useSeverity)
					{
						query = query.OrderBy(i => i.SeverityName).ThenBy(i => i.IncidentId);
					}
					else
					{
						query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentId);
					}

					//Execute the query
					if (incidentCount > 0)
					{
						incidents = query.Take(incidentCount).ToList();
					}
					else
					{
						return new List<IncidentView>();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all defects in the system along with associated meta-data</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="includeDeleted">Whether to include deleted items in the list or not.</param>
		/// <returns>Incident List</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		public List<IncidentView> Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.IncidentsView
								where (!i.IsDeleted || includeDeleted) && i.ProjectId == projectId
								select i;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by creation date descending
						query = query.OrderByDescending(i => i.CreationDate).ThenBy(i => i.IncidentId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "IncidentId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleIncidentSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
						Expression<Func<IncidentView, bool>> filterClause = CreateFilterExpression<IncidentView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Incident, filters, utcOffset, null, HandleIncidentSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<IncidentView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						return new List<IncidentView>();
					}

					//Execute the query
					incidents = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all the incidents that will be resolved in a particular release/iteration</summary>
		/// <param name="releaseId">
		/// The ID of the release/iteration we want to retrieve the incidents for
		/// (passing null gets all incidents in the project not associated with a release/iteration
		/// </param>
		/// <returns>An incident list</returns>
		/// <remarks>Unlike the general Retrieve method this doesn't include child iterations for a release</remarks>
		public List<IncidentView> RetrieveByReleaseId(int projectId, int? releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				//Create custom LINQ WHERE clause for retrieving the incidents for the release/iteration
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where i.ProjectId == projectId && (includeDeleted || !i.IsDeleted)
								select i;

					//See if we have a release specified
					if (releaseId.HasValue)
					{
						//Filter by resolved release
						query = query.Where(i => i.ResolvedReleaseId == releaseId.Value);

						//Order by start date then closed date
						query = query.OrderBy(i => i.StartDate).ThenBy(i => i.ClosedDate).ThenBy(i => i.IncidentId);
					}
					else
					{
						//Only include open incidents for this case and order by priority name
						query = query.Where(i => !i.ResolvedReleaseId.HasValue && i.IncidentStatusIsOpenStatus);
						query = query.OrderBy(i => i.PriorityName).ThenBy(i => i.IncidentId);
					}

					incidents = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single incident in the system that has a certain ID</summary>
		/// <param name="incidentId">The ID of the incident to be returned</param>
		/// <returns>Incident dataset</returns>
		/// <param name="includeDeleted">Should we include deleted incidents</param>
		/// <param name="includeResolutions">Should we include the resolutions</param>
		/// <param name="includeTestRunSteps">should we include test run steps</param>
		/// <remarks>Also retrieves associated resolutions, but they are NOT sorted</remarks>
		public Incident RetrieveById(int incidentId, bool includeResolutions, bool includeDeleted = false, bool includeTestRunSteps = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Incident incident;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we want to include resolutions
					ObjectQuery<Incident> incidentSet = context.Incidents;
					if (includeResolutions)
					{
						incidentSet = (ObjectQuery<Incident>)incidentSet.Include("Resolutions").Include("Resolutions.Creator").Include("Resolutions.Creator.Profile");
					}
					if (includeTestRunSteps)
					{
						incidentSet = (ObjectQuery<Incident>)incidentSet.Include(i => i.TestRunSteps);
					}

					//Create custom LINQ WHERE clause for retrieving the incident by id and execute
					var query = from i in incidentSet
								where i.IncidentId == incidentId && (includeDeleted || !i.IsDeleted)
								select i;

					incident = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will not be expecting null)
				if (incident == null)
				{
					throw new ArtifactNotExistsException("Incident " + incidentId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incident;
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

		/// <summary>Retrieves a single incident view in the system that has a certain ID</summary>
		/// <param name="incidentId">The ID of the incident to be returned</param>
		/// <returns>Incident view</returns>
		/// <param name="includeDeleted">Should we include deleted incidents</param>
		public IncidentView RetrieveById2(int incidentId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentView incident;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create custom LINQ WHERE clause for retrieving the incident by id and execute
					var query = from i in context.IncidentsView
								where i.IncidentId == incidentId && (includeDeleted || !i.IsDeleted)
								select i;

					incident = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will not be expecting null)
				if (incident == null)
				{
					throw new ArtifactNotExistsException("Incident " + incidentId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incident;
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

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns></returns>
		public List<IncidentView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentsView
								where i.IsDeleted && i.ProjectId == projectId
								orderby i.IncidentId
								select i;

					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return new List<IncidentView>();
			}
		}

		/// <summary>
		/// Retrieves the incidents in the system that are associated with a specific test step
		/// (both directly and indirectly through a test run)
		/// </summary>
		/// <param name="testStepId">The ID of the test step that the incidents can be linked to</param>
		/// <returns>Incident dataset</returns>
		/// <remarks>
		/// Includes any incidents linked to previous test runs of the test step
		/// </remarks>
		public List<IncidentView> RetrieveByTestStepId(int testStepId)
		{
			const string METHOD_NAME = "RetrieveByTestStepId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of incidents, joined against the view that links to test steps
					var query = from i in context.IncidentsView
								join ti in context.TestStepIncidentsView on i.IncidentId equals ti.IncidentId
								where !i.IsDeleted && ti.TestStepId == testStepId
								orderby i.CreationDate descending, i.IncidentId
								select i;

					incidents = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the incidents in the system that are associated with a specific Test Run Step ID</summary>
		/// <param name="testRunStepId">The ID of the test run step that the incidents needs to be associated with</param>
		/// <returns>Incident dataset</returns>
		public List<IncidentView> RetrieveByTestRunStepId(int testRunStepId)
		{
			const string METHOD_NAME = "RetrieveByTestRunStepId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of incidents, that are linked to this test run step explicitly
					var query = from i in context.IncidentsView
								join ti in context.TestRunStepIncidentsView on i.IncidentId equals ti.IncidentId
								where
									!i.IsDeleted &&
									ti.TestRunStepId == testRunStepId
								orderby i.CreationDate descending, i.IncidentId
								select i;

					incidents = query.ToList();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Marks an Incident as being deleted and no longer available in the system.</summary>
		/// <param name="incidentId">The ID to mark as 'deleted'.</param>
		/// <param name="userId">The userId of the user performing the delete.</param>
		public void MarkAsDeleted(int projectId, int incidentId, int userId)
		{
			const string METHOD_NAME = "MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the incident to see if it's linked to a release
				int? resolvedReleaseId = null;
				bool deletePerformed = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.Incidents
								where i.IncidentId == incidentId && !i.IsDeleted
								select i;

					Incident incident = query.FirstOrDefault();
					if (incident != null)
					{
						//Capture release
						resolvedReleaseId = incident.ResolvedReleaseId;

						//Mark as deleted
						incident.StartTracking();
						incident.LastUpdateDate = DateTime.UtcNow;
						incident.IsDeleted = true;
						context.SaveChanges();
						deletePerformed = true;
					}
				}

				if (deletePerformed)
				{
					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, DateTime.UtcNow);

					//Update the release's effort calculations
					if (resolvedReleaseId.HasValue)
						new Business.ReleaseManager().RefreshProgressEffortTestStatus(projectId, resolvedReleaseId.Value);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Undeletes an incident, making it available to users.</summary>
		/// <param name="incidentId">The incident to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void UnDelete(int incidentId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the incident (needs to be marked as deleted)
					var query = from i in context.Incidents
								where i.IncidentId == incidentId && i.IsDeleted
								select i;

					//Get the incident
					Incident incident = query.FirstOrDefault();
					if (incident != null)
					{
						//See if we need to refresh the release's progress/effort information
						int? releaseId = incident.ResolvedReleaseId;
						int projectId = incident.ProjectId;

						//Mark as undeleted
						incident.StartTracking();
						incident.LastUpdateDate = DateTime.UtcNow;
						incident.IsDeleted = false;

						//Save changes, no history logged, that's done later
						context.SaveChanges();

						//Log the undelete
						if (logHistory)
						{
							//Okay, mark it as being undeleted.
							new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, rollbackId, DateTime.UtcNow);

							//Now refresh the linked release if appropriate
							if (releaseId.HasValue)
								new ReleaseManager().RefreshProgressEffortTestStatus(projectId, releaseId.Value);
						}
					}

				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}
		}

		/// <summary>Deletes an incident in the system that has the specified ID</summary>
		/// <param name="incidentId">The ID of the incident to be deleted</param>
		public void DeleteFromDatabase(int incidentId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the incident to see if it's linked to a release
				Incident incident;
				try
				{
					incident = RetrieveById(incidentId, false, true);
				}
				catch (ArtifactNotExistsException)
				{
					//If it's already deleted, just fail quietly
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				int projectId = incident.ProjectId;

				//First we need to delete any attachments associated with the incident
				Business.AttachmentManager attachment = new Business.AttachmentManager();
				attachment.DeleteByArtifactId(incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident);

				//Next we need to delete any artifact links to/from this incident
				Business.ArtifactLinkManager artifactLink = new Business.ArtifactLinkManager();
				artifactLink.DeleteByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId);

				//Next we need to delete any custom properties associated with the incident
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident);

				//Actually perform the delete on the incident and its resolutions
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Incident_Delete(incidentId);
				}

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, DateTime.UtcNow, incident.Name);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}


		public TST_ARTIFACT_SIGNATURE RetrieveIncidentSignature(int incidentId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveIncidentSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of incident in the project
				TST_ARTIFACT_SIGNATURE incidentSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ArtifactSignatures
								where t.ARTIFACT_ID == incidentId && t.ARTIFACT_TYPE_ID == artifactTypeId
								select t;

					query = query.OrderByDescending(r => r.UPDATE_DATE);

					incidentSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void IncidentSignatureInsert(int projectId, int currentStatusId, Incident incident, string meaning, int? loggedinUserId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				var newReqSignature = new TST_ARTIFACT_SIGNATURE
				{
					STATUS_ID = currentStatusId,
					ARTIFACT_ID = incident.IncidentId,
					ARTIFACT_TYPE_ID = (int)ArtifactTypeEnum.Incident,
					USER_ID = (int)loggedinUserId,
					UPDATE_DATE = DateTime.Now,
					MEANING = meaning,
				};

				context.ArtifactSignatures.AddObject(newReqSignature);

				context.SaveChanges();
				//log history
				new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.IncidentSignature, incident.IncidentId, DateTime.UtcNow);

			}
		}

		public IncidentStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				IncidentStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.IncidentStati
								where t.IncidentStatusId == statusId
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


		/// <summary>Associates incidents with an iteration/release (resolved release field)</summary>
		/// <param name="incidentIds">The incidents being associated</param>
		/// <param name="releaseId">The id of the iteration/release being associated with</param>
		/// <param name="userId">The ID of the user making the change</param>
		/// <remarks>Associating with an iteration also sets the start date to match the iteration</remarks>
		public void AssociateToIteration(List<int> incidentIds, int releaseId, int userId)
		{
			const string METHOD_NAME = "AssociateToIteration";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the release - if we get an exception need to re-throw
				ReleaseView release = null;
				ReleaseManager releaseManager = new ReleaseManager();
				try
				{
					release = releaseManager.RetrieveById2(null, releaseId);
				}
				catch (ArtifactNotExistsException)
				{
					throw new ApplicationException(GlobalResources.Messages.ReleaseManager_ReleaseIterationNotExistsForAssociation);
				}

				int? projectId = null;
				List<int> releaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the incident
					var query = from i in context.Incidents
								where incidentIds.Contains(i.IncidentId) && !i.IsDeleted && (i.ResolvedReleaseId != releaseId || !i.ResolvedReleaseId.HasValue)
								select i;

					//Get the incidents
					List<Incident> incidents = query.ToList();
					foreach (Incident incident in incidents)
					{
						//Store the release and project for use later
						projectId = incident.ProjectId;
						if (incident.ResolvedReleaseId.HasValue && !releaseIds.Contains(incident.ResolvedReleaseId.Value))
						{
							releaseIds.Add(incident.ResolvedReleaseId.Value);
						}

						//Update the release id
						incident.StartTracking();
						incident.ResolvedReleaseId = releaseId;
						incident.StartDate = release.StartDate;
						incident.LastUpdateDate = DateTime.UtcNow;
						incident.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes,(logging history)
						context.SaveChanges(userId, true, true, null);
					}

					if (!releaseIds.Contains(releaseId))
					{
						releaseIds.Add(releaseId);
					}
				}

				//Next refresh the releases that were changed (source and destination)
				if (projectId.HasValue && releaseIds.Count > 0)
				{
					releaseManager.RefreshProgressEffortTestStatus(projectId.Value, releaseIds);
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

		/// <summary>Assigns an incident to a user</summary>
		/// <param name="incidentId">The incident being associated</param>
		/// <param name="ownerId">The id of the user it's being assigned to (or null to deassign)</param>
		/// <param name="changerId">The ID of the user making the change</param>
		public void AssignToUser(int incidentId, Nullable<int> ownerId, int changerId)
		{
			const string METHOD_NAME = "AssignToUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the incident - if it doesn't exist, just ignore
					var query = from i in context.Incidents
								where i.IncidentId == incidentId
								select i;

					//Get the incident, if it doesn't exist, just ignore
					Incident incident = query.FirstOrDefault();
					if (incident != null)
					{
						//Update the incident
						incident.StartTracking();
						incident.OwnerId = ownerId;
						incident.LastUpdateDate = DateTime.UtcNow;
						incident.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes,(logging history)
						//Need to force a detection of changes
						context.DetectChanges();
						context.SaveChanges(changerId, true, true, null);
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

		/// <summary>Updates an Incident that is passed-in</summary>
		/// <param name="incident">The incident to be persisted</param>
		/// <param name="userId">The user making the change</param>
		/// <param name="rollbackId">Whether or not this save is a rollback. Default: NULL</param>
		/// <remarks>1) Sends notifications if incident status has changed
		/// 2) Inserts, updates or deletes any changes to the resolutions list</remarks>
		public void Update(Incident incident, int userId, long? rollbackId = null, bool sendNotification = false)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (incident == null)
			{
				return;
			}

			//Next we need to formally validate the data
			Dictionary<string, string> validationMessages = Validate(incident);
			if (validationMessages.Count > 0)
			{
				//We need to return these messages back as special exceptions
				//We just sent back the first message
				string validationMessage = validationMessages.First().Value;
				throw new DataValidationException(validationMessage);
			}

			try
			{
				int? oldReleaseId = null;
				int? newReleaseId = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					incident.StartTracking();

					//We dynamically set the %completion and projected effort based on the user-supplied
					//effort fields. Any values passed into the dataset will be overwritten
					CalculateCompletion(incident);

					//See if we need to refresh the release after updating
					if (incident.ChangeTracker.OriginalValues.ContainsKey("ResolvedReleaseId"))
					{
						//Check old/new values
						if (incident.ResolvedReleaseId.HasValue)
						{
							newReleaseId = incident.ResolvedReleaseId.Value;
						}
						if (incident.ChangeTracker.OriginalValues["ResolvedReleaseId"] != null && incident.ChangeTracker.OriginalValues["ResolvedReleaseId"] is Int32)
						{
							oldReleaseId = (int)incident.ChangeTracker.OriginalValues["ResolvedReleaseId"];
						}
					}

					//If any of the effort/date/status fields have changed, need to at least update the current requirement/release
					if (incident.ChangeTracker.OriginalValues.ContainsKey("EstimatedEffort") ||
						incident.ChangeTracker.OriginalValues.ContainsKey("ActualEffort") ||
						incident.ChangeTracker.OriginalValues.ContainsKey("RemainingEffort"))
					{
						if (incident.ResolvedReleaseId.HasValue && !newReleaseId.HasValue)
						{
							newReleaseId = incident.ResolvedReleaseId.Value;
						}
					}

					//Update the last-update and concurrency dates
					incident.LastUpdateDate = DateTime.UtcNow;
					incident.ConcurrencyDate = DateTime.UtcNow;

					//Now apply the changes (will auto-insert any provided resolutions)
					context.Incidents.ApplyChanges(incident);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, rollbackId);
				}

				//Update any release info
				ReleaseManager releaseManager = new ReleaseManager();
				List<int> releaseIds = new List<int>();
				if (oldReleaseId.HasValue)
				{
					releaseIds.Add(oldReleaseId.Value);
				}
				if (newReleaseId.HasValue)
				{
					releaseIds.Add(newReleaseId.Value);
				}
				releaseManager.RefreshProgressEffortTestStatus(incident.ProjectId, releaseIds);
			}
			catch (OptimisticConcurrencyException exception)
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>De-associates incidents from their currently assigned release/iteration</summary>
		/// <param name="incidentIds">The ID of the incidents to be de-associated</param>
		/// <remarks>Only affects the resolved release field</remarks>
		/// <param name="changerId">The ID of the person making the change</param>
		public void RemoveReleaseAssociation(List<int> incidentIds, int changerId)
		{
			const string METHOD_NAME = "RemoveReleaseAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? projectId = null;
				List<int> releaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the incident to see if it's linked to a release
					var query = from i in context.Incidents.Include("Status")
								where incidentIds.Contains(i.IncidentId) && i.ResolvedReleaseId.HasValue && !i.IsDeleted
								select i;

					//Get the incidents
					List<Incident> incidents = query.ToList();
					foreach (Incident incident in incidents)
					{
						//Make sure that the incident is in an open status, otherwise throw a business exception
						if (!incident.Status.IsOpenStatus)
						{
							throw new IncidentClosedException("You can't change the resolved release for a closed incident.");
						}

						//Store the release and project for use later then set to null
						projectId = incident.ProjectId;
						if (!releaseIds.Contains(incident.ResolvedReleaseId.Value))
						{
							releaseIds.Add(incident.ResolvedReleaseId.Value);
						}
						incident.StartTracking();
						incident.ResolvedReleaseId = null;
						incident.LastUpdateDate = DateTime.UtcNow;
						incident.ConcurrencyDate = DateTime.UtcNow;

						//Save changes
						context.SaveChanges(changerId, true, false, null);
					}
				}
				//Now refresh the linked release(s) if appropriate
				ReleaseManager releaseManager = new ReleaseManager();
				if (projectId.HasValue && releaseIds.Count > 0)
				{
					releaseManager.RefreshProgressEffortTestStatus(projectId.Value, releaseIds);
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
		/// Copies across the incident fields and workflows from one project template to another
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="incidentWorkflowMapping">The workflow mapping</param>
		/// <param name="incidentStatusMapping">The status mapping</param>
		/// <param name="incidentTypeMapping">The type mapping</param>
		/// <param name="incidentPriorityMapping">The priority mapping</param>
		/// <param name="incidentSeverityMapping">The severity mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> incidentWorkflowMapping, Dictionary<int, int> incidentStatusMapping, Dictionary<int, int> incidentTypeMapping, Dictionary<int, int> incidentPriorityMapping, Dictionary<int, int> incidentSeverityMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			List<IncidentStatus> incidentStati = this.IncidentStatus_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < incidentStati.Count; i++)
			{
				int oldIncidentStatusId = incidentStati[i].IncidentStatusId;
				int newIncidentStatusId = this.IncidentStatus_Insert(
					newProjectTemplateId,
					incidentStati[i].Name,
					incidentStati[i].IsOpenStatus,
					incidentStati[i].IsDefault,
					incidentStati[i].IsActive);
				incidentStatusMapping.Add(oldIncidentStatusId, newIncidentStatusId);
			}

			//***** Now we need to copy across the incident workflows *****
			WorkflowManager workflowManager = new WorkflowManager();
			List<Workflow> workflows = workflowManager.Workflow_Retrieve(existingProjectTemplateId, false);
			foreach (Workflow workflow in workflows)
			{
				int oldWorkflowId = workflow.WorkflowId;
				int newWorkflowId = workflowManager.Workflow_Insert(
					newProjectTemplateId,
					workflow.Name,
					workflow.IsDefault,
					workflow.IsNotify,
					workflow.IsActive).WorkflowId;
				incidentWorkflowMapping.Add(oldWorkflowId, newWorkflowId);

				//***** Now we need to copy across the workflow transitions *****
				Workflow oldWorkflow = workflowManager.Workflow_RetrieveById(oldWorkflowId, true, true);
				Workflow newWorkflow = workflowManager.Workflow_RetrieveById(newWorkflowId, true, true);
				foreach (WorkflowTransition workflowTransition in oldWorkflow.Transitions)
				{
					//Get the mapped incident statuses
					int oldInputIncidentStatusId = workflowTransition.InputIncidentStatusId;
					int oldOutputIncidentStatusId = workflowTransition.OutputIncidentStatusId;

					if (incidentStatusMapping.ContainsKey(oldInputIncidentStatusId) && incidentStatusMapping.ContainsKey(oldOutputIncidentStatusId))
					{
						int oldWorkflowTransitionId = workflowTransition.WorkflowTransitionId;
						int newInputIncidentStatusId = (int)incidentStatusMapping[oldInputIncidentStatusId];
						int newOuputIncidentStatusId = (int)incidentStatusMapping[oldOutputIncidentStatusId];

						//Insert the new transition
						int newWorkflowTransitionId = workflowManager.WorkflowTransition_Insert(
							newWorkflowId,
							newInputIncidentStatusId,
							newOuputIncidentStatusId,
							workflowTransition.Name,
							workflowTransition.IsExecuteByDetector,
							workflowTransition.IsExecuteByOwner,
							workflowTransition.IsNotifyDetector,
							workflowTransition.IsNotifyOwner,
							workflowTransition.NotifySubject,
							workflowTransition.IsSignatureRequired
							).WorkflowTransitionId;

						//Now we need to copy across the workflow transition roles
						WorkflowTransition newWorkflowTransition = workflowManager.WorkflowTransition_RetrieveById(newWorkflowId, newWorkflowTransitionId);
						newWorkflowTransition.StartTracking();
						foreach (WorkflowTransitionRole workflowTransitionRole in workflowTransition.TransitionRoles)
						{
							newWorkflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { WorkflowTransitionRoleTypeId = workflowTransitionRole.WorkflowTransitionRoleTypeId, ProjectRoleId = workflowTransitionRole.ProjectRoleId });
						}
						workflowManager.WorkflowTransition_Update(newWorkflowTransition);
					}
				}

				//***** Now we need to copy across the workflow field states *****
				newWorkflow.StartTracking();
				foreach (WorkflowField workflowField in oldWorkflow.Fields)
				{
					//Get the mapped incident status
					int oldIncidentStatusId = workflowField.IncidentStatusId;
					if (incidentStatusMapping.ContainsKey(oldIncidentStatusId))
					{
						int newIncidentStatusId = (int)incidentStatusMapping[oldIncidentStatusId];
						newWorkflow.Fields.Add(new WorkflowField() { WorkflowFieldStateId = workflowField.WorkflowFieldStateId, ArtifactFieldId = workflowField.ArtifactFieldId, IncidentStatusId = newIncidentStatusId });
					}
				}

				//***** Now we need to copy across the workflow custom property states *****
				foreach (WorkflowCustomProperty workflowCustomProperty in oldWorkflow.CustomProperties)
				{
					//Get the mapped incident status
					int oldIncidentStatusId = workflowCustomProperty.IncidentStatusId;
					if (incidentStatusMapping.ContainsKey(oldIncidentStatusId))
					{
						int newIncidentStatusId = (int)incidentStatusMapping[oldIncidentStatusId];

						//Get the mapped custom property id
						if (customPropertyIdMapping.ContainsKey(workflowCustomProperty.CustomPropertyId))
						{
							int newCustomPropertyId = customPropertyIdMapping[workflowCustomProperty.CustomPropertyId];
							newWorkflow.CustomProperties.Add(new WorkflowCustomProperty() { WorkflowFieldStateId = workflowCustomProperty.WorkflowFieldStateId, CustomPropertyId = newCustomPropertyId, IncidentStatusId = newIncidentStatusId });
						}
					}
				}

				//Save the changes
				workflowManager.Workflow_Update(newWorkflow);
			}

			//***** Now we need to copy across the incident types *****
			List<IncidentType> incidentTypes = this.RetrieveIncidentTypes(existingProjectTemplateId, false);
			for (int i = 0; i < incidentTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (incidentWorkflowMapping.ContainsKey(incidentTypes[i].WorkflowId))
				{
					int workflowId = (int)incidentWorkflowMapping[incidentTypes[i].WorkflowId];
					int newIncidentTypeId = this.InsertIncidentType(
						newProjectTemplateId,
						incidentTypes[i].Name,
						workflowId,
						incidentTypes[i].IsIssue,
						incidentTypes[i].IsRisk,
						incidentTypes[i].IsDefault,
						incidentTypes[i].IsActive);
					incidentTypeMapping.Add(incidentTypes[i].IncidentTypeId, newIncidentTypeId);
				}
			}

			//***** Now we need to copy across the incident priorities *****
			List<IncidentPriority> incidentPriorities = this.RetrieveIncidentPriorities(existingProjectTemplateId, false);
			for (int i = 0; i < incidentPriorities.Count; i++)
			{
				int newPriorityId = this.InsertIncidentPriority(
					newProjectTemplateId,
					incidentPriorities[i].Name,
					incidentPriorities[i].Color,
					incidentPriorities[i].IsActive);
				incidentPriorityMapping.Add(incidentPriorities[i].PriorityId, newPriorityId);
			}

			//***** Now we need to copy across the incident severities *****
			List<IncidentSeverity> incidentSeverities = this.RetrieveIncidentSeverities(existingProjectTemplateId, false);
			for (int i = 0; i < incidentSeverities.Count; i++)
			{
				int newSeverityId = this.InsertIncidentSeverity(
					newProjectTemplateId,
					incidentSeverities[i].Name,
					incidentSeverities[i].Color,
					incidentSeverities[i].IsActive);
				incidentSeverityMapping.Add(incidentSeverities[i].SeverityId, newSeverityId);
			}
		}

		/// <summary>
		/// Creates the default incident entries for a new project template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		protected internal void CreateforNewProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateforNewProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				WorkflowManager workflowManager = new WorkflowManager();

				//Incident Statuses
				int statusId1 = this.IncidentStatus_Insert(projectTemplateId, "New", true, true, true);
				int statusId2 = this.IncidentStatus_Insert(projectTemplateId, "Open", true, false, true);
				int statusId3 = this.IncidentStatus_Insert(projectTemplateId, "Assigned", true, false, true);
				int statusId4 = this.IncidentStatus_Insert(projectTemplateId, "Resolved", false, false, true);
				int statusId5 = this.IncidentStatus_Insert(projectTemplateId, "Closed", false, false, true);
				int statusId6 = this.IncidentStatus_Insert(projectTemplateId, "Not Reproducible", false, false, true);
				int statusId7 = this.IncidentStatus_Insert(projectTemplateId, "Duplicate", false, false, true);
				int statusId8 = this.IncidentStatus_Insert(projectTemplateId, "Reopen", true, false, true);

				//Incident Workflows
				Workflow workflow = workflowManager.Workflow_Insert(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true, true, true);
				int workflowId = workflow.WorkflowId;

				//Workflow Transitions
				int workflowTransitionId1 = workflowManager.WorkflowTransition_Insert(workflowId, statusId1, statusId2, "Review Incident", false, false, false, false).WorkflowTransitionId;
				int workflowTransitionId2 = workflowManager.WorkflowTransition_Insert(workflowId, statusId1, statusId3, "Assign Incident", false, false, false, true).WorkflowTransitionId;
				int workflowTransitionId3 = workflowManager.WorkflowTransition_Insert(workflowId, statusId2, statusId3, "Assign Incident", false, false, false, true).WorkflowTransitionId;
				int workflowTransitionId4 = workflowManager.WorkflowTransition_Insert(workflowId, statusId2, statusId7, "Duplicate Incident", false, true, false, false).WorkflowTransitionId;
				int workflowTransitionId5 = workflowManager.WorkflowTransition_Insert(workflowId, statusId3, statusId4, "Resolve Incident", false, true, true, false).WorkflowTransitionId;
				int workflowTransitionId6 = workflowManager.WorkflowTransition_Insert(workflowId, statusId3, statusId6, "Unable to Reproduce", false, true, true, false).WorkflowTransitionId;
				int workflowTransitionId7 = workflowManager.WorkflowTransition_Insert(workflowId, statusId3, statusId7, "Duplicate Incident", false, true, false, false).WorkflowTransitionId;
				int workflowTransitionId8 = workflowManager.WorkflowTransition_Insert(workflowId, statusId4, statusId5, "Close Incident", true, false, false, false).WorkflowTransitionId;
				int workflowTransitionId9 = workflowManager.WorkflowTransition_Insert(workflowId, statusId4, statusId8, "Reopen Incident", true, true, false, true).WorkflowTransitionId;
				int workflowTransitionId10 = workflowManager.WorkflowTransition_Insert(workflowId, statusId5, statusId8, "Reopen Incident", true, true, false, true).WorkflowTransitionId;
				int workflowTransitionId11 = workflowManager.WorkflowTransition_Insert(workflowId, statusId6, statusId8, "Reopen Incident", true, true, false, true).WorkflowTransitionId;
				int workflowTransitionId12 = workflowManager.WorkflowTransition_Insert(workflowId, statusId7, statusId8, "Reopen Incident", true, true, false, true).WorkflowTransitionId;
				int workflowTransitionId13 = workflowManager.WorkflowTransition_Insert(workflowId, statusId8, statusId3, "Assign Incident", false, true, false, true).WorkflowTransitionId;
				int workflowTransitionId14 = workflowManager.WorkflowTransition_Insert(workflowId, statusId8, statusId4, "Resolve Incident", false, true, true, false).WorkflowTransitionId;
				int workflowTransitionId15 = workflowManager.WorkflowTransition_Insert(workflowId, statusId8, statusId7, "Duplicate Incident", false, true, false, false).WorkflowTransitionId;
				int workflowTransitionId16 = workflowManager.WorkflowTransition_Insert(workflowId, statusId8, statusId6, "Unable to Reproduce", false, true, true, false).WorkflowTransitionId;

				//Workflow Transition Roles
				//Review Incident
				WorkflowTransition workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId1);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleProjectOwner, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Assign Incident
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId2);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleProjectOwner, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Assign Incident
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId3);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleProjectOwner, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Unable to Reproduce
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId6);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Close Incident
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId8);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleProjectOwner, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Reopen Incident
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId12);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleProjectOwner, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Execute });
				workflowManager.WorkflowTransition_Update(workflowTransition);
				//Unable to Reproduce
				workflowTransition = workflowManager.WorkflowTransition_RetrieveById(workflowId, workflowTransitionId16);
				workflowTransition.StartTracking();
				workflowTransition.TransitionRoles.Add(new WorkflowTransitionRole() { ProjectRoleId = ProjectManager.ProjectRoleManager, WorkflowTransitionRoleTypeId = (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify });
				workflowManager.WorkflowTransition_Update(workflowTransition);

				//Workflow Fields

				//All fields are active, visible and not-required by default, so only need to populate the ones
				//that are exceptions to that case

				//Status=New
				IncidentStatus incidentStatus = this.IncidentStatus_RetrieveById(statusId1, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 48, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 127, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 7, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				//Hidden
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 8, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 9, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 14, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 136, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Open
				incidentStatus = this.IncidentStatus_RetrieveById(statusId2, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 48, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 127, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				//Hidden
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 9, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 14, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Hidden });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Assigned
				incidentStatus = this.IncidentStatus_RetrieveById(statusId3, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 48, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 127, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 45, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 47, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Resolved
				incidentStatus = this.IncidentStatus_RetrieveById(statusId4, true);
				incidentStatus.StartTracking();
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 12, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 45, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 47, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 48, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 127, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Closed
				incidentStatus = this.IncidentStatus_RetrieveById(statusId5, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 1, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 7, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 12, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 14, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Not Reproducible
				incidentStatus = this.IncidentStatus_RetrieveById(statusId6, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 1, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 7, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 12, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Duplicate
				incidentStatus = this.IncidentStatus_RetrieveById(statusId7, true);
				incidentStatus.StartTracking();
				//Inactive
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 1, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 7, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive });
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 12, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Status=Reopen
				incidentStatus = this.IncidentStatus_RetrieveById(statusId8, true);
				incidentStatus.StartTracking();
				//Required
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 2, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 4, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 5, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 6, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 10, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 11, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 12, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 45, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				incidentStatus.WorkflowFields.Add(new WorkflowField() { WorkflowId = workflowId, ArtifactFieldId = 47, WorkflowFieldStateId = (int)WorkflowFieldState.WorkflowFieldStateEnum.Required });
				this.IncidentStatus_Update(incidentStatus);

				//Incident Types
				this.InsertIncidentType(projectTemplateId, "Incident", workflowId, false, false, true, true);
				this.InsertIncidentType(projectTemplateId, "Bug", workflowId, false, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Enhancement", workflowId, false, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Issue", workflowId, true, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Training", workflowId, false, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Limitation", workflowId, false, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Change Request", workflowId, false, false, false, true);
				this.InsertIncidentType(projectTemplateId, "Problem", workflowId, false, true, false, true);

				//Incident Priorities
				this.InsertIncidentPriority(projectTemplateId, "1 - Critical", "f47457", true);
				this.InsertIncidentPriority(projectTemplateId, "2 - High", "f29e56", true);
				this.InsertIncidentPriority(projectTemplateId, "3 - Medium", "f5d857", true);
				this.InsertIncidentPriority(projectTemplateId, "4 - Low", "f4f356", true);

				//Incident Severities
				this.InsertIncidentSeverity(projectTemplateId, "1 - Critical", "f47457", true);
				this.InsertIncidentSeverity(projectTemplateId, "2 - High", "f29e56", true);
				this.InsertIncidentSeverity(projectTemplateId, "3 - Medium", "f5d857", true);
				this.InsertIncidentSeverity(projectTemplateId, "4 - Low", "f4f356", true);
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Makes a copy of an incident within the same project</summary>
		/// <param name="userId">The id of the user making the copy</param>
		/// <param name="incidentId">The id of the incident we want to make a copy of</param>
		/// <returns>The id of the newly created copy</returns>
		public int Copy(int userId, int incidentId)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the incident we want to copy
				Incident incident = this.RetrieveById(incidentId, true, false, true);

				//Get the project and project template id
				int projectId = incident.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the component ids
				List<int> componentIds = incident.ComponentIds.FromDatabaseSerialization_List_Int32();

				//Get the linked test run steps
				List<int> testRunStepIds = incident.TestRunSteps.Select(t => t.TestRunStepId).ToList();

				//Actually perform the insert of the copy
				int copiedIncidentId = this.Insert(
					incident.ProjectId,
					incident.PriorityId,
					incident.SeverityId,
					incident.OpenerId,
					incident.OwnerId,
					testRunStepIds,
					incident.Name + CopiedArtifactNameSuffix,
					incident.Description,
					incident.DetectedReleaseId,
					incident.ResolvedReleaseId,
					incident.VerifiedReleaseId,
					incident.IncidentTypeId,
					incident.IncidentStatusId,
					DateTime.UtcNow,
					incident.StartDate,
					incident.ClosedDate,
					incident.EstimatedEffort,
					incident.ActualEffort,
					incident.RemainingEffort,
					incident.BuildId,
					componentIds,
					userId
					);

				//Now we need to copy across any resolutions
				this.CopyResolutions(incidentId, copiedIncidentId);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, incidentId, copiedIncidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachment = new AttachmentManager();
				attachment.Copy(incident.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, copiedIncidentId);

				//Finally we need to add an association from the old incident to the new one
				ArtifactLinkManager artifactLink = new ArtifactLinkManager();
				artifactLink.Insert(
					incident.ProjectId,
					DataModel.Artifact.ArtifactTypeEnum.Incident,
					incidentId,
					DataModel.Artifact.ArtifactTypeEnum.Incident,
					copiedIncidentId,
					userId,
					"Copied Incident",
					DateTime.UtcNow
					);

				//Send a notification
				this.SendCreationNotification(copiedIncidentId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the incident id of the copy
				return copiedIncidentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Validates an incident row prior to it being sent for update</summary>
		/// <param name="incident">The incident row being validated</param>
		/// <returns>A dictionary of validation messages</returns>
		public Dictionary<string, string> Validate(Incident incident)
		{
			const string METHOD_NAME = "Validate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Dictionary<string, string> messages = new Dictionary<string, string>();

			try
			{
				//Check the percentage range
				if (incident.CompletionPercent < 0 || incident.CompletionPercent > 100)
				{
					if (!messages.ContainsKey("ClosedDate"))
					{
						messages.Add("ClosedDate", GlobalResources.Messages.Incident_CompletionPercentOutOfRange);
					}
				}

				//Make sure that the closed date is not before the creation/detection date
				if (incident.ClosedDate.HasValue)
				{
					if (incident.CreationDate.Date > incident.ClosedDate.Value.Date)
					{
						if (!messages.ContainsKey("ClosedDate"))
						{
							messages.Add("ClosedDate", GlobalResources.Messages.Incident_ClosedDateCannotBeBeforeDetected);
						}
					}
				}

				//Make sure that the start date is not before the detected date(!)
				//Since start date has no time, we add 24 hours of buffer to avoid annoying messages
				//when you create and start an incident on the same day
				if (incident.StartDate.HasValue && incident.CreationDate.Date > incident.StartDate.Value.Date.AddHours(24))
				{
					if (!messages.ContainsKey("StartDate"))
					{
						messages.Add("StartDate", GlobalResources.Messages.Incident_StartDateCannotBeBeforeDetected);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return messages;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports an incident from one project to another</summary>
		/// <param name="incidentId">The id of the incident we want to make a copy of</param>
		/// <param name="destProjectId">The project we want to export it to</param>
		/// <returns>The id of the newly created copy</returns>
		/// <remarks>Any project template configurable fields (priority, status, etc.) will be either unset or set to default values
		/// if the projects don't use the same template. Also the start and closed dates have to be left unset
		/// because we have a new detected on date.
		/// </remarks>
		public int Export(int incidentId, int destProjectId, int userId)
		{
			const string METHOD_NAME = "Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the incident we want to copy
				Incident existingIncident = this.RetrieveById(incidentId, true);

				//Get the IDs of the source and destination project templates
				TemplateManager templateManager = new TemplateManager();
				int sourceProjectTemplateId = templateManager.RetrieveForProject(existingIncident.ProjectId).ProjectTemplateId;
				int destProjectTemplateId = templateManager.RetrieveForProject(destProjectId).ProjectTemplateId;
				bool projectsUseSameTemplate = (sourceProjectTemplateId == destProjectTemplateId);

				int incidentStatusId;
				int incidentTypeId;
				int? incidentPriorityId = null;
				int? incidentSeverityId = null;
				if (projectsUseSameTemplate)
				{
					incidentStatusId = existingIncident.IncidentStatusId;
					incidentTypeId = existingIncident.IncidentTypeId;
					incidentPriorityId = existingIncident.PriorityId;
					incidentSeverityId = existingIncident.SeverityId;
				}
				else
				{
					//Need to get the default type and status for the project
					incidentStatusId = this.IncidentStatus_RetrieveDefault(destProjectTemplateId).IncidentStatusId;
					incidentTypeId = this.GetDefaultIncidentType(destProjectTemplateId);
				}

				//Actually perform the insert of the copy
				//Some will always be null as the projects don't have the same
				int exportedIncidentId = this.Insert(
					destProjectId,
					incidentPriorityId,
					incidentSeverityId,
					existingIncident.OpenerId,
					existingIncident.OwnerId,
					null,
					existingIncident.Name,
					existingIncident.Description,
					null,
					null,
					null,
					incidentTypeId,
					incidentStatusId,
					existingIncident.CreationDate,
					null,
					null,
					existingIncident.EstimatedEffort,
					existingIncident.ActualEffort,
					existingIncident.RemainingEffort,
					null,
					null,
					userId,
					false
					);

				//Create history item..
				new HistoryManager().LogImport(destProjectId, existingIncident.ProjectId, existingIncident.IncidentId, userId, DataModel.Artifact.ArtifactTypeEnum.Incident, exportedIncidentId, DateTime.UtcNow);

				//Now we need to copy across any resolutions
				CopyResolutions(incidentId, exportedIncidentId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(existingIncident.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, destProjectId, exportedIncidentId);

				//If they use the same template, also copy across any custom properties
				if (projectsUseSameTemplate)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, existingIncident.ProjectId, incidentId, destProjectId, exportedIncidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the incident id of the copy
				return exportedIncidentId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<IncidentStatus> RetrieveStatuses(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			List<IncidentStatus> incidentStatuses = new List<IncidentStatus>();

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.IncidentStati
								where r.IsActive && r.ProjectTemplateId == projectTemplateId
								orderby  r.IncidentStatusId
								select r;

					incidentStatuses = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<IncidentStatusResponse> WorkflowTransition_RetrieveByInputStatusById(int workflowId, int inputIncidentStatusId, int projectTemplateId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.WorkflowTransitions.Include("InputIncidentStatus").Include("OutputIncidentStatus")
								join i in context.IncidentStati
								on t.InputIncidentStatusId equals i.IncidentStatusId
								join o in context.IncidentStati
								on t.OutputIncidentStatusId equals o.IncidentStatusId
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId && t.InputIncidentStatusId == inputIncidentStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new IncidentStatusResponse
								{
									InputIncidentStatusId = t.InputIncidentStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByDetector,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									IncidentWorkflowId = t.WorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyDetector,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
									URL = "",
									OutURL = "",
									InURL = "",
									OutputIncidentStatusId = t.OutputIncidentStatusId,
								};

					var workflows = query.ToList();

					foreach (var c in workflows)
					{
						c.InURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/IncidentWorkflowStep.aspx?workflowId=" + workflowId + "&incidentStatusId=" + c.InputIncidentStatusId;
						c.URL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/IncidentWorkflowTransition.aspx?workflowId=" + workflowId + "&workflowTransitionId=" + c.WorkflowTransitionId;
						c.OutURL = "/ValidationMaster/pt/" + projectTemplateId + "/Administration/IncidentWorkflowStep.aspx?workflowId=" + workflowId + "&incidentStatusId=" + c.OutputIncidentStatusId;
					}

					query = workflows.AsQueryable();

					List<IncidentStatusResponse> workflowTransitions = query.ToList();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return workflowTransitions;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		public List<IncidentStatusResponse> WorkflowTransition_RetrieveAllStatuses(int workflowId, int inputIncidentStatusId)
		{
			const string METHOD_NAME = "WorkflowTransition_RetrieveByInputStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.WorkflowTransitions.Include("InputIncidentStatus").Include("OutputIncidentStatus")
								join i in context.IncidentStati
								on t.InputIncidentStatusId equals i.IncidentStatusId
								join o in context.IncidentStati
								on t.OutputIncidentStatusId equals o.IncidentStatusId
								where t.OutputStatus.IsActive && t.WorkflowId == workflowId && t.InputIncidentStatusId == inputIncidentStatusId
								orderby t.Name, t.WorkflowTransitionId
								select new IncidentStatusResponse
								{
									InputIncidentStatusId = t.InputIncidentStatusId,
									TransitionName = t.Name,
									InputStatusName = i.Name,
									OutputStatusName = o.Name,
									IsExecuteByCreator = t.IsExecuteByDetector,
									IsExecuteByOwner = t.IsExecuteByOwner,
									WorkflowTransitionId = t.WorkflowTransitionId,
									IncidentWorkflowId = t.WorkflowId,
									IsBlankOwner = t.IsBlankOwner,
									NotifySubject = t.NotifySubject,
									IsNotifyCreator = t.IsNotifyDetector,
									IsNotifyOwner = t.IsNotifyOwner,
									TransitionRoles = t.TransitionRoles,
									Workflow = t.Workflow,
								};

					List<IncidentStatusResponse> workflowTransitions = query.ToList();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
					return workflowTransitions;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}


		#endregion

		#region Incident Resolution Functions

		/// <summary>Copies across all the resolutions from one incident to another</summary>
		/// <param name="sourceIncidentId">The incident we're copying resolutions from</param>
		/// <param name="destIncidentId">The incident we're copying resolutions to</param>
		public void CopyResolutions(int sourceIncidentId, int destIncidentId)
		{
			const string METHOD_NAME = "CopyResolutions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the source incident and its resolutions
				Incident incident = this.RetrieveById(sourceIncidentId, true);

				//Now iterate through the resolutions and add the resolution to the destination one
				foreach (IncidentResolution resolution in incident.Resolutions)
				{
					this.InsertResolution(destIncidentId, resolution.Resolution, resolution.CreationDate, resolution.CreatorId, false);
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
		/// Retrieves an incident resolution by its id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="incidentResolutionId">The id of the incident resolution</param>
		/// <returns>The incident resolution</returns>
		public IncidentResolution Resolution_RetrieveById(int projectId, int incidentId, int incidentResolutionId)
		{
			const string METHOD_NAME = "Resolution_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentResolution incidentResolution;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First make sure the incident exists and is not deleted
					var query = from i in context.Incidents
								where !i.IsDeleted && i.IncidentId == incidentId
								select i;

					if (query.Count() < 1)
					{
						throw new ArtifactNotExistsException("Incident " + incidentId + " doesn't exist in the system.");
					}

					//Need to get the resolution associated with the incident
					var query2 = from i in context.IncidentResolutions.Include("Creator").Include("Creator.Profile")
								 where i.IncidentId == incidentId && i.IncidentResolutionId == incidentResolutionId
								 select i;

					incidentResolution = query2.FirstOrDefault();
				}

				//Make sure we have at least one row
				if (incidentResolution == null)
				{
					throw new ArtifactNotExistsException("Unable to find an incident resolution with id=" + incidentResolutionId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentResolution;
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

		public IncidentResolution Resolution_RetrieveByResolutionId(int incidentResolutionId)
		{
			const string METHOD_NAME = "Resolution_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentResolution incidentResolution;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Need to get the resolution associated with the incident
					var query2 = from i in context.IncidentResolutions.Include("Creator").Include("Creator.Profile")
								 where i.IncidentResolutionId == incidentResolutionId
								 select i;

					incidentResolution = query2.FirstOrDefault();
				}

				//Make sure we have at least one row
				if (incidentResolution == null)
				{
					throw new ArtifactNotExistsException("Unable to find an incident resolution with id=" + incidentResolutionId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentResolution;
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
		/// Deletes an incident resolution by its id
		/// </summary>
		/// <param name="incidentId">the id of the incident</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="changerId">The id of the user making the change</param>
		/// <param name="incidentResolutionId">The id of the incident resolution</param>
		public void Resolution_Delete(int projectId, int incidentId, int incidentResolutionId, int changerId)
		{
			const string METHOD_NAME = "Resolution_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Incident notificationArtifact;
					//First make sure the incident exists and is not deleted
					var query = from i in context.Incidents
								where !i.IsDeleted && i.IncidentId == incidentId
								select i;

					if (query.Count() < 1)
					{
						throw new ArtifactNotExistsException("Incident " + incidentId + " doesn't exist in the system.");
					}

					notificationArtifact = query.FirstOrDefault();

					//Need to get the resolution associated with the incident
					var query2 = from i in context.IncidentResolutions
								 where i.IncidentId == incidentId && i.IncidentResolutionId == incidentResolutionId
								 select i;

					//Now delete the resolution record, if it exists
					IncidentResolution incidentResolution = query2.FirstOrDefault();
					if (incidentResolution != null)
					{
						incidentResolution.StartTracking();
						context.DeleteObject(incidentResolution);
						context.SaveChanges();

						new HistoryManager().LogDeletion(projectId, changerId, DataModel.Artifact.ArtifactTypeEnum.IncidentResolution, incidentResolutionId, DateTime.UtcNow, null, null, incidentResolution.Resolution);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
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

		/// <summary>Inserts a new resolution into an existing incident</summary>
		/// <param name="incidentId">The incident to add the resolution to</param>
		/// <param name="resolution">The description of the resolution/comment</param>
		/// <param name="creationDate">The date that the resolution was added/created</param>
		/// <param name="creatorId">The user who's adding the resolution</param>
		/// <returns>The ID of the incident resolution</returns>
		public int InsertResolution(int incidentId, string resolution, DateTime creationDate, int creatorId, bool sendNotification)
		{
			const string METHOD_NAME = "InsertResolution";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentResolutionId;
				Incident notificationArtifact;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First make sure the incident exists and is not deleted
					var query = from i in context.Incidents
								where !i.IsDeleted && i.IncidentId == incidentId
								select i;

					notificationArtifact = query.FirstOrDefault();
					if (notificationArtifact == null)
					{
						throw new ArtifactNotExistsException("Incident " + incidentId + " doesn't exist in the system.");
					}

					//Now add the resolution entity
					IncidentResolution incidentResolution = new IncidentResolution();
					incidentResolution.IncidentId = incidentId;
					incidentResolution.Resolution = resolution;
					incidentResolution.CreatorId = creatorId;
					incidentResolution.CreationDate = creationDate;
					context.IncidentResolutions.AddObject(incidentResolution);
					context.SaveChanges();
					incidentResolutionId = incidentResolution.IncidentResolutionId;

					//context.SaveChanges();
					new HistoryManager().LogCreation(notificationArtifact.ProjectId, (int)creatorId, DataModel.Artifact.ArtifactTypeEnum.IncidentResolution, incidentResolutionId, DateTime.UtcNow);
				}

				//Send to Notification to see if we need to send anything out.
				if (notificationArtifact != null && sendNotification)
				{
					try
					{
						new NotificationManager().SendNotificationForArtifact(notificationArtifact, null, resolution);
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentResolutionId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Graphing/Reporting Functions

		/// <summary>Retrieves the test case coverage for resolved incidents in the project/release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release we want to filter on (null for all)</param>
		/// <returns>List of incident test case coverage</returns>
		/// <remarks>Always returns all the execution status codes</remarks>
		public List<IncidentTestCoverage> RetrieveTestCoverage(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveTestCoverage";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentTestCoverage> testCoverage;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to get the data
					testCoverage = context.Incident_RetrieveTestCoverage(projectId, releaseId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCoverage;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the incident aging count for a project group as a whole</summary>
		/// <param name="projectGroupId">The project group we're interested in</param>
		/// <param name="ageInterval">The age interval</param>
		/// <param name="maximumAge">The maxium age that is displayed on the graph</param>
		/// <returns>The requested aging data</returns>
		public DataSet RetrieveAging(int projectGroupId, int maximumAge, int ageInterval)
		{
			const string METHOD_NAME = "RetrieveAging";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the final dataset
			DataSet incidentAgingDataSet = new DataSet();

			try
			{
				//First get the count of open incidents
				List<IncidentAgeCount> ageOpenCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ageOpenCount = context.Incident_RetrieveGroupAgingCount(projectGroupId).ToList();
				}

				//Store the maximum age value (needed later on) - corresponds to last row
				int maxAge = maximumAge + 1;
				if (ageOpenCount.Count > 0)
				{
					IncidentAgeCount lastItem = ageOpenCount.Last();
					if (lastItem.Age.HasValue && ageOpenCount.Last().Age.Value > maxAge)
					{
						maxAge = ageOpenCount.Last().Age.Value;
					}
				}

				//Now we need to create an empty dataset containing the # days aged and a count column
				incidentAgingDataSet.Tables.Add("IncidentAging");
				incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("Age", typeof(string));
				incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("Count", typeof(int));

				//Make Age the primary key
				incidentAgingDataSet.Tables["IncidentAging"].PrimaryKey = new DataColumn[] {
					incidentAgingDataSet.Tables["IncidentAging"].Columns["Age"]
					};

				//Now loop through the dataset and populate with the age-ranges labels
				//and count the number of incidents per priority in each range

				int lowerBound = 0;
				int upperBound = ageInterval;

				//Loop until we have a lower bound that equals the maximum age
				while (lowerBound <= (maximumAge + 1))
				{
					//Populate the age value label (special case for greater than the maximum age)
					System.Data.DataRow dataRow = incidentAgingDataSet.Tables["IncidentAging"].NewRow();
					if (lowerBound < (maximumAge + 1))
					{
						dataRow["Age"] = lowerBound.ToString() + "-" + upperBound.ToString();
					}
					else
					{
						dataRow["Age"] = "> " + maximumAge;
						upperBound = maxAge; //Catch anything bigger than the maximumAge
					}
					incidentAgingDataSet.Tables["IncidentAging"].Rows.Add(dataRow);

					//Now find all incidents that lie within this aging interval
					int ageTotal = 0;
					for (int j = lowerBound; j <= upperBound; j++)
					{
						//Get the row that matches this age interval
						IncidentAgeCount foundAgeInternal = ageOpenCount.FirstOrDefault(a => a.Age == j);
						if (foundAgeInternal != null && foundAgeInternal.OpenCount.HasValue)
						{
							ageTotal += foundAgeInternal.OpenCount.Value;
						}
					}
					dataRow["Count"] = ageTotal;

					//Move to the next aging interval
					lowerBound = upperBound + 1;
					upperBound += ageInterval;

					//(special case for last but one interval)
					if (upperBound == maximumAge + 1)
					{
						lowerBound = maximumAge - ageInterval + 1;
						upperBound = maximumAge;
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

		/// <summary>Retrieves the incident aging count for the project/release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release we're interested in (optional)</param>
		/// <param name="ageInterval">The age interval</param>
		/// <param name="maximumAge">The maxium age that is displayed on the graph</param>
		/// <returns>The requested aging data</returns>
		public DataSet RetrieveAging(int projectId, int? releaseId, int maximumAge, int ageInterval)
		{
			const string METHOD_NAME = "RetrieveAging";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create the new dataset
			DataSet incidentAgingDataSet = new DataSet();

			try
			{
				//First get the count of open incidents
				List<IncidentAgeCount> ageOpenCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ageOpenCount = context.Incident_RetrieveProjectAgingCount(projectId, releaseId).ToList();
				}

				//Store the maximum age value (needed later on) - corresponds to last row
				int maxAge = maximumAge + 1;
				if (ageOpenCount.Count > 0)
				{
					IncidentAgeCount lastItem = ageOpenCount.Last();
					if (lastItem.Age.HasValue && ageOpenCount.Last().Age.Value > maxAge)
					{
						maxAge = ageOpenCount.Last().Age.Value;
					}
				}

				//Now we need to create an empty dataset containing the # days aged and a count column
				incidentAgingDataSet.Tables.Add("IncidentAging");
				incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("Age", typeof(string));
				incidentAgingDataSet.Tables["IncidentAging"].Columns.Add("Count", typeof(int));

				//Make Age the primary key
				incidentAgingDataSet.Tables["IncidentAging"].PrimaryKey = new DataColumn[] {
					incidentAgingDataSet.Tables["IncidentAging"].Columns["Age"]
					};

				//Now loop through the dataset and populate with the age-ranges labels
				//and count the number of incidents per priority in each range

				int lowerBound = 0;
				int upperBound = ageInterval;

				//Loop until we have a lower bound that equals the maximum age
				while (lowerBound <= (maximumAge + 1))
				{
					//Populate the age value label (special case for greater than the maximum age)
					System.Data.DataRow dataRow = incidentAgingDataSet.Tables["IncidentAging"].NewRow();
					if (lowerBound < (maximumAge + 1))
					{
						dataRow["Age"] = lowerBound.ToString() + "-" + upperBound.ToString();
					}
					else
					{
						dataRow["Age"] = "> " + maximumAge;
						upperBound = maxAge; //Catch anything bigger than the maximumAge
					}
					incidentAgingDataSet.Tables["IncidentAging"].Rows.Add(dataRow);

					//Now find all incidents that lie within this aging interval
					int ageTotal = 0;
					for (int j = lowerBound; j <= upperBound; j++)
					{
						//Get the row that matches this age interval
						IncidentAgeCount foundAgeInternal = ageOpenCount.FirstOrDefault(a => a.Age == j);
						if (foundAgeInternal != null && foundAgeInternal.OpenCount.HasValue)
						{
							ageTotal += foundAgeInternal.OpenCount.Value;
						}
					}
					dataRow["Count"] = ageTotal;

					//Move to the next aging interval
					lowerBound = upperBound + 1;
					upperBound += ageInterval;

					//(special case for last but one interval)
					if (upperBound == maximumAge + 1)
					{
						lowerBound = maximumAge - ageInterval + 1;
						upperBound = maximumAge;
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

		/// <summary>Retrieves the incident summary (importance vs. status) data-table for a particular project and incident-type</summary>
		/// <param name="projectId">The project we want the summary report for</param>
		/// <param name="releaseId">The release we want to filter on (Null for all)</param>
		/// <param name="incidentTypeId">The type of incident we're interested in (pass Null for all)</param>
		/// <param name="useSeverity">Should we order by severity instead of priority</param>
		/// <param name="useResolvedRelease">Should we use resolved release instead of detected release for the filter</param>
		/// <param name="projectTemplateId">The id of the project template being used</param>
		/// <returns>An untyped dataset of priority/severity vs. status</returns>
		public DataSet RetrieveProjectSummary(int projectId, int projectTemplateId, int? incidentTypeId, int? releaseId, bool useSeverity, bool useResolvedRelease)
		{
			const string METHOD_NAME = "RetrieveProjectSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new empty untyped dataset with a table to hold the summary data and columns
			//to hold the status codes
			System.Data.DataSet projectSummaryDataSet = new System.Data.DataSet();
			projectSummaryDataSet.Tables.Add("IncidentSummary");
			projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add("IncidentStatusId", typeof(int));
			projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add("IncidentStatusName", typeof(string));
			//Make IncidentStatusId the primary key
			projectSummaryDataSet.Tables["IncidentSummary"].PrimaryKey = new DataColumn[] { projectSummaryDataSet.Tables["IncidentSummary"].Columns["IncidentStatusId"] };

			//We need to get a list of all the possible priority or severity levels (for the column headings)
			List<IncidentSeverity> incidentSeverities = null;
			List<IncidentPriority> incidentPriorities = null;
			if (useSeverity)
			{
				incidentSeverities = this.RetrieveIncidentSeverities(projectTemplateId, true);
				//Iterate through the summary dataset and add the columns
				foreach (IncidentSeverity incidentSeverity in incidentSeverities)
				{
					//Add the severity ID as the column name
					projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add(incidentSeverity.SeverityId.ToString(), typeof(int));
				}
			}
			else
			{
				incidentPriorities = this.RetrieveIncidentPriorities(projectTemplateId, true);
				//Iterate through the summary dataset and add the columns
				foreach (IncidentPriority incidentPriority in incidentPriorities)
				{
					//Add the priority ID as the column name
					projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add(incidentPriority.PriorityId.ToString(), typeof(int));
				}
			}

			//Now add the (None) and TOTAL columns
			projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add("None", typeof(int));
			projectSummaryDataSet.Tables["IncidentSummary"].Columns.Add("Total", typeof(int));

			//Get a list of the different status codes and names and add to summary
			List<IncidentStatus> incidentStati = IncidentStatus_Retrieve(projectTemplateId, true);
			for (int i = 0; i < incidentStati.Count; i++)
			{
				System.Data.DataRow dataRow = projectSummaryDataSet.Tables["IncidentSummary"].NewRow();
				dataRow["IncidentStatusId"] = incidentStati[i].IncidentStatusId;
				dataRow["IncidentStatusName"] = incidentStati[i].Name;
				projectSummaryDataSet.Tables["IncidentSummary"].Rows.Add(dataRow);
			}

			//Now we need to execute the query for each column to retrieve the count of incidents per status id
			//Iterate through the summary dataset and add the columns
			List<IncidentStatusCount> incidentStatusCount;
			if (useSeverity)
			{
				for (int i = 0; i < incidentSeverities.Count; i++)
				{
					//Get the severity id
					int severityId = incidentSeverities[i].SeverityId;

					//Get the dataset of incident counts against status for a particular severity, release and incident tpe
					incidentStatusCount = this.RetrieveCountBySeverity(projectId, severityId, incidentTypeId, releaseId, useResolvedRelease);

					//Now iterate through this dataset of count against status and add to summary
					for (int j = 0; j < incidentStatusCount.Count; j++)
					{
						//capture the incidentStatusId from the count dataset
						int incidentStatusId = incidentStatusCount[j].IncidentStatusId;

						//Find the row with the matching ID in the summary table (make sure we have a match)
						System.Data.DataRow dataRow = projectSummaryDataSet.Tables["IncidentSummary"].Rows.Find(incidentStatusId);

						//The severity ID is the column name
						if (dataRow != null)
						{
							dataRow[severityId.ToString()] = incidentStatusCount[j].IncidentCount;
						}
					}
				}
				//Now we need to add the incidents that don't have a severity level
				incidentStatusCount = this.RetrieveCountBySeverity(projectId, null, incidentTypeId, releaseId, useResolvedRelease);
			}
			else
			{
				for (int i = 0; i < incidentPriorities.Count; i++)
				{
					//Get the priority id
					int priorityId = incidentPriorities[i].PriorityId;

					//Get the dataset of incident counts against status for a particular priority, release and incident tpe
					incidentStatusCount = this.RetrieveCountByPriority(projectId, priorityId, incidentTypeId, releaseId, useResolvedRelease);

					//Now iterate through this dataset of count against status and add to summary
					for (int j = 0; j < incidentStatusCount.Count; j++)
					{
						//capture the incidentStatusId from the count dataset
						int incidentStatusId = incidentStatusCount[j].IncidentStatusId;

						//Find the row with the matching ID in the summary table (make sure we have a match)
						System.Data.DataRow dataRow = projectSummaryDataSet.Tables["IncidentSummary"].Rows.Find(incidentStatusId);

						//The priority ID is the column name
						if (dataRow != null)
						{
							dataRow[priorityId.ToString()] = incidentStatusCount[j].IncidentCount;
						}
					}
				}
				//Now we need to add the incidents that don't have a priority level
				incidentStatusCount = this.RetrieveCountByPriority(projectId, null, incidentTypeId, releaseId, useResolvedRelease);
			}

			//Now iterate through this dataset of count against status and add to summary
			for (int j = 0; j < incidentStatusCount.Count; j++)
			{
				//capture the incidentStatusId from the count dataset
				int incidentStatusId = incidentStatusCount[j].IncidentStatusId;

				//Find the row with the matching ID in the summary table
				System.Data.DataRow dataRow = projectSummaryDataSet.Tables["IncidentSummary"].Rows.Find(incidentStatusId);

				//The priority ID is the column name
				if (dataRow != null)
				{
					dataRow["None"] = incidentStatusCount[j].IncidentCount;
				}
			}

			//Finally we need to iterate through each row and calculate the total column
			for (int i = 0; i < projectSummaryDataSet.Tables["IncidentSummary"].Rows.Count; i++)
			{
				//Now iterate through the columns (except for the last one which is the total and first which is the ID)
				int total = 0;
				for (int j = 1; j < projectSummaryDataSet.Tables["IncidentSummary"].Columns.Count; j++)
				{
					if ((projectSummaryDataSet.Tables["IncidentSummary"].Rows[i][j]).GetType() == typeof(int))
					{
						int count = (int)projectSummaryDataSet.Tables["IncidentSummary"].Rows[i][j];
						total += count;
					}
				}

				//Finally set the row total field
				projectSummaryDataSet.Tables["IncidentSummary"].Rows[i]["Total"] = total;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return (projectSummaryDataSet);
		}

		/// <summary>Retrieves a count of incidents by status for a particular priority level</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="priorityId">The priority ID (pass Null) for those where it's NULL</param>
		/// <param name="incidentTypeId">The incident type ID (pass Null for all)</param>
		/// <param name="releaseId">The release to filter on (pass NullParameter for all)</param>
		/// <param name="includeDeleted">Whether to include deleted Incidents or not.</param>
		/// <param name="useResolvedRelease">Should we use resolved release instead of detected by release</param>
		/// <returns>List of incident count by status</returns>
		protected List<IncidentStatusCount> RetrieveCountByPriority(int projectId, int? priorityId, int? incidentTypeId, int? releaseId, bool useResolvedRelease, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveCountByPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentStatusCount> incidentStatusCount;

				//Call the stored procedure to get the incident count
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					incidentStatusCount = context.Incident_RetrieveCountByPriority(projectId, releaseId, priorityId, incidentTypeId, useResolvedRelease, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatusCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a count of incidents that are open or closed</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>List of incident count by open/closed status</returns>
		/// <param name="releaseId">Should we filter the results by release</param>
		/// <param name="useResolvedRelease">If filtering by release, should we use the resolved release (instead of detected release)</param>
		public List<IncidentOpenClosedCount> RetrieveOpenClosedCount(int projectId, int? releaseId, bool useResolvedRelease)
		{
			const string METHOD_NAME = "RetrieveOpenClosedCount";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentOpenClosedCount> incidentStatusCount;

				//Call the stored procedure to get the incident count
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					incidentStatusCount = context.Incident_RetrieveOpenClosedCount(projectId, releaseId, useResolvedRelease).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatusCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a count of incidents by status for a particular severity level</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="severityId">The severity ID (pass Null) for those where it's NULL</param>
		/// <param name="incidentTypeId">The incident type ID (pass Null for all)</param>
		/// <param name="releaseId">The release to filter on (pass Null for all)</param>
		/// <param name="includeDeleted">Whether or not to include deleted Incidents.</param>
		/// <param name="useResolvedRelease">Should we use resolved release instead of detected release</param>
		/// <returns>Untyped dataset of incident count by status</returns>
		protected List<IncidentStatusCount> RetrieveCountBySeverity(int projectId, int? severityId, int? incidentTypeId, int? releaseId, bool useResolvedRelease, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveCountBySeverity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentStatusCount> incidentStatusCount;

				//Call the stored procedure to get the incident count
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					incidentStatusCount = context.Incident_RetrieveCountBySeverity(projectId, releaseId, severityId, incidentTypeId, useResolvedRelease, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentStatusCount;

			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Static Helper Functions

		/// <summary>Calculates the colors to display in the incident progress bar</summary>
		/// <param name="incident">The incident entity</param>
		/// <param name="percentGreen">The % that should be displayed as green</param>
		/// <param name="percentRed">The % that should be displayed as red</param>
		/// <param name="percentYellow">The % that should be displayed as yellow</param>
		/// <param name="percentGray">The % that should be displayed as gray</param>
		/// <returns>The textual version of the progress information</returns>
		public static string CalculateProgress(Incident incident, out int percentGreen, out int percentRed, out int percentYellow, out int percentGray)
		{
			//Set the default percents
			percentGreen = 0;
			percentRed = 0;
			percentYellow = 0;
			percentGray = 0;

			//We need to get the start-date, and % completion
			//Unlike tasks, incidents don't have a due-date / end-date
			//so the bar with only ever be green or yellow not red
			DateTime startDate = DateTime.MaxValue;
			if (incident.StartDate.HasValue)
			{
				startDate = incident.StartDate.Value;
			}
			int percentComplete = incident.CompletionPercent;

			//Now populate the equalizer graph
			string tooltipText = percentComplete + "% " + GlobalResources.General.Incident_Complete;

			//See if we're completed or not
			if (percentComplete == 0)
			{
				//We've not yet started so display as gray unless the start date has already been passed
				if (startDate >= DateTime.UtcNow)
				{
					percentGray = 100;
					tooltipText += ", " + GlobalResources.General.Incident_NotStarted;
				}
				else
				{
					percentYellow = 100;
					tooltipText += ", " + GlobalResources.General.Incident_ShouldHaveStartedOn + " " + String.Format(Dates.FORMAT_DATE, startDate);
				}
			}
			else if (percentComplete < 100)
			{
				//If not complete, show bar to indicate progress
				percentGreen = percentComplete;
				percentGray = 100 - percentComplete;
			}
			else
			{
				//If completed, just show green bar
				percentGreen = 100;
			}
			return tooltipText;
		}

		/// <summary>Calculates the %complete and projected effort columns for an incident data row</summary>
		/// <param name="incident">The incident</param>
		public static void CalculateCompletion(Incident incident)
		{
			int completionPercentage = 0;
			int? projectedEffort;
			CalculateCompletion(incident.EstimatedEffort, incident.ActualEffort, incident.RemainingEffort, out completionPercentage, out projectedEffort);
			incident.CompletionPercent = completionPercentage;
			incident.ProjectedEffort = projectedEffort;
		}

		/// <summary>Calculates the %complete and projected effort</summary>
		/// <param name="estimatedEffort">The estimated effort</param>
		/// <param name="actualEffort">The actual effort</param>
		/// <param name="remainingEffort">The remaining effort</param>
		/// <param name="completionPercentage">The completion percentage (out)</param>
		/// <param name="projectedEffort">The projected effort (out)</param>
		public static void CalculateCompletion(Nullable<int> estimatedEffort, Nullable<int> actualEffort, Nullable<int> remainingEffort, out int completionPercentage, out Nullable<int> projectedEffort)
		{
			//If we have no estimated effort then default to 0% complete
			if (!estimatedEffort.HasValue)
			{
				completionPercentage = 0;
				projectedEffort = null;
				return;
			}
			//Handle the special case of a zero-effort incident
			if (estimatedEffort.Value == 0)
			{
				completionPercentage = 100;
				projectedEffort = 0;
			}
			if (!remainingEffort.HasValue)
			{
				//If we have a remaining effort value not set then assume same as estimated value
				remainingEffort = estimatedEffort.Value;
			}
			//Now we know we have an estimated effort and remaining effort value
			double percentRemaining = ((double)remainingEffort.Value / (double)estimatedEffort.Value) * 100D;
			completionPercentage = (int)(100D - percentRemaining);

			//Now we need to handle the projected effort
			if (actualEffort.HasValue)
			{
				projectedEffort = actualEffort + remainingEffort;
			}
			else
			{
				//If we have no actual hours logged, we don't truly know if this task will take longer or not
				//So for now we shall simply set the projected effort to the estimated effort
				//In future we might be able to calculate it based on the resource dates, etc.
				projectedEffort = estimatedEffort;
			}

			//Make sure %completion is in the range 0% - 100%
			if (completionPercentage < 0)
			{
				completionPercentage = 0;
			}
			if (completionPercentage > 100)
			{
				completionPercentage = 100;
			}
		}

		#endregion

		#region Lookup Retrieves

		/// <summary>Returns a sorted list of values to populate the lookup for the incident progress filter</summary>
		/// <returns>Dictionary containing filter values</returns>
		/// <remarks>Since incidents do not have an 'end-date' currently, we cannot filter on those 'running late'</remarks>
		public Dictionary<string, string> RetrieveProgressFiltersLookup()
		{
			const string METHOD_NAME = "RetrieveProgressFiltersLookup";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the filters list populated, then create, otherwise just return
				if (this.progressFiltersList == null)
				{
					this.progressFiltersList = new Dictionary<string, string>();
					this.progressFiltersList.Add("1", GlobalResources.General.Task_NotStarted);
					this.progressFiltersList.Add("2", GlobalResources.General.Task_StartingLate);
					this.progressFiltersList.Add("3", GlobalResources.General.Task_OnSchedule);
					//this.progressFiltersList.Add("4", GlobalResources.General.Task_RunningLate);
					this.progressFiltersList.Add("5", GlobalResources.General.Task_Completed);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return this.progressFiltersList;
		}

		#endregion

		#region Priority Functions

		/// <summary>Retrieves a list of configured incident priorities for the current project template</summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <param name="activeOnly">Whether to only retrieve active priorities</param>
		/// <returns>List of incident priorities</returns>
		public List<IncidentPriority> RetrieveIncidentPriorities(int projectTemplateId, bool activeOnly)
		{
			const string METHOD_NAME = "RetrieveIncidentPriorities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentPriority> incidentPriorities;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentPriorities
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.PriorityId, i.Name
								select i;

					incidentPriorities = query.OrderByDescending(i=> i.PriorityId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentPriorities;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves an incident priority by its id</summary>
		/// <param name="priorityId">The id of the priority</param>
		/// <returns>incident priority</returns>
		public IncidentPriority RetrieveIncidentPriorityById(int priorityId)
		{
			const string METHOD_NAME = "RetrieveIncidentPriorityById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentPriority incidentPriority;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentPriorities
								where i.PriorityId == priorityId
								select i;

					incidentPriority = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentPriority;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the incident priorities for a project</summary>
		/// <param name="incidentPriority">The incident priority to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void IncidentPriority_Update(IncidentPriority incidentPriority)
		{
			const string METHOD_NAME = "IncidentPriority_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.IncidentPriorities.ApplyChanges(incidentPriority);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new incident priority for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the incident priority belongs to</param>
		/// <param name="name">The display name of the incident priority</param>
		/// <param name="active">Whether the incident priority is active or not</param>
		/// <param name="color">The color code for the priority (in rrggbb hex format)</param>
		/// <returns>The newly created incident priority id</returns>
		public int InsertIncidentPriority(int projectTemplateId, string name, string color, bool active)
		{
			const string METHOD_NAME = "InsertIncidentPriority";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int priorityId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new incident priority
					IncidentPriority incidentPriority = new IncidentPriority();
					incidentPriority.ProjectTemplateId = projectTemplateId;
					incidentPriority.Name = name.MaxLength(20);
					incidentPriority.Color = color.MaxLength(6);
					incidentPriority.IsActive = active;

					context.IncidentPriorities.AddObject(incidentPriority);
					context.SaveChanges();
					priorityId = incidentPriority.PriorityId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return priorityId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Severity Functions

		/// <summary>Inserts a new incident severity for a specific project template</summary>
		/// <param name="projectId">The project template that the incident severity belongs to</param>
		/// <param name="name">The display name of the incident severity</param>
		/// <param name="active">Whether the incident severity is active or not</param>
		/// <param name="color">The color code for the severity (in rrggbb hex format)</param>
		/// <returns>The newly created incident severity id</returns>
		public int InsertIncidentSeverity(int projectTemplateId, string name, string color, bool active)
		{
			const string METHOD_NAME = "InsertIncidentSeverity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int severityId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new incident severity
					IncidentSeverity incidentSeverity = new IncidentSeverity();
					incidentSeverity.ProjectTemplateId = projectTemplateId;
					incidentSeverity.Name = name.MaxLength(20);
					incidentSeverity.Color = color.MaxLength(6);
					incidentSeverity.IsActive = active;

					context.IncidentSeverities.AddObject(incidentSeverity);
					context.SaveChanges();
					severityId = incidentSeverity.SeverityId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return severityId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the incident severities for a project</summary>
		/// <param name="incidentSeverity">The incident severity to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void IncidentSeverity_Update(IncidentSeverity incidentSeverity)
		{
			const string METHOD_NAME = "IncidentSeverity_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.IncidentSeverities.ApplyChanges(incidentSeverity);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves an incident severity by its id</summary>
		/// <param name="severityId">The id of the severity</param>
		/// <returns>incident severity</returns>
		public IncidentSeverity RetrieveIncidentSeverityById(int severityId)
		{
			const string METHOD_NAME = "RetrieveIncidentSeverityById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentSeverity incidentSeverity;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentSeverities
								where i.SeverityId == severityId
								select i;

					incidentSeverity = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentSeverity;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of configured incident severities for the current project template</summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <param name="activeOnly">Whether to only retrieve active severities</param>
		/// <returns>List of incident severities</returns>
		public List<IncidentSeverity> RetrieveIncidentSeverities(int projectTemplateId, bool activeOnly)
		{
			const string METHOD_NAME = "RetrieveIncidentSeverities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentSeverity> incidentSeverities;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentSeverities
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.SeverityId, i.Name
								select i;

					incidentSeverities = query.OrderByDescending(i => i.SeverityId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentSeverities;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Incident Type Functions

		/// <summary>Retrieves a list of incident types for the current project template</summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <param name="activeOnly">Whether to only retrieve active types</param>
		/// <returns>List of incident types</returns>
		public List<IncidentType> RetrieveIncidentTypes(int projectTemplateId, bool activeOnly)
		{
			const string METHOD_NAME = "RetrieveIncidentTypes";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentType> incidentTypes;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentTypes
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.IncidentTypeId, i.Name
								select i;

					incidentTypes = query.OrderByDescending(i => i.IncidentTypeId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a specific incident type</summary>
		/// <param name="incidentTypeId">The incident type record to retrieve</param>
		/// <returns>The incident type</returns>
		public IncidentType RetrieveIncidentTypeById(int incidentTypeId)
		{
			const string METHOD_NAME = "RetrieveIncidentTypeById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				IncidentType incidentType;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentTypes
								where i.IncidentTypeId == incidentTypeId
								select i;

					incidentType = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentType;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new incident type for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the incident type belongs to</param>
		/// <param name="name">The display name of the incident type</param>
		/// <param name="active">Whether the incident type is active or not</param>
		/// <param name="issue">Is this incident type considered a project issue</param>
		/// <param name="risk">Is this incident type considered a project risk</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <param name="defaultType">Is this the default (initial) type of newly created incidents</param>
		/// <returns>The newly created incident type id</returns>
		public int InsertIncidentType(int projectTemplateId, string name, int? workflowId, bool issue, bool risk, bool defaultType, bool active)
		{
			const string METHOD_NAME = "InsertIncidentType";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					WorkflowManager workflowManager = new WorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).WorkflowId;
				}

				int incidentTypeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					IncidentType incidentType = new IncidentType();
					incidentType.ProjectTemplateId = projectTemplateId;
					incidentType.Name = name.MaxLength(20);
					incidentType.IsDefault = defaultType;
					incidentType.IsActive = active;
					incidentType.IsIssue = issue;
					incidentType.IsRisk = risk;
					incidentType.WorkflowId = workflowId.Value;

					context.IncidentTypes.AddObject(incidentType);
					context.SaveChanges();
					incidentTypeId = incidentType.IncidentTypeId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentTypeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="incidentId">The id of the incident</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int incidentId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				IncidentView notificationArt = RetrieveById2(incidentId);
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

		/// <summary>Gets the default (i.e. initial) incident type for all newly created incidents</summary>
		/// <param name="projectTemplateId">The project template we're interested in</param>
		/// <returns>The incident type id</returns>
		public int GetDefaultIncidentType(int projectTemplateId)
		{
			const string METHOD_NAME = "GetDefaultIncidentType";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentTypeId = -1;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from i in context.IncidentTypes
								where i.ProjectTemplateId == projectTemplateId && i.IsDefault
								select i;

					IncidentType incidentType = query.FirstOrDefault();
					if (incidentType != null)
					{
						incidentTypeId = incidentType.IncidentTypeId;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentTypeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the risk incident type IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> IncidentType_RetrieveRiskIds(int projectTemplateId)
		{
			const string METHOD_NAME = "IncidentType_RetrieveRiskIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> typeIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentTypes
								where i.ProjectTemplateId == projectTemplateId && i.IsRisk && i.IsActive
								orderby i.IncidentTypeId
								select i.IncidentTypeId;

					typeIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return typeIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the issue incident type IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> IncidentType_RetrieveIssueIds(int projectTemplateId)
		{
			const string METHOD_NAME = "IncidentType_RetrieveIssueIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> typeIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.IncidentTypes
								where i.ProjectTemplateId == projectTemplateId && i.IsIssue && i.IsActive
								orderby i.IncidentTypeId
								select i.IncidentTypeId;

					typeIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return typeIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the incident types for a project</summary>
		/// <param name="incidentType">The incident type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void IncidentType_Update(IncidentType incidentType)
		{
			const string METHOD_NAME = "IncidentType_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.IncidentTypes.ApplyChanges(incidentType);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Project Group Functions

		/// <summary>Retrieves a list of all defects in the project group along with associated meta-data</summary>
		/// <param name="projectGroupId">The project group we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="includeDeleted">Whether to include deleted items in the list or not.</param>
		/// <returns>Incident List</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		public List<IncidentView> Incident_RetrieveForGroup(int projectGroupId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<IncidentView> incidents;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.IncidentsView
								where (!i.IsDeleted || includeDeleted) && i.ProjectGroupId == projectGroupId && i.ProjectIsActive
								select i;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by creation date descending
						query = query.OrderByDescending(i => i.CreationDate).ThenBy(i => i.IncidentId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "IncidentId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleIncidentSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<IncidentView, bool>> filterClause = CreateFilterExpression<IncidentView>(null, null, Artifact.ArtifactTypeEnum.Incident, filters, utcOffset, null, HandleIncidentSpecificFiltersForGroup);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<IncidentView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						return new List<IncidentView>();
					}

					//Execute the query
					incidents = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidents;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the incidents in the project group</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectGroupId">The project we're interested in</param>
		/// <param name="includeDeleted">True if you want the count to return deleted incidents as well.</param>
		/// <returns>The total number of Incidents</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Incident_CountForGroup(int projectGroupId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Incident_CountForGroup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int incidentCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.IncidentsView
								where (!i.IsDeleted || includeDeleted) && i.ProjectGroupId == projectGroupId && i.ProjectIsActive
								select i;

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleIncidentSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<IncidentView, bool>> filterClause = CreateFilterExpression<IncidentView>(null, null, Artifact.ArtifactTypeEnum.Incident, filters, utcOffset, null, HandleIncidentSpecificFiltersForGroup);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<IncidentView>)query.Where(filterClause);
						}
					}

					//Get the count
					incidentCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return incidentCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of columns to display in the incident list for a specific project group / user with a visible/hidden flag
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
				//We exclude components because they are per-project and not part of the view since multi-select
				List<ArtifactField> artifactListFields = new ArtifactManager().ArtifactField_RetrieveForLists(Artifact.ArtifactTypeEnum.Incident);
				foreach (ArtifactField artifactListField in artifactListFields.Where(af => af.Name != "ComponentIds"))
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

				//Also add the additional column for 'Project' since we have a column for the project name
				{
					ArtifactListFieldDisplay artifactField = new ArtifactListFieldDisplay();
					artifactField.Name = "ProjectId";
					artifactField.ArtifactFieldTypeId = (int)Artifact.ArtifactFieldTypeEnum.Lookup;
					artifactField.Caption = "Project";  //Will be localized later in the web page
					artifactField.LookupProperty = "ProjectName";
					artifactField.IsVisible = true;
					artifactFields.Add(artifactField);

					//See if we have a column visibility setting specified
					if (columnSettings[artifactField.Name] != null && columnSettings[artifactField.Name] is bool)
					{
						artifactField.IsVisible = (bool)columnSettings[artifactField.Name];
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
					ArtifactField artifactField = new ArtifactManager().ArtifactField_RetrieveByName(Artifact.ArtifactTypeEnum.Incident, fieldName);
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
	}

	/// <summary>
	/// This exception is thrown when you try and perform certain operations on an incident that is in a closed
	/// status. You can't make certain changes in a closed status.
	/// </summary>
	public class IncidentClosedException : ApplicationException
	{
		public IncidentClosedException()
		{
		}
		public IncidentClosedException(string message)
			: base(message)
		{
		}
		public IncidentClosedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
