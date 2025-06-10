<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestRunListPanel.ascx.cs"
    Inherits="Inflectra.SpiraTest.Web.UserControls.TestRunListPanel" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>  
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
<div class="TabControlHeader">
    <div class="btn-group priority3">
        <tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="load_data()">
            <span class="fas fa-sync"></span>
            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
        </tstsc:HyperLinkEx>
        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="apply_filters()">
			<DropMenuItems>
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			</DropMenuItems>
		</tstsc:DropMenu>
    </div>
    <div class="btn-group priority4">
        <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTestRunList" ClientScriptMethod="toggle_visibility" />
    </div>
</div>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,TestRunListPanel_TestRuns %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid ID="grdTestRunList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" AllowColumnPositioning="true"
    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
    RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-TestRun.svg"
    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestRunService"
    Authorized_ArtifactType="TestRun" Authorized_Permission="BulkEdit">
    <ContextMenuItems>
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="TestRun" Authorized_Permission="View" />
        <tstsc:ContextMenuItem Divider="True" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="TestRun" Authorized_Permission="BulkEdit" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="TestRun" Authorized_Permission="Delete" ConfirmationMessage="<%$Resources:Messages,TestRunList_DeleteConfirm %>" />
    </ContextMenuItems> 
</tstsc:SortedGrid>

<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.TestRunPanel = function ()
    {
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.TestRunPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.TestRunPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TestRunPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TestRunPanel.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_artifactId: function()
        {
            return this._artifactId;
        },
        set_artifactId: function(value)
        {
            this._artifactId = value;
        },

        get_artifactTypeId: function()
        {
            return this._artifactTypeId;
        },
        set_artifactTypeId: function(value)
        {
            this._artifactTypeId = value;
        },

        /* Functions */
        check_hasData: function(callback)
        {
            //See if we have data
            var artifactReference = {
                artifactId: this._artifactId,
                artifactTypeId: this._artifactTypeId,
            };
            Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.TestRun_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
        },
        check_hasData_success: function(count, callback)
        {
            if (callback)
            {
                //Specify if we have data or not
                callback(count > 0);
            }
        },
        check_hasData_failure: function(ex)
        {
            //Fail quietly
        },

        load_data: function(filters, loadNow)
        {
            var baseUrl = '<%=this.GridBaseUrl%>';
            if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testCase)
            {
                baseUrl += '?testCaseId=' + SpiraContext.ArtifactId;
            }
            if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testSet)
            {
                baseUrl += '?testSetId=' + SpiraContext.ArtifactId;
            }
            if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.release)
            {
                baseUrl += '?releaseId=' + SpiraContext.ArtifactId;
            }
            if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.automationHost)
            {
                baseUrl += '?automationHostId=' + SpiraContext.ArtifactId;
            }

            //Set the base URL of the grid and standard filters depending on the artifact we're for
            var grdTestRunList = $find('<%=grdTestRunList.ClientID%>');
            grdTestRunList.set_standardFilters(filters);
            grdTestRunList.set_baseUrl(baseUrl);
            if (loadNow)
            {
                grdTestRunList.load_data();
            }
            else
            {
                grdTestRunList.clear_loadingComplete();
            }
        }
    }
    var tstucTestRunPanel = $create(Inflectra.SpiraTest.Web.UserControls.TestRunPanel);

</script>