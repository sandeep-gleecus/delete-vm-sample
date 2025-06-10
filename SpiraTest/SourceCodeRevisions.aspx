<%@ Page Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="SourceCodeRevisions.aspx.cs" Inherits="Inflectra.SpiraTest.Web.SourceCodeRevisions" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
	<table class="panel-container">
        <tbody>
            <tr>
                <td class="main-panel">
                    <asp:PlaceHolder runat="server" ID="plcToolbar">
                        <div class="btn-toolbar toolbar" role="toolbar">
                            <div class="btn-group priority2" role="group">
						        <tstsc:DropMenu id="DropMenu1" runat="server" GlyphIconCssClass="mr3 fas fa-sync" CssClass="fas fa-sync" Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="load_data()" />
                            </div>
                            <div class="btn-group priority2"> 
                                <div class="label-addon">
    						        <tstsc:LabelEx ID="LabelEx1" runat="server" Text="<%$Resources:Main,SourceCodeList_CurrentBranch %>" AssociatedControlID="mnuBranches" AppendColon="true" />
                                </div>                     
						        <tstsc:DropMenu id="mnuBranches" runat="server" GlyphIconCssClass="mr3 fas fa-code-branch" ButtonTextSelectsItem="true" />
					        </div>
                            <div class="btn-group priority1">
						        <tstsc:DropMenu id="DropMenu2" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
							        MenuWidth="140px" ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="apply_filters()">
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
                    </asp:PlaceHolder>
                    <div class="main-content">
                        <asp:PlaceHolder runat="server" ID="plcLegend">
                            <div class="alert alert-warning alert-narrow">
                                <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
	                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                                <%=" "%>
                                <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                                <%=" "%>
                                <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SourceCodeRevisions_Revisions %>" />.
                                <%=" "%>
                                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                            </div>
                        </asp:PlaceHolder>
                        <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
                        <tstsc:SortedGrid ID="grdSourceCodeRevisionList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" ItemImage="artifact-Revision.svg"
				            SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="lblMessage"
				            RowCssClass="Normal" AutoLoad="true" DisplayAttachments="false" AllowEditing="false"
                            VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
				            runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" />
			            <br />
                        <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />

                        <%-- custom css to affect which columns are visible in the grid --%>
                        <style>
                            @media (max-width: 1024px) {
                                /*show the commit message*/
                                table.DataGrid tr>:nth-child(4) {
                                    display: table-cell;
                                }
                            }
                            @media (max-width: 544px) {
                                /*hide the commit icon*/
                                table.DataGrid tr>:nth-child(2) img {
                                    display: none;
                                }
                            }
                        </style>
                    </div>
    			</td>
	    	</tr>
	    </tbody>
    </table>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />  
       </Services>  
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">

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
            var grdSourceCodeRevisionList = $find('<%=this.grdSourceCodeRevisionList.ClientID%>');
            var standardfilters = {};
            standardfilters[globalFunctions.keyPrefix + 'BranchKey'] = globalFunctions.serializeValueString(branchPath);
            grdSourceCodeRevisionList.set_standardFilters(standardfilters);

            //Reload the grid
            grdSourceCodeRevisionList.load_data();
        }
        function mnuBranches_click_failure(ex) {
            var lblMessage = $get('<%=this.lblMessage.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
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
