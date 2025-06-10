using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Objects;
using System.Transactions;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Project Groups in the system
	/// </summary>
	public class ProjectGroupManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ProjectGroupManager::";

		#region Project Group functions

		/// <summary>
		/// Retrieves a single project group in the system by its id
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="includeMembership">Should we return the list of project group members (users) as well</param>
		/// <returns>Project group entity</returns>
		/// <remarks>The users collection will be unsorted if included</remarks>
		public ProjectGroup RetrieveById(int projectGroupId, bool includeMembership = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectGroup projectGroup;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Query to get the list of project groups with matching project group id
					ObjectQuery<ProjectGroup> projectGroups = context.ProjectGroups;
					if (includeMembership)
					{
						projectGroups = projectGroups
							.Include("Users")
							.Include("Users.User")
							.Include("Users.User.Profile")
							.Include("Users.ProjectGroupRole");
					}

					var query = from p in projectGroups
								where p.ProjectGroupId == projectGroupId
								select p;

					projectGroup = query.FirstOrDefault();
					//Throw a business exception if no record found
					if (projectGroup == null)
					{
						throw new ArtifactNotExistsException("Project Group " + projectGroupId + " doesn't exist in the system");
					}
				}

				//Return the project group
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroup;
			}
			catch (ArtifactNotExistsException)
			{
				//Don't log this one, leave that decision up to the parent
				//to avoid spurious errors in the event log
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
		/// Retrieves all the active groups in the system
		/// </summary>
		/// <returns>Project group list</returns>
		public List<ProjectGroup> RetrieveActive()
		{
			const string METHOD_NAME = "RetrieveActive";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroup> projectGroups;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Query to get the list of project groups with active = true
					var query = from p in context.ProjectGroups
								where p.IsActive
								orderby p.Name, p.ProjectGroupId
								select p;

					projectGroups = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroups;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of programs in the specified portfolio
		/// </summary>
		/// <param name="portfolioId">The ID of the portfolio (null = no portfolio)</param>
		/// <param name="activeOnly">Do we want active only [Default: True]</param>
		/// <returns>Project group list</returns>
		public List<ProjectGroup> ProjectGroup_RetrieveByPortfolio(int? portfolioId, bool activeOnly = true)
		{
			const string METHOD_NAME = "ProjectGroup_RetrieveByPortfolio";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroup> projectGroups;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Query to get the list of project groups
					var query = from p in context.ProjectGroups
								where (p.IsActive || !activeOnly)
								select p;

					if (portfolioId.HasValue)
					{
						query = query.Where(p => p.PortfolioId == portfolioId.Value);
					}
					else
					{
						query = query.Where(p => !p.PortfolioId.HasValue);
					}

					//Add the sorts
					query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectGroupId);

					projectGroups = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroups;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all project groups that match the passed-in filter
		/// </summary>
		/// <param name="filters">The hashtable of filters to apply to the project group list</param>
		/// <param name="groupOwnerId">Return only project groups that the user is the owner of (null for all projects)</param>
		/// <param name="sortExpression">The sort expression to use, pass null for default</param>
		/// <returns>A project dataset</returns>
		/// <remarks>
		/// Pass filters = null for all projects.
		/// The filters supported are for name, web-site, default and active flag only
		/// </remarks>
		public List<ProjectGroup> Retrieve(Hashtable filters, string sortExpression, int? groupOwnerId = null)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroup> projectGroups;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query

					//See if we're only to return projects that are owned by the user
					IQueryable<ProjectGroup> query;
					if (groupOwnerId.HasValue)
					{
						//First get the list of projects that are owned by the user (not counting group owners)
						var query2 = from pu in context.ProjectGroupUsers
									 where
										pu.UserId == groupOwnerId.Value &&
										pu.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner
									 select pu.ProjectGroupId;
						var projectGroupIds = query2.ToList();

						//Create select command for retrieving these project group records
						query = from p in context.ProjectGroups
								where projectGroupIds.Contains(p.ProjectGroupId)
								select p;
					}
					else
					{
						//Create select command for retrieving all the project group records
						//We include the template and portfolio information as well
						query = from p in context.ProjectGroups
									.Include(p => p.ProjectTemplate)
									.Include(p => p.Portfolio)
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

						//Default Flag (check for both Y/N form and the newer boolean form)
						if (filters["DefaultYn"] != null && filters["DefaultYn"] is String)
						{
							bool isDefault = ((string)filters["DefaultYn"] == "Y");
							query = query.Where(p => p.IsDefault == isDefault);
						}
						if (filters["IsDefault"] != null)
						{
							bool isDefault;
							if (filters["IsDefault"] is Boolean)
							{
								isDefault = (bool)filters["IsDefault"];
							}
							else
							{
								isDefault = ((string)filters["IsDefault"] == "Y");
							}
							query = query.Where(p => p.IsDefault == isDefault);
						}

						//Project Group ID
						if (filters["ProjectGroupId"] != null)
						{
							//The value might be an Int32 or String, so use ToString() to be on the safe side
							//Need to make sure that the project group id is numeric
							int projectGroupId;
							if (Int32.TryParse(filters["ProjectGroupId"].ToString(), out projectGroupId))
							{
								query = query.Where(p => p.ProjectGroupId == projectGroupId);
							}
						}

						//Project Template ID
						if (filters["ProjectTemplateId"] != null)
						{
							//The value might be an Int32 or String, so use ToString() to be on the safe side
							//Need to make sure that the project template id is numeric
							int projectTemplateId;
							if (Int32.TryParse(filters["ProjectTemplateId"].ToString(), out projectTemplateId))
							{
								query = query.Where(p => p.ProjectTemplateId == projectTemplateId);
							}
						}

						//Portfolio ID
						if (filters["PortfolioId"] != null)
						{
							//The value might be an Int32 or String, so use ToString() to be on the safe side
							//Need to make sure that the project template id is numeric
							int portfolioId;
							if (Int32.TryParse(filters["PortfolioId"].ToString(), out portfolioId))
							{
								query = query.Where(p => p.PortfolioId == portfolioId);
							}
						}
					}

					//Add the sorts and execute
					if (String.IsNullOrEmpty(sortExpression))
					{
						query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectGroupId);
					}
					else
					{
						query = query.OrderUsingSortExpression(sortExpression, "ProjectGroupId");
					}
					projectGroups = query.ToList();
				}

				//Return the dataset
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroups;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the completion metrics of the specified project group
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		protected internal void RefreshRequirementCompletion(int projectGroupId)
		{
			const string METHOD_NAME = "RefreshRequirementCompletion";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.ProjectGroup_RefreshRequirementCompletion(projectGroupId);
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
		/// Returns the id of the default project group
		/// </summary>
		/// <returns>The id of the default group</returns>
		public int GetDefault()
		{
			const string METHOD_NAME = "GetDefault";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int projectGroupId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectGroups
								where p.IsDefault
								select p.ProjectGroupId;

					if (query.Count() == 0)
					{
						throw new ProjectGroupDefaultException("Unable to locate a default project group");
					}
					projectGroupId = query.FirstOrDefault();
				}

				//Return the project group id
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupId;
			}
			catch (ArtifactNotExistsException)
			{
				//Don't log this one, leave that decision up to the parent
				//to avoid spurious errors in the event log
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
		/// Inserts a new project group into the system
		/// </summary>
		/// <param name="name">The name of the project</param>
		/// <param name="description">A description of the project group (optional)</param>
		/// <param name="website">A URL for the project group (optional)</param>
		/// <param name="isActive">Is the project group active?</param>
		/// <param name="isDefault">Is this the default project group?</param>
		/// <param name="projectTemplateId">Should we associate the program with a project template (optional)</param>
		/// <param name="portfolioId">The portfolio that the program is associated with (optional)</param>
		/// <param name="userId">The id of the user creating the program, they need to be added to program membership</param>
		/// <returns>The newly created projectGroupId</returns>
		public int Insert(string name, string description, string website, bool isActive, bool isDefault, int? projectTemplateId = null, int? userId = null, int? portfolioId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//AdminAudit manager, in case.
				AdminAuditManager adminAuditManager = new AdminAuditManager();

				int projectGroupId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the new entity
					ProjectGroup projectGroup = new ProjectGroup();
					projectGroup.IsActive = isActive;
					projectGroup.IsDefault = isDefault;
					projectGroup.Name = name;
					projectGroup.Description = description;
					projectGroup.Website = website;
					projectGroup.ProjectTemplateId = projectTemplateId;
					projectGroup.PortfolioId = portfolioId;

					//Make sure you can't make an inactive project the default
					if (!isActive && isDefault)
					{
						throw new ProjectGroupDefaultException("You cannot make an inactive project group the default one in the system.");
					}

					//Save changes
					context.ProjectGroups.AddObject(projectGroup);
					context.SaveChanges();

					string newValue = projectGroup.Name;

					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = newValue;

					//Log history.
					if (logHistory)
						adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), projectGroup.ProjectGroupId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Program, "ProjectGroupId");

					//Now capture the newly created project group id and return
					projectGroupId = projectGroup.ProjectGroupId;

					//If the default flag was specified as Yes, need to make all the other project groups default = "N";
					if (isDefault)
					{
						var query = from p in context.ProjectGroups
									where p.IsDefault && p.ProjectGroupId != projectGroupId
									select p;

						List<ProjectGroup> projectGroups = query.ToList();
						foreach (ProjectGroup projectGroup2 in projectGroups)
						{
							projectGroup2.StartTracking();
							projectGroup2.IsDefault = false;
						}
						context.SaveChanges();
					}

					//Now we need to insert a new member for the program - the system admin
					List<ProjectGroupUser> projectGroupUsers = new List<ProjectGroupUser>();
					ProjectGroupUser mainSystemAdim = new ProjectGroupUser();
					mainSystemAdim.MarkAsAdded();
					mainSystemAdim.ProjectGroupId = projectGroupId;
					mainSystemAdim.UserId = UserManager.UserSystemAdministrator;
					mainSystemAdim.ProjectGroupRoleId = (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner;
					projectGroupUsers.Add(mainSystemAdim);

					//If we have a passed in user id parameter (ie the person creating the program from the UI) add them as an owner too.
					//Make sure that user is NOT system admin ID:1 so we don't try and add them twice
					if (userId.HasValue && userId != UserManager.UserSystemAdministrator)
					{
						ProjectGroupUser newProjectGroupUser = new ProjectGroupUser();
						newProjectGroupUser.MarkAsAdded();
						newProjectGroupUser.ProjectGroupId = projectGroupId;
						newProjectGroupUser.UserId = (int)userId;
						newProjectGroupUser.ProjectGroupRoleId = (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner;
						projectGroupUsers.Add(newProjectGroupUser);
					}

					ProjectGroupManager projectGroupManager = new ProjectGroupManager();
					projectGroupManager.SaveUserMembership(projectGroupUsers);

					//Update the project group's completion metrics to program and portfolio
					RefreshRequirementCompletion(projectGroupId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single projectGroup record by projectGroup id</summary>
		/// <param name="projectGroupId">The ID of the projectGroup to retrieve</param>
		/// <returns>A projectGroup entity</returns>
		public ProjectGroup RetrieveById(int projectGroupId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the ProjectGroup record
				ProjectGroup projectGroup;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectGroups
								where p.ProjectGroupId == projectGroupId
								select p;

					projectGroup = query.FirstOrDefault();
				}

				//Throw an exception if the project record is not found
				if (projectGroup == null)
				{
					throw new ArtifactNotExistsException("ProjectGroup " + projectGroupId + " doesn't exist in the system");
				}

				//Return the portfolio
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

		/// <summary>
		/// Deletes a project group from the system
		/// </summary>
		/// <param name="projectGroupId">The project group to be deleted</param>
		/// <remarks>It doesn't delete any projects, just moves them to the default group</remarks>
		public void Delete(int projectGroupId, int? userId = null)
		{
			const string METHOD_NAME = "Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the id of the default project group
				int defaultProjectGroupId = this.GetDefault();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Make sure the group isn't the default one and exists
					var query1 = from p in context.ProjectGroups
									 .Include(p => p.Users)
								 where p.ProjectGroupId == projectGroupId
								 select p;

					ProjectGroup projectGroup = query1.FirstOrDefault();
					if (projectGroup == null)
					{
						throw new ArtifactNotExistsException("Project Group PG" + projectGroupId + " doesn't exist in the system");
					}
					else
					{
						if (projectGroup.IsDefault)
						{
							throw new ProjectGroupDefaultException("You cannot delete the default project group.");
						}

						//Need to move all the projects to the default group
						var query2 = from p in context.Projects
									 where p.ProjectGroupId == projectGroupId
									 orderby p.ProjectId
									 select p;

						List<Project> projects = query2.ToList();
						foreach (Project project in projects)
						{
							project.StartTracking();
							project.ProjectGroupId = defaultProjectGroupId;
						}
						context.SaveChanges();

						//Now we need to delete all the user membership and the project group itself
						context.ProjectGroups.DeleteObject(projectGroup);
						context.SaveChanges();

						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "View / Edit Programs";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)userId, projectGroup.ProjectGroupId, projectGroup.Name, adminSectionId, "Program Deleted", DateTime.UtcNow, ArtifactTypeEnum.Program, "ProjectGroupId");

						//Update the default project group's completion metrics to program and portfolio
						RefreshRequirementCompletion(defaultProjectGroupId);
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

		/// <summary>
		/// Updates an updated project groups that is passed-in
		/// </summary>
		/// <param name="projectGroup">The entity to be persisted</param>
		public void Update(ProjectGroup projectGroup, int? userId, int? adminSectionId, string action)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure that we're not trying to make the default group inactive
				//And also capture the id of the default item if it's recently set
				int defaultProjectGroupId = -1;
				bool changingDefault = false;
				if (!projectGroup.IsActive && projectGroup.IsDefault)
				{
					throw new ProjectGroupDefaultException("You cannot make an inactive project group the default one in the system.");
				}
				if (projectGroup.ChangeTracker.OriginalValues.ContainsKey("IsDefault"))
				{
					changingDefault = true;
					if (!projectGroup.IsDefault && (bool)projectGroup.ChangeTracker.OriginalValues["IsDefault"] == true)
					{
						//You can't make an existing project group not the default, instead you need to make another group the default.
						throw new ProjectGroupDefaultException("You need to have at one project group set as the default in the system.");
					}
					if (projectGroup.IsDefault && (bool)projectGroup.ChangeTracker.OriginalValues["IsDefault"] == false)
					{
						defaultProjectGroupId = projectGroup.ProjectGroupId;
					}
				}

				//See if the portfolio id changed, need to refresh the old one if so
				int? oldPortfolioId = null;
				if (projectGroup.ChangeTracker.OriginalValues.ContainsKey("PortfolioId") && projectGroup.ChangeTracker.OriginalValues["PortfolioId"] is Int32)
				{
					oldPortfolioId = (int)projectGroup.ChangeTracker.OriginalValues["PortfolioId"];
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We use a transaction block to avoid multiple defaults being set
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Attach the entity to the context and save changes
						context.ProjectGroups.ApplyChanges(projectGroup);
						context.AdminSaveChanges(userId, projectGroup.ProjectGroupId, null, adminSectionId, action, true, true, null);
						//Save the changes
						context.SaveChanges();
						

						//If we have a new default item, set all the other groups to default = n
						if (changingDefault && defaultProjectGroupId > 0)
						{
							var query = from p in context.ProjectGroups
										where p.ProjectGroupId != defaultProjectGroupId
										select p;

							List<ProjectGroup> projectGroups = query.ToList();
							foreach (ProjectGroup projectGroup2 in projectGroups)
							{
								projectGroup2.StartTracking();
								projectGroup2.IsDefault = false;
							}

							//Save the changes
							context.SaveChanges();
						}

						//Commit the transaction
						transactionScope.Complete();
					}
				}

				//Update the project group's completion metrics to program and portfolio
				RefreshRequirementCompletion(projectGroup.ProjectGroupId);

				if (oldPortfolioId.HasValue)
				{
					new PortfolioManager().RefreshRequirementCompletion(oldPortfolioId.Value);
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

		#region Resources Functions

		/// <summary>
		/// Generates the tooltip to display on project group task progress bars
		/// </summary>
		public static string GenerateTaskProgressTooltip(ProjectTaskProgressEntryView projectTaskProgressEntryView)
		{
			int taskCount = projectTaskProgressEntryView.TaskCount;

			//Handle case of no tasks
			if (taskCount == 0)
			{
				return GlobalResources.General.Global_NoTasks;
			}

			string tooltipText =
				"# " + GlobalResources.General.Global_Tasks + "=" + taskCount +
				", " + GlobalResources.General.Task_OnSchedule + "=" + projectTaskProgressEntryView.TaskPercentOnTime +
				"%, " + GlobalResources.General.Task_RunningLate + "=" + projectTaskProgressEntryView.TaskPercentLateFinish +
				"%, " + GlobalResources.General.Task_StartingLate + "=" + projectTaskProgressEntryView.TaskPercentLateStart +
				"%, " + GlobalResources.General.Task_NotStarted + "=" + projectTaskProgressEntryView.TaskPercentNotStart + "%";
			return tooltipText;
		}

		/// <summary>
		/// Retrieves the list of resources for a project group
		/// </summary>
		/// <param name="projectGroupId">The ID of the project group to retrieve the users for</param>
		/// <param name="filters">Any filters to apply</param>
		/// <param name="sortAscending">Whether to sort ascending</param>
		/// <param name="sortProperty">The property to sort by</param>
		/// <param name="utcOffset">The UTC offset</param>
		/// <returns>A project resource list</returns>
		/// <remarks>To get the sorted version, need to access the default data view</remarks>
		public List<ProjectResourceView> RetrieveResourcesForGroup(int projectGroupId, string sortProperty = null, bool? sortAscending = null, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "RetrieveResourcesForGroup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new project resource list for the group
			List<ProjectResourceView> projectGroupResources = new List<ProjectResourceView>();

			try
			{
				//Get the list of projects in the group
				ProjectManager projectManager = new ProjectManager();
				Hashtable filters2 = new Hashtable();
				filters2.Add("ProjectGroupId", projectGroupId);
				List<ProjectView> projects = projectManager.Retrieve(filters2, null);

				//Now get the list of resources in each project in turn and add to group dataset
				foreach (ProjectView project in projects)
				{
					if (project.IsActive)
					{
						int projectId = project.ProjectId;
						List<ProjectResourceView> projectResources = projectManager.RetrieveResources(projectId);
						foreach (ProjectResourceView projectResource in projectResources)
						{
							int userId = projectResource.UserId;
							ProjectResourceView projectGroupResource = projectGroupResources.FirstOrDefault(p => p.UserId == userId);
							if (projectGroupResource == null)
							{
								ProjectResourceView projectGroupResource2 = new ProjectResourceView();
								projectGroupResource2.UserId = userId;
								projectGroupResource2.FullName = projectResource.FullName;
								projectGroupResource2.ProjectId = 0;
								projectGroupResource2.ProjectRoleId = 0;
								projectGroupResource2.ProjectRoleName = "";
								projectGroupResource2.isOverAllocated = false;
								projectGroupResource2.ReqTaskEffort = projectResource.ReqTaskEffort;
								projectGroupResource2.IncidentEffort = projectResource.IncidentEffort;
								projectGroupResource2.totalEffort = projectResource.TotalEffort;
								projectGroupResource2.resourceEffort = projectResource.ResourceEffort;
								projectGroupResource2.remainingEffort = projectResource.RemainingEffort;
								projectGroupResource2.totalOpenEffort = projectResource.TotalOpenEffort;
								projectGroupResources.Add(projectGroupResource2);
							}
							else
							{
								if (projectResource.ReqTaskEffort.HasValue)
								{
									if (projectGroupResource.ReqTaskEffort.HasValue)
									{
										projectGroupResource.ReqTaskEffort += projectResource.ReqTaskEffort;
									}
									else
									{
										projectGroupResource.ReqTaskEffort = projectResource.ReqTaskEffort;
									}
								}
								if (projectResource.IncidentEffort.HasValue)
								{
									if (projectGroupResource.IncidentEffort.HasValue)
									{
										projectGroupResource.IncidentEffort += projectResource.IncidentEffort;
									}
									else
									{
										projectGroupResource.IncidentEffort = projectResource.IncidentEffort;
									}
								}
								projectGroupResource.totalEffort += projectResource.TotalEffort;
								projectGroupResource.totalOpenEffort += projectResource.TotalOpenEffort;
								if (projectResource.ResourceEffort.HasValue)
								{
									if (projectGroupResource.ResourceEffort.HasValue)
									{
										projectGroupResource.resourceEffort += projectResource.ResourceEffort;
									}
									else
									{
										projectGroupResource.resourceEffort = projectResource.ResourceEffort;
									}
								}
								if (projectResource.RemainingEffort.HasValue)
								{
									if (projectGroupResource.RemainingEffort.HasValue)
									{
										projectGroupResource.remainingEffort += projectResource.RemainingEffort;
									}
									else
									{
										projectGroupResource.remainingEffort = projectResource.RemainingEffort;
									}
								}
							}
						}
					}
				}

				//Now we need to use LINQ filtering (not LINQ-Entities since purely in-memory)
				if (filters != null)
				{
					Expression<Func<ProjectResourceView, bool>> filterExpression = CreateFilterExpression<ProjectResourceView>(null, null, Artifact.ArtifactTypeEnum.User, filters, utcOffset, new List<string> { "AllocationIndicator" }, null, false);
					if (filterExpression != null)
					{
						projectGroupResources = projectGroupResources.Where(filterExpression.Compile()).ToList();
					}

					//The allocation filter is more complex and needs to be handled separately
					if (filters.ContainsKey("AllocationIndicator") && filters["AllocationIndicator"] is Int32)
					{
						int allocationFilter = (int)filters["AllocationIndicator"];
						{
							switch (allocationFilter)
							{
								//since right now with no release selected, we don't have a value for resource effort
								//So we have to use a much simpler Yes/No filter option (same as for entire project view)
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
									projectGroupResources = projectGroupResources.Where(p => p.TotalEffort == 0).ToList();
									break;
								case 2:
								//>= 25%
								case 3:
								//>= 50%
								case 4:
								//>= 75%
								case 5:
									//= 100%
									projectGroupResources = projectGroupResources.Where(p => p.TotalEffort > 0).ToList();
									break;
								case 6:
									//> 100%
									//Not possible when no release selected, since we don't know what the resource has available
									projectGroupResources = projectGroupResources.Where(p => false).ToList();
									break;

									/*
                                    case 1:
                                        //= 0%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 0).ToList();
                                        break;
                                    case 2:
                                        //>= 25%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 25).ToList();
                                        break;
                                    case 3:
                                        //>= 50%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 50).ToList();
                                        break;
                                    case 4:
                                        //>= 75%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) >= 75).ToList();
                                        break;
                                    case 5:
                                        //= 100%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) == 100).ToList();
                                        break;
                                    case 6:
                                        //> 100%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) > 100).ToList();
                                        break;
                                    case 7:
                                        //< 25%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 25).ToList();
                                        break;
                                    case 8:
                                        //< 50%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 50).ToList();
                                        break;
                                    case 9:
                                        //< 75%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 75).ToList();
                                        break;
                                    case 10:
                                        //< 100%
                                        projectGroupResources = projectGroupResources.Where(p => ((p.TotalEffort * 100) / p.ResourceEffort) < 100).ToList();
                                        break;*/
							}
						}
					}
				}

				//Now we need to use LINQ sorting (not LINQ-Entities since purely in-memory)
				if (!String.IsNullOrEmpty(sortProperty) && sortAscending.HasValue)
				{
					projectGroupResources = new GenericSorter<ProjectResourceView>().Sort(projectGroupResources, sortProperty, sortAscending.Value).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return projectGroupResources;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Project Group Membership Functions

		/// <summary>
		/// Retrieves the project group membership for a specific user
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>Project group user list</returns>
		public List<ProjectGroupUserView> RetrieveForUser(int userId)
		{
			const string METHOD_NAME = "RetrieveForUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroupUserView> projectGroupUsers;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the project group membership
					var query = from p in context.ProjectGroupUsersView
								where p.UserId == userId && p.IsActive
								orderby p.ProjectGroupName, p.ProjectGroupId
								select p;

					projectGroupUsers = query.ToList();
				}

				//Return the dataset
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupUsers;
			}
			catch (ArtifactNotExistsException)
			{
				//Don't log this one, leave that decision up to the parent
				//to avoid spurious errors in the event log
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
		/// Returns a list of programs that the specified user is an owner of
		/// </summary>
		/// <param name="programOwnerId"></param>
		/// <returns></returns>
		public List<ProjectGroupUserView> RetrieveProgramsByOwner(int programOwnerId)
		{
			const string METHOD_NAME = "RetrieveProgramsByOwner";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroupUserView> programs;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectGroupUsersView
								where p.UserId == programOwnerId && p.IsActive && p.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner
								orderby p.ProjectGroupName, p.ProjectGroupId
								select p;

					programs = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return programs;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of either active or all project group roles for use in lookups
		/// </summary>
		/// <param name="activeOnly">Return all roles or just active ones</param>
		/// <returns>Dataset of project group roles</returns>
		public List<ProjectGroupRole> RetrieveRoles(bool activeOnly)
		{
			const string METHOD_NAME = "RetrieveRoles";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroupRole> projectGroupRoles;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectGroupRoles
								select p;

					//Add the active only filter if necessary
					if (activeOnly)
					{
						query = query.Where(p => p.ActiveYn == "Y");
					}

					//Sort
					query = query.OrderBy(p => p.Name).ThenBy(p => p.ProjectGroupRoleId);

					//Execute
					projectGroupRoles = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupRoles;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ProjectGroupRole RetrieveProjectGoupRoleById(int projectGroupRoleId)
		{
			const string METHOD_NAME = "RetrieveProjectGoupRoleById";

			System.Data.DataSet permissionDataSet = new System.Data.DataSet();

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectGroupRole projectGroupRole;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from p in context.ProjectGroupRoles
								where p.ProjectGroupRoleId == projectGroupRoleId
								select p;

					projectGroupRole = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupRole;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the group membership for a project group
		/// </summary>
		/// <param name="projectGroupId">The id of the group</param>
		/// <returns>The list of current membership</returns>
		public List<ProjectGroupUser> RetrieveUserMembership(int projectGroupId)
		{
			const string METHOD_NAME = "RetrieveUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ProjectGroupUser> projectGroupUsers;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.ProjectGroupUsers
								where p.ProjectGroupId == projectGroupId
								orderby p.UserId, p.ProjectGroupRoleId
								select p;

					//Execute
					projectGroupUsers = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return projectGroupUsers;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the project group's membership information
		/// </summary>
		/// <param name="projectGroupUsers">The project group user list containing the project group membership information</param>
		public void SaveUserMembership(List<ProjectGroupUser> projectGroupUsers, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "SaveUserMembership";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Actually perform the database persist
				try
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Apply the changes
						foreach (ProjectGroupUser projectGroupUser in projectGroupUsers)
						{
							context.ProjectGroupUsers.ApplyChanges(projectGroupUser);
							context.AdminSaveChanges(userId, projectGroupUser.ProjectGroupId, null, adminSectionId, action, true, true, null);
						}
						 context.SaveChanges();

					}
				}
				catch (EntityConstraintViolationException)
				{
					//If we have a unique constraint violation, throw a business exception
					throw new ProjectDuplicateMembershipRecordException("That project group membership row already exists!");
				}
			}
			catch (ProjectDuplicateMembershipRecordException exception)
			{
				//Log this as a warning
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
		/// Determines if a user is a group admin of any project groups
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <returns>True if the user is the admin of at least one group</returns>
		public bool IsAdmin(int userId)
		{
			const string METHOD_NAME = "IsAdmin";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isAdmin = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve any group membership records that belong to this user
					//for the group role of Group Owner
					var query = from p in context.ProjectGroupUsers
								where p.UserId == userId && p.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner
								select p;

					isAdmin = query.Any();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isAdmin;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Determines if a user is a group admin of a specific group
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <returns>True if the user is the admin of the group</returns>
		public bool IsAdmin(int userId, int projectGroupId)
		{
			const string METHOD_NAME = "IsAdmin";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isAdmin = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve any group membership records that belong to this user
					//for the group role of Group Owner
					var query = from p in context.ProjectGroupUsers
								where p.UserId == userId && p.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner && p.ProjectGroupId == projectGroupId
								select p;

					isAdmin = query.Any();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isAdmin;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}




		public bool IsSystemAdmin(int userId)
		{
			const string METHOD_NAME = "IsSystemAdmin";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isSystemAdmin = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve any group membership records that belong to this user
					//for the group role of Group Owner
					var query = from p in context.UserProfiles
								where p.UserId == userId && p.IsAdmin==true
								select p;

					isSystemAdmin = query.Any();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isSystemAdmin;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Determines if a user is allowed to view the project group
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <returns>True if the user has a permissions record in the group (any role)</returns>
		public bool IsAuthorized(int userId, int projectGroupId)
		{
			const string METHOD_NAME = "IsAuthorized";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool isAuthorized = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to retrieve any group membership records that belong to this user
					//for the group role of Group Owner
					var query = from p in context.ProjectGroupUsers
								where p.UserId == userId && p.ProjectGroupId == projectGroupId
								select p;

					isAuthorized = query.Any();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isAuthorized;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void Update(ProjectGroup projectGroup)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	/// <summary>
	/// This exception is thrown if the system cannot find a default group or if you try and create multiple defaults
	/// </summary>
	public class ProjectGroupDefaultException : ApplicationException
	{
		public ProjectGroupDefaultException()
		{
		}
		public ProjectGroupDefaultException(string message)
			: base(message)
		{
		}
		public ProjectGroupDefaultException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
