<%@ Page 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    AutoEventWireup="true" 
    CodeBehind="SourceCodeFileDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.SourceCodeFileDetails" Title="Untitled Page" 
%>

<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="PrismSyntaxHighlighterStylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel 
                BodyHeight="150" 
                ErrorMessageControlId="lblMessage"
                ClientScriptMethod="load_data(true)" 
                ClientScriptServerControlId="trvFolders" 
                HeaderCaption="<%$Resources:Main,Global_Folders %>"
                ID="pnlFolders" 
                runat="server"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService"
                >
                <div class="Widget panel">
                    <tstsc:TreeView 
                        AllowEditing="false" 
                        Authorized_ArtifactType="SourceCodeFile" 
                        Authorized_Permission="View" 
                        ClientScriptMethod="refresh_data();" 
                        ClientScriptServerControlId="navSourceCodeFiles" 
                        CssClass="FolderTree" 
                        EditDescriptions="false"
                        ErrorMessageControlId="lblMessage"
                        ID="trvFolders" 
                        ItemName="<%$Resources:Fields,Folder %>"
                        LoadingImageUrl="Images/action-Spinner.svg" 
                        NodeLegendFormat="{0}" 
                        runat="server" 
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService"
                        />
                </div>
            </tstsc:SidebarPanel>

            <tstsc:NavigationBar 
                AutoLoad="true" 
                BodyHeight="580px" 
                IncludeAssigned="false"
                IncludeDetected="false"
                ErrorMessageControlId="lblMessage"
                ID="navSourceCodeFiles" 
                ListScreenCaption="<%$Resources:Main,SourceCodeFileDetails_BackToList%>"
                runat="server" 
                EnableLiveLoading="true" 
                FormManagerControlId="ajxFormManager"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService" 
                />
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="main-content">

                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                
                <asp:Panel ID="pnlFolderPath" runat="server" CssClass="mt3" />
                <div class="flex justify-between items-center pr4 py3 sticky top-nav bg-white z-10">
                    <div class="df items-center">
                        <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                            <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,SourceCodeFileDetails_BackToList%>">
                                <span class="fas fa-arrow-left"></span>
                            </tstsc:HyperlinkEx>
                        </div>           
                        <span class="mr3 py2 px3" runat="server" title="<%$Resources:Main,SourceCodeList_CurrentBranch%>"> 
                            <span class="fas fa-code-branch"></span> 
                            <asp:Label ID="lblBranchName" runat="server" /> 
                        </span>
                        <h2 class="dib my0">
                            <tstsc:LabelEx ID="lblFileName" runat="server" />
                        </h2>
                    </div>

                    <tstsc:HyperLinkEx ID="lnkFileName" CssClass="btn btn-default" runat="server" Target="_blank">
                        <i class="fas fa-external-link-alt"></i>
                        <asp:Localize runat="server" Text="<%$Resources:Main,SourceCode_ViewRaw%>" />
                    </tstsc:HyperLinkEx>

                </div>

                <div class="py2 px4 mb2 bg-near-white br2 df items-center flex-wrap mr4 justify-between">
                    <div>
                        <tstsc:ImageEx 
                            CssClass="w5 h5 mr3"
                            ID="imgDocumentFileType" 
                            runat="server"
                            /> 
                        <span class="fs-h6">
                            <tstsc:LabelEx ID="lblSize" runat="server" />
                        </span>
                    </div>

                    <div>
                        <tstsc:LabelEx 
                                ID="lnkLatestRevisionLabel" 
                                AppendColon="true"
                                runat="server" 
                                Text="<%$Resources:Fields,LatestRevisionInBranch %>" 
                                />
                        <tstsc:ArtifactHyperLink 
                            AlternateText="<%$Resources:Fields,Revision %>"
                            CssClass="ArtifactHyperLink"
                            DisplayChangeLink="false"
                            ID="lnkLatestRevision" 
                            runat="server" 
                            />
                        <span class="fs-90 ml3"> 
                            (<tstsc:LabelEx ID="lblLastEdited" runat="server" />)
                        </span>
                    </div>
                </div>





                <tstsc:TabControl ID="tclFileDetails" CssClass="TabControl2" TabWidth="100" TabHeight="25"
                    TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                    runat="Server">
                    <TabPages>
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Preview %>" 
                            ID="tabPreview" 
                            runat="server"
                            TabPageControlId="pnlPreview" 
                            TabPageIcon="far fa-eye"
                            />
                        <tstsc:TabPage 
                            AjaxServerControlId="grdSourceCodeRevisionList" 
                            Caption="<%$Resources:ServerControls,TabControl_History %>" 
                            ID="tabRevisions" 
                            runat="server"
                            TabPageControlId="pnlRevisions" 
                            TabPageIcon="fas fa-history"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Associations %>" 
                            AjaxControlContainer="tstAssociationPanel"
                            AjaxServerControlId="grdAssociationLinks" 
                            ID="tabAssociations" 
                            runat="server"
                            TabPageControlId="pnlAssociations" 
                            TabPageIcon="fas fa-link"
                            />
                    </TabPages>
                </tstsc:TabControl>


    

                <asp:Panel ID="pnlPreview" runat="server" CssClass="TabControlPanel">
                    <div id="codePreview"  style="display: none"> 
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,Global_Loading %>" />
                    </div>
                    <div id="noPreview" style="display: none">
                        <div class="alert alert-info alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Messages,SourceCodeFileDetails_PreviewNotAvailable %>" />
                        </div>
                    </div>
                    <div id="imagePreview" class="preview-image" style="display: none">
                        <a id="imgPreviewHyperLink" target="_blank">
                            <img id="imgPreview" />
                        </a>
                    </div>
                    <div id="markdownPreview"  style="display: none">
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,Global_Loading %>" />
                    </div>
                </asp:Panel>




				<asp:Panel ID="pnlRevisions" Runat="server" CssClass="TabControlPanel">
                    <div style="width:100%" class="TabControlHeader">
                        <div class="btn-group priority3">
                            <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkRefreshRQTK" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="load_data()">
                                <span class="fas fa-sync"></span>
                                <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
                            </tstsc:HyperLinkEx>
                            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                    ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="apply_filters()">
			                    <DropMenuItems>
				                    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				                    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			                    </DropMenuItems>
		                    </tstsc:DropMenu>
                        </div>
                    </div>

                    <div class="bg-near-white-hover py2 px3 br2 transition-all">
	                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                        <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                        <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SourceCodeRevisions_Revisions %>" />.
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                    </div>

				    <tstsc:SortedGrid ID="grdSourceCodeRevisionList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
				        SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="lblMessage"
				        RowCssClass="Normal" AutoLoad="true" DisplayAttachments="false" AllowEditing="false"
                        FilterInfoControlId="lblFilterInfo" VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount"
				        runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" />
                </asp:Panel>




				<asp:Panel ID="pnlAssociations" Runat="server" CssClass="TabControlPanel">
                    <tstuc:AssociationsPanel 
                        ID="tstAssociationPanel" 
                        runat="server" 
                        /> 
                </asp:Panel>
            </div>
        </div>
	</div>

    <tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage" ArtifactImageControlId="imgDocumentFileType"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService" ArtifactTypeName="<%$Resources:Fields,File%>"
        FolderPathControlId="pnlFolderPath" DisplayPageName="false" NameField="Name" WorkflowEnabled="false" CheckUnsaved="false">
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblFilePath" DataField="FileKey" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAuthor" DataField="AuthorName" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblLastEdited" DataField="LastUpdated" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblSize" DataField="Size" Direction="In" PropertyName="textValue" />
            <tstsc:AjaxFormControl ControlId="lnkLatestRevision" Direction="In" DataField="Revision" />
            <tstsc:AjaxFormControl ControlId="lblFileType" DataField="FileType" Direction="In" PropertyName="textValue" />
            <tstsc:AjaxFormControl ControlId="imgFileType" DataField="FileType" Direction="In" PropertyName="tooltip" />  
            <tstsc:AjaxFormControl ControlId="imgDocumentFileType" DataField="FileType" Direction="In" PropertyName="tooltip" />  
        </ControlReferences>
    </tstsc:AjaxFormManager>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Scripts>
        <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" /> 
        
        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.prism.js" />
        <asp:ScriptReference Path="~/TypeScript/SyntaxHighlighting.js" />

        <asp:ScriptReference Path="~/TypeScript/SourceCodeFileDetails.js" />        
      </Scripts>
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />  
      </Services>  
    </tstsc:ScriptManagerProxyEx>
    <script type="text/javascript">
        //Set initial load status
        var initialLoad = true;

        //Get artifact types
        var artifactTypes = [];
        $(document).ready(function(){
            artifactTypes = globalFunctions.getArtifactTypes();
        });

        function ajxFormManager_loaded_sourceCode(dontClearMessages)
        {
            //Get the current data item
	        var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
	        var dataItem = ajxFormManager.get_dataItem();
	        SpiraContext.ArtifactId = ajxFormManager.get_primaryKey();
	        SpiraContext.IsArtifactCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
	        var fileKey = dataItem.Fields.FileKey.textValue;
	        var name = dataItem.Fields.Name.textValue;
	        var branchKey = '<%=GlobalFunctions.JSEncode(BranchKey)%>';

	        //Change the page title and the download hyperlink
	        window.document.title = resx.ArtifactType_SourceCode + ': ' + name + ' | ' + SpiraContext.ProductType;
	        var url = sourceCodeViewer_urlTemplate.replace('{0}', fileKey).replace('{1}', branchKey);
            $get('<%=lnkFileName.ClientID%>').href = url;
            $('#<%=lblFileName.ClientID%>').text(name);

            //Update the preview panel
            sourceCodeFileDetails.updatePreview(branchKey, fileKey);

            if (!initialLoad) {
                //Update the nav bar
                updateNavBar(dontClearMessages);

                //Update the files grid
                updateRevisionsGrid(fileKey);

                //Update the associations grid
                updateAssociationsGrid();
            }
            else {
                initialLoad = false;

                //Set the permissions on the association grid
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstAssociationPanel.load_data(filters, false);
            }

            //Always get the latest count to see if we 'have data'
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_CountForFile(SpiraContext.ProjectId, fileKey, updateRevisionsGrid_checkData);

            //See if we have any associations
            if (typeof (tstuc_cplMainContent_tstAssociationPanel) != 'undefined' && tstuc_cplMainContent_tstAssociationPanel) {
                tstuc_cplMainContent_tstAssociationPanel.check_hasData(associations_callback);
            }
        }

        function page_operation_failure(ex) {
            var lblMessage = $get('<%=this.lblMessage.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
        }

        function updateNavBar(dontClearMessages)
	    {
	        var navigationBar = $find('<%=this.navSourceCodeFiles.ClientID%>');
	        navigationBar.set_selectedItemId(SpiraContext.ArtifactId);
	        navigationBar.refresh_data(dontClearMessages);
        }

        function updateAssociationsGrid()
	    {
	        //See if the tab is visible
	        var loadNow = ($find('<%=tclFileDetails.ClientID%>').get_selectedTabClientId() == '<%=pnlAssociations.ClientID%>');

	        //Reload the tab's data
	        var filters = {};
	        filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
	        filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
	        tstuc_cplMainContent_tstAssociationPanel.load_data(filters, loadNow);
	    }

        function updateRevisionsGrid(fileKey)
	    {
            var grdSourceCodeRevisionList = $find('<%=grdSourceCodeRevisionList.ClientID%>');
	        var filters = {};
	        filters[globalFunctions.keyPrefix + 'FileKey'] = globalFunctions.serializeValueString(fileKey);
	        grdSourceCodeRevisionList.set_standardFilters(filters);
	        grdSourceCodeRevisionList.load_data();
	    }
        function updateRevisionsGrid_checkData(count)
	    {
	        $find('<%=tclFileDetails.ClientID%>').updateHasData('tabRevisions', (count > 0));
	    }
	    function associations_callback(hasData)
	    {
	        $find('<%=tclFileDetails.ClientID%>').updateHasData('tabAssociations', hasData);
	    }

        function grdSourceCodeRevisionList_loaded()
        {
            if (!SpiraContext.uiState.artifactTypes) {
                SpiraContext.uiState.artifactTypes = globalFunctions.getArtifactTypes();
            }
            //Search the grid messages for any tokens and make them artifact hyperlinks
            var grdSourceCodeRevisionList_id = '<%=this.grdSourceCodeRevisionList.ClientID%>';
            var els = $('#' + grdSourceCodeRevisionList_id + ' tr.Normal td div:contains("[")');
            var regex = /\[(?<key>[A-Z]{2})[:\-](?<id>\d*?)\]/gi;
            for (var i = 0; i <= els.length; i++)
            {
                if (els[i])
                {
                    var text = els[i].innerHTML;
                    if (text)
                    {
                        els[i].innerHTML = text.replace(regex, replacer);
                    }
                }
            }
        }

        function replacer(match, artifactPrefix, artifactId, offset, string) {
            var artifactTypes = SpiraContext.uiState.artifactTypes;
            if (artifactPrefix && artifactId) {
                var artifactUrlPart = null;
                for (var i = 0; i < artifactTypes.length; i++) {
                    if (artifactTypes[i].token == artifactPrefix) {
                        artifactUrlPart = artifactTypes[i].val;
                        break;
                    }
                }
                if (artifactUrlPart) {
                    return '<a href="' + globalFunctions.replaceBaseUrl('~/' + SpiraContext.ProjectId + '/' + artifactUrlPart + '/' + artifactId + '.aspx') + '">[' + artifactPrefix + ':' + artifactId + ']</a>';
                }
            }
            return '';
        }

        function pnlFolderPath_click(folderId)
        {
            var trvFolders = $find('<%=this.trvFolders.ClientID%>');
            trvFolders.nodeClick(folderId);
        }

        //Page objects
        var ajxFormManager_id = '<%=ajxFormManager.ClientID%>';
        var tabControl_id = '<%=tclFileDetails.ClientID%>';

        //URL templates
        var sourceCodeViewer_urlTemplate = '<%=SourceCodeFileViewerUrl%>';

        //Set some initialstate
        $(document).ready(function () {
            // make sure the preview tabs render properly - needs to do each time you open the tab to make sure line numbers align correctly
            var previewObserver = new MutationObserver(function (mutations) {
                mutations.forEach(function (mutationRecord) {
                    Prism.highlightAllUnder(mutationRecord.target);
                });
            });

            var target = document.getElementById('<%=pnlPreview.ClientID%>');
            previewObserver.observe(target, { attributes: true, attributeFilter: ['style'] });
        });
    </script>
</asp:Content>
