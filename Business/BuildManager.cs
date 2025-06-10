using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Collections;
using System.Data.Objects;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class encapsulates all the data access functionality for
    /// reading and writing builds in the system
    /// </summary>
    public class BuildManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.BuildManager::";

        /// <summary>
        /// Returns the number of revisions associated with the project/build
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="buildId">The id of the build</param>
        /// <returns>The number of revisions</returns>
        public int CountRevisions(int projectId, int buildId)
        {
            const string METHOD_NAME = "CountRevisions";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int count = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.BuildSourceCodes.Include("Build")
                                where !b.Build.IsDeleted && b.BuildId == buildId && b.Build.ProjectId == projectId
                                select b;

                    //Now execute the query to get the count
                    count = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return count;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Returns the number of builds in the project/release
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release</param>
        /// <returns>The number of builds</returns>
        public int CountForRelease(int projectId, int releaseId)
        {
            const string METHOD_NAME = "CountForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the release and its child iterations
                List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);

                int count = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.Builds
                                where !b.IsDeleted && releaseIds.Contains(b.ReleaseId) && b.ProjectId == projectId
                                select b;


                    //Now execute the query to get the count
                    count = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return count;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the list of build statuses
        /// </summary>
        /// <param name="includeInactive">Include inactive statuses</param>
        /// <returns>The list of statuses</returns>
        public List<BuildStatus> RetrieveStatuses(bool includeInactive = false)
        {
            const string METHOD_NAME = "RetrieveStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<BuildStatus> statusList;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.BuildStatuses
                                where (includeInactive || b.IsActive)
                                orderby b.Name ascending
                                select b;

                    statusList = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return statusList;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all the most recent builds for all the active projects in the specified project group
        /// </summary>
        /// <param name="projectGroupId">The id of the project group the builds are in</param>
        /// <param name="groupByActiveReleases">
        /// Do we want to just get the most recent results for the projects, or the latest for the actual releases
        /// </param>
        /// <returns>The list of builds by project</returns>
        public List<BuildView> RetrieveForProjectGroup(int projectGroupId, bool groupByActiveReleases)
        {
            const string METHOD_NAME = "RetrieveForProjectGroup";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<BuildView> builds;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See if we need to group by product or active release
                    if (groupByActiveReleases)
                    {
                        //Based on https://smehrozalam.wordpress.com/2009/12/29/linq-how-to-get-the-latest-last-record-with-a-group-by-clause/
                        var query = from b in context.BuildsView
                                    join r in context.Releases on b.ReleaseId equals r.ReleaseId
                                    where
                                        !b.IsDeleted &&
                                        b.ProjectGroupId == projectGroupId &&
                                        b.ProjectIsActive &&
                                        (r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Planned
                                            || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.InProgress
                                            || r.ReleaseStatusId == (int)Release.ReleaseStatusEnum.Completed)
                                    group b by b.ReleaseId into grp
                                    let mostRecentBuildDatePerRelease = grp.Max(g => g.CreationDate)
                                    from b in grp
                                    where
                                        b.CreationDate == mostRecentBuildDatePerRelease
                                    orderby b.ProjectName, b.CreationDate descending, b.BuildId
                                    select b;
                        builds = query.ToList();
                    }
                    else
                    {
                        //Based on https://smehrozalam.wordpress.com/2009/12/29/linq-how-to-get-the-latest-last-record-with-a-group-by-clause/
                        var query = from b in context.BuildsView
                                    where !b.IsDeleted && b.ProjectGroupId == projectGroupId && b.ProjectIsActive
                                    group b by b.ProjectId into grp
                                    let mostRecentBuildDatePerProject = grp.Max(g => g.CreationDate)
                                    from b in grp
                                    where b.CreationDate == mostRecentBuildDatePerProject
                                    orderby b.CreationDate descending, b.BuildId
                                    select b;
                        builds = query.ToList();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return builds;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of all the builds in the project (used for lookups)
        /// </summary>
        /// <param name="projectId">The id of the project the builds are in</param>
        /// <param name="includeDeleted">Should we include deleted builds [Optional]</param>
        /// <returns>The list of builds</returns>
        public List<BuildView> RetrieveForProject(int projectId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveForProject";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<BuildView> builds;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.BuildsView
                                where (!b.IsDeleted || includeDeleted) && b.ProjectId == projectId
                                orderby b.Name, b.BuildId
                                select b;
                    builds = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return builds;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Adds a new source code revision association to an existing build
        /// </summary>
        /// <param name="buildId">The id of the build</param>
        /// <param name="revisionKey">The revision key</param>
        /// <param name="creationDate">The creation date</param>
        /// <returns>The populated build source code object</returns>
        public BuildSourceCode InsertSourceCodeRevision(int buildId, string revisionKey, DateTime creationDate)
        {
            const string METHOD_NAME = "InsertSourceCodeRevision";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First make sure the build exists in this project
                int projectId;
                try
                {
                    Build build = RetrieveById(buildId);
                    projectId = build.ProjectId;
                }
                catch (ArtifactNotExistsException)
                {
                    //The build doesn't exist, so throw an exception
                    throw new EntityForeignKeyException(String.Format(GlobalResources.Messages.BuildManager_BuildNotExists, buildId));
                }

                BuildSourceCode buildSourceCode;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new buildSourceCode entity
                    buildSourceCode = new BuildSourceCode();
                    buildSourceCode.BuildId = buildId;
                    buildSourceCode.RevisionKey = revisionKey;
                    buildSourceCode.CreationDate = creationDate;

                    //Persist the new entity
                    context.AddObject("BuildSourceCodes", buildSourceCode);
                    context.SaveChanges();
                }

                //Attempt to retrieve one of the revisions, so that a cache refresh is kicked off if needed
                try
                {
                    new SourceCodeManager(projectId).RetrieveRevisionByKey(revisionKey);
                }
                catch (Exception)
                {
                    //Do nothing
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return buildSourceCode;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds a new build to an iteration in the project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release</param>
        /// <param name="name">The name of the build</param>
        /// <param name="description">The description of the build</param>
        /// <param name="creationDate">The creation date of the build</param>
        /// <param name="status">The status of the build</param>
        /// <returns>The build object with its ID populated</returns>
        /// <param name="userId">The id of the user recording the new build</param>
        public Build Insert(int projectId, int releaseId, string name, string description, DateTime creationDate, Build.BuildStatusEnum status, int userId)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First make sure the release exists in this project
                ReleaseManager release = new ReleaseManager();
                try
                {
                    release.RetrieveById(UserManager.UserInternal, projectId, releaseId);
                }
                catch (ArtifactNotExistsException)
                {
                    //The release doesn't exist, so throw an exception
                    throw new EntityForeignKeyException(String.Format(GlobalResources.Messages.BuildManager_ReleaseNotExists, releaseId));
                }

                Build build;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
					//Create a new build entity
					build = new Build();
                    build.ProjectId = projectId;
                    build.ReleaseId = releaseId;
                    build.Name = name;
                    build.Description = description;
                    build.CreationDate = creationDate;
                    build.BuildStatusId = (int)status;
                    build.IsDeleted = false;
                    build.LastUpdateDate = DateTime.UtcNow;

					//Persist the new entity
					context.AddObject("Builds", build);
					context.SaveChanges();
				}

                //Now we need to see if there are any auto-scheduled test sets to update, only on success
                if (status == Build.BuildStatusEnum.Succeeded)
                {
                    new TestSetManager().AutoScheduleTestSets(projectId, releaseId, userId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return build;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Marks all the builds in a release as deleted
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release</param>
        public void MarkAsDeletedForRelease (int projectId, int releaseId)
        {
            const string METHOD_NAME = "MarkAsDeletedForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.Builds
                                where b.ReleaseId == releaseId && b.ProjectId == projectId && !b.IsDeleted
                                select b;
                    List<Build> builds = query.ToList();
                    foreach (Build build in builds)
                    {
                        build.StartTracking();
                        build.IsDeleted = true;
                    }

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
        /// Marks all the builds in a release as not-deleted
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release</param>
        public void UnDeleteForRelease(int projectId, int releaseId)
        {
            const string METHOD_NAME = "UnDeleteForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.Builds
                                where b.ReleaseId == releaseId && b.ProjectId == projectId && b.IsDeleted
                                select b;
                    List<Build> builds = query.ToList();
                    foreach (Build build in builds)
                    {
                        build.StartTracking();
                        build.IsDeleted = false;
                    }
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
        /// Gets the list of the builds in the project/release without any filters and using the default sort
        /// </summary>
        /// <param name="projectId">The id of the project the builds are in</param>
        /// <param name="releaseId">The id of the release the builds belong to</param>
        /// <param name="includeDeleted">Should we include deleted builds [Optional]</param>
        /// <returns>The list of builds</returns>
        public List<BuildView> RetrieveForRelease(int projectId, int releaseId, double utcOffset, bool includeDeleted = false)
        {
            int count;
            return RetrieveForRelease(projectId, releaseId, "Name ASC", 0, Int32.MaxValue, null, utcOffset, out count, includeDeleted);
        }

        /// <summary>
        /// Gets the list of builds in the project/release that match the provided filter and are sorted by the provided sort
        /// </summary>
        /// <param name="projectId">The id of the project the builds are in</param>
        /// <param name="releaseId">The id of the release the builds belong to</param>
        /// <param name="sortExpression">The sort expression in the format [PropertyName] [ASC|DESC]</param>
        /// <param name="pageIndex">The start index</param>
        /// <param name="pageSize">The pagination size</param>
        /// <param name="filterList">The list of filter</param>
        /// <param name="artifactCount">The total number of builds that match the filter</param>
        /// <param name="includeDeleted">Should we include deleted builds [Optional]</param>
        /// <param name="utcOffset">The current timezone offset</param>
        /// <returns>The list of builds</returns>
        /// <remarks>Includes the child iterations for a release</remarks>
        public List<BuildView> RetrieveForRelease(int projectId, int releaseId, string sortExpression, int pageIndex, int pageSize, Hashtable filterList, double utcOffset, out int artifactCount, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the release and its child iterations
                List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId, releaseId, includeDeleted);

                List<BuildView> builds;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.BuildsView
                                where (!b.IsDeleted || includeDeleted) && releaseIds.Contains(b.ReleaseId) && b.ProjectId == projectId
                                select b;

                    //Add the dynamic filters
                    if (filterList != null)
                    {
                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<BuildView, bool>> filterClause = CreateFilterExpression<BuildView>(projectId, null, Artifact.ArtifactTypeEnum.None, filterList, utcOffset);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<BuildView>)query.Where(filterClause);
                        }
                    }

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortExpression))
                    {
                        //Default to sorting by creation date descending
                        query = query.OrderByDescending(b => b.CreationDate);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        query = query.OrderUsingSortExpression(sortExpression, "BuildId");
                    }

                    //Now execute the query to get the count and paginated results
                    artifactCount = query.Count();

                    //Make pagination is in range
                    if (pageIndex < 0)
                    {
                        pageIndex = 0;
                    }
                    if (pageIndex > artifactCount - 1)
                    {
                        pageIndex = (int)artifactCount - pageSize;
                        if (pageIndex < 0)
                        {
                            pageIndex = 0;
                        }
                    }

                    //If we have no matching results, return empty list
                    if (artifactCount > 0)
                    {
                        builds = query
                            .Skip(pageIndex)
                            .Take(pageSize)
                            .ToList();
                    }
                    else
                    {
                        return new List<BuildView>();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return builds;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of builds in the project/release that match the provided filter and are sorted by the provided sort
        /// </summary>
        /// <param name="projectId">The id of the project the builds are in</param>
        /// <param name="sortExpression">The sort expression in the format [PropertyName] [ASC|DESC]</param>
        /// <param name="pageIndex">The start index</param>
        /// <param name="pageSize">The pagination size</param>
        /// <param name="filterList">The list of filter</param>
        /// <param name="artifactCount">The total number of builds that match the filter</param>
        /// <param name="includeDeleted">Should we include deleted builds [Optional]</param>
        /// <returns>The list of builds</returns>
        /// <remarks>Includes the child iterations for a release</remarks>
        public List<BuildView> RetrieveForProject(int projectId, string sortExpression, int pageIndex, int pageSize, Hashtable filterList, out int artifactCount, double utcOffset, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveForProject";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<BuildView> builds;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.BuildsView
                                where (!b.IsDeleted || includeDeleted) && b.ProjectId == projectId
                                select b;

                    //Add the dynamic filters
                    if (filterList != null)
                    {
                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<BuildView, bool>> filterClause = CreateFilterExpression<BuildView>(projectId, null, Artifact.ArtifactTypeEnum.None, filterList, utcOffset);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<BuildView>)query.Where(filterClause);
                        }
                    }

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortExpression))
                    {
                        //Default to sorting by creation date descending
                        query = query.OrderByDescending(b => b.CreationDate);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        query = query.OrderUsingSortExpression(sortExpression, "BuildId");
                    }

                    //Now execute the query to get the count and paginated results
                    artifactCount = query.Count();

                    //Make pagination is in range
                    if (pageIndex > artifactCount - 1)
                    {
                        pageIndex = (int)artifactCount - pageSize;
                    }
                    if (pageIndex < 0)
                    {
                        pageIndex = 0;
                    }

                    //If we have no matching results, return empty list
                    if (artifactCount > 0)
                    {
                        builds = query
                            .Skip(pageIndex)
                            .Take(pageSize)
                            .ToList();
                    }
                    else
                    {
                        return new List<BuildView>();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return builds;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Gets the list of source code revisions that are part of the specified build
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="buildId">The id of the build</param>
        /// <returns>The list of articles</returns>
        public List<BuildSourceCode> RetrieveRevisionsForBuild(int projectId, int buildId)
        {
            const string METHOD_NAME = "RetrieveRevisionsForBuild";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<BuildSourceCode> revisions;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from b in context.BuildSourceCodes.Include("Build")
                                where !b.Build.IsDeleted && b.BuildId == buildId && b.Build.ProjectId == projectId
                                orderby b.BuildId, b.RevisionKey
                                select b;

                    revisions = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return revisions;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of Builds that include a specific revision
        /// </summary>
        /// <param name="projectId">The project id</param>
        /// <param name="revisionKey">The revision key</param>
        /// <returns>List of builds</returns>
        public List<Build> RetrieveForRevision(int projectId, string revisionKey)
        {
            const string METHOD_NAME = "RetrieveForRevision";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Build> builds;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.Builds
                                    .Include(b => b.SourceCodeRevisions)
                                    .Include(b => b.Status)
                                where b.ProjectId == projectId && b.SourceCodeRevisions.Any(s => s.RevisionKey == revisionKey)
                                select b;
                    builds = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return builds;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a Build by its id
        /// </summary>
        /// <param name="buildId">The build id</param>
        /// <param name="includeDeleted">Should we retrieve a deleted item</param>
        /// <returns>Build object</returns>
        public Build RetrieveById(int buildId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Build build;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from b in context.Builds.Include("Status")
                                where b.BuildId == buildId && (!b.IsDeleted || includeDeleted)
                                select b;
                    build = query.FirstOrDefault();
                }

                //Make sure data was returned
                if (build == null)
                {
                    throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.BuildManager_BuildNotExists, buildId));
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return build;
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }
    }
}
