<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="SourceCodeRevisionDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.SourceCodeRevisionDetails" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    Title="Untitled Page" 
%>

<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
			<tstsc:NavigationBar 
                AutoLoad="true"  
                EnableLiveLoading="true" 
                ErrorMessageControlId="lblMessage" 
                FormManagerControlId="ajxFormManager"
                ID="navRevisionList" 
                IncludeAssigned="false" 
                IncludeDetected="false"
                ItemImage="Images/artifact-Revision.svg"
				ListScreenCaption="<%$Resources:Main,SourceCodeRevisionDetails_BackToList%>" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" 
                />
		</div>


        <div class="main-panel pl4 grow-1">                 
			<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

            <div class="mt3 py2 px3" runat="server" title="<%$Resources:Main,SourceCodeList_CurrentBranch%>"> 
                <span class="fas fa-code-branch"></span> 
                <asp:Label ID="lblBranchName" runat="server" /> 
            </div>

            <div class="flex justify-between items-center pr4 py3 sticky top-nav bg-white z-10">
                <div class="flex items-center">
                    <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                        <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" ToolTip="<%$Resources:Main,SourceCodeRevisionDetails_BackToList%>">
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperlinkEx>
                    </div> 
                    <div class="mr3">
                        <tstsc:ImageEx CssClass="w4 h4" ID="imgIcon" runat="server" ImageUrl="Images/artifact-Revision.svg" AlternateText="<%$Resources:Fields,Revision %>" />
				        <tstsc:LabelEx
                            Tooltip="<%$Resources:Buttons,CopyToClipboard %>"
                            ID="lblRevisionName"
                            CssClass="pointer dib transition-all orange-hover"
                            runat="server" />
                    </div>
                   
                    <h2 class="dib my0 fs-h3">
                        <tstsc:LabelEx ID="txtRevisionNotes" runat="server" />
                    </h2>
                </div>
            </div>

            <div class="py3 px4 mb2 bg-near-white br2 df items-center flex-wrap mr4 justify-between">
                <div>
                    <tstsc:LabelEx ID="lblAuthor" runat="server" />
                    <span class="fs-90 ml2">
                        (<tstsc:LabelEx ID="lblLastEdited" runat="server" />)
                    </span>
                </div>
                <div>
                    <tstsc:LabelEx
                        AppendColon="true" 
                        AssociatedControlID="lnkBuild" 
                        ID="lnkBuildLabel" 
                        CssClass="my0"
                        runat="server" 
                        Text="<%$Resources:Fields,BuildId %>" 
                        />
                    <tstsc:ArtifactHyperLink 
                        AlternateText="<%$Resources:Fields,Build %>"
                        CssClass="ArtifactHyperLink"
                        DisplayChangeLink="false"
                        ID="lnkBuild" 
                        ItemImage="Images/artifact-Build.svg" 
                        runat="server" 
                        />
                </div>
            </div>  
                        



			<tstsc:TabControl ID="tclRevisionDetails" CssClass="TabControl2" TabWidth="130" TabHeight="25"
				TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
				runat="Server">
				<TabPages>
					<tstsc:TabPage 
                        AjaxServerControlId="grdSourceCodeFileList" 
                        Caption="<%$Resources:ServerControls,TabControl_Files %>" 
                        ID="tabFiles" 
                        runat="server" 
                        TabPageControlId="pnlFiles"
                        TabPageImageUrl="Images/artifact-Document.svg" 
                        />
                    <tstsc:TabPage 
                        Caption="<%$Resources:Fields,Branches %>" 
                        ID="TabBranches" 
                        runat="server" 
                        TabPageControlId="pnlBranches" 
                        TabPageIcon="fas fa-code-branch"
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





            <%-- FILES PANEL --%>
			<asp:Panel ID="pnlFiles" runat="server" CssClass="TabControlPanel">
				<div style="width: 100%" class="TabControlHeader">
					<div class="btn-group priority3">
                        <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkRefreshFiles" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdSourceCodeFileList" ClientScriptMethod="load_data()">
                            <span class="fas fa-sync"></span>
                            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
                        </tstsc:HyperLinkEx>
                        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                ClientScriptServerControlId="grdSourceCodeFileList" ClientScriptMethod="apply_filters()">
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
	                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SourceCodeList_FilesInCommit %>" />.
                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                </div>

				<tstsc:SortedGrid ID="grdSourceCodeFileList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
					SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="lblMessage"
					RowCssClass="Normal" DisplayAttachments="false" AllowEditing="false" 
                    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
					runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService" />
			</asp:Panel>



            <%-- BRANCHES PANEL --%>
            <asp:Panel ID="pnlBranches" runat="server" CssClass="TabControlPanel">
				<div id="target-branches" class="pa4">
                    <asp:Localize runat="server" Text="<%$Resources:ClientScript,GlobalFunctions_TooltipLoading %>" />
                </div>
                <script id="template-branches" type="x-tmpl-mustache">
                    <ul 
                        class="u-box_list" 
                        id="branches" 
                        >
                        {{#.}}
                            <li class="lh3">
                                <span class="mr1 fas fa-code-branch"></span>
                                <a href="javascript:changeBranch('{{.}}')">
                                {{ . }}
                                </a>
                            </li>
                        {{/.}}
                    </ul>
                </script>
            </asp:Panel>



            <%-- ASSOCIATIONS PANEL --%>
			<asp:Panel ID="pnlAssociations" runat="server" CssClass="TabControlPanel">
                <tstuc:AssociationsPanel 
                    ID="tstAssociationPanel" 
                    runat="server" 
                    />
			</asp:Panel>
        </div>
	</div>


    <tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage" ArtifactImageControlId="imgIcon"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" ArtifactTypeName="<%$Resources:Fields,Revision%>"
        ItemImage="Images/artifact-Revision.svg" AlternateItemImage="Images/artifact-Revision.svg" DisplayPageName="false" NameField="Name"
        WorkflowEnabled="false" CheckUnsaved="false">
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblRevisionName" DataField="Name" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtRevisionNotes" DataField="Message" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAuthor" DataField="AuthorName" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblLastEdited" DataField="UpdateDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblContentChanged" DataField="ContentChanged" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblPropertiesChanged" DataField="PropertiesChanged" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lnkBuild" DataField="BuildId" Direction="In" />
        </ControlReferences>
    </tstsc:AjaxFormManager>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />        
            <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.mustache.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
        //Set initial load status
	    var initialLoad = true;

	    var artifactTypes = [];
	    $(document).ready(function(){
	        artifactTypes = globalFunctions.getArtifactTypes();

            //Add the text area used for copying things into clipboard
            //This code is based off the function handleCopyToClipbard in GlobalFunctions.js
            //Repeated here because we want to copy not the element contents but a different field - and we can't (as of 6.7.1) set 2 data items on a single control
	        var textArea = document.createElement("textarea");
	        textArea.className = "fixed top0 left0 w0 h0";
	        textArea.visibility = "hidden";
	        textArea.id = "copyToClipboardHiddenTextarea";
	        document.body.appendChild(textArea);

            //If they click on the revision name, put key into clipboard
			$('#<%=lblRevisionName.ClientID%>').on("click", function(){
	            var element = $(this)[0];

	            //Get the full commit from the form manager
                var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
	            var dataItem = ajxFormManager.get_dataItem();
	            var revisionKey = dataItem.Fields.RevisionKey.textValue;

	            //Put in the clipboard
	            var copyTextarea = document.getElementById("copyToClipboardHiddenTextarea");
	            copyTextarea.value = revisionKey;
	            copyTextarea.focus();
	            copyTextarea.select();
	            try {
	                var success = document.execCommand("copy");
	                //handle animation on success
	                element.classList.add("u-mini-bounceIn");
	                setTimeout(function () { element.classList.remove("u-mini-bounceIn") }, 1000);

	            } catch (err) {
	                console.log("could not copy the text as requested")
	            }	           
	        });
	    });

        function changeBranch(branchPath)
        {
            //Set the new branch
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCode_SetSelectedBranch(SpiraContext.ProjectId, branchPath, changeBranch_success, page_operation_failure, branchPath);
        }
        function changeBranch_success(data, branchPath)
        {
            //Change the branch display
            var lblBranchName = $get('<%=this.lblBranchName.ClientID%>');
            lblBranchName.innerHTML = branchPath; 

            //Reload the navigation bar
            var navRevisionList = $find('<%=this.navRevisionList.ClientID%>');
            navRevisionList.load_data(true);

            //Reload the file grid
            var grdSourceCodeFileList = $find('<%=this.grdSourceCodeFileList.ClientID%>');
            var standardfilters = grdSourceCodeFileList.get_standardFilters();
            standardfilters[globalFunctions.keyPrefix + 'BranchKey'] = globalFunctions.serializeValueString(branchPath);
            grdSourceCodeFileList.set_standardFilters(standardfilters);
            grdSourceCodeFileList.load_data();

            //Always get the latest count to see if we 'have data'
            var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
	        var dataItem = ajxFormManager.get_dataItem();
            var revisionKey = dataItem.Fields.RevisionKey.textValue;
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCodeFile_CountForRevision(SpiraContext.ProjectId, revisionKey, updateFilesGrid_checkData);
        }
        function page_operation_failure(ex) {
            var lblMessage = $get('<%=this.lblMessage.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
        }

	    function ajxFormManager_loaded_sourceCode(dontClearMessages)
	    {
	        //Get the current data item
	        var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
	        var dataItem = ajxFormManager.get_dataItem();
	        SpiraContext.ArtifactId = ajxFormManager.get_primaryKey();
	        SpiraContext.IsArtifactCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();

	        //Change the page title
	        window.document.title = resx.ArtifactType_SourceCodeRevision + ' ' + dataItem.Fields.Name.textValue + ' | ' + SpiraContext.ProductType;

	        //Make any artifact tokens hyperlinks
	        makeArtifactTokensLinks();

	        var revisionKey = dataItem.Fields.RevisionKey.textValue;
	        if (!initialLoad)
	        {
	            //Update the nav bar
	            updateNavBar(dontClearMessages);

	            //Update the files grid
	            updateFilesGrid(revisionKey);

                //Update the associations grid
	            updateAssociationsGrid();
            }
	        else
	        {
                initialLoad = false;

                //Set the permissions on the association grid
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstAssociationPanel.load_data(filters, false);
	        }

	        //Always get the latest count to see if we 'have data'
	        Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCodeFile_CountForRevision(SpiraContext.ProjectId, revisionKey, updateFilesGrid_checkData);

            //See if we have any associations
	        if (typeof(tstuc_cplMainContent_tstAssociationPanel) != 'undefined' && tstuc_cplMainContent_tstAssociationPanel)
	        {
	            tstuc_cplMainContent_tstAssociationPanel.check_hasData(associations_callback);
	        }

	        //Load the list of branches
	        loadBranchesList(revisionKey);
	    }
        
	    function makeArtifactTokensLinks()
	    {
	        var txtRevisionNotes = $get('<%=this.txtRevisionNotes.ClientID%>');
	        if (txtRevisionNotes)
	        {
	            var regex = /\[(?<key>[A-Z]{2})[:\-](?<id>\d*?)\]/gi;
	            var text = txtRevisionNotes.innerHTML;
	            if (text)
	            {
	                txtRevisionNotes.innerHTML = text.replace(regex, replacer);
	            }
	        }
	    }

	    function updateNavBar(dontClearMessages)
	    {
	        var navigationBar = $find('<%=this.navRevisionList.ClientID%>');
	        navigationBar.set_selectedItemId(SpiraContext.ArtifactId);
	        navigationBar.refresh_data(dontClearMessages);
	    }

	    function loadBranchesList(revisionKey)
	    {
	        Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_RetrieveBranches(SpiraContext.ProjectId, revisionKey, loadBranchesList_success, page_operation_failure);
	    }
	    function loadBranchesList_success(branches)
	    {
	        var template = document.getElementById('template-branches').innerHTML;
	        var rendered = Mustache.render(template, branches);
	        document.getElementById('target-branches').innerHTML = rendered;
	    }

	    function updateAssociationsGrid()
	    {
	        //See if the tab is visible
	        var loadNow = ($find('<%=tclRevisionDetails.ClientID%>').get_selectedTabClientId() == '<%=pnlAssociations.ClientID%>');

	        //Reload the tab's data
	        var filters = {};
	        filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
	        filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
	        tstuc_cplMainContent_tstAssociationPanel.load_data(filters, loadNow);
	    }

	    function updateFilesGrid(revisionKey)
	    {
	        var grdSourceCodeFileList = $find('<%=grdSourceCodeFileList.ClientID%>');
	        var filters = {};
	        filters[globalFunctions.keyPrefix + 'RevisionKey'] = globalFunctions.serializeValueString(revisionKey);
	        grdSourceCodeFileList.set_standardFilters(filters);
	        grdSourceCodeFileList.load_data();
	    }
	    function updateFilesGrid_checkData(count)
	    {
	        $find('<%=tclRevisionDetails.ClientID%>').updateHasData('tabFiles', (count > 0));
	    }
	    function associations_callback(hasData)
	    {
	        $find('<%=tclRevisionDetails.ClientID%>').updateHasData('tabAssociations', hasData);
	    }

	    function replacer(match, artifactPrefix, artifactId, offset, string) {
	        if (artifactPrefix && artifactId)
	        {
	            var artifactUrlPart = null;
	            for (var i = 0; i < artifactTypes.length; i++)
	            {
	                if (artifactTypes[i].token == artifactPrefix)
	                {
	                    artifactUrlPart = artifactTypes[i].val;
	                    break;
	                }
	            }
	            if (artifactUrlPart)
	            {
	                return '<a href="' + globalFunctions.replaceBaseUrl('~/' + SpiraContext.ProjectId + '/' + artifactUrlPart + '/' + artifactId + '.aspx') + '">[' + artifactPrefix + ':' + artifactId + ']</a>';
	            }
	        }
	        return '';
	    }
    </script>
</asp:Content>
