using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Linq.Expressions;
using System.Data;
using System.Transactions;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class encapsulates all the data access functionality for
    /// reading and writing Test Configuration Sets (and releated entities) in the system
    /// </summary>
    public class TestConfigurationManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.TestConfigurationManager::";

        #region Test Configuration Sets

        /// <summary>Handles any automation host specific filters that are not generic</summary>
        /// <param name="expressionList">The existing list of expressions</param>
        /// <param name="filter">The current filter</param>
        /// <param name="projectId">The current project</param>
        /// <param name="p">The LINQ parameter</param>
        /// <param name="utcOffset">The current offset from UTC</param>
        /// <returns>True if handled, return False for the standard filter handling</returns>
        protected internal bool HandleTestConfigurationSetSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
        {
            //No custom filters currently supported
            return false;
        }

        /// <summary>Counts all the test configuration sets in the project</summary>
        /// <param name="filters">The collection of filters - pass null if none specified</param>
        /// <param name="projectId">The project we're interested in</param>
        /// <param name="includeDeleted">Whether or not to include deleted sets in the count.</param>
        /// <returns>The total number of sets</returns>
        /// <remarks>Used to help with pagination</remarks>
        public int CountSets(int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
        {
            const string METHOD_NAME = "CountSets";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int count = 0;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestConfigurationSets
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
                                select t;

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestConfigurationSet, bool>> filterClause = CreateFilterExpression<TestConfigurationSet>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestConfigurationSet, filters, utcOffset, null, HandleTestConfigurationSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestConfigurationSet>)query.Where(filterClause);
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

        /// <summary>Retrieves a list of all the active test configuration sets in the project (used for lookups)</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="includeDeleted">Whether or not to include deleted hosts.</param>
        /// <returns>The list of sets</returns>
        public List<TestConfigurationSet> RetrieveSets(int projectId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveSets()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestConfigurationSet> testConfigurationSets;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the record(s)
                    var query = from t in context.TestConfigurationSets
                                where t.ProjectId == projectId && (!t.IsDeleted || includeDeleted) && t.IsActive
                                orderby t.Name, t.TestConfigurationSetId
                                select t;

                    testConfigurationSets = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testConfigurationSets;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a filtered, sorted list of test configuration sets (inactive and active)</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="sortProperty">The property to sort by</param>
        /// <param name="sortAscending">Should we sort ascending or descending</param>
        /// <param name="startRow">The first row to return</param>
        /// <param name="numberOfRows">The number of rows to return</param>
        /// <param name="filters">The filters to apply</param>
        /// <param name="includeDeleted">Wether or not to include deleted sets.</param>
        /// <returns>List of test configuration sets</returns>
        public List<TestConfigurationSet> RetrieveSets(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveSets()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestConfigurationSet> testConfigurationSets;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Build the base query
                    var query = from t in context.TestConfigurationSets
                                where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
                                select t;

                    //Add the dynamic sort
                    if (String.IsNullOrEmpty(sortProperty))
                    {
                        //Default to sorting by name ascending
                        query = query.OrderByDescending(t => t.Name).ThenBy(t => t.TestConfigurationSetId);
                    }
                    else
                    {
                        //We always sort by the physical ID to guarantee stable sorting
                        string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                        query = query.OrderUsingSortExpression(sortExpression, "TestConfigurationSetId");
                    }

                    //Add the dynamic filters
                    if (filters != null)
                    {
                        //Get the template for this project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        //Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
                        Expression<Func<TestConfigurationSet, bool>> filterClause = CreateFilterExpression<TestConfigurationSet>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestConfigurationSet, filters, utcOffset, null, HandleTestConfigurationSetSpecificFilters);
                        if (filterClause != null)
                        {
                            query = (IOrderedQueryable<TestConfigurationSet>)query.Where(filterClause);
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
                        return new List<TestConfigurationSet>();
                    }

                    //Execute the query
                    testConfigurationSets = query
                        .Skip(startRow - 1)
                        .Take(numberOfRows)
                        .ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testConfigurationSets;
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
        public List<TestConfigurationSet> RetrieveDeletedSets(int projectId)
        {
            const string METHOD_NAME = "RetrieveDeletedSets()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestConfigurationSet> deletedSets;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the record(s)
                    var query = from t in context.TestConfigurationSets
                                where t.IsDeleted && t.ProjectId == projectId
                                orderby t.TestConfigurationSetId
                                select t;

                    //Actually execute the query and return the dataset
                    deletedSets = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return deletedSets;
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                //Do not rethrow.
                return new List<TestConfigurationSet>();
            }
        }

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>List of soft-deleted tasks</returns>
		public List<TestConfigurationSet> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestConfigurationSet> configs;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.TestConfigurationSets
								where t.ProjectId == projectId && t.IsDeleted
								orderby t.TestConfigurationSetId
								select t;

					configs = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return configs;
			}
			catch (Exception ex)
			{
				//Do not rethrow, just return an empty list
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				return new List<TestConfigurationSet>();
			}
		}


		/// <summary>Retrieves a single test configuration set by its ID</summary>
		/// <param name="testConfigurationSetId">The id of the set to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>The test configuration set entity or NULL if it doesn't exist</returns>
		public TestConfigurationSet RetrieveSetById(int testConfigurationSetId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "RetrieveSetById()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                TestConfigurationSet testConfigurationSet;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the record(s)
                    var query = from t in context.TestConfigurationSets
                                where
                                    (!t.IsDeleted || includeDeleted) &&
                                    t.TestConfigurationSetId == testConfigurationSetId
                                select t;

                    testConfigurationSet = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testConfigurationSet;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Updates a test configuration set entity, handling concurrent updates correctly</summary>
        /// <param name="testConfigurationSet">The test configuration set entity</param>
        /// <param name="userId">The id of the user making the change</param>
        public void UpdateSet(TestConfigurationSet testConfigurationSet, int userId, long? rollBackId = null)
        {
            const string METHOD_NAME = "UpdateSet()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //If we have a null entity just return
            if (testConfigurationSet == null)
            {
                return;
            }

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Start tracking changes
                    testConfigurationSet.StartTracking();

                    //Update the last-update and concurrency dates
                    testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
                    testConfigurationSet.ConcurrencyDate = DateTime.UtcNow;

                    //Now apply the changes
                    context.TestConfigurationSets.ApplyChanges(testConfigurationSet);

					//Save the changes
					context.SaveChanges(userId, true, false, rollBackId);
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

        /// <summary>Deletes an automation host in the current project</summary>
        /// <param name="testConfigurationSetId">The id of the host</param>
        public void DeleteHostFromDatabase(int testConfigurationSetId, int userId)
        {
            const string METHOD_NAME = "DeleteHostFromDatabase()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First make sure it exists
                    var query = from a in context.TestConfigurationSets
                                where a.TestConfigurationSetId == testConfigurationSetId
                                select a;

                    TestConfigurationSet testConfigurationSet = query.FirstOrDefault();
                    if (testConfigurationSet != null)
                    {
                        //Capture the project id
                        int projectId = testConfigurationSet.ProjectId;

                        //Actually perform the delete
                        context.TestConfiguration_DeleteConfigValues(testConfigurationSetId);

						//Log the purge.
						new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, testConfigurationSetId, DateTime.UtcNow, testConfigurationSet.Name);
					}
				}
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>Undeletes a test configuration set, making it available to users.</summary>
        /// <param name="testConfigurationSetId">automation host ID</param>
        /// <param name="userId">The userId performing the undelete.</param>
        /// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
        public void UnDeleteSet(int testConfigurationSetId, int userId, long? rollbackId = null, bool logHistory = true)
        {
            const string METHOD_NAME = "UnDeleteSet()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            int projectId = -1;
            using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
            {
                //We need to initially retrieve the host (needs to be marked as deleted)
                var query = from t in context.TestConfigurationSets
                            where t.TestConfigurationSetId == testConfigurationSetId && t.IsDeleted
                            select t;

                //Get the test configuration set
                TestConfigurationSet testConfigurationSet = query.FirstOrDefault();
                if (testConfigurationSet != null)
                {
                    projectId = testConfigurationSet.ProjectId;

                    //Mark as undeleted
                    testConfigurationSet.StartTracking();
                    testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
                    testConfigurationSet.ConcurrencyDate = DateTime.UtcNow;
                    testConfigurationSet.IsDeleted = false;

                    //Save changes, no history logged, that's done later
                    context.SaveChanges();
                }
            }

            //Log the undelete
            if (logHistory && projectId > 0)
            {
                //Okay, mark it as being undeleted.
                //new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, testConfigurationSetId, rollbackId, DateTime.UtcNow);
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>Marks a test configuration set as being deleted.</summary>
        /// <param name="testConfigurationSetId">The test configuration set ID.</param>
        /// <param name="userId">The user performing the delete.</param>
        public void MarkSetAsDeleted(int projectId, int testConfigurationSetId, int userId)
        {
            const string METHOD_NAME = "MarkSetAsDeleted()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //We need to initially retrieve the automation host to see that it exists
                bool deletePerformed = false;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestConfigurationSets
                                where t.TestConfigurationSetId == testConfigurationSetId && !t.IsDeleted
                                select t;

                    TestConfigurationSet testConfigurationSet = query.FirstOrDefault();
                    if (testConfigurationSet != null)
                    {
                        //Mark as deleted
                        testConfigurationSet.StartTracking();
                        testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
                        testConfigurationSet.IsDeleted = true;
                        context.SaveChanges();
                        deletePerformed = true;
                    }
                }

                if (deletePerformed)
                {
                    //Add a changeset to mark it as deleted.
                    new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, testConfigurationSetId, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                throw ex;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>Inserts a new test configuration set into the specified project</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="name">The name of the test configuration set</param>
        /// <param name="description">The description of the test configuration set (optional)</param>
        /// <param name="active">Whether the test configuration set is active or not</param>
        /// <param name="userId">The user creating the test configuration set.</param>
        /// <returns>The id of the new test configuration set</returns>
        public int InsertSet(int projectId, string name, string description, bool active, int userId)
        {
            const string METHOD_NAME = "InsertSet()";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                int testConfigurationSetId;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Populate the new entity
                    TestConfigurationSet testConfigurationSet = new TestConfigurationSet();
                    testConfigurationSet.ProjectId = projectId;
                    testConfigurationSet.Name = name;
                    testConfigurationSet.Description = description;
                    testConfigurationSet.CreationDate = DateTime.UtcNow;
                    testConfigurationSet.IsActive = active;
                    testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
                    testConfigurationSet.ConcurrencyDate = DateTime.UtcNow;
                    testConfigurationSet.IsDeleted = false;

                    //Persist the automation host and get the new id
                    context.TestConfigurationSets.AddObject(testConfigurationSet);
                    context.SaveChanges();
					//Log creation
					new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, testConfigurationSet.TestConfigurationSetId, DateTime.UtcNow);

					testConfigurationSetId = testConfigurationSet.TestConfigurationSetId;
                }

                //Add a history record..
                //new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet, testConfigurationSetId, DateTime.UtcNow);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testConfigurationSetId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Updates just the last updated date of the test configuration set</summary>
        /// <param name="testSetId">The test set in question</param>
        protected void UpdateLastUpdatedDate(int testConfigurationSetId)
        {
            const string METHOD_NAME = "UpdateLastUpdatedDate";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from t in context.TestConfigurationSets
                                where t.TestConfigurationSetId == testConfigurationSetId
                                select t;

                    TestConfigurationSet testConfigurationSet = query.FirstOrDefault();
                    if (testConfigurationSet != null)
                    {
                        //Update the last updated date
                        testConfigurationSet.StartTracking();
                        testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
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

        #endregion

        #region Test Configuration Entries

        /// <summary>
        /// Copies across the test configuration entries from a set in one project to an empty set
        /// </summary>
        /// <param name="existingTestConfigurationSetId">The id of the original set</param>
        /// <param name="newTestConfigurationSetId">The id of the new set</param>
        /// <param name="testCaseParameterMapping">The mapping of test case parameters between projects</param>
        /// <param name="sameTemplates">Do the two projects use the same template or not</param>
        /// <remarks>
        /// 1) The new test configuration set is empty, without any entries already
        /// 2) This works both for the same project template or different project templates
        /// </remarks>
        protected internal void CopyEntries(int existingTestConfigurationSetId, int newTestConfigurationSetId, bool sameTemplates, Dictionary<int,int> testCaseParameterMapping, Dictionary<int,int> customPropertyValueMapping)
        {
            const string METHOD_NAME = "CopyEntries()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First we need to copy across the parameters
                    var query = from t in context.TestConfigurationSetParameters
                                where t.TestConfigurationSetId == existingTestConfigurationSetId
                                select t;

                    List<TestConfigurationSetParameter> parameters = query.ToList();
                    foreach (TestConfigurationSetParameter parameter in parameters)
                    {
                        int? testCaseParameterId = null;
                        if (testCaseParameterMapping.ContainsKey(parameter.TestCaseParameterId))
                        {
                            testCaseParameterId = testCaseParameterMapping[parameter.TestCaseParameterId];
                        }
                        if (testCaseParameterId.HasValue)
                        {
                            //Add to the new set
                            TestConfigurationSetParameter newParameter = new TestConfigurationSetParameter();
                            newParameter.TestConfigurationSetId = newTestConfigurationSetId;
                            newParameter.TestCaseParameterId = testCaseParameterId.Value;
                            context.TestConfigurationSetParameters.AddObject(newParameter);
                        }
                    }

                    //Save the changes
                    context.SaveChanges();

					//Log Creation

                    //Next we need to loop through all of the entries and associated values
                    var query2 = from t in context.TestConfigurations.Include(t => t.ParameterValues)
                                where t.TestConfigurationSetId == existingTestConfigurationSetId
                                select t;

                    List<TestConfiguration> entries = query2.ToList();
                    foreach (TestConfiguration entry in entries)
                    {
                        //Add to the new set
                        TestConfiguration newEntry = new TestConfiguration();
                        newEntry.TestConfigurationSetId = newTestConfigurationSetId;
                        newEntry.Position = entry.Position;
                        context.TestConfigurations.AddObject(newEntry);

                        //Add the values
                        foreach (TestConfigurationParameterValue parameterValue in entry.ParameterValues)
                        {
                            //Get the IDs in the new project
                            int? testCaseParameterId = null;
                            if (testCaseParameterMapping.ContainsKey(parameterValue.TestCaseParameterId))
                            {
                                testCaseParameterId = testCaseParameterMapping[parameterValue.TestCaseParameterId];
                            }

                            //See if we have the same template or not
                            int? customPropertyValueId = null;
                            if (sameTemplates)
                            {
                                //Same template, therefore same ID
                                customPropertyValueId = parameterValue.CustomPropertyValueId;
                            }
                            else if (customPropertyValueMapping.ContainsKey(parameterValue.CustomPropertyValueId))
                            {
                                customPropertyValueId = customPropertyValueMapping[parameterValue.CustomPropertyValueId];
                            }

                            if (testCaseParameterId.HasValue && customPropertyValueId.HasValue)
                            {
                                //Add to the new entry
                                TestConfigurationParameterValue newParameterValue = new TestConfigurationParameterValue();
                                newParameterValue.TestConfigurationSetId = newTestConfigurationSetId;
                                newParameterValue.TestCaseParameterId = testCaseParameterId.Value;
                                newParameterValue.CustomPropertyValueId = customPropertyValueId.Value;
                                context.TestConfigurationParameterValues.AddObject(newParameterValue);
                                newEntry.ParameterValues.Add(newParameterValue);
                            }
                        }
                    }

                    //Save the changes
                    context.SaveChanges();
					//Log Creation
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>Retrieves a list of test configuration entries in a specific set</summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="testConfigurationSetId">The id of thee test configuration set</param>
        /// <returns>The list of configuration entries</returns>
        public List<TestConfigurationEntry> RetrieveEntries(int projectId, int testConfigurationSetId)
        {
            const string METHOD_NAME = "RetrieveEntries()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<TestConfigurationEntry> testConfigurationEntries = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First make sure that the test configuration set exists and is in the specified project
                    var query = from t in context.TestConfigurationSets
                                where t.ProjectId == projectId && t.TestConfigurationSetId == testConfigurationSetId
                                select t;

                    if (query.Count() > 0)
                    {
                        //Now get the entries
                        testConfigurationEntries = context.TestConfiguration_RetrieveConfigValues(testConfigurationSetId).ToList();
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return testConfigurationEntries;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Moves thee specified test configurations to a different position in the set
        /// </summary>
        /// <param name="testConfigurationSetId">The id of the test configuration set</param>
        /// <param name="sourceTestConfigurationId">The id of the item being moved</param>
        /// <param name="destTestConfigurationId">The id of the item it's being moved in front of (null = at the end of the list)</param>
        public void MoveTestConfigurations(int testConfigurationSetId, int sourceTestConfigurationId, int? destTestConfigurationId)
        {
            const string METHOD_NAME = "MoveTestConfigurations";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing mapped test configurations
                    var query = from t in context.TestConfigurations
                                where t.TestConfigurationSetId == testConfigurationSetId
                                orderby t.Position, t.TestConfigurationId
                                select t;

                    List<TestConfiguration> existingTestConfigurations = query.ToList();
                    if (existingTestConfigurations.Count < 1)
                    {
                        //End if there are no test cases to move!
                        return;
                    }

                    //Make sure that we have the row to be moved in this dataset
                    TestConfiguration sourceTestConfigurationRow = existingTestConfigurations.FirstOrDefault(t => t.TestConfigurationId == sourceTestConfigurationId);
                    if (sourceTestConfigurationRow == null)
                    {
                        throw new ArtifactNotExistsException(GlobalResources.Messages.TestConfigurationManager_TestConfigurationIdNotExist);
                    }
                    int sourcePosition = sourceTestConfigurationRow.Position;

                    //Now lets see if we have a destination row as well
                    //If not, the dest position is simply the last position in the list
                    int destPosition = existingTestConfigurations.Count;
                    TestConfiguration destTestConfiguration = null;
                    if (destTestConfigurationId.HasValue)
                    {
                        destTestConfiguration = existingTestConfigurations.FirstOrDefault(t => t.TestConfigurationId == destTestConfigurationId.Value);
                        if (destTestConfiguration != null)
                        {
                            destPosition = destTestConfiguration.Position;
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
                        int tempPosition = existingTestConfigurations[existingTestConfigurations.Count - 1].Position + 1;
                        sourceTestConfigurationRow.StartTracking();
                        sourceTestConfigurationRow.Position = tempPosition;
                        context.SaveChanges();

                        //If the destination is after the source in position, need to move items between the two up in position
                        //otherwise need to moveitems between the two down in position
                        bool sourceTestConfigurationMoved = false;
                        if (destPosition > sourcePosition)
                        {
                            //Need to decrement by one because we are removing a position (unless move to end)
                            if (destTestConfiguration != null)
                            {
                                destPosition--;
                            }
                            for (int i = 0; i < existingTestConfigurations.Count; i++)
                            {
                                if (existingTestConfigurations[i].Position > sourcePosition && existingTestConfigurations[i].Position <= destPosition)
                                {
                                    //Move the item one up in the list
                                    //If we are moving the source item, set the flag
                                    if (existingTestConfigurations[i] == sourceTestConfigurationRow)
                                    {
                                        sourceTestConfigurationMoved = true;
                                    }
                                    TestConfiguration itemToMove = existingTestConfigurations[i];
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
                            for (int i = existingTestConfigurations.Count - 1; i >= 0; i--)
                            {
                                if (existingTestConfigurations[i].Position >= destPosition && existingTestConfigurations[i].Position < sourcePosition)
                                {
                                    //Move the item one down in the list
                                    TestConfiguration itemToMove = existingTestConfigurations[i];
                                    itemToMove.StartTracking();
                                    itemToMove.Position++;

                                    //Make the database change
                                    context.SaveChanges();
                                }
                            }
                        }


                        //Finally if the source item was not already repositioned, need to set its position to the destination
                        if (!sourceTestConfigurationMoved)
                        {
                            sourceTestConfigurationRow.StartTracking();
                            sourceTestConfigurationRow.Position = destPosition;
                            //Make the database changes
                            context.SaveChanges();
                        }

                        //Commit transaction - needed to maintain integrity of position ordering
                        transactionScope.Complete();
                    }

                    //Finally we need to update the test set's last updated date
                    UpdateLastUpdatedDate(testConfigurationSetId);
                }
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestConfigurationManager_TestConfigurationPositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestConfigurationManager_TestConfigurationPositionInUse, exception);
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
        /// Removes the specified test configurations from the test configuration set
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testConfigurationSetId">The id of the test configuration set</param>
        /// <param name="testConfigurationIds">The ids of the test configurations to remove</param>
        public void RemoveTestConfigurationsFromSet(int projectId, int testConfigurationSetId, List<int> testConfigurationIds)
        {
            const string METHOD_NAME = "RemoveTestConfigurationsFromSet";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First lets retrieve a list of the existing test configurations
                    var query = from t in context.TestConfigurations
                                    .Include(t => t.ParameterValues)
                                where t.TestConfigurationSetId == testConfigurationSetId
                                orderby t.Position, t.TestConfigurationId
                                select t;

                    List<TestConfiguration> existingTestConfigurations = query.ToList();

                    //Loop through the ids
                    foreach (int testConfigurationId in testConfigurationIds)
                    {
                        //Make sure that we have the row to be deleted in this dataset, ignore otherwise
                        TestConfiguration existingTestConfiguration = existingTestConfigurations.FirstOrDefault(t => t.TestConfigurationId == testConfigurationId);
                        if (existingTestConfiguration != null)
                        {
                            int testConfigurationIndex = existingTestConfigurations.IndexOf(existingTestConfiguration);

                            //Begin transaction - needed to maintain integrity of positional data
                            using (TransactionScope transactionScope = new TransactionScope())
                            {
                                //First we need to actually delete the mapped test case and associated parameters
                                context.TestConfigurations.DeleteObject(existingTestConfiguration);

                                //Make the database change the position data
                                context.SaveChanges();

								//Log Deletion

                                //Next we need to move up any of the other test-cases that are below the passed-in test case
                                for (int i = testConfigurationIndex + 1; i < existingTestConfigurations.Count; i++)
                                {
                                    //Move the row up one position
                                    existingTestConfigurations[i].StartTracking();
                                    existingTestConfigurations[i].Position--;

                                    //Make the database change the position data
                                    context.SaveChanges();
                                }

                                //Commit transaction - needed to maintain integrity of the position data
                                transactionScope.Complete();
                            }
                        }
                    }
                }

                //Finally we need to update the test set's last updated date
                UpdateLastUpdatedDate(testConfigurationSetId);
            }
            catch (EntityConstraintViolationException exception)
            {
                //Log a warning if we try and use a Position that's already in use and rethrow as a business exception
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.TestConfigurationManager_TestConfigurationPositionInUse);
                Logger.Flush();
                throw new TestSetDuplicateTestCasePositionException(GlobalResources.Messages.TestConfigurationManager_TestConfigurationPositionInUse, exception);
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
        /// Populates the test configurations in a set, removing what was there before
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="testConfigurationSetId">The id of the test configuration set to populate</param>
        /// <param name="testParametersDic">The test case parameters to use for the population (key=parameterId, value=customListId)</param>
        public void PopulateTestConfigurations(int projectId, int testConfigurationSetId, Dictionary<int, int> testParametersDic)
        {
            const string METHOD_NAME = "PopulateTestConfigurations";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First make sure the test configuration set exists in the specified project (for security reasons)
                    var query = from t in context.TestConfigurationSets
                                where t.TestConfigurationSetId == testConfigurationSetId && t.ProjectId == projectId
                                select t;

                    TestConfigurationSet testConfigurationSet = query.FirstOrDefault();
                    if (testConfigurationSet == null)
                    {
                        throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.TestConfigurationManager_TestConfigurationSetIdNotExist, testConfigurationSetId));
                    }

                    //Call the stored procedure to do the delete
                    context.TestConfiguration_DeleteConfigValues(testConfigurationSetId);

                    //Next we need to populate the new entries and update the date
                    var query2 = from t in context.TestConfigurationSets
                                    .Include(t => t.Parameters)
                                    .Include(t => t.Configurations)
                                    .Include("Configurations.ParameterValues")
                                 where t.TestConfigurationSetId == testConfigurationSetId && t.ProjectId == projectId
                                 select t;

                    testConfigurationSet = query.FirstOrDefault();
					if (testConfigurationSet != null)
					{
						testConfigurationSet.StartTracking();
						testConfigurationSet.LastUpdatedDate = DateTime.UtcNow;
					}

                    //Populate the test configuration parameters
                    foreach (KeyValuePair<int,int> testParameter in testParametersDic)
                    {
                        //First populate the parameter itself
                        int testCaseParameterId = testParameter.Key;
                        int customListId = testParameter.Value;
                        TestConfigurationSetParameter testConfigurationSetParameter = new TestConfigurationSetParameter();
                        testConfigurationSetParameter.TestCaseParameterId = testCaseParameterId;
						if (testConfigurationSet != null)
						{
							testConfigurationSet.Parameters.Add(testConfigurationSetParameter);
						}
                    }

                    //Next we need to get a list of custom lists with values for use later
                    List<CustomPropertyList> retrievedLists = new List<CustomPropertyList>();
                    List<int> testCaseParameterIds = new List<int>();
                    foreach (KeyValuePair<int, int> testParameter in testParametersDic)
                    {
                        int testCaseParameterId = testParameter.Key;
                        testCaseParameterIds.Add(testCaseParameterId);
                        int customListId = testParameter.Value;
                        CustomPropertyList customPropertyList = customPropertyManager.CustomPropertyList_RetrieveById(customListId, true, false);
                        retrievedLists.Add(customPropertyList);
                    }

                    //Next populate the test configurations
                    //This requires a recursive function
                    int position = 1;
                    int testParameterIndex = 0;
                    Dictionary<int, int> customPropertyValueIds = new Dictionary<int, int>();
                    PopulateTestConfigurationsForParameter(context, testConfigurationSet, retrievedLists, testCaseParameterIds, testParameterIndex, customPropertyValueIds, ref position);

                    //Save all the changes
                    context.SaveChanges();
                }
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

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        protected void PopulateTestConfigurationsForParameter(SpiraTestEntitiesEx context, TestConfigurationSet testConfigurationSet, List<CustomPropertyList> retrievedLists, List<int> testCaseParameterIds, int testParameterIndex, Dictionary<int, int> customPropertyValueIds, ref int position)
        {
            //First loop through and add the values for this parameter
            if (retrievedLists.Count > testParameterIndex)
            {
                CustomPropertyList customPropertyList = retrievedLists[testParameterIndex];
                int testCaseParameterId = testCaseParameterIds[testParameterIndex];

                //Now we need to add each specific configuration entry
                if (customPropertyList != null && customPropertyList.Values != null)
                {
                    foreach (CustomPropertyValue cpv in customPropertyList.Values)
                    {
                        //Add this custom property to the list to the next level
                        Dictionary<int, int> newCustomPropertyValueIds = new Dictionary<int, int>(customPropertyValueIds);
                        newCustomPropertyValueIds.Add(cpv.CustomPropertyValueId, testCaseParameterId);

                        //Add the configuration entry and values on the innermost loop only
                        if (retrievedLists.Count == testParameterIndex + 1)
                        {
                            TestConfiguration testConfiguration = new TestConfiguration();
                            testConfiguration.Position = position;
							if (testConfigurationSet != null)
							{
								testConfigurationSet.Configurations.Add(testConfiguration);
							}
                            position++;

                            foreach (KeyValuePair<int, int> kvp in newCustomPropertyValueIds)
                            {
                                //Find the matching parameter
                                int customPropertyValueId = kvp.Key;
                                int testCaseParameterId2 = kvp.Value;
                                TestConfigurationSetParameter testConfigurationSetParameter = testConfigurationSet.Parameters.FirstOrDefault(t => t.TestCaseParameterId == testCaseParameterId2);

                                //Now we add the parameter value entries
                                TestConfigurationParameterValue testConfigurationParameterValue = new TestConfigurationParameterValue();
                                testConfigurationParameterValue.CustomPropertyValueId = customPropertyValueId;
                                testConfigurationSetParameter.ParameterValues.Add(testConfigurationParameterValue);
                                testConfiguration.ParameterValues.Add(testConfigurationParameterValue);
                            }
                        }

						if (testConfigurationSet != null)
						{
							//Now we need to go through the next parameter recursively
							PopulateTestConfigurationsForParameter(context, testConfigurationSet, retrievedLists, testCaseParameterIds, testParameterIndex + 1, newCustomPropertyValueIds, ref position);
						}
                    }
                }
            }
        }

        #endregion

        #region Other Functions

        /// <summary>
        /// Updates all test configuration sets to handle a project template change
        /// </summary>
        /// <param name="projectId">The project in question</param>
        /// <param name="oldTemplateId">The current template the project uses</param>
        /// <param name="newTemplateId">The new template the project uses</param>
        /// <remarks>
        /// If we cannot find a matching list or value in the new template, the entries are removed from the set
        /// </remarks>
        protected internal void RemapToNewTemplate(int projectId, int oldTemplateId, int newTemplateId)
        {
            const string METHOD_NAME = "RemapToNewTemplate()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //First get the custom lists for both templates
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                List<CustomPropertyList> oldCustomLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(oldTemplateId, true, false);
                List<CustomPropertyList> newCustomLists = customPropertyManager.CustomPropertyList_RetrieveForProjectTemplate(newTemplateId, true, false);

                //Now get the configuration sets and remap them
                List<int> entriesToRemove = new List<int>();
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the record(s)
                    var query = from t in context.TestConfigurationSets
                                            .Include(t => t.Parameters)
                                            .Include(t => t.Configurations)
                                            .Include("Configurations.ParameterValues")
                                where t.ProjectId == projectId
                                orderby t.TestConfigurationSetId
                                select t;

                    //Loop through all the sets
                    List<TestConfigurationSet> testConfigurationSets = query.ToList();
                    foreach (TestConfigurationSet set in testConfigurationSets)
                    {
                        //Loop through each entry in the set
                        foreach (TestConfiguration entry in set.Configurations)
                        {
                            //Loop through each parameter
                            foreach (TestConfigurationParameterValue paramValue in entry.ParameterValues)
                            {
                                int oldCustomPropertyValueId = paramValue.CustomPropertyValueId;
                                //See if we have a match
                                string oldListName = null;
                                CustomPropertyValue oldValue = null;
                                foreach (CustomPropertyList oldList in oldCustomLists)
                                {
                                    if (oldList.Values != null && oldList.Values.Count > 0)
                                    {
                                        oldValue = oldList.Values.FirstOrDefault(c => c.CustomPropertyValueId == oldCustomPropertyValueId);
                                        oldListName = oldList.Name;
                                        if (oldValue != null)
                                        {
                                            break;
                                        }
                                    }
                                }

                                //See if there is a match
                                CustomPropertyValue newValue = null;
                                if (!String.IsNullOrEmpty(oldListName)  && oldValue != null)
                                {
                                    CustomPropertyList newList = newCustomLists.FirstOrDefault(c => c.Name == oldListName);
                                    if (newList != null && newList.Values != null && newList.Values.Count > 0)
                                    {
                                        newValue = newList.Values.FirstOrDefault(c => c.Name == oldValue.Name);
                                    }
                                }

                                //If there is a match, update the entry, otherwise we need  to remove it
                                if (newValue == null)
                                {
                                    if (!entriesToRemove.Contains(entry.TestConfigurationId))
                                    {
                                        entriesToRemove.Add(entry.TestConfigurationId);
                                    }
                                }
                                else
                                {
                                    paramValue.StartTracking();
                                    paramValue.CustomPropertyValueId = newValue.CustomPropertyValueId;
                                }
                            }
                        }

                        //Save Changes
                        context.SaveChanges();

                        //Now remove any entries that we could not match
                        int testConfigurationSetId = set.TestConfigurationSetId;
                        foreach (int testConfigurationId in entriesToRemove)
                        {
                            RemoveTestConfigurationsFromSet(projectId, testConfigurationSetId, entriesToRemove);
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

        #endregion
    }
}
