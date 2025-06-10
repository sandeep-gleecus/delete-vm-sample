<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestSetList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestSetList" MasterPageFile="~/MasterPages/Main.Master" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex flex-column-reverse-sm">
        <div class="side-panel hidden-sm-second-child sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlFolders" runat="server" HeaderCaption="<%$Resources:Main,Global_Folders %>" MinWidth="100" MaxWidth="500"  data-panel="folder-tree"
                DisplayRefresh="true" ClientScriptServerControlId="trvFolders" ClientScriptMethod="load_data(true)" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <div class="Widget panel">
                    <tstsc:TreeView ID="trvFolders" runat="server" ClientScriptMethod="trvFolders_folderChanged" NodeLegendFormat="{0}" EditDescriptions="true"
                        LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="divMessage"
                        Authorized_Permission="BulkEdit" Authorized_ArtifactType="TestSet" AllowEditing="true" ItemName="<%$Resources:Fields,Folder %>"
                        NodeLegendControlId="txtFolderInfo" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
                        />
                </div>    
            </tstsc:SidebarPanel>
            <tstsc:SidebarPanel ID="pnlQuickFilters" runat="server" HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" MinWidth="100" MaxWidth="500"
                ErrorMessageControlId="divMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService" CssClass="blob">
                <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="TestSet" FolderLegendControlId="txtFolderInfo"
                    DisplayReleases="false"
                    AjaxServerControlId="grdTestSetList" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"  />
            </tstsc:SidebarPanel>
            <tstsc:SidebarPanel 
                HeaderCaption="<%$Resources:Dialogs,SidebarPanel_Charts %>"
                ID="pnlCharts" 
                MaxWidth="500"
                MinWidth="100" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <div 
                    class="u-chart db"
                    id="txExecutionStatus"
                    ></div>
                <div
                    class="u-chart db" 
                    id="txOverdue"
                    ></div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                <div class="btn-group priority4 priority1-first-child" role="group">
				    <tstsc:DropMenu id="btnInsert" runat="server" GlyphIconCssClass="mr3 fas fa-plus" Text="<%$Resources:Dialogs,TestSetList_NewTestSet %>"
					    AlternateText="Insert" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" MenuWidth="175px"
					    ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="insert_item()" />
				    <tstsc:DropMenu id="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt"
					    Authorized_ArtifactType="TestSet" Authorized_Permission="Delete"
					    Confirmation="true" ConfirmationMessage="<%$Resources:Messages,TestSetList_DeleteConfirm %>"
					    ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="delete_items()" />
                </div>
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync"
					    Text="<%$Resources:Buttons,Refresh %>"
					    ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="load_data()" />
                </div>
                <div class="btn-group priority4" role="group">
				    <tstsc:DropMenu id="btnFocusOn" runat="server" GlyphIconCssClass="mr3 far fa-folder-open"
					    Text="<%$Resources:Buttons,FocusOn %>"
					    ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="focus_on()" />
                </div>
                <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
					    MenuWidth="150px" ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="apply_filters()">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						    <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>" GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
						    <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>" GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
                </div>
                <div class="btn-group priority4" role="group">
				    <tstsc:DropMenu id="btnEdit" runat="server" GlyphIconCssClass="mr3 far fa-edit" Text="<%$Resources:Buttons,Edit %>"
					    Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" MenuWidth="150px"
                        ClientScriptMethod="edit_items()"
					    ClientScriptServerControlId="grdTestSetList">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Copy" Value="<%$Resources:Buttons,CopyItems%>" GlyphIconCssClass="mr3 fas fa-copy" ClientScriptMethod="copy_items()" />
						    <tstsc:DropMenuItem Name="Cut" Value="<%$Resources:Buttons,CutItems%>" GlyphIconCssClass="mr3 fas fa-cut" ClientScriptMethod="cut_items()" />
						    <tstsc:DropMenuItem Name="Paste" Value="<%$Resources:Buttons,PasteItems%>" GlyphIconCssClass="mr3 far fa-clipboard" ClientScriptMethod="paste_items()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
            	    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog" Text="<%$Resources:Buttons,Tools %>" MenuWidth="150px">
					    <DropMenuItems>
                    	    <tstsc:DropMenuItem Name="Execute" Value="<%$Resources:Buttons,Execute %>" GlyphIconCssClass="mr3 fas fa-play" ClientScriptMethod="grdTestSetList_execute()" Authorized_ArtifactType="TestRun" Authorized_Permission="Create" />
                    	    <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="print_items()" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                            <tstsc:DropMenuItem Divider="true" />
                            <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="TestSet" Authorized_Permission="View" ClientScriptMethod="print_items('excel')" />
				            <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="TestSet" Authorized_Permission="View" ClientScriptMethod="print_items('word')" />
				            <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="TestSet" Authorized_Permission="View" ClientScriptMethod="print_items('pdf')" />    
					    </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="toggle_visibility" />
                </div>
            </div>
            <div class="bg-near-white-hover py2 px3 br2 transition-all alert-narrow flex-container flex-item-center flex-wrap-reverse">
                <div class="flex-grow-1">
			        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
			        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
			        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,TestSetList_TestSetsForProject %>" />.
                    <asp:Label runat="server" ID="txtFolderInfo" CssClass="badge mx3 align-v-reset" />
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                </div>
                <div class="flex-none">
				    <tstsc:DropDownHierarchy ID="ddlSelectRelease" Runat="server" NoValueItem="true"
                        NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" AutoPostBack="false" DataTextField="FullName"
                        DataValueField="ReleaseId" Width="300px" ListWidth="300px" SkinID="ReleaseDropDownListFarRight" ActiveItemField="IsActive"
				        ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="custom_operation_select" ClientScriptParameter="SelectRelease" />
                </div>
            </div>
            <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
            <div class="main-content">
				<tstsc:ScriptManagerProxyEx ID="ajxScriptManagerProxy" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
                    <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
                    <asp:ServiceReference Path="~/Services/Ajax/GraphingService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:SortedGrid ID="grdTestSetList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" ConcurrencyEnabled="true"
				    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
				    RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-TestSet.svg" FolderItemImage="Folder.svg"
				    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
                    NegativePrimaryKeysDisabled="false" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit"
                    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
                    AllowColumnPositioning="true">
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-folder-open" Caption="<%$Resources:Buttons,FocusOn %>" CommandName="focus_on" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Buttons,Insert%>" CommandName="insert_item" CommandArgument="TestSet" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete%>" CommandName="delete_items" Authorized_ArtifactType="TestSet" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,TestSetList_DeleteConfirm %>" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Buttons,CopyItems%>" CommandName="copy_items" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-cut" Caption="<%$Resources:Buttons,CutItems%>" CommandName="cut_items" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-clipboard" Caption="<%$Resources:Buttons,PasteItems%>" CommandName="paste_items" Authorized_ArtifactType="TestSet" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-play" Caption="<%$Resources:Buttons,Execute%>" ClientScriptMethod="grdTestSetList_execute()" Authorized_ArtifactType="TestRun" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="TestSet" Authorized_Permission="View" />
                    </ContextMenuItems>     
                </tstsc:SortedGrid>
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                <tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="divMessage"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
            </div>
        </div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        function grdTestSetList_execute()
        {
            var projectId = <%=ProjectId %>;
            var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

            //Get the list of selected test sets
            var grdTestSetList = $find('<%=grdTestSetList.ClientID %>');

            //We need exactly one test set
            var testSetIds = grdTestSetList.get_selected_items();
            if (testSetIds.length != 1)
            {
                alert (resx.TestSetList_SelectSingleTestSet);
                return;
            }
            var testSetId = parseInt(testSetIds[0] || 0);

            //Make sure that it is not a folder (negative ID)
            if (testSetId < 1)
            {
                alert(resx.TestSetList_CannotExecuteFolder);
            }
            else
            {
                //Actually start the background process of creating the test runs
                ajxBackgroundProcessManager.display(projectId, 'TestSet_Execute', resx.TestSetList_ExecuteTestSet, resx.TestSetList_ExecuteTestSetDesc, testSetId);
            }
        }
        function ajxBackgroundProcessManager_success(msg, returnCode)
        {
            //Need to redirect to the test runs pending or to the test automation launch file
            //A return code of -2 means that this is an automated test and need to redirect to the .TST launch page
            if (returnCode == -2)
            {
                var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
                var testSetId = ajxBackgroundProcessManager.get_parameter1();
                var baseUrl = '<%=TestSetLaunchUrl %>';
                var url = baseUrl.replace(globalFunctions.artifactIdToken, testSetId);
                window.location = url;
            }
            if (returnCode && returnCode > 0)
            {
                var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
                var projectId = ajxBackgroundProcessManager.get_projectId();
                var baseUrl = '<%=TestRunsPendingUrl %>';
                var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
                window.location = url;
            }
        }
        //Prints the selected items
        function print_items(format)
        {
            //Get the items and convert to csv
            var grdTestSetList = $find('<%=grdTestSetList.ClientID %>');
            var items = grdTestSetList.get_selected_items();
            if (items.length < 1)
            {
                alert(resx.Global_SelectOneCheckBoxForCommand);
            }
            else
            {
                var folderId = null;
                for (var i = 0; i < items.length; i++)
                {
                    if (parseInt(items[i]) < 0)
                    {
                        folderId = -items[i];
                    }
                }
                //Cannot select a folder and other items
                if (folderId && items.length > 1)
                {
                    alert (resx.Global_CannotSelectMoreThanOneFolder);
                    return;
                }

                var reportToken;
                var filter;
                var artifacts;

                //Handle folder case
                if (folderId)
                {
                    if (format == 'excel')
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetSummary%>";
                        filter = "&fl_7=";
                    }
                    else
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetDetailed%>";
                        filter = "&fl_8=";
                    }
                    artifacts = folderId;
                }
                else
                {
                    var item_list = globalFunctions.convertIntArrayToString(items);
                    //Open the report for the specified format
                    if (format == 'excel')
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetSummary%>";
                        filter = "&af_7_91=";
                    }
                    else
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetDetailed%>";
                        filter = "&af_8_91=";
                    }
                    artifacts = item_list;
                 }

                //Open the report for the specified format
                globalFunctions.launchStandardReport(reportToken, format, filter, artifacts);
            }
        }
        function trvFolders_dragEnd()
        {
            //Simply reload the grid
            var ajaxControl = $find('<%=grdTestSetList.ClientID %>');
            ajaxControl.load_data();
        }
        function grdTestSetList_focusOn(nodeId)
        {
            //It means the folder may have changed, so reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.set_selectedNode(nodeId);
            trvFolders.load_data(true);
        }
        function trvFolders_folderChanged(folderId)
        {
            //Set the standard filter and reload
            var grdTestSetList = $find('<%=grdTestSetList.ClientID %>');
            var args = {};
            args['_FolderId'] = globalFunctions.serializeValueInt(folderId);
            grdTestSetList.set_standardFilters(args);
            grdTestSetList.load_data();
        }
        function grdTestSetList_loaded()
        {
            loadGraphs();
        }

        /* Charts */
        var sidepanelCharts = new Object;

        function loadGraphs()
        {
            //Get the release (if we have one)
            var releaseId = null;
            var ddlSelectRelease = $find('<%=this.ddlSelectRelease.ClientID%>');
            if (ddlSelectRelease && ddlSelectRelease.get_selectedItem() && ddlSelectRelease.get_selectedItem().get_value() != '')
            {
                releaseId = parseInt(ddlSelectRelease.get_selectedItem().get_value());
            }

            //Load the execution status graph
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.TestSet_RetrieveExecutionSummary(SpiraContext.ProjectId, releaseId, function (data)
            {
                //Success
                var cleanedData = prepDataForC3(data);

                sidepanelCharts.chartTestSetExecutionStatus = c3.generate({
                    bindto: d3.select('#txExecutionStatus'),
                    data: {
                        columns: cleanedData.columns,
                        type: "donut",
                        colors: cleanedData.colors
                    },
                    donut: {
                        label: {
                            format: function (value, ratio, id) {
                                return d3.format('d')(value);
                            }
                        }
                    }
                });
            }, function(ex) {
                //Fail quietly
            });

            //Load the overdue count graph
            //We don't filter this one by release, because the release dropdown is for filtering the execution status, not the test sets
            //themselves.
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.TestSet_RetrieveScheduleSummary(SpiraContext.ProjectId, null, function(data) {
                //Success
                var cleanedData = prepDataForC3(data);

                sidepanelCharts.chartTestSetOverdue = c3.generate({
                    bindto: d3.select('#txOverdue'),
                    data: {
                        columns: cleanedData.columns,
                        type: "donut",
                        colors: cleanedData.colors
                    },
                    donut: {
                        label: {
                            format: function (value, ratio, id) {
                                return d3.format('d')(value);
                            }
                        }
                    }
                });
            }, function(ex) {
                //Fail quietly
            });

            function prepDataForC3(data) {
                var res = new Object();
                if (data)
                {
                    res.columns = new Array;
                    res.colors = new Object;
                    for (var i = 0; i < data.length; i++)
                    {
                        var column = [data[i].caption, data[i].count];
                        res.columns.push(column);
                        res.colors[data[i].caption] = "#" + (data[i].color == null ? "f1f1f1" : data[i].color);
                    }
                }
                return res;
            }
        }
    </script>
</asp:Content>
