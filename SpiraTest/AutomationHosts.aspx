<%@ Page Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="True" CodeBehind="AutomationHosts.aspx.cs" Inherits="Inflectra.SpiraTest.Web.AutomationHosts" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
<div class="panel-container df">
    <div class="main-panel pl4 grow-1">
        <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnNewHost" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
					    Text="<%$Resources:Dialogs,AutomationHosts_NewHost %>" Authorized_ArtifactType="AutomationHost" Authorized_Permission="Create" ClientScriptServerControlId="grdAutomationHosts" ClientScriptMethod="insert_item('AutomationHost')" />
				    <tstsc:DropMenu id="btnDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptServerControlId="grdAutomationHosts" ClientScriptMethod="delete_items()"
					    Text="<%$Resources:Buttons,Delete %>" Authorized_ArtifactType="AutomationHost" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,AutomationHosts_DeleteConfirm %>" />
			    </div>
                <div class="btn-group priority2" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync"
					    Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdAutomationHosts" ClientScriptMethod="load_data()" />
			    </div>
			    <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
					    Text="<%$Resources:Buttons,Filter %>" MenuWidth="125px" ClientScriptServerControlId="grdAutomationHosts" ClientScriptMethod="apply_filters()">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						    <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>" GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
						    <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>" GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
			    </div>
			    <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdAutomationHosts" ClientScriptMethod="toggle_visibility" />
                </div>
	        </div>
	        <div class="row main-content">
                <div class="bg-near-white-hover py2 px3 br2 transition-all">
			        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" /> <asp:Label ID="lblVisibleHostCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalHostCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,AutomationHosts_AutomationHostsForProject %>" />.
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />              
			    </div>
			    <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
			    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/AutomationHostService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:SortedGrid 
                    AllowColumnPositioning="true"
				    Authorized_ArtifactType="AutomationHost" 
                    Authorized_Permission="BulkEdit" 
				    ConcurrencyEnabled="true" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    EditRowCssClass="Editing" 
                    ErrorMessageControlId="divMessage"
                    FilterInfoControlId="lblFilterInfo"
                    HeaderCssClass="Header"
                    ID="grdAutomationHosts" 
                    ItemImage="artifact-AutomationHost.svg"
				    RowCssClass="Normal" 
				    runat="server" 
                    SelectedRowCssClass="Highlighted" 
				    SubHeaderCssClass="SubHeader" 
                    TotalCountControlId="lblTotalHostCount" 
				    VisibleCountControlId="lblVisibleHostCount" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.AutomationHostService"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="AutomationHost" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="AutomationHost" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="AutomationHost" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Dialogs,AutomationHosts_NewHost %>" CommandName="insert_item" CommandArgument="AutomationHost" Authorized_ArtifactType="AutomationHost" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="AutomationHost" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,AutomationHosts_DeleteConfirm %>" />
                    </ContextMenuItems>    
                </tstsc:SortedGrid>
			    <br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>
</asp:Content>
