<%@ Page 
    Language="C#" 
    MasterPageFile="~/MasterPages/Dashboard.Master" 
    AutoEventWireup="true" 
    CodeBehind="Default.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.Enterprise._Default" 
    Title="" 
    %>

<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>

<%@ Register TagPrefix="tstuc" TagName="OverallCompletion" Src="~/UserControls/WebParts/EnterpriseHome/OverallCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="PortfolioCompletion" Src="~/UserControls/WebParts/EnterpriseHome/PortfolioCompletion.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="PortfolioRelativeSize" Src="~/UserControls/WebParts/EnterpriseHome/PortfolioRelativeSize.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="Schedule" Src="~/UserControls/WebParts/EnterpriseHome/Schedule.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="OpenRisks" Src="~/UserControls/WebParts/EnterpriseHome/OpenRisks.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RecentBuilds" Src="~/UserControls/WebParts/EnterpriseHome/RecentBuilds.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <script type="text/javascript">
        //This is the collection of items that need to be called when data is returned
        var g_enterprise_workspaceLoadCallbacks = new Array();
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
        <ContentTemplate>
            <tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="EnterpriseHome" OnInit="prtManager_Init"  OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
                <StaticConnections>
                </StaticConnections>
            </tstsc:WebPartManagerEx>

            <div class="py4 pt2-xs px3 flex justify-between">
		        <h2  class="pr3">
                    <tstsc:LabelEx CssClass="fs-h4-xs db-xs mb3-xs" id="lblEnterprise" runat="server" Text="<%$Resources:Fields,Enterprise%>" />
                </h2>
                <div class="btn-group hidden-mobile fr toolbar-sec" role="group">
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
                </div>   
            </div>    

            <!-- Catalog Zone -->
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
                                </WebPartsTemplate>
                            </asp:DeclarativeCatalogPart>
			            </ZoneTemplate>
			        </asp:CatalogZone>
                </div>
            </div>

            <!-- Editor Zone -->
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
   			                <tstuc:OverallCompletion runat="server" ID="ucOverallCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,PortfolioHome_OverallCompletion %>" />		                
       			            <tstuc:OpenRisks runat="server" ID="ucOpenRisks" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectGroupHome_TopOpenRisks %>" />		                
			                <tstuc:RecentBuilds runat="server" ID="ucRecentBuilds" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_RecentBuilds %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>

                <%-- Right Zone --%>
		        <div class ="w-50 w-100-sm pr3 pl2 pl3-sm relative">
 			        <tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
   			                <tstuc:PortfolioCompletion runat="server" ID="ucPortfolioCompletion" MessageBoxId="lblMessage" Title="<%$Resources:Main,EnterpriseHome_PortfolioCompletion %>" />		                
   			                <tstuc:PortfolioRelativeSize runat="server" ID="ucPortfolioRelativeSize" MessageBoxId="lblMessage" Title="<%$Resources:Main,EnterpriseHome_PortfolioRelativeSize %>" />		                
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>

            <%-- Bottom Zone --%>
            <div class ="flex flex-wrap">
		        <div class ="w-100 w-100-sm pl3 pr3 relative">
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
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server"> 
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/EnterpriseService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>  

    <script type="text/javascript">
        var g_prtManager_ajaxWebParts_clientId = '<%=ajaxWebParts.ClientID%>';

        function enterpriseDashboard_loadWorkspaceData()
        {
            //load the dashboard workspace overall data
            Inflectra.SpiraTest.Web.Services.Ajax.EnterpriseService.Workspace_RetrieveCompletionData(
                0,
                function (workspaceData) {

                    //Add the workspace type to the data
                    workspaceData.workspaceType = SpiraContext.WorkspaceEnums.enterprise;
                    workspaceData.workspaceEnums = SpiraContext.WorkspaceEnums;

                    //Load each of the widgets on the page in turn that need this data
                    for (var i = 0; i < g_enterprise_workspaceLoadCallbacks.length; i++)
                    {
                        g_enterprise_workspaceLoadCallbacks[i](workspaceData);
                    }
                },
                function (ex) {
                    //Display the error message
                    globalFunctions.display_error(null, ex);
                }
            );
        }
        $(document).ready(function ()
        {
            //register a handler so that the workspace data is reloaded if the update panel refreshes
            var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
            pageRequestManager.add_endRequest(enterpriseDashboard_loadWorkspaceData);

            //Load the intial set of workspace data
            enterpriseDashboard_loadWorkspaceData();
        });
    </script>
</asp:Content>
