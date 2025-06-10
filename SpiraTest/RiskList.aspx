<%@ Page language="c#" validateRequest="false" Codebehind="RiskList.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.RiskList" MasterPageFile="~/MasterPages/Main.Master" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlQuickFilters" runat="server" HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" MinWidth="100" MaxWidth="500"
                ErrorMessageControlId="divMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <tstuc:QuickFilterPanel
                    ID="ucQuickFilterPanel" 
                    runat="server"
                    ArtifactType="Risk"
                    AjaxServerControlId="grdRiskList"
                    DisplayReleases="false" />
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
                    id="rkChartExposures"
                    ></div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="btn-group priority2 priority1-first-child" role="group">
				    <tstsc:DropMenu 
                        id="btnNewRisk" 
                        runat="server" 
                        GlyphIconCssClass="mr3 fas fa-plus"
					    Text="<%$Resources:Dialogs,RiskList_NewRisk %>" 
                        Authorized_ArtifactType="Risk" 
                        Authorized_Permission="Create" 
                        ClientScriptMethod="grdRiskList_SaveAndNew()"
                        />
				    <tstsc:DropMenu id="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt"
				        ClientScriptServerControlId="grdRiskList" ClientScriptMethod="delete_items()"
					    Authorized_ArtifactType="Risk" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,RiskList_DeleteConfirm %>" />
			    </div>
			    <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync"
					    ClientScriptServerControlId="grdRiskList" ClientScriptMethod="load_data()" />
			    </div>
                <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
					    MenuWidth="125px" ClientScriptServerControlId="grdRiskList" ClientScriptMethod="apply_filters()">
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
					    ClientScriptServerControlId="grdRiskList" ClientScriptMethod="edit_items()" Text="<%$Resources:Buttons,Edit %>" Authorized_ArtifactType="Risk" Authorized_Permission="BulkEdit">
                        <DropMenuItems>
                            <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_items()" Authorized_Permission="Create" Authorized_ArtifactType="Risk" />
                        </DropMenuItems>
                    </tstsc:DropMenu>
            	    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
					    Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" ClientScriptServerControlId="grdRiskList">
					    <DropMenuItems>
                    	    <tstsc:DropMenuItem Name="Export" Value="<%$Resources:Dialogs,Global_ExportToProject %>" GlyphIconCssClass="mr3 fas fa-sign-out-alt" ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Global_ExportItems, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                    	    <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="get_element;print_items()" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                            <tstsc:DropMenuItem Divider="true" />
                            <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Risk" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('excel')" />
				            <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Risk" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('word')" />
				            <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Risk" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('pdf')" />
					    </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
			    <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdRiskList" ClientScriptMethod="toggle_visibility" />
                </div>
            </div>
			<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
            <div class="bg-near-white-hover py2 px3 br2 transition-all">
			    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                <asp:Label ID="lblVisibleRiskCount" Runat="server" Font-Bold="True" />
			    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                <asp:Label ID="lblTotalRiskCount" Runat="server" Font-Bold="True" />
			    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,RiskList_RisksForProject %>" />.
                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
            </div>
            <div class="main-content">
				<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                    <asp:ServiceReference Path="~/Services/Ajax/RisksService.svc" />  
                    </Services>  
                </tstsc:ScriptManagerProxyEx>
                <tstsc:SortedGrid 
                    AllowColumnPositioning="true"
				    Authorized_ArtifactType="Risk" 
                    Authorized_Permission="BulkEdit"
				    ConcurrencyEnabled="true" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    EditRowCssClass="Editing" 
                    ErrorMessageControlId="divMessage"
                    FilterInfoControlId="lblFilterInfo" 
                    HeaderCssClass="Header"
                    ID="grdRiskList" 
                    ItemImage="artifact-Risk.svg"
				    RowCssClass="Normal" 
				    runat="server" 
                    SelectedRowCssClass="Highlighted" 
				    SubHeaderCssClass="SubHeader" 
                    TotalCountControlId="lblTotalRiskCount"
				    VisibleCountControlId="lblVisibleRiskCount" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Dialogs,RiskList_NewRisk%>" ClientScriptMethod="grdRiskList_NewRisk"  Authorized_ArtifactType="Risk" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-clone" Caption=" <%$Resources:Dialogs,RiskList_CloneRisk%>" CommandName="clone_items" Authorized_ArtifactType="Risk" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Risk" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Dialogs,RiskList_DeleteRisk%>" CommandName="delete_items" Authorized_ArtifactType="Risk" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,RiskList_DeleteConfirm %>" />
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
        //Redirects to the new risk page
        function grdRiskList_NewRisk()
        {
            var url = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Risks, ProjectId, -1)) %>';
            window.location = url;
        }

        //Handles click new risk - save changes to the grid if any are being made
        function grdRiskList_SaveAndNew() {
            // get the grid js object and check if it is in edit mode
            var grdRiskList = $find('<%=grdRiskList.ClientID %>');
            if (grdRiskList._isInEdit) {
                //update the grid edits
                //Pass in a callback to redirect to the new RK page - only if the updates have succeeded
                grdRiskList._onUpdateClick(grdRiskList_NewRisk, null);
            } else {
                //Else immediately open the new incident page
                grdRiskList_NewRisk();
            }
        }

        //Prints the selected items
        function print_items(format)
        {
            //Get the items and convert to csv
            var grdRiskList = $find('<%=grdRiskList.ClientID %>');
            var items = grdRiskList.get_selected_items();
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
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RiskSummary%>";
                    filter = "&af_22_200=";
                }
                else
                {
                    reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RiskDetailed%>";
                    filter = "&af_23_200=";
                }

                //Open the report for the specified format
                globalFunctions.launchStandardReport(reportToken, format, filter, item_list);
             }
        }

        /* Charts */
        var sidepanelCharts = new Object;

        document.addEventListener("DOMContentLoaded", function ()
        {
            //Load the count by exposure graph
            Inflectra.SpiraTest.Web.Services.Ajax.RisksService.Risk_RetrieveCountByExposure(SpiraContext.ProjectId, function (data)
            {
                //Success
                var cleanedData = prepDataForC3(data);

                sidepanelCharts.chartExposures = c3.generate({
                    bindto: d3.select('#rkChartExposures'),
                    data: {
                        columns: cleanedData.columns,
                        type: "donut",
                        colors: cleanedData.colors
                    },
                    donut: {
                        label: {
                            format: function (value, ratio, id) {
                                return d3.format('d')(id);
                            }
                        }
                    }
                });
            }, function(ex) {
                //Fail quietly
            });

            function prepDataForC3(data) {
                var res = new Object();
                if (data && data.Series)
                {
                    res.columns = new Array;
                    res.colors = new Object;
                    for (var i = 0; i < data.Series.length; i++)
                    {
                        var column = [data.Series[i].Caption, data.Series[i].Value];
                        res.columns.push(column);
                        res.colors[data.Series[i].Caption] = (data.Series[i].Color == null ? "#f1f1f1" : data.Series[i].Color);
                    }
                }
                return res;
            }

        });
    </script>
</asp:Content>
