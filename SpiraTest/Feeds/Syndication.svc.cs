using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Web.Security;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Feeds
{
    /// <summary>
    /// Displays an RSS feed of a user's saved search
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Syndication : ISyndication
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Feeds.Syndication::";

        private const string BASE_NAMESPACE = "http://www.inflectra.com/spiratest/";

        /// <summary>
        /// The ID of the current user
        /// </summary>
        public int UserId
        {
            get
            {
                return this.userId;
            }
        }
        private int userId = -1;

        /// <summary>
        /// The display name of the current user
        /// </summary>
        public string UserFullName
        {
            get
            {
                return this.userFullName;
            }
        }
        private string userFullName = "";

        /// <summary>
        /// Returns the current timezone for this user (taking into account, server, application and profile settings)
        /// </summary>
        public string TimezoneId
        {
            get
            {
                return this.timezoneId;
            }
        }
        private string timezoneId = "";

        protected List<ProjectUser> projectMembership;

        /// <summary>
        /// Sets the culture and populates some of the common properties
        /// </summary>
        /// <param name="userId">The user's user ID</param>
        /// <param name="rssToken">
        /// The user's RSS token
        /// </param>
        protected void Initialize(int userId, string rssToken)
        {
            const string METHOD_NAME = "Initialize";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Validate the user based in user id and RSS token combination. Locks the user if retries exceeded
                //which prevents a brute force attack
                string fullRssToken = "{" + rssToken + "}";
                SpiraMembershipProvider spiraProvider = (SpiraMembershipProvider)Membership.Provider;
                bool valid = spiraProvider.ValidateUserByRssToken(userId, fullRssToken);
                if (valid)
                {
                    //Retrieve the user's profile based on the ID
                    UserManager userManager = new UserManager();
                    User user = userManager.GetUserById(userId, true);

                    //Get the id and full name of the current user
                    this.userId = user.UserId;
                    this.userFullName = user.FullName;

                    //See if we have a system-wide culture set
                    if (!String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultCulture))
                    {
                        if (Thread.CurrentThread.CurrentCulture.Name != ConfigurationSettings.Default.Globalization_DefaultCulture)
                        {
                            Thread.CurrentThread.CurrentCulture = new CultureInfo(ConfigurationSettings.Default.Globalization_DefaultCulture);
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo(ConfigurationSettings.Default.Globalization_DefaultCulture);
                        }
                    }

                    //See if we need to update the thread's culture based on the user's profile
                    UserSettingsCollection userSettingsCollection = new UserSettingsCollection(UserId, GlobalFunctions.USER_SETTINGS_USER_PROFILE_SETTINGS);
                    userSettingsCollection.Restore();
                    if (userSettingsCollection.ContainsKey(GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE))
                    {
                        string userCulture = (string)userSettingsCollection[GlobalFunctions.USER_SETTINGS_KEY_CURRENT_CULTURE];
                        if (Thread.CurrentThread.CurrentCulture.Name != userCulture)
                        {
                            Thread.CurrentThread.CurrentCulture = new CultureInfo(userCulture);
                            Thread.CurrentThread.CurrentUICulture = new CultureInfo(userCulture);
                        }
                    }

                    //Get the user's membership, we will use this later for authorization checking
                    this.projectMembership = new Business.ProjectManager().RetrieveProjectMembershipForUser(UserId);

                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                }
                else
                {
                    //We set the user as -1 so nothing will get written
                    this.userId = -1;
                }
            }
            catch (ArtifactNotExistsException exception)
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
                Logger.Flush();
                //We leave the user as -1 so nothing will get written
                return;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Gets the ~/ internal URL and makes it a fully specified absolute URL
        /// </summary>
        /// <returns>The absolute url</returns>
        protected string ResolveAbsoluteUrl(string internalUrl)
        {
            if (String.IsNullOrEmpty(internalUrl))
            {
                return internalUrl;
            }

            //Replace the ~ with the base url
            string baseUrl = ConfigurationSettings.Default.General_WebServerUrl;
            string absoluteUrl = internalUrl.Replace ("~", baseUrl);

            return absoluteUrl;
        }

        /// <summary>
        /// Localizes a UTC date for the specific user's timezone
        /// </summary>
        /// <param name="utcDate">The universal date</param>
        /// <returns>The localized date</returns>
        protected DateTime LocalizeDate(DateTime utcDate)
        {
            //If we have an unspecified date, force to utc
            if (utcDate.Kind == DateTimeKind.Unspecified)
            {
                utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            }

            //If we have a local date already, do nothing
            if (utcDate.Kind == DateTimeKind.Local)
            {
                return utcDate;
            }

            if (String.IsNullOrEmpty(TimezoneId))
            {
                //Fallback to using the system local time
                return utcDate.ToLocalTime();
            }
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimezoneId);
            if (timeZone == null)
            {
                //Fallback to using the system local time
                return utcDate.ToLocalTime();
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZone);
        }

        /// <summary>
        /// Populates the generic (non-feed specific info)
        /// </summary>
        /// <param name="feed">The feed handle</param>
        protected void PopulateFeedInfo(SyndicationFeed feed)
        {
            feed.Authors.Add(new SyndicationPerson(ConfigurationSettings.Default.EmailSettings_EMailFrom));
            feed.Contributors.Add(new SyndicationPerson(ConfigurationSettings.Default.EmailSettings_EMailFrom));
            feed.Categories.Add(new SyndicationCategory("/Computers/Software/Project_Management/", "http://www.dmoz.org", "Project Management"));
            feed.Categories.Add(new SyndicationCategory("/Computers/Software/Quality_Assurance/", "http://www.dmoz.org", "Quality Assurance"));
            feed.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");
            feed.Language = Thread.CurrentThread.CurrentUICulture.Name;
            feed.Generator = ConfigurationSettings.Default.License_ProductType;
            feed.ElementExtensions.Add(new SyndicationElementExtension("ttl", "", "120"));
        }

        /// <summary>
        /// Displays the list of artifacts in a user's saved filter/search
        /// </summary>
        /// <param name="rssToken">The RSS token</param>
        /// <param name="savedFilterId">The id of the saved filter</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS 2.0 feed</returns>
        public Rss20FeedFormatter SavedSearch(int userId, string rssToken, int savedFilterId)
        {
            const string METHOD_NAME = "SavedSearch";

            //For safety we only get the first 100 matching results
            const int MAX_NUMBER_RECORDS = 100;

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //Now restore the saved filter to determine the artifact and project id
                    SavedFilterManager savedFilterManager = new SavedFilterManager();
                    savedFilterManager.Restore(savedFilterId);
                    if (!savedFilterManager.ProjectId.HasValue)
                    {
                        Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "There is no project id set, so redirecting to MyPage");
                        Logger.Flush();
                        return null;
                    }
                    int projectId = savedFilterManager.ProjectId.Value;
                    DataModel.Artifact.ArtifactTypeEnum artifactType = savedFilterManager.Type;

                    //If this user doesn't match the current user then return nothing (security issue)
                    if (savedFilterManager.UserId != UserId)
                    {
                        Logger.LogFailureAuditEvent(CLASS_NAME + METHOD_NAME, "The user IDs do not match, so redirecting to MyPage");
                        Logger.Flush();
                        return null;
                    }

                    //Get the appropriate settings collections for the artifact type in question
                    //Sortable artifacts actually have two collections
                    string filterCollectionName = "";
                    string sortCollectionName = "";
                    string sortProperty = "";
                    bool sortAscending = true;

                    switch (artifactType)
                    {
                        case DataModel.Artifact.ArtifactTypeEnum.Requirement:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS;
                            sortProperty = "RequirementId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Release:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_RELEASE_FILTERS_LIST;
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_CASE_GENERAL_SETTINGS;
                            sortProperty = "TestCaseId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_SET_GENERAL_SETTINGS;
                            sortProperty = "TestSetId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Incident:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_INCIDENT_SORT_EXPRESSION;
                            sortProperty = "IncidentId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Task:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TASK_SORT_EXPRESSION;
                            sortProperty = "TaskId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.TestRun:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_TEST_RUN_SORT_EXPRESSION;
                            sortProperty = "TestRunId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_AUTOMATION_HOST_SORT_EXPRESSION;
                            sortProperty = "AutomationHostId";
                            break;
                        case DataModel.Artifact.ArtifactTypeEnum.Document:
                            filterCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_FILTERS_LIST;
                            sortCollectionName = GlobalFunctions.PROJECT_SETTINGS_DOCUMENTS_GENERAL_SETTINGS;
                            sortProperty = "AttachmentId";
                            break;
                    }

                    //Now restore the appropriate project settings collection
                    if (projectId == -1 || artifactType == DataModel.Artifact.ArtifactTypeEnum.None || filterCollectionName == "")
                    {
                        //Return nothing since it is an unknown type of saved filter
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to match project or artifact type for saved filter " + savedFilterId);
                        Logger.Flush();
                        return null;
                    }
                    ProjectSettingsCollection filterProjectSettings = new ProjectSettingsCollection(projectId, UserId, filterCollectionName);

                    //Extract the sort info
                    ProjectSettingsCollection sortProjectSettings = null;
                    if (sortCollectionName != "")
                    {
                        sortProjectSettings = new ProjectSettingsCollection(projectId, UserId, sortCollectionName);
                        if (sortProjectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION] != null)
                        {
                            string sortExpression = (string)sortProjectSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION];
                            sortProperty = sortExpression.Substring(0, sortExpression.IndexOf(" "));
                            string sortDirectionString = sortExpression.Substring(sortExpression.IndexOf(" "), sortExpression.Length - sortExpression.IndexOf(" ")).Trim();
                            sortAscending = (sortDirectionString == "ASC");
                        }
                    }

                    //Now populate the collection(s) from the saved filter
                    savedFilterManager.Populate(filterProjectSettings, sortProjectSettings, GlobalFunctions.PROJECT_SETTINGS_KEY_SORT_EXPRESSION);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MySavedSearches + " (" + savedFilterManager.Name + ")");
                    feed.Description = new TextSyndicationContent(Resources.Main.Syndication_SavedSearchesDescription);
                    feed.Id = BASE_NAMESPACE + "mypage/savedsearches";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SavedSearch, projectId, savedFilterId))));

                    //Now load the appropriate type of items and generate the feed
                    DiscussionManager discussion = new DiscussionManager();
                    switch (artifactType)
                    {
                        case DataModel.Artifact.ArtifactTypeEnum.Requirement:
                            List<RequirementView> requirements = new RequirementManager().Retrieve(userId, projectId, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                            PopulateFeedItems(feed, requirements, discussion);
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.Release:
                            {
                                List<ReleaseView> releases = new ReleaseManager().RetrieveByProjectId(userId, projectId, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, releases, discussion);
                            }
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.TestCase:
                            {
                                List<TestCaseView> testCases = new TestCaseManager().Retrieve(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, testCases, discussion);
                            }
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.TestSet:
                            {
                                List<TestSetView> testSets = new TestSetManager().Retrieve(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset(), null, false);
                                PopulateFeedItems(feed, testSets, discussion);
                            }
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.Incident:
                            {
                                IncidentManager incidentManager = new IncidentManager();
                                List<IncidentView> incidents = incidentManager.Retrieve(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, incidents, incidentManager);
                            }
                            break;
                        
                        case DataModel.Artifact.ArtifactTypeEnum.Task:
                            List<TaskView> tasks = new TaskManager().Retrieve(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                            PopulateFeedItems(feed, tasks, discussion);
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.TestRun:
                            {
                                TestRunManager testRunManager = new TestRunManager();
                                List<TestRunView> testRuns = testRunManager.Retrieve(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, testRuns, testRunManager, projectId);
                            }
                            break;

                        case DataModel.Artifact.ArtifactTypeEnum.AutomationHost:
                            {
                                AutomationManager automationManager = new AutomationManager();
                                List<AutomationHostView> automationHosts = automationManager.RetrieveHosts(projectId, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, automationHosts);
                            }
                            break;


                        case DataModel.Artifact.ArtifactTypeEnum.Document:
                            {
                                AttachmentManager attachmentManager = new AttachmentManager();
                                List<ProjectAttachmentView> attachments = attachmentManager.RetrieveForProject(projectId, null, sortProperty, sortAscending, 1, MAX_NUMBER_RECORDS, filterProjectSettings, GlobalFunctions.GetCurrentTimezoneUtcOffset());
                                PopulateFeedItems(feed, attachments, discussion);
                            }
                            break;

                    }

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the list of assigned incidents
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS Feed</returns>
        public Rss20FeedFormatter AssignedIncidents(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedIncidents";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of incidents belonging to the current user
                    IncidentManager incidentManager = new IncidentManager();
                    UserManager user = new UserManager();
                    List<IncidentView> incidents = incidentManager.RetrieveOpenByOwnerId(userId, null, null);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedIncidents + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "incidents/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of incidents
                    PopulateFeedItems(feed, incidents, incidentManager);
                    
                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the list of assigned risks
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS Feed</returns>
        public Rss20FeedFormatter AssignedRisks(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedRisks";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of risks belonging to the current user
                    RiskManager riskManager = new RiskManager();
                    UserManager user = new UserManager();
                    List<RiskView> risks = riskManager.Risk_RetrieveOpenByOwnerId(userId, null, null);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedRisks + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "risks/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of risks
                    PopulateFeedItems(feed, risks, riskManager);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the list of assigned documents
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS Feed</returns>
        public Rss20FeedFormatter AssignedDocuments(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedDocuments";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of incidents belonging to the current user
                    AttachmentManager attachmentManager = new AttachmentManager();
                    DiscussionManager discussion = new DiscussionManager();
                    UserManager user = new UserManager();
                    List<ProjectAttachmentView> attachments = attachmentManager.RetrieveOpenByOpenerId(userId, null);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedDocuments + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "documents/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of incidents
                    PopulateFeedItems(feed, attachments, attachmentManager, discussion);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Populates the feed for incidents
        /// </summary>
        /// <param name="incident">The business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="dataTable">The data table</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<IncidentView> incidents, IncidentManager incidentManager)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the incidents
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (IncidentView incident in incidents)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == incident.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the incident opener's email address
                    string incidentOpenerEmailAddress = "";
                    try
                    {
                        User opener = userManager.GetUserById(incident.OpenerId);
                        incidentOpenerEmailAddress = opener.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, incident.IncidentId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + incident.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, incident.ProjectId, incident.IncidentId))));
                    item.PublishDate = LocalizeDate(incident.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(incident.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", incident.OpenerName, incidentOpenerEmailAddress), incident.OpenerName, BASE_NAMESPACE + "/user/" + incident.OpenerId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = Strings.StripHTML(incident.Description, false, true);

                    //Add the resolutions in as simple list of bullets
                    try
                    {
                        Incident incidentWithResolutions = incidentManager.RetrieveById(incident.IncidentId, true);

                        if (incidentWithResolutions.Resolutions != null && incidentWithResolutions.Resolutions.Count > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IncidentResolution resolution in incidentWithResolutions.Resolutions)
                            {
                                description = description + "<li>[" + resolution.Creator.FullName + " - " + resolution.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(resolution.Resolution, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the type, priority, severity and status in as categories
                    item.Categories.Add(new SyndicationCategory(incident.PriorityName, BASE_NAMESPACE + "incident/priority", Resources.Fields.Priority));
                    item.Categories.Add(new SyndicationCategory(incident.SeverityName, BASE_NAMESPACE + "incident/severity", Resources.Fields.Severity));
                    item.Categories.Add(new SyndicationCategory(incident.IncidentTypeName, BASE_NAMESPACE + "incident/type", Resources.Fields.IncidentTypeId));
                    item.Categories.Add(new SyndicationCategory(incident.IncidentStatusName, BASE_NAMESPACE + "incident/status", Resources.Fields.IncidentStatusId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for risks
        /// </summary>
        /// <param name="risk">The business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="dataTable">The data table</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<RiskView> risks, RiskManager riskManager)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the risks
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (RiskView risk in risks)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == risk.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Risk, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the risk opener's email address
                    string riskOpenerEmailAddress = "";
                    try
                    {
                        User opener = userManager.GetUserById(risk.CreatorId);
                        riskOpenerEmailAddress = opener.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_INCIDENT, risk.RiskId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + risk.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, risk.ProjectId, risk.RiskId))));
                    item.PublishDate = LocalizeDate(risk.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(risk.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", risk.CreatorName, riskOpenerEmailAddress), risk.CreatorName, BASE_NAMESPACE + "/user/" + risk.CreatorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = Strings.StripHTML(risk.Description, false, true);

                    //Add the comments and mitigations in as simple list of bullets
                    try
                    {
                        Risk riskWithComments = riskManager.Risk_RetrieveById(risk.RiskId, true, true);

                        //Discussions
                        if (riskWithComments.Discussions != null && riskWithComments.Discussions.Count > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (RiskDiscussion discussion in riskWithComments.Discussions)
                            {
                                description = description + "<li>[" + discussion.Creator.FullName + " - " + discussion.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussion.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }

                        //Mitigations
                        if (riskWithComments.Mitigations != null && riskWithComments.Mitigations.Count > 0)
                        {
                            description = description + "<p><u>" + Resources.ServerControls.TabControl_Mitigations + "</u></p>";
                            description = description + "<ol>";
                            foreach (RiskMitigation mitigation in riskWithComments.Mitigations)
                            {
                                description = description + "<li>[" + mitigation.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(mitigation.Description, false, true) + "</li>";
                            }
                            description = description + "</ol>";
                        }

                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the type, priority, severity and status in as categories
                    item.Categories.Add(new SyndicationCategory(risk.RiskProbabilityName, BASE_NAMESPACE + "risk/probability", Resources.Fields.RiskProbabilityId));
                    item.Categories.Add(new SyndicationCategory(risk.RiskImpactName, BASE_NAMESPACE + "risk/impact", Resources.Fields.RiskImpactId));
                    if (risk.RiskExposure.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(risk.RiskExposure.Value.ToString(), BASE_NAMESPACE + "risk/exposure", Resources.Fields.RiskExposure));
                    }
                    item.Categories.Add(new SyndicationCategory(risk.RiskTypeName, BASE_NAMESPACE + "risk/type", Resources.Fields.RiskTypeId));
                    item.Categories.Add(new SyndicationCategory(risk.RiskStatusName, BASE_NAMESPACE + "risk/status", Resources.Fields.RiskStatusId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for attachments
        /// </summary>
        /// <param name="incident">The business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="dataTable">The data table</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<ProjectAttachmentView> attachments, AttachmentManager attachmentManager, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the incidents
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (ProjectAttachmentView attachment in attachments)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == attachment.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the document editor's email address
                    string attachmentEditorEmailAddress = "";
                    try
                    {
                        User opener = userManager.GetUserById(attachment.EditorId);
                        attachmentEditorEmailAddress = opener.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT, attachment.AttachmentId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + attachment.Filename);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, attachment.ProjectId, attachment.AttachmentId))));
                    item.PublishDate = LocalizeDate(attachment.UploadDate);
                    item.LastUpdatedTime = LocalizeDate(attachment.EditedDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", attachment.EditorName, attachmentEditorEmailAddress), attachment.EditorName, BASE_NAMESPACE + "/user/" + attachment.EditorName));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (!String.IsNullOrEmpty(attachment.Description))
                    {
                        description = Strings.StripHTML(attachment.Description, false, true);
                    }

                    //Add the comments in a simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(attachment.AttachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the type, and status in as categories
                    item.Categories.Add(new SyndicationCategory(attachment.DocumentTypeName, BASE_NAMESPACE + "attachment/type", Resources.Fields.DocumentType));
                    item.Categories.Add(new SyndicationCategory(attachment.DocumentStatusName, BASE_NAMESPACE + "attachment/status", Resources.Fields.DocumentStatusId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Displays the list of assigned requirements
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter AssignedRequirements(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedRequirements";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of requirements belonging to the current user
                    RequirementManager requirementManager = new RequirementManager();
                    DiscussionManager discussion = new DiscussionManager();
                    UserManager user = new UserManager();
                    List<RequirementView> requirements = requirementManager.RetrieveByOwnerId(userId, null, null, false);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedRequirements + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "requirements/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of requirements
                    PopulateFeedItems(feed, requirements, discussion);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Populates the feed for requirements
        /// </summary>
        /// <param name="discussion">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="requirements">The list of requirements</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<RequirementView> requirements, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the requirements
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (RequirementView requirement in requirements)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == requirement.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the requirement author's email address
                    string requirementAuthorEmailAddress = "";
                    try
                    {
                        User author = userManager.GetUserById(requirement.AuthorId);
                        requirementAuthorEmailAddress = author.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT, requirement.RequirementId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + requirement.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, requirement.ProjectId, requirement.RequirementId))));
                    item.PublishDate = LocalizeDate(requirement.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(requirement.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", requirement.AuthorName, requirementAuthorEmailAddress), requirement.AuthorName, BASE_NAMESPACE + "/user/" + requirement.AuthorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (!String.IsNullOrEmpty(requirement.Description))
                    {
                        description = Strings.StripHTML(requirement.Description, false, true);
                    }

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(requirement.RequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the release, importance and status in as categories
                    if (!requirement.ImportanceId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(requirement.ImportanceName, BASE_NAMESPACE + "requirement/importance", Resources.Fields.Importance));
                    }
                    if (!requirement.ReleaseId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(requirement.ReleaseVersionNumber, BASE_NAMESPACE + "requirement/release", Resources.Fields.VersionNumber));
                    }
                    item.Categories.Add(new SyndicationCategory(requirement.RequirementStatusName, BASE_NAMESPACE + "requirement/status", Resources.Fields.ScopeLevelId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for releases
        /// </summary>
        /// <param name="discussion">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="releases">The releases</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<ReleaseView> releases, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the requirements
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (ReleaseView release in releases)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == release.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the release creator's email address
                    string releaseCreatorEmailAddress = "";
                    try
                    {
                        User creator = userManager.GetUserById(release.CreatorId);
                        releaseCreatorEmailAddress = creator.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_RELEASE, release.ReleaseId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + release.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, release.ProjectId, release.ReleaseId))));
                    item.PublishDate = LocalizeDate(release.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(release.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", release.CreatorName, releaseCreatorEmailAddress), release.CreatorName, BASE_NAMESPACE + "/user/" + release.CreatorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (!String.IsNullOrEmpty(release.Description))
                    {
                        description = Strings.StripHTML(release.Description, false, true);
                    }

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(release.ReleaseId, DataModel.Artifact.ArtifactTypeEnum.Release);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the status/type as categories
                    item.Categories.Add(new SyndicationCategory(Resources.Fields.ReleaseStatusId + "=" + release.ReleaseStatusName, BASE_NAMESPACE + "release/status", Resources.Fields.ReleaseStatusId));
                    item.Categories.Add(new SyndicationCategory(Resources.Fields.ReleaseTypeId + "=" + release.ReleaseTypeName, BASE_NAMESPACE + "release/type", Resources.Fields.ReleaseTypeId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for test cases
        /// </summary>
        /// <param name="discussion">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="testCases">The data table</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<TestCaseView> testCases, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the test cases
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (TestCaseView dataRow in testCases)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == dataRow.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View)  != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the testCase author's email address
                    string testCaseAuthorEmailAddress = "";
                    try
                    {
                        User author = userManager.GetUserById(dataRow.AuthorId);
                        testCaseAuthorEmailAddress = author.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE, dataRow.TestCaseId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + dataRow.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestCases, dataRow.ProjectId, dataRow.TestCaseId))));
                    item.PublishDate = LocalizeDate(dataRow.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(dataRow.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", dataRow.AuthorName, testCaseAuthorEmailAddress), dataRow.AuthorName, BASE_NAMESPACE + "/user/" + dataRow.AuthorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (!String.IsNullOrEmpty(dataRow.Description))
                    {
                        description = Strings.StripHTML(dataRow.Description, false, true);
                    }

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(dataRow.TestCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the priority and status in as categories
                    if (dataRow.TestCasePriorityId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(dataRow.TestCasePriorityName, BASE_NAMESPACE + "testcase/priority", Resources.Fields.TestCasePriorityId));
                    }
                    item.Categories.Add(new SyndicationCategory(dataRow.ExecutionStatusName, BASE_NAMESPACE + "testcase/executionstatus", Resources.Fields.ExecutionStatusId));
                    item.Categories.Add(new SyndicationCategory(dataRow.TestCaseStatusName, BASE_NAMESPACE + "testcase/status", Resources.Fields.TestCaseStatusId));
                    item.Categories.Add(new SyndicationCategory(dataRow.TestCaseTypeName, BASE_NAMESPACE + "testcase/type", Resources.Fields.TestCaseTypeId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Displays the list of assigned tasks
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter AssignedTasks(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedTasks";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of tasks belonging to the current user
                    TaskManager taskManager = new TaskManager();
                    DiscussionManager discussion = new DiscussionManager();
                    UserManager user = new UserManager();
                    List<TaskView> tasks = taskManager.RetrieveByOwnerId(userId, null, null, false);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedTasks + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "tasks/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of tasks
                    PopulateFeedItems(feed, tasks, discussion);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the list of subscribed artifacts
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter SubscribedArtifacts(int userId, string rssToken)
        {
            const string METHOD_NAME = "SubscribedArtifacts";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of artifacts subscribed by the current user
                    NotificationManager notificationManager = new NotificationManager();
                    List<NotificationUserSubscriptionView> notifications = notificationManager.RetrieveSubscriptionsForUser(userId, null);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MySubscribedArtifacts + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "artifacts/subscribed";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of artifacts
                    PopulateFeedItems(feed, notifications);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Populates the feed for tasks
        /// </summary>
        /// <param name="discussion">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="tasks">The list of tasks</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<TaskView> tasks, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the tasks
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (TaskView task in tasks)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == task.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View)  != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the task creator's email address
                    string taskAuthorEmailAddress = "";
                    try
                    {
                        User author = userManager.GetUserById(task.CreatorId);
                        taskAuthorEmailAddress = author.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TASK, task.TaskId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + task.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, task.ProjectId, task.TaskId))));
                    item.PublishDate = LocalizeDate(task.CreationDate);
                    item.LastUpdatedTime = LocalizeDate(task.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", task.CreatorName, taskAuthorEmailAddress), task.CreatorName, BASE_NAMESPACE + "/user/" + task.CreatorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (String.IsNullOrEmpty(task.Description))
                    {
                        description = Strings.StripHTML(task.Description, false, true);
                    }

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(task.TaskId, DataModel.Artifact.ArtifactTypeEnum.Task);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the release, priority and status in as categories
                    if (task.TaskPriorityId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(task.TaskPriorityName, BASE_NAMESPACE + "task/priority", Resources.Fields.TaskPriorityId));
                    }
                    if (!task.ReleaseId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(task.ReleaseVersionNumber, BASE_NAMESPACE + "task/release", Resources.Fields.VersionNumber));
                    }
                    item.Categories.Add(new SyndicationCategory(task.TaskStatusName, BASE_NAMESPACE + "task/status", Resources.Fields.TaskStatusId));
                    item.Categories.Add(new SyndicationCategory(task.TaskTypeName, BASE_NAMESPACE + "task/type", Resources.Fields.TaskTypeId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for attachments
        /// </summary>
        /// <param name="discussionManager">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="attachments">The list of attachments</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<ProjectAttachmentView> attachments, DiscussionManager discussionManager)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the tasks
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (ProjectAttachmentView attachment in attachments)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == attachment.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the attachment author's email address
                    string attachmentAuthorEmailAddress = "";
                    try
                    {
                        User author = userManager.GetUserById(attachment.AuthorId);
                        attachmentAuthorEmailAddress = author.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_DOCUMENT, attachment.AttachmentId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + attachment.Filename);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Documents, attachment.ProjectId, attachment.AttachmentId))));
                    item.PublishDate = LocalizeDate(attachment.UploadDate);
                    item.LastUpdatedTime = LocalizeDate(attachment.EditedDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", attachment.AuthorName, attachmentAuthorEmailAddress), attachment.AuthorName, BASE_NAMESPACE + "/user/" + attachment.AuthorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (String.IsNullOrEmpty(attachment.Description))
                    {
                        description = Strings.StripHTML(attachment.Description, false, true);
                    }

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussionManager.Retrieve(attachment.AttachmentId, DataModel.Artifact.ArtifactTypeEnum.Document);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the folder, type and status in as categories
                    item.Categories.Add(new SyndicationCategory(attachment.AttachmentTypeName, BASE_NAMESPACE + "document/attachment-type", Resources.Fields.Type));
                    item.Categories.Add(new SyndicationCategory(attachment.DocumentStatusName, BASE_NAMESPACE + "document/status", Resources.Fields.DocumentStatusId));
                    item.Categories.Add(new SyndicationCategory(attachment.DocumentTypeName, BASE_NAMESPACE + "document/type", Resources.Fields.DocumentType));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for automation hosts
        /// </summary>
        /// <param name="feed">The feed</param>
        /// <param name="automationHosts">The list of automation hosts</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<AutomationHostView> automationHosts)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the tasks
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (AutomationHostView automationHost in automationHosts)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == automationHost.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited)
                {
                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_AUTOMATION_HOST, automationHost.AutomationHostId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + automationHost.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AutomationHosts, automationHost.ProjectId, automationHost.AutomationHostId))));
                    item.PublishDate = LocalizeDate(automationHost.LastUpdateDate);
                    item.LastUpdatedTime = LocalizeDate(automationHost.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (String.IsNullOrEmpty(automationHost.Description))
                    {
                        description = Strings.StripHTML(automationHost.Description, false, true);
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the Token as a category
                    item.Categories.Add(new SyndicationCategory(automationHost.Token, BASE_NAMESPACE + "host/token", Resources.Fields.Token));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Populates the feed for test runs
        /// </summary>
        /// <param name="testRunManager">The testRun business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="testRuns">The data table</param>
        /// <param name="projectId">The id of the project - since test runs don't contain this</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<TestRunView> testRuns, TestRunManager testRunManager, int projectId)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Make sure the user is authorized to view this artifact for this project
            Business.ProjectManager projectManager = new Business.ProjectManager();
            ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == projectId && p.UserId == UserId);
            if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View)  != Project.AuthorizationState.Prohibited)
            {
                //Loop through the tasks
                UserManager userManager = new UserManager();
                foreach (TestRunView dataRow in testRuns)
                {
                    //We need to lookup the tester's email address
                    string testerEmailAddress = "";
                    try
                    {
                        User tester = userManager.GetUserById(dataRow.TesterId);
                        testerEmailAddress = tester.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_RUN, dataRow.TestRunId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + dataRow.Name + "-" + dataRow.ExecutionStatusName);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestRuns, projectId, dataRow.TestRunId))));
                    item.PublishDate = LocalizeDate(dataRow.StartDate);
                    if (dataRow.EndDate.HasValue)
                    {
                        item.LastUpdatedTime = LocalizeDate(dataRow.EndDate.Value);
                    }
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", dataRow.TesterName, testerEmailAddress), dataRow.TesterName, BASE_NAMESPACE + "/user/" + dataRow.TesterId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = Strings.StripHTML(dataRow.Description, false, true);

                    //See if we have an automated or manual test run
                    if (dataRow.TestRunTypeId == (int)TestRun.TestRunTypeEnum.Automated)
                    {
                        //Add the test run message to the test run item
                        if (!String.IsNullOrWhiteSpace(dataRow.RunnerMessage))
                        {
                            description += "<br/>\n";
                            description += dataRow.RunnerMessage;
                        }
                        
                        //Add the stack trace to the test run item
                        if (!String.IsNullOrWhiteSpace(dataRow.RunnerStackTrace))
                        {
                            description += "<br/>\n";
                            description += dataRow.RunnerStackTrace;
                        }
                    }
                    else
                    {
                        //Add the test run steps in as simple list of bullets
                        try
                        {

                            List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(dataRow.TestRunId);

                            if (testRunSteps.Count > 0)
                            {
                                description = description + "<p><u>" + Resources.ServerControls.TabControl_TestRunSteps + "</u></p>";
                                description = description + "<ol>";
                                foreach (TestRunStepView stepRow in testRunSteps)
                                {
                                    if (String.IsNullOrWhiteSpace(stepRow.ActualResult))
                                    {
                                        description = description + "<li>" + Strings.StripHTML(stepRow.Description, false, true) + " - " + Strings.StripHTML(stepRow.ExpectedResult, false, true) + " - Status: " + stepRow.ExecutionStatusName.ToUpperInvariant() + "</li>";
                                    }
                                    else
                                    {
                                        description = description + "<li>" + Strings.StripHTML(stepRow.Description, false, true) + " - " + Strings.StripHTML(stepRow.ExpectedResult, false, true) + " - Status: " + stepRow.ExecutionStatusName.ToUpperInvariant() + " (" + Strings.StripHTML(stepRow.ActualResult, false, true) + ")</li>";

                                    }
                                }
                                description = description + "</ol>";
                            }
                        }
                        catch (ArtifactNotExistsException)
                        {
                            //Ignore and carry on
                        }
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the test set, status and release in as categories
                    if (dataRow.ReleaseId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(dataRow.ReleaseVersionNumber, BASE_NAMESPACE + "testrun/release", Resources.Fields.VersionNumber));
                    }
                    if (dataRow.TestSetId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(dataRow.TestSetName, BASE_NAMESPACE + "testrun/testset", Resources.Fields.TestSetId));
                    }
                    item.Categories.Add(new SyndicationCategory(dataRow.ExecutionStatusName, BASE_NAMESPACE + "testrun/executionstatus", Resources.Fields.ExecutionStatusId));

                    //Add the item to the list
                    items.Add(item);
                }

                //Update the feed items with the list
                feed.Items = items;
            }
        }

        /// <summary>
        /// Populates the feed for subscribed artifacts
        /// </summary>
        /// <param name="feed">The feed</param>
        /// <param name="notifications">The list of notifications</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<NotificationUserSubscriptionView> notifications)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the artifacts
            Business.ProjectManager projectManager = new Business.ProjectManager();
            foreach (NotificationUserSubscriptionView notification in notifications)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == notification.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, (DataModel.Artifact.ArtifactTypeEnum)notification.ArtifactTypeId, Project.PermissionEnum.View)  != Project.AuthorizationState.Prohibited)
                {
                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.GetPrefixForArtifactType((DataModel.Artifact.ArtifactTypeEnum)notification.ArtifactTypeId), notification.ArtifactId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + notification.ArtifactName);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)notification.ArtifactTypeId, notification.ProjectId, notification.ArtifactId))));
                    item.PublishDate = LocalizeDate(notification.LastUpdateDate);
                    item.LastUpdatedTime = LocalizeDate(notification.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = "";
                    if (!String.IsNullOrEmpty(notification.ArtifactDescription))
                    {
                        description = Strings.StripHTML(notification.ArtifactDescription, false, true);
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the artifact type as a category
                    item.Categories.Add(new SyndicationCategory(notification.ArtifactTypeName, BASE_NAMESPACE + "artifact/type", Resources.Fields.ArtifactTypeId));

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Displays the list of assigned test cases
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter AssignedTestCases(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedTestCases";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of test cases belonging to the current user
                    TestCaseManager testCaseManager = new TestCaseManager();
                    DiscussionManager discussion = new DiscussionManager();
                    UserManager user = new UserManager();
                    List<TestCaseView> testCases = testCaseManager.RetrieveByOwnerId(userId, null, 250, false);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedTestCases + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "testcases/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of test cases
                    PopulateFeedItems(feed, testCases, discussion);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Displays the list of assigned test sets
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter AssignedTestSets(int userId, string rssToken)
        {
            const string METHOD_NAME = "AssignedTestSets";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of test sets belonging to the current user
                    TestSetManager testSetManager = new TestSetManager();
                    DiscussionManager discussion = new DiscussionManager();
                    UserManager user = new UserManager();
                    List<TestSetView> testSets = testSetManager.RetrieveByOwnerId(userId, null, 250, false);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyAssignedTestSets + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "testsets/assigned";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of test sets
                    PopulateFeedItems(feed, testSets, discussion);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Populates the feed for test sets
        /// </summary>
        /// <param name="discussion">The discussion business object</param>
        /// <param name="feed">The feed</param>
        /// <param name="testSets">The data table</param>
        protected void PopulateFeedItems(SyndicationFeed feed, List<TestSetView> testSets, DiscussionManager discussion)
        {
            //Create the list of items
            List<SyndicationItem> items = new List<SyndicationItem>();

            //Loop through the test cases
            Business.ProjectManager projectManager = new Business.ProjectManager();
            UserManager userManager = new UserManager();
            foreach (TestSetView testSet in testSets)
            {
                //Make sure the user is authorized to view this artifact for this project
                ProjectUser membershipRow = this.projectMembership.FirstOrDefault(p => p.ProjectId == testSet.ProjectId && p.UserId == UserId);
                if (membershipRow != null && projectManager.IsAuthorized(membershipRow.ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View)  != Project.AuthorizationState.Prohibited)
                {
                    //We need to lookup the testSet author's email address
                    string testSetAuthorEmailAddress = "";
                    try
                    { 
                        User author = userManager.GetUserById(testSet.CreatorId);
                        testSetAuthorEmailAddress = author.EmailAddress;
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore
                    }

                    SyndicationItem item = new SyndicationItem();
                    string artifactToken = GlobalFunctions.GetTokenForArtifact(GlobalFunctions.ARTIFACT_PREFIX_TEST_SET, testSet.TestSetId);
                    item.Title = new TextSyndicationContent(artifactToken + " - " + testSet.Name);
                    item.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.TestSets, testSet.ProjectId, testSet.TestSetId))));

                    //For test sets, we use the planned date as the publish date if it exists
                    if (testSet.PlannedDate.HasValue)
                    {
                        item.PublishDate = LocalizeDate(testSet.PlannedDate.Value);
                    }
                    else
                    {
                        item.PublishDate = LocalizeDate(testSet.CreationDate);
                    }
                    item.LastUpdatedTime = LocalizeDate(testSet.LastUpdateDate);
                    item.Id = artifactToken;
                    item.Authors.Add(new SyndicationPerson(String.Format("{0} ({1})", testSet.CreatorName, testSetAuthorEmailAddress), testSet.CreatorName, BASE_NAMESPACE + "/user/" + testSet.CreatorId));
                    item.Copyright = new TextSyndicationContent("(C) " + Resources.Main.Global_Copyright + " 2006-" + DateTime.Now.Year + " " + ConfigurationSettings.Default.License_Organization + ".");

                    //Get the main description in plain-text
                    string description = Strings.StripHTML(testSet.Description, false, true);

                    //Add the comments in as simple list of bullets
                    try
                    {
                        IEnumerable<IDiscussion> comments = discussion.Retrieve(testSet.TestSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);

                        if (comments.Count() > 0)
                        {
                            description = description + "<p><u>" + Resources.Main.Global_Discussion + "</u></p>";
                            description = description + "<ul>";
                            foreach (IDiscussion discussionRow in comments)
                            {
                                description = description + "<li>[" + discussionRow.CreatorName + " - " + discussionRow.CreationDate.ToShortDateString() + "] " + Strings.StripHTML(discussionRow.Text, false, true) + "</li>";
                            }
                            description = description + "</ul>";
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Ignore and carry on
                    }

                    //Need to convert the description into plain text for the RSS description
                    item.Summary = new TextSyndicationContent(description);

                    //Add the status and release as RSS categories
                    item.Categories.Add(new SyndicationCategory(testSet.TestSetStatusName, BASE_NAMESPACE + "testset/status", Resources.Fields.TestSetStatusId));
                    if (testSet.ReleaseId.HasValue)
                    {
                        item.Categories.Add(new SyndicationCategory(testSet.ReleaseVersionNumber, BASE_NAMESPACE + "testset/release", Resources.Fields.VersionNumber));
                    }

                    //Add the item to the list
                    items.Add(item);
                }
            }

            //Update the feed items with the list
            feed.Items = items;
        }

        /// <summary>
        /// Displays an RSS feed of a user's detected incidents
        /// </summary>
        /// <param name="rssToken">The user's RSS token</param>
        /// <param name="userId">The user's ID</param>
        /// <returns>The RSS feed</returns>
        public Rss20FeedFormatter DetectedIncidents(int userId, string rssToken)
        {
            const string METHOD_NAME = "DetectedIncidents";

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Initialize the feed
                Initialize(userId, rssToken);

                //Make sure we have a user id
                if (UserId > 0)
                {
                    //First get the list of incidents detected by the current user
                    IncidentManager incidentManager = new IncidentManager();
                    UserManager user = new UserManager();
                    List<IncidentView> incidents = incidentManager.RetrieveOpenByOpenerId(userId, null);

                    //Set the feed info
                    SyndicationFeed feed = new SyndicationFeed();
                    PopulateFeedInfo(feed);
                    feed.Title = new TextSyndicationContent(Resources.Main.ProjectList_MyDetectedIncidents + " (" + UserFullName + ")");
                    feed.Id = BASE_NAMESPACE + "incidents/detected";
                    feed.ElementExtensions.Add(new SyndicationElementExtension("link", "", ResolveAbsoluteUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage))));

                    //Populate the field with the list of incidents
                    PopulateFeedItems(feed, incidents, incidentManager);

                    //SpiraTest currently supports only RSS 2.0 (not ATOM)
                    Rss20FeedFormatter rssFormattedFeed = new Rss20FeedFormatter(feed);
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    return rssFormattedFeed;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }
    }
}
