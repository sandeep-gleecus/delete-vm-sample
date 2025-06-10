<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="Inflectra.SpiraTest.Web.ReportViewer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
        <div class="mx-auto w10 mw-100 px4-sm mt6 mt5-sm tc">
            <h2 class="fs-h1 mb5 yolk">
                <asp:Literal ID="ltrReportTitle" runat="server" />
            </h2>
            <div class="my3">
                <tstsc:HyperLinkEx  SkinID="ButtonDefaultWrap" ID="lnkBackToReportConfiguration" runat="server">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Main,ReportViewer_BackToReportConfiguration %>" />
                </tstsc:HyperLinkEx>
            </div>
		    <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
            <div class="h8 bg-off-white br3 mt5 mb4 pa3 ba b-vlight-gray df justify-center items-center">
                <div id="reportBeingGenerated" class="pa4 fs-110 mb4 o-50">
                    <asp:Localize runat="server" Text="<%$Resources:Main,ReportViewer_ReportBeingGenerated %>" />
                </div>
                <div id="divReportGenerated" class="pa4" style="display:none;">
                    <p class="fs-110 mb4">
                        <asp:Localize runat="server" Text="<%$Resources:Main,ReportViewer_GenerationComplete %>" />
                    </p>
                    <tstsc:HyperLinkEx SkinID="ButtonPrimaryWrap" runat="server" Target="_blank" ID="lnkDisplayReport" ClientScriptMethod="lnkDisplayReport_click()">
                        <asp:Localize runat="server" Text="<%$Resources:Main,ReportViewer_ClickToDisplayReport %>" />
                        <span class="fas fa-external-link-alt"></span>
                    </tstsc:HyperLinkEx>                    
                </div>
                <div id="divRegenerateReport" style="display:none; padding: 10px;">
                    <p class="alert alert-success alert-narrow">
                        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,ReportViewer_GenerationComplete %>" />
                    </p>
                    <tstsc:HyperLinkEx  SkinID="ButtonPrimaryWrap" runat="server" NavigateUrl="javascript:void(0)" ID="lnkRegenerateReport" ClientScriptMethod="ajxBackgroundProcessManager_init()">
                        <span class="fas fa-sync"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,ReportViewer_ClickToRegenerateReport %>" />
                    </tstsc:HyperLinkEx>
                </div>
            </div>
        </div>

    <tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage"
            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
        </Services>  
    </tstsc:ScriptManagerProxyEx>

    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;
        function ajxBackgroundProcessManager_init()
        {
            var projectId = <%=ProjectId %>;
            var reportId = <%=reportId %>;
            var queryString = '<%=QueryString %>';
            var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

            //Actually start the background process of creating the report
            ajxBackgroundProcessManager.display(projectId, 'Report_Generate', resx.ReportViewer_GeneratingReport, resx.ReportViewer_GeneratingReportDesc, reportId, null, queryString);
        }
        function ajxBackgroundProcessManager_success(msg, returnCode)
        {
            //Need to redirect to the test runs pending
            if (returnCode && returnCode > 0)
            {
                var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
                var projectId = ajxBackgroundProcessManager.get_projectId();
                var baseUrl = '<%=ReportGeneratedUrl%>';
                var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
                var lnkDisplayReport = $get('<%=lnkDisplayReport.ClientID%>');
                lnkDisplayReport.href = url;

                //Displays the link after being generated
                $('#reportBeingGenerated').hide();
                $('#divReportGenerated').show();
                $('#divRegenerateReport').hide();
            }
        }
        function lnkDisplayReport_click()
        {
            //Hides the link after being clicked
            $('#divReportGenerated').hide();
            $('#divRegenerateReport').show();
        }
    </script>
</asp:Content>
