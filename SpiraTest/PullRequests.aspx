<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="PullRequests.aspx.cs" Inherits="Inflectra.SpiraTest.Web.PullRequests" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="main-panel pl4 grow-1">
            <asp:PlaceHolder runat="server" ID="plcToolbar">
                <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                    <div class="btn-group priority3" role="group">
                        <tstsc:DropMenu ID="btnNewPullRequest" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
                            Text="<%$Resources:Main,PullRequests_NewPullRequest %>" Authorized_ArtifactType="Task" Authorized_Permission="Create" ClientScriptMethod="btnNewPullRequest_click()" />
                        <tstsc:DropMenu ID="btnDelete" runat="server" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptServerControlId="grdPullRequests" ClientScriptMethod="delete_items()"
                            Text="<%$Resources:Buttons,Delete %>" Authorized_ArtifactType="Task" Authorized_Permission="Delete" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,PullRequest_DeleteConfirm %>" />
                    </div>
                    <div class="btn-group priority2" role="group">
                        <tstsc:DropMenu ID="btnRefresh" runat="server" GlyphIconCssClass="mr3 fas fa-sync"
                            Text="<%$Resources:Buttons,Refresh %>" ClientScriptServerControlId="grdPullRequests" ClientScriptMethod="load_data()" />
                    </div>
                    <div class="btn-group priority1" role="group">
                        <tstsc:DropMenu ID="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter"
                            Text="<%$Resources:Buttons,Filter %>" MenuWidth="125px" ClientScriptServerControlId="grdPullRequests" ClientScriptMethod="apply_filters()">
                            <DropMenuItems>
                                <tstsc:DropMenuItem Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
                                <tstsc:DropMenuItem Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                </div>
            </asp:PlaceHolder>

            <div class="main-content">
                <asp:PlaceHolder runat="server" ID="plcLegend">
                    <div class="bg-near-white-hover py2 px3 br2 transition-all">
                        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                        <%=" "%>
                        <asp:Label ID="lblVisibleSetCount" runat="server" Font-Bold="True" />
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                        <%=" "%>
                        <asp:Label ID="lblTotalSetCount" runat="server" Font-Bold="True" />
                        <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,PullRequests_PullRequestsForProject %>" />.
                        <%=" "%>
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                    </div>
                </asp:PlaceHolder>
                <tstsc:MessageBox ID="divMessage" runat="server" SkinID="MessageBox" />
                <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>
                        <asp:ServiceReference Path="~/Services/Ajax/PullRequestService.svc" />
                    </Services>
                </tstsc:ScriptManagerProxyEx>
                <tstsc:SortedGrid
                    AllowColumnPositioning="false"
                    Authorized_ArtifactType="Task"
                    Authorized_Permission="BulkEdit"
                    ConcurrencyEnabled="true"
                    CssClass="DataGrid DataGrid-no-bands"
                    EditRowCssClass="Editing"
                    ErrorMessageControlId="divMessage"
                    FilterInfoControlId="lblFilterInfo"
                    HeaderCssClass="Header"
                    ID="grdPullRequests"
                    ItemImage="artifact-PullRequest.svg"
                    RowCssClass="Normal"
                    runat="server"
                    SelectedRowCssClass="Highlighted"
                    SubHeaderCssClass="SubHeader"
                    TotalCountControlId="lblTotalSetCount"
                    VisibleCountControlId="lblVisibleSetCount"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.PullRequestService">
                    <ContextMenuItems>
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Task" Authorized_Permission="View" />
                        <tstsc:ContextMenuItem Divider="True" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Main,PullRequests_NewPullRequest %>" ClientScriptMethod="btnNewPullRequest_click" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="Task" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,PullRequest_DeleteConfirm %>" />
                    </ContextMenuItems>
                </tstsc:SortedGrid>
                <br />
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
            </div>
        </div>
    </div>

    <tstsc:DialogBoxPanel
        ID="dlgCreatePullRequest"
        runat="server"
        CssClass="PopupPanel mobile-fullscreen"
        ErrorMessageControlId="msgCreateMessage"
        Modal="true"
        Width="500px"
        Height="300px"
        Title="<%$Resources:Main,PullRequests_NewPullRequest %>">
        <div class="PopupPanelBody">
            <tstsc:MessageBox ID="msgCreateMessage" runat="server" SkinID="MessageBox" />
            <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="true" ShowSummary="False" DisplayMode="BulletList"
                runat="server" ID="ValidationSummary1" />
            <div id="tblAttachment" style="width: 100%">
                <div class="mt3 df flex-column-xs">
                    <tstsc:LabelEx CssClass="w7" runat="server" ID="ddlSourceBranchLabel" AssociatedControlID="ddlSourceBranch" Required="true" Text="<%$Resources:Fields,SourceBranch %>" AppendColon="true" />
                    <tstsc:DropDownListEx ID="ddlSourceBranch" runat="server" DataValueField="BranchId" DataTextField="Name" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" />
                </div>
                <div class="mt3 df flex-column-xs">
                    <tstsc:LabelEx CssClass="w7" runat="server" ID="ddlDestBranchLabel" AssociatedControlID="ddlDestBranch" Required="true" Text="<%$Resources:Fields,DestBranch %>" AppendColon="true" />
                    <tstsc:DropDownListEx ID="ddlDestBranch" runat="server" DataValueField="BranchId" DataTextField="Name" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" />
                </div>
                <div class="mt3 df flex-column-xs">
                    <tstsc:LabelEx CssClass="w7" runat="server" ID="txtNameLabel" AssociatedControlID="txtName" Required="true" Text="<%$Resources:Fields,Name %>" AppendColon="true" />
                    <div class="grow-1">
                        <tstsc:TextBoxEx ID="txtName" runat="server" MaxLength="50" Text="" />
                    </div>
                </div>
                <div class="mt3 df flex-column-xs">
                    <tstsc:LabelEx CssClass="w7" runat="server" ID="ddlReleaseLabel" AssociatedControlID="ddlRelease" Required="false" Text="<%$Resources:Fields,ReleaseId %>" AppendColon="true" ActiveItemField="IsActive" />
                    <tstsc:DropDownHierarchy ID="ddlRelease" runat="server" DataValueField="ReleaseId" DataTextField="FullName" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" SkinID="ReleaseDropDownList" />
                </div>
                <div class="mt3 df flex-column-xs">
                    <tstsc:LabelEx CssClass="w7" runat="server" ID="ddlOwnerLabel" AssociatedControlID="ddlOwner" Required="false" Text="<%$Resources:Fields,OwnerId %>" AppendColon="true" />
                    <tstsc:DropDownUserList ID="ddlOwner" runat="server" DataValueField="UserId" DataTextField="FullName" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" />
                </div>
                <div class="mt4 mb3 btn-group ml7">
                    <tstsc:HyperLinkEx ID="btnCreate" SkinID="ButtonPrimary" runat="server" CausesValidation="true" Text="<%$Resources:Buttons,Create %>" ClientScriptMethod="btnCreate_click()" />
                    <tstsc:HyperLinkEx ID="btnCancel" SkinID="ButtonDefault" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgCreatePullRequest" ClientScriptMethod="close()" />
                </div>
            </div>
        </div>
    </tstsc:DialogBoxPanel>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        function btnNewPullRequest_click() {
            var dlgCreatePullRequest = $find('<%=dlgCreatePullRequest.ClientID%>');
            dlgCreatePullRequest.display();

            //Clear form inputs
            var ddlSourceBranch = $find('<%=ddlSourceBranch.ClientID%>');
            var ddlDestBranch = $find('<%=ddlDestBranch.ClientID%>');
            var ddlRelease = $find('<%=ddlRelease.ClientID%>');
            var ddlOwner = $find('<%=ddlOwner.ClientID%>');
            var txtName = $get('<%=txtName.ClientID%>');

            ddlSourceBranch.set_selectedItem('');
            ddlDestBranch.set_selectedItem('');
            ddlRelease.set_selectedItem('');
            ddlOwner.set_selectedItem('');
            txtName.value = '';
        }
        function btnCreate_click()
        {
            //Get control references
            var ddlSourceBranch = $find('<%=ddlSourceBranch.ClientID%>');
            var ddlDestBranch = $find('<%=ddlDestBranch.ClientID%>');
            var ddlRelease = $find('<%=ddlRelease.ClientID%>');
            var ddlOwner = $find('<%=ddlOwner.ClientID%>');
            var txtName = $get('<%=txtName.ClientID%>');

            //Validate data entry
            if (txtName.value == '')
            {
				globalFunctions.globalAlert(resx.PullRequests_NameNeeded, 'warning', true);
                return;
            }
            if (!ddlSourceBranch.get_selectedItem() || ddlSourceBranch.get_selectedItem().get_value() == '') {
				globalFunctions.globalAlert(resx.PullRequests_SourceBranchNeeded, 'warning', true);
                return;
            }
            if (!ddlDestBranch.get_selectedItem() || ddlDestBranch.get_selectedItem().get_value() == '') {
				globalFunctions.globalAlert(resx.PullRequests_DestBranchNeeded, 'warning', true);
                return;
            }

            //Create the pull request and reload the grid if successful
            var name = txtName.value;
            var sourceBranch = ddlSourceBranch.get_selectedItem().get_text();
            var destBranch = ddlDestBranch.get_selectedItem().get_text();

            //Make sure the TO/FROM branches are not the same
            if (sourceBranch == destBranch) {
				globalFunctions.globalAlert(resx.PullRequests_DifferentBranchesNeeded, 'warning', true);
                return;
            }

            var ownerId = null;
            if (ddlOwner.get_selectedItem() && ddlOwner.get_selectedItem().get_value() != '') {
                ownerId = ddlOwner.get_selectedItem().get_value();
            }
            var releaseId = null;
            if (ddlRelease.get_selectedItem() && ddlRelease.get_selectedItem().get_value() != '') {
                releaseId = parseInt(ddlRelease.get_selectedItem().get_value());
            }

            //Display spinner and make call
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.PullRequestService.PullRequest_Create(SpiraContext.ProjectId, name, sourceBranch, destBranch, ownerId, releaseId, createPullRequest_success, createPullRequest_failure);
        }
        function createPullRequest_success(taskId)
        {
            //Stop spinner
            globalFunctions.hide_spinner();

            //Close dialog
            var dlgCreatePullRequest = $find('<%=dlgCreatePullRequest.ClientID%>');
            dlgCreatePullRequest.close();

            //Reload grid
            var grdPullRequests = $find('<%=grdPullRequests.ClientID%>');
            grdPullRequests.load_data();
        }
        function createPullRequest_failure(exception)
        {
            //Stop spinner
            globalFunctions.hide_spinner();

            //Display error
            globalFunctions.display_error($get('<%=msgCreateMessage.ClientID%>'), exception);
        }

        var txtName_id = '<%=txtName.ClientID%>';
        $(document).ready(function () {
            //Add standard merge text to name of merge request
            $('#' + txtName_id).on("focus", function () {
                var ddlSourceBranch = $find('<%=ddlSourceBranch.ClientID%>');
                var ddlDestBranch = $find('<%=ddlDestBranch.ClientID%>');
                if ((!$get(txtName_id).value || $get(txtName_id).value == '')
                    && ddlSourceBranch.get_selectedItem() && ddlDestBranch.get_selectedItem()
                    && ddlSourceBranch.get_selectedItem().get_value() != '' && ddlDestBranch.get_selectedItem().get_value() != '')
                {
                    var sourceBranch = ddlSourceBranch.get_selectedItem().get_text();
                    var destBranch = ddlDestBranch.get_selectedItem().get_text();

                    //Populate the default name
                    var name = resx.PullRequests_DefaultName.replace('{0}', sourceBranch).replace('{1}', destBranch);
                    $get(txtName_id).value = name;
                }
            });
        });
    </script>
</asp:Content>
