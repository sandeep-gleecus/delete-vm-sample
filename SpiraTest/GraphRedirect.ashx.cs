using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// Redirects the user to a page from a graph drilldown
    /// </summary>
    public class GraphRedirect : IHttpHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.GraphRedirect::";

        public void ProcessRequest(HttpContext context)
        {
            const string METHOD_NAME = "ProcessRequest";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Make sure we have an authenticated user
                if (!CurrentUserId.HasValue)
                {
                    return;
                }

                //Get the graph name
                string graph = context.Request.QueryString["graph"];

                #region Test Execution Status

                if (graph != null && graph.ToLowerInvariant().Trim() == "testexecutionstatus")
                {
                    int projectId;
                    int? releaseId = null;
                    int executionStatusId;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        if (!String.IsNullOrEmpty(context.Request.QueryString["executionStatusId"]) && Int32.TryParse(context.Request.QueryString["executionStatusId"], out executionStatusId))
                        {
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }

                            //Set the test case list release filter if necessary
                            ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS);
                            projectSettingsCollection.Restore();
                            if (releaseId.HasValue)
                            {
                                projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID] = releaseId.Value;
                            }
                            else if (projectSettingsCollection.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID))
                            {
                                projectSettingsCollection.Remove(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID);
                            }

                            //Also reset to the root folder (so the counts match)
                            if (projectSettingsCollection.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID))
                            {
                                projectSettingsCollection.Remove(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID);
                            }
                            projectSettingsCollection.Save();

                            //Store the execution status as the test list filter and redirect to the list
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            //Set the execution status id and redirect
                            filters.Add("ExecutionStatusId", executionStatusId);
                            filters.Save();
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, projectId), true);
                        }
                    }
                }

                #endregion

                #region Test Set Status Graph

                if (graph != null && graph.ToLowerInvariant().Trim() == "testsetstatus")
                {
                    int projectId;
                    int? releaseId = null;
                    int executionStatusId;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        if (!String.IsNullOrEmpty(context.Request.QueryString["executionStatusId"]) && Int32.TryParse(context.Request.QueryString["executionStatusId"], out executionStatusId))
                        {
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }

                            //Store the execution status as the test set list filter and redirect to the list
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();

                            //Now we need to convert the execution status into the most appropriate execution status filter
                            int executionStatusFilterId = -1;
                            switch (executionStatusId)
                            {
                                case 1:
                                    //Failed
                                    executionStatusFilterId = 7;   //Only show > 0% failed
                                    break;

                                case 2:
                                    //Passed
                                    executionStatusFilterId = 4;   //Only show > 0% passed
                                    break;

                                case 3:
                                    //Not Run
                                    executionStatusFilterId = 3;   //Only show < 100% run
                                    break;

                                case 4:
                                    //Blocked
                                    executionStatusFilterId = 13;   //Only show > 0% blocked
                                    break;

                                case 5:
                                    //Caution
                                    executionStatusFilterId = 10;   //Only show > 0% caution
                                    break;
                            }

                            if (executionStatusFilterId != -1)
                            {
                                filters.Add("ExecutionStatusId", executionStatusFilterId);
                            }
                            filters.Save();

                            //Also reset to the root folder (so the counts match)
                            ProjectSettingsCollection projectSettingsCollection = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS);
                            projectSettingsCollection.Restore();
                            if (projectSettingsCollection.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID))
                            {
                                projectSettingsCollection.Remove(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_NODE_ID);
                            }

                            //Set the release 'display data for', if appropriate
                            if (releaseId.HasValue)
                            {
                                projectSettingsCollection[GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID] = releaseId.Value;
                            }
                            else if (projectSettingsCollection.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID))
                            {
                                projectSettingsCollection.Remove(GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID);
                            }
                            projectSettingsCollection.Save();

                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, projectId), true);
                        }
                    }
                }

                #endregion

                #region Incident Aging Graph

                if (graph != null && graph.ToLowerInvariant().Trim() == "incidentaging")
                {
                    int projectId;
                    int? releaseId = null;
                    string category;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        if (!String.IsNullOrEmpty(context.Request.QueryString["category"]))
                        {
                            category = context.Request.QueryString["category"].Trim();
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }

                            //Set the resolved release and age range and redirect
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            if (releaseId.HasValue && releaseId > 0)
                            {
                                filters.Add("DetectedReleaseId", releaseId.Value);
                            }

                            //Now we need to convert the incident age range into the most appropriate coverage filter
                            DateRange dateRange = new DateRange();
                            dateRange.ConsiderTimes = false;
                            switch (category)
                            {
                                case "0-15":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-15);
                                        dateRange.EndDate = DateTime.UtcNow;
                                    }
                                    break;

                                case "16-30":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-30);
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-16);
                                    }
                                    break;

                                case "31-45":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-45);
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-31);
                                    }
                                    break;

                                case "46-60":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-60);
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-46);
                                    }
                                    break;

                                case "61-75":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-75);
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-61);
                                    }
                                    break;

                                case "76-90":
                                    {
                                        dateRange.StartDate = DateTime.UtcNow.AddDays(-90);
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-76);
                                    }
                                    break;

                                case "> 90":
                                    {
                                        dateRange.StartDate = null;
                                        dateRange.EndDate = DateTime.UtcNow.AddDays(-90);
                                    }
                                    break;
                            }

                            filters.Add("IncidentStatusId", IncidentManager.IncidentStatusId_AllOpen);
                            filters.Add("CreationDate", dateRange);
                            filters.Save();

                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId), true);
                        }
                    }
                }

                #endregion

                #region Incident Open / Closed Status Graph

                if (graph != null && graph.ToLowerInvariant().Trim() == "incidentopenclosedcount")
                {
                    int projectId;
                    int? releaseId = null;
                    string category;
                    bool useResolvedRelease = false;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        if (!String.IsNullOrEmpty(context.Request.QueryString["category"]))
                        {
                            category = context.Request.QueryString["category"].Trim();
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }
                            if (!String.IsNullOrEmpty(context.Request.QueryString["useResolvedRelease"]) && context.Request.QueryString["useResolvedRelease"].ToLowerInvariant() == "true")
                            {
                                useResolvedRelease = true;
                            }

                            //Set the release and status filters
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            if (releaseId.HasValue && releaseId > 0)
                            {
                                if (useResolvedRelease)
                                {
                                    filters.Add("ResolvedReleaseId", releaseId.Value);
                                }
                                else
                                {
                                    filters.Add("DetectedReleaseId", releaseId.Value);
                                }
                            }

                            if (category == "(All Open)")
                            {
                                filters.Add("IncidentStatusId", (int)IncidentManager.IncidentStatusId_AllOpen);
                            }
                            if (category == "(All Closed)")
                            {
                                filters.Add("IncidentStatusId", (int)IncidentManager.IncidentStatusId_AllClosed);
                            }
                            filters.Save();

                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId), true);
                        }
                    }
                }

                #endregion

                #region Incident Open Status by Priority

                if (graph != null && graph.ToLowerInvariant().Trim() == "incidentopencountbypriority")
                {
                    int projectId;
                    int? releaseId = null;
                    string category;
                    bool useResolvedRelease = false;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        //Get the template associated with the project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        if (!String.IsNullOrEmpty(context.Request.QueryString["category"]))
                        {
                            category = context.Request.QueryString["category"].Trim();
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }
                            if (!String.IsNullOrEmpty(context.Request.QueryString["useResolvedRelease"]) && context.Request.QueryString["useResolvedRelease"].ToLowerInvariant() == "true")
                            {
                                useResolvedRelease = true;
                            }

                            //Set the release and status filters
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            if (releaseId.HasValue && releaseId > 0)
                            {
                                if (useResolvedRelease)
                                {
                                    filters.Add("ResolvedReleaseId", releaseId.Value);
                                }
                                else
                                {
                                    filters.Add("DetectedReleaseId", releaseId.Value);
                                }
                            }

                            //See if we can find a priority with a matching name
                            IncidentManager incidentManager = new IncidentManager();
                            List<IncidentPriority> priorities = incidentManager.RetrieveIncidentPriorities(projectTemplateId, false);
                            IncidentPriority incidentPriority = priorities.FirstOrDefault(p => p.Name == category);
                            if (incidentPriority != null)
                            {
                                MultiValueFilter mvf = new MultiValueFilter();
                                mvf.Values.Add(incidentPriority.PriorityId);
                                filters.Add("PriorityId", mvf);
                            }
                            else
                            {
                                filters.Add("PriorityId", new MultiValueFilter() { IsNone = true });
                            }
                            filters.Add("IncidentStatusId", (int)IncidentManager.IncidentStatusId_AllOpen);
                            filters.Save();

                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId), true);
                        }
                    }
                }
                
                #endregion

                #region Incident Open Status by Severity

                if (graph != null && graph.ToLowerInvariant().Trim() == "incidentopencountbyseverity")
                {
                    int projectId;
                    int? releaseId = null;
                    string category;
                    bool useResolvedRelease = false;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        //Get the template associated with the project
                        int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                        if (!String.IsNullOrEmpty(context.Request.QueryString["category"]))
                        {
                            category = context.Request.QueryString["category"].Trim();
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }
                            if (!String.IsNullOrEmpty(context.Request.QueryString["useResolvedRelease"]) && context.Request.QueryString["useResolvedRelease"].ToLowerInvariant() == "true")
                            {
                                useResolvedRelease = true;
                            }

                            //Set the release and status filters
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            if (releaseId.HasValue && releaseId > 0)
                            {
                                if (useResolvedRelease)
                                {
                                    filters.Add("ResolvedReleaseId", releaseId.Value);
                                }
                                else
                                {
                                    filters.Add("DetectedReleaseId", releaseId.Value);
                                }
                            }

                            //See if we can find a severity with a matching name
                            IncidentManager incidentManager = new IncidentManager();
                            List<IncidentSeverity> severities = incidentManager.RetrieveIncidentSeverities(projectTemplateId, false);
                            IncidentSeverity incidentSeverity = severities.FirstOrDefault(p => p.Name == category);
                            if (incidentSeverity != null)
                            {
                                MultiValueFilter mvf = new MultiValueFilter();
                                mvf.Values.Add(incidentSeverity.SeverityId);
                                filters.Add("SeverityId", mvf);
                            }
                            else
                            {
                                filters.Add("SeverityId", new MultiValueFilter() { IsNone = true });
                            }

                            filters.Add("IncidentStatusId", (int)IncidentManager.IncidentStatusId_AllOpen);
                            filters.Save();

                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId), true);
                        }
                    }
                }
                
                #endregion

                #region Requirements Coverage (New) Graph

                if (graph != null && graph.ToLowerInvariant().Trim() == "requirementscoveragenew")
                {
                    int projectId;
                    int? releaseId = null;
                    int index;
                    if (!String.IsNullOrEmpty(context.Request.QueryString["projectId"]) && Int32.TryParse(context.Request.QueryString["projectId"], out projectId))
                    {
                        if (!String.IsNullOrEmpty(context.Request.QueryString["index"]) && Int32.TryParse(context.Request.QueryString["index"], out index))
                        {
                            if (!String.IsNullOrEmpty(context.Request.QueryString["releaseId"]))
                            {
                                releaseId = Int32.Parse(context.Request.QueryString["releaseId"]);
                            }

                            //Set the release and coverage filter id and redirect
                            ProjectSettingsCollection filters = new ProjectSettingsCollection(projectId, CurrentUserId.Value, GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST);
                            filters.Restore();
                            filters.Clear();
                            if (releaseId.HasValue && releaseId > 0)
                            {
                                filters.Add("ReleaseId", releaseId.Value);
                            }

                            //Now we need to convert the coverage status into the most appropriate coverage filter
                            int coverageFilterId = -1;
                            switch (index)
                            {
                                case 1:
                                    //Passed
                                    coverageFilterId = 15;   //Only show > 0% passed requirements
                                    break;

                                case 2:
                                    //Failed
                                    coverageFilterId = 5;   //Only show > 0% failed requirements
                                    break;

                                case 3:
                                    //Blocked
                                    coverageFilterId = 11;   //Only show > 0% blocked requirements
                                    break;

                                case 4:
                                    //Caution
                                    coverageFilterId = 8;   //Only show > 0% caution requirements
                                    break;

                                case 5:
                                    //Not Run
                                    coverageFilterId = 4;   //Only show < 100% run requirements
                                    break;

                                case 6:
                                    //Not Covered
                                    coverageFilterId = 1;   //Only show not covered requirements
                                    break;
                            }

                            if (coverageFilterId != -1)
                            {
                                filters.Add("CoverageId", coverageFilterId);
                            }
                            filters.Save();

                            //Before redirecting, also expand all packages so that the counts match
                            new RequirementManager().ExpandToLevel(CurrentUserId.Value, projectId, null);
                            context.Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, projectId), true);
                        }
                    }
                }

                #endregion

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();

                //Display the error message on the screen
                context.Response.Write(exception.Message);
            }
        }


        /// <summary>
        /// Returns the ID of the currently logged-in user
        /// </summary>
        public int? CurrentUserId
        {
            get
            {
                MembershipUser user = Membership.GetUser();
                if (user != null)
                {
                    return (int)user.ProviderUserKey;
                }
                return null;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}