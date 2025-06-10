<%@ Page 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    AutoEventWireup="true" 
    CodeBehind="SourceCodeRevisionFileDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.SourceCodeRevisionFileDetails" Title="Untitled Page" 
%>

<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>

<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DiffPlexStylesheet" />
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder1" runat="server" SkinID="PrismSyntaxHighlighterStylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel 
                BodyHeight="250px" 
                ErrorMessageControlId="lblClientMessage"
                HeaderCaption="<%$Resources:Main,SourceCodeRevisionFileDetails_FileSidebar %>"
                ID="pnlSidebar" 
                runat="server"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService"
                >
                <div class="mt2">
                    <div class="has-tooltip">
                        <tstsc:ImageEx CssClass="w4 h4 ml2" runat="server" ImageUrl="Images/artifact-Revision.svg" AlternateText="<%$Resources:Fields,Revision%>"  />
                        <a class="dib mb2 fw-b tdn" id="a-navigation-current-revision"></a>
                        <div class="is-tooltip" id="a-navigation-current-revision-tooltip">
                        </div>
                    </div>

                    <div id="target-files">
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,GlobalFunctions_TooltipLoading %>" />
                    </div>
                    <script id="template-files" type="x-tmpl-mustache">
                        <div id="revisions-navigation" class="db pl3 mb2 mt1 ml2">
                            {{#.}}
                            <div class="ws-nowrap pl3 py1 bg-vlight-gray-hover transition-all" tst:filekey="{{ Fields.Name.tooltip }}">
                                <img class="w4 h4" src="{{ Fields.Filetype.textValue }}" alt="{{ Fields.Filetype.tooltip }}" />
                                <a class="tdn py1" href="{{ customUrl }}" onmouseover="ddrivetip('<u>{{ Fields.Name.tooltip }}</u>')" onmouseout="hideddrivetip()" onclick="load_file(event, '{{ Fields.Name.tooltip }}')">
                                    {{ Fields.Name.textValue }}</a>
                            </div>
            
                            {{/.}}
                        </div>
                    </script>
                </div>
            </tstsc:SidebarPanel>

            <tstsc:SidebarPanel 
                BodyHeight="350px" 
                ErrorMessageControlId="lblClientMessage"
                HeaderCaption="<%$Resources:Main,SourceCodeRevisionFileDetails_RevisionSidebar %>"
                ID="pnlSidebar2" 
                runat="server"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService"
                >
                <div class="mt2">
                    <div class="mt2 ml2">
                        <span class="fas fa-code-branch"></span> 
                        <asp:Label ID="lblBranchName" runat="server" /> 
                    </div>
                    <div class="mt3 has-tooltip">
                        <img class="w4 h4 ml2" id="img-navigation-current-file" />
                        <a class="dib mb2 fw-b tdn" id="a-navigation-current-file"></a>
                        <div class="is-tooltip" id="a-navigation-current-file-tooltip">
                        </div>
                    </div>

                    <div id="target-revisions">
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,GlobalFunctions_TooltipLoading %>" />
                    </div>
                    <script id="template-revisions" type="x-tmpl-mustache">
                        <div id="revisions-navigation" class="db pl3 mb2 mt1 ml2">
                            {{#.}}
                            <div class="ws-nowrap pl3 py1 bg-vlight-gray-hover transition-all" tst:revisionkey="{{ Fields.Name.textValue }}">
                                <img class="w4 h4" src="{{ Fields.Action.textValue }}" alt="{{ Fields.Action.tooltip }}" />
                                <a class="tdn py1" href="{{ customUrl }}" onmouseover="ddrivetip('<u>{{ Fields.Name.textValue }} - {{ Fields.UpdateDate.tooltip }}</u><br />{{ Fields.Name.tooltip }}<br /><i>- {{ Fields.AuthorName.textValue }}</i>')" onmouseout="hideddrivetip()" onclick="load_revision(event, '{{ Fields.Name.textValue }}')">
                                    {{ Fields.Name.caption }}</a>
                                <small>
                                (<span>{{ Fields.UpdateDate.textValue }}</span>)
                                </small>
                            </div>
            
                            {{/.}}
                        </div>
                    </script>
                </div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="main-content">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <tstsc:MessageBox ID="lblClientMessage" runat="server" SkinID="MessageBox" />

                <div class="df justify-between items-center pr4 py3 sticky top-nav bg-white z-10 mt3">
                    <div class="df items-center">
                        <span class="mr3 bg-near-white br2 py2 px4 has-tooltip">
						    <tstsc:ImageEx 
                                AlternateText="<%$Resources:Fields,Revision%>"
                                ID="imgRevision" 
                                CssClass="w4 h4"
                                ImageUrl="Images/artifact-Revision.svg" 
                                ImageAlign="AbsMiddle" 
                                runat="server" 
                                />
						    <tstsc:HyperLinkEx 
                                ID="lnkRevision" 
                                runat="server" 
                                />
                            <span class="is-tooltip" id="lnkRevisionTooltip">
                            </span>
					    </span>
                        <h2 class="dib my0">
                            <tstsc:LabelEx ID="lblFileName" runat="server" />
                        </h2>
                    </div>
                    <div class="btn-group" role="group">
                        <tstsc:HyperLinkEx ID="lnkFileName" CssClass="btn btn-default" runat="server" Target="_blank">
                            <i class="fas fa-external-link-alt"></i>
                            <asp:Localize runat="server" Text="<%$Resources:Main,SourceCode_ViewRaw%>" />
                        </tstsc:HyperLinkEx>
                        <tstsc:HyperLinkEx ID="lnkFilePath" CssClass="btn btn-default" runat="server">
                            <tstsc:ImageEx 
                                AlternateText="Revision" 
                                CssClass="w4 h4"
                                ImageUrl="Images/artifact-SourceCode.svg" 
                                runat="server" 
                                />
                        </tstsc:HyperLinkEx>
                    </div>
                </div>

                <div class="py2 px4 mb2 bg-near-white br2 df items-center flex-wrap mr4 justify-between">
                    <div>
                        <tstsc:ImageEx 
                            CssClass="w5 h5 mr3"
                            ID="imgFileType" 
                            runat="server"
                            />
                        <span class="fs-h6">
                            <tstsc:LabelEx ID="lblSize" runat="server" />
                        </span>
                        <i class="fal fa-horizontal-rule fa-rotate-90 px3 light-gray"></i>
                        <tstsc:LabelEx 
                            AppendColon="true"
                            AssociatedControlID="lblChanger" 
                            ID="lblChangerLabel" 
                            runat="server" 
                            Text="<%$Resources:Fields,ChangerId %>" 
                            />
                        <tstsc:LabelEx ID="lblChanger" runat="server" />
                        <span class="fs-90 ml2">
                            (<tstsc:LabelEx ID="lblChangeDate" runat="server" />)
                        </span>
                    </div>

                    <div runat="server" id="boxPreviousChange" class="has-tooltip">
                        <tstsc:LabelEx 
                            AppendColon="true"
                            AssociatedControlID="lnkPreviousChange" 
                            ID="lnkPreviousChangeLabel" 
                            runat="server" 
                            Text="<%$Resources:Fields,PreviousRevision %>" 
                            />
						<tstsc:HyperLinkEx 
                            ID="lnkPreviousChange" 
                            runat="server"
                            ClientScriptMethod="load_revision(event, this.getAttribute('tst:revisionkey'))" 
                            />
                        <div class="is-tooltip" id="lnkPreviousChangeTooltip">
                        </div>
                    </div>
                </div>





                <tstsc:TabControl ID="tclFileDetails" CssClass="TabControl2" TabWidth="100" TabHeight="25"
                    TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                    runat="Server">
                    <TabPages>
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Changes %>" 
                            ID="tabChanges" 
                            runat="server"
                            TabPageControlId="pnlChanges" 
                            TabPageIcon="far fa-exchange"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Preview %>" 
                            ID="tabPreviewPrevious" 
                            runat="server"
                            TabPageControlId="pnlPreviewPrevious" 
                            TabPageIcon="far fa-code"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Preview %>" 
                            ID="tabPreview" 
                            runat="server"
                            TabPageControlId="pnlPreview" 
                            TabPageIcon="far fa-code"
                            />
                    </TabPages>
                </tstsc:TabControl>


                <asp:Panel ID="pnlChanges" runat="server" CssClass="TabControlPanel">
                    <div id="noChanges" style="display: none">
                        <div class="alert alert-info alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Messages,SourceCodeFileDetails_ChangesNotAvailable %>" />
                        </div>
                    </div>
                    <div id="target-changes">
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,Global_Loading %>" />
                    </div>
                    <div id="no-difference" style="display: none">
                        <asp:Localize runat="server" Text="<%$Resources:ClientScript,SourceCode_NoDifferenceBetweenRevisions %>" />
                    </div>

                    <script id="template-unified-changes" type="x-tmpl-mustache">
                        <div id="diffBox" class="mx-auto mb4 br3 ba b-vlight-gray ov-hidden">
                            <table cellpadding="0" cellspacing="0" class="ff-mono w-100">
                                <thead>
                                    <tr class="ff-kanit">
                                        <th colspan=2 class="fw-b pa3 bg-off-white bb b-vlight-gray">
                                            <div class="df items-center justify-between">
                                                <span class="df items-center" title="{{changesTooltip}}">
                                                    <span class="pl3">{{totalLineEdits}}</span>
                                                    <span class="pl3 pr5 w7 h4 py1 df">
                                                        <span style="width: {{percentInserts}}%;" class="diff-lineNumber-Inserted br b-off-white"></span>
                                                        <span style="width: {{percentDeletes}}%;" class="diff-lineNumber-Deleted br b-off-white"></span>
                                                        <span style="width: {{percentChanges}}%;" class="diff-lineNumber-Modified"></span>
                                                    </span>
                                                </span>
                                                <span>{{oldRevisionName}} => {{newRevisionName}}</span>
                                                <div class="btn-group">
                                                    <button type="button" class="btn btn-sm btn-default active cursor-default">
                                                        <i class="far fa-list"></i>
                                                        {{unifiedTitle}}
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-default" id="btn-diffBox-to-split" onclick="changeDiff('split')">
                                                        <i class="far fa-columns"></i>
                                                        {{splitTitle}}
                                                    </button>
                                                    {{#isCollapseLines}}
                                                        <button type="button" class="btn btn-sm btn-default" onclick="toggleCollapseUnified()">
                                                            <span id="btn-diff-unified-toggle-to-expand" title="{{expandTitle}}">
                                                                <i class="far fa-arrow-to-top"></i>
                                                                <i class="far fa-arrow-to-bottom"></i>
                                                            </span>
                                                            <span id="btn-diff-unified-toggle-to-collapse" class="dn" title="{{collapseTitle}}">
                                                                <i class="far fa-arrow-from-top"></i>
                                                                <i class="far fa-arrow-from-bottom"></i>
                                                            </span>
                                                        </button>
                                                    {{/isCollapseLines}}
                                                </div>
                                            </div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody id="tbody-diff-unified" {{#isCollapseLines}}class="diff-is-collapsed"{{/isCollapseLines}}>
                                    {{#lines}}
                                        <tr {{#show}}class="diff-row-show{{#isSummary}} diff-row-summary{{/isSummary}}"{{/show}}>
                                            <td class="pr3 pl4 pt2 tr fs-80 light-silver lh3 br b-vlight-gray v-top diff-lineNumber-{{type}}">
                                                {{ position }}
                                            </td>
                                            <td class="w-100 pl4 fs-90 lh3 v-top diff-line-{{type}}">
                                                <span class="wb-break dit ws-prewrap tab-4 diff-lineText">{{^subPieces}}{{text}}{{/subPieces}}{{#subPieces}}<span class="diff-piece-{{type}} diff-piece">{{text}}</span>{{/subPieces}}</span>
                                            </td>
                                        </tr>
                                    {{/lines}}
                                </tbody>
                            </table>
                        </div>
                    </script>

                    <script id="template-sideBySide-changes" type="x-tmpl-mustache">
                        <div id="diffBox" class="mx-auto mb4 br3 ba b-vlight-gray ov-hidden">
                            <table cellpadding="0" cellspacing="0" class="ff-mono w-100">
                                <thead>
                                    <tr class="ff-kanit">
                                        <th colspan=4 class="fw-b pa3 bg-off-white bb b-vlight-gray">
                                            <div class="df items-center justify-between">
                                                <span class="df items-center" title="{{changesTooltip}}">
                                                    <span class="pl3">{{totalLineEdits}}</span>
                                                    <span class="pl3 pr5 w7 h4 py1 df">
                                                        <span style="width: {{percentInserts}}%;" class="diff-lineNumber-Inserted br b-off-white"></span>
                                                        <span style="width: {{percentDeletes}}%;" class="diff-lineNumber-Deleted br b-off-white"></span>
                                                        <span style="width: {{percentChanges}}%;" class="diff-lineNumber-Modified"></span>
                                                    </span>
                                                </span>
                                                <span>{{oldRevisionName}} => {{newRevisionName}}</span>
                                                <div class="btn-group">
                                                    <button type="button" class="btn btn-sm btn-default" id="btn-diffBox-to-unified" onclick="changeDiff('unified')">
                                                        <i class="far fa-list"></i>
                                                        {{unifiedTitle}}
                                                    </button>
                                                    <button type="button" class="btn btn-sm btn-default active cursor-default">
                                                        <i class="far fa-columns"></i>
                                                        {{splitTitle}}
                                                    </button>
                                                    {{#isCollapseLines}}
                                                        <button type="button" class="btn btn-sm btn-default" onclick="toggleCollapseSideBySide()">
                                                            <span id="btn-diff-sideBySide-toggle-to-expand" title="{{expandTitle}}">
                                                                <i class="far fa-arrow-to-top"></i>
                                                                <i class="far fa-arrow-to-bottom"></i>
                                                            </span>
                                                            <span id="btn-diff-sideBySide-toggle-to-collapse" class="dn" title="{{collapseTitle}}">
                                                                <i class="far fa-arrow-from-top"></i>
                                                                <i class="far fa-arrow-from-bottom"></i>
                                                            </span>
                                                        </button>
                                                    {{/isCollapseLines}}
                                                </div>
                                            </div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody id="tbody-diff-sideBySide" {{#isCollapseLines}}class="diff-is-collapsed"{{/isCollapseLines}}>
                                    {{#lines}}
                                        <tr {{#show}}class="diff-row-show{{#isSummary}} diff-row-summary{{/isSummary}}"{{/show}}>
                                            <td class="pr3 pl4 pt2 tr fs-80 light-silver lh3 br b-vlight-gray v-top diff-lineNumber-{{oldType}}">
                                                {{ oldPosition }}
                                            </td>
                                            <td class="w-50 pl4 fs-90 lh3 v-top diff-line-{{oldType}}">
                                                <span class="wb-break dit ws-prewrap tab-4 diff-lineText">{{^oldSubPieces}}{{oldText}}{{/oldSubPieces}}{{#oldSubPieces}}<span class="diff-piece-{{type}} diff-piece">{{text}}</span>{{/oldSubPieces}}</span>
                                            </td>

                                            <td class="pr3 pl4 pt2 tr fs-80 light-silver lh3 br b-vlight-gray v-top diff-lineNumber-{{newType}}">
                                                {{ newPosition }}
                                            </td>
                                            <td class="w-50 pl4 fs-90 lh3 v-top diff-line-{{newType}}">
                                                <span class="wb-break dit ws-prewrap tab-4 diff-lineText">{{^newSubPieces}}{{newText}}{{/newSubPieces}}{{#newSubPieces}}<span class="diff-piece-{{type}} diff-piece">{{text}}</span>{{/newSubPieces}}</span>
                                            </td>
                                        </tr>
                                    {{/lines}}
                                </tbody>
                            </table>
                        </div>
                    </script>
                </asp:Panel>

                <asp:Panel ID="pnlPreview" runat="server" CssClass="TabControlPanel">
                    <div id="codePreview"  style="display: none">                                
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
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlPreviewPrevious" runat="server" CssClass="TabControlPanel">
                    <div id="codePreviewPrevious"  style="display: none">                                
                    </div>
                    <div id="noPreviewPrevious" style="display: none">
                        <div class="alert alert-info alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Messages,SourceCodeFileDetails_PreviewNotAvailable %>" />
                        </div>
                    </div>
                    <div id="imagePreviewPrevious" class="preview-image" style="display: none">
                        <a id="imgPreviewPreviousHyperLink" target="_blank">
                            <img id="imgPreviewPrevious" />
                        </a>
                    </div>
                    <div id="markdownPreviewPrevious"  style="display: none">                                
                    </div>
                </asp:Panel>

            </div>
        </div>
	</div>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Scripts>
        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.mustache.js" />
        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.prism.js" />
        <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" /> 
        <asp:ScriptReference Path="~/TypeScript/SyntaxHighlighting.js" />
        <asp:ScriptReference Path="~/TypeScript/SourceCodeRevisionFileDetails.js" />        
      </Scripts>
      <Services>
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionFileService.svc" />  
      </Services>  
    </tstsc:ScriptManagerProxyEx>
    <script type="text/javascript">
        //Get the initial variables from the server side
        var fileKey = '<%=GlobalFunctions.JSEncode(fileKey)%>';
	    var branchKey = '<%=GlobalFunctions.JSEncode(branchKey)%>';
        var revisionKey = '<%=GlobalFunctions.JSEncode(revisionKey)%>';
        var filename = '';
        var mimeType = '';
        var previousRevisionKey = '';
        var revisionName = '';
        var previousRevisionName = '';
        var diffMode = '<%=GlobalFunctions.JSEncode(DiffMode)%>';

        //Url templates
        var urlTemplate_commitDetails = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "={0}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "={1}"))%>';
        var urlTemplate_fileDetails = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl("~/SourceCodeFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "={0}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "={1}"))%>';
        var urlTemplate_commitFileDetails = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl("~/SourceCodeRevisionFileDetails.aspx?" + GlobalFunctions.PARAMETER_PROJECT_ID + "={0}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_REVISION_KEY + "={1}&" + GlobalFunctions.PARAMETER_SOURCE_CODE_FILE_KEY + "={2}"))%>';

        SpiraContext.uiState.pnlPreview = '<%=pnlPreview.ClientID%>';
        SpiraContext.uiState.pnlPreviewPrevious = '<%=pnlPreviewPrevious.ClientID%>';

        $(document).ready(function () {
            //Load the main data
            loadPageData(branchKey, fileKey, revisionKey);

            // make sure the preview tabs render properly - needs to do each time you open the tab to make sure line numbers align correctly
            var previewObserver = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutationRecord) {
                    Prism.highlightAllUnder(mutationRecord.target);
                });    
            });

            var target = document.getElementById(SpiraContext.uiState.pnlPreview);
            var targetPrevious = document.getElementById(SpiraContext.uiState.pnlPreviewPrevious);
            previewObserver.observe(target, { attributes : true, attributeFilter : ['style'] });
            previewObserver.observe(targetPrevious, { attributes : true, attributeFilter : ['style'] });
        });

        function loadPageData(branchKey, fileKey, revisionKey)
        {
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCode_RetrieveDataItem(SpiraContext.ProjectId, fileKey, revisionKey, branchKey, loadPageData_success, loadPageData_failure);
        }
        function loadPageData_success(dataItem)
        {
            //Get the key data used by the TypeScript file
            fileKey = dataItem.Fields.FileKey.textValue;
            revisionKey = dataItem.Fields.Revision.textValue;
            revisionName = dataItem.Fields.Revision.caption;
            mimeType = dataItem.Fields._MimeType.textValue;
            var revisionUrl = urlTemplate_commitDetails.replace('{0}', SpiraContext.ProjectId).replace('{1}', revisionKey);
            var fileUrl = urlTemplate_fileDetails.replace('{0}', SpiraContext.ProjectId).replace('{1}', fileKey);
            
            //Populate the fields
            filename = dataItem.Fields.Filename.textValue;
            var viewingUrl = dataItem.customUrl;
            var fileTypeUrl = SpiraContext.BaseThemeUrl + 'Images/' +  dataItem.Fields.FileType.tooltip;
            $('#<%=this.lblFileName.ClientID%>').text(filename);
            $('#<%=this.lnkFileName.ClientID%>').attr('href', viewingUrl);
            $('#<%=this.lnkRevision.ClientID%>').text(revisionName);
            $('#<%=this.lnkRevision.ClientID%>').attr('href', revisionUrl);
            $('#<%=this.lblChanger.ClientID%>').text(dataItem.Fields.AuthorName.textValue);
            $('#<%=this.lblChangeDate.ClientID%>').text(dataItem.Fields.LastUpdated.tooltip);
            $('#<%=this.lblSize.ClientID%>').text(dataItem.Fields.Size.textValue);
            $('#<%=this.imgFileType.ClientID%>').attr('src', fileTypeUrl).attr('alt', dataItem.Fields.FileType.textValue);
            $('#<%=this.lnkFilePath.ClientID%>').attr('href', fileUrl);

            //Add tooltips (using new has-tooltip > is-tooltip CSS)
            $('#lnkRevisionTooltip').text(dataItem.Fields.Revision.tooltip);

            //Tab name
            var tclFileDetails = $find('<%=this.tclFileDetails.ClientID%>');
            tclFileDetails.get_tabPages()[2].set_caption(revisionName);

            //Previous commit
            if (dataItem.Fields.PreviousRevision)
            {
                previousRevisionKey = dataItem.Fields.PreviousRevision.textValue;
                previousRevisionName = dataItem.Fields.PreviousRevision.caption;

                var previousRevisionFileUrl = urlTemplate_commitFileDetails.replace('{0}', SpiraContext.ProjectId).replace('{1}', previousRevisionKey).replace('{2}', fileKey);
                $('#<%=this.lnkPreviousChange.ClientID%>').text(previousRevisionName);
                $('#<%=this.lnkPreviousChange.ClientID%>').attr('href', previousRevisionFileUrl);
                $get('<%=this.lnkPreviousChange.ClientID%>').setAttribute('tst:revisionkey', previousRevisionKey);
                $('#lnkPreviousChangeTooltip').text(dataItem.Fields.PreviousRevision.tooltip);
                tclFileDetails.get_tabPages()[1].set_caption(previousRevisionName);
                tclFileDetails.get_tabPages()[0].set_visible(true);
                tclFileDetails.get_tabPages()[1].set_visible(true);
                $('#<%=boxPreviousChange.ClientID%>').show();
            }
            else
            {
                tclFileDetails.get_tabPages()[0].set_visible(false);
                tclFileDetails.get_tabPages()[1].set_visible(false);
                tclFileDetails.set_selectedTabClientId(tclFileDetails.get_tabPages()[2].get_tabPageClientId());
                $('#<%=boxPreviousChange.ClientID%>').hide();
            }
            
            //Force reload of the tab control
            tclFileDetails.get_element().innerHTML = '';
            tclFileDetails.initialize();

            //Populate the window title
            window.document.title = resx.ArtifactType_SourceCodeRevision + ' ' + revisionName + ' - ' + filename + ' | ' + SpiraContext.ProductType;

            //Navigation sidebar
            load_navigation_sidebar(branchKey, fileKey, revisionKey, dataItem, revisionName, revisionUrl, fileUrl);

            //Load the preview panel
            sourceCodeRevisionFileAction.updatePreview(branchKey, fileKey, revisionKey, previousRevisionKey, (diffMode == 'unified'));

            //hide spinner
            globalFunctions.hide_spinner();
        }
        function loadPageData_failure (ex)
        {
            //hide spinner
            globalFunctions.hide_spinner();

            var lblMessage = $get('<%=this.lblMessage.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
        }
        
        function load_navigation_sidebar(branchKey, fileKey, revisionKey, dataItem, revisionName, revisionUrl, fileUrl)
        {
            //Display the current revision
            $('#a-navigation-current-revision').text(revisionName).attr('href', revisionUrl);
            $('#a-navigation-current-revision-tooltip').text(dataItem.Fields.Revision.tooltip);

            //Display the current file
            var fileTypeUrl = SpiraContext.BaseThemeUrl + 'Images/' + dataItem.Fields.FileType.tooltip;
            $('#img-navigation-current-file').attr('src', fileTypeUrl).attr('alt', dataItem.Fields.FileType.textValue);
            $('#a-navigation-current-file').text(dataItem.Fields.Filename.textValue).attr('href', fileUrl);
            $('#a-navigation-current-file-tooltip').text(fileKey);

            //Load revisions for the current file (and the reverse)
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCode_RetrieveRevisionsForFile(SpiraContext.ProjectId, branchKey, fileKey, SpiraContext.BaseThemeUrl, load_navigation_revisions_success, loadPageData_failure, revisionKey);
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCode_RetrieveFilesForRevision(SpiraContext.ProjectId, branchKey, revisionKey, SpiraContext.BaseThemeUrl, load_navigation_files_success, loadPageData_failure, fileKey);
        }
        function load_navigation_revisions_success(dataItems, currentRevisionKey)
        {
            var template = document.getElementById('template-revisions').innerHTML;
            var rendered = Mustache.render(template, dataItems);
            document.getElementById('target-revisions').innerHTML = rendered;

            //Highlight the current revision
            $("#target-revisions [tst\\:revisionkey='" + currentRevisionKey + "']").addClass('Selected');
        }
        function load_navigation_files_success(dataItems, currentFileKey) {
            var template = document.getElementById('template-files').innerHTML;
            var rendered = Mustache.render(template, dataItems);
            document.getElementById('target-files').innerHTML = rendered;

            //Highlight the current file
            $("#target-files [tst\\:filekey='" + currentFileKey + "']").addClass('Selected');
        }

        //Live Loading functions
        function load_revision(evt, newRevisionKey)
        {
            //Ignore shift/ctrl clicks
            if (!evt.shiftKey && !evt.ctrlKey && !evt.metaKey)
            {
                //Just reload the page's data
                loadPageData(branchKey, fileKey, newRevisionKey);

                //Also need to rewrite the URL to match
                if (history && history.pushState)
                {
                    var url = urlTemplate_commitFileDetails.replace('{0}', SpiraContext.ProjectId).replace('{1}', newRevisionKey).replace('{2}', fileKey);
                    history.pushState({ fileKey: fileKey, revisionKey: newRevisionKey }, null, url);
                }

                //Stop normal navigation
                evt.stopPropagation();
                evt.preventDefault();
            }
        }
        function load_file(evt, newFileKey) {
            //Ignore shift/ctrl clicks
            if (!evt.shiftKey && !evt.ctrlKey && !evt.metaKey) {
                //Just reload the page's data
                loadPageData(branchKey, newFileKey, revisionKey);

                //Also need to rewrite the URL to match
                if (history && history.pushState) {
                    var url = urlTemplate_commitFileDetails.replace('{0}', SpiraContext.ProjectId).replace('{1}', revisionKey).replace('{2}', newFileKey);
                    history.pushState({ fileKey: newFileKey, revisionKey: revisionKey }, null, url);
                }

                //Stop normal navigation
                evt.stopPropagation();
                evt.preventDefault();
            }
        }

        //Change DIFF mode
        function changeDiff(mode)
        {
            //Update the on-page setting
            diffMode = mode;

            //Store on the server (fail quietly)
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionFileService.SourceCode_UpdateDiffViewSetting(SpiraContext.ProjectId, (mode == 'unified'));
        }

        //Toggle collapsed mode
        function toggleCollapseUnified() {
            document.getElementById("tbody-diff-unified").classList.toggle("diff-is-collapsed");
            document.getElementById("btn-diff-unified-toggle-to-expand").classList.toggle("dn");
            document.getElementById("btn-diff-unified-toggle-to-collapse").classList.toggle("dn");
        }
        function toggleCollapseSideBySide() {
            document.getElementById("tbody-diff-sideBySide").classList.toggle("diff-is-collapsed");
            document.getElementById("btn-diff-sideBySide-toggle-to-expand").classList.toggle("dn");
            document.getElementById("btn-diff-sideBySide-toggle-to-collapse").classList.toggle("dn");
        }

        //Page objects
        var tabControl_id = '<%=tclFileDetails.ClientID%>';

        //URL templates
        var sourceCodeViewer_urlTemplate = '<%=SourceCodeFileViewerUrl%>';
    </script>
</asp:Content>
