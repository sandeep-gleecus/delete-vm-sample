<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Page Language="c#" ValidateRequest="false" CodeBehind="Default.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration._Default" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstuc" TagName="SystemInfo" Src="~/UserControls/WebParts/Administration/SystemInfo.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="EventList" Src="~/UserControls/WebParts/Administration/EventList.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DataSynchronization" Src="~/UserControls/WebParts/Administration/DataSynchronization.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="LockedOutUsers" Src="~/UserControls/WebParts/Administration/LockedOutUsers.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="UserRequests" Src="~/UserControls/WebParts/Administration/UserRequests.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DashboardStylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
        <ContentTemplate>
            <tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="Administration" OnInit="prtManager_Init"  OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
                <StaticConnections>
                </StaticConnections>
            </tstsc:WebPartManagerEx>
            <div class="py4 px3 flex justify-between items-center">
                <div>
			        <h1>
				        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_Home_Title %>" />
			        </h1>
				    <div class="alert alert-warning text-center pa4 my3" id="divNotActivated" runat="server">
                        <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <div class="db pull-left">
                            <tstsc:ImageEx runat="server" ID="imgTaraVaultLogo" class="logo-product logo-v-8"/>
                        </div>
					    <h4 class="display-inline-block">
                            <asp:Label runat="server" ID="lblActivate" Text="<%$ Resources:Messages,Admin_TaraVault_Active %>" />
					    </h4>
                        <br />
				        <tstsc:ButtonEX ID="btnTaraVaultActivate" SkinID="ButtonPrimary" runat="server" Text="<%$ Resources:Buttons,ActivateTara %>" />
				    </div>
			        <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
			        <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage" DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
                </div>

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

            <tstsc:MessageBox id="MessageBox1" Runat="server" SkinID="MessageBox" />

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

	        <div class ="flex flex-wrap pb4">
		        <div class ="w-50 w-100-sm pl3 pr2 pr3-sm relative">
 			        <tstsc:WebPartZoneEx ID="prtLeftZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_LeftSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
                            <tstuc:SystemInfo runat="server" ID="ucSystemInfo" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_SystemInfo %>" />
                            <tstuc:UserRequests runat="server" ID="ucUserRequests" MessageBoxId="lblMessage" Title="<%$Resources:Main,UserRequests_Title %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />
                            <tstuc:LockedOutUsers runat="server" ID="ucLockedOutUsers" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_LockedOutUsers %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />

			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
		        <div class ="w-50 w-100-sm pr3 pl2 pl3-sm relative">
 			        <tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
			            <ZoneTemplate>
                            <tstuc:EventList runat="server" ID="ucEventList" MessageBoxId="lblMessage" Title="<%$Resources:Main,EventLog_Title %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />
                            <tstuc:DataSynchronization runat="server" ID="ucDataSynchronization" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_DataSynchronization %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
			            </ZoneTemplate>
			        </tstsc:WebPartZoneEx>
		        </div>
	        </div>
	    </ContentTemplate>
	</tstsc:UpdatePanelEx>
</asp:Content>
