<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ReleaseGantt.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.ReleaseGantt" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">

    <div id="tblMainBody" class="MainContent w-100">
        <div class="df flex-column min-h-insideHeadAndFoot">

            <div class="df items-center justify-between flex-wrap-xs mvw-100">
                <h1 class="w-100-xs fs-h3-xs fw-b-xs pl4 px3-xs">
                    <asp:Localize runat="server" Text="<%$Resources:Main,SiteMap_ReleaseGantt %>" />
                    <span class="badge">
                        <asp:Localize runat="server" Text="<%$Resources:Main,Global_Beta %>" />
                    </span>
                </h1>

                <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                    <div class="dif items-center pr4 pl4-sm btn-group" role="group">
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                            <span class="fas fa-indent"></span>
                        </a>
                        <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -9) %>' runat="server" title="<%$ Resources:Main,Global_Gantt %>">
                            <span class="fas fa-align-left"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Releases, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                            <span class="fas fa-project-diagram"></span>
                        </a>
                    </div>
                </asp:PlaceHolder> 
            </div>

            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

            <div class="dn-md-up db-sm alert alert-info mx4">
                <asp:Literal
                    runat="server"
                    Text="<%$Resources:Messages,Gantt_CannotDisplayOnThisScreen %>"
                    />
            </div>
            <div class="form-group mx4">
                <label class="v-top pr3">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,Global_Displaying %>"/>:</label>
                <tstsc:UnityDropDownHierarchy 
                    AutoPostBack="false" 
                    ActiveItemField="IsActive"
                    ClientScriptMethod="prepareGantt"
                    DataTextField="FullName" 
                    DataValueField="ReleaseId" 
                    Enabled="false"
                    ID="ddlSelectRelease" 
                    Runat="server" 
                    NoValueItem="true"
                    NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" 
                    />
            </div>


            <div id="releaseGanttChart" class="ml3 mr3 dn-sm"></div>

            <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
        </div>
    </div>

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>  
            <asp:ServiceReference Path="~/Services/Ajax/ProjectService.svc" />
        </Services>  
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/rct_comp_reactGantt.js" />
            <asp:ScriptReference Path="~/TypeScript/rct_comp_workspaceSchedule.js" />
            <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-gantt.js" />  
        </Scripts>
    </tstsc:ScriptManagerProxyEx>  

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var g_lblMessage_id = '<%=lblMessage.ClientID%>';
        $(document).ready(function () {
            prepareGantt();
        });

        var prepareGantt = function (releaseId) {
            //load the dashboard workspace overall data
            var projectId = <%=ProjectId%>;
            Inflectra.SpiraTest.Web.Services.Ajax.ProjectService.Workspace_RetrieveCompletionData(
                projectId,
                function (workspaceData) {

                    //Add the workspace type to the data
                    workspaceData.workspaceType = SpiraContext.WorkspaceEnums.product;
                    workspaceData.workspaceEnums = SpiraContext.WorkspaceEnums;

                    //Set other properties on the data
                    workspaceData.isCustomTaskClass = true;
                    workspaceData.productId = SpiraContext.ProjectId;
                    workspaceData.heightClass = "h-insideHeadAndFoot";

                    //Display the task Gantt chart
                    loadData(workspaceData);
                },
                function (ex) {
                    //Display the error message
                    globalFunctions.display_error($get(g_lblMessage_id), ex);
                }
            );
        };

        var loadData = function (workspaceData) {
            var divWorkspaceSchedule = $get('releaseGanttChart');
            var scheduleComponent = React.createElement(WorkspaceSchedule, workspaceData, null);
            ReactDOM.render(scheduleComponent, divWorkspaceSchedule);
        };
    </script>
</asp:Content>
