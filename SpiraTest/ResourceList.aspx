<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="ResourceList.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.ResourceList" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    Title="Untitled Page" 
 %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
	<div class="main-panel pa4">
        <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
            <div class="btn-group priority3" role="group">
			    <tstsc:DropMenu id="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync" ClientScriptServerControlId="grdResourceList" ClientScriptMethod="load_data()" />
            </div>
            <div class="btn-group priority1" role="group">
				<tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" 
					Text="<%$Resources:Buttons,Filter%>" MenuWidth="125px" ClientScriptServerControlId="grdResourceList" ClientScriptMethod="apply_filters()">
					<DropMenuItems>
						<tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						<tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptMethod="clear_filters()" />
					</DropMenuItems>
				</tstsc:DropMenu>
			</div>
        </div>


	    <div class="row main-content">
            <div class="bg-near-white-hover py2 px3 br2 transition-all flex-container flex-item-center flex-wrap-reverse">
                <div class="flex-grow-1">
			        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
			        <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
			        <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,ResourceList_ResourcesInGroupOrProject %>" />.
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                </div>
                <div class="flex-none">
			        <tstsc:DropDownHierarchy ID="ddlSelectRelease" Runat="server" NoValueItem="true" AutoPostBack="false" DataTextField="FullName" DataValueField="ReleaseId"
				        Width="300px" ListWidth="300px" SkinID="ReleaseDropDownListFarRight"
				        ClientScriptServerControlId="grdResourceList" ClientScriptMethod="custom_operation_select" ClientScriptParameter="SelectRelease" />
                </div>
		    </div>
			<tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
	        <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                <Services>  
                <asp:ServiceReference Path="~/Services/Ajax/UserService.svc" />  
                </Services>  
            </tstsc:ScriptManagerProxyEx>  
            <tstsc:SortedGrid 
                AllowDragging="false" 
                AllowEditing="false" 
				AutoLoad="true" 
                CssClass="DataGrid DataGrid-no-bands" 
                DisplayAttachments="false"
                EditRowCssClass="Editing" 
                ErrorMessageControlId="lblMessage"
                FilterInfoControlId="lblFilterInfo"
                HeaderCssClass="Header"
                ID="grdResourceList" 
                ItemImage="artifact-Resource.svg"
				RowCssClass="Normal" 
				runat="server" 
                SelectedRowCssClass="Highlighted" 
				SubHeaderCssClass="SubHeader" 
                TotalCountControlId="lblTotalCount" 
                VisibleCountControlId="lblVisibleCount" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.UserService"
                />
			<br />
            <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
		</div>
    </div>
</asp:Content>
