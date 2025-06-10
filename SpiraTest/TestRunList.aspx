<%@ Page Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="TestRunList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestRunList" Title="Untitled Page" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel SkinID="RelativeSidebarPanel" ID="pnlQuickFilters" runat="server" HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" MinWidth="100" MaxWidth="500"
                ErrorMessageControlId="divMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="TestRun" AjaxServerControlId="grdTestRunList" ReleaseFilterField="ReleaseId" />
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
                    id="c3_testRunProgress"
                    ></div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="delete_items()"
					    Authorized_ArtifactType="TestRun" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,TestRunList_DeleteConfirm %>" />
                </div>
			    <div class="btn-group priority2" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync"
					    ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="load_data()" />
                </div>
			    <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
					    Text="<%$Resources:Buttons,Filter %>" MenuWidth="125px" ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="apply_filters()">
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
						    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						    <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>" GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
						    <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>" GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
			    </div>
			    <div class="btn-group" role="group">
            	    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog" Text="<%$Resources:Buttons,Tools %>" MenuWidth="150px">
					    <DropMenuItems>
                    	    <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="print_items()" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
                            <tstsc:DropMenuItem Divider="true" />
                            <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="TestRun" Authorized_Permission="View" ClientScriptMethod="print_items('excel')" />
				            <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="TestRun" Authorized_Permission="View" ClientScriptMethod="print_items('word')" />
				            <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="TestRun" Authorized_Permission="View" ClientScriptMethod="print_items('pdf')" />    
					    </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
			    <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="toggle_visibility" />
                </div>
	        </div>
            <div class="bg-near-white-hover py2 px3 br2 transition-all">
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" /> <asp:Label ID="lblVisibleTestRunCount" Runat="server" Font-Bold="True" /> <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_OutOf %>" /> <asp:Label ID="lblTotalTestRunCount" Runat="server" Font-Bold="True" /> <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,TestRunList_TestRunsForProject %>" />.
                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
			</div>
			<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
	        <div class="main-content">
			    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>  
                <tstsc:SortedGrid ID="grdTestRunList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
				    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
				    RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-TestRun.svg"
				    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestRunService"
				    Authorized_ArtifactType="TestRun" Authorized_Permission="BulkEdit"
				    VisibleCountControlId="lblVisibleTestRunCount" TotalCountControlId="lblTotalTestRunCount"
                    AllowColumnPositioning="true" FilterInfoControlId="lblFilterInfo">
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="TestRun" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem  GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="TestRun" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,TestRunList_DeleteConfirm %>" />
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
       var resx = Inflectra.SpiraTest.Web.GlobalResources;
       //Prints the selected items
       function print_items(format)
       {
           //Get the items and convert to csv
           var grdTestRunList = $find('<%=grdTestRunList.ClientID %>');
           var items = grdTestRunList.get_selected_items();
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
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestRunSummary%>";
                    filter = "&af_10_100=";
                }
                else
                {
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestRunDetailed%>";
                    filter = "&af_11_100=";
                }

                //Open the report for the specified format
                globalFunctions.launchStandardReport(reportToken, format, filter, item_list);
            }
       }
       function grdTestRunList_loaded()
       {
           loadGraphs();
       }

       /* Charts */
       var sidepanelCharts = new Object;

       function loadGraphs()
       {
           //Check the date format
           var dateFormatMonthFirst = <%=DateFormatMonthFirst.ToString().ToLowerInvariant()%>;

           //Get the release (if we have one)
           var releaseId = null;
           var grdTestRunList = $find('<%=this.grdTestRunList.ClientID%>');
           var gridData = grdTestRunList.get_dataSource();
           if (gridData.items[0].Fields.ReleaseId && gridData.items[0].Fields.ReleaseId.intValue)
           {
               releaseId = gridData.items[0].Fields.ReleaseId.intValue;
           }

           Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.TestRun_RetrieveProgress(SpiraContext.ProjectId, releaseId, function (data)
           {
               //Success
               var cleanedData = prepDataForC3(data);

               sidepanelCharts.testRunProgress = c3.generate({
                   bindto: d3.select('#c3_testRunProgress'),
                   data: {
                       x: 'x',
                       columns: cleanedData.columns,
                       colors: cleanedData.colors,
                       type: "area-spline",
                       groups: cleanedData.groups
                   },
                   grid: {
                       y: {
                           show: false
                       }
                   },
                   axis: {
                       x: {
                           show: false,
                           type: 'timeseries',
                           tick: {
                               format: function (x)
                               {
                                   if (dateFormatMonthFirst)
                                       return (x.getMonth() + 1) + '/' + x.getDate();
                                   else
                                       return x.getDate() + '/' + (x.getMonth() + 1);
                               }
                           }
                       }
                   }
               });
           }, function (ex)
           {
               //Fail quietly
           });
       }

       function prepDataForC3(data)
       {
           var res = new Object();
           if (data && data.Series)
           {
               res.columns = [['x']];
               res.colors = new Object;
               res.groups = [[]];
               for (var j = 0; j < data.XAxis.length; j++)
               {
                   //Convert the WCF date
                   var date = new Date(parseInt(data.XAxis[j].DateValue.substr(6)));
                   res.columns[0].push(date);
               }
               for (var i = 0; i < data.Series.length; i++)
               {
                   var column = [data.Series[i].Caption];
                   var values = data.Series[i].Values;
                   res.groups[0].push(data.Series[i].Caption);
                   for (var key in values)
                   {
                       if (key != '__type')
                       {
                           column.push(values[key]);
                       }
                   }
                   res.columns.push(column);
                   res.colors[data.Series[i].Caption] = "#" + (data.Series[i].Color == null ? "f1f1f1" : data.Series[i].Color);
               }
           }
           return res;
       }
    </script>
</asp:Content>
