using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Projects, Roles, Permissions, etc. in the system
	/// </summary>
	public class ProjectManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ProjectManager::";

		protected static SortedList<int, string> allocationFiltersList;

		//The built-in project roles. They are not allowed to be deleted
		//since they are added to new project workflows
		//They should not be referenced outside of this class since the roles are now customizable
		//except for the project owner as it's needed when creating a new project via. the SOAP API
		public const int ProjectRoleProjectOwner = 1;
		public const int ProjectRoleManager = 2;
		public const int ProjectRoleDeveloper = 3;
		public const int ProjectRoleTester = 4;
		public const int ProjectRoleObserver = 5;
		public const int ProjectRoleIncidentUser = 6;

		//This is used to associate the demo login with the appropriate sample project and built-in role
		public const int ProjectIdSampleProject = 1;
		public const int ProjectRoleIdForDemoAccount = 2;   //Manager

		public const int DEFAULT_POINT_EFFORT = 480;    //8 hours per story point (480 minutes)

		//Object lock..
		internal static object lockObj = new object();

		//This is used to hold a single instance of the list of permissions for all roles
		static List<ProjectRole> authorizedProjectRoles;

		/// <summary>Constructor method for class. Sets up the datasets and table mappings</summary>
		public ProjectManager()
			: base()
		{
			const string METHOD_NAME = "ctor";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Load the list of project role permissions into a static list if not already set
			if (authorizedProjectRoles == null)
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Loading Roles and Permissions into memory...");
				authorizedProjectRoles = RetrieveRolePermissions();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Public accessor for the list of authorized project roles
		/// </summary>
		public List<ProjectRole> AuthorizedProjectRoles
		{
			get
			{
				return authorizedProjectRoles;
			}
		}

		#region Project and Project Membership Methods

		/// <summary>Determines if the current user is authorized to view source code</summary>
		/// <param name="projectRoleId">The current role</param>
		/// <returns>True if authorized</returns>
		public bool IsAuthorizedToViewSourceCode(int projectRoleId)
		{
			ProjectRole projectRole = authorizedProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId);
			if (projectRole != null)
			{
				//See if we can view source code
				if (projectRole.IsSourceCodeView)
				{
					return (projectRole.IsLimitedView) ? false : true;
				}
			}
			return false;
		}

		/// <summary>Determines if the current user is authorized add comments to artifacts.</summary>
		/// <param name="projectRoleId">The current role</param>
		/// <returns>True if authorised</returns>
		public bool IsAuthorizedToAddComment(int? projectRoleId)
		{
			bool retValue = false;
			if (projectRoleId.HasValue)
			{
				ProjectRole projectRole = authorizedProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId.Value);
				if (projectRole != null)
					retValue = projectRole.IsDiscussionsAdd;
			}

			return retValue;
		}

		/// <summary>
		/// Updates a list of project user membership
		/// </summary>
		/// <param name="projectUsers">The list to be persisted</param>
		/// <remarks>Prevents deletes of the system admin user from any project</remarks>
		public void UpdateMembership(List<ProjectUser> projectUsers, int? userId = null)
		{
			const string METHOD_NAME = "UpdateMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Apply changes and update
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (ProjectUser projectUser in projectUsers)
					{
						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

						string adminSectionName = "Users";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;
						//Ignore attempts to delete/change entries for the system administrator user
						//Since they must be project owner for all projects
						if (projectUser.UserId == User.UserSystemAdministrator)
						{
							projectUser.ProjectRoleId = ProjectRoleProjectOwner;
							if (projectUser.ChangeTracker != null && projectUser.ChangeTracker.State == ObjectState.Deleted)
							{
								projectUser.MarkAsUnchanged();
							}
						}
						else
						{
							context.ProjectUsers.ApplyChanges(projectUser);
						}
						context.UserSaveChanges(userId, projectUser.UserId, adminSectionId, "Updated Project Membership", true, true, null);
					}
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

		/// <summary>Retrieves a list of artifact permissions set for a particular role. It also retrieves the project role record as well</summary>
		/// <param name="projectRoleId">The role we're interested in</param>
		/// <returns>List of project role permissions</returns>
		public ProjectRole RetrieveRolePermissions(int projectRoleId)
		{
			const string METHOD_NAME = "RetrieveRolePermissions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectRole projectRole;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project role and associated permissions
					var query = from p in context.ProjectRoles.Include("RolePermissions")
								where p.ProjectRoleId == projectRoleId
								select p;

					projectRole = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRole;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a project role record for a particular role</summary>
		/// <param name="projectRoleId">The role we're interested in</param>
		/// <returns>project role record</returns>
		public ProjectRole RetrieveRoleById(int projectRoleId)
		{
			const string METHOD_NAME = "RetrieveRoleById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectRole projectRole;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project role and associated permissions
					var query = from p in context.ProjectRoles
								where p.ProjectRoleId == projectRoleId
								select p;

					projectRole = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRole;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}



		/// <summary>
		/// Deletes a project from the system including all artifacts and configuration data
		/// </summary>
		/// <param name="userId">The user we're performing the operation as</param>
		/// <param name="projectId">The project to be deleted</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <remarks>
		/// Does not delete the associated template
		/// </remarks>
		public void Delete(int userId, int projectId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Deleting project PR" + projectId);

				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(0, GlobalResources.Messages.Project_Delete_DataMappings);

				//Now delete any custom property mapping records
				Project project;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Make sure the project exists
					var query = from p in context.Projects
								where p.ProjectId == projectId
								select p;

					project = query.FirstOrDefault();
					if (project == null)
					{
						//Already deleted
						return;
					}

					context.Project_DeleteDataMappings(projectId, userId);
				}

				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(5, GlobalResources.Messages.Project_Delete_Attachments);

				//Next we need to delete all the attachments/documents associated with the project
				//That means that we won't need to do it for each of the individual artifacts
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.DeleteAllProjectAttachmentInfo(projectId, userId);

				//Next we need to delete all the incidents associated with the project,
				//including resolutions and associations to/from, any any placeholder artifacts
				//These are non cascadable
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteIncidents(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(15, GlobalResources.Messages.Project_Delete_Associations);

				//Delete any associations to other projects
				ProjectAssociation_DeleteAll(projectId);

				//Delete any baselines we have.
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					var baselines = ct.ProjectBaselines.Where(b => b.ProjectId == projectId).ToList();
					foreach (var baseline in baselines)
						ct.ProjectBaselines.DeleteObject(baseline);
					ct.SaveChanges();
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(87, GlobalResources.Messages.Project_Delete_Project);

				//Next we need to delete all the artifact links
				//including resolutions and associations to/from
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteAssociations(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(25, GlobalResources.Messages.Project_Delete_TestRuns);

				//Next we need to delete all the test runs associated with the project
				//including all test run steps and user navigation data as well as any pending entries
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteTestRuns(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(35, GlobalResources.Messages.Project_Delete_TestSets);

				//Next we need to delete all the test sets associated with the project
				//including all mapped test cases and user navigation data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteTestSets(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(45, GlobalResources.Messages.Project_Delete_TestCases);

				//Next we need to delete all the test cases associated with the project
				//including test steps, parameters, release/req mappings and all user navigation
				//The association between test steps and incidents will already have been handled by the
				//incident delete section
				//Now we need to delete all the project task folders and tasks
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteTestCasesTasks(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(55, GlobalResources.Messages.Project_Delete_Requirements);

				//Next we need to delete all the requirements associated with the project
				//together with any associations and test case mappings
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteRequirements(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(60, GlobalResources.Messages.Project_Delete_Risks);

				//Next we need to delete all the risks associated with the project
				//together with any mitigations
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteRisks(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(65, GlobalResources.Messages.Project_Delete_Releases);

				//Next we need to delete all the releases associated with the project
				//together with any test case mappings and automation hosts
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteReleasesAutomation(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(70, GlobalResources.Messages.Project_Delete_Releases);

				//Now we need to delete all the user/project custom property settings
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteCustomProperties(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(75, GlobalResources.Messages.Project_Delete_Releases);

				//The template is not automatically deleted by the project delete, needs to be done separately

				//No need to delete notifications, they are set to cascade deletes from the Project.

				//Now we need to delete any saved reports
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_DeleteReports(projectId);
				}
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(85, GlobalResources.Messages.Project_Delete_Project);

				//Delete any Taravault Users and the project.
				new VaultManager().Project_DeleteTaraVault(projectId);
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(90, GlobalResources.Messages.Project_Delete_Project);

				//Log a deletion into the history table.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Project, projectId, DateTime.UtcNow);
				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(95, GlobalResources.Messages.Project_Delete_Project);

				//Next, delete the project itself (and unset the last opened project for any users)
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Project_Delete(projectId);

					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
					string adminSectionName = "View / Edit Projects";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;

					//Add a changeset to mark it as deleted.
					new AdminAuditManager().LogDeletion1(userId, projectId, project.Name, adminSectionId, "Project Deleted", DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");
				}

				//Update the deleted project's completion metrics to program and portfolio
				new ProjectGroupManager().RefreshRequirementCompletion(project.ProjectGroupId);

				if (updateBackgroundProcessStatus != null)
					updateBackgroundProcessStatus(100, "");
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw new Exception("Cannot Delete Project", exception);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Determines if a particular project role is allowed to do something and whether it is limited by the id of the artifact or not</summary>
		/// <param name="projectRoleId">The role in question</param>
		/// <param name="artifactType">The type of artifact being acted upon</param>
		/// <param name="permission">The type of action being taken</param>
		/// <returns>One of the possible authorized states</returns>
		/// <remarks>1) The System Admin case is not handled here, it needs to be handled by the calling function
		/// 2) The product license will override the project settings (e.g. test cases always disabled for SpiraPlan)</remarks>
		public Project.AuthorizationState IsAuthorized(int projectRoleId, DataModel.Artifact.ArtifactTypeEnum artifactType, Project.PermissionEnum permission)
		{
			//Handle the special case of the 'None' permission
			if (permission == Project.PermissionEnum.None)
			{
				return Project.AuthorizationState.Authorized;
			}

			Project.AuthorizationState authorized = Project.AuthorizationState.Prohibited;
			//Handle the special case of the project administrator permission
			ProjectRole projectRole = authorizedProjectRoles.FirstOrDefault(p => p.ProjectRoleId == projectRoleId);
			if (permission == Project.PermissionEnum.ProjectAdmin)
			{
				//See if we have an active record for this role
				if (projectRole != null)
				{
					//If the role is a project admin, then we're authorized for all items
					if (projectRole.IsAdmin)
					{
						authorized = Project.AuthorizationState.Authorized;
					}
				}
			}
			else
			{
				//If the artifact type is set to None, but the permission is set to View,
				//need to determine if the user only has limited permissions
				if (permission == Project.PermissionEnum.View && artifactType == Artifact.ArtifactTypeEnum.None)
				{
					if (projectRole == null)
					{
						return Project.AuthorizationState.Prohibited;
					}
					return (projectRole.IsLimitedView) ? Project.AuthorizationState.Limited : Project.AuthorizationState.Authorized;
				}

				//See if we have a record for this permission, role and artifact type
				if (projectRole != null)
				{
					ProjectRolePermission projectRolePermission = projectRole.RolePermissions.FirstOrDefault(p => p.ArtifactTypeId == (int)artifactType && p.PermissionId == (int)permission);
					if (projectRolePermission == null)
					{
						//See if we have the special case of a user who can only edit their own items
						if (permission == Project.PermissionEnum.Modify && projectRole.RolePermissions.Any(p => p.ArtifactTypeId == (int)artifactType && p.PermissionId == (int)Project.PermissionEnum.LimitedModify))
						{
							authorized = Project.AuthorizationState.Limited;
						}
					}
					else
					{
						//See if we have the special case of a user who can only view their own items
						if (permission == Project.PermissionEnum.View && projectRole.IsLimitedView)
						{
							authorized = Project.AuthorizationState.Limited;
						}
						else
						{
							authorized = Project.AuthorizationState.Authorized;
						}
					}
				}
			}

			//Handle the special case where we are using a product license that doesn't support certain artifacts
			if (!ArtifactManager.IsSupportedByLicense(artifactType))
			{
				authorized = Project.AuthorizationState.Prohibited;
			}

			return authorized;
		}

		/// <summary>Retrieves a list of all artifact permissions set for all roles in the system. It also retrieves all the active project role records</summary>
		/// <returns>List of project roles with associated permissions</returns>
		public List<ProjectRole> RetrieveRolePermissions()
		{
			const string METHOD_NAME = "RetrieveRolePermissions";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectRole> projectRoles;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get all the active project roles with the associated role permissions
					var query = from p in context.ProjectRoles.Include("RolePermissions")
								where p.IsActive
								orderby p.ProjectRoleId
								select p;

					projectRoles = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRoles;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of either active or all project roles for use in lookups</summary>
		/// <param name="activeOnly">Return all roles or just active ones</param>
		/// <returns>List of project roles</returns>
		public List<ProjectRole> RetrieveProjectRoles(bool activeOnly, bool includePermissions = false)
		{
			const string METHOD_NAME = "RetrieveProjectRoles";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectRole> projectRoles;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					ObjectQuery<ProjectRole> projectRolesSet = context.ProjectRoles;

					//See if we need to get the associated permission records
					if (includePermissions)
					{
						projectRolesSet = projectRolesSet.Include("RolePermissions");
					}

					var query = from p in projectRolesSet
								select p;

					//Add the active filter
					if (activeOnly)
					{
						query = query.Where(p => p.IsActive);
					}

					//Add the sort
					query = query.OrderBy(p => p.ProjectRoleId);

					//Actually execute the query and return the list
					projectRoles = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRoles;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ProjectRole> RetrieveProjectRoles1(bool activeOnly, int pageIndex, int pageSize, out int totalRecords, bool includePermissions = false)
		{
			const string METHOD_NAME = "RetrieveProjectRoles";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectRole> projectRoles;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					ObjectQuery<ProjectRole> projectRolesSet = context.ProjectRoles;

					//See if we need to get the associated permission records
					if (includePermissions)
					{
						projectRolesSet = projectRolesSet.Include("RolePermissions");
					}

					var query = from p in projectRolesSet
								select p;

					//Add the active filter
					if (activeOnly)
					{
						query = query.Where(p => p.IsActive);
					}

					//Add the sort
					query = query.OrderBy(p => p.ProjectRoleId);

					//Actually execute the query and return the list
					//projectRoles = query.ToList();

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
					projectRoles = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRoles;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Deletes a specific project role record
		/// </summary>
		/// <param name="projectRoleId">The id of the project role being deleted</param>
		/// <remarks>
		/// 1) You cannot delete any of the six built-in roles
		/// 2) This will delete any associated permissions, workflow transition roles or project membership
		/// </remarks>
		public void DeleteProjectRole(int projectRoleId, int? userId = null)
		{
			const string METHOD_NAME = "DeleteProjectRole";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If the project role is one of the built-in ones, throw a business exception
				if (projectRoleId == ProjectManager.ProjectRoleProjectOwner ||
					projectRoleId == ProjectManager.ProjectRoleManager ||
					projectRoleId == ProjectManager.ProjectRoleDeveloper ||
					projectRoleId == ProjectManager.ProjectRoleTester ||
					projectRoleId == ProjectManager.ProjectRoleObserver ||
					projectRoleId == ProjectManager.ProjectRoleIncidentUser)
				{
					throw new ProjectRoleNotDeletableException("You cannot delete one of the default project roles");
				}

				//Perform the delete using the stored procedure
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query1 = from p in context.ProjectRoles
								 where p.ProjectRoleId == projectRoleId
								 select p;

					ProjectRole projectRole = query1.FirstOrDefault();

					context.Project_DeleteProjectRole(projectRoleId);

					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
					string adminSectionName = "View / Edit Project Roles";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;
					if(projectRole!=null)
					//Add a changeset to mark it as deleted.
					  new AdminAuditManager().LogDeletion1((int)userId, projectRole.ProjectRoleId, projectRole.Name, adminSectionId, "ProjectRole Deleted", DateTime.UtcNow, ArtifactTypeEnum.ProjectRole, "ProjectRoleId");
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
		/// Deletes the project memebership for a specific user and project combination
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user</param>
		public void DeleteUserMembership(int userId, int projectId, int? loggedInUserId)
		{
			const string METHOD_NAME = "DeleteUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First retrieve the items
					var query = from p in context.ProjectUsers
								where p.ProjectId == projectId && p.UserId == userId && p.UserId != UserManager.UserSystemAdministrator
								select p;

					List<ProjectUser> projectUsers = query.ToList();
					foreach (ProjectUser projectUser in projectUsers)
					{
						context.DeleteObject(projectUser);

						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "Users";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)loggedInUserId, projectUser.ProjectId, projectUser.ProjectName, adminSectionId, "User Membership Deleted", DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");
					}

					//Commit the delete
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

		/// <summary>
		/// Inserts a new user membership record for the project
		/// </summary>
		/// <param name="userId">The user we're adding to the project as a member</param>
		/// <param name="projectId">The project we're adding the member to</param>
		/// <param name="projectRoleId">The role the person is being added as</param>
		public void InsertUserMembership(int userId, int projectId, int projectRoleId, int? loggedUserId = null)
		{
			const string METHOD_NAME = "InsertUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new user membership record
					ProjectUser projectUser = new ProjectUser();
					projectUser.ProjectId = projectId;
					projectUser.UserId = userId;
					projectUser.ProjectRoleId = projectRoleId;
					context.ProjectUsers.AddObject(projectUser);

					ProjectManager pm = new ProjectManager();
					var projectData = pm.RetrieveProjectById(projectId);

					//Commit the insert
					context.SaveChanges();

					Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();

					string adminSectionName = "Users";
					var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

					int adminSectionId = adminSection.ADMIN_SECTION_ID;

					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = projectData.Name;

					adminAuditManager.LogCreation1(Convert.ToInt32(loggedUserId), Convert.ToInt32(adminSectionId), projectRoleId, "User membership added ", details, DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");
					//context.UserSaveChanges(userId, projectUser.UserId, adminSectionId, "Inserted Project Membership", true, true, null);
				}
			}
			catch (EntityConstraintViolationException)
			{
				//If we have a unique constraint violation, throw a business exception
				throw new ProjectDuplicateMembershipRecordException("That project membership row already exists!");
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
		/// Inserts a new project role into the system
		/// </summary>
		/// <param name="name">The name of the role</param>
		/// <param name="description">The description of the role</param>
		/// <param name="admin">Does this role have project administration rights?</param>
		/// <param name="active">Is this role active</param>
		/// <param name="discussionsAdd">Can this role post discussion comments</param>
		/// <param name="documentFoldersEditXXX">Can this role add/edit/delete document folders (not used any more)</param>
		/// <param name="sourceCodeEdit">Can this role edit source code associations</param>
		/// <param name="sourceCodeView">Can this role view the linked source code repository</param>
		/// <param name="limitedView">Does this role have a limited view of the project</param>
		/// <returns>The id of the newly inserted project role</returns>
		/// <remarks>
		/// documentFoldersEditXXX has been replaced by the Document artifact's main permissions
		/// </remarks>
		public int InsertProjectRole(string name, string description, bool admin, bool active, bool discussionsAdd, bool sourceCodeView, bool sourceCodeEdit, bool documentFoldersEditXXX, bool limitedView, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "InsertProjectRole";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				int projectRoleId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Populate the new object
					ProjectRole projectRole = new ProjectRole();
					projectRole.Name = name;
					projectRole.Description = description;
					projectRole.IsAdmin = admin;
					projectRole.IsActive = active;
					projectRole.IsDiscussionsAdd = discussionsAdd;
					projectRole.IsSourceCodeView = sourceCodeView;
					projectRole.IsSourceCodeEdit = sourceCodeEdit;
					projectRole.IsLimitedView = limitedView;

					//Actually perform the insert into the ProjectRole table and capture the new id created
					context.ProjectRoles.AddObject(projectRole);
					context.SaveChanges();
					projectRoleId = projectRole.ProjectRoleId;
					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = projectRole.Name;
					//Log history.
					if (logHistory)
						adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), projectRoleId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRoleId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a project roles
		/// </summary>
		/// <param name="projectRoles">The project role list</param>
		/// <remarks>
		/// 1) This updates the permissions, as well as the role
		/// 2) You cannot make the project owner role inactive
		/// </remarks>
		public void UpdateProjectRole(List<ProjectRole> projectRoles, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "UpdateProjectRole";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we're not making the project owner role inactive
				foreach (ProjectRole projectRole in projectRoles)
				{
					if (!projectRole.IsActive && projectRole.ProjectRoleId == ProjectManager.ProjectRoleProjectOwner)
					{
						throw new ProjectRoleNotDeactivatableException("You cannot make the project owner role inactive");
					}
				}

				//Apply the changes
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (ProjectRole projectRole in projectRoles)
					{
						context.ProjectRoles.ApplyChanges(projectRole);
						context.AdminSaveChanges(userId, projectRole.ProjectRoleId, null, adminSectionId, action, true, true, null);
					}
					context.SaveChanges();
				}

				//Now refresh the cached set of permissions
				authorizedProjectRoles = RetrieveRolePermissions();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves an list of the various permissions</summary>
		/// <returns>List of all active permissions</returns>
		public List<Permission> RetrievePermissions()
		{
			const string METHOD_NAME = "RetrievePermissions";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Permission> permissions;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.Permissions
								where p.IsActive
								orderby p.PermissionId
								select p;

					permissions = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return permissions;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public Permission RetrievePermissionById(int permissionId)
		{
			const string METHOD_NAME = "RetrievePermissionById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Permission permission;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.Permissions
								where p.PermissionId == permissionId
								select p;

					permission = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return permission;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ProjectRole RetrieveProjectRoleById(int projectRoleId)
		{
			const string METHOD_NAME = "RetrieveProjectById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectRole projectRole;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.ProjectRoles
								where p.ProjectRoleId == projectRoleId
								select p;

					projectRole = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectRole;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public Project RetrieveProjectById(int projectId)
		{
			const string METHOD_NAME = "RetrieveProjectById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Project project;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.Projects
								where p.ProjectId == projectId
								select p;

					project = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return project;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public User RetrieveUserById(int userId)
		{
			const string METHOD_NAME = "RetrieveUserById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				User user;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.Users
								where p.UserId == userId
								select p;

					user = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return user;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ProjectGroup RetrieveProjectGroupById(int projectGroupId)
		{
			const string METHOD_NAME = "RetrieveProjectGroupById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectGroup projectGroup;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.ProjectGroups
								where p.ProjectGroupId == projectGroupId
								select p;

					projectGroup = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroup;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the user membership records by project id</summary>
		/// <param name="projectId">The ID of the project to retrieve the users for</param>
		/// <param name="includeInactive">should we include inactive</param>
		/// <returns>A project user list, sorted by user's name</returns>
		/// <remarks>This method is used for administering membership, so group membership is not considered in this method</remarks>
		public List<ProjectUser> RetrieveUserMembershipById(int projectId, bool includeInactive = true)
		{
			const string METHOD_NAME = "RetrieveUserMembershipById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectUser> projectUsers;

				//Create select command for retrieving the project record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectUsers.Include("User").Include("User.Profile").Include("Role").Include("Project")
								where p.ProjectId == projectId && p.User.IsApproved
								select p;

					//Add the inactive filter (if specified)
					if (!includeInactive)
					{
						query = query.Where(p => p.User.IsActive);
					}

					//Add the sort
					query = query.OrderBy(p => p.UserId).ThenBy(p => p.ProjectRoleId);

					//Execute the query
					projectUsers = query.ToList();
				}

				//Sort by name (in memory)
				projectUsers = projectUsers.OrderBy(p => p.FullName).ThenBy(p => p.UserId).ToList();

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectUsers;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all the artifacts that a user can view by project</summary>
		/// <param name="userId">The ID of the user to retrieve the project artifact view permissions for</param>
		/// <returns>A  list of projects and artifacts</returns>
		/// <remarks>Includes project group membership</remarks>
		public List<ProjectViewPermission> RetrieveProjectViewPermissionsForUser(int userId)
		{
			const string METHOD_NAME = "RetrieveProjectViewPermissionsForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectViewPermission> projectViewPermissions;

				//Retrieve the data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					projectViewPermissions = context.Project_RetrieveViewPermissions(userId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the data
				return projectViewPermissions;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the user membership records by user id</summary>
		/// <param name="userId">The ID of the user to retrieve the project membership for</param>
		/// <returns>A project user list</returns>
		/// <remarks>This method does NOT consider Group membership</remarks>
		public List<ProjectUser> RetrieveProjectMembershipForUser(int userId)
		{
			const string METHOD_NAME = "RetrieveProjectMembershipForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectUser> projectUsers;

				//Create select command for retrieving the project record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectUsers.Include("Project")
								where p.UserId == userId && p.Role.IsActive
								select p;

					projectUsers = query.ToList();
					projectUsers = projectUsers.OrderBy(p => p.ProjectName).ThenBy(p => p.ProjectId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectUsers;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the user membership records by user id</summary>
		/// <param name="userId">The ID of the user to retrieve the project membership for</param>
		/// <returns>A project user list</returns>
		/// <remarks>This method DOES consider Group membership</remarks>
		public List<ProjectUserView> RetrieveProjectMembershipForUserIncludingGroupMembership(int userId)
		{
			const string METHOD_NAME = "RetrieveProjectMembershipForUserIncludingGroupMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectUserView> projectUsers;

				//Create select command for retrieving the project record
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectUsersView
								where p.UserId == userId && p.IsActive
								orderby p.ProjectName, p.ProjectId
								select p;

					projectUsers = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectUsers;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a user membership record by project id and user id
		/// </summary>
		/// <param name="userId">The ID of the user we're interested in</param>
		/// <param name="projectId">The ID of the project we're interested in</param>
		/// <returns>A project user record</returns>
		/// <remarks>
		/// If the user is a member of a project group, they will have at least the observer role
		/// in all the contained projects.
		/// </remarks>
		public ProjectUserView RetrieveUserMembershipById(int projectId, int userId)
		{
			const string METHOD_NAME = "RetrieveUserMembershipById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectUserView projectUser;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectUsersView
								where p.ProjectId == projectId && p.UserId == userId
								select p;

					projectUser = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the record
				return projectUser;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Recent Projects

		/// <summary>
		/// Adds/updates a project to the list of recently accessed ones
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectId">The id of the project</param>
		public void AddUpdateRecentProject(int userId, int projectId)
		{
			const string METHOD_NAME = "AddUpdateRecentProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we already have this project tracked
					var query = from p in context.UserRecentProjects
								where p.UserId == userId && p.ProjectId == projectId
								select p;

					UserRecentProject userRecentProject = query.FirstOrDefault();
					if (userRecentProject == null)
					{
						//Add this entry
						userRecentProject = new UserRecentProject();
						userRecentProject.UserId = userId;
						userRecentProject.ProjectId = projectId;
						userRecentProject.LastAccessDate = DateTime.UtcNow;
						context.UserRecentProjects.AddObject(userRecentProject);
					}
					else
					{
						//Update the date
						userRecentProject.StartTracking();
						userRecentProject.LastAccessDate = DateTime.UtcNow;
					}

					//Save changes
					context.SaveChanges();
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
		/// Retrieves a list of recently accessed projects for a user
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="rowsToDisplay">The number of rows to return</param>
		/// <param name="authorizedProjects">The list of authorized projects</param>
		/// <returns>The list of recent projects</returns>
		public List<UserRecentProject> RetrieveRecentProjectsForUser(int userId, int rowsToDisplay, List<int> authorizedProjects = null)
		{
			const string METHOD_NAME = "RetrieveRecentProjectsForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<UserRecentProject> recentProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the list of projects for a user
					var query = from p in context.UserRecentProjects
									.Include(p => p.Project)
									.Include(p => p.Project.Group)
									.Include(p => p.Project.Group.Portfolio)
								where p.UserId == userId
								select p;

					//Filter by authorized project (if specified)
					if (authorizedProjects != null)
					{
						query = query.Where(p => authorizedProjects.Contains(p.ProjectId));
					}

					//Sort by date (descending)
					query = query.OrderByDescending(p => p.LastAccessDate);

					//Add the row limit
					query = query.Take(rowsToDisplay);

					//Return the list
					recentProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return recentProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<RecentProjectDetails> RetrieveRecentProjectsForUser1(int userId, int rowsToDisplay, List<int> authorizedProjects = null)
		{
			const string METHOD_NAME = "RetrieveRecentProjectsForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RecentProjectDetails> recentProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the list of projects for a user
					var query = from p in context.UserRecentProjects
								join d in context.Projects
								on p.ProjectId equals d.ProjectId
								join g in context.ProjectGroups
								on d.ProjectGroupId equals g.ProjectGroupId
								where p.UserId == userId
								select new RecentProjectDetails
								{
									ProjectId = p.ProjectId,	
									ProjectName = d.Name,
									ProjectGroup = g.Name,
									CreationDate = d.CreationDate,
									LastActivityDate = p.LastAccessDate
								};

					//Filter by authorized project (if specified)
					if (authorizedProjects != null)
					{
						query = query.Where(p => authorizedProjects.Contains(p.ProjectId));
					}

					//Sort by date (descending)
					query = query.OrderByDescending(p => p.LastActivityDate);

					//Add the row limit
					query = query.Take(rowsToDisplay);

					//Return the list
					recentProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return recentProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		#endregion

		#region Project Association Methods

		/// <summary>
		/// Adds a new project association
		/// </summary>
		/// <param name="sourceProjectId">The source project</param>
		/// <param name="destProjectId">The project being shared with</param>
		/// <param name="artifactTypeIds">The list of artifact types</param>
		public void ProjectAssociation_Add(int sourceProjectId, int destProjectId, List<int> artifactTypeIds)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Add the items
					foreach (int artifactTypeId in artifactTypeIds)
					{
						ProjectArtifactSharing projectArtifactSharing = new ProjectArtifactSharing();
						projectArtifactSharing.SourceProjectId = sourceProjectId;
						projectArtifactSharing.DestProjectId = destProjectId;
						projectArtifactSharing.ArtifactTypeId = artifactTypeId;
						context.ProjectArtifactSharings.AddObject(projectArtifactSharing);
					}
					context.SaveChanges();
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
		/// Updates an existing project association
		/// </summary>
		/// <param name="sourceProjectId">The source project</param>
		/// <param name="destProjectId">The project being shared with</param>
		/// <param name="artifactTypeIds">The list of artifact types</param>
		public void ProjectAssociation_Update(int sourceProjectId, int destProjectId, List<int> artifactTypeIds)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the existing list of artifacts shared
					var query = from p in context.ProjectArtifactSharings
								where p.SourceProjectId == sourceProjectId && p.DestProjectId == destProjectId
								select p;

					List<ProjectArtifactSharing> existingAssociations = query.ToList();
					foreach (ProjectArtifactSharing existingAssociation in existingAssociations)
					{
						//See if we have this one in the list, if not delete
						if (!artifactTypeIds.Contains(existingAssociation.ArtifactTypeId))
						{
							context.ProjectArtifactSharings.DeleteObject(existingAssociation);
						}
					}

					//Now add any missing ones
					foreach (int artifactTypeId in artifactTypeIds)
					{
						if (!existingAssociations.Any(p => p.ArtifactTypeId == artifactTypeId))
						{
							ProjectArtifactSharing projectArtifactSharing = new ProjectArtifactSharing();
							projectArtifactSharing.SourceProjectId = sourceProjectId;
							projectArtifactSharing.DestProjectId = destProjectId;
							projectArtifactSharing.ArtifactTypeId = artifactTypeId;
							context.ProjectArtifactSharings.AddObject(projectArtifactSharing);
						}
					}

					//Save the changes
					context.SaveChanges();
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
		/// Deletes all instances of project associations to/from the specified project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		protected internal void ProjectAssociation_DeleteAll(int projectId)
		{
			const string METHOD_NAME = "ProjectAssociation_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Find the items to delete
					var query = from p in context.ProjectArtifactSharings
								where p.SourceProjectId == projectId || p.DestProjectId == projectId
								select p;

					List<ProjectArtifactSharing> associationsToDelete = query.ToList();
					foreach (ProjectArtifactSharing associationToDelete in associationsToDelete)
					{
						context.ProjectArtifactSharings.DeleteObject(associationToDelete);
					}
					context.SaveChanges();
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
		/// Deletes the sharing between two projects (all artifacts)
		/// </summary>
		/// <param name="sourceProjectId">The ID of the source project</param>
		/// <param name="destProjectId">The ID of the destination project</param>
		public void ProjectAssociation_Delete(int sourceProjectId, int destProjectId)
		{
			const string METHOD_NAME = "ProjectAssociation_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Find the items to delete
					var query = from p in context.ProjectArtifactSharings
								where p.SourceProjectId == sourceProjectId && p.DestProjectId == destProjectId
								select p;

					List<ProjectArtifactSharing> associationsToDelete = query.ToList();
					foreach (ProjectArtifactSharing associationToDelete in associationsToDelete)
					{
						context.ProjectArtifactSharings.DeleteObject(associationToDelete);
					}
					context.SaveChanges();
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
		/// Can the source project share with the specified (destination) one
		/// </summary>
		/// <param name="destProjectId">The project being shared with</param>
		/// <param name="sourceProjectId">The project doing the sharing</param>
		/// <param name="artifactType">The type of artifact</param>
		/// <returns></returns>
		public bool ProjectAssociation_CanProjectShare(int destProjectId, int sourceProjectId, Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ProjectAssociation_CanProjectShare";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If the two projects are the same, they can share
				if (sourceProjectId == destProjectId)
				{
					return true;
				}

				bool canShare;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
								where
									p.DestProjectId == destProjectId &&
									p.ArtifactTypeId == (int)artifactType &&
									p.SourceProjectId == sourceProjectId
								select p;

					canShare = query.Any();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return canShare;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects that the current project can include
		/// </summary>
		/// <param name="destProjectId">The id of the project that needs the shared artifacts</param>
		/// <returns>The list of projects</returns>
		public List<ProjectArtifactSharing> ProjectAssociation_RetrieveForDestProjectAndArtifact(int destProjectId, Artifact.ArtifactTypeEnum artifactType)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForDestProjectAndArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectArtifactSharing> sharedProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
									.Include(p => p.SourceProject)
								where p.DestProjectId == destProjectId && p.ArtifactTypeId == (int)artifactType
								orderby p.SourceProject.Name, p.SourceProjectId
								select p;

					sharedProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return sharedProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects that the current project can include
		/// </summary>
		/// <param name="artifactTypeIds">The types of artifact that we want to consider</param>
		/// <param name="destProjectId">The id of the project that needs the shared artifacts</param>
		/// <returns>The list of projects</returns>
		public List<ProjectArtifactSharing> ProjectAssociation_RetrieveForDestProjectAndArtifacts(int destProjectId, List<int> artifactTypeIds)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForDestProjectAndArtifact";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectArtifactSharing> sharedProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
									.Include(p => p.SourceProject)
									.Include(p => p.ArtifactType)
								where p.DestProjectId == destProjectId && artifactTypeIds.Contains(p.ArtifactTypeId)
								orderby p.SourceProject.Name, p.SourceProjectId
								select p;

					sharedProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return sharedProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects that the current project is being used in lists for
		/// </summary>
		/// <returns>The list of projects</returns>
		public List<Project> ProjectAssociation_RetrieveForSourceProject(int sourceProjectId)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForSourceProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Project> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
									.Include(p => p.DestProject)
								where p.SourceProjectId == sourceProjectId
								orderby p.DestProject.Name, p.DestProjectId
								select p.DestProject;

					projects = query.Distinct().ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects that the current project can have in its lists, including itself
		/// </summary>
		/// <returns>The list of projects</returns>
		public List<Project> ProjectAssociation_RetrieveForDestProjectIncludingSelf(int destProjectId)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForDestProjectIncludingSelf";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Project> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
									.Include(p => p.SourceProject)
								where p.DestProjectId == destProjectId
								orderby p.SourceProject.Name, p.SourceProjectId
								select p.SourceProject;

					projects = query.Distinct().ToList();
				}

				//Add the current project
				Project destProject = this.RetrieveById(destProjectId);
				if (!projects.Any(p => p.ProjectId == destProject.ProjectId))
				{
					projects.Add(destProject);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of projects that the current project can share artifacts with
		/// </summary>
		/// <param name="sourceProjectId">The id of the project that can share its artifacts</param>
		/// <returns>The list of projects and artifact types that it shares</returns>
		public List<ProjectArtifactSharing> ProjectAssociation_RetrieveForProject(int sourceProjectId)
		{
			const string METHOD_NAME = "ProjectAssociation_RetrieveForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectArtifactSharing> sharedProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectArtifactSharings
									.Include(p => p.DestProject)
									.Include(p => p.DestProject.Group)
									.Include(p => p.ArtifactType)
								where p.SourceProjectId == sourceProjectId
								orderby p.DestProject.Name, p.DestProjectId
								select p;

					sharedProjects = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return sharedProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Project Collection/Setting Methods

		/// <summary>Updates an existing project setting value</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to update an entry in</param>
		/// <param name="entryKey">The key name of the entry we're updating</param>
		/// <param name="entryValue">The new setting value to be stored</param>
		/// <param name="typeCode">The underlying type of the value we're storing</param>
		internal void UpdateSetting(int projectId, int userId, string collectionName, string entryKey, string entryValue, int typeCode)
		{
			const string METHOD_NAME = "UpdateSetting";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have a real project and user, or otherwise fail quietly
				if (projectId < 1 || userId < 1)
				{
					return;
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the specified entry for this collection 
					var query = from pce in context.ProjectCollectionEntries
								where
									pce.Collection.Name == collectionName &&
									pce.UserId == userId &&
									pce.ProjectId == projectId &&
									pce.EntryKey == entryKey
								select pce;

					//Get the entry
					ProjectCollectionEntry projectCollectionEntry = query.FirstOrDefault();

					//Make sure we have a setting, the code calling this function should have already checked for the insert case
					if (projectCollectionEntry != null)
					{
						projectCollectionEntry.StartTracking();
						projectCollectionEntry.EntryValue = entryValue;
						projectCollectionEntry.EntryTypeCode = (int)typeCode;
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Inserts a new project setting value</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to add an entry to</param>
		/// <param name="entryKey">The key name of the entry we're inserting</param>
		/// <param name="entryValue">The new setting value to be stored</param>
		/// <param name="typeCode">The underlying type of the value we're storing</param>
		internal void InsertSetting(int projectId, int userId, string collectionName, string entryKey, string entryValue, int typeCode)
		{
			const string METHOD_NAME = "InsertSetting";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have a real project and user, or otherwise fail quietly
				if (projectId < 1 || userId < 1)
				{
					return;
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the specified collection
					var query = from pc in context.ProjectCollections
								where pc.Name == collectionName
								select pc;

					//Get the collection
					ProjectCollection projectCollection = query.FirstOrDefault();

					//Make sure the collection exists
					if (projectCollection == null)
					{
						throw new UserSettingNotExistsException(String.Format(GlobalResources.Messages.UserManager_SettingNotExists, collectionName));
					}

					//Add the new entry
					projectCollection.StartTracking();

					ProjectCollectionEntry projectCollectionEntry = new ProjectCollectionEntry();
					projectCollectionEntry.UserId = userId;
					projectCollectionEntry.ProjectId = projectId;
					projectCollectionEntry.EntryKey = entryKey;
					projectCollectionEntry.EntryValue = entryValue;
					projectCollectionEntry.EntryTypeCode = (int)typeCode;
					projectCollection.Entries.Add(projectCollectionEntry);
					context.SaveChanges();
				}
			}
			catch (EntityConstraintViolationException)
			{
				//Ignore and attempt an update instead
				UpdateSetting(projectId, userId, collectionName, entryKey, entryValue, typeCode);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes an existing project setting value</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to delete an entry in</param>
		/// <param name="entryKey">The key name of the entry we're updating</param>
		internal void DeleteSetting(int projectId, int userId, string collectionName, string entryKey)
		{
			const string METHOD_NAME = "DeleteSetting";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the specified collection and entries
					var query = from pc in context.ProjectCollections.Include("Entries")
								where pc.Name == collectionName
								select pc;

					//Get the entry
					ProjectCollection projectCollection = query.FirstOrDefault();

					//Make sure the collection exists
					if (projectCollection == null)
					{
						throw new UserSettingNotExistsException(String.Format(GlobalResources.Messages.UserManager_SettingNotExists, collectionName));
					}

					//Delete the entry (make sure it hasn't already been deleted)
					projectCollection.StartTracking();
					ProjectCollectionEntry projectCollectionEntry = projectCollection.Entries.FirstOrDefault(pce => pce.EntryKey == entryKey && pce.ProjectId == projectId && pce.UserId == userId);
					if (projectCollectionEntry != null)
					{
						projectCollectionEntry.MarkAsDeleted();
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves a collection of project settings for a particular project, user and collection name</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're interested in</param>
		///	<param name="collectionName">The name of the collection we want to return settings for</param>
		/// <returns>The list of settings</returns>
		internal ProjectCollection RetrieveSettings(int projectId, int userId, string collectionName)
		{
			const string METHOD_NAME = "RetrieveSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			ProjectCollection projectCollection;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the the collection
					var query1 = from pc in context.ProjectCollections
								 where pc.Name == collectionName
								 select pc;

					//Next get the entries for this collection (joined by fix-up)
					var query2 = from pce in context.ProjectCollectionEntries
								 where pce.Collection.Name == collectionName && pce.UserId == userId && pce.ProjectId == projectId
								 orderby pce.EntryKey
								 select pce;

					projectCollection = query1.FirstOrDefault();
					query2.ToList();    //Joined implicitly by 'fix-up'
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return projectCollection;
		}

		/// <summary>
		/// Returns a count all the (active) projects in the system
		/// </summary>
		/// <param name="includeInactive">should we count inactive</param>
		/// <returns>the count</returns>
		public int Count(bool includeInactive = false)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectsView
								where (p.IsActive || includeInactive)
								select p;

					count = query.Count();
				}

				//Return the list
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

		/// <summary>
		/// Returns a list all the active projects in the system
		/// </summary>
		/// <returns></returns>
		public List<ProjectView> Retrieve()
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectView> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectsView
								where p.IsActive
								orderby p.Name, p.ProjectId
								select p;

					projects = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list all the projects in the specified group
		/// </summary>
		/// <param name="activeOnly">Do we want active only</param>
		/// <param name="projectGroupId">The id of the group</param>
		/// <returns></returns>
		public List<ProjectView> Project_RetrieveByGroup(int projectGroupId, bool activeOnly = true)
		{
			const string METHOD_NAME = "Project_RetrieveByGroup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectView> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectsView
								where p.ProjectGroupId == projectGroupId
								orderby p.Name, p.ProjectId
								select p;

					if (activeOnly)
					{
						query = (IOrderedQueryable<ProjectView>)query.Where(p => p.IsActive);
					}

					projects = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Returns a list of projects that the specified user is an owner of
		/// </summary>
		/// <param name="projectOwnerId"></param>
		/// <returns></returns>
		public List<ProjectUserView> RetrieveProjectsByOwner(int projectOwnerId)
		{
			const string METHOD_NAME = "RetrieveProjectsByOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectUserView> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from pu in context.ProjectUsersView
								where pu.UserId == projectOwnerId && pu.IsAdmin
								orderby pu.ProjectName, pu.ProjectId
								select pu;

					projects = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}



		/// <summary>Retrieves a list of all projects that match the passed-in filter</summary>
		/// <param name="filters">The hashtable of filters to apply to the project list</param>
		/// <param name="projectOwnerId">Return only project's that the user is the owner of (null for all projects)</param>
		/// <returns>A project dataset</returns>
		/// <remarks>Pass filters = null for all projects.
		/// The filters supported are for name, web-site, creation-date and active flag</remarks>
		public List<ProjectView> Retrieve(Hashtable filters, int? projectOwnerId)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectView> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query
					//See if we're only to return projects that are owned by the user
					IQueryable<ProjectView> query;
					if (projectOwnerId.HasValue)
					{
						//First get the list of projects that are owned by the user (not counting group owners)
						var query2 = from pu in context.ProjectUsers
									 where
										pu.UserId == projectOwnerId.Value &&
										pu.ProjectRoleId == ProjectManager.ProjectRoleProjectOwner
									 select pu.ProjectId;
						var projectIds = query2.ToList();

						//Create select command for retrieving these project records
						query = from p in context.ProjectsView
								where projectIds.Contains(p.ProjectId)
								select p;
					}
					else
					{
						//Create select command for retrieving all the project records
						query = from p in context.ProjectsView
								select p;
					}

					//Now add any specified filters
					if (filters != null)
					{
						//Project Name
						if (filters["Name"] != null)
						{
							//Break up the name into keywords
							MatchCollection keywordMatches = Regex.Matches((string)filters["Name"], Common.Global.REGEX_KEYWORD_MATCHER);
							foreach (Match keywordMatch in keywordMatches)
							{
								string keyword = keywordMatch.Value.Replace("\"", "");
								query = query.Where(p => p.Name.Contains(keyword));
							}
						}

						//Web Site
						if (filters["WebSite"] != null)
						{
							//Break up the website into keywords
							MatchCollection keywordMatches = Regex.Matches((string)filters["WebSite"], Common.Global.REGEX_KEYWORD_MATCHER);
							foreach (Match keywordMatch in keywordMatches)
							{
								string keyword = keywordMatch.Value.Replace("\"", "");
								query = query.Where(p => p.Website.Contains(keyword));
							}
						}

						//Project Group
						if (filters["ProjectGroupId"] != null)
						{
							int projectGroupId = (int)filters["ProjectGroupId"];
							query = query.Where(p => p.ProjectGroupId == projectGroupId);
						}

						//Project Template
						if (filters["ProjectTemplateId"] != null)
						{
							int projectTemplateId = (int)filters["ProjectTemplateId"];
							query = query.Where(p => p.ProjectTemplateId == projectTemplateId);
						}

						//Creation Date
						if (filters["CreationDate"] != null)
						{
							if (filters["CreationDate"].GetType() == typeof(string))
							{
								DateTime creationDate = DateTime.Parse((string)filters["CreationDate"]);

								//We want to ignore times in the filter
								DateTime startDate = creationDate.Date;
								DateTime endDate = creationDate.Date.AddDays(1);
								query = query.Where(p => p.CreationDate >= startDate);
								query = query.Where(p => p.CreationDate < endDate);
							}
							if (filters["CreationDate"].GetType() == typeof(DateTime))
							{
								DateTime creationDate = (DateTime)filters["CreationDate"];

								//We want to ignore times in the filter
								DateTime startDate = creationDate.Date;
								DateTime endDate = creationDate.Date.AddDays(1);
								query = query.Where(p => p.CreationDate >= startDate);
								query = query.Where(p => p.CreationDate < endDate);
							}
						}

						//Active Flag (check for both Y/N form and the newer boolean form)
						if (filters["ActiveYn"] != null && filters["ActiveYn"] is String)
						{
							bool isActive = ((string)filters["ActiveYn"] == "Y");
							query = query.Where(p => p.IsActive == isActive);
						}
						if (filters["IsActive"] != null)
						{
							bool isActive;
							if (filters["IsActive"] is Boolean)
							{
								isActive = (bool)filters["IsActive"];
							}
							else
							{
								isActive = ((string)filters["IsActive"] == "Y");
							}
							query = query.Where(p => p.IsActive == isActive);
						}

						//Project ID
						if (filters["ProjectId"] != null)
						{
							//Need to make sure that the project id is numeric
							int projectId;
							if (Int32.TryParse((string)filters["ProjectId"], out projectId))
							{
								query = query.Where(p => p.ProjectId == projectId);
							}
						}
					}

					//Add the sorts and execute
					query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectId);
					projects = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;

			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single project record by project id</summary>
		/// <param name="projectId">The ID of the project to retrieve</param>
		/// <returns>A project entity</returns>
		/// <remarks>Returns all projects (active and inactive)</remarks>
		public Project RetrieveById(int projectId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project record
				Project project;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Projects
								where p.ProjectId == projectId
								select p;

					project = query.FirstOrDefault();
				}

				//Throw an exception if the project record is not found
				if (project == null)
				{
					throw new ArtifactNotExistsException("Project " + projectId + " doesn't exist in the system");
				}

				//Return the project
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return project;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single project record by project id</summary>
		/// <param name="projectId">The ID of the project to retrieve</param>
		/// <returns>A project view entity</returns>
		/// <remarks>Returns all projects (active and inactive) and selects from the View unlike RetrieveById</remarks>
		public ProjectView RetrieveById2(int projectId)
		{
			const string METHOD_NAME = "RetrieveById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project record
				ProjectView project;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectsView
								where p.ProjectId == projectId
								select p;

					project = query.FirstOrDefault();
				}

				//Throw an exception if the project record is not found
				if (project == null)
				{
					throw new ArtifactNotExistsException("Project " + projectId + " doesn't exist in the system");
				}

				//Return the project
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return project;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Calculates the colors to display in the project resources progress bar</summary>
		/// <param name="resourceRow">The resource row</param>
		/// <param name="percentGreen">The % that should be displayed as green</param>
		/// <param name="percentRed">The % that should be displayed as red</param>
		/// <param name="percentYellow">The % that should be displayed as yellow</param>
		/// <param name="percentGray">The % that should be displayed as gray</param>
		/// <returns>The textual version of the progress information</returns>
		public static string CalculateResourceProgress(ProjectResourceView projectResource, out int percentGreen, out int percentRed, out int percentYellow, out int percentGray)
		{
			//Set the default percents
			percentGreen = 0;
			percentRed = 0;
			percentYellow = 0;
			percentGray = 100;

			//Display a green bar illustrating the % work allocated to a user. Go red if over-allocated
			//Indicate how much work is 'open' by using the yellow color
			string tooltipText = "";
			if (projectResource.ResourceEffort.HasValue)
			{
				decimal percentAllocated = (decimal)projectResource.TotalEffort / (decimal)projectResource.ResourceEffort.Value * (decimal)100;
				decimal percentAllocatedOpen = (decimal)projectResource.TotalOpenEffort / (decimal)projectResource.ResourceEffort.Value * (decimal)100;
				if (percentAllocated > 100)
				{
					if (percentAllocatedOpen > 0M)
					{
						percentRed = 100;
						percentGray = 0;
					}
					else
					{
						percentGreen = 100;
						percentGray = 0;
					}
				}
				else
				{
					percentGreen = (int)percentAllocated - (int)percentAllocatedOpen;
					percentYellow = (int)percentAllocatedOpen;
					percentGray = 100 - percentGreen - percentYellow;
				}
				tooltipText = percentAllocated.ToString("0") + "% " + GlobalResources.General.Project_ResourceAllocated + ", " + percentAllocatedOpen.ToString("0") + "% " + GlobalResources.General.Project_ResourceAllocatedOpen;
			}
			else
			{
				//This is the case when we are looking at an entire project or group, where we don't have any resource effort
				//since we don't have a defined date range
				if (projectResource.TotalEffort <= 0)
				{
					percentGray = 100;
					percentGreen = 0;
					percentYellow = 0;
					tooltipText = GlobalResources.General.Global_None;
				}
				else
				{
					decimal percentAllocatedOpen = (decimal)projectResource.TotalOpenEffort / (decimal)projectResource.TotalEffort * (decimal)100;
					if (percentAllocatedOpen > 100)
					{
						percentGray = 0;
						percentYellow = 100;
						percentGreen = 0;
					}
					else
					{
						percentGray = 0;
						percentYellow = (int)percentAllocatedOpen;
						percentGreen = 100 - (int)percentAllocatedOpen;
					}
					tooltipText = percentAllocatedOpen.ToString("0") + "% " + GlobalResources.General.Project_ResourceAllocatedOpen;
				}
			}

			return tooltipText;
		}

		public List<ProjectUserView> RetrieveTotalResources(int projectId)
		{
			const string METHOD_NAME = "RetrieveTotalResources";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Define the return list
				List<ProjectUserView> projectResources;

				
					//Create query for retrieving the project resources information
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from p in context.ProjectUsersView
									where p.ProjectId == projectId
									orderby p.FullName, p.UserId
									select p;

						projectResources = query.ToList();
					}
			
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectResources;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ProjectResourceView> RetrieveAssignedResources(int projectId)
		{
			const string METHOD_NAME = "RetrieveAssignedResources";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Define the return list
				List<ProjectResourceView> projectResources;


				//Create query for retrieving the project resources information
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectResourcesView
								where p.ProjectId == projectId
								orderby p.FullName, p.UserId
								select p;

					projectResources = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectResources;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves the list of resources for a project, filtered by release</summary>
		/// <param name="projectId">The ID of the project to retrieve the users for</param>
		/// <param name="releaseId">The ID of the release/iteration to filter by (null = entire project)</param>
		/// <param name="filters">Any filters to apply</param>
		/// <param name="sortAscending">Whether to sort ascending</param>
		/// <param name="sortProperty">A sort property to apply</param>
		/// <param name="utcOffset">The current timezone's UTC offset (only needed if we have a date filter)</param>
		/// <returns>A project resource list</returns>
		public List<ProjectResourceView> RetrieveResources(int projectId, int? releaseId = null, string sortProperty = null, bool? sortAscending = null, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "RetrieveResources";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Define the return list
				List<ProjectResourceView> projectResources;

				//See if we have a release or not as we need to use different queries in each case
				if (releaseId.HasValue)
				{
					//Call the stored procedure to get the resources by release/iteration
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						projectResources = context.Project_RetrieveResourcesByRelease(projectId, releaseId.Value).ToList();
					}
				}
				else
				{
					//Create query for retrieving the project resources information
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from p in context.ProjectResourcesView
									where p.ProjectId == projectId
									orderby p.FullName, p.UserId
									select p;

						projectResources = query.ToList();
					}
				}

				//Need to get the project configuration settings
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Get the available person effort for the release (if we have one)
				int? availablePersonEffort = null;
				if (releaseId.HasValue)
				{
					ReleaseManager releaseManager = new ReleaseManager();
					try
					{
						ReleaseView releaseRow = releaseManager.RetrieveById2(projectId, releaseId.Value);
						int workingDaysPerWeek = project.WorkingDays;
						int workingHoursPerDay = project.WorkingHours;
						int nonWorkingHours = project.NonWorkingHours;
						availablePersonEffort = ReleaseManager.CalculatePlannedEffort(releaseRow.StartDate, releaseRow.EndDate, 1, workingDaysPerWeek, workingHoursPerDay, nonWorkingHours, 0);
					}
					catch (ArtifactNotExistsException)
					{
						//Fail quietly
					}
				}

				//Now we need to iterate through the records and calculate the total effort
				//this depends on how the system is configured (i.e. which effort gets included)
				//Also flag those items that have exceeded the effort available in the release/iteration
				foreach (ProjectResourceView projectResource in projectResources)
				{
					//Add the person's available effort to the row
					projectResource.resourceEffort = availablePersonEffort;

					//Add Task Effort if configured to do so
					if (project.IsEffortTasks && projectResource.ReqTaskEffort.HasValue)
					{
						projectResource.totalEffort += projectResource.ReqTaskEffort.Value;
					}
					//Add Incident Effort if configured to do so
					if (project.IsEffortIncidents && projectResource.IncidentEffort.HasValue)
					{
						projectResource.totalEffort += projectResource.IncidentEffort.Value;
					}

					//Add Task Open Effort if configured to do so
					if (project.IsEffortTasks && projectResource.ReqTaskEffortOpen.HasValue)
					{
						projectResource.totalOpenEffort += projectResource.ReqTaskEffortOpen.Value;
					}
					//Add Incident Open Effort if configured to do so
					if (project.IsEffortIncidents && projectResource.IncidentEffortOpen.HasValue)
					{
						projectResource.totalOpenEffort += projectResource.IncidentEffortOpen.Value;
					}

					//If the total exceeds the person's available, set the overallocated flag
					if (availablePersonEffort.HasValue && projectResource.TotalEffort > availablePersonEffort.Value)
					{
						projectResource.isOverAllocated = true;
					}

					//Set the net remaining effort
					if (availablePersonEffort.HasValue)
					{
						projectResource.remainingEffort = availablePersonEffort.Value - projectResource.totalEffort;
					}
				}

				//Now we need to use LINQ filtering (not LINQ-Entities since purely in-memory)
				if (filters != null)
				{
					Expression<Func<ProjectResourceView, bool>> filterExpression = CreateFilterExpression<ProjectResourceView>(projectId, null, Artifact.ArtifactTypeEnum.User, filters, utcOffset, new List<string> { "AllocationIndicator" }, null, false);
					if (filterExpression != null)
					{
						projectResources = projectResources.Where(filterExpression.Compile()).ToList();
					}

					//The allocation filter is more complex and needs to be handled separately
					if (filters.ContainsKey("AllocationIndicator") && filters["AllocationIndicator"] is Int32)
					{
						int allocationFilter = (int)filters["AllocationIndicator"];

						//The functionality has to work differently depending on whether we have a release or not
						//since right now with no release selected, we don't have a value for resource effort
						if (releaseId.HasValue)
						{
							switch (allocationFilter)
							{
								case 1:
									//= 0%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 0).ToList();
									break;
								case 2:
									//>= 25%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 25).ToList();
									break;
								case 3:
									//>= 50%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 50).ToList();
									break;
								case 4:
									//>= 75%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 75).ToList();
									break;
								case 5:
									//= 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 100).ToList();
									break;
								case 6:
									//> 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) > 100).ToList();
									break;
								case 7:
									//< 25%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 25).ToList();
									break;
								case 8:
									//< 50%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 50).ToList();
									break;
								case 9:
									//< 75%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 75).ToList();
									break;
								case 10:
									//< 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 100).ToList();
									break;
							}
						}
						else
						{
							switch (allocationFilter)
							{
								case 1:
								//= 0%
								case 7:
								//< 25%
								case 8:
								//< 50%
								case 9:
								//< 75%
								case 10:
									//< 100%
									projectResources = projectResources.Where(p => p.TotalEffort == 0).ToList();
									break;
								case 2:
								//>= 25%
								case 3:
								//>= 50%
								case 4:
								//>= 75%
								case 5:
									//= 100%
									projectResources = projectResources.Where(p => p.TotalEffort > 0).ToList();
									break;
								case 6:
									//> 100%
									//Not possible when no release selected, since we don't know what the resource has available
									projectResources = projectResources.Where(p => false).ToList();
									break;
							}
						}
					}
				}

				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty) && sortAscending.HasValue)
				{
					projectResources = new GenericSorter<ProjectResourceView>().Sort(projectResources, sortProperty, sortAscending.Value).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectResources;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ProjectResourceView> RetrieveAssignedResources(int projectId, int? releaseId = null, string sortProperty = null, bool? sortAscending = null, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "RetrieveAssignedResources";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Define the return list
				List<ProjectResourceView> projectResources;

				//See if we have a release or not as we need to use different queries in each case
				if (releaseId.HasValue)
				{
					//Call the stored procedure to get the resources by release/iteration
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{

						projectResources = context.Project_RetrieveResourcesByRelease(projectId, releaseId.Value).ToList();

						if(projectResources.Count > 0)
						{
							projectResources = (List<ProjectResourceView>)projectResources.Where(h => h.UserId > 0);
						}
					}
				}
				else
				{
					//Create query for retrieving the project resources information
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from p in context.ProjectResourcesView
									where p.ProjectId == projectId && p.UserId > 0
									orderby p.FullName, p.UserId
									select p;

						query = (IOrderedQueryable<ProjectResourceView>)query.Where(h => h.UserId > 0);

						projectResources = query.ToList();
					}
				}

				//Need to get the project configuration settings
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Get the available person effort for the release (if we have one)
				int? availablePersonEffort = null;
				if (releaseId.HasValue)
				{
					ReleaseManager releaseManager = new ReleaseManager();
					try
					{
						ReleaseView releaseRow = releaseManager.RetrieveById2(projectId, releaseId.Value);
						int workingDaysPerWeek = project.WorkingDays;
						int workingHoursPerDay = project.WorkingHours;
						int nonWorkingHours = project.NonWorkingHours;
						availablePersonEffort = ReleaseManager.CalculatePlannedEffort(releaseRow.StartDate, releaseRow.EndDate, 1, workingDaysPerWeek, workingHoursPerDay, nonWorkingHours, 0);
					}
					catch (ArtifactNotExistsException)
					{
						//Fail quietly
					}
				}

				//Now we need to iterate through the records and calculate the total effort
				//this depends on how the system is configured (i.e. which effort gets included)
				//Also flag those items that have exceeded the effort available in the release/iteration
				foreach (ProjectResourceView projectResource in projectResources)
				{
					//Add the person's available effort to the row
					projectResource.resourceEffort = availablePersonEffort;

					//Add Task Effort if configured to do so
					if (project.IsEffortTasks && projectResource.ReqTaskEffort.HasValue)
					{
						projectResource.totalEffort += projectResource.ReqTaskEffort.Value;
					}
					//Add Incident Effort if configured to do so
					if (project.IsEffortIncidents && projectResource.IncidentEffort.HasValue)
					{
						projectResource.totalEffort += projectResource.IncidentEffort.Value;
					}

					//Add Task Open Effort if configured to do so
					if (project.IsEffortTasks && projectResource.ReqTaskEffortOpen.HasValue)
					{
						projectResource.totalOpenEffort += projectResource.ReqTaskEffortOpen.Value;
					}
					//Add Incident Open Effort if configured to do so
					if (project.IsEffortIncidents && projectResource.IncidentEffortOpen.HasValue)
					{
						projectResource.totalOpenEffort += projectResource.IncidentEffortOpen.Value;
					}

					//If the total exceeds the person's available, set the overallocated flag
					if (availablePersonEffort.HasValue && projectResource.TotalEffort > availablePersonEffort.Value)
					{
						projectResource.isOverAllocated = true;
					}

					//Set the net remaining effort
					if (availablePersonEffort.HasValue)
					{
						projectResource.remainingEffort = availablePersonEffort.Value - projectResource.totalEffort;
					}
				}

				//Now we need to use LINQ filtering (not LINQ-Entities since purely in-memory)
				if (filters != null)
				{
					Expression<Func<ProjectResourceView, bool>> filterExpression = CreateFilterExpression<ProjectResourceView>(projectId, null, Artifact.ArtifactTypeEnum.User, filters, utcOffset, new List<string> { "AllocationIndicator" }, null, false);
					if (filterExpression != null)
					{
						projectResources = projectResources.Where(filterExpression.Compile()).ToList();
					}

					//The allocation filter is more complex and needs to be handled separately
					if (filters.ContainsKey("AllocationIndicator") && filters["AllocationIndicator"] is Int32)
					{
						int allocationFilter = (int)filters["AllocationIndicator"];

						//The functionality has to work differently depending on whether we have a release or not
						//since right now with no release selected, we don't have a value for resource effort
						if (releaseId.HasValue)
						{
							switch (allocationFilter)
							{
								case 1:
									//= 0%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 0).ToList();
									break;
								case 2:
									//>= 25%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 25).ToList();
									break;
								case 3:
									//>= 50%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 50).ToList();
									break;
								case 4:
									//>= 75%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 75).ToList();
									break;
								case 5:
									//= 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 100).ToList();
									break;
								case 6:
									//> 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) > 100).ToList();
									break;
								case 7:
									//< 25%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 25).ToList();
									break;
								case 8:
									//< 50%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 50).ToList();
									break;
								case 9:
									//< 75%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 75).ToList();
									break;
								case 10:
									//< 100%
									projectResources = projectResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 100).ToList();
									break;
							}
						}
						else
						{
							switch (allocationFilter)
							{
								case 1:
								//= 0%
								case 7:
								//< 25%
								case 8:
								//< 50%
								case 9:
								//< 75%
								case 10:
									//< 100%
									projectResources = projectResources.Where(p => p.TotalEffort == 0).ToList();
									break;
								case 2:
								//>= 25%
								case 3:
								//>= 50%
								case 4:
								//>= 75%
								case 5:
									//= 100%
									projectResources = projectResources.Where(p => p.TotalEffort > 0).ToList();
									break;
								case 6:
									//> 100%
									//Not possible when no release selected, since we don't know what the resource has available
									projectResources = projectResources.Where(p => false).ToList();
									break;
							}
						}
					}
				}

				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty) && sortAscending.HasValue)
				{
					projectResources = new GenericSorter<ProjectResourceView>().Sort(projectResources, sortProperty, sortAscending.Value).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectResources;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Refreshes the requirements and release task progress for a project</summary>
		/// <param name="projectId">The project being refreshed</param>
		public void RefreshTaskProgressCache(int projectId)
		{
			const string METHOD_NAME = "RefreshTaskProgressCache";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to refresh the requirements' task progress
				RequirementManager requirementManager = new RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(UserManager.UserInternal, projectId, 1, Int32.MaxValue, null, 0);
				foreach (RequirementView requirement in requirements)
				{
					requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirement.RequirementId);
				}

				//Now iterate through all the releases/iterations in the project and refresh their task progress
				//we also update their requirement completion values as well
				//The method will automatically roll-up to the containing project, programs and portfolios as well
				ReleaseManager releaseManager = new ReleaseManager();
				List<int> releaseIds = releaseManager.GetAllReleaseIdsInProject(projectId, false);
				releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>The implementation of the RefreshTestStatusAndTaskProgressCache method that can be called synchronously or asychronously</summary>
		/// <param name="state">The context passed to the asynchronous handler</param>
		private void RefreshTestStatusAndTaskProgressCache_Callback(object state)
		{
			const string METHOD_NAME = "RefreshTestStatusAndTaskProgressCache";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGINATION_SIZE = 500;

			try
			{
				//Unpack the dataset and project from the state dictionary
				Dictionary<string, object> stateContext = (Dictionary<string, object>)state;

				int projectId = (int)stateContext["projectId"];
				int? releaseId = (int?)stateContext["releaseId"];

				//Call the methods to refresh the cached counts within the test case hierarchy
				TestCaseManager testCaseManager = new TestCaseManager();
				TestRunManager testRunManager = new TestRunManager();

				//Call the method to refresh the test case execution status
				int count = testCaseManager.Count(projectId, null, 0, null, false, true);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, index, PAGINATION_SIZE, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
					foreach (TestCaseView testCase in testCases)
					{
						testRunManager.RefreshTestCaseExecutionStatus3(projectId, testCase.TestCaseId);
					}
				}

				//Now refresh the folder counts
				testCaseManager.RefreshFolderCounts(projectId);

				//Call the method to refresh the requirements test status
				RequirementManager requirementManager = new RequirementManager();
				count = requirementManager.Count(Business.UserManager.UserInternal, projectId, null, 0);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					List<RequirementView> requirements = requirementManager.Retrieve(UserManager.UserInternal, projectId, index, PAGINATION_SIZE, null, 0);
					foreach (RequirementView requirement in requirements)
					{
						requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirement.RequirementId);
					}
				}

				//Now iterate through all the releases/iterations in the project and refresh their test status
				//Or just refresh the one selected release
				ReleaseManager releaseManager = new ReleaseManager();
				List<int> releaseIds = releaseManager.GetAllReleaseIdsInProject(projectId, false);
				releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);

				//Finally we need to also update the test sets
				TestSetManager testSetManager = new TestSetManager();
				List<TestSetView> testSets = testSetManager.Retrieve(projectId, null, true, 1, Int32.MaxValue, null, 0);
				foreach (TestSetView testSet in testSets)
				{
					testSetManager.TestSet_RefreshExecutionData(projectId, testSet.TestSetId);
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

		/// <summary>Refreshes the summary requirement completion (start/end-date, # requirements, % complete) for a particular project, program, and portfolio</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <remarks>It rolls up the values from the project to the program, to the portfolio</remarks>
		protected internal void RefreshRequirementCompletion(int projectId)
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
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.Project_RefreshRequirementCompletion(projectId);
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


		/// <summary>Refreshes the requirements and release execution test status and task for a project</summary>
		/// <param name="projectId">The project being refreshed</param>
		/// <param name="releaseId">The release being refreshed, or NULL for all releases in the project</param>
		/// <param name="runAsync">Should we run asynchronously or not</param>
		public void RefreshTestStatusAndTaskProgressCache(int projectId, bool runAsync, int? releaseId = null)
		{
			const string METHOD_NAME = "RefreshTestStatusAndTaskProgressCache";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If running in background, queue the background task, otherwise run immediately
			Dictionary<string, object> state = new Dictionary<string, object>();
			state.Add("projectId", projectId);
			state.Add("releaseId", releaseId);

			//See if we can queue this operation in the thread-pool. If not then just call directly as a backup option
			if (runAsync && ThreadPool.QueueUserWorkItem(new WaitCallback(RefreshTestStatusAndTaskProgressCache_Callback), state))
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "RefreshTestStatusAndTaskProgressCache_Callback initiated as a background thread");
			}
			else
			{
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "RefreshTestStatusAndTaskProgressCache_Callback initiated as part of main thread");
				RefreshTestStatusAndTaskProgressCache_Callback(state);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Refreshes the requirements and release execution test status for a project</summary>
		/// <param name="projectId">The project being refreshed</param>
		public void RefreshTestStatusCache(int projectId)
		{
			const string METHOD_NAME = "RefreshTestStatusCache";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGINATION_SIZE = 500;

			try
			{
				//Call the method to refresh the cached counts within the test case hierarchy
				TestCaseManager testCaseManager = new TestCaseManager();
				TestRunManager testRunManager = new TestRunManager();

				//Call the method to refresh the test case execution status
				int count = testCaseManager.Count(projectId, null, 0, null, false, true);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					List<TestCaseView> testCases = testCaseManager.Retrieve(projectId, "Name", true, index, PAGINATION_SIZE, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
					foreach (TestCaseView testCase in testCases)
					{
						testRunManager.RefreshTestCaseExecutionStatus3(projectId, testCase.TestCaseId);
					}
				}

				//Now refresh the folder counts
				testCaseManager.RefreshFolderCounts(projectId);

				//Call the method to refresh the requirements test status
				RequirementManager requirementManager = new RequirementManager();
				count = requirementManager.Count(UserManager.UserInternal, projectId, null, 0);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					List<RequirementView> requirements = requirementManager.Retrieve(UserManager.UserInternal, projectId, index, PAGINATION_SIZE, null, 0);
					foreach (RequirementView requirement in requirements)
					{
						requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirement.RequirementId);
					}
				}

				//Now iterate through all the releases/iterations in the project and refresh their test status
				ReleaseManager releaseManager = new ReleaseManager();
				List<int> releaseIds = releaseManager.GetAllReleaseIdsInProject(projectId, false);
				releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);

				//Finally we need to also update the test sets
				TestSetManager testSetManager = new TestSetManager();
				count = testSetManager.Count(projectId, null, 0, null, false, true);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					List<TestSetView> testSets = testSetManager.Retrieve(projectId, null, true, index, PAGINATION_SIZE, null, 0);
					foreach (TestSetView testSet in testSets)
					{
						testSetManager.TestSet_RefreshExecutionData(projectId, testSet.TestSetId);
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

		/// <summary>Returns a sorted list of values to populate the lookup for the resource allocation filter</summary>
		/// <returns>Sorted List containing filter values</returns>
		public static SortedList<int, string> RetrieveAllocationFilters()
		{
			const string METHOD_NAME = "RetrieveAllocationFilters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the filters list populated, then create, otherwise just return
				if (allocationFiltersList == null)
				{
					allocationFiltersList = new SortedList<int, string>();
					allocationFiltersList.Add(1, "=  0%");
					allocationFiltersList.Add(2, ">= 25%");
					allocationFiltersList.Add(3, ">= 50%");
					allocationFiltersList.Add(4, ">= 75%");
					allocationFiltersList.Add(5, "= 100%");
					allocationFiltersList.Add(6, "> 100%");
					allocationFiltersList.Add(7, "< 25%");
					allocationFiltersList.Add(8, "< 50%");
					allocationFiltersList.Add(9, "< 75%");
					allocationFiltersList.Add(10, "< 100%");
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return allocationFiltersList;
		}

		/// <summary>
		/// Retrieves the list of projects that use a specific template
		/// </summary>
		/// <param name="projectTemplateId">The id of the template</param>
		/// <returns>The list of projects</returns>
		public List<Project> RetrieveForTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project records
				List<Project> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Projects
									.Include(p => p.Group)
								where p.ProjectTemplateId == projectTemplateId
								orderby p.Name, p.ProjectId
								select p;

					projects = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public UserRecentProject RetrieveRecentProjectForCurrentUser(int userId, List<int> authorizedProjects = null)
		{
			const string METHOD_NAME = "RetrieveRecentProjectsForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				UserRecentProject recentProjects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the list of projects for a user
					var query = from p in context.UserRecentProjects
									.Include(p => p.Project)
									.Include(p => p.Project.Group)
									.Include(p => p.Project.Group.Portfolio)
								where p.UserId == userId
								select p;

					//Filter by authorized project (if specified)
					if (authorizedProjects != null)
					{
						query = query.Where(p => authorizedProjects.Contains(p.ProjectId));
					}

					//Sort by date (descending)
					query = query.OrderByDescending(p => p.LastAccessDate);

					//Return the list
					recentProjects = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return recentProjects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves all the projects that a user has rights to access
		/// </summary>
		/// <param name="userId">The ID of the user whose projects we want to retrieve</param>
		/// <returns>A project dataset</returns>
		/// <remarks>
		/// 1)  Returns only active projects
		/// 2)  If a user is a member of a project group, they will be granted the role of observer in any
		///     of its child projects if they don't explicitly have a role
		/// </remarks>
		public List<ProjectForUserView> RetrieveForUser(int userId)
		{
			const string METHOD_NAME = "RetrieveForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project records
				List<ProjectForUserView> projects;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectForUsersView
								where p.UserId == userId
								orderby p.Name, p.ProjectId
								select p;

					projects = query.ToList();
				}


				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projects;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Copies the artifact custom properties between an old and new project
		/// </summary>
		protected void CopyArtifactCustomProperties(int userId, int existingProjectId, int newProjectId, int existingProjectTemplateId, int newProjectTemplateId, Artifact.ArtifactTypeEnum artifactType, Dictionary<int, int> artifactIdMapping, Dictionary<int, int> customPropertyValueMapping)
		{
			//See if the templates are the same for the two projects
			bool sameTemplate = (existingProjectTemplateId == newProjectTemplateId);

			//First get the custom property definitions in the old and new project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> newProjectCustomPropDefinitions = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(newProjectTemplateId, artifactType, false);

			//Now get the artifact custom properties for all requirements in the project
			List<ArtifactCustomProperty> existingArtifactCustomProperties = customPropertyManager.ArtifactCustomProperty_RetrieveForArtifactType(existingProjectId, existingProjectTemplateId, artifactType);
			foreach (ArtifactCustomProperty existingArtifactCustomProperty in existingArtifactCustomProperties)
			{
				int oldArtifactId = existingArtifactCustomProperty.ArtifactId;
				int newArtifactId;
				if (artifactIdMapping.TryGetValue(oldArtifactId, out newArtifactId))
				{
					//Clone the column values, making sure we map any custom property values to their new project equivalents (unless templates the same)
					ArtifactCustomProperty newArtifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(newProjectId, artifactType, newArtifactId, newProjectCustomPropDefinitions);
					foreach (CustomProperty newProjectCustomPropDefinition in newProjectCustomPropDefinitions)
					{
						int customPropertyNumber = newProjectCustomPropDefinition.PropertyNumber;
						object oldValue = existingArtifactCustomProperty.CustomProperty(customPropertyNumber);

						//Only the list types need to be modified (since the values have different IDs)
						if (newProjectCustomPropDefinition.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.List && oldValue is Int32)
						{
							int oldCustomPropertyValueId = (int)oldValue;
							if (sameTemplate)
							{
								newArtifactCustomProperty.SetCustomProperty(customPropertyNumber, oldCustomPropertyValueId);
							}
							else
							{
								if (customPropertyValueMapping.ContainsKey(oldCustomPropertyValueId))
								{
									int newCustomPropertyValueId = customPropertyValueMapping[oldCustomPropertyValueId];
									newArtifactCustomProperty.SetCustomProperty(customPropertyNumber, newCustomPropertyValueId);
								}
							}
						}
						else if (newProjectCustomPropDefinition.CustomPropertyTypeId == (int)CustomProperty.CustomPropertyTypeEnum.MultiList && oldValue is List<Int32>)
						{
							List<int> oldCustomPropertyValueIds = (List<int>)oldValue;
							List<int> newCustomPropertyValueIds = new List<int>();
							foreach (int oldCustomPropertyValueId in oldCustomPropertyValueIds)
							{
								if (sameTemplate)
								{
									newCustomPropertyValueIds.Add(oldCustomPropertyValueId);
								}
								else
								{
									if (customPropertyValueMapping.ContainsKey(oldCustomPropertyValueId))
									{
										int newCustomPropertyValueId = customPropertyValueMapping[oldCustomPropertyValueId];
										newCustomPropertyValueIds.Add(newCustomPropertyValueId);
									}
								}
							}
							newArtifactCustomProperty.SetCustomProperty(customPropertyNumber, newCustomPropertyValueIds);
						}
						else
						{
							newArtifactCustomProperty.SetCustomProperty(customPropertyNumber, oldValue);
						}
					}

					//Save the new record
					customPropertyManager.ArtifactCustomProperty_Save(newArtifactCustomProperty, userId);
				}
			}
		}

		/// <summary>
		/// Copies the artifact associations properties between an old and new project - for a specific artifact id with a specific artifact type. Used for copying projects
		/// <param name="projectId">The project to create the associations in</param>
		/// <param name="sourceArtifactType">The artifactTypeEnum of the source artifact - should match the artifact type used in artifactMapping</param>
		/// <param name="destinationArtifactType">The artifactTypeEnum of the destination artifact - the one to find associations with</param>
		/// <param name="sourceArtifactMapping">The dictionary of artifact maps for the source artifact - old ids mapped against the new ids</param>
		/// <param name="destArtifactMapping">The dictionary of artifact maps for the destination artifacts - old ids mapped against the new ids</param>
		/// </summary>
		protected void CopyArtifactAssociations(int projectId, DataModel.Artifact.ArtifactTypeEnum sourceArtifactType, DataModel.Artifact.ArtifactTypeEnum destinationArtifactType, Dictionary<int, int> sourceArtifactMapping, Dictionary<int, int> destArtifactMapping)
		{
			ArtifactLinkManager artifactLinkManager = new ArtifactLinkManager();

			foreach (KeyValuePair<int, int> sourceArtifact in sourceArtifactMapping)
			{
				//Get the links from the source artifact to the specified artifact type
				List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(sourceArtifactType, sourceArtifact.Key, destinationArtifactType);
				foreach (ArtifactLinkView artifactLinkRow in artifactLinks)
				{
					//Lookup these artifacts in the mapping list
					if (destArtifactMapping.ContainsKey(artifactLinkRow.ArtifactId))
					{
						int sourceArtifacttId = sourceArtifactMapping[sourceArtifact.Key];
						int destArtifactId = destArtifactMapping[artifactLinkRow.ArtifactId];
						artifactLinkManager.Insert(
							projectId,
							sourceArtifactType,
							sourceArtifacttId,
							destinationArtifactType,
							destArtifactId,
							artifactLinkRow.CreatorId,
							artifactLinkRow.Comment,
							artifactLinkRow.CreationDate,
							(ArtifactLink.ArtifactLinkTypeEnum)artifactLinkRow.ArtifactLinkTypeId
						);
					}
				}
			}
		}

		/// <summary>
		/// Copies the artifact discussion. Used for copying projects [all artifacts except incidents]
		/// <param name="projectId">The project to create the associations in</param>
		/// <param name="artifactMapping">The dictionary of artifact maps - old ids mapped against the new ids</param>
		/// <param name="ArtifactType">The artifactTypeEnum of the artifact in question - should match the artifact type used in artifactMapping</param>
		/// </summary>
		protected void CopyArtifactDiscussions(int projectId, Dictionary<int, int> artifactMapping, DataModel.Artifact.ArtifactTypeEnum ArtifactType)
		{
			DiscussionManager discussionManager = new DiscussionManager();

			foreach (KeyValuePair<int, int> artifact in artifactMapping)
			{
				IEnumerable<IDiscussion> discussions = discussionManager.Retrieve(artifact.Key, ArtifactType);
				if (discussions.Count() > 0)
				{
					foreach (IDiscussion discussion in discussions)
					{
						discussionManager.Insert(discussion.CreatorId, artifact.Value, ArtifactType, discussion.Text, discussion.CreationDate, projectId, discussion.IsPermanent, false);
					}
				}
			}
		}

		/// <summary>
		/// Clones an existing project into a new project
		/// </summary>
		/// <param name="existingProjectId">The project to be copied</param>
		/// <param name="userId">The user we're performing the operation as</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <param name="createNewTemplate">Should we create a new template (default = False)</param>
		/// <returns>The id of the copied project</returns>
		/// <remarks>
		/// Copies the project meta-data using CreateFrom() and
		/// copies all artifacts and attachments in the project and
		/// copies all comments, attachment links, test coverage, and associations (within the project only)
		/// Cloning can potentially use the same or different templates, so need to account for that.
		/// </remarks>
		public int Copy(int userId, int existingProjectId, bool createNewTemplate = false, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGE_SIZE = 100;  //Copy in batches of 100

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.Project_Copy_CreateShell);
				}

				//Create the mapping dictionaries for storing old vs new IDs
				Dictionary<int, int> requirementMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseMapping = new Dictionary<int, int>();
				Dictionary<int, int> testStepMapping = new Dictionary<int, int>();
				Dictionary<int, int> releaseMapping = new Dictionary<int, int>();
				Dictionary<int, int> testSetFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> testSetMapping = new Dictionary<int, int>();
				Dictionary<int, int> automationHostMapping = new Dictionary<int, int>();
				Dictionary<int, int> componentMapping = new Dictionary<int, int>();
				Dictionary<int, int> taskFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> taskMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseParameterMapping = new Dictionary<int, int>();
				Dictionary<int, int> testConfigurationSetMapping = new Dictionary<int, int>();
				Dictionary<int, int> incidentMapping = new Dictionary<int, int>();
				Dictionary<int, int> riskMapping = new Dictionary<int, int>();
				Dictionary<int, int> testRunMapping = new Dictionary<int, int>();

				//First we need to retrieve the existing project
				Project project = this.RetrieveById(existingProjectId);
				if (project == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Project_ArtifactNotExists, existingProjectId));
				}

				//Get the project details
				string projectName = project.Name + CopiedArtifactNameSuffix;

				//Create a copy of this project with the same user membership, workflows and incident lists
				Dictionary<int, int> attachmentFolderMapping;
				Dictionary<int, int> incidentStatusMapping;
				Dictionary<int, int> incidentTypeMapping;
				Dictionary<int, int> incidentPriorityMapping;
				Dictionary<int, int> incidentSeverityMapping;
				Dictionary<int, int> requirementTypeMapping;
				Dictionary<int, int> requirementImportanceMapping;
				Dictionary<int, int> taskTypeMapping;
				Dictionary<int, int> taskPriorityMapping;
				Dictionary<int, int> testCaseTypeMapping;
				Dictionary<int, int> testCasePriorityMapping;
				Dictionary<int, int> documentTypeMapping;
				Dictionary<int, int> documentStatusMapping;
				Dictionary<int, int> riskStatusMapping;
				Dictionary<int, int> riskTypeMapping;
				Dictionary<int, int> riskProbabilityMapping;
				Dictionary<int, int> riskImpactMapping;
				//Dictionary<int, int> riskDetectabilityMapping; - TODO once add detectability
				Dictionary<int, int> customPropertyIdMapping;
				Dictionary<int, int> customPropertyValueMapping;
				Dictionary<int, int> attachmentIdsMapping = new Dictionary<int, int>();
				int newProjectId = CreateFromExisting(projectName,
					project.Description,
					project.Website,
					existingProjectId,
					project.IsActive,
					createNewTemplate,
					out attachmentFolderMapping,
					out incidentStatusMapping,
					out incidentTypeMapping,
					out incidentPriorityMapping,
					out incidentSeverityMapping,
					out requirementTypeMapping,
					out requirementImportanceMapping,
					out taskTypeMapping,
					out taskPriorityMapping,
					out testCaseTypeMapping,
					out testCasePriorityMapping,
					out documentTypeMapping,
					out documentStatusMapping,
					out riskTypeMapping,
					out riskStatusMapping,
					out riskProbabilityMapping,
					out riskImpactMapping,
					// out riskDetectabilityMapping, - TODO once add detectability
					out customPropertyIdMapping,
					out customPropertyValueMapping,
					userId,
					adminSectionId,
					action
					);

				//Get the id of the old and new template projects
				TemplateManager templateManager = new TemplateManager();
				int existingProjectTemplateId = templateManager.RetrieveForProject(existingProjectId).ProjectTemplateId;
				int newProjectTemplateId = templateManager.RetrieveForProject(newProjectId).ProjectTemplateId;
				bool sameTemplates = !createNewTemplate;

				//If the source and destination templates are the same, then we can use the same field values
				//Otherwise we have to create new ones and map them in the dictionaries
				//sameTemplates is the flag to check

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(5, GlobalResources.Messages.Project_Copy_Attachments);
				}

				#region Copy Attachments

				//Now we need to copy across any project attachments (may be attached to artifacts or may not be)
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.ExportAllProjectAttachments(
					userId,
					existingProjectId,
					newProjectId,
					sameTemplates,
					attachmentFolderMapping,
					documentTypeMapping,
					documentStatusMapping,
					attachmentIdsMapping,
					true);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(10, GlobalResources.Messages.Project_Copy_Releases);
				}

				#region Copy Releases

				//***** Now we need to copy across the release list *****
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(existingProjectId, false);
				foreach (ReleaseView release in releases)
				{
					int oldReleaseId = release.ReleaseId;
					int newReleaseId = releaseManager.Insert(
						userId,
						newProjectId,
						release.CreatorId,
						release.Name,
						release.Description,
						release.VersionNumber,
						release.IndentLevel,
						(Release.ReleaseStatusEnum)release.ReleaseStatusId,
						(Release.ReleaseTypeEnum)release.ReleaseTypeId,
						release.StartDate,
						release.EndDate,
						release.ResourceCount,
						release.DaysNonWorking,
						release.OwnerId);

					releaseMapping.Add(oldReleaseId, newReleaseId);

					//We need to make a summary item if necessary
					if (release.IsSummary)
					{
						ReleaseView release2 = releaseManager.RetrieveById2(newProjectId, newReleaseId);
						release2.IsSummary = true;
						release2.IsExpanded = true;
						releaseManager.UpdatePositionalData(userId, new List<ReleaseView>() { release2 });
					}

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, oldReleaseId, newProjectId, newReleaseId, attachmentIdsMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(15, GlobalResources.Messages.Project_Copy_Components);
				}

				#region Copy Components

				//***** Now we need to copy across the project components *****
				ComponentManager componentManager = new ComponentManager();
				List<Component> components = componentManager.Component_Retrieve(existingProjectId);
				if (components != null)
				{
					//Iterate through all the active components, add to destination and map new ID
					foreach (Component component in components)
					{
						int newComponentId = componentManager.Component_Insert(newProjectId, component.Name);
						if (!componentMapping.ContainsKey(component.ComponentId))
						{
							componentMapping.Add(component.ComponentId, newComponentId);
						}
					}
				}

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(25, GlobalResources.Messages.Project_Copy_Components);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(17, GlobalResources.Messages.Project_Copy_Requirements);
				}

				#region Copy Requirements

				//***** Now we need to copy across the requirements matrix *****
				RequirementManager requirementManager = new RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(UserManager.UserInternal, existingProjectId, 1, Int32.MaxValue, null, 0);
				foreach (RequirementView requirement in requirements)
				{
					//Lookup the release in the destination project
					int? releaseId = null;
					if (requirement.ReleaseId.HasValue)
					{
						if (releaseMapping.ContainsKey(requirement.ReleaseId.Value))
						{
							releaseId = releaseMapping[requirement.ReleaseId.Value];
						}
					}

					//Lookup the component in the destination project
					int? componentId = null;
					if (requirement.ComponentId.HasValue)
					{
						if (componentMapping.ContainsKey(requirement.ComponentId.Value))
						{
							componentId = componentMapping[requirement.ComponentId.Value];
						}
					}

					//See if we need to lookup the new id values
					int? requirementTypeId = null;
					int? requirementImportanceId = null;
					if (sameTemplates)
					{
						requirementTypeId = requirement.RequirementTypeId;
						requirementImportanceId = requirement.ImportanceId;
					}
					else
					{
						if (requirementTypeMapping.ContainsKey(requirement.RequirementTypeId))
						{
							requirementTypeId = requirementTypeMapping[requirement.RequirementTypeId];
						}
						if (requirement.ImportanceId.HasValue && requirementImportanceMapping.ContainsKey(requirement.ImportanceId.Value))
						{
							requirementImportanceId = requirementImportanceMapping[requirement.ImportanceId.Value];
						}
					}

					int oldRequirementId = requirement.RequirementId;
					int newRequirementId = requirementManager.Insert(
						userId,
						newProjectId,
						releaseId,
						componentId,
						requirement.IndentLevel,
						(Requirement.RequirementStatusEnum)requirement.RequirementStatusId,
						requirementTypeId,
						requirement.AuthorId,
						requirement.OwnerId,
						requirementImportanceId,
						requirement.Name,
						requirement.Description,
						requirement.EstimatePoints,
						userId
						);
					requirementMapping.Add(oldRequirementId, newRequirementId);

					//We need to make a summary item if necessary
					if (requirement.IsSummary)
					{
						RequirementView requirement2 = requirementManager.RetrieveById(userId, newProjectId, newRequirementId);
						requirement2.IsSummary = true;
						requirement2.IsExpanded = true;
						requirementManager.UpdatePositionalData(userId, new List<RequirementView>() { requirement2 });
					}

					//Copy across the requirement steps (if any)
					requirementManager.CopyScenario(userId, newProjectId, oldRequirementId, newRequirementId);

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, oldRequirementId, newProjectId, newRequirementId, attachmentIdsMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(22, GlobalResources.Messages.Project_Copy_TestCaseFolders);
				}

				#region Copy Test Case Folders

				//***** Now we need to copy across the test case folder hierarchy *****
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.TestCaseFolder_ExportAll(userId, existingProjectId, newProjectId, testCaseFolderMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(24, GlobalResources.Messages.Project_Copy_TestCases);
				}

				#region Copy Test Cases

				//***** Now we need to copy across the test case list *****
				List<TestCaseView> testCases = testCaseManager.Retrieve(existingProjectId, "Name", true, 1, Int32.MaxValue, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				foreach (TestCaseView testCase in testCases)
				{
					int oldTestCaseId = testCase.TestCaseId;
					int? destFolderId = null;
					if (testCase.TestCaseFolderId.HasValue)
					{
						if (testCaseFolderMapping.ContainsKey(testCase.TestCaseFolderId.Value))
						{
							destFolderId = testCaseFolderMapping[testCase.TestCaseFolderId.Value];
						}
					}

					//Lookup the component in the destination project
					List<int> newComponentIds = null;
					if (!string.IsNullOrWhiteSpace(testCase.ComponentIds))
					{
						newComponentIds = new List<int>();
						List<int> oldComponentIds = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
						foreach (int oldComponentId in oldComponentIds)
						{
							if (componentMapping.ContainsKey(oldComponentId))
							{
								int newComponentId = componentMapping[oldComponentId];
								newComponentIds.Add(newComponentId);
							}
						}
					}

					int? destAutomationAttachmentId = null;
					if (testCase.AutomationAttachmentId.HasValue && attachmentIdsMapping.ContainsKey(testCase.AutomationAttachmentId.Value))
					{
						destAutomationAttachmentId = attachmentIdsMapping[testCase.AutomationAttachmentId.Value];
					}

					//See if we need to lookup the new id values
					int? testCaseTypeId = null;
					int? testCasePriorityId = null;
					if (sameTemplates)
					{
						testCaseTypeId = testCase.TestCaseTypeId;
						testCasePriorityId = testCase.TestCasePriorityId;
					}
					else
					{
						if (testCaseTypeMapping.ContainsKey(testCase.TestCaseTypeId))
						{
							testCaseTypeId = testCaseTypeMapping[testCase.TestCaseTypeId];
						}
						if (testCase.TestCasePriorityId.HasValue && testCasePriorityMapping.ContainsKey(testCase.TestCasePriorityId.Value))
						{
							testCasePriorityId = testCasePriorityMapping[testCase.TestCasePriorityId.Value];
						}
					}

					int newTestCaseId = testCaseManager.Insert(
						userId,
						newProjectId,
						testCase.AuthorId,
						testCase.OwnerId,
						testCase.Name,
						testCase.Description,
						testCaseTypeId,
						(TestCase.TestCaseStatusEnum)testCase.TestCaseStatusId,
						testCasePriorityId,
						destFolderId,
						testCase.EstimatedDuration,
						testCase.AutomationEngineId,
						destAutomationAttachmentId,
						true,
						false,
						newComponentIds
						);
					testCaseMapping.Add(oldTestCaseId, newTestCaseId);

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, oldTestCaseId, newProjectId, newTestCaseId, attachmentIdsMapping);

					//***** Now we need to copy across any test case parameters *****
					List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(oldTestCaseId);
					foreach (TestCaseParameter testCaseParameter in testCaseParameters)
					{
						int newTestCaseParameterId = testCaseManager.InsertParameter(
							newProjectId,
							newTestCaseId,
							testCaseParameter.Name,
							testCaseParameter.DefaultValue, userId);

						if (!testCaseParameterMapping.ContainsKey(testCaseParameter.TestCaseParameterId))
						{
							testCaseParameterMapping.Add(testCaseParameter.TestCaseParameterId, newTestCaseParameterId);
						}
					}
				}

				//***** Now we need to copy across the test steps and linked test cases*****
				foreach (KeyValuePair<int, int> testCaseMappingEntry in testCaseMapping)
				{
					int oldTestCaseId = testCaseMappingEntry.Key;
					int newTestCaseId = testCaseMappingEntry.Value;
					List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(oldTestCaseId);
					foreach (TestStepView testStep in testSteps)
					{
						int oldTestStepId = testStep.TestStepId;
						//Handle the two cases of the test step and linked test case
						if (testStep.LinkedTestCaseId.HasValue)
						{
							//Lookup the linked test case
							int oldLinkedTestCaseId = testStep.LinkedTestCaseId.Value;
							int newLinkedTestCaseId;
							if (testCaseMapping.TryGetValue(oldLinkedTestCaseId, out newLinkedTestCaseId))
							{
								//Get the parameter values
								Dictionary<string, string> parameters = new Dictionary<string, string>();
								List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);
								foreach (TestStepParameter testStepParameterValue in testStepParameterValues)
								{
									parameters.Add(testStepParameterValue.Name, testStepParameterValue.Value);
								}

								testCaseManager.InsertLink(
									userId,
									newTestCaseId,
									null,
									newLinkedTestCaseId,
									parameters);
							}
						}
						else
						{
							int newTestStepId = testCaseManager.InsertStep(
								userId,
								newTestCaseId,
								testStep.Position,
								testStep.Description,
								testStep.ExpectedResult,
								testStep.SampleData,
								false,
								testStep.ExecutionStatusId);

							testStepMapping.Add(oldTestStepId, newTestStepId);

							//Copy across the links to any attachments
							attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, oldTestStepId, newProjectId, newTestStepId, attachmentIdsMapping);
						}
					}
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(29, GlobalResources.Messages.Project_Copy_AutomationHosts);
				}

				#region Copy Automation Hosts

				//***** Now we need to copy across the automation hosts list *****
				Business.AutomationManager automationManager = new Business.AutomationManager();
				List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(existingProjectId);
				foreach (AutomationHostView automationHost in automationHosts)
				{
					//Insert the new host, getting the new ID
					int newAutomationHostId = automationManager.InsertHost(
						newProjectId,
						automationHost.Name,
						automationHost.Token,
						automationHost.Description,
						automationHost.IsActive,
						userId
						);

					//Add to the mapping
					if (!automationHostMapping.ContainsKey(automationHost.AutomationHostId))
					{
						automationHostMapping.Add(automationHost.AutomationHostId, newAutomationHostId);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(32, GlobalResources.Messages.Project_Copy_TestConfigurations);
				}

				#region Copy Test Configurations

				//***** Now we need to copy across the test configuration sets *****
				TestConfigurationManager testConfigurationManager = new TestConfigurationManager();
				List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(existingProjectId);
				foreach (TestConfigurationSet testConfigurationSet in testConfigurationSets)
				{
					//Insert the new set, getting the new ID
					int newTestConfigurationSetId = testConfigurationManager.InsertSet(
						newProjectId,
						testConfigurationSet.Name,
						testConfigurationSet.Description,
						testConfigurationSet.IsActive,
						userId
						);

					//Add to the mapping
					if (!testConfigurationSetMapping.ContainsKey(testConfigurationSet.TestConfigurationSetId))
					{
						testConfigurationSetMapping.Add(testConfigurationSet.TestConfigurationSetId, newTestConfigurationSetId);
					}

					//Also we need to copy across the test configuration entries, mapping the test parameters and custom list values
					testConfigurationManager.CopyEntries(testConfigurationSet.TestConfigurationSetId, newTestConfigurationSetId, sameTemplates, testCaseParameterMapping, customPropertyValueMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(35, GlobalResources.Messages.Project_Copy_TestSetFolders);
				}

				#region Copy Test Set Folders

				//***** Now we need to copy across the test set folder hierarchy *****
				TestSetManager testSetManager = new TestSetManager();
				testSetManager.TestSetFolder_ExportAll(userId, existingProjectId, newProjectId, testSetFolderMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(37, GlobalResources.Messages.Project_Copy_TestSets);
				}

				#region Copy Test Sets

				//***** Now we need to copy across the test set list *****
				List<TestSetView> testSets = testSetManager.Retrieve(existingProjectId, "Name", true, 1, Int32.MaxValue, null, 0, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				foreach (TestSetView testSet in testSets)
				{
					//Lookup release, folder and test run type
					int? releaseId = null;
					if (testSet.ReleaseId.HasValue)
					{
						if (releaseMapping.ContainsKey(testSet.ReleaseId.Value))
						{
							releaseId = releaseMapping[testSet.ReleaseId.Value];
						}
					}

					TestRun.TestRunTypeEnum testRunType = TestRun.TestRunTypeEnum.Manual;
					if (testSet.TestRunTypeId.HasValue)
					{
						testRunType = (TestRun.TestRunTypeEnum)testSet.TestRunTypeId.Value;
					}

					//See if we have a mapped automation host
					int? automationHostId = null;
					if (testSet.AutomationHostId.HasValue && automationHostMapping.ContainsKey(testSet.AutomationHostId.Value))
					{
						automationHostId = automationHostMapping[testSet.AutomationHostId.Value];
					}

					//See if we have a mapped test configuration set
					int? testConfigurationSetId = null;
					if (testSet.TestConfigurationSetId.HasValue && testConfigurationSetMapping.ContainsKey(testSet.TestConfigurationSetId.Value))
					{
						testConfigurationSetId = testConfigurationSetMapping[testSet.TestConfigurationSetId.Value];
					}

					Nullable<TestSet.RecurrenceEnum> recurrence = null;
					if (testSet.RecurrenceId.HasValue)
					{
						recurrence = (TestSet.RecurrenceEnum)testSet.RecurrenceId;
					}

					int? destFolderId = null;
					if (testSet.TestSetFolderId.HasValue)
					{
						if (testSetFolderMapping.ContainsKey(testSet.TestSetFolderId.Value))
						{
							destFolderId = testSetFolderMapping[testSet.TestSetFolderId.Value];
						}
					}

					int oldTestSetId = testSet.TestSetId;

					//The test sets are always initially in 'not-started' mode in the new project
					int newTestSetId = testSetManager.Insert(
						userId,
						newProjectId,
						destFolderId,
						releaseId,
						testSet.CreatorId,
						testSet.OwnerId,
						(TestSet.TestSetStatusEnum)testSet.TestSetStatusId,
						testSet.Name,
						testSet.Description,
						testSet.PlannedDate,
						testRunType,
						automationHostId,
						recurrence,
						testSet.IsAutoScheduled,
						testSet.IsDynamic,
						testSet.BuildExecuteTimeInterval,
						testSet.DynamicQuery,
						testConfigurationSetId
						);

					testSetMapping.Add(oldTestSetId, newTestSetId);

					//Export any test set parameter values
					List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(oldTestSetId);
					foreach (TestSetParameter testSetParameter in testSetParameterValues)
					{
						if (testCaseParameterMapping.ContainsKey(testSetParameter.TestCaseParameterId))
						{
							int newTestCaseParameterId = testCaseParameterMapping[testSetParameter.TestCaseParameterId];
							testSetManager.AddTestSetParameter(newTestSetId, newTestCaseParameterId, testSetParameter.Value,newProjectId,userId);
						}
					}

					//Now we need to copy across the list of mapped test cases
					List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(oldTestSetId);
					int copiedTestCaseId;
					foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
					{
						if (testCaseMapping.TryGetValue(testSetTestCase.TestCaseId, out copiedTestCaseId))
						{
							//We always use the default owner for the test cases
							Dictionary<int, int> newTestCaseInfo = testSetManager.AddTestCases(newProjectId, newTestSetId, new List<int>() { copiedTestCaseId }, null,null, userId);
							int newTestSetTestCaseId = newTestCaseInfo.First().Value;

							//Add any test set parameter values
							List<TestSetTestCaseParameter> newTestSetTestCaseParameterValues = new List<TestSetTestCaseParameter>();
							List<TestSetTestCaseParameter> testSetTestCaseParameterValues = testSetManager.RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);
							foreach (TestSetTestCaseParameter testSetTestCaseParameter in testSetTestCaseParameterValues)
							{
								//testSetTestCase.TestSetTestCaseId
								if (testCaseParameterMapping.ContainsKey(testSetTestCaseParameter.TestCaseParameterId))
								{
									TestSetTestCaseParameter newTestSetTestCaseParameter = new TestSetTestCaseParameter();
									newTestSetTestCaseParameter.TestCaseParameterId = testCaseParameterMapping[testSetTestCaseParameter.TestCaseParameterId];
									newTestSetTestCaseParameter.TestSetTestCaseId = newTestSetTestCaseId;
									newTestSetTestCaseParameter.Value = testSetTestCaseParameter.Value;
									newTestSetTestCaseParameterValues.Add(newTestSetTestCaseParameter);
								}
							}
							testSetManager.SaveTestCaseParameterValues(newTestSetTestCaseParameterValues);
						}
					}

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, oldTestSetId, newProjectId, newTestSetId, attachmentIdsMapping);
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(42, GlobalResources.Messages.Project_Copy_Mappings);
				}

				#region Copy Requirement/Test Case/Release Mapping

				//***** Now we need to copy across the requirements' test case coverage and release mapping *****
				foreach (KeyValuePair<int, int> testCaseMappingEntry in testCaseMapping)
				{
					int oldTestCaseId = testCaseMappingEntry.Key;
					int newTestCaseId = testCaseMappingEntry.Value;

					//Load in the old project's coverage information and get the available and covered for this new test case
					requirements = requirementManager.RetrieveCoveredByTestCaseId(userId, existingProjectId, oldTestCaseId);
					List<int> newRequirementsList = new List<int>();
					foreach (RequirementView requirement in requirements)
					{
						int oldRequirementId = requirement.RequirementId;
						int newRequirementId;
						if (requirementMapping.TryGetValue(oldRequirementId, out newRequirementId))
						{
							newRequirementsList.Add(newRequirementId);
						}
					}
					requirementManager.AddToTestCase(newProjectId, newTestCaseId, newRequirementsList, userId);

					//Load in the old project's mapping information and get the available and mapped for this new test case
					List<ReleaseView> oldReleases = releaseManager.RetrieveMappedByTestCaseId(userId, existingProjectId, oldTestCaseId);
					List<int> newReleasesList = new List<int>();
					foreach (ReleaseView oldRelease in oldReleases)
					{
						int oldReleaseId = oldRelease.ReleaseId;
						int newReleaseId;
						if (releaseMapping.TryGetValue(oldReleaseId, out newReleaseId))
						{
							newReleasesList.Add(newReleaseId);
						}
					}
					releaseManager.AddToTestCase(newProjectId, newTestCaseId, newReleasesList, userId);
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(47, GlobalResources.Messages.Project_Copy_Incidents);
				}

				#region Copy Incident

				//***** Copy across the incident list *****
				IncidentManager incidentManager = new IncidentManager();
				int incidentCount = incidentManager.Count(existingProjectId, null, 0);
				for (int i = 1; i <= incidentCount; i += PAGE_SIZE)
				{
					List<IncidentView> incidents = incidentManager.Retrieve(existingProjectId, "IncidentId", true, i, PAGE_SIZE, null, 0);
					foreach (IncidentView incident in incidents)
					{
						//Lookup the release ids
						int? detectedReleaseId = null;
						int? resolvedReleaseId = null;
						int? verifiedReleaseId = null;
						if (incident.DetectedReleaseId.HasValue && releaseMapping.ContainsKey(incident.DetectedReleaseId.Value))
						{
							detectedReleaseId = releaseMapping[incident.DetectedReleaseId.Value];
						}
						if (incident.ResolvedReleaseId.HasValue && releaseMapping.ContainsKey(incident.ResolvedReleaseId.Value))
						{
							resolvedReleaseId = releaseMapping[incident.ResolvedReleaseId.Value];
						}
						if (incident.VerifiedReleaseId.HasValue && releaseMapping.ContainsKey(incident.VerifiedReleaseId.Value))
						{
							verifiedReleaseId = releaseMapping[incident.VerifiedReleaseId.Value];
						}

						//Lookup the component in the destination project
						List<int> newComponentIds = null;
						if (!string.IsNullOrWhiteSpace(incident.ComponentIds))
						{
							newComponentIds = new List<int>();
							List<int> oldComponentIds = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
							foreach (int oldComponentId in oldComponentIds)
							{
								if (componentMapping.ContainsKey(oldComponentId))
								{
									int newComponentId = componentMapping[oldComponentId];
									newComponentIds.Add(newComponentId);
								}
							}
						}

						//See if we need to lookup the new id values
						int? incidentTypeId = null;
						int? incidentStatusId = null;
						int? incidentPriorityId = null;
						int? incidentSeverityId = null;
						if (sameTemplates)
						{
							incidentTypeId = incident.IncidentTypeId;
							incidentStatusId = incident.IncidentStatusId;
							incidentPriorityId = incident.PriorityId;
							incidentSeverityId = incident.SeverityId;
						}
						else
						{
							if (incidentTypeMapping.ContainsKey(incident.IncidentTypeId))
							{
								incidentTypeId = incidentTypeMapping[incident.IncidentTypeId];
							}
							if (incidentStatusMapping.ContainsKey(incident.IncidentStatusId))
							{
								incidentStatusId = incidentStatusMapping[incident.IncidentStatusId];
							}
							if (incident.PriorityId.HasValue && incidentPriorityMapping.ContainsKey(incident.PriorityId.Value))
							{
								incidentPriorityId = incidentPriorityMapping[incident.PriorityId.Value];
							}
							if (incident.SeverityId.HasValue && incidentSeverityMapping.ContainsKey(incident.SeverityId.Value))
							{
								incidentSeverityId = incidentSeverityMapping[incident.SeverityId.Value];
							}
						}

						//Actually perform the insert of the copy
						int newIncidentId = incidentManager.Insert(
							newProjectId,
							incidentPriorityId,
							incidentSeverityId,
							incident.OpenerId,
							incident.OwnerId,
							null, /* handled in the test run section */
							incident.Name,
							incident.Description,
							detectedReleaseId,
							resolvedReleaseId,
							verifiedReleaseId,
							incidentTypeId,
							incidentStatusId,
							incident.CreationDate,
							incident.StartDate,
							incident.ClosedDate,
							incident.EstimatedEffort,
							incident.ActualEffort,
							incident.RemainingEffort,
							null,
							newComponentIds
							);

						//Add to the mapping
						if (!incidentMapping.ContainsKey(incident.IncidentId))
						{
							incidentMapping.Add(incident.IncidentId, newIncidentId);
						}

						//Copy across any resolutions (discussions) - this is the only artifact that handles discussions in the copy region
						incidentManager.CopyResolutions(incident.IncidentId, newIncidentId);

						//Copy across the links to any attachments
						attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incident.IncidentId, newProjectId, newIncidentId, attachmentIdsMapping);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(52, GlobalResources.Messages.Project_Copy_Risks);
				}

				#region Copy Risks

				//***** Copy across the risk list *****
				RiskManager riskManager = new RiskManager();
				int riskCount = riskManager.Risk_Count(existingProjectId, null, 0);
				for (int i = 1; i <= riskCount; i += PAGE_SIZE)
				{
					List<RiskView> risks = riskManager.Risk_Retrieve(existingProjectId, "RiskId", true, i, PAGE_SIZE, null, 0);
					foreach (RiskView risk in risks)
					{
						//Lookup the release id
						int? releaseId = null;
						if (risk.ReleaseId.HasValue && releaseMapping.ContainsKey(risk.ReleaseId.Value))
						{
							releaseId = releaseMapping[risk.ReleaseId.Value];
						}

						//Lookup the component
						int? componentId = null;
						if (risk.ComponentId.HasValue)
						{
							if (componentMapping.ContainsKey(risk.ComponentId.Value))
							{
								componentId = componentMapping[risk.ComponentId.Value];
							}
						}

						//See if we need to lookup the new id values
						int? riskTypeId = null;
						int? riskStatusId = null;
						int? riskProbabilityId = null;
						int? riskImpactId = null;
						int? riskDetectabilityId = null;
						if (sameTemplates)
						{
							riskTypeId = risk.RiskTypeId;
							riskStatusId = risk.RiskStatusId;
							riskProbabilityId = risk.RiskProbabilityId;
							riskImpactId = risk.RiskImpactId;
							riskDetectabilityId = risk.RiskDetectabilityId;
						}
						else
						{
							if (riskTypeMapping.ContainsKey(risk.RiskTypeId))
							{
								riskTypeId = riskTypeMapping[risk.RiskTypeId];
							}
							if (riskStatusMapping.ContainsKey(risk.RiskStatusId))
							{
								riskStatusId = riskStatusMapping[risk.RiskStatusId];
							}
							if (risk.RiskProbabilityId.HasValue && riskProbabilityMapping.ContainsKey(risk.RiskProbabilityId.Value))
							{
								riskProbabilityId = riskProbabilityMapping[risk.RiskProbabilityId.Value];
							}
							if (risk.RiskImpactId.HasValue && riskImpactMapping.ContainsKey(risk.RiskImpactId.Value))
							{
								riskImpactId = riskImpactMapping[risk.RiskImpactId.Value];
							}
							//TODO - when add detectability need to make sure to set the values here
							//if (risk.RiskDetectabilityId.HasValue && riskDetectabilityMapping.ContainsKey(risk.RiskDetectabilityId.Value))
							//{
							//	riskDetectabilityId = riskProbabilityMapping[risk.RiskDetectabilityId.Value];
							//}
						}

						//Actually perform the insert of the copy
						int newRiskId = riskManager.Risk_Insert(
							newProjectId,
							riskStatusId,
							riskTypeId,
							riskProbabilityId,
							riskImpactId,
							risk.CreatorId,
							risk.OwnerId,
							risk.Name,
							risk.Description,
							releaseId,
							componentId,
							risk.CreationDate,
							risk.ReviewDate,
							risk.ClosedDate
							);

						//Add to the mapping
						if (!riskMapping.ContainsKey(risk.RiskId))
						{
							riskMapping.Add(risk.RiskId, newRiskId);
						}

						//Copy across any mitigations
						List<RiskMitigation> mitigations = riskManager.RiskMitigation_Retrieve(risk.RiskId);
						if (mitigations.Count() > 0)
						{
							foreach (RiskMitigation mitigation in mitigations)
							{
								riskManager.RiskMitigation_Insert(
									newProjectId,
									newRiskId,
									null,
									mitigation.Description,
									risk.CreatorId,
									mitigation.CreationDate,
									mitigation.ReviewDate
									);
							}
						}

						//Copy across the links to any attachments
						attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Risk, risk.RiskId, newProjectId, newRiskId, attachmentIdsMapping);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(57, GlobalResources.Messages.Project_Copy_TaskFolders);
				}

				#region Copy Task Folders

				//***** First we need to copy across the task folders *****
				TaskManager taskManager = new TaskManager();
				List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(existingProjectId);
				foreach (TaskFolderHierarchyView taskFolder in taskFolders)
				{
					int? parentTaskFolderId = null;
					if (taskFolder.ParentTaskFolderId.HasValue && taskFolderMapping.ContainsKey(taskFolder.ParentTaskFolderId.Value))
					{
						parentTaskFolderId = taskFolderMapping[taskFolder.ParentTaskFolderId.Value];
					}
					int newTaskFolderId = taskManager.TaskFolder_Create(taskFolder.Name, newProjectId, parentTaskFolderId).TaskFolderId;

					//Add to the mapping
					if (!taskFolderMapping.ContainsKey(taskFolder.TaskFolderId))
					{
						taskFolderMapping.Add(taskFolder.TaskFolderId, newTaskFolderId);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(59, GlobalResources.Messages.Project_Copy_Tasks);
				}

				#region Copy Tasks

				//***** Now we need to copy across the task list *****
				int taskCount = taskManager.Count(existingProjectId, null, 0);
				for (int i = 1; i <= taskCount; i += PAGE_SIZE)
				{
					List<TaskView> tasks = taskManager.Retrieve(existingProjectId, "TaskId", true, i, PAGE_SIZE, null, 0);
					foreach (TaskView task in tasks)
					{
						//Lookup the release and requirement ids
						int? requirementId = null;
						if (task.RequirementId.HasValue && requirementMapping.ContainsKey(task.RequirementId.Value))
						{
							requirementId = requirementMapping[task.RequirementId.Value];
						}
						int? releaseId = null;
						if (task.ReleaseId.HasValue && releaseMapping.ContainsKey(task.ReleaseId.Value))
						{
							releaseId = releaseMapping[task.ReleaseId.Value];
						}
						int? riskId = null;
						if (task.RiskId.HasValue && riskMapping.ContainsKey(task.RiskId.Value))
						{
							riskId = riskMapping[task.RiskId.Value];
						}

						//Lookup the folder id
						int? taskFolderId = null;
						if (task.TaskFolderId.HasValue && taskFolderMapping.ContainsKey(task.TaskFolderId.Value))
						{
							taskFolderId = taskFolderMapping[task.TaskFolderId.Value];
						}

						//See if we need to lookup the new id values
						int? taskTypeId = null;
						int? taskPriorityId = null;
						if (sameTemplates)
						{
							taskTypeId = task.TaskTypeId;
							taskPriorityId = task.TaskPriorityId;
						}
						else
						{
							if (taskTypeMapping.ContainsKey(task.TaskTypeId))
							{
								taskTypeId = taskTypeMapping[task.TaskTypeId];
							}
							if (task.TaskPriorityId.HasValue && taskPriorityMapping.ContainsKey(task.TaskPriorityId.Value))
							{
								taskPriorityId = taskPriorityMapping[task.TaskPriorityId.Value];
							}
						}

						//Actually perform the insert of the copy
						int newTaskId = taskManager.Insert(
							newProjectId,
							task.CreatorId,
							(Task.TaskStatusEnum)task.TaskStatusId,
							taskTypeId,
							taskFolderId,
							requirementId,
							releaseId,
							task.OwnerId,
							taskPriorityId,
							task.Name,
							task.Description,
							task.StartDate,
							task.EndDate,
							task.EstimatedEffort,
							task.ActualEffort,
							task.RemainingEffort,
							userId,
							true,
							riskId
							);

						//Add to the mapping
						if (!taskMapping.ContainsKey(task.TaskId))
						{
							taskMapping.Add(task.TaskId, newTaskId);
						}

						//Copy across the links to any attachments
						attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, task.TaskId, newProjectId, newTaskId, attachmentIdsMapping);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(64, GlobalResources.Messages.Project_Copy_TestRuns);
				}

				#region Copy Test Runs

				//***** copy across the test run list *****
				TestRunManager testRunManager = new TestRunManager();
				List<TestRunView> testRuns = testRunManager.Retrieve(existingProjectId, "TestRunId", true, 1, Int32.MaxValue, null, 0);
				foreach (TestRunView testRun in testRuns)
				{
					//Lookup specific fields that are specific to new project
					TestRun.TestRunFormatEnum testRunFormatId = TestRun.TestRunFormatEnum.PlainText;
					if (testRun.TestRunFormatId.HasValue)
					{
						testRunFormatId = (TestRun.TestRunFormatEnum)testRun.TestRunFormatId.Value;
					}
					DateTime endDate = testRun.StartDate;
					if (testRun.EndDate.HasValue)
					{
						endDate = testRun.EndDate.Value;
					}
					int? testCaseId = null;
					if (testCaseMapping.ContainsKey(testRun.TestCaseId))
					{
						testCaseId = testCaseMapping[testRun.TestCaseId];
					}

					int? testSetId = null;
					int? testSetTestCaseId = null;
					if (testRun.TestSetId.HasValue && testSetMapping.ContainsKey(testRun.TestSetId.Value))
					{
						testSetId = testSetMapping[testRun.TestSetId.Value];
						List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(testSetId.Value);
						TestSetTestCaseView testSetTestCaseMatch = testSetTestCases.Where(testCase => testCase.TestCaseId == testCaseId).FirstOrDefault();
						if (testSetTestCaseMatch != null)
						{
							testSetTestCaseId = testSetTestCaseMatch.TestSetTestCaseId;
						}
					}

					int? releaseId = null;
					if (testRun.ReleaseId.HasValue && releaseMapping.ContainsKey(testRun.ReleaseId.Value))
					{
						releaseId = releaseMapping[testRun.ReleaseId.Value];
					}
					int? automationHostId = null;
					if (testRun.AutomationHostId.HasValue && automationHostMapping.ContainsKey(testRun.AutomationHostId.Value))
					{
						automationHostId = automationHostMapping[testRun.AutomationHostId.Value];
					}

					//Clone test run steps				
					//We retrieve the step view list to check for incident count, but save the data as a TestRunStep
					List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(testRun.TestRunId);
					List<TestRunStep> newTestRunSteps = new List<TestRunStep>();
					if (testRunSteps != null && testRunSteps.Count > 0)
					{
						foreach (TestRunStepView testRunStep in testRunSteps)
						{
							TestRunStep newTestRunStep = new TestRunStep();
							newTestRunStep.ActualDuration = testRunStep.ActualDuration;
							newTestRunStep.ActualResult = testRunStep.ActualResult;
							newTestRunStep.Description = testRunStep.Description;
							newTestRunStep.EndDate = testRunStep.EndDate;
							newTestRunStep.ExecutionStatusId = testRunStep.ExecutionStatusId;
							newTestRunStep.ExpectedResult = testRunStep.ExpectedResult;
							newTestRunStep.Position = testRunStep.Position;
							newTestRunStep.SampleData = testRunStep.SampleData;
							newTestRunStep.StartDate = testRunStep.StartDate;

							//Lookup specific fields that are specific to new project
							//Match up to the new test case
							if (testRunStep.TestCaseId.HasValue && testCaseMapping.ContainsKey(testRunStep.TestCaseId.Value))
							{
								newTestRunStep.TestCaseId = testCaseMapping[testRunStep.TestCaseId.Value];
							}
							//Match up to the new test step
							if (testRunStep.TestStepId.HasValue && testStepMapping.ContainsKey(testRunStep.TestStepId.Value))
							{
								newTestRunStep.TestStepId = testStepMapping[testRunStep.TestStepId.Value];
							}
							newTestRunSteps.Add(newTestRunStep);
						}
					}

					int newTestRunId = testRunManager.TestRun_Insert(
						newProjectId,
						testRun.TesterId,
						(int)testCaseId,
						releaseId,
						testSetId,
						testSetTestCaseId,
						testRun.StartDate,
						endDate,
						testRun.ExecutionStatusId,
						(TestRun.TestRunTypeEnum)testRun.TestRunTypeId,
						testRun.RunnerName,
						testRun.RunnerTestName,
						testRun.RunnerAssertCount,
						testRun.RunnerMessage,
						testRun.RunnerStackTrace,
						automationHostId,
						testRun.AutomationEngineId,
						null,
						testRunFormatId,
						newTestRunSteps,
						false,
						false
						);
					testRunMapping.Add(testRun.TestRunId, newTestRunId);

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRun.TestRunId, newProjectId, newTestRunId, attachmentIdsMapping);

					//Copy across any links to incidents
					TestRun newTestRun = testRunManager.RetrieveByIdWithSteps(newTestRunId);
					foreach (TestRunStepView testRunStep in testRunSteps)
					{
						if (testRunStep.IncidentCount > 0)
						{
							//Getting the mapped incidents ids as a list
							List<IncidentView> testRunincidents = incidentManager.RetrieveByTestRunStepId(testRunStep.TestRunStepId);
							List<int> incidentIds = new List<int>();
							foreach (IncidentView incident in testRunincidents)
							{
								//Retrieve the matching incident and add to the new test run step
								if (incidentMapping.ContainsKey(incident.IncidentId))
								{
									incidentIds.Add(incidentMapping[incident.IncidentId]);
								}
							}
							//Match the old testrunstep id to the new one
							if (newTestRun.TestRunSteps.Count > 0)
							{
								List<TestRunStep> stepsMatchPosition = newTestRun.TestRunSteps.Where(step => step.Position == testRunStep.Position).ToList();
								if (stepsMatchPosition != null && stepsMatchPosition.Count > 0)
								{
									int newTestRunStepId = stepsMatchPosition.Select(step => step.TestRunStepId).FirstOrDefault();
									incidentManager.Incident_AssociateToTestRunStep(newProjectId, newTestRunStepId, incidentIds, testRun.TesterId);
								}
							}
						}
					}
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(70, GlobalResources.Messages.Project_Copy_CustomProperties);
				}

				#region Copy Custom Properties

				//Now we need to copy across all the custom properties assigned to all Requirements in the project
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Requirement, requirementMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Cases
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestCase, testCaseMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Steps
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestStep, testStepMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Releases
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Release, releaseMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Sets
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestSet, testSetMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Tasks
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Task, taskMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Documents
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Document, attachmentIdsMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Automation Hosts
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Incidents
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Incident, incidentMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Risks
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Risk, riskMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Runs
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestRun, testRunMapping, customPropertyValueMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(85, GlobalResources.Messages.Project_Copy_Associations);
				}

				#region Copy Associations 
				// within the project only, not cross project

				//Requirements
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Requirement, ArtifactTypeEnum.Requirement, requirementMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Requirement, ArtifactTypeEnum.Incident, requirementMapping, incidentMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Requirement, ArtifactTypeEnum.Risk, requirementMapping, riskMapping);

				//Test cases
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestCase, ArtifactTypeEnum.Task, testCaseMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestCase, ArtifactTypeEnum.Risk, testCaseMapping, riskMapping);

				//Test step
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestStep, ArtifactTypeEnum.Requirement, testStepMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestStep, ArtifactTypeEnum.Incident, testStepMapping, incidentMapping);

				//Incidents
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Incident, ArtifactTypeEnum.Requirement, incidentMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Incident, ArtifactTypeEnum.TestStep, incidentMapping, testStepMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Incident, ArtifactTypeEnum.Task, incidentMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Incident, ArtifactTypeEnum.Incident, incidentMapping, incidentMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Incident, ArtifactTypeEnum.Risk, incidentMapping, riskMapping);

				//Risks
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Task, riskMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Requirement, riskMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Risk, riskMapping, riskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.TestCase, riskMapping, testCaseMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Incident, riskMapping, incidentMapping);

				//Tasks
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.Incident, taskMapping, incidentMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.Task, taskMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.TestCase, taskMapping, testCaseMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.TestRun, taskMapping, testRunMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(93, GlobalResources.Messages.Project_Copy_Comments);
				}

				#region Copy Discussions
				CopyArtifactDiscussions(newProjectId, attachmentIdsMapping, DataModel.Artifact.ArtifactTypeEnum.Document);
				CopyArtifactDiscussions(newProjectId, releaseMapping, DataModel.Artifact.ArtifactTypeEnum.Release);
				CopyArtifactDiscussions(newProjectId, requirementMapping, DataModel.Artifact.ArtifactTypeEnum.Requirement);
				CopyArtifactDiscussions(newProjectId, riskMapping, DataModel.Artifact.ArtifactTypeEnum.Risk);
				CopyArtifactDiscussions(newProjectId, taskMapping, DataModel.Artifact.ArtifactTypeEnum.Task);
				CopyArtifactDiscussions(newProjectId, testCaseMapping, DataModel.Artifact.ArtifactTypeEnum.TestCase);
				CopyArtifactDiscussions(newProjectId, testSetMapping, DataModel.Artifact.ArtifactTypeEnum.TestSet);
				//NOTE: incident discussions are handled in the incident section because they work differently

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(100, "");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newProjectId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Clones and resets an existing project into a new project
		/// </summary>
		/// <param name="existingProjectId">The project to be copied</param>
		/// <param name="userId">The user we're performing the operation as</param>
		/// <param name="updateBackgroundProcessStatus">Callback used to report back the status of the function</param>
		/// <param name="createNewTemplate">Should we create a new template (default = False)</param>
		/// <returns>The id of the copied project</returns>
		/// <remarks>
		/// Copies the project meta-data using CreateFrom() and
		/// copies releases, requirements, documents, test cases, steps, sets, configurations, automation hosts, tasks (dates forced to be blank), risks (without mitigations) and
		/// copies all comments, attachment links, test coverage and relevant associations
		/// all statuses are reset and all execution statues are reset
		/// Cloning can potentially use the same or different templates, so need to account for that.
		/// </remarks>
		public int CopyReset(int userId, int existingProjectId, bool createNewTemplate = false, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "CopyReset";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PAGE_SIZE = 100;  //Copy in batches of 100

			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(0, GlobalResources.Messages.Project_Copy_CreateShell);
				}

				//Create the mapping dictionaries for storing old vs new IDs
				Dictionary<int, int> requirementMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseMapping = new Dictionary<int, int>();
				Dictionary<int, int> testStepMapping = new Dictionary<int, int>();
				Dictionary<int, int> releaseMapping = new Dictionary<int, int>();
				Dictionary<int, int> testSetFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> testSetMapping = new Dictionary<int, int>();
				Dictionary<int, int> automationHostMapping = new Dictionary<int, int>();
				Dictionary<int, int> componentMapping = new Dictionary<int, int>();
				Dictionary<int, int> taskFolderMapping = new Dictionary<int, int>();
				Dictionary<int, int> taskMapping = new Dictionary<int, int>();
				Dictionary<int, int> testCaseParameterMapping = new Dictionary<int, int>();
				Dictionary<int, int> testConfigurationSetMapping = new Dictionary<int, int>();
				Dictionary<int, int> riskMapping = new Dictionary<int, int>();

				//First we need to retrieve the existing project
				Project project = this.RetrieveById(existingProjectId);
				if (project == null)
				{
					throw new ArtifactNotExistsException(string.Format(GlobalResources.Messages.Project_ArtifactNotExists, existingProjectId));
				}

				//Get the project details
				string projectName = project.Name + CopiedArtifactNameSuffix;

				//Create a copy of this project with the same user membership, workflows and incident lists
				Dictionary<int, int> attachmentFolderMapping;
				Dictionary<int, int> incidentStatusMapping;
				Dictionary<int, int> incidentTypeMapping;
				Dictionary<int, int> incidentPriorityMapping;
				Dictionary<int, int> incidentSeverityMapping;
				Dictionary<int, int> requirementTypeMapping;
				Dictionary<int, int> requirementImportanceMapping;
				Dictionary<int, int> taskTypeMapping;
				Dictionary<int, int> taskPriorityMapping;
				Dictionary<int, int> testCaseTypeMapping;
				Dictionary<int, int> testCasePriorityMapping;
				Dictionary<int, int> documentTypeMapping;
				Dictionary<int, int> documentStatusMapping;
				Dictionary<int, int> riskStatusMapping;
				Dictionary<int, int> riskTypeMapping;
				Dictionary<int, int> riskProbabilityMapping;
				Dictionary<int, int> riskImpactMapping;
				//Dictionary<int, int> riskDetectabilityMapping; - TODO once add detectability
				Dictionary<int, int> customPropertyIdMapping;
				Dictionary<int, int> customPropertyValueMapping;
				Dictionary<int, int> attachmentIdsMapping = new Dictionary<int, int>();
				int newProjectId = CreateFromExisting(projectName,
					project.Description,
					project.Website,
					existingProjectId,
					project.IsActive,
					createNewTemplate,
					out attachmentFolderMapping,
					out incidentStatusMapping,
					out incidentTypeMapping,
					out incidentPriorityMapping,
					out incidentSeverityMapping,
					out requirementTypeMapping,
					out requirementImportanceMapping,
					out taskTypeMapping,
					out taskPriorityMapping,
					out testCaseTypeMapping,
					out testCasePriorityMapping,
					out documentTypeMapping,
					out documentStatusMapping,
					out riskTypeMapping,
					out riskStatusMapping,
					out riskProbabilityMapping,
					out riskImpactMapping,
					// out riskDetectabilityMapping, - TODO once add detectability
					out customPropertyIdMapping,
					out customPropertyValueMapping,
					userId,
					adminSectionId,
					action
					);

				//Get the id of the old and new template projects
				TemplateManager templateManager = new TemplateManager();
				int existingProjectTemplateId = templateManager.RetrieveForProject(existingProjectId).ProjectTemplateId;
				int newProjectTemplateId = templateManager.RetrieveForProject(newProjectId).ProjectTemplateId;
				bool sameTemplates = !createNewTemplate;

				//If the source and destination templates are the same, then we can use the same field values
				//Otherwise we have to create new ones and map them in the dictionaries
				//sameTemplates is the flag to check

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(5, GlobalResources.Messages.Project_Copy_Attachments);
				}

				#region Copy Attachments

				//Now we need to copy across any project attachments (may be attached to artifacts or may not be)
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.ExportAllProjectAttachments(
					userId,
					existingProjectId,
					newProjectId,
					sameTemplates,
					attachmentFolderMapping,
					documentTypeMapping,
					documentStatusMapping,
					attachmentIdsMapping,
					true,
					true);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(10, GlobalResources.Messages.Project_Copy_Releases);
				}

				#region Copy Releases

				//***** Now we need to copy across the release list *****
				Business.ReleaseManager releaseManager = new Business.ReleaseManager();
				List<ReleaseView> releases = releaseManager.RetrieveByProjectId(existingProjectId, false);
				foreach (ReleaseView release in releases)
				{
					int oldReleaseId = release.ReleaseId;
					int newReleaseId = releaseManager.Insert(
						userId,
						newProjectId,
						release.CreatorId,
						release.Name,
						release.Description,
						release.VersionNumber,
						release.IndentLevel,
						//set the status to the default
						Release.ReleaseStatusEnum.Planned,
						(Release.ReleaseTypeEnum)release.ReleaseTypeId,
						release.StartDate,
						release.EndDate,
						release.ResourceCount,
						release.DaysNonWorking,
						release.OwnerId);

					releaseMapping.Add(oldReleaseId, newReleaseId);

					//We need to make a summary item if necessary
					if (release.IsSummary)
					{
						ReleaseView release2 = releaseManager.RetrieveById2(newProjectId, newReleaseId);
						release2.IsSummary = true;
						release2.IsExpanded = true;
						releaseManager.UpdatePositionalData(userId, new List<ReleaseView>() { release2 });
					}

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Release, oldReleaseId, newProjectId, newReleaseId, attachmentIdsMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(15, GlobalResources.Messages.Project_Copy_Components);
				}

				#region Copy Components

				//***** Now we need to copy across the project components *****
				ComponentManager componentManager = new ComponentManager();
				List<Component> components = componentManager.Component_Retrieve(existingProjectId);
				if (components != null)
				{
					//Iterate through all the active components, add to destination and map new ID
					foreach (Component component in components)
					{
						int newComponentId = componentManager.Component_Insert(newProjectId, component.Name);
						if (!componentMapping.ContainsKey(component.ComponentId))
						{
							componentMapping.Add(component.ComponentId, newComponentId);
						}
					}
				}

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(25, GlobalResources.Messages.Project_Copy_Components);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(17, GlobalResources.Messages.Project_Copy_Requirements);
				}

				#region Copy Requirements

				//***** Now we need to copy across the requirements matrix *****
				RequirementManager requirementManager = new RequirementManager();
				List<RequirementView> requirements = requirementManager.Retrieve(UserManager.UserInternal, existingProjectId, 1, Int32.MaxValue, null, 0);
				foreach (RequirementView requirement in requirements)
				{
					//Lookup the release in the destination project
					int? releaseId = null;
					if (requirement.ReleaseId.HasValue)
					{
						if (releaseMapping.ContainsKey(requirement.ReleaseId.Value))
						{
							releaseId = releaseMapping[requirement.ReleaseId.Value];
						}
					}

					//Lookup the component in the destination project
					int? componentId = null;
					if (requirement.ComponentId.HasValue)
					{
						if (componentMapping.ContainsKey(requirement.ComponentId.Value))
						{
							componentId = componentMapping[requirement.ComponentId.Value];
						}
					}

					//See if we need to lookup the new id values
					int? requirementTypeId = null;
					int? requirementImportanceId = null;
					if (sameTemplates)
					{
						requirementTypeId = requirement.RequirementTypeId;
						requirementImportanceId = requirement.ImportanceId;
					}
					else
					{
						if (requirementTypeMapping.ContainsKey(requirement.RequirementTypeId))
						{
							requirementTypeId = requirementTypeMapping[requirement.RequirementTypeId];
						}
						if (requirement.ImportanceId.HasValue && requirementImportanceMapping.ContainsKey(requirement.ImportanceId.Value))
						{
							requirementImportanceId = requirementImportanceMapping[requirement.ImportanceId.Value];
						}
					}

					int oldRequirementId = requirement.RequirementId;
					int newRequirementId = requirementManager.Insert(
						userId,
						newProjectId,
						releaseId,
						componentId,
						requirement.IndentLevel,
						//set the status to the default
						Requirement.RequirementStatusEnum.Requested,
						requirementTypeId,
						requirement.AuthorId,
						requirement.OwnerId,
						requirementImportanceId,
						requirement.Name,
						requirement.Description,
						requirement.EstimatePoints,
						userId
						);
					requirementMapping.Add(oldRequirementId, newRequirementId);

					//We need to make a summary item if necessary
					if (requirement.IsSummary)
					{
						RequirementView requirement2 = requirementManager.RetrieveById(userId, newProjectId, newRequirementId);
						requirement2.IsSummary = true;
						requirement2.IsExpanded = true;
						requirementManager.UpdatePositionalData(userId, new List<RequirementView>() { requirement2 });
					}

					//Copy across the requirement steps (if any)
					requirementManager.CopyScenario(userId, newProjectId, oldRequirementId, newRequirementId);

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, oldRequirementId, newProjectId, newRequirementId, attachmentIdsMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(22, GlobalResources.Messages.Project_Copy_TestCaseFolders);
				}

				#region Copy Test Case Folders

				//***** Now we need to copy across the test case folder hierarchy *****
				TestCaseManager testCaseManager = new TestCaseManager();
				testCaseManager.TestCaseFolder_ExportAll(userId, existingProjectId, newProjectId, testCaseFolderMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(24, GlobalResources.Messages.Project_Copy_TestCases);
				}

				#region Copy Test Cases

				//***** Now we need to copy across the test case list *****
				List<TestCaseView> testCases = testCaseManager.Retrieve(existingProjectId, "Name", true, 1, Int32.MaxValue, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);
				foreach (TestCaseView testCase in testCases)
				{
					int oldTestCaseId = testCase.TestCaseId;
					int? destFolderId = null;
					if (testCase.TestCaseFolderId.HasValue)
					{
						if (testCaseFolderMapping.ContainsKey(testCase.TestCaseFolderId.Value))
						{
							destFolderId = testCaseFolderMapping[testCase.TestCaseFolderId.Value];
						}
					}

					//Lookup the component in the destination project
					List<int> newComponentIds = null;
					if (!string.IsNullOrWhiteSpace(testCase.ComponentIds))
					{
						newComponentIds = new List<int>();
						List<int> oldComponentIds = testCase.ComponentIds.FromDatabaseSerialization_List_Int32();
						foreach (int oldComponentId in oldComponentIds)
						{
							if (componentMapping.ContainsKey(oldComponentId))
							{
								int newComponentId = componentMapping[oldComponentId];
								newComponentIds.Add(newComponentId);
							}
						}
					}

					int? destAutomationAttachmentId = null;
					if (testCase.AutomationAttachmentId.HasValue && attachmentIdsMapping.ContainsKey(testCase.AutomationAttachmentId.Value))
					{
						destAutomationAttachmentId = attachmentIdsMapping[testCase.AutomationAttachmentId.Value];
					}

					//See if we need to lookup the new id values
					int? testCaseTypeId = null;
					int? testCasePriorityId = null;
					if (sameTemplates)
					{
						testCaseTypeId = testCase.TestCaseTypeId;
						testCasePriorityId = testCase.TestCasePriorityId;
					}
					else
					{
						if (testCaseTypeMapping.ContainsKey(testCase.TestCaseTypeId))
						{
							testCaseTypeId = testCaseTypeMapping[testCase.TestCaseTypeId];
						}
						if (testCase.TestCasePriorityId.HasValue && testCasePriorityMapping.ContainsKey(testCase.TestCasePriorityId.Value))
						{
							testCasePriorityId = testCasePriorityMapping[testCase.TestCasePriorityId.Value];
						}
					}

					int newTestCaseId = testCaseManager.Insert(
						userId,
						newProjectId,
						testCase.AuthorId,
						testCase.OwnerId,
						testCase.Name,
						testCase.Description,
						testCaseTypeId,
						//set the status to the default
						TestCase.TestCaseStatusEnum.Draft,
						testCasePriorityId,
						destFolderId,
						testCase.EstimatedDuration,
						testCase.AutomationEngineId,
						destAutomationAttachmentId,
						true,
						false,
						newComponentIds
						);
					testCaseMapping.Add(oldTestCaseId, newTestCaseId);

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, oldTestCaseId, newProjectId, newTestCaseId, attachmentIdsMapping);

					//***** Now we need to copy across any test case parameters *****
					List<TestCaseParameter> testCaseParameters = testCaseManager.RetrieveParameters(oldTestCaseId);
					foreach (TestCaseParameter testCaseParameter in testCaseParameters)
					{
						int newTestCaseParameterId = testCaseManager.InsertParameter(
							newProjectId,
							newTestCaseId,
							testCaseParameter.Name,
							testCaseParameter.DefaultValue, userId);

						if (!testCaseParameterMapping.ContainsKey(testCaseParameter.TestCaseParameterId))
						{
							testCaseParameterMapping.Add(testCaseParameter.TestCaseParameterId, newTestCaseParameterId);
						}
					}
				}

				//***** Now we need to copy across the test steps and linked test cases*****
				foreach (KeyValuePair<int, int> testCaseMappingEntry in testCaseMapping)
				{
					int oldTestCaseId = testCaseMappingEntry.Key;
					int newTestCaseId = testCaseMappingEntry.Value;
					List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(oldTestCaseId);
					foreach (TestStepView testStep in testSteps)
					{
						int oldTestStepId = testStep.TestStepId;
						//Handle the two cases of the test step and linked test case
						if (testStep.LinkedTestCaseId.HasValue)
						{
							//Lookup the linked test case
							int oldLinkedTestCaseId = testStep.LinkedTestCaseId.Value;
							int newLinkedTestCaseId;
							if (testCaseMapping.TryGetValue(oldLinkedTestCaseId, out newLinkedTestCaseId))
							{
								//Get the parameter values
								Dictionary<string, string> parameters = new Dictionary<string, string>();
								List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);
								foreach (TestStepParameter testStepParameterValue in testStepParameterValues)
								{
									parameters.Add(testStepParameterValue.Name, testStepParameterValue.Value);
								}

								testCaseManager.InsertLink(
									userId,
									newTestCaseId,
									null,
									newLinkedTestCaseId,
									parameters);
							}
						}
						else
						{
							int newTestStepId = testCaseManager.InsertStep(
								userId,
								newTestCaseId,
								testStep.Position,
								testStep.Description,
								testStep.ExpectedResult,
								testStep.SampleData,
								false);

							testStepMapping.Add(oldTestStepId, newTestStepId);

							//Copy across the links to any attachments
							attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, oldTestStepId, newProjectId, newTestStepId, attachmentIdsMapping);
						}
					}
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(29, GlobalResources.Messages.Project_Copy_AutomationHosts);
				}

				#region Copy Automation Hosts

				//***** Now we need to copy across the automation hosts list *****
				Business.AutomationManager automationManager = new Business.AutomationManager();
				List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(existingProjectId);
				foreach (AutomationHostView automationHost in automationHosts)
				{
					//Insert the new host, getting the new ID
					int newAutomationHostId = automationManager.InsertHost(
						newProjectId,
						automationHost.Name,
						automationHost.Token,
						automationHost.Description,
						automationHost.IsActive,
						userId
						);

					//Add to the mapping
					if (!automationHostMapping.ContainsKey(automationHost.AutomationHostId))
					{
						automationHostMapping.Add(automationHost.AutomationHostId, newAutomationHostId);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(32, GlobalResources.Messages.Project_Copy_TestConfigurations);
				}

				#region Copy Test Configurations

				//***** Now we need to copy across the test configuration sets *****
				Business.TestConfigurationManager testConfigurationManager = new Business.TestConfigurationManager();
				List<TestConfigurationSet> testConfigurationSets = testConfigurationManager.RetrieveSets(existingProjectId);
				foreach (TestConfigurationSet testConfigurationSet in testConfigurationSets)
				{
					//Insert the new set, getting the new ID
					int newTestConfigurationSetId = testConfigurationManager.InsertSet(
						newProjectId,
						testConfigurationSet.Name,
						testConfigurationSet.Description,
						testConfigurationSet.IsActive,
						userId
						);

					//Add to the mapping
					if (!testConfigurationSetMapping.ContainsKey(testConfigurationSet.TestConfigurationSetId))
					{
						testConfigurationSetMapping.Add(testConfigurationSet.TestConfigurationSetId, newTestConfigurationSetId);
					}

					//Also we need to copy across the test configuration entries, mapping the test parameters and custom list values
					testConfigurationManager.CopyEntries(testConfigurationSet.TestConfigurationSetId, newTestConfigurationSetId, sameTemplates, testCaseParameterMapping, customPropertyValueMapping);
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(35, GlobalResources.Messages.Project_Copy_TestSetFolders);
				}

				#region Copy Test Set Folders

				//***** Now we need to copy across the test set folder hierarchy *****
				TestSetManager testSetManager = new TestSetManager();
				testSetManager.TestSetFolder_ExportAll(userId, existingProjectId, newProjectId, testSetFolderMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(37, GlobalResources.Messages.Project_Copy_TestSets);
				}

				#region Copy Test Sets

				//***** Now we need to copy across the test set list *****
				List<TestSetView> testSets = testSetManager.Retrieve(existingProjectId, "Name", true, 1, Int32.MaxValue, null, 0, TestSetManager.TEST_SET_FOLDER_ID_ALL_TEST_SETS);
				foreach (TestSetView testSet in testSets)
				{
					//Lookup release, folder and test run type
					int? releaseId = null;
					if (testSet.ReleaseId.HasValue)
					{
						if (releaseMapping.ContainsKey(testSet.ReleaseId.Value))
						{
							releaseId = releaseMapping[testSet.ReleaseId.Value];
						}
					}

					TestRun.TestRunTypeEnum testRunType = TestRun.TestRunTypeEnum.Manual;
					if (testSet.TestRunTypeId.HasValue)
					{
						testRunType = (TestRun.TestRunTypeEnum)testSet.TestRunTypeId.Value;
					}

					//See if we have a mapped automation host
					int? automationHostId = null;
					if (testSet.AutomationHostId.HasValue && automationHostMapping.ContainsKey(testSet.AutomationHostId.Value))
					{
						automationHostId = automationHostMapping[testSet.AutomationHostId.Value];
					}

					//See if we have a mapped test configuration set
					int? testConfigurationSetId = null;
					if (testSet.TestConfigurationSetId.HasValue && testConfigurationSetMapping.ContainsKey(testSet.TestConfigurationSetId.Value))
					{
						testConfigurationSetId = testConfigurationSetMapping[testSet.TestConfigurationSetId.Value];
					}

					Nullable<TestSet.RecurrenceEnum> recurrence = null;
					if (testSet.RecurrenceId.HasValue)
					{
						recurrence = (TestSet.RecurrenceEnum)testSet.RecurrenceId;
					}

					int? destFolderId = null;
					if (testSet.TestSetFolderId.HasValue)
					{
						if (testSetFolderMapping.ContainsKey(testSet.TestSetFolderId.Value))
						{
							destFolderId = testSetFolderMapping[testSet.TestSetFolderId.Value];
						}
					}

					int oldTestSetId = testSet.TestSetId;

					//The test sets are always initially in 'not-started' mode in the new project
					int newTestSetId = testSetManager.Insert(
						userId,
						newProjectId,
						destFolderId,
						releaseId,
						testSet.CreatorId,
						testSet.OwnerId,
						//set the status to the default
						TestSet.TestSetStatusEnum.NotStarted,
						testSet.Name,
						testSet.Description,
						testSet.PlannedDate,
						testRunType,
						automationHostId,
						recurrence,
						testSet.IsAutoScheduled,
						testSet.IsDynamic,
						testSet.BuildExecuteTimeInterval,
						testSet.DynamicQuery,
						testConfigurationSetId
						);

					testSetMapping.Add(oldTestSetId, newTestSetId);

					//Export any test set parameter values
					List<TestSetParameter> testSetParameterValues = testSetManager.RetrieveParameterValues(oldTestSetId);
					foreach (TestSetParameter testSetParameter in testSetParameterValues)
					{
						if (testCaseParameterMapping.ContainsKey(testSetParameter.TestCaseParameterId))
						{
							int newTestCaseParameterId = testCaseParameterMapping[testSetParameter.TestCaseParameterId];
							testSetManager.AddTestSetParameter(newTestSetId, newTestCaseParameterId, testSetParameter.Value, newProjectId, userId);
						}
					}

					//Now we need to copy across the list of mapped test cases
					List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases(oldTestSetId);
					int copiedTestCaseId;
					foreach (TestSetTestCaseView testSetTestCase in testSetTestCases)
					{
						if (testCaseMapping.TryGetValue(testSetTestCase.TestCaseId, out copiedTestCaseId))
						{
							//We always use the default owner for the test cases
							Dictionary<int, int> newTestCaseInfo = testSetManager.AddTestCases(newProjectId, newTestSetId, new List<int>() { copiedTestCaseId }, null, userId);
							int newTestSetTestCaseId = newTestCaseInfo.First().Value;

							//Add any test set parameter values
							List<TestSetTestCaseParameter> newTestSetTestCaseParameterValues = new List<TestSetTestCaseParameter>();
							List<TestSetTestCaseParameter> testSetTestCaseParameterValues = testSetManager.RetrieveTestCaseParameterValues(testSetTestCase.TestSetTestCaseId);
							foreach (TestSetTestCaseParameter testSetTestCaseParameter in testSetTestCaseParameterValues)
							{
								//testSetTestCase.TestSetTestCaseId
								if (testCaseParameterMapping.ContainsKey(testSetTestCaseParameter.TestCaseParameterId))
								{
									TestSetTestCaseParameter newTestSetTestCaseParameter = new TestSetTestCaseParameter();
									newTestSetTestCaseParameter.TestCaseParameterId = testCaseParameterMapping[testSetTestCaseParameter.TestCaseParameterId];
									newTestSetTestCaseParameter.TestSetTestCaseId = newTestSetTestCaseId;
									newTestSetTestCaseParameter.Value = testSetTestCaseParameter.Value;
									newTestSetTestCaseParameterValues.Add(newTestSetTestCaseParameter);
								}
							}
							testSetManager.SaveTestCaseParameterValues(newTestSetTestCaseParameterValues);
						}
					}

					//Copy across the links to any attachments
					attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, oldTestSetId, newProjectId, newTestSetId, attachmentIdsMapping);
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(42, GlobalResources.Messages.Project_Copy_Mappings);
				}

				#region Copy Requirement/Test Case/Release Mapping

				//***** Now we need to copy across the requirements' test case coverage and release mapping *****
				foreach (KeyValuePair<int, int> testCaseMappingEntry in testCaseMapping)
				{
					int oldTestCaseId = testCaseMappingEntry.Key;
					int newTestCaseId = testCaseMappingEntry.Value;

					//Load in the old project's coverage information and get the available and covered for this new test case
					requirements = requirementManager.RetrieveCoveredByTestCaseId(userId, existingProjectId, oldTestCaseId);
					List<int> newRequirementsList = new List<int>();
					foreach (RequirementView requirement in requirements)
					{
						int oldRequirementId = requirement.RequirementId;
						int newRequirementId;
						if (requirementMapping.TryGetValue(oldRequirementId, out newRequirementId))
						{
							newRequirementsList.Add(newRequirementId);
						}
					}
					requirementManager.AddToTestCase(newProjectId, newTestCaseId, newRequirementsList, userId);

					//Load in the old project's mapping information and get the available and mapped for this new test case
					List<ReleaseView> oldReleases = releaseManager.RetrieveMappedByTestCaseId(userId, existingProjectId, oldTestCaseId);
					List<int> newReleasesList = new List<int>();
					foreach (ReleaseView oldRelease in oldReleases)
					{
						int oldReleaseId = oldRelease.ReleaseId;
						int newReleaseId;
						if (releaseMapping.TryGetValue(oldReleaseId, out newReleaseId))
						{
							newReleasesList.Add(newReleaseId);
						}
					}
					releaseManager.AddToTestCase(newProjectId, newTestCaseId, newReleasesList, userId);
				}
				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(47, GlobalResources.Messages.Project_Copy_Risks);
				}

				#region Copy Risks

				//***** Copy across the risk list *****
				RiskManager riskManager = new RiskManager();

				//Get the default status once - in case we need to use it
				ProjectTemplate projectTemplate = templateManager.RetrieveForProject(newProjectId);
				RiskStatus defaultRiskStatus = riskManager.RiskStatus_RetrieveDefault(projectTemplate.ProjectTemplateId);

				int riskCount = riskManager.Risk_Count(existingProjectId, null, 0);
				for (int i = 1; i <= riskCount; i += PAGE_SIZE)
				{
					List<RiskView> risks = riskManager.Risk_Retrieve(existingProjectId, "RiskId", true, i, PAGE_SIZE, null, 0);
					foreach (RiskView risk in risks)
					{
						//Lookup the release id
						int? releaseId = null;
						if (risk.ReleaseId.HasValue && releaseMapping.ContainsKey(risk.ReleaseId.Value))
						{
							releaseId = releaseMapping[risk.ReleaseId.Value];
						}

						//Lookup the component
						int? componentId = null;
						if (risk.ComponentId.HasValue)
						{
							if (componentMapping.ContainsKey(risk.ComponentId.Value))
							{
								componentId = componentMapping[risk.ComponentId.Value];
							}
						}

						//See if we need to lookup the new id values
						int? riskTypeId = null;
						int? riskProbabilityId = null;
						int? riskImpactId = null;
						int? riskDetectabilityId = null;
						if (sameTemplates)
						{
							riskTypeId = risk.RiskTypeId;
							riskProbabilityId = risk.RiskProbabilityId;
							riskImpactId = risk.RiskImpactId;
							riskDetectabilityId = risk.RiskDetectabilityId;
						}
						else
						{
							if (riskTypeMapping.ContainsKey(risk.RiskTypeId))
							{
								riskTypeId = riskTypeMapping[risk.RiskTypeId];
							}
							if (risk.RiskProbabilityId.HasValue && riskProbabilityMapping.ContainsKey(risk.RiskProbabilityId.Value))
							{
								riskProbabilityId = riskProbabilityMapping[risk.RiskProbabilityId.Value];
							}
							if (risk.RiskImpactId.HasValue && riskImpactMapping.ContainsKey(risk.RiskImpactId.Value))
							{
								riskImpactId = riskImpactMapping[risk.RiskImpactId.Value];
							}
							//TODO - when add detectability need to make sure to set the values here
							//if (risk.RiskDetectabilityId.HasValue && riskDetectabilityMapping.ContainsKey(risk.RiskDetectabilityId.Value))
							//{
							//	riskDetectabilityId = riskProbabilityMapping[risk.RiskDetectabilityId.Value];
							//}
						}

						//Actually perform the insert of the copy
						int newRiskId = riskManager.Risk_Insert(
							newProjectId,
							defaultRiskStatus.RiskStatusId,
							riskTypeId,
							riskProbabilityId,
							riskImpactId,
							risk.CreatorId,
							risk.OwnerId,
							risk.Name,
							risk.Description,
							releaseId,
							componentId,
							risk.CreationDate,
							risk.ReviewDate,
							risk.ClosedDate
							);

						//Add to the mapping
						if (!riskMapping.ContainsKey(risk.RiskId))
						{
							riskMapping.Add(risk.RiskId, newRiskId);
						}

						//Do not copy across the mitigations

						//Copy across the links to any attachments
						attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Risk, risk.RiskId, newProjectId, newRiskId, attachmentIdsMapping);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(57, GlobalResources.Messages.Project_Copy_TaskFolders);
				}

				#region Copy Task Folders

				//***** First we need to copy across the task folders *****
				TaskManager taskManager = new TaskManager();
				List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(existingProjectId);
				foreach (TaskFolderHierarchyView taskFolder in taskFolders)
				{
					int? parentTaskFolderId = null;
					if (taskFolder.ParentTaskFolderId.HasValue && taskFolderMapping.ContainsKey(taskFolder.ParentTaskFolderId.Value))
					{
						parentTaskFolderId = taskFolderMapping[taskFolder.ParentTaskFolderId.Value];
					}
					int newTaskFolderId = taskManager.TaskFolder_Create(taskFolder.Name, newProjectId, parentTaskFolderId).TaskFolderId;

					//Add to the mapping
					if (!taskFolderMapping.ContainsKey(taskFolder.TaskFolderId))
					{
						taskFolderMapping.Add(taskFolder.TaskFolderId, newTaskFolderId);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(59, GlobalResources.Messages.Project_Copy_Tasks);
				}

				#region Copy Tasks

				//***** Now we need to copy across the task list *****
				int taskCount = taskManager.Count(existingProjectId, null, 0);
				for (int i = 1; i <= taskCount; i += PAGE_SIZE)
				{
					List<TaskView> tasks = taskManager.Retrieve(existingProjectId, "TaskId", true, i, PAGE_SIZE, null, 0);
					foreach (TaskView task in tasks)
					{
						//Lookup the release and requirement ids
						int? requirementId = null;
						if (task.RequirementId.HasValue && requirementMapping.ContainsKey(task.RequirementId.Value))
						{
							requirementId = requirementMapping[task.RequirementId.Value];
						}
						int? releaseId = null;
						if (task.ReleaseId.HasValue && releaseMapping.ContainsKey(task.ReleaseId.Value))
						{
							releaseId = releaseMapping[task.ReleaseId.Value];
						}
						int? riskId = null;
						if (task.RiskId.HasValue && riskMapping.ContainsKey(task.RiskId.Value))
						{
							riskId = riskMapping[task.RiskId.Value];
						}

						//Lookup the folder id
						int? taskFolderId = null;
						if (task.TaskFolderId.HasValue && taskFolderMapping.ContainsKey(task.TaskFolderId.Value))
						{
							taskFolderId = taskFolderMapping[task.TaskFolderId.Value];
						}

						//See if we need to lookup the new id values
						int? taskTypeId = null;
						int? taskPriorityId = null;
						if (sameTemplates)
						{
							taskTypeId = task.TaskTypeId;
							taskPriorityId = task.TaskPriorityId;
						}
						else
						{
							if (taskTypeMapping.ContainsKey(task.TaskTypeId))
							{
								taskTypeId = taskTypeMapping[task.TaskTypeId];
							}
							if (task.TaskPriorityId.HasValue && taskPriorityMapping.ContainsKey(task.TaskPriorityId.Value))
							{
								taskPriorityId = taskPriorityMapping[task.TaskPriorityId.Value];
							}
						}

						//Actually perform the insert of the copy
						int newTaskId = taskManager.Insert(
							newProjectId,
							task.CreatorId,
							//set the status to the default
							Task.TaskStatusEnum.NotStarted,
							taskTypeId,
							taskFolderId,
							requirementId,
							releaseId,
							task.OwnerId,
							taskPriorityId,
							task.Name,
							task.Description,
							null,
							null,
							task.EstimatedEffort,
							null,
							null,
							userId,
							true,
							riskId,
							false
							);

						//Add to the mapping
						if (!taskMapping.ContainsKey(task.TaskId))
						{
							taskMapping.Add(task.TaskId, newTaskId);
						}

						//Copy across the links to any attachments
						attachmentManager.Export(existingProjectId, DataModel.Artifact.ArtifactTypeEnum.Task, task.TaskId, newProjectId, newTaskId, attachmentIdsMapping);
					}
				}

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(70, GlobalResources.Messages.Project_Copy_CustomProperties);
				}

				#region Copy Custom Properties

				//Now we need to copy across all the custom properties assigned to all Requirements in the project
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Requirement, requirementMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Cases
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestCase, testCaseMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Steps
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestStep, testStepMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Releases
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Release, releaseMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Test Sets
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.TestSet, testSetMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Tasks
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Task, taskMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Documents
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Document, attachmentIdsMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Automation Hosts
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.AutomationHost, automationHostMapping, customPropertyValueMapping);

				//Now we need to copy across all the custom properties assigned to Risks
				CopyArtifactCustomProperties(userId, existingProjectId, newProjectId, existingProjectTemplateId, newProjectTemplateId, Artifact.ArtifactTypeEnum.Risk, riskMapping, customPropertyValueMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(85, GlobalResources.Messages.Project_Copy_Associations);
				}

				#region Copy Associations 
				// within the project only, not cross project

				//Requirements
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Requirement, ArtifactTypeEnum.Requirement, requirementMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Requirement, ArtifactTypeEnum.Risk, requirementMapping, riskMapping);

				//Test cases
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestCase, ArtifactTypeEnum.Task, testCaseMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestCase, ArtifactTypeEnum.Risk, testCaseMapping, riskMapping);

				//Test step
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.TestStep, ArtifactTypeEnum.Requirement, testStepMapping, requirementMapping);

				//Risks
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Task, riskMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Requirement, riskMapping, requirementMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.Risk, riskMapping, riskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Risk, ArtifactTypeEnum.TestCase, riskMapping, testCaseMapping);

				//Tasks
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.Task, taskMapping, taskMapping);
				CopyArtifactAssociations(newProjectId, ArtifactTypeEnum.Task, ArtifactTypeEnum.TestCase, taskMapping, testCaseMapping);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(93, GlobalResources.Messages.Project_Copy_Comments);
				}

				#region Copy Discussions
				CopyArtifactDiscussions(newProjectId, attachmentIdsMapping, DataModel.Artifact.ArtifactTypeEnum.Document);
				CopyArtifactDiscussions(newProjectId, releaseMapping, DataModel.Artifact.ArtifactTypeEnum.Release);
				CopyArtifactDiscussions(newProjectId, requirementMapping, DataModel.Artifact.ArtifactTypeEnum.Requirement);
				CopyArtifactDiscussions(newProjectId, riskMapping, DataModel.Artifact.ArtifactTypeEnum.Risk);
				CopyArtifactDiscussions(newProjectId, taskMapping, DataModel.Artifact.ArtifactTypeEnum.Task);
				CopyArtifactDiscussions(newProjectId, testCaseMapping, DataModel.Artifact.ArtifactTypeEnum.TestCase);
				CopyArtifactDiscussions(newProjectId, testSetMapping, DataModel.Artifact.ArtifactTypeEnum.TestSet);

				#endregion

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(100, "");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newProjectId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Creates a new project using an existing one as a template
		/// </summary>
		/// <param name="name">The name of the new project</param>
		/// <param name="description">The description of the new project (optional)</param>
		/// <param name="website">A URL for the new project (optional)</param>
		/// <param name="existingProjectId">The ID of the existing project</param>
		/// <param name="createNewTemplate">Should we create a new template for the project</param>
		/// <param name="isActive">Is the project active</param>
		/// <returns>The newly created project ID </returns>
		/// <remarks>
		/// This overload doesn't require the caller to provide out references for the various mapping dictionaries
		/// </remarks>
		public int CreateFromExisting(string name, string description, string website, int existingProjectId, bool isActive = true, bool createNewTemplate = false, int? userId = null, int? adminSectionId = null, string action = null)
		{
			//Declare the mapping dictionaries (not used in this overload)
			Dictionary<int, int> attachmentFolderMapping;
			Dictionary<int, int> incidentStatusMapping;
			Dictionary<int, int> incidentTypeMapping;
			Dictionary<int, int> incidentPriorityMapping;
			Dictionary<int, int> incidentSeverityMapping;
			Dictionary<int, int> requirementTypeMapping;
			Dictionary<int, int> requirementImportanceMapping;
			Dictionary<int, int> taskTypeMapping;
			Dictionary<int, int> taskPriorityMapping;
			Dictionary<int, int> testCaseTypeMapping;
			Dictionary<int, int> testCasePriorityMapping;
			Dictionary<int, int> documentTypeMapping;
			Dictionary<int, int> documentStatusMapping;
			Dictionary<int, int> riskStatusMapping;
			Dictionary<int, int> riskTypeMapping;
			Dictionary<int, int> riskProbabilityMapping;
			Dictionary<int, int> riskImpactMapping;
			Dictionary<int, int> customPropertyIdMapping;
			Dictionary<int, int> propertyValueMapping;
			return this.CreateFromExisting(name, description, website, existingProjectId, isActive, createNewTemplate,
				out attachmentFolderMapping,
				out incidentStatusMapping,
				out incidentTypeMapping,
				out incidentPriorityMapping,
				out incidentSeverityMapping,
				out requirementTypeMapping,
				out requirementImportanceMapping,
				out taskTypeMapping,
				out taskPriorityMapping,
				out testCaseTypeMapping,
				out testCasePriorityMapping,
				out documentTypeMapping,
				out documentStatusMapping,
				out riskTypeMapping,
				out riskStatusMapping,
				out riskProbabilityMapping,
				out riskImpactMapping,
				out customPropertyIdMapping,
				out propertyValueMapping,
				userId,
				adminSectionId,
				action
				);
		}

		/// <summary>
		/// Creates a new project using an existing one as a template
		/// </summary>
		/// <param name="name">The name of the new project</param>
		/// <param name="description">The description of the new project (optional)</param>
		/// <param name="website">A URL for the new project (optional)</param>
		/// <param name="existingProjectId">The ID of the existing project</param>
		/// <param name="attachmentFolderMapping">The mapping of attachment folders (out)</param>
		/// <param name="attachmentTypeMapping">The mapping of attachment types (out)</param>
		/// <param name="propertyValueMapping">The mapping of custom property values (out)</param>
		/// <returns>The newly created project ID </returns>
		/// <param name="createNewTemplate">Should we create a new template for the project</param>
		/// <param name="isActive">Is the project active</param>
		/// <remarks>
		/// This creates a new project, but instead of creating a default workflow and set of incident lists,
		/// it copies them from the existing project. It also copies across the existing project membership.
		/// It copies:
		/// 1) Workflows with transitions, steps, etc.
		/// 2) Project membership
		/// 3) Incident types
		/// 4) Incident statuses
		/// 5) Incident priorities
		/// 6) Incident severities
		/// 7) Custom properties and associated list values
		/// 8) Project attachment types and folders
		/// 9) Notification events.
		/// </remarks>
		protected internal int CreateFromExisting(string name, string description, string website, int existingProjectId, bool isActive, bool createNewTemplate,
			out Dictionary<int, int> attachmentFolderMapping,
			out Dictionary<int, int> incidentStatusMapping,
			out Dictionary<int, int> incidentTypeMapping,
			out Dictionary<int, int> incidentPriorityMapping,
			out Dictionary<int, int> incidentSeverityMapping,
			out Dictionary<int, int> requirementTypeMapping,
			out Dictionary<int, int> requirementImportanceMapping,
			out Dictionary<int, int> taskTypeMapping,
			out Dictionary<int, int> taskPriorityMapping,
			out Dictionary<int, int> testCaseTypeMapping,
			out Dictionary<int, int> testCasePriorityMapping,
			out Dictionary<int, int> documentTypeMapping,
			out Dictionary<int, int> documentStatusMapping,
			out Dictionary<int, int> riskTypeMapping,
			out Dictionary<int, int> riskStatusMapping,
			out Dictionary<int, int> riskProbabilityMapping,
			out Dictionary<int, int> riskImpactMapping,
			out Dictionary<int, int> customPropertyIdMapping,
			out Dictionary<int, int> propertyValueMapping,
			int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "CreateFromExisting";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			AdminAuditManager adminAuditManager = new AdminAuditManager();

			try
			{
				//Create the mapping hashtables for storing old vs new IDs
				Dictionary<int, int> workflowMapping = new Dictionary<int, int>();
				incidentStatusMapping = new Dictionary<int, int>();
				incidentTypeMapping = new Dictionary<int, int>();
				incidentPriorityMapping = new Dictionary<int, int>();
				incidentSeverityMapping = new Dictionary<int, int>();
				customPropertyIdMapping = new Dictionary<int, int>();
				requirementImportanceMapping = new Dictionary<int, int>();
				requirementTypeMapping = new Dictionary<int, int>();
				taskPriorityMapping = new Dictionary<int, int>();
				taskTypeMapping = new Dictionary<int, int>();
				testCasePriorityMapping = new Dictionary<int, int>();
				testCaseTypeMapping = new Dictionary<int, int>();
				documentTypeMapping = new Dictionary<int, int>();
				documentStatusMapping = new Dictionary<int, int>();
				riskStatusMapping = new Dictionary<int, int>();
				riskTypeMapping = new Dictionary<int, int>();
				riskProbabilityMapping = new Dictionary<int, int>();
				riskImpactMapping = new Dictionary<int, int>();
				propertyValueMapping = new Dictionary<int, int>();

				//First we need to retrieve the existing project so that we can set the same base information
				Project existingProject = this.RetrieveById(existingProjectId);

				//Should we use the existing project's template, or create a new one that everything is copied from
				int projectTemplateId;
				if (createNewTemplate)
				{
					//We want to create a new template based on the existing one
					TemplateManager templateManager = new TemplateManager();
					projectTemplateId = templateManager.Insert(name,
						description,
						isActive,
						existingProject.ProjectTemplateId,
						out incidentStatusMapping,
						out incidentTypeMapping,
						out incidentPriorityMapping,
						out incidentSeverityMapping,
						out requirementTypeMapping,
						out requirementImportanceMapping,
						out taskTypeMapping,
						out taskPriorityMapping,
						out testCaseTypeMapping,
						out testCasePriorityMapping,
						out documentTypeMapping,
						out documentStatusMapping,
						out riskTypeMapping,
						out riskStatusMapping,
						out riskProbabilityMapping,
						out riskImpactMapping,
						out customPropertyIdMapping,
						out propertyValueMapping,
						userId, adminSectionId, action
						);
				}
				else
				{
					projectTemplateId = existingProject.ProjectTemplateId;
				}

				int newProjectId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Next create the new project itself
					Project newProject = new Project();
					newProject.ProjectGroupId = existingProject.ProjectGroupId;
					newProject.ProjectTemplateId = projectTemplateId;
					newProject.Name = name.MaxLength(50);
					newProject.Description = description;
					newProject.Website = website.MaxLength(255);
					newProject.CreationDate = DateTime.UtcNow;
					newProject.IsActive = isActive;
					newProject.WorkingHours = existingProject.WorkingHours;
					newProject.WorkingDays = existingProject.WorkingDays;
					newProject.NonWorkingHours = existingProject.NonWorkingHours;
					newProject.IsTimeTrackIncidents = existingProject.IsTimeTrackIncidents;
					newProject.IsTimeTrackTasks = existingProject.IsTimeTrackTasks;
					newProject.IsEffortIncidents = existingProject.IsEffortIncidents;
					newProject.IsEffortTasks = existingProject.IsEffortTasks;
					newProject.IsEffortTestCases = existingProject.IsEffortTestCases;
					newProject.IsTasksAutoCreate = existingProject.IsTasksAutoCreate;
					newProject.ReqDefaultEstimate = existingProject.ReqDefaultEstimate;
					newProject.ReqPointEffort = existingProject.ReqPointEffort;
					newProject.TaskDefaultEffort = existingProject.TaskDefaultEffort;
					newProject.IsReqStatusByTasks = existingProject.IsReqStatusByTasks;
					newProject.IsReqStatusByTestCases = existingProject.IsReqStatusByTestCases;
					newProject.IsReqStatusAutoPlanned = existingProject.IsReqStatusAutoPlanned;

					//Save the new object
					context.Projects.AddObject(newProject);
					context.SaveChanges();

					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = newProject.Name;

					//Now capture the newly created project id
					newProjectId = newProject.ProjectId;

					//Next we need to copy across any project settings
					List<ProjectSettingValue> projectSettingValues = context.ProjectSettingValues.Where(p => p.ProjectId == existingProjectId).ToList();
					foreach (ProjectSettingValue projectSettingValue in projectSettingValues)
					{
						//Add the setting to the new project
						ProjectSettingValue newProjectSettingValue = new ProjectSettingValue();
						newProjectSettingValue.ProjectId = newProjectId;
						newProjectSettingValue.Value = projectSettingValue.Value;
						newProjectSettingValue.ProjectSettingId = projectSettingValue.ProjectSettingId;
						newProject.ProjectSettings.Add(newProjectSettingValue);
					}
					context.SaveChanges();

					if (userId != null)
					{
						//Log history.
						if (logHistory)
							adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), newProjectId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");
					}
				}

				//***** Now we need to copy across the project membership *****                
				List<ProjectUser> projectMembership = this.RetrieveUserMembershipById(existingProjectId);
				foreach (ProjectUser projectUser in projectMembership)
				{
					this.InsertUserMembership(
						projectUser.UserId,
						newProjectId,
						projectUser.ProjectRoleId);
				}

				//Now we need to copy across the project attachment folders
				AttachmentManager attachment = new AttachmentManager();
				attachmentFolderMapping = attachment.CopyFolders(existingProjectId, newProjectId);

				//Now copy across any project data-sync mappings
				DataMappingManager dataMappingManager = new DataMappingManager();
				dataMappingManager.CopyProjectMappings(
					existingProjectId,
					newProjectId,
					createNewTemplate,
					incidentSeverityMapping,
					incidentPriorityMapping,
					incidentStatusMapping,
					incidentTypeMapping,
					requirementImportanceMapping,
					requirementTypeMapping,
					taskPriorityMapping,
					taskTypeMapping,
					testCasePriorityMapping,
					testCaseTypeMapping,
					propertyValueMapping,
					customPropertyIdMapping
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newProjectId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new project record into the system
		/// </summary>
		/// <param name="name">The name of the project</param>
		/// <param name="description">A description of the project (optional)</param>
		/// <param name="website">A URL for the project (optional)</param>
		/// <param name="active">Is the project active?</param>
		/// <param name="projectTemplateId">The id of the project template (null = create new template)</param>
		/// <param name="projectGroupId">The id of the group it belongs to (optional)</param>
		/// <param name="userId">The id of the user creating the project (optional)</param>
		/// <returns>The newly created projectId</returns>
		/// <remarks>This version uses the standard schedule information</remarks>
		public int Insert(string name, int? projectGroupId, string description, string website, bool active, int? projectTemplateId, int? userId = null, int? adminSectionId = null, string action = null)
		{
			return this.Insert(name, projectGroupId, description, website, active, projectTemplateId, ReleaseManager.DEFAULT_HOURS_PER_DAY, ReleaseManager.DEFAULT_DAYS_PER_WEEK, ReleaseManager.DEFAULT_NON_WORKING_HOURS_PER_MONTH, true, true, true, true, false, false, 1.0M, ProjectManager.DEFAULT_POINT_EFFORT, null, true, true, true, userId, adminSectionId, action);
		}

		/// <summary>
		/// Inserts a new project record into the system
		/// </summary>
		/// <param name="name">The name of the project</param>
		/// <param name="description">A description of the project (optional)</param>
		/// <param name="website">A URL for the project (optional)</param>
		/// <param name="active">Is the project active?</param>
		/// <param name="effortCalculateIncidents">Do incidents factor into effort calculations</param>
		/// <param name="effortCalculateTasks">Do tasks factor into effort calculations</param>
		/// <param name="effortCalculateTestCases">Do test cases factor into effort calculations</param>
		/// <param name="nonWorkingHoursPerMonth">How many non-work hours do we have in a normal month</param>
		/// <param name="timeTrackingIncidents">Do incidents allow time-tracking</param>
		/// <param name="timeTrackingTasks">Do tasks allow time-tracking</param>
		/// <param name="workingDaysPerWeek">How many working days are in a week</param>
		/// <param name="workingHoursPerDay">How many working hours are in a day</param>
		/// <param name="projectGroupId">The id of the group it belongs to (optional)</param>
		/// <param name="reqDefaultEstimate">The default estimate associated with a new requirement</param>
		/// <param name="taskDefaultEffort">The default effort associated with a new task</param>
		/// <param name="tasksAutoCreate">Should we auto-create tasks when a requirement is moved to 'In-Progress'</param>
		/// <param name="reqPointEffort">The effort per story point starting metric</param>
		/// <param name="isReqStatusByTasks">Should task status affect the status of their associated requirements</param>
		/// <param name="isReqStatusByTestCases">Should test case status affect the status of their associated requirements</param>
		/// <param name="isReqStatusAutoPlanned">Should requirements automatically switch to 'Planned' status when a release is specified</param>
		/// <param name="projectTemplateId">The id of the project template (null = create new template)</param>
		/// <param name="userId">The id of the user to be added as owner - ie the creator of the project</param>
		/// <returns>The newly created projectId</returns>
		public int Insert(string name, int? projectGroupId, string description, string website, bool active, int? projectTemplateId, int workingHoursPerDay, int workingDaysPerWeek, int nonWorkingHoursPerMonth, bool timeTrackingIncidents, bool timeTrackingTasks, bool effortCalculateIncidents, bool effortCalculateTasks, bool effortCalculateTestCases, bool tasksAutoCreate, decimal? reqDefaultEstimate, int? reqPointEffort, int? taskDefaultEffort, bool isReqStatusByTasks = true, bool isReqStatusByTestCases = true, bool isReqStatusAutoPlanned = true, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert";

			string newValue = "";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			try
			{
				//AdminAudit manager, in case.
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				//Get the default project group if one is not specified
				if (!projectGroupId.HasValue)
				{
					//Get the default project group instead
					ProjectGroupManager projectGroupManager = new ProjectGroupManager();
					projectGroupId = projectGroupManager.GetDefault();
				}

				//Use the default point effort metric if none specified
				if (!reqPointEffort.HasValue)
				{
					reqPointEffort = DEFAULT_POINT_EFFORT;
				}

				if (!projectTemplateId.HasValue)
				{
					//We need to create a new template for this project
					//For now, use the same name, description active flag as the project
					TemplateManager templateManager = new TemplateManager();
					projectTemplateId = templateManager.Insert(name, description, active, null, userId, adminSectionId, action);
				}

				int projectId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new project
					Project project = new Project();
					project.ProjectGroupId = projectGroupId.Value;
					project.ProjectTemplateId = projectTemplateId.Value;
					project.Name = name.MaxLength(50);
					project.Description = description;
					project.Website = website.MaxLength(255);
					project.CreationDate = DateTime.UtcNow;
					project.IsActive = active;
					project.WorkingHours = workingHoursPerDay;
					project.WorkingDays = workingDaysPerWeek;
					project.NonWorkingHours = nonWorkingHoursPerMonth;
					project.IsTimeTrackIncidents = timeTrackingIncidents;
					project.IsTimeTrackTasks = timeTrackingTasks;
					project.IsEffortIncidents = effortCalculateIncidents;
					project.IsEffortTasks = effortCalculateTasks;
					project.IsEffortTestCases = effortCalculateTestCases;
					project.IsTasksAutoCreate = tasksAutoCreate;
					project.ReqDefaultEstimate = reqDefaultEstimate;
					project.ReqPointEffort = reqPointEffort.Value;
					project.TaskDefaultEffort = taskDefaultEffort;
					project.IsReqStatusByTasks = isReqStatusByTasks;
					project.IsReqStatusByTestCases = isReqStatusByTestCases;
					project.IsReqStatusAutoPlanned = isReqStatusAutoPlanned;

					//Save the new object
					context.Projects.AddObject(project);
					context.SaveChanges();

					//Now capture the newly created project id
					projectId = project.ProjectId;
					newValue = project.Name;
				}

				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				details.NEW_VALUE = newValue;

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), projectId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Project, "ProjectId");

				//Most items are part of the template in v6.0+ and not part of the project, so we only have to
				//add membership and document folders

				//Now we need to insert a new member for the project - the system admin
				this.InsertUserMembership(UserManager.UserSystemAdministrator, projectId, ProjectManager.ProjectRoleProjectOwner);

				//And a user Id if passed in to the call
				//Make sure that user is NOT system admin ID:1 so we don't try and add them twice
				if (userId.HasValue && userId != UserManager.UserSystemAdministrator)
				{
					this.InsertUserMembership((int)userId, projectId, ProjectManager.ProjectRoleProjectOwner);
				}

				//Need to create a default attachment folder
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.InsertProjectAttachmentFolder(projectId, AttachmentManager.DEFAULT_FOLDER_NAME, null);

				//Update the project's completion metrics to program and portfolio
				RefreshRequirementCompletion(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a project entity
		/// </summary>
		/// <param name="project">The project to be updated</param>
		public void Update(Project project, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//See if the project group id changed, need to refresh the old one if so
				int? oldProjectGroupId = null;
				if (project.ChangeTracker.OriginalValues.ContainsKey("ProjectGroupId") && project.ChangeTracker.OriginalValues["ProjectGroupId"] is Int32)
				{
					oldProjectGroupId = (int)project.ChangeTracker.OriginalValues["ProjectGroupId"];
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach the entity to the context and save
					context.Projects.ApplyChanges(project);
					context.AdminSaveChanges(userId, project.ProjectId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();	
				}

				//Update the project's completion metrics to program and portfolio
				RefreshRequirementCompletion(project.ProjectId);
				if (oldProjectGroupId.HasValue)
				{
					new ProjectGroupManager().RefreshRequirementCompletion(oldProjectGroupId.Value);
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

	/// <summary>This exception is thrown when you try and insert a membership record that already exists and causes a unique constraint violation</summary>
	public class ProjectDuplicateMembershipRecordException : ApplicationException
	{
		public ProjectDuplicateMembershipRecordException()
		{
		}
		public ProjectDuplicateMembershipRecordException(string message)
			: base(message)
		{
		}
		public ProjectDuplicateMembershipRecordException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>This exception is thrown when you try and delete one of the built-in project roles</summary>
	public class ProjectRoleNotDeletableException : ApplicationException
	{
		public ProjectRoleNotDeletableException()
		{
		}
		public ProjectRoleNotDeletableException(string message)
			: base(message)
		{
		}
		public ProjectRoleNotDeletableException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	/// <summary>This exception is thrown when you try and deactivate the Project Owner Role</summary>
	public class ProjectRoleNotDeactivatableException : ApplicationException
	{
		public ProjectRoleNotDeactivatableException()
		{
		}
		public ProjectRoleNotDeactivatableException(string message)
			: base(message)
		{
		}
		public ProjectRoleNotDeactivatableException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
