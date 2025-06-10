<%@ Control 
    AutoEventWireup="True" 
    CodeBehind="GlobalNavigation.ascx.cs" 
    EnableViewState="false"
	Inherits="Inflectra.SpiraTest.Web.UserControls.GlobalNavigation" 
    Language="c#" 
    %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<!-- Begin Global Navigation User Control -->
<asp:SiteMapDataSource ID="srcProjectNavigation" runat="server" ShowStartingNode="false" />
<asp:SiteMapDataSource ID="srcProjectGroupNavigation" runat="server" ShowStartingNode="false" />
<asp:SiteMapDataSource ID="srcSecondaryNavigation" runat="server" />




<!-- React Global Navigation (and ajax activity spinner) -->
<div id="global-navigation">
	<div class="mainMenu"></div>
</div>

<div class="slinding-bar">
<% if(ProjectId != -1)	{ %>

    <div class="ProjectNavigation last bhi">
		<div class="s-tab static-tab-6">
			<div id="enterprise" class="HeaderDropMenuIcon">
				<a class="topNavLink" href="/ValidationMaster/Enterprise" >  
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-organization.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Enterprise%>" /></div>
					</p>				  
				</a>
			</div>
		</div>
	</div>

	<div class="ProjectNavigation last">
		<div class="s-tab static-tab-6">
			<div id="dashboard" class="HeaderDropMenuIcon">
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>.aspx" >  
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-dashboard.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Dashboard%>" /></div>
					</p>				  
				</a>
									
				<ul class="dropdown-menu" id="dashboardDropDown" role="menu" aria-labelledby="dashboardMenu">
					<asp:Repeater ID="rptProjectList2" runat="server">
						<ItemTemplate>
							<asp:PlaceHolder runat="server">
								<li>moo
								</li>
							</asp:PlaceHolder>
							<asp:PlaceHolder runat="server" Visible="true">
								<li class="divider"></li>
							</asp:PlaceHolder>
						</ItemTemplate>
					</asp:Repeater>
				</ul>	
			</div>
		</div>
	</div>

	<div class="ProjectNavigation last">
		<div class="s-tab static-tab-6">
			<div id="planning" class="HeaderDropMenuIcon dropdown">
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/Requirement/List.aspx">     
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-planning.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Planning%>" /></div>
					</p>
				</a>	
				<button class="btn btn-primary dropdown-toggle" id="planningMenu" type="button" data-toggle="dropdown">
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu" id="planningDropDown" role="menu" aria-labelledby="planningMenu">
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Requirement/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Requirements%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/PlanningBoard.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_PlanningBoard%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Release/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Releases%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Document/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Documents%>" /></a></li>
				</ul>							
			</div>
		</div>
	</div>
	
	<div class="ProjectNavigation last">
		<div class="s-tab static-tab-6">
			<div id="testing" class="dropdown">
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/TestCase/List.aspx">     
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-testing.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Testing%>" /></div>
					</p>
				</a>
				<button class="btn btn-primary dropdown-toggle" id="testingMenu" type="button" data-toggle="dropdown">
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu" id="testingDropDown" role="menu" aria-labelledby="testingMenu">
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/TestCase/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_TestCases%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/TestSet/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_TestSets%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/TestRun/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_TestRuns%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/AutomationHost/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_AutomationHosts%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/TestConfiguration/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_TestConfigurations%>" /></a></li>
				</ul>						    
			</div>
		</div>
	</div>
	
	<div class="ProjectNavigation last">
		<div class="s-tab static-tab-6">
			<div id="tracking" class="HeaderDropMenuIcon dropdown">
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/Incident/List.aspx">     
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-tracking.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Tracking%>" /></div>
					</p>
				</a>	
				<button class="btn btn-primary dropdown-toggle" id="trackingMenu" type="button" data-toggle="dropdown">
					<span class="caret"></span>
				</button>
				<ul class="dropdown-menu" id="trackingDropDown" role="menu" aria-labelledby="trackingMenu">
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Incident/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Incidents%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Task/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Tasks%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/Resource/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Resources%>" /></a></li>
					<li role="presentation"><a role="menuitem" tabindex="-1" href="/ValidationMaster/<%= ProjectId %>/SourceCode/List.aspx"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_SourceCode%>" /></a></li>
				</ul>							
			</div>
		</div>
	</div>
	
	<div class="ProjectNavigation">
		<div class="s-tab static-tabs-1">
			<div id="reporting" class="HeaderDropMenuIcon">
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/Report/List.aspx">
					<p class="image">
						<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-reporting.png">
						<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Reporting%>" /></div>
					</p>
				</a>
			</div>
		</div>
	</div>
	<% } %>
  <div class="ProjectNavigation">
	<div class="s-tab static-tabs-1">
	  <div id="myPage" class="HeaderDropMenuIcon">
	  <% if(ProjectId != -1)	{ %>
			<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/MyPage.aspx">
		<% } else {%>
			<a class="topNavLink" href="/ValidationMaster/MyPage.aspx">
		<% } %>
			<p class="image">
				<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-mypage.png">
				<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_MyPage%>" /></div>
			</p>
		</a>
	  </div>
	</div>
  </div>
	 
	  <div class="ProjectNavigation last">
        <div class="s-tab static-tab-6">
          <div id="admin" class="HeaderDropMenuIcon">
			<% if(ProjectId != -1)	{ %>
				<a class="topNavLink" href="/ValidationMaster/<%= ProjectId %>/Administration.aspx" >   
			<% } else { %>
				<a class="topNavLink" href="/ValidationMaster/Administration.aspx" > 
			<% } %>
				<p class="image">
					<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-administration.png">
					<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,GlobalNavigation_Administration%>" /></div>
				</p>			  
            </a>
          </div>
        </div>
      </div>
			
      <div class="ProjectNavigation last">
        <div class="s-tab static-tab-6">
          <div id="VMportal" class="HeaderDropMenuIcon">
			<a class="topNavLink" href="https://onshore.sharepoint.com/vmaster/Shared%20Documents/BatchExports" target ="_blank" >  
				<p class="image">
					<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/icon-sharepoint.png">
					<div class="ImageText"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Portal%>" /></div>
				</p>				  
            </a>
          </div>
        </div>
      </div>

	<div class="ProjectNavigation last">
        <div class="s-tab static-tab-6">
          <div id="Support" class="HeaderDropMenuIcon">
			<a class="topNavLink" href="https://validationmaster.atlassian.net/servicedesk/customer/portal/2/group/-1" target ="_blank" >  
				<p class="image">
					<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/supporticon.png">
					<div class="ImageText" style="color: black;"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Support%>" /></div>
				</p>				  
            </a>
          </div>
        </div>
    </div>

	<div class="ProjectNavigation last">
        <div class="s-tab static-tab-6">
          <div id="teams" class="HeaderDropMenuIcon">
			<a class="topNavLink" href="https://teams.microsoft.com" target ="_blank" >  
				<p class="image">
					<img runat="server" src="../App_Themes/ValidationMasterTheme/Images/Microsoft_Office_Teams.svg">
					<div class="ImageText" style="color: black;"><asp:Literal runat="server" Text="<%$Resources:Main,SiteMap_Teams%>" /></div>
				</p>				  
            </a>
          </div>
        </div>
    </div>
