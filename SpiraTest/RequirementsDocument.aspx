<%@ Page 
    AutoEventWireup="True" 
    Codebehind="RequirementsDocument.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.RequirementsDocument" 
    language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    validateRequest="false" 
%>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DiagramsStylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
<div class="panel-container df">
    <div class="side-panel dn-sm dn-xs">
        <tstsc:SidebarPanel 
            SkinID="RelativeSidebarPanel"
            ErrorMessageControlId="lblMessage" 
            HeaderCaption="<%$Resources:Main,RequirementsDocument_PackageSelector %>"
            ID="pnlNavigation" 
            runat="server" 
            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
            >
            <div id="epics" class="h-insideHeadAndFoot">
                <tstsc:HyperLinkEx 
                    CssClass="tdn py2 pr2 pl3 db"
                    ID="lstPackages_artifactRoot"
                    ClientIDMode="Static"
                    NavigateUrl='javascript:focusOnRequirement(null)'
                    runat="server"
                    Text="<%$ Resources:Main,HierarchicalDocument_RootLink_Label %>"
                    Tooltip="<%$ Resources:Main,HierarchicalDocument_RootLink_Tooltip %>"
                    />
                <tstsc:DataListEx ID="lstPackages" runat="server" ShowFooter="False" ShowHeader="False" DataKeyField="RequirementId" ItemMaxCount="1000" RepeatLayout="Flow" CssClass="db"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService">
                    <ItemStyle Wrap="False" Height="16pt" CssClass="Normal w-100" />
                    <SelectedItemStyle Wrap="False" Height="16pt" CssClass="Selected" />
                    <ItemTemplate>
                        <div class="db pa2" id='<%# "lstPackages_artifactId-" + ((RequirementView) Container.DataItem).RequirementId %>'>
                            <asp:Label runat="server" Style='<%#"margin-left:" + ((((RequirementView) Container.DataItem).IndentLevel.Length - 3) * 5) + "px" %>' />
				            <tstsc:ImageEx 
                                CssClass="w4 h4"
                                Runat="server" AlternateText="<%$Resources:Fields,Requirement %>"
				                ImageUrl="Images/artifact-RequirementSummary.svg"
				                ID="imgPackage" ImageAlign="AbsMiddle" />
                            <tstsc:HyperLinkEx 
                                Font-Bold="False" 
                                CssClass="tdn"
                                ID="lnkFindPackage" 
                                NavigateUrl='<%#"javascript:focusOnRequirement(" + ((RequirementView) Container.DataItem).RequirementId + ")" %>'
                                runat="server" 
                                Data-PrimaryKey="<%#((RequirementView) Container.DataItem).RequirementId %>"
                                >
                                <span>
                                    <asp:Literal runat="server" Text="<%#: ((RequirementView) Container.DataItem).Name %>" />
                                </span>
                                <span>
                                    [<asp:Literal runat="server" Text="<%# ((RequirementView) Container.DataItem).ArtifactToken %>" />]
                                </span>
                            </tstsc:HyperLinkEx>
                        </div>
                    </ItemTemplate>
                </tstsc:DataListEx>
            </div>
        </tstsc:SidebarPanel>
    </div>


    <div class="main-panel pl4 grow-1">
        <div class="df flex-column h-insideHeadAndFoot">
            <div class="df items-center justify-between flex-wrap-xs mt3" id="main-panel-header">
                <h1 class="w-100-xs fs-h3 fw-b my3">
                    <asp:Localize runat="server" Text="<%$Resources:Main,SiteMap_RequirementsDocument %>" />
                </h1>
                <div class="dif">
                    <div class="btn-group mr3" role="group">
                        <button 
                            runat="server" 
                            type="button" 
                            class="btn btn-default" 
                            onclick="window.print()"
                            title="<%$ Resources:Buttons,Print %>"
                            >
                            <i class="fad fa-print"></i>
                        </button>
				        <tstsc:DropDownListEx 
                            ID="ddlShowHideColumns"
                            Runat="server" 
                            DataValueField="Key" 
                            DataTextField="Value" 
                            CssClass="DropDownList" 
                            AutoPostBack="false" 
                            NoValueItem="True" 
                            NoValueItemText="<%$Resources:Dialogs,Global_ShowHideFields %>" 
                            Width="180px" 
                            ClientScriptMethod="toggle_visibility"
                            />
                    </div>
                
                    <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                        <span class="btn-group priority1 mb4-xs ml3 ml0-xs" role="group">
                            <a class="btn btn-default mb0" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                                <span class="fas fa-indent"></span>
                            </a>
                            <a class="btn btn-default mb0" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -4) %>' runat="server" title="<%$ Resources:Main,Global_List %>">
                                <span class="fas fa-list"></span>
                            </a>
                            <a class="btn btn-default mb0" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -5) %>' runat="server" title="<%$ Resources:Main,Global_Board %>">
                                <span class="fas fa-align-left rotate90"></span>
                            </a>
                            <a class="btn btn-default mb0 active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -7) %>' runat="server" title="<%$ Resources:Main,Global_Document %>">
                                <span class="fas fa-paragraph"></span>
                            </a>
                            <a class="btn btn-default mb0" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                                <span class="fas fa-project-diagram"></span>
                            </a>
                        </span>
                    </asp:PlaceHolder>
                </div>
            </div>

            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
            <div id="requirementsDocument" class="w-100 mvw-100 relative mb4 ov-auto" role="document">
            </div>

            <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
        </div>
    </div>
