<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectList" Src="~/UserControls/WebParts/MyPage/ProjectList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SavedSearches" Src="~/UserControls/WebParts/MyPage/SavedSearches.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SavedReports" Src="~/UserControls/WebParts/MyPage/SavedReports.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssignedIncidents" Src="~/UserControls/WebParts/MyPage/AssignedIncidents.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DetectedIncidents" Src="~/UserControls/WebParts/MyPage/DetectedIncidents.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="PendingTestRuns" Src="~/UserControls/WebParts/MyPage/PendingTestRuns.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementsList" Src="~/UserControls/WebParts/MyPage/RequirementsList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TaskList" Src="~/UserControls/WebParts/MyPage/TaskList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestCaseList" Src="~/UserControls/WebParts/MyPage/TestCaseList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestSetList" Src="~/UserControls/WebParts/MyPage/TestSetList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="QuickLaunch" Src="~/UserControls/WebParts/MyPage/QuickLaunch.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SubscribedArtifacts" Src="~/UserControls/WebParts/MyPage/SubscribedArtifacts.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="NewsReader" Src="~/UserControls/WebParts/MyPage/NewsReader.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="Contacts" Src="~/UserControls/WebParts/MyPage/Contacts.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssignedDocuments" Src="~/UserControls/WebParts/MyPage/AssignedDocuments.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssignedRisks" Src="~/UserControls/WebParts/MyPage/AssignedRisks.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RecentArtifacts" Src="~/UserControls/WebParts/MyPage/RecentArtifacts.ascx" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<%@ Page Language="c#" CodeBehind="ProjectList.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.ProjectList" MasterPageFile="~/MasterPages/Dashboard.Master" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	<tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
		<ContentTemplate>
			<tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="MyPage" OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
				<StaticConnections>
					<asp:WebPartConnection ID="conPendingTestRunsTestSets" ConsumerID="ucPendingTestRuns" ConsumerConnectionPointID="ReloadTestSetsConsumer" ProviderID="ucTestSetList" ProviderConnectionPointID="ReloadableProvider" />
					<asp:WebPartConnection ID="conPendingTestRunsTestCases" ConsumerID="ucPendingTestRuns" ConsumerConnectionPointID="ReloadTestCasesConsumer" ProviderID="ucTestCaseList" ProviderConnectionPointID="ReloadableProvider" />
				</StaticConnections>
			</tstsc:WebPartManagerEx>
            <div class="py4 pt2-xs px3 flex justify-between items-center flex-wrap-xs">
			    <h2 class="w-100-xs">
					<asp:Label CssClass="fs-h4-xs" ID="Localize1" runat="server" Text="<%$ Resources:Main,ProjectList_MyPage %>" />
                    <tstsc:LabelEx CssClass="o-50 fw-b db-xs mt3-xs" ID="lblFullName" runat="server"/>
                </h2>
                <div class="flex">
                    <div 
                        class="btn-group radio-group priority1 mr4" 
                        id="divProjectFilter" 
                        role="group" 
                        runat="server" 
                        >
                        <asp:Label ID="lblAllProjects" runat="server" AssociatedControlID="radAllProjects" CssClass="btn btn-default br-pill">
						    <tstsc:RadioButtonEx ID="radAllProjects" runat="server" GroupName="ProjectFilter" AutoPostBack="true" />
						    <asp:Localize ID="Localize4" runat="server" Text="<%$ Resources:Main,ProjectList_AllProjects %>" />
                        </asp:Label>
                        <asp:Label ID="lblCurrentProjects" runat="server" AssociatedControlID="radCurrentProjects" CssClass="btn btn-default br-pill">
                            <tstsc:RadioButtonEx ID="radCurrentProjects" runat="server" GroupName="ProjectFilter" AutoPostBack="true" />
						    <asp:Localize ID="Localize5" runat="server" Text="<%$ Resources:Main,ProjectList_CurrentProject %>" />
                        </asp:Label>
                    </div>
                    <div 
                        class="btn-group hidden-mobile toolbar-sec self-center" 
                        role="group"
                        >
                        <tstsc:LinkButtonEx 
                            ID="btnBrowseView" 
                            runat="server" 
                            SkinID="ButtonPrimary"
                            ToolTip="<%$Resources:Main,ProjectHome_ReturntoNormalView%>"
                            >
                            <span 
                                class="fas fa-undo fa-fw"
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

            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

			<div class="w10 mx-auto">
                <div class="modal-content catalog">
                    <asp:CatalogZone ID="prtCatalogZone" runat="server" SkinID="CatalogZone" HeaderText="<%$Resources:ServerControls,CatalogZone_AddRemoveItems%>" InstructionText="<%$Resources:ServerControls,CatalogZone_InstructionText%>" AddVerb-Text="<%$Resources:Buttons,Add%>" CloseVerb-Text="<%$Resources:Buttons,Close%>" HeaderCloseVerb-Text="<%$Resources:Buttons,Close%>" AddVerb-Description="<%$Resources:ServerControls,CatalogZone_AddVerbDescription%>" SelectTargetZoneText="<%$Resources:ServerControls,CatalogZone_SelectTargetZoneText%>" CloseVerb-Description="<%$Resources:ServerControls,CatalogZone_CloseVerbDescription%>" HeaderCloseVerb-Description="<%$Resources:ServerControls,CatalogZone_CloseVerbDescription%>">
					    <ZoneTemplate>
						    <asp:PageCatalogPart ID="prtPageCatalog" runat="server" Title="<%$ Resources:Dialogs,WebParts_ClosedWidgets %>" />
						    <asp:DeclarativeCatalogPart ID="prtDeclarativeCatalog" runat="server" Title="<%$ Resources:Dialogs,WebParts_AvailableWidgets %>">
							    <WebPartsTemplate>
								    <tstuc:SavedReports runat="server" ID="ucSavedReports" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MySavedReports %>" />
								    <tstuc:SubscribedArtifacts runat="server" ID="ucSubscribedArtifacts" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MySubscribedArtifacts %>" />
							    </WebPartsTemplate>
						    </asp:DeclarativeCatalogPart>
					    </ZoneTemplate>
				    </asp:CatalogZone>
                </div>
            </div>
            <div class="w10 mx-auto">
                <div class="modal-content catalog">
                    <asp:EditorZone ID="prtEditorZone" runat="server" SkinID="EditorZone" HeaderText="<%$Resources:ServerControls,EditorZone_DashboardSettings%>" InstructionText="<%$Resources:ServerControls,EditorZone_InstructionText%>" HeaderCloseVerb-Text="<%$Resources:Buttons,Close%>" ApplyVerb-Text="<%$Resources:Buttons,Apply%>" CancelVerb-Text="<%$Resources:Buttons,Cancel%>" OKVerb-Text="<%$Resources:Buttons,OK%>" CancelVerb-Description="<%$Resources:ServerControls,EditorZone_CancelVerbDescription%>" OKVerb-Description="<%$Resources:ServerControls,EditorZone_OKVerbDescription%>" ApplyVerb-Description="<%$Resources:ServerControls,EditorZone_ApplyVerbDescription%>" HeaderCloseVerb-Description="<%$Resources:ServerControls,EditorZone_CloseVerbDescription%>">
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
							<tstuc:NewsReader runat="server" ID="ucNewsReader" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyNewsFeeds %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

            <div class ="flex flex-wrap pb4">
                <%-- Left Zone --%>
		        <div class ="w-50 w-100-sm pl3 pr2 pr3-sm relative ov-hidden">
					<tstsc:WebPartZoneEx ID="prtLeftZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_LeftSide%>" SkinID="WebPartZone">
						<ZoneTemplate>
							<tstuc:ProjectList runat="server" ID="ucProjectList" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_RecentProducts %>" />
							<tstuc:RecentArtifacts runat="server" ID="ucRecentArtifacts" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_RecentArtifacts %>" />
							<tstuc:SavedSearches runat="server" ID="ucSavedSearches" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MySavedSearches %>" />
							<tstuc:RequirementsList runat="server" ID="ucRequirementsList" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedRequirements %>" />
							<tstuc:TestSetList runat="server" ID="ucTestSetList" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedTestSets %>" />
							<tstuc:TestCaseList runat="server" ID="ucTestCaseList" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedTestCases %>" />
							<tstuc:DetectedIncidents runat="server" ID="ucDetectedIncidents" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyDetectedIncidents %>" />
							<tstuc:PendingTestRuns runat="server" ID="ucPendingTestRuns" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyPendingTestRuns %>" />
						</ZoneTemplate>
					</tstsc:WebPartZoneEx>
				</div>

                <%-- Right Zone --%>
		        <div class ="w-50 w-100-sm pr3 pl2 pl3-sm relative ov-hidden">
					<tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
						<ZoneTemplate>
							<tstuc:QuickLaunch runat="server" ID="ucQuickLaunch" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_QuickLaunch %>" />
							<tstuc:Contacts runat="server" ID="ucContacts" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_Contacts %>" />
							<tstuc:AssignedRisks runat="server" ID="ucAssignedRisks" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedRisks %>" />
							<tstuc:AssignedIncidents runat="server" ID="ucAssignedIncidents" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedIncidents %>" />
							<tstuc:TaskList runat="server" ID="ucTaskList" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedTasks %>" />
                            <tstuc:AssignedDocuments runat="server" ID="ucAssignedDocuments" MessageBoxId="lblMessage" Title="<%$ Resources:Main,ProjectList_MyAssignedDocuments %>" />
						</ZoneTemplate>
					</tstsc:WebPartZoneEx>
		        </div>
	        </div>

            <%-- Bottom Zone --%>
             <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr2 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtBottomZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_Bottom%>" SkinID="WebPartZone">
			            <ZoneTemplate>       
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>
		</ContentTemplate>
	</tstsc:UpdatePanelEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var g_prtManager_ajaxWebParts_clientId = '<%=ajaxWebParts.ClientID%>';
    </script>
</asp:Content>
