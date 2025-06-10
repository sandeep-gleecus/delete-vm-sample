<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.Default" %>

<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Register TagPrefix="tstuc" TagName="ProjectOverview" Src="~/UserControls/WebParts/ProjectAdmin/ProjectOverview.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryChanges" Src="~/UserControls/WebParts/ProjectAdmin/HistoryChanges.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="Components" Src="~/UserControls/WebParts/ProjectAdmin/Components.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SourceCode" Src="~/UserControls/WebParts/ProjectAdmin/SourceCode.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="UserMembership" Src="~/UserControls/WebParts/ProjectAdmin/UserMembership.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DataMapping" Src="~/UserControls/WebParts/ProjectAdmin/DataMapping.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
	<tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DashboardStylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<tstsc:UpdatePanelEx ID="ajaxWebParts" runat="server">
		<ContentTemplate>
			<tstsc:WebPartManagerEx ID="prtManager" runat="server" DashboardName="ProjectAdmin" OnInit="prtManager_Init" OnAuthorizeWebPart="prtManager_AuthorizeWebPart">
				<StaticConnections>
				</StaticConnections>
			</tstsc:WebPartManagerEx>
			<div class="py4 px3 flex justify-between items-center">
				<div>
					<h1>
						<asp:Localize runat="server" Text="<%$Resources:Main,Admin_Project_Home_Title %>" />
						<small>
							<asp:Label ID="lblProjectName" runat="server" /></small>
					</h1>
					<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
					<asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="ValidationMessage" DisplayMode="BulletList" ShowSummary="True" ShowMessageBox="False" />
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
				</div>
			</div>

			<tstsc:MessageBox ID="MessageBox1" runat="server" SkinID="MessageBox" />

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

			<div class="flex flex-wrap pb4">
				<div class="w-50 w-100-sm pl3 pr2 pr3-sm relative">
					<tstsc:WebPartZoneEx ID="prtLeftZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_LeftSide%>" SkinID="WebPartZone">
						<ZoneTemplate>
							<tstuc:ProjectOverview runat="server" ID="ucProjectOverview" MessageBoxId="lblMessage" Title="<%$Resources:Main,ProjectHome_ProjectOverview %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
							<tstuc:UserMembership runat="server" ID="ucUserMembership" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_ProjectMembership %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />
						</ZoneTemplate>
					</tstsc:WebPartZoneEx>
				</div>
				<div class="w-50 w-100-sm pr3 pl2 pl3-sm relative">
					<tstsc:WebPartZoneEx ID="prtRightZone" runat="server" HeaderText="<%$Resources:ServerControls,WebPartZone_RightSide%>" SkinID="WebPartZone">
						<ZoneTemplate>
							<tstuc:HistoryChanges runat="server" ID="ucHistoryChanges" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_HistoryChangeset %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />
							<tstuc:Components runat="server" ID="ucComponents" MessageBoxId="lblMessage" Title="<%$Resources:Main,Global_Components %>" Subtitle="<%$Resources:Main,Global_ViewAll %>" />
							<tstuc:SourceCode runat="server" ID="ucSourceCode" MessageBoxId="lblMessage" Title="<%$Resources:Main,SiteMap_SourceCode %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
							<tstuc:DataMapping runat="server" ID="ucDataMapping" MessageBoxId="lblMessage" Title="<%$Resources:Main,Admin_DataSynchronization %>" Subtitle="<%$Resources:Main,Global_ViewDetails %>" />
						</ZoneTemplate>
					</tstsc:WebPartZoneEx>
				</div>
			</div>
		</ContentTemplate>
	</tstsc:UpdatePanelEx>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
</asp:Content>
