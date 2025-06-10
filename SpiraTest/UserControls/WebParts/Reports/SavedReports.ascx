<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="SavedReports.ascx.cs" 
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SavedReports" 
    %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<tstsc:DataListEx 
    ID="lstSavedReports" 
    runat="server" 
    DataMember="ReportSaved"
    ShowHeader="false" 
    ShowFooter="false" 
    BorderStyle="None"
    >
    <ItemStyle BorderStyle="None" />
    
    <ItemTemplate>
        <tstsc:HyperLinkEx 
            SkinID="ButtonLink"
            ID="btnDisplayReport" 
            runat="server"
            Target="_blank"
            NavigateUrl='<%# "~/" + ProjectId + "/Report/Saved/" + ((SavedReportView)Container.DataItem).ReportSavedId + ".aspx?" + GlobalFunctions.PARAMETER_THEME_NAME + "=" + this.Page.Theme%>' 
            ToolTip='<%#:((SavedReportView)Container.DataItem).Name %>'
            >
            <%#:GlobalFunctions.TruncateName((string)((SavedReportView)Container.DataItem).Name)%>    
        </tstsc:HyperLinkEx>
        <small>
            <tstsc:LinkButtonEx 
                ID="btnDeleteReport" 
                runat="server" 
                CommandName="DeleteReport" 
                CommandArgument='<%#((SavedReportView)Container.DataItem).ReportSavedId %>' 
                CausesValidation="false" 
                Confirmation="true" 
                ConfirmationMessage="<%$Resources:Messages,Reports_DeleteSaved %>" Text="<%$Resources:Buttons,Delete %>" 
                />
        </small>
    </ItemTemplate>
    
</tstsc:DataListEx>
<tstsc:LabelEx 
    ID="lblNoSavedReports" 
    runat="server" 
    Visible="false" 
    Text="<%$Resources:Main,Reports_NoneAvailable %>" 
    />
