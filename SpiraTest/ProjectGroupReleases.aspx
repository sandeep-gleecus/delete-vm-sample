<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ProjectGroupReleases.aspx.cs" Inherits="Inflectra.SpiraTest.Web.ProjectGroupReleases" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
	<table class="panel-container">
        <tbody>
            <tr>
                <td class="main-panel">
                    <div class="btn-toolbar toolbar" role="toolbar">
                        <div class="btn-group priority1" role="group">
                            <tstsc:DropMenu id="btnRefresh" runat="server" AlternateText="Refresh" GlyphIconCssClass="mr3 fas fa-sync"
                                Text="<%$Resources:Buttons,Refresh %>"
			                    ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="load_data()" />    
		                    <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="false"
			                    DataTextField="Value" DataValueField="Key" CssClass="DropDownList"
			                    ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="expand_to_level" Width="110px" />
		                    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                     ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="apply_filters()">
			                    <DropMenuItems>
				                    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				                    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			                    </DropMenuItems>
		                    </tstsc:DropMenu>
                        </div>
                        <div class="btn-group" role="group">
		                    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="toggle_visibility" />
                        </div>
	                </div>
	                <div class="row main-content">
                        <div class="bg-near-white-hover py2 px3 br2 transition-all">
                            <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
		                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                            <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
		                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                            <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
		                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Releases_ReleasesForProgram %>" />.
                            <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                        </div>
		                <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
		                <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                            <Services>  
                                <asp:ServiceReference Path="~/Services/Ajax/GroupReleasesService.svc" />  
                            </Services>  
                        </tstsc:ScriptManagerProxyEx>  
                        <tstsc:HierarchicalGrid ID="grdReleaseList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" ConcurrencyEnabled="true"
			                SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
			                RowCssClass="Normal" ItemImage="artifact-Release.svg" SummaryItemImage="artifact-Release.svg"
			                runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GroupReleasesService" AlternateItemImage="artifact-Iteration.svg"
			                ExpandedItemImage="artifact-Release.svg" Authorized_ArtifactType="Release" AllowEditing="false"
                            VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo" AllowColumnPositioning="false">
                            <ContextMenuItems>
                                <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" />
                                <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank"  />
                            </ContextMenuItems>         
                        </tstsc:HierarchicalGrid>
		                <br />
                        <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                    </div>
                </td>
            </tr>
        </tbody>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
</asp:Content>
