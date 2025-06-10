<%@ Page 
    Language="C#" 
    MasterPageFile="~/MasterPages/ProjectGroupDashboard.Master" 
    AutoEventWireup="true" 
    CodeBehind="ProjectGroupHome3.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.ProjectGroupHome3" 
    Title="Untitled Page" 
    %>

<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectGroupOverview" Src="~/UserControls/WebParts/ProjectGroupHome/ProjectGroupOverview.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectList" Src="~/UserControls/WebParts/ProjectGroupHome/ProjectList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementsCoverage" Src="~/UserControls/WebParts/ProjectGroupHome/RequirementsCoverage.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestExecutionStatus" Src="~/UserControls/WebParts/ProjectGroupHome/TestExecutionStatus.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentAging" Src="~/UserControls/WebParts/ProjectGroupHome/IncidentAging.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TaskProgress" Src="~/UserControls/WebParts/ProjectGroupHome/TaskProgress.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OpenIssues" Src="~/UserControls/WebParts/ProjectGroupHome/OpenIssues.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OpenRisks" Src="~/UserControls/WebParts/ProjectGroupHome/OpenRisks.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RecentBuilds" Src="~/UserControls/WebParts/ProjectGroupHome/RecentBuilds.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="Schedule" Src="~/UserControls/WebParts/ProjectGroupHome/Schedule.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OverallCompletion" Src="~/UserControls/WebParts/ProjectGroupHome/OverallCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ProductCompletion" Src="~/UserControls/WebParts/ProjectGroupHome/ProductCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ProductRelativeSize" Src="~/UserControls/WebParts/ProjectGroupHome/ProductRelativeSize.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectTestSummary" Src="~/UserControls/WebParts/ProjectGroupHome/ProjectTestSummary.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
        <ContentTemplate>
            <tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="ProjectGroupHome3" OnInit="prtManager_Init"  OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
                <StaticConnections>
                </StaticConnections>
            </tstsc:WebPartManagerEx>
            <div class="py4 pt2-xs px3 flex justify-between">
			    <h2  class="pr3">
                    <tstsc:LabelEx CssClass="fs-h4-xs db-xs mb3-xs" AppendColon="true" id="lblProgram" runat="server" Text="<%$Resources:Fields,Program%>" />
			        <asp:label id="lblProjectGroupName" Runat="server"/>
			        <asp:Label CssClass="o-50 fw-b fs-h4-xs" id="lblProjectGroupId" Runat="server" />
                </h2>

                <div class="w9 w8-xs">
                    <div 
                        class="btn-group db" 
                        role="group"
                        >
                        <tstsc:HyperLinkEx 
                            aria-selected="false" runat="server" 
                            CssClass="btn btn-default br-pill w-33 fs-90-xs ov-hidden" 
                            ID="lnkGeneral" 
                            Text="<%$Resources:Buttons,General %>" 
                            />
                        <tstsc:HyperLinkEx 
                            aria-selected="true" runat="server" 
                            CssClass="btn btn-default br-pill active w-33 fs-90-xs ov-hidden" 
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

                    <div class="w-33 tc dib hidden-mobile"></div>
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
       			                    <tstuc:OpenRisks runat="server" ID="ucOpenRisks" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_TopOpenRisks %>" />		                
			                        <tstuc:RequirementsCoverage runat="server" ID="ucRequirementsCoverage" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_RequirementsCoverage %>" />		                
        			                <tstuc:TestExecutionStatus runat="server" ID="ucTestExecutionStatus" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_TestExecutionStatus %>" />
   			                        <tstuc:OverallCompletion runat="server" ID="ucOverallCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_OverallCompletion %>" />		                
   			                        <tstuc:ProductCompletion runat="server" ID="ucProductCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_ProductCompletion %>" />		                
   			                        <tstuc:ProductRelativeSize runat="server" ID="ucProductRelativeSize" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_ProductRelativeSize %>" />		                
   			                        <tstuc:ProjectTestSummary runat="server" ID="ucProjectTestSummary" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_ProductTestSummary %>" />		                
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

            <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />


             <%-- Top Zone --%>
             <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr2 pr3 relative">
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
   			                <tstuc:ProjectGroupOverview runat="server" ID="ucProjectGroupOverview" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_GroupOverview %>" />		                
			                <tstuc:RecentBuilds runat="server" ID="ucRecentBuilds" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RecentBuilds %>" />
			                <tstuc:TaskProgress runat="server" ID="ucTaskProgress" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_TaskProgress %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>

                <%-- Right Zone --%>
		        <div class ="w-50 w-100-sm pr3 pl2 pl3 relative">
 			        <tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
   			                <tstuc:ProjectList runat="server" ID="ucProjectList" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_ProjectList %>" />		                
      			            <tstuc:OpenIssues runat="server" ID="ucOpenIssues" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_TopOpenIssues %>" />		                
			                <tstuc:IncidentAging runat="server" ID="ucIncidentAging" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_IncidentAging %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

            <%-- Bottom Zone --%>
             <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr2 pr3-sm relative">
			        <tstsc:WebPartZoneEx ID="prtBottomZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_Bottom%>" SkinID="WebPartZone">
			            <ZoneTemplate>       
   			                <tstuc:Schedule 
                                   runat="server" 
                                   ID="ucSchedule" 
                                   MessageBoxId="lblMessage" 
                                   Title="<%$Resources:Main,ProjectGroupHome_Schedule %>" 
                                   />	                                     
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

	    </ContentTemplate>
	</tstsc:UpdatePanelEx>
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
