using System;

namespace Inflectra.SpiraTest.Common
{
	public static class UrlRoots
	{
        public const string PROJECT_GROUP_PREFIX = "pg";
        public const string PROJECT_TEMPLATE_PREFIX = "pt";
        public const string PORTFOLIO_PREFIX = "pf";

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="aduditTrailPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveProjectAuditTrailUrl()
		{
			return "~/Administration/AuditTrail.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="adminAuditTrailPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveProjectAdminAuditTrailUrl()
		{
			return "~/Administration/AdminAuditTrail.aspx";
		}

		public static string RetrieveLandingScreenUrl()
		{
			return "~/LandingScreen.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="adminTrailPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveAuditTrailUrl()
		{
			return "~/Administration/AuditTrail.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="webservicePage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveWebServiceUrl()
		{
			return "~/Administration/Integration/WebServices.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="userActivityLogPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveUserActivityLogUrl()
		{
			return "~/Administration/UserActivityLog.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="userAduditTrailPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveUserAuditTrailUrl()
		{
			return "~/Administration/UserAuditTrail.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="aduditTrailSettingsPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveProjectAuditTrailSettingsUrl()
		{
			return "~/Administration/AuditTrailSettings.aspx";
		}

		/// <summary>
		/// Returns a rewriter format administration url
		/// </summary>
		/// <param name="projectId">The id of the project, -3 uses {0} for format strings, -2 is for using {art}</param>
		/// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
		/// <returns></returns>
		public static string RetrieveProjectAdminUrl(int projectId, string administrationPage)
        {
            if (projectId == -3)
            {
                return "~/{0}/Administration/" + administrationPage + ".aspx";
            }
            else if (projectId == -2)
            {
                return "~/{art}/Administration/" + administrationPage + ".aspx";
            }
            else
            {
                return string.Format("~/{0}/Administration/{1}.aspx", projectId, administrationPage);
            }
        }

        /// <summary>
        /// Returns a rewriter format administration url for portfolio admin pages
        /// </summary>
        /// <param name="portfolioId">The id of the portfolio (-3 = use format string)</param>
        /// <param name="administrationPage">The actual page name (e.g. "Default") without the .aspx part</param>
        /// <returns>The URL</returns>
        public static string RetrievePortfolioAdminUrl(int portfolioId, string administrationPage)
        {
            if (portfolioId == -3)
            {
                return "~/" + PORTFOLIO_PREFIX + "/{0}/Administration/" + administrationPage + ".aspx";
            }
            else
            {
                return string.Format("~/" + PORTFOLIO_PREFIX + "/{0}/Administration/{1}.aspx", portfolioId, administrationPage);
            }
        }

        /// <summary>
        /// Returns a rewriter format administration url
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template (-3 = use format string)</param>
        /// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
        /// <returns>The URL</returns>
        public static string RetrieveTemplateAdminUrl(int projectTemplateId, string administrationPage)
        {
            if (projectTemplateId == -3)
            {
                return "~/" + PROJECT_TEMPLATE_PREFIX + "/{0}/Administration/" + administrationPage + ".aspx";
            }
            else
            {
                return string.Format("~/" + PROJECT_TEMPLATE_PREFIX + "/{0}/Administration/{1}.aspx", projectTemplateId, administrationPage);
            }
        }

        /// <summary>
        /// Returns a rewriter format administration url
        /// </summary>
        /// <param name="projectGroupId">The id of the project group (-3 = use format string)</param>
        /// <param name="administrationPage">The actual page name ("ProjectAssociations") without the .aspx part</param>
        /// <returns></returns>
        public static string RetrieveGroupAdminUrl(int projectGroupId, string administrationPage)
        {
            if (projectGroupId == -3)
            {
                return "~/" + PROJECT_GROUP_PREFIX + "/{0}/Administration/" + administrationPage + ".aspx";
            }
            else
            {
                return string.Format("~/" + PROJECT_GROUP_PREFIX + "/{0}/Administration/{1}.aspx", projectGroupId, administrationPage);
            }
        }

        /// <summary>Requests a 'pretty' URL to use.</summary>
        /// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
        /// <param name="ProjectID">The project ID of the artifact. Ignored if not needed. Specifying -1 will not include ProjectPath, specifying -2 will insert the token {proj} for the ProjectID, and specifying -3 will insert the token {0} for use in format strings.</param>
        /// <param name="ArtifactID">
        /// The ID of the artifact. Ignored if not needed, specifying -2 will insert the token {art} for the ArtifactID, and specifying -3 will insert the token {0} for use in format strings, -4 will specify the Table view and -5 will specify the Board view,
        /// -9 will specify the GANTT view
        /// </param>
        /// <param name="TabName">The name of the tab or extra item in the URL. Null if not specified.</param>
        /// <returns>String of the new URL.</returns>
        public static string RetrieveURL(NavigationLinkEnum navigationLink, int ProjectID, int ArtifactID, string TabName, bool deReference = false, int? userId = null)
		{
			string hostBase = ((deReference) ? ConfigurationSettings.Default.General_WebServerUrl : "~/");
			if (!hostBase.EndsWith("/")) hostBase += "/";
			string retUrl = "";
			string strArtName = "";
			string strProject = ((ProjectID == -2) ? "{proj}" : ProjectID.ToString());
			string strArtifact = ((ArtifactID == -2) ? "{art}" : ArtifactID.ToString());
			if (ProjectID == -3)
			{
				strProject = "{0}";    //Used in format strings
			}
            if (ProjectID == -4)
            {
                strProject = "{projId}";    //Used in the sitemap
            }
			if (ArtifactID == -3)
			{
				strArtifact = "{0}";    //Used in format strings
			}

			switch (navigationLink)
			{
				#region Static Pages
				//Login
				case NavigationLinkEnum.Login:
					return "~/Login.aspx";

				//Error Pages
				case NavigationLinkEnum.ErrorPage:
					return "~/ErrorPage.aspx";

				case NavigationLinkEnum.InvalidDatabase:
					return "~/InvalidDatabase.aspx";

				case NavigationLinkEnum.InvalidLicense:
					return "~/InvalidLicense.aspx";

				//My Page
				case NavigationLinkEnum.MyPage:
                    if (ProjectID > 0)
						return "~/" + ProjectID.ToString() + "/MyPage.aspx";
                    else if (ProjectID == -4)
                        return "~/" + strProject + "/MyPage.aspx";
					else
						return "~/MyPage.aspx";

                //MyProfile
                case NavigationLinkEnum.MyProfile:
                    if (ProjectID > 0)
                        return "~/" + ProjectID.ToString() + "/MyProfile.aspx";
                    else if (ProjectID == -4)
                        return "~/" + strProject + "/MyProfile.aspx";
                    else
                        return "~/MyProfile.aspx";

                //My Timecard
                case NavigationLinkEnum.MyTimecard:
                    if (ProjectID > 0)
                        return "~/" + ProjectID.ToString() + "/MyTimecard.aspx";
                    else if (ProjectID == -4)
                        return "~/" + strProject + "/MyTimecard.aspx";
                    else
                        return "~/MyTimecard.aspx";

                //Administration
                case NavigationLinkEnum.Administration:
                    if (ProjectID > 0)
                    {
                        return "~/" + ProjectID.ToString() + "/Administration.aspx";
                    }
                    else if (ProjectID == -3)
                    {
                        return "~/{0}/Administration.aspx";
                    }
                    else
                    {
                        return "~/Administration.aspx";
                    }

				#endregion

				// Project Pages.
                case NavigationLinkEnum.ProjectGroupHome:
                    {
                        if (ArtifactID > 0)
                        {
                            return hostBase + strProject + "/GroupHome/" + ArtifactID + ".aspx";
                        }
                        else
                        {
                            return hostBase + strProject + "/GroupHome.aspx";
                        }
                    }

                case NavigationLinkEnum.ProjectHome:
                    {
                        if (string.IsNullOrEmpty(TabName))
                        {
                            return hostBase + strProject + ".aspx";
                        }
                        else
                        {
                            return hostBase + strProject + "/" + TabName + ".aspx";
                        }
                    }

				//Documents
				case NavigationLinkEnum.Documents:
					strArtName = "Document";
					break;

				case NavigationLinkEnum.Attachment:
					strArtName = "Attachment";
					break;

                case NavigationLinkEnum.AttachmentVersion:
                    strArtName = "AttachmentVersion";
                    break;

                //Source Code
                case NavigationLinkEnum.SourceCode:
                    strArtName = "SourceCode";
                    break;

                case NavigationLinkEnum.SourceCodeRevision:
                    strArtName = "SourceCodeRevision";
                    break;

				//Requirements
				case NavigationLinkEnum.Requirements:
					strArtName = "Requirement";
					break;

				//Test Cases
				case NavigationLinkEnum.TestCases:
					strArtName = "TestCase";
					break;

				//Test Runs
				case NavigationLinkEnum.TestRuns:
					strArtName = "TestRun";
					break;

				//Test Runs when you want the latest test run for a given test case
				case NavigationLinkEnum.TestCaseRuns:
					strArtName = "TestCaseRun";
					break;

                //Test Runs when you want the latest test run for a given test case in a test set
                case NavigationLinkEnum.TestSetTestCaseRuns:
                    strArtName = "TestSetTestCaseRun";
                    break;

				//Test Runs when you want the latest test run for a given test case and test step
				case NavigationLinkEnum.TestStepRuns:
					strArtName = "TestStepRun";
					break;

				//Test Steps
				case NavigationLinkEnum.TestSteps:
					strArtName = "TestStep";
					break;

				//Test Sets
				case NavigationLinkEnum.TestSets:
					strArtName = "TestSet";
					break;

				//Automation Hosts
				case NavigationLinkEnum.AutomationHosts:
					strArtName = "AutomationHost";
					break;

                //Automation Hosts
                case NavigationLinkEnum.TestConfigurations:
                    strArtName = "TestConfiguration";
                    break;

				//Incidents
				case NavigationLinkEnum.Incidents:
					strArtName = "Incident";
					break;

                //Risks
                case NavigationLinkEnum.Risks:
                    strArtName = "Risk";
                    break;

                //Releases
                case NavigationLinkEnum.Releases:
					strArtName = "Release";
					break;

                //Iterations
                case NavigationLinkEnum.Iterations:
                    return hostBase + strProject + "/IterationPlan.aspx";

				//Tasks
				case NavigationLinkEnum.Tasks:
					strArtName = "Task";
					break;

				//Reports
				case NavigationLinkEnum.Reports:
					strArtName = "Report";
					break;
                case NavigationLinkEnum.Graphs:
                    strArtName = "Graph";
                    break;

				//Test Execution
				case NavigationLinkEnum.TestExecute:
					strArtName = "TestExecute";
					break;

                //Test Execution - Exploratory
                case NavigationLinkEnum.TestExecuteExploratory:
                    strArtName = "TestExecuteExploratory";
                    break;

                //Automated Test Launching
                case NavigationLinkEnum.TestLaunch:
					strArtName = "TestLaunch";
					break;

				//Resources/Users
				case NavigationLinkEnum.Resources:
					strArtName = "Resource";
					break;

                //Resources
                case NavigationLinkEnum.PlanningBoard:
					return hostBase + strProject + "/PlanningBoard.aspx";

                //Saved Searches
                case NavigationLinkEnum.SavedSearch:
                    strArtName = "SavedSearch";
                    break;

                //Builds
                case NavigationLinkEnum.Builds:
                    strArtName = "Build";
                    break;

                //Test Set Test Cases
                case NavigationLinkEnum.TestSetTestCases:
                    strArtName = "TestSetTestCase";
                    break;

                //Activity Stream
                case NavigationLinkEnum.ActivityStream:
                    strArtName = "Activity";
                    break;

                case NavigationLinkEnum.ScreenshotUpload:
                    strArtName = "ScreenshotUpload";
                    break;

				case NavigationLinkEnum.PullRequest:
					strArtName = "PullRequest";
					break;

				default:
					return "";
			}

			//Now add the artifact ID and extra tabName, if set.
			retUrl = hostBase;
			if (ProjectID != -1) retUrl += strProject + "/";
			retUrl += strArtName + "/";

			if (userId == null)
			{
				switch (ArtifactID)
				{
					case -2:
						retUrl += "{art}";
						break;
					case -1:
						retUrl += "New";
						break;
					case 0:
						retUrl += "List";
						break;
					case -4:
						retUrl += "Table";
						break;
					case -5:
						retUrl += "Board";
						break;
					case -6:
						retUrl += "Tree";
						break;
					case -7:
						retUrl += "Document";
						break;
					case -8:
						retUrl += "Map";
						break;
					case -9:
						retUrl += "Gantt";
						break;
					default:
						retUrl += strArtifact;
						break;
				}
			}
			else
			{
				retUrl += "Tree";
			}
			if (string.IsNullOrWhiteSpace(TabName))
				retUrl += ".aspx";
			else
				retUrl += "/" + TabName + ".aspx";

			return retUrl;
		}

        /// <summary>Requests a 'pretty' URL to use for project group pages.</summary>
        /// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
        /// <param name="tabName">The name of any page suffix after the main URL</param>
        /// <param name="projectGroupId">The project ID of the artifact. Ignored if not needed. Specifying -3 will insert the token {0} for use in format strings. Specifying -12 will insert the token {projGroupId} for directing to project group home</param>
        /// <returns>String of the new URL.</returns>
        public static string RetrieveGroupURL(NavigationLinkEnum navigationLink, int projectGroupId, bool deReference = false, string tabName = "")
        {
            string hostBase = ((deReference) ? ConfigurationSettings.Default.General_WebServerUrl : "~/");
            if (!hostBase.EndsWith("/")) hostBase += "/";
            string retUrl = "";
            string projectGroup = ((projectGroupId == -2) ? "{proj}" : projectGroupId.ToString());
            if (projectGroupId == -3)
            {
                projectGroup = "{0}";    //Used in format strings
            } 
            if (projectGroupId == -12)
            {
                projectGroup = "{projGroupId}";    //Used in the sitemap
            }

            //See if we have a tab name to include
            string urlSuffix = "";
            if (!String.IsNullOrEmpty(tabName))
            {
                urlSuffix = "/" + tabName;
            }

            switch (navigationLink)
            {
                // Program HomePage.
                case NavigationLinkEnum.ProjectGroupHome:
                    retUrl = hostBase + "pg/" + projectGroup + urlSuffix + ".aspx";
                    break;

                // Program Planning Board.
                case NavigationLinkEnum.GroupPlanningBoard:
                    retUrl = hostBase + "pg/" + projectGroup + "/PlanningBoard" + urlSuffix + ".aspx";
                    break;

                // Program Releases.
                case NavigationLinkEnum.GroupReleases:
                    retUrl = hostBase + "pg/" + projectGroup + "/Releases" + urlSuffix + ".aspx";
                    break;

                // Program Incidents.
                case NavigationLinkEnum.GroupIncidents:
                    retUrl = hostBase + "pg/" + projectGroup + "/Incidents" + urlSuffix + ".aspx";
                    break;

                default:
					return "";
            }

            return retUrl;
        }

        /// <summary>Requests a 'pretty' URL to use for portfolio pages.</summary>
        /// <param name="navigationLink">Enumeration of RewriterArtifactType</param>
        /// <param name="portfolioId">The portfolio ID of the artifact. Ignored if not needed. Specifying -3 will insert the token {0} for use in format strings. Specifying -401 will insert the token {projGroupId} for directing to project group home</param>
        /// <returns>String of the new URL.</returns>
        public static string RetrievePortfolioURL(NavigationLinkEnum navigationLink, int portfolioId, bool deReference = false)
        {
            string hostBase = ((deReference) ? ConfigurationSettings.Default.General_WebServerUrl : "~/");
            if (!hostBase.EndsWith("/")) hostBase += "/";
            string retUrl = "";
            string programUrlPart = ((portfolioId == -2) ? "{proj}" : portfolioId.ToString());
            if (portfolioId == -3)
            {
                programUrlPart = "{0}";    //Used in format strings
            }
            if (portfolioId == -401)
            {
                programUrlPart = "{portfolioId}";    //Used in the sitemap
            }

            switch (navigationLink)
            {
                // Portfolio HomePage.
                case NavigationLinkEnum.PortfolioHome:
                    retUrl = hostBase + "pf/" + programUrlPart + ".aspx";
                    break;

                default:
                    return "";
            }

            return retUrl;
        }

        public enum NavigationLinkEnum : int
		{
			// Needs to match DataAccess.ArtifactType values. Value in SiteMap file must match enums listed here.
			None = -1,
            Requirements = 1,
            TestCases = 2,
            Incidents = 3,
            Releases = 4,
            TestRuns = 5,
            Tasks = 6,
            TestSteps = 7,
            TestSets = 8,
            AutomationHosts = 9,
            Documents = 13,
            Risks = 14,
            Login = -2,
			MyPage = -3,
			ProjectHome = -4,
			Reports = -5,
			Administration = -6,
			ErrorPage = -7,
			MyProfile = -8,
			Iterations = -9,
			Resources = -11,
			ProjectGroupHome = -12,
			SourceCode = -13,
			Attachment = -14,
			TestStepRuns = -15,
			TestExecute = -16,
			TestLaunch = -17,
			TestCaseRuns = -18,
			InvalidDatabase = -20,
			InvalidLicense = -21,
            Graphs = -22,
            PlanningBoard = -23,
            MyTimecard = -24,
            AttachmentVersion = -25,
            SavedSearch = -26,
            Builds = -27,
            SourceCodeRevision = -28,
            ActivityStream = -29,
			PullRequest = -38,
			ScreenshotUpload = -30,
            TestSetTestCaseRuns = -31,
            TestExecuteExploratory = -32,
            TestConfigurations = -33,
            GroupReleases = -34,
            GroupIncidents = -35,
            GroupPlanningBoard = -36,
            TestSetTestCases = -37,
            PortfolioHome = -401, /* neg as not artifact proper, 4 for the enum of the workspace type, 01 as homepage is the first page we made for portfolios */
            EnterpriseHome = -501 /* neg as not artifact proper, 5 for the enum of the workspace type, 01 as home is the first page we made for enterprise */
        }
	}
}