</div>

<!-- Sub header -->
<div class="nav-secondary">
    <div class="col-sm-6">
        <asp:Repeater runat="server" ID="rptNavigationLinks" DataSourceID="srcSecondaryNavigation">
            <ItemTemplate>
                <tstsc:HyperLinkEx

                    ID="lnkSecondaryNavigation" 
                    NavigateUrl='<%# GetDetokenizedUrl(Container.DataItem, true)%>'
                    runat="server" 
                    >
                    <%# Eval("title")%>
                </tstsc:HyperLinkEx>
                <asp:PlaceHolder 
                    ID="plcBreadcrumbText" 
                    runat="server" 
                    Visible="false"
                    >
                    <span class="nav-muted fa fa-chevron-right v-mid menu-separator"></span>
                    <tstsc:LabelEx 
                        CssClass="nav-muted px3"
                        ID="lblBreadcrumbText" 
                        runat="server"  Visible="true"
                        />
                </asp:PlaceHolder>
            </ItemTemplate>
            <SeparatorTemplate>
                <span class="nav-muted sep">/</span>
            </SeparatorTemplate>
        </asp:Repeater>
    </div>
    <div class="col-sm-6 text-right">
        <div class="has-tooltip di fr">
        <asp:Label
            CssClass="sub-header-right"
            ID="ltrRoleName" 
            runat="server" Visible="true"
            />
        <div class="is-tooltip hidden" runat="server" id="tooltipRoleName" />
    </div>
    </div>
