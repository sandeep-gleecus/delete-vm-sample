<%@ Page language="c#" validateRequest="false" Codebehind="TestCaseList.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.TestCaseList" MasterPageFile="~/MasterPages/Main.Master" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex flex-column-reverse-sm">
        <div class="side-panel hidden-sm-second-child sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlFolders" runat="server" HeaderCaption="<%$Resources:Main,Global_Folders %>" MinWidth="100" MaxWidth="500" data-panel="folder-tree"
                DisplayRefresh="true" ClientScriptServerControlId="trvFolders" ClientScriptMethod="load_data(true)" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <div class="Widget panel">
                    <tstsc:TreeView ID="trvFolders" runat="server" ClientScriptMethod="trvFolders_folderChanged" NodeLegendFormat="{0}" EditDescriptions="true"
                        LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="divMessage"
                        Authorized_Permission="BulkEdit" Authorized_ArtifactType="TestCase" AllowEditing="true" ItemName="<%$Resources:Fields,Folder %>"
                        NodeLegendControlId="txtFolderInfo" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" />
                </div>    
            </tstsc:SidebarPanel>
            <tstsc:SidebarPanel ID="pnlQuickFilters" runat="server" HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" MinWidth="100" MaxWidth="500"
                ErrorMessageControlId="divMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="TestCase" FolderLegendControlId="txtFolderInfo"
                    DisplayReleases="false"
                    AjaxServerControlId="grdTestCaseList" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService"  />
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
                    id="tcChartExecutionStatus"
                    ></div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                <div class="btn-group priority4 priority1-first-child" role="group">
				    <tstsc:DropMenu id="btnInsert" runat="server" GlyphIconCssClass="mr3 fas fa-plus" Text="<%$Resources:Dialogs,TestCaseList_NewTestCase %>"
					    AlternateText="Insert" Authorized_ArtifactType="TestCase" Authorized_Permission="Create" MenuWidth="175px"
					    ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="insert_item()" />
				    <tstsc:DropMenu id="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt"
					    Authorized_ArtifactType="TestCase" Authorized_Permission="Delete"
					    Confirmation="true" ConfirmationMessage="<%$Resources:Messages,TestCaseList_DeleteConfirm %>"
					    ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="delete_items()" />
                </div>
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync"
					    Text="<%$Resources:Buttons,Refresh %>"
					    ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="load_data()" />
                </div>
                <div class="btn-group priority3" role="group">
				    <tstsc:DropMenu id="btnFocusOn" runat="server" GlyphIconCssClass="mr3 far fa-folder-open"
					    Text="<%$Resources:Buttons,FocusOn %>"
					    ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="focus_on()" />
                </div>
                <div class="btn-group priority1" role="group">
				    <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
					    Text="<%$Resources:Buttons,Filter %>" MenuWidth="150px" ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="apply_filters()">
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
					    AlternateText="Edit" Authorized_ArtifactType="TestCase" Authorized_Permission="BulkEdit" MenuWidth="150px"
					    ClientScriptServerControlId="grdTestCaseList" PostBackOnClick="false" ClientScriptMethod="edit_items()" >
					    <DropMenuItems>
						    <tstsc:DropMenuItem Name="Copy" Value="<%$Resources:Buttons,CopyItems%>" GlyphIconCssClass="mr3 fas fa-copy" ClientScriptMethod="copy_items()" />
						    <tstsc:DropMenuItem Name="Cut" Value="<%$Resources:Buttons,CutItems%>" GlyphIconCssClass="mr3 fas fa-cut" ClientScriptMethod="cut_items()" />
						    <tstsc:DropMenuItem Name="Paste" Value="<%$Resources:Buttons,PasteItems%>" GlyphIconCssClass="mr3 far fa-clipboard" ClientScriptMethod="paste_items()" />
                            <tstsc:DropMenuItem Divider="true" />
						    <tstsc:DropMenuItem Name="Block" Value="<%$Resources:Dialogs,TestCases_BlockTestCases%>" GlyphIconCssClass="mr3 fas fa-ban"  ClientScriptMethod="custom_list_operation('Block', null, Inflectra.SpiraTest.Web.GlobalResources.TestCases_BlockSucceeded)" />
						    <tstsc:DropMenuItem Name="UnBlock" Value="<%$Resources:Dialogs,TestCases_UnBlockTestCases%>" GlyphIconCssClass="mr3 fas fa-square silver" ClientScriptMethod="custom_list_operation('UnBlock', null, Inflectra.SpiraTest.Web.GlobalResources.TestCases_UnBlockSucceeded)" />
					    </DropMenuItems>
				    </tstsc:DropMenu>
				    <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog" PostBackOnClick="false"
					    Text="<%$Resources:Buttons,Tools %>" MenuWidth="175px" ClientScriptServerControlId="grdTestCaseList">
					    <DropMenuItems>
					        <tstsc:DropMenuItem 
                                Name="Execute" 
                                Value="<%$Resources:Dialogs,TestCaseList_ExecuteTests%>" 
                                GlyphIconCssClass="mr3 fas fa-play" 
                                Authorized_ArtifactType="TestRun" 
                                Authorized_Permission="Create" 
                                ClientScriptMethod="prototype; grdTestCaseList_execute()" 
                                runat="server"
                                id="btnTools_execute"
                                />
						    <tstsc:DropMenuItem 
                                Name="Export" 
                                runat="server"
                                Value="<%$Resources:Dialogs,Global_ExportToProject %>" 
                                GlyphIconCssClass="mr3 fas fa-sign-out-alt" 
                                ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Global_ExportItems, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)" 
                                />
                    	    <tstsc:DropMenuItem 
                                Name="Print" 
                                runat="server"
                                Value="<%$Resources:Dialogs,Global_PrintItems %>" 
                                GlyphIconCssClass="mr3 fas fa-print" 
                                ClientScriptMethod="get_element;print_items()" 
                                Authorized_ArtifactType="TestCase" 
                                Authorized_Permission="View" 
                                />
                            <tstsc:DropMenuItem Divider="true" runat="server"/>
                            <tstsc:DropMenuItem 
                                Name="ExportToExcel" 
                                runat="server"
                                Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                ImageUrl="Images/Filetypes/Excel.svg" 
                                Authorized_ArtifactType="TestCase" 
                                Authorized_Permission="View" 
                                ClientScriptMethod="get_element;print_items('excel')" 
                                />
				            <tstsc:DropMenuItem 
                                Name="ExportToWord" 
                                runat="server"
                                Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                ImageUrl="Images/Filetypes/Word.svg" 
                                Authorized_ArtifactType="TestCase" 
                                Authorized_Permission="View" 
                                ClientScriptMethod="get_element;print_items('word')" 
                                />
				            <tstsc:DropMenuItem 
                                Name="ExportToPdf" 
                                runat="server"
                                Value="<%$Resources:Dialogs,Global_ExportToPdf %>" 
                                ImageUrl="Images/Filetypes/Acrobat.svg" 
                                Authorized_ArtifactType="TestCase" 
                                Authorized_Permission="View" 
                                ClientScriptMethod="get_element;print_items('pdf')" 
                                />    
                            <tstsc:DropMenuItem Divider="true" runat="server"/>
						    <tstsc:DropMenuItem 
                                Name="AddToRelease" 
                                runat="server"
                                Value="<%$Resources:Dialogs,TestCaseList_AddToRelease %>" 
                                ImageUrl="Images/artifact-Release.svg" 
                                Authorized_ArtifactType="Release" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="prototype; page.grdTestCaseList_addToRelease()" 
                                />
						    <tstsc:DropMenuItem 
                                Name="AddToTestSet" 
                                runat="server"
                                Value="<%$Resources:Dialogs,TestCaseList_AddToTestSet %>" 
                                ImageUrl="Images/artifact-TestSet.svg" 
                                Authorized_ArtifactType="TestSet" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="prototype; page.grdTestCaseList_addToTestSet()" 
                                />
						    <tstsc:DropMenuItem 
                                Name="AddToRequirement" 
                                runat="server"
                                Value="<%$Resources:Dialogs,TestCaseList_AddToRequirement %>" 
                                ImageUrl="Images/artifact-Requirement.svg" 
                                Authorized_ArtifactType="Requirement" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="prototype; page.grdTestCaseList_addToRequirement()" 
                                />
                            <tstsc:DropMenuItem Divider="true" runat="server"/>
						    <tstsc:DropMenuItem 
                                Name="RemoveFromRelease" 
                                runat="server"
                                Value="<%$Resources:Dialogs,TestCaseList_RemoveFromRelease %>" 
                                ImageUrl="Images/artifact-Release.svg" 
                                Authorized_ArtifactType="Release" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="prototype; page.grdTestCaseList_removeFromRelease()" 
                                />
					    </DropMenuItems>
				    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
				    <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTestCaseList" ClientScriptMethod="toggle_visibility" />
                </div>
                <div class="btn-group priority3" role="group">
                    <asp:PlaceHolder ID="plcWorX" runat="server" Visible="false">
				        <tstsc:DropMenu id="mnuWorX" runat="server" Text="WorX" MenuWidth="100px" />
                    </asp:PlaceHolder>
                </div>
            </div>
            <div class="bg-near-white-hover py2 px3 br2 transition-all flex-container flex-item-center flex-wrap-reverse">
                <div class="flex-grow-1">
					<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                    <asp:Label ID="lblVisibleTestCaseCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <asp:Label ID="lblTotalTestCaseCount" Runat="server" Font-Bold="True" />
                    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,TestCaseList_TestCasesForRelease %>" />.
                    <asp:Label runat="server" ID="txtFolderInfo" CssClass="badge mx3 align-v-reset" />
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                </div>
                <div class="flex-none">
                    <tstsc:DropDownHierarchy ID="ddlSelectRelease" Runat="server" NoValueItem="true" SkinID="ReleaseDropDownListFarRight"
                        NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" AutoPostBack="false" DataTextField="FullName"
                        DataValueField="ReleaseId" Width="300px" ListWidth="300px" ClientScriptServerControlId="grdTestCaseList"
                        ActiveItemField="IsActive" ClientScriptMethod="custom_operation_select" ClientScriptParameter="SelectRelease" />
                </div>
            </div>
			<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
            <div class="main-content">
				<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>  
                        <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
                        <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />
                        <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
                    </Services>
                    <Scripts>
                        <asp:ScriptReference Path="~/TypeScript/rct_comp_testRunsPendingExecuteNewOrExisting.js" />
                    </Scripts>
                </tstsc:ScriptManagerProxyEx>  




                <tstsc:SortedGrid 
                    ID="grdTestCaseList" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    HeaderCssClass="Header" 
                    ConcurrencyEnabled="true"
				    SubHeaderCssClass="SubHeader" 
                    SelectedRowCssClass="Highlighted" 
                    ErrorMessageControlId="divMessage"
				    RowCssClass="Normal" 
                    EditRowCssClass="Editing" 
                    ItemImage="artifact-TestCaseNoSteps.svg" 
                    NegativePrimaryKeysDisabled="false"
				    runat="server" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" 
                    AlternateItemImage="artifact-TestCase.svg"
				    Authorized_ArtifactType="TestCase" 
                    Authorized_Permission="BulkEdit" 
                    FolderItemImage="Folder.svg"
				    TotalCountControlId="lblTotalTestCaseCount" 
                    VisibleCountControlId="lblVisibleTestCaseCount"
                    FilterInfoControlId="lblFilterInfo" 
                    AllowColumnPositioning="true"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-mouse-pointer" 
                            runat="server"
                            Caption="<%$Resources:Buttons,OpenItem %>" 
                            CommandName="open_item" 
                            CommandArgument="_self" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-external-link-alt"
                            runat="server"
                            Caption="<%$Resources:Buttons,OpenItemNewTab %>" 
                            CommandName="open_item" 
                            CommandArgument="_blank" 
                            Authorized_ArtifactType="TestCase" Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 far fa-folder-open"
                            runat="server"
                            Caption="<%$Resources:Buttons,FocusOn %>" 
                            CommandName="focus_on" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem Divider="True" runat="server"/>
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-plus" 
                            runat="server"
                            Caption="<%$Resources:Buttons,Insert%>" 
                            CommandName="insert_item" 
                            CommandArgument="TestCase"
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="Create" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 far fa-edit" 
                            runat="server"
                            Caption="<%$Resources:Buttons,EditItems%>" 
                            CommandName="edit_items" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="BulkEdit" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-trash-alt" 
                            runat="server"
                            Caption="<%$Resources:Buttons,Delete%>" 
                            CommandName="delete_items" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="Delete" 
                            ConfirmationMessage="<%$Resources:Messages,TestCaseList_DeleteConfirm %>" 
                            />
                        <tstsc:ContextMenuItem Divider="True" runat="server"/>
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-copy" 
                            runat="server"
                            Caption="<%$Resources:Buttons,CopyItems%>" 
                            CommandName="copy_items" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="BulkEdit" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-cut" 
                            runat="server"
                            Caption="<%$Resources:Buttons,CutItems%>" 
                            CommandName="cut_items" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="BulkEdit" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 far fa-clipboard"
                            runat="server"
                            Caption="<%$Resources:Buttons,PasteItems%>" 
                            CommandName="paste_items" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="BulkEdit" 
                            />
                        <tstsc:ContextMenuItem Divider="True" runat="server"/>
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-play" 
                            runat="server"
                            id="grdTestCaseList_contextExecute"
                            Caption="<%$Resources:Buttons,Execute%>" 
                            ClientScriptMethod="grdTestCaseList_execute()" 
                            Authorized_ArtifactType="TestRun" 
                            Authorized_Permission="Create" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-print" 
                            runat="server"
                            Caption="<%$Resources:Dialogs,Global_PrintItems%>" 
                            ClientScriptMethod="print_items()" 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="View" 
                            />
                    </ContextMenuItems>     
                </tstsc:SortedGrid>

                <tstsc:DialogBoxPanel ID="dlgCustomListOperation" runat="server" SkinID="PopupOverflowVisible"
                    Width="500px" Height="220px" Modal="true" Title="">
                    <tstsc:MessageBox ID="msgCustomListOperation" runat="server" SkinID="MessageBox" Width="100%" />
                    <div class="my3">
                        <div class="alert alert-warning alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <span id="spnCustomListOperationText"></span>
                        </div>
                        <div class="form-group">
                            <div class="DataLabel col-md-3">
                                <tstsc:LabelEx ID="ddlCustomOperationArtifactLabel" runat="server" AssociatedControlID="ddlCustomOperationArtifact" />:
                            </div>
                            <div class="DataEntry col-md-8 mb3 data-entry-wide">
                                <tstsc:UnityDropDownHierarchy ID="ddlCustomOperationArtifact" runat="server" DataTextField="Name" DataValueField="TestCaseParameterId"
                                    NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" IndentLevelField="IndentLevel" SummaryItemField="IsSummary" />
                            </div>
                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <div class="btn-group">
                        <tstsc:ButtonEx ID="btnOperation" runat="server" SkinID="ButtonPrimary" ClientScriptMethod="page.customListOperation_execute()" />
						<tstsc:ButtonEx ID="btnCancel" runat="server" ClientScriptServerControlId="dlgCustomListOperation"
                            ClientScriptMethod="close()" Text="<%$Resources:Buttons,Cancel %>" />
                    </div>

                </tstsc:DialogBoxPanel>
            </div>
            <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            <tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="divMessage"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
		</div>
	</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;

        /* The Page Class */
        Type.registerNamespace('Inflectra.SpiraTest.Web');
        Inflectra.SpiraTest.Web.TestCaseList = function ()
        {
            Inflectra.SpiraTest.Web.TestCaseList.initializeBase(this);

            this._msgCustomListOperation = $get('<%=msgCustomListOperation.ClientID%>');
            this._divMessage = $get('<%=divMessage.ClientID%>');
            this._btnOperation = $get('<%=btnOperation.ClientID%>');
            this._projectId = <%=ProjectId%>;
            this._operation = null;
            this._artifactName = '';
            this._successMessage = '';
            this._lastOperation = null;
            this._sidepanelCharts = new Object;
        };
        Inflectra.SpiraTest.Web.TestCaseList.prototype =
        {
            /* Constructors */
            initialize: function ()
            {
                Inflectra.SpiraTest.Web.TestCaseList.callBaseMethod(this, 'initialize');
            },
            dispose: function ()
            {
                Inflectra.SpiraTest.Web.TestCaseList.callBaseMethod(this, 'dispose');
            },

            /* Methods */

            grdTestCaseList_addToRelease: function()
            {
                //Make sure we have items selected
                var grdTestCaseList = $find('<%=grdTestCaseList.ClientID%>');
                var items = grdTestCaseList.get_selected_items();
                if (items.length > 0)
                {
                    //Set operation
                    this._operation = 'Release';
                    this._artifactName = resx.ArtifactType_Release;
                    this._successMessage = resx.TestCaseList_AddToReleaseSuccess;

                    //Display dialog
                    $('#<%=ddlCustomOperationArtifactLabel.ClientID%>').text(resx.ArtifactType_Release);
                    this._btnOperation.value = resx.Global_Add;
                    $('#spnCustomListOperationText').text(resx.TestCaseList_SelectRelease);
                    var dlgCustomListOperation = $find('<%=this.dlgCustomListOperation.ClientID%>');
                    dlgCustomListOperation.set_title(resx.TestCaseList_AddToRelease);
                    dlgCustomListOperation.display();

                    //Set the ddl images
                    var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                    globalFunctions.getHierarchyLookupImages(ddlCustomOperationArtifact, 'Release');

                    //Load the artifacts
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveLookupList(this._projectId, 'Release', Function.createDelegate(this, this.customListOperation_loaded), Function.createDelegate(this, this.customListOperation_failure));
                }
                else
                {
                    alert(resx.Global_SelectOneCheckBoxForCommand);
                }
            },
            grdTestCaseList_addToTestSet: function()
            {
                //Make sure we have items selected
                var grdTestCaseList = $find('<%=grdTestCaseList.ClientID%>');
                var items = grdTestCaseList.get_selected_items();
                if (items.length > 0)
                {
                    //Set operation
                    this._operation = 'TestSet';
                    this._artifactName = resx.ArtifactType_TestSet;
                    this._successMessage = resx.TestCaseList_AddToTestSetSuccess;

                    //Display dialog
                    $('#<%=ddlCustomOperationArtifactLabel.ClientID%>').text(resx.ArtifactType_TestSet);
                    this._btnOperation.value = resx.Global_Add;
                    $('#spnCustomListOperationText').text(resx.TestCaseList_SelectTestSet);
                    var dlgCustomListOperation = $find('<%=this.dlgCustomListOperation.ClientID%>');
                    dlgCustomListOperation.set_title(resx.TestCaseList_AddToTestSet);
                    dlgCustomListOperation.display();

                    //Set the ddl images
                    var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                    globalFunctions.getHierarchyLookupImages(ddlCustomOperationArtifact, 'TestSet');

                    //Load the artifacts
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveLookupList(this._projectId, 'TestSet', Function.createDelegate(this, this.customListOperation_loaded), Function.createDelegate(this, this.customListOperation_failure));
                }
                else
                {
                    alert(resx.Global_SelectOneCheckBoxForCommand);
                }
            },
            grdTestCaseList_addToRequirement: function()
            {
                //Make sure we have items selected
                var grdTestCaseList = $find('<%=grdTestCaseList.ClientID%>');
                var items = grdTestCaseList.get_selected_items();
                if (items.length > 0)
                {
                    //Set operation
                    this._operation = 'Requirement';
                    this._artifactName = resx.ArtifactType_Requirement;
                    this._successMessage = resx.TestCaseList_AddToRequirementSuccess;

                    //Display dialog
                    $('#<%=ddlCustomOperationArtifactLabel.ClientID%>').text(resx.ArtifactType_Requirement);
                    this._btnOperation.value = resx.Global_Add;
                    $('#spnCustomListOperationText').text(resx.TestCaseList_SelectRequirement);
                    var dlgCustomListOperation = $find('<%=this.dlgCustomListOperation.ClientID%>');
                    dlgCustomListOperation.set_title(resx.TestCaseList_AddToRequirement);
                    dlgCustomListOperation.display();

                    //Set the ddl images
                    var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                    globalFunctions.getHierarchyLookupImages(ddlCustomOperationArtifact, 'Requirement');

                    //Load the artifacts
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveLookupList(this._projectId, 'Requirement', Function.createDelegate(this, this.customListOperation_loaded), Function.createDelegate(this, this.customListOperation_failure));
                }
                else
                {
                    alert(resx.Global_SelectOneCheckBoxForCommand);
                }
            },
            grdTestCaseList_removeFromRelease: function()
            {
                //Make sure we have items selected
                var grdTestCaseList = $find('<%=grdTestCaseList.ClientID%>');
                var items = grdTestCaseList.get_selected_items();
                if (items.length > 0)
                {
                    //Set operation
                    this._operation = 'ReleaseRemove';
                    this._artifactName = resx.ArtifactType_Release;
                    this._successMessage = resx.TestCaseList_RemoveFromReleaseSuccess;

                    //Display dialog
                    $('#<%=ddlCustomOperationArtifactLabel.ClientID%>').text(resx.ArtifactType_Release);
                    this._btnOperation.value = resx.Global_Remove;
                    $('#spnCustomListOperationText').text(resx.TestCaseList_SelectReleaseToRemove);
                    var dlgCustomListOperation = $find('<%=this.dlgCustomListOperation.ClientID%>');
                    dlgCustomListOperation.set_title(resx.TestCaseList_RemoveFromRelease);
                    dlgCustomListOperation.display();

                    //Set the ddl images
                    var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                    globalFunctions.getHierarchyLookupImages(ddlCustomOperationArtifact, 'Release');

                    //Load the artifacts
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveLookupList(this._projectId, 'Release', Function.createDelegate(this, this.customListOperation_loaded), Function.createDelegate(this, this.customListOperation_failure));
                }
                else
                {
                    alert(resx.Global_SelectOneCheckBoxForCommand);
                }
            },

            customListOperation_execute: function()
            {
                //Get the items
                var grdTestCaseList = $find('<%=grdTestCaseList.ClientID%>');
                var items = grdTestCaseList.get_selected_items();

                //Get the artifact
                var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                if (ddlCustomOperationArtifact.get_selectedItem() && ddlCustomOperationArtifact.get_selectedItem().get_value())
                {
                    var artifactId = ddlCustomOperationArtifact.get_selectedItem().get_value();
 
                    //Close the dialog box
                    var dlgCustomListOperation = $find('<%=this.dlgCustomListOperation.ClientID%>');
                    dlgCustomListOperation.close();

                    //Invoke the action
                    grdTestCaseList.custom_list_operation(this._operation, artifactId, this._successMessage);
                }
                else
                {
                    var msg = resx.Global_NeedToSelectItem.replace('{0}', this._artifactName);
                    alert(msg);
                }
            },

            customListOperation_loaded: function(data)
            {
                //Databind the dropdown list
                globalFunctions.hide_spinner();
                var ddlCustomOperationArtifact = $find('<%=ddlCustomOperationArtifact.ClientID%>');
                ddlCustomOperationArtifact.clearItems()

                //Add the please select item
                ddlCustomOperationArtifact.addItem('', '0', '', '', 'Y', '-- ' + resx.Global_PleaseSelect + ' --');
                ddlCustomOperationArtifact.set_dataSource(data);
                ddlCustomOperationArtifact.dataBind();

                if (this._lastOperation != this._operation)
                {
                    ddlCustomOperationArtifact.set_selectedItem('');
                    this._lastOperation = this._operation;
                }
            },
            customListOperation_failure: function (exception)
            {
                //Display the error
                globalFunctions.hide_spinner();
                globalFunctions.display_error(this._msgCustomListOperation, exception);
            },
            loadGraphs: function()
            {
                //Load the count by priority graph
                var releaseId = null;
                var ddlSelectRelease = $find('<%=this.ddlSelectRelease.ClientID%>');
                if (ddlSelectRelease && ddlSelectRelease.get_selectedItem() && ddlSelectRelease.get_selectedItem().get_value() != '')
                {
                    releaseId = parseInt(ddlSelectRelease.get_selectedItem().get_value());
                }
                Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.TestCase_RetrieveExecutionSummary(SpiraContext.ProjectId, releaseId, Function.createDelegate(this, this.loadGraphs_success), function(ex) { /* Fail quietly */ });
            },
            loadGraphs_success: function (data)
            {
                var cleanedData = this.prepDataForC3(data);
                
                this._sidepanelCharts.chartExecutionStatus = c3.generate({
                    bindto: d3.select('#tcChartExecutionStatus'),
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
            },
            prepDataForC3: function (data)
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
        };
        var page = $create(Inflectra.SpiraTest.Web.TestCaseList);

        function grdTestCaseList_loaded()
        {
            //Load the graphs
            page.loadGraphs();
        }

        var grdTestCaseListSelectedIds = [];
        function grdTestCaseList_execute()
        {

            //Get the list of selected test cases
            var grdTestCaseList = $find('<%=grdTestCaseList.ClientID %>');
            grdTestCaseListSelectedIds = grdTestCaseList.get_selected_items();

            //We need at least one test case
            if (grdTestCaseListSelectedIds.length < 1)
            {
                globalFunctions.globalAlert(resx.TestSetDetails_SelectTestCaseFirst, 'danger', true);
                return;
            }

            if (grdTestCaseListSelectedIds.length === 1) {
                //First check if the user has an existing testrunpending for this test case
                Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.RetrievePendingByUserIdAndTestCase(
                    SpiraContext.ProjectId,
                    grdTestCaseListSelectedIds[0],
                    AspNetAjax$Function.createDelegate(this, this.retrieveExistingPending_success),
                    AspNetAjax$Function.createDelegate(this, this.execute_test_case_process)
                );
            } else {
                execute_test_case_process();
            }

        }
        function retrieveExistingPending_success(data) {
            if (data && data.length) {
                // make sure the message dialog is clear then render
                globalFunctions.dlgGlobalDynamicClear();
                ReactDOM.render(
                    React.createElement(RctTestRunsPendingExecuteNewOrExisting, {
                        data: data,
                        newTestName: resx.TestCaseList_ExecuteTestCase,
                        executeFunction: this.execute_test_case_process
                    }, null),
                    document.getElementById('dlgGlobalDynamic')
                );
            }
            else {
                execute_test_case_process();
            }
        }
        function execute_test_case_process(testRunsPendingId)
        {
            //store the test case ids in function so we can clear the global var (safety)
            var testCaseIds = grdTestCaseListSelectedIds;
            grdTestCaseListSelectedIds = [];

            //Actually start the background process of creating the test runs
            if (!testRunsPendingId)
            {
                var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

                //See if we have a release set
                var releaseId = null;
                var ddlSelectRelease = $find('<%=ddlSelectRelease.ClientID %>');
                if (ddlSelectRelease.get_selectedItem() && ddlSelectRelease.get_selectedItem().get_value() != '')
                {
                    releaseId = ddlSelectRelease.get_selectedItem().get_value();
                }

                ajxBackgroundProcessManager.display(
                    SpiraContext.ProjectId,
                    'TestCase_ExecuteMultiple',
                    resx.TestCaseList_ExecuteTestCase,
                    resx.TestCaseList_ExecuteTestCaseDesc,
                    releaseId,
                    testCaseIds
                );
            }
            else
            {
                window.open(globalFunctions.getArtifactDefaultUrl(SpiraContext.BaseUrl, SpiraContext.ProjectId, "TestExecute", testRunsPendingId), "_self");
            }
        }
        function ajxBackgroundProcessManager_success(msg, returnCode)
        {
            
            //Need to redirect to the test runs pending
            if (returnCode && returnCode > 0)
            {
                var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
                var projectId = ajxBackgroundProcessManager.get_projectId();

                //set the base url to either exploratory or normal - based on what is returned from the server in the success params
                var baseUrl = msg === "testcase_executeexploratory" ? '<%=TestRunsPendingExploratoryUrl %>' : '<%=TestRunsPendingUrl %>';
                var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
                window.location = url;

            }
        }
        function print_items(format)
        {
            //Get the items and convert to csv
            var grdTestCaseList = $find('<%=grdTestCaseList.ClientID %>');
            var items = grdTestCaseList.get_selected_items();
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
                    //Open the report for the specified format
                    if (format == 'excel')
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseSummary%>";
                        filter = "&fl_5=";
                    }
                    else
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseDetailed%>";
                        filter = "&fl_6=";
                    }
                    artifacts = folderId;
                }
                else
                {
                    var item_list = globalFunctions.convertIntArrayToString(items);
                    //Open the report for the specified format
                    if (format == 'excel')
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseSummary%>";
                        filter = "&af_5_87=";
                    }
                    else
                    {
                        reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseDetailed%>";
                        filter = "&af_6_87=";
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
            var ajaxControl = $find('<%=grdTestCaseList.ClientID %>');
            ajaxControl.load_data();
        }
        function trvFolders_folderChanged(folderId)
        {
            //Set the standard filter and reload
            var grdTestCaseList = $find('<%=grdTestCaseList.ClientID %>');
            var args = {};
            args['_FolderId'] = globalFunctions.serializeValueInt(folderId);
            grdTestCaseList.set_standardFilters(args);
            grdTestCaseList.load_data();
        }
        function grdTestCaseList_focusOn(nodeId)
        {
            //It means the folder may have changed, so reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.set_selectedNode(nodeId);
            trvFolders.load_data(true);
        }
    </script>
</asp:Content>
