<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="ProjectGroupPlanningBoard.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.ProjectGroupPlanningBoard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="PlanningBoardStylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <table id="tblMainBody" class="MainContent w-100">
        <tr>
            <td class="px0 v-mid">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

               <div class="bg-white w-100 flex justify-between pt3 pb4">

                    <div class="dif items-center pl4" id="board-controls-view-groupby">
 
                        <tstsc:LabelEx runat="server" Text="<%$Resources:Main,SiteMap_Planning %>" ID="ddlSelectProjectLabel" AssociatedControlID="ddlSelectProject" AppendColon="true" />
                        <tstsc:UnityDropDownListEx ID="ddlSelectProject" runat="server" NoValueItem="true"
                            CssClass="u-dropdown"
                            NoValueItemText="<%$Resources:Main,PlanningBoard_GroupBacklog %>" AutoPostBack="false" DataTextField="Name"
                            DataValueField="ProjectId" Width="250px" ListWidth="300px" ClientScriptServerControlId="ajxPlanningBoard" ClientScriptMethod="updateRelease" />
                        <tstsc:HyperLinkEx ID="imgRefresh" runat="server"
                            CssClass="btn btn-launch br0 mr5"                             
                            ClientScriptServerControlId="ajxPlanningBoard" ClientScriptMethod="load_data()">
                            <span class="fas fa-sync"></span>
                        </tstsc:HyperLinkEx>



                        <div class="dif items-center pr4">
                            <tstsc:LabelEx runat="server" ID="ddlGroupByLabel" AssociatedControlID="ddlGroupBy"
                                CssClass="mr3 ma0"
                                Text="<%$Resources:Main,PlanningBoard_GroupBy %>" AppendColon="true" />
                            <tstsc:UnityDropDownListEx ID="ddlGroupBy" runat="server" NoValueItem="false"
                                CssClass="u-dropdown"
                                DataValueField="Id" DataTextField="Caption" ActiveItemField="IsActive"
                                AutoPostBack="false" Width="200px" ListWidth="200px"
                                ClientScriptServerControlId="ajxPlanningBoard" ClientScriptMethod="updateGroupBy" />
                        </div>
                    </div>


                   <div id="board-controls-options">
                        <div class="u-checkbox-toggle">
                            <tstsc:CheckBoxEx ID="chkIncludeDetails" runat="server"
                                    Text="<%$Resources:Main,PlanningBoard_IncludeDetails%>" ClientScriptMethod="chkIncludeDetails_click()" />
                            <tstsc:CheckBoxEx ID="chkIncludeTasks" runat="server"
                                    Text="<%$Resources:Main,SiteMap_Tasks%>" ClientScriptMethod="chkIncludeTasks_click()" />
                            <tstsc:CheckBoxEx ID="chkIncludeTestCases" runat="server"
                                    Text="<%$Resources:Main,SiteMap_TestCases%>"
                                    ClientScriptMethod="chkIncludeTestCases_click()" />
                        </div>

                        <div class="btn-group mr3 dn" role="group" >
                            <%-- hiding the print button as the print experience is not yet as good as it needs to be [IN:2430] --%>
                            <button 
                                runat="server" 
                                type="button" 
                                class="btn btn-default" 
                                onclick="window.print()"
                                title="<%$ Resources:Buttons,Print %>"
                                >
                                <i class="fad fa-print"></i>
                            </button>
                        </div>

                   </div>
                </div>





                <tstsc:PlanningBoard
                    BoardSupportsEditing="false"
                    Width="100%" runat="server" ID="ajxPlanningBoard" CssClass="PlanningBoard"
                    ErrorMessageControlId="lblMessage" Authorized_ArtifactType="Requirement" AllowCreate="false"
                    GroupByControlId="ddlGroupBy" ReleaseControlId="ddlSelectProject"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GroupPlanningBoardService" />
                <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>
                        <asp:ServiceReference Path="~/Services/Ajax/GroupPlanningBoardService.svc" />
                    </Services>
                </tstsc:ScriptManagerProxyEx>
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                <br />
                <br />
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
   <script type="text/javascript">
       $(document).ready(function () {
           $('#global-nav-keyboard-shortcuts #shortcuts-planning-board').removeClass('dn');
       });


        function chkIncludeDetails_click()
        {
            var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
            var chkIncludeDetails = $get('<%=chkIncludeDetails.ClientID %>');
            ajxPlanningBoard.updateOptions('IncludeDetails', chkIncludeDetails.checked)
        }
        function chkIncludeTasks_click()
        {
            var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
            var chkIncludeTasks = $get('<%=chkIncludeTasks.ClientID %>');
            ajxPlanningBoard.updateOptions('IncludeTasks', chkIncludeTasks.checked)
        }
        function chkIncludeTestCases_click()
        {
            var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
            var chkIncludeTestCases = $get('<%=chkIncludeTestCases.ClientID %>');
            ajxPlanningBoard.updateOptions('IncludeTestCases', chkIncludeTestCases.checked)
        }
        function ajxPlanningBoard_changeGroupBy(groupById)
        {
            //Do nothing
        }
    </script>
</asp:Content>
