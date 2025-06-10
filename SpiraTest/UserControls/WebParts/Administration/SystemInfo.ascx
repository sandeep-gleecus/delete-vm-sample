<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SystemInfo.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.SystemInfo" %>
<div class="form-horizonal">
    <div class="form-group row">
        <tstsc:LabelEx runat="server" Text="<%$ Resources:Fields,ProductType %>" AppendColon="true" CssClass="control-label col-sm-4" />
        <div class="col-sm-8">
            <tstsc:HyperLinkEx ID="lnkLicenseProduct" runat="server" />
        </div>
    </div>
    <div class="form-group row">
        <div class="control-label col-sm-4">
            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_LicenseType %>" Font-Bold="true" AssociatedControlID="lblLicenseType" ID="lblLicenseTypeLabel" />:
        </div>
        <div class="col-sm-8">
            <tstsc:LabelEx ID="lblLicenseType" runat="server" />
            <tstsc:HyperLinkEx
                ID="HyperLinkEx1" SkinID="ButtonDefault"
                NavigateUrl='<%# "~/Administration/LicenseDetails.aspx" %>' runat="server" Text="<%$ Resources:Main,Global_ViewDetails %>" />
        </div>
    </div>
    <div class="form-group row">
        <div class="control-label col-sm-4">
            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_NumConcurrentUsers %>" Font-Bold="true" AssociatedControlID="lblConcurrentUsers" ID="lblConcurrentUsersLabel" />:
        </div>
        <div class="col-sm-8">
            <tstsc:LabelEx ID="lblConcurrentUsers" runat="server" />&nbsp;
            <asp:Localize runat="server" Text="<%$ Resources:Fields,general_isactive %>" />
            <tstsc:HyperLinkEx
                ID="lnkActiveSessions" SkinID="ButtonDefault"
                NavigateUrl='<%# "~/Administration/ActiveSessions.aspx" %>' runat="server" Text="<%$ Resources:Main,Admin_LicenseDetails_ViewActiveSessions %>" />
        </div>
    </div>
    <div class="form-group row">
        <div class="control-label col-sm-4">
            <tstsc:LabelEx runat="server" Text="<%$Resources:Main,LicenseDetails_Expiration %>" Font-Bold="true" AssociatedControlID="lblLicenseExpiration" ID="lblLicenseExpirationLabel" />:
        </div>
        <div class="col-sm-8">
            <tstsc:LabelEx ID="lblLicenseExpiration" runat="server" />
        </div>
    </div>
    <div class="form-group row">
        <div class="control-label col-sm-4">
            <tstsc:LabelEx ID="txtLicenseOrganizationLabel" runat="server" Text="<%$Resources:Main,LicenseDetail_Organization %>" AppendColon="true"/>
        </div>
        <div class="col-sm-8">
            <tstsc:LabelEx ID="lblOrganization" runat="server" />
        </div>
    </div>
    <div class="form-group row">
        <tstsc:LabelEx runat="server" Text="<%$ Resources:Main,SystemInfo_NumberProjects %>" AppendColon="true" CssClass="control-label col-sm-4" />
        <div class="col-sm-8">
            <tstsc:HyperLinkEx ID="lnkNumberProjects" runat="server" NavigateUrl="~/Administration/ProjectList.aspx" />
        </div>
    </div>
    <asp:Placeholder runat="server" ID="plcDeleteSampleData" Visible="false">
        <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
        <div class="pa4 bg-off-white br3">
            <p>
                <asp:Localize runat="server" Text="<%$ Resources:Main,SystemInfo_DeleteSampleDataInfo %>" />
            </p>
            <tstsc:ButtonEx 
                ClientScriptMethod="deleteSampleDataConfirm()" 
                runat="server"
                ID="btnDeleteSampleData"
                Text="<%$Resources:Buttons,Delete %>"
                />
        </div>
        <script type="text/javascript">
            function deleteSampleDataConfirm() {
                globalFunctions.globalConfirm(
                    resx.SystemInfo_DeleteSampleDataConfirm,
                    "info",
                    deleteSampleData,
                    null,
                    null,
                    null
                )
            }

            function deleteSampleData(confirm) {
                if (confirm) {
                    var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
                    ajxBackgroundProcessManager.display(null, 'System_DeleteSampleData', resx.SystemInfo_DeleteSampleData, resx.SystemInfo_DeleteSampleData);
                }
            }

            function deleteSampleData_completed() {
                //reload the page
                window.location.reload(true);
            }
        </script>
    </asp:Placeholder>
</div>
<tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
	<Services>
		<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
	</Services>
</tstsc:ScriptManagerProxyEx>
