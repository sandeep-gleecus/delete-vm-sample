<%@ Page 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    AutoEventWireup="True"
    CodeBehind="Documents.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.Documents" 
    Title="Untitled Page" 
    %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Register TagPrefix="tstuc" TagName="QuickFilterPanel" Src="~/UserControls/QuickFilterPanel.ascx" %>
<asp:Content 
    ID="Content1" 
    ContentPlaceHolderID="cplHead" 
    runat="server"
    >
</asp:Content>
<asp:Content 
    ID="Content2" 
    ContentPlaceHolderID="cplMainContent" 
    runat="server"
    >
    <div class="panel-container flex flex-column-reverse-sm">
        <div class="side-panel hidden-sm-second-child sticky top-nav self-start">
            <tstsc:SidebarPanel 
                ID="pnlFolders" 
                runat="server" 
                HeaderCaption="<%$Resources:Main,Global_Folders %>" 
                MinWidth="100" 
                MaxWidth="500" 
                data-panel="folder-tree"
                DisplayRefresh="true" 
                ClientScriptServerControlId="trvFolders" 
                ClientScriptMethod="load_data(true)" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <div class="Widget panel">
                    <tstsc:TreeView 
                        ID="trvFolders" runat="server" 
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
                        LoadingImageUrl="Images/action-Spinner.svg" 
                        CssClass="FolderTree" ErrorMessageControlId="lblMessage"
                        NodeLegendControlId="txtFolderInfo" 
                        NodeLegendFormat="{0}"
                        AllowEditing="true"
                        Authorized_Permission="BulkEdit"
                        Authorized_ArtifactType="Document"
                        ItemName="<%$Resources:Fields,Folder %>"
                        ClientScriptMethod="trvFolders_folderChanged" 
                        />
                </div>
            </tstsc:SidebarPanel>
            <tstsc:SidebarPanel ID="pnlQuickFilters" runat="server" HeaderCaption="<%$Resources:Dialogs,SidebarPanel_QuickFilters %>" MinWidth="100" MaxWidth="500"
                ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <tstuc:QuickFilterPanel ID="ucQuickFilterPanel" runat="server" ArtifactType="Document" FolderLegendControlId="txtFolderInfo"
                    DisplayReleases="false"
                    AjaxServerControlId="grdDocumentList" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"  />
            </tstsc:SidebarPanel>
            <tstsc:SidebarPanel 
                ID="pnlTagCloud" 
                runat="server" 
                HeaderCaption="<%$Resources:Main,Documents_TagCloud %>" 
                MinWidth="100" 
                MaxWidth="500" 
                DisplayRefresh="false"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <div class="Widget panel">
                    <tstsc:TagCloud 
                        ID="tagCloud" 
                        runat="server" 
                        SkinID="TagCloud" 
                        DataCountField="Frequency" 
                        DataKeywordField="Name" 
                        DataMember="ProjectAttachmentTags"
                        Debug="true" 
                        KeywordURLFormat="javascript:page.tag_click('%k')" 
                        />
                </div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                <div class="btn-group priority1" role="group">
                    <tstsc:DropMenu 
                        id="btnInsert" 
                        runat="server" 
                        GlyphIconCssClass="mr3 fas fa-plus"
                        Authorized_ArtifactType="Document" 
                        Authorized_Permission="Create"
					    ClientScriptMethod="page.add_document('file')" 
                        Text="<%$Resources:Dialogs,Documents_AddDocument %>">
					    <DropMenuItems>
						    <tstsc:DropMenuItem 
                                Name="Upload" 
                                Runat="server"
                                Value="<%$Resources:Buttons,Upload %>" 
                                GlyphIconCssClass="mr3 fas fa-upload" 
                                ClientScriptMethod="page.add_document('file')" 
                                />
                            <tstsc:DropMenuItem Divider="true" Runat="server" />
						    <tstsc:DropMenuItem 
                                Name="NewUrl" 
                                Runat="server"
                                Value="<%$Resources:Fields,URL %>" 
                                ImageUrl="Images/Filetypes/Link.svg"
                                ClientScriptMethod="page.add_document('url')" 
                                />
						    <tstsc:DropMenuItem 
                                Name="NewScreenshot"
                                Runat="server"
                                Value="<%$Resources:Fields,Screenshot %>" 
                                GlyphIconCssClass="mr3 fas fa-image" 
                                ClientScriptMethod="page.add_document('image')" 
                                />
                            <tstsc:DropMenuItem Divider="true" Runat="server" />
						    <tstsc:DropMenuItem 
                                Name="NewMarkdown"
                                Runat="server"
                                Value="<%$Resources:Dialogs,Global_Markdown %>" 
                                ImageUrl="Images/Filetypes/Markdown.svg"
                                ClientScriptMethod="page.add_document('markdown')" 
                                />
						    <tstsc:DropMenuItem 
                                Name="NewRichText" 
                                Runat="server"
                                Value="<%$Resources:Main,Admin_CustomPropertyOptions_RichText %>" 
                                ImageUrl="Images/Filetypes/HTML.svg"
                                ClientScriptMethod="page.add_document('html')" 
                                />
                            <tstsc:DropMenuItem 
                                Name="NewFeature" 
                                Runat="server"
                                Value="<%$Resources:Dialogs,Global_Feature_BDD %>" 
                                ImageUrl="Images/Filetypes/Feature.svg"
                                ClientScriptMethod="page.add_document('feature')" 
                                />
                            <tstsc:DropMenuItem Divider="true" Runat="server" />
                            <tstsc:DropMenuItem 
                                Name="NewDiagram" 
                                Runat="server"
                                Value="<%$Resources:Dialogs,Global_Diagram %>" 
                                ImageUrl="Images/Filetypes/Diagram.svg"
                                ClientScriptMethod="page.add_document('diagram')" 
                                />
                            <tstsc:DropMenuItem 
                                Name="NewOrgchart" 
                                Runat="server"
                                Value="<%$Resources:Dialogs,Global_Orgchart %>" 
                                ImageUrl="Images/Filetypes/Orgchart.svg"
                                ClientScriptMethod="page.add_document('orgchart')" 
                                />
                            <tstsc:DropMenuItem 
                                Name="NewMindmap" 
                                Runat="server"
                                Value="<%$Resources:Dialogs,Global_Mindmap %>" 
                                ImageUrl="Images/Filetypes/Mindmap.svg"
                                ClientScriptMethod="page.add_document('mindmap')" 
                                />
                        </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
			        <tstsc:DropMenu 
                        id="btnRefresh" 
                        runat="server" 
                        GlyphIconCssClass="mr3 fas fa-sync" 
                        Text="<%$Resources:Buttons,Refresh %>" 
                        ClientScriptServerControlId="grdDocumentList" 
                        ClientScriptMethod="load_data()" 
                        />
                </div>
                <div class="btn-group priority1" role="group">
                    <tstsc:DropDownListEx 
                        ID="ddlDisplayMode" 
                        runat="server" 
                        ClientScriptMethod="page.display_mode_changed"
                        >
                        <asp:ListItem Value="1" Text="<%$Resources:Dialogs,Global_ItemsInFolder %>" />
                        <asp:ListItem Value="2" Text="<%$Resources:Dialogs,Global_AllItems %>" />
                    </tstsc:DropDownListEx>
				    <tstsc:DropMenu 
                            ID  ="btnFilters" 
                        runat="server" 
                        GlyphIconCssClass="mr3 fas fa-filter" 
                        Text="<%$Resources:Buttons,Filter %>"
					    MenuWidth="140px" 
                        ClientScriptServerControlId="grdDocumentList" 
                        ClientScriptMethod="apply_filters()"
                        >
					    <DropMenuItems>
						    <tstsc:DropMenuItem 
                                Name="Apply" 
                                Value="<%$Resources:Buttons,ApplyFilter %>" 
                                GlyphIconCssClass="mr3 fas fa-filter" 
                                ClientScriptMethod="apply_filters()" 
                                />
						    <tstsc:DropMenuItem 
                                Name="Clear" 
                                Value="<%$Resources:Buttons,ClearFilter %>" 
                                GlyphIconCssClass="mr3 fas fa-times" 
                                ClientScriptMethod="clear_filters()" 
                                />
						    <tstsc:DropMenuItem Name="Retrieve" Value="<%$Resources:Buttons,RetrieveFilter %>"
                                GlyphIconCssClass="mr3 fas fa-search" ClientScriptMethod="retrieve_filter()" />
						    <tstsc:DropMenuItem Name="Save" Value="<%$Resources:Buttons,SaveFilter %>"
                        		GlyphIconCssClass="mr3 fas fa-save" ClientScriptMethod="save_filters()" />
					    </DropMenuItems>
				    </tstsc:DropMenu>                        
                </div>
                <div class="btn-group priority4" role="group">
            	    <tstsc:DropMenu 
                        id="btnTools" 
                        runat="server" 
                        GlyphIconCssClass="mr3 fas fa-cog"
					    Text="<%$Resources:Buttons,Tools %>" 
                        MenuCssClass="DropMenu" 
                        MenuWidth="150px" 
                        ClientScriptServerControlId="grdDocumentList"
                        >
					    <DropMenuItems>
                    	    <tstsc:DropMenuItem 
                                Name="Delete" 
                                Value="<%$Resources:Buttons,Delete %>" 
                                GlyphIconCssClass="mr3 fas fa-trash-alt"
                                ClientScriptMethod="delete_items()"
                                Confirmation="True" 
                                ConfirmationMessage="<%$Resources:Messages,Documents_DeleteConfirm %>"
                                Authorized_ArtifactType="Document" 
                                Authorized_Permission="Delete" 
                                />
                    	    <tstsc:DropMenuItem 
                                Name="Export" 
                                Value="<%$Resources:Dialogs,Global_ExportToProject %>" 
                                GlyphIconCssClass="mr3 fas fa-sign-out-alt"
                                ClientScriptMethod="export_items('Project', Inflectra.SpiraTest.Web.GlobalResources.Documents_ExportDocuments, Inflectra.SpiraTest.Web.GlobalResources.Global_PleaseSelectProjectToExportTo, Inflectra.SpiraTest.Web.GlobalResources.Global_Export)"
                                Authorized_ArtifactType="Document" 
                                Authorized_Permission="View" 
                                />
					    </DropMenuItems>
                    </tstsc:DropMenu>
                </div>
                <div class="btn-group" role="group">
                </div>
                <div class="btn-group" role="group">
				    <tstsc:DropDownListEx 
                        ID="ddlShowHideColumns" 
                        Runat="server" 
                        DataValueField="Key" 
                        DataTextField="Value" 
                        CssClass="DropDownList" 
                        AutoPostBack="false" 
                        NoValueItem="True" 
                        NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" 
                        Width="180px" 
                        ClientScriptServerControlId="grdDocumentList" 
                        ClientScriptMethod="toggle_visibility" 
                        />
                </div>
            </div>
            <div class="bg-near-white-hover py2 px3 br2 transition-all">
	            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Documents_Documents %>" />
                <asp:Label runat="server" ID="txtFolderInfo" CssClass="badge v-mid mx3" />
                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
            </div>
                <tstsc:MessageBox 
                    ID="lblMessage" 
                    runat="server" 
                    SkinID="MessageBox" 
                    />
            <div class="row main-content">
                <tstsc:SortedGrid 
                    ID="grdDocumentList" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    HeaderCssClass="Header"
                    VisibleCountControlId="lblVisibleCount" 
                    TotalCountControlId="lblTotalCount" 
                    FilterInfoControlId="lblFilterInfo"
                    EditRowCssClass="Editing" 
                    FolderItemImage="Folder.svg"
                    SubHeaderCssClass="SubHeader" 
                    SelectedRowCssClass="Highlighted" 
                    ErrorMessageControlId="lblMessage"
                    RowCssClass="Normal" 
                    AutoLoad="true" 
                    DisplayAttachments="false" 
                    AllowEditing="true"
                    Authorized_ArtifactType="Document" 
                    Authorized_Permission="BulkEdit" 
                    AllowColumnPositioning="true"                                
                    runat="server" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
                    >
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-mouse-pointer" 
                            Caption="<%$Resources:Buttons,ViewItem %>" 
                            CommandName="open_item" 
                            CommandArgument="_self" 
                            Authorized_ArtifactType="Document" 
                            Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-external-link-alt" 
                            Caption="<%$Resources:Buttons,ViewItemNewTab %>" 
                            CommandName="open_item" 
                            CommandArgument="_target" 
                            Authorized_ArtifactType="Document" 
                            Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 fas fa-download" 
                            Caption="<%$Resources:Buttons,OpenItem %>" 
                            ClientScriptMethod="grdDocumentList_openItem()" 
                            Authorized_ArtifactType="Document" 
                            Authorized_Permission="View" 
                            />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem 
                            GlyphIconCssClass="mr3 far fa-edit" 
                            Caption="<%$Resources:Buttons,EditItems%>" 
                            CommandName="edit_items" 
                            Authorized_ArtifactType="Document" 
                            Authorized_Permission="BulkEdit" 
                            />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" 
                            Caption="<%$Resources:Buttons,Delete%>" 
                            CommandName="delete_items" 
                            Authorized_ArtifactType="Document" 
                            Authorized_Permission="Delete" 
                            />
                    </ContextMenuItems>    
                </tstsc:SortedGrid>



                <tstsc:DialogBoxPanel
                    ID="dlgUploadDocument" 
                    runat="server" 
                    CssClass="PopupPanel mobile-fullscreen"
                    ErrorMessageControlId="msgUploadMessage" 
                    Modal="true"
                    Width="500px" 
                    Height="520px" 
                            Title="<%$Resources:Dialogs,FileUploadDialog_AddNewDocument %>" 
                        >
                            <div class="PopupPanelBody">
                                <tstsc:MessageBox ID="msgUploadMessage" runat="server" SkinID="MessageBox" />
                                <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="true" ShowSummary="False" DisplayMode="BulletList"
		                            Runat="server" id="ValidationSummary1" />
                                <div id="tblAttachment" style="width:100%">
                                    <div id="tblAttachment_trFilename">
                                        <div>
                                            <div id="file-upload-container" class="file-upload-single">
                                                <div class="file-upload-icon">
                                                    <label for="filAttachment" id="lblFilAttachment">
                                                        <span class="fas fa-cloud-upload-alt pointer"></span> 
		                                            </label>
                                                </div>
                                                <input type="file" id="filAttachment" />
                                                <div>
                                                    <p class="file-upload-inner file-upload-large">
                                                        <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_SelectFiles %>" />
                                                    </p>
                                                    <p class="file-upload-inner mobile-hide">
                                                        <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_DragAndDrop %>" />
                                                    </p>
                                                    <p class="file-upload-filename">
                                                        <span id="file-upload-filename"></span>
                                                    </p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>


                                    <div id="tblAttachment_trUrl" class="hidden-mobile df">
                                        <tstsc:LabelEx CssClass="w7" runat="server" ID="txtURLLabel" AssociatedControlID="txtURL" Required="true" Text="<%$Resources:Fields,URL %>" AppendColon="true"/>
                                        <div class="grow-1">
                                            <tstsc:TextBoxEx ID="txtURL" Runat="server" MaxLength="255" CssClass="text-box" />
	    	                                <asp:RegularExpressionValidator ID="vldWebSite" runat="server" ControlToValidate="txtURL" ErrorMessage="<%$Resources:Messages,FileUploadDialog_URLNotValid %>" Text="*" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_URL%>" />
                                        </div>
                                    </div>


                                    <div id="tblAttachment_trScreenshot" class="hidden-mobile df flex-column-xs">
                                        <tstsc:LabelEx CssClass="w7" runat="server" ID="txtScreenshotFilenameLabel" AssociatedControlID="txtScreenshotFilename" Required="true" Text="<%$Resources:Fields,Screenshot %>" AppendColon="true" />
                                        <div class="grow-1">
                                            <tstsc:TextBoxEx ID="txtScreenshotFilename" runat="server" TextMode="SingleLine" />
                                            <tstsc:ScreenshotCapture ID="ajxScreenshotCapture" runat="server" />
                                        </div>
                                    </div>


                                    <div id="tblAttachment_trMarkdown" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtMarkdownFilenameLabel" 
                                            AssociatedControlID="txtMarkdownFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtMarkdownFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.md
                                            </span>
                                        </div>
                                    </div>

                                    <div id="tblAttachment_trRichText" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtRichTextFilenameLabel" 
                                            AssociatedControlID="txtRichTextFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtRichTextFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.html
                                            </span>
                                        </div>
                                    </div>

                                    <div id="tblAttachment_trFeature" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtFeatureFilenameLabel" 
                                            AssociatedControlID="txtFeatureFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtFeatureFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.feature
                                            </span>
                                        </div>
                                    </div>

                                    <div id="tblAttachment_trDiagram" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtDiagramFilenameLabel" 
                                            AssociatedControlID="txtDiagramFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtDiagramFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.diagram
                                            </span>
                                        </div>
                                    </div>

                                    <div id="tblAttachment_trOrgchart" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtOrgchartFilenameLabel" 
                                            AssociatedControlID="txtOrgchartFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtOrgchartFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.orgchart
                                            </span>
                                        </div>
                                    </div>

                                    <div id="tblAttachment_trMindmap" class="df flex-column-xs">
                                        <tstsc:LabelEx 
                                            CssClass="w7" 
                                            runat="server" 
                                            ID="txtMindmapFilenameLabel" 
                                            AssociatedControlID="txtMindmapFilename" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Filename %>" 
                                            AppendColon="true" 
                                            />
                                        <div class="grow-1 input-group w-max-content">
                                            <tstsc:TextBoxEx ID="txtMindmapFilename" runat="server" TextMode="SingleLine" MaxLength="50" SkinID="FormControl" />
                                            <span class="input-group-addon">.mindmap
                                            </span>
                                        </div>
                                    </div>

                                    <div class="mt3 df flex-column-xs">
                                        <tstsc:LabelEx CssClass="w7" runat="server" ID="txtAttachmentDescriptionLabel" AssociatedControlID="txtAttachmentDescription" Required="false" Text="<%$Resources:Fields,Description %>" AppendColon="true" />
                                        <div class="grow-1">
                                            <tstsc:TextBoxEx ID="txtAttachmentDescription" runat="server" Height="50px" DynamicHeight="False" RichText="false"
                                                TextMode="MultiLine"/>
                                        </div>
                                    </div>
                                    <div class="mt3 df flex-column-xs">
                                        <tstsc:LabelEx  CssClass="w7" runat="server" ID="lblDocType" AssociatedControlID="ddlDocType" Required="true" Text="<%$Resources:Fields,DocumentType %>" AppendColon="true" />
	                                    <tstsc:DropDownListEx id="ddlDocType" Runat="server" DataValueField="DocumentTypeId" DataTextField="Name" DataMember="DocumentType" NoValueItem="false" />
                                    </div>
                                    <div class="mt3 df flex-column-xs">
                                        <tstsc:LabelEx  CssClass="w7" runat="server" ID="txtVersionNumberLabel" AssociatedControlID="txtVersionNumber" Required="false" Text="<%$Resources:Fields,Version %>" AppendColon="true" />
	                                    <div class="grow-1">
                                            <tstsc:TextBoxEx id="txtVersionNumber" Runat="server" MaxLength="5" Text="1.0"/>
	    	                                <asp:RegularExpressionValidator ID="vldVersionNumber" runat="server" ControlToValidate="txtVersionNumber" ErrorMessage="<%$Resources:Messages,DocumentDetails_VersionNumberInValid %>" Text="*" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_VERSION_NUMBER%>" />    
                                        </div>
                                    </div>
                                    <div class="mt3 df flex-column-xs">
		                                <tstsc:LabelEx  CssClass="w7" runat="server" ID="txtTagsLabel" Required="false" AssociatedControlID="txtTags" Text="<%$Resources:Fields,Tags %>" AppendColon="true" />
		                                <div class="grow-1">
                                            <tstsc:TextBoxEx id="txtTags" Runat="server" MaxLength="255" />
	    	                                <asp:RegularExpressionValidator ID="vldTags" runat="server" ControlToValidate="txtTags" ErrorMessage="<%$Resources:Messages,FileUploadDialog_TagsNotValid %>" Text="*" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_TAGS%>" />    
                                        </div>
                                    </div>
                                    <div class="mt4 mb3 btn-group ml7">
                                        <tstsc:HyperLinkEx ID="btnUpload" SkinID="ButtonPrimary" runat="server" CausesValidation="true" Text="<%$Resources:Buttons,Add %>" ClientScriptMethod="btnUpload_click()" />
                                        <tstsc:HyperLinkEx ID="btnCancel" SkinID="ButtonDefault" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgUploadDocument" ClientScriptMethod="close()" />
                                    </div>
                                </div>
                            </div>
                        </tstsc:DialogBoxPanel>
                <br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/DocumentsService.svc" />
        </Services>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        /* The Page Class */
        Type.registerNamespace('Inflectra.SpiraTest.Web');
        Inflectra.SpiraTest.Web.Documents = function ()
        {
            /* Properties */
            this._projectId = <%=ProjectId%>;
            this._userId = <%=UserId%>;
            this._fileExtension = 'png';
            this._mode = 'file';
            this._trFile = null;
            this._trUrl = null;
            this._trScreenshot = null;
            this._trMarkdown = null;
            this._trRichText = null;
            this._btnUpload = null;
            this._grdDocumentList_id = '<%=grdDocumentList.ClientID%>';
            this._dlgUploadDocument_id = '<%=dlgUploadDocument.ClientID%>';
            this._droppedFile = null;
        }
        Inflectra.SpiraTest.Web.Documents.prototype =
        {
            /* Constructors */
            initialize: function ()
            {
                this._trFile = $get('tblAttachment_trFilename');
                this._trUrl = $get('tblAttachment_trUrl');
                this._trScreenshot = $get('tblAttachment_trScreenshot');
                this._trMarkdown = $get('tblAttachment_trMarkdown');
                this._trRichText = $get('tblAttachment_trRichText');
				this._trFeature = $get('tblAttachment_trFeature');
				this._trDiagram = $get('tblAttachment_trDiagram');
				this._trOrgchart = $get('tblAttachment_trOrgchart');
				this._trMindmap = $get('tblAttachment_trMindmap');
                this._btnUpload = $get('<%=btnUpload.ClientID%>');

                //Set the default mode to file
                this.show_attachment_type_file();
            },
            dispose: function ()
            {
                delete this._trFile;
                delete this._trUrl;
                delete this._trScreenshot;
                delete this._trMarkdown;
                delete this._trRichText;
				delete this._trFeature;
				delete this._trDiagram;
				delete this._trOrgchart;
				delete this._trMindmap;
                delete this._btnUpload;
            },

            /* Properties */
            set_droppedFile: function(value)
            {
                this._droppedFile = value;
            },

            get_mode: function()
            {
                return this._mode;
            },
            set_mode: function(mode)
            {
                this._mode = mode;
            },

            /* Public Methods */

            //Handles clicks on the add document link
            add_document: function(mode)
            {
                //Store mode
                this._mode = mode;
                this.updateMode();

                //get a handle to the dialog box and screenshot catcher
                var dialog = $find('<%=this.dlgUploadDocument.ClientID%>');
                var ajxScreenshotCapture = $find('<%=ajxScreenshotCapture.ClientID%>');

                //Clear certain fields
                $('#<%=txtAttachmentDescription.ClientID%>').val('');
                $('#<%=txtScreenshotFilename.ClientID%>').val('');
                $('#<%=txtTags.ClientID%>').val('');
                $('#<%=txtURL.ClientID%>').val('');
                $('#file-upload-filename').text('');
                $('.file-upload-inner').show();
                //Hide any drag-and-drop mentions on mobile
                if (SpiraContext.IsMobile)
                {
                    $('.file-upload-inner.mobile-hide').hide();
                }
                this.set_droppedFile(null);
                $get('filAttachment').value = null;
                ajxScreenshotCapture.clearPreview();

                //Position the dialog box and display
                dialog.set_left(450);
                dialog.set_top(130);
                dialog.display();
            },
            updateMode: function()
            {
                switch (this._mode)
                {
                    case 'file':
                        this.show_attachment_type_file();
                        break;

                    case 'url':
                        this.show_attachment_type_url();
                        break;

                    case 'image':
                        this.show_attachment_type_screenshot();
                        break;

                    case 'markdown':
                        this.show_attachment_type_markdown();
                        break;

                    case 'html':
                        this.show_attachment_type_richtext();
                        break;

                    case 'feature':
                        this.show_attachment_type_feature();
                        break;

					case 'diagram':
						this.show_attachment_type_diagram();
                        break;

					case 'orgchart':
						this.show_attachment_type_orgchart();
                        break;

					case 'mindmap':
						this.show_attachment_type_mindmap();
						break;
                }
            },

            display_mode_changed: function(item)
            {
                //See which display mode we have
                var displayMode = item.get_value();

                //serialize as a filter and set on the grid
                var grdDocumentList = $find('<%=grdDocumentList.ClientID %>');
                var filters = {};
                filters.DisplayMode = globalFunctions.serializeValueInt(displayMode);
                grdDocumentList.set_standardFilters(filters);
                grdDocumentList.load_data();
            },

            tag_click: function (tagName)
            {
                //We need to set the tag filter, change the dropdown to 'all items' which will then load the data
                var grdDocumentList = $find('<%=grdDocumentList.ClientID %>');
                grdDocumentList.custom_operation('FilterOnTags', tagName);

                var ddlDisplayMode = $find('<%=ddlDisplayMode.ClientID %>');
                ddlDisplayMode.set_selectedItem(2);
            },

            show_attachment_type_file: function ()
            {
                //Show the file attachment row and hide the others
                this._trFile.classList.remove("dn");
                this._trUrl.classList.add("dn");
                this._trScreenshot.classList.add("dn");
                this._trMarkdown.classList.add("dn");
                this._trRichText.classList.add("dn");
				this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
            show_attachment_type_url: function ()
            {
                //Show the url attachment row and hide the others
                this._trUrl.classList.remove("dn");
                this._trFile.classList.add("dn");
                this._trScreenshot.classList.add("dn");
                this._trMarkdown.classList.add("dn");
                this._trRichText.classList.add("dn");
                this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
            show_attachment_type_screenshot: function ()
            {
                //Show the url attachment row and hide the others
                this._trScreenshot.classList.remove("dn");
                this._trFile.classList.add("dn");
                this._trUrl.classList.add("dn");
                this._trMarkdown.classList.add("dn");
                this._trRichText.classList.add("dn");
                this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");

                //Focus on the screenshot
                var ajxScreenshotCapture = $find('<%=ajxScreenshotCapture.ClientID %>');
                if (ajxScreenshotCapture)
                {
                    ajxScreenshotCapture.focusOnPasteCatcher();
                }
            },
            show_attachment_type_markdown: function ()
            {
                //Show the markdown attachment row and hide the others
                this._trUrl.classList.add("dn");
                this._trFile.classList.add("dn");
                this._trScreenshot.classList.add("dn");
                this._trMarkdown.classList.remove("dn");
                this._trRichText.classList.add("dn");
                this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
            show_attachment_type_richtext: function ()
            {
                //Show the rich text attachment row and hide the others
                this._trUrl.classList.add("dn");
                this._trFile.classList.add("dn");
                this._trScreenshot.classList.add("dn");
                this._trMarkdown.classList.add("dn");
                this._trRichText.classList.remove("dn");
                this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
            show_attachment_type_feature: function () {
                //Show the rich text attachment row and hide the others
                this._trUrl.classList.add("dn");
                this._trFile.classList.add("dn");
                this._trScreenshot.classList.add("dn");
                this._trMarkdown.classList.add("dn");
                this._trRichText.classList.add("dn");
                this._trFeature.classList.remove("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
			show_attachment_type_diagram: function () {
				//Show the rich text attachment row and hide the others
				this._trUrl.classList.add("dn");
				this._trFile.classList.add("dn");
				this._trScreenshot.classList.add("dn");
				this._trMarkdown.classList.add("dn");
				this._trRichText.classList.add("dn");
				this._trFeature.classList.add("dn");
				this._trDiagram.classList.remove("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.add("dn");
            },
			show_attachment_type_orgchart: function () {
				//Show the rich text attachment row and hide the others
				this._trUrl.classList.add("dn");
				this._trFile.classList.add("dn");
				this._trScreenshot.classList.add("dn");
				this._trMarkdown.classList.add("dn");
				this._trRichText.classList.add("dn");
				this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.remove("dn");
				this._trMindmap.classList.add("dn");
            },
			show_attachment_type_mindmap: function () {
				//Show the rich text attachment row and hide the others
				this._trUrl.classList.add("dn");
				this._trFile.classList.add("dn");
				this._trScreenshot.classList.add("dn");
				this._trMarkdown.classList.add("dn");
				this._trRichText.classList.add("dn");
				this._trFeature.classList.add("dn");
				this._trDiagram.classList.add("dn");
				this._trOrgchart.classList.add("dn");
				this._trMindmap.classList.remove("dn");
			},

            pasteScreenshot: function (data)
            {
                //Clear any prior errors
                globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));

                //Store the data and get the filetype
                //data:image/png;base64,XXXXXXXXXX
                var reDataUrl = /^data:image\/(png|gif|jpg|jpeg|tiff|bmp);base64,(.*)/;
                if (reDataUrl.test(data))
                {
                    var m = reDataUrl.exec(data);
                    this._fileExtension = m[1];
                    this._encodedData = data;

                    //Provide a default filename
                    var txtScreenshotFilename = $get('<%=txtScreenshotFilename.ClientID%>');
                    txtScreenshotFilename.value = resx.FileArtifactUploadDialog_DefaultScreenshotFilename + '.' + this._fileExtension;
                }
                else
                {
                    globalFunctions.globalAlert(resx.ScreenshotCapture_InvalidImageFormat, 'danger', true);
                }
            },

            upload: function ()
            {
                //Clear any prior errors
                globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));

                //First fire the asp.net client-side validators
                var isPageValid = Page_ClientValidate();
                if (!isPageValid)
                {
                    //Validation failed, so return false
                    return false;
                }

                //See what type of attachment we have
                var filename = null;
                var url = null;
                var file = null;
                if (this._mode == 'image')
                {
                    //Get the data from the form and do some additional validation
                    var txtScreenshotFilename = $get('<%=txtScreenshotFilename.ClientID%>');
                    filename = txtScreenshotFilename.value;
                    if (globalFunctions.trim(filename) == '')
                    {
                        globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_ScreenshotFilenameRequired);
                        return;
                    }
                    if (!filename.toLowerCase().match('\.' + this._fileExtension + '$'))
                    {
                        globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_ScreenshotFilenameWrongExtension);
                        return;
                    }
                    //Get the image from the stored data
                    if (!this._encodedData)
                    {
                        globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_SupaPasteImageFirst);
                        return;
                    }
                }
                else if (this._mode == 'file')
                {
                    //See if we have a dropped file
                    if (this._droppedFile)
                    {
                        file = this._droppedFile;
                    }
                    else
                    {
                        //Get a handle to the upload box
                        var files = $get('filAttachment').files;
                        if (files && files.length > 0)
                        {
                            file = files[0];
                        }
                    }

                    //Make sure we have file data present
                    if (!file)
                    {
                        globalFunctions.globalAlert(resx.FileArtifactUploadDialog_FilenameRequired, 'danger', true);
                        return;
                    }
                    if (file.size <= 0)
                    {
                        globalFunctions.globalAlert(resx.FileUpload_AttachmentEmpty, 'danger', true);
                        return;
                    }
                    if (file.size > SpiraContext.MaxAllowedContentLength)
                    {
                        globalFunctions.globalAlert(resx.FileUpload_AttachmentTooLarge.replace('{0}', (SpiraContext.MaxAllowedContentLength/1024)), 'danger', true);
                        return;
                    }
                }
                else if (this._mode == 'url')
                {
                    //Get the URL from the form
                    var txtURL = $get('<%=txtURL.ClientID%>');
                    url = txtURL.value;
                }
                else if (this._mode == 'markdown')
                {
                    //Get the Filename from the form
                    var txtMarkdownFilename = $get('<%=txtMarkdownFilename.ClientID%>');
                    filename = txtMarkdownFilename.value;
                    //Make sure we have a filename specified
                    if (!filename || filename == '')
                    {
                        globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
                        return;
                    }
                    filename += '.md';
                }
                else if (this._mode == 'html')
                {
                    //Get the Filename from the form
                    var txtRichTextFilename = $get('<%=txtRichTextFilename.ClientID%>');
                    filename = txtRichTextFilename.value;
                    //Make sure we have a filename specified
                    if (!filename || filename == '')
                    {
                        globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
                        return;
                    }
                    filename += '.html';
                }
                else if (this._mode == 'feature') {
                    //Get the Filename from the form
                    var txtFeatureFilename = $get('<%=txtFeatureFilename.ClientID%>');
                    filename = txtFeatureFilename.value;
                    //Make sure we have a filename specified
                    if (!filename || filename == '') {
                        globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
                        return;
                    }
                    filename += '.feature';
                }
				else if (this._mode == 'diagram') {
					//Get the Filename from the form
					var txtDiagramFilename = $get('<%=txtDiagramFilename.ClientID%>');
					filename = txtDiagramFilename.value;
					//Make sure we have a filename specified
					if (!filename || filename == '') {
						globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
						return;
					}
					filename += '.diagram';
                }
				else if (this._mode == 'orgchart') {
					//Get the Filename from the form
					var txtOrgchartFilename = $get('<%=txtOrgchartFilename.ClientID%>');
					filename = txtOrgchartFilename.value;
					//Make sure we have a filename specified
					if (!filename || filename == '') {
						globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
						return;
					}
					filename += '.orgchart';
                }
				else if (this._mode == 'mindmap') {
					//Get the Filename from the form
					var txtMindmapFilename = $get('<%=txtMindmapFilename.ClientID%>');
					filename = txtMindmapFilename.value;
					//Make sure we have a filename specified
					if (!filename || filename == '') {
						globalFunctions.globalAlert(resx.Documents_FilenameRequired, "danger", true);
						return;
					}
					filename += '.mindmap';
				}

                var description = $get('<%=txtAttachmentDescription.ClientID%>').value;
                var version = $get('<%=txtVersionNumber.ClientID %>').value;
                var tags = $get('<%=txtTags.ClientID%>').value;
                var typeId = null;
                var ddlDocType = $find('<%=ddlDocType.ClientID%>');
                // if we have a folderId save it as an integer to pass back when uploading
                var folderId = $find(this._grdDocumentList_id) && $find(this._grdDocumentList_id)._standardFilters && $find(this._grdDocumentList_id)._standardFilters[globalFunctions.keyPrefix + "_FolderId"];
                folderId = typeof folderId != "undefined" && folderId !== null ? globalFunctions.deserializeValueInt(folderId) : null;
                
                if (ddlDocType && ddlDocType.get_selectedItem && ddlDocType.get_selectedItem().get_value() != '')
                {
                    typeId = parseInt(ddlDocType.get_selectedItem().get_value());
                } 

                //Call the webservice and upload the file or url
                globalFunctions.display_info_message($get('<%=msgUploadMessage.ClientID%>'), resx.Global_Uploading);
                if (this._mode == 'url')
                {
                    this.uploadUrl(url, description, version, tags, typeId, folderId);
                }
				if (this._mode == 'markdown' || this._mode == 'html' || this._mode == 'feature' || this._mode == 'diagram' || this._mode == 'orgchart' || this._mode == 'mindmap')
                {
                    this.createTextFile(filename, description, version, tags, typeId, folderId, this._mode);
                }
                if (this._mode == 'image')
                {
                    this.uploadScreenshot(filename, description, version, tags, typeId, folderId);
                }
                if (this._mode == 'file' && file)
                {
                    var reader = new FileReader();
                    reader.readAsDataURL(file);
                    reader.onload = Function.createCallback(this.uploadFile, {
                        thisRef: this,
                        filename: file.name,
                        description: description,
                        version: version,
                        tags: tags,
                        typeId: typeId,
                        folderId: folderId
                    });
                }
            },
            uploadScreenshot: function(filename, description, version, tags, typeId, folderId)
            {
                var context = {};
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadFile(
                    this._projectId,
                    filename,
                    description,
                    this._userId,
                    this._encodedData,
                    null,
                    null,
                    version,
                    tags,
                    typeId,
                    folderId,
                    Function.createDelegate(this, this.upload_success),
                    Function.createDelegate(this, this.upload_failure), context
                );
            },
            uploadFile: function(evt, args)
            {
                var result = evt.target.result;
                var context = {};
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadFile(
                    args.thisRef._projectId,
                    args.filename,
                    args.description,
                    args.thisRef._userId,
                    result,
                    null,
                    null,
                    args.version,
                    args.tags,
                    args.typeId,
                    args.folderId,
                    Function.createDelegate(args.thisRef, args.thisRef.upload_success),
                    Function.createDelegate(args.thisRef, args.thisRef.upload_failure),
                    context
                );
            },
            uploadUrl: function(url, description, version, tags, typeId, folderId)
            {
                var context = {};
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadUrl(
                    this._projectId,
                    url,
                    description,
                    this._userId,
                    null,
                    null,
                    version,
                    tags,
                    typeId,
                    folderId,
                    Function.createDelegate(this, this.upload_success),
                    Function.createDelegate(this, this.upload_failure),
                    context
                );
            },
            upload_success: function (attachmentId, context)
            {
                globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));
                    
                //Now we need to close the dialog box and refresh the datagrid
                $find(this._dlgUploadDocument_id).close();
                $find(this._grdDocumentList_id).load_data();
            },
            upload_failure: function (exception, context)
            {
                globalFunctions.display_error($get('<%=msgUploadMessage.ClientID%>'), exception);
            },
            createTextFile: function(filename, description, version, tags, typeId, folderId, format)
            {
                var context = {};
                Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_CreateTextFile(
                    this._projectId,
                    filename,
                    description,
                    this._userId,
                    version,
                    tags,
                    typeId,
                    folderId,
                    format,
                    Function.createDelegate(this, this.createTextFile_success),
                    Function.createDelegate(this, this.upload_failure),
                    context
                );
            },
            createTextFile_success: function (attachmentId, context)
            {
                globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));
                    
                //Redirect to the appropriate document details page - and open the edit tab automatically
                var url = urlTemplate_documentDetails.replace(globalFunctions.artifactIdToken, attachmentId + "/Edit");
                window.location = url;
            }
        }
        var page = $create(Inflectra.SpiraTest.Web.Documents);
        page.initialize();

        function trvFolders_dragEnd()
        {
            //Simply reload the grid
            var ajaxControl = $find('<%=grdDocumentList.ClientID %>');
            ajaxControl.load_data();
        }
        function trvFolders_folderChanged(folderId)
        {
            //Set the standard filter and reload
            var grdDocumentList = $find('<%=grdDocumentList.ClientID %>');
            var args = {};
            args['_FolderId'] = globalFunctions.serializeValueInt(folderId);
            grdDocumentList.set_standardFilters(args);
            grdDocumentList.load_data();
        }
        function grdDocumentList_focusOn(nodeId)
        {
            //It means the folder may have changed, so reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.set_selectedNode(nodeId);
            trvFolders.load_data(true);
        }
        function grdDocumentList_openItem()
        {
            var grdDocumentList = $find('<%=grdDocumentList.ClientID%>');
            var items = grdDocumentList.get_selected_items();
            if (items.length == 1)
            {
                var url = '<%=AttachmentOpenUrl%>';
                url = url.replace(globalFunctions.artifactIdToken, items[0]);
                window.open(url);
            }
            else
            {
                globalFunctions.globalAlert(resx.Global_SelectOneCheckBoxForOpen, 'info');
            }
        }

        //Global handler for screenshot pasting
        function ajxScreenshotCapture_imagePaste(data)
        {
            page.pasteScreenshot(data);
        }

        //Handlers
        function btnUpload_click()
        {
            page.upload();
        }

        //Set some initialstate
        $(document).ready(function() {
            //Hide any drag-and-drop mentions on mobile
            if (SpiraContext.IsMobile)
            {
                $('.mobile-hide').hide();
            }

            // because we allow users to click on the surrounding container (file-upload-container) we need to stop the events from clickin the label and the input itself from bubbling up to that container
            // if we do not then clicking the label registers as a click on the container which as below then in code clicks the input again which causes errors on the page
			$('#filAttachment').on("click", function (event) {
                event.stopPropagation();
            });
			$('#lblFilAttachment').on("click", function (event) {
                event.stopPropagation();
            });

            $('#file-upload-container')
				.on("click", function() {
                    $('#filAttachment')[0].click();
                })
                .on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
                    // preventing the unwanted behaviours
                    e.preventDefault();
                    e.stopPropagation();
                })
				.on('dragover dragenter', function ()
				{
				    $('#file-upload-container').addClass('file-upload-is-dragover');
				})
				.on('dragleave dragend drop', function () {
				    $('#file-upload-container').removeClass('file-upload-is-dragover');
				})
				.on('drop', function (e) {
				    droppedFiles = e.originalEvent.dataTransfer.files; // the files that were dropped
				    if (droppedFiles && droppedFiles.length > 0)
				    {
				        $('#file-upload-filename').text(droppedFiles[0].name);
				        $('.file-upload-inner').hide();
				        page.set_droppedFile(droppedFiles[0]);
				    }
				});

			$('#filAttachment').on("change", function(){
                var files = $('#filAttachment')[0].files;
                if (files && files.length > 0)
                {
                    $('#file-upload-filename').text(files[0].name);
                    $('.file-upload-inner').hide();
                    page.set_droppedFile(null);
                }
            });
        });

        //URL Templates
        var urlTemplate_documentDetails = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Documents, ProjectId, -2))%>';
    </script>
</asp:Content>
