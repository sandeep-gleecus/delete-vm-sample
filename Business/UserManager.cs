using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for reading and writing Users in the system</summary>
	public class UserManager : ManagerBase
	{
		private const string CLASS = "Business.UserManager::";

		public const int UserInternal = 0; //Always has no navigation data, used for viewing all expanded
		public const int UserSystemAdministrator = 1; // The built in system administrator

		#region Recent Artifacts

		/// <summary>
		/// Retrieves a list of recently accessed artifacts for a user
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="filterProjectId">filter by a specific project (null = all projects)</param>
		/// <param name="rowsToDisplay">The number of rows to return</param>
		/// <returns>The list of artifacts</returns>
		public List<UserRecentArtifact> RetrieveRecentArtifactsForUser(int userId, int? filterProjectId, int rowsToDisplay)
		{
			const string METHOD_NAME = "RetrieveRecentArtifactsForUser";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<UserRecentArtifact> recentArtifacts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the list of artifacts for a user
					var query = from a in context.UserRecentArtifacts
									.Include(a => a.Project)
								where a.UserId == userId
								select a;

					//Filter by project if necessary
					if (filterProjectId.HasValue)
					{
						query = query.Where(a => a.ProjectId == filterProjectId.Value);
					}

					//Sort by date (descending)
					query = query.OrderByDescending(a => a.LastAccessDate);

					//Add the row limit
					query = query.Take(rowsToDisplay);

					//Return the list
					recentArtifacts = query.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return recentArtifacts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds/updates an artifact to the list of recently accessed ones
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactTypeId">The type of the artifact</param>
		public void AddUpdateRecentArtifact(int userId, int projectId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "AddUpdateRecentArtifact";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we already have this artifact tracked
					var query = from p in context.UserRecentArtifacts
								where
									p.UserId == userId &&
									p.ProjectId == projectId &&
									p.ArtifactTypeId == artifactTypeId &&
									p.ArtifactId == artifactId
								select p;

					UserRecentArtifact userRecentArtifact = query.FirstOrDefault();
					if (userRecentArtifact == null)
					{
						//Add this entry
						userRecentArtifact = new UserRecentArtifact();
						userRecentArtifact.UserId = userId;
						userRecentArtifact.ProjectId = projectId;
						userRecentArtifact.ArtifactId = artifactId;
						userRecentArtifact.ArtifactTypeId = artifactTypeId;
						userRecentArtifact.LastAccessDate = DateTime.UtcNow;
						context.UserRecentArtifacts.AddObject(userRecentArtifact);
					}
					else
					{
						//Update the date
						userRecentArtifact.StartTracking();
						userRecentArtifact.LastAccessDate = DateTime.UtcNow;
					}

					//Save changes
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Structs

		// Container struct for use in aggregating columns for queries
		private struct ProfileColumnData
		{
			public string PropertyName;
			public SettingsPropertyValue PropertyValue;
			public object Value;
			public Type DataType;

			public ProfileColumnData(string propertyName, SettingsPropertyValue pv, object val, Type type)
			{
				EnsureValidEntityName(propertyName);
				PropertyName = propertyName;
				PropertyValue = pv;
				Value = val;
				DataType = type;
			}
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Updates an existing user setting value
		/// </summary>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to update an entry in</param>
		/// <param name="entryKey">The key name of the entry we're updating</param>
		/// <param name="entryValue">The new setting value to be stored</param>
		/// <param name="typeCode">The underlying type of the value we're storing</param>
		internal void UpdateSetting(int userId, string collectionName, string entryKey, string entryValue, int typeCode)
		{
			const string METHOD_NAME = "UpdateSetting";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the specified entry for this collection 
					var query = from uce in context.UserCollectionEntries
								where uce.Collection.Name == collectionName && uce.UserId == userId && uce.EntryKey == entryKey
								select uce;

					//Get the entry
					UserCollectionEntry userCollectionEntry = query.FirstOrDefault();

					//Make sure we have a setting, the code calling this function should have already checked for the insert case
					if (userCollectionEntry != null)
					{
						userCollectionEntry.StartTracking();
						userCollectionEntry.EntryValue = entryValue;
						userCollectionEntry.EntryTypeCode = typeCode;
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
		}

		/// <summary>
		/// Inserts a new user setting value
		/// </summary>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to add an entry to</param>
		/// <param name="entryKey">The key name of the entry we're inserting</param>
		/// <param name="entryValue">The new setting value to be stored</param>
		/// <param name="typeCode">The underlying type of the value we're storing</param>
		internal void InsertSetting(int userId, string collectionName, string entryKey, string entryValue, int typeCode)
		{
			const string METHOD_NAME = "InsertSetting";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the specified collection
					var query = from uc in context.UserCollections
								where uc.Name == collectionName
								select uc;

					//Get the entry
					UserCollection userCollection = query.FirstOrDefault();

					//Make sure the collection exists
					if (userCollection == null)
					{
						throw new UserSettingNotExistsException(string.Format(GlobalResources.Messages.UserManager_SettingNotExists, collectionName));
					}

					//Add the new entry
					userCollection.StartTracking();

					UserCollectionEntry userCollectionEntry = new UserCollectionEntry();
					userCollectionEntry.UserId = userId;
					userCollectionEntry.EntryKey = entryKey;
					userCollectionEntry.EntryValue = entryValue;
					userCollectionEntry.EntryTypeCode = typeCode;
					userCollection.Entries.Add(userCollectionEntry);
					context.SaveChanges();
				}				
			}
			catch (EntityConstraintViolationException)
			{
				//Ignore and attempt an update instead
				UpdateSetting(userId, collectionName, entryKey, entryValue, typeCode);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
		}

		/// <summary>
		/// Deletes an existing user setting value
		/// </summary>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to delete an entry in</param>
		/// <param name="entryKey">The key name of the entry we're updating</param>
		internal void DeleteSetting(int userId, string collectionName, string entryKey)
		{
			const string METHOD_NAME = "DeleteSetting";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the specified collection and entries
					var query = from uc in context.UserCollections.Include("Entries")
								where uc.Name == collectionName
								select uc;

					//Get the entry
					UserCollection userCollection = query.FirstOrDefault();

					//Make sure the collection exists
					if (userCollection == null)
					{
						throw new UserSettingNotExistsException(string.Format(GlobalResources.Messages.UserManager_SettingNotExists, collectionName));
					}

					//Delete the entry (make sure it hasn't already been deleted)
					userCollection.StartTracking();
					UserCollectionEntry userCollectionEntry = userCollection.Entries.FirstOrDefault(uce => uce.EntryKey == entryKey && uce.UserId == userId);
					if (userCollectionEntry != null)
					{
						userCollectionEntry.MarkAsDeleted();
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
		}

		/// <summary>
		/// Retrieves a collection of user settings for a particular user and collection name
		/// </summary>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to return settings for</param>
		/// <returns>The dataset of settings</returns>
		internal UserCollection RetrieveSettings(int userId, string collectionName)
		{
			const string METHOD_NAME = "RetrieveSettings";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			UserCollection userCollection;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the the collection
					var query1 = from uc in context.UserCollections
								 where uc.Name == collectionName
								 select uc;

					//Next get the entries for this collection (joined by fix-up)
					var query2 = from uce in context.UserCollectionEntries
								 where uce.Collection.Name == collectionName && uce.UserId == userId
								 orderby uce.EntryKey
								 select uce;

					userCollection = query1.FirstOrDefault();
					query2.ToList();    //Joined implicitly by 'fix-up'
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
			return userCollection;
		}

        /// <summary>
        /// Retrieves a collection of user settings for a particular user and collection name, sorted by its value
        /// </summary>
        /// <param name="userId">The user we're interested in</param>
        ///	<param name="collectionName">The name of the collection we want to return settings for</param>
        /// <returns>The dataset of settings</returns>
        /// <remarks>
        /// This version is only used by the user recent artifacts widget, since it needs to get a list of items sorted by most recent,
        /// and the setting value is actually the date
        /// </remarks>
        public UserCollection RetrieveSettingsSorted(int userId, string collectionName, bool sortAscending = true)
        {
            const string METHOD_NAME = "RetrieveSettingsSorted";

            Logger.LogEnteringEvent(CLASS + METHOD_NAME);

            UserCollection userCollection;

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //First get the the collection
                    var query1 = from uc in context.UserCollections
                                 where uc.Name == collectionName
                                 select uc;

                    //Next get the entries for this collection (joined by fix-up)
                    var query2 = from uce in context.UserCollectionEntries
                                 where uce.Collection.Name == collectionName && uce.UserId == userId
                                 select uce;

                    if (sortAscending)
                    {
                        query2 = query2.OrderBy(uce => uce.EntryValue);
                    }
                    else
                    {
                        query2 = query2.OrderByDescending(uce => uce.EntryValue);
                    }

                    userCollection = query1.FirstOrDefault();
                    query2.ToList();    //Joined implicitly by 'fix-up'
                }
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS + METHOD_NAME);
            return userCollection;
        }

        #endregion

        #region User Contacts Methods

        /// <summary>
        /// Retrieves a list of users (including profile) that are contacts of the current user
        /// </summary>
        /// <param name="userId">The current user</param>
        /// <returns>List of users who are listed as a contact of this user</returns>
        public List<User> UserContact_Retrieve(int userId)
		{
			const string METHOD_NAME = "UserContact_Retrieve";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<User> users;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.UserContactsOf.Any(uc => uc.UserId == userId)
								orderby u.Profile.FirstName, u.Profile.LastName, u.UserId
								select u;

					users = query.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return users;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds a new contact to the current user's list
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="contactUserId">The user to add as a contact</param>
		public void UserContact_Add(int userId, int contactUserId)
		{
			const string METHOD_NAME = "UserContact_Add";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Make sure they're not adding themselves as a contact
				if (userId == contactUserId)
				{
					throw new InvalidOperationException("You cannot add yourself as a contact!");
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First make sure we don't have this contact already
					var query = from u in context.Users.Include("UserContacts")
								where u.UserId == userId
								select u;

					User user = query.FirstOrDefault();
					if (user != null)
					{
						if (!user.UserContacts.Any(u => u.UserId == contactUserId))
						{
							//Need to check if the contact is already in context before attaching
							//Happens usually if they try and attach themselves!!
							user.StartTracking();
							User newContact = new User();
							newContact.UserId = contactUserId;
							context.Users.Attach(newContact);
							user.UserContacts.Add(newContact);
							context.SaveChanges();
						}
					}
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Removes an existing contact to the current user's list
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="contactUserId">The user to remove as a contact</param>
		public void UserContact_Remove(int userId, int contactUserId)
		{
			const string METHOD_NAME = "UserContact_Add";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First make sure we have this contact already
					var query = from u in context.Users.Include("UserContacts")
								where u.UserId == userId
								select u;

					User user = query.FirstOrDefault();
					if (user != null)
					{
						User contact = user.UserContacts.FirstOrDefault(u => u.UserId == contactUserId);
						if (contact != null)
						{
							user.StartTracking();
							user.UserContacts.Remove(contact);
							context.SaveChanges();
						}
					}
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Is the specified user a contact of the current user
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="contactUserId">The user to add as a contact</param>
		/// <returns>True if the user is a contact</returns>
		public bool UserContact_IsContact(int userId, int contactUserId)
		{
			const string METHOD_NAME = "UserContact_Add";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				bool isFound = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if the specified user is a contact of the primary user
					var query = from u in context.Users
								where u.UserContactsOf.Any(uc => uc.UserId == userId) && u.UserId == contactUserId
								select u;

					isFound = (query.FirstOrDefault() != null);
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return isFound;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}


		#endregion

		#region Public Methods

		/// <summary>
		/// Determines whether a user is authorized to access a certain project
		/// </summary>
		/// <param name="userId">The user who we want to check authorization for</param>
		/// <param name="projectId">The project that we want to check authorization for</param>
		/// <returns>True if authorized</returns>
		/// <remarks>Also considers group membership</remarks>
		public bool Authorize(int userId, int projectId)
		{
			const string METHOD_NAME = "Authorize";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			bool authorizedUser = false;
			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Query the project user view that has project and group membership combined
					var query = from pg in context.ProjectUsersView
								where pg.ProjectId == projectId && pg.UserId == userId
								select pg;

					authorizedUser = (query.Count() > 0);

					//If the user is not authorized also check to see if they are a system admin, in which case allow it
					if (!authorizedUser)
					{
						User user = GetUserById(userId);
						if (user.Profile.IsAdmin)
						{
							authorizedUser = true;
						}
					}

				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);

				//Return whether we have successfully authorized or not
				return authorizedUser;
			}
			catch (ArtifactNotExistsException)
			{
				//Invalid user id so just return false
				return false;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns the list of users who should be notified about a change in incident status
		/// </summary>
		/// <param name="workflowId">The workflow we're using</param>
		/// <param name="inputIncidentStatusId">The status of the incident before the change</param>
		/// <param name="outputIncidentStatusId">The status of the incident after the change</param>
		/// <returns>The list of users to be notified</returns>
		public List<User> RetrieveNotifyListForWorkflowRole(int projectId, int workflowId, int inputIncidentStatusId, int outputIncidentStatusId)
		{
			const string METHOD_NAME = "RetrieveNotifyListForWorkflowRole";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the specified workflow to make sure it exists
				Workflow workflow = new WorkflowManager().Workflow_RetrieveById(workflowId);

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get a list of project roles that need to be notified
					var roleQuery = from wtr in context.WorkflowTransitionRoles
									where wtr.Transition.InputIncidentStatusId == inputIncidentStatusId &&
										  wtr.Transition.OutputIncidentStatusId == outputIncidentStatusId &&
										  wtr.Role.IsActive && wtr.Transition.WorkflowId == workflowId &&
										  wtr.WorkflowTransitionRoleTypeId == (int)WorkflowTransitionRoleType.WorkflowTransitionRoleTypeEnum.Notify
									orderby wtr.ProjectRoleId
									select wtr;
					List<WorkflowTransitionRole> workflowTransitionRoles = roleQuery.ToList();
					List<int> projectRoleIds = new List<int>();
					foreach (WorkflowTransitionRole workflowTransitionRole in workflowTransitionRoles)
					{
						projectRoleIds.Add(workflowTransitionRole.ProjectRoleId);
					}

					//Now get the list of users in these roles
					var userQuery = from u in context.Users.Include("Profile")
									where u.IsActive && u.ProjectMembership.Any(pr => projectRoleIds.Contains(pr.ProjectRoleId) && pr.ProjectId == projectId)
									orderby u.UserId
									select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return userQuery.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of users who have a certain role on a project
		/// </summary>
		/// <param name="projectId">The ID of the project</param>
		/// <param name="projectRoleId">The ID of the project role</param>
		/// <returns>A user list</returns>
		/// <remarks>Only returns active users</remarks>
		public List<User> RetrieveByProjectRoleId(int projectId, int projectRoleId)
		{
			const string METHOD_NAME = "RetrieveByProjectRoleId";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.ProjectMembership.Any(p => p.ProjectId == projectId && p.ProjectRoleId == projectRoleId) && u.IsActive
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of template admins
		/// </summary>
		/// <param name="projectTemplateId"></param>
		/// <returns>The list of project admins</returns>
		public List<User> RetrieveProjectTemplateAdmins(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveProjectTemplateAdmins";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.ProjectMembership.Any(p => p.Project.ProjectTemplateId == projectTemplateId && p.Role.IsTemplateAdmin) && u.IsActive
								orderby u.Profile.FirstName, u.Profile.LastName, u.UserId
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Retrieves a list of project owners by project id
		/// </summary>
		/// <param name="projectId">The ID of the project to retrieve owners for</param>
		/// <returns>A user list</returns>
		public List<User> RetrieveOwnersByProjectId(int projectId)
		{
			const string METHOD_NAME = "RetrieveOwnersByProjectId";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.ProjectMembership.Any(p => p.ProjectId == projectId && p.Role.IsAdmin) && u.IsActive
								orderby u.Profile.FirstName, u.Profile.LastName, u.UserId
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of all users in the project (active and inactive) and includes their active flag
		/// so that special dropdown control can filter out those that are not active from being displayed in the actual
		/// list.
		/// </summary>
		/// <param name="projectId">The project the users have to be a member of</param>
		/// <returns>List of users</returns>
		public List<User> RetrieveForProject(int projectId)
		{
			const string METHOD_NAME = "RetrieveForProject";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.ProjectMembership.Any(p => p.ProjectId == projectId)
								orderby u.Profile.FirstName, u.Profile.LastName, u.UserId
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of active users in the system for a given project.
		/// This is used in page lookups.
		/// </summary>
		/// <param name="projectId">The project the users have to be a member of</param>
		/// <returns>List of users</returns>
		public List<User> RetrieveActiveByProjectId(int projectId)
		{
			const string METHOD_NAME = "RetrieveActiveByProjectId";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.ProjectMembership.Any(p => p.ProjectId == projectId) && u.IsActive
								orderby u.Profile.FirstName, u.Profile.LastName, u.UserId
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of active users in the system (for all projects)
		/// This is used in page lookups.
		/// </summary>
		/// <returns>List of users</returns>
		public List<User> RetrieveActive()
		{
			const string METHOD_NAME = "RetrieveActive";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.IsActive
								select u;

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return query.ToList();
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single user record by Rss Token (GUID)
		/// </summary>
		/// <param name="rssToken">The Rss Token of the user to retrieve</param>
		/// <returns>A user entity</returns>
		/// <param name="updateLastActivity">Should we update the last activity date</param>
		/// <remarks>Currently only returns active users that are approved and not locked-out</remarks>
		public User RetrieveByRssToken(string rssToken, bool updateLastActivity = false)
		{
			const string METHOD_NAME = "RetrieveByRssToken";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<User> userContext = context.Users.Include("Profile");

					var query = from u in userContext
								where u.RssToken == rssToken && u.IsActive && !u.IsLocked && u.IsApproved
								select u;
					user = query.FirstOrDefault();

					//Now update the last activity date
					if (updateLastActivity)
					{
						user.StartTracking();
						user.LastActivityDate = DateTime.UtcNow;
						context.SaveChanges();
					}
				}

				//Make sure data was returned
				if (user == null)
				{
					throw new ArtifactNotExistsException("User with RSS token '" + rssToken + "' doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return user;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a user by its id</summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>The user object</returns>
		/// <remarks>Includes inactive users</remarks>
		public User GetUserById(int userId, bool updateLastActivity = false)
		{
			const string METHOD_NAME = "GetUserById";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<User> userContext = context.Users.Include("Profile");

					var query = from u in userContext
								where u.UserId == userId
								select u;
					user = query.FirstOrDefault();

					//Now update the last activity date
					if (updateLastActivity)
					{
						user.StartTracking();
						user.LastActivityDate = DateTime.UtcNow;
						context.SaveChanges();
					}
				}

				//Make sure data was returned
				if (user == null)
				{
					throw new ArtifactNotExistsException("User " + userId + " does not exist in the system");
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return user;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Log as a trace only because we already have a Failure Audit logged by the Membership Provider
				Logger.LogTraceEvent(CLASS + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of users who's ids are in the provided list
		/// </summary>
		/// <param name="userIds">The id of the users</param>
		/// <returns>The user list</returns>
		/// <remarks>Returns all users (active and inactive)</remarks>
		public List<User> GetUsersByIds(List<int> userIds)
		{
			const string METHOD_NAME = "GetUserByIds";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<User> users;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<User> userContext = context.Users.Include("Profile");

					var query = from u in userContext
								where userIds.Contains(u.UserId)
								select u;
					users = query.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return users;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Sees how many users are currently online
		/// </summary>
		/// <param name="minutesSinceLastInActive">The number of mins since last inactive</param>
		/// <returns>The number of users</returns>
		/// <remarks>Not currently used by SpiraTest</remarks>
		public int GetNumberOfUsersOnline(int minutesSinceLastInActive)
		{
			const string METHOD_NAME = "GetNumberOfUsersOnline";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Determine the active date we want to compare against
				DateTime dateActive = DateTime.UtcNow.AddMinutes(-minutesSinceLastInActive);
				int userCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users
								where u.LastActivityDate > dateActive
								select u;
					userCount = query.Count();
				}

				return userCount;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Delete the user
		/// </summary>
		/// <param name="username">The user's username name</param>
		/// <param name="physicallyDelete">Do we want to permanently delete them</param>
		/// <remarks>If we don't permanently delete what we're actually doing is setting Inactive</remarks>
		/// <returns>True if deleted, false if user does not exist</returns>
		public bool DeleteUser(string username, bool permanentlyDelete)
		{
			const string METHOD_NAME = "DeleteUser";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
                //Make sure the user exists
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get all the referenced collections so that it deletes OK
                    var query = from u in context.Users
                                    .Include(u => u.Profile)
                                    .Include(u => u.UserContacts)
                                    .Include(u => u.UserContactsOf)
                                    .Include(u => u.DataSyncMappings)
                                    .Include(u => u.SettingsEntries)
                                where u.UserName.ToLower() == username.ToLower()
                                select u;

                    User user = query.FirstOrDefault();

                    //Make sure data was returned
                    if (user == null)
                    {
                        return false;
                    }

                    user.StartTracking();
                    if (permanentlyDelete)
                    {
                        //The delete of child objects (e.g. Profile) is handled by the DB cascades
                        context.DeleteObject(user);
                    }
                    else
                    {
                        user.IsActive = false;
                    }
                    context.SaveChanges();
                }
				return true;
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Delete the user</summary>
		/// <param name="userId">Tghe userId of the user to delete.</param>
		/// <param name="physicallyDelete">Do we want to permanently delete them</param>
		/// <remarks>If we don't permanently delete what we're actually doing is setting Inactive</remarks>
		/// <returns>True if deleted, false if user does not exist</returns>
		public bool DeleteUser(int userId, bool permanentlyDelete)
		{
			const string METHOD_NAME = CLASS + "DeleteUser";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool retValue = false;
			try
			{
				//Make sure the user exists
				User user = this.GetUserById(userId);

                //Delete the user
				retValue = this.DeleteUser(user.UserName, permanentlyDelete);
			}
			catch (ArtifactNotExistsException)
			{
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				throw;
			}

			return retValue;
		}

		/// <summary>
		/// Delete the user's profile (but not the user itself)
		/// </summary>
		/// <param name="username">The user's username name</param>
		/// <returns>True if deleted</returns>
		public bool DeleteProfile(string username)
		{
			const string METHOD_NAME = "DeleteProfile";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Make sure the user exists
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.UserName.ToLower() == username.ToLower()
								select u;
					user = query.FirstOrDefault();

					//Make sure data was returned
					if (user != null && user.Profile != null)
					{
						//Delete the profile
						user.StartTracking();
						user.Profile.MarkAsDeleted();
						user.LastActivityDate = DateTime.UtcNow;
						context.SaveChanges();

						Logger.LogExitingEvent(CLASS + METHOD_NAME);
						return true;
					}
					else
					{
						Logger.LogExitingEvent(CLASS + METHOD_NAME);
						return false;
					}
				}

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of inactive profiles
		/// </summary>
		/// <param name="userInactiveSinceDate">The user inactive sinc date</param>
		/// <returns>The number of profiles deleted</returns>
		public List<User> GetInactiveProfiles(DateTime userInactiveSinceDate)
		{
			int totalRecords;
			return GetInactiveProfiles(userInactiveSinceDate, 1, int.MaxValue, out totalRecords);
		}

		/// <summary>
		/// Gets the list of inactive profiles
		/// </summary>
		/// <param name="userInactiveSinceDate">The user inactive sinc date (UTC)</param>
		/// <returns>The number of profiles deleted</returns>
		/// <param name="pageIndex">The first records to return</param>
		/// <param name="pageSize">The page size</param>
		/// <param name="totalRecords">The total # records</param>
		public List<User> GetInactiveProfiles(DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			const string METHOD_NAME = "GetInactiveProfiles";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the list of matching users
				List<User> inactiveUsers;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where (u.LastActivityDate == null || u.LastActivityDate < userInactiveSinceDate) && u.Profile != null
								orderby u.UserName, u.UserId
								select u;
					totalRecords = query.Count();
					inactiveUsers = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return inactiveUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all profiles
		/// </summary>
		/// <returns>The number of profiles</returns>
		/// <param name="pageIndex">The first records to return</param>
		/// <param name="pageSize">The page size</param>
		/// <param name="totalRecords">The total # records</param>
		public List<User> GetProfiles(int pageIndex, int pageSize, out int totalRecords)
		{
			const string METHOD_NAME = "GetProfiles";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the list of matching users
				List<User> inactiveUsers;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.Profile != null
								orderby u.UserName, u.UserId
								select u;
					totalRecords = query.Count();
					inactiveUsers = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return inactiveUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of inactive profiles that partially match the username
		/// </summary>
		/// <param name="userInactiveSinceDate">The user inactive sync date (UTC)</param>
		/// <returns>The number of profiles deleted</returns>
		/// <param name="pageIndex">The first records to return</param>
		/// <param name="pageSize">The page size</param>
		/// <param name="totalRecords">The total # records</param>
		/// <param name="usernameToMatch">The username to match on</param>
		public List<User> FindInactiveProfilesByLogin(string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			const string METHOD_NAME = "FindInactiveProfilesByLogin";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the list of matching users
				List<User> inactiveUsers;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where (u.LastActivityDate == null || u.LastActivityDate < userInactiveSinceDate) && u.Profile != null
									&& u.UserName.Contains(usernameToMatch)
								orderby u.UserName, u.UserId
								select u;
					totalRecords = query.Count();
					inactiveUsers = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return inactiveUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all profiles that partially match the username
		/// </summary>
		/// <param name="userInactiveSinceDate">The user inactive sinc date</param>
		/// <returns>The number of profiles deleted</returns>
		/// <param name="pageIndex">The first records to return</param>
		/// <param name="pageSize">The page size</param>
		/// <param name="totalRecords">The total # records</param>
		/// <param name="usernameToMatch">The username to match on</param>
		public List<User> FindProfilesByLogin(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			const string METHOD_NAME = "FindProfilesByLogin";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the list of matching users
				List<User> inactiveUsers;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.Profile != null && u.UserName.Contains(usernameToMatch)
								orderby u.UserName, u.UserId
								select u;
					totalRecords = query.Count();
					inactiveUsers = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return inactiveUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes inactive profiles
		/// </summary>
		/// <param name="userInactiveSinceDate">The user inactive sinc date</param>
		/// <returns>The number of profiles deleted</returns>
		public int DeleteInactiveProfiles(DateTime userInactiveSinceDate)
		{
			const string METHOD_NAME = "DeleteInactiveProfiles";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the list of matching users
				List<User> inactiveUsers = GetInactiveProfiles(userInactiveSinceDate);
				//Make sure data was returned
				if (inactiveUsers != null)
				{
					foreach (User user in inactiveUsers)
					{
						//Delete the profile
						DeleteProfile(user.UserName);
					}

					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return inactiveUsers.Count;
				}
				else
				{
					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return 0;
				}

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates a new user in the system using the provided information</summary>
		/// <param name="username">The user's username</param>
		/// <param name="password">The user's password (enrypted)</param>
		/// <param name="unencryptedPassword">The user's unencrypted password (used for emails)</param>
		/// <param name="passwordSalt">The user's password SALT</param>
		/// <param name="email">Email address</param>
		/// <param name="passwordQuestion">Password question</param>
		/// <param name="passwordAnswer">Password answer</param>
		/// <param name="isApproved">Approved?</param>
		/// <param name="requiresUniqueEmail">Do we require unique emails per user</param>
		/// <param name="passwordFormat">The password format</param>
		/// <param name="ldapDn">The ldap dn</param>
		/// <param name="rssToken">The rss token</param>
		/// <param name="errorCode">Any error codes</param>
		/// <returns>The new user entity</returns>       		
		public User CreateUser(string username, string password, string passwordSalt, string email, string passwordQuestion, string passwordAnswer, bool isApproved, bool requiresUniqueEmail, int passwordFormat, string ldapDn, string rssToken, string unencryptedPassword, out int errorCode,  Guid? oAuthID = null, int? adminSectionId = null, string action = null, int? updatedUserId = null, int? userId = null, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS + "CreateUser()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int returnId = -1;

			errorCode = 0;
			try
			{
				//UserAuditManager, in case.
				UserAuditManager userAuditManager = new UserAuditManager();
				//First make sure that the username is not already in use
				User user;
				try
				{
					user = GetUserByLogin(username);
					if (user.UserName.ToLower() == username.ToLower())
					{
						errorCode = 6;
						return null;
					}
				}
				catch (ArtifactNotExistsException)
				{
					//This is expected, there should not be a user with this username
				}

				//See if we require a unique email address
				if (requiresUniqueEmail)
				{
					//Make sure the email address does not already exist
					try
					{
						user = GetUserByEmailAddress(email);
						if (user.EmailAddress.ToLower() == email.ToLower())
						{
							errorCode = 7;
							return null;
						}
					}
					catch (ArtifactNotExistsException)
					{
						//This is expected, there should not be a user with this username
					}
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new user entity
					Logger.LogTraceEvent(METHOD_NAME, "Creating new User Entity");

					user = new User
					{
						UserName = username,
						EmailAddress = email,
						IsActive = true,
						IsApproved = isApproved,
						IsLocked = false,
						Password = password,
						PasswordSalt = passwordSalt,
						PasswordQuestion = passwordQuestion,
						PasswordAnswer = passwordAnswer,
						PasswordFormat = passwordFormat,
						CreationDate = DateTime.UtcNow,
						LastActivityDate = DateTime.UtcNow,
						FailedPasswordAnswerAttemptCount = 0,
						FailedPasswordAttemptCount = 0,
						LdapDn = ldapDn,
						RssToken = rssToken
					};

					//Persist the new user
					context.Users.AddObject(user);
					context.SaveChanges();

					//Log history.
					if (logHistory)
						userAuditManager.LogCreation(Convert.ToInt32(user.UserId), Convert.ToInt32(adminSectionId), Convert.ToInt32(updatedUserId), user.UserName, action, DateTime.UtcNow);

					//Get the ID.
					returnId = user.UserId;
				}

				//Send an email notitication letting the user know that their account has been created.
				//Need to tailor the message depending on whether:
				//(a) it is approved
				//(b) it is LDAP or not

				//Create the Email object.
				NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();

				//Get the URL and product name
				string productName = Common.ConfigurationSettings.Default.License_ProductType;
				string webServerUrl = Common.ConfigurationSettings.Default.General_WebServerUrl;

				//Create the mail message
				string subject = "New " + productName + " Account";
				string messageBody = "";
				if (string.IsNullOrWhiteSpace(ldapDn) && !oAuthID.HasValue)
				{
					//If the unencrypted password is null, this is a demo account so do not send
					if (!string.IsNullOrWhiteSpace(unencryptedPassword))
					{
						if (isApproved)
						{
							//If the account is approved, then it was created by the administrator
							//and we need to include the plain text password
							messageBody = string.Format(GlobalResources.Messages.UserManager_NewUserEmail_Approved,
								productName,
								webServerUrl,
								username,
								unencryptedPassword);
						}
						else
						{
							//If the account is not approved, then it was created by the user themselves and as a result
							//we will not send it out in plain text.
							messageBody = string.Format(GlobalResources.Messages.UserManager_NewUserEmail_Unapproved,
								productName,
								webServerUrl,
								username,
								unencryptedPassword);
						}
					}
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(ldapDn))
					{
						messageBody = string.Format(GlobalResources.Messages.UserManager_NewUserEmail_Approved,
							productName,
							webServerUrl,
							username,
							GlobalResources.Messages.UserManager_NewUserEmail_Approved_LDAPPass);
					}
					else if (!oAuthID.HasValue)
					{
						//Get the provider name.
						var prov = new OAuthManager().Providers_RetrieveById(oAuthID.Value);
						string oAuthName = "(None)";
						if (prov != null) oAuthName = prov.Name;

						//Make the message.`
						messageBody = string.Format(GlobalResources.Messages.UserManager_NewUserEmail_Oauth,
							productName,
							webServerUrl,
							"",
							"",
							oAuthName);
					}
				}

				try
				{
					msgToSend.subjectList.Add(0, subject);
					msgToSend.projectToken = "XX-xx";
					msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
					{
						Address = user.EmailAddress,
						SubjectId = 0,
						UserId = user.UserId,
						ArtifactTokenId = -1,
						Name = user.FullName,
						Source = "NewUser"
					});

					new NotificationManager().SendEmail(msgToSend, messageBody);
				}
				catch
				{ }

				return user;
			}
			catch (EntityConstraintViolationException)
			{
				//This can occur if the user already exists but is inactive
				//since the GetUserByLogin doesn't return inactive users
				errorCode = 6;
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Sets the user's new password
		/// </summary>
		/// <param name="username">The username name</param>
		/// <param name="newPassword">The new password</param>
		/// <param name="passwordSalt">The password SALT</param>
		/// <param name="passwordFormat">The password format</param>
		/// <returns>The status code</returns>
		public int SetPassword(string username, string newPassword, string passwordSalt, int passwordFormat)
		{
			const string METHOD_NAME = "SetPassword";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			int status = 0;
			try
			{
				User user = GetUserByLogin(username, false, true);
				user.StartTracking();
				user.Password = newPassword;
				user.PasswordFormat = passwordFormat;
				user.PasswordSalt = passwordSalt;
				user.LastPasswordChangedDate = DateTime.UtcNow;
				user.IsLegacyFormat = false;    //All new passwords will be in the new (SHA1) format rather than MD5
				Update(user, "Updated Password", user.UserId);

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return status;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				status = 1;
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Changes the password question and answer
		/// </summary>
		/// <param name="username"></param>
		/// <param name="newPasswordQuestion"></param>
		/// <param name="newPasswordAnswer"></param>
		/// <returns></returns>
		public int ChangePasswordQuestionAndAnswer(string username, string newPasswordQuestion, string newPasswordAnswer)
		{
			const string METHOD_NAME = "ChangePasswordQuestionAndAnswer";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			int status = 0;
			try
			{
				User user = GetUserByLogin(username);
				user.StartTracking();
				user.PasswordQuestion = newPasswordQuestion;
				user.PasswordAnswer = newPasswordAnswer;
				Update(user, "Updated User", user.UserId);

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return status;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				status = 1;
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the last opened project for a given user
		/// </summary>
		/// <param name="userId">The user Id we want to update the last opened project for</param>
		/// <, param name="lastOpenedProjectId">The ID of the last opened project</param>
		public void UpdateLastOpenedProject(int userId, int lastOpenedProjectId)
		{
			const string METHOD_NAME = "UpdateLastOpenedProject";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Make sure the project id is valid
				if (lastOpenedProjectId > 0)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Update the last opened project value for this user (just attach so we don't need to retrieve
						UserProfile userProfile = new UserProfile() { UserId = userId };
						context.UserProfiles.Attach(userProfile);
						userProfile.StartTracking();
						userProfile.LastOpenedProjectId = lastOpenedProjectId;
						context.SaveChanges();
					}
				}
			}
			catch (EntityForeignKeyException)
			{
				//Project has been deleted, log but ignore
				Logger.LogWarningEvent(CLASS + METHOD_NAME, string.Format("Unable to change last opened project to project PR{0} because it no longer exists", lastOpenedProjectId));
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
		}

		/// <summary>
		/// Updates the user's password info
		/// </summary>
		/// <param name="username">The user's username name</param>
		/// <param name="isPasswordCorrect">Is the password correct</param>
		/// <param name="updateLastLoginActivityDate">Do we need to update the last username activity date</param>
		/// <param name="maxInvalidPasswordAttempts">What is the max number of invalid password attempts allowed</param>
		/// <param name="passwordAttemptWindow">What is the time window (in mins) before attempts are allowed again</param>
		/// <param name="lastLoginDate">The last time the user logged-in</param>
		/// <param name="lastActivityDate">The last time the user performed some activity</param>
		/// <returns>The error code</returns>
		public int UpdatePasswordInfo(string username, bool isPasswordCorrect, bool updateLastLoginActivityDate, int maxInvalidPasswordAttempts, int passwordAttemptWindow, DateTime lastLoginDate, DateTime lastActivityDate)
		{
			const string METHOD_NAME = "UpdatePasswordInfo";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			int errorCode = 0;
			try
			{
				//First get the user from the provided username
				User user = GetUserByLogin(username);
				user.StartTracking();

				//See if the user is locked out
				if (user.IsLocked)
				{
					errorCode = 99;
				}

				//See if the password is correct
				if (!isPasswordCorrect)
				{
					DateTime failedPasswordAttemptWindowStart = DateTime.MinValue;
					if (user.FailedPasswordAttemptWindowStart.HasValue)
					{
						failedPasswordAttemptWindowStart = user.FailedPasswordAttemptWindowStart.Value;
					}
					if (DateTime.UtcNow > failedPasswordAttemptWindowStart.AddMinutes(passwordAttemptWindow))
					{
						user.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
						user.FailedPasswordAttemptCount = 1;
					}
					else
					{
						user.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
						user.FailedPasswordAttemptCount++;
					}

					if (user.FailedPasswordAttemptCount >= maxInvalidPasswordAttempts)
					{
						user.IsLocked = true;
						user.LastLockoutDate = DateTime.UtcNow;
					}
				}
				else
				{
					if (user.FailedPasswordAttemptCount > 0 || user.FailedPasswordAnswerAttemptCount > 0)
					{
						user.FailedPasswordAttemptCount = 0;
						user.FailedPasswordAttemptWindowStart = null;
						user.FailedPasswordAnswerAttemptCount = 0;
						user.FailedPasswordAnswerAttemptWindowStart = null;
						user.LastLockoutDate = null;
					}
				}

				//Now update the last activity date
				if (updateLastLoginActivityDate && !user.IsLocked)
				{
					if (lastActivityDate == DateTime.MinValue)
						user.LastActivityDate = null;
					else
						user.LastActivityDate = lastActivityDate;

					if (lastLoginDate == DateTime.MinValue)
						user.LastLoginDate = null;
					else
						user.LastLoginDate = lastLoginDate;
				}

				//Update the data
				Update(user, "Updated User", user.UserId);

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return errorCode;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				errorCode = 1;
				return errorCode;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Increases the lock count of a user if their MFA one-time password is wrong. Needed to prevent brute force attacks
		/// </summary>
		/// <param name="username">The username</param>
		/// <param name="failedPasswordAttemptCount">The number of failed attempts, from before the normal login/password check may have reset it</param>
		/// <param name="failedPasswordAttemptWindowStart">The date/time of the first attempt</param>
		public void LockAccountIfMfaRetriesExceeded(string username, int failedPasswordAttemptCount, DateTime? failedPasswordAttemptWindowStart)
		{
			const string METHOD_NAME = "LockAccountIfMfaRetriesExceeded";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users
								where u.UserName.ToLower() == username.ToLower()
								select u;

					User user = query.FirstOrDefault();
					if (user != null)
					{
						user.StartTracking();
						DateTime failedPasswordAttemptWindowStartValue = DateTime.MinValue;
						if (failedPasswordAttemptWindowStart.HasValue)
						{
							failedPasswordAttemptWindowStartValue = failedPasswordAttemptWindowStart.Value;
						}
						if (DateTime.UtcNow > failedPasswordAttemptWindowStartValue.AddMinutes(Common.ConfigurationSettings.Default.Membership_PasswordAttemptWindow))
						{
							user.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
							user.FailedPasswordAttemptCount = 1;
						}
						else
						{
							user.FailedPasswordAttemptWindowStart = DateTime.UtcNow;
							user.FailedPasswordAttemptCount = failedPasswordAttemptCount + 1;
						}

						if (user.FailedPasswordAttemptCount >= Common.ConfigurationSettings.Default.Membership_MaxInvalidPasswordAttempts)
						{
							user.IsLocked = true;
							user.LastLockoutDate = DateTime.UtcNow;
						}

						//Save the changes
						context.SaveChanges();
					}
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Unlocks a locked user
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <returns>True if successful</returns>
		public bool UnlockUser(string username)
		{
			const string METHOD_NAME = "UnlockUser";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//First get the user from the provided username
				User user = GetUserByLogin(username);
				user.StartTracking();

				//Unlock the user
				user.IsLocked = false;
				user.FailedPasswordAnswerAttemptCount = 0;
				user.FailedPasswordAttemptCount = 0;
				user.FailedPasswordAnswerAttemptWindowStart = null;
				user.FailedPasswordAttemptWindowStart = null;
				user.LastLockoutDate = null;

				//Persist changes
				Update(user, "Updated User", user.UserId);

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return true;
			}
			catch (ArtifactNotExistsException)
			{
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Resets the user's password
		/// </summary>
		/// <param name="username">The user's username</param>
		/// <param name="newPassword">The new password</param>
		/// <param name="maxInvalidPasswordAttempts">The max. number of invalid attempts allowed</param>
		/// <param name="passwordAttemptWindow">The time it takes to reset the # attempts</param>
		/// <param name="passwordSalt">The password salt</param>
		/// <param name="passwordFormat">The password format</param>
		/// <param name="passwordAnswer">The password answer (pass null if not required)</param>
		/// <returns></returns>
		public int ResetPassword(string username, string newPassword, int maxInvalidPasswordAttempts, int passwordAttemptWindow, string passwordSalt, int passwordFormat, string passwordAnswer)
		{
			const string METHOD_NAME = "ResetPassword";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			int errorCode = 0;
			try
			{

				//First get the user from the provided username
				User user = GetUserByLogin(username);
				user.StartTracking();
				passwordFormat = user.PasswordFormat;

				//See if the user is locked out
				if (user.IsLocked)
				{
					errorCode = 99;
					return errorCode;
				}

				//If the user is an LDAP maanged user throw an exception
				if (!string.IsNullOrEmpty(user.LdapDn))
				{
					throw new UserCannotResetLdapPassword(string.Format(GlobalResources.Messages.UserManager_NotResetLDAP, Common.ConfigurationSettings.Default.License_ProductType));
				}

				//Reset the password if the password answer is null or matches
				if (string.IsNullOrEmpty(passwordAnswer) || user.PasswordAnswer.ToLower() == passwordAnswer.ToLower())
				{
					user.Password = newPassword;
					user.LastPasswordChangedDate = DateTime.UtcNow;
					user.PasswordFormat = passwordFormat;
					user.PasswordSalt = passwordSalt;
					if (user.IsLegacyFormat)
					{
						//All new password use the new SHA1 salted algorithm
						user.IsLegacyFormat = false;
					}
					user.FailedPasswordAnswerAttemptCount = 0;
					user.FailedPasswordAnswerAttemptWindowStart = null;
				}
				else
				{
					DateTime failedPasswordAnswerAttemptWindowStart = DateTime.MinValue;
					if (user.FailedPasswordAnswerAttemptWindowStart.HasValue)
					{
						failedPasswordAnswerAttemptWindowStart = user.FailedPasswordAnswerAttemptWindowStart.Value;
					}
					if (DateTime.UtcNow > failedPasswordAnswerAttemptWindowStart.AddMinutes(passwordAttemptWindow))
					{
						user.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
						user.FailedPasswordAnswerAttemptCount = 1;
					}
					else
					{
						user.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
						user.FailedPasswordAnswerAttemptCount++;
					}

					if (user.FailedPasswordAnswerAttemptCount > maxInvalidPasswordAttempts)
					{
						user.IsLocked = true;
						user.LastLockoutDate = DateTime.UtcNow;
					}
					errorCode = 3;
				}

				//Persist the changes
				Update(user, "Updated User", user.UserId);

				return errorCode;
			}
			catch (ArtifactNotExistsException)
			{
				errorCode = 1;
				return errorCode;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the password with format information
		/// </summary>
		/// <param name="useLegacyFormat">Does this user use the legacy Spira v3.2 password format</param>
		/// <param name="username"></param>
		/// <param name="updateLastLoginActivityDate"></param>
		/// <param name="status"></param>
		/// <param name="password"></param>
		/// <param name="passwordFormat"></param>
		/// <param name="passwordSalt"></param>
		/// <param name="failedPasswordAttemptCount"></param>
		/// <param name="failedPasswordAnswerAttemptCount"></param>
		/// <param name="isApproved"></param>
		/// <param name="lastLoginDate"></param>
		/// <param name="lastActivityDate"></param>
		/// <param name="isActive">Is the user active</param>
		/// <param name="ldapDn">The user's LDAP DN</param>
		public void GetPasswordWithFormat(string username, bool updateLastLoginActivityDate, out int status, out string password, out int passwordFormat, out string passwordSalt, out int failedPasswordAttemptCount, out int failedPasswordAnswerAttemptCount, out bool isApproved, out DateTime lastLoginDate, out DateTime lastActivityDate, out bool useLegacyFormat, out string ldapDn, out bool isActive, out bool isOauth)
		{
			const string METHOD_NAME = "GetPasswordWithFormat";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				status = 0;

				//First get the user from the provided username
				User user = GetUserByLogin(username, false, true);
				useLegacyFormat = user.IsLegacyFormat;
				user.StartTracking();

				//See if the user is inactive
				isActive = user.IsActive;

				//See if the user is locked out
				if (user.IsLocked)
				{
					status = 99;
					password = null;
					passwordFormat = 0;
					passwordSalt = null;
					failedPasswordAttemptCount = 0;
					failedPasswordAnswerAttemptCount = 0;
					isApproved = false;
					lastLoginDate = DateTime.UtcNow;
					lastActivityDate = DateTime.UtcNow;
					ldapDn = null;
					isOauth = false;
					return;
				}

				//Update the out params from the user object
				password = user.Password;
				passwordFormat = user.PasswordFormat;
				passwordSalt = user.PasswordSalt;
				failedPasswordAttemptCount = user.FailedPasswordAttemptCount;
				failedPasswordAnswerAttemptCount = user.FailedPasswordAnswerAttemptCount;
				isApproved = user.IsApproved;
				lastLoginDate = (user.LastLoginDate.HasValue) ? user.LastLoginDate.Value : DateTime.MinValue;
				lastActivityDate = (user.LastActivityDate.HasValue) ? user.LastActivityDate.Value : DateTime.MinValue;
				ldapDn = user.LdapDn;
				isOauth = user.OAuthProviderId.HasValue;

				//Update the username activity if required
				if (updateLastLoginActivityDate && user.IsApproved)
				{
					user.LastActivityDate = DateTime.UtcNow;
					user.LastLoginDate = DateTime.UtcNow;
					UpdateLogin(user, "Login");
				}

				return;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				useLegacyFormat = false;
				password = null;
				passwordFormat = 0;
				passwordSalt = null;
				failedPasswordAttemptCount = 0;
				failedPasswordAnswerAttemptCount = 0;
				isApproved = false;
				lastLoginDate = DateTime.UtcNow;
				lastActivityDate = DateTime.UtcNow;
				status = 1;
				ldapDn = null;
				isActive = false;
				isOauth = false;
				return;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Gets the password from the database</summary>
		/// <param name="username">The user's username</param>
		/// <param name="passwordAnswer">The answer to the password question</param>
		/// <param name="requiresQuestionAndAnswer">Do we require a question and answer</param>
		/// <param name="passwordFormat">The password format</param>
		/// <param name="maxInvalidPasswordAttempts">The maximum number of times we can enter the wrong password answer</param>
		/// <param name="passwordAttemptWindow">How int (in mins) do they have to wait to retry password answers</param>
		/// <param name="status">The status</param>
		/// <returns>The password</returns>
		public string GetPassword(string username, string passwordAnswer, bool requiresQuestionAndAnswer, int passwordAttemptWindow, int maxInvalidPasswordAttempts, out int passwordFormat, out int status)
		{
			const string METHOD_NAME = "GetPassword()";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				string password = null;
				status = 0;

				//First get the user from the provided username
				User user = GetUserByLogin(username);
				user.StartTracking();
				passwordFormat = user.PasswordFormat;

				//See if the user is locked out
				if (user.IsLocked)
				{
					status = 99;
					password = null;
					return password;
				}

				//See if we have a password answer
				if (!string.IsNullOrEmpty(passwordAnswer))
				{
					if (string.IsNullOrEmpty(user.PasswordAnswer) || user.PasswordAnswer.ToLower() != passwordAnswer.ToLower())
					{
						DateTime failedPasswordAnswerAttemptWindowStart = DateTime.MinValue;
						if (user.FailedPasswordAnswerAttemptWindowStart.HasValue)
						{
							failedPasswordAnswerAttemptWindowStart = user.FailedPasswordAnswerAttemptWindowStart.Value;
						}
						if (DateTime.UtcNow > failedPasswordAnswerAttemptWindowStart.AddMinutes(passwordAttemptWindow))
						{
							user.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
							user.FailedPasswordAnswerAttemptCount = 1;
						}
						else
						{
							user.FailedPasswordAnswerAttemptWindowStart = DateTime.UtcNow;
							user.FailedPasswordAnswerAttemptCount++;
						}

						if (user.FailedPasswordAnswerAttemptCount >= maxInvalidPasswordAttempts)
						{
							user.IsLocked = true;
							user.LastLockoutDate = DateTime.UtcNow;
						}

						//Set the status code
						status = 3;
					}
					else
					{
						if (user.FailedPasswordAnswerAttemptCount > 0)
						{
							//Reset the # failed attempts
							user.FailedPasswordAnswerAttemptCount = 0;
							user.FailedPasswordAnswerAttemptWindowStart = null;
						}
					}

					//Persist the changes
					try
					{
						Update(user, "Get Password", user.UserId);
					}
					catch (EntityException)
					{
						status = -1;
					}
				}

				//If no errors, get the password
				if (status == 0)
				{
					password = user.Password;
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return password;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				status = 1;
				passwordFormat = 0;
				return null;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a user's username
		/// </summary>
		/// <param name="username">The existing username</param>
		/// <param name="newLogin">The new username</param>
		/// <remarks>If the existing username is in use return FALSE, otherwise return TRUE</remarks>
		public bool ChangeLogin(string username, string newLogin)
		{
			const string METHOD_NAME = "ChangeLogin";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//First we need to get the current user
				User user = GetUserByLogin(username);
				//If the existing user doesn't exist at this stage we have an error so let it pass back up to the calling class

				//Now make sure that the new username doesn't exist
				try
				{
					GetUserByLogin(newLogin);
				}
				catch (ArtifactNotExistsException)
				{
					//This is expected so update the username
					user.StartTracking();
					user.UserName = newLogin;
					Update(user, "Changed Login", user.UserId);
					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return true;
				}

				//If a user does exist with that username, then we can't make the change
				return false;
			}
			catch (EntityConstraintViolationException)
			{
				//This can occur if the user already exists but is inactive
				//since the GetUserByLogin doesn't return inactive users
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>
		/// Updates a user's information with the provided info
		/// </summary>
		/// <param name="lastActivityDate">The last activity date (in UTC)</param>
		/// ><param name="lastLoginDate">The last username date (in UTC)</param>
		public int UpdateUser(string username, string emailAddress, string comment, bool approved, DateTime lastLoginDate, DateTime lastActivityDate, bool requireUniqueEmail)
		{
			const string METHOD_NAME = "UpdateUser";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			int status = 0;
			try
			{
				//First get the user from the provided username
				User user = GetUserByLogin(username, includeInActive: true);

				//If we are required to have unique email addresses make sure it is unique
				if (requireUniqueEmail && IsEmailAddressInUse(emailAddress, username))
				{
					status = 7;
					Logger.LogExitingEvent(CLASS + METHOD_NAME);
					return status;
				}

				//Update the user record (handle 'nulls' correctly)
				user.StartTracking();
				if (lastActivityDate == DateTime.MinValue)
				{
					user.LastActivityDate = null;
				}
				else
				{
					user.LastActivityDate = lastActivityDate;
				}
				user.EmailAddress = emailAddress;
				user.Comment = comment;
				user.IsApproved = approved;
				if (lastLoginDate == DateTime.MinValue)
				{
					user.LastLoginDate = null;
				}
				else
				{
					user.LastLoginDate = lastLoginDate;
				}

				//Persist the changes
				Update(user, "Updated User", user.UserId);

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return status;
			}
			catch (ArtifactNotExistsException)
			{
				//We don't have a user so return the status code for it
				status = 1;
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a user entity in the system
		/// </summary>
		/// <param name="user">The user entity</param>
		public void UpdateLogin(User user, string action = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			UserActivityLogManager userActivityLogManager = new UserActivityLogManager();

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

					string adminSectionName = "Users";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;
					//Attach the entity to the context and save
					context.Users.ApplyChanges(user);

					context.SaveChanges();

					userActivityLogManager.LogCreation(Convert.ToInt32(user.UserId), Convert.ToInt32(adminSectionId), Convert.ToInt32(user.UserId), user.UserName, action, DateTime.UtcNow);
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (EntityConstraintViolationException)
			{
				//If we have a unique constraint violation, throw a business exception
				throw new UserDuplicateUserNameException("That user-name is already in use!");
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a user entity in the system
		/// </summary>
		/// <param name="user">The user entity</param>
		public void Update(User user, string action = null, int? userId = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			UserActivityLogManager userActivityLogManager = new UserActivityLogManager();

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

					string adminSectionName = "Users";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;
					//Attach the entity to the context and save
					context.Users.ApplyChanges(user);

					context.UserSaveChanges(userId, user.UserId, adminSectionId, action, true, true, null);
					context.SaveChanges();	
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (EntityConstraintViolationException)
			{
				//If we have a unique constraint violation, throw a business exception
				throw new UserDuplicateUserNameException("That user-name is already in use!");
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Is the email address already being used
		/// </summary>
		/// <param name="emailAddress">The email address</param>
		/// <param name="ignoreLogin">The username of the user that we're checking it for</param>
		/// <returns>True if already in use by another username</returns>
		protected bool IsEmailAddressInUse(string emailAddress, string ignoreLogin)
		{
			const string METHOD_NAME = "IsEmailAddressInUse";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				int count;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users
								where u.UserName.ToLower() != ignoreLogin.ToLower() && u.EmailAddress.ToLower() == emailAddress.ToLower()
								select u;
					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return count > 0;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a user by its username
		/// </summary>
		/// <param name="username">The username of the user</param>
		/// <returns>The user object</returns>
		/// <param name="updateLastActivity">Do we want to update its last activity date/time</param>
		/// <remarks>Is case insensitive and only returns active users</remarks>
		public User GetUserByLogin(string username, bool updateLastActivity = false, bool includeInActive = false)
		{
			const string METHOD_NAME = "GetUserByLogin";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<User> userContext = context.Users.Include("Profile");

					var query = from u in userContext
								where u.UserName.ToLower() == username.ToLower() &&
								(includeInActive == true || u.IsActive == !includeInActive)
								select u;

					user = query.FirstOrDefault();

					//Make sure data was returned
					if (user == null)
					{
						throw new ArtifactNotExistsException("User with username '" + username + "' does not exist in the system");
					}

					//Now update the last activity date
					if (updateLastActivity)
					{
						user.StartTracking();
						user.LastActivityDate = DateTime.UtcNow;
						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

						string adminSectionName = "Users";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;
						context.UserSaveChanges(user.UserId, user.UserId, adminSectionId, "GetLogin", true, true, null);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return user;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Log as a trace only because we already have a Failure Audit logged by the Membership Provider
				Logger.LogTraceEvent(CLASS + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all users in the system (sorted by first name, last name)</summary>
		/// <param name="includeProfile">Should we include the profile</param>
		/// <returns>The user collection</returns>
		public List<User> GetUsers(bool includeProfile = false)
		{
			int totalRecords;
			List<User> retList = new List<User>();
			retList = GetUsers(0, int.MaxValue, out totalRecords, includeProfile);
			return retList;
		}

		/// <summary>Retrieves all users in the system (sorted by first name, last name)</summary>
		/// <param name="pageSize">How many users to return in a page</param>
		/// <param name="pageIndex">Which page index to return</param>
		/// <param name="includeProfile">Should we include the profile</param>
		/// <param name="totalRecords">How many total users we have</param>
		/// <returns>The user collection</returns>
		public List<User> GetUsers(int pageIndex, int pageSize, out int totalRecords, bool includeProfile = false)
		{
			List<SortEntry<User, string>> sorts = new List<SortEntry<User, string>>();
			SortEntry<User, string> sort1 = new SortEntry<User, string>();
			sort1.Direction = SortDirection.Ascending;
			sort1.Expression = u => u.UserName;
			sorts.Add(sort1);
			return GetUsers(null, sorts, pageIndex, pageSize, out totalRecords, includeProfile);
		}

		/// <summary>
		/// Retrieves a sorted, filterable list of users, including their profiles
		/// </summary>
		/// <param name="pageSize">How many users to return in a page</param>
		/// <param name="pageIndex">Which page index to return (zero-based)</param>
		/// <param name="includeProfile">Should we include the profile</param>
		/// <param name="totalRecords">How many total users we have</param>
		/// <param name="filters">The list of filters</param>
		/// <param name="includeInactive">Should we include inactive users</param>
		/// <param name="includeUnapproved">Should we include unapproved users</param>
		/// <param name="sortExpression">The sort expression (column [ASC|DESC])</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <param name="excludeProjectGroupMembersProjectGroupId">The id of the project group we should exclude any members of</param>
		/// <param name="excludeProjectMembersProjectId">The id of the project we should exclude any members of</param>
		/// <returns>The user collection</returns>
		public List<User> GetUsers(Hashtable filters, string sortExpression, int pageIndex, int pageSize, double utcOffset, out int totalRecords, bool includeUnapproved = false, bool includeInactive = false, int? excludeProjectMembersProjectId = null, int? excludeProjectGroupMembersProjectGroupId = null)
		{
			const string METHOD_NAME = "GetUsers";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<User> users;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from u in context.Users.Include("Profile")
								select u;

					//Add the standard filters
					if (!includeUnapproved)
					{
						query = query.Where(u => u.IsApproved);
					}
					if (!includeInactive)
					{
						query = query.Where(u => u.IsActive);
					}

					//Special clause to exclude any project members (also adds an active filter)
					if (excludeProjectMembersProjectId.HasValue)
					{
						query = query.Where(u => !u.ProjectMembership.Any(p => p.ProjectId == excludeProjectMembersProjectId.Value));
					}

					//Special clause to exclude any project group members
					if (excludeProjectGroupMembersProjectGroupId.HasValue)
					{
						query = query.Where(u => !u.ProjectGroupMembership.Any(p => p.ProjectGroupId == excludeProjectGroupMembersProjectGroupId.Value));
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Ignore some of the old v3.2 filters in case they are saved
						List<string> ignoreList = new List<string>();
						ignoreList.Add("FirstName");
						ignoreList.Add("LastName");
						ignoreList.Add("MiddleInitial");
						ignoreList.Add("Department");
						ignoreList.Add("AdminYn");
						ignoreList.Add("ActiveYn");
						ignoreList.Add("EmailEnabledYn");
						Expression<Func<User, bool>> filterExpression = CreateFilterExpression<User>(null, null, Artifact.ArtifactTypeEnum.None, filters, utcOffset, ignoreList);
						if (filterExpression != null)
						{
							query = (IOrderedQueryable<User>)query.Where(filterExpression);
						}
					}

					//Translate any sort expressions
					sortExpression = User.TranslateSortExpression(sortExpression);

					//Add the dynamic sorts
					//Always sort by user id at the end to ensure stable sorting
					query = query.OrderUsingSortExpression(sortExpression, "UserId");

					//Make sure pagination is in range
					totalRecords = query.Count();
					if (pageIndex > totalRecords - 1)
					{
						pageIndex = (int)totalRecords - pageSize;
					}
					if (pageIndex < 0)
					{
						pageIndex = 0;
					}

					totalRecords = query.Count();
					users = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return users;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all users in the system (paginated)
		/// </summary>
		/// <param name="filters">The LINQ filters expression</param>
		/// <param name="sorts">A collection of LINQ sorts</param>
		/// <param name="pageSize">How many users to return in a page</param>
		/// <param name="pageIndex">Which page index to return</param>
		/// <param name="totalRecords">How many total users we have</param>
		/// <param name="includeProfile">Should we include the user's profile</param>
		/// <returns>The user collection</returns>
		/// <remarks>Includes the user's profile as well</remarks>
		public List<User> GetUsers<SortDataType>(Expression<Func<User, bool>> filters, List<SortEntry<User, SortDataType>> sorts, int pageIndex, int pageSize, out int totalRecords, bool includeProfile = false)
		{
			const string METHOD_NAME = "GetUsers";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<User> users;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<User> userQuery = context.Users;
					if (includeProfile)
					{
						userQuery = userQuery.Include("Profile");
					}

					//Build the base query
					var query = from u in userQuery
								select u;

					//Add the dynamic filters
					if (filters != null)
					{
						query = (IOrderedQueryable<User>)query.Where(filters);
					}

					//Add the dynamic sorts
					bool isFirst = true;
					foreach (SortEntry<User, SortDataType> sortEntry in sorts)
					{
						if (sortEntry.Direction == SortDirection.Descending)
						{
							if (isFirst)
							{
								query = query.OrderByDescending(sortEntry.Expression);
							}
							else
							{
								query = ((IOrderedQueryable<User>)query).ThenByDescending(sortEntry.Expression);
							}
						}
						else
						{
							if (isFirst)
							{
								query = query.OrderBy(sortEntry.Expression);
							}
							else
							{
								query = ((IOrderedQueryable<User>)query).ThenBy(sortEntry.Expression);
							}
						}
						isFirst = false;
					}

					totalRecords = query.Count();

					//Make sure pagination is in range
					totalRecords = query.Count();
					if (pageIndex > totalRecords - 1)
					{
						pageIndex = (int)totalRecords - pageSize;
					}
					if (pageIndex < 0)
					{
						pageIndex = 0;
					}
					users = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return users;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<User> GetUsersAssignedToTaraVaultProject(long projectId)
		{
			List<User> retValue = new List<User>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				ObjectQuery<User> userQuery = context.Users
                    .Include(u => u.Profile)
                    .Include(u => u.TaraVault);

				//Build the base query
				var query = from u in userQuery
							where u.TaraVault.VaultProject.Count(vp => vp.ProjectId == projectId) == 1
							select u;

				//Add the dynamic sorts
				/*
                bool isFirst = true;
                foreach (SortEntry<User, SortDataType> sortEntry in sorts)
                {
                    if (sortEntry.Direction == SortDirection.Descending)
                    {
                        if (isFirst)
                        {
                            query = query.OrderByDescending(sortEntry.Expression);
                        }
                        else
                        {
                            query = ((IOrderedQueryable<User>)query).ThenByDescending(sortEntry.Expression);
                        }
                    }
                    else
                    {
                        if (isFirst)
                        {
                            query = query.OrderBy(sortEntry.Expression);
                        }
                        else
                        {
                            query = ((IOrderedQueryable<User>)query).ThenBy(sortEntry.Expression);
                        }
                    }
                    isFirst = false;
                }
                 */

				// totalRecords = query.Count();

				//Make sure pagination is in range
				/*
                totalRecords = query.Count();
                if (pageIndex > totalRecords - 1)
                {
                    pageIndex = (int)totalRecords - pageSize;
                }
                if (pageIndex < 0)
                {
                    pageIndex = 0;
                }
                users = query
                    .Skip(pageIndex)
                    .Take(pageSize)
                    .ToList();
                 */
				retValue = query.ToList();
			}

			return retValue;
		}

		/// <summary>
		/// Sets the user's profile information
		/// </summary>
		/// <param name="collection">The collection of profile properties</param>
		/// <param name="username">The user's username</param>
		public void SetProfileData(SettingsPropertyValueCollection collection, string username, bool userIsAuthenticated)
		{
			const string METHOD_NAME = "SetProfileData";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				bool anyItemsToSave = false;

				// First make sure we have at least one item to save
				foreach (SettingsPropertyValue pp in collection)
				{
					if (pp.IsDirty)
					{
						if (!userIsAuthenticated)
						{
							bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
							if (!allowAnonymous)
								continue;
						}
						anyItemsToSave = true;
						break;
					}
				}

				if (!anyItemsToSave)
				{
					return;
				}

				List<ProfileColumnData> columnData = new List<ProfileColumnData>(collection.Count);
				foreach (SettingsPropertyValue pp in collection)
				{
					if (!userIsAuthenticated)
					{
						bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
						if (!allowAnonymous)
							continue;
					}

					//Normal logic for original SQL provider
					//if (!pp.IsDirty && pp.UsingDefaultValue) // Not fetched from DB and not written to

					//Can eliminate unnecessary updates since we are using a table though
					if (!pp.IsDirty)
						continue;

					object value = null;

					// REVIEW: Is this handling null case correctly?
					if (pp.Deserialized && pp.PropertyValue == null)
					{ // is value null?
						value = null;
					}
					else
					{
						value = pp.PropertyValue;
					}

					// REVIEW: Might be able to ditch datatype
					columnData.Add(new ProfileColumnData(pp.Name, pp, value, pp.Property.PropertyType));
				}

				//Get the user entity
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.UserName.ToLower() == username.ToLower()
								select u;
					user = query.FirstOrDefault();

					//Make sure that the user itself exists. If no user exists, just do nothing and return
					if (user == null)
					{
						return;
					}

					//If the user exists, but no profile exists, create a new profile
					if (user.Profile == null)
					{
						user.Profile = new UserProfile();
					}
					user.StartTracking();
					user.Profile.StartTracking();
					//Populate any default required fields
					user.Profile.LastUpdateDate = DateTime.Now;
					user.Profile.UnreadMessages = 0;
					user.Profile.IsBusy = false;
					user.Profile.IsAway = false;

					for (int i = 0; i < columnData.Count; ++i)
					{
						//Need to find matching property name
						ProfileColumnData colData = columnData[i];
						PropertyInfo propInfo = user.Profile.GetType().GetProperty(colData.PropertyName);
						SettingsPropertyValue propValue = colData.PropertyValue;
						if (propInfo != null)
						{
							if (propValue == null || propValue.PropertyValue == null)
							{
								if (propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) || (propInfo.PropertyType == typeof(string)))
								{
									propInfo.SetValue(user.Profile, null, null);
								}
							}
							else if (propValue.PropertyValue.GetType() == colData.DataType)
							{
								//The types match
								propInfo.SetValue(user.Profile, propValue.PropertyValue, null);
							}
							else if (colData.DataType.IsGenericType && (propValue.PropertyValue.GetType() == Nullable.GetUnderlyingType(colData.DataType)))
							{
								//We have a nullable property and its base type matches
								propInfo.SetValue(user.Profile, propValue.PropertyValue, null);
							}
						}
					}

					//Now update the last activity date and save all changes
					user.LastActivityDate = DateTime.UtcNow;
					context.SaveChanges();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the user's profile information
		/// </summary>
		/// <param name="properties">The collection of profile properties</param>
		/// <param name="svc">The collection of property values</param>
		/// <param name="username">The user's username</param>
		public void GetProfileData(SettingsPropertyCollection properties, SettingsPropertyValueCollection svc, string username)
		{
			const string METHOD_NAME = "GetProfileData";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				List<ProfileColumnData> columnData = new List<ProfileColumnData>(properties.Count);

				int columnCount = 0;
				foreach (SettingsProperty prop in properties)
				{
					SettingsPropertyValue value = new SettingsPropertyValue(prop);
					svc.Add(value);

					columnData.Add(new ProfileColumnData(prop.Name, value, null /* not needed for get */, prop.PropertyType));
					++columnCount;
				}

				//Get the user entity
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users.Include("Profile")
								where u.UserName.ToLower() == username.ToLower() && u.IsActive
								select u;
					user = query.FirstOrDefault();

					//Make sure data was returned
					if (user != null && user.Profile != null)
					{
						for (int i = 0; i < columnData.Count; ++i)
						{
							//Need to find matching property name
							ProfileColumnData colData = columnData[i];
							PropertyInfo propInfo = user.Profile.GetType().GetProperty(colData.PropertyName);
							SettingsPropertyValue propValue = colData.PropertyValue;
							if (propInfo != null)
							{
								object val = propInfo.GetValue(user.Profile, null);
								//Only initialize a SettingsPropertyValue for non-null values
								if (val != null && (val.GetType() == colData.DataType || val.GetType() == Nullable.GetUnderlyingType(colData.DataType)))
								{
									propValue.PropertyValue = val;
									propValue.IsDirty = false;
									propValue.Deserialized = true;
								}
							}
						}

						//We used to update the last activity date, that caused performance issues
						//so we no longer do that here. We only update it once per page load
					}
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a user by its email address
		/// </summary>
		/// <param name="emailAddress">The email address of the user</param>
		/// <returns>The user object</returns>
		/// <remarks>Is case insensitive and only returns active users</remarks>
		public User GetUserByEmailAddress(string emailAddress)
		{
			const string METHOD_NAME = "GetUserByEmailAddress";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from u in context.Users
								where u.EmailAddress.ToLower() == emailAddress.ToLower() && u.IsActive
								select u;
					user = query.FirstOrDefault();
				}

				//Make sure data was returned
				if (user == null)
				{
					throw new ArtifactNotExistsException("User with email address '" + emailAddress + "' does not exist in the system");
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return user;
			}
			catch (ArtifactNotExistsException exception)
			{
				//Log as a trace only because we already have a Failure Audit logged by the Membership Provider
				Logger.LogTraceEvent(CLASS + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Sends an email to the Administrator, requesting a new account be created</summary>
		/// <param name="username">The username of the account</param>
		public void SendNewAccountRequestEmail(string username)
		{
			const string METHOD = CLASS + "SendNewAccountRequestEmail()";
			Logger.LogEnteringEvent(METHOD);

			//Generate the mail message.
			NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();
			msgToSend.projectToken = "PR-xx";

			//Next get the administrator's email address to send the email to
			User administratorUser = this.GetUserById(UserManager.UserSystemAdministrator);
			msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
			{
				Address = administratorUser.EmailAddress,
				Name = administratorUser.FullName,
				SubjectId = 1,
				UserId = administratorUser.UserId,
				Source = "NewAcctReq"
			});

			//Get the new user's information (they will not be approved currently)
			User newUser = this.GetUserByLogin(username);

			//Get the URL and product name
			string productName = Common.ConfigurationSettings.Default.License_ProductType;
			string webServerUrl = Common.ConfigurationSettings.Default.General_WebServerUrl;

			//Create the mail message
			msgToSend.subjectList.Add(1, "Request for new " + productName + " account.");
			string messageBody = "There has been a new request for a " + productName + " account for the following person:\r\n\r\n" +
				"First Name: " + newUser.Profile.FirstName + "\r\n" +
				"Last Name: " + newUser.Profile.LastName + "\r\n" +
				"Middle Initial: " + newUser.Profile.MiddleInitial + "\r\n" +
				"Email Address: " + newUser.EmailAddress + "\r\n\r\n" +
				"You need to login to the system and approve this account.\r\n\r\n" +
				"URL: " + webServerUrl + "\r\n";

			try
			{
				new NotificationManager().SendEmail(msgToSend, messageBody);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(METHOD, exception);
				throw;
			}

			Logger.LogExitingEvent(CLASS + METHOD);
		}

		/// <summary>Sends an email letting the user know that their account has been approved</summary>
		/// <param name="username">the username for the user</param>
		public void SendUserApproveNotification(string username)
		{
			const string METHOD = CLASS + "SendUserApproveNotification()";
			Logger.LogEnteringEvent(METHOD);

			//Generate the mail message.
			NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();
			msgToSend.projectToken = "PR-xx";

			//Retrieve the user record
			User user = this.GetUserByLogin(username);
			msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
			{
				Address = user.EmailAddress,
				Name = user.FullName,
				SubjectId = 1,
				UserId = user.UserId,
				Source = "AcctApp"
			});

			//Get the URL and product name
			string productName = Common.ConfigurationSettings.Default.License_ProductType;
			string webServerUrl = Common.ConfigurationSettings.Default.General_WebServerUrl;

			//Create the new mail message - eventually will need to template it
			msgToSend.subjectList.Add(1, "New " + productName + " user account approved.");
			string body = "Your request for a new " + productName + " account has been approved:\r\n\r\n" +
				"URL: " + webServerUrl +
				"\r\nUser Name: " + username +
				"\r\n\r\nPlease contact the System Administrator with any questions.\r\n";

			try
			{
				new NotificationManager().SendEmail(msgToSend, body);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD, exception);
				throw;
			}

			Logger.LogExitingEvent(METHOD);
		}

		/// <summary>Sends an email letting the user know that their password has been reset</summary>
		/// <param name="username">the username for the user</param>
		/// <param name="newPassword">The new password (unencrypted)</param>
		/// <remarks>Makes sure we don't sent more than 1 email in X seconds (avoid flooding attacks)</remarks>
		public void SendPasswordResetNotification(string username, string newPassword)
		{
			const string METHOD = CLASS + "SendPasswordResetNotification()";
			Logger.LogEnteringEvent(METHOD);

			//Make sure we didn't send one recently
			DateTime lastEmailSent = Common.ConfigurationSettings.Default.EmailSettings_LastResetEmailDateTime;
			int timeInterval = Common.ConfigurationSettings.Default.EmailSettings_LastResetTimeIntervalSeconds;
			DateTime utcNow = DateTime.UtcNow;

			if (lastEmailSent > utcNow.AddSeconds(-timeInterval))
			{
				//Log a message why we did not send email
				Logger.LogFailureAuditEvent(METHOD, GlobalResources.Messages.UserManager_DidNotSendPasswordResetEmailDueToSpam);
			}
			else
			{
				//Generate the mail message.
				NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();
				msgToSend.projectToken = "PR-xx";

				//Retrieve the user record
				User user = this.GetUserByLogin(username);
				msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
				{
					Address = user.EmailAddress,
					Name = user.FullName,
					SubjectId = 1,
					UserId = user.UserId,
					Source = "PassRest"
				});

				//Get the URL and product name
				string productName = Common.ConfigurationSettings.Default.License_ProductType;
				string webServerUrl = Common.ConfigurationSettings.Default.General_WebServerUrl;

				//Create the new mail message - eventually will need to template it
				msgToSend.subjectList.Add(1, "Reset " + productName + " Password");
				string body = "Your " + productName + " password for " + webServerUrl + "  has been reset to:\r\n\r\n" + newPassword + "\r\n\r\nPlease contact the System Administrator with any questions.\r\n";

				try
				{
					new NotificationManager().SendEmail(msgToSend, body);
				}
				catch (System.Exception exception)
				{
					Logger.LogErrorEvent(METHOD, exception);
					throw;
				}

				//Update time of last send
				Common.ConfigurationSettings.Default.EmailSettings_LastResetEmailDateTime = utcNow;
				Common.ConfigurationSettings.Default.Save();
			}

			Logger.LogExitingEvent(METHOD);
		}

		public ProjectSignature RetrieveAssignedApproversForProjectAndUser(int projectId, int? userId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.IsActive && x.IsTestCaseSignatureRequired == true);
				if (userId.HasValue)
				{
					signatures = signatures.Where(x => x.UserId == userId.Value);
				}

				return signatures.FirstOrDefault();
			}
		}

		public List<ProjectSignature> RetrieveAssignedApproversForProjectAndUserId(int projectId, int? userId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.IsActive && x.IsTestCaseSignatureRequired == true);
				if (userId.HasValue)
				{
					signatures = signatures.Where(x => x.UserId == userId.Value);
				}

				return signatures.ToList();
			}
		}

		public List<ProjectSignature> RetrieveAssignedApproversForProjectId(int projectId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Where(x => x.ProjectId == projectId && x.IsActive && x.IsTestCaseSignatureRequired == true);				

				return signatures.ToList();
			}
		}

		public List<ProjectSignature> RetrieveAssignedApproversToSendEmail(int projectId, int userId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Where(x => x.ProjectId == projectId && x.UserId != userId && x.IsActive && x.IsTestCaseSignatureRequired == true);

				return signatures.ToList();
			}
		}

		public List<ProjectSignature> RetrieveDeactivateUsersForProjectId(int projectId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Where(x => x.ProjectId == projectId && !x.IsActive && x.IsTestCaseSignatureRequired == false);

				return signatures.ToList();
			}
		}


		public TST_REQUIREMENT_APPROVAL_USERS RetrieveAssignedApproversForProjectAndUser1(int projectId, int? userId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.RequirementApprovalUsers.Include(x => x.TST_USER).Include(x => x.TST_USER.Profile).Where(x => x.PROJECT_ID == projectId && x.IS_ACTIVE);
				if (userId.HasValue)
				{
					signatures = signatures.Where(x => x.USER_ID == userId.Value);
				}

				return signatures.FirstOrDefault();
			}
		}

		public ProjectSignature RetrieveAssignedApproversForProject(int projectId, int userId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.UserId == userId && x.IsActive && x.IsTestCaseSignatureRequired == true);
				

				return signatures.FirstOrDefault();
			}
		}

		public ProjectSignature RetrieveAssignedApproverById(int projectSignatureId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectUserSignatureId == projectSignatureId);

				return signatures.FirstOrDefault();
			}
		}

		public List<ProjectSignature> RetrieveAssignedApproversForProject(int projectId, int? artifactId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.IsActive && x.IsTestCaseSignatureRequired == true);
				if (artifactId.HasValue)
				{
					signatures = signatures.Where(x => x.ArtifactTypeId == artifactId.Value);
				}

				return signatures.ToList();
			}
		}

		public ProjectSignature RetrieveAllAssignedApproversForProject(int projectId, int userId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.IsActive == true && x.IsTestCaseSignatureRequired == true && x.UserId == userId);
				
				return signatures.FirstOrDefault();
			}
		}

		public List<ProjectSignature> RetrieveAllAssignedApproversForProject(int projectId, int? artifactId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId && x.IsTestCaseSignatureRequired == false);
				if (artifactId.HasValue)
				{
					signatures = signatures.Where(x => x.ArtifactTypeId == artifactId.Value);
				}

				return signatures.ToList();
			}
		}

		public List<ProjectSignature> RetrieveAssignedApproversForProject1(int projectId, int? artifactId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.ProjectSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.ProjectId == projectId);
				if (artifactId.HasValue)
				{
					signatures = signatures.Where(x => x.ArtifactTypeId == artifactId.Value);
				}

				return signatures.ToList();
			}
		}


		public void UpdateProjectAssignedApprovers(List<ProjectSignature> updatedEntities)
		{

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				foreach (var item in updatedEntities)
				{
					context.ProjectSignatures.ApplyChanges(item);
				}

				context.SaveChanges();
			}
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown when you try and add/update a user setting for a collection that does not exist in the database
	/// </summary>
	public class UserSettingNotExistsException : ApplicationException
	{
		public UserSettingNotExistsException()
		{
		}
		public UserSettingNotExistsException(string message)
			: base(message)
		{
		}
		public UserSettingNotExistsException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try and insert/update a user
	/// with a user-name that is already in use
	/// </summary>
	public class UserDuplicateUserNameException : ApplicationException
	{
		public UserDuplicateUserNameException()
		{
		}
		public UserDuplicateUserNameException(string message)
			: base(message)
		{
		}
		public UserDuplicateUserNameException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try and delete a user
	/// account that is NOT a demo account (i.e. first-name = "demo")
	/// </summary>
	public class UserCannotDeleteRealUserException : ApplicationException
	{
		public UserCannotDeleteRealUserException()
		{
		}
		public UserCannotDeleteRealUserException(string message)
			: base(message)
		{
		}
		public UserCannotDeleteRealUserException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// This exception is thrown when you try and reset the password for an LDAP-managed user
	/// </summary>
	public class UserCannotResetLdapPassword : ApplicationException
	{
		public UserCannotResetLdapPassword()
		{
		}
		public UserCannotResetLdapPassword(string message)
			: base(message)
		{
		}
		public UserCannotResetLdapPassword(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>
	/// Extends the hashtable class to encapsulate storage and retrieval
	/// of user configuration data in the database
	/// </summary>
	public class UserSettingsCollection : Hashtable
	{
		protected int userId;
		protected string collectionName;

		/// <summary>
		/// Constructor method
		/// </summary>
		public UserSettingsCollection(int userId, string collectionName)
			: base()
		{
			//Store the user this collection is stored for
			this.userId = userId;
			this.collectionName = collectionName;
		}

		/// <summary>
		/// Returns the current user id
		/// </summary>
		public int UserId
		{
			get
			{
				return this.userId;
			}
		}

		/// <summary>
		/// Persists the hashtable entries back to the database
		/// </summary>
		public void Save()
		{
			Business.UserManager user = new Business.UserManager();

			//First get a list of the existing hashtable entries from the database
			UserCollection userCollection = user.RetrieveSettings(this.userId, this.collectionName);

			//Now iterate through the hashtable and compare with saved data
			//We cannot use a for...each loop in case the hashtable has been changed elsewhere
			foreach (string entryKey in this.Keys)
			{
				//Get the entry key and values (convert to string and typecode)
				object entryValue = this[entryKey];
				if (entryKey != null)
				{
					int typeCode;
					string entryValueString = SerializeValue(entryValue, out typeCode);

					//See if the key exists in the entity collection
					bool keyFound = false;
					foreach (UserCollectionEntry userCollectionEntry in userCollection.Entries)
					{
						if (userCollectionEntry.EntryKey == entryKey)
						{
							keyFound = true;
							//Update the row if the values different
							if (entryValueString != userCollectionEntry.EntryValue)
							{
								user.UpdateSetting(this.userId, this.collectionName, entryKey, entryValueString, typeCode);
							}
						}
					}
					//if the key was not found, then we need to do an insert
					if (!keyFound)
					{
						user.InsertSetting(this.userId, this.collectionName, entryKey, entryValueString, typeCode);
					}
				}
			}

			//Finally if there is a dataset record with no matching key, then we need to delete the row
			foreach (UserCollectionEntry userCollectionEntry in userCollection.Entries)
			{
				//Get the key name from the database record
				string entryKey = userCollectionEntry.EntryKey;
				if (this[entryKey] == null)
				{
					user.DeleteSetting(this.userId, this.collectionName, entryKey);
				}
			}
		}

		/// <summary>
		/// Converts the native object into a string and associated type-code
		/// </summary>
		/// <param name="entryValue">The native object value</param>
		/// <param name="typeCode">The type code of the native object [out]</param>
		/// <returns>The string representation of the object</returns>
		protected string SerializeValue(object entryValue, out int typeCode)
		{
			//See if we have one of our custom types that we need to handle
			if (entryValue.GetType() == typeof(DateRange))
			{
				typeCode = (int)Common.Global.CustomTypeCodes.DateRange;
				return ((DateRange)entryValue).ToString();
			}
			if (entryValue.GetType() == typeof(DecimalRange))
			{
				typeCode = (int)Common.Global.CustomTypeCodes.DecimalRange;
				return ((DecimalRange)entryValue).ToString();
			}
			if (entryValue.GetType() == typeof(EffortRange))
			{
				typeCode = (int)Common.Global.CustomTypeCodes.EffortRange;
				return ((EffortRange)entryValue).ToString();
			}
			if (entryValue.GetType() == typeof(IntRange))
			{
				typeCode = (int)Common.Global.CustomTypeCodes.IntRange;
				return ((IntRange)entryValue).ToString();
			}
			if (entryValue.GetType() == typeof(MultiValueFilter))
			{
				typeCode = (int)Common.Global.CustomTypeCodes.MultiValueFilter;
				return ((MultiValueFilter)entryValue).ToString();
			}

			//Now handle the built-in typecodes

			//First we need to get the typecode of the object
			typeCode = (int)System.Type.GetTypeCode(entryValue.GetType());

			//Next we need to convert the value to string. Need to handle date-time differently
			if (typeCode == (int)TypeCode.DateTime)
			{
				return ((DateTime)entryValue).ToString("yyyyMMddTHHmmss");
			}
			else
			{
				return entryValue.ToString();
			}
		}

		/// <summary>
		/// Converts a string representation into the native object
		/// </summary>
		/// <param name="entryValue">The string version of the data</param>
		/// <param name="typeCode">The type of the object we want</param>
		/// <returns>The data in its native form</returns>
		protected object DeSerializeValue(string entryValue, int typeCode)
		{
			//First test the custom typecodes
			if (typeCode == (int)Common.Global.CustomTypeCodes.DateRange)
			{
				DateRange dateRange;
				DateRange.TryParse(entryValue, out dateRange);
				return dateRange;
			}
			if (typeCode == (int)Common.Global.CustomTypeCodes.DecimalRange)
			{
				DecimalRange decimalRange;
				DecimalRange.TryParse(entryValue, out decimalRange);
				return decimalRange;
			}
			if (typeCode == (int)Common.Global.CustomTypeCodes.EffortRange)
			{
				EffortRange effortRange;
				EffortRange.TryParse(entryValue, out effortRange);
				return effortRange;
			}
			if (typeCode == (int)Common.Global.CustomTypeCodes.IntRange)
			{
				IntRange intRange;
				IntRange.TryParse(entryValue, out intRange);
				return intRange;
			}
			if (typeCode == (int)Common.Global.CustomTypeCodes.MultiValueFilter)
			{
				MultiValueFilter multiValueFilter;
				MultiValueFilter.TryParse(entryValue, out multiValueFilter);
				return multiValueFilter;
			}

			//Now the built-in typecodes
			if (typeCode == (int)TypeCode.Boolean)
			{
				return bool.Parse(entryValue);
			}
			else if (typeCode == (int)TypeCode.Int32)
			{
				return int.Parse(entryValue);
			}
			else if (typeCode == (int)TypeCode.Int16)
			{
				return short.Parse(entryValue);
			}
			else if (typeCode == (int)TypeCode.Int64)
			{
				return long.Parse(entryValue);
			}
			else if (typeCode == (int)TypeCode.DateTime)
			{
				return DateTime.ParseExact(entryValue, "yyyyMMddTHHmmss", System.Globalization.CultureInfo.InvariantCulture);
			}
			else
			{
				//Keep as a string value
				return entryValue;
			}

		}

		/// <summary>
		/// Loads the hashtable entries from the database
		/// </summary>
		public void Restore()
		{
			Business.UserManager user = new Business.UserManager();
			UserCollection userCollection = user.RetrieveSettings(this.userId, this.collectionName);

			//Now populate the hashtable
			this.Clear();
			foreach (UserCollectionEntry userCollectionEntry in userCollection.Entries)
			{
				string entryKey = userCollectionEntry.EntryKey;
				string entryValue = userCollectionEntry.EntryValue;
				int typeCode = userCollectionEntry.EntryTypeCode;
				this.Add(entryKey, DeSerializeValue(entryValue, typeCode));
			}
		}
	}
}
