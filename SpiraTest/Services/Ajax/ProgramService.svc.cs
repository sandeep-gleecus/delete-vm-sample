using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.Ajax
{
    /// <summary>
    /// Used for making Ajax calls that display program data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ]
    public class ProgramService : AjaxWebServiceBase, IProgramService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ProgramService::";

        #region IWorkspace Methods

        /// <summary>
        /// Displays the overall progress of the program
        /// </summary>
        /// <param name="workspaceId">The id of the program</param>
        /// <returns>The workspace overview, or null if not found</returns>
        public WorkspaceData Workspace_RetrieveCompletionData(int workspaceId)
        {
            const string METHOD_NAME = "Workspace_RetrieveCompletionData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized for this program
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            if (!projectGroupManager.IsAuthorized(userId, workspaceId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The data being returned
                WorkspaceData workspace = null;

                //First lets retrieve the program
                ProjectGroup projectGroup = projectGroupManager.RetrieveById(workspaceId);
                if (projectGroup != null)
                {
                    workspace = new WorkspaceData();

                    //Add the primary information about the program
                    workspace.Workspace = new WorkspaceItem();
                    workspace.Workspace.WorkspaceId = projectGroup.ProjectGroupId;
                    workspace.Workspace.WorkspaceName = projectGroup.Name;
                    workspace.Workspace.StartDate = projectGroup.StartDate;
                    workspace.Workspace.EndDate = projectGroup.EndDate;
                    workspace.Workspace.RequirementsAll = projectGroup.RequirementCount;
                    workspace.Workspace.PercentComplete = projectGroup.PercentComplete;

                    //Next lets get the products
                    ProjectManager projectManager = new ProjectManager();
                    List<ProjectView> projects = projectManager.Project_RetrieveByGroup(workspaceId);
                    workspace.Products = new List<WorkspaceItem>();
                    foreach (ProjectView project in projects)
                    {
                        WorkspaceItem productItem = new WorkspaceItem();
                        productItem = new WorkspaceItem();
                        productItem.ParentId = project.ProjectGroupId;
                        productItem.WorkspaceId = project.ProjectId;
                        productItem.WorkspaceName = project.Name;
                        productItem.StartDate = project.StartDate;
                        productItem.EndDate = project.EndDate;
                        productItem.RequirementsAll = project.RequirementCount;
                        productItem.PercentComplete = project.PercentComplete;
                        workspace.Products.Add(productItem);
                    }

                    //Next lets get the releases and sprints in the program
                    ReleaseManager releaseManager = new ReleaseManager();
                    List<Release> releasesAndSprints = releaseManager.Release_RetrieveByProjectGroup(workspaceId);

                    //Sort into releases and iterations/sprints/phases and return as two separate collections

                    //Releases
                    workspace.Releases = new List<WorkspaceItem>();
                    foreach (Release release in releasesAndSprints.Where(r => !r.IsIterationOrPhase))
                    {
                        int? releaseParentId = releaseManager.GetParentReleaseId(release.ReleaseId);
                        WorkspaceItem releaseItem = new WorkspaceItem();
                        releaseItem = new WorkspaceItem();
                        if (releaseParentId == null)
                        {
                            releaseItem.ParentId = release.ProjectId;
                        }
                        else
                        {
                            // if the release has a parent, we need to check that parent is in our list of active releases
                            bool parentIsActive = releasesAndSprints.Any(r => r.ReleaseId == releaseParentId);
                            if (parentIsActive)
                            {
                                releaseItem.ParentId = releaseParentId;
                                releaseItem.ParentIsSameType = true;
                            }
                            // if the parent does not exist in this list, then it is inactive, so we should show the release as a directly child of its project
                            else
                            {
                                releaseItem.ParentId = release.ProjectId;
                            }
                        }
                        releaseItem.WorkspaceId = release.ReleaseId;
                        releaseItem.WorkspaceName = release.Name;
                        releaseItem.StartDate = release.StartDate;
                        releaseItem.EndDate = release.EndDate;
                        releaseItem.RequirementsAll = release.RequirementCount;
                        releaseItem.PercentComplete = release.PercentComplete;
                        workspace.Releases.Add(releaseItem);
                    }

                    //Sprints
                    workspace.Sprints = new List<WorkspaceItem>();
                    foreach (Release sprint in releasesAndSprints.Where(r => r.IsIterationOrPhase))
                    {
                        //Find the parent by indent level
                        string parentIndentLevel = sprint.IndentLevel.SafeSubstring(0, sprint.IndentLevel.Length - 3);
                        Release parent = releasesAndSprints.FirstOrDefault(r => r.ProjectId == sprint.ProjectId && r.IndentLevel == parentIndentLevel);

                        WorkspaceItem sprintItem = new WorkspaceItem();
                        sprintItem = new WorkspaceItem();
                        if (parent == null)
                        {
                            sprintItem.ParentId = sprint.ProjectId;
                        }
                        else
                        {
                            sprintItem.ParentId = parent.ReleaseId;
                            sprintItem.ParentIsSameType = true;
                        }
                        sprintItem.WorkspaceId = sprint.ReleaseId;
                        sprintItem.WorkspaceName = sprint.Name;
                        sprintItem.StartDate = sprint.StartDate;
                        sprintItem.EndDate = sprint.EndDate;
                        sprintItem.RequirementsAll = sprint.RequirementCount;
                        sprintItem.PercentComplete = sprint.PercentComplete;
                        workspace.Sprints.Add(sprintItem);
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return workspace;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion

        #region Program Methods

        /// <summary>
        /// Displays the list of products, releases and builds for the current program
        /// </summary>
        /// <param name="rowsToDisplay">The max number of releases/sprints to display per project</param>
        /// <param name="projectGroupId">The id of the program</param>
        /// <returns>The list of products, releases and builds</returns>
        public WorkspaceData Program_RetrieveBuilds(int projectGroupId, int rowsToDisplay)
        {
            const string METHOD_NAME = "Program_RetrieveBuilds";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Instantiate managers
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            BuildManager buildManager = new BuildManager();

            //Make sure we're authorized for this program
            if (!projectGroupManager.IsAuthorized(userId, projectGroupId))
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The data being returned
                WorkspaceData workspace = new WorkspaceData();
                workspace.Products = new List<WorkspaceItem>();

                //Now get the builds/releases/products in the program
                List<BuildView> builds = buildManager.RetrieveForProjectGroup(projectGroupId, true);
                if (builds.Count > 0)
                {
                    //Group by project
                    List<IGrouping<int, BuildView>> buildsByProject = builds.GroupBy(b => b.ProjectId).ToList();

                    foreach (IGrouping<int, BuildView> project in buildsByProject)
                    {
                        bool containsBuilds_project = false;
                        WorkspaceItem projectItem = new WorkspaceItem();
                        projectItem = new WorkspaceItem();
                        projectItem.ParentId = projectGroupId;
                        projectItem.WorkspaceId = project.Key;
                        projectItem.WorkspaceName = project.First().ProjectName;
                        projectItem.DataItems = new List<DataItem>();
                        int count = 0;
                        foreach (BuildView build in project)
                        {
                            DataItem buildDataItem = new DataItem();
                            projectItem.DataItems.Add(buildDataItem);
                            PopulationFunctions.PopulateDataItem(buildDataItem, build);
                            containsBuilds_project = true;
                            count++;
                            if (count >= rowsToDisplay)
                            {
                                break;
                            }
                        }

                        if (containsBuilds_project)
                        {
                            workspace.Products.Add(projectItem);
                        }
                    }
                }


                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                return workspace;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        #endregion
    }
}
