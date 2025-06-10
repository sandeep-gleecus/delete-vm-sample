<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IncidentListPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.IncidentListPanel" %>
<tstsc:MessageBox 
    ID="divIncidentsMessage" 
    runat="server" 
    SkinID="MessageBox" 
    />
<div class="TabControlHeader">
    <div class="btn-group priority1">
        <tstsc:HyperLinkEx 
            ClientScriptMethod="load_data()" 
            ClientScriptServerControlId="grdIncidentList" 
            ID="lnkRefresh" 
            NavigateUrl="javascript:void(0)"
            runat="server" 
            SkinID="ButtonDefault"
            >
            <asp:Localize 
                runat="server" 
                Text="<%$Resources:Buttons,Refresh %>" 
                />
            <span class="fas fa-sync"></span>
        </tstsc:HyperLinkEx>
        <tstsc:DropDownListEx 
            AutoPostBack="false" 
            ClientScriptMethod="toggle_visibility" 
            ClientScriptServerControlId="grdIncidentList"
            CssClass="DropDownList" 
            DataTextField="Value" 
            DataValueField="Key"
            ID="ddlShowHideIncidentColumns" 
            NoValueItem="True"
            NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" 
            runat="server" 
            Width="180px" 
            />
        <tstsc:DropMenu 
            ClientScriptMethod="apply_filters()"
		    ClientScriptServerControlId="grdIncidentList" 
            GlyphIconCssClass="mr3 fas fa-filter" 
            id="btnFilters" 
            runat="server" 
            Text="<%$Resources:Buttons,Filter %>"
            >
		    <DropMenuItems>
			    <tstsc:DropMenuItem 
                    ClientScriptMethod="apply_filters()" 
                    GlyphIconCssClass="mr3 fas fa-filter" 
                    Name="Apply" 
                    runat="server" 
                    SkinID="ButtonDefault" 
                    Value="<%$Resources:Buttons,ApplyFilter %>" 
                    />
			    <tstsc:DropMenuItem 
                    ClientScriptMethod="clear_filters()" 
                    GlyphIconCssClass="mr3 fas fa-times" 
                    Name="Clear" 
                    runat="server" 
                    SkinID="ButtonDefault" 
                    Value="<%$Resources:Buttons,ClearFilter %>" 
                    />
		    </DropMenuItems>
	    </tstsc:DropMenu>        
    </div>
</div>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
    <span class="fas fa-info-circle"></span>
	<asp:Localize ID="Localize25" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblIncidentVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize26" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblIncidentTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="locArtifactTypeLegend" runat="server" />.
    <tstsc:LabelEx ID="lblIncidentFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid 
    AllowColumnPositioning="true"
    Authorized_ArtifactType="Incident"
    Authorized_Permission="BulkEdit"
    AutoLoad="false" 
    ConcurrencyEnabled="true" 
    CssClass="DataGrid DataGrid-no-bands" 
    EditRowCssClass="Editing" 
    ErrorMessageControlId="divIncidentsMessage"
    FilterInfoControlId="lblIncidentFilterInfo"
    HeaderCssClass="Header" 
    ID="grdIncidentList" 
    ItemImage="artifact-Incident.svg" 
    RowCssClass="Normal" 
    runat="server" 
    SelectedRowCssClass="Highlighted" 
    SubHeaderCssClass="SubHeader" 
    TotalCountControlId="lblIncidentTotalCount" 
    VisibleCountControlId="lblIncidentVisibleCount" 
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
    />

<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.IncidentPanel = function ()
    {
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.IncidentPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.IncidentPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'dispose');
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
            Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
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
            var grdIncidentList = $find('<%=grdIncidentList.ClientID%>');
            grdIncidentList.set_standardFilters(filters);
            if (loadNow)
            {
                grdIncidentList.load_data();
            }
            else
            {
                grdIncidentList.clear_loadingComplete();
            }
        }
    }
    var tstucIncidentPanel = $create(Inflectra.SpiraTest.Web.UserControls.IncidentPanel);

</script>