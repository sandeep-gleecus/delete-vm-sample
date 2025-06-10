<%@ Control
	Language="c#"
	AutoEventWireup="True"
	CodeBehind="BaselinePanel.ascx.cs"
	Inherits="Inflectra.SpiraTest.Web.UserControls.BaselinePanel"
	TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>

<%@ Register
	TagPrefix="tstsc"
	Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<tstsc:MessageBox ID="divMessage" runat="server" SkinID="MessageBox" />
<div runat="server" id="baselineAddPanel"></div>

<asp:Panel runat="server" ID="panelControl" class="TabControlHeader">
	<div class="btn-toolbar-mid-page">
		<div class="btn-group priority3 mr5">
			<tstsc:HyperLinkEx
                Authorized_ArtifactType="Release"
                Authorized_Permission="Create"
				ID="btnNewBaseline"
                NavigateUrl="javascript:void(0)" 
                runat="server" 
                SkinID="ButtonDefault" 
                >
                <span class="fas fa-layer-plus"></span>
                <asp:Localize 
                    Text="<%$Resources:Buttons,NewBaseline %>" 
                    runat="server" 
                    />
		    </tstsc:HyperLinkEx>
			<tstsc:HyperLinkEx runat="server"
				ID="lnkRefresh"
				SkinID="ButtonDefault"
				NavigateUrl="javascript:void(0)"
				ClientScriptServerControlId="grdBaselineList"
				ClientScriptMethod="load_data()"
                >
                <span class="fas fa-sync"></span>
                <asp:Localize 
                    Text="<%$ Resources:Buttons,Refresh %>" 
                    runat="server" 
                    />
			</tstsc:HyperLinkEx>
			<tstsc:DropMenu runat="server"
				ID="btnFilters"
				GlyphIconCssClass="mr3 fas fa-filter"
				Text="<%$ Resources:Buttons,Filter %>"
				ClientScriptServerControlId="grdBaselineList"
				ClientScriptMethod="apply_filters()"
                >
				<DropMenuItems>
					<tstsc:DropMenuItem 
                        runat="server" 
                        SkinID="ButtonDefault" 
                        Name="Apply" 
                        Value="<%$ Resources:Buttons,ApplyFilter %>" 
                        GlyphIconCssClass="mr3 fas fa-filter" 
                        ClientScriptMethod="apply_filters()" 
                        />
					<tstsc:DropMenuItem runat="server" 
                        SkinID="ButtonDefault" 
                        Name="Clear" 
                        Value="<%$ Resources:Buttons,ClearFilter %>" 
                        GlyphIconCssClass="mr3 fas fa-times" 
                        ClientScriptMethod="clear_filters()" 
                        />
				</DropMenuItems>
			</tstsc:DropMenu>
		</div>
        <div class="btn-group priority3">
            <tstsc:DropMenu runat="server"
				Authorized_ArtifactType="Release"
				Authorized_Permission="Delete"
				ClientScriptMethod="delete_items()"
				ClientScriptServerControlId="grdBaselineList"
				Confirmation="True"
				ConfirmationMessage="<%$ Resources:Messages,Confirm_BaselineDelete %>" 
				GlyphIconCssClass="mr3 fad fa-layer-minus"
				ID="btnDelete"
				Text="<%$ Resources:Buttons,Delete %>"
                />
        </div>
	</div>
</asp:Panel>

<!-- Filter Information -->
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
	<asp:Label ID="lblVisibleCount" runat="server" Font-Bold="True" />
	<asp:Localize runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
	<asp:Label ID="lblTotalCount" runat="server" Font-Bold="True" />
	<asp:Localize runat="server" Text="<%$Resources:Main,HistoryPanel_Changes %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>

