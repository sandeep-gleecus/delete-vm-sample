<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreateNewReport.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.CreateNewReport" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<div class="df flex-wrap">
    <asp:Repeater ID="rptReportCategories" runat="server">
        <ItemTemplate>
            <div class="w-100 w-33-sm">
                <div>
				    <h4><%#((ReportCategory) Container.DataItem).Name%></h4>
				    <asp:Repeater ID="rptReports" runat="server" DataSource="<%#reports%>">
					    <ItemTemplate>
						    <div></div>
						    <tstsc:HyperLinkEx Runat="server" SkinID="ButtonLink" ID="lnkConfigureReport"
                                NavigateUrl='<%# UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Reports, ProjectId, ((Report) Container.DataItem).ReportId, GlobalFunctions.PARAMETER_TAB_REPORT_CONFIGURATION) %>'
                                ToolTip='<%#"<u>" + ((Report)Container.DataItem).Name + "</u><br />" + ((Report) Container.DataItem).Description%>'><%#:((Report)Container.DataItem).Name%></tstsc:HyperLinkEx>
					    </ItemTemplate>
				    </asp:Repeater>
	            </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
