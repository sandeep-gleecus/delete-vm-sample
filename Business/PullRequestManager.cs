using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Objects;
using System.Reflection;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>This class encapsulates all the data access functionality for working with pull requests</summary>
    public class PullRequestManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.PullRequestManager::";

        #region PullRequest functions

        public int PullRequest_Create(int projectId, string name, int creatorId, string sourceBranchName, string destBranchName, int? releaseId, int? ownerId)
        {
            const string METHOD_NAME = "PullRequest_Create";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                int pullRequestId;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First make sure the two branches exist
                    var query1 = from b in context.VersionControlBranches
                                 where b.Name == sourceBranchName && b.ProjectId == projectId
                                 select b;
                    var query2 = from b in context.VersionControlBranches
                                 where b.Name == destBranchName && b.ProjectId == projectId
                                 select b;

                    //Retrieve the branches and verify they exist
                    VersionControlBranch sourceBranch = query1.FirstOrDefault();
                    VersionControlBranch destBranch = query2.FirstOrDefault();
                    if (sourceBranch == null)
                    {
                        throw new ArtifactNotExistsException(String.Format("Branch '{0}' does not exist in project PR{1}.", sourceBranchName, projectId));
                    }
                    if (destBranch == null)
                    {
                        throw new ArtifactNotExistsException(String.Format("Branch '{0}' does not exist in project PR{1}.", destBranchName, projectId));
                    }

                    //Get the task type for pull requests
                    TaskManager taskManager = new TaskManager();
                    List<TaskType> types = taskManager.TaskType_Retrieve(projectTemplateId);
                    TaskType type = types.FirstOrDefault(t => t.IsPullRequest);
                    if (type == null)
                    {
                        throw new ArtifactNotExistsException(String.Format("Unable to find a Pull Request task type in project PR{0} so cannot continue!", projectId));
                    }

                    //First insert the new item into the task list in the root folder
                    pullRequestId = taskManager.Insert(
                        projectId,
                        creatorId,
                        Task.TaskStatusEnum.NotStarted,
                        type.TaskTypeId,
                        null,
                        null,
                        releaseId,
                        ownerId,
                        null,
                        name,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        creatorId,
                        true
                        );

                    //We now need to populate the appropriate default custom properties
                    CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                    Task task = taskManager.RetrieveById(pullRequestId);
                    ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, pullRequestId, Artifact.ArtifactTypeEnum.Task, true);
                    if (task != null)
                    {
                        //If the artifact custom property row is null, create a new one and populate the defaults
                        if (artifactCustomProperty == null)
                        {
                            List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, false);
                            artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, Artifact.ArtifactTypeEnum.Task, pullRequestId, customProperties);
                            artifactCustomProperty = customPropertyManager.CustomProperty_PopulateDefaults(projectTemplateId, artifactCustomProperty);
                        }

                        //Save the custom properties
                        customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, creatorId);
                    }

                    //Now link this new pull request task to the branches
                    VersionControlPullRequest vcpq = new VersionControlPullRequest();
                    vcpq.TaskId = pullRequestId;
                    vcpq.SourceBranchId = sourceBranch.BranchId;
                    vcpq.DestBranchId = destBranch.BranchId;
                    context.VersionControlPullRequests.AddObject(vcpq);
                    context.SaveChanges();

					//Send a creation notification
					taskManager.SendCreationNotification(pullRequestId, null, null);
				}

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return pullRequestId;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

		/// <summary>
		/// Retrieves a pull request view by its ID
		/// </summary>
		/// <param name="taskId"></param>
		public PullRequest PullRequest_RetrieveById(int taskId)
        {
            const string METHOD_NAME = "PullRequest_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                PullRequest pullRequest;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.PullRequests
                                where t.TaskId == taskId && !t.IsDeleted
                                select t;

                    pullRequest = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return pullRequest;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a list of all pull requests in a project</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="startRow">The first row to retrieve (starting at 1)</param>
        /// <param name="numberOfRows">The number of rows to retrieve</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>PullRequest list</returns>
        public List<PullRequest> PullRequest_Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset)
        {
            const string METHOD_NAME = "PullRequest_Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<PullRequest> pullRequests;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.PullRequests
                                where t.ProjectId == projectId
                                select t;

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by last updated date descending
                        query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TaskId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TaskId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<PullRequest, bool>> filterClause = CreateFilterExpression<PullRequest>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, filters, utcOffset, null, HandlePullRequestSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<PullRequest>)query.Where(filterClause);
                        }
                    }

                    //Get the count
                    int artifactCount = query.Count();

                    //Make pagination is in range
                    if (startRow < 1 || startRow > artifactCount)
                    {
                        startRow = 1;
                    }

                    //Execute the query
                    pullRequests = query
                        .Skip(startRow - 1)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return pullRequests;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

		/// <summary>Handles any pull request specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandlePullRequestSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of release filters where we want to also retrieve child iterations
			if (filterProperty == "ReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "ReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>Counts all the pull requests in the project</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>The total number of pull requests</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int PullRequest_Count(int projectId, Hashtable filters, double utcOffset)
        {
            const string METHOD_NAME = "PullRequest_Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int count = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.PullRequests
                                where t.ProjectId == projectId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<PullRequest, bool>> filterClause = CreateFilterExpression<PullRequest>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Task, filters, utcOffset, null, null);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<PullRequest>)query.Where(filterClause);
                        }
                    }

                    //Get the count
                    count = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return count;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        #endregion

        #region Task methods

        /// <summary>Retrieves a particular task entity by its ID, including branch relationships</summary>
        /// <param name="taskId">The ID of the task we want to retrieve</param>
        /// <returns>A task entity</returns>
        public Task Task_RetrieveById(int taskId)
        {
            const string METHOD_NAME = "Task_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Task task;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the query for retrieving the task entity
                    var query = from t in context.Tasks
                                    .Include(t => t.PullRequests)
                                where t.TaskId == taskId && !t.IsDeleted
                                select t;

                    task = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the task
                return task;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Updates a pull request task that is passed-in</summary>
        /// <param name="task">The task to be persisted</param>
        /// <param name="userId">The user making the change</param>
        /// <param name="isRollback">Whether the update is a rollback or not. Default: FALSE</param>
        /// <param name="rollbackId">Whether or not to update history. Default: TRUE</param>
        /// <remarks>
        /// Just a wrapper around the TaskManager.Update() method
        /// </remarks>
        /// <seealso cref="TaskManager.Update(Task, int, long?, bool)"/>
        public void Task_Update(Task task, int userId, long? rollbackId = null, bool updHist = true)
        {
            //First we need to make sure the associated pull request is handled OK
            using (SpiraTestEntities context = new SpiraTestEntities())
            {
                if (task.PullRequests.Count > 0)
                {
                    //We need to remove the existing pull request link record and then add the new one
                    var query = from p in context.VersionControlPullRequests
                                where p.TaskId == task.TaskId
                                select p;

                    VersionControlPullRequest request = query.FirstOrDefault();
                    if (request != null)
                    {
                        context.VersionControlPullRequests.DeleteObject(request);
                    }

                    //Now add the new version
                    VersionControlPullRequest updatedRequest = task.PullRequests.FirstOrDefault();
                    updatedRequest.MarkAsAdded();
                    context.SaveChanges();
                }
            }
            //Now update the other task fields
            new TaskManager().Update(task, userId, rollbackId, updHist);
        }

		/// <summary>
		/// Copies over the branch information between two tasks (source and dest)
		/// </summary>
		/// <param name="taskId">The source task</param>
		/// <param name="copiedTaskId">The destination task</param>
		protected internal void CopyBranchInfo(int taskId, int copiedTaskId)
		{
			const string METHOD_NAME = "CopyBranchInfo";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.VersionControlPullRequests
								where p.TaskId == taskId
								select p;

					VersionControlPullRequest sourcePullRequest = query.FirstOrDefault();
					if (sourcePullRequest != null)
					{
						VersionControlPullRequest copiedPullRequest = new VersionControlPullRequest();
						copiedPullRequest.TaskId = copiedTaskId;
						copiedPullRequest.SourceBranchId = sourcePullRequest.SourceBranchId;
						copiedPullRequest.DestBranchId = sourcePullRequest.DestBranchId;
						context.VersionControlPullRequests.AddObject(copiedPullRequest);
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

		#endregion
	}
}
