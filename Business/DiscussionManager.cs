using System;
using System.Data;
using System.Linq;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Discussions, etc. in the system
	/// </summary>
	public class DiscussionManager : ManagerBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.DiscussionManager::";

		#region Retrieve Records

		/// <summary>Retrieves all discussions for the given  artifact.</summary>
		/// <param name="artifactId">The artifact ID to retrieve for.</param>
		/// <param name="artifactType">The artifact type to retrieve for.</param>
		/// <param name="includeDeleted">Include deleted discussions or not. (Default = False)</param>
		/// <returns>A dataset of discussions.</returns>
        public IEnumerable<IDiscussion> Retrieve(int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Retrieve(int,ArtifactType,bool)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            IEnumerable<IDiscussion> discussions = this.retrieveRecords(artifactType, includeDeleted, artifactId: artifactId);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return discussions;
		}

		/// <summary>Retrieves all discussions for the given artifact type.</summary>
        /// <param name="artifactType">The artifact type to retrieve for.</param>
		/// <param name="includeDeleted">Include deleted discussions or not. (Default = False)</param>
		/// <returns>A dataset of discussions.</returns>
        public IEnumerable<IDiscussion> Retrieve(DataModel.Artifact.ArtifactTypeEnum artifactType, bool includeDeleted)
		{
			const string METHOD_NAME = "Retrieve(ArtifactType,bool)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            IEnumerable<IDiscussion> discussions = this.retrieveRecords(artifactType, includeDeleted: includeDeleted);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return discussions;
		}

		public DocumentDiscussion RetrieveDocumentDiscussionById(int documentDiscussionId)
		{
			const string METHOD_NAME = "RetrieveDocumentDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				DocumentDiscussion documentDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.DocumentDiscussions
								where p.DiscussionId == documentDiscussionId
								select p;

					documentDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return documentDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TaskDiscussion RetrieveTaskDiscussionById(int taskDiscussionId)
		{
			const string METHOD_NAME = "RetrieveTaskDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TaskDiscussion taskDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.TaskDiscussions
								where p.DiscussionId == taskDiscussionId
								select p;

					taskDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return taskDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public RiskDiscussion RetrieveRiskDiscussionById(int riskDiscussionId)
		{
			const string METHOD_NAME = "RetrieveRiskDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskDiscussion riskDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.RiskDiscussions
								where p.DiscussionId == riskDiscussionId
								select p;

					riskDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestCaseDiscussion RetrieveTestCaseDiscussionById(int testCaseDiscussionId)
		{
			const string METHOD_NAME = "RetrieveTestCaseDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseDiscussion testCaseDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.TestCaseDiscussions
								where p.DiscussionId == testCaseDiscussionId
								select p;

					testCaseDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestSetDiscussion RetrieveTestSetDiscussionById(int testSetDiscussionId)
		{
			const string METHOD_NAME = "RetrieveTestSetDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestSetDiscussion testSetDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.TestSetDiscussions
								where p.DiscussionId == testSetDiscussionId
								select p;

					testSetDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testSetDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public RequirementDiscussion RetrieveRequirementDiscussionById(int requirementDiscussionId)
		{
			const string METHOD_NAME = "RetrieveRequirementDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementDiscussion requirementDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.RequirementDiscussions
								where p.DiscussionId == requirementDiscussionId
								select p;

					requirementDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ReleaseDiscussion RetrieveReleaseDiscussionById(int releaseDiscussionId)
		{
			const string METHOD_NAME = "RetrieveReleaseDiscussionById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReleaseDiscussion releaseDiscussion;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.ReleaseDiscussions
								where p.DiscussionId == releaseDiscussionId
								select p;

					releaseDiscussion = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return releaseDiscussion;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single discussion based on the ID and artifact type.</summary>
		/// <param name="discussionId">The ID of the discussion to retrieve.</param>
		/// <param name="artifactType">The Discussion type to look for the ID in.</param>
		/// <returns>A dataset containing the discussion entry.</returns>
		public IDiscussion RetrieveById(int discussionId, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
            const string METHOD_NAME = "RetrieveById(int,artifactType)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            IDiscussion discussion = this.retrieveRecords(artifactType, discussionId: discussionId, includeDeleted: true).FirstOrDefault();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            return discussion;
		}

		/// <summary>Grabs the records asked for, returns the dataset to the requesting public function.</summary>
		/// <param name="discussionId">The single discussion ID to retrieve for.</param>
		/// <param name="artifactId">The artifact ID to retrieve discussions for.</param>
		/// <param name="artifactType">The Artifact Type to get discussions for.</param>
		/// <param name="includeDeleted">Include deleted discussions or not.</param>
		/// <returns>List containing the requested records.</returns>
        private IEnumerable<IDiscussion> retrieveRecords(DataModel.Artifact.ArtifactTypeEnum artifactType, bool includeDeleted = false, int? discussionId = null, int? artifactId = null)
		{
			const string METHOD_NAME = "retrieveRecords";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            IEnumerable<IDiscussion> discussions = null;

			try
			{
				//Make sure we have either a discussion id or an artifact id
				if (!discussionId.HasValue && !artifactId.HasValue)
				{
					throw new ArgumentException("You need to provide either a discussion id or an artifact id to this function");
				}

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See which artifact we have
                    switch (artifactType)
                    {
                        case Artifact.ArtifactTypeEnum.Requirement:
                            {
                                var query = from p in context.RequirementDiscussions.Include("Creator").Include("Creator.Profile")
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Release:
                            {
                                var query = from p in context.ReleaseDiscussions.Include("Creator").Include("Creator.Profile")
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Task:
                            {
                                var query = from p in context.TaskDiscussions.Include("Creator").Include("Creator.Profile")
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Document:
                            {
                                var query = from p in context.DocumentDiscussions
                                                .Include(p => p.Creator)
                                                .Include(p => p.Creator.Profile)
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Risk:
                            {
                                var query = from p in context.RiskDiscussions
                                                .Include(p => p.Creator)
                                                .Include(p => p.Creator.Profile)
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestCase:
                            {
                                var query = from p in context.TestCaseDiscussions.Include("Creator").Include("Creator.Profile")
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestSet:
                            {
                                var query = from p in context.TestSetDiscussions.Include("Creator").Include("Creator.Profile")
                                            select p;

                                if (discussionId.HasValue)
                                {
                                    query = query.Where(p => p.DiscussionId == discussionId.Value);
                                }
                                if (artifactId.HasValue)
                                {
                                    query = query.Where(p => p.ArtifactId == artifactId.Value);
                                }
                                if (!includeDeleted)
                                {
                                    query = query.Where(p => !p.IsDeleted);
                                }
                                query = query.OrderBy(p => p.CreationDate).ThenBy(p => p.DiscussionId);
                                discussions = query.ToList();
                            }
                            break;
                    }
                }
                
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return discussions;
            }
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		#endregion

		#region Delete Record

		/// <summary>Deletes a discussion from the system.</summary>
        /// <param name="discussionId">The id of the discussion to delete.</param>
        public void DeleteDiscussionId(int discussionId, DataModel.Artifact.ArtifactTypeEnum artifactType, bool deletePermanent = false, int? userId = null, int? projectId = null)
		{
            const string METHOD_NAME = "DeleteDiscussionId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See which artifact we have
                    switch (artifactType)
                    {
                        case Artifact.ArtifactTypeEnum.Requirement:
                            {
                                var query = from d in context.RequirementDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                RequirementDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									// context.RequirementDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.RequirementDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Release:
                            {
                                var query = from d in context.ReleaseDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                ReleaseDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									//context.ReleaseDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.ReleaseDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Task:
                            {
                                var query = from d in context.TaskDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                TaskDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									// context.TaskDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TaskDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Document:
                            {
                                var query = from d in context.DocumentDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                DocumentDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
									//do not allow deleting permanent comments if we are not also explicitly allowing for their deletion
                                    if (discussion.IsPermanent && !deletePermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									// context.DocumentDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.DocumentDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Risk:
                            {
                                var query = from d in context.RiskDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                RiskDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									//context.RiskDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.RiskDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestCase:
                            {
                                var query = from d in context.TestCaseDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                TestCaseDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									// context.TestCaseDiscussions.DeleteObject(discussion);
									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TestCaseDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestSet:
                            {
                                var query = from d in context.TestSetDiscussions
                                            where d.DiscussionId == discussionId
                                            select d;

                                TestSetDiscussion discussion = query.FirstOrDefault();
                                if (discussion != null)
                                {
                                    if (discussion.IsPermanent)
                                    {
                                        throw new DiscussionCannotBeDeletedIfPermanentException(GlobalResources.Messages.DiscussionManager_CannotDeletePermanentDiscussion);
                                    }
									//context.TestSetDiscussions.DeleteObject(discussion);

									discussion.IsDeleted = true;
									context.SaveChanges(userId, false, false, null);

									new HistoryManager().LogDeletion((int)projectId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.TestSetDiscussion, discussionId, DateTime.UtcNow);
								}
                            }
                            break;

                    }

					//Save changes
					//context.SaveChanges(userId, true, true, null);
					
					context.SaveChanges();
				}
            }
            catch (DiscussionCannotBeDeletedIfPermanentException)
            {
                //Throw without logging
                throw;
            }
            catch (System.Exception ex)
            {
                //Log then throw
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                Logger.Flush();
                throw ex;
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}
		#endregion

		#region Insert Records

		/// <summary>Inserts a new discussion entry into the associated table.</summary>
		/// <param name="userId">The userId whom created the discussion.</param>
		/// <param name="artifactId">The artifactId that the discussion belongs to.</param>
		/// <param name="artifactType">The type of the artifact.</param>
		/// <param name="text">The text of the message.</param>
		/// <param name="projectId">The current projectId of the item.</param>
        /// <param name="sendNotification">Should we send a notification</param>
        /// <param name="isPermanent">Is this a permanent (non-deleteable) comment</param>
		/// <returns>The ID of the new discussion.</returns>
        public int Insert(int userId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, string text, int projectId, bool isPermanent, bool sendNotification)
		{
			const string METHOD_NAME = "Insert()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            int retNumber = this.Insert(userId, artifactId, artifactType, text, DateTime.UtcNow, projectId, isPermanent, sendNotification);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return retNumber;
		}

		/// <summary>Inserts a new discussion entry into the associated table.</summary>
		/// <param name="userId">The userId whom created the discussion.</param>
		/// <param name="artifactId">The artifactId that the discussion belongs to.</param>
		/// <param name="artifactType">The type of the artifact.</param>
		/// <param name="text">The text of the message.</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="sendNotification">Should we send a notification (false if caller will be doing it)</param>
        /// <param name="isPermanent">Is this a permanent (non-deleteable) comment</param>
        /// <param name="dateSubmitted">The date the message was submitted.</param>
		/// <returns>The ID of the new discussion.</returns>
        public int Insert(int userId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, string text, DateTime dateSubmitted, int projectId, bool isPermanent, bool sendNotification)
		{
			const string METHOD_NAME = "Insert()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                IDiscussion discussionInterface = null;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See what type of artifact we have
                    switch (artifactType)
                    {
                        case Artifact.ArtifactTypeEnum.Incident:
                            {
                                //Incidents have their own functions, but we can use the DiscussionManager as a facade for those
                                //changes as well. They don't support permanent comments though
                                new IncidentManager().InsertResolution(artifactId, text, dateSubmitted, userId, sendNotification);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Requirement:
                            {
                                RequirementDiscussion discussion = new RequirementDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.RequirementDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RequirementDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        case Artifact.ArtifactTypeEnum.Release:
                            {
                                ReleaseDiscussion discussion = new ReleaseDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.ReleaseDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.ReleaseDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        case Artifact.ArtifactTypeEnum.Task:
                            {
                                TaskDiscussion discussion = new TaskDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.TaskDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TaskDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        case Artifact.ArtifactTypeEnum.TestCase:
                            {
                                TestCaseDiscussion discussion = new TestCaseDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.TestCaseDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCaseDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        case Artifact.ArtifactTypeEnum.TestSet:
                            {
                                TestSetDiscussion discussion = new TestSetDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.TestSetDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSetDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        case Artifact.ArtifactTypeEnum.Document:
                            {
                                DocumentDiscussion discussion = new DocumentDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.DocumentDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.DocumentDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
							break;

                        case Artifact.ArtifactTypeEnum.Risk:
                            {
                                RiskDiscussion discussion = new RiskDiscussion();
                                discussionInterface = discussion;
                                discussion.CreationDate = dateSubmitted;
                                discussion.ArtifactId = artifactId;
                                discussion.CreatorId = userId;
                                discussion.IsDeleted = false;
                                discussion.IsPermanent = isPermanent;
                                discussion.Text = text;
                                context.RiskDiscussions.AddObject(discussion);

								context.SaveChanges();
								new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RiskDiscussion, discussion.DiscussionId, DateTime.UtcNow);
							}
                            break;

                        default:
                            {
                                throw new Exception("Can not add a discussion for an artifact type of " + artifactType.ToString() + ".");
                            }
                    }

					//Save changes
					//context.SaveChanges(userId, true, true, null, projectId);
					
				}

                //Send to Notification to see if we need to send anything out.
                if (sendNotification)
                {
                    try
                    {
                        DataModel.Artifact notificationArtifact = null;
                        switch (artifactType)
                        {
                            case DataModel.Artifact.ArtifactTypeEnum.Requirement:
                                {
                                    notificationArtifact = new RequirementManager().RetrieveById2(null, artifactId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactTypeEnum.Task:
                                {
                                    notificationArtifact = new TaskManager().TaskView_RetrieveById(artifactId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                                {
                                    notificationArtifact = new TestCaseManager().RetrieveById(null, artifactId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                                {
                                    notificationArtifact = new TestSetManager().RetrieveById(null, artifactId);
                                }
                                break;

                            case DataModel.Artifact.ArtifactTypeEnum.Document:
								notificationArtifact = new AttachmentManager().RetrieveForProjectById2(projectId,artifactId);
								break;

							case DataModel.Artifact.ArtifactTypeEnum.Release:
								{
									notificationArtifact = new ReleaseManager().RetrieveById2(null, artifactId);
								}
								break;
							case DataModel.Artifact.ArtifactTypeEnum.Risk:
								{
									notificationArtifact = new RiskManager().Risk_RetrieveById2(artifactId);
								}
								break;
						}

                        if (notificationArtifact != null)
                        {
                            new NotificationManager().SendNotificationForArtifact(notificationArtifact, null, text);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for Incident.");
                    }
                }

				//Return the ID.
                int discussionId = -1;
                if (discussionInterface != null)
                {
                    discussionId = discussionInterface.DiscussionId;
                }
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return discussionId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}
		#endregion

		#region Save Data

		public void UpdateRequirementDiscussion(RequirementDiscussion requirementDiscussion, int userId, long? rollbackId = null, bool updHistory = true)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (requirementDiscussion == null)
			{
				return;
			}

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					requirementDiscussion.StartTracking();

					//Now apply the changes
					context.RequirementDiscussions.ApplyChanges(requirementDiscussion);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, rollbackId);
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void RequirementDiscussionUnDelete( int projectId, int reqId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.RequirementDiscussions
								where r.DiscussionId == reqId && r.IsDeleted
								select r;

					RequirementDiscussion requirementDiscussion = query.FirstOrDefault();
					if (requirementDiscussion != null)
					{
						requirementDiscussion.StartTracking();
						requirementDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RequirementDiscussion, reqId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void DocumentDiscussionUnDelete(int projectId, int docId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.DocumentDiscussions
								where r.DiscussionId == docId && r.IsDeleted
								select r;

					DocumentDiscussion documentDiscussion = query.FirstOrDefault();
					if (documentDiscussion != null)
					{
						documentDiscussion.StartTracking();
						documentDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.DocumentDiscussion, docId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void RiskDiscussionUnDelete(int projectId, int riskId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.RiskDiscussions
								where r.DiscussionId == riskId && r.IsDeleted
								select r;

					RiskDiscussion riskDiscussion = query.FirstOrDefault();
					if (riskDiscussion != null)
					{
						riskDiscussion.StartTracking();
						riskDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RiskDiscussion, riskId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void ReleaseDiscussionUnDelete(int projectId, int releaseId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.ReleaseDiscussions
								where r.DiscussionId == releaseId && r.IsDeleted
								select r;

					ReleaseDiscussion releaseDiscussion = query.FirstOrDefault();
					if (releaseDiscussion != null)
					{
						releaseDiscussion.StartTracking();
						releaseDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.ReleaseDiscussion, releaseId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void TestCaseDiscussionUnDelete(int projectId, int testCaseId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.TestCaseDiscussions
								where r.DiscussionId == testCaseId && r.IsDeleted
								select r;

					TestCaseDiscussion testCaseDiscussion = query.FirstOrDefault();
					if (testCaseDiscussion != null)
					{
						testCaseDiscussion.StartTracking();
						testCaseDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestCaseDiscussion, testCaseId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void TestSetDiscussionUnDelete(int projectId, int testSetId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.TestSetDiscussions
								where r.DiscussionId == testSetId && r.IsDeleted
								select r;

					TestSetDiscussion testSetDiscussion = query.FirstOrDefault();
					if (testSetDiscussion != null)
					{
						testSetDiscussion.StartTracking();
						testSetDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TestSetDiscussion, testSetId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public void TaskDiscussionUnDelete(int projectId, int taskId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.TaskDiscussions
								where r.DiscussionId == taskId && r.IsDeleted
								select r;

					TaskDiscussion taskDiscussion = query.FirstOrDefault();
					if (taskDiscussion != null)
					{
						taskDiscussion.StartTracking();
						taskDiscussion.IsDeleted = false;
						context.SaveChanges();
					}

					if (logHistory)
						new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.TaskDiscussion, taskId, rollbackId, DateTime.UtcNow);
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		///<summary>Updates a discussion in the database.</summary>
		/// <param name="discussion">The discussion to be saved.</param>
		public void Save(IDiscussion discussion, DataModel.Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "Save";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //See what type of artifact we have
                    switch (artifactType)
                    {
                        case Artifact.ArtifactTypeEnum.Requirement:
                            {
                                RequirementDiscussion discussionObj = (RequirementDiscussion)discussion;
                                context.RequirementDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Release:
                            {
                                ReleaseDiscussion discussionObj = (ReleaseDiscussion)discussion;
                                context.ReleaseDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Task:
                            {
                                TaskDiscussion discussionObj = (TaskDiscussion)discussion;
                                context.TaskDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Document:
                            {
                                DocumentDiscussion discussionObj = (DocumentDiscussion)discussion;
                                context.DocumentDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.Risk:
                            {
                                RiskDiscussion discussionObj = (RiskDiscussion)discussion;
                                context.RiskDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestCase:
                            {
                                TestCaseDiscussion discussionObj = (TestCaseDiscussion)discussion;
                                context.TestCaseDiscussions.ApplyChanges(discussionObj);
                            }
                            break;

                        case Artifact.ArtifactTypeEnum.TestSet:
                            {
                                TestSetDiscussion discussionObj = (TestSetDiscussion)discussion;
                                context.TestSetDiscussions.ApplyChanges(discussionObj);
                            }
                            break;
                    }

                    //Save the changes
                    context.SaveChanges();
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

		#endregion
	}    

    /// <summary>
    /// Exception thrown if you try and delete a 'permanent' non-deletable discussion
    /// </summary>
    public class DiscussionCannotBeDeletedIfPermanentException : ApplicationException
    {
        public DiscussionCannotBeDeletedIfPermanentException()
        {
        }
        public DiscussionCannotBeDeletedIfPermanentException(string message)
            : base(message)
        {
        }
        public DiscussionCannotBeDeletedIfPermanentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
