<%@ Page 
    AutoEventWireup="True" 
    Codebehind="Requirements.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.Requirements" 
    language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    validateRequest="false" 
%>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
<div class="panel-container flex">
    <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
        <tstsc:SidebarPanel 
            SkinID="RelativeSidebarPanel"
            ErrorMessageControlId="divMessage" 
            HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>"
            ID="pnlQuickFilters" 
            runat="server" 
            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
            >
            <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="Requirement" AjaxServerControlId="grdRequirementsMatrix" />
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
                id="rqRequirementsTestCoverage"
                ></div>
            <div 
                class="u-chart db"
                id="rqRequirementsBurndown"
                ></div>

        </tstsc:SidebarPanel>
    </div>


    <div class="main-panel pl4 grow-1">
        <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
            <div class="btn-group priority3 priority1-first-child" role="group">
			    <tstsc:DropMenu id="btnInsert" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
					ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="insert_item('Requirement')" Text="<%$Resources:Buttons,Insert %>"
                    Authorized_ArtifactType="Requirement" Authorized_Permission="Create">
                	<DropMenuItems>
						<tstsc:DropMenuItem runat="server" Name="Requirement" Value="<%$Resources:Dialogs,RequirementList_NewRequirement %>" ImageUrl="Images/artifact-Requirement.svg" ClientScriptMethod="insert_item('Requirement')" />
						<tstsc:DropMenuItem runat="server" Name="ChildRequirement" Value="<%$Resources:Dialogs,RequirementList_ChildRequirement %>" ImageUrl="Images/action-InsertChildRequirement.svg" ClientScriptMethod="insert_item('ChildRequirement')" />
					</DropMenuItems>
                </tstsc:DropMenu>
			    <tstsc:DropMenu id="btnDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt"
					ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="delete_items()" Text="<%$Resources:Buttons,Delete %>"
                    Authorized_ArtifactType="Requirement" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,RequirementsList_DeleteConfirm %>" />
			</div>
			<div class="btn-group priority3" role="group">
			    <tstsc:DropMenu id="btnIndent" runat="server" GlyphIconCssClass="mr3 glyphicon fas fa-arrow-right"
					ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="indent_items()" Text="<%$Resources:Buttons,Indent %>" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
			    <tstsc:DropMenu id="btnOutdent" runat="server" GlyphIconCssClass="mr3 glyphicon fas fa-arrow-left"
					ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="outdent_items()" Text="<%$Resources:Buttons,Outdent %>" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
			    <tstsc:DropMenu id="btnRefresh" runat="server" AlternateText="Refresh" GlyphIconCssClass="mr3 fas fa-sync" Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="load_data()" />
            </div>
			<div class="btn-group priority1" role="group">
				<tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="False" DataTextField="Value" DataValueField="Key" CssClass="DropDownList" ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="expand_to_level" Width="110px" />
                <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
					ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="apply_filters()">
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
					Text="<%$Resources:Buttons,Edit%>" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" MenuWidth="140px"
                    ClientScriptMethod="edit_items()"
					ClientScriptServerControlId="grdRequirementsMatrix">
					<DropMenuItems>
                        <tstsc:DropMenuItem Name="Edit" Value="<%$Resources:Buttons,EditItems%>" GlyphIconCssClass="mr3 far fa-edit" ClientScriptMethod="edit_items()" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
						<tstsc:DropMenuItem Divider="true" />
                        <tstsc:DropMenuItem Name="Copy" Value="<%$Resources:Buttons,CopyItems%>" GlyphIconCssClass="mr3 fas fa-copy" ClientScriptMethod="copy_items()" />
						<tstsc:DropMenuItem Name="Cut" Value="<%$Resources:Buttons,CutItems%>" GlyphIconCssClass="mr3 fas fa-cut" ClientScriptMethod="cut_items()" />
						<tstsc:DropMenuItem Name="Paste" Value="<%$Resources:Buttons,PasteItems%>" GlyphIconCssClass="mr3 far fa-clipboard" ClientScriptMethod="paste_items()" />
					</DropMenuItems>
				</tstsc:DropMenu>
            	<tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
					Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" ClientScriptServerControlId="grdRequirementsMatrix">
					<DropMenuItems>
                    	<tstsc:DropMenuItem Name="Export" Value="<%$Resources:Dialogs,Global_ExportToProject %>" GlyphIconCssClass="mr3 fas fa-sign-out-alt" ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Global_ExportItems, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                    	<tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="get_element;print_items()" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                        <tstsc:DropMenuItem Divider="true" />
						<tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('excel')" />
				        <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('word')" />
				        <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="get_element;print_items('pdf')" />
                        <tstsc:DropMenuItem Divider="true" />
                        <tstsc:DropMenuItem Name="CreateTestCase" Value="<%$Resources:Dialogs,Requirements_CreateTestCases %>" ImageUrl="Images/artifact-TestCase.svg" Authorized_ArtifactType="TestCase" Authorized_Permission="Create" ClientScriptMethod="custom_list_operation('CreateTestCase', null, Inflectra.SpiraTest.Web.GlobalResources.Requirements_SuccessfullyCreatedTestCases)" />
						<tstsc:DropMenuItem Name="CreateTestSet" Value="<%$Resources:Dialogs,Requirements_CreateTestSet %>" ImageUrl="Images/artifact-TestSet.svg" Authorized_ArtifactType="TestSet" Authorized_Permission="Create" ClientScriptMethod="custom_list_operation('CreateTestSet', null, Inflectra.SpiraTest.Web.GlobalResources.Requirements_SuccessfullyCreatedTestSet)" />
                        <tstsc:DropMenuItem Divider="true" />
						<tstsc:DropMenuItem Name="FocusOn" Value="<%$Resources:Dialogs,Requirements_FocusOn %>" GlyphIconCssClass="mr3 far fa-eye" ClientScriptMethod="custom_list_operation('FocusOn', null, null)" />				        
					</DropMenuItems>
                </tstsc:DropMenu>
            </div>
            <div class="btn-group" role="group">
				<tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdRequirementsMatrix" ClientScriptMethod="toggle_visibility" />
            </div>
		</div>
        <div class="bg-near-white-hover py2 px3 br2 transition-all">
		    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
            <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
		    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
            <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
		    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Requirements_RequirementsForProject %>" />.
            <tstsc:LabelEx ID="lblFilterInfo" runat="server" />        

            <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                <span class="btn-group priority1 pull-right pull-left-xs" style="margin-left:5px" role="group">
                    <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                        <span class="fas fa-indent"></span>
                    </a>
                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -4) %>' runat="server" title="<%$ Resources:Main,Global_List %>">
                        <span class="fas fa-list"></span>
                    </a>
                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -5) %>' runat="server" title="<%$ Resources:Main,Global_Board %>">
                        <span class="fas fa-align-left rotate90"></span>
                    </a>
                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -7) %>' runat="server" title="<%$ Resources:Main,Global_Document %>">
                        <span class="fas fa-paragraph"></span>
                    </a>
                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                        <span class="fas fa-project-diagram"></span>
                    </a>
                </span>
            </asp:PlaceHolder>  

            <asp:PlaceHolder ID="plcProjectSelector" runat="server">
                    <span class="btn-group priority1 radio-group pull-right pull-left-xs" role="group">
                    <asp:Label ID="lblCurrentProjects" runat="server" AssociatedControlID="radCurrentProjects" CssClass="btn btn-default">
                        <tstsc:RadioButtonEx ID="radCurrentProjects" runat="server" GroupName="ProjectFilter" AutoPostBack="false" ClientScriptMethod="grdRequirementsMatrix_changeProjectFilter('current')" />
						<asp:Localize ID="Localize5" runat="server" Text="<%$ Resources:Main,ProjectList_CurrentProject %>" />
                    </asp:Label>
                    <asp:Label ID="lblAllProjects" runat="server" AssociatedControlID="radAllProjects" CssClass="btn btn-default">
						<tstsc:RadioButtonEx ID="radAllProjects" runat="server" GroupName="ProjectFilter" AutoPostBack="false" ClientScriptMethod="grdRequirementsMatrix_changeProjectFilter('all')" />
						<asp:Localize ID="Localize4" runat="server" Text="<%$ Resources:Main,ProjectList_AllProjects %>" />
                    </asp:Label>
                </span>
            </asp:PlaceHolder>  

            <span class="clearfix"></span>              
        </div>
        <tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
        <div class="main-content">
			<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                <Services>  
                <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
                </Services>  
            </tstsc:ScriptManagerProxyEx>  
			<tstsc:HierarchicalGrid 
                AllowColumnPositioning="true"
                AlternateItemImage="artifact-UseCase.svg"
				Authorized_ArtifactType="Requirement" 
                Authorized_Permission="BulkEdit" 
                ConcurrencyEnabled="true"
                CssClass="DataGrid DataGrid-no-bands" 
                EditRowCssClass="Editing" 
                ErrorMessageControlId="divMessage"
                FilterInfoControlId="lblFilterInfo"
                HeaderCssClass="Header"
                ID="grdRequirementsMatrix" 
                ItemImage="artifact-Requirement.svg" 
				RowCssClass="Normal" 
				runat="server" 
                SelectedRowCssClass="Highlighted" 
				SubHeaderCssClass="SubHeader" 
                SummaryItemImage="artifact-RequirementSummary.svg" 
                TotalCountControlId="lblTotalCount" 
                VisibleCountControlId="lblVisibleCount" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" 
                >
                <ContextMenuItems>
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
					<tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-eye" Caption="<%$Resources:Dialogs,Requirements_FocusOn %>" CommandName="custom_list_operation" CommandArgument="FocusOn" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                    <tstsc:ContextMenuItem Divider="True" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Buttons,Insert%>" CommandName="insert_item" CommandArgument="Requirement" Authorized_ArtifactType="Requirement" Authorized_Permission="Create" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete%>" CommandName="delete_items" Authorized_ArtifactType="Requirement" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,RequirementsList_DeleteConfirm %>" />
                    <tstsc:ContextMenuItem Divider="True" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 glyphicon fas fa-arrow-right" Caption="<%$Resources:Buttons,Indent%>" CommandName="indent_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 glyphicon fas fa-arrow-left" Caption="<%$Resources:Buttons,Outdent%>" CommandName="outdent_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem Divider="True" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Buttons,CopyItems%>" CommandName="copy_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-cut" Caption="<%$Resources:Buttons,CutItems%>" CommandName="cut_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-clipboard" Caption="<%$Resources:Buttons,PasteItems%>" CommandName="paste_items" Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" />
                    <tstsc:ContextMenuItem Divider="True" />
                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-print" Caption="<%$Resources:Dialogs,Global_PrintItems%>" ClientScriptMethod="print_items()" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                </ContextMenuItems>     
            </tstsc:HierarchicalGrid>
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
		function print_items(format) {
			//Get the items and convert to csv
			var grdRequirementsMatrix = $find('<%=grdRequirementsMatrix.ClientID %>');
			var items = grdRequirementsMatrix.get_selected_items();
			if (items.length < 1) {
				alert(resx.Global_SelectOneCheckBoxForCommand);
			}
			else {
				var item_list = globalFunctions.convertIntArrayToString(items);
				//Open the report for the specified format
				var reportToken;
				var filter;
				if (format == 'excel') {
					reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RequirementSummary%>";
					filter = "&af_2_85=";
				}
				else {
					reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RequirementDetailed%>";
                    filter = "&af_3_85=";
                }

                //Open the report for the specified format
                globalFunctions.launchStandardReport(reportToken, format, filter, item_list);
            }
        }

        function grdRequirementsMatrix_changeProjectFilter(state)
        {
            if (state == 'all')
            {
                $('#<%=this.lblAllProjects.ClientID%>').attr('data-checked', 'checked');
                $('#<%=this.lblCurrentProjects.ClientID%>').attr('data-checked', '');
                $find('<%=this.grdRequirementsMatrix.ClientID%>').custom_operation('Update_Project_Filter', 'all');
            }
            else
            {
                $('#<%=this.lblAllProjects.ClientID%>').attr('data-checked', '');
                $('#<%=this.lblCurrentProjects.ClientID%>').attr('data-checked', 'checked');
                $find('<%=this.grdRequirementsMatrix.ClientID%>').custom_operation('Update_Project_Filter', 'current');
            }
        }
        function grdRequirementsMatrix_loaded()
        {
            loadGraphs();
        }

        /* Charts */
        var sidepanelCharts = new Object;

        function loadGraphs()
        {
            //Get the release (if we have one)
            var releaseId = null;
            var grdRequirementsMatrix = $find('<%=this.grdRequirementsMatrix.ClientID%>');
			var gridData = grdRequirementsMatrix.get_dataSource();
			if (gridData.items[0].Fields.ReleaseId && gridData.items[0].Fields.ReleaseId.intValue) {
				releaseId = gridData.items[0].Fields.ReleaseId.intValue;
			}

			//Load the test coverage graph
			Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.Requirement_RetrieveTestCoverage(SpiraContext.ProjectId, releaseId, false, function (data) {
				//Success
				var cleanedData = prepDataForC3(data);

				sidepanelCharts.chartReqTestCoverage = c3.generate({
					bindto: d3.select('#rqRequirementsTestCoverage'),
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
			}, function (ex) {
				//Fail quietly
			});

			//Load the requirements burndown graph
			Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.Requirement_RetrieveBurndown(SpiraContext.ProjectId, releaseId, function (data) {
				//Series types (snapshot charts only)
				var _seriesType_Bar = 1;
				var _seriesType_Line = 2;
				var _seriesType_CumulativeBar = 3;

				//Success
				var res = new Object();
				if (data && data.Series && data.XAxis) {
					var axisPoints = data.XAxis;
					res.groups = new Array();
					res.categories = new Array();
					res.columns = new Array;
					res.colors = new Object;
					res.types = new Object;
					for (var j = 0; j < data.Series.length; j++) {
						var groupName = data.Series[j].Caption;
						if (data.Series[j].Color) {
							res.colors[groupName] = '#' + data.Series[j].Color;
						}

						//See if we have a bar or line renderer to override the default
						if (data.Series[j].Type) {
							if (data.Series[j].Type == _seriesType_Bar) {
								res.groups.push(groupName);
							}
							if (data.Series[j].Type == _seriesType_CumulativeBar) {
								res.groups.push(groupName);
							}
							if (data.Series[j].Type == _seriesType_Line) {
								//Override type to Line
								res.types[groupName] = 'line';
							}
						}

						var column = new Array();
						column.push(groupName);
						for (var i = 0; i < axisPoints.length; i++) {
							var axisPoint = data.XAxis[i];
							if (data.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id] != undefined) {
								var category = axisPoint.StringValue;
								var value = data.Series[j].Values[globalFunctions.keyPrefix + axisPoint.Id];
								//Only add the x-axis categories for the first series
								if (j == 0) {
									res.categories.push(category);
								}
								//If this series is a cumulative bar, need to subtract the total of other stacked bars so far
								//So that it's relative to the existing stacked bars
								if (data.Series[j].Type == _seriesType_CumulativeBar) {
									for (var k = i - 1; k >= 0; k--) {
										var itemValue = data.Series[k].Values[globalFunctions.keyPrefix + axisPoint.Id];;
										value -= itemValue;
										//If we hit a cumulative bar, don't subtract any further bars,
										//but check to make sure that the cumulative bar was not exceeded by the constituent items
										if (data.Series[k].Type == _seriesType_CumulativeBar) {
											var childTotal = 0;
											for (var m = k - 1; m >= 0; m--) {
												childTotal += data.Series[m].Values[globalFunctions.keyPrefix + axisPoint.Id];
											}
											if (childTotal > itemValue) {
												var delta = childTotal - itemValue;
												value -= delta;
											}
											break;
										}
									}

									//Bars can never be negative (lines can)
									if (data.Series[j].Type == _seriesType_CumulativeBar && value < 0) {
										value = 0;
									}
								}
								column.push(value);
							}
						}
						res.columns.push(column);
					}
				}

				sidepanelCharts.chartReqBurndown = c3.generate({
					bindto: d3.select('#rqRequirementsBurndown'),
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
			}, function (ex) {
				//Fail quietly
			});

			function prepDataForC3(data) {
				var res = new Object();
				if (data) {
					res.columns = new Array;
					res.colors = new Object;
					for (var i = 0; i < data.length; i++) {
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