</div>

<script type="text/javascript">
    //Needed because ScriptManager tries to check dependencies before the main scripts are loaded
    if (!window) this.window = this;
    window.Type = Function;
    Type._checkDependency = function (a, b) { return true; }

    //Set the navigation json
    SpiraContext.Navigation = <%=NavigationJSON%>;
    SpiraContext.GuidedToursSeen = true; // <%=GuidedToursJSON%>;
    SpiraContext.VersionNumber = '<%=VersionNumber%>';

	function ucGlobalNavigation_displayShortcuts() {
		$('#global-nav-keyboard-shortcuts').modal();
	}

	//We add the onboarding tours here as the global nav is the first react component that needs access to them
	SpiraContext.GuidedTours = [
		{
			name: "appIntro",
			title: resx.Onboarding_AppIntro_Title.replace("{0}", SpiraContext.ProductType),
			description: "",
			image: "tours-appIntro-icon.svg",
			steps: [
				{
					title: resx.Onboarding_TourAims_Title,
					description: resx.Onboarding_TourAims_AppIntro
				},
				{
					title: resx.Global_MyPage,
					description: resx.GlobalNavigation_TooltipMyPage,
					image: "tours-appIntro-myPage-" + SpiraContext.ProductType + ".png"
				},
				{
					title: resx.Global_Workspaces,
					description: resx.GlobalNavigation_TooltipWorkspace,
					image: "tours-appIntro-workspaces-" + SpiraContext.ProductType + ".png"
				},
				{
					title: resx.Global_WorkspacesDashboard,
					description: resx.GlobalNavigation_TooltipWorkspaceHome,
					image: "tours-appIntro-workspaceHome.png"
				},
				{
					title: resx.Global_Artifacts,
					description: resx.GlobalNavigation_TooltipArtifacts,
					image: "tours-appIntro-artifacts-" + SpiraContext.ProductType + ".png"
				},
				{
					title: resx.ReportViewer_GeneratingReport,
					description: resx.GlobalNavigation_TooltipReporting,
					image: "tours-appIntro-reports.png"
				},
				{
					title: resx.Global_UserProfile,
					description: resx.GlobalNavigation_TooltipUserProfile,
					image: "tours-appIntro-userProfile-" + SpiraContext.ProductType + ".png"
				},
				{
					title: resx.Global_Administration,
					isAdmin: true,
					description: resx.GlobalNavigation_TooltipAdministration,
					image: "tours-appIntro-administration.png"
				},
			]
		},
		{
			name: "update-6.0.0",
			title: "6.0: Foundational changes to Spira",
			description: "",
			image: "tours-update-6.0.0-icon.svg",
			steps: [
				{
					title: resx.Onboarding_TourAims_Title,
					description: "Version 6.0 of " + SpiraContext.ProductType + " is a major release and brings lots of improvements. In the next couple of minutes we're going to walk through some of the ways the app works differently (and, dare we say, better)."
				},
				{
					title: "New terms",
					description: "We have changed some terms here and there, most importantly we now have a concept of a workspace. This sets the context for your work: a project (now called product) is a workspace, a group of products (a program - or project group as was) is also a workspace. And templates (see later in the tour) are also workspaces."
				},
				{
					title: "Navigation",
					description: "We've streamlined the app navigation (the nav bar at the top of every page). In doing so we had to move a few things round. This new design should make it easier to see where you are and get where you want to in the app.",
					image: "tours-update-6.0.0-navigation.png"
				},
				{
					title: "Templates",
					description: "Before, each product was largely its own island. The custom fields and workflows, and incident types in one product were completely separate from those in another product. Now we have templates. You can now manage and change many aspects of a product from its template. One template can control many products at once. For existing products, this gives you more ways to customize types. For new products, you can have them use an existing template.",
					image: "tours-update-6.0.0-templates.png",
					link: "https://www.inflectra.com/Ideas/Entry/spira-60-project-templates-customizable-fields-773.aspx"
				},
				{
					title: "Risks",
					description: "Available in SpiraPlan, the new risks module helps you track, understand, and assess any risks to your products. You can also set up mitigations for each risk",
					image: "tours-update-6.0.0-risks.png",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spiraplan-60-enterprise-risk-management-785.aspx"
				},
				{
					title: "Document Workflows",
					description: "Documents now have full workflow and version control. This lets you have check-ins for documents, along with comments and more fine grained controls of who sees what when.",
					image: "tours-update-6.0.0-document-workflow.png",
					link: "http://www.inflectra.com/Ideas/Entry/spira-60-document-management-workflows-781.aspx"
				},
				{
					title: "Administration",
					description: "Administration menus have been redesigned. Admins can see a full menu with a single click. We have grouped different admin sections together, showing and highlighting the right links at the right time. Templates bring the biggest changes to administration, so experienced admins should take some time to explore the new functionality.",
					image: "tours-appIntro-administration.png"
				}
			]
		},
		{
			name: "update-6.1.0",
			title: "6.1: Hello Dark Mode",
			description: "",
			image: "tours-update-6.1.0-icon.svg",
			steps: [
				{
					title: "Introducing Dark Mode",
					description: "Version 6.1 of " + SpiraContext.ProductType + " comes with a bold new look - dark mode. You can switch between the different modes from the user profile dropdown",
					image: "tours-update-6.1.0-dark-mode.png",
					link: ""
				},
				{
					title: "What's new in 6.0",
					linkToTour: "update-6.0.0",
				}
			]
		},
		{
			name: "update-6.2.0",
			title: "6.2: Requirements go to 11",
			description: "",
			image: "tours-update-6.2.0-icon.svg",
			steps: [
				{
					title: "New Requirement Management Views",
					description: "Version 6.2 of Validation Master and SpiraPlan adds 4 new ways to interact with requirements. As well as the hierarchical view, there's now a document view, mindmap, sortable table, and board view.",
					image: "tours-update-6.2.0-requirement-views.png",
					link: "https://www.inflectra.com/Ideas/Entry/spotlight-on-Validation Master-62-requirements-management-879.aspx"
				},
				{
					title: "Use Case Process Flow Diagrams",
					description: "Validation Master and SpiraPlan let you view your steps for use cases in a graphical flow diagram - viewable on each relevant requirement and on the requirements document view.",
					image: "tours-update-6.2.0-use-case-diagrams.png",
					link: "https://www.inflectra.com/Ideas/Entry/spotlight-on-Validation Master-62-requirements-management-879.aspx"
				},
				{
					title: "Agile Board Enhancements",
					description: "We've made a number of improvements to our agile boards and planning with new views, and enhanced features.",
					link: "https://www.inflectra.com/Ideas/Entry/spotlight-on-Validation Master-62-agile-board-enhancements-880.aspx"
				},
				{
					title: "Risk Management",
					description: "SpiraPlan's risk functionality is now more... functional. We've made it easy to associate risks to requirements, tests, incidents, or other risks.",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-on-spiraplan-62-risk-management-features-881.aspx"
				},
				{
					title: "What's new in 6.0",
					linkToTour: "update-6.0.0",
				}
			]
		},
		{
			name: "update-6.3.0",
			title: "6.3: Filters and Folders",
			description: "",
			image: "tours-update-6.3.0-icon.png",
			steps: [
				{
					title: "Improved filters",
					description: "You can now update your filters and shared filters. Your filters also save information about the columns to show, their order, and width.",
					link: "http://spiradoc.inflectra.com/Spira-User-Manual/Application-Wide/#filtering",
					image: "tours-update-6.3.0-filters.png",
				},
				{
					title: "Improved navigation between folders and hierarchies",
					description: "Each folder now has its own unique url, so you can share links to specific folders with your team. For requirements, releases, and all artifacts with folders new clickable breadcrumbs making it easy to go straight to an artifact's parent.",
					image: "tours-update-6.3.0-folders.png",
				},
				{
					title: "What's new in 6.2",
					linkToTour: "update-6.2.0",
				}
			]
		},
		{
			name: "update-6.4.0",
			title: "6.4: Single Sign On and Reporting",
			description: "",
			image: "tours-update-6.4.0-icon.svg",
			steps: [
				{
					title: "Sign Sign On",
					description: "Sign in to Spira using the cloud identity provider you already use. System admins can let users connect using Github, Gitlab, Google, Microsoft Azure AD, Microsoft ADFS, or Okta services.",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spira-64-single-sign-on-oauth-reports-989.aspx",
					image: "tours-update-6.4.0-oauth.png",
				},
				{
					title: "Reporting by Release",
					description: "Spira reports center dashboard has a new, central release picker. This selection will affect all of the reporting / graphing widgets simultaneously, and will make configuring the dashboard by release much easier.",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spira-64-single-sign-on-oauth-reports-989.aspx",
					image: "tours-update-6.4.0-reporting.png",
				},
				{
					title: "What's new in 6.3",
					linkToTour: "update-6.3.0",
				}
			]
		},
		{
			name: "update-6.5.0",
			title: "6.5: Portfolios, Gantt charts, Completion charts",
			description: "",
			image: "org-Portfolio.svg",
			steps: [
				{
					title: "Portfolios",
					description: "Available in SpiraPlan, portfolios are new workspaces that let you create higher level groups of workspaces with collections of programs. There is also a new 'enterprise' view for a system-wide view.",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spira-65-portfolio-program-management-1023.aspx",
					image: "tours-update-6.5.0-portfolios.png",
				},
				{
					title: "Enterprise View",
					description: "Availalbe in SpiraPlan, portfolios are new workspaces that let you create higher level groups of workspaces with collections of programs. Ther is also a new 'enterprise' view for a system-wide view.",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spira-65-portfolio-program-management-1023.aspx",
					image: "tours-update-6.5.0-enterprise-view.png",
				},
				{
					title: "My Page Improvements",
					description: "The My Assigned Products widget has been replaced with 2 new widgets: 'Recent Products' shows your five most recently visited products; and 'Recent Artifacts' shows you the five artifacts you last visited.",
					image: "tours-update-6.5.0-mypage.png",
				},
				{
					title: "Improved Dashboards",
					description: "All dashboards now have new widgets that show you key information about how ready each workspace is. These are based off the scheduled dates of releases and if their requirements are completed or not",
					link: "http://www.inflectra.com/Ideas/Entry/spotlight-spira-65-portfolio-program-management-1023.aspx",
					image: "tours-update-6.5.0-dashboards.png",
				},
				{
					title: "Gantt Charts",
					description: "New Gantt charts for workspaces and, in Validation Master and SpiraPlan, releases and tasks, let you see the hierarchy of artifacts in a graphical view, plotted against the dates along the x-axis.",
					image: "tours-update-6.5.0-gantt.png",
				},
				{
					title: "Release Pert Chart",
					description: "Available in Validation Master and SpiraPlan Another a hierarchical PERT chart shows the decomposition of releases, phases and sprints in a top-down tree diagram.",
					image: "tours-update-6.5.0-pert.png",
				},
				{
					title: "Administration Menu",
					description: "We have changed the administration menu to order the page links in each section not alphabetically but in a logical order. This also provides consistency across languages.",
					isAdmin: true,
				},
				{
					title: "What's new in 6.4",
					linkToTour: "update-6.4.0",
				}
			]
		},
		{
			name: "update-6.5.1",
			title: "6.5.1: Extra Planning Dashboard Widgets",
			description: "",
			image: "artifact-Build.svg",
			steps: [
				{
					title: "Product Test Summary",
					description: "This new program home page widget lets you see key metrics across all products at a glance. See the requirements, coverage, test status, and open incidents by priority. You can configure the widget to show data for all or only active releases",
					image: "tours-update-6.5.1-test-summary.png",
					link: "https://www.inflectra.com/Ideas/Entry/spotlight-spira-651-program-portfolio-devops-1041.aspx"
				},
				{
					title: "Recent Builds",
					description: "View recent builds across all active releases in all workspaces in brand new widgets for the program, portfolio, and enterprise home pages. This is a great way to get an overview of your DevOps pipelines across your workspaces",
					image: "tours-update-6.5.1-recent-builds.png",
				},
				{
					title: "More Widget Improvements",
					description: "A number of widgets on the program home page have been upgraded to show results for active releases for each product (each widget has a setting to instead show for all releases). Widgets affected: Requirements Coverage, Test Excecution Status, and Task Progress",
				},
				{
					title: "What's new in 6.5",
					linkToTour: "update-6.5.0",
				}
			]
		},
		{
			name: "update-6.5.2",
			title: "6.5.2: Baselining & Testing Settings",
			description: "",
			image: "artifact-Baseline.svg",
			steps: [
				{
					title: "Baselines",
					description: "Validation Master and SpiraPlan products can now be set to use baselining. Baselines create a snapshot of the entire product against a release/sprint at that exact point in time.",
					image: "tours-update-6.5.2-baselines.png",
					link: "https://www.inflectra.com/Ideas/Entry/1051.aspx"
				},
				{
					title: "Testing Settings",
					description: "Testing Settings are now configured at the product, not system level. There are now over a dozen options for tailoring how testing works in a product: disable certain execution statuses, only allow test set execution, always require an actual result and more.",
					image: "tours-update-6.5.2-testing-settings.png",
					link: "http://spiradoc.inflectra.com/Spira-Administration-Guide/Product-Planning/#testing-settings"
				},
				{
					title: "Testing Settings Admin",
					description: "NOTE: because testing settings are now at the product level: 1) any previous system wide customizations have been reset; and 2) editing these settings now requires product admin access to each product, not system admin access. Product admins will need to update their individual products to the required new settings.",
					isAdmin: true,
				},
				{
					title: "DevOps Toolchain",
					description: "Validation Master and SpiraPlan now provide better traceability between source code revisions, CI builds, DevOps pipelines, and Spira artifacts.",
					image: "tours-update-6.5.2-devops.png",
					link: "https://www.inflectra.com/Ideas/Entry/spira-652-visibility-user-stories-devops-toolchain-1050.aspx"
				},
				{
					title: "What's new in 6.5",
					linkToTour: "update-6.5.0",
				}
			]
		},
		{
			name: "update-6.6.0",
			title: "6.6: Planning, Kanban, and Baselining",
			description: "",
			image: "tours-update-6.6.0-icon.svg",
			steps: [
				{
					title: "Planning Board",
					description: "Planning and kanban boards have some great new features: group by components or epics when viewing releases / 'All Releases'; quickly expand/collapse all rows; improved collapsing of columns; colors to flag if a column is too full.",
					image: "tours-update-6.6.0-planningBoard.png",
				},
				{
					title: "Plan by Points",
					description: "You can now set a product to estimate releases and requirements only with points (not hours). Planning boards and details pages will fully switch out hours for points.",
					image: "tours-update-6.6.0-planByPoints.png",
				},
				{
					title: "WIP Limits",
					description: "Use WIP limits on the planning board to help manage your kanban flow. Dynamically set the number of cards allowed for each status. You can optionally use different values for releases and sprints.",
					image: "tours-update-6.6.0-wip.png",
				},
				{
					title: "Baselines",
					description: "View all baselines created across all releases in a product, and drill down into a baseline to review every artifact that changed during that baselines period of activity. These pages are available to all product admins.",
					image: "tours-update-6.6.0-baselines.png",
					link: "http://spiradoc.inflectra.com/Spira-Administration-Guide/Product-General-Settings/#baselines"
				},
				{
					title: "What's new in 6.5.2",
					linkToTour: "update-6.5.2",
				}
			]
		}
	];

</script>
<!-- End Global Navigation User Control -->
