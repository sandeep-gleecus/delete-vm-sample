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
    /// Used for making Ajax calls that display enterprise-wide data
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ]
    public class EnterpriseService : AjaxWebServiceBase, IEnterpriseService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.EnterpriseService::";

        #region IWorkspace Methods

        private const int ENTERPRISE_ID = 0;    //Fake workspace ID used for the enterprise view
        private const int DEFAULT_PORTFOLIO_ID = -1; //Fake portfolio ID for those programs not in a portfolio

        /// <summary>
        /// Displays the overall progress of the enterprise (all portfolios)
        /// </summary>
        /// <param name="workspaceId">Not used for the enterprise view</param>
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

                //We need to capture the min and max dates as well as the other variables for the enterprise
                DateTime? enterpriseStartDate = null;
                DateTime? enterpriseEndDate = null;
                int enterpriseRequirementCount = 0;
                double enterpriseRequirementCompletedCount = 0;

                //First lets retrieve the active portfolios
                List<Portfolio> portfolios = portfolioManager.Portfolio_Retrieve();
                workspace = new WorkspaceData();
                workspace.Portfolios = new List<WorkspaceItem>();
                workspace.Programs = new List<WorkspaceItem>();
                workspace.Products = new List<WorkspaceItem>();
                workspace.Releases = new List<WorkspaceItem>();
                workspace.Sprints = new List<WorkspaceItem>();
                foreach (Portfolio portfolio in portfolios)
                {
                    int portfolioId = portfolio.PortfolioId;
                    int portfolioRequirementsCount = portfolio.RequirementCount;
                    WorkspaceItem portfolioItem = new WorkspaceItem();
                    portfolioItem.ParentId = ENTERPRISE_ID;
                    portfolioItem.WorkspaceId = portfolio.PortfolioId;
                    portfolioItem.WorkspaceName = portfolio.Name;
                    portfolioItem.StartDate = portfolio.StartDate;
                    portfolioItem.EndDate = portfolio.EndDate;
                    portfolioItem.RequirementsAll = portfolio.RequirementCount;
                    portfolioItem.PercentComplete = portfolio.PercentComplete;
                    workspace.Portfolios.Add(portfolioItem);

                    //Update the running counts for the enterprise
                    enterpriseRequirementCount += portfolioRequirementsCount;
                    enterpriseRequirementCompletedCount += ((double)portfolioRequirementsCount * (double)portfolio.PercentComplete) / 100;
                    if (portfolio.StartDate.HasValue && (!enterpriseStartDate.HasValue || portfolio.StartDate < enterpriseStartDate))
                    {
                        enterpriseStartDate = portfolio.StartDate;
                    }
                    if (portfolio.EndDate.HasValue && (!enterpriseEndDate.HasValue || portfolio.EndDate > enterpriseEndDate))
                    {
                        enterpriseEndDate = portfolio.EndDate;
                    }

                    //Next lets get  the programs
                    List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolioId);
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

                //Finally lets also add the dummy 'portfolio' for the programs that are not part of a portfolio
                {
                    int defaultPortfolio_requirementCount = 0;
                    int defaultPortfolio_percentComplete = 0;
                    double defaultPortfolio_requirementCompletedCount = 0;
                    DateTime? defaultPortfolio_startDate = null;
                    DateTime? defaultPortfolio_endDate = null;

                    //Next lets get the programs
                    List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(null);
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

                        //Update the running counts for the dummy portfolio
                        defaultPortfolio_requirementCount += projectGroup.RequirementCount;
                        defaultPortfolio_requirementCompletedCount += ((double)projectGroup.RequirementCount * (double)projectGroup.PercentComplete) / 100;
                        if (projectGroup.StartDate.HasValue && (!defaultPortfolio_startDate.HasValue || projectGroup.StartDate < defaultPortfolio_startDate))
                        {
                            defaultPortfolio_startDate = projectGroup.StartDate;
                        }
                        if (projectGroup.EndDate.HasValue && (!defaultPortfolio_endDate.HasValue || projectGroup.EndDate > defaultPortfolio_endDate))
                        {
                            defaultPortfolio_endDate = projectGroup.EndDate;
                        }

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

                    //Calculate the percentage complete for the default portfolio
                    if (defaultPortfolio_requirementCount == 0)
                    {
                        defaultPortfolio_percentComplete = 0;
                    }
                    else
                    {
                        defaultPortfolio_percentComplete = (int)Math.Round((defaultPortfolio_requirementCompletedCount * 100 / (double)defaultPortfolio_requirementCount));
                    }

                    WorkspaceItem portfolioItem = new WorkspaceItem();
                    portfolioItem.ParentId = ENTERPRISE_ID;
                    portfolioItem.WorkspaceId = DEFAULT_PORTFOLIO_ID;
                    portfolioItem.WorkspaceName = Resources.ClientScript.GlobalNavigation_DefaultPortfolio;
                    portfolioItem.StartDate = defaultPortfolio_startDate;
                    portfolioItem.EndDate = defaultPortfolio_endDate;
                    portfolioItem.RequirementsAll = defaultPortfolio_requirementCount;
                    portfolioItem.PercentComplete = defaultPortfolio_percentComplete;
                    workspace.Portfolios.Add(portfolioItem);

                    //Update the running counts for the enterprise
                    enterpriseRequirementCount += defaultPortfolio_requirementCount;
                    enterpriseRequirementCompletedCount += ((double)defaultPortfolio_requirementCount * (double)defaultPortfolio_percentComplete) / 100;
                    if (defaultPortfolio_startDate.HasValue && (!enterpriseStartDate.HasValue || defaultPortfolio_startDate < enterpriseStartDate))
                    {
                        enterpriseStartDate = defaultPortfolio_startDate;
                    }
                    if (defaultPortfolio_endDate.HasValue && (!enterpriseEndDate.HasValue || defaultPortfolio_endDate > enterpriseEndDate))
                    {
                        enterpriseEndDate = defaultPortfolio_endDate;
                    }
                }

                //Add the primary information about the enterprise
                workspace.Workspace = new WorkspaceItem();
                workspace.Workspace.WorkspaceId = ENTERPRISE_ID;    //constant for the enterprise
                workspace.Workspace.WorkspaceName = Resources.Fields.Enterprise;
                workspace.Workspace.StartDate = enterpriseStartDate;
                workspace.Workspace.EndDate = enterpriseEndDate;
                workspace.Workspace.RequirementsAll = enterpriseRequirementCount;
                workspace.Workspace.PercentComplete = (int)Math.Round((enterpriseRequirementCompletedCount * 100 / (double)enterpriseRequirementCount));

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

        #region Enterprise Methods

        /// <summary>
        /// Displays the list of portfolios, programs, products, releases and builds for the entire system
        /// </summary>
        /// <param name="rowsToDisplay">The max number of releases/sprints to display per project</param>
        /// <returns>The list of portfolios, programs, products, releases and builds</returns>
        public WorkspaceData Enterprise_RetrieveBuilds(int rowsToDisplay)
        {
            const string METHOD_NAME = "Enterprise_RetrieveBuilds";

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
                BuildManager buildManager = new BuildManager();

                //The data being returned
                WorkspaceData workspace = new WorkspaceData();

                //First lets retrieve the active portfolios
                List<Portfolio> portfolios = portfolioManager.Portfolio_Retrieve();
                workspace.Portfolios = new List<WorkspaceItem>();
                foreach (Portfolio portfolio in portfolios)
                {
                    bool containsBuilds_portfolio = false;
                    WorkspaceItem portfolioItem = new WorkspaceItem();
                    portfolioItem.ParentId = ENTERPRISE_ID;
                    portfolioItem.WorkspaceId = portfolio.PortfolioId;
                    portfolioItem.WorkspaceName = portfolio.Name;

                    //Now get the programs in this portfolio
                    List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(portfolio.PortfolioId);
                    if (projectGroups.Count > 0)
                    {
                        portfolioItem.Children = new List<WorkspaceItem>();
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
                                        containsBuilds_portfolio = true;
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
                                portfolioItem.Children.Add(programItem);
                            }
                        }
                    }

                    if (containsBuilds_portfolio)
                    {
                        workspace.Portfolios.Add(portfolioItem);
                    }
                }

                //Finally lets also add the dummy 'portfolio' for the programs that are not part of a portfolio
                {
                    //Next lets get the programs
                    List<ProjectGroup> projectGroups = projectGroupManager.ProjectGroup_RetrieveByPortfolio(null);
                    if (projectGroups.Count > 0)
                    {
                        bool containsBuilds_portfolio = false;

                        //Create the default portfolio
                        WorkspaceItem portfolioItem = new WorkspaceItem();
                        portfolioItem.ParentId = ENTERPRISE_ID;
                        portfolioItem.WorkspaceId = DEFAULT_PORTFOLIO_ID;
                        portfolioItem.WorkspaceName = Resources.ClientScript.GlobalNavigation_DefaultPortfolio;

                        portfolioItem.Children = new List<WorkspaceItem>();
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
                                        containsBuilds_portfolio = true;
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
                                portfolioItem.Children.Add(programItem);
                            }
                        }

                        if (containsBuilds_portfolio)
                        {
                            workspace.Portfolios.Add(portfolioItem);
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
