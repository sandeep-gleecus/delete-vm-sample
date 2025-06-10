<%@ Page 
    language="c#"
    Codebehind="Releases.aspx.cs" 
    AutoEventWireup="True" 
    Inherits="Inflectra.SpiraTest.Web.Releases" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>

<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	<div class="panel-container df">
        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                <div class="btn-group priority3 priority1-first-child" role="group">
		            <tstsc:DropMenu id="btnInsert" runat="server" GlyphIconCssClass="mr3 fas fa-plus" Text="<%$Resources:Buttons,Insert %>"
			            AlternateText="Insert" Authorized_ArtifactType="Release" Authorized_Permission="Create" MenuWidth="125px"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="insert_item('Release')" >
			            <DropMenuItems>
				            <tstsc:DropMenuItem Name="Release" Value="<%$Resources:Dialogs,Releases_NewRelease %>" ImageUrl="Images/artifact-Release.svg" ClientScriptMethod="insert_item('Release')" />
				            <tstsc:DropMenuItem Name="Release" Value="<%$Resources:Dialogs,Releases_ChildRelease %>" ImageUrl="Images/action-InsertChildRelease.svg" ClientScriptMethod="insert_item('ChildRelease')" />
				            <tstsc:DropMenuItem Divider="true" />
				            <tstsc:DropMenuItem Name="Iteration" Value="<%$Resources:Dialogs,Releases_NewIteration %>" ImageUrl="Images/artifact-Iteration.svg" ClientScriptMethod="insert_item('Iteration')" />
				            <tstsc:DropMenuItem Name="Iteration" Value="<%$Resources:Dialogs,Releases_ChildIteration %>" ImageUrl="Images/action-InsertChildIteration.svg" ClientScriptMethod="insert_item('ChildIteration')" />
			            </DropMenuItems>
		            </tstsc:DropMenu>
		            <tstsc:DropMenu id="btnDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt"
			            Authorized_ArtifactType="Release" Text="<%$Resources:Buttons,Delete %>"
			            Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,ReleaseList_DeleteConfirm %>" Confirmation="true"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="delete_items()" />
                </div>
                <div class="btn-group priority3" role="group">
	                <tstsc:DropMenu id="btnIndent" runat="server" GlyphIconCssClass="mr3 glyphicon fas fa-arrow-right"
			            Text="<%$Resources:Buttons,Indent %>" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="indent_items()" />	    
                    <tstsc:DropMenu id="btnOutdent" runat="server" GlyphIconCssClass="mr3 glyphicon fas fa-arrow-left"
			            Text="<%$Resources:Buttons,Outdent %>" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="outdent_items()" />
                    <tstsc:DropMenu id="btnRefresh" runat="server" AlternateText="Refresh" GlyphIconCssClass="mr3 fas fa-sync"
                        Text="<%$Resources:Buttons,Refresh %>"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="load_data()" />
                </div>
                <div class="btn-group priority1" role="group">
		            <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="false"
			            DataTextField="Value" DataValueField="Key" CssClass="DropDownList"
			            ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="expand_to_level" Width="110px" />
		            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="apply_filters()">
			            <DropMenuItems>
				            <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				            <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
				            <tstsc:DropMenuItem Divider="true" />
                            <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>" GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
				            <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>" GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
			            </DropMenuItems>
		            </tstsc:DropMenu>
                </div>
                <div class="btn-group priority4" role="group">
		            <tstsc:DropMenu id="btnEdit" runat="server" GlyphIconCssClass="mr3 far fa-edit" Text="<%$Resources:Buttons,Edit %>"
			            Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" MenuWidth="140px"
                        ClientScriptMethod="edit_items()"
			            ClientScriptServerControlId="grdReleaseList">
			            <DropMenuItems>
                            <tstsc:DropMenuItem runat="server" Name="Edit" Value="<%$Resources:Buttons,EditItems%>" GlyphIconCssClass="mr3 far fa-edit" ClientScriptMethod="edit_items()" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
						    <tstsc:DropMenuItem runat="server" Divider="true" />
				            <tstsc:DropMenuItem Name="Copy" Value="<%$Resources:Buttons,CopyItems%>" GlyphIconCssClass="mr3 fas fa-copy" ClientScriptMethod="copy_items()" />
				            <tstsc:DropMenuItem Name="Cut" Value="<%$Resources:Buttons,CutItems%>" GlyphIconCssClass="mr3 fas fa-cut" ClientScriptMethod="cut_items()" />
				            <tstsc:DropMenuItem Name="Paste" Value="<%$Resources:Buttons,PasteItems%>" GlyphIconCssClass="mr3 far fa-clipboard" ClientScriptMethod="paste_items()" />
			            </DropMenuItems>
		            </tstsc:DropMenu>
                    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
			            Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" ClientScriptServerControlId="grdReleaseList">
			            <DropMenuItems>
                            <tstsc:DropMenuItem Name="Export" Value="<%$Resources:Dialogs,Global_ExportToProject %>" GlyphIconCssClass="mr3 fas fa-sign-out-alt" ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Global_ExportItems, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)" Authorized_ArtifactType="Release" Authorized_Permission="View" />
                            <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="get_element;print_items('html')" Authorized_ArtifactType="Release" Authorized_Permission="View" />
				            <tstsc:DropMenuItem Divider="true" />
				            <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('excel')" />
				            <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('word')" />
				            <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('pdf')" />
				            <tstsc:DropMenuItem Divider="true" />
				            <tstsc:DropMenuItem Name="CreateTestSet" Value="<%$Resources:Dialogs,Releases_CreateTestSet %>" ImageUrl="Images/artifact-TestSet.svg" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" ClientScriptMethod="custom_list_operation('CreateTestSet', null, Inflectra.SpiraTest.Web.GlobalResources.Releases_SuccessfullyCreatedTestSet)" />
			            </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
		            <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" ClientScriptServerControlId="grdReleaseList" ClientScriptMethod="toggle_visibility" />
                </div>
	        </div>
	        <div class="row main-content">
                <div class="bg-near-white-hover py2 px3 br2 transition-all">
		            <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
		            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
		            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Releases_ReleasesForProject %>" />.
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />

                    <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                        <span class="btn-group priority1 pull-right pull-left-xs" style="margin-left:5px" role="group">
                            <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                                <span class="fas fa-indent"></span>
                            </a>
                            <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -9) %>' runat="server" title="<%$ Resources:Main,Global_Gantt %>">
                                <span class="fas fa-align-left"></span>
                            </a>
                            <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                                <span class="fas fa-project-diagram"></span>
                            </a>
                        </span>
                    </asp:PlaceHolder> 
                </div>
		        <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
		        <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                        <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:HierarchicalGrid ID="grdReleaseList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" ConcurrencyEnabled="true"
			        SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
			        RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Release.svg" SummaryItemImage="artifact-Release.svg"
			        runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService" AlternateItemImage="artifact-Iteration.svg"
			        ExpandedItemImage="artifact-Release.svg" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit"
                    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo" AllowColumnPositioning="true">
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Release" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Release" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem ImageUrl="Images/artifact-Release.svg" Caption="<%$Resources:Dialogs,Releases_NewRelease %>" CommandName="insert_item" CommandArgument="Release" Authorized_ArtifactType="Release" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem ImageUrl="Images/artifact-Iteration.svg" Caption="<%$Resources:Dialogs,Releases_NewIteration %>" CommandName="insert_item" CommandArgument="Iteration" Authorized_ArtifactType="Release" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete%>" CommandName="delete_items" Authorized_ArtifactType="Release" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,ReleaseList_DeleteConfirm %>" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 glyphicon fas fa-arrow-right" Caption="<%$Resources:Buttons,Indent%>" CommandName="indent_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 glyphicon fas fa-arrow-left" Caption="<%$Resources:Buttons,Outdent%>" CommandName="outdent_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Buttons,CopyItems%>" CommandName="copy_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-cut" Caption="<%$Resources:Buttons,CutItems%>" CommandName="cut_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-clipboard" Caption="<%$Resources:Buttons,PasteItems%>" CommandName="paste_items" Authorized_ArtifactType="Release" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="Release" Authorized_Permission="View" />
                    </ContextMenuItems>         
                </tstsc:HierarchicalGrid>
		        <br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        //Prints the selected items
        function print_items(format)
        {
            //Get the items and convert to csv
            var grdReleaseList = $find('<%=grdReleaseList.ClientID %>');
            var items = grdReleaseList.get_selected_items();
            if (items.length < 1)
            {
                alert(resx.Global_SelectOneCheckBoxForCommand);
            }
            else
            {
                var item_list = globalFunctions.convertIntArrayToString(items);
                //Open the report for the specified format
                var reportToken;
                var filter;
                if (format == 'excel')
                {
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.ReleaseSummary%>";
                    filter = "&af_16_98=";
                }
                else
                {
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.ReleaseDetailed%>";
                    filter = "&af_17_98=";
                }

                //Open the report for the specified format
                globalFunctions.launchStandardReport(reportToken, format, filter, item_list);
            }
        }
    </script>
</asp:Content>