<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TestSetListPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.TestSetListPanel" %>
<tstsc:MessageBox ID="divTestSetsMessage" runat="server" SkinID="MessageBox" />
<div class="TabControlHeader">
    <div class="btn-group priority1">
		<tstsc:HyperLinkEx ID="btnRefreshTestSets" runat="server"
            NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdTestSetList"
            ClientScriptMethod="load_data()" SkinID="ButtonDefault">
            <span class="fas fa-sync"></span>
            <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>" />
		</tstsc:HyperLinkEx>
        <tstsc:DropDownListEx ID="ddlShowHideColumns" runat="server" DataValueField="Key"
            DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True"
            NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px"
            ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="toggle_visibility" />
        <tstsc:DropMenu id="DropMenu1" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
		    ClientScriptServerControlId="grdTestSetList" ClientScriptMethod="apply_filters()">
		    <DropMenuItems>
			    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
			    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
		    </DropMenuItems>
	    </tstsc:DropMenu>        
    </div>
</div>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
    <span class="fas fa-info-circle"></span>
    <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblTestSetVisibleCount" runat="server" Font-Bold="True" />
    <asp:Localize ID="Localize24" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTestSetTotalCount" runat="server" Font-Bold="True" />
    <asp:Localize ID="locTestSetsLegend" runat="server" Text="<%$Resources:Main,TestCaseDetails_TestSets %>" />.
	<tstsc:LabelEx ID="lblTestSetFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid ID="grdTestSetList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
    AllowColumnPositioning="true" VisibleCountControlId="lblTestSetVisibleCount"
    TotalCountControlId="lblTestSetTotalCount" FilterInfoControlId="lblTestSetFilterInfo"
    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divTestSetsMessage"
    RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-TestSet.svg" runat="server"
    Authorized_ArtifactType="TestSet" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
    Authorized_Permission="BulkEdit" ConcurrencyEnabled="true" AutoLoad="false" />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>
        <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
    </Services>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.TestSetPanel = function ()
    {
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.TestSetPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.TestSetPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TestSetPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TestSetPanel.callBaseMethod(this, 'dispose');
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
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.TestSet_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
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
            var grdTestSetList = $find('<%=grdTestSetList.ClientID%>');
            grdTestSetList.set_standardFilters(filters);
            if (loadNow)
            {
                grdTestSetList.load_data();
            }
            else
            {
                grdTestSetList.clear_loadingComplete();
            }
        }
    }
    var tstucTestSetPanel = $create(Inflectra.SpiraTest.Web.UserControls.TestSetPanel);

</script>
