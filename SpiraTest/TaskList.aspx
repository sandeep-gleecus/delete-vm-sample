<%@ Page 
    AutoEventWireup="True" 
    CodeBehind="TaskList.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.TaskList" Title="Untitled Page" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex flex-column-reverse-sm">
        <div class="side-panel hidden-sm-second-child sticky top-nav self-start">
            <tstsc:SidebarPanel 
                ClientScriptMethod="load_data(true)" 
                ClientScriptServerControlId="trvFolders" 
                data-panel="folder-tree"
                DisplayRefresh="true" 
                HeaderCaption="<%$Resources:Main,Global_Folders %>" 
                ID="pnlFolders" 
                MaxWidth="500" 
                MinWidth="100" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <div class="Widget panel">
                    <tstsc:TreeView 
                        AllowEditing="true" 
                        Authorized_ArtifactType="Task" 
                        Authorized_Permission="BulkEdit" 
                        ClientScriptMethod="trvFolders_folderChanged" 
                        CssClass="FolderTree" 
                        ErrorMessageControlId="lblMessage"
                        ID="trvFolders" 
                        ItemName="<%$Resources:Fields,Folder %>"
                        LoadingImageUrl="Images/action-Spinner.svg" 
                        NodeLegendControlId="txtFolderInfo" 
                        NodeLegendFormat="{0}"      
                        runat="server" 
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"
                        />
                </div>    
            </tstsc:SidebarPanel>

            <tstsc:SidebarPanel 
                SkinID="RelativeSidebarPanel"
                ID="pnlQuickFilters" 
                runat="server" 
                HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" 
                MinWidth="100" 
                MaxWidth="500"
                ErrorMessageControlId="divMessage" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="Task" FolderLegendControlId="txtFolderInfo"
                    AjaxServerControlId="grdTaskList" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"  />
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
                    id="tkTaskProgress"
                    ></div>
                <div 
                    class="u-chart db"
                    id="tkTaskBurndown"
                    ></div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                <div class="btn-group priority2 priority1-first-child" role="group">
				    <tstsc:DropMenu id="btnNewTask" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
					    Text="<%$Resources:Dialogs,TaskList_NewTask%>" Authorized_ArtifactType="Task" Authorized_Permission="Create" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="insert_item('Task')" />
				    <tstsc:DropMenu id="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete%>" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="delete_items()"
					    Authorized_ArtifactType="Task" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,TaskList_DeleteConfirm %>" />
			    </div>
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh%>" GlyphIconCssClass="mr3 fas fa-sync"
					    ClientScriptServerControlId="grdTaskList" ClientScriptMethod="load_data()" />
			    </div>
                <div class="btn-group priority1" role="group">
                    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
					    Text="<%$Resources:Buttons,Filter%>" MenuWidth="125px" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="apply_filters()">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						    <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>" GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
						    <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>" GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
			    </div>
                <div class="btn-group priority4" role="group">	
                    <tstsc:DropMenu id="btnEdit" runat="server" GlyphIconCssClass="mr3 far fa-edit"
					    ClientScriptServerControlId="grdTaskList" ClientScriptMethod="edit_items()" Text="<%$Resources:Buttons,Edit %>" Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit">
                        <DropMenuItems>
                            <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_items()" Authorized_Permission="Create" Authorized_ArtifactType="Task" />
                        </DropMenuItems>
                    </tstsc:DropMenu>
                    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
					    Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" ClientScriptServerControlId="grdTaskList">
					    <DropMenuItems>
                    	    <tstsc:DropMenuItem Name="Export" Value="<%$Resources:Dialogs,Global_ExportToProject %>" GlyphIconCssClass="mr3 fas fa-sign-out-alt" ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Global_ExportItems, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                    	    <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="get_element;print_items()" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                            <tstsc:DropMenuItem Divider="true" />
                            <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Incident" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('excel')" />
				            <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Incident" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('word')" />
				            <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Incident" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('pdf')" />    
					    </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="toggle_visibility" />
				</div>
		    </div>
            <div class="bg-near-white-hover py2 px3 br2 transition-all">
			    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                <asp:Label ID="lblVisibleTaskCount" Runat="server" Font-Bold="True" />
			    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                <asp:Label ID="lblTotalTaskCount" Runat="server" Font-Bold="True" />
			    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,TaskList_TasksForProject %>" />
                <asp:Label runat="server" ID="txtFolderInfo" CssClass="badge v-mid mx3" />
                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />

                <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                        <span class="btn-group priority1 pull-right pull-left-xs" role="group">
                        <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -4)) %>' runat="server" title="<%$ Resources:Main,Global_List %>">
                            <span class="fas fa-list"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -5)) %>' runat="server" title="<%$ Resources:Main,Global_Board %>">
                            <span class="fas fa-align-left rotate90"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -9)) %>' runat="server" title="<%$ Resources:Main,Global_Gantt %>">
                            <span class="fas fa-align-left"></span>
                        </a>
                    </span>
                </asp:PlaceHolder>  
            </div>
			<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
            <div class="main-content">
				<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:SortedGrid 
                    AllowColumnPositioning="true" 
				    Authorized_ArtifactType="Task" 
                    Authorized_Permission="BulkEdit" 
				    ConcurrencyEnabled="true" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    EditRowCssClass="Editing" 
                    ErrorMessageControlId="divMessage"
                    FilterInfoControlId="lblFilterInfo"
                    FolderItemImage="Folder.svg"
                    HeaderCssClass="Header"
                    ID="grdTaskList" 
                    ItemImage="artifact-Task.svg" 
                    NegativePrimaryKeysDisabled="false"
                    AlternateItemImage="artifact-PullRequest.svg"
				    RowCssClass="Normal"
				    runat="server" 
                    SelectedRowCssClass="Highlighted" 
				    SubHeaderCssClass="SubHeader" 
                    TotalCountControlId="lblTotalTaskCount"
				    VisibleCountControlId="lblVisibleTaskCount" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Dialogs,TaskList_NewTask%>" CommandName="insert_item" CommandArgument="Task" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-clone" Caption="<%$Resources:Dialogs,TaskList_CloneTask%>" CommandName="clone_items" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Dialogs,TaskList_DeleteTask%>" CommandName="delete_items" Authorized_ArtifactType="Task" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,TaskList_DeleteConfirm %>" />
                    </ContextMenuItems>    
                </tstsc:SortedGrid>
				<br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        function lnkAllFolders_click()
        {
            var trvFolders = $find('<%=trvFolders.ClientID %>');
            trvFolders.nodeClick(null, '', '');
        }
        //Handles items being dragged to a folder
        function trvFolders_iconDragStart(evt, context)
        {
            //get a handle to the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');

            //Tell the treeview to handle the drag
            trvFolders.start_dragging(context.artifactIds, context.img, context.artifactName, trvFolders_iconDragEnd);
        }
        function trvFolders_iconDragEnd()
        {
            //Simply reload the grid
            var ajaxControl = $find('<%=grdTaskList.ClientID %>');
            ajaxControl.load_data();
        }

        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        //Prints the selected items
        function print_items(format)
        {
            //Get the items and convert to csv
            var grdTaskList = $find('<%=grdTaskList.ClientID %>');
            var items = grdTaskList.get_selected_items();
            if (items.length < 1)
            {
                alert(resx.Global_SelectOneCheckBoxForCommand);
            }
            else
            {
				var folderId = null;
				for (var i = 0; i < items.length; i++) {
					if (parseInt(items[i]) < 0) {
						folderId = -items[i];
					}
				}
				//Cannot select a folder and other items
				if (folderId && items.length > 1) {
					alert(resx.Global_CannotSelectMoreThanOneFolder);
					return;
                }

				var reportToken;
				var filter;
				var artifacts;

				//Handle folder case
				if (folderId) {
					//Open the report for the specified format
					if (format == 'excel') {
						reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskSummary%>";
                        filter = "&fl_14=";
                    }
                    else
                    {
						reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskDetailed%>";
						filter = "&fl_15=";
					}
					artifacts = folderId;
				}
				else {

                    var item_list = globalFunctions.convertIntArrayToString(items);
                    //Open the report for the specified format
                    var reportToken;
                    var filter;
                    if (format == 'excel')
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskSummary%>";
                        filter = "&af_14_95=";
                    }
                    else
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskDetailed%>";
                        filter = "&af_15_95=";
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
            var ajaxControl = $find('<%=grdTaskList.ClientID %>');
            ajaxControl.load_data();
        }
        function trvFolders_folderChanged(folderId)
        {
            //Set the standard filter and reload
            var grdTaskList = $find('<%=grdTaskList.ClientID %>');
            var args = {};
            args['_FolderId'] = globalFunctions.serializeValueInt(folderId);
            grdTaskList.set_standardFilters(args);
            grdTaskList.load_data();
        }
        function grdTaskList_focusOn(nodeId)
        {
            //It means the folder may have changed, so reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.set_selectedNode(nodeId);
            trvFolders.load_data(true);
        }

        function grdTaskList_loaded()
        {
            loadGraphs();
        }

        /* Charts */
        var sidepanelCharts = new Object;

        function loadGraphs()
        {
            //Get the release (if we have one)
            var releaseId = null;
            var grdTaskList = $find('<%=this.grdTaskList.ClientID%>');
            var gridData = grdTaskList.get_dataSource();
            if (gridData.items[0].Fields.ReleaseId && gridData.items[0].Fields.ReleaseId.intValue)
            {
                releaseId = gridData.items[0].Fields.ReleaseId.intValue;
            }

            //Load the task progress
            Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_RetrieveProgress(SpiraContext.ProjectId, releaseId, function (data)
            {
                //Success
                var cleanedData = prepDataForC3(data);

                sidepanelCharts.tkTaskProgress = c3.generate({
                    bindto: d3.select('#tkTaskProgress'),
                    data: {
                        columns: cleanedData.columns,
                        type: "donut",
                        colors: cleanedData.colors
                    },
                    donut: {
                        label: {
                            format: function (value, ratio, id)
                            {
                                return Math.round(ratio * 100.0) + '%';
                            }
                        }
                    }
                });
            }, function (ex)
            {
                //Fail quietly
            });

            //Load the tasks burndown graph
            Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_RetrieveBurndown(SpiraContext.ProjectId, releaseId, function (data)
            {
                //Series types (snapshot charts only)
                var _seriesType_Bar = 1;
                var _seriesType_Line = 2;
                var _seriesType_CumulativeBar = 3;

                //Success
                var res = new Object();
                if (data && data.Series && data.XAxis)
                {
                    var axisPoints = data.XAxis;
                    res.groups = new Array();
                    res.categories = new Array();
                    res.columns = new Array;
                    res.colors = new Object;
                    res.types = new Object;
                    for (var j = 0; j < data.Series.length; j++)
                    {
                        var groupName = data.Series[j].Caption;
                        if (data.Series[j].Color)
                        {
                            res.colors[groupName] = '#' + data.Series[j].Color;
                        }

                        //See if we have a bar or line renderer to override the default
                        if (data.Series[j].Type)
                        {
                            if (data.Series[j].Type == _seriesType_Bar)
                            {
                                res.groups.push(groupName);
                            }
                            if (data.Series[j].Type == _seriesType_CumulativeBar)
                            {
                                res.groups.push(groupName);
                            }
                            if (data.Series[j].Type == _seriesType_Line)
                            {
                                //Override type to Line
                                res.types[groupName] = 'line';
                            }
                        }

                        var column = new Array();
                        column.push(groupName);
                        for (var i = 0; i < axisPoints.length; i++)
                        {
                            var axisPoint = data.XAxis[i];
                            if (data.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined)
                            {
                                var category = axisPoint.StringValue;
                                var value = data.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
                                //Only add the x-axis categories for the first series
                                if (j == 0)
                                {
                                    res.categories.push(category);
                                }
                                //If this series is a cumulative bar, need to subtract the total of other stacked bars so far
                                //So that it's relative to the existing stacked bars
                                if (data.Series[j].Type == _seriesType_CumulativeBar)
                                {
                                    for (var k = i - 1; k >= 0; k--)
                                    {
                                        var itemValue = data.Series[k].Values[globalFunctions.keyPrefix + axisPoint.Id];;
                                        value -= itemValue;
                                        //If we hit a cumulative bar, don't subtract any further bars,
                                        //but check to make sure that the cumulative bar was not exceeded by the constituent items
                                        if (data.Series[k].Type == _seriesType_CumulativeBar)
                                        {
                                            var childTotal = 0;
                                            for (var m = k - 1; m >= 0; m--)
                                            {
                                                childTotal += data.Series[m].Values[globalFunctions.keyPrefix + axisPoint.Id];
                                            }
                                            if (childTotal > itemValue)
                                            {
                                                var delta = childTotal - itemValue;
                                                value -= delta;
                                            }
                                            break;
                                        }
                                    }

                                    //Bars can never be negative (lines can)
                                    if (data.Series[j].Type == _seriesType_CumulativeBar && value < 0)
                                    {
                                        value = 0;
                                    }
                                }
                                column.push(value);
                            }
                        }
                        res.columns.push(column);
                    }
                }

                sidepanelCharts.tkTaskBurndown = c3.generate({
                    bindto: d3.select('#tkTaskBurndown'),
                    data: {
                        columns: res.columns,
                        type: "bar",
                        types: res.types,
                        colors: res.colors,
                        groups: [res.groups]
                    },
                    axis: {
                        x: {
                            type: 'category',
                            tick: {
                                rotate: 75,
                                multiline: false
                            },
                            categories: res.categories
                        }
                    }
                });
            }, function (ex)
            {
                //Fail quietly
            });

            function prepDataForC3(data)
            {
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
