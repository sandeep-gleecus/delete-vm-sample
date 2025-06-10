using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;

using System.Web.SessionState;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Redirects the user to the appropriate list page, loading the appropriate saved filter first
    /// </summary>
    public class SavedFilter : LocalizedHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SavedFilter::";

        /// <summary>
        /// Does the actual redirect
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(HttpContext context)
        {
            base.ProcessRequest(context);

            const string METHOD_NAME = "ProcessRequest";

            try
            {
                //Load a particular saved search, put that project in session and redirect to the appropriate list page

                //First get the id of the saved search we want to use
                int savedFilterId = 0;
                if (!String.IsNullOrWhiteSpace(context.Request.QueryString[GlobalFunctions.PARAMETER_SAVED_FILTER_ID]) && Int32.TryParse(context.Request.QueryString[GlobalFunctions.PARAMETER_SAVED_FILTER_ID], out savedFilterId))
                {
                    //Now restore the saved filter to determine the artifact and project id
                    Business.SavedFilterManager savedFilterManager = new Business.SavedFilterManager();
                    savedFilterManager.Restore(savedFilterId);
                    if (!savedFilterManager.ProjectId.HasValue)
                    {
                        Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "There is no project id set, so redirecting to MyPage");
                        Logger.Flush();
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId), true);
                    }
                    int projectId = savedFilterManager.ProjectId.Value;
                    DataModel.Artifact.ArtifactTypeEnum artifactType = savedFilterManager.Type;

                    //If this user doesn't match the current user then redirect (security issue)
                    //unless it's a shared filter
                    if (savedFilterManager.UserId != CurrentUserId.Value && !savedFilterManager.IsShared)
                    {
                        Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "The user IDs do not match, so redirecting to MyPage");
                        Logger.Flush();
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId), true);
                    }

                    //If this project doesn't match the specified project then redirect (security issue)
                    if (projectId != ProjectId)
                    {
                        Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "The project IDs do not match, so redirecting to MyPage");
                        Logger.Flush();
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId), true);
                    }

                    //Get the appropriate settings collections for the artifact type in question
                    //Sortable artifacts actually have two collections
                    string filterCollectionName = "";
                    string sortCollectionName = "";
                    switch (artifactType)
                    {
                        case DataModel.Artifact.ArtifactTypeEnum.Requirement:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Release:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestRun:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Incident:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Task:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Risk:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_FILTERS;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_RISK_LIST_GENERAL;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Document:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS;
                            break;
                    }

                    //Now restore the appropriate project settings collection
                    if (projectId == -1 || artifactType == DataModel.Artifact.ArtifactTypeEnum.None || filterCollectionName == "")
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to match project or artifact type for saved filter " + savedFilterId);
                        Logger.Flush();
                        context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + context.Server.UrlEncode(Resources.Messages.SavedSearches_UnableToRetrieveSeach), true);
                    }
                    ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, CurrentUserId.Value, filterCollectionName);
                    ProjectSettingsCollection sortProjectSettings = null;
                    if (sortCollectionName != "")
                    {
                        sortProjectSettings = new ProjectSettingsCollection(ProjectId, CurrentUserId.Value, sortCollectionName);
                    }

                    //Now populate the collection(s) from the saved filter
                    savedFilterManager.Populate(filterProjectSettings, sortProjectSettings, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION);

                    //Now set the project to the appropriate one and redirect to the appropriate page
                    Business.ProjectManager projectManager = new Business.ProjectManager();
                    Project project = projectManager.RetrieveById(projectId);
                    string projectName = project.Name;

                    switch (artifactType)
                    {
                        case DataModel.Artifact.ArtifactTypeEnum.Requirement:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Release:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestRun:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Incident:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Task:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Risk:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, projectId), true);
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Document:
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, projectId), true);
                            break;
                    }
                }
                else
                {
                    Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to locate saved filter " + savedFilterId);
                    Logger.Flush();
                    context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId), true);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();

                //Display the error message on the screen
                context.Response.Write(exception.Message);
            }
        }

        public override bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}