<!-- The Grid -->
<tstsc:SortedGrid runat="server"
	ID="grdBaselineList"
	CssClass="DataGrid DataGrid-no-bands"
	HeaderCssClass="Header"
	VisibleCountControlId="lblVisibleCount"
	TotalCountControlId="lblTotalCount"
	FilterInfoControlId="lblFilterInfo"
	SubHeaderCssClass="SubHeader"
	SelectedRowCssClass="Highlighted"
	ErrorMessageControlId="divMessage"
	EditRowCssClass="Editing"
    Authorized_ArtifactType="Release"
    Authorized_Permission="Modify"
	RowCssClass="Normal"
	DisplayAttachments="false"
	AllowEditing="true"
	DisplayCheckboxes="true"
    ItemImage="artifact-Baseline.svg"
	WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BaselineService" />

<br />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
	<Services>
		<asp:ServiceReference Path="~/Services/Ajax/BaselineService.svc" />
	</Services>
    <Scripts>
        <asp:ScriptReference Path="~/TypeScript/BaselineAdd.js" />
    </Scripts>
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
	/* The User Control Class */
	Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
	Inflectra.SpiraTest.Web.UserControls.BaselinePanel = function () {
		this._userId = <%= UserId %>;
		this._projectId = <%= ProjectId %>;
		this._artifactId = <%= ArtifactId %>;
		this._artifactTypeId = <%= (int)this.ArtifactTypeEnum %>;
		Inflectra.SpiraTest.Web.UserControls.BaselinePanel.initializeBase(this);
	}

	Inflectra.SpiraTest.Web.UserControls.BaselinePanel.prototype =
	{
		initialize: function () {
			Inflectra.SpiraTest.Web.UserControls.BaselinePanel.callBaseMethod(this, 'initialize');
		},
		dispose: function () {
			Inflectra.SpiraTest.Web.UserControls.BaselinePanel.callBaseMethod(this, 'dispose');
		},

		/* Properties */
		get_artifactId: function () {
			return this._artifactId;
		},
		set_artifactId: function (value) {
			this._artifactId = value;
		},

		get_artifactTypeId: function () {
			return this._artifactTypeId;
		},
		set_artifactTypeId: function (value) {
			this._artifactTypeId = value;
		},

		/* Functions */
		check_hasData: function (callback) {
			//See if we have data
			var artifactReference = {
				artifactId: this._artifactId,
				artifactTypeId: this._artifactTypeId,
			};
			Inflectra.SpiraTest.Web.Services.Ajax.BaselineService.Baseline_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
		},
		check_hasData_success: function (count, callback) {
			if (callback) {
				//Specify if we have data or not
				callback(count > 0);
			}
		},
		check_hasData_failure: function (ex) {
			console.log("Error calling service: " + ex._message);
		},

		load_data: function (filters, loadNow) {
			var grdBaseList = $find('<%= grdBaselineList.ClientID %>');
            grdBaseList.set_standardFilters(filters);

            //Update the grids edit permissions
            if (grdBaseList) {
                var authorizationThisRelease = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, globalFunctions.artifactTypeEnum.release);
                var canModifyThisRelease = false;
                var ajxFormManager = $find(ajxFormManager_id);
                var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
                if (authorizationThisRelease == globalFunctions.authorizationStateEnum.authorized)
                {
                    canModifyThisRelease = true;
                }
                if (authorizationThisRelease == globalFunctions.authorizationStateEnum.limited && isCreatorOrOwner)
                {
                    canModifyThisRelease = true;
                }
                grdBaseList.set_allowEdit(canModifyThisRelease);
            }

			if (loadNow) {
				grdBaseList.load_data();
			}
			else {
				grdBaseList.clear_loadingComplete();
			}
		}
	}
    var tstucBaselinePanel = $create(Inflectra.SpiraTest.Web.UserControls.BaselinePanel);

    //adding a new baseline - open the react panel
	$('#<%=btnNewBaseline.ClientID%>').on("click", function () {
        ReactDOM.render(
            React.createElement(RctBaselineAdd, {
                lnkAddBtnId: '<%=btnNewBaseline.ClientID%>',
                domId: '<%=baselineAddPanel.ClientID%>',
                gridId: '<%=grdBaselineList.ClientID%>'
            }, null),
            document.getElementById('<%=baselineAddPanel.ClientID%>')
        );
    });

</script>
