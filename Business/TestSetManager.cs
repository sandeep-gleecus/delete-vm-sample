using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Objects;
using System.Collections;
using System.Linq.Expressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Transactions;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class encapsulates all the data access functionality for
    /// reading and writing Test Sets that are created and managed in the system
    /// </summary>
    public class TestSetManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TestSetManager::";

        public const int TEST_SET_FOLDER_ID_ALL_TEST_SETS = -999;

        #region Sub Classes

        /// <summary>
        /// A single test set entry for lookups/dropdowns
        /// </summary>
        public class TestSetLookupEntry
        {
            public int TestSetId { get; set; }
            public string Name { get; set; }
            public bool IsFolder { get; set; }
            public string IndentLevel { get; set; }
        }

        #endregion

        //Cached lists
        public static List<TestSetStatus> _staticTestSetStatuses = null;
        public static List<Recurrence> _staticRecurrences = null;

        protected static SortedList<int, string> executionStatusFiltersList = new SortedList<int, string>()
		{
			{1, "=  0% Run"},
			{2, "<= 50% Run"},
			{3, "<  100% Run"},
			{4, ">  0% Passed"},
			{5, ">= 50% Passed"},
			{6, "=  100% Passed"},
			{7, ">  0% Failed"},
			{8, ">= 50% Failed"},
			{9, "=  100% Failed"},
			{10, ">  0% Caution"},
			{11, ">= 50% Caution"},
			{12, "=  100% Caution"},
			{13, ">  0% Blocked"},
			{14, ">= 50% Blocked"},
			{15, "=  100% Blocked"}
		};

        #region Test Set parameter functions

        /// <summary>
        /// Adds a test set parameter
        /// </summary>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        /// <param name="newValue">The value for the parameter</param>
        public void AddTestSetParameter(int testSetId, int testCaseParameterId, string newValue, int? projectId = null,int? userId = null)
        {
            const string METHOD_NAME = "AddTestSetParameter";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Make sure it doesn't already exist
                    var query = from p in context.TestSetParameters
                                where p.TestSetId == testSetId && p.TestCaseParameterId == testCaseParameterId
                                select p;

                    TestSetParameter testSetParameter = query.FirstOrDefault();
                    if (testSetParameter == null)
                    {
                        //Add the parameter value
                        testSetParameter = new TestSetParameter();
                        testSetParameter.TestSetId = testSetId;
                        testSetParameter.TestCaseParameterId = testCaseParameterId;
                        testSetParameter.Value = newValue;
                        context.TestSetParameters.AddObject(testSetParameter);
                        context.SaveChanges();

						new HistoryManager().LogCreation((int)projectId, (int)userId, Artifact.ArtifactTypeEnum.TestSet, (int)testSetId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestSetParameter, (int)testSetParameter.TestCaseParameterId);
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
        /// Updates a test set parameter
        /// </summary>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        /// <param name="newValue">The new value for the parameter</param>
        public void UpdateTestSetParameter(int testSetId, int testCaseParameterId, string newValue, int? userId = null)
        {
            const string METHOD_NAME = "UpdateTestSetParameter";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from p in context.TestSetParameters
                                where p.TestSetId == testSetId && p.TestCaseParameterId == testCaseParameterId
                                select p;

                    TestSetParameter testSetParameter = query.FirstOrDefault();
                    if (testSetParameter != null)
                    {
                        //Update the parameter value
                        testSetParameter.StartTracking();
                        testSetParameter.Value = newValue;
						//context.SaveChanges();
						context.SaveChanges(userId, true, false, null);
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
        /// Deletes a test set parameter
        /// </summary>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testCaseParameterId">The id of the test case parameter</param>
        public void DeleteTestSetParameter(int testSetId, int testCaseParameterId, int? projectId = null, int? userId = null)
        {
            const string METHOD_NAME = "DeleteTestSetParameter";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from p in context.TestSetParameters
                                where p.TestSetId == testSetId && p.TestCaseParameterId == testCaseParameterId
                                select p;

                    TestSetParameter testSetParameter = query.FirstOrDefault();
                    if (testSetParameter != null)
                    {
                        //Delete the parameter value
                        context.TestSetParameters.DeleteObject(testSetParameter);
                        context.SaveChanges();

						new HistoryManager().LogDeletion((int)projectId, (int)userId, Artifact.ArtifactTypeEnum.TestSet, (int)testSetId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestSetParameter, (int)testSetParameter.TestCaseParameterId, testSetParameter.Value);

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
        /// Retrieves a list of test case parameters and values for the test set
        /// </summary>
        /// <param name="testSetId">The ID of the test set</param>
        /// <returns>List of test case parameters and test set values</returns>
        public List<TestSetParameter> RetrieveParameterValues(int testSetId)
        {
            const string METHOD_NAME = "RetrieveParameterValues";


            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetParameter> testSetParameters;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetParameters.Include(t => t.TestCaseParameter)
                                where t.TestSetId == testSetId
                                select t;

                    testSetParameters = query.ToList();
                }

                //Resort by name (in-memory)
                testSetParameters = testSetParameters.OrderBy(t => t.Name).ToList();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetParameters;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of test case parameters and values for a specific test case parameter - useful to check if a parameter is in use by any test sets
        /// </summary>
        /// <param name="testSetId">The ID of the test set</param>
        /// <returns>List of test case parameters and test set values</returns>
        public List<TestSetParameter> RetrieveParameterValuesByParameter(int testCaseParameterId)
        {
            const string METHOD_NAME = "RetrieveParameterValuesByParameter";


            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetParameter> testSetParameters;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetParameters.Include(t => t.TestCaseParameter)
                                where t.TestCaseParameterId == testCaseParameterId
                                select t;

                    testSetParameters = query.ToList();
                }

                //Resort by name (in-memory)
                testSetParameters = testSetParameters.OrderBy(t => t.Name).ToList();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetParameters;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Removes all the test case parameter values from a test set test case instance
        /// </summary>
        /// <param name="testSetTestCaseId">The id of the test set test case</param>
        /// <remarks>We cannot use SaveTestCaseParameterValues() because we don't have any test set test case id field </remarks>
        public void RemoveTestCaseParameterValues(int testSetTestCaseId)
        {
            const string METHOD_NAME = "RemoveTestCaseParameterValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Now retrieve the parameters in the database
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from p in context.TestSetTestCaseParameters
                                where p.TestSetTestCaseId == testSetTestCaseId
                                select p;

                    List<TestSetTestCaseParameter> parameterValuesInDb = query.ToList();

                    //Perform the delete
                    foreach (TestSetTestCaseParameter parameterToDelete in parameterValuesInDb)
                    {
                        context.TestSetTestCaseParameters.DeleteObject(parameterToDelete);
                    }

                    //Save all the changes
                    context.SaveChanges();
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
		/// Updates a list of TestSet-TestCase parameter values
		/// This updates the value only, not the name, but will insert/delete if necessary
		/// </summary>
        /// <param name="testSetTestCaseParameters">The list of test case parameters to be persisted</param>
        /// <remarks>Pass in just the new list of parameters and this function will do the INSERT/UPDATE/DELETE</remarks>
        public void SaveTestCaseParameterValues(List<TestSetTestCaseParameter> testSetTestCaseParameters)
        {
            const string METHOD_NAME = "SaveTestCaseParameterValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First get the unique list of test case test set ids
                List<IGrouping<int, TestSetTestCaseParameter>> groupedParametersByTestSetTestCaseId = testSetTestCaseParameters.GroupBy(p => p.TestSetTestCaseId).ToList();

                //Loop through each test group in turn
                foreach (IGrouping<int, TestSetTestCaseParameter> group in groupedParametersByTestSetTestCaseId)
                {
                    int testSetTestCaseId = group.Key;
                    List<TestSetTestCaseParameter> parametersInGroup = group.ToList();

                    //Now retrieve the parameters in the database
                    using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                    {
                        var query = from p in context.TestSetTestCaseParameters
                                    where p.TestSetTestCaseId == testSetTestCaseId
                                    select p;

                        List<TestSetTestCaseParameter> parameterValuesInDb = query.ToList();

                        //See if we need to add/update any existing parameters
                        foreach (TestSetTestCaseParameter parameter in parametersInGroup)
                        {
                            TestSetTestCaseParameter parameterValueInDb = parameterValuesInDb.FirstOrDefault(p => p.TestCaseParameterId == parameter.TestCaseParameterId);
                            if (parameterValueInDb == null)
                            {
                                //Insert
                                context.TestSetTestCaseParameters.AddObject(parameter);
                            }
                            else
                            {
                                //Update
                                parameterValueInDb.StartTracking();
                                parameterValueInDb.Value = parameter.Value;
                            }
                        }

                        //See if there are any that need deleting
                        List<TestSetTestCaseParameter> parametersToDelete = new List<TestSetTestCaseParameter>();
                        foreach (TestSetTestCaseParameter parameterValueInDb in parameterValuesInDb)
                        {
                            if (!parametersInGroup.Any(p => p.TestCaseParameterId == parameterValueInDb.TestCaseParameterId))
                            {
                                parametersToDelete.Add(parameterValueInDb);
                            }
                        }
                        foreach (TestSetTestCaseParameter parameterToDelete in parametersToDelete)
                        {
                            context.TestSetTestCaseParameters.DeleteObject(parameterToDelete);
                        }

                        //Save all the changes
                        context.SaveChanges();
                    }
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of parameters and values for a test case that is part of the test set
        /// </summary>
        /// <param name="testSetTestCaseId">The unique record for the instance of the test case in the test set</param>
        /// <returns>List of test case parameters and test set values</returns>
        public List<TestSetTestCaseParameter> RetrieveTestCaseParameterValues(int testSetTestCaseId)
        {
            const string METHOD_NAME = "RetrieveParameterValues";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetTestCaseParameter> testSetTestCaseParameters;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetTestCaseParameters.Include(t => t.Parameter)
                                where t.TestSetTestCaseId == testSetTestCaseId
                                select t;

                    testSetTestCaseParameters = query.ToList();
                }

                //Resort by name (in-memory)
                testSetTestCaseParameters = testSetTestCaseParameters.OrderBy(t => t.Name).ToList();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCaseParameters;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        #endregion

        #region Test Set functions

        /// <summary>Counts all the test sets in the project</summary>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="folderId">
        /// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test sets
        /// </param>
        /// <param name="countAllFolders">Should we count the test sets in all folders in the project</param>
        /// <returns>The total number of test sets</returns>
        /// <remarks>Used to help with pagination</remarks>
        public int Count(int projectId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool countAllFolders = false)
        {
            const string METHOD_NAME = "Count";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testSetCount = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetsView
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetView, bool>> filterClause = CreateFilterExpression<TestSetView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetView>)query.Where(filterClause);
                        }
                    }

                    //See if we need to filter by folder
                    if (folderId.HasValue)
                    {
                        int folderIdValue = folderId.Value;
                        query = query.Where(t => t.TestSetFolderId == folderIdValue);
                    }
                    else if ((filters == null || filters.Count == 0) && !countAllFolders)
                    {
                        //test sets that have no folder (i.e. root), unless we have filters in which case show all
                        query = query.Where(t => !t.TestSetFolderId.HasValue);
                    }

                    //Get the count
                    testSetCount = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Counts how many test sets are linked to a specific configuration set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testConfigurationSetId">The id of the test case</param>
        /// <param name="filters">Any filters</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>The count of unique instances</returns>
        public int CountByTestConfigurationSet(int projectId, int testConfigurationSetId, Hashtable filters = null, double utcOffset = 0)
        {
            const string METHOD_NAME = "CountByTestConfigurationSet";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testSetCount = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetsView
                                where !t.IsDeleted && t.ProjectId == projectId && t.TestConfigurationSetId == testConfigurationSetId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetView, bool>> filterClause = CreateFilterExpression<TestSetView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetView>)query.Where(filterClause);
                        }
                    }

                    //Get the count
                    testSetCount = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Counts how many test sets use a specific test case
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testCaseId">The id of the test case</param>
        /// <param name="filters">Any filters</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>The count of unique instances</returns>
        public int CountByTestCase(int projectId, int testCaseId, Hashtable filters = null, double utcOffset = 0)
        {
            const string METHOD_NAME = "CountByTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testSetCount = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetsView
                                join sc in context.TestSetTestCases on t.TestSetId equals sc.TestSetId
                                where !t.IsDeleted  && t.ProjectId == projectId && sc.TestCaseId == testCaseId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetView, bool>> filterClause = CreateFilterExpression<TestSetView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetView>)query.Where(filterClause);
                        }
                    }

                    //Get the count
                    testSetCount = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Counts all the test sets in the project</summary>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The ID of the release we're interested in</param>
        /// <param name="folderId">
        /// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test sets
        /// </param>
        /// <param name="countAllFolders">Should we count the test sets in all folders in the project</param>
        /// <returns>The total number of test sets</returns>
        /// <remarks>Used to help with pagination</remarks>
        public int CountByRelease(int projectId, int releaseId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool countAllFolders = false)
        {
            const string METHOD_NAME = "CountByRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testSetCount = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetReleasesView
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId && t.DisplayReleaseId == releaseId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetReleaseView, bool>> filterClause = CreateFilterExpression<TestSetReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetReleaseView>)query.Where(filterClause);
                        }
                    }

                    //See if we need to filter by folder
                    if (folderId.HasValue)
                    {
                        int folderIdValue = folderId.Value;
                        query = query.Where(t => t.TestSetFolderId == folderIdValue);
                    }
                    else if ((filters == null || filters.Count == 0) && !countAllFolders)
                    {
                        //test sets that have no folder (i.e. root), unless we have filters in which case show all
                        query = query.Where(t => !t.TestSetFolderId.HasValue);
                    }

                    //Get the count
                    testSetCount = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetCount;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the test sets by the owner regardless of project, sorted by status then date</summary>
        /// <param name="ownerId">The owner of the test sets we want returned</param>
        /// <param name="projectId">The project ID to filter by (null = all projects)</param>
		/// <param name="numberRows">The number of rows to return</param>
        /// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
        /// <returns>Test Set list</returns>
        /// <remarks>It also only returns test sets for active projects.</remarks>
        public List<TestSetView> RetrieveByOwnerId(int ownerId, int? projectId, int numberRows = 500, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveByOwnerId";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetView> testSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetsView
                                where t.OwnerId == ownerId && t.IsProjectActive
                                select t;

                    //Add on the project filter if appropriate
                    if (projectId.HasValue)
                    {
                        query = query.Where(t => t.ProjectId == projectId.Value);
                    }

                    //Add on the deleted filter
                    if (!includeDeleted)
                    {
                        query = query.Where(t => !t.IsDeleted);
                    }

                    //Sort by planned date
                    query = query.OrderBy(t => t.PlannedDate).ThenBy(t => t.TestSetId);

                    //Execute the query
                    testSets = query.Take(numberRows).ToList();
                }


                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of the count of test sets that are on-schedule, unscheduled, or late
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release (null = all releases)</param>
        /// <returns></returns>
        public Dictionary<string, int> RetrieveScheduleSummary(int projectId, int? releaseId)
        {
            const string METHOD_NAME = "RetrieveScheduleSummary";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<int> releaseAndIterations = null;
                if (releaseId.HasValue)
                {
                    releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
                }

                Dictionary<string, int> scheduleSummary = new Dictionary<string, int>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First the base query
                    //We exclude the ones that are Completed, Blocked or Deferred
                    var query = from t in context.TestSets
                                where
                                    !t.IsDeleted &&
                                    t.ProjectId == projectId &&
                                    (t.TestSetStatusId == (int)TestSet.TestSetStatusEnum.NotStarted || t.TestSetStatusId == (int)TestSet.TestSetStatusEnum.InProgress)
                                select t;

                    //Add on the release filter
                    if (releaseId.HasValue)
                    {
                        query = query.Where(t => releaseAndIterations.Contains(t.ReleaseId.Value));
                    }

                    //Now the count of ones that are late
                    int lateCount = query.Where(t => t.PlannedDate.HasValue && t.PlannedDate < DateTime.UtcNow).Count();
                    int futureCount = query.Where(t => t.PlannedDate.HasValue && t.PlannedDate >= DateTime.UtcNow).Count();
                    int unscheduledCount = query.Where(t => !t.PlannedDate.HasValue).Count();

                    //Add to the dictionary
                    scheduleSummary.Add(GlobalResources.General.TestSet_Overdue, lateCount);
                    scheduleSummary.Add(GlobalResources.General.TestSet_Future, futureCount);
                    scheduleSummary.Add(GlobalResources.General.TestSet_Unscheduled, unscheduledCount);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return scheduleSummary;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the test-set execution status summary for a project</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we want to filter on (null for all)</param>
        /// <returns>List of test set execution summary</returns>
        /// <remarks>Always returns all the execution status codes</remarks>
        public List<TestSet_ExecutionStatusSummary> RetrieveExecutionStatusSummary(int projectId, int? releaseId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveExecutionStatusSummary";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSet_ExecutionStatusSummary> testSetExecutionSummary;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    testSetExecutionSummary = context.TestSet_RetrieveSummaryData(projectId, releaseId, includeDeleted).ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetExecutionSummary;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the list of test sets that are overdue in order of oldest first, does not include completed</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we're interested in (optional)</param>
        /// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
        /// <returns>Test Set list</returns>
        public List<TestSetView> RetrieveOverdue(int projectId, int? releaseId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveOverdue";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetView> overdueTestSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the base query
                    var query = from t in context.TestSetsView
                                where
                                    t.PlannedDate < DateTime.UtcNow &&
                                    t.PlannedDate.HasValue &&
                                    t.TestSetStatusId != (int)TestSet.TestSetStatusEnum.Completed &&
                                    t.TestSetStatusId != (int)TestSet.TestSetStatusEnum.Deferred &&
                                    t.ProjectId == projectId
                                select t;

                    //Add on the deleted filter
                    if (!includeDeleted)
                    {
                        query = query.Where(t => !t.IsDeleted);
                    }

                    //Add on the release filter
                    if (releaseId.HasValue)
                    {
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<int> releaseIds = releaseManager.GetSelfAndIterations(projectId, releaseId.Value);
                        query = query.Where(t => releaseIds.Contains(t.ReleaseId.Value));
                    }

                    //Sort by date
                    query = query.OrderBy(t => t.PlannedDate).ThenBy(t => t.TestSetId);

                    //Execute the query
                    overdueTestSets = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return overdueTestSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a list of all testSets in the system (for a project)</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="startRow">The first row to retrieve (starting at 1)</param>
        /// <param name="numberOfRows">The number of rows to retrieve</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="folderId">
        /// The folder to filter by, null = root folder, TEST_SET_FOLDER_ID_ALL_TEST_SETS = all test sets
        /// </param>
        /// <param name="includeDeleted">Should we include deleted testSets</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>TestSetView list</returns>
        /// <remarks>Also brings across any associated custom properties</remarks>
        /// <param name="ignoreRootFolderIfFilterSet">Should we ignore the root folder restriction if a filter is applied (default: TRUE)</param>
        public List<TestSetView> Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool ignoreRootFolderIfFilterSet = true)
        {
            const string METHOD_NAME = "Retrieve";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetView> testSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetsView
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
                                select t;

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by last updated date descending
                        query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TestSetId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestSetId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
                        
                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetView, bool>> filterClause = CreateFilterExpression<TestSetView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetView>)query.Where(filterClause);
                        }
                    }

                    //See if we need to filter by folder
                    if (folderId.HasValue)
                    {
                        if (folderId != TEST_SET_FOLDER_ID_ALL_TEST_SETS)
                        {
                            int folderIdValue = folderId.Value;
                            query = query.Where(t => t.TestSetFolderId == folderIdValue);
                        }
                    }
                    else if (filters == null || filters.Count == 0 || ignoreRootFolderIfFilterSet == false)
                    {
                        //test sets that have no folder (i.e. root)
                        //if we have a filter active then show all folder matches
                        query = query.Where(t => !t.TestSetFolderId.HasValue);
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
                        //Return nothing
                        return new List<TestSetView>();
                    }

                    //Execute the query
                    testSets = query
                        .Skip(startRow - 1)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a list of all testSets that contain the specified test case</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="testCaseId">The id of the test case</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="startRow">The first row to retrieve (starting at 1)</param>
        /// <param name="numberOfRows">The number of rows to retrieve</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="includeDeleted">Should we include deleted testSets</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>TestSetView list</returns>
        /// <remarks>Also brings across any associated custom properties</remarks>
        public List<TestSetView> RetrieveByTestCaseId(int projectId, int testCaseId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveByTestCaseId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetView> testSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    //Need to use distinct because the same test case could be in a test set multiple times
                    var query = (from t in context.TestSetsView
                                join sc in context.TestSetTestCases on t.TestSetId equals sc.TestSetId
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId && sc.TestCaseId == testCaseId
                                select t).Distinct();

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by last updated date descending
                        query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TestSetId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestSetId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
                        
                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetView, bool>> filterClause = CreateFilterExpression<TestSetView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetView>)query.Where(filterClause);
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
                        //Return nothing
                        return new List<TestSetView>();
                    }

                    //Execute the query
                    testSets = query
                        .Skip(startRow - 1)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a list of all testSets in the system (for a project)</summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="startRow">The first row to retrieve (starting at 1)</param>
        /// <param name="numberOfRows">The number of rows to retrieve</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="folderId">
        /// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test sets
        /// </param>
        /// <param name="releaseId">The release we're interested in</param>
        /// <param name="includeDeleted">Should we include deleted testSets</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <returns>TestSetView list</returns>
        /// <remarks>Also brings across any associated custom properties</remarks>
        /// <param name="ignoreRootFolderIfFilterSet">Should we ignore the root folder restriction if a filter is applied (default: TRUE)</param>
        public List<TestSetReleaseView> RetrieveByReleaseId(int projectId, int releaseId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool ignoreRootFolderIfFilterSet = true)
        {
            const string METHOD_NAME = "RetrieveByReleaseId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetReleaseView> testSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestSetReleasesView
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId && t.DisplayReleaseId == releaseId
                                select t;

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by last updated date descending
                        query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TestSetId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestSetId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetReleaseView, bool>> filterClause = CreateFilterExpression<TestSetReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetReleaseView>)query.Where(filterClause);
                        }
                    }

                    //See if we need to filter by folder
                    if (folderId.HasValue)
                    {
                        if (folderId != TEST_SET_FOLDER_ID_ALL_TEST_SETS)
                        {
                            int folderIdValue = folderId.Value;
                            query = query.Where(t => t.TestSetFolderId == folderIdValue);
                        }
                    }
                    else if (filters == null || filters.Count == 0 || ignoreRootFolderIfFilterSet == false)
                    {
                        //test sets that have no folder (i.e. root)
                        //if we have a filter active then show all folder matches
                        query = query.Where(t => !t.TestSetFolderId.HasValue);
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
                        //Return nothing
                        return new List<TestSetReleaseView>();
                    }

                    //Execute the query
                    testSets = query
                        .Skip(startRow - 1)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the test sets by the automation host regardless of project, sorted by project then planned date</summary>
		/// <param name="automationHostId">The automation host who's test sets we want returned</param>
		/// <param name="startDate">Only consider test sets that are planner after this date/time</param>
		/// <param name="endDate">Only consider test sets that are planned before this date/time</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Test Set list</returns>
        public List<TestSetView> RetrieveByAutomationHostId(int automationHostId, Nullable<DateTime> startDate = null, Nullable<DateTime> endDate = null, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveByAutomationHostId";

            try
            {
                List<TestSetView> testSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetsView
                                where
                                    t.AutomationHostId == automationHostId &&
                                    t.TestSetStatusId == (int)TestSet.TestSetStatusEnum.NotStarted &&
                                    t.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated &&
                                    t.IsProjectActive
                                select t;

                    //Add on the deleted filter
                    if (!includeDeleted)
                    {
                        query = query.Where(t => !t.IsDeleted);
                    }

                    //Add on the appropriate date filter
                    if (startDate.HasValue)
                    {
                        query = query.Where(t => t.PlannedDate >= startDate.Value);
                    }
                    //if (endDate.HasValue)
                    //{
                    //    query = query.Where(t => t.PlannedDate <= endDate.Value);
                    //}

                    //Add on the sorts
                    query = query.OrderBy(t => t.ProjectId).ThenBy(t => t.PlannedDate).ThenBy(t => t.TestSetId);

                    testSets = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			    return testSets;
            }
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
        }

        /// <summary>Retrieves a particular test set view entity by its ID</summary>
        /// <param name="testSetId">The ID of the test set we want to retrieve</param>
        /// <param name="includeDeleted">Should we include deleted</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>A test set entity</returns>
        /// <seealso cref="RetrieveById2"/>
        public TestSetView RetrieveById(int? projectId, int testSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetView testSetView;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the query for retrieving the test set entity
                    var query = from t in context.TestSetsView
                                where t.TestSetId == testSetId && (!t.IsDeleted || includeDeleted)
                                select t;

                    //Add the project filter if specified
                    if (projectId.HasValue)
                    {
                        query = query.Where(t => t.ProjectId == projectId.Value);
                    }

                    testSetView = query.FirstOrDefault();
                }
                //If we don't have a record, throw a specific exception (since client will be expecting one record)
                if (testSetView == null)
                {
                    throw new ArtifactNotExistsException("Test Set " + testSetId.ToString() + " doesn't exist in the system.");
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the test set
                return testSetView;
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

        /// <summary>Retrieves a particular test set view entity by its ID</summary>
        /// <param name="testSetId">The ID of the test set we want to retrieve</param>
        /// <param name="includeDeleted">Should we include deleted</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">The id of the release we want the data for</param>
        /// <returns>A test set entity, or null if not existing</returns>
        /// <seealso cref="RetrieveById"/>
        public TestSetReleaseView RetrieveByIdForRelease(int projectId, int testSetId, int releaseId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveByIdForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetReleaseView testSetView;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the query for retrieving the test set entity
                    var query = from t in context.TestSetReleasesView
                                where t.TestSetId == testSetId && (!t.IsDeleted || includeDeleted) && t.DisplayReleaseId == releaseId && t.ProjectId == projectId
                                select t;

                    testSetView = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the test set
                return testSetView;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Returns a simple key/value dictionary that can be used in test set dropdown lists
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The key/value dictionary</returns>
        public Dictionary<string, string> RetrieveForLookups(int projectId)
        {
            const string METHOD_NAME = "RetrieveForLookups";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                string runningIndentLevel = "AAA";
                Dictionary<string, string> testSetLookup = new Dictionary<string, string>();

                //Get the complete folder list for the project
                List<TestSetFolderHierarchyView> testSetFolders = TestSetFolder_GetList(projectId);

                //Next we need to get all the test sets with name, folder
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSets
                                where t.ProjectId == projectId && !t.IsDeleted
                                orderby t.Name, t.TestSetId
                                select new { t.TestSetId, t.Name, t.TestSetFolderId };

                    //Get the list and group by folder
                    var testSetList = query.ToList();
                    var groupedTestSetsByFolder = testSetList.GroupBy(t => t.TestSetFolderId).ToList();


                    //Add the root folder
                    //value_indent_summary_alternate_active
                    testSetLookup.Add(0 + "_" + (runningIndentLevel.Length / 3) + "_Y_N_Y", GlobalResources.General.Global_Root);

                    //Add the root-folder testSets (if any)
                    var groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == null);
                    if (groupedTestSets != null)
                    {
                        var groupedTestSetList = groupedTestSets.ToList();
                        if (groupedTestSetList != null && groupedTestSetList.Count > 0)
                        {
                            foreach (var testSetEntry in groupedTestSetList)
                            {
                                testSetLookup.Add(
                                    testSetEntry.TestSetId + "_" + ((runningIndentLevel.Length / 3) + 1) + "_N_N_Y",
                                    testSetEntry.Name);
                            }

                            //Increment the indent level
                            runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                        }
                        else
                        {
                            //No testSets at root, so folder is first child entry
                            runningIndentLevel = runningIndentLevel + "AAA";
                        }
                    }
                    else
                    {
                        //No testSets at root, so folder is first child entry
                        runningIndentLevel = runningIndentLevel + "AAA";
                    }

                    //Loop through the folders
                    int lastFolderLevel = 0;
                    foreach (TestSetFolderHierarchyView testSetFolder in testSetFolders)
                    {
                        //See if this folder is a peer, child or above the last one
                        if (testSetFolder.HierarchyLevel.HasValue)
                        {
                            if (testSetFolder.HierarchyLevel.Value > lastFolderLevel)
                            {
                                runningIndentLevel = runningIndentLevel + "AAA";
                            }
                            else if (testSetFolder.HierarchyLevel.Value == lastFolderLevel)
                            {
                                //Increment the indent level
                                runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                            }
                            else
                            {
                                int numberOfLevels = lastFolderLevel - testSetFolder.HierarchyLevel.Value;
                                runningIndentLevel = runningIndentLevel.SafeSubstring(0, runningIndentLevel.Length - (numberOfLevels * 3));
                                runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                            }
                            lastFolderLevel = testSetFolder.HierarchyLevel.Value;
                        }

                        //Add the folder item
                        //Make the folders negative to avoid collisions with the testSets themselves
                        testSetLookup.Add(
                            -testSetFolder.TestSetFolderId + "_" + (runningIndentLevel.Length / 3) + "_Y_N_Y",
                            testSetFolder.Name);

                        //Add the testSets (if any)
                        groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == testSetFolder.TestSetFolderId);
                        if (groupedTestSets != null)
                        {
                            var groupedTestSetList = groupedTestSets.ToList();
                            if (groupedTestSetList != null && groupedTestSetList.Count > 0)
                            {
                                foreach (var testSetEntry in groupedTestSetList)
                                {
                                    testSetLookup.Add(
                                        testSetEntry.TestSetId + "_" + ((runningIndentLevel.Length / 3) + 1) + "_N_N_Y",
                                        testSetEntry.Name);
                                }
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetLookup;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Returns a list of test set/test set folder entries that can be used in test set dropdown lists
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>The list of entries</returns>
        public List<TestSetLookupEntry> RetrieveForLookups2(int projectId)
        {
            const string METHOD_NAME = "RetrieveForLookups2";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                string runningIndentLevel = "AAA";
                List<TestSetLookupEntry> testSetLookups = new List<TestSetLookupEntry>();

                //Get the complete folder list for the project
                List<TestSetFolderHierarchyView> testSetFolders = TestSetFolder_GetList(projectId);

                //Next we need to get all the test sets with name, folder
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSets
                                where t.ProjectId == projectId && !t.IsDeleted
                                orderby t.Name, t.TestSetId
                                select new { t.TestSetId, t.Name, t.TestSetFolderId };

                    //Get the list and group by folder
                    var testSetList = query.ToList();
                    var groupedTestSetsByFolder = testSetList.GroupBy(t => t.TestSetFolderId).ToList();


                    //Add the root folder
                    testSetLookups.Add(new TestSetLookupEntry()
                        {
                            TestSetId = 0,
                            Name = GlobalResources.General.Global_Root,
                            IndentLevel = runningIndentLevel,
                            IsFolder = true
                        });

                    //Add the root-folder testSets (if any)
                    var groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == null);
                    if (groupedTestSets != null)
                    {
                        var groupedTestSetList = groupedTestSets.ToList();
                        if (groupedTestSetList != null && groupedTestSetList.Count > 0)
                        {
                            foreach (var testSetEntry in groupedTestSetList)
                            {
                                testSetLookups.Add(new TestSetLookupEntry()
                                {
                                    TestSetId = testSetEntry.TestSetId,
                                    Name = testSetEntry.Name,
                                    IndentLevel = runningIndentLevel,
                                    IsFolder = false
                                });
                            }

                            //Increment the indent level
                            runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                        }
                        else
                        {
                            //No testSets at root, so folder is first child entry
                            runningIndentLevel = runningIndentLevel + "AAA";
                        }
                    }
                    else
                    {
                        //No testSets at root, so folder is first child entry
                        runningIndentLevel = runningIndentLevel + "AAA";
                    }

                    //Loop through the folders
                    int lastFolderLevel = 0;
                    foreach (TestSetFolderHierarchyView testSetFolder in testSetFolders)
                    {
                        //See if this folder is a peer, child or above the last one
                        if (testSetFolder.HierarchyLevel.HasValue)
                        {
                            if (testSetFolder.HierarchyLevel.Value > lastFolderLevel)
                            {
                                runningIndentLevel = runningIndentLevel + "AAA";
                            }
                            else if (testSetFolder.HierarchyLevel.Value == lastFolderLevel)
                            {
                                //Increment the indent level
                                runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                            }
                            else
                            {
                                int numberOfLevels = lastFolderLevel - testSetFolder.HierarchyLevel.Value;
                                runningIndentLevel = runningIndentLevel.SafeSubstring(0, runningIndentLevel.Length - (numberOfLevels * 3));
                                runningIndentLevel = HierarchicalList.IncrementIndentLevel(runningIndentLevel);
                            }
                            lastFolderLevel = testSetFolder.HierarchyLevel.Value;
                        }

                        //Add the folder item
                        //Make the folders negative to avoid collisions with the testSets themselves
                        testSetLookups.Add(new TestSetLookupEntry()
                        {
                            TestSetId = -testSetFolder.TestSetFolderId,
                            Name = testSetFolder.Name,
                            IndentLevel = runningIndentLevel,
                            IsFolder = true
                        });

                        //Add the testSets (if any)
                        groupedTestSets = groupedTestSetsByFolder.FirstOrDefault(f => f.Key == testSetFolder.TestSetFolderId);
                        if (groupedTestSets != null)
                        {
                            var groupedTestSetList = groupedTestSets.ToList();
                            if (groupedTestSetList != null && groupedTestSetList.Count > 0)
                            {
                                foreach (var testSetEntry in groupedTestSetList)
                                {
                                    testSetLookups.Add(new TestSetLookupEntry()
                                    {
                                        TestSetId = testSetEntry.TestSetId,
                                        Name = testSetEntry.Name,
                                        IndentLevel = runningIndentLevel,
                                        IsFolder = false
                                    });
                                }
                            }
                        }
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetLookups;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a particular test set entity by its ID</summary>
        /// <param name="testSetId">The ID of the test set we want to retrieve</param>
        /// <returns>A test set entity</returns>
        /// <param name="includeDeleted">Should we include deleted</param>
        /// <param name="projectId">The current project</param>
        /// <param name="includeTestCases">Should we include the test cases</param>
        /// <seealso cref="RetrieveById"/>
        public TestSet RetrieveById2(int? projectId, int testSetId, bool includeDeleted = false, bool includeTestCases = false)
        {
            const string METHOD_NAME = "RetrieveById2";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSet testSet;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create the query for retrieving the test set entity
                    var query = from t in context.TestSets
                                where t.TestSetId == testSetId && (!t.IsDeleted || includeDeleted)
                                select t;

                    //Add the project filter if specified
                    if (projectId.HasValue)
                    {
                        query = query.Where(t => t.ProjectId == projectId.Value);
                    }

                    testSet = query.FirstOrDefault();

                    //If we don't have a record, throw a specific exception (since client will be expecting one record)
                    if (testSet == null)
                    {
                        throw new ArtifactNotExistsException("Test Case " + testSetId.ToString() + " doesn't exist in the system.");
                    }

                    //Create select command for retrieving the mapped test cases (joined using fix-up)
                    var query2 = from t in context.TestSetTestCases.Include(t => t.TestCase)
                                where (!t.TestCase.IsDeleted || includeDeleted) && t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    query2.ToList();
                }
 
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the test set
                return testSet;
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

        /// <summary>Retrieves all test sets in the specified project that ARE marked for deletion.</summary>
        /// <param name="projectId">The project ID to get items for.</param>
        /// <returns></returns>
        public List<TestSetView> RetrieveDeleted(int projectId)
        {
            const string METHOD_NAME = "RetrieveDeleted()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetView> deletedTestSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSetsView
                                where t.ProjectId == projectId && t.IsDeleted
                                orderby t.TestSetId
                                select t;

                    deletedTestSets = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return deletedTestSets;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                //Do not rethrow.
                return new List<TestSetView>();
            }
        }

        /// <summary>Deletes a test-set in the system that has the specified ID</summary>
        /// <param name="userId">The user we're viewing the test sets as</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="testSetId">The ID of the test-set to be deleted</param>
        public void DeleteFromDatabase(int testSetId, int userId)
        {
            const string METHOD_NAME = "DeleteFromDatabase()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //We need to initially retrieve the test set to make sure it exists
                TestSet testSet = null;
                try
                {
                    testSet = RetrieveById2(null, testSetId, true);
                }
                catch (ArtifactNotExistsException)
                {
                    //If it's already deleted, just fail quietly
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return;
                }
                int projectId = testSet.ProjectId;

                //First we need to delete any attachments associated with the test set
                Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
                attachmentManager.DeleteByArtifactId(testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);

                //Next we need to delete any custom properties associated with the test set		
                new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);

                //Finally call the stored procedure to delete the test set itself
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    context.TestSet_Delete(testSetId);
                }

                //Log the purge.
                new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, DateTime.UtcNow, testSet.Name);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Undeletes a test set and all children test sets, making it available to users.</summary>
        /// <param name="testSetId">The test set to undelete.</param>
        /// <param name="userId">The userId performing the undelete.</param>
        /// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
        public void UnDelete(int testSetId, int userId, long rollbackId, bool logHistory = true)
        {
            const string METHOD_NAME = "UnDelete()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            int? testSetFolderId = null;
            int projectId = 0;
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                //We need to initially retrieve the test set (needs to be marked as deleted)
                var query = from t in context.TestSets
                            where t.TestSetId == testSetId && t.IsDeleted
                            select t;

                //Get the test set
                TestSet testSet = query.FirstOrDefault();
                if (testSet != null)
                {
                    projectId = testSet.ProjectId;
                    testSetFolderId = testSet.TestSetFolderId;

                    //Mark as undeleted
                    testSet.StartTracking();
                    testSet.LastUpdateDate = DateTime.UtcNow;
                    testSet.IsDeleted = false;

                    //Save changes, no history logged, that's done later
                    context.SaveChanges();

                    //Log the undelete
                    if (logHistory)
                    {
                        //Okay, mark it as being undeleted.
                        new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, rollbackId, DateTime.UtcNow);
                    }
                }
            }

            //Now roll up the execution status to the folders
            if (testSetFolderId.HasValue && projectId > 0)
            {
                this.TestSetFolder_RefreshExecutionData(projectId, testSetFolderId.Value);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>Updates just the last updated date of the test set</summary>
        /// <param name="testSetId">The test set in question</param>
        protected void UpdateLastUpdatedDate(int testSetId)
        {
            const string METHOD_NAME = "UpdateLastUpdatedDate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestSets
                                where t.TestSetId == testSetId
                                select t;

                    TestSet testSet = query.FirstOrDefault();
                    if (testSet != null)
                    {
                        //Update the last updated date
                        testSet.StartTracking();
                        testSet.LastUpdateDate = DateTime.UtcNow;
                        context.SaveChanges();
                    }
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }



        /// <summary>Updates a passed-in test set</summary>
        /// <param name="testSet">The dataset to be persisted</param>
        /// <param name="userId">The user making the change</param>
        /// <param name="updHistory">Should we update history</param>
        /// <param name="rollbackId">The rollback id if a rollback</param>
        public void Update(TestSet testSet, int userId, long? rollbackId = null, bool updHistory = true)
        {
            const string METHOD_NAME = "Update()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //If we have a null entity just return
            if (testSet == null)
            {
                return;
            }

            try
            {
                bool recurrenceEventNeeded = false;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Start tracking changes
                    testSet.StartTracking();

                    //If the test set was completed but the planned date has been subsequently changed, switch back to 'Not Run'
                    if (testSet.TestSetStatusId == (int)TestSet.TestSetStatusEnum.Completed && testSet.PlannedDate.HasValue && testSet.ChangeTracker.OriginalValues.ContainsKey("PlannedDate"))
                    {
                        testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
                    }

                    //If the test set was just set to completed and it has a recurrence, then need to switch it back
                    //to Not Started with a new planned date. However we need to do this after the first update so that
                    //the history tab has a record of the change and any events based on it changing to Completed fire
                    //correctly
                    if (testSet.RecurrenceId.HasValue && testSet.PlannedDate.HasValue &&
                            testSet.TestSetStatusId == (int)TestSet.TestSetStatusEnum.Completed &&
                            testSet.ChangeTracker.OriginalValues.ContainsKey("TestSetStatusId"))
                    {
                        recurrenceEventNeeded = true;
                    }

                    //Update the last-update and concurrency dates
                    testSet.LastUpdateDate = DateTime.UtcNow;
                    testSet.ConcurrencyDate = DateTime.UtcNow;

                    //Now apply the changes
                    context.TestSets.ApplyChanges(testSet);

                    //Save the changes, recording any history changes, and sending any notifications
                    context.SaveChanges(userId, true, true, rollbackId);
                }

                //We need to change the context to avoid concurrency issues

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Now for any test set that has a recurrence pattern set we need to see if it was just made
                    //Completed and if so, we need to switch it back to "Not Started" with the date advanced appropriately
                    if (recurrenceEventNeeded)
                    {
                        //We need to get a fresh copy of the record to avoid concurrency issues
                        var query = from t in context.TestSets
                                    where t.TestSetId == testSet.TestSetId && !t.IsDeleted
                                    select t;

                        TestSet testSet2 = query.FirstOrDefault();

                        if (testSet2 != null && testSet2.RecurrenceId.HasValue)
                        {
                            testSet2.StartTracking();

                            //Change the status back to not-started and advance the date/time
                            //We check the status and date again in case it has changed in the meantime (theoretically possible but unlikely)
                            if (testSet2.TestSetStatusId == (int)TestSet.TestSetStatusEnum.Completed && testSet2.PlannedDate.HasValue)
                            {
                                testSet2.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
                                TestSet.RecurrenceEnum recurrence = (TestSet.RecurrenceEnum)testSet2.RecurrenceId.Value;
                                switch (recurrence)
                                {
                                    case TestSet.RecurrenceEnum.Hourly:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddHours(1);
                                        break;

                                    case TestSet.RecurrenceEnum.Daily:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddDays(1);
                                        break;

                                    case TestSet.RecurrenceEnum.Weekly:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddDays(7);
                                        break;

                                    case TestSet.RecurrenceEnum.Monthly:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddMonths(1);
                                        break;

                                    case TestSet.RecurrenceEnum.Quarterly:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddMonths(3);
                                        break;

                                    case TestSet.RecurrenceEnum.Yearly:
                                        testSet2.PlannedDate = testSet2.PlannedDate.Value.AddYears(1);
                                        break;
                                }

                                //Make sure all changes are applied (fixes [IN:4539])
                                //Need to force a detection of changes
                                context.DetectChanges();

                                //Save the changes, recording any history changes, and sending any notifications
                                context.SaveChanges(userId, true, true, null);
                            }
                        }
                    }
                }
                //Rollup the execution data to the folder if we have one
                if (testSet.TestSetFolderId.HasValue)
                {
                    TestSetFolder_RefreshExecutionData(testSet.ProjectId, testSet.TestSetFolderId.Value);
                }
            }
            catch (OptimisticConcurrencyException exception)
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

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>Marks the specified test set (and any children) as deleted.</summary>
        /// <param name="userId">The userId making the deletion.</param>
        /// <param name="projectId">The projectId that the test set belongs to.</param>
        /// <param name="testSetId">The test set to delete.</param>
        public void MarkAsDeleted(int userId, int projectId, int testSetId)
        {
            const string METHOD_NAME = "MarkAsDeleted()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int? testSetFolderId = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //We need to initially retrieve the test set (cannot be already deleted)
                    var query = from t in context.TestSets
                                where t.TestSetId == testSetId && !t.IsDeleted
                                select t;

                    //Get the test set
                    TestSet testSet = query.FirstOrDefault();
                    if (testSet != null)
                    {
                        //Get the folder id
                        testSetFolderId = testSet.TestSetFolderId;

                        //Mark as deleted
                        testSet.StartTracking();
                        testSet.LastUpdateDate = DateTime.UtcNow;
                        testSet.IsDeleted = true;

                        //Save changes, no history logged, that's done later for the delete
                        context.SaveChanges();

                        //Add a changeset to mark it as deleted.
                        new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, DateTime.UtcNow);
                    }
                }

                //Now roll up the execution status to the folders
                if (testSetFolderId.HasValue)
                {
                    this.TestSetFolder_RefreshExecutionData(projectId, testSetFolderId.Value);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Creates a new test case from an existing release (containing test sets)
        /// </summary>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="releaseId">The release we want to get the tests from</param>
        /// <param name="userId">The user performing the operation</param>
        /// <returns>The id of the newly created test set</returns>
        /// <remarks>The new test set is simply added to the end of the list</remarks>
        public int CreateFromRelease(int projectId, int releaseId, int userId)
        {
            const string METHOD_NAME = "CreateFromRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            int testSetId = -1;
            try
            {
                //First we need to retrieve the specified release
                Business.ReleaseManager releaseManager = new Business.ReleaseManager();
                ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId);

                //Next we need to insert a new test set in the root folder
                testSetId = Insert(
                   userId,
                   projectId,
                   null,
                   releaseId,
                   release.CreatorId,
                   null,
                   TestSet.TestSetStatusEnum.NotStarted,
                   GlobalResources.General.TestSet_NewTestSetBasedOnRelease + " " + release.VersionNumber,
                   release.Name,
                   null,
                   TestRun.TestRunTypeEnum.Manual,
                   null,
                   null
                   );

                //Now we need to copy across the various test cases from the release to the test set
                Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
                List<TestCase> mappedTestCases = testCaseManager.RetrieveMappedByReleaseId(projectId, releaseId);

                //Add the test cases to the copied test set
                List<int> testCaseIds = mappedTestCases.Select(t => t.TestCaseId).ToList();
                AddTestCases(projectId, testSetId, testCaseIds, null, null, userId);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return testSetId;
        }

        /// <summary>
        /// Creates a new test set from the passed-in requirements
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <param name="projectId">The current project</param>
        /// <param name="requirementIds">The list of requirements</param>
        /// <returns>The id of the new test set</returns>
        /// <remarks>The test set consists of the test cases linked to the provided requirements</remarks>
        public int CreateFromRequirements(int userId, int projectId, List<int> requirementIds)
        {
            const string METHOD_NAME = "CreateFromRequirements";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            int testSetId = -1;
            int? releaseId = null;
            try
            {
                //First we need to retrieve the specified requirements, one at a time, getting their child items and then their mapped test cases
                List<int> testCaseIds = new List<int>();
                RequirementManager requirementManager = new Business.RequirementManager();
                TestCaseManager testCaseManager = new TestCaseManager();
                foreach (int requirementId in requirementIds)
                {
                    try
                    {
                        RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);

                        //If we have a single requirement, set the test set's release to the same as the requirement
                        if (requirementIds.Count == 1 && requirement.ReleaseId.HasValue)
                        {
                            releaseId = requirement.ReleaseId.Value;
                        }

                        List<TestCase> coveredTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirement.RequirementId);
                        foreach (TestCase testCaseRow in coveredTestCases)
                        {
                            if (!testCaseIds.Contains(testCaseRow.TestCaseId))
                            {
                                testCaseIds.Add(testCaseRow.TestCaseId);
                            }
                        }
                        if (requirement.IsSummary)
                        {
                            List<RequirementView> childRequirements = requirementManager.RetrieveChildren(UserManager.UserInternal, projectId, requirement.IndentLevel, true);
                            foreach (RequirementView childRequirement in childRequirements)
                            {
                                coveredTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, childRequirement.RequirementId);
                                foreach (TestCase testCaseRow in coveredTestCases)
                                {
                                    if (!testCaseIds.Contains(testCaseRow.TestCaseId))
                                    {
                                        testCaseIds.Add(testCaseRow.TestCaseId);
                                    }
                                }
                            }
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and continue
                    }
                }

                if (testCaseIds.Count > 0)
                {
                    //Next we need to insert a new test set at the end of the list based on the release
                    testSetId = Insert(
                       userId,
                       projectId,
                       null,
                       releaseId,
                       userId,
                       null,
                       TestSet.TestSetStatusEnum.NotStarted,
                       GlobalResources.General.TestSet_NewTestSetBasedOnRequirements,
                       null,
                       null,
                       TestRun.TestRunTypeEnum.Manual,
                       null,
                       null
                       );

                    //Now we need to add the various test cases to the test set
                    AddTestCases(projectId, testSetId, testCaseIds, null, null,userId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetId;
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
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="artifactCustomProperty">The custom property row</param>
        /// <param name="newComment">The new comment (if any)</param>
        /// <remarks>Fails quietly but logs errors</remarks>
        public void SendCreationNotification(int testSetId, ArtifactCustomProperty artifactCustomProperty, string newComment)
        {
            const string METHOD_NAME = "SendCreationNotification";
            //Send a notification
            try
            {
                TestSetView notificationArt = RetrieveById(null, testSetId);
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

        /// <summary>
        /// Auto-schedules test sets associated with the specific release/iteration
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">the id of the release/iteration</param>
        /// <param name="userId">The id of the user making the change</param>
        protected internal void AutoScheduleTestSets(int projectId, int releaseId, int userId)
        {
            const string METHOD_NAME = "AutoScheduleTestSets";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the list of auto-scheduled test sets for this release/project
                    var query = from t in context.TestSets
                                where
                                    t.IsAutoScheduled &&
                                    t.ProjectId == projectId &&
                                    t.ReleaseId == releaseId
                                orderby t.TestSetId
                                select t;

                    List<TestSet> testSets = query.ToList();
                    if (testSets.Count > 0)
                    {
                        //We add a small offset to each so that they're not all scheduled for the exact same time
                        //in case they are for the same automation host
                        int offset = 0;
                        foreach (TestSet testSet in testSets)
                        {
                            DateTime newPlannedDate = DateTime.UtcNow;
                            if (testSet.BuildExecuteTimeInterval.HasValue)
                            {
                                newPlannedDate = newPlannedDate.AddSeconds(testSet.BuildExecuteTimeInterval.Value + offset);
                            }

                            //Set the planned date to be x minutes in the future and change the status back to 'Not Started'
                            testSet.StartTracking();
                            testSet.PlannedDate = newPlannedDate;
                            testSet.TestSetStatusId = (int)TestSet.TestSetStatusEnum.NotStarted;
                            offset += 10; //10 seconds between them
                        }
                    }

                    //Save the changes, recording history
                    context.SaveChanges(userId, true, true, null);
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

        /// <summary>Inserts a new test set into the system with a specified indent level</summary>
        /// <param name="folderId">The folder the test set is being inserted in (null = root)</param>
        /// <param name="testSetStatus">The status of the test set</param>
        /// <param name="userId">The user we're creating the test sets as</param>
        /// <param name="creatorId">The creator's user id</param>
        /// <param name="description">The detailed description (optional)</param>
        /// <param name="name">The short name</param>
        /// <param name="ownerId">The owner's user id (optional)</param>
        /// <param name="plannedDate">The planned date to execute (optional)</param>
        /// <param name="projectId">The project we're adding it to</param>
        /// <param name="releaseId">The release it should be targeted against (optional)</param>
        /// <param name="testSetStatusId">The test set's status</param>
        /// <param name="automationHostId">The id of the automation host (optional)</param>
        /// <param name="testRunType">The id of the type of test set (automated vs manual)</param>
        /// <param name="buildExecuteTimeInterval">The interval between the build being completed and the new test set Planned Date</param>
        /// <param name="dynamicQuery">The dynamic query used for a dynamic test set</param>
        /// <param name="isAutoScheduled">Is the test set auto scheduled after a build finishes</param>
        /// <param name="isDynamic">Is this a dynamic test set based on a query</param>
        /// <param name="recurrence">The recurrence pattern</param>
        /// <param name="testConfigurationSetId">Is this linked to a test configuration set</param>
        /// <returns>The ID of the newly created test set</returns>
        public int Insert(int userId, int projectId, int? folderId, int? releaseId, int creatorId, int? ownerId, TestSet.TestSetStatusEnum testSetStatus, string name, string description, DateTime? plannedDate, TestRun.TestRunTypeEnum testRunType, int? automationHostId, TestSet.RecurrenceEnum? recurrence, bool isAutoScheduled = false, bool isDynamic = false, int? buildExecuteTimeInterval = null, string dynamicQuery = null, int? testConfigurationSetId = null)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Inititialization
            int testSetId = -1;

            try
            {
                //Fill out entity with data for new test set
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    TestSet testSet = new TestSet();
                    testSet.ProjectId = projectId;
                    testSet.TestSetFolderId = folderId;
                    testSet.ReleaseId = releaseId;
                    testSet.CreatorId = creatorId;
                    testSet.OwnerId = ownerId;
                    testSet.TestSetStatusId = (int)testSetStatus;
                    testSet.Name = name;
                    testSet.Description = description;
                    testSet.PlannedDate = plannedDate;
                    testSet.TestRunTypeId = (int)testRunType;
                    testSet.AutomationHostId = automationHostId;
                    testSet.RecurrenceId = (recurrence.HasValue) ? (int?)((int)recurrence.Value) : null;
                    testSet.CreationDate = DateTime.UtcNow;
                    testSet.LastUpdateDate = DateTime.UtcNow;
                    testSet.ConcurrencyDate = DateTime.UtcNow;
                    testSet.CountBlocked = 0;
                    testSet.CountCaution = 0;
                    testSet.CountFailed = 0;
                    testSet.CountNotApplicable = 0;
                    testSet.CountNotRun = 0;
                    testSet.CountPassed = 0;
                    testSet.IsAutoScheduled = isAutoScheduled;
                    testSet.IsDynamic = isDynamic;
                    testSet.DynamicQuery = dynamicQuery;
                    testSet.BuildExecuteTimeInterval = buildExecuteTimeInterval;
                    testSet.TestConfigurationSetId = testConfigurationSetId;

                    //Save test set and capture ID
                    context.TestSets.AddObject(testSet);
                    context.SaveChanges();
                    testSetId = testSet.TestSetId;
                }

                //Insert our history item.
                new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, DateTime.UtcNow);

                //Next rollup any changes in execution DATA to the folder (if any)
                if (folderId.HasValue)
                {
                    this.TestSetFolder_RefreshExecutionData(projectId, folderId.Value);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetId;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Refreshes the execution data for a specific test set, and optionally for a specific release
        /// </summary>
        /// <param name="projectId">The current project</param>
        /// <param name="testSetId">The test set to be updated</param>
        /// <param name="releaseId">The release to be updated (optional) - if not specified, updates all releases</param>
        protected internal void TestSet_RefreshExecutionData(int projectId, int testSetId, int? releaseId = null)
        {
            const string METHOD_NAME = "TestSet_RefreshExecutionData";

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
                    //Make sure we have a test set
                    var query = from t in context.TestSets
                                where t.TestSetId == testSetId && !t.IsDeleted
                                select t;

                    TestSet testSet = query.FirstOrDefault();

                    if (testSet != null)
                    {
                        context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;

                        //First we refresh the test set execution data
                        context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
                        context.TestSet_RefreshExecutionData(projectId, testSetId);

                        //Finally we roll-up the folders
                        if (testSet.TestSetFolderId.HasValue)
                        {
                            this.TestSetFolder_RefreshExecutionData(projectId, testSet.TestSetFolderId.Value);
                        }
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

        /// <summary>Copies a test set (and its test mapped cases) from one location to another. Will not copy deleted test sets.</summary>
        /// <param name="userId">The user that is performing the copy</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceTestSetId">The test set we want to copy</param>
        /// <param name="destTestSetFolderId">The folder we want to copy it into (null = root)</param>
        /// <param name="prependName">Should we prepend 'Copy of ...' to name</param>
        /// <returns>The id of the copy of the test set</returns>
        public int TestSet_Copy(int userId, int projectId, int sourceTestSetId, int? destTestSetFolderId, bool prependName = true)
        {
            const string METHOD_NAME = "TestSet_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to retrieve the test set being copied
                TestSet sourceTestSet = this.RetrieveById2(projectId, sourceTestSetId);

                //Get the template for this project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Insert a new test set with the data copied from the existing one
                //** We do not copy across the test automation info - since that causes issues if the user
                //changes the test script in one test set that affects the other (change in v3.0 patch 11)
                string name = sourceTestSet.Name;
                if (prependName)
                {
                    name = name + CopiedArtifactNameSuffix;
                }

                int copiedTestSetId = this.Insert(
                    userId,
                    projectId,
                    destTestSetFolderId,
                    sourceTestSet.ReleaseId,
                    sourceTestSet.CreatorId,
                    sourceTestSet.OwnerId,
                    (TestSet.TestSetStatusEnum)sourceTestSet.TestSetStatusId,
                    name,
                    sourceTestSet.Description,
                    sourceTestSet.PlannedDate,
                    (TestRun.TestRunTypeEnum)sourceTestSet.TestRunTypeId,
                    sourceTestSet.AutomationHostId,
                    (sourceTestSet.RecurrenceId.HasValue) ? (TestSet.RecurrenceEnum?)sourceTestSet.RecurrenceId.Value : null
                    );

                //Copy across the test set parameter values
                CopyTestSetParameterValues(projectId, sourceTestSetId, copiedTestSetId, userId);

                //Now we need to copy across the various mapped test cases
                CopyMappedTestCases(projectId, sourceTestSetId, copiedTestSetId);

                //Now we need to copy across any custom properties
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                customPropertyManager.ArtifactCustomProperty_Copy(projectId, projectTemplateId, sourceTestSetId, copiedTestSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, userId);

                //Now we need to copy across any linked attachments
                AttachmentManager attachmentManager = new AttachmentManager();
                attachmentManager.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, sourceTestSetId, copiedTestSetId);

                //Send a notification
                this.SendCreationNotification(copiedTestSetId, null, null);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the test set id of the copy
                return copiedTestSetId;
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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
        /// Updates the folder that a test set is linked to
        /// </summary>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="folderId">The id of the folder</param>
        /// <remarks>Folder changes are not tracked in the artifact history for test sets</remarks>
        public void TestSet_UpdateFolder(int testSetId, int? folderId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "TestSet_UpdateFolder";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int? projectId = null;
                int? oldTestSetFolderId = null;
                int? newTestSetFolderId = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Retrieve the testSet
                    var query = from t in context.TestSets
                                where t.TestSetId == testSetId && (!t.IsDeleted || includeDeleted)
                                select t;

                    TestSet testSet = query.FirstOrDefault();
                    if (testSet != null)
                    {
                        projectId = testSet.ProjectId;
                        oldTestSetFolderId = testSet.TestSetFolderId;
                        newTestSetFolderId = folderId;
                        testSet.StartTracking();
                        testSet.TestSetFolderId = folderId;

                        //We don't need to log history for this
                        context.SaveChanges();
                    }
                }

                //Now roll up the execution status to the folders
                if (oldTestSetFolderId.HasValue && projectId.HasValue)
                {
                    this.TestSetFolder_RefreshExecutionData(projectId.Value, oldTestSetFolderId.Value);
                }
                if (newTestSetFolderId.HasValue && projectId.HasValue)
                {
                    this.TestSetFolder_RefreshExecutionData(projectId.Value, newTestSetFolderId.Value);
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

        #region Test Set Folder functions

        /// <summary>
        /// Deletes a folder in the system, cascading any deletes to the child folders
        /// </summary>
        /// <param name="userId">The id of the user doing the delete</param>
        /// <param name="folderId">The id of the folder</param>
        /// <param name="projectId">The id of the current project</param>
        public void TestSetFolder_Delete(int projectId, int folderId, int userId)
        {
            const string METHOD_NAME = "TestSetFolder_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get this folder and any child ones
                    List<TestSetFolderHierarchyView> childFolders = context.TestSet_RetrieveChildFolders(projectId, folderId, true).OrderByDescending(t => t.IndentLevel).ToList();

                    //If we have child folders, we need to delete them recursively
                    for (int i = 0; i < childFolders.Count; i++)
                    {
                        //The database will unset the Folder Id of the test sets using the database cascade

                        //Just do a 'detached' object delete by id
                        TestSetFolder folderToDelete = new TestSetFolder();
                        folderToDelete.TestSetFolderId = childFolders[i].TestSetFolderId;
                        context.TestSetFolders.Attach(folderToDelete);
                        context.ObjectStateManager.ChangeObjectState(folderToDelete, EntityState.Deleted);
                        context.SaveChanges();
                    }
                }

                //Next refresh the folder hierarchy cache
                TestSetFolder_RefreshHierarchy(projectId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Gets a folder by its id
        /// </summary>
        /// <param name="folderId">The folder id</param>
        /// <returns>A folder or null if it doesn't exist</returns>
        public TestSetFolder TestSetFolder_GetById(int folderId)
        {
            const string METHOD_NAME = "TestSetFolder_GetById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetFolder folder;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from f in context.TestSetFolders
                                where f.TestSetFolderId == folderId
                                select f;
                    folder = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folder;
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
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

        /// <summary> 
        /// Checks if a folder exists by its id in the specified project 
        /// </summary> 
        /// <param name="projectId">The folder id</param> 
        /// <param name="folderId">The folder id</param> 
        /// <returns>bool of true if the folder exists in the project</returns> 
        public bool TestSetFolder_Exists(int projectId, int folderId)
        {
            const string METHOD_NAME = "TestSetFolder_Exists";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetFolder folder;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from f in context.TestSetFolders
                                where f.TestSetFolderId == folderId && f.ProjectId == projectId
                                select f;
                    folder = query.FirstOrDefault();
                }

                //Make sure data was returned 
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folder == null ? false : true;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "Looking for TestSetFolder");
                throw;
            }
        }

        /// <summary>Exports all the test set folders (not test sets) from one project to another</summary>
        /// <param name="userId">The user exporting the test set folder</param>
        /// <param name="sourceProjectId">The project we're exporting from</param>
        /// <param name="destProjectId">The project we're exporting to</param>
        /// <param name="testFolderMapping">A dictionary used to keep track of any exported test folders</param>
        /// <param name="parentFolder">The id of the parent folder (null = root)</param>
        /// <remarks>Used by the Project Copy function</remarks>
        protected internal void TestSetFolder_ExportAll(int userId, int sourceProjectId, int destProjectId, Dictionary<int, int> testFolderMapping, int? parentFolder = null)
        {
            const string METHOD_NAME = "TestSetFolder_ExportAll";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to retrieve the immediate child folders
                List<TestSetFolder> sourceTestSetFolders = TestSetFolder_GetByParentId(sourceProjectId, parentFolder);

                foreach (TestSetFolder sourceTestSetFolder in sourceTestSetFolders)
                {
                    int? destParentFolderId = null;
                    if (sourceTestSetFolder.ParentTestSetFolderId.HasValue)
                    {
                        if (testFolderMapping.ContainsKey(sourceTestSetFolder.ParentTestSetFolderId.Value))
                        {
                            destParentFolderId = testFolderMapping[sourceTestSetFolder.ParentTestSetFolderId.Value];
                        }
                    }

                    //Firstly export the test folder itself
                    int exportedTestFolderId = this.TestSetFolder_Create(
                        sourceTestSetFolder.Name,
                        destProjectId,
                        sourceTestSetFolder.Description,
                        destParentFolderId).TestSetFolderId;

                    //Add to the mapping
                    if (!testFolderMapping.ContainsKey(sourceTestSetFolder.TestSetFolderId))
                    {
                        testFolderMapping.Add(sourceTestSetFolder.TestSetFolderId, exportedTestFolderId);
                    }

                    //Copy the folder into the new folder, leaving the name unchanged
                    this.TestSetFolder_ExportAll(userId, sourceProjectId, destProjectId, testFolderMapping, sourceTestSetFolder.TestSetFolderId);
                }

                //Next refresh the folder hierarchy cache
                TestSetFolder_RefreshHierarchy(destProjectId);

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
        /// Gets the list of all folders in the project according to their hierarchical relationship, in alphabetical order (per-level)
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>List of folders</returns>
        public List<TestSetFolderHierarchyView> TestSetFolder_GetList(int projectId)
        {
            const string METHOD_NAME = "TestSetFolder_GetList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetFolderHierarchyView> folders;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from f in context.TestSetFoldersHierarchyView
                                where f.ProjectId == projectId
                                orderby f.IndentLevel, f.TestSetFolderId
                                select f;

                    folders = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folders;
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
        /// Refreshes the test set folder hierachy in a project after folders are changed
        /// </summary>
        /// <param name="projectId">The ID of the current project</param>
        public void TestSetFolder_RefreshHierarchy(int projectId)
        {
            const string METHOD_NAME = "TestSetFolder_RefreshHierarchy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Set a longer timeout for this as it's run infrequently to speed up retrieves
                    context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
                    context.TestSet_RefreshFolderHierarchy(projectId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                //Don't throw, just log
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                //Don't throw, just log
            }
        }

        /// <summary>
        /// Gets the list of all parents of the specified folder in hierarchy order
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>List of folders</returns>
        public List<TestSetFolderHierarchyView> TestSetFolder_GetParents(int projectId, int testFolderId, bool includeSelf = false)
        {
            const string METHOD_NAME = "TestSetFolder_GetParents";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetFolderHierarchyView> folders;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    folders = context.TestSet_RetrieveParentFolders(projectId, testFolderId, includeSelf).ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folders;
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
        /// Gets the list of all children of the specified folder in hierarchy order
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <returns>List of folders</returns>
        public List<TestSetFolderHierarchyView> TestSetFolder_GetChildren(int projectId, int testFolderId, bool includeSelf = false)
        {
            const string METHOD_NAME = "TestSetFolder_GetChildren";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetFolderHierarchyView> folders;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    folders = context.TestSet_RetrieveChildFolders(projectId, testFolderId, includeSelf).ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folders;
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
        /// Commits any changes made to a folder entity
        /// </summary>
        /// <param name="folder">The folder entity</param>
        /// <remarks>Can be used for both reordering the folder and making updates to the specific one</remarks>
        public void TestSetFolder_Update(TestSetFolder folder)
        {
            const string METHOD_NAME = CLASS_NAME + "TestSetFolder_Update(TestSetFolder)";
            Logger.LogEnteringEvent(METHOD_NAME);

            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                try
                {
                    //If the folder has switched from a root one, make sure that there is at least one other folder in the project
                    if (folder.ChangeTracker.OriginalValues.ContainsKey("ParentTestSetFolderId"))
                    {
                        if (folder.ChangeTracker.OriginalValues["ParentTestSetFolderId"] == null && folder.ParentTestSetFolderId.HasValue)
                        {
                            //See how many root folders we have
                            var query = from t in context.TestSetFolders
                                        where t.ProjectId == folder.ProjectId && !t.ParentTestSetFolderId.HasValue && t.TestSetFolderId != folder.TestSetFolderId
                                        select t;

                            //Make sure we have one remaining
                            if (query.Count() < 1)
                            {
                                throw new ProjectDefaultTestSetFolderException("You need to have at least one top-level test set folder in the project");
                            }
                        }

                        //Make sure the new parent folder is not a child of the current one (would create circular loop)
                        if (folder.ParentTestSetFolderId.HasValue)
                        {
                            List<TestSetFolderHierarchyView> childFolders = this.TestSetFolder_GetChildren(folder.ProjectId, folder.TestSetFolderId, false);
                            if (childFolders.Any(f => f.TestSetFolderId == folder.ParentTestSetFolderId.Value))
                            {
                                throw new FolderCircularReferenceException(GlobalResources.Messages.TestSet_CannotMakeParentChildOfCurrentTestSet);
                            }
                        }
                    }

                    context.TestSetFolders.ApplyChanges(folder);
                    context.SaveChanges();

                    //Next refresh the folder hierarchy cache
                    TestSetFolder_RefreshHierarchy(folder.ProjectId);
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEvent(METHOD_NAME, ex, "Saving folder:");
                    throw;
                }
            }

            Logger.LogExitingEvent(METHOD_NAME);
        }

        /// <summary>Copies a test folder (and its child test sets) from one location to another. Will not copy deleted test sets.</summary>
        /// <param name="userId">The user that is making the copy</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="sourceTestFolderId">The folder we want to copy</param>
        /// <param name="destTestSetFolderId">The folder we want to copy it into (null = root)</param>
        /// <param name="prependName">Should we prepend 'Copy of ...' to name</param>
        /// <returns>The id of the copy of the test folder</returns>
        public int TestSetFolder_Copy(int userId, int projectId, int sourceTestFolderId, int? destTestSetFolderId, bool prependName = true)
        {
            const string METHOD_NAME = "TestSetFolder_Copy";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First we need to retrieve the test folder being copied
                TestSetFolder sourceTestSetFolder = TestSetFolder_GetById(sourceTestFolderId);

                string name = sourceTestSetFolder.Name;
                if (prependName)
                {
                    name = name + CopiedArtifactNameSuffix;
                }

                //Firstly copy the test folder itself
                int copiedTestFolderId = this.TestSetFolder_Create(
                    name,
                    projectId,
                    sourceTestSetFolder.Description,
                    destTestSetFolderId).TestSetFolderId;

                //Next we need to copy all the child test sets
                List<TestSetView> childTestSets = this.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, sourceTestFolderId);
                foreach (TestSetView childTestSet in childTestSets)
                {
                    //Copy the test set, leaving its name unchanged
                    this.TestSet_Copy(userId, projectId, childTestSet.TestSetId, copiedTestFolderId, false);
                }

                //Next we need to recursively call this function for any child test set folders
                List<TestSetFolder> childTestFolders = TestSetFolder_GetByParentId(projectId, sourceTestFolderId);
                if (childTestFolders != null && childTestFolders.Count > 0)
                {
                    foreach (TestSetFolder childTestFolder in childTestFolders)
                    {
                        //Copy the folder into the new folder, leaving the name unchanged
                        this.TestSetFolder_Copy(userId, projectId, childTestFolder.TestSetFolderId, copiedTestFolderId, false);
                    }
                }

                //Next refresh the folder hierarchy cache
                TestSetFolder_RefreshHierarchy(projectId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the test set id of the copy
                return copiedTestFolderId;
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                throw;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }


        /// <summary>Creates a new test set folder</summary>
        /// <param name="name">Name of the folder.</param>
        /// <param name="description">The description of the folder</param>
        /// <returns>The newly created testSet folder</returns>
        /// <param name="projectId">The id of the project</param>
        /// <param name="parentTestSetFolderId">The id of a parent folder (null = top-level folder)</param>
        public TestSetFolder TestSetFolder_Create(string name, int projectId, string description, int? parentTestSetFolderId = null)
        {
            const string METHOD_NAME = "TestSetFolder_Create";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            TestSetFolder testSetFolder = new TestSetFolder();

            try
            {

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create a new testSet folder entity
                    Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Creating new TestSet Folder Entity");

                    testSetFolder.Name = name;
                    testSetFolder.Description = description;
                    testSetFolder.ProjectId = projectId;
                    testSetFolder.ParentTestSetFolderId = parentTestSetFolderId;
                    testSetFolder.CountBlocked = 0;
                    testSetFolder.CountCaution = 0;
                    testSetFolder.CountFailed = 0;
                    testSetFolder.CountNotApplicable = 0;
                    testSetFolder.CountNotRun = 0;
                    testSetFolder.CountPassed = 0;
                    testSetFolder.LastUpdateDate = DateTime.UtcNow;
                    testSetFolder.CreationDate = DateTime.UtcNow;

                    //Persist the new article folder
                    context.TestSetFolders.AddObject(testSetFolder);
                    context.SaveChanges();

                    //Persist changes
                    context.SaveChanges();
                }

                //Next refresh the folder hierarchy cache
                TestSetFolder_RefreshHierarchy(projectId);
            }
            catch (EntityForeignKeyException exception)
            {
                //This exception occurs if the parent has been deleted, throw a business exception
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw new ArtifactNotExistsException("The parent test set folder specified no longer exists", exception);
            }
            catch (EntityException exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return testSetFolder;
        }

        /// <summary>
        /// Gets the list of child folders for a parent folder. If the folder id is null it gets the root folder
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="folderId">The parent folder id (or null for root folder)</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <returns>List of folders</returns>
        public List<TestSetFolder> TestSetFolder_GetByParentId(int projectId, int? folderId, string sortProperty = null, bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
        {
            const string METHOD_NAME = "TestSetFolder_GetByParentId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetFolder> folders;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //We have to use this syntax because using int? == int? comparison in EF4 LINQ will
                    //result in invalid SQL (== NULL instead of IS NULL)
                    IQueryable<TestSetFolder> query;
                    if (folderId.HasValue)
                    {
                        query = from f in context.TestSetFolders
                                where f.ParentTestSetFolderId == folderId.Value && f.ProjectId == projectId
                                select f;
                    }
                    else
                    {
                        query = from f in context.TestSetFolders
                                where f.ParentTestSetFolderId == null && f.ProjectId == projectId
                                select f;
                    }

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by name ascending
                        query = query.OrderBy(f => f.Name).ThenBy(f => f.TestSetFolderId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestSetFolderId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetFolder, bool>> filterClause = CreateFilterExpression<TestSetFolder>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestFolderSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetFolder>)query.Where(filterClause);
                        }
                    }

                    folders = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folders;
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
        /// Gets the list of child folders for a parent folder. If the folder id is null it gets the root folder
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="folderId">The parent folder id (or null for root folder)</param>
        /// <param name="utcOffset">The offset from UTC</param>
        /// <param name="releaseId">The id of the release we're interested in</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <returns>List of folders</returns>
        public List<TestSetFolderReleaseView> TestSetFolder_GetByParentIdForRelease(int projectId, int? folderId, int releaseId, string sortProperty = null, bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
        {
            const string METHOD_NAME = "TestSetFolder_GetByParentIdForRelease";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetFolderReleaseView> folders;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //We have to use this syntax because using int? == int? comparison in EF4 LINQ will
                    //result in invalid SQL (== NULL instead of IS NULL)
                    IQueryable<TestSetFolderReleaseView> query;
                    if (folderId.HasValue)
                    {
                        query = from f in context.TestSetFolderReleasesView
                                where f.ParentTestSetFolderId == folderId.Value && f.ProjectId == projectId && f.DisplayReleaseId == releaseId
                                select f;
                    }
                    else
                    {
                        query = from f in context.TestSetFolderReleasesView
                                where f.ParentTestSetFolderId == null && f.ProjectId == projectId && f.DisplayReleaseId == releaseId
                                select f;
                    }

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by name ascending
                        query = query.OrderBy(f => f.Name).ThenBy(f => f.TestSetFolderId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestSetFolderId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
                        
                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestSetFolderReleaseView, bool>> filterClause = CreateFilterExpression<TestSetFolderReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, filters, utcOffset, null, HandleTestFolderSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestSetFolderReleaseView>)query.Where(filterClause);
                        }
                    }

                    folders = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return folders;
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

        #endregion

        #region Test Set Test Case functions

        /// <summary>
        /// Updates the planned date of a test case in a test set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set</param>
        /// <param name="testSetTestCaseId">The id of the test case in the test set (instance id)</param>
        /// <param name="plannedDate">The date to set it to (or null)</param>
        public void UpdateTestCasePlannedDate(int projectId, int testSetId, int testSetTestCaseId, DateTime? plannedDate)
        {
            const string METHOD_NAME = "RetrieveTestCaseById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetTestCase testSetTestCase;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSet.ProjectId == projectId && t.TestSetId == testSetId && t.TestSetTestCaseId == testSetTestCaseId
                                select t;

                    testSetTestCase = query.FirstOrDefault();

                    if (testSetTestCase != null)
                    {
                        //Save without changing history (test set test case has no history)
                        testSetTestCase.StartTracking();
                        testSetTestCase.PlannedDate = plannedDate;
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
        /// Retrieves a single test case mapped to a test set
        /// </summary>
        /// <param name="includeDeleted">Should we include a deleted record</param>
        /// <param name="testCaseTestSetId">The id of the test set test case record</param>
        /// <returns>The test set test case</returns>
        public TestSetTestCase RetrieveTestCaseById(int testCaseTestSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveTestCaseById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetTestCase testSetTestCase;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the mapped test cases
                    var query = from t in context.TestSetTestCases.Include(t => t.TestCase).Include(t => t.Owner)
                                where (!t.TestCase.IsDeleted || includeDeleted) && t.TestSetTestCaseId == testCaseTestSetId
                                select t;

                    testSetTestCase = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCase;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a single test case view mapped to a test set
        /// </summary>
        /// <param name="includeDeleted">Should we include a deleted record</param>
        /// <param name="testCaseTestSetId">The id of the test set test case record</param>
        /// <returns>The test set test case</returns>
        public TestSetTestCaseView RetrieveTestCaseById2(int testCaseTestSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveTestCaseById2";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestSetTestCaseView testSetTestCase;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the mapped test cases
                    var query = from t in context.TestSetTestCasesView
                                where (!t.IsTestCaseDeleted || includeDeleted) && t.TestSetTestCaseId == testCaseTestSetId
                                select t;

                    testSetTestCase = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCase;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the list of mapped test cases for a particular test set</summary>
        /// <param name="testSetId">The id of the test set in question</param>
        /// <returns>A list of mapped test cases</returns>
        /// <remarks>This does not provide the execution data. The pages displaying the test case list use the TestSet.RetrieveTestCases3(int, int, int?) method instead</remarks>
        public List<TestSetTestCase> RetrieveTestCases2(int testSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveTestCases2";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetTestCase> testSetTestCases;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the mapped test cases
                    var query = from t in context.TestSetTestCases.Include(t => t.TestCase)
                                where (!t.TestCase.IsDeleted || includeDeleted) && t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    testSetTestCases = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCases;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the list of mapped test cases for a particular test set</summary>
        /// <param name="testSetId">The id of the test set in question</param>
        /// <returns>A list of mapped test cases</returns>
        /// <remarks>This does not provide the execution data. The pages displaying the test case list use the TestSet.RetrieveTestCases3(int, int, int?) method instead</remarks>
        public List<TestSetTestCaseView> RetrieveTestCases(int testSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveTestCases";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetTestCaseView> testSetTestCases;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the mapped test cases
                    var query = from t in context.TestSetTestCasesView
                                where (!t.IsTestCaseDeleted || includeDeleted) && t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    testSetTestCases = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCases;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves the list of mapped test cases for a particular test set</summary>
        /// <param name="testSetId">The id of the test set in question</param>
        /// <returns>A list of mapped test cases</returns>
        /// <param name="includeDeleted">Should we include deleted test cases</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="releaseId">Are we filtering by release (null = all releases)</param>
        /// <remarks>Includes the execution data and returns the execution status based on release</remarks>
        public List<TestSetTestCaseView> RetrieveTestCases3(int projectId, int testSetId, int? releaseId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveTestCases3";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestSetTestCaseView> testSetTestCases;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Called stored procedure to retrieve test cases with execution data
                    testSetTestCases = context.TestSet_RetrieveTestCases(projectId, testSetId, releaseId, includeDeleted).ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testSetTestCases;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Updates the owners associated with specific test cases in the test set
        /// </summary>
        /// <param name="testSetTestCases">The list of test set test cases</param>
        /// <param name="userId">The id of the user making the change</param>
        /// <remarks>Also updates the last updated date of the test set itself</remarks>
        public void UpdateTestCases (List<TestSetTestCase> testSetTestCases, int userId)
        {
            const string METHOD_NAME = "UpdateTestCases()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Loop through and apply the changes
                    List<int> testSetIds = new List<int>();
                    foreach (TestSetTestCase testSetTestCase in testSetTestCases)
                    {
                        context.TestSetTestCases.ApplyChanges(testSetTestCase);
                        if (!testSetIds.Contains(testSetTestCase.TestSetId))
                        {
                            testSetIds.Add(testSetTestCase.TestSetId);
                        }
                    }

                    //Update the last updated date of the test sets
                    //We leave the concurrency date alone since the rest of the test set is not changing
                    var query = from t in context.TestSets
                                where testSetIds.Contains(t.TestSetId)
                                select t;

                    List<TestSet> testSets = query.ToList();
                    foreach (TestSet testSet in testSets)
                    {
                        testSet.StartTracking();
                        testSet.LastUpdateDate = DateTime.UtcNow;
                    }
                    
                    //Save
                    context.SaveChanges(userId, false, false, null);
                }
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

        		/// <summary>
		/// Adds a test case to a test set at the specified position (or at end if no existing item specified)
		/// </summary>
		/// <param name="testCaseId">The test case we want to add</param>
		/// <param name="testSetId">The test set in question</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="ownerId">The id of an owner if we don't want to use the test set default (optional)</param>
		/// <param name="existingTestSetTestCaseId">The id of the existing entry we want to insert it before (optional)</param>
		/// <param name="parameterValues">Any parameter values to set on the test case</param>
		/// <remarks>
		/// If the we are passed in the id of a test folder, we add all the child test cases,
		/// ignoring any duplicates.
		/// </remarks>
		/// <returns>The unique test set test case instance ids of the added test cases. Key=TestCaseId, Value=TestCaseTestSetId</returns>
		/// <exception cref="ArtifactNotExistsException">Thrown if the passed in test case doesn't exist</exception>
		/// <exception cref="TestSetDuplicateTestCasePositionException">Thrown when an added test case is added to a position already in use</exception>
        public int AddTestCase(int projectId, int testSetId, int testCaseId, int? ownerId, int? existingTestSetTestCaseId, Dictionary<string, string> parameterValues = null)
        {
            const string METHOD_NAME = "AddTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testSetTestCaseId;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    List<TestSetTestCase> existingTestSetTestCases = query.ToList();

                    //Make sure the test case exists
                    var query2 = from t in context.TestCases
                                 where t.TestCaseId == testCaseId
                                 select t;

                    TestCase testCase = query2.FirstOrDefault();

                    if (testCase == null)
                    {
                        throw new ArtifactNotExistsException("Test Case TC" + testCase + " does not exist!");
                    }
                    //Make the new position, the last position in the list unless there is an item we're inserting in front of
                    int position = 1;
                    if (existingTestSetTestCases.Count > 0)
                    {
                        position = existingTestSetTestCases.Last().Position + 1;
                    }

                    //Begin transaction - needed to maintain integrity of position ordering
                    using (TransactionScope transactionScope = new TransactionScope())
                    {
                        //If we have an existing item, need to first move the existing items down in the list
                        //so that there is a position number available
                        if (existingTestSetTestCaseId.HasValue)
                        {
                            position = MoveTestCasesDown(context, existingTestSetTestCases, existingTestSetTestCaseId.Value, 1);
                        }

                        //Add the test case to the test set at the specified position
                        TestSetTestCase testSetTestCase = new TestSetTestCase();
                        testSetTestCase.TestSetId = testSetId;
                        testSetTestCase.TestCaseId = testCaseId;
                        testSetTestCase.Position = position;
                        testSetTestCase.OwnerId = ownerId;
                        context.TestSetTestCases.AddObject(testSetTestCase);

                        //Save the changes
                        context.SaveChanges();

                        //Commit the transaction
                        transactionScope.Complete();

                        testSetTestCaseId = testSetTestCase.TestSetTestCaseId;
                    }
                }

                //Now we need to add any of the parameter values
                if (parameterValues != null)
                {
                    //First retrieve the list of parameters, both those directly defined
                    //on the child test case and those inherited (including those already set)
                    List<TestSetTestCaseParameter> testSetTestCaseParameters = new List<TestSetTestCaseParameter>();
                    List<TestCaseParameter> testCaseParameters = new TestCaseManager().RetrieveParameters(testCaseId, true, true);
                    foreach (TestCaseParameter testCaseParameter in testCaseParameters)
                    {
                        //See if we have a match for this parameter name in the dictionary
                        if (parameterValues.ContainsKey(testCaseParameter.Name))
                        {
                            string parameterValue = parameterValues[testCaseParameter.Name];
                            TestSetTestCaseParameter testSetTestCaseParameter = new TestSetTestCaseParameter();
                            testSetTestCaseParameter.TestSetTestCaseId = testSetTestCaseId;
                            testSetTestCaseParameter.TestCaseParameterId = testCaseParameter.TestCaseParameterId;
                            testSetTestCaseParameter.Value = parameterValue;
                            testSetTestCaseParameters.Add(testSetTestCaseParameter);
                        }
                    }
                    SaveTestCaseParameterValues(testSetTestCaseParameters);
                }

                //Finally we need to update the test set's last updated date and test case status
                UpdateLastUpdatedDate(testSetId);
                TestSet_RefreshExecutionData(projectId, testSetId);

                //Return the new test set test case id
                return testSetTestCaseId;
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestSet_TestCasePositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestSet_TestCasePositionInUse, exception);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds test cases to a test set at the specified position (or at end if no existing item specified)
        /// </summary>
        /// <param name="testCaseIds">The test cases we want to add</param>
        /// <param name="testSetId">The test set in question</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="ownerId">The id of an owner if we don't want to use the test set default (optional)</param>
        /// <param name="existingTestSetTestCaseId">The id of the existing entry we want to insert it before (optional)</param>
        /// <returns>The unique test set test case instance ids of the added test cases. Key=TestCaseId, Value=TestCaseTestSetId</returns>
        /// <exception cref="ArtifactNotExistsException">Thrown if the passed in test case doesn't exist</exception>
        /// <exception cref="TestSetDuplicateTestCasePositionException">Thrown when an added test case is added to a position already in use</exception>
        public Dictionary<int, int> AddTestCases(int projectId, int testSetId, List<int> testCaseIds, int? ownerId, int? existingTestSetTestCaseId, int? userId = null)
        {
            const string METHOD_NAME = "AddTestCases";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Stores the mapping between TestCaseId and TestSetTestCaseId
                Dictionary<int, int> mappingInfo = new Dictionary<int, int>();

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    List<TestSetTestCase> existingTestSetTestCases = query.ToList();

                    //Make sure the test cases exists
                    var query2 = from t in context.TestCases
                                 where testCaseIds.Contains(t.TestCaseId)
                                 orderby t.TestCaseId
                                 select t.TestCaseId;

                    List<int> testCaseIdsThatExist = query2.ToList();

                    if (testCaseIdsThatExist.Count > 0)
                    {
                        //Make the new position, the last position in the list unless there is an item we're inserting in front of
                        int position = 1;
                        if (existingTestSetTestCases.Count > 0)
                        {
                            position = existingTestSetTestCases.Last().Position + 1;
                        }

                        //Begin transaction - needed to maintain integrity of position ordering
                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            //If we have an existing item, need to first move the existing items down in the list
                            //so that there is a position number available
                            if (existingTestSetTestCaseId.HasValue)
                            {
                                position = MoveTestCasesDown(context, existingTestSetTestCases, existingTestSetTestCaseId.Value, testCaseIdsThatExist.Count);
                            }

                            //Add the test cases to the test set at the specified position
                            List<TestSetTestCase> addedTestCases = new List<TestSetTestCase>();
                            foreach (int testCaseId in testCaseIdsThatExist)
                            {
                                TestSetTestCase testSetTestCase = new TestSetTestCase();
                                testSetTestCase.TestSetId = testSetId;
                                testSetTestCase.TestCaseId = testCaseId;
                                testSetTestCase.Position = position;
                                testSetTestCase.OwnerId = ownerId;
                                context.TestSetTestCases.AddObject(testSetTestCase);

                                //Add to list to extract id mapping
                                addedTestCases.Add(testSetTestCase);

                                //Increment the position
                                position++;

								new HistoryManager().LogCreation(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, DateTime.UtcNow, DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId);
							}

                            //Save the changes
                            context.SaveChanges();

							//Commit the transaction
							transactionScope.Complete();

                            //Add to the mapping
                            foreach (TestSetTestCase addedTestSetTestCase in addedTestCases)
                            {
                                if (!mappingInfo.ContainsKey(addedTestSetTestCase.TestCaseId))
                                {
                                    mappingInfo.Add(addedTestSetTestCase.TestCaseId, addedTestSetTestCase.TestSetTestCaseId);
                                }
                            }
                        }
                    }
                }

                //Finally we need to update the test set's last updated date
                UpdateLastUpdatedDate(testSetId);
                TestSet_RefreshExecutionData(projectId, testSetId);

                return mappingInfo;
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestSet_TestCasePositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestSet_TestCasePositionInUse, exception);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds a folder of test cases to a test set at the specified position (or at end if no existing item specified)
        /// </summary>
        /// <param name="testFolderId">The id of the folder containing test cases we want to add</param>
        /// <param name="testSetId">The test set in question</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="ownerId">The id of an owner if we don't want to use the test set default (optional)</param>
        /// <param name="existingTestSetTestCaseId">The id of the existing entry we want to insert it before (optional)</param>
        /// <exception cref="ArtifactNotExistsException">Thrown if the passed in test case doesn't exist</exception>
        /// <exception cref="TestSetDuplicateTestCasePositionException">Thrown when an added test case is added to a position already in use</exception>
        public void AddTestFolder(int projectId, int testSetId, int testFolderId, int? ownerId, int? existingTestSetTestCaseId)
        {
            const string METHOD_NAME = "AddTestFolder";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    List<TestSetTestCase> existingTestSetTestCases = query.ToList();

                    //Next get all of the test cases in the folder (including those in sub-folders)
                    TestCaseManager testCaseManager = new TestCaseManager();
                    List<TestCase> testCasesInFolder = testCaseManager.RetrieveAllInFolder(projectId, testFolderId);

                    if (testCasesInFolder.Count > 0)
                    {
                        //Make the new position, the last position in the list unless there is an item we're inserting in front of
                        int position = 1;
                        if (existingTestSetTestCases.Count > 0)
                        {
                            position = existingTestSetTestCases.Last().Position + 1;
                        }

                        //Begin transaction - needed to maintain integrity of position ordering
                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            //If we have an existing item, need to first move the existing items down in the list
                            //so that there is a position number available
                            if (existingTestSetTestCaseId.HasValue)
                            {
                                position = MoveTestCasesDown(context, existingTestSetTestCases, existingTestSetTestCaseId.Value, 1);
                            }

                            //Add the test cases to the test set at the specified position
                            List<TestSetTestCase> addedTestCases = new List<TestSetTestCase>();
                            foreach (TestCase testCase in testCasesInFolder)
                            {
                                TestSetTestCase testSetTestCase = new TestSetTestCase();
                                testSetTestCase.TestSetId = testSetId;
                                testSetTestCase.TestCaseId = testCase.TestCaseId;
                                testSetTestCase.Position = position;
                                testSetTestCase.OwnerId = ownerId;
                                context.TestSetTestCases.AddObject(testSetTestCase);

                                //Add to list to extract id mapping and increment position
                                addedTestCases.Add(testSetTestCase);
                                position++;
                            }

                            //Save the changes
                            context.SaveChanges();

                            //Commit the transaction
                            transactionScope.Complete();
                        }
                    }
                }

                //Finally we need to update the test set's last updated date
                UpdateLastUpdatedDate(testSetId);
                TestSet_RefreshExecutionData(projectId, testSetId);
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestSet_TestCasePositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestSet_TestCasePositionInUse, exception);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Moves the test cases in the set down by the specified number of positions (from a certain point onwards)
        /// </summary>
        /// <param name="existingTestSetTestCases">The list of the test cases in the set</param>
        /// <param name="existingTestSetTestCaseId">The id of the test set where we want to start the moving (inclusive of this one)</param>
        /// <param name="positionOffset">The number of positions to move down</param>
        /// <param name="context">The EF context</param>
        /// <returns>The first position number made available</returns>
        protected int MoveTestCasesDown(SpiraTestEntitiesEx context, List<TestSetTestCase> existingTestSetTestCases, int existingTestSetTestCaseId, int positionOffset)
        {
            const string METHOD_NAME = "MoveTestCasesDown";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Iterate through the list of test cases until we get the match, moving them down
            //Need to use reverse order to avoid duplicate position numbers
            int availablePosition = 1;
            for (int i = existingTestSetTestCases.Count - 1; i >= 0; i--)
            {
                //Make changes
                TestSetTestCase existingTestSetTestCase = existingTestSetTestCases[i];
                existingTestSetTestCase.StartTracking();

                //Move the test case down the appropriate number of positions
                availablePosition = existingTestSetTestCases[i].Position;
                existingTestSetTestCases[i].Position += positionOffset;

                //Make the database changes, need to save after each one to ensure they are saved in the correct order
                context.SaveChanges();

                //If we are on the specified item, stop here
                if (existingTestSetTestCases[i].TestSetTestCaseId == existingTestSetTestCaseId)
                {
                    break;
                }
            }
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return availablePosition;
        }

        /// <summary>Moves a test case in the test set list</summary>
        /// <param name="sourceTestSetTestCaseId">The id of the testSetTestCase entry we want to move</param>
        /// <param name="destTestSetTestCaseId">The destination of where we want to move it to (null means end of list)</param>
        /// <param name="testSetId">The id of the test set we're interested in</param>
        /// <remarks>Throws an exception if you pass-in a non-existant source test case entry</remarks>
        public void MoveTestCase(int testSetId, int sourceTestSetTestCaseId, int? destTestSetTestCaseId)
        {
            const string METHOD_NAME = "MoveTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    List<TestSetTestCase> existingTestSetTestCases = query.ToList();
                    if (existingTestSetTestCases.Count < 1)
                    {
                        //End if there are no test cases to move!
                        return;
                    }

                    //Make sure that we have the row to be moved in this dataset
                    TestSetTestCase sourceTestSetCaseRow = existingTestSetTestCases.FirstOrDefault(t => t.TestSetTestCaseId == sourceTestSetTestCaseId);
                    if (sourceTestSetCaseRow == null)
                    {
                        throw new ArtifactNotExistsException(GlobalResources.Messages.TestSet_TestSetTestCaseIdNotExist);
                    }
                    int sourcePosition = sourceTestSetCaseRow.Position;

                    //Now lets see if we have a destination row as well
                    //If not, the dest position is simply the last position in the list
                    int destPosition = existingTestSetTestCases.Count;
                    TestSetTestCase destTestSetCaseRow = null;
                    if (destTestSetTestCaseId.HasValue)
                    {
                        destTestSetCaseRow = existingTestSetTestCases.FirstOrDefault(t => t.TestSetTestCaseId == destTestSetTestCaseId.Value);
                        if (destTestSetCaseRow != null)
                        {
                            destPosition = destTestSetCaseRow.Position;
                        }
                    }

                    //Make sure the source and dest are not the same (in which case we have nothing to do)
                    if (sourcePosition == destPosition)
                    {
                        return;
                    }

                    //Begin transaction - needed to maintain integrity of position ordering
                    using (TransactionScope transactionScope = new TransactionScope())
                    {
                        //First we need to move the source test case to the end of the list
                        int tempPosition = existingTestSetTestCases[existingTestSetTestCases.Count - 1].Position + 1;
                        sourceTestSetCaseRow.StartTracking();
                        sourceTestSetCaseRow.Position = tempPosition;
                        context.SaveChanges();

                        //If the destination is after the source in position, need to move items between the two up in position
                        //otherwise need to moveitems between the two down in position
                        bool sourceTestCaseMoved = false;
                        if (destPosition > sourcePosition)
                        {
                            //Need to decrement by one because we are removing a position (unless move to end)
                            if (destTestSetCaseRow != null)
                            {
                                destPosition--;
                            }
                            for (int i = 0; i < existingTestSetTestCases.Count; i++)
                            {
                                if (existingTestSetTestCases[i].Position > sourcePosition && existingTestSetTestCases[i].Position <= destPosition)
                                {
                                    //Move the item one up in the list
                                    //If we are moving the source item, set the flag
                                    if (existingTestSetTestCases[i] == sourceTestSetCaseRow)
                                    {
                                        sourceTestCaseMoved = true;
                                    }
                                    TestSetTestCase itemToMove = existingTestSetTestCases[i];
                                    itemToMove.StartTracking();
                                    itemToMove.Position--;

                                    //Make the database change
                                    context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            //Need to iterate in reverse order to avoid duplicate position values
                            for (int i = existingTestSetTestCases.Count - 1; i >= 0; i--)
                            {
                                if (existingTestSetTestCases[i].Position >= destPosition && existingTestSetTestCases[i].Position < sourcePosition)
                                {
                                    //Move the item one down in the list
                                    TestSetTestCase itemToMove = existingTestSetTestCases[i];
                                    itemToMove.StartTracking();
                                    itemToMove.Position++;

                                    //Make the database change
                                    context.SaveChanges();
                                }
                            }
                        }


                        //Finally if the source item was not already repositioned, need to set its position to the destination
                        if (!sourceTestCaseMoved)
                        {
                            sourceTestSetCaseRow.StartTracking();
                            sourceTestSetCaseRow.Position = destPosition;
                            //Make the database changes
                            context.SaveChanges();
                        }

                        //Commit transaction - needed to maintain integrity of position ordering
                        transactionScope.Complete();
                    }

                    //Finally we need to update the test set's last updated date
                    UpdateLastUpdatedDate(testSetId);
                }
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestSet_TestCasePositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestSet_TestCasePositionInUse, exception);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Toggles which columns are hidden/visible for the current user/project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="fieldName">The field name to toggle show/hide</param>
        /// <param name="userId">The id of the user</param>
        public void ToggleColumnVisibility(int userId, int projectId, string fieldName)
        {
            const string METHOD_NAME = "ToggleColumnVisibility";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {

                //Now we need to see which columns are visible from project settings
                ProjectSettingsCollection projectSettings = new ProjectSettingsCollection(projectId, userId, "TestSetDetails.TestCases.Columns");
                projectSettings.Restore();

                if (projectSettings[fieldName] == null)
                {
                    //Add a setting
                    bool isVisible = false;

                    if (CustomPropertyManager.IsFieldCustomProperty(fieldName).HasValue)
                    {
                        //All custom properties will be initially hidden, so need to be made visible
                        isVisible = true;
                    }
                    else
                    {
                        //All standard fields except Parameters will be initially visible
                        if (fieldName == "Parameters")
                        {
                            isVisible = true;
                        }
                        else
                        {
                            isVisible = false;
                        }
                    }

                    projectSettings.Add(fieldName, isVisible);
                }
                else
                {
                    //Toggle the setting
                    bool isVisible = (bool)projectSettings[fieldName];
                    projectSettings[fieldName] = !isVisible;
                }

                //Save any changes
                projectSettings.Save();

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
        /// Retrieves the list of columns to display in the test case list for a specific project/user with a visible/hidden flag
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>The list of columns, does not include the Name/ID columns since they are not configurable</returns>
        public List<ArtifactListFieldDisplay> RetrieveTestSetTestCaseFieldsForList(int projectId, int projectTemplateId, int userId)
        {
            const string METHOD_NAME = "RetrieveTestSetTestCaseFieldsForList";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First add the standard fields
                List<ArtifactListFieldDisplay> artifactFields = new List<ArtifactListFieldDisplay>();

                //Test Case Owner
                ArtifactListFieldDisplay artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "OwnerId";
                artifactField.LookupProperty = "OwnerName";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Test Priority
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "TestCasePriorityId";
                artifactField.LookupProperty = "TestCasePriorityName";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Est. Duration
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "EstimatedDuration";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Actual Duration
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "ActualDuration";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Last Executed
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "ExecutionDate";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.DateTime;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Test Case Status
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "TestCaseStatusId";
                artifactField.LookupProperty = "TestCaseStatusName";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                artifactField.IsVisible = false; //Default
                artifactFields.Add(artifactField);

                //Test Case Type
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "TestCaseTypeId";
                artifactField.LookupProperty = "TestCaseTypeName";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Execution Status
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "ExecutionStatusId";
                artifactField.LookupProperty = "ExecutionStatusName";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Lookup;
                artifactField.IsVisible = true; //Default
                artifactFields.Add(artifactField);

                //Parameters
                artifactField = new ArtifactListFieldDisplay();
                artifactField.Name = "Parameters";
                artifactField.ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Html;
                artifactField.IsVisible = false; //Default
                artifactFields.Add(artifactField);

                //Now add the custom properties for test cases
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomProperty> testCaseCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, false);
                foreach (CustomProperty testCaseCustomProperty in testCaseCustomProperties)
                {
                    artifactField = new ArtifactListFieldDisplay();
                    artifactField.Name = testCaseCustomProperty.CustomPropertyFieldName;
                    artifactField.Caption = testCaseCustomProperty.Name;
                    artifactField.ArtifactFieldTypeId = (int)testCaseCustomProperty.FieldTypeId;
                    artifactField.IsVisible = false;    //Default
                    artifactFields.Add(artifactField);
                }
                
                //Now we need to see which columns are visible from project settings
                ProjectSettingsCollection projectSettings = new ProjectSettingsCollection(projectId, userId, "TestSetDetails.TestCases.Columns");
                projectSettings.Restore();
                foreach (DictionaryEntry entry in projectSettings)
                {
                    string fieldName = (string)entry.Key;
                    bool isVisible = (bool)entry.Value;
                    ArtifactListFieldDisplay matchingField = artifactFields.FirstOrDefault(f => f.Name == fieldName);
                    if (matchingField != null)
                    {
                        matchingField.IsVisible = isVisible;
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
        /// Removes a test case from a test set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set we're interested in</param>
        /// <param name="testSetTestCaseId">The id of the test set test case id we want to remove (this is not the test case id)</param>
        /// <exception cref="ArtifactNotExistsException">Thrown if the passed in testSetTestCaseId entry doesn't exist</exception>
        /// <exception cref="TestSetDuplicateTestCasePositionException">Thrown when removing a test case causes duplicate positions to occur</exception>
        public void RemoveTestCase(int projectId, int testSetId, int testSetTestCaseId, int? userId = null)
        {
            //Delegate to the more general method
            this.RemoveTestCase(projectId, testSetId, new List<int>() { testSetTestCaseId }, userId);
        }

        /// <summary>
        /// Removes test cases from a test set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testSetId">The id of the test set we're interested in</param>
        /// <param name="testSetTestCaseId">The id of the test set test case ids we want to remove</param>
        /// <exception cref="ArtifactNotExistsException">Thrown if the passed in testSetTestCaseId entry doesn't exist</exception>
        /// <exception cref="TestSetDuplicateTestCasePositionException">Thrown when removing a test case causes duplicate positions to occur</exception>
        public void RemoveTestCase(int projectId, int testSetId, List<int> testSetTestCaseIds, int? userId = null)
        {
            const string METHOD_NAME = "RemoveTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test cases
                    var query = from t in context.TestSetTestCases
                                where t.TestSetId == testSetId
                                orderby t.Position, t.TestSetTestCaseId
                                select t;

                    List<TestSetTestCase> existingTestSetTestCases = query.ToList();

                    //Loop through the ids
                    foreach (int testSetTestCaseId in testSetTestCaseIds)
                    {
                        //Make sure that we have the row to be deleted in this dataset, ignore otherwise
                        TestSetTestCase existingTestSetCase = existingTestSetTestCases.FirstOrDefault(t => t.TestSetTestCaseId == testSetTestCaseId);
                        if (existingTestSetCase != null)
                        {
                            int testSetCaseIndex = existingTestSetTestCases.IndexOf(existingTestSetCase);

                            //Begin transaction - needed to maintain integrity of positional data
                            using (TransactionScope transactionScope = new TransactionScope())
                            {
                                //First we need to actually delete the mapped test case and associated parameters
                                context.TestSet_DeleteTestCase(testSetTestCaseId);

                                //Next we need to move up any of the other test-cases that are below the passed-in test case
                                for (int i = testSetCaseIndex + 1; i < existingTestSetTestCases.Count; i++)
                                {
                                    //Move the row up one position
                                    existingTestSetTestCases[i].StartTracking();
                                    existingTestSetTestCases[i].Position--;

                                    //Make the database change the position data
                                    context.SaveChanges();
									
								}
								new HistoryManager().LogDeletion(projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, DateTime.UtcNow,DataModel.Artifact.ArtifactTypeEnum.TestCase, existingTestSetCase.TestCaseId);

								//Commit transaction - needed to maintain integrity of the position data
								transactionScope.Complete();
								
							}
                        }
                    }
                }

                //Finally we need to update the test set's last updated date
                UpdateLastUpdatedDate(testSetId);
                TestSet_RefreshExecutionData(projectId, testSetId);
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestSet_TestCasePositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestSet_TestCasePositionInUse, exception);
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        #endregion

        #region Lookup Retrieves

        /// <summary>
        /// Retrieves a dataset of test set statuses
        /// </summary>
        /// <returns>List of test set statuses</returns>
        public List<TestSetStatus> RetrieveStatuses()
        {
            const string METHOD_NAME = "RetrieveStatuses";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create select command for retrieving the lookup data
                if (_staticTestSetStatuses == null)
                {
                    using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                    {
                        //We order by ID since that's in the order of the lifecycle
                        var query = from t in context.TestSetStati
                                    where t.IsActive
                                    orderby t.TestSetStatusId
                                    select t;

                        _staticTestSetStatuses = query.ToList();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return _staticTestSetStatuses;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a test set status by its ID
        /// </summary>
        /// <param name="statusId">The id of the status</param>
        /// <returns>List of test set statuses</returns>
        public TestSetStatus RetrieveStatusById(int statusId)
        {
            const string METHOD_NAME = "RetrieveStatusById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create select command for retrieving the lookup data
                TestSetStatus status;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //We order by ID since that's in the order of the lifecycle
                    var query = from t in context.TestSetStati
                                where t.TestSetStatusId == statusId
                                select t;

                    status = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return status;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a dataset of test set recurrences
        /// </summary>
        /// <returns>List of test set recurrences</returns>
        public List<Recurrence> RetrieveRecurrences()
        {
            const string METHOD_NAME = "RetrieveRecurrences";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create select command for retrieving the lookup data
                if (_staticRecurrences == null)
                {
                    using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                    {
                        //We order by ID since that's in the order of the lifecycle
                        var query = from r in context.Recurrences
                                    where r.IsActive
                                    orderby r.RecurrenceId
                                    select r;

                        _staticRecurrences = query.ToList();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return _staticRecurrences;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a test set recurrence by its ID
        /// </summary>
        /// <param name="recurrenceId">The id of the recurrence</param>
        /// <returns>List of test set recurrences</returns>
        public Recurrence RetrieveRecurrenceById(int recurrenceId)
        {
            const string METHOD_NAME = "RetrieveRecurrenceById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create select command for retrieving the lookup data
                Recurrence Recurrence;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //We order by ID since that's in the order of the lifecycle
                    var query = from r in context.Recurrences
                                where r.RecurrenceId == recurrenceId
                                select r;

                    Recurrence = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return Recurrence;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Returns a sorted list of values to populate the lookup for the
        /// test set execution status filter
        /// </summary>
        /// <returns>Sorted List containing filter values</returns>
        public SortedList<int, string> RetrieveExecutionStatusFiltersLookup()
        {
            const string METHOD_NAME = CLASS_NAME + "RetrieveExecutionStatusFiltersLookup()";
            Logger.LogEnteringEvent(METHOD_NAME);

            Logger.LogExitingEvent(METHOD_NAME);
            return TestSetManager.executionStatusFiltersList;
        }

        #endregion

        #region Helper Functions

        /// <summary>Handles any test set specific filters that are not generic</summary>
        /// <param name="expressionList">The existing list of expressions</param>
        /// <param name="filter">The current filter</param>
        /// <param name="projectId">The current project</param>
        /// <param name="p">The LINQ parameter</param>
        /// <param name="utcOffset">The current offset from UTC</param>
        /// <returns>True if handled, return False for the standard filter handling</returns>
        protected internal bool HandleTestSetSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
        {
            //By default, let the generic filter convertor handle the filter
            string filterProperty = filter.Key;
            object filterValue = filter.Value;

            //Handle the special case of execution status, since it doesn't map to a single column
            if (filterProperty == "ExecutionStatusId")
            {
                if (filterValue is Int32)
                {
                    HandleExecutionStatusFilters(p, expressionList, (int)filterValue);
                }
                return true;
            }

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

            return false;
        }

        /// <summary>Handles any test set folder specific filters that are not generic</summary>
        /// <param name="expressionList">The existing list of expressions</param>
        /// <param name="filter">The current filter</param>
        /// <param name="projectId">The current project</param>
        /// <param name="p">The LINQ parameter</param>
        /// <param name="utcOffset">The current offset from UTC</param>
        /// <returns>True if handled, return False for the standard filter handling</returns>
        protected internal bool HandleTestFolderSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
        {
            //By default, let the generic filter convertor handle the filter
            string filterProperty = filter.Key;
            object filterValue = filter.Value;

            //Handle the special case of execution status, since it doesn't map to a single column
            if (filterProperty == "ExecutionStatusId")
            {
                if (filterValue is Int32)
                {
                    HandleExecutionStatusFilters(p, expressionList, (int)filterValue);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates an expression for the total count of all test cases status counts (CountTotal) for a test set
        /// </summary>
        /// <returns>LINQ Expression</returns>
        /// <remarks>
        /// Equivalent to COUNT_PASSED + COUNT_FAILED + COUNT_BLOCKED + COUNT_CAUTION + COUNT_NOT_RUN + COUNT_NOT_APPLICABLE
        /// </remarks>
        protected Expression CreateTotalCountExpression(ParameterExpression p)
        {
            MemberExpression countExp1 = LambdaExpression.PropertyOrField(p, "CountPassed");
            MemberExpression countExp2 = LambdaExpression.PropertyOrField(p, "CountFailed");
            MemberExpression countExp3 = LambdaExpression.PropertyOrField(p, "CountBlocked");
            MemberExpression countExp4 = LambdaExpression.PropertyOrField(p, "CountCaution");
            MemberExpression countExp5 = LambdaExpression.PropertyOrField(p, "CountNotRun");
            MemberExpression countExp6 = LambdaExpression.PropertyOrField(p, "CountNotApplicable");

            //Sum these up
            Expression countTotal = Expression.Add(countExp1, countExp2);
            countTotal = Expression.Add(countTotal, countExp3);
            countTotal = Expression.Add(countTotal, countExp4);
            countTotal = Expression.Add(countTotal, countExp5);
            countTotal = Expression.Add(countTotal, countExp6);

            //Return
            return countTotal;
        }

        /// <summary>
        /// Handles the execution status special range filters
        /// </summary>
        /// <param name="p"></param>
        /// <param name="expressionList"></param>
        /// <param name="filterValue"></param>
        protected void HandleExecutionStatusFilters(ParameterExpression p, List<Expression> expressionList, int filterValue)
        {
            switch (filterValue)
            {
                //Run Filters
                case 1:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_NOT_RUN = EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countNotRunExp = LambdaExpression.PropertyOrField(p, "CountNotRun");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.Equal(countNotRunExp, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 2:
                    {
                        //EXE.COUNT_TOTAL > 0 AND (EXE.COUNT_NOT_RUN * 2) > EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countNotRunExp = LambdaExpression.PropertyOrField(p, "CountNotRun");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2a = Expression.Multiply(countNotRunExp, LambdaExpression.Constant(2));
                        Expression expression2 = Expression.GreaterThan(expression2a, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 3:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_NOT_RUN > 0
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countNotRunExp = LambdaExpression.PropertyOrField(p, "CountNotRun");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.GreaterThan(countNotRunExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression2);
                        break;
                    }

                //Passed Filters
                case 4:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_PASSED > 0
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countPassedExp = LambdaExpression.PropertyOrField(p, "CountPassed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.GreaterThan(countPassedExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression2);
                        break;
                    }
                case 5:
                    {
                        //EXE.COUNT_TOTAL > 0 AND (EXE.COUNT_PASSED * 2) >= EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countPassedExp = LambdaExpression.PropertyOrField(p, "CountPassed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2a = Expression.Multiply(countPassedExp, LambdaExpression.Constant(2));
                        Expression expression2 = Expression.GreaterThanOrEqual(expression2a, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 6:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_PASSED = EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countPassedExp = LambdaExpression.PropertyOrField(p, "CountPassed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.Equal(countPassedExp, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }

                //Failed Filters
                case 7:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_FAILED > 0
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countFailedExp = LambdaExpression.PropertyOrField(p, "CountFailed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.GreaterThan(countFailedExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression2);
                        break;
                    }
                case 8:
                    {
                        //EXE.COUNT_TOTAL > 0 AND (EXE.COUNT_FAILED * 2) >= EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countFailedExp = LambdaExpression.PropertyOrField(p, "CountFailed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2a = Expression.Multiply(countFailedExp, LambdaExpression.Constant(2));
                        Expression expression2 = Expression.GreaterThanOrEqual(expression2a, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 9:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_FAILED = EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countFailedExp = LambdaExpression.PropertyOrField(p, "CountFailed");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.Equal(countFailedExp, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }

                //Caution Filters
                case 10:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_CAUTION > 0
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countCautionExp = LambdaExpression.PropertyOrField(p, "CountCaution");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.GreaterThan(countCautionExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression2);
                        break;
                    }
                case 11:
                    {
                        //EXE.COUNT_TOTAL > 0 AND (EXE.COUNT_CAUTION * 2) >= EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countCautionExp = LambdaExpression.PropertyOrField(p, "CountCaution");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2a = Expression.Multiply(countCautionExp, LambdaExpression.Constant(2));
                        Expression expression2 = Expression.GreaterThanOrEqual(expression2a, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 12:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_CAUTION = EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countCautionExp = LambdaExpression.PropertyOrField(p, "CountCaution");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.Equal(countCautionExp, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }

                //Blocked Filters
                case 13:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_BLOCKED > 0
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countBlockedExp = LambdaExpression.PropertyOrField(p, "CountBlocked");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.GreaterThan(countBlockedExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression2);
                        break;
                    }
                case 14:
                    {
                        //EXE.COUNT_TOTAL > 0 AND (EXE.COUNT_BLOCKED * 2) >= EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countBlockedExp = LambdaExpression.PropertyOrField(p, "CountBlocked");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2a = Expression.Multiply(countBlockedExp, LambdaExpression.Constant(2));
                        Expression expression2 = Expression.GreaterThanOrEqual(expression2a, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
                case 15:
                    {
                        //EXE.COUNT_TOTAL > 0 AND EXE.COUNT_BLOCKED = EXE.COUNT_TOTAL
                        Expression countTotalExp = CreateTotalCountExpression(p);
                        MemberExpression countBlockedExp = LambdaExpression.PropertyOrField(p, "CountBlocked");
                        Expression expression1 = Expression.GreaterThan(countTotalExp, LambdaExpression.Constant(0));
                        expressionList.Add(expression1);
                        Expression expression2 = Expression.Equal(countBlockedExp, countTotalExp);
                        expressionList.Add(expression2);
                        break;
                    }
            }
        }

        /// <summary>
        /// Updates the execution data of a test set folder and all its successive parents
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="folderId">The id of the folder</param>
        protected internal void TestSetFolder_RefreshExecutionData(int projectId, int folderId)
        {
            const string METHOD_NAME = "TestSetFolder_RefreshExecutionData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				TestSetFolder testSetFolder;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the specified folder
                    var query1 = from f in context.TestSetFolders
                                 where f.TestSetFolderId == folderId
                                 select f;

                    testSetFolder = query1.FirstOrDefault();
                    if (testSetFolder == null)
                    {
                        //Just exit
                        return;
                    }

                    //Now do the update using the stored procedure
                    context.TestSet_RefreshFolderExecutionStatus(projectId, folderId);
                }

                //Finally recursively call this function to make sure that successive parents are rolled-up
                if (testSetFolder.ParentTestSetFolderId.HasValue)
                {
                    this.TestSetFolder_RefreshExecutionData(projectId, testSetFolder.ParentTestSetFolderId.Value);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Copies across the test set parameter values from one test set to another
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="sourceTestSetId">The id of the source test set</param>
        /// <param name="destTestSetId">The id of the destination test set</param>
        protected void CopyTestSetParameterValues(int projectId, int sourceTestSetId, int destTestSetId, int userId)
        {
            List<TestSetParameter> testSetParameterValues = RetrieveParameterValues(sourceTestSetId);
            foreach (TestSetParameter parameterValue in testSetParameterValues)
            {
                //Add the value to the destination test set
                AddTestSetParameter(destTestSetId, parameterValue.TestCaseParameterId, parameterValue.Value, projectId, userId);
            }
        }

        /// <summary>Copies across the mapped test cases from one test set to another</summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="sourceTestSetId">The id of the source test set</param>
        /// <param name="destTestSetId">The id of the destination test set</param>
        protected void CopyMappedTestCases(int projectId, int sourceTestSetId, int destTestSetId)
        {
            List<TestSetTestCase> sourceTestSetTestCases = RetrieveTestCases2(sourceTestSetId);
            foreach (TestSetTestCase testSetTestCase in sourceTestSetTestCases)
            {
                //Get any parameter values
                Dictionary<string, string> parameterValues = new Dictionary<string, string>();
                List<TestSetTestCaseParameter> testSetTestCaseParameterValues = RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);
                foreach (TestSetTestCaseParameter parameterValue in testSetTestCaseParameterValues)
                {
                    if (!parameterValues.ContainsKey(parameterValue.Name))
                    {
                        parameterValues.Add(parameterValue.Name, parameterValue.Value);
                    }
                }

                //Add the test case to the copied test set
                AddTestCase(projectId, destTestSetId, testSetTestCase.TestCaseId, testSetTestCase.OwnerId, null, parameterValues);
            }
        }

        #endregion
    }

    /// <summary>
    /// Stores each parameter value being evaluated during the creation of a new test run from a test case / set
    /// </summary>
    /// <remarks>
    /// We use this class because different parameter values have precedence so we need to keep track of the type
    /// of value as well as the value itself. Test Set overides a Test Link and a Test link overises the default value
    /// </remarks>
    public class TestRunParameter
    {
        /// <summary>
        /// Constructor for class
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">What type of value has been set</param>
        public TestRunParameter(string name, string value, ParameterValueType type)
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
        }

        /// <summary>
        /// The different possible value types
        /// </summary>
        public enum ParameterValueType
        {
            Default = 1,
            TestLink = 2,
            TestSet = 3
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// The type of value that has been set
        /// </summary>
        public ParameterValueType Type
        {
            get;
            set;
        }
    }

    /// <summary>
    /// This exception is thrown when you try execute a test set with no test cases
    /// or pass in an empty list of test cases to execute
    /// </summary>
    public class TestRunNoTestCasesException : ApplicationException
    {
        public TestRunNoTestCasesException()
        {
        }
        public TestRunNoTestCasesException(string message)
            : base(message)
        {
        }
        public TestRunNoTestCasesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when you associate a test run with a non-existant test set or
    /// a test set FOLDER
    /// </summary>
    public class TestRunInvalidTestSetException : ApplicationException
    {
        public TestRunInvalidTestSetException()
        {
        }
        public TestRunInvalidTestSetException(string message)
            : base(message)
        {
        }
        public TestRunInvalidTestSetException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>This exception is thrown if you try and run an automated test set manually</summary>
    public class TestSetNotManualException : ApplicationException
    {
        /// <summary>The id of the test set</summary>
        public int TestSetId
        {
            get;
            private set;
        }

        public TestSetNotManualException(string message, int testSetId)
            : base(message)
        {
            this.TestSetId = testSetId;
        }
    }

    /// <summary>
    /// This exception is thrown if you try and perform an operation that would result in there being no remaining top-level test set folders
    /// </summary>
    public class ProjectDefaultTestSetFolderException : ApplicationException
    {
        public ProjectDefaultTestSetFolderException()
        {
        }
        public ProjectDefaultTestSetFolderException(string message)
            : base(message)
        {
        }
        public ProjectDefaultTestSetFolderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>This exception is thrown when you try and add a test case to a test set in a position that's already in use</summary>
    public class TestSetDuplicateTestCasePositionException : ApplicationException
    {
        public TestSetDuplicateTestCasePositionException()
        {
        }
        public TestSetDuplicateTestCasePositionException(string message)
            : base(message)
        {
        }
        public TestSetDuplicateTestCasePositionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
