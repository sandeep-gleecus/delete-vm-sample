<%@ Page language="c#" Codebehind="ProjectHome.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.ProjectHome" MasterPageFile="~/MasterPages/ProjectDashboard.Master" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>

<%@ Register TagPrefix="tstuc" TagName="IncidentSummary" Src="~/UserControls/WebParts/ProjectHome/IncidentSummary.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OpenIssues" Src="~/UserControls/WebParts/ProjectHome/OpenIssues.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OpenRisks" Src="~/UserControls/WebParts/ProjectHome/OpenRisks.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RiskSummary" Src="~/UserControls/WebParts/ProjectHome/RiskSummary.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectOverview" Src="~/UserControls/WebParts/ProjectHome/ProjectOverview.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ReleaseTaskProgress" Src="~/UserControls/WebParts/ProjectHome/ReleaseTaskProgress.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ReleaseTestSummary" Src="~/UserControls/WebParts/ProjectHome/ReleaseTestSummary.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementIncidentCount" Src="~/UserControls/WebParts/ProjectHome/RequirementIncidentCount.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementsCoverageAll" Src="~/UserControls/WebParts/ProjectHome/RequirementsCoverageAll.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementsCoverageNew" Src="~/UserControls/WebParts/ProjectHome/RequirementsCoverageNew.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementsSummary" Src="~/UserControls/WebParts/ProjectHome/RequirementsSummary.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TasksLateFinishing" Src="~/UserControls/WebParts/ProjectHome/TasksLateFinishing.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TasksLateStarting" Src="~/UserControls/WebParts/ProjectHome/TasksLateStarting.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestExecutionStatus" Src="~/UserControls/WebParts/ProjectHome/TestExecutionStatus.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestSetStatus" Src="~/UserControls/WebParts/ProjectHome/TestSetStatus.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentAging" Src="~/UserControls/WebParts/ProjectHome/IncidentAging.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentOpenCount" Src="~/UserControls/WebParts/ProjectHome/IncidentOpenCount.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentTestCoverage" Src="~/UserControls/WebParts/ProjectHome/IncidentTestCoverage.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TaskGraphs" Src="~/UserControls/WebParts/ProjectHome/TaskGraphs.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementGraphs" Src="~/UserControls/WebParts/ProjectHome/RequirementGraphs.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DocumentTags" Src="~/UserControls/WebParts/ProjectHome/DocumentTags.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RecentBuilds" Src="~/UserControls/WebParts/ProjectHome/RecentBuilds.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SharedSearches" Src="~/UserControls/WebParts/ProjectHome/SharedSearches.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ActivityFeed" Src="~/UserControls/WebParts/ProjectHome/ActivityFeed.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="PendingTestRuns" Src="~/UserControls/WebParts/ProjectHome/PendingTestRuns.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestRunProgress" Src="~/UserControls/WebParts/ProjectHome/TestRunProgress.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SourceCodeCommits" Src="~/UserControls/WebParts/ProjectHome/SourceCodeCommits.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestCasesPerStatusAndDay" Src="~/UserControls/WebParts/ProjectHome/TestCasesPerStatusAndDay.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="Schedule" Src="~/UserControls/WebParts/ProjectHome/Schedule.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OverallCompletion" Src="~/UserControls/WebParts/ProjectHome/OverallCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ReleaseCompletion" Src="~/UserControls/WebParts/ProjectHome/ReleaseCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ReleaseRelativeSize" Src="~/UserControls/WebParts/ProjectHome/ReleaseRelativeSize.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestPreparationStatus" Src="~/UserControls/WebParts/ProjectHome/TestPreparationStatus.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
        <ContentTemplate>
            <tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="ProjectHome" OnInit="prtManager_Init"  OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
                <StaticConnections>
                </StaticConnections>
            </tstsc:WebPartManagerEx>
            <div class="py4 px3 flex justify-between flex-wrap-xs title">
                <div class="pr3">
			        <h2>
                        <asp:label id="lblProjectName" Runat="server"/>			            
                        <asp:Label CssClass="o-50 fw-b fs-h4-xs title-id" id="lblProjectId" Runat="server"/>
                    </h2>
                    <div class="form-group">
                        <label class="v-top pr3">
                            <asp:Localize 
                                runat="server" 
                                Text="<%$Resources:Main,Global_Displaying %>"/>:</label>
                        <tstsc:UnityDropDownHierarchy 
                            ID="ddlSelectRelease" 
                            Runat="server" 
                            NoValueItem="true"
                            NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" 
                            ActiveItemField="IsActive"
                            NavigateToText ="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
                            AutoPostBack="true" 
                            DataTextField="FullName" 
                            SkinID="ReleaseDropDownListFarRight"
                            DataValueField="ReleaseId" />
                    </div>
                </div>
                <div>
                    <div class="w9 w8-xs">
                        <div 
                            class="btn-group db" 
                            role="group"
                            >
                            <tstsc:HyperLinkEx 
                                aria-selected="true" runat="server" 
                                CssClass="btn btn-default br-pill active w-33 fs-90-xs ov-hidden" 
                                ID="lnkGeneral" 
                                Text="<%$Resources:Buttons,General %>" 
                                />
                            <tstsc:HyperLinkEx 
                                aria-selected="false" runat="server" 
                                CssClass="btn btn-default br-pill w-33 fs-90-xs ov-hidden" 
                                ID="lnkDevelopment" 
                                Text="<%$Resources:Buttons,Development %>" 
                                />
                            <tstsc:HyperLinkEx 
                                aria-selected="false" 
                                CssClass="btn btn-default br-pill w-33 fs-90-xs ov-hidden" 
                                ID="lnkTesting" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Testing %>" 
                                />
                        </div>
                        <div class="w-33 dib tc toolbar-sec hidden-mobile">
                            <div 
                                class="btn-group" 
                                role="group"
                                >
                                <tstsc:LinkButtonEx 
                                    ID="btnBrowseView" 
                                    runat="server" 
                                    SkinID="ButtonPrimary"
                                    ToolTip="<%$Resources:Main,ProjectHome_ReturntoNormalView%>"
                                    >
                                    <span 
                                        class="fas fa-check fa-fw"
                                        runat="server" 
                                        >
                                    </span>
                                </tstsc:LinkButtonEx>
                                <tstsc:LinkButtonEx 
                                    ID="btnDesignView" 
                                    runat="server"
                                    ToolTip="<%$Resources:Main,ProjectHome_ModifyLayoutSettings%>"
                                    >
                                    <span 
                                        class="fas fa-cog fa-fw"
                                        runat="server" 
                                        >
                                    </span>
                                </tstsc:LinkButtonEx>
		                        <tstsc:LinkButtonEx 
                                    ID="btnCustomize" 
                                    runat="server"
                                    ToolTip="<%$Resources:Main,ProjectHome_AddItems%>"
                                    >
                                    <span 
                                        class="fas fa-plus fa-fw"
                                        runat="server" 
                                        >
                                    </span>
		                        </tstsc:LinkButtonEx>
                                <a class="btn btn-default" id="dashboard_export_as_image" data-action="export_as_image">
                                    <span 
                                        class="far fa-image fa-fw"
                                        runat="server" 
                                        >
                                    </span>
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
		        
            <div class="w10 mx-auto">
                <div class="modal-content catalog">
                    <asp:CatalogZone ID="prtCatalogZone" runat="server" SkinID="CatalogZone" HeaderText="<%$Resources:ServerControls,CatalogZone_AddRemoveItems%>"
                        InstructionText="<%$Resources:ServerControls,CatalogZone_InstructionText%>" AddVerb-Text="<%$Resources:Buttons,Add%>" CloseVerb-Text="<%$Resources:Buttons,Close%>"
                        HeaderCloseVerb-Text="<%$Resources:Buttons,Close%>" AddVerb-Description="<%$Resources:ServerControls,CatalogZone_AddVerbDescription%>" SelectTargetZoneText="<%$Resources:ServerControls,CatalogZone_SelectTargetZoneText%>"
                        CloseVerb-Description="<%$Resources:ServerControls,CatalogZone_CloseVerbDescription%>" HeaderCloseVerb-Description="<%$Resources:ServerControls,CatalogZone_CloseVerbDescription%>">
			            <ZoneTemplate>
			                <asp:PageCatalogPart ID="prtPageCatalog" runat="server" Title="<%$ Resources:Dialogs,WebParts_ClosedWidgets %>" />
    			            <asp:DeclarativeCatalogPart ID="prtDeclarativeCatalog" runat="server" Title="<%$ Resources:Dialogs,WebParts_AvailableWidgets %>">
                                <WebPartsTemplate>
		                            <tstuc:RequirementsCoverageAll runat="server" ID="ucRequirementsCoverageAll" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_RequirementsRegressionCoverage %>" />
                                    <tstuc:TestSetStatus runat="server" ID="ucTestSetStatus" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_TestSetStatus%>" />
                                    <tstuc:IncidentAging runat="server" ID="ucIncidentAging" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_IncidentAging%>" />
                                    <tstuc:IncidentTestCoverage runat="server" ID="ucIncidentTestCoverage" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_IncidentTestCoverage%>" />
                                    <tstuc:DocumentTags runat="server" ID="ucDocumentTags" MessageBoxId="lblMessage" Title="<%$ Resources:Main,Documents_TagCloud%>" />
                                    <tstuc:RecentBuilds runat="server" ID="ucRecentBuilds" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_RecentBuilds%>" />
   			                        <tstuc:TasksLateStarting runat="server" ID="ucTasksLateStarting" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_LateStartingTasks %>" />		                
   			                        <tstuc:PendingTestRuns runat="server" ID="ucPendingTestRuns" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_PendingTestRuns %>" />		                
   			                        <tstuc:SourceCodeCommits runat="server" ID="ucSourceCodeCommits" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectHome_SourceCodeCommits%>" />
   			                        <tstuc:TestRunProgress runat="server" ID="ucTestRunProgress" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_TestRunProgress %>" />		                
                                    <tstuc:TestCasesPerStatusAndDay runat="server" ID="ucTestCasesPerStatusAndDay" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_TestCasesPerStatusAndDay %>" />		                
   			                        <tstuc:TestExecutionStatus runat="server" ID="ucTestExecutionStatus" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_TestExecutionStatus %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />		                
   			                        <tstuc:ReleaseTestSummary runat="server" ID="ucReleaseTestSummary" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ReleaseTestSummary %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />		                
   			                        <tstuc:ReleaseTaskProgress runat="server" ID="ucReleaseTaskProgress" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ReleaseTaskProgress %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
                                    <tstuc:TestPreparationStatus runat="server" ID="ucTestPreparationStatus" MessageBoxId="lblMessage" Title="Test Preparation Status" />
                                </WebPartsTemplate>
                            </asp:DeclarativeCatalogPart>
			            </ZoneTemplate>
			        </asp:CatalogZone>
                </div>
            </div>
            <div class="w10 mx-auto">
                <div class="modal-content catalog">
                    <asp:EditorZone ID="prtEditorZone" runat="server" SkinID="EditorZone" HeaderText="<%$Resources:ServerControls,EditorZone_DashboardSettings%>"
                        InstructionText="<%$Resources:ServerControls,EditorZone_InstructionText%>" HeaderCloseVerb-Text="<%$Resources:Buttons,Close%>" ApplyVerb-Text="<%$Resources:Buttons,Apply%>"
                        CancelVerb-Text="<%$Resources:Buttons,Cancel%>" OKVerb-Text="<%$Resources:Buttons,OK%>"
                        CancelVerb-Description="<%$Resources:ServerControls,EditorZone_CancelVerbDescription%>"
                        OKVerb-Description="<%$Resources:ServerControls,EditorZone_OKVerbDescription%>"
                        ApplyVerb-Description="<%$Resources:ServerControls,EditorZone_ApplyVerbDescription%>"
                        HeaderCloseVerb-Description="<%$Resources:ServerControls,EditorZone_CloseVerbDescription%>">
			            <ZoneTemplate>
                            <asp:PropertyGridEditorPart runat="server" ID="prtPropertyGridEditor" Title="<%$ Resources:Dialogs,WebParts_EditWidgetSettings %>" />
			            </ZoneTemplate>
			        </asp:EditorZone>
                </div>
            </div>


            <%-- Top Zone --%>
            <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr2 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtTopZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_Top%>" SkinID="WebPartZone">
			            <ZoneTemplate>
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>


            <div class ="flex flex-wrap">
                <%-- Left Zone --%>
		        <div class ="w-50 w-100-sm pl3 pr2 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtLeftZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_LeftSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
   			                <tstuc:ProjectOverview runat="server" ID="ucProjectOverview" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ProjectOverview %>" />		                
   			                <tstuc:ActivityFeed runat="server" ID="ucActivityFeed" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ActivityStream %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />		                
   			                <tstuc:OverallCompletion runat="server" ID="ucOverallCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_OverallCompletion %>" />		                
   			                <tstuc:SharedSearches runat="server" ID="ucSharedSearches" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_SharedSearches %>" />		                
   			                <tstuc:RequirementsSummary runat="server" ID="ucRequirementsSummary" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RequirementsSummary %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />		                
   			                <tstuc:RequirementsCoverageNew runat="server" ID="ucRequirementsCoverageNew" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RequirementsCoverage %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />		                
   			                <tstuc:RequirementGraphs runat="server" ID="ucRequirementGraphs" MessageBoxId="lblMessage" ArtifactTypeId="Requirement" Title="<%$Resources:Main,Reports_RequirementGraphs %>" />
   			                <tstuc:TasksLateFinishing runat="server" ID="ucTasksLateFinishing" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_LateFinishingTasks %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />		                
   			                <tstuc:TaskGraphs runat="server" ID="ucTaskReports" MessageBoxId="lblMessage" ArtifactTypeId="Task" Title="<%$Resources:Main,Reports_TaskGraphs %>"  Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>

                <%-- Right Zone --%>
		        <div class ="w-50 w-100-sm pr3 pl2 pl3-sm relative">
 			        <tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
   			                <tstuc:OpenIssues runat="server" ID="ucOpenIssues" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_TopOpenIssues %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />		                
   			                <tstuc:RiskSummary runat="server" ID="ucRiskSummary" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RiskSummary %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />		                
   			                <tstuc:OpenRisks runat="server" ID="ucOpenRisks" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_TopOpenRisks %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />		                
   			                <tstuc:ReleaseCompletion runat="server" ID="ucReleaseCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ReleaseCompletion %>" />		                
   			                <tstuc:ReleaseRelativeSize runat="server" ID="ucReleaseRelativeSize" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ReleaseRelativeSize %>" />		                
   			                <tstuc:IncidentSummary runat="server" ID="ucIncidentSummary" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_IncidentSummary %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />		                
                            <tstuc:IncidentOpenCount runat="server" ID="ucIncidentOpenCount" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_IncidentOpenCount %>" />
   			                <tstuc:RequirementIncidentCount runat="server" ID="ucRequirementIncidentCount" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RequirementIncidentCount %>" />		                
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

            <%-- Bottom Zone --%>
             <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr2 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtBottomZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_Bottom%>" SkinID="WebPartZone">
			            <ZoneTemplate>       
   			                <tstuc:Schedule runat="server" ID="ucSchedule" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_Schedule %>" />                                     
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

	    </ContentTemplate>
	</tstsc:UpdatePanelEx>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/GraphingService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>
    <script type="text/javascript">
		//function to shorten long urls on mobile devices
		$(document).ready(function () {
			if ($(window).width() < 768) {
				$(".url-to-shorten").each(function (i) {
					var str = $(this).text();
					var newStr = '';
					if (str.substring(0, 7) == 'http://') {
						newStr = str.substring(7, 27) + '...';
					} else {
						newStr = str.substring(0, 20) + '...';
					};
					$(this).html(newStr);
				});
			}
		});
		var g_prtManager_ajaxWebParts_clientId = '<%=ajaxWebParts.ClientID%>';
	</script>
</asp:Content>
