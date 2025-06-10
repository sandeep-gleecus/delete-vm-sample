<%@ Page language="c#" Codebehind="Reports.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Reports" MasterPageFile="~/MasterPages/Dashboard.Master" %>
<%@ Register TagPrefix="tstuc" TagName="SavedReports" Src="~/UserControls/WebParts/Reports/SavedReports.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="CreateNewReport" Src="~/UserControls/WebParts/Reports/CreateNewReport.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DateRangeGraphs" Src="~/UserControls/WebParts/Reports/DateRangeGraphs.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SummaryGraphs" Src="~/UserControls/WebParts/Reports/SummaryGraphs.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SnapshotGraphs" Src="~/UserControls/WebParts/Reports/SnapshotGraphs.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="CustomGraphs" Src="~/UserControls/WebParts/Reports/CustomGraphs.ascx" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
        <ContentTemplate>
            <tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="Reports" OnInit="prtManager_Init"  OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
                <StaticConnections>
                </StaticConnections>
            </tstsc:WebPartManagerEx>
			<div class="pt4 px3 flex flex-wrap justify-between">
				<div class="pr3">
					<h2>
						<tstsc:LabelEx ID="lblProjectName" runat="server" />
						<tstsc:LabelEx
							CssClass="o-50 fw-b"
							ID="lblReportsCenter"
							runat="server"
							Text="<%$Resources:Main,Reports_ReportsCenter %>" />
					</h2>

					<div class="form-group">
						<tstsc:LabelEx
							AppendColon="true"
							AssociatedControlID="ddlSelectRelease"
							CssClass="v-top pr3"
							ID="ddlSelectReleaseLabel"
							runat="server"
							Text="<%$Resources:Main,Global_Displaying %>" />


						<tstsc:UnityDropDownHierarchy
							ID="ddlSelectRelease"
							runat="server"
                            DisabledCssClass="u-dropdown disabled"
                            Enabled="false"
                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
							NoValueItem="true"
                            NavigateToText ="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
							NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>"
							ActiveItemField="IsActive"
							AutoPostBack="true"
							DataTextField="FullName"
							DataValueField="ReleaseId" />
					</div>
				</div>

				<div class="btn-group hidden-mobile fr toolbar-sec" role="group">
					<tstsc:LinkButtonEx
						ID="btnBrowseView"
						runat="server"
						SkinID="ButtonPrimary"
						ToolTip="<%$Resources:Main,ProjectHome_ReturntoNormalView%>">
                        <span 
                            class="fas fa-check fa-fw"
                            runat="server" 
                            >
                        </span>
					</tstsc:LinkButtonEx>
					<tstsc:LinkButtonEx
						ID="btnDesignView"
						runat="server"
						ToolTip="<%$Resources:Main,ProjectHome_ModifyLayoutSettings%>">
                        <span 
                            class="fas fa-cog fa-fw"
                            runat="server" 
                            >
                        </span>
					</tstsc:LinkButtonEx>
					<tstsc:LinkButtonEx
						ID="btnCustomize"
						runat="server"
						ToolTip="<%$Resources:Main,ProjectHome_AddItems%>">
                        <span 
                            class="fas fa-plus fa-fw"
                            runat="server" 
                            >
                        </span>
					</tstsc:LinkButtonEx>
					<a class="btn btn-default" id="dashboard_export_as_image" data-action="export_as_image">
						<span
							class="far fa-image fa-fw"
							runat="server"></span>
					</a>
				</div>
			</div>
            
       	    
            <!--Clear Fix-->
            <div class="cb"></div>

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
                                <tstuc:CustomGraphs runat="server" ID="ucCustomGraphs" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_CustomGraphs %>" />
   			                    <tstuc:DateRangeGraphs runat="server" ID="ucIncidentDateRangeGraphs" MessageBoxId="lblMessage" ArtifactTypeId="Incident" Title="<%$Resources:Main,Reports_IncidentDateRangeGraphs %>" />
   			                    <tstuc:DateRangeGraphs runat="server" ID="ucTestRunDateRangeGraphs" MessageBoxId="lblMessage" ArtifactTypeId="TestRun" Title="<%$Resources:Main,Reports_TestingDateRangeGraphs %>" />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucRequirementSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_RequirementSummaryGraph %>" ArtifactTypeId="Requirement" GraphId="RequirementSummary" />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucTestCaseSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TestCaseSummaryGraph %>" ArtifactTypeId="TestCase" GraphId="TestCaseSummary"  />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucTestRunSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TestRunSummaryGraph %>" ArtifactTypeId="TestRun" GraphId="TestRunSummary"  />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucTestSetSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TestSetSummaryGraph %>" ArtifactTypeId="TestSet" GraphId="TestSetSummary"  />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucTaskSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TaskSummaryGraph %>" ArtifactTypeId="Task" GraphId="TaskSummary"  />
   			                    <tstuc:SummaryGraphs runat="server" ID="ucIncidentSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_IncidentSummaryGraph %>" ArtifactTypeId="Incident" GraphId="IncidentSummary"  />
   			                    <tstuc:SnapshotGraphs runat="server" ID="ucRequirementReports" MessageBoxId="lblMessage" ArtifactTypeId="Requirement" Title="<%$Resources:Main,Reports_RequirementGraphs %>" />
   			                    <tstuc:SnapshotGraphs runat="server" ID="ucIncidentReports" MessageBoxId="lblMessage" ArtifactTypeId="Incident" Title="<%$Resources:Main,Reports_IncidentGraphs %>" />
   			                    <tstuc:SnapshotGraphs runat="server" ID="ucTaskReports" MessageBoxId="lblMessage" ArtifactTypeId="Task" Title="<%$Resources:Main,Reports_TaskGraphs %>" />
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

	        <div class ="flex flex-wrap">
		        <div class ="w-25 w-100-sm pl3 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtSideBar" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_SideBar%>" SkinID="WebPartZone">
			            <ZoneTemplate>
   			                <tstuc:SavedReports runat="server" ID="ucProjectOverview" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_SavedReports %>" />		                
   			                <tstuc:CreateNewReport runat="server" ID="ucOpenIssues" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_CreateNewReport %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
                <div class="w-75 w-100-sm">
                    <div class="flex flex-wrap">
                        <div class="w-100 px3 relative">
 			                <tstsc:WebPartZoneEx ID="prtTopZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_Top%>" SkinID="WebPartZone">
			                    <ZoneTemplate>
   			                        <tstuc:DateRangeGraphs runat="server" ID="ucIncidentDateRangeGraphs" ArtifactTypeId="Incident" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_IncidentDateRangeGraphs %>" />
   			                        <tstuc:DateRangeGraphs runat="server" ID="ucTestRunDateRangeGraphs" ArtifactTypeId="TestRun" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TestingDateRangeGraphs %>" />
			                    </ZoneTemplate>
			                </tstsc:WebPartZoneEx>
		                </div>
                        <div class="w-50 w-100-sm pl3 pr2 relative">
 			                <tstsc:WebPartZoneEx ID="prtLeftZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_LeftSide%>" SkinID="WebPartZone">
			                    <ZoneTemplate>
   			                        <tstuc:SummaryGraphs runat="server" ID="ucRequirementSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_RequirementSummaryGraph %>" ArtifactTypeId="Requirement" GraphId="RequirementSummary" />
   			                        <tstuc:SnapshotGraphs runat="server" ID="ucIncidentReports" MessageBoxId="lblMessage" ArtifactTypeId="Incident" Title="<%$Resources:Main,Reports_IncidentGraphs %>" />
			                    </ZoneTemplate>
			                </tstsc:WebPartZoneEx>
		                </div>
		                <div class="w-50 w-100-sm pl2 pr3 relative">
 			                <tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
			                    <ZoneTemplate>
   			                        <tstuc:SummaryGraphs runat="server" ID="ucTestCaseSummaryGraph" MessageBoxId="lblMessage" Title="<%$Resources:Main,Reports_TestCaseSummaryGraph %>" ArtifactTypeId="TestCase" GraphId="TestCaseSummary"  />
   			                        <tstuc:SnapshotGraphs runat="server" ID="ucTaskReports" MessageBoxId="lblMessage" ArtifactTypeId="Task" Title="<%$Resources:Main,Reports_TaskGraphs %>" />
			                    </ZoneTemplate>
			                </tstsc:WebPartZoneEx>
		                </div>
                    </div>
                </div>
	        </div>
	    </ContentTemplate>
	</tstsc:UpdatePanelEx>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Scripts>
            <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.Dashboard.js" Assembly="Web" />
        </Scripts>
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/GraphingService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var g_prtManager_ajaxWebParts_clientId = '<%=ajaxWebParts.ClientID%>';
    </script>
</asp:Content>
