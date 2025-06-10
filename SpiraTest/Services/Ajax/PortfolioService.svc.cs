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
    public class PortfolioService : AjaxWebServiceBase, IPortfolioService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.PortfolioService::";

        #region IWorkspace Methods

        /// <summary>
        /// Displays the overall progress of the portfolio
        /// </summary>
        /// <param name="workspaceId">The id of the portfolio</param>
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

            //Make sure we're authorized to view portfolios
            if (!UserIsPortfolioViewer)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate managers
                PortfolioManager portfolioManager = new PortfolioManager();
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                ProjectManager projectManager = new ProjectManager();
                ReleaseManager releaseManager = new ReleaseManager();

                //The data being returned
                WorkspaceData workspace = null;

                //First lets retrieve the portfolio
                int portfolioId = workspaceId;
                Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId);
                if (portfolio != null)
                {
                    workspace = new WorkspaceData();

                    //Add the primary information about the portfolio
                    workspace.Workspace = new WorkspaceItem();
                    workspace.Workspace.WorkspaceId = portfolio.PortfolioId;
                    workspace.Workspace.WorkspaceName = portfolio.Name;
                    workspace.Workspace.StartDate = portfolio.StartDate;
                    workspace.Workspace.EndDate = portfolio.EndDate;
                    workspace.Workspace.RequirementsAll = portfolio.RequirementCount;
                    workspace.Workspace.PercentComplete = portfolio.PercentComplete;

                    //Next lets get  the programs
                    List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId);
                    workspace.Programs = new List<WorkspaceItem>();
                    workspace.Products = new List<WorkspaceItem>();
                    workspace.Releases = new List<WorkspaceItem>();
                    workspace.Sprints = new List<WorkspaceItem>();
                    foreach (ProjectGroup projectGroup in projectGroups)
                    {
                        WorkspaceItem programItem = new WorkspaceItem();
                        programItem = new WorkspaceItem();
                        programItem.ParentId = projectGroup.PortfolioId;
                        programItem.WorkspaceId = projectGroup.ProjectGroupId;
                        programItem.WorkspaceName = projectGroup.Name;
                        programItem.StartDate = projectGroup.StartDate;
                        programItem.EndDate = projectGroup.EndDate;
                        programItem.RequirementsAll = projectGroup.RequirementCount;
                        programItem.PercentComplete = projectGroup.PercentComplete;
                        workspace.Programs.Add(programItem);

                        //Next lets get the projects in the program
                        List<ProjectView> projects = projectManager.Project_RetrieveByGroup(projectGroup.ProjectGroupId);
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
                        List<Release> releasesAndSprints = releaseManager.Release_RetrieveByProjectGroup(projectGroup.ProjectGroupId);

                        //Sort into releases and iterations/sprints/phases and return as two separate collections

                        //Releases
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

        #region Portfolio Methods

        /// <summary>
        /// Displays the list of programs, products, releases and builds for the current portfolio
        /// </summary>
        /// <param name="rowsToDisplay">The max number of releases/sprints to display per project</param>
        /// <param name="portfolioId">The id of the portfolio</param>
        /// <returns>The list of programs, products, releases and builds</returns>
        public WorkspaceData Portfolio_RetrieveBuilds(int portfolioId, int rowsToDisplay)
        {
            const string METHOD_NAME = "Portfolio_RetrieveBuilds";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!this.CurrentUserId.HasValue)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = this.CurrentUserId.Value;

            //Make sure we're authorized to view portfolios
            if (!UserIsPortfolioViewer)
            {
                throw new System.ServiceModel.FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //Instantiate managers
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                BuildManager buildManager = new BuildManager();

                //The data being returned
                WorkspaceData workspace = new WorkspaceData();

                //Now get the programs in this portfolio
                List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId);
                if (projectGroups.Count > 0)
                {
                    workspace.Programs = new List<WorkspaceItem>();
                    foreach (ProjectGroup projectGroup in projectGroups)
                    {
                        bool containsBuilds_projectGroup = false;
                        WorkspaceItem programItem = new WorkspaceItem();
                        programItem = new WorkspaceItem();
                        programItem.ParentId = projectGroup.PortfolioId;
                        programItem.WorkspaceId = projectGroup.ProjectGroupId;
                        programItem.WorkspaceName = projectGroup.Name;

                        //Now get the builds/releases/products in the program
                        List<BuildView> builds = buildManager.RetrieveForProjectGroup(projectGroup.ProjectGroupId, true);
                        if (builds.Count > 0)
                        {
                            //Group by project
                            programItem.Children = new List<WorkspaceItem>();
                            List<IGrouping<int, BuildView>> buildsByProject = builds.GroupBy(b => b.ProjectId).ToList();

                            foreach (IGrouping<int, BuildView> project in buildsByProject)
                            {
                                bool containsBuilds_project = false;
                                WorkspaceItem projectItem = new WorkspaceItem();
                                projectItem = new WorkspaceItem();
                                projectItem.ParentId = projectGroup.ProjectGroupId;
                                projectItem.WorkspaceId = project.Key;
                                projectItem.WorkspaceName = project.First().ProjectName;
                                projectItem.DataItems = new List<DataItem>();
                                int count = 0;
                                foreach (BuildView build in project)
                                {
                                    DataItem buildDataItem = new DataItem();
                                    projectItem.DataItems.Add(buildDataItem);
                                    PopulationFunctions.PopulateDataItem(buildDataItem, build);
                                    containsBuilds_projectGroup = true;
                                    containsBuilds_project = true;
                                    count++;
                                    if (count >= rowsToDisplay)
                                    {
                                        break;
                                    }
                                }

                                if (containsBuilds_project)
                                {
                                    programItem.Children.Add(projectItem);
                                }
                            }
                        }

                        if (containsBuilds_projectGroup)
                        {
                            workspace.Programs.Add(programItem);
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
