using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// This class encapsulates all the data access functionality for creating
    /// and retrieving static links/associations between different artifacts
    /// </summary>
    public class ArtifactLinkManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ArtifactLinkManager::";

        /// <summary>
        ///	Retrieves a set of linked artifacts (where the provided one is the source)
        /// </summary>
        /// <param name="sourceArtifactType">The type of the artifact we want to look for links from</param>
        /// <param name="sourceArtifactId">The ID of the artifact we want to look for links from</param>
        /// <param name="destArtifactType">The type of the artifact we want to look for links to</param>
        /// <returns>An artifact link list</returns>
        /// <remarks>
        ///	This overload does not automatically find items where the artifact id is the destination, use the public overload for that.
        ///	Also this overload does not include any indirectly linked items (e.g. from test runs).
        ///	</remarks>
        protected internal List<ArtifactLinkView> RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, int sourceArtifactId, DataModel.Artifact.ArtifactTypeEnum destArtifactType)
        {
            const string METHOD_NAME = "RetrieveByArtifactId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<ArtifactLinkView> artifactLinks;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Call stored procedure for retrieving the artifact link record(s)
                    artifactLinks = context.ArtifactLink_RetrieveByArtifactWithDestType((int)sourceArtifactType, sourceArtifactId, (int)destArtifactType).ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the list
                return artifactLinks;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of link types
        /// </summary>
        /// <param name="includeInactive">Should we include inactive types</param>
        /// <returns>List of types</returns>
        public List<ArtifactLinkType> RetrieveLinkTypes(bool includeInactive = false)
		{
            const string METHOD_NAME = "RetrieveLinkTypes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<ArtifactLinkType> artifactLinkTypes;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the artifact link record
                    var query = from a in context.ArtifactLinkTypes
                                where a.IsActive || includeInactive
                                orderby a.Name, a.ArtifactLinkTypeId
                                select a;

                    artifactLinkTypes = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the entities
                return artifactLinkTypes;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        ///	Retrieves a single artifact link record by its link id
        /// </summary>
        /// <param name="artifactLinkId">The ID of the specific artifact link record</param>
        /// <returns>An artifact link entity</returns>
        /// <remarks>This is used for updating the comments only, so no need to get all the joined tables</remarks>
        public ArtifactLink RetrieveById(int artifactLinkId)
        {
            const string METHOD_NAME = "RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                ArtifactLink artifactLink;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Create select command for retrieving the artifact link record
                    var query = from a in context.ArtifactLinks
                                where a.ArtifactLinkId == artifactLinkId
                                select a;

                    artifactLink = query.FirstOrDefault();
                }

                //Make sure we have one record
                if (artifactLink == null)
                {
                    throw new ArtifactNotExistsException("Artifact Link " + artifactLinkId + " doesn't exist in the system.");
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the entity
                return artifactLink;
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
		///	Retrieves a single artifact link record by its link id
		/// </summary>
		/// <param name="artifactLinkTypeId">The ID of the specific artifact link type record</param>
		/// <returns>An artifact link entity</returns>
		/// <remarks>This is used for updating the comments only, so no need to get all the joined tables</remarks>
		public ArtifactLinkType RetrieveByLinkId(int artifactLinkTypeId)
		{
			const string METHOD_NAME = "RetrieveByLinkId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ArtifactLinkType artifactLinkType;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the artifact link record
					var query = from a in context.ArtifactLinkTypes
								where a.ArtifactLinkTypeId == artifactLinkTypeId
								select a;

					artifactLinkType = query.FirstOrDefault();
				}

				//Make sure we have one record
				if (artifactLinkType == null)
				{
					throw new ArtifactNotExistsException("Artifact Link Type " + artifactLinkTypeId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the entity
				return artifactLinkType;
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
		/// Updates the artifact link record
		/// </summary>
		/// <param name="attachmentDataSet">The dataset of artifact link records to be updated</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void Update(ArtifactLink artifactLink, int userId, int? projectId = null, long? rollBackId = null)
        {
            const string METHOD_NAME = "Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Update the creator
                    artifactLink.CreatorId = userId;

                    //Apply the changes and save
                    context.ArtifactLinks.ApplyChanges(artifactLink);
					//context.SaveChanges();
					context.SaveChanges(userId, true, false, rollBackId, projectId, artifactLink.ArtifactLinkId);
					//new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, DateTime.UtcNow);
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

        /// <summary>
        ///	Retrieves a set of linked artifacts (where the provided one is either the source or destination)
        /// </summary>
        /// <param name="artifactType">The type of the artifact we want to look for links to/from</param>
        /// <param name="artifactId">The ID of the artifact we want to look for links to/from</param>
        /// <param name="filters">The list of filters</param>
        /// <param name="sortProperty">The property name to be sorted on</param>
        /// <param name="sortAscending">Whether to sort the data ascending</param>
        /// <returns>An artifact link list</returns>
        /// <remarks>
        ///		This method currently only handles links between:
        ///			- Incidents and Requirements
        ///			- Incidents and Incidents
        ///			- Requirements and Requirements
        ///			- Test Steps > Incidents
        ///			- Test Runs > Incidents
        ///	</remarks>
        public List<ArtifactLinkView> RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, string sortProperty = "", bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
        {
            const string METHOD_NAME = "RetrieveByArtifactId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //For the case of Requirements linked to Incidents (and vice-versa) we need to create a special
                //query that adds the list of incidents linked indirectly to requirements via  test runs and
                //test case coverage relationships. To distinguish them from each other we use a negative number
                //for the artifact-link-id to denote that it's not a direct link that can be edited.
                List<ArtifactLinkView> artifactLinks;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Call stored procedure for retrieving the artifact link record(s)
                    artifactLinks = context.ArtifactLink_RetrieveByArtifact((int)artifactType, artifactId).ToList();
                }

                //We use in-memory sorting and filtering because so many different UNIONs are being used
                if (filters != null)
                {
                    Expression<Func<ArtifactLinkView, bool>> filterExpression = CreateFilterExpression<ArtifactLinkView>(null, null, Artifact.ArtifactTypeEnum.None , filters, utcOffset, null, null, false);
                    if (filterExpression != null)
                    {
                        artifactLinks = artifactLinks.Where(filterExpression.Compile()).ToList();
                    }
                }
                if (!String.IsNullOrEmpty(sortProperty))
                {
                    string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
                    artifactLinks = artifactLinks.AsQueryable().OrderUsingSortExpression(sortExpression, "ArtifactLinkId").ToList();
                }

                //Return the list
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return artifactLinks;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        ///	Retrieves a set of linked artifacts linked to an attachment for a particular project
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="attachmentId">The id of the attachment we're interested in</param>
        /// <returns>An artifact link dataset</returns>
        /// <remarks>
        /// The artifact link id has to be synthetically generated from a combination of the artifact id and artifact type id
        ///	</remarks>
        public List<ArtifactAttachmentView> RetrieveByAttachmentId(int projectId, int attachmentId)
        {
            const string METHOD_NAME = "RetrieveByAttachmentId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Create select command for retrieving the artifact link record(s)
                //We need to access the built-in view that links each of the queries for each artifact type
                List<ArtifactAttachmentView> artifactAttachments;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from a in context.ArtifactAttachmentsView
                                where a.AttachmentId == attachmentId && a.ProjectId == projectId
                                orderby a.ArtifactTypeName, a.ArtifactId, a.ArtifactLinkId
                                select a;

                    artifactAttachments = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the list
                return artifactAttachments;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        ///	Counts a set of linked artifacts linked to an attachment for a particular project
        /// </summary>
        /// <param name="projectId">The id of the current project</param>
        /// <param name="attachmentId">The id of the attachment we're interested in</param>
        /// <returns>The count</returns>
        public int CountByAttachmentId(int projectId, int attachmentId)
        {
            const string METHOD_NAME = "CountByAttachmentId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Count if we have any artifact-attachment associations
                int count;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from a in context.ArtifactAttachments
                                where a.AttachmentId == attachmentId && a.ProjectId == projectId
                                select a;

                    count = query.Count();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

                //Return the list
                return count;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Inserts a new artifact link into the system
        /// </summary>
        /// <param name="projectId">The project id the artifacts belong to</param>
        /// <param name="sourceArtifactType">The type of artifact being linked FROM</param>
        /// <param name="sourceArtifactId">The artifact id being linked FROM</param>
        /// <param name="destArtifactType">The type of artifact being linked TO</param>
        /// <param name="destArtifactId">The artifact id being linked TO</param>
        /// <param name="creatorId">The user creating the link</param>
        /// <param name="comment">Any user-supplied comments (optional)</param>
        /// <param name="creationDate">The date/time the association was added</param>
        /// <param name="artifactLinkType">The type of link being added</param>
        /// <returns>The ID of the newly created artifact link</returns>
        public int Insert(int projectId, DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, int sourceArtifactId, DataModel.Artifact.ArtifactTypeEnum destArtifactType, int destArtifactId, int creatorId, string comment, DateTime creationDate, ArtifactLink.ArtifactLinkTypeEnum artifactLinkType = ArtifactLink.ArtifactLinkTypeEnum.RelatedTo)
        {
            const string METHOD_NAME = "Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure we're not trying to link to ourself
            if (sourceArtifactType == destArtifactType && sourceArtifactId == destArtifactId)
            {
                throw new ArtifactLinkSelfReferentialException("You cannot link to yourself!");
            }

            //Get the list of projects we are allowed to link to
            List<ProjectArtifactSharing> sharingProjects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, destArtifactType);

            //Next make sure that we are only linking to ourself or an allowed project
            if (destArtifactType == DataModel.Artifact.ArtifactTypeEnum.Incident)
            {
                IncidentManager incidentManager = new IncidentManager();
                Incident incident;
                try
                {
                    incident = incidentManager.RetrieveById(destArtifactId, false);
                }
                catch (ArtifactNotExistsException)
                {
                    //Rethrow as a specific exception that the destination not found
                    throw new ArtifactLinkDestNotFoundException("This incident does not exist");
                }
                if (incident.ProjectId != projectId && !sharingProjects.Any(p => p.SourceProjectId == incident.ProjectId))
                {
                    throw new ArtifactLinkDestNotFoundException("This incident is not in the same project");
                }
            }
            //Next make sure that we are only linking to ourself or an allowed project
            if (destArtifactType == DataModel.Artifact.ArtifactTypeEnum.Task)
            {
                TaskManager taskManager = new TaskManager();
                Task task;
                try
                {
                    task = taskManager.RetrieveById(destArtifactId);
                }
                catch (ArtifactNotExistsException)
                {
                    //Rethrow as a specific exception that the destination not found
                    throw new ArtifactLinkDestNotFoundException("This task does not exist");
                }
                if (task.ProjectId != projectId && !sharingProjects.Any(p => p.SourceProjectId == task.ProjectId))
                {
                    throw new ArtifactLinkDestNotFoundException("This task is not in the same project");
                }
            }
            //Next make sure that we are only linking to ourself or an allowed project
            if (destArtifactType == DataModel.Artifact.ArtifactTypeEnum.Requirement)
            {
                RequirementManager requirementManager = new RequirementManager();
                RequirementView requirement;
                try
                {
                    requirement = requirementManager.RetrieveById2(null, destArtifactId);
                }
                catch (ArtifactNotExistsException)
                {
                    //Rethrow as a specific exception that the destination not found
                    throw new ArtifactLinkDestNotFoundException("This requirement does not exist");
                }
                if (requirement.ProjectId != projectId && !sharingProjects.Any(p => p.SourceProjectId == requirement.ProjectId))
                {
                    throw new ArtifactLinkDestNotFoundException("This requirement is not in the same project");
                }
            }

            try
            {
                int artifactLinkId;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Fill out entity with data for new artifact link
                    ArtifactLink artifactLink = new ArtifactLink();
                    artifactLink.SourceArtifactId = sourceArtifactId;
                    artifactLink.SourceArtifactTypeId = (int)sourceArtifactType;
                    artifactLink.DestArtifactId = destArtifactId;
                    artifactLink.DestArtifactTypeId = (int)destArtifactType;
                    artifactLink.CreatorId = creatorId;
                    artifactLink.CreationDate = creationDate;
                    artifactLink.Comment = (String.IsNullOrEmpty(comment) ? null : comment);
                    artifactLink.ArtifactLinkTypeId = (int)artifactLinkType;

                    //Actually perform the insert into the Artifact Link table and capture the Artifact Link ID
                    context.ArtifactLinks.AddObject(artifactLink);
                    context.SaveChanges();
                    artifactLinkId = artifactLink.ArtifactLinkId;

					new HistoryManager().LogCreation(projectId, creatorId, sourceArtifactType, sourceArtifactId, DateTime.UtcNow, destArtifactType, destArtifactId);
				}

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return artifactLinkId;
            }
            catch (EntityConstraintViolationException)
            {
                //If we have a unique constraint violation, throw a business exception
                throw new ArtifactLinkDuplicateException("That artifact is already linked.");
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Deletes all the associations that have an artifact as either the source or destination
        /// </summary>
        /// <param name="artifactType">The type of artifact</param>
        /// <param name="artifactId">The ID of the artifact</param>
        public void DeleteByArtifactId(DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId)
        {
            const string METHOD_NAME = "DeleteByArtifactId";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Delete all artifact links that have the artifact as the source and dest id
                    context.ArtifactLink_DeleteByArtifact((int)artifactType, artifactId);
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

        /// <summary>
        /// Deletes an artifact link in the system that has the specified ID
        /// </summary>
        /// <param name="artifactLinkId">The ID of the artifact link to be deleted</param>
        public void Delete(int artifactLinkId, ArtifactTypeEnum artifactType, int? projectId = null, int? userId = null)
        {
            const string METHOD_NAME = "Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First retrieve the specified link
                    var query = from a in context.ArtifactLinks
                                where a.ArtifactLinkId == artifactLinkId
                                select a;

                    ArtifactLink artifactLink = query.FirstOrDefault();

					var query1 = from a in context.ArtifactTypes
								where a.ArtifactTypeId == artifactLink.DestArtifactTypeId
								select a;

					ArtifactType artifactTypeName = query1.FirstOrDefault();

					if (artifactLink != null)
                    {
                        //Delete the object
                        context.ArtifactLinks.DeleteObject(artifactLink);
                        context.SaveChanges();
						new HistoryManager().LogDeletion((int)projectId, (int)userId, artifactType, artifactLink.SourceArtifactId, DateTime.UtcNow, (ArtifactTypeEnum)artifactTypeName.ArtifactTypeId, artifactLink.DestArtifactId);
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

		public void Delete(int artifactLinkId)
		{
			throw new NotImplementedException();
		}
	}

    /// <summary>
    /// This exception is thrown when you try and insert a link to
    /// and artifact that is already in place
    /// </summary>
    public class ArtifactLinkDuplicateException : ApplicationException
    {
        public ArtifactLinkDuplicateException()
        {
        }
        public ArtifactLinkDuplicateException(string message)
            : base(message)
        {
        }
        public ArtifactLinkDuplicateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when you try and insert a link to yourself
    /// </summary>
    public class ArtifactLinkSelfReferentialException : ApplicationException
    {
        public ArtifactLinkSelfReferentialException()
        {
        }
        public ArtifactLinkSelfReferentialException(string message)
            : base(message)
        {
        }
        public ArtifactLinkSelfReferentialException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when you try and insert a link to an artifact that can't be found
    /// </summary>
    public class ArtifactLinkDestNotFoundException : ApplicationException
    {
        public ArtifactLinkDestNotFoundException()
        {
        }
        public ArtifactLinkDestNotFoundException(string message)
            : base(message)
        {
        }
        public ArtifactLinkDestNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
