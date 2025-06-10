<%@ Page Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="SourceCodeList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.SourceCodeList" Title="Untitled Page" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex flex-column-reverse-sm">
        <div class="side-panel hidden-sm-second-child sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlFolders" runat="server" HeaderCaption="<%$Resources:Main,Global_Folders %>" MinWidth="100" MaxWidth="500" data-panel="folder-tree"
                DisplayRefresh="true" ClientScriptServerControlId="trvFolders" ClientScriptMethod="load_data(true)" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <div class="Widget panel">
					<tstsc:TreeView ID="trvFolders" runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService"
						LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="lblMessage" ClientScriptMethod="load_data();"
						ClientScriptServerControlId="grdSourceCodeFileList" NodeLegendControlId="spanFolderName" />
                </div>
            </tstsc:SidebarPanel>
        </div>


        <div class="main-panel pl4 grow-1">
            <asp:PlaceHolder runat="server" ID="plcToolbar">
                <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm bb b-light-gray" role="toolbar">
                    <div class="btn-group priority2" role="group">
					    <tstsc:DropMenu id="btnRefresh" GlyphIconCssClass="mr3 fas fa-sync" runat="server" CssClass="fas fa-sync" Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdSourceCodeFileList" ClientScriptMethod="load_data()" />
                    </div>
                    <div class="btn-group priority1"> 
                        <div class="label-addon">
    					    <tstsc:LabelEx runat="server" Text="<%$Resources:Main,SourceCodeList_CurrentBranch %>" AssociatedControlID="mnuBranches" AppendColon="true" />
                        </div>                     
					    <tstsc:DropMenu 
                            id="mnuBranches" 
                            runat="server" 
                            GlyphIconCssClass="mr3 fas fa-code-branch" 
                            ClientScriptMethod="void(0)" 
                            ButtonTextSelectsItem="true"
                            />
				    </div>
                    <div class="btn-group priority1">
                        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
						    MenuWidth="140px" ClientScriptServerControlId="grdSourceCodeFileList" ClientScriptMethod="apply_filters()">
						    <DropMenuItems>
							    <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
							    <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
						    </DropMenuItems>
					    </tstsc:DropMenu>  
				    </div>
                    <asp:PlaceHolder runat="server" ID="plcTaraVault" Visible="false">
                        <div class="btn-group priority1" id="taravault-clone">
                            <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <asp:Localize runat="server" Text="<%$Resources:Main,SourceCodeList_CloneOrCheckout %>" />
                                <span class="caret"></span>
                            </button>
                            <div class="dropdown-menu pa3 w9">
                                <h3 class="mt2"><asp:Literal runat="server" ID="ltrCloneCheckoutTitle" />
                                </h3>
                                <div class="form-group">
                                    <asp:Literal runat="server" ID="ltrCloneCheckoutIntro" />
                                </div>
                                <div class="form-group">
                                    <tstsc:TextBoxEx runat="server" ID="txtCloneOrCheckoutUrl" SkinID="FormControl" Width="340px" />
                                </div>
                                <div class="form-group">
                                    <div class="input-group">
                                        <span class="input-group-addon">
                                            <tstsc:LabelEx runat="server" AssociatedControlID="txtTaraVaultLogin" Text="<%$Resources:Fields,Login %>" />
                                        </span>
                                        <tstsc:TextBoxEx runat="server" ID="txtTaraVaultLogin" SkinID="FormControl" Width="240px" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="input-group">
                                        <span class="input-group-addon">
                                            <tstsc:LabelEx runat="server" AssociatedControlID="txtTaraVaultPassword" Text="<%$Resources:Fields,Password %>" />
                                        </span>
                                        <tstsc:TextBoxEx runat="server" ID="txtTaraVaultPassword" SkinID="FormControlObscured" Width="240px" />
                                    </div>
                                </div>
                                <br style="clear:both" />
                                <div class="form-group">
                                    <button type="button" class="btn btn-primary pull-right">
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Close %>" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder runat="server" ID="plcOnPremise" Visible="false">
                        <div class="btn-group priority1" id="on-premise-clone">
                            <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                <asp:Localize runat="server" Text="<%$Resources:Main,SourceCodeList_CloneOrCheckout %>" />
                                <span class="caret"></span>
                            </button>
                            <div class="dropdown-menu pa3 w9">
                                <h3 class="mt2"><asp:Literal runat="server" ID="ltrOnPremiseConnectTitle" />
                                </h3>
                                <div class="form-group">
                                    <asp:Literal runat="server" ID="ltrOnPremiseConnectIntro" />
                                </div>
                                <div class="form-group">
                                    <tstsc:TextBoxEx runat="server" ID="txtOnPremiseConnection" SkinID="FormControl" Width="340px" />
                                </div>
                                <br style="clear:both" />
                                <div class="form-group">
                                    <button type="button" class="btn btn-primary pull-right">
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Close %>" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                    <div class="br2 bg-off-white px4 py2 dib ml3">
					    <tstsc:LabelEx ID="lblRepositoryName" runat="server" />
                    </div>                            
                </div>
                <div class="bg-near-white-hover py2 px3 br2 transition-all">
				    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                    <%=" "%>
				    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
				    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                    <%=" "%>
				    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
				    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SourceCodeList_Files %>" />
                    <%=" "%>
				    <span id="spanFolderName" runat="server" style="font-weight: bold">(<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,SourceCodeList_NoFolderSelected %>" />)</span>.
				    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                </div>
            </asp:PlaceHolder>
            <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
			<tstsc:MessageBox id="lblServerMessage" Runat="server" SkinID="MessageBox" />
			<div class="row main-content">
                <tstsc:SortedGrid 
                    AllowEditing="false" 
                    AutoLoad="true" 
                    CssClass="DataGrid DataGrid-no-bands" 
                    DisplayAttachments="false" 
                    ErrorMessageControlId="lblMessage"
                    FilterInfoControlId="lblFilterInfo"
                    HeaderCssClass="Header"
                    ID="grdSourceCodeFileList" 
					RowCssClass="Normal" 
					runat="server" 
                    SelectedRowCssClass="Highlighted" 
					SubHeaderCssClass="SubHeader" 
                    TotalCountControlId="lblTotalCount" 
					VisibleCountControlId="lblVisibleCount" 
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService" 
                    />
				<br />
				<asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
	  <Services>  
		<asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />  
		<asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />  
	   </Services>  
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
    <script type="text/javascript">
        $(document).ready(function ()
        {
            //Auto select all when clicked
			$('#taravault-clone input[type=text]').on("click", function ()
            {
                $(this).select();
            });
			$('#on-premise-clone input[type=text]').on("click", function ()
            {
                $(this).select();
            });
        });

        function mnuBranches_click(branchPath)
        {
            //Set the new branch
            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCode_SetSelectedBranch(SpiraContext.ProjectId, branchPath, mnuBranches_click_success, mnuBranches_click_failure, branchPath);
        }
        function mnuBranches_click_success(data, branchPath)
        {
            //Change the branch display
            var mnuBranches = $find('<%=this.mnuBranches.ClientID%>');
            mnuBranches.set_text(branchPath);
            mnuBranches.update_menu();
            mnuBranches.refreshItems(true);
                
            //Reset the standard filter
            var grdSourceCodeFileList = $find('<%=this.grdSourceCodeFileList.ClientID%>');
            var standardfilters = {};
            standardfilters[globalFunctions.keyPrefix + 'BranchKey'] = globalFunctions.serializeValueString(branchPath);
            grdSourceCodeFileList.set_standardFilters(standardfilters);

            //Reload the grid
            grdSourceCodeFileList.load_data();

            //Reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.load_data(true);

            //Clear the folder selector legend
            $get('<%=spanFolderName.ClientID%>').innerHTML = '';
        }
        function mnuBranches_click_failure(ex) {
            var lblMessage = $get('<%=this.lblMessage.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
        }

        function grdSourceCodeFileList_focusOn(nodeId)
        {
            //It means the folder may have changed, so reload the treeview
            var trvFolders = $find('<%=trvFolders.ClientID%>');
            trvFolders.set_selectedNode(nodeId);
            trvFolders.load_data(true);
        }
    </script>
</asp:Content>
