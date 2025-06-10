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
    public class ProjectService : AjaxWebServiceBase, IProjectService
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.ProjectService::";

        #region IWorkspace Methods

        /// <summary>
        /// Displays the overall progress of the project
        /// </summary>
        /// <param name="workspaceId">The id of the project</param>
        /// <returns>The workspace overview, or null if not found</returns>
        public WorkspaceData Workspace_RetrieveCompletionData(int workspaceId)
        {
            const string METHOD_NAME = "Workspace_RetrieveCompletionData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authenticated
            if (!CurrentUserId.HasValue)
            {
                throw new FaultException(AjaxAuthModule.AUTHENTICATION_MESSAGE);
            }
            int userId = CurrentUserId.Value;

            //Make sure we're authorized
            int projectId = workspaceId;
            Project.AuthorizationState authorizationState = IsAuthorized(projectId, Project.PermissionEnum.View, Artifact.ArtifactTypeEnum.Requirement);
            if (authorizationState == Project.AuthorizationState.Prohibited || authorizationState == Project.AuthorizationState.Limited)
            {
                throw new FaultException(AjaxAuthModule.AUTHORIZATION_MESSAGE);
            }

            try
            {
                //The data being returned
                WorkspaceData workspace = null;

                //Get the release id from settings, see if we're displaying for project, release or sprint
                int? releaseId = GetProjectSetting(userId, projectId, GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
                if (releaseId.HasValue && releaseId < 0)
                {
                    releaseId = null;
                }

                //First lets retrieve the project
                ProjectManager projectManager = new ProjectManager();
                Project project = projectManager.RetrieveById(projectId);
                if (project != null)
                {
                    workspace = new WorkspaceData();

                    //If we are filtering by release, then display sprints, otherwise don't
                    if (releaseId.HasValue && releaseId > 0)
                    {
                        workspace.DisplaySprints = true;
                    }
                    else
                    {
                        workspace.DisplaySprints = false;
                    }

                    //See if we have a release/sprint or project
                    if (releaseId.HasValue)
                    {
                        //Release/Sprint
                        ReleaseManager releaseManager = new ReleaseManager();
                        ReleaseView release = releaseManager.RetrieveById2(projectId, releaseId.Value);
                        if (release != null)
                        {
                            //Add the primary information about the release
                            workspace.Workspace = new WorkspaceItem();
                            workspace.Workspace.WorkspaceId = release.ReleaseId;
                            workspace.Workspace.WorkspaceName = release.Name;
                            workspace.Workspace.StartDate = release.StartDate;
                            workspace.Workspace.EndDate = release.EndDate;
                            workspace.Workspace.RequirementsAll = release.RequirementCount;
                            workspace.Workspace.PercentComplete = release.PercentComplete;

                            //If we have a release, get the child releases and sprints
                            if (!release.IsIterationOrPhase)
                            {
                                List<ReleaseView> releasesAndSprints = releaseManager.RetrieveSelfAndChildren(projectId, release.ReleaseId, true, true);

                                //Sort into releases and iterations/sprints/phases and return as two separate collections

                                //Releases
                                workspace.Releases = new List<WorkspaceItem>();
                                foreach (ReleaseView childRelease in releasesAndSprints.Where(r => !r.IsIterationOrPhase && r.ReleaseId != release.ReleaseId))
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
                                    releaseItem.ParentId = release.ReleaseId;
                                    releaseItem.ParentIsSameType = true;
                                    releaseItem.WorkspaceId = childRelease.ReleaseId;
                                    releaseItem.WorkspaceName = childRelease.Name;
                                    releaseItem.StartDate = childRelease.StartDate;
                                    releaseItem.EndDate = childRelease.EndDate;
                                    releaseItem.RequirementsAll = childRelease.RequirementCount;
                                    releaseItem.PercentComplete = childRelease.PercentComplete;
                                    workspace.Releases.Add(releaseItem);
                                }

                                //Sprints/Phases/Iterations
                                workspace.Sprints = new List<WorkspaceItem>();
                                foreach (ReleaseView sprint in releasesAndSprints.Where(r => r.IsIterationOrPhase))
                                {
                                    //Find the parent by indent level
                                    string parentIndentLevel = sprint.IndentLevel.SafeSubstring(0, sprint.IndentLevel.Length - 3);
                                    ReleaseView parent = releasesAndSprints.FirstOrDefault(r => r.IndentLevel == parentIndentLevel);

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
                    }
                    else
                    {
                        //Project
                        //Add the primary information about the project
                        workspace.Workspace = new WorkspaceItem();
                        workspace.Workspace.WorkspaceId = project.ProjectId;
                        workspace.Workspace.WorkspaceName = project.Name;
                        workspace.Workspace.StartDate = project.StartDate;
                        workspace.Workspace.EndDate = project.EndDate;
                        workspace.Workspace.RequirementsAll = project.RequirementCount;
                        workspace.Workspace.PercentComplete = project.PercentComplete;

                        //Next lets get the releases and sprints in the project
                        ReleaseManager releaseManager = new ReleaseManager();
                        List<ReleaseView> releasesAndSprints = releaseManager.RetrieveByProjectId(projectId, true, true);

                        //Sort into releases and iterations/sprints/phases and return as two separate collections

                        //Releases
                        workspace.Releases = new List<WorkspaceItem>();
                        foreach (ReleaseView release in releasesAndSprints.Where(r => !r.IsIterationOrPhase))
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

                        //Sprints/Phases/Iterations
                        workspace.Sprints = new List<WorkspaceItem>();
                        foreach (ReleaseView sprint in releasesAndSprints.Where(r => r.IsIterationOrPhase))
                        {
                            //Find the parent by indent level
                            string parentIndentLevel = sprint.IndentLevel.SafeSubstring(0, sprint.IndentLevel.Length - 3);
                            ReleaseView parent = releasesAndSprints.FirstOrDefault(r => r.IndentLevel == parentIndentLevel);

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
            catch (ArtifactNotExistsException)
            {
                //Return no data
                return null;
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
