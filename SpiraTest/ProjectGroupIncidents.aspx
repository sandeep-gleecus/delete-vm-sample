<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ProjectGroupIncidents.aspx.cs" Inherits="Inflectra.SpiraTest.Web.ProjectGroupIncidents" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <table class="panel-container">
        <tbody>
            <tr>
                <td class="main-panel">
                    <div class="btn-toolbar toolbar" role="toolbar">
                        <div class="btn-group priority1" role="group">
				            <tstsc:DropMenu id="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync"
					            ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="load_data()" />
				            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
					            MenuWidth="125px" ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="apply_filters()">
					            <DropMenuItems>
						            <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						            <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
					            </DropMenuItems>
				            </tstsc:DropMenu>
			            </div>
			            <div class="btn-group" role="group">
				            <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="toggle_visibility" />
                        </div>
                    </div>
			        <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
                    <div class="bg-near-white-hover py2 px3 br2 transition-all">
			            <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                        <asp:Label ID="lblVisibleIncidentCount" Runat="server" Font-Bold="True" />
			            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                        <asp:Label ID="lblTotalIncidentCount" Runat="server" Font-Bold="True" />
			            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,IncidentList_IncidentsForProgram %>" />.
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                    </div>
                    <div class="main-content">
				        <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                          <Services>  
                            <asp:ServiceReference Path="~/Services/Ajax/GroupIncidentsService.svc" />  
                          </Services>  
                        </tstsc:ScriptManagerProxyEx>
                        <tstsc:SortedGrid 
                            AllowEditing="false"
                            AllowColumnPositioning="false"
				            ConcurrencyEnabled="true" 
                            CssClass="DataGrid DataGrid-no-bands" 
                            ErrorMessageControlId="divMessage"
                            FilterInfoControlId="lblFilterInfo" 
                            HeaderCssClass="Header"
                            ID="grdIncidentList" 
                            ItemImage="artifact-Incident.svg"
				            RowCssClass="Normal" 
				            runat="server" 
                            SelectedRowCssClass="Highlighted" 
				            SubHeaderCssClass="SubHeader" 
                            TotalCountControlId="lblTotalIncidentCount"
				            VisibleCountControlId="lblVisibleIncidentCount" 
                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GroupIncidentsService"
                            >
                            <ContextMenuItems>
                                <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" />
                                <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" />
                            </ContextMenuItems> 
                        </tstsc:SortedGrid>
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
