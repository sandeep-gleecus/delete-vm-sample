<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="TestConfigurations.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestConfigurations" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
	<div class="panel-container flex">
        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnNewConfigurationSet" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
					    Text="<%$Resources:Main,TestConfigurationSets_NewTestConfigurationSet %>" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" ClientScriptServerControlId="grdTestConfigurationSets" ClientScriptMethod="insert_item('TestConfigurationSet')" />
				    <tstsc:DropMenu id="btnDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptServerControlId="grdTestConfigurationSets" ClientScriptMethod="delete_items()"
					    Text="<%$Resources:Buttons,Delete %>" Authorized_ArtifactType="TestSet" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,TestConfigurationSet_DeleteConfirm %>" />
			    </div>
                <div class="btn-group priority2" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync"
					    Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdTestConfigurationSets" ClientScriptMethod="load_data()" />
			    </div>
			    <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
					    Text="<%$Resources:Buttons,Filter %>" MenuWidth="125px" ClientScriptServerControlId="grdTestConfigurationSets" ClientScriptMethod="apply_filters()">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
			    </div>
	        </div>


	        <div class="main-content">
                <div class="bg-near-white-hover py2 px3 br2 transition-all">
			        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" /> <asp:Label ID="lblVisibleSetCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalSetCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,TestConfigurations_TestConfigurationsForProject %>" />.
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />              
			    </div>
			    <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
			    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/TestConfigurationSetService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:SortedGrid 
                    AllowColumnPositioning="false"
				    Authorized_ArtifactType="TestSet" 
                    Authorized_Permission="BulkEdit" 
				    ConcurrencyEnabled="true" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    EditRowCssClass="Editing" 
                    ErrorMessageControlId="divMessage"
                    FilterInfoControlId="lblFilterInfo"
                    HeaderCssClass="Header"
                    ID="grdTestConfigurationSets" 
                    ItemImage="artifact-Configuration.svg"
				    RowCssClass="Normal" 
				    runat="server" 
                    SelectedRowCssClass="Highlighted" 
				    SubHeaderCssClass="SubHeader" 
                    TotalCountControlId="lblTotalSetCount" 
				    VisibleCountControlId="lblVisibleSetCount" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationSetService"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Main,TestConfigurationSets_NewTestConfigurationSet %>" CommandName="insert_item" CommandArgument="TestConfigurationSet" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="TestSet" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,TestConfigurationSet_DeleteConfirm %>" />
                    </ContextMenuItems>    
                </tstsc:SortedGrid>
			    <br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
</asp:Content>
