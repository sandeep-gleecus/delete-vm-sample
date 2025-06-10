using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.App_Models;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.UserControls
{

    /// <summary>
    ///		This user control displays the global navigation bar and handles the context-sensitive
    ///		display of information and responding to events
    /// </summary>
    public partial class GlobalNavigation : UserControlBase
    {
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.GlobalNavigation::";

        private NavigationHighlightedLink highlightedLink;
        private string breadcrumbText = "";
        private string helpJumpTag = "";
		private string navigationJSON = "";
		private string guidedToursJSON = "";
		private string versionNumber = "";

		private static Regex prjUrl = new Regex(@"^" + Regex.Escape(HttpContext.Current.Request.ApplicationPath) + @"/(?<id>\d+)[\./].*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        private static Regex prjToken = new Regex(@"{projid}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		#region Enumerations

		public enum NavigationHighlightedLink
        {
            // Needs to match DataModel.Artifact.ArtifactTypeEnum values. Value in SiteMap file must match enums listed here.
            None = -1,
            Login = -2,
            MyPage = -3,
            ProjectHome = -4,
            Requirements = 1,
            TestCases = 2,
            Incidents = 3,
            Releases = 4,
            Reports = -5,
            TestSets = 8,
            Administration = -6,
            ErrorPage = -7,
            MyProfile = -8,
            Tasks = 6,
            Iterations = -9,
            Documents = 13,
			Risks = 14,
			Resources = -11,
            ProjectGroupHome = -12,
            SourceCode = -13,
            TestRuns = 5,
            TestSteps = 7,
            Attachment = -14,
            AutomationHosts = 9,
            PlanningBoard = -23,
            MyTimecard = -24,
			TestConfigurations = -33,
			GroupReleases = -34,
			GroupIncidents = -35,
            GroupPlanningBoard = -36,
            Builds = -27,
            SourceCodeRevisions = -28,
            PullRequests = -38,
            TestSetTestCases = -37,
            PortfolioHome = -401, /* neg as not artifact proper, 4 for the enum of the workspace type, 01 as homepage is the first page we made for portfolios */
            EnterpriseHome = -501 /* neg as not artifact proper, 5 for the enum of the workspace type, 01 as home is the first page we made for enterprise */
        }

		#endregion

		#region Properties

		/// <summary>
		/// Determines which link in the navigation to highlight
		/// </summary>
		public NavigationHighlightedLink HighlightedLink
        {
            get
            {
                return highlightedLink;
            }
            set
            {
                this.highlightedLink = value;
            }
        }

        /// <summary>
        /// Any additional text that we want to display in the breadcrumbing
        /// </summary>
        public string BreadcrumbText
        {
            get
            {
                return this.breadcrumbText;
            }
            set
            {
                this.breadcrumbText = value;
            }
        }

        /// <summary>
        /// The optional jump-tag which allows you to jump to a specific point in the help documentation
        /// </summary>
        public string HelpJumpTag
        {
            get
            {
                return this.helpJumpTag;
            }
            set
            {
                this.helpJumpTag = value;
            }
        }

		/// <summary>
		/// JSON string for navigation to be used on the client to display nav bar
		/// </summary>
		public string NavigationJSON
		{
			get
			{
				return this.navigationJSON;
			}
			set
			{
				this.navigationJSON = value;
			}
		}

		/// <summary>
		/// JSON string to tell client which onboarding tours the user has already seen
		/// </summary>
		public string GuidedToursJSON
		{
			get
			{
				return this.guidedToursJSON;
			}
			set
			{
				this.guidedToursJSON = value;
			}
		}

		/// <summary>
		/// string of the product version number
		/// </summary>
		public string VersionNumber
		{
			get
			{
				return this.versionNumber;
			}
			set
			{
				this.versionNumber = value;
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Adds the event handlers and gets the navigation attributes before any databinding occurs
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnInit(EventArgs e)
        {
			const string METHOD_NAME = "OnInit";

			base.OnInit(e);

			//First make sure we're authenticated
			if (Page.User.Identity.IsAuthenticated && UserId > 0)
			{
				try
				{
					//WRITING OUT THE NAVIGATION OBJECT TO JSON FOR CLIENT SIDE RENDERING
					JSON_GlobalNavigation globalNavigation = new JSON_GlobalNavigation();

					//Set up the role profiles here for use later in the method
                    PortfolioManager portfolioManager = new PortfolioManager();
                    ProjectGroupManager projectGroupManager = new ProjectGroupManager();
					ProjectManager projectManager = new ProjectManager();
					TemplateManager templateManager = new TemplateManager();

					//See if the page we're bound to has the NavigationLink attribute set
					HeaderSettingsAttribute headerSettingsAttribute = (HeaderSettingsAttribute)System.Attribute.GetCustomAttribute(this.Page.GetType(), typeof(HeaderSettingsAttribute));

					//Set the links that are always available
					globalNavigation.MyPageUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId);
					globalNavigation.MyProfileUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyProfile, ProjectId);

					//Get the selected navigation link from the custom page attribute
					//Default to no tabs highlighted if no attribute found on the page
					this.highlightedLink = headerSettingsAttribute != null ? headerSettingsAttribute.HighlightedLink : NavigationHighlightedLink.None;

					//Default to no help and no current page selection
					//Set the current location and help
					globalNavigation.CurrentLocation = headerSettingsAttribute != null ? headerSettingsAttribute.HighlightedLink : NavigationHighlightedLink.None;

					//Set the help url - for admin if they are in admin, otherwise, for the user guide
					string helpUrlEndTag = headerSettingsAttribute != null ? headerSettingsAttribute.HelpJumpTag : "";
					string helpUrlSection = headerSettingsAttribute.HighlightedLink == NavigationHighlightedLink.Administration ? "[OnShore to replace]" : "[OnShore to replace]";
					//Formulate the full help url
					globalNavigation.HelpUrl = "[OnShore to replace]" + helpUrlSection + "/";
					if (helpUrlEndTag != null && helpUrlEndTag != "")
					{
						globalNavigation.HelpUrl += helpUrlEndTag;
						globalNavigation.HelpSection = helpUrlEndTag;
					}

					//Set timecard if sufficient permissions based on the version of Spira installed
					if (ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.MyTimecard))
					{
						globalNavigation.MyTimecardUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyTimecard, ProjectId);
					}

					//Set admin if sufficient permissions
					bool hasAnyAdminAccess = UserIsAdmin || SpiraContext.Current.IsProjectAdmin || SpiraContext.Current.IsTemplateAdmin || SpiraContext.Current.IsGroupAdmin || UserIsReportAdmin;
					if (hasAnyAdminAccess)
					{
						globalNavigation.AdminUrl = UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Administration));
					}

					//Set user information
					globalNavigation.User = new JSON_UserNav();
					globalNavigation.User.ProfileUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyProfile, ProjectId);
					ProfileEx profile = new ProfileEx();
					if (profile != null)
					{
						globalNavigation.User.FullName = profile.FullName;
						globalNavigation.User.FirstName = profile.FirstName;
						globalNavigation.User.LastName = profile.LastName;
						globalNavigation.User.HasIcon = !string.IsNullOrEmpty(profile.AvatarImage);
						globalNavigation.User.AvatarUrl = UrlRewriterModule.ResolveUserAvatarUrl(UserId, Page.Theme);
					}

					//Set the list of programs the user has any access to 
					List<ProjectGroupUserView> programList = projectGroupManager.RetrieveForUser(UserId);

					// We also create a simple list to store the program IDs in - to check if the program a project is part of is not listed here
					// Ie to the handle the case where a user has access to a project but not the program
					List<int> programIdList = new List<int>();
					globalNavigation.Programs = new List<JSON_Program>();

                    if (programList.Count != 0)
                    {
                        globalNavigation.ProgramIdsByOwner = new List<int>();

                    foreach (ProjectGroupUserView program in programList)
                    {
                        JSON_Program json_program = new JSON_Program()
                        {
                            Id = program.ProjectGroupId,
                            Name = program.ProjectGroupName,
                            WorkspaceUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, program.ProjectGroupId),
                            ArtifactUrl = SetProgramArtifactUrl(program.ProjectGroupId, highlightedLink),
                            IsEnabled = true,
                            PortfolioId = program.PortfolioId
                        };
                        //Add the id to a list of program IDs - internal method use only 
                        programIdList.Add(program.ProjectGroupId);
                        //Add the field to the object.
                        globalNavigation.Programs.Add(json_program);

                            //If the user is an owner, add to the list of owner program ids (this controls whether the user can see admin information about this program)
                            if (program.ProjectGroupRoleId == (int)ProjectGroup.ProjectGroupRoleEnum.GroupOwner)
                            {
                                globalNavigation.ProgramIdsByOwner.Add(program.ProjectGroupId);
                            }
                        }
					}


					//Set the list of project ids that the user is an owner of
					List<ProjectUserView> projectListByOwner = projectManager.RetrieveProjectsByOwner(UserId);
					if (projectListByOwner.Count != 0)
					{
						globalNavigation.ProjectIdsByOwner = new List<int>();
						foreach (ProjectUserView project in projectListByOwner)
						{
							globalNavigation.ProjectIdsByOwner.Add(project.ProjectId);
						}
					}

					//Set the list of projects the user has any access to
					List<ProjectForUserView> projectList = projectManager.RetrieveForUser(UserId);
					globalNavigation.Projects = new List<JSON_Project>();

                    if (projectList.Count != 0)
                    {
                        List<ProjectGroup> programsActive = projectGroupManager.RetrieveActive();

                    foreach (ProjectForUserView project in projectList)
                    {
                        JSON_Project json_project = new JSON_Project()
                        {
                            Id = project.ProjectId,
                            Name = project.Name,
                            //Description = project.Description, NOTE: commented out on 2020-06-19 to reduce final JSON size of SpiraContext.Navigation by 15%
                            WorkspaceUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, project.ProjectId),
                            ArtifactUrl = SetProjectArtifactUrl(project.ProjectId, highlightedLink),
                            IsEnabled = true
                        };
                            //Add the object to the array
                            globalNavigation.Projects.Add(json_project);

                        // handle cases where a project group id has been set on the project (should be always)
                        if (project.ProjectGroupId > 0)
                        {
                            json_project.ProgramId = project.ProjectGroupId;
                            // if the program this project belongs to is not already in the program list, we need to add for display purposes
                            // but the user won't have access to this program so make sure it's disabled
                            if (!programIdList.Contains(project.ProjectGroupId))
                            {
                                //Retrieve the program to check it is active - if not we do not add it so that it and its products are hidden in the nav dropdown
                                ProjectGroup program = programsActive.FirstOrDefault(p => p.ProjectGroupId == project.ProjectGroupId);
                                if (program != null)
                                {
                                // add the program to our list of program ids so we don't add duplicates
                                programIdList.Add(project.ProjectGroupId);
                                // now add data about the programe to the global nav program list
                                JSON_Program json_program_fromProject = new JSON_Program()
                                {
                                    Id = program.ProjectGroupId,
                                    Name = program.Name,
                                    WorkspaceUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, program.ProjectGroupId),
                                    ArtifactUrl = SetProgramArtifactUrl(program.ProjectGroupId, highlightedLink),
                                    IsEnabled = false,
                                    //the project has the portfolio id set and this id will be the same as for the program
                                    PortfolioId = program.PortfolioId
                                };
                                globalNavigation.Programs.Add(json_program_fromProject);
                                    }
                                }
							}
						}
                    }


                    //Get the list of active portfolios
                    if (Common.Global.Feature_Portfolios)
                    {
                        List<Portfolio> portfolios = portfolioManager.Portfolio_Retrieve();
                        if (portfolios.Count > 0)
                        {
                            globalNavigation.Portfolios = new List<JSON_Portfolio>();
                            foreach (Portfolio portfolio in portfolios)
                            {
                                JSON_Portfolio json_portfolio = new JSON_Portfolio();
                                int[] childPrograms = new int[] { };

                                //See if we have any programs under this portfolio
                                if (globalNavigation.Programs != null && globalNavigation.Programs.Count > 0)
                                {
                                    childPrograms = globalNavigation.Programs.Where(p => p.PortfolioId == portfolio.PortfolioId).Select(p => p.Id).ToArray();
                                }

                                //Add the portfolio if the user is a portfolio admin or they have access to programs inside the portfolio
                                if (profile.IsPortfolioAdmin || childPrograms.Length > 0)
                                {
                                    json_portfolio.Id = portfolio.PortfolioId;
                                    json_portfolio.Name = portfolio.Name;
                                    //json_portfolio.Description = portfolio.Description; NOTE: commented out on 2020-06-19 to reduce final JSON size of SpiraContext.Navigation by 15%
                                    json_portfolio.WorkspaceUrl = UrlRewriterModule.RetrievePortfolioRewriterURL(UrlRoots.NavigationLinkEnum.PortfolioHome, portfolio.PortfolioId);
                                    json_portfolio.ArtifactUrl = SetPortfolioArtifactUrl(portfolio.PortfolioId, highlightedLink);
                                    json_portfolio.IsEnabled = true;
                                
                                    //Add any programs under this portfolio
                                    if (childPrograms.Length > 0)
                                    {
                                        json_portfolio.ProgramIds = childPrograms;
                                    }

                                    //Add the field to the object.
                                    globalNavigation.Portfolios.Add(json_portfolio);
                                }
                            }
                        }
					}

					//Set the list of templates that the user is an owner of
					List<ProjectTemplate> templatesByOwner = templateManager.RetrieveTemplatesByAdmin(UserId);
					if (templatesByOwner.Count != 0)
					{
						globalNavigation.Templates = new List<JSON_Template>();
						foreach (ProjectTemplate template in templatesByOwner)
						{
							// only show active templates
							if (template.IsActive)
							{
								// add core template information
								JSON_Template json_template = new JSON_Template()
								{
									Id = template.ProjectTemplateId,
									Name = template.Name,
									//Description = template.Description NOTE: commented out on 2020 - 06 - 19 to reduce final JSON size of SpiraContext.Navigation by 15 %
                                };

								// add information about the projects tied to this template
								List<Project> projects = projectManager.RetrieveForTemplate(template.ProjectTemplateId);
								if (projects.Count != 0)
								{
									json_template.Projects = new List<JSON_Project>();
									foreach (Project project in projects)
									{
										JSON_Project json_project_of_template = new JSON_Project()
										{
											Id = project.ProjectId,
											Name = project.Name,
											WorkspaceUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, project.ProjectId)
										};
										json_template.Projects.Add(json_project_of_template);
									}
								}

								globalNavigation.Templates.Add(json_template);
							}

						}
					}

					//Create nested list of allowed links - effectively a simplified sitemap for JSON use
					globalNavigation.NodeTree = new JSON_SiteMapNode();
					globalNavigation.NodeTree = GlobalFunctions.GetFullJsonSiteMapNode(SiteMap.RootNode);



					//Create the admin nav structure
					//First check if the user has any admin access. For projects/programs/templates only render if the current page is for that type
					if (hasAnyAdminAccess)
					{
						JSON_AdminNavigation adminNavigation = new JSON_AdminNavigation();

						// create system admin items if user is a system admin
						if (UserIsAdmin || UserIsReportAdmin)
						{
							adminNavigation.System = new JSON_AdminSection();
							adminNavigation.System = CreateAdminNavSystem();
						}

						// create product admin items if user is product admin (of current product) and is either on a product or template workspace
						// don't think you can be a product admin and in a template other than the one for that specific product
						if (SpiraContext.Current.IsProjectAdmin &&
							    (
                                    SpiraContext.Current.WorkspaceType == (int)Workspace.WorkspaceTypeEnum.Product ||
								    SpiraContext.Current.WorkspaceType == (int)Workspace.WorkspaceTypeEnum.ProjectTemplate
							    )
						    )
						{
							adminNavigation.Project = new JSON_AdminSection();
							adminNavigation.Project = CreateAdminNavProject();
						}

						// create program items if program admin/owner and on a program workspace
						if (SpiraContext.Current.IsGroupAdmin && SpiraContext.Current.WorkspaceType == (int)Workspace.WorkspaceTypeEnum.Program)
						{
							adminNavigation.Program = new JSON_AdminSection();
							adminNavigation.Program = CreateAdminNavProgram();
						}

						// create template admin items if template admin and either on a product or template workspace
						if (SpiraContext.Current.IsTemplateAdmin &&
							(
								SpiraContext.Current.WorkspaceType == (int)Workspace.WorkspaceTypeEnum.ProjectTemplate ||
                                SpiraContext.Current.WorkspaceType == (int)Workspace.WorkspaceTypeEnum.Product
							)
							)
						{
							adminNavigation.Template = new JSON_AdminSection();
							adminNavigation.Template = CreateAdminNavTemplate();
						}

						// now write it out to the main global nav object
						globalNavigation.AdminNavigation = adminNavigation;
					}

					//Set and write out the JSON to the page
					JsonSerializerSettings settings = new JsonSerializerSettings()
					{
						Formatting = Formatting.None,
						NullValueHandling = NullValueHandling.Ignore,
						ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
						DefaultValueHandling = DefaultValueHandling.Ignore,
                        StringEscapeHandling = StringEscapeHandling.EscapeHtml  //Fixes XSS vulnerability
                    };
					this.navigationJSON = JsonConvert.SerializeObject(globalNavigation, Formatting.None, settings);

					// Write out the guidedTour information to the page
					GlobalService globalService = new GlobalService();
					UserSettingsCollection guidedTourCollection = GuidedTour_RetrieveAll();
					this.guidedToursJSON = JsonConvert.SerializeObject(guidedTourCollection, Formatting.None, settings);

					// Write out any extra meta data required
					this.versionNumber = GlobalFunctions.DISPLAY_SOFTWARE_VERSION;












					//Get the selected navigation link from the custom page attribute

					//Default to no tabs highlighted if no attribute found on the page
					this.highlightedLink = NavigationHighlightedLink.None;

					//See if the page we're bound to has the NavigationLink attribute set
					//HeaderSettingsAttribute headerSettingsAttribute = (HeaderSettingsAttribute)System.Attribute.GetCustomAttribute(this.Page.GetType(), typeof(HeaderSettingsAttribute));

					if (headerSettingsAttribute != null)
					{
						//Get the various header attributes
						this.highlightedLink = headerSettingsAttribute.HighlightedLink;
						this.helpJumpTag = headerSettingsAttribute.HelpJumpTag;

						//See if we can localize the breadcrumb text or not
						if (String.IsNullOrEmpty(Resources.Main.ResourceManager.GetString(headerSettingsAttribute.BreadcrumbText)))
						{
							this.breadcrumbText = headerSettingsAttribute.BreadcrumbText;
						}
						else
						{
							this.breadcrumbText = Resources.Main.ResourceManager.GetString(headerSettingsAttribute.BreadcrumbText);
						}

						if (String.IsNullOrEmpty(this.breadcrumbText))
						{
							//Add the appropriate breadcrumbing
							switch (highlightedLink)
							{
								case NavigationHighlightedLink.MyPage:
								case NavigationHighlightedLink.MyProfile:
									breadcrumbText = Encoder.HtmlEncode(new ProfileEx().FullName);
									break;

								case NavigationHighlightedLink.Administration:
									if (!String.IsNullOrEmpty(ProjectName))
									{
										breadcrumbText = ProjectName;
									}
									break;

								case NavigationHighlightedLink.ProjectHome:
									breadcrumbText = ProjectName;
									break;
							}
						}
					}

					//Register the various event handlers
					this.rptNavigationLinks.ItemDataBound += new RepeaterItemEventHandler(rptNavigationLinks_ItemDataBound);
					this.srcSecondaryNavigation.Load += new EventHandler(srcSecondaryNavigation_Load);
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				}
			}
        }

		/// <summary>
		/// Returns a string of the url to use for a program where it can direct to a program artifact list page
		/// </summary>
		/// <param name="programid"></param>
		/// <param name="link"></param>
        protected string SetProgramArtifactUrl(int programId, NavigationHighlightedLink link)
		{
			string artifactUrl = "";
			// if we are on artifact that programs have (for either a program or project) then go to the program artifact
			// we also check if the group artifact is supported by the license
			if (
                ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.GroupPlanningBoard) 
				&& (link == NavigationHighlightedLink.GroupPlanningBoard || link == NavigationHighlightedLink.PlanningBoard)
				)
			{
				artifactUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.GroupPlanningBoard, programId);
			}
			else if (
                ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.GroupReleases) 
				&& (link == NavigationHighlightedLink.GroupReleases || link == NavigationHighlightedLink.Releases)
				)
			{
				artifactUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.GroupReleases, programId);
			}
			else if (
                ArtifactManager.IsSupportedByLicense(DataModel.Artifact.ArtifactTypeEnum.GroupIncidents) 
				&& (link == NavigationHighlightedLink.GroupIncidents || link == NavigationHighlightedLink.Incidents)
				)
			{
				artifactUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.GroupIncidents, programId);
			}
			// otherwise go to the program dashboard
			else
			{
				artifactUrl = UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, programId);
			}

			return artifactUrl;
		}

		/// <summary>
        /// Returns a string of the url to use for a portfolio where it can direct to a portfolio artifact list page
        /// </summary>
        /// <param name="portfolioId">The id of the portfolio</param>
        /// <param name="link">The artifact link</param>
        protected string SetPortfolioArtifactUrl(int portfolioId, NavigationHighlightedLink link)
        {
            //Right now we don't have any portfolio artifacts, only the dashboard

            string artifactUrl = UrlRewriterModule.RetrievePortfolioRewriterURL(UrlRoots.NavigationLinkEnum.PortfolioHome, portfolioId);

            return artifactUrl;
        }

        /// <summary>
		/// Returns a string of the url to use for a project artifact 
		/// </summary>
		/// <param name="projectId"></param>
		/// <param name="link"></param>
        protected string SetProjectArtifactUrl(int projectId, NavigationHighlightedLink link)
		{
			string artifactUrl = "";
			// if we are on a program artifact we switch to the project equivalent
			if (link == NavigationHighlightedLink.GroupPlanningBoard)
			{
				artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.PlanningBoard, projectId);
			}
			else if (link == NavigationHighlightedLink.GroupReleases)
			{
				artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, projectId);
			}
			else if (link == NavigationHighlightedLink.GroupIncidents)
			{
				artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, projectId);
			}

            // if we are on the portfolio or enterprise dashboard we switch to the project home page
            else if (link == NavigationHighlightedLink.PortfolioHome || link == NavigationHighlightedLink.EnterpriseHome)
            {
                artifactUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, projectId);
            }

			// otherwise we go straight to the normal project artifact
			else
			{
				artifactUrl = UrlRewriterModule.RetrieveRewriterURL((UrlRoots.NavigationLinkEnum)link, projectId);
			}

			return artifactUrl;
		}

        /// <summary>Before the secondary navigation databinds, need to specify the starting node</summary>
        /// <param name="sender">DataSource</param>
        /// <param name="e">EventArgs</param>
        void srcSecondaryNavigation_Load(object sender, EventArgs e)
        {
            //Determine the starting node from the sitemap by value
            foreach (SiteMapNode node in SiteMap.RootNode.GetAllNodes())
            {
                if (node["value"] != null)
                {
                    int linkId = Int32.Parse(node["value"]);
                    if ((int)this.highlightedLink == linkId)
                    {
                        //If we are at depth 0, we have no sub items, otherwise display children
                        if (node.ParentNode == null)
                        {
                            //We are at the root node (i.e. My Page)
                            this.srcSecondaryNavigation.StartingNodeUrl = node.Url;
                            this.srcSecondaryNavigation.ShowStartingNode = true;
                        }
                        else
                        {
                            //See if we're directly under the root node
                            if (node.ParentNode == SiteMap.RootNode)
                            {
                                //Just display the existing node as the only secondary link
                                this.srcSecondaryNavigation.StartingNodeUrl = node.Url;
                                this.srcSecondaryNavigation.ShowStartingNode = true;
                            }
                            else
                            {
                                //Otherwise we need to display the various child options
                                this.srcSecondaryNavigation.StartingNodeUrl = node.ParentNode.Url;
                                this.srcSecondaryNavigation.ShowStartingNode = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This sets up the user control upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            //Set the client-side event handlers
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("loaded", "ajxSearchResults_loaded");
            //this.ajxSearchResults.SetClientEventHandlers(handlers);

            //Delegate to the load and bind method
            LoadAndBind();
        }

        /// <summary>
        /// Loads and databind the global navigation.
        /// </summary>
        internal void LoadAndBind()
        {
			const string METHOD_NAME = "LoadAndBind";

			try
			{
				//Display the user's project role in the subheader     
				ProfileEx profile = new ProfileEx();
                if (!String.IsNullOrEmpty(ProjectRoleName))
                {
                    this.ltrRoleName.Text = profile.FullName + ": ";
                    string roleName = "";

                    if (profile.IsAdmin)
                    {
                        roleName = "Product Owner";
                    }
                    else
                    {
                        roleName = ProjectRoleName;
                    }
                    //Safely encode the final string so any bad html in the name or product role does not get run on render
                    this.ltrRoleName.Text += roleName;
                    this.ltrRoleName.Text = Encoder.HtmlEncode(this.ltrRoleName.Text);

                    //Add the description as a tooltip
                    ProjectManager projectManager = new ProjectManager();
                    ProjectRole projectRole = projectManager.RetrieveRoleById(profile.IsAdmin ? ProjectManager.ProjectRoleProjectOwner : ProjectRoleId);
                    if (projectRole != null)
                    {
                        this.tooltipRoleName.InnerHtml = Encoder.HtmlEncode(projectRole.Description);
                    }
                }

				//If the user doesn't have view permissions for the page, redirect back to project list
				if (!GetPermissions((int)this.highlightedLink))
				{
					Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow because we cannot display an error page since it uses the navigation
				Response.Write(exception.Message);
			}
        }

        /// <summary>
        /// Highlights the selected secondary navigation link when the repeater is databound
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        void rptNavigationLinks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            //Appropriately highlight the secondary navigation
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //See if this item is the same as the highlighted link or not
                SiteMapNode node = (SiteMapNode)e.Item.DataItem;
                if (node["value"] != null)
                {
                    int linkId = Int32.Parse(node["value"]);
                    if ((int)this.highlightedLink == linkId)
                    {
                        HyperLink lnkSecondaryNavigation = (HyperLink)e.Item.FindControl("lnkSecondaryNavigation");
                        if (lnkSecondaryNavigation != null)
                        {
							lnkSecondaryNavigation.CssClass = "tdn tdn-hover nav-active nav-active-hover";
                        }

                        //Now set the appropriate breadcrumb text
                        if (breadcrumbText != "")
                        {
                            Label lblBreadcrumbText = (Label)e.Item.FindControl("lblBreadcrumbText");
                            if (lblBreadcrumbText != null)
                            {
                                lblBreadcrumbText.Text = Microsoft.Security.Application.Encoder.HtmlEncode(breadcrumbText);
                            }
							PlaceHolder plcBreadcrumbText = (PlaceHolder)e.Item.FindControl("plcBreadcrumbText");
							{
								plcBreadcrumbText.Visible = true;
							}
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods Internal to the Control

        /// <summary>
        /// Returns all tour information
        /// </summary>
        private UserSettingsCollection GuidedTour_RetrieveAll()
        {
            //First get the guided tour settings collection
            UserSettingsCollection guidedTourCollection = new UserSettingsCollection(UserId, GlobalFunctions.USER_SETTINGS_GUIDED_TOURS_STATE);
            guidedTourCollection.Restore();
            return guidedTourCollection;
        }

        /// <summary>
        /// Displays the url from the sitemap with the project id token converted into the current project id
        /// </summary>
        /// <param name="dataItem">The data item</param>
        /// <returns>The project-specific URL</returns>
        protected string GetDetokenizedUrl(object dataItem, bool checkForGroup = false)
        {
            if (dataItem is SiteMapNode)
            {
                SiteMapNode siteMapNode = (SiteMapNode)dataItem;

                //See if we need to transform the ${projId} token
                int projectId = -1;
                if (prjUrl.IsMatch(HttpContext.Current.Request.RawUrl))
                {
                    Match match = prjUrl.Match(HttpContext.Current.Request.RawUrl);
                    int projIdTry = 0;
                    if (int.TryParse(match.Groups["id"].Value, out projIdTry))
                    {
                        projectId = projIdTry;
                    }
                }

                if (projectId == -1)
                {
                    // No projectID in match, get it out of profile.
                    ProfileEx profile = new ProfileEx();
                    if (profile.LastOpenedProjectId.HasValue)
                    {
                        projectId = profile.LastOpenedProjectId.Value;
                    }
                }

                //Replace the URL if appropriate
                if (projectId > -1)
                {
                    string decodedUrl = UrlRewriterModule.GetSiteMapURL(HttpContext.Current.Server.UrlDecode(siteMapNode.Url));
                    string fixedUrl = prjToken.Replace(decodedUrl, projectId.ToString()).Trim('#');

					//See if we need to replace the {projGroupId} token
					if (checkForGroup && ProjectGroupId > 0)
					{
						fixedUrl = fixedUrl.Replace("{projgroupid}", ProjectGroupId.ToString(), StringComparison.InvariantCultureIgnoreCase);
					}
					return fixedUrl;
				}
				else
                {
					//There is no project ID, so simply remove
                    string decodedUrl = UrlRewriterModule.GetSiteMapURL(HttpContext.Current.Server.UrlDecode(siteMapNode.Url));
                    string fixedUrl = prjToken.Replace(decodedUrl, "").Trim('#');
					//Handle the case where we are installed as the app root (no vdir)
					if (fixedUrl.StartsWith("//"))
					{
						fixedUrl = fixedUrl.Substring(1);
					}
                    return fixedUrl;
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the enabled permissions for menu items
        /// </summary>
        /// <param name="linkId">The id of the selected link</param>
        protected bool GetPermissions(int linkId)
        {
            bool enabled = true;

			//Check if user is a system admin and a member of the project
			//If so then they should have the same permissions as the read only role of Product Owner - which is 1.
			int projectRoleIdToCheck = ProjectRoleId;
			if (UserIsAdmin && ProjectRoleId > 0)
			{
				projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
			}

            //Hide the navigation link(s)
            //We allow limited view permissions because that means the individual pages and web services will then check the permissions
            Business.ProjectManager projectManager = new Business.ProjectManager();
            switch ((GlobalNavigation.NavigationHighlightedLink)linkId)
            {
                case GlobalNavigation.NavigationHighlightedLink.Requirements:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Releases:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.TestCases:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.TestCase, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.TestRuns:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.TestRun, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.TestSets:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.TestSet, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Incidents:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Incident, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Tasks:
				case GlobalNavigation.NavigationHighlightedLink.PullRequests:
					enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

				case GlobalNavigation.NavigationHighlightedLink.Documents:
					enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Document, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
					break;

                case GlobalNavigation.NavigationHighlightedLink.Iterations:
                case GlobalNavigation.NavigationHighlightedLink.PlanningBoard:
                    enabled = (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Release, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited
                        && projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Task, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited);
                    break;

                case GlobalNavigation.NavigationHighlightedLink.MyTimecard:
					enabled = License.LicenseProductName != LicenseProductNameEnum.SpiraTest;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Resources:
                    //Anyone with project access can see resources, since we limit the data on the resources pages
                    //as necessary
                    enabled = License.LicenseProductName != LicenseProductNameEnum.SpiraTest;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Administration:
                    enabled = (this.UserIsAdmin
                        || SpiraContext.Current.IsProjectAdmin
						|| SpiraContext.Current.IsTemplateAdmin
						|| SpiraContext.Current.IsGroupAdmin
						|| this.UserIsReportAdmin);
                    break;

                case GlobalNavigation.NavigationHighlightedLink.AutomationHosts:
                    enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.AutomationHost, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
                    break;

                case GlobalNavigation.NavigationHighlightedLink.Reports:
                    enabled = (projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.None, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized);
                    break;

					//PCS
                case GlobalNavigation.NavigationHighlightedLink.SourceCode:
                case GlobalNavigation.NavigationHighlightedLink.SourceCodeRevisions:
                    enabled = (License.LicenseProductName != LicenseProductNameEnum.ValidationMaster
                        && projectManager.IsAuthorizedToViewSourceCode(projectRoleIdToCheck));
                    break;
					//PCS
				case GlobalNavigation.NavigationHighlightedLink.Risks:
					enabled = projectManager.IsAuthorized(projectRoleIdToCheck, DataModel.Artifact.ArtifactTypeEnum.Risk, Project.PermissionEnum.View) != Project.AuthorizationState.Prohibited;
					break;
			}

            return enabled;
        }


		/// <summary>
		/// Creates The admin navigation for the system section
		/// </summary>
		/// <returns>Admin section object</returns>
		protected JSON_AdminSection CreateAdminNavSystem()
		{
			//Create the main section object and populate its properties
			JSON_AdminSection section = new JSON_AdminSection();
			section.Id = "system";
			section.Name = Resources.Main.Admin_System + ": " + Resources.Main.Administration_SystemAdminHome;
			section.Url = "";

			//Create the section links
			//None

			// Create the subsections list
			section.SubSections = new List<JSON_AdminSubSection>();




			
			if (UserIsAdmin)
			{
				// Create the subsections
				JSON_AdminSubSection overview = new JSON_AdminSubSection();
				overview.Index = 1;
				overview.Id = "overview";
				overview.Name = Resources.ClientScript.Global_Workspaces;
				overview.Links = new List<JSON_AdminLink>();

				JSON_AdminSubSection users = new JSON_AdminSubSection();
				users.Index = 2;
				users.Id = "users";
				users.Name = Resources.Main.Admin_Users;
				users.Links = new List<JSON_AdminLink>();

				JSON_AdminSubSection system = new JSON_AdminSubSection();
				system.Index = 3;
				system.Id = "system";
				system.Name = Resources.Main.Admin_System;
				system.Links = new List<JSON_AdminLink>();

				JSON_AdminSubSection integration = new JSON_AdminSubSection();
				integration.Index = 4;
				integration.Id = "integration";
				integration.Name = Resources.Main.Admin_Integration;
				integration.Links = new List<JSON_AdminLink>();


				// Create the overview links
				JSON_AdminLink projectList = new JSON_AdminLink();
				projectList.Index = 1;
				projectList.Id = "projectList";
				projectList.Name = Resources.Main.Admin_ViewEditProjects;
				projectList.Url = "ProjectList";
				overview.Links.Add(projectList);

				JSON_AdminLink programList = new JSON_AdminLink();
				programList.Index = 2;
				programList.Id = "programList";
				programList.Name = Resources.Main.Admin_ViewEditPrograms;
				programList.Url = "ProgramList";
				overview.Links.Add(programList);

				if (Common.Global.Feature_Portfolios)
				{
					JSON_AdminLink portfolioList = new JSON_AdminLink();
					portfolioList.Index = 3;
					portfolioList.Id = "portfolioList";
					portfolioList.Name = Resources.Main.Admin_PortfolioList_Title;
					portfolioList.Url = "PortfolioList";
					overview.Links.Add(portfolioList);
				}

				JSON_AdminLink projectTemplateList = new JSON_AdminLink();
				projectTemplateList.Index = 4;
				projectTemplateList.Id = "projectTemplateList";
				projectTemplateList.Name = Resources.Main.Admin_ViewEditProjectTemplates;
				projectTemplateList.Url = "ProjectTemplateList";
				overview.Links.Add(projectTemplateList);


				// Create the user links
				JSON_AdminLink userList = new JSON_AdminLink();
				userList.Index = 1;
				userList.Id = "userList";
				userList.Name = Resources.Main.UserList_Title;
				userList.Url = "UserList";
				users.Links.Add(userList);

				JSON_AdminLink roleList = new JSON_AdminLink();
				roleList.Index = 2;
				roleList.Id = "roleList";
				roleList.Name = Resources.Main.SiteMap_ViewEditRoles;
				roleList.Url = "RoleList";
				users.Links.Add(roleList);

				JSON_AdminLink activeSessions = new JSON_AdminLink();
				activeSessions.Index = 3;
				activeSessions.Id = "activeSessions";
				activeSessions.Name = Resources.Main.Admin_ActiveSessions;
				activeSessions.Url = "ActiveSessions";
				users.Links.Add(activeSessions);

				if (Common.ConfigurationSettings.Default.Membership_AllowUserRegistration)
				{
					JSON_AdminLink userRequests = new JSON_AdminLink();
					userRequests.Index = 4;
					userRequests.Id = "userRequests";
					userRequests.Name = Resources.Main.UserRequests_Title;
					userRequests.Url = "UserRequests";
					users.Links.Add(userRequests);
				}

				users.Links.Add(new JSON_AdminLink
				{
					Index = 5,
					Id = "ldapConfiguration",
					Name = Resources.Main.Admin_LDAPConfiguration,
					Url = "LdapConfiguration"
				});

				users.Links.Add(new JSON_AdminLink
				{
					Index = 6,
					Id = "oAuth",
					Name = Resources.Main.Admin_OAuthList_Title,
					Url = "LoginProviders"
				});


				// Create the general settings links
				JSON_AdminLink generalSettings = new JSON_AdminLink();
				generalSettings.Index = 1;
				generalSettings.Id = "generalSettings";
				generalSettings.Name = Resources.Main.GeneralSettings_Title;
				generalSettings.Url = "GeneralSettings";
				system.Links.Add(generalSettings);

				JSON_AdminLink eventLog = new JSON_AdminLink();
				eventLog.Index = 2;
				eventLog.Id = "eventLog";
				eventLog.Name = Resources.Main.EventLog_Title;
				eventLog.Url = "EventLog";
				system.Links.Add(eventLog);

				JSON_AdminLink securitySettings = new JSON_AdminLink();
				securitySettings.Index = 3;
				securitySettings.Id = "securitySettings";
				securitySettings.Name = Resources.Main.SecuritySettings_Title;
				securitySettings.Url = "SecuritySettings";
				system.Links.Add(securitySettings);

				JSON_AdminLink emailConfiguration = new JSON_AdminLink();
				emailConfiguration.Index = 4;
				emailConfiguration.Id = "emailConfiguration";
				emailConfiguration.Name = Resources.Main.Admin_Notification_EMailServerSettings;
				emailConfiguration.Url = "EmailConfiguration";
				system.Links.Add(emailConfiguration);

				JSON_AdminLink fileTypeList = new JSON_AdminLink();
				fileTypeList.Index = 5;
				fileTypeList.Id = "fileTypeList";
				fileTypeList.Name = Resources.Main.Admin_FileTypes_NavHeaderFileIcon;
				fileTypeList.Url = "FileTypeList";
				system.Links.Add(fileTypeList);

				// show license details and system info if on premise
				if (Common.Properties.Settings.Default.LicenseEditable)
				{
					JSON_AdminLink licenseDetails = new JSON_AdminLink();
					licenseDetails.Index = 6;
					licenseDetails.Id = "licenseDetails";
					licenseDetails.Name = Resources.Main.Admin_LicenseDetails;
					licenseDetails.Url = "LicenseDetails";
					system.Links.Add(licenseDetails);

					JSON_AdminLink systemInfo = new JSON_AdminLink();
					systemInfo.Index = 7;
					systemInfo.Id = "systemInfo";
					systemInfo.Name = Resources.Main.Admin_SystemInfo;
					systemInfo.Url = "SystemInfo";
					system.Links.Add(systemInfo);
				}


				// Create the integration links
				// only allow version control in SpiraTeam and SpiraPlan AND we are not using TaraVault
				if (Common.License.LicenseProductName != Common.LicenseProductNameEnum.SpiraTest && !Common.Global.Feature_TaraVault)
				{
					JSON_AdminLink versionControl = new JSON_AdminLink();
					versionControl.Index = 1;
					versionControl.Id = "versionControl";
					versionControl.Name = Resources.Main.SiteMap_SourceCode;
					versionControl.Url = "VersionControl";
					integration.Links.Add(versionControl);
				}

				// show taravault if we are hosted and its enabled
				if (Common.Global.Feature_TaraVault)
				{
					JSON_AdminLink taraVault = new JSON_AdminLink();
					taraVault.Index = 1;
					taraVault.Id = "taraVault";
					taraVault.Name = Resources.Main.Admin_VersionControl_TaraVault;
					taraVault.Url = "TaraVault";
					integration.Links.Add(taraVault);
				}

				JSON_AdminLink dataSynchronization = new JSON_AdminLink();
				dataSynchronization.Index = 2;
				dataSynchronization.Id = "dataSynchronization";
				dataSynchronization.Name = Resources.Main.Admin_DataSynchronization;
				dataSynchronization.Url = "DataSynchronization";
				integration.Links.Add(dataSynchronization);

				integration.Links.Add(new JSON_AdminLink
				{
					Index = 3,
					Id = "automationEngines",
					Name = Resources.Main.Admin_TestAutomation,
					Url = "AutomationEngines"
				});

				integration.Links.Add(new JSON_AdminLink
				{
					Index = 4,
					Id = "services",
					Name = Resources.Main.Admin_WebServices,
					Url = "Services/",
					UrlIsFromRoot = true
				});


				// finally add all the subsections
				section.SubSections.Add(overview);
				section.SubSections.Add(users);
				section.SubSections.Add(system);
				section.SubSections.Add(integration);
			}

			// Create the reporting section - available to either system or report admins
			if (UserIsAdmin || UserIsReportAdmin)
			{
				JSON_AdminSubSection reporting = new JSON_AdminSubSection();
				reporting.Index = 5;
				reporting.Id = "reporting";
				reporting.Name = Resources.Main.Admin_Reporting;
				reporting.Links = new List<JSON_AdminLink>();
				reporting.AdminPermissions = new int[] { (int)Project.PermissionEnum.SystemAdmin, (int)Project.PermissionEnum.ReportAdmin };


				// Create the reporting links
				JSON_AdminLink reports = new JSON_AdminLink();
				reports.Index = 1;
				reports.Id = "reports";
				reports.Name = Resources.Main.Admin_Reports;
				reports.Url = "Reports";
				reporting.Links.Add(reports);

				JSON_AdminLink graphs = new JSON_AdminLink();
				graphs.Index = 2;
				graphs.Id = "graphs";
				graphs.Name = Resources.Main.Admin_Graphs;
				graphs.Url = "Graphs";
				reporting.Links.Add(graphs);


				// finally add all the subsections
				section.SubSections.Add(reporting);
			}

			return section;
		}


		/// <summary>
		/// Creates The admin navigation for the system section
		/// </summary>
		/// <returns>Admin section object</returns>
		protected JSON_AdminSection CreateAdminNavProject()
		{
			//Create the main section object and populate its properties
			JSON_AdminSection section = new JSON_AdminSection();
			section.Id = "project";
			section.WorkspaceId = (int)SpiraContext.Current.ProjectId;
			section.Name = SpiraContext.Current.ProjectName;
			section.Url = "Default";

			//Create the section links
			// none

			// Create the subsections
			section.SubSections = new List<JSON_AdminSubSection>();

			JSON_AdminSubSection overview = new JSON_AdminSubSection();
			overview.Index = 1;
			overview.Id = "overview";
			overview.Name = Resources.Main.GeneralSettings_Title;
			overview.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection users = new JSON_AdminSubSection();
			users.Index = 2;
			users.Id = "users";
			users.Name = Resources.Main.Admin_Users;
			users.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection planning = new JSON_AdminSubSection();
			planning.Index = 3;
			planning.Id = "planning";
			planning.Name = Resources.Main.Admin_Planning;
			planning.Links = new List<JSON_AdminLink>();


			// Create the overview links
			JSON_AdminLink historyList = new JSON_AdminLink();
			historyList.Index = 1;
			historyList.Id = "historyList";
			historyList.Name = Resources.Main.Admin_HistoryChangeset;
			historyList.Url = "HistoryList";
			overview.Links.Add(historyList);

            JSON_AdminLink projectAssociations = new JSON_AdminLink();
            projectAssociations.Index = 2;
            projectAssociations.Id = "projectAssociations";
            projectAssociations.Name = Resources.Main.Admin_ProjectAssociations;
            projectAssociations.Url = "ProjectAssociations";
            overview.Links.Add(projectAssociations);

            JSON_AdminLink dataSynchronization = new JSON_AdminLink();
            dataSynchronization.Index = 3;
            dataSynchronization.Id = "dataSynchronization";
            dataSynchronization.Name = Resources.Main.Admin_DataSynchronization;
            dataSynchronization.Url = "DataSynchronization";
            overview.Links.Add(dataSynchronization);

            // only allow source code admin in SpiraTeam and SpiraPlan
            if (Common.License.LicenseProductName != Common.LicenseProductNameEnum.SpiraTest)
            {
                //Is TaraVault enabled or external source code
                if (Common.Global.Feature_TaraVault)
                {
                    JSON_AdminLink taraVault = new JSON_AdminLink();
                    taraVault.Index = 4;
                    taraVault.Id = "taraVault";
                    taraVault.Name = Resources.Main.SiteMap_SourceCode;
                    taraVault.Url = "TaraVaultProjectSettings";
                    overview.Links.Add(taraVault);
                }
                // otherwise show the normal link to source code
                else
                {
                    JSON_AdminLink versionControl = new JSON_AdminLink();
                    versionControl.Index = 4;
                    versionControl.Id = "versionControl";
                    versionControl.Name = Resources.Main.SiteMap_SourceCode;
                    versionControl.Url = "VersionControlProjectSettings";
                    overview.Links.Add(versionControl);
                }
            }

            JSON_AdminLink dataTools = new JSON_AdminLink();
            dataTools.Index = 5;
            dataTools.Id = "dataTools";
            dataTools.Name = Resources.Main.Admin_DataTools;
            dataTools.Url = "DataTools";
            overview.Links.Add(dataTools);

            //get project settings and check if baselining is enabled for this project and install
            ProjectSettings projectSettings = new ProjectSettings(ProjectId);
            if (Common.Global.Feature_Baselines && projectSettings != null && projectSettings.BaseliningEnabled)
            {
                JSON_AdminLink baselines = new JSON_AdminLink();
                baselines.Index = 6;
                baselines.Id = "baselines";
                baselines.Name = Resources.Main.Admin_BaselineList;
                baselines.Url = "BaselineList";
                overview.Links.Add(baselines);

            }


            // Create the user links
            JSON_AdminLink projectMembership = new JSON_AdminLink();
			projectMembership.Index = 1;
			projectMembership.Id = "projectMembership";
			projectMembership.Name = Resources.Main.Admin_ProjectMembership;
			projectMembership.Url = "ProjectMembership";
			users.Links.Add(projectMembership);

			JSON_AdminLink assignedApprovers = new JSON_AdminLink();
			assignedApprovers.Index = 2;
			assignedApprovers.Id = "assignedApprovers";
			assignedApprovers.Name = "Assigned Approvers";
			assignedApprovers.Url = "AssignedApprovers";
			users.Links.Add(assignedApprovers);

			// Create the planning links
			JSON_AdminLink planningOptions = new JSON_AdminLink();
			planningOptions.Index = 1;
			planningOptions.Id = "planningOptions";
			planningOptions.Name = Resources.Main.Admin_PlanningOptions;
			planningOptions.Url = "PlanningOptions";
			planning.Links.Add(planningOptions);

            JSON_AdminLink testingSettings = new JSON_AdminLink();
            testingSettings.Index = 2;
            testingSettings.Id = "testingSettings";
            testingSettings.Name = Resources.Main.TestingSettings_Title;
            testingSettings.Url = "TestingSettings";
            planning.Links.Add(testingSettings);

            JSON_AdminLink components = new JSON_AdminLink();
			components.Index = 3;
			components.Id = "components";
			components.Name = Resources.Main.Admin_EditComponents;
			components.Url = "Components";
			planning.Links.Add(components);


			// finally add all the subsections
			section.SubSections.Add(overview);
			section.SubSections.Add(users);
			section.SubSections.Add(planning);

			return section;
		}

		/// <summary>
		/// Creates The admin navigation for the system section
		/// </summary>
		/// <returns>Admin section object</returns>
		protected JSON_AdminSection CreateAdminNavProgram()
		{
			//Create the main section object and populate its properties
			JSON_AdminSection section = new JSON_AdminSection();
			section.Id = Resources.Fields.Program;
			section.WorkspaceId = (int)SpiraContext.Current.ProjectGroupId;
			section.Name = SpiraContext.Current.ProjectGroupName;

			//Create the section links
			// none

			// Create the subsections
			section.SubSections = new List<JSON_AdminSubSection>();

			JSON_AdminSubSection overview = new JSON_AdminSubSection();
			overview.Index = 1;
			overview.Id = "overview";
			overview.Name = Resources.Main.Admin_Programs;
			overview.Links = new List<JSON_AdminLink>();

			// Create the overview links
			JSON_AdminLink edit = new JSON_AdminLink();
			edit.Index = 1;
			edit.Id = "edit";
			edit.Name = Resources.Main.Admin_ViewEditPrograms;
			edit.Url = "Edit";
			overview.Links.Add(edit);

			// finally add all the subsections
			section.SubSections.Add(overview);

			return section;
		}

		/// <summary>
		/// Creates The admin navigation for the system section
		/// </summary>
		/// <returns>Admin section object</returns>
		protected JSON_AdminSection CreateAdminNavTemplate()
		{
			//Create the main section object and populate its properties
			JSON_AdminSection section = new JSON_AdminSection();
			section.Id = "template";
			section.WorkspaceId = (int)SpiraContext.Current.ProjectTemplateId;
			section.Name = Resources.Fields.Template + (String.IsNullOrEmpty(SpiraContext.Current.ProjectTemplateName) ? "" : ": " + SpiraContext.Current.ProjectTemplateName);
			section.Url = "Default";

			//Create the section links
			//none

			// Create the subsections
			section.SubSections = new List<JSON_AdminSubSection>();

			JSON_AdminSubSection requirements = new JSON_AdminSubSection();
			requirements.Index = 1;
			requirements.Id = "requirements";
			requirements.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Requirement;
			requirements.Name = Resources.Main.Admin_Requirements;
			requirements.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection releases = new JSON_AdminSubSection();
			releases.Index = 2;
			releases.Id = "releases";
			releases.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Release;
			releases.Name = Resources.Main.SiteMap_Releases;
			releases.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection documents = new JSON_AdminSubSection();
			documents.Index = 3;
			documents.Id = "documents";
			documents.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Document;
			documents.Name = Resources.Main.Admin_Documents;
			documents.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection testCases = new JSON_AdminSubSection();
			testCases.Index = 4;
			testCases.Id = "testCases";
			testCases.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.TestCase;
			testCases.Name = Resources.Main.SiteMap_TestCases;
			testCases.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection incidents = new JSON_AdminSubSection();
			incidents.Index = 5;
			incidents.Id = "incidents";
			incidents.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Incident;
			incidents.Name = Resources.Main.Admin_Incidents;
			incidents.Links = new List<JSON_AdminLink>();

			// only allow tasks if supported
			if (Common.Global.Feature_Tasks)
			{
				JSON_AdminSubSection tasks = new JSON_AdminSubSection();
				tasks.Index = 6;
				tasks.Id = "tasks";
				tasks.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Task;
				tasks.Name = Resources.Main.SiteMap_Tasks;
				tasks.Links = new List<JSON_AdminLink>();

                // Create the task links
                JSON_AdminLink taskWorkflows = new JSON_AdminLink();
                taskWorkflows.Index = 1;
                taskWorkflows.Id = "taskWorkflows";
                taskWorkflows.Name = Resources.Main.Admin_Workflows_Title;
                taskWorkflows.Url = "TaskWorkflows";
                tasks.Links.Add(taskWorkflows);

                JSON_AdminLink taskTypes = new JSON_AdminLink();
				taskTypes.Index = 2;
				taskTypes.Id = "taskTypes";
				taskTypes.Name = Resources.Main.Admin_Types_Title;
				taskTypes.Url = "TaskTypes";
				tasks.Links.Add(taskTypes);

				JSON_AdminLink taskPriorities = new JSON_AdminLink();
				taskPriorities.Index = 3;
				taskPriorities.Id = "taskPriorities";
				taskPriorities.Name = Resources.Main.Admin_Priority_Title;
				taskPriorities.Url = "TaskPriorities";
				tasks.Links.Add(taskPriorities);

				JSON_AdminLink taskCustomProperties = new JSON_AdminLink();
				taskCustomProperties.Index = 4;
				taskCustomProperties.Id = "taskCustomProperties";
				taskCustomProperties.Name = Resources.Main.Admin_CustomProperties;
				taskCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Task;
				tasks.Links.Add(taskCustomProperties);

				section.SubSections.Add(tasks);
			}

			// only allow risks if supported
            if (Common.Global.Feature_Risks)
			{
				JSON_AdminSubSection risks = new JSON_AdminSubSection();
				risks.Index = 7;
				risks.Id = "risks";
				risks.NavigationId = (int)DataModel.Artifact.ArtifactTypeEnum.Risk;
				risks.Name = Resources.Main.SiteMap_Risks;
				risks.Links = new List<JSON_AdminLink>();

                // Create the risk links
                JSON_AdminLink riskWorkflows = new JSON_AdminLink();
                riskWorkflows.Index = 1;
                riskWorkflows.Id = "riskWorkflows";
                riskWorkflows.Name = Resources.Main.Admin_Workflows_Title;
                riskWorkflows.Url = "RiskWorkflows";
                risks.Links.Add(riskWorkflows);

				JSON_AdminLink riskStatuses = new JSON_AdminLink();
				riskStatuses.Index = 2;
				riskStatuses.Id = "riskStatuses";
				riskStatuses.Name = Resources.Main.Admin_Statuses_Title;
				riskStatuses.Url = "RiskStatuses";
				risks.Links.Add(riskStatuses);

                JSON_AdminLink riskTypes = new JSON_AdminLink();
                riskTypes.Index = 3;
                riskTypes.Id = "riskTypes";
                riskTypes.Name = Resources.Main.Admin_Types_Title;
                riskTypes.Url = "RiskTypes";
                risks.Links.Add(riskTypes);

                JSON_AdminLink riskProbabilities = new JSON_AdminLink();
				riskProbabilities.Index = 4;
				riskProbabilities.Id = "riskProbabilities";
				riskProbabilities.Name = Resources.Main.Admin_Probability_Title;
				riskProbabilities.Url = "RiskProbabilities";
				risks.Links.Add(riskProbabilities);

				JSON_AdminLink riskImpacts = new JSON_AdminLink();
				riskImpacts.Index = 5;
				riskImpacts.Id = "riskImpacts";
				riskImpacts.Name = Resources.Main.Admin_Impact_Title;
				riskImpacts.Url = "RiskImpacts";
				risks.Links.Add(riskImpacts);

				JSON_AdminLink riskCustomProperties = new JSON_AdminLink();
				riskCustomProperties.Index = 6;
				riskCustomProperties.Id = "riskCustomProperties";
				riskCustomProperties.Name = Resources.Main.Admin_CustomProperties;
				riskCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Risk;
				risks.Links.Add(riskCustomProperties);

				section.SubSections.Add(risks);
			}

			JSON_AdminSubSection customProperties = new JSON_AdminSubSection();
			customProperties.Index = 7;
			customProperties.Id = "customProperties";
			customProperties.Name = Resources.Main.Admin_CustomProperties;
			customProperties.Links = new List<JSON_AdminLink>();

			JSON_AdminSubSection notifications = new JSON_AdminSubSection();
			notifications.Index = 8;
			notifications.Id = "notifications";
			notifications.Name = Resources.Main.Admin_Notifications;
			notifications.Links = new List<JSON_AdminLink>();


            // Create the requirements links
            JSON_AdminLink requirementWorkflows = new JSON_AdminLink();
            requirementWorkflows.Index = 1;
            requirementWorkflows.Id = "requirementWorkflows";
            requirementWorkflows.Name = Resources.Main.Admin_Workflows_Title;
            requirementWorkflows.Url = "RequirementWorkflows";
            requirements.Links.Add(requirementWorkflows);

            JSON_AdminLink requirementTypes = new JSON_AdminLink();
			requirementTypes.Index = 2;
			requirementTypes.Id = "requirementTypes";
			requirementTypes.Name = Resources.Main.Admin_Types_Title;
			requirementTypes.Url = "RequirementTypes";
			requirements.Links.Add(requirementTypes);

			JSON_AdminLink requirementImportances = new JSON_AdminLink();
			requirementImportances.Index = 3;
			requirementImportances.Id = "requirementImportances";
			requirementImportances.Name = Resources.Main.Admin_Importance_Title;
			requirementImportances.Url = "RequirementImportances";
			requirements.Links.Add(requirementImportances);

			JSON_AdminLink requirementCustomProperties = new JSON_AdminLink();
			requirementCustomProperties.Index = 4;
			requirementCustomProperties.Id = "requirementCustomProperties";
			requirementCustomProperties.Name = Resources.Main.Admin_CustomProperties;
			requirementCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Requirement;
			requirements.Links.Add(requirementCustomProperties);


			// Create the releases links
			JSON_AdminLink releaseWorkflows = new JSON_AdminLink();
			releaseWorkflows.Index = 1;
			releaseWorkflows.Id = "releaseWorkflows";
			releaseWorkflows.Name = Resources.Main.Admin_Workflows_Title;
			releaseWorkflows.Url = "ReleaseWorkflows";
			releases.Links.Add(releaseWorkflows);

			JSON_AdminLink releaseCustomProperties = new JSON_AdminLink();
			releaseCustomProperties.Index = 2;
			releaseCustomProperties.Id = "releaseCustomProperties";
			releaseCustomProperties.Name = Resources.Main.Admin_CustomProperties;
			releaseCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Release;
			releases.Links.Add(releaseCustomProperties);


			// Create the document links
			JSON_AdminLink documentWorkflows = new JSON_AdminLink();
            documentWorkflows.Index = 1;
            documentWorkflows.Id = "documentWorkflows";
            documentWorkflows.Name = Resources.Main.Admin_Workflows_Title;
            documentWorkflows.Url = "DocumentWorkflows";
            documents.Links.Add(documentWorkflows);

            JSON_AdminLink documentTypes = new JSON_AdminLink();
			documentTypes.Index = 2;
			documentTypes.Id = "documentTypes";
			documentTypes.Name = Resources.Main.Admin_Types_Title;
			documentTypes.Url = "DocumentTypes";
			documents.Links.Add(documentTypes);

			JSON_AdminLink documentStatuses = new JSON_AdminLink();
			documentStatuses.Index = 3;
			documentStatuses.Id = "documentStatuses";
			documentStatuses.Name = Resources.Main.Admin_Statuses_Title;
			documentStatuses.Url = "DocumentStatuses";
			documents.Links.Add(documentStatuses);

			JSON_AdminLink documentCustomProperties = new JSON_AdminLink();
			documentCustomProperties.Index = 4;
			documentCustomProperties.Id = "documentCustomProperties";
			documentCustomProperties.Name = Resources.Main.Admin_CustomProperties;
			documentCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Document;
			documents.Links.Add(documentCustomProperties);


			// Create the test cases links
			JSON_AdminLink testCaseWorkflows = new JSON_AdminLink();
            testCaseWorkflows.Index = 1;
            testCaseWorkflows.Id = "testCaseWorkflows";
            testCaseWorkflows.Name = Resources.Main.Admin_Workflows_Title;
            testCaseWorkflows.Url = "TestCaseWorkflows";
            testCases.Links.Add(testCaseWorkflows);

            JSON_AdminLink testCaseTypes = new JSON_AdminLink();
			testCaseTypes.Index = 2;
			testCaseTypes.Id = "testCaseTypes";
			testCaseTypes.Name = Resources.Main.Admin_Types_Title;
			testCaseTypes.Url = "TestCaseTypes";
			testCases.Links.Add(testCaseTypes);

			JSON_AdminLink testCasePriorities = new JSON_AdminLink();
			testCasePriorities.Index = 3;
			testCasePriorities.Id = "testCasePriorities";
			testCasePriorities.Name = Resources.Main.Admin_Priority_Title;
			testCasePriorities.Url = "TestCasePriorities";
			testCases.Links.Add(testCasePriorities);

			JSON_AdminLink testCaseCustomProperties = new JSON_AdminLink();
			testCaseCustomProperties.Index = 4;
			testCaseCustomProperties.Id = "testCaseCustomProperties";
			testCaseCustomProperties.Name = Resources.Main.Admin_CustomProperties;
			testCaseCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.TestCase;
			testCases.Links.Add(testCaseCustomProperties);


			// Create the incident links
			JSON_AdminLink incidentWorkflows = new JSON_AdminLink();
            incidentWorkflows.Index = 1;
            incidentWorkflows.Id = "incidentWorkflows";
            incidentWorkflows.Name = Resources.Main.Admin_Workflows_Title;
            incidentWorkflows.Url = "IncidentWorkflows";
            incidents.Links.Add(incidentWorkflows);

			JSON_AdminLink incidentStatuses = new JSON_AdminLink();
			incidentStatuses.Index = 2;
			incidentStatuses.Id = "incidentStatuses";
			incidentStatuses.Name = Resources.Main.Admin_Statuses_Title;
			incidentStatuses.Url = "IncidentStatuses";
			incidents.Links.Add(incidentStatuses);

            JSON_AdminLink incidentTypes = new JSON_AdminLink();
            incidentTypes.Index = 3;
            incidentTypes.Id = "incidentTypes";
            incidentTypes.Name = Resources.Main.Admin_Types_Title;
            incidentTypes.Url = "IncidentTypes";
            incidents.Links.Add(incidentTypes);

            JSON_AdminLink incidentPriorities = new JSON_AdminLink();
			incidentPriorities.Index = 4;
			incidentPriorities.Id = "incidentPriorities";
			incidentPriorities.Name = Resources.Main.Admin_Priority_Title;
			incidentPriorities.Url = "IncidentPriorities";
			incidents.Links.Add(incidentPriorities);

			JSON_AdminLink incidentSeverities = new JSON_AdminLink();
			incidentSeverities.Index = 5;
			incidentSeverities.Id = "incidentSeverities";
			incidentSeverities.Name = Resources.Main.Admin_Severity_Title;
			incidentSeverities.Url = "IncidentSeverities";
			incidents.Links.Add(incidentSeverities);

			JSON_AdminLink incidentCustomProperties = new JSON_AdminLink();
			incidentCustomProperties.Index = 6;
			incidentCustomProperties.Id = "incidentCustomProperties";
			incidentCustomProperties.Name = Resources.Main.Admin_CustomProperties;
			incidentCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.Incident;
			incidents.Links.Add(incidentCustomProperties);


			// Create the custom props links
			JSON_AdminLink testStepCustomProperties = new JSON_AdminLink();
			testStepCustomProperties.Index = 1;
			testStepCustomProperties.Id = "testStepCustomProperties";
			testStepCustomProperties.Name = Resources.Main.Admin_CustomProperties_TestStep;
			testStepCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.TestStep;
			customProperties.Links.Add(testStepCustomProperties);

			JSON_AdminLink testSetCustomProperties = new JSON_AdminLink();
			testSetCustomProperties.Index = 2;
			testSetCustomProperties.Id = "testSetCustomProperties";
			testSetCustomProperties.Name = Resources.Main.SiteMap_TestSets;
			testSetCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.TestSet;
			customProperties.Links.Add(testSetCustomProperties);

			JSON_AdminLink testRunCustomProperties = new JSON_AdminLink();
			testRunCustomProperties.Index = 3;
			testRunCustomProperties.Id = "testRunCustomProperties";
			testRunCustomProperties.Name = Resources.Main.SiteMap_TestRuns;
			testRunCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.TestRun;
			customProperties.Links.Add(testRunCustomProperties);

			JSON_AdminLink automationHostCustomProperties = new JSON_AdminLink();
			automationHostCustomProperties.Index = 4;
			automationHostCustomProperties.Id = "automationHostCustomProperties";
			automationHostCustomProperties.Name = Resources.Main.SiteMap_AutomationHosts;
			automationHostCustomProperties.Url = "CustomProperties.aspx?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + (int)Artifact.ArtifactTypeEnum.AutomationHost;
			customProperties.Links.Add(automationHostCustomProperties);

			JSON_AdminLink lists = new JSON_AdminLink();
			lists.Index = 5;
			lists.Id = "lists";
			lists.Name = Resources.Main.CustomLists_Title;
			lists.Url = "CustomLists";
			customProperties.Links.Add(lists);


			// Create the notifcation links
			JSON_AdminLink notificationEvents = new JSON_AdminLink();
			notificationEvents.Index = 1;
			notificationEvents.Id = "notificationEvents";
			notificationEvents.Name = Resources.Main.Admin_Notification_ViewEditNotification;
			notificationEvents.Url = "NotificationEvents";
			notifications.Links.Add(notificationEvents);

			JSON_AdminLink notificationTemplates = new JSON_AdminLink();
			notificationTemplates.Index = 2;
			notificationTemplates.Id = "notificationTemplates";
			notificationTemplates.Name = Resources.Main.Admin_Notification_ViewNotificationTemplates;
			notificationTemplates.Url = "NotificationTemplates";
			notifications.Links.Add(notificationTemplates);

			// finally add all the subsections
			section.SubSections.Add(requirements);
			section.SubSections.Add(releases);
			section.SubSections.Add(testCases);
			section.SubSections.Add(documents);
			section.SubSections.Add(incidents);
			section.SubSections.Add(customProperties);
			section.SubSections.Add(notifications);

			return section;
		}


		#endregion
	}
}
