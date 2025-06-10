<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentBuilds.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome.RecentBuilds" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<table id="<%=this.UniqueID + "_tblRecentBuilds"%>" class="WidgetGrid" style="width:100%">
    <thead>
        <tr>
            <th class="priority2">
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Release %>" />
            </th>
            <th class="priority1">
                <asp:Localize runat="server" Text="<%$Resources:Fields,BuildId %>" />
            </th>
            <th class="priority1">
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,BuildStatusId %>" />
            </th>
            <th class="priority3">
                <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,CreationDate %>" />
            </th>
        </tr>
    </thead>
    <tbody>
        <!-- ko with: programs -->
        <!-- ko foreach: $data -->
            <tr>
                <td class="priority1" colspan="4">
                    <tstsc:ImageEx ImageUrl="Images/org-Program-Outline.svg" AlternateText="<%$Resources:Fields,Program %>" ToolTip="<%$Resources:Fields,Program %>" runat="server" CssClass="w4 h4" style="margin-left: 10px;" />
                    <a data-bind="attr: { href: urlTemplate_projectGroupHome.replace(globalFunctions.projectIdToken, workspaceId()) }">
                        <span data-bind="text: workspaceName"></span>
                    </a>
                </td>
            </tr>
            <!-- ko with: children -->
            <!-- ko foreach: $data -->
                <tr>
                    <td class="priority1" colspan="4">
                        <tstsc:ImageEx ImageUrl="Images/org-Project-Outline.svg" AlternateText="<%$Resources:Fields,Project %>" ToolTip="<%$Resources:Fields,Project %>" runat="server" CssClass="w4 h4" style="margin-left: 20px;" />
                        <a data-bind="attr: { href: urlTemplate_projectHome.replace(globalFunctions.projectIdToken, workspaceId()) }">
                        <span data-bind="text: workspaceName"></span>
                        </a>
                    </td>
                </tr>
                <!-- ko with: dataItems -->
                <!-- ko foreach: $data -->
                    <tr>
                        <td class="priority2">
                            <tstsc:ImageEx ImageUrl="Images/artifact-Release.svg" AlternateText="<%$Resources:Fields,Release %>" ToolTip="<%$Resources:Fields,Release %>" runat="server" CssClass="w4 h4" style="margin-left: 30px;" />
                            <a data-bind="attr: { href: urlTemplate_releaseDetails.replace(globalFunctions.projectIdToken, Fields.ProjectId.intValue()).replace(globalFunctions.artifactIdToken, Fields.ReleaseId.intValue()) }, event: { mouseover: tblRecentBuilds_releaseMouseOver, mouseout: tblRecentBuilds_releaseMouseOut }">
                                <span data-bind="text: Fields.ReleaseId.textValue"></span>
                            </a> 
                        </td>
                        <td class="priority1">
                            <a data-bind="attr: { href: urlTemplate_buildDetails.replace(globalFunctions.projectIdToken, Fields.ProjectId.intValue()).replace(globalFunctions.artifactIdToken, primaryKey()) }, event: { mouseover: tblRecentBuilds_mouseOver, mouseout: tblRecentBuilds_mouseOut }">
                                <span data-bind="text: Fields.Name.textValue"></span>
                            </a> 
                        </td>
                        <td class="priority1"  data-bind="css: Fields.BuildStatusId.cssClass">
                            <span data-bind="text: Fields.BuildStatusId.textValue"></span>
                        </td>
                        <td class="priority3">
                            <span data-bind="text: Fields.CreationDate.textValue"></span>
                        </td>
                    </tr>
                <!-- /ko -->
                <!-- /ko -->
            <!-- /ko -->
            <!-- /ko -->
        <!-- /ko -->
        <!-- /ko -->
    </tbody>
</table>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/PortfolioService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">

    //View Model
    var g_<%=this.UniqueID%>_newsViewModel = null;

    //Url templates
    var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, -2, 0)))%>';
    var urlTemplate_releaseDetails = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Releases, -2, -2)))%>';
    var urlTemplate_buildDetails = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Builds, -2, -2)))%>';
    var urlTemplate_projectGroupHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveGroupRewriterURL(UrlRoots.NavigationLinkEnum.ProjectGroupHome, -2)))%>';

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_recentBuilds(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_recentBuilds(); });

    var tblRecentBuilds_isOverNameDesc = false;
    function tblRecentBuilds_mouseOver(build, evt)
    {
        var buildId = build.primaryKey();
        var projectId = build.Fields.ProjectId.intValue();

        //Display the loading message
        ddrivetip(resx.GlobalFunctions_TooltipLoading);
        tblRecentBuilds_isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        Inflectra.SpiraTest.Web.Services.Ajax.BuildService.RetrieveNameDesc(projectId, buildId, null, tblRecentBuilds_mouseOver_success, tblRecentBuilds_mouseOver_failure);
    }
    function tblRecentBuilds_mouseOver_success (tooltipData)
    {
        if (tblRecentBuilds_isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    }
    function tblRecentBuilds_mouseOver_failure (exception)
    {
        //Fail quietly
    }
    function tblRecentBuilds_mouseOut(build, evt)
    {
        hideddrivetip();
        tblRecentBuilds_isOverNameDesc = false;
    }

    var tblRecentBuilds_isOverRelease = false;
    function tblRecentBuilds_releaseMouseOver(build, evt)
    {
        var releaseId = build.Fields.ReleaseId.intValue();
        var projectId = build.Fields.ProjectId.intValue();

        //Display the loading message
        ddrivetip(resx.GlobalFunctions_TooltipLoading);
        tblRecentBuilds_isOverRelease = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService.RetrieveNameDesc(projectId, releaseId, null, tblRecentBuilds_releaseMouseOver_success, tblRecentBuilds_releaseMouseOver_failure);
    }
    function tblRecentBuilds_releaseMouseOver_success (tooltipData)
    {
        if (tblRecentBuilds_isOverRelease)
        {
            ddrivetip(tooltipData);
        }
    }
    function tblRecentBuilds_releaseMouseOver_failure (exception)
    {
        //Fail quietly
    }
    function tblRecentBuilds_releaseMouseOut(build, evt)
    {
        hideddrivetip();
        tblRecentBuilds_isOverRelease = false;
    }

    function load_recentBuilds() {
        //Load the list of builds and databind using KnockoutJS
        var portfolioId = SpiraContext.PortfolioId;
        var rowsToDisplay = <%=RowsToDisplay.ToString()%>;
        Inflectra.SpiraTest.Web.Services.Ajax.PortfolioService.Portfolio_RetrieveBuilds(portfolioId, rowsToDisplay, function (data)
        {
            //Success

            //Databind
            if (g_<%=this.UniqueID%>_newsViewModel)
            {
                ko.mapping.fromJS(data, g_<%=this.UniqueID%>_newsViewModel);
            }
            else
            {
                g_<%=this.UniqueID%>_newsViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(g_<%=this.UniqueID%>_newsViewModel, $get('<%=this.UniqueID + "_tblRecentBuilds"%>'));
            }
        }, function (ex)
        {
            //Display error
            var messageBox = $get('<%=this.MessageBoxClientID%>');
            globalFunctions.display_error(messageBox, ex);
        });
    }

</script>