</div>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
    </Services>  
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/rct_comp_hierarchicalDocument.js" />       
        <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.graphlib-dot.js" Assembly="Web" />
        <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.dagre-d3.js" Assembly="Web" />
        <asp:ScriptReference Path="~/ckEditor5/ckeditor.js" />
        <asp:ScriptReference Path="~/ckEditor5/react/ckeditor.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>  
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        // Create and configure the diagram renderer
        var render = dagreD3.render();
        var g;
        var lstPackages_id = '<%=this.lstPackages.ClientID%>';
        SpiraContext.uiState.showOutlineCode = '<%=ShowOutlineCode%>' == "true";
        SpiraContext.uiState.parentRequirementId = '<%=ParentRequirementId%>'
        
        $(document).ready(function () {
            //Display the requirements document
            react_HierarchicalDocument.documentDomId = 'requirementsDocument';
            react_HierarchicalDocument.messageBoxId = '<%=lblMessage.ClientID%>';
            react_HierarchicalDocument.artifactToken = '<%=GlobalFunctions.ARTIFACT_PREFIX_REQUIREMENT%>';
            react_HierarchicalDocument.artifactType = globalFunctions.artifactTypeEnum.requirement;
            react_HierarchicalDocument.webServiceClass = Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService;
            react_HierarchicalDocument.itemImage = 'artifact-Requirement.svg';
            react_HierarchicalDocument.alternateItemImage = 'artifact-UseCase.svg';
            react_HierarchicalDocument.summaryItemImage = 'artifact-RequirementSummary.svg';
            react_HierarchicalDocument.urlTemplate = '<%=UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -3)%>';
            react_HierarchicalDocument.urlUploadTemplate = '<%=UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3)%>';
            react_HierarchicalDocument.showOutlineCode = SpiraContext.uiState.showOutlineCode;
            react_HierarchicalDocument.parentRequirementId = SpiraContext.uiState.parentRequirementId || null; /* make sure we set an empty string to null */
            react_HierarchicalDocument.loadData();

            //Add the event handler on the package tooltips
            $('#epics a[data-primarykey]').on("mouseover", function () {
                var requirementId = $(this)[0].getAttribute('data-primarykey');
                $find(lstPackages_id).display_tooltip(requirementId, SpiraContext.ProjectId);
            });
			$('#epics a[data-primarykey]').on("mouseout", function () {
                $find(lstPackages_id).hide_tooltip();
            });
        });

        function focusOnRequirement(requirementId)
        {
            //Jump to the specific requirement
            if (window && window.rct_comp_hierarchicalDocument && window.rct_comp_hierarchicalDocument.focusOnRequirement) {
				window.rct_comp_hierarchicalDocument.focusOnRequirement(requirementId);
            }
        }
        function showEpicTooltip(requirementId)
        {

        }
        function hideEpicTooltip(requirementId)
        {

        }
        // called when show/hide column dropdown is changed
        function toggle_visibility(select) {

            // check we have a value
            if (select.get_value() != '') {
                //set the uistate this to local this so we can access later
                SpiraContext.uiState.this = this;

                // call a global confirm dialog if we have any unsaved changes on the page
                if (SpiraContext.uiState.hierarchicalDocumentHasChanges == true) {
                    globalFunctions.globalConfirm(
                        resx.AjxFormManager_UnsavedChanges,
                        "info",
                        this.toggle_visibility_confirm,
                        select,
                        null);
                } else {
                    this.toggle_visibility_confirm(true, select);
                }
            }
        }
        // if no unsaved changes, or user has confirmed to discard changes, proceed with changing fields to show (which will cause reloading of the page)
        function toggle_visibility_confirm(shouldContinue, select) {
            if (shouldContinue && select.get_value()) {
                SpiraContext.uiState.hierarchicalDocumentFieldName = select.get_value();

                //Call the appropriate webservice to toggle the visibility
                //NOTE: we use SpiraContext.uiState.this instead of just this, because the function can be called from a different context (globalConfirm), so need to ensure we always have the correct context
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.HierarchicalDocument_ToggleFieldVisibility(
                    SpiraContext.ProjectId,
                    SpiraContext.uiState.hierarchicalDocumentFieldName,
                    Function.createDelegate(SpiraContext.uiState.this, SpiraContext.uiState.this.toggle_visibility_success),
                    Function.createDelegate(SpiraContext.uiState.this, SpiraContext.uiState.this.toggle_visibility_failure)
                );

                //Now reset the dropdown and change show to hide or vice versa
                var currentLabel = select.get_text();
                var labelLength = currentLabel.length;
                if (labelLength > 4) {
                    if (currentLabel.substring(0, resx.Global_Show.length) == resx.Global_Show) {
                        select.set_text(resx.Global_Hide + currentLabel.substring(resx.Global_Show.length, labelLength));
                    }
                    else if (currentLabel.substring(0, resx.Global_Hide.length) == resx.Global_Hide) {
                        select.set_text(resx.Global_Show + currentLabel.substring(resx.Global_Hide.length, labelLength));
                    }
                }
                select.get_parent().set_selectedItem('');
            }
        }
        function toggle_visibility_success(data) {
            //See if we have a validation error or not
            globalFunctions.hide_spinner();
            //If the user toggled the outline code - update the page stored value now
            if (SpiraContext.uiState.hierarchicalDocumentFieldName == "OutlineCode") {
                SpiraContext.uiState.showOutlineCode = !SpiraContext.uiState.showOutlineCode;
                react_HierarchicalDocument.showOutlineCode = SpiraContext.uiState.showOutlineCode;
            }
            // get the current parentRequirement stored in state
            react_HierarchicalDocument.parentRequirementId = rct_comp_hierarchicalDocument.state.parentRequirementId;
            // reload the data
            ReactDOM.unmountComponentAtNode(document.getElementById('requirementsDocument'));
			react_HierarchicalDocument.loadData();
            // reset the onpage check for if the doc has changes
            window.onbeforeunload = null;
            SpiraContext.uiState.hierarchicalDocumentHasChanges == false;
        }
        function toggle_visibility_failure(error) {
            //Populate the error message control if we have one (if not use alert instead)
            globalFunctions.hide_spinner();
            //Display validation exceptions in a friendly manner
            globalFunctions.globalAlert(exception, "error");
        }
    </script>

    <style>
        @media print {
            #global-navigation,
            .nav-secondary,
            .side-panel,
            .hierarchical-body-item-edit,
            #main-panel-header,
            footer,
            .footer {
                display: none;
            }

            #requirementsDocument {
                overflow: visible;
                max-width: 100vw;
            }

            .hierarchical-body-item-fields {
                background-color: var(--vlight-gray);
                border-top: 1px solid #999;
                border-bottom: 2px solid #999;
                border-radius: 0;
                padding: .125rem;
            }

            a[href]:after {
		        content: none !important;
	        }
            a {
		        text-decoration: none;
	        }
        }

    </style>
</asp:Content>